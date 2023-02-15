namespace WorkitemCreator
{
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
}
