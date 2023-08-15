﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace BuildXL.Cache.BlobLifetimeManager.Library
{
    public class BlobQuotaKeeperConfig
    {
        public required TimeSpan LastAccessTimeDeletionThreshold { get; init; } 
        public required List<GarbageCollectionNamespaceConfig> Namespaces { get; init; }
        public double LruEnumerationPercentileStep { get; init; } = 0.05;
        public int LruEnumerationBatchSize { get; init; } = 1000;
    }

    public record GarbageCollectionNamespaceConfig(string Universe, string Namespace, double MaxSizeGb);
}
