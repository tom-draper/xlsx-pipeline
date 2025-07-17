using ClosedXML.Excel;

namespace ExcelPipeline.Actions.Data
{
    public class SortDataAction : ActionBase
    {
        public string SheetName { get; set; } = "";
        public string Range { get; set; }
        public string SortColumn { get; set; }
        public bool Ascending { get; set; } = true;

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = string.IsNullOrEmpty(SheetName)
                    ? workbook.Worksheets.First()
                    : workbook.Worksheet(SheetName);

                var range = worksheet.Range(Range);
                var sortRange = range.Sort(SortColumn, Ascending ? XLSortOrder.Ascending : XLSortOrder.Descending);

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