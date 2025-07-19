using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data
{
    public class CopyColumnAction : ActionBase
    {
        public string SheetName { get; set; } = "";
        public string SourceColumn { get; set; }
        public string DestinationColumn { get; set; }
        public int Count { get; set; } = 1;
        public string DestinationSheetName { get; set; } = "";
        public bool InsertColumns { get; set; } = false; // If true, insert new columns; if false, overwrite existing

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

                var sourceColumnNumber = sourceSheet.Column(SourceColumn).ColumnNumber();
                var destColumnNumber = destSheet.Column(DestinationColumn).ColumnNumber();

                // If we need to insert columns at destination
                if (InsertColumns && destSheet != null)
                {
                    destSheet.Column(destColumnNumber).InsertColumnsAfter(Count);
                }

                // Copy the columns
                for (int i = 0; i < Count; i++)
                {
                    var sourceCol = sourceSheet.Column(sourceColumnNumber + i);
                    var destCol = destSheet.Column(destColumnNumber + i);
                    sourceCol.CopyTo(destCol);
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