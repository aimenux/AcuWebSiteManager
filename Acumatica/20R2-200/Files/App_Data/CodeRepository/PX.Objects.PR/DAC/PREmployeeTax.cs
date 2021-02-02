using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PREmployeeTax)]
	[Serializable]
	public class PREmployeeTax : IBqlTable
	{
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PREmployee.bAccountID))]
		[PXParent(typeof(Select<PREmployee, Where<PREmployee.bAccountID, Equal<Current<PREmployeeTax.bAccountID>>>>))]
		public virtual int? BAccountID { get; set; }
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlInt.Field<taxID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Tax Code")]
		[PXSelector(typeof(PRTaxCode.taxID), DescriptionField = typeof(PRTaxCode.description), SubstituteKey = typeof(PRTaxCode.taxCD))]
		public virtual int? TaxID { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public virtual bool? IsActive { get; set; }
		#endregion

		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		[PXString]
		[PXUIField(DisplayName = "State", Visible = false)]
		[PXFormula(typeof(Selector<PREmployeeTax.taxID, PRTaxCode.taxState>))]
		public virtual string State { get; set; }
		#endregion
		#region ErrorLevel
		public abstract class errorLevel : PX.Data.BQL.BqlInt.Field<errorLevel> { }
		[PXInt]
		public virtual int? ErrorLevel { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public abstract class tStamp : IBqlField { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : IBqlField { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}