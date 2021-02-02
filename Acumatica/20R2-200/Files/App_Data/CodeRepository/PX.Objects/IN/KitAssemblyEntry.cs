using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common.Attributes;
using System.Diagnostics;
using PX.SM;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common.Bql;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN.Overrides.INDocumentRelease;
using PX.Objects.Common;
using PX.Objects.Common.Exceptions;

namespace PX.Objects.IN
{
	public class KitAssemblyEntry : PXGraph<KitAssemblyEntry, INKitRegister>
	{
		#region Selects/Views
		public PXSelect<INTran> dummy_intran;
		public PXSelect<INKitRegister, Where<INKitRegister.docType, Equal<Optional<INKitRegister.docType>>>> Document;
		public PXSelect<INKitRegister, Where<INKitRegister.docType, Equal<Current<INKitRegister.docType>>,
			And<INKitRegister.refNbr, Equal<Current<INKitRegister.refNbr>>>>> DocumentProperties;
		public PXSelectJoin<INComponentTran,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INComponentTran.inventoryID>>,
				LeftJoin<INKitSpecStkDet, On<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitRegister.kitInventoryID>>,
				And2<Where<INKitSpecStkDet.compSubItemID, Equal<INComponentTran.subItemID>, Or<Where<INKitSpecStkDet.compSubItemID, IsNull, And<INComponentTran.subItemID, IsNull>>>>,
				And<INKitSpecStkDet.revisionID, Equal<Current<INKitRegister.kitRevisionID>>,
				And<INKitSpecStkDet.compInventoryID, Equal<INComponentTran.inventoryID>>>>>>>,
			Where<INComponentTran.docType, Equal<Current<INKitRegister.docType>>,
			And<INComponentTran.refNbr, Equal<Current<INKitRegister.refNbr>>,
			And<InventoryItem.stkItem, Equal<boolTrue>,
			And<INComponentTran.lineNbr, NotEqual<Current<INKitRegister.kitLineNbr>>>>>>> Components;		
		public PXSelectJoin<INOverheadTran,
			InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INOverheadTran.inventoryID>>,
				LeftJoin<INKitSpecNonStkDet, On<INKitSpecNonStkDet.kitInventoryID, Equal<Current<INKitRegister.kitInventoryID>>,
				And<INKitSpecNonStkDet.revisionID, Equal<Current<INKitRegister.kitRevisionID>>,
				And<INKitSpecNonStkDet.compInventoryID, Equal<INOverheadTran.inventoryID>>>>>>,
			Where<INOverheadTran.docType, Equal<Current<INKitRegister.docType>>,
			And<INOverheadTran.refNbr, Equal<Current<INKitRegister.refNbr>>,
			And<InventoryItem.stkItem, Equal<False>>>>> Overhead;
		public PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Current<INKitRegister.kitInventoryID>>,
			And<INKitSpecHdr.revisionID, Equal<Current<INKitRegister.kitRevisionID>>>>> Spec;
		public PXSelectJoin<INKitSpecStkDet,
			InnerJoin<InventoryItem, On<INKitSpecStkDet.FK.CompInventoryItem>>,
			Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitRegister.kitInventoryID>>,
			And<INKitSpecStkDet.revisionID, Equal<Current<INKitRegister.kitRevisionID>>>>> SpecComponents;
		public PXSelectJoin<INKitSpecNonStkDet,
			InnerJoin<InventoryItem, On<INKitSpecNonStkDet.FK.CompInventoryItem>>,
			Where<INKitSpecNonStkDet.kitInventoryID, Equal<Current<INKitRegister.kitInventoryID>>,
			And<INKitSpecNonStkDet.revisionID, Equal<Current<INKitRegister.kitRevisionID>>>>> SpecOverhead;

		[PXCopyPasteHiddenView()]
		public PXSelect<INComponentTranSplit, 
			Where<INComponentTranSplit.docType, Equal<Current<INComponentTran.docType>>, 
			And<INComponentTranSplit.refNbr, Equal<Current<INComponentTran.refNbr>>, 
			And<INComponentTranSplit.lineNbr, Equal<Current<INComponentTran.lineNbr>>>>>> ComponentSplits;

		[PXCopyPasteHiddenView()]
		public PXSelect<INKitTranSplit,
			Where<INKitTranSplit.docType, Equal<Current<INKitRegister.docType>>,
			And<INKitTranSplit.refNbr, Equal<Current<INKitRegister.refNbr>>,
			And<INKitTranSplit.lineNbr, Equal<Current<INKitRegister.kitLineNbr>>>>>> MasterSplits;
		public PXSelect<INKitSerialPart, Where<INKitSerialPart.docType, Equal<Current<INKitRegister.docType>>,
			And<INKitSerialPart.refNbr, Equal<Current<INKitRegister.refNbr>>>>> SerialNumberedParts;
		
		public PXSetup<INSetup> Setup;
		public LSINComponentTran lsselect;
		public LSINComponentMasterTran lsselect2;
		#endregion

