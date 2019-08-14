using ResearchToolsBackend.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResearchToolsBackend.ResearchCards
{
    public static class Mappings
    {
        public static ResearchCardTableEntity ToTableEntity (this ResearchCard researchCard)
        {
            return new ResearchCardTableEntity
            {
                PartitionKey = researchCard.ProjectId,
                RowKey = researchCard.Id,
                CreatedTime = researchCard.CreatedTime,
                Source = researchCard.Source,
                Quote = researchCard.Quote,
                Summary = researchCard.Summary,
                Commentary = researchCard.Commentary,
                Keywords = researchCard.Keywords
            };
        }

        public static ResearchCard ToResearchCard (this ResearchCardTableEntity researchCard)
        {
            return new ResearchCard
            {
                Id = researchCard.RowKey,
                ProjectId = researchCard.PartitionKey,
                CreatedTime = researchCard.CreatedTime,
                Source = researchCard.Source,
                Quote = researchCard.Quote,
                Summary = researchCard.Summary,
                Commentary = researchCard.Commentary,
                Keywords = researchCard.Keywords
            };
        }
    }
}
