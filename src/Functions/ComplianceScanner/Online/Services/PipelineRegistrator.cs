#nullable enable

using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Domain.Extensions;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

[assembly: InternalsVisibleTo("Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests")]

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Services;

/// <inheritdoc />
public class PipelineRegistrator : IPipelineRegistrator
{
    private readonly ICmdbClient _cmdbClient;
    private readonly IPipelineRegistrationStorageRepository _storageRepository;
    private readonly IPipelineRegistrationRepository _pipelineRegistrationRepository;
    private readonly IManageHooksService _manageHooksService;

    /// <summary>
    /// Initializes a new instance of the class <see cref="PipelineRegistrator"/>
    /// </summary>
    public PipelineRegistrator(ICmdbClient cmdbClient, IPipelineRegistrationStorageRepository storageRepository,
        IPipelineRegistrationRepository pipelineRegistrationRepository, IManageHooksService manageHooksService)
    {
        _cmdbClient = cmdbClient;
        _storageRepository = storageRepository;
        _pipelineRegistrationRepository = pipelineRegistrationRepository;
        _manageHooksService = manageHooksService;
    }

    /// <inheritdoc />
    public async Task<IActionResult> RegisterNonProdPipelineAsync(
        string organization, string projectId, string pipelineId, string pipelineType, string stageId)
    {
        ValidateInput(organization, projectId, pipelineId, pipelineType);

        var registrations = await _pipelineRegistrationRepository.GetAsync(new GetPipelineRegistrationRequest
        {
            Organization = organization,
            PipelineId = pipelineId,
            PipelineType = pipelineType,
            ProjectId = projectId
        });

        // Check if a production registration already exists for this pipeline
        if (registrations != null && registrations.Exists(r => r.IsProduction))
        {
            return new BadRequestObjectResult(
                $"The registration failed because the specific pipeline is already registered as a " +
                $"production pipeline."
            );
        }

        // Update a NON-PROD registration in table-storage so we have no configuration items and therefore null is passed
        await AddTableStorageRegistrationAsync(null, organization, projectId, pipelineId, pipelineType, stageId, null);
        return new OkResult();
    }

    /// <inheritdoc />
    public async Task<IActionResult> RegisterProdPipelineAsync(string organization, string projectId, string pipelineId,
        string pipelineType, string? userMailAddress, RegistrationRequest input)
    {
        ValidateInput(organization, projectId, pipelineId, pipelineType, userMailAddress);

        var configurationItem = await _cmdbClient.GetCiAsync(input.CiIdentifier);

        if (configurationItem == null)
        {
            return new BadRequestObjectResult(ErrorMessages.CiDoesNotExist(input.CiIdentifier));
        }

        var result = await ValidateCiAsync(organization, projectId, pipelineId, input.Environment, userMailAddress, configurationItem);

        if (result.GetType() != typeof(OkResult))
        {
            return result;
        }

        await InsertCmdbRegistrationAsync(organization, projectId, pipelineId, input.Environment, input.Profile, configurationItem.CiName);
        await AlignProfileExistingCmdbRegistrationsAsync(organization, projectId, pipelineId, pipelineType, input.Profile);
        await DeleteNonProdTableStorageRegistrationAsync(projectId, pipelineId, pipelineType, null);
        await UpdateProfileTableStorageRegistrationAsync(organization, projectId, pipelineId, pipelineType, input.Profile);
        await AddTableStorageRegistrationAsync(configurationItem, organization, projectId, pipelineId, pipelineType, input.Environment, input.Profile);
        await _manageHooksService.CreateHookAsync(organization, projectId, pipelineType, pipelineId);

        return new OkResult();
    }

