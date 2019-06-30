using AzureFunctionsSample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureFunctionsSample
{
    public static class TodoApi
    {
        private static readonly Dictionary<Guid, Todo> items = new Dictionary<Guid, Todo>();

        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodoAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "todo")] HttpRequest request,
            ILogger logger)
        {
            logger.LogInformation("Creating a new todo list item...");

            string body = await request.ReadAsStringAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(body);

            var todo = new Todo
            {
                Description = input.Description
            };
            items[todo.Id] = todo;

            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static IActionResult GetTodosAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "todo")] HttpRequest request,
            ILogger logger)
        {
            logger.LogInformation("Getting todo list items...");

            return new OkObjectResult(items.Values.ToList());
        }

        [FunctionName("GetTodoById")]
        public static IActionResult GetTodoByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "todo/{id}")] HttpRequest request,
            ILogger logger,
            Guid id)
        {
            logger.LogInformation("Getting todo by id {@id}...", id);

            if (items.TryGetValue(id, out var value))
            {
                return new OkObjectResult(value);
            }
            else
            {
                return new NotFoundResult();
            }
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodoAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "todo/{id}")] HttpRequest request,
            ILogger logger,
            Guid id)
        {
            logger.LogInformation("Updating todo by id {@id}...", id);

            if (!items.TryGetValue(id, out var todo))
                return new NotFoundResult();

            var body = await request.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(body);

            todo.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrWhiteSpace(updated.Description))
            {
                todo.Description = updated.Description;
            }

            return new OkObjectResult(todo);
        }

        [FunctionName("DeleteTodo")]
        public static IActionResult DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "todo/{id}")] HttpRequest request,
            ILogger logger,
            Guid id
        )
        {
            if (!items.Remove(id))
                return new NotFoundResult();

            return new OkResult();
        }
    }
}
