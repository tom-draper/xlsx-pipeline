using ClosedXML.Excel;

namespace XLSXPipeline.Actions.File;

public class ProtectFileAction : ActionBase
{
    public required string Password { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Source file path cannot be null or empty");

            // Validate source file exists
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException($"Source file not found: {filePath}");

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
