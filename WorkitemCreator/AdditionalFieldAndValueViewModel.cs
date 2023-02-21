namespace WorkitemCreator
{
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

    public class AdditionalFieldAndValueViewModel : FieldAndValue
    {
        public List<WorkItemTypeFieldInstance> AllFields { get; set; } = new List<WorkItemTypeFieldInstance>();

        // these will be nice to have to hide already selected fields from other field dropdown lists.
        //public List<WorkItemTypeFieldInstance> FieldsUsed { get; set; } = new List<WorkItemTypeFieldInstance>(); 

        //public List<WorkItemTypeFieldInstance> AvailableFields => AllFields.Except(FieldsUsed).ToList();
    }
}