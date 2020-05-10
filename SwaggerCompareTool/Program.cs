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
using System.Collections.Immutable;

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
                       var currentApi = new OpenApiStreamReader().Read(streamCurrent, out var diagnosticCurrent);

                       if (diagnosticCurrent.Errors.Count > 0)
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

                       if (exitCode == 0)
                       {
                           var complaints = SwaggerCompare(currentApi, previousApi);
                           exitCode = SwaggerIsBroken(complaints) ? 1 : 0;

                           if (o.ExcelCsv)
                           {
                               ExcelCsvDump(complaints, o);
                           }

                           if (o.JsonDump)
                           {
                               JsonDump(complaints, o);
                           }

                           if (o.WebReport)
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
                    if ((c.Severity == Models.SwaggerErrorSeverity.Critical) ||
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

            #region "Info"

            if (current.Info.Title.CompareTo(previous.Info.Title) != 0)
            {
                c.Add(new Models.SwaggerCompareItem()
                {
                    Severity = Models.SwaggerErrorSeverity.Information,
                    Element = Models.SwaggerCompareElement.Info,
                    ElementName = "Title",
                    Message = $"{previous.Info.Title} => {current.Info.Title}"
                });
            }

            if (current.Info.TermsOfService?.ToString().CompareTo(previous.Info.TermsOfService?.ToString()) != 0)
            {
                c.Add(new Models.SwaggerCompareItem()
                {
                    Severity = Models.SwaggerErrorSeverity.Information,
                    Element = Models.SwaggerCompareElement.Info,
                    ElementName = "TermsOfService",
                    Message = $"{previous.Info.TermsOfService?.ToString()} => {current.Info.TermsOfService?.ToString()}"
                });
            }

            if (current.Info.Description.CompareTo(previous.Info.Description) != 0)
            {
                c.Add(new Models.SwaggerCompareItem()
                {
                    Severity = Models.SwaggerErrorSeverity.Information,
                    Element = Models.SwaggerCompareElement.Info,
                    ElementName = "Description",
                    Message = $"{previous.Info.Description} => {current.Info.Description}"
                });
            }

            if (current.Info.Version.CompareTo(previous.Info.Version) != 0)
            {
                c.Add(new Models.SwaggerCompareItem()
                {
                    Severity = Models.SwaggerErrorSeverity.Warning,
                    Element = Models.SwaggerCompareElement.Info,
                    ElementName = "Version",
                    Message = $"{previous.Info.License.Url.ToString()} => {current.Info.License.Url.ToString()}"
                });
            }

            if (current.Info.Contact.Email.CompareTo(previous.Info.Contact.Email) != 0)
            {
                c.Add(new Models.SwaggerCompareItem()
                {
                    Severity = Models.SwaggerErrorSeverity.Information,
                    Element = Models.SwaggerCompareElement.Info,
                    ElementName = "Contact.eMail",
                    Message = $"{previous.Info.Contact.Email} => {current.Info.Contact.Email}"
                });
            }

            if (current.Info.Contact.Name.CompareTo(previous.Info.Contact.Name) != 0)
            {
                c.Add(new Models.SwaggerCompareItem()
                {
                    Severity = Models.SwaggerErrorSeverity.Information,
                    Element = Models.SwaggerCompareElement.Info,
                    ElementName = "Contact.Name",
                    Message = $"{previous.Info.Contact.Name} => {current.Info.Contact.Name}"
                });
            }

            if (current.Info.Contact.Url.ToString().CompareTo(previous.Info.Contact.Url.ToString()) != 0)
            {
                c.Add(new Models.SwaggerCompareItem()
                {
                    Severity = Models.SwaggerErrorSeverity.Information,
                    Element = Models.SwaggerCompareElement.Info,
                    ElementName = "Contact.Url",
                    Message = $"{previous.Info.Contact.Url} => {current.Info.Contact.Url}"
                });
            }

            if (current.Info.License.Name.CompareTo(previous.Info.License.Name) != 0)
            {
                c.Add(new Models.SwaggerCompareItem()
                {
                    Severity = Models.SwaggerErrorSeverity.Information,
                    Element = Models.SwaggerCompareElement.Info,
                    ElementName = "License.Name",
                    Message = $"{previous.Info.License.Name} => {current.Info.License.Name}"
                });
            }

            if (current.Info.License.Url.ToString().CompareTo(previous.Info.License.Url.ToString()) != 0)
            {
                c.Add(new Models.SwaggerCompareItem()
                {
                    Severity = Models.SwaggerErrorSeverity.Information,
                    Element = Models.SwaggerCompareElement.Info,
                    ElementName = "License.Url",
                    Message = $"{previous.Info.License.Url.ToString()} => {current.Info.License.Url.ToString()}"
                });
            }

            #endregion

            #region "Servers"

            foreach (var s in current.Servers)
            {
                var item = previous.Servers.Where(p => p.Description == s.Description).FirstOrDefault();

                if (item == null)
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Element = Models.SwaggerCompareElement.Servers,
                        Severity = Models.SwaggerErrorSeverity.Information,
                        ElementName = item.Description,
                        Message = $"Previous Missing: {s.Description}"
                    });
                }

                item = previous.Servers.Where(p => p.Url == s.Url).FirstOrDefault();
                if (item == null)
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Element = Models.SwaggerCompareElement.Servers,
                        Severity = Models.SwaggerErrorSeverity.Information,
                        ElementName = item.Url,
                        Message = $"Previous Missing: {s.Url}"
                    });
                }
            }

            foreach (var s in previous.Servers)
            {
                var item = current.Servers.Where(p => p.Description == s.Description).FirstOrDefault();

                if (item == null)
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Element = Models.SwaggerCompareElement.Servers,
                        Severity = Models.SwaggerErrorSeverity.Information,
                        ElementName = item.Description,
                        Message = $"Current Missing: {s.Description}"
                    });
                }

                item = current.Servers.Where(p => p.Url == s.Url).FirstOrDefault();
                if (item == null)
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Element = Models.SwaggerCompareElement.Servers,
                        Severity = Models.SwaggerErrorSeverity.Information,
                        ElementName = item.Url,
                        Message = $"Current Missing: {s.Url}"
                    });
                }
            }

            #endregion

            #region "Paths"

            foreach (var d in current.Paths)
            {
                var item = previous.Paths.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Paths,
                        ElementName = "Paths.Key",
                        Message = $"Current Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Paths,
                            ElementName = "Paths.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in previous.Paths)
            {
                var item = current.Paths.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Paths,
                        ElementName = "Paths.Key",
                        Message = $"Previous Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Paths,
                            ElementName = "Paths.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                        });
                    }
                }
            }

            #endregion

            #region "Components"

            foreach (var d in current.Components.Callbacks)
            {
                var item = previous.Components.Callbacks.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Callbacks.Key",
                        Message = $"Previous Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Components,
                            ElementName = "Callbacks.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in previous.Components.Callbacks)
            {
                var item = current.Components.Callbacks.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Callbacks.Key",
                        Message = $"Current Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Components,
                            ElementName = "Callbacks.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in current.Components.Examples)
            {
                var item = previous.Components.Examples.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Examples.Key",
                        Message = $"Previous Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Paths,
                            ElementName = "Examples.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in previous.Components.Examples)
            {
                var item = current.Components.Examples.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Examples.Key",
                        Message = $"Current Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Components,
                            ElementName = "Examples.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in current.Components.Extensions)
            {
                var item = previous.Components.Extensions.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Extensions.Key",
                        Message = $"Previous Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Paths,
                            ElementName = "Extensions.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in previous.Components.Examples)
            {
                var item = current.Components.Examples.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Extensions.Key",
                        Message = $"Current Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Components,
                            ElementName = "Extensions.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in current.Components.Headers)
            {
                var item = previous.Components.Headers.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Headers.Key",
                        Message = $"Previous Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Paths,
                            ElementName = "Headers.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in previous.Components.Headers)
            {
                var item = current.Components.Headers.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Headers.Key",
                        Message = $"Current Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Components,
                            ElementName = "Headers.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in current.Components.Links)
            {
                var item = previous.Components.Links.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Links.Key",
                        Message = $"Previous Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Paths,
                            ElementName = "Links.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in previous.Components.Links)
            {
                var item = current.Components.Links.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Links.Key",
                        Message = $"Current Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Components,
                            ElementName = "Links.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in current.Components.Parameters)
            {
                var item = previous.Components.Parameters.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Parameters.Key",
                        Message = $"Previous Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Paths,
                            ElementName = "Parameters.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in previous.Components.Parameters)
            {
                var item = current.Components.Parameters.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Parameters.Key",
                        Message = $"Current Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Components,
                            ElementName = "Parameters.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in current.Components.RequestBodies)
            {
                var item = previous.Components.RequestBodies.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "RequestBodies.Key",
                        Message = $"Previous Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Paths,
                            ElementName = "RequestBodies.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in previous.Components.RequestBodies)
            {
                var item = current.Components.RequestBodies.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "RequestBodies.Key",
                        Message = $"Current Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Components,
                            ElementName = "RequestBodies.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in current.Components.Responses)
            {
                var item = previous.Components.Responses.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Responses.Key",
                        Message = $"Previous Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Paths,
                            ElementName = "Responses.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in previous.Components.Responses)
            {
                var item = current.Components.Responses.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Responses.Key",
                        Message = $"Current Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Components,
                            ElementName = "Responses.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in current.Components.Schemas)
            {
                var item = previous.Components.Schemas.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Schemas.Key",
                        Message = $"Previous Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Paths,
                            ElementName = "Schemas.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in previous.Components.Schemas)
            {
                var item = current.Components.Schemas.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "Schemas.Key",
                        Message = $"Current Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Components,
                            ElementName = "Schemas.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in current.Components.SecuritySchemes)
            {
                var item = previous.Components.SecuritySchemes.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "SecuritySchemes.Key",
                        Message = $"Previous Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Paths,
                            ElementName = "SecuritySchemes.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in previous.Components.SecuritySchemes)
            {
                var item = current.Components.SecuritySchemes.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Components,
                        ElementName = "SecuritySchemes.Key",
                        Message = $"Current Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Components,
                            ElementName = "SecuritySchemes.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                        });
                    }
                }
            }

            #endregion

            #region "Extensions"

            foreach (var d in current.Extensions)
            {
                var item = previous.Extensions.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Extensions,
                        ElementName = "Extensions.Key",
                        Message = $"Previous Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Extensions,
                            ElementName = "Extensions.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                        });
                    }
                }
            }

            foreach (var d in previous.Extensions)
            {
                var item = current.Extensions.Where(p => p.Key == d.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item.Key))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Extensions,
                        ElementName = "Extensions.Key",
                        Message = $"Current Missing Key: {d.Key}"
                    });
                }
                else
                {
                    if (!d.Value.Equals(item.Value))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Extensions,
                            ElementName = "Extensions.Key.Value",
                            Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                        });
                    }
                }
            }

            #endregion

            #region "ExternalDocs"

            if((current.ExternalDocs != null) && (previous.ExternalDocs != null))
            {
                foreach (var d in current.ExternalDocs?.Extensions)
                {
                    var item = previous.ExternalDocs.Extensions.Where(p => p.Key == d.Key).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(item.Key))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.ExternalDocs,
                            ElementName = "ExternalDocs.Extensions.Key",
                            Message = $"Previous Missing Key: {d.Key}"
                        });
                    }
                    else
                    {
                        if (!d.Value.Equals(item.Value))
                        {
                            c.Add(new Models.SwaggerCompareItem()
                            {
                                Severity = Models.SwaggerErrorSeverity.Warning,
                                Element = Models.SwaggerCompareElement.ExternalDocs,
                                ElementName = "ExternalDocs.Extensions.Key.Value",
                                Message = $"Missmatched Value: {d.Key}: Current: {d.Value}, Previous: {item.Value}"
                            });
                        }
                    }
                }

                if (!current.ExternalDocs.Description.Equals(previous.ExternalDocs.Description))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.ExternalDocs,
                        ElementName = "ExternalDocs.Description",
                        Message = $"Current: {current.ExternalDocs.Description}, Previous: {previous.ExternalDocs.Description}"
                    });
                }

                if (!current.ExternalDocs.Url.ToString().Equals(previous.ExternalDocs.Url.ToString()))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.ExternalDocs,
                        ElementName = "ExternalDocs.Url",
                        Message = $"Current: {current.ExternalDocs.Url}, Previous: {previous.ExternalDocs.Url}"
                    });
                }

                foreach (var d in previous.ExternalDocs?.Extensions)
                {
                    var item = current.ExternalDocs.Extensions.Where(p => p.Key == d.Key).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(item.Key))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.ExternalDocs,
                            ElementName = "ExternalDocs.Extensions.Key",
                            Message = $"Current Missing Key: {d.Key}"
                        });
                    }
                    else
                    {
                        if (!d.Value.Equals(item.Value))
                        {
                            c.Add(new Models.SwaggerCompareItem()
                            {
                                Severity = Models.SwaggerErrorSeverity.Warning,
                                Element = Models.SwaggerCompareElement.ExternalDocs,
                                ElementName = "ExternalDocs.Extensions.Key.Value",
                                Message = $"Missmatched Value: {d.Key}: Previous: {d.Value}, Current: {item.Value}"
                            });
                        }
                    }
                }
            }
            #endregion

            #region "SecurityRequirements"


            #endregion

            #region "Tags"

            foreach(var d in current.Tags)
            {
                var item = previous.Tags.Where(p => p.Name == d.Name).FirstOrDefault();
                if(string.IsNullOrWhiteSpace(item?.Name))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Tags,
                        ElementName = "Tags.Name",
                        Message = $"Previous Tags.Name {d.Name} is missing"
                    });
                } else
                {
                    if(!d.Description.Equals(item.Description))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Tags,
                            ElementName = "Tags.Desciption",
                            Message = $"Mismatched. Current: {d.Description}, Previous: {item.Description}"
                        });
                    }
               
                    if(!d.Reference.ExternalResource.Equals(item.Reference.ExternalResource))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Tags,
                            ElementName = "Tags.Reference.ExternalResource",
                            Message = $"Mismatched. Current: {d.Reference.ExternalResource}, Previous: {item.Reference.ExternalResource}"
                        });
                    }

                }
            }

            foreach (var d in previous.Tags)
            {
                var item = current.Tags.Where(p => p.Name == d.Name).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(item?.Name))
                {
                    c.Add(new Models.SwaggerCompareItem()
                    {
                        Severity = Models.SwaggerErrorSeverity.Warning,
                        Element = Models.SwaggerCompareElement.Tags,
                        ElementName = "Tags.Name",
                        Message = $"Current Tags.Name {d.Name} is missing"
                    });
                }
                else
                {

                    if (!d.Description.Equals(item.Description))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Tags,
                            ElementName = "Tags.Desciption",
                            Message = $"Mismatched. Previous: {d.Description}, Current: {item.Description}"
                        });
                    }

                    if (!d.Reference.ExternalResource.Equals(item.Reference.ExternalResource))
                    {
                        c.Add(new Models.SwaggerCompareItem()
                        {
                            Severity = Models.SwaggerErrorSeverity.Warning,
                            Element = Models.SwaggerCompareElement.Tags,
                            ElementName = "Tags.Reference.ExternalResource",
                            Message = $"Mismatched. Previous: {d.Reference.ExternalResource}, Current: {item.Reference.ExternalResource}"
                        });
                    }

                }
            }

            #endregion

            c = c.OrderBy(p => (int)p.Severity).ThenBy(p => (int)p.Element).ToList();
            return c;
        }

        #region "Reports"

        public static void ExcelCsvDump(List<Models.SwaggerCompareItem> c, Models.SwaggerCompareToolOptions o)
        {
            var reportName = Path.ChangeExtension(o.ReportName, ".csv");
            reportName = Path.Combine(o.OutputFolder, reportName);
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
            reportName = Path.Combine(o.OutputFolder, reportName);
            if (File.Exists(reportName)) File.Delete(reportName);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(c);

            File.WriteAllText(reportName, json);

            Console.WriteLine($"JSON: {reportName}");
        }

        public static void WebReport(List<Models.SwaggerCompareItem> c, Models.SwaggerCompareToolOptions o)
        {
            var reportName = Path.ChangeExtension(o.ReportName, ".html");
            reportName = Path.Combine(o.OutputFolder, reportName);
            if (File.Exists(reportName)) File.Delete(reportName);

            using (var file = new System.IO.StreamWriter(reportName))
            {
                file.WriteLine(HtmlReportParts.Top);

                file.WriteLine($"<h1>Swagger Compare Report {DateTime.Now:f}</h1>");
                file.WriteLine("<div class='container-fluid'>");
                
                if (c.Count > 0)
                {
                    file.WriteLine("<table class='table table-bordered'>");

                    file.WriteLine("<tr>");
                    file.Write("<th scope='col'>Severity</th>");
                    file.Write("<th scope='col'>Component</strong></th>");
                    file.Write("<th scope='col'>Element</strong></th>");
                    file.WriteLine("<th scope='col'>Message</strong></th>");
                    file.WriteLine("</tr>");

                    foreach (var d in c)
                    {
                        file.WriteLine("<tr>");
                        file.Write($"<td scope='col'>{(int)d.Severity} {d.Severity}</td>");
                        file.Write($"<td scope='col'>{d.Element}</td>");
                        file.Write($"<td scope='col'>{d.ElementName}</td>");
                        file.WriteLine($"<td scope='col'>{d.Message}</td>");
                        file.WriteLine("</tr>");
                    }

                    file.WriteLine("</table>");
                }
                else
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
