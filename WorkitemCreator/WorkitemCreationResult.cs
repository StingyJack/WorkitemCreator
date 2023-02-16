namespace WorkitemCreator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class WorkitemCreationResult
    {
        public bool IsOk { get; set; }

        public List<string> Logs { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();

        public List<WorkitemBaseDetails> WorkitemsCreated { get; set; }


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
