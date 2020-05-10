using System;
using System.Linq;
using System.IO;
using CommandLine;
using CommandLine.Text;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers.ParseNodes;
using Microsoft.OpenApi.Readers;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Linq;

namespace SwaggerCompareTool
{
    class Program
    {
        #region "Standard Shell Fields"
        static int exitCode = 0; // Zero is good, not zero is bad
        #endregion

        static void Main(string[] args)
        {
            #region "Global error handler"
            // Notice a not handled exception from UOW will be caught by global error handler
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            #endregion

            Console.WriteLine($"{HeadingInfo.Default} {CopyrightInfo.Default}");

            Parser.Default.ParseArguments<Models.SwaggerCompareToolOptions>(args)
                   .WithParsed<Models.SwaggerCompareToolOptions>(o =>
                   {
                       var arguments = CommandLine.Parser.Default.FormatCommandLine(o);
                       Console.WriteLine($"{arguments}");

                       if (!File.Exists(o.Current))
                       {
                           Console.Error.WriteLine($"Current JSON Not Found: {o.Current}");
                           exitCode = -2;
                       }

                       if (!File.Exists(o.Previous))
                       {
                           Console.Error.WriteLine($"Previous JSON Not Found: {o.Previous}");
                           exitCode = -3;
                       }

                       if (File.Exists(o.RuleFile))
                       {
                           // TODO: Rule file
                       }

                       var streamCurrent = new MemoryStream(File.ReadAllBytes(o.Current));
                       var currentApi =  new OpenApiStreamReader().Read(streamCurrent, out var diagnosticCurrent);

                       if(diagnosticCurrent.Errors.Count > 0)
                       {
                           exitCode = -4;
                           Console.Error.WriteLine($"{o.Current} {diagnosticCurrent.SpecificationVersion} - Errors:");
                           foreach (var e in diagnosticCurrent.Errors)
                           {
                               Console.Error.WriteLine($"\t{e.Message} at {e.Pointer}");
                           }
                       }

                       var streamPrevious = new MemoryStream(File.ReadAllBytes(o.Previous));
                       var previousApi = new OpenApiStreamReader().Read(streamPrevious, out var diagnosticPrevious);

                       if (diagnosticCurrent.Errors.Count > 0)
                       {
                           exitCode = -5;
                           Console.Error.WriteLine($"{o.Current} {diagnosticPrevious.SpecificationVersion} - Errors:");
                           foreach (var e in diagnosticPrevious.Errors)
                           {
                               Console.Error.WriteLine($"\t{e.Message} at {e.Pointer}");
                           }
                       }

                       if(exitCode == 0)
                       {
                           var complaints = SwaggerCompare(currentApi, previousApi);
                           exitCode = SwaggerIsBroken(complaints) ? 1 : 0;
                           
                           if(o.ExcelCsv)
                           {
                               ExcelCsvDump(complaints, o);
                           }

                           if(o.JsonDump)
                           {
                               JsonDump(complaints, o);
                           }

                           if(o.WebReport)
                           {
                               WebReport(complaints, o);
                           }
                          
                       }

                   }).WithNotParsed(errs =>
                   {
                       foreach (var e in errs)
                       {
                           Console.WriteLine($"Error: {e.Tag}");
                       }
                       exitCode = -1;
                   });

            Environment.ExitCode = exitCode;
        }

        public static bool SwaggerIsBroken(List<Models.SwaggerCompareItem> complaints)
        {
            bool isBroken = false;
            if (complaints != null)
            {
                foreach (var c in complaints)
                {
                    if((c.Severity == Models.SwaggerErrorSeverity.Critical) ||
                       (c.Severity == Models.SwaggerErrorSeverity.Error))
                    {
                        isBroken = true;
                        break;
                    }
                }
            }
            return isBroken;
        }

        public static List<Models.SwaggerCompareItem> SwaggerCompare(OpenApiDocument current, OpenApiDocument previous)
        {
            var c = new List<Models.SwaggerCompareItem>();



            c = c.OrderBy(p => (int)p.Severity).ThenBy(p => (int)p.Element).ToList();
            return c;
        }

        public static void ExcelCsvDump(List<Models.SwaggerCompareItem> c, Models.SwaggerCompareToolOptions o)
        {

        }

        public static void JsonDump(List<Models.SwaggerCompareItem> c, Models.SwaggerCompareToolOptions o)
        {

        }

        public static void WebReport(List<Models.SwaggerCompareItem> c, Models.SwaggerCompareToolOptions o)
        {

        }

        #region "Global Error Handler"

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = new ApplicationException("Unhandled exception caused crash, please see logs");
            if ((e != null) && (e.ExceptionObject != null))
            {
                if (e.ExceptionObject is Exception ex2)
                {
                    ex = ex2;
                }
            }

            Console.Error.WriteLine($"CurrentDomain_UnhandledException: {ex.Message}");
            exitCode = -1; // unhandled exception
        }

        #endregion

    }
}
