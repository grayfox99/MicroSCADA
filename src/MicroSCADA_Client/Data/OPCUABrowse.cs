using Opc.UaFx;
using Opc.UaFx.Client;
using Microsoft.AspNetCore.Components;
using MicroSCADA_Client.Pages;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MicroSCADA_Client.Data
{
    public class OPCUABrowse: INotifyPropertyChanged
    {
        public  string UAendpointAddress { get; private set; } = "opc.tcp://laptop-jvk86rqt:51210/UA/SampleServer";

        public  List<OPCNodeObject>? OpcNodes { get; private set; } = new List<OPCNodeObject>() { new OPCNodeObject("0", "Initialize", "0") };

        public List<int> TagValues { get; private set; }  =  new List<int> { 0 };

        public List<string> SubscribeChanged { get; set; } = new List<string> { "Empty" };

        public OpcClient Client { get; private set; } = new OpcClient();

        public bool ConnectionEstabilished { get; private set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public void HandleNodesTreeViewAfterExpand(string opcnodeid)
        {
            OpcNodeInfo machineNode = Client.BrowseNode(opcnodeid);
            OpcNodes?.Clear();

            try
            {
                foreach (var childNode in machineNode.Children())
                {
                    if (!Browse(childNode))
                        break;

                    OpcNodes?.Add
                        (new OPCNodeObject(childNode.NodeId.ToString(), childNode.Name.ToString(), childNode.Attribute(OpcAttribute.Value)?.Value.ToString()));

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Initializes the OPC UA connection by creating a certificate and then connecting to the given UAendpointAddress
        /// </summary>
        public void Initialize()
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

        private bool Browse(OpcNodeInfo node)
        {
            //if (opcNodes is not null) opcNodes.Clear();

            var result = false;

            try
            {

                //if (node is OpcObjectNodeInfo)
                //{
                //    if (node.Reference.TypeDefinitionId == Opc.Ua.ObjectTypeIds.FolderType) { }
                //}
                //else if (node is OpcMethodNodeInfo)
                //{

                //}
                //else if (node is OpcVariableNodeInfo)
                //{
                //    if (node.Reference.ReferenceType == OpcReferenceType.HasProperty) { }
                //}

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
        public bool Subcribe(List<string> tagIds)
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
                item.SamplingInterval = 500;

                // Add the item to the subscription.
                subscription.AddMonitoredItem(item);
            }

            // After adding the items (or configuring the subscription), apply the changes.
            subscription.ApplyChanges();

            return true; 
        }

        public void HandleDataChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            // The tag property contains the previously set value.
            OpcMonitoredItem item = (OpcMonitoredItem)sender;

            SubscribeChanged.Add($"Data Change from Index {item.Tag} to {e.Item.Value}");

        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "SubcribeChange")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}