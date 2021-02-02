using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PREarningTypeDetail)]
	[Serializable]
	public class PREarningTypeDetail : IBqlTable
	{
		#region TypeCD
		public abstract class typecd : IBqlField { }
		[PXDBString(2, IsKey = true, IsUnicode = true)]
		[PXDefault(typeof(EPEarningType.typeCD))]
		[PXUIField(DisplayName = "Earning Type Code")]
		public string TypeCD { get; set; }
		#endregion
		#region TaxID
		public abstract class taxID : IBqlField { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Tax Code")]
		[PXSelector(typeof(PRTaxCode.taxID), DescriptionField = typeof(PRTaxCode.description), SubstituteKey = typeof(PRTaxCode.taxCD))]
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
