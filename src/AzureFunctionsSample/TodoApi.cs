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
        public static async Task<ActionResult<Todo>> CreateTodoAsync(
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

            return todo;
        }

        [FunctionName("GetTodos")]
        public static ActionResult<IEnumerable<Todo>> GetTodosAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "todo")] HttpRequest _,
            ILogger logger)
        {
            logger.LogInformation("Getting todo list items...");

            return items.Values.ToList();
        }

        [FunctionName("GetTodoById")]
        public static ActionResult<Todo> GetTodoByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "todo/{id}")] HttpRequest request,
            ILogger logger,
            Guid id)
        {
            logger.LogInformation("Getting todo by id {@id}...", id);

            if (items.TryGetValue(id, out var value))
            {
                return value;
            }
            else
            {
                return new NotFoundResult();
            }
        }

        [FunctionName("UpdateTodo")]
        public static async Task<ActionResult> UpdateTodoAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "todo/{id}")] HttpRequest request,
            ILogger logger,
            Guid id)
        {
            logger.LogInformation("Updating todo by id {@id}...", id);

            if (items.TryGetValue(id, out var value))
            {
                var body = await request.ReadAsStringAsync();
            }
            else
            {
                return new NotFoundResult();
            }
        }
    }
}
