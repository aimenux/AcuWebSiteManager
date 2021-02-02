using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.GL.DAC;
using Branch = PX.Objects.GL.Branch;
using PX.Objects.GL.Attributes;

namespace PX.Objects.AP
{
	[Serializable]
	[PXPrimaryGraph(typeof(MISC1099EFileProcessing))]
	[PXProjection(typeof(Select5<AP1099History,
		InnerJoin<AP1099Box, On<AP1099Box.boxNbr, Equal<AP1099History.boxNbr>>,
		InnerJoin<Vendor, On<Vendor.bAccountID, Equal<AP1099History.vendorID>>,
		InnerJoin<Contact, On<Contact.bAccountID, Equal<Vendor.bAccountID>,
			And<Contact.contactID, Equal<Vendor.defContactID>>>,
		InnerJoin<Location, On<Location.bAccountID, Equal<Vendor.bAccountID>,
			And<Location.locationID, Equal<Vendor.defLocationID>>>,
		InnerJoin<Branch, On<AP1099History.branchID, Equal<Branch.branchID>>,
		InnerJoin<Ledger, On<Branch.ledgerID, Equal<Ledger.ledgerID>>,
		LeftJoin<Organization, On<Branch.organizationID, Equal<Organization.organizationID>>>>>>>>>,
		Where<CurrentValue<MISC1099EFileFilter.box7>, Equal<MISC1099EFileFilter.box7.box7All>,
			Or<CurrentValue<MISC1099EFileFilter.box7>, Equal<MISC1099EFileFilter.box7.box7Equal>,
				And<AP1099History.boxNbr, Equal<MISC1099EFileFilter.box7.box7Nbr>,
			Or<CurrentValue<MISC1099EFileFilter.box7>, Equal<MISC1099EFileFilter.box7.box7NotEqual>,
				And<AP1099History.boxNbr, NotEqual<MISC1099EFileFilter.box7.box7Nbr>>>>>>,
		Aggregate<
			GroupBy<Vendor.bAccountID,
			GroupBy<AP1099History.branchID,
			GroupBy<AP1099History.finYear,
			Sum<AP1099History.histAmt>>>>>>), Persistent = false)]
	public class MISC1099EFileProcessingInfoRaw : AP1099History
	{
		#region VAcctCD
		public abstract class vAcctCD : PX.Data.BQL.BqlString.Field<vAcctCD> { }

		[PXUIField(DisplayName = "Vendor")]
		[PXDBString(30, IsUnicode = true, InputMask = "", BqlField = typeof(Vendor.acctCD))]
		public virtual string VAcctCD { get; set; }
		#endregion

		#region VAcctName
		public abstract class vAcctName : PX.Data.BQL.BqlString.Field<vAcctName> { }

		[PXDBString(60, IsUnicode = true, BqlField = typeof(Vendor.acctName))]
		[PXUIField(DisplayName = "Vendor Name")]
		public virtual string VAcctName { get; set; }
		#endregion

		#region LTaxRegistrationID
		public abstract class lTaxRegistrationID : PX.Data.BQL.BqlString.Field<lTaxRegistrationID> { }

		[PXDBString(50, IsUnicode = true, BqlField = typeof(Location.taxRegistrationID))]
		[PXUIField(DisplayName = "Tax Registration ID")]
		public virtual string LTaxRegistrationID { get; set; }
		#endregion

		#region PayerOrganizationID
		public abstract class payerOrganizationID : PX.Data.BQL.BqlInt.Field<payerOrganizationID> { }

		[PXDBInt(BqlField = typeof(Organization.organizationID))]
		[PXDimensionSelector(OrganizationAttribute._DimensionName, typeof(Search<Organization.organizationID>), typeof(Organization.organizationCD))]
		[PXUIField(DisplayName = "Payer Company")]
		public virtual int? PayerOrganizationID { get; set; }
		#endregion

		#region PayerBranchID
		public abstract class payerBranchID : PX.Data.BQL.BqlInt.Field<payerBranchID> { }

