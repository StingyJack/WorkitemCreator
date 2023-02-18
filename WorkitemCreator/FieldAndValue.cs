namespace WorkitemCreator
{
    public class FieldAndValue
    {
        public string FieldName { get; set; }
        public string Value { get; set; }
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     True if the field exists for the workitem type
        /// </summary>
        public bool IsEligible { get; set; }
    }
}