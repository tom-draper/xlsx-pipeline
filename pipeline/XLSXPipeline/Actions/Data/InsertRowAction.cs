using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data
{
    public class InsertRowAction : ActionBase
    {
        public string SheetName { get; set; } = "";
        public int RowNumber { get; set; }
        public int Count { get; set; } = 1;

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = string.IsNullOrEmpty(SheetName)
                    ? workbook.Worksheets.First()
                    : workbook.Worksheet(SheetName);

                worksheet.Row(RowNumber).InsertRowsAbove(Count);

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