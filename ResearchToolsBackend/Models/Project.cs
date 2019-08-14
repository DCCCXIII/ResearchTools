using System;
using System.Collections.Generic;
using System.Text;

namespace ResearchToolsBackend.Models
{
    public class Project
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public string Name { get; set; }
    }
}
