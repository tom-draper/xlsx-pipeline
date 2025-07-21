using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Worksheet;

public class RenameSheetAction : ActionBase
{
    public required string OriginalName { get; set; }
    public required string NewName { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
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