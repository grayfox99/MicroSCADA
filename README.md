# MicroSCADA

A small SCADA-style OPC UA client I built with Blazor Server and the OPC Foundation UA .NET Standard stack. Connect to an OPC UA server, browse its node tree, and subscribe to live tag values.

#### Video Demo: https://youtu.be/wmz7Coc42Pw

## What it does

- Connect to any OPC UA server by endpoint URL
- Browse the node tree with lazy expansion
- Multi-select tags and subscribe to live value updates
- One stable row per subscribed tag with live value + timestamp. Values are locked to 3 sig figs so the column doesn't jitter while values update
- Dark/light theme toggle in the app bar (dark by default)
- Ships with a standalone node-opcua simulator under `tools/` so you can poke at the app without hunting down an external server

## Stack

- .NET 8 Blazor Server
- MudBlazor 9
- OPC Foundation UA .NET Standard 1.5.x
- node-opcua for the test simulator

## Layout

```
src/
├── MicroSCADA/              # CLI browser — connect + dump the node tree
└── MicroSCADA_Client/       # Blazor web client
    ├── Models/OpcNode.cs
    ├── Services/            # IOpcUaService + impl
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

- `IOpcUaService` / `OpcUaService` do the OPC work: connect, browse, subscribe, disconnect
- `Index.razor` consumes it via DI. `MudTreeView` lazy-loads children via its `ServerData` hook. The subscription panel renders as a table keyed by NodeId so rows stay put — only the value and timestamp refresh on data change
- Endpoint selection uses `CoreClientUtils.SelectEndpoint(config, url, useSecurity: false)` so we negotiate whatever the server actually offers (most dev servers are anonymous / None). Constructing `EndpointDescription` by hand will throw `[80210000] Endpoint does not support the user identity type provided` on any server that isn't using the default secured profile
- Data change callbacks from OPC subscriptions get marshaled to the UI thread with `InvokeAsync`

## Credits

- [OPC Foundation](https://opcfoundation.org/) — UA .NET Standard stack
- [MudBlazor](https://mudblazor.com/)
- [node-opcua](https://github.com/node-opcua/node-opcua) — the bundled simulator is built on their server library
