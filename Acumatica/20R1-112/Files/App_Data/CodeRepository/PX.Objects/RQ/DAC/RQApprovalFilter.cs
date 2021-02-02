using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.EP;
using PX.TM;

namespace PX.Objects.RQ
{
	[System.SerializableAttribute()]
    [PXHidden]
	public partial class RQApprovalFilter : IBqlTable
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

		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;
		[PXInt()]
		[PXUIField(DisplayName = "Creator", Visibility = PXUIVisibility.Visible)]
		[PXEPEmployeeSelector]
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
	}

	public class AllPriority : PX.Data.BQL.BqlInt.Constant<AllPriority>
	{
		public AllPriority() : base(-1) { }
	}
}
