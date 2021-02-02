using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.IN;
using System.Collections;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.AP;

namespace PX.Objects.DR
{
	public class DRRevenueAccumAttribute : PXAccumulatorAttribute
	{
		public DRRevenueAccumAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			DRRevenueProjectionAccum item = (DRRevenueProjectionAccum)row;
			columns.Update<DRRevenueProjectionAccum.pTDProjected>(item.PTDProjected, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<DRRevenueProjectionAccum.pTDRecognized>(item.PTDRecognized, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<DRRevenueProjectionAccum.pTDRecognizedSamePeriod>(item.PTDRecognizedSamePeriod, PXDataFieldAssign.AssignBehavior.Summarize);
			
			return true;
		}
	}

	public class DRExpenseAccumAttribute : PXAccumulatorAttribute
	{
		public DRExpenseAccumAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			DRExpenseProjectionAccum item = (DRExpenseProjectionAccum)row;
			columns.Update<DRExpenseProjectionAccum.pTDProjected>(item.PTDProjected, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<DRExpenseProjectionAccum.pTDRecognized>(item.PTDRecognized, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<DRExpenseProjectionAccum.pTDRecognizedSamePeriod>(item.PTDRecognizedSamePeriod, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}
	}

	public class DRComponentSelectorAttribute : PXSelectorAttribute
	{
		private const string EmptyComponentCD = Messages.EmptyComponentCD;

		public DRComponentSelectorAttribute()
			: base(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>>>>), new Type[] { typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr) })
		{
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (object.Equals(DRScheduleDetail.EmptyComponentID.ToString(), e.NewValue)
				|| object.Equals(DRScheduleDetail.EmptyComponentID, e.NewValue))
				return;

			base.FieldVerifying(sender, e);
		}

		public override void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (object.Equals(EmptyComponentCD, e.NewValue))
			{
				e.NewValue = DRScheduleDetail.EmptyComponentID;
			}
			else
				base.SubstituteKeyFieldUpdating(sender, e);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (object.Equals(DRScheduleDetail.EmptyComponentID.ToString(), e.ReturnValue))
			{
				e.ReturnValue = EmptyComponentCD;
			}
			else
				base.FieldSelecting(sender, e);
		}
	}

	/// <summary>
	/// Attribute to be put onto a reference number field
	/// of various deferred revenue entities (e.g. <see cref="DRSchedule.RefNbr"/>
	/// that allows selecting relevant AR / AP documents that the DR entity refers to.
	/// </summary>
	public class DRDocumentSelectorAttribute : PXCustomSelectorAttribute
	{
		protected readonly Type _moduleField;
		protected readonly Type _docTypeField;
		protected readonly Type _businessAccountField;

		/// <summary>
		/// Gets or sets a flag indicating whether unreleased 
		/// documents should be excluded from the selection.
		/// </summary>
		public bool ExcludeUnreleased
		{
			get;
			set;
		}

		/// <param name="businessAccountField">
		/// Optional business account field. If not <c>null</c>,
		/// then the choice in the selector will be restricted by documents
		/// that correspond to that business account.
		/// </param>
		public DRDocumentSelectorAttribute(Type moduleField, Type docTypeField, Type businessAccountField = null) 
			: base(typeof(DRDocumentRecord.refNbr))
		{
			if (moduleField == null) throw new ArgumentNullException(nameof(moduleField));
			if (docTypeField == null) throw new ArgumentNullException(nameof(docTypeField));

			if (BqlCommand.GetItemType(moduleField).Name != BqlCommand.GetItemType(docTypeField).Name ||
				businessAccountField != null && 
					BqlCommand.GetItemType(businessAccountField).Name != 
					BqlCommand.GetItemType(moduleField).Name)
			{
				throw new ArgumentException(Messages.AllFieldsMustBelongToTheSameType);
			}
			
			this._moduleField = moduleField;
			this._docTypeField = docTypeField;
			this._businessAccountField = businessAccountField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_BqlTable.Name != BqlCommand.GetItemType(_moduleField).Name)
			{
				throw new ArgumentException(Messages.AllFieldsMustBelongToTheSameType);
			}
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null || e.NewValue == null) return;

			string module = (string)sender.GetValue(e.Row, _moduleField.Name);
			string documentType = (string)sender.GetValue(e.Row, _docTypeField.Name);

			BqlCommand relevantDocumentsSelect;

