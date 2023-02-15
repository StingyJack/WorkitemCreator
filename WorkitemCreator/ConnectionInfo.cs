namespace WorkitemCreator
{
    using Microsoft.VisualStudio.Services.WebApi;

    public class ConnectionInfo
    {
        public VssConnection CurrentConnection { get; }
        public string ProjectCollectionName { get; set; }
        public string ProjectName { get; set; }

        public ConnectionInfo(VssConnection currentConnection)
        {
            CurrentConnection = currentConnection;
        }
    }
}