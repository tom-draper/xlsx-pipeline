using ClosedXML.Excel;

namespace XLSXPipeline.Actions;

internal class Helpers
{
    public static IXLWorksheet GetWorksheet(XLWorkbook workbook, string sheetName)
    {
        var worksheet = workbook.Worksheet(sheetName);
        if (worksheet == null)
            throw new InvalidOperationException($"Sheet '{sheetName}' does not exist in workbook.");
        return worksheet;
    }

    public static IXLWorksheet GetWorksheetOrFirst(XLWorkbook workbook, string? sheetName)
    {
        if (string.IsNullOrEmpty(sheetName))
        {
            if (workbook.Worksheets.Count == 0)
                throw new InvalidOperationException("No worksheets found in the workbook.");
            return workbook.Worksheets.First();
        }

        return GetWorksheet(workbook, sheetName);
    }

    public static XLWorkbook GetOrCreateWorkbook(string filepath)
    {
        return System.IO.File.Exists(filepath)
            ? new XLWorkbook(filepath)
            : new XLWorkbook();
    }

    public static IXLWorksheet GetOrCreateWorksheet(XLWorkbook workbook, string? sheetName)
    {
        if (string.IsNullOrEmpty(sheetName))
        {
            if (workbook.Worksheets.Count == 0)
                return workbook.Worksheets.Add("Sheet1");
            return workbook.Worksheets.First();
        }
        var worksheet = workbook.Worksheet(sheetName);
        if (worksheet == null)
            worksheet = workbook.Worksheets.Add(sheetName);
        return worksheet;
    }

    public static string DetermineOutputPath(string sourceFilePath, string fileExtension, string? outputPath, string? filename)
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
            outputFilename = Path.HasExtension(filename) ? filename : filename + "." + fileExtension.Replace(".", string.Empty);
        }
        else
        {
            // Default to source filename with .csv extension
            var sourceFileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            outputFilename = sourceFileName + "." + fileExtension.Replace(".", string.Empty);
        }

        var fullOutputPath = Path.Combine(outputDirectory, outputFilename);
        var resolvedPath = Path.GetFullPath(fullOutputPath);

        // Ensure the output directory exists
        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return resolvedPath;
    }

    public static string ReplaceDateTimePlaceholders(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var now = DateTime.Now;

        return input
            .Replace("{year}", now.Year.ToString("D4"))
            .Replace("{month}", now.Month.ToString("D2"))
            .Replace("{day}", now.Day.ToString("D2"))
            .Replace("{hour}", now.Hour.ToString("D2"))
            .Replace("{minute}", now.Minute.ToString("D2"))
            .Replace("{second}", now.Second.ToString("D2"));
    }

    public static void EnsureDirectory(string filepath, bool createDirectories)
    {
        var directory = Path.GetDirectoryName(ReplaceDateTimePlaceholders(filepath));
        if (string.IsNullOrEmpty(directory) || Directory.Exists(directory))
            return;

        if (createDirectories)
            Directory.CreateDirectory(directory);
        else
            throw new DirectoryNotFoundException($"Directory does not exist: {directory}");
    }

    public static string DetermineDestinationFilePath(string sourcePath, string destinationPath, bool appendSourceExtension, string? filename = null)
    {
        // If filename is provided, use it to override any filename in the destination path
        if (!string.IsNullOrWhiteSpace(filename))
        {
            string directory;

            // Determine the directory from destinationPath
            if (IsDestinationDirectory(destinationPath) || IsExistingDirectory(destinationPath))
            {
                directory = destinationPath;
            }
            else if (HasFileExtension(destinationPath) || IsFileInExistingDirectory(destinationPath))
            {
                directory = Path.GetDirectoryName(destinationPath) ?? string.Empty;
            }
            else
            {
                // Treat as directory if it doesn't look like a file path
                directory = LooksLikeFilePath(destinationPath) ? Path.GetDirectoryName(destinationPath) ?? string.Empty : destinationPath;
            }

            // Use the provided filename, appending source extension if needed
            string finalFilename = AppendExtensionIfNeeded(sourcePath, filename, appendSourceExtension);
            return Path.Combine(directory, finalFilename);
        }

        // Original logic when no filename is provided
        if (IsDestinationDirectory(destinationPath))
            return CombineWithSourceFileName(sourcePath, destinationPath);

        if (HasFileExtension(destinationPath))
            return destinationPath;

        if (IsExistingDirectory(destinationPath))
            return CombineWithSourceFileName(sourcePath, destinationPath);

        if (IsFileInExistingDirectory(destinationPath))
            return AppendExtensionIfNeeded(sourcePath, destinationPath, appendSourceExtension);

        return DetermineByPathStructure(sourcePath, destinationPath, appendSourceExtension);
    }

    private static bool IsDestinationDirectory(string destinationPath)
    {
        return destinationPath.EndsWith(Path.DirectorySeparatorChar) ||
               destinationPath.EndsWith(Path.AltDirectorySeparatorChar);
    }

    private static bool HasFileExtension(string destinationPath)
    {
        return Path.HasExtension(destinationPath);
    }

    private static bool IsExistingDirectory(string destinationPath)
    {
        return Directory.Exists(destinationPath);
    }

    private static bool IsFileInExistingDirectory(string destinationPath)
    {
        var parentDir = Path.GetDirectoryName(destinationPath);
        return !string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir);
    }

    private static string CombineWithSourceFileName(string sourcePath, string destinationPath)
    {
        var fileName = Path.GetFileName(sourcePath);
        return Path.Combine(destinationPath, fileName);
    }

    private static string AppendExtensionIfNeeded(string sourceFilePath, string destinationPath, bool appendSourceExtension)
    {
        if (!appendSourceExtension)
            return destinationPath;

        var sourceExtension = Path.GetExtension(sourceFilePath);
        return destinationPath + sourceExtension;
    }

    private static string DetermineByPathStructure(string sourceFilePath, string destinationPath, bool appendSourceExtension)
    {
        if (LooksLikeFilePath(destinationPath))
            return AppendExtensionIfNeeded(sourceFilePath, destinationPath, appendSourceExtension);

        return CombineWithSourceFileName(sourceFilePath, destinationPath);
    }

    private static bool LooksLikeFilePath(string path)
    {
        return Path.IsPathRooted(path) &&
               (path.Contains(Path.DirectorySeparatorChar) ||
                path.Contains(Path.AltDirectorySeparatorChar));
    }
    public static int GetColumnNumberFromLetter(string columnLetter)
    {
        if (string.IsNullOrWhiteSpace(columnLetter))
            throw new ArgumentException("Column letter cannot be null or whitespace.", nameof(columnLetter));

        columnLetter = columnLetter.Trim().ToUpperInvariant();
        int sum = 0;

        for (int i = 0; i < columnLetter.Length; i++)
        {
            char c = columnLetter[i];

            if (c < 'A' || c > 'Z')
                throw new ArgumentException($"Invalid character '{c}' in column letter.", nameof(columnLetter));

            sum *= 26;
            sum += (c - 'A' + 1);
        }

        return sum;
    }
 }
