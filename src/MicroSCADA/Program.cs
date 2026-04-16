using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

var endpointUrl = args.Length > 0
    ? args[0]
    : "opc.tcp://localhost:51210/UA/SampleServer";

Console.WriteLine($"Connecting to: {endpointUrl}");

var config = new ApplicationConfiguration
{
    ApplicationName = "MicroSCADA Console",
    ApplicationUri = Utils.Format(@"urn:{0}:MicroSCADA:Console", System.Net.Dns.GetHostName()),
    ApplicationType = ApplicationType.Client,
    SecurityConfiguration = new SecurityConfiguration
    {
        ApplicationCertificate = new CertificateIdentifier
        {
            StoreType = CertificateStoreType.Directory,
            StorePath = Path.Combine(Directory.GetCurrentDirectory(), "pki", "own"),
            SubjectName = "CN=MicroSCADA Console"
        },
        TrustedIssuerCertificates = new CertificateTrustList
        {
            StoreType = CertificateStoreType.Directory,
            StorePath = Path.Combine(Directory.GetCurrentDirectory(), "pki", "issuer")
        },
        TrustedPeerCertificates = new CertificateTrustList
        {
            StoreType = CertificateStoreType.Directory,
            StorePath = Path.Combine(Directory.GetCurrentDirectory(), "pki", "trusted")
        },
        RejectedCertificateStore = new CertificateTrustList
        {
            StoreType = CertificateStoreType.Directory,
            StorePath = Path.Combine(Directory.GetCurrentDirectory(), "pki", "rejected")
        },
        AutoAcceptUntrustedCertificates = true
    },
    TransportConfigurations = new TransportConfigurationCollection(),
    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 }
};

await config.ValidateAsync(ApplicationType.Client);

config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = true; };

var endpointConfiguration = EndpointConfiguration.Create(config);
var selectedEndpoint = CoreClientUtils.SelectEndpoint(config, endpointUrl, useSecurity: false);
var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

try
{
    using var session = await new DefaultSessionFactory(null!).CreateAsync(
        config,
        endpoint,
        false,
        "MicroSCADA Console",
        60000,
        new UserIdentity(new AnonymousIdentityToken()),
        null
    ) as Session ?? throw new InvalidOperationException("Failed to create session.");

    Console.WriteLine("Connected successfully.\n");

    await BrowseAsync(session, ObjectIds.ObjectsFolder, 0);

    Console.WriteLine("\nPress Enter to disconnect...");
    Console.ReadLine();
    await session.CloseAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

static async Task BrowseAsync(Opc.Ua.Client.ISession session, NodeId nodeId, int level, int maxLevel = 4)
{
    if (level >= maxLevel) return;

    var (_, _, references) = await session.BrowseAsync(
        null, null, nodeId, 0u,
        BrowseDirection.Forward,
        ReferenceTypeIds.HierarchicalReferences,
        true, 0
    );

    if (references == null) return;

    foreach (var reference in references)
    {
        string? value = null;
        if (reference.NodeClass == NodeClass.Variable)
        {
            try
            {
                var dataValue = await session.ReadValueAsync((NodeId)reference.NodeId);
                value = dataValue?.ToString();
            }
            catch { }
        }

        Console.WriteLine("{0}{1}: {2} ({3})",
            new string(' ', level * 4),
            reference.DisplayName,
            value,
            reference.NodeId);

        await BrowseAsync(session, (NodeId)reference.NodeId, level + 1, maxLevel);
    }
}
