namespace WorkitemCreator
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Config
    {
        public const string CONFIG_FILE_NAME = "config.json";

        [JsonProperty(Order = 0)] public string ServiceUrl { get; set; }
        [JsonProperty(Order = 1)] public string LastSelectedTeamProject { get; set; }

        [JsonProperty(Order = 2)] public List<ConfiguredWitReference> Templates { get; set; }

        //public bool IncludeStoryNumberInTaskTitle { get; set; }
        [JsonIgnore] public string CurrentLogFilePath { get; set; }
        [JsonIgnore] public string AzDoPat { get; set; }
    }
}