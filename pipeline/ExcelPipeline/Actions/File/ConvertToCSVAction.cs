using ClosedXML.Excel;
using System.Text;

namespace ExcelPipeline.Actions.File
{
    public class ConvertToCSVAction : ActionBase
    {
        public string SheetName { get; set; } = "";
        public string OutputPath { get; set; }
        public string Delimiter { get; set; } = ",";

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = string.IsNullOrEmpty(SheetName)
                    ? workbook.Worksheets.First()
                    : workbook.Worksheet(SheetName);

                var csvContent = new StringBuilder();
                var usedRange = worksheet.RangeUsed();

                if (usedRange != null)
                {
                    for (int row = 1; row <= usedRange.LastRow().RowNumber(); row++)
                    {
                        var rowValues = new List<string>();
                        for (int col = 1; col <= usedRange.LastColumn().ColumnNumber(); col++)
                        {
                            var cellValue = worksheet.Cell(row, col).Value.ToString();
                            rowValues.Add(cellValue);
                        }
                        csvContent.AppendLine(string.Join(Delimiter, rowValues));
                    }
                }

                System.IO.File.WriteAllText(OutputPath, csvContent.ToString());
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}
