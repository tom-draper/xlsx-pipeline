using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data
{
    public class InsertColumnAction : ActionBase
    {
        public string SheetName { get; set; } = "";
        public string ColumnName { get; set; }
        public int Count { get; set; } = 1;

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = string.IsNullOrEmpty(SheetName)
                    ? workbook.Worksheets.First()
                    : workbook.Worksheet(SheetName);

                worksheet.Column(ColumnName).InsertColumnsAfter(Count);

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