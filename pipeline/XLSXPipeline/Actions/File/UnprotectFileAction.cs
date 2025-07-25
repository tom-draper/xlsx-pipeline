using ClosedXML.Excel;

namespace XLSXPipeline.Actions.File;

public class UnprotectFileAction : ActionBase
{
    public required string Password { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            ValidateInputs(filePath);
            UnprotectWorkbook(filePath);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to unprotect file: {ex.Message}", ex);
        }
    }

    private void ValidateInputs(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Source file path cannot be null or empty");

        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException($"Source file not found: {filePath}");

        Validation.ValidatePassword(Password);
    }

    private void UnprotectWorkbook(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        if (!workbook.IsProtected)
            return;

        RemoveProtection(workbook);
        workbook.Save();
    }

    private void RemoveProtection(XLWorkbook workbook)
    {
        workbook.Unprotect(Password);
    }
}