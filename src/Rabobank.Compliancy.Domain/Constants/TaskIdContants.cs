namespace Rabobank.Compliancy.Domain.Constants;

public static class TaskContants
{
    public static class MainframeCobolConstants
    {
        public static readonly string DbbBuildTaskId = "f0ed76ac-b927-42fa-a758-a36c1838a13b";
        public static readonly string DbbBuildTaskName = "dbb-build";
        public static readonly string DbbPackageTaskId = "dc5c403b-4cd3-48f2-9dcc-4405e1b6f981";
        public static readonly string DbbPackageTaskName = "dbb-package";
        public static readonly string DbbDeployTaskId = "206089fc-dcf1-4d0a-bc10-135adf95db3c";
        public static readonly string DbbDeployTaskName = "dbb-deploy-prod";

        public static readonly string OrganizationName = "organizationName";
        public static readonly string ProjectId = "projectId";
        public static readonly string PipelineId = "pipelineId";
    }
}