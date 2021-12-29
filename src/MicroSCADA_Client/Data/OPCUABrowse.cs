using Opc.UaFx;
using Opc.UaFx.Client;
using static MicroSCADA_Client.Pages.Index;


namespace MicroSCADA_Client.Data
{
    public class OPCNodeObject
    {
        public string? Id { get; set; }
        public string? NodeName { get; set; }
        public string? NodeValue { get; set; }

        public OPCNodeObject (string? id, string? nodeName, string? nodeValue)
        {
            Id = id;
            NodeName = nodeName;
            NodeValue = nodeValue;
        }
    }
    public static class OPCUABrowse
    {
        #region Properties    
        public static string UAendpointAddress { get; set; } = "opc.tcp://laptop-jvk86rqt:51210/UA/SampleServer";

        public static List<OPCNodeObject>? opcNodes { get; private set; } = new List<OPCNodeObject>() { new OPCNodeObject("0", "Initialize", "0")};
        public static OpcClient Client { get; set; }

        public static bool ConnectionEstabilished { get; set; } = false;
        #endregion

        #region Methods

        /// <summary>
        /// Initializes the OPC UA connection by creating a certificate and then connecting to the given UAendpointAddress
        /// </summary>
        public static void Initialize()
        {
            Client = new OpcClient(UAendpointAddress);

            //might need to expand this by putting a try catch around it and trying
            //all security methods instead of hardcoding a specific one
            Client.Security.EndpointPolicy = new OpcSecurityPolicy(OpcSecurityMode.Sign, OpcSecurityAlgorithm.Basic256Sha256);

            var certificate = OpcCertificateManager.CreateCertificate(Client);
            Client.Certificate = certificate;

            try
            {
                Client.Connect();
                var node = Client.BrowseNode(OpcObjectTypes.ObjectsFolder);
                Browse(node);
                ConnectionEstabilished = true;
            }
            catch (OpcException ex)
            {
                Console.WriteLine("Failed to Connect: " + ex.Message);
            }
        }

        private static bool Browse(OpcNodeInfo node)
        {
            //if (opcNodes is not null) opcNodes.Clear();

            var result = false;

            try
            {

                if (node is OpcObjectNodeInfo)
                {
                    if (node.Reference.TypeDefinitionId == Opc.Ua.ObjectTypeIds.FolderType) { }
                }
                else if (node is OpcMethodNodeInfo)
                {

                }
                else if (node is OpcVariableNodeInfo)
                {
                    if (node.Reference.ReferenceType == OpcReferenceType.HasProperty) { }
                }

                opcNodes?.Add
                    ( new OPCNodeObject(node.NodeId.ToString(), node.Name.ToString(), node.Attribute(OpcAttribute.Value)?.Value.ToString()));

                result = true;
            }
            catch (OpcException ex)
            {
                Console.WriteLine("Failed to browse: " + ex.Message);
            }

            return result;
        }

        public static void HandleNodesTreeViewAfterExpand(string opcnodeid)
        {
            OpcNodeInfo machineNode = Client.BrowseNode(opcnodeid);
            opcNodes?.Clear();

            foreach (var childNode in machineNode.Children())
            {
                if (!Browse(childNode))
                    break;

                opcNodes.Add
                    (new OPCNodeObject(childNode.NodeId.ToString(), childNode.Name.ToString(), childNode.Attribute(OpcAttribute.Value)?.Value.ToString()));

            }
        }
        #endregion
    }
}
