using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using DirSync.Model;
using DotNet.Globbing;

namespace DirSync
{
    public class Options
    {
        [Value(1, MetaName = "Source dir", HelpText = "The source directory which sync from.")]
        public string SourceDir { get; set; }

        [Value(2, MetaName = "Target dir", HelpText = "The target directory which sync to.")]
        public string TargetDir { get; set; }

        [Option('c', "cleanup", HelpText = "Cleanup target directory files which are not in source directory.")]
        public bool Cleanup { get; set; }

        [Option('f', "force", HelpText = "Force to overwrite files if already exists in target directory.")]
        public bool Force { get; set; }

        [Option('s', "strict", HelpText = "Do binary check of target files if already exists.")]
        public bool Strict { get; set; }

        [Option('i', "include", HelpText = "Do binary check of target files if already exists.", Separator = ' ')]
        public IEnumerable<string> Include { get; set; }

        [Option('e', "exclude", HelpText = "Do binary check of target files if already exists.", Separator = ' ')]
        public IEnumerable<string> Exclude { get; set; }

        [Option("config")] public string Config { get; set; }

        [Option('v', "verbose")]
        public bool Verbose { get; set; }

        [Usage(ApplicationAlias = "dirsync")] public static IEnumerable<Example> Examples => new Example[] { };

        public List<SyncConfig> SyncConfigs { get; set; } = new List<SyncConfig>();

        public Lazy<List<Glob>> IncludePatterns
        {
            get
            {
                return new Lazy<List<Glob>>(() =>
                {
                    var patterns = new List<Glob>();
                    if (Include != null && Include.Any())
                    {
                        patterns.AddRange(Include.Select(Glob.Parse));
                    }

                    return patterns;
                }, true);
            }
        }

        public Lazy<List<Glob>> ExcludePatterns
        {
            get
            {
                return new Lazy<List<Glob>>(() =>
                {
                    var patterns = new List<Glob>();
                    if (Exclude != null && Exclude.Any())
                    {
                        patterns.AddRange(Exclude.Select(Glob.Parse));
                    }

                    return patterns;
                }, true);
            }
        }
    }
}