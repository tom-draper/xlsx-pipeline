using ClosedXML.Excel;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace XLSXPipeline.Actions.File;

public class ExportToPDFAction : ActionBase
{
    public required string OutputPath { get; set; }
    public string? FileName { get; set; }
    public string? SheetName { get; set; }
    public PdfPaperSize PaperSize { get; set; } = PdfPaperSize.A4;
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;
    public bool FitToPage { get; set; } = true;
    public float MarginLeft { get; set; } = 36f; // Points (0.5 inch)
    public float MarginRight { get; set; } = 36f;
    public float MarginTop { get; set; } = 72f; // Points (1 inch)
    public float MarginBottom { get; set; } = 72f;
    public bool ShowGridlines { get; set; } = true;
    public string? Header { get; set; }
    public string? Footer { get; set; }
    public float BaseFontSize { get; set; } = 10f;
    public bool ReplaceFile { get; set; } = false;

    protected override async Task ExecuteInternalAsync(string filePath)
    {
        try
        {
            ValidateInputs(filePath);

            using var workbook = new XLWorkbook(filePath);
            var worksheet = GetWorksheet(workbook);
            var outputPath = DetermineOutputPath(filePath, OutputPath, FileName);

            await CreatePdfAsync(worksheet, outputPath);

            if (ReplaceFile)
                System.IO.File.Delete(filePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to convert to PDF: {ex.Message}", ex);
        }
    }

    private void ValidateInputs(string filePath)
    {
        if (string.IsNullOrWhiteSpace(OutputPath))
            throw new ArgumentException("Output path cannot be null or empty", nameof(OutputPath));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Source file path cannot be null or empty");

        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException($"Source file not found: {filePath}");
    }

    public static string DetermineOutputPath(string sourceFilePath, string? outputPath, string? filename)
    {
        string outputDirectory;
        string outputFilename;

        // Determine the output directory
        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            if (Path.HasExtension(outputPath))
                // OutputPath includes a filename, extract directory and ignore the filename part
                outputDirectory = Path.GetDirectoryName(outputPath) ?? string.Empty;
            else
                // OutputPath is just a directory
                outputDirectory = outputPath;
        }
        else
        {
            // Default to same directory as source file
            outputDirectory = Path.GetDirectoryName(sourceFilePath) ?? string.Empty;
        }

        // Determine the filename
        if (!string.IsNullOrWhiteSpace(filename))
        {
            // Use the explicitly provided FileName
            outputFilename = Path.HasExtension(filename) ? filename : filename + ".pdf";
        }
        else
        {
            // Default to source filename with .pdf extension
            var sourceFileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            outputFilename = sourceFileName + ".pdf";
        }

        var fullOutputPath = Path.Combine(outputDirectory, outputFilename);
        var resolvedPath = Path.GetFullPath(fullOutputPath);

        // Ensure the output directory exists
        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return resolvedPath;
    }

    private IXLWorksheet GetWorksheet(XLWorkbook workbook)
    {
        if (string.IsNullOrEmpty(SheetName))
            return workbook.Worksheets.First();

        var worksheet = workbook.Worksheet(SheetName);
        if (worksheet == null)
            throw new InvalidOperationException($"Sheet '{SheetName}' does not exist.");

        return worksheet;
    }

