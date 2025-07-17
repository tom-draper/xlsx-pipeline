using ClosedXML.Excel;

namespace ExcelPipeline.Actions.Formatting
{
    public class FormatCellsAction : ActionBase
    {
        public string SheetName { get; set; } = "";
        public string Range { get; set; }
        public string NumberFormat { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public string BackgroundColor { get; set; }
        public string FontColor { get; set; }

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = string.IsNullOrEmpty(SheetName)
                    ? workbook.Worksheets.First()
                    : workbook.Worksheet(SheetName);

                var range = worksheet.Range(Range);

                if (!string.IsNullOrEmpty(NumberFormat))
                    range.Style.NumberFormat.Format = NumberFormat;

                if (!string.IsNullOrEmpty(FontName))
                    range.Style.Font.FontName = FontName;

                if (FontSize > 0)
                    range.Style.Font.FontSize = FontSize;

                if (Bold)
                    range.Style.Font.Bold = true;

                if (Italic)
                    range.Style.Font.Italic = true;

                if (!string.IsNullOrEmpty(BackgroundColor))
                    range.Style.Fill.BackgroundColor = XLColor.FromName(BackgroundColor);

                if (!string.IsNullOrEmpty(FontColor))
                    range.Style.Font.FontColor = XLColor.FromName(FontColor);

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