    public async Task<IActionResult> UpdateProdPipelineRegistrationAsync(string organization, string projectId, string pipelineId,
        string pipelineType, string? userMailAddress, UpdateRequest input)
    {
        ValidateInput(organization, projectId, pipelineId, pipelineType, userMailAddress);

        var configurationItem = await _cmdbClient.GetCiAsync(input.CiIdentifier);

        if (configurationItem == null)
        {
            return new BadRequestObjectResult(ErrorMessages.CiDoesNotExist(input.CiIdentifier));
        }

        if (!await UserIsAuthorizedAsync(userMailAddress, configurationItem))
        {
            return new UnauthorizedObjectResult(ErrorMessages.RegistrationUpdateUnAuthorized);
        }

        var fieldToUpdate = EnumHelper.ParseEnumOrNull<FieldToUpdate>(input.FieldToUpdate);
        if (fieldToUpdate == FieldToUpdate.CiIdentifier)
        {
            //delete existing registration for 'old' CI
            await DeleteCmdbRegistrationAsync(configurationItem, organization, projectId, pipelineId, input.Environment);

            //validate 'new' CI for registration and insert new registration
            var newConfigurationItem = await _cmdbClient.GetCiAsync(input.NewValue);

            if (newConfigurationItem == null)
            {
                return new BadRequestObjectResult(ErrorMessages.CiDoesNotExist(input.NewValue));
            }

            var result = await ValidateCiAsync(organization, projectId, pipelineId, input.Environment, userMailAddress, newConfigurationItem);

            if (result.GetType() != typeof(OkResult))
            {
                return result;
            }
            await InsertCmdbRegistrationAsync(organization, projectId, pipelineId, input.Environment, input.Profile, newConfigurationItem.CiName);
            await UpdateTableStorageRegistrationAsync(input.CiIdentifier, organization, projectId, pipelineId, pipelineType, input.Environment, input.NewValue);
        }

        if (fieldToUpdate == FieldToUpdate.Environment)
        {
            await UpdateStageIdExistingCmdbRegistrationsAsync(configurationItem, organization, projectId, pipelineId, input.Environment, input.NewValue);
            await UpdateTableStorageRegistrationAsync(input.CiIdentifier, organization, projectId, pipelineId, pipelineType, input.Environment, null, input.NewValue);
        }

        if (fieldToUpdate == FieldToUpdate.Profile)
        {
            await AlignProfileExistingCmdbRegistrationsAsync(organization, projectId, pipelineId, pipelineType, input.NewValue);
            await UpdateProfileTableStorageRegistrationAsync(organization, projectId, pipelineId, pipelineType, input.NewValue);
        }

        return new OkObjectResult(Constants.SuccessfullMessageResult);
    }

    public async Task<IActionResult> DeleteProdPipelineRegistrationAsync(string organization, string projectId, string pipelineId,
        string pipelineType, string? userMailAddress, DeleteRegistrationRequest input)
    {
        ValidateInput(organization, projectId, pipelineId, pipelineType, userMailAddress);

        var configurationItem = await _cmdbClient.GetCiAsync(input.CiIdentifier);

        if (configurationItem == null)
        {
            return new BadRequestObjectResult(ErrorMessages.CiDoesNotExist(input.CiIdentifier));
        }

        if (!await UserIsAuthorizedAsync(userMailAddress, configurationItem))
        {
            return new UnauthorizedObjectResult(ErrorMessages.RegistrationDeleteUnAuthorized);
        }

        await DeleteCmdbRegistrationAsync(configurationItem, organization, projectId, pipelineId, input.Environment);
        await _storageRepository.DeleteEntityAsync(input.CiIdentifier, projectId, pipelineId, pipelineType, input.Environment);

        return new OkObjectResult(Constants.SuccessfullMessageResult);
    }

    /// Is used by include in compliancy hub for non-prod pipelines
    public async Task<IActionResult> UpdateNonProdRegistrationAsync(string organization, string projectId, string pipelineId, string pipelineType, string? stageId)
    {
        ValidateInput(organization, projectId, pipelineId, pipelineType);

        await AddTableStorageRegistrationAsync(null, organization, projectId, pipelineId, pipelineType, stageId, null, true);

        // If stageId is not empty or null, there should also be a base-registration without a stage. This needs to be updated to ToBeScanned = true
        if (!string.IsNullOrEmpty(stageId))
        {
            await AddTableStorageRegistrationAsync(null, organization, projectId, pipelineId, pipelineType, null, null, true);
        }

        return new OkResult();
    }

