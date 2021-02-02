using System;

using PX.Data;
using PX.Data.EP;

using PX.Objects.GL;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AR
{
	/// <summary>
	/// Represents a salesperson who can be associated with sales of stock and non-stock 
	/// items in the system. To associate a salesperson with a sale, a salesperson's identifier 
	/// is recorded in a sales order or invoice line (<see cref="SO.SOLine.SalesPersonID"/> and 
	/// <see cref="ARTran.SalesPersonID"/>). The entities of this type can be edited on the 
	/// Salespersons (AR205000) form, which corresponds to the <see cref="SalesPersonMaint"/> graph.
	/// </summary>
	/// <remarks>
	/// A salesperson can be matched to a company <see cref="EP.EPEmployee">employee
	/// </see> through its <see cref="EP.EPEmployee.SalesPersonID"/> field.
	/// </remarks>
	[Serializable]
	[PXCacheName(Messages.SalesPerson)]
	[PXPrimaryGraph(typeof(SalesPersonMaint))]
	public partial class SalesPerson : PX.Data.IBqlTable
	{
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;
		/// <summary>
		/// The unique integer identifier of the salesperson.
		/// This field is a surrogate identity field.
		/// </summary>
		[PXDBIdentity()]
		[PXUIField(DisplayName = "SalesPerson ID", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXReferentialIntegrityCheck]
		public virtual Int32? SalesPersonID
		{
			get
			{
				return this._SalesPersonID;
			}
			set
			{
				this._SalesPersonID = value;
			}
		}
		#endregion
		#region SalesPersonCD
		public abstract class salesPersonCD : PX.Data.BQL.BqlString.Field<salesPersonCD> { }
		protected String _SalesPersonCD;
		/// <summary>
		/// The unique identifier of the salesperson. 
		/// This field is the key field.
		/// </summary>
		[PXDefault()]
		[SalesPersonRaw(IsKey = true, Visibility = PXUIVisibility.SelectorVisible,DisplayName="Salesperson ID")]
		[PXFieldDescription]
		public virtual String SalesPersonCD
		{
			get
			{
				return this._SalesPersonCD;
			}
			set
			{
				this._SalesPersonCD = value;
			}
		}
		#endregion
		#region CommnPct
		public abstract class commnPct : PX.Data.BQL.BqlDecimal.Field<commnPct> { }
		protected Decimal? _CommnPct;
		/// <summary>
		/// The default commission percentage of the salesperson.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Default Commission %")]
		public virtual Decimal? CommnPct
		{
			get
			{
				return this._CommnPct;
			}
			set
			{
				this._CommnPct = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.BQL.BqlInt.Field<salesSubID> { }
		protected Int32? _SalesSubID;
		/// <summary>
		/// The default sales subaccount associated with the salesperson.
		/// The value of this field can be used to construct the <see 
		/// cref="ARTran.SubID">sales subaccount</see> in the invoice line that 
		/// references the salesperson according to the rules defined by <see 
		/// cref="ARSetup.SalesSubMask"/>.
		/// </summary>
		[SubAccount(DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
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
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		/// <summary>
		/// The name of the salesperson.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Name")]
		[PXFieldDescription]
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
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected bool? _IsActive;
		/// <summary>
		/// Indicates (if set to <c>true</c>) that the salesperson 
		/// is active and can be used for recording sales in 
		/// <see cref="ARTran">invoice lines</see> or <see cref="SO.SOLine">
		/// sales order lines</see>.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Is Active")]
		public virtual bool? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(DescriptionField = typeof(SalesPerson.salesPersonCD))]
		public virtual Guid? NoteID { get; set; }
		#endregion

#if ALLOW_EDIT_CONTACT
		#region CreateNewContact
		public abstract class createNewContact : PX.Data.BQL.BqlBool.Field<createNewContact> { }
		protected bool? _CreateNewContact;
		[PXBool()]
		[PXDefault(false,PersistingCheck= PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Create New Contact")]
		public virtual bool? CreateNewContact
		{
			get
			{
				return this._CreateNewContact;
			}
			set
			{
				this._CreateNewContact = value;
			}
		}
		#endregion
		#region HasContact
		public abstract class hasContact : PX.Data.BQL.BqlBool.Field<hasContact> { }
		protected bool? _HasContact;
		[PXBool()]
		[PXUIField(DisplayName = "Create New Contact")]
		public virtual bool? HasContact
		{
			get
			{
				return this._HasContact;
			}
			set
			{
				this._HasContact = value;
			}
		}
		#endregion

#endif
	}
}
