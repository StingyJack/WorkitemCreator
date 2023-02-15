namespace WorkitemCreator
{
    using System;
    using System.CodeDom;
    using System.Threading.Tasks;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
    using Microsoft.VisualStudio.Services.WebApi;

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

            //get fields, match them to the given properties

            var witc = _connectionInfo.CurrentConnection.GetClient<WorkItemTrackingHttpClient>();
            var projectWiTypes = await witc.GetWorkItemTypesAsync(_connectionInfo.ProjectName);

            // get the paretnt template wi type
            //use the .Fields on that to build the JsonPatchDocument 
            // create it, get reference to it
            // then create the child items being sure to reference back to the parent.

            foreach (var wiType in projectWiTypes)
            {
                WorkitemType parsedWiType;
                if (Enum.TryParse(wiType.Name, out parsedWiType) == false)
                {
                    continue;
                }

            


            }

            var fields = await witc.GetFieldsAsync(_connectionInfo.ProjectName);


            throw new NotImplementedException(nameof(wit));
        }


    }
}