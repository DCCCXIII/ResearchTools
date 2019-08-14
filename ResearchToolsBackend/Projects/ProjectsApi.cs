using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;
using ResearchToolsBackend.Models;
using Microsoft.WindowsAzure.Storage;

namespace ResearchToolsBackend.Projects
{
    public static class ProjectsApi
    {
        [FunctionName("Projects_GetAll")]
        public static async Task<IActionResult> GetProjects(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "projects")] HttpRequest req,
            [Table("projects", Connection = "AzureWebJobsStorage")] CloudTable projectTable, ILogger log)
        {
            log.LogInformation("Getting all projects");

            var query = new TableQuery<ProjectTableEntity>();
            var segment = await projectTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.ToProject));
        }

        [FunctionName("Projects_GetById")]
        public static IActionResult GetProjectById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "projects/{id}")]HttpRequest req,
            [Table("projects", "PROJECT", "{id}", Connection = "AzureWebJobsStorage")] ProjectTableEntity todo,
            ILogger log, string id)
        {
            log.LogInformation("Getting project item by id");
            if (todo == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }
            return new OkObjectResult(todo.ToProject());
        }

        [FunctionName("Projects_Create")]
        public static async Task<IActionResult> CreateProject(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "projects")]HttpRequest req,
            [Table("projects", Connection = "AzureWebJobsStorage")] IAsyncCollector<ProjectTableEntity> projectTable,
            ILogger log)
        {
            log.LogInformation("Creating a new project.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ProjectCreateModel>(requestBody);

            var project = new Project() { Name = input.Name };
            await projectTable.AddAsync(project.ToTableEntity());
            return new OkObjectResult(project);
        }

        [FunctionName("Projects_Update")]
        public static async Task<IActionResult> UpdateProject(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "projects/{id}")] HttpRequest req,
            [Table("projects", Connection = "AzureWebJobsStorage")] CloudTable projectTable,
            ILogger log, string id)
        {
            log.LogInformation("Updating a project");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<ProjectUpdateModel>(requestBody);
            var findOperation = TableOperation.Retrieve<ProjectTableEntity>("PROJECT", id);
            var findResult = await projectTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }
            var existingRow = (ProjectTableEntity)findResult.Result;

            if (!string.IsNullOrEmpty(updated.Name))
            {
                existingRow.Name = updated.Name;
            }

            var replaceOperation = TableOperation.Replace(existingRow);
            await projectTable.ExecuteAsync(replaceOperation);

            return new OkObjectResult(existingRow.ToProject());
        }

        [FunctionName("Projects_Delete")]
        public static async Task<IActionResult> DeleteProject(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "projects/{id}")] HttpRequest req,
            [Table("projects", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log, string id)
        {
            log.LogInformation("Deleting a project");
            var deleteOperation = TableOperation.Delete(
                new TableEntity() { PartitionKey = "PROJECT", RowKey = id, ETag = "*" });
            try
            {
                var deleteResult = await todoTable.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }

            return new OkResult();
        }
    }
}
