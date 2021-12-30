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
}
