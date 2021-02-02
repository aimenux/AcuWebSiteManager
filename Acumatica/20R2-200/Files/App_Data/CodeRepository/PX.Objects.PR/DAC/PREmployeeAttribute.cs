using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using PX.Payroll.Data;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PREmployeeAttribute)]
	[Serializable]
	public class PREmployeeAttribute : IBqlTable, IPRSetting, IStateSpecific
	{
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PREmployee.bAccountID))]
		[PXParent(typeof(Select<PREmployee, Where<PREmployee.bAccountID, Equal<Current<PREmployeeAttribute.bAccountID>>>>))]
		public virtual int? BAccountID { get; set; }
		#endregion
		#region TypeName
		public abstract class typeName : PX.Data.BQL.BqlString.Field<typeName> { }
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Type", Visible = false, Enabled = false)]
		[PXDefault(typeof(PRTaxCode.typeName))]
		public virtual string TypeName { get; set; }
		#endregion
		#region SettingName
		public abstract class settingName : PX.Data.BQL.BqlString.Field<settingName> { }
		[PXDBString(255, IsKey = true, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Setting", Visible = false, Enabled = false)]
		[PXSelector(typeof(SearchFor<PRCompanyTaxAttribute.settingName>))]
		public virtual string SettingName { get; set; }
		#endregion
		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		[PXDBString(3, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "State", Visible = true, Enabled = false)]
		public virtual string State { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string Description { get; set; }
		#endregion
		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Value")]
		[PXUIEnabled(typeof(Where<PREmployeeAttribute.useDefault, Equal<False>>))]
		public virtual string Value { get; set; }
		#endregion
		#region UseDefault
		public abstract class useDefault : PX.Data.BQL.BqlBool.Field<useDefault> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Default")]
		public virtual bool? UseDefault { get; set; }
		#endregion
		#region AllowOverride
		public abstract class allowOverride : PX.Data.BQL.BqlBool.Field<allowOverride> { }
		[PXBool]
		[PXUIField(DisplayName = "Allow Employee Override", Enabled = false, Visible = false)]
		public virtual bool? AllowOverride { get; set; }
		#endregion
		#region IsFederal
		public abstract class isFederal : PX.Data.BQL.BqlBool.Field<isFederal> { }
		[PXBool]
		[PXUIField(DisplayName = "Is Federal Attribute")]
		public virtual bool? IsFederal
		{
			[PXDependsOnFields(typeof(PREmployeeAttribute.state))]
			get
			{
				return State == LocationConstants.FederalStateCode;
			}
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		[PXInt]
		[PXUIField(DisplayName = "Sort Order")]
		public virtual int? SortOrder { get; set; }
		#endregion
		#region Required
		public abstract class required : PX.Data.BQL.BqlBool.Field<required> { }
		[PXBool]
		[PXUIField(DisplayName = "Required", Enabled = false, Visible = false)]
		public virtual bool? Required { get; set; }
		#endregion
		#region AatrixMapping
		public abstract class aatrixMapping : PX.Data.BQL.BqlInt.Field<aatrixMapping> { }
		[PXDBInt]
		[PXUIField(Visible = false)]
		public virtual int? AatrixMapping { get; set; }
		#endregion
		#region ErrorLevel
		public abstract class errorLevel : PX.Data.BQL.BqlInt.Field<errorLevel> { }
		[PXInt]
		public virtual int? ErrorLevel { get; set; }
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