			if (module == BatchModule.AR)
			{
				relevantDocumentsSelect = new Select<
					ARRegister,
					Where<
						ARRegister.docType, Equal<Required<ARRegister.docType>>,
						And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>();

				if (ExcludeUnreleased)
				{
					relevantDocumentsSelect =
						relevantDocumentsSelect.WhereAnd<Where<ARRegister.released, Equal<True>>>();
				}
			}
			else if (module == BatchModule.AP)
			{
				relevantDocumentsSelect = new Select<
					APRegister,
					Where<
						APRegister.docType, Equal<Required<APRegister.docType>>,
						And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>();

				if (ExcludeUnreleased)
				{
					relevantDocumentsSelect =
						relevantDocumentsSelect.WhereAnd<Where<APRegister.released, Equal<True>>>();
				}
			}
			else
			{
				throw new PXException(Messages.UnexpectedModuleSpecified);
			}

			PXView relevantDocuments = new PXView(_Graph, true, relevantDocumentsSelect);

			if (relevantDocuments.SelectSingle(documentType, e.NewValue) == null)
			{
				throwNoItem(
					restricted: null, 
					external: true, 
					value: e.NewValue);
			}
		}

		protected virtual IEnumerable GetRecords()
		{
			PXCache cache = this._Graph.Caches[BqlCommand.GetItemType(_moduleField)];

			if (cache.Current == null) yield break;

			string docType = (string)cache.GetValue(cache.Current, _docTypeField.Name);
			string module = (string)cache.GetValue(cache.Current, _moduleField.Name);

			// Since this method is called on the cache-level attribute,
			// we need to check the value of ExcludeUnreleased of item-level
			// attribute of the current record.
			// -
			bool excludeUnreleased = cache
				.GetAttributesReadonly(cache.Current, _FieldName)
				.OfType<DRDocumentSelectorAttribute>()
				.First()
				.ExcludeUnreleased;

			int? businessAccountID = null;

			if (_businessAccountField != null)
			{
				businessAccountID = 
					(int?)cache.GetValue(cache.Current, _businessAccountField.Name);
			}

			if (module == BatchModule.AR)
			{
				PXSelectBase<ARInvoice> relevantInvoices = new PXSelectJoin<
					ARInvoice, 
						InnerJoin<BAccount, On<BAccount.bAccountID, Equal<ARInvoice.customerID>>>,
					Where<
						ARInvoice.docType, Equal<Required<ARInvoice.docType>>>>
					(this._Graph);

				if (excludeUnreleased)
				{
					relevantInvoices.WhereAnd<Where<ARInvoice.released, Equal<True>>>();
				}

				object[] queryParameters;

				if (businessAccountID.HasValue)
				{
					relevantInvoices.WhereAnd<
						Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>>>();

					queryParameters = new object[] { docType, businessAccountID };
				}
				else
				{
					queryParameters = new object[] { docType };
				}

				foreach (PXResult<ARInvoice, BAccount> result in relevantInvoices.Select(queryParameters))
				{
					ARInvoice arInvoice = result;
					BAccount customer = result;

					string status = null;

					ARDocStatus.ListAttribute documentStatusListAttribute = 
						new ARDocStatus.ListAttribute();

					if (documentStatusListAttribute.ValueLabelDic.ContainsKey(arInvoice.Status))
					{
						status = documentStatusListAttribute.ValueLabelDic[arInvoice.Status];
					}

					DRDocumentRecord record = new DRDocumentRecord
					{
						BAccountCD = customer.AcctCD,
						RefNbr = arInvoice.RefNbr,
						Status = status,
						FinPeriodID = arInvoice.FinPeriodID,
						DocType = arInvoice.DocType,
						DocDate = arInvoice.DocDate,
						LocationID = arInvoice.CustomerLocationID,
						CuryOrigDocAmt = arInvoice.CuryOrigDocAmt,
						CuryID = arInvoice.CuryID
					};

					yield return record;
				}
			}
			else if (module == BatchModule.AP)
			{
				PXSelectBase<APInvoice> relevantBills = new PXSelectJoin<
					APInvoice,
					InnerJoin<BAccount, On<BAccount.bAccountID, Equal<APInvoice.vendorID>>>,
					Where<
						APInvoice.docType, Equal<Required<APInvoice.docType>>>>
					(this._Graph);

				if (excludeUnreleased)
				{
					relevantBills.WhereAnd<Where<APInvoice.released, Equal<True>>>();
				}

				object[] queryParameters;

				if (businessAccountID.HasValue)
				{
					relevantBills.WhereAnd<
						Where<APInvoice.vendorID, Equal<Required<APInvoice.vendorID>>>>();

					queryParameters = new object[] { docType, businessAccountID };
				}
				else
				{
					queryParameters = new object[] { docType };
				}

				foreach (PXResult<APInvoice, BAccount> result in relevantBills.Select(queryParameters))
				{
					APInvoice apInvoice = result;
					BAccount vendor = result;

					string status = null;

					APDocStatus.ListAttribute documentStatusListAttribute =
						new APDocStatus.ListAttribute();

					if (documentStatusListAttribute.ValueLabelDic.ContainsKey(apInvoice.Status))
					{
						status = documentStatusListAttribute.ValueLabelDic[apInvoice.Status];
					}

					DRDocumentRecord record = new DRDocumentRecord
					{
						BAccountCD = vendor.AcctCD,
						RefNbr = apInvoice.RefNbr,
						Status = status,
						FinPeriodID = apInvoice.FinPeriodID,
						DocType = apInvoice.DocType,
						DocDate = apInvoice.DocDate,
						LocationID = apInvoice.VendorLocationID,
						CuryOrigDocAmt = apInvoice.CuryOrigDocAmt,
						CuryID = apInvoice.CuryID
					};

					yield return record;
				}
			}
			else
			{
				throw new PXException(Messages.UnexpectedModuleSpecified);
			}
		}
	}

	public class DRLineSelectorAttribute : PXCustomSelectorAttribute
	{
		private readonly Type moduleField;
		private readonly Type docTypeField;
		private readonly Type refNbrField;

		public DRLineSelectorAttribute(Type moduleField, Type docTypeField, Type refNbrField)
			: base(typeof(DRLineRecord.lineNbr))
		{
			if (moduleField == null) throw new ArgumentNullException(nameof(moduleField));
			if (docTypeField == null) throw new ArgumentNullException(nameof(docTypeField));
			if (refNbrField == null) throw new ArgumentNullException(nameof(refNbrField));

			if (BqlCommand.GetItemType(moduleField).Name != BqlCommand.GetItemType(docTypeField).Name ||
				BqlCommand.GetItemType(moduleField).Name != BqlCommand.GetItemType(refNbrField).Name)
			{
				throw new ArgumentException(Messages.AllFieldsMustBelongToTheSameType);
			}

			this.moduleField = moduleField;
			this.docTypeField = docTypeField;
			this.refNbrField = refNbrField;
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		protected virtual IEnumerable GetRecords()
		{
			PXCache cache = this._Graph.Caches[BqlCommand.GetItemType(moduleField)];

			if (cache.Current == null) yield break;

			string docType = (string)cache.GetValue(cache.Current, docTypeField.Name);
			string refNbr = (string)cache.GetValue(cache.Current, refNbrField.Name);
			string module = (string)cache.GetValue(cache.Current, moduleField.Name);

			if (module == BatchModule.AR)
			{
				PXSelectBase<ARTran> documentLines = new PXSelect<
					ARTran,
					Where<
						ARTran.tranType, Equal<Required<ARTran.tranType>>,
						And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
						And<Where<
							ARTran.lineType, IsNull,
							Or<ARTran.lineType, NotEqual<SO.SOLineType.discount>>>>>>>
					(_Graph);

				foreach (ARTran documentLine in documentLines.Select(docType, refNbr))
				{
					ARReleaseProcess.Amount salesPostingAmount =
						ARReleaseProcess.GetSalesPostingAmount(_Graph, documentLine);

					DRLineRecord record = new DRLineRecord
					{
						CuryInfoID = documentLine.CuryInfoID,
						CuryTranAmt = salesPostingAmount.Cury,
						InventoryID = documentLine.InventoryID,
						LineNbr = documentLine.LineNbr,
						TranAmt = salesPostingAmount.Base,
						TranDesc = documentLine.TranDesc,
						BranchID = documentLine.BranchID
					};

					yield return record;
				}
			}
			else
			{
				PXSelectBase<APTran> documentLines = new PXSelect<
					APTran,
					Where<
						APTran.tranType, Equal<Required<APTran.tranType>>,
						And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
						And<Where<
							APTran.lineType, IsNull,
							Or<APTran.lineType, NotEqual<SO.SOLineType.discount>>>>>>>
					(_Graph);

				foreach (APTran documentLine in documentLines.Select(docType, refNbr))
				{
					ARReleaseProcess.Amount expensePostingAmount =
						APReleaseProcess.GetExpensePostingAmount(_Graph, documentLine);

					DRLineRecord record = new DRLineRecord
					{
						CuryInfoID = documentLine.CuryInfoID,
						CuryTranAmt = expensePostingAmount.Cury,
						InventoryID = documentLine.InventoryID,
						LineNbr = documentLine.LineNbr,
						TranAmt = expensePostingAmount.Base,
						TranDesc = documentLine.TranDesc,
						BranchID = documentLine.BranchID
					};

					yield return record;
				}
			}
		}
	}

	[Serializable]
    [PXHidden]
	public partial class DRDocumentRecord : IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]
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
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		protected DateTime? _DocDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate
		{
			get
			{
				return this._DocDate;
			}
			set
			{
				this._DocDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[FinPeriodID]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region BAccountCD
		public abstract class bAccountCD : PX.Data.BQL.BqlString.Field<bAccountCD> { }
		[PXDBString]
		[PXUIField(DisplayName = Messages.BusinessAccount, Visibility=PXUIVisibility.SelectorVisible)]
		public virtual string BAccountCD
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<DRDocumentRecord.bAccountCD>>>), Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Location.descr))]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;
		[PXDBString()]
		[PXUIField(DisplayName = "Processing Status", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		protected Decimal? _CuryOrigDocAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCury(typeof(DRDocumentRecord.curyID))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryOrigDocAmt
		{
			get
			{
				return this._CuryOrigDocAmt;
			}
			set
			{
				this._CuryOrigDocAmt = value;
			}
		}
		#endregion
	}

	[Serializable]
    [PXHidden]
	public partial class DRLineRecord : IBqlTable
	{
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[NonStockItem(Filterable = true, Visibility = PXUIVisibility.SelectorVisible)]
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
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Transaction Descr.", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(ARRegister.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }
		protected Decimal? _CuryTranAmt;
		[PXDBCurrency(typeof(DRLineRecord.curyInfoID), typeof(DRLineRecord.tranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranAmt
		{
			get
			{
				return this._CuryTranAmt;
			}
			set
			{
				this._CuryTranAmt = value;
			}
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
		protected Decimal? _TranAmt;
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[Branch]
		[PXUIField(DisplayName = "Branch", Visibility =PXUIVisibility.SelectorVisible)]
		public virtual int? BranchID { get; set; }
        #endregion
        }

	public class DRTerms
	{
		public const string Day = "D";
		public const string Week = "W";
		public const string Month = "M";
		public const string Year = "Y";

		public class UOMListAttribute : PXStringListAttribute
		{
			public UOMListAttribute()
				: base(
				new string[] { Day, Week, Month, Year },
				new string[] { Messages.Day, Messages.Week, Messages.Month, Messages.Year })
			{
			}
		}

		public class Term : Tuple<decimal?, string>
		{
			public decimal? Length { get { return Item1; } }
			public string UOM { get { return Item2; } }

			public Term(decimal? term, string uom)
				: base(term, uom)
			{
			}

			public DateTime? Delay(DateTime? origin)
			{
				if (origin.HasValue == false)
					return null;

				DateTime date = origin.Value;
				int lag = (int)Length;

				if (lag == 0)
					return date;

				switch (UOM)
				{
					case Day:
						date = date.AddDays(lag);
						break;
					case Week:
						date = date.AddDays(7 * lag);
						break;
					case Month:
						date = date.AddMonths(lag);
						break;
					case Year:
						date = date.AddYears(lag);
						break;
					default:
						return null;
				}

				return date.AddDays(-1);
			}
		}

		public static Term GetDefaultItemTerms(PXGraph graph, InventoryItem item)
		{
			if (item == null || item.DeferredCode == null)
				return null;

			var deferralCode = (DRDeferredCode)PXSelectorAttribute.Select<InventoryItem.deferredCode>(graph.Caches[typeof(InventoryItem)], item);
			if (deferralCode == null)
				return null;

			if (deferralCode.MultiDeliverableArrangement == false && DeferredMethodType.RequiresTerms(deferralCode.Method))
			{
				return new Term(item.DefaultTerm, item.DefaultTermUOM);
			}
			else if (deferralCode.MultiDeliverableArrangement == true)
			{
				var components = PXSelect<INComponent, Where<INComponent.inventoryID, Equal<Required<INComponent.inventoryID>>>>.Select(graph, item.InventoryID);
				foreach (INComponent component in components)
				{
					var componentCode = (DRDeferredCode)PXSelectorAttribute.Select<INComponent.deferredCode>(graph.Caches[typeof(INComponent)], component);
					if (componentCode != null && DeferredMethodType.RequiresTerms(componentCode.Method))
					{
						return new Term(component.DefaultTerm, component.DefaultTermUOM);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// The attribute sets and updates the underlying field that specifies 
		/// whether the deferral terms are required for the current item.
		/// If <see cref="DatesAttribute.VerifyDatesPresent"/> property is set, the attribute verifies 
		/// that the Term Start Date and Term End Date are present. If they are present,
		/// ensures that they are consistent with each other.
		/// Subscribes to the Field Updated event of Inventory ID, Start Date, and Deferral Code
		/// to set default dates and update the underlying field.
		/// </summary>
		public class DatesAttribute : PXEventSubscriberAttribute,
			IPXRowSelectedSubscriber,
			IPXRowSelectingSubscriber,
			IPXRowPersistingSubscriber
		{
			private Type _StartDateField;
			private Type _EndDateField;
			private Type _InventoryField;
			private Type _DeferralCodeField;

			public bool VerifyDatesPresent { get; set; }

			public DatesAttribute(
				Type startDateField, Type endDateField,
				Type inventoryField, Type deferralCodeField)
			{
				_StartDateField = startDateField;
				_EndDateField = endDateField;
				_InventoryField = inventoryField;
				_DeferralCodeField = deferralCodeField;

				VerifyDatesPresent = true;
			}

			public DatesAttribute(
				Type startDateField, Type endDateField, Type inventoryField)
				: this(startDateField, endDateField, inventoryField, null)
			{
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				sender.Graph.FieldUpdated.AddHandler(_BqlTable, _InventoryField.Name, InventoryOrCodeUpdated);
				sender.Graph.FieldUpdated.AddHandler(_BqlTable, _StartDateField.Name, StartDateUpdated);

				if(_DeferralCodeField != null)
				{
					sender.Graph.FieldUpdated.AddHandler(_BqlTable, _DeferralCodeField.Name, InventoryOrCodeUpdated);
				}
			}

			protected virtual void InventoryOrCodeUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				if (e.Row != null)
				{
					UpdateRequiresTerms(sender, e.Row);
					InventoryItem item = (InventoryItem)PXSelectorAttribute.Select(sender, e.Row, sender.GetField(_InventoryField));
					var terms = DRTerms.GetDefaultItemTerms(sender.Graph, item);
					SetDefaultDates(sender, e.Row, terms);
				}
			}

			protected virtual void StartDateUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				if(e.Row != null)
				{
					var startDate = (DateTime?)sender.GetValue(e.Row, _StartDateField.Name);
					if(startDate != null)
					{
						InventoryItem item = (InventoryItem)PXSelectorAttribute.Select(sender, e.Row, sender.GetField(_InventoryField));
						var term = DRTerms.GetDefaultItemTerms(sender.Graph, item);
						var endDate = term != null ? term.Delay(startDate) : null;
						sender.SetValueExt(e.Row, _EndDateField.Name, endDate);
					}
				}
			}

			public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
			{
				if(e.Row != null)
				{
					bool requireTerms = false;

					var itemID = (int?)sender.GetValue(e.Row, _InventoryField.Name);

					using (new PXConnectionScope())
					{
						if (_DeferralCodeField == null)
						{
							InventoryItem item = (InventoryItem)PXSelectorAttribute.Select(sender, e.Row, sender.GetField(_InventoryField));
							requireTerms = DRTerms.GetDefaultItemTerms(sender.Graph, item) != null;
						}
						else
						{
							var deferralCodeID = (string)sender.GetValue(e.Row, _DeferralCodeField.Name);
							requireTerms = RequireTerms(sender, e.Row, deferralCodeID);
						}
					}

					sender.SetValue(e.Row, _FieldOrdinal, requireTerms);
				}
			}

			protected virtual void UpdateRequiresTerms(PXCache cache, object row)
			{
				bool requireTerms = false;

				var itemID = (int?)cache.GetValue(row, _InventoryField.Name);
					
				if (_DeferralCodeField == null)
				{
					InventoryItem item = (InventoryItem)PXSelectorAttribute.Select(cache, row, cache.GetField(_InventoryField));
					var terms = DRTerms.GetDefaultItemTerms(cache.Graph, item);
					requireTerms = terms != null;
				}
				else
				{
					var deferralCodeID = (string)cache.GetValue(row, _DeferralCodeField.Name);
					requireTerms = RequireTerms(cache, row, deferralCodeID);
				}

				cache.SetValue(row, _FieldOrdinal, requireTerms);
			}


			protected virtual bool RequireTerms(PXCache sender, object row, string code)
			{
				if (code == null)
					return false;

				DRDeferredCode deferralCode = PXSelect<DRDeferredCode, Where<DRDeferredCode.deferredCodeID, Equal<Required<DRDeferredCode.deferredCodeID>>>>.Select(sender.Graph, code);
				if (deferralCode == null)
					return false;

				if (deferralCode.MultiDeliverableArrangement == false)
					return DeferredMethodType.RequiresTerms(deferralCode.Method);
				else
				{
					InventoryItem item = (InventoryItem)PXSelectorAttribute.Select(sender, row, sender.GetField(_InventoryField));
					var terms = DRTerms.GetDefaultItemTerms(sender.Graph, item);

					return terms != null;
				}

			}

			protected virtual void SetDefaultDates(PXCache cache, object row, DRTerms.Term term)
			{
				if (term != null)
				{
					cache.SetDefaultExt(row, _StartDateField.Name);

					DateTime? startDate = (DateTime?)cache.GetValue(row, _StartDateField.Name);
					DateTime? endDate = startDate.HasValue ? term.Delay(startDate) : null;

					cache.SetValue(row, _EndDateField.Name, endDate);
				}
				else
				{
					cache.SetValue(row, _StartDateField.Name, null);
					cache.SetValue(row, _EndDateField.Name, null);
				}
			}

			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				if(e.Row != null)
				{
					bool requireTerms = ((bool?)sender.GetValue(e.Row, _FieldOrdinal) ?? false);

					PXUIFieldAttribute.SetEnabled(sender, e.Row, _StartDateField.Name, requireTerms);
					PXUIFieldAttribute.SetEnabled(sender, e.Row, _EndDateField.Name, requireTerms);

					var termsCheck = requireTerms && VerifyDatesPresent ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;
					PXDefaultAttribute.SetPersistingCheck(sender, _StartDateField.Name, e.Row, termsCheck);
					PXDefaultAttribute.SetPersistingCheck(sender, _EndDateField.Name, e.Row, termsCheck);
				}
			}

			public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				var requiresTerms = (bool?)sender.GetValue(e.Row, _FieldOrdinal);
				var startDate = (DateTime?)sender.GetValue(e.Row, _StartDateField.Name);
				var endDate = (DateTime?)sender.GetValue(e.Row, _EndDateField.Name);

				if (requiresTerms == true && 
					startDate.HasValue && 
					endDate.HasValue && 
					endDate <= startDate)
				{
					if (sender.RaiseExceptionHandling(_EndDateField.Name, e.Row, endDate, new PXSetPropertyException(DR.Messages.TermCantBeNegative, endDate, startDate)))
					{
						throw new PXRowPersistingException(_EndDateField.Name, endDate, DR.Messages.TermCantBeNegative, endDate, startDate);
					}
				}

				if (!startDate.HasValue && endDate.HasValue)
				{
					if (sender.RaiseExceptionHandling(_StartDateField.Name, e.Row, null, new PXSetPropertyException(DR.Messages.TermNeedsStartDate)))
					{
						throw new PXRowPersistingException(_StartDateField.Name, null, DR.Messages.TermNeedsStartDate);
					}
				}
			}
		}

		public class VerifyResidualAttribute : PXEventSubscriberAttribute,
			IPXRowSelectedSubscriber,
			IPXRowSelectingSubscriber
		{
			private Type _InventoryField;
			private Type _DeferralCodeField;
			private Type _drUnitPriceField;
			private Type _amountField;

			public VerifyResidualAttribute(Type inventoryField, Type deferralCodeField, Type drUnitPriceField, Type amountField)
			{
				_InventoryField = inventoryField;
				_DeferralCodeField = deferralCodeField;
				_drUnitPriceField = drUnitPriceField;
				_amountField = amountField;
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				sender.Graph.FieldUpdated.AddHandler(_BqlTable, _InventoryField.Name, InventoryOrCodeUpdated);

				if (_DeferralCodeField != null)
				{
					sender.Graph.FieldUpdated.AddHandler(_BqlTable, _DeferralCodeField.Name, InventoryOrCodeUpdated);
				}
			}

			protected virtual void InventoryOrCodeUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				if (e.Row != null)
				{
					UpdateHasResidual(sender, e.Row);
				}
			}

			public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
			{
				if (e.Row != null)
				{
					using (new PXConnectionScope())
					{
						UpdateHasResidual(sender, e.Row);
					}
				}
			}

			protected virtual void UpdateHasResidual(PXCache cache, object row)
			{
				bool hasResidual = _DeferralCodeField != null && cache.GetValue(row, _DeferralCodeField.Name) == null
					? false
					: HasResidual(cache, row);
				cache.SetValue(row, _FieldOrdinal, hasResidual);
			}

			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				if (e.Row != null)
				{
					bool hasResidual = ((bool?)sender.GetValue(e.Row, _FieldOrdinal) ?? false);
					decimal? drUnitPrice = (decimal?)sender.GetValue(e.Row, _drUnitPriceField.Name);
					decimal? amount = (decimal?)sender.GetValue(e.Row, _amountField.Name);

					if(hasResidual && (drUnitPrice ?? 0.0m) == 0.0m && (amount ?? 0.0m) != 0.0m)
					{
						sender.RaiseExceptionHandling(_InventoryField.Name, e.Row, sender.GetValue(e.Row, _InventoryField.Name),
							new PXSetPropertyException(Messages.NoPriceListWithResidual, PXErrorLevel.Warning));
					}
					else
					{
						var error = PXUIFieldAttribute.GetError(sender, e.Row, _InventoryField.Name);
						if(error == PXMessages.LocalizeNoPrefix(Messages.NoPriceListWithResidual))
						{
							sender.RaiseExceptionHandling(_InventoryField.Name, e.Row, sender.GetValue(e.Row, _InventoryField.Name), null);
						}
					}
				}
			}

			private bool HasResidual(PXCache sender, object row)
			{
				InventoryItem item = (InventoryItem) PXSelectorAttribute.Select(sender, row, sender.GetField(_InventoryField));
				if (item == null || item.IsSplitted != true)
					return false;
				else
					return PXSelect<INComponent, Where<INComponent.inventoryID, Equal<Required<INComponent.inventoryID>>>>.Select(sender.Graph, item.InventoryID)
						.RowCast<INComponent>()
						.Any(c => c.AmtOption == INAmountOption.Residual);
			}
		}
	}

    #region SubAccountARList
    public class SubAccountARList
    {
        public class CustomListAttribute : PXStringListAttribute
        {
            public string[] AllowedValues
            {
                get
                {
                    return _AllowedValues;
                }
            }

            public string[] AllowedLabels
            {
                get
                {
                    return _AllowedLabels;
                }
            }

            public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels)
                : base(AllowedValues, AllowedLabels)
            {
            }
        }

        /// <summary>
        /// Specialized version of the string list attribute which represents <br/>
        /// the list of the possible sources of the segments for the sub-account <br/>
        /// defaulting in the AP transactions. <br/>
        /// </summary>
		public class ClassListAttribute : CustomListAttribute
        {
            public ClassListAttribute()
                : base(new string[] { MaskLocation, MaskItem, MaskEmployee, MaskDeferralCode, MaskSalesPerson }, new string[] { PXAccess.FeatureInstalled<FeaturesSet.accountLocations>() ? AR.Messages.MaskLocation : AR.Messages.MaskCustomer,
                    DR.Messages.MaskItem,
                    AR.Messages.MaskEmployee,
                    DR.Messages.MaskDeferralCode,
                    AR.Messages.MaskSalesPerson })
            {
            }

            public override void CacheAttached(PXCache sender)
            {
                _AllowedValues = new[] { MaskLocation, MaskItem, MaskEmployee, MaskDeferralCode, MaskSalesPerson };
                _AllowedLabels = new[] { PXAccess.FeatureInstalled<FeaturesSet.accountLocations>() ? AR.Messages.MaskLocation : AR.Messages.MaskCustomer,
                    DR.Messages.MaskItem,
                    AR.Messages.MaskEmployee,
                    DR.Messages.MaskDeferralCode,
                    AR.Messages.MaskSalesPerson };
                _NeutralAllowedLabels = _AllowedLabels;

                base.CacheAttached(sender);
            }
        }
        public const string MaskLocation = "L";
        public const string MaskItem = "I";
        public const string MaskEmployee = "E";
        public const string MaskDeferralCode = "D";
        public const string MaskSalesPerson = "S";    
    }
    #endregion

    [PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = SubAccountAttribute.DimensionName)]
    [SubAccountARList.ClassList]
	public sealed class SubAccountMaskARAttribute : AcctSubAttribute
	{
		private string _DimensionName = "SUBACCOUNT";
		private string _MaskName = "DRCODEAR";

		public SubAccountMaskARAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, SubAccountARList.MaskDeferralCode, new SubAccountARList.ClassListAttribute().AllowedValues, new SubAccountARList.ClassListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            base.GetSubscriber<ISubscriber>(subscribers);
            subscribers.Remove(_Attributes.OfType<SubAccountARList.ClassListAttribute>().FirstOrDefault() as ISubscriber);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var stringlist = (SubAccountARList.ClassListAttribute)_Attributes.First(x => x.GetType() == typeof(SubAccountARList.ClassListAttribute));
            var dimensionmaskattribute = (PXDimensionMaskAttribute)_Attributes.First(x => x.GetType() == typeof(PXDimensionMaskAttribute));
            dimensionmaskattribute.SynchronizeLabels(stringlist.AllowedValues, stringlist.AllowedLabels);
        }

        public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new SubAccountARList.ClassListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new SubAccountARList.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

    #region DRAcctSubDefault
    public class DRAcctSubDefault
    {
        public class CustomListAttribute : PXStringListAttribute
        {
            public string[] AllowedValues
            {
                get
                {
                    return _AllowedValues;
                }
            }

            public string[] AllowedLabels
            {
                get
                {
                    return _AllowedLabels;
                }
            }

            public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels)
                : base(AllowedValues, AllowedLabels)
            {
            }
        }

        /// <summary>
        /// Specialized version of the string list attribute which represents <br/>
        /// the list of the possible sources of the segments for the sub-account <br/>
        /// defaulting in the AP transactions. <br/>
        /// </summary>
		public class ClassListAttribute : CustomListAttribute
        {
            public ClassListAttribute()
                : base(new string[] { MaskLocation, MaskItem, MaskEmployee, MaskDeferralCode }, new string[] { PXAccess.FeatureInstalled<FeaturesSet.accountLocations>() ? AP.Messages.MaskLocation : AP.Messages.MaskVendor,
                    DR.Messages.MaskItem,
                    AP.Messages.MaskEmployee,
                    DR.Messages.MaskDeferralCode })
            {
            }

            public override void CacheAttached(PXCache sender)
            {
                _AllowedValues = new[] { MaskLocation, MaskItem, MaskEmployee, MaskDeferralCode };
                _AllowedLabels = new[] { PXAccess.FeatureInstalled<FeaturesSet.accountLocations>() ? AP.Messages.MaskLocation : AP.Messages.MaskVendor,
                    DR.Messages.MaskItem,
                    AP.Messages.MaskEmployee,
                    DR.Messages.MaskDeferralCode };
                _NeutralAllowedLabels = _AllowedLabels;

                base.CacheAttached(sender);
            }
        }
        public const string MaskLocation = "L";
        public const string MaskItem = "I";
        public const string MaskEmployee = "E";
        public const string MaskDeferralCode = "D";
    }

