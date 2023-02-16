namespace WorkitemCreator
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
    using Microsoft.VisualStudio.Services.WebApi.Patch;
    using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

    public class WorkitemMaker
    {
        private readonly ConnectionInfo _connectionInfo;

        public WorkitemMaker(ConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo ?? throw new ArgumentNullException(nameof(connectionInfo));
        }

        public async Task CreateWorkitemAsync(WorkitemTemplate wit)
        {
            _ = wit ?? throw new ArgumentNullException(nameof(wit));

            //is the type present for the project? (no User Story in Scrum, only PBI

            var witc = _connectionInfo.CurrentConnection.GetClient<WorkItemTrackingHttpClient>();
            var projectWiTypes = await witc.GetWorkItemTypesAsync(_connectionInfo.ProjectName);

            if (projectWiTypes.Any(p => string.Equals(wit.WorkitemType, p.Name, StringComparison.OrdinalIgnoreCase)) == false)
            {
                throw new ArgumentException($"The workitem type {wit.WorkitemType} does not exist for project {_connectionInfo.ProjectName}");
            }

            // get the parent template wi type
            //use the .Fields on that to build the JsonPatchDocument 
            // create it, get reference to it
            // then create the child items being sure to reference back to the parent.
            //needs some status reporting/logging in here
            var parentWorkitemCandidate = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = wit.Title
                }
            };

            var parentWorkitem = await witc.CreateWorkItemAsync(parentWorkitemCandidate, _connectionInfo.ProjectName, wit.WorkitemType);


            throw new NotImplementedException(nameof(wit));
        }
    }
}