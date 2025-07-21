using ClosedXML.Excel;

namespace XLSXPipeline.Actions.Advanced;

public class TransposeAction : ActionBase
{
    public required string SourceSheetName { get; set; }
    public required string SourceRange { get; set; }
    public required string DestinationSheetName { get; set; }
    public required string DestinationCell { get; set; }

    protected override Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);

            var sourceSheet = workbook.Worksheet(SourceSheetName);
            if (sourceSheet == null)
                throw new ArgumentException($"Source sheet '{SourceSheetName}' not found.");

            var destSheet = workbook.Worksheet(DestinationSheetName);
            if (destSheet == null)
                // Create the destination sheet if it doesn't exist
                destSheet = workbook.Worksheets.Add(DestinationSheetName);

            var sourceRange = sourceSheet.Range(SourceRange);
            var destinationCell = destSheet.Cell(DestinationCell);

            // Get the data from the source range
            var data = sourceRange.CellsUsed(); // Only get cells with content

            // Transpose the data
            // Determine the dimensions of the source data
            var rowCount = sourceRange.RowCount();
            var columnCount = sourceRange.ColumnCount();

            for (int r = 1; r <= rowCount; r++)
            {
                for (int c = 1; c <= columnCount; c++)
                {
                    var sourceCell = sourceRange.Cell(r, c);
                    if (!sourceCell.IsEmpty()) // Only copy if the source cell is not empty
                    {
                        // The transposed cell will be at (destinationCell.Row + c - 1, destinationCell.Column + r - 1)
                        destSheet.Cell(destinationCell.WorksheetRow().RowNumber() + c - 1, destinationCell.WorksheetColumn().ColumnNumber() + r - 1).Value = sourceCell.Value;
                    }
                }
            }

            workbook.Save();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }
}