using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.PO;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.CS;
using System.Collections;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.AR;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.TM;
using PX.Objects.Common;
using PX.Objects.Common.Bql;

namespace PX.Objects.RQ
{
	//Cache merged
    [Serializable]
    [PXHidden]
	public partial class VendorContact : Contact
	{
		#region ContactID

		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		#endregion
	}

	public class RQRequisitionEntry : PXGraph<RQRequisitionEntry>, IGraphWithInitialization
	{
		public PXFilter<BAccount> cbaccount;
		public PXFilter<Vendor> cbendor;
		public PXFilter<EPEmployee> cemployee;

		public PXSave<RQRequisition> Save;
		public PXCancel<RQRequisition> Cancel;
		public PXInsert<RQRequisition> Insert;
		public PXDelete<RQRequisition> Delete;
		public PXCopyPasteAction<RQRequisition> CopyPaste;
		public PXFirst<RQRequisition> First;
		public PXPrevious<RQRequisition> Previous;
		public PXNext<RQRequisition> Next;
		public PXLast<RQRequisition> Last;


		[PXViewName(Messages.RQRequisition)]
		[PXCopyPasteHiddenFields(typeof(RQRequisition.status), typeof(RQRequisition.hold), typeof(RQRequisition.quoted), typeof(RQRequisition.employeeID))]
		public PXSelectJoin<RQRequisition,
			LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<RQRequisition.customerID>>,
			LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<RQRequisition.vendorID>>>>,
			Where2<Where<Customer.bAccountID, IsNull,
								Or<Match<Customer, Current<AccessInfo.userName>>>>,
 				 And<Where<Vendor.bAccountID, IsNull,
								Or<Match<Vendor, Current<AccessInfo.userName>>>>>>> Document;

		[PXCopyPasteHiddenFields(typeof(RQRequisition.vendorID), typeof(RQRequisition.vendorLocationID), typeof(RQRequisition.vendorRefNbr), typeof(RQRequisition.vendorRequestSent))]
		public PXSelect<RQRequisition, Where<RQRequisition.reqNbr, Equal<Current<RQRequisition.reqNbr>>>> CurrentDocument;
		public PXSelect<InventoryItem> invItems;
		[PXCopyPasteHiddenFields(typeof(RQRequisitionLine.cancelled))]
		public PXSelect<RQRequisitionLine, Where<RQRequisitionLine.reqNbr, Equal<Optional<RQRequisition.reqNbr>>>> Lines;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<RQBiddingVendor,
					LeftJoin<Location, On<Location.bAccountID, Equal<RQBiddingVendor.vendorID>,
								And<Location.locationID, Equal<RQBiddingVendor.vendorLocationID>>>>,
					Where<RQBiddingVendor.reqNbr, Equal<Optional<RQRequisition.reqNbr>>>> Vendors;

		[PXViewName(CR.Messages.Employee)]
		public PXSetup<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<RQRequisition.employeeID>>>> Employee;  

		public PXSelect<RQBidding,
			Where<RQBidding.reqNbr, Equal<Current<RQRequisition.reqNbr>>>> Bidding;
		[PXViewName(PO.Messages.POShipAddress)]
		public PXSelect<POShipAddress, Where<POShipAddress.addressID, Equal<Current<RQRequisition.shipAddressID>>>> Shipping_Address;
		[PXViewName(PO.Messages.POShipContact)]
		public PXSelect<POShipContact, Where<POShipContact.contactID, Equal<Current<RQRequisition.shipContactID>>>> Shipping_Contact;
		[PXViewName(PO.Messages.PORemitAddress)]
		public PXSelect<PORemitAddress, Where<PORemitAddress.addressID, Equal<Current<RQRequisition.remitAddressID>>>> Remit_Address;
		[PXViewName(PO.Messages.PORemitContact)]
		public PXSelect<PORemitContact, Where<PORemitContact.contactID, Equal<Current<RQRequisition.remitContactID>>>> Remit_Contact;

		public PXFilter<RQRequisitionStatic> Filter;
		public PXFilter<RQRequestLineFilter> RequestFilter;

		public PXSelectJoin<RQRequisitionContent,
								InnerJoin<RQRequestLine,
											 On<RQRequestLine.orderNbr, Equal<RQRequisitionContent.orderNbr>,
											 And<RQRequestLine.lineNbr, Equal<RQRequisitionContent.lineNbr>>>,
								InnerJoin<RQRequest, On<RQRequest.orderNbr, Equal<RQRequisitionContent.orderNbr>>>>,
								Where<RQRequisitionContent.reqNbr, Equal<Current<RQRequisitionStatic.reqNbr>>,
									And<RQRequisitionContent.reqLineNbr, Equal<Current<RQRequisitionStatic.lineNbr>>>>> Contents;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<RQRequestLine,
					InnerJoin<RQRequest, On<RQRequest.orderNbr, Equal<RQRequestLine.orderNbr>,
								And<RQRequest.status, Equal<RQRequestStatus.open>>>>,
					Where<RQRequestLine.openQty, Greater<PX.Objects.CS.decimal0>,
					And<RQRequestLine.orderNbr, Equal<Required<RQRequestLine.orderNbr>>,
					And<RQRequestLine.lineNbr, Equal<Required<RQRequestLine.lineNbr>>>>>> SourceRequestLines;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<RQRequestLineSelect,
						 InnerJoin<RQRequest, On<RQRequest.orderNbr, Equal<RQRequestLineSelect.orderNbr>, And<RQRequest.status, Equal<RQRequestStatus.open>>>,
							LeftJoinSingleTable<Customer,On<Customer.bAccountID, Equal<RQRequest.employeeID>>>>,			
					Where<RQRequestLineSelect.openQty, Greater<PX.Objects.CS.decimal0>,
						And2<Where<Customer.bAccountID, IsNull,
								Or<Match<Customer, Current<AccessInfo.userName>>>>, 
						And<Where<Current<RQRequisition.customerID>, IsNull,
									 Or<RQRequest.employeeID, Equal<Current<RQRequisition.customerID>>,
									And<RQRequest.locationID, Equal<Current<RQRequisition.customerLocationID>>>>>>>>> SourceRequests;

		public PXSelect<RQRequisitionOrder, Where<RQRequisitionOrder.reqNbr, Equal<Optional<RQRequisition.reqNbr>>>> ReqOrders;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<POOrder,
				InnerJoin<RQRequisitionOrder,
							 On<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.po>,
							 And<RQRequisitionOrder.orderType, Equal<POOrder.orderType>,
							And<RQRequisitionOrder.orderNbr, Equal<POOrder.orderNbr>>>>>,
			Where<RQRequisitionOrder.reqNbr, Equal<Optional<RQRequisition.reqNbr>>,
			And<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>>> POOrders;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<SOOrder,
				InnerJoin<RQRequisitionOrder,
							 On<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.so>,
							 And<RQRequisitionOrder.orderType, Equal<SOOrder.orderType>,
							And<RQRequisitionOrder.orderNbr, Equal<SOOrder.orderNbr>>>>>,
			Where<RQRequisitionOrder.reqNbr, Equal<Optional<RQRequisition.reqNbr>>>> SOOrders;

		public PXSelect<POOrder, Where<POOrder.rQReqNbr, Equal<Current<RQRequisition.reqNbr>>,
									And<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>>> Orders;
		[PXCopyPasteHiddenView]
		public PXSelectJoin<POLine,
				InnerJoin<POOrder,
							 On<POOrder.orderType, Equal<POLine.orderType>,
							And<POOrder.orderNbr, Equal<POLine.orderNbr>>>>,
			Where<POLine.rQReqNbr, Equal<Current<RQRequisitionStatic.reqNbr>>,
			And<POLine.rQReqLineNbr, Equal<Current<RQRequisitionStatic.lineNbr>>,
			And<POLine.orderType, NotEqual<POOrderType.regularSubcontract>>>>> OrderLines;

		[PXHidden]
		public PXSelect<VendorContact,
			Where<VendorContact.contactID, Equal<Current<Vendor.defContactID>>>> 
			VndrCont;

		[PXHidden]
		[CRDefaultMailTo(typeof(Select2<VendorContact, 
			InnerJoin<Vendor, On<VendorContact.contactID, Equal<Vendor.defContactID>>>, 
			Where<Vendor.bAccountID, Equal<Current<RQRequisition.vendorID>>>>))]
		public CRActivityList<RQRequisition>
			Activity;

		[PXHidden]
		[CRReference(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>>>))]
		[CRDefaultMailTo(typeof(Select2<VendorContact,
			InnerJoin<Vendor, On<VendorContact.contactID, Equal<Vendor.defContactID>>>,
			Where<Vendor.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>>>))]
		public CRActivityList<RQRequisition>
			BidActivity;

		public PXSelect<RQSetupApproval, Where<RQSetupApproval.type, Equal<RQType.requisition>>> SetupApproval;
		[PXViewName(Messages.Approval)]
		public EPApprovalAutomation<RQRequisition, RQRequisition.approved, RQRequisition.rejected, RQRequisition.hold, RQSetupApproval> Approval;

		public PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Optional<RQRequisition.vendorID>>>> bAccount;
		public PXSetup<Vendor, Where<Vendor.bAccountID, Equal<Optional<RQRequisition.vendorID>>>> vendor;
		public PXSetup<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>> vendorclass;
		public PXSetup<Location, Where<Location.bAccountID, Equal<Optional<POOrder.vendorID>>, And<Location.locationID, Equal<Optional<POOrder.vendorLocationID>>>>> location;
		public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<RQRequisition.customerID>>>> customer;
		public PXSetup<CustomerClass, Where<CustomerClass.customerClassID, Equal<Optional<Customer.customerClassID>>>> customerclass;
		public PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>>> vendorBidder;



		public PXSelect<PORemitAddress, Where<PORemitAddress.addressID, Equal<Current<RQBiddingVendor.remitAddressID>>>> Bidding_Remit_Address;
		public PXSelect<PORemitContact, Where<PORemitContact.contactID, Equal<Current<RQBiddingVendor.remitContactID>>>> Bidding_Remit_Contact;

		public PXSetup<RQSetup> Setup;
		public PXSetup<INSetup> INSetup;
		public CMSetupSelect cmsetup;
		public PXSetup<Company> company;
		public PXSelect<RQRequest> request;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Optional<RQRequisition.curyInfoID>>>> currencyinfo;
		public ToggleCurrency<RQRequisition> CurrencyView;

        #region SiteStatus Lookup
        public PXFilter<POSiteStatusFilter> sitestatusfilter;
		[PXFilterable]
		[PXCopyPasteHiddenView]
		public INSiteStatusLookup<RQSiteStatusSelected, INSiteStatusFilter> sitestatus;

		public PXAction<RQRequisition> addInvBySite;
		[PXUIField(DisplayName = "Add Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
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

		public PXAction<RQRequisition> addInvSelBySite;
		[PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable AddInvSelBySite(PXAdapter adapter)
		{
			Lines.Cache.ForceExceptionHandling = true;

			foreach (RQSiteStatusSelected line in sitestatus.Cache.Cached)
			{
				if (line.Selected == true && line.QtySelected > 0)
				{
					RQRequisitionLine newline = new RQRequisitionLine();
					newline.SiteID = line.SiteID;
					newline.InventoryID = line.InventoryID;
					newline.SubItemID = line.SubItemID;
					newline.UOM = line.PurchaseUnit;
					newline.OrderQty = line.QtySelected;
					Lines.Insert(newline);
				}
			}
			sitestatus.Cache.Clear();
			return adapter.Get();
		}
		protected virtual void POSiteStatusFilter_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			POSiteStatusFilter row = (POSiteStatusFilter)e.Row;
			if (row != null && Document.Current != null)
			{
				PXUIFieldAttribute.SetEnabled<POSiteStatusFilter.onlyAvailable>(sitestatusfilter.Cache, row, Document.Current.VendorID != null);
				row.OnlyAvailable = Document.Current.VendorID != null;
				row.VendorID = Document.Current.VendorID;
			}
		}

		public PXAction<RQRequisition> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (RQRequisition current in adapter.Get<RQRequisition>())
			{
				if (current != null)
				{
					PORemitAddress address = this.Remit_Address.Select();
					if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
					{
						PXAddressValidator.Validate<PORemitAddress>(this, address, true);
					}

					POShipAddress shipAddress = this.Shipping_Address.Select();
					if (shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
					{
						PXAddressValidator.Validate<POShipAddress>(this, shipAddress, true);
					}
				}
				yield return current;
			}
		}
		#endregion

		#region EPApproval Cahce Attached
		[PXDBDate()]
		[PXDefault(typeof(RQRequisition.orderDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(RQRequisition.employeeID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(RQRequisition.description), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		[CurrencyInfo(typeof(RQRequisition.curyInfoID))]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(RQRequisition.curyEstExtCostTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(typeof(RQRequisition.estExtCostTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDBGuid()]
		[PXDefault(typeof(RQRequisition.ownerID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region Redefine attributes
		[PXDBDate()]
		[PXUIField(DisplayName = "Entry Date", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void RQBiddingVendor_EntryDate_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>), Messages.SubcontractOrdersAreNotSupported)]
		protected virtual void POOrder_OrderNbr_CacheAttached(PXCache cache)
		{
		}

		[PXRemoveBaseAttribute(typeof(CurrencyInfoAttribute))]
		protected virtual void POOrder_CuryInfoID_CacheAttached(PXCache cache)
		{
		}

		[PXRemoveBaseAttribute(typeof(CurrencyInfoAttribute))]
		protected virtual void SOOrder_CuryInfoID_CacheAttached(PXCache cache)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>), Messages.SubcontractOrdersAreNotSupported)]
		protected virtual void POLine_PONbr_CacheAttached(PXCache cache)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[InterBranchRestrictor(typeof(Where<SameOrganizationBranch<INSite.branchID, Current<RQRequisition.branchID>>>))]
		protected virtual void POSiteStatusFilter_SiteID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		public RQRequisitionEntry()
		{
			PXUIFieldAttribute.SetEnabled(SourceRequests.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<RQRequestLineSelect.selected>(SourceRequests.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<RQRequestLineSelect.selectQty>(SourceRequests.Cache, null, true);
			this.Views.Caches.Add(typeof(RQRequestLine));
			this.SourceRequests.WhereAndCurrent<RQRequestLineFilter>(
				typeof(RQRequestLineFilter.ownerID).Name,
				typeof(RQRequestLineFilter.workGroupID).Name);
			if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				PXUIFieldAttribute.SetVisible<RQRequestLineFilter.subItemID>(this.RequestFilter.Cache, null,
					INSetup.Current.UseInventorySubItem == true);
			}

			FieldDefaulting.AddHandler<InventoryItem.stkItem>((sender, e) => { if (e.Row != null && InventoryHelper.CanCreateStockItem(sender.Graph) == false) e.NewValue = false; });

			PXStringListAttribute.SetList<InventoryItem.itemType>(
					invItems.Cache, null,
					new string[] { INItemTypes.FinishedGood, INItemTypes.Component, INItemTypes.SubAssembly, INItemTypes.NonStockItem, INItemTypes.LaborItem, INItemTypes.ServiceItem, INItemTypes.ChargeItem, INItemTypes.ExpenseItem },
					new string[] { IN.Messages.FinishedGood, IN.Messages.Component, IN.Messages.SubAssembly, IN.Messages.NonStockItem, IN.Messages.LaborItem, IN.Messages.ServiceItem, IN.Messages.ChargeItem, IN.Messages.ExpenseItem }
					);

			this.POOrders.Cache.AllowInsert = this.POOrders.Cache.AllowUpdate = this.POOrders.Cache.AllowDelete = false;
			this.SOOrders.Cache.AllowInsert = this.SOOrders.Cache.AllowUpdate = this.SOOrders.Cache.AllowDelete = false;
		}

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += _licenseLimits.GetCheckerDelegate<RQRequisition>(
					new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(RQRequisitionLine), (graph) =>
					{
						return new PXDataFieldValue[]
						{
							new PXDataFieldValue<RQRequisitionLine.reqNbr>(((RQRequisitionEntry)graph).Document.Current?.ReqNbr)
						};
					}));
			}
		}

		public PXAction<RQRequisition> ViewBidding;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(DisplayName = Messages.BiddingBtn, Visible = false)]
		public virtual IEnumerable viewBidding(PXAdapter adapter)
		{
			if (this.IsDirty) this.Save.Press();
			foreach (RQRequisition item in adapter.Get<RQRequisition>())
			{
				RQBiddingProcess graph = PXGraph.CreateInstance<RQBiddingProcess>();
				graph.Document.Current = graph.Document.Search<RQRequisition.reqNbr>(item.ReqNbr);
				if (graph.Document.Current != null)
				{
					graph.State.Current.SingleMode = true;
					throw new PXPopupRedirectException(graph, "View Bidding", true);
				}
				yield return item;
			}
		}

		public PXAction<RQRequisition> Transfer;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(Visible = false)]
		public virtual IEnumerable transfer(PXAdapter adapter)
		{
			RQRequisition destination = null;

			foreach (RQRequisition req in adapter.Get<RQRequisition>())
			{
				foreach (RQRequisitionLine line in PXSelect<RQRequisitionLine,
								Where<RQRequisitionLine.reqNbr, Equal<Required<RQRequisitionLine.reqNbr>>,
									And<RQRequisitionLine.transferRequest, Equal<Required<RQRequisitionLine.transferRequest>>>>>.Select(this, req.ReqNbr, true))
				{
					if (destination == null)
					{
						string source = null;
						if (Filter.AskExt() == WebDialogResult.OK)
							source = Filter.Current.ReqNbr;
						else
							yield break;

						if (source != null)
							this.Document.Current = this.Document.Search<RQRequisition.reqNbr>(source);
						else
							this.Document.Cache.Insert();
						destination = this.Document.Current;
					}

					this.Lines.Cache.Delete(line);

					RQRequisitionLine newLine = (RQRequisitionLine)this.Lines.Cache.CreateCopy(line);
					newLine.ReqNbr = destination.ReqNbr;
					newLine.LineNbr = null;
					newLine.AlternateID = null;
					newLine.TransferQty = 0m;
					newLine.TransferRequest = false;
					newLine.TransferType = RQTransferType.Transfer;
					newLine.SourceTranReqNbr = line.ReqNbr;
					newLine.SourceTranLineNbr = line.LineNbr;
					this.Lines.Insert(newLine);
				}

				yield return req;
			}

			if (destination == null)
				throw new PXException(Messages.TransferLinesNotExsist);
		}

		public PXAction<RQRequisition> Merge;

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.BoxIn)]
		[PXUIField(DisplayName = Messages.MergeLines)]
		public virtual IEnumerable merge(PXAdapter adapter)
		{
			foreach (RQRequisition req in adapter.Get<RQRequisition>())
			{
                var requisitionLines =
								PXSelect<RQRequisitionLine,
								Where<RQRequisitionLine.reqNbr, Equal<Required<RQRequisitionLine.reqNbr>>>,
                     OrderBy<
                         Asc<RQRequisitionLine.byRequest,
												Asc<RQRequisitionLine.inventoryID,
												Asc<RQRequisitionLine.uOM,
												Asc<RQRequisitionLine.expenseAcctID,
                         Asc<RQRequisitionLine.expenseSubID>>>>>>>
                     .Select(this, req.ReqNbr, true);

				RequisitionLinesMergeResult mergeResults = MergeLinesOfRequisition(requisitionLines);
                       
                if (!mergeResults.Merged && mergeResults.ResultLine != null)
				{
                    string warningMessage = mergeResults.ResultLine.InventoryID == null 
                        ? Messages.MergeLinesInventoryID
                        : Messages.MergeLinesNoSource;

                    var exception = new PXSetPropertyException(warningMessage, PXErrorLevel.Warning);
                    Lines.Cache.RaiseExceptionHandling<RQRequisitionLine.selected>(mergeResults.ResultLine, true, exception);
				}

				yield return req;
			}
		}

        protected virtual RequisitionLinesMergeResult MergeLinesOfRequisition(PXResultset<RQRequisitionLine> requisitionLines)
				{
            RQRequisitionLine destination = null;
            bool merged = false;

            foreach (RQRequisitionLine lineToMerge in requisitionLines)
            {
                if (lineToMerge.Selected != true)
                    continue;

					if (destination == null ||
                    destination.InventoryID != lineToMerge.InventoryID ||
                    destination.ExpenseAcctID != lineToMerge.ExpenseAcctID ||
                    destination.ExpenseSubID != lineToMerge.ExpenseSubID ||
                    destination.ByRequest != lineToMerge.ByRequest)
					{
						if (destination != null && !merged)
                    {
                        string warningMessage = destination.InventoryID == null
                            ? Messages.MergeLinesInventoryID
                            : Messages.MergeLinesNoSource;

                        Lines.Cache.RaiseExceptionHandling<RQRequisitionLine.selected>(destination, true,
                            new PXSetPropertyException(warningMessage, PXErrorLevel.Warning));
                    }

                    destination = lineToMerge;
						destination.Selected = false;
						merged = false;
						continue;
					}

					merged = true;
                decimal? curyEstUnitCost = lineToMerge.CuryEstUnitCost;
				decimal? curyEstExtCost;

				if (destination.CuryEstExtCost == 0m)
				{
						destination.CuryEstExtCost = curyEstUnitCost * destination.OrderQty;
					curyEstExtCost = destination.CuryEstExtCost;
				}
				else
				{
					curyEstExtCost = lineToMerge.CuryEstExtCost;
				}

				if (lineToMerge.ByRequest == true)
                {
                    destination = MergeRequisitionLineCreatedFromRequest(destination, lineToMerge, curyEstUnitCost, curyEstExtCost);
                }
                else
                {
                    destination = MergeRequisitionLineCreatedFromDraft(destination, lineToMerge, curyEstUnitCost, curyEstExtCost);
                }

                Lines.Cache.Delete(lineToMerge);
            }

            return new RequisitionLinesMergeResult(merged, destination);
        }

        private RQRequisitionLine MergeRequisitionLineCreatedFromRequest(RQRequisitionLine destLine, RQRequisitionLine lineToMerge,
                                                                         decimal? curyEstUnitCost, decimal? curyEstExtCost)
					{
            decimal? newCuryExyExtCost = destLine.CuryEstExtCost + curyEstExtCost;

            var rqContentAndRequestLines =
							PXSelectJoin<RQRequisitionContent,
                    InnerJoin<RQRequestLine,
                        On<RQRequestLine.orderNbr, Equal<RQRequisitionContent.orderNbr>,
									And<RQRequestLine.lineNbr, Equal<RQRequisitionContent.lineNbr>>>>,
								Where<RQRequisitionContent.reqNbr, Equal<Required<RQRequisitionContent.reqNbr>>,
                        And<RQRequisitionContent.reqLineNbr, Equal<Required<RQRequisitionContent.reqLineNbr>>>>>
                    .Select(this, lineToMerge.ReqNbr, lineToMerge.LineNbr);

            foreach (PXResult<RQRequisitionContent, RQRequestLine> rec in rqContentAndRequestLines)
						{
                RQRequisitionContent rqContent = (RQRequisitionContent)rec;
                RQRequestLine requestLine = (RQRequestLine)rec;

                Contents.Delete(rqContent);

                RQRequisitionContent content = UpdateContent(destLine, requestLine, rqContent.ItemQty ?? 0m);

                if (rqContent.ItemQty.GetValueOrDefault() > 0 && content.ReqQty.GetValueOrDefault() == 0m)
							{
                    content.ReqQty = INUnitAttribute.ConvertFromBase(Lines.Cache, lineToMerge.InventoryID, lineToMerge.UOM,
                                                                     content.BaseReqQty ?? 0m, INPrecision.QUANTITY);
                    Contents.Update(content);
							}

                if (destLine.CuryEstUnitCost != curyEstUnitCost)
							{
                    destLine = PXCache<RQRequisitionLine>.CreateCopy(destLine);
					destLine.CuryEstUnitCost = newCuryExyExtCost / destLine.OrderQty;
                    destLine = (RQRequisitionLine)Lines.Cache.Update(destLine);
						}
					}

            return destLine;
					}

		private RQRequisitionLine MergeRequisitionLineCreatedFromDraft(RQRequisitionLine destLine, RQRequisitionLine lineToMerge,
                                                                                 decimal? curyEstUnitCost, decimal? curyEstExtCostFromLineToMerge) 
				{
            destLine = PXCache<RQRequisitionLine>.CreateCopy(destLine);
            decimal? qtyFromLineToMerge = destLine.UOM == lineToMerge.UOM
                ? lineToMerge.OrderQty
                : INUnitAttribute.ConvertFromBase(Lines.Cache, lineToMerge.InventoryID, lineToMerge.UOM, lineToMerge.BaseOrderQty ?? 0m,
                                                  INPrecision.QUANTITY);

            if (destLine.CuryEstUnitCost != curyEstUnitCost)
            {
				destLine.CuryEstUnitCost = (destLine.CuryEstExtCost + curyEstExtCostFromLineToMerge) / (destLine.OrderQty + qtyFromLineToMerge);
			}

            destLine.OrderQty += qtyFromLineToMerge;
            destLine = (RQRequisitionLine)Lines.Cache.Update(destLine);
            return destLine;
		}

		public PXAction<RQRequisition> AddRequestLine;

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(DisplayName = Messages.AddRequest)]
		public virtual IEnumerable addRequestLine(PXAdapter adapter)
		{
			PXView.InitializePanel initAction = delegate(PXGraph graph, string view)
			{
				PXCache filter = graph.Caches[typeof(RQRequestLineFilter)];
				RQRequestLineFilter filterCurrent = (RQRequestLineFilter)filter.CreateCopy(filter.Current);
				filterCurrent.InventoryID = null;
				filterCurrent.SubItemID = null;
				filterCurrent.AddExists = this.Setup.Current.RequisitionMergeLines;
				filterCurrent.AllowUpdate = true;
				filter.Update(filterCurrent);
			};

			foreach (RQRequisition item in adapter.Get<RQRequisition>())
			{
				if (item.Hold == true && this.SourceRequests.AskExt(initAction) == WebDialogResult.OK)
				{
					foreach (RQRequestLineSelect line in this.SourceRequests.Select())
					{
						if (line.Selected == true && line.SelectQty > 0m)
						{
							RQRequestLine upd = this.SourceRequestLines.SelectWindowed(0, 1, line.OrderNbr, line.LineNbr);

							if (upd != null)
								InsertRequestLine(upd, line.SelectQty.GetValueOrDefault(), this.RequestFilter.Current.AddExists == true);
						}
					}

					this.SourceRequests.Cache.Clear();
					this.SourceRequests.View.Clear();
					this.SourceRequests.View.RequestRefresh();
				}

				yield return item;
			}
		}

		public PXAction<RQRequisition> AddRequestContent;

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		[PXUIField(DisplayName = Messages.AddRequest)]
		public virtual IEnumerable addRequestContent(PXAdapter adapter)
		{
			PXView.InitializePanel initAction = delegate(PXGraph graph, string view)
			{
				PXCache filter = graph.Caches[typeof(RQRequestLineFilter)];

				RQRequisitionLine line =
                    PXSelect<RQRequisitionLine,
					Where<RQRequisitionLine.reqNbr, Equal<Current<RQRequisitionStatic.reqNbr>>,
						 And<RQRequisitionLine.lineNbr, Equal<Current<RQRequisitionStatic.lineNbr>>>>>
					.Select(this);

				RQRequestLineFilter filterCurrent = (RQRequestLineFilter)filter.CreateCopy(filter.Current);

				if (line != null)
				{
					filterCurrent.InventoryID = line.InventoryID;
					filterCurrent.SubItemID = line.SubItemID;
					filterCurrent.AddExists = true;
					filterCurrent.AllowUpdate = false;
				}

				filter.Update(filterCurrent);
			};

			foreach (RQRequisition item in adapter.Get<RQRequisition>())
			{
				if (item.Hold == true && this.SourceRequests.AskExt(initAction) == WebDialogResult.OK)
				{
					foreach (RQRequestLineSelect line in this.SourceRequests.Select())
					{
						if (line.Selected == true && line.SelectQty > 0m)
						{
							RQRequestLine upd = this.SourceRequestLines.SelectWindowed(0, 1, line.OrderNbr, line.LineNbr);
							InsertRequestLine(upd, line.SelectQty.GetValueOrDefault(), true);
						}

						line.Selected = false;
						line.SelectQty = 0;
						this.SourceRequests.Cache.Update(line);
					}

					this.Contents.View.RequestRefresh();
				}

				yield return item;
			}
		}

		public PXAction<RQRequisition> ViewDetails;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(DisplayName = Messages.RequestDetails)]
		public virtual IEnumerable viewDetails(PXAdapter adapter)
		{
			PXView.InitializePanel initAction = delegate(PXGraph graph, string view)
			{
				PXCache filter = graph.Caches[typeof(RQRequisitionStatic)];
				RQRequisitionLine line = graph.Caches[typeof(RQRequisitionLine)].Current as RQRequisitionLine;
				RQRequisitionStatic filterCurrent = new RQRequisitionStatic();
				if (line != null)
				{
					filterCurrent.ReqNbr = line.ReqNbr;
					filterCurrent.LineNbr = line.LineNbr;
				}
				filter.Current = filterCurrent;
			};
			if (this.Lines.Current != null && this.Lines.Current.ByRequest == true)
				this.Contents.AskExt(initAction);
			this.Contents.ClearDialog();
			return adapter.Get();
		}

		public PXAction<RQRequisition> ViewRequest;
		[PXUIField(DisplayName = Messages.ViewRequest)]
		[PXLookupButton]
		public virtual IEnumerable viewRequest(PXAdapter adapter)
		{
			if (this.Contents.Current != null)
			{
				EntityHelper helper = new EntityHelper(this);
				helper.NavigateToRow(typeof(RQRequest).FullName, new object[] { this.Contents.Current.OrderNbr }, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<RQRequisition> ViewPOOrder;
		[PXUIField(DisplayName = Messages.ViewOrder)]
		[PXLookupButton]
		public virtual IEnumerable viewPOOrder(PXAdapter adapter)
		{
			if (this.POOrders.Current != null)
			{
				POOrderEntry graph = PXGraph.CreateInstance<POOrderEntry>();
				graph.Document.Current = graph.Document.Search<POOrder.orderNbr>(this.POOrders.Current.OrderNbr, this.POOrders.Current.OrderType);
				if (graph.Document.Current != null)
					throw new PXRedirectRequiredException(graph, true, "View Order"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}
			return adapter.Get();
		}
		public PXAction<RQRequisition> ViewSOOrder;
		[PXUIField(DisplayName = Messages.ViewOrder)]
		[PXLookupButton]
		public virtual IEnumerable viewSOOrder(PXAdapter adapter)
		{
			if (this.SOOrders.Current != null)
			{
				SOOrderEntry graph = PXGraph.CreateInstance<SOOrderEntry>();
				graph.Document.Current = graph.Document.Search<SOOrder.orderNbr>(this.SOOrders.Current.OrderNbr, this.SOOrders.Current.OrderType);
				if (graph.Document.Current != null)
					throw new PXRedirectRequiredException(graph, true, "View Order") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}
		public PXAction<RQRequisition> ViewOrderByLine;
		[PXUIField(DisplayName = Messages.ViewOrder)]
		[PXLookupButton]
		public virtual IEnumerable viewOrderByLine(PXAdapter adapter)
		{
			if (this.OrderLines.Current != null)
			{
				POOrderEntry graph = PXGraph.CreateInstance<POOrderEntry>();
				graph.Document.Current = graph.Document.Search<POOrder.orderNbr>(this.OrderLines.Current.OrderNbr, this.OrderLines.Current.OrderType);
				if (graph.Document.Current != null)
					throw new PXRedirectRequiredException(graph, true, "View Order"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}
			return adapter.Get();
		}

		public PXAction<RQRequisition> Assign;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(DisplayName = Messages.Assign, Visible = false)]
		public virtual IEnumerable assign(PXAdapter adapter)
		{
			foreach (RQRequisition req in adapter.Get<RQRequisition>())
			{
				if (Setup.Current.RequisitionAssignmentMapID != null)
				{
					var processor = new EPAssignmentProcessor<RQRequisition>();
					processor.Assign(req, Setup.Current.RequisitionAssignmentMapID);
                    req.WorkgroupID = req.ApprovalWorkgroupID;
                    req.OwnerID = req.ApprovalOwnerID;

				}
				yield return req;
			}
		}

		public PXAction<RQRequisition> createPOOrder;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(DisplayName = Messages.CreateOrders)]
		public virtual IEnumerable CreatePOOrder(PXAdapter adapter)
		{
			foreach (RQRequisition item in adapter.Get<RQRequisition>())
			{
				this.Document.Current = item;
				bool validateResult = true;

				foreach (RQRequisitionLine line in this.Lines.Select(item.ReqNbr))
				{
					if (!ValidateOpenState(line, PXErrorLevel.Error))
						validateResult = false;
				}

				if (!validateResult)
					throw new PXRowPersistingException(typeof(RQRequisition).Name, item, Messages.UnableToCreateOrders);

				this.Document.Current = this.Document.Search<RQRequisition.reqNbr>(item.ReqNbr);

				using (PXTransactionScope scope = new PXTransactionScope())
				{
					try
					{
						POOrderEntry graph = PXGraph.CreateInstance<POOrderEntry>();			
						graph.TimeStamp = this.TimeStamp;
						graph.Document.Current = null;

						POOrder oldOrder = 
                            PXSelect<POOrder, 
							   Where<POOrder.rQReqNbr, Equal<Required<POOrder.rQReqNbr>>>>
							.SelectWindowed(this, 0, 1, item.ReqNbr);

						if (oldOrder == null)
						{
							PO4SO po4so = new PO4SO();

							if (item.VendorID != null && item.VendorLocationID != null)
							{
								POOrder order = (POOrder)graph.Document.Cache.CreateInstance();
								order.OrderType = item.POType;
								order.OrderDesc = item.Description;

								foreach (PXResult<RQRequisitionLine, RQBidding, RQBiddingVendor,RQRequisitionLineCustomers> rec in
									PXSelectJoin<RQRequisitionLine,
										LeftJoin<RQBidding,
													On<RQBidding.reqNbr, Equal<RQRequisitionLine.reqNbr>,
													And<RQBidding.lineNbr, Equal<RQRequisitionLine.lineNbr>,
													And<RQBidding.vendorID, Equal<Current<RQRequisition.vendorID>>,
													And<RQBidding.vendorLocationID, Equal<Current<RQRequisition.vendorLocationID>>>>>>,
										LeftJoin<RQBiddingVendor,
												On<RQBiddingVendor.reqNbr, Equal<Current<RQRequisition.reqNbr>>,
												And<RQBiddingVendor.vendorID, Equal<Current<RQRequisition.vendorID>>,
												And<RQBiddingVendor.vendorLocationID, Equal<Current<RQRequisition.vendorLocationID>>>>>,
										LeftJoin<RQRequisitionLineCustomers,
													On<RQRequisitionLineCustomers.reqNbr, Equal<RQRequisitionLine.reqNbr>,
												 And<RQRequisitionLineCustomers.reqLineNbr, Equal<RQRequisitionLine.lineNbr>>>>>>,
									Where<RQRequisitionLine.reqNbr, Equal<Current<RQRequisition.reqNbr>>>>
									.SelectMultiBound(this, new object[]{item}))
								{
									RQRequisitionLine line = rec;
									RQRequisitionLineCustomers cust = rec;
									RQBiddingVendor biddier = rec;
									RQBidding bid = rec;

                                    decimal custqty = 0;
                                    decimal qty = line.OrderQty.Value;

									if (bid.OrderQty == null || (bid.OrderQty == 0 && qty < bid.MinQty)) 
                                    {
                                        bid = new RQBidding
                                        {
                                            MinQty = 0,
                                            QuoteQty = 0,
                                            OrderQty = 0,
                                            CuryQuoteUnitCost = 0
                                        };
                                    }
									
                                    if (bid.OrderQty == 0)
                                    {
                                        bid.OrderQty = bid.QuoteQty;
                                    }
								
                                    if (cust.ReqQty > 0)
                                    {
                                        custqty = cust.ReqQty.Value;
                                    }

                                    if (item.CustomerLocationID != null)
                                    {
                                        custqty = qty;
                                    }

                                    if (custqty > 0)
                                    {
                                        qty -= custqty;
                                    }
	
									if (order != null)
									{
										order = PXCache<POOrder>.CreateCopy(graph.Document.Insert(order));

										if (bid.CuryInfoID != null)
										{
											order.CuryID = (string)Bidding.GetValueExt<RQBidding.curyID>(bid);
											order.CuryInfoID = CopyCurrenfyInfo(graph, bid.CuryInfoID);
										}
										else
										{
											order.CuryID = item.CuryID;
											order.CuryInfoID = CopyCurrenfyInfo(graph, item.CuryInfoID);											
										}
										order = PXCache<POOrder>.CreateCopy(graph.Document.Update(order));

										order.VendorID = item.VendorID;
										order.VendorLocationID = item.VendorLocationID;
										order.RemitAddressID = item.RemitAddressID;
										order.RemitContactID = item.RemitContactID;
										order.VendorRefNbr = item.VendorRefNbr;
										order.TermsID = item.TermsID;

										order = PXCache<POOrder>.CreateCopy(graph.Document.Update(order));

										if (biddier?.PromisedDate != null)
										{
											order.ExpectedDate = biddier.PromisedDate;
										}

										order.FOBPoint = item.FOBPoint;
										order.ShipDestType = item.ShipDestType;
										order.SiteID = item.SiteID;
										order.ShipToBAccountID = item.ShipToBAccountID;
										order.ShipToLocationID = item.ShipToLocationID;
										order.ShipContactID = item.ShipContactID;
										order.ShipAddressID = item.ShipAddressID;
										order.ShipContactID = item.ShipContactID;
										order.ShipVia = item.ShipVia;
										order.FOBPoint = item.FOBPoint;
										order.RQReqNbr = item.ReqNbr;

										order = graph.Document.Update(order);
										order = null;
									}

									InventoryItem inv = InventoryItem.PK.Find(graph, line.InventoryID);
									string lineType = inv != null && inv.StkItem == true
                                        ? POLineType.GoodsForSalesOrder
                                        : line.LineType;									
									
									decimal minQty = custqty < bid.OrderQty
                                        ? custqty
                                        : bid.OrderQty.Value;

									po4so.Add(line.LineNbr, InsertPOLine(graph, line, minQty, bid.CuryQuoteUnitCost, bid, lineType, biddier?.PromisedDate));
								
									custqty -= minQty;
									bid.OrderQty -= minQty;

									if (custqty > 0)
                                    {
                                        po4so.Add(line.LineNbr, InsertPOLine(graph, line, custqty, line.CuryEstUnitCost, line, lineType, biddier?.PromisedDate));
                                    }

									minQty = qty < bid.OrderQty 
                                        ? qty 
                                        : bid.OrderQty.Value;

									InsertPOLine(graph, line, minQty, bid.CuryQuoteUnitCost, bid, line.LineType, biddier?.PromisedDate);

                                    qty -= minQty;

                                    if (qty > 0)
                                    {
                                        InsertPOLine(graph, line, qty, line.CuryEstUnitCost, line, line.LineType, biddier?.PromisedDate);
                                    }
								}
							}
							else
							{
								Dictionary<int?, decimal> usedLine = new Dictionary<int?, decimal>();

								foreach (PXResult<RQBidding, RQRequisitionLine, RQRequisitionLineCustomers, Vendor, RQBiddingVendor> e
										in
									PXSelectJoin<RQBidding,
									InnerJoin<RQRequisitionLine,
												 On<RQRequisitionLine.reqNbr, Equal<RQBidding.reqNbr>,
												And<RQRequisitionLine.lineNbr, Equal<RQBidding.lineNbr>>>,
									LeftJoin<RQRequisitionLineCustomers,
												On<RQRequisitionLineCustomers.reqNbr, Equal<RQRequisitionLine.reqNbr>,
											 And<RQRequisitionLineCustomers.reqLineNbr, Equal<RQRequisitionLine.lineNbr>>>,
									InnerJoin<Vendor,
													On<Vendor.bAccountID, Equal<RQBidding.vendorID>>,
									LeftJoin<RQBiddingVendor,
												On<RQBiddingVendor.reqNbr, Equal<RQBidding.reqNbr>,
												And<RQBiddingVendor.vendorID, Equal<RQBidding.vendorID>,
												And<RQBiddingVendor.vendorLocationID, Equal<RQBidding.vendorLocationID>>>>>>>>,
								 Where<RQBidding.reqNbr, Equal<Required<RQBidding.reqNbr>>,
									 And<RQBidding.orderQty, Greater<CS.decimal0>>>,
								OrderBy<Asc<RQBidding.vendorID, Asc<RQBidding.vendorLocationID, Asc<RQRequisitionLine.lineNbr>>>>>.Select(this, item.ReqNbr))
								{
									RQBidding bidding = e;
									RQRequisitionLine line = e;
									RQBiddingVendor biddier = e;
									RQRequisitionLineCustomers cust = e;
									Vendor vendor = e;
									POOrder order = null;

									if (graph.Document.Current == null ||
										 graph.Document.Current.VendorID != bidding.VendorID ||
										 graph.Document.Current.VendorLocationID != bidding.VendorLocationID)
									{
										if (graph.IsDirty)
										{
											PersistOrder(graph);
											graph.Clear();
											graph.TimeStamp = this.TimeStamp;
										}

										order = (POOrder)graph.Document.Cache.CreateInstance();
									}

									decimal custQty = 0;
									decimal qty = bidding.OrderQty.Value;

									int? key = bidding != null 
                                        ? bidding.LineID 
                                        : line.LineNbr;

									if (cust.ReqQty == null && item.CustomerLocationID != null)
										custQty = qty;

									if (cust.ReqQty > 0)
										custQty = cust.ReqQty.Value;


									if (custQty > 0 && usedLine.ContainsKey(key))
										custQty -= usedLine[key];

									if (custQty > qty)
										custQty = qty;

									if (qty > 0 && order != null)
									{
										order.OrderType = item.POType;
										order.OrderDesc = item.Description;
										order = PXCache<POOrder>.CreateCopy(graph.Document.Insert(order));
										order.CuryID = (string)Bidding.GetValueExt<RQBidding.curyID>(bidding);
										order.CuryInfoID = CopyCurrenfyInfo(graph, bidding.CuryInfoID);
										order = PXCache<POOrder>.CreateCopy(graph.Document.Update(order));
										order.VendorID = bidding.VendorID;
										order.VendorLocationID = bidding.VendorLocationID;
										order = PXCache<POOrder>.CreateCopy(graph.Document.Update(order));

										if (biddier?.PromisedDate != null)
										{
											order.ExpectedDate = biddier.PromisedDate;
										}

										order.ShipDestType = item.ShipDestType;
										order.ShipToBAccountID = item.ShipToBAccountID;
										order.ShipToLocationID = item.ShipToLocationID;
										order.ShipContactID = item.ShipContactID;
										order.ShipAddressID = item.ShipAddressID;
										order.ShipVia = biddier.ShipVia;
										order.FOBPoint = biddier.FOBPoint;
										order.RemitAddressID = biddier.RemitAddressID;
										order.RemitContactID = biddier.RemitContactID;
										order.RQReqNbr = item.ReqNbr;
										order = graph.Document.Update(order);
										order = null;
									}

									if (custQty > 0)
									{
										po4so.Add(line.LineNbr, InsertPOLine(graph, line, custQty, bidding.CuryQuoteUnitCost, bidding, line.LineType, biddier?.PromisedDate));

                                        if (!usedLine.ContainsKey(key))
                                        {
                                            usedLine[key] = 0;
                                        }

										usedLine[key] += custQty;
									}

									if (qty > custQty)
										InsertPOLine(graph, line, qty - custQty, bidding.CuryQuoteUnitCost, bidding, line.LineType, biddier?.PromisedDate);
								}
							}
							
							PersistOrder(graph);
							graph.Clear();

                            CreateSOOrderAndCloseAllQuotations(po4so, item);
                        }

						if (this.IsDirty)
                            this.Save.Press();
					}
					catch
							{
						this.Clear();
						throw;
					}

					scope.Complete();
					yield return item;
				}
			}
		}

        private void CreateSOOrderAndCloseAllQuotations(PO4SO po4so, RQRequisition requisition)
        {
            if (po4so.Count == 0)
                return;

								SOOrderEntry sograph = PXGraph.CreateInstance<SOOrderEntry>();
								PXCache vendorCache = sograph.Caches[typeof(Vendor)];

								sograph.TimeStamp = this.TimeStamp;
								sograph.Document.Current = null;
								sograph.SOPOLinkShowDocumentsOnHold = true;

            foreach (RQSOSource rqSourceForSO in GetSOSource(requisition))
            {
                CreateSOOrderAndSOLines(sograph, requisition, rqSourceForSO, po4so);
            }

            if (sograph.IsDirty)
                sograph.Save.Press();

            //Close all quotation
            foreach (RQRequisitionOrder requisitionOrder in
                PXSelect<RQRequisitionOrder,
                Where<RQRequisitionOrder.reqNbr, Equal<Required<RQRequisitionOrder.reqNbr>>,
                    And<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.so>>>>
                 .Select(this, requisition.ReqNbr))
            {
                sograph.Document.Current = sograph.Document.Search<SOOrder.orderNbr>(requisitionOrder.OrderNbr, requisitionOrder.OrderType);

                if (sograph.Document.Current != null && sograph.Document.Current.OrderType == "QT")
                {
                    SOOrder upd = PXCache<SOOrder>.CreateCopy(sograph.Document.Current);
                    upd.Status = SOOrderStatus.Completed;
                    upd.Completed = true;

                    sograph.Document.Update(upd);
                    sograph.Save.Press();
                }
            }
        }

        private IEnumerable<RQSOSource> GetSOSource(RQRequisition requisition)
        {
            IEnumerable<RQSOSource> resQuery;

            if (requisition.CustomerLocationID != null)
            {
                resQuery = Lines.Select(requisition.ReqNbr)
                                .RowCast<RQRequisitionLine>()
                                .Select(rqLine => CreateRQSOSourceFromRequisitionLine(requisition, rqLine));
                return resQuery;
            }

            var rqContentBqlQuery =
                PXSelectJoin<RQRequisitionContent,
                    InnerJoin<RQRequestLine,
                        On<RQRequestLine.orderNbr, Equal<RQRequisitionContent.orderNbr>,
                        And<RQRequestLine.lineNbr, Equal<RQRequisitionContent.lineNbr>>>,
                    InnerJoin<RQRequest,
                        On<RQRequest.orderNbr, Equal<RQRequisitionContent.orderNbr>>,
                    InnerJoin<RQRequestClass,
                        On<RQRequestClass.reqClassID, Equal<RQRequest.reqClassID>>,
                    InnerJoin<RQRequisitionLine,
                        On<RQRequisitionLine.reqNbr, Equal<RQRequisitionContent.reqNbr>,
                        And<RQRequisitionLine.lineNbr, Equal<RQRequisitionContent.reqLineNbr>>>>>>>,
                    Where<RQRequisitionContent.reqNbr, Equal<Required<RQRequisitionContent.reqNbr>>,
                        And<RQRequestClass.customerRequest, Equal<CS.boolTrue>>>,
                    OrderBy<
                        Asc<RQRequest.employeeID,
                        Asc<RQRequisitionContent.reqLineNbr>>>>
                    .Select(this, requisition.ReqNbr).AsEnumerable();

            resQuery = rqContentBqlQuery.Select(res => (PXResult<RQRequisitionContent, RQRequestLine, RQRequest,
                                                                 RQRequestClass, RQRequisitionLine>)res)
                                        .Select(res => CreateRQSOSourceFromRequest(res, res, res, res));
            return resQuery;
        }

        /// <summary>
        /// Creates <see cref="RQSOSource"/> from requisition line on SO order creation. This method is an extension point for Lexware PriceUnit customization. 
        /// </summary>
        /// <param name="requisition">The requisition.</param>
        /// <param name="rqLine">The requisition line.</param>
        /// <returns/>       
        protected virtual RQSOSource CreateRQSOSourceFromRequisitionLine(RQRequisition requisition, RQRequisitionLine rqLine)
        {
            return new RQSOSource
            {
                LineNbr = rqLine.LineNbr.GetValueOrDefault(),
                CustomerID = requisition.CustomerID,
                CustomerLocationID = requisition.CustomerLocationID,
                InventoryID = rqLine.InventoryID,
                UOM = rqLine.UOM,
                SubItemID = rqLine.SubItemID,
                OrderQty = rqLine.OrderQty,
                IsUseMarkup = rqLine.IsUseMarkup,
                MarkupPct = rqLine.MarkupPct,
                EstUnitCost = rqLine.EstUnitCost,
                CuryEstUnitCost = rqLine.CuryEstUnitCost,
				Description = rqLine.Description
            };
        }

        /// <summary>
        /// Creates <see cref="RQSOSource"/> from request line on SO order creation. This method is an extension point for Lexware PriceUnit customization. 
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="requestLine">The request line.</param>
        /// <param name="rqContent">The request content.</param>
        /// <param name="rqLine">The requisition line.</param>
        /// <returns/>       
        protected virtual RQSOSource CreateRQSOSourceFromRequest(RQRequest request, RQRequestLine requestLine, RQRequisitionContent rqContent, 
																 RQRequisitionLine rqLine)
        {
            return new RQSOSource
            {
                UOM = requestLine.UOM,
                CustomerID = request.EmployeeID,
                CustomerLocationID = request.LocationID,
                LineNbr = rqContent.ReqLineNbr.GetValueOrDefault(),
                InventoryID = requestLine.InventoryID,
                SubItemID = requestLine.SubItemID,
                OrderQty = rqContent.ReqQty,
                IsUseMarkup = rqLine.IsUseMarkup,
                MarkupPct = rqLine.MarkupPct,
                EstUnitCost = requestLine.EstUnitCost,
                CuryEstUnitCost = requestLine.CuryEstUnitCost,
				Description = requestLine.Description
            };
        }

		/// <summary>
		/// Creates SO order and SO lines on PO order creation.
		/// </summary>
		/// <param name="sograph">The <see cref="SOOrderEntry"/> graph.</param>
		/// <param name="requisition">The requisition.</param>
		/// <param name="rqSourceForSO">The requisition DTO for SO documents.</param>
		/// <param name="po4so">The PO for SO DTO.</param>
		private void CreateSOOrderAndSOLines(SOOrderEntry sograph, RQRequisition requisition, RQSOSource rqSourceForSO, PO4SO po4so)
								{
									if (sograph.Document.Current == null ||
                sograph.Document.Current.CustomerID != rqSourceForSO.CustomerID ||
                sograph.Document.Current.CustomerLocationID != rqSourceForSO.CustomerLocationID)
									{
										if (sograph.IsDirty)
											sograph.Save.Press();

                PXResultset<SOOrder> quotes =
                    PXSelectJoin<SOOrder,
									        InnerJoin<RQRequisitionOrder,
									            On<RQRequisitionOrder.orderType, Equal<SOOrder.orderType>,
									           And<RQRequisitionOrder.orderNbr, Equal<SOOrder.orderNbr>>>>,
									        Where<RQRequisitionOrder.orderCategory, Equal<Required<RQRequisitionOrder.orderCategory>>,
									          And<RQRequisitionOrder.reqNbr, Equal<Required<RQRequisitionOrder.reqNbr>>,
                        And<SOOrder.status, Equal<SOOrderStatus.open>>>>>
                    .SelectWindowed(this, 0, 2, RQOrderCategory.SO, requisition.ReqNbr);

										SOOrder order = (SOOrder)sograph.Document.Cache.CreateInstance();
										string soOrderType = "SO";

										if (!PXAccess.FeatureInstalled<FeaturesSet.inventory>())
											soOrderType = "IN";

                SOOrderType OrderType = SOOrderType.PK.Find(this, soOrderType);

										if (OrderType == null || OrderType.Active != true)
										{
											throw new PXException(Messages.UnableToCreateSOOrderOrderTypeInactive, soOrderType);
										}

										order.OrderType = soOrderType;

										order = sograph.Document.Insert(order);
										order = PXCache<SOOrder>.CreateCopy(sograph.Document.Search<SOOrder.orderNbr>(order.OrderNbr));

                order.CustomerID = rqSourceForSO.CustomerID;
                order.CustomerLocationID = rqSourceForSO.CustomerLocationID;

										order = PXCache<SOOrder>.CreateCopy(sograph.Document.Update(order));

                order.CuryID = requisition.CuryID;
                order.CuryInfoID = CopyCurrenfyInfo(sograph, requisition.CuryInfoID);

									    if (quotes.Count == 1)
									    {
									        SOOrder quote = quotes[0];
									        order.OrigOrderType = quote.OrderType;
									        order.OrigOrderNbr = quote.OrderNbr;
									    }

										order = sograph.Document.Update(order);
										sograph.Save.Press();

										RQRequisitionOrder link = new RQRequisitionOrder();
										link.OrderCategory = RQOrderCategory.SO;
										link.OrderType = sograph.Document.Current.OrderType;
										link.OrderNbr = sograph.Document.Current.OrderNbr;
										this.ReqOrders.Insert(link);
									}

            if (!po4so.ContainsKey(rqSourceForSO.LineNbr))
                return;

            foreach (POLine poLine in po4so[rqSourceForSO.LineNbr].Where(po4soLine => po4soLine.OrderQty > 0))
									{
                decimal qty = poLine.OrderQty.Value;

                if (rqSourceForSO.OrderQty < qty)
                    qty = rqSourceForSO.OrderQty.Value;

                if (qty <= 0)
                    continue;

                SOLine newSoLine = CreateSOLineFromPO4SOLine(sograph, poLine, qty, requisition, rqSourceForSO);

                foreach (POLine3 supply in sograph.POSupply())
										{
                    if (supply.OrderType == poLine.OrderType && supply.OrderNbr == poLine.OrderNbr && supply.LineNbr == poLine.LineNbr)
                    {
                        supply.Selected = true;
                        sograph.posupply.Update(supply);
                        break;
                    }
                }

                sograph.LinkSupplyDemand();

                if (sograph.IsDirty)
                    sograph.Save.Press();

                poLine.OrderQty -= qty;
                rqSourceForSO.OrderQty -= qty;
            }
        }

		/// <summary>
		/// Creates SO line from PO for SO (<see cref="PO4SO"/>) line on Create Orders action. This is an extension point used by Lexware customization.
		/// </summary>
		/// <param name="soGraph">The SO graph.</param>
		/// <param name="poLine">The PO line.</param>
		/// <param name="qty">The quantity.</param>
		/// <param name="requisition">The requisition.</param>
		/// <param name="rqSourceForSO">The requisition DTO for SO documents.</param>
		/// <returns/>       
		protected virtual SOLine CreateSOLineFromPO4SOLine(SOOrderEntry soGraph, POLine poLine, decimal qty, RQRequisition requisition, 
														   RQSOSource rqSourceForSO)
											{
												SOLine quote =
													PXSelectJoin<SOLine,
													InnerJoin<RQRequisitionLine,
																 On<RQRequisitionLine.qTOrderNbr, Equal<SOLine.orderNbr>,
																And<RQRequisitionLine.qTLineNbr, Equal<SOLine.lineNbr>>>,
													InnerJoin<SOOrder,
																 On<SOOrder.orderType, Equal<SOLine.orderType>,
																 And<SOOrder.orderNbr, Equal<SOLine.orderNbr>,
																And<SOOrder.status, Equal<SOOrderStatus.open>>>>>>,
													Where<SOLine.orderType, Equal<Required<SOLine.orderType>>,
														And<RQRequisitionLine.reqNbr, Equal<Required<RQRequisitionLine.reqNbr>>,
														And<RQRequisitionLine.lineNbr, Equal<Required<RQRequisitionLine.lineNbr>>>>>>
                	.Select(this, "QT", poLine.RQReqNbr, poLine.RQReqLineNbr);

            SOLine newSoLine = (SOLine)soGraph.Transactions.Cache.CreateInstance();

            newSoLine.OrderType = soGraph.Document.Current.OrderType;
            newSoLine.OrderNbr = soGraph.Document.Current.OrderNbr;

            newSoLine = PXCache<SOLine>.CreateCopy(soGraph.Transactions.Insert(newSoLine));
            newSoLine.InventoryID = rqSourceForSO.InventoryID;

            if (newSoLine.InventoryID != null)
                newSoLine.SubItemID = rqSourceForSO.SubItemID;

            newSoLine.UOM = rqSourceForSO.UOM;
            newSoLine.Qty = qty;
            newSoLine.SiteID = poLine.SiteID;
			newSoLine.TranDesc = rqSourceForSO.Description;

												if (quote != null)
												{
                FillSOLineFromQuotation(newSoLine, quote);
												}
            else if (rqSourceForSO.IsUseMarkup == true)
												{
                decimal profit = 1m + (rqSourceForSO.MarkupPct.GetValueOrDefault() / 100m);

                FillSOLine(soGraph, newSoLine, rqSourceForSO,
                                            areCurrenciesSame: soGraph.Document.Current.CuryID == requisition.CuryID,
                                            profitMultiplier: profit);
												}
												
            newSoLine.POCreate = true;
            newSoLine.POSource = requisition.POType == POOrderType.DropShip
                ? INReplenishmentSource.DropShipToOrder
                : INReplenishmentSource.PurchaseToOrder;

            newSoLine.VendorID = poLine.VendorID;

            newSoLine = PXCache<SOLine>.CreateCopy(soGraph.Transactions.Update(newSoLine));
            return newSoLine;
								}

        /// <summary>
        /// Fill SO line from the quotation on POOrder creation. This is an extension point used by Lexware PriceUnit customization.
        /// </summary>
        /// <param name="newSoLine">The new SO line.</param>
        /// <param name="quote">The quotation.</param>
        protected virtual void FillSOLineFromQuotation(SOLine newSoLine, SOLine quote) 
									{
            newSoLine.ManualPrice = true;
            newSoLine.CuryUnitPrice = quote.CuryUnitPrice;
            newSoLine.OrigOrderType = quote.OrderType;
            newSoLine.OrigOrderNbr = quote.OrderNbr;
            newSoLine.OrigLineNbr = quote.LineNbr;
						}

        /// <summary>
        /// Fill SO line from the requisition source for SO on POOrder creation. This is an extension point used by Lexware PriceUnit customization.
        /// </summary>
        /// <param name="sograph">The SO graph.</param>
        /// <param name="newSoLine">The new SO line.</param>
        /// <param name="rqSourceForSO">The requisition source for SO.</param>
        /// <param name="areCurrenciesSame">True if are currencies same.</param>
        /// <param name="profitMultiplier">The profit multiplier.</param>
        protected virtual void FillSOLine(SOOrderEntry sograph, SOLine newSoLine, RQSOSource rqSourceForSO, bool areCurrenciesSame,
										  decimal profitMultiplier)
					{
            newSoLine.ManualPrice = true;

            if (areCurrenciesSame)
			{
                newSoLine.CuryUnitPrice = rqSourceForSO.CuryEstUnitCost * profitMultiplier;
			}
			else
			{
                newSoLine.UnitPrice = rqSourceForSO.EstUnitCost * profitMultiplier;
                PXCurrencyAttribute.CuryConvCury<SOLine.curyInfoID>(sograph.Transactions.Cache, newSoLine);
			}
		}
     
		public PXAction<RQRequisition> createQTOrder;

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(DisplayName = Messages.CreateQuotation)]
		public virtual IEnumerable CreateQTOrder(PXAdapter adapter)
		{
			SOOrderEntry sograph = PXGraph.CreateInstance<SOOrderEntry>();
			List<SOOrder> newSOOrders = new List<SOOrder>();

			foreach (RQRequisition requisition in adapter.Get<RQRequisition>())
			{
				RQRequisition result = requisition;

				RQRequisitionOrder req =
				PXSelectJoin<RQRequisitionOrder,
					InnerJoin<SOOrder,
								 On<SOOrder.orderNbr, Equal<RQRequisitionOrder.orderNbr>,
								And<SOOrder.status, Equal<SOOrderStatus.open>>>>,
					Where<RQRequisitionOrder.reqNbr, Equal<Required<RQRequisitionOrder.reqNbr>>,
						And<RQRequisitionOrder.orderCategory, Equal<RQOrderCategory.so>>>>
					.Select(this, requisition.ReqNbr);

				if (requisition.CustomerID != null && req == null)
				{
					this.Document.Current = requisition;

					bool validateResult = true;

					foreach (RQRequisitionLine line in this.Lines.Select(requisition.ReqNbr))
					{
						if (!ValidateOpenState(line, PXErrorLevel.Error))
							validateResult = false;
					}

					if (!validateResult)
						throw new PXRowPersistingException(typeof(RQRequisition).Name, requisition, Messages.UnableToCreateOrders);

					sograph.TimeStamp = this.TimeStamp;
					sograph.Document.Current = null;

					foreach (PXResult<RQRequisitionLine, InventoryItem> r in
						PXSelectJoin<RQRequisitionLine,
						LeftJoin<InventoryItem,
							On<RQRequisitionLine.FK.InventoryItem>>,
						Where<RQRequisitionLine.reqNbr, Equal<Required<RQRequisition.reqNbr>>>>.Select(this, requisition.ReqNbr))
					{
						RQRequisitionLine rqLine = r;
						InventoryItem inventoryItem = r;

                        CreateQTOrderAndLines(sograph, requisition, rqLine, inventoryItem, newSOOrders);
                    }

					using (PXTransactionScope scope = new PXTransactionScope())
					{
						try
						{
                            if (sograph.IsDirty)
                            {
                                sograph.Save.Press();
                            }

							RQRequisition upd = PXCache<RQRequisition>.CreateCopy(requisition);
							upd.Quoted = true;
							result = this.Document.Update(upd);
							this.Save.Press();
						}
						catch
						{
							this.Clear();
							throw;
						}

						scope.Complete();
					}
				}
				else
				{
					RQRequisition upd = PXCache<RQRequisition>.CreateCopy(requisition);
					upd.Quoted = true;
					result = this.Document.Update(upd);
					this.Save.Press();
				}

				yield return result;
			}

			if (newSOOrders.Count == 1 && adapter.MassProcess == true)
			{
				sograph.Clear();
				sograph.SelectTimeStamp();
				sograph.Document.Current = newSOOrders[0];
				throw new PXRedirectRequiredException(sograph, SO.Messages.SOOrder);
			}
		}

        /// <summary>
        /// Creates quotation order and its lines. This is an extension point used by Lexware PriceUnit customization.
        /// </summary>
        /// <param name="sograph">The SO graph.</param>
        /// <param name="requisition">The requisition.</param>
        /// <param name="rqLine">The request line.</param>
        /// <param name="inventoryItem">The inventory item.</param>
        /// <param name="newSOOrders">The new SO orders.</param>
        protected virtual void CreateQTOrderAndLines(SOOrderEntry sograph, RQRequisition requisition, RQRequisitionLine rqLine, InventoryItem inventoryItem,
                                                     List<SOOrder> newSOOrders)
        {
            RQBidding bidding = GetBiddingForQTOrderCreation(requisition, rqLine);

						if (sograph.Document.Current == null)
						{
							SOOrder order = (SOOrder)sograph.Document.Cache.CreateInstance();

							order.OrderType = "QT";

							order = sograph.Document.Insert(order);
							order = PXCache<SOOrder>.CreateCopy(sograph.Document.Search<SOOrder.orderNbr>(order.OrderNbr));

                order.CustomerID = requisition.CustomerID;
                order.CustomerLocationID = requisition.CustomerLocationID;

							order = PXCache<SOOrder>.CreateCopy(sograph.Document.Update(order));
                order.CuryID = requisition.CuryID;
                order.CuryInfoID = CopyCurrenfyInfo(sograph, requisition.CuryInfoID);

							sograph.Document.Update(order);
							sograph.Save.Press();

							order = sograph.Document.Current;
                newSOOrders.Add(order);

                RQRequisitionOrder link = new RQRequisitionOrder
                {
                    OrderCategory = RQOrderCategory.SO,
                    OrderType = order.OrderType,
                    OrderNbr = order.OrderNbr
                };

                ReqOrders.Insert(link);
						}

            SOLine newSoLine = (SOLine)sograph.Transactions.Cache.CreateInstance();

            newSoLine.OrderType = sograph.Document.Current.OrderType;
            newSoLine.OrderNbr = sograph.Document.Current.OrderNbr;

            newSoLine = PXCache<SOLine>.CreateCopy(sograph.Transactions.Insert(newSoLine));

            newSoLine.InventoryID = rqLine.InventoryID;

            if (newSoLine.InventoryID != null)
                newSoLine.SubItemID = rqLine.SubItemID;

            newSoLine.UOM = rqLine.UOM;
            newSoLine.Qty = rqLine.OrderQty;

            if (rqLine.SiteID != null)
                newSoLine.SiteID = rqLine.SiteID;

            if (rqLine.IsUseMarkup == true)
						{
                decimal profit = 1m + (rqLine.MarkupPct.GetValueOrDefault() / 100m);
                FillSOLine(sograph, newSoLine, requisition, rqLine, bidding, profit);
            }

			newSoLine.TranDesc = rqLine.Description;

            newSoLine = PXCache<SOLine>.CreateCopy(sograph.Transactions.Update(newSoLine));
            RQRequisitionLine upd = PXCache<RQRequisitionLine>.CreateCopy(rqLine);

            rqLine.QTOrderNbr = newSoLine.OrderNbr;
            rqLine.QTLineNbr = newSoLine.LineNbr;
            this.Lines.Update(rqLine);
        }

        private RQBidding GetBiddingForQTOrderCreation(RQRequisition requisition, RQRequisitionLine rqLine)
							{
            if (requisition.VendorID == null)
            {
                
                 return PXSelect<RQBidding,
                 	       Where<RQBidding.reqNbr, Equal<Current<RQRequisitionLine.reqNbr>>,
                 		     And<RQBidding.lineNbr, Equal<Current<RQRequisitionLine.lineNbr>>,
                 		     And<RQBidding.orderQty, Greater<decimal0>>>>,
                 	     OrderBy<
                 		         Desc<RQBidding.quoteUnitCost>>>
                 	    .SelectSingleBound(this, new object[] { rqLine });
							}
							else
							{
                return PXSelect<RQBidding,
                          Where<RQBidding.reqNbr, Equal<Current<RQRequisitionLine.reqNbr>>,
                            And<RQBidding.lineNbr, Equal<Current<RQRequisitionLine.lineNbr>>,
                            And<RQBidding.vendorID, Equal<Current<RQRequisition.vendorID>>,
                            And<RQBidding.vendorLocationID, Equal<Current<RQRequisition.vendorLocationID>>>>>>>
                      .SelectSingleBound(this, new object[] { rqLine, requisition });
							}
						}

		/// <summary>
		/// Fill SO line during QT Order creation.
		/// </summary>
		/// <param name="sograph">The SO graph.</param>
		/// <param name="newSoLine">The new SO line.</param>
		/// <param name="requisition">The requisition.</param>
		/// <param name="rqLine">The requisition line.</param>
		/// <param name="bidding">The bidding.</param>
		/// <param name="profitMultiplier">The profit multiplier.</param>
		protected virtual void FillSOLine(SOOrderEntry sograph, SOLine newSoLine, RQRequisition requisition, 
                                          RQRequisitionLine rqLine, RQBidding bidding, decimal profitMultiplier)
					{
            newSoLine.ManualPrice = true;

            string curyID = requisition.CuryID;
            decimal unitPrice = rqLine.EstUnitCost ?? 0m;
            decimal curyUnitPrice = rqLine.CuryEstUnitCost ?? 0m;

            if (bidding != null && bidding.MinQty <= newSoLine.OrderQty && bidding.OrderQty >= newSoLine.OrderQty)
						{
                curyID = (string)Bidding.GetValueExt<RQBidding.curyID>(bidding);
                unitPrice = bidding.QuoteUnitCost ?? 0m;
                curyUnitPrice = bidding.CuryQuoteUnitCost ?? 0m;
						}

            if (curyID == sograph.Document.Current.CuryID)
						{
                newSoLine.CuryUnitPrice = curyUnitPrice * profitMultiplier;
				}
				else
				{
                newSoLine.UnitPrice = unitPrice * profitMultiplier;
                PXCurrencyAttribute.CuryConvCury<SOLine.curyUnitPrice>(sograph.Transactions.Cache, newSoLine);
			}
		}

		public PXAction<RQRequisition> ViewLineDetails;

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		[PXUIField(DisplayName = Messages.PurchaseDetails)]
		public virtual IEnumerable viewLineDetails(PXAdapter adapter)
		{
			if (Document.Current != null &&
					Document.Current.Released == true &&
					Lines.Current != null)
			{
				this.Filter.Current.ReqNbr = Lines.Current.ReqNbr;
				this.Filter.Current.LineNbr = Lines.Current.LineNbr;
				OrderLines.AskExt();
				OrderLines.ClearDialog();
			}

			return adapter.Get();
		}

		public PXAction<RQRequisition> ChooseVendor;

		[PXButton]
		[PXUIField(DisplayName = Messages.ChooseVendor)]
		public virtual IEnumerable chooseVendor(PXAdapter adapter)
		{
			foreach (RQRequisition item in adapter.Get<RQRequisition>())
			{
				if (this.Vendors.Current != null &&
					(item.Status == RQRequisitionStatus.Bidding || item.Status == RQRequisitionStatus.Hold))
				{
                    bool errors = false;
                    foreach (RQRequisitionLine line in this.Lines.View.SelectMultiBound(new object[] { item }))
                    {
                        RQRequisitionLine l = (RQRequisitionLine)this.Lines.Cache.CreateCopy(line);

                        RQBidding bidding = PXSelect<RQBidding,
                                Where<RQBidding.reqNbr, Equal<Current<RQRequisitionLine.reqNbr>>,
                                    And<RQBidding.lineNbr, Equal<Current<RQRequisitionLine.lineNbr>>,
                                    And<RQBidding.vendorID, Equal<Current<RQBiddingVendor.vendorID>>,
                                    And<RQBidding.vendorLocationID, Equal<Current<RQBiddingVendor.vendorLocationID>>>>>>>
                                .SelectSingleBound(this, new object[] { l, vendor });

                        if (bidding != null && (bidding.MinQty != 0 || bidding.QuoteQty != 0))
                        {

                            if (bidding.MinQty > line.OrderQty)
                            {
                                this.Lines.Cache.RaiseExceptionHandling<RQRequisitionLine.orderQty>(line, line.OrderQty, new PXException(Messages.OrderQtyLessMinQty));
                                errors = true;
                            }

                            if (bidding.QuoteQty < line.OrderQty)
                            {
                                this.Lines.Cache.RaiseExceptionHandling<RQRequisitionLine.orderQty>(line, line.OrderQty, new PXException(Messages.OrderQtyMoreQuoteQty));
                                errors = true;
                            }
                        }
                    }
                    if (errors)
                        throw new PXException(Messages.ChooseVendorError);
                    
					foreach (RQBidding bid in PXSelect<RQBidding,
					Where<RQBidding.reqNbr, Equal<Required<RQBidding.reqNbr>>,
						And<RQBidding.orderQty, Greater<Required<RQBidding.orderQty>>>>>.Select(this, item.ReqNbr, 0m))
					{
						RQBidding b = (RQBidding)this.Bidding.Cache.CreateCopy(bid);
						b.OrderQty = 0;
						this.Bidding.Update(b);
					}

					RQRequisition upd = (RQRequisition)this.Document.Cache.CreateCopy(item);
					upd.VendorID = this.Vendors.Current.VendorID;
					upd.VendorLocationID = this.Vendors.Current.VendorLocationID;
					upd.RemitAddressID = this.Vendors.Current.RemitAddressID;
					upd.RemitContactID = this.Vendors.Current.RemitContactID;
					upd = this.Document.Update(upd);
					upd = (RQRequisition)Document.Search<RQRequisition.reqNbr>(upd.ReqNbr);
					yield return upd;
				}
				else
					yield return item;
			}
		}

		public PXAction<RQRequisition> ResponseVendor;

		[PXButton]
		[PXUIField(DisplayName = Messages.VendorResponse)]
		public virtual IEnumerable responseVendor(PXAdapter adapter)
		{
			foreach (RQRequisition item in adapter.Get<RQRequisition>())
			{
				if (this.Vendors.Current != null)
				{
					RQBiddingEntry graph = PXGraph.CreateInstance<RQBiddingEntry>();
					graph.Vendor.Current =
						graph.Vendor.Search<RQBiddingVendor.reqNbr,
							RQBiddingVendor.lineID>(this.Vendors.Current.ReqNbr, this.Vendors.Current.LineID);
					if (graph.Vendor.Current != null)
						throw new PXRedirectRequiredException(graph, "Vendor Response");
				}
				yield return item;
			}
		}


		public PXAction<RQRequisition> VendorInfo;
		[PXButton]
		[PXUIField(DisplayName = Messages.VendorInfo)]
		public virtual IEnumerable vendorInfo(PXAdapter adapter)
		{
			foreach (RQRequisition item in adapter.Get<RQRequisition>())
			{
				if (this.Vendors.Current != null)
					this.Vendors.AskExt();

				yield return item;
			}
		}

		[Obsolete(Common.Messages.MethodIsObsoleteAndWillBeRemoved2020R2)]
		protected virtual POLine InsertPOLine(POOrderEntry graph, RQRequisitionLine line, decimal? qty, decimal? unitCost,
			IBqlTable costOriginDac, string lineType)
				=> InsertPOLine(graph, line, qty, unitCost, costOriginDac, lineType, null);

		protected virtual POLine InsertPOLine(POOrderEntry graph, RQRequisitionLine line, decimal? qty, decimal? unitCost,
			IBqlTable costOriginDac, string lineType, DateTime? bidPromisedDate)
		{
			if (qty <= 0)
                return null;

			POLine ooline = (POLine)graph.Transactions.Cache.CreateInstance();

			ooline.OrderType = graph.Document.Current.OrderType;
			ooline.OrderNbr = graph.Document.Current.OrderNbr;

			ooline = PXCache<POLine>.CreateCopy(graph.Transactions.Insert(ooline));

			ooline.LineType = lineType;			
			ooline.InventoryID = line.InventoryID;

			if (ooline.InventoryID != null)
				ooline.SubItemID = line.SubItemID;

			ooline.TranDesc = line.Description;
			ooline.UOM = line.UOM;
			ooline.AlternateID = line.AlternateID;
			ooline = graph.Transactions.Update(ooline);

			if (line.SiteID != null)
				graph.Transactions.Cache.RaiseExceptionHandling<POLine.siteID>(ooline, null, null);

			ooline = PXCache<POLine>.CreateCopy(ooline);

            FillPOLineFromRequisitionLine(ooline, graph, line, qty, unitCost, costOriginDac, lineType);

			if (bidPromisedDate != null)
				ooline.PromisedDate = bidPromisedDate;

			ooline = graph.Transactions.Update(ooline);
			//Force Non-Project code since RQ has no support for Project yet.
			ooline.ProjectID = PM.ProjectDefaultAttribute.NonProject();
			PXUIFieldAttribute.SetError<POLine.subItemID>(graph.Transactions.Cache, ooline, null);
			PXUIFieldAttribute.SetError<POLine.expenseSubID>(graph.Transactions.Cache, ooline, null);
			return ooline;
		}

        /// <summary>
        /// Fill PO line from requisition line. This method is an extension point used by the PriceUnit Lexware customization.
        /// </summary>
        /// <param name="poline">The PO line to fill.</param>
        /// <param name="graph">The <see cref="POOrderEntry"/> graph.</param>
        /// <param name="rqLine">The request line.</param>
        /// <param name="qty">The quantity.</param>
        /// <param name="unitCost">The unit cost.</param>
        /// <param name="costOriginDac">The DAC from which the cost was taken.</param>
        /// <param name="lineType">Type of the line.</param>
        protected virtual void FillPOLineFromRequisitionLine(POLine poline, POOrderEntry graph, RQRequisitionLine rqLine, decimal? qty,
                                                             decimal? unitCost, IBqlTable costOriginDac, string lineType)
			{
            if (rqLine.SiteID != null)
            {
                graph.Transactions.Cache.RaiseExceptionHandling<POLine.siteID>(poline, null, null);
                poline.SiteID = rqLine.SiteID;
			}

            poline.OrderQty = qty;

			if (unitCost != null)
                poline.CuryUnitCost = unitCost;

            poline.RcptQtyAction = rqLine.RcptQtyAction;
            poline.RcptQtyMin = rqLine.RcptQtyMin;
            poline.RcptQtyMax = rqLine.RcptQtyMax;
            poline.RcptQtyThreshold = rqLine.RcptQtyThreshold;
            poline.RQReqNbr = rqLine.ReqNbr;
            poline.RQReqLineNbr = rqLine.LineNbr.GetValueOrDefault();
            poline.ManualPrice = true;

			if (lineType != POLineType.GoodsForInventory)
			{
                if (rqLine.ExpenseAcctID != null)
                    poline.ExpenseAcctID = rqLine.ExpenseAcctID;

                if (rqLine.ExpenseAcctID != null && rqLine.ExpenseSubID != null)
                    poline.ExpenseSubID = rqLine.ExpenseSubID;

                poline.ProjectID = PM.ProjectDefaultAttribute.NonProject();
			}

            if (rqLine.PromisedDate != null)
                poline.PromisedDate = rqLine.PromisedDate;

            if (rqLine.RequestedDate != null)
                poline.RequestedDate = rqLine.RequestedDate;

            PXNoteAttribute.CopyNoteAndFiles(Lines.Cache, rqLine, graph.Transactions.Cache, poline);
		}

		private void PersistOrder(POOrderEntry graph)
		{
			POOrder upd = (POOrder)graph.Document.Cache.CreateCopy(graph.Document.Current);

			if(Setup.Current.POHold != true)
				upd.Hold = false;			
            
			upd.CuryControlTotal = upd.CuryOrderTotal;
			graph.Document.Update(upd);

			PXAdapter adapter = new PXAdapter(graph.Document);
			adapter.StartRow = 0;
			adapter.MaximumRows = 1;
			adapter.Searches = new object[] { upd.OrderType, upd.OrderNbr };
			adapter.SortColumns = new string[] { typeof(POOrder.orderType).Name, typeof(POOrder.orderNbr).Name };

			foreach (object item in graph.hold.Press(adapter)) ;    //NOTE Looks like a bug
			
			graph.Save.Press();

			RQRequisitionOrder link = new RQRequisitionOrder();
			link.OrderCategory = RQOrderCategory.PO;
			link.OrderType = graph.Document.Current.OrderType;
			link.OrderNbr = graph.Document.Current.OrderNbr;
			this.ReqOrders.Insert(link);
		}


		public PXAction<RQRequisition> hold;
		[PXUIField(Visible = false)]
		[PXButton]
		protected virtual IEnumerable Hold(PXAdapter adapter)
		{
			foreach (RQRequisition order in adapter.Get<RQRequisition>())
			{
				this.Document.Current = order;

				if (order.Hold == true)
				{
					yield return order;
				}
				else
				{
					if (order.Hold != true && order.Approved != true)
					{
						if (Setup.Current.RequisitionAssignmentMapID != null)
						{
							var processor = new EPAssignmentProcessor<RQRequisition>();
							processor.Assign(order, Setup.Current.RequisitionAssignmentMapID);
                            order.WorkgroupID = order.ApprovalWorkgroupID;
                            order.OwnerID = order.ApprovalOwnerID;
						}
					}

					yield return (RQRequisition)Document.Search<RQRequisition.reqNbr>(order.ReqNbr);
				}

			}
		}

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
                            ((PXAction<RQRequisition>)this.Actions["Hold"]).PressImpl(false);
                        }
                    }
                }
            }
            return base.ExecuteUpdate(viewName, keys, values, parameters);
        }

                public PXAction<RQRequisition> action;

		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable Action(PXAdapter adapter,
		[PXInt]
		[PXIntList(new int[] { 1, 2 }, new string[] { "Persist", "Update" })]
		int? actionID,
		[PXBool]
		bool refresh,
		[PXString]
		                                     string actionName)
		{
			List<RQRequisition> result = new List<RQRequisition>();

			if (actionName != null)
			{
				PXAction a = this.Actions[actionName];

				if (a != null)
					result.AddRange(a.Press(adapter).Cast<RQRequisition>());
			}
			else
				result.AddRange(adapter.Get<RQRequisition>());

			if (refresh)
			{
				foreach (RQRequisition order in result)									
                {
					Document.Search<RQRequisition.reqNbr>(order.ReqNbr);				
			}
			}

			switch (actionID)
			{
				case 1:
					Save.Press();
					break;
				case 2:
					break;
			}

			return result;
		}

		public PXAction<RQRequisition> vendorNotifications;
		[PXUIField(DisplayName = "Vendor Notifications"/*, Visible = false*/)]
		[PXButton()]
		protected virtual IEnumerable VendorNotifications(PXAdapter adapter,
			[PXString] string notificationCD,
			[PXBool] bool currentVendor,
			                                              [PXBool] bool updateVendorStatus)
		{
			foreach (RQRequisition order in adapter.Get<RQRequisition>())
			{
				Document.Cache.Current = order;

				PXResultset<RQBiddingVendor> source = null;

				if (currentVendor)
				{
					source = new PXResultset<RQBiddingVendor>();
					source.Add(new PXResult<RQBiddingVendor>(this.Vendors.Current));
				}

				bool error = false;

				foreach (RQBiddingVendor vndr in source ?? Vendors.Select(order.ReqNbr))
				{
					this.Vendors.Current = vndr;
						var parameters = new Dictionary<string, string>();										

						if (currentVendor == true || vndr.Status != true)
						{
							bool updateStatus = updateVendorStatus; 

							try
							{
								string vendorCD = (string)this.Vendors.GetValueExt<RQBiddingVendor.vendorID>(vndr);
								parameters["ReqNbr"] = Document.Current.ReqNbr;
								parameters["VendorID"] = vendorCD;
								BidActivity.SendNotification(APNotificationSource.Vendor, notificationCD, order.BranchID, parameters);
							}
							catch (Exception e)
							{
                            if (currentVendor)
                                throw;

								PXTrace.WriteError(e);
								error = true;
								updateStatus = false;
								Vendors.Cache.RaiseExceptionHandling<RQBiddingVendor.status>(vndr, false, e);
							}

							if (updateStatus)
							{
								RQBiddingVendor upd = PXCache<RQBiddingVendor>.CreateCopy(vndr);
								upd.Status = true;
								this.Vendors.Update(upd);
								Save.Press();
							}
						}
					}

                if (error)
                    throw new PXException(ErrorMessages.SeveralItemsFailed);

				yield return order;
			}			
		}

		public PXAction<RQRequisition> report;

		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Report(PXAdapter adapter,
			                                 [PXString(8, InputMask = "CC.CC.CC.CC")] string reportID)
		{
            List<RQRequisition> list = adapter.Get<RQRequisition>().ToList();

			if (!String.IsNullOrEmpty(reportID))
			{
				Save.Press();				
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["ReqNbr"] = Document.Current.ReqNbr;
				throw new PXReportRequiredException(parameters, reportID, "Report " + reportID);				
			}

			return list;
		}
		

		#region CurrencyInfo events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurrencyInfo row = (CurrencyInfo)e.Row;
				RQRequisition doc = (RQRequisition)this.Document.Current;
				if (row != null && doc != null && row.CuryInfoID == doc.CuryInfoID)
				{
					if (customer.Current != null && !string.IsNullOrEmpty(customer.Current.CuryID))
					{
						e.NewValue = customer.Current.CuryID;
						e.Cancel = true;
					}
					else if (vendor.Current != null && !string.IsNullOrEmpty(vendor.Current.CuryID))
					{
						e.NewValue = vendor.Current.CuryID;
						e.Cancel = true;
					}
				}				
				else if (vendorBidder.Current != null && !string.IsNullOrEmpty(vendorBidder.Current.CuryID))
				{
					e.NewValue = vendorBidder.Current.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurrencyInfo row = (CurrencyInfo)e.Row;
				RQRequisition doc = (RQRequisition)this.Document.Current;
				if (row != null && doc != null && row.CuryInfoID == doc.CuryInfoID)
				{
					if (customer.Current != null && !string.IsNullOrEmpty(customer.Current.CuryID))
					{
						e.NewValue = customer.Current.CuryRateTypeID ?? cmsetup.Current.ARRateTypeDflt;
						e.Cancel = true;
					}
					else if (vendor.Current != null && !string.IsNullOrEmpty(vendor.Current.CuryID))
					{
						e.NewValue = vendor.Current.CuryRateTypeID ?? cmsetup.Current.APRateTypeDflt;
						e.Cancel = true;
					}
				}			
				else if (vendorBidder.Current != null)
				{
					e.NewValue = vendorBidder.Current.CuryRateTypeID ?? cmsetup.Current.APRateTypeDflt;
					e.Cancel = true;
				}
				else
				{
					e.NewValue = cmsetup.Current.APRateTypeDflt;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((RQRequisition)Document.Cache.Current).OrderDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.IsReadOnly != true && this.Lines.Cache.AllowInsert && this.Lines.Cache.AllowDelete;
				bool curyrateenabled = info.AllowUpdate(this.Lines.Cache);
				
				RQRequisition doc = (RQRequisition)this.Document.Current;
				if (doc != null && info.CuryInfoID == doc.CuryInfoID)
				{
					if (customer.Current != null)
					{
						if (customer.Current.AllowOverrideCury != true)
							curyenabled = false;

						if (customer.Current.AllowOverrideRate != true)
							curyrateenabled = false;
					}
					else if (vendor.Current != null)
					{
						if (vendor.Current.AllowOverrideCury != true)
							curyenabled = false;

						if (vendor.Current.AllowOverrideRate != true)
							curyrateenabled = false;
					}
				}
				else if (doc != null)
				{
					PXResult<RQBiddingVendor, Vendor> v =
						(PXResult<RQBiddingVendor, Vendor>)
						PXSelectJoin<RQBiddingVendor,
						InnerJoin<Vendor, On<RQBiddingVendor.vendorID, Equal<Vendor.bAccountID>>>,
						Where<RQBiddingVendor.curyInfoID, Equal<Required<RQBiddingVendor.curyInfoID>>>>
						.SelectWindowed(this, 0, 1, info.CuryInfoID);

					Vendor vendor = v != null ? (Vendor)v : null;

					curyenabled = (vendor != null) ? vendor.AllowOverrideCury == true : false;
					curyrateenabled = (vendor != null) ? vendor.AllowOverrideRate == true : false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyrateenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyrateenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyrateenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyrateenabled);
			}
		}
		#endregion

		protected virtual void RQRequisition_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;

			if (row == null)
                return;

			bool rHold = row.Hold == true;

			Transfer.SetEnabled(rHold);
			Merge.SetEnabled(rHold);
			ViewLineDetails.SetEnabled(row.Status == RQRequisitionStatus.Released);
			AddRequestLine.SetEnabled(rHold);
			AddRequestContent.SetEnabled(rHold);
			CMSetup setup = cmsetup.Current;
			PXUIFieldAttribute.SetVisible<RQRequest.curyID>(sender, row, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
			bool noLines = this.Lines.Select().Count == 0;

			bool enableCurrenty = false;

			if (noLines)
            {
				if (customer.Current != null)
					enableCurrenty = customer.Current.AllowOverrideCury == true;
				else if (this.vendor.Current != null)
					enableCurrenty = this.vendor.Current.AllowOverrideCury == true;
				else
					enableCurrenty = true;
            }

			PXUIFieldAttribute.SetEnabled<RQRequisition.customerID>(sender, row, noLines);
			PXUIFieldAttribute.SetEnabled<RQRequisition.customerLocationID>(sender, row, noLines);
			PXUIFieldAttribute.SetEnabled<RQRequisition.curyID>(sender, row, enableCurrenty);
			PXUIFieldAttribute.SetVisible<RQRequisition.quoted>(sender, row, row.CustomerLocationID != null);
			this.addInvBySite.SetEnabled(rHold && !(bool)row.Released);
			POShipAddress shipAddress = this.Shipping_Address.Select();
			PORemitAddress remitAddress = this.Remit_Address.Select();

			bool enableAddressValidation = (row.Released== false && row.Cancelled == false)
				&& ((shipAddress != null && shipAddress.IsDefaultAddress == false && shipAddress.IsValidated == false)
				|| (remitAddress != null && remitAddress.IsDefaultAddress == false && remitAddress.IsValidated == false));

			this.validateAddresses.SetEnabled(enableAddressValidation);
            Vendors.Cache.AllowUpdate = Vendors.Cache.AllowInsert = Vendors.Cache.AllowDelete = row.Status != RQRequisitionStatus.Released;

			PXUIFieldAttribute.SetEnabled<RQRequisition.shipToBAccountID>(sender, row,
				(row.ShipDestType != POShippingDestination.CompanyLocation && row.ShipDestType != POShippingDestination.Site));
			PXUIFieldAttribute.SetEnabled<RQRequisition.shipToLocationID>(sender, row,
				(row.ShipDestType != POShippingDestination.Site));
			PXUIFieldAttribute.SetEnabled<RQRequisition.siteID>(sender, row, (row.ShipDestType == POShippingDestination.Site));

			PXUIFieldAttribute.SetRequired<RQRequisition.siteID>(sender, true);
			PXUIFieldAttribute.SetRequired<RQRequisition.shipToBAccountID>(sender, (row.ShipDestType != POShippingDestination.Site));
			PXUIFieldAttribute.SetRequired<RQRequisition.shipToLocationID>(sender, (row.ShipDestType != POShippingDestination.Site));

			PXUIFieldAttribute.SetVisible<RQRequisition.shipToBAccountID>(sender, row, (row.ShipDestType != POShippingDestination.Site));
			PXUIFieldAttribute.SetVisible<RQRequisition.shipToLocationID>(sender, row, (row.ShipDestType != POShippingDestination.Site));
			PXUIFieldAttribute.SetVisible<RQRequisition.siteID>(sender, row, (row.ShipDestType == POShippingDestination.Site));

			if (row != null && row.ShipDestType == POShippingDestination.Site)
			{
				var siteIdErrorString = PXUIFieldAttribute.GetError<RQRequisition.siteID>(sender, e.Row);
				if (siteIdErrorString == null)
				{
					var siteIdErrorMessage = row.SiteIdErrorMessage;
					if (!string.IsNullOrWhiteSpace(siteIdErrorMessage))
					{
						sender.RaiseExceptionHandling<RQRequisition.siteID>(e.Row, sender.GetValueExt<RQRequisition.siteID>(e.Row),
							new PXSetPropertyException(siteIdErrorMessage, PXErrorLevel.Error));
					}
				}
			}
		}

		protected virtual void RQRequisition_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row != null)
			{
				using (ReadOnlyScope rs = new ReadOnlyScope(Shipping_Address.Cache, Shipping_Contact.Cache))
				{
					POShipAddressAttribute.DefaultRecord<RQRequisition.shipAddressID>(sender, e.Row);
					POShipContactAttribute.DefaultRecord<RQRequisition.shipContactID>(sender, e.Row);
				}
			}
		}

		protected virtual void RQRequisition_ShipDestType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;
			if (row == null) return;

			if (row.ShipDestType == POShippingDestination.Site)
			{
				sender.SetDefaultExt<RQRequisition.siteID>(e.Row);
				sender.SetValueExt<RQRequisition.shipToBAccountID>(e.Row, null);
				sender.SetValueExt<RQRequisition.shipToLocationID>(e.Row, null);
			}
			else
			{
				sender.SetValueExt<RQRequisition.siteID>(e.Row, null);
				sender.SetDefaultExt<RQRequisition.shipToBAccountID>(e.Row);
				sender.SetDefaultExt<RQRequisition.shipToLocationID>(e.Row);
			}
		}

		protected virtual void RQRequisition_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;

			if (row != null && row.ShipDestType == POShippingDestination.Site)
			{
				string siteIdErrorMessage = string.Empty;

				try
				{
					POShipAddressAttribute.DefaultRecord<RQRequisition.shipAddressID>(sender, e.Row);
				}
				catch (SharedRecordMissingException)
				{
					sender.RaiseExceptionHandling<RQRequisition.siteID>(e.Row, sender.GetValueExt<RQRequisition.siteID>(e.Row),
						new PXSetPropertyException(PO.Messages.ShippingAddressMayNotBeEmpty, PXErrorLevel.Error));
					sender.SetValueExt<RQRequisition.shipAddressID>(e.Row, null);
					siteIdErrorMessage = PO.Messages.ShippingAddressMayNotBeEmpty;
				}
				try
				{
					POShipContactAttribute.DefaultRecord<RQRequisition.shipContactID>(sender, e.Row);
				}
				catch (SharedRecordMissingException)
				{
					sender.RaiseExceptionHandling<RQRequisition.siteID>(e.Row, sender.GetValueExt<RQRequisition.siteID>(e.Row),
						new PXSetPropertyException(PO.Messages.ShippingContactMayNotBeEmpty, PXErrorLevel.Error));
					sender.SetValueExt<RQRequisition.shipContactID>(e.Row, null);
					siteIdErrorMessage = PO.Messages.ShippingContactMayNotBeEmpty;
				}

				sender.SetValueExt<RQRequest.siteIdErrorMessage>(e.Row, siteIdErrorMessage);

				if (string.IsNullOrWhiteSpace(siteIdErrorMessage))
					PXUIFieldAttribute.SetError<RQRequisition.siteID>(sender, e.Row, null);
			}
		}

		protected virtual void RQRequisition_ShipToBAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;

			if (row != null)
			{
				sender.SetDefaultExt<RQRequisition.shipToLocationID>(e.Row);
			}
		}

		protected virtual void RQRequisition_ShipToLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;

			if (row != null)
			{
				try
				{
					POShipAddressAttribute.DefaultRecord<RQRequisition.shipAddressID>(sender, e.Row);
				}
				catch (SharedRecordMissingException)
				{
					sender.RaiseExceptionHandling<RQRequisition.siteID>(e.Row, sender.GetValueExt<RQRequisition.siteID>(e.Row),
						new PXSetPropertyException(PO.Messages.ShippingAddressMayNotBeEmpty, PXErrorLevel.Error));
				}
				try
				{
					POShipContactAttribute.DefaultRecord<RQRequest.shipContactID>(sender, e.Row);
				}
				catch (SharedRecordMissingException)
				{
					sender.RaiseExceptionHandling<RQRequisition.siteID>(e.Row, sender.GetValueExt<RQRequisition.siteID>(e.Row),
						new PXSetPropertyException(PO.Messages.ShippingContactMayNotBeEmpty, PXErrorLevel.Error));
				}
			}
		}

		protected virtual void RQRequisition_ShipToLocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;
			if (row != null && row.ShipDestType == POShippingDestination.Site)
			{
				e.Cancel = true;
				e.NewValue = null;
			}
		}

		protected virtual void RQRequisition_ShipToBAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;
			if (row == null) return;

			if (row.ShipDestType == POShippingDestination.Site)
			{
				e.Cancel = true;
				e.NewValue = null;
			}
		}

		protected virtual void RQRequisition_ShipToLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;
			if (row != null && (row.ShipDestType == POShippingDestination.CompanyLocation)
				&& (row.VendorID != null && row.VendorLocationID != null))
			{
				Location vendorLocation = PXSelectReadonly<Location,
												Where<Location.bAccountID, Equal<Required<RQRequisition.vendorID>>,
												And<Location.locationID, Equal<Required<RQRequisition.vendorLocationID>>>>>.Select(this, row.VendorID, row.VendorLocationID);
				if (vendorLocation != null && vendorLocation.VBranchID != null)
				{
					e.NewValue = vendorLocation.VBranchID;
					e.Cancel = true;
				}
			}
		}
		protected virtual void RQRequisition_BiddingComplete_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;
			e.NewValue = row.VendorLocationID != null;
		}

		protected virtual void RQRequisition_Quoted_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;
			e.NewValue = row.CustomerLocationID == null;
		}

		protected virtual void RQRequisition_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			this.customer.Current = null;
			sender.SetDefaultExt<RQRequisition.customerLocationID>(e.Row);
			sender.SetDefaultExt<RQRequisition.curyID>(e.Row);

			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.ExternalCall || sender.GetValuePending<RQRequisition.curyID>(e.Row) == null)
			{
                    if (sender.GetValuePending<RQRequisition.vendorID>(e.Row) != null)
                        return;

				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<RQRequisition.curyInfoID>(sender, e.Row);
				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);

				if (string.IsNullOrEmpty(message) == false)
				{
					sender.RaiseExceptionHandling<RQRequisition.orderDate>(e.Row, ((SOOrder)e.Row).OrderDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
				}

				if (info != null)
				{
					((RQRequisition)e.Row).CuryID = info.CuryID;
				}
			}
		}
		}

		protected virtual void RQRequisition_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;
			RQRequisition newRow = (RQRequisition)e.NewRow;

			if (row != null &&
				 (row.CustomerID != newRow.CustomerID || row.CustomerLocationID != newRow.CustomerLocationID))
			{
				RQRequisitionContent cont =
					PXSelectJoin<RQRequisitionContent,
						InnerJoin<RQRequest,
									 On<RQRequest.orderNbr, Equal<RQRequisitionContent.orderNbr>>>,
					Where<RQRequisitionContent.reqNbr, Equal<Required<RQRequisitionContent.reqNbr>>,
						And<Where<RQRequest.employeeID, NotEqual<Required<RQRequest.employeeID>>,
									 Or<RQRequest.locationID, NotEqual<Required<RQRequest.locationID>>>>>>>.SelectWindowed(this, 0, 1,
									 row.ReqNbr, newRow.CustomerID, newRow.CustomerLocationID);
				if (cont != null &&
					this.Contents.Ask(Messages.AskConfirmation,
					Messages.CustomerUpdateConfirmation,
					MessageButtons.YesNo) == WebDialogResult.No)
				{
					newRow.CustomerID = row.CustomerID;
					newRow.CustomerLocationID = row.CustomerLocationID;
				}
			}
		}

		protected virtual void RQRequisition_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.OldRow;
			RQRequisition newRow = (RQRequisition)e.Row;

			if (newRow.CustomerID == null && newRow.POType == POOrderType.DropShip)
				newRow.POType = POOrderType.RegularOrder;

			if (row.POType != newRow.POType &&
				newRow.POType == POOrderType.DropShip &&
				newRow.ShipDestType != POShippingDestination.Customer)
			{
				RQRequisition upd = PXCache<RQRequisition>.CreateCopy(newRow);
				upd.ShipDestType = POShippingDestination.Customer;
				upd.ShipToBAccountID = row.CustomerID;
				upd.ShipToLocationID = row.CustomerLocationID;
				this.Document.Update(upd);
			}
			else if (row.POType != newRow.POType &&
				newRow.POType != POOrderType.DropShip &&
				newRow.ShipDestType == POShippingDestination.Customer)
			{
				RQRequisition upd = PXCache<RQRequisition>.CreateCopy(newRow);
				upd.ShipDestType = POShippingDestination.CompanyLocation;
				sender.SetDefaultExt<RQRequisition.shipDestType>(upd);
				this.Document.Update(upd);
			}

			if (row.CustomerID == null &&
					row.VendorID != newRow.VendorID && PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				Vendor rVendor = vendor.Current;

				if (rVendor == null || rVendor.BAccountID != ((RQRequisition)e.Row).VendorID)
				{
					vendor.RaiseFieldUpdated(sender, e.Row);
					rVendor = vendor.Current;
				}

				if (rVendor != null)
				{
					CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy(currencyinfo.Select(row.CuryInfoID));

					RQBiddingVendor bidder =
						PXSelect<RQBiddingVendor,
							Where<RQBiddingVendor.reqNbr, Equal<Required<RQBiddingVendor.reqNbr>>,
								And<RQBiddingVendor.vendorID, Equal<Required<RQBiddingVendor.vendorID>>,
									And<RQBiddingVendor.vendorLocationID, Equal<Required<RQBiddingVendor.vendorLocationID>>>>>>
							.SelectWindowed(this, 0, 1,
															newRow.ReqNbr,
															newRow.VendorID,
															newRow.VendorLocationID);

					CurrencyInfo vendorInfo = bidder != null ? currencyinfo.Select(bidder.CuryInfoID) : null;
					bool externalCall = e.ExternalCall;

					if (vendorInfo == null)
					{
						vendorInfo = new CurrencyInfo();
						vendorInfo.CuryID = vendor.Current.CuryID ?? company.Current.BaseCuryID;
						vendorInfo.CuryRateTypeID = vendor.Current.CuryRateTypeID ?? cmsetup.Current.APRateTypeDflt;
						externalCall = true;
					}

					bool update = false;

					if (vendorInfo.CuryID != info.CuryID &&
							(vendor.Current.AllowOverrideCury != true || externalCall != true))
					{
						info.CuryID = vendorInfo.CuryID;
						update = true;
					}

					if (vendorInfo.CuryRateTypeID != info.CuryRateTypeID &&
							(vendor.Current.AllowOverrideRate != true || externalCall != true))
					{
						info.CuryRateTypeID = vendorInfo.CuryRateTypeID;
						update = true;
					}

					if (update)
					{
						try
						{
							PXDBCurrencyAttribute.SetBaseCalc<RQRequisitionLine.curyEstUnitCost>(this.Lines.Cache, null, false);
							currencyinfo.Update(info);
						}
						finally
						{
							PXDBCurrencyAttribute.SetBaseCalc<RQRequisitionLine.curyEstUnitCost>(this.Lines.Cache, null, true);
						}

						string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);

						if (string.IsNullOrEmpty(message) == false)
						{
							sender.RaiseExceptionHandling<RQRequisition.orderDate>(e.Row, ((RQRequisition)e.Row).OrderDate,
																																		 new PXSetPropertyException(message, PXErrorLevel.Warning));
						}

						newRow.CuryID = info.CuryID;

						foreach (RQRequisitionLine line in this.Lines.View.SelectMultiBound(new object[] { e.Row }))
						{
							RQBidding bidding = PXSelect<RQBidding,
							Where<RQBidding.reqNbr, Equal<Current<RQRequisitionLine.reqNbr>>,
								And<RQBidding.lineNbr, Equal<Current<RQRequisitionLine.lineNbr>>,
								And<RQBidding.vendorID, Equal<Current<RQBiddingVendor.vendorID>>,
								And<RQBidding.vendorLocationID, Equal<Current<RQBiddingVendor.vendorLocationID>>>>>>>
							.SelectSingleBound(this, new object[] { line, vendor });

							if (bidding != null)
								CopyUnitCost(line, bidding);
							else
							{
								RQRequisitionLine upd = (RQRequisitionLine)this.Lines.Cache.CreateCopy(line);
								decimal unitCost;
                                PXCurrencyAttribute.CuryConvCury<RQRequisitionLine.curyInfoID>(this.Lines.Cache, upd, upd.EstUnitCost ?? 0, out unitCost, true);
								upd.CuryEstUnitCost = unitCost;

								this.Lines.Cache.SetDefaultExt<RQRequisitionLine.curyEstUnitCost>(upd);
								this.Lines.Cache.SetDefaultExt<RQRequisitionLine.siteID>(upd);
								this.Lines.Update(upd);
							}
						}
					}
				}
			}

			if (newRow.Hold == true)
				return;

			if (Lines.Select().AsEnumerable().Any(_ => ((RQRequisitionLine)_).InventoryID == null))
			{
				sender.RaiseExceptionHandling<RQRequisition.hold>(newRow, newRow.Hold,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty,
											   PXUIFieldAttribute.GetDisplayName<RQRequisitionLine.inventoryID>(Lines.Cache),
											   PXErrorLevel.Error));
			}
			else
				sender.RaiseExceptionHandling<RQRequisition.hold>(newRow, newRow.Hold, null);
		}

		protected virtual void RQRequisition_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;

			PXDefaultAttribute.SetPersistingCheck<RQRequisition.siteID>(sender, row, (row.ShipDestType == POShippingDestination.Site) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<RQRequisition.shipToLocationID>(sender, row, (row.ShipDestType != POShippingDestination.Site) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<RQRequisition.shipToBAccountID>(sender, row, (row.ShipDestType != POShippingDestination.Site) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}

		/// <summary>
		/// Copies the unit cost from <see cref="RQBidding"/> to <see cref="RQRequisitionLine"/> on RQRequisition row updated event. This is an extension point used by Lexware PriceUnit customization.
		/// </summary>
		/// <param name="line">The line.</param>
		/// <param name="bidding">The bidding.</param>
		public virtual void CopyUnitCost(RQRequisitionLine line, RQBidding bidding)
        {
            if ((bidding.MinQty == 0 && bidding.QuoteQty == 0) ||
                 (bidding.MinQty <= line.OrderQty && bidding.QuoteQty >= line.OrderQty))
            {
                RQRequisitionLine copy = (RQRequisitionLine) Lines.Cache.CreateCopy(line);
                decimal unitCost;

                PXCurrencyAttribute.CuryConvCury<RQRequisitionLine.curyInfoID>(Lines.Cache, copy, bidding.QuoteUnitCost ?? 0, out unitCost, true);
                copy.CuryEstUnitCost = unitCost;

                Lines.Update(copy);
            }
        }

		protected virtual void RQRequisition_CustomerLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;
			row.Quoted = row.CustomerLocationID == null;

			if (row != null && row.Hold == true)
            {
				foreach (RQRequisitionLine line in this.Lines.Select(row.ReqNbr))
				{
					RQRequisitionLine upd = (RQRequisitionLine)this.Lines.Cache.CreateCopy(line);
					this.Lines.Cache.SetDefaultExt<RQRequisitionLine.curyEstUnitCost>(upd);
					this.Lines.Cache.SetDefaultExt<RQRequisitionLine.siteID>(upd);
					this.Lines.Update(upd);
				}
		}
		}

		protected virtual void RQRequisition_POType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;

			if (row.CustomerID == null && (string)e.NewValue == POOrderType.DropShip)
				throw new PXSetPropertyException(Messages.DropShipRequisition);
		}


		protected virtual void RQRequisition_VendorID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;
			Vendor vendor = PXSelect<Vendor, Where<Vendor.acctCD, Equal<Required<Vendor.acctCD>>>>
				.SelectWindowed(this, 0, 1, e.NewValue);

			if (vendor != null && PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurrencyInfo current = currencyinfo.Select(row.CuryInfoID);

				if (vendor.CuryID == null)
                    vendor.CuryID = company.Current.BaseCuryID;

				if (vendor.CuryRateTypeID == null)
                    vendor.CuryRateTypeID = cmsetup.Current.APRateTypeDflt;

				string message = null;

				if (vendor.AllowOverrideCury != true && vendor.CuryID != current.CuryID)
					message = PXMessages.LocalizeFormatNoPrefixNLA(Messages.RequisitionVendorCuryIDValidation, current.CuryID);
				else if (vendor.AllowOverrideRate != true && vendor.CuryRateTypeID != current.CuryRateTypeID)
					message = PXMessages.LocalizeFormatNoPrefixNLA(Messages.RequisitionVendorCuryRateIDValidation, current.CuryRateTypeID);

				if (message != null &&
					this.Document.Ask(row, Messages.Warning, message, MessageButtons.OKCancel) != WebDialogResult.OK)
				{
					e.Cancel = true;
				}
			}
		}

		protected virtual void RQRequisition_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<RQRequisition.vendorLocationID>(e.Row);
			sender.SetDefaultExt<RQRequisition.termsID>(e.Row);
			object VendorRefNbr = ((RQRequisition)e.Row).VendorRefNbr;
			sender.RaiseFieldVerifying<RQRequisition.vendorRefNbr>(e.Row, ref VendorRefNbr);

		}

		protected virtual void RQRequisition_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Location current = (Location)this.location.Current;
			RQRequisition row = (RQRequisition)e.Row;

			if (current == null || (current.BAccountID != row.VendorID || current.LocationID != row.VendorLocationID))
			{
				current = this.location.Select();
				this.location.Current = current;
			}

			sender.SetDefaultExt<RQRequisition.shipVia>(e.Row);
			sender.SetDefaultExt<RQRequisition.fOBPoint>(e.Row);

			if (row.ShipDestType == POShippingDestination.Vendor)
				sender.SetDefaultExt<RQRequisition.shipToLocationID>(e.Row);

			sender.SetDefaultExt<RQRequisition.biddingComplete>(e.Row);

			PORemitAddressAttribute.DefaultRecord<RQRequisition.remitAddressID>(sender, e.Row);
			PORemitContactAttribute.DefaultRecord<RQRequisition.remitContactID>(sender, e.Row);

			foreach (RQRequisitionLine line in this.Lines.Select(row.ReqNbr))
			{
				RQRequisitionLine upd = (RQRequisitionLine)this.Lines.Cache.CreateCopy(line);
				this.Lines.Cache.SetDefaultExt<RQRequisitionLine.curyEstUnitCost>(upd);				
                
				try
				{
					this.Lines.Cache.SetDefaultExt<RQRequisitionLine.siteID>(upd);
					this.Lines.Cache.SetDefaultExt<RQRequisitionLine.rcptQtyAction>(upd);
					this.Lines.Cache.SetDefaultExt<RQRequisitionLine.rcptQtyMin>(upd);
					this.Lines.Cache.SetDefaultExt<RQRequisitionLine.rcptQtyMax>(upd);
					this.Lines.Cache.SetDefaultExt<RQRequisitionLine.rcptQtyThreshold>(upd);
				}
				catch
				{					
				}

				this.Lines.Update(upd);				
			}
		}

		protected virtual void RQRequisition_VendorRequestSent_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequisition doc = (RQRequisition)e.Row;

			if (doc.VendorRequestSent == true)
			{
				foreach (RQBiddingVendor bidder in this.Vendors.Select(doc.ReqNbr))
				{
					RQBiddingVendor upd = PXCache<RQBiddingVendor>.CreateCopy(bidder);
					upd.Status = true;
					this.Vendors.Update(upd);
				}
			}
		}
		protected virtual void RQRequisition_Hold_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			RQRequisition doc = (RQRequisition)e.Row;

			if (doc.Hold == true && doc.Hold != (bool?)e.OldValue)
			{
				cache.SetDefaultExt<RQRequisition.biddingComplete>(e.Row);
				cache.SetDefaultExt<RQRequisition.quoted>(e.Row);

				if (doc.Cancelled == true)
					cache.SetValueExt<RQRequisition.cancelled>(doc, false);

				foreach (RQBiddingVendor bidder in this.Vendors.Select(doc.ReqNbr))
				{
					RQBiddingVendor upd = PXCache<RQBiddingVendor>.CreateCopy(bidder);
					upd.Status = false;
					this.Vendors.Update(upd);
				}
			}
		}

		protected virtual void RQRequisitionLine_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<RQRequisitionLine.uOM>(e.Row);
			sender.SetDefaultExt<RQRequisitionLine.description>(e.Row);
			sender.SetDefaultExt<RQRequisitionLine.subItemID>(e.Row);
			sender.SetDefaultExt<RQRequisitionLine.estUnitCost>(e.Row);
			if (((RQRequisitionLine)e.Row)?.ManualPrice != true)
			sender.SetValue<RQRequisitionLine.curyEstUnitCost>(e.Row, null);
			sender.SetDefaultExt<RQRequisitionLine.curyEstUnitCost>(e.Row);
			sender.SetDefaultExt<RQRequisitionLine.promisedDate>(e.Row);
			sender.SetDefaultExt<RQRequisitionLine.markupPct>(e.Row);
			sender.SetDefaultExt<RQRequisitionLine.rcptQtyAction>(e.Row);
			sender.SetDefaultExt<RQRequisitionLine.rcptQtyMax>(e.Row);
			sender.SetDefaultExt<RQRequisitionLine.rcptQtyMin>(e.Row);			
		}

		protected virtual void RQRequisitionLine_SiteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			RQRequisitionLine line = (RQRequisitionLine)e.Row;
			if (line == null) return;

			if (this.Document.Current != null && this.Document.Current.CustomerLocationID != null)
				return;

			if (!PXAccess.FeatureInstalled<FeaturesSet.inventory>())
			{
				e.NewValue = null;
				e.Cancel = true;
				return;
			}

			int? siteID = null;
			foreach (RQRequisitionContent rq in
				PXSelect<RQRequisitionContent,
				Where<RQRequisitionContent.reqNbr, Equal<Required<RQRequisitionContent.reqNbr>>,
					And<RQRequisitionContent.reqLineNbr, Equal<Required<RQRequisitionContent.reqLineNbr>>>>>
					.Select(this, line.ReqNbr, line.LineNbr))
			{
				PXResult<RQRequest, RQRequestClass, Location> res =
					(PXResult<RQRequest, RQRequestClass, Location>)
					PXSelectJoin<RQRequest,
					InnerJoin<RQRequestClass, On<RQRequestClass.reqClassID, Equal<RQRequest.reqClassID>>,
					InnerJoin<Location, On<Location.bAccountID, Equal<RQRequest.employeeID>, And<Location.locationID, Equal<RQRequest.locationID>>>>>,
					Where<RQRequest.orderNbr, Equal<Required<RQRequest.orderNbr>>>>.SelectWindowed(this, 0, 1, rq.OrderNbr);

				RQRequestClass reqClass = res;
				Location location = res;

				if (location == null || location.LocationCD == null) continue;
				int? locationSiteID = reqClass.CustomerRequest == true ?
					location.CSiteID :
					location.CMPSiteID;

				if (siteID == null)
				{
					siteID = locationSiteID;
				}
				else if (siteID != locationSiteID)
				{
					siteID = null;
					break;
				}
			}
			if (siteID != null)
			{
				e.NewValue = siteID;
				e.Cancel = true;
			}
		}
		protected virtual void RQRequisitionLine_Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			RQRequisitionLine row = (RQRequisitionLine)e.Row;
			if (row == null)
			{
				e.ReturnValue = string.Empty;
				return;
			}

			INSiteStatus availability = PXSelect<INSiteStatus,
				Where<INSiteStatus.inventoryID, Equal<Required<RQRequisitionLine.inventoryID>>,
				And<INSiteStatus.subItemID, Equal<Required<RQRequisitionLine.subItemID>>,
				And<INSiteStatus.siteID, Equal<Required<RQRequisitionLine.siteID>>>>>>.SelectWindowed(this, 0, 1, row.InventoryID, row.SubItemID, row.SiteID);

			if (availability != null)
			{
				availability.QtyOnHand = INUnitAttribute.ConvertFromBase<RQRequisitionLine.inventoryID, RQRequisitionLine.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
				availability.QtyAvail = INUnitAttribute.ConvertFromBase<RQRequisitionLine.inventoryID, RQRequisitionLine.uOM>(sender, e.Row, (decimal)availability.QtyAvail, INPrecision.QUANTITY);
				availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<RQRequisitionLine.inventoryID, RQRequisitionLine.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
				availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<RQRequisitionLine.inventoryID, RQRequisitionLine.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);

				e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(IN.Messages.Availability_Info,
						sender.GetValue<RQRequisitionLine.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));

			}
			else
			{
				e.ReturnValue = string.Empty;
			}
		}

		protected virtual string FormatQty(decimal? value)
		{
			return (value == null) ? string.Empty : ((decimal)value).ToString("N" + CommonSetupDecPl.Qty.ToString(), System.Globalization.NumberFormatInfo.CurrentInfo);
		}

		protected virtual void RQRequisitionLine_SubItemID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (this.Document.Current.Hold == true)
			{
				if (((RQRequisitionLine)e.Row).ManualPrice != true)
				sender.SetValue<RQRequisitionLine.curyEstUnitCost>(e.Row, null);
				sender.SetDefaultExt<RQRequisitionLine.curyEstUnitCost>(e.Row);
			}
		}

		protected virtual void RQRequisitionLine_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<RQRequisitionLine.curyEstUnitCost>(e.Row);
		}

		protected virtual void RQRequisitionLine_CuryEstUnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			RQRequisitionLine row = e.Row as RQRequisitionLine;

			if (row == null)
				return;

				e.NewValue = row.CuryEstUnitCost;
			RQRequisition order = Document.Current;

			RQBidding bidding = 
				PXSelect<RQBidding,
					Where<RQBidding.reqNbr, Equal<Current<RQRequisitionLine.reqNbr>>,
						And<RQBidding.lineNbr, Equal<Current<RQRequisitionLine.lineNbr>>,
						And<RQBidding.vendorID, Equal<Current<RQRequisition.vendorID>>,
						And<RQBidding.vendorLocationID, Equal<Current<RQRequisition.vendorLocationID>>>>>>>
					.SelectSingleBound(this, new object[] { row, order });

				if (bidding != null && bidding.MinQty > row.OrderQty && row.OrderQty < bidding.QuoteQty)
				{
					if ((string)Bidding.GetValueExt<RQBidding.curyID>(bidding) == order.CuryID)
				{
						e.NewValue = bidding.CuryQuoteUnitCost;
				}
					else
					{
					decimal unitCost;
					PXCurrencyAttribute.CuryConvCury<RQRequisitionLine.curyInfoID>(this.Lines.Cache, row, bidding.QuoteUnitCost ?? 0, out unitCost, true);
						e.NewValue = unitCost;
					}
				}
				else if (order != null && row.InventoryID != null && order.Hold == true)
				{
				if (row.ManualPrice == true && row.CuryEstUnitCost != null)
				{
					e.NewValue = row.CuryEstUnitCost;
					return;
				}

					decimal? vendorUnitCost = null;

				if (row.UOM != null)
					{
						DateTime date = Document.Current.OrderDate.Value;
					CurrencyInfo curyInfo = currencyinfo.Search<CurrencyInfo.curyInfoID>(order.CuryInfoID);
					vendorUnitCost = APVendorPriceMaint.CalculateUnitCost(sender, order.VendorID, order.VendorLocationID, row.InventoryID, 
																		  row.SiteID, curyInfo, row.UOM, row.OrderQty, date, row.CuryEstUnitCost);
						e.NewValue = vendorUnitCost;
					}

					if (vendorUnitCost == null)
					{ 
					decimal? newPrice = 
						POItemCostManager.Fetch<RQRequisitionLine.inventoryID, RQRequisitionLine.curyInfoID>(sender.Graph, row, order.VendorID, 
																											 order.VendorLocationID, order.OrderDate,
																											 order.CuryID, row.InventoryID, row.SubItemID, row.SiteID, 
																											 row.UOM, onlyVendor: e.NewValue != null);

					if (newPrice >= 0)
					{
						e.NewValue = newPrice;
				}
				}

					APVendorPriceMaint.CheckNewUnitCost<RQRequisitionLine, RQRequisitionLine.curyEstUnitCost>(sender, row, e.NewValue);
			}
		}

		protected virtual void RQRequisitionLine_Cancelled_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequisitionLine row = (RQRequisitionLine)e.Row;
			row.OrderQty = 0;
		}

		protected virtual void RQRequisitionLine_OrderQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null) return;
			if (Equals(e.OldValue, ((RQRequisitionLine)e.Row).OrderQty)) return;

			if (((RQRequisitionLine)e.Row).ManualPrice != true)
			sender.SetValue<RQRequisitionLine.curyEstUnitCost>(e.Row, null);
			sender.SetDefaultExt<RQRequisitionLine.curyEstUnitCost>(e.Row);
		}

		protected virtual void RQRequisitionLine_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null) return;
			if (Equals(e.OldValue, ((RQRequisitionLine)e.Row).UOM)) return;

			if (((RQRequisitionLine)e.Row).ManualPrice != true)
			sender.SetValue<RQRequisitionLine.curyEstUnitCost>(e.Row, null);
			sender.SetDefaultExt<RQRequisitionLine.curyEstUnitCost>(e.Row);
		}

		protected virtual void RQRequisitionLine_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null) return;
			if (Equals(e.OldValue, ((RQRequisitionLine)e.Row).SiteID)) return;

			if (((RQRequisitionLine)e.Row).ManualPrice != true)
				sender.SetValue<RQRequisitionLine.curyEstUnitCost>(e.Row, null);
			sender.SetDefaultExt<RQRequisitionLine.curyEstUnitCost>(e.Row);
		}

		protected virtual void RQRequisitionLine_ManualPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (RQRequisitionLine)e.Row;
			if (row != null && row.ManualPrice != true && !sender.Graph.IsCopyPasteContext)
			{
			sender.SetValue<RQRequisitionLine.curyEstUnitCost>(e.Row, null);
			sender.SetDefaultExt<RQRequisitionLine.curyEstUnitCost>(e.Row);
		}
		}

		protected virtual void RQRequisitionLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			RQRequisitionLine row = (RQRequisitionLine)e.Row;
			RQRequisitionLine oldRow = (RQRequisitionLine)e.OldRow;
			if (this.Document.Current != null && this.Document.Current.Hold == true && !(row.Cancelled == true))
				row.OriginQty = row.OrderQty;

			if (row == null)
				return;

			if (row.ByRequest == true && row.UOM != oldRow.UOM)
			{
				foreach (RQRequisitionContent content in PXSelect<RQRequisitionContent,
						Where<RQRequisitionContent.reqNbr, Equal<Required<RQRequisitionLine.reqNbr>>,
							And<RQRequisitionContent.reqLineNbr, Equal<Required<RQRequisitionLine.lineNbr>>>>>.Select(this, row.ReqNbr, row.LineNbr))
				{
					RQRequisitionContent upd = PXCache<RQRequisitionContent>.CreateCopy(content);
					upd.RecalcOnly = true;
					this.Contents.Update(upd);
				}
			}

			if ((e.ExternalCall || sender.Graph.IsImport)
				&& sender.ObjectsEqual<RQRequisitionLine.branchID, RQRequisitionLine.inventoryID, RQRequisitionLine.siteID, RQRequisitionLine.uOM, RQRequisitionLine.orderQty, RQRequisitionLine.manualPrice>(e.Row, e.OldRow)
				&& !sender.ObjectsEqual<RQRequisitionLine.curyEstUnitCost, RQRequisitionLine.curyEstExtCost>(e.Row, e.OldRow))
				row.ManualPrice = true;
		}
		protected virtual void RQRequisitionLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RQRequisitionLine row = (RQRequisitionLine)e.Row;

			if (row == null)
                return;

            PXPersistingCheck persistingCheckMode = Document.Current.Hold == true
                ? PXPersistingCheck.Nothing
                : PXPersistingCheck.NullOrBlank;

            PXDefaultAttribute.SetPersistingCheck<RQRequisitionLine.inventoryID>(sender, row, persistingCheckMode);

			if (row.InventoryID == null)
			{
				if (Document.Current.Hold != true)
					sender.DisplayFieldError<RQRequisitionLine.inventoryID>(row, ErrorMessages.FieldIsEmpty, 
                                                                            PXUIFieldAttribute.GetDisplayName<RQRequisitionLine.inventoryID>(sender));
				else
					sender.ClearFieldSpecificError<RQRequisitionLine.inventoryID>(row, ErrorMessages.FieldIsEmpty,
                                                                                  PXUIFieldAttribute.GetDisplayName<RQRequisitionLine.inventoryID>(sender));
			}

			//PXUIFieldAttribute.SetEnabled<RQRequisitionLine.uOM>(sender, row, !(row.ByRequest == true));
			PXUIFieldAttribute.SetEnabled<RQRequisitionLine.orderQty>(sender, row, !(row.ByRequest == true));
			PXUIFieldAttribute.SetEnabled<RQRequestLine.subItemID>(sender, row, row.InventoryID != null && row.LineType == POLineType.GoodsForInventory);

			PXUIFieldAttribute.SetEnabled<RQRequisitionLine.siteID>(sender, row, true);
			PXUIFieldAttribute.SetEnabled<RQRequisitionLine.markupPct>(sender, row, row.IsUseMarkup == true);

			PXUIFieldAttribute.SetEnabled<RQRequisitionLine.lineType>(sender, row, PXAccess.FeatureInstalled<FeaturesSet.inventory>());

			if (this.Document.Current.Status == RQRequisitionStatus.PendingApproval ||
			this.Document.Current.Status == RQRequisitionStatus.Open ||
			this.Document.Current.Status == RQRequisitionStatus.PendingQuotation)
			{
				ValidateOpenState(row, PXErrorLevel.Warning);
			}

			PXUIFieldAttribute.SetEnabled<RQRequisitionLine.alternateID>(sender, row, this.Document.Current.VendorLocationID != null);
		}


		private bool ValidateOpenState(RQRequisitionLine row, PXErrorLevel level)
		{
			bool result = true;
			Type[] requestOnOpen =
				row.LineType == POLineType.GoodsForInventory && row.InventoryID != null
					? new Type[] { typeof (RQRequisitionLine.uOM), typeof (RQRequisitionLine.siteID), typeof (RQRequisitionLine.subItemID)}
					: row.LineType == POLineType.NonStock
						  ? new Type[] { typeof (RQRequisitionLine.uOM), typeof (RQRequisitionLine.siteID),}
						  : new Type[] { typeof (RQRequisitionLine.uOM)};


			foreach (Type type in requestOnOpen)
			{
				object value = this.Lines.Cache.GetValue(row, type.Name);

				if (value == null)
				{
					this.Lines.Cache.RaiseExceptionHandling(type.Name, row, null,
						new PXSetPropertyException(Messages.ShouldBeDefined, level));
					result = false;
				}
				else
					this.Lines.Cache.RaiseExceptionHandling(type.Name, row, value, null);
			}

			return result;
		}

		protected virtual void RQRequestLineSelect_SelectQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RQRequestLineSelect row = (RQRequestLineSelect)e.Row;

			if (e.NewValue == null || (Decimal)e.NewValue > row.OpenQty)
			{
				e.NewValue = row.OpenQty;
			}

			if (e.NewValue != null && (Decimal)e.NewValue > 0)
				row.Selected = true;
		}

		protected virtual void RQRequestLineSelect_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			RQRequestLineSelect row = (RQRequestLineSelect)e.NewRow;
			RQRequestLineSelect old = (RQRequestLineSelect)e.Row;

			if (row.Selected == true && old.Selected != row.Selected && row.SelectQty.GetValueOrDefault() == 0m)
			{
				row.SelectQty = row.OpenQty;
				row.BaseSelectQty = row.OpenQty;
			}
		}
		protected virtual void RQRequestLineFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RQRequestLineFilter row = (RQRequestLineFilter)e.Row;
			if (row == null) return;
			PXUIFieldAttribute.SetEnabled<RQRequestLineFilter.inventoryID>(sender, null, row.AllowUpdate == true);
			PXUIFieldAttribute.SetEnabled<RQRequestLineFilter.subItemID>(sender, null, row.AllowUpdate == true);
			PXUIFieldAttribute.SetEnabled<RQRequestLineFilter.addExists>(sender, null, row.AllowUpdate == true);
		}

		protected virtual void RQRequisitionContent_ItemQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RQRequisitionContent row = (RQRequisitionContent)e.Row;

			RQRequestLine line = PXSelect<RQRequestLine,
				Where<RQRequestLine.orderNbr, Equal<Required<RQRequestLine.orderNbr>>,
					And<RQRequestLine.lineNbr, Equal<Required<RQRequestLine.lineNbr>>>>>.Select(this, row.OrderNbr, row.LineNbr);

			Decimal delta = ((Decimal?)e.NewValue).GetValueOrDefault() - row.ItemQty.GetValueOrDefault();

			if (delta > 0 && line.OpenQty < delta)
			{
				e.NewValue = row.ItemQty + line.OpenQty;
				sender.RaiseExceptionHandling<RQRequisitionContent.itemQty>(row, null,
					new PXSetPropertyException(Messages.InsuffQty_LineQtyUpdated, PXErrorLevel.Warning));
			}
		}

		protected virtual void RQRequisitionContent_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			RQRequisitionContent row = (RQRequisitionContent)e.NewRow;
			RQRequisitionContent old = (RQRequisitionContent)e.Row;

			RQRequestLine line = PXSelect<RQRequestLine,
				Where<RQRequestLine.orderNbr, Equal<Required<RQRequestLine.orderNbr>>,
					And<RQRequestLine.lineNbr, Equal<Required<RQRequestLine.lineNbr>>>>>.Select(this, row.OrderNbr, row.LineNbr);

			RQRequisitionLine req = PXSelect<RQRequisitionLine,
							Where<RQRequisitionLine.reqNbr, Equal<Required<RQRequisitionLine.reqNbr>>,
								And<RQRequisitionLine.lineNbr, Equal<Required<RQRequisitionLine.lineNbr>>>>>.Select(this, row.ReqNbr, row.ReqLineNbr);
			if (row.RecalcOnly == true)
			{
				if (line.UOM != req.UOM)
					row.ReqQty = INUnitAttribute.ConvertFromBase(sender, req.InventoryID, req.UOM, row.BaseReqQty.GetValueOrDefault(), INPrecision.QUANTITY);
				else
					row.ReqQty = row.ItemQty;
			}
			else if (line.InventoryID == null)
			{
				if (row.ItemQty != old.ItemQty)
				{
					row.BaseItemQty =
					row.ReqQty =
					row.BaseReqQty = row.ItemQty;
				}
				else if (row.ReqQty != old.ReqQty)
				{
					row.BaseItemQty =
					row.ItemQty =
					row.BaseReqQty = row.ReqQty;
				}
			}
			else if (line.InventoryID == req.InventoryID)
			{
				if (row.ItemQty != old.ItemQty)
				{
					row.BaseReqQty =
					row.BaseItemQty = INUnitAttribute.ConvertToBase(sender, line.InventoryID, line.UOM, row.ItemQty.GetValueOrDefault(), INPrecision.QUANTITY);

					if (line.UOM != req.UOM)
						row.ReqQty = INUnitAttribute.ConvertFromBase(sender, req.InventoryID, req.UOM, row.BaseReqQty.GetValueOrDefault(), INPrecision.QUANTITY);
					else
						row.ReqQty = row.ItemQty;
				}

				if (row.ReqQty != old.ReqQty)
				{
					row.BaseReqQty =
					row.BaseItemQty = INUnitAttribute.ConvertToBase(sender, req.InventoryID, req.UOM, row.ReqQty.GetValueOrDefault(), INPrecision.QUANTITY);

					Decimal value = INUnitAttribute.ConvertFromBase(sender, line.InventoryID, line.UOM, row.BaseReqQty.GetValueOrDefault(), INPrecision.QUANTITY);
					if (line.UOM == req.UOM) value = row.ReqQty ?? 0;
					object newValue = value;
					sender.RaiseFieldVerifying<RQRequisitionContent.itemQty>(row, ref newValue);

					row.ItemQty = (Decimal)newValue;

					if ((Decimal)newValue != value)
					{
						row.BaseReqQty =
						row.BaseItemQty = INUnitAttribute.ConvertToBase(sender, line.InventoryID, line.UOM, row.ItemQty.GetValueOrDefault(), INPrecision.QUANTITY);

						if (line.UOM != req.UOM)
							row.ReqQty = INUnitAttribute.ConvertFromBase(sender, req.InventoryID, req.UOM, row.BaseReqQty.GetValueOrDefault(), INPrecision.QUANTITY);
						else
							row.ReqQty = row.ItemQty;
					}
				}
			}
			else
			{
				if (row.ItemQty != old.ItemQty)
					row.BaseItemQty = INUnitAttribute.ConvertToBase(sender, line.InventoryID, line.UOM, row.ItemQty.GetValueOrDefault(), INPrecision.QUANTITY);

				if (row.ReqQty != old.ReqQty)
					row.BaseReqQty = INUnitAttribute.ConvertToBase(sender, req.InventoryID, req.UOM, row.ReqQty.GetValueOrDefault(), INPrecision.QUANTITY);
			}
		}

		protected virtual void RQBiddingVendor_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			RQBiddingVendor row = (RQBiddingVendor)e.Row;

			if (row != null && row.VendorLocationID != null)
			{
				row.ReqNbr = this.Document.Current.ReqNbr;
				PORemitAddressAttribute.DefaultRecord<RQBiddingVendor.remitAddressID>(sender, e.Row);
				PORemitContactAttribute.DefaultRecord<RQBiddingVendor.remitContactID>(sender, e.Row);
				e.Cancel = !ValidateBiddingVendorDuplicates(sender, row, null);
			}
		}

		protected virtual void RQBiddingVendor_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			RQBiddingVendor row = (RQBiddingVendor)e.Row;
			RQBiddingVendor newRow = (RQBiddingVendor)e.NewRow;

			if (row != null && newRow != null && row != newRow &&
				 (row.VendorID != newRow.VendorID || row.VendorLocationID != newRow.VendorLocationID))
            {
				e.Cancel = !ValidateBiddingVendorDuplicates(sender, newRow, row);
		}
		}

		protected virtual void RQBiddingVendor_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RQBiddingVendor row = (RQBiddingVendor)e.Row;

			if (row == null)
                return;

			RQBidding b =
			PXSelect<RQBidding,
				Where<RQBidding.reqNbr, Equal<Current<RQBiddingVendor.reqNbr>>,
					And<RQBidding.vendorID, Equal<Current<RQBiddingVendor.vendorID>>,
						And<RQBidding.vendorLocationID, Equal<Current<RQBiddingVendor.vendorLocationID>>>>>>
						.SelectSingleBound(this, new object[] { row });

			bool enabled = b == null ||
										 row.VendorID == null ||
										 row.VendorLocationID == null;

			PXUIFieldAttribute.SetEnabled<RQBiddingVendor.vendorID>(sender, row, enabled);
			PXUIFieldAttribute.SetEnabled<RQBiddingVendor.vendorLocationID>(sender, row, enabled);
			PXUIFieldAttribute.SetEnabled<RQBiddingVendor.curyID>(sender, row, enabled);
			PXUIFieldAttribute.SetEnabled<RQBiddingVendor.curyInfoID>(sender, row, enabled);
		}
		protected virtual void RQBiddingVendor_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				vendorBidder.Current = (Vendor)vendorBidder.View.SelectSingleBound(new object[] { e.Row });
				sender.SetDefaultExt<RQBiddingVendor.curyID>(e.Row);

				if (e.ExternalCall)
				{
					CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<RQBiddingVendor.curyInfoID>(sender, e.Row);

					string message = PXUIFieldAttribute.GetError<RQBiddingVendor.curyID>(currencyinfo.Cache, info);

					if (string.IsNullOrEmpty(message) == false)
					{
						sender.RaiseExceptionHandling<RQRequisition.orderDate>(e.Row, ((SOOrder)e.Row).OrderDate, new PXSetPropertyException(message, PXErrorLevel.Warning));
					}

					if (info != null)
					{
						((RQBiddingVendor)e.Row).CuryID = info.CuryID;
						((RQBiddingVendor)e.Row).CuryInfoID = info.CuryInfoID;
					}
				}				
			}
		}
		
		protected virtual void RQBiddingVendor_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PORemitAddressAttribute.DefaultRecord<RQBiddingVendor.remitAddressID>(sender, e.Row);
			PORemitContactAttribute.DefaultRecord<RQBiddingVendor.remitContactID>(sender, e.Row);
		}

		protected virtual void RQBiddingVendor_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			vendorBidder.Current = null;
		}

		protected virtual void RQBiddingVendor_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			vendorBidder.Current = null;
		}

		protected virtual void RQRequest_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			RQRequest row = e.Row as RQRequest;
			if (e.Operation == PXDBOperation.Update && row != null)
			{
				row.Status = row.OpenOrderQty > 0 ? RQRequestStatus.Open : RQRequestStatus.Closed;
			}
		}

		private bool ValidateBiddingVendorDuplicates(PXCache sender, RQBiddingVendor row, RQBiddingVendor oldRow)
		{
			if (row.VendorLocationID != null)
            {
				foreach (RQBiddingVendor sibling in Vendors.Select(row.ReqNbr ?? this.Document.Current.ReqNbr))
				{
					if (sibling.VendorID == row.VendorID &&
							sibling.VendorLocationID == row.VendorLocationID &&
							row.LineID != sibling.LineID)
					{
						if (oldRow == null || oldRow.VendorID != row.VendorID)
                        {
							sender.RaiseExceptionHandling<RQBiddingVendor.vendorID>(
                                row, row.VendorID, new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded));
                        }

						if (oldRow == null || oldRow.VendorLocationID != row.VendorLocationID)
                        {
							sender.RaiseExceptionHandling<RQBiddingVendor.vendorLocationID>(
                                row, row.VendorLocationID, new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded));
                        }

						return false;
					}
				}
            }

			PXUIFieldAttribute.SetError<RQBiddingVendor.vendorID>(sender, row, null);
			PXUIFieldAttribute.SetError<RQBiddingVendor.vendorLocationID>(sender, row, null);
			return true;
		}

        public virtual void InsertRequestLine(RQRequestLine line, decimal selectQty, bool mergeLines)
		{
			RQRequisitionLine req = null;
            RQRequest r = PXSelect<RQRequest,
                             Where<RQRequest.orderNbr, Equal<Required<RQRequest.orderNbr>>>>
                          .SelectWindowed(this, 0, 1, line.OrderNbr);

            if (r == null)
                return;

            if (mergeLines && line.InventoryID != null && line.ManualPrice != true)
            {
                var pars = new List<object> {Document.Current.ReqNbr, line.InventoryID, line.SubItemID, line.Description};
                var reqEnq = new PXSelect<RQRequisitionLine,
					Where<RQRequisitionLine.reqNbr, Equal<Required<RQRequisitionLine.reqNbr>>,
						And<RQRequisitionLine.inventoryID, Equal<Required<RQRequisitionLine.inventoryID>>,
						And<RQRequisitionLine.subItemID, Equal<Required<RQRequisitionLine.subItemID>>,
                        And<RQRequisitionLine.description, Equal<Required<RQRequisitionLine.description>>,
                        And<RQRequisitionLine.byRequest, Equal<True>,
                        And<RQRequisitionLine.manualPrice, Equal<False>>>>>>>>
                        (this);

                if (line.ExpenseAcctID != null && line.ExpenseSubID != null)
                {
                    reqEnq.WhereAnd<
                        Where<RQRequisitionLine.expenseAcctID, Equal<Required<RQRequisitionLine.expenseAcctID>>,
                        And<RQRequisitionLine.expenseSubID, Equal<Required<RQRequisitionLine.expenseSubID>>>>>();
                    pars.AddRange(new object[] {line.ExpenseAcctID, line.ExpenseSubID});
                }
                else
                    reqEnq.WhereAnd<
                        Where<RQRequisitionLine.expenseAcctID, IsNull,
                        And<RQRequisitionLine.expenseSubID, IsNull>>>();

                req = reqEnq.SelectSingle(pars.ToArray());
            }

			if (req == null)
			{
                req = CreateNewRequisitionLineFromRequestLine(r, line);
				req = this.Lines.Update(req);

				PXNoteAttribute.CopyNoteAndFiles(SourceRequestLines.Cache, line, Lines.Cache, req);
			}
			else
			{
				decimal? curyEstUnitCost = null;
				decimal? curyEstExtCost = null;

				if (Document.Current.CuryID == r.CuryID)
				{
					curyEstUnitCost = line.CuryEstUnitCost;
					curyEstExtCost = line.CuryEstExtCost;
				}
				else
				{
                    decimal unitCost;

                    //requestLine is RQRequestLine while RQRequisitionLine cache and curyInfoID field is used - looks suspicious
                    PXCurrencyAttribute.CuryConvCury<RQRequisitionLine.curyInfoID>(Lines.Cache, line, line.EstUnitCost ?? 0m, out unitCost, true);
					curyEstUnitCost = unitCost;

                    PXCurrencyAttribute.CuryConvCury<RQRequisitionLine.curyInfoID>(Lines.Cache, line, line.EstExtCost ?? 0m, out unitCost);
					curyEstExtCost = unitCost;
				}

				if (curyEstUnitCost != 0 && curyEstUnitCost != req.CuryEstExtCost)
				{
					if (req.CuryEstUnitCost == 0)
					{
						req.CuryEstUnitCost = curyEstUnitCost;
					}

					decimal? total = req.CuryEstExtCost + curyEstExtCost;
					req = PXCache<RQRequisitionLine>.CreateCopy(req);

                    UpdateExistingRQRequisitionLineCosts(req, line, selectQty, total, 
                                                                            areCurrenciesSame: Document.Current.CuryID == r.CuryID);
                    req = Lines.Update(req);
				}
			}

			UpdateContent(req, line, selectQty);

            if (req == null)
                return;

				req = PXCache<RQRequisitionLine>.CreateCopy(req);

				if (req.LineType == null && req.InventoryID == null)
            {
					req.LineType = POLineType.Service;
			}

            Lines.Cache.SetDefaultExt<RQRequisitionLine.siteID>(req);
            Lines.Cache.Update(req);
        }

        /// <summary>
        /// Creates new requisition line from request line on insertion of request line. This is an extension point used by Lexware PriceUnit customization
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="requestLine">The request line.</param>
        /// <returns>
        /// The new new requisition line from request line on insert request line.
        /// </returns>
        protected virtual RQRequisitionLine CreateNewRequisitionLineFromRequestLine(RQRequest request, RQRequestLine requestLine)
        {
			var requisitionLine = new RQRequisitionLine
            {
				ReqNbr = Document.Current.ReqNbr,
				InventoryID = requestLine.InventoryID,
				SubItemID = requestLine.SubItemID,
				Description = requestLine.Description,
				UOM = requestLine.UOM,
				OrderQty = 0m,
				ManualPrice = requestLine.ManualPrice,
				ExpenseAcctID = requestLine.ExpenseAcctID,
				ExpenseSubID = requestLine.ExpenseSubID,
				RequestedDate = requestLine.RequestedDate,
				PromisedDate = requestLine.PromisedDate,
				ByRequest = true
            };
            
            if (Document.Current.CuryID == request.CuryID)
            {
                requisitionLine.CuryEstUnitCost = requestLine.CuryEstUnitCost;
            }
            else
            {
                decimal unitCost;
				PXCurrencyAttribute.CuryConvCury<RQRequisitionLine.curyInfoID>(Lines.Cache, requestLine, requestLine.EstUnitCost.GetValueOrDefault(), out unitCost, true);
                requisitionLine.CuryEstUnitCost = unitCost;
            }
            return requisitionLine;
		}

        /// <summary>
        /// Updates the existing requisition line costs on insertion of request lines. This is an extension point used in Lexware PriceUnit customization.
        /// </summary>
        /// <param name="requisitionLine">The requisition line.</param>
        /// <param name="selectQty">The selected quantity.</param>
        /// <param name="totalCost">The total cost of requisition and request lines</param>
        protected virtual void UpdateExistingRQRequisitionLineCosts(RQRequisitionLine requisitionLine, RQRequestLine requestLine,
                                                                    decimal selectQty, decimal? totalCost, bool areCurrenciesSame)
		{
            requisitionLine.CuryEstUnitCost = totalCost / (requisitionLine.OrderQty + selectQty);
        }

        public RQRequisitionContent UpdateContent(RQRequisitionLine rqLine, RQRequestLine requestLine, decimal selectQty)
		{
			RQRequisitionContent content = 
				PXSelect<RQRequisitionContent,
				Where<RQRequisitionContent.orderNbr, Equal<Required<RQRequisitionContent.orderNbr>>,
					And<RQRequisitionContent.lineNbr, Equal<Required<RQRequisitionContent.lineNbr>>,
					And<RQRequisitionContent.reqNbr, Equal<Required<RQRequisitionContent.reqNbr>>,
					And<RQRequisitionContent.reqLineNbr, Equal<Required<RQRequisitionContent.reqLineNbr>>>>>>>
                 .Select(this, requestLine.OrderNbr, requestLine.LineNbr, rqLine.ReqNbr, rqLine.LineNbr);

			if (content == null)
			{
                content = new RQRequisitionContent
                {
                    OrderNbr = requestLine.OrderNbr,
                    LineNbr = requestLine.LineNbr,
                    ReqNbr = rqLine.ReqNbr,
                    ReqLineNbr = rqLine.LineNbr
                };

				content = this.Contents.Insert(content);
			}

			content = (RQRequisitionContent)this.Contents.Cache.CreateCopy(content);
			content.ItemQty += selectQty;
			return this.Contents.Update(content);
		}

		private long? CopyCurrenfyInfo(PXGraph graph, long? SourceCuryInfoID)
		{
			CurrencyInfo curryInfo = currencyinfo.Select(SourceCuryInfoID);
			curryInfo.CuryInfoID = null;
			graph.Caches[typeof (CurrencyInfo)].Clear();
			curryInfo = (CurrencyInfo)graph.Caches[typeof(CurrencyInfo)].Insert(curryInfo);
			return curryInfo.CuryInfoID;
		}

		private class PO4SO : Dictionary<int?, List<POLine>>
		{
			public virtual void Add(int? key, POLine line)
			{
				if (line == null)
                    return;

				List<POLine> source;

				if (!this.TryGetValue(key, out source))
					this[key] = source = new List<POLine>();

				source.Add(line);
			}
		}

		public class PriceRecalcExt : PriceRecalcExt<RQRequisitionEntry, RQRequisition, RQRequisitionLine, RQRequisitionLine.curyEstUnitCost>
		{
			protected override PXSelectBase<RQRequisitionLine> DetailSelect => Base.Lines;
			protected override IPricedLine WrapLine(RQRequisitionLine line) => new PricedLine(line);

			private class PricedLine : IPricedLine
			{
				private readonly RQRequisitionLine _line;
				public PricedLine(RQRequisitionLine line) { _line = line; }

				public bool? ManualPrice
				{
					get { return _line.ManualPrice; }
					set { _line.ManualPrice = value; }
				}

				public int? InventoryID
				{
					get { return _line.InventoryID; }
					set { _line.InventoryID = value; }
				}

				public decimal? CuryUnitPrice
				{
					get { return _line.CuryEstUnitCost; }
					set { _line.CuryEstUnitCost = value; }
				}

				public decimal? CuryExtPrice
				{
					get { return _line.CuryEstExtCost; }
					set { _line.CuryEstExtCost = value; }
				}
			}
		}
	}

    [Serializable]
	public partial class RQRequisitionStatic : IBqlTable
	{
		#region SourceReqNbr
		public abstract class sourceReqNbr : PX.Data.BQL.BqlString.Field<sourceReqNbr> { }
		protected String _SourceReqNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Source Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(RQSetup.requisitionNumberingID), typeof(RQRequisition.orderDate))]
		[PXSelectorAttribute(
			typeof(Search<RQRequisition.reqNbr, Where<RQRequisition.status, Equal<RQRequisitionStatus.hold>>>),
			typeof(RQRequisition.employeeID),
			typeof(RQRequisition.vendorID),
			Filterable = true)]
		public virtual String SourceReqNbr
		{
			get
			{
				return this._SourceReqNbr;
			}
			set
			{
				this._SourceReqNbr = value;
			}
		}
		#endregion
		#region ReqNbr
		public abstract class reqNbr : PX.Data.BQL.BqlString.Field<reqNbr> { }
		protected String _ReqNbr;

		[PXDBString(15, IsUnicode = true, InputMask = "")]
		public virtual String ReqNbr
		{
			get
			{
				return this._ReqNbr;
			}
			set
			{
				this._ReqNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected int? _LineNbr;
		[PXDBInt]
		public virtual int? LineNbr
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
	}
	[PX.TM.OwnedEscalatedFilter.Projection(
		typeof(RQRequestLineFilter),
		typeof(RQRequestLine),
		typeof(InnerJoin<RQRequest, On<RQRequest.orderNbr, Equal<RQRequestLine.orderNbr>,
									And<RQRequest.status, Equal<RQRequestStatus.open>,
									And<RQRequestLine.openQty, Greater<PX.Objects.CS.decimal0>>>>>),
		null,
		typeof(RQRequest.workgroupID),
		typeof(RQRequest.ownerID),
		typeof(RQRequest.orderDate),
		typeof(Where<CurrentValue<RQRequestLineFilter.filterSet>, Equal<False>>))]
    [Serializable]
	public partial class RQRequestLineSelect : IBqlTable
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
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected string _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(RQRequestLine.orderNbr))]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected int? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(RQRequestLine.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual int? LineNbr
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(RQRequest.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Currency.curyID))]
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
		#region SelectQty
		public abstract class selectQty : PX.Data.BQL.BqlDecimal.Field<selectQty> { }
		protected Decimal? _SelectQty;
		[PXQuantity(typeof(RQRequestLineSelect.uOM), typeof(RQRequestLineSelect.baseSelectQty))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Select Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? SelectQty
		{
			get
			{
				return this._SelectQty;
			}
			set
			{
				this._SelectQty = value;
			}
		}
		#endregion
		#region BaseSelectQty
		public abstract class baseSelectQty : PX.Data.BQL.BqlDecimal.Field<baseSelectQty> { }
		protected Decimal? _BaseSelectQty;
		[PXDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? BaseSelectQty
		{
			get
			{
				return this._BaseSelectQty;
			}
			set
			{
				this._BaseSelectQty = value;
			}
		}
		#endregion
		#region OpenQty
		public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
		protected Decimal? _OpenQty;
		[PXDBQuantity(typeof(RQRequestLine.uOM), typeof(RQRequestLine.baseOpenQty), HandleEmptyKey = true, BqlField = typeof(RQRequestLine.openQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXDBDecimal(6, BqlField = typeof(RQRequestLine.baseOpenQty))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(Filterable = true, BqlField = typeof(RQRequestLine.inventoryID))]
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
		[SubItem(typeof(RQRequestLine.inventoryID), BqlField = typeof(RQRequestLine.subItemID))]
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
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(RQRequestLine.description))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<RQRequestLineSelect.inventoryID>>>>))]
		[INUnit(typeof(RQRequestLineSelect.inventoryID), DisplayName = "UOM", BqlField = typeof(RQRequestLine.uOM))]
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

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[VendorNonEmployeeActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, BqlField=typeof(RQRequestLine.vendorID))]
		[PXDefault(
			typeof(
			Search2<Vendor.bAccountID,
				LeftJoin<InventoryItem,
							On<InventoryItem.preferredVendorID, Equal<Vendor.bAccountID>,
						And<InventoryItem.inventoryID, Equal<Current<RQRequestLine.inventoryID>>>>>,
			Where2<
					Where<Current<RQRequest.vendorID>, IsNotNull, And<Vendor.bAccountID, Equal<Current<RQRequest.vendorID>>>>,
			Or<
					Where<Current<RQRequest.vendorID>, IsNull, And<InventoryItem.preferredVendorID, IsNotNull>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region VendorLocationID
		public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
		protected Int32? _VendorLocationID;

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<RQRequestLine.vendorID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, BqlField = typeof(RQRequestLine.vendorLocationID))]
		[PXDefault(typeof(Search<Vendor.defLocationID, Where<Vendor.bAccountID, Equal<Current<RQRequestLine.vendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? VendorLocationID
		{
			get
			{
				return this._VendorLocationID;
			}
			set
			{
				this._VendorLocationID = value;
			}
		}
		#endregion
		#region VendorName
		public abstract class vendorName : PX.Data.BQL.BqlString.Field<vendorName> { }
		protected String _VendorName;
		[PXString(50, IsUnicode = true)]
		[PXDBScalar(typeof(Search<Vendor.acctName, Where<Vendor.bAccountID, Equal<RQRequestLine.vendorID>>>))]
		[PXDefault(typeof(Search<Vendor.acctName, Where<Vendor.bAccountID, Equal<Current<RQRequestLine.vendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Vendor Name", Enabled = false)]
		public virtual String VendorName
		{
			get
			{
				return this._VendorName;
			}
			set
			{
				this._VendorName = value;
			}
		}
		#endregion
		#region VendorRefNbr
		public abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr> { }
		protected String _VendorRefNbr;
		[PXDBString(40, IsUnicode = true, BqlField = typeof(RQRequestLine.vendorRefNbr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Vendor Ref.")]
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
		#region VendorDescription
		public abstract class vendorDescription : PX.Data.BQL.BqlString.Field<vendorDescription> { }
		protected String _VendorDescription;
		[PXDBString(100, IsUnicode = true, BqlField=typeof(RQRequestLine.vendorDescription))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Vendor Description")]
		public virtual String VendorDescription
		{
			get
			{
				return this._VendorDescription;
			}
			set
			{
				this._VendorDescription = value;
			}
		}
		#endregion
		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;
		[PXDBString(50, IsUnicode = true, InputMask = "", BqlField=typeof(RQRequestLine.alternateID))]
		[PXUIField(DisplayName = "Alternate ID")]
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
		#region RequestedDate
		public abstract class requestedDate : PX.Data.BQL.BqlDateTime.Field<requestedDate> { }
		protected DateTime? _RequestedDate;
		[PXDBDate(BqlField=typeof(RQRequestLine.requestedDate))]
		[PXUIField(DisplayName = "Required Date")]
		public virtual DateTime? RequestedDate
		{
			get
			{
				return this._RequestedDate;
			}
			set
			{
				this._RequestedDate = value;
			}
		}
		#endregion
		#region PromisedDate
		public abstract class promisedDate : PX.Data.BQL.BqlDateTime.Field<promisedDate> { }
		protected DateTime? _PromisedDate;
		[PXDBDate(BqlField=typeof(RQRequestLine.promisedDate))]
		[PXUIField(DisplayName = "Promised Date")]
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
		#region ReqQty
		public abstract class reqQty : PX.Data.BQL.BqlDecimal.Field<reqQty> { }
		protected Decimal? _ReqQty;
		[PXDBQuantity(typeof(RQRequestLine.uOM), typeof(RQRequestLine.baseReqQty), HandleEmptyKey = true, BqlField=typeof(RQRequestLine.reqQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Requisition Qty.", Enabled = false)]
		public virtual Decimal? ReqQty
		{
			get
			{
				return this._ReqQty;
			}
			set
			{
				this._ReqQty = value;
			}
		}
		#endregion
		#region BaseReqQty
		public abstract class baseReqQty : PX.Data.BQL.BqlDecimal.Field<baseReqQty> { }
		protected Decimal? _BaseReqQty;
		[PXDBDecimal(6, BqlField = typeof(RQRequestLine.baseReqQty))]
		public virtual Decimal? BaseReqQty
		{
			get
			{
				return this._BaseReqQty;
			}
			set
			{
				this._BaseReqQty = value;
			}
		}
		#endregion	
	}

	//Add fields
	[System.SerializableAttribute()]
	public partial class RQRequestLineFilter : RQRequestSelection
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(Filterable = true)]
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
		[SubItem(typeof(RQRequestLineFilter.inventoryID))]
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
		#region AllowUpdate
		public abstract class allowUpdate : PX.Data.BQL.BqlBool.Field<allowUpdate> { }
		protected Boolean? _AllowUpdate = true;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual Boolean? AllowUpdate
		{
			get
			{
				return this._AllowUpdate;
			}
			set
			{
				this._AllowUpdate = value;
			}
		}
		#endregion
		#region FilterSet
		public new abstract class filterSet : PX.Data.BQL.BqlBool.Field<filterSet> { }
		#endregion
	}
}
