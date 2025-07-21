using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Data;

public class MoveColumnAction : ActionBase
{
    public required string From { get; set; }
    public required string To { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.First();
            var columnToMove = worksheet.Column(From);
            columnToMove.CopyTo(worksheet.Column(To));
            columnToMove.Delete();
            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}