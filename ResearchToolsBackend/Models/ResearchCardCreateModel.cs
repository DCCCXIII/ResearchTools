using System;
using System.Collections.Generic;
using System.Text;

namespace ResearchToolsBackend.Models
{
    public class ResearchCardCreateModel
    {
        public string Source { get; set; }
        public string Quote { get; set; }
        public string Summary { get; set; }
        public string Commentary { get; set; }
        public List<string> Keywords = new List<string>();
    }
}
