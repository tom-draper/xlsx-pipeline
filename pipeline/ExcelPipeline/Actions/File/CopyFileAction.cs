namespace ExcelPipeline.Actions.File
{
    public class CopyFileAction : ActionBase
    {
        public string DestinationPath { get; set; }
        
        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var destinationFilePath = Path.Combine(DestinationPath, fileName);
                System.IO.File.Copy(filePath, destinationFilePath, true);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}
