using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.EP;
using System.Collections;
using PX.Objects.IN;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.TM;
using PX.Objects.PO;

namespace PX.Objects.RQ
{
	[PX.TM.OwnedEscalatedFilter.Projection(
			typeof(RQRequestSelection),
			typeof(RQRequestLine),
			typeof(InnerJoin<RQRequest, On<RQRequest.orderNbr, Equal<RQRequestLine.orderNbr>,
										And<RQRequest.status, Equal<RQRequestStatus.open>,
										And<RQRequestLine.openQty, Greater<PX.Objects.CS.decimal0>>>>,
						 InnerJoin<RQRequestClass, On<RQRequestClass.reqClassID, Equal<RQRequest.reqClassID>>>>),
			null,
			typeof(RQRequest.workgroupID),
			typeof(RQRequest.ownerID),
			typeof(RQRequest.orderDate))]
	[Serializable]
	public partial class RQRequestLineOwned : RQRequestLine
	{
		#region OrderNbr
		public new abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXSelectorAttribute(typeof(Search<RQRequest.orderNbr>))]
		[PXUIField(DisplayName = "Ref. Nbr.")]
        public override String OrderNbr
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
		public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#endregion
		#region VendorID
		public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		#endregion
		#region Description
		public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		#endregion
		#region ReqClassID
		public abstract class reqClassID : PX.Data.BQL.BqlString.Field<reqClassID> { }
		protected string _ReqClassID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(RQRequest.reqClassID))]
		[PXDefault(typeof(RQSetup.defaultReqClassID))]
		[PXUIField(DisplayName = "Request Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(RQRequestClass.reqClassID), DescriptionField = typeof(RQRequestClass.descr))]
		public virtual string ReqClassID
		{
			get
			{
				return this._ReqClassID;
			}
			set
			{
				this._ReqClassID = value;
			}
		}
		#endregion
		#region Priority
		public abstract class priority : PX.Data.BQL.BqlInt.Field<priority> { }
		protected int? _Priority;

		[PXDBInt(BqlField = typeof(RQRequest.priority))]
		[PXUIField]
		[PXDefault(1)]
		[PXIntList(new int[] { 0, 1, 2 },
			new string[] { "Low", "Normal", "High" })]
		public virtual Int32? Priority
		{
			get
			{
				return this._Priority;
			}
			set
			{
				this._Priority = value;
			}
		}
		#endregion
		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
		protected DateTime? _OrderDate;
		[PXDBDate(BqlField = typeof(RQRequest.orderDate))]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region CustomerRequest
		public abstract class customerRequest : PX.Data.BQL.BqlBool.Field<customerRequest> { }
		protected bool? _CustomerRequest;
		[PXDBBool(BqlField = typeof(RQRequestClass.customerRequest))]
		[PXUIField(DisplayName = "Customer Request", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual bool? CustomerRequest
		{
			get
			{
				return this._CustomerRequest;
			}
			set
			{
				this._CustomerRequest = value;
			}
		}
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;
		[PXUIField(DisplayName = "Requested By", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBInt(BqlField = typeof(RQRequest.employeeID))]
		[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[RQRequesterSelector(typeof(RQRequestApprove.reqClassID))]
		public virtual Int32? EmployeeID
		{
			get
			{
				return this._EmployeeID;
			}
			set
			{
				this._EmployeeID = value;
			}
		}
		#endregion
		#region DepartmentID
		public new abstract class departmentID : PX.Data.BQL.BqlString.Field<departmentID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Department")]
		public override String DepartmentID
		{
			get
			{
				return this._DepartmentID;
			}
			set
			{
				this._DepartmentID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected int? _LocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Location.bAccountID>>),
								DescriptionField = typeof(Location.descr),
								Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location", BqlField = typeof(RQRequest.locationID))]
		public int? LocationID
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
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[Inventory(Filterable = true)]
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
		#region ShipDestType
		public abstract class shipDestType : PX.Data.BQL.BqlString.Field<shipDestType> { }
		protected String _ShipDestType;
		[PXDBString(1, IsFixed = true, BqlField = typeof(RQRequest.shipDestType))]
		[POShippingDestination.List()]
		[PXUIField(DisplayName = "Shipping Destination Type")]
		public virtual String ShipDestType
		{
			get
			{
				return this._ShipDestType;
			}
			set
			{
				this._ShipDestType = value;
			}
		}
		#endregion
	}

	[Serializable]
	[TableAndChartDashboardType]
	public class RQRequestProcess : PXGraph<RQRequestProcess>
	{
		public PXCancel<RQRequestSelection> Cancel;
		public PXAction<RQRequestSelection> details;
		public PXFilter<RQRequestSelection> Filter;
		public PXFilter<BAccount> bAccount;
		public PXFilter<Vendor> Vendor;
		public PXSetup<RQSetup> Setup;		
		[PXFilterable]	
		public RQRequestProcessing Records;


		public RQRequestProcess()
		{
			Records.SetSelected<RQRequestLine.selected>();			
		}

		public virtual void RQRequestSelection_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RQRequestSelection filter = Filter.Current;
			Records.SetProcessDelegate(list => GenerateRequisition(filter, list));
		}

		

		public class RQRequestProcessing : PXFilteredProcessingJoin<RQRequestLineOwned, RQRequestSelection,
				 LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<RQRequestLineOwned.employeeID>>>,			
			   Where2<Where<Current<RQRequestSelection.selectedPriority>, Equal<AllPriority>,
						        Or<RQRequestLineOwned.priority, Equal<Current<RQRequestSelection.selectedPriority>>>>,
						And<Where<Customer.bAccountID, IsNull,
								Or<Match<Customer, Current<AccessInfo.userName>>>>>>,
					OrderBy<Desc<RQRequestLineOwned.priority,Asc<RQRequestLineOwned.orderNbr,Asc<RQRequestLineOwned.lineNbr>>>>>
		{
			public RQRequestProcessing(PXGraph graph)
				: base(graph)
			{
				base._OuterView.WhereAndCurrent<RQRequestSelection>(typeof(RQRequestSelection.workGroupID).Name, typeof(RQRequestSelection.ownerID).Name);
			}
			public RQRequestProcessing(PXGraph graph, Delegate handler)
				: base(graph, handler)
			{
				base._OuterView.WhereAndCurrent<RQRequestSelection>(typeof(RQRequestSelection.workGroupID).Name, typeof(RQRequestSelection.ownerID).Name);
			}			
			[PXUIField(DisplayName = "Process", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
			[PXProcessButton]
			protected override IEnumerable Process(PXAdapter adapter)
			{
				return !CheckCustomer(adapter, true) ? adapter.Get() : base.Process(adapter);
			}

			[PXUIField(DisplayName = "Process All", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
			[PXProcessButton]
			protected override IEnumerable ProcessAll(PXAdapter adapter)
			{
				return !CheckCustomer(adapter, false) ? adapter.Get() : base.ProcessAll(adapter);
			}

			private bool CheckCustomer(PXAdapter adapter, bool onlySelected)
			{
				try
				{
					if (System.Web.HttpContext.Current == null)
						return true;
				}
				catch (Exception)
				{
					return true;
				}								

				RQRequestLineOwned prev = null;
				bool isCustomerDiffer = false;
                var list = onlySelected ? GetSelectedItems(this.View.Cache, this.View.Cache.Cached) : this.View.SelectMulti().RowCast<RQRequestLineOwned>();

                foreach (RQRequestLineOwned line in list)
				{
					if(!(prev == null ||
							(prev.CustomerRequest == true &&
								prev.EmployeeID == line.EmployeeID &&
								prev.LocationID == line.LocationID) ||
							(prev.CustomerRequest == line.CustomerRequest && line.CustomerRequest != true))) 
					{							
						isCustomerDiffer = true;
						break;
					}						
					prev = line;
				}
				return !isCustomerDiffer ||
					this.Ask(CR.Messages.Confirmation, Messages.RequisitionCreationConfirmation, MessageButtons.YesNo) == WebDialogResult.Yes;
			}
		}

		
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXEditDetailButton]
		public virtual IEnumerable Details(PXAdapter adapter)
		{
			if (Records.Current != null && Filter.Current != null)
			{
				RQRequestEntry graph = PXGraph.CreateInstance<RQRequestEntry>();
				graph.Document.Current = graph.Document.Search<RQRequest.orderNbr>(Records.Current.OrderNbr);
				throw new PXRedirectRequiredException(graph, true, AR.Messages.ViewDocument) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

        public struct VendorRef
        {
            public int VendorID;
            public int LocationID;
        }

        private static void GenerateRequisition(RQRequestSelection filter, List<RQRequestLineOwned> lines)
        {
            RQRequisitionEntry graph = PXGraph.CreateInstance<RQRequisitionEntry>();

			RQRequisition requisition = (RQRequisition)graph.Document.Cache.CreateInstance();
            graph.Document.Insert(requisition);
            requisition.ShipDestType = null;

            bool isCustomerSet = true;
            bool isVendorSet = true;
            bool isShipToSet = true;
            int? shipContactID = null;
            int? shipAddressID = null;
            var vendors = new HashSet<VendorRef>();

            foreach (RQRequestLine line in lines)
            {
                PXResult<RQRequest, RQRequestClass> e =
                    (PXResult<RQRequest, RQRequestClass>)
                    PXSelectJoin<RQRequest,
                    	InnerJoin<RQRequestClass, 
                    		On<RQRequestClass.reqClassID, Equal<RQRequest.reqClassID>>>,
                    	Where<RQRequest.orderNbr, Equal<Required<RQRequest.orderNbr>>>>
                    	.Select(graph, line.OrderNbr);

                RQRequest req = e;
                RQRequestClass reqclass = e;

                requisition = PXCache<RQRequisition>.CreateCopy(graph.Document.Current);

                if (reqclass.CustomerRequest == true && isCustomerSet)
                {
                    if (requisition.CustomerID == null)
                    {
                        requisition.CustomerID = req.EmployeeID;
                        requisition.CustomerLocationID = req.LocationID;
                    }
                    else if (requisition.CustomerID != req.EmployeeID || requisition.CustomerLocationID != req.LocationID)
                    {
                        isCustomerSet = false;
                    }
                }
                else
                    isCustomerSet = false;

                if (isShipToSet)
                {
                    if (shipContactID == null && shipAddressID == null)
                    {
                        requisition.ShipDestType = req.ShipDestType;
						requisition.SiteID = req.SiteID;
						requisition.ShipToBAccountID = req.ShipToBAccountID;
                        requisition.ShipToLocationID = req.ShipToLocationID;
                        shipContactID = req.ShipContactID;
                        shipAddressID = req.ShipAddressID;
                    }
                    else if (requisition.ShipDestType != req.ShipDestType ||
							requisition.SiteID != req.SiteID ||
							requisition.ShipToBAccountID != req.ShipToBAccountID ||
                            requisition.ShipToLocationID != req.ShipToLocationID)
                    {
                        isShipToSet = false;
                    }
                }

                if (line.VendorID != null && line.VendorLocationID != null)
                {
                    VendorRef vendor = new VendorRef()
                    {
                        VendorID = line.VendorID.Value,
                        LocationID = line.VendorLocationID.Value
                    };

                    vendors.Add(vendor);

                    if (isVendorSet)
                    {
                        if (requisition.VendorID == null)
                        {
                            requisition.VendorID = line.VendorID;
                            requisition.VendorLocationID = line.VendorLocationID;
                        }
                        else if (requisition.VendorID != line.VendorID ||
                                 requisition.VendorLocationID != line.VendorLocationID)
                            isVendorSet = false;
                    }
                }
                else
                    isVendorSet = false;

                if (!isCustomerSet)
                {
                    requisition.CustomerID = null;
                    requisition.CustomerLocationID = null;
                }

                if (!isVendorSet)
                {
                    requisition.VendorID = null;
                    requisition.VendorLocationID = null;
                    requisition.RemitAddressID = null;
                    requisition.RemitContactID = null;
                }
                else if (requisition.VendorID == req.VendorID && requisition.VendorLocationID == req.VendorLocationID)
                {
                    requisition.RemitAddressID = req.RemitAddressID;
                    requisition.RemitContactID = req.RemitContactID;
                }

                if (!isShipToSet)
                {
                    requisition.ShipDestType = PX.Objects.PO.POShippingDestination.CompanyLocation;
                    graph.Document.Cache.SetDefaultExt<RQRequisition.shipToBAccountID>(requisition);
                }

                graph.Document.Update(requisition);

                if (line.OpenQty > 0)
                {
                    if (!graph.Lines.Cache.IsDirty && req.CuryID != requisition.CuryID)
                    {
                        requisition = PXCache<RQRequisition>.CreateCopy(graph.Document.Current);
                        requisition.CuryID = req.CuryID;
                        graph.Document.Update(requisition);
                    }

                    graph.InsertRequestLine(line, line.OpenQty.GetValueOrDefault(), filter.AddExists == true);
                }
            }

            if (isShipToSet)
            {
                foreach (PXResult<POAddress, POContact> res in
                            PXSelectJoin<POAddress, 
                    	        CrossJoin<POContact>,
                    	        Where<POAddress.addressID, Equal<Required<RQRequisition.shipAddressID>>,
                    		        And<POContact.contactID, Equal<Required<RQRequisition.shipContactID>>>>>
                    	        .Select(graph, shipAddressID, shipContactID))
                {
                    AddressAttribute.CopyRecord<RQRequisition.shipAddressID>(graph.Document.Cache, graph.Document.Current, (POAddress)res, true);
                    AddressAttribute.CopyRecord<RQRequisition.shipContactID>(graph.Document.Cache, graph.Document.Current, (POContact)res, true);
                }
            }

            if (requisition.VendorID == null && vendors.Count > 0)
            {
                foreach (var vendor in vendors)
                {
                    RQBiddingVendor bid = PXCache<RQBiddingVendor>.CreateCopy(graph.Vendors.Insert());
                    bid.VendorID = vendor.VendorID;
                    bid.VendorLocationID = vendor.LocationID;
                    graph.Vendors.Update(bid);
                }
            }

            if (graph.Lines.Cache.IsDirty)
            {
                graph.Save.Press();
                throw new PXRedirectRequiredException(graph, string.Format(Messages.RequisitionCreated, graph.Document.Current.ReqNbr));
            }

            for (int i = 0; i < lines.Count; i++)
            {
                PXProcessing<RQRequestLine>.SetInfo(i, PXMessages.LocalizeFormatNoPrefixNLA(Messages.RequisitionCreated, graph.Document.Current.ReqNbr));
            }
        }		
	}

	[Serializable]
	public partial class RQRequestSelection : IBqlTable
	{
		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.BQL.BqlGuid.Field<currentOwnerID> { }

		[PXDBGuid]
		[CR.CRCurrentOwnerID]
		public virtual Guid? CurrentOwnerID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		protected Guid? _OwnerID;
		[PXDBGuid]
		[PXUIField(DisplayName = "Assigned To")]
		[PX.TM.PXSubordinateOwnerSelector]
		public virtual Guid? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region MyOwner
		public abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }
		protected Boolean? _MyOwner;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner
		{
			get
			{
				return _MyOwner;
			}
			set
			{
				_MyOwner = value;
			}
		}
		#endregion
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region MyWorkGroup
		public abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }
		protected Boolean? _MyWorkGroup;
		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup
		{
			get
			{
				return _MyWorkGroup;
			}
			set
			{
				_MyWorkGroup = value;
			}
		}
		#endregion
		#region MyEscalated
		public abstract class myEscalated : PX.Data.BQL.BqlBool.Field<myEscalated> { }
		protected Boolean? _MyEscalated;
		[PXDefault(true)]
		[PXDBBool]
		[PXUIField(DisplayName = "Display Escalated", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyEscalated
		{
			get
			{
				return _MyEscalated;
			}
			set
			{
				_MyEscalated = value;
			}
		}
		#endregion
		#region FilterSet
		public abstract class filterSet : PX.Data.BQL.BqlBool.Field<filterSet> { }
		[PXDefault(false)]
		[PXDBBool]
		public virtual bool? FilterSet
		{
			get
			{
				return
					this.OwnerID != null ||
					this.WorkGroupID != null ||
					this.MyWorkGroup == true ||
					this.MyEscalated == true;
			}
		}
		#endregion
		#region ReqClassID
		public abstract class reqClassID : PX.Data.BQL.BqlString.Field<reqClassID> { }
		protected string _ReqClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Request Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(RQRequestClass.reqClassID), DescriptionField = typeof(RQRequestClass.descr))]
		public virtual string ReqClassID
		{
			get
			{
				return this._ReqClassID;
			}
			set
			{
				this._ReqClassID = value;
			}
		}
		#endregion
		#region SelectedPriority
		public abstract class selectedPriority : PX.Data.BQL.BqlInt.Field<selectedPriority> { }
		protected Int32? _SelectedPriority;
		[PXDBInt]
		[PXDefault(-1)]
		[PXIntList(new int[] { -1, 0, 1, 2 },
			new string[] { "All", "Low", "Normal", "High" })]
		[PXUIField(DisplayName = "Priority")]
		public virtual Int32? SelectedPriority
		{
			get
			{
				return this._SelectedPriority;
			}
			set
			{
				this._SelectedPriority = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[VendorNonEmployeeActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
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
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;
		[PXDBInt]
		[RQRequesterSelector(typeof(RQRequestSelection.reqClassID))]
		[PXUIField(DisplayName = "Requested By", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? EmployeeID
		{
			get
			{
				return this._EmployeeID;
			}
			set
			{
				this._EmployeeID = value;
			}
		}
		#endregion
		#region DepartmentID
		public abstract class departmentID : PX.Data.BQL.BqlString.Field<departmentID> { }
		protected String _DepartmentID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
		[PXUIField(DisplayName = "Department", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String DepartmentID
		{
			get
			{
				return this._DepartmentID;
			}
			set
			{
				this._DepartmentID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected int? _LocationID;
		[PXDefault(typeof(Search<BAccountR.defLocationID, Where<BAccountR.bAccountID, Equal<Current<RQRequestSelection.employeeID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<RQRequestSelection.employeeID>>>),
													DescriptionField = typeof(Location.descr),
													Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
		[PXFormula(typeof(Default<RQRequestSelection.employeeID>))]
		public int? LocationID
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
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
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
		#region DescriptionWildcard
		public abstract class descriptionWildcard : PX.Data.BQL.BqlString.Field<descriptionWildcard> { }
		protected String _DescriptionWildcard;
		[PXDBString(60, IsUnicode = true)]
		public virtual String DescriptionWildcard
		{
			get
			{
				return this._Description != null ? "%" + this._Description + "%" : null;
			}
		}
		#endregion
		#region AddExists
		public abstract class addExists : PX.Data.BQL.BqlBool.Field<addExists> { }
		protected Boolean? _AddExists;
		[PXDBBool()]
		[PXUIField(DisplayName = "Merge Lines", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false, typeof(Search<RQSetup.requisitionMergeLines>))]
		public virtual Boolean? AddExists
		{
			get
			{
				return this._AddExists;
			}
			set
			{
				this._AddExists = value;
			}
		}
		#endregion
	}
}
