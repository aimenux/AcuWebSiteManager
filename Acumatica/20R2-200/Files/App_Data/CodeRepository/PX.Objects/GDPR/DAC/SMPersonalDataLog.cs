using System;
using PX.Data;
using PX.SM;

namespace PX.Objects.GDPR
{
	[Serializable]
	public class SMPersonalDataLog : IBqlTable
	{
		#region UIKey
		public abstract class uIKey : PX.Data.BQL.BqlString.Field<uIKey> { }

		[PXString]
		[PXUIField(DisplayName = "Key")]
		public virtual string UIKey
		{
			get { return CombinedKey; }
		}
		#endregion

		#region UIKey
		public abstract class logID : PX.Data.BQL.BqlInt.Field<logID> { }

		[PXDBIdentity(IsKey = true)]
		public virtual int? LogID { get; set; }
		#endregion

		#region CombinedKey
		public abstract class combinedKey : PX.Data.BQL.BqlString.Field<combinedKey> { }

		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Key")]
		public virtual string CombinedKey { get; set; }
		#endregion

		#region TableName
		public abstract class tableName : PX.Data.BQL.BqlString.Field<tableName> { }

		[PXDBString]
		[PXUIField(DisplayName = "Entity")]
		public virtual string TableName { get; set; }
		#endregion
		
		#region PseudonymizationStatus
		public abstract class pseudonymizationStatus : PX.Data.BQL.BqlInt.Field<pseudonymizationStatus> { }

		[PXPseudonymizationStatusField]
		[PXUIField(DisplayName = "Set Status", Visible = true)]
		public virtual int? PseudonymizationStatus { get; set; }
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		//[PXSelector(typeof(Search<Users.pKID>), new Type[] { typeof(Users.username), typeof(Users.fullName) }, DescriptionField = typeof(Users.displayName))]
		[PXDBCreatedByID]
		[PXUIField(DisplayName = "By User")]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = "On", Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
	}
}