const child_process = require("child_process");

const BXL_TRACER = `/workspaces/BuildXL/Out/Bin/Demos/debug/net8.0linux-x64/ReportAccesses`;

const traceLines = [];

async function runWithTracing(command) {
    const child = child_process.spawn(BXL_TRACER, command, {
        shell: false,
        env: process.env,
        stdio: ["pipe", "pipe", "pipe", "pipe"],
    });

    const stream = child.stdio[3];

    stream.setEncoding("utf8");

    let remaining = "";

    stream.on("data", (chunk) => {
        console.log(`TRACE: ${chunk}`);
        if (!chunk) {
            return;
        }

        remaining += chunk;
        let startIndex = 0;
        let endIndex = remaining.indexOf("\n");
        while (endIndex > 0) {
            const line = remaining.slice(startIndex, endIndex);
            traceLines.push(line);

            startIndex = endIndex + 1;
            endIndex = remaining.indexOf("\n", startIndex);
        }

        if (startIndex > 0) {
            remaining = remaining.slice(startIndex);
        }
    });

    await new Promise((resolve) => {
        child.on("exit", () => {
            resolve();
        });
    });
}

runWithTracing(["touch", "/tmp/foo"]).then(() => {
    console.log(`TRACE RESULT`);
    console.log(traceLines.join("\n"));
});

