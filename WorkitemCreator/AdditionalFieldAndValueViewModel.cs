namespace WorkitemCreator
{
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

    public class AdditionalFieldAndValueViewModel : FieldAndValue
    {
        public List<WorkItemTypeFieldInstance> AvailableFields { get; set; } = new List<WorkItemTypeFieldInstance>();
    }
}