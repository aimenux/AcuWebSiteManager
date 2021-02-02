using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CT;
using PX.Objects.PM.BudgetControl;
using PX.Objects.PO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PM
{
	public class BudgetOverrunInquiry : PXGraph<BudgetOverrunInquiry>
	{
		public PXFilter<BudgetOverrunFilter> Filter;
		public PXCancel<BudgetOverrunFilter> Cancel;

		public PXSelect<PMProject> Project;

		public PXSelect<PMBudgetControlLineExt> BudgetControlLinesBuffer;

		public PXSelect<PMBudgetControlLineExt> BudgetControlLines;
		public virtual IEnumerable budgetControlLines()
		{
			BudgetControlLines.Cache.AllowInsert = false;
			BudgetControlLines.Cache.AllowUpdate = false;
			BudgetControlLines.Cache.AllowDelete = false;

			foreach (PMBudgetControlLineExt line in BudgetControlLinesBuffer.Cache.Inserted)
				yield return line;
		}

		protected virtual void _(Events.FieldUpdated<BudgetOverrunFilter, BudgetOverrunFilter.projectID> e)
		{
			if (e.Row != null)
				e.Cache.SetDefaultExt<BudgetOverrunFilter.dateFrom>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<BudgetOverrunFilter, BudgetOverrunFilter.dateFrom> e)
		{
			if (e.Row != null && e.Row.ProjectID != null)
			{
				PMProject project = Project.Search<PMProject.contractID>(e.Row.ProjectID);
				e.NewValue = project.StartDate;
			}
		}

		protected virtual void _(Events.RowUpdated<BudgetOverrunFilter> e)
		{
			BudgetControlLinesBuffer.Cache.Clear();
		}

		public PXAction<BudgetOverrunFilter> calculate;
		[PXUIField(DisplayName = Messages.BudgetControlCalculate, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton(Tooltip = Messages.BudgetControlCalculateTip)]
		public virtual IEnumerable Calculate(PXAdapter adapter)
		{
			var inquiry = this.Clone();
			PXLongOperation.StartOperation(this, () =>
			{
				if (inquiry.Filter.Current.ProjectID == null)
					throw new Exception(Messages.ProjectMustBeSpecified);

				if (inquiry.Filter.Current.DateFrom == null)
					throw new Exception(Messages.DateFromMustBeSpecified);

				if (inquiry.Filter.Current.DateTo == null)
					throw new Exception(Messages.DateToMustBeSpecified);

				if (inquiry.Filter.Current.DocType == null)
					throw new Exception(Messages.DocumentTypeMustBeSpecified);

				var lineID = 0;
				var docTypes = BudgetControlDocumentType.Parse(inquiry.Filter.Current.DocType);

				if (docTypes.Contains(BudgetControlDocumentType.PurchaseOrder) || docTypes.Contains(BudgetControlDocumentType.Subcontract))
					CalculatePurchaseOrders(inquiry, docTypes, ref lineID);

				if (docTypes.Contains(BudgetControlDocumentType.APBill))
					CalcualteAPBills(inquiry, ref lineID);
					
				if (docTypes.Contains(BudgetControlDocumentType.ChangeOrder))
					CalculateChangeOrders(inquiry, ref lineID);

				inquiry.BudgetControlLinesBuffer.Cache.IsDirty = false;
				PXLongOperation.SetCustomInfo(inquiry);
			});
			return adapter.Get();
		}

		private void CalculatePurchaseOrders(BudgetOverrunInquiry inquiry, string[] docTypes, ref int lineID)
		{
			var poTypes = new List<string>();
			if (docTypes.Contains(BudgetControlDocumentType.PurchaseOrder))
			{
				poTypes.Add(POOrderType.RegularOrder);
				poTypes.Add(POOrderType.DropShip);
			}
			if (docTypes.Contains(BudgetControlDocumentType.Subcontract)) poTypes.Add(POOrderType.RegularSubcontract);

			var docs = SelectFrom<POOrder>
				.Where<POOrder.orderDate.IsBetween<BudgetOverrunFilter.dateFrom.FromCurrent, BudgetOverrunFilter.dateTo.FromCurrent>
					.And<Not<POOrder.hold.IsEqual<False>.And<POOrder.approved.IsEqual<True>>>>
					.And<POOrder.orderType.IsIn<@P.AsString>>>.View.Select(inquiry, new object[] { poTypes.ToArray() }).AsEnumerable();

			if (!docs.Any()) return;

			var entry = CreateInstance<POOrderEntry>();
			foreach (POOrder doc in docs)
			{
				entry.Clear(PXClearOption.ClearAll);
				entry.Document.Current = doc;

				var entryExt = entry.GetExtension<BudgetControl.POOrderEntryExt>();
				foreach (PMBudgetControlLine line in entryExt.BudgetControlLines.Cache.Inserted)
				{
					if (LineIsMatch(inquiry, line))
					{
						var lineExt = CreateLine(line);
						lineExt.LineID = lineID++;
						lineExt.RefNoteID = doc.NoteID;
						lineExt.DocType = doc.OrderType == POOrderType.RegularSubcontract ?
							Messages.BudgetControlDocumentType_Subcontract : EntityHelper.GetFriendlyEntityName(typeof(POOrder));
						inquiry.BudgetControlLinesBuffer.Insert(lineExt);
					}
				}
			}
		}

		private void CalcualteAPBills(BudgetOverrunInquiry inquiry, ref int lineID)
		{
			var docs = SelectFrom<APInvoice>
						.Where<APInvoice.docDate.IsBetween<BudgetOverrunFilter.dateFrom.FromCurrent, BudgetOverrunFilter.dateTo.FromCurrent>
							.And<APInvoice.docType.IsNotEqual<APDocType.debitAdj>>
							.And<APInvoice.released.IsEqual<False>>
							.And<APInvoice.voided.IsEqual<False>>
							.And<APInvoice.rejected.IsEqual<False>>>.View.Select(inquiry);
			if (!docs.Any()) return;

			var entry = CreateInstance<APInvoiceEntry>();
			foreach (APInvoice doc in docs)
			{
				entry.Clear(PXClearOption.ClearAll);
				entry.Document.Current = doc;

				var entryExt = entry.GetExtension<BudgetControl.APInvoiceEntryExt>();
				foreach (PMBudgetControlLine line in entryExt.BudgetControlLines.Cache.Inserted)
				{
					if (LineIsMatch(inquiry, line))
					{
						var lineExt = CreateLine(line);
						lineExt.LineID = lineID++;
						lineExt.RefNoteID = doc.NoteID;
						lineExt.DocType = EntityHelper.GetFriendlyEntityName(typeof(APInvoice));
						inquiry.BudgetControlLinesBuffer.Insert(lineExt);
					}
				}
			}
		}

		private void CalculateChangeOrders(BudgetOverrunInquiry inquiry, ref int lineID)
		{
			var docs = SelectFrom<PMChangeOrder>
						.Where<PMChangeOrder.projectID.IsEqual<BudgetOverrunFilter.projectID.FromCurrent>
							.And<PMChangeOrder.date.IsBetween<BudgetOverrunFilter.dateFrom.FromCurrent, BudgetOverrunFilter.dateTo.FromCurrent>>
							.And<PMChangeOrder.released.IsEqual<False>>
							.And<PMChangeOrder.rejected.IsEqual<False>>>.View.Select(inquiry);
			if (!docs.Any()) return;

			var entry = CreateInstance<ChangeOrderEntry>();
			foreach (PMChangeOrder doc in docs)
			{
				entry.Clear(PXClearOption.ClearAll);
				entry.Document.Current = doc;

				var entryExt = entry.GetExtension<ChangeOrderEntryExt>();
				foreach (PMBudgetControlLine line in entryExt.BudgetControlLines.Cache.Inserted)
				{
					if (LineIsMatch(inquiry, line))
					{
						var lineExt = CreateLine(line);
						lineExt.LineID = lineID++;
						lineExt.RefNoteID = doc.NoteID;
						lineExt.DocType = EntityHelper.GetFriendlyEntityName(typeof(PMChangeOrder));
						inquiry.BudgetControlLinesBuffer.Insert(lineExt);
					}
				}
			}
		}

		private bool LineIsMatch(BudgetOverrunInquiry inquiry, PMBudgetControlLine line)
		{
			if (inquiry.Filter.Current.ProjectID != line.ProjectID) return false;
			if (inquiry.Filter.Current.TaskID != null && inquiry.Filter.Current.TaskID != line.TaskID) return false;
			if (inquiry.Filter.Current.AccountGroupID != null && inquiry.Filter.Current.AccountGroupID != line.AccountGroupID) return false;
			if (inquiry.Filter.Current.CostCodeID != null && inquiry.Filter.Current.CostCodeID != line.CostCodeID) return false;
			if (inquiry.Filter.Current.InventoryID != null && inquiry.Filter.Current.TaskID != line.InventoryID) return false;
			return line.RemainingAmount < 0;
		}

		private PMBudgetControlLineExt CreateLine(PMBudgetControlLine line)
		{
			PMProject project = Project.Search<PMProject.contractID>(line.ProjectID);

			var result = new PMBudgetControlLineExt();
			result.ProjectID = line.ProjectID;
			result.TaskID = line.TaskID;
			result.AccountGroupID = line.AccountGroupID;
			result.CostCodeID = line.CostCodeID;
			result.InventoryID = line.InventoryID;
			result.LineNumbers = line.LineNumbers;
			result.BudgetedAmount = line.BudgetedAmount;
			result.ConsumedAmount = line.ConsumedAmount;
			result.DocumentAmount = line.DocumentAmount;
			result.CuryID = project.CuryID;
			return result;
		}

		public PXAction<BudgetOverrunFilter> EditDocument;
		[PXButton]
		[PXUIField]
		public virtual void editDocument()
		{
			var helper = new EntityHelper(this);
			helper.NavigateToRow(BudgetControlLines.Current.RefNoteID.Value, PXRedirectHelper.WindowMode.NewWindow);
		}
	}

	[PXHidden]
	public class BudgetOverrunFilter : IBqlTable
	{
		#region DocType
		public abstract class docType : BqlString.Field<docType> { }
		[PXUIField(DisplayName = "Document Type")]
		[PXDBString]
		[BudgetControlDocumentType.List]
		public virtual string DocType { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : BqlInt.Field<projectID> { }
		[PXDefault]
		[Project(typeof(Where<PMProject.baseType, Equal<CTPRType.project>, And<PMProject.nonProject, Equal<False>>>), WarnIfCompleted = false)]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region DateFrom
		public abstract class dateFrom : BqlDateTime.Field<dateFrom> { }
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "From")]
		public virtual DateTime? DateFrom
		{
			get;
			set;
		}
		#endregion
		#region DateTo
		public abstract class dateTo : BqlDateTime.Field<dateTo> { }
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "To")]
		public virtual DateTime? DateTo
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : BqlInt.Field<accountGroupID> { }
		[AccountGroup]
		public virtual int? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public abstract class taskID : BqlInt.Field<taskID> { }
		[ProjectTask(typeof(projectID))]
		public virtual int? TaskID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : BqlInt.Field<costCodeID> { }
		[CostCode(Filterable = false, SkipVerification = true)]
		public virtual int? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
		[PMInventorySelector(Filterable = true)]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
	}

	[PXCacheName("Budget Lines")]
	public class PMBudgetControlLineExt : PMBudgetControlLine
	{
		#region LineID
		public abstract class lineID : BqlInt.Field<lineID> { }
		[PXDBInt(IsKey = true)]
		public virtual int? LineID
		{
			get;
			set;
		}
		#endregion
		#region ProjectID
		public new abstract class projectID : BqlInt.Field<projectID> { }
		[Project(typeof(Where<PMProject.baseType, Equal<CTPRType.project>, And<PMProject.nonProject, Equal<False>>>), WarnIfCompleted = false)]
		public override int? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region TaskID
		public new abstract class taskID : BqlInt.Field<taskID> { }
		[ProjectTask(typeof(PMBudgetControlLine.projectID))]
		public override int? TaskID
		{
			get;
			set;
		}
		#endregion
		#region AccountGroupID
		public new abstract class accountGroupID : BqlInt.Field<accountGroupID> { }
		[AccountGroup]
		public override int? AccountGroupID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public new abstract class inventoryID : BqlInt.Field<inventoryID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
		[PMInventorySelector(Filterable = true)]
		public override int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public new abstract class costCodeID : BqlInt.Field<costCodeID> { }
		[CostCode(Filterable = false, SkipVerification = true)]
		public override int? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : BqlString.Field<docType> { }
		[PXUIField(DisplayName = "Type")]
		[PXDBString(15)]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNoteID
		public abstract class refNoteID : BqlGuid.Field<refNoteID> { }
		[PXRefNote(LastKeyOnly = true)]
		[PXUIField(DisplayName = "Reference Nbr.")]
		public virtual Guid? RefNoteID
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : BqlString.Field<curyID> { }
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Project Currency")]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class BudgetControlDocumentType
	{
		private static char Separator = ',';

		public class ListAttribute : PXStringListAttribute, IPXFieldDefaultingSubscriber
		{
			public ListAttribute() : base
			(
				Values,
				new string[] 
				{
					Messages.BudgetControlDocumentType_PurchaseOrder,
					Messages.BudgetControlDocumentType_Subcontract,
					Messages.BudgetControlDocumentType_APBill,
					Messages.BudgetControlDocumentType_ChangeOrder
				}
			)
			{
				MultiSelect = true;
			}

			public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
			{
				e.NewValue = string.Join(Separator.ToString(), Values);
			}
		}

		private static string[] Values = { PurchaseOrder, Subcontract, APBill, ChangeOrder };

		public const string PurchaseOrder = "PO";
		public const string Subcontract = "SC";
		public const string APBill = "AP";
		public const string ChangeOrder = "CO";

		public static string[] Parse(string value)
		{
			var result = value.Split(Separator);
			return result;
		}
	}
}