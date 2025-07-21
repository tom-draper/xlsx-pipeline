using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class CopySheetAction : ActionBase
{
    public required string SourceSheetName { get; set; }
    public required string NewSheetName { get; set; }
    public string? DestinationFilePath { get; set; } // Optional

    protected override Task ExecuteInternalAsync(string sourceFilePath)
    {
        try
        {
            using var sourceWorkbook = new XLWorkbook(sourceFilePath);
            var sourceSheet = sourceWorkbook.Worksheet(SourceSheetName);

            if (sourceSheet == null)
                throw new ArgumentException($"Worksheet '{SourceSheetName}' not found in '{sourceFilePath}'.");

            if (string.IsNullOrWhiteSpace(DestinationFilePath) || DestinationFilePath == sourceFilePath)
            {
                // Copy within the same workbook
                sourceSheet.CopyTo(NewSheetName);
                sourceWorkbook.Save();
            }
            else
            {
                // Copy to a different workbook
                using var destWorkbook = System.IO.File.Exists(DestinationFilePath)
                    ? new XLWorkbook(DestinationFilePath)
                    : new XLWorkbook();

                // Prevent name collision
                if (destWorkbook.Worksheets.Any(ws => ws.Name == NewSheetName))
                    throw new ArgumentException($"Worksheet '{NewSheetName}' already exists in destination workbook.");

                // Add a copy of the sheet to the destination workbook
                sourceSheet.CopyTo(destWorkbook, NewSheetName);
                destWorkbook.SaveAs(DestinationFilePath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}
