using Opc.UaFx;
using Opc.UaFx.Client;

namespace MicroSCADA_Client.Data
{
    public static class OPCUABrowse
    {
        public static string UAendpointAddress { get; private set; } = "opc.tcp://laptop-jvk86rqt:51210/UA/SampleServer";

        public static List<OPCNodeObject>? OpcNodes { get; private set; } = new List<OPCNodeObject>() { new OPCNodeObject("0", "Initialize", "0") };

        public static HashSet<string>? SubcribeList { get; private set; } = null;

        public static OpcClient? Client { get; private set; }

        public static bool ConnectionEstabilished { get; private set; } = false;

        public static void HandleNodesTreeViewAfterExpand(string opcnodeid)
        {
            OpcNodeInfo machineNode = Client.BrowseNode(opcnodeid);
            OpcNodes?.Clear();

            foreach (var childNode in machineNode.Children())
            {
                if (!Browse(childNode))
                    break;

                OpcNodes?.Add
                    (new OPCNodeObject(childNode.NodeId.ToString(), childNode.Name.ToString(), childNode.Attribute(OpcAttribute.Value)?.Value.ToString()));

            }
        }


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

                OpcNodes?.Add
                    (new OPCNodeObject(node.NodeId.ToString(), node.Name.ToString(), node.Attribute(OpcAttribute.Value)?.Value.ToString()));

                result = true;
            }
            catch (OpcException ex)
            {
                Console.WriteLine("Failed to browse: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Handles subscription for all TagIds passed to the method 
        /// </summary>
        /// <param name="tagIds"></param>
        /// <returns></returns>
        public static bool Subcribe(List<string> tagIds)
        {
            // Create an (empty) subscription to which we will addd OpcMonitoredItems.
            OpcSubscription subscription;

            try
            {
                subscription = Client.SubscribeNodes();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;

            }
            int index = 0;

            foreach (string tagId in tagIds)
            {
                // Create an OpcMonitoredItem for the NodeId.
                var item = new OpcMonitoredItem(tagId, OpcAttribute.Value);
                item.DataChangeReceived += HandleDataChanged;

                // You can set your own values on the "Tag" property
                // that allows you to identify the source later.
                index++;
                item.Tag = index;

                // Set a custom sampling interval on the 
                // monitored item.
                item.SamplingInterval = 200;

                // Add the item to the subscription.
                subscription.AddMonitoredItem(item);
            }

            // After adding the items (or configuring the subscription), apply the changes.
            subscription.ApplyChanges();

            return true; 
        }

        private static void HandleDataChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            // The tag property contains the previously set value.
            OpcMonitoredItem item = (OpcMonitoredItem)sender;

            string _tagsubscriptiondata = $"Data Change from Index {item.Tag}: {e.Item.Value}";     

            SubcribeList?.Add(_tagsubscriptiondata);
        }
    }
}