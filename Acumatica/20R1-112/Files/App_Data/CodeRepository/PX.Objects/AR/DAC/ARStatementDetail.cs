using System;

using PX.Data;

using PX.Objects.GL;
using PX.Objects.CM;

namespace PX.Objects.AR
{
	/// <summary>
	/// Represents a document that has been included into a <see cref="ARStatement">Customer Statement</see>.
	/// The entities of this type are created by the Prepare Statements (AR503000) process and
	/// can be seen on the Customer Statement (AR641500) and Customer Statement MC (AR642000) reports.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.ARStatementDetail)]
	public partial class ARStatementDetail : IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		/// <summary>
		/// The integer identifier of the <see cref="Branch"/> of the <see cref="ARStatement">
		/// Customer Statement</see>, to which the detail belongs. This field is part of the compound 
		/// key of the statement detail, and part of the foreign key referencing the 
		/// <see cref="ARStatement">Customer Statement</see> record. 
		/// Corresponds to the <see cref="ARStatement.BranchID"/> field.
		/// </summary>
		[Branch]
        public virtual int? BranchID
        {
			get;
			set;
        }
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		/// <summary>
		/// The integer identifier of the <see cref="Customer"/> of the <see cref="ARStatement">
		/// Customer Statement</see>, to which the detail belongs. This field is part of the compound
		/// key of the statement detail, and part of the foreign key referencing the
		/// <see cref="ARStatement">Customer Statement</see> record.
		/// Corresponds to the <see cref="ARStatement.CustomerID"/> field.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(ARStatement.customerID))]
		[PXUIField(DisplayName = "Customer ID")]
		[PXParent(typeof(Select<
			ARStatement,
			Where<
				ARStatement.customerID, Equal<Current<ARStatementDetail.customerID>>,
				And<ARStatement.statementDate, Equal<Current<ARStatementDetail.statementDate>>,
				And<ARStatement.curyID, Equal<Current<ARStatementDetail.curyID>>>>>>))]
		public virtual int? CustomerID
		{
			get;
			set;
		}
		#endregion
		#region StatementDate
		public abstract class statementDate : PX.Data.BQL.BqlDateTime.Field<statementDate> { }
		/// <summary>
		/// The date of the <see cref="ARStatement">Customer Statement</see>, to which
		/// the detail belongs. This field is part of the compound key of the statement
		/// detail, and part of the foreign key referencing the <see cref="ARStatement">
		/// Customer Statement</see> record.
		/// Corresponds to the <see cref="ARStatement.StatementDate"/> field.
		/// </summary>
		[PXDBDate(IsKey = true)]
		[PXDefault(typeof(ARStatement.statementDate))]
		[PXUIField(DisplayName = "Statement Date")]
		public virtual DateTime? StatementDate
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		/// <summary>
		/// The currency of the <see cref="ARStatement">Customer Statement</see>, to
		/// which the detail belongs. This field is part of the compound key of the
		/// statement detail, and part of the foreign key referencing the <see cref="ARStatement">
		/// Customer Statement</see> record.
		/// Corresponds to the <see cref="ARStatement.StatementDate"/> field.
		/// </summary>
		[PXDBString(5, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(ARStatement.curyID))]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency ID")]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		/// <summary>
		/// The type of the document, for which the statement detail is created.
		/// This field is part of the compound key of the statement detail,
		/// and part of the foreign key referencing the <see cref="ARRegister"/>
		/// document. Corresponds to the <see cref="ARRegister.DocType"/> field.
		/// </summary>
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "DocType")]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		/// <summary>
		/// The reference number of the document, for which the statement
		/// detail is created. This field is part of the compound key of
		/// the statement detail, and part of the foreign key referencing
		/// the <see cref="ARRegister"/> document. Corresponds to the
		/// <see cref="ARRegister.RefNbr"/> field.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Ref. Nbr.")]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region DocBalance
		public abstract class docBalance : PX.Data.BQL.BqlDecimal.Field<docBalance> { }
		/// <summary>
		/// Indicates the balance, in base currency, that the document
		/// has on the statement date.
		/// </summary>
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Doc. Balance")]
		public virtual decimal? DocBalance
		{
			get;
			set;
		}
		#endregion
		#region CuryDocBalance
		public abstract class curyDocBalance : PX.Data.BQL.BqlDecimal.Field<curyDocBalance> { }
		/// <summary>
		/// Indicates the balance, in document currency, that the document
		/// has on the statement date.
		/// </summary>
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Cury. Doc. Balance")]
		public virtual decimal? CuryDocBalance
		{
			get;
			set;
		}
		#endregion
		#region IsOpen
		public abstract class isOpen : PX.Data.BQL.BqlBool.Field<isOpen> { }
		/// <summary>
		/// If set to <c>true</c>, indicates that the document
		/// is open on the statement date.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		public virtual bool? IsOpen
		{
			get;
			set;
		}
		#endregion
	}
}