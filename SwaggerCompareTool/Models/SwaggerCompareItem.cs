using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SwaggerCompareTool.Models
{
    public class SwaggerCompareItem
    {
        public SwaggerCompareElement Element { get; set; } = SwaggerCompareElement.Unknown;
        public SwaggerErrorSeverity Severity { get; set; } = SwaggerErrorSeverity.None;

        public string ElementName { get; set; }

        public string Message { get; set; }

        public OpenApiSchema CurrentSchema { get; set; }
        public OpenApiSchema PreviousSchema { get; set; }

        public static bool AreEqual(OpenApiSchema a, OpenApiSchema b, List<string> complaints)
        {
            var ja = JToken.FromObject(a);
            var jb = JToken.FromObject(b);
            return CompareJson(ja, jb, complaints);
        }

        // See: https://stackoverflow.com/questions/24876082/find-and-return-json-differences-using-newtonsoft-in-c (bottom of page)
        public static bool CompareJson(JToken actual, JToken expected, List<string> complaints)
        {
            if (complaints == null) complaints = new List<string>();

            if (actual == null)
            {
                return false;
            }
            if (expected == null)
            {
                return false;
            }

            if (actual == null)
            {
                complaints.Add($"Diff on {expected.Path}: actual - null, expected - {expected}");
                return false;
            }

            if (expected == null)
            {
                complaints.Add($"Diff on {actual.Path}: actual - {actual}, expected - null");
                return false;
            }

            if (actual.Type != JTokenType.Object && actual.Type != JTokenType.Array && actual.Type != JTokenType.Property)
            {
                if (!JToken.DeepEquals(actual, expected))
                {
                    complaints.Add($"Diff on {actual.Path}: actual- {actual}, expected - {expected}");
                }

                return false;
            }

            foreach (var jItem in actual)
            {
                var newExpected = expected.Root.SelectToken(jItem.Path);
                _ = CompareJson(jItem, newExpected, complaints);
            }

            return (complaints.Count == 0);
        }

        public static string ToReport(OpenApiSchema s, string sep = "; ")
        {
            var sb = new StringBuilder();

            if (s != null)
            {
                if (s.AdditionalProperties != null)
                {
                    //sb.Append(s.AdditionalProperties.ToReport());
                }

                sb.Append($"Additional Properties Allowed: {s.AdditionalPropertiesAllowed}{sep}");
                sb.Append($"Deprecated: {s.Deprecated}{sep}");
                sb.Append($"Description: {s.Description}{sep}");

                if (s.Discriminator != null)
                {
                    sb.Append($"Discriminator: {s.Discriminator.PropertyName}{sep}");
                }

                if (s.Enum != null)
                {
                    sb.Append("Enum: ");
                    foreach (var k in s.Enum)
                    {
                        sb.Append($"{((Microsoft.OpenApi.Any.OpenApiPrimitive<int>)k).Value}");
                        sb.Append(',');
                    }
                    sb.Append(sep);
                }

                if (s.Default != null)
                {
                    sb.Append($"Default: {s.Default.AnyType} {sep}");
                }

                if (s.Example != null)
                {
                    sb.Append($"Example: {s.Example}{sep}");
                }

                if (s.ExclusiveMaximum.HasValue)
                {
                    sb.Append($"ExclusiveMaximum: {s.ExclusiveMaximum}{sep}");
                }

                if (s.ExclusiveMinimum.HasValue)
                {
                    sb.Append($"ExclusiveMinimum: {s.ExclusiveMinimum}{sep}");
                }

                if (s.Extensions != null)
                {
                    sb.Append("Extensions: ");
                    foreach (var d in s.Extensions)
                    {
                        sb.Append($"{d.Key}: {d.Value}, ");
                    }
                    sb.Append(sep);
                }

                if (s.ExternalDocs != null)
                {
                    sb.Append($"Ext. Docs: {s.ExternalDocs.Description} @ {s.ExternalDocs.Url} {sep}");
                }

                if (!string.IsNullOrWhiteSpace(s.Format))
                {
                    sb.Append($"Format: {s.Format}{sep}");
                }

                if (s.Items != null)
                {
                    sb.Append($"Items: {JsonSerializer.Serialize(s.Items)}{sep}");
                }

                if (s.Maximum.HasValue)
                {
                    sb.Append($"Maximum: {s.Maximum}{sep}");
                }

                if (s.MaxItems.HasValue)
                {
                    sb.Append($"Max Items: {s.MaxItems}{sep}");
                }

                if (s.MaxLength.HasValue)
                {
                    sb.Append($"Max Length: {s.MaxLength}{sep}");
                }

                if (s.MaxProperties.HasValue)
                {
                    sb.Append($"Max Properties: {s.MaxProperties}{sep}");
                }

                if (s.Minimum.HasValue)
                {
                    sb.Append($"Minimum: {s.Minimum}{sep}");
                }

                if (s.MinItems.HasValue)
                {
                    sb.Append($"Min Items: {s.MinItems}{sep}");
                }

                if (s.MinLength.HasValue)
                {
                    sb.Append($"Min Length: {s.MinLength}{sep}");
                }

                if (s.MinProperties.HasValue)
                {
                    sb.Append($"Min Properties: {s.MinProperties}{sep}");
                }

                if (s.MultipleOf.HasValue)
                {
                    sb.Append($"Multiple Of: {s.MultipleOf}{sep}");
                }

                if (s.Not != null)
                {
                    sb.Append($"Not: {s.Not}{sep}");
                }

                sb.Append($"Nullable: {s.Nullable}{sep}");

                if (s.OneOf != null)
                {
                    foreach (var d in s.OneOf)
                    {
                        sb.Append($"OneOf: {JsonSerializer.Serialize(d)}{sep}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(s.Pattern))
                {
                    sb.Append($"Pattern: {s.Pattern}{sep}");
                }

                if (s.Properties != null)
                {
                    sb.Append("Properties: ");
                    foreach (var d in s.Properties)
                    {
                        sb.Append($"{d.Key}:{d.Value}, ");
                    }
                    sb.Append(sep);
                }

                sb.Append($"ReadOnly: {s.ReadOnly}{sep}");

                if (s.Reference != null)
                {
                    sb.Append($"{s.Reference.ExternalResource}, {s.Reference.Id}, {s.Reference?.Type}{sep}");
                }

                if (s.Required != null)
                {
                    sb.Append("Required: ");
                    foreach (var d in s.Required)
                    {
                        sb.Append($"{d}, ");
                    }
                    sb.Append(sep);
                }

                sb.Append($"Title: {s.Title}{sep}");

                sb.Append($"Type: {s.Type}{sep}");

                if (s.UniqueItems.HasValue)
                {
                    sb.Append($"Unique Items: {s.UniqueItems}{sep}");
                }

                sb.Append($"Unresolved Reference: {s.UnresolvedReference}{sep}");

                sb.Append($"Write Only: {s.WriteOnly}{sep}");

                if ((s.Xml != null) && !string.IsNullOrWhiteSpace(s.Xml.Name))
                {
                    sb.Append($"XML Name: {s.Xml.Name}{sep}");
                }
            }
            return sb.ToString();
        }

    }
}
