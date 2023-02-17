namespace WorkitemCreator
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
    using Microsoft.VisualStudio.Services.WebApi.Patch;
    using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

    public class WorkitemMaker
    {
        private readonly ConnectionInfo _connectionInfo;

        public WorkitemMaker(ConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo ?? throw new ArgumentNullException(nameof(connectionInfo));
        }

        public async Task<WorkitemCreationResult> CreateWorkitemAsync(WorkitemTemplate wit)
        {
            _ = wit ?? throw new ArgumentNullException(nameof(wit));

            var returnValue = new WorkitemCreationResult();

            var witc = _connectionInfo.CurrentConnection.GetClient<WorkItemTrackingHttpClient>();
            var projectWiTypes = await witc.GetWorkItemTypesAsync(_connectionInfo.ProjectName);
            var parentWiType = projectWiTypes.FirstOrDefault(p => string.Equals(wit.WorkitemType, p.Name, StringComparison.OrdinalIgnoreCase));
            if (parentWiType == null)
            {
                returnValue.SetFail($"The parent workitem type {wit.WorkitemType} does not exist for project {_connectionInfo.ProjectName}");
                return returnValue;
            }

            // get the parent template wi type
            //use the .Fields on that to build the JsonPatchDocument 
            // create it, get reference to it
            // then create the child items being sure to reference back to the parent.
            //needs some status reporting/logging in here
            var parentWorkitemCandidate = ApplyFieldsAndValues(wit, parentWiType, returnValue);
            WorkItem parentWorkitem;
            try
            {
                parentWorkitem = await witc.CreateWorkItemAsync(parentWorkitemCandidate, _connectionInfo.ProjectName, wit.WorkitemType);
                if (parentWorkitem == null || parentWorkitem.Id.HasValue == false)
                {
                    throw new InvalidOperationException("Parent workitem was not created.");
                }
            }
            catch (Exception e)
            {
                returnValue.SetFail(e.ToString());
                Trace.TraceError(e.ToString());
                return returnValue;
            }

            returnValue.WorkitemsCreated.Add(new WorkitemBaseDetails
            {
                Id = parentWorkitem.Id.Value,
                WorkitemType = wit.WorkitemType,
                Uri = parentWorkitem.Url,
                Title = wit.Title
            });

            foreach (var cwit in wit.Children)
            {
                var wiType = projectWiTypes.FirstOrDefault(p => string.Equals(cwit.WorkitemType, p.Name, StringComparison.OrdinalIgnoreCase));
                if (wiType == null)
                {
                    returnValue.SetFail($"The child workitem type {cwit.WorkitemType} does not exist for project {_connectionInfo.ProjectName}");
                    return returnValue;
                }

                var childWorkItemCandidate = ApplyFieldsAndValues(cwit, wiType, returnValue);
                var parentRelation = new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = parentWorkitem.Url
                    }
                };
                childWorkItemCandidate.Add(parentRelation);

                WorkItem childWorkitem;
                try
                {
                    childWorkitem = await witc.CreateWorkItemAsync(childWorkItemCandidate, _connectionInfo.ProjectName, cwit.WorkitemType);
                    if (childWorkitem == null || childWorkitem.Id.HasValue == false)
                    {
                        throw new InvalidOperationException("Child workitem was not created.");
                    }
                }
                catch (Exception e)
                {
                    returnValue.SetFail(e.Message);
                    Trace.TraceError(e.ToString());
                    return returnValue;
                }

                returnValue.WorkitemsCreated.Add(new WorkitemBaseDetails
                {
                    Id = childWorkitem.Id.Value,
                    WorkitemType = cwit.WorkitemType,
                    Uri = childWorkitem.Url,
                    Title = cwit.Title
                });
            }


            returnValue.IsOk = true;


            return returnValue;
        }

        private static JsonPatchDocument ApplyFieldsAndValues(WorkitemTemplate wit, WorkItemType wiType, WorkitemCreationResult returnValue)
        {
            var workitemCandidate = new JsonPatchDocument();
            foreach (var item in wit.AsDictionary())
            {
                var field = wiType.Fields.FirstOrDefault(f => string.Equals(f.Name, item.Key, StringComparison.OrdinalIgnoreCase));
                if (field == null)
                {
                    returnValue.AddNonTermError($"Could not find a workitem field matching the name {item.Key}");
                    continue;
                }

                var jpo = new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = $"/fields/{field.ReferenceName}", // should be like "/fields/System.Title"
                    Value = item.Value
                };
                workitemCandidate.Add(jpo);
            }


            workitemCandidate.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.History",
                Value = "Created by WorkitemCreator",
            });

            return workitemCandidate;
        }
    }
}