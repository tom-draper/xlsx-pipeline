# Excel Pipeline

Automate your local Excel files.

Excel Pipeline runs as a background service running automated workflows on your Excel sheets.

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
            "type": "SaveAs",
            "destinationPath": "C:\\Sales\\Summaries\\",
            "fileNameFormat": "Summary_{month}_{year}.xlsx"
        }
    ]
}
```

You can have any number of pipelines running at once, just add a new file to the `Pipelines/` directory.

```text
ExcelPipeline.exe
Pipelines/
├── CleanReport.json
├── TransferSalesData.json
├── ApplyAnalytics.json
└── UpdateLeads.json
```

