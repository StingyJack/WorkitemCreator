namespace WorkitemCreator
{
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

    public class AdditionalFieldAndValueViewModel 
    {
        public List<WorkItemTypeFieldInstance> AllFields { get; set; } = new List<WorkItemTypeFieldInstance>();

        // these will be nice to have to hide already selected fields from other field dropdown lists.
        //public List<WorkItemTypeFieldInstance> FieldsUsed { get; set; } = new List<WorkItemTypeFieldInstance>(); 

        //public List<WorkItemTypeFieldInstance> AvailableFields => AllFields.Except(FieldsUsed).ToList();

        /// <summary>
        ///     The friendly name of the field.  "Iteration Path" for example
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        ///     The system reference name of the field. "System.IterationPath" for example
        /// </summary>
        public string FieldReferenceName { get; set; }

        /// <summary>
        ///     A documentation tag is not going to help if you dont know what this is for.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     True if the field should be included when creating workitems. 
        /// </summary>
        public bool IncludeWhenCreating { get; set; }

        /// <summary>
        ///     True if the field exists for the workitem type
        /// </summary>
        public bool IsEligible { get; set; }

    }
}