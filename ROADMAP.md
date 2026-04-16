# Roadmap

Stuff I want to get to when I have the time. Nothing is committed — this is a scratchpad for ideas, not a promise of delivery.

## Recently shipped (v2.0)

- .NET 8 + OPC Foundation stack migration
- Dark mode by default + light/dark toggle
- Subscription panel rewrite (stable rows, 3 sig figs, locked column widths)
- Bundled node-opcua simulator under `tools/`

## Next up (things I want to hit first)

### Live charts
- Line chart(s) for subscribed values, probably using MudBlazor's `MudChart` first since it's already in the stack. If it's too limited I'll look at ApexCharts.Blazor.
- Ring buffer per tag, ~60–300 samples, redraw on each subscription tick
- Time window picker (30s / 1m / 5m / 15m)
- Multiple tags on one chart, colored per tag

### Connection diagnostics
- Status pill in the app bar (connected / reconnecting / disconnected)
- Keep-alive latency, missed keep-alive count
- Subscription stats — monitored items, publish interval, last notification timestamp
- Last error surfaced somewhere I don't have to dig into the console for
- Hook into `Session.KeepAlive` and `Session.Notification` events for this

### Writes
- Sim already exposes a writable `Setpoint` tag, so I can test this end-to-end without adding anything
- Inline edit on the subscription table, or a right-click menu

### Async migration
- OPC Foundation marks a bunch of sync methods obsolete (`Session.Create`, `Browse`, `ReadValue`, `Subscription.Create`/`Delete`, `ApplicationConfiguration.Validate`, `CoreClientUtils.SelectEndpoint`). Build is currently 1 warning because of this. Swap to the `*Async` versions.

### Security policy picker
- Currently hardcoded to `useSecurity: false` (anonymous / None). Real plant servers won't accept that.
- Dropdown for security policy + message mode, plus username/password auth

## Later on

### Node browser polish
- Search/filter by name within the tree
- Show node metadata on click (data type, access level, description)
- Right-click context menu for read / write / subscribe

### Data grid view
- `MudDataGrid` instead of (or alongside) the current subscription table — sorting, filtering, column config
- CSV export of whatever's in the grid

### Saved connections
- Remember recent endpoints, maybe favorite them
- Just local storage or a small JSON file next to the app

### Alarms & events
- Subscribe to OPC UA alarms/conditions
- List view with severity, acknowledge/confirm
- Notification badge, optional sound

### Historical data
- Read history from servers that support Historical Access
- Time range picker
- Overlay on the live chart so you can see past + present

## Way out

### Multi-server
- Connect to more than one server at a time
- Tabbed or split pane per server
- Cross-server tag comparison

### Dashboard builder
- Drag-and-drop widgets (charts, gauges, tables, indicators)
- Saved layouts
- Fullscreen / kiosk mode for control room screens

### Deployment & security
- Dockerfile + env-configured endpoints
- Role-based access (viewer / operator / admin)
- Audit log for writes and config changes
- Proper HTTPS guidance — the dev-cert workaround currently in the README is fine for local but not for deploy
