namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.TeamFoundation.Core.WebApi;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
    using Microsoft.VisualStudio.Services.Client;
    using Microsoft.VisualStudio.Services.Common;
    using Microsoft.VisualStudio.Services.WebApi;
    using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

    public class AzDoService
    {
        public string ProjectCollectionName { get; set; }
        public string ProjectName { get; set; }
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


            var projectClient = _connection.GetClient<ProjectHttpClient>();
            var projects = (await projectClient.GetProjects()).ToList();
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


            var witc = _connection.GetClient<WorkItemTrackingHttpClient>();
            var wiTypes = (await witc.GetWorkItemTypesAsync(ProjectName)).ToList();
            returnValue.IsOk = true;
            returnValue.Data = wiTypes;
            return returnValue;
        }

        public async Task<OpResult<List<WorkItemTypeFieldWithReferences>>> GetWorkItemTypeFieldsAsync(string workItemTypeName)
        {
            var returnValue = new OpResult<List<WorkItemTypeFieldWithReferences>>();
            if (_isConnected == false)
            {
                returnValue.IsOk = false;
                returnValue.Errors = "Not connected, so cant get workitem types  ";
                return returnValue;
            }

            var client = _connection.GetClient<WorkItemTrackingHttpClient>();
            var typeFields = (await client.GetWorkItemTypeFieldsWithReferencesAsync(ProjectName, workItemTypeName, WorkItemTypeFieldsExpandLevel.All)).ToList();
            returnValue.IsOk = true;
            returnValue.Data = typeFields;
            return returnValue;
        }

        public async Task<OpResult<List<WorkItemClassificationNode>>> GetIterationsAsync()
        {
            return await GetClassificationNodes(TreeNodeStructureType.Iteration);
        }

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

            var nodes = await client.GetClassificationNodesAsync(ProjectName, new[] { root.Id }, depth: 20);
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


    }
}