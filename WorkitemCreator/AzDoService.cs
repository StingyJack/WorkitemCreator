namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.TeamFoundation.Core.WebApi;
    using Microsoft.TeamFoundation.Core.WebApi.Types;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
    using Microsoft.VisualStudio.Services.Client;
    using Microsoft.VisualStudio.Services.Common;
    using Microsoft.VisualStudio.Services.WebApi;
    using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

    public class AzDoService
    {
        public string ProjectName { get; set; }
        public Guid ProjectId { get; set; }
        public string TeamName { get; set; }

        private VssConnection _connection;

        private bool _isConnected;

        public async Task<ConnectResult> ConnectAsync(string uriString, string pat = null)
        {
            _ = uriString ?? throw new ArgumentNullException(nameof(uriString));
            var returnValue = new ConnectResult();

            if (_isConnected)
            {
                returnValue.IsConnected = true;
                return returnValue;
            }

            var uri = new Uri(uriString);

            if (string.IsNullOrWhiteSpace(pat))
            {
                var creds = new VssClientCredentials(new WindowsCredential(false),
                    new VssFederatedCredential(false),
                    CredentialPromptType.PromptIfNeeded);

                _connection = new VssConnection(uri, creds);
            }
            else
            {
                _connection = new VssConnection(uri, new VssBasicCredential(string.Empty, pat));
            }

            try
            {
                await _connection.ConnectAsync();
                _isConnected = true;
            }
            catch (Exception ex)
            {
                returnValue.ConnectionError = ex.ToString();
                _isConnected = false;
            }

            returnValue.IsConnected = _isConnected;
            return returnValue;
        }


        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public Task<OpResult<List<TeamProjectCollectionReference>>> GetProjectCollectionsAsync()
        {
            throw new NotSupportedException("Getting the project collections requires a different URL than all the other operations, and " +
                                            "possibly would require a separate auth. Just use the project collection url when connecting and" +
                                            "avoid doing this project collection listing.");

            //var returnValue = new OpResult<List<TeamProjectCollectionReference>>();
            //if (_isConnected == false)
            //{
            //    returnValue.IsOk = false;
            //    returnValue.Errors = $"Not connected, so cant get project collections";
            //    return returnValue;
            //}


            //var projectCollectionClient = _connection.GetClient<ProjectCollectionHttpClient>();
            //var projectCollections = (await projectCollectionClient.GetProjectCollections()).ToList();
            //returnValue.IsOk = true;
            //returnValue.Data = projectCollections;
            //return returnValue;
        }

        public async Task<OpResult<List<TeamProjectReference>>> GetProjectsAsync()
        {
            var returnValue = new OpResult<List<TeamProjectReference>>();
            if (_isConnected == false)
            {
                returnValue.IsOk = false;
                returnValue.Errors = "Not connected, so cant get projects";
                return returnValue;
            }


            var client = _connection.GetClient<ProjectHttpClient>();
            var projects = (await client.GetProjects()).ToList();
            returnValue.IsOk = true;
            returnValue.Data = projects;
            return returnValue;
        }

        public async Task<OpResult<List<WorkItemType>>> GetWorkItemTypesAsync()
        {
            var returnValue = new OpResult<List<WorkItemType>>();
            if (_isConnected == false)
            {
                returnValue.IsOk = false;
                returnValue.Errors = "Not connected, so cant get workitem types  ";
                return returnValue;
            }


            var client = _connection.GetClient<WorkItemTrackingHttpClient>();
            var wiTypes = (await client.GetWorkItemTypesAsync(ProjectName)).ToList();
            returnValue.IsOk = true;
            returnValue.Data = wiTypes;
            return returnValue;
        }

        public async Task<OpResult<List<WebApiTeam>>> GetTeamsAsync()
        {
            var returnValue = new OpResult<List<WebApiTeam>>();
            if (_isConnected == false)
            {
                returnValue.IsOk = false;
                returnValue.Errors = "Not connected, so cant get teams";
                return returnValue;
            }


            var client = _connection.GetClient<TeamHttpClient>();
            var data = (await client.GetTeamsAsync(ProjectId.ToString())).ToList();
            returnValue.IsOk = true;
            returnValue.Data = data;
            return returnValue;
        }

        public async Task<OpResult<WorkItemTemplate>> GetTeamWorkitemTemplateAsync(Guid templateId)
        {
            var returnValue = new OpResult<WorkItemTemplate>();
            if (_isConnected == false)
            {
                returnValue.IsOk = false;
                returnValue.Errors = "Not connected, so cant get template ";
                return returnValue;
            }


            var client = _connection.GetClient<WorkItemTrackingHttpClient>();
            var data = await client.GetTemplateAsync(new TeamContext(ProjectName, TeamName), templateId);
            returnValue.IsOk = true;
            returnValue.Data = data;
            return returnValue;
        }

        public async Task<OpResult<List<WorkItemTemplateReference>>> GetTeamWorkitemTemplateReferencesAsync()
        {
            var returnValue = new OpResult<List<WorkItemTemplateReference>>();
            if (_isConnected == false)
            {
                returnValue.IsOk = false;
                returnValue.Errors = "Not connected, so cant get template references";
                return returnValue;
            }


            var client = _connection.GetClient<WorkItemTrackingHttpClient>();
            var data = (await client.GetTemplatesAsync(new TeamContext(ProjectName, TeamName))).ToList();
            returnValue.IsOk = true;
            returnValue.Data = data;
            return returnValue;
        }


        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public async Task<OpResult<List<WorkItemClassificationNode>>> GetIterationsAsync()
        {
            return await GetClassificationNodes(TreeNodeStructureType.Iteration);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public async Task<OpResult<List<WorkItemClassificationNode>>> GetAreaPathsAsync()
        {
            return await GetClassificationNodes(TreeNodeStructureType.Area);
        }

        private async Task<OpResult<List<WorkItemClassificationNode>>> GetClassificationNodes(TreeNodeStructureType nodeType)
        {
            var returnValue = new OpResult<List<WorkItemClassificationNode>>();
            if (_isConnected == false)
            {
                returnValue.IsOk = false;
                returnValue.Errors = $"Not connected, so cant get {nodeType}";
                return returnValue;
            }

            var client = _connection.GetClient<WorkItemTrackingHttpClient>();
            var rootClassificationNodes = (await client.GetRootNodesAsync(ProjectName)).ToList();
            var root = rootClassificationNodes.First(r => r.StructureType == nodeType);

            var nodes = await client.GetClassificationNodesAsync(ProjectName, new[] { root.Id }, 20);
            returnValue.IsOk = true;
            returnValue.Data = nodes;
            return returnValue;
        }

        public async Task<OpResult<WorkItem>> CreateWorkitemAsync(JsonPatchDocument candidateWorkitem, string workItemType)
        {
            var returnValue = new OpResult<WorkItem>();
            if (_isConnected == false)
            {
                returnValue.IsOk = false;
                returnValue.Errors = "Not connected, so cant create a workitem";
                return returnValue;
            }

            var witc = _connection.GetClient<WorkItemTrackingHttpClient>();
            var workitem = await witc.CreateWorkItemAsync(candidateWorkitem, ProjectName, workItemType);
            returnValue.IsOk = true;
            returnValue.Data = workitem;
            return returnValue;
        }

        public async Task<OpResult<WorkItem>> GetWorkitemFromBaseTemplateAsync(string workItemType)
        {
            var returnValue = new OpResult<WorkItem>();
            if (_isConnected == false)
            {
                returnValue.IsOk = false;
                returnValue.Errors = "Not connected, so cant create a workitem";
                return returnValue;
            }

            var witc = _connection.GetClient<WorkItemTrackingHttpClient>();
            var workitem = await witc.GetWorkItemTemplateAsync(ProjectName, workItemType);
            returnValue.IsOk = true;
            returnValue.Data = workitem;
            return returnValue;
        }
    }
}