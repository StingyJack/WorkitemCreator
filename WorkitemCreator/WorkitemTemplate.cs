namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;

    public class WorkitemTemplate
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string WorkitemType { get; set; }
        public List<WorkitemTemplate> Children { get; set; } = new List<WorkitemTemplate>();
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public List<string> GetAllWiTypesInTemplate()
        {
            var returnValue = new List<string> { WorkitemType };

            foreach (var c in Children)
            {
                if (returnValue.Contains(c.WorkitemType) == false)
                {
                    returnValue.Add(c.WorkitemType);
                }
            }


            return returnValue;
        }
    }
}