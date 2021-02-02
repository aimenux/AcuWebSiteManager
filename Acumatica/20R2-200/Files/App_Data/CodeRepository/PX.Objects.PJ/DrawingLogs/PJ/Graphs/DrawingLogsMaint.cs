using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.Services;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using AttributeProperties = PX.Objects.CN.Common.Descriptor.Constants.AttributeProperties;

namespace PX.Objects.PJ.DrawingLogs.PJ.Graphs
{
    public class DrawingLogsMaint : PXGraph<DrawingLogsMaint>, PXImportAttribute.IPXPrepareItems
    {
        [PXHidden]
        [PXCheckCurrent]
        public PXSetup<DrawingLogSetup> Setup;

        public PXFilter<DrawingLogFilter> Filter;

        [PXImport(typeof(DrawingLogFilter))]
        public SelectFrom<DrawingLog>
            .InnerJoin<PMProject>.On<DrawingLog.projectId.IsEqual<PMProject.contractID>>
            .Where<Brackets<DrawingLog.projectId.IsEqual<DrawingLogFilter.projectId.FromCurrent>
                    .Or<DrawingLogFilter.projectId.FromCurrent.IsNull>>
                .And<DrawingLog.disciplineId.IsEqual<DrawingLogFilter.disciplineId.FromCurrent>
                    .Or<DrawingLogFilter.disciplineId.FromCurrent.IsNull>>
                .And<DrawingLog.projectTaskId.IsEqual<DrawingLogFilter.projectTaskId.FromCurrent>
                    .Or<DrawingLogFilter.projectTaskId.FromCurrent.IsNull>>
                .And<DrawingLog.isCurrent.IsEqual<DrawingLogFilter.isCurrentOnly.FromCurrent>
                    .Or<DrawingLogFilter.isCurrentOnly.FromCurrent.IsEqual<False>>>>
            .OrderBy<Desc<PMProject.contractCD>>.View DrawingLogs;

        public PXSave<DrawingLogFilter> Save;
        public PXCancel<DrawingLogFilter> Cancel;
        public PXAction<DrawingLogFilter> InsertDrawingLog;
        public PXAction<DrawingLogFilter> InsertDrawingLogInGrid;
        public PXAction<DrawingLogFilter> CreateIssue;
        public PXAction<DrawingLogFilter> CreateNewRequestForInformation;
        public PXAction<DrawingLogFilter> CreateNewProjectIssue;
        public PXAction<DrawingLogFilter> DownloadZip;
        public PXAction<DrawingLogFilter> EditDrawingLog;

        private readonly DrawingLogZipService drawingLogZipService;

        public DrawingLogsMaint()
        {
            DrawingLogs.AllowInsert = Filter.Current.ProjectId.HasValue;
            drawingLogZipService = new DrawingLogZipService(DrawingLogs.Cache, Filter.Current?.IsCurrentOnly);
            ShouldDrawingLogBeInserted = true;
            var importAttribute = DrawingLogs.GetAttribute<PXImportAttribute>();
            importAttribute.MappingPropertiesInit += AddMissingDrawingLogFields;
        }

        [InjectDependency]
        public IProjectDataProvider ProjectDataProvider
        {
            get;
            set;
        }

        [InjectDependency]
        public IDrawingLogDataProvider DrawingLogDataProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this field should prevent
        /// inserting non committed <see cref="DrawingLog"/> on updating <see cref="DrawingLogFilter"/>.
        /// </summary>
        private bool ShouldDrawingLogBeInserted
        {
            get;
            set;
        }

        [PXInsertButton]
        [PXUIField(DisplayName = "")]
        public virtual void insertDrawingLog()
        {
            var graph = CreateInstance<DrawingLogEntry>();
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
        }

        [PXInsertButton(CommitChanges = true)]
        [PXUIField(DisplayName = DrawingLogMessages.AddNewRow)]
        public virtual void insertDrawingLogInGrid()
        {
            DrawingLogs.Insert();
        }

