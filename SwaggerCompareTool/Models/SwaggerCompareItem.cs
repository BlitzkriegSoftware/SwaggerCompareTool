using System;
using System.Collections.Generic;
using System.Text;

namespace SwaggerCompareTool.Models
{
    public class SwaggerCompareItem
    {
        public SwaggerCompareElement Element { get; set; } = SwaggerCompareElement.Unknown;
        public SwaggerErrorSeverity Severity { get; set; } = SwaggerErrorSeverity.Unknown;

        public string ElementName { get; set; }

        public string Message { get; set; }



    }
}
