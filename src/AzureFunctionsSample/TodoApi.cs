using AzureFunctionsSample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureFunctionsSample
{
    public static class TodoApi
    {
        private static List<Todo> items = new List<Todo>();

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
            items.Add(todo);

            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static Task<IActionResult> GetTodosAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "todo")] HttpRequest _,
            ILogger logger)
        {
            logger.LogInformation("Getting todo list items...");

            return Task.FromResult(new OkObjectResult(items) as IActionResult);
        }
    }
}
