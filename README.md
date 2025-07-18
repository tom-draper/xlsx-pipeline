# XLSX Pipeline

Automate your local .xlsx files.

XLSX Pipeline runs as a background service processing automated workflows on your sheets.

Workflows, denoted 'Pipelines', like the one below are defined using a flexible DML using JSON.

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

