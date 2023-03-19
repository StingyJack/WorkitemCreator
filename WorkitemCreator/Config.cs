namespace WorkitemCreator
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Config
    {
        public const string CONFIG_FILE_NAME = "config.json";

        [JsonProperty(Order = 0)] public string ServiceUrl { get; set; }
        [JsonProperty(Order = 1)] public string LastSelectedTeamProject { get; set; }
        [JsonProperty(Order = 2)] public string LastSelectedTeam { get; set; }

        [JsonProperty(Order = 3)] public Dictionary<string, List<ConfiguredWitReference>> TeamTemplates { get; set; } = new();

        //public bool IncludeStoryNumberInTaskTitle { get; set; }
        [JsonIgnore] public string CurrentLogFilePath { get; set; }
        [JsonIgnore] public string AzDoPat { get; set; }

        public List<ConfiguredWitReference> GetTeamTemplates(string teamName)
        {
            if (TeamTemplates.ContainsKey(teamName))
            {
                return TeamTemplates[teamName];
            }

            return new List<ConfiguredWitReference>();
        }
    }
}