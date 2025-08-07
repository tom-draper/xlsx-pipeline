
<img width="1290" height="786" alt="xlsx pipeline" src="/website/src/assets/banner.png" />

<br>

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

You can have any number of pipelines running at once, just add a new file to the `Pipelines/` directory.

```text
XLSXPipeline.exe
Pipelines/
├── CleanReport.json
├── TransferSalesData.json
├── ApplyAnalytics.json
└── UpdateLeads.json
```

Pipelines can have:

- scheduled triggers (e.g., 6pm every day), or
- file watcher triggers (e.g., when a new file is created in a directory),

followed by a series of actions that will execute sequentially.

Check out the docs to help build your own pipelines, or browse the [examples](/pipeline/XLSXPipeline/Examples).

## Getting Started

No Excel or spreadsheet software required.

Download the latest release from <a href="https://github.com/tom-draper/xlsx-pipeline/releases/latest">GitHub</a>, or clone the repo and build locally:

```bash
git clone https://github.com/tom-draper/xlsx-pipeline.git
dotnet publish pipeline/XLSXPipeline -c Release --self-contained -p:PublishSingleFile=true
```

### Windows

Create a new service with PowerShell.

```powershell
New-Service -Name "XLSXPipeline" -BinaryPathName "C:\Path\To\XLSXPipeline.exe" -DisplayName "XLSX Pipeline" -StartupType Automatic
Start-Service XLSXPipeline
```

Or using `sc`:

```bash
sc create XLSXPipeline binPath="C:\Path\To\XLSXPipeline.exe"
```

### Linux

Create a new file: `/etc/systemd/system/xlsx-pipeline.service`

```
[Unit]
Description=XLSX Pipeline Service
After=network.target

[Service]
WorkingDirectory=/opt/XLSXPipeline
ExecStart=/opt/XLSXPipeline/XLSXPipeline
Restart=always
# Optional: Set user/group
# User=youruser
# Group=yourgroup
Environment=DOTNET_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

Restart the service after making any changes to a pipeline files within `/Pipeline` to reload.

## Contributions

Contributions, issues and feature requests are welcome.

- Fork it (https://github.com/tom-draper/xlsx-pipeline)
- Create your feature branch (`git checkout -b my-new-feature`)
- Commit your changes (`git commit -am 'Add some feature'`)
- Push to the branch (`git push origin my-new-feature`)
- Create a new Pull Request

