using PX.Data.EP;

namespace PX.Objects.EP
{
	using System;
	using PX.Data;
	using System.Diagnostics;
	using PX.TM;
	using PX.Objects.EP;
	using PX.SM;

	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(EPAssignmentMaint))]
	[DebuggerDisplay("Name={Name} WorkgroupID={WorkgroupID} RouterID={RouterID}")]
	[PXCacheName(Messages.LegacyAssignmentRoute)]
	public partial class EPAssignmentRoute : PX.Data.IBqlTable
	{
		#region AssignmentRouteID
		public abstract class assignmentRouteID : PX.Data.BQL.BqlInt.Field<assignmentRouteID> { }
		protected Int32? _AssignmentRouteID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Route ID")]
		[PXSelector(typeof(Search<EPAssignmentRoute.assignmentRouteID,
		   Where<EPAssignmentRoute.assignmentMapID, Equal<Current<EPAssignmentMap.assignmentMapID>>>>), DescriptionField = typeof(EPAssignmentRoute.name))]
		[PXParent(typeof(Select<EPAssignmentRoute, Where<EPAssignmentRoute.assignmentRouteID, Equal<Current<EPAssignmentRoute.parent>>>>))]
		public virtual Int32? AssignmentRouteID
		{
			get
			{
				return this._AssignmentRouteID;
			}
			set
			{
				this._AssignmentRouteID = value;
			}
		}
		#endregion
		#region Parent
		public abstract class parent : PX.Data.BQL.BqlInt.Field<parent> { }
		protected Int32? _Parent;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(EPAssignmentRoute.assignmentRouteID))]
		public virtual Int32? Parent
		{
			get
			{
				return this._Parent;
			}
			set
			{
				this._Parent = value;
			}
		}
		#endregion
		#region AssignmentMapID
		public abstract class assignmentMapID : PX.Data.BQL.BqlInt.Field<assignmentMapID> { }
		protected Int32? _AssignmentMapID;
		[PXDBInt()]
		[PXDBDefault(typeof(EPAssignmentMap.assignmentMapID))]
		[PXParent(typeof(Select<EPAssignmentMap, Where<EPAssignmentMap.assignmentMapID, Equal<Current<EPAssignmentRoute.assignmentMapID>>>>))]
		public virtual Int32? AssignmentMapID
		{
			get
			{
				return this._AssignmentMapID;
			}
			set
			{
				this._AssignmentMapID = value;
			}
		}
		#endregion		
		#region RouterType
		public abstract class routerType : PX.Data.BQL.BqlString.Field<routerType> { }
		protected String _RouterType;
		[PXDBString(1, IsFixed = true)]
		[EPRouterType.List()]
		[PXUIField(DisplayName = "Type")]
		[PXDefault(EPRouterType.Workgroup)]
		public virtual String RouterType
		{
			get
			{
				return this._RouterType;
			}
			set
			{
				this._RouterType = value;
			}
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected Int32? _WorkgroupID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Assign to")]
		[PXCompanyTreeSelector]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<EPAssignmentRoute.routerType>))]
		public virtual Int32? WorkgroupID
		{
			get
			{
				return this._WorkgroupID;
			}
			set
			{
				this._WorkgroupID = value;
			}
		}
		#endregion	
		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		protected Guid? _OwnerID;
		[PXDBGuid()]
		[PXOwnerSelector(typeof(EPAssignmentRoute.workgroupID))]
		[PXChildUpdatable(AutoRefresh = true)]
		[PXUIField(DisplayName = "Assign to")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<EPAssignmentRoute.routerType>))]
		public virtual Guid? OwnerID
		{
			get
			{
				return this._OwnerID;
			}
			set
			{
				this._OwnerID = value;
			}
		}
		#endregion		
		#region OwnerSource
		public abstract class ownerSource : PX.Data.BQL.BqlString.Field<ownerSource> { }
		protected String _OwnerSource;
		[PXDBString(250)]
		[PXUIField(DisplayName = "Owner Source", Visibility = PXUIVisibility.Visible)]
		public virtual String OwnerSource
		{
			get
			{
				return this._OwnerSource;
			}
			set
			{
				this._OwnerSource = value;
			}
		}
		#endregion
		#region UseWorkgroupByOwner
		public abstract class useWorkgroupByOwner : PX.Data.BQL.BqlBool.Field<useWorkgroupByOwner> { }
		protected bool? _UseWorkgroupByOwner;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Workgroup By Owner")]
		public virtual bool? UseWorkgroupByOwner
		{
			get
			{
				return this._UseWorkgroupByOwner;
			}
			set
			{
				this._UseWorkgroupByOwner = value;
			}
		}
		#endregion
		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		protected String _Name;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(
			Coalesce<
			Search<EPAssignmentRoute.name, Where<EPAssignmentRoute.assignmentRouteID, Equal<Current<EPAssignmentRoute.assignmentRouteID>>>>, 
			Search<EPCompanyTree.description, 
			Where<EPCompanyTree.workGroupID, Equal<Current<EPAssignmentRoute.workgroupID>>>>>))]
		[PXFormula(typeof(Default<EPAssignmentRoute.workgroupID>))]
		public virtual String Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = string.IsNullOrEmpty(value) ? null : value;
			}
		}
		#endregion
		#region RouteID
		public abstract class routeID : PX.Data.BQL.BqlInt.Field<routeID> { }
		protected Int32? _RouteID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Jump to")]
		[PXDefault]
		[PXSelector(typeof(Search<EPAssignmentRoute.assignmentRouteID, Where<EPAssignmentRoute.routerType, Equal<EPRouterType.group>, And<EPAssignmentRoute.assignmentMapID, Equal<Current<EPAssignmentMap.assignmentMapID>>>>>), DescriptionField = typeof(EPAssignmentRoute.name))]		
		public virtual Int32? RouteID
		{
			get
			{
				return this._RouteID;
			}
			set
			{
				this._RouteID = value;
			}
		}
		#endregion
		#region Sequence
		public abstract class sequence : PX.Data.BQL.BqlInt.Field<sequence> { }
		protected Int32? _Sequence;
		[PXDBInt()]
		[PXUIField(DisplayName = "Seq.", Enabled = false)]
		public virtual Int32? Sequence
		{
			get
			{
				return this._Sequence;
			}
			set
			{
				this._Sequence = value;
			}
		}
		#endregion
		#region RuleType
		public abstract class ruleType : PX.Data.BQL.BqlString.Field<ruleType> { }
		protected String _RuleType;
		[PXDBString(1, IsFixed = true)]
		[RuleType.List()]
		[PXDefault(PX.Objects.EP.RuleType.AllTrue)]
		[PXUIField(DisplayName = "Rule Type")]
		public virtual String RuleType
		{
			get
			{
				return this._RuleType;
			}
			set
			{
				this._RuleType = value;
			}
		}
		#endregion
		#region WaitTime
		public abstract class waitTime : PX.Data.BQL.BqlInt.Field<waitTime> { }

		[PXDefault(0)]
		[PXUIField(DisplayName = "Wait Time", Visibility = PXUIVisibility.Visible)]
		[PXDBTimeSpanLong(Format = TimeSpanFormatType.DaysHoursMinites)]
		public virtual int? WaitTime { get; set; }
		#endregion

		#region Icon
		public abstract class icon : PX.Data.BQL.BqlString.Field<icon> { }
		protected String _Icon;
		[PXString(250)]
		public virtual String Icon
		{
			get
			{
				return this._Icon;
			}
			set
			{
				this._Icon = value;
			}
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
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
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
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
		[PXDBCreatedByScreenID()]
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
		[PXDBCreatedDateTime()]
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
		[PXDBLastModifiedByID()]
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
		[PXDBLastModifiedByScreenID()]
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
		[PXDBLastModifiedDateTime()]
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
	}

	public static class EPRouterType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] {Workgroup, Group, Router},
					new string[] {Messages.Assign, Messages.Group, Messages.Jump})
			{
				;
			}
		}

		public const string Workgroup = "W";
		public const string Router = "R";
		public const string Group = "G";

		public class router : PX.Data.BQL.BqlString.Constant<router>
		{
			public router() : base(Router)
			{
			}
		}
		public class group : PX.Data.BQL.BqlString.Constant<group>
		{
			public group() : base(Group)
			{
			}
		}
		public class workgroup : PX.Data.BQL.BqlString.Constant<workgroup>
		{
			public workgroup() : base(Group)
			{
			}
		}

}

	public static class RuleType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { AllTrue, AtleastOneConditionIsTrue, AtleastOneConditionIsFalse },
				new string[] { CR.Messages.AllTrue, CR.Messages.AtleastOneIsTrue, CR.Messages.AtleastOneIsFalse }) { ; }
		}
		public const string AllTrue = "A";
		public const string AtleastOneConditionIsTrue = "T";
		public const string AtleastOneConditionIsFalse = "F";
	}
}
