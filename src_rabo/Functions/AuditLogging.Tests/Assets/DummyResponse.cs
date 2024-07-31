namespace Rabobank.Compliancy.Functions.AuditLogging.Tests.Assets;

public static class DummyResponse
{
    public static string Response(string message)
    {
        return $@"
========================== Starting Command Output ===========================
/usr/bin/bash /home/vsts/work/_temp/e47fc1f2-b197-4ee3-9313-a5538b620b52.sh
Executing curl -s https://pipelinebreakerv2dev.azurewebsites.net/api/pipeline-compliant/raboweb-test/53410703-e2e5-4238-9025-233bd7c811b3/395666/Resources/build
{ message }
Finishing: Pre-job: check pipeline registration and compliancy Linux or Darwin OS agent
";
    }
}