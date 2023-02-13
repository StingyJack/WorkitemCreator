namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class Config
    {
        public List<WorkitemTemplate> Templates { get; set; }
        public string ServiceUrl { get; set; }
        public string LastSelectedTeamProjectCollection { get; set; }
        public string LastSelectedTeamProject { get; set; }
        public bool IncludeStoryNumberInTaskTitle { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    
    public enum WorkitemType
    {
        Unknown,
        UserStory,
        Task
    }

    public class WorkitemTemplate
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public WorkitemType WorkitemType { get; set; }
        public List<WorkitemTemplate> Children { get; set; } = new List<WorkitemTemplate>();
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
