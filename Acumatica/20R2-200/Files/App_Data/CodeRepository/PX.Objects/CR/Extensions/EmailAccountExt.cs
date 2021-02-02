using System;
using PX.Data;
using PX.Objects.EP;
using PX.SM;
using PX.TM;

namespace PX.Objects.CR
{
	[Serializable]
	public partial class EMailAccountExt : PXCacheExtension<EMailAccount>
	{
		#region EmailAccountID
		public abstract class emailAccountID : PX.Data.BQL.BqlInt.Field<emailAccountID> { }
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Email Account ID")]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.defContactID, Equal<Current<EMailAccount.defaultOwnerID>>>>))]
		[PXEMailAccountIDSelector]
		public virtual Int32? EmailAccountID { get; set; }
		#endregion

		#region DefaultWorkgroupID
		public abstract class defaultWorkgroupID : PX.Data.BQL.BqlInt.Field<defaultWorkgroupID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Default Email Workgroup")]
		[PXCompanyTreeSelector]
		public virtual int? DefaultWorkgroupID { get; set; }
		#endregion

		#region DefaultOwnerID
		public abstract class defaultOwnerID : PX.Data.BQL.BqlInt.Field<defaultOwnerID> { }

		[Owner(typeof(EMailAccount.defaultWorkgroupID), DisplayName = "Default Email Owner")]
		[PXDefault(typeof(Search<EPCompanyTreeMember.contactID, Where<EPCompanyTreeMember.contactID, Equal<Current<AccessInfo.contactID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? DefaultOwnerID { get; set; }
		#endregion

		#region DefaultEmailAssignmentMapID
		public abstract class defaultEmailAssignmentMapID : PX.Data.BQL.BqlInt.Field<defaultEmailAssignmentMapID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Email Assignment Map")]
		[PXSelector(typeof(Search<EPAssignmentMap.assignmentMapID,
			Where<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeActivity>,
				Or<EPAssignmentMap.entityType, Equal<AssignmentMapType.AssignmentMapTypeEmail>>>>))]
		public virtual int? DefaultEmailAssignmentMapID { get; set; }
		#endregion

        #region CreateCase
        public abstract class createCase : PX.Data.BQL.BqlBool.Field<createCase> { }
        [PXDBBool]
        [PXUIField(DisplayName = "Create New Case")]
        public virtual bool? CreateCase { get; set; }
        #endregion
        #region CreateCaseClassID
        public abstract class createCaseClassID : PX.Data.BQL.BqlString.Field<createCaseClassID> { }
        [PXDBString(10)]
        [PXUIField(DisplayName = "New Case Class")]
        [PXSelector(typeof(CRCaseClass.caseClassID), 
			DescriptionField = typeof(CRCaseClass.description), 
			CacheGlobal = true)]
        [PXUIEnabled(typeof(EMailAccountExt.createCase))]
        public virtual string CreateCaseClassID { get; set; }
        #endregion        
        #region CreateLead
        public abstract class createLead : PX.Data.BQL.BqlBool.Field<createLead> { }
        [PXDBBool]
        [PXUIField(DisplayName = "Create New Lead")]
        public virtual bool? CreateLead { get; set; }
        #endregion
        #region CreateLeadClassID
        public abstract class createLeadClassID : PX.Data.BQL.BqlString.Field<createLeadClassID> { }
        [PXDBString(10)]
        [PXUIField(DisplayName = "New Lead Class")]
        [PXSelector(typeof(CRLeadClass.classID), DescriptionField = typeof(CRLeadClass.description), CacheGlobal = true)]		
        [PXUIEnabled(typeof(EMailAccountExt.createLead))]
        public virtual string CreateLeadClassID { get; set; }
        #endregion
	}
}
