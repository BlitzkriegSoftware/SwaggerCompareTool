using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace SwaggerCompareTool.Models
{
    public class SwaggerCompareToolOptions
    {
        [Option('v', "Verbose", Default = false, HelpText = "Enable Verbose Output")]
        public bool Verbose { get; set; }

        [Option('c', "Current", Required = true, HelpText = "Current OpenAPI Json")]
        public string Current { get; set; }

        [Option('p', "Previous", Required = true, HelpText = "Previous OpenAPI Json")]
        public string Previous { get; set; }

        [Option('w', "web-report", Default = true, HelpText = "HTML Report")]
        public bool WebReport { get; set; }

        [Option('j', "JsonDump", Default = false, HelpText = "JSON Dump")]
        public bool JsonDump { get; set; }

        [Option('e', "Excel", Default = false, HelpText = "CSV for Excel")]
        public bool ExcelCsv { get; set; }

        [Option('r', "RuleFile", Default = "", HelpText = "Rule File")]
        public string RuleFile { get; set; }

        /// <summary>
        /// Help
        /// </summary>
        [Usage(ApplicationAlias = "SwaggerCompareTool")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("Useful", new SwaggerCompareToolOptions { Current="current.json", Previous="previous.json", ExcelCsv=true, WebReport=true, Verbose=true  }),
                    new Example("Minimal", new SwaggerCompareToolOptions { Current="current.json", Previous="previous.json", ExcelCsv=true }),
                    new Example("With Rules", new SwaggerCompareToolOptions { Current="current.json", Previous="previous.json", ExcelCsv=true, RuleFile = "rules.json" })
                };
            }
        }


    }
}
