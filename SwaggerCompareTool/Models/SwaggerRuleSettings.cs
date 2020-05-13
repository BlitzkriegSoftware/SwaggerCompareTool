using System;
using System.Collections.Generic;
using System.Text;

namespace SwaggerCompareTool.Models
{
    /// <summary>
    /// Flags for Rule Interpretation
    /// 
    /// </summary>
    public class SwaggerRuleSettings
    {

        public SwaggerErrorSeverity Info_Title { get; set; } = SwaggerErrorSeverity.Information;

        public SwaggerErrorSeverity Info_Terms_of_Use { get; set; } = SwaggerErrorSeverity.Information;

        public SwaggerErrorSeverity Info_Description { get; set; } = SwaggerErrorSeverity.Information;

        public SwaggerErrorSeverity Info_Contact_Name { get; set; } = SwaggerErrorSeverity.Information;
        
        public SwaggerErrorSeverity Info_Contact_Url { get; set; } = SwaggerErrorSeverity.Information;
        
        public SwaggerErrorSeverity Info_Contact_Email { get; set; } = SwaggerErrorSeverity.Information;
        
        public SwaggerErrorSeverity Info_License_Name { get; set; } = SwaggerErrorSeverity.None;
        
        public SwaggerErrorSeverity Info_License_Url { get; set; } = SwaggerErrorSeverity.None;

        public SwaggerErrorSeverity Info_Version { get; set; } = SwaggerErrorSeverity.Error;

        public SwaggerErrorSeverity OpenApi_Version_3 { get; set; } = SwaggerErrorSeverity.Error;

        public SwaggerErrorSeverity Components_Schemas_Nullability_NoMatch { get; set; } = SwaggerErrorSeverity.Critical;

        public SwaggerErrorSeverity Components_Schemas_Type_NoMatch { get; set; } = SwaggerErrorSeverity.Critical;
        public SwaggerErrorSeverity Components_Schemas_Headers_NoMatch { get; set; } = SwaggerErrorSeverity.Warning;
        public SwaggerErrorSeverity Components_Schemas_Parameters { get; set; } = SwaggerErrorSeverity.Warning;
        public SwaggerErrorSeverity Components_Schemas_RequestBodies { get; set; } = SwaggerErrorSeverity.Warning;
        public SwaggerErrorSeverity Components_Schemas_Responses { get; set; } = SwaggerErrorSeverity.Warning;
        public SwaggerErrorSeverity Components_Schemas_Missing { get; set; } = SwaggerErrorSeverity.Warning;
        public SwaggerErrorSeverity Components_Security_Mismatch { get; set; } = SwaggerErrorSeverity.Warning;

        public SwaggerErrorSeverity Components_Schemas_Format_NoMatch { get; set; } = SwaggerErrorSeverity.Critical;

        public SwaggerErrorSeverity Components_Schemas_Required_Field_Added { get; set; } = SwaggerErrorSeverity.Critical;

        public SwaggerErrorSeverity Paths_VersionSame_Missing_Operation { get; set; } = SwaggerErrorSeverity.Critical;

        public SwaggerErrorSeverity Paths_VersionSame_Contact_Mismatch { get; set; } = SwaggerErrorSeverity.Error;

        public SwaggerErrorSeverity Server_Description { get; set; } = SwaggerErrorSeverity.Information;
        public SwaggerErrorSeverity Server_Url { get; set; } = SwaggerErrorSeverity.None;
        public SwaggerErrorSeverity Tag_Mismatch { get; set; } = SwaggerErrorSeverity.None;

        public SwaggerErrorSeverity DefaultLevel { get; set; } = SwaggerErrorSeverity.Information;

    }
}
