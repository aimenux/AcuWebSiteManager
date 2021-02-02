using System;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.CacheExtensions
{
    /*
     *  This table in acumatica has a primary key on (CompanyID) InventoryID, SiteID, & PlanID,
     *  however in the DAC only the InventoryID, and PlanID are marked with IsKey = true.
     *
     * We need to include siteID in the PXTableAttribute to avoid an error like this:
            Cannot insert the value NULL into column 'SiteID', table 'AcumaticaDB.dbo.INItemPlanAMExtension'; column does not allow nulls. INSERT fails. 
            The statement has been terminated.

        Having issues when marking for PO on sales order: duplicate initemplan records and the extension table is getting a planid = 1 record every time (which on 2nd+ attempts fails with another process has added)

        Unable to get the extension on INItemPlan to function correctly on sales orders. As a result we are unable to implement this extension as an extension. Using as a standalone table.
     */

    //[PXTable(
    //    typeof(INItemPlan.inventoryID),
    //    typeof(INItemPlan.siteID),
    //    typeof(INItemPlan.planID), IsOptional = true)]
    [Serializable]
    [PXCacheName(AM.Messages.AMItemPlan)]
    public class INItemPlanAMExtension : IBqlTable // : PXCacheExtension<INItemPlan>
    {
        //As of 17.209 the DB has 3 keys but in the INItemPlan DAC there are only 2 (excluded SiteID)
        #region PlanID
        public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
        protected Int64? _PlanID;
        [PXDBLong(IsKey = true)]
        public virtual Int64? PlanID
        {
            get
            {
                return this._PlanID;
            }
            set
            {
                this._PlanID = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [AnyInventory(IsKey = true)]
        [PXDefault]
        [PXParent(typeof(Select<INItemPlan, 
            Where<INItemPlan.planID, Equal<Current<INItemPlanAMExtension.planID>>,
            And<INItemPlan.inventoryID, Equal<Current<INItemPlanAMExtension.inventoryID>>>>>))]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;
        [Site]
        [PXDefault]
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

        #region AMSoftSupplyPlanID
        /// <summary>
        /// Soft link the given demand plan to a supply plan
        /// </summary>
        public abstract class aMSoftSupplyPlanID : PX.Data.BQL.BqlLong.Field<aMSoftSupplyPlanID> { }
        /// <summary>
        /// Soft link the given demand plan to a supply plan
        /// </summary>
        [PXDBLong]
        //[PXSelector(typeof(Search<INItemPlan.planID>), DirtyRead = true)]
        public virtual Int64? AMSoftSupplyPlanID { get; set; }
        #endregion
    }
}