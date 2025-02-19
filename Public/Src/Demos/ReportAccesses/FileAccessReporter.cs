// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BuildXL.Processes;
using BuildXL.Utilities.Core;
using BuildXL.Utilities.Instrumentation.Common;
using Microsoft.Win32.SafeHandles;

namespace BuildXL.Demo
{
    public class DemoDetoursEventListener : IDetoursEventListener
    {
        private StreamWriter outputFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoDetoursEventListener"/> class.
        /// </summary>
        public DemoDetoursEventListener(StreamWriter outputFile)
        {
            this.outputFile = outputFile;
            SetMessageHandlingFlags(MessageHandlingFlags.FileAccessNotify);
        }

        /// <inheritdoc />
        public override void HandleFileAccess(FileAccessData fileAccessData)
        {
            if (fileAccessData.RequestedAccess != RequestedAccess.None)
            {
                var requestedAccessPrefix = fileAccessData.RequestedAccess switch
                {
                    RequestedAccess.Write => "W",
                    RequestedAccess.Enumerate => "E",
                    _ => "R",
                };
                outputFile.WriteLine($"{requestedAccessPrefix} {fileAccessData.Path}");
                outputFile.Flush();
            }
        }

        /// <inheritdoc />
        public override void HandleDebugMessage(DebugData debugData)
        {
        }

        /// <inheritdoc />
        public override void HandleProcessData(ProcessData processData)
        {
        }

        /// <inheritdoc />
        public override void HandleProcessDetouringStatus(ProcessDetouringStatusData data)
        {
        }
    }

    /// <summary>
    /// A very simplistic use of BuildXL sandbox, just meant for observing file accesses
    /// </summary>
    public class FileAccessReporter : ISandboxedProcessFileStorage
    {
        private readonly LoggingContext m_loggingContext;

        /// <nodoc/>
        public PathTable PathTable { get; }

        /// <nodoc/>
        public FileAccessReporter()
        {
            PathTable = new PathTable();
            m_loggingContext = new LoggingContext(nameof(FileAccessReporter));
        }

        /// <summary>
        /// Runs the given tool with the provided arguments under the BuildXL sandbox and reports the result in a <see cref="SandboxedProcessResult"/>
        /// </summary>
        public async Task<SandboxedProcessResult> RunProcessUnderSandbox(string pathToProcess, string arguments)
        {

            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (pathEnv == null)
            {
                return null;
            }

            string[] paths = pathEnv.Split(Path.PathSeparator);
            string executablePath = null;
            foreach (string path in paths)
            {
                string fullPath = Path.Combine(path, pathToProcess);
                if (File.Exists(fullPath))
                {
                    executablePath = fullPath;
                }
            }
            if (executablePath == null)
            {
                Console.WriteLine($"Could not find {pathToProcess} in PATH");
                return null;
            }


            using (FileStream stream = new FileStream(new SafeFileHandle((IntPtr)3, false), FileAccess.Write))
            {
                using (StreamWriter outputFile = new StreamWriter(stream))
                {
                    var envVars = BuildParameters
                            .GetFactory()
                            .PopulateFromEnvironment();
                    var info = new SandboxedProcessInfo(
                        PathTable,
                        this,
                        executablePath,
                        CreateManifestToAllowAllAccesses(PathTable),
                        disableConHostSharing: false,
                        loggingContext: m_loggingContext,
                        detoursEventListener: new DemoDetoursEventListener(outputFile))
                    {
                        Arguments = arguments,
                        WorkingDirectory = Directory.GetCurrentDirectory(),
                        EnvironmentVariables = envVars,
                        PipSemiStableHash = 0,
                        PipDescription = "Simple sandbox demo",
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardOutputObserver = stdOutStr => Console.WriteLine(stdOutStr),

                        StandardErrorEncoding = Encoding.UTF8,
                        StandardErrorObserver = stdErrStr => Console.Error.WriteLine(stdErrStr),
                    };

                    info.SandboxConnection = new SandboxConnectionLinuxDetours((int status, string description) =>
                    {
                        info.SandboxConnection?.Dispose();
                    });

                    var process = SandboxedProcessFactory.StartAsync(info, forceSandboxing: true).GetAwaiter().GetResult();

                    return await process.GetResultAsync();
                }
            }

        }

        /// <nodoc />
        string ISandboxedProcessFileStorage.GetFileName(SandboxedProcessFile file)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), file.DefaultFileName());
        }

        /// <summary>
        /// The manifest is configured so all file accesses are allowed but reported, including child processes.
        /// </summary>
        /// <remarks>
        /// Some special folders (Windows, InternetCache and History) are added as known scopes. Everything else will be flagged
        /// as an 'unexpected' access. However, unexpected accesses are configured so they are not blocked.
        /// </remarks>
        private static FileAccessManifest CreateManifestToAllowAllAccesses(PathTable pathTable)
        {
            var fileAccessManifest = new FileAccessManifest(pathTable)
            {
                FailUnexpectedFileAccesses = false,
                ReportFileAccesses = true,
                MonitorChildProcesses = true,
                EnableLinuxPTraceSandbox = true,
                PipId = 1,
                ExplicitlyReportDirectoryProbes = true,
                ProbeDirectorySymlinkAsDirectory = true,
            };

            fileAccessManifest.AddScope(AbsolutePath.Invalid, FileAccessPolicy.MaskNothing, FileAccessPolicy.MaskNothing);

            return fileAccessManifest;
        }
    }
}
