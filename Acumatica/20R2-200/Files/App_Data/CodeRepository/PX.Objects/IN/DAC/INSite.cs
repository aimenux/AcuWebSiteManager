using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.CR;
using PX.Data.BQL.Fluent;

namespace PX.Objects.IN
{
	[Serializable]
    [PXHidden]
	public partial class INCostSite : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INCostSite>.By<costSiteID>
		{
			public static INCostSite Find(PXGraph graph, int? costSiteID) => FindBy(graph, costSiteID);
		}
		#endregion
		#region CostSiteID
		public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
		protected Int32? _CostSiteID;
		[PXDBIdentity()]
		public virtual Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
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
	}
	
	[System.SerializableAttribute()]
	[PXPrimaryGraph(
		new Type[] { typeof(INSiteMaint)},
		new Type[] { typeof(Select<INSite, 
			Where<INSite.siteID, Equal<Current<INSite.siteID>>>>)
		})]
	[PXCacheName(Messages.INSite, PXDacType.Catalogue, CacheGlobal = true)]
	public partial class INSite : PX.Data.IBqlTable, PX.SM.IIncludable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INSite>.By<siteID>.Dirty
		{
			public static INSite Find(PXGraph graph, int? siteID) => FindBy(graph, siteID, (siteID ?? 0) <= 0);
		}
		public class UK : PrimaryKeyOf<INSite>.By<siteCD>
		{
			public static INSite Find(PXGraph graph, string siteCD) => FindBy(graph, siteCD);
		}
		public static class FK
		{
			public class InvtSub : Sub.PK.ForeignKeyOf<INSite>.By<invtSubID> { }
			public class COGSSub : Sub.PK.ForeignKeyOf<INSite>.By<cOGSSubID> { }
			public class SalesSub : Sub.PK.ForeignKeyOf<INSite>.By<salesSubID> { }
			public class StdCstRevSub : Sub.PK.ForeignKeyOf<INSite>.By<stdCstRevSubID> { }
			public class PPVSub : Sub.PK.ForeignKeyOf<INSite>.By<pPVSubID> { }
			public class POAccrualSub : Sub.PK.ForeignKeyOf<INSite>.By<pOAccrualSubID> { }
			public class StdCstVarSub : Sub.PK.ForeignKeyOf<INSite>.By<stdCstVarSubID> { }
			public class LCVarianceSub : Sub.PK.ForeignKeyOf<INSite>.By<lCVarianceSubID> { }
			public class ReceiptLocation : INLocation.PK.ForeignKeyOf<INSite>.By<receiptLocationID> { }
			public class ShipLocation : INLocation.PK.ForeignKeyOf<INSite>.By<shipLocationID> { }
			public class ReturnLocation : INLocation.PK.ForeignKeyOf<INSite>.By<returnLocationID> { }
			public class DropShipLocation : INLocation.PK.ForeignKeyOf<INSite>.By<dropShipLocationID> { }
			public class ReasonCodeSub : Sub.PK.ForeignKeyOf<INSite>.By<reasonCodeSubID> { }
			public class ReplenishmentClass : INReplenishmentClass.PK.ForeignKeyOf<INSite>.By<replenishmentClassID> { }
			public class Address : CR.Address.PK.ForeignKeyOf<INSite>.By<addressID> { }
			public class Contact : CR.Contact.PK.ForeignKeyOf<INSite>.By<contactID> { }
			public class Building : INSiteBuilding.PK.ForeignKeyOf<INSite>.By<buildingID> { }
		}
        #endregion
        #region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDBForeignIdentity(typeof(INCostSite))]
		[PXReferentialIntegrityCheck]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region SiteCD
		public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
		protected String _SiteCD;
        [PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>), IN.Messages.TransitSiteIsNotAvailable)]
        [IN.SiteRaw(true, IsKey=true)]
		[PXDefault()]
		[PX.Data.EP.PXFieldDescription]
		public virtual String SiteCD
		{
			get
			{
				return this._SiteCD;
			}
			set
			{
				this._SiteCD = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName="Description", Visibility=PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region ReasonCodeSubID
		public abstract class reasonCodeSubID : PX.Data.BQL.BqlInt.Field<reasonCodeSubID> { }
		protected Int32? _ReasonCodeSubID;
		[SubAccount(DisplayName = "Reason Code Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? ReasonCodeSubID
		{
			get
			{
				return this._ReasonCodeSubID;
			}
			set
			{
				this._ReasonCodeSubID = value;
			}
		}
		#endregion
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		protected Int32? _SalesAcctID;
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<INSite.salesAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.BQL.BqlInt.Field<salesSubID> { }
		protected Int32? _SalesSubID;
		[SubAccount(typeof(INSite.salesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<INSite.salesSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region InvtAcctID
		public abstract class invtAcctID : PX.Data.BQL.BqlInt.Field<invtAcctID> { }
		protected Int32? _InvtAcctID;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Inventory Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.IN)]
		[PXForeignReference(typeof(Field<INSite.invtAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? InvtAcctID
		{
			get
			{
				return this._InvtAcctID;
			}
			set
			{
				this._InvtAcctID = value;
			}
		}
		#endregion
		#region InvtSubID
		public abstract class invtSubID : PX.Data.BQL.BqlInt.Field<invtSubID> { }
		protected Int32? _InvtSubID;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(INSite.invtAcctID), DisplayName = "Inventory Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<INSite.invtSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? InvtSubID
		{
			get
			{
				return this._InvtSubID;
			}
			set
			{
				this._InvtSubID = value;
			}
		}
		#endregion
		#region COGSAcctID
		public abstract class cOGSAcctID : PX.Data.BQL.BqlInt.Field<cOGSAcctID> { }
		protected Int32? _COGSAcctID;
		[Account(DisplayName = "COGS/Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<INSite.cOGSAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? COGSAcctID
		{
			get
			{
				return this._COGSAcctID;
			}
			set
			{
				this._COGSAcctID = value;
			}
		}
		#endregion
		#region COGSSubID
		public abstract class cOGSSubID : PX.Data.BQL.BqlInt.Field<cOGSSubID> { }
		protected Int32? _COGSSubID;
		[SubAccount(typeof(INSite.cOGSAcctID), DisplayName = "COGS/Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<INSite.cOGSSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? COGSSubID
		{
			get
			{
				return this._COGSSubID;
			}
			set
			{
				this._COGSSubID = value;
			}
		}
		#endregion
		#region StdCstRevAcctID
		public abstract class stdCstRevAcctID : PX.Data.BQL.BqlInt.Field<stdCstRevAcctID> { }
		protected Int32? _StdCstRevAcctID;
		[Account(DisplayName = "Standard Cost Revaluation Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<INSite.stdCstRevAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? StdCstRevAcctID
		{
			get
			{
				return this._StdCstRevAcctID;
			}
			set
			{
				this._StdCstRevAcctID = value;
			}
		}
		#endregion
		#region StdCstRevSubID
		public abstract class stdCstRevSubID : PX.Data.BQL.BqlInt.Field<stdCstRevSubID> { }
		protected Int32? _StdCstRevSubID;
		[SubAccount(typeof(INSite.stdCstRevAcctID), DisplayName = "Standard Cost Revaluation Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<INSite.stdCstRevSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? StdCstRevSubID
		{
			get
			{
				return this._StdCstRevSubID;
			}
			set
			{
				this._StdCstRevSubID = value;
			}
		}
		#endregion
		#region StdCstVarAcctID
		public abstract class stdCstVarAcctID : PX.Data.BQL.BqlInt.Field<stdCstVarAcctID> { }
		protected Int32? _StdCstVarAcctID;
		[Account(DisplayName = "Standard Cost Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<INSite.stdCstVarAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? StdCstVarAcctID
		{
			get
			{
				return this._StdCstVarAcctID;
			}
			set
			{
				this._StdCstVarAcctID = value;
			}
		}
		#endregion
		#region StdCstVarSubID
		public abstract class stdCstVarSubID : PX.Data.BQL.BqlInt.Field<stdCstVarSubID> { }
		protected Int32? _StdCstVarSubID;
		[SubAccount(typeof(INSite.stdCstVarAcctID), DisplayName = "Standard Cost Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<INSite.stdCstVarSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? StdCstVarSubID
		{
			get
			{
				return this._StdCstVarSubID;
			}
			set
			{
				this._StdCstVarSubID = value;
			}
		}
		#endregion
		#region PPVAcctID
		public abstract class pPVAcctID : PX.Data.BQL.BqlInt.Field<pPVAcctID> { }
		protected Int32? _PPVAcctID;
		[Account(DisplayName = "Purchase Price Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<INSite.pPVAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? PPVAcctID
		{
			get
			{
				return this._PPVAcctID;
			}
			set
			{
				this._PPVAcctID = value;
			}
		}
		#endregion
		#region PPVSubID
		public abstract class pPVSubID : PX.Data.BQL.BqlInt.Field<pPVSubID> { }
		protected Int32? _PPVSubID;
		[SubAccount(typeof(INSite.pPVAcctID), DisplayName = "Purchase Price Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<INSite.pPVSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? PPVSubID
		{
			get
			{
				return this._PPVSubID;
			}
			set
			{
				this._PPVSubID = value;
			}
		}
		#endregion
		#region POAccrualAcctID
		public abstract class pOAccrualAcctID : PX.Data.BQL.BqlInt.Field<pOAccrualAcctID> { }
		protected Int32? _POAccrualAcctID;
		[Account(DisplayName = "PO Accrual Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.PO)]
		[PXForeignReference(typeof(Field<INSite.pOAccrualAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? POAccrualAcctID
		{
			get
			{
				return this._POAccrualAcctID;
			}
			set
			{
				this._POAccrualAcctID = value;
			}
		}
		#endregion
		#region POAccrualSubID
		public abstract class pOAccrualSubID : PX.Data.BQL.BqlInt.Field<pOAccrualSubID> { }
		protected Int32? _POAccrualSubID;
		[SubAccount(typeof(INSite.pOAccrualAcctID), DisplayName = "PO Accrual Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<INSite.pOAccrualSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? POAccrualSubID
		{
			get
			{
				return this._POAccrualSubID;
			}
			set
			{
				this._POAccrualSubID = value;
			}
		}
		#endregion
		#region LCVarianceAcctID
		public abstract class lCVarianceAcctID : PX.Data.BQL.BqlInt.Field<lCVarianceAcctID> { }
		protected Int32? _LCVarianceAcctID;
		[Account(DisplayName = "Landed Cost Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<INSite.lCVarianceAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? LCVarianceAcctID
		{
			get
			{
				return this._LCVarianceAcctID;
			}
			set
			{
				this._LCVarianceAcctID = value;
			}
		}
		#endregion
		#region LCVarianceSubID
		public abstract class lCVarianceSubID : PX.Data.BQL.BqlInt.Field<lCVarianceSubID> { }
		protected Int32? _LCVarianceSubID;
		[SubAccount(typeof(INSite.lCVarianceAcctID), DisplayName = "Landed Cost Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<INSite.lCVarianceSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? LCVarianceSubID
		{
			get
			{
				return this._LCVarianceSubID;
			}
			set
			{
				this._LCVarianceSubID = value;
			}
		}
		#endregion
		#region ReceiptLocationID
		public abstract class receiptLocationID : PX.Data.BQL.BqlInt.Field<receiptLocationID> { }
		protected Int32? _ReceiptLocationID;
		[PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), Messages.LocationIsNotActive)]
		[IN.Location(typeof(INSite.siteID), DisplayName = "Receiving Location", DescriptionField = typeof(INLocation.descr), DirtyRead = true)]
		public virtual Int32? ReceiptLocationID
		{
			get
			{
				return this._ReceiptLocationID;
			}
			set
			{
				this._ReceiptLocationID = value;
			}
		}
		#endregion
		#region ReceiptLocationIDOverride
		public abstract class receiptLocationIDOverride : PX.Data.BQL.BqlBool.Field<receiptLocationIDOverride> { }
		protected Boolean? _ReceiptLocationIDOverride;
		[PXBool()]		
		public virtual Boolean? ReceiptLocationIDOverride
		{
			get
			{
				return this._ReceiptLocationIDOverride;
			}
			set
			{
				this._ReceiptLocationIDOverride = value;
			}
		}
		#endregion
		#region ShipLocationID
		public abstract class shipLocationID : PX.Data.BQL.BqlInt.Field<shipLocationID> { }
		protected Int32? _ShipLocationID;
		[PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), Messages.LocationIsNotActive)]
		[IN.Location(typeof(INSite.siteID), DisplayName = "Shipping Location", DescriptionField = typeof(INLocation.descr), DirtyRead = true)]
		public virtual Int32? ShipLocationID
		{
			get
			{
				return this._ShipLocationID;
			}
			set
			{
				this._ShipLocationID = value;
			}
		}
		#endregion
		#region ShipLocationIDOverride
		public abstract class shipLocationIDOverride : PX.Data.BQL.BqlBool.Field<shipLocationIDOverride> { }
		protected Boolean? _ShipLocationIDOverride;
		[PXBool()]		
		public virtual Boolean? ShipLocationIDOverride
		{
			get
			{
				return this._ShipLocationIDOverride;
			}
			set
			{
				this._ShipLocationIDOverride = value;
			}
		}
		#endregion
		#region DropShipLocationID
		public abstract class dropShipLocationID : PX.Data.BQL.BqlInt.Field<dropShipLocationID> { }
		protected Int32? _DropShipLocationID;
		[PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), Messages.LocationIsNotActive)]
		[IN.Location(typeof(INSite.siteID), DisplayName = "Drop-Ship Location", DescriptionField = typeof(INLocation.descr), DirtyRead = true)]
		public virtual Int32? DropShipLocationID
		{
			get
			{
				return this._DropShipLocationID;
			}
			set
			{
				this._DropShipLocationID = value;
			}
		}
		#endregion
		#region ReturnLocationID
		public abstract class returnLocationID : PX.Data.BQL.BqlInt.Field<returnLocationID> { }
		protected Int32? _ReturnLocationID;
		[PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), Messages.LocationIsNotActive)]
		[IN.Location(typeof(INSite.siteID), DisplayName = "RMA Location", DescriptionField = typeof(INLocation.descr), DirtyRead = true)]
		public virtual Int32? ReturnLocationID
		{
			get
			{
				return this._ReturnLocationID;
			}
			set
			{
				this._ReturnLocationID = value;
			}
		}
		#endregion
		#region LocationValid
		public abstract class locationValid : PX.Data.BQL.BqlString.Field<locationValid> { }
		protected String _LocationValid;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Location Entry")]
		[INLocationValid.List()]
		[PXDefault(INLocationValid.Validate)]
		public virtual String LocationValid
		{
			get
			{
				return this._LocationValid;
			}
			set
			{
				this._LocationValid = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch()]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region AddressID
		public abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		protected Int32? _AddressID;
		[PXDBInt]
		[PXDBChildIdentity(typeof(Address.addressID))]
		/*[PXSelector(typeof(Search<Address.addressID,
			Where<Address.bAccountID, Equal<Current<BAccount.bAccountID>>>>),
			DescriptionField = typeof(Address.displayName), DirtyRead = true)]*/
		public virtual Int32? AddressID
		{
			get
			{
				return this._AddressID;
			}
			set
			{
				this._AddressID = value;
			}
		}
		#endregion
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		protected Int32? _ContactID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		//[PXSelector(typeof(Search<Contact.contactID,Where<Contact.bAccountID,Equal<Current<BAccount.bAccountID>>>>))]
		public virtual Int32? ContactID
		{
			get
			{
				return this._ContactID;
			}
			set
			{
				this._ContactID = value;
			}
		}
		#endregion
		#region BuildingID
		[PXDBInt]
		[PXUIField(DisplayName = "Building ID")]
		[PXForeignReference(typeof(FK.Building))]
		[PXSelector(
			typeof(SearchFor<INSiteBuilding.buildingID>.In<SelectFrom<INSiteBuilding>.Where<INSiteBuilding.branchID.IsEqual<branchID.FromCurrent>>>),
			SubstituteKey = typeof(INSiteBuilding.buildingCD),
			DescriptionField = typeof(INSiteBuilding.descr))]
		public virtual Int32? BuildingID { get; set; }
		public abstract class buildingID : PX.Data.BQL.BqlInt.Field<buildingID> { }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(INSite.siteCD), 
			Selector = typeof(INSite.siteCD), 
			FieldList = new [] { typeof(INSite.siteCD), typeof(INSite.descr), typeof(INSite.replenishmentClassID) })]
		public virtual Guid? NoteID
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
		#region ReplenishmentClassID
		public abstract class replenishmentClassID : PX.Data.BQL.BqlString.Field<replenishmentClassID> { }
		protected String _ReplenishmentClassID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Replenishment Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INReplenishmentClass.replenishmentClassID>), DescriptionField = typeof(INReplenishmentClass.descr))]
		public virtual String ReplenishmentClassID
		{
			get
			{
				return this._ReplenishmentClassID;
			}
			set
			{
				this._ReplenishmentClassID = value;
			}
		}
		#endregion
		#region AvgDefaultCost
		public abstract class avgDefaultCost : PX.Data.BQL.BqlString.Field<avgDefaultCost>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					new[]
					{
						Pair(AverageCost, Messages.AverageCost),
						Pair(LastCost, Messages.LastCost),
					}) {}
			}

			public const string AverageCost = "A";
			public const string LastCost = "L";

			public class averageCost : PX.Data.BQL.BqlString.Constant<averageCost>
			{
				public averageCost() : base(AverageCost){}
			}
			public class lastCost : PX.Data.BQL.BqlString.Constant<lastCost>
			{
				public lastCost() : base(LastCost) {}
			}
		}
		protected String _AvgDefaultCost;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(avgDefaultCost.AverageCost, PersistingCheck = PXPersistingCheck.Nothing)]
		[avgDefaultCost.List]
		[PXUIField(DisplayName = "Avg. Default Returns Cost")]
		public virtual String AvgDefaultCost
		{
			get
			{
				return _AvgDefaultCost;
			}
			set
			{
				_AvgDefaultCost = value;
			}
		}
		#endregion
		#region FIFODefaultCost
		public abstract class fIFODefaultCost : PX.Data.BQL.BqlString.Field<fIFODefaultCost> { }
		protected String _FIFODefaultCost;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(avgDefaultCost.AverageCost, PersistingCheck = PXPersistingCheck.Nothing)]
		[avgDefaultCost.List]
		[PXUIField(DisplayName = "FIFO Default Returns Cost")]
		public virtual String FIFODefaultCost
		{
			get
			{
				return _FIFODefaultCost;
			}
			set
			{
				_FIFODefaultCost = value;
			}
		}
		#endregion
		#region OverrideInvtAccSub
		public abstract class overrideInvtAccSub : PX.Data.BQL.BqlBool.Field<overrideInvtAccSub> { }
		protected bool? _OverrideInvtAccSub;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = Messages.OverrideInventoryAcctSub)]
		public virtual bool? OverrideInvtAccSub
		{
			get
			{
				return _OverrideInvtAccSub;
			}
			set
			{
				_OverrideInvtAccSub = value;
			}
		}
		#endregion
        #region Active
        public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? Active { get; set; }
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
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
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
		[PXDBLastModifiedDateTime()]
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
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;
		[PXDBGroupMask()]
		public virtual Byte[] GroupMask
		{
			get
			{
				return this._GroupMask;
			}
			set
			{
				this._GroupMask = value;
			}
		}
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
		#region UseItemDefaultLocationForPicking
		public abstract class useItemDefaultLocationForPicking : PX.Data.BQL.BqlBool.Field<useItemDefaultLocationForPicking> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Item Default Location for Picking")]
		public virtual bool? UseItemDefaultLocationForPicking
		{
			get;
			set;
		}
		#endregion

		#region DiscAcctID
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class discAcctID : Data.BQL.BqlInt.Field<discAcctID> { }
		[PXInt]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual int? DiscAcctID
		{
			get { return null; }
			set { }
		}
		#endregion
		#region DiscSubID
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class discSubID : Data.BQL.BqlInt.Field<discSubID> { }
		[PXInt]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual int? DiscSubID
		{
			get { return null; }
			set { }
		}
		#endregion
		#region FreightAcctID
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class freightAcctID : Data.BQL.BqlInt.Field<freightAcctID> { }
		[PXInt]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual int? FreightAcctID
		{
			get { return null; }
			set { }
		}
		#endregion
		#region FreightSubID
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class freightSubID : Data.BQL.BqlInt.Field<freightSubID> { }
		[PXInt]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual int? FreightSubID
		{
			get { return null; }
			set { }
		}
		#endregion
		#region MiscAcctID
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class miscAcctID : Data.BQL.BqlInt.Field<miscAcctID> { }
		[PXInt]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual int? MiscAcctID
		{
			get { return null; }
			set { }
		}
		#endregion
		#region MiscSubID
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class miscSubID : Data.BQL.BqlInt.Field<miscSubID> { }
		[PXInt]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual int? MiscSubID
		{
			get { return null; }
			set { }
		}
		#endregion

		public const string Main = "MAIN";
		public class main : PX.Data.BQL.BqlString.Constant<main>
		{
			public main()
				: base(Main)
			{
			}
		}
	}
	#region Attributes
	public class INLocationValid
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Validate, Messages.LocValidate),
					Pair(Warn, Messages.LocWarn),
					Pair(NoValidate, Messages.LocNoValidate),
				}) {}
		}

		public const string Validate = "V";
		public const string Warn = "W";
		public const string NoValidate = "N";

		public class validate : PX.Data.BQL.BqlString.Constant<validate>
		{
			public validate() : base(Validate) { ;}
		}

		public class noValidate : PX.Data.BQL.BqlString.Constant<noValidate>
		{
			public noValidate() : base(NoValidate) { ;}
		}

		public class warn : PX.Data.BQL.BqlString.Constant<warn>
		{
			public warn() : base(Warn) { ;}
		}
	}
	#endregion

	public sealed class UserPreferenceExt : PXCacheExtension<PX.SM.UserPreferences>
	{
		#region DefaultSite
		[Site(DisplayName = "Default Warehouse")]
		[PXForeignReference(typeof(Field<defaultSite>.IsRelatedTo<INSite.siteID>))]
		public int? DefaultSite { get; set; }
		public abstract class defaultSite : PX.Data.BQL.BqlInt.Field<defaultSite> { }
		#endregion
	}
}