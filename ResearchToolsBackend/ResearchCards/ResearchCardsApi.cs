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

namespace ResearchToolsBackend.ResearchCards
{
    public static class ResearchCardsApi
    {
        [FunctionName("Cards_GetAll")]
        public static async Task<IActionResult> GetCards(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{projectId}/cards")] HttpRequest req,
            [Table("cards", "{projectId}", Connection = "AzureWebJobsStorage")] CloudTable researchTable, 
            ILogger log, string projectId)
        {
            log.LogInformation("Getting all research cards for a project.");

            var query = new TableQuery<ResearchCardTableEntity>();
            var segment = await researchTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.ToResearchCard));
        }

        [FunctionName("Cards_GetById")]
        public static IActionResult GetCardtById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{projectId}/cards/{id}")]HttpRequest req,
            [Table("cards", "{projectId}", "{id}", Connection = "AzureWebJobsStorage")] ResearchCardTableEntity researchCardTable,
            ILogger log, string id, string projectId)
        {
            log.LogInformation("Getting research card by id");
            if (researchCardTable == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }
            return new OkObjectResult(researchCardTable.ToResearchCard());
        }

        [FunctionName("Cards_Create")]
        public static async Task<IActionResult> CreateCard(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "{projectId}/cards")]HttpRequest req,
            [Table("cards", Connection = "AzureWebJobsStorage")] IAsyncCollector<ResearchCardTableEntity> researchCardTable,
            ILogger log, string projectId)
        {
            log.LogInformation("Creating a new card.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<ResearchCardCreateModel>(requestBody);

            var researchCard = new ResearchCard()
            {
                ProjectId = projectId,
                Source = input.Source,
                Quote = input.Quote,
                Summary = input.Summary,
                Commentary = input.Commentary,
                Keywords = input.Keywords
            };
            await researchCardTable.AddAsync(researchCard.ToTableEntity());
            return new OkObjectResult(researchCard);
        }

        [FunctionName("Cards_Update")]
        public static async Task<IActionResult> UpdateProject(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "{projectId}/cards/{id}")] HttpRequest req,
            [Table("cards", Connection = "AzureWebJobsStorage")] CloudTable researchCardsTable,
            ILogger log, string projectId, string id)
        {
            log.LogInformation("Updating a card");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<ResearchCardUpdateModel>(requestBody);
            var findOperation = TableOperation.Retrieve<ResearchCardTableEntity>(projectId, id);
            var findResult = await researchCardsTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }
            var existingRow = (ResearchCardTableEntity)findResult.Result;

            if (!string.IsNullOrEmpty(updated.Source))
            {
                existingRow.Source = updated.Source;
            }
            if (!string.IsNullOrEmpty(updated.Quote))
            {
                existingRow.Quote = updated.Quote;
            }
            if (!string.IsNullOrEmpty(updated.Summary))
            {
                existingRow.Summary = updated.Summary;
            }
            if (!string.IsNullOrEmpty(updated.Commentary))
            {
                existingRow.Commentary = updated.Commentary;
            }
            existingRow.Keywords = updated.Keywords;

            var replaceOperation = TableOperation.Replace(existingRow);
            await researchCardsTable.ExecuteAsync(replaceOperation);

            return new OkObjectResult(existingRow.ToResearchCard());
        }

        [FunctionName("Cards_Delete")]
        public static async Task<IActionResult> DeleteProject(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "{projectId}/cards/{id}")] HttpRequest req,
            [Table("cards", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log, string projectId, string id)
        {
            log.LogInformation("Deleting a card");
            var deleteOperation = TableOperation.Delete(
                new TableEntity() { PartitionKey = projectId, RowKey = id, ETag = "*" });
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
