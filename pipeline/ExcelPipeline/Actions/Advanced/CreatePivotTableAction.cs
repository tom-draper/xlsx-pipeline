using ClosedXML.Excel;

namespace ExcelPipeline.Actions.Advanced
{
    public class CreatePivotTableAction : ActionBase
    {
        public string SourceSheetName { get; set; } = "";
        public string SourceRange { get; set; }
        public string DestinationSheetName { get; set; }
        public string DestinationCell { get; set; }
        public List<string> RowFields { get; set; } = new();
        public List<string> ColumnFields { get; set; } = new();
        public List<string> DataFields { get; set; } = new();

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var sourceSheet = string.IsNullOrEmpty(SourceSheetName)
                    ? workbook.Worksheets.First()
                    : workbook.Worksheet(SourceSheetName);

                var destSheet = workbook.Worksheet(DestinationSheetName) ?? workbook.Worksheets.Add(DestinationSheetName);

                var sourceRange = sourceSheet.Range(SourceRange);
                var pivotTable = destSheet.PivotTables.Add("PivotTable1", destSheet.Cell(DestinationCell), sourceRange);

                foreach (var field in RowFields)
                    pivotTable.RowLabels.Add(field);

                foreach (var field in ColumnFields)
                    pivotTable.ColumnLabels.Add(field);

                foreach (var field in DataFields)
                    pivotTable.Values.Add(field);

                workbook.Save();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}
