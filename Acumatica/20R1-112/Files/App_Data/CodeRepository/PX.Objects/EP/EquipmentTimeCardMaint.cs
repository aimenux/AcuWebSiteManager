using System;
using System.Collections;
using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.GL;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
using PX.Objects.CR;
using PX.TM;

namespace PX.Objects.EP
{
    [Serializable]
    public class EquipmentTimeCardMaint : PXGraph<EquipmentTimeCardMaint, EPEquipmentTimeCard>
    {
        #region DAC Overrides


        #endregion

        #region Selects

        [PXHidden]
        public PXSetup<EPSetup> EpSetup;

        [PXViewName(Messages.TimeCardDocument)]
        public PXSelect<EPEquipmentTimeCard> Document;

		[PXViewName(Messages.EquipmentSummary)]
        public PXSelect<EPEquipmentSummary, Where<EPEquipmentSummary.timeCardCD, Equal<Current<EPEquipmentTimeCard.timeCardCD>>>, OrderBy<Asc<EPEquipmentSummary.lineNbr>>> Summary;


		[PXViewName(Messages.EquipmentDetail)]
        public PXSelect<EPEquipmentDetail, Where<EPEquipmentDetail.timeCardCD, Equal<Current<EPEquipmentTimeCard.timeCardCD>>>, OrderBy<Asc<EPEquipmentDetail.lineNbr>>> Details;

        public PXSetup<EPSetup> Setup;

        //TODO: Check with Kesha if this is required and implemented correctly!
        [PXViewName(Messages.Approval)]
        public EPApprovalAction<EPEquipmentTimeCard, EPEquipmentTimeCard.isApproved, EPEquipmentTimeCard.isRejected> Approval;
        #endregion

        public EquipmentTimeCardMaint()
        {
            if (EpSetup.Current.EquipmentTimeCardNumberingID == null)
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(EPSetup), typeof(EPSetup).Name);
			
			Approval.StatusHandler =
				(item) =>
				{
					switch (Approval.GetResult(item))
					{
						case ApprovalResult.Approved:
							item.Status = EPEquipmentTimeCardStatusAttribute.Approved;
							break;

						case ApprovalResult.Rejected:
							item.Status = EPEquipmentTimeCardStatusAttribute.Rejected;
							break;

						case ApprovalResult.PendingApproval:
							item.Status = EPEquipmentTimeCardStatusAttribute.PendingApproval;
							break;

						case ApprovalResult.Submitted:
							item.Status = EPEquipmentTimeCardStatusAttribute.Released;
							break;
					}
				};
        }

		public override bool CanClipboardCopyPaste() { return false; }

        /// <summary>
        /// Gets the source for the generated PMTran.AccountID
        /// </summary>
        public string ExpenseAccountSource
        {
            get
            {
                string result = PM.PMExpenseAccountSource.InventoryItem;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccountSource))
                {
                    result = setup.ExpenseAccountSource;
                }

