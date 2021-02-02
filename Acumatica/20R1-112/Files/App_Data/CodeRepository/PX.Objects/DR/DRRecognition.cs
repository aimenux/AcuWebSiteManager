using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using PX.Data;
using PX.Objects.GL;
using PX.Objects.GL.DAC.Abstract;
using PX.Objects.GL.FinPeriods;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects.Common.Tools;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.DR
{
	[TableAndChartDashboardType]
	public class DRRecognition: PXGraph<DRRecognition>
	{
		public PXCancel<ScheduleRecognitionFilter> Cancel;
		public PXAction<ScheduleRecognitionFilter> viewSchedule;
		public PXFilter<ScheduleRecognitionFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<ScheduledTran, ScheduleRecognitionFilter> Items;
		public PXSetup<DRSetup> Setup;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

	    public virtual IEnumerable items()
		{
			ScheduleRecognitionFilter filter = Filter.Current;
			if (filter == null)
			{
				yield break;
			}
			bool found = false;
			foreach (ScheduledTran item in Items.Cache.Inserted)
			{
				found = true;
				yield return item;
			}
			if (found)
				yield break;


			PXSelectBase<DRScheduleTran> select = new PXSelectJoin<DRScheduleTran,
				InnerJoin<DRScheduleDetail,
					On<DRScheduleTran.scheduleID, Equal<DRScheduleDetail.scheduleID>,
					And<DRScheduleTran.componentID, Equal<DRScheduleDetail.componentID>,
					And<DRScheduleTran.detailLineNbr, Equal<DRScheduleDetail.detailLineNbr>>>>,
				InnerJoin<DRSchedule, On<DRScheduleTran.scheduleID, Equal<DRSchedule.scheduleID>>,
				LeftJoin<InventoryItem, On<DRScheduleTran.componentID, Equal<InventoryItem.inventoryID>>>>>,
						Where<DRScheduleTran.recDate, LessEqual<Current<ScheduleRecognitionFilter.recDate>>,
							And<DRScheduleTran.status, Equal<DRScheduleTranStatus.OpenStatus>,
							And<DRScheduleDetail.status, NotEqual<DRScheduleStatus.DraftStatus>>>>,
				OrderBy<
					Asc<DRScheduleTran.scheduleID,
					Asc<DRScheduleTran.componentID,
					Asc<DRScheduleTran.detailLineNbr,
					Asc<DRScheduleTran.recDate,
					Asc<DRScheduleTran.lineNbr>>>>>>>(this);

			if (!string.IsNullOrEmpty(filter.DeferredCode))
			{
				select.WhereAnd<Where<DRScheduleDetail.defCode, Equal<Current<ScheduleRecognitionFilter.deferredCode>>>>();
			}

			if (filter.BranchID != null)
			{
				select.WhereAnd<Where<DRScheduleDetail.branchID, Equal<Current<ScheduleRecognitionFilter.branchID>>>>();
			}

			Dictionary<string, string> added = new Dictionary<string, string>();

			foreach (PXResult<DRScheduleTran, DRScheduleDetail, DRSchedule, InventoryItem> resultSet in select.Select())
			{
				DRScheduleTran tran = (DRScheduleTran)resultSet;
				DRSchedule schedule = (DRSchedule)resultSet;
				DRScheduleDetail scheduleDetail = (DRScheduleDetail)resultSet;
				InventoryItem item = (InventoryItem)resultSet;

				string key = string.Format("{0}.{1}.{2}", tran.ScheduleID, tran.ComponentID, tran.DetailLineNbr);

				bool doInsert = false;

				if (added.ContainsKey(key))
				{
					string addedFinPeriod = added[key];

					if (tran.FinPeriodID == addedFinPeriod)
					{
						doInsert = true;
					}
				}
				else
				{
					doInsert = true;
					added.Add(key, tran.FinPeriodID);
				}

				if (doInsert)
				{
					ScheduledTran result = new ScheduledTran();
					result.BranchID = tran.BranchID;
					result.AccountID = tran.AccountID;
					result.Amount = tran.Amount;
					result.ComponentID = tran.ComponentID;
					result.DetailLineNbr = tran.DetailLineNbr;
					result.DefCode = scheduleDetail.DefCode;
					result.FinPeriodID = tran.FinPeriodID;
					result.LineNbr = tran.LineNbr;
					result.RecDate = tran.RecDate;
					result.ScheduleID = tran.ScheduleID;
					result.ScheduleNbr = schedule.ScheduleNbr;
					result.SubID = tran.SubID;
					result.ComponentCD = item.InventoryCD;
					result.DocType = DRScheduleDocumentType.BuildDocumentType(scheduleDetail.Module, scheduleDetail.DocType);
					
					Items.Cache.SetStatus(result, PXEntryStatus.Inserted);
					yield return result;// Items.Insert(result);
				}
			}


			//Virtual Records (CashReceipt):
			
			PXSelectBase<ARInvoice> s = null;

			if (!string.IsNullOrEmpty(filter.DeferredCode))
			{
				s = new PXSelectJoinGroupBy<ARInvoice,
				InnerJoin<ARTran, On<ARTran.tranType, Equal<ARInvoice.docType>,
					And<ARTran.refNbr, Equal<ARInvoice.refNbr>>>,
				InnerJoin<DRDeferredCode, On<ARTran.deferredCode, Equal<DRDeferredCode.deferredCodeID>,
					And<DRDeferredCode.method, Equal<DeferredMethodType.cashReceipt>,
					And<DRDeferredCode.deferredCodeID, Equal<Current<ScheduleRecognitionFilter.deferredCode>>>>>,
				InnerJoin<DRSchedule, On<ARTran.tranType, Equal<DRSchedule.docType>,
					And<ARTran.refNbr, Equal<DRSchedule.refNbr>,
					And<ARTran.lineNbr, Equal<DRSchedule.lineNbr>>>>,
				InnerJoin<DRScheduleDetail, On<DRSchedule.scheduleID, Equal<DRScheduleDetail.scheduleID>>>>>>,
				Where<ARInvoice.released, Equal<True>,
				And<DRScheduleDetail.isOpen, Equal<True>>>,
				Aggregate<GroupBy<ARInvoice.docType, GroupBy<ARInvoice.refNbr>>>>(this);
			}
			else
			{
				s = new PXSelectJoinGroupBy<ARInvoice,
				InnerJoin<ARTran, On<ARTran.tranType, Equal<ARInvoice.docType>,
					And<ARTran.refNbr, Equal<ARInvoice.refNbr>>>,
				InnerJoin<DRDeferredCode, On<ARTran.deferredCode, Equal<DRDeferredCode.deferredCodeID>,
					And<DRDeferredCode.method, Equal<DeferredMethodType.cashReceipt>>>,
				InnerJoin<DRSchedule, On<ARTran.tranType, Equal<DRSchedule.docType>,
					And<ARTran.refNbr, Equal<DRSchedule.refNbr>,
					And<ARTran.lineNbr, Equal<DRSchedule.lineNbr>>>>,
				InnerJoin<DRScheduleDetail, On<DRSchedule.scheduleID, Equal<DRScheduleDetail.scheduleID>>>>>>,
				Where<ARInvoice.released, Equal<True>,
				And<DRScheduleDetail.isOpen, Equal<True>>>,
				Aggregate<GroupBy<ARInvoice.docType, GroupBy<ARInvoice.refNbr>>>>(this);
			}


			foreach (ARInvoice inv in s.Select())
			{
				PXSelectBase<ARTran> trs =
					new PXSelectJoin<ARTran,
						InnerJoin<DRDeferredCode, On<ARTran.deferredCode, Equal<DRDeferredCode.deferredCodeID>,
						And<DRDeferredCode.method, Equal<DeferredMethodType.cashReceipt>>>>,
						Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
						And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>(this);

				foreach ( PXResult<ARTran, DRDeferredCode> res in trs.Select(inv.DocType, inv.RefNbr) )
				{
					List<ScheduledTran> virtualRecords = new List<ScheduledTran>();
					List<ScheduledTran> virtualVoidedRecords = new List<ScheduledTran>();

					ARTran tr = (ARTran)res;
					DRDeferredCode dc = (DRDeferredCode)res;

					decimal trPart = 0;
					if ( inv.LineTotal.Value != 0 )
						trPart = tr.TranAmt.Value / inv.LineTotal.Value;
					decimal trPartRest = tr.TranAmt.Value;

					InventoryItem invItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, tr.InventoryID);
					
					//NOTE: Multiple Components are not supported in CashReceipt Deferred Revenue Recognition.
					DRSchedule schedule = GetScheduleByFID(BatchModule.AR, inv.DocType, inv.RefNbr, tr.LineNbr);
					DRScheduleDetail scheduleDetail = GetScheduleDetailbyID(schedule.ScheduleID, tr.InventoryID != null ? tr.InventoryID : DRScheduleDetail.EmptyComponentID );
					int lineNbr = scheduleDetail.LineCntr ?? 0;
					
					
					PXSelectBase<ARAdjust> ads =
						new PXSelectJoin<ARAdjust,
							LeftJoin<DRScheduleTran, On<ARAdjust.adjgDocType, Equal<DRScheduleTran.adjgDocType>,
								And<ARAdjust.adjgRefNbr, Equal<DRScheduleTran.adjgRefNbr>>>>,
							Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
							And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
							And<DRScheduleTran.scheduleID, IsNull,
							And<ARAdjust.adjgDocType, NotEqual<ARDocType.creditMemo>>>>>,
							OrderBy<Asc<ARAdjust.adjgDocDate>>>(this);

                    foreach (ARAdjust ad in ads.Select(inv.DocType, inv.RefNbr))
					{
						lineNbr++;
						decimal amtRaw = Math.Min(trPart * ad.AdjAmt.Value, trPartRest);
						trPartRest -= amtRaw;
						decimal amt = PXDBCurrencyAttribute.BaseRound(this, amtRaw);

						ScheduledTran result = new ScheduledTran();

					    result.BranchID = ad.AdjgBranchID;
						result.Amount = amt;
						result.ComponentID = tr.InventoryID;
						result.DefCode = tr.DeferredCode;
						result.FinPeriodID = FinPeriodRepository.GetPeriodIDFromDate(ad.AdjgDocDate, PXAccess.GetParentOrganizationID(ad.AdjgBranchID));
						result.LineNbr = lineNbr;
						result.DetailLineNbr = scheduleDetail.LineNbr;
						result.Module = schedule.Module;
						result.RecDate = ad.AdjgDocDate;
						result.ScheduleID = schedule.ScheduleID;
						result.ScheduleNbr = schedule.ScheduleNbr;
						result.DocType = schedule.DocType;
						result.AdjgDocType = ad.AdjgDocType;
						result.AdjgRefNbr = ad.AdjgRefNbr;
						result.AdjNbr = ad.AdjNbr;
						result.IsVirtual = true;
						result.AccountID = scheduleDetail.AccountID;
						result.SubID = scheduleDetail.SubID;
						result.ComponentCD = invItem == null ? "" : invItem.InventoryCD;
												
						if (ad.Voided == true)
						{
							if (ad.AdjgDocType == ARDocType.VoidPayment && virtualVoidedRecords.Count > 0)
							{
								ScheduledTran tran = virtualVoidedRecords.First(v => v.AdjgDocType == ARDocType.Payment && v.AdjgRefNbr == ad.AdjgRefNbr && v.AdjNbr == ad.VoidAdjNbr);
								if (tran != null)
									virtualVoidedRecords.Remove(tran);
							}
							else
							{
								virtualVoidedRecords.Add(result);
							}
						}
						else
						{
							
							virtualRecords.Add(result);
						}
					}

					foreach (ScheduledTran v in virtualRecords)
					{
						Items.Cache.SetStatus(v, PXEntryStatus.Inserted);
						yield return v;// Items.Insert(v);
					}

					foreach (ScheduledTran v in virtualVoidedRecords)
					{
						Items.Cache.SetStatus(v, PXEntryStatus.Inserted);
						yield return v;// Items.Insert(v);
					}

				}
			}
			

			Items.Cache.IsDirty = false;
		}

		#region Actions / Buttons

		
		[PXUIField(DisplayName = "", Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			if (Items.Current != null)
			{
				DRRedirectHelper.NavigateToDeferralSchedule(this, Items.Current.ScheduleID);
			}
			return adapter.Get();
		} 

		#endregion

				
		public DRRecognition()
		{
			DRSetup setup = Setup.Current;
			Items.SetSelected<ScheduledTran.selected>();
		}

		#region EventHandlers
		protected virtual void ScheduleRecognitionFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Items.Cache.Clear();
		}

		protected virtual void ScheduleRecognitionFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ScheduleRecognitionFilter filter = Filter.Current;
            DateTime? filterDate = filter.RecDate;

            Items.SetProcessDelegate(
                    delegate(List<ScheduledTran> items)
					{
                        RunRecognition(items, filterDate);
					});
		}

		#endregion

		public static void RunRecognition(List<ScheduledTran> trans, DateTime? filterDate)
		{
			ScheduleMaint scheduleMaint = PXGraph.CreateInstance<ScheduleMaint>();
			scheduleMaint.Clear();

			bool failed = false;

			List<ScheduledTran> items = GetValidatedItems(trans, scheduleMaint);

			failed = items.Count() < trans.Count();

			// Save virtual records:
			// -
			foreach (ScheduledTran tr in items)
			{
				PXProcessing<ScheduledTran>.SetCurrentItem(tr);
				if (tr.IsVirtual == true)
				{
					try
					{
						scheduleMaint.Document.Current = PXSelect<DRScheduleDetail,
							Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>,
							And<DRScheduleDetail.componentID, Equal<Required<DRScheduleDetail.componentID>>,
							And<DRScheduleDetail.detailLineNbr, Equal<Required<DRScheduleDetail.detailLineNbr>>>>>>
						.Select(scheduleMaint, tr.ScheduleID, tr.ComponentID ?? DRScheduleDetail.EmptyComponentID, tr.DetailLineNbr);

						DRScheduleTran tran = new DRScheduleTran();
						tran.BranchID = tr.BranchID;
						tran.AccountID = tr.AccountID;
						tran.SubID = tr.SubID;
						tran.AdjgDocType = tr.AdjgDocType;
						tran.AdjgRefNbr = tr.AdjgRefNbr;
						tran.AdjNbr = tr.AdjNbr;
						tran.Amount = tr.Amount;
						tran.ComponentID = tr.ComponentID ?? DRScheduleDetail.EmptyComponentID;
						tran.DetailLineNbr = tr.DetailLineNbr;
						tran.FinPeriodID = tr.FinPeriodID;
						tran.ScheduleID = tr.ScheduleID;
						tran.RecDate = tr.RecDate;
						tran.Status = DRScheduleTranStatus.Open;

						tran = scheduleMaint.OpenTransactions.Insert(tran);
						tr.LineNbr = tran.LineNbr;

						scheduleMaint.RebuildProjections();

						scheduleMaint.Save.Press();
						byte[] ts = scheduleMaint.TimeStamp;
						scheduleMaint.Clear();
						scheduleMaint.TimeStamp = ts;
						PXProcessing<ScheduledTran>.SetProcessed();
					}

					catch (Exception ex)
					{
						failed = true;
						PXProcessing<ScheduledTran>.SetError(ex.Message);
					}
				}
				else
				{
					PXProcessing<ScheduledTran>.SetProcessed();
				}
			}

			PXProcessing<ScheduledTran>.SetCurrentItem(null);

			List<DRBatch> list = SplitByFinPeriod(items);

			DRProcess process = CreateInstance<DRProcess>();
			process.Clear();
			process.TimeStamp = scheduleMaint.TimeStamp;
			List<Batch> batchlist = process.RunRecognition(list, filterDate);
			if (process.Exceptions.Count > 0)
			{
				foreach (Exception e in process.Exceptions)
				{
					var item = items.Where(_ => _.ScheduleID == (int?)e.Data[typeof(DRSchedule.scheduleID).Name]).FirstOrDefault();
					PXProcessing<DRRecognition.ScheduledTran>.SetCurrentItem(item);
					PXProcessing<DRRecognition.ScheduledTran>.SetError(e.Message);
				}
				PXProcessing<ScheduledTran>.SetCurrentItem(null);
			}

			PostGraph pg = PXGraph.CreateInstance<PostGraph>();
			//Post Batches if AutoPost

			bool postFailed = false;
			if (pg.AutoPost)
			{
				foreach (Batch batch in batchlist)
				{
					try
					{
						pg.Clear();
						pg.TimeStamp = batch.tstamp;
						pg.PostBatchProc(batch);
					}
					catch (Exception)
					{
						postFailed = true;
					}
				}
				if (postFailed)
				{
					throw new PXException(Messages.AutoPostFailed);
				}
			}

			if (failed)
			{
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}
		}

		private static List<ScheduledTran> GetValidatedItems(List<ScheduledTran> items, ScheduleMaint scheduleMaint)
		{
			List<ScheduledTran> validatedItems = new List<ScheduledTran>();

			foreach (ScheduledTran item in items)
			{
				PXProcessing<ScheduledTran>.SetCurrentItem(item);
				try
				{
					FinPeriod finPeriod = scheduleMaint.FinPeriodRepository.FindByID(PXAccess.GetParentOrganizationID(item.BranchID), item.FinPeriodID);
					scheduleMaint.FinPeriodUtils.CanPostToPeriod(finPeriod).RaiseIfHasError();

					validatedItems.Add(item);
				}
				catch (Exception ex)
				{
					PXProcessing<ScheduledTran>.SetError(ex.Message);
				}
			}

			return validatedItems;
		}

		private static List<DRBatch> SplitByFinPeriod(List<ScheduledTran> items)
		{
			List<DRBatch> list = items.GroupBy(t => new { t.FinPeriodID, t.BranchID })
				.Select(b => new DRBatch(b.Key.FinPeriodID, b.Key.BranchID)
				{
					Trans = b.Select(tr => new DRTranKey(tr.ScheduleID, tr.ComponentID ?? DRScheduleDetail.EmptyComponentID, tr.DetailLineNbr, tr.LineNbr)).ToList()
				}
				).OrderBy(g => g.FinPeriod).ThenBy(g => g.BranchID).ToList();

			return list;
		}

	    public DRSchedule GetScheduleByFID(string module, string docType, string refNbr, int? lineNbr)
		{
			return PXSelect<DRSchedule,
				Where<DRSchedule.module, Equal<Required<DRSchedule.module>>,
					And<DRSchedule.docType, Equal<Required<DRSchedule.docType>>,
						And<DRSchedule.refNbr, Equal<Required<DRSchedule.refNbr>>,
					And<DRSchedule.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>>
				.Select(this, module, docType, refNbr, lineNbr);
		}

	    public DRScheduleDetail GetScheduleDetailbyID(int? scheduleID, int? inventoryID)
		{
			return PXSelect<DRScheduleDetail,
							Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>,
					And<DRScheduleDetail.componentID, Equal<Required<DRScheduleDetail.componentID>>>>>
				.Select(this, scheduleID, inventoryID);
		}
		
		#region Local Types
		[Serializable]
		public partial class ScheduleRecognitionFilter : IBqlTable
		{
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

			[Branch(PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual int? BranchID { get; set; }
			#endregion
			#region RecDate
			public abstract class recDate : PX.Data.BQL.BqlDateTime.Field<recDate> { }
			protected DateTime? _RecDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Recognition Date")]
			public virtual DateTime? RecDate
			{
				get
				{
					return this._RecDate;
				}
				set
				{
					this._RecDate = value;
				}
			}
			#endregion
			#region DeferredCode
			public abstract class deferredCode : PX.Data.BQL.BqlString.Field<deferredCode> { }
			protected String _DeferredCode;
			[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXUIField(DisplayName = "Deferral Code")]
			[PXSelector(typeof(DRDeferredCode.deferredCodeID))]
            [PXRestrictor(typeof(Where<DRDeferredCode.active, Equal<True>>), DR.Messages.InactiveDeferralCode, typeof(DRDeferredCode.deferredCodeID))]
            public virtual String DeferredCode
			{
				get
				{
					return this._DeferredCode;
				}
				set
				{
					this._DeferredCode = value;
				}
			}
			#endregion
		}

		[Serializable]
		public partial class ScheduledTran : IBqlTable, IAccountable
		{
			#region ScheduleID
			public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }
			protected Int32? _ScheduleID;
			[PXDBInt(IsKey = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual Int32? ScheduleID
			{
				get
				{
					return this._ScheduleID;
				}
				set
				{
					this._ScheduleID = value;
				}
			}
			#endregion
			#region ComponentID
			public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }
			protected int? _ComponentID;
			[PXUIField(DisplayName = Messages.ComponentID, Visibility = PXUIVisibility.Visible)]
			[PXDBInt(IsKey = true)]
			[PXSelector(typeof(Search2<DRScheduleDetail.componentID, LeftJoin<InventoryItem, On<DRScheduleDetail.componentID, Equal<InventoryItem.inventoryID>>>, Where<DRScheduleDetail.scheduleID, Equal<Current<ScheduledTran.scheduleID>>>>), new Type[] { typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr) })]
			public virtual int? ComponentID
			{
				get
				{
					return this._ComponentID;
				}
				set
				{
					this._ComponentID = value;
				}
			}
			#endregion
			#region DetailLineNbr
			public abstract class detailLineNbr : IBqlField { }
			[PXDBLiteDefault(typeof(DRScheduleDetail.detailLineNbr))]
			[PXDBInt(IsKey = true)]
			public virtual int? DetailLineNbr { get; set; }
			#endregion
			#region ScheduleNbr
			public abstract class scheduleNbr : PX.Data.BQL.BqlString.Field<scheduleNbr> { }

			[PXString(15, IsUnicode = true)]
			[PXUIField(DisplayName = Messages.ScheduleNbr)]
			[PXSelector(typeof(DRSchedule.scheduleNbr))]
			public virtual string ScheduleNbr { get; set; }
			#endregion
			#region ComponentCD
			public abstract class componentCD : PX.Data.BQL.BqlString.Field<componentCD> { }
			protected string _ComponentCD;
			[PXUIField(DisplayName = Messages.ComponentID, Visibility = PXUIVisibility.Visible)]
			[PXDBString]
			public virtual string ComponentCD
			{
				get
				{
					return this._ComponentCD;
				}
				set
				{
					this._ComponentCD = value;
				}
			}
			#endregion
			#region LineNbr
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			protected Int32? _LineNbr;
			[PXDBInt(IsKey = true)]
			[PXDefault()]
			[PXUIField(DisplayName = Messages.TransactionNumber, Enabled = false)]
			public virtual Int32? LineNbr
			{
				get
				{
					return this._LineNbr;
				}
				set
				{
					this._LineNbr = value;
				}
			}
			#endregion
            #region BranchID
            public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
            protected Int32? _BranchID;
            [Branch()]
            public virtual Int32? BranchID
            {
                get
                {
                    return this._BranchID;
                }
                set
                {
                    this._BranchID = value;
                }
            }
            #endregion
			#region Module
			public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
			protected String _Module;
			[PXDBString(2, IsFixed = true)]
			[PXDefault("")]
			[PXUIField(DisplayName = "Module")]
			public virtual String Module
			{
				get
				{
					return this._Module;
				}
				set
				{
					this._Module = value;
				}
			}
			#endregion
			#region RecDate
			public abstract class recDate : PX.Data.BQL.BqlDateTime.Field<recDate> { }
			protected DateTime? _RecDate;
			[PXDBDate()]
			[PXDefault()]
			[PXUIField(DisplayName = "Rec. Date")]
			public virtual DateTime? RecDate
			{
				get
				{
					return this._RecDate;
				}
				set
				{
					this._RecDate = value;
				}
			}
			#endregion
			#region Amount
			public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
			protected Decimal? _Amount;
			[PXDBBaseCury()]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Amount")]
			public virtual Decimal? Amount
			{
				get
				{
					return this._Amount;
				}
				set
				{
					this._Amount = value;
				}
			}
			#endregion
			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			protected Int32? _AccountID;
			[Account(DisplayName = "Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
			public virtual Int32? AccountID
			{
				get
				{
					return this._AccountID;
				}
				set
				{
					this._AccountID = value;
				}
			}
			#endregion
			#region SubID
			public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
			protected Int32? _SubID;
			//[SubAccount(typeof(DRScheduleTran.accountID), DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
            [PXInt]
			public virtual Int32? SubID
			{
				get
				{
					return this._SubID;
				}
				set
				{
					this._SubID = value;
				}
			}
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			protected String _FinPeriodID;
			[FinPeriodID()]
			[PXUIField(DisplayName = "Fin. Period", Enabled = false)]
			public virtual String FinPeriodID
			{
				get
				{
					return this._FinPeriodID;
				}
				set
				{
					this._FinPeriodID = value;
				}
			}
			#endregion
			#region DefCode
			public abstract class defCode : PX.Data.BQL.BqlString.Field<defCode> { }
			protected String _DefCode;
			[PXDBString(10, IsUnicode = true)]
			[PXDefault("")]
			[PXUIField(DisplayName = "Deferral Code", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String DefCode
			{
				get
				{
					return this._DefCode;
				}
				set
				{
					this._DefCode = value;
				}
			}
			#endregion
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			protected String _DocType;
			[PXDBString(3, IsFixed = true, InputMask = "")]
			[PXUIField(DisplayName = Messages.DocumentType)]
			[DRScheduleDocumentType.List]
			public virtual String DocType
			{
				get
				{
					return this._DocType;
				}
				set
				{
					this._DocType = value;
				}
			}
			#endregion
			
			#region AdjgDocType
			public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
			protected String _AdjgDocType;
			[PXDBString(3, IsFixed = true, InputMask = "")]
			public virtual String AdjgDocType
			{
				get
				{
					return this._AdjgDocType;
				}
				set
				{
					this._AdjgDocType = value;
				}
			}
			#endregion
			#region AdjgRefNbr
			public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
			protected String _AdjgRefNbr;
			[PXDBString(15, IsUnicode = true)]
			public virtual String AdjgRefNbr
			{
				get
				{
					return this._AdjgRefNbr;
				}
				set
				{
					this._AdjgRefNbr = value;
				}
			}
			#endregion
			#region AdjNbr
			public abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
			protected Int32? _AdjNbr;
			[PXDBInt()]
			public virtual Int32? AdjNbr
			{
				get
				{
					return this._AdjNbr;
				}
				set
				{
					this._AdjNbr = value;
				}
			}
			#endregion
			#region IsVirtual
			public abstract class isVirtual : PX.Data.BQL.BqlBool.Field<isVirtual> { }
			protected bool? _IsVirtual = false;
			[PXBool]
			[PXDefault(false)]
			public bool? IsVirtual
			{
				get
				{
					return _IsVirtual;
				}
				set
				{
					_IsVirtual = value;
				}
			}
			#endregion
				
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Visible)]
			public bool? Selected
			{
				get
				{
					return _Selected;
				}
				set
				{
					_Selected = value;
				}
			}
			#endregion

			public static int CompareFinPeriod(ScheduledTran a, ScheduledTran b)
			{
				return a.FinPeriodID.CompareTo(b.FinPeriodID);
			}
		}

		[DebuggerDisplay("{FinPeriod} Trans.Count={Trans.Count}")]
		public class DRBatch
		{
			public string FinPeriod { get; private set; }
			public int? BranchID { get; private set; }
			public List<DRTranKey> Trans { get; set; }

			public DRBatch(string finPeriod, int? branchID)
			{
				FinPeriod = finPeriod;
				BranchID = branchID;
				Trans = new List<DRTranKey>();
			}
		}

		[DebuggerDisplay("{ScheduleID}.{LineNbr}")]
		public struct DRTranKey
		{
			public int? ScheduleID;
			public int? ComponentID;
			public int? DetailLineNbr;
			public int? LineNbr;

			public DRTranKey(int? scheduleID, int? componentID, int? detailLineNbr, int? lineNbr)
			{
				this.ScheduleID = scheduleID;
				this.ComponentID = componentID;
				this.DetailLineNbr = detailLineNbr;
				this.LineNbr = lineNbr;
			}
		}

		#endregion
	}

	
}
