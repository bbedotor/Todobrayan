using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Todobrayan.Common.Models;
using Todobrayan.Common.Responses;
using Todobrayan.Functions.Entities;

namespace Todobrayan.Functions.Functions
{
    public static class TodoApi
    {
        [FunctionName(nameof(CreateTodo))]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new todo");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            if (string.IsNullOrEmpty(todo?.TaskDescription))
            {
                return new BadRequestObjectResult(new response
                {
                    IsSuccess = false,
                    Message = "The request must have a TaskDescription."
                });
            }


            Todoentity todoentity = new Todoentity
            {
                CreatedTime = DateTime.UtcNow,
                ETag = "*",
                IsCompleted = false,
                PartitionKey = "TODO",
                RowKey = Guid.NewGuid().ToString(),
                TaskDescription = todo.TaskDescription
            };

            TableOperation addOperation = TableOperation.Insert(todoentity);

            await todoTable.ExecuteAsync(addOperation);

            string message = "New todo stored in table";
            log.LogInformation(message);


            return new OkObjectResult(new response
            {
                IsSuccess = true,
                Message = message,
                Result = todoentity

            });
        }

        [FunctionName(nameof(UpdateTodo))]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            string id ,
            ILogger log)
        {
            log.LogInformation($"Update for todo {id}, received");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            // validate todo id
            TableOperation findOperation = TableOperation.Retrieve<Todoentity>("TODO", id);
            TableResult findResult = await todoTable.ExecuteAsync(findOperation);

            if (findResult.Result == null) 
            {
                return new BadRequestObjectResult(new response
                {
                    IsSuccess = false,
                    Message = "Todo not found"
                });
            }

            // Update todo

            Todoentity todoentity = (Todoentity)findResult.Result;
            todoentity.IsCompleted = todo.IsCompleted;

            if (!string.IsNullOrEmpty(todo.TaskDescription))
            {
                todoentity.TaskDescription = todo.TaskDescription;
            }

            TableOperation addOperation = TableOperation.Replace(todoentity);

            await todoTable.ExecuteAsync(addOperation);

            string message = $"todo: {id}, updated in table. ";
            log.LogInformation(message);


            return new OkObjectResult(new response
            {
                IsSuccess = true,
                Message = message,
                Result = todoentity

            });
        }

        [FunctionName(nameof(GetAllTodos))]
        public static async Task<IActionResult> GetAllTodos(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
           [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
           ILogger log)
        {
            log.LogInformation("Get all todos received");

            TableQuery<Todoentity> query = new TableQuery<Todoentity>();
            TableQuerySegment<Todoentity> todos = await todoTable.ExecuteQuerySegmentedAsync(query, null);          

            string message = "retieved all todos";
            log.LogInformation(message);


            return new OkObjectResult(new response
            {
                IsSuccess = true,
                Message = message,
                Result = todos

            });
        }

        [FunctionName(nameof(GettodoByid))]
        public static IActionResult GettodoByid(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
           [Table("todo","TODO", "{id}", Connection = "AzureWebJobsStorage")] Todoentity todoentity,
           string id,
           ILogger log)
        {
            log.LogInformation($"Get todo by id: {id}  received");

            if (todoentity == null)
            {
                return new BadRequestObjectResult(new response { 
                    IsSuccess = false,
                    Message = "Todo not found"
                });
            }         

            string message = $" Todo:{id}, retrieved";
            log.LogInformation(message);


            return new OkObjectResult(new response
            {
                IsSuccess = true,
                Message = message,
                Result = todoentity

            });
        }

        [FunctionName(nameof(DeleteTodo))]
        public static async Task <IActionResult> DeleteTodo(
           [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
           [Table("todo", "TODO", "{id}", Connection = "AzureWebJobsStorage")] Todoentity todoentity,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
           string id,
           ILogger log)
        {
            log.LogInformation($"Delete todo: {id}  received");

            if (todoentity == null)
            {
                return new BadRequestObjectResult(new response
                {
                    IsSuccess = false,
                    Message = "Todo not found"
                });
            }

            await todoTable.ExecuteAsync(TableOperation.Delete(todoentity));
            string message = $" Todo:{id}, deleted";
            log.LogInformation(message);


            return new OkObjectResult(new response
            {
                IsSuccess = true,
                Message = message,
                Result = todoentity

            });
        }
    }
}