                return result;
            }
        }

        public string ExpenseSubMask
        {
            get
            {
                string result = null;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseSubMask))
                {
                    result = setup.ExpenseSubMask;
                }

                return result;
            }
        }

        public string ExpenseAccrualAccountSource
        {
            get
            {
                string result = PM.PMExpenseAccountSource.InventoryItem;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccountSource))
                {
                    result = setup.ExpenseAccrualAccountSource;
                }

                return result;
            }
        }

        public string ExpenseAccrualSubMask
        {
            get
            {
                string result = null;

                PMSetup setup = PXSelect<PMSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccrualSubMask))
                {
                    result = setup.ExpenseAccrualSubMask;
                }

                return result;
            }
        }

        public string ActivityTimeUnit
        {
            get
            {
                string result = EPSetup.Minute;

                EPSetup setup = PXSelect<EPSetup>.Select(this);
                if (setup != null && !string.IsNullOrEmpty(setup.ActivityTimeUnit))
                {
                    result = setup.ActivityTimeUnit;
                }

                return result;
            }
        }

        #region Actions


        public PXAction<EPEquipmentTimeCard> action;
        [PXUIField(DisplayName = Messages.Actions)]
        [PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
        protected virtual IEnumerable Action(PXAdapter adapter)
        {
            return adapter.Get();
        }

        
		public PXAction<EPEquipmentTimeCard> submit;
		[PXUIField(DisplayName = Messages.Submit)]
        [PXButton]
		protected virtual void Submit()
		{
			if (Document.Current != null)
			{
				if (EpSetup.Current.EquipmentTimeCardAssignmentMapID != null && PXAccess.FeatureInstalled<CS.FeaturesSet.approvalWorkflow>())
				{
					Approval.Assign(Document.Current, EpSetup.Current.EquipmentTimeCardAssignmentMapID, EpSetup.Current.EquipmentTimeCardAssignmentNotificationID);
				}
				else
				{
					Document.Current.IsApproved = true;
				}
				Document.Current.IsHold = false;
				Document.Update(Document.Current);
				Document.Search<EPEquipmentTimeCard.timeCardCD>(Document.Current.TimeCardCD);
			}

		}

		public PXAction<EPEquipmentTimeCard> edit;
		[PXUIField(DisplayName = Messages.PutOnHold)]
		[PXButton]
		protected virtual void Edit()
		{
			if (Document.Current != null)
			{
				Document.Current.IsApproved = false;
				Document.Current.IsRejected = false;
				Document.Current.IsHold = true;
				Document.Update(Document.Current);
				PXSelectBase<EPApproval> select = new PXSelect<EPApproval, Where<EPApproval.refNoteID, Equal<Required<EPApproval.refNoteID>>>>(this);
				foreach (EPApproval approval in select.Select((Document.Current.NoteID)))
				{
					this.Caches[typeof(EPApproval)].Delete(approval);
				}
				Document.Search<EPEquipmentTimeCard.timeCardCD>(Document.Current.TimeCardCD);
			}

        }

        public PXAction<EPEquipmentTimeCard> release;
        [PXUIField(DisplayName = Messages.Release)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Release)]
        protected virtual IEnumerable Release(PXAdapter adapter)
        {
			List<EPEquipmentTimeCard> list = adapter.Get().Cast<EPEquipmentTimeCard>().Where(item => item.IsReleased != true).ToList();
			
			if (list.Any())
			{
				Save.Press();
				PXLongOperation.StartOperation(this, delegate ()
				{
					var releaseGraph = (PM.RegisterEntry)PXGraph.CreateInstance(typeof(PM.RegisterEntry));
					var timecardGraph = PXGraph.CreateInstance<EquipmentTimeCardMaint>();
					releaseGraph.Clear();

					foreach (EPEquipmentTimeCard item in list)
					{
						releaseGraph.Clear();

						PMSetup setup = PXSelect<PMSetup>.Select(releaseGraph);
						if (setup == null)
						{
							//Setup may be null because the PM module is not enabled. and yet we need to be able to generate PMTran for the Contract billing.
							releaseGraph.Setup.Insert();
						}

						using (PXTransactionScope ts = new PXTransactionScope())
						{
							timecardGraph.Clear();
							timecardGraph.Document.Current = item;

							if (string.IsNullOrEmpty(item.OrigTimeCardCD))
							{
								timecardGraph.ProcessRegularTimecard(releaseGraph, item);
							}
							else
							{
								timecardGraph.ProcessCorrectingTimecard(releaseGraph, item);
							}

							releaseGraph.Save.Press();

							item.Status = EPEquipmentTimeCardStatusAttribute.Released;
							item.IsReleased = true;
							timecardGraph.Document.Update(item);

							timecardGraph.Save.Press();

							ts.Complete();
						}

						if (EpSetup.Current.AutomaticReleasePM == true)
						{
							PX.Objects.PM.RegisterRelease.Release(releaseGraph.Document.Current);
						}
					}
				});
			}
			else throw new PXException(Messages.AlreadyReleased);

			return list;
        }



        public PXAction<EPEquipmentTimeCard> correct;
        [PXUIField(DisplayName = Messages.Correct)]
        [PXButton]
        protected virtual IEnumerable Correct(PXAdapter adapter)
        {
            if (Document.Current != null)
            {
                EPEquipmentTimeCard source = Document.Current;

                EPEquipmentTimeCard newCard = (EPEquipmentTimeCard)Document.Cache.Insert();
                newCard.EquipmentID = source.EquipmentID;
                newCard.WeekID = source.WeekID;
                newCard.OrigTimeCardCD = source.TimeCardCD;

                foreach (EPEquipmentDetail row in Details.View.SelectMultiBound(new object[] { source }))
                {
                    EPEquipmentDetail newDetail = PXCache<EPEquipmentDetail>.CreateCopy(row);
                    newDetail.TimeCardCD = null;
                    newDetail.LineNbr = null;
                    newDetail.OrigLineNbr = row.LineNbr;

                    Details.Insert(newDetail);
                }


                return new EPEquipmentTimeCard[] { newCard };
            }

            return adapter.Get();
        }


        public PXAction<EPEquipmentTimeCard> preloadFromPreviousTimecard;
        [PXUIField(DisplayName = Messages.PreloadFromPreviousTimecard)]
        [PXButton]
        protected virtual void PreloadFromPreviousTimecard()
        {
            if (Document.Current == null)
                return;

            if (Document.Current.WeekID == null)
                return;

            EPEquipmentTimeCard previous = PXSelect<EPEquipmentTimeCard, Where<EPEquipmentTimeCard.equipmentID, Equal<Current<EPEquipmentTimeCard.equipmentID>>, And<EPEquipmentTimeCard.weekId, Equal<Required<EPEquipmentTimeCard.weekId>>>>>.Select(this, Document.Current.WeekID.Value - 1);
            if (previous == null)
                return;

            PXSelectBase<EPEquipmentSummary> summarySelect = new PXSelect<EPEquipmentSummary, Where<EPEquipmentSummary.timeCardCD, Equal<Required<EPEquipmentSummary.timeCardCD>>>>(this);
            foreach (EPEquipmentSummary item in summarySelect.Select(previous.TimeCardCD))
            {
                EPEquipmentSummary summary = PXCache<EPEquipmentSummary>.CreateCopy(item);
                summary.TimeCardCD = null;
                summary.LineNbr = null;
	            summary.NoteID = null;

                Summary.Insert(summary);
            }

        }

        #endregion

		#region DAC Overrides

		[PXDBDate()]
		[PXDefault(typeof(EPEquipmentTimeCard.createdDateTime), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(500, IsUnicode = true)]
		[PXDefault(typeof(Search<EPEquipment.equipmentCD, Where<EPEquipment.equipmentID, Equal<Current<EPEquipmentTimeCard.equipmentID>>,
							And<EPEquipment.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(Search<EPEmployee.bAccountID,
				Where<EPEmployee.userID, Equal<Current<EPEquipmentTimeCard.createdByID>>>>),
					PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBGuid()]
		[PXDefault(typeof(Search<CREmployee.userID,
				Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
					And<Current<EPEquipmentTimeCard.workgroupID>, IsNull>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender)
		{
		}

		#endregion

        #region Event Handlers

		protected virtual void EPEquipmentTimeCard_RowInserted(PXCache sender, PXRowInsertedEventArgs e) { }
		
		protected virtual void EPEquipmentTimeCard_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            EPEquipmentTimeCard row = e.Row as EPEquipmentTimeCard;
            if (row == null) return;

            if (row.WeekID == null)
	        {
				try
				{
                row.WeekID = GetNextWeekID(row.EquipmentID);
        }
				catch (PXException exception)
				{
					row.WeekID = null;
					sender.RaiseExceptionHandling<EPEquipmentTimeCard.weekId>(row, null, exception);
				}
			}

        }

        protected virtual void EPEquipmentTimeCard_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var row = e.Row as EPEquipmentTimeCard;
            var oldRow = e.OldRow as EPEquipmentTimeCard;
            if (row == null || oldRow == null) return;

            if (row.WeekID == null || row.EquipmentID != oldRow.EquipmentID)
			{
				try
				{
                row.WeekID = GetNextWeekID(row.EquipmentID);
        }
				catch (PXException exception)
				{
					row.WeekID = null;
					sender.RaiseExceptionHandling<EPEquipmentTimeCard.weekId>(row, null, exception);
				}
			}

        }

        protected virtual void EPEquipmentTimeCard_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            EPEquipmentTimeCard timeCard = PXSelect<EPEquipmentTimeCard, Where<EPEquipmentTimeCard.equipmentID, Equal<Current<EPEquipmentTimeCard.equipmentID>>,
                                And<EPEquipmentTimeCard.weekId, Greater<Current<EPEquipmentTimeCard.weekId>>>>>.SelectWindowed(this, 0, 1);

            if (timeCard != null)
            {
                throw new PXException(Messages.TimeCardNoDelete);
            }

            var row = e.Row as EPEquipmentTimeCard;
            if (row == null) return;
            if (!string.IsNullOrEmpty(row.OrigTimeCardCD))
            {
                PXSelectBase<EPEquipmentDetail> select = new PXSelect<EPEquipmentDetail, Where<EPEquipmentDetail.timeCardCD, Equal<Current<EPEquipmentTimeCard.timeCardCD>>>>(this);
                foreach (EPEquipmentDetail detail in select.Select())
                {
                    Details.Delete(detail);
                }
            }
        }

        protected virtual void EPEquipmentTimeCard_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = e.Row as EPEquipmentTimeCard;
            if (row == null) return;

            var allowDelete = row.IsReleased != true;
            Document.Cache.AllowDelete = allowDelete;

            var allowEdit = row.IsHold == true && row.IsApproved != true &&
                row.IsRejected != true && row.IsReleased != true && row.EquipmentID != null;

            Details.Cache.AllowInsert = allowEdit;
            Details.Cache.AllowUpdate = allowEdit;
            Details.Cache.AllowDelete = allowEdit;
            Summary.Cache.AllowInsert = allowEdit;
            Summary.Cache.AllowUpdate = allowEdit;
            Summary.Cache.AllowDelete = allowEdit;
            preloadFromPreviousTimecard.SetEnabled(allowEdit);

            if (row.EquipmentID != null)
            {
                if (Summary.Select().Count > 0)
                {
                    PXUIFieldAttribute.SetEnabled<EPEquipmentTimeCard.equipmentID>(sender, row, false);
                }
                else
                {
                    PXUIFieldAttribute.SetEnabled<EPEquipmentTimeCard.equipmentID>(sender, row, allowEdit);
                }
            }

            var isFirst = IsFirstTimeCard(row.EquipmentID);
            PXUIFieldAttribute.SetEnabled<EPEquipmentTimeCard.weekId>(sender, row, isFirst && allowEdit);

            RecalculateTotals(row);
        }

        protected virtual void EPEquipmentTimeCard_EquipmentID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            EPEquipmentTimeCard row = e.Row as EPEquipmentTimeCard;
            if (row != null)
            {
                EPEquipmentTimeCard futureTimeCard = PXSelect<EPEquipmentTimeCard, Where<EPEquipmentTimeCard.equipmentID, Equal<Current<EPEquipmentTimeCard.equipmentID>>, And<EPEquipmentTimeCard.weekId, Greater<Current<EPEquipmentTimeCard.weekId>>>>>.Select(this);
                if (futureTimeCard != null)
                {
                    throw new PXSetPropertyException(Messages.EquipmentTimeCardInFutureExists);
                }

            }
        }


        protected virtual void EPEquipmentSummary_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            Debug.Print("EPEquipmentSummary_RowInserted Start");
            Debug.Indent();
            try
            {
                EPEquipmentSummary row = e.Row as EPEquipmentSummary;
                if (row == null) return;
                if (dontSyncDetails) return;

                UpdateAdjustingDetails(row);
            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPEquipmentSummary_RowInserted End");
            }
        }

        protected virtual void EPEquipmentSummary_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            Debug.Print("EPEquipmentSummary_RowUpdated Start");
            Debug.Indent();
            try
            {
                EPEquipmentSummary row = e.Row as EPEquipmentSummary;
                EPEquipmentSummary oldRow = e.OldRow as EPEquipmentSummary;

                Debug.Print("Old: {0}", oldRow);
                Debug.Print("New: {0}", row);

                if (row == null) return;
                if (dontSyncDetails) return;

                UpdateAdjustingDetails(row);
            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPEquipmentSummary_RowUpdated End");
            }
        }

        protected virtual void EPEquipmentSummary_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            Debug.Print("EPEquipmentSummary_RowDeleted Start");
            Debug.Indent();
            try
            {
                EPEquipmentSummary row = e.Row as EPEquipmentSummary;
                if (row == null) return;
                if (dontSyncDetails) return;

                UpdateAdjustingDetails(row);
            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPEquipmentSummary_RowDeleted End");
            }
        }

		protected virtual void _(Events.FieldDefaulting<EPEquipmentDetail, EPEquipmentDetail.date> e)
		{
			if (Document.Current != null)
			{
				e.NewValue = Document.Current.WeekStartDate;
			}
		}

		protected virtual void EPEquipmentDetail_Date_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPEquipmentDetail row = e.Row as EPEquipmentDetail;
			if (row == null) return;

			DateTime? newValue = null;
			DateTime valFromString;
			if (e.NewValue is string &&
				DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, System.Globalization.DateTimeStyles.None, out valFromString))
			{
				newValue = valFromString;
			}
			if (e.NewValue is DateTime)
				newValue = (DateTime)e.NewValue;

			PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, Document.Current.WeekID.Value);
			if (newValue != null && !weekInfo.IsValid(((DateTime)newValue).Date))
			{
				throw new PXSetPropertyException(Messages.DateNotInWeek);
			}
		}

		protected virtual void EPEquipmentDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            Debug.Print("EPEquipmentDetail_RowInserted Start");
            Debug.Indent();
            try
            {
                EPEquipmentDetail row = e.Row as EPEquipmentDetail;
                if (row == null) return;
                if (dontSyncSummary) return;

                SortedList<string, EPEquipmentSummary> summary = GetSummaryRecords(row);
                AddToSummary(summary, row);
            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPTimecardDetail_RowInserted End");
            }


        }

        protected virtual void EPEquipmentDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            Debug.Print("EPEquipmentDetail_RowUpdated Start");
            Debug.Indent();
            try
            {
                EPEquipmentDetail row = e.Row as EPEquipmentDetail;
                EPEquipmentDetail oldRow = e.OldRow as EPEquipmentDetail;
                if (row == null) return;

                if (!dontSyncSummary)
                {
                    SortedList<string, EPEquipmentSummary> oldSummary = GetSummaryRecords(oldRow);
                    SubtractFromSummary(oldSummary, oldRow);

                    SortedList<string, EPEquipmentSummary> summary = GetSummaryRecords(row);
                    AddToSummary(summary, row);

                }
            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPEquipmentDetail_RowUpdated End");
            }

        }

        protected virtual void EPEquipmentDetail_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            Debug.Print("EPEquipmentDetail_RowDeleting Start");
            Debug.Indent();
            try
            {
                EPEquipmentDetail row = e.Row as EPEquipmentDetail;

                if (row != null)
                {
                    if (Document.Current != null && !string.IsNullOrEmpty(Document.Current.OrigTimeCardCD) && ((EPEquipmentDetail)e.Row).OrigLineNbr != null
                        && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Deleted)
                    {
                        throw new PXException(Messages.CannotDeleteCorrectionRecord);
                    }
                }
            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPEquipmentDetail_RowDeleting End");
            }
        }

        protected virtual void EPEquipmentDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            Debug.Print("EPEquipmentDetail_RowDeleted Start");
            Debug.Indent();
            try
            {
                EPEquipmentDetail row = e.Row as EPEquipmentDetail;

                if (row != null)//adjust activity is deleted
                {
                    if (!dontSyncSummary)
                    {
                        SortedList<string, EPEquipmentSummary> summary = GetSummaryRecords(row);
                        SubtractFromSummary(summary, row);
                    }
                }
            }
            finally
            {
                Debug.Unindent();
                Debug.Print("EPEquipmentDetail_RowDeleted End");
            }
        }


        protected virtual void EPApproval_Details_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (this.Document.Current == null) return;

            e.NewValue = null;
            Type[] fields = new Type[]
			{
				typeof(EPEquipmentTimeCard.weekDescription), typeof(EPEquipmentTimeCard.timeTotalCalc), typeof(EPEquipmentTimeCard.timeBillableTotalCalc)
			};
            foreach (Type t in fields)
            {
                PXStringState strState = this.Document.Cache.GetValueExt(this.Document.Current, t.Name) as PXStringState;
                if (strState != null)
                {
                    string value =
                        strState.InputMask != null ? Mask.Format(strState.InputMask, strState) :
                        strState.Value != null ? strState.Value.ToString() : null;

                    if (!string.IsNullOrEmpty(value))
                        e.NewValue += (e.NewValue != null ? ", " : string.Empty) + strState.DisplayName + "=" + value.Trim();
                }
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void ProcessRegularTimecard(PM.RegisterEntry releaseGraph, EPEquipmentTimeCard timecard)
        {
            PMRegister pmDoc = (PMRegister)releaseGraph.Document.Cache.Insert();
            pmDoc.OrigDocType = PMOrigDocType.EquipmentTimecard;
			pmDoc.OrigNoteID = timecard.NoteID;
            EPEquipment equipment = PXSelect<EPEquipment, Where<EPEquipment.equipmentID, Equal<Required<EPEquipment.equipmentID>>>>.Select(this, timecard.EquipmentID);

            foreach (EPEquipmentDetail detail in Details.Select())
            {
                if (detail.SetupTime.GetValueOrDefault() != 0 && equipment.SetupRateItemID == null)
                {
                    throw new PXException(Messages.EquipmentSetupRateIsNotDefined);
                }
                if (detail.RunTime.GetValueOrDefault() != 0 && equipment.RunRateItemID == null)
                {
                    throw new PXException(Messages.EquipmentRunRateIsNotDefined, equipment.EquipmentID);
                }
                if (detail.SuspendTime.GetValueOrDefault() != 0 && equipment.SuspendRateItemID == null)
                {
                    throw new PXException(Messages.EquipmentSuspendRateIsNotDefined);
                }

                InsertPMTran(releaseGraph, detail, equipment.SetupRateItemID, detail.SetupTime, GetSetupCost(equipment.EquipmentID, detail.ProjectID), equipment.EquipmentID);
                InsertPMTran(releaseGraph, detail, equipment.RunRateItemID, detail.RunTime, GetRunCost(equipment.EquipmentID, detail.ProjectID), equipment.EquipmentID);
                InsertPMTran(releaseGraph, detail, equipment.SuspendRateItemID, detail.SuspendTime, GetSuspendCost(equipment.EquipmentID, detail.ProjectID), equipment.EquipmentID);
            }
        }

        protected virtual void ProcessCorrectingTimecard(PM.RegisterEntry releaseGraph, EPEquipmentTimeCard timecard)
        {
            EPEquipment equipment = PXSelect<EPEquipment, Where<EPEquipment.equipmentID, Equal<Required<EPEquipment.equipmentID>>>>.Select(this, timecard.EquipmentID);
            if (equipment == null)
                throw new PXException(Messages.FailedSelectEquipment);

            PXCache registerCache = releaseGraph.Document.Cache;
            PXCache tranCache = releaseGraph.Transactions.Cache;
            PM.PMRegister doc = (PM.PMRegister)registerCache.Insert();
            doc.OrigDocType = PMOrigDocType.EquipmentTimecard;
			doc.OrigNoteID = timecard.NoteID;
            doc.Description = PXMessages.LocalizeFormatNoPrefixNLA(Messages.Correction, equipment.EquipmentCD, timecard.WeekID);


            //process deleted items
            PXSelectBase<EPEquipmentDetailOrig> selectDeletedItems = new PXSelectJoin<EPEquipmentDetailOrig,
                LeftJoin<EPEquipmentDetailEx, On<EPEquipmentDetailEx.timeCardCD, Equal<Current<EPEquipmentTimeCard.timeCardCD>>,
                    And<EPEquipmentDetailEx.origLineNbr, Equal<EPEquipmentDetailOrig.lineNbr>>>>,
                Where<EPEquipmentDetailOrig.timeCardCD, Equal<Current<EPEquipmentTimeCard.origTimeCardCD>>,
                    And<EPEquipmentDetailEx.timeCardCD, IsNull>>>(this);

            foreach (PXResult<EPEquipmentDetailOrig, EPEquipmentDetailEx> res in selectDeletedItems.View.SelectMultiBound(new object[] { timecard }))
            {
                EPEquipmentDetailOrig orig = (EPEquipmentDetailOrig)res;

                if (orig.SetupTime.GetValueOrDefault() > 0)
                {
                    if (equipment.SetupRateItemID == null)
                    {
                        throw new PXException(Messages.EquipmentSetupRateIsNotDefined);
                    }

                    InsertPMTran(releaseGraph, orig, equipment.SetupRateItemID, -orig.SetupTime, GetSetupCost(equipment.EquipmentID, orig.ProjectID), equipment.EquipmentID);
                }

                if (orig.RunTime.GetValueOrDefault() > 0)
                {
                    if (equipment.RunRateItemID == null)
                    {
                        throw new PXException(Messages.EquipmentRunRateIsNotDefined, equipment.EquipmentID);
                    }

                    InsertPMTran(releaseGraph, orig, equipment.RunRateItemID, -orig.RunTime, GetRunCost(equipment.EquipmentID, orig.ProjectID), equipment.EquipmentID);
                }

                if (orig.SuspendTime.GetValueOrDefault() > 0)
                {
                    if (equipment.SuspendRateItemID == null)
                    {
                        throw new PXException(Messages.EquipmentSuspendRateIsNotDefined);
                    }

                    InsertPMTran(releaseGraph, orig, equipment.SuspendRateItemID, -orig.SuspendTime, GetSuspendCost(equipment.EquipmentID, orig.ProjectID), equipment.EquipmentID);
                }
            }

            //process added items
            PXSelectBase<EPEquipmentDetail> selectAddedItems = new PXSelect<EPEquipmentDetail,
                Where<EPEquipmentDetail.timeCardCD, Equal<Current<EPEquipmentTimeCard.timeCardCD>>,
                    And<EPEquipmentDetail.origLineNbr, IsNull>>>(this);
            foreach (EPEquipmentDetail item in selectAddedItems.View.SelectMultiBound(new object[] { timecard }))
            {
                if (item.SetupTime.GetValueOrDefault() > 0)
                {
                    if (equipment.SetupRateItemID == null)
                    {
                        throw new PXException(Messages.EquipmentSetupRateIsNotDefined);
                    }

                    InsertPMTran(releaseGraph, item, equipment.SetupRateItemID, item.SetupTime, GetSetupCost(equipment.EquipmentID, item.ProjectID), equipment.EquipmentID);
                }

                if (item.RunTime.GetValueOrDefault() > 0)
                {
                    if (equipment.RunRateItemID == null)
                    {
                        throw new PXException(Messages.EquipmentRunRateIsNotDefined, equipment.EquipmentID);
                    }

                    InsertPMTran(releaseGraph, item, equipment.RunRateItemID, item.RunTime, GetRunCost(equipment.EquipmentID, item.ProjectID), equipment.EquipmentID);
                }

                if (item.SuspendTime.GetValueOrDefault() > 0)
                {
                    if (equipment.SuspendRateItemID == null)
                    {
                        throw new PXException(Messages.EquipmentSuspendRateIsNotDefined);
                    }

                    InsertPMTran(releaseGraph, item, equipment.SuspendRateItemID, item.SuspendTime, GetSuspendCost(equipment.EquipmentID, item.ProjectID), equipment.EquipmentID);
                }
            }

            //process modified records
            PXSelectBase<EPEquipmentDetailOrig> selectModifiedItems = new PXSelectJoin<EPEquipmentDetailOrig,
                LeftJoin<EPEquipmentDetailEx, On<EPEquipmentDetailEx.timeCardCD, Equal<Current<EPEquipmentTimeCard.timeCardCD>>,
                    And<EPEquipmentDetailEx.origLineNbr, Equal<EPEquipmentDetailOrig.lineNbr>>>>,
                Where<EPEquipmentDetailOrig.timeCardCD, Equal<Current<EPEquipmentTimeCard.origTimeCardCD>>,
                    And<EPEquipmentDetailEx.timeCardCD, IsNotNull>>>(this);
            foreach (PXResult<EPEquipmentDetailOrig, EPEquipmentDetailEx> res in selectModifiedItems.View.SelectMultiBound(new object[] { timecard }))
            {
                EPEquipmentDetailOrig orig = (EPEquipmentDetailOrig)res;
                EPEquipmentDetailEx item = (EPEquipmentDetailEx)res;

                if (item.SetupTime.GetValueOrDefault() - orig.SetupTime.GetValueOrDefault() != 0)
                {
                    if (equipment.SetupRateItemID == null)
                    {
                        throw new PXException(Messages.EquipmentSetupRateIsNotDefined);
                    }

					if (item.CostCodeID == orig.CostCodeID)
					{
						InsertPMTran(releaseGraph, item, equipment.SetupRateItemID, item.SetupTime.GetValueOrDefault() - orig.SetupTime.GetValueOrDefault(), GetSetupCost(equipment.EquipmentID, item.ProjectID), equipment.EquipmentID);
					}
					else
					{
						InsertPMTran(releaseGraph, orig, equipment.SetupRateItemID, -orig.SetupTime.GetValueOrDefault(), GetSetupCost(equipment.EquipmentID, item.ProjectID), equipment.EquipmentID);
						InsertPMTran(releaseGraph, item, equipment.SetupRateItemID, item.SetupTime.GetValueOrDefault(), GetSetupCost(equipment.EquipmentID, item.ProjectID), equipment.EquipmentID);
					}
                }

                if (item.RunTime.GetValueOrDefault() - orig.RunTime.GetValueOrDefault() != 0)
                {
                    if (equipment.RunRateItemID == null)
                    {
                        throw new PXException(Messages.EquipmentRunRateIsNotDefined, equipment.EquipmentID);
                    }

					if (item.CostCodeID == orig.CostCodeID)
					{
						InsertPMTran(releaseGraph, item, equipment.RunRateItemID, item.RunTime.GetValueOrDefault() - orig.RunTime.GetValueOrDefault(), GetRunCost(equipment.EquipmentID, item.ProjectID), equipment.EquipmentID);
					}
					else
					{
						InsertPMTran(releaseGraph, orig, equipment.RunRateItemID, -orig.RunTime.GetValueOrDefault(), GetRunCost(equipment.EquipmentID, item.ProjectID), equipment.EquipmentID);
						InsertPMTran(releaseGraph, item, equipment.RunRateItemID, item.RunTime.GetValueOrDefault(), GetRunCost(equipment.EquipmentID, item.ProjectID), equipment.EquipmentID);
					}
				}

                if (item.SuspendTime.GetValueOrDefault() - orig.SuspendTime.GetValueOrDefault() != 0)
                {
                    if (equipment.SuspendRateItemID == null)
                    {
                        throw new PXException(Messages.EquipmentSuspendRateIsNotDefined);
                    }

					if (item.CostCodeID == orig.CostCodeID)
					{
						InsertPMTran(releaseGraph, item, equipment.SuspendRateItemID, item.SuspendTime.GetValueOrDefault() - orig.SuspendTime.GetValueOrDefault(), GetSuspendCost(equipment.EquipmentID, item.ProjectID), equipment.EquipmentID);
					}
					else
					{
						InsertPMTran(releaseGraph, orig, equipment.SuspendRateItemID, -orig.SuspendTime.GetValueOrDefault(), GetSuspendCost(equipment.EquipmentID, item.ProjectID), equipment.EquipmentID);
						InsertPMTran(releaseGraph, item, equipment.SuspendRateItemID, item.SuspendTime.GetValueOrDefault(), GetSuspendCost(equipment.EquipmentID, item.ProjectID), equipment.EquipmentID);
					}
				}
            }
        }


        protected virtual SortedList<string, EPEquipmentSummary> GetSummaryRecords(EPEquipmentDetail detail)
        {
            if (detail == null)
                throw new ArgumentNullException();

            SortedList<string, EPEquipmentSummary> list = new SortedList<string, EPEquipmentSummary>();

            if (detail.SetupTime.GetValueOrDefault() != 0)
            {
                EPEquipmentSummary setupSummary = PXSelect<EPEquipmentSummary,
                    Where<EPEquipmentSummary.timeCardCD, Equal<Current<EPEquipmentTimeCard.timeCardCD>>,
                    And<EPEquipmentSummary.lineNbr, Equal<Required<EPEquipmentSummary.lineNbr>>>>>.Select(this, detail.SetupSummaryLineNbr);

                if (setupSummary != null)
                {
                    list.Add(EPEquipmentSummary.Setup, setupSummary);
                }
            }

            if (detail.RunTime.GetValueOrDefault() != 0)
            {
                EPEquipmentSummary runSummary = PXSelect<EPEquipmentSummary,
                    Where<EPEquipmentSummary.timeCardCD, Equal<Current<EPEquipmentTimeCard.timeCardCD>>,
                    And<EPEquipmentSummary.lineNbr, Equal<Required<EPEquipmentSummary.lineNbr>>>>>.Select(this, detail.RunSummaryLineNbr);

                if (runSummary != null)
                {
                    list.Add(EPEquipmentSummary.Run, runSummary);
                }
            }

            if (detail.SuspendTime.GetValueOrDefault() != 0)
            {
                EPEquipmentSummary suspendSummary = PXSelect<EPEquipmentSummary,
                    Where<EPEquipmentSummary.timeCardCD, Equal<Current<EPEquipmentTimeCard.timeCardCD>>,
                    And<EPEquipmentSummary.lineNbr, Equal<Required<EPEquipmentSummary.lineNbr>>>>>.Select(this, detail.SuspendSummaryLineNbr);

                if (suspendSummary != null)
                {
                    list.Add(EPEquipmentSummary.Suspend, suspendSummary);
                }
            }

            return list;
        }

        protected virtual List<EPEquipmentSummary> AddToSummary(SortedList<string, EPEquipmentSummary> list, EPEquipmentDetail detail)
        {
            return AddToSummary(list, detail, 1);
        }

        protected virtual void SubtractFromSummary(SortedList<string, EPEquipmentSummary> list, EPEquipmentDetail detail)
        {
            List<EPEquipmentSummary> result = AddToSummary(list, detail, -1);

            try
            {
                dontSyncDetails = true;
                foreach (EPEquipmentSummary item in result)
                {
                    if (item.TimeSpent.Value == 0)
                    {
                        Summary.Delete(item);//cascadly will delete detail record through PXParent.
                    }
                }

            }
            finally
            {
                dontSyncDetails = false;
            }

        }

        /// <summary>
        /// When True detail row is not updated when a summary record is modified.
        /// </summary>
        protected bool dontSyncDetails = false;
        protected virtual List<EPEquipmentSummary> AddToSummary(SortedList<string, EPEquipmentSummary> list, EPEquipmentDetail detail, int mult)
        {
            if (detail == null)
                throw new ArgumentNullException();

            Debug.Print("AddToSummary Mult:{0} Start", mult);
            Debug.Indent();


            if (detail.SetupTime.GetValueOrDefault() == 0 && detail.SuspendTime.GetValueOrDefault() == 0 && detail.RunTime.GetValueOrDefault() == 0)
            {
                Debug.Unindent();
                Debug.Print("Detail is empty. Exiting AddToSummary");
                return new List<EPEquipmentSummary>();
            }

            if (detail.SetupTime.GetValueOrDefault() != 0 && !list.ContainsKey(EPEquipmentSummary.Setup))
            {
                EPEquipmentSummary summary = (EPEquipmentSummary)Summary.Cache.CreateInstance();
                summary.RateType = EPEquipmentSummary.Setup;
                summary.ProjectID = detail.ProjectID;
                summary.ProjectTaskID = detail.ProjectTaskID;
				summary.CostCodeID = detail.CostCodeID;
				summary.IsBillable = detail.IsBillable;

                dontSyncDetails = true;
                try
                {
                    summary = Summary.Insert(summary);
                    detail.SetupSummaryLineNbr = summary.LineNbr;
                    list.Add(EPEquipmentSummary.Setup, summary);
                }
                finally
                {
                    dontSyncDetails = false;
                }
            }

            if (detail.RunTime.GetValueOrDefault() != 0 && !list.ContainsKey(EPEquipmentSummary.Run))
            {
                EPEquipmentSummary summary = (EPEquipmentSummary)Summary.Cache.CreateInstance();
                summary.RateType = EPEquipmentSummary.Run;
                summary.ProjectID = detail.ProjectID;
                summary.ProjectTaskID = detail.ProjectTaskID;
				summary.CostCodeID = detail.CostCodeID;
				summary.IsBillable = detail.IsBillable;

                dontSyncDetails = true;
                try
                {
                    summary = Summary.Insert(summary);
                    detail.RunSummaryLineNbr = summary.LineNbr;
                    list.Add(EPEquipmentSummary.Run, summary);
                }
                finally
                {
                    dontSyncDetails = false;
                }
            }

            if (detail.SuspendTime.GetValueOrDefault() != 0 && !list.ContainsKey(EPEquipmentSummary.Suspend))
            {
                EPEquipmentSummary summary = (EPEquipmentSummary)Summary.Cache.CreateInstance();
                summary.RateType = EPEquipmentSummary.Suspend;
                summary.ProjectID = detail.ProjectID;
                summary.ProjectTaskID = detail.ProjectTaskID;
				summary.CostCodeID = detail.CostCodeID;
				summary.IsBillable = detail.IsBillable;

                dontSyncDetails = true;
                try
                {
                    summary = Summary.Insert(summary);
                    detail.SuspendSummaryLineNbr = summary.LineNbr;
                    list.Add(EPEquipmentSummary.Suspend, summary);
                }
                finally
                {
                    dontSyncDetails = false;
                }
            }

            List<EPEquipmentSummary> result = new List<EPEquipmentSummary>();
            foreach (EPEquipmentSummary item in list.Values)
            {
                int time = 0;
                switch (item.RateType)
                {
                    case EPEquipmentSummary.Setup:
                        time = detail.SetupTime.GetValueOrDefault();
                        break;
                    case EPEquipmentSummary.Suspend:
                        time = detail.SuspendTime.GetValueOrDefault();
                        break;
                    default:
                        time = detail.RunTime.GetValueOrDefault();
                        break;
                }

                if (time != 0)
                {
                    switch (detail.Date.Value.DayOfWeek)
                    {
                        case DayOfWeek.Monday:

                            item.Mon = item.Mon.GetValueOrDefault() + (mult * time);
                            break;
                        case DayOfWeek.Tuesday:
                            item.Tue = item.Tue.GetValueOrDefault() + (mult * time);
                            break;
                        case DayOfWeek.Wednesday:
                            item.Wed = item.Wed.GetValueOrDefault() + (mult * time);
                            break;
                        case DayOfWeek.Thursday:
                            item.Thu = item.Thu.GetValueOrDefault() + (mult * time);
                            break;
                        case DayOfWeek.Friday:
                            item.Fri = item.Fri.GetValueOrDefault() + (mult * time);
                            break;
                        case DayOfWeek.Saturday:
                            item.Sat = item.Sat.GetValueOrDefault() + (mult * time);
                            break;
                        case DayOfWeek.Sunday:
                            item.Sun = item.Sun.GetValueOrDefault() + (mult * time);
                            break;
                    }
                }

                try
                {
                    dontSyncDetails = true;
                    result.Add(Summary.Update(item));
                }
                finally
                {
                    dontSyncDetails = false;
                }
            }


            Debug.Unindent();
            Debug.Print("AddToSummary End");


            return result;
        }

        /// <summary>
        /// When True Summary records are not updated as a result of a detail row update.
        /// </summary>
        protected bool dontSyncSummary = false;
        protected virtual void UpdateAdjustingDetails(EPEquipmentSummary summary)
        {
            if (summary == null)
                throw new ArgumentNullException();

            EPEquipmentTimeCard doc = PXSelect<EPEquipmentTimeCard, Where<EPEquipmentTimeCard.timeCardCD, Equal<Required<EPEquipmentTimeCard.timeCardCD>>>>.Select(this, summary.TimeCardCD);
            if (doc == null)
                return;

            Dictionary<DayOfWeek, DayDetails> dict = GetDetailRecords(summary, doc);


            PX.Objects.EP.PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, doc.WeekID.Value);

            if (weekInfo.Mon.Enabled)
                UpdateAdjustingDetails(summary, dict, DayOfWeek.Monday, weekInfo.Mon.Date.Value);
            if (weekInfo.Tue.Enabled)
                UpdateAdjustingDetails(summary, dict, DayOfWeek.Tuesday, weekInfo.Tue.Date.Value);
            if (weekInfo.Wed.Enabled)
                UpdateAdjustingDetails(summary, dict, DayOfWeek.Wednesday, weekInfo.Wed.Date.Value);
            if (weekInfo.Thu.Enabled)
                UpdateAdjustingDetails(summary, dict, DayOfWeek.Thursday, weekInfo.Thu.Date.Value);
            if (weekInfo.Fri.Enabled)
                UpdateAdjustingDetails(summary, dict, DayOfWeek.Friday, weekInfo.Fri.Date.Value);
            if (weekInfo.Sat.Enabled)
                UpdateAdjustingDetails(summary, dict, DayOfWeek.Saturday, weekInfo.Sat.Date.Value);
            if (weekInfo.Sun.Enabled)
                UpdateAdjustingDetails(summary, dict, DayOfWeek.Sunday, weekInfo.Sun.Date.Value);
        }

        protected virtual void UpdateAdjustingDetails(EPEquipmentSummary summary, Dictionary<DayOfWeek, DayDetails> dict, DayOfWeek dayOfWeek, DateTime startDate)
        {
            if (summary == null)
                throw new ArgumentNullException(nameof(summary));

            if (dict == null)
                throw new ArgumentNullException(nameof(dict));

            Debug.Print("UpdateAdjustingDetails for {0} Start", dayOfWeek);
            Debug.Indent();

            int summaryTimeTotal = 0;

            if (Summary.Cache.GetStatus(summary) != PXEntryStatus.Deleted && Summary.Cache.GetStatus(summary) != PXEntryStatus.InsertedDeleted)
                summaryTimeTotal = summary.GetTimeTotal(dayOfWeek).GetValueOrDefault();

            int dayTotal = 0;
            if (dict.ContainsKey(dayOfWeek))
            {
                dayTotal = dict[dayOfWeek].GetTotalTime();
            }

            EPEquipmentDetail adjust = null;
            if (dict.ContainsKey(dayOfWeek))
            {
                adjust = dict[dayOfWeek].GetAdjustingActivity();
            }

            if (summaryTimeTotal != dayTotal)
            {
                if (adjust == null && summaryTimeTotal - dayTotal != 0)
                {
                    adjust = (EPEquipmentDetail)Details.Cache.CreateInstance();

                    adjust.Date = startDate;
                    adjust.IsBillable = summary.IsBillable;
                    if (!string.IsNullOrEmpty(summary.Description))
                        adjust.Description = summary.Description;
                    else
                    {
						adjust.Description = PXMessages.LocalizeFormatNoPrefixNLA(Messages.SummaryRecord, DateTimeFormatInfo.CurrentInfo?.GetDayName(dayOfWeek));
                    }
                    switch (summary.RateType)
                    {
                        case "ST":
                            adjust.SetupTime = (summaryTimeTotal - dayTotal);
                            adjust.SetupSummaryLineNbr = summary.LineNbr;
                            break;
                        case "SD":
                            adjust.SuspendTime = (summaryTimeTotal - dayTotal);
                            adjust.SuspendSummaryLineNbr = summary.LineNbr;
                            break;
                        default:
                            adjust.RunTime = (summaryTimeTotal - dayTotal);
                            adjust.RunSummaryLineNbr = summary.LineNbr;
                            break;
                    }


                    adjust.ProjectID = summary.ProjectID;
                    adjust.ProjectTaskID = summary.ProjectTaskID;
					adjust.CostCodeID = summary.CostCodeID;

                    dontSyncSummary = true;
                    try
                    {
                        adjust = Details.Insert(adjust);
                    }
                    finally
                    {
                        dontSyncSummary = false;
                    }
                }
                else if (adjust != null && summaryTimeTotal == 0 && adjust.OrigLineNbr == null)//delete only adjusting activity that was added automatically on summary update.
                {
                    dontSyncSummary = true;
                    try
                    {
                        Details.Delete(adjust); 
                    }
                    finally
                    {
                        dontSyncSummary = false;
                    }
                }
                else if (adjust != null)
                {
                    switch (summary.RateType)
                    {
                        case "ST":
                            adjust.SetupTime = (adjust.SetupTime + summaryTimeTotal - dayTotal);
                            break;
                        case "SD":
                            adjust.SuspendTime = (adjust.SuspendTime + summaryTimeTotal - dayTotal);
                            break;
                        default:
                            adjust.RunTime = (adjust.RunTime + summaryTimeTotal - dayTotal);
                            break;
                    }

                    if (!string.IsNullOrEmpty(summary.Description))
                        adjust.Description = summary.Description;
                    adjust.IsBillable = summary.IsBillable;
                    adjust.ProjectID = summary.ProjectID;
                    adjust.ProjectTaskID = summary.ProjectTaskID;

                    dontSyncSummary = true;
                    try
                    {
                        Details.Update(adjust);
                    }
                    finally
                    {
                        dontSyncSummary = false;
                    }

                }
            }
            else
            {
                if (adjust != null)
                {
                    if (!string.IsNullOrEmpty(summary.Description))
                        adjust.Description = summary.Description;
                    adjust.IsBillable = summary.IsBillable;
                    adjust.ProjectID = summary.ProjectID;
                    adjust.ProjectTaskID = summary.ProjectTaskID;
					adjust.CostCodeID = summary.CostCodeID;

					dontSyncSummary = true;
                    try
                    {
                        Details.Update(adjust);
                    }
                    finally
                    {
                        dontSyncSummary = false;
                    }

                }
            }

            Debug.Unindent();
            Debug.Print("UpdateAdjustingDetails for {0} End", dayOfWeek);
        }

        protected virtual Dictionary<DayOfWeek, DayDetails> GetDetailRecords(EPEquipmentSummary summary, EPEquipmentTimeCard doc)
        {
            if (summary == null)
                throw new ArgumentNullException("summary");
            if (doc == null)
                throw new ArgumentNullException("doc");

            Dictionary<DayOfWeek, DayDetails> dict = new Dictionary<DayOfWeek, DayDetails>();

            foreach (EPEquipmentDetail detail in GetDetails(summary, doc))
            {
                DayOfWeek day = detail.Date.Value.DayOfWeek;

                if (dict.ContainsKey(detail.Date.Value.DayOfWeek))
                {
                    dict[detail.Date.Value.DayOfWeek].Details.Add(detail);
                }
                else
                {
                    DayDetails d = new DayDetails(summary.RateType);
                    d.Day = day;
                    d.Details.Add(detail);
                    dict.Add(detail.Date.Value.DayOfWeek, d);
                }
            }

            return dict;
        }


        protected virtual List<EPEquipmentDetail> GetDetails(EPEquipmentSummary summary, EPEquipmentTimeCard doc)
        {
            if (summary == null)
                throw new ArgumentNullException("summary");
            if (doc == null)
                throw new ArgumentNullException("doc");

            PXSelectBase<EPEquipmentDetail> select = new PXSelect<EPEquipmentDetail,
            Where<EPEquipmentDetail.timeCardCD, Equal<Current<EPEquipmentSummary.timeCardCD>>,
            And<Where<EPEquipmentDetail.setupSummarylineNbr, Equal<Current<EPEquipmentSummary.lineNbr>>,
                Or<EPEquipmentDetail.runSummarylineNbr, Equal<Current<EPEquipmentSummary.lineNbr>>,
                Or<EPEquipmentDetail.suspendSummarylineNbr, Equal<Current<EPEquipmentSummary.lineNbr>>>>>>>>(this);

            List<object> resultset = select.View.SelectMultiBound(new object[] { summary, doc });

            List<EPEquipmentDetail> result = new List<EPEquipmentDetail>(resultset.Count);
            foreach (EPEquipmentDetail item in resultset)
            {
                result.Add(item);
            }

            return result;
        }

        protected virtual void RecalculateTotals(EPEquipmentTimeCard timecard)
        {
            if (timecard == null)
                throw new ArgumentNullException();

            List<EPEquipmentDetail> list = new List<EPEquipmentDetail>();

            foreach (EPEquipmentDetail detail in Details.Select())
            {
                list.Add(detail);
            }

            RecalculateTotals(timecard, list);
        }

        protected virtual void RecalculateTotals(EPEquipmentTimeCard timecard, List<EPEquipmentDetail> details)
        {
            if (timecard == null)
                throw new ArgumentNullException("timecard");

            if (details == null)
                throw new ArgumentNullException("details");

            int setup = 0;
            int run = 0;
            int suspend = 0;
            int setupBillable = 0;
            int runBillable = 0;
            int suspendBillable = 0;

            foreach (EPEquipmentDetail detail in details)
            {
                setup += detail.SetupTime.GetValueOrDefault();
                run += detail.RunTime.GetValueOrDefault();
                suspend += detail.SuspendTime.GetValueOrDefault();

                if (detail.IsBillable == true)
                {
                    setupBillable += detail.SetupTime.GetValueOrDefault();
                    runBillable += detail.RunTime.GetValueOrDefault();
                    suspendBillable += detail.SuspendTime.GetValueOrDefault();
                }
            }


            timecard.TimeSetupCalc = setup;
            timecard.TimeRunCalc = run;
            timecard.TimeSuspendCalc = suspend;

            timecard.TimeBillableSetupCalc = setupBillable;
            timecard.TimeBillableRunCalc = runBillable;
            timecard.TimeBillableSuspendCalc = suspendBillable;

        }

        protected virtual bool IsFirstTimeCard(int? equipmentID)
        {
            return equipmentID == null ||
                PXSelectReadonly<EPEquipmentTimeCard,
                    Where<EPEquipmentTimeCard.equipmentID, Equal<Required<EPEquipmentTimeCard.equipmentID>>>>.
                SelectWindowed(this, 0, 1, equipmentID).Count == 0;
        }

        protected virtual int? GetNextWeekID(int? equipmentID)
        {
            var isFist = IsFirstTimeCard(equipmentID);
            if (!isFist)
            {
                var lastCard = (EPEquipmentTimeCard)PXSelectReadonly<EPEquipmentTimeCard,
                    Where<EPEquipmentTimeCard.equipmentID, Equal<Required<EPEquipmentTimeCard.equipmentID>>>,
                    OrderBy<Desc<EPEquipmentTimeCard.weekId>>>.
                    SelectWindowed(this, 0, 1, equipmentID);
                if (lastCard != null && lastCard.WeekID != null)
                {
                    return PXWeekSelector2Attribute.GetNextWeekID(this, lastCard.WeekID.Value);
                }
            }
            return Accessinfo.BusinessDate.With(_ => PXWeekSelector2Attribute.GetWeekID(this, _));
        }

        protected virtual void InsertPMTran(RegisterEntry pmGraph, EPEquipmentDetail row, int? inventoryID, int? minutes, decimal? rate, int? equipmentID)
        {
            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);
            if (item != null && minutes != null)
            {
                if (item.InvtAcctID == null)
                {
                    throw new PXException(Messages.ExpenseAccrualIsRequired, item.InventoryCD.Trim());
                }

                if (item.InvtSubID == null)
                {
                    throw new PXException(Messages.ExpenseAccrualSubIsRequired, item.InventoryCD.Trim());
                }

                PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, row.ProjectID);

                decimal qtyInBase = INUnitAttribute.ConvertGlobalUnits(this, ActivityTimeUnit, item.BaseUnit, minutes.GetValueOrDefault(), INPrecision.QUANTITY);

                int? accountID = item.COGSAcctID;
                int? offsetaccountID = item.InvtAcctID;
                int? accountGroupID = null;
                string subCD = null;
                string offsetSubCD = null;

                if (project.NonProject != true)
                {
                    PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, row.ProjectID, row.ProjectTaskID);
                    EPEquipment equipemnt = PXSelect<EPEquipment, Where<EPEquipment.equipmentID, Equal<Required<EPEquipment.equipmentID>>>>.Select(this, equipmentID);

                    #region Combine Account and Subaccount

                    if (ExpenseAccountSource == PMAccountSource.Project)
                    {
                        if (project.DefaultAccountID != null)
                        {
                            accountID = project.DefaultAccountID;
                            Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
                            if (account.AccountGroupID == null)
                            {
                                throw new PXException(Messages.NoAccountGroupOnProject, account.AccountCD.Trim(), project.ContractCD.Trim());
                            }
                            accountGroupID = account.AccountGroupID;
                        }
                        else
                        {
                            PXTrace.WriteWarning(Messages.NoDefualtAccountOnProject, project.ContractCD.Trim());
                        }
                    }
                    else if (ExpenseAccountSource == PMAccountSource.Task)
                    {

                        if (task.DefaultAccountID != null)
                        {
                            accountID = task.DefaultAccountID;
                            Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
                            if (account.AccountGroupID == null)
                            {
                                throw new PXException(Messages.NoAccountGroupOnTask, account.AccountCD.Trim(), project.ContractCD.Trim(), task.TaskCD.Trim());
                            }
                            accountGroupID = account.AccountGroupID;
                        }
                        else
                        {
                            PXTrace.WriteWarning(Messages.NoDefualtAccountOnTask, project.ContractCD.Trim(), task.TaskCD.Trim());
                        }
                    }
                    else if (ExpenseAccountSource == PMAccountSource.Employee)//!! change to Resource/Equipment
                    {
                        if (equipemnt.DefaultAccountID != null)
                        {
                            accountID = equipemnt.DefaultAccountID;
                            Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
                            if (account.AccountGroupID == null)
                            {
                                throw new PXException(Messages.NoAccountGroupOnEquipment, account.AccountCD.Trim(), equipemnt.EquipmentCD.Trim());
                            }
                            accountGroupID = account.AccountGroupID;
                        }
                        else
                        {
                            PXTrace.WriteWarning(Messages.NoDefaultAccountOnEquipment, equipemnt.EquipmentCD.Trim());
                        }
                    }
                    else
                    {
                        if (accountID == null)
                        {
                            throw new PXException(Messages.NoExpenseAccountOnInventory, item.InventoryCD.Trim());
                        }

                        //defaults to InventoryItem.COGSAcctID
                        Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
                        if (account.AccountGroupID == null)
                        {
                            throw new PXException(Messages.NoAccountGroupOnInventory, account.AccountCD.Trim(), item.InventoryCD.Trim());
                        }
                        accountGroupID = account.AccountGroupID;
                    }

                    if (accountGroupID == null)
                    {
                        //defaults to InventoryItem.COGSAcctID
                        Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
                        if (account.AccountGroupID == null)
                        {
                            throw new PXException(Messages.AccountGroupIsNotAssignedForAccount, account.AccountCD.Trim());
                        }
                        accountGroupID = account.AccountGroupID;
                    }


                    if (!string.IsNullOrEmpty(ExpenseSubMask))
                    {
						if (ExpenseSubMask.Contains(PMAccountSource.InventoryItem) && item.COGSSubID == null)
						{
							throw new PXException(Messages.NoExpenseSubOnInventory, item.InventoryCD.Trim());
						}
						if (ExpenseSubMask.Contains(PMAccountSource.Project) && project.DefaultSubID == null)
                        {
                            throw new PXException(Messages.NoExpenseSubOnProject, project.ContractCD.Trim());
                        }
                        if (ExpenseSubMask.Contains(PMAccountSource.Task) && task.DefaultSubID == null)
                        {
                            throw new PXException(Messages.NoExpenseSubOnTask, project.ContractCD.Trim(), task.TaskCD.Trim());
                        }
                        if (ExpenseSubMask.Contains(PMAccountSource.Employee) && equipemnt.DefaultSubID == null)
                        {
                            throw new PXException(Messages.NoDefaultSubOnEquipment, equipemnt.EquipmentCD.Trim());
                        }


                        subCD = PM.SubAccountMaskAttribute.MakeSub<PMSetup.expenseSubMask>(this, ExpenseSubMask,
                            new object[] { item.COGSSubID, project.DefaultSubID, task.DefaultSubID, equipemnt.DefaultSubID },
                            new Type[] { typeof(InventoryItem.cOGSSubID), typeof(PMProject.defaultSubID), typeof(PMTask.defaultSubID), typeof(EPEquipment.defaultSubID) });
                    }

                    #endregion

                    #region Combine Accrual Account and Subaccount

                    if (ExpenseAccrualAccountSource == PMAccountSource.Project)
                    {
                        if (project.DefaultAccrualAccountID != null)
                        {
                            offsetaccountID = project.DefaultAccrualAccountID;
                        }
                        else
                        {
                            PXTrace.WriteWarning(EP.Messages.NoDefualtAccrualAccountOnProject, project.ContractCD.Trim());
                        }
                    }
                    else if (ExpenseAccrualAccountSource == PMAccountSource.Task)
                    {
                        if (task.DefaultAccrualAccountID != null)
                        {
                            offsetaccountID = task.DefaultAccrualAccountID;
                        }
                        else
                        {
                            PXTrace.WriteWarning(EP.Messages.NoDefualtAccountOnTask, project.ContractCD.Trim(), task.TaskCD.Trim());
                        }
                    }
                    else
                    {
                        if (offsetaccountID == null)
                        {
                            throw new PXException(EP.Messages.NoAccrualExpenseAccountOnInventory, item.InventoryCD.Trim());
                        }
                    }

                    if (!string.IsNullOrEmpty(ExpenseAccrualSubMask))
                    {
						if (ExpenseAccrualSubMask.Contains(PMAccountSource.InventoryItem) && item.InvtSubID == null)
						{
							throw new PXException(EP.Messages.NoExpenseAccrualSubOnInventory, item.InventoryCD.Trim());
						}
						if (ExpenseAccrualSubMask.Contains(PMAccountSource.Project) && project.DefaultAccrualSubID == null)
                        {
                            throw new PXException(EP.Messages.NoExpenseAccrualSubOnProject, project.ContractCD.Trim());
                        }
                        if (ExpenseAccrualSubMask.Contains(PMAccountSource.Task) && task.DefaultAccrualSubID == null)
                        {
                            throw new PXException(EP.Messages.NoExpenseAccrualSubOnTask, project.ContractCD.Trim(), task.TaskCD.Trim());
                        }

						if (ExpenseAccrualSubMask.Contains(PMAccountSource.Employee) && equipemnt.DefaultSubID == null)
						{
							throw new PXException(Messages.NoDefaultSubOnEquipment, equipemnt.EquipmentCD.Trim());
						}
						
						offsetSubCD = PM.SubAccountMaskAttribute.MakeSub<PMSetup.expenseAccrualSubMask>(this, ExpenseAccrualSubMask,
							new object[] { item.InvtSubID, project.DefaultAccrualSubID, task.DefaultAccrualSubID, equipemnt.DefaultSubID },
							new Type[] { typeof(InventoryItem.invtSubID), typeof(PMProject.defaultAccrualSubID), typeof(PMTask.defaultAccrualSubID), typeof(EPEquipment.defaultSubID) });

                    }

                    #endregion
                }
                else
                {
                    //defaults to InventoryItem.COGSAcctID
                    Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
                    if (account.AccountGroupID == null)
                    {
                        throw new PXException(Messages.NoAccountGroupOnInventory, account.AccountCD.Trim(), item.InventoryCD.Trim());
                    }
                    accountGroupID = account.AccountGroupID;
                }

                int? subID = item.COGSSubID;
                int? offsetSubID = item.InvtSubID;
                EPSetup epsetup = PXSelect<EPSetup>.Select(this);
                if (epsetup != null && epsetup.PostingOption == EPPostOptions.PostToOffBalance)
                {
                    accountGroupID = epsetup.OffBalanceAccountGroupID;
                    accountID = null;
                    offsetaccountID = null;
                    offsetSubID = null;
                    subCD = null;
                    subID = null;
                }

                PMTran tran = (PMTran)pmGraph.Transactions.Cache.Insert();
                tran.AccountID = accountID;
                if (string.IsNullOrEmpty(subCD))
                    tran.SubID = item.COGSSubID;
                if (string.IsNullOrEmpty(offsetSubCD))
                    tran.OffsetSubID = offsetSubID;
                tran.AccountGroupID = accountGroupID;
                tran.ProjectID = row.ProjectID;
                tran.TaskID = row.ProjectTaskID;
				tran.CostCodeID = row.CostCodeID;
				tran.InventoryID = inventoryID;
                tran.Description = row.Description;
                tran.Qty = qtyInBase;
                tran.Billable = row.IsBillable;
                tran.BillableQty = qtyInBase;
                tran.UOM = item.BaseUnit;
				tran.TranCuryUnitRate = rate;
				tran.OffsetAccountID = item.InvtAcctID;
                tran.Date = row.Date;
                tran.StartDate = row.Date;
                tran.EndDate = row.Date;

                pmGraph.Transactions.Update(tran);

                if (!string.IsNullOrEmpty(subCD))
                    pmGraph.Transactions.SetValueExt<PMTran.subID>(tran, subCD);

                if (!string.IsNullOrEmpty(offsetSubCD))
                    pmGraph.Transactions.SetValueExt<PMTran.offsetSubID>(tran, offsetSubCD);

				PXNoteAttribute.CopyNoteAndFiles(Details.Cache, row, pmGraph.Transactions.Cache, tran, Setup.Current.GetCopyNoteSettings<PXModule.pm>());
            }
        }

        protected virtual decimal? GetRunCost(int? equipmentID, int? projectID)
        {
            EPEquipmentRate rate = PXSelect<EPEquipmentRate, Where<EPEquipmentRate.equipmentID, Equal<Required<EPEquipmentRate.equipmentID>>, And<EPEquipmentRate.projectID, Equal<Required<EPEquipmentRate.projectID>>>>>.Select(this, equipmentID, projectID);
            if (rate != null)
            {
                return rate.RunRate;
            }
            else
            {
                EPEquipment equipment = PXSelect<EPEquipment, Where<EPEquipment.equipmentID, Equal<Required<EPEquipment.equipmentID>>>>.Select(this, equipmentID);

                if (equipment != null)
                    return equipment.RunRate;
                else
                {
                    PXTrace.WriteWarning("Failed to determine Run Cost for Equipment. Equipment with the given id was not found in the system. EquipmentID={0}", equipmentID);
                    return null;
                }
            }
        }

        protected virtual decimal? GetSetupCost(int? equipmentID, int? projectID)
        {
            EPEquipmentRate rate = PXSelect<EPEquipmentRate, Where<EPEquipmentRate.equipmentID, Equal<Required<EPEquipmentRate.equipmentID>>, And<EPEquipmentRate.projectID, Equal<Required<EPEquipmentRate.projectID>>>>>.Select(this, equipmentID, projectID);
            if (rate != null)
            {
                return rate.SetupRate;
            }
            else
            {
                EPEquipment equipment = PXSelect<EPEquipment, Where<EPEquipment.equipmentID, Equal<Required<EPEquipment.equipmentID>>>>.Select(this, equipmentID);

                if (equipment != null)
                    return equipment.SetupRate;
                else
                {
                    PXTrace.WriteWarning("Failed to determine Setup Cost for Equipment. Equipment with the given id was not found in the system. EquipmentID={0}", equipmentID);
                    return null;
                }
            }
        }

        protected virtual decimal? GetSuspendCost(int? equipmentID, int? projectID)
        {
            EPEquipmentRate rate = PXSelect<EPEquipmentRate, Where<EPEquipmentRate.equipmentID, Equal<Required<EPEquipmentRate.equipmentID>>, And<EPEquipmentRate.projectID, Equal<Required<EPEquipmentRate.projectID>>>>>.Select(this, equipmentID, projectID);
            if (rate != null)
            {
                return rate.SuspendRate;
            }
            else
            {
                EPEquipment equipment = PXSelect<EPEquipment, Where<EPEquipment.equipmentID, Equal<Required<EPEquipment.equipmentID>>>>.Select(this, equipmentID);

                if (equipment != null)
                    return equipment.SuspendRate;
                else
                {
                    PXTrace.WriteWarning("Failed to determine Suspend Cost for Equipment. Equipment with the given id was not found in the system. EquipmentID={0}", equipmentID);
                    return null;
                }
            }
        }
        #endregion

        #region Local Types

        public class DayDetails
        {
            private string rateType;
            public List<EPEquipmentDetail> Details;
            public DayOfWeek Day;

            public DayDetails(string rateType)
            {
                this.rateType = rateType;
                Details = new List<EPEquipmentDetail>();
            }

            public int GetTotalTime()
            {
                int total = 0;
                foreach (EPEquipmentDetail item in Details)
                {
                    switch (rateType)
                    {
                        case "ST":
                            total += item.SetupTime.GetValueOrDefault();
                            break;
                        case "SD":
                            total += item.SuspendTime.GetValueOrDefault();
                            break;
                        default:
                            total += item.RunTime.GetValueOrDefault();
                            break;
                    }
                }

                return total;
            }

            public EPEquipmentDetail GetAdjustingActivity()
            {
                if (Details.Count > 0)
                {
                    return Details[Details.Count - 1];
                }

                return null;
            }

        }

        [PXHidden]
        public class EPEquipmentDetailOrig : EPEquipmentDetail
        {
            public new abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }
            public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
            public new abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
        }

        [PXHidden]
        public class EPEquipmentDetailEx : EPEquipmentDetail
        {
            public new abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }
            public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
            public new abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
        }

        #endregion
    }
}
