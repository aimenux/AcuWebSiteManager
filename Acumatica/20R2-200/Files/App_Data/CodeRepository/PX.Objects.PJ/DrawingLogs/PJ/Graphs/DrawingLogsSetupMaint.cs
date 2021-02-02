using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.Descriptor.DataView;
using PX.Objects.PJ.DrawingLogs.PJ.Services;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Services;
using PX.Objects.CS;

namespace PX.Objects.PJ.DrawingLogs.PJ.Graphs
{
    public class DrawingLogsSetupMaint : StatusSetupMaintBase<DrawingLogsSetupMaint, DrawingLog, DrawingLogStatus>
    {
        public PXSave<DrawingLogSetup> Save;
        public PXCancel<DrawingLogSetup> Cancel;

        public PXSelect<DrawingLogSetup> DrawingLogSetup;

        public DisciplinesOrderedSelect<DrawingLogSetup, DrawingLogDiscipline,
            OrderBy<Asc<DrawingLogDiscipline.sortOrder>>> DrawingLogDisciplines;

        public PXSelect<DrawingLogStatus> DrawingLogStatuses;

        public PXSelectJoin<CSAttributeGroup,
            InnerJoin<CSAttribute, On<CSAttribute.attributeID, Equal<CSAttributeGroup.attributeID>>>,
            Where<CSAttributeGroup.entityType, Equal<DrawingLog.typeName>,
                And<CSAttributeGroup.entityClassID, Equal<DrawingLog.drawingLogClassId>>>,
            OrderBy<Asc<CSAttributeGroup.sortOrder>>> Attributes;

        public SelectFrom<CSAnswers>
            .RightJoin<DrawingLog>.On<CSAnswers.refNoteID.IsEqual<DrawingLog.noteID>>.View Answers;

        private readonly DrawingLogDataProvider drawingLogDataProvider;
        private readonly CommonAttributesService commonAttributesService;

        public DrawingLogsSetupMaint()
        {
            drawingLogDataProvider = new DrawingLogDataProvider(this);
            commonAttributesService = new CommonAttributesService(this, Attributes);
        }

        protected override PXSelectBase<DrawingLogStatus> Statuses => DrawingLogStatuses;

        protected override string DocumentName => DrawingLogMessages.DrawingLogDocuments;

        public virtual void _(Events.RowPersisting<DrawingLogDiscipline> args)
        {
            var drawings =
                drawingLogDataProvider.GetDrawingLogs<DrawingLog.disciplineId>(args.Row.DrawingLogDisciplineId);
            if (args.Operation == PXDBOperation.Delete && drawings.Any())
            {
                throw new Exception(DrawingLogMessages.DisciplineAlreadyInUse);
            }
        }

        public virtual void _(Events.RowSelected<DrawingLogDiscipline> args)
        {
            if (args.Row.IsDefault.GetValueOrDefault())
            {
                PXUIFieldAttribute.SetEnabled<DrawingLogDiscipline.name>(args.Cache, args.Row, false);
            }
        }

        public virtual void _(Events.RowInserting<CSAttributeGroup> args)
        {
            commonAttributesService.InitializeInsertedAttribute<DrawingLog>(args.Row, Constants.DrawingLogClassId);
        }

        public virtual void _(Events.RowDeleting<CSAttributeGroup> args)
        {
            commonAttributesService.DeleteAnswersIfRequired<DrawingLog>(args);
        }

        public virtual void _(Events.FieldSelecting<CSAttributeGroup, CSAttributeGroup.defaultValue> args)
        {
            if (args.ReturnState == null)
            {
                return;
            }
            args.ReturnState = commonAttributesService.GetNewReturnState(args.ReturnState, args.Row);
        }

        public virtual void _(Events.FieldDefaulting<DrawingLogDiscipline.sortOrder> args)
        {
            if (DrawingLogSetup.Current != null && args.Row is DrawingLogDiscipline)
            {
                var maxSortOrder = drawingLogDataProvider.GetDisciplineSortOrders().Max();
                args.NewValue = maxSortOrder + 1;
            }
        }

        public virtual void _(Events.RowDeleting<DrawingLogDiscipline> args)
        {
            if (args.Row.IsDefault.GetValueOrDefault())
            {
                throw new Exception(ProjectManagementMessages.SystemRecordCannotBeDeleted);
            }
            if (args.Row.IsActive.GetValueOrDefault())
            {
                throw new Exception(DrawingLogMessages.ActiveDisciplineCannotBeDeleted);
            }
            if (drawingLogDataProvider.GetDrawingLogs<DrawingLog.disciplineId>(args.Row.DrawingLogDisciplineId).Any())
            {
                throw new Exception(DrawingLogMessages.DisciplineAlreadyInUse);
            }
        }

        protected override IEnumerable<DrawingLog> GetDocuments(int? statusId)
        {
            return drawingLogDataProvider.GetDrawingLogs<DrawingLog.statusId>(statusId);
        }

        protected override DrawingLogStatus GetDefaultStatus()
        {
            return SelectFrom<DrawingLogStatus>
                .Where<DrawingLogStatus.isDefault.IsEqual<True>>.View.Select(this);
        }
    }
}
