using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet
{
    public class UnprotectSheetAction : ActionBase
    {
        public string? SheetName { get; set; }
        public required string Password { get; set; }

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = string.IsNullOrEmpty(SheetName)
                    ? workbook.Worksheets.First()
                    : workbook.Worksheet(SheetName);

                // Check if sheet is protected before attempting to unprotect
                if (worksheet.Protection.IsProtected)
                {
                    worksheet.Unprotect(Password);
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
