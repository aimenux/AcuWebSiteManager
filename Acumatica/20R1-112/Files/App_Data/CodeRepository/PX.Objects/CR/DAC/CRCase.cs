using System;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CR.Workflows;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.TM;

namespace PX.Objects.CR
{
	/// <exclude/>
	[Serializable]
	[PXPrimaryGraph(typeof(CRCaseMaint))]
	[PXCacheName(Messages.Case)]
	[CREmailContactsView(typeof(Select2<Contact,
		LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
		Where<Contact.bAccountID, Equal<Optional<CRCase.customerID>>,
		   Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>))]
	[PXEMailSource]//NOTE: for assignment map
	public partial class CRCase : IBqlTable, IAssign, IPXSelectable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected { get; set; }
		#endregion

		#region CaseCD
		public abstract class caseCD : PX.Data.BQL.BqlString.Field<caseCD> { }

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Case ID", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(CRSetup.caseNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(Search2<CRCase.caseCD,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRCase.customerID>>>, 
			Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>,
			OrderBy<Desc<CRCase.caseCD>>>),
			typeof(CRCase.caseCD),
			typeof(CRCase.subject),
			typeof(CRCase.status),
			typeof(CRCase.priority),
			typeof(CRCase.severity),
			typeof(CRCase.caseClassID),
			typeof(CRCase.isActive),
			typeof(BAccount.acctName),
			Filterable = true)]
		[PXFieldDescription]
		public virtual String CaseCD { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime(InputMask = "g")]
		[PXUIField(DisplayName = "Date Reported", Enabled = false)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region CaseClassID
		public abstract class caseClassID : PX.Data.BQL.BqlString.Field<caseClassID> { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault(typeof(Search<CRSetup.defaultCaseClassID>))]
		[PXUIField(DisplayName = "Class ID")]
		[PXSelector(typeof(CRCaseClass.caseClassID), 
			DescriptionField = typeof(CRCaseClass.description), 
			CacheGlobal = true)]
		[PXMassUpdatableField]
		public virtual String CaseClassID { get; set; }
		#endregion

		#region Subject
		public abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }

		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Subject { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		protected String _Description;
		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
				_plainText = null;
			}
		}
		#endregion

		#region DescriptionAsPlainText
		public abstract class descriptionAsPlainText : PX.Data.BQL.BqlString.Field<descriptionAsPlainText> { }

		private string _plainText;
		[PXString(IsUnicode = true)]
		[PXUIField(Visible = false)]
		[PXDependsOnFields(typeof(description))]
		public virtual String DescriptionAsPlainText
		{
			get
			{
				return _plainText ?? (_plainText = PX.Data.Search.SearchService.Html2PlainText(this.Description));
			}
		}
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		public class Value<Field>:IBqlOperand
			where Field : PX.Data.IBqlField { }
		[CustomerAndProspect(DisplayName = "Business Account")]
		[PXRestrictor(typeof(Where<Current<CRCaseClass.requireCustomer>, Equal<False>,
  		Or<BAccount.type, Equal<BAccountType.customerType>,
			Or<BAccount.type, Equal<BAccountType.combinedType>>>>), Messages.CustomerRequired, typeof(BAccount.acctCD))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Switch<Case<Where<CRCase.caseClassID, IsNotNull, And<Selector<CRCase.caseClassID, CRCaseClass.requireCustomer>, Equal<True>,
                                         And<Current<CRCase.customerID>, IsNotNull, And<Selector<Current<CRCase.customerID>, BAccount.type>, Equal<BAccountType.prospectType>>>>>,
                                         Null>,
                             CRCase.customerID>))]
		[PXFormula(typeof(Switch<Case<Where<Current<CRCase.customerID>, IsNull, And<Current<CRCase.contractID>, IsNotNull>>, 
								IsNull<Selector<CRCase.contractID, Selector<ContractBillingSchedule.accountID, BAccount.acctCD>>,
											 Selector<CRCase.contractID, Selector<Contract.customerID, BAccount.acctCD>>>>, 
							 CRCase.customerID>))]
		[PXFormula(typeof(Switch<Case<Where<Current<CRCase.customerID>, IsNull, 
														 And<Current<CRCase.contactID>, IsNotNull, 
														 And<Selector<CRCase.contactID, Contact.bAccountID>, IsNotNull>>>, 
								Selector<CRCase.contactID, Selector<Contact.bAccountID, BAccount.acctCD>>>, 
							CRCase.customerID>))]
		public virtual Int32? CustomerID { get; set; }
		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<CRCase.customerID>>>), DescriptionField = typeof(Location.descr))]
		[PXFormula(typeof(Switch<
							Case<Where<Current<CRCase.locationID>, IsNull, And<Current<CRCase.contractID>, IsNotNull>>,
									IsNull<Selector<CRCase.contractID, Selector<ContractBillingSchedule.locationID, Location.locationCD>>,
												 Selector<CRCase.contractID, Selector<Contract.locationID, Location.locationCD>>>,
						  Case<Where<Current<CRCase.locationID>, IsNull, And<Current<CRCase.customerID>, IsNotNull>>,
									 Selector<CRCase.customerID, Selector<BAccount.defLocationID, Location.locationCD>>,
							Case<Where<Current<CRCase.customerID>, IsNull>, Null>>>,
						  CRCase.locationID>))]
        [PXFormula(typeof(Default<CRCase.customerID>))]
		public virtual Int32? LocationID { get; set; }
		#endregion

		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Contract")]
		[PXSelector(typeof(Search2<Contract.contractID,
				LeftJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>>,
			Where<Contract.baseType, Equal<CTPRType.contract>,
				And<Where<Current<CRCase.customerID>, IsNull,
						Or2<Where<Contract.customerID, Equal<Current<CRCase.customerID>>,
							And<Current<CRCase.locationID>, IsNull>>,
						Or2<Where<ContractBillingSchedule.accountID, Equal<Current<CRCase.customerID>>,
							And<Current<CRCase.locationID>, IsNull>>,
						Or2<Where<Contract.customerID, Equal<Current<CRCase.customerID>>,
							And<Contract.locationID, Equal<Current<CRCase.locationID>>>>,
						Or<Where<ContractBillingSchedule.accountID, Equal<Current<CRCase.customerID>>,
							And<ContractBillingSchedule.locationID, Equal<Current<CRCase.locationID>>>>>>>>>>>,
			OrderBy<Desc<Contract.contractCD>>>),
			DescriptionField = typeof(Contract.description),
			SubstituteKey = typeof(Contract.contractCD), Filterable = true)]
		[PXRestrictor(typeof(Where<Contract.status, Equal<Contract.status.active>>), Messages.ContractIsNotActive)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, LessEqual<Contract.graceDate>, Or<Contract.expireDate, IsNull>>), Messages.ContractExpired)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, GreaterEqual<Contract.startDate>>), Messages.ContractActivationDateInFuture, typeof(Contract.startDate))]		
		[PXFormula(typeof(Default<CRCase.customerID>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? ContractID { get; set; }

		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(SelectFrom<Contact>
				.LeftJoin<BAccount>
					.On<BAccount.bAccountID.IsEqual<Contact.bAccountID>>
				.Where<
					Brackets<
						BAccount.bAccountID.IsNull
						.Or<Match<BAccount, Current<AccessInfo.userName>>>

					>
					.And<
						Brackets<
							CRCase.customerID.FromCurrent.IsNull
							.Or<BAccount.bAccountID.IsEqual<CRCase.customerID.FromCurrent>>
						>
					>
					.And<Contact.contactType.IsNotEqual<ContactTypesAttribute.lead>>
				>
				.SearchFor<Contact.contactID>),
			DescriptionField = typeof(Contact.displayName), Filterable = true, DirtyRead = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<CRCase.customerID>))]
		[PXRestrictor(typeof(Where<Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>,
				And<WhereEqualNotNull<BAccount.bAccountID, CRCase.customerID>>>), Messages.ContactBAccountDiff)]
		[PXRestrictor(typeof(Where<Contact.isActive, Equal<True>>), Messages.ContactInactive, typeof(Contact.displayName))]		
        [PXRestrictor(typeof(Where<Current<CRCaseClass.requireCustomer>, Equal<False>,
			Or<BAccount.type, Equal<BAccountType.customerType>,
			Or<BAccount.type, Equal<BAccountType.combinedType>>>>), Messages.CustomerRequired, typeof(BAccount.acctCD))]
		public virtual Int32? ContactID { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXDBString(1, IsFixed = true)]
		[CaseWorkflow.States.List]
		[PXUIField(DisplayName = "Status")]
		[PXChildUpdatable(UpdateRequest = true)]
		public virtual string Status { get; set; }
		#endregion

		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion

		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Released", Enabled = false)]
		public virtual Boolean? Released { get; set; }
		#endregion

		#region Resolution
		public abstract class resolution : PX.Data.BQL.BqlString.Field<resolution> { }

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Reason")]
		[PXChildUpdatable]
		[PXMassUpdatableField]
		[PXStringList(new string[0], new string[0])]
		public virtual string Resolution { get; set; }
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		[PXDBInt]
		[PXChildUpdatable(UpdateRequest = true)]
		[PXUIField(DisplayName = "Workgroup")]
		[PXCompanyTreeSelector]
		[PXMassUpdatableField]
		public virtual Int32? WorkgroupID { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

		[PXDBGuid()]
		[PXOwnerSelector(typeof(CRCase.workgroupID))]
		[PXChildUpdatable(AutoRefresh = true, TextField = "AcctName", ShowHint = false)]
		[PXUIField(DisplayName = "Owner")]
		[PXMassUpdatableField]
		public virtual Guid? OwnerID { get; set; }
		#endregion

		#region AssignDate
		public abstract class assignDate : PX.Data.BQL.BqlDateTime.Field<assignDate> { }

		private DateTime? _assignDate;
		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Assignment Date")]
		public virtual DateTime? AssignDate
		{
			get { return _assignDate ?? CreatedDateTime; }
			set { _assignDate  = value; }
		}

		#endregion

		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Source")]
		[CaseSources]
		public virtual String Source { get; set; }
		#endregion

		#region Severity
		public abstract class severity : PX.Data.BQL.BqlString.Field<severity> { }

		[PXDBString(1, IsFixed = true)]
        [PXDefault(CRCaseSeverityAttribute._MEDIUM)]
		[PXUIField(DisplayName = "Severity")]
        [CRCaseSeverity()]
		public virtual String Severity { get; set; }
		#endregion

		#region Priority
		public abstract class priority : PX.Data.BQL.BqlString.Field<priority> { }

		[PXDBString(1, IsFixed = true)]
        [PXDefault(CRCasePriorityAttribute._MEDIUM)]
		[PXUIField(DisplayName = "Priority")]
        [CRCasePriority()]
		public virtual String Priority { get; set; }
		#endregion

		#region SLAETA
		public abstract class sLAETA : PX.Data.BQL.BqlDateTime.Field<sLAETA> { }

		[PXDBDate(PreserveTime = true, DisplayMask = "g")]
		[PXUIField(DisplayName = "SLA")]
		[PXFormula(typeof(Default<CRCase.contractID, CRCase.severity, CRCase.caseClassID>))]
		public virtual DateTime? SLAETA { get; set; }
		#endregion

		#region TimeEstimated
		public abstract class timeEstimated : PX.Data.BQL.BqlInt.Field<timeEstimated> { }

		[PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		[PXUIField(DisplayName = "Estimation")]
		public virtual Int32? TimeEstimated { get; set; }
		#endregion

		#region ETA
		public abstract class eTA : PX.Data.BQL.BqlDateTime.Field<eTA> { }

		[PXDate(DisplayMask = "g")]
		[PXUIField(DisplayName = "Promised")]
		public virtual DateTime? ETA
		{
			get
			{
				if (CreatedDateTime == null || TimeEstimated == null)
					return null;
				return ((DateTime)CreatedDateTime).AddMinutes((int)TimeEstimated);
			}
		}
		#endregion

		#region RemaininingDate
		public abstract class remaininingDate : PX.Data.BQL.BqlInt.Field<remaininingDate> { }
		[PXDependsOnFields(typeof(eTA), typeof(createdDateTime), typeof(timeEstimated))]
		[PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		[PXUIField(DisplayName = "Remaining")]
		public virtual Int32? RemaininingDate
		{
			get
			{
				var eta = ETA	;
				if (eta == null) return null;
				return ((DateTime)eta).Minute - PXTimeZoneInfo.Now.Minute;
			}
		}
		#endregion


		#region RemaininingDateMinutes
		public abstract class remaininingDateMinutes : PX.Data.BQL.BqlInt.Field<remaininingDateMinutes> { }
		[PXInt]
		[PXUIField(DisplayName = "Remaining (minutes)", Enabled = false, Visible = false)]
		public virtual Int32? RemaininingDateMinutes
		{
			get { return RemaininingDate; }
		}
		#endregion
		
		#region LastActivity
		public abstract class lastActivity : PX.Data.BQL.BqlDateTime.Field<lastActivity> { }
		[PXDate(InputMask = "g")]
		[PXUIField(DisplayName = "Last Activity Date", Enabled = false)]
		public virtual DateTime? LastActivity { get; set; }
		#endregion

		#region LastModified
		public abstract class lastModified : PX.Data.BQL.BqlDateTime.Field<lastModified> { }
		[PXDate(InputMask = "g")]
		[PXFormula(typeof(Switch<Case<Where<lastActivity, IsNotNull, And<lastModifiedDateTime, IsNull>>, lastActivity,
			Case<Where<lastModifiedDateTime, IsNotNull, And<lastActivity, IsNull>>, lastModifiedDateTime,
			Case<Where<lastActivity, Greater<lastModifiedDateTime>>, lastActivity>>>, lastModifiedDateTime>))]
		[PXUIField(DisplayName = "Last Modified", Enabled = false)]
		public virtual DateTime? LastModified { get; set; }
		#endregion
		
		#region InitResponse
		public abstract class initResponse : PX.Data.BQL.BqlInt.Field<initResponse> { }

		[CRTimeSpanCalced(typeof(Minus1<
			Search<CRActivity.startDate,
				Where<CRActivity.refNoteID, Equal<CRCase.noteID>,
					And2<Where<CRActivity.isPrivate, IsNull, Or<CRActivity.isPrivate, Equal<False>>>,
					And<CRActivity.ownerID, IsNotNull,
					And2<Where<CRActivity.incoming, IsNull, Or<CRActivity.incoming, Equal<False>>>,
					And<Where<CRActivity.isExternal, IsNull, Or<CRActivity.isExternal, Equal<False>>>>>>>>,
				OrderBy<Asc<CRActivity.startDate>>>,
			CRCase.createdDateTime>))]
		[PXUIField(DisplayName = "Init. Response", Enabled = false)]
		[PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		public virtual Int32? InitResponse { get; set; }

		#endregion

		#region InitResponseMinutes

		public abstract class initResponseMinutes : PX.Data.BQL.BqlInt.Field<initResponseMinutes> { }

		[PXInt]
		[PXUIField(DisplayName = "Init. Response (minutes)", Enabled = false, Visible = false)]
		public virtual Int32? InitResponseMinutes
		{
			get { return InitResponse; }
		}

		#endregion

		#region TimeSpent
		public abstract class timeSpent : PX.Data.BQL.BqlInt.Field<timeSpent> { }

		[PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		[PXUIField(DisplayName = "Time Spent", Enabled = false)]
		public virtual Int32? TimeSpent { get; set; }
		#endregion

		#region OvertimeSpent
		public abstract class overtimeSpent : PX.Data.BQL.BqlInt.Field<overtimeSpent> { }

		[PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		[PXUIField(DisplayName = "Overtime Spent", Enabled = false)]
		public virtual Int32? OvertimeSpent { get; set; }
		#endregion

		#region IsBillable
		public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Billable", FieldClass = "BILLABLE")]
		[PXFormula(typeof(Selector<CRCase.caseClassID, CRCaseClass.isBillable>))]
		public virtual Boolean? IsBillable { get; set; }
		#endregion

		#region ManualBillableTimes

		public abstract class manualBillableTimes : PX.Data.BQL.BqlBool.Field<manualBillableTimes> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Manual Override", FieldClass = "BILLABLE")]
		[PXFormula(typeof(Switch<Case<Where<Selector<CRCase.caseClassID, CRCaseClass.perItemBilling>, Equal<BillingTypeListAttribute.perActivity>>, False>, CRCase.manualBillableTimes>))]
		public virtual Boolean? ManualBillableTimes { get; set; }

		#endregion

		#region TimeBillable

		public abstract class timeBillable : PX.Data.BQL.BqlInt.Field<timeBillable> { }
		protected int? _timeBillable;

		[PXDBInt]
		[PXUIField(DisplayName = "Billable Time", Enabled = false, FieldClass = "BILLABLE")]
		public virtual Int32? TimeBillable
		{
			get { return _timeBillable; }
			set { _timeBillable = value; }
		}

		#endregion

		#region OvertimeBillable

		public abstract class overtimeBillable : PX.Data.BQL.BqlInt.Field<overtimeBillable> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Billable Overtime", FieldClass = "BILLABLE")]
		public virtual Int32? OvertimeBillable { get; set; }

		#endregion

		#region ResolutionDate
		public abstract class resolutionDate : PX.Data.BQL.BqlDateTime.Field<resolutionDate> { }

		[PXDBDate(PreserveTime = true, DisplayMask = "g")]
		[PXUIField(DisplayName = "Closing Date", Enabled = false)]
		public virtual DateTime? ResolutionDate { get; set; }
		#endregion

		#region TimeResolution
		public abstract class timeResolution : PX.Data.BQL.BqlInt.Field<timeResolution> { }

		[PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		[PXUIField(DisplayName = "Resolution Time", Enabled = false)]
		public virtual int? TimeResolution
		{
			[PXDependsOnFields(typeof(resolutionDate), typeof(createdDateTime))]
			get
			{
				if (CreatedDateTime == null)
					return null;

				// only for open cases to show how much time passed sinse case is in work
				// don't need to check it it import scenario, so not need to add to PXDependsOnFields
				DateTime? startTime = IsActive == false || Released == true
					? ResolutionDate
					: PXTimeZoneInfo.Now;

				if (startTime == null)
					return null;

				return (int)(startTime.Value - CreatedDateTime.Value).TotalMinutes;
			}
		}
		#endregion

		#region TimeResolutionMinutes
		public abstract class timeResolutionMinutes : PX.Data.BQL.BqlInt.Field<timeResolutionMinutes> { }
		[PXInt]
		[PXUIField(DisplayName = "Resolution (minutes)", Enabled = false, Visible = false)]
		public virtual Int32? TimeResolutionMinutes
		{
			get { return TimeResolution; }
		}
		#endregion

		#region ARRefNbr
		public abstract class aRRefNbr : PX.Data.BQL.BqlString.Field<aRRefNbr> { }

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "AR Reference Nbr.")]
		[PXSelector(typeof(Search<ARInvoice.refNbr>))]
		public virtual String ARRefNbr { get; set; }
		#endregion

		#region Attributes
		public abstract class attributes : BqlAttributes.Field<attributes> { }

		[CRAttributesField(typeof(CRCase.caseClassID))]
		public virtual string[] Attributes { get; set; }
		#endregion

		public string ClassID
		{
			get { return CaseClassID; }
		}

		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Billing Date")]
		public virtual DateTime? Date { get; set; }
		#endregion

        #region Age

        public abstract class age : PX.Data.BQL.BqlInt.Field<age> { }

        [PXTimeSpanLong(InputMask = "#### ds ## hrs ## mins")]
        [PXFormula(typeof(Sub<PXDateAndTimeAttribute.now, CRCase.createdDateTime>))]
        [PXUIField(DisplayName = "Age")]
        public virtual Int32? Age { get; set; }

        #endregion

        #region LastActivityAge

        public abstract class lastActivityAge : PX.Data.BQL.BqlInt.Field<lastActivityAge> { }

        [PXTimeSpanLong(InputMask = "#### ds ## hrs ## mins")]
        [PXFormula(typeof(Sub<PXDateAndTimeAttribute.now, CRCase.lastActivity>))]
        [PXUIField(DisplayName = "Last Activity Age")]
        public virtual Int32? LastActivityAge { get; set; }

        #endregion

		#region StatusDate
		public abstract class statusDate : PX.Data.BQL.BqlDateTime.Field<statusDate> { }

		[PXDBLastChangeDateTime(typeof(CRCase.status))]
		public virtual DateTime? StatusDate { get; set; }
		#endregion

		#region StatusRevision
		public abstract class statusRevision : PX.Data.BQL.BqlInt.Field<statusRevision> { }

		[PXDBRevision(typeof(CRCase.status))]
		public virtual int? StatusRevision { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXSearchable(SM.SearchCategory.CR, Messages.CaseSearchTitle, new Type[] { typeof(CRCase.caseCD), typeof(CRCase.customerID), typeof(BAccount.acctName) },
			new Type[] { typeof(CRCase.contactID), typeof(Contact.firstName), typeof(Contact.lastName), typeof(Contact.eMail), 
				typeof(CRCase.ownerID), typeof(EP.EPEmployee.acctCD),typeof(EP.EPEmployee.acctName),typeof(CRCase.subject) },
			NumberFields = new Type[] { typeof(CRCase.caseCD) },
			MatchWithJoin = typeof(LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRCase.customerID>>>),
			 Line1Format = "{1}{3}{4}", Line1Fields = new Type[] { typeof(CRCase.caseClassID), typeof(CRCaseClass.description), typeof(CRCase.contactID), typeof(Contact.fullName), typeof(CRCase.status) },
			 Line2Format = "{0}", Line2Fields = new Type[] { typeof(CRCase.subject) }
		 )]
		[PXNote(
			DescriptionField = typeof(CRCase.caseCD),
			Selector = typeof(CRCase.caseCD),
			ShowInReferenceSelector = true)]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;


		[PXDBLastModifiedDateTime(InputMask = "g")]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}
}