		[PXDBInt(BqlField = typeof(AP1099History.branchID))]
		[PXDimensionSelector(BranchAttribute._DimensionName, typeof(Search<Branch.branchID>), typeof(Branch.branchCD))]
		[PXUIField(DisplayName = "Payer Branch")]
		public virtual int? PayerBranchID { get; set; }
		#endregion

		#region PayerBAccountID
		public abstract class payerBAccountID : PX.Data.BQL.BqlInt.Field<payerBAccountID> { }

		[PXInt]
		[PXDimensionSelector(BAccountAttribute.DimensionName, typeof(Search<BAccount.bAccountID>), typeof(BAccount.acctCD))]
		[PXUIField(DisplayName = "Payer")]
		public virtual int? PayerBAccountID { get; set; }
		#endregion

		public new abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		public new abstract class boxNbr : PX.Data.BQL.BqlInt.Field<boxNbr> { }
	}

	// TODO: Workaround awaiting AC-64107
	[Serializable]
	[PXPrimaryGraph(typeof(MISC1099EFileProcessing))]
	public class MISC1099EFileProcessingInfo : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion

		#region PayerOrganizationID
		public abstract class payerOrganizationID : PX.Data.BQL.BqlInt.Field<payerOrganizationID> { }

		[PXInt(IsKey = true)]
		public virtual int? PayerOrganizationID { get; set; }
		#endregion

		#region PayerBranchID
		public abstract class payerBranchID : PX.Data.BQL.BqlInt.Field<payerBranchID> { }

		[PXInt(IsKey = true)]
		public virtual int? PayerBranchID { get; set; }
		#endregion

		#region PayerBAccountID
		public abstract class payerBAccountID : PX.Data.BQL.BqlInt.Field<payerBAccountID> { }

		[BAccount(IsDBField = false)]
		[PXUIField(DisplayName = "Payer")]
		public virtual int? PayerBAccountID { get; set; }
		#endregion

		#region DisplayOrganizationID
		public abstract class displayOrganizationID : PX.Data.BQL.BqlInt.Field<payerOrganizationID> { }

		[PXInt]
		[PXDimensionSelector(OrganizationAttribute._DimensionName, typeof(Search<Organization.organizationID>), typeof(Organization.organizationCD))]
		[PXUIField(DisplayName = "Payer Company")]
		public virtual int? DisplayOrganizationID { get; set; }
		#endregion

		#region DisplayBranchID
		public abstract class displayBranchID : PX.Data.BQL.BqlInt.Field<payerBranchID> { }

		[PXInt]
		[PXDimensionSelector(BranchAttribute._DimensionName, typeof(Search<Branch.branchID>), typeof(Branch.branchCD))]
		[PXUIField(DisplayName = "Payer Branch")]
		public virtual int? DisplayBranchID { get; set; }
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[PXInt(IsKey = true)]
		public virtual int? VendorID { get; set; }
		#endregion

		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }

		[PXString(4, IsKey = true, IsFixed = true)]
		public virtual string FinYear { get; set; }
		#endregion

		#region BoxNbr
		public abstract class boxNbr : PX.Data.BQL.BqlShort.Field<boxNbr> { }

		[PXShort(IsKey = true)]
		public virtual short? BoxNbr { get; set; }
		#endregion

		#region HistAmt
		public abstract class histAmt : PX.Data.BQL.BqlDecimal.Field<histAmt> { }

		[PXBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual decimal? HistAmt { get; set; }
		#endregion

		#region VAcctCD
		public abstract class vAcctCD : PX.Data.BQL.BqlString.Field<vAcctCD> { }

		[PXUIField(DisplayName = "Vendor")]
		[PXString(30, IsUnicode = true, InputMask = "")]
		public virtual string VAcctCD { get; set; }
		#endregion

		#region VAcctName
		public abstract class vAcctName : PX.Data.BQL.BqlString.Field<vAcctName> { }

		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Vendor Name")]
		public virtual string VAcctName { get; set; }
		#endregion

		#region LTaxRegistrationID
		public abstract class lTaxRegistrationID : PX.Data.BQL.BqlString.Field<lTaxRegistrationID> { }

		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Registration ID")]
		public virtual string LTaxRegistrationID { get; set; }
		#endregion
	}
}
