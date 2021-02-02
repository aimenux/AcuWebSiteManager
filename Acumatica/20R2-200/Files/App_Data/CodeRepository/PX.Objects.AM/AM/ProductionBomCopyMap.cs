using PX.Objects.AM.Attributes;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Mapping bom DACs to production DACs
    /// </summary>
    public static class ProductionBomCopyMap
    {
        #region OPER
        /// <summary>
        /// Copy bom operation DAC fields to a production operation DAC
        /// </summary>
        /// <param name="bomOper">Existing AMBomOper DAC record</param>
        /// <param name="prodOper">AMProdOper DAC record</param>
        /// <returns>Passed prodOper object with the copied data</returns>
        public static AMProdOper CopyOper(AMBomOper bomOper, AMProdOper prodOper)
        {
            if (prodOper == null)
            {
                prodOper = new AMProdOper();
            }

            if (bomOper == null)
            {
                return prodOper;
            }

            prodOper.BFlush = bomOper.BFlush;
            prodOper.Descr = bomOper.Descr;
            prodOper.MachineUnitTime = bomOper.MachineUnitTime;
            prodOper.MachineUnits = bomOper.MachineUnits;
            prodOper.QueueTime = bomOper.QueueTime;
            prodOper.RunUnits = bomOper.RunUnits;
            prodOper.RunUnitTime = bomOper.RunUnitTime;
            prodOper.SetupTime = bomOper.SetupTime;
            prodOper.WcID = bomOper.WcID;
            prodOper.ScrapAction = bomOper.ScrapAction;
            prodOper.OutsideProcess = bomOper.OutsideProcess;
            prodOper.DropShippedToVendor = bomOper.DropShippedToVendor;
            prodOper.VendorID = bomOper.VendorID;
            prodOper.VendorLocationID = bomOper.VendorLocationID;

            SetPhtmBomReferences(ref prodOper, bomOper, null);

            return prodOper;
        }

        /// <summary>
        /// Copy bom operation DAC fields to a production operation DAC
        /// </summary>
        /// <param name="bomOper">Existing AMBomOper DAC record</param>
        /// <returns>new AMProdOper DAC record</returns>
        public static AMProdOper CopyOper(AMBomOper bomOper)
        {
            return CopyOper(bomOper, new AMProdOper());
        }
        #endregion

        #region MATL
        /// <summary>
        /// Copy bom material DAC fields to a production material DAC
        /// </summary>
        /// <param name="bomMatl">Existing AMBomOvhd DAC record</param>
        /// <param name="prodMatl">AMProdMatl DAC record</param>
        /// <returns>Passed prodMatl object with the copied data</returns>
        public static AMProdMatl CopyMatl(AMBomMatl bomMatl, AMProdMatl prodMatl)
        {
            if (prodMatl == null)
            {
                prodMatl = new AMProdMatl();
            }

            if (bomMatl == null)
            {
                return prodMatl;
            }

            prodMatl.BFlush = bomMatl.BFlush;
            prodMatl.CompBOMID = bomMatl.CompBOMID;
            prodMatl.CompBOMRevisionID = bomMatl.CompBOMRevisionID;
            prodMatl.Descr = bomMatl.Descr;
            prodMatl.SortOrder = bomMatl.SortOrder;
            prodMatl.InventoryID = bomMatl.InventoryID;
            prodMatl.SubItemID = bomMatl.SubItemID;
            prodMatl.MaterialType = bomMatl.MaterialType;
            prodMatl.PhantomRouting = bomMatl.PhantomRouting;
            prodMatl.ScrapFactor = bomMatl.ScrapFactor;
            prodMatl.QtyReq = bomMatl.QtyReq;
            prodMatl.UOM = bomMatl.UOM;
            prodMatl.SiteID = bomMatl.SiteID;
            prodMatl.WarehouseOverride = bomMatl.SiteID != null;
            prodMatl.LocationID = bomMatl.LocationID;
            prodMatl.BatchSize = bomMatl.BatchSize;
            prodMatl.SubcontractSource = bomMatl.SubcontractSource;
            //if material type is not subcontract, let POCreate default normally
            if (prodMatl.MaterialType == AMMaterialType.Subcontract)
            {
                prodMatl.POCreate = prodMatl.SubcontractSource == AMSubcontractSource.Purchase;
            }

            // Do not set the UnitCost - let it rebuild from the PXDefault to get the latest unit cost
            //prodMatl.UnitCost = bomMatl.UnitCost;

            SetPhtmBomReferences(ref prodMatl, bomMatl, bomMatl.LineID);

            return prodMatl;
        }

        /// <summary>
        /// Copy bom material DAC fields to a production material DAC
        /// </summary>
        /// <param name="bomMatl">Existing AMBomOvhd DAC record</param>
        /// <returns>new AMProdMatl DAC record</returns>
        public static AMProdMatl CopyMatl(AMBomMatl bomMatl)
        {
            return CopyMatl(bomMatl, new AMProdMatl());
        }

        #endregion

        #region OVHD
        /// <summary>
        /// Copy bom overhead DAC fields to a production overhead DAC
        /// </summary>
        /// <param name="bomOvhd">Existing AMBomOvhd DAC record</param>
        /// <param name="prodOvhd">AMProdOvhd DAC record</param>
        /// <returns>Passed prodOvhd object with the copied data</returns>
        public static AMProdOvhd CopyOvhd(AMBomOvhd bomOvhd, AMProdOvhd prodOvhd)
        {
            if (prodOvhd == null)
            {
                prodOvhd = new AMProdOvhd();
            }

            if (bomOvhd == null)
            {
                return prodOvhd;
            }

            prodOvhd.OFactor = bomOvhd.OFactor;
            prodOvhd.OvhdID = bomOvhd.OvhdID;

            SetPhtmBomReferences(ref prodOvhd, bomOvhd, bomOvhd.LineID);

            return prodOvhd;
        }

        /// <summary>
        /// Copy bom overhead DAC fields to a production overhead DAC
        /// </summary>
        /// <param name="bomOvhd">Existing AMBomOvhd DAC record</param>
        /// <returns>new AMProdOvhd DAC record</returns>
        public static AMProdOvhd CopyOvhd(AMBomOvhd bomOvhd)
        {
            return CopyOvhd(bomOvhd, new AMProdOvhd());
        }
        #endregion

        #region TOOL
        /// <summary>
        /// Copy bom tool DAC fields to a production tool DAC
        /// </summary>
        /// <param name="bomTool">Existing AMBomTool DAC record</param>
        /// <param name="prodTool">AMProdTool DAC record</param>
        /// <returns>Passed prodTool object with the copied data</returns>
        public static AMProdTool CopyTool(AMBomTool bomTool, AMProdTool prodTool)
        {
            if (prodTool == null)
            {
                prodTool = new AMProdTool();
            }

            if (bomTool == null)
            {
                return prodTool;
            }

            prodTool.Descr = bomTool.Descr;
            prodTool.QtyReq = bomTool.QtyReq;
            prodTool.ToolID = bomTool.ToolID;
            prodTool.UnitCost = bomTool.UnitCost;

            SetPhtmBomReferences(ref prodTool, bomTool, bomTool.LineID);

            return prodTool;
        }

        /// <summary>
        /// Copy bom tool DAC fields to a production tool DAC
        /// </summary>
        /// <param name="bomTool">Existing AMBomTool DAC record</param>
        /// <returns>new AMProdTool DAC record</returns>
        public static AMProdTool CopyTool(AMBomTool bomTool)
        {
            return CopyTool(bomTool, new AMProdTool());
        }
        #endregion

        #region STEP
        /// <summary>
        /// Copy bom step DAC fields to a production step DAC
        /// </summary>
        /// <param name="bomStep">Existing AMBomStep DAC record</param>
        /// <param name="prodStep">AMProdStep DAC record</param>
        /// <returns>Passed prodStep object with the copied data</returns>
        public static AMProdStep CopyStep(AMBomStep bomStep, AMProdStep prodStep)
        {
            if (prodStep == null)
            {
                prodStep = new AMProdStep();
            }

            if (bomStep == null)
            {
                return prodStep;
            }

            prodStep.Descr = bomStep.Descr;
            prodStep.SortOrder = bomStep.SortOrder.GetValueOrDefault();

            SetPhtmBomReferences(ref prodStep, bomStep, bomStep.LineID);

            return prodStep;
        }

        /// <summary>
        /// Copy bom step DAC fields to a production step DAC
        /// </summary>
        /// <param name="bomStep">Existing AMBomStep DAC record</param>
        /// <returns>new AMProdStep DAC record</returns>
        public static AMProdStep CopyStep(AMBomStep bomStep)
        {
            return CopyStep(bomStep, new AMProdStep());
        }
        #endregion

        #region Attributes
        /// <summary>
        /// Copy bom attribute DAC fields to a production attribute DAC
        /// </summary>
        /// <param name="bomAttribute">Existing AMBomAttribute DAC record</param>
        /// <param name="prodAttribute">AMProdAttribute DAC record</param>
        /// <returns>Passed prodAttribute object with the copied data</returns>
        public static AMProdAttribute CopyAttributes(AMBomAttribute bomAttribute, AMProdAttribute prodAttribute)
        {
            if (prodAttribute == null)
            {
                prodAttribute = new AMProdAttribute();
            }

            if (bomAttribute == null)
            {
                return null;
            }

            prodAttribute.Level = bomAttribute.Level == AMAttributeLevels.Operation ? AMAttributeLevels.Operation : AMAttributeLevels.Order;
            prodAttribute.Source = AMAttributeSource.BOM;
            prodAttribute.AttributeID = bomAttribute.AttributeID;
            prodAttribute.Label = bomAttribute.Label;
            prodAttribute.Descr = bomAttribute.Descr;
            prodAttribute.Enabled = bomAttribute.Enabled;
            prodAttribute.TransactionRequired = bomAttribute.TransactionRequired;
            prodAttribute.Value = bomAttribute.Value;
            
            return prodAttribute;
        }

        /// <summary>
        /// Copy bom attribute DAC fields to a production attribute DAC
        /// </summary>
        /// <param name="bomAttribute">Existing AMBomAttribute DAC record</param>
        /// <returns>new AMProdAttribute DAC record</returns>
        public static AMProdAttribute CopyAttributes(AMBomAttribute bomAttribute)
        {
            return CopyAttributes(bomAttribute, new AMProdAttribute());
        }

        /// <summary>
        /// Copy configuration attribute DAC fields to a production attribute DAC
        /// </summary>
        /// <param name="configAttribute">Existing configuration attribute DAC record</param>
        /// <param name="configResultAttribute">Existing configuration results attribute DAC record</param>
        /// <param name="prodAttribute">AMProdAttribute DAC record</param>
        /// <returns>Passed prodAttribute object with the copied data</returns>
        public static AMProdAttribute CopyAttributes(AMConfigurationAttribute configAttribute, AMConfigResultsAttribute configResultAttribute, AMProdAttribute prodAttribute)
        {
            if (prodAttribute == null)
            {
                prodAttribute = new AMProdAttribute();
            }

            if (configAttribute == null || configResultAttribute == null)
            {
                return null;
            }

            prodAttribute.Level = AMAttributeLevels.Order;
            prodAttribute.Source = AMAttributeSource.Configuration;
            prodAttribute.AttributeID = configAttribute.AttributeID;
            prodAttribute.Label = configAttribute.Label;
            prodAttribute.Descr = configAttribute.Descr;
            prodAttribute.Enabled = false;
            prodAttribute.TransactionRequired = false;
            prodAttribute.Value = configResultAttribute.Value;

            return prodAttribute;
        }

        /// <summary>
        /// Copy configuration attribute DAC fields to a production attribute DAC
        /// </summary>
        /// <param name="configAttribute">Existing configuration attribute DAC record</param>
        /// <param name="configResultAttribute">Existing configuration results attribute DAC record</param>
        /// <returns>new AMProdAttribute DAC record</returns>
        public static AMProdAttribute CopyAttributes(AMConfigurationAttribute configAttribute, AMConfigResultsAttribute configResultAttribute)
        {
            return CopyAttributes(configAttribute, configResultAttribute, new AMProdAttribute());
        }

        /// <summary>
        /// Copy production attribute values from one row to another.
        /// </summary>
        /// <param name="fromAttribute"></param>
        /// <param name="toAttribute"></param>
        /// <returns>Return the toAttribute row</returns>
        public static AMProdAttribute CopyAttributes(AMProdAttribute fromAttribute, AMProdAttribute toAttribute)
        {
            if (fromAttribute == null
                || toAttribute == null)
            {
                return toAttribute;
            }

            toAttribute.Level = fromAttribute.Level;
            toAttribute.OperationID = fromAttribute.OperationID;
            toAttribute.Source = fromAttribute.Source;
            toAttribute.AttributeID = fromAttribute.AttributeID;
            toAttribute.Label = fromAttribute.Label;
            toAttribute.Descr = fromAttribute.Descr;
            toAttribute.Enabled = fromAttribute.Enabled;
            toAttribute.TransactionRequired = fromAttribute.TransactionRequired;
            toAttribute.Value = fromAttribute.Value;
            return toAttribute;
        }
        #endregion

        public static void SetPhtmBomReferences<Table>(ref Table prodRow, IBomOper bomRow, int? lineId) where Table : IBqlTable, IPhantomBomReference
        {
            prodRow.PhtmBOMID = bomRow.BOMID;
            prodRow.PhtmBOMRevisionID = bomRow.RevisionID;
            prodRow.PhtmBOMOperationID = bomRow.OperationID;
            prodRow.PhtmBOMLineRef = lineId;
            prodRow.PhtmLevel = 0;
        }
    }
}