#endregion

    [PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = SubAccountAttribute.DimensionName)]
    [DRAcctSubDefault.ClassList]
	public sealed class SubAccountMaskAPAttribute : AcctSubAttribute
	{
		private string _DimensionName = "SUBACCOUNT";
		private string _MaskName = "DRCODEAP";

		public SubAccountMaskAPAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, DRAcctSubDefault.MaskDeferralCode, new DRAcctSubDefault.ClassListAttribute().AllowedValues, new DRAcctSubDefault.ClassListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            base.GetSubscriber<ISubscriber>(subscribers);
            subscribers.Remove(_Attributes.OfType<DRAcctSubDefault.ClassListAttribute>().FirstOrDefault() as ISubscriber);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var stringlist = (DRAcctSubDefault.ClassListAttribute)_Attributes.First(x => x.GetType() == typeof(DRAcctSubDefault.ClassListAttribute));
            var dimensionmaskattribute = (PXDimensionMaskAttribute)_Attributes.First(x => x.GetType() == typeof(PXDimensionMaskAttribute));
            dimensionmaskattribute.SynchronizeLabels(stringlist.AllowedValues, stringlist.AllowedLabels);
        }

        public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new DRAcctSubDefault.ClassListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new DRAcctSubDefault.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	public class DeferralAccountSource
	{
		public const string Item = "I";
		public const string DeferralCode = "D";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { Item, DeferralCode },
					new string[] { Messages.MaskItem, Messages.MaskDeferralCode })
			{
			}
		}
	}
}
