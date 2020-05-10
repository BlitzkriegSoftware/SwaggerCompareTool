# SwaggerCompareTool
A small tool to detect changes in swagger and potentially highlight breaking changes

## Uses NuGet

* https://github.com/Microsoft/OpenAPI.NET

## Usage

```dos
SwaggerCompareTool.exe --help
```
```text
SwaggerCompareTool 1.1.0 Copyright c 2019-2020 Blitzkrieg Software
SwaggerCompareTool 1.1.0
Copyright c 2019-2020 Blitzkrieg Software
USAGE:
Useful:
  SwaggerCompareTool --Current current.json --Excel --OutputFolder .\ --Previous previous.json --Verbose --web-report
Minimal:
  SwaggerCompareTool --Current current.json --Excel --OutputFolder .\ --Previous previous.json
With Rules:
  SwaggerCompareTool --Current current.json --Excel --OutputFolder .\ --Previous previous.json --RuleFile rules.json

  -v, --Verbose         (Default: false) Enable Verbose Output

  -c, --Current         Required. Current OpenAPI Json

  -p, --Previous        Required. Previous OpenAPI Json

  -w, --web-report      (Default: true) HTML Report

  -j, --JsonDump        (Default: false) JSON Dump

  -e, --Excel           (Default: false) CSV for Excel

  -r, --RuleFile        (Default: ) Rule File

  -o, --OutputFolder    (Default: .\) Output Folder for Reports

  --help                Display this help screen.

  --version             Display version information.
```

> -r is a future feature and does nothing (yet)

