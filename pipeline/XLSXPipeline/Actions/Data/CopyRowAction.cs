using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data
{
    public class CopyRowAction : ActionBase
    {
        public string? SheetName { get; set; }
        public int SourceRow { get; set; }
        public int DestinationRow { get; set; }
        public int Count { get; set; } = 1;
        public string? DestinationSheetName { get; set; }
        public bool InsertRows { get; set; } = false; // If true, insert new rows; if false, overwrite existing

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var sourceSheet = string.IsNullOrEmpty(SheetName)
                    ? workbook.Worksheets.First()
                    : workbook.Worksheet(SheetName);

                var destSheet = string.IsNullOrEmpty(DestinationSheetName)
                    ? sourceSheet
                    : workbook.Worksheet(DestinationSheetName);

                // If destination sheet doesn't exist, create it
                if (destSheet == null && !string.IsNullOrEmpty(DestinationSheetName))
                {
                    destSheet = workbook.Worksheets.Add(DestinationSheetName);
                }

                // If we need to insert rows at destination
                if (InsertRows && destSheet != null)
                {
                    destSheet.Row(DestinationRow).InsertRowsAbove(Count);
                }

                // Copy the rows
                for (int i = 0; i < Count; i++)
                {
                    var sourceRowRange = sourceSheet.Row(SourceRow + i);
                    var destRowRange = destSheet.Row(DestinationRow + i);
                    sourceRowRange.CopyTo(destRowRange);
                }

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