using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;

namespace gmdtool
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArgumentsStrict(args, options))
            {
                HandleOption(options);
            }
            else
            {
                Console.WriteLine(options.GetHelp());
            }
        }
        static void HandleOption(Options options)
        {
            GMD gmd = new GMD(options.GMDFile);
            if (options.Extract)
            {
                File.WriteAllText(options.TextFile, gmd.ToString());
            }
            else if (options.Import)
            {
                gmd.FromString(File.ReadAllText(options.TextFile));
                gmd.Save(options.GMDFile);
            }
        }
    }
    class Options
    {
        [Option('x', "extract", HelpText = "Extract text in gmd file.", MutuallyExclusiveSet = "mode")]
        public bool Extract { get; set; }
        [Option('p', "import", HelpText = "Import text to a exist gmd file.", MutuallyExclusiveSet = "mode")]
        public bool Import { get; set; }
        [Option('f', "file", Required = true, HelpText = "Specific gmd file.")]
        public string GMDFile { get; set; }
        [Option('t', "text", Required = true, HelpText = "Specific text file.")]
        public string TextFile { get; set; }

        [HelpOption]
        public string GetHelp()
        {
            return HelpText.AutoBuild(this,
                (HelpText cur) => HelpText.DefaultParsingErrorsHandler(this, cur));
        }
    }
}
