using System.Text.Json.Serialization;
using XLSXPipeline.Models;
using XLSXPipeline.Actions;
using XLSXPipeline.Actions.Data;
using XLSXPipeline.Actions.File;
using XLSXPipeline.Actions.Worksheet;
using XLSXPipeline.Actions.Time;
using XLSXPipeline.Actions.Formatting;
using XLSXPipeline.Actions.Cells;
using XLSXPipeline.Actions.Advanced;

namespace XLSXPipeline.Services;

[JsonSerializable(typeof(Pipeline))]
[JsonSerializable(typeof(Trigger))]
[JsonSerializable(typeof(ScheduledPipeline))]
[JsonSerializable(typeof(FileWatcherPipeline))]
[JsonSerializable(typeof(List<Pipeline>))]
[JsonSerializable(typeof(List<ScheduledPipeline>))]
[JsonSerializable(typeof(List<FileWatcherPipeline>))]
// Base action types
[JsonSerializable(typeof(ActionBase))]
[JsonSerializable(typeof(List<ActionBase>))]
// File actions
[JsonSerializable(typeof(ExportToCSVAction))]
[JsonSerializable(typeof(ExportToPDFAction))]
[JsonSerializable(typeof(CopyFileAction))]
[JsonSerializable(typeof(MoveFileAction))]
[JsonSerializable(typeof(OpenFileAction))]
[JsonSerializable(typeof(ProtectFileAction))]
[JsonSerializable(typeof(RenameFileAction))]
[JsonSerializable(typeof(UnprotectFileAction))]
// Worksheet actions
[JsonSerializable(typeof(AddSheetAction))]
[JsonSerializable(typeof(CopySheetAction))]
[JsonSerializable(typeof(DeleteHiddenSheetsAction))]
[JsonSerializable(typeof(DeleteSheetAction))]
[JsonSerializable(typeof(MoveSheetAction))]
[JsonSerializable(typeof(ProtectSheetAction))]
[JsonSerializable(typeof(RenameSheetAction))]
[JsonSerializable(typeof(ReplaceSheetAction))]
[JsonSerializable(typeof(UnprotectSheetAction))]
// Data actions
[JsonSerializable(typeof(CopyColumnAction))]
[JsonSerializable(typeof(CopyRowAction))]
[JsonSerializable(typeof(DeleteColumnAction))]
[JsonSerializable(typeof(DeleteRowAction))]
[JsonSerializable(typeof(FilterDataAction))]
[JsonSerializable(typeof(InsertColumnAction))]
[JsonSerializable(typeof(InsertRowAction))]
[JsonSerializable(typeof(MergeDataAction))]
[JsonSerializable(typeof(MoveColumnAction))]
[JsonSerializable(typeof(MoveRowAction))]
[JsonSerializable(typeof(SortDataAction))]
// Time actions
[JsonSerializable(typeof(WaitAction))]
// Formatting actions
[JsonSerializable(typeof(FormatCellsAction))]
// Cell actions
[JsonSerializable(typeof(ApplyFormulaAction))]
[JsonSerializable(typeof(SetCellValueAction))]
[JsonSerializable(typeof(ValidateDataAction))]
// Advanced actions
[JsonSerializable(typeof(CreatePivotTableAction))]
[JsonSerializable(typeof(TransposeAction))]
public partial class PipelineJsonContext : JsonSerializerContext
{
}