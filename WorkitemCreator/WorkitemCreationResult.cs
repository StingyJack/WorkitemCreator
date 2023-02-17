namespace WorkitemCreator
{
    using System.Collections.Generic;

    public class WorkitemCreationResult
    {
        public bool IsOk { get; set; }

        public List<string> Logs { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();

        public List<WorkitemBaseDetails> WorkitemsCreated { get; set; } = new List<WorkitemBaseDetails>();


        public void SetFail(string errorMessage)
        {
            IsOk = false;
            Errors.Add(errorMessage);
        }

        public void AddNonTermError(string errorMessage)
        {
            Errors.Add(errorMessage);
        }

    }
}
