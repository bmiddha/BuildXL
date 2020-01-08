// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import * as Managed from "Sdk.Managed";
import * as Deployment from "Sdk.Deployment";

export declare const qualifier: {
    targetFramework: "netcoreapp3.1" | "net472";
    targetRuntime: "win-x64" | "osx-x64";
};

const nativePackage = importFrom("RocksDbNative").pkg;
const managedPackage = importFrom("RocksDbSharpSigned").pkg;

@@public
export const pkg = managedPackage.override<Managed.ManagedNugetPackage>({
    name: "RocksDbSharp",
    version: nativePackage.version,
    runtimeContent: {
        contents: [ <Deployment.NestedDefinition>{
            subfolder: r`native`,
            contents: [ Deployment.createFromFilteredStaticDirectory(nativePackage.contents, r`build/native`) ] }
        ]
    },
});
