// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using BuildXL.Utilities.Core;

namespace BuildXL.Scheduler.Fingerprints
{
    /// <summary>
    /// State bag used when processing observed inputs after execution and during caching
    /// </summary>
    internal class ObservedInputProcessingState : IDisposable
    {
        private static readonly ObjectPool<ObservedInputProcessingState> s_pool = new ObjectPool<ObservedInputProcessingState>(
            () => new ObservedInputProcessingState(),
            state => state.Clear());

        // NOTE: Be sure to add any collections created here to Clear
        public readonly Dictionary<AbsolutePath, ObservedInputType> AllowedUndeclaredReads = new Dictionary<AbsolutePath, ObservedInputType>();
        public readonly List<(AbsolutePath path, DynamicObservationKind observationType)> DynamicObservations = new List<(AbsolutePath, DynamicObservationKind)>();
        public readonly List<SourceSealWithPatterns> SourceDirectoriesAllDirectories = new List<SourceSealWithPatterns>();
        public readonly List<SourceSealWithPatterns> SourceDirectoriesTopDirectoryOnly = new List<SourceSealWithPatterns>();
        public readonly HashSet<AbsolutePath> ObservationArtifacts = new HashSet<AbsolutePath>();
        public readonly HashSet<AbsolutePath> DirectoryDependencyContentsFilePaths = new HashSet<AbsolutePath>();
        public readonly Dictionary<AbsolutePath, (DirectoryMembershipFilter, DirectoryEnumerationMode)> EnumeratedDirectories = new Dictionary<AbsolutePath, (DirectoryMembershipFilter, DirectoryEnumerationMode)>();
        public readonly HashSet<HierarchicalNameId> AllDependencyPathIds = new HashSet<HierarchicalNameId>();
        public readonly HashSet<AbsolutePath> SearchPaths = new HashSet<AbsolutePath>();
        /// <summary>
        /// Shared opaque outputs paths produced by the pip to whether they are allowed undeclared rewrites
        /// </summary>
        public readonly Dictionary<AbsolutePath, bool> SharedOpaqueOutputs = new Dictionary<AbsolutePath, bool>();

        /// <summary>
        /// Gets a pooled instance instance of <see cref="ObservedInputProcessingState"/>.
        /// </summary>
        public static ObservedInputProcessingState GetInstance()
        {
            return s_pool.GetInstance().Instance;
        }

        private void Clear()
        {
            DynamicObservations.Clear();
            AllowedUndeclaredReads.Clear();
            SourceDirectoriesAllDirectories.Clear();
            SourceDirectoriesTopDirectoryOnly.Clear();
            ObservationArtifacts.Clear();
            DirectoryDependencyContentsFilePaths.Clear();
            EnumeratedDirectories.Clear();
            AllDependencyPathIds.Clear();
            SearchPaths.Clear();
            SharedOpaqueOutputs.Clear();
    }

    /// <summary>
    /// Returns the instance to the pool
    /// </summary>
    public void Dispose()
        {
            s_pool.PutInstance(this);
        }
    }
}
