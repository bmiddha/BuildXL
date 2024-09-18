// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using BuildXL.Demo;
using BuildXL.Processes;
using BuildXL.Utilities.Core;

namespace BuildXL.SandboxDemo
{
    /// <summary>
    /// An arbitrary process can be run under the BuildXL Sandbox and all files accesses of itself and its 
    /// child processes are reported
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Expected arguments: 
        /// - args[0]: path to the process to be executed under the sandbox
        /// - args[1..n]: optional arguments that are passed to the process 'as is'
        /// </summary>
        public static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintUsage();
                return 1;
            }

            var tool = args[0];
            var arguments = string.Join(" ", args.Skip(1));

            var fileAccessReporter = new FileAccessReporter();
            var result = fileAccessReporter.RunProcessUnderSandbox(tool, arguments).GetAwaiter().GetResult();

            return result.ExitCode;
        }

        private static void PrintUsage()
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            Console.WriteLine($"{processName} <pathToTool> [<arguments>]");
        }
    }
}