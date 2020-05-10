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

        #region "Reports"

        public static void ExcelCsvDump(List<Models.SwaggerCompareItem> c, Models.SwaggerCompareToolOptions o)
        {
            var reportName = Path.ChangeExtension(o.ReportName, ".csv");
            if (File.Exists(reportName)) File.Delete(reportName);
            if (c.Count > 0)
            {
                using (var file = new System.IO.StreamWriter(reportName))
                {
                    file.Write('"');
                    file.Write("Severity");
                    file.Write('"');
                    file.Write(',');
                    file.Write('"');
                    file.Write("Element");
                    file.Write('"');
                    file.Write(',');
                    file.Write('"');
                    file.Write("Element Name");
                    file.Write('"');
                    file.Write(',');
                    file.Write('"');
                    file.Write("Message");
                    file.WriteLine('"');

                    foreach (var d in c)
                    {
                        file.Write('"');
                        file.Write(d.Severity);
                        file.Write('"');
                        file.Write(",");
                        file.Write('"');
                        file.Write(d.Element);
                        file.Write('"');
                        file.Write(",");
                        file.Write('"');
                        file.Write(d.ElementName);
                        file.Write('"');
                        file.Write(",");
                        file.Write('"');
                        file.Write(d.Message);
                        file.WriteLine('"');
                    }
                }
            }
            Console.WriteLine($"CSV: {reportName}");
        }

        public static void JsonDump(List<Models.SwaggerCompareItem> c, Models.SwaggerCompareToolOptions o)
        {
            var reportName = Path.ChangeExtension(o.ReportName, ".json");
            if (File.Exists(reportName)) File.Delete(reportName);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(c);

            File.WriteAllText(reportName, json);

            Console.WriteLine($"JSON: {reportName}");
        }

        public static void WebReport(List<Models.SwaggerCompareItem> c, Models.SwaggerCompareToolOptions o)
        {
            var reportName = Path.ChangeExtension(o.ReportName, ".html");
            if (File.Exists(reportName)) File.Delete(reportName);
            using (var file = new System.IO.StreamWriter(reportName))
            {
                file.WriteLine(HtmlReportParts.Top);

                file.WriteLine($"<h1>Swagger Compare Report {DateTime.Now:f}</h1>");
                file.WriteLine("<div class='container-fluid'>");

                if (c.Count > 0)
                {
                    file.WriteLine("<div class='row'>");
                    file.Write("<div class='col-sm'><strong>Severity</strong></div>");
                    file.Write("<div class='col-sm'><strong>Component</strong></div>");
                    file.Write("<div class='col-sm'><strong>Element</strong></div>");
                    file.WriteLine("<div class='col-sm'><strong>Message</strong></div>");
                    file.WriteLine("</div>");
                    foreach(var d in c)
                    {
                        file.WriteLine("<div class='row'>");
                        file.Write($"<div class='col-sm'>{(int) d.Severity} {d.Severity}</div>");
                        file.Write($"<div class='col-sm'>{d.Element}</div>");
                        file.Write($"<div class='col-sm'>{d.ElementName}</div>");
                        file.WriteLine($"<div class='col-sm'>{d.Message}</div>");
                        file.WriteLine("</div>");
                    }
                } else
                {
                    file.WriteLine("<p>No Problems Detected...</p>");
                }
                file.WriteLine("</div>");
                file.WriteLine(HtmlReportParts.Bottom);
            }
            Console.WriteLine($"HTML: {reportName}");
        }

        #endregion

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
