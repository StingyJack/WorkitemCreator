namespace WorkitemCreator
{
    using System.Collections.Generic;

    public class WorkitemsCreationResult
    {
        public bool IsOk { get; set; }
        public List<string> Logs { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
        public List<WorkitemBaseDetails> WorkitemsCreated { get; set; } = new List<WorkitemBaseDetails>();

        public WorkitemsCreationResult()
        {
            IsOk = true;
        }

        public WorkitemsCreationResult(WorkitemCreationResult incoming)
        {
            IsOk = true;
            Merge(incoming);
        }

        public void Merge(WorkitemCreationResult incoming)
        {
            IsOk = IsOk && incoming.IsOk;
            Logs.AddRange(incoming.Logs);
            Errors.Add(incoming.Errors);
            WorkitemsCreated.Add(incoming.Data);
        }

        public void SetFail(string errorMessage)
        {
            IsOk = false;
            Errors.Add(errorMessage);
        }
    }
}