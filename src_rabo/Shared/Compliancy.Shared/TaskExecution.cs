
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Compliancy.Shared
{
    public class TaskExecution
    {
        public async Task ExecuteAsync(TaskProperties taskProperties, CancellationToken cancellationToken)
        {
            TaskLogger? taskLogger = null;
            using var taskClient = new TaskClient(taskProperties);
            var taskResult = TaskResult.Succeeded;
            try
            {
                // create timeline record if not provided
                taskLogger = new TaskLogger(taskProperties, taskClient);
                await taskLogger.CreateTaskTimelineRecordIfRequired(taskClient, cancellationToken).ConfigureAwait(false);

                // report task started
                await taskLogger.LogImmediately("Task started");
                await taskClient.ReportTaskStarted(taskProperties.TaskInstanceId, cancellationToken).ConfigureAwait(false);
                await Task.Delay(1000);
                await taskClient.ReportTaskProgress(taskProperties.TaskInstanceId, cancellationToken).ConfigureAwait(false);

                await Task.Delay(1000);
                // report task completed with status
                await taskLogger.LogImmediately("Task completed");
                await taskClient.ReportTaskCompleted(taskProperties.TaskInstanceId, taskResult, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (taskLogger != null)
                {
                    await taskLogger.Log(e.ToString()).ConfigureAwait(false);
                }

                await taskClient.ReportTaskCompleted(taskProperties.TaskInstanceId, taskResult, cancellationToken).ConfigureAwait(false);
                throw;
            }
            finally
            {
                if (taskLogger != null)
                {
                    await taskLogger.End().ConfigureAwait(false);
                }
            }
        }
    }
}
