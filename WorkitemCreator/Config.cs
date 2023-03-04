namespace WorkitemCreator
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Config
    {
        public List<ConfiguredWitReference> Templates { get; set; }
        public string ServiceUrl { get; set; }

        public string LastSelectedTeamProject { get; set; }

        //public bool IncludeStoryNumberInTaskTitle { get; set; }
        [JsonIgnore] public string CurrentLogFilePath { get; set; }
        [JsonIgnore] public string AzDoPat { get; set; }
    }
}