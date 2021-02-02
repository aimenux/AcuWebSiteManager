using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;
using System;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    public class AMTimeCardCreate : PXGraph<AMTimeCardCreate>
    {
        public PXCancel<AMTimeCardFilter> Cancel;
        public PXFilter<AMTimeCardFilter> TimeCardFilter;
        [PXFilterable]
        public PXProcessing<
                AMMTran,
                Where<AMMTran.docType, Equal<AMDocType.labor>,
                    And<AMMTran.released, Equal<True>,
                    And<Where<IsNull<AMMTran.timeCardStatus, TimeCardStatus.unprocessed>, Equal<TimeCardStatus.unprocessed>,
                        Or<Current<AMTimeCardFilter.showAll>, Equal<True>>>>>>> Items;

        public AMTimeCardCreate()
        {
            var filter = TimeCardFilter.Current;
            Items.SetProcessDelegate(
                delegate (List<AMMTran> list)
                {
                    ProcessDoc(list, filter, true);
                });

            if (!PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>())
            {
                Items.SetProcessAllEnabled(false);
                Items.SetProcessEnabled(false);

                throw new PX.Data.PXSetupNotEnteredException(Messages.TimeReportingModuleNotEnabled, typeof(FeaturesSet), Messages.EnableDisableFeatures);
            }
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXSelector(typeof(Search<AMBatch.batNbr, Where<AMBatch.docType, Equal<Current<AMMTran.docType>>>, OrderBy<Desc<AMBatch.batNbr>>>), ValidateValue = false)]
        protected virtual void AMMTran_BatNbr_CacheAttached(PXCache sender)
        {
            //Add support for hyperlink in UI
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXSelector(typeof(Search<AMLaborCode.laborCodeID>), ValidateValue = false)]
        protected virtual void AMMTran_LaborCodeID_CacheAttached(PXCache sender)
        {
            //Add support for hyperlink in UI
        }

        protected virtual void AMMTran_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            AllowForIndirectLabor(cache);
        }

        public static void AllowForIndirectLabor(PXCache cache)
        {
            PXDefaultAttribute.SetPersistingCheck<AMMTran.orderType>(cache, null, PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<AMMTran.prodOrdID>(cache, null, PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<AMMTran.operationID>(cache, null, PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<AMMTran.inventoryID>(cache, null, PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<AMMTran.siteID>(cache, null, PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<AMMTran.locationID>(cache, null, PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<AMMTran.uOM>(cache, null, PXPersistingCheck.Nothing);
        }

        private static bool CanProcess(AMMTran row)
        {
            return row != null
                && ((row.TimeCardStatus ?? TimeCardStatus.Unprocessed) == TimeCardStatus.Unprocessed
                    || row.TimeCardStatus == TimeCardStatus.Skipped);
        }

        public static void SetSkipped(List<AMMTran> list, bool isMassProcess)
        {
            if (list == null)
            {
                return;
            }

            var graph = CreateInstance<AMTimeCardCreate>();

            var failed = false;

            for (var i = 0; i < list.Count; i++)
            {
                try
                {
                    graph.SetSkipped(list[i]);

                    if (isMassProcess)
                    {
                        PXProcessing<AMMTran>.SetInfo(i, ActionsMessages.RecordProcessed);
                    }
                }
                catch (Exception e)
                {
                    if (isMassProcess)
                    {
                        PXProcessing<AMMTran>.SetError(i, e);
                        failed = true;
                    }
                    if (list.Count == 1)
                    {
                        throw new PXOperationCompletedSingleErrorException(e);
                    }
                    else
                    {
                        failed = true;
                    }

                    //Record error to trace window 
                    PXTraceHelper.PxTraceException(e);
                }
            }

            if (failed)
            {
                throw new PXOperationCompletedException(ErrorMessages.SeveralItemsFailed);
            }
        }

        protected virtual void SetSkipped(AMMTran row)
        {
            if (row?.BatNbr == null || !CanProcess(row))
            {
                return;
            }

            row.TimeCardStatus = TimeCardStatus.Skipped;
            Items.Cache.PersistUpdated(Items.Update(row));
        }

        public static void ProcessDoc(List<AMMTran> list, AMTimeCardFilter filter, bool isMassProcess)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (filter.SkipSelected.GetValueOrDefault())
            {
                SetSkipped(list, isMassProcess);
                return;
            }

            ProcessDoc(list, isMassProcess);
        }

        public static void ProcessDoc(List<AMMTran> list, bool isMassProcess)
        {
            if (list == null)
            {
                return;
            }

            var failed = false;

            var empActEntry = CreateInstance<EmployeeActivitiesEntry>();
            var createTimeCardGraph = CreateInstance<AMTimeCardCreate>();

            Common.Cache.AddCacheView<AMMTran>(empActEntry);

            empActEntry.RowSelected.AddHandler<AMMTran>((cache, args) =>
            {
                AllowForIndirectLabor(cache);
            });

            for (var i = 0; i < list.Count; i++)
            {
                var row = list[i];
                try
                {
                    empActEntry.Clear();

                    if (CanProcess(row))
                    {
                        createTimeCardGraph.ProcessDoc(empActEntry, row);
                    }

                    if (!empActEntry.IsDirty)
                    {
                        PXProcessing<AMMTran>.SetWarning(i, Messages.UnableToProcessRecord);
                        continue;
                    }

                    if (empActEntry.IsDirty)
                    {
                        empActEntry.Persist();
                    }

                    if (isMassProcess)
                    {
                        PXProcessing<AMMTran>.SetInfo(i, ActionsMessages.RecordProcessed);
                    }
                }
                catch (Exception e)
                {
                    if (isMassProcess)
                    {
                        PXProcessing<AMMTran>.SetError(i, e);
                        failed = true;
                    }
                    if (list.Count == 1)
                    {
                        throw new PXOperationCompletedSingleErrorException(e);
                    }
                    else
                    {
                        failed = true;
                    }

                    //Record error to trace window 
                    PXTraceHelper.PxTraceException(e);
                }
            }

            if (failed)
            {
                throw new PXOperationCompletedException(ErrorMessages.SeveralItemsFailed);
            }
        }

        protected virtual void ProcessDoc(EmployeeActivitiesEntry empActEntry, AMMTran row)
        {
            if (empActEntry == null || row?.BatNbr == null)
            {
                return;
            }

            EPEmployee emp = PXSelect<
                        EPEmployee,
                        Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>
                        .SelectWindowed(empActEntry, 0, 1, row.EmployeeID);

            if (emp?.BAccountID == null)
            {
                PXTrace.WriteWarning(Messages.RecordMissing, Common.Cache.GetCacheName(typeof(EPEmployee)));
                return;
            }

            var newAct = empActEntry.Activity.Insert(new EPActivityApprove
            {
                OwnerID = emp.DefContactID,
                ApprovalStatus = ActivityStatusListAttribute.Open,
                Date = row.TranDate,
                EarningTypeID = empActEntry.EPsetingst?.Current?.RegularHoursType,
                ProjectID = row.ProjectID,
                TimeSpent = row.LaborTime,
                Summary = $"{row.OrderType} {row.ProdOrdID}",
                RefNoteID = row.NoteID,
                EmployeeRate = 0
            });
            newAct.ProjectTaskID = row.TaskID;
            newAct.CostCodeID = row.CostCodeID;
            var newActExt = newAct.GetExtension<PMTimeActivityExt>();
            newActExt.AMIsProd = true;
            empActEntry.Activity.Update(newAct);

            row.TimeCardStatus = TimeCardStatus.Processed;
            empActEntry.Caches<AMMTran>().Update(row);
        }

        public PXAction<AMMTran> SkipSelected;
        [PXUIField(DisplayName = "Skip Selected", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update), PXProcessButton]
        public virtual IEnumerable skipSelected(PXAdapter adapter)
        {
            TimeCardFilter.Current.SkipSelected = true;
            return Actions[PX.Objects.IN.Messages.Process].Press(adapter);
        }
    }

    [Serializable]
    [PXCacheName("AM Time Card Filter")]
    public class AMTimeCardFilter : IBqlTable
    {
        #region ShowAll

        public abstract class showAll : PX.Data.BQL.BqlBool.Field<showAll> { }

        protected Boolean? _ShowAll;

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Show All Items")]
        public virtual Boolean? ShowAll
        {
            get { return this._ShowAll; }
            set { this._ShowAll = value; }
        }

        #endregion
        #region SkipSelected

        public abstract class skipSelected : PX.Data.BQL.BqlBool.Field<skipSelected> { }

        protected Boolean? _SkipSelected;

        [PXBool]
        [PXUIField(Visible = false, Enabled = false)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? SkipSelected
        {
            get { return this._SkipSelected; }
            set { this._SkipSelected = value; }
        }

        #endregion
    }
}
