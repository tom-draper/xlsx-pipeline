using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet
{
    public class AddSheetAction : ActionBase
    {
        public required string SheetName { get; set; }

        protected override Task ExecuteInternalAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);

                if (workbook.Worksheets.Any(ws => ws.Name == SheetName))
                    throw new InvalidOperationException($"Sheet '{SheetName}' already exists.");

                workbook.AddWorksheet(SheetName);
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
