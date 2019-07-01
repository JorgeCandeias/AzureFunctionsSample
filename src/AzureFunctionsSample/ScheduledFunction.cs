using AzureFunctionsSample.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace AzureFunctionsSample
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Run(
            [TimerTrigger("0 */5 * * * *")] TimerInfo timer,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable table,
            ILogger log)
        {
            var query = new TableQuery<TodoTableEntity>();
            var segment = await table.ExecuteQuerySegmentedAsync(query, null);
            var deleted = 0;
            foreach (var todo in segment)
            {
                if (todo.IsCompleted)
                {
                    await table.ExecuteAsync(TableOperation.Delete(todo));
                    ++deleted;
                }
            }
            log.LogInformation("Deleted {@Count} items at {@Timestamp}",
                deleted, DateTime.UtcNow);
        }
    }
}
