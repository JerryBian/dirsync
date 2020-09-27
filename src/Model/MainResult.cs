using System;
using System.Collections.Generic;
using System.Linq;

namespace DirSync.Model
{
    public class MainResult
    {
        public List<SyncConfig> Configs { get; set; } = new List<SyncConfig>();

        public Dictionary<SyncConfig, int> SrcDirCount { get; set; } = new Dictionary<SyncConfig, int>();

        public Dictionary<SyncConfig, int> SrcFileCount { get; set; } = new Dictionary<SyncConfig, int>();

        public Dictionary<SyncConfig, int> TargetAffectedFileCount { get; set; } = new Dictionary<SyncConfig, int>();

        public bool Succeed { get; set; }

        public TimeSpan Took { get; set; }

        public Exception Error { get; set; }

        public void AggregateSyncIngestResults(List<SyncIngestResult> results)
        {
            foreach (var syncIngestResult in results)
            {
                SrcDirCount.Add(syncIngestResult.SyncConfig, syncIngestResult.SourceDirCount);
                SrcFileCount.Add(syncIngestResult.SyncConfig, syncIngestResult.SourceFileCount);
            }
        }

        public void AggregateCleanupIngestResults(List<CleanupIngestResult> results)
        {
        }

        public void AggregateRunResults(List<SyncRunResult> syncRunResults, List<CleanupRunResult> cleanupRunResults)
        {
            foreach (var syncRunResult in syncRunResults)
            {
                TargetAffectedFileCount.Add(syncRunResult.SyncConfig, syncRunResult.TargetAffectedFileCount);

                var cleanupResult = cleanupRunResults.FirstOrDefault(x => x.SyncConfig == syncRunResult.SyncConfig);
                if (cleanupResult != null)
                {
                    TargetAffectedFileCount[syncRunResult.SyncConfig] += cleanupResult.TargetAffectedFileCount;
                }
            }

            foreach (var cleanupIngestResult in cleanupRunResults)
            {
                if (!TargetAffectedFileCount.ContainsKey(cleanupIngestResult.SyncConfig))
                {
                    TargetAffectedFileCount.Add(cleanupIngestResult.SyncConfig,
                        cleanupIngestResult.TargetAffectedFileCount);
                }
            }

            if (Configs != null && Configs.Any())
            {
                foreach (var syncConfig in Configs)
                {
                    if (!TargetAffectedFileCount.ContainsKey(syncConfig))
                    {
                        TargetAffectedFileCount.Add(syncConfig, 0);
                    }
                }
            }
        }
    }
}