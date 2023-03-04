namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
    using Newtonsoft.Json;

    public class ConfiguredWitReference
    {
        public string TemplateSetName { get; set; }
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string WorkItemType { get; set; }
        public List<ConfiguredWitReference> Children { get; set; } = new List<ConfiguredWitReference>();
    }

    public class LocalWiTemplateReference : WorkItemTemplateReference
    {
        public string TemplateSetName { get; set; }
        public string Title => $"{WorkItemTypeName} - {Name}";
        public List<LocalWiTemplateReference> ChildTemplates { get; set; } = new List<LocalWiTemplateReference>();

        public LocalWiTemplateReference()
        {
            
        }

        public LocalWiTemplateReference(WorkItemTemplateReference witr)
        {
            Id = witr.Id;
            Name = witr.Name;
            Description = witr.Description;
            Links = witr.Links;
            WorkItemTypeName = witr.WorkItemTypeName;
            Url = witr.Url;
        }

        public LocalWiTemplateReference Clone()
        {
            var returnValue = JsonConvert.DeserializeObject<LocalWiTemplateReference>(JsonConvert.SerializeObject(this));
            return returnValue;
        }
    }
}