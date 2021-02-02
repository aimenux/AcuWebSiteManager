using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.TX;
using POLine = PX.Objects.PO.POLine;
using POOrder = PX.Objects.PO.POOrder;
using System.Threading.Tasks;
using PX.CarrierService;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using ARRegisterAlias = PX.Objects.AR.Standalone.ARRegisterAlias;
using PX.Objects.AR.MigrationMode;
using PX.Objects.Common;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Extensions;
using PX.Objects.IN.Overrides.INDocumentRelease;
using PX.CS.Contracts.Interfaces;
using Message = PX.CarrierService.Message;
using PX.TaxProvider;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.Extensions.PaymentTransaction;
using PX.Objects.SO.GraphExtensions.CarrierRates;
using PX.Objects.SO.Attributes;
using PX.Objects.Common.Bql;

namespace PX.Objects.SO
{
	[Serializable]
	public class SOOrderEntry : PXGraph<SOOrderEntry, SOOrder>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess, IGraphWithInitialization
	{
		//[Serializable]
		//public partial class CCProcTranV : CCProcTran
		//{
		//	#region TranNbr
		//	public new abstract class tranNbr : PX.Data.BQL.BqlInt.Field<tranNbr> { }
		//	#endregion
		//	#region PMInstanceID
		//	public new abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		//	#endregion
		//	#region ProcessingCenterID
		//	public new abstract class processingCenterID : PX.Data.BQL.BqlString.Field<processingCenterID> { }
		//	#endregion
		//	#region DocType
		//	public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		//	#endregion
		//	#region RefNbr
		//	public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		//	#endregion
		//	#region TranType
		//	#endregion
		//	#region ProcStatus
		//	public new abstract class procStatus : PX.Data.BQL.BqlString.Field<procStatus> { }
		//	#endregion
		//	#region TranStatus
		//	public new abstract class tranStatus : PX.Data.BQL.BqlString.Field<tranStatus> { }
		//	#endregion
		//	#region RefTranNbr
		//	public new abstract class refTranNbr : PX.Data.BQL.BqlInt.Field<refTranNbr> { }

		//	#endregion
		//}


		private DiscountEngine<SOLine, SOOrderDiscountDetail> _discountEngine => DiscountEngineProvider.GetEngineFor<SOLine, SOOrderDiscountDetail>();

		[PXHidden]
		public PXSelect<BAccount> bAccountBasic;

		[PXHidden]
		public PXSelect<BAccountR> bAccountRBasic;

		public ToggleCurrency<SOOrder> CurrencyView;
		public PXFilter<Vendor> _Vendor;

		/// <summary>
		/// If true the SO-PO Link dialog will display PO Orders on hold.
		/// </summary>
		/// <remarks>This setting is used when linking On-Hold PO Orders with SO created through RQRequisitionEntry.</remarks>
		public bool SOPOLinkShowDocumentsOnHold { get; set; }

		#region Selects
		[PXViewName(Messages.SOOrder)]
		public PXSelectJoin<SOOrder,
			LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<SOOrder.customerID>>>,
			Where<SOOrder.orderType, Equal<Optional<SOOrder.orderType>>,
			And<Where<Customer.bAccountID, IsNull,
			Or<Match<Customer, Current<AccessInfo.userName>>>>>>> Document;
		[PXCopyPasteHiddenFields(typeof(SOOrder.cancelled), typeof(SOOrder.preAuthTranNumber), typeof(SOOrder.ownerID), typeof(SOOrder.workgroupID))]
		public PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> CurrentDocument;

		public PXSelect<RUTROT.RUTROT, Where<True, Equal<False>>> Rutrots;
		public PXSelect<RQ.RQRequisitionOrder> rqrequisitionorder;
		public PXSelect<RQ.RQRequisition, Where<RQ.RQRequisition.reqNbr, Equal<Required<RQ.RQRequisition.reqNbr>>>> rqrequisition;

		[PXViewName(Messages.SOLine)]
		[PXImport(typeof(SOOrder))]
		[PXCopyPasteHiddenFields(typeof(SOLine.completed))]
		public PXOrderedSelect<SOOrder, SOLine,
			Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
				And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>>>,
			OrderBy<Asc<SOLine.orderType, Asc<SOLine.orderNbr, Asc<SOLine.sortOrder, Asc<SOLine.lineNbr>>>>>> Transactions;

		public PXSelectReadonly<INItemCost, Where<INItemCost.inventoryID, Equal<Optional<SOLine.inventoryID>>>> initemcost;

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		protected virtual IEnumerable transactions()
		{
			if (sosetup.Current.MinGrossProfitValidation == MinGrossProfitValidationType.None || IsTransferOrder)
				return null;

			PXSelectBase<SOLine> query =
				new PXSelectReadonly2<SOLine,
						InnerJoin<INItemCost, On<INItemCost.inventoryID, Equal<SOLine.inventoryID>>>,
						Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
							And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>>>,
						OrderBy<Asc<SOLine.orderType, Asc<SOLine.orderNbr, Asc<SOLine.sortOrder, Asc<SOLine.lineNbr>>>>>>(this);

			using (new PXFieldScope(query.View, typeof(INItemCost)))
			{
			int startRow = PXView.StartRow;
			int totalRows = 0;
			foreach (PXResult<SOLine, INItemCost> record in query.View.Select(
				PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns,
				PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				INItemCost itemCost = (INItemCost)record;

					initemcost.StoreCached(new PXCommandKey(new object[] { itemCost.InventoryID }), new List<object> { itemCost });
			}
			}
			return null;
		}

		public PXSelect<ExternalTransaction,
			Where<ExternalTransaction.origRefNbr, Equal<Current<SOOrder.orderNbr>>,
				And<ExternalTransaction.origDocType, Equal<Current<SOOrder.orderType>>>>,
			OrderBy<Desc<ExternalTransaction.transactionID>>> ExternalTran;

		public PXSelectOrderBy<CCProcTran, OrderBy<Desc<CCProcTran.tranNbr>>> ccProcTran;
		public IEnumerable CcProcTran()
		{
			var externalTrans = ExternalTran.Select();
			var query = new PXSelect<CCProcTran,
				Where<CCProcTran.transactionID, Equal<Required<CCProcTran.transactionID>>>>(this);
			foreach (ExternalTransaction extTran in externalTrans)
			{
				foreach (CCProcTran procTran in query.Select(extTran.TransactionID))
				{
					yield return procTran;
				}
			}
		}

		public PXSelect<INSiteStatus> sitestatusview_dummy;
		public PXSelect<SiteStatus> sitestatusview;
		public PXSelect<INItemSite> initemsite;
		public PXSelect<SOTax, Where<SOTax.orderType, Equal<Current<SOOrder.orderType>>, And<SOTax.orderNbr, Equal<Current<SOOrder.orderNbr>>>>, OrderBy<Asc<SOTax.orderType, Asc<SOTax.orderNbr, Asc<SOTax.taxID>>>>> Tax_Rows;
		public PXSelectJoin<SOTaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<SOTaxTran.taxID>>>,
				Where<SOTaxTran.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOTaxTran.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> Taxes;
		public PXSelectJoin<SOOrderShipment, LeftJoin<SOShipment, On<SOShipment.shipmentNbr, Equal<SOOrderShipment.shipmentNbr>, And<SOShipment.shipmentType, Equal<SOOrderShipment.shipmentType>>>>, Where<SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> shipmentlist;
		public PXSelect<INItemSiteSettings, Where<INItemSiteSettings.inventoryID, Equal<Required<INItemSiteSettings.inventoryID>>, And<INItemSiteSettings.siteID, Equal<Required<INItemSiteSettings.siteID>>>>> initemsettings;

		public PXSelect<ARRegister> arregister;

		[PXViewName(Messages.BillingAddress)]
		public PXSelect<SOBillingAddress, Where<SOBillingAddress.addressID, Equal<Current<SOOrder.billAddressID>>>> Billing_Address;
		[PXViewName(Messages.ShippingAddress)]
		public PXSelect<SOShippingAddress, Where<SOShippingAddress.addressID, Equal<Current<SOOrder.shipAddressID>>>> Shipping_Address;
		[PXViewName(Messages.BillingContact)]
		public PXSelect<SOBillingContact, Where<SOBillingContact.contactID, Equal<Current<SOOrder.billContactID>>>> Billing_Contact;
		[PXViewName(Messages.ShippingContact)]
		public PXSelect<SOShippingContact, Where<SOShippingContact.contactID, Equal<Current<SOOrder.shipContactID>>>> Shipping_Contact;

		public PXSelect<SOSetupApproval, Where<SOSetupApproval.orderType, Equal<Optional<SOOrder.orderType>>>> SetupApproval;

		[PXViewName(Messages.Approval)]
		public EPApprovalAutomation<SOOrder, SOOrder.approved, SOOrder.rejected, SOOrder.hold, SOSetupApproval> Approval;

		[PXCopyPasteHiddenView()]
		public PlanningHelper<SOLineSplit> Planning;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOOrder.curyInfoID>>>> currencyinfo;

		public PXSetup<ARSetup> arsetup;
		[PXViewName(AR.Messages.Customer)]
		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<SOOrder.customerID>>>> customer;
		public PXSetup<CustomerClass, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>> customerclass;
		public PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<SOOrder.taxZoneID>>>> taxzone;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>, And<Location.locationID, Equal<Optional<SOOrder.customerLocationID>>>>> location;
		public PXSelect<ARBalances> arbalances;
		public PXSetup<SOOrderType, Where<SOOrderType.orderType, Equal<Optional<SOOrder.orderType>>>> soordertype;
		public PXSetup<SOOrderTypeOperation,
			Where<SOOrderTypeOperation.orderType, Equal<Optional<SOOrderType.orderType>>,
			And<SOOrderTypeOperation.operation, Equal<Optional<SOOrderType.defaultOperation>>>>> sooperation;

		[PXCopyPasteHiddenView()]
		[PXFilterable]
		public PXSelect<SOLineSplit, Where<SOLineSplit.orderType, Equal<Current<SOLine.orderType>>, And<SOLineSplit.orderNbr, Equal<Current<SOLine.orderNbr>>, And<SOLineSplit.lineNbr, Equal<Current<SOLine.lineNbr>>>>>> splits;
		public PXSelect<SOLine, Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>, And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>, And<SOLine.isFree, Equal<boolTrue>>>>, OrderBy<Asc<SOLine.orderType, Asc<SOLine.orderNbr, Asc<SOLine.lineNbr>>>>> FreeItems;

		public PXSelect<SOOrderDiscountDetail, Where<SOOrderDiscountDetail.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderDiscountDetail.orderNbr, Equal<Current<SOOrder.orderNbr>>>>, OrderBy<Asc<SOOrderDiscountDetail.lineNbr>>> DiscountDetails;

		public PXSetup<INSetup> insetup;
		public PXSetup<SOSetup> sosetup;
		public PXSetup<Branch, 
			InnerJoin<INSite, 
				On<INSite.branchID, Equal<Branch.branchID>>>, 
			Where<INSite.siteID, Equal<Optional<SOOrder.destinationSiteID>>>> Company;
		public PXSetupOptional<CommonSetup> commonsetup;

		public PXSelect<CurrencyInfo> DummyCuryInfo;
		public LSSOLine lsselect;
		public PMCommitmentSelect pmselect;

		public PXFilter<SOParamFilter> soparamfilter;
		public PXFilter<AddInvoiceFilter> addinvoicefilter;
		public PXFilter<CopyParamFilter> copyparamfilter;
		public PXFilter<RecalcDiscountsParamFilter> recalcdiscountsfilter;

		public PXSelect<INTranSplit> intransplit;

		public PXFilter<InvoiceSplits> invoicesplits;

		public PXSelect<SOLine, Where<SOLine.orderType, Equal<Optional<SOLine.orderType>>, And<SOLine.orderNbr, Equal<Optional<SOLine.orderNbr>>, And<SOLine.lineNbr, Equal<Optional<SOLine.lineNbr>>>>>> currentposupply;
		[PXCopyPasteHiddenView()]
		public PXSelect<POLine3> posupply;
		[PXCopyPasteHiddenView()]
		public PXSelect<POOrder> poorderlink;

		[PXCopyPasteHiddenView()]
		public PXSelectJoin<SOLineSplit2, 
			LeftJoin<INItemPlan, 
				On<INItemPlan.planID, Equal<SOLineSplit2.planID>>>, 
			Where<SOLineSplit2.sOOrderType, Equal<Optional<SOLineSplit.orderType>>, 
				And<SOLineSplit2.sOOrderNbr, Equal<Optional<SOLineSplit.orderNbr>>, 
				And<SOLineSplit2.sOLineNbr, Equal<Optional<SOLineSplit.lineNbr>>, 
				And<SOLineSplit2.sOSplitLineNbr, Equal<Optional<SOLineSplit.splitLineNbr>>>>>>> sodemand;

		[CRReference(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<SOOrder.customerID>>>>))]
		[CRDefaultMailTo(typeof(Select<SOShippingContact, Where<SOShippingContact.contactID, Equal<Current<SOOrder.shipContactID>>>>))]
		public CRActivityList<SOOrder>
			Activity;

		public PXSelect<SOSalesPerTran, Where<SOSalesPerTran.orderType, Equal<Current<SOOrder.orderType>>, And<SOSalesPerTran.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> SalesPerTran;
		public PXSelect<SOOrderSite, Where<SOOrderSite.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderSite.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> SiteList;

		public PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> DocumentProperties;
		[PXCopyPasteHiddenView]
		public PXSelect<SOPackageInfoEx, Where<SOPackageInfoEx.orderType, Equal<Current<SOOrder.orderType>>, And<SOPackageInfoEx.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> Packages;

		public PXSelect<INReplenishmentOrder> Replenihment;
		[Obsolete(Common.Messages.ViewIsObsoleteAndWillBeRemoved2020R1 + " Please use ReplenishmentLinesWithPlans instead.")]
		public PXSelect<INReplenishmentLine,
			Where<INReplenishmentLine.sOType, Equal<Current<SOLine.orderType>>,
				And<INReplenishmentLine.sONbr, Equal<Current<SOLine.orderNbr>>,
				And<INReplenishmentLine.sOLineNbr, Equal<Current<SOLine.lineNbr>>>>>> ReplenishmentLines;

		public PXSelectJoin<INReplenishmentLine,
			InnerJoin<INItemPlan, On<INItemPlan.planID, Equal<INReplenishmentLine.planID>>>,
			Where<INReplenishmentLine.sOType, Equal<Current<SOOrder.orderType>>,
				And<INReplenishmentLine.sONbr, Equal<Current<SOOrder.orderNbr>>>>> ReplenishmentLinesWithPlans;

		[PXCopyPasteHiddenView()]
		public PXSelectJoin<SOAdjust, InnerJoin<ARPayment, On<ARPayment.docType, Equal<SOAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<SOAdjust.adjgRefNbr>>>>> Adjustments;
		public PXSelectJoin<SOAdjust,
							InnerJoin<ARRegisterAlias, On<ARRegisterAlias.docType, Equal<SOAdjust.adjgDocType>, And<ARRegisterAlias.refNbr, Equal<SOAdjust.adjgRefNbr>>>,
							InnerJoinSingleTable<ARPayment, On<ARRegisterAlias.docType, Equal<ARPayment.docType>, And<ARRegisterAlias.refNbr, Equal<ARPayment.refNbr>>>,
							InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARRegisterAlias.curyInfoID>>>>>,
							Where<SOAdjust.adjdOrderType, Equal<Current<SOOrder.orderType>>,
								And<SOAdjust.adjdOrderNbr, Equal<Current<SOOrder.orderNbr>>>>> Adjustments_Raw;

		[Obsolete(Common.Messages.ViewIsObsoleteAndWillBeRemoved2020R1)]
		public PXSelectJoin<INReplenishmentLine,
			InnerJoin<INItemPlan, On<INItemPlan.planID, Equal<INReplenishmentLine.planID>>>,
			Where<INReplenishmentLine.sOType, Equal<Current<SOLine.orderType>>,
				And<INReplenishmentLine.sONbr, Equal<Current<SOLine.orderNbr>>,
					And<INReplenishmentLine.sOLineNbr, Equal<Current<SOLine.lineNbr>>>>>> replenishment;

		public PXSelect<CCProcessingCenter, Where<CCProcessingCenter.processingCenterID, Equal<Current<CustomerPaymentMethodC.cCProcessingCenterID>>>> PMProcessingCenter;

		public PXSelect<RUTROT.RUTROTDistribution,
					Where<True, Equal<False>>> RRDistribution;

		[PXViewName(CR.Messages.SalesPerson)]
		public PXSelect<EPEmployee, Where<EPEmployee.salesPersonID, Equal<Current<SOOrder.salesPersonID>>>> SalesPerson;

		[PXViewName(CR.Messages.MainContact)]
		public PXSelect<Contact> DefaultCompanyContact;



		public override void CopyPasteGetScript(bool isImportSimple, List<PX.Api.Models.Command> script, List<PX.Api.Models.Container> containers)
		{
			script.Where(_ => _.ObjectName.StartsWith("Transactions")).ForEach(_ => _.Commit = false);
			script.Where(_ => _.ObjectName.StartsWith("Transactions")).Last().Commit = true;

			// Customer Order Nbr field may raise an exception, so it should be copied at the end.
			int custOrderNbrIndex = script.FindIndex(_ => _.FieldName == nameof(SOOrder.CustomerOrderNbr));
			if (custOrderNbrIndex == -1)
				return;

			Api.Models.Command cmdCustOrderNbr = script[custOrderNbrIndex];
			Api.Models.Container cntCustOrderNbr = containers[custOrderNbrIndex];

			script.Remove(cmdCustOrderNbr);
			containers.Remove(cntCustOrderNbr);

			script.Add(cmdCustOrderNbr);
			containers.Add(cntCustOrderNbr);
		}
		protected virtual IEnumerable defaultCompanyContact()
		{
			return OrganizationMaint.GetDefaultContactForCurrentOrganization(this);
		}

		public virtual IEnumerable adjustments()
		{
			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOOrder.curyInfoID>>>>.Select(this);

			foreach (PXResult<SOAdjust, ARRegisterAlias, ARPayment, CurrencyInfo> res in Adjustments_Raw.Select())
			{
				ARPayment payment = (ARPayment)res;
				SOAdjust adj = (SOAdjust)res;
				CurrencyInfo pay_info = (CurrencyInfo)res;

				if (adj == null)
					continue;

				if (payment != null)
					PXCache<ARRegister>.RestoreCopy(payment, (ARRegisterAlias)res);

				SOAdjust other = PXSelectGroupBy<SOAdjust, Where<SOAdjust.adjgDocType, Equal<Required<SOAdjust.adjgDocType>>, And<SOAdjust.adjgRefNbr, Equal<Required<SOAdjust.adjgRefNbr>>, And<Where<SOAdjust.adjdOrderType, NotEqual<Required<SOAdjust.adjdOrderType>>, Or<SOAdjust.adjdOrderNbr, NotEqual<Required<SOAdjust.adjdOrderNbr>>>>>>>, Aggregate<GroupBy<SOAdjust.adjgDocType, GroupBy<SOAdjust.adjgRefNbr, Sum<SOAdjust.curyAdjgAmt, Sum<SOAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr, adj.AdjdOrderType, adj.AdjdOrderNbr);
				if (other != null && other.AdjdOrderNbr != null)
				{
					payment.CuryDocBal -= other.CuryAdjgAmt;
					payment.DocBal -= other.AdjAmt;
				}

				ARAdjust fromar = PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<ARAdjust.released, Equal<boolFalse>>>>, Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, Sum<ARAdjust.curyAdjgAmt, Sum<ARAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr);
				if (fromar != null && fromar.AdjdRefNbr != null)
				{
					payment.CuryDocBal -= fromar.CuryAdjgAmt;
					payment.DocBal -= fromar.AdjAmt;
				}

				decimal CuryDocBal;
				if (string.Equals(pay_info.CuryID, inv_info.CuryID))
				{
					CuryDocBal = (decimal)payment.CuryDocBal;
				}
				else
				{
					decimal docBal = ((payment.Released == true) ? payment.DocBal : payment.OrigDocAmt) ?? 0m;

					PXDBCurrencyAttribute.CuryConvCury(Adjustments.Cache, inv_info, docBal, out CuryDocBal);
				}

				if (adj.CuryAdjdAmt > CuryDocBal)
				{
					//if reconsidered need to calc RGOL
					adj.CuryDocBal = CuryDocBal;
					adj.CuryAdjdAmt = 0m;
				}
				else
				{
					adj.CuryDocBal = CuryDocBal - adj.CuryAdjdAmt;
				}

				RecalcTotals(CurrentDocument.Cache, (SOOrder)CurrentDocument.Cache.Current);
				yield return res;
			}

			//if (Document.Current != null && (Document.Current.ARDocType == ARDocType.Invoice || Document.Current.ARDocType == ARDocType.DebitMemo) && Document.Current.Completed == false && Document.Current.Cancelled == false)
			if (Document.Current != null && Document.Current.ARDocType == null)
			{
				using (ReadOnlyScope rs = new ReadOnlyScope(Adjustments.Cache, Document.Cache, arbalances.Cache))
				{
					PXSelectBase<ARPayment> s = new PXSelectReadonly2<ARPayment,
						InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>,
						LeftJoin<SOAdjust, On<SOAdjust.adjgDocType, Equal<ARPayment.docType>,
							And<SOAdjust.adjgRefNbr, Equal<ARPayment.refNbr>,
							And<SOAdjust.adjdOrderType, Equal<Current<SOOrder.orderType>>,
							And<SOAdjust.adjdOrderNbr, Equal<Current<SOOrder.orderNbr>>>>>>>>,
						Where<ARPayment.customerID, In3<Current<SOOrder.customerID>, Current<Customer.consolidatingBAccountID>>,
							And2<Where<ARPayment.docType, Equal<ARDocType.payment>, Or<ARPayment.docType, Equal<ARDocType.prepayment>, Or<ARPayment.docType, Equal<ARDocType.creditMemo>>>>,
							And<ARPayment.openDoc, Equal<boolTrue>,
							And<SOAdjust.adjdOrderNbr, IsNull>>>>>(this);

					foreach (PXResult<ARPayment, CurrencyInfo> res in s.Select())
					{
						ARPayment payment = res;
						SOAdjust adj = new SOAdjust();

						adj.CustomerID = Document.Current.CustomerID;
						adj.AdjdOrderType = Document.Current.OrderType;
						adj.AdjdOrderNbr = Document.Current.OrderNbr;
						adj.AdjgDocType = payment.DocType;
						adj.AdjgRefNbr = payment.RefNbr;

						if (Adjustments.Cache.Locate(adj) == null)
						{
							yield return new PXResult<SOAdjust, ARPayment>(Adjustments.Insert(adj), payment);
						}
					}
				}
			}
		}

		public PXFilter<CustomerPaymentMethodInputMode> InputModeFilter;

		public PXSelect<CustomerPaymentMethodC,
		Where<CustomerPaymentMethodC.pMInstanceID, Equal<Optional<SOOrder.pMInstanceID>>>> DefPaymentMethodInstance;

		public PXSelectJoin<CustomerPaymentMethodDetail, InnerJoin<PaymentMethodDetail,
			On<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
				And<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
					And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
			Where<PaymentMethodDetail.isCCProcessingID, Equal<True>, And<CustomerPaymentMethodDetail.pMInstanceID, Equal<Optional<SOOrder.pMInstanceID>>>>> ccpIdDet;

		public PXSelectJoin<CustomerPaymentMethodDetail,
			  InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
			  And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
			  And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
				Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Optional<SOOrder.pMInstanceID>>>, OrderBy<Asc<PaymentMethodDetail.orderIndex>>> DefPaymentMethodInstanceDetails;

		public PXSelectJoin<CustomerPaymentMethodDetail,
			  InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<CustomerPaymentMethodDetail.paymentMethodID>,
			  And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>,
			  And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
				Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Optional<SOOrder.pMInstanceID>>>, OrderBy<Asc<PaymentMethodDetail.orderIndex>>> DefPaymentMethodInstanceDetailsAll;

		public virtual IEnumerable defPaymentMethodInstanceDetails()
		{
			if (DefPaymentMethodInstance.Current != null)
			{
				int? pmInstanceId = DefPaymentMethodInstance.Current.PMInstanceID;
				bool isTokenized = CCProcessingHelper.IsTokenizedPaymentMethod(this, pmInstanceId);
				bool isHF = CCProcessingHelper.IsHFPaymentMethod(this, pmInstanceId);
				bool showToken = InputModeFilter.Current != null && InputModeFilter.Current.InputMode == InputModeType.Token && isTokenized;
				bool showDetails = InputModeFilter.Current == null || InputModeFilter.Current.InputMode == InputModeType.Details && !isHF;
				return CCProcessingHelper.GetPMdetails(this, DefPaymentMethodInstance.Current.PMInstanceID, showToken, showDetails);
			}
			else
			{
				return null;
			}

		}

		public PXSelect<CustomerProcessingCenterID, Where<CustomerProcessingCenterID.bAccountID, Equal<Current<SOOrder.customerID>>,
			And<CustomerProcessingCenterID.cCProcessingCenterID, Equal<Required<AR.CustomerPaymentMethod.cCProcessingCenterID>>,
			And<CustomerProcessingCenterID.customerCCPID, Equal<Required<AR.CustomerPaymentMethod.customerCCPID>>>>>> CustomerProcessingID;

		public PXSelect<PaymentMethodDetail,
			Where<PaymentMethodDetail.paymentMethodID, Equal<Optional<CustomerPaymentMethodC.paymentMethodID>>,
			 And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>> PMDetails;
		public PXSelect<PaymentMethod,
		  Where<PaymentMethod.paymentMethodID, Equal<Optional<AR.CustomerPaymentMethod.paymentMethodID>>>> PaymentMethodDef;

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<CCProcTran.origDocType>>, And<SOOrder.orderNbr, Equal<Current<CCProcTran.origRefNbr>>>>>))]
		protected virtual void CCProcTran_RefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<RQ.RQRequisitionOrder.orderType>>, And<SOOrder.orderNbr, Equal<Current<RQ.RQRequisitionOrder.orderNbr>>>>>))]
		protected virtual void RQRequisitionOrder_OrderNbr_CacheAttached(PXCache sender)
		{
		}

		private struct POSupplyResult
		{
			public string POOrderType;
			public string POOrderNbr;
			public int? POLineNbr;
			public POLine3 POLine;
			public POOrder POOrder;
			public List<SOLineSplit> CurrentSOLineSplits;
			public List<SOLineSplit> ForeignSOLineSplits;
		}

		public virtual IEnumerable POSupply()
		{
			SOLine currentSOLine = (SOLine)currentposupply.Select();

			List<POLine3> ret = new List<POLine3>();
			if (currentSOLine == null) return ret;

			List<POSupplyResult> mergedResults = new List<POSupplyResult>();

			foreach (PXResult<POLine3, POOrder, SOLineSplit> res in
				PXSelectReadonly2<POLine3,
				InnerJoin<POOrder,
					On<POOrder.orderNbr, Equal<POLine3.orderNbr>, And<POOrder.orderType, Equal<POLine3.orderType>>>,
				LeftJoin<SOLineSplit,
					On<SOLineSplit.pOType, Equal<POLine3.orderType>,
				And<SOLineSplit.pONbr, Equal<POLine3.orderNbr>,
				And<SOLineSplit.pOLineNbr, Equal<POLine3.lineNbr>>>>>>,
				Where2<
					Where<Current<SOLine.pOSource>, Equal<INReplenishmentSource.purchaseToOrder>,
								And<POLine3.orderType, In3<POOrderType.regularOrder, POOrderType.blanket>,
							Or<Current<SOLine.pOSource>, Equal<INReplenishmentSource.dropShipToOrder>,
								And<Where<POLine3.orderType, Equal<POOrderType.dropShip>,
									And2<Where<Current<SOLine.customerID>, Equal<POOrder.shipToBAccountID>,
										Or<POOrder.shipDestType, NotEqual<POShippingDestination.customer>>>,
									Or<POLine3.orderType, Equal<POOrderType.blanket>>>>>>>>,
				And<POLine3.lineType, In3<POLineType.goodsForInventory,
					POLineType.nonStock,
					POLineType.goodsForDropShip,
					POLineType.nonStockForDropShip,
					POLineType.goodsForSalesOrder,
					POLineType.goodsForServiceOrder,
					POLineType.nonStockForSalesOrder,
					POLineType.nonStockForServiceOrder,
					POLineType.goodsForReplenishment>,
				And<POLine3.inventoryID, Equal<Current<SOLine.inventoryID>>,
				And2<Where<Current<SOLine.subItemID>, IsNull,
						Or<POLine3.subItemID, Equal<Current<SOLine.subItemID>>>>,
				And<POLine3.siteID, Equal<Current<SOLine.pOSiteID>>,
				And2<Where<Current<SOLine.vendorID>, IsNull,
					Or<POLine3.vendorID, Equal<Current<SOLine.vendorID>>>>,
				And<Current2<SOLine.pOSource>, IsNotNull>>>>>>>>.SelectMultiBound(this, new object[] { currentSOLine }))
			{
				POLine3 supply = PXCache<POLine3>.CreateCopy(res);
				POOrder poorder = (POOrder)this.Caches[typeof(POOrder)].CreateCopy(this.Caches[typeof(POOrder)].Locate((POOrder)res)) ?? res;
				SOLineSplit split = PXCache<SOLineSplit>.CreateCopy(res);
				SOLineSplit foreignsplit = new SOLineSplit();

				SOLineSplit selectedSplitCached = (SOLineSplit)this.Caches[typeof(SOLineSplit)].Locate((SOLineSplit)res);

				if (selectedSplitCached != null)
				{
					if (selectedSplitCached.PONbr == null || this.Caches[typeof(SOLineSplit)].GetStatus(selectedSplitCached) == PXEntryStatus.Deleted
						|| selectedSplitCached.POType != supply.OrderType || selectedSplitCached.PONbr != supply.OrderNbr || selectedSplitCached.POLineNbr != supply.LineNbr)
					{
						// Selected split found in cache, but was deleted or linked to another POLine.
						split = new SOLineSplit();
					}
					else
					{
						// Selected split found in cache, replace selected plan with cached plan.
						split = (SOLineSplit)this.Caches[typeof(SOLineSplit)].CreateCopy(selectedSplitCached);
					}
				}

				if (split.PONbr == null)
				{
					split = new SOLineSplit();
				}
				else if (split.OrderType != currentSOLine.OrderType || split.OrderNbr != currentSOLine.OrderNbr || split.LineNbr != currentSOLine.LineNbr)
				{
					foreignsplit = (SOLineSplit)this.Caches[typeof(SOLineSplit)].CreateCopy(split);
					split = new SOLineSplit();
				}

				POSupplyResult result = new POSupplyResult
				{
					POOrderType = supply.OrderType,
					POOrderNbr = supply.OrderNbr,
					POLineNbr = supply.LineNbr,
					POLine = supply,
					POOrder = poorder,
					CurrentSOLineSplits = split.SplitLineNbr != null ? new List <SOLineSplit> { split } : new List<SOLineSplit> { },
					ForeignSOLineSplits = foreignsplit.SplitLineNbr != null ? new List <SOLineSplit> { foreignsplit } : new List<SOLineSplit> { }
				};

				POSupplyResult existingResult = mergedResults.FirstOrDefault(x => x.POOrderType == result.POOrderType && x.POOrderNbr == result.POOrderNbr && x.POLineNbr == result.POLineNbr);
				if (existingResult.POOrderNbr != null)
				{
					if (!existingResult.CurrentSOLineSplits.Any(x => x.OrderType == split.OrderType && x.OrderNbr == split.OrderNbr && x.LineNbr == split.LineNbr && x.SplitLineNbr == split.SplitLineNbr))
					{
						existingResult.CurrentSOLineSplits.Add(split);
					}
					if (!existingResult.ForeignSOLineSplits.Any(x => x.OrderType == foreignsplit.OrderType && x.OrderNbr == foreignsplit.OrderNbr && x.LineNbr == foreignsplit.LineNbr && x.SplitLineNbr == foreignsplit.SplitLineNbr))
					{
						existingResult.ForeignSOLineSplits.Add(foreignsplit);
					}
				}
				else
				{
					mergedResults.Add(result);
				}
			}

			bool allSplitsCompleted = true;
			foreach(SOLineSplit split in splits.Select())
			{
				if (split.Completed != true)
				{
					allSplitsCompleted = false;
					break;
				}
			}
			//searching for other matching splits in cache and checking if all splits completed
			foreach (SOLineSplit splitFromCache in PXSelect<SOLineSplit,
														Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>,
															And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>>>>
														.Select(this, currentSOLine.OrderType, currentSOLine.OrderNbr)
														.Where(x => ((SOLineSplit)x).PONbr != null))
			{
				POSupplyResult existingResult = mergedResults.FirstOrDefault(
					x => x.POOrderType == splitFromCache.POType && x.POOrderNbr == splitFromCache.PONbr && x.POLineNbr == splitFromCache.POLineNbr);
				if (existingResult.POOrderNbr != null)
				{
					if (splitFromCache.LineNbr == currentSOLine.LineNbr)
					{
						//matching splits for current SOLine
						if (!existingResult.CurrentSOLineSplits.Any(x => x.SplitLineNbr == splitFromCache.SplitLineNbr))
					{
						existingResult.CurrentSOLineSplits.Add((SOLineSplit)this.Caches[typeof(SOLineSplit)].CreateCopy(splitFromCache));
					}
				}
					else
					{
						//matching splits for other SOLines
						if (!existingResult.ForeignSOLineSplits.Any(x => x.LineNbr == splitFromCache.LineNbr && x.SplitLineNbr == splitFromCache.SplitLineNbr))
						{
							existingResult.ForeignSOLineSplits.Add((SOLineSplit)this.Caches[typeof(SOLineSplit)].CreateCopy(splitFromCache));
						}
					}
				}
			}

			foreach (POSupplyResult res in mergedResults)
			{
				POLine3 supply = PXCache<POLine3>.CreateCopy(res.POLine);
				POOrder poorder = res.POOrder;

				decimal demandQty = 0m;
				SOLineSplit linkWithCurrentSOLine = null;
				foreach (SOLineSplit split in res.CurrentSOLineSplits)
				{
					if (split.PONbr != null)
					{
						if (split.PlanID != null && split.POCompleted != true)
							demandQty += (split.BaseQty ?? 0m) - (split.BaseReceivedQty ?? 0m);
						if (linkWithCurrentSOLine == null)
							linkWithCurrentSOLine = split;
					}
				}
				bool linkedWithCurrentSOLine = linkWithCurrentSOLine != null;
				bool linkedWithForeignSOLines = false;

				foreach (SOLineSplit foreignsplit in res.ForeignSOLineSplits)
				{
					if (foreignsplit.PONbr != null)
					{
						if (foreignsplit.PlanID != null && foreignsplit.POCompleted != true)
							demandQty += (foreignsplit.BaseQty ?? 0m) - (foreignsplit.BaseReceivedQty ?? 0m);
						linkedWithForeignSOLines = true;
					}
				}

				if (currentSOLine.POSource == INReplenishmentSource.DropShipToOrder &&
					  supply.OrderType == PO.POOrderType.RegularOrder) continue;

				if (!linkedWithCurrentSOLine 
					&& (poorder.Hold == true && !SOPOLinkShowDocumentsOnHold || (allSplitsCompleted || currentSOLine.Completed == true)))
						continue;

				if (linkedWithCurrentSOLine
					|| supply.OrderType != PO.POOrderType.DropShip && supply.Completed == false && supply.Cancelled == false && supply.BaseOpenQty - demandQty > 0m
					|| supply.OrderType == PO.POOrderType.DropShip && !linkedWithForeignSOLines && 
						(supply.Completed == false && supply.Cancelled == false && supply.BaseOrderQty >= 0m || supply.BaseReceivedQty > 0m))
				{
					// Records may be marked updated from UI by checking "selected" checkbox.
					POLine3 cachedSupply = posupply.Locate(supply);
					if (cachedSupply == null)
					{
						// New record should be stored to cache to preserve unbound fields.
						posupply.Cache.Hold(supply);
						cachedSupply = supply;
						}

					if (cachedSupply.SOOrderType != currentSOLine.OrderType
						|| cachedSupply.SOOrderNbr != currentSOLine.OrderNbr
						|| cachedSupply.SOOrderLineNbr != currentSOLine.LineNbr)
					{
						// Preserve user input on Grid refresh/filter update.
						cachedSupply.Selected = linkedWithCurrentSOLine;
					}

					cachedSupply.SOOrderType = currentSOLine.OrderType;
					cachedSupply.SOOrderNbr = currentSOLine.OrderNbr;
					cachedSupply.SOOrderLineNbr = currentSOLine.LineNbr;
					cachedSupply.SOOrderSplitLineNbr = linkedWithCurrentSOLine ? linkWithCurrentSOLine.SplitLineNbr : null;
					cachedSupply.LinkedToCurrentSOLine = linkedWithCurrentSOLine;
					cachedSupply.VendorRefNbr = poorder.VendorRefNbr;
					cachedSupply.DemandQty = demandQty;

					ret.Add(cachedSupply);
				}
			}
			return ret;
		}

		public virtual IEnumerable Invoicesplits()
		{
			List<InvoiceSplits> list = new List<InvoiceSplits>(invoicesplits.Cache.Inserted.Cast<InvoiceSplits>());

			if (list.Count > 0)
				return list; //return cached.

			int lineNbr = 0;

			if (addinvoicefilter.Current != null && addinvoicefilter.Current.RefNbr != null)
			{
				SOInvoicedRecords splits = lsselect.SelectInvoicedRecords(addinvoicefilter.Current.DocType, addinvoicefilter.Current.RefNbr);

				foreach (SOInvoicedRecords.Record record in splits.Records)
				{
					bool expand = record.Transactions.Count > 0;

					if (record.Item.KitItem == true && record.Item.StkItem == false)
					{
						expand = addinvoicefilter.Current.Expand == true && record.Transactions.Count > 0;
					}

					List<InvoiceSplits> expandedList = new List<InvoiceSplits>();
					foreach (SOInvoicedRecords.INTransaction tr in record.Transactions.Values)
					{
						foreach (Tuple<INTranSplit, bool> s in tr.Splits)
						{
							if (s.Item2)//IsAvailable
							{
								InvoiceSplits invSplit = CreateInvoiceSplits(record.ARTran, record.SOLine, record.SalesPerTran, tr.Transaction, s.Item1);
								expandedList.Add(invSplit);

								if (!string.IsNullOrEmpty(s.Item1.LotSerialNbr) && Document.Current.Behavior == SOBehavior.CM && record.Item.StkItem == false)
								{
									expand = true; //force expand.
								}
							}
						}
					}

					if (expand)
					{
						foreach (InvoiceSplits split in expandedList)
						{
							split.LineNbr = lineNbr++;
							InvoiceSplits invSplit = invoicesplits.Insert(split);
							list.Add(invSplit);
						}
					}
					else
					{
						InvoiceSplits invSplit = CreateInvoiceSplits(record.ARTran, record.SOLine, record.SalesPerTran, null, null);
						invSplit.LineNbr = lineNbr++;
						invSplit = invoicesplits.Insert(invSplit);
						list.Add(invSplit);
					}
				}

			}

			return list;
		}

		public virtual InvoiceSplits CreateInvoiceSplits(ARTran artran, SOLine line, SOSalesPerTran sptran, INTran tran, INTranSplit split)
		{
			InvoiceSplits invSplit = new InvoiceSplits();
			invSplit.TranTypeARTran = artran.TranType;
			invSplit.RefNbrARTran = artran.RefNbr;
			invSplit.LineNbrARTran = artran.LineNbr;

			invSplit.OrderTypeSOLine = line.OrderType;
			invSplit.OrderNbrSOLine = line.OrderNbr;
			invSplit.LineNbrSOLine = line.LineNbr;
			invSplit.LineTypeSOLine = line.LineType;
			invSplit.TranDesc = line.TranDesc;
			invSplit.InventoryID = line.InventoryID;
			invSplit.SiteID = line.SiteID;
			invSplit.LocationID = line.LocationID;
			invSplit.LotSerialNbr = line.LotSerialNbr;
			invSplit.UOM = artran.UOM;
			invSplit.Qty = artran.Qty;
			invSplit.BaseQty = artran.BaseQty;

			if (tran != null)
			{
				invSplit.DocTypeINTran = tran.DocType;
				invSplit.RefNbrINTran = tran.RefNbr;
				invSplit.LineNbrINTran = tran.LineNbr;
				invSplit.InventoryID = tran.InventoryID;
				invSplit.SubItemID = split.SubItemID ?? tran.SubItemID;
				invSplit.SiteID = tran.SiteID;
				invSplit.LocationID = split.LocationID ?? tran.LocationID;
				invSplit.LotSerialNbr = split.LotSerialNbr;
				invSplit.UOM = split.UOM ?? tran.UOM;
				invSplit.Qty = split.Qty ?? tran.Qty;
				invSplit.BaseQty = split.BaseQty ?? tran.BaseQty;

				invSplit.DocTypeINTranSplit = split.DocType ?? String.Empty;
				invSplit.RefNbrINTranSplit = split.RefNbr ?? String.Empty;
				invSplit.LineNbrINTranSplit = split.LineNbr ?? 0;
				invSplit.SplitLineNbrINTranSplit = split.SplitLineNbr ?? 0;
			}

			if (sptran != null)
			{
				invSplit.OrderTypeSOSalesPerTran = sptran.OrderType;
				invSplit.OrderNbrSOSalesPerTran = sptran.OrderNbr;
				invSplit.SalespersonIDSOSalesPerTran = sptran.SalespersonID;
			}

			return invSplit;
		}

		protected virtual void POLine3_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			//TODO: change to warnings and disable selection
			//if (supply.OrderType != PO.POOrderType.DropShip &&
			//        (supply.BaseOpenQty - demand.PlanQty > 0m && soline.ShipComplete != SOShipComplete.ShipComplete || supply.BaseOpenQty - demand.PlanQty >= soline.BaseOrderQty * soline.CompleteQtyMin / 100m) &&
			//        supply.Completed == false && supply.Cancelled == false ||
			//    supply.OrderType == PO.POOrderType.DropShip && IsLinkedToSO == false && (supply.Completed == false &&
			//        (supply.BaseOrderQty >= soline.BaseOrderQty * soline.CompleteQtyMin / 100m || soline.ShipComplete != SOShipComplete.ShipComplete) && supply.BaseOrderQty <= soline.BaseOrderQty * soline.CompleteQtyMax / 100m ||
			//        (supply.BaseReceivedQty >= soline.BaseOrderQty * soline.CompleteQtyMin / 100m || soline.ShipComplete == SOShipComplete.CancelRemainder) && supply.BaseReceivedQty <= soline.BaseOrderQty * soline.CompleteQtyMax / 100m) ||
			//    link != null)
			POLine3 supply = e.Row as POLine3;
			if (supply == null) return;
			SOLine soline = PXSelect<SOLine, Where<SOLine.orderType, Equal<Current<POLine3.sOOrderType>>, And<SOLine.orderNbr, Equal<Current<POLine3.sOOrderNbr>>, And<SOLine.lineNbr, Equal<Current<POLine3.sOOrderLineNbr>>>>>>.Select(this);

			bool RegularOrderMinMax = supply != null && soline != null && supply.OrderType != PO.POOrderType.DropShip &&
					(supply.BaseOpenQty - supply.DemandQty > 0m && soline.ShipComplete != SOShipComplete.ShipComplete || supply.BaseOpenQty - supply.DemandQty >= soline.BaseOrderQty * soline.CompleteQtyMin / 100m);

			bool DropShipOrderMinMax = supply != null && soline != null && supply.OrderType == PO.POOrderType.DropShip &&
					(supply.Completed == false && (supply.BaseOrderQty >= soline.BaseOrderQty * soline.CompleteQtyMin / 100m || soline.ShipComplete != SOShipComplete.ShipComplete) && supply.BaseOrderQty <= soline.BaseOrderQty * soline.CompleteQtyMax / 100m ||
					(supply.BaseReceivedQty >= soline.BaseOrderQty * soline.CompleteQtyMin / 100m || soline.ShipComplete == SOShipComplete.CancelRemainder) && supply.BaseReceivedQty <= soline.BaseOrderQty * soline.CompleteQtyMax / 100m);

			bool PartiallyReceipted = supply != null && soline != null && supply.Selected == true && supply.BaseOrderQty - supply.BaseOpenQty > 0 && supply.LinkedToCurrentSOLine == true;

			PXUIFieldAttribute.SetEnabled<POLine3.selected>(sender, e.Row, (RegularOrderMinMax || DropShipOrderMinMax || supply.LinkedToCurrentSOLine == true) && !PartiallyReceipted);
			if (PartiallyReceipted) PXUIFieldAttribute.SetWarning<POLine3.selected>(sender, e.Row, Messages.PurchaseOrderCannotBeDeselected);
		}

		public PXSelect<IN.InventoryItem> dummy_stockitem_for_redirect_newitem;

		[PXHidden]
		public PXSelect<CRRelation> RelationsLink;

		#endregion

		#region Buttons And Delegates
		public PXQuickProcess.Action<SOOrder>.ConfiguredBy<SOQuickProcessParameters> quickProcess;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Quick Process")]
		protected virtual IEnumerable QuickProcess(PXAdapter adapter) => SOQuickProcessExt.ButtonHandler(adapter);

		public PXAction<SOOrder> pOSupplyOK;
		[PXUIField(DisplayName = "PO Link", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable POSupplyOK(PXAdapter adapter)
		{

			if (Transactions.Current != null &&
				Transactions.Current.POCreate == true &&
				currentposupply.AskExt(POSupplyDialogInitializer) == WebDialogResult.OK)
			{
				LinkSupplyDemand();
			}

			return adapter.Get();
		}

		public virtual void POSupplyDialogInitializer(PXGraph graph, string viewName)
		{
			foreach (POLine3 supply in posupply.Cache.Updated)
			{
				// We should not preserve user input if dialog was closed without saving.
				supply.Selected = false;
				supply.SOOrderType = null;
				supply.SOOrderNbr = null;
				supply.SOOrderLineNbr = null;
			}
		}

		public virtual void LinkSupplyDemand()
		{
			//PXSelect<SOLineSplit, Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>, And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>, And<SOLineSplit.lineNbr, Equal<Required<SOLineSplit.lineNbr>>>>>>.Select(this, supply.SOOrderType, supply.SOOrderNbr, supply.SOOrderLineNbr)
			List<SOLineSplit> splits = this.splits.Select().RowCast<SOLineSplit>().ToList();

			//unlink first
			bool removedLink = false;
			foreach (POLine3 supply in posupply.Cache.Updated)
			{
				SOLine line = (SOLine)currentposupply.Select(supply.SOOrderType, supply.SOOrderNbr, supply.SOOrderLineNbr);

				line = PXCache<SOLine>.CreateCopy(line);

				if (supply.Selected == false && supply.LinkedToCurrentSOLine == true)
				{
					foreach (SOLineSplit split in splits)
					{
                        if (this.splits.Cache.GetStatus(split) == PXEntryStatus.Deleted || this.splits.Cache.GetStatus(split) == PXEntryStatus.InsertedDeleted)
                            continue;

						if (split.POType == supply.OrderType && split.PONbr == supply.OrderNbr && split.POLineNbr == supply.LineNbr &&
							split.POCompleted == false && split.Completed == false || supply.OrderType == POOrderType.DropShip)
						{
							if (split.POType != null && split.PONbr != null && split.POType == supply.OrderType && split.PONbr == supply.OrderNbr)
							{
								POOrder poorder = PXSelect<POOrder, Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
										And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(this, supply.OrderType, supply.OrderNbr);

								if (poorder != null)
								{
									if (split.RefNoteID == poorder.NoteID)
										split.RefNoteID = null;
									if (poorder.SOOrderType == split.OrderType && poorder.SOOrderNbr == split.OrderNbr)
									{
										poorder.SOOrderType = null;
										poorder.SOOrderNbr = null;
										poorderlink.Update(poorder);
									}
								}
							}
							if(split.PONbr != null)
							{
								split.ClearPOReferences();
								split.POCompleted = false;
								removedLink = true;
							}
							
							split.ReceivedQty = 0m;
							split.ShippedQty = 0m;
							split.Completed = false;

							this.splits.Update(split);

							if (supply.OrderType == PO.POOrderType.DropShip)
							{
								line.ShippedQty = 0m;
								line.UnbilledQty = line.OrderQty;
								line.OpenQty = line.OrderQty;
								line.ClosedQty = 0m;
								line.Completed = false;

								Transactions.Update(line);
							}
						}
					}
					supply.SOOrderSplitLineNbr = null;
					supply.LinkedToCurrentSOLine = false;
				}
			}

			//then link
			bool addedLink = false;
			bool poLineCompleted = false;
			foreach (POLine3 supply in posupply.Cache.Updated)
			{
				SOLine line = (SOLine)currentposupply.Select(supply.SOOrderType, supply.SOOrderNbr, supply.SOOrderLineNbr);

				line = PXCache<SOLine>.CreateCopy(line);

				if (supply.Selected == true && supply.LinkedToCurrentSOLine != true)
				{
					decimal? BaseOpenQty = supply.BaseOrderQty - supply.DemandQty;

					for (int i = 0; i < splits.Count; i++)
					{
						SOLineSplit split = PXCache<SOLineSplit>.CreateCopy(splits[i]);

						//TODO: it should not be possible to unallocate TR schedules
						if (string.IsNullOrEmpty(split.SOOrderNbr) && string.IsNullOrEmpty(split.PONbr) && split.IsAllocated == false && split.Completed == false && split.BaseQty > 0m)
						{
							if (supply.OrderType != POOrderType.Blanket)
							{
								supply.LineType =
									(line.POSource == INReplenishmentSource.DropShipToOrder) ?
									(line.LineType == SOLineType.Inventory ? POLineType.GoodsForDropShip : POLineType.NonStockForDropShip) :
									(line.LineType == SOLineType.Inventory ? POLineType.GoodsForSalesOrder : POLineType.NonStockForSalesOrder);
							}

							supply.SOOrderSplitLineNbr = split.SplitLineNbr;
							supply.LinkedToCurrentSOLine = true;

							INItemPlan plan;
							if (supply.Completed == false)
							{
								plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(this, supply.PlanID);
								if (plan == null) continue;
								if (supply.OrderType != PX.Objects.PO.POOrderType.Blanket)
								{
									plan = PXCache<INItemPlan>.CreateCopy(plan);
									plan.PlanType = (line.POSource == INReplenishmentSource.DropShipToOrder ? INPlanConstants.Plan74 : INPlanConstants.Plan76);
									this.Caches[typeof(INItemPlan)].Update(plan);
								}

								if (supply.OrderType == PO.POOrderType.DropShip)
								{
									POReceiptLine receipted = PXSelectJoinGroupBy<POReceiptLine,
										InnerJoin<POReceipt, On<POReceipt.receiptType, Equal<POReceiptLine.receiptType>, And<POReceipt.receiptNbr, Equal<POReceiptLine.receiptNbr>>>>,
										Where<POReceiptLine.pOType, Equal<Current<POLine3.orderType>>,
										And<POReceiptLine.pONbr, Equal<Current<POLine3.orderNbr>>,
										And<POReceiptLine.pOLineNbr, Equal<Current<POLine3.lineNbr>>,
										And<POReceipt.released, Equal<True>>>>>,
										Aggregate<Sum<POReceiptLine.baseReceiptQty>>>.SelectSingleBound(this, new object[] { supply });

									split.BaseShippedQty = receipted.BaseReceiptQty ?? 0m;
									PXDBQuantityAttribute.CalcTranQty<SOLineSplit.shippedQty>(this.splits.Cache, split);
									split.OpenQty = (split.Qty - split.ShippedQty);

									line.BaseShippedQty += split.BaseShippedQty;
									PXDBQuantityAttribute.CalcTranQty<SOLine.shippedQty>(Transactions.Cache, line);
									line.OpenQty = (line.OrderQty - line.ShippedQty);
									line.ClosedQty = line.ShippedQty;

									Transactions.Update(line);
								}
							}

							plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(this, split.PlanID);

							if (supply.OrderType == PO.POOrderType.DropShip)
							{
								foreach (PXResult<POReceiptLine, POReceipt> res in PXSelectJoin<POReceiptLine,
									InnerJoin<POReceipt,
										On<POReceipt.receiptType, Equal<POReceiptLine.receiptType>,
										And<POReceipt.receiptNbr, Equal<POReceiptLine.receiptNbr>,
										And<POReceipt.released, Equal<True>>>>>,
									Where<POReceiptLine.pOType, Equal<Required<POReceiptLine.pOType>>,
										And<POReceiptLine.pONbr, Equal<Required<POReceiptLine.pONbr>>,
										And<POReceiptLine.pOLineNbr, Equal<Required<POReceiptLine.pOLineNbr>>>>>>.Select(this, supply.OrderType, supply.OrderNbr, supply.LineNbr))
								{
									POReceiptLine porl = res;
									SOOrderShipment os = shipmentlist.Select().Where(s => ((SOOrderShipment)s).ShipmentNbr == porl.ReceiptNbr).FirstOrDefault();
									if (os == null)
									{
										os = new SOOrderShipment();
										os.OrderType = Document.Current.OrderType;
										os.OrderNbr = Document.Current.OrderNbr;
										os.ShipAddressID = Document.Current.ShipAddressID;
										os.ShipContactID = Document.Current.ShipContactID;
										os.ShipmentType = INDocType.DropShip;
										os.ShipmentNbr = porl.ReceiptNbr;
										os.ShippingRefNoteID = ((POReceipt)res).NoteID;
										os.Operation = SOOperation.Issue;
										os.ShipDate = porl.ReceiptDate;
										os.CustomerID = Document.Current.CustomerID;
										os.CustomerLocationID = Document.Current.CustomerLocationID;
										os.SiteID = null;
										os.ShipmentWeight = porl.ExtWeight;
										os.ShipmentVolume = porl.ExtVolume;
										os.ShipmentQty = porl.ReceiptQty;
										os.LineTotal = 0m;
										os.Confirmed = true;
										os.CreateINDoc = true;

										os.OrderType = Document.Current.OrderType;
										os.OrderNbr = Document.Current.OrderNbr;
										os.OrderNoteID = Document.Current.NoteID;
										shipmentlist.Insert(os);

										Document.Current.ShipmentCntr++;
									}
									else
									{
										os.ShipmentWeight += porl.ExtWeight;
										os.ShipmentVolume += porl.ExtVolume;
										os.ShipmentQty += porl.ReceiptQty;
										shipmentlist.Update(os);
									}
								}


								if (supply.Completed == true)
								{
									this.Caches[typeof(INItemPlan)].Delete(plan);
									plan = null;

									split.BaseShippedQty = supply.BaseReceivedQty;
									PXDBQuantityAttribute.CalcTranQty<SOLineSplit.shippedQty>(this.splits.Cache, split);
									split.Completed = true;
									split.PlanID = null;

									line.BaseShippedQty += split.BaseShippedQty;
									PXDBQuantityAttribute.CalcTranQty<SOLine.shippedQty>(Transactions.Cache, line);
									line.UnbilledQty -= (line.OrderQty - line.ShippedQty);
									line.OpenQty = 0m;
									line.ClosedQty = line.OrderQty;
									line.Completed = true;
									poLineCompleted = true;

									using (lsselect.SuppressedModeScope(true))
									Transactions.Update(line);
								}

								if (plan != null && plan.SupplyPlanID == null)
								{
									plan = PXCache<INItemPlan>.CreateCopy(plan);
									plan.SupplyPlanID = supply.PlanID;
									this.Caches[typeof(INItemPlan)].Update(plan);
								}
							}
							else
							{
								plan = PXCache<INItemPlan>.CreateCopy(plan);

								plan.PlanType = (supply.OrderType == PO.POOrderType.Blanket) ?
									(line.POSource == INReplenishmentSource.PurchaseToOrder ? INPlanConstants.Plan6B : INPlanConstants.Plan6E) :
									(line.POSource == INReplenishmentSource.DropShipToOrder ? INPlanConstants.Plan6D : INPlanConstants.Plan66);

								plan.FixedSource = INReplenishmentSource.Purchased;
								plan.SupplyPlanID = supply.PlanID;

								POOrder poorder = PXSelect<POOrder, Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
										And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>.Select(this, supply.OrderType, supply.OrderNbr);

								if (poorder != null)
								{
									plan.VendorID = poorder.VendorID;
									plan.VendorLocationID = poorder.VendorLocationID;
								}
								this.Caches[typeof(INItemPlan)].Update(plan);
							}

							split.POCreate = true;
							split.VendorID = supply.VendorID;
							split.POType = supply.OrderType;
							split.PONbr = supply.OrderNbr;
							split.POLineNbr = supply.LineNbr;
							addedLink = true;

							if (split.BaseQty <= BaseOpenQty)
							{
								BaseOpenQty -= split.BaseQty;

								split = this.splits.Update(split);
							}
							else
							{
								SOLineSplit copy = PXCache<SOLineSplit>.CreateCopy(split);

								copy.SplitLineNbr = null;
								copy.IsAllocated = false;
								
								copy.ClearPOFlags();
								copy.ClearPOReferences();
								copy.VendorID = null;
								copy.POCreate = true;

								copy.BaseQty = copy.BaseQty - BaseOpenQty;
								copy.Qty = INUnitAttribute.ConvertFromBase(this.splits.Cache, copy.InventoryID, copy.UOM, (decimal)copy.BaseQty, INPrecision.QUANTITY);
								copy.ShippedQty = 0m;
								copy.ReceivedQty = 0m;
								copy.UnreceivedQty = copy.BaseQty;
								copy.PlanID = null;
								copy.Completed = false;

								split.BaseQty = BaseOpenQty;
								split.Qty = INUnitAttribute.ConvertFromBase(this.splits.Cache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
								BaseOpenQty = 0m;
								split = this.splits.Update(split);

								if ((copy = this.splits.Insert(copy)) != null)
								{
									splits.Insert(i + 1, copy);
								}
							}
							splits[i] = split;
						}

						if (BaseOpenQty <= 0m) break;
					}
				}
			}

			var soLine = Transactions.Current;
			if (addedLink)
			{
				if (soLine.POCreated != true)
					Transactions.Cache.SetValue<SOLine.pOCreated>(soLine, true);

				if (poLineCompleted)
					lsselect.CompleteSchedules(lsselect.Cache, soLine);
			}
			else if(removedLink)
			{
				if (soLine.POCreated == true)
				{
					var linked = splits.Any(x => x.POCreate == true && x.PONbr != null);
					if (!linked)
						Transactions.Cache.SetValue<SOLine.pOCreated>(soLine, linked);
				}
			}
		}

		public PXAction<SOOrder> hold;
		[PXUIField(DisplayName = "Hold")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Hold(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<SOOrder> cancelled;
		[PXUIField(Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Cancelled(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<SOOrder> creditHold;
		[PXUIField(DisplayName = "Credit Hold")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable CreditHold(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<SOOrder> flow;
		[PXUIField(DisplayName = "Flow")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Flow(PXAdapter adapter,
			[PXInt]
			[PXIntList(new int[] { 5, 6 }, new string[] { "OnShipment", "OnDeleteInvoice" })]
			int? actionID,
			[PXString(1)]
			[SOOrderStatus.List()]
			string orderStatus1,
			[PXString(1)]
			[SOOrderStatus.List()]
			string orderStatus2,
			[PXString(1)]
			[SOShipmentStatus.List()]
			string shipmentStatus1,
			[PXString(1)]
			[SOShipmentStatus.List()]
			string shipmentStatus2)
		{
			switch (actionID)
			{
				case 5: //OnShipment //OBSOLETE - REMOVE
					{
						List<SOOrder> list = new List<SOOrder>();
						foreach (SOOrder order in adapter.Get<SOOrder>())
						{
							list.Add(order);
						}

						Save.Press();

						PXLongOperation.StartOperation(this, delegate ()
						{
							SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();

							foreach (SOOrder order in list)
							{
								SOOrder item = docgraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);

								if ((int)item.OpenShipmentCntr > 0)
								{
									item.Status = orderStatus1;
								}
								else
								{
									item.Status = orderStatus2;
								}

								docgraph.Document.Cache.SetStatus(item, PXEntryStatus.Updated);

								docgraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);

								docgraph.Save.Press();
							}
						});
					}
					break;
				case 6: //OnDeleteInvoice //OBSOLETE - REMOVE
					{
						List<SOOrder> list = new List<SOOrder>();
						foreach (SOOrder order in adapter.Get<SOOrder>())
						{
							list.Add(order);
						}

						Save.Press();

						PXLongOperation.StartOperation(this, delegate ()
						{
							SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();

							foreach (SOOrder order in list)
							{
								SOOrder item = docgraph.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);

								PXResultset<SOOrderShipment> shipments = PXSelect<SOOrderShipment, Where<SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>.SelectMultiBound(docgraph, new object[] { order });

								if (shipments.Count == 0)
								{
									item.Status = orderStatus1;

									docgraph.Document.Cache.SetStatus(item, PXEntryStatus.Updated);
								}
								else
								{
									foreach (SOOrderShipment ordershipment in shipments)
									{
										if (string.IsNullOrEmpty(ordershipment.InvoiceNbr))
										{
											SOShipment shipment = (SOShipment)PXSelect<SOShipment, Where<SOShipment.shipmentNbr, Equal<Current<SOOrderShipment.shipmentNbr>>>>.SelectSingleBound(docgraph, new object[] { ordershipment });

											shipment.Status = shipmentStatus1;

											docgraph.Caches[typeof(SOShipment)].SetStatus(shipment, PXEntryStatus.Updated);
										}
									}

									if (!docgraph.Views.Caches.Contains(typeof(SOShipment)))
									{
										docgraph.Views.Caches.Add(typeof(SOShipment));
									}
								}
								docgraph.Save.Press();
							}
						});
					}
					break;
				default:
					Save.Press();
					break;
			}
			return adapter.Get();
		}

		public PXAction<SOOrder> action;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter,
			[PXInt]
			[PXIntList(
				new int[] { 1, 2, 3, 4, 5, 6, 7 },
				new string[] {
					"Create Shipment",
					"Apply Assignment Rules",
					"Create Invoice",
					"Post Invoice to IN",
					"Create Purchase Order",
					"Create Transfer Order",
					"Re-open Order"
				})]
			int? actionID,
			[PXDate]
			DateTime? shipDate,
			[PXSelector(typeof(INSite.siteCD))]
			string siteCD,
			[SOOperation.List]
			string operation,
			[PXString()]
			string ActionName
			)
		{
			if (!string.IsNullOrEmpty(ActionName))
			{
				PXAction action = this.Actions[ActionName];

				if (action != null)
				{
					Save.Press();
					List<object> result = new List<object>();
					foreach (object data in action.Press(adapter))
					{
						result.Add(data);
					}
					return result;
				}
			}

			List<SOOrder> list = new List<SOOrder>();
			foreach (SOOrder order in adapter.Get<SOOrder>())
			{
				list.Add(order);
			}

			switch (actionID)
			{
				case 1:
					{
						if (shipDate != null)
						{
							soparamfilter.Current.ShipDate = shipDate;
						}

						if (soparamfilter.Current.ShipDate == null)
						{
							soparamfilter.Current.ShipDate = Accessinfo.BusinessDate;
						}

						if (siteCD != null)
						{
							soparamfilter.Cache.SetValueExt<SOParamFilter.siteID>(soparamfilter.Current, siteCD);
						}

						if (!adapter.MassProcess)
						{
							if (soparamfilter.Current.SiteID == null)
							{
								soparamfilter.Current.SiteID = GetPreferedSiteID();
							}
							if (adapter.ExternalCall)
								soparamfilter.AskExt(true);
						}
						if (soparamfilter.Current.SiteID != null || adapter.MassProcess)
						{
							try
							{
								RecalculateExternalTaxesSync = true;
							Save.Press();
							}
							finally
							{
								RecalculateExternalTaxesSync = false;
							}
							PXAutomation.RemovePersisted(this, typeof(SOOrder), new List<object>(list));

							SOParamFilter filter = soparamfilter.Current;
							PXLongOperation.StartOperation(this, delegate ()
							{
								bool anyfailed = false;
								SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
								DocumentList<SOShipment> created = new DocumentList<SOShipment>(docgraph);

								//address AC-92776
								for (int i = 0; i < list.Count; i++)
								{
									SOOrder order = list[i];
									if (adapter.MassProcess)
									{
										PXProcessing<SOOrder>.SetCurrentItem(order);
									}

									List<int?> sites = new List<int?>();

									if (filter.SiteID != null)
									{
										sites.Add(filter.SiteID);
									}
									else
									{
										foreach (SOShipmentPlan plan in PXSelectGroupBy<SOShipmentPlan,
											Where<SOShipmentPlan.orderType, Equal<Current<SOOrder.orderType>>,
												And<SOShipmentPlan.orderNbr, Equal<Current<SOOrder.orderNbr>>>>,
											Aggregate<GroupBy<SOShipmentPlan.siteID>>,
											OrderBy<Asc<SOShipmentPlan.siteID>>>.SelectMultiBound(docgraph, new object[] { order }))
										{
											INSite inSite = INSite.PK.Find(docgraph, plan.SiteID);
											
											// AC-144778. We can't use Match<> inside long run operation
											if (GroupHelper.IsAccessibleToUser(docgraph.Caches[typeof(INSite)], inSite, Accessinfo.UserName, forceUnattended: true))
												sites.Add(plan.SiteID);
										}
									}

									foreach (int? SiteID in sites)
									{
										SOOrder ordercopy = (SOOrder)this.Caches[typeof(SOOrder)].CreateCopy(order);
										try
										{
											using (var ts = new PXTransactionScope())
											{
												PXTimeStampScope.SetRecordComesFirst(typeof(SOOrder), true);
												docgraph.CreateShipment(order, SiteID, filter.ShipDate, adapter.MassProcess, operation, created, adapter.QuickProcessFlow);
												ts.Complete();
											}

											if (adapter.MassProcess)
											{
												PXProcessing<SOOrder>.SetProcessed();
											}
										}
										catch (SOShipmentException ex)
										{
											this.Caches[typeof(SOOrder)].RestoreCopy(order, ordercopy);
											if (!adapter.MassProcess)
											{
												throw;
											}
											order.LastSiteID = SiteID;
											order.LastShipDate = filter.ShipDate;
											order.Status = SOOrderStatus.Shipping;

											docgraph.Clear();

											var ordergraph = PXGraph.CreateInstance<SOOrderEntry>();
											ordergraph.Clear();

											ordergraph.Document.Cache.MarkUpdated(order);
											PXAutomation.CompleteSimple(ordergraph.Document.View);
											try
											{
											ordergraph.Save.Press();
											PXAutomation.RemovePersisted(ordergraph, order);

											PXTrace.WriteInformation(ex);
											PXProcessing<SOOrder>.SetWarning(ex);
										}
											catch(Exception inner)
											{
												this.Caches[typeof(SOOrder)].RestoreCopy(order, ordercopy);
												PXProcessing<SOOrder>.SetError(inner);
												anyfailed = true;
											}
										}
										catch (Exception ex)
										{
											this.Caches[typeof(SOOrder)].RestoreCopy(order, ordercopy);
											docgraph.Clear();

											if (!adapter.MassProcess)
											{
												throw;
											}
											PXProcessing<SOOrder>.SetError(ex);
											anyfailed = true;
										}


									}
								}
								if (adapter.AllowRedirect && !adapter.MassProcess && created.Count > 0)
								{
									using (new PXTimeStampScope(null))
									{
										docgraph.Clear();
										docgraph.Document.Current = docgraph.Document.Search<SOShipment.shipmentNbr>(created[0].ShipmentNbr);
										throw new PXRedirectRequiredException(docgraph, "Shipment");
									}
								}

								if (anyfailed)
								{
									throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
								}
							});
						}
					}
					break;
				case 2:
					{
						if (sosetup.Current.DefaultOrderAssignmentMapID == null)
						{
							throw new PXSetPropertyException(Messages.AssignNotSetup, Messages.SOSetup);
						}
						var processor = new EPAssignmentProcessor<SOOrder>();
						processor.Assign(Document.Current, sosetup.Current.DefaultOrderAssignmentMapID);
						Document.Update(Document.Current);
					}
					break;
				case 3:
					{
						Save.Press();

						PXLongOperation.StartOperation(this, delegate ()
						{
							SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
							var created = new InvoiceList(docgraph);

							this.InvoiceOrder(adapter.Arguments, (IEnumerable<SOOrder>)list, created, adapter.MassProcess, adapter.QuickProcessFlow);

							if (!adapter.MassProcess && created.Count > 0)
							{
								using (new PXTimeStampScope(null))
								{
									SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();
									ie.Document.Current = ie.Document.Search<ARInvoice.docType, ARInvoice.refNbr>(((ARInvoice)created[0]).DocType, ((ARInvoice)created[0]).RefNbr, ((ARInvoice)created[0]).DocType);
									throw new PXRedirectRequiredException(ie, "Invoice");
								}
							}
						});
					}
					break;
				case 4:
					{
						Save.Press();

						PXLongOperation.StartOperation(this, delegate ()
						{
							SOOrderEntry docgraph = PXGraph.CreateInstance<SOOrderEntry>();
							DocumentList<INRegister> created = new DocumentList<INRegister>(docgraph);
							INIssueEntry ie = PXGraph.CreateInstance<INIssueEntry>();
							ie.FieldVerifying.AddHandler<INTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
							ie.FieldVerifying.AddHandler<INTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
							foreach (SOOrder order in list)
							{
								try
								{
									if (adapter.MassProcess) PXProcessing<SOOrder>.SetCurrentItem(order);

									docgraph.PostOrder(ie, order, created);
								}
								catch (Exception ex)
								{
									if (!adapter.MassProcess)
									{
										throw;
									}
									PXProcessing<SOOrder>.SetError(ex);
								}
							}

							if (docgraph.sosetup.Current.AutoReleaseIN == true && created.Count > 0 && created[0].Hold == false)
							{
								INDocumentRelease.ReleaseDoc(created, false);
							}
						});
					}
					break;
				case 5:
					if (list.Count > 0)
					{
						Save.Press();
						POCreate graph = PXGraph.CreateInstance<POCreate>();
						graph.Filter.Current.OrderType = list[0].OrderType;
						graph.Filter.Current.OrderNbr = list[0].OrderNbr;
						throw new PXRedirectRequiredException(graph, PO.Messages.PurchaseOrderCreated);
					}
					break;
				case 6:
					if (list.Count > 0)
					{
						Save.Press();
						SOCreate graph = PXGraph.CreateInstance<SOCreate>();
						graph.Filter.Current.OrderType = list[0].OrderType;
						graph.Filter.Current.OrderNbr = list[0].OrderNbr;
						throw new PXRedirectRequiredException(graph, Messages.TransferOrderCreated);
					}
					break;
				case 7: /* Re-open order */
					break;
				default:
					Save.Press();
					break;
			}
			return list;
		}

		private Int32? GetPreferedSiteID()
		{
			int? siteID = null;
			PXResultset<SOOrderSite> osites = PXSelectJoin<SOOrderSite,
				InnerJoin<INSite, On<SOOrderSite.FK.Site>>,
				Where<SOOrderSite.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOOrderSite.orderNbr, Equal<Current<SOOrder.orderNbr>>,
						And<Match<INSite, Current<AccessInfo.userName>>>>>>.Select(this);
			SOOrderSite preferred;
			if (osites.Count == 1)
			{
				siteID = ((SOOrderSite)osites).SiteID;
			}
			else if ((preferred = PXSelectJoin<SOOrderSite,
						InnerJoin<INSite, 
							On<SOOrderSite.FK.Site>>,
						Where<SOOrderSite.orderType, Equal<Current<SOOrder.orderType>>,
							And<SOOrderSite.orderNbr, Equal<Current<SOOrder.orderNbr>>,
								And<SOOrderSite.siteID, Equal<Current<SOOrder.defaultSiteID>>,
									And<Match<INSite, Current<AccessInfo.userName>>>>>>>.Select(this)) != null)
			{
				siteID = preferred.SiteID;
			}
			return siteID;
		}

		public PXAction<SOOrder> inquiry;
		[PXUIField(DisplayName = "Inquiries", MapEnableRights = PXCacheRights.Select)]
		[PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.InquiriesFolder)]
		protected virtual IEnumerable Inquiry(PXAdapter adapter,
			[PXInt]
			[PXIntList(new int[] { }, new string[] { })]
			int? inquiryID,
			[PXString()]
			string ActionName
			)
		{
			if (!string.IsNullOrEmpty(ActionName))
			{
				PXAction action = this.Actions[ActionName];

				if (action != null)
				{
					Save.Press();
					foreach (object data in action.Press(adapter)) ;
				}
			}
			return adapter.Get();
		}

		public PXAction<SOOrder> report;
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Report(PXAdapter adapter,
			[PXString(8, InputMask = "CC.CC.CC.CC")]
			string reportID
			)
		{
			List<SOOrder> list = adapter.Get<SOOrder>().ToList();
			if (!String.IsNullOrEmpty(reportID))
			{
				Save.Press();
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				string actualReportID = null;

				PXReportRequiredException ex = null;
				Dictionary<PX.SM.PrintSettings, PXReportRequiredException> reportsToPrint = new Dictionary<PX.SM.PrintSettings, PXReportRequiredException>();

				foreach (SOOrder order in list)
				{
					parameters = new Dictionary<string, string>();
					parameters["SOOrder.OrderType"] = order.OrderType;
					parameters["SOOrder.OrderNbr"] = order.OrderNbr;

					object cstmr = PXSelectorAttribute.Select<SOOrder.customerID>(Document.Cache, order);
					actualReportID = new NotificationUtility(this).SearchReport(SONotificationSource.Customer, cstmr, reportID, order.BranchID);
					ex = PXReportRequiredException.CombineReport(ex, actualReportID, parameters);

					reportsToPrint = PX.SM.SMPrintJobMaint.AssignPrintJobToPrinter(reportsToPrint, parameters, adapter, new NotificationUtility(this).SearchPrinter, SONotificationSource.Customer, reportID, actualReportID, order.BranchID);
				}

				if (ex != null)
				{
					PX.SM.SMPrintJobMaint.CreatePrintJobGroups(reportsToPrint);

					throw ex;
				}
			}
			return list;
		}
		public PXAction<SOOrder> notification;
		[PXUIField(DisplayName = "Notifications", Visible = false)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntryF)]
		protected virtual IEnumerable Notification(PXAdapter adapter,
		[PXString]
		string notificationCD
		)
		{
			bool massProcess = adapter.MassProcess;

			PXLongOperation.StartOperation(this, () =>
			{
				bool anyfailed = false;

			foreach (SOOrder order in adapter.Get<SOOrder>())
			{
					if (massProcess) PXProcessing<SOOrder>.SetCurrentItem(order);

					try
					{
				Document.Current = order;
				var parameters = new Dictionary<string, string>();
				parameters["SOOrder.OrderType"] = order.OrderType;
				parameters["SOOrder.OrderNbr"] = order.OrderNbr;
				Caches["SOOrder"].Current = order;
				Activity.SendNotification(ARNotificationSource.Customer, notificationCD, order.BranchID, parameters);

						if (massProcess) PXProcessing<SOOrder>.SetProcessed();
					}
					catch (Exception exception)
					{
						if (!massProcess) throw;

						//This code is needed to address the situation with incorrect update of SOOrder in case one or more attempts to email SO fail during mass processing. 
						Document.Cache.SetStatus(order, PXEntryStatus.Notchanged);
						Document.Cache.Remove(order);

						PXProcessing<SOOrder>.SetError(exception);
						anyfailed = true;
					}
			}

				if (anyfailed)
				{
					throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
				}
			});

			return adapter.Get();
		}
		public PXAction<SOOrder> prepareInvoice;
		[PXUIField(DisplayName = "Prepare Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable PrepareInvoice(PXAdapter adapter)
		{
			List<SOOrder> list = adapter.Get<SOOrder>().ToList();

			foreach (SOOrder order in list)
			{
				if (this.Document.Cache.GetStatus(order) != PXEntryStatus.Inserted)
					this.Document.Cache.SetStatus(order, PXEntryStatus.Updated);
			}

			if (!adapter.MassProcess)
			{
				try
				{
					RecalculateExternalTaxesSync = true;
					Save.Press();
				}
				finally
				{
					RecalculateExternalTaxesSync = false;
				}
			}

			PXLongOperation.StartOperation(this, delegate ()
			{
				SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
				var created = new InvoiceList(docgraph);

				this.InvoiceOrder(adapter.Arguments, (IEnumerable<SOOrder>)list, created, adapter.MassProcess, adapter.QuickProcessFlow);

				if (!adapter.MassProcess && created.Count > 0)
				{
					using (new PXTimeStampScope(null))
					{
						SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();
						ie.Document.Current = ie.Document.Search<ARInvoice.docType, ARInvoice.refNbr>(((ARInvoice)created[0]).DocType, ((ARInvoice)created[0]).RefNbr, ((ARInvoice)created[0]).DocType);
						throw new PXRedirectRequiredException(ie, "Invoice");
					}
				}
			});
			return list;
		}

		public PXAction<SOOrder> addInvoice;
		[PXUIField(DisplayName = Messages.AddInvoice, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable AddInvoice(PXAdapter adapter)
		{
			try
			{
				if ((IsCreditMemoOrder || IsRMAOrder) && Transactions.Cache.AllowInsert && invoicesplits.AskExt() == WebDialogResult.OK)
				{
					foreach (InvoiceSplits res in invoicesplits.Cache.Cached.RowCast<InvoiceSplits>().Where(res => res.Selected == true))
					{
						INTran tran = PXSelectReadonly<INTran,
							Where<INTran.docType, Equal<Required<INTran.docType>>,
								And<INTran.lineNbr, Equal<Required<INTran.lineNbr>>,
								And<INTran.refNbr, Equal<Required<INTran.refNbr>>>>>>
							.Select(this, res.DocTypeINTran, res.LineNbrINTran, res.RefNbrINTran);
						SOLine origLine = SOLine.PK.Find(this, res.OrderTypeSOLine, res.OrderNbrSOLine, res.LineNbrSOLine);
						ARTran artran = PXSelectReadonly<ARTran,
							Where<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>,
								And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
								And<ARTran.tranType, Equal<Required<ARTran.tranType>>>>>>
							.Select(this, res.LineNbrARTran, res.RefNbrARTran, res.TranTypeARTran);
						ARRegister invoice = PXSelectReadonly<ARRegister,
							Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
								And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>
							.Select(this, res.TranTypeARTran, res.RefNbrARTran);

						SOLine existing = PXSelect<SOLine, Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
									And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>,
									And<SOLine.origOrderType, Equal<Required<SOLine.origOrderType>>,
									And<SOLine.origOrderNbr, Equal<Required<SOLine.origOrderNbr>>,
									And<SOLine.origLineNbr, Equal<Required<SOLine.origLineNbr>>,
									And<SOLine.inventoryID, Equal<Required<SOLine.inventoryID>>,
									And<SOLine.invoiceType, Equal<Required<SOLine.invoiceType>>,
							And<SOLine.invoiceNbr, Equal<Required<SOLine.invoiceNbr>>,
							And<SOLine.invoiceLineNbr, Equal<Required<SOLine.invoiceLineNbr>>>>>>>>>>>>
							.Select(this, origLine.OrderType, origLine.OrderNbr, origLine.LineNbr, res.InventoryID,
										tran != null ? tran.ARDocType : (artran != null ? artran.TranType : null),
								tran != null ? tran.ARRefNbr : (artran != null ? artran.RefNbr : null),
								tran != null ? tran.ARLineNbr : (artran != null ? artran.LineNbr : null));
						if (existing != null)
						{
							Transactions.Current = existing;
						}
						else
						{
							SOLine newline = new SOLine();
							newline.BranchID = origLine.BranchID;
							newline.Operation = SOOperation.Receipt;

							if (tran != null)
							{
								newline.InvoiceType = tran.ARDocType;
								newline.InvoiceNbr = tran.ARRefNbr;
								newline.InvoiceLineNbr = tran.ARLineNbr;
							}
							else if (artran != null)
							{
								newline.InvoiceType = artran.TranType;
								newline.InvoiceNbr = artran.RefNbr;
								newline.InvoiceLineNbr = artran.LineNbr;
							}

							newline.InvoiceDate = artran.TranDate;
							newline.OrigOrderType = res.OrderTypeSOLine;
							newline.OrigOrderNbr = res.OrderNbrSOLine;
							newline.OrigLineNbr = res.LineNbrSOLine;
							newline.SalesAcctID = null;
							newline.SalesSubID = null;
							newline.TaxCategoryID = origLine.TaxCategoryID;
							newline.SalesPersonID = res.SalespersonIDSOSalesPerTran;
							newline.Commissionable = artran.Commissionable;

							newline.ManualPrice = true;
							newline.ManualDisc = true;

							newline.InventoryID = res.InventoryID;
							newline.SubItemID = res.SubItemID;
							newline.SiteID = res.SiteID;
							//newline.LocationID = res.LocationID;
							newline.LotSerialNbr = res.LotSerialNbr;
							newline.UOM = (newline.InventoryID == artran.InventoryID) ? artran.UOM : res.UOM;
							newline.CuryInfoID = Document.Current.CuryInfoID;
							
							if (artran?.AvalaraCustomerUsageType != null)
							{
								newline.AvalaraCustomerUsageType = artran.AvalaraCustomerUsageType;
							}

							if (origLine.LineType == SOLineType.MiscCharge || origLine.LineType == SOLineType.NonInventory || tran == null)
							{
								if (newline.InventoryID == artran.InventoryID)
									newline.UnitCost = artran.BaseQty > 0m ? (artran.TranCost / artran.BaseQty) : artran.TranCost;
							}
							else
							{
								if (newline.InventoryID == tran.InventoryID)
									newline.UnitCost = tran.Qty > 0m ? (tran.TranCost / tran.Qty) : tran.TranCost;
							}

							newline.DiscPctDR = artran.DiscPctDR;
							if (artran.CuryUnitPriceDR != null && invoice != null)
							{
								if (Document.Current.CuryID == invoice.CuryID)
								{
									newline.CuryUnitPriceDR = artran.CuryUnitPriceDR;
								}
								else
								{
									decimal unitPriceDR = 0m;
											PXDBCurrencyAttribute.CuryConvBase(this.Caches[typeof(ARTran)], artran, artran.CuryUnitPriceDR ?? 0m, out unitPriceDR, true);

									decimal orderCuryUnitPriceDR = 0m;
											PXDBCurrencyAttribute.CuryConvCury(Transactions.Cache, newline, unitPriceDR, out orderCuryUnitPriceDR, CommonSetupDecPl.PrcCst);
									newline.CuryUnitPriceDR = orderCuryUnitPriceDR;
								}
							}

							newline.DRTermStartDate = artran.DRTermStartDate;
							newline.DRTermEndDate = artran.DRTermEndDate;

							newline.ReasonCode = origLine.ReasonCode;
							newline.TaskID = origLine.TaskID;
							newline.CostCodeID = origLine.CostCodeID;

							if (!string.IsNullOrEmpty(artran.DeferredCode))
							{
								DRSchedule drSchedule = PXSelectReadonly<DRSchedule,
									Where<DRSchedule.module, Equal<BatchModule.moduleAR>,
										And<DRSchedule.docType, Equal<Required<ARTran.tranType>>,
										And<DRSchedule.refNbr, Equal<Required<ARTran.refNbr>>,
										And<DRSchedule.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>>
									.Select(this, artran.TranType, artran.RefNbr, artran.LineNbr);
								if (drSchedule != null)
								{
									newline.DefScheduleID = drSchedule.ScheduleID;
								}
							}

							decimal CuryUnitCost;
									PXDBCurrencyAttribute.CuryConvCury(Transactions.Cache, newline, (decimal)newline.UnitCost, out CuryUnitCost, CommonSetupDecPl.PrcCst);
							newline.CuryUnitCost = CuryUnitCost;

							if (invoice != null && newline.InventoryID == artran.InventoryID)
							{
								if (Document.Current.CuryID == invoice.CuryID)
								{
									decimal UnitPrice;
											PXDBCurrencyAttribute.CuryConvBase(Transactions.Cache, newline, (decimal)artran.CuryUnitPrice, out UnitPrice, CommonSetupDecPl.PrcCst);
									newline.CuryUnitPrice = artran.CuryUnitPrice;
									newline.UnitPrice = UnitPrice;
								}
								else
								{
									decimal CuryUnitPrice;
											PXDBCurrencyAttribute.CuryConvCury(Transactions.Cache, newline, (decimal)artran.UnitPrice, out CuryUnitPrice, CommonSetupDecPl.PrcCst);
									newline.CuryUnitPrice = CuryUnitPrice;
									newline.UnitPrice = artran.UnitPrice;
								}
							}

							newline = Transactions.Insert(newline);

							SOSalesPerTran pertran = PXSelectReadonly<SOSalesPerTran,
								Where<SOSalesPerTran.orderNbr, Equal<Required<SOSalesPerTran.orderNbr>>,
									And<SOSalesPerTran.orderType, Equal<Required<SOSalesPerTran.orderType>>,
									And<SOSalesPerTran.salespersonID, Equal<Required<SOSalesPerTran.salespersonID>>>>>>
								.Select(this, res.OrderNbrSOSalesPerTran, res.OrderTypeSOSalesPerTran, res.SalespersonIDSOSalesPerTran);
							if (SalesPerTran.Current != null && SalesPerTran.Cache.ObjectsEqual<SOSalesPerTran.salespersonID>(pertran, SalesPerTran.Current))
							{
								SOSalesPerTran salespertran_copy = PXCache<SOSalesPerTran>.CreateCopy(SalesPerTran.Current);
								SalesPerTran.Cache.SetValueExt<SOSalesPerTran.commnPct>(SalesPerTran.Current, pertran.CommnPct);
								SalesPerTran.Cache.RaiseRowUpdated(SalesPerTran.Current, salespertran_copy);
							}

							//clear splits
							lsselect.RaiseRowDeleted(Transactions.Cache, newline);

							existing = newline;
						}
						SOLine copy = PXCache<SOLine>.CreateCopy(existing);

						INTranSplit split = PXSelectReadonly<INTranSplit,
							Where<INTranSplit.docType, Equal<Required<INTranSplit.docType>>,
								And<INTranSplit.lineNbr, Equal<Required<INTranSplit.lineNbr>>,
								And<INTranSplit.refNbr, Equal<Required<INTranSplit.refNbr>>,
								And<INTranSplit.splitLineNbr, Equal<Required<INTranSplit.splitLineNbr>>>>>>>
							.Select(this, res.DocTypeINTranSplit, res.LineNbrINTranSplit, res.RefNbrINTranSplit, res.SplitLineNbrINTranSplit);
							bool processSplits = split != null && (lsselect.IsLSEntryEnabled || lsselect.IsAllocationEntryEnabled) && (!string.IsNullOrEmpty(split.LotSerialNbr) || lsselect.IsLocationEnabled);
							if (!processSplits)
							{
								copy.BaseQty += res.BaseQty;
							}

							if (copy.BaseQty == 0m)
							{
								if (Document.Current.CuryID == invoice.CuryID)
								{
									decimal LineAmt;
									PXDBCurrencyAttribute.CuryConvBase<SOLine.curyInfoID>(Transactions.Cache, copy, (decimal)artran.CuryTranAmt, out LineAmt);
									copy.CuryLineAmt = artran.CuryTranAmt;
									copy.LineAmt = LineAmt;
								}
								else
								{
									decimal CuryLineAmt;
									PXDBCurrencyAttribute.CuryConvCury<SOLine.curyInfoID>(Transactions.Cache, copy, (decimal)artran.TranAmt, out CuryLineAmt);
									copy.CuryLineAmt = CuryLineAmt;
									copy.LineAmt = artran.TranAmt;
								}
							}

							PXDBQuantityAttribute.CalcTranQty<SOLine.orderQty>(Transactions.Cache, copy);

							try
							{
								copy = Transactions.Update(copy);
							}
							catch (PXSetPropertyException) {; }

							if (processSplits)
							{
								SOLineSplit newsplit = new SOLineSplit();
								newsplit.SubItemID = split.SubItemID;
								if (lsselect.IsLocationEnabled)
									newsplit.LocationID = split.LocationID;
								newsplit.LotSerialNbr = split.LotSerialNbr;
								newsplit.ExpireDate = split.ExpireDate;
								newsplit.UOM = split.UOM;

								newsplit = splits.Insert(newsplit);
								newsplit.Qty = split.Qty;
								newsplit = splits.Update(newsplit);
								string error = PXUIFieldAttribute.GetError<SOLineSplit.qty>(splits.Cache, newsplit);

								if (!string.IsNullOrEmpty(error))
								{
									newsplit.Qty = 0;
									newsplit = splits.Update(newsplit);
								}
							}

								if (Document.Current.CuryID == invoice.CuryID)
								{
									decimal DiscAmt;
								PXDBCurrencyAttribute.CuryConvBase<SOLine.curyInfoID>(Transactions.Cache, copy, (decimal) artran.CuryDiscAmt, out DiscAmt);
							copy.CuryDiscAmt = artran.CuryDiscAmt;
									copy.DiscAmt = DiscAmt;
								}
								else
								{
									decimal CuryDiscAmt;
							PXDBCurrencyAttribute.CuryConvCury<SOLine.curyInfoID>(Transactions.Cache, copy, (decimal)artran.DiscAmt, out CuryDiscAmt);
									copy.CuryDiscAmt = CuryDiscAmt;
							copy.DiscAmt = artran.DiscAmt;
							}

						copy.DiscPct = artran.DiscPct;
						copy.FreezeManualDisc = true;

							try
							{
								copy = Transactions.Update(copy);
							}
							catch (PXSetPropertyException) {; }
					}
				}

				if (addinvoicefilter.Current != null)
				{
					if (!IsImport)
						addinvoicefilter.Current.RefNbr = null;
					else
						addinvoicefilter.Current = null;
				}
			}
			finally
			{
				this.invoicesplits.Cache.Clear();
				this.invoicesplits.View.Clear();
			}

			return adapter.Get();
		}

		public PXAction<SOOrder> addInvoiceOK;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddInvoiceOK(PXAdapter adapter)
		{
			invoicesplits.View.Answer = WebDialogResult.OK;

			return AddInvoice(adapter);
		}

		public PXAction<SOOrder> checkCopyParams;
		[PXUIField(DisplayName = "OK", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable CheckCopyParams(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<SOOrder> reopenOrder;
		[PXUIField(DisplayName = "Re-open Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ReopenOrder(PXAdapter adapter)
		{
			Document.Current.Completed = false;
			Document.Current.Cancelled = false;
			if (Document.Current.Behavior == "SO")
			{
				Document.Current.Status = SOOrderStatus.BackOrder;
			}
			else
			{
				Document.Current.Status = SOOrderStatus.Open;
			}
			Document.Cache.Update(Document.Current);
			return adapter.Get();
		}

		public PXAction<SOOrder> copyOrder;
		[PXUIField(DisplayName = "Copy Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable CopyOrder(PXAdapter adapter)
		{
			List<SOOrder> list = adapter.Get<SOOrder>().ToList();
			WebDialogResult dialogResult = copyparamfilter.AskExt(setStateFilter, true);
			if ((dialogResult == WebDialogResult.OK || (this.IsContractBasedAPI && dialogResult == WebDialogResult.Yes)) && string.IsNullOrEmpty(copyparamfilter.Current.OrderType) == false)
			{
				this.Save.Press();
				SOOrder order = PXCache<SOOrder>.CreateCopy(Document.Current);

				IsCopyOrder = true;
				try
				{
					this.CopyOrderProc(order, copyparamfilter.Current);
				}
				finally
				{
					IsCopyOrder = false;
				}

				List<SOOrder> rs = new List<SOOrder> { Document.Current };
				return rs;
			}
			return list;
		}

		private void setStateFilter(PXGraph aGraph, string ViewName)
		{
			checkCopyParams.SetEnabled(!string.IsNullOrEmpty(copyparamfilter.Current.OrderType) && !string.IsNullOrEmpty(copyparamfilter.Current.OrderNbr));
		}


		public virtual void CopyOrderProc(SOOrder sourceOrder, CopyParamFilter copyFilter)
		{
			string newOrderType = copyFilter.OrderType;
			string newOrderNbr = copyFilter.OrderNbr;
			bool recalcUnitPrices = (bool)copyFilter.RecalcUnitPrices;
			bool overrideManualPrices = (bool)copyFilter.OverrideManualPrices;
			bool recalcDiscounts = (bool)copyFilter.RecalcDiscounts;
			bool overrideManualDiscounts = (bool)copyFilter.OverrideManualDiscounts;

			var userDefinedFieldValues = Document.Cache.Fields
				.Where(Document.Cache.IsKvExtAttribute)
				.ToDictionary(
					udField => udField,
					udField => ((PXFieldState)Document.Cache.GetValueExt(sourceOrder, udField))?.Value);

			SOOrderType ordertype = soordertype.SelectWindowed(0, 1, sourceOrder.OrderType);

			this.Clear(PXClearOption.PreserveTimeStamp);

			PXResultset<SOOrder> orderWithCurrency =
				PXSelectJoin<SOOrder,
				InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<SOOrder.curyInfoID>>>,
				Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
					And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
				.Select(this, sourceOrder.OrderType, sourceOrder.OrderNbr);
			foreach (PXResult<SOOrder, CurrencyInfo> res in orderWithCurrency)
			{
				SOOrder orderBeingCopied = (SOOrder)res;
				CurrencyInfo currencyInfo = (CurrencyInfo)res;

				if (orderBeingCopied.Behavior == SOOrderTypeConstants.QuoteOrder)
				{
					orderBeingCopied.Completed = true;
					orderBeingCopied.Status = SOOrderStatus.Completed;
					Document.Cache.SetStatus(orderBeingCopied, PXEntryStatus.Updated);
				}

				CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy(currencyInfo);
				info.CuryInfoID = null;
				info.IsReadOnly = false;
				info = this.currencyinfo.Insert(info);
				CurrencyInfo copyinfo = PXCache<CurrencyInfo>.CreateCopy(info);

				var newOrder = new SOOrder
				{
					CuryInfoID = info.CuryInfoID,
					OrderType = newOrderType,
					OrderNbr = newOrderNbr,
					GroundCollect = orderBeingCopied.GroundCollect
				};
				newOrder = Document.Insert(newOrder);

				//Automation
				newOrder = Document.Search<SOOrder.orderNbr>(newOrder.OrderNbr, newOrder.OrderType);

				//Disable tax calculation for freight as well
				TaxBaseAttribute.SetTaxCalc<SOOrder.freightTaxCategoryID>(this.Document.Cache, null, TaxCalc.ManualCalc);

				SOOrder targetOrder = PXCache<SOOrder>.CreateCopy(orderBeingCopied);

				targetOrder.OwnerID = newOrder.OwnerID;
				targetOrder.WorkgroupID = null;
				targetOrder.OrderType = newOrder.OrderType;
				targetOrder.OrderNbr = newOrder.OrderNbr;
				targetOrder.Behavior = newOrder.Behavior;
				targetOrder.ARDocType = newOrder.ARDocType;
				targetOrder.DefaultOperation = newOrder.DefaultOperation;
				targetOrder.ShipAddressID = newOrder.ShipAddressID;
				targetOrder.ShipContactID = newOrder.ShipContactID;
				targetOrder.BillAddressID = newOrder.BillAddressID;
				targetOrder.BillContactID = newOrder.BillContactID;
				targetOrder.OrigOrderType = orderBeingCopied.OrderType;
				targetOrder.OrigOrderNbr = orderBeingCopied.OrderNbr;
				targetOrder.ShipmentCntr = 0;
				targetOrder.OpenShipmentCntr = 0;
				targetOrder.OpenLineCntr = 0;
				targetOrder.ReleasedCntr = 0;
				targetOrder.BilledCntr = 0;
				targetOrder.OrderQty = 0m;
				targetOrder.OrderWeight = 0m;
				targetOrder.OrderVolume = 0m;
				targetOrder.OpenOrderQty = 0m;
				targetOrder.UnbilledOrderQty = 0m;
				targetOrder.CuryInfoID = newOrder.CuryInfoID;
				targetOrder.Status = newOrder.Status;
				targetOrder.Hold = newOrder.Hold;
				targetOrder.Completed = newOrder.Completed;
				targetOrder.Cancelled = newOrder.Cancelled;
				targetOrder.InclCustOpenOrders = newOrder.InclCustOpenOrders;
				targetOrder.OrderDate = newOrder.OrderDate;
				targetOrder.CuryMiscTot = 0m;
				targetOrder.CuryUnbilledMiscTot = 0m;
				targetOrder.CuryLineTotal = 0m;
				targetOrder.CuryOpenLineTotal = 0m;
				targetOrder.CuryUnbilledLineTotal = 0m;
				targetOrder.CuryVatExemptTotal = 0m;
				targetOrder.CuryVatTaxableTotal = 0m;
				targetOrder.CuryTaxTotal = 0m;
				targetOrder.CuryOrderTotal = 0m;
				targetOrder.CuryOpenOrderTotal = 0m;
				targetOrder.CuryOpenTaxTotal = 0m;
				targetOrder.CuryUnbilledOrderTotal = 0m;
				targetOrder.CuryUnbilledTaxTotal = 0m;
				targetOrder.CuryUnbilledDiscTotal = 0m;
				targetOrder.CuryOpenDiscTotal = 0m;
				targetOrder.FreightTaxCategoryID = null;
				targetOrder.CreatedByID = newOrder.CreatedByID;
				targetOrder.CreatedByScreenID = newOrder.CreatedByScreenID;
				targetOrder.CreatedDateTime = newOrder.CreatedDateTime;
				targetOrder.DisableAutomaticDiscountCalculation = orderBeingCopied.DisableAutomaticDiscountCalculation;
				targetOrder.ApprovedCredit = false;
				targetOrder.ApprovedCreditAmt = 0m;
				targetOrder.PackageWeight = 0m;

				if (targetOrder.RequestDate < newOrder.OrderDate)
				{
					targetOrder.RequestDate = newOrder.RequestDate;
				}

				if (targetOrder.ShipDate < newOrder.OrderDate)
				{
					targetOrder.ShipDate = newOrder.ShipDate;
				}

				if (orderBeingCopied.Behavior == SOOrderTypeConstants.QuoteOrder)
				{
					targetOrder.BillSeparately = newOrder.BillSeparately;
					targetOrder.ShipSeparately = newOrder.ShipSeparately;
				}
				Document.Cache.SetDefaultExt<SOOrder.invoiceDate>(targetOrder);
				Document.Cache.SetDefaultExt<SOOrder.finPeriodID>(targetOrder);
				targetOrder.ExtRefNbr = null;
				targetOrder.NoteID = null;
				Document.Cache.ForceExceptionHandling = true;
				targetOrder = Document.Update(targetOrder);

				PXNoteAttribute.CopyNoteAndFiles(Document.Cache, orderBeingCopied, Document.Cache, targetOrder, ordertype);

				if (orderBeingCopied.Behavior.IsIn(SOOrderTypeConstants.QuoteOrder, targetOrder.Behavior))
					foreach ((string fieldName, object value) in userDefinedFieldValues)
						Document.Cache.SetValueExt(targetOrder, fieldName, value);

				if (info != null)
				{
					info.CuryID = copyinfo.CuryID;
					info.CuryEffDate = copyinfo.CuryEffDate;
					info.CuryRateTypeID = copyinfo.CuryRateTypeID;
					info.CuryRate = copyinfo.CuryRate;
					info.RecipRate = copyinfo.RecipRate;
					info.CuryMultDiv = copyinfo.CuryMultDiv;
					this.currencyinfo.Update(info);
				}
			}
			AddressAttribute.CopyRecord<SOOrder.billAddressID>(Document.Cache, Document.Current, sourceOrder, false);
			ContactAttribute.CopyRecord<SOOrder.billContactID>(Document.Cache, Document.Current, sourceOrder, false);
			AddressAttribute.CopyRecord<SOOrder.shipAddressID>(Document.Cache, Document.Current, sourceOrder, false);
			ContactAttribute.CopyRecord<SOOrder.shipContactID>(Document.Cache, Document.Current, sourceOrder, false);
			OrderCreated(Document.Current, sourceOrder);
			bool exceptionHappenedOnSOLineCopying = false;

			TaxBaseAttribute.SetTaxCalc<SOLine.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);

			if (this.customer.Current != null && this.customer.Current.CreditRule == null)
			{
				Customer cust = PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<SOOrder.customerID>>>>.Select(this);
				if (cust != null)
					this.customer.Current.CreditRule = cust.CreditRule;
			}

			string[] notSupportingPOtoSOLinkBehaviors = { SOBehavior.QT, SOBehavior.CM, SOBehavior.IN };
			string[] notSupportingPOtoSORedefaultingBehaviors = { SOBehavior.CM, SOBehavior.IN }; // it is allowed to redefault PO Source when copying from QT order

			PXResultset<SOLine> sourceSOLines =
				PXSelectReadonly<SOLine,
				Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
					And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>>>>
				.Select(this, sourceOrder.OrderType, sourceOrder.OrderNbr);
			foreach (SOLine sourceSOLine in sourceSOLines)
			{
				SOLine targetSOLine = PXCache<SOLine>.CreateCopy(sourceSOLine);
				targetSOLine.OrigOrderType = targetSOLine.OrderType;
				targetSOLine.OrigOrderNbr = targetSOLine.OrderNbr;
				targetSOLine.OrigLineNbr = targetSOLine.LineNbr;
				targetSOLine.Behavior = null;
				targetSOLine.OrderType = null;
				targetSOLine.OrderNbr = null;
				targetSOLine.InvtMult = null;
				targetSOLine.CuryInfoID = null;
				targetSOLine.PlanType = null;
				targetSOLine.TranType = null;
				targetSOLine.RequireShipping = null;
				targetSOLine.RequireAllocation = null;
				targetSOLine.RequireLocation = null;
				targetSOLine.RequireReasonCode = null;
				targetSOLine.OpenLine = null;
				targetSOLine.Completed = false;
				targetSOLine.CancelDate = null;
				//targetSOLine.POType = null;
				//targetSOLine.PONbr = null;
				//targetSOLine.POLineNbr = null;
				targetSOLine.OrderDate = null;
				targetSOLine.CommitmentID = null;
				targetSOLine.POCreated = null;

				if (targetSOLine.RequestDate < Document.Current.OrderDate)
				{
					targetSOLine.RequestDate = null;
				}
				if (targetSOLine.ShipDate < Document.Current.OrderDate)
				{
					targetSOLine.ShipDate = null;
				}

				if (notSupportingPOtoSOLinkBehaviors.Contains(sourceOrder.Behavior) || notSupportingPOtoSOLinkBehaviors.Contains(Document.Current.Behavior))
				{
					targetSOLine.POCreate = null;
					targetSOLine.POSource = null;
				}

				if (soordertype.Current.RequireLocation == true && targetSOLine.ShipComplete == SOShipComplete.BackOrderAllowed)
				{
					targetSOLine.ShipComplete = null;
				}

				if (soordertype.Current.RequireLocation == false)
				{
					targetSOLine.LocationID = null;
					targetSOLine.LotSerialNbr = null;
					targetSOLine.ExpireDate = null;
				}

				if (!IsCreditMemoOrder && !IsRMAOrder)
				{
					targetSOLine.InvoiceType = null;
					targetSOLine.InvoiceNbr = null;
					targetSOLine.InvoiceLineNbr = null;
				}

				var orderTypeOp = SOOrderTypeOperation.PK.Find(this, Document.Current.OrderType, targetSOLine.Operation);
				if (orderTypeOp == null || orderTypeOp.Active != true)
				{
					targetSOLine.Operation = null;
				}

				if (targetSOLine.IsFree == true && targetSOLine.ManualDisc == false && recalcDiscounts)
				{
					continue;
				}

				if (overrideManualDiscounts)
				{
					targetSOLine.ManualDisc = false;
					targetSOLine.ManualPrice = false;
				}

				if (recalcUnitPrices && targetSOLine.ManualPrice != true)
				{
					targetSOLine.CuryUnitPrice = null;
					targetSOLine.CuryExtPrice = null;
				}

				if (overrideManualPrices)
				{
					targetSOLine.ManualPrice = false;
				}

				if (!recalcDiscounts)
				{
					targetSOLine.ManualDisc = true;
					targetSOLine.SkipDisc = true;
				}

				targetSOLine.UnassignedQty = 0m;
				targetSOLine.OpenQty = null;
				targetSOLine.ClosedQty = 0m;
				targetSOLine.BilledQty = 0m;
				targetSOLine.UnbilledQty = null;
				targetSOLine.ShippedQty = 0m;
				targetSOLine.CuryBilledAmt = 0m;
				targetSOLine.CuryUnbilledAmt = null;
				targetSOLine.CuryOpenAmt = null;
				targetSOLine.CuryLineAmt = null;

				if (recalcDiscounts)
                {
                    targetSOLine.DocumentDiscountRate = 1;
                    targetSOLine.GroupDiscountRate = 1;
                    targetSOLine.DiscountsAppliedToLine = new ushort[0];
                }

				targetSOLine.NoteID = null;

				try
				{
					FieldUpdated.RemoveHandler<SOLine.discountID>(SOLine_DiscountID_FieldUpdated);
					try
					{
						Transactions.Cache.ForceExceptionHandling = true;
						targetSOLine = Transactions.Insert(targetSOLine);

						if (targetSOLine == null)
							continue;

						PXNoteAttribute.CopyNoteAndFiles(Transactions.Cache, sourceSOLine, Transactions.Cache, targetSOLine, ordertype);

						bool clearMarkForPO = notSupportingPOtoSORedefaultingBehaviors.Contains(sourceOrder.Behavior)
											  || notSupportingPOtoSOLinkBehaviors.Contains(Document.Current.Behavior)
											  || targetSOLine.Operation == SOOperation.Receipt;
						if (clearMarkForPO)
						{
							targetSOLine.POCreate = false;
							targetSOLine.POSource = null;
						}

						Transactions.Update(targetSOLine);
					}
					catch (PXSetPropertyException)
					{
						exceptionHappenedOnSOLineCopying = true;
					}
				}
				finally
				{
					this.FieldUpdated.AddHandler<SOLine.discountID>(SOLine_DiscountID_FieldUpdated);
				}

				Transactions.Cache.SetDefaultExt<SOLine.curyUnitCost>(targetSOLine);
			}

			bool recalcTaxes = exceptionHappenedOnSOLineCopying || recalcDiscounts || recalcUnitPrices || overrideManualDiscounts || overrideManualPrices;

			if (recalcTaxes)
			{
				TaxBaseAttribute.SetTaxCalc<SOLine.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualCalc);
			}

			PXResultset<SOTaxTran> sourceSOTaxTrans =
				PXSelect<SOTaxTran,
				Where<SOTaxTran.orderType, Equal<Required<SOTaxTran.orderType>>,
					And<SOTaxTran.orderNbr, Equal<Required<SOTaxTran.orderNbr>>>>>
				.Select(this, sourceOrder.OrderType, sourceOrder.OrderNbr);
			foreach (SOTaxTran sourceTaxTran in sourceSOTaxTrans)
			{
				var targetTaxTran = new SOTaxTran
				{
					OrderType = Document.Current.OrderType,
					OrderNbr = Document.Current.OrderNbr,
					LineNbr = int.MaxValue,
					TaxID = sourceTaxTran.TaxID
				};

				targetTaxTran = this.Taxes.Insert(targetTaxTran);

				if (!recalcTaxes && targetTaxTran != null)
				{
					targetTaxTran = PXCache<SOTaxTran>.CreateCopy(targetTaxTran);
					targetTaxTran.TaxRate = sourceTaxTran.TaxRate;
					targetTaxTran.CuryTaxableAmt = sourceTaxTran.CuryTaxableAmt;
					targetTaxTran.CuryExemptedAmt = sourceTaxTran.CuryExemptedAmt;
					targetTaxTran.CuryTaxAmt = sourceTaxTran.CuryTaxAmt;

					targetTaxTran.CuryUnshippedTaxableAmt = sourceTaxTran.CuryTaxableAmt;
					targetTaxTran.CuryUnshippedTaxAmt = sourceTaxTran.CuryTaxAmt;
					targetTaxTran.CuryUnbilledTaxableAmt = sourceTaxTran.CuryTaxableAmt;
					targetTaxTran.CuryUnbilledTaxAmt = sourceTaxTran.CuryTaxAmt;

					this.Taxes.Update(targetTaxTran);
				}
			}

			if (sourceOrder.FreightTaxCategoryID != null)
			{
				TaxBaseAttribute.SetTaxCalc<SOOrder.freightTaxCategoryID>(this.Document.Cache, null, TaxCalc.ManualLineCalc);
				Document.Current.FreightTaxCategoryID = sourceOrder.FreightTaxCategoryID;
				Document.Update(Document.Current);
			}

			if (recalcTaxes)
			{
				TaxBaseAttribute.SetTaxCalc<SOLine.taxCategoryID>(this.Transactions.Cache, null, TaxCalc.ManualLineCalc);
			}

			if (!DisableGroupDocDiscount)
			{
				//copy all discounts except free-items:
				PXResultset<SOOrderDiscountDetail> soOrderDiscountDetails =
					PXSelect<SOOrderDiscountDetail,
					Where<SOOrderDiscountDetail.orderType, Equal<Required<SOOrderDiscountDetail.orderType>>,
						And<SOOrderDiscountDetail.orderNbr, Equal<Required<SOOrderDiscountDetail.orderNbr>>,
						And<SOOrderDiscountDetail.freeItemID, IsNull>>>>
					.Select(this, sourceOrder.OrderType, sourceOrder.OrderNbr);
				foreach (SOOrderDiscountDetail sourceOrderDiscount in soOrderDiscountDetails)
				{
					if (!recalcDiscounts || sourceOrderDiscount.IsManual == true)
					{
						SOOrderDiscountDetail targetOrderDiscount = PXCache<SOOrderDiscountDetail>.CreateCopy(sourceOrderDiscount);
						targetOrderDiscount.OrderType = Document.Current.OrderType;
						targetOrderDiscount.OrderNbr = Document.Current.OrderNbr;
						targetOrderDiscount.IsManual = true;
						_discountEngine.InsertDiscountDetail(this.DiscountDetails.Cache, DiscountDetails, targetOrderDiscount);
					}
				}
			}

			RecalcDiscountsParamFilter filter = recalcdiscountsfilter.Current;
			filter.OverrideManualDiscounts = overrideManualDiscounts;
			filter.OverrideManualDocGroupDiscounts = overrideManualDiscounts;
			filter.OverrideManualPrices = overrideManualPrices;
			filter.RecalcDiscounts = recalcDiscounts;
			filter.RecalcUnitPrices = recalcUnitPrices;
			filter.RecalcTarget = RecalcDiscountsParamFilter.AllLines;
			_discountEngine.RecalculatePricesAndDiscounts(
				cache: Transactions.Cache,
				lines: Transactions,
				currentLine: Transactions.Current,
				discountDetails: DiscountDetails,
				locationID: Document.Current.CustomerLocationID,
				date: Document.Current.OrderDate,
				recalcFilter: filter,
				discountCalculationOptions: DiscountEngine.DefaultARDiscountCalculationParameters);

			RecalculateTotalDiscount();

			RefreshFreeItemLines(Transactions.Cache);
		}
		public delegate void OrderCreatedDelegate(SOOrder document, SOOrder source);
		protected virtual void OrderCreated(SOOrder document, SOOrder source)
		{

		}
		public PXAction<SOOrder> inventorySummary;
		[PXUIField(DisplayName = "Inventory Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable InventorySummary(PXAdapter adapter)
		{
			PXCache tCache = Transactions.Cache;
			SOLine line = Transactions.Current;
			if (line == null) return adapter.Get();

			InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);
			if (item != null && item.StkItem == true)
			{
				INSubItem sbitem = (INSubItem)PXSelectorAttribute.Select<SOLine.subItemID>(tCache, line);
				InventorySummaryEnq.Redirect(item.InventoryID,
											 ((sbitem != null) ? sbitem.SubItemCD : null),
											 line.SiteID,
											 line.LocationID);
			}
			return adapter.Get();
		}

		public PXAction<SOOrder> calculateFreight;
		[PXUIField(DisplayName = Messages.RefreshFreight, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable CalculateFreight(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.IsManualPackage != true && Document.Current.IsPackageValid != true)
			{
				CarrierRatesExt.RecalculatePackagesForOrder(Document.Current);
			}

			CalculateFreightCost(false);

			return adapter.Get();
		}



		public PXAction<SOOrder> createPayment;
		[PXUIField(DisplayName = "Create Payment", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew, Tooltip = "Create Payment")]
		protected virtual void CreatePayment()
		{
			if (Document.Current != null)
			{
				CheckTermsInstallmentType();

				this.Save.Press();

				PXGraph target;
				CreatePaymentProc(Document.Current, out target);

				throw new PXPopupRedirectException(target, "New Payment", true);
			}
		}

		public PXAction<SOOrder> createPrepayment;
		[PXUIField(DisplayName = "Create Prepayment", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew, Tooltip = "Create Prepayment")]
		protected virtual void CreatePrepayment()
		{
			if (Document.Current != null)
			{
				CheckTermsInstallmentType();

				this.Save.Press();

				PXGraph target;
				CreatePaymentProc(Document.Current, out target, ARPaymentType.Prepayment);

				throw new PXPopupRedirectException(target, "New Payment", true);
			}
		}

		public virtual void CheckTermsInstallmentType()
		{
			Terms terms = PXSelect<Terms, Where<Terms.termsID, Equal<Current<SOOrder.termsID>>>>.Select(this);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single)
			{
				throw new PXSetPropertyException(AR.Messages.PrepaymentAppliedToMultiplyInstallments);
			}
		}

		public virtual void CreatePaymentProc(SOOrder order, out PXGraph target, string paymentType = ARPaymentType.Payment)
		{
			ARPaymentEntry docgraph = PXGraph.CreateInstance<ARPaymentEntry>();
			target = docgraph;

			docgraph.Clear();
			ARPayment payment = new ARPayment()
			{
				DocType = paymentType,
			};

			AROpenPeriodAttribute.SetThrowErrorExternal<ARPayment.adjFinPeriodID>(docgraph.Document.Cache, true);
			payment = PXCache<ARPayment>.CreateCopy(docgraph.Document.Insert(payment));
			AROpenPeriodAttribute.SetThrowErrorExternal<ARPayment.adjFinPeriodID>(docgraph.Document.Cache, false);

			payment.CustomerID = order.CustomerID;
			payment.CustomerLocationID = order.CustomerLocationID;
			payment.PaymentMethodID = order.PaymentMethodID;
			payment.PMInstanceID = order.PMInstanceID;
			payment.CuryOrigDocAmt = order.IsCCCaptured == true ? order.CuryCCCapturedAmt : order.IsCCAuthorized == true ? order.CuryCCPreAuthAmount : 0m;
			payment.DocDesc = order.OrderDesc;
			payment.CashAccountID = order.CashAccountID;
			payment = docgraph.Document.Update(payment);

			InsertSOAdjustments(order, docgraph, payment);

			if (payment.CuryOrigDocAmt == 0m)
			{
				payment.CuryOrigDocAmt = payment.CurySOApplAmt;
				payment = docgraph.Document.Update(payment);
			}

			if (order.IsCCCaptured == true || order.IsCCAuthorized == true)
			{
				ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(this, ExternalTran);
				IExternalTransaction tran = state.ExternalTransaction;
				payment.DocDate = tran.LastActivityDate.Value.Date;
                payment.AdjDate = tran.LastActivityDate.Value.Date;
				payment.Hold = false;

				if (tran.RefNbr == null && tran.DocType == null)
				{
					payment.ExtRefNbr = order.ExtRefNbr;
				}

				payment = docgraph.Document.Update(payment);

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					docgraph.Save.Press();

					PXDatabase.Update<ExternalTransaction>(
						new PXDataFieldAssign("DocType", docgraph.Document.Current.DocType),
						new PXDataFieldAssign("RefNbr", docgraph.Document.Current.RefNbr),
						new PXDataFieldRestrict("OrigDocType", PXDbType.Char, 3, order.OrderType, PXComp.EQ),
						new PXDataFieldRestrict("OrigRefNbr", PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ),
						new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, null, PXComp.ISNULL)
					);
					PXDatabase.Update<CCProcTran>(
						new PXDataFieldAssign("DocType", docgraph.Document.Current.DocType),
						new PXDataFieldAssign("RefNbr", docgraph.Document.Current.RefNbr),
						new PXDataFieldRestrict("OrigDocType", PXDbType.Char, 3, order.OrderType, PXComp.EQ),
						new PXDataFieldRestrict("OrigRefNbr", PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ),
						new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, null, PXComp.ISNULL)
					);

					if (order.IsCCCaptured == false && order.IsCCAuthorized == true)
					{
						PXDatabase.Update<SOOrder>(
							new PXDataFieldAssign("CuryCCPreAuthAmount", 0m),
							new PXDataFieldAssign("IsCCAuthorized", false),
							new PXDataFieldRestrict("OrderType", PXDbType.Char, 3, order.OrderType, PXComp.EQ),
							new PXDataFieldRestrict("OrderNbr", PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ)
							);
					}
					ts.Complete();
				}

				if (order.IsCCCaptured == true)
				{
					this.SelectTimeStamp();
					order.IsCCCaptured = false;
					order.IsCCAuthorized = false;
					order.CuryCCCapturedAmt = 0m;
					this.Document.Update(order);
					this.Save.Press();
					if (!state.IsOpenForReview && payment.Released == false && arsetup.Current.IntegratedCCProcessing == true)
					{
						PaymentTransactionGraph<SOOrderEntry,SOOrder>.ReleaseARDocument(payment);
					}
				}
			}
		}

		protected virtual void InsertSOAdjustments(SOOrder order, ARPaymentEntry docgraph, ARPayment payment)
		{
			SOAdjust adj = new SOAdjust()
			{
				AdjdOrderType = order.OrderType,
				AdjdOrderNbr = order.OrderNbr
			};

			try
			{
				docgraph.SOAdjustments.Insert(adj);
			}
			catch (PXSetPropertyException)
			{
				if (order.IsCCCaptured == true || order.IsCCAuthorized == true)
				{
					throw;
				}
				payment.CuryOrigDocAmt = 0m;
			}
		}

		public PXAction<SOOrder> viewPayment;
		[PXUIField(DisplayName = "View Payment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton(Tooltip = "View Payment")]
		public virtual void ViewPayment()
		{
			if (Document.Current != null && Adjustments.Current != null)
			{
				ARPaymentEntry pe = PXGraph.CreateInstance<ARPaymentEntry>();
				pe.Document.Current = pe.Document.Search<ARPayment.refNbr>(Adjustments.Current.AdjgRefNbr, Adjustments.Current.AdjgDocType);

				throw new PXRedirectRequiredException(pe, true, "Payment") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
		}

		public class SOOrderMessageDisplay : IPXCustomInfo
		{
			public void Complete(PXLongRunStatus status, PXGraph graph)
			{
				if (status == PXLongRunStatus.Completed && graph is SOOrderEntry)
				{
					((SOOrderEntry)graph).RowSelected.AddHandler<SOOrder>((sender, e) =>
					{
						SOOrder order = e.Row as SOOrder;
						if (order != null)
						{
							sender.RaiseExceptionHandling<SOOrder.cCPaymentStateDescr>(order, order.CCPaymentStateDescr, _message);
						}
					});
				}
			}

			private PXSetPropertyException _message;

			public SOOrderMessageDisplay(PXSetPropertyException message)
			{
				_message = message;
			}
		}

		public class PaymentProfileHostedForm : Extensions.PaymentProfile.PaymentProfileGraph<SOOrderEntry,SOOrder>
		{
			protected override CustomerPaymentMethodMapping GetCustomerPaymentMethodMapping()
			{
				return new CustomerPaymentMethodMapping(typeof(CustomerPaymentMethodC)); 
			}

			protected override CustomerPaymentMethodDetailMapping GetCusotmerPaymentMethodDetailMapping()
			{
				return new CustomerPaymentMethodDetailMapping(typeof(AR.CustomerPaymentMethodDetail));
			}

			protected override PaymentmethodDetailMapping GetPaymentMethodDetailMapping()
			{
				return new PaymentmethodDetailMapping(typeof(CA.PaymentMethodDetail));
			}

			protected override void RowSelected(Events.RowSelected<SOOrder> e)
			{
				base.RowSelected(e);
				SOOrder row = e.Row;
				if (row == null)
					return;
				if (row.CreatePMInstance.GetValueOrDefault())
				{
					int? pmInstance = row.PMInstanceID;
					bool isHFPM = CCProcessingHelper.IsHFPaymentMethod(this.Base, pmInstance);
					bool isIDFilled = CCProcessingHelper.IsCCPIDFilled(this.Base, pmInstance);
					bool enable = isHFPM && !isIDFilled;
					this.RefreshCreatePaymentAction(enable, isHFPM);
				}
				else
				{
					this.RefreshCreatePaymentAction(false, false);
				}
				this.RefreshSyncPaymentAction(true);
				this.RefreshManagePaymentAction(false, false);
			}

			protected override void MapViews(SOOrderEntry graph)
			{
				var pmDetails = graph.DefPaymentMethodInstanceDetailsAll;
				this.CustomerPaymentMethodDetail = new PXSelectExtension<Extensions.PaymentProfile.CustomerPaymentMethodDetail>(pmDetails);		
			}
		}

		public PXAction<SOOrder> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton(/*ImageKey = PX.Web.UI.Sprite.Main.Process*/)]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (SOOrder current in adapter.Get<SOOrder>())
			{
				if (current != null)
				{
					SOBillingAddress address = this.Billing_Address.Select();
					if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
					{
						PXAddressValidator.Validate<SOBillingAddress>(this, address, true, true);
					}

					SOShippingAddress shipAddress = this.Shipping_Address.Select();
					if (shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
					{
						PXAddressValidator.Validate<SOShippingAddress>(this, shipAddress, true, true);
					}
				}
				yield return current;
			}
		}

		public PXAction<SOOrder> recalculateDiscountsAction;
		[PXUIField(DisplayName = "Recalculate Prices", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable RecalculateDiscountsAction(PXAdapter adapter)
		{
			if (adapter.MassProcess)
			{
				PXLongOperation.StartOperation(this, () => this.RecalculateDiscountsProc(false));
			}
			else if (!adapter.ExternalCall || recalcdiscountsfilter.AskExt() == WebDialogResult.OK)
			{
				if (!adapter.ExternalCall)
				{
					this.RecalculateDiscountsProc(true);
				}
				else
				{
					SOOrderEntry clone = this.Clone();
					PXLongOperation.StartOperation(this, () => clone.RecalculateDiscountsProc(true));
				}
			}
			return adapter.Get();
		}

		public PXAction<SOOrder> RecalculateDiscountsFromImport;
		[PXUIField(DisplayName = "Recalculate Discounts From Import", Visible = true)]
		[PXButton]
		public void recalculateDiscountsFromImport()
		{
			if (Document.Current != null)
			{
				_discountEngine.AutoRecalculatePricesAndDiscounts(Transactions.Cache, Transactions, null, DiscountDetails, Document.Current.CustomerLocationID, Document.Current.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current) | DiscountEngine.DiscountCalculationOptions.DisablePriceCalculation | DiscountEngine.DiscountCalculationOptions.CalculateDiscountsFromImport);
				this.Save.Press();
			}
		}

		protected virtual void RecalculateDiscountsProc(bool redirect)
		{
			_discountEngine.RecalculatePricesAndDiscounts(Transactions.Cache, Transactions, Transactions.Current, DiscountDetails, Document.Current.CustomerLocationID, Document.Current.OrderDate, recalcdiscountsfilter.Current, GetDefaultSODiscountCalculationOptions(Document.Current));
			if (redirect)
			{
				PXLongOperation.SetCustomInfo(this);
			}
			else
			{
				this.Save.Press();
			}
		}

		public PXAction<SOOrder> recalcOk;
		[PXUIField(DisplayName = "OK", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable RecalcOk(PXAdapter adapter)
		{
			return adapter.Get();
		}

		#endregion

		#region SiteStatus Lookup
		public PXFilter<SOSiteStatusFilter> sitestatusfilter;
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public SOSiteStatusLookup<SOSiteStatusSelected, SOSiteStatusFilter> sitestatus;

		public PXAction<SOOrder> addInvBySite;
		[PXUIField(DisplayName = "Add Stock Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable AddInvBySite(PXAdapter adapter)
		{
			sitestatusfilter.Cache.Clear();
			if (sitestatus.AskExt() == WebDialogResult.OK)
			{
				return AddInvSelBySite(adapter);
			}
			sitestatusfilter.Cache.Clear();
			sitestatus.Cache.Clear();
			return adapter.Get();
		}

		public PXAction<SOOrder> addInvSelBySite;
		[PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddInvSelBySite(PXAdapter adapter)
		{
			Transactions.Cache.ForceExceptionHandling = true;

			foreach (SOSiteStatusSelected line in sitestatus.Cache.Cached)
			{
				if (line.Selected == true && line.QtySelected > 0)
				{
					SOLine newline = PXCache<SOLine>.CreateCopy(Transactions.Insert(new SOLine()));
					newline.SiteID = line.SiteID;
					newline.InventoryID = line.InventoryID;
					if (line.SubItemID != null) // line.SubItemID is null when the line doesn't have INSiteStatus
						newline.SubItemID = line.SubItemID;
					
					newline.UOM = line.SalesUnit;
					newline.AlternateID = line.AlternateID;
                    newline = PXCache<SOLine>.CreateCopy(Transactions.Update(newline));
                    if (newline.RequireLocation != true)
						newline.LocationID = null;
					newline = PXCache<SOLine>.CreateCopy(Transactions.Update(newline));
					newline.Qty = line.QtySelected;
					cnt = 0;
					Transactions.Update(newline);
                }
			}
			sitestatus.Cache.Clear();
			return adapter.Get();
		}

		protected virtual void SOSiteStatusFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			SOSiteStatusFilter row = (SOSiteStatusFilter)e.Row;
			if (row != null && !PXAccess.FeatureInstalled<FeaturesSet.inventory>())
				row.OnlyAvailable = false;
		}
		protected virtual void SOSiteStatusFilter_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			SOSiteStatusFilter row = (SOSiteStatusFilter)e.Row;
			if (row != null && Document.Current != null)
				row.SiteID = Document.Current.DefaultSiteID;
		}
		int cnt;
		public override IEnumerable<PXDataRecord> ProviderSelect(BqlCommand command, int topCount, params PXDataValue[] pars)
		{
			cnt++;
			return base.ProviderSelect(command, topCount, pars);
		}
		#endregion

		#region CurrencyInfo events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.Row != null && IsCopyOrder)
				{
					e.NewValue = ((CurrencyInfo)e.Row).CuryID ?? customer?.Current?.CuryID;
					e.Cancel = true;
				}
				else
				{
					if (customer.Current != null && !string.IsNullOrEmpty(customer.Current.CuryID))
					{
						e.NewValue = customer.Current.CuryID;
						e.Cancel = true;
					}
				}


			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.Row != null && IsCopyOrder)
				{
					e.NewValue = ((CurrencyInfo)e.Row).CuryRateTypeID;
					e.Cancel = true;
				}
				else
				{
					if (customer.Current != null && !string.IsNullOrEmpty(customer.Current.CuryRateTypeID))
					{
						e.NewValue = customer.Current.CuryRateTypeID;
						e.Cancel = true;
					}
					else
					{
						CMSetup cmsetup = PXSelect<CMSetup>.Select(this);
						if (cmsetup != null)
						{
							e.NewValue = cmsetup.ARRateTypeDflt;
							e.Cancel = true;
						}
					}
				}

			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((SOOrder)Document.Cache.Current).OrderDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.AllowUpdate(this.Transactions.Cache);

				if (customer.Current != null && !(bool)customer.Current.AllowOverrideRate)
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyenabled);
			}
		}
		#endregion

		public override void InitCacheMapping(Dictionary<Type, Type> map)
		{
			base.InitCacheMapping(map);

			this.Caches.AddCacheMapping(typeof(INLotSerialStatus), typeof(INLotSerialStatus));
			this.Caches.AddCacheMapping(typeof(CustomerPaymentMethod), typeof(CustomerPaymentMethodC));
			this.Caches.AddCacheMapping(typeof(CustomerPaymentMethod), typeof(CustomerPaymentMethod));
		}

		protected virtual void ParentFieldUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<SOOrder.orderDate, SOOrder.curyID>(e.Row, e.OldRow))
			{
				foreach (SOLine tran in Transactions.Select())
				{
					Transactions.Cache.MarkUpdated(tran);
				}
			}
		}

		public SOOrderEntry()
		{
			RowUpdated.AddHandler<SOOrder>(ParentFieldUpdated);

			shipmentlist.Cache.AllowUpdate = false; // Needs to save notes (see PXNoteAttribute.NoteTextGenericFieldUpdating: "!sender.AllowUpdate")

			PXUIFieldAttribute.SetVisible<SOOrderShipment.operation>(shipmentlist.Cache, null, false);
			PXUIFieldAttribute.SetVisible<SOOrderShipment.orderType>(shipmentlist.Cache, null, false);
			PXUIFieldAttribute.SetVisible<SOOrderShipment.orderNbr>(shipmentlist.Cache, null, false);
			PXUIFieldAttribute.SetVisible<SOOrderShipment.shipmentNbr>(shipmentlist.Cache, null, false);

			{
				SOSetup record = sosetup.Current;
			}

			ARSetupNoMigrationMode.EnsureMigrationModeDisabled(this);

			PXFieldState state = (PXFieldState)this.Transactions.Cache.GetStateExt<SOLine.inventoryID>(null);
			viewInventoryID = state != null ? state.ViewName : null;

			PXUIFieldAttribute.SetVisible<SOLine.taskID>(Transactions.Cache, null, PM.ProjectAttribute.IsPMVisible(BatchModule.SO));

			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.CustomerType; });
			FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null && InventoryHelper.CanCreateStockItem(sender.Graph) == false) e.NewValue = false; });
			PXUIFieldAttribute.SetEnabled(invoicesplits.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<InvoiceSplits.selected>(invoicesplits.Cache, null, true);

			if (!PXAccess.FeatureInstalled<FeaturesSet.carrierIntegration>())
			{
				CarrierRatesExt.shopRates.SetCaption(PXMessages.LocalizeNoPrefix(Messages.Packages));
			}

			if (!PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				addInvBySite.SetCaption(PXMessages.LocalizeNoPrefix(Messages.NonStockItem));
			}
		}


		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<SOOrder>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(SOLine), (graph) =>
					{
						return new PXDataFieldValue[]
						{
								new PXDataFieldValue<SOLine.orderType>(PXDbType.Char, ((SOOrderEntry)graph).Document.Current?.OrderType),
								new PXDataFieldValue<SOLine.orderNbr>(((SOOrderEntry)graph).Document.Current?.OrderNbr)
						};
					}),
					new TableQuery(TransactionTypes.SerialsPerDocument, typeof(SOLineSplit), (graph) =>
					{
						SOOrder order = ((SOOrderEntry)graph).Document.Current;
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<SOLineSplit.orderType>(PXDbType.Char, order?.Behavior.IsIn(SOBehavior.CM, SOBehavior.IN) == true ? order?.OrderType : null),
							new PXDataFieldValue<SOLineSplit.orderNbr>(order?.Behavior.IsIn(SOBehavior.CM, SOBehavior.IN) == true ? order?.OrderNbr : null)
						};
					}));
			}
		}

		#region SOAdjust Events
		protected virtual void SOAdjust_AdjgRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOOrder.curyInfoID>>>>.Select(this);
			SOAdjust adj = (SOAdjust)e.Row;

			PXSelectBase<ARPayment> s = new PXSelectReadonly2<ARPayment,
				InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>>,
				Where<ARPayment.customerID, In3<Current<SOOrder.customerID>, Current<Customer.consolidatingBAccountID>>,
					And2<Where<ARPayment.docType, Equal<ARDocType.payment>, Or<ARPayment.docType, Equal<ARDocType.prepayment>, Or<ARPayment.docType, Equal<ARDocType.creditMemo>>>>,
					And<ARPayment.openDoc, Equal<boolTrue>,
					And<ARPayment.docType, Equal<Required<ARPayment.docType>>,
					And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>>>>(this);

			foreach (PXResult<ARPayment, CurrencyInfo> res in s.Select(adj.AdjgDocType, adj.AdjgRefNbr))
			{
				ARPayment payment = PXCache<ARPayment>.CreateCopy(res);

				CurrencyInfo pay_info = (CurrencyInfo)res;

				adj.CustomerID = Document.Current.CustomerID;
				adj.AdjdOrderType = Document.Current.OrderType;
				adj.AdjdOrderNbr = Document.Current.OrderNbr;
				adj.AdjgDocType = payment.DocType;
				adj.AdjgRefNbr = payment.RefNbr;

				SOAdjust other = PXSelectGroupBy<SOAdjust, Where<SOAdjust.adjgDocType, Equal<Required<SOAdjust.adjgDocType>>, And<SOAdjust.adjgRefNbr, Equal<Required<SOAdjust.adjgRefNbr>>, And<Where<SOAdjust.adjdOrderType, NotEqual<Required<SOAdjust.adjdOrderType>>, Or<SOAdjust.adjdOrderNbr, NotEqual<Required<SOAdjust.adjdOrderNbr>>>>>>>, Aggregate<GroupBy<SOAdjust.adjgDocType, GroupBy<SOAdjust.adjgRefNbr, Sum<SOAdjust.curyAdjgAmt, Sum<SOAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr, adj.AdjdOrderType, adj.AdjdOrderNbr);
				if (other != null && other.AdjdOrderNbr != null)
				{
					payment.CuryDocBal -= other.CuryAdjgAmt;
					payment.DocBal -= other.AdjAmt;
				}

				ARAdjust fromar = PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<ARAdjust.released, Equal<boolFalse>>>>, Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, Sum<ARAdjust.curyAdjgAmt, Sum<ARAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr);
				if (fromar != null && fromar.AdjdRefNbr != null)
				{
					payment.CuryDocBal -= fromar.CuryAdjgAmt;
					payment.DocBal -= fromar.AdjAmt;
				}

				if (Adjustments.Cache.Locate(adj) == null)
				{
					adj.AdjgCuryInfoID = payment.CuryInfoID;
					adj.AdjdOrigCuryInfoID = Document.Current.CuryInfoID;
					//if LE constraint is removed from payment selection this must be reconsidered
					adj.AdjdCuryInfoID = Document.Current.CuryInfoID;

					decimal CuryDocBal;
					if (string.Equals(pay_info.CuryID, inv_info.CuryID))
					{
						CuryDocBal = (decimal)payment.CuryDocBal;
					}
					else
					{
						decimal docBal = ((payment.Released == true) ? payment.DocBal : payment.OrigDocAmt) ?? 0m;

						PXDBCurrencyAttribute.CuryConvCury(Adjustments.Cache, inv_info, docBal, out CuryDocBal);
					}
					adj.CuryDocBal = CuryDocBal;
				}
			}
		}

		protected virtual void SOAdjust_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = true;
			e.Cancel = true;
		}

		protected virtual void SOAdjust_CuryAdjdAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOAdjust adj = (SOAdjust)e.Row;
			Terms terms = PXSelect<Terms, Where<Terms.termsID, Equal<Current<SOOrder.termsID>>>>.Select(this);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(AR.Messages.PrepaymentAppliedToMultiplyInstallments);
			}

			if (adj.AdjgCuryInfoID == null || adj.CuryDocBal == null)
			{
				PXResult<ARPayment, CurrencyInfo> res = (PXResult<ARPayment, CurrencyInfo>)PXSelectJoin<ARPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>>, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>, And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr);

				ARPayment payment = PXCache<ARPayment>.CreateCopy(res);
				CurrencyInfo pay_info = (CurrencyInfo)res;
				CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOOrder.curyInfoID>>>>.Select(this);

				SOAdjust other = PXSelectGroupBy<SOAdjust, Where<SOAdjust.adjgDocType, Equal<Required<SOAdjust.adjgDocType>>, And<SOAdjust.adjgRefNbr, Equal<Required<SOAdjust.adjgRefNbr>>, And<Where<SOAdjust.adjdOrderType, NotEqual<Required<SOAdjust.adjdOrderType>>, Or<SOAdjust.adjdOrderNbr, NotEqual<Required<SOAdjust.adjdOrderNbr>>>>>>>, Aggregate<GroupBy<SOAdjust.adjgDocType, GroupBy<SOAdjust.adjgRefNbr, Sum<SOAdjust.curyAdjgAmt, Sum<SOAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr, adj.AdjdOrderType, adj.AdjdOrderNbr);
				if (other != null && other.AdjdOrderNbr != null)
				{
					payment.CuryDocBal -= other.CuryAdjgAmt;
					payment.DocBal -= other.AdjAmt;
				}

				ARAdjust fromar = PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>, And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>, And<ARAdjust.released, Equal<boolFalse>>>>, Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr, Sum<ARAdjust.curyAdjgAmt, Sum<ARAdjust.adjAmt>>>>>>.Select(this, adj.AdjgDocType, adj.AdjgRefNbr);
				if (fromar != null && fromar.AdjdRefNbr != null)
				{
					payment.CuryDocBal -= fromar.CuryAdjgAmt;
					payment.DocBal -= fromar.AdjAmt;
				}

				decimal CuryDocBal;
				if (string.Equals(pay_info.CuryID, inv_info.CuryID))
				{
					CuryDocBal = (decimal)payment.CuryDocBal;
				}
				else
				{
					decimal docBal = ((payment.Released == true) ? payment.DocBal : payment.OrigDocAmt) ?? 0m;

					PXDBCurrencyAttribute.CuryConvCury(sender, inv_info, docBal, out CuryDocBal);
				}

				adj.CuryDocBal = CuryDocBal - adj.CuryAdjdAmt;
				adj.AdjgCuryInfoID = payment.CuryInfoID;
			}

			if (adj.AdjdCuryInfoID == null || adj.AdjdOrigCuryInfoID == null)
			{
				adj.AdjdCuryInfoID = Document.Current.CuryInfoID;
				adj.AdjdOrigCuryInfoID = Document.Current.CuryInfoID;
			}

			if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(AR.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjdAmt).ToString());
			}
		}

		protected virtual void SOAdjust_CuryAdjdAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOAdjust adj = (SOAdjust)e.Row;
			decimal CuryAdjgAmt;
			decimal AdjdAmt;
			decimal AdjgAmt;

			PXDBCurrencyAttribute.CuryConvBase<SOAdjust.adjdCuryInfoID>(sender, e.Row, (decimal)adj.CuryAdjdAmt, out AdjdAmt);

			CurrencyInfo pay_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOAdjust.adjgCuryInfoID>>>>.SelectSingleBound(this, new object[] { adj });
			CurrencyInfo inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOAdjust.adjdCuryInfoID>>>>.SelectSingleBound(this, new object[] { adj });

			if (string.Equals(pay_info.CuryID, inv_info.CuryID))
			{
				CuryAdjgAmt = (decimal)adj.CuryAdjdAmt;
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvCury<SOAdjust.adjgCuryInfoID>(sender, e.Row, AdjdAmt, out CuryAdjgAmt);
			}

			if (object.Equals(pay_info.CuryID, inv_info.CuryID) && object.Equals(pay_info.CuryRate, inv_info.CuryRate) && object.Equals(pay_info.CuryMultDiv, inv_info.CuryMultDiv))
			{
				AdjgAmt = AdjdAmt;
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvBase<SOAdjust.adjgCuryInfoID>(sender, e.Row, CuryAdjgAmt, out AdjgAmt);
			}

			adj.CuryAdjgAmt = CuryAdjgAmt;
			adj.AdjAmt = AdjdAmt;
			adj.RGOLAmt = AdjgAmt - AdjdAmt;
			adj.CuryDocBal = adj.CuryDocBal + (decimal?)e.OldValue - adj.CuryAdjdAmt;
		}

		protected virtual void SOAdjust_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOAdjust row = e.Row as SOAdjust;
			if (row == null) return;

			viewPayment.SetEnabled(!string.IsNullOrEmpty(row.AdjgRefNbr) && !string.IsNullOrEmpty(row.AdjgRefNbr));
			PXUIFieldAttribute.SetEnabled<SOAdjust.adjgDocType>(sender, row, row.AdjgRefNbr == null);
		}

		#endregion


		#region SOOrderShipment Events
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDBGuidAttribute))]
		[CopiedShipmentNoteID(IsKey = true)]
		protected virtual void SOOrderShipment_ShippingRefNoteID_CacheAttached(PXCache sender)
		{
		}

		[PXDBGuid]
		protected virtual void _(Events.CacheAttached<SOOrderShipment.orderNoteID> args)
		{
		}

		protected virtual void SOOrderShipment_ShipmentNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void SOOrderShipment_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<SOOrderShipment.selected>(sender, e.Row, true);
		}
		#endregion

		#region SOOrder Events

		protected virtual void SOOrder_Cancelled_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOOrder row = (SOOrder)e.Row;
			PXResultset<ExternalTransaction> trans = PXSelect<ExternalTransaction, 
				Where<ExternalTransaction.origRefNbr, Equal<Current<SOOrder.orderNbr>>,
					And<ExternalTransaction.origDocType, Equal<Current<SOOrder.orderType>>,
					And<ExternalTransaction.refNbr, IsNull,
					And<ExternalTransaction.docType, IsNull>>>>>
			.Select(this);
			ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(sender.Graph, trans.RowCast<ExternalTransaction>());
			ExternalTranHelper.UpdateCCPaymentState(row, state);
			if (row != null && row.IsCCAuthorized == true)
			{
				bool authIsValid = false;
				if (row.IsCCAuthorized == true && row.CCAuthExpirationDate.HasValue)
				{
					authIsValid = row.CCAuthExpirationDate.Value > PXTimeZoneInfo.Now;
				}
				if (authIsValid)
				{
					throw new PXSetPropertyException<SOOrder.cCPaymentStateDescr>(Messages.CannotCancelCCProcessed);
				}
			}

			if (row != null && (bool?)e.NewValue == true)
			{
				SOOrderShipment openShipment = PXSelectReadonly<SOOrderShipment,
					Where<SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>,
						And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>,
						And<SOOrderShipment.confirmed, Equal<False>>>>>
					.SelectSingleBound(this, new object[] { row });
				if (openShipment != null)
				{
					throw new PXException(Messages.OrderCantBeCancelled, row.OrderNbr, row.OrderType, openShipment.ShipmentNbr);
				}
			}
		}

		protected virtual void SOOrder_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				//Do not redefault if value exists and overide flag is ON:
				if (!string.IsNullOrEmpty(row.TaxZoneID) && row.OverrideTaxZone == true)
				{
					e.NewValue = row.TaxZoneID;
					return;
				}

				Location customerLocation = location.Select();
				if (customerLocation != null)
				{
					if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
					{
						TaxZone taxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(this, customerLocation.CTaxZoneID);
						if (taxZone != null)
						{
							e.NewValue = customerLocation.CTaxZoneID;
							return;
						}
					}
				}

				if (IsCommonCarrier(row.ShipVia))
				{
					SOAddress address = Shipping_Address.Select();
					if (address != null && !string.IsNullOrEmpty(address.PostalCode))
						e.NewValue = TaxBuilderEngine.GetTaxZoneByZip(this, address.PostalCode);
				}
				else
				{
					BAccount companyAccount = PXSelectJoin<BAccountR, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(this, row.BranchID);
					if (companyAccount != null)
					{
						Location companyLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(this, companyAccount.BAccountID, companyAccount.DefLocationID);
						if (companyLocation != null)
							e.NewValue = companyLocation.VTaxZoneID;
					}
				}
			}
		}

		protected virtual void SOOrder_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOOrder.taxZoneID>(e.Row);
		}

		protected virtual void SOOrder_CreditHold_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null && PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>())
			{
				SOSetupApproval setupApproval = SetupApproval.Select();
				if (setupApproval != null && (setupApproval.IsActive ?? false))
				{
					sender.RaiseFieldUpdated<SOOrder.hold>(row, true);
				}
			}
		}

		protected virtual bool IsCommonCarrier(string carrierID)
		{
			if (string.IsNullOrEmpty(carrierID))
			{
				return false; //pickup;
			}
			else
			{
				Carrier carrier = Carrier.PK.Find(this, carrierID);
				if (carrier == null)
				{
					return false;
				}
				else
				{
					return carrier.IsCommonCarrier == true;
				}
			}
		}

		protected virtual void SOOrder_OrderType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (orderType != null)
			{
				e.NewValue = orderType;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_DestinationSiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null || !IsTransferOrder)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_DestinationSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Company.RaiseFieldUpdated(sender, e.Row);
			Branch company = null;
			string destinationSiteIdErrorMessage = string.Empty;

			using (new PXReadBranchRestrictedScope())
			{
				company = Company.Select();

			if (IsTransferOrder && company != null)
			{
				sender.SetValueExt<SOOrder.customerID>(e.Row, company.BranchCD);
			}

			try
			{
				SOShippingAddressAttribute.DefaultRecord<SOOrder.shipAddressID>(sender, e.Row);
			}
			catch (SharedRecordMissingException)
			{
				var missingaddressexception = new PXSetPropertyException(Messages.DestinationSiteAddressMayNotBeEmpty, PXErrorLevel.Error);
				if (UnattendedMode)
					throw missingaddressexception;
				else
					sender.RaiseExceptionHandling<SOOrder.destinationSiteID>(e.Row, sender.GetValueExt<SOOrder.destinationSiteID>(e.Row), missingaddressexception);

				destinationSiteIdErrorMessage = Messages.DestinationSiteAddressMayNotBeEmpty;
				sender.SetValueExt<SOOrder.shipAddressID>(e.Row, null);
			}
			try
			{
				SOShippingContactAttribute.DefaultRecord<SOOrder.shipContactID>(sender, e.Row);
			}
			catch (SharedRecordMissingException)
			{
				var missingcontactexception = new PXSetPropertyException(Messages.DestinationSiteContactMayNotBeEmpty, PXErrorLevel.Error);
				if (UnattendedMode)
					throw missingcontactexception;
				else
					sender.RaiseExceptionHandling<SOOrder.destinationSiteID>(e.Row, sender.GetValueExt<SOOrder.destinationSiteID>(e.Row), missingcontactexception);

				destinationSiteIdErrorMessage = Messages.DestinationSiteContactMayNotBeEmpty;
				sender.SetValueExt<SOOrder.shipContactID>(e.Row, null);
			}
			}

			sender.SetValueExt<SOOrder.destinationSiteIdErrorMessage>(e.Row, destinationSiteIdErrorMessage);
		}

		protected virtual void SOOrder_CustomerLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (!IsTransferOrder)
			{
			sender.SetDefaultExt<SOOrder.branchID>(e.Row);
			}
			sender.SetDefaultExt<SOOrder.salesPersonID>(e.Row);
			sender.SetDefaultExt<SOOrder.taxZoneID>(e.Row);
			sender.SetDefaultExt<SOOrder.taxCalcMode>(e.Row);
			sender.SetDefaultExt<SOOrder.avalaraCustomerUsageType>(e.Row);
			sender.SetDefaultExt<SOOrder.workgroupID>(e.Row);
			sender.SetDefaultExt<SOOrder.shipVia>(e.Row);
			sender.SetDefaultExt<SOOrder.fOBPoint>(e.Row);
			sender.SetDefaultExt<SOOrder.resedential>(e.Row);
			sender.SetDefaultExt<SOOrder.saturdayDelivery>(e.Row);
			sender.SetDefaultExt<SOOrder.groundCollect>(e.Row);
			sender.SetDefaultExt<SOOrder.insurance>(e.Row);
			sender.SetDefaultExt<SOOrder.shipTermsID>(e.Row);
			sender.SetDefaultExt<SOOrder.shipZoneID>(e.Row);
			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				sender.SetDefaultExt<SOOrder.defaultSiteID>(e.Row);
			}
			sender.SetDefaultExt<SOOrder.priority>(e.Row);
			if (customerChanged)
			{
				if (!HasDetailRecords())
					sender.SetDefaultExt<SOOrder.shipComplete>(e.Row);
			}
			else
			{
				sender.SetDefaultExt<SOOrder.shipComplete>(e.Row);
			}
			sender.SetDefaultExt<SOOrder.shipDate>(e.Row);

			try
			{
				try
				{
					SOShippingAddressAttribute.DefaultRecord<SOOrder.shipAddressID>(sender, e.Row);
				}
				catch (SharedRecordMissingException)
				{
					var missingaddressexception = new PXSetPropertyException(Messages.DestinationSiteAddressMayNotBeEmpty, PXErrorLevel.Error);
					if (UnattendedMode)
						throw missingaddressexception;
					else
						sender.RaiseExceptionHandling<SOOrder.destinationSiteID>(e.Row, sender.GetValueExt<SOOrder.destinationSiteID>(e.Row), missingaddressexception);
				}
				try
				{
					SOShippingContactAttribute.DefaultRecord<SOOrder.shipContactID>(sender, e.Row);
				}
				catch (SharedRecordMissingException)
				{
					var missingcontactexception = new PXSetPropertyException(Messages.DestinationSiteContactMayNotBeEmpty, PXErrorLevel.Error);
					if (UnattendedMode)
						throw missingcontactexception;
					else
						sender.RaiseExceptionHandling<SOOrder.destinationSiteID>(e.Row, sender.GetValueExt<SOOrder.destinationSiteID>(e.Row), missingcontactexception);
				}
			}
			catch (PXFieldValueProcessingException ex)
			{
				ex.ErrorValue = location.Current.LocationCD;
				throw;
			}

			foreach (SOLine line in Transactions.Select())
			{
				try
				{
					Transactions.Cache.SetDefaultExt<SOLine.salesAcctID>(line);
				}
				catch (PXSetPropertyException)
				{
					Transactions.Cache.SetValue<SOLine.salesAcctID>(line, null);
				}
			}
		}

		protected virtual void SOOrder_CustomerID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row == null) return;

			if (!HasDetailRecords())
				return;

			Customer newCustomer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, e.NewValue);
			if (newCustomer != null)
			{
				if (newCustomer.AllowOverrideCury != true && newCustomer.CuryID != row.CuryID && !string.IsNullOrEmpty(newCustomer.CuryID))
				{
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnOrderWithRestrictedCurrency);
				}

				if (currencyinfo.Current != null &&
					currencyinfo.Current.CuryID != currencyinfo.Current.BaseCuryID &&
					newCustomer.AllowOverrideRate != true &&
					newCustomer.CuryRateTypeID != currencyinfo.Current.CuryRateTypeID)
				{
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnOrderWithRestrictedRateType);
				}
			}

			if (Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Inserted)
			{
				SOOrderShipment topShipment = shipmentlist.SelectSingle();
				if (topShipment != null)
				{
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnShippedOrder);
				}

				PXSelectBase<POLine> selectlinkedDropShips = new PXSelectJoin<POLine,
					InnerJoin<SOLineSplit, On<SOLineSplit.pOType, Equal<POLine.orderType>, And<SOLineSplit.pONbr, Equal<POLine.orderNbr>, And<SOLineSplit.pOLineNbr, Equal<POLine.lineNbr>>>>>,
					Where<SOLineSplit.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOLineSplit.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					And<POLine.orderType, Equal<POOrderType.dropShip>>>>>(this);

				POLine topPOLine = selectlinkedDropShips.SelectWindowed(0, 1);
				if (topPOLine != null)
				{
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnOrderWithDropShip);
				}

				if (Document.Current.Behavior == SOBehavior.QT)
				{
				SOOrder associatedOrder = PXSelect<SOOrder, Where<SOOrder.origOrderType, Equal<Current<SOOrder.orderType>>,
				And<SOOrder.origOrderNbr, Equal<Current<SOOrder.orderNbr>>>>>.Select(this);
				if (associatedOrder != null && associatedOrder.CustomerID != (int?)e.NewValue)
				{
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnQuoteWithOrder);
				}
				}

				if (row.IsCCAuthorized == true || row.IsCCCaptured == true)
				{
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnOrderWithCCProcessed);
				}

				SOAdjust topSOAdjust = Adjustments.SelectSingle();
				if (topSOAdjust != null)
				{
					RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnOrderWithARPayments);
				}
			}

			SOLine invoiced = (SOLine)Transactions.Select().AsEnumerable().Where(res => !string.IsNullOrEmpty(((SOLine)res).InvoiceNbr)).FirstOrDefault();
			if (invoiced != null)
			{
				RaiseCustomerIDSetPropertyException(sender, row, e.NewValue, Messages.CustomerChangedOnOrderWithInvoices);
			}


		}

		public virtual void RaiseCustomerIDSetPropertyException(PXCache sender, SOOrder order, object newCustomerID, string error)
		{
			BAccountR newAccount = (BAccountR)PXSelectorAttribute.Select<SOOrder.customerID>(sender, order, newCustomerID);
			var ex = new PXSetPropertyException(error);
			ex.ErrorValue = newAccount?.AcctCD;
			throw ex;
		}

		public virtual bool HasDetailRecords()
		{
			if (Transactions.Current != null)
				return true;

			if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Inserted)
			{
				return Transactions.Cache.IsDirty;
			}
			else
			{
				return Transactions.Select().Count > 0;
			}
		}

		protected bool customerChanged = false;

		[PopupMessage]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void SOOrder_CustomerID_CacheAttached(PXCache sender)
		{
		}


		protected virtual void SOOrder_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			int? oldCustomerID = (int?)e.OldValue;
			customerChanged = oldCustomerID != null && ((SOOrder)e.Row).CustomerID != oldCustomerID;

			sender.SetValue<SOOrder.creditHold>(e.Row, false);
			sender.SetValue<SOOrder.inclCustOpenOrders>(e.Row, true);
			sender.SetValue<SOOrder.extRefNbr>(e.Row, null);
			sender.SetValue<SOOrder.approvedCredit>(e.Row, false);
			sender.SetValue<SOOrder.approvedCreditAmt>(e.Row, 0m);
			sender.SetValue<SOOrder.overrideTaxZone>(e.Row, false);
			sender.SetDefaultExt<SOOrder.paymentMethodID>(e.Row);
			sender.SetDefaultExt<SOOrder.billSeparately>(e.Row);
			sender.SetDefaultExt<SOOrder.shipSeparately>(e.Row);
			sender.SetValue<SOOrder.origOrderType>(e.Row, null);
			sender.SetValue<SOOrder.origOrderNbr>(e.Row, null);
			sender.SetDefaultExt<SOOrder.projectID>(e.Row);

			if (customerChanged)
			{
				sender.SetValue<SOOrder.customerRefNbr>(e.Row, null);
			}

			if (!e.ExternalCall && customer.Current != null)
			{
				customer.Current.CreditRule = null;
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>()
				&& (e.ExternalCall || sender.GetValuePending<SOOrder.curyID>(e.Row) == null))
			{
				if (oldCustomerID == null || customerChanged && !HasDetailRecords())
				{
					CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<SOOrder.curyInfoID>(sender, e.Row);

					string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
					if (string.IsNullOrEmpty(message) == false)
					{
						sender.RaiseExceptionHandling<SOOrder.orderDate>(e.Row, ((SOOrder)e.Row).OrderDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
					}

					if (info != null)
					{
						sender.SetValue<SOOrder.curyID>(e.Row, info.CuryID);
					}
				}
				else
				{
					// We should not change the currency when we have details, just reloading rate by effective date.
					CurrencyInfoAttribute.SetEffectiveDate<SOOrder.orderDate>(sender, e);
				}
			}

			{
				sender.SetDefaultExt<SOOrder.customerLocationID>(e.Row);
				if (e.ExternalCall || sender.GetValuePending<SOOrder.termsID>(e.Row) == null)
				{
					if (soordertype.Current.ARDocType != ARDocType.CreditMemo)
					{
						sender.SetDefaultExt<SOOrder.termsID>(e.Row);
					}
					else
					{
						sender.SetValueExt<SOOrder.termsID>(e.Row, null);
					}
				}
				//sender.SetDefaultExt<SOOrder.pMInstanceID>(e.Row);
			}


			try
			{
				SOBillingAddressAttribute.DefaultRecord<SOOrder.billAddressID>(sender, e.Row);
				SOBillingContactAttribute.DefaultRecord<SOOrder.billContactID>(sender, e.Row);
			}
			catch (PXFieldValueProcessingException ex)
			{
				ex.ErrorValue = customer.Current.AcctCD;
				throw;
			}
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				sender.SetDefaultExt<SOOrder.ownerID>(row);

			}
			sender.SetDefaultExt<SOOrder.taxZoneID>(e.Row);
			sender.SetDefaultExt<SOOrder.createPMInstance>(e.Row);

			foreach (SOLine line in Transactions.Select())
			{
				line.CustomerID = Document.Current.CustomerID;
				Transactions.Update(line);
			}
			if (row.ProjectID != null)
			{
				//show warning if Project.Customer - Customer missmatch
				object val = row.ProjectID;
				sender.RaiseFieldVerifying<SOOrder.projectID>(row, ref val);
			}
			sender.SetValue<SOOrder.emailed>(row, false);
		}

		protected virtual void SOOrder_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOOrder.pMInstanceID>(e.Row);
			sender.SetDefaultExt<SOOrder.cashAccountID>(e.Row);

			var doc = (SOOrder)e.Row;

			if (IsPaymentInfoEnabled && doc.BillSeparately == false && !string.IsNullOrEmpty(doc.PaymentMethodID))
			{
				PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);

				if (pm.PaymentType == PaymentMethodType.CreditCard)
				{
					sender.SetValueExt<SOOrder.billSeparately>(doc, true);
				}
			}
		}

		protected virtual void SOOrder_PMInstanceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOOrder.cashAccountID>(e.Row);
			sender.SetValueExt<SOOrder.refTranExtNbr>(e.Row, null);

			var doc = (SOOrder)e.Row;

			if (IsPaymentInfoEnabled && doc.BillSeparately == false && !string.IsNullOrEmpty(doc.PaymentMethodID))
			{
				PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);

				if (pm.PaymentType == PaymentMethodType.CreditCard)
				{
					sender.SetValueExt<SOOrder.billSeparately>(doc, true);
				}
			}
			this.DefPaymentMethodInstance.Current = this.DefPaymentMethodInstance.Select();	// workaround for AC-119536
		}

		protected virtual void SOOrder_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOOrder doc = (SOOrder)e.Row;

			PXDefaultAttribute.SetPersistingCheck<SOOrder.invoiceDate>(sender, doc, (IsInvoiceOrder && doc.BillSeparately == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.finPeriodID>(sender, doc, (IsInvoiceOrder && doc.BillSeparately == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.invoiceNbr>(sender, doc, (IsInvoiceOrder && IsUserInvoiceNumbering) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.termsID>(sender, doc, (doc.ARDocType != ARDocType.Undefined
							&& doc.ARDocType != ARDocType.CreditMemo) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			bool pmInstanceRequired = false;
			bool isPMCreditCard = false;
			if (this.IsPaymentInfoEnabled && String.IsNullOrEmpty(doc.PaymentMethodID) == false)
			{
				PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);
				pmInstanceRequired = (pm != null && pm.IsAccountNumberRequired == true);
				isPMCreditCard = pm != null && pm.PaymentType == PaymentMethodType.CreditCard;
			}
			PXDefaultAttribute.SetPersistingCheck<SOOrder.pMInstanceID>(sender, doc, pmInstanceRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.dueDate>(sender, doc, (IsInvoiceOrder && doc.BillSeparately == true
							&& doc.ARDocType != ARDocType.CreditMemo) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.discDate>(sender, doc, (IsInvoiceOrder && doc.BillSeparately == true
							&& doc.ARDocType != ARDocType.CreditMemo) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.paymentMethodID>(sender, doc, (IsInvoiceOrder && doc.BillSeparately == true && (doc.ARDocType == ARDocType.CashSale
							|| doc.ARDocType == ARDocType.CashReturn)) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.cashAccountID>(sender, doc, (IsInvoiceOrder && doc.BillSeparately == true && (doc.ARDocType == ARDocType.CashSale
							|| doc.ARDocType == ARDocType.CashReturn) && !string.IsNullOrEmpty(doc.PaymentMethodID)) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.extRefNbr>(sender, doc, (IsInvoiceOrder && doc.BillSeparately == true && (doc.ARDocType == ARDocType.CashSale
							|| doc.ARDocType == ARDocType.CashReturn) && !isPMCreditCard && arsetup.Current.RequireExtRef == true) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<SOOrder.destinationSiteID>(sender, doc, IsTransferOrder ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() && orderType != SOOrderTypeConstants.TransferOrder && doc.CuryInfoID != null)
				{
					var curinfo = (CurrencyInfo)currencyinfo.SelectWindowed(0, 1, doc.CuryInfoID);
					CMSetup cmsetup = PXSelect<CMSetup>.Select(this);

					if (cmsetup != null && curinfo != null && curinfo.BaseCuryID != curinfo.CuryID && curinfo.CuryRateTypeID == null)
						throw new PXRowPersistingException(typeof(SOOrder.curyID).Name, doc.CuryID, Messages.EmptyCurrencyRateType);
				}

				var state = ExternalTranHelper.GetActiveTransactionState(this,ExternalTran);
				if (((IsInvoiceOrder && doc.BillSeparately == true && (doc.ARDocType == ARDocType.CashSale || doc.ARDocType == ARDocType.CashReturn)) || doc.ARDocType == ARDocType.Invoice)
						&& string.IsNullOrEmpty(doc.PreAuthTranNumber) == false && !state.IsPreAuthorized && !state.IsCaptured
						&& (doc.CuryCCPreAuthAmount <= decimal.Zero))
				{
					if (sender.RaiseExceptionHandling<SOOrder.curyCCPreAuthAmount>(e.Row, doc.CuryCCPreAuthAmount, new PXSetPropertyException(Messages.PreAutorizationAmountShouldBeEntered)))
					{
						throw new PXRowPersistingException(typeof(SOOrder.preAuthTranNumber).Name, doc.PreAuthTranNumber, Messages.PreAutorizationAmountShouldBeEntered);
					}
				}

				if ((doc.CuryDiscTot ?? 0m) > Math.Abs((doc.CuryLineTotal ?? 0m) + (doc.CuryMiscTot ?? 0m)))
				{
					if (sender.RaiseExceptionHandling<SOOrder.curyDiscTot>(e.Row, doc.CuryDiscTot, new PXSetPropertyException(AR.Messages.DiscountGreaterLineMiscTotal, PXErrorLevel.Error)))
					{
						throw new PXRowPersistingException(typeof(SOOrder.curyDiscTot).Name, null, AR.Messages.DiscountGreaterLineMiscTotal);
					}
				}

				if (doc.Status == SOOrderStatus.Completed && doc.Hold == true)
				{
					throw new PXRowPersistingException(typeof(SOOrder.status).Name, null, PXMessages.LocalizeFormatNoPrefixNLA(Messages.DocumentOnHoldCannotBeCompleted, doc.OrderNbr));
				}

				if (doc != null && IsTransferOrder)
				{
					var destinationSiteIdErrorString = PXUIFieldAttribute.GetError<SOOrder.destinationSiteID>(sender, e.Row);
					if (destinationSiteIdErrorString == null)
					{
						var destinationSiteIdErrorMessage = doc.DestinationSiteIdErrorMessage;
						if (!string.IsNullOrWhiteSpace(destinationSiteIdErrorMessage))
						{
							throw new PXRowPersistingException(typeof(SOOrder.destinationSiteID).Name, sender.GetValueExt<SOOrder.destinationSiteID>(e.Row), destinationSiteIdErrorMessage);
						}
					}
				}

				if (doc.PMInstanceID != null)
				{
					CustomerPaymentMethodC pmInstance = this.DefPaymentMethodInstance.Select();
					if (pmInstance?.BAccountID != null && pmInstance?.BAccountID != doc.CustomerID)
					{
						throw new PXRowPersistingException(
							typeof(SOOrder.pMInstanceID).Name,
							doc.PMInstanceID,
							Messages.PMInstanceNotBelongToCustomer,
							pmInstance.Descr,
							sender.GetValueExt<SOOrder.customerID>(doc));
					}
					if (pmInstance?.PaymentMethodID != null && pmInstance?.PaymentMethodID != doc.PaymentMethodID)
					{
						throw new PXRowPersistingException(
							typeof(SOOrder.pMInstanceID).Name,
							doc.PMInstanceID,
							Messages.PMInstanceNotCorrespondToPaymentMethod,
							pmInstance.Descr,
							sender.GetValueExt<SOOrder.paymentMethodID>(doc));
					}
				}
			}

			if (e.Operation == PXDBOperation.Update)
			{
				if (doc.ShipmentCntr < 0 || doc.OpenShipmentCntr < 0 || doc.ShipmentCntr < doc.BilledCntr + doc.ReleasedCntr && doc.Behavior == SOBehavior.SO)
				{
					throw new PXSetPropertyException(Messages.InvalidShipmentCounters);
				}
			}

			if (IsMobile) // check control total when persisting from mobile
			{
				if (((SOOrder)e.Row).Hold == false && ((SOOrder)e.Row).Completed == false)
				{
					if (((SOOrder)e.Row).CuryOrderTotal != ((SOOrder)e.Row).CuryControlTotal)
					{
						sender.RaiseExceptionHandling<SOOrder.curyControlTotal>(e.Row, ((SOOrder)e.Row).CuryControlTotal, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else if (((SOOrder)e.Row).CuryOrderTotal < 0m && ((SOOrder)e.Row).ARDocType != ARDocType.NoUpdate && doc.Behavior != SOBehavior.RM)
					{
						if (soordertype.Current.RequireControlTotal == true)
						{
							sender.RaiseExceptionHandling<SOOrder.curyControlTotal>(e.Row, ((SOOrder)e.Row).CuryControlTotal, new PXSetPropertyException(Messages.DocumentBalanceNegative));
						}
						else
						{
							sender.RaiseExceptionHandling<SOOrder.curyOrderTotal>(e.Row, ((SOOrder)e.Row).CuryOrderTotal, new PXSetPropertyException(Messages.DocumentBalanceNegative));
						}
					}
					else
					{
						if (soordertype.Current.RequireControlTotal == true)
						{
							sender.RaiseExceptionHandling<SOOrder.curyControlTotal>(e.Row, null, null);
						}
						else
						{
							sender.RaiseExceptionHandling<SOOrder.curyOrderTotal>(e.Row, null, null);
						}
					}
				}
			}
		}

		protected virtual void SOOrder_OrderDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CurrencyInfoAttribute.SetEffectiveDate<SOOrder.orderDate>(sender, e);
		}

		protected virtual void SOOrder_BillSeparately_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOOrder.invoiceDate>(e.Row);
			sender.SetDefaultExt<SOOrder.invoiceNbr>(e.Row);
			sender.SetDefaultExt<SOOrder.pMInstanceID>(e.Row);
			sender.SetDefaultExt<SOOrder.extRefNbr>(e.Row);

			if (((SOOrder)e.Row).BillSeparately == false)
			{
				sender.SetValuePending<SOOrder.invoiceDate>(e.Row, null);
				sender.SetValuePending<SOOrder.invoiceNbr>(e.Row, null);
				sender.SetValuePending<SOOrder.pMInstanceID>(e.Row, null);
				sender.SetValuePending<SOOrder.extRefNbr>(e.Row, null);
				sender.SetValuePending<SOOrder.finPeriodID>(e.Row, null);
			}
		}

		protected virtual void SOOrder_FinPeriodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null || ((SOOrder)e.Row).BillSeparately == false || soordertype.Current == null || IsInvoiceOrder == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_InvoiceDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null || ((SOOrder)e.Row).BillSeparately == false || soordertype.Current == null || IsInvoiceOrder == false)
			{
				e.NewValue = null;
			}
			else
			{
				e.NewValue = sosetup.Current.UseShipDateForInvoiceDate == true ? sender.GetValue<SOOrder.shipDate>(e.Row) : sender.GetValue<SOOrder.orderDate>(e.Row);
			}
			e.Cancel = true;
		}

		protected virtual void SOOrder_InvoiceNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null || ((SOOrder)e.Row).BillSeparately == false || soordertype.Current == null || IsInvoiceOrder == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_Priority_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				if (location.Current != null && location.Current.COrderPriority != null)
				{
					e.NewValue = location.Current.COrderPriority ?? 0;
				}
			}
		}

		protected virtual void SOOrder_ShipComplete_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				if (location.Current != null && !string.IsNullOrEmpty(location.Current.CShipComplete))
				{
					e.NewValue = location.Current.CShipComplete;
				}
				else
				{
					e.NewValue = SOShipComplete.CancelRemainder;
				}

				if ((string)e.NewValue == SOShipComplete.BackOrderAllowed && soordertype.Current != null && soordertype.Current.RequireLocation == true)
				{
					e.NewValue = SOShipComplete.CancelRemainder;
				}
			}
		}

		protected virtual void SOOrder_PMInstanceID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null || soordertype.Current == null || IsPaymentInfoEnabled == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_PaymentMethodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null || soordertype.Current == null || IsPaymentInfoEnabled == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_CashAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null || soordertype.Current == null || IsPaymentInfoEnabled == false)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}
		protected virtual void SOOrder_CashAccountID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (e.Exception == null)
				return;

			SOOrder doc = e.Row as SOOrder;
			if (doc != null)
			{
				e.Cancel = true;
				doc.CashAccountID = null;
			}
		}
		protected virtual void SOOrder_OverrideTaxZone_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row == null) return;

			if (row.OverrideTaxZone != true)
			{
				sender.SetDefaultExt<SOOrder.taxZoneID>(e.Row);
			}
		}

		protected virtual void SOOrder_ShipVia_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				if (row.OverrideTaxZone != true && (e.OldValue == null || (e.OldValue != null && IsCommonCarrier(e.OldValue.ToString()) != IsCommonCarrier(row.ShipVia))))
					sender.SetDefaultExt<SOOrder.taxZoneID>(e.Row);

				sender.SetDefaultExt<SOOrder.freightTaxCategoryID>(e.Row);
				row.UseCustomerAccount = CanUseCustomerAccount(row);
			}
		}

		protected virtual void SOOrder_IsManualPackage_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null && row.IsManualPackage != true)
			{
				foreach (SOPackageInfoEx pack in Packages.Select())
				{
					Packages.Delete(pack);
				}
				row.PackageWeight = 0;
				sender.SetValue<SOOrder.isPackageValid>(row, false);
			}
		}

		protected virtual void SOOrder_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				foreach (SOLine tran in Transactions.Select())
				{
					tran.ProjectID = row.ProjectID;
					Transactions.Update(tran);
				}

			}
		}

		protected virtual bool CanUseCustomerAccount(SOOrder row)
		{
			Carrier carrier = Carrier.PK.Find(this, row.ShipVia);
			if (carrier != null && !string.IsNullOrEmpty(carrier.CarrierPluginID))
			{
				foreach (CarrierPluginCustomer cpc in PXSelect<CarrierPluginCustomer,
						Where<CarrierPluginCustomer.carrierPluginID, Equal<Required<CarrierPluginCustomer.carrierPluginID>>,
						And<CarrierPluginCustomer.customerID, Equal<Required<CarrierPluginCustomer.customerID>>,
						And<CarrierPluginCustomer.isActive, Equal<True>>>>>.Select(this, carrier.CarrierPluginID, row.CustomerID))
				{
					if (!string.IsNullOrEmpty(cpc.CarrierAccount) &&
						(cpc.CustomerLocationID == row.CustomerLocationID || cpc.CustomerLocationID == null)
						)
					{
						return true;
					}
				}
			}

			return false;
		}

		protected virtual bool CanUseGroundCollect(SOOrder row)
		{
			if (string.IsNullOrEmpty(row.ShipVia))
				return false;

			Carrier carrier = Carrier.PK.Find(this, row.ShipVia);
			if (carrier?.IsExternal != true || string.IsNullOrEmpty(carrier?.CarrierPluginID))
				return false;

			return CarrierPluginMaint.GetCarrierPluginAttributes(this, carrier.CarrierPluginID).Contains("COLLECT");
		}

		protected virtual void SOOrder_UseCustomerAccount_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null)
			{
				bool canBeTrue = CanUseCustomerAccount(row);

				if (e.NewValue != null && ((bool)e.NewValue) && !canBeTrue)
				{
					e.NewValue = false;
					throw new PXSetPropertyException(Messages.CustomeCarrierAccountIsNotSetup);
				}
			}
		}


		protected virtual void SOOrder_ShipDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetDefaultExt<SOOrder.invoiceDate>(e.Row);

			DateTime? oldDate = (DateTime?)e.OldValue;
			if (oldDate != ((SOOrder)e.Row).ShipDate)
			{
				if ((SOLine)Transactions.Select() != null && (Document.View.Answer == WebDialogResult.None && !this.IsMobile) && ((SOOrder)e.Row).ShipComplete == SOShipComplete.BackOrderAllowed)
					Document.Ask(GL.Messages.Confirmation, Messages.ConfirmShipDateRecalc, MessageButtons.YesNo);
			}
		}

		protected virtual void SOOrder_ExtRefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null || ((SOOrder)e.Row).BillSeparately == false || soordertype.Current == null || IsInvoiceOrder == false || soordertype.Current.ARDocType != ARDocType.CashSale && soordertype.Current.ARDocType != ARDocType.CashReturn)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_CCCardNumber_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = (SOOrder)e.Row;
			string ccCardNumber = row.CCCardNumber;
			if (string.IsNullOrEmpty(ccCardNumber) == false)
			{
				AR.CustomerPaymentMethod existingCPM = null;
				CustomerPaymentMethodDetail existingCpmIDdetail = null;

				foreach (PXResult<AR.CustomerPaymentMethod, PaymentMethod, PaymentMethodDetail, CustomerPaymentMethodDetail> it in PXSelectReadonly2<AR.CustomerPaymentMethod,
										LeftJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<AR.CustomerPaymentMethod.paymentMethodID>,
											And<PaymentMethod.isActive, Equal<True>,
											And<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>>>>,
										LeftJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<AR.CustomerPaymentMethod.paymentMethodID>,
											And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
											And<PaymentMethodDetail.isIdentifier, Equal<True>>>>,
										LeftJoin<CustomerPaymentMethodDetail, On<CustomerPaymentMethodDetail.pMInstanceID, Equal<AR.CustomerPaymentMethod.pMInstanceID>,
											And<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>>>>>>,
							Where<AR.CustomerPaymentMethod.bAccountID, Equal<Required<AR.CustomerPaymentMethod.bAccountID>>,
										And<AR.CustomerPaymentMethod.isActive, Equal<True>
										//,And<CustomerPaymentMethod.descr, Equal<Required<CustomerPaymentMethod.descr>>>
										>>>.Select(this, row.CustomerID, ccCardNumber))
				{
					AR.CustomerPaymentMethod cpm = (AR.CustomerPaymentMethod)it;
					CustomerPaymentMethodDetail cpmDetail = (CustomerPaymentMethodDetail)it;
					//PaymentMethod def = (PaymentMethod) def;
					//string id = CustomerPaymentMethodMaint.IDObfuscator.MaskID(row.Value, def.EntryMask, def.DisplayMask);
					if (cpmDetail.PMInstanceID.HasValue && cpmDetail.Value == ccCardNumber
							&& (cpm.ExpirationDate.HasValue == false || cpm.ExpirationDate.Value > DateTime.Now))
					{
						existingCPM = cpm;
						existingCpmIDdetail = cpmDetail;
						break;
					}
				}
				if (existingCPM != null && existingCpmIDdetail != null)
				{
					sender.SetValueExt<SOOrder.pMInstanceID>(e.Row, existingCPM.PMInstanceID);
					row.CCCardNumber = string.Empty;
					//SOOrder copy =(SOOrder)this.Document.Cache.CreateCopy(doc);
					//row.Value = existingCpmIDdetail.Value;
					//copy.CreatePMInstance= false;
					//doc = this.Document.Update(copy);
					//SOOrder copy1 = (SOOrder)this.Document.Cache.CreateCopy(doc);
					//copy1.PMInstanceID = existingCPM.PMInstanceID;
					//doc = this.Document.Update(copy1);
					//isIDChanged = true;
				}
				else
				{
					sender.SetValueExt<SOOrder.createPMInstance>(e.Row, true);
				}
			}
		}

		public void SOOrder_PaymentProfileID_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			if (this.IsContractBasedAPI)
			{
				CustomerPaymentMethodDetail PaymentProfileID = PXSelectJoin<
				   CustomerPaymentMethodDetail,
				   InnerJoin<AR.CustomerPaymentMethod,
					   On<CustomerPaymentMethodDetail.pMInstanceID, Equal<AR.CustomerPaymentMethod.pMInstanceID>>,
				   InnerJoin<PaymentMethodDetail,
					   On<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
					   And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>>>>>,
				   Where<AR.CustomerPaymentMethod.bAccountID, Equal<Current2<SOOrder.customerID>>,
					   And<PaymentMethodDetail.isCCProcessingID, Equal<True>,
					   And<AR.CustomerPaymentMethod.paymentMethodID, Equal<Current2<SOOrder.paymentMethodID>>,
					   And<AR.CustomerPaymentMethod.pMInstanceID, Equal<Current<SOOrder.pMInstanceID>>>>>>>.Select(this);
				e.ReturnValue = PaymentProfileID?.Value;
			}
		}

		public void SOOrder_PaymentProfileID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			SOOrder order = (SOOrder)e.Row;
			if (order != null)
			{
				var paymentProfileID = order.PaymentProfileID;
				if (paymentProfileID != null)
				{
					string encryptedPaymentProfileID = PXRSACryptStringWithMaskAttribute.Encrypt(paymentProfileID);
					if (encryptedPaymentProfileID == paymentProfileID)
					{
						encryptedPaymentProfileID = Convert.ToBase64String(Encoding.Unicode.GetBytes(paymentProfileID));
					}
					CustomerPaymentMethodDetail customerPaymentMethod = PXSelectJoin<
					CustomerPaymentMethodDetail,
					InnerJoin<AR.CustomerPaymentMethod,
						On<CustomerPaymentMethodDetail.pMInstanceID, Equal<AR.CustomerPaymentMethod.pMInstanceID>>,
					InnerJoin<PaymentMethodDetail,
						On<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>,
						And<PaymentMethodDetail.detailID, Equal<CustomerPaymentMethodDetail.detailID>>>>>,
					Where<AR.CustomerPaymentMethod.bAccountID, Equal<Required<SOOrder.customerID>>,
						And<PaymentMethodDetail.isCCProcessingID, Equal<True>,
						And<AR.CustomerPaymentMethod.paymentMethodID, Equal<Required<SOOrder.paymentMethodID>>,
						And<CustomerPaymentMethodDetail.value, Equal<Required<SOOrder.paymentProfileID>>>>>>>.Select(cache.Graph, order.CustomerID, order.PaymentMethodID, encryptedPaymentProfileID);
					if (customerPaymentMethod?.PMInstanceID == null)
					{
						throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ValueDoesntExistOrNoRights, "PaymentProfileID", paymentProfileID));
					}
					order.PMInstanceID = customerPaymentMethod.PMInstanceID;
				}
			}
		}

		protected bool IsCreditMemoOrder
		{
			get
			{
				return ((soordertype.Current.ARDocType == ARDocType.CreditMemo || soordertype.Current.ARDocType == ARDocType.CashReturn) && (sooperation.Current.INDocType == INTranType.CreditMemo || sooperation.Current.INDocType == INTranType.Return || sooperation.Current.INDocType == INTranType.NoUpdate));
			}
		}

		protected bool IsRMAOrder
		{
			get
			{
				return (soordertype.Current.Behavior == SOOrderTypeConstants.RMAOrder);
			}
		}

		public bool IsTransferOrder
		{
			get
			{
				return (sooperation.Current.INDocType == INTranType.Transfer);
			}
		}

		protected bool IsDebitMemoOrder
		{
			get
			{
				return (soordertype.Current.ARDocType == ARDocType.DebitMemo && sooperation.Current.INDocType == INTranType.DebitMemo);
			}
		}

		protected bool IsInvoiceOrder
		{
			get
			{
				return (soordertype.Current.RequireShipping == false && soordertype.Current.ARDocType != ARDocType.NoUpdate);
			}
		}

		protected bool IsNoAROrder
		{
			get
			{
				return (soordertype.Current.ARDocType == ARDocType.NoUpdate);
			}
		}
		
		protected bool IsPaymentInfoEnabled
		{
			get
			{
				return (soordertype.Current.ARDocType == ARDocType.CashSale
					|| soordertype.Current.ARDocType == ARDocType.CashReturn
					|| soordertype.Current.ARDocType == ARDocType.Invoice);
			}
		}

		protected bool IsCashSale
		{
			get
			{
				return (soordertype.Current.ARDocType == ARDocType.CashSale
					|| soordertype.Current.ARDocType == ARDocType.CashReturn);
			}
		}

		protected bool IsUserInvoiceNumbering
		{
			get
			{
				return (soordertype.Current.UserInvoiceNumbering == true);
			}
		}

		public bool IsCopyOrder
		{
			get;
			protected set;
		}

		/// <summary>
		/// The flag indicates that group and document discounts are disabled.
		/// </summary>
		protected virtual bool DisableGroupDocDiscount
		{
			get
			{
				return (IsRMAOrder || IsTransferOrder) && soordertype.Current.ARDocType == ARDocType.NoUpdate;
			}
		}

		public virtual void SOOrder_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			SOOrder order = (SOOrder)e.Row;
			if (order != null && !e.IsReadOnly)
			{
				RecalcTotals(sender, order);
			}
		}

		private void RecalcTotals(PXCache sender, SOOrder order)
		{
			using (new PXConnectionScope())
			{
				bool IsReadOnly = sender.GetStatus(order) == PXEntryStatus.Notchanged;

				CurrencyInfo inv_info = null;

				decimal? CuryApplAmt = 0m;

				PXView view = IsReadOnly ? this.TypedViews.GetView(Adjustments_Raw.View.BqlSelect, true) : Adjustments_Raw.View;
				foreach (PXResult<SOAdjust, ARRegisterAlias, ARPayment, CurrencyInfo> res in view.SelectMultiBound(new object[] { order }))
				{
					if (inv_info == null)
					{
						SOOrder cached = order;
						if (order.CuryInfoID == null)
						{
							cached = (SOOrder)sender.Locate(order);
						}
						inv_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<SOOrder.curyInfoID>>>>.SelectSingleBound(this, new object[] { cached });
					}
					ARPayment payment = (ARPayment)res;
					SOAdjust adj = (SOAdjust)res;
					CurrencyInfo pay_info = (CurrencyInfo)res;

					if (payment != null)
						PXCache<ARRegister>.RestoreCopy(payment, (ARRegisterAlias)res);

					decimal CuryDocBal;
					if (string.Equals(pay_info.CuryID, inv_info.CuryID))
					{
						CuryDocBal = (decimal)payment.CuryDocBal;
					}
					else
					{
						decimal docBal = ((payment.Released == true) ? payment.DocBal : payment.OrigDocAmt) ?? 0m;

						PXDBCurrencyAttribute.CuryConvCury(Adjustments.Cache, inv_info, docBal, out CuryDocBal);
					}

					if (adj != null)
					{
						if (adj.CuryAdjdAmt > CuryDocBal)
						{
							//if reconsidered need to calc RGOL
							adj.CuryDocBal = CuryDocBal;
							adj.CuryAdjdAmt = 0m;
						}

						CuryApplAmt += adj.CuryAdjdAmt;
					}
				}

				sender.SetValue<SOOrder.curyPaymentTotal>(order, CuryApplAmt);
				sender.RaiseFieldUpdated<SOOrder.curyPaymentTotal>(order, null);
			}
		}

		protected SOOrder _LastSelected;
		protected string orderType;
		protected virtual void SOOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			SOOrder doc = e.Row as SOOrder;

			if (doc == null)
			{
				return;
			}

			orderType = doc.OrderType;

			bool operationVisible = (soordertype.Current.Behavior == SOOrderTypeConstants.RMAOrder || soordertype.Current.Behavior == SOOrderTypeConstants.Invoice);
			if (operationVisible)
			{
				SOOrderTypeOperation nonDefault = PXSelectReadonly<SOOrderTypeOperation,
					Where<SOOrderTypeOperation.active, Equal<True>,
						And<SOOrderTypeOperation.orderType, Equal<Required<SOOrderTypeOperation.orderType>>,
							And<SOOrderTypeOperation.operation, NotEqual<Required<SOOrderTypeOperation.operation>>>>>>
					.SelectWindowed(this, 0, 1, soordertype.Current.OrderType, soordertype.Current.DefaultOperation);
				if (nonDefault == null)
					operationVisible = false;
			}
			if (!object.ReferenceEquals(doc, _LastSelected))
			{
				PXUIFieldAttribute.SetVisible<SOLine.operation>(this.Transactions.Cache, null, operationVisible);
				PXUIFieldAttribute.SetEnabled<SOLine.operation>(this.Transactions.Cache, null, operationVisible);
				PXUIFieldAttribute.SetVisible<SOPackageInfo.operation>(this.Packages.Cache, null, operationVisible);
				PXUIFieldAttribute.SetVisible<SOLine.autoCreateIssueLine>(this.Transactions.Cache, null, operationVisible);
				PXUIFieldAttribute.SetVisible<SOLine.curyUnitCost>(this.Transactions.Cache, null, IsRMAOrder || IsCreditMemoOrder);
				PXUIFieldAttribute.SetEnabled<SOLine.curyUnitCost>(this.Transactions.Cache, null, IsRMAOrder || IsCreditMemoOrder);
				_LastSelected = doc;
			}
			PXUIFieldAttribute.SetVisible<SOOrder.curyID>(cache, doc,
				PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() && orderType != SOOrderTypeConstants.TransferOrder);

			PXUIFieldAttribute.SetVisible<SOLine.pOCreate>(Transactions.Cache, null, soordertype.Current.RequireShipping ?? false);
			PXUIFieldAttribute.SetVisible<SOLineSplit.pOCreate>(splits.Cache, null, soordertype.Current.RequireShipping ?? false);
			PXUIFieldAttribute.SetVisible<SOLine.pOSource>(Transactions.Cache, null, soordertype.Current.RequireShipping ?? false);
			PXUIFieldAttribute.SetVisible<SOLineSplit.pOSource>(splits.Cache, null, soordertype.Current.RequireShipping ?? false);

			ValidateControlTotal(cache, e);

			bool curyenabled = true;

			if (customer.Current != null && customer.Current.AllowOverrideCury == false)
			{
				curyenabled = false;
			}
			bool isPMCreditCard = false;
			bool isFreightEditable = true;
			bool isTokenized = CCProcessingHelper.IsTokenizedPaymentMethod(this, doc.PMInstanceID);
			bool isHFPM = CCProcessingHelper.IsHFPaymentMethod(this, doc.PMInstanceID);

			PXUIFieldAttribute.SetVisible<CustomerPaymentMethodInputMode.inputMode>(InputModeFilter.Cache, null, doc.CreatePMInstance == true && isTokenized && !isHFPM);

			this.prepareInvoice.SetEnabled(doc.Hold == false && doc.Cancelled == false &&
				soordertype.Current.ARDocType != ARDocType.NoUpdate &&
				(doc.ShipmentCntr - doc.OpenShipmentCntr - doc.BilledCntr - doc.ReleasedCntr) > 0 ||
				(doc.OrderQty == 0 && (doc.CuryUnbilledMiscTot > 0 || doc.UnbilledOrderQty > 0)) ||
				(doc.Status == SOOrderStatus.Open && soordertype.Current.RequireShipping == false));

			PXUIFieldAttribute.SetVisible<SOOrder.refTranExtNbr>(cache, doc, doc.ARDocType == ARDocType.CashReturn);

			bool allowAllocation = AllowAllocation();
			if (doc == null || doc.Completed == true || doc.Cancelled == true || !allowAllocation)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				cache.AllowDelete = false;
				cache.AllowUpdate = allowAllocation;
				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = false;
				Transactions.Cache.AllowInsert = false;

				this.Caches.SubscribeCacheCreated(Adjustments.GetItemType(), delegate
				{
					Adjustments.Cache.AllowInsert = false;
					Adjustments.Cache.AllowUpdate = false;
					Adjustments.Cache.AllowDelete = false;
				});


				DiscountDetails.Cache.AllowDelete = false;
				DiscountDetails.Cache.AllowUpdate = false;
				DiscountDetails.Cache.AllowInsert = false;

				Taxes.Cache.AllowUpdate = false;
			}
			else
			{
				CustomerPaymentMethodC cpmRow = null;
				if (doc.PMInstanceID.HasValue)
				{
					cpmRow = this.DefPaymentMethodInstance.Select();
					bool isCCInserted = this.DefPaymentMethodInstance.Cache.GetStatus(cpmRow) == PXEntryStatus.Inserted;
					doc.CreatePMInstance = isCCInserted;

					#region Credit Card Processing

					if (cpmRow != null && !String.IsNullOrEmpty(cpmRow.PaymentMethodID))
					{
						PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, cpmRow.PaymentMethodID);
						if (pm != null && pm.PaymentType == CA.PaymentMethodType.CreditCard)
						{
							isPMCreditCard = true;
						}
					}

					#endregion
				}

				ExternalTransactionState state = new ExternalTransactionState();
				doc.PCResponseReasonText = string.Empty;
				if (doc.CreatePMInstance == false && doc.PMInstanceID.HasValue && isPMCreditCard)
				{
					state = ExternalTranHelper.GetActiveTransactionState(this, ExternalTran);
				}

				bool isCashReturn = doc.ARDocType == ARDocType.CashReturn;
				PXUIFieldAttribute.SetEnabled(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<SOOrder.refTranExtNbr>(cache, doc, (doc.CreatePMInstance == true || doc.PMInstanceID.HasValue) && isPMCreditCard && isCashReturn && !state.IsRefunded);
				PXUIFieldAttribute.SetEnabled<SOOrder.status>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.orderQty>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.orderWeight>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.orderVolume>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.packageWeight>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyOrderTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyUnpaidBalance>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyLineTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyMiscTot>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyFreightCost>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.freightCostIsValid>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyFreightAmt>(cache, doc, doc.OverrideFreightAmount == true);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyTaxTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.openOrderQty>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyOpenOrderTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyOpenLineTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyOpenTaxTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.unbilledOrderQty>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyUnbilledOrderTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyUnbilledLineTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyUnbilledTaxTotal>(cache, doc, false);

				PXUIFieldAttribute.SetEnabled<SOOrder.curyPaymentTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyID>(cache, doc, curyenabled);
			
				PXUIFieldAttribute.SetEnabled<SOOrder.cCPaymentStateDescr>(cache, doc, false);
				
				PXUIFieldAttribute.SetEnabled<SOOrder.pCResponseReasonText>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.captureTranNumber>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyCCCapturedAmt>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.origOrderType>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.origOrderNbr>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyVatExemptTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyVatTaxableTotal>(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.overrideFreightAmount>(cache, doc, AllowChangingOverrideFreightAmount(doc));
				PXUIFieldAttribute.SetEnabled<SOOrder.freightAmountSource>(cache, doc, false);

				if (soordertype.Current != null)
				{
					bool isInvoiceInfoEnabled = IsInvoiceOrder && doc.BillSeparately == true;
					bool hasActiveCCTran = state.IsCaptured || state.IsPreAuthorized;
					bool enableCreateCC = (IsPaymentInfoEnabled && doc.CustomerID.HasValue && !hasActiveCCTran);
					bool isPMInstanceRequired = false;
					bool hasPaymentMethod = !String.IsNullOrEmpty(doc.PaymentMethodID);
					if (IsPaymentInfoEnabled && hasPaymentMethod)
					{
						PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, doc.PaymentMethodID);
						if (pm == null)
						{
							cache.RaiseExceptionHandling<SOOrder.paymentMethodID>(doc, doc.PaymentMethodID,
								new PXSetPropertyException(Messages.PaymentMethodNotFound, PXErrorLevel.Error));
						}
						else
						{
							cache.RaiseExceptionHandling<SOOrder.paymentMethodID>(doc, doc.PaymentMethodID, null);
							isPMInstanceRequired = pm.IsAccountNumberRequired == true;
							isPMCreditCard = pm.PaymentType == PaymentMethodType.CreditCard;
						}

					}
                    PXUIFieldAttribute.SetEnabled<SOOrder.billSeparately>(cache, doc, !isPMCreditCard && !IsNoAROrder);
					PXUIFieldAttribute.SetEnabled<SOOrder.createPMInstance>(cache, doc, enableCreateCC && isPMInstanceRequired);
					PXUIFieldAttribute.SetEnabled<SOOrder.invoiceDate>(cache, doc, isInvoiceInfoEnabled);
					PXUIFieldAttribute.SetRequired<SOOrder.invoiceDate>(cache, (isInvoiceInfoEnabled && soordertype.Current.Behavior == SOOrderTypeConstants.Invoice) || (doc.ARDocType == ARDocType.CreditMemo && doc.BillSeparately == true));
					PXUIFieldAttribute.SetEnabled<SOOrder.invoiceNbr>(cache, doc, isInvoiceInfoEnabled);
					PXUIFieldAttribute.SetEnabled<SOOrder.finPeriodID>(cache, doc, isInvoiceInfoEnabled);
					PXUIFieldAttribute.SetRequired<SOOrder.finPeriodID>(cache, (isInvoiceInfoEnabled && soordertype.Current.Behavior == SOOrderTypeConstants.Invoice) || (doc.ARDocType == ARDocType.CreditMemo && doc.BillSeparately == true));

					bool enableCCSelection = (IsPaymentInfoEnabled && doc.CustomerID.HasValue & doc.CreatePMInstance != true && !hasActiveCCTran);
					PXUIFieldAttribute.SetEnabled<SOOrder.paymentMethodID>(cache, doc, enableCCSelection);
					PXUIFieldAttribute.SetRequired<SOOrder.paymentMethodID>(cache, doc.BillSeparately == true
						&& (doc.ARDocType == ARDocType.CashSale || doc.ARDocType == ARDocType.CashReturn));
					PXUIFieldAttribute.SetEnabled<SOOrder.pMInstanceID>(cache, doc, enableCCSelection && isPMInstanceRequired);
					PXUIFieldAttribute.SetRequired<SOOrder.pMInstanceID>(cache, enableCCSelection && isPMInstanceRequired);
					PXUIFieldAttribute.SetEnabled<SOOrder.cCCardNumber>(cache, doc, enableCCSelection && enableCreateCC && !doc.PMInstanceID.HasValue
						&& isPMInstanceRequired && !isTokenized);
					PXUIFieldAttribute.SetEnabled<SOOrder.cashAccountID>(cache, doc, IsPaymentInfoEnabled && hasPaymentMethod);
					PXUIFieldAttribute.SetRequired<SOOrder.cashAccountID>(cache, IsPaymentInfoEnabled && hasPaymentMethod && IsInvoiceOrder
						&& doc.BillSeparately == true && (doc.ARDocType == ARDocType.CashSale || doc.ARDocType == ARDocType.CashReturn));

					PXUIFieldAttribute.SetEnabled<SOOrder.extRefNbr>(cache, doc, IsCashSale);
					PXUIFieldAttribute.SetRequired<SOOrder.extRefNbr>(cache, (IsCashSale && IsInvoiceOrder && doc.BillSeparately == true) && !isPMCreditCard && arsetup.Current.RequireExtRef == true);

					if (isInvoiceInfoEnabled && doc.InvoiceDate != null)
					{
						OpenPeriodAttribute.SetValidatePeriod<SOOrder.finPeriodID>(cache, doc, PeriodValidation.DefaultSelectUpdate);
					}
					else
					{
						OpenPeriodAttribute.SetValidatePeriod<SOOrder.finPeriodID>(cache, doc, PeriodValidation.Nothing);
					}
					PXUIFieldAttribute.SetEnabled<SOOrder.dueDate>(cache, doc, isInvoiceInfoEnabled && soordertype.Current.ARDocType != ARDocType.CreditMemo);
					PXUIFieldAttribute.SetRequired<SOOrder.dueDate>(cache, doc.BillSeparately == true
						&& (soordertype.Current.ARDocType == ARDocType.CashSale || soordertype.Current.ARDocType == ARDocType.CashReturn || soordertype.Current.ARDocType == ARDocType.Invoice));
					PXUIFieldAttribute.SetEnabled<SOOrder.discDate>(cache, doc, isInvoiceInfoEnabled && soordertype.Current.ARDocType != ARDocType.CreditMemo);
					PXUIFieldAttribute.SetRequired<SOOrder.discDate>(cache, doc.BillSeparately == true
						&& (soordertype.Current.ARDocType == ARDocType.CashSale || soordertype.Current.ARDocType == ARDocType.CashReturn || soordertype.Current.ARDocType == ARDocType.Invoice));

					bool isInserted = cache.GetStatus(doc) == PXEntryStatus.Inserted;
				}
				cache.AllowUpdate = true;
				bool isAuthorizedCashSale = (doc.ARDocType == ARDocType.CashSale && (doc.IsCCAuthorized == true || doc.IsCCCaptured == true));
				bool isRefundedCashReturn = isCashReturn && state.IsRefunded;
				Transactions.Cache.AllowDelete = !isAuthorizedCashSale && !isRefundedCashReturn;
				Transactions.Cache.AllowUpdate = !isAuthorizedCashSale && !isRefundedCashReturn;
				Transactions.Cache.AllowInsert = (doc.CustomerID != null && doc.CustomerLocationID != null && (doc.ProjectID != null || !PM.ProjectAttribute.IsPMVisible(BatchModule.SO)))
					&& !isAuthorizedCashSale && !isRefundedCashReturn;
				PXUIFieldAttribute.SetEnabled<SOOrder.curyDiscTot>(cache, doc, !PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() && !isAuthorizedCashSale && !isRefundedCashReturn);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyPremiumFreightAmt>(cache, doc, !isAuthorizedCashSale && !isRefundedCashReturn);
				isFreightEditable = !isAuthorizedCashSale && !isRefundedCashReturn;

				Taxes.Cache.AllowUpdate = true;

				bool PaymentsAndApplicationsEnabled = soordertype.Current.CanHaveApplications;
				createPayment.SetEnabled(PaymentsAndApplicationsEnabled && cache.GetStatus(e.Row) != PXEntryStatus.Inserted);
				createPrepayment.SetEnabled(PaymentsAndApplicationsEnabled && cache.GetStatus(e.Row) != PXEntryStatus.Inserted);
				Adjustments.Cache.AllowInsert = PaymentsAndApplicationsEnabled;
				Adjustments.Cache.AllowDelete = PaymentsAndApplicationsEnabled;
				Adjustments.Cache.AllowUpdate = PaymentsAndApplicationsEnabled;

				DiscountDetails.Cache.AllowDelete = true;
				DiscountDetails.Cache.AllowUpdate = !DisableGroupDocDiscount;
				DiscountDetails.Cache.AllowInsert = !DisableGroupDocDiscount;
			}
			splits.Cache.AllowInsert = Transactions.Cache.AllowInsert;
			splits.Cache.AllowUpdate = Transactions.Cache.AllowUpdate;
			splits.Cache.AllowDelete = Transactions.Cache.AllowDelete;

			PXUIFieldAttribute.SetEnabled<SOOrder.orderType>(cache, doc);
			PXUIFieldAttribute.SetEnabled<SOOrder.orderNbr>(cache, doc);
			PXUIFieldAttribute.SetVisible<SOLine.invoiceType>(Transactions.Cache, null, IsCreditMemoOrder || IsRMAOrder);
			PXUIFieldAttribute.SetVisible<SOLine.invoiceNbr>(Transactions.Cache, null, IsCreditMemoOrder || IsRMAOrder);
			PXUIFieldAttribute.SetEnabled<SOLine.reasonCode>(Transactions.Cache, null, true);
			addInvoice.SetEnabled((IsCreditMemoOrder || IsRMAOrder) && Transactions.Cache.AllowInsert);

			Taxes.Cache.AllowDelete = Transactions.Cache.AllowDelete;
			Taxes.Cache.AllowInsert = Transactions.Cache.AllowInsert;

			PXNoteAttribute.SetTextFilesActivitiesRequired<SOLine.noteID>(Transactions.Cache, null);
			PXUIFieldAttribute.SetVisible<SOLine.branchID>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetEnabled<SOLine.branchID>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.curyLineAmt>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.curyUnitPrice>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.curyExtCost>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.curyExtPrice>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.discPct>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.discAmt>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.curyDiscAmt>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.curyDiscPrice>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.manualDisc>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.discountID>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetEnabled<SOLine.discountID>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.curyUnbilledAmt>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.salesPersonID>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.taxCategoryID>(this.Transactions.Cache, null, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOLine.commissionable>(this.Transactions.Cache, null, !IsTransferOrder);

			if (soordertype.Current != null)
			{
				PXUIFieldAttribute.SetVisible<SOOrder.curyControlTotal>(cache, e.Row, soordertype.Current.RequireControlTotal == true);
			}

			if (soordertype.Current.RequireLocation == true)
			{
				PXStringListAttribute.SetList<SOLine.shipComplete>(Transactions.Cache, null, new string[] { SOShipComplete.CancelRemainder, SOShipComplete.ShipComplete }, new string[] { Messages.CancelRemainder, Messages.ShipComplete });
			}
			else
			{
				PXStringListAttribute.SetList<SOLine.shipComplete>(Transactions.Cache, null, new string[] { SOShipComplete.BackOrderAllowed, SOShipComplete.CancelRemainder, SOShipComplete.ShipComplete }, new string[] { Messages.BackOrderAllowed, Messages.CancelRemainder, Messages.ShipComplete });
			}
			PXUIFieldAttribute.SetVisible<SOOrder.destinationSiteID>(cache, e.Row, IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.customerOrderNbr>(cache, e.Row, !IsTransferOrder);

			Packages.Cache.AllowInsert = ((SOOrder)e.Row).IsManualPackage == true;
			Packages.Cache.AllowDelete = ((SOOrder)e.Row).IsManualPackage == true;
			PXUIFieldAttribute.SetEnabled<SOPackageInfo.inventoryID>(Packages.Cache, null, ((SOOrder)e.Row).IsManualPackage == true);
			PXUIFieldAttribute.SetEnabled<SOPackageInfo.boxID>(Packages.Cache, null, ((SOOrder)e.Row).IsManualPackage == true);
			PXUIFieldAttribute.SetEnabled<SOPackageInfo.declaredValue>(Packages.Cache, null, ((SOOrder)e.Row).IsManualPackage == true);

			if (!string.IsNullOrEmpty(((SOOrder)e.Row).ShipVia))
			{
				Carrier shipVia = Carrier.PK.Find(this, ((SOOrder)e.Row).ShipVia);
				if (shipVia != null)
				{
					PXUIFieldAttribute.SetVisible<SOPackageInfo.declaredValue>(Packages.Cache, null, shipVia.PluginMethod != null);
					PXUIFieldAttribute.SetVisible<SOPackageInfo.cOD>(Packages.Cache, null, shipVia.PluginMethod != null);
					PXUIFieldAttribute.SetEnabled<SOOrder.curyFreightCost>(cache, doc, isFreightEditable && shipVia.CalcMethod == CarrierCalcMethod.Manual);
				}
			}
			cache.RaiseExceptionHandling<SOOrder.shipVia>(doc, doc.ShipVia, this.BuildShipViaException(doc));
			PXUIFieldAttribute.SetEnabled<SOOrder.taxZoneID>(cache, e.Row, ((SOOrder)e.Row).OverrideTaxZone == true);

			if (!UnattendedMode)
			{
				SOShippingAddress shipAddress = this.Shipping_Address.Select();
				SOBillingAddress billingAddress = this.Billing_Address.Select();
				bool enableAddressValidation = (doc.Completed == false && doc.Cancelled == false) && ((shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
												|| (billingAddress != null && billingAddress.IsDefaultAddress == false && billingAddress.IsValidated == false));
				this.validateAddresses.SetEnabled(enableAddressValidation);
			}

			PXUIFieldAttribute.SetVisible<SOOrder.groundCollect>(cache, doc, this.CanUseGroundCollect(doc));

			PXUIFieldAttribute.SetVisible<SOOrder.customerID>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.customerLocationID>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyVatExemptTotal>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyVatTaxableTotal>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyTaxTotal>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyOrderTotal>(cache, doc, !IsTransferOrder);

			PXUIFieldAttribute.SetVisible<SOOrder.taxZoneID>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.overrideTaxZone>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.avalaraCustomerUsageType>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.billSeparately>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.invoiceNbr>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.invoiceDate>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.termsID>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.dueDate>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.discDate>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.finPeriodID>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.salesPersonID>(cache, doc, !IsTransferOrder);

			PXUIFieldAttribute.SetVisible<SOOrder.curyLineTotal>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyMiscTot>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyDiscTot>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyFreightCost>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.freightCostIsValid>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.overrideFreightAmount>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyFreightAmt>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyPremiumFreightAmt>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.freightTaxCategoryID>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyOpenOrderTotal>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.unbilledOrderQty>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyUnbilledOrderTotal>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyPaymentTotal>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyCCPreAuthAmount>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyUnpaidBalance>(cache, doc, !IsTransferOrder);
			this.calculateFreight.SetVisible(!IsTransferOrder);

			Taxes.View.AllowSelect = !IsTransferOrder;
			SalesPerTran.View.AllowSelect = !IsTransferOrder;
			DiscountDetails.View.AllowSelect = !IsTransferOrder;
			Adjustments.View.AllowSelect = !IsTransferOrder;


			PXUIFieldAttribute.SetVisible<SOOrder.paymentMethodID>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.pMInstanceID>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.cashAccountID>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.extRefNbr>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.cCCardNumber>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.createPMInstance>(cache, doc, !IsTransferOrder);
			DefPaymentMethodInstance.View.AllowSelect = !IsTransferOrder;
			InputModeFilter.View.AllowSelect = !IsTransferOrder;
			DefPaymentMethodInstanceDetails.View.AllowSelect = !IsTransferOrder;
			PXUIFieldAttribute.SetVisible<SOOrder.cCPaymentStateDescr>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.pCResponseReasonText>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.preAuthTranNumber>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.cCAuthExpirationDate>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyCCPreAuthAmount>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyPaymentTotal>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyUnpaidBalance>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyCCCapturedAmt>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.captureTranNumber>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.refTranExtNbr>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.curyCCCapturedAmt>(cache, doc, !IsTransferOrder);
			PXUIFieldAttribute.SetVisible<SOOrder.captureTranNumber>(cache, doc, !IsTransferOrder);

			PXUIFieldAttribute.SetRequired<SOOrder.termsID>(cache, (doc.ARDocType != ARDocType.Undefined
				&& doc.ARDocType != ARDocType.CreditMemo));

			Billing_Contact.View.AllowSelect = !IsTransferOrder;
			Billing_Address.View.AllowSelect = !IsTransferOrder;

			PXUIFieldAttribute.SetVisible<SOOrder.approved>(cache, doc, !(bool)doc.DontApprove);
			Approval.AllowSelect = !(bool)doc.DontApprove;
			if (doc.Hold != true && doc.DontApprove != true)
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				PXUIFieldAttribute.SetEnabled<SOOrder.orderType>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<SOOrder.orderNbr>(cache, doc, true);
				PXUIFieldAttribute.SetEnabled<SOOrder.hold>(cache, doc, true);

				Transactions.Cache.AllowDelete = false;
				Transactions.Cache.AllowUpdate = false;
				Transactions.Cache.AllowInsert = false;

				splits.Cache.AllowDelete = false;
				splits.Cache.AllowUpdate = false;
				splits.Cache.AllowInsert = false;

				Adjustments.Cache.AllowInsert = false;
				Adjustments.Cache.AllowUpdate = false;
				Adjustments.Cache.AllowDelete = false;

				DiscountDetails.Cache.AllowDelete = false;
				DiscountDetails.Cache.AllowUpdate = false;
				DiscountDetails.Cache.AllowInsert = false;

				Taxes.Cache.AllowInsert = false;
				Taxes.Cache.AllowUpdate = false;
				Taxes.Cache.AllowDelete = false;

				bool PaymentsAndApplicationsEnabled = soordertype.Current.CanHaveApplications;
				createPayment.SetEnabled(!(doc == null || doc.Completed == true || doc.Cancelled == true || !allowAllocation) && PaymentsAndApplicationsEnabled && (doc.Approved ?? false));
				createPrepayment.SetEnabled(!(doc == null || doc.Completed == true || doc.Cancelled == true || !allowAllocation) && PaymentsAndApplicationsEnabled && (doc.Approved ?? false));
			}

			addInvBySite.SetEnabled(Transactions.Cache.AllowInsert);

			if (soordertype.Current == null || soordertype.Current.Active != true)
			{
				SetReadOnly(true);
				cache.AllowInsert = true;
				cache.RaiseExceptionHandling<SOOrder.orderType>(doc, doc.OrderType, new PXSetPropertyException(Messages.OrderTypeInactive, PXErrorLevel.Warning));
			}

			if (!PXPreserveScope.IsScoped() && PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.InProcess)
			{
				SetReadOnly(true);
			}

			if (doc != null && IsTransferOrder)
			{
				var destinationSiteIdErrorString = PXUIFieldAttribute.GetError<SOOrder.destinationSiteID>(cache, e.Row);
				if (destinationSiteIdErrorString == null)
				{
					var destinationSiteIdErrorMessage = doc.DestinationSiteIdErrorMessage;
					if (!string.IsNullOrWhiteSpace(destinationSiteIdErrorMessage))
					{
						if (UnattendedMode)
							throw new PXSetPropertyException(destinationSiteIdErrorMessage, PXErrorLevel.Error);
						else
							cache.RaiseExceptionHandling<POOrder.siteID>(e.Row, cache.GetValueExt<POOrder.siteID>(e.Row), new PXSetPropertyException(destinationSiteIdErrorMessage, PXErrorLevel.Error));
					}
				}
			}
			PXUIFieldAttribute.SetVisible<SOOrder.emailed>(cache, doc, !new[] { SOBehavior.RM, SOBehavior.CM }.Contains(doc.Behavior));
		}

		protected virtual PXException BuildShipViaException(SOOrder order)
		{
			if (string.IsNullOrEmpty(order.ShipVia))
				return null;

			var shipVia = Carrier.PK.Find(this, order.ShipVia);
			if (shipVia?.IsExternal != true)
				return null;

			var plugin = CarrierPlugin.PK.Find(this, shipVia.CarrierPluginID);
			if (plugin?.SiteID == null)
				return null;

			return SiteList.Select().RowCast<SOOrderSite>().All(s => s.SiteID == plugin.SiteID)
				? null
				: new PXSetPropertyException(Messages.ShipViaNotApplicableToOrder, PXErrorLevel.Warning);
		}

		protected virtual bool AllowChangingOverrideFreightAmount(SOOrder doc)
		{
			// need to prevent changing Freight Amount Source if a shipment exists
			ShipTerms terms = ShipTerms.PK.Find(this, doc.ShipTermsID);
			return terms == null && doc.ShipmentCntr <= 0 || terms?.FreightAmountSource == FreightAmountSourceAttribute.OrderBased;
		}

		protected virtual void SOOrder_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (e.Row != null && ((SOOrder)e.Row).ShipmentCntr > 0)
			{
				throw new PXSetPropertyException(Messages.BackOrderCannotBeDeleted);
			}

			if (this.Adjustments.Select().Count > 0 && Document.Ask(Messages.Warning, Messages.SalesOrderWillBeDeleted, MessageButtons.OKCancel) != WebDialogResult.OK)
			{
				e.Cancel = true;
			}
		}

		protected virtual void SOOrder_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			SOOrder quoteorder = PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Current<SOOrder.origOrderType>>, And<SOOrder.orderNbr, Equal<Current<SOOrder.origOrderNbr>>>>>.SelectSingleBound(this, new object[] { e.Row });
			if (quoteorder != null && quoteorder.Behavior == SOOrderTypeConstants.QuoteOrder)
			{
				quoteorder.Completed = false;
				Document.Cache.SetStatus(quoteorder, PXEntryStatus.Updated);
			}
		}

		protected virtual void RQRequisitionOrder_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var reqord = (RQ.RQRequisitionOrder)e.Row;
			SOOrderType ordtype = soordertype.SelectWindowed(0, 1, reqord.OrderType);
			if (ordtype.Behavior == SOOrderTypeConstants.QuoteOrder)
			{
				var req = (RQ.RQRequisition)PXParentAttribute.SelectParent(sender, reqord, typeof(RQ.RQRequisition));
				req.Quoted = false;
				rqrequisition.Cache.SetStatus(req, PXEntryStatus.Updated);
			}
		}

		public virtual void UpdateControlTotal(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;

			if (row.Completed == false)
			{
				if (soordertype.Current.RequireControlTotal == false)
				{
					if (row.CuryOrderTotal != row.CuryControlTotal)
					{
						if (row.CuryOrderTotal != null && row.CuryOrderTotal != 0)
							sender.SetValueExt<SOOrder.curyControlTotal>(e.Row, row.CuryOrderTotal);
						else
							sender.SetValueExt<SOOrder.curyControlTotal>(e.Row, 0m);
					}
				}
			}
		}

		public virtual void ValidateControlTotal(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row == null) return;

			bool isex = false;

			if (row.Hold == false && row.Completed == false)
			{
				if (soordertype.Current.RequireControlTotal == true)
				{
					if (row.CuryOrderTotal != row.CuryControlTotal)
					{
						sender.RaiseExceptionHandling<SOOrder.curyControlTotal>(e.Row, row.CuryControlTotal, new PXSetPropertyException(Messages.DocumentOutOfBalance));
						isex = true;
					}
					else if (row.CuryOrderTotal < 0m && row.ARDocType != ARDocType.NoUpdate && row.Behavior != SOBehavior.RM)
					{
						sender.RaiseExceptionHandling<SOOrder.curyControlTotal>(e.Row, row.CuryControlTotal, new PXSetPropertyException(Messages.DocumentBalanceNegative));
						isex = true;
					}
				}
				else
				{
					if (row.CuryOrderTotal < 0m && row.ARDocType != ARDocType.NoUpdate && row.Behavior != SOBehavior.RM)
					{
						sender.RaiseExceptionHandling<SOOrder.curyOrderTotal>(e.Row, row.CuryOrderTotal, new PXSetPropertyException(Messages.DocumentBalanceNegative));
						isex = true;
					}
				}
			}

			if (!isex)
			{
				if (soordertype.Current.RequireControlTotal == true)
				{
					sender.RaiseExceptionHandling<SOOrder.curyControlTotal>(e.Row, null, null);
				}
				else
				{
					sender.RaiseExceptionHandling<SOOrder.curyOrderTotal>(e.Row, null, null);
				}
			}
		}

		protected virtual void SOOrder_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			SOOrder oldRow = e.OldRow as SOOrder;
			if (row == null) return;

			if (!sender.ObjectsEqual<SOOrder.shipVia>(e.OldRow, e.Row))
			{
				if (oldRow.ShipVia != null && oldRow.ShipVia == row.ShipVia || row.IsManualPackage == true)
				{
					// do not delete packages
				}
				else
				{
					//autopackaging
					if (string.IsNullOrEmpty(row.ShipVia))
					{
						foreach (SOPackageInfoEx package in Packages.Select())
						{
							Packages.Delete(package);
						}
						row.PackageWeight = 0;
					}
					else
					{
						CarrierRatesExt.RecalculatePackagesForOrder(Document.Current);
					}
				}
			}

			if (e.ExternalCall && (!sender.ObjectsEqual<SOOrder.disableAutomaticDiscountCalculation>(e.OldRow, e.Row)) && row.DisableAutomaticDiscountCalculation == true)
			{
				foreach (SOOrderDiscountDetail discountDetail in DiscountDetails.Select())
				{
					discountDetail.IsManual = true;
					DiscountDetails.Update(discountDetail);
				}

				foreach (SOLine line in Transactions.Select())
				{
					if (line.IsFree != true && line.LineType != SOLineType.Discount)
					{
						line.ManualDisc = true;
						Transactions.Update(line);
					}
				}

				Document.Cache.RaiseExceptionHandling<SOOrder.disableAutomaticDiscountCalculation>(row, null,
							new PXSetPropertyException(Messages.ManualDiscountFlagSetOnAllLines, PXErrorLevel.Warning));
			}

			if (e.ExternalCall && (!sender.ObjectsEqual<SOOrder.orderDate>(e.OldRow, e.Row)))
			{
				using(GetPriceCalculationScope().AppendContext<SOOrder.orderDate>())
				_discountEngine.AutoRecalculatePricesAndDiscounts(Transactions.Cache, Transactions, null, DiscountDetails, row.CustomerLocationID, row.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current));
			}

			if (!_discountEngine.IsInternalDiscountEngineCall && e.ExternalCall && sender.GetStatus(row) != PXEntryStatus.Deleted && !sender.ObjectsEqual<SOOrder.curyDiscTot>(e.OldRow, e.Row))
			{
				_discountEngine.SetTotalDocDiscount(Transactions.Cache, Transactions, DiscountDetails,
					Document.Current.CuryDiscTot, DiscountEngine.DiscountCalculationOptions.DisableAPDiscountsCalculation);
				RecalculateTotalDiscount();
			}

			UpdateControlTotal(sender, e);

			if (row.CustomerID != null && row.CuryDiscTot != null && row.CuryDiscTot > 0 && row.CuryLineTotal != null && row.CuryMiscTot != null && (row.CuryLineTotal > 0 || row.CuryMiscTot > 0))
			{
				decimal discountLimit = _discountEngine.GetDiscountLimit(sender, row.CustomerID);
				if (((row.CuryLineTotal + row.CuryMiscTot) / 100 * discountLimit) < row.CuryDiscTot)
					PXUIFieldAttribute.SetWarning<SOOrder.curyDiscTot>(sender, row, PXMessages.LocalizeFormatNoPrefix(AR.Messages.DocDiscountExceedLimit, discountLimit));
			}

			if (!sender.ObjectsEqual<SOOrder.isCCAuthorized>(e.Row, e.OldRow) && row.IsCCAuthorized == true ||
				!sender.ObjectsEqual<SOOrder.isCCCaptured>(e.Row, e.OldRow) && row.IsCCCaptured == true)
			{
				List<object> records = new List<object>();
				records.Add(e.Row);
				PXView dummyView = new PXView.Dummy(this, Document.View.BqlSelect, records);
				foreach (object item in creditHold.Press(new PXAdapter(dummyView))) {; }
			}

			if (!sender.ObjectsEqual<SOOrder.lineTotal, SOOrder.orderWeight, SOOrder.packageWeight, SOOrder.orderVolume, SOOrder.shipTermsID, SOOrder.shipZoneID, SOOrder.shipVia, SOOrder.useCustomerAccount>(e.OldRow, e.Row)
				|| !sender.ObjectsEqual<SOOrder.curyFreightCost, SOOrder.overrideFreightAmount>(e.OldRow, e.Row))
			{
				Carrier carrier = Carrier.PK.Find(sender.Graph, row.ShipVia);
				if (!sender.ObjectsEqual<SOOrder.shipVia>(e.OldRow, e.Row) && carrier?.CalcMethod == CarrierCalcMethod.Manual)
				{
					row.FreightCost = 0m;
				}
				if (carrier != null && carrier.IsExternal == true && soordertype.Current?.CalculateFreight == true)
				{
					row.FreightCostIsValid = false;
				}
				else
				{
					if (!IsImportFromExcel)
						CalcFreight(sender, row);
				}
			}

			if (row.IsManualPackage != true &&
				(row.OrderWeight != oldRow.OrderWeight ||
				(row.ShipVia != oldRow.ShipVia && !string.IsNullOrEmpty(oldRow.ShipVia))))
			{
				sender.SetValue<SOOrder.isPackageValid>(row, false);
			}

			if (!sender.ObjectsEqual<SOOrder.curyFreightTot, SOOrder.freightTaxCategoryID>(e.OldRow, e.Row))
			{
				SOOrderTaxAttribute.Calculate<SOOrder.freightTaxCategoryID>(sender, e);
			}
			if (!sender.ObjectsEqual<SOOrder.hold>(e.Row, e.OldRow) && row.Hold != true)
			{
				if (soordertype.Current.RequireShipping == true && soordertype.Current.ARDocType != ARDocType.NoUpdate)
					foreach (SOLine line in Transactions.Select())
					{
						if ((line.SalesAcctID == null || line.SalesSubID == null))
							Transactions.Cache.MarkUpdated(line);

						PXDefaultAttribute.SetPersistingCheck<SOLine.salesAcctID>(Transactions.Cache, line, PXPersistingCheck.NullOrBlank);

						PXDefaultAttribute.SetPersistingCheck<SOLine.salesSubID>(Transactions.Cache, line, PXPersistingCheck.NullOrBlank);
					}
			}

			if (!sender.ObjectsEqual<SOOrder.customerLocationID, SOOrder.orderDate>(e.Row, e.OldRow))
			{
				WebDialogResult old_answer = Document.View.Answer;

				Document.View.Answer = WebDialogResult.None;
				try
				{
					RecalcDiscountsParamFilter recalcFilter = new RecalcDiscountsParamFilter();
					recalcFilter.OverrideManualDiscounts = false;
					recalcFilter.OverrideManualPrices = false;
					recalcFilter.RecalcDiscounts = true;
					recalcFilter.RecalcUnitPrices = true;
					recalcFilter.RecalcTarget = RecalcDiscountsParamFilter.AllLines;

					using (GetPriceCalculationScope().AppendContext<SOOrder.customerLocationID, SOOrder.orderDate>())
					{
					_discountEngine.RecalculatePricesAndDiscounts(Transactions.Cache, Transactions, Transactions.Current, DiscountDetails, Document.Current.CustomerLocationID, Document.Current.OrderDate, recalcFilter, GetDefaultSODiscountCalculationOptions(Document.Current));
					RecalculateTotalDiscount();
				}
				}
				finally
				{
					Document.View.Answer = old_answer;
				}
			}

			if (!sender.ObjectsEqual<SOOrder.completed, SOOrder.cancelled>(e.Row, e.OldRow) && (row.Completed == true && row.BilledCntr == 0 && row.ShipmentCntr <= row.BilledCntr + row.ReleasedCntr || row.Cancelled == true))
			{
				foreach (SOAdjust adj in Adjustments_Raw.Select())
				{
					SOAdjust copy = PXCache<SOAdjust>.CreateCopy(adj);
					copy.CuryAdjdAmt = 0m;
					copy.CuryAdjgAmt = 0m;
					copy.AdjAmt = 0m;
					Adjustments.Update(copy);
				}
			}
		}

		private void CalcFreight(PXCache sender, SOOrder document)
		{
			if (!(soordertype.Current?.CalculateFreight == false))
			{
				FreightCalculator fc = CreateFreightCalculator();
				fc.CalcFreightCost<SOOrder, SOOrder.curyFreightCost>(sender, document);
				document.FreightCostIsValid = true;
				if (document.OverrideFreightAmount != true)
				{
					PXResultset<SOLine> res = Transactions.Select();
					fc.ApplyFreightTerms<SOOrder, SOOrder.curyFreightAmt>(sender, document, res.Count);
				}
			}
		}

		protected virtual void SOOrder_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			SOOrder row = (SOOrder)e.Row;
			if (row == null) return;

			if (!PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>())
			{
				row.IsManualPackage = true;
			}
		}

		protected virtual void SOOrder_CreatePMInstance_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;

			if (row != null && row.CreatePMInstance != (bool)e.OldValue)
			{
				bool needUpdate = false;
				int? oldID = row.PMInstanceID;
				if (row.CreatePMInstance == true)
				{
					AR.CustomerPaymentMethod res = this.CreateDefPaymentMethod(row);
					row.PMInstanceID = res.PMInstanceID;
					needUpdate = true;
				}
				else
				{
					this.RemoveInsertedPMInstance();
					row.PMInstanceID = null;
					needUpdate = true;
				}
				if (needUpdate)
				{
					sender.RaiseFieldUpdated<SOOrder.pMInstanceID>(row, oldID);
					this.DefPaymentMethodInstance.View.RequestRefresh();
				}
			}
		}

		protected virtual void SOOrder_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null && row.CreatePMInstance == true)
			{
				CustomerPaymentMethodC pmInstance = (CustomerPaymentMethodC)this.DefPaymentMethodInstance.Current;
				if (pmInstance != null)
				{
					this.DefPaymentMethodInstance.SetValueExt<CustomerPaymentMethodC.cashAccountID>(pmInstance, row.CashAccountID);
				}
			}
		}

		protected virtual void SOOrder_PreAuthTranNumber_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOOrder row = e.Row as SOOrder;
			if (row != null && row.IsCCAuthorized == false && !string.IsNullOrEmpty(row.PreAuthTranNumber))
			{
				decimal amount = row.CuryOrderTotal.Value;
				sender.SetValue<SOOrder.curyCCPreAuthAmount>(e.Row, amount);
				sender.SetValuePending<SOOrder.curyCCPreAuthAmount>(e.Row, amount);
			}
		}

		protected virtual void SOOrder_TermsID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Terms terms = (Terms)PXSelectorAttribute.Select<SOOrder.termsID>(sender, e.Row);

			if (terms != null && terms.InstallmentType != TermsInstallmentType.Single)
			{
				PXResultset<SOAdjust> adjustments = Adjustments.Select();
				if (adjustments.Count > 0)
				{
					PXUIFieldAttribute.SetWarning<SOOrder.termsID>(sender, e.Row, Messages.TermsChangedToMultipleInstallment);
					foreach (SOAdjust adj in adjustments)
					{
						Adjustments.Cache.Delete(adj);
					}
				}
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<Current<SOOrder.shipmentCntr>, Equal<int0>, And<Current<SOOrder.overrideFreightAmount>, Equal<False>,
			Or<ShipTerms.freightAmountSource, Equal<Current<SOOrder.freightAmountSource>>>>>),
			Messages.CantSelectShipTermsWithFreightAmountSource, typeof(ShipTerms.freightAmountSource))]
		protected virtual void SOOrder_ShipTermsID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void SOOrder_CuryFreightAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ShowWarningIfPartiallyInvoiced<SOOrder.curyFreightAmt>(sender, (SOOrder)e.Row);
		}

		protected virtual void SOOrder_CuryPremiumFreightAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ShowWarningIfPartiallyInvoiced<SOOrder.curyPremiumFreightAmt>(sender, (SOOrder)e.Row);
		}

		protected virtual void ShowWarningIfPartiallyInvoiced<amtField>(PXCache sender, SOOrder doc)
			where amtField : IBqlField
		{
			if (!this.UnattendedMode
				&& sosetup.Current.FreightAllocation == FreightAllocationList.FullAmount
				&& doc?.BilledCntr + doc?.ReleasedCntr > 0)
			{
				sender.RaiseExceptionHandling<amtField>(
					doc, sender.GetValue<amtField>(doc),
					new PXSetPropertyException(Messages.PleaseAdjustManuallyInInvoice, PXErrorLevel.Warning,
						PXUIFieldAttribute.GetDisplayName<amtField>(sender)));
			}
		}

		#endregion

		#region Default Payment Method Events

		protected virtual void CustomerPaymentMethodC_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CustomerPaymentMethod;
			if (row != null)
			{
				SOOrder doc = this.Document.Current;
				if (doc != null && doc.PMInstanceID == row.PMInstanceID)
				{
					bool isInserted = (cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
					PXUIFieldAttribute.SetEnabled(cache, e.Row, isInserted); //Allow Edit new record only
					PXUIFieldAttribute.SetEnabled(this.DefPaymentMethodInstanceDetails.Cache, null, isInserted);
					PXUIFieldAttribute.SetEnabled<CustomerPaymentMethodC.paymentMethodID>(cache, e.Row, (isInserted || String.IsNullOrEmpty(row.PaymentMethodID)));
					PXUIFieldAttribute.SetEnabled<CustomerPaymentMethodC.descr>(cache, row, false);
					PXUIFieldAttribute.SetVisible<CustomerPaymentMethodC.cCProcessingCenterID>(cache, row, doc.CreatePMInstance == true);
					if (!String.IsNullOrEmpty(row.PaymentMethodID))
					{
						PaymentMethod pmDef = (PaymentMethod)this.PaymentMethodDef.Select();
						bool singleInstance = pmDef?.ARIsOnePerCustomer ?? false;
						bool isIDMaskExists = false;
						if (!singleInstance)
						{
							foreach (PaymentMethodDetail iDef in this.PMDetails.Select(row.PaymentMethodID))
							{
								if ((iDef.IsIdentifier ?? false) && (!string.IsNullOrEmpty(iDef.DisplayMask)))
								{
									isIDMaskExists = true;
									break;
								}
							}
						}
						if (!(isIDMaskExists || singleInstance))
						{
							PXUIFieldAttribute.SetEnabled<CustomerPaymentMethodC.descr>(cache, row, true);
						}
						if (!isInserted)
						{
							ExternalTransaction tran = PXSelectReadonly<ExternalTransaction, Where<ExternalTransaction.pMInstanceID, Equal<Required<ExternalTransaction.pMInstanceID>>>>
								.SelectWindowed(this, 0, 1, row.PMInstanceID);
							bool hasTransactions = tran != null;
							this.DefPaymentMethodInstanceDetails.Cache.AllowDelete = !hasTransactions;
						}
					}
				}
			}
		}

		protected virtual void CustomerPaymentMethodC_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (CurrentDocument.Current.CreatePMInstance != true)
				this.DefPaymentMethodInstance.Cache.Remove((CustomerPaymentMethodC)e.Row);
		}

		protected virtual void CustomerPaymentMethodC_PaymentMethodID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				SOOrder order = this.Document.Current;
				if (order != null)
				{
					e.NewValue = order.PaymentMethodID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CustomerPaymentMethodC_CashAccountID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				SOOrder order = this.Document.Current;
				if (order != null)
				{
					e.NewValue = order.CashAccountID;
				}
			}
		}

		protected virtual void CustomerPaymentMethodC_PaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			AR.CustomerPaymentMethod row = (AR.CustomerPaymentMethod)e.Row;
			this.ClearPMDetails();
			this.AddPMDetails();
			row.CashAccountID = null;
			cache.SetDefaultExt<CustomerPaymentMethodC.cashAccountID>(e.Row);
			PaymentMethod pmDef = this.PaymentMethodDef.Select();
			if (pmDef.ARIsOnePerCustomer ?? false)
			{
				row.Descr = pmDef.Descr;
			}
			this.DefPaymentMethodInstanceDetails.View.RequestRefresh();
		}

		protected virtual void CustomerPaymentMethodC_Descr_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			AR.CustomerPaymentMethod row = (AR.CustomerPaymentMethod)e.Row;
			if (row != null && row.PaymentMethodID != null)
			{
				PaymentMethod pmDef = this.PaymentMethodDef.Select(row.PaymentMethodID);
				if (pmDef != null)
					if (pmDef.ARIsOnePerCustomer ?? false)
					{
						row.Descr = pmDef.Descr;
					}
			}
		}

		protected virtual void CustomerPaymentMethodC_Descr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			AR.CustomerPaymentMethod row = (AR.CustomerPaymentMethod)e.Row;
			PaymentMethod def = this.PaymentMethodDef.Select(row.PaymentMethodID);
			if (!(def.ARIsOnePerCustomer ?? false))
			{
				AR.CustomerPaymentMethod existing = PXSelect<AR.CustomerPaymentMethod,
				Where<AR.CustomerPaymentMethod.bAccountID, Equal<Required<AR.CustomerPaymentMethod.bAccountID>>,
				And<AR.CustomerPaymentMethod.paymentMethodID, Equal<Required<AR.CustomerPaymentMethod.paymentMethodID>>,
				And<AR.CustomerPaymentMethod.pMInstanceID, NotEqual<Required<AR.CustomerPaymentMethod.pMInstanceID>>,
				And<AR.CustomerPaymentMethod.descr, Equal<Required<AR.CustomerPaymentMethod.descr>>,
				And<AR.CustomerPaymentMethod.isActive, Equal<True>>>>>>>.Select(this, row.BAccountID, row.PaymentMethodID, row.PMInstanceID, row.Descr);
				if (existing != null)
				{
					cache.RaiseExceptionHandling<CustomerPaymentMethodC.descr>(row, row.Descr, new PXSetPropertyException(AR.Messages.CustomerPMInstanceHasDuplicatedDescription, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void CustomerPaymentMethodC_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			AR.CustomerPaymentMethod row = (AR.CustomerPaymentMethod)e.Row;
			if (row == null) return;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
			if (!string.IsNullOrEmpty(row.CustomerCCPID))
			{
				CustomerProcessingCenterID test = CustomerProcessingID.Select(row.CCProcessingCenterID, row.CustomerCCPID);
				if (test == null)
				{
					CustomerProcessingCenterID cPCID = new CustomerProcessingCenterID();
					cPCID.BAccountID = row.BAccountID;
					cPCID.CCProcessingCenterID = row.CCProcessingCenterID;
					cPCID.CustomerCCPID = row.CustomerCCPID;
					CustomerProcessingID.Insert(cPCID);
				}
			}
		}

		#region PM Details Events

		protected virtual void CustomerPaymentMethodDetail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CustomerPaymentMethodDetail row = (CustomerPaymentMethodDetail)e.Row;
			if (row != null)
			{
				bool isPMInserted = this.DefPaymentMethodInstance.Cache.GetStatus(this.DefPaymentMethodInstance.Current) == PXEntryStatus.Inserted;
				PaymentMethodDetail iTempl = this.FindTemplate(row);
				bool isRequired = (iTempl != null) && (iTempl.IsRequired ?? false);
				PXDefaultAttribute.SetPersistingCheck<CustomerPaymentMethodDetail.value>(cache, row, (isRequired) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXRSACryptStringAttribute.SetDecrypted<CustomerPaymentMethodDetail.value>(cache, row, !(iTempl?.IsEncrypted ?? false));
				if (iTempl?.IsExpirationDate == true)
				{
					PXUIFieldAttribute.SetEnabled<CustomerPaymentMethodDetail.value>(cache, row, isPMInserted);
				}
				string procCenterId = DefPaymentMethodInstance.Current.CCProcessingCenterID;
				if (procCenterId != null && !CCPluginTypeHelper.CheckProcessingCenterSupported(this, procCenterId))
				{
					PXUIFieldAttribute.SetEnabled(this.DefPaymentMethodInstanceDetails.Cache, null, false);
				}
			}
		}

		protected virtual void CustomerPaymentMethodDetail_Value_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethodDetail row = e.Row as CustomerPaymentMethodDetail;
			PaymentMethodDetail def = this.FindTemplate(row);
			if (def != null)
			{
				if (def.IsIdentifier ?? false)
				{
					string id = CustomerPaymentMethodMaint.IDObfuscator.GetMaskByID(this, row.Value, def.DisplayMask, DefPaymentMethodInstanceDetails.Current.PMInstanceID);

					if (this.DefPaymentMethodInstance.Current.Descr != id)
					{
						CustomerPaymentMethodC parent = this.DefPaymentMethodInstance.Current;
						parent.Descr = String.Format("{0}:{1}", parent.PaymentMethodID, id);
						this.DefPaymentMethodInstance.Update(parent);
					}
				}
				if (def.IsExpirationDate ?? false)
				{
					CustomerPaymentMethodC parent = this.DefPaymentMethodInstance.Current;
					DefPaymentMethodInstance.Cache.SetValueExt<CustomerPaymentMethodC.expirationDate>(parent, CustomerPaymentMethodMaint.ParseExpiryDate(this, parent, row.Value));
					this.DefPaymentMethodInstance.Update(parent);
				}
			}
		}

		protected virtual void CustomerPaymentMethodDetail_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			CustomerPaymentMethodDetail row = (CustomerPaymentMethodDetail)e.Row;
			PaymentMethodDetail def = this.FindTemplate(row);
			if (def.IsIdentifier ?? false)
			{
				this.DefPaymentMethodInstance.Current.Descr = null;
			}
		}

		#endregion

		protected virtual void AddPMDetails()
		{
			string pmID = this.DefPaymentMethodInstance.Current.PaymentMethodID;
			if (!String.IsNullOrEmpty(pmID))
			{
				foreach (PaymentMethodDetail it in this.PMDetails.Select())
				{
					CustomerPaymentMethodDetail det = new CustomerPaymentMethodDetail();
					det.DetailID = it.DetailID;
					det = this.DefPaymentMethodInstanceDetails.Insert(det);
				}
			}
		}
		protected virtual void ClearPMDetails()
		{
			foreach (CustomerPaymentMethodDetail iDet in this.DefPaymentMethodInstanceDetails.Select())
			{
				this.DefPaymentMethodInstanceDetails.Delete(iDet);
			}
		}
		protected virtual PaymentMethodDetail FindTemplate(CustomerPaymentMethodDetail aDet)
		{
			PaymentMethodDetail res = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
						And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
						And<PaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>>>>>.Select(this, aDet.PaymentMethodID, aDet.DetailID);
			return res;
		}

		protected virtual CustomerPaymentMethod CreateDefPaymentMethod(SOOrder doc)
		{
			CustomerPaymentMethodC pmInstance = new CustomerPaymentMethodC();
			CustomerPaymentMethodC currentPM = this.DefPaymentMethodInstance.Current;
			if (currentPM != null)
			{
				RemoveInsertedPMInstance();
			}
			pmInstance = this.DefPaymentMethodInstance.Insert(pmInstance);
			DefPaymentMethodInstance.Cache.SetDefaultExt<CustomerPaymentMethodC.cCProcessingCenterID>(pmInstance);
			if (pmInstance.BAccountID == null)
			{
				pmInstance.BAccountID = doc.CustomerID;

			}
			this.AddPMDetails();
			this.DefPaymentMethodInstance.Current = pmInstance;

			if (!String.IsNullOrEmpty(doc.CCCardNumber))
			{
				CustomerPaymentMethodDetail idDetail = null;
				foreach (PXResult<CustomerPaymentMethodDetail, PaymentMethodDetail> iDet in this.DefPaymentMethodInstanceDetails.Select(pmInstance.PMInstanceID))
				{
					if (((PaymentMethodDetail)iDet).IsIdentifier == true)
					{
						idDetail = iDet;
					}
				}
				if (idDetail != null)
				{
					this.DefPaymentMethodInstanceDetails.Cache.SetValueExt<CustomerPaymentMethodDetail.value>(idDetail, doc.CCCardNumber);
					this.Document.Cache.SetValueExt<SOOrder.cCCardNumber>(doc, string.Empty);
				}
			}


			return pmInstance;
		}

		protected virtual void RemoveInsertedPMInstance()
		{
			foreach (CustomerPaymentMethodC currentPM in this.DefPaymentMethodInstance.Select())
			{
				if (this.DefPaymentMethodInstance.Cache.GetStatus(currentPM) == PXEntryStatus.Inserted)
				{
					this.DefPaymentMethodInstance.Delete(currentPM);
					foreach (CustomerPaymentMethodDetail iDet in this.DefPaymentMethodInstanceDetails.Select())
					{
						this.DefPaymentMethodInstanceDetails.Delete(iDet);
					}
				}
			}
		}
		#endregion

		#region SOLine events

		protected object GetValue<Field>(object data)
			where Field : IBqlField
		{
			return this.Caches[BqlCommand.GetItemType(typeof(Field))].GetValue(data, typeof(Field).Name);
		}

		[PXBool]
		[DRTerms.Dates(typeof(SOLine.dRTermStartDate), typeof(SOLine.dRTermEndDate), typeof(SOLine.inventoryID), VerifyDatesPresent = false)]
		protected virtual void SOLine_ItemRequiresTerms_CacheAttached(PXCache sender) { }

		[SOCommitment]
		[PXDBGuid]
		protected virtual void SOLine_CommitmentID_CacheAttached(PXCache sender) { }

		protected virtual void SOLine_SiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Current != null && Document.Current.DefaultSiteID != null)
			{
				e.NewValue = Document.Current.DefaultSiteID;
				e.Cancel = true;
				return;
			}

			SOLine line = (SOLine)e.Row;
			if (line == null) return;
			InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);
			if (item != null && item.StkItem != true)
			{
				e.NewValue = item.DfltSiteID;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row == null) return;

			object oldValue = sender.GetValue<SOLine.inventoryID>(row);
			if (oldValue != null)
			{
				if (row.InvoiceNbr != null)
				{
					e.NewValue = oldValue;
				}
				else
				{
					foreach (SOLineSplit split in splits.Select())
					{
						if (split.Completed == true)
						{
							e.NewValue = oldValue;
							sender.RaiseExceptionHandling<SOLine.inventoryID>(row, oldValue, new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.InventoryIDCannotBeChanged, split.SplitLineNbr.ToString()), PXErrorLevel.Warning));
						}
					}
				}
			}
		}
		protected virtual void SOLine_POCreate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row == null) return;

			bool? newVal = (bool?)e.NewValue;
			POCreateVerifyValue(sender, row, newVal);
		}

		public virtual void POCreateVerifyValue(PXCache sender, SOLine row, bool? value)
		{
			if (row == null) return;

			if (row.InventoryID != null && value == true)
			{
				InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
				if (item != null && item.StkItem != true)
				{
					if (item.KitItem == true)
					{
						throw new PXSetPropertyException(Messages.SOPOLinkNotForNonStockKit);
					}
					else if ((item.NonStockShip != true || item.NonStockReceipt != true) && PXAccess.FeatureInstalled<FeaturesSet.sOToPOLink>())
					{
						sender.RaiseExceptionHandling<SOLine.pOCreate>(row, value, new PXSetPropertyException(Messages.NonStockShipReceiptIsOff, PXErrorLevel.Warning));
					}
				}
			}
		}

		protected virtual void SOLine_SalesAcctID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null && row.ProjectID != null && !PM.ProjectDefaultAttribute.IsNonProject(row.ProjectID) && row.TaskID != null)
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, e.NewValue);
				if (account != null && account.AccountGroupID == null)
				{
					sender.RaiseExceptionHandling<SOLine.salesAcctID>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Warning, account.AccountCD));
				}
			}
		}


		protected virtual void SOLine_SalesAcctID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null && row.TaskID == null)
			{
				sender.SetDefaultExt<SOLine.taskID>(e.Row);
			}
		}


		protected virtual void SOLine_SubItemID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (((SOLine)e.Row).InvoiceNbr != null)
			{
				object oldValue = sender.GetValue<SOLine.subItemID>(e.Row);
				if (oldValue != null)
				{
					e.NewValue = oldValue;
				}
			}
		}

		protected virtual void SOLine_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine line = (SOLine)e.Row;
			if (line != null && line.InvoiceNbr != null)
			{
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_UOM_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (((SOLine)e.Row)?.InvoiceNbr != null)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_UOM_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (((SOLine)e.Row).InvoiceNbr != null)
			{
				object oldValue = sender.GetValue<SOLine.uOM>(e.Row);
				if (oldValue != null)
				{
					e.NewValue = oldValue;
				}
			}
		}

		protected virtual void SOLine_SalesAcctID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine line = (SOLine)e.Row;

			if (line != null && IsTransferOrder == false)
			{
				InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);
				if (item == null)
					return;
				switch (soordertype.Current.SalesAcctDefault)
				{
					case SOSalesAcctSubDefault.MaskItem:
						e.NewValue = GetValue<InventoryItem.salesAcctID>(item);
						e.Cancel = true;
						break;
					case SOSalesAcctSubDefault.MaskSite:
						INSite site = INSite.PK.Find(this, line.SiteID);
						if (site != null)
						{
							e.NewValue = GetValue<INSite.salesAcctID>(site);
							e.Cancel = true;
						}
						break;
					case SOSalesAcctSubDefault.MaskClass:
						INPostClass postclass = INPostClass.PK.Find(this, item.PostClassID) ?? new INPostClass();
						e.NewValue = GetValue<INPostClass.salesAcctID>(postclass);
						e.Cancel = true;
						break;
					case SOSalesAcctSubDefault.MaskLocation:
						Location customerloc = location.Current;
						e.NewValue = GetValue<Location.cSalesAcctID>(customerloc);
						e.Cancel = true;
						break;
					case SOSalesAcctSubDefault.MaskReasonCode:
						ReasonCode reasoncode = ReasonCode.PK.Find(this, line.ReasonCode);
						e.NewValue = GetValue<ReasonCode.salesAcctID>(reasoncode);
						e.Cancel = true;
						break;
				}
			}
		}

		protected virtual void SOLine_SalesSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine line = (SOLine)e.Row;

			if (line != null && IsTransferOrder == false && line.SalesAcctID != null)
			{
				InventoryItem item = InventoryItem.PK.Find(this, line.InventoryID);
				INSite site = INSite.PK.Find(this, line.SiteID);
				ReasonCode reasoncode = ReasonCode.PK.Find(this, line.ReasonCode);
				SalesPerson salesperson = (SalesPerson)PXSelectorAttribute.Select<SOLine.salesPersonID>(sender, e.Row);
				INPostClass postclass = INPostClass.PK.Find(this, item?.PostClassID) ?? new INPostClass();
				EPEmployee employee = (EPEmployee)PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Current<SOOrder.ownerID>>>>.Select(this);
				CRLocation companyloc =
					PXSelectJoin<CRLocation, InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>, InnerJoin<Branch, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>>>, Where<Branch.branchID, Equal<Required<SOLine.branchID>>>>.Select(this, line.BranchID);
				Location customerloc = location.Current;

				object item_SubID = GetValue<InventoryItem.salesSubID>(item);
				object site_SubID = GetValue<INSite.salesSubID>(site);
				object postclass_SubID = GetValue<INPostClass.salesSubID>(postclass);
				object customer_SubID = GetValue<Location.cSalesSubID>(customerloc);
				object employee_SubID = GetValue<EPEmployee.salesSubID>(employee);
				object company_SubID = GetValue<CRLocation.cMPSalesSubID>(companyloc);
				object salesperson_SubID = GetValue<SalesPerson.salesSubID>(salesperson);
				object reasoncode_SubID = GetValue<ReasonCode.salesSubID>(reasoncode);

				object value = null;

				try
				{
					value = SOSalesSubAccountMaskAttribute.MakeSub<SOOrderType.salesSubMask>(this, soordertype.Current.SalesSubMask,
																							 new object[]
																							 {
																								 item_SubID,
																								 site_SubID,
																								 postclass_SubID,
																								 customer_SubID,
																								 employee_SubID,
																								 company_SubID,
																								 salesperson_SubID,
																								 reasoncode_SubID
																							 },
																							 new Type[]
																							 {
																								 typeof(InventoryItem.salesSubID),
																								 typeof(INSite.salesSubID),
																								 typeof(INPostClass.salesSubID),
																								 typeof(Location.cSalesSubID),
																								 typeof(EPEmployee.salesSubID),
																								 typeof(Location.cMPSalesSubID),
																								 typeof(SalesPerson.salesSubID),
																								 typeof(ReasonCode.subID)
																							 });

					sender.RaiseFieldUpdating<SOLine.salesSubID>(line, ref value);
				}
				catch (PXMaskArgumentException ex)
				{
					sender.RaiseExceptionHandling<SOLine.salesSubID>(e.Row, null, new PXSetPropertyException(ex.Message));
					value = null;
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<SOLine.salesSubID>(e.Row, value, ex);
					value = null;
				}

				e.NewValue = (int?)value;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_Completed_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && ((SOLine)e.Row).LineType == SOLineType.MiscCharge)
			{
				e.NewValue = true;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_ShipComplete_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null && row.Completed == false)
			{
				foreach (SOLineSplit split in PXParentAttribute.SelectChildren(splits.Cache, e.Row, typeof(SOLine)))
				{
					if (split.Completed != true && split.POCompleted != true && split.POCancelled != true)
						splits.Cache.SetValue<SOLineSplit.shipComplete>(split, row.ShipComplete);
				}
			}
		}

		protected virtual void SOLine_VendorID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (!(soordertype.Current.RequireShipping == true && soordertype.Current.RequireLocation == false && row != null && row.TranType != INDocType.Undefined))
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row != null && row.RequireLocation != true)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}
		protected virtual void SOLineSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLineSplit row = (SOLineSplit)e.Row;
			if (row != null && row.RequireLocation != true)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_Completed_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if ((bool?)sender.GetValue<SOLine.completed>(e.Row) != true)
			{
				sender.SetValueExt<SOLine.closedQty>(e.Row, sender.GetValue<SOLine.shippedQty>(e.Row));
			}
		}

		protected virtual void SOLine_TranType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOLine.pOCreate>(e.Row);
		}

		protected virtual void SOLine_POCreate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (!(soordertype.Current.RequireShipping == true && soordertype.Current.RequireLocation == false && soordertype.Current.RequireAllocation == false && row != null && row.TranType != INDocType.Undefined && row.Operation == SOOperation.Issue))
			{
				e.NewValue = false;
				e.Cancel = true;
			}
		}

		protected virtual void SiteStatus_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (lsselect.AvailabilityFetching) return;
			var row = (SiteStatus)e.Row;
			if (row == null)
				return;

			var itemSettings = (INItemSiteSettings)initemsettings.SelectSingle(row.InventoryID, row.SiteID);
			if (Document.Current != null && Document.Current.Behavior != SOOrderTypeConstants.QuoteOrder && itemSettings != null && itemSettings.INItemSiteExists != true)
				row.InitSiteStatus = true;
		}

		protected virtual void SOLine_POSiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row != null)
			{
				INItemSiteSettings itemSettings = initemsettings.SelectSingle(row.InventoryID, row.SiteID);

				object newVal = null;
				if (itemSettings != null)
				{
					if (itemSettings.ReplenishmentSource == INReplenishmentSource.Purchased ||
						itemSettings.ReplenishmentSource == INReplenishmentSource.PurchaseToOrder ||
						itemSettings.ReplenishmentSource == INReplenishmentSource.DropShipToOrder)
						newVal = itemSettings.ReplenishmentSourceSiteID;
				}
				if (newVal == null)
					newVal = row.SiteID;

				e.NewValue = newVal;
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_POSource_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row != null)
			{
				if (row.POCreate != true)
				{
					e.NewValue = null;
					e.Cancel = true;
					return;
				}
				else if (PXAccess.FeatureInstalled<FeaturesSet.dropShipments>())
				{
					InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
					if (item != null && item.StkItem != true && item.NonStockReceipt == true && item.NonStockShip == true)
					{
						e.NewValue = INReplenishmentSource.DropShipToOrder;
						e.Cancel = true;
						return;
					}
				}

				INItemSiteSettings itemSettings = initemsettings.SelectSingle(row.InventoryID, row.SiteID);
				object newVal = null;
				if (itemSettings != null)
				{
					if (itemSettings.POSource == INReplenishmentSource.PurchaseToOrder ||
						itemSettings.POSource == INReplenishmentSource.DropShipToOrder)
						newVal = itemSettings.POSource;
				}

				if (newVal != null)
				{
					e.NewValue = newVal;
					e.Cancel = true;
				}
			}
		}

		protected virtual void SOLine_POSource_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row == null) return;

			if (e.NewValue != null && e.NewValue.ToString() == INReplenishmentSource.DropShipToOrder)
			{
				InventoryItem item = InventoryItem.PK.Find(this, row.InventoryID);
				if (item != null && item.StkItem != true)
				{
					if (item.NonStockReceipt != true || item.NonStockShip != true || row.LineType == SOLineType.MiscCharge)
					{
						throw new PXSetPropertyException<SOLine.pOSource>(Messages.ReceiptShipmentRequiredForDropshipNonstock);

					}
				}
			}
		}

		protected virtual void SOLine_POCreate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (row.POCreated != true)
			{
				if (row.POCreate == true)
					sender.SetDefaultExt<SOLine.pOSource>(e.Row);
				else
					sender.SetValueExt<SOLine.pOSource>(e.Row, null);
			}
			LSSOLine.ResetAvailabilityCounters(row);
		}

		protected virtual void SOLineSplit_POCreate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLineSplit row = e.Row as SOLineSplit;
			if (row.POCreate == true)
			{
				sender.SetDefaultExt<SOLineSplit.pOSource>(e.Row);
			}
			else
			{
				sender.SetValueExt<SOLineSplit.pOSource>(e.Row, null);
			}
		}

		protected virtual void SOLineSplit_POSource_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null && row.POCreate != true)
			{
				e.NewValue = null;
				e.Cancel = true;
			}
		}

		protected virtual void SOLineSplit_POSource_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLineSplit row = e.Row as SOLineSplit;

			//TODO: revisit this code
			//row.POType = null;
			//row.PONbr = null;
			//row.POLineNbr = null;
		}

		protected virtual void SOLineSplit_LotSerialNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLineSplit row = (SOLineSplit)e.Row;
			if (row != null && row.RequireLocation != true)
			{
				e.Cancel = true;
			}
		}

		protected virtual void SOLine_UnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && string.IsNullOrEmpty(((SOLine)e.Row).UOM) == false && (((SOLine)e.Row).LineType == SOLineType.NonInventory || ((SOLine)e.Row).LineType == SOLineType.MiscCharge) && Document.Current != null)
			{
				InventoryItem initem = InventoryItem.PK.Find(this, ((SOLine)e.Row).InventoryID);
				if (initem != null)
				{
					if (initem.StdCostDate <= Document.Current.OrderDate)
						e.NewValue = initem.StdCost ?? 0m;
					else
						e.NewValue = initem.LastStdCost ?? 0m;
					e.Cancel = true;
				}
			}
		}

		protected virtual void SOLine_CuryUnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && string.IsNullOrEmpty(((SOLine)e.Row).UOM) == false)
			{
				object unitcost;
				sender.RaiseFieldDefaulting<SOLine.unitCost>(e.Row, out unitcost);

				if (unitcost != null && (decimal)unitcost != 0m)
				{
					decimal newval = INUnitAttribute.ConvertToBase<SOLine.inventoryID, SOLine.uOM>(sender, e.Row, (decimal)unitcost, INPrecision.NOROUND);
					PXCurrencyAttribute.CuryConvCury(sender, e.Row, newval, out newval, CommonSetupDecPl.PrcCst);
					e.NewValue = newval;
					e.Cancel = true;
				}
			}
		}

		protected virtual void SOLine_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{

			SOLine row = (SOLine)e.Row;
			string oldUOM = (string)e.OldValue;

			if (row != null && oldUOM != row.UOM)
			{
				sender.SetDefaultExt<SOLine.curyUnitPrice>(row);
				sender.SetDefaultExt<SOLine.curyUnitCost>(row);
				sender.SetValueExt<SOLine.extWeight>(row, row.BaseQty * row.UnitWeigth);
				sender.SetValueExt<SOLine.extVolume>(row, row.BaseQty * row.UnitVolume);
			}
		}

		protected virtual void SOLine_Operation_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOLine.tranType>(e.Row);
			sender.SetDefaultExt<SOLine.invtMult>(e.Row);
			sender.SetDefaultExt<SOLine.planType>(e.Row);
			sender.SetDefaultExt<SOLine.requireReasonCode>(e.Row);
			sender.SetDefaultExt<SOLine.autoCreateIssueLine>(e.Row);

			SOLine row = (SOLine)e.Row;
			if (row == null) return;
			sender.SetValueExt<SOLine.curyUnitPrice>(e.Row, row.CuryUnitPrice);
			sender.SetValueExt<SOLine.discPct>(e.Row, row.DiscPct);
			sender.SetValueExt<SOLine.curyDiscAmt>(e.Row, row.CuryDiscAmt);
		}

		protected virtual void SOLine_SalesPersonID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOLine.salesSubID>(e.Row);
		}

		protected virtual void SOLine_ReasonCode_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null)
			{
				ReasonCode reasoncd = ReasonCode.PK.Find(this, (string)e.NewValue);

				if (reasoncd != null)
				{
					if (row.TranType == INTranType.Transfer && reasoncd.Usage == ReasonCodeUsages.Transfer ||
						row.TranType != INTranType.Transfer && reasoncd.Usage == ReasonCodeUsages.Issue ||
						row.TranType != INTranType.Issue && row.TranType != INTranType.Return && reasoncd.Usage == ReasonCodeUsages.Sales)
					{
						e.Cancel = true;
					}
					else
					{
						throw new PXSetPropertyException(IN.Messages.ReasonCodeDoesNotMatch);
					}

				}
			}
		}

		protected virtual void SOLine_ReasonCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<SOLine.salesAcctID>(e.Row);
			try
			{
				sender.SetDefaultExt<SOLine.salesSubID>(e.Row);
			}
			catch (PXSetPropertyException)
			{
				sender.SetValue<SOLine.salesSubID>(e.Row, null);
			}
		}

		protected virtual void SOLine_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (SOLine)e.Row;
			sender.SetDefaultExt<SOLine.salesAcctID>(row);
			try
			{
				sender.SetDefaultExt<SOLine.salesSubID>(row);
			}
			catch (PXSetPropertyException)
			{
				sender.SetValue<SOLine.salesSubID>(row, null);
			}
			sender.SetDefaultExt<SOLine.pOCreate>(row);
			sender.SetDefaultExt<SOLine.pOSource>(row);

			if (string.IsNullOrEmpty(row.InvoiceNbr))
			{
				sender.SetDefaultExt<SOLine.curyUnitCost>(row);
			}
			sender.SetDefaultExt<SOLine.pOSiteID>(row);
			using (GetPriceCalculationScope().AppendContext<SOLine.siteID>())
			sender.SetDefaultExt<SOLine.curyUnitPrice>(row);
		}

		[PopupMessage]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void SOLine_InventoryID_CacheAttached(PXCache sender)
		{
		}


		protected virtual void SOLine_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			sender.SetDefaultExt<SOLine.lineType>(e.Row);
			if (row.Operation == null)
				sender.SetDefaultExt<SOLine.operation>(e.Row);
			sender.SetDefaultExt<SOLine.tranType>(e.Row);
			sender.RaiseExceptionHandling<SOLine.uOM>(e.Row, null, null);
			sender.SetDefaultExt<SOLine.uOM>(e.Row);
			sender.SetValue<SOLine.closedQty>(e.Row, 0m);
			sender.SetDefaultExt<SOLine.orderQty>(e.Row);
			if (IsImport)
			{
				sender.SetDefaultExt<SOLine.salesPersonID>(e.Row);
				sender.SetDefaultExt<SOLine.reasonCode>(e.Row);
			}
			sender.SetDefaultExt<SOLine.salesAcctID>(e.Row);
			try
			{
				sender.SetDefaultExt<SOLine.salesSubID>(e.Row);
			}
			catch (PXSetPropertyException)
			{
				sender.SetValue<SOLine.salesSubID>(e.Row, null);
			}
			sender.SetDefaultExt<SOLine.tranDesc>(e.Row);
			sender.SetDefaultExt<SOLine.taxCategoryID>(e.Row);
			sender.SetDefaultExt<SOLine.vendorID>(e.Row);
			sender.SetDefaultExt<SOLine.curyUnitCost>(e.Row);
			sender.SetDefaultExt<SOLine.unitWeigth>(e.Row);
			sender.SetDefaultExt<SOLine.unitVolume>(e.Row);
			sender.SetDefaultExt<SOLine.pOSiteID>(e.Row);
			sender.SetDefaultExt<SOLine.completeQtyMin>(e.Row);
			sender.SetDefaultExt<SOLine.completeQtyMax>(e.Row);
		}

		protected virtual void SOLine_OrderQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			decimal? oldOrderQty = (decimal?)e.OldValue;

			if (row != null && row.Qty != oldOrderQty)
			{
				if (row.Qty == 0)
				{
					sender.SetValueExt<SOLine.curyDiscAmt>(row, decimal.Zero);
					sender.SetValueExt<SOLine.discPct>(row, decimal.Zero);
				}
				using (GetPriceCalculationScope().AppendContext<SOLine.orderQty>())
				sender.SetDefaultExt<SOLine.curyUnitPrice>(row);
			}
		}

		protected virtual void SOLine_ManualPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null)
				sender.SetDefaultExt<SOLine.curyUnitPrice>(row);
		}

		protected virtual void SOLine_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row == null) return;

			sender.SetDefaultExt<SOLine.salesPersonID>(e.Row);

			try
			{
				sender.SetDefaultExt<SOLine.salesSubID>(e.Row);
			}
			catch (PXSetPropertyException)
			{
				sender.SetValue<SOLine.salesSubID>(e.Row, null);
			}
		}

		public virtual bool RecalculatePriceAndDiscount()
		{
			//TODO based on setting. Default is True, If UnattendedMode & PrompUser - use Default.
			return true;
		}

		protected virtual void SOLine_OrderQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal?)e.NewValue < ((SOLine)e.Row).ClosedQty && ((SOLine)e.Row).RequireShipping == true && (((SOLine)e.Row).LineType == "GI" || ((SOLine)e.Row).LineType == "GN"))
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, sender.GetStateExt<SOLine.closedQty>(e.Row));
			}
		}

		protected virtual void SOLine_DiscPct_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
		}

		protected virtual void SOLine_DiscPct_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (row == null)
				return;

			sender.RaiseExceptionHandling<SOLine.discPct>(row, null, null);

			if (this.GetMinGrossProfitValidationOption(sender, row) == MinGrossProfitValidationType.None)
				return;

			var mgpc = new MinGrossProfitClass
			{
				DiscPct = (decimal?)e.NewValue,
				CuryDiscAmt = row.CuryDiscAmt,
				CuryUnitPrice = row.CuryUnitPrice
			};

			SOLineValidateMinGrossProfit(sender, row, mgpc);

			e.NewValue = mgpc.DiscPct;
		}

		protected virtual void SOLine_CuryDiscAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (row == null)
				return;

			sender.RaiseExceptionHandling<SOLine.curyDiscAmt>(row, null, null);

			if (this.GetMinGrossProfitValidationOption(sender, row) == MinGrossProfitValidationType.None)
				return;

			var mgpc = new MinGrossProfitClass
			{
				DiscPct = row.DiscPct,
				CuryDiscAmt = (decimal?)e.NewValue,
				CuryUnitPrice = row.CuryUnitPrice
			};

			SOLineValidateMinGrossProfit(sender, row, mgpc);

			e.NewValue = mgpc.CuryDiscAmt;
		}


		/// <summary>
		/// Checks if ManualPrice flag should be set automatically on import from Excel.
		/// This method is intended to be called from _FieldVerifying event handler.
		/// </summary>
		protected virtual bool IsManualPriceFlagNeeded(PXCache sender, SOLine row)
		{
			if (row != null && row.ManualPrice != true && sender.Graph.IsImportFromExcel)
			{
				decimal price;

				object curyUnitPrice = sender.GetValuePending<SOLine.curyUnitPrice>(row);
				object curyExtPrice = sender.GetValuePending<SOLine.curyExtPrice>(row);
				object manualPrice = sender.GetValuePending<SOLine.manualPrice>(row);

				if (((curyUnitPrice != PXCache.NotSetValue && curyUnitPrice != null && Decimal.TryParse(curyUnitPrice.ToString(), out price))
					|| (curyExtPrice != PXCache.NotSetValue && curyExtPrice != null && Decimal.TryParse(curyExtPrice.ToString(), out price)))
					&& (manualPrice == PXCache.NotSetValue || manualPrice == null))
				{
					return true;
				}
			}
			return false;
		}

		protected virtual void SOLine_CuryExtPrice_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (row == null)
				return;

			if (IsManualPriceFlagNeeded(sender, row))
				row.ManualPrice = true;
		}

		[Obsolete(Common.InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2)]
		protected virtual void SOLine_CuryLineAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (row == null)
				return;
		}

		protected virtual void SOLine_CuryUnitPrice_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (row == null)
				return;

			if (row.IsFree != true && this.GetMinGrossProfitValidationOption(sender, row) != MinGrossProfitValidationType.None)
			{
				sender.RaiseExceptionHandling<SOLine.curyUnitPrice>(row, null, null);

				var mgpc = new MinGrossProfitClass
				{
					DiscPct = row.DiscPct,
					CuryDiscAmt = row.CuryDiscAmt,
					CuryUnitPrice = (decimal?)e.NewValue
				};

				SOLineValidateMinGrossProfit(sender, row, mgpc);

				e.NewValue = mgpc.CuryUnitPrice;
			}

			if (IsManualPriceFlagNeeded(sender, row))
				row.ManualPrice = true;
		}

		protected virtual void SOLine_CuryUnitPrice_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = (SOLine)e.Row;
			if (row == null) return;

			bool isPriceUpdateNeeded;
			using (var priceScope = GetPriceCalculationScope())
				isPriceUpdateNeeded = priceScope.IsUpdateNeeded<SOLine.inventoryID>();

			if (row.TranType == INTranType.Transfer)
			{
				e.NewValue = 0m;
			}
			else if (row.InventoryID != null && row.ManualPrice != true && row.IsFree != true && !sender.Graph.IsCopyPasteContext
				&& isPriceUpdateNeeded)
			{
				string customerPriceClass = ARPriceClass.EmptyPriceClass;
				Location c = location.Select();
				if (!string.IsNullOrEmpty(c?.CPriceClassID))
					customerPriceClass = c.CPriceClassID;
				CurrencyInfo curyInfo = currencyinfo.Select();

				ARSalesPriceMaint salesPriceMaint = ARSalesPriceMaint.SingleARSalesPriceMaint;
				bool alwaysFromBaseCury = salesPriceMaint.GetAlwaysFromBaseCurrencySetting(sender);
				ARSalesPriceMaint.SalesPriceItem priceItem = null;
				try
				{
					priceItem = salesPriceMaint.FindSalesPrice(
						sender,
						customerPriceClass,
						row.CustomerID,
						row.InventoryID,
						row.SiteID,
						curyInfo.BaseCuryID,
						alwaysFromBaseCury ? curyInfo.BaseCuryID : curyInfo.CuryID,
						Math.Abs(row.Qty ?? 0m),
						row.UOM,
						Document.Current.OrderDate.Value);
				}
				catch (PXUnitConversionException)
				{
				}

				decimal? price = salesPriceMaint.AdjustSalesPrice(sender, priceItem, row.InventoryID, curyInfo, row.UOM);
				e.NewValue = price ?? 0m;

				ARSalesPriceMaint.CheckNewUnitPrice<SOLine, SOLine.curyUnitPrice>(sender, row, price);

				if (priceItem?.UOM != row.UOM || priceItem?.CuryID != Document.Current.CuryID)
				{
					priceItem = null;
				}
				row.PriceType = priceItem?.PriceType;
				row.IsPromotionalPrice = priceItem?.IsPromotionalPrice ?? false;
			}
			else
				e.NewValue = row.CuryUnitPrice ?? 0m;
		}

		protected virtual void SOLine_IsFree_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null)
			{
				if (row.IsFree == true)
				{
					sender.SetValueExt<SOLine.curyUnitPrice>(row, 0m);
					sender.SetValueExt<SOLine.discPct>(row, 0m);
					sender.SetValueExt<SOLine.curyDiscAmt>(row, 0m);
					if (e.ExternalCall)
						sender.SetValueExt<SOLine.manualDisc>(row, true);
				}
				else
				{
					sender.SetDefaultExt<SOLine.curyUnitPrice>(row);
				}
			}
		}

		protected virtual void SOLine_IsPromotionalPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row == null) return;

			bool raiseMinGrossProfitValidation = ((bool?)e.OldValue == true && row.IsPromotionalPrice == false);
			if (raiseMinGrossProfitValidation)
			{
				sender.SetValueExt<SOLine.curyUnitPrice>(e.Row, row.CuryUnitPrice);
				sender.SetValueExt<SOLine.discPct>(e.Row, row.DiscPct);
				sender.SetValueExt<SOLine.curyDiscAmt>(e.Row, row.CuryDiscAmt);
			}
		}

		protected virtual void SOLine_PriceType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row == null) return;

			bool raiseMinGrossProfitValidation = ((string)e.OldValue).IsIn(PriceTypes.Customer, PriceTypes.CustomerPriceClass)
				&& row.PriceType.IsNotIn(PriceTypes.Customer, PriceTypes.CustomerPriceClass);
			if (raiseMinGrossProfitValidation)
			{
				sender.SetValueExt<SOLine.curyUnitPrice>(e.Row, row.CuryUnitPrice);
				sender.SetValueExt<SOLine.discPct>(e.Row, row.DiscPct);
				sender.SetValueExt<SOLine.curyDiscAmt>(e.Row, row.CuryDiscAmt);
			}
		}

		protected virtual void SOLine_DiscountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (e.ExternalCall && row != null)
			{
				_discountEngine.UpdateManualLineDiscount(sender, Transactions, row, DiscountDetails, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current));
			}
		}

		protected virtual void SOLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOLine row = (SOLine)e.Row;
			if (row == null) return;
			bool lineTypeInventory = row.LineType == SOLineType.Inventory;
			bool isNonStockKit = (!row.IsStockItem ?? false) && (row.IsKit ?? false);
			PXUIFieldAttribute.SetEnabled<SOLine.subItemID>(sender, row, lineTypeInventory && !isNonStockKit);
			PXUIFieldAttribute.SetEnabled<SOLine.locationID>(sender, row, lineTypeInventory);

			bool enabled = (soordertype.Current.RequireShipping == true && row.TranType != INDocType.Undefined && row.Operation == SOOperation.Issue);
			PXUIFieldAttribute.SetEnabled<SOLine.pOCreate>(sender, row, enabled);

			if (enabled == false)
			{
				row.POCreate = false;
				row.POSource = null;
			}

			bool editable = false;
			if (row.POCreate == true)
			{
				ARTran tran = PXSelect<ARTran, Where<ARTran.sOOrderType, Equal<Required<ARTran.sOOrderType>>, And<ARTran.sOOrderNbr, Equal<Required<ARTran.sOOrderNbr>>, And<ARTran.sOOrderLineNbr, Equal<Required<ARTran.sOOrderLineNbr>>>>>>.SelectWindowed(this, 0, 1, row.OrderType, row.OrderNbr, row.LineNbr);
				editable = (tran == null);
			}

			PXUIFieldAttribute.SetEnabled<POLine3.selected>(posupply.Cache, null, enabled && editable);
			PXUIFieldAttribute.SetEnabled<SOLine.curyUnitPrice>(sender, row, row.IsFree != true);

			bool autoFreeItem = row.ManualDisc != true && row.IsFree == true;
			bool freeItem = row.IsFree == true;

			PXUIFieldAttribute.SetEnabled<SOLine.manualDisc>(sender, e.Row, !freeItem && (Document.Current != null && Document.Current.DisableAutomaticDiscountCalculation != true));
			PXUIFieldAttribute.SetEnabled<SOLine.orderQty>(sender, e.Row, !autoFreeItem);
			PXUIFieldAttribute.SetEnabled<SOLine.isFree>(sender, e.Row, !autoFreeItem && row.InventoryID != null);
			PXUIFieldAttribute.SetEnabled<SOLine.pOSource>(sender, e.Row, enabled && row.POCreated != true && row.POCreate == true && (row.POSource != INReplenishmentSource.PurchaseToOrder || row.ShippedQty == 0m));

			bool? Completed = ((SOLine)e.Row).Completed;

			if (((SOLine)e.Row).ShippedQty > 0m)
			{
				PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLine.tranDesc>(sender, e.Row);
				PXUIFieldAttribute.SetEnabled<SOLine.orderQty>(sender, e.Row, Completed == false);
				PXUIFieldAttribute.SetEnabled<SOLine.shipComplete>(sender, e.Row, Completed == false);
				PXUIFieldAttribute.SetEnabled<SOLine.completeQtyMin>(sender, e.Row, Completed == false);
				PXUIFieldAttribute.SetEnabled<SOLine.completeQtyMax>(sender, e.Row, Completed == false);
				PXUIFieldAttribute.SetEnabled<SOLine.completed>(sender, e.Row, true);
				PXUIFieldAttribute.SetEnabled<SOLine.pOCreate>(sender, row, enabled);
			}

			Transactions.Cache.Adjust<PXUIFieldAttribute>(row)
				.For<SOLine.vendorID>(a => a.Enabled = row.POCreate == true)
				.SameFor<SOLine.pOSiteID>();

			SOLine line = (SOLine)e.Row;
			if (line != null && line.Operation == SOOperation.Issue)
			{
				PXUIFieldAttribute.SetEnabled<SOLine.autoCreateIssueLine>(sender, e.Row, false);
			}

			splits.Cache.AllowInsert = Transactions.Cache.AllowInsert && Completed != true;
			splits.Cache.AllowUpdate = Transactions.Cache.AllowUpdate && Completed != true;
			splits.Cache.AllowDelete = Transactions.Cache.AllowDelete && Completed != true;

			// TODO: Low Priority refactor to SOLineSplit
			//else if (((SOLine)e.Row).PONbr != null)
			//{
			//    PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
			//    PXUIFieldAttribute.SetEnabled<SOLine.tranDesc>(sender, e.Row);
			//    PXUIFieldAttribute.SetEnabled<SOLine.manualDisc>(sender, e.Row, !autoFreeItem);
			//    PXUIFieldAttribute.SetEnabled<SOLine.orderQty>(sender, e.Row, !autoFreeItem);
			//    PXUIFieldAttribute.SetEnabled<SOLine.isFree>(sender, e.Row, !autoFreeItem);
			//    PXUIFieldAttribute.SetEnabled<SOLine.discPct>(sender, e.Row, !autoFreeItem);
			//    PXUIFieldAttribute.SetEnabled<SOLine.curyDiscAmt>(sender, e.Row, !autoFreeItem);
			//    PXUIFieldAttribute.SetEnabled<SOLine.curyLineAmt>(sender, e.Row, !autoFreeItem);

			//    PXUIFieldAttribute.SetEnabled<SOLine.curyUnitPrice>(sender, row, row.IsFree != true);
			//    PXUIFieldAttribute.SetEnabled<SOLine.pOCreate>(sender, row, enabled);
			//    PXUIFieldAttribute.SetEnabled<SOLine.orderQty>(sender, e.Row, true);
			//    PXUIFieldAttribute.SetEnabled<SOLine.shipComplete>(sender, e.Row, true);
			//    PXUIFieldAttribute.SetEnabled<SOLine.completeQtyMin>(sender, e.Row, true);
			//    PXUIFieldAttribute.SetEnabled<SOLine.completeQtyMax>(sender, e.Row, true);
			//    PXUIFieldAttribute.SetEnabled<SOLine.shipDate>(sender, e.Row, this.Document.Current.ShipComplete == SOShipComplete.BackOrderAllowed);
			//    PXUIFieldAttribute.SetEnabled<SOLine.requestDate>(sender, e.Row, true);
			//}

			SOOrder header = Document.Current;

			if (header != null)
			{
				PXUIFieldAttribute.SetEnabled<SOLine.shipDate>(sender, row, header.ShipComplete == SOShipComplete.BackOrderAllowed);

				if (header.Hold != true && header.DontApprove != true)
				{
					PXUIFieldAttribute.SetEnabled(sender, row, false);
				}
			}

			POCreateVerifyValue(sender, row, row.POCreate);

			if (row != null && row.CuryUnitPrice != null && row.DiscPct != null && row.DiscAmt != null)
			{
				SOLineValidateMinGrossProfit(sender, row, new MinGrossProfitClass { CuryDiscAmt = row.CuryDiscAmt, CuryUnitPrice = row.CuryUnitPrice, DiscPct = row.DiscPct });
			}

			if (row.InvoiceNbr != null)
			{
				PXUIFieldAttribute.SetEnabled<SOLine.operation>(sender, row, false);
			}
		}

		protected virtual void SOLine_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row == null)
				return;

			if (Document.Current != null && Document.Current.DisableAutomaticDiscountCalculation == true && row.IsFree != true)
				row.ManualDisc = true;

			if (sender.Graph.IsCopyPasteContext)
			{
				if (row.RequireLocation == false)
					row.LocationID = null;
				if (row.ManualDisc != true && row.IsFree == true)
				{
					ResetQtyOnFreeItem(sender, row);
					_discountEngine.ClearDiscount(sender, row);
				}

				RecalculateDiscounts(sender, row);

				if (row.ManualDisc != true)
				{
					var discountCode = (ARDiscount)PXSelectorAttribute.Select<SOLine.discountID>(sender, row);
					row.DiscPctDR = (discountCode != null && discountCode.IsAppliedToDR == true) ? row.DiscPct : 0.0m;
				}

				row.ManualPrice = true;

				TaxAttribute.Calculate<SOLine.taxCategoryID>(sender, e);

				DirtyFormulaAttribute.RaiseRowUpdated<SOLine.openLine>(sender, new PXRowUpdatedEventArgs(e.Row, new SOLine(), e.ExternalCall));
			}
			else
			{
				if (!this.IsImportFromExcel)
				{
					RecalculateDiscounts(sender, (SOLine)e.Row);
				}

				TaxAttribute.Calculate<SOLine.taxCategoryID>(sender, e);
			}
		}

		protected virtual void SOLine_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation.Command().IsNotIn(PXDBOperation.Insert, PXDBOperation.Update))
				return;

			var line = (SOLine)e.Row;

				PXDefaultAttribute.SetPersistingCheck<SOLine.salesAcctID>(sender, e.Row,
						soordertype.Current == null || Document.Current == null ||
						soordertype.Current.ARDocType == ARDocType.NoUpdate ||
						Document.Current.Hold == true
						? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

				PXDefaultAttribute.SetPersistingCheck<SOLine.salesSubID>(sender, e.Row,
						soordertype.Current == null || Document.Current == null ||
						soordertype.Current.ARDocType == ARDocType.NoUpdate ||
						Document.Current.Hold == true
						? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

				PXDefaultAttribute.SetPersistingCheck<SOLine.subItemID>(sender, e.Row,
					soordertype.Current == null || soordertype.Current.RequireLocation == true || line.LineType != SOLineType.Inventory || ((!line.IsStockItem ?? false) && (line.IsKit ?? false))
					? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

				PXDefaultAttribute.SetPersistingCheck<SOLine.reasonCode>(sender, e.Row,
					line.RequireReasonCode == true
					? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

				PXDefaultAttribute.SetPersistingCheck<SOLine.pOSource>(sender, e.Row, line.POCreate == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<SOLine.taskID>(sender, e.Row, ProjectDefaultAttribute.IsProject(this, ((SOLine)e.Row).ProjectID) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

				lsselect.MemoAvailabilityCheck(sender, line, true);
		}

		protected virtual void SOLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			SOLine oldRow = e.OldRow as SOLine;

			if (row != null && row.RequireLocation == false)
				row.LocationID = null;

			if (!sender.ObjectsEqual<SOLine.branchID>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.customerID>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.inventoryID>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<SOLine.siteID>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.baseOrderQty>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.isFree>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<SOLine.curyUnitPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.curyExtPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.curyLineAmt>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<SOLine.curyDiscAmt>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.discPct>(e.Row, e.OldRow) ||
					!sender.ObjectsEqual<SOLine.manualDisc>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.discountID>(e.Row, e.OldRow))
				if (row.ManualDisc != true)
				{
					if (oldRow.ManualDisc == true)//Manual Discount Unckecked
					{
						if (row.IsFree == true)
						{
							ResetQtyOnFreeItem(sender, row);
						}
					}

					if (row.IsFree == true)
					{
						_discountEngine.ClearDiscount(sender, row);
					}

					RecalculateDiscounts(sender, row);
				}
				else
				{
					RecalculateDiscounts(sender, row, oldRow);
				}

			if (row.ManualDisc != true)
			{
				var discountCode = (ARDiscount)PXSelectorAttribute.Select<SOLine.discountID>(sender, row);
				row.DiscPctDR = (discountCode != null && discountCode.IsAppliedToDR == true) ? row.DiscPct : 0.0m;
			}

			if ((e.ExternalCall || sender.Graph.IsImport)
				&& sender.ObjectsEqual<SOLine.customerID>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<SOLine.inventoryID>(e.Row, e.OldRow) && sender.ObjectsEqual<SOLine.uOM>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<SOLine.orderQty>(e.Row, e.OldRow) && sender.ObjectsEqual<SOLine.branchID>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<SOLine.siteID>(e.Row, e.OldRow)
				&& sender.ObjectsEqual<SOLine.manualPrice>(e.Row, e.OldRow)
				&& (!sender.ObjectsEqual<SOLine.curyUnitPrice>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOLine.curyExtPrice>(e.Row, e.OldRow)))
			{
				row.ManualPrice = true;
				sender.SetValueExt<SOLine.priceType>(row, null);
				sender.SetValueExt<SOLine.isPromotionalPrice>(row, false);
			}

			if (row.ManualPrice != true)
			{
				row.CuryUnitPriceDR = row.CuryUnitPrice;
			}

			TaxAttribute.Calculate<SOLine.taxCategoryID>(sender, e);

			DirtyFormulaAttribute.RaiseRowUpdated<SOLine.openLine>(sender, e);

			if ((e.ExternalCall || sender.Graph.IsImport)
			   && !sender.ObjectsEqual<SOLine.completed>(e.Row, e.OldRow) && ((SOLine)e.Row).Completed != true && ((SOLine)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed)
			{
				foreach (SOLineSplit split in PXParentAttribute.SelectChildren(splits.Cache, e.Row, typeof(SOLine)))
				{
					if (split.ShipmentNbr != null || split.ShippedQty > 0m)
						sender.SetValueExt<SOLine.shipComplete>(e.Row, SOShipComplete.BackOrderAllowed);
				}
			}

			if (sender.Graph.IsMobile)
			{
				var cur = sender.Locate(e.Row);
				sender.Current = cur;
			}
		}

		protected virtual void SOLine_RowDeleting(PXCache sedner, PXRowDeletingEventArgs e)
		{
			SOLine row = e.Row as SOLine;
			if (row != null && (row.ShippedQty > 0 || splits.Select().AsEnumerable().Where(x => ((SOLineSplit)x).ShipmentNbr != null).Count() > 0))
				throw new PXException(Messages.ShippedLineDeleting);
		}

		protected virtual void SOLineSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			// Please check whether the new logic should be located in LSSOLine.SOLineSplit_RowSelected() instead of this handler.
			SOLineSplit row = (SOLineSplit)e.Row;
			if (row == null)
				return;

			if (row.POCancelled == true && row.Completed == true && row.PONbr != null)
			{
				splits.Cache.RaiseExceptionHandling<SOLineSplit.refNoteID>(row, null, new PXSetPropertyException(Messages.POLinkedToSOCancelled, PXErrorLevel.RowWarning, row.PONbr));
			}
		}

		protected virtual void SOLineSplit_LotSerialNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOLineSplit row = (SOLineSplit)e.Row;
			if (row == null) return;

			if (soordertype.Current?.RequireShipping == true && soordertype.Current.INDocType != INTranType.NoUpdate
				&& row.Operation == SOOperation.Issue && row.LotSerialNbr != null)
			{
				var item = InventoryItem.PK.Find(this, row.InventoryID);
				var lotserialclass = INLotSerClass.PK.Find(this, item?.LotSerClassID);
				if (lotserialclass != null && lotserialclass.LotSerAssign != INLotSerAssign.WhenReceived)
				{
					splits.Cache.RaiseExceptionHandling<SOLineSplit.lotSerialNbr>(row, null,
						new PXSetPropertyException(Messages.LotSerialSelectionForOnReceiptOnly, PXErrorLevel.Warning));
					row.LotSerialNbr = null;
				}
			}
		}

		protected virtual void SOLineSplit_RowDeleting(PXCache sedner, PXRowDeletingEventArgs e)
		{
			SOLineSplit row = e.Row as SOLineSplit;
			if (row != null && (row.ShippedQty > 0 || row.ShipmentNbr != null))
				throw new PXException(Messages.ShippedLineDeleting);
		}


		[PXDBString(15, IsUnicode = true, BqlField = typeof(SOLineSplit.sOOrderNbr))]
		[PXDBDefault(typeof(SOOrder.orderNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void SOLineSplit2_SOOrderNbr_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[SOLineSplitPlanID(typeof(SOOrder.noteID), typeof(SOOrder.hold), typeof(SOOrder.orderDate))]
		protected virtual void SOLineSplit_PlanID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void SOLineSplit_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (!IsTransferOrder) return;

			foreach (PXResult<SOLineSplit2, INItemPlan> r in this.sodemand.View.SelectMultiBound(new object[] { e.Row }))
			{
				SOLineSplit2 upd = PXCache<SOLineSplit2>.CreateCopy(r);

				upd.SOOrderType = null;
				upd.SOOrderNbr = null;
				upd.SOLineNbr = null;
				upd.SOSplitLineNbr = null;
				upd.SiteID = upd.ToSiteID;
				upd.IsAllocated = false;
				upd.RefNoteID = null;

				this.sodemand.Update(upd);

				INItemPlan plan = r;
				if (plan.PlanType != null)
				{
					var op = SOOrderTypeOperation.PK.Find(this, upd.OrderType, upd.Operation);
					if (op != null && op.OrderPlanType != null)
						plan.PlanType = op.OrderPlanType;
					plan.SupplyPlanID = null;
					plan.SiteID = upd.ToSiteID;
					plan.FixedSource = null;

					sender.Graph.Caches[typeof(INItemPlan)].Update(plan);
				}
			}

			var split = (SOLineSplit)e.Row;
			if (split?.PlanID != null)
			{
				INItemPlan orphanedPlan = PXSelectReadonly<INItemPlan,
					Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>,
						And<INItemPlan.planType, Equal<INPlanConstants.plan94>>>>
					.Select(this, split.PlanID);
				if (orphanedPlan != null)
				{
					Caches[typeof(INItemPlan)].Delete(orphanedPlan);
				}
			}
		}

		protected virtual void SOLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			SOLine row = e.Row as SOLine;

			if (Document.Current != null && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Deleted && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.InsertedDeleted && !(row.IsFree == true && row.ManualDisc == false))
			{
				if (!DisableGroupDocDiscount)
				{
					_discountEngine.RecalculateGroupAndDocumentDiscounts(sender, Transactions, null, DiscountDetails, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current));
				}
				RecalculateTotalDiscount();
				RefreshFreeItemLines(sender);
			}

			if (Document.Current != null)
			{
				Document.Cache.MarkUpdated(Document.Current);
			}
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R1)]
		public virtual void ClearReplenishmentPlans(SOLine row)
		{
			foreach (PXResult<INReplenishmentLine, INItemPlan> r in replenishment.View.SelectMultiBound(new object[] { row }))
			{
				INReplenishmentLine line = r;
				INItemPlan plan = r;
				replenishment.Delete(line);
				this.Caches[typeof(INItemPlan)].Delete(plan);
			}
		}

		protected virtual void SOLine_AvgCost_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				decimal? AvgCost = (decimal?)sender.GetValue<SOLine.avgCost>(e.Row);

				if (AvgCost != null)
				{
					AvgCost = INUnitAttribute.ConvertToBase<SOLine.inventoryID, SOLine.uOM>(sender, e.Row, (decimal)AvgCost, INPrecision.UNITCOST);

					if (!sender.Graph.Accessinfo.CuryViewState)
					{
						decimal CuryAvgCost;
						PXCurrencyAttribute.CuryConvCury(sender, e.Row, (decimal)AvgCost, out CuryAvgCost, CommonSetupDecPl.PrcCst);
						e.ReturnValue = CuryAvgCost;
					}
					else
					{
						e.ReturnValue = AvgCost;
					}
				}
			}
		}

		public class MinGrossProfitClass
		{
			public decimal? CuryUnitPrice { get; set; }
			public decimal? CuryDiscAmt { get; set; }
			public decimal? DiscPct { get; set; }
			public MinGrossProfitClass() { }
		}

		protected virtual void SOLineValidateMinGrossProfit(PXCache sender, SOLine row, MinGrossProfitClass mgpc)
		{
			if (row == null) return;

			string minGrossProfitValidation = this.GetMinGrossProfitValidationOption(sender, row);
			if (minGrossProfitValidation == MinGrossProfitValidationType.None)
				return;

			if (row.IsFree == true || sender.Graph.UnattendedMode)
				return;

			if (IsTransferOrder || row.Operation == SOOperation.Receipt)
				return;

			if (row.InventoryID != null && row.UOM != null && mgpc.CuryUnitPrice >= 0)
			{
				InventoryItem inItem = InventoryItem.PK.Find(this, row.InventoryID);
				INItemCost itemCost = initemcost.Select(row.InventoryID);

				mgpc.CuryUnitPrice = MinGrossProfitValidator<SOLine>.ValidateUnitPrice<SOLine.curyInfoID, SOLine.inventoryID, SOLine.uOM>(sender, row, inItem, itemCost, mgpc.CuryUnitPrice, minGrossProfitValidation);

				if (mgpc.DiscPct != 0)
				{
					mgpc.DiscPct = MinGrossProfitValidator<SOLine>.ValidateDiscountPct<SOLine.inventoryID, SOLine.uOM>(sender, row, inItem, itemCost, row.UnitPrice, mgpc.DiscPct, minGrossProfitValidation);
				}

				if (mgpc.CuryDiscAmt != 0 && row.Qty != null && Math.Abs(row.Qty.GetValueOrDefault()) != 0)
				{
					mgpc.CuryDiscAmt = MinGrossProfitValidator<SOLine>.ValidateDiscountAmt<SOLine.inventoryID, SOLine.uOM>(sender, row, inItem, itemCost, row.UnitPrice, mgpc.CuryDiscAmt, minGrossProfitValidation);
				}
				}
			}

		public virtual string GetMinGrossProfitValidationOption(PXCache sender, SOLine row)
		{
			if (row.IsPromotionalPrice == true && sosetup.Current.IgnoreMinGrossProfitPromotionalPrice == true
				|| row.PriceType == PriceTypes.Customer && sosetup.Current.IgnoreMinGrossProfitCustomerPrice == true
				|| row.PriceType == PriceTypes.CustomerPriceClass && sosetup.Current.IgnoreMinGrossProfitCustomerPriceClass == true)
			{
				return MinGrossProfitValidationType.None;
			}
			return sosetup.Current.MinGrossProfitValidation;
		}

		#endregion


		#region SOOrderDiscountDetail events

		protected virtual void SOOrderDiscountDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOOrderDiscountDetail discountDetail = (SOOrderDiscountDetail)e.Row;
			if (discountDetail == null) return;

			if (Document != null && Document.Current != null)
				Document.Cache.SetValueExt<SOOrder.curyDocDisc>(Document.Current, _discountEngine.GetTotalGroupAndDocumentDiscount(DiscountDetails, true));
		}

		protected virtual void SOOrderDiscountDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			SOOrderDiscountDetail discountDetail = (SOOrderDiscountDetail)e.Row;
			if (!_discountEngine.IsInternalDiscountEngineCall && discountDetail != null)
			{
				if (discountDetail.DiscountID != null)
			{
					_discountEngine.InsertManualDocGroupDiscount(Transactions.Cache, Transactions, DiscountDetails, discountDetail, discountDetail.DiscountID, null, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current));
				RefreshTotalsAndFreeItems(sender);
			}

				if (_discountEngine.SetExternalManualDocDiscount(Transactions.Cache, Transactions, DiscountDetails, discountDetail, null, GetDefaultSODiscountCalculationOptions(Document.Current)))
					RecalculateTotalDiscount();
			}

			if (discountDetail != null && discountDetail.DiscountID != null && discountDetail.DiscountSequenceID != null && discountDetail.Description == null)
			{
				object description = null;
				sender.RaiseFieldDefaulting<SOOrderDiscountDetail.description>(discountDetail, out description);
				sender.SetValue<SOOrderDiscountDetail.description>(discountDetail, description);
			}
		}

		protected virtual void SOOrderDiscountDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOOrderDiscountDetail discountDetail = (SOOrderDiscountDetail)e.Row;
			SOOrderDiscountDetail oldDiscountDetail = (SOOrderDiscountDetail)e.OldRow;
			if (!_discountEngine.IsInternalDiscountEngineCall && discountDetail != null)
			{
				if (!sender.ObjectsEqual<SOOrderDiscountDetail.skipDiscount>(e.Row, e.OldRow))
				{
					_discountEngine.UpdateDocumentDiscount(Transactions.Cache, Transactions, DiscountDetails, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.OrderDate, discountDetail.Type != DiscountType.Document, GetDefaultSODiscountCalculationOptions(Document.Current));
					RefreshTotalsAndFreeItems(sender);
				}
				if (!sender.ObjectsEqual<SOOrderDiscountDetail.discountID>(e.Row, e.OldRow) || !sender.ObjectsEqual<SOOrderDiscountDetail.discountSequenceID>(e.Row, e.OldRow))
				{
					_discountEngine.UpdateManualDocGroupDiscount(Transactions.Cache, Transactions, DiscountDetails, discountDetail, discountDetail.DiscountID, sender.ObjectsEqual<SOOrderDiscountDetail.discountID>(e.Row, e.OldRow) ? discountDetail.DiscountSequenceID : null, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.OrderDate, GetDefaultSODiscountCalculationOptions(Document.Current));
					RefreshTotalsAndFreeItems(sender);
				}

				if (_discountEngine.SetExternalManualDocDiscount(Transactions.Cache, Transactions, DiscountDetails, discountDetail, oldDiscountDetail, GetDefaultSODiscountCalculationOptions(Document.Current)))
					RecalculateTotalDiscount();
			}
		}

		protected virtual void SOOrderDiscountDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			SOOrderDiscountDetail discountDetail = (SOOrderDiscountDetail)e.Row;
			if (!_discountEngine.IsInternalDiscountEngineCall && discountDetail != null && !DisableGroupDocDiscount)
			{
				_discountEngine.UpdateDocumentDiscount(Transactions.Cache, Transactions, DiscountDetails, Document.Current.BranchID, Document.Current.CustomerLocationID, Document.Current.OrderDate, (discountDetail.Type != null && discountDetail.Type != DiscountType.Document && discountDetail.Type != DiscountType.ExternalDocument), GetDefaultSODiscountCalculationOptions(Document.Current));
			}
			if (_discountEngine.IsInternalDiscountEngineCall && Document.Current != null && Document.Current.DisableAutomaticDiscountCalculation == true)
			{
				Document.Cache.RaiseExceptionHandling<SOOrder.disableAutomaticDiscountCalculation>(Document.Current, Document.Current.DisableAutomaticDiscountCalculation, new PXSetPropertyException(Messages.OneOrMoreDiscountsDeleted, PXErrorLevel.Warning));
			}
			RefreshTotalsAndFreeItems(sender);
		}

		protected virtual void SOOrderDiscountDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOOrderDiscountDetail discountDetail = (SOOrderDiscountDetail)e.Row;

			bool isExternalDiscount = discountDetail.Type == DiscountType.ExternalDocument;

			PXDefaultAttribute.SetPersistingCheck<SOOrderDiscountDetail.discountID>(sender, discountDetail, isExternalDiscount ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
			PXDefaultAttribute.SetPersistingCheck<SOOrderDiscountDetail.discountSequenceID>(sender, discountDetail, isExternalDiscount ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
		}
		#endregion

		[PXDBString(10, IsUnicode = true)]
		[PXDBDefault(typeof(SOOrder.taxZoneID))]
		[PXUIFieldAttribute(DisplayName = "Customer Tax Zone", Enabled = false)]
		protected virtual void SOTaxTran_TaxZoneID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void SOTaxTran_TaxZoneID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			Exception ex = e.Exception as PXSetPropertyException;
			if (ex != null)
			{
				Document.Cache.RaiseExceptionHandling<SOOrder.taxZoneID>(Document.Current, null, ex);
			}
		}

		protected virtual void SOTaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (!(e.Row is SOTaxTran soTaxTran))
				return;

			PXUIFieldAttribute.SetEnabled<SOTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);			
		}

		protected virtual void CustomerPaymentMethodInputMode_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CustomerPaymentMethodInputMode inputMode = e.Row as CustomerPaymentMethodInputMode;
			if (inputMode == null) return;
			if (DefPaymentMethodInstance.Current != null)
			{
				bool? isVisible = null;
				foreach (PXEventSubscriberAttribute attribute in cache.GetAttributesReadonly<CustomerPaymentMethodInputMode.inputMode>(inputMode))
				{
					if (attribute is PXUIFieldAttribute)
					{
						isVisible = ((PXUIFieldAttribute)attribute).Visible;
					}
				}
				if (isVisible == false)
				{
					int? pmInstanceId = DefPaymentMethodInstance.Current.PMInstanceID;
					bool isTokenized = CCProcessingHelper.IsTokenizedPaymentMethod(this, pmInstanceId);
					bool isHFPM = CCProcessingHelper.IsHFPaymentMethod(this, pmInstanceId);
					inputMode.InputMode = isHFPM ? InputModeType.Token : InputModeType.Details;
				}
			}
		}

		protected virtual void SOShippingAddress_PostalCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SOShippingAddress row = e.Row as SOShippingAddress;
			if (row != null)
			{
				Document.Cache.SetDefaultExt<SOOrder.taxZoneID>(Document.Current);
			}
		}

		protected virtual void AddInvoiceFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			invoicesplits.Cache.Clear();
		}


		#region SOPackageInfo
		protected virtual void SOPackageInfoEx_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOPackageInfo row = e.Row as SOPackageInfo;
			if (row != null && Document.Current != null)
			{
				row.WeightUOM = commonsetup.Current.WeightUOM;
				PXUIFieldAttribute.SetEnabled<SOPackageInfo.inventoryID>(sender, e.Row, Document.Current.IsManualPackage == true);
				PXUIFieldAttribute.SetEnabled<SOPackageInfo.siteID>(sender, e.Row, Document.Current.IsManualPackage == true);
			}
		}

		protected virtual void SOPackageInfoEx_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SOPackageInfo row = e.Row as SOPackageInfo;
			if (row != null)
			{
				PXDefaultAttribute.SetPersistingCheck<SOPackageInfo.siteID>(sender, row, PXAccess.FeatureInstalled<FeaturesSet.inventory>() ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}
		protected virtual void SOPackageInfoEx_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			SOPackageInfo row = e.Row as SOPackageInfo;
			if (row != null)
			{
				CSBox box = PXSelect<CSBox, Where<CSBox.boxID, Equal<Required<CSBox.boxID>>>>.Select(this, row.BoxID);
				if (box != null && box.MaxWeight < row.GrossWeight)
				{
					sender.RaiseExceptionHandling<SOPackageInfo.grossWeight>(row, row.GrossWeight, new PXSetPropertyException(Messages.WeightExceedsBoxSpecs));
				}
			}
		}
		#endregion

		protected readonly string viewInventoryID;

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName.ToLower() == "document" && values != null)
			{
				if (IsImport || IsExport || IsMobile || IsContractBasedAPI)
				{
					Document.Cache.Locate(keys);
					if (values.Contains("Hold") && values["Hold"] != PXCache.NotSetValue && values["Hold"] != null)
					{
						var hold = Document.Current.Hold ?? false;
						if (Convert.ToBoolean(values["Hold"]) != hold)
						{
							((PXAction<SOOrder>)this.Actions["Hold"]).PressImpl(false);
						}
					}

					if (values.Contains("CreditHold") && values["CreditHold"] != PXCache.NotSetValue && values["CreditHold"] != null)
					{
						var creditHold = Document.Current.CreditHold ?? false;
						if (Convert.ToBoolean(values["CreditHold"]) != creditHold)
						{
							((PXAction<SOOrder>)this.Actions["CreditHold"]).PressImpl(false);
						}
					}
				}

				values["Hold"] = PXCache.NotSetValue;
				values["CreditHold"] = PXCache.NotSetValue;
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		#region Discount

		protected virtual void RecalculateDiscounts(PXCache sender, SOLine line)
		{
			RecalculateDiscounts(sender, line, null);
		}

		protected virtual void RecalculateDiscounts(PXCache sender, SOLine line, SOLine oldline)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() && 
			    (line.InventoryID != null || (sender.Graph.IsImportFromExcel && line.SkipDisc != true)) && line.Qty != null && line.CuryLineAmt != null && 
			    (line.IsFree != true || (oldline != null && !sender.ObjectsEqual<SOLine.isFree>(line, oldline))))
			{
				DiscountEngine.DiscountCalculationOptions discountCalculationOptions = GetDefaultSODiscountCalculationOptions(Document.Current);
				if (line.CalculateDiscountsOnImport == true)
					discountCalculationOptions = discountCalculationOptions | DiscountEngine.DiscountCalculationOptions.CalculateDiscountsFromImport;

				_discountEngine.SetDiscounts(
					sender, 
					Transactions, 
					line, 
					DiscountDetails, 
					Document.Current.BranchID, 
					Document.Current.CustomerLocationID, 
					Document.Current.CuryID, 
					Document.Current.OrderDate,
					recalcdiscountsfilter.Current,
					discountCalculationOptions);

				RecalculateTotalDiscount();

				RefreshFreeItemLines(sender);
			}
			else if (!PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() && Document.Current != null)
			{
				_discountEngine.CalculateDocumentDiscountRate(Transactions.Cache, Transactions, line, DiscountDetails);
			}

		}

		public virtual DiscountEngine.DiscountCalculationOptions GetDefaultSODiscountCalculationOptions(SOOrder doc)
		{
			return DiscountEngine.DefaultARDiscountCalculationParameters | (DisableGroupDocDiscount ? DiscountEngine.DiscountCalculationOptions.DisableGroupAndDocumentDiscounts : DiscountEngine.DiscountCalculationOptions.CalculateAll) |
					((doc != null && doc.DisableAutomaticDiscountCalculation == true) ? DiscountEngine.DiscountCalculationOptions.DisableAllAutomaticDiscounts : DiscountEngine.DiscountCalculationOptions.CalculateAll);
		}

		protected virtual void RefreshFreeItemLines(PXCache sender)
		{
			if (sender.Graph.IsCopyPasteContext || sender.Graph.IsImportFromExcel)
				return;

			Dictionary<int, decimal> groupedByInventory = new Dictionary<int, decimal>();
			Dictionary<int, string> baseUomOfInventories = new Dictionary<Int32, String>();

			PXSelectBase<SOOrderDiscountDetail> select =
				new PXSelectJoin<SOOrderDiscountDetail,
				InnerJoin<InventoryItem, On<SOOrderDiscountDetail.FK.FreeItem>>,
				Where<SOOrderDiscountDetail.orderType, Equal<Current<SOOrder.orderType>>,
				And<SOOrderDiscountDetail.orderNbr, Equal<Current<SOOrder.orderNbr>>,
				And<SOOrderDiscountDetail.skipDiscount, NotEqual<boolTrue>>>>>(this);

			foreach (PXResult<SOOrderDiscountDetail, InventoryItem> row in select.Select())
			{
				SOOrderDiscountDetail discountDetail = row;
				InventoryItem item = row;

				if (discountDetail.FreeItemID != null)
				{
					if (groupedByInventory.ContainsKey(discountDetail.FreeItemID.Value))
					{
						groupedByInventory[discountDetail.FreeItemID.Value] += discountDetail.FreeItemQty ?? 0;
					}
					else
					{
						groupedByInventory.Add(discountDetail.FreeItemID.Value, discountDetail.FreeItemQty ?? 0);
						baseUomOfInventories.Add(item.InventoryID.Value, item.BaseUnit);
					}

				}

			}

			bool refreshView = false;

			#region Delete Unvalid FreeItems
			foreach (SOLine line in FreeItems.Select())
			{
				if (line.ManualDisc == false && line.InventoryID != null)
				{
					if (line.ShippedQty == 0m)
					{
						if (groupedByInventory.ContainsKey(line.InventoryID.Value))
						{
							if (groupedByInventory[line.InventoryID.Value] == 0)
							{
								FreeItems.Delete(line);
								refreshView = true;
							}
						}
						else
						{
							FreeItems.Delete(line);
							refreshView = true;
						}
					}
					else
					{
						PXUIFieldAttribute.SetWarning<SOLine.orderQty>(FreeItems.Cache, line, Messages.CannotRecalculateFreeItemQuantity);
						refreshView = true;
					}
				}
			}

			#endregion

			int? defaultWarehouse = GetDefaultWarehouse();
			foreach (KeyValuePair<int, decimal> kv in groupedByInventory)
			{
				SOLine currentLine = this.Transactions.Current;
				SOLine freeLine = GetFreeLineByItemID(kv.Key);

				if (freeLine == null)
				{
					if (kv.Value > 0)
					{
						SOLine line = new SOLine();
						line.InventoryID = kv.Key;
						line.IsFree = true;
						line.SiteID = defaultWarehouse;
						line.OrderQty = kv.Value;

						if (arsetup.Current.ApplyQuantityDiscountBy == ApplyQuantityDiscountType.BaseUOM)
							line.UOM = baseUomOfInventories[line.InventoryID.Value];

						line = FreeItems.Insert(line);

						refreshView = true;
					}
				}
				else
				{
					if (freeLine.ShippedQty == 0m)
					{
						if (freeLine.OrderQty != kv.Value)
						{
							SOLine copy = PXCache<SOLine>.CreateCopy(freeLine);
							copy.OrderQty = kv.Value;
							FreeItems.Cache.Update(copy);

							refreshView = true;
						}
					}
					else
					{
						PXUIFieldAttribute.SetWarning<SOLine.orderQty>(FreeItems.Cache, freeLine, Messages.CannotRecalculateFreeItemQuantity);
						refreshView = true;
					}
				}
				if (currentLine != null && currentLine != this.Transactions.Current)
				{
					this.Transactions.Current = currentLine;
				}
			}

			if (refreshView)
			{
				Transactions.View.RequestRefresh();
			}
		}

		private SOLine GetFreeLineByItemID(int? inventoryID)
		{
			return PXSelect<SOLine,
				Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>,
				And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>,
				And<SOLine.isFree, Equal<boolTrue>,
				And<SOLine.inventoryID, Equal<Required<SOLine.inventoryID>>,
				And<SOLine.manualDisc, Equal<boolFalse>>>>>>>.Select(this, inventoryID);
		}

		private void ResetQtyOnFreeItem(PXCache sender, SOLine line)
		{
			PXSelectBase<SOOrderDiscountDetail> select = new PXSelect<SOOrderDiscountDetail,
				Where<SOOrderDiscountDetail.orderType, Equal<Current<SOOrder.orderType>>,
				And<SOOrderDiscountDetail.orderNbr, Equal<Current<SOOrder.orderNbr>>,
				And<SOOrderDiscountDetail.freeItemID, Equal<Required<SOOrderDiscountDetail.freeItemID>>>>>>(this);

			decimal? qtyTotal = 0;
			foreach (SOOrderDiscountDetail item in select.Select(line.InventoryID))
			{
				if (item.SkipDiscount != true && item.FreeItemID != null && item.FreeItemQty != null && item.FreeItemQty.Value > 0)
				{
					qtyTotal += item.FreeItemQty.Value;
				}
			}

			sender.SetValueExt<SOLine.orderQty>(line, qtyTotal);
		}

		/// <summary>
		/// If all lines are from one site/warehouse - return this warehouse otherwise null;
		/// </summary>
		/// <returns>Default Wartehouse for Free Item</returns>
		private int? GetDefaultWarehouse()
		{
			PXResultset<SOOrderSite> osites = PXSelectJoin<SOOrderSite,
				InnerJoin<INSite, 
					On<SOOrderSite.FK.Site>>,
				Where<SOOrderSite.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOOrderSite.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					And<Match<INSite, Current<AccessInfo.userName>>>>>>.Select(this);

			if (osites.Count == 1)
			{
				return ((SOOrderSite)osites).SiteID;
			}
			return null;
		}

		private void RecalculateTotalDiscount()
		{
			if (Document.Current != null && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Deleted && Document.Cache.GetStatus(Document.Current) != PXEntryStatus.InsertedDeleted)
			{
				SOOrder copy = PXCache<SOOrder>.CreateCopy(Document.Current);
				Document.Cache.SetValueExt<SOOrder.curyDiscTot>(Document.Current, _discountEngine.GetTotalGroupAndDocumentDiscount(DiscountDetails));
				Document.Cache.RaiseRowUpdated(Document.Current, copy);
			}
		}

		private void RefreshTotalsAndFreeItems(PXCache sender)
		{
			RecalculateTotalDiscount();
			RefreshFreeItemLines(sender);
		}
		#endregion

		#region Carrier Freight Cost

		protected virtual bool CollectFreight
		{
			get
			{
				if (DocumentProperties.Current != null)
				{
					if (DocumentProperties.Current.UseCustomerAccount == true)
						return false;

					if (DocumentProperties.Current.GroundCollect == true && this.CanUseGroundCollect(DocumentProperties.Current))
						return false;
				}

				return true;
			}
		}

		private void CalculateFreightCost(bool supressErrors)
		{
			if (Document.Current.ShipVia != null)
			{
				Carrier carrier = Carrier.PK.Find(this, Document.Current.ShipVia);
				if (carrier != null && carrier.IsExternal == true)
				{
					CarrierPlugin plugin = CarrierPlugin.PK.Find(this, carrier.CarrierPluginID);
					ICarrierService cs = CarrierPluginMaint.CreateCarrierService(this, plugin);
					cs.Method = carrier.PluginMethod;

					CarrierRequest cr = CarrierRatesExt.BuildRateRequest(Document.Current);
					CarrierResult<RateQuote> result = cs.GetRateQuote(cr);

					if (result != null)
					{
						StringBuilder sb = new StringBuilder();
						foreach (Message message in result.Messages)
						{
							sb.AppendFormat("{0}:{1} ", message.Code, message.Description);
						}

						if (result.IsSuccess)
						{
							decimal baseCost = ConvertAmtToBaseCury(result.Result.Currency, arsetup.Current.DefaultRateTypeID, Document.Current.OrderDate.Value, result.Result.Amount);
							SetFreightCost(baseCost);

							//show warnings:
                            if (result.Messages.Count > 0)
							{
                                if (!supressErrors)
								Document.Cache.RaiseExceptionHandling<SOOrder.curyFreightCost>(Document.Current, Document.Current.CuryFreightCost,
									new PXSetPropertyException(sb.ToString(), PXErrorLevel.Warning));
                                else
                                    PXTrace.WriteWarning(sb.ToString());
							}
						}
						else
						{
                            if (!supressErrors)
						{
							Document.Cache.RaiseExceptionHandling<SOOrder.curyFreightCost>(Document.Current, Document.Current.CuryFreightCost,
									new PXSetPropertyException(Messages.CarrierServiceError, PXErrorLevel.Error, sb.ToString()));

							throw new PXException(Messages.CarrierServiceError, sb.ToString());
						}
                            else
                            {
                                PXTrace.WriteError(string.Format(Messages.CarrierServiceError, sb.ToString()));
                            }
						}

					}
				}
			}
		}

		public virtual FreightCalculator CreateFreightCalculator()
		{
			return new FreightCalculator(this);
		}

		protected virtual void SetFreightCost(decimal baseCost)
		{
			SOOrder copy = (SOOrder)Document.Cache.CreateCopy(Document.Current);

			if (soordertype.Current != null && soordertype.Current.CalculateFreight == false)
			{
				copy.FreightCost = 0;
				CM.PXCurrencyAttribute.CuryConvCury<SOOrder.curyFreightCost>(Document.Cache, copy);
			}
			else
			{
				if (!CollectFreight)
					baseCost = 0;

				copy.FreightCost = baseCost;
				CM.PXCurrencyAttribute.CuryConvCury<SOOrder.curyFreightCost>(Document.Cache, copy);
				if (copy.OverrideFreightAmount != true)
				{
					PXResultset<SOLine> res = Transactions.Select();
					FreightCalculator fc = CreateFreightCalculator();
					fc.ApplyFreightTerms<SOOrder, SOOrder.curyFreightAmt>(Document.Cache, copy, res.Count);
				}
			}

			copy.FreightCostIsValid = true;
			Document.Update(copy);
		}

		private decimal ConvertAmtToBaseCury(string from, string rateType, DateTime effectiveDate, decimal amount)
		{
			decimal result = amount;

			using (ReadOnlyScope rs = new ReadOnlyScope(DummyCuryInfo.Cache))
			{
				CurrencyInfo ci = new CurrencyInfo();
				ci.CuryRateTypeID = rateType;
				ci.CuryID = from;
				object newval;
				DummyCuryInfo.Cache.RaiseFieldDefaulting<CurrencyInfo.baseCuryID>(ci, out newval);
				DummyCuryInfo.Cache.SetValue<CurrencyInfo.baseCuryID>(ci, newval);

				DummyCuryInfo.Cache.RaiseFieldDefaulting<CurrencyInfo.basePrecision>(ci, out newval);
				DummyCuryInfo.Cache.SetValue<CurrencyInfo.basePrecision>(ci, newval);

				DummyCuryInfo.Cache.RaiseFieldDefaulting<CurrencyInfo.curyPrecision>(ci, out newval);
				DummyCuryInfo.Cache.SetValue<CurrencyInfo.curyPrecision>(ci, newval);

				DummyCuryInfo.Cache.RaiseFieldDefaulting<CurrencyInfo.curyRate>(ci, out newval);
				DummyCuryInfo.Cache.SetValue<CurrencyInfo.curyRate>(ci, newval);

				DummyCuryInfo.Cache.RaiseFieldDefaulting<CurrencyInfo.recipRate>(ci, out newval);
				DummyCuryInfo.Cache.SetValue<CurrencyInfo.recipRate>(ci, newval);

				DummyCuryInfo.Cache.RaiseFieldDefaulting<CurrencyInfo.curyMultDiv>(ci, out newval);
				DummyCuryInfo.Cache.SetValue<CurrencyInfo.curyMultDiv>(ci, newval);

				ci.SetCuryEffDate(DummyCuryInfo.Cache, effectiveDate);
				PXCurrencyAttribute.CuryConvBase(DummyCuryInfo.Cache, ci, amount, out result);
			}

			return result;
		}

		#endregion

		#region External Tax
		public virtual bool IsExternalTax(string TaxZoneID)
		{
			return false;
		}

		public virtual SOOrder CalculateExternalTax(SOOrder order)
		{
			return order;
		}

		public bool RecalculateExternalTaxesSync { get; set; }

		protected virtual void RecalculateExternalTaxes()
		{
		}

		#endregion

		public virtual void InvoiceOrder(Dictionary<string, object> parameters, IEnumerable<SOOrder> list, InvoiceList created, bool isMassProcess, PXQuickProcess.ActionFlow quickProcessFlow)
		{
			SOShipmentEntry docgraph = PXGraph.CreateInstance<SOShipmentEntry>();
			SOInvoiceEntry ie = PXGraph.CreateInstance<SOInvoiceEntry>();
			Lazy<SOOrderEntry> soe = Lazy.By(CreateInstance<SOOrderEntry>);

			foreach (SOOrder order in list.OrderBy(o => o.OrderType).ThenBy(o => o.OrderNbr))
			{
				try
				{
					if (isMassProcess) PXProcessing<SOOrder>.SetCurrentItem(order);

					ie.Clear();
					ie.ARSetup.Current.RequireControlTotal = false;

					List<PXResult<SOOrderShipment>> shipments = new List<PXResult<SOOrderShipment>>();
					PXResultset<SOShipLine, SOLine> details = null;

					foreach (PXResult<SOOrderType, SOOrderShipment, SOOrderTypeOperation, CurrencyInfo, SOAddress, SOContact, Customer> res in
						PXSelectReadonly2<SOOrderType,
						LeftJoin<SOOrderShipment,
							On2<SOOrderShipment.FK.OrderType,
								And<SOOrderShipment.orderNbr, Equal<Required<SOOrder.orderNbr>>,
								And<SOOrderShipment.confirmed, Equal<True>,
								And<SOOrderShipment.invoiceNbr, IsNull>>>>,
					  LeftJoin<SOOrderTypeOperation,
									On2<SOOrderTypeOperation.FK.OrderType,
									And<Where2<Where<SOOrderShipment.operation, IsNull,
													And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>,
												  Or<Where<SOOrderTypeOperation.operation, Equal<SOOrderShipment.operation>>>>>>,
						CrossJoin<CurrencyInfo, CrossJoin<SOAddress, CrossJoin<SOContact, CrossJoin<Customer>>>>>>,
						Where<SOOrderType.orderType, Equal<Required<SOOrder.orderType>>,
							And<CurrencyInfo.curyInfoID, Equal<Required<SOOrder.curyInfoID>>,
							And<SOAddress.addressID, Equal<Required<SOOrder.billAddressID>>,
							And<SOContact.contactID, Equal<Required<SOOrder.billContactID>>,
							And<Customer.bAccountID, Equal<Required<SOOrder.customerID>>>>>>>>
							.Select(docgraph, order.OrderNbr, order.OrderType, order.CuryInfoID, order.BillAddressID, order.BillContactID, order.CustomerID))
					{
						SOOrderShipment shipment = (SOOrderShipment)res;

						if (((SOOrderType)res).RequireShipping == false || ((SOOrderTypeOperation)res).INDocType == INTranType.NoUpdate)
						{
							//if order is created with zero lines, invoiced, and then new line added, this will save us
							if (shipment.ShipmentNbr == null)
							{
								shipment = (SOOrderShipment)order;
								shipment.ShipmentType = INTranType.DocType(((SOOrderTypeOperation)res).INDocType);
							}

							if (details == null)
							{
								details = new PXResultset<SOShipLine, SOLine>();
							}

							foreach (SOLine line in PXSelectJoin<SOLine, 
								InnerJoin<InventoryItem, 
									On<SOLine.FK.InventoryItem>>, 
								Where<SOLine.orderType, Equal<Required<SOLine.orderType>>, 
								And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>, 
								And<SOLine.lineType, NotEqual<SOLineType.miscCharge>>>>>.Select(docgraph, order.OrderType, order.OrderNbr))
							{
								details.Add(new PXResult<SOShipLine, SOLine>((SOShipLine)line, line));
							}
						}
						else if (order.OpenLineCntr == 0 && order.ShipmentCntr == 0)
						{
							if (shipment.ShipmentNbr == null)
							{
								shipment = (SOOrderShipment)order;
								shipment.ShipmentType = INTranType.DocType(((SOOrderTypeOperation)res).INDocType);
							}
						}

						if (shipment.ShipmentType == SOShipmentType.DropShip)
						{
							if (details == null)
							{
								details = new PXResultset<SOShipLine, SOLine>();
							}

							foreach (PXResult<POReceiptLine, SOLineSplit, SOLine> line in PXSelectJoin<POReceiptLine,
								InnerJoin<SOLineSplit, On<SOLineSplit.pOType, Equal<POReceiptLine.pOType>, And<SOLineSplit.pONbr, Equal<POReceiptLine.pONbr>, And<SOLineSplit.pOLineNbr, Equal<POReceiptLine.pOLineNbr>>>>,
						InnerJoin<SOLine, On<SOLineSplit.FK.Line>>>,
											Where2<Where<POReceiptLine.lineType, Equal<POLineType.goodsForDropShip>, Or<POReceiptLine.lineType, Equal<POLineType.nonStockForDropShip>>>, And<POReceiptLine.receiptNbr, Equal<Current<SOOrderShipment.shipmentNbr>>, And<SOLine.orderType, Equal<Current<SOOrderShipment.orderType>>, And<SOLine.orderNbr, Equal<Current<SOOrderShipment.orderNbr>>>>>>>.SelectMultiBound(docgraph, new object[] { shipment }))
							{
								details.Add(new PXResult<SOShipLine, SOLine>((SOShipLine)line, line));
							}
						}

						if (shipment.ShipmentNbr != null)
						{
							shipments.Add(new PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType, SOOrderTypeOperation, Customer>(shipment, order, (CurrencyInfo)res, (SOAddress)res, (SOContact)res, (SOOrderType)res, (SOOrderTypeOperation)res, (Customer)res));
						}
					}

					shipments = new List<PXResult<SOOrderShipment>>(shipments.OrderBy(s => PXResult.Unwrap<SOOrderShipment>(s).Operation == PXResult.Unwrap<SOOrderType>(s).DefaultOperation ? 0 : 1)
						.ThenBy(s => PXResult.Unwrap<SOOrderShipment>(s).ShipmentNbr));

					foreach (PXResult<SOOrderShipment, SOOrder, CurrencyInfo, SOAddress, SOContact, SOOrderType, SOOrderTypeOperation> res in shipments)
					{
						try
						{
							this.Clear();
							var soorder = (SOOrder)res;
							Document.Current = Document.Search<SOOrder.orderNbr>(soorder.OrderNbr, soorder.OrderType);

							using (var ts = new PXTransactionScope())
							{
								PXTimeStampScope.SetRecordComesFirst(typeof(SOOrder), true);
								ie.InvoiceOrder((DateTime)ie.Accessinfo.BusinessDate, res, details, customer.Current, created, quickProcessFlow, !isMassProcess);
								if (shipments.Count == 1)
								{
									soe.Value.Clear();
									soe.Value.Document.Cache.SetStatus(soe.Value.Document.Current = ie.soorder.Current, PXEntryStatus.Updated);
									PXAutomation.CompleteSimple(soe.Value.Document.View);
									soe.Value.Save.Press();
									PXAutomation.RemovePersisted(ie, typeof(SOOrder), new List<object> { order });
								}
								ts.Complete();
							}

							PXProcessing<SOOrder>.SetProcessed();
						}
						catch
						{
							List<object> orders = new List<object>();
							orders.Add(order);
							PXAutomation.RemovePersisted(ie, ie.soorder.GetItemType(), orders);
							throw;
						}
					}
				}
				catch (Exception ex)
				{
					if (!isMassProcess)
					{
						throw;
					}
					PXProcessing<SOOrder>.SetError(ex);
				}
			}
		}

		public virtual void PostOrder(INIssueEntry docgraph, SOOrder order, DocumentList<INRegister> list)
		{
			PostOrder(docgraph, order, list, null);
		}

		public virtual void PostOrder(INIssueEntry docgraph, SOOrder order, DocumentList<INRegister> list, SOOrderShipment orderShipment)
		{
			INItemPlanIDAttribute.SetReleaseMode<INTranSplit.planID>(docgraph.Caches<INTranSplit>(), true);
			this.Clear();
			docgraph.Clear();

			docgraph.insetup.Current.HoldEntry = false;
			docgraph.insetup.Current.RequireControlTotal = false;

			Document.Current = Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);

			if (orderShipment == null)
			{
				orderShipment = PXSelect<SOOrderShipment,
					Where<SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					And<SOOrderShipment.invtRefNbr, IsNull>>>>.Select(this, new object[] { order });
			}

			//TODO: Temporary solution. Review when AC-80210 is fixed
			if (orderShipment != null && orderShipment.ShipmentType != SOShipmentType.DropShip && orderShipment.ShipmentNbr != Constants.NoShipmentNbr && orderShipment.Confirmed != true)
			{
				throw new PXException(Messages.UnableToProcessUnconfirmedShipment, orderShipment.ShipmentNbr);
			}

			ARRegister ardoc = null;
			if (orderShipment != null)
			{
				ardoc = PXSelect<ARRegister, Where<ARRegister.docType, Equal<Required<ARRegister.docType>>, And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(this, orderShipment.InvoiceType, orderShipment.InvoiceNbr);
			}

			INRegister newdoc = new INRegister()
			{
				BranchID = ardoc?.BranchID ?? order.BranchID,
				DocType = INTranType.DocType(sooperation.Current.INDocType),
				SiteID = null,
				TranDate = ardoc?.DocDate,
				FinPeriodID = ardoc?.FinPeriodID,
				OrigModule = GL.BatchModule.SO,
			};
			if (docgraph.issue.Insert(newdoc) == null)
			{
				return;
			}

			SOLine prev_line = null;
			ARTran prev_artran = null;
			INTran newline = null;

			var reattachedPlans = new List<INItemPlan>();
			foreach (PXResult<SOLine, SOLineSplit, SOOrderType, ARTran, INTran, INItemPlan> res in
				PXSelectJoin<SOLine,
				LeftJoin<SOLineSplit, On<SOLineSplit.FK.Line>,
				InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOLine.orderType>>,
				LeftJoin<ARTran, On<ARTran.sOOrderType, Equal<SOLine.orderType>, And<ARTran.sOOrderNbr, Equal<SOLine.orderNbr>, And<ARTran.sOOrderLineNbr, Equal<SOLine.lineNbr>, And<ARTran.lineType, Equal<SOLine.lineType>>>>>,
				LeftJoin<INTran, On<INTran.sOOrderType, Equal<SOLine.orderType>, And<INTran.sOOrderNbr, Equal<SOLine.orderNbr>, And<INTran.sOOrderLineNbr, Equal<SOLine.lineNbr>>>>,
				LeftJoin<INItemPlan, On<INItemPlan.planID, Equal<SOLineSplit.planID>>>>>>>,
			Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>, And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>, And<SOLine.lineType, Equal<SOLineType.inventory>,
				And2<Where<SOOrderType.aRDocType, Equal<ARDocType.noUpdate>,
						Or<ARTran.tranType, Equal<SOOrderType.aRDocType>,
								And<ARTran.refNbr, IsNotNull>>>, And<INTran.refNbr, IsNull>>>>>,
			OrderBy<Asc<SOLine.orderType, Asc<SOLine.orderNbr, Asc<SOLine.lineNbr>>>>>.Select(this))
			{
				SOLine line = res;
				SOLineSplit split = ((SOLineSplit)res).SplitLineNbr != null ? (SOLineSplit)res : (SOLineSplit)line;
				INItemPlan plan = res;
				INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType) ?? new INPlanType();
				ARTran artran = res;
				SOOrderType ordertype = res;

				//avoid ReadItem()
				if (plan.PlanID != null)
				{
					Caches[typeof(INItemPlan)].SetStatus(plan, PXEntryStatus.Notchanged);
				}

				bool reattachExistingPlan = false;
				if (plantype.DeleteOnEvent == true)
				{
					reattachExistingPlan = true;
					Caches[typeof(SOLineSplit)].SetStatus(split, PXEntryStatus.Updated);
					split = (SOLineSplit)Caches[typeof(SOLineSplit)].Locate(split);
					if (split != null) split.PlanID = null;
					Caches[typeof(SOLineSplit)].IsDirty = true;
				}
				else if (string.IsNullOrEmpty(plantype.ReplanOnEvent) == false)
				{
					plan = PXCache<INItemPlan>.CreateCopy(plan);
					plan.PlanType = plantype.ReplanOnEvent;
					Caches[typeof(INItemPlan)].Update(plan);

					//split.Confirmed = true;
					Caches[typeof(SOLineSplit)].SetStatus(split, PXEntryStatus.Updated);
					Caches[typeof(SOLineSplit)].IsDirty = true;
				}

				if ((Caches[typeof(SOLine)].ObjectsEqual(prev_line, line) == false || object.Equals(line.InventoryID, split.InventoryID) == false) && split.IsStockItem == true
					&& line.Qty != 0)
				{
					newline = new INTran();
					newline.BranchID = artran.BranchID ?? line.BranchID;
					newline.TranType = line.TranType;
					newline.SOShipmentNbr = Constants.NoShipmentNbr;
					newline.SOShipmentType = docgraph.issue.Current.DocType;
					newline.SOShipmentLineNbr = null;
					newline.SOOrderType = line.OrderType;
					newline.SOOrderNbr = line.OrderNbr;
					newline.SOOrderLineNbr = line.LineNbr;
					newline.SOLineType = line.LineType;
					newline.ARDocType = artran.TranType;
					newline.ARRefNbr = artran.RefNbr;
					newline.ARLineNbr = artran.LineNbr;
					newline.AcctID = artran.AccountID;
					newline.SubID = artran.SubID;

					newline.InventoryID = split.InventoryID;
					newline.SiteID = line.SiteID;
					newline.BAccountID = line.CustomerID;
					newline.InvtMult = line.InvtMult;
					newline.Qty = 0m;
					newline.ProjectID = line.ProjectID;
					newline.TaskID = line.TaskID;
					newline.CostCodeID = line.CostCodeID;
					if (object.Equals(line.InventoryID, split.InventoryID) == false)
					{
						newline.SubItemID = split.SubItemID;
						newline.UOM = split.UOM;
						newline.UnitPrice = 0m;
						newline.UnitCost = 0m;
						newline.TranDesc = null;
						if (line.Operation == SOOperation.Receipt)
						{
							var invoiceLines = INTran.FK.SOLine.SelectChildren(this, new SOLine
							{
								OrderType = line.OrigOrderType,
								OrderNbr = line.OrigOrderNbr,
								LineNbr = line.OrigLineNbr
							}).ToList();
							if (invoiceLines.Any())
							{
								INTran invoiceLine = invoiceLines[0];
								newline.UnitCost = invoiceLine.UnitCost;
							}
							else
							{
								var itemCost = INItemCost.PK.Find(this, split.InventoryID);
								if (itemCost != null)
									newline.UnitCost = itemCost.LastCost;
							}
						}
					}
					else
					{
						newline.SubItemID = line.SubItemID;
						newline.UOM = line.UOM;
						newline.UnitPrice = artran.UnitPrice ?? 0m;
						newline.UnitCost = line.UnitCost;
						newline.TranDesc = line.TranDesc;
						newline.ReasonCode = line.ReasonCode;
					}

					newline = docgraph.lsselect.Insert(newline);
				}

				prev_line = line;
				prev_artran = artran;

				if (split.IsStockItem == true)
				{
					if (split.Qty != 0)
					{
					INTranSplit newsplit = (INTranSplit)newline;
					newsplit.SplitLineNbr = null;
					newsplit.SubItemID = split.SubItemID;
					newsplit.LocationID = split.LocationID;
					newsplit.LotSerialNbr = split.LotSerialNbr;
					newsplit.ExpireDate = split.ExpireDate;
					newsplit.UOM = split.UOM;
					newsplit.Qty = split.Qty;
					newsplit.BaseQty = null;
					if (reattachExistingPlan)
					{
						newsplit.PlanID = plan.PlanID;
						reattachedPlans.Add(plan);
					}

					docgraph.splits.Insert(newsplit);
					}
					else
					{
						Caches[typeof(INItemPlan)].Delete(plan);
					}

					if (object.Equals(line.InventoryID, split.InventoryID) && line.Qty != 0)
					{
						bool signMismatch = artran.DrCr == DrCr.Credit && artran.SOOrderLineOperation == SOOperation.Receipt
							|| artran.DrCr == DrCr.Debit && artran.SOOrderLineOperation == SOOperation.Issue;

						newline.TranCost = line.ExtCost;
						newline.TranAmt = (signMismatch ? -artran.TranAmt : artran.TranAmt) ?? 0m;
					}
				}
				else if (plantype.DeleteOnEvent == true)
				{
					Caches[typeof(INItemPlan)].Delete(plan);
				}
			}
			INItemPlanIDAttribute.SetReleaseMode<INTranSplit.planID>(docgraph.Caches<INTranSplit>(), false);

			INRegister copy = PXCache<INRegister>.CreateCopy(docgraph.issue.Current);
			PXFormulaAttribute.CalcAggregate<INTran.qty>(docgraph.transactions.Cache, copy);
			PXFormulaAttribute.CalcAggregate<INTran.tranAmt>(docgraph.transactions.Cache, copy);
			PXFormulaAttribute.CalcAggregate<INTran.tranCost>(docgraph.transactions.Cache, copy);
			docgraph.issue.Update(copy);

			//using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					if (docgraph.transactions.Cache.IsDirty)
					{
						docgraph.Save.Press();

						foreach (SOOrderShipment item in PXSelect<SOOrderShipment, Where<SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>.SelectMultiBound(this, new object[] { order }))
						{
							item.InvtDocType = docgraph.issue.Current.DocType;
							item.InvtRefNbr = docgraph.issue.Current.RefNbr;
							item.InvtNoteID = docgraph.issue.Current.NoteID;

							shipmentlist.Cache.Update(item);

							UpdatePlansRefNoteID(item, item.InvtNoteID, reattachedPlans);
						}

						this.Save.Press();

						INRegister existing;
						if ((existing = list.Find(docgraph.issue.Current)) == null)
						{
							list.Add(docgraph.issue.Current);
						}
						else
						{
							docgraph.issue.Cache.RestoreCopy(existing, docgraph.issue.Current);
						}
					}
					ts.Complete();
				}
			}
		}

		public virtual void UpdatePlansRefNoteID(SOOrderShipment orderShipment, Guid? refNoteID, IEnumerable<INItemPlan> reattachedPlans)
		{
			if (!reattachedPlans.Any()) return;

			// supposed that at this point there may be only deleted INItemPlan records in our graph
			// they should be persisted before the following direct update
			this.Caches[typeof(INItemPlan)].Persist(PXDBOperation.Delete);
			this.Caches[typeof(INItemPlan)].Persisted(false);

			// update INItemPlan.RefNoteID with the new IN Issue identifier
			PXUpdateJoin<
				Set<INItemPlan.refNoteID, Required<INItemPlan.refNoteID>,
				Set<INItemPlan.refEntityType, Common.Constants.DACName<INRegister>>>,
				INItemPlan,
					InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<INItemPlan.planID>>>,
				Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>,
					And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>>>>
			.Update(this,
				refNoteID,
				orderShipment.OrderType,
				orderShipment.OrderNbr);

			var stamp = PXDatabase.SelectTimeStamp();
			foreach (var plan in reattachedPlans)
				PXTimeStampScope.PutPersisted(this.Caches[typeof(INItemPlan)], plan, stamp);
		}

		#region EPApproval Cahce Attached
		[PXDBDate()]
		[PXDefault(typeof(SOOrder.orderDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(SOOrder.customerID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBGuid()]
		[PXDefault(typeof(SOOrder.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(SOOrder.orderDesc), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[CurrencyInfo(typeof(SOOrder.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(SOOrder.curyOrderTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(SOOrder.orderTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}
		#endregion

		protected CCOrderList toRecord;

		public bool AllowAllocation()
		{
			bool allowAllocation = soordertype.Current != null && soordertype.Current.RequireAllocation != true
				|| PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>()
				|| PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>()
				|| PXAccess.FeatureInstalled<FeaturesSet.subItem>()
				|| PXAccess.FeatureInstalled<FeaturesSet.replenishment>()
				|| PXAccess.FeatureInstalled<FeaturesSet.sOToPOLink>();
			return allowAllocation;
		}

		protected virtual void RemoveOrphanReplenishmentLines()
		{
			if (this.UnattendedMode)
				return;

			var removedPlans = Caches[typeof(INItemPlan)].Deleted
				.Cast<INItemPlan>()
				.Select(p => p.PlanID)
				.ToHashSet();

			if (removedPlans.Count == 0)
				return;

			foreach (var replenishmentLine in ReplenishmentLinesWithPlans.Select())
			{
				var plan = replenishmentLine.GetItem<INItemPlan>();
				if (plan?.SupplyPlanID == null || !removedPlans.Contains(plan.SupplyPlanID))
					continue;

				ReplenishmentLinesWithPlans.Delete(replenishmentLine);
				Caches[typeof(INItemPlan)].Delete(plan);
			}
		}

		public override void Persist()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Debug.Print("{0} Enter Persist()", DateTime.Now.TimeOfDay);
			Debug.Indent();

			if (toRecord == null)
				toRecord = new CCOrderList(this);
			else
				toRecord.Clear();

			RemoveOrphanReplenishmentLines();

			foreach (SiteStatus ss in sitestatusview.Cache.Inserted)
			{
				if (ss.InitSiteStatus != true)
					continue;

				INItemSite itemsite = INReleaseProcess.SelectItemSite(this, ss.InventoryID, ss.SiteID);
				if (itemsite == null)
				{
					InventoryItem item = InventoryItem.PK.Find(this, ss.InventoryID);
					if (item.StkItem == true)
					{
						INSite site = INSite.PK.Find(this, ss.SiteID);
						INPostClass postclass = INPostClass.PK.Find(this, item.PostClassID);

						itemsite = new INItemSite();
						itemsite.InventoryID = ss.InventoryID;
						itemsite.SiteID = ss.SiteID;
						INItemSiteMaint.DefaultItemSiteByItem(this, itemsite, item, site, postclass);
						itemsite = initemsite.Insert(itemsite);
					}
				}

			}

			foreach (SOOrder doc in Document.Cache.Cached)
			{
				if ((Document.Cache.GetStatus(doc) == PXEntryStatus.Inserted || Document.Cache.GetStatus(doc) == PXEntryStatus.Updated)
				 && doc.Completed == false)
				{
					SOOrderType orderType = (this.soordertype.Current != null && this.soordertype.Current.OrderType == doc.OrderType) ? this.soordertype.Current : this.soordertype.Select(doc.OrderType);
					if (orderType.ARDocType == ARDocType.Invoice)
					{
						decimal? CuryApplAmt = 0m;
						bool appliedToOrderUpdated = false;

						foreach (PXResult<SOAdjust, ARRegisterAlias, ARPayment, CurrencyInfo> res in Adjustments_Raw.View.SelectMultiBound(new object[] { doc }))
						{
							SOAdjust adj = (SOAdjust)res;

							if (adj != null)
							{
								CuryApplAmt += adj.CuryAdjdAmt;

								if (Adjustments.Cache.GetStatus(adj) == PXEntryStatus.Updated || Adjustments.Cache.GetStatus(adj) == PXEntryStatus.Inserted)
									appliedToOrderUpdated = true;

								if (doc.CuryDocBal - CuryApplAmt < 0m && CuryApplAmt > 0m)
								{
									if (appliedToOrderUpdated || ((decimal?)Document.Cache.GetValueOriginal<SOOrder.curyOrderTotal>(doc) != doc.CuryOrderTotal))
									{
										Adjustments.Cache.MarkUpdated(adj);
										Adjustments.Cache.RaiseExceptionHandling<SOAdjust.curyAdjdAmt>(adj, adj.CuryAdjdAmt, new PXSetPropertyException(Messages.OrderApplyAmount_Cannot_Exceed_OrderTotal));
										throw new PXException(Messages.OrderApplyAmount_Cannot_Exceed_OrderTotal);
									}
								}
							}
						}
					}

					var state = ExternalTranHelper.GetActiveTransactionState(this, ExternalTran);
					if (doc.PMInstanceID.HasValue
					 && string.IsNullOrEmpty(doc.PreAuthTranNumber) == false && !state.IsPreAuthorized && !state.IsCaptured)
					{
						SOOrder copy = (SOOrder)this.Document.Cache.CreateCopy(doc);
						copy.IsCCAuthorized = true;
						copy = this.Document.Update(copy);
						toRecord.Add(copy);
					}
				}
			}

			if (Document.Current != null
				&& Document.Current.IsPackageValid != true
				&& !string.IsNullOrEmpty(Document.Current.ShipVia)
				&& (soordertype.Current.RequireShipping == true || soordertype.Current?.Behavior == SOBehavior.QT))
			{
				try
				{
					if (Document.Current.IsManualPackage != true)
					{
						CarrierRatesExt.RecalculatePackagesForOrder(Document.Current);
					}
				}
				catch (Exception ex)
				{
					PXTrace.WriteError(ex);
				}
			}


			if (Document.Current != null
				&& Document.Current.FreightCostIsValid != true
				&& soordertype.Current?.CalculateFreight == true
				&& !string.IsNullOrEmpty(Document.Current.ShipVia))
			{
				try
				{
					CalculateFreightCost(true);
				}
				catch (Exception ex)
				{
					PXTrace.WriteError(ex);
				}
			}

			foreach (SOOrder order in Document.Cache.Updated)
			{
				if ((order.Behavior == SOBehavior.SO || order.Behavior == SOBehavior.RM) && order.ShipmentCntr > 0 && order.OpenShipmentCntr == 0 && order.OpenLineCntr == 0)
				{
					order.Approved = Approval.IsApproved(order);
					order.Completed = true;
					order.Hold = false;
					Document.Update(order);
					Document.Search<SOOrder.orderNbr>(Document.Current.OrderNbr, Document.Current.OrderType);
				}
			}
			_discountEngine.ValidateDiscountDetails(DiscountDetails);

			//When the calling process is a long-running operation recalculate taxes on the same thread before the Persist.
			// PXAutomation.CompleteAction is called even if there is an Exception after the base.Persist() call.
			if (RecalculateExternalTaxesSync)
				RecalculateExternalTaxes();

			if (toRecord.Count > 0)
			{
				this.RowPersisted.AddHandler<CustomerPaymentMethodC>(toRecord.Authorize);
				this.RowPersisted.AddHandler<SOOrder>(toRecord.Authorize);
				try
				{
					base.Persist();
				}
				finally
				{
					this.RowPersisted.RemoveHandler<SOOrder>(toRecord.Authorize);
					this.RowPersisted.RemoveHandler<CustomerPaymentMethodC>(toRecord.Authorize);

				}
			}
			else
				base.Persist();

			if (!RecalculateExternalTaxesSync) //When the calling process is the 'UI' thread.
				RecalculateExternalTaxes();

			sw.Stop();
			Debug.Unindent();
			Debug.Print("{0} Exit Persist in {1} millisec", DateTime.Now.TimeOfDay, sw.ElapsedMilliseconds);
		}

		protected void SetReadOnly(bool isReadOnly)
		{
			PXCache[] cachearr = new PXCache[Caches.Count];
			try
			{
				Caches.Values.CopyTo(cachearr, 0);
			}
			catch (ArgumentException)
			{
				cachearr = new PXCache[Caches.Count + 5];
				Caches.Values.CopyTo(cachearr, 0);
			}
			foreach (PXCache cache in cachearr)
			{
				if (cache != null)
			{
				cache.AllowDelete = !isReadOnly;
				cache.AllowUpdate = !isReadOnly;
				cache.AllowInsert = !isReadOnly;
			}
		}
		}

		protected sealed class CCOrderList : List<SOOrder>
		{
			private CCPaymentProcessingGraph ccProcGraph = PXGraph.CreateInstance<CCPaymentProcessingGraph>();
			SOOrderEntry orderEntry;
			public CCOrderList(SOOrderEntry graph)
			{
				orderEntry = graph;
			}
			public void Authorize(PXCache sender, PXRowPersistedEventArgs e)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					SOOrder iOrder = null;
					SOOrder row = e.Row as SOOrder;
					if (row != null && this.Contains(row)
						&& row.PMInstanceID.HasValue && row.PMInstanceID > 0)
					{
						iOrder = row;
					}
					else
					{
						AR.CustomerPaymentMethod iCard = e.Row as AR.CustomerPaymentMethod;
						if (iCard != null && e.Operation == PXDBOperation.Insert)
						{
							iOrder = this.Find((SOOrder op) => { return op.PMInstanceID == iCard.PMInstanceID && !string.IsNullOrEmpty(op.PreAuthTranNumber); });
						}
					}
					if (iOrder != null)
					{
						decimal authAmount = (iOrder.CuryCCPreAuthAmount) ?? Decimal.Zero;
						if (authAmount == Decimal.Zero)
							throw new PXException(Messages.OrderHaveZeroBalanceError);
		
						TranRecordData tranRecord = new TranRecordData();
						tranRecord.ExternalTranId = iOrder.PreAuthTranNumber;
						tranRecord.Amount = iOrder.CuryCCPreAuthAmount;
						tranRecord.ExpirationDate = iOrder.CCAuthExpirationDate;
						tranRecord.ResponseText = AR.Messages.ImportedExternalCCTransaction;
						ccProcGraph.RecordAuthorization(iOrder, tranRecord);
					}
				}
			}
		}

		[Serializable]
		public partial class SOAdjust : PX.Objects.SO.SOAdjust
		{
			#region AdjdOrderType
			public new abstract class adjdOrderType : PX.Data.BQL.BqlString.Field<adjdOrderType> { }
			[PXDBString(2, IsKey = true, IsFixed = true)]
			[PXDBDefault(typeof(SOOrder.orderType))]
			[PXUIField(DisplayName = "Order Type")]
			public override String AdjdOrderType
			{
				get
				{
					return this._AdjdOrderType;
				}
				set
				{
					this._AdjdOrderType = value;
				}
			}
			#endregion
			#region AdjdOrderNbr
			public new abstract class adjdOrderNbr : PX.Data.BQL.BqlString.Field<adjdOrderNbr> { }
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			[PXDBDefault(typeof(SOOrder.orderNbr))]
			[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOAdjust.adjdOrderType>>, And<SOOrder.orderNbr, Equal<Current<SOAdjust.adjdOrderNbr>>>>>))]
			[PXUnboundFormula(typeof(Switch<Case<Where<SOAdjust.curyAdjdAmt, Greater<decimal0>>, int1>, int0>), typeof(SumCalc<SOOrder.paymentCntr>))]
			[PXUIField(DisplayName = "Order Nbr.")]
			public override String AdjdOrderNbr
			{
				get
				{
					return this._AdjdOrderNbr;
				}
				set
				{
					this._AdjdOrderNbr = value;
				}
			}
			#endregion
			#region AdjgDocType
			public new abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
			[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
			[ARPaymentType.SOList()]
			[PXDefault(ARDocType.Payment)]
			[PXUIField(DisplayName = "Doc. Type")]
			public override String AdjgDocType
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
			public new abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
			[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
			[PXDefault()]
			[PXUIField(DisplayName = "Reference Nbr.")]
			[ARPaymentType.AdjgRefNbr(typeof(Search<ARPayment.refNbr,
				Where<ARPayment.customerID, In3<Current<SOOrder.customerID>, Current<Customer.consolidatingBAccountID>>,
					And<ARPayment.docType, Equal<Optional<SOAdjust.adjgDocType>>,
					And<ARPayment.openDoc, Equal<True>>>>>), Filterable = true)]
			public override String AdjgRefNbr
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
			#region CustomerID
			public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			[PXDBInt]
			[PXDBDefault(typeof(SOOrder.customerID))]
			[PXUIField(DisplayName = "CustomerID", Visibility = PXUIVisibility.Visible, Visible = false)]
			public override Int32? CustomerID
			{
				get
				{
					return this._CustomerID;
				}
				set
				{
					this._CustomerID = value;
				}
			}
			#endregion
			#region CuryAdjdAmt
			public new abstract class curyAdjdAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjdAmt> { }
			[PXDBCurrency(typeof(SOAdjust.adjdCuryInfoID), typeof(SOAdjust.adjAmt))]
			[PXUIField(DisplayName = "Applied To Order")]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXFormula(null, typeof(SumCalc<SOOrder.curyPaymentTotal>))]
			[PXFormula(typeof(Sub<SOAdjust.curyOrigAdjdAmt, SOAdjust.curyAdjdBilledAmt>))]
			public override Decimal? CuryAdjdAmt
			{
				get
				{
					return this._CuryAdjdAmt;
				}
				set
				{
					this._CuryAdjdAmt = value;
				}
			}
			#endregion
			#region AdjAmt
			public new abstract class adjAmt : PX.Data.BQL.BqlDecimal.Field<adjAmt> { }
			[PXDBDecimal(4)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXFormula(typeof(Sub<SOAdjust.origAdjAmt, SOAdjust.adjBilledAmt>))]
			public override Decimal? AdjAmt
			{
				get
				{
					return this._AdjAmt;
				}
				set
				{
					this._AdjAmt = value;
				}
			}
			#endregion
			#region CuryAdjgAmt
			public new abstract class curyAdjgAmt : PX.Data.BQL.BqlDecimal.Field<curyAdjgAmt> { }
			[PXDBDecimal(4)]
			[PXFormula(typeof(Sub<SOAdjust.curyOrigAdjgAmt, SOAdjust.curyAdjgBilledAmt>))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? CuryAdjgAmt
			{
				get
				{
					return this._CuryAdjgAmt;
				}
				set
				{
					this._CuryAdjgAmt = value;
				}
			}
			#endregion
			#region AdjdOrigCuryInfoID
			public new abstract class adjdOrigCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdOrigCuryInfoID> { }
			[PXDBLong()]
			[CurrencyInfo(typeof(SOOrder.curyInfoID), ModuleCode = BatchModule.SO, CuryIDField = "AdjdOrigCuryID")]
			public override Int64? AdjdOrigCuryInfoID
			{
				get
				{
					return this._AdjdOrigCuryInfoID;
				}
				set
				{
					this._AdjdOrigCuryInfoID = value;
				}
			}
			#endregion
			#region AdjgCuryInfoID
			public new abstract class adjgCuryInfoID : PX.Data.BQL.BqlLong.Field<adjgCuryInfoID> { }
			[PXDBLong()]
			[PXDefault()]
			[CurrencyInfo(ModuleCode = BatchModule.SO, CuryIDField = "AdjgCuryID")]
			public override Int64? AdjgCuryInfoID
			{
				get
				{
					return this._AdjgCuryInfoID;
				}
				set
				{
					this._AdjgCuryInfoID = value;
				}
			}
			#endregion
			#region AdjdCuryInfoID
			public new abstract class adjdCuryInfoID : PX.Data.BQL.BqlLong.Field<adjdCuryInfoID> { }
			[PXDBLong()]
			[CurrencyInfo(typeof(SOOrder.curyInfoID), ModuleCode = BatchModule.SO, CuryIDField = "AdjdCuryID")]
			public override Int64? AdjdCuryInfoID
			{
				get
				{
					return this._AdjdCuryInfoID;
				}
				set
				{
					this._AdjdCuryInfoID = value;
				}
			}
			#endregion
			#region AdjgDocDate
			public new abstract class adjgDocDate : PX.Data.BQL.BqlDateTime.Field<adjgDocDate> { }
			[PXDBDate()]
			[PXDBDefault(typeof(SOOrder.orderDate))]
			public override DateTime? AdjgDocDate
			{
				get
				{
					return this._AdjgDocDate;
				}
				set
				{
					this._AdjgDocDate = value;
				}
			}
			#endregion
			#region AdjdOrderDate
			public new abstract class adjdOrderDate : PX.Data.BQL.BqlDateTime.Field<adjdOrderDate> { }
			[PXDBDate()]
			[PXDBDefault(typeof(SOOrder.orderDate))]
			[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public override DateTime? AdjdOrderDate
			{
				get
				{
					return this._AdjdOrderDate;
				}
				set
				{
					this._AdjdOrderDate = value;
				}
			}
			#endregion

			#region CuryDocBal
			public new abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
			[PXCurrency(typeof(SOAdjust.adjgCuryInfoID), typeof(SOAdjust.docBal), BaseCalc = false)]
			[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public override Decimal? CuryDocBal
			{
				get
				{
					return this._CuryDocBal;
				}
				set
				{
					this._CuryDocBal = value;
				}
			}
			#endregion
			#region DocBal
			public new abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
			[PXDecimal(4)]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? DocBal
			{
				get
				{
					return this._DocBal;
				}
				set
				{
					this._DocBal = value;
				}
			}
			#endregion
		}

		#region CopyParamFilter
		protected virtual void CopyParamFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			CopyParamFilter row = e.Row as CopyParamFilter;

			if (row.OrderType != null)
			{
				var orderType = SOOrderType.PK.Find(this, row.OrderType);
				
				Numbering numbering = PXSelect<Numbering, 
					Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>.Select(this, orderType.OrderNumberingID);

				PXUIFieldAttribute.SetEnabled<CopyParamFilter.orderNbr>(sender, e.Row, numbering.UserNumbering == true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled<CopyParamFilter.orderNbr>(sender, e.Row, false);
			}
			checkCopyParams.SetEnabled(!string.IsNullOrEmpty(row.OrderType) && !string.IsNullOrEmpty(row.OrderNbr));
			if (string.IsNullOrEmpty(row.OrderType))
				PXUIFieldAttribute.SetWarning<CopyParamFilter.orderType>(sender, e.Row, PXMessages.LocalizeFormatNoPrefix(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyParamFilter.orderType>(sender)));
			else
				PXUIFieldAttribute.SetWarning<CopyParamFilter.orderType>(sender, e.Row, null);
			if (string.IsNullOrEmpty(row.OrderNbr))
				PXUIFieldAttribute.SetWarning<CopyParamFilter.orderNbr>(sender, e.Row, PXMessages.LocalizeFormatNoPrefix(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CopyParamFilter.orderNbr>(sender)));
			else
				PXUIFieldAttribute.SetWarning<CopyParamFilter.orderNbr>(sender, e.Row, null);

			PXUIFieldAttribute.SetEnabled<CopyParamFilter.overrideManualDiscounts>(sender, row, (row.RecalcDiscounts == true && Document.Current != null && Document.Current.DisableAutomaticDiscountCalculation != true));

			sender.IsDirty = false;
		}

		protected virtual void CopyParamFilter_OrderType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CopyParamFilter row = e.Row as CopyParamFilter;
			if (row != null)
			{
				if (row.OrderType != null)
					sender.SetDefaultExt<CopyParamFilter.orderNbr>(e.Row);
				else
					row.OrderNbr = null;
			}
		}
		#endregion

		#region Implementation of IPXPrepareItems

		public MultiDuplicatesSearchEngine<SOLine> DuplicateFinder { get; set; }

		private bool DontUpdateExistRecords
		{
			get
			{
				object dontUpdateExistRecords;
				return IsImportFromExcel && PXExecutionContext.Current.Bag.TryGetValue(PXImportAttribute._DONT_UPDATE_EXIST_RECORDS, out dontUpdateExistRecords) &&
					true.Equals(dontUpdateExistRecords);
			}
		}

		protected virtual Type[] GetAlternativeKeyFields()
		{
			var keys = new List<Type>()
			{
				typeof(SOLine.branchID),
				typeof(SOLine.inventoryID),
				typeof(SOLine.siteID),
				typeof(SOLine.locationID),
				typeof(SOLine.alternateID),
				typeof(SOLine.invoiceNbr),
			};

			if (PXAccess.FeatureInstalled<FeaturesSet.subItem>())
				keys.Add(typeof(SOLine.subItemID));

			return keys.ToArray();
		}

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, nameof(Transactions), true) == 0)
			{
				if (values.Contains(nameof(SOLine.orderType))) values[nameof(SOLine.orderType)] = Document.Current.OrderType;
				else values.Add(nameof(SOLine.orderType), Document.Current.OrderType);

				if (values.Contains(nameof(SOLine.orderNbr))) values[nameof(SOLine.orderNbr)] = Document.Current.OrderNbr;
				else values.Add(nameof(SOLine.orderNbr), Document.Current.OrderNbr);
				//this._blockUIUpdate = true;

				if (!DontUpdateExistRecords)
				{
					if (DuplicateFinder == null)
					{
						var details = Transactions.SelectMain();
						DuplicateFinder = new MultiDuplicatesSearchEngine<SOLine>(Transactions.Cache, GetAlternativeKeyFields(), details);
					}
					var duplicate = DuplicateFinder.Find(values);
					if (duplicate != null)
					{
						DuplicateFinder.RemoveItem(duplicate);

						if (keys.Contains(nameof(SOLine.lineNbr)))
							keys[nameof(SOLine.LineNbr)] = duplicate.LineNbr;
						else
							keys.Add(nameof(SOLine.LineNbr), duplicate.LineNbr);
					}
					else if (keys.Contains(nameof(SOLine.lineNbr)))
					{
						bool lineExists = false;

						object value = keys[nameof(SOLine.lineNbr)];
						if (Transactions.Cache.RaiseFieldUpdating<SOLine.lineNbr>(null, ref value) &&
							value is int lineNbr)
						{
							var line = new SOLine()
							{
								OrderType = Document.Current.OrderType,
								OrderNbr = Document.Current.OrderNbr,
								LineNbr = lineNbr
							};

							lineExists = Transactions.Cache.Locate(line) != null;
						}
						
						if (lineExists)			
							keys.Remove(nameof(SOLine.lineNbr));
					}
				}
			}

			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items)
		{
		}

		public virtual void ImportDone(PXImportAttribute.ImportMode.Value mode)
		{
			DuplicateFinder = null;

			var soOrder = Document.Current;
			if (soOrder != null)
			{
				CalcFreight(Document.Cache, soOrder);
				
				_discountEngine.AutoRecalculatePricesAndDiscounts(
					cache: Transactions.Cache, 
					lines: Transactions, 
					currentLine: null, 
					discountDetails: DiscountDetails, 
					locationID: Document.Current.CustomerLocationID, 
					date: Document.Current.OrderDate, 
					discountCalculationOptions: GetDefaultSODiscountCalculationOptions(Document.Current) | DiscountEngine.DiscountCalculationOptions.EnableOptimizationOfGroupAndDocumentDiscountsCalculation);
			}
		}

		#endregion

		public static void ProcessPOReceipt(PXGraph graph, IEnumerable<PXResult<INItemPlan, INPlanType>> list, string POReceiptType, string POReceiptNbr)
		{
			var soorder = new PXSelect<SOOrder>(graph);
			if (!graph.Views.Caches.Contains(typeof(SOOrder)))
				graph.Views.Caches.Add(typeof(SOOrder));
			var solinesplit = new PXSelect<SOLineSplit>(graph);
			if (!graph.Views.Caches.Contains(typeof(SOLineSplit)))
				graph.Views.Caches.Add(typeof(SOLineSplit));
			var initemplan = new PXSelect<INItemPlan>(graph);

			List<SOLineSplit> splitsToDeletePlanID = new List<SOLineSplit>();

			List<SOLineSplit> insertedSchedules = new List<SOLineSplit>();
			List<INItemPlan> deletedPlans = new List<INItemPlan>();

			foreach (PXResult<INItemPlan, INPlanType> res in list)
			{
				INItemPlan plan = PXCache<INItemPlan>.CreateCopy(res);
				INPlanType plantype = res;

				//avoid ReadItem()
				if (initemplan.Cache.GetStatus(plan) != PXEntryStatus.Inserted)
				{
					initemplan.Cache.SetStatus(plan, PXEntryStatus.Notchanged);
				}

				//Original Schedule Marked for PO / Allocated on Remote Whse
				//SOLineSplit schedule = PXSelect<SOLineSplit, Where<SOLineSplit.planID, Equal<Required<SOLineSplit.planID>>, And<SOLineSplit.completed, Equal<False>>>>.Select(this, plan.DemandPlanID);
				SOLineSplit schedule = PXSelect<SOLineSplit, Where<SOLineSplit.planID, Equal<Required<SOLineSplit.planID>>>>.Select(graph, plan.DemandPlanID);

				if (schedule != null && (schedule.Completed == false || solinesplit.Cache.GetStatus(schedule) == PXEntryStatus.Updated))
				{
					schedule = PXCache<SOLineSplit>.CreateCopy(schedule);

					schedule.BaseReceivedQty += plan.PlanQty;
					schedule.ReceivedQty = INUnitAttribute.ConvertFromBase(solinesplit.Cache, schedule.InventoryID, schedule.UOM, (decimal)schedule.BaseReceivedQty, INPrecision.QUANTITY);

					solinesplit.Cache.Update(schedule);

					INItemPlan origplan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(graph, plan.DemandPlanID);
					if (origplan != null)
					{
						origplan.PlanQty = schedule.BaseQty - schedule.BaseReceivedQty;
						initemplan.Cache.Update(origplan);
					}

					//select Allocated line if any, exclude allocated on Remote Whse
					PXSelectBase<INItemPlan> cmd = new PXSelectJoin<INItemPlan, InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<INItemPlan.planID>>>, Where<INItemPlan.demandPlanID, Equal<Required<INItemPlan.demandPlanID>>, And<SOLineSplit.isAllocated, Equal<True>, And<SOLineSplit.siteID, Equal<Required<SOLineSplit.siteID>>>>>>(graph);
					if (!string.IsNullOrEmpty(plan.LotSerialNbr))
					{
						cmd.WhereAnd<Where<INItemPlan.lotSerialNbr, Equal<Required<INItemPlan.lotSerialNbr>>>>();
					}
					PXResult<INItemPlan> allocres = cmd.Select(plan.DemandPlanID, plan.SiteID, plan.LotSerialNbr);

					if (allocres != null)
					{
						schedule = PXResult.Unwrap<SOLineSplit>(allocres);
						solinesplit.Cache.SetStatus(schedule, PXEntryStatus.Notchanged);
						schedule = PXCache<SOLineSplit>.CreateCopy(schedule);
						schedule.BaseQty += plan.PlanQty;
						schedule.Qty = INUnitAttribute.ConvertFromBase(solinesplit.Cache, schedule.InventoryID, schedule.UOM, (decimal)schedule.BaseQty, INPrecision.QUANTITY);
						schedule.POReceiptType = POReceiptType;
						schedule.POReceiptNbr = POReceiptNbr;

						solinesplit.Cache.Update(schedule);

						INItemPlan allocplan = PXCache<INItemPlan>.CreateCopy(res);
						allocplan.PlanQty += plan.PlanQty;

						initemplan.Cache.Update(allocplan);

						plantype = PXCache<INPlanType>.CreateCopy(plantype);
						plantype.ReplanOnEvent = null;
						plantype.DeleteOnEvent = true;
					}
					else
					{
						soorder.Current = (SOOrder)PXParentAttribute.SelectParent(solinesplit.Cache, schedule, typeof(SOOrder));
						schedule = PXCache<SOLineSplit>.CreateCopy(schedule);

						long? oldPlanID = schedule.PlanID;
						ClearScheduleReferences(ref schedule);

						schedule.IsAllocated = (plantype.ReplanOnEvent != INPlanConstants.Plan60);
						schedule.LotSerialNbr = plan.LotSerialNbr;
						schedule.POCreate = false;
						schedule.POSource = null;
						schedule.POReceiptType = POReceiptType;
						schedule.POReceiptNbr = POReceiptNbr;
						schedule.SiteID = plan.SiteID;
						schedule.VendorID = null;

						schedule.BaseReceivedQty = 0m;
						schedule.ReceivedQty = 0m;
						schedule.BaseQty = plan.PlanQty;
						schedule.Qty = INUnitAttribute.ConvertFromBase(solinesplit.Cache, schedule.InventoryID, schedule.UOM, (decimal)schedule.BaseQty, INPrecision.QUANTITY);

						//update SupplyPlanID in existing item plans (replenishment)
						foreach (PXResult<INItemPlan> demand_res in PXSelect<INItemPlan,
							Where<INItemPlan.supplyPlanID, Equal<Required<INItemPlan.supplyPlanID>>>>.Select(graph, oldPlanID))
						{
							INItemPlan demand_plan = PXCache<INItemPlan>.CreateCopy(demand_res);
							initemplan.Cache.SetStatus(demand_plan, PXEntryStatus.Notchanged);
							demand_plan.SupplyPlanID = plan.PlanID;
							initemplan.Cache.Update(demand_plan);
						}

						schedule.PlanID = plan.PlanID;

						schedule = (SOLineSplit)solinesplit.Cache.Insert(schedule);
						insertedSchedules.Add(schedule);
					}
				}
				else if (plan.DemandPlanID != null)
				{
					//Original schedule was completed/plan record deleted by Cancel Order or Confirm Shipment
					plantype = PXCache<INPlanType>.CreateCopy(plantype);
					plantype.ReplanOnEvent = null;
					plantype.DeleteOnEvent = true;
				}
				else
				{
					//Original schedule Marked for PO
					//TODO: verify this is sufficient for Original SO marked for TR.
					schedule = PXSelect<SOLineSplit, Where<SOLineSplit.planID, Equal<Required<SOLineSplit.planID>>, And<SOLineSplit.completed, Equal<False>>>>.Select(graph, plan.PlanID);
					if (schedule != null)
					{
						solinesplit.Cache.SetStatus(schedule, PXEntryStatus.Notchanged);
						schedule = PXCache<SOLineSplit>.CreateCopy(schedule);

						schedule.Completed = true;
						schedule.POCompleted = true;
						splitsToDeletePlanID.Add(schedule);
						solinesplit.Cache.Update(schedule);

						INItemPlan origplan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(graph, plan.PlanID);
						deletedPlans.Add(origplan);

						initemplan.Cache.Delete(origplan);
					}
				}

				if (plantype.ReplanOnEvent != null)
				{
					plan.PlanType = plantype.ReplanOnEvent;
					plan.SupplyPlanID = null;
					plan.DemandPlanID = null;
					initemplan.Cache.Update(plan);
				}
				else if (plantype.DeleteOnEvent == true)
				{
					initemplan.Delete(plan);
				}
			}

			//Create new schedules for partially received schedules marked for PO.
			SOLineSplit prevSplit = null;
			foreach (SOLineSplit newsplit in insertedSchedules)
			{
				if (prevSplit != null && prevSplit.OrderType == newsplit.OrderType && prevSplit.OrderNbr == newsplit.OrderNbr
					&& prevSplit.LineNbr == newsplit.LineNbr && prevSplit.InventoryID == newsplit.InventoryID
					&& prevSplit.SubItemID == newsplit.SubItemID && prevSplit.ParentSplitLineNbr == newsplit.ParentSplitLineNbr
					&& prevSplit.LotSerialNbr != null && newsplit.LotSerialNbr != null)
					continue;

				SOLineSplit parentschedule = PXSelect<SOLineSplit, Where<SOLineSplit.orderType, Equal<Required<SOLineSplit.orderType>>,
					And<SOLineSplit.orderNbr, Equal<Required<SOLineSplit.orderNbr>>,
					And<SOLineSplit.lineNbr, Equal<Required<SOLineSplit.lineNbr>>,
					And<SOLineSplit.splitLineNbr, Equal<Required<SOLineSplit.parentSplitLineNbr>>>>>>>.Select(graph, newsplit.OrderType, newsplit.OrderNbr, newsplit.LineNbr, newsplit.ParentSplitLineNbr);

				if (parentschedule != null && parentschedule.Completed == true && parentschedule.POCompleted == true && parentschedule.BaseQty > parentschedule.BaseReceivedQty
							&& deletedPlans.Exists(x => x.PlanID == parentschedule.PlanID))
				{
					soorder.Current = (SOOrder)PXParentAttribute.SelectParent(solinesplit.Cache, parentschedule, typeof(SOOrder));

					parentschedule = PXCache<SOLineSplit>.CreateCopy(parentschedule);
					INItemPlan demand = PXCache<INItemPlan>.CreateCopy(deletedPlans.First(x => x.PlanID == parentschedule.PlanID));

					UpdateSchedulesFromCompletedPO(graph, solinesplit, initemplan, parentschedule, soorder, demand);
				}
				prevSplit = newsplit;
			}

			//Added because of MySql AutoIncrement counters behavior
			foreach (SOLineSplit split in splitsToDeletePlanID)
			{
				SOLineSplit schedule = (SOLineSplit)solinesplit.Cache.Locate(split);
				if (schedule != null)
				{
					schedule.PlanID = null;
					solinesplit.Cache.Update(schedule);
				}
			}
		}

		public static void ProcessPOOrder(PXGraph graph, POOrder poOrder)
		{
			if (poOrder == null) return;

			var soorder = new PXSelect<SOOrder>(graph);
			if (!graph.Views.Caches.Contains(typeof(SOOrder)))
				graph.Views.Caches.Add(typeof(SOOrder));
			var solinesplit = new PXSelect<SOLineSplit>(graph);
			if (!graph.Views.Caches.Contains(typeof(SOLineSplit)))
				graph.Views.Caches.Add(typeof(SOLineSplit));
			var initemplan = new PXSelect<INItemPlan>(graph);

			//Search for completed/cancelled POLines with uncompleted linked schedules
			foreach (PXResult<POLine, INItemPlan, SOLineSplit> res in PXSelectJoin<POLine,
				InnerJoin<INItemPlan, On<INItemPlan.supplyPlanID, Equal<POLine.planID>>,
				InnerJoin<SOLineSplit, On<SOLineSplit.planID, Equal<INItemPlan.planID>, And<SOLineSplit.pOType, Equal<POLine.orderType>, And<SOLineSplit.pONbr, Equal<POLine.orderNbr>, And<SOLineSplit.pOLineNbr, Equal<POLine.lineNbr>>>>>>>,
			Where<POLine.orderType, Equal<Required<POLine.orderType>>, And<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
				And2<Where<POLine.cancelled, Equal<boolTrue>, Or<POLine.completed, Equal<boolTrue>>>,
				And<SOLineSplit.receivedQty, Less<SOLineSplit.qty>, And<SOLineSplit.pOCancelled, NotEqual<boolTrue>, And<SOLineSplit.completed, NotEqual<boolTrue>>>>>>>>.Select(graph, poOrder.OrderType, poOrder.OrderNbr))
			{
				POLine poline = res;
				INItemPlan plan = PXCache<INItemPlan>.CreateCopy(res);
				SOLineSplit parentschedule = PXCache<SOLineSplit>.CreateCopy(res);

				soorder.Current = (SOOrder)PXParentAttribute.SelectParent(solinesplit.Cache, parentschedule, typeof(SOOrder));

				if (parentschedule.Completed != true && parentschedule.POCancelled != true && parentschedule.BaseQty > parentschedule.BaseReceivedQty)
				{
					bool cancelDropShip = poline.Cancelled == true && POLineType.IsDropShip(poline.LineType);

					UpdateSchedulesFromCompletedPO(graph, solinesplit, initemplan, parentschedule, soorder, plan, cancelDropShip);

					if (initemplan.Cache.GetStatus(plan) != PXEntryStatus.Inserted)
					{
						initemplan.Delete(plan);
					}

					solinesplit.Cache.SetStatus(parentschedule, PXEntryStatus.Notchanged);
					parentschedule = PXCache<SOLineSplit>.CreateCopy(parentschedule);

					parentschedule.PlanID = null;
					parentschedule.Completed = true;
					parentschedule.POCompleted = true;
					parentschedule.POCancelled = true;
					solinesplit.Cache.Update(parentschedule);
				}
			}
		}

		private static void UpdateSchedulesFromCompletedPO(PXGraph graph, PXSelect<SOLineSplit> solinesplit, PXSelect<INItemPlan> initemplan, SOLineSplit parentschedule, PXSelect<SOOrder> soorder, INItemPlan demand, bool cancelDropShip = false)
		{
			graph.FieldDefaulting.AddHandler<SOLineSplit.locationID>((sender, e) =>
			{
				if (e.Row != null && ((SOLineSplit)e.Row).RequireLocation != true)
				{
					e.NewValue = null;
					e.Cancel = true;
				}
			});

			SOLineSplit newschedule = PXCache<SOLineSplit>.CreateCopy(parentschedule);

			ClearScheduleReferences(ref newschedule);

			newschedule.LotSerialNbr = demand.LotSerialNbr;
			newschedule.SiteID = demand.SiteID;

			decimal? processedBaseQty = (parentschedule.POSource == INReplenishmentSource.DropShipToOrder ? parentschedule.BaseShippedQty : parentschedule.BaseReceivedQty);
			newschedule.BaseQty = parentschedule.BaseQty - processedBaseQty;
			newschedule.Qty = INUnitAttribute.ConvertFromBase(solinesplit.Cache, newschedule.InventoryID, newschedule.UOM, (decimal)newschedule.BaseQty, INPrecision.QUANTITY);
			newschedule.BaseReceivedQty = 0m;
			newschedule.ReceivedQty = 0m;
			newschedule.BaseShippedQty = 0m;
			newschedule.ShippedQty = 0m;

			if (cancelDropShip)
			{
				newschedule.POCreate = true;
				newschedule.POSource = INReplenishmentSource.DropShipToOrder;
			}

			//creating new plan
			INItemPlan newPlan = PXCache<INItemPlan>.CreateCopy(demand);
			newPlan.PlanID = null;
			newPlan.SupplyPlanID = null;
			newPlan.DemandPlanID = null;
			newPlan.PlanQty = newschedule.BaseQty;
			newPlan.VendorID = cancelDropShip ? demand.VendorID : null;
			newPlan.VendorLocationID = cancelDropShip ? demand.VendorLocationID : null;
			newPlan.FixedSource = cancelDropShip ? INReplenishmentSource.Purchased : INReplenishmentSource.None;
			newPlan.PlanType = cancelDropShip ? demand.PlanType :
			  (soorder.Current?.Hold == true) ? INPlanConstants.Plan69 : INPlanConstants.Plan60;
			newPlan = (INItemPlan)initemplan.Cache.Insert(newPlan);

			newschedule.PlanID = newPlan.PlanID;
			solinesplit.Cache.Insert(newschedule);
		}

		public static void ClearScheduleReferences(ref SOLineSplit schedule)
		{
			schedule.ParentSplitLineNbr = schedule.SplitLineNbr;
			schedule.SplitLineNbr = null;
			schedule.Completed = false;
			schedule.PlanID = null;

			schedule.ClearPOFlags();
			schedule.ClearPOReferences();
			schedule.POSource = INReplenishmentSource.None;

			schedule.ClearSOReferences();

			schedule.RefNoteID = null;
		}
		public virtual UpdateIfFieldsChangedScope GetPriceCalculationScope()
			=> new SOOrderPriceCalculationScope();

		#region Well-known extensions
		public SOQuickProcess SOQuickProcessExt => GetExtension<SOQuickProcess>();
		public class SOQuickProcess : PXGraphExtension<SOOrderEntry>
		{
			public static bool IsActive() => true;

			[PXLocalizable]
			public static class Msg
			{
				public const string DoNotEmail = "Invoice will be emailed during quick processing though the {0} customer does not require sending invoices by email.";
				public const string DoEmail = "Invoice emailing will be skipped during quick processing though the {0} customer requires sending invoices by email.";
				public const string DoNotPrint = "Invoice will be printed during quick processing though the {0} customer does not require printing invoices.";
				public const string DoPrint = "Invoice printing will be skipped during quick processing though the {0} customer requires printing invoices.";
				public const string CannotShip = "The order cannot be shipped. See availability section for more info.";
				public const string OnlyCurrentShipmentWillBeInvoiced = "Only the shipment created by this process will be invoiced.";
				public const string SomeLinesWillBeSkipedDueToDateSelection = "This date selection will skip {0} open sales order lines from shipment.";
			}

			public virtual IEnumerable<SOOrder> ButtonHandler(PXAdapter adapter)
			{
				QuickProcessParameters.AskExt(InitQuickProcessPanel);
				Base.Save.Press();
				PXQuickProcess.Start(Base, Base.Document.Current, QuickProcessParameters.Current);
				return new[] { Base.Document.Current };
			}

			public PXAction<SOOrder> quickProcessOk;
			[PXButton, PXUIField(DisplayName = "OK")]
			public virtual IEnumerable QuickProcessOk(PXAdapter adapter) => adapter.Get();

			public PXFilter<SOQuickProcessParameters> QuickProcessParameters;

			/// <summary><see cref="SOOrder"/> Selected</summary>
			protected virtual void SOOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
			{
				Base.quickProcess.SetVisible(Base.soordertype.Current.AllowQuickProcess == true);
			}

			/// <summary><see cref="SOLine.SiteID"/> Updated</summary>
			protected virtual void SOLine_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				if (Base.soordertype.Current?.AllowQuickProcess == true && string.IsNullOrEmpty(QuickProcessParameters.Current?.OrderType) == false)
				{
					QuickProcessParameters.Current.SiteID = null;
				}
			}

			[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
			/// <summary><see cref="SOQuickProcessParameters.OrderType"/> CacheAttached</summary>
			protected virtual void SOQuickProcessParameters_OrderType_CacheAttached(PXCache cache) { }

			/// <summary><see cref="SOQuickProcessParameters.SiteID"/> Updated</summary>
			protected virtual void SOQuickProcessParameters_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				RecalculateAvailabilityStatus((SOQuickProcessParameters) e.Row);
				EnsureSiteID(sender, (SOQuickProcessParameters) e.Row);
			}

			/// <summary><see cref="SOQuickProcessParameters.ShipDate"/> Updated</summary>
			protected virtual void SOQuickProcessParameters_ShipDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e) => RecalculateAvailabilityStatus((SOQuickProcessParameters) e.Row);

			/// <summary><see cref="SOQuickProcessParameters"/> Inserted</summary>
			protected virtual void SOQuickProcessParameters_RowInserted(PXCache sender, PXRowInsertedEventArgs e) => sender.IsDirty = false;

			/// <summary><see cref="SOQuickProcessParameters"/> Updated</summary>
			protected virtual void SOQuickProcessParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e) => sender.IsDirty = false;

			/// <summary><see cref="SOQuickProcessParameters"/> Selected</summary>
			protected virtual void SOQuickProcessParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				quickProcessOk.SetEnabled(false);
				var row = (SOQuickProcessParameters)e.Row;
				if (row != null && Base.soordertype.Current?.AllowQuickProcess == true && string.IsNullOrEmpty(row.OrderType) == false)
				{
					VerifyPrepareInvoice(row);
					VerifyPrintInvoice(row);
					VerifyEmailInvoice(row);

					if (row.CreateShipment == true)
					{
						bool siteIsValid = (Base.Document.Current.OpenOrderQty > 0 && PXAccess.FeatureInstalled<FeaturesSet.warehouse>()).Implies(row.SiteID != null);

						String status = QuickProcessParameters.Cache.GetExtension<SOQuickProcessParametersAvailabilityExt>(row).AvailabilityStatus;
						Boolean canShip = status.IsIn(AvailabilityStatus.CanShipAll, AvailabilityStatus.CanShipPartBackOrder, AvailabilityStatus.CanShipPartCancelRemainder);

						sender.RaiseExceptionHandling<SOQuickProcessParameters.siteID>(
							row,
							row.SiteID,
							siteIsValid
								? null
								: new PXSetPropertyException(
									ErrorMessages.FieldIsEmpty,
									PXUIFieldAttribute.GetDisplayName<SOQuickProcessParameters.siteID>(sender),
									PXErrorLevel.Error));
						sender.RaiseExceptionHandling<SOQuickProcessParameters.createShipment>(
							row,
							row.CreateShipment,
							canShip && siteIsValid
								? null
								: new PXSetPropertyException(Msg.CannotShip, PXErrorLevel.Error));

						quickProcessOk.SetEnabled(canShip && siteIsValid);
					}
					else
					{
						quickProcessOk.SetEnabled(true);
					}
				}
			}

			protected virtual void EnsureSiteID(PXCache sender, SOQuickProcessParameters row)
			{
				if (row.SiteID == null)
				{
					Int32? preferedSiteID = Base.GetPreferedSiteID();
					if (preferedSiteID != null)
						sender.SetValueExt<SOQuickProcessParameters.siteID>(row, preferedSiteID);
				}
				row.SiteCD = (PXStringState) sender.GetStateExt<SOQuickProcessParameters.siteID>(row);
			}

			protected virtual void VerifyPrepareInvoice(SOQuickProcessParameters row)
			{
				if (row != null && Base.Document.Current != null)
				{
					SOOrder doc = Base.Document.Current;
					Boolean alreadyHasShipments = doc.ShipmentCntr - doc.OpenShipmentCntr - doc.BilledCntr - doc.ReleasedCntr > 0;
					QuickProcessParameters.Cache.RaiseExceptionHandling<SOQuickProcessParameters.prepareInvoiceFromShipment>(
						row,
						row.PrepareInvoiceFromShipment,
						row.PrepareInvoiceFromShipment == true && alreadyHasShipments
							? new PXSetPropertyException<SOQuickProcessParameters.prepareInvoiceFromShipment>(
								Msg.OnlyCurrentShipmentWillBeInvoiced, PXErrorLevel.Warning)
							: null);
				}
			}
			protected virtual void VerifyEmailInvoice(SOQuickProcessParameters row)
			{
				if (row != null && Base.customer.Current != null)
				{
					QuickProcessParameters.Cache.RaiseExceptionHandling<SOQuickProcessParameters.emailInvoice>(
						row,
						row.EmailInvoice,
						row.EmailInvoice != Base.customer.Current.MailInvoices
							? new PXSetPropertyException<SOQuickProcessParameters.emailInvoice>(
								row.EmailInvoice == true ? Msg.DoNotEmail : Msg.DoEmail,
								PXErrorLevel.Warning,
								Base.customer.Current.AcctCD)
							: null);
				}
			}
			protected virtual void VerifyPrintInvoice(SOQuickProcessParameters row)
			{
				if (row != null && Base.customer.Current != null)
				{
					Boolean? printInvoice = QuickProcessParameters.Cache.GetExtension<SOQuickProcessParametersReportsExt>(row).PrintInvoice;
					QuickProcessParameters.Cache.RaiseExceptionHandling<SOQuickProcessParametersReportsExt.printInvoice>(
						row,
						printInvoice,
						printInvoice != Base.customer.Current.PrintInvoices
							? new PXSetPropertyException<SOQuickProcessParametersReportsExt.printInvoice>(
								printInvoice == true ? Msg.DoNotPrint : Msg.DoPrint,
								PXErrorLevel.Warning,
								Base.customer.Current.AcctCD)
							: null);
				}
			}

			protected virtual Tuple<string, int> OrderAvailabilityStatus(int? SiteID, DateTime? ShipDate)
			{
				if (SiteID == null || ShipDate == null) return new Tuple<string, int>(AvailabilityStatus.NothingToShip, 0);

				var splits = PXSelect<SOLineSplit,
					Where<SOLineSplit.orderType, Equal<Current<SOOrder.orderType>>,
					And<SOLineSplit.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					And<SOLineSplit.completed, Equal<False>>>>>.Select(Base).RowCast<SOLineSplit>()
					.Where(s => (s.SiteID == SiteID || s.ToSiteID == SiteID && s.IsAllocated == true) &&
						(s.LineType == SOLineType.Inventory || s.LineType == SOLineType.NonInventory) &&
						s.RequireShipping == true &&
						s.Operation == SOOperation.Issue).ToList();

				Func<int?, int?, decimal?> availabilityFetch = (InventoryID, SubItemID) =>
				{
					if (!PXTransactionScope.IsScoped)
					{
						using (var cs = new PXConnectionScope())
						using (var ts = new PXTransactionScope())
						{
							try
							{
								Base.Caches[typeof(ItemLotSerial)].PersistInserted();
								Base.Caches[typeof(SiteLotSerial)].PersistInserted();
								Base.Caches[typeof(SiteStatus)].PersistInserted();
							}
							//TODO: create separate exception class for accumulators validations
							catch (PXException)
							{
								return decimal.MinValue;
							}
						}
					}

					SiteStatus delta = new SiteStatus { InventoryID = InventoryID, SiteID = SiteID, SubItemID = SubItemID };
					delta = (SiteStatus)Base.Caches[typeof(SiteStatus)].Insert(delta);

					bool allowShipNegQty = (delta.NegQty == true && Base.soordertype.Current.ShipFullIfNegQtyAllowed == true);
					if (allowShipNegQty)
					{
						INLotSerClass lotSerClass = INLotSerClass.PK.Find(Base, delta.LotSerClassID);
						if (lotSerClass?.LotSerTrack == INLotSerTrack.NotNumbered || lotSerClass?.LotSerAssign == INLotSerAssign.WhenUsed)
						{
							return decimal.MaxValue;
						}
					}

					InventoryItem inventoryItem = InventoryItem.PK.Find(Base, InventoryID);
					// Impossible to perform availability validation in Quick Process for non-stock kits. Availability validation will be ignored.
					if (inventoryItem.StkItem != true && inventoryItem.KitItem == true) return decimal.MaxValue;

					SiteStatus status = PXSelectReadonly<SiteStatus, Where<SiteStatus.inventoryID, Equal<Required<SiteStatus.inventoryID>>,
						And<SiteStatus.siteID, Equal<Required<SiteStatus.siteID>>,
						And<SiteStatus.subItemID, Equal<Required<SiteStatus.subItemID>>>>>>.Select(Base, InventoryID, SiteID, SubItemID);

					if (status != null)
					{
						return status.QtyHardAvail + delta.QtyHardAvail;
					}

					return delta.QtyHardAvail;
				};

				var splitsgrouped = splits
					.GroupBy(t => new
					{
						t.InventoryID,
						t.SiteID,
						t.SubItemID,
						Allocated = t.IsAllocated == true && t.SiteID == SiteID,
						RemoteAllocated = t.IsAllocated == true && t.SiteID != SiteID,
						MarkedForPO = t.POCreate == true,
						NonStock = t.LineType == SOLineType.NonInventory,
						FutureShipments = t.ShipDate > ShipDate
					})
					.Select(tg => new
					{
						Item = tg.Key,
						SumBaseQty = tg.Sum(q => q.BaseQty),
						SumDeduction =
							tg.Key.Allocated == false &&
							tg.Key.MarkedForPO == false &&
							tg.Key.NonStock == false &&
							tg.Key.FutureShipments == false ? tg.Sum(q => q.BaseQty) : 0m,
						AvailableForShipping =
							tg.Key.RemoteAllocated == false &&
							tg.Key.MarkedForPO == false &&
							tg.Key.NonStock == false &&
							tg.Key.FutureShipments == false ? availabilityFetch(tg.Key.InventoryID, tg.Key.SubItemID) : 0m
					}).ToList();

				var availableForShipping = splitsgrouped.Where(s => !s.Item.FutureShipments && s.SumBaseQty > 0m && (s.AvailableForShipping > 0 || s.Item.NonStock || s.Item.Allocated));
				var remoteAllocated = splitsgrouped.Where(s => !s.Item.FutureShipments && s.SumBaseQty > 0m && s.Item.RemoteAllocated);
				var markedForPO = splitsgrouped.Where(s => !s.Item.FutureShipments && s.SumBaseQty > 0m && s.Item.MarkedForPO);
				var futureShipments = splitsgrouped.Where(s => s.SumBaseQty > 0m && s.Item.FutureShipments);
				var notAvailableForShipping = splitsgrouped.Where(s => !s.Item.FutureShipments && s.SumDeduction > s.AvailableForShipping && !s.Item.NonStock);

				if (!availableForShipping.Any())
				{
					return new Tuple<string, int>(AvailabilityStatus.NothingToShip, 0);
				}

				int skippedLinesCount = 0;

				if (futureShipments.Any())
				{
					skippedLinesCount = splits
						.Where(line => line.ShipDate > ShipDate)
						.GroupBy(s => new { s.LineNbr })
						.Count();
				}

				if (notAvailableForShipping.Any() || markedForPO.Any() || remoteAllocated.Any())
				{
					switch (Base.Document.Current.ShipComplete)
					{
						case SOShipComplete.ShipComplete:
							return new Tuple<string, int>(AvailabilityStatus.NoItemsAvailableToShip, skippedLinesCount);
						case SOShipComplete.CancelRemainder:
							return new Tuple<string, int>(AvailabilityStatus.CanShipPartCancelRemainder, skippedLinesCount);
						case SOShipComplete.BackOrderAllowed:
							return new Tuple<string, int>(AvailabilityStatus.CanShipPartBackOrder, skippedLinesCount);
					}
				}

				return new Tuple<string, int>(AvailabilityStatus.CanShipAll, skippedLinesCount);
			}

			private void RecalculateAvailabilityStatus(SOQuickProcessParameters row)
			{
				DateTime? shipDate = QuickProcessParameters.Cache.GetExtension<SOQuickProcessParametersShipDateExt>(row).ShipDate;
				var status = OrderAvailabilityStatus(row.SiteID, shipDate);
				QuickProcessParameters.Cache.SetValueExt<SOQuickProcessParametersAvailabilityExt.availabilityStatus>(row, status.Item1);
				QuickProcessParameters.Cache.SetValueExt<SOQuickProcessParametersAvailabilityExt.skipByDateMsg>(row, status.Item2 == 0 ? "" : PXLocalizer.LocalizeFormat(Msg.SomeLinesWillBeSkipedDueToDateSelection, status.Item2));
				QuickProcessParameters.Cache.RaiseRowSelected(QuickProcessParameters.Current);
			}

			public static void InitQuickProcessPanel(PXGraph graph, string viewName)
			{
				var ext = ((SOOrderEntry)graph).SOQuickProcessExt;
				if (string.IsNullOrEmpty(ext.QuickProcessParameters.Current.OrderType))
				{
					ext.QuickProcessParameters.Cache.Clear();
					ext.QuickProcessParameters.Insert(PXSelectReadonly<SOQuickProcessParameters, Where<SOQuickProcessParameters.orderType, Equal<Current<SOOrder.orderType>>>>.Select(ext.Base));
				}

				if (ext.QuickProcessParameters.Current.CreateShipment == true)
				{
					ext.EnsureSiteID(ext.QuickProcessParameters.Cache, ext.QuickProcessParameters.Current);
					DateTime? shipDate = ext.Base.Accessinfo.BusinessDate > ext.Base.Document.Current.ShipDate ? ext.Base.Accessinfo.BusinessDate : ext.Base.Document.Current.ShipDate;
					SOQuickProcessParametersShipDateExt.SetDate(ext.QuickProcessParameters.Cache, ext.QuickProcessParameters.Current, shipDate.Value);
					ext.RecalculateAvailabilityStatus(ext.QuickProcessParameters.Current);
				}
			}
		}

		public CarrierRates CarrierRatesExt => FindImplementation<CarrierRates>();
		public class CarrierRates : CarrierRatesExtension<SOOrderEntry, SOOrder>
		{
			public virtual void RecalculatePackagesForOrder(SOOrder order) => base.RecalculatePackagesForOrder(Documents.Cache.GetExtension<Document>(order));
			public virtual CarrierRequest BuildRateRequest(SOOrder order) => base.BuildRateRequest(Documents.Cache.GetExtension<Document>(order));
			public virtual CarrierRequest BuildQuoteRequest(SOOrder order, CarrierPlugin plugin) => base.BuildQuoteRequest(Documents.Cache.GetExtension<Document>(order), plugin);

			protected override DocumentMapping GetDocumentMapping() => new DocumentMapping(typeof(SOOrder)) { DocumentDate = typeof(SOOrder.orderDate) };

			protected override CarrierRequest GetCarrierRequest(Document doc, UnitsType unit, List<string> methods, List<CarrierBoxEx> boxes)
			{
				var order = (SOOrder)Documents.Cache.GetMain(doc);

				SOShippingAddress shipAddress = Base.Shipping_Address.Select();
				BAccount companyAccount = PXSelectJoin<BAccountR, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(Base, Base.Accessinfo.BranchID);
				Address companyAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, companyAccount.DefAddressID);

				CarrierRequest cr = new CarrierRequest(unit, order.CuryID);
				cr.Shipper = companyAddress;
				cr.Origin = null;
				cr.Destination = shipAddress;
				cr.PackagesEx = boxes;
				cr.Resedential = order.Resedential == true;
				cr.SaturdayDelivery = order.SaturdayDelivery == true;
				cr.Insurance = order.Insurance == true;
				cr.ShipDate = order.ShipDate.Value;
				cr.Methods = methods;
				cr.Attributes = new List<string>();
				cr.InvoiceLineTotal = Base.Document.Current.CuryLineTotal.GetValueOrDefault();

				if (order.GroundCollect == true && Base.CanUseGroundCollect(order))
					cr.Attributes.Add("COLLECT");

				return cr;
			}

			protected override IList<SOPackageEngine.PackSet> GetPackages(Document doc, bool suppressRecalc)
			{
				var order = (SOOrder)Documents.Cache.GetMain(doc);
				return order.IsPackageValid == true || order.IsManualPackage == true || suppressRecalc
					? GetPackages(order)
					: CalculatePackages(doc, null);
			}

			protected virtual IList<SOPackageEngine.PackSet> GetPackages(SOOrder order)
			{
				Dictionary<int, SOPackageEngine.PackSet> packs = new Dictionary<int, SOPackageEngine.PackSet>();
				foreach (SOPackageInfoEx package in Base.Packages.View.SelectMultiBound(new object[] { order }))
				{
					SOPackageEngine.PackSet set = null;
					if (!packs.ContainsKey(package.SiteID.Value))
					{
						set = new SOPackageEngine.PackSet(package.SiteID.Value);
						packs.Add(set.SiteID, set);
					}
					else
					{
						set = packs[package.SiteID.Value];
					}

					set.Packages.Add(package);
				}

				return packs.Values.ToList();
			}

			protected override void ValidatePackages()
			{
				if (Base.Document.Current.IsManualPackage == true)
				{
					PXResultset<SOPackageInfoEx> resultset = Base.Packages.Select();

					if (resultset.Count == 0)
						throw new PXException(Messages.AtleastOnePackageIsRequired);
					else
					{
						bool failed = false;
						foreach (SOPackageInfoEx p in resultset)
						{
							if (p.SiteID == null)
							{
								Base.Packages.Cache.RaiseExceptionHandling<SOPackageInfoEx.siteID>(p, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error, typeof(SOPackageInfoEx.siteID).Name));
								failed = true;
							}
						}
						if (failed)
							throw new PXException(Messages.AtleastOnePackageIsInvalid);
					}
				}
			}

			protected override void RateHasBeenSelected(SOCarrierRate cr)
			{
				if (Base.CollectFreight)
				{
					decimal baseCost = Base.ConvertAmtToBaseCury(Base.Document.Current.CuryID, arsetup.Current.DefaultRateTypeID, Base.Document.Current.OrderDate.Value, cr.Amount.Value);
					Base.SetFreightCost(baseCost);
				}
			}

			protected override WebDialogResult AskForRateSelection() => Base.DocumentProperties.AskExt();

			protected override void ClearPackages(Document doc)
			{
				foreach (SOPackageInfoEx package in Base.Packages.View.SelectMultiBound(new object[] { Documents.Cache.GetMain(doc) }))
					Base.Packages.Delete(package);
			}

			protected override void InsertPackages(IEnumerable<SOPackageInfoEx> packages)
			{
				foreach (SOPackageInfoEx package in packages)
					Base.Packages.Insert(package);
			}

			protected override IEnumerable<Tuple<ILineInfo, InventoryItem>> GetLines(Document doc)
			{
				var order = (SOOrder)Documents.Cache.GetMain(doc);

				return 
					PXSelectJoin<SOLine,
					InnerJoin<InventoryItem, On<SOLine.FK.InventoryItem>>,
					Where<SOLine.orderType, Equal<Required<SOOrder.orderType>>,
						And<SOLine.orderNbr, Equal<Required<SOOrder.orderNbr>>>>,
					OrderBy<Asc<SOLine.orderType, Asc<SOLine.orderNbr, Asc<SOLine.lineNbr>>>>>
					.Select(Base, order.OrderType, order.OrderNbr).AsEnumerable()
					.Cast<PXResult<SOLine, InventoryItem>>()
					.Select(r => Tuple.Create<ILineInfo, InventoryItem>(new LineInfo(r), r));
			}

			protected override IEnumerable<CarrierPlugin> GetApplicableCarrierPlugins()
			{
				var orderSites = new Lazy<SOOrderSite[]>(() => Base.SiteList.Select().RowCast<SOOrderSite>().ToArray());
				return base.GetApplicableCarrierPlugins()
					.Where(p => p.SiteID == null || orderSites.Value.Any(s => s.SiteID == p.SiteID));
			}

			private class LineInfo : ILineInfo
			{
				private SOLine _line;
				public LineInfo(SOLine line) { _line = line; }

				public decimal? BaseQty => _line.BaseQty;
				public decimal? CuryLineAmt => _line.CuryLineAmt;
				public decimal? ExtWeight => _line.ExtWeight;
				public int? SiteID => _line.SiteID;
				public string Operation => _line.Operation;
			}
		}
		#endregion
	}

	[Serializable()]
	public partial class AddInvoiceFilter : IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		protected String _DocType;
		[PXString(3, IsFixed = true)]
		[PXDefault(ARDocType.Invoice)]
		[PXStringList(
				new string[] { ARDocType.Invoice, ARDocType.CashSale, ARDocType.DebitMemo },
				new string[] { AR.Messages.Invoice, AR.Messages.CashSale, AR.Messages.DebitMemo })]
		[PXUIField(DisplayName = "Type")]
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
		protected string _RefNbr;
		[PXString(15, IsUnicode = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[ARInvoiceType.RefNbr(typeof(Search2<AR.Standalone.ARRegisterAlias.refNbr,
			InnerJoinSingleTable<ARInvoice, On<ARInvoice.docType, Equal<AR.Standalone.ARRegisterAlias.docType>,
				And<ARInvoice.refNbr, Equal<AR.Standalone.ARRegisterAlias.refNbr>>>>,
			Where<AR.Standalone.ARRegisterAlias.docType, Equal<Optional<AddInvoiceFilter.docType>>,
				And<AR.Standalone.ARRegisterAlias.released, Equal<boolTrue>,
				And<AR.Standalone.ARRegisterAlias.origModule, Equal<BatchModule.moduleSO>,
				And<AR.Standalone.ARRegisterAlias.customerID, Equal<Current<SOOrder.customerID>>>>>>,
			OrderBy<Desc<AR.Standalone.ARRegisterAlias.refNbr>>>), Filterable = true)]
		[PXRestrictor(typeof(Where<ARRegisterAlias.canceled, NotEqual<True>>),
			Messages.InvoiceCanceled, typeof(ARRegisterAlias.refNbr))]
		[PXRestrictor(typeof(Where<ARRegisterAlias.isUnderCorrection, NotEqual<True>>),
			Messages.InvoiceUnderCorrection, typeof(ARRegisterAlias.refNbr))]
		[PXFormula(typeof(Default<AddInvoiceFilter.docType>))]
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
		#region Expand
		public abstract class expand : PX.Data.BQL.BqlBool.Field<expand> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Show Non-Stock Kits by Components")]
		public virtual Boolean? Expand
		{
			get; set;
		}
		#endregion
	}
	[Serializable]
	public partial class SOParamFilter : IBqlTable
	{
		#region ShipDate
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
		protected DateTime? _ShipDate;
		[PXDate]
		[PXUIField(DisplayName = "Shipment Date", Required = true)]
		public virtual DateTime? ShipDate
		{
			get
			{
				return this._ShipDate;
			}
			set
			{
				this._ShipDate = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse ID", Required = true, FieldClass = SiteAttribute.DimensionName)]
		[OrderSiteSelector()]
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
	}

	[Serializable()]
	public partial class CopyParamFilter : IBqlTable
	{
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXDefault(typeof(Search<SOSetup.defaultOrderType>))]
		[PXSelector(typeof(Search2<SOOrderType.orderType, 
			InnerJoin<SOOrderTypeOperation, 
				On2<SOOrderTypeOperation.FK.OrderType, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>>>))]
		[PXRestrictor(typeof(Where<SOOrderTypeOperation.iNDocType, NotEqual<INTranType.transfer>, Or<FeatureInstalled<FeaturesSet.warehouse>>>), ErrorMessages.ElementDoesntExist, typeof(CopyParamFilter.orderType))]
		[PXRestrictor(typeof(Where<SOOrderType.requireAllocation, NotEqual<True>, Or<AllocationAllowed>>), ErrorMessages.ElementDoesntExist, typeof(CopyParamFilter.orderType))]
		[PXRestrictor(typeof(Where<SOOrderType.active, Equal<True>>), ErrorMessages.ElementDoesntExist, typeof(CopyParamFilter.orderType))]
		[PXUIField(DisplayName = "Order Type")]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region RecalcUnitPrices
		public abstract class recalcUnitPrices : PX.Data.BQL.BqlBool.Field<recalcUnitPrices> { }
		protected Boolean? _RecalcUnitPrices;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Recalculate Unit Prices")]
		public virtual Boolean? RecalcUnitPrices
		{
			get
			{
				return this._RecalcUnitPrices;
			}
			set
			{
				this._RecalcUnitPrices = value;
			}
		}
		#endregion
		#region OverrideManualPrices
		public abstract class overrideManualPrices : PX.Data.BQL.BqlBool.Field<overrideManualPrices> { }
		protected Boolean? _OverrideManualPrices;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Manual Prices", Visible = false)]
		public virtual Boolean? OverrideManualPrices
		{
			get
			{
				return this._OverrideManualPrices;
			}
			set
			{
				this._OverrideManualPrices = value;
			}
		}
		#endregion
		#region RecalcDiscounts
		public abstract class recalcDiscounts : PX.Data.BQL.BqlBool.Field<recalcDiscounts> { }
		protected Boolean? _RecalcDiscounts;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Recalculate Discounts")]
		public virtual Boolean? RecalcDiscounts
		{
			get
			{
				return this._RecalcDiscounts;
			}
			set
			{
				this._RecalcDiscounts = value;
			}
		}
		#endregion
		#region OverrideManualDiscounts
		public abstract class overrideManualDiscounts : PX.Data.BQL.BqlBool.Field<overrideManualDiscounts> { }
		protected Boolean? _OverrideManualDiscounts;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Manual Discounts")]
		public virtual Boolean? OverrideManualDiscounts
		{
			get
			{
				return this._OverrideManualDiscounts;
			}
			set
			{
				this._OverrideManualDiscounts = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search2<Numbering.newSymbol, InnerJoin<SOOrderType, On<Numbering.numberingID, Equal<SOOrderType.orderNumberingID>>>, Where<SOOrderType.orderType, Equal<Current<CopyParamFilter.orderType>>, And<Numbering.userNumbering, Equal<False>>>>))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
	}

	[Serializable()]
	[PXProjection(typeof(Select<POLine>), Persistent = true)]
	public partial class POLine3 : PX.Data.IBqlTable, ISortOrder
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
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
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(POLine.orderType))]
		[PXDefault()]
		[PXUIField(DisplayName = "PO Type", Enabled = false)]
		[PO.POOrderType.List()]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POLine.orderNbr))]
		[PXDefault()]
		[PXUIField(DisplayName = "PO Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Current<POLine3.orderType>>>>), DescriptionField = typeof(POOrder.orderDesc))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region VendorRefNbr
		public abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr> { }
		protected String _VendorRefNbr;
		[PXString(40)]
		[PXUIField(DisplayName = "Vendor Ref.", Enabled = false)]
		public virtual String VendorRefNbr
		{
			get
			{
				return this._VendorRefNbr;
			}
			set
			{
				this._VendorRefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(POLine.lineNbr))]
		[PXDefault()]
		[PXUIField(DisplayName = "PO Line Nbr.", Visible = false)]
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
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected Int32? _SortOrder;
		[PXDBInt(BqlField = typeof(POLine.sortOrder))]
		public virtual Int32? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POLine.lineType))]
		[PO.POLineType.List()]
		[PXUIField(DisplayName = "Line Type", Enabled = false)]
		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(Filterable = true, BqlField = typeof(POLine.inventoryID), Enabled = false)]
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
		[SubItem(BqlField = typeof(POLine.subItemID), Enabled = false)]
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
		#region PlanID
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		protected Int64? _PlanID;
		[PXDBLong(BqlField = typeof(POLine.planID))]
		[PXUIField(Visible = false, Enabled = false)]
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
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[AP.Vendor(typeof(Search<BAccountR.bAccountID,
			Where<Vendor.type, NotEqual<BAccountType.employeeType>>>),
			BqlField = typeof(POLine.vendorID), Enabled = false)]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
		protected DateTime? _OrderDate;
		[PXDBDate(BqlField = typeof(POLine.orderDate))]
		[PXUIField(DisplayName = "Order Date", Enabled = false)]
		public virtual DateTime? OrderDate
		{
			get
			{
				return this._OrderDate;
			}
			set
			{
				this._OrderDate = value;
			}
		}
		#endregion
		#region PromisedDate
		public abstract class promisedDate : PX.Data.BQL.BqlDateTime.Field<promisedDate> { }
		protected DateTime? _PromisedDate;
		[PXDBDate(BqlField = typeof(POLine.promisedDate))]
		[PXUIField(DisplayName = "Promised", Enabled = false)]
		public virtual DateTime? PromisedDate
		{
			get
			{
				return this._PromisedDate;
			}
			set
			{
				this._PromisedDate = value;
			}
		}
		#endregion
		#region Cancelled
		public abstract class cancelled : PX.Data.BQL.BqlBool.Field<cancelled> { }
		protected Boolean? _Cancelled;
		[PXDBBool(BqlField = typeof(POLine.cancelled))]
		public virtual Boolean? Cancelled
		{
			get
			{
				return this._Cancelled;
			}
			set
			{
				this._Cancelled = value;
			}
		}
		#endregion
		#region Completed
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		[PXDBBool(BqlField = typeof(POLine.completed))]
		public virtual bool? Completed
			{
			get;
			set;
			}
		#endregion
		#region Closed
		public abstract class closed : PX.Data.BQL.BqlBool.Field<closed> { }
		[PXDBBool(BqlField = typeof(POLine.closed))]
		public virtual bool? Closed
		{
			get;
			set;
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDBInt(BqlField = typeof(POLine.siteID))]
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
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDBString(6, IsUnicode = true, BqlField = typeof(POLine.uOM))]
		[PXUIField(DisplayName = "UOM", Enabled = false)]
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
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;
		[PXDBQuantity(BqlField = typeof(POLine.orderQty))]
		[PXUIField(DisplayName = "Order Qty.", Enabled = false)]
		public virtual Decimal? OrderQty
		{
			get
			{
				return this._OrderQty;
			}
			set
			{
				this._OrderQty = value;
			}
		}
		#endregion
		#region BaseOrderQty
		public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }
		protected Decimal? _BaseOrderQty;
		[PXDBQuantity(BqlField = typeof(POLine.baseOrderQty))]
		public virtual Decimal? BaseOrderQty
		{
			get
			{
				return this._BaseOrderQty;
			}
			set
			{
				this._BaseOrderQty = value;
			}
		}
		#endregion
		#region OpenQty
		public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
		protected Decimal? _OpenQty;
		[PXDBQuantity(BqlField = typeof(POLine.openQty))]
		[PXUIField(DisplayName = "Open Qty.", Enabled = false)]
		public virtual Decimal? OpenQty
		{
			get
			{
				return this._OpenQty;
			}
			set
			{
				this._OpenQty = value;
			}
		}
		#endregion
		#region BaseOpenQty
		public abstract class baseOpenQty : PX.Data.BQL.BqlDecimal.Field<baseOpenQty> { }
		protected Decimal? _BaseOpenQty;
		[PXDBDecimal(6, BqlField = typeof(POLine.baseOpenQty))]
		public virtual Decimal? BaseOpenQty
		{
			get
			{
				return this._BaseOpenQty;
			}
			set
			{
				this._BaseOpenQty = value;
			}
		}
		#endregion
		#region ReceivedQty
		public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
		protected Decimal? _ReceivedQty;
		[PXDBDecimal(6, BqlField = typeof(POLine.receivedQty))]
		public virtual Decimal? ReceivedQty
		{
			get
			{
				return this._ReceivedQty;
			}
			set
			{
				this._ReceivedQty = value;
			}
		}
		#endregion
		#region BaseReceivedQty
		public abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty> { }
		protected Decimal? _BaseReceivedQty;
		[PXDBDecimal(6, BqlField = typeof(POLine.baseReceivedQty))]
		public virtual Decimal? BaseReceivedQty
		{
			get
			{
				return this._BaseReceivedQty;
			}
			set
			{
				this._BaseReceivedQty = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(POLine.tranDesc))]
		[PXUIField(DisplayName = "Line Description", Enabled = false)]
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
		#region ReceiptStatus
		public abstract class receiptStatus : PX.Data.BQL.BqlString.Field<receiptStatus> { }
		protected String _ReceiptStatus;
		[PXDBString(1, IsFixed = true, BqlField = typeof(POLine.receiptStatus))]
		public virtual String ReceiptStatus
		{
			get
			{
				return this._ReceiptStatus;
			}
			set
			{
				this._ReceiptStatus = value;
			}
		}
		#endregion
		#region SOOrderType
		public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
		protected String _SOOrderType;
		[PXString(2, IsFixed = true)]
		public virtual String SOOrderType
		{
			get
			{
				return this._SOOrderType;
			}
			set
			{
				this._SOOrderType = value;
			}
		}
		#endregion
		#region SOOrderNbr
		public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
		protected String _SOOrderNbr;
		[PXString(15, IsUnicode = true)]
		public virtual String SOOrderNbr
		{
			get
			{
				return this._SOOrderNbr;
			}
			set
			{
				this._SOOrderNbr = value;
			}
		}
		#endregion
		#region SOOrderLineNbr
		public abstract class sOOrderLineNbr : PX.Data.BQL.BqlInt.Field<sOOrderLineNbr> { }
		protected Int32? _SOOrderLineNbr;
		[PXInt()]
		public virtual Int32? SOOrderLineNbr
		{
			get
			{
				return this._SOOrderLineNbr;
			}
			set
			{
				this._SOOrderLineNbr = value;
			}
		}
		#endregion
		#region LinkedToCurrentSOLine
		public abstract class linkedToCurrentSOLine : PX.Data.BQL.BqlBool.Field<linkedToCurrentSOLine> { }

		[PXBool()]
		public virtual bool? LinkedToCurrentSOLine
		{
			get;
			set;
		}
		#endregion
		#region SOOrderSplitLineNbr
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2020R2 + "LinkedToCurrentSOLine field should be used instead.")]
		public abstract class sOOrderSplitLineNbr : PX.Data.BQL.BqlInt.Field<sOOrderSplitLineNbr> { }
		protected Int32? _SOOrderSplitLineNbr;
		[PXInt()]
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2020R2 + "LinkedToCurrentSOLine field should be used instead.")]
		public virtual Int32? SOOrderSplitLineNbr
		{
			get
			{
				return this._SOOrderSplitLineNbr;
			}
			set
			{
				this._SOOrderSplitLineNbr = value;
			}
		}
		#endregion
		#region DemandQty
		public abstract class demandQty : PX.Data.BQL.BqlDecimal.Field<demandQty> { }
		protected Decimal? _DemandQty;
		[PXDecimal(6)]
		public virtual Decimal? DemandQty
		{
			get
			{
				return this._DemandQty;
			}
			set
			{
				this._DemandQty = value;
			}
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(BqlField = typeof(POLine.Tstamp))]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID(BqlField = typeof(POLine.lastModifiedByID))]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID(BqlField = typeof(POLine.lastModifiedByScreenID))]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime(BqlField = typeof(POLine.lastModifiedDateTime))]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}

	[Serializable]
	public partial class SOSiteStatusFilter : INSiteStatusFilter
	{
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		[PXUIField(DisplayName = "Warehouse")]
		[SiteAttribute]
		[InterBranchRestrictor(typeof(Where2<SameOrganizationBranch<INSite.branchID, Current<SOOrder.branchID>>,
			Or<Current<SOOrder.behavior>, Equal<SOBehavior.qT>>>))]
		[PXDefault(typeof(INRegister.siteID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region Inventory
		public new abstract class inventory : PX.Data.BQL.BqlString.Field<inventory> { }
		#endregion
		#region Mode
		public abstract class mode : PX.Data.BQL.BqlInt.Field<mode> { }
		protected int? _Mode;
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Selection Mode")]
		[SOAddItemMode.List]
		public virtual int? Mode
		{
			get
			{
				return _Mode;
			}
			set
			{
				_Mode = value;
			}
		}
		#endregion
		#region HistoryDate
		public abstract class historyDate : PX.Data.BQL.BqlDateTime.Field<historyDate> { }
		protected DateTime? _HistoryDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Sold Since")]
		public virtual DateTime? HistoryDate
		{
			get
			{
				return this._HistoryDate;
			}
			set
			{
				this._HistoryDate = value;
			}
		}
		#endregion
		#region DropShipSales
		public abstract class dropShipSales : PX.Data.BQL.BqlBool.Field<dropShipSales> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXFormula(typeof(Default<mode>))]
		[PXUIField(DisplayName = "Show Drop-Ship Sales")]
		public virtual bool? DropShipSales
		{
			get;
			set;
		}
		#endregion
	}

	public class SOAddItemMode
	{
		public const int BySite = 0;
		public const int ByCustomer = 1;

		public class ListAttribute : PXIntListAttribute
		{
			public ListAttribute() : base(
				new[]
			{
					Pair(BySite, Messages.BySite),
					Pair(ByCustomer, Messages.ByCustomer),
				})
			{ }
		}

		public class bySite : PX.Data.BQL.BqlInt.Constant<bySite> { public bySite() : base(BySite) { } }
		public class byCustomer : PX.Data.BQL.BqlInt.Constant<byCustomer> { public byCustomer() : base(ByCustomer) { } }
	}

	[System.SerializableAttribute()]
	[PXProjection(typeof(Select2<InventoryItem,
		LeftJoin<INSiteStatus,
						On<INSiteStatus.inventoryID, Equal<InventoryItem.inventoryID>,
						And<InventoryItem.stkItem, Equal<boolTrue>,
						And<INSiteStatus.siteID, NotEqual<SiteAttribute.transitSiteID>>>>,
		LeftJoin<INSubItem,
						On<INSiteStatus.FK.SubItem>,
		LeftJoin<INSite,
						On<INSiteStatus.FK.Site>,
		LeftJoin<INItemXRef,
						On<INItemXRef.inventoryID, Equal<InventoryItem.inventoryID>,
						And2<Where<INItemXRef.subItemID, Equal<INSiteStatus.subItemID>,
								Or<INSiteStatus.subItemID, IsNull>>,
						And<Where<CurrentValue<SOSiteStatusFilter.barCode>, IsNotNull,
						And<INItemXRef.alternateType, Equal<INAlternateType.barcode>>>>>>,
		LeftJoin<INItemPartNumber,
						On<INItemPartNumber.inventoryID, Equal<InventoryItem.inventoryID>,
						And<INItemPartNumber.alternateID, Like<CurrentValue<SOSiteStatusFilter.inventory_Wildcard>>,
						And2<Where<INItemPartNumber.bAccountID, Equal<Zero>,
									  Or<INItemPartNumber.bAccountID, Equal<CurrentValue<SOOrder.customerID>>,
										Or<INItemPartNumber.alternateType, Equal<INAlternateType.vPN>>>>,
						And<Where<INItemPartNumber.subItemID, Equal<INSiteStatus.subItemID>,
								   Or<INSiteStatus.subItemID, IsNull>>>>>>,
		LeftJoin<INItemClass,
						On<InventoryItem.FK.ItemClass>,
		LeftJoin<INPriceClass,
						On<INPriceClass.priceClassID, Equal<InventoryItem.priceClassID>>,
		LeftJoin<BAccountR,
						On<BAccountR.bAccountID, Equal<InventoryItem.preferredVendorID>>,
		LeftJoin<INItemCustSalesStats,
				  On<CurrentValue<SOSiteStatusFilter.mode>, Equal<SOAddItemMode.byCustomer>,
							And<INItemCustSalesStats.inventoryID, Equal<InventoryItem.inventoryID>,
							And<INItemCustSalesStats.subItemID, Equal<INSiteStatus.subItemID>,
							And<INItemCustSalesStats.siteID, Equal<INSiteStatus.siteID>,
							And<INItemCustSalesStats.bAccountID, Equal<CurrentValue<SOOrder.customerID>>,
							And<Where<INItemCustSalesStats.lastDate, GreaterEqual<CurrentValue<SOSiteStatusFilter.historyDate>>,
								Or<CurrentValue<SOSiteStatusFilter.dropShipSales>, Equal<True>, 
									And<INItemCustSalesStats.dropShipLastDate, GreaterEqual<CurrentValue<SOSiteStatusFilter.historyDate>>>>>>>>>>>,
	LeftJoin<INUnit,
					On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>,
					And<INUnit.unitType, Equal<INUnitType.inventoryItem>,
					And<INUnit.fromUnit, Equal<InventoryItem.salesUnit>,
					And<INUnit.toUnit, Equal<InventoryItem.baseUnit>>>>>
							>>>>>>>>>>,
		Where<CurrentValue<SOOrder.customerID>, IsNotNull,
		  And2<CurrentMatch<InventoryItem, AccessInfo.userName>,
			And2<Where<INSiteStatus.siteID, IsNull, Or<INSite.branchID, IsNotNull, And2<CurrentMatch<INSite, AccessInfo.userName>,
				And<Where2<FeatureInstalled<FeaturesSet.interBranch>,
					Or2<SameOrganizationBranch<INSite.branchID, Current<SOOrder.branchID>>,
					Or<CurrentValue<SOOrder.behavior>, Equal<SOBehavior.qT>>>>>>>>,
			And2<Where<INSiteStatus.subItemID, IsNull, Or<CurrentMatch<INSubItem, AccessInfo.userName>>>,
			And2<Where<CurrentValue<INSiteStatusFilter.onlyAvailable>, Equal<boolFalse>,
				   Or<INSiteStatus.qtyAvail, Greater<CS.decimal0>>>,
		  And2<Where<CurrentValue<SOSiteStatusFilter.mode>, Equal<SOAddItemMode.bySite>,
					Or<INItemCustSalesStats.lastQty, Greater<decimal0>,
					Or<CurrentValue<SOSiteStatusFilter.dropShipSales>, Equal<True>, And<INItemCustSalesStats.dropShipLastQty, Greater<decimal0>>>>>,
			And<InventoryItem.isTemplate, Equal<False>,
			And<InventoryItem.itemStatus, NotIn3<
				 InventoryItemStatus.unknown,
				 InventoryItemStatus.inactive,
				 InventoryItemStatus.markedForDeletion,
				 InventoryItemStatus.noSales>>>>>>>>>>), Persistent = false)]
	public partial class SOSiteStatusSelected : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
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

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(BqlField = typeof(InventoryItem.inventoryID), IsKey = true)]
		[PXDefault()]
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

		#region InventoryCD
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		protected string _InventoryCD;
		[PXDefault()]
		[InventoryRaw(BqlField = typeof(InventoryItem.inventoryCD))]
		public virtual String InventoryCD
		{
			get
			{
				return this._InventoryCD;
			}
			set
			{
				this._InventoryCD = value;
			}
		}
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected string _Descr;
		[PXDBLocalizableString(60, IsUnicode = true, BqlField = typeof(InventoryItem.descr), IsProjection = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion

		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		protected int? _ItemClassID;
		[PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
		[PXUIField(DisplayName = "Item Class ID", Visible = false)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassID), typeof(INItemClass.itemClassCD), ValidComboRequired = true)]
		public virtual int? ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion

		#region ItemClassCD
		public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
		protected string _ItemClassCD;
		[PXDBString(30, IsUnicode = true, BqlField = typeof(INItemClass.itemClassCD))]
		public virtual string ItemClassCD
		{
			get
			{
				return this._ItemClassCD;
			}
			set
			{
				this._ItemClassCD = value;
			}
		}
		#endregion

		#region ItemClassDescription
		public abstract class itemClassDescription : PX.Data.BQL.BqlString.Field<itemClassDescription> { }
		protected String _ItemClassDescription;
		[PXDBLocalizableString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INItemClass.descr), IsProjection = true)]
		[PXUIField(DisplayName = "Item Class Description", Visible = false, ErrorHandling = PXErrorHandling.Always)]
		public virtual String ItemClassDescription
		{
			get
			{
				return this._ItemClassDescription;
			}
			set
			{
				this._ItemClassDescription = value;
			}
		}
		#endregion

		#region PriceClassID
		public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }

		protected string _PriceClassID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(InventoryItem.priceClassID))]
		[PXUIField(DisplayName = "Price Class ID", Visible = false)]
		public virtual String PriceClassID
		{
			get
			{
				return this._PriceClassID;
			}
			set
			{
				this._PriceClassID = value;
			}
		}
		#endregion

		#region PriceClassDescription
		public abstract class priceClassDescription : PX.Data.BQL.BqlString.Field<priceClassDescription> { }
		protected String _PriceClassDescription;
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INPriceClass.description))]
		[PXUIField(DisplayName = "Price Class Description", Visible = false, ErrorHandling = PXErrorHandling.Always)]
		public virtual String PriceClassDescription
		{
			get
			{
				return this._PriceClassDescription;
			}
			set
			{
				this._PriceClassDescription = value;
			}
		}
		#endregion

		#region PreferredVendorID
		public abstract class preferredVendorID : PX.Data.BQL.BqlInt.Field<preferredVendorID> { }

		protected Int32? _PreferredVendorID;
		[AP.VendorNonEmployeeActive(DisplayName = "Preferred Vendor ID", Required = false, DescriptionField = typeof(BAccountR.acctName), BqlField = typeof(InventoryItem.preferredVendorID), Visible = false, ErrorHandling = PXErrorHandling.Always)]
		public virtual Int32? PreferredVendorID
		{
			get
			{
				return this._PreferredVendorID;
			}
			set
			{
				this._PreferredVendorID = value;
			}
		}
		#endregion

		#region PreferredVendorDescription
		public abstract class preferredVendorDescription : PX.Data.BQL.BqlString.Field<preferredVendorDescription> { }
		protected String _PreferredVendorDescription;
		[PXDBString(250, IsUnicode = true, BqlField = typeof(BAccountR.acctName))]
		[PXUIField(DisplayName = "Preferred Vendor Name", Visible = false, ErrorHandling = PXErrorHandling.Always)]
		public virtual String PreferredVendorDescription
		{
			get
			{
				return this._PreferredVendorDescription;
			}
			set
			{
				this._PreferredVendorDescription = value;
			}
		}
		#endregion

		#region BarCode
		public abstract class barCode : PX.Data.BQL.BqlString.Field<barCode> { }
		protected String _BarCode;
		[PXDBString(255, BqlField = typeof(INItemXRef.alternateID), IsUnicode = true)]
		[PXUIField(DisplayName = "Barcode", Visible = false)]
		public virtual String BarCode
		{
			get
			{
				return this._BarCode;
			}
			set
			{
				this._BarCode = value;
			}
		}
		#endregion

		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;
		[PXDBString(225, IsUnicode = true, InputMask = "", BqlField = typeof(INItemPartNumber.alternateID))]
		[PXUIField(DisplayName = "Alternate ID")]
		[PXExtraKey]
		public virtual String AlternateID
		{
			get
			{
				return this._AlternateID;
			}
			set
			{
				this._AlternateID = value;
			}
		}
		#endregion

		#region AlternateType
		public abstract class alternateType : PX.Data.BQL.BqlString.Field<alternateType> { }
		protected String _AlternateType;
		[PXDBString(4, BqlField = typeof(INItemPartNumber.alternateType))]
		[INAlternateType.List()]
		[PXDefault(INAlternateType.Global)]
		[PXUIField(DisplayName = "Alternate Type")]
		public virtual String AlternateType
		{
			get
			{
				return this._AlternateType;
			}
			set
			{
				this._AlternateType = value;
			}
		}
		#endregion

		#region Descr
		public abstract class alternateDescr : PX.Data.BQL.BqlString.Field<alternateDescr> { }
		protected String _AlternateDescr;
		[PXDBString(60, IsUnicode = true, BqlField = typeof(INItemPartNumber.descr))]
		[PXUIField(DisplayName = "Alternate Description", Visible = false)]
		public virtual String AlternateDescr
		{
			get
			{
				return this._AlternateDescr;
			}
			set
			{
				this._AlternateDescr = value;
			}
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected int? _SiteID;
		[PXUIField(DisplayName = "Warehouse")]
		[SiteAttribute(BqlField = typeof(INSiteStatus.siteID))]
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

		#region SiteCD
		public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
		protected String _SiteCD;
		[PXString(IsUnicode = true, IsKey = true)]
		[PXDBCalced(typeof(IsNull<RTrim<INSite.siteCD>, Empty>), typeof(string))]
		public virtual String SiteCD
		{
			get
			{
				return this._SiteCD;
			}
			set
			{
				this._SiteCD = value;
			}
		}
		#endregion

		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected int? _SubItemID;
		[SubItem(typeof(SOSiteStatusSelected.inventoryID), BqlField = typeof(INSubItem.subItemID))]
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

		#region SubItemCD
		public abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
		protected String _SubItemCD;
		[PXString(IsUnicode = true, IsKey = true)]
		[PXDBCalced(typeof(IsNull<RTrim<INSubItem.subItemCD>, Empty>), typeof(string))]
		public virtual String SubItemCD
		{
			get
			{
				return this._SubItemCD;
			}
			set
			{
				this._SubItemCD = value;
			}
		}
		#endregion

		#region BaseUnit
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }

		protected string _BaseUnit;
		[INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.Visible, BqlField = typeof(InventoryItem.baseUnit))]
		public virtual String BaseUnit
		{
			get
			{
				return this._BaseUnit;
			}
			set
			{
				this._BaseUnit = value;
			}
		}
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
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

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXLong()]
		[CurrencyInfo()]
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

		#region SalesUnit
		public abstract class salesUnit : PX.Data.BQL.BqlString.Field<salesUnit> { }
		protected string _SalesUnit;
		[INUnit(typeof(SOSiteStatusSelected.inventoryID), DisplayName = "Sales Unit", BqlField = typeof(InventoryItem.salesUnit))]
		public virtual String SalesUnit
		{
			get
			{
				return this._SalesUnit;
			}
			set
			{
				this._SalesUnit = value;
			}
		}
		#endregion

		#region QtySelected
		public abstract class qtySelected : PX.Data.BQL.BqlDecimal.Field<qtySelected> { }
		protected Decimal? _QtySelected;
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Selected")]
		public virtual Decimal? QtySelected
		{
			get
			{
				return this._QtySelected ?? 0m;
			}
			set
			{
				if (value != null && value != 0m)
					this._Selected = true;
				this._QtySelected = value;
			}
		}
		#endregion

		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity(BqlField = typeof(INSiteStatus.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion

		#region QtyAvail
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		protected Decimal? _QtyAvail;
		[PXDBQuantity(BqlField = typeof(INSiteStatus.qtyAvail))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvail
		{
			get
			{
				return this._QtyAvail;
			}
			set
			{
				this._QtyAvail = value;
			}
		}
		#endregion

		#region QtyLast
		public abstract class qtyLast : PX.Data.BQL.BqlDecimal.Field<qtyLast> { }
		protected Decimal? _QtyLast;
		[PXDBQuantity(BqlField = typeof(INItemCustSalesStats.lastQty))]
		public virtual Decimal? QtyLast
		{
			get
			{
				return this._QtyLast;
			}
			set
			{
				this._QtyLast = value;
			}
		}
		#endregion

		#region BaseUnitPrice
		public abstract class baseUnitPrice : PX.Data.BQL.BqlDecimal.Field<baseUnitPrice> { }
		protected Decimal? _BaseUnitPrice;
		[PXDBPriceCost(true, BqlField = typeof(INItemCustSalesStats.lastUnitPrice))]
		public virtual Decimal? BaseUnitPrice
		{
			get
			{
				return this._BaseUnitPrice;
			}
			set
			{
				this._BaseUnitPrice = value;
			}
		}
		#endregion

		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
		protected Decimal? _CuryUnitPrice;
		[PXUnitPriceCuryConv(typeof(SOSiteStatusSelected.curyInfoID), typeof(SOSiteStatusSelected.baseUnitPrice))]
		[PXUIField(DisplayName = "Last Unit Price", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryUnitPrice
		{
			get
			{
				return this._CuryUnitPrice;
			}
			set
			{
				this._CuryUnitPrice = value;
			}
		}
		#endregion

		#region QtyAvailSale
		public abstract class qtyAvailSale : PX.Data.BQL.BqlDecimal.Field<qtyAvailSale> { }
		protected Decimal? _QtyAvailSale;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INSiteStatus.qtyAvail, INUnit.unitRate>>,
			Div<INSiteStatus.qtyAvail, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvailSale
		{
			get
			{
				return this._QtyAvailSale;
			}
			set
			{
				this._QtyAvailSale = value;
			}
		}
		#endregion

		#region QtyOnHandSale
		public abstract class qtyOnHandSale : PX.Data.BQL.BqlDecimal.Field<qtyOnHandSale> { }
		protected Decimal? _QtyOnHandSale;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INSiteStatus.qtyOnHand, INUnit.unitRate>>,
			Div<INSiteStatus.qtyOnHand, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual Decimal? QtyOnHandSale
		{
			get
			{
				return this._QtyOnHandSale;
			}
			set
			{
				this._QtyOnHandSale = value;
			}
		}
		#endregion

		#region QtyLastSale
		public abstract class qtyLastSale : PX.Data.BQL.BqlDecimal.Field<qtyLastSale> { }
		protected Decimal? _QtyLastSale;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INItemCustSalesStats.lastQty, INUnit.unitRate>>,
			Div<INItemCustSalesStats.lastQty, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXUIField(DisplayName = "Qty. Last Sales")]
		public virtual Decimal? QtyLastSale
		{
			get
			{
				return this._QtyLastSale;
			}
			set
			{
				this._QtyLastSale = value;
			}
		}
		#endregion

		#region LastSalesDate
		public abstract class lastSalesDate : PX.Data.BQL.BqlDateTime.Field<lastSalesDate> { }
		protected DateTime? _LastSalesDate;
		[PXDBDate(BqlField = typeof(INItemCustSalesStats.lastDate))]
		[PXUIField(DisplayName = "Last Sales Date")]
		public virtual DateTime? LastSalesDate
		{
			get
			{
				return this._LastSalesDate;
			}
			set
			{
				this._LastSalesDate = value;
			}
		}
		#endregion

		#region DropShipLastQty
		public abstract class dropShipLastBaseQty : PX.Data.BQL.BqlDecimal.Field<dropShipLastBaseQty> { }
		[PXDBQuantity(BqlField = typeof(INItemCustSalesStats.dropShipLastQty))]
		public virtual Decimal? DropShipLastBaseQty
		{
			get;
			set;
		}
		#endregion

		#region DropShipLastQty
		public abstract class dropShipLastQty : PX.Data.BQL.BqlDecimal.Field<dropShipLastQty> { }
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INItemCustSalesStats.dropShipLastQty, INUnit.unitRate>>,
			Div<INItemCustSalesStats.dropShipLastQty, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXUIField(DisplayName = "Qty. of Last Drop Ship")]
		public virtual Decimal? DropShipLastQty
		{
			get;
			set;
		}
		#endregion

		#region DropShipLastUnitPrice
		public abstract class dropShipLastUnitPrice : PX.Data.BQL.BqlDecimal.Field<dropShipLastUnitPrice> { }
		[PXDBPriceCost(true, BqlField = typeof(INItemCustSalesStats.dropShipLastUnitPrice))]
		public virtual Decimal? DropShipLastUnitPrice
		{
			get;
			set;
		}
		#endregion

		#region DropShipCuryUnitPrice
		public abstract class dropShipCuryUnitPrice : PX.Data.BQL.BqlDecimal.Field<dropShipCuryUnitPrice> { }
		[PXUnitPriceCuryConv(typeof(SOSiteStatusSelected.curyInfoID), typeof(SOSiteStatusSelected.dropShipLastUnitPrice))]
		[PXUIField(DisplayName = "Unit Price of Last Drop Ship", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? DropShipCuryUnitPrice
		{
			get;
			set;
		}
		#endregion

		#region DropShipLastDate
		public abstract class dropShipLastDate : PX.Data.BQL.BqlDateTime.Field<dropShipLastDate> { }
		[PXDBDate(BqlField = typeof(INItemCustSalesStats.dropShipLastDate))]
		[PXUIField(DisplayName = "Date of Last Drop Ship")]
		public virtual DateTime? DropShipLastDate
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(BqlField = typeof(InventoryItem.noteID))]
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
	}
	[System.SerializableAttribute()]
	[PXSubstitute(GraphType = typeof(SOInvoiceEntry))]
	public partial class CustomerPaymentMethodC : AR.CustomerPaymentMethod
	{
		public new abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }

		public new abstract class cCProcessingCenterID : PX.Data.BQL.BqlString.Field<cCProcessingCenterID> { }

		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBInt]
		[PXDefault(typeof(SOOrder.customerID))]
		[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<AR.CustomerPaymentMethod.bAccountID>>>>))]
		public override Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion

		#region PaymentMethodID
		public new abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Card Type", Enabled = false)]
		[PXDefault()]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.isActive, Equal<boolTrue>,
				And<PaymentMethod.useForAR, Equal<True>,
				And<PaymentMethod.aRIsOnePerCustomer, Equal<False>>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		public override String PaymentMethodID
		{
			get
			{
				return this._PaymentMethodID;
			}
			set
			{
				this._PaymentMethodID = value;
			}
		}
		#endregion

		#region CashAccountID
		public new abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[CashAccount(null, typeof(Search2<CashAccount.cashAccountID, InnerJoin<PaymentMethodAccount,
		  On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
			And<PaymentMethodAccount.paymentMethodID, Equal<Current<CustomerPaymentMethodC.paymentMethodID>>>>>,
		  Where<Match<Current<AccessInfo.userName>>>>), DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(Search<CA.PaymentMethodAccount.cashAccountID,
				Where<CA.PaymentMethodAccount.paymentMethodID, Equal<Optional<CustomerPaymentMethodC.paymentMethodID>>,
				And<CA.PaymentMethodAccount.useForAR, Equal<True>>>, OrderBy<Desc<PaymentMethodAccount.aRIsDefault>>>))]
		public override Int32? CashAccountID
		{
			get
			{
				return this._CashAccountID;
			}
			set
			{
				this._CashAccountID = value;
			}
		}
		#endregion

		#region Descr
		public new abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBString(255, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Identifier", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public override String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion

	}

	[System.SerializableAttribute()]
	public class InvoiceSplits : IBqlTable
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

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXInt(IsKey = true)]
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

		#region ARTran
		#region TranType
		public abstract class tranTypeARTran : PX.Data.BQL.BqlString.Field<tranTypeARTran> { }
		protected String _TranTypeARTran;
		[PXString(3, IsFixed = true, IsKey = true)]
		[PXUIField(DisplayName = "Tran. Type", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String TranTypeARTran
		{
			get
			{
				return this._TranTypeARTran;
			}
			set
			{
				this._TranTypeARTran = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbrARTran : PX.Data.BQL.BqlString.Field<refNbrARTran> { }
		protected String _RefNbrARTran;
		[PXString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String RefNbrARTran
		{
			get
			{
				return this._RefNbrARTran;
			}
			set
			{
				this._RefNbrARTran = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbrARTran : PX.Data.BQL.BqlInt.Field<lineNbrARTran> { }
		protected Int32? _LineNbrARTran;
		[PXInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int32? LineNbrARTran
		{
			get
			{
				return this._LineNbrARTran;
			}
			set
			{
				this._LineNbrARTran = value;
			}
		}
		#endregion

		#endregion

		#region SOLine
		#region OrderType
		public abstract class orderTypeSOLine : PX.Data.BQL.BqlString.Field<orderTypeSOLine> { }
		protected String _OrderTypeSOLine;
		[PXString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Order Type", Visible = false)]
		[PXSelector(typeof(Search<SOOrderType.orderType>), CacheGlobal = true)]
		public virtual String OrderTypeSOLine
		{
			get
			{
				return this._OrderTypeSOLine;
			}
			set
			{
				this._OrderTypeSOLine = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbrSOLine : PX.Data.BQL.BqlString.Field<orderNbrSOLine> { }
		protected String _OrderNbrSOLine;
		[PXString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Order Nbr.", Visible = false)]
		public virtual String OrderNbrSOLine
		{
			get
			{
				return this._OrderNbrSOLine;
			}
			set
			{
				this._OrderNbrSOLine = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbrSOLine : PX.Data.BQL.BqlInt.Field<lineNbrSOLine> { }
		protected Int32? _LineNbrSOLine;
		[PXInt()]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual Int32? LineNbrSOLine
		{
			get
			{
				return this._LineNbrSOLine;
			}
			set
			{
				this._LineNbrSOLine = value;
			}
		}
		#endregion

		#region LineTypeSOLine
		public abstract class lineTypeSOLine : PX.Data.BQL.BqlString.Field<lineTypeSOLine> { }
		protected String _LineTypeSOLine;
		[PXString(2, IsFixed = true)]
		public virtual String LineTypeSOLine
		{
			get
			{
				return this._LineTypeSOLine;
			}
			set
			{
				this._LineTypeSOLine = value;
			}
		}
		#endregion


		#endregion

		#region INTran
		#region DocType
		public abstract class docTypeINTran : PX.Data.BQL.BqlString.Field<docTypeINTran> { }
		protected String _DocTypeINTran;
		[PXString(1, IsFixed = true)]
		public virtual String DocTypeINTran
		{
			get
			{
				return this._DocTypeINTran;
			}
			set
			{
				this._DocTypeINTran = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbrINTran : PX.Data.BQL.BqlString.Field<refNbrINTran> { }
		protected String _RefNbrINTran;
		[PXString(15, IsUnicode = true)]
		public virtual String RefNbrINTran
		{
			get
			{
				return this._RefNbrINTran;
			}
			set
			{
				this._RefNbrINTran = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbrINTran : PX.Data.BQL.BqlInt.Field<lineNbrINTran> { }
		protected Int32? _LineNbrINTran;
		[PXInt()]
		public virtual Int32? LineNbrINTran
		{
			get
			{
				return this._LineNbrINTran;
			}
			set
			{
				this._LineNbrINTran = value;
			}
		}
		#endregion
		#region Inventory
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(DisplayName = "Inventory ID")]
		public virtual Int32? InventoryID
		{
			get
			{
				return _InventoryID;
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
		[IN.SubItem(typeof(inventoryID))]
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
		[IN.Site()]
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
		[IN.Location()]
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
		[LotSerialNbr]
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

		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
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
		[PXQuantity()]
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
		[PXQuantity()]
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

		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Line Description")]
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

		#endregion

		#region INTranSplit
		#region DocType
		public abstract class docTypeINTranSplit : PX.Data.BQL.BqlString.Field<docTypeINTranSplit> { }
		protected String _DocTypeINTranSplit;
		[PXString(1, IsFixed = true)]
		public virtual String DocTypeINTranSplit
		{
			get
			{
				return this._DocTypeINTranSplit;
			}
			set
			{
				this._DocTypeINTranSplit = value;
			}
		}
		#endregion
		#region RefNbr
		public abstract class refNbrINTranSplit : PX.Data.BQL.BqlString.Field<refNbrINTranSplit> { }
		protected String _RefNbrINTranSplit;
		[PXString(15, IsUnicode = true)]
		public virtual String RefNbrINTranSplit
		{
			get
			{
				return this._RefNbrINTranSplit;
			}
			set
			{
				this._RefNbrINTranSplit = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbrINTranSplit : PX.Data.BQL.BqlInt.Field<lineNbrINTranSplit> { }
		protected Int32? _LineNbrINTranSplit;
		[PXInt()]
		public virtual Int32? LineNbrINTranSplit
		{
			get
			{
				return this._LineNbrINTranSplit;
			}
			set
			{
				this._LineNbrINTranSplit = value;
			}
		}
		#endregion

		#region LineNbr
		public abstract class splitLineNbrINTranSplit : PX.Data.BQL.BqlInt.Field<splitLineNbrINTranSplit> { }
		protected Int32? _SplitLineNbrINTranSplit;
		[PXInt()]
		public virtual Int32? SplitLineNbrINTranSplit
		{
			get
			{
				return this._SplitLineNbrINTranSplit;
			}
			set
			{
				this._SplitLineNbrINTranSplit = value;
			}
		}
		#endregion
		#endregion

		#region SOSalesPerTran
		#region OrderType
		public abstract class orderTypeSOSalesPerTran : PX.Data.BQL.BqlString.Field<orderTypeSOSalesPerTran> { }
		protected String _OrderTypeSOSalesPerTran;
		[PXString(2, IsFixed = true)]
		public virtual String OrderTypeSOSalesPerTran
		{
			get
			{
				return this._OrderTypeSOSalesPerTran;
			}
			set
			{
				this._OrderTypeSOSalesPerTran = value;
			}
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbrSOSalesPerTran : PX.Data.BQL.BqlString.Field<orderNbrSOSalesPerTran> { }
		protected String _OrderNbrSOSalesPerTran;
		[PXString(15, IsUnicode = true)]
		public virtual String OrderNbrSOSalesPerTran
		{
			get
			{
				return this._OrderNbrSOSalesPerTran;
			}
			set
			{
				this._OrderNbrSOSalesPerTran = value;
			}
		}
		#endregion
		#region SalespersonID
		public abstract class salespersonIDSOSalesPerTran : PX.Data.BQL.BqlInt.Field<salespersonIDSOSalesPerTran> { }
		protected Int32? _SalespersonIDSOSalesPerTran;
		[PXInt()]
		public virtual Int32? SalespersonIDSOSalesPerTran
		{
			get
			{
				return this._SalespersonIDSOSalesPerTran;
			}
			set
			{
				this._SalespersonIDSOSalesPerTran = value;
			}
		}
		#endregion

		#endregion
	}

	public class PaymentTransaction : PaymentTransactionGraph<SOOrderEntry, SOOrder>
	{
		public PXSelect<ExternalTransaction> externalTran;

		protected override PaymentMapping GetPaymentMapping()
		{
			return new PaymentMapping(typeof(SOOrder));
		}

		protected override ExternalTransactionDetailMapping GetExternalTransactionMapping()
		{
			return new ExternalTransactionDetailMapping(typeof(ExternalTransaction));
		}

		protected override PaymentTransactionDetailMapping GetPaymentTransactionMapping()
		{
			return new PaymentTransactionDetailMapping(typeof(CCProcTran));
		}

		protected override void BeforeAuthorizePayment(SOOrder doc)
		{
			//Added for mass processing call AuthorizeCCPayment via Automation steps
			doc.CuryUnpaidBalance = PXFormulaAttribute.Evaluate<SOOrder.curyUnpaidBalance>(Base.Caches[typeof(SOOrder)], doc) as decimal?;
		}

		protected override void BeforeCapturePayment(SOOrder doc)
		{
			doc.CuryUnpaidBalance = PXFormulaAttribute.Evaluate<SOOrder.curyUnpaidBalance>(Base.Caches[typeof(SOOrder)], doc) as decimal?;
			IExternalTransaction tran = GetExtTrans().FirstOrDefault();
			if (tran != null)
			{
				ExternalTransactionState state = ExternalTranHelper.GetTransactionState(Base, tran);
				if (state.IsPreAuthorized && state.ExternalTransaction?.DocType != null)
				{
					throw new PXException(Messages.CannotPerformCCCapture);
				}
			}
		}

		protected override IEnumerable<AfterTranProcDelegate> GetAfterAuthorizeActions()
		{
			yield return UpdateSOOrderState;
		}

		protected override IEnumerable<AfterTranProcDelegate> GetAfterCaptureActions()
		{
			yield return UpdateSOOrderState;
		}

		protected override IEnumerable<AfterTranProcDelegate> GetAfterVoidActions()
		{
			yield return UpdateSOOrderState;
		}

		protected override IEnumerable<AfterTranProcDelegate> GetAfterCreditActions()
		{
			yield return UpdateSOOrderState;
		}

		protected override void MapViews(SOOrderEntry graph)
		{
			base.MapViews(graph);
			this.PaymentTransaction = new PXSelectExtension<PaymentTransactionDetail>(Base.ccProcTran);
			this.ExternalTransaction = new PXSelectExtension<ExternalTransactionDetail>(Base.ExternalTran);
		}

		protected override void RowSelected(Events.RowSelected<SOOrder> e)
		{
			base.RowSelected(e);
			SOOrder doc = e.Row;
			if (doc == null)
				return;
			TranHeldwarnMsg = Messages.CCProcessingSOTranHeldWarning;
			SOOrderEntry orderEntry = this.Base;
			PXCache cache = orderEntry.Caches[typeof(SOOrder)];

			ExternalTransactionState state = this.GetActiveTransactionState();
			IExternalTransaction extTran = state.ExternalTransaction;
			ICCPaymentTransaction lastTranRecord = GetProcTrans().FirstOrDefault();
			doc.CCPaymentStateDescr = GetPaymentStateDescr(state);
			doc.PCResponseReasonText = string.Empty;
			if (lastTranRecord != null && lastTranRecord is PaymentTransactionDetail)
			{
				doc.PCResponseReasonText = ((PaymentTransactionDetail)lastTranRecord).PCResponseReasonText;
			}

			if (state.IsPreAuthorized)
			{
				doc.PreAuthTranNumber = extTran.TranNumber;
			}
			if (state.IsCaptured)
			{
				doc.CaptureTranNumber = extTran.TranNumber;
			}

			bool allowAlloc = Base.AllowAllocation();
			if (doc.Completed == true || doc.Cancelled == true || !allowAlloc)
			{
				this.authorizeCCPayment.SetEnabled(false);
				this.voidCCPayment.SetEnabled(false);
				this.captureCCPayment.SetEnabled(false);
				this.creditCCPayment.SetEnabled(false);
				this.authorizeCCPayment.SetVisible(false);
				this.voidCCPayment.SetVisible(false);
				this.captureCCPayment.SetVisible(false);
				this.creditCCPayment.SetVisible(false);
			}
			else
			{ 
				CustomerPaymentMethodC cpmRow = null;
				bool isPMCreditCard = false;

				if (doc.PMInstanceID.HasValue)
				{
					cpmRow = orderEntry.DefPaymentMethodInstance.Select();
					bool isCCInserted = orderEntry.DefPaymentMethodInstance.Cache.GetStatus(cpmRow) == PXEntryStatus.Inserted;
					doc.CreatePMInstance = isCCInserted;
					if (cpmRow != null && !String.IsNullOrEmpty(cpmRow.PaymentMethodID))
					{
						PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this.Base, cpmRow.PaymentMethodID);
						if (pm != null && pm.PaymentType == CA.PaymentMethodType.CreditCard)
						{
							isPMCreditCard = true;
						}
					}
				}

				decimal? docBalance = ((ICCPayment)doc).CuryDocBal;
				bool isNotNullAmt = docBalance != null && docBalance > 0;

				bool canAuthorize = !(state.IsPreAuthorized || state.IsCaptured) && isNotNullAmt;
				bool canCapture = !(state.IsCaptured) && isNotNullAmt;
				bool canVoid = (state.IsCaptured || state.IsPreAuthorized);
				bool canRefund = state.IsRefunded;
				bool isCashReturn = doc.ARDocType == ARDocType.CashReturn;

				bool isInserted = cache.GetStatus(doc) == PXEntryStatus.Inserted;
				this.authorizeCCPayment.SetVisible(isPMCreditCard && !isCashReturn && !orderEntry.IsTransferOrder);
				this.authorizeCCPayment.SetEnabled(!isInserted && isPMCreditCard && !isCashReturn && canAuthorize);
				this.captureCCPayment.SetVisible(isPMCreditCard && !isCashReturn && !orderEntry.IsTransferOrder);
				this.captureCCPayment.SetEnabled(!isInserted && isPMCreditCard && !isCashReturn && canCapture);
				this.voidCCPayment.SetVisible(isPMCreditCard && !isCashReturn && !orderEntry.IsTransferOrder);
				this.voidCCPayment.SetEnabled(!isInserted && isPMCreditCard && !isCashReturn && canVoid);
				this.creditCCPayment.SetVisible(isPMCreditCard && isCashReturn && !orderEntry.IsTransferOrder);
				this.creditCCPayment.SetEnabled(doc.RefTranExtNbr != null && !canRefund);
				cache.AllowDelete = !ExternalTranHelper.HasSuccessfulTrans(Base.ExternalTran);
				if (state.IsOpenForReview)
				{
					cache.RaiseExceptionHandling<SOOrder.cCPaymentStateDescr>(doc, null,
						new PXSetPropertyException(Messages.CCProcessingSOTranHeldWarning, PXErrorLevel.RowWarning));
				}
				bool enableCCAuthEntering = (doc.CreatePMInstance == true || doc.PMInstanceID.HasValue) && isPMCreditCard
					&& !state.IsCaptured && !state.IsPreAuthorized && !isCashReturn;
				PXUIFieldAttribute.SetEnabled<SOOrder.preAuthTranNumber>(cache, doc, enableCCAuthEntering);
				PXUIFieldAttribute.SetEnabled<SOOrder.cCAuthExpirationDate>(cache, doc, enableCCAuthEntering && string.IsNullOrEmpty(doc.PreAuthTranNumber) == false);
				PXUIFieldAttribute.SetEnabled<SOOrder.curyCCPreAuthAmount>(cache, doc, enableCCAuthEntering && string.IsNullOrEmpty(doc.PreAuthTranNumber) == false);
			}
			this.recordCCPayment.SetVisible(false);
			this.recordCCPayment.SetEnabled(false);
			this.captureOnlyCCPayment.SetVisible(false);
			this.captureOnlyCCPayment.SetEnabled(false);
			this.voidCCPayment.SetCaption("Void CC Auth. / Payment");
		}

		private string GetPaymentStateDescr(ExternalTransactionState state)
		{
			return GetLastTransactionDescription();
		}

		public static void UpdateSOOrderState(IBqlTable aDoc, CCTranType lastOperation, bool success)
		{
			if (!success) return;
			SOOrder olddoc = aDoc as SOOrder;
			if (olddoc == null) return;
			UpdateDocState(olddoc, lastOperation);
		}

		public static void UpdateDocState(SOOrder olddoc, CCTranType lastOperation)
		{
			SOOrderEntry graph = PXGraph.CreateInstance<SOOrderEntry>();
			graph.Document.Current = graph.Document.Search<SOOrder.orderNbr>(olddoc.OrderNbr, olddoc.OrderType);

			SOOrder doc = (SOOrder)graph.Document.Cache.CreateCopy(graph.Document.Current);
			IExternalTransaction extTran = graph.ExternalTran.SelectSingle();
			ExternalTransactionState state = ExternalTranHelper.GetTransactionState(graph, extTran);
			bool needUpdate = ExternalTranHelper.UpdateCCPaymentState(doc, state);

			if (doc.IsCCCaptured == true || doc.IsCCAuthorized == true)
			{
				doc.ExtRefNbr = extTran.TranNumber;
				doc = graph.Document.Update(doc);
			}
			else if (doc.IsCCCaptured == false && doc.IsCCAuthorized == false && (state.IsVoided || state.IsDeclined))
			{
				doc.ExtRefNbr = null;
				doc = graph.Document.Update(doc);
			}

			bool needsPersist = false;
			if (needUpdate)
			{
				doc.PreAuthTranNumber = null;
				doc = graph.Document.Update(doc);
				graph.Document.Search<SOOrder.orderNbr>(doc.OrderNbr, doc.OrderType);
				needsPersist = true;
			}
			try
			{
				if (graph.soordertype.Current.CanHaveApplications)
				{
					if ((state.IsVoided || state.IsRefunded)
						&& graph.arsetup.Current.IntegratedCCProcessing == true)
					{
						PXSetPropertyException message = null;
						ARPaymentEntry docgraph = PXGraph.CreateInstance<ARPaymentEntry>();
						ARPayment payment = PXSelect<ARPayment, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
							And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(docgraph, extTran.DocType, extTran.RefNbr);
						if (payment != null && payment.Voided == false)
						{
							docgraph.VoidCheckProcExt(payment);

							ARPayment voidPayment = docgraph.Document.Current;
							if(voidPayment?.DocType == ARDocType.VoidPayment)
							{
								voidPayment.ExtRefNbr = extTran.TranNumber;
							}

							docgraph.Save.Press();
							PaymentTransactionGraph<SOOrderEntry, SOOrder>.ReleaseARDocument(docgraph.Document.Current);
							needsPersist = true;
						}
						else if(payment != null)
						{
							message = new PXSetPropertyException(Messages.CouldNotVoidCCTranPayment, PXErrorLevel.Warning, extTran.RefNbr);
						}

						if (message != null)
						{
							PXLongOperation.SetCustomInfo(new SOOrderEntry.SOOrderMessageDisplay(message));
						}
					}
					if (state.IsCaptured)
					{
						graph.Save.Press();
						PXGraph target;
						graph.CreatePaymentProc(graph.Document.Current, out target);
						needsPersist = true;
					}
				}
			}
			finally
			{
				if (needsPersist)
				{
					graph.Save.Press();
				}
			}
		}
	}
}
