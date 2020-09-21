using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace DirSync
{
    public class Options
    {
        [Value(1, Required = true, MetaName = "Source dir", HelpText = "The source directory which sync from.")]
        public string SourceDir { get; set; }

        [Value(2, Required = true, MetaName = "Target dir", HelpText = "The target directory which sync to.")]
        public string TargetDir { get; set; }

        [Option('c', "cleanup", HelpText = "Cleanup target directory files which are not in source directory.")]
        public bool Cleanup { get; set; }

        [Option('f', "force", HelpText = "Force to overwrite files if already exists in target directory.")]
        public bool Force { get; set; }

        [Option('s', "strict", HelpText = "Do binary check of target files if already exists.")]
        public bool Strict { get; set; }

        [Usage(ApplicationAlias = "dirsync")]
        public static IEnumerable<Example> Examples => new Example[]{};
    }
}