		public KitAssemblyEntry()
		{
			Spec.Cache.AllowInsert = false;
			Spec.Cache.AllowDelete = false;
			Spec.Cache.AllowUpdate = false;
			SpecComponents.Cache.AllowInsert = false;
			SpecComponents.Cache.AllowDelete = false;
			SpecComponents.Cache.AllowUpdate = false;
			SpecOverhead.Cache.AllowInsert = false;
			SpecOverhead.Cache.AllowDelete = false;
			SpecOverhead.Cache.AllowUpdate = false;

			this.Views["LSINComponentMasterTran_lotseropts"].AllowSelect = 
				PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>()
				&& (
					PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>()
					|| PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>()
					|| PXAccess.FeatureInstalled<FeaturesSet.subItem>()
					|| PXAccess.FeatureInstalled<FeaturesSet.replenishment>()
					|| PXAccess.FeatureInstalled<FeaturesSet.sOToPOLink>()
				)
				;

			OpenPeriodAttribute.SetValidatePeriod<INKitRegister.finPeriodID>(Document.Cache, null, PeriodValidation.DefaultSelectUpdate);
		}
		#region Cache Attach
		#region INKitSerialPart
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(typeof(INKitRegister.docType))]
		protected virtual void INKitSerialPart_DocType_CacheAttached(PXCache sender)
		{			
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(INKitRegister.refNbr))]
		[PXParent(typeof(Select<INKitRegister, Where<INKitRegister.docType, Equal<Current<INKitSerialPart.docType>>, And<INKitRegister.refNbr, Equal<Current<INKitSerialPart.refNbr>>>>>))]
		protected virtual void INKitSerialPart_RefNbr_CacheAttached(PXCache sender)
		{
		}		
		#endregion
        #endregion

        #region Actions/Buttons
        public PXAction<INKitRegister> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			List<INKitRegister> ret = new List<INKitRegister>();
			foreach (INKitRegister inKitDoc in adapter.Get<INKitRegister>())
			{
				if (inKitDoc.Hold == false && inKitDoc.Released == false)
				{
					Document.Cache.Update(inKitDoc);
					ret.Add(inKitDoc);
				}
			}
			if (ret.Count == 0)
			{
				throw new PXException(Messages.Document_Status_Invalid);
			}
			
			Save.Press();

			List<INRegister> list = new List<INRegister>();
			foreach (INKitRegister kitRegister in ret)
			{
				INRegister doc = PXSelect<INRegister, Where<INRegister.docType, Equal<Required<INRegister.docType>>, And<INRegister.refNbr, Equal<Required<INRegister.refNbr>>>>>.Select(this, kitRegister.DocType, kitRegister.RefNbr);
				list.Add(doc);
			}

			PXLongOperation.StartOperation(this, delegate() { INDocumentRelease.ReleaseDoc(list, false); });
			return ret;
		} 

		public PXAction<INKitRegister> viewBatch;
		[PXUIField(DisplayName = "Review Batch", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewBatch(PXAdapter adapter)
		{
			if (Document.Current != null && !String.IsNullOrEmpty(Document.Current.BatchNbr))
			{
				GL.JournalEntry graph = PXGraph.CreateInstance<GL.JournalEntry>();
				graph.BatchModule.Current = graph.BatchModule.Search<GL.Batch.batchNbr>(Document.Current.BatchNbr, "IN");
				throw new PXRedirectRequiredException(graph, "Current batch record");
			}
			return adapter.Get();
		}
		#endregion

		#region Event Handlers

		#region INKitRegister

		protected virtual void INKitRegister_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INKitRegister row = e.Row as INKitRegister;
			if (row != null)
			{
				if (row.DocType == INDocType.Disassembly)
				{
					row.InvtMult = -1;
				}
				else
				{
					row.InvtMult = 1;
				}
			}
		}

		protected virtual void INKitRegister_KitInventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INKitRegister row = e.Row as INKitRegister;
			if (row != null)
			{
				PXResultset<INKitSpecHdr> resultset = PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Current<INKitRegister.kitInventoryID>>>>.SelectWindowed(this, 0, 2);
				if (resultset.Count == 1)
				{
					row.KitRevisionID = ((INKitSpecHdr)resultset).RevisionID;
					row.SubItemID = ((INKitSpecHdr)resultset).KitSubItemID;
				}
				else
				{
					row.KitRevisionID = null;
				}

				sender.SetDefaultExt<INKitRegister.uOM>(e.Row);
			}
		}

		protected virtual void INKitRegister_KitRevisionID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			INKitRegister row = e.Row as INKitRegister;
			if (row != null)
			{
				PXResultset<INKitSpecHdr> resultset = PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Current<INKitRegister.kitInventoryID>>>>.SelectWindowed(this, 0, 2);
				if (resultset != null)
				{
					row.SubItemID = ((INKitSpecHdr)resultset).KitSubItemID;
				}
			}
		}

		protected virtual void INKitRegister_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			((INKitRegister)e.Row).LineCntr = (short)1;
		}

		protected bool _InternalCall = false;

		protected virtual void INComponentTran_LineNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_InternalCall == false)
			{
				_InternalCall = true;
				object newval;
				try
				{
					sender.RaiseFieldDefaulting<INComponentTran.lineNbr>(e.Row, out newval);
				}
				finally
				{
					_InternalCall = false;
				}

				foreach (INOverheadTran other in Overhead.Cache.Deleted)
				{
					if (other.LineNbr == (int)newval)
					{
						newval = (short)newval + 1;
					}
				}

				foreach (INOverheadTran other in Overhead.Cache.Updated)
				{
					if (other.LineNbr == (short)newval)
					{
						newval = (short)newval + 1;
					}
				}
				e.NewValue = newval;
				e.Cancel = true;
			}
		}

		protected virtual void INOverheadTran_LineNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_InternalCall == false)
			{
				_InternalCall = true;
				object newval;
				try
				{
					sender.RaiseFieldDefaulting<INOverheadTran.lineNbr>(e.Row, out newval);
				}
				finally
				{
					_InternalCall = false;
				}

				foreach (INComponentTran other in Components.Cache.Deleted)
				{
					if (other.LineNbr == (short)newval)
					{
						newval = (short)newval + 1;
					}
				}

				foreach (INComponentTran other in Components.Cache.Updated)
				{
					if (other.LineNbr == (short)newval)
					{
						newval = (short)newval + 1;
					}
				}
				e.NewValue = newval;
				e.Cancel = true;
			}
		}

		/// <summary><see cref="INKitRegister"/> Selecting</summary>
		protected virtual void INKitRegister_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			var row = (INKitRegister) e.Row;
			if (row != null && (row.TotalCostStock == null || row.TotalCostNonStock == null))
			{
				using (new PXConnectionScope())
				{
					PXFormulaAttribute.CalcAggregate<INComponentTran.tranCost>(Components.Cache, row, true);
					sender.RaiseFieldUpdated<INKitRegister.totalCostStock>(row, null);

					PXFormulaAttribute.CalcAggregate<INOverheadTran.tranCost>(Overhead.Cache, row, true);
					sender.RaiseFieldUpdated<INKitRegister.totalCostNonStock>(row, null);
				}
			}
		}

		protected virtual void INKitRegister_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INKitRegister row = e.Row as INKitRegister;
			if (row == null) return;

			release.SetEnabled(row.Hold == false && row.Released == false);
			viewBatch.SetEnabled(!string.IsNullOrEmpty(row.BatchNbr));

			INKitSpecHdr spec = Spec.Select();
			bool allowCompAddition = spec != null && spec.AllowCompAddition == true;

			lsselect.AllowInsert = row.Released != true && row.KitInventoryID != null && allowCompAddition;
			lsselect.AllowUpdate = row.Released != true;
			lsselect.AllowDelete = row.Released != true && allowCompAddition;

			lsselect2.AllowUpdate = row.Released != true;
			lsselect2.AllowDelete = row.Released != true;
			
			Overhead.Cache.AllowInsert = row.Released != true && row.KitInventoryID != null && allowCompAddition;
			Overhead.Cache.AllowUpdate = row.Released != true;
			Overhead.Cache.AllowDelete = row.Released != true && allowCompAddition;

			PXUIFieldAttribute.SetEnabled<INKitRegister.kitInventoryID>(sender, row, sender.GetStatus(row) == PXEntryStatus.Inserted);
			PXUIFieldAttribute.SetEnabled<INKitRegister.kitRevisionID>(sender, row, sender.GetStatus(row) == PXEntryStatus.Inserted);
			PXUIFieldAttribute.SetEnabled<INKitRegister.status>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<INKitRegister.branchID>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<INKitRegister.reasonCode>(sender, row, sender.AllowUpdate && row.DocType == INDocType.Disassembly);

            PXUIFieldAttribute.SetVisible<INKitSpecStkDet.allowQtyVariation>(SpecComponents.Cache, null, row.DocType != INDocType.Disassembly);
            PXUIFieldAttribute.SetVisible<INKitSpecStkDet.maxCompQty>(SpecComponents.Cache, null, row.DocType != INDocType.Disassembly);
            PXUIFieldAttribute.SetVisible<INKitSpecStkDet.minCompQty>(SpecComponents.Cache, null, row.DocType != INDocType.Disassembly);
            PXUIFieldAttribute.SetVisible<INKitSpecStkDet.disassemblyCoeff>(SpecComponents.Cache, null, row.DocType == INDocType.Disassembly);
            PXUIFieldAttribute.SetVisible<INKitSpecNonStkDet.allowQtyVariation>(SpecOverhead.Cache, null, row.DocType != INDocType.Disassembly);
            PXUIFieldAttribute.SetVisible<INKitSpecNonStkDet.maxCompQty>(SpecOverhead.Cache, null, row.DocType != INDocType.Disassembly);
            PXUIFieldAttribute.SetVisible<INKitSpecNonStkDet.minCompQty>(SpecOverhead.Cache, null, row.DocType != INDocType.Disassembly);

            PXUIFieldAttribute.SetEnabled<INKitSpecStkDet.allowQtyVariation>(SpecComponents.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<INKitSpecStkDet.maxCompQty>(SpecComponents.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<INKitSpecStkDet.minCompQty>(SpecComponents.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<INKitSpecStkDet.disassemblyCoeff>(SpecOverhead.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<INKitSpecNonStkDet.allowQtyVariation>(SpecOverhead.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<INKitSpecNonStkDet.maxCompQty>(SpecOverhead.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<INKitSpecNonStkDet.minCompQty>(SpecOverhead.Cache, null, false);
		}

		protected virtual void INKitRegister_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			//Debug.Print("INKitRegister_RowUpdated");

			INKitRegister row = e.Row as INKitRegister;
			if (row != null)
			{
                if (!(row.DocType == INDocType.Disassembly && IsWhenReceivedSerialNumbered(row.KitInventoryID)))
				{
					if (!sender.ObjectsEqual<INKitRegister.kitInventoryID, INKitRegister.kitRevisionID>(e.Row, e.OldRow))
					{
						decimal inBase = INUnitAttribute.ConvertToBase(sender, row.KitInventoryID, row.UOM, (row.Qty ?? 0), INPrecision.QUANTITY);
						RebuildComponents(inBase);
					}
					else if (!sender.ObjectsEqual<INKitRegister.qty,INKitRegister.uOM>(e.Row, e.OldRow))
					{
						decimal inBaseQty = INUnitAttribute.ConvertToBase(sender, row.KitInventoryID, row.UOM, (row.Qty ?? 0), INPrecision.QUANTITY);
                        decimal inBaseQtyOld = INUnitAttribute.ConvertToBase(sender, row.KitInventoryID, ((INKitRegister)e.OldRow).UOM, (((INKitRegister)e.OldRow).Qty ?? 0), INPrecision.QUANTITY);
						RecountComponents(inBaseQty, inBaseQtyOld);
					}
				}

				if (!sender.ObjectsEqual<INKitRegister.siteID>(e.Row, e.OldRow))
				{
					foreach (INComponentTran tran in Components.Select())
					{
						INComponentTran copy = (INComponentTran) Components.Cache.CreateCopy(tran);
                        copy.BranchID = row.BranchID;
                        copy.SiteID = row.SiteID;
						Components.Update(copy);
					}

					foreach (INOverheadTran tran in Overhead.Select())
					{
						INOverheadTran copy = (INOverheadTran)Overhead.Cache.CreateCopy(tran);
                        copy.BranchID = row.BranchID;
						copy.SiteID = row.SiteID;
						Overhead.Update(copy);
					}
				}

				if (row.Hold != true && row.Qty == 0m && !sender.ObjectsEqual<INKitRegister.hold, INKitRegister.qty>(e.Row, e.OldRow) &&
					(Components.SelectMain().Any(c => c.Qty > 0m) || Overhead.SelectMain().Any(c => c.Qty > 0m)))
				{
					sender.RaiseExceptionHandling<INKitRegister.qty>(row, row.Qty, new PXSetPropertyException(CS.Messages.Entry_GT, (object)0m));
				}
			}
		}

		protected virtual void INKitRegister_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				INKitRegister row = e.Row as INKitRegister;

				if (row != null && row.Hold != true && row.Qty == 0m &&
					(Components.SelectMain().Any(c => c.Qty > 0m) || Overhead.SelectMain().Any(c => c.Qty > 0m)))
				{
					sender.RaiseExceptionHandling<INKitRegister.qty>(row, row.Qty, new PXSetPropertyException(CS.Messages.Entry_GT, (object)0m));
				}

				PXDefaultAttribute.SetPersistingCheck<INKitRegister.reasonCode>(sender, e.Row, ((INKitRegister)e.Row).DocType == INDocType.Disassembly ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}
		
		protected virtual void INKitRegister_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			//to compensate for lsselects - delete/insert on Master_Update

			INKitRegister row = e.Row as INKitRegister;
			if (row != null)
			{
				if (row.KitInventoryID != null && row.KitRevisionID != null)
				{
					decimal inBase = INUnitAttribute.ConvertToBase(sender, row.KitInventoryID, row.UOM, (row.Qty ?? 0), INPrecision.QUANTITY);
					RebuildComponents(inBase);
				}
			}

			Guid? noteid = ((INKitRegister)e.Row).NoteID;

			if (noteid != null)
			{
				PXCache cache = sender.Graph.Caches[typeof(Note)];
				foreach (Note note in cache.Cached)
				{
					if (note.NoteID == noteid)
					{
						if (cache.GetStatus(note) == PXEntryStatus.Deleted)
						{
							cache.SetStatus(note, PXEntryStatus.Updated);
						}
						if (cache.GetStatus(note) == PXEntryStatus.InsertedDeleted)
						{
							cache.SetStatus(note, PXEntryStatus.Inserted);
						}
					}
				}

				cache = sender.Graph.Caches[typeof(NoteDoc)];
				foreach (NoteDoc note in cache.Cached)
				{
					if (note.NoteID == noteid)
					{
						if (cache.GetStatus(note) == PXEntryStatus.Deleted)
						{
							cache.SetStatus(note, PXEntryStatus.Updated);
						}
						if (cache.GetStatus(note) == PXEntryStatus.InsertedDeleted)
						{
							cache.SetStatus(note, PXEntryStatus.Inserted);
						}
					}
				}
			}
		}

		protected virtual void INKitRegister_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<INKitRegister.branchID>(e.Row);
		}

		//protected virtual void INKitRegister_SiteID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		//{
		//	INSite site = PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(this, e.NewValue);

		//	if (site != null)
		//	{
		//		object newBranchID = site.BranchID;

		//		cache.RaiseFieldVerifying<INKitRegister.branchID>(e.Row, ref newBranchID);
		//	}
		//}

		protected virtual void INKitRegister_ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INKitRegister row = e.Row as INKitRegister;
			if (row != null)
			{
				CT.Contract nonProject = PXSelect<CT.Contract, Where<CT.Contract.nonProject, Equal<True>>>.Select(this);
				if (nonProject != null)
				{
					e.NewValue = nonProject.ContractID;
				}
			}
		}

		#endregion
		
		#region INComponentTran

        protected virtual void INComponentTran_ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            INComponentTran row = e.Row as INComponentTran;
            if (row != null)
            {
                CT.Contract nonProject = PXSelect<CT.Contract, Where<CT.Contract.nonProject, Equal<True>>>.Select(this);
                if (nonProject != null)
                {
                    e.NewValue = nonProject.ContractID;
                }
            }
        }
		
		protected virtual void INComponentTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<INComponentTran.uOM>(e.Row);
			sender.SetDefaultExt<INComponentTran.tranDesc>(e.Row);
			sender.SetDefaultExt<INComponentTran.unitCost>(e.Row);
		}

		protected virtual void INComponentTran_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<INComponentTran.unitCost>(e.Row);
		}

		protected virtual void INComponentTran_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INComponentTran row = e.Row as INComponentTran;
			if (row != null)
			{
				INKitSpecStkDet spec = GetComponentSpecByID(row.InventoryID, row.SubItemID);
				if (spec != null && spec.AllowSubstitution == false && spec.CompInventoryID != Convert.ToInt32(e.NewValue))
				{
					var ex = new PXSetPropertyException(Messages.KitSubstitutionIsRestricted, PXErrorLevel.Error);

					InventoryItem item = InventoryItem.PK.Find(this, (int?)e.NewValue);
					ex.ErrorValue = item?.InventoryCD;

					throw ex;
				}
			}
		}  

		protected virtual void INComponentTran_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = GetInvtMult((INTran)e.Row);
		}

        protected virtual void INComponentTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            INComponentTran row = (INComponentTran)e.Row;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<INComponentTran.unitCost>(sender, row, sender.AllowUpdate && row.DocType == INDocType.Disassembly);
			PXUIFieldAttribute.SetEnabled<INComponentTran.reasonCode>(sender, row, sender.AllowUpdate && row.DocType == INDocType.Disassembly);
        }

        protected virtual void INComponentTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            INComponentTran Row = (INComponentTran)e.Row;
            if (Row != null && Row.InventoryID != null)
            {
                sender.SetDefaultExt<INComponentTran.unitCost>(Row);
            }
        }

		protected virtual void INComponentTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Debug.Print("INComponentTran_RowPersisting: {0} {1}", e.Operation, ((INComponentTran)e.Row).LineNbr);
			INComponentTran row = e.Row as INComponentTran;
            if (row == null)
                return;

            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

				INKitSpecStkDet spec = GetComponentSpecByID(row.InventoryID, row.SubItemID);
            INKitRegister assembly = Document.Current;

            if (!VerifyQtyVariance(sender, (INTran)e.Row, spec, assembly))
			{
				if (sender.RaiseExceptionHandling<INComponentTran.qty>(e.Row, row.Qty, new PXSetPropertyException(Messages.KitQtyVarianceIsRestricted)))
				{
					throw new PXSetPropertyException(typeof(INComponentTran.qty).Name, null, Messages.KitQtyVarianceIsRestricted);
				}
			}
            else if (!VerifyQtyBounds(sender, (INTran)e.Row, spec, assembly))
			{
				if (sender.RaiseExceptionHandling<INComponentTran.qty>(e.Row, row.Qty, new PXSetPropertyException(Messages.KitQtyOutOfBounds, spec.MinCompQty * Document.Current.Qty, spec.MaxCompQty * Document.Current.Qty, spec.UOM)))
				{
					throw new PXSetPropertyException(typeof(INComponentTran.qty).Name, null, Messages.KitQtyOutOfBounds, spec.MinCompQty * Document.Current.Qty, spec.MaxCompQty * Document.Current.Qty, spec.UOM);
				}
			}
		}        

		protected virtual void INComponentTran_UnitCost_FieldDefaulting(PXCache sedner, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = 0m;
			INComponentTran row = e.Row as INComponentTran;
			if(row != null && row.InventoryID != null && row.UOM != null && row.SiteID != null)
			{
				var res  = (PXResult<INItemSite, InventoryItem>)
				PXSelectJoin<INItemSite,
					InnerJoin<InventoryItem, On<INItemSite.FK.InventoryItem>>,
					Where<INItemSite.inventoryID, Equal<Current<INComponentTran.inventoryID>>,
						And<INItemSite.siteID, Equal<Current<INComponentTran.siteID>>>>>
					.SelectSingleBound(this, new object[] {row});
				InventoryItem item = res;
				if (item != null && item.InventoryID != null)				
					e.NewValue = PO.POItemCostManager.ConvertUOM(this, item, item.BaseUnit, ((INItemSite) res).TranUnitCost.GetValueOrDefault(), row.UOM);				
			}
		}

		#endregion

		#region INComponentTranSplit Events

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[INKitTranSplitPlanID(typeof(INKitRegister.noteID), typeof(INKitRegister.hold), typeof(INKitRegister.transferType))]
		protected virtual void INComponentTranSplit_PlanID_CacheAttached(PXCache sender)
		{
		}

		#endregion


		#region INOverheadTran

		protected virtual void INOverheadTran_UnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = 0m;
			INOverheadTran row = e.Row as INOverheadTran;
			if (row != null && row.InventoryID != null && row.UOM != null)
			{
				InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
				if (item != null)
					e.NewValue = PO.POItemCostManager.ConvertUOM(this, item, item.BaseUnit, item.StdCost.GetValueOrDefault(), row.UOM);
			}
		}

		protected virtual void INOverheadTran_ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            INOverheadTran row = e.Row as INOverheadTran;
            if (row != null)
            {
                CT.Contract nonProject = PXSelect<CT.Contract, Where<CT.Contract.nonProject, Equal<True>>>.Select(this);
                if (nonProject != null)
                {
                    e.NewValue = nonProject.ContractID;
                }
            }
        }
		
		protected virtual void INOverheadTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<INOverheadTran.uOM>(e.Row);
			sender.SetDefaultExt<INOverheadTran.tranDesc>(e.Row);
			sender.SetDefaultExt<INOverheadTran.unitCost>(e.Row);
		}

		protected virtual void INOverheadTran_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = GetInvtMult((INOverheadTran)e.Row);
		}

		protected virtual void INOverheadTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INOverheadTran row = (INOverheadTran)e.Row;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<INOverheadTran.reasonCode>(sender, row, sender.AllowUpdate && row.DocType == INDocType.Disassembly);
		}

		protected virtual void INOverheadTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INOverheadTran row = e.Row as INOverheadTran;
            if (row == null)
                return;
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

            INKitSpecNonStkDet spec = GetNonStockComponentSpecByID(row.InventoryID);
            INKitRegister assembly = Document.Current;

            if (!VerifyQtyVariance(sender, (INOverheadTran)e.Row, spec, assembly))
			{
				if (sender.RaiseExceptionHandling<INOverheadTran.qty>(e.Row, row.Qty, new PXSetPropertyException(Messages.KitQtyVarianceIsRestricted)))
				{
					throw new PXSetPropertyException(typeof(INOverheadTran.qty).Name, null, Messages.KitQtyVarianceIsRestricted);
				}
			}
            else if (!VerifyQtyBounds(sender, (INOverheadTran)e.Row, spec, assembly))
			{
				if (sender.RaiseExceptionHandling<INOverheadTran.qty>(e.Row, row.Qty, new PXSetPropertyException(Messages.KitQtyOutOfBounds, spec.MinCompQty * Document.Current.Qty, spec.MaxCompQty * Document.Current.Qty, spec.UOM)))
				{
					throw new PXSetPropertyException(typeof(INOverheadTran.qty).Name, null, Messages.KitQtyOutOfBounds, spec.MinCompQty * Document.Current.Qty, spec.MaxCompQty * Document.Current.Qty, spec.UOM);
				}
			}
		}

		#endregion

		#region INKitTranSplit

		protected virtual void INKitTranSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INKitTranSplit row = e.Row as INKitTranSplit;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<INKitTranSplit.lotSerialNbr>(sender, row, row.DocType == INDocType.Disassembly);
				PXUIFieldAttribute.SetEnabled<INKitTranSplit.subItemID>(sender, row, sender.GetStatus(row) == PXEntryStatus.Inserted);
				PXUIFieldAttribute.SetEnabled<INKitTranSplit.locationID>(sender, row, sender.GetStatus(row) == PXEntryStatus.Inserted);
			}
		}

		protected virtual void INKitTranSplit_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			INKitTranSplit row = e.Row as INKitTranSplit;
			if (row != null)
			{
                if (row.DocType == INDocType.Disassembly && IsWhenReceivedSerialNumbered(row.InventoryID))
				{
					if (row.Qty > 0)
					{
						AddKit(row.LotSerialNbr, row.InventoryID);
					}
				}

			}
		}

		protected virtual void INKitTranSplit_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			//Debug.Print("INComponentMasterTranSplit_RowUpdated {0}", ((INComponentMasterTranSplit)e.Row).LotSerialNbr);

			INKitTranSplit row = e.Row as INKitTranSplit;
			if (row != null)
			{
                if (row.DocType == INDocType.Disassembly && IsWhenReceivedSerialNumbered(row.InventoryID))
				{
					if (!sender.ObjectsEqual<INKitTranSplit.qty>(e.Row, e.OldRow))
					{
						if (((INKitTranSplit)e.Row).Qty == 0)
						{
							RemoveKit(row.LotSerialNbr, row.InventoryID);
						}
						else
						{
							AddKit(row.LotSerialNbr, row.InventoryID);
						}
					}
					else if (!sender.ObjectsEqual<INKitTranSplit.lotSerialNbr>(e.Row, e.OldRow))
					{
						RemoveKit(((INKitTranSplit)e.OldRow).LotSerialNbr, ((INKitTranSplit)e.OldRow).InventoryID);
						AddKit(((INKitTranSplit)e.Row).LotSerialNbr, ((INKitTranSplit)e.OldRow).InventoryID);
					}
				}

			}
		}

		protected virtual void INKitTranSplit_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			INKitTranSplit row = e.Row as INKitTranSplit;
			if (row != null)
			{
				if (row.DocType == INDocType.Disassembly && IsWhenReceivedSerialNumbered(row.InventoryID))
				{
					if (row.Qty > 0)
					{
						RemoveKit(row.LotSerialNbr, row.InventoryID);
					}
				}
			}
			//Debug.Print("INComponentMasterTranSplit_RowDeleted {0}", ((INComponentMasterTranSplit)e.Row).LotSerialNbr);
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[INKitTranSplitPlanID(typeof(INKitRegister.noteID), typeof(INKitRegister.hold), typeof(INKitRegister.transferType))]
		protected virtual void INKitTranSplit_PlanID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#endregion

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (IsCopyPasteContext)
			{
				bool componentsAllowInsert = Components.Cache.AllowInsert;
				bool componentsAllowUpdate = Components.Cache.AllowUpdate;
				bool componentsAllowDelete = Components.Cache.AllowDelete;
				bool overheadAllowInsert = Overhead.Cache.AllowInsert;
				bool overheadAllowUpdate = Overhead.Cache.AllowUpdate;
				bool overheadAllowDelete = Overhead.Cache.AllowDelete;

				try
				{
					Components.Cache.AllowInsert = true;
					Components.Cache.AllowUpdate = true;
					Components.Cache.AllowDelete = true;
					Overhead.Cache.AllowInsert = true;
					Overhead.Cache.AllowUpdate = true;
					Overhead.Cache.AllowDelete = true;

					return base.ExecuteUpdate(viewName, keys, values, parameters);
				}
				finally
				{
					Components.Cache.AllowInsert = componentsAllowInsert;
					Components.Cache.AllowUpdate = componentsAllowUpdate;
					Components.Cache.AllowDelete = componentsAllowDelete;
					Overhead.Cache.AllowInsert = overheadAllowInsert;
					Overhead.Cache.AllowUpdate = overheadAllowUpdate;
					Overhead.Cache.AllowDelete = overheadAllowDelete;
				}

			}
			else
			{
				return base.ExecuteUpdate(viewName, keys, values, parameters);
			}
		}
		
		public override void Persist()
        {
            if (Document.Current != null)
                DistributeParts();
            base.Persist();
        }

		public virtual void RebuildComponents(decimal numberOfKits)
		{
			if (this.IsCopyPasteContext)
				return;

			foreach (INComponentTran t in Components.Select())
			{
				Components.Delete(t);
			}

			foreach (INOverheadTran t in Overhead.Select())
			{
				Overhead.Delete(t);
			}

			if (Document.Current != null)
			{
				foreach (PXResult<INKitSpecStkDet, InventoryItem> res in SpecComponents.Select())
				{
					INKitSpecStkDet spec = (INKitSpecStkDet)res;
					InventoryItem item = (InventoryItem)res;

					INComponentTran tran = new INComponentTran();
					tran.DocType = Document.Current.DocType;
					tran.TranType = tran.DocType == INDocType.Disassembly ? INTranType.Disassembly : INTranType.Assembly;
					tran.InvtMult = GetInvtMult(tran);
					tran.InventoryID = spec.CompInventoryID;
                    tran = PXCache<INComponentTran>.CreateCopy( Components.Insert(tran));

					tran.SubItemID = spec.CompSubItemID;
					if (tran.DocType == INDocType.Disassembly)
					{
						tran.Qty = spec.DfltCompQty * numberOfKits * spec.DisassemblyCoeff;
					}
					else
					{
						tran.Qty = spec.DfltCompQty * numberOfKits;
					}
					tran.UOM = spec.UOM;
					tran.SiteID = Document.Current.SiteID;

					if (Document.Current.DocType == INDocType.Disassembly)
					{
						tran.LocationID = Document.Current.LocationID;
					}

					tran = Components.Update(tran);
				}

				foreach (PXResult<INKitSpecNonStkDet, InventoryItem> res in SpecOverhead.Select())
				{
					INKitSpecNonStkDet spec = (INKitSpecNonStkDet)res;
					InventoryItem item = (InventoryItem)res;

					INOverheadTran tran = new INOverheadTran();
					tran.DocType = Document.Current.DocType;
					tran.TranType = tran.DocType == INDocType.Disassembly ? INTranType.Disassembly : INTranType.Assembly;
					tran.InvtMult = GetInvtMult(tran);
					tran.InventoryID = spec.CompInventoryID;
					tran.Qty = spec.DfltCompQty * numberOfKits;
					tran.UOM = spec.UOM;
					tran.SiteID = Document.Current.SiteID;

					Overhead.Insert(tran);
				}
			}

			
		}

		public virtual void RecountComponents(decimal numberOfKits, decimal oldNumberOfKits)
		{
			foreach (PXResult<INComponentTran, InventoryItem, INKitSpecStkDet> res in Components.Select())
			{
				INComponentTran tran = (INComponentTran) Components.Cache.CreateCopy((INComponentTran)res);
				INKitSpecStkDet spec = (INKitSpecStkDet)res;

				if (spec.DfltCompQty != null)
				{
					if (tran.DocType == INDocType.Disassembly)
					{
						tran.Qty = spec.DfltCompQty * numberOfKits * spec.DisassemblyCoeff;
					}
					else
					{
						tran.Qty = spec.DfltCompQty * numberOfKits;
					}
				}
				else
				{
					//Component not found in Specs. Prorate Qty:
					if (oldNumberOfKits > 0)
					{
						
						tran.Qty = tran.Qty * numberOfKits / oldNumberOfKits;
					}
					else
					{
						tran.Qty = numberOfKits;
					}
				}
				
				if ( spec.UOM != null )
					tran.UOM = spec.UOM;
				Components.Update(tran);
			}

			foreach (PXResult<INOverheadTran, InventoryItem, INKitSpecNonStkDet> res in Overhead.Select())
			{
				INOverheadTran tran = (INOverheadTran)Overhead.Cache.CreateCopy((INOverheadTran)res); ;
				INKitSpecNonStkDet spec = (INKitSpecNonStkDet)res;

				if (spec.DfltCompQty != null)
				{
					tran.Qty = (spec.DfltCompQty ?? 1) * numberOfKits;
				}
				else
				{
					//Component not found in Specs. Prorate Qty:

					if (oldNumberOfKits > 0)
					{
						tran.Qty = tran.Qty * numberOfKits / oldNumberOfKits;
					}
					else
					{
						tran.Qty = numberOfKits;
					}
				}

				if (spec.UOM != null)
					tran.UOM = spec.UOM;
				Overhead.Update(tran);
			}

		}

		public virtual bool VerifyQtyVariance(PXCache sender, INTran row, INKitSpecStkDet spec, INKitRegister assembly)
		{
			if (Document.Current != null && row != null && spec != null && spec.AllowQtyVariation == false )
			{
                decimal inBase = INUnitAttribute.ConvertToBase(sender, row.InventoryID, row.UOM, (row.Qty ?? 0), INPrecision.QUANTITY);
                decimal UOMQtyCoef = INUnitAttribute.ConvertToBase(sender, assembly.KitInventoryID, assembly.UOM, (assembly.Qty ?? 0), INPrecision.QUANTITY);
                decimal value = (spec.DfltCompQty ?? 0) * UOMQtyCoef;
                if (IsSerialNumbered(row.InventoryID)) value = Math.Ceiling(value);
                decimal inBaseSpec = INUnitAttribute.ConvertToBase(sender, row.InventoryID, spec.UOM, value, INPrecision.QUANTITY);
				
                if (Document.Current.DocType != INDocType.Disassembly)
				{
                   return inBase == inBaseSpec;
				}
			}

			return true;
		}

		public virtual bool VerifyQtyVariance(PXCache sender, INOverheadTran row, INKitSpecNonStkDet spec, INKitRegister assembly)
		{
			if (Document.Current != null && row != null && spec != null && spec.AllowQtyVariation == false)
			{
                decimal inBase = INUnitAttribute.ConvertToBase(sender, row.InventoryID, row.UOM, (row.Qty ?? 0), INPrecision.QUANTITY);
                decimal UOMQtyCoef = INUnitAttribute.ConvertToBase(sender, assembly.KitInventoryID, assembly.UOM, (assembly.Qty ?? 0), INPrecision.QUANTITY);
                decimal inBaseSpec = INUnitAttribute.ConvertToBase(sender, row.InventoryID, spec.UOM, (spec.DfltCompQty ?? 0) * UOMQtyCoef, INPrecision.QUANTITY);

				if (Document.Current.DocType != INDocType.Disassembly)
				{
					return inBase == inBaseSpec;
				}
			}

			return true;
		}

		public virtual bool VerifyQtyBounds(PXCache sender, INTran row, INKitSpecStkDet spec, INKitRegister assembly)
		{
            if (Document.Current != null && row != null && spec != null && spec.AllowQtyVariation == true && Document.Current.DocType != INDocType.Disassembly)
			{
                decimal inBase = INUnitAttribute.ConvertToBase(sender, row.InventoryID, row.UOM, row.Qty ?? 0, INPrecision.QUANTITY);
                decimal UOMQtyCoef = INUnitAttribute.ConvertToBase(sender, assembly.KitInventoryID, assembly.UOM, (assembly.Qty ?? 0), INPrecision.QUANTITY);

				if (spec.MinCompQty != null)
				{
                    decimal inBaseSpec = INUnitAttribute.ConvertToBase(sender, row.InventoryID, spec.UOM, spec.MinCompQty.Value * UOMQtyCoef, INPrecision.QUANTITY);

					if (inBase < inBaseSpec)
						return false;
				}

				if (spec.MaxCompQty != null)
				{
                    decimal inBaseSpec = INUnitAttribute.ConvertToBase(sender, row.InventoryID, spec.UOM, spec.MaxCompQty.Value * UOMQtyCoef, INPrecision.QUANTITY);

					if (inBase > inBaseSpec)
						return false;
				}
			}

			return true;
		}

		public virtual bool VerifyQtyBounds(PXCache sender, INOverheadTran row, INKitSpecNonStkDet spec, INKitRegister assembly)
		{
			if (Document.Current != null && row != null && spec != null && spec.AllowQtyVariation == true && Document.Current.DocType != INDocType.Disassembly)
			{
				decimal inBase = INUnitAttribute.ConvertToBase(sender, row.InventoryID, row.UOM, row.Qty ?? 0, INPrecision.QUANTITY);
                decimal UOMQtyCoef = INUnitAttribute.ConvertToBase(sender, assembly.KitInventoryID, assembly.UOM, (assembly.Qty ?? 0), INPrecision.QUANTITY);

				if (spec.MinCompQty != null)
				{
                    decimal inBaseSpec = INUnitAttribute.ConvertToBase(sender, row.InventoryID, spec.UOM, spec.MinCompQty.Value * UOMQtyCoef, INPrecision.QUANTITY);

					if (inBase < inBaseSpec)
						return false;
				}

				if (spec.MaxCompQty != null)
				{
                    decimal inBaseSpec = INUnitAttribute.ConvertToBase(sender, row.InventoryID, spec.UOM, spec.MaxCompQty.Value * UOMQtyCoef, INPrecision.QUANTITY);

					if (inBase > inBaseSpec)
						return false;
				}
			}

			return true;
		}
				
		public virtual short? GetInvtMult(INTran tran)
		{
			short? result = null;

			if (Document.Current != null)
			{
				if (Document.Current.DocType == INDocType.Disassembly)
				{
					if (tran.LineNbr != Document.Current.KitLineNbr)
					{
						result = 1;
					}
					else
					{
						result = -1;
					}
				}
				else
				{
					if (tran.LineNbr != Document.Current.KitLineNbr)
					{
						result = -1;
					}
					else
					{
						result = 1;
					}
				}
			}

			return result;
		}

		public virtual short? GetInvtMult(INOverheadTran tran)
		{
			short? result = null;

			if (Document.Current != null)
			{
				if (Document.Current.DocType == INDocType.Disassembly)
				{
					if (tran.LineNbr != Document.Current.KitLineNbr)
					{
						result = 1;
					}
					else
					{
						result = -1;
					}
				}
				else
				{
					if (tran.LineNbr != Document.Current.KitLineNbr)
					{
						result = -1;
					}
					else
					{
						result = 1;
					}
				}
			}

			return result;
		}
				
		public virtual bool IsSerialNumbered(int? inventoryID)
		{
			bool result = false;
			var item = InventoryItem.PK.Find(this, inventoryID);
			var lotSerClass = INLotSerClass.PK.Find(this, item?.LotSerClassID);
			if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				result = true;
			}

			return result;
		}

		public virtual bool IsWhenReceivedSerialNumbered(int? inventoryID)
		{
			bool result = false;
			var item = InventoryItem.PK.Find(this, inventoryID);
			var lotSerClass = INLotSerClass.PK.Find(this, item?.LotSerClassID);

			if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered && lotSerClass.LotSerAssign == INLotSerAssign.WhenReceived)
			{
				result = true;
			}

			return result;
		}

		public virtual void DistributeParts()
        {
            Dictionary<string , INKitSerialPart> tracks = GetPartsDistribution();
            foreach (INKitSerialPart part in SerialNumberedParts.Select())
            {
                if (!tracks.ContainsKey(string.Format("{0}.{1}-{2}.{3}", part.KitLineNbr, part.KitSplitLineNbr, part.PartLineNbr, part.PartSplitLineNbr)))
                {
                    SerialNumberedParts.Delete(part);
                }
            }

            foreach (INKitSerialPart track in tracks.Values)
            {
                INKitSerialPart part = SerialNumberedParts.Locate(track);

                if (part == null)
                {
                    SerialNumberedParts.Insert(track);
                }
            }

        }

		public virtual Dictionary<string, INKitSerialPart> GetPartsDistribution()
        {
            Dictionary<string, INKitSerialPart> list = new Dictionary<string, INKitSerialPart>();
            if (IsSerialNumbered(Document.Current.KitInventoryID))
            {
                PXResultset<INKitTranSplit> kitSplits = MasterSplits.Select();

                PXSelectBase<INComponentTranSplit> CompSplits = new PXSelect<INComponentTranSplit,
                Where<INComponentTranSplit.docType, Equal<Required<INComponentTran.docType>>,
                And<INComponentTranSplit.refNbr, Equal<Required<INComponentTran.refNbr>>,
                And<INComponentTranSplit.lineNbr, Equal<Required<INComponentTran.lineNbr>>>>>>(this);

                for (int kitIndex = 0; kitIndex < kitSplits.Count; kitIndex++)
                {
                    foreach (INComponentTran component in Components.Select())
                    {
                        if (IsWhenReceivedSerialNumbered(component.InventoryID))
                        {
                            PXResultset<INComponentTranSplit> compSplits = CompSplits.Select(component.DocType, component.RefNbr, component.LineNbr);

                            if (compSplits.Count % kitSplits.Count != 0)
                            {
								if (Components.Cache.GetStatus(component) == PXEntryStatus.Notchanged)
								{
									Components.Cache.SetStatus(component, PXEntryStatus.Modified);
								}

								if (Components.Cache.RaiseExceptionHandling<INComponentTran.qty>(component, component.Qty, new PXSetPropertyException(Messages.KitQtyNotEvenDistributed)))
                                {
                                    throw new PXSetPropertyException(typeof(INComponentTran.qty).Name, null, Messages.KitQtyNotEvenDistributed);
                                }
                            }

                            int partsInKit = compSplits.Count / kitSplits.Count;
                            int startIndex = kitIndex * partsInKit;
                            for (int partIndex = startIndex; partIndex < startIndex + partsInKit; partIndex++)
                            {
                                INKitTranSplit kitSplit = kitSplits[kitIndex];
                                INComponentTranSplit partSplit = compSplits[partIndex];

                                INKitSerialPart track = new INKitSerialPart();
                                track.DocType = kitSplit.DocType;
                                track.RefNbr = kitSplit.RefNbr;
                                track.KitLineNbr = kitSplit.LineNbr;
                                track.KitSplitLineNbr = kitSplit.SplitLineNbr;
                                track.PartLineNbr = partSplit.LineNbr;
                                track.PartSplitLineNbr = partSplit.SplitLineNbr;

                                list.Add(string.Format("{0}.{1}-{2}.{3}", track.KitLineNbr, track.KitSplitLineNbr, track.PartLineNbr, track.PartSplitLineNbr), track);
                            }
                        }
                    }
                }
            }
            return list;
        }

		public virtual void AddKit(string serialNumber, int? inventoryID)
        {
            INTranSplit originalMasterTranSplit;
            INTran originalMasterTran;
            INRegister originalDoc;

            SearchOriginalAssembySplitLine(serialNumber, inventoryID, out originalMasterTranSplit, out originalMasterTran, out originalDoc);
            if (originalMasterTranSplit != null)
            {
                decimal numberOfKits = originalMasterTran.Qty.Value;

                #region Stock Items / Components

                PXResultset<INTran> items = PXSelectJoin<INTran,
                            InnerJoin<InventoryItem, On2<INTran.FK.InventoryItem,
                                And<InventoryItem.stkItem, Equal<True>>>,
                            LeftJoin<INKitSpecStkDet, On<INTran.inventoryID, Equal<INKitSpecStkDet.compInventoryID>,
                                And<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>,
                                And<INKitSpecStkDet.revisionID, Equal<Required<INKitSpecStkDet.revisionID>>>>>>>,
                            Where<INTran.docType, Equal<INDocType.production>,
                            And<INTran.refNbr, Equal<Required<INTran.refNbr>>,
                            And<INTran.invtMult, Equal<shortMinus1>>>>>.Select(this, originalDoc.KitInventoryID, originalDoc.KitRevisionID, originalDoc.RefNbr);

                foreach (PXResult<INTran, InventoryItem, INKitSpecStkDet> res in items)
                {
                    InventoryItem item = (InventoryItem)res;
                    INTran origTran = (INTran)res;
                    INKitSpecStkDet spec = (INKitSpecStkDet)res;

                    INComponentTran tran = GetComponentByInventoryID(item.InventoryID);

                    if (tran != null)
                    {
                        Components.Current = tran;

                        if (IsWhenReceivedSerialNumbered(item.InventoryID))
                        {
                            #region Add Splits
                            PXResultset<INTranSplit> parts = GetComponentSplits(originalMasterTranSplit, origTran);
                            foreach (INTranSplit part in parts)
                            {
                                INComponentTranSplit split = PXCache<INComponentTranSplit>.CreateCopy(ComponentSplits.Insert(new INComponentTranSplit()));
                                split.LotSerialNbr = part.LotSerialNbr;
                                split.Qty = part.Qty;
                                ComponentSplits.Update(split);
                            }
                            #endregion
                        }
                        else
                        {
                            INComponentTran copy = PXCache<INComponentTran>.CreateCopy(tran);
                            tran.Qty += (origTran.Qty / numberOfKits) * (spec.DisassemblyCoeff ?? 1);
                            Components.Cache.SetValueExt<INComponentTran.qty>(tran, tran.Qty);
							Components.Cache.MarkUpdated(tran);
							Components.Cache.RaiseRowUpdated(tran, copy);
                        }
                    }
                    else
                    {
                        #region Insert New Component Tran

                        tran = new INComponentTran();
                        tran.DocType = Document.Current.DocType;
						tran.TranType = tran.DocType == INDocType.Disassembly ? INTranType.Disassembly : INTranType.Assembly;
                        tran.InvtMult = GetInvtMult(tran);
                        tran.InventoryID = origTran.InventoryID;
                        tran.SubItemID = origTran.SubItemID;
                        tran.UOM = origTran.UOM;
                       

                        if (IsWhenReceivedSerialNumbered(item.InventoryID))
                        {
                            Components.Insert(tran);

                            #region Add Splits

                            PXResultset<INTranSplit> parts = GetComponentSplits(originalMasterTranSplit, origTran);
                            foreach (INTranSplit part in parts)
                            {
                                INComponentTranSplit split = PXCache<INComponentTranSplit>.CreateCopy(ComponentSplits.Insert(new INComponentTranSplit()));
                                split.LotSerialNbr = part.LotSerialNbr;
                                split.Qty = part.Qty;
                                ComponentSplits.Update(split);
                            }

                            #endregion

                        }
                        else
                        {
                            tran.Qty = (origTran.Qty / numberOfKits) * (spec.DisassemblyCoeff ?? 1);
                            Components.Insert(tran);
                        }

                        #endregion
                    }
                }

                #endregion

                #region Non Stock Items /Overhead

                PXResultset<INTran> overheadItems = PXSelectJoin<INTran,
                            InnerJoin<InventoryItem, 
								On2<INTran.FK.InventoryItem,
                                And<InventoryItem.stkItem, Equal<boolFalse>>>,
                            LeftJoin<INKitSpecNonStkDet, On<INTran.inventoryID, Equal<INKitSpecNonStkDet.compInventoryID>,
                                And<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>,
                                And<INKitSpecNonStkDet.revisionID, Equal<Required<INKitSpecNonStkDet.revisionID>>>>>>>,
                            Where<INTran.docType, Equal<INDocType.production>,
                            And<INTran.refNbr, Equal<Required<INTran.refNbr>>,
                            And<INTran.invtMult, Equal<shortMinus1>>>>>.Select(this, originalDoc.KitInventoryID, originalDoc.KitRevisionID, originalDoc.RefNbr);


                foreach (PXResult<INTran, InventoryItem, INKitSpecNonStkDet> res in overheadItems)
                {
                    InventoryItem item = (InventoryItem)res;
                    INTran origTran = (INTran)res;
                    INKitSpecNonStkDet spec = (INKitSpecNonStkDet)res;

                    INOverheadTran tran = GetOverheadByInventoryID(item.InventoryID);

                    if (tran != null)
                    {
                        tran.Qty += origTran.Qty / numberOfKits;
                        Overhead.Update(tran);
                    }
                    else
                    {
                        #region Insert New Overhead Tran

                        tran = new INOverheadTran();
                        tran.DocType = Document.Current.DocType;
						tran.TranType = tran.DocType == INDocType.Disassembly ? INTranType.Disassembly : INTranType.Assembly;
                        tran.InvtMult = GetInvtMult(tran);
                        tran.InventoryID = origTran.InventoryID;
                        tran.Qty = origTran.Qty / numberOfKits;
                        tran.UOM = origTran.UOM;
                        tran.SiteID = origTran.SiteID;
                        //location for disassembled components will be default from default receipt location
                        //tran.LocationID = origTran.LocationID;

                        Overhead.Insert(tran);

                        #endregion
                    }
                }

                #endregion

            }
            else
            {
                #region Original Kit Assembly was not found - Use Specification
                foreach (PXResult<INKitSpecStkDet, InventoryItem> res in SpecComponents.Select())
                {
                    INKitSpecStkDet spec = (INKitSpecStkDet)res;
                    InventoryItem item = (InventoryItem)res;

                    INComponentTran tran = GetComponentByInventoryID(item.InventoryID);

                    if (tran != null)
                    {
                        INComponentTran copy = PXCache<INComponentTran>.CreateCopy(tran);
                        tran.Qty += spec.DfltCompQty * (spec.DisassemblyCoeff ?? 1);
                        Components.Cache.SetValueExt<INComponentTran.qty>(tran, tran.Qty);
						Components.Cache.MarkUpdated(tran);
						Components.Cache.RaiseRowUpdated(tran, copy);
                    }
                    else
                    {
                        #region Insert New Component Tran

                        tran = new INComponentTran();
                        tran.DocType = Document.Current.DocType;
						tran.TranType = tran.DocType == INDocType.Disassembly ? INTranType.Disassembly : INTranType.Assembly;
                        tran.InvtMult = GetInvtMult(tran);
                        tran.InventoryID = spec.CompInventoryID;
                        tran.SubItemID = spec.CompSubItemID;
                        tran.UOM = spec.UOM;
                        tran.SiteID = Document.Current.SiteID;
                        //location for disassembled components will be default from default receipt location
                        //tran.LocationID = Document.Current.LocationID;
                        tran.Qty = spec.DfltCompQty * (spec.DisassemblyCoeff ?? 1);
                        Components.Insert(tran);

                        #endregion
                    }

                }

				foreach (PXResult<INKitSpecNonStkDet, InventoryItem> res in SpecOverhead.Select())
				{
					INKitSpecNonStkDet spec = (INKitSpecNonStkDet)res;
					InventoryItem item = (InventoryItem)res;

					INOverheadTran tran = GetOverheadByInventoryID(item.InventoryID);

					if (tran != null)
					{
						tran.Qty += spec.DfltCompQty;
						Overhead.Update(tran);
					}
					else
					{
						#region Insert New Overhad Tran
						tran = new INOverheadTran();
						tran.DocType = Document.Current.DocType;
						tran.TranType = tran.DocType == INDocType.Disassembly ? INTranType.Disassembly : INTranType.Assembly;
						tran.InvtMult = GetInvtMult(tran);
						tran.InventoryID = spec.CompInventoryID;
						tran.Qty = spec.DfltCompQty;
						tran.UOM = spec.UOM;
						tran.SiteID = Document.Current.SiteID;
                        //location for disassembled components will be default from default receipt location
                        //tran.LocationID = Document.Current.LocationID;

						Overhead.Insert(tran);
						#endregion
					}
				} 
				#endregion
			}
		}
		
		public virtual void RemoveKit(string serialNumber, int? inventoryID)
		{
			INTranSplit originalMasterTranSplit;
			INTran originalMasterTran;
			INRegister originalDoc;

			SearchOriginalAssembySplitLine(serialNumber, inventoryID, out originalMasterTranSplit, out originalMasterTran, out originalDoc);
			if (originalMasterTranSplit != null)
			{
				decimal numberOfKits = originalMasterTran.Qty.Value;

				#region Stock Items / Components
				PXResultset<INTran> items = PXSelectJoin<INTran,
							InnerJoin<InventoryItem, On<INTran.FK.InventoryItem>,
							LeftJoin<INKitSpecStkDet, On<INTran.inventoryID, Equal<INKitSpecStkDet.compInventoryID>>>>,
							Where<INTran.docType, Equal<INDocType.production>,
							And<INTran.refNbr, Equal<Required<INTran.refNbr>>,
							And<INTran.invtMult, Equal<shortMinus1>,
							And<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>,
							And<INKitSpecStkDet.revisionID, Equal<Required<INKitSpecStkDet.revisionID>>>>>>>>.Select(this, originalDoc.RefNbr, originalDoc.KitInventoryID, originalDoc.KitRevisionID);

				foreach (PXResult<INTran, InventoryItem, INKitSpecStkDet> res in items)
				{
					InventoryItem item = (InventoryItem)res;
					INTran origTran = (INTran)res;
					INKitSpecStkDet spec = (INKitSpecStkDet)res;

					INComponentTran tran = GetComponentByInventoryID(item.InventoryID);

					if (tran != null)
					{
						Components.Current = tran;

						if (IsSerialNumbered(item.InventoryID))
						{
							#region Remove Splits
							PXResultset<INTranSplit> parts = GetComponentSplits(originalMasterTranSplit, origTran);
							foreach (INTranSplit part in parts)
							{
								INComponentTranSplit split = PXSelect<INComponentTranSplit,
									Where<INComponentTranSplit.docType, Equal<Current<INKitRegister.docType>>,
									And<INComponentTranSplit.refNbr, Equal<Current<INKitRegister.refNbr>>,
									And<INComponentTranSplit.lineNbr, Equal<Required<INComponentTranSplit.lineNbr>>,
									And<INComponentTranSplit.lotSerialNbr, Equal<Required<INComponentTranSplit.lotSerialNbr>>>>>>>.Select(this, tran.LineNbr, part.LotSerialNbr);
                                
                                INComponentTran parent = (INComponentTran)LSParentAttribute.SelectParent(ComponentSplits.Cache, split, typeof(INComponentTran));
                               
								ComponentSplits.Delete(split);

                                INComponentTran copy = PXCache<INComponentTran>.CreateCopy(parent);
                                copy.Qty--;
                                //copy.UnassignedQty--;
                                Components.Cache.Update(copy);
							}
							#endregion
						}
						else
						{
							INComponentTran copy = PXCache<INComponentTran>.CreateCopy(tran);
							tran.Qty -= (origTran.Qty / numberOfKits) * spec.DisassemblyCoeff ?? 1;
							if (tran.Qty < 0)
								tran.Qty = 0;
                            Components.Cache.SetValueExt<INComponentTran.qty>(tran, tran.Qty);
							Components.Cache.MarkUpdated(tran);
							Components.Cache.RaiseRowUpdated(tran, copy);
						}
					}
				} 
				#endregion
				
				#region NonStcok Items / Overhead
				PXResultset<INTran> overheadItems = PXSelectJoin<INTran,
							InnerJoin<InventoryItem, On<INTran.FK.InventoryItem>,
							LeftJoin<INKitSpecNonStkDet, On<INTran.inventoryID, Equal<INKitSpecNonStkDet.compInventoryID>>>>,
							Where<INTran.docType, Equal<INDocType.production>,
							And<INTran.refNbr, Equal<Required<INTran.refNbr>>,
							And<INTran.invtMult, Equal<shortMinus1>,
							And<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>,
							And<INKitSpecNonStkDet.revisionID, Equal<Required<INKitSpecNonStkDet.revisionID>>>>>>>>.Select(this, originalDoc.RefNbr, originalDoc.KitInventoryID, originalDoc.KitRevisionID);

				foreach (PXResult<INTran, InventoryItem, INKitSpecNonStkDet> res in overheadItems)
				{
					InventoryItem item = (InventoryItem)res;
					INTran origTran = (INTran)res;
					INKitSpecNonStkDet spec = (INKitSpecNonStkDet)res;

					INOverheadTran tran = GetOverheadByInventoryID(item.InventoryID);

					if (tran != null)
					{
						tran.Qty -= origTran.Qty / numberOfKits;

						if (tran.Qty < 0)
							tran.Qty = 0;

						Overhead.Update(tran);
					}
				} 
				#endregion
			}
			else
			{
				#region Original Kit Assembly was not found - Use Specification
				foreach (PXResult<INKitSpecStkDet, InventoryItem> res in SpecComponents.Select())
				{
					INKitSpecStkDet spec = (INKitSpecStkDet)res;
					InventoryItem item = (InventoryItem)res;

					INComponentTran tran = GetComponentByInventoryID(item.InventoryID);

					if (tran != null)
					{
						INComponentTran copy = PXCache<INComponentTran>.CreateCopy(tran);
						tran.Qty -= spec.DfltCompQty * spec.DisassemblyCoeff ?? 1;

						if (tran.Qty < 0)
							tran.Qty = 0;
                        Components.Cache.SetValueExt<INComponentTran.qty>(tran, tran.Qty);
						Components.Cache.MarkUpdated(tran);
						Components.Cache.RaiseRowUpdated(tran, copy);
					}
				}

				foreach (PXResult<INKitSpecNonStkDet, InventoryItem> res in SpecOverhead.Select())
				{
					INKitSpecNonStkDet spec = (INKitSpecNonStkDet)res;
					InventoryItem item = (InventoryItem)res;

					INOverheadTran tran = GetOverheadByInventoryID(item.InventoryID);

					if (tran != null)
					{
						tran.Qty -= spec.DfltCompQty;
						if (tran.Qty < 0)
							tran.Qty = 0;

						Overhead.Update(tran);
					}
				}
				#endregion
			}
		}


		public virtual void SearchOriginalAssembySplitLine(string serialNumber, int? inventoryID, out INTranSplit originalSplit, out INTran originalMasterTran, out INRegister originalDoc)
		{
			originalSplit = null;
			originalMasterTran = null;
			originalDoc = null;

			if (!string.IsNullOrEmpty(serialNumber))
			{
                PXSelectBase<INTranSplit> split =
                 new PXSelectJoin<INTranSplit,
                 InnerJoin<INTran, On<INTranSplit.FK.Tran>,
                 InnerJoin<INRegister, On<INRegister.FK.KitTran>>>,
                 Where<INRegister.docType, Equal<INDocType.production>,
                     And<INTranSplit.lotSerialNbr, Equal<Required<INTranSplit.lotSerialNbr>>,
                     And<INTranSplit.inventoryID, Equal<Required<INTranSplit.inventoryID>>,
					 And<INTran.qty, NotEqual<decimal0>>>>>>(this);

                PXResultset<INTranSplit> set = split.Select(serialNumber, inventoryID);
			if (set != null && set.Count > 0)
			{
				PXResult<INTranSplit, INTran, INRegister> res = (PXResult<INTranSplit, INTran, INRegister>)set[0];
				originalSplit = (INTranSplit)res;
				originalMasterTran = (INTran)res;
				originalDoc = (INRegister)res;
			}
		}
		}

		public virtual INKitSpecStkDet GetComponentSpecByID(int? inventoryID, int? subItemID)
		{
			return PXSelect<INKitSpecStkDet, Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitRegister.kitInventoryID>>,
				And<INKitSpecStkDet.revisionID, Equal<Current<INKitRegister.kitRevisionID>>,
				And<INKitSpecStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>,
				And<Where<INKitSpecStkDet.compSubItemID, Equal<Required<INKitSpecStkDet.compSubItemID>>, 
					Or<Required<INKitSpecStkDet.compSubItemID>, IsNull>>>>>>>.Select(this, inventoryID, subItemID, subItemID);
		}

		public virtual INKitSpecNonStkDet GetNonStockComponentSpecByID(int? inventoryID)
		{
			return PXSelect<INKitSpecNonStkDet, Where<INKitSpecNonStkDet.kitInventoryID, Equal<Current<INKitRegister.kitInventoryID>>,
				And<INKitSpecNonStkDet.revisionID, Equal<Current<INKitRegister.kitRevisionID>>,
				And<INKitSpecNonStkDet.compInventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>>>>>.Select(this, inventoryID);
		}

		public virtual InventoryItem GetInventoryItemByID(int? inventoryID)
		{
			return InventoryItem.PK.Find(this, inventoryID);
		}

		public virtual INComponentTran GetComponentByInventoryID(int? inventoryID)
		{
			return PXSelect<INComponentTran,
							Where<INComponentTran.docType, Equal<Current<INKitRegister.docType>>,
							And<INComponentTran.refNbr, Equal<Current<INKitRegister.refNbr>>,
							And<INComponentTran.inventoryID, Equal<Required<INComponentTran.inventoryID>>>>>>.Select(this, inventoryID);

		}

		public virtual INOverheadTran GetOverheadByInventoryID(int? inventoryID)
		{
			return PXSelect<INOverheadTran,
							Where<INOverheadTran.docType, Equal<Current<INKitRegister.docType>>,
							And<INOverheadTran.refNbr, Equal<Current<INKitRegister.refNbr>>,
							And<INOverheadTran.inventoryID, Equal<Required<INOverheadTran.inventoryID>>>>>>.Select(this, inventoryID);

		}

		public virtual PXResultset<INTranSplit> GetComponentSplits(INTranSplit originalKitSplit, INTran originalComponent)
		{
			return PXSelectJoin<INTranSplit,
				InnerJoin<INKitSerialPart, On<INKitSerialPart.docType, Equal<INTranSplit.docType>,
					And<INKitSerialPart.refNbr, Equal<INTranSplit.refNbr>,
					And<INKitSerialPart.partLineNbr, Equal<INTranSplit.lineNbr>,
					And<INKitSerialPart.partSplitLineNbr, Equal<INTranSplit.splitLineNbr>>>>>>,
				Where<INKitSerialPart.docType, Equal<Required<INKitSerialPart.docType>>,
				And<INKitSerialPart.refNbr, Equal<Required<INKitSerialPart.refNbr>>,
				And<INKitSerialPart.kitLineNbr, Equal<Required<INKitSerialPart.kitLineNbr>>,
				And<INKitSerialPart.kitSplitLineNbr, Equal<Required<INKitSerialPart.kitSplitLineNbr>>,
				And<INKitSerialPart.partLineNbr, Equal<Required<INKitSerialPart.partLineNbr>>>>>>>>.Select(this, originalKitSplit.DocType, originalKitSplit.RefNbr, originalKitSplit.LineNbr, originalKitSplit.SplitLineNbr, originalComponent.LineNbr);
		}     

	}

	#region DAC Extensions
	[PXProjection(typeof(Select<INTran, Where<INTran.assyType, Equal<INAssyType.compTran>>>), Persistent = true)]
    [PXCacheName(Messages.INComponentTran)]
    [Serializable]
	public partial class INComponentTran : INTran
	{
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch(typeof(INKitRegister.branchID), Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
		public override Int32? BranchID
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
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(typeof(INKitRegister.docType))]
		public override String DocType
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
		#region TranType
		public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXFormula(typeof(Switch<Case<Where<docType, Equal<INDocType.disassembly>>, INTranType.disassembly>, INTranType.assembly>))]
		public override String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(INKitRegister.refNbr))]
		[PXParent(typeof(Select<INKitRegister,
			Where<INKitRegister.docType, Equal<Current<INComponentTran.docType>>,
				And<INKitRegister.refNbr, Equal<Current<INComponentTran.refNbr>>>>>))]
		public override String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXLineNbr(typeof(INKitRegister.lineCntr))]
		public override Int32? LineNbr
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
		#region AssyType
		public new abstract class assyType : PX.Data.BQL.BqlString.Field<assyType> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INAssyType.CompTran)]
		public override String AssyType
		{
			get
			{
				return this._AssyType;
			}
			set
			{
				this._AssyType = value;
			}
		}
		#endregion
        #region ProjectID
        public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
        [PX.Objects.PM.ProjectDefault]
        [PXDBInt(BqlField = typeof(INTran.projectID))]
        public override Int32? ProjectID
        {
            get
            {
                return this._ProjectID;
            }
            set
            {
                this._ProjectID = value;
            }
        }
        #endregion
		#region TranDate
		public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		[PXDBDate()]
		[PXDBDefault(typeof(INKitRegister.tranDate))]
		public override DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[FinPeriodID(
			branchSourceType: typeof(INComponentTran.branchID),
			masterFinPeriodIDType: typeof(INComponentTran.tranPeriodID),
			headerMasterFinPeriodIDType: typeof(INKitRegister.tranPeriodID))]
		public override String FinPeriodID
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
		#region TranPeriodID
		public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		[PeriodID(BqlField = typeof(INTran.tranPeriodID))]
		public override String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region InvtMult
		public new abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		
		[PXDBShort()]
		[PXDefault()]
		public override Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region UOM
		public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<INComponentTran.inventoryID>>>>))]
		[INUnit(typeof(INComponentTran.inventoryID))]
		public override String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region Qty
		public new abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

		[PXDBQuantity(typeof(INComponentTran.uOM), typeof(INComponentTran.baseQty), InventoryUnitType.BaseUnit)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public override Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDefault()]
        [Inventory(typeof(Search2<InventoryItem.inventoryID, 
			InnerJoin<INLotSerClass, On<InventoryItem.FK.LotSerClass>>, 
			Where2<Match<Current<AccessInfo.userName>>, And<InventoryItem.stkItem, Equal<True>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr), DisplayName = "Inventory ID")]
        [PXRestrictor(typeof(Where<INLotSerTrack.serialNumbered, Equal<Current<INKitRegister.lotSerTrack>>,
                                    Or<INLotSerClass.lotSerTrack, Equal<INLotSerTrack.lotNumbered>,
                                    Or<INLotSerClass.lotSerTrack, Equal<INLotSerTrack.notNumbered>>>
                            >), Messages.SNComponentInSNKit)]
        public override Int32? InventoryID
        {
            get
            {
                return _InventoryID;
            }
            set
            {
                _InventoryID = value;
            }
        }
		#endregion
		#region SubItemID
		public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<INComponentTran.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[IN.SubItem(
			typeof(INComponentTran.inventoryID),
			typeof(LeftJoin<INSiteStatus,
				On2<INSiteStatus.FK.SubItem,
				And<INSiteStatus.inventoryID, Equal<Optional<INComponentTran.inventoryID>>,
				And<INSiteStatus.siteID, Equal<Optional<INComponentTran.siteID>>>>>>))]
		[PXFormula(typeof(Default<INComponentTran.inventoryID>))]
		public override Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}

		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[PXDefault(typeof(INKitRegister.siteID))]
		[PXDBInt]
		[PXSelector(typeof(Search<INSite.siteID>), SubstituteKey = typeof(INSite.siteCD))]
		public override Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        [LocationAvail(typeof(INComponentTran.inventoryID),
                      typeof(INComponentTran.subItemID),
                      typeof(INComponentTran.siteID),
					  typeof(Where2<Where<INComponentTran.tranType, Equal<INTranType.assembly>, Or<INComponentTran.tranType, Equal<INTranType.disassembly>>>, And<INComponentTran.invtMult, Equal<shortMinus1>>>),
					  typeof(Where2<Where<INComponentTran.tranType, Equal<INTranType.assembly>, Or<INComponentTran.tranType, Equal<INTranType.disassembly>>>, And<INComponentTran.invtMult, Equal<short1>>>),
                      typeof(Where<False, Equal<True>>))]
        [PXRestrictor(typeof(Where<True, Equal<True>>), null, ReplaceInherited = true)]
        public override Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region UnitCost
		public new abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		
		[PXDBPriceCost()]
		[PXUIField(DisplayName = "Unit Cost")]
		[PXFormula(typeof(Default<INComponentTran.uOM>))]
		public override Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
		#region TranCost
		[PXDBBaseCury(BqlField = typeof(INTran.tranCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ext. Cost")]
		[PXFormula(typeof(Mult<qty, unitCost>), typeof(SumCalc<INKitRegister.totalCostStock>))]
		public override Decimal? TranCost { get; set; }
		public new abstract class tranCost : PX.Data.BQL.BqlDecimal.Field<tranCost> { }
		#endregion
		#region ReasonCode
		public new abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true, BqlField = typeof(INTran.reasonCode))]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<Optional<INComponentTran.docType>>>>))]
		[PXUIField(DisplayName = "Reason Code")]
		public override String ReasonCode
		{
			get
			{
				return this._ReasonCode;
			}
			set
			{
				this._ReasonCode = value;
			}
		}
		#endregion
		#region TranDesc
		public new abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		[PXDBString(256, IsUnicode = true, BqlField = typeof(INTran.tranDesc))]
		[PXUIField(DisplayName = "Description")]
		[PXDefault(typeof(Search<InventoryItem.descr, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>),
			CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public override String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion

		#region Methods

		//This is a bad idea... but still
		public static implicit operator INComponentTranSplit(INComponentTran item)
		{
			INComponentTranSplit ret = new INComponentTranSplit();
			ret.DocType = item.DocType;
			ret.TranType = item.TranType;
			ret.RefNbr = item.RefNbr;
			ret.LineNbr = item.LineNbr;
			ret.SplitLineNbr = (short)1;
			ret.InventoryID = item.InventoryID;
			ret.SiteID = item.SiteID;
			ret.SubItemID = item.SubItemID;
			ret.LocationID = item.LocationID;
			ret.LotSerialNbr = item.LotSerialNbr;
			ret.ExpireDate = item.ExpireDate;
			ret.Qty = item.Qty;
			ret.UOM = item.UOM;
			ret.TranDate = item.TranDate;
			ret.BaseQty = item.BaseQty;
			ret.InvtMult = item.InvtMult;
			ret.Released = item.Released;

			return ret;
		}

		//This is a bad idea... but still
		public static implicit operator INComponentTran(INComponentTranSplit item)
		{
			INComponentTran ret = new INComponentTran();
			ret.DocType = item.DocType;
			ret.TranType = item.TranType;
			ret.RefNbr = item.RefNbr;
			ret.LineNbr = item.LineNbr;
			ret.InventoryID = item.InventoryID;
			ret.SiteID = item.SiteID;
			ret.SubItemID = item.SubItemID;
			ret.LocationID = item.LocationID;
			ret.LotSerialNbr = item.LotSerialNbr;
			ret.Qty = item.Qty;
			ret.UOM = item.UOM;
			ret.TranDate = item.TranDate;
			ret.BaseQty = item.BaseQty;
			ret.InvtMult = item.InvtMult;
			ret.Released = item.Released;

			return ret;
		}
		#endregion
	}

	[PXProjection(typeof(Select<INTran, Where<INTran.assyType, Equal<INAssyType.overheadTran>>>), Persistent = true)]
    [PXCacheName(Messages.INOverheadTran)]
    [Serializable]
	public partial class INOverheadTran : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(typeof(INKitRegister.branchID), BqlField = typeof(INTran.branchID), Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
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
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(1, IsFixed = true, IsKey = true, BqlField=typeof(INTran.docType))]
		[PXDefault(typeof(INKitRegister.docType))]
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
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXDBString(3, IsFixed = true, BqlField=typeof(INTran.tranType))]
		[PXDefault]
		[PXFormula(typeof(Switch<Case<Where<docType, Equal<INDocType.disassembly>>, INTranType.disassembly>, INTranType.assembly>))]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTran.refNbr))]
		[PXDBDefault(typeof(INKitRegister.refNbr))]
		[PXParent(typeof(Select<INKitRegister, 
			Where<INKitRegister.docType, Equal<Current<INOverheadTran.docType>>, 
			And<INKitRegister.refNbr, Equal<Current<INOverheadTran.refNbr>>>>>))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField=typeof(INTran.lineNbr))]
		[PXDefault()]
		[PXLineNbr(typeof(INKitRegister.lineCntr))]
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
		#region AssyType
		public abstract class assyType : PX.Data.BQL.BqlString.Field<assyType> { }
		protected String _AssyType;
		[PXDefault(INAssyType.OverheadTran)]
		[PXDBString(1, IsFixed = true, BqlField=typeof(INTran.assyType))]
		public virtual String AssyType
		{
			get
			{
				return this._AssyType;
			}
			set
			{
				this._AssyType = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PX.Objects.PM.ProjectDefault]
		[PXDBInt(BqlField = typeof(INTran.projectID))]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
		[PXDBDate(BqlField=typeof(INTran.tranDate))]
		[PXDBDefault(typeof(INKitRegister.tranDate))]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[NonStockItem(BqlField=typeof(INTran.inventoryID), DisplayName="Inventory ID")]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(typeof(INOverheadTran.inventoryID), BqlField=typeof(INTran.subItemID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDefault(typeof(INKitRegister.siteID))]
		[PXDBInt(BqlField=typeof(INTran.siteID))]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[LocationAvail(typeof(INOverheadTran.inventoryID), typeof(INOverheadTran.subItemID), typeof(INOverheadTran.siteID), typeof(INOverheadTran.tranType), typeof(INOverheadTran.invtMult), BqlField=typeof(INTran.locationID))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		protected Int16? _InvtMult;
		[PXDBShort(BqlField=typeof(INTran.invtMult))]
		[PXDefault()]
		public virtual Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<INOverheadTran.inventoryID>>>>))]
		[INUnit(typeof(INOverheadTran.inventoryID), BqlField=typeof(INTran.uOM))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity(typeof(INOverheadTran.uOM), typeof(INOverheadTran.baseQty), InventoryUnitType.BaseUnit, BqlField=typeof(INTran.qty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[PXDBDefault(typeof(INKitRegister.finPeriodID))]
		[FinPeriodID(
			branchSourceType: typeof(INOverheadTran.branchID),
			masterFinPeriodIDType: typeof(INOverheadTran.tranPeriodID),
			headerMasterFinPeriodIDType: typeof(INKitRegister.tranPeriodID),
			BqlField = typeof(INTran.finPeriodID))]
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
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(INTran.tranDesc))]
		[PXUIField(DisplayName = "Description")]
		[PXDefault(typeof(Search<InventoryItem.descr, Where<InventoryItem.inventoryID, Equal<Current<INOverheadTran.inventoryID>>>>), 
			CacheGlobal = true, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
        #region ReasonCode
        public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
        protected String _ReasonCode;
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true, BqlField = typeof(INTran.reasonCode))]
        [PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<Optional<INOverheadTran.docType>>>>))]
        [PXUIField(DisplayName = "Reason Code")]
        public virtual String ReasonCode
        {
            get
            {
                return this._ReasonCode;
            }
            set
            {
                this._ReasonCode = value;
            }
        }
        #endregion
		#region BaseQty
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		protected Decimal? _BaseQty;
		[PXDBQuantity(BqlField=typeof(INTran.baseQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseQty;
			}
			set
			{
				this._BaseQty = value;
			}
		}
		#endregion
		#region UnassignedQty
		public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }
		protected Decimal? _UnassignedQty;
		[PXDBDecimal(6, BqlField = typeof(INTran.unassignedQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnassignedQty
		{
			get
			{
				return this._UnassignedQty;
			}
			set
			{
				this._UnassignedQty = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool(BqlField=typeof(INTran.released))]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		protected String _TranPeriodID;
		[PXDBDefault(typeof(INKitRegister.tranPeriodID))]
		[PeriodID(BqlField=typeof(INTran.tranPeriodID))]
		public virtual String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region UnitPrice
		public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }
		protected Decimal? _UnitPrice;
		[PXDBPriceCost(BqlField = typeof(INTran.unitPrice))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price")]
		public virtual Decimal? UnitPrice
		{
			get
			{
				return this._UnitPrice;
			}
			set
			{
				this._UnitPrice = value;
			}
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
		protected Decimal? _TranAmt;
		[PXDBBaseCury(BqlField = typeof(INTran.tranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ext. Price")]
		public virtual Decimal? TranAmt
		{
			get
			{
				return this._TranAmt;
			}
			set
			{
				this._TranAmt = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXDBPriceCost(BqlField=typeof(INTran.unitCost))]
		[PXUIField(DisplayName = "Unit Cost")]
		[PXFormula(typeof(Default<INOverheadTran.uOM>))]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
		#region TranCost
		public abstract class tranCost : PX.Data.BQL.BqlDecimal.Field<tranCost> { }
		protected Decimal? _TranCost;
		[PXDBBaseCury(BqlField = typeof(INTran.tranCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ext. Cost")]
		[PXFormula(typeof(Mult<qty, unitCost>), typeof(SumCalc<INKitRegister.totalCostNonStock>))]
		public virtual Decimal? TranCost
		{
			get
			{
				return this._TranCost;
			}
			set
			{
				this._TranCost = value;
			}
		}
		#endregion
		#region UpdateShippedNotInvoiced
		public abstract class updateShippedNotInvoiced : PX.Data.BQL.BqlBool.Field<updateShippedNotInvoiced> { }
		protected Boolean? _UpdateShippedNotInvoiced;
		[PXDBBool(BqlField = typeof(INTran.updateShippedNotInvoiced))]
		[PXDefault(false)]
		public virtual Boolean? UpdateShippedNotInvoiced
		{
			get
			{
				return this._UpdateShippedNotInvoiced;
			}
			set
			{
				this._UpdateShippedNotInvoiced = value;
			}
		}
		#endregion
		
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID(BqlField = typeof(INTran.createdByID))]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region TranCreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID(BqlField = typeof(INTran.createdByScreenID))]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime(BqlField = typeof(INTran.createdDateTime))]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID(BqlField = typeof(INTran.lastModifiedByID))]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID(BqlField = typeof(INTran.lastModifiedByScreenID))]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime(BqlField = typeof(INTran.lastModifiedDateTime))]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
		protected Byte[] _Tstamp;
		[PXDBTimestamp(BqlField = typeof(INTran.Tstamp))]
		public virtual Byte[] Tstamp
		{
			get
			{
				return this._Tstamp;
			}
			set
			{
				this._Tstamp = value;
			}
		}
		#endregion
	}

	[DebuggerDisplay("SerialNbr={LotSerialNbr}")]
    [PXCacheName(Messages.INComponentTranSplit)]
    [Serializable]
	public partial class INComponentTranSplit : INTranSplit
	{
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(typeof(INComponentTran.docType))]
		public override String DocType
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
		#region TranType
		public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		[PXDBString(3)]
		[PXDefault]
		[PXFormula(typeof(Switch<Case<Where<docType, Equal<INDocType.disassembly>>, INTranType.disassembly>, INTranType.assembly>))]
		public override String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(INKitRegister.refNbr))]
		[PXParent(typeof(Select<INComponentTran, 
			Where<INComponentTran.docType, Equal<Current<docType>>, 
			And<INComponentTran.refNbr, Equal<Current<refNbr>>, 
			And<INComponentTran.lineNbr, Equal<Current<lineNbr>>,
			And<INComponentTran.lineNbr, NotEqual<Current<INKitRegister.kitLineNbr>>>>>>>))]
		public override String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(INComponentTran.lineNbr))]
		public override Int32? LineNbr
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
		#region SplitLineNbr
		public new abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(INKitRegister.lineCntr))]
		public override Int32? SplitLineNbr
		{
			get
			{
				return this._SplitLineNbr;
			}
			set
			{
				this._SplitLineNbr = value;
			}
		}
		#endregion
		#region TranDate
		public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		[PXDBDate()]
		[PXDBDefault(typeof(INKitRegister.tranDate))]
		public override DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region InvtMult
		public new abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		[PXDBShort()]
		[PXDefault(typeof(INComponentTran.invtMult))]
		public override Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		[StockItem(Visible = false)]
		[PXDefault(typeof(INComponentTran.inventoryID))]
		public override Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region SubItemID
		public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		[SubItem(typeof(INComponentTranSplit.inventoryID))]
		[PXDefault()]
		public override Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[Site()]
		[PXDefault(typeof(INComponentTran.siteID))]
		public override Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        [LocationAvail(typeof(INComponentTranSplit.inventoryID),
                        typeof(INComponentTranSplit.subItemID),
                        typeof(INComponentTranSplit.siteID),
						typeof(Where2<Where<INComponentTranSplit.tranType, Equal<INTranType.assembly>, Or<INComponentTranSplit.tranType, Equal<INTranType.disassembly>>>, And<INComponentTranSplit.invtMult, Equal<shortMinus1>>>),
						typeof(Where2<Where<INComponentTranSplit.tranType, Equal<INTranType.assembly>, Or<INComponentTranSplit.tranType, Equal<INTranType.disassembly>>>, And<INComponentTranSplit.invtMult, Equal<short1>>>),
                        typeof(Where<False, Equal<True>>))]
        [PXRestrictor(typeof(Where<True, Equal<True>>), null, ReplaceInherited = true)]
        [PXDefault()]
        public override Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

		[INLotSerialNbr(typeof(INComponentTranSplit.inventoryID), typeof(INComponentTranSplit.subItemID), typeof(INComponentTranSplit.locationID), typeof(INComponentTran.lotSerialNbr))]
		public override String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region UOM
		public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		[INUnit(typeof(INComponentTranSplit.inventoryID), DisplayName = "UOM", Enabled = false)]
		[PXDefault]
		public override String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region Qty
		public new abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		[PXDBQuantity(typeof(INComponentTranSplit.uOM), typeof(INComponentTranSplit.baseQty), InventoryUnitType.BaseUnit)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public override Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region BaseQty
		public new abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		#endregion
		#region PlanID
		public new abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		[PXDBLong(IsImmutable = true)]
		public override Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
			}
		}
		#endregion
	}

	[PXProjection(typeof(Select<INTranSplit>), Persistent = true)]
    [PXCacheName(Messages.INKitTranSplit)]
    [Serializable]
	public partial class INKitTranSplit : IBqlTable, ILSDetail
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
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
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(INTranSplit.docType))]
		[PXDefault(typeof(INKitRegister.docType))]
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
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXDBString(3, BqlField = typeof(INTranSplit.tranType))]
		[PXDefault]
		[PXFormula(typeof(Switch<Case<Where<docType, Equal<INDocType.disassembly>>, INTranType.disassembly>, INTranType.assembly>))]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(INTranSplit.refNbr))]
		[PXDBDefault(typeof(INKitRegister.refNbr))]
		[PXParent(typeof(Select<INKitRegister,
			Where<INKitRegister.docType, Equal<Current<INKitTranSplit.docType>>,
			And<INKitRegister.refNbr, Equal<Current<INKitTranSplit.refNbr>>,
			And<INKitRegister.kitLineNbr, Equal<Current<INKitTranSplit.lineNbr>>>>>>))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(INTranSplit.lineNbr))]
		[PXDefault(typeof(INKitRegister.kitLineNbr))]
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
		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
		protected Int32? _SplitLineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(INTranSplit.splitLineNbr))]
		[PXLineNbr(typeof(INKitRegister.lineCntr))]
		public virtual Int32? SplitLineNbr
		{
			get
			{
				return this._SplitLineNbr;
			}
			set
			{
				this._SplitLineNbr = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
		[PXDBDate(BqlField = typeof(INTranSplit.tranDate))]
		[PXDBDefault(typeof(INKitRegister.tranDate))]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		protected Int16? _InvtMult;
		[PXDBShort(BqlField = typeof(INTranSplit.invtMult))]
		[PXDefault(typeof(INKitRegister.invtMult))]
		public virtual Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(Visible = false, BqlField = typeof(INTranSplit.inventoryID))]
		[PXDefault(typeof(INKitRegister.kitInventoryID))]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region IsStockItem
		public bool? IsStockItem
		{
			get
			{
				return true;
			}
			set { }
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(typeof(INKitTranSplit.inventoryID), BqlField = typeof(INTranSplit.subItemID))]
		[PXDefault()]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site(BqlField = typeof(INTranSplit.siteID))]
		[PXDefault(typeof(INKitRegister.siteID))]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[LocationAvail(typeof(INKitTranSplit.inventoryID), typeof(INKitTranSplit.subItemID), typeof(INKitTranSplit.siteID), typeof(INKitTranSplit.tranType), typeof(INKitTranSplit.invtMult), BqlField = typeof(INTranSplit.locationID))]
		[PXDefault()]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[INLotSerialNbr(typeof(INKitTranSplit.inventoryID), typeof(INKitTranSplit.subItemID), typeof(INKitTranSplit.locationID), typeof(INKitRegister.lotSerialNbr), BqlField = typeof(INTranSplit.lotSerialNbr))]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region LotSerClassID
		public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }
		protected String _LotSerClassID;
		[PXString(10, IsUnicode = true)]
		public virtual String LotSerClassID
		{
			get
			{
				return this._LotSerClassID;
			}
			set
			{
				this._LotSerClassID = value;
			}
		}
		#endregion
		#region AssignedNbr
		public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr> { }
		protected String _AssignedNbr;
		[PXString(30, IsUnicode = true)]
		public virtual String AssignedNbr
		{
			get
			{
				return this._AssignedNbr;
			}
			set
			{
				this._AssignedNbr = value;
			}
		}
		#endregion
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
		[INExpireDate(typeof(INKitTranSplit.inventoryID), BqlField = typeof(INTranSplit.expireDate))]
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool(BqlField = typeof(INTranSplit.released))]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(INKitTranSplit.inventoryID), DisplayName = "UOM", Enabled = false, BqlField = typeof(INTranSplit.uOM))]
		[PXDefault]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity(typeof(INKitTranSplit.uOM), typeof(INKitTranSplit.baseQty), InventoryUnitType.BaseUnit, BqlField = typeof(INTranSplit.qty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region BaseQty
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		protected Decimal? _BaseQty;
		[PXDBQuantity(BqlField = typeof(INTranSplit.baseQty))]
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseQty;
			}
			set
			{
				this._BaseQty = value;
			}
		}
		#endregion
		#region PlanID
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		protected Int64? _PlanID;
		[PXDBLong(BqlField = typeof(INTranSplit.planID), IsImmutable = true)]
		public virtual Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
			}
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXFormula(typeof(Selector<INKitTranSplit.locationID, INLocation.projectID>))]
		[PXInt]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXFormula(typeof(Selector<INKitTranSplit.locationID, INLocation.taskID>))]
		[PXInt]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID(BqlField = typeof(INTranSplit.createdByID))]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID(BqlField = typeof(INTranSplit.createdByScreenID))]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime(BqlField = typeof(INTranSplit.createdDateTime))]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID(BqlField = typeof(INTranSplit.lastModifiedByID))]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID(BqlField = typeof(INTranSplit.lastModifiedByScreenID))]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime(BqlField = typeof(INTranSplit.lastModifiedDateTime))]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp(BqlField = typeof(INTranSplit.Tstamp))]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion

		#region Methods
		public INKitTranSplit()
		{
		}
		public INKitTranSplit(string LotSerialNbr, string AssignedNbr, string LotSerClassID)
			: this()
		{
			this.LotSerialNbr = LotSerialNbr;
			this.AssignedNbr = AssignedNbr;
			this.LotSerClassID = LotSerClassID;
		}
		public static implicit operator INLotSerialStatus(INKitTranSplit item)
		{
			INLotSerialStatus ret = new INLotSerialStatus();
			ret.InventoryID = item.InventoryID;
			ret.SiteID = item.SiteID;
			ret.LocationID = item.LocationID;
			ret.SubItemID = item.SubItemID;
			ret.LotSerialNbr = item.LotSerialNbr;

			return ret;
		}
		#endregion

	}

	[PXPrimaryGraph(typeof(KitAssemblyEntry))]
	[PXCacheName(Messages.INKit)]
	[PXProjection(typeof(Select2<INRegister, InnerJoin<INTran, On<INRegister.FK.KitTran>>>), Persistent=true)]
	[Serializable]
	public partial class INKitRegister : IBqlTable, ILSPrimary
	{
		#region INRegister Fields

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[GL.Branch(typeof(Search<INSite.branchID, Where<INSite.siteID, Equal<Current<INKitRegister.siteID>>>>), IsDetail = false, BqlField = typeof(INRegister.branchID), Enabled = false)]
		[PXDependsOnFields(typeof(INKitRegister.tranBranchID))]
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
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(1, IsKey = true, IsFixed = true, BqlField = typeof(INRegister.docType))]
		[PXDefault(INDocType.Production)]
		[INDocType.KitListAttribute()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDependsOnFields(typeof(INKitRegister.tranDocType))]
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
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(INRegister.refNbr))]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INKitRegister.refNbr, Where<INKitRegister.docType, Equal<Optional<INKitRegister.docType>>>, OrderBy<Desc<INKitRegister.refNbr>>>), Filterable = true)]
		[AutoNumber(typeof(INKitRegister.docType), typeof(INKitRegister.tranDate),
					new string[] { INDocType.Production, INDocType.Change, INDocType.Disassembly },
					new Type[] { typeof(INSetup.kitAssemblyNumberingID), typeof(INSetup.kitAssemblyNumberingID), typeof(INSetup.kitAssemblyNumberingID)})]
		[PXDependsOnFields(typeof(INKitRegister.tranRefNbr))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
		protected String _OrigModule;
		[PXDBString(2, IsFixed = true, BqlField = typeof(INRegister.origModule))]
		[PXDefault(BatchModule.IN)]
		public virtual String OrigModule
		{
			get
			{
				return this._OrigModule;
			}
			set
			{
				this._OrigModule = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(INRegister.tranDesc))]
		[PXUIField(DisplayName = "Description")]
		[PXFormula(typeof(tranTranDesc))]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool(BqlField = typeof(INRegister.released))]
		[PXDefault(false)]
		[NoUpdateDBField(NoInsert = true)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		protected Boolean? _Hold;
		[PXDBBool(BqlField = typeof(INRegister.hold))]
		[PXDefault(typeof(INSetup.holdEntry))]
		[PXUIField(DisplayName = "Hold")]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString(1, IsFixed = true, BqlField = typeof(INRegister.status))]
		[PXDefault()]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[INDocStatus.List()]
		public virtual String Status
		{
			[PXDependsOnFields(typeof(released), typeof(hold))]
			get
			{
				return this._Status;
			}
			set
			{
				//this._Status = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
		[PXDBDate(BqlField = typeof(INRegister.tranDate))]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDependsOnFields(typeof(INKitRegister.tranTranDate))]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region TransferType
		public abstract class transferType : PX.Data.BQL.BqlString.Field<transferType> { }
		protected String _TransferType;
		[PXDBString(1, IsFixed = true, BqlField=typeof(INRegister.transferType))]
		[PXDefault(INTransferType.OneStep)]
		[INTransferType.List()]
		[PXUIField(DisplayName = "Transfer Type")]
		public virtual String TransferType
		{
			get
			{
				return this._TransferType;
			}
			set
			{
				this._TransferType = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[INOpenPeriod(typeof(INKitRegister.tranDate), 
			branchSourceType: typeof(INKitRegister.siteID), 
			branchSourceFormulaType: typeof(Selector<INKitRegister.siteID, INSite.branchID>),
			masterFinPeriodIDType: typeof(INKitRegister.tranPeriodID),
			IsHeader = true,
			BqlField = typeof(INRegister.finPeriodID))]
		[PXDefault]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDependsOnFields(typeof(INKitRegister.tranFinPeriodID))]
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
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		protected String _TranPeriodID;
		[PeriodID(BqlField = typeof(INRegister.tranPeriodID))]
		public virtual String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		protected Int32? _LineCntr;
		[PXDBInt(BqlField=typeof(INRegister.lineCntr))]
		[PXDefault(1)]
		public virtual Int32? LineCntr
		{
			get
			{
				return this._LineCntr;
			}
			set
			{
				this._LineCntr = value;
			}
		}
		#endregion
		#region TotalQty
		public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty> { }
		protected Decimal? _TotalQty;
		[PXDBQuantity(BqlField=typeof(INRegister.totalQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Qty.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXFormula(typeof(qty))]
		public virtual Decimal? TotalQty
		{
			get
			{
				return this._TotalQty;
			}
			set
			{
				this._TotalQty = value;
			}
		}
		#endregion
		#region TotalAmount
		public abstract class totalAmount : PX.Data.BQL.BqlDecimal.Field<totalAmount> { }
		protected Decimal? _TotalAmount;
		[PXDBBaseCury(BqlField = typeof(INRegister.totalAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXFormula(typeof(tranAmt))]
		public virtual Decimal? TotalAmount
		{
			get
			{
				return this._TotalAmount;
			}
			set
			{
				this._TotalAmount = value;
			}
		}
		#endregion
		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		protected Decimal? _TotalCost;
		[PXDBBaseCury(BqlField = typeof(INRegister.totalCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Cost", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXFormula(typeof(tranCost))]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion
		#region TotalCostStock
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Stock Total Cost", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? TotalCostStock { get; set; }
		public abstract class totalCostStock : PX.Data.BQL.BqlDecimal.Field<totalCostStock> { }
		#endregion
		#region TotalCostNonStock
		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Non-Stock Total Cost", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? TotalCostNonStock { get; set; }
		public abstract class totalCostNonStock : PX.Data.BQL.BqlDecimal.Field<totalCostNonStock> { }
		#endregion
		#region ControlQty
		public abstract class controlQty : PX.Data.BQL.BqlDecimal.Field<controlQty> { }
		protected Decimal? _ControlQty;
		[PXDBQuantity(BqlField = typeof(INRegister.controlQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Qty.")]
		public virtual Decimal? ControlQty
		{
			get
			{
				return this._ControlQty;
			}
			set
			{
				this._ControlQty = value;
			}
		}
		#endregion
		#region ControlAmount
		public abstract class controlAmount : PX.Data.BQL.BqlDecimal.Field<controlAmount> { }
		protected Decimal? _ControlAmount;
		[PXDBBaseCury(BqlField = typeof(INRegister.controlAmount))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Amount")]
		public virtual Decimal? ControlAmount
		{
			get
			{
				return this._ControlAmount;
			}
			set
			{
				this._ControlAmount = value;
			}
		}
		#endregion
		#region ControlCost
		public abstract class controlCost : PX.Data.BQL.BqlDecimal.Field<controlCost> { }
		protected Decimal? _ControlCost;
		[PXDBBaseCury(BqlField = typeof(INRegister.controlCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Cost")]
		public virtual Decimal? ControlCost
		{
			get
			{
				return this._ControlCost;
			}
			set
			{
				this._ControlCost = value;
			}
		}
		#endregion
		#region KitInventoryID
		public abstract class kitInventoryID : PX.Data.BQL.BqlInt.Field<kitInventoryID> { }
		protected Int32? _KitInventoryID;
		[PXDefault]
		[PXDBInt(BqlField=typeof(INRegister.kitInventoryID))]
		[PXUIField(DisplayName="Inventory ID", Visibility=PXUIVisibility.Visible)]
		[PXDimensionSelector(InventoryAttribute.DimensionName,
			typeof(Search2<InventoryItem.inventoryID,
				InnerJoin<INKitSpecHdrDistinct, On<InventoryItem.inventoryID, Equal<INKitSpecHdrDistinct.kitInventoryID>>>,
				Where<InventoryItem.stkItem, Equal<boolTrue>, And<Match<Current<AccessInfo.userName>>>>>),
			typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
		[PXDependsOnFields(typeof(INKitRegister.inventoryID))]
		public virtual Int32? KitInventoryID
		{
			get
			{
				return this._KitInventoryID;
			}
			set
			{
				this._KitInventoryID = value;
			}
		}
		#endregion
		#region KitRevisionID
		public abstract class kitRevisionID : PX.Data.BQL.BqlString.Field<kitRevisionID> { }
		protected String _KitRevisionID;
		[PXDefault()]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa", BqlField = typeof(INRegister.kitRevisionID))]
		[PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.Visible)]
		[PXRestrictor(typeof(Where<INKitSpecHdr.isActive, Equal<True>>), Messages.InactiveKitRevision, typeof(INKitSpecHdr.revisionID))]
		[PXSelector(typeof(Search<INKitSpecHdr.revisionID,
			Where<INKitSpecHdr.kitInventoryID, Equal<Current<INKitRegister.kitInventoryID>>>>), typeof(INKitSpecHdr.kitInventoryID), typeof(INKitSpecHdr.descr))]
		public virtual String KitRevisionID
		{
			get
			{
				return this._KitRevisionID;
			}
			set
			{
				this._KitRevisionID = value;
			}
		}
		#endregion
		#region KitLineNbr
		public abstract class kitLineNbr : PX.Data.BQL.BqlInt.Field<kitLineNbr> { }
		protected Int32? _KitLineNbr;
		[PXDBInt(BqlField = typeof(INRegister.kitLineNbr))]
		[PXDefault(0)]
		[PXDependsOnFields(typeof(INKitRegister.lineNbr))]
		public virtual Int32? KitLineNbr
		{
			get
			{
				return this._KitLineNbr;
			}
			set
			{
				this._KitLineNbr = value;
			}
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true, BqlField = typeof(INRegister.batchNbr))]
		[PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleIN>>>))]
		public virtual String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
        #region LotSerTrack
        public abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
        protected String _LotSerTrack;
        [PXString(1)]
		[PXFormula(typeof(Selector<INKitRegister.kitInventoryID, Selector<InventoryItem.lotSerClassID, INLotSerClass.lotSerTrack>>))]
        public virtual String LotSerTrack
        {
            get
            {
                return this._LotSerTrack;
            }
            set
            {
                this._LotSerTrack = value;
            }
        }
        #endregion       
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID(BqlField=typeof(INRegister.createdByID))]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID(BqlField = typeof(INRegister.createdByScreenID))]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime(BqlField = typeof(INRegister.createdDateTime))]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false)]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID(BqlField = typeof(INRegister.lastModifiedByID))]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID(BqlField = typeof(INRegister.lastModifiedByScreenID))]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime(BqlField = typeof(INRegister.lastModifiedDateTime))]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXSearchable(SM.SearchCategory.IN, Messages.SearchableTitleKit, new Type[] { typeof(INKitRegister.docType), typeof(INKitRegister.refNbr), typeof(INKitRegister.tranDocType), typeof(INKitRegister.tranType), typeof(INKitRegister.tranRefNbr) },//excessive fields are required because of _DocType that is referensed in 2 property get/set
			new Type[] { typeof(INKitRegister.tranDesc), typeof(INKitRegister.tranTranDesc) },
			NumberFields = new Type[] { typeof(INKitRegister.refNbr) },
			Line1Format = "{1}{2}{3:d}{4}", Line1Fields = new Type[] { typeof(INKitRegister.kitInventoryID), typeof(InventoryItem.inventoryCD), typeof(INKitRegister.kitRevisionID), typeof(INKitRegister.tranDate), typeof(INKitRegister.status) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(INKitRegister.tranTranDesc)},
			SelectForFastIndexing = typeof(Select<INKitRegister, Where<INKitRegister.docType, Equal<INDocType.production>, And<INKitRegister.docType, Equal<INDocType.disassembly>>>>)
		)]
		[PXNote(BqlField = typeof(INRegister.noteID))]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp(BqlField = typeof(INRegister.Tstamp))]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion

		#endregion
		
		#region INTran Fields

		#region TranDocType
		public abstract class tranDocType : PX.Data.BQL.BqlString.Field<tranDocType> { }
		[PXDBString(1, IsFixed = true, BqlField=typeof(INTran.docType) )]
		[PXDefault]
		[PXRestriction()]
		public virtual String TranDocType
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
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXDBString(3, IsFixed = true, BqlField = typeof(INTran.tranType))]
		[PXDefault]
		[PXFormula(typeof(Switch<Case<Where<tranDocType, Equal<INDocType.disassembly>>, INTranType.disassembly>, INTranType.assembly>))]
		[INTranType.List()]
		[PXUIField(DisplayName = "Tran. Type")]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region TranRefNbr
		public abstract class tranRefNbr : PX.Data.BQL.BqlString.Field<tranRefNbr> { }
		[PXDBString(15, IsUnicode = true, BqlField = typeof(INTran.refNbr))]
		[PXDefault]
		[PXRestriction()]
		public virtual String TranRefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region TranBranchID
		public abstract class tranBranchID : PX.Data.BQL.BqlInt.Field<tranBranchID> { }
		[PXDBInt(BqlField = typeof(INTran.branchID))]
		public virtual Int32? TranBranchID
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(BqlField = typeof(INTran.lineNbr))]
		[PXDefault()]
		[PXRestriction()]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._KitLineNbr;
			}
			set
			{
				this._KitLineNbr = value;
			}
		}
		#endregion
		#region AssyType
		public abstract class assyType : PX.Data.BQL.BqlString.Field<assyType> { }
		protected String _AssyType;
		[PXDefault(INAssyType.KitTran)]
		[PXDBString(1, IsFixed = true, BqlField = typeof(INTran.assyType))]
		public virtual String AssyType
		{
			get
			{
				return this._AssyType;
			}
			set
			{
				this._AssyType = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PX.Objects.PM.ProjectDefault]
		[PXDBInt(BqlField = typeof(INTran.projectID))]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDBInt(BqlField = typeof(INTran.taskID))]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region TranTranDate
		public abstract class tranTranDate : PX.Data.BQL.BqlDateTime.Field<tranTranDate> { }
		[PXDBDate(BqlField = typeof(INTran.tranDate))]
		public virtual DateTime? TranTranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		protected Int16? _InvtMult;
		[PXDBShort(BqlField = typeof(INTran.invtMult))]
		[PXDefault((short)1)]
		public virtual Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		/// <summary>
		/// This field declaration is for population of <see cref="INTran.InventoryID"/> within projection
		/// </summary>
		[PXDBInt(BqlField=typeof(INTran.inventoryID))]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._KitInventoryID;
			}
			set
			{
				this._KitInventoryID = value;
			}
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[IN.SubItem(typeof(INKitRegister.kitInventoryID), BqlField=typeof(INTran.subItemID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDefault]
		[IN.SiteAvail(typeof(INKitRegister.kitInventoryID), typeof(INKitRegister.subItemID), BqlField=typeof(INTran.siteID))]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<AccessInfo.branchID>>>))]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
        [LocationAvail(typeof(INKitRegister.kitInventoryID),
                      typeof(INKitRegister.subItemID),
                      typeof(INKitRegister.siteID),
					  typeof(Where<False>),
					  typeof(Where2<Where<INKitRegister.tranType, Equal<INTranType.assembly>, Or<INKitRegister.tranType, Equal<INTranType.disassembly>>>, And<INKitRegister.invtMult, Equal<short1>>>),
					  typeof(Where<False, Equal<True>>), BqlField = typeof(INTran.locationID))]
        public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault( typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<INKitRegister.kitInventoryID>>>>))]
		[INUnit(typeof(INKitRegister.kitInventoryID), BqlField=typeof(INTran.uOM))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity(typeof(INKitRegister.uOM), typeof(INKitRegister.baseQty), InventoryUnitType.BaseUnit, BqlField=typeof(INTran.qty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region BaseQty
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		protected Decimal? _BaseQty;
		[PXDBQuantity(BqlField=typeof(INTran.baseQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseQty;
			}
			set
			{
				this._BaseQty = value;
			}
		}
		#endregion
		#region UnassignedQty
		public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }
		protected Decimal? _UnassignedQty;
		[PXDBDecimal(6, BqlField = typeof(INTran.unassignedQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnassignedQty
		{
			get
			{
				return this._UnassignedQty;
			}
			set
			{
				this._UnassignedQty = value;
			}
		}
		#endregion
		#region TranReleased
		public abstract class tranReleased : PX.Data.BQL.BqlBool.Field<tranReleased> { }
		protected Boolean? _TranReleased;
		[PXDBBool(BqlField=typeof(INTran.released))]
		[PXDefault(false)]
		public virtual Boolean? TranReleased
		{
			get
			{
				return this._TranReleased;
			}
			set
			{
				this._TranReleased = value;
			}
		}
		#endregion
		#region TranFinPeriodID
		public abstract class tranFinPeriodID : PX.Data.BQL.BqlString.Field<tranFinPeriodID> { }
		[PXDefault]
		[FinPeriodID(
			sourceType: typeof(INKitRegister.tranTranDate),
			branchSourceType: typeof(INKitRegister.tranBranchID),
			masterFinPeriodIDType: typeof(INKitRegister.tranTranPeriodID),
			IsHeader = true,
			BqlField = typeof(INTran.finPeriodID))]
		public virtual String TranFinPeriodID
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
		#region TranTranPeriodID
		public abstract class tranTranPeriodID : PX.Data.BQL.BqlString.Field<tranTranPeriodID> { }
		[FinPeriodID(BqlField=typeof(INTran.tranPeriodID))]
		public virtual String TranTranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		

		#region UnitPrice
		public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }
		protected Decimal? _UnitPrice;
		[PXDBPriceCost(BqlField=typeof(INTran.unitPrice))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Price")]
		public virtual Decimal? UnitPrice
		{
			get
			{
				return this._UnitPrice;
			}
			set
			{
				this._UnitPrice = value;
			}
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
		protected Decimal? _TranAmt;
		[PXDBBaseCury(BqlField = typeof(INTran.tranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ext. Price")]
		public virtual Decimal? TranAmt
		{
			get
			{
				return this._TranAmt;
			}
			set
			{
				this._TranAmt = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXDBPriceCost(BqlField = typeof(INTran.unitCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unit Cost")]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
		#region TranCost
		public abstract class tranCost : PX.Data.BQL.BqlDecimal.Field<tranCost> { }
		protected Decimal? _TranCost;
		[PXDBBaseCury(BqlField = typeof(INTran.tranCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ext. Cost")]
		[PXFormula(typeof(Add<totalCostStock, totalCostNonStock>))]
		public virtual Decimal? TranCost
		{
			get
			{
				return this._TranCost;
			}
			set
			{
				this._TranCost = value;
			}
		}
		#endregion
		#region TranTranDesc
		public abstract class tranTranDesc : PX.Data.BQL.BqlString.Field<tranTranDesc> { }
		protected String _TranTranDesc;
		[PXDBString(60, IsUnicode = true,BqlField = typeof(INTran.tranDesc))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Selector<INKitRegister.kitInventoryID, InventoryItem.descr>))]
		public virtual String TranTranDesc
		{
			get
			{
				return this._TranTranDesc;
			}
			set
			{
				this._TranTranDesc = value;
			}
		}
		#endregion
		#region ReasonCode
		public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
		protected String _ReasonCode;
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true, BqlField = typeof(INTran.reasonCode))]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<Optional<INKitRegister.docType>>>>))]
		[PXUIField(DisplayName = "Reason Code")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String ReasonCode
		{
			get
			{
				return this._ReasonCode;
			}
			set
			{
				this._ReasonCode = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[INLotSerialNbr(typeof(INKitRegister.kitInventoryID), typeof(INKitRegister.subItemID), typeof(INKitRegister.locationID), PersistingCheck = PXPersistingCheck.Nothing, BqlField = typeof(INTran.lotSerialNbr))]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
		[INExpireDate(typeof(INKitRegister.kitInventoryID), PersistingCheck = PXPersistingCheck.Nothing, BqlField=typeof(INTran.expireDate))]
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion
		#region UpdateShippedNotInvoiced
		public abstract class updateShippedNotInvoiced : PX.Data.BQL.BqlBool.Field<updateShippedNotInvoiced> { }
		protected Boolean? _UpdateShippedNotInvoiced;
		[PXDBBool(BqlField = typeof(INTran.updateShippedNotInvoiced))]
		[PXDefault(false)]
		public virtual Boolean? UpdateShippedNotInvoiced
		{
			get
			{
				return this._UpdateShippedNotInvoiced;
			}
			set
			{
				this._UpdateShippedNotInvoiced = value;
			}
		}
		#endregion
		#region HasMixedProjectTasks
		public abstract class hasMixedProjectTasks : PX.Data.BQL.BqlBool.Field<hasMixedProjectTasks> { }
		protected bool? _HasMixedProjectTasks;
		/// <summary>
		/// Returns true if the splits associated with the line has mixed ProjectTask values.
		/// This field is used to validate the record on persist. 
		/// </summary>
		[PXBool]
		[PXFormula(typeof(False))]
		public virtual bool? HasMixedProjectTasks
		{
			get
			{
				return _HasMixedProjectTasks;
			}
			set
			{
				_HasMixedProjectTasks = value;
			}
		}
		#endregion

		#region TranCreatedByID
		public abstract class trancreatedByID : PX.Data.BQL.BqlGuid.Field<trancreatedByID> { }
		protected Guid? _TranCreatedByID;
		[PXDBCreatedByID(BqlField=typeof(INTran.createdByID))]
		public virtual Guid? TranCreatedByID
		{
			get
			{
				return this._TranCreatedByID;
			}
			set
			{
				this._TranCreatedByID = value;
			}
		}
		#endregion
		#region TranCreatedByScreenID
		public abstract class trancreatedByScreenID : PX.Data.BQL.BqlString.Field<trancreatedByScreenID> { }
		protected String _TranCreatedByScreenID;
		[PXDBCreatedByScreenID(BqlField = typeof(INTran.createdByScreenID))]
		public virtual String TranCreatedByScreenID
		{
			get
			{
				return this._TranCreatedByScreenID;
			}
			set
			{
				this._TranCreatedByScreenID = value;
			}
		}
		#endregion
		#region TranCreatedDateTime
		public abstract class trancreatedDateTime : PX.Data.BQL.BqlDateTime.Field<trancreatedDateTime> { }
		protected DateTime? _TranCreatedDateTime;
		[PXDBCreatedDateTime(BqlField = typeof(INTran.createdDateTime))]
		public virtual DateTime? TranCreatedDateTime
		{
			get
			{
				return this._TranCreatedDateTime;
			}
			set
			{
				this._TranCreatedDateTime = value;
			}
		}
		#endregion
		#region TranLastModifiedByID
		public abstract class tranlastModifiedByID : PX.Data.BQL.BqlGuid.Field<tranlastModifiedByID> { }
		protected Guid? _TranLastModifiedByID;
		[PXDBLastModifiedByID(BqlField = typeof(INTran.lastModifiedByID))]
		public virtual Guid? TranLastModifiedByID
		{
			get
			{
				return this._TranLastModifiedByID;
			}
			set
			{
				this._TranLastModifiedByID = value;
			}
		}
		#endregion
		#region TranLastModifiedByScreenID
		public abstract class tranlastModifiedByScreenID : PX.Data.BQL.BqlString.Field<tranlastModifiedByScreenID> { }
		protected String _TranLastModifiedByScreenID;
		[PXDBLastModifiedByScreenID(BqlField = typeof(INTran.lastModifiedByScreenID))]
		public virtual String TranLastModifiedByScreenID
		{
			get
			{
				return this._TranLastModifiedByScreenID;
			}
			set
			{
				this._TranLastModifiedByScreenID = value;
			}
		}
		#endregion
		#region TranLastModifiedDateTime
		public abstract class tranlastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<tranlastModifiedDateTime> { }
		protected DateTime? _TranLastModifiedDateTime;
		[PXDBLastModifiedDateTime(BqlField = typeof(INTran.lastModifiedDateTime))]
		public virtual DateTime? TranLastModifiedDateTime
		{
			get
			{
				return this._TranLastModifiedDateTime;
			}
			set
			{
				this._TranLastModifiedDateTime = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class tranTstamp : PX.Data.BQL.BqlByteArray.Field<tranTstamp> { }
		protected Byte[] _Trantstamp;
		[PXDBTimestamp(BqlField = typeof(INTran.Tstamp))]
		public virtual Byte[] Trantstamp
		{
			get
			{
				return this._Trantstamp;
			}
			set
			{
				this._Trantstamp = value;
			}
		}
		#endregion


		#endregion

		#region Methods
		protected virtual void SetStatus()
		{
			if (this._Hold != null && (bool)this._Hold)
			{
				this._Status = INDocStatus.Hold;
			}
			else if (this._Released != null && this._Released == false)
			{
				this._Status = INDocStatus.Balanced;
			}
			else
			{
				this._Status = INDocStatus.Released;
			}
		}

		//This is a bad idea... but still
		public static implicit operator INKitTranSplit(INKitRegister item)
		{
			INKitTranSplit ret = new INKitTranSplit();
			ret.DocType = item.DocType;
			ret.TranType = item.TranType;
			ret.RefNbr = item.RefNbr;
			ret.LineNbr = item.LineNbr;
			ret.SplitLineNbr = (short)1;
			ret.InventoryID = item.KitInventoryID;
			ret.SiteID = item.SiteID;
			ret.SubItemID = item.SubItemID;
			ret.LocationID = item.LocationID;
			ret.LotSerialNbr = item.LotSerialNbr;
			ret.ExpireDate = item.ExpireDate;
			ret.Qty = item.Qty;
			ret.UOM = item.UOM;
			ret.TranDate = item.TranDate;
			ret.BaseQty = item.BaseQty;
			ret.InvtMult = item.InvtMult;
			ret.Released = item.Released;

			return ret;
		}

		//This is a bad idea... but still
		//public static implicit operator INKitRegister(INComponentTranSplit item)
		//{
		//    INKitRegister ret = new INKitRegister();
		//    ret.DocType = item.DocType;
		//    ret.TranType = item.TranType;
		//    ret.RefNbr = item.RefNbr;
		//    ret.LineNbr = item.LineNbr;
		//    ret.InventoryID = item.InventoryID;
		//    ret.SiteID = item.SiteID;
		//    ret.SubItemID = item.SubItemID;
		//    ret.LocationID = item.LocationID;
		//    ret.LotSerialNbr = item.LotSerialNbr;
		//    ret.Qty = item.Qty;
		//    ret.UOM = item.UOM;
		//    ret.TranDate = item.TranDate;
		//    ret.BaseQty = item.BaseQty;
		//    ret.InvtMult = item.InvtMult;
		//    ret.Released = item.Released;

		//    return ret;
		//}

		public static explicit operator INRegister(INKitRegister inKitDoc)
		{
			INRegister indoc = new INRegister
			                   	{
			                   		BatchNbr = inKitDoc.BatchNbr,
			                   		ControlAmount = inKitDoc.ControlAmount,
			                   		ControlCost = inKitDoc.ControlCost,
			                   		ControlQty = inKitDoc.ControlQty,
			                   		CreatedByID = inKitDoc.CreatedByID,
			                   		CreatedByScreenID = inKitDoc.CreatedByScreenID,
			                   		CreatedDateTime = inKitDoc.CreatedDateTime,
			                   		DocType = inKitDoc.DocType,
			                   		FinPeriodID = inKitDoc.FinPeriodID,
			                   		Hold = inKitDoc.Hold,
			                   		KitInventoryID = inKitDoc.KitInventoryID,
			                   		KitLineNbr = inKitDoc.KitLineNbr,
			                   		KitRevisionID = inKitDoc.KitRevisionID,
			                   		LastModifiedByID = inKitDoc.LastModifiedByID,
			                   		LastModifiedByScreenID = inKitDoc.LastModifiedByScreenID,
			                   		LastModifiedDateTime = inKitDoc.LastModifiedDateTime,
			                   		LineCntr = inKitDoc.LineCntr,
			                   		NoteID = inKitDoc.NoteID,
			                   		OrigModule = inKitDoc.OrigModule,
			                   		RefNbr = inKitDoc.RefNbr,
			                   		Released = inKitDoc.Released,
			                   		SiteID = inKitDoc.SiteID,
			                   		Status = inKitDoc.Status,
			                   		TotalAmount = inKitDoc.TotalAmount,
			                   		TotalCost = inKitDoc.TotalCost,
			                   		TotalQty = inKitDoc.TotalQty,
			                   		TranDate = inKitDoc.TranDate,
			                   		TranDesc = inKitDoc.TranDesc,
			                   		TranPeriodID = inKitDoc.TranPeriodID,
			                   		TransferType = inKitDoc.TransferType,
			                   		tstamp = inKitDoc.tstamp
			                   	};
			return indoc;
		}

		#endregion
	}

	[PXProjection(typeof(Select4<INKitSpecHdr, Where<INKitSpecHdr.isActive, Equal<boolTrue>>, Aggregate<GroupBy<INKitSpecHdr.kitInventoryID>>>))]
    [Serializable]
    [PXHidden]
	public partial class INKitSpecHdrDistinct : IBqlTable
	{
		#region KitInventoryID
		public abstract class kitInventoryID : PX.Data.BQL.BqlInt.Field<kitInventoryID> { }
		protected Int32? _KitInventoryID;
		[PXDBInt(BqlField = typeof(INKitSpecHdr.kitInventoryID))]
		public virtual Int32? KitInventoryID
		{
			get
			{
				return this._KitInventoryID;
			}
			set
			{
				this._KitInventoryID = value;
			}
		}
		#endregion
	} 

	#endregion

	#region Attribute Extensions
	
	public class INKitTranSplitPlanIDAttribute : INTranSplitPlanIDAttribute
	{
		#region Ctor
		public INKitTranSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentTransferType)
			: base(ParentNoteID, ParentHoldEntry, ParentTransferType)
		{
			_ParentTransferType = ParentTransferType;
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (!sender.Graph.IsImport && !sender.Graph.UnattendedMode)
			{
				//plan source should go before plan
				//to bind errors from INItemPlan.SubItemID -> SOLine.SubItemID or SOLineSplit.SubItemID
				if (!sender.Graph.Views.Caches.Contains(_ItemType))
					sender.Graph.Views.Caches.Add(_ItemType);
				if (sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
					sender.Graph.Views.Caches.Remove(typeof(INItemPlan));
				sender.Graph.Views.Caches.Add(typeof(INItemPlan));
			}

			if (sender.Graph.Views.Caches.Contains(typeof(LotSerialStatus)))
				sender.Graph.Views.Caches.Remove(typeof(LotSerialStatus));
			sender.Graph.Views.Caches.Add(typeof(LotSerialStatus));
			if (sender.Graph.Views.Caches.Contains(typeof(ItemLotSerial)))
				sender.Graph.Views.Caches.Remove(typeof(ItemLotSerial));
			sender.Graph.Views.Caches.Add(typeof(ItemLotSerial));
			if (sender.Graph.Views.Caches.Contains(typeof(SiteLotSerial)))
				sender.Graph.Views.Caches.Remove(typeof(SiteLotSerial));
			sender.Graph.Views.Caches.Add(typeof(SiteLotSerial));
			if (sender.Graph.Views.Caches.Contains(typeof(LocationStatus)))
				sender.Graph.Views.Caches.Remove(typeof(LocationStatus));
			sender.Graph.Views.Caches.Add(typeof(LocationStatus));
			if (sender.Graph.Views.Caches.Contains(typeof(SiteStatus)))
				sender.Graph.Views.Caches.Remove(typeof(SiteStatus));
			sender.Graph.Views.Caches.Add(typeof(SiteStatus));

		}
		#endregion
		#region Implementation
		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Parent_RowUpdated(sender, e);

			if (!sender.ObjectsEqual<INKitRegister.hold>(e.Row, e.OldRow))
			{
				PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];

				//preload plans in cache first
				PXResultset<INItemPlan> not_inserted = PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<INKitRegister.noteID>>>>.Select(sender.Graph);

				foreach (INTranSplit split in PXSelect<INTranSplit, Where<INTranSplit.docType, Equal<Current<INKitRegister.docType>>, And<INTranSplit.refNbr, Equal<Current<INKitRegister.refNbr>>>>>.Select(sender.Graph))
				{
					foreach (INItemPlan plan in plancache.Cached)
					{
						if (object.Equals(plan.PlanID, split.PlanID) && plancache.GetStatus(plan) != PXEntryStatus.Deleted && plancache.GetStatus(plan) != PXEntryStatus.InsertedDeleted)
						{
							plan.Hold = (bool?)sender.GetValue<INKitRegister.hold>(e.Row);
						}
					}
				}
			}
		}

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			if ( orig_Row is INKitTranSplit )
			{
				INKitTranSplit row = (INKitTranSplit) orig_Row;

				INTranSplit properties = new INTranSplit();
				properties.InventoryID = row.InventoryID;
				properties.SubItemID = row.SubItemID;
				properties.SiteID = row.SiteID;
				properties.LocationID = row.LocationID;
				properties.LotSerialNbr = row.LotSerialNbr;
				properties.AssignedNbr = row.AssignedNbr;
				properties.BaseQty = row.BaseQty;
				properties.TranType = row.TranType;
				properties.InvtMult = row.InvtMult;

				return base.DefaultValues(sender, plan_Row, properties);

			}
			else
			{
				return base.DefaultValues(sender, plan_Row, orig_Row);
			}
		}
						
		#endregion
	}
	#endregion

	#region LSSelect Extensions

	public class LSINComponentTran : LSSelect<INComponentTran, INComponentTranSplit, 
		Where<INComponentTranSplit.docType, Equal<Current<INKitRegister.docType>>,
		And<INComponentTranSplit.refNbr, Equal<Current<INKitRegister.refNbr>>>>>
	{
		#region Ctor
		public LSINComponentTran(PXGraph graph)
			: base(graph)
		{
			graph.FieldDefaulting.AddHandler<INComponentTranSplit.subItemID>(INTranSplit_SubItemID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INComponentTranSplit.locationID>(INTranSplit_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INComponentTranSplit.invtMult>(INTranSplit_InvtMult_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INComponentTranSplit.lotSerialNbr>(INTranSplit_LotSerialNbr_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INComponentTranSplit.qty>(INTranSplit_Qty_FieldVerifying);
			graph.FieldVerifying.AddHandler<INComponentTran.qty>(INTran_Qty_FieldVerifying);
			graph.RowUpdated.AddHandler<INKitRegister>(INKitRegister_RowUpdated);
            graph.RowUpdated.AddHandler<INComponentTran>(INComponentTran_RowUpdated);
		}
		#endregion

		#region Implementation

		protected override void SetEditMode()
		{
			if (!Initialized)
			{
				PXUIFieldAttribute.SetEnabled(MasterCache, null, true);
				PXUIFieldAttribute.SetEnabled(DetailCache, null, true);
				PXUIFieldAttribute.SetEnabled(DetailCache, null, "UOM", false);
				Initialized = true;
			}
		}

		protected virtual void INKitRegister_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<INKitRegister.hold>(e.Row, e.OldRow) && (bool?)sender.GetValue<INKitRegister.hold>(e.Row) == false)
            {
                PXCache cache = sender.Graph.Caches[typeof(INComponentTran)];

				foreach (INComponentTran item in PXParentAttribute.SelectSiblings(cache, null, typeof(INKitRegister)))
				{
					if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && (item.UnassignedQty >= 0.0000005m || item.UnassignedQty <= -0.0000005m))
					{
						cache.RaiseExceptionHandling<INComponentTran.qty>(item, item.Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned));

						cache.MarkUpdated(item);
					}
				}
			}
		}
       
		protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{ 
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				PXCache cache = sender.Graph.Caches[typeof(INKitRegister)];
				object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(INKitRegister)) ?? cache.Current;

				bool? OnHold = (bool?)cache.GetValue<INKitRegister.hold>(doc);

                if (OnHold == false && Math.Abs((decimal)((INComponentTran)e.Row).BaseQty) >= 0.0000005m && (((INComponentTran)e.Row).UnassignedQty >= 0.0000005m || ((INComponentTran)e.Row).UnassignedQty <= -0.0000005m))
				{
					if (sender.RaiseExceptionHandling<INComponentTran.qty>(e.Row, ((INComponentTran)e.Row).Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned)))
					{
						throw new PXRowPersistingException(typeof(INComponentTran.qty).Name, ((INComponentTran)e.Row).Qty, Messages.BinLotSerialNotAssigned);
					}
				}
			}
			base.Master_RowPersisting(sender, e);
		}

        protected virtual void INComponentTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            INComponentTran row = (INComponentTran)e.Row;
            if (row == null) return;
            if (!PXLongOperation.Exists(sender.Graph.UID))
            {
                IQtyAllocated availability = AvailabilityFetch(sender, (INComponentTran)e.Row, AvailabilityFetchMode.ExcludeCurrent);

                if (availability != null)
                {
                    PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INComponentTran)e.Row).InventoryID);

                    availability.QtyOnHand = INUnitAttribute.ConvertFromBase<INComponentTran.inventoryID, INComponentTran.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
                    availability.QtyAvail = INUnitAttribute.ConvertFromBase<INComponentTran.inventoryID, INComponentTran.uOM>(sender, e.Row, (decimal)availability.QtyAvail, INPrecision.QUANTITY);
                    availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<INComponentTran.inventoryID, INComponentTran.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
                    availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<INComponentTran.inventoryID, INComponentTran.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);

                    AvailabilityCheck(sender, (INComponentTran)e.Row, availability);
                }
            }
        }
        protected override DateTime? ExpireDateByLot(PXCache sender, ILSMaster item, ILSMaster master)
        {
            if (master != null && master.InvtMult > 0)
            {
                item.ExpireDate=null;
                return base.ExpireDateByLot(sender, item, null);
            }
            else return base.ExpireDateByLot(sender, item, master);           
        }

        public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (!PXLongOperation.Exists(sender.Graph.UID))
            {
                IQtyAllocated availability = AvailabilityFetch(sender, (INComponentTran)e.Row, ((INComponentTran)e.Row)?.Released == true ? AvailabilityFetchMode.None : AvailabilityFetchMode.ExcludeCurrent);

                if (availability != null)
                {
                    PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INComponentTran)e.Row).InventoryID);

                    availability.QtyOnHand = INUnitAttribute.ConvertFromBase<INComponentTran.inventoryID, INComponentTran.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
                    availability.QtyAvail = INUnitAttribute.ConvertFromBase<INComponentTran.inventoryID, INComponentTran.uOM>(sender, e.Row, (decimal)availability.QtyAvail, INPrecision.QUANTITY);
                    availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<INComponentTran.inventoryID, INComponentTran.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
                    availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<INComponentTran.inventoryID, INComponentTran.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);
					availability.QtyActual = INUnitAttribute.ConvertFromBase<INComponentTran.inventoryID, INComponentTran.uOM>(sender, e.Row, (decimal)availability.QtyActual, INPrecision.QUANTITY);

					e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(Messages.Availability_ActualInfo,
						sender.GetValue<INTran.uOM>(e.Row),
						FormatQty(availability.QtyOnHand),
						FormatQty(availability.QtyAvail),
						FormatQty(availability.QtyHardAvail),
						FormatQty(availability.QtyActual));
                }
                else
                {
                    e.ReturnValue = string.Empty;
                }
            }
            else
            {
                e.ReturnValue = string.Empty;
            }

            base.Availability_FieldSelecting(sender, e);
        }

		protected override IEnumerable<PXExceptionInfo> GetAvailabilityCheckErrors(PXCache sender, ILSMaster Row, IQtyAllocated availability)
		{
			foreach (var errorInfo in base.GetAvailabilityCheckErrors(sender, Row, availability))
				yield return errorInfo;
			if (Row.InvtMult == -1 && Row.BaseQty > 0m)
			{
				if (availability != null && availability.QtyAvail < Row.Qty)
				{
					if (availability is LotSerialStatus)
						yield return new PXExceptionInfo(Messages.StatusCheck_QtyLotSerialNegative);
					else if (availability is LocationStatus)
						yield return new PXExceptionInfo(Messages.StatusCheck_QtyLocationNegative);
					else if (availability is SiteStatus)
						yield return new PXExceptionInfo(Messages.StatusCheck_QtyNegative);
				}
			}
		}

		public override INComponentTranSplit Convert(INComponentTran item)
		{
			using (InvtMultScope<INComponentTran> ms = new InvtMultScope<INComponentTran>(item))
			{
				INComponentTranSplit ret = item;
				//baseqty will be overriden in all cases but AvailabilityFetch
				ret.BaseQty = item.BaseQty - item.UnassignedQty;
				return ret;
			}
		}

		public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
			{
				throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
			}
		}

		public virtual void INTranSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				if (((INComponentTranSplit)e.Row).BaseQty != 0m && ((INComponentTranSplit)e.Row).LocationID == null)
				{
					ThrowFieldIsEmpty<INComponentTranSplit.locationID>(sender, e.Row);
				}
			}
		}

		public virtual void INTranSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INComponentTran)];
			if (cache.Current != null && (e.Row == null || ((INComponentTran)cache.Current).LineNbr == ((INComponentTranSplit)e.Row).LineNbr))
			{
				e.NewValue = ((INComponentTran)cache.Current).SubItemID;
				e.Cancel = true;
			}
		}

		public virtual void INTranSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INComponentTran)];
			if (cache.Current != null && (e.Row == null || ((INComponentTran)cache.Current).LineNbr == ((INComponentTranSplit)e.Row).LineNbr))
			{
				e.NewValue = ((INComponentTran)cache.Current).LocationID;
				e.Cancel = true;
			}
		}

		public virtual void INTranSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INComponentTran)];
			if (cache.Current != null && (e.Row == null || ((INComponentTran)cache.Current).LineNbr == ((INComponentTranSplit)e.Row).LineNbr))
			{
				using (InvtMultScope<INComponentTran> ms = new InvtMultScope<INComponentTran>((INComponentTran)cache.Current))
				{
					e.NewValue = ((INComponentTran)cache.Current).InvtMult;
					e.Cancel = true;
				}
			}
		}

		public virtual void INTranSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INComponentTranSplit)e.Row).InventoryID);

			if (item != null)
			{
				object InvtMult = ((INComponentTranSplit)e.Row).InvtMult;
				if (InvtMult == null)
				{
					sender.RaiseFieldDefaulting<INComponentTranSplit.invtMult>(e.Row, out InvtMult);
				}

				object TranType = ((INComponentTranSplit)e.Row).TranType;
				if (TranType == null)
				{
					sender.RaiseFieldDefaulting<INComponentTranSplit.tranType>(e.Row, out TranType);
				}

				INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(item, (string)TranType, (short?)InvtMult);
				if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
				{
					ILotSerNumVal lotSerNum = ReadLotSerNumVal(sender, item);
					foreach (INComponentTranSplit lssplit in INLotSerialNbrAttribute.CreateNumbers<INComponentTranSplit>(sender, item, lotSerNum, mode, 1m))
					{
						e.NewValue = lssplit.LotSerialNbr;
						e.Cancel = true;
					}
				}
				//otherwise default via attribute
			}
		}

		public virtual void INTranSplit_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INComponentTranSplit)e.Row).InventoryID);

			if (item != null && INLotSerialNbrAttribute.IsTrackSerial(item, ((INComponentTranSplit)e.Row).TranType, ((INComponentTranSplit)e.Row).InvtMult))
			{
				if (e.NewValue != null && e.NewValue is decimal && (decimal)e.NewValue != 0m && (decimal)e.NewValue != 1m)
				{
					e.NewValue = 1m;
				}
			}
		}

		public virtual void INTran_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal?)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, PXErrorLevel.Error, (int)0);
			}
		}

		protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo ei)
		{
			if (row is INComponentTran)
			{
				sender.RaiseExceptionHandling<INComponentTran.qty>(row, null, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<INComponentTran.inventoryID>(row), sender.GetStateExt<INComponentTran.subItemID>(row), sender.GetStateExt<INComponentTran.siteID>(row), sender.GetStateExt<INComponentTran.locationID>(row), sender.GetValue<INComponentTran.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<INComponentTranSplit.qty>(row, null, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<INComponentTranSplit.inventoryID>(row), sender.GetStateExt<INComponentTranSplit.subItemID>(row), sender.GetStateExt<INComponentTranSplit.siteID>(row), sender.GetStateExt<INComponentTranSplit.locationID>(row), sender.GetValue<INComponentTranSplit.lotSerialNbr>(row)));
			}
		}
				
				
		#endregion
	}

	public class LSINComponentMasterTran : LSSelect<INKitRegister, INKitTranSplit, 
		Where<INKitTranSplit.docType, Equal<Current<INKitRegister.docType>>,
	    And<INKitTranSplit.refNbr, Equal<Current<INKitRegister.refNbr>>>>>
	{
		#region Ctor
		public LSINComponentMasterTran(PXGraph graph)
			: base(graph)
		{
			graph.FieldDefaulting.AddHandler<INKitTranSplit.subItemID>(INTranSplit_SubItemID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INKitTranSplit.locationID>(INTranSplit_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INKitTranSplit.invtMult>(INTranSplit_InvtMult_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INKitTranSplit.lotSerialNbr>(INTranSplit_LotSerialNbr_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INKitTranSplit.qty>(INTranSplit_Qty_FieldVerifying);
            graph.FieldVerifying.AddHandler<INKitRegister.qty>(INTran_Qty_FieldVerifying); 
            graph.RowUpdated.AddHandler<INKitRegister>(INKitRegister_RowUpdated);
		}
		#endregion

		#region Implementation
		public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (!PXLongOperation.Exists(sender.Graph.UID))
			{
				IQtyAllocated availability = AvailabilityFetch(sender, (INKitRegister)e.Row, ((INKitRegister)e.Row)?.Released == true ? AvailabilityFetchMode.None : AvailabilityFetchMode.ExcludeCurrent);

				if (availability != null)
				{
					PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INKitRegister)e.Row).KitInventoryID);

					availability.QtyOnHand = INUnitAttribute.ConvertFromBase<INKitRegister.kitInventoryID, INKitRegister.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
					availability.QtyAvail = INUnitAttribute.ConvertFromBase<INKitRegister.kitInventoryID, INKitRegister.uOM>(sender, e.Row, (decimal)availability.QtyAvail, INPrecision.QUANTITY);
					availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<INKitRegister.kitInventoryID, INKitRegister.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
					availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<INKitRegister.kitInventoryID, INKitRegister.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);
					availability.QtyActual = INUnitAttribute.ConvertFromBase<INKitRegister.inventoryID, INKitRegister.uOM>(sender, e.Row, (decimal)availability.QtyActual, INPrecision.QUANTITY);

					e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(Messages.Availability_ActualInfo,
						sender.GetValue<INTran.uOM>(e.Row),
						FormatQty(availability.QtyOnHand),
						FormatQty(availability.QtyAvail),
						FormatQty(availability.QtyHardAvail),
						FormatQty(availability.QtyActual));

					AvailabilityCheck(sender, (INKitRegister)e.Row, availability);
				}
				else
				{
					e.ReturnValue = string.Empty;
				}
			}
			else
			{
				e.ReturnValue = string.Empty;
			}

			base.Availability_FieldSelecting(sender, e);
		}

		public override INKitTranSplit Convert(INKitRegister item)
		{
			using (InvtMultScope<INKitRegister> ms = new InvtMultScope<INKitRegister>(item))
			{
				INKitTranSplit ret = item;
				//baseqty will be overriden in all cases but AvailabilityFetch
				ret.BaseQty = item.BaseQty - item.UnassignedQty;
				return ret;
			}
		}

		public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
			{
				throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
			}
		}

		public virtual void INTranSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				if (((INKitTranSplit)e.Row).BaseQty != 0m && ((INKitTranSplit)e.Row).LocationID == null)
				{
					ThrowFieldIsEmpty<INKitTranSplit.locationID>(sender, e.Row);
				}               
			}
		}
        protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                INKitRegister row = e.Row as INKitRegister;
				if (row != null && row.Hold == false && Math.Abs((decimal)row.BaseQty) >= 0.0000005m && (row.UnassignedQty >= 0.0000005m || row.UnassignedQty <= -0.0000005m))
                {
                    if (sender.RaiseExceptionHandling<INKitRegister.qty>(e.Row, row.Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned)))
                    {
                        throw new PXRowPersistingException(typeof(INKitRegister.qty).Name, row.Qty, Messages.BinLotSerialNotAssigned);
                    }
                }
            }
            base.Master_RowPersisting(sender, e);
        }
        protected virtual void INKitRegister_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {           
            if (!sender.ObjectsEqual<INKitRegister.hold>(e.Row, e.OldRow) && (bool?)sender.GetValue<INKitRegister.hold>(e.Row) == false)
            {
                if (((INKitRegister)e.Row).UnassignedQty != 0)
                {
                    sender.RaiseExceptionHandling<INKitRegister.qty>(e.Row, ((INKitRegister)e.Row).Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned));
                }
            }
        }

		public virtual void INTranSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INKitRegister)];
			if (cache.Current != null && (e.Row == null || ((INKitRegister)cache.Current).LineNbr == ((INKitTranSplit)e.Row).LineNbr))
			{
				e.NewValue = ((INKitRegister)cache.Current).SubItemID;
				e.Cancel = true;
			}
		}

		public virtual void INTranSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INKitRegister)];
			if (cache.Current != null && (e.Row == null || ((INKitRegister)cache.Current).LineNbr == ((INKitTranSplit)e.Row).LineNbr))
			{
				e.NewValue = ((INKitRegister)cache.Current).LocationID;
				e.Cancel = true;
			}
		}

		public virtual void INTranSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(INKitRegister)];
			if (cache.Current != null && (e.Row == null || ((INKitRegister)cache.Current).LineNbr == ((INKitTranSplit)e.Row).LineNbr))
			{
				using (InvtMultScope<INKitRegister> ms = new InvtMultScope<INKitRegister>((INKitRegister)cache.Current))
				{
					e.NewValue = ((INKitRegister)cache.Current).InvtMult;
					e.Cancel = true;
				}
			}
		}

		public virtual void INTranSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INKitTranSplit)e.Row).InventoryID);

			if (item != null)
			{
				object InvtMult = ((INKitTranSplit)e.Row).InvtMult;
				if (InvtMult == null)
				{
					sender.RaiseFieldDefaulting<INKitTranSplit.invtMult>(e.Row, out InvtMult);
				}

				object TranType = ((INKitTranSplit)e.Row).TranType;
				if (TranType == null)
				{
					sender.RaiseFieldDefaulting<INKitTranSplit.tranType>(e.Row, out TranType);
				}

				INLotSerTrack.Mode mode = INLotSerialNbrAttribute.TranTrackMode(item, (string)TranType, (short?)InvtMult);
				if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
				{
					ILotSerNumVal lotSerNum = ReadLotSerNumVal(sender, item);
					foreach (INKitTranSplit lssplit in INLotSerialNbrAttribute.CreateNumbers<INKitTranSplit>(sender, item, lotSerNum, mode, 1m))
					{
						e.NewValue = lssplit.LotSerialNbr;
						e.Cancel = true;
					}
				}
				//otherwise default via attribute
			}
		}

		public virtual void INTranSplit_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((INKitTranSplit)e.Row).InventoryID);

			if (item != null && INLotSerialNbrAttribute.IsTrackSerial(item, ((INKitTranSplit)e.Row).TranType, ((INKitTranSplit)e.Row).InvtMult))				
			{
				if (e.NewValue != null && e.NewValue is decimal && (decimal)e.NewValue != 0m && (decimal)e.NewValue != 1m)
				{
					e.NewValue = 1m;
				}
			}
		}

		public virtual void INTran_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal?)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, PXErrorLevel.Error, (int)0);
			}
		}

		protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo ei)
		{
			if (row is INKitRegister)
			{
				sender.RaiseExceptionHandling<INKitRegister.qty>(row, null, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<INKitRegister.kitInventoryID>(row), sender.GetStateExt<INKitRegister.subItemID>(row), sender.GetStateExt<INKitRegister.siteID>(row), sender.GetStateExt<INKitRegister.locationID>(row), sender.GetValue<INKitRegister.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<INKitTranSplit.qty>(row, null, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<INKitTranSplit.inventoryID>(row), sender.GetStateExt<INKitTranSplit.subItemID>(row), sender.GetStateExt<INKitTranSplit.siteID>(row), sender.GetStateExt<INKitTranSplit.locationID>(row), sender.GetValue<INKitTranSplit.lotSerialNbr>(row)));
			}
		}

		protected override object[] SelectDetail(PXCache sender, INKitRegister row)
		{
			PXSelectBase<INKitTranSplit> select = new PXSelect<INKitTranSplit,
			Where<INKitTranSplit.docType, Equal<Required<INKitRegister.docType>>,
			And<INKitTranSplit.refNbr, Equal<Required<INKitRegister.refNbr>>,
			And<INKitTranSplit.lineNbr, Equal<Required<INKitRegister.kitLineNbr>>>>>>(_Graph);

			PXResultset<INKitTranSplit> res = select.Select(row.DocType, row.RefNbr, row.KitLineNbr);

			List<object> list = new List<object>(res.Count);

			foreach (INKitTranSplit detail in res)
			{
				list.Add(detail);
			}

			return list.ToArray();
		}

		protected override object[] SelectDetail(PXCache sender, INKitTranSplit row)
		{
			INKitRegister kitRow = (INKitRegister) PXParentAttribute.SelectParent(sender, row, typeof(INKitRegister));

			return SelectDetail(sender, kitRow);
		}

		#endregion
	}
		
	#endregion

}
