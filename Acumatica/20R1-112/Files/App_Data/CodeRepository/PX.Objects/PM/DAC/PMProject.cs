using System;
using PX.Data;
using PX.Objects.CT;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CM;
using System.Collections.Generic;
using System.Text;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.SQLTree;

namespace PX.Objects.PM
{
	/// <summary>
	/// Planned set of interrelated tasks to be executed over a fixed period and within certain cost and other limitations. 
	/// Each project consists of tasks that need to be completed to complete the project. 
	/// Project budget, profitability and balances are monitored in scope of account groups.
	/// </summary>
	[PXCacheName(Messages.Project)]
	[Serializable]
	[PXEMailSource]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMProject : Contract, IAssign, PX.SM.IIncludable
	{
		[Obsolete(Common.InternalMessages.ClassIsObsoleteAndWillBeRemoved2019R2)]
		public class ProjectBaseType : CTPRType.project { }

		#region ContractID
		public new abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

		/// <summary>
		/// Get or sets ProjectID
		/// </summary>
		[PXDBIdentity]
		[PXReferentialIntegrityCheck(CheckPoint = CheckPoint.OnPersisting)]
		[PXUIField(DisplayName = "Project ID")]
		public override Int32? ContractID
		{
			get
			{
				return this._ContractID;
			}
			set
			{
				this._ContractID = value;
			}
		}
		#endregion
		#region BaseType
		public new abstract class baseType : PX.Data.BQL.BqlString.Field<baseType> { }
		/// <summary>
		/// Gets or sets the type of the record. It can be either Contract or Project.
		/// The default value is Project.
		/// </summary>
		[PXDBString(1, IsFixed = true, IsKey = true)]
		[PXDefault(CTPRType.Project)]
		[PXUIField(DisplayName = "Base Type")]
		public override string BaseType
		{
			get => base.BaseType;
			set => base.BaseType = value;
		}
		#endregion
		#region ContractCD
		public new abstract class contractCD : PX.Data.BQL.BqlString.Field<contractCD> { }
		/// <summary>
		/// Gets or sets ProjectCD. 
		/// This is a segmented key and format is configured under segmented key maintenance screen in CS module.
		/// </summary>
		[PXDimensionSelector(ProjectAttribute.DimensionName,
			typeof(Search2<PMProject.contractCD,
						LeftJoin<Customer, On<Customer.bAccountID, Equal<PMProject.customerID>>,
						LeftJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<PMProject.contractID>>>>,
						Where<PMProject.baseType, Equal<CTPRType.project>,
						 And<PMProject.nonProject, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>>)
						, typeof(PMProject.contractCD), typeof(PMProject.contractCD), typeof(PMProject.description),
						typeof(PMProject.customerID), typeof(PMProject.customerID_Customer_acctName), typeof(PMProject.locationID), typeof(PMProject.status),
						typeof(PMProject.approverID), typeof(PMProject.startDate), typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate))]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Project ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override String ContractCD
		{
			get
			{
				return this._ContractCD;
			}
			set
			{
				this._ContractCD = value;
			}
		}
		#endregion
		#region Description
		public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		/// <summary>
		/// Gets or sets Project description
		/// </summary>
		[PXDBLocalizableString(255, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override String Description
		{
			get
			{
				return _Description;
			}
			set
			{
				_Description = value;
			}
		}
		#endregion
		#region OriginalContractID
		public new abstract class originalContractID : PX.Data.BQL.BqlInt.Field<originalContractID> { }
		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBInt()]
		public override Int32? OriginalContractID
		{
			get
			{
				return this._OriginalContractID;
			}
			set
			{
				this._OriginalContractID = value;
			}
		}
		#endregion
		#region MasterContractID
		public new abstract class masterContractID : PX.Data.BQL.BqlInt.Field<masterContractID> { }
		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBInt()]
		public override Int32? MasterContractID
		{
			get
			{
				return this._MasterContractID;
			}
			set
			{
				this._MasterContractID = value;
			}
		}
		#endregion
		#region CaseItemID
		public new abstract class caseItemID : PX.Data.BQL.BqlInt.Field<caseItemID> { }
		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBInt()]
		public override Int32? CaseItemID
		{
			get
			{
				return this._CaseItemID;
			}
			set
			{
				this._CaseItemID = value;
			}
		}
		#endregion
		
		#region BudgetLevel
		public abstract class budgetLevel : PX.Data.BQL.BqlString.Field<budgetLevel> { }
		[PXDBString(1, IsFixed = true)]
		[PMBudgetLevelList]
		[PXDefault(BudgetLevels.Task)]
		[PXUIField(DisplayName = "Revenue Budget Level")]
		public virtual String BudgetLevel { get; set; }
        #endregion
        #region CostBudgetLevel
        public abstract class costBudgetLevel : PX.Data.BQL.BqlString.Field<costBudgetLevel> { }
        [PXDBString(1, IsFixed = true)]
        [PMBudgetLevelList]
        [PXDefault(BudgetLevels.Task)]
        [PXUIField(DisplayName = "Cost Budget Level")]
        public virtual String CostBudgetLevel { get; set; }
        #endregion

        #region BudgetFinalized
        public abstract class budgetFinalized : PX.Data.BQL.BqlBool.Field<budgetFinalized> { }

		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? BudgetFinalized
		{
			get;set;
		}
		#endregion
		#region CustomerID
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		/// <summary>
		/// Gets or sets Customer for a Project. Project can be oof 2 types - internal and external.
		/// Internal projects are those that have this value = NULL and hense are not billable.
		/// </summary>
		[CustomerActive(DescriptionField = typeof(Customer.acctName))]
		public override Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region CustomerID_Customer_acctName
		public abstract class customerID_Customer_acctName : PX.Data.BQL.BqlString.Field<customerID_Customer_acctName> { }
		#endregion
		#region LocationID
		public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		/// <summary>
		/// Gets or sets the Customer Location.
		/// </summary>
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<customerID>>>), DisplayName = "Default Location", DescriptionField = typeof(Location.descr))]
		[PXDefault(typeof(Search<Customer.defLocationID, Where<Customer.bAccountID, Equal<Current<customerID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public override Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		[PXDefault]
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Project Currency", Required = true)]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.isFinancial, Equal<True>>>),
			typeof(CurrencyList.curyID),
			typeof(CurrencyList.description),
			CacheGlobal = true)]
		public override string CuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryIDCopy
		public abstract class curyIDCopy : PX.Data.BQL.BqlString.Field<curyIDCopy> { }
		[PXString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency Rate for Budget", IsReadOnly = true, FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		[PXSelector(typeof(CurrencyList.curyID))]
		public virtual string CuryIDCopy
		{
			get
			{
				return CuryID;
			}
			set
			{
				CuryID = value;
			}
		}
		#endregion
		#region RateTypeID
		public new abstract class rateTypeID : PX.Data.BQL.BqlString.Field<rateTypeID> { }
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "Currency Rate Type")]
		public override string RateTypeID
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		[CurrencyInfo(ModuleCode = BatchModule.PM)]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoIDCopy
		public abstract class curyInfoIDCopy : PX.Data.BQL.BqlLong.Field<curyInfoIDCopy> { }
		[PXLong]
		[CurrencyInfo(CuryIDField = nameof(CuryIDCopy), ModuleCode = BatchModule.PM)]
		public virtual long? CuryInfoIDCopy
		{
			get
			{
				return CuryInfoID;
			}
			set
			{
				CuryInfoID = value;
			}
		}
		#endregion

		#region DefaultAccountID
		public new abstract class defaultAccountID : PX.Data.BQL.BqlInt.Field<defaultAccountID> { }
		#endregion
		#region DefaultSubID
		public new abstract class defaultSubID : PX.Data.BQL.BqlInt.Field<defaultSubID> { }
		/// <summary>
		/// Default Sub Account Associated with the project. This Subaccount can be later used in allocation and billing rules.
		/// </summary>
		[PXDefault(typeof(Search<Location.cSalesSubID, Where<Location.bAccountID, Equal<Current<PMProject.customerID>>, And<Location.locationID, Equal<Current<PMProject.locationID>>>>>))]
		[SubAccount(DisplayName = "Default Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public override Int32? DefaultSubID
		{
			get
			{
				return this._DefaultSubID;
			}
			set
			{
				this._DefaultSubID = value;
			}
		}
		#endregion
        #region DefaultAccrualAccountID
        public new abstract class defaultAccrualAccountID : PX.Data.BQL.BqlInt.Field<defaultAccrualAccountID> { }
        #endregion
        #region DefaultAccrualSubID
        public new abstract class defaultAccrualSubID : PX.Data.BQL.BqlInt.Field<defaultAccrualSubID> { }
		/// <summary>
        /// Default Project Accrual Subaccount. Is used depending on the <see cref="PMSetup.ExpenseAccrualSubMask"/> mask setting.
		/// </summary>
        [SubAccount(DisplayName = "Accrual Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        public override Int32? DefaultAccrualSubID
        {
            get
            {
                return this._DefaultAccrualSubID;
            }
            set
            {
                this._DefaultAccrualSubID = value;
            }
        }
		#endregion
		#region DefaultBranchID
		public new abstract class defaultBranchID : PX.Data.BQL.BqlInt.Field<defaultBranchID> { }
		#endregion

		#region BillingID
		public new abstract class billingID : PX.Data.BQL.BqlString.Field<billingID> { }
		/// <summary>
		/// Gets or sets the Billing Rule <see cref="PMBilling"/> for the Project. 
		/// Billing rule is set at the <see cref="PMTask"/> level. Here its just a default value for the Tasks created under the given project.
		/// </summary>
        [PXSelector(typeof(Search<PMBilling.billingID, Where<PMBilling.isActive, Equal<True>>>), DescriptionField = typeof(PMBilling.description))]
        [PXForeignReference(typeof(Field<billingID>.IsRelatedTo<PMBilling.billingID>))]
		[PXUIField(DisplayName="Billing Rule")]
		[PXDBString(PMBilling.billingID.Length, IsUnicode = true)]
		public override String BillingID
		{
			get
			{
				return this._BillingID;
			}
			set
			{
				this._BillingID = value;
			}
		}
		#endregion
		#region BillAddressID
		public abstract class billAddressID : PX.Data.BQL.BqlInt.Field<billAddressID> { }
		protected Int32? _BillAddressID;

		/// <summary>
		/// The identifier of the <see cref="PMAddress">Billing Address object</see>, associated with the customer.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PMAddress.AddressID"/> field.
		/// </value>
		[PXDBInt()]
		[PMAddress(typeof(Select2<Customer,
			InnerJoin<CR.Standalone.Location, On<CR.Standalone.Location.bAccountID, Equal<Customer.bAccountID>, And<CR.Standalone.Location.locationID, Equal<Customer.defLocationID>>>,
			InnerJoin<Address, On<Address.bAccountID, Equal<Customer.bAccountID>, And<Address.addressID, Equal<Customer.defBillAddressID>>>,
			LeftJoin<PMAddress, On<PMAddress.customerID, Equal<Address.bAccountID>, And<PMAddress.customerAddressID, Equal<Address.addressID>, And<PMAddress.revisionID, Equal<Address.revisionID>, And<PMAddress.isDefaultBillAddress, Equal<True>>>>>>>>,
			Where<Customer.bAccountID, Equal<Current<PMProject.customerID>>>>), typeof(customerID))]
		public virtual Int32? BillAddressID
		{
			get
			{
				return this._BillAddressID;
			}
			set
			{
				this._BillAddressID = value;
			}
		}
		#endregion
		#region BillContactID
		public abstract class billContactID : PX.Data.BQL.BqlInt.Field<billContactID> { }

		/// <summary>
		/// The identifier of the <see cref="ARContact">Billing Contact object</see>, associated with the customer.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="ARContact.ContactID"/> field.
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(PMContact.contactID), ValidateValue = false)]    //Attribute for showing contact email field on Automatic Notifications screen in the list of availible emails for
																			//Invoices and Memos screen. Relies on the work of platform, which uses PXSelector to compose email list
		[PXUIField(DisplayName = "Billing Contact", Visible = false)]       //Attribute for displaying user friendly contact email field on Automatic Notifications screen in the list of availible emails.
		[PMContact(typeof(Select2<Customer,
							InnerJoin<
									  CR.Standalone.Location, On<CR.Standalone.Location.bAccountID, Equal<Customer.bAccountID>,
								  And<CR.Standalone.Location.locationID, Equal<Customer.defLocationID>>>,
							InnerJoin<
									  Contact, On<Contact.bAccountID, Equal<Customer.bAccountID>,
								  And<Contact.contactID, Equal<Customer.defBillContactID>>>,
							LeftJoin<
									 PMContact, On<PMContact.customerID, Equal<Contact.bAccountID>,
								 And<PMContact.customerContactID, Equal<Contact.contactID>,
								 And<PMContact.revisionID, Equal<Contact.revisionID>,
								 And<PMContact.isDefaultContact, Equal<True>>>>>>>>,
							Where<Customer.bAccountID, Equal<Current<PMProject.customerID>>>>), typeof(customerID))]
		public virtual int? BillContactID
		{
			get;
			set;
		}
		#endregion
		#region BillingCuryID
		public abstract class billingCuryID : PX.Data.BQL.BqlString.Field<billingCuryID> { }
		[PXDefault]
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Billing Currency")]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.isFinancial, Equal<True>>>),
			typeof(CurrencyList.curyID),
			typeof(CurrencyList.description),
			CacheGlobal = true)]
		public virtual string BillingCuryID
		{
			get;
			set;
		}
		#endregion
		
		#region AllocationID
		public new abstract class allocationID : PX.Data.BQL.BqlString.Field<allocationID> { }

		/// <summary>
		/// Gets or sets the Allocation Rule <see cref="PMAllocation"/> for the Project. 
		/// Allocation rule is set at the <see cref="PMTask"/> level. Here its just a default value for the Tasks created under the given project. 
		/// </summary>
		[PXForeignReference(typeof(Field<allocationID>.IsRelatedTo<PMAllocation.allocationID>))]
		[PXSelector(typeof(Search<PMAllocation.allocationID, Where<PMAllocation.isActive, Equal<True>>>), DescriptionField = typeof(PMAllocation.description))]
		[PXUIField(DisplayName = "Allocation Rule")]
		[PXDBString(PMAllocation.allocationID.Length, IsUnicode = true)]
		public override String AllocationID
		{
			get
			{
				return this._AllocationID;
			}
			set
			{
				this._AllocationID = value;
			}
		}
		#endregion
		#region TermsID
		public new abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		
		/// <summary>
		/// The identifier of the <see cref="Terms">Credit Terms</see> object associated with the document.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Customer.TermsID">credit terms</see> that are selected for the <see cref="CustomerID">customer</see>.
		/// Corresponds to the <see cref="Terms.TermsID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Customer.termsID, Where<Customer.bAccountID, Equal<Current<PMProject.customerID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms")]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		public override String TermsID
		{
			get;
			set;
		}
		#endregion
		#region ApproverID
		public new abstract class approverID : PX.Data.BQL.BqlInt.Field<approverID> { }
		/// <summary>
		/// Gets or Sets Project manager for the Project. Project manager can Approve and Reject activities that require approval. 
		/// Activity required approval if and only if the <see cref="PMTask.ApproverID"/> is set for a given <see cref="PMTask"/> 
		/// </summary>
		[PXDBInt]
		[PXEPEmployeeSelector]
		[PXForeignReference(typeof(Field<approverID>.IsRelatedTo<BAccount.bAccountID>))]
		[PXUIField(DisplayName = "Project Manager", Visibility = PXUIVisibility.SelectorVisible)]
		public override Int32? ApproverID
		{
			get
			{
				return this._ApproverID;
			}
			set
			{
				this._ApproverID = value;
			}
		}
		#endregion
		#region WorkgroupID
		public new abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		#endregion
		#region OwnerID
		public new abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		#endregion
        #region RateTableID
        public new abstract class rateTableID : PX.Data.BQL.BqlString.Field<rateTableID> { }

		/// <summary>
		/// Gets or sets the RateTable <see cref="PMRateTable"/> for the project. 
		/// </summary>
        [PXDBString(PMRateTable.rateTableID.Length, IsUnicode = true)]
        [PXUIField(DisplayName = "Rate Table")]
        [PXSelector(typeof(PMRateTable.rateTableID), DescriptionField = typeof(PMRateTable.description))]
		[PXForeignReference(typeof(Field<rateTableID>.IsRelatedTo<PMRateTable.rateTableID>))]
		public override String RateTableID
        {
            get
            {
                return this._RateTableID;
            }
            set
            {
                this._RateTableID = value;
            }
        }
        #endregion
		#region TemplateID
		public new abstract class templateID : PX.Data.BQL.BqlInt.Field<templateID> { }

		/// <summary>
		/// Gets or sets Template for the project.
		/// </summary>
		[PXUIField(DisplayName = "Template", Visibility = PXUIVisibility.Visible, FieldClass = ProjectAttribute.DimensionNameTemplate)]
		[PXDimensionSelector(ProjectAttribute.DimensionNameTemplate,
				typeof(Search2<PMProject.contractID,
						LeftJoin<ContractBillingSchedule, On<ContractBillingSchedule.contractID, Equal<PMProject.contractID>>>, 
							Where<PMProject.baseType, Equal<CT.CTPRType.projectTemplate>, And<PMProject.isActive, Equal<True>>>>), 
				typeof(PMProject.contractCD),
				typeof(PMProject.contractCD),
				typeof(PMProject.description),
				typeof(PMProject.budgetLevel),
				typeof(PMProject.billingID),
				typeof(ContractBillingSchedule.type),
				typeof(PMProject.approverID),
				DescriptionField = typeof(PMProject.description))]
		[PXDBInt]
		[PXForeignReference(typeof(Field<templateID>.IsRelatedTo<PMProject.contractID>))]
		public override Int32? TemplateID
        {
			get
			{
				return this._TemplateID;
			}
			set
			{
				this._TemplateID = value;
			}
		}
		#endregion
		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <summary>
		/// Gets or sets status of the Project. <see cref="ProjectStatus"/>
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[ProjectStatus.List()]
		[PXDefault(ProjectStatus.Planned)]
		[PXUIField(DisplayName = "Status", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		public override String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region Duration
		public new abstract class duration : PX.Data.BQL.BqlInt.Field<duration> { }

		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBInt()]
		public override Int32? Duration
		{
			get
			{
				return this._Duration;
			}
			set
			{
				this._Duration = value;
			}
		}
		#endregion
		#region DurationType
		public new abstract class durationType : PX.Data.BQL.BqlString.Field<durationType> { }
		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		public override string DurationType
		{
			get
			{
				return this._DurationType;
			}
			set
			{
				this._DurationType = value;
			}
		}
		#endregion
		#region StartDate
		public new abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		/// <summary>
		/// Gets or sets Start date of a project.
		/// </summary>		
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Start Date")]
		public override DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region ExpireDate
		public new abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

		/// <summary>
		/// Gets or sets End date of a project.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
		public override DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion
		#region GracePeriod
		public new abstract class gracePeriod : PX.Data.BQL.BqlInt.Field<gracePeriod> { }
		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(0)]
		public override Int32? GracePeriod
		{
			get
			{
				return this._GracePeriod;
			}
			set
			{
				this._GracePeriod = value;
			}
		}
		#endregion
		#region AutoRenew
		public new abstract class autoRenew : PX.Data.BQL.BqlBool.Field<autoRenew> { }
		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public override Boolean? AutoRenew
		{
			get
			{
				return this._AutoRenew;
			}
			set
			{
				this._AutoRenew = value;
			}
		}
		#endregion
		#region AutoRenewDays
		public new abstract class autoRenewDays : PX.Data.BQL.BqlInt.Field<autoRenewDays> { }
		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(0)]
		public override Int32? AutoRenewDays
		{
			get
			{
				return this._AutoRenewDays;
			}
			set
			{
				this._AutoRenewDays = value;
			}
		}
		#endregion
		#region IsTemplate
		[Obsolete(Common.InternalMessages.PropertyIsObsoleteAndWillBeRemoved2019R2)]
		public new abstract class isTemplate : PX.Data.BQL.BqlBool.Field<isTemplate> { }
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		protected String _ExtRefNbr;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "External Ref. Nbr")]
		public virtual String ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion

		public new abstract class certifiedJob : PX.Data.BQL.BqlBool.Field<certifiedJob> { }

		#region RestrictToEmployeeList
		public new abstract class restrictToEmployeeList : PX.Data.BQL.BqlBool.Field<restrictToEmployeeList> { }
		#endregion
		#region RestrictToResourceList
		public new abstract class restrictToResourceList : PX.Data.BQL.BqlBool.Field<restrictToResourceList> { }
		#endregion

		#region DetailedBilling
		public new abstract class detailedBilling : PX.Data.BQL.BqlInt.Field<detailedBilling> { }
		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBInt()]
		[PXDefault(Contract.detailedBilling.Summary)]
		public override Int32? DetailedBilling
		{
			get
			{
				return this._DetailedBilling;
			}
			set
			{
				this._DetailedBilling = value;
			}
		}
		#endregion
		#region AllowOverride
		public new abstract class allowOverride : PX.Data.BQL.BqlBool.Field<allowOverride> { }
		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public override Boolean? AllowOverride
		{
			get
			{
				return this._AllowOverride;
			}
			set
			{
				this._AllowOverride = value;
			}
		}
		#endregion
		#region RefreshOnRenewal
		public new abstract class refreshOnRenewal : PX.Data.BQL.BqlBool.Field<refreshOnRenewal> { }
		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public override Boolean? RefreshOnRenewal
		{
			get
			{
				return this._RefreshOnRenewal;
			}
			set
			{
				this._RefreshOnRenewal = value;
			}
		}
		#endregion
		#region IsContinuous
		public new abstract class isContinuous : PX.Data.BQL.BqlBool.Field<isContinuous> { }
		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public override Boolean? IsContinuous
		{
			get
			{
				return this._IsContinuous;
			}
			set
			{
				this._IsContinuous = value;
			}
		}
		#endregion
              
		#region IsActive
		public new abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		/// <summary>
		/// Gets or sets whether Project is active or not. Transactions can be added only to the active projects.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active", Enabled=false, Visible=false, Visibility=PXUIVisibility.Visible)]
		public override Boolean? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region IsCompleted
		public new abstract class isCompleted : PX.Data.BQL.BqlBool.Field<isCompleted> { }

		/// <summary>
		/// Gets or sets whether project is Completed or not.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Completed", Enabled = false, Visible = false, Visibility = PXUIVisibility.Visible)]
		public override Boolean? IsCompleted
		{
			get
			{
				return this._IsCompleted;
			}
			set
			{
				this._IsCompleted = value;
			}
		}
		#endregion
        #region AutoAllocate
        public new abstract class autoAllocate : PX.Data.BQL.BqlBool.Field<autoAllocate> { }

		/// <summary>
		/// Gets or sets whether to run allocation everytime a <see cref="PMTran"/> is released.
		/// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Run Allocation on Release of Project Transactions")]
        public override Boolean? AutoAllocate
        {
            get
            {
                return this._AutoAllocate;
            }
            set
            {
                this._AutoAllocate = value;
            }
        }
        #endregion

		#region VisibleInGL
		public new abstract class visibleInGL : PX.Data.BQL.BqlBool.Field<visibleInGL> { }

		/// <summary>
		/// Gets or sets whether the Project is visible in the GL Module.
		/// If Project is set as invisible - it will not show up in the field selectors in the given module.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInGL>))]
		[PXUIField(DisplayName = "GL")]
		public override Boolean? VisibleInGL
		{
			get
			{
				return this._VisibleInGL;
			}
			set
			{
				this._VisibleInGL = value;
			}
		}
		#endregion
		#region VisibleInAP
		public new abstract class visibleInAP : PX.Data.BQL.BqlBool.Field<visibleInAP> { }
		/// <summary>
		/// Gets or sets whether the Project is visible in the AP Module.
		/// If Project is set as invisible - it will not show up in the field selectors in the given module.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInAP>))]
		[PXUIField(DisplayName = "AP")]
		public override Boolean? VisibleInAP
		{
			get
			{
				return this._VisibleInAP;
			}
			set
			{
				this._VisibleInAP = value;
			}
		}
		#endregion
		#region VisibleInAR
		public new abstract class visibleInAR : PX.Data.BQL.BqlBool.Field<visibleInAR> { }
		/// <summary>
		/// Gets or sets whether the Project is visible in the AR Module.
		/// If Project is set as invisible - it will not show up in the field selectors in the given module.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInAR>))]
		[PXUIField(DisplayName = "AR")]
		public override Boolean? VisibleInAR
		{
			get
			{
				return this._VisibleInAR;
			}
			set
			{
				this._VisibleInAR = value;
			}
		}
		#endregion
		#region VisibleInSO
		public new abstract class visibleInSO : PX.Data.BQL.BqlBool.Field<visibleInSO> { }
		/// <summary>
		/// Gets or sets whether the Project is visible in the SO Module.
		/// If Project is set as invisible - it will not show up in the field selectors in the given module.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInSO>))]
		[PXUIField(DisplayName = "SO")]
		public override Boolean? VisibleInSO
		{
			get
			{
				return this._VisibleInSO;
			}
			set
			{
				this._VisibleInSO = value;
			}
		}
		#endregion
		#region VisibleInPO
		public new abstract class visibleInPO : PX.Data.BQL.BqlBool.Field<visibleInPO> { }
		/// <summary>
		/// Gets or sets whether the Project is visible in the PO Module.
		/// If Project is set as invisible - it will not show up in the field selectors in the given module.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInPO>))]
		[PXUIField(DisplayName = "PO")]
		public override Boolean? VisibleInPO
		{
			get
			{
				return this._VisibleInPO;
			}
			set
			{
				this._VisibleInPO = value;
			}
		}
		#endregion

		#region VisibleInTA
		public new abstract class visibleInTA : PX.Data.BQL.BqlBool.Field<visibleInTA> { }
		/// <summary>
		/// Gets or sets whether the Project is visible in the EP Time Module.
		/// If Project is set as invisible - it will not show up in the field selectors in the given module.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInTA>))]
		[PXUIField(DisplayName = "Time Entries")]
		public override Boolean? VisibleInTA
		{
			get
			{
				return this._VisibleInTA;
			}
			set
			{
				this._VisibleInTA = value;
			}
		}
		#endregion
		#region VisibleInEA
		public new abstract class visibleInEA : PX.Data.BQL.BqlBool.Field<visibleInEA> { }
		/// <summary>
		/// Gets or sets whether the Project is visible in the EP Expense Module.
		/// If Project is set as invisible - it will not show up in the field selectors in the given module.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInEA>))]
		[PXUIField(DisplayName = "Expenses")]
		public override Boolean? VisibleInEA
		{
			get
			{
				return this._VisibleInEA;
			}
			set
			{
				this._VisibleInEA = value;
			}
		}
		#endregion

		#region VisibleInIN
		public new abstract class visibleInIN : PX.Data.BQL.BqlBool.Field<visibleInIN> { }
		/// <summary>
		/// Gets or sets whether the Project is visible in the IN Module.
		/// If Project is set as invisible - it will not show up in the field selectors in the given module.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInIN>))]
		[PXUIField(DisplayName = "IN")]
		public override Boolean? VisibleInIN
		{
			get
			{
				return this._VisibleInIN;
			}
			set
			{
				this._VisibleInIN = value;
			}
		}
		#endregion
		#region VisibleInCA
		public new abstract class visibleInCA : PX.Data.BQL.BqlBool.Field<visibleInCA> { }
		/// <summary>
		/// Gets or sets whether the Project is visible in the CA Module.
		/// If Project is set as invisible - it will not show up in the field selectors in the given module.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInCA>))]
		[PXUIField(DisplayName = "CA")]
		public override Boolean? VisibleInCA
		{
			get
			{
				return this._VisibleInCA;
			}
			set
			{
				this._VisibleInCA = value;
			}
		}
		#endregion
		#region VisibleInCR
		public new abstract class visibleInCR : PX.Data.BQL.BqlBool.Field<visibleInCR> { }
		/// <summary>
		/// Gets or sets whether the Project is visible in the CR Module.
		/// If Project is set as invisible - it will not show up in the field selectors in the given module.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInCR>))]
		[PXUIField(DisplayName = "CRM")]
		public override Boolean? VisibleInCR
		{
			get
			{
				return this._VisibleInCR;
			}
			set
			{
				this._VisibleInCR = value;
			}
		}
		#endregion
		#region NonProject
		public new abstract class nonProject : PX.Data.BQL.BqlBool.Field<nonProject> { }
		/// <summary>
		/// Only one project in the system is a Non-Project. A non-project is used whenever you have a transaction that is not applicable to any other project.
		/// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is Global", Visibility = PXUIVisibility.Visible, Visible = false)]
        public override Boolean? NonProject
        {
            get
            {
                return this._NonProject;
            }
            set
            {
                this._NonProject = value;
            }
        }
        #endregion
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXSearchable(SM.SearchCategory.PM, Messages.ProjectSearchTitle, new Type[] { typeof(PMProject.contractCD), typeof(PMProject.customerID), typeof(BAccount.acctName) },
		   new Type[] { typeof(PMProject.description) },
		   NumberFields = new Type[] { typeof(PMProject.contractCD) },
		   Line1Format = "{0}{1}{2:d}", Line1Fields = new Type[] { typeof(PMProject.templateID), typeof(PMProject.status), typeof(PMProject.startDate) },
		   Line2Format = "{0}", Line2Fields = new Type[] { typeof(PMProject.description) },
		   WhereConstraint = typeof(Where<Current<PMProject.baseType>, Equal<CTPRType.project>, And<Current<PMProject.nonProject>, NotEqual<True>>>),
		   MatchWithJoin = typeof(LeftJoin<Customer, On<Customer.bAccountID, Equal<PMProject.customerID>>>)
		)]
		[PXNote(DescriptionField = typeof(PMProject.contractCD))]
		public override Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		
		#region ServiceActivate
		public new abstract class serviceActivate : PX.Data.BQL.BqlBool.Field<serviceActivate> { }
		/// <summary>
		/// This field in not used with Projects. 
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public override Boolean? ServiceActivate
		{
			get
			{
				return this._ServiceActivate;
			}
			set
			{
				this._ServiceActivate = value;
			}
		}
		#endregion

		#region Attributes
		public new abstract class attributes : BqlAttributes.Field<attributes> { }
		/// <summary>
		/// Gets or sets entity attributes.
		/// </summary>
		[CRAttributesField(typeof(PMProject.classID), typeof(Contract.noteID))]
		public override string[] Attributes { get; set; }

		#region ClassID
		public new abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
		/// <summary>
		/// Gets ClassID for the attributes. Always returns current <see cref="GroupTypes.Project"/>
		/// </summary>
		[PXString(20)]
		public override string ClassID
		{
			get { return GroupTypes.Project; }
		}

		#endregion
	
		#endregion

		#region GroupMask
		public new abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		#endregion
		#region Included
		public abstract class included : PX.Data.BQL.BqlBool.Field<included> { }
		protected bool? _Included;
		[PXBool]
		[PXUIField(DisplayName = "Included")]
		[PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? Included
		{
			get
			{
				return this._Included;
			}
			set
			{
				this._Included = value;
			}
		}
		#endregion

		#region RestrictProjectSelect
		public abstract class restrictProjectSelect : PX.Data.BQL.BqlString.Field<restrictProjectSelect> { }
		protected String _RestrictProjectSelect;
		[PMRestrictOption.List]
		[PXString(1)]
		[PXDefault(PMRestrictOption.CustomerProjects, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Restrict Project Selection")]
		[PXDBScalar(
			typeof(Search<PMSetup.restrictProjectSelect>))]
		public virtual String RestrictProjectSelect
		{
			get
			{
				return this._RestrictProjectSelect;
			}
			set
			{
				this._RestrictProjectSelect = value;
			}
		}
		#endregion

		#region RetainagePct
		public new abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct> { }
		[PXDBDecimal(2, MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Retainage (%)", FieldClass = nameof(FeaturesSet.Retainage))]
		public override decimal? RetainagePct
		{
			get;
			set;
		}
		#endregion

		#region CuryCapAmount
		/// <exclude/>
		public abstract class curyCapAmount : PX.Data.BQL.BqlDecimal.Field<curyCapAmount> { }
		/// <summary>
		/// Retainage Cap Amount
		/// </summary>
		[PXCurrency(typeof(PMProject.curyInfoID), typeof(PMProject.capAmount))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Cap Amount", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual Decimal? CuryCapAmount
		{
			get;
			set;
		}
		#endregion

		#region CapAmount
		/// <exclude/>
		public abstract class capAmount : PX.Data.BQL.BqlDecimal.Field<capAmount> { }
		/// <summary>
		/// Retainage Cap Amount (in Base Currency)
		/// </summary>
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? CapAmount
		{
			get;
			set;
		}
		#endregion
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class ProjectStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Planned, Messages.InPlanning),
					Pair(Active, Messages.Active),
					Pair(Completed, Messages.Completed),
					Pair(Cancelled, Messages.Canceled),
					Pair(OnHold, Messages.Suspend),
					Pair(PendingApproval, Messages.PendingApproval),
					Pair(Contract.status.InUpgrade, CT.Messages.InUpgrade),
				}) {}
		}

        public class TemplStatusListAttribute : PXStringListAttribute
        {
			public TemplStatusListAttribute() : base(
				new[]
				{
					Pair(Active, Messages.Active),
					Pair(OnHold, Messages.OnHold),
				}) {}
        }

		public const string Planned = Contract.status.Draft;
		public const string Active = Contract.status.Active;
		public const string Completed = Contract.status.Completed;
        public const string OnHold = Contract.status.Expired;
		public const string Cancelled = Contract.status.Canceled;
		public const string PendingApproval = Contract.status.InApproval;

		public class active : PX.Data.BQL.BqlString.Constant<active>
		{
			public active() : base(Active) { ;}
		}

		public class completed : PX.Data.BQL.BqlString.Constant<completed>
		{
			public completed() : base(Completed) { ;}
		}

	}


	public sealed class NonProject : IBqlCreator, IBqlOperand
	{
		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection) {
			return true;
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			value = ID;
		}
		
		public static int ID
		{
			get
			{
				ProjectDefinition def = PXDatabase.GetSlot<ProjectDefinition>(typeof (NonProject).FullName);
				return def.ID;
			}
		}

		private class ProjectDefinition : IPrefetchable 
		{
			public int ID;

			public void Prefetch()
			{
			 using (PXConnectionScope s = new PXConnectionScope())
				{
					using (PXDataRecord record = PXDatabase.SelectSingle<Contract>(
						new PXDataField<Contract.contractID>(),
						new PXDataFieldValue<Contract.nonProject>(1)))
					{
						ID = record.GetInt32(0) ?? 0;
					}					
				}
			}

		}
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public static class BudgetLevels
	{
		public const string Task = "T";
		public const string Item = "I";
		public const string CostCode = "C";
        public const string Detail = "D";
    }

}
