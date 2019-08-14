using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResearchToolsBackend.ResearchCards
{
    public class ResearchCardTableEntity : TableEntity
    {
        public DateTime CreatedTime { get; set; }
        public string Source { get; set; }
        public string Quote { get; set; }
        public string Summary { get; set; }
        public string Commentary { get; set; }
        public List<string> Keywords = new List<string>();
    }
}
