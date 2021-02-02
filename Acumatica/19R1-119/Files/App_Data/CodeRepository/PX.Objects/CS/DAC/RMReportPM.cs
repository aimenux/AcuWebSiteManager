using System;
using PX.Data;
using PX.CS;

namespace PX.Objects.CS
{
	[Serializable]
	public partial class RMReportPM : PXCacheExtension<RMReport>
	{
		//PM Specific:
		#region RequestStartAccountGroup
		public abstract class requestStartAccountGroup : PX.Data.BQL.BqlBool.Field<requestStartAccountGroup> { }
		protected bool? _RequestStartAccountGroup;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request")]
		public virtual bool? RequestStartAccountGroup
		{
			get
			{
				return this._RequestStartAccountGroup;
			}
			set
			{
				this._RequestStartAccountGroup = value;
			}
		}
		#endregion
		#region RequestEndAccountGroup
		public abstract class requestEndAccountGroup : PX.Data.BQL.BqlBool.Field<requestEndAccountGroup> { }
		protected bool? _RequestEndAccountGroup;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request")]
		public virtual bool? RequestEndAccountGroup
		{
			get
			{
				return this._RequestEndAccountGroup;
			}
			set
			{
				this._RequestEndAccountGroup = value;
			}
		}
		#endregion
		#region RequestStartProject
		public abstract class requestStartProject : PX.Data.BQL.BqlBool.Field<requestStartProject> { }
		protected bool? _RequestStartProject;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request")]
		public virtual bool? RequestStartProject
		{
			get
			{
				return this._RequestStartProject;
			}
			set
			{
				this._RequestStartProject = value;
			}
		}
		#endregion
		#region RequestEndProject
		public abstract class requestEndProject : PX.Data.BQL.BqlBool.Field<requestEndProject> { }
		protected bool? _RequestEndProject;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request")]
		public virtual bool? RequestEndProject
		{
			get
			{
				return this._RequestEndProject;
			}
			set
			{
				this._RequestEndProject = value;
			}
		}
		#endregion
		#region RequestStartProjectTask
		public abstract class requestStartProjectTask : PX.Data.BQL.BqlBool.Field<requestStartProjectTask> { }
		protected bool? _RequestStartProjectTask;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request")]
		public virtual bool? RequestStartProjectTask
		{
			get
			{
				return this._RequestStartProjectTask;
			}
			set
			{
				this._RequestStartProjectTask = value;
			}
		}
		#endregion
		#region RequestEndProjectTask
		public abstract class requestEndProjectTask : PX.Data.BQL.BqlBool.Field<requestEndProjectTask> { }
		protected bool? _RequestEndProjectTask;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request")]
		public virtual bool? RequestEndProjectTask
		{
			get
			{
				return this._RequestEndProjectTask;
			}
			set
			{
				this._RequestEndProjectTask = value;
			}
		}
		#endregion
		#region RequestStartInventory
		public abstract class requestStartInventory : PX.Data.BQL.BqlBool.Field<requestStartInventory> { }
		protected bool? _RequestStartInventory;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request")]
		public virtual bool? RequestStartInventory
		{
			get
			{
				return this._RequestStartInventory;
			}
			set
			{
				this._RequestStartInventory = value;
			}
		}
		#endregion
		#region RequestEndInventory
		public abstract class requestEndInventory : PX.Data.BQL.BqlBool.Field<requestEndInventory> { }
		protected bool? _RequestEndInventory;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request")]
		public virtual bool? RequestEndInventory
		{
			get
			{
				return this._RequestEndInventory;
			}
			set
			{
				this._RequestEndInventory = value;
			}
		}
		#endregion

		#region RequestStartPeriod
		public abstract class requestStartPeriod : PX.Data.BQL.BqlBool.Field<requestStartPeriod> { }
		protected bool? _RequestStartPeriod;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Request")]
		public virtual bool? RequestStartPeriod
		{
			get
			{
				return this._RequestStartPeriod;
			}
			set
			{
				this._RequestStartPeriod = value;
			}
		}
		#endregion
		#region RequestEndPeriod
		public abstract class requestEndPeriod : PX.Data.BQL.BqlShort.Field<requestEndPeriod> { }
		protected short? _RequestEndPeriod;
		[PXDBShort]
		[PXDefault((short)0)]
		[PXIntList(new int[] { 0, 1, 2 }, new string[] { "Not Set", "Request", "Use Start" })]
		[PXUIField(DisplayName = "Default")]
		public virtual short? RequestEndPeriod
		{
			get
			{
				return this._RequestEndPeriod;
			}
			set
			{
				this._RequestEndPeriod = value;
			}
		}
		#endregion

		#region Extra

		public abstract class subCD : PX.Data.BQL.BqlString.Field<subCD> { }

		[PXDimension(GL.SubAccountAttribute.DimensionName)]
		[PXSelector(typeof(GL.Sub.subCD))]
		[PXString]
		public string SubCD
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		public decimal? Amount
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public abstract class drilldown : PX.Data.BQL.BqlDecimal.Field<drilldown> { }
		public decimal? Drilldown
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion
	}
}
