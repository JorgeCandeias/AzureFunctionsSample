using System;

namespace AzureFunctionsSample.Models
{
    public static class TodoExtensions
    {
        public static TodoTableEntity ToTableEntity(this Todo todo) =>
            new TodoTableEntity
            {
                PartitionKey = "TODO",
                RowKey = todo.Id.ToString(),
                Created = todo.Created,
                IsCompleted = todo.IsCompleted,
                Description = todo.Description
            };

        public static Todo ToTodo(this TodoTableEntity entity) =>
            new Todo
            {
                Id = Guid.Parse(entity.RowKey),
                Created = entity.Created,
                IsCompleted = entity.IsCompleted,
                Description = entity.Description
            };
    }
}
