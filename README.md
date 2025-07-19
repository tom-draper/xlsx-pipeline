# XLSX Pipeline

Automate your local .xlsx files.

XLSX Pipeline runs as a background service processing automated workflows (pipelines) on your sheets.

Pipelines like the one below are defined using a flexible DML in JSON.

```json
{
    "pipelineName": "Generate Monthly Sales Summary",
    "trigger": {
        "type": "OnChange",
        "path": "C:\\Sales\\Monthly\\SalesData.xlsx"
    },
    "actions": [
        {
            "type": "DuplicateSheet",
            "sourceName": "Template",
            "newName": "Summary"
        },
        {
            "type": "FillFormula",
            "sheet": "Summary",
            "range": "B2:B100",
            "formula": "=SUM(Sales!B2:D2)"
        },
        {
            "type": "CopyFile",
            "destinationPath": "C:\\Sales\\Summaries\\",
            "fileName": "Summary_{month}_{year}.xlsx"
        }
    ]
}
```

You can have any number of pipelines running at once, just add a new file to the `pipelines/` directory.

```text
xlsx-pipeline.exe
pipelines/
├── clean-report.json
├── transfer-sales-data.json
├── apply-analytics.json
└── update-leads.json
```

Pipelines can have scheduled triggers (e.g. every week), or file watcher triggers (e.g. when a new file is created in a directory).

## Getting Started

<a href="https://dotnet.microsoft.com/en-us/download/dotnet/9.0">.NET Runtime 9.0 is required</a>

### Windows

Create a new service with PowerShell.

```powershell
New-Service -Name "XLSXPipeline" -BinaryPathName "C:\Path\To\xlsx-pipeline.exe" -DisplayName "XLSX Pipeline" -StartupType Automatic
Start-Service XLSXPipeline
```

Or using `sc`:

```bash
sc create XLSXPipeline binPath= "C:\Path\To\xlsx-pipeline.exe"
```

### Linux

Create a new file: `/etc/systemd/system/xlsx-pipeline.service`

```
[Unit]
Description=My .NET Worker Service
After=network.target

[Service]
WorkingDirectory=/opt/xlsx-pipeline
ExecStart=/opt/xlsx-pipeline/xlsx-pipeline
Restart=always
# Optional: Set user/group
# User=youruser
# Group=yourgroup
Environment=DOTNET_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

### macOS


