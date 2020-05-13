# SwaggerCompareTool
A small tool to detect changes in swagger and potentially highlight breaking changes

## Uses NuGet

* https://github.com/Microsoft/OpenAPI.NET

## Usage

```dos
SwaggerCompareTool.exe --help
```
```text
SwaggerCompareTool 1.3.0 Copyright c 2019-2020 Blitzkrieg Software
Copyright c 2019-2020 Blitzkrieg Software

USAGE:
Useful:
  SwaggerCompareTool --Current current.json --Excel --OutputFolder .\ --Previous previous.json --Verbose --web-report
Minimal:
  SwaggerCompareTool --Current current.json --Excel --OutputFolder .\ --Previous previous.json
With Rules:
  SwaggerCompareTool --Current current.json --Excel --OutputFolder .\ --Previous previous.json --RuleFile rules.json

  -v, --Verbose          (Default: false) Enable Verbose Output

  -c, --Current          Current OpenAPI Json

  -p, --Previous         Previous OpenAPI Json

  -w, --web-report       (Default: false) HTML Report

  -j, --JsonDump         (Default: false) JSON Dump

  -e, --Excel            (Default: false) CSV for Excel

  -r, --RuleFile         (Default: ) Rule File

  -o, --OutputFolder     (Default: .\) Output Folder for Reports

  -m, --MakeRulesFile    (Default: false) Make a sample rule file

  --help                 Display this help screen.

  --version              Display version information.
```


## Rule Severity

|Severity|Value|
|:---|:---:|
|None|0|
|Information|1|
|Warning|2|
|Error|3|
|Critical|4|

## Default Rule File

```json
{
  "Info_Title": 1,
  "Info_Terms_of_Use": 1,
  "Info_Description": 1,
  "Info_Contact_Name": 1,
  "Info_Contact_Url": 1,
  "Info_Contact_Email": 1,
  "Info_License_Name": 0,
  "Info_License_Url": 0,
  "Info_Version": 3,
  "OpenApi_Version_3": 3,
  "Components_Schemas_Nullability_NoMatch": 4,
  "Components_Schemas_Type_NoMatch": 4,
  "Components_Schemas_Headers_NoMatch": 2,
  "Components_Schemas_Parameters": 2,
  "Components_Schemas_RequestBodies": 2,
  "Components_Schemas_Responses": 2,
  "Components_Schemas_Missing": 2,
  "Components_Security_Mismatch": 2,
  "Components_Schemas_Format_NoMatch": 4,
  "Components_Schemas_Required_Field_Added": 4,
  "Paths_VersionSame_Missing_Operation": 4,
  "Paths_VersionSame_Contact_Mismatch": 3,
  "Server_Description": 1,
  "Server_Url": 0,
  "Tag_Mismatch": 0,
  "DefaultLevel": 1
}
```



