using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.PO
{
    [Serializable]
	[PXCacheName(Messages.POLandedCostReceipt)]
	public class POLandedCostReceipt : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<POLandedCostReceipt>.By<lCDocType, lCRefNbr, pOReceiptType, pOReceiptNbr>
		{
			public static POLandedCostReceipt Find(PXGraph graph, string lCDocType, string lCRefNbr, string pOReceiptType, string pOReceiptNbr)
				=> FindBy(graph, lCDocType, lCRefNbr, pOReceiptType, pOReceiptNbr);
		}
		public static class FK
		{
			public class LandedCostDoc : POLandedCostDoc.PK.ForeignKeyOf<POLandedCostReceipt>.By<lCDocType, lCRefNbr> { }
			public class Receipt : POReceipt.PK.ForeignKeyOf<POLandedCostReceipt>.By<pOReceiptNbr> { }
		}
		#endregion

		#region LCDocType
		public abstract class lCDocType : PX.Data.BQL.BqlString.Field<lCDocType> { }

		[POLandedCostDocType.List()]
		[PXDBString(1, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(POLandedCostDoc.docType))]
		[PXUIField(DisplayName = "Landed Cost Type", Visible = false)]
		public virtual String LCDocType
		{
			get;
			set;
		}
		#endregion
		#region LCRefNbr
		public abstract class lCRefNbr : PX.Data.BQL.BqlString.Field<lCRefNbr> { }

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(POLandedCostDoc.refNbr))]
		[PXUIField(DisplayName = "Landed Cost Nbr.")]
		[PXParent(typeof(FK.LandedCostDoc))]
		public virtual String LCRefNbr
		{
			get;
			set;
		}
		#endregion

		#region POReceiptType
		public abstract class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }

		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "PO Receipt Type", Visible = false)]
		public virtual String POReceiptType
		{
			get;
			set;
		}
		#endregion
		#region POReceiptNbr
		public abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "PO Receipt Nbr.")]
		[PXSelector(typeof(Search<POReceipt.receiptNbr, Where<POReceipt.receiptType, Equal<Current<pOReceiptType>>>>))]
		public virtual String POReceiptNbr
		{
			get;
			set;
		}
		#endregion

		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		[PXInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
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
	}
}
