using AzureFunctionsSample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AzureFunctionsSample
{
    public static class TodoApi
    {
        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodoAsync(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "todo")] HttpRequest request,
            [Table("todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<TodoTableEntity> table,
            [Queue("todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<Todo> queue,
            ILogger logger)
        {
            logger.LogInformation("Creating a new todo list item...");

            string body = await request.ReadAsStringAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(body);

            var todo = new Todo
            {
                Description = input.Description
            };

            await table.AddAsync(todo.ToTableEntity());
            await queue.AddAsync(todo);

            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodosAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "todo")] HttpRequest request,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable table,
            ILogger logger)
        {
            logger.LogInformation("Getting todo list items...");

            var query = new TableQuery<TodoTableEntity>();
            var segment = await table.ExecuteQuerySegmentedAsync(query, null);
            var result = segment.Select(x => x.ToTodo());

            return new OkObjectResult(result);
        }

        [FunctionName("GetTodoById")]
        public static IActionResult GetTodoByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "todo/{id}")] HttpRequest request,
            [Table("todos", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoTableEntity entity,
            ILogger logger,
            Guid id)
        {
            logger.LogInformation("Getting todo by id {@id}...", id);

            if (entity == null)
            {
                logger.LogInformation("Item {@Id} not found", id);
                return new NotFoundResult();
            }

            return new OkObjectResult(entity.ToTodo());
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodoAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "todo/{id}")] HttpRequest request,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable table,
            ILogger logger,
            Guid id)
        {
            logger.LogInformation("Updating todo by id {@id}...", id);

            var body = await request.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(body);

            var findOperation = TableOperation.Retrieve<TodoTableEntity>("TODO", id.ToString());
            var findResult = await table.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }

            var existing = (TodoTableEntity)findResult.Result;
            existing.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrWhiteSpace(updated.Description))
            {
                existing.Description = updated.Description;
            }

            var replaceOperation = TableOperation.Replace(existing);
            await table.ExecuteAsync(replaceOperation);

            return new OkObjectResult(existing.ToTodo());
        }

        [FunctionName("DeleteTodo")]
        public static async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "todo/{id}")] HttpRequest request,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable table,
            ILogger logger,
            Guid id
        )
        {
            try
            {
                await table.ExecuteAsync(TableOperation.Delete(new TableEntity
                {
                    PartitionKey = "TODO",
                    RowKey = id.ToString(),
                    ETag = "*"
                }));
            }
            catch (StorageException error) when (error.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }

            return new OkResult();
        }
    }
}
