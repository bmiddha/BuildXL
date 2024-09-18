// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import * as Deployment from "Sdk.Deployment";
import * as Managed from "Sdk.Managed";

namespace Deployment {
    
    const deployment: Deployment.Definition = {
        contents: [
            ReportAccesses.exe
        ]
    };

    const frameworkSpecificPart = BuildXLSdk.isDotNetCoreOrStandard
        ? qualifier.targetFramework + qualifier.targetRuntime
        : qualifier.targetFramework;

    @@public
    export const deployed = BuildXLSdk.DeploymentHelpers.deploy({
        definition: deployment,
        targetLocation: r`Demos/${qualifier.configuration}/${frameworkSpecificPart}`,
        omitFromDrop: true,
    });
}
