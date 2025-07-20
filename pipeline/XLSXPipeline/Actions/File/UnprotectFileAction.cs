using ClosedXML.Excel;

namespace XLSXPipeline.Actions.File
{
    public class UnprotectFileAction : ActionBase
    {
        public required string Password { get; set; }

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);

                if (!workbook.IsProtected)
                    return Task.CompletedTask;

                workbook.Unprotect(Password);
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
