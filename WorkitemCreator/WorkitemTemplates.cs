namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;

    class WorkitemTemplates
    {
        public List<WorkitemTemplate> Templates { get; set; }
        public string ServiceUrl { get; set; }
    }


    enum WorkitemType
    {
        Unknown,
        UserStory,
        Task
    }

    class WorkitemTemplate
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public WorkitemType WorkitemType { get; set; }
        public List<WorkitemTemplate> Children { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