    private async Task<IActionResult> ValidateCiAsync(string organization, string projectId, string pipelineId, string environment,
        string? userMailAddress, ConfigurationItem configurationItem)
    {
        if (!configurationItem.IsTypeValid)
        {
            return new BadRequestObjectResult(
                $"The registration failed with a bad request error, " +
                $"because the Configuration Item {configurationItem.CiID} is invalid. " +
                $"Please make sure the CI Type is 'application' or 'subapplication'."
            );
        }

        if (!configurationItem.IsEnvironmentValid)
        {
            return new BadRequestObjectResult(
                $"The registration failed with a bad request error, " +
                $"because the Configuration Item {configurationItem.CiID} is invalid. " +
                $"Please make sure the CI Environment contains 'Production'"
            );
        }

        if (!configurationItem.IsCiStatusValid)
        {
            return new BadRequestObjectResult(
                $"The registration failed with a bad request error, " +
                $"because the Configuration Item {configurationItem.CiID} is invalid. " +
                $"Please make sure the CI Status is 'In Use - ...'"
            );
        }

        if (!await UserIsAuthorizedAsync(userMailAddress, configurationItem))
        {
            return new UnauthorizedObjectResult(ErrorMessages.RegistrationUpdateUnAuthorized);
        }

        var deploymentInformation = await _cmdbClient.GetDeploymentMethodAsync(configurationItem.CiName);

        if (RegistrationExists(deploymentInformation ?? Enumerable.Empty<DeploymentInformation>(),
                organization, projectId, pipelineId, environment))
        {
            return new BadRequestObjectResult(ErrorMessages.ExistingRegistrationError

            );
        }
        return new OkResult();
    }

    private static void ValidateInput(string organization, string projectId, string pipelineId, string pipelineType, string? userMailAddress)
    {
        ValidateInput(organization, projectId, pipelineId, pipelineType);
        if (string.IsNullOrEmpty(userMailAddress))
        {
            throw new ArgumentNullException(nameof(userMailAddress));
        }
    }

    private static void ValidateInput(string organization, string projectId, string pipelineId, string pipelineType)
    {
        if (string.IsNullOrEmpty(organization))
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (string.IsNullOrEmpty(projectId))
        {
            throw new ArgumentNullException(nameof(projectId));
        }

        if (string.IsNullOrEmpty(pipelineId))
        {
            throw new ArgumentNullException(nameof(pipelineId));
        }

        if (string.IsNullOrEmpty(pipelineType))
        {
            throw new ArgumentNullException(nameof(pipelineType));
        }
    }

    internal async Task<bool> UserIsAuthorizedAsync(string? userMailAddress, ConfigurationItem ci)
    {
        var assignmentGroup = await GetAssignmentGroupAsync(ci.ConfigAdminGroup);

        return !string.IsNullOrEmpty(userMailAddress) &&
               assignmentGroup?.GroupMembers != null &&
               assignmentGroup.GroupMembers.Contains(userMailAddress, StringComparer.InvariantCultureIgnoreCase);
    }

    private async Task<AssignmentGroup?> GetAssignmentGroupAsync(string? assignmentGroup) =>
        string.IsNullOrEmpty(assignmentGroup)
            ? null
            : await _cmdbClient.GetAssignmentGroupAsync(assignmentGroup);

    internal static bool RegistrationExists(IEnumerable<DeploymentInformation> deploymentMethods,
        string? organization, string? projectId, string? pipelineId, string? stageId) =>
        deploymentMethods
            .Where(deploymentInformation => deploymentInformation.Method == DeploymentInformation.AzureDevOpsMethod)
            .Select(deploymentInformation => SupplementaryInformation.ParseSupplementaryInfo(deploymentInformation.Information))
            .Any(supplementaryInformation => supplementaryInformation != null &&
                      supplementaryInformation.Organization == organization &&
                      supplementaryInformation.Project == projectId &&
                      supplementaryInformation.Pipeline == pipelineId &&
                      supplementaryInformation.Stage.Equals(stageId?.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase));

