using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure.Models.Yaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infrastructure.Extensions;

internal static class RepositoryModelExtensions
{
    internal static GitRepo ToGitRepo(this RepositoryModel repositoryModel)
    {
        return new();
    }
}