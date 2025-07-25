using ClosedXML.Excel;

namespace XLSXPipeline.Actions.File;

public class ProtectFileAction : ActionBase
{
    public required string Password { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            ValidateInputs(filePath);
            ProtectWorkbook(filePath);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to protect file: {ex.Message}", ex);
        }
    }

    private void ValidateInputs(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Source file path cannot be null or empty");

        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException($"Source file not found: {filePath}");
    }

    private void ProtectWorkbook(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        ValidateWorkbookNotProtected(workbook);
        ApplyProtection(workbook);
        workbook.Save();
    }

    private static void ValidateWorkbookNotProtected(XLWorkbook workbook)
    {
        if (workbook.IsProtected)
            throw new InvalidOperationException("Workbook is already protected.");
    }

    private void ApplyProtection(XLWorkbook workbook)
    {
        workbook.Protect(Password);
    }
}