# MicroSCADA

A small SCADA-style OPC UA client I built with Blazor Server and the OPC Foundation UA .NET Standard stack. Connect to an OPC UA server, browse its node tree, and subscribe to live tag values.

## What it does

- Connect to any OPC UA server by endpoint URL
- Browse the node tree with lazy expansion — leaf tags are colored green when the initial read succeeds, red when it doesn't, so unsubscribable nodes stand out
- Multi-select tags and subscribe to live value updates
- One stable row per subscribed tag with live value + timestamp. Values are locked to 3 sig figs so the column doesn't jitter while values update
- Live chart card with window picker (30s / 1m / 5m / 15m), up to 4 tags plotted at once from a per-tag ring buffer, each chip keyed to a stable color
- Diagnostics card showing session state, endpoint, security policy, uptime, monitored-item count, publish interval, keep-alive timing and miss count, and the last surfaced error
- Connection status pill in the app bar that follows session state across pages
- Dark/light theme toggle in the app bar (dark by default) — chart adapts to the active palette via CSS variables
- Ships with a standalone node-opcua simulator under `tools/` so you can poke at the app without hunting down an external server

## Stack

- .NET 8 Blazor Server
- MudBlazor 9
- Blazor-ApexCharts 6 for the live chart
- OPC Foundation UA .NET Standard 1.5.x
- node-opcua for the test simulator

## Layout

```
src/
├── MicroSCADA/              # CLI browser — connect + dump the node tree
└── MicroSCADA_Client/       # Blazor web client
    ├── Models/OpcNode.cs
    ├── Services/
    │   ├── IOpcUaService + impl      # connect, browse, subscribe, keep-alive hook
    │   ├── ITagHistoryService + impl # per-tag ring buffer, 1 Hz redraw tick
    │   ├── IDiagnosticsService + impl # 1 Hz snapshot of session health
    │   └── RingBuffer.cs
    ├── Pages/Index.razor
    ├── Shared/MainLayout.razor
    ├── App.razor, Routes.razor, Program.cs
tools/
└── opcua-simulator/         # node-opcua test server with simulated PLC tags
```

## Running it

### Prereqs

- .NET 8 SDK
- Node 18+ (only if you want to run the bundled simulator)

### Fire up the simulator

```bash
cd tools/opcua-simulator
npm install
node server.js --port 4840
```

Exposes a dozen simulated PLC tags (pressure, temperature, flow, motor speed, voltage, current, etc.) at `opc.tcp://localhost:4840/UA/MicroSCADA`.

### Run the web client

```bash
cd src/MicroSCADA_Client
dotnet run
```

Then open `https://localhost:7145`, paste your endpoint, click **Connect**.

If `dotnet dev-certs https` won't generate a cert on your machine (common on macOS), run HTTP-only instead:

```bash
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="http://localhost:5199" \
    dotnet run --no-launch-profile
```

### Run the CLI browser

```bash
cd src/MicroSCADA
dotnet run -- "opc.tcp://your-server:port/path"
```

Connects, browses 4 levels deep, prints everything to stdout.

## How it's wired

- All three services (`IOpcUaService`, `ITagHistoryService`, `IDiagnosticsService`) are registered as Singletons so the OPC session, tag history, and diagnostic state survive tab close / reload. `OpcUaService` guards its mutating paths with a `SemaphoreSlim`; `BrowseAsync` captures the session reference locally so reads don't need the lock
- History and diagnostics are eagerly constructed at startup — they attach to `OpcUaService` events before any user hits Connect, so early samples and keep-alive events aren't lost
- `TagHistoryService` parses values to `double`, drops non-numeric ones silently, and coalesces redraw notifications through a 1 Hz timer so the chart doesn't re-render once per subscription tick
- `DiagnosticsService` polls the OPC service at 1 Hz, builds an immutable `DiagnosticsSnapshot`, and emits it via an `Updated` event. It never sees raw `Opc.Ua` types so the diagnostics pipeline stays decoupled from the OPC stack
- `Index.razor` consumes everything via DI. `MudTreeView` lazy-loads children via its `ServerData` hook. The subscription panel renders as a table keyed by NodeId so rows stay put — only the value and timestamp refresh on data change
- ApexCharts options use MudBlazor CSS variables (`--mud-palette-text-primary`, `--mud-palette-lines-default`) for `foreColor` / grid lines, which lets the chart follow the MudThemeProvider palette without a separate theme-state service
- Endpoint selection uses `CoreClientUtils.SelectEndpoint(config, url, useSecurity: false)` so we negotiate whatever the server actually offers (most dev servers are anonymous / None). Constructing `EndpointDescription` by hand will throw `[80210000] Endpoint does not support the user identity type provided` on any server that isn't using the default secured profile
- Data change callbacks from OPC subscriptions get marshaled to the UI thread with `InvokeAsync`

## Credits

- [OPC Foundation](https://opcfoundation.org/) — UA .NET Standard stack
- [MudBlazor](https://mudblazor.com/)
- [Blazor-ApexCharts](https://github.com/apexcharts/Blazor-ApexCharts)
- [node-opcua](https://github.com/node-opcua/node-opcua) — the bundled simulator is built on their server library
