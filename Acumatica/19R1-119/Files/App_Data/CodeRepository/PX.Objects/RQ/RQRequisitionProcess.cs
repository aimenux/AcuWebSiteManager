using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.EP;
using System.Collections;
using PX.Objects.GL;
using PX.TM;
using PX.Objects.AR;

namespace PX.Objects.RQ
{
	[PX.TM.OwnedEscalatedFilter.Projection(
			typeof(RQRequisitionProcess.RQRequisitionSelection),
			typeof(RQRequisition.workgroupID),
			typeof(RQRequisition.ownerID),
			typeof(RQRequisition.orderDate))]
	[Serializable]
	public partial class RQRequisitionOwned : RQRequisition
	{
		#region ReqNbr
		public new abstract class reqNbr : PX.Data.BQL.BqlString.Field<reqNbr> { }
		#endregion
		#region OrderDate
		public new abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
		#endregion
		#region Priority
		public new abstract class priority : PX.Data.BQL.BqlInt.Field<priority> { }
		#endregion
		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		#endregion
		#region Description
		public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		#endregion
		#region WorkgroupID
		public new abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		#endregion
		#region OwnerID
		public new abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		#endregion
		#region VendorID
		public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		#endregion
		#region VendorLocationID
		public new abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
		#endregion
		#region VendorRefNbr
		public new abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr> { }
		#endregion
	}

	[Serializable]
	[TableAndChartDashboardType]
	public class RQRequisitionProcess : PXGraph<RQRequisitionProcess>
	{
		public PXCancel<RQRequisitionSelection> Cancel;
		public PXAction<RQRequisitionSelection> details;
		public PXFilter<RQRequisitionSelection> Filter;
		public PXFilter<Vendor> Vendor;
		[PXFilterable]
		public RQRequisitionProcessing Records;

		public RQRequisitionProcess()
		{
			Records.SetSelected<RQRequisitionLine.selected>();
			Records.SetProcessCaption(IN.Messages.Process);
			Records.SetProcessAllCaption(IN.Messages.ProcessAll);
		}

		public class RQRequisitionProcessing : PXFilteredProcessingJoin<RQRequisitionOwned,					
				RQRequisitionSelection,
				LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<RQRequisitionOwned.customerID>>,
				LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<RQRequisitionOwned.vendorID>>>>,
				Where<Current<RQRequisitionSelection.action>, NotEqual<NullAction>,
					And2<Where<Current<RQRequisitionSelection.selectedPriority>, Equal<AllPriority>,
								 Or<RQRequisitionOwned.priority, Equal<Current<RQRequisitionSelection.selectedPriority>>>>,
			    And2<Where<Customer.bAccountID, IsNull,
								Or<Match<Customer, Current<AccessInfo.userName>>>>,
 					And<Where<Vendor.bAccountID, IsNull,
								 Or<Match<Vendor, Current<AccessInfo.userName>>>>>>>>>
		{
			public RQRequisitionProcessing(PXGraph graph)
				: base(graph)
			{
				base._OuterView.WhereAndCurrent<RQRequisitionSelection>(typeof(RQRequisitionSelection.ownerID).Name, typeof(RQRequisitionSelection.workGroupID).Name);
			}
			public RQRequisitionProcessing(PXGraph graph, Delegate handler)
				: base(graph, handler)
			{
				base._OuterView.WhereAndCurrent<RQRequisitionSelection>(typeof(RQRequisitionSelection.ownerID).Name, typeof(RQRequisitionSelection.workGroupID).Name);
			}
		}
		[Serializable]
		public partial class RQRequisitionSelection : IBqlTable
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
			#region Action
			public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
			protected string _Action;
			[PXAutomationMenu]
			public virtual string Action
			{
				get
				{
					return this._Action;
				}
				set
				{
					this._Action = value;
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
			[PXDBInt()]			
			[PXSubordinateSelector]
			[PXUIField(DisplayName = "Creator", Visibility = PXUIVisibility.SelectorVisible)]
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
		}

		
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXEditDetailButton]
		public virtual IEnumerable Details(PXAdapter adapter)
		{
			if (Records.Current != null && Filter.Current != null)
			{
				RQRequisitionEntry graph = PXGraph.CreateInstance<RQRequisitionEntry>();
				graph.Document.Current = graph.Document.Search<RQRequisition.reqNbr>(Records.Current.ReqNbr);
				throw new PXRedirectRequiredException(graph, true, AR.Messages.ViewDocument) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		protected virtual void RQRequisitionSelection_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RQRequisitionSelection o = (RQRequisitionSelection)e.Row;			
			if (o != null && !String.IsNullOrEmpty(o.Action))
			{
				Records.SetProcessTarget(null, null, null, o.Action);
			}
		}
		public class NullAction : PX.Data.BQL.BqlString.Constant<NullAction> { public NullAction() : base("<SELECT>") { } }
	}
}