    private async Task CreatePdfAsync(IXLWorksheet worksheet, string outputPath)
    {
        await Task.Run(() =>
        {
            using var writer = new PdfWriter(outputPath);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, GetPageSize());

            ConfigureDocument(document);
            AddContent(document, worksheet);
        });
    }

    private void ConfigureDocument(Document document)
    {
        document.SetMargins(MarginTop, MarginRight, MarginBottom, MarginLeft);

        if (!string.IsNullOrEmpty(Header))
        {
            var headerParagraph = new Paragraph(Header)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12);
            document.Add(headerParagraph);
        }
    }

    private void AddContent(Document document, IXLWorksheet worksheet)
    {
        var usedRange = worksheet.RangeUsed();
        if (usedRange == null) return;

        int cols = usedRange.ColumnCount();

        var table = new Table(cols);

        if (FitToPage)
            table.SetWidth(UnitValue.CreatePercentValue(100));

        ConfigureTableBorders(table);
        PopulateTable(table, worksheet, usedRange);

        document.Add(table);

        if (!string.IsNullOrEmpty(Footer))
        {
            var footerParagraph = new Paragraph(Footer)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10)
                .SetMarginTop(20);
            document.Add(footerParagraph);
        }
    }

    private void ConfigureTableBorders(Table table)
    {
        if (ShowGridlines)
            table.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));
        else
            table.SetBorder(Border.NO_BORDER);
    }

    private void PopulateTable(Table table, IXLWorksheet worksheet, IXLRange usedRange)
    {
        int startRow = usedRange.FirstRow().RowNumber();
        int endRow = usedRange.LastRow().RowNumber();
        int startCol = usedRange.FirstColumn().ColumnNumber();
        int endCol = usedRange.LastColumn().ColumnNumber();

        for (int row = startRow; row <= endRow; row++)
        {
            for (int col = startCol; col <= endCol; col++)
            {
                var xlCell = worksheet.Cell(row, col);
                var pdfCell = CreatePdfCell(xlCell);
                table.AddCell(pdfCell);
            }
        }
    }

    private Cell CreatePdfCell(IXLCell xlCell)
    {
        var cellValue = GetCellValueAsString(xlCell);
        var cell = new Cell().Add(new Paragraph(cellValue));

        ApplyCellFormatting(cell, xlCell);
        ConfigureCellBorders(cell);

        return cell;
    }

    private void ApplyCellFormatting(Cell pdfCell, IXLCell xlCell)
    {
        var paragraph = (Paragraph?)pdfCell.GetChildren().FirstOrDefault();
        if (paragraph == null) return;

        // Font size
        float fontSize = (float)(xlCell.Style.Font.FontSize > 0 ? xlCell.Style.Font.FontSize : BaseFontSize);
        paragraph.SetFontSize(fontSize);

        // Font style
        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        if (xlCell.Style.Font.Bold && xlCell.Style.Font.Italic)
            font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLDOBLIQUE);
        else if (xlCell.Style.Font.Bold)
            font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        else if (xlCell.Style.Font.Italic)
            font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

        paragraph.SetFont(font);

        // Font color
        if (xlCell.Style.Font.FontColor.ColorType == XLColorType.Color)
        {
            var color = xlCell.Style.Font.FontColor.Color;
            paragraph.SetFontColor(new DeviceRgb(color.R, color.G, color.B));
        }

        // Background color
        if (xlCell.Style.Fill.BackgroundColor.ColorType == XLColorType.Color)
        {
            var bgColor = xlCell.Style.Fill.BackgroundColor.Color;
            pdfCell.SetBackgroundColor(new DeviceRgb(bgColor.R, bgColor.G, bgColor.B));
        }

        // Text alignment
        var alignment = xlCell.Style.Alignment.Horizontal switch
        {
            XLAlignmentHorizontalValues.Center => TextAlignment.CENTER,
            XLAlignmentHorizontalValues.Right => TextAlignment.RIGHT,
            XLAlignmentHorizontalValues.Left => TextAlignment.LEFT,
            _ => TextAlignment.LEFT
        };
        paragraph.SetTextAlignment(alignment);

        // Padding
        pdfCell.SetPadding(3);
    }

    private void ConfigureCellBorders(Cell cell)
    {
        if (ShowGridlines)
            cell.SetBorder(new SolidBorder(ColorConstants.GRAY, 0.5f));
        else
            cell.SetBorder(Border.NO_BORDER);
    }

    private iText.Kernel.Geom.PageSize GetPageSize()
    {
        var size = PaperSize switch
        {
            PdfPaperSize.A3 => iText.Kernel.Geom.PageSize.A3,
            PdfPaperSize.A4 => iText.Kernel.Geom.PageSize.A4,
            PdfPaperSize.A5 => iText.Kernel.Geom.PageSize.A5,
            PdfPaperSize.Letter => iText.Kernel.Geom.PageSize.LETTER,
            PdfPaperSize.Legal => iText.Kernel.Geom.PageSize.LEGAL,
            PdfPaperSize.Tabloid => iText.Kernel.Geom.PageSize.TABLOID,
            _ => iText.Kernel.Geom.PageSize.A4
        };

        return Orientation == PageOrientation.Landscape ? size.Rotate() : size;
    }

    private static string GetCellValueAsString(IXLCell cell)
    {
        if (cell.IsEmpty())
            return "";

        return cell.DataType switch
        {
            XLDataType.DateTime => cell.GetDateTime().ToString("yyyy-MM-dd HH:mm:ss"),
            XLDataType.TimeSpan => cell.GetTimeSpan().ToString(),
            XLDataType.Boolean => cell.GetBoolean().ToString(),
            XLDataType.Number => cell.GetDouble().ToString("F2"),
            _ => cell.GetString()
        };
    }
}

public enum PageOrientation
{
    Portrait,
    Landscape
}

public enum PdfPaperSize
{
    A3,
    A4,
    A5,
    Letter,
    Legal,
    Tabloid
}