    private async Task InsertCmdbRegistrationAsync(string organization, string projectId,
        string pipelineId, string stageId, string profile, string? ciName)
    {
        var newRegistration = new DeploymentMethod(ciName, organization, projectId, pipelineId, stageId, profile);
        await _cmdbClient.InsertDeploymentMethodAsync(newRegistration);
    }

    private async Task DeleteCmdbRegistrationAsync(ConfigurationItem configurationItem, string organization, string projectId,
        string pipelineId, string stageId)
    {
        var deploymentInformations = await _cmdbClient.GetDeploymentMethodAsync(configurationItem.CiName);

        var existingRegistration = deploymentInformations?
            .Where(x => x.Method == DeploymentInformation.AzureDevOpsMethod)
            .Select(x => SupplementaryInformation.ParseSupplementaryInfo(x.Information))
            .FirstOrDefault(x => x != null &&
                                 x.Organization == organization &&
                                 x.Project == projectId &&
                                 x.Pipeline == pipelineId &&
                                 x.Stage == stageId);

        if (existingRegistration == default)
        {
            throw new InvalidOperationException("Registration not found");
        }

        await _cmdbClient.DeleteDeploymentMethodAsync(configurationItem, existingRegistration);
    }

    private async Task AlignProfileExistingCmdbRegistrationsAsync(string organization, string projectId, string pipelineId, string pipelineType, string profile)
    {
        var registrations = await _pipelineRegistrationRepository.GetAsync(new GetPipelineRegistrationRequest
        {
            Organization = organization,
            PipelineId = pipelineId,
            PipelineType = pipelineType,
            ProjectId = projectId
        });

        if (!registrations.Any())
        {
            return;
        }

        var registrationsPerCi = registrations
            .Where(pipelineRegistration => pipelineRegistration.IsProduction)
            .GroupBy(r => r.CiName)
            .Select(r => r.First());

        foreach (var ciIdentifier in registrationsPerCi.Select(pipelineRegistration => pipelineRegistration.CiIdentifier))
        {
            var configurationItem = await _cmdbClient.GetCiAsync(ciIdentifier);
            if (configurationItem == null)
            {
                continue;
            }

            var deploymentMethods = await _cmdbClient.GetDeploymentMethodAsync(configurationItem.CiName);

            var deploymentMethodList = deploymentMethods?
                .Where(deploymentInformation => deploymentInformation.Method == DeploymentInformation.AzureDevOpsMethod)
                .Select(deploymentInformation => SupplementaryInformation.ParseSupplementaryInfo(deploymentInformation.Information))
                .Where(supplementaryInformation => supplementaryInformation != null &&
                            supplementaryInformation.Organization == organization &&
                            supplementaryInformation.Project == projectId &&
                            supplementaryInformation.Pipeline == pipelineId &&
                            supplementaryInformation.Profile != profile)
                .Select(supplementaryInformation => supplementaryInformation);

            if (deploymentMethodList == null)
            {
                continue;
            }

            foreach (var deploymentMethod in deploymentMethodList)
            {
                if (deploymentMethod == null)
                {
                    continue;
                }

                var newMethod = new DeploymentMethod(configurationItem.CiName, deploymentMethod.Organization,
                    deploymentMethod.Project, deploymentMethod.Pipeline, deploymentMethod.Stage, profile);
                await _cmdbClient.UpdateDeploymentMethodAsync(configurationItem, deploymentMethod, newMethod);
            }
        }
    }

    private async Task UpdateStageIdExistingCmdbRegistrationsAsync(ConfigurationItem configurationItem, string organization, string projectId, string pipelineId, string stageId, string newStageId)
    {
        var deploymentMethods = await _cmdbClient.GetDeploymentMethodAsync(configurationItem.CiName);

        var currentMethod = deploymentMethods?
            .Where(x => x.Method == DeploymentInformation.AzureDevOpsMethod)
            .Select(x => SupplementaryInformation.ParseSupplementaryInfo(x.Information))
            .FirstOrDefault(x => x != null &&
                                 x.Organization == organization &&
                                 x.Project == projectId &&
                                 x.Pipeline == pipelineId &&
                                 x.Stage == stageId);

        if (currentMethod == null)
        {
            throw new InvalidOperationException("Registration not found");
        }

        var newMethod = new DeploymentMethod(configurationItem, currentMethod)
        {
            Stage = newStageId
        };

        await _cmdbClient.UpdateDeploymentMethodAsync(configurationItem, currentMethod, newMethod);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Legacy. Will be refactored completely")]
    private async Task AddTableStorageRegistrationAsync(ConfigurationItem? ci, string organization, string projectId, string pipelineId, string pipelineType,
        string? stageId, string? profileName, bool? toBeScanned = null)
    {
        var registrationEntity = new PipelineRegistration(ci, organization, projectId, pipelineId, pipelineType, stageId, profileName, toBeScanned);
        await _storageRepository.InsertOrMergeEntityAsync(registrationEntity);
    }

    private async Task DeleteNonProdTableStorageRegistrationAsync(string projectId, string pipelineId, string pipelineType, string? stageId) =>
        await _storageRepository.DeleteEntitiesForPipelineAsync(null, projectId, pipelineId, pipelineType, stageId);

    private async Task UpdateProfileTableStorageRegistrationAsync(string organization, string projectId, string pipelineId,
        string pipelineType, string profileName)
    {
        //search for all registrations for this pipeline and update profile for all of them
        var registrations = await _pipelineRegistrationRepository.GetAsync(new GetPipelineRegistrationRequest
        {
            Organization = organization,
            PipelineId = pipelineId,
            PipelineType = pipelineType,
            ProjectId = projectId
        });

        foreach (var registration in registrations)
        {
            var profile = EnumHelper.ParseEnumOrDefault<Profiles>(profileName);
            registration.RuleProfileName = profile.ToString();
            await _storageRepository.InsertOrMergeEntityAsync(registration);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Legacy. Will be refactored completely")]
    private async Task UpdateTableStorageRegistrationAsync(string ciIdentifier, string organization, string projectId,
        string pipelineId, string pipelineType, string? stageId, string? newCiIdentifier = null, string? newStageId = null)
    {
        var registration = (await _pipelineRegistrationRepository.GetAsync(organization, projectId, pipelineId, stageId))
            .Find(x => x.CiIdentifier == ciIdentifier) ?? throw new InvalidOperationException("Registration not found");
        await _storageRepository.DeleteEntityAsync(registration);

        if (newStageId != null)
        {
            registration.StageId = newStageId;
            registration.RowKey = CreateRowKey(ciIdentifier, projectId, pipelineId, pipelineType, newStageId);
        }
        if (newCiIdentifier != null)
        {
            registration.CiIdentifier = newCiIdentifier;
            registration.RowKey = CreateRowKey(newCiIdentifier, projectId, pipelineId, pipelineType, stageId);
        }

        await _storageRepository.InsertOrMergeEntityAsync(registration);
    }

    internal static string CreateRowKey(
        string ciIdentifier, string projectId, string pipelineId, string pipelineType, string? stageId) =>
        SanitizeKey($"{ciIdentifier}|{projectId}|{pipelineId}|{pipelineType}|{stageId}");

    internal static string SanitizeKey(string key)
    {
        var regEx = new Regex(@"[\\\\#%+ /?\u0000-\u001F\u007F-\u009F]");
        return regEx.IsMatch(key) ? regEx.Replace(key, string.Empty) : key;
    }
}