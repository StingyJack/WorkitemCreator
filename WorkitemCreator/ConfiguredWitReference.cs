namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;

    public class ConfiguredWitReference
    {
        public string TemplateSetName { get; set; }
        public Guid TemplateId { get; set; }
        public List<ConfiguredWitReference> Children { get; set; } = new List<ConfiguredWitReference>();
    }
}