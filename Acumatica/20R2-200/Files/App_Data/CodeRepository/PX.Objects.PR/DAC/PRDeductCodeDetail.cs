using System;
using PX.Data;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRDeductCodeBeforeTaxes)]
	[Serializable]
	public class PRDeductCodeDetail : IBqlTable
	{
		#region CodeID
		public abstract class codeID : IBqlField { }
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(PRDeductCode.codeID))]
		[PXUIField(DisplayName = "Code ID")]
		[PXParent(typeof(Select<PRDeductCode, Where<PRDeductCode.codeID, Equal<Current<PRDeductCodeDetail.codeID>>>>))]
		public int? CodeID { get; set; }
		#endregion
		#region TaxID
		public abstract class taxID : IBqlField { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Tax ID")]
		[PXSelector(typeof(PRTaxCode.taxID), SubstituteKey = typeof(PRTaxCode.taxCD), DescriptionField = typeof(PRTaxCode.description))]
		public int? TaxID { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
