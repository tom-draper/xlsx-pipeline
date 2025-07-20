using ClosedXML.Excel;

namespace XLSXPipeline.Actions.File
{
    public class ProtectFileAction : ActionBase
    {
        public required string Password { get; set; }

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);

                if (workbook.IsProtected)
                    throw new InvalidOperationException("Workbook is already protected.");

                workbook.Protect(Password);
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
