using System;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryEquipmentExtension : DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXViewName(ViewNames.Equipment)]
        [PXCopyPasteHiddenFields(typeof(EquipmentProjection.equipmentTimeCardCd))]
        public SelectFrom<EquipmentProjection>
            .Where<EquipmentProjection.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View Equipment;

        public PXAction<DailyFieldReport> ViewEquipmentTimeCard;

        private EquipmentTimeCardMaint graph;

        private int currentWeek;

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.Equipment, ViewNames.Equipment);

        protected override Type RelationPrimaryCacheType => typeof(EquipmentProjection);

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
        }

        [PXButton]
        [PXUIField]
        public virtual void viewEquipmentTimeCard()
        {
            if (Equipment.Current?.TimeCardCD is string timeCardReferenceNumber)
            {
                var equipmentTimeCardMaint = PXGraph.CreateInstance<EquipmentTimeCardMaint>();
                equipmentTimeCardMaint.Document.Current = equipmentTimeCardMaint.Document
                    .Search<EPEquipmentTimeCard.timeCardCD>(timeCardReferenceNumber);
                PXRedirectHelper.TryRedirect(equipmentTimeCardMaint, PXRedirectHelper.WindowMode.NewWindow);
            }
        }

        [PXOverride]
        public void Persist(Action baseMethod)
        {
            if (!Equipment.Cache.IsInsertedUpdatedDeleted || Equipment.SelectMain().Any(e => e.EquipmentId == null))
            {
                baseMethod();
                return;
            }
            graph = PXGraph.CreateInstance<EquipmentTimeCardMaint>();
            currentWeek = PXWeekSelectorAttribute.GetWeekID(Base.DailyFieldReport.Current.Date.GetValueOrDefault());
            Equipment.Cache.Cached.RowCast<EquipmentProjection>().ForEach(PersistEquipment);
            baseMethod();
            Equipment.Cache.Clear();
            graph.Persist();
        }

        public virtual void _(Events.RowDeleting<EquipmentProjection> args)
        {
            RaiseExceptionIfTimeCardSubmitted(args.Row);
        }

        public virtual void _(Events.RowSelected<EquipmentProjection> args)
        {
            var equipment = args.Row;
            if (equipment == null)
            {
                return;
            }
            if (Base.IsMobile)
            {
                equipment.LastModifiedDateTime = equipment.LastModifiedDateTime.GetValueOrDefault().Date;
            }
            PXUIFieldAttribute.SetEnabled(args.Cache, equipment,
                equipment.TimeCardStatus == EPEquipmentTimeCardStatusAttribute.OnHold);
            var isEquipmentIdEnabled = args.Cache.GetValueOriginal<EquipmentProjection.equipmentId>(equipment) == null;
            PXUIFieldAttribute.SetEnabled<EquipmentProjection.equipmentId>(args.Cache, equipment,
                isEquipmentIdEnabled);
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(EquipmentProjection))
            {
                RelationNumber = typeof(EquipmentProjection.equipmentTimeCardCd)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(Equipment);
        }

        private static void RaiseExceptionIfTimeCardSubmitted(EquipmentProjection equipment)
        {
            if (equipment.TimeCardStatus != EPEquipmentTimeCardStatusAttribute.OnHold)
            {
                throw new PXSetPropertyException<EquipmentProjection.equipmentId>(DailyFieldReportMessages
                    .ThisEquipmentIsAssociatedWithSubmittedTimeCard);
            }
        }

        private void RaiseExceptionIfTimeCardSubmitted(string timeCardStatus, EquipmentProjection equipment)
        {
            if (timeCardStatus != EPEquipmentTimeCardStatusAttribute.OnHold)
            {
                Equipment.Cache.RaiseException<EquipmentProjection.equipmentId>(equipment,
                    DailyFieldReportMessages.ThisEquipmentIsAssociatedWithSubmittedTimeCard, null,
                    PXErrorLevel.RowError);
            }
        }

        private void PersistEquipment(EquipmentProjection equipment)
        {
            var applicableTimeCard = GetEquipmentTimeCard(equipment.EquipmentId) ??
                CreateEquipmentTimeCard(equipment.EquipmentId);
            graph.Document.Current = applicableTimeCard;
            var rowStatus = Equipment.Cache.GetStatus(equipment);
            var detailsCache = graph.Details.Cache;
            switch (rowStatus)
            {
                case PXEntryStatus.Inserted:
                    InsertEquipment(detailsCache, applicableTimeCard, equipment);
                    break;
                case PXEntryStatus.Updated:
                    RaiseExceptionIfTimeCardSubmitted(applicableTimeCard.Status, equipment);
                    detailsCache.Update(equipment);
                    graph.Summary.Cache.Updated.RowCast<EPEquipmentSummary>()
                        .ForEach(x => x.tstamp = PXDatabase.SelectTimeStamp());
                    break;
                case PXEntryStatus.Deleted:
                    detailsCache.Delete(equipment);
                    break;
            }
        }

        private void InsertEquipment(PXCache cache, EPEquipmentTimeCard applicableTimeCard,
            EquipmentProjection equipment)
        {
            RaiseExceptionIfTimeCardSubmitted(applicableTimeCard.Status, equipment);
            equipment.TimeCardCD = applicableTimeCard.TimeCardCD;
            equipment.LineNbr =
                (int?) PXLineNbrAttribute.NewLineNbr<EPEquipmentDetail.lineNbr>(cache, applicableTimeCard);
            equipment.DailyFieldReportId = Base.DailyFieldReport.Current.DailyFieldReportId;
            equipment.EquipmentTimeCardCd = equipment.TimeCardCD;
            equipment.EquipmentDetailLineNumber = equipment.LineNbr;
            cache.Insert(equipment);
        }

        private EPEquipmentTimeCard CreateEquipmentTimeCard(int? equipmentId)
        {
            var timeCard = new EPEquipmentTimeCard
            {
                EquipmentID = equipmentId,
                WeekID = currentWeek
            };
            timeCard = graph.Document.Insert(timeCard);
            graph.Persist();
            return timeCard;
        }

        private EPEquipmentTimeCard GetEquipmentTimeCard(int? equipmentId)
        {
            return SelectFrom<EPEquipmentTimeCard>
                .Where<EPEquipmentTimeCard.equipmentID.IsEqual<P.AsInt>
                    .And<EPEquipmentTimeCard.weekId.IsEqual<P.AsInt>>>.View
                .Select(graph, equipmentId, currentWeek);
        }
    }
}