using System;
using System.IO;
using CommandLine;
using CommandLine.Text;


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

                       if(!File.Exists(o.Current))
                       {
                           Console.Error.WriteLine($"Current JSON Not Found: {o.Current}");
                           exitCode = -2;
                       }

                       if (!File.Exists(o.Previous))
                       {
                           Console.Error.WriteLine($"Previous JSON Not Found: {o.Previous}");
                           exitCode = -3;
                       }

                       if(File.Exists(o.RuleFile))
                       {
                           // TODO: Rule file
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
