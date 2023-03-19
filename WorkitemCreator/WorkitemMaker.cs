namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
    using Microsoft.VisualStudio.Services.WebApi;
    using Microsoft.VisualStudio.Services.WebApi.Patch;
    using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

    public class WorkitemMaker
    {
        private readonly AzDoService _azDoService;

        public WorkitemMaker(AzDoService azDoService)
        {
            _azDoService = azDoService ?? throw new ArgumentNullException(nameof(azDoService));
        }

        public async Task<WorkitemsCreationResult> CreateWorkitemsFromTemplateAsync(LocalWiTemplateReference parentWiTemplateReference)
        {
            var returnValue = new WorkitemsCreationResult { IsOk = true };

            if (parentWiTemplateReference == null)
            {
                returnValue.IsOk = false;
                returnValue.Errors.Add("Need a parent template to create workitems");
                return returnValue;
            }

            returnValue.Logs.Add("Getting workitem types");
            var workitemTypesResult = await _azDoService.GetWorkItemTypesAsync();
            if (workitemTypesResult.IsOk == false)
            {
                returnValue.IsOk = false;
                returnValue.Errors.Add(workitemTypesResult.Errors);
                return returnValue;
            }

            var parentWorkitemCreationResult = await CreateWorkitemFromTemplateAsync(parentWiTemplateReference, workitemTypesResult.Data);
            returnValue.Merge(parentWorkitemCreationResult);
            if (parentWorkitemCreationResult.IsOk == false)
            {
                return returnValue;
            }

            foreach (var child in parentWiTemplateReference.ChildTemplates)
            {
                var childWorkitemCreationResult = await CreateWorkitemFromTemplateAsync(child, workitemTypesResult.Data, parentWorkitemCreationResult.Data.Uri);

                returnValue.Merge(childWorkitemCreationResult);
                if (childWorkitemCreationResult.IsOk == false)
                {
                    break;
                }
            }

            return returnValue;
        }

        private async Task<WorkitemCreationResult> CreateWorkitemFromTemplateAsync(WorkItemTemplateReference wiTemplateReference, IEnumerable<WorkItemType> workItemTypes, string parentUrl = null)
        {
            var returnValue = new WorkitemCreationResult();
            var workitemTemplateResult = await _azDoService.GetTeamWorkitemTemplateAsync(wiTemplateReference.Id);

            if (workitemTemplateResult.IsOk == false)
            {
                returnValue.SetFail(workitemTemplateResult.Errors);
                return returnValue;
            }

            var wiType = workItemTypes.FirstOrDefault(p => string.Equals(wiTemplateReference.WorkItemTypeName, p.Name, StringComparison.OrdinalIgnoreCase));
            if (wiType == null)
            {
                returnValue.SetFail($"The workitem type {wiTemplateReference.WorkItemTypeName} does not exist for project {_azDoService.ProjectName}");
                return returnValue;
            }

            var workitemFromTemplateResult = await _azDoService.GetWorkitemFromBaseTemplateAsync(wiType.Name);
            if (workitemFromTemplateResult.IsOk == false)
            {
                returnValue.SetFail(workitemFromTemplateResult.Errors);
                return returnValue;
            }

            var workitemCandidate = BuildWorkitemFromTemplate(workitemFromTemplateResult.Data, workitemTemplateResult.Data, wiType, ref returnValue);
            if (string.IsNullOrWhiteSpace(parentUrl) == false)
            {
                var parentRelation = new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = parentUrl
                    }
                };
                workitemCandidate.Add(parentRelation);
            }

            WorkItem workitem;
            try
            {
                var workitemCreationResult = await _azDoService.CreateWorkitemAsync(workitemCandidate, wiTemplateReference.WorkItemTypeName);
                if (workitemCreationResult.IsOk == false)
                {
                    returnValue.SetFail(workitemCreationResult.Errors);
                    return returnValue;
                }

                workitem = workitemCreationResult.Data;
                if (workitem.Id.HasValue == false)
                {
                    throw new InvalidOperationException("Workitem was not created, Id is null.");
                }
            }
            catch (RuleValidationException rve) // want to do something different, not sure what yet
            {
                returnValue.SetFail(rve.Message);
                Trace.TraceError(rve.ToString());
                return returnValue;
            }
            catch (Exception e)
            {
                returnValue.SetFail(e.ToString());
                Trace.TraceError(e.ToString());
                return returnValue;
            }

            returnValue.Data = new WorkitemBaseDetails
            {
                Id = workitem.Id.Value,
                Title = Convert.ToString(workitem.Fields["System.Title"]),
                Uri = ((ReferenceLink)workitem.Links.Links["html"]).Href,
                WorkitemTypeName = wiTemplateReference.WorkItemTypeName
            };
            returnValue.IsOk = true;
            return returnValue;
        }

        private static JsonPatchDocument BuildWorkitemFromTemplate(WorkItem workitemFromTemplate, WorkItemTemplate wit, WorkItemType wiType, ref WorkitemCreationResult returnValue)
        {
            var workitemCandidate = new JsonPatchDocument();
            foreach (var wiField in workitemFromTemplate.Fields)
            {
                ApplyFieldToCandidate(wiType, returnValue, wiField, workitemCandidate);
            }

            foreach (var templateField in wit.Fields)
            {
                var tfKvp = new KeyValuePair<string, object>(templateField.Key, templateField.Value);

                ApplyFieldToCandidate(wiType, returnValue, tfKvp, workitemCandidate);
            }

            workitemCandidate.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.History",
                Value = "Created with WorkitemCreator"
            });

            return workitemCandidate;
        }

        private static void ApplyFieldToCandidate(WorkItemType wiType, WorkitemCreationResult returnValue,
            KeyValuePair<string, object> wiField, JsonPatchDocument workitemCandidate)
        {
            var field = wiType.Fields.FirstOrDefault(f => string.Equals(f.ReferenceName, wiField.Key, StringComparison.OrdinalIgnoreCase));
            if (field == null)
            {
                returnValue.AddNonTermError($"Could not find a workitem field matching the name {wiField.Key}");
                return;
            }

            var fieldPath = $"/fields/{field.ReferenceName}";
            var existingPatchRecord = workitemCandidate.SingleOrDefault(j => j.Path.Equals(fieldPath, StringComparison.OrdinalIgnoreCase));

            if (existingPatchRecord != null)
            {
                returnValue.AddNonTermError($"Field {field.Name} has already been applied to this workitem once. The previous value '{existingPatchRecord.Value}' " +
                                            $"will be replaced with the new value '{wiField.Value}'");
                existingPatchRecord.Value = wiField.Value;
                return;
            }

            var jpo = new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = fieldPath, 
                Value = wiField.Value
            };

            workitemCandidate.Add(jpo);
        }
    }
}