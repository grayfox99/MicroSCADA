namespace MicroSCADA_Client.Models;

public class OpcNode
{
    public string NodeId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? Value { get; set; }
    public bool HasChildren { get; set; }
}
