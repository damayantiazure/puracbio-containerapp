# Open Permissions on Azure DevOps Protected Resources

## -> Background: The Protected Resource has been Reconciled. 
An Open Permission action can be performed on non-production resources. These resources may not have had been used for production deployments in the past 450 days, and may also not be linked to another resource that has been deployed to production in that time period.

## -> Scenario 1: Open Permissions on Azure DevOps Repositories
**Given** a user wants to **ALLOW**  _<Actions>_ for _<Roles>_ on a specific Azure DevOps repository  
**And** the repository was **RECONCILED** resulting in **DENY** permission on _<Actions>_ for _<Roles>_  
**When** the user sends a request to open permissions for the repository  
**Then** the system should verify that the Pipeline does not have to be not be retained   
**And** enable **ALLOW** permission on _<Actions>_ for _<Roles>_  

**Examples:**  
| Actions                                       | Roles                                                      |
|-----------------------------------------------|------------------------------------------------------------|
| DeleteRepository, ManagePermissions           | ProjectAdministrators, BuildAdministrators, Contributors   |

## -> Scenario 2: Enabling Permissions on Azure DevOps Pipelines
**Given** a user wants to **ALLOW** _<Actions>_ for _<Roles>_ on a specific Azure DevOps pipeline of type _<PipelineType>_ and build type _<BuildType>_ (if applicable)  
**And** the pipeline was **RECONCILED** resulting in **DENY** permission on _<Actions>_ for _<Roles>_  
**When** the user sends a request to enable permissions for the pipeline  
**Then** the system should verify that the Pipeline does not have to be not be retained  
**And** the system should enable **ALLOW** permission on _<Actions>_ for _<Roles>_ on the pipeline  

**Examples:**  
| PipelineType | BuildType | Actions                                                           | Roles                                                      |
|--------------|-----------|-------------------------------------------------------------------|------------------------------------------------------------|
| Build        | YAML      | DeleteBuilds, DestroyBuilds, DeleteBuildDefinition, AdministerBuildPermissions | ProjectAdministrators, BuildAdministrators, Contributors   |
| Build        | Classic   | DeleteBuilds, DestroyBuilds, DeleteBuildDefinition, AdministerBuildPermissions | ProjectAdministrators, BuildAdministrators, Contributors   |
| Release      | N/A       | DeleteReleasePipelines, AdministerReleasePermissions, DeleteReleases | ProjectAdministrators, ReleaseAdministrators, Contributors |

## -> Scenario 3: Handling Invalid Requests
**Given** a user sends an invalid request (e.g., non-existent repository or pipeline)  
**When** the system processes the request  
**Then** the system should identify the invalid request  
**And** the system should return an error message to the user indicating the nature of the error

## -> Scenario 4: Handling Unauthorized Requests
**Given** a user who does not have the necessary rights tries to enable permissions on a repository or pipeline  
**When** the system processes the request  
**Then** the system should identify that the user does not have the necessary rights  
**And** the system should deny the request and inform the user that they are unauthorized

## -> Scenario 5: Handling Production Deployments Marked for Retention
**Given** a user wants to Open Permissions on a Protected Azure DevOps Resource  
**And** the resource was deployed to production within the last 450 days  
**When** the system processes the request  
**Then** the system should identify that the _<ResourceType>_ should be retained  
**And** the system should deny the request and inform the user with an IsProductionItemException

**Examples:**  
| ResourceType               |
|---------------------------|
| YAML Build Pipeline        |
| Classic Build Pipeline    |
| Release Pipeline          |
| GitRepo                   |
