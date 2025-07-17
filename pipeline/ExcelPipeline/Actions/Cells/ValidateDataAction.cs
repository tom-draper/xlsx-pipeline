using ClosedXML.Excel;

namespace ExcelPipeline.Actions.Cells
{
    public class ValidateDataAction : ActionBase
    {
        public string SheetName { get; set; } = "";
        public string Range { get; set; }
        public string ValidationType { get; set; } = "List";
        public string ValidationCriteria { get; set; }
        public string ErrorMessage { get; set; }

        public override Task ExecuteAsync(string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = string.IsNullOrEmpty(SheetName)
                    ? workbook.Worksheets.First()
                    : workbook.Worksheet(SheetName);

                var range = worksheet.Range(Range);

                switch (ValidationType.ToLower())
                {
                    case "list":
                        range.GetDataValidation().List(ValidationCriteria);
                        break;
                    case "whole":
                        range.GetDataValidation().WholeNumber.Between(int.Parse(ValidationCriteria.Split(',')[0]), int.Parse(ValidationCriteria.Split(',')[1]));
                        break;
                    case "decimal":
                        range.GetDataValidation().Decimal.Between(double.Parse(ValidationCriteria.Split(',')[0]), double.Parse(ValidationCriteria.Split(',')[1]));
                        break;
                }

                if (!string.IsNullOrEmpty(ErrorMessage))
                    range.GetDataValidation().ErrorMessage = ErrorMessage;

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