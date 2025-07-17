using ClosedXML.Excel;

namespace ExcelPipeline.Actions.Cells
{
    public class ApplyFormulaAction : ActionBase
    {
        public string SheetName { get; set; } = "";
        public string CellAddress { get; set; }
        public string Formula { get; set; }

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = string.IsNullOrEmpty(SheetName)
                    ? workbook.Worksheets.First()
                    : workbook.Worksheet(SheetName);

                worksheet.Cell(CellAddress).FormulaA1 = Formula;
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