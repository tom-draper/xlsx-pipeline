using ClosedXML.Excel;

namespace ExcelPipeline.Actions.Worksheet
{
    public class RenameSheetAction : ActionBase
    {
        public string OriginalName { get; set; }
        public string NewName { get; set; }

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(OriginalName);
                if (worksheet != null)
                {
                    worksheet.Name = NewName;
                    workbook.Save();
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}