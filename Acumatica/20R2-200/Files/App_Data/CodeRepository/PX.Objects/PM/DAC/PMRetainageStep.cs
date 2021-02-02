using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.PM
{
	/// <summary>
	/// Stepped Retainage Detail Line
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName("Retainage Step")]
	[Serializable]
	public class PMRetainageStep : PX.Data.IBqlTable
	{
		#region ProjectID
		/// <exclude/>
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		/// <summary>
		/// Project
		/// </summary>
		[PXDefault]
		[PXDBInt(IsKey = true)]
		public virtual Int32? ProjectID
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		/// <exclude/>
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		/// <summary>
		/// Line Number
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(PMProject))]
		[PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<PMRetainageStep.projectID>>>>))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual Int32? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region ThresholdPct
		/// <exclude/>
		public abstract class thresholdPct : PX.Data.BQL.BqlDecimal.Field<thresholdPct> { }
		/// <summary>
		/// Threshold %. Contract completion (%) on reaching which the value of the default Retainage (%) will change to the figure specified in the Retainage (%) column of this record
		/// </summary>
		[PXDBDecimal(2)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Threshold (%)")]
		public virtual Decimal? ThresholdPct
		{
			get;
			set;

		}
		#endregion
		#region RetainagePct
		/// <exclude/>
		public abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct> { }
		/// <summary>
		/// Retainage (%) - new retainage (%) to be applied to the project revenue budget lines in accordance with the retainage mode used
		/// </summary>
		[PXDBDecimal(2)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retainage (%)")]
		public virtual Decimal? RetainagePct
		{
			get;
			set;

		}
		#endregion


		#region System Columns
		#region NoteID
		/// <exclude/>
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		/// <summary>
		/// Note
		/// </summary>
		[PXNote]
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
		#region tstamp
		/// <exclude/>
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		/// <summary>
		/// Timestamp
		/// </summary>
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
		/// <exclude/>
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		/// <summary>
		/// Created By User ID
		/// </summary>
		[PXDBCreatedByID]
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
		/// <exclude/>
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		/// <summary>
		/// Created By Screen ID
		/// </summary>
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
		/// <exclude/>
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		/// <summary>
		/// Created Date and Time
		/// </summary>
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
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
		/// <exclude/>
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		/// <summary>
		/// Last Modified By User ID
		/// </summary>
		[PXDBLastModifiedByID]
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
		/// <exclude/>
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		/// <summary>
		/// Last Modified By Screen ID
		/// </summary>
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
		/// <exclude/>
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		/// <summary>
		/// Last Modified By date and time
		/// </summary>
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
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
		#endregion

	}
}
