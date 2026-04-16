# MicroSCADA Roadmap

Future improvements and feature ideas, roughly prioritized.

## Near-Term

### Live Data Visualization
- Add real-time line charts for subscribed tag values using MudBlazor's `MudChart` or a library like ApexCharts.Blazor
- Configurable time window (last 30s, 1m, 5m, 15m)
- Multiple tags on the same chart with color coding

### Connection Management
- Save/load favorite server endpoints (local storage or config file)
- Display server status info (server name, build info, current time, state)
- Support authenticated connections (username/password, certificate-based)
- Connection timeout and automatic reconnection handling

### Node Browser Enhancements
- Search/filter nodes by name within the tree
- Show node metadata on click (data type, access level, description, timestamp)
- Right-click context menu for node operations (read, write, subscribe)
- Write support for writable variable nodes

## Mid-Term

### Data Grid View
- `MudDataGrid` for tabular display of subscribed values with sorting and filtering
- Export subscription data to CSV
- Configurable column display (node ID, display name, value, timestamp, status)

### Alarm & Event Monitoring
- Subscribe to OPC UA alarms and conditions
- Alarm list with severity indicators, acknowledge/confirm actions
- Notification badges and optional sound alerts

### Historical Data
- Read historical data from OPC UA servers that support the Historical Access service set
- Time-range picker for historical queries
- Overlay historical trends on live charts

## Long-Term

### Multi-Server Support
- Connect to multiple OPC UA servers simultaneously
- Tabbed or split-pane interface per server
- Cross-server tag comparison views

### Dashboard Builder
- Drag-and-drop dashboard with customizable widgets (charts, gauges, indicators, tables)
- Save/load dashboard layouts per user
- Full-screen kiosk mode for control room displays

### Deployment & Security
- Docker container support with configurable endpoints via environment variables
- Role-based access control (viewer, operator, admin)
- Audit logging for write operations and configuration changes
- HTTPS certificate configuration guidance for production deployments
