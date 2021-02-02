namespace PX.Objects.AM
{
    /// <summary>
    /// Maps Estimate records to and from BOM records
    /// </summary>
    public static class EstimateBomMap
    {
        public static AMBomOper CopyOperToBom(AMEstimateOper estimateOper, AMBomOper bomOper)
        {
            if (bomOper == null)
            {
                bomOper = new AMBomOper();
            }

            if (estimateOper == null)
            {
                return bomOper;
            }

            bomOper.OperationCD = estimateOper.OperationCD;
            bomOper.BFlush = estimateOper.BackFlushLabor;
            bomOper.Descr = estimateOper.Description;
            bomOper.MachineUnits = estimateOper.MachineUnits;
            bomOper.MachineUnitTime = estimateOper.MachineUnitTime;
            bomOper.QueueTime = estimateOper.QueueTime;
            bomOper.RunUnits = estimateOper.RunUnits;
            bomOper.RunUnitTime = estimateOper.RunUnitTime;
            bomOper.SetupTime = estimateOper.SetupTime;
            bomOper.WcID = estimateOper.WorkCenterID;
            bomOper.ScrapAction = Attributes.ScrapAction.NoAction;
            bomOper.OutsideProcess = estimateOper.OutsideProcess;
            bomOper.DropShippedToVendor = estimateOper.DropShippedToVendor;
            bomOper.VendorID = estimateOper.VendorID;
            bomOper.VendorID = estimateOper.VendorLocationID;

            return bomOper;
        }

        public static AMBomOper CopyOperToBom(AMEstimateOper estimateOper)
        {
            return CopyOperToBom(estimateOper, new AMBomOper());
        }

        public static AMBomMatl CopyMatlToBom(AMEstimateMatl estimateMatl, AMBomMatl bomMatl)
        {
            if (bomMatl == null)
            {
                bomMatl = new AMBomMatl();
            }

            if (estimateMatl == null 
                || estimateMatl.IsNonInventory.GetValueOrDefault())
            {
                return bomMatl;
            }

            bomMatl.BFlush = estimateMatl.BackFlush;
            bomMatl.Descr = estimateMatl.ItemDesc;
            bomMatl.InventoryID = estimateMatl.InventoryID;
            bomMatl.SubItemID = estimateMatl.SubItemID;
            bomMatl.MaterialType = estimateMatl.MaterialType;
            bomMatl.PhantomRouting = estimateMatl.PhantomRouting;
            bomMatl.ScrapFactor = estimateMatl.ScrapFactor;
            bomMatl.QtyReq = estimateMatl.QtyReq;
            bomMatl.UOM = estimateMatl.UOM;
            bomMatl.SiteID = estimateMatl.SiteID;
            bomMatl.LocationID = estimateMatl.LocationID;
            bomMatl.BatchSize = estimateMatl.BatchSize;
            bomMatl.SubcontractSource = estimateMatl.SubcontractSource;

            // Do not set the UnitCost - let it rebuild from the PXDefault to get the latest unit cost
            //prodMatl.UnitCost = bomMatl.UnitCost;

            return bomMatl;
        }

        public static AMBomMatl CopyMatlToBom(AMEstimateMatl estimateMatl)
        {
            return CopyMatlToBom(estimateMatl, new AMBomMatl());
        }


        public static AMBomOvhd CopyOvhdToBom(AMEstimateOvhd estimateOvhd, AMBomOvhd bomOvhd)
        {
            if (bomOvhd == null)
            {
                bomOvhd = new AMBomOvhd();
            }

            if (estimateOvhd == null)
            {
                return bomOvhd;
            }

            bomOvhd.OFactor = estimateOvhd.OFactor;
            bomOvhd.OvhdID = estimateOvhd.OvhdID;

            return bomOvhd;
        }


        public static AMBomOvhd CopyOvhdToBom(AMEstimateOvhd estimateOvhd)
        {
            return CopyOvhdToBom(estimateOvhd, new AMBomOvhd());
        }

        public static AMBomTool CopyToolToBom(AMEstimateTool estimateTool, AMBomTool bomTool)
        {
            if (bomTool == null)
            {
                bomTool = new AMBomTool();
            }

            if (estimateTool == null)
            {
                return bomTool;
            }

            bomTool.Descr = estimateTool.Description;
            bomTool.QtyReq = estimateTool.QtyReq;
            bomTool.ToolID = estimateTool.ToolID;
            bomTool.UnitCost = estimateTool.UnitCost;

            return bomTool;
        }

        public static AMBomTool CopyToolToBom(AMEstimateTool estimateTool)
        {
            return CopyToolToBom(estimateTool, new AMBomTool());
        }

        public static AMBomStep CopyStepToBom(AMEstimateStep estimateStep, AMBomStep bomStep)
        {
            if (bomStep == null)
            {
                bomStep = new AMBomStep();
            }

            if (estimateStep == null)
            {
                return bomStep;
            }

            bomStep.Descr = estimateStep.Description;
            
            return bomStep;
        }

        public static AMBomStep CopyStepToBom(AMEstimateStep estimateStep)
        {
            return CopyStepToBom(estimateStep, new AMBomStep());
        }
    }
}