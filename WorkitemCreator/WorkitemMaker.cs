namespace WorkitemCreator
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
    using Microsoft.VisualStudio.Services.WebApi.Patch;
    using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

    public class WorkitemMaker
    {
        private readonly AzDoService _azDoService;

        public WorkitemMaker(AzDoService azDoService)
        {
            _azDoService = azDoService ?? throw new ArgumentNullException(nameof(azDoService));
        }

        public async Task<WorkitemCreationResult> CreateWorkitemAsync(WorkitemTemplate wit)
        {
            _ = wit ?? throw new ArgumentNullException(nameof(wit));

            var returnValue = new WorkitemCreationResult();

            var workitemTypesResult = await _azDoService.GetWorkItemTypesAsync();
            if (workitemTypesResult.IsOk == false)
            {
                returnValue.IsOk = false;
                returnValue.Errors.Add(workitemTypesResult.Errors);
                return returnValue;
            }

            var parentWiType = workitemTypesResult.Data.FirstOrDefault(p => string.Equals(wit.WorkitemType, p.Name, StringComparison.OrdinalIgnoreCase));
            if (parentWiType == null)
            {
                returnValue.SetFail($"The parent workitem type {wit.WorkitemType} does not exist for project {_azDoService.ProjectName}");
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
                var parentWorkitemCreationResult = await _azDoService.CreateWorkitemAsync(parentWorkitemCandidate, wit.WorkitemType);
                if (parentWorkitemCreationResult.IsOk == false)
                {
                    returnValue.SetFail(parentWorkitemCreationResult.Errors);
                    return returnValue;
                }

                parentWorkitem = parentWorkitemCreationResult.Data;
                if (parentWorkitem.Id.HasValue == false)
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
                var wiType = workitemTypesResult.Data.FirstOrDefault(p => string.Equals(cwit.WorkitemType, p.Name, StringComparison.OrdinalIgnoreCase));
                if (wiType == null)
                {
                    returnValue.SetFail($"The child workitem type {cwit.WorkitemType} does not exist for project {_azDoService.ProjectName}");
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
                    var childWorkitemCreationResult = await _azDoService.CreateWorkitemAsync(childWorkItemCandidate, cwit.WorkitemType);
                    if (childWorkitemCreationResult.IsOk == false)
                    {
                        returnValue.SetFail(childWorkitemCreationResult.Errors);
                        return returnValue;
                    }

                    childWorkitem = childWorkitemCreationResult.Data; 
                    if (childWorkitem.Id.HasValue == false)
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
            } //next child


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