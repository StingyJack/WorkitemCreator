namespace WorkitemCreator
{
    using System.Collections.Generic;

    public class WorkitemCreationResult : OpResult<WorkitemBaseDetails>
    {
        public List<string> Logs { get; set; } = new List<string>();

        public void SetFail(string errorMessage)
        {
            IsOk = false;
            SetError(errorMessage);
        }

        private void SetError(string errorMessage)
        {
            Errors = string.IsNullOrWhiteSpace(Errors) ? errorMessage : $"{Errors}\n{errorMessage}";
        }

        public void AddNonTermError(string errorMessage)
        {
            SetError(errorMessage);
        }
    }
}