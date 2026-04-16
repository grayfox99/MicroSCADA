// MicroSCADA OPC UA Simulator
// Standalone test server exposing simulated PLC tags.
//
// Usage:
//   node server.js                     # default port 4840
//   node server.js --port 48400        # custom port
//   node server.js --channel Plant1 --device PLC1
//
// Tags are exposed as: ns=2;s={channel}.{device}.{tagName}

const path = require("path");
const {
    OPCUAServer,
    DataType,
    Variant,
    StatusCodes,
    MessageSecurityMode,
    SecurityPolicy,
    OPCUACertificateManager,
} = require("node-opcua");

function parseArgs() {
    const args = process.argv.slice(2);
    const config = {
        port: 4840,
        channel: "PLC_Tags",
        device: "Demo_Device",
    };
    for (let i = 0; i < args.length; i++) {
        switch (args[i]) {
            case "--port":
                config.port = parseInt(args[++i], 10);
                break;
            case "--channel":
                config.channel = args[++i];
                break;
            case "--device":
                config.device = args[++i];
                break;
        }
    }
    return config;
}

function buildTagDefs() {
    return [
        {
            name: "Pressure_psi",
            desc: "Vessel pressure",
            unit: "psi",
            dataType: DataType.Float,
            initial: 100.0,
            simulate: (t, v) => clamp(100 + Math.sin(t / 40) * 30 + noise(2), 50, 200),
        },
        {
            name: "Temperature_F",
            desc: "Process temperature",
            unit: "°F",
            dataType: DataType.Float,
            initial: 250.0,
            simulate: (t, v) => clamp(250 + Math.sin(t / 60) * 50 + noise(3), 100, 400),
        },
        {
            name: "FlowRate_GPM",
            desc: "Flow rate",
            unit: "GPM",
            dataType: DataType.Float,
            initial: 75.0,
            simulate: (t, v) => clamp(75 + Math.sin(t / 35) * 25 + noise(2), 0, 150),
        },
        {
            name: "Level_pct",
            desc: "Tank level",
            unit: "%",
            dataType: DataType.Double,
            initial: 60.0,
            simulate: (t, v) => clamp(60 + Math.sin(t / 80) * 30 + noise(1), 0, 100),
        },
        {
            name: "MotorSpeed_RPM",
            desc: "Motor speed",
            unit: "RPM",
            dataType: DataType.Float,
            initial: 1750.0,
            simulate: (t, v) => clamp(1750 + Math.sin(t / 50) * 200 + noise(15), 0, 3000),
        },
        {
            name: "Voltage_V",
            desc: "Bus voltage",
            unit: "V",
            dataType: DataType.Float,
            initial: 480.0,
            simulate: (t, v) => clamp(480 + Math.sin(t / 90) * 10 + noise(0.5), 440, 520),
        },
        {
            name: "Current_A",
            desc: "Motor current",
            unit: "A",
            dataType: DataType.Float,
            initial: 25.0,
            simulate: (t, v) => clamp(25 + Math.sin(t / 45) * 8 + noise(0.4), 0, 50),
        },
        {
            name: "ValvePosition_pct",
            desc: "Valve position",
            unit: "%",
            dataType: DataType.Float,
            initial: 50.0,
            simulate: (t, v) => clamp(50 + Math.sin(t / 30) * 40 + noise(1), 0, 100),
        },
        {
            name: "Power_kW",
            desc: "Active power",
            unit: "kW",
            dataType: DataType.Float,
            initial: 120.0,
            simulate: (t, v) => clamp(120 + Math.sin(t / 70) * 40 + noise(3), 0, 250),
        },
        {
            name: "Status_Run",
            desc: "Run status",
            unit: "",
            dataType: DataType.Boolean,
            initial: true,
            simulate: (t, v) => v,
        },
        {
            name: "Status_Fault",
            desc: "Fault status",
            unit: "",
            dataType: DataType.Boolean,
            initial: false,
            simulate: (t, v) => v,
        },
        {
            name: "Setpoint",
            desc: "Setpoint (writable)",
            unit: "",
            dataType: DataType.Float,
            initial: 1.0,
            simulate: (t, v) => v,
        },
    ];
}

function noise(amplitude) {
    return (Math.random() - 0.5) * 2 * amplitude;
}

function clamp(value, min, max) {
    return Math.min(max, Math.max(min, value));
}

(async () => {
    const config = parseArgs();
    const prefix = `${config.channel}.${config.device}`;

    console.log("=".repeat(60));
    console.log("  MicroSCADA OPC UA Simulator");
    console.log("=".repeat(60));
    console.log(`  Port:    ${config.port}`);
    console.log(`  Channel: ${config.channel}`);
    console.log(`  Device:  ${config.device}`);
    console.log(`  Tags:    ns=2;s=${prefix}.<TagName>`);
    console.log("=".repeat(60));
    console.log();

    const pkiDir = path.join(__dirname, "pki");
    const server = new OPCUAServer({
        port: config.port,
        resourcePath: "/UA/MicroSCADA",
        allowAnonymous: true,
        securityModes: [MessageSecurityMode.None],
        securityPolicies: [SecurityPolicy.None],
        serverCertificateManager: new OPCUACertificateManager({
            rootFolder: pkiDir,
            automaticallyAcceptUnknownCertificate: true,
        }),
    });

    await server.initialize();

    const addressSpace = server.engine.addressSpace;
    const ns = addressSpace.registerNamespace("urn:MicroSCADA:Simulator");

    const channelFolder = ns.addObject({
        organizedBy: addressSpace.rootFolder.objects,
        browseName: config.channel,
    });

    const deviceFolder = ns.addObject({
        componentOf: channelFolder,
        browseName: config.device,
    });

    const tagDefs = buildTagDefs();
    const tagState = {};

    for (const def of tagDefs) {
        const nodeId = `ns=2;s=${prefix}.${def.name}`;
        tagState[def.name] = { value: def.initial };

        const uaVar = ns.addVariable({
            componentOf: deviceFolder,
            browseName: def.name,
            nodeId: nodeId,
            dataType: def.dataType,
            description: def.desc,
            minimumSamplingInterval: 1000,
            value: {
                get: () =>
                    new Variant({
                        dataType: def.dataType,
                        value: tagState[def.name].value,
                    }),
                set: (variant) => {
                    tagState[def.name].value = variant.value;
                    return StatusCodes.Good;
                },
            },
        });

        tagState[def.name].uaVariable = uaVar;
    }

    let tick = 0;
    const simInterval = setInterval(() => {
        tick++;
        for (const def of tagDefs) {
            const state = tagState[def.name];
            const newValue = def.simulate(tick, state.value);
            state.value = newValue;
            state.uaVariable.setValueFromSource(
                new Variant({ dataType: def.dataType, value: newValue })
            );
        }
    }, 1000);

    await server.start();

    const endpoint = server.endpoints[0].endpointDescriptions()[0].endpointUrl;
    console.log(`Server started: ${endpoint}`);
    console.log();
    console.log("Available tags:");
    for (const def of tagDefs) {
        const nodeId = `ns=2;s=${prefix}.${def.name}`;
        const typeLabel = def.unit || (def.dataType === DataType.Boolean ? "Boolean" : "Float");
        console.log(`  ${nodeId.padEnd(55)} ${def.desc} (${typeLabel})`);
    }
    console.log();
    console.log("Press CTRL+C to stop.");

    process.on("SIGINT", async () => {
        console.log("\nShutting down...");
        clearInterval(simInterval);
        await server.shutdown();
        process.exit(0);
    });
})();
