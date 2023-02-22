namespace WorkitemCreator
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Config
    {
        public List<WorkitemTemplate> Templates { get; set; }
        public string ServiceUrl { get; set; }
        public string LastSelectedTeamProjectCollection { get; set; }
        public string LastSelectedTeamProject { get; set; }
        public bool IncludeStoryNumberInTaskTitle { get; set; }
        [JsonIgnore]
        public string CurrentLogFilePath { get; set; }

    }

}
