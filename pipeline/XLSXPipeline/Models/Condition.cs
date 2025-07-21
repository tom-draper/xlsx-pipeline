using ClosedXML.Excel;
using XLSXPipeline.Actions;

namespace XLSXPipeline.Models
{
    public class IfAction : ActionBase
    {
        public required Condition Condition { get; set; }
        public required List<ActionBase> Then { get; set; }
        public List<ActionBase>? Else { get; set; }

        protected override async Task ExecuteInternalAsync(string filePath)
        {
            if (Condition.Evaluate(filePath))
            {
                foreach (var action in Then)
                    await action.ExecuteAsync(filePath);
            }
            else if (Else != null)
            {
                foreach (var action in Else)
                    await action.ExecuteAsync(filePath);
            }
        }
    }

    public abstract class Condition
    {
        public required string Type { get; set; }
        public abstract bool Evaluate(string filePath);
    }

    public class SheetExistsCondition : Condition
    {
        public required string SheetName { get; set; }

        public override bool Evaluate(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);
            return workbook.Worksheets.Contains(SheetName);
        }
    }

    public class AndCondition : Condition
    {
        public required List<Condition> Conditions { get; set; }

        public override bool Evaluate(string filePath)
        {
            return Conditions.All(c => c.Evaluate(filePath));
        }
    }

    public class OrCondition : Condition
    {
        public required List<Condition> Conditions { get; set; }

        public override bool Evaluate(string filePath)
        {
            return Conditions.Any(c => c.Evaluate(filePath));
        }
    }

}
