using ResearchToolsBackend.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResearchToolsBackend.Projects
{
    public static class Mappings
    {
        public static ProjectTableEntity ToTableEntity (this Project project)
        {
            return new ProjectTableEntity
            {
                PartitionKey = "PROJECT",
                RowKey = project.Id,
                CreatedTime = project.CreatedTime,
                Name = project.Name
            };
        }

        public static Project ToProject (this ProjectTableEntity project)
        {
            return new Project
            {
                Id = project.RowKey,
                CreatedTime = project.CreatedTime,
                Name = project.Name
            };
        }
    }
}
