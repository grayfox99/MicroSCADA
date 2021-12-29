// See https://aka.ms/new-console-template for more information
using Opc.UaFx;
using Opc.UaFx.Client;


var client = new OpcClient("opc.tcp://laptop-jvk86rqt:51210/UA/SampleServer");
client.Security.EndpointPolicy = new OpcSecurityPolicy(
        OpcSecurityMode.Sign, OpcSecurityAlgorithm.Basic256Sha256);

var certificate = OpcCertificateManager.CreateCertificate(client);
client.Certificate = certificate;

client.Connect();

var node = client.BrowseNode(OpcObjectTypes.ObjectsFolder);

BrowseNodes browseNodes = new BrowseNodes();
browseNodes.Browse(node);

class BrowseNodes
{
    public List<OpcNodeInfo> opcNodes = new List<OpcNodeInfo>();
    public int nodeCount = 0;
    public void Browse(OpcNodeInfo node, int level = 0)
    {
        try
        {
            while (level < 4)
            {

                //opcNodes.Append(node);
                //node = (OpcNodeInfo)opcNodes.TakeLast(1);
                Console.WriteLine("{0}{1}: {2} ({3})",
                        new string('_', level * 4),
                        node.Attribute(OpcAttribute.DisplayName).Value,
                        node.Attribute(OpcAttribute.Value)?.Value,
                        node.NodeId);
                level++;


                foreach (var childNode in node.Children())
                {
                    Browse(childNode, level);
                    
                }

                //Console.ReadLine();
            }
        }
        catch (Exception ex)
        { 
            Console.WriteLine("\n\n"+ex.ToString()+"\n\n"); 
        }

        


        //foreach (var childNode in node.Children())
        //    Browse(childNode, level);
    }
}