using CommandLine;
using CommandLine.Text;
using DirSync.Model;
using DotNet.Globbing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DirSync
{
    public class Options
    {
        [Value(1, MetaName = "Source directory", HelpText = "The source directory which sync from. Required while config file is not provided.")]
        public string SourceDir { get; set; }

        [Value(2, MetaName = "Target directory", HelpText = "The target directory which sync to. Required while config file is not provided.")]
        public string TargetDir { get; set; }

        [Option('c', "cleanup", HelpText = "Cleanup target directory files which are not in source directory. Default: False.")]
        public bool Cleanup { get; set; }

        [Option('f', "force", HelpText = "Force to overwrite files if already exists in target directory. Default: False.")]
        public bool Force { get; set; }

        [Option('s', "strict", HelpText = "Do binary check of target files if already exists. Default: False.")]
        public bool Strict { get; set; }

        [Option('i', "include", HelpText = "Use glob pattern to include files. Default: inlucde all files.", Separator = ' ')]
        public IEnumerable<string> Include { get; set; }

        [Option('e', "exclude", HelpText = "Use glob pattern to exclude files. Default: no files to exclude. Note: this has higher priority over -i/--include.", Separator = ' ')]
        public IEnumerable<string> Exclude { get; set; }

        [Option("config", HelpText = "Json configuration file for specifying multiple source-target mappings.")] 
        public string Config { get; set; }

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