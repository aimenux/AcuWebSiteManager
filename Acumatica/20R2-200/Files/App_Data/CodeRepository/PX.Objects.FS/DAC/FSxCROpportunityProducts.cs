using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using System;

using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.FS
{
    [PXTable(typeof(CROpportunityProducts.quoteID), typeof(CROpportunityProducts.lineNbr), IsOptional = true)]
    public class FSxCROpportunityProducts : PXCacheExtension<CROpportunityProducts>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>()
                && PXAccess.FeatureInstalled<FeaturesSet.customerModule>();
        }

        #region BillingRule
        public abstract class billingRule : ListField_BillingRule
        {
        }

        [PXDBString(4, IsFixed = true)]
        [billingRule.List]
        [PXDefault(ID.BillingRule.FLAT_RATE)]
        [PXUIField(DisplayName = "Billing Rule")]
        public virtual string BillingRule { get; set; }
        #endregion
        #region EstimatedDuration
        public abstract class estimatedDuration : PX.Data.BQL.BqlInt.Field<estimatedDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimated Duration")]
        [PXFormula(typeof(Default<CROpportunityProducts.inventoryID>))]
        public virtual int? EstimatedDuration { get; set; }
        #endregion
        #region VendorLocationID
        public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
        protected Int32? _VendorLocationID;
        [PXFormula(typeof(Default<CROpportunityProducts.vendorID>))]
        [PXDefault(typeof(Coalesce<
            Search<INItemSiteSettings.preferredVendorLocationID,
            Where<INItemSiteSettings.inventoryID, Equal<Current<CROpportunityProducts.inventoryID>>,
                    And<INItemSiteSettings.preferredVendorID, Equal<Current<CROpportunityProducts.vendorID>>>>>,
            Search2<Vendor.defLocationID,
                InnerJoin<CRLocation,
                    On<CRLocation.locationID, Equal<Vendor.defLocationID>,
                    And<CRLocation.bAccountID, Equal<Vendor.bAccountID>>>>,
                Where<Vendor.bAccountID, Equal<Current<CROpportunityProducts.vendorID>>,
                    And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<CROpportunityProducts.vendorID>>>),
                DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Vendor Location ID", Visible = true)]
        public virtual int? VendorLocationID { get; set; }
        #endregion
        #region ChkServiceManagement
        public abstract class ChkServiceManagement : PX.Data.BQL.BqlBool.Field<ChkServiceManagement> { }

        [PXBool]
        [PXUIField(Visible = false)]
        public virtual bool? chkServiceManagement
        {
            get
            {
                return true;
            }
        }
        #endregion
    }
}
