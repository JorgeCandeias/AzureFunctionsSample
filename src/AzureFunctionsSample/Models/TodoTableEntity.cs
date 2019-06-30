using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzureFunctionsSample.Models
{
    public class TodoTableEntity : TableEntity
    {
        public DateTime Created { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
    }
}
