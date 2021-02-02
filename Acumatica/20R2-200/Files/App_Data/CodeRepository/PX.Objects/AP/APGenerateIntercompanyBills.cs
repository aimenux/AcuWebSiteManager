using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;

using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.CA;
using PX.Objects.AR;
using PX.Objects.PM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.CM.Extensions;

using PX.Objects.AR.GraphExtensions;
using PX.Data.Update.ExchangeService;
using PX.Objects.Common.Attributes;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.AP
{
	[TableAndChartDashboardType]
	public class APGenerateIntercompanyBills : PXGraph<APGenerateIntercompanyBills>
	{
		public PXFilter<APGenerateIntercompanyBillsFilter> Filter;
		public PXCancel<APGenerateIntercompanyBillsFilter> Cancel;
		public MigrationMode.APSetupNoMigrationMode apsetup;

		[PXFilterable]
		public PXFilteredProcessing<ARDocumentForAPDocument, APGenerateIntercompanyBillsFilter> Documents;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.interBranch>();
		}

		public APGenerateIntercompanyBills()
		{
			// Acuminator disable once PX1085 DatabaseQueriesInPXGraphInitialization [Justification]
			APSetup setup = apsetup.Current;

			PXUIFieldAttribute.SetEnabled(Documents.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARDocumentForAPDocument.selected>(Documents.Cache, null, true);

			Documents.Cache.AllowInsert = false;
			Documents.Cache.AllowDelete = false;

			Documents.SetSelected<ARDocumentForAPDocument.selected>();
			Documents.SetProcessCaption(IN.Messages.Process);
			Documents.SetProcessAllCaption(IN.Messages.ProcessAll);

			bool _hasProject = PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
			PXUIFieldAttribute.SetVisible<APGenerateIntercompanyBillsFilter.copyProjectInformationto>(Filter.Cache, null, _hasProject);
			PXUIFieldAttribute.SetVisible<APGenerateIntercompanyBillsFilter.projectID>(Filter.Cache, null, _hasProject);
		}

		protected void GenerateIntercompanyBill(ARInvoiceEntry arInvoiceEntryGraph, ARDocumentForAPDocument arInvoice, APGenerateIntercompanyBillsFilter filter)
		{
			var generateBillExt = arInvoiceEntryGraph.GetExtension<GenerateIntercompanyBillExtension>();
			var apInvoiceEntryGraph = PXGraph.CreateInstance<APInvoiceEntry>();

			var parameters = generateBillExt.generateBillParameters.Current;
			parameters.CreateOnHold = filter.CreateOnHold == true;
			parameters.FinPeriodID = filter.CreateInSpecificPeriod == true ? Filter.Current.FinPeriodID : null;
			parameters.CopyProjectInformation = filter.CopyProjectInformation == true;
			parameters.MassProcess = true;

			PXProcessing<ARDocumentForAPDocument>.SetCurrentItem(arInvoice);

			arInvoiceEntryGraph.Document.Current = arInvoiceEntryGraph.Document.Search<ARInvoice.refNbr>(arInvoice.RefNbr, arInvoice.DocType);
			if (arInvoiceEntryGraph.CurrentDocument?.Current?.NoteID != null)
			{
				APInvoice apdoc = generateBillExt.GenerateIntercompanyBill(apInvoiceEntryGraph, arInvoiceEntryGraph.CurrentDocument?.Current, parameters);
				apInvoiceEntryGraph.Save.Press();

				if (apdoc != null)
				{
					arInvoice.IntercompanyAPDocumentNoteID = apdoc.NoteID;
				}

				ProcessingResult result = generateBillExt.CheckGeneratedAPDocument(arInvoiceEntryGraph.CurrentDocument?.Current, apdoc);
				if (string.IsNullOrEmpty(PXProcessing<ARDocumentForAPDocument>.GetItemMessage()?.Message))
				{
					if (!result.IsSuccess)
					{
						PXProcessing<ARDocumentForAPDocument>.SetError(result.GeneralMessage);
					}
					else if(result.HasWarning)
					{
						PXProcessing<ARDocumentForAPDocument>.SetWarning(result.GeneralMessage);
					}
					else
					{
						PXProcessing<ARDocumentForAPDocument>.SetProcessed();
					}
				}
			}
		}

		#region Events
		public void _(Events.RowSelected<APGenerateIntercompanyBillsFilter> e)
		{
			APGenerateIntercompanyBillsFilter filter = e.Row;

			PXUIFieldAttribute.SetEnabled<APGenerateIntercompanyBillsFilter.finPeriodID>(e.Cache, filter, filter.CreateInSpecificPeriod == true);
			
			Documents.SetProcessDelegate<ARInvoiceEntry>(
				delegate (ARInvoiceEntry arInvoiceEntry, ARDocumentForAPDocument doc)
				{
					GenerateIntercompanyBill(arInvoiceEntry, doc, filter);
				}
			);

			const int maxAPDocumentsCount = 1000;
			Documents.SetParametersDelegate(delegate (List<ARDocumentForAPDocument> list)
			{
				bool processing = true;
				if (PX.Common.PXContext.GetSlot<PX.SM.AUSchedule>() == null && list.Count > maxAPDocumentsCount)
				{
					string msg = "The process will process more than 1000 items. It will take time.";//TODO: move the string to constants
					WebDialogResult dialogResult = Documents.Ask(msg, MessageButtons.OKCancel);
					processing = dialogResult == WebDialogResult.OK;
				}
				return processing;
			});
		}

		public void _(Events.FieldUpdating<APGenerateIntercompanyBillsFilter, APGenerateIntercompanyBillsFilter.createInSpecificPeriod> e)
		{
			if((bool?)e.NewValue == true && e.NewValue != e.OldValue 
				&& e.Row.CustomerID == null && !PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>())
			{
				e.NewValue = false;
				PXUIFieldAttribute.SetError<APGenerateIntercompanyBillsFilter.customerID>(e.Cache, e.Row, Messages.PurchaserCannotBeEmpty);
			}
		}

		public void _(Events.FieldUpdated<APGenerateIntercompanyBillsFilter, APGenerateIntercompanyBillsFilter.createInSpecificPeriod> e)
		{
			if((bool?)e.NewValue == false && e.NewValue != e.OldValue)
			{
				e.Row.FinPeriodID = null;
			}
		}
		
		public void _(Events.FieldUpdated<APGenerateIntercompanyBillsFilter, APGenerateIntercompanyBillsFilter.customerID> e)
		{
			if (e.NewValue == null && e.NewValue != e.OldValue && !PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>())
			{
				e.Row.CreateInSpecificPeriod = false;
				e.Row.FinPeriodID = null;
			}
		}
		#endregion

		public virtual IEnumerable documents(PXAdapter adapter)
		{
			PXView view = GetInvoicesSelectCommand().View;

			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = view.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		protected virtual PXSelectBase<ARDocumentForAPDocument> GetInvoicesSelectCommand()
		{
			APGenerateIntercompanyBillsFilter filter = Filter.Current;
			PXSelectBase<ARDocumentForAPDocument> select = 	new PXSelect<ARDocumentForAPDocument, Where<Match<Current<AccessInfo.userName>>>>(this);

			if (filter.Date != null)
			{
				select.WhereAnd<Where<ARDocumentForAPDocument.docDate, LessEqual<Current<APGenerateIntercompanyBillsFilter.date>>>>();
			}
			if (filter.CustomerID != null) 
			{
				select.WhereAnd<Where<ARDocumentForAPDocument.customerID, Equal<Current<APGenerateIntercompanyBillsFilter.customerID>>>>();
			}
			if (filter.VendorID != null)
			{
				select.WhereAnd<Where<ARDocumentForAPDocument.vendorID, Equal<Current<APGenerateIntercompanyBillsFilter.vendorID>>>>();
			}
			if (filter.ProjectID != null)
			{
				select.WhereAnd<Where<ARDocumentForAPDocument.projectID, Equal<Current<APGenerateIntercompanyBillsFilter.projectID>>>>();
			}
			return select;
		}

		#region viewARDocument
		public PXAction<APGenerateIntercompanyBillsFilter> viewARDocument;

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewARDocument(PXAdapter adapter)
		{
			ARDocumentForAPDocument doc = Documents.Current;
			if (doc != null)
			{
				ARInvoiceEntry arInvoiceEntryGraph = PXGraph.CreateInstance<ARInvoiceEntry>();
				arInvoiceEntryGraph.Document.Current = arInvoiceEntryGraph.Document.Search<ARInvoice.refNbr>(doc.RefNbr, doc.DocType);
				PXRedirectHelper.TryRedirect(arInvoiceEntryGraph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return Filter.Select();
		}

		public PXAction<APGenerateIntercompanyBillsFilter> viewAPDocument;

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewAPDocument(PXAdapter adapter)
		{
			ARDocumentForAPDocument doc = Documents.Current;
			if (doc != null && doc.IntercompanyAPDocumentNoteID != null)
			{
				EntityHelper entityHelper = new EntityHelper(this);
				entityHelper.NavigateToRow(doc.IntercompanyAPDocumentNoteID, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}
		#endregion

		public override bool IsDirty { get { return false; }}
	}

	public class PurchaserOrganizationID<CustomerID> : BqlFormulaEvaluator<CustomerID>
		where CustomerID : IBqlField
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			int? customerID = (int?)parameters[typeof(CustomerID)];
			object result = PXAccess.GetParentOrganizationID(cache.Graph.Accessinfo.BranchID);
			if (!PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>()
				&& customerID != null)
			{
				Branch purchaserBranch = SelectFrom<Branch>
					.Where<Branch.bAccountID.IsEqual<@P.AsInt>>
					.View
					.SelectSingleBound(cache.Graph, null, customerID);

				result = purchaserBranch?.OrganizationID ?? result;
			}
			return result;
		}
	}

	[Serializable]
	[PXHidden]
	public partial class APGenerateIntercompanyBillsFilter : IBqlTable
	{
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		[PXDate()]
		[PXUnboundDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? Date { get; set; }
		#endregion

		#region VendorID 
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[VendorActive(DisplayName = "Seller", Visibility = PXUIVisibility.SelectorVisible, Required = false, DescriptionField = typeof(Vendor.acctName), Filterable = true)]
		[PXRestrictor(typeof(Where<Vendor.isBranch, Equal<True>>), "It is not Seller", typeof(Vendor.acctCD))]
		public virtual int? VendorID { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[CustomerActive(DisplayName = "Purchaser", Visibility = PXUIVisibility.SelectorVisible, Required = false, DescriptionField = typeof(Customer.acctName), Filterable = true)]
		[PXRestrictor(typeof(Where<Customer.isBranch, Equal<True>>), "It is not Purchaser", typeof(Customer.acctCD))]
		[PXDefault(typeof(SearchFor<Customer.bAccountID>
			.In<SelectFrom<Customer>
				.InnerJoin<Branch>
					.On<Customer.bAccountID.IsEqual<Branch.bAccountID>>
				.Where<Branch.branchID.IsEqual<AccessInfo.branchID.FromCurrent>
					.And<Customer.status.IsNull
						.Or<Customer.status.IsEqual<BAccount.status.active>>
						.Or<Customer.status.IsEqual<BAccount.status.oneTime>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? CustomerID { get; set; }
		#endregion

		#region CreateOnHold
		public abstract class createOnHold : PX.Data.BQL.BqlBool.Field<createOnHold> { }
		[PXBool()]
		[PXUnboundDefault(true)]
		[PXUIField(DisplayName = "Create AP Documents on Hold", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? CreateOnHold { get; set; }
		#endregion

		#region CreateInSpecificPeriod
		public abstract class createInSpecificPeriod : PX.Data.BQL.BqlBool.Field<createInSpecificPeriod> { }
		[PXBool()]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Create AP Documents in Specific Period", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? CreateInSpecificPeriod { get; set; }
		#endregion

		#region OrganizationID
		public abstract class organizationID : BqlInt.Field<organizationID> { }
		[PXDBInt]
		[PXFormula(typeof(PurchaserOrganizationID<customerID>))]
		public int? OrganizationID { get; set; }
		#endregion

		#region FinPeriod
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[APOpenPeriod(
			sourceType: null, 
			organizationSourceType: typeof(organizationID))] 
		[PXUIField(DisplayName = "Fin. Period" ,Visibility = PXUIVisibility.Visible, Required = false)]
		public virtual string FinPeriodID { get; set; }
		#endregion

		#region CopyProjectInformation
		public abstract class copyProjectInformationto : PX.Data.BQL.BqlBool.Field<copyProjectInformationto> { }
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Copy Project Information to AP Document", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? CopyProjectInformation { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[Project()]
		public virtual Int32? ProjectID { get; set; }
		#endregion
	}
	public sealed class APGenerateIntercompanyBillsFilterVisibilityRestriction : PXCacheExtension<APGenerateIntercompanyBillsFilter>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region CustomerID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public int? CustomerID { get; set; }
		#endregion
	}

	[Serializable]
	[PXProjection(typeof(Select2<ARInvoice
			, InnerJoin<ARDocumentCustomer, On<ARDocumentCustomer.customerID, Equal<ARInvoice.customerID>>
			, InnerJoin<ARDocumentVendor, On<ARDocumentVendor.branchID, Equal<ARInvoice.branchID>>
			, LeftJoin<PMProject, On<PMProject.contractID, Equal<ARInvoice.projectID>>
			, LeftJoin<IntercompanyBill, On<IntercompanyBill.intercompanyInvoiceNoteID, Equal<ARInvoice.noteID>>>>>>
		,Where<ARInvoice.released, Equal<True>,
				And<ARInvoice.voided, Equal<False>,
				And<ARInvoice.isMigratedRecord, Equal<False>,
				And<ARInvoice.isRetainageDocument, Equal<False>,
				And<ARInvoice.retainageApply, Equal<False>,
				And<ARInvoice.docType, In3<ARDocType.invoice, ARDocType.creditMemo, ARDocType.debitMemo>,
				And2<Where<ARInvoice.docType.IsNotEqual<ARDocType.creditMemo>
					.Or<ARInvoice.pendingPPD.IsNotEqual<True>>>,
				And<ARInvoice.masterRefNbr, IsNull,
				And<ARInvoice.installmentNbr, IsNull,
				And<IntercompanyBill.refNbr, IsNull,
				And<ARInvoice.isHiddenInIntercompanySales, NotEqual<True>>>>>>>>>>>>>), Persistent = false)]
	[PXHidden]
	[DebuggerDisplay("DocType = {DocType}, RefNbr = {RefNbr}")]
	public class ARDocumentForAPDocument : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck=PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion

		#region Keys
		/// <exclude/>
		public class PK : PrimaryKeyOf<ARDocumentForAPDocument>.By<docType, refNbr>
		{
			public static ARDocumentForAPDocument Find(PXGraph graph, string docType, string refNbr) => FindBy(graph, docType, refNbr);
		}
		#endregion

		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDefault]
		[PXDBString(ARRegister.docType.Length, IsKey = true, IsFixed = true, BqlField = typeof(ARInvoice.docType))]
		[ARInvoiceType.List]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual string DocType { get; set; }
		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDefault]
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(ARInvoice.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Enabled = false)]
		public virtual String RefNbr { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		/// <summary>
		/// The status of the document.
		/// The value of the field is determined by the values of the status flags,
		/// such as <see cref="Hold"/>, <see cref="Released"/>, <see cref="Voided"/>, <see cref="Scheduled"/>.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="ARDocStatus.ListAttribute"/>.
		/// Defaults to <see cref="ARDocStatus.Hold"/>.
		/// </value>
		[PXDBString(1, IsFixed = true, BqlField = typeof(ARInvoice.status))]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[ARDocStatus.List]
		public virtual string Status
		{
			get;
			set;
		}
		#endregion

		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		[PXDBDate(BqlField =typeof(ARInvoice.docDate))]
		[PXUIField(DisplayName = "Doc. Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? DocDate { get; set; }
		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		/// <summary>
		/// <see cref="PX.Objects.GL.Obsolete.FinPeriod">Financial Period</see> of the document.
		/// </summary>
		/// <value>
		/// Defaults to the period, to which the <see cref="ARRegister.DocDate"/> belongs, but can be overriden by user.
		/// </value>
		[FinPeriodID(BqlField = typeof(ARInvoice.finPeriodID))]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual String FinPeriodID
		{
			get;
			set;
		}
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXDBInt(BqlField = typeof(ARInvoice.customerID))]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible, DisplayName = "Purchaser ID")]
		public virtual Int32? CustomerID { get; set; }
		#endregion
		#region CustomerCD
		public abstract class customerCD : PX.Data.BQL.BqlString.Field<customerCD> { }
		[PXDefault()]
		[PXDBString(30, IsUnicode = true, BqlField = typeof(ARDocumentCustomer.customerCD))]
		[PXUIField(DisplayName = "Purchaser", Visibility = PXUIVisibility.Visible)]
		public virtual String CustomerCD { get; set; }
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[PXDBInt(BqlField = typeof(ARDocumentVendor.vendorID))]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible, DisplayName = "Seller ID")]
		public virtual Int32? VendorID { get; set; }
		#endregion
		#region VendorCD
		public abstract class vendorCD : PX.Data.BQL.BqlString.Field<vendorCD> { }
		[PXDBString(30, IsUnicode = true, BqlField = typeof(ARDocumentVendor.vendorCD))]
		[PXUIField(DisplayName = "Seller", Visibility = PXUIVisibility.Visible)]
		public virtual String VendorCD { get; set; }
		#endregion

		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
		[CM.PXDBBaseCury(BqlField = typeof(ARInvoice.curyOrigDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryOrigDocAmt { get; set; }
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		[PXDBString(5, BqlField = typeof(ARInvoice.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.Visible)]
		public virtual String CuryID { get; set; }
		#endregion

		#region DocDesc
		public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(ARInvoice.docDesc))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		public virtual String DocDesc { get; set; }
		#endregion

		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }

		/// <summary>
		/// The original reference number or ID assigned by the customer to the customer document.
		/// </summary>
		[PXDBString(40, IsUnicode = true, BqlField = typeof(ARInvoice.invoiceNbr))]
		[PXUIField(DisplayName = "Customer Order Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual String InvoiceNbr
		{
			get;
			set;
		}
		#endregion

		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }

		/// <summary>
		/// The identifier of the <see cref="Terms">Credit Terms</see> object associated with the document.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Customer.TermsID">credit terms</see> that are selected for the <see cref="CustomerID">customer</see>.
		/// Corresponds to the <see cref="Terms.TermsID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true, BqlField = typeof(ARInvoice.termsID))]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible, Visible =false, Enabled = false)]
		[ARTermsSelector]
		[Terms(typeof(ARInvoice.docDate), typeof(ARInvoice.dueDate), typeof(ARInvoice.discDate), typeof(ARInvoice.curyOrigDocAmt), typeof(ARInvoice.curyOrigDiscAmt))]
		public virtual string TermsID
		{
			get;
			set;
		}
		#endregion

		#region DueDate
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }

		/// <summary>
		/// The due date of the document.
		/// </summary>
		[PXDBDate(BqlField = typeof(ARInvoice.dueDate))]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual DateTime? DueDate
		{
			get;
			set;
		}
		#endregion

		#region DiscDate
		public abstract class discDate : PX.Data.BQL.BqlDateTime.Field<discDate> { }

		/// <summary>
		/// The date when the cash discount can be taken in accordance with the <see cref="ARInvoice.TermsID">credit terms</see>.
		/// </summary>
		[PXDBDate(BqlField = typeof(ARInvoice.discDate))]
		[PXUIField(DisplayName = "Cash Discount Date", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual DateTime? DiscDate
		{
			get;
			set;
		}
		#endregion

		#region CuryDocBal
		public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }

		/// <summary>
		/// The open balance of the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBDecimal(BqlField = typeof(ARInvoice.curyDiscBal))]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual Decimal? CuryDocBal
		{
			get;
			set;
		}
		#endregion

		#region CuryTaxTotal
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }
		/// <summary>
		/// The total amount of tax associated with the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBDecimal(BqlField = typeof(ARInvoice.curyTaxTotal))]
		[PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual Decimal? CuryTaxTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryOrigDiscAmt
		public abstract class curyOrigDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDiscAmt> { }

		/// <summary>
		/// The cash discount entered for the document.
		/// Given in the <see cref="CuryID">currency of the document</see>.
		/// </summary>
		[PXDBDecimal(BqlField = typeof(ARInvoice.curyOrigDiscAmt))]
		[PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual Decimal? CuryOrigDiscAmt
		{
			get;
			set;
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(BqlField = typeof(ARInvoice.projectID))]
		[PXUIField(DisplayName = "Project ID", Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? ProjectID { get; set; }
		#endregion
		#region ProjectCD
		public abstract class projectCD : PX.Data.BQL.BqlString.Field<projectCD> { }
		[PXDBString(30, IsUnicode = true, BqlField = typeof(PMProject.contractCD))]
		[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible)]
		public virtual String ProjectCD { get; set; }
		#endregion

		#region IntercompanyInvoiceNoteID
		public abstract class intercompanyAPDocumentNoteID : BqlGuid.Field<intercompanyAPDocumentNoteID> { }

		[PXDBGuid(BqlField = typeof(IntercompanyBill.intercompanyInvoiceNoteID))]
		[PXUIField(DisplayName = "AP Document", FieldClass = nameof(FeaturesSet.InterBranch), Enabled = false)]
		[DocumentSelector(typeof(SearchFor<APInvoice.noteID>.In<SelectFrom<APInvoice>>),
			SubstituteKey = typeof(APInvoice.documentKey),
			SelectorMode = PXSelectorMode.DisplayModeValue | PXSelectorMode.TextModeReadonly)]
		public virtual Guid? IntercompanyAPDocumentNoteID { get; set; }
		#endregion
	}

	[PXHidden]
	[PXProjection(typeof(Select<BAccount2, 
		Where<BAccount2.isBranch, Equal<True>, 
			And<BAccount2.type, In3<BAccountType.customerType, BAccountType.combinedType>>>>), Persistent = false)]
	public class ARDocumentCustomer : IBqlTable
	{
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXDBInt(BqlField = typeof(BAccount.bAccountID))]
		public virtual Int32? CustomerID { get; set; }
		#endregion
		#region CustomerCD
		public abstract class customerCD : PX.Data.BQL.BqlString.Field<customerCD> { }
		[PXDBString(30, IsUnicode = true, BqlField = typeof(BAccount.acctCD))]
		public virtual String CustomerCD { get; set; }
		#endregion
	}

	[PXHidden]
	[PXProjection(typeof(Select2<BranchAlias
			,InnerJoin<BAccount2, 
				On<BAccount2.bAccountID, Equal<BranchAlias.bAccountID>, 
					And<BAccount2.type, In3<BAccountType.vendorType, BAccountType.combinedType>, 
					And<BAccount2.isBranch, Equal<True>>>>>>), Persistent = false)]
	public class ARDocumentVendor : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[PXDBInt(BqlField = typeof(BranchAlias.branchID))]
		public virtual Int32? BranchID { get; set; }
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		[PXDBInt(BqlField = typeof(BranchAlias.bAccountID))]
		public virtual Int32? VendorID { get; set; }
		#endregion
		#region VendorCD
		public abstract class vendorCD : PX.Data.BQL.BqlString.Field<vendorCD> { }
		[PXDBString(30, IsUnicode = true, BqlField = typeof(BAccount.acctCD))]
		public virtual String VendorCD { get; set; }
		#endregion
	}

	[Serializable]
	[PXHidden]
	public class IntercompanyBill : APInvoice
	{
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		public new abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		public new abstract class intercompanyInvoiceNoteID : PX.Data.BQL.BqlString.Field<intercompanyInvoiceNoteID> { }
	}
}
