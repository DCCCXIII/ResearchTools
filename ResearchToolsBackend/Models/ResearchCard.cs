﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ResearchToolsBackend.Models
{
    public class ResearchCard
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public string ProjectId { get; set; }
        public string Source { get; set; }
        public string Quote { get; set; }
        public string Summary { get; set; }
        public string Commentary { get; set; }
        public List<string> Keywords = new List<string>();
    }
}
