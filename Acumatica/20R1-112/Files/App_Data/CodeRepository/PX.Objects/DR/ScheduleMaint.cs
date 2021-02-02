using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using PX.Data;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.Common.Tools;
using PX.Objects.Common.Extensions;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.DR
{
    [Serializable]
	public class ScheduleMaint : PXGraph<ScheduleMaint>
	{
		public PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>> Schedule;
		public PXSelect<DRScheduleDetail> Document;
		public PXSelect<
			DRScheduleDetail, 
			Where<DRScheduleDetail.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>, 
				And<DRScheduleDetail.componentID, Equal<Current<DRScheduleDetail.componentID>>,
				And<DRScheduleDetail.detailLineNbr, Equal<Current<DRScheduleDetail.detailLineNbr>>>>>> DocumentProperties;
		public PXSelect<DRScheduleTran, 
			Where<DRScheduleTran.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>, 
			And<DRScheduleTran.componentID, Equal<Current<DRScheduleDetail.componentID>>,
			And<DRScheduleTran.detailLineNbr, Equal<Current<DRScheduleDetail.detailLineNbr>>,
			And<DRScheduleTran.lineNbr, NotEqual<Current<DRScheduleDetail.creditLineNbr>>>>>>> Transactions;
		public PXSelect<DRScheduleTran, 
			Where<DRScheduleTran.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>, 
			And<DRScheduleTran.componentID, Equal<Current<DRScheduleDetail.componentID>>,
			And<DRScheduleTran.detailLineNbr, Equal<Current<DRScheduleDetail.detailLineNbr>>,
			And<DRScheduleTran.status, Equal<DRScheduleTranStatus.OpenStatus>,
			And<DRScheduleTran.lineNbr, NotEqual<Current<DRScheduleDetail.creditLineNbr>>>>>>>> OpenTransactions;
		public PXSelect<DRScheduleTran,
			Where<DRScheduleTran.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>,
			And<DRScheduleTran.componentID, Equal<Current<DRScheduleDetail.componentID>>,
			And<DRScheduleTran.detailLineNbr, Equal<Current<DRScheduleDetail.detailLineNbr>>,
			And<DRScheduleTran.status, Equal<DRScheduleTranStatus.ProjectedStatus>,
			And<DRScheduleTran.lineNbr, NotEqual<Current<DRScheduleDetail.creditLineNbr>>>>>>>> ProjectedTransactions;
		public PXSelect<DRScheduleDetailEx> Associated;
		public PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Current<DRScheduleDetail.defCode>>>> DeferredCode;
		public PXSelect<DRExpenseBalance> ExpenseBalance;
		public PXSelect<DRExpenseProjectionAccum> ExpenseProjection;
		public PXSelect<DRRevenueBalance> RevenueBalance;
		public PXSelect<DRRevenueProjectionAccum> RevenueProjection;
		public PXSetup<DRSetup> Setup;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		public ScheduleMaint()
		{
			DRSetup setup = Setup.Current;
		}

		public virtual IEnumerable associated([PXDBInt] int? scheduleID, [PXDBString] string componentID)
		{
			if (scheduleID != null)
			{
				InventoryItem item = PXSelect<InventoryItem>.Search<InventoryItem.inventoryCD>(this, componentID);

				if (item != null)
				{
					Document.Current = PXSelect<DRScheduleDetail, Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>,
						And<DRScheduleDetail.componentID, Equal<Required<DRScheduleDetail.componentID>>>>>.Select(this, scheduleID, item.InventoryID);
				}
				else
				{
					Document.Current = PXSelect<DRScheduleDetail, Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>,
					And<DRScheduleDetail.componentID, Equal<Required<DRScheduleDetail.componentID>>>>>.Select(this, scheduleID, DRScheduleDetail.EmptyComponentID);
				}

				DRSchedule sc = PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>.Select(this);
				if (sc != null)
				{
					if (sc.Module == BatchModule.AR)
					{
						if (sc.DocType == ARDocType.CreditMemo)
						{
							ARTran arTran = PXSelect<ARTran,
								Where<ARTran.tranType, Equal<Current<DRScheduleDetailEx.docType>>,
								And<ARTran.refNbr, Equal<Current<DRScheduleDetailEx.refNbr>>,
								And<ARTran.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>.Select(this, sc.LineNbr);

							if (arTran != null)
							{
								return PXSelect<DRScheduleDetailEx, Where<DRScheduleDetailEx.componentID, Equal<Current<DRScheduleDetail.componentID>>,
									And<DRScheduleDetailEx.scheduleID, Equal<Required<DRScheduleDetailEx.scheduleID>>>>>.Select(this, arTran.DefScheduleID);
							}
						}
						else if (sc.DocType == ARDocType.Invoice || sc.DocType == ARDocType.DebitMemo)
						{
							List<DRScheduleDetailEx> list = new List<DRScheduleDetailEx>();
							foreach (ARTran arTran in PXSelect<ARTran, Where<ARTran.defScheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>.Select(this))
							{
								list.Add(PXSelectJoin<DRScheduleDetailEx,
									InnerJoin<DRSchedule, On<DRSchedule.scheduleID, Equal<DRScheduleDetailEx.scheduleID>>>,
									Where<DRScheduleDetailEx.module, Equal<BatchModule.moduleAR>,
									And<DRScheduleDetailEx.docType, Equal<Required<ARTran.tranType>>,
									And<DRScheduleDetailEx.refNbr, Equal<Required<ARTran.refNbr>>,
									And<DRScheduleDetailEx.componentID, Equal<Current<DRScheduleDetail.componentID>>,
									And<DRSchedule.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>>>.Select(this, arTran.TranType, arTran.RefNbr, arTran.LineNbr));

							}

							return list;
						}
					}
					else if (sc.Module == BatchModule.AP)
					{
						if (sc.DocType == APDocType.DebitAdj)
						{
							APTran apTran = PXSelect<APTran,
								Where<APTran.tranType, Equal<Current<DRScheduleDetail.docType>>,
								And<APTran.refNbr, Equal<Current<DRScheduleDetail.refNbr>>,
								And<APTran.lineNbr, Equal<Required<DRSchedule.lineNbr>>>>>>.Select(this, sc.LineNbr);

							if (apTran != null)
							{
								return PXSelect<DRScheduleDetailEx, Where<DRScheduleDetailEx.componentID, Equal<Current<DRScheduleDetail.componentID>>,
									And<DRScheduleDetailEx.scheduleID, Equal<Required<DRScheduleDetailEx.scheduleID>>>>>.Select(this, apTran.DefScheduleID);
							}
						}
						else if (sc.DocType == APDocType.Invoice || sc.DocType == APDocType.CreditAdj)
						{
							List<DRScheduleDetail> list = new List<DRScheduleDetail>();
							foreach (APTran apTran in PXSelect<APTran, Where<APTran.defScheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>.Select(this))
							{
								list.Add(PXSelectJoin<DRScheduleDetailEx,
									InnerJoin<DRSchedule, On<DRSchedule.scheduleID, Equal<DRScheduleDetail.scheduleID>>>,
									Where<DRScheduleDetailEx.module, Equal<BatchModule.moduleAP>,
									And<DRScheduleDetailEx.docType, Equal<Required<APTran.tranType>>,
									And<DRScheduleDetailEx.refNbr, Equal<Required<APTran.refNbr>>,
									And<DRScheduleDetailEx.componentID, Equal<Current<DRScheduleDetail.componentID>>,
									And<DRSchedule.lineNbr, Equal<Required<APTran.lineNbr>>>>>>>>.Select(this, apTran.TranType, apTran.RefNbr, apTran.LineNbr));
							}

							return list;
						}


					}
				}
			}
			return new List<DRScheduleDetail>();
		}

		#region Actions/Buttons

		public PXAction<DRScheduleDetail> cancelex;
		[PXCancelButton]
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable CancelEx(PXAdapter a)
		{
			DRScheduleDetail current = null;
			int? scheduleID = null;
			string componentCD = null;
			int? componentID = DRScheduleDetail.EmptyComponentID;

			#region Extract Keys
			if (a.Searches != null)
			{
				if (a.Searches.Length > 0 && a.Searches[0] != null)
					scheduleID = int.Parse(a.Searches[0].ToString());
				if (a.Searches.Length > 1 && a.Searches[1] != null)
					componentCD = a.Searches[1].ToString();
			}
			#endregion

			if (!string.IsNullOrEmpty(componentCD))
			{
				InventoryItem item = PXSelect<InventoryItem>.Search<InventoryItem.inventoryCD>(this, componentCD);
				if (item != null)
				{
					componentID = item.InventoryID;
				}
			}

			DRSchedule schedule = PXSelectReadonly<DRSchedule, Where<DRSchedule.scheduleID, Equal<Required<DRSchedule.scheduleID>>>>.Select(this, scheduleID);
			
			if (Document.Current == null)
			{
				foreach (DRScheduleDetail headerCanceled in Cancel.Press(a))
				{
					current = headerCanceled;
				}
			}
			else
			{
				if (schedule != null)
				{
					//existing mode:

					if (Document.Current.ScheduleID != scheduleID)
					{
						DRScheduleDetail detail2 = PXSelect<DRScheduleDetail, Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>>>.SelectWindowed(this, 0, 1, scheduleID);
						InventoryItem inv = null;
						if (detail2 != null)
						{
							inv = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, detail2.ComponentID);
						}
						if (inv == null)
							a.Searches[1] = null;
						else
							a.Searches[1] = inv.InventoryCD;


						foreach (DRScheduleDetail headerCanceled in Cancel.Press(a))
						{
							current = headerCanceled;
						}
					}
					else
					{
						InventoryItem inv = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, componentID);
						int? compID = inv == null ? DRScheduleDetail.EmptyComponentID : inv.InventoryID;

						if (Document.Current.ComponentID != compID)
						{
							DRScheduleDetail d = PXSelect<DRScheduleDetail,
								Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>,
								And<DRScheduleDetail.componentID, Equal<Required<DRScheduleDetail.componentID>>>>>.Select(this, scheduleID, compID);

							if (d != null)
							{
								current = d;
							}
							else
							{
								foreach (DRScheduleDetail headerCanceled in Cancel.Press(a))
								{
									current = headerCanceled;
								}
							}

						}
						else
						{
							foreach (DRScheduleDetail headerCanceled in Cancel.Press(a))
							{
								current = headerCanceled;
							}
						}
					}
				}
				else
				{
					//New mode:
					Document.Cache.Remove(Document.Current);
					Document.Current = null;

					DRScheduleDetail newDetail = new DRScheduleDetail();
					newDetail.ScheduleID = scheduleID;
					newDetail.ComponentID = componentID;
					current = Document.Insert(newDetail);
					Document.Cache.IsDirty = false;
				}
			}

			
			
			yield return current;
		}

		public PXSave<DRScheduleDetail> Save;
		public PXCancel<DRScheduleDetail> Cancel;
		public PXInsert<DRScheduleDetail> Insert;
		public PXDelete<DRScheduleDetail> Delete;
		public PXFirst<DRScheduleDetail> First;
		public PXPrevious<DRScheduleDetail> Prev;
		public PXNext<DRScheduleDetail> Next;
		public PXLast<DRScheduleDetail> Last;

		public PXAction<DRScheduleDetail> viewDoc;
		[PXUIField(DisplayName = Messages.ViewDocument)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewDoc(PXAdapter adapter)
		{
			if (Schedule.Current != null)
			{
				DRRedirectHelper.NavigateToOriginalDocument(this, Schedule.Current);
			}
			else if (Document.Current != null)
			{
				DRRedirectHelper.NavigateToOriginalDocument(this, Document.Current);
			}

			return adapter.Get();
		}

		public PXAction<DRScheduleDetail> viewSchedule;
		[PXUIField(DisplayName = Messages.ViewSchedule)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
		public virtual IEnumerable ViewSchedule(PXAdapter adapter)
		{
			if (Associated.Current != null)
			{
				ScheduleMaint target = PXGraph.CreateInstance<ScheduleMaint>();
				target.Clear();
				target.Document.Current = PXSelect<DRScheduleDetail,
					Where<DRScheduleDetail.scheduleID, Equal<Required<DRScheduleDetail.scheduleID>>,
					And<DRScheduleDetail.componentID, Equal<Required<DRScheduleDetail.componentID>>>>>.Select(this, Associated.Current.ScheduleID, Associated.Current.ComponentID);
				
				throw new PXRedirectRequiredException(target, "View Referenced Schedule");
			}
			return adapter.Get();
		}

		public PXAction<DRScheduleDetail> viewBatch;
		[PXUIField(DisplayName = Messages.ViewGLBatch)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			JournalEntry target = PXGraph.CreateInstance<JournalEntry>();
			target.Clear();
			Batch batch = PXSelect<Batch, Where<Batch.module, Equal<BatchModule.moduleDR>, And<Batch.batchNbr, Equal<Current<DRScheduleTran.batchNbr>>>>>.Select(this);
			if (batch != null)
			{
				target.BatchModule.Current = batch;
				throw new PXRedirectRequiredException(target, "ViewBatch");
			}

			return adapter.Get();
		} 		

		public PXAction<DRScheduleDetail> release;
		[PXUIField(DisplayName = Messages.Release, Enabled=false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			ReleaseCustomScheduleDetail();

			return adapter.Get();

		}

		public PXAction<DRScheduleDetail> genTran;
		[PXUIField(DisplayName = Messages.CreateTransactions)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable GenTran(PXAdapter adapter)
		{
			if ( Document.Current != null )
			{
				DRDeferredCode defCode = DeferredCode.Select();
				if (defCode != null)
				{
					PXResultset<DRScheduleTran> res = Transactions.Select();

					if (res.Count > 0)
					{
						WebDialogResult result = Document.View.Ask(Document.Current, GL.Messages.Confirmation, Messages.RegenerateTran, MessageButtons.YesNo, MessageIcon.Question);
						if (result == WebDialogResult.Yes)
						{
							CreateTransactions(defCode);
						}
					}
					else
					{
						CreateTransactions(defCode);
					}
				}
			}
			return adapter.Get();
		}

		#endregion

		#region Event Handlers
		protected virtual void DRScheduleDetail_ScheduleID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if ((int?)e.ReturnValue < 0) e.ReturnValue = null;
		}

		protected virtual void DRSchedule_DocDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;
			if (row != null)
			{
				e.NewValue = Accessinfo.BusinessDate;
			}
		}

		protected virtual void DRSchedule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			DRSchedule row = e.Row as DRSchedule;
			if (row != null)
			{
				if (string.IsNullOrEmpty(row.Module))
				{
					Document.Cache.RaiseExceptionHandling<DRScheduleDetail.documentType>(Document.Current, null, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, typeof(DRScheduleDetail.documentType).Name));
				}

				if (string.IsNullOrEmpty(row.FinPeriodID))
				{
					Document.Cache.RaiseExceptionHandling<DRScheduleDetail.finPeriodID>(Document.Current, null, new PXSetPropertyException(Data.ErrorMessages.FieldIsEmpty, typeof(DRScheduleDetail.finPeriodID).Name));
				}
			}
		}

		protected virtual void DRScheduleDetail_DocumentType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				string newModule = DRScheduleDocumentType.ExtractModule(row.DocumentType);
				row.DocType = DRScheduleDocumentType.ExtractDocType(row.DocumentType);

				if (row.Module != newModule)
				{
					row.Module = newModule;
					row.DefCode = null;
					row.DefAcctID = null;
					row.DefSubID = null;
					row.BAccountID = null;
					row.AccountID = null;
					row.SubID = null;
					row.RefNbr = null;

					InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<DRScheduleDetail.componentID>>>>.Select(this, row.ComponentID);
					if (item != null)
					{
						row.AccountID = row.Module == BatchModule.AP ? item.COGSAcctID : item.SalesAcctID;
						row.SubID = row.Module == BatchModule.AP ? item.COGSSubID : item.SalesSubID;

					}
				}

				
			}
		}

		protected virtual void DRScheduleDetail_TotalAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null && row.Status == DRScheduleStatus.Draft)
			{
				row.DefAmt = row.TotalAmt;
			}
		}

		protected virtual void DRScheduleDetail_DefCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRScheduleDetail.defCode>>>>.Select(this, row.DefCode);

				if (defCode != null)
				{
					row.DefCode = defCode.DeferredCodeID;
					row.DefAcctID = defCode.AccountID;
					row.DefSubID = defCode.SubID;
				}
			}
		}

		protected virtual void DRScheduleDetail_DocDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				e.NewValue = Accessinfo.BusinessDate;
			}
		}

		protected virtual void DRScheduleDetail_BAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				if (row.ComponentID == null || row.ComponentID == DRScheduleDetail.EmptyComponentID || row.AccountID == null)
				{
					switch (row.Module)
					{
						case BatchModule.AP:														
							PXResultset<Vendor> res = PXSelectJoin<Vendor,
								InnerJoin<Location, On<Vendor.bAccountID, Equal<Location.bAccountID>,
									And<Vendor.defLocationID, Equal<Location.locationID>>>>,
								Where<Vendor.bAccountID, Equal<Current<DRScheduleDetail.bAccountID>>>>.Select(this);

							Location loc = (Location) res[0][1];

							if (loc.VExpenseAcctID != null)
							{
								row.AccountID = loc.VExpenseAcctID;
								row.SubID = loc.VExpenseSubID;
							}
							break;
						case BatchModule.AR:
							PXResultset<Customer> res2 = PXSelectJoin<Customer,
								InnerJoin<Location, On<Customer.bAccountID, Equal<Customer.bAccountID>,
									And<Customer.defLocationID, Equal<Location.locationID>>>>,
								Where<Customer.bAccountID, Equal<Current<DRScheduleDetail.bAccountID>>>>.Select(this);

							Location loc2 = (Location)res2[0][1];

							if (loc2.CSalesAcctID != null)
							{
								row.AccountID = loc2.CSalesAcctID;
								row.SubID = loc2.CSalesSubID;
							}
							break;
					}
				}
			}
		}

		protected virtual void DRScheduleDetail_ScheduleID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				e.Cancel = true;
			}
		}

		protected virtual void DRScheduleDetail_RefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				if (row.Module == BatchModule.AR)
				{
					ARInvoice invoice = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Current<DRScheduleDetail.docType>>, And<ARInvoice.refNbr, Equal<Current<DRScheduleDetail.refNbr>>>>>.Select(this);
					if (invoice != null)
					{
						object oldRow = sender.CreateCopy(row);
						row.BAccountID = invoice.CustomerID;
						sender.RaiseFieldUpdated<DRScheduleDetail.bAccountID>(row, oldRow);
					}

						
				}
				else if (row.Module == BatchModule.AP)
				{
					APInvoice bill = PXSelect<APInvoice, Where<APInvoice.docType, Equal<Current<DRScheduleDetail.docType>>, And<APInvoice.refNbr, Equal<Current<DRScheduleDetail.refNbr>>>>>.Select(this);
					if (bill != null)
					{
						object oldRow = sender.CreateCopy(row);
						row.BAccountID = bill.VendorID;
						sender.RaiseFieldUpdated<DRScheduleDetail.bAccountID>(row, oldRow);
					}
						
				}
			}
		}

		protected virtual void DRScheduleDetail_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				e.Cancel = true;
			}
		}

		protected virtual void DRScheduleDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null)
			{
				row.Status = DRScheduleStatus.Draft;
				row.IsCustom = true;

				if (row.ComponentID == null)
				{
					row.ComponentID = DRScheduleDetail.EmptyComponentID;
				}


				Schedule.Cache.Clear();
				DRSchedule sc = Schedule.Insert(new DRSchedule());
				row.ScheduleID = sc.ScheduleID;

				Schedule.Cache.IsDirty = false;
				sender.Normalize();
			}

			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<DRScheduleDetail.componentID>>>>.Select(this, row.ComponentID);
			if (item != null)
			{
				row.AccountID = row.Module == BatchModule.AP ? item.COGSAcctID : item.SalesAcctID;
				row.SubID = row.Module == BatchModule.AP ? item.COGSSubID : item.SalesSubID;

				DRDeferredCode defCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRScheduleDetail.defCode>>>>.Select(this, item.DeferredCode);

				if (defCode != null)
				{
					row.DefCode = defCode.DeferredCodeID;
					row.DefAcctID = defCode.AccountID;
					row.DefSubID = defCode.SubID;
				}
			}
		}

		protected virtual void DRScheduleDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			DRScheduleDetail scheduleDetail = e.Row as DRScheduleDetail;

			if (scheduleDetail == null || !Schedule.Cache.AllowUpdate)
				return;

			DRSchedule correspondingSchedule = Schedule.Select();

			if (correspondingSchedule != null)
			{
				correspondingSchedule.Module = scheduleDetail.Module;
				correspondingSchedule.DocType = scheduleDetail.DocType;
				correspondingSchedule.RefNbr = scheduleDetail.RefNbr;
				correspondingSchedule.LineNbr = scheduleDetail.LineNbr;
				correspondingSchedule.DocDate = scheduleDetail.DocDate;
				correspondingSchedule.FinPeriodID = scheduleDetail.TranPeriodID;

				Schedule.Update(correspondingSchedule);
			}
		}
		
		protected virtual void DRSchedule_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			DRSchedule schedule = e.Row as DRSchedule;
			if (schedule == null) return;

			sender.AllowDelete = schedule.IsDraft == true;

			release.SetVisible(schedule.IsCustom == true);
			release.SetEnabled(schedule.IsDraft == true);
		}

		protected virtual void DRScheduleDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;

			if (row != null)
			{
				row.DocumentType = DRScheduleDocumentType.BuildDocumentType(row.Module, row.DocType);

				row.DefTotal = SumOpenAndProjectedTransactions();
				PXUIFieldAttribute.SetEnabled<DRScheduleDetail.componentID>(sender, row, row.ComponentID != DRScheduleDetail.EmptyComponentID);
				if (row.Status == DRScheduleStatus.Draft)
				{
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.finPeriodID>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.defAcctID>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.defSubID>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.totalAmt>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.documentType>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.refNbr>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.lineNbr>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.accountID>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.subID>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.bAccountID>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.defCode>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.docDate>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<DRScheduleDetail.componentID>(sender, row, true);
				}

				PXDefaultAttribute.SetPersistingCheck<DRScheduleDetail.defCode>(sender, row, row.IsResidual == true ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
			}
		}

		protected virtual void DRScheduleDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			DRScheduleDetail row = e.Row as DRScheduleDetail;
			if (row != null && row.Status == DRScheduleStatus.Draft)
			{
				DRSchedule sc = Schedule.Select();
				Schedule.Delete(sc);
			}
		}


		protected virtual void DRScheduleTran_RecDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (row != null)
			{
				row.FinPeriodID = FinPeriodRepository.GetPeriodIDFromDate(row.RecDate, FinPeriod.organizationID.MasterValue);
			}
		}

		protected virtual void DRScheduleTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (row != null && Document.Current != null && Document.Current.Status != DRScheduleStatus.Draft)
			{
				DRDeferredCode dc = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Current<DRScheduleDetail.defCode>>>>.Select(this);
				bool checkTotal = true;

				if (SkipTotalCheck(dc))
				{
					checkTotal = false;
				}
				
				if ( checkTotal )
				{
					decimal defTotal = SumOpenAndProjectedTransactions();
					if (defTotal != Document.Current.DefAmt)
					{
						if (sender.RaiseExceptionHandling<DRScheduleDetail.defTotal>(e.Row, defTotal, new PXSetPropertyException(Messages.DeferredAmountSumError)))
						{
							throw new PXRowPersistingException(typeof(DRScheduleDetail.defTotal).Name, defTotal, Messages.DeferredAmountSumError);
						}
					}
				}
			}

			
		}
	    public virtual bool SkipTotalCheck(DRDeferredCode dc)
	    {
	        return dc != null && dc.Method == DeferredMethodType.CashReceipt;
	    }
        protected virtual void DRScheduleTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.recDate>(sender, row, row.Status == DRScheduleTranStatus.Open || row.Status == DRScheduleTranStatus.Projected);
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.amount>(sender, row, row.Status == DRScheduleTranStatus.Open || row.Status == DRScheduleTranStatus.Projected);
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.accountID>(sender, row, row.Status == DRScheduleTranStatus.Open || row.Status == DRScheduleTranStatus.Projected);
				PXUIFieldAttribute.SetEnabled<DRScheduleTran.subID>(sender, row, row.Status == DRScheduleTranStatus.Open || row.Status == DRScheduleTranStatus.Projected);
			}
		}

		protected virtual void DRScheduleTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (!sender.ObjectsEqual<DRScheduleTran.finPeriodID, DRScheduleTran.accountID, DRScheduleTran.subID, DRScheduleTran.amount>(e.Row, e.OldRow))
			{
				if (Document.Current != null && Document.Current.Status == DRScheduleStatus.Open)
				{
					DRScheduleTran oldRow = (DRScheduleTran)e.OldRow;
					Subtract(oldRow);
					Add(row);
				}
			}
		}

		protected virtual void DRScheduleTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (row != null && Document.Current != null && Document.Current.Status == DRScheduleStatus.Open)
			{
				Add(row);
			}
		}

		protected virtual void DRScheduleTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			DRScheduleTran row = e.Row as DRScheduleTran;
			if (row != null && Document.Current != null && Document.Current.Status == DRScheduleStatus.Open)
			{
				Subtract(row);
			}
		}
		#endregion

		private void CreateTransactions(DRDeferredCode defCode)
		{
			if (Document.Current != null && Document.Current.Status == DRScheduleStatus.Draft)
			{
				foreach (DRScheduleTran tran in Transactions.Select())
				{
					Transactions.Delete(tran);
				}

				if (Schedule.Current != null && Document.Current != null && defCode != null)
				{
					IList<DRScheduleTran> tranList = GetTransactionsGenerator(defCode).GenerateTransactions(Schedule.Current, Document.Current);

					foreach (DRScheduleTran tran in tranList)
					{
						Transactions.Insert(tran);
					}
				}
			}
		}

		public static void ReleaseCustomSchedules(IEnumerable<DRSchedule> schedules)
		{
			ScheduleMaint maint = PXGraph.CreateInstance<ScheduleMaint>();

			foreach (var schedule in schedules)
			{
				maint.Clear();

				maint.ReleaseCustomSchedule(schedule);
			}
		}

		public virtual void ReleaseCustomSchedule(DRSchedule schedule)
		{
			using (var ts = new PXTransactionScope())
			{
				var details = PXSelect<
					DRScheduleDetail, 
					Where<DRScheduleDetail.scheduleID, Equal<Required<DRSchedule.scheduleID>>, 
						And<DRScheduleDetail.isResidual, Equal<False>>>>
					.Select(this, schedule.ScheduleID)
					.RowCast<DRScheduleDetail>();

				FinPeriodUtils.ValidateFinPeriod<DRScheduleDetail>(details, m => schedule.FinPeriodID, m => m.BranchID.SingleToArray());

				foreach (var detail in details)
				{
					Clear();
					Document.Current = detail;

					ReleaseCustomScheduleDetail();
				}

				ts.Complete();
			}
		}

		internal void ReleaseCustomScheduleDetail()
		{
			if (Document.Current != null && Document.Current.Status == DRScheduleStatus.Draft)
			{
				CreateIncomingTransaction();
				UpdateHistory();

				Document.Current.Status = DRScheduleStatus.Open;
				Document.Current.IsOpen = true;
				Document.Update(Document.Current);

				Schedule.Current.IsDraft = false;
				Schedule.Update(Schedule.Current);
				
				this.Save.Press();
			}
		}

		private void CreateIncomingTransaction()
		{
			DRScheduleTran tran = new DRScheduleTran();
			tran.BranchID = Document.Current.BranchID;
			tran.AccountID = Document.Current.AccountID;
			tran.SubID = Document.Current.SubID;
			tran.Amount = Document.Current.TotalAmt;
			tran.RecDate = this.Accessinfo.BusinessDate;
			tran.TranDate = this.Accessinfo.BusinessDate;
			tran.FinPeriodID = Document.Current.FinPeriodID;
			tran.LineNbr = 0;
			tran.ScheduleID = Document.Current.ScheduleID;
			tran.ComponentID = Document.Current.ComponentID;
			tran.Status = DRScheduleTranStatus.Posted;

			tran = Transactions.Insert(tran);

			if ( Document.Current.Module == BatchModule.AR )
				InitBalance(tran, Document.Current, DeferredAccountType.Income);
			else
				InitBalance(tran, Document.Current, DeferredAccountType.Expense);
		}

		private void UpdateHistory()
		{
			foreach (DRScheduleTran tran in OpenTransactions.Select())
			{
				if (Document.Current.Module == BatchModule.AR)
					UpdateBalanceProjection(tran, Document.Current, DeferredAccountType.Income);
				else
					UpdateBalanceProjection(tran, Document.Current, DeferredAccountType.Expense);
			}
		}

		public virtual void RebuildProjections()
		{
			List<DRScheduleTran> existingTrans = GetExistingTrans(Document.Current);

			decimal balance = Document.Current.TotalAmt.Value;
			List<string> existingPeriods = new List<string>();

			foreach (DRScheduleTran tran in existingTrans)
			{
				if (!existingPeriods.Contains(tran.FinPeriodID))
				{
					existingPeriods.Add(tran.FinPeriodID);
				}

				balance -= Math.Min(tran.Amount.GetValueOrDefault(0), balance);
			}

			DRSchedule sc = PXSelect<DRSchedule, Where<DRSchedule.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>.Select(this);
			DRDeferredCode defCode = DeferredCode.Select();

			IList<DRScheduleTran> tranList = GetTransactionsGenerator(defCode).GenerateTransactions(sc, Document.Current);

			if (existingTrans.Count > 0)
			{
				int? organizationID = PXAccess.GetParentOrganizationID(Document.Current.BranchID);
				// Remove projected transactions that is either already exist or will not be payed:
				//DateTime firstPaymentPeriodStartDate = FinPeriodIDAttribute.PeriodStartDate(this, existingPeriods[0]);
				DateTime lastPaymentPeriodStartDate = FinPeriodRepository.PeriodStartDate(existingPeriods[existingPeriods.Count - 1], organizationID);

				List<DRScheduleTran> delete = new List<DRScheduleTran>();
				for (int i = 0; i < tranList.Count - 1; i++)
				{
					DateTime periodStartDate = FinPeriodRepository.PeriodStartDate(tranList[i].FinPeriodID, organizationID);
					if (periodStartDate <= lastPaymentPeriodStartDate ||
						 existingPeriods.Contains(tranList[i].FinPeriodID)
						)
					{
						delete.Add(tranList[i]);
					}
				}

				//remove last only if DefAmt is 0.
				if (Document.Current.DefAmt == 0)
				{
					delete.Add(tranList[tranList.Count - 1]);
				}

				foreach (DRScheduleTran tran in delete)
				{
					tranList.Remove(tran);
				}

				if (tranList.Count > 0)
				{
					decimal partRaw = balance / tranList.Count;
					decimal part = PXCurrencyAttribute.BaseRound(this, partRaw);

					for (int i = 0; i < tranList.Count - 1; i++)
					{
						tranList[i].Amount = part;
						balance -= part;
					}

					tranList[tranList.Count - 1].Amount = PXCurrencyAttribute.BaseRound(this, balance);
				}
			}

			if (tranList.Count > 0)
			{
				//Remove all projected lines and add the following:

				foreach (DRScheduleTran tran in ProjectedTransactions.Select())
				{
					ProjectedTransactions.Delete(tran);
				}

				foreach (DRScheduleTran tran in tranList)
				{
					ProjectedTransactions.Insert(tran);
					//Debug.Print("Line Nbr={0} FinPeriod={1} RecDate={2} Amount={3}", tran.LineNbr, tran.FinPeriodID, tran.RecDate, tran.Amount);
				}
			}

			
		}
				
		private List<DRScheduleTran> GetExistingTrans(DRScheduleDetail sd)
		{
			PXResultset<DRScheduleTran> existingTrans = PXSelect<
				DRScheduleTran,
				Where<DRScheduleTran.scheduleID, Equal<Required<DRScheduleTran.scheduleID>>,
					And<DRScheduleTran.componentID, Equal<Required<DRScheduleTran.componentID>>,
					And<DRScheduleTran.detailLineNbr, Equal<Required<DRScheduleTran.detailLineNbr>>,
					And<DRScheduleTran.lineNbr, NotEqual<Required<DRScheduleTran.lineNbr>>,
					And<DRScheduleTran.status, NotEqual<DRScheduleTranStatus.ProjectedStatus>>>>>>,
				OrderBy<
					Asc<DRScheduleTran.finPeriodID>>>
				.Select(this, sd.ScheduleID, sd.ComponentID, sd.DetailLineNbr, sd.CreditLineNbr);

			List<DRScheduleTran> list = new List<DRScheduleTran>(existingTrans.Count);

			foreach (DRScheduleTran tran in existingTrans)
				list.Add(tran);

			return list;
		}


		private void Subtract(DRScheduleTran tran)
		{
			Debug.Print("Subtract FinPeriod={0} Status={1} Amount={2}", tran.FinPeriodID, tran.Status, tran.Amount);
			DRDeferredCode code = DeferredCode.Select();

			if (code.AccountType == DeferredAccountType.Expense)
			{
				SubtractExpenseFromProjection(tran);
				SubtractExpenseFromBalance(tran);
			}
			else
			{
				SubtractRevenueFromProjection(tran);
				SubtractRevenueFromBalance(tran);
			}
		}

		private void SubtractRevenueFromProjection(DRScheduleTran tran)
		{
			DRRevenueProjectionAccum finHist = CreateDRRevenueProjectionAccum(Document.Current, tran.FinPeriodID);
			DRRevenueProjectionAccum tranHist = CreateDRRevenueProjectionAccum(Document.Current, tran.TranPeriodID);

			finHist.PTDProjected -= tran.Amount;
			tranHist.TranPTDProjected -= tran.Amount;
		}

		private void SubtractExpenseFromProjection(DRScheduleTran tran)
		{
			DRExpenseProjectionAccum finHist = CreateDRExpenseProjectionAccum(Document.Current, tran.FinPeriodID);
			DRExpenseProjectionAccum tranHist = CreateDRExpenseProjectionAccum(Document.Current, tran.TranPeriodID);

			finHist.PTDProjected -= tran.Amount;
			tranHist.TranPTDProjected -= tran.Amount;
		}

		private void SubtractRevenueFromBalance(DRScheduleTran tran)
		{
			DRRevenueBalance finHist = CreateDRRevenueBalance(Document.Current, tran.FinPeriodID);
			DRRevenueBalance tranHist = CreateDRRevenueBalance(Document.Current, tran.TranPeriodID);

			finHist.PTDProjected -= tran.Amount;
			finHist.EndProjected += tran.Amount;

			tranHist.TranPTDProjected -= tran.Amount;
			tranHist.TranEndProjected += tran.Amount;
		}

		private void SubtractExpenseFromBalance(DRScheduleTran tran)
		{
			DRExpenseBalance finHist = CreateDRExpenseBalance(Document.Current, tran.FinPeriodID);
			DRExpenseBalance tranHist = CreateDRExpenseBalance(Document.Current, tran.TranPeriodID);

			finHist.PTDProjected -= tran.Amount;
			finHist.EndProjected += tran.Amount;

			tranHist.PTDProjected -= tran.Amount;
			tranHist.EndProjected += tran.Amount;
		}

		private void Add(DRScheduleTran tran)
		{
			Debug.Print("Add FinPeriod={0} Status={1} Amount={2}", tran.FinPeriodID, tran.Status, tran.Amount);
			DRDeferredCode code = DeferredCode.Select();

			if (code.AccountType == DeferredAccountType.Expense)
			{
				AddExpenseToProjection(tran);
				AddExpenseToBalance(tran);
			}
			else
			{
				AddRevenueToProjection(tran);
				AddRevenueToBalance(tran);
			}
		}

		private void AddRevenueToProjection(DRScheduleTran tran)
		{
			DRRevenueProjectionAccum finHist = CreateDRRevenueProjectionAccum(Document.Current, tran.FinPeriodID);
			DRRevenueProjectionAccum tranHist = CreateDRRevenueProjectionAccum(Document.Current, tran.TranPeriodID);

			finHist.PTDProjected += tran.Amount;
			tranHist.TranPTDProjected += tran.Amount;
		}

		private void AddExpenseToProjection(DRScheduleTran tran)
		{
			DRExpenseProjectionAccum finHist = CreateDRExpenseProjectionAccum(Document.Current, tran.FinPeriodID);
			DRExpenseProjectionAccum tranHist = CreateDRExpenseProjectionAccum(Document.Current, tran.TranPeriodID);

			finHist.PTDProjected += tran.Amount;
			tranHist.TranPTDProjected += tran.Amount;
		}

		private void AddRevenueToBalance(DRScheduleTran tran)
		{
			DRRevenueBalance finHist = CreateDRRevenueBalance(Document.Current, tran.FinPeriodID);
			DRRevenueBalance tranHist = CreateDRRevenueBalance(Document.Current, tran.TranPeriodID);

			finHist.PTDProjected += tran.Amount;
			finHist.EndProjected -= tran.Amount;

			tranHist.TranPTDProjected += tran.Amount;
			tranHist.TranEndProjected -= tran.Amount;
		}

		private void AddExpenseToBalance(DRScheduleTran tran)
		{
			DRExpenseBalance finHist = CreateDRExpenseBalance(Document.Current, tran.FinPeriodID);
			DRExpenseBalance tranHist = CreateDRExpenseBalance(Document.Current, tran.TranPeriodID);

			finHist.PTDProjected += tran.Amount;
			finHist.EndProjected -= tran.Amount;

			tranHist.TranPTDProjected += tran.Amount;
			tranHist.TranEndProjected -= tran.Amount;
		}
		
		private decimal SumOpenAndProjectedTransactions()
		{
			decimal total = 0;
			foreach (DRScheduleTran tran in OpenTransactions.Select())
			{
				total += tran.Amount.Value;
			}

			foreach (DRScheduleTran tran in ProjectedTransactions.Select())
			{
				total += tran.Amount.Value;
			}

			return total;
		}


		private void InitBalance(DRScheduleTran tran, DRScheduleDetail sd, string deferredAccountType)
		{
			switch (deferredAccountType)
			{
				case DeferredAccountType.Expense:
					InitExpenseBalance(tran, sd);
					break;
				case DeferredAccountType.Income:
					InitRevenueBalance(tran, sd);
					break;
				default:
					throw new PXException(Messages.InvalidAccountType, DeferredAccountType.Expense, DeferredAccountType.Income, deferredAccountType);
			}
		}

		private void InitRevenueBalance(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRRevenueBalance finHist = CreateDRRevenueBalance(sd, tran.FinPeriodID);
			DRRevenueBalance tranHist = CreateDRRevenueBalance(sd, tran.TranPeriodID);

			if (IsReversed(sd))
			{
				finHist.PTDDeferred -= tran.Amount;
				finHist.EndBalance -= tran.Amount;
				finHist.EndProjected -= tran.Amount;

				tranHist.TranPTDDeferred -= tran.Amount;
				tranHist.TranEndBalance -= tran.Amount;
				tranHist.TranEndProjected -= tran.Amount;
			}
			else
			{
				finHist.PTDDeferred += tran.Amount;
				finHist.EndBalance += tran.Amount;
				finHist.EndProjected += tran.Amount;

				tranHist.TranPTDDeferred += tran.Amount;
				tranHist.TranEndBalance += tran.Amount;
				tranHist.TranEndProjected += tran.Amount;
			}
		}

		private void InitExpenseBalance(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRExpenseBalance finHist = CreateDRExpenseBalance(sd, tran.FinPeriodID);
			DRExpenseBalance tranHist = CreateDRExpenseBalance(sd, tran.TranPeriodID);

			if (IsReversed(sd))
			{
				finHist.PTDDeferred -= tran.Amount;
				finHist.EndBalance -= tran.Amount;
				finHist.EndProjected -= tran.Amount;

				tranHist.TranPTDDeferred -= tran.Amount;
				tranHist.TranEndBalance -= tran.Amount;
				tranHist.TranEndProjected -= tran.Amount;
			}
			else
			{
				finHist.PTDDeferred += tran.Amount;
				finHist.EndBalance += tran.Amount;
				finHist.EndProjected += tran.Amount;

				tranHist.TranPTDDeferred += tran.Amount;
				tranHist.TranEndBalance += tran.Amount;
				tranHist.TranEndProjected += tran.Amount;
			}
		}

		private static bool IsReversed(DRScheduleDetail sd)
		{
			return sd.DocType == ARDocType.CreditMemo || sd.DocType == APDocType.DebitAdj;
		}


		private void UpdateBalanceProjection(DRScheduleTran tran, DRScheduleDetail sd, string deferredAccountType)
		{
			switch (deferredAccountType)
			{
				case DeferredAccountType.Expense:
					UpdateExpenseBalanceProjection(tran, sd);
					UpdateExpenseProjection(tran, sd);
					break;
				case DeferredAccountType.Income:
					UpdateRevenueBalanceProjection(tran, sd);
					UpdateRevenueProjection(tran, sd);
					break;

				default:
					throw new PXException(Messages.InvalidAccountType, DeferredAccountType.Expense, DeferredAccountType.Income, deferredAccountType);
			}
		}

		private void UpdateRevenueBalanceProjection(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRRevenueBalance finHist = CreateDRRevenueBalance(sd, tran.FinPeriodID);
			DRRevenueBalance tranHist = CreateDRRevenueBalance(sd, tran.TranPeriodID);

			if (IsReversed(sd))
			{
				finHist.PTDProjected -= tran.Amount;
				finHist.EndProjected += tran.Amount;

				tranHist.TranPTDProjected -= tran.Amount;
				tranHist.TranEndProjected += tran.Amount;
			}
			else
			{
				finHist.PTDProjected += tran.Amount;
				finHist.EndProjected -= tran.Amount;

				tranHist.TranPTDProjected += tran.Amount;
				tranHist.TranEndProjected -= tran.Amount;
			}
		}

		private void UpdateExpenseBalanceProjection(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRExpenseBalance finHist = CreateDRExpenseBalance(sd, tran.FinPeriodID);
			DRExpenseBalance tranHist = CreateDRExpenseBalance(sd, tran.TranPeriodID);

			if (IsReversed(sd))
			{
				finHist.PTDProjected -= tran.Amount;
				finHist.EndProjected += tran.Amount;

				tranHist.TranPTDProjected -= tran.Amount;
				tranHist.TranEndProjected += tran.Amount;
			}
			else
			{
				finHist.PTDProjected += tran.Amount;
				finHist.EndProjected -= tran.Amount;

				tranHist.TranPTDProjected += tran.Amount;
				tranHist.TranEndProjected -= tran.Amount;
			}
		}

		private void UpdateRevenueProjection(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRRevenueProjectionAccum finHist = CreateDRRevenueProjectionAccum(sd, tran.FinPeriodID);
			DRRevenueProjectionAccum tranHist = CreateDRRevenueProjectionAccum(sd, tran.TranPeriodID);

			if (IsReversed(sd))
			{
				finHist.PTDProjected -= tran.Amount;
				tranHist.TranPTDProjected -= tran.Amount;
			}
			else
			{
				finHist.PTDProjected += tran.Amount;
				tranHist.TranPTDProjected += tran.Amount;
			}
		}

		private void UpdateExpenseProjection(DRScheduleTran tran, DRScheduleDetail sd)
		{
			DRExpenseProjectionAccum finHist = CreateDRExpenseProjectionAccum(sd, tran.FinPeriodID);
			DRExpenseProjectionAccum tranHist = CreateDRExpenseProjectionAccum(sd, tran.TranPeriodID);

			if (IsReversed(sd))
			{
				finHist.PTDProjected -= tran.Amount;
				tranHist.TranPTDProjected -= tran.Amount;
			}
			else
			{
				finHist.PTDProjected += tran.Amount;
				tranHist.TranPTDProjected += tran.Amount;
			}
		}

		private DRExpenseBalance CreateDRExpenseBalance(DRScheduleDetail scheduleDetail, string periodID)
		{
			DRExpenseBalance hist = new DRExpenseBalance();
			hist.FinPeriodID = periodID;
			hist.BranchID = scheduleDetail.BranchID;
			hist.AcctID = scheduleDetail.DefAcctID;
			hist.SubID = scheduleDetail.DefSubID;
			hist.ComponentID = scheduleDetail.ComponentID ?? 0;
			hist.ProjectID = scheduleDetail.ProjectID ?? 0;
			hist.VendorID = scheduleDetail.BAccountID ?? 0;

			return ExpenseBalance.Insert(hist);
		}

		private DRRevenueBalance CreateDRRevenueBalance(DRScheduleDetail scheduleDetail, string periodID)
		{
			DRRevenueBalance hist = new DRRevenueBalance();
			hist.FinPeriodID = periodID;
			hist.BranchID = scheduleDetail.BranchID;
			hist.AcctID = scheduleDetail.DefAcctID;
			hist.SubID = scheduleDetail.DefSubID;
			hist.ComponentID = scheduleDetail.ComponentID ?? 0;
			hist.ProjectID = scheduleDetail.ProjectID ?? 0;
			hist.CustomerID = scheduleDetail.BAccountID ?? 0;

			return RevenueBalance.Insert(hist);
		}

		private DRExpenseProjectionAccum CreateDRExpenseProjectionAccum(DRScheduleDetail scheduleDetail, string periodID)
		{
			DRExpenseProjectionAccum hist = new DRExpenseProjectionAccum();
			hist.FinPeriodID = periodID;
			hist.BranchID = scheduleDetail.BranchID;
			hist.AcctID = scheduleDetail.AccountID;
			hist.SubID = scheduleDetail.SubID;
			hist.ComponentID = scheduleDetail.ComponentID ?? 0;
			hist.ProjectID = scheduleDetail.ProjectID ?? 0;
			hist.VendorID = scheduleDetail.BAccountID ?? 0;

			return ExpenseProjection.Insert(hist);
		}

		private DRRevenueProjectionAccum CreateDRRevenueProjectionAccum(DRScheduleDetail scheduleDetail, string periodID)
		{
			DRRevenueProjectionAccum hist = new DRRevenueProjectionAccum();
			hist.FinPeriodID = periodID;
			hist.BranchID = scheduleDetail.BranchID;
			hist.AcctID = scheduleDetail.AccountID;
			hist.SubID = scheduleDetail.SubID;
			hist.ComponentID = scheduleDetail.ComponentID ?? 0;
			hist.ProjectID = scheduleDetail.ProjectID ?? 0;
			hist.CustomerID = scheduleDetail.BAccountID ?? 0;

			return RevenueProjection.Insert(hist);
		}

		[Serializable]
		public partial class DRScheduleDetailEx : DRScheduleDetail
		{
			#region ScheduleID
			public new abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }
			[PXDBInt(IsKey = true)]
			[PXUIField(DisplayName = "Schedule ID")]
			public override Int32? ScheduleID
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
			public new abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }
			
			[PXDBInt(IsKey = true)]
			[PXUIField(DisplayName = "Component", Visibility = PXUIVisibility.Visible)]
			[PXSelector(typeof(Search2<DRScheduleDetail.componentID, LeftJoin<InventoryItem, On<DRScheduleDetail.componentID, Equal<InventoryItem.inventoryID>>>, Where<DRScheduleDetail.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>), new Type[] { typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr) })]
			public override Int32? ComponentID
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
		}

		#region Factory Methods

		protected virtual TransactionsGenerator GetTransactionsGenerator(DRDeferredCode deferralCode)
			=> new TransactionsGenerator(this, deferralCode);

		#endregion
	}
}
