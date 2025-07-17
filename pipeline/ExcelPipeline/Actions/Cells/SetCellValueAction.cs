using ClosedXML.Excel;

namespace ExcelPipeline.Actions.Cells
{
    public class SetCellValueAction : ActionBase
    {
        public string SheetName { get; set; } = "";
        public string CellAddress { get; set; }
        public string Value { get; set; }

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = string.IsNullOrEmpty(SheetName)
                    ? workbook.Worksheets.First()
                    : workbook.Worksheet(SheetName);

                worksheet.Cell(CellAddress).Value = Value;
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