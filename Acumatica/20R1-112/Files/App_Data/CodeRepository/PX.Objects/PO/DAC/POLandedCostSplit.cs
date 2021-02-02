using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;

namespace PX.Objects.PO
{
    [Serializable]
	[PXHidden]
	public class POLandedCostSplit : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POLandedCostSplit>.By<docType, refNbr, receiptLineNbr, detailLineNbr>
		{
			public static POLandedCostSplit Find(PXGraph graph, string docType, string refNbr, int? receiptLineNbr, int? detailLineNbr)
				=> FindBy(graph, docType, refNbr, receiptLineNbr, detailLineNbr);
		}
		public static class FK
		{
			public class LandedCostDoc : POLandedCostDoc.PK.ForeignKeyOf<POLandedCostSplit>.By<docType, refNbr> { }
			public class LandedCostReceiptLine : POLandedCostReceiptLine.PK.ForeignKeyOf<POLandedCostSplit>.By<docType, refNbr, receiptLineNbr> { }
			public class LandedCostDetail : POLandedCostDetail.PK.ForeignKeyOf<POLandedCostSplit>.By<docType, refNbr, detailLineNbr> { }
		}
		#endregion

		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		/// <summary>
		/// [key] The type of the landed cost receipt line.
		/// </summary>
		/// <value>
		/// The field is determined by the type of the parent <see cref="POLandedCostDoc">document</see>.
		/// For the list of possible values see <see cref="POLandedCostDoc.DocType"/>.
		/// </value>
		[APDocType.List()]
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(POLandedCostDoc.docType))]
		[PXUIField(DisplayName = "Doc. Type", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String DocType
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		/// <summary>
		/// [key] Reference number of the parent <see cref="POLandedCostDoc">document</see>.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(POLandedCostDoc.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(FK.LandedCostDoc))]
		public virtual String RefNbr
		{
			get;
			set;
		}
		#endregion
		#region DetailLineNbr
		public abstract class detailLineNbr : PX.Data.BQL.BqlInt.Field<detailLineNbr> { }

		/// <summary>
		/// [key] The number of the transaction line in the document.
		/// </summary>
		/// <value>
		/// Note that the sequence of line numbers of the transactions belonging to a single document may include gaps.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Detail Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int32? DetailLineNbr
		{
			get;
			set;
		}
		#endregion
		#region ReceiptLineNbr
		public abstract class receiptLineNbr : PX.Data.BQL.BqlInt.Field<receiptLineNbr> { }

		/// <summary>
		/// [key] The number of the transaction line in the document.
		/// </summary>
		/// <value>
		/// Note that the sequence of line numbers of the transactions belonging to a single document may include gaps.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Receipt Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int32? ReceiptLineNbr
		{
			get;
			set;
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong()]
		[CurrencyInfo(typeof(POLandedCostDoc.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion

		#region CuryLineAmt
		public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }

		[PXDBCurrency(typeof(curyInfoID), typeof(lineAmt), BaseCalc = false)]
		[PXUIField(DisplayName = "Ext. Cost")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryLineAmt
		{
			get;
			set;
		}
		#endregion
		#region LineAmt
		public abstract class lineAmt : PX.Data.BQL.BqlDecimal.Field<lineAmt> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LineAmt
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
	}
}
