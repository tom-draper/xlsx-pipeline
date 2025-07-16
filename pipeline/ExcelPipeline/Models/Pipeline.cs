using System.Text.Json.Serialization;
using ClosedXML.Excel;

namespace ExcelPipeline.Models
{
    public class Pipeline
    {
        public string PipelineName { get; set; }
        public Trigger Trigger { get; set; }
        public List<ActionBase> Actions { get; set; }
    }

    public class Trigger
    {
        public string Type { get; set; }
        public string Path { get; set; }
    }

    [JsonConverter(typeof(ActionJsonConverter))]
    public abstract class ActionBase
    {
        public string Type { get; set; }
        public abstract Task ExecuteAsync(string filePath);
    }

    public class RenameSheetAction : ActionBase
    {
        public string OriginalName { get; set; }
        public string NewName { get; set; }

        public override Task ExecuteAsync(string filePath)
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

    public class MoveColumnAction : ActionBase
    {
        public string From { get; set; }
        public string To { get; set; }

        public override Task ExecuteAsync(string filePath)
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

    public class CopyFileAction : ActionBase
    {
        public string DestinationPath { get; set; }

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var destinationFilePath = Path.Combine(DestinationPath, fileName);
                File.Copy(filePath, destinationFilePath, true);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }

        }
    }

    public class ScheduledPipeline
    {
        public Pipeline Pipeline { get; set; } = null!;
        public string FilePath { get; set; } = string.Empty;
        public string TriggerType { get; set; } = string.Empty;
        public DateTime NextRunTime { get; set; }
        public bool IsRecurring { get; set; }
        public TimeSpan RecurrenceInterval { get; set; }
        public bool IsMonthly { get; set; }
        public bool IsCron { get; set; }
        public string CronExpression { get; set; } = string.Empty;
    }

    public class FileWatcherPipeline
    {
        public Pipeline Pipeline { get; set; } = null!;
        public string FilePath { get; set; } = string.Empty;
        public string WatchPath { get; set; } = string.Empty;
    }
}
