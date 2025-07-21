using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet
{
    public class MoveSheetAction : ActionBase
    {
        public required string SheetName { get; set; }
        public int TargetIndex { get; set; } // 1-based index

        protected override Task ExecuteInternalAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(SheetName);

                if (worksheet == null)
                    throw new InvalidOperationException($"Sheet '{SheetName}' not found.");

                if (TargetIndex < 1 || TargetIndex > workbook.Worksheets.Count)
                    throw new ArgumentOutOfRangeException(nameof(TargetIndex), "Index out of range.");

                worksheet.Position = TargetIndex;
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
