namespace WorkitemCreator
{
    public class AdditionalFieldAndValueViewModel
    {
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