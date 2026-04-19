using MicroSCADA_Client.Models;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace MicroSCADA_Client.Services;

public class OpcUaService : IOpcUaService
{
    private Opc.Ua.Client.ISession? _session;
    private readonly List<Subscription> _subscriptions = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public bool IsConnected => _session?.Connected ?? false;
    public event Action<string, string>? DataChanged;

    public async Task ConnectAsync(string endpointUrl)
    {
        await _lock.WaitAsync();
        try
        {
            if (_session?.Connected == true)
                await DisconnectCoreAsync();

            var config = new ApplicationConfiguration
            {
                ApplicationName = "MicroSCADA",
                ApplicationUri = Utils.Format(@"urn:{0}:MicroSCADA", System.Net.Dns.GetHostName()),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = CertificateStoreType.Directory,
                        StorePath = Path.Combine(Directory.GetCurrentDirectory(), "pki", "own"),
                        SubjectName = "CN=MicroSCADA, O=MicroSCADA"
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

            _session = await new DefaultSessionFactory(null!).CreateAsync(
                config,
                endpoint,
                false,
                "MicroSCADA Session",
                60000,
                new UserIdentity(new AnonymousIdentityToken()),
                null
            );
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DisconnectAsync()
    {
        await _lock.WaitAsync();
        try
        {
            await DisconnectCoreAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task DisconnectCoreAsync()
    {
        foreach (var sub in _subscriptions)
        {
            try { await sub.DeleteAsync(true); } catch { }
        }
        _subscriptions.Clear();

        if (_session != null)
        {
            await _session.CloseAsync();
            _session.Dispose();
            _session = null;
        }
    }

    public async Task<List<OpcNode>> BrowseAsync(string? nodeId = null)
    {
        var session = _session
            ?? throw new InvalidOperationException("Not connected to OPC UA server.");

        var startNodeId = string.IsNullOrEmpty(nodeId)
            ? ObjectIds.ObjectsFolder
            : NodeId.Parse(nodeId);

        var (_, _, references) = await session.BrowseAsync(
            null,
            null,
            startNodeId,
            0u,
            BrowseDirection.Forward,
            ReferenceTypeIds.HierarchicalReferences,
            true,
            0
        );
        var nodes = new List<OpcNode>();
        if (references == null)
            return nodes;

        foreach (var reference in references)
        {
            string? value = null;
            bool hasChildren = reference.NodeClass == NodeClass.Object;

            if (reference.NodeClass == NodeClass.Variable)
            {
                try
                {
                    var dataValue = await session.ReadValueAsync((NodeId)reference.NodeId);
                    value = dataValue?.ToString();
                }
                catch { }
            }

            nodes.Add(new OpcNode
            {
                NodeId = reference.NodeId.ToString(),
                DisplayName = reference.DisplayName?.Text ?? reference.BrowseName.Name,
                Value = value,
                HasChildren = hasChildren
            });
        }

        return nodes;
    }

    public async Task SubscribeAsync(IEnumerable<string> nodeIds)
    {
        await _lock.WaitAsync();
        try
        {
            if (_session == null)
                throw new InvalidOperationException("Not connected to OPC UA server.");

            var subscription = new Subscription(_session.DefaultSubscription)
            {
                DisplayName = "MicroSCADA Subscription",
                PublishingInterval = 1000,
                KeepAliveCount = 10,
                LifetimeCount = 30
            };

            foreach (var nodeId in nodeIds)
            {
                var item = new MonitoredItem(subscription.DefaultItem)
                {
                    DisplayName = nodeId,
                    StartNodeId = NodeId.Parse(nodeId),
                    AttributeId = Attributes.Value,
                    SamplingInterval = 500
                };
                item.Notification += OnMonitoredItemNotification;
                subscription.AddItem(item);
            }

            _session.AddSubscription(subscription);
            await subscription.CreateAsync();
            _subscriptions.Add(subscription);
        }
        finally
        {
            _lock.Release();
        }
    }

    private void OnMonitoredItemNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
    {
        if (e.NotificationValue is MonitoredItemNotification notification)
        {
            DataChanged?.Invoke(
                item.StartNodeId.ToString(),
                notification.Value?.WrappedValue.ToString() ?? "null"
            );
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _lock.Dispose();
    }
}