        [PXUIField(DisplayName = "Download Zip",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable downloadZip(PXAdapter adapter)
        {
            var selectedDrawingLogs = GetSelectedDrawingLogs();
            var fileIds = drawingLogZipService.GetDocumentFileIds(selectedDrawingLogs);
            PXLongOperation.StartOperation(this, () =>
            {
                drawingLogZipService.DownloadZipFile(fileIds);
                Persist();
            });
            return adapter.Get();
        }

        [PXUIField(DisplayName = "Create")]
        [PXButton]
        public virtual void createIssue()
        {
        }

        [PXUIField(DisplayName = "NEW RFI",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton(CommitChanges = true)]
        public virtual void createNewRequestForInformation()
        {
            var service = new RequestForInformationRedirectionService();
            CreateNewEntity(service, DrawingLogMessages.NewRequestForInformationWithDifferentProjects);
        }

        [PXUIField(DisplayName = "NEW Project Issue",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton(CommitChanges = true)]
        public virtual void createNewProjectIssue()
        {
            var service = new ProjectIssueRedirectionService();
            CreateNewEntity(service, DrawingLogMessages.NewProjectIssueWithDifferentProjects);
        }

        [PXButton]
        [PXUIField(DisplayName = "")]
        public virtual void editDrawingLog()
        {
            this.RedirectToEntity(DrawingLogs.Current, PXRedirectHelper.WindowMode.InlineWindow);
        }

        [PXCustomizeBaseAttribute(typeof(PXDBIdentityAttribute), AttributeProperties.IsKey, true)]
        public virtual void DrawingLog_DrawingLogId_CacheAttached(PXCache cache)
        {
        }

        [PXCustomizeBaseAttribute(typeof(PXDBStringAttribute), AttributeProperties.IsKey, false)]
        public virtual void DrawingLog_DrawingLogCd_CacheAttached(PXCache cache)
        {
        }

        public virtual void _(Events.RowUpdated<DrawingLogFilter> args)
        {
            ShouldDrawingLogBeInserted = false;
        }

        public virtual void _(Events.RowInserting<DrawingLog> args)
        {
            if (!ShouldDrawingLogBeInserted)
            {
                args.Cancel = true;
                ShouldDrawingLogBeInserted = true;
            }
        }

        public virtual void _(Events.RowSelected<DrawingLogFilter> args)
        {
            var doesDrawingLogsHaveActiveRequiredAttribute = DoesDrawingLogsHaveActiveRequiredAttribute();
            SetDrawingLogActionsAvailability(doesDrawingLogsHaveActiveRequiredAttribute);
            SetDrawingLogCreationActionTooltip(doesDrawingLogsHaveActiveRequiredAttribute);
        }

        public virtual void _(Events.RowSelected<DrawingLog> args)
        {
            var drawingLog = args.Row;
            if (drawingLog != null)
            {
                SetFieldsEnabled(args.Cache, drawingLog);
                DrawingLogs.AllowDelete = ProjectDataProvider.IsActiveProject(this, drawingLog.ProjectId);
            }
        }

        public virtual void _(Events.RowPersisting<DrawingLog> args)
        {
            var drawingLog = args.Row;
            args.Cache.ClearFieldErrors<DrawingLog.projectId>(drawingLog);
            args.Cache.ClearFieldErrors<DrawingLog.projectTaskId>(drawingLog);
            args.Cache.ClearFieldErrors<DrawingLog.disciplineId>(drawingLog);
            CheckIfAllRequiredAttributesAreSpecified(drawingLog.NoteID);
        }

        public virtual void _(Events.RowUpdating<DrawingLogFilter> args)
        {
            if (DrawingLogs.Cache.IsDirty && ShowCancelDialog() == WebDialogResult.No)
            {
                args.Cancel = true;
            }
            else
            {
                DrawingLogs.Cache.Clear();
                DrawingLogs.Cache.ClearQueryCache();
                DrawingLogs.View.Clear();
            }
        }

        public virtual void _(Events.RowPersisting<DrawingLogFilter> args)
        {
            ChangeFilterStatusToAvoidConfirmationDialogOnAttachFile(PXEntryStatus.Notchanged);
        }

        public virtual void _(Events.RowUpdating<DrawingLog> args)
        {
            ChangeFilterStatusToAvoidConfirmationDialogOnAttachFile(PXEntryStatus.Inserted);
        }

        public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches,
            string[] sortColumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows,
            ref int totalRows)
        {
            OverrideDisciplinesSelectIfRequired(viewName, sortColumns, ref searches);
            return base.ExecuteSelect(viewName, parameters, searches, sortColumns,
                descendings, filters, ref startRow, maximumRows, ref totalRows);
        }

        public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
        {
            if (viewName == nameof(DrawingLogs))
            {
                var drawingLogCd = ((string) values[nameof(DrawingLog.DrawingLogCd)])?.Trim();
                if (drawingLogCd == null)
                {
                    return true;
                }
                if (!drawingLogCd.Contains(Constants.DrawingLogCdSearchPattern))
                {
                    return false;
                }
                if (ShouldUpdateDrawingLogKey())
                {
                    keys[nameof(DrawingLog.DrawingLogId)] = DrawingLogDataProvider
                        .GetDrawingLog<DrawingLog.drawingLogCd>(drawingLogCd)?.DrawingLogId;
                }
            }
            return true;
        }

        public bool RowImporting(string viewName, object row)
        {
            return row == null;
        }

        public bool RowImported(string viewName, object row, object oldRow)
        {
            return oldRow == null;
        }

        public void PrepareItems(string viewName, IEnumerable items)
        {
        }

        public bool ShouldUpdateDrawingLogKey()
        {
            var importSettings = (PXImportAttribute.XLSXSettings) this.Caches<PXImportAttribute.XLSXSettings>().Current;
            return importSettings.Mode != PXImportAttribute.ImportMode.INSERT_ALL_RECORDS;
        }

        private void AddMissingDrawingLogFields(object cache, PXImportAttribute.MappingPropertiesInitEventArgs args)
        {
            args.Names.Add(nameof(DrawingLog.ProjectTaskId));
            args.DisplayNames.Add("Project Task");
            args.Names.Add(nameof(DrawingLog.OriginalDrawingId));
            args.DisplayNames.Add(DrawingLogLabels.OriginalDrawingId);
        }

        private void SetDrawingLogActionsAvailability(bool doesDrawingLogsHaveRequiredAttribute)
        {
            var isEnabled = IsProjectValid(Filter.Current.ProjectId) && !doesDrawingLogsHaveRequiredAttribute;
            InsertDrawingLogInGrid.SetEnabled(isEnabled);
            PXImportAttribute.SetEnabled(this, nameof(DrawingLogs), isEnabled);
        }

        private void SetDrawingLogCreationActionTooltip(bool doesDrawingLogsHaveRequiredAttribute)
        {
            var tooltip = doesDrawingLogsHaveRequiredAttribute
                ? DrawingLogMessages.AddNewRowIsNotAvailable
                : DrawingLogMessages.AddNewRow;
            InsertDrawingLogInGrid.SetTooltip(tooltip);
        }

        private bool IsProjectValid(int? projectId)
        {
            if (projectId == null)
            {
                return false;
            }
            var project = ProjectDataProvider.GetProject(this, projectId);
            return project.Status == ProjectStatus.Active || project.Status == ProjectStatus.Planned;
        }

        private WebDialogResult ShowCancelDialog()
        {
            return Filter.View.Ask(DrawingLogMessages.CancelDrawingLogChanges, MessageButtons.YesNo);
        }

        private List<DrawingLog> GetSelectedDrawingLogs()
        {
            return DrawingLogs.Select().FirstTableItems.Where(x => x.Selected == true).ToList();
        }

        private void CreateNewEntity<TRedirectionService>(TRedirectionService service, string message)
            where TRedirectionService : class, IRedirectionService, new()
        {
            Persist();
            var selectedDrawingLogs = GetSelectedDrawingLogs();
            CheckIfAnyDrawingLogIsSelected(selectedDrawingLogs);
            CheckIfDrawingLogsWithSameProjectsAreSelected(selectedDrawingLogs, message);
            service.RedirectToEntity(selectedDrawingLogs);
        }

        private void SetFieldsEnabled(PXCache cache, DrawingLog drawingLog)
        {
            DisableDrawingLogFieldIfNeeded<DrawingLog.projectTaskId>(
                Filter.Current.ProjectTaskId, cache, drawingLog);
            DisableDrawingLogFieldIfNeeded<DrawingLog.disciplineId>(
                Filter.Current.DisciplineId, cache, drawingLog);
            DisableDrawingLogIfAttributesRequired(cache, drawingLog);
            PXUIFieldAttribute.SetEnabled<DrawingLog.projectId>(cache, drawingLog, false);
            PXUIFieldAttribute.SetEnabled<DrawingLog.drawingLogCd>(cache, drawingLog, false);
        }

        private bool DoesDrawingLogsHaveActiveRequiredAttribute()
        {
            return GetActiveRequiredAttributes().Any();
        }

        private void DisableDrawingLogIfAttributesRequired(PXCache cache, BaseCache drawingLog)
        {
            if (AreThereAnyRequiredAttributeUnanswered(drawingLog.NoteID))
            {
                PXUIFieldAttribute.SetEnabled(cache, drawingLog, false);
                cache.RaiseException<DrawingLog.drawingLogCd>(drawingLog,
                    DrawingLogMessages.DisableDrawingLogWithAttributes, errorLevel: PXErrorLevel.RowWarning);
            }
        }

        private bool AreThereAnyRequiredAttributeUnanswered(Guid? noteId)
        {
            var numberOfActiveRequiredAttributes = GetActiveRequiredAttributes().Count();
            var numberOfActiveRequiredAnswers = GetAnswers()
                .Count(x => x.RefNoteID == noteId && x.Value != null && x.Value != string.Empty);
            return numberOfActiveRequiredAttributes != numberOfActiveRequiredAnswers;
        }

        private IQueryable<CSAnswers> GetAnswers()
        {
            return GetActiveRequiredAttributes()
                .Join(Select<CSAnswers>(), group => group.AttributeID, answers => answers.AttributeID,
                    (group, answers) => answers);
        }

        private IQueryable<CSAttributeGroup> GetActiveRequiredAttributes()
        {
            return Select<CSAttributeGroup>()
                .Where(attributeGroup => attributeGroup.EntityType == typeof(DrawingLog).FullName &&
                    attributeGroup.EntityClassID == Constants.DrawingLogClassId &&
                    attributeGroup.Required == true &&
                    attributeGroup.IsActive == true);
        }

        private static void OverrideDisciplinesSelectIfRequired(string viewName, string[] sortColumns, ref object[] searches)
        {
            if (viewName == Constants.DisciplineViewName ||
                viewName == Constants.DisciplineFilterViewName &&
                sortColumns.Contains(Constants.DisciplineNameField))
            {
                var index = sortColumns.FindIndex(column => column == Constants.DisciplineNameField);
                sortColumns[index] = Constants.DisciplineSortOrderField;
                searches = new[]
                {
                    (object) null
                };
            }
        }

        private static void CheckIfAnyDrawingLogIsSelected(IEnumerable<DrawingLog> drawingLogs)
        {
            if (drawingLogs.IsEmpty())
            {
                throw new Exception(DrawingLogMessages.NoRecordsWereSelected);
            }
        }

        private static void CheckIfDrawingLogsWithSameProjectsAreSelected(IEnumerable<DrawingLog> drawingLogs, string message)
        {
            if (drawingLogs
                .Distinct(dl => dl.ProjectId)
                .HasAtLeastTwoItems())
            {
                throw new PXException(message);
            }
        }

        private static void DisableDrawingLogFieldIfNeeded<TDrawingLogField>(int? filterValue, PXCache cache, object gridRow)
            where TDrawingLogField : IBqlField
        {
            if (filterValue != null)
            {
                PXUIFieldAttribute.SetEnabled<TDrawingLogField>(cache, gridRow, false);
            }
        }

        /// <summary>
        /// This method used for updating Filter status to avoid confirmation dialog on attempt to
        /// attach files to Drawing Log document. Confirmation dialog invokes because Filter view is a
        /// primary view for DrawingLogsMaint, so in case if Filter status not marked as PXEntryStatus.Notchanged
        /// confirmation dialog will be appear.
        /// </summary>
        private void ChangeFilterStatusToAvoidConfirmationDialogOnAttachFile(PXEntryStatus status)
        {
            Filter.Cache.SetStatus(Filter.Current, status);
        }

        private void CheckIfAllRequiredAttributesAreSpecified(Guid? noteId)
        {
            if (AreThereAnyRequiredAttributeUnanswered(noteId))
            {
                throw new PXException(DrawingLogMessages.DisableDrawingLogWithAttributes);
            }
        }
    }
}