using System;
using PX.Data;
using PX.Objects.SO;

namespace PX.Objects.CR
{
	[Serializable]
	[PXCacheName(Messages.Relations)]
	public partial class CRRelation : IBqlTable
	{
		#region RelationID

		public abstract class relationID : PX.Data.BQL.BqlInt.Field<relationID> { }

		[PXDBIdentity(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual Int32? RelationID { get; set; }

		#endregion

		#region RefNoteID

		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

		[PXParent(typeof(Select<Contact, Where<Contact.noteID, Equal<Current<CRRelation.refNoteID>>>>))]
		[PXParent(typeof(Select<BAccount, Where<BAccount.noteID, Equal<Current<CRRelation.refNoteID>>>>))]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.noteID, Equal<Current<CRRelation.refNoteID>>>>))]
		[PXDBGuid]
		[PXDBDefault(null)]
		public virtual Guid? RefNoteID { get; set; }

		#endregion

		#region Role

		public abstract class role : PX.Data.BQL.BqlString.Field<role> { }

		[PXDBString(2)]
		[PXUIField(DisplayName = "Role")]
		[PXDefault]
		[CRRoleTypeList.List]
		public virtual String Role { get; set; }

		#endregion

		#region IsPrimary

		public abstract class isPrimary : PX.Data.BQL.BqlBool.Field<isPrimary> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Primary")]
		[PXDefault(typeof(False))]
		public virtual bool? IsPrimary { get; set; }

		#endregion

		#region TargetType

		public abstract class targetType : PX.Data.BQL.BqlString.Field<targetType> { }

		[PXDBString(40)]
		[PXUIField(DisplayName = "Type")]
		[CRRelationTypeListAttribure(typeof(CRRelation.role))]
		[PXFormula(typeof(Default<CRRelation.role>))]
		public virtual string TargetType { get; set; }

		#endregion

		#region TargetNoteID

		public abstract class targetNoteID : PX.Data.BQL.BqlGuid.Field<targetNoteID> { }

		[EntityIDSelector(typeof(CRRelation.targetType))]
		[PXDBGuid()]
		[PXUIField(DisplayName = "Document")]
		[PXFormula(typeof(Default<CRRelation.targetType>))]
		[PXUIEnabled(typeof(Where<CRRelation.role, NotEqual<CRRoleTypeList.referrer>,
							  And<CRRelation.role, NotEqual<CRRoleTypeList.supervisor>,
							  And<CRRelation.role, NotEqual<CRRoleTypeList.businessUser>,
							  And<CRRelation.role, NotEqual<CRRoleTypeList.decisionMaker>,
							  And<CRRelation.role, NotEqual<CRRoleTypeList.technicalExpert>,
							  And<CRRelation.role, NotEqual<CRRoleTypeList.supportEngineer>,
							  And<CRRelation.role, NotEqual<CRRoleTypeList.evaluator>,
							  And<CRRelation.role, NotEqual<CRRoleTypeList.licensee>>>>>>>>>))]
		public virtual Guid? TargetNoteID { get; set; }

		#endregion

		#region DocNoteID

		public abstract class docNoteID : PX.Data.BQL.BqlGuid.Field<docNoteID> { }

		[PXDBGuid()]						
		public virtual Guid? DocNoteID { get; set; }

		#endregion

		#region EntityID

		public abstract class entityID : PX.Data.BQL.BqlInt.Field<entityID> { }

		[PXDBInt]
		[PXSelector(typeof(Search<BAccount.bAccountID,
					Where2<Where<BAccount.type, Equal<BAccountType.prospectType>,
						Or<BAccount.type, Equal<BAccountType.customerType>,
						Or<BAccount.type, Equal<BAccountType.vendorType>,
						Or<BAccount.type, Equal<BAccountType.combinedType>>>>>,
					And<Match<Current<AccessInfo.userName>>>>>),
			new[]
			{
					typeof (BAccount.acctCD), typeof (BAccount.acctName), typeof (BAccount.classID), typeof(BAccount.type),
					typeof (BAccount.parentBAccountID), typeof (BAccount.acctReferenceNbr)
			},
			SubstituteKey = typeof(BAccount.acctCD),
			Filterable = true,
			DirtyRead = true)]
		[PXUIField(DisplayName = "Account")]
		[PXFormula(typeof(Default<CRRelation.targetNoteID>))]
		[CRRelationAccount]
		public virtual Int32? EntityID { get; set; }

		#endregion

		#region EntityCD

		public abstract class entityCD : PX.Data.BQL.BqlString.Field<entityCD> { }

		[PXString]
		[PXUIField(DisplayName = "Account/Employee", Enabled = false)]
		public virtual String EntityCD { get; set; }

		#endregion

		

		#region ContactID

		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Contact")]
		[PXFormula(typeof(Default<CRRelation.targetNoteID>))]
		[PXFormula(typeof(Default<CRRelation.entityID>))]
		[CRRelationContactSelector]
		public virtual Int32? ContactID { get; set; }

		#endregion

		#region AddToCC

		public abstract class addToCC : PX.Data.BQL.BqlBool.Field<addToCC> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Add to CC")]
		public virtual Boolean? AddToCC { get; set; }

		#endregion

		#region Name

		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }

		[PXString]
		[PXUIField(DisplayName = "Name", Enabled = false)]
		public virtual String Name { get; set; }

		#endregion

		#region ContactName

		public abstract class contactName : PX.Data.BQL.BqlString.Field<contactName> { }

		[PXString]
		[PXUIField(DisplayName = "Contact", Enabled = false)]
		public virtual String ContactName { get; set; }

		#endregion

		#region Email

		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }

		[PXString]
		[PXUIField(DisplayName = "Email", Enabled = false)]
		public virtual String Email { get; set; }

		#endregion
	}
}
