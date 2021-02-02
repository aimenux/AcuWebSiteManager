using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Build Manufacturing material rows for use in material transactions such as material wizard and backflush of material based on qty on hand
    /// </summary>
    public class MaterialTranBuilder
    {
        /// <summary>
        /// Calling graph
        /// </summary>
        protected PXGraph _graph;
        /// <summary>
        /// Storage of previously processed transaction lines.
        /// Could be from within this class process or added from external sources.
        /// </summary>
        protected List<AMMTran> _AllBuiltTranLines;
        // <summary>
        /// Are the transactions built for lot/serial status qty? (when true - and given item is L/S tracked).
        /// Or only at the location level (when false).
        /// Default = false
        /// </summary>
        protected bool _StatusByLotSerial;
        /// <summary>
        /// Indicates if lot/serial tracking feature is enabled
        /// </summary>
        private bool _lotSerialTracking = PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>();

        public MaterialTranBuilder(PXGraph graph)
        {
            _graph = graph ?? throw new PXArgumentException(nameof(graph));
            _AllBuiltTranLines = new List<AMMTran>();
            ExcludePreviousTransactions = true;
            _StatusByLotSerial = false;
        }

        /// <summary>
        /// Should previous build AMMTran transaction lines be excluded from available qty for matching item/site/location/lot serial?
        /// Default = true
        /// </summary>
        public bool ExcludePreviousTransactions { get; set; }

        /// <summary>
        /// Are the transactions built for lot/serial status qty? (when true - and given item is L/S tracked).
        /// Or only at the location level (when false).
        /// Default = false
        /// </summary>
        public bool StatusByLotSerial
        {
            get { return _StatusByLotSerial && _lotSerialTracking; }
            set { _StatusByLotSerial = value; }
        }

        /// <summary>
        /// Store transaction line to list of built tran lines for excluding qty later.
        /// </summary>
        /// <param name="ammTran">new transaction to add</param>
        public void AddBuiltTranLine(AMMTran ammTran)
        {
            if (ammTran == null)
            {
                return;
            }

            _AllBuiltTranLines.Add(ammTran);
        }

        /// <summary>
        /// Store transaction lines to list of built tran lines for excluding qty later.
        /// </summary>
        /// <param name="ammTrans">new transactions to add</param>
        public void AddBuiltTranLines(List<AMMTran> ammTrans)
        {
            if (ammTrans == null || ammTrans.Count == 0)
            {
                return;
            }

            _AllBuiltTranLines.AddRange(ammTrans);
        }

        /// <summary>
        /// Clear built list of transaction lines
        /// </summary>
        public void ClearBuiltTranLines()
        {
            _AllBuiltTranLines = new List<AMMTran>();
        }

        public virtual List<AMMTran> BuildTransactions(AMProdMatl prodMatl, decimal totalIssueQty, string issueUom, int? siteID, out string exception)
        {
            return BuildTransactions(prodMatl, totalIssueQty, issueUom, siteID, null, out exception);
        }

        /// <summary>
        /// Build a set of transaction lines based on item and available locations
        /// </summary>
        /// <param name="prodMatl">Production material record</param>
        /// <param name="totalIssueQty">Transaction qty to issue for given item/warehouse</param>
        /// <param name="issueUom">Transaction unit of measure</param>
        /// <param name="siteID">warehouse</param>
        /// <param name="locationID">optional bin location. If no location specified the defaults then pick priory bins</param>
        /// <returns>new built transaction lines</returns>
        public virtual List<AMMTran> BuildTransactions(AMProdMatl prodMatl, decimal totalIssueQty, string issueUom, int? siteID, int? locationID, out string exception)
        {
            if (prodMatl == null)
            {
                throw new PXArgumentException(nameof(prodMatl));
            }

            if (prodMatl.IsByproduct.GetValueOrDefault())
            {
                exception = null;
                return BuildMaterialByProductTransactions(prodMatl, totalIssueQty, issueUom, siteID, locationID);
            }

            var itemResult = InventoryHelper.GetItemLotSerClass(_graph, prodMatl.InventoryID);

            return BuildMaterialIssueTransactions(prodMatl, itemResult, itemResult, totalIssueQty, issueUom, siteID, locationID, out exception);
        }

        /// <summary>
        /// Build a set of transaction lines based on item and available locations
        /// </summary>
        /// <param name="prodMatl">Production material record</param>
        /// <param name="inventoryItem">Inventory item record related to prodMatl record</param>
        /// <param name="lotSerClass">Item lot serial class record related to inventoryItem record</param>
        /// <param name="totalIssueQty">Transaction qty to issue for given item/warehouse</param>
        /// <param name="issueUom">Transaction unit of measure</param>
        /// <param name="siteID">warehouse</param>
        /// <param name="locationID">optional bin location. If no location specified the defaults then pick priory bins</param>
        /// <returns>new built transaction lines</returns>
        public virtual List<AMMTran> BuildTransactions(AMProdMatl prodMatl, InventoryItem inventoryItem, INLotSerClass lotSerClass, decimal totalIssueQty, string issueUom, int? siteID, int? locationID, out string exception)
        {
            exception = null;
            if (prodMatl != null && prodMatl.IsByproduct.GetValueOrDefault())
            {
                return BuildMaterialByProductTransactions(prodMatl, totalIssueQty, issueUom, siteID, locationID);
            }

            if (totalIssueQty < 0)
            {
                return BuildMaterialReturnTransactions(prodMatl, inventoryItem, lotSerClass, totalIssueQty, issueUom, siteID, locationID, out exception);
            }
            
            return BuildMaterialIssueTransactions(prodMatl, inventoryItem, lotSerClass, totalIssueQty, issueUom, siteID, locationID, out exception);
        }

        public virtual List<AMMTran> BuildMaterialReturnTransactions(AMProdMatl prodMatl, InventoryItem inventoryItem,
            INLotSerClass lotSerClass, decimal totalIssueQty, string issueUom, int? siteID, int? locationID, out string exception)
        {
            exception = null;
            var retList = new List<AMMTran>();

            if (prodMatl?.OperationID == null || prodMatl.IsByproduct ==  true)
            {
                // returns cannot occur without a production matl record. If one doesn't exists and returning (negative qty) this is assumed as an on the fly by product which would not be this method
                return retList;
            }

            var isLotSerTracked = lotSerClass.LotSerTrack != INLotSerTrack.NotNumbered;
            var rtBaseQty = isLotSerTracked ? 0m : GetMatchingBaseQtyFromTrans(prodMatl, _AllBuiltTranLines);
            var remainingIssueQty = Math.Abs(totalIssueQty);

            foreach (PXResult<AMMTranSplit, AMMTran, InventoryItem> result in PXSelectJoin<
                AMMTranSplit,
                InnerJoin<AMMTran, 
                    On<AMMTranSplit.docType, Equal<AMMTran.docType>,
                    And<AMMTranSplit.batNbr, Equal<AMMTran.batNbr>,
                    And<AMMTranSplit.lineNbr, Equal<AMMTran.lineNbr>>>>,
                InnerJoin<InventoryItem,
                    On<AMMTran.inventoryID, Equal<InventoryItem.inventoryID>>>>,
                Where<AMMTran.docType, Equal<AMDocType.material>,
                    And<AMMTran.released, Equal<True>,
                    And<AMMTran.orderType, Equal<Required<AMMTran.orderType>>,
                    And<AMMTran.prodOrdID, Equal<Required<AMMTran.prodOrdID>>,
                    And<AMMTran.operationID, Equal<Required<AMMTran.operationID>>,
                    And<AMMTran.matlLineId, Equal<Required<AMMTran.matlLineId>>>>>>>>,
                OrderBy<
                    Desc<AMMTranSplit.tranDate, 
                    Desc<AMMTranSplit.docType, 
                    Desc<AMMTranSplit.batNbr, 
                    Desc<AMMTranSplit.lineNbr, 
                    Desc<AMMTranSplit.splitLineNbr>>>>>>
            >
                .Select(_graph, prodMatl.OrderType, prodMatl.ProdOrdID, prodMatl.OperationID, prodMatl.LineID))
            {
                if (remainingIssueQty <= 0)
                {
                    break;
                }

                var split = (AMMTranSplit) result;
                var tran = (AMMTran) result;

                if (split?.InventoryID == null
                    || tran?.InventoryID == null
                    // We are ignoring non backflush transactions.
                    || tran.OrigBatNbr == null)
                {
                    continue;
                }

                var isReturn = tran.Qty < 0;

                var splitBaseQty = Math.Abs(split.BaseQty.GetValueOrDefault()) * (isReturn ? -1 : 1);

                if (!isReturn && isLotSerTracked)
                {
                    // When transactions are split into multiple lines in the batch we need to make sure we are not over returning the lot/serial nbr from previous lines inserted.
                    var cachedQty = GetCachedLotSerialBaseQty(split);
                    if (cachedQty != 0m)
                    {
                        splitBaseQty += cachedQty;
                    }
                }

                rtBaseQty += splitBaseQty;

                if (splitBaseQty <= 0 || rtBaseQty <= 0)
                {
                    //this is already a return or already returned this qty
                    continue;
                }

                var returnBaseQty = Math.Min(Math.Min(rtBaseQty, remainingIssueQty), Math.Abs(splitBaseQty));
                
                retList.Add(MakeReturn(tran, split, (InventoryItem)result, returnBaseQty * -1));
                
                remainingIssueQty -= returnBaseQty;
                rtBaseQty -= returnBaseQty;
            }

            return retList;
        }

        protected virtual AMMTran MakeReturn(AMMTran tran, AMMTranSplit split, InventoryItem inventoryItem, decimal returnBaseQty)
        {
            if (tran == null || split == null || inventoryItem == null)
            {
                return null;
            }

            var returnTran = PXCache<AMMTran>.CreateCopy(tran);
            returnTran.BatNbr = null;
            returnTran.LineNbr = null;
            returnTran.TranType = AMTranType.Return;
            returnTran.InvtMult = 1;
            returnTran.MultDiv = null;
            returnTran.NoteID = null;
            returnTran.FinPeriodID = null;
            returnTran.TranPeriodID = null;
            returnTran.TranDate = null;
            returnTran.OrigDocType = tran.DocType;
            returnTran.OrigBatNbr = tran.BatNbr;
            returnTran.OrigLineNbr = tran.LineNbr;
            returnTran.Released = null;
            returnTran.CreatedByID = null;
            returnTran.CreatedByScreenID = null;
            returnTran.CreatedDateTime = null;
            returnTran.LotSerCntr = null;
            returnTran.LineCntrAttribute = null;
            returnTran.GLBatNbr = null;
            returnTran.GLLineNbr = null;
            returnTran.INDocType = null;
            returnTran.INBatNbr = null;
            returnTran.INLineNbr = null;

            returnTran.LotSerialNbr = string.IsNullOrWhiteSpace(split.LotSerialNbr) ? null : split.LotSerialNbr;
            returnTran.LocationID = split.LocationID;

            returnTran.BaseQty = returnBaseQty;
            returnTran.Qty = returnBaseQty;
            if (inventoryItem.BaseUnit != returnTran.UOM)
            {
                //alternate UOM
                returnTran.Qty = ConvertToTranUOM<AMMTran.inventoryID>(_graph.Caches<AMMTran>(), returnTran, inventoryItem,
                    returnBaseQty, returnTran.UOM);
            }
            
            return returnTran;
        }

        private decimal GetMatchingBaseQtyFromTrans(AMProdMatl prodMatl, List<AMMTran> trans)
        {
            var qty = 0m;
            if (trans == null)
            {
                return qty;
            }

            foreach (var tran in trans)
            {
                if (!tran.IsSameMatl(prodMatl))
                {
                    continue;
                }

                qty += tran.BaseQty.GetValueOrDefault();
            }
            return qty;
        }

        private decimal GetCachedLotSerialBaseQty(AMMTranSplit split)
        {
            return GetCachedLotSerialBaseQty(split?.InventoryID, split?.SubItemID, split?.SiteID, split?.LocationID, split?.LotSerialNbr);
        }

        private decimal GetCachedLotSerialBaseQty(int? inventoryID, int? subItemID, int? siteID, int? locationID, string lotSerialNbr)
        {
            var totalBaseQty = 0m;
            if (_AllBuiltTranLines == null)
            {
                return totalBaseQty;
            }

            foreach (var tran in _AllBuiltTranLines)
            {
                if (!tran.LotSerialNbr.EqualsWithTrim(lotSerialNbr) || tran.InventoryID != inventoryID ||
                    tran.SiteID != siteID || tran.LocationID != locationID)
                {
                    continue;
                }

                if (InventoryHelper.SubItemFeatureEnabled && tran.SubItemID != subItemID)
                {
                    continue;
                }

                totalBaseQty += tran.BaseQty.GetValueOrDefault();
            }

            return totalBaseQty;
        }

        /// <summary>
        /// Build a set of transaction lines for by product transaction
        /// </summary>
        /// <param name="prodMatl">Production material record</param>
        /// <param name="totalIssueQty">Transaction qty to issue for given item/warehouse</param>
        /// <param name="issueUom">Transaction unit of measure</param>
        /// <param name="siteID">warehouse</param>
        /// <param name="locationID">optional bin location. If no location specified the defaults then pick priory bins</param>
        /// <returns>new built transaction lines</returns>
        public virtual List<AMMTran> BuildMaterialByProductTransactions(AMProdMatl prodMatl, decimal totalIssueQty, string issueUom, int? siteID, int? locationID)
        {
            var ammTrans = new List<AMMTran>();
            if (totalIssueQty == 0)
            {
                return ammTrans;
            }

            return new List<AMMTran>
            {
                CreateAMMTran(prodMatl, totalIssueQty < 0 ? totalIssueQty : totalIssueQty * -1 , prodMatl.UOM, siteID, locationID ?? GetDefaultReceiptLocationID(prodMatl.InventoryID, siteID, true))
            };
        }

        /// <summary>
        /// Build a set of transaction lines based on item and available locations
        /// </summary>
        /// <param name="prodMatl">Production material record</param>
        /// <param name="totalIssueQty">Transaction qty to issue for given item/warehouse</param>
        /// <param name="issueUom">Transaction unit of measure</param>
        /// <param name="siteID">warehouse</param>
        /// <param name="locationID">optional bin location. If no location specified the defaults then pick priory bins</param>
        /// <returns>new built transaction lines</returns>
        public virtual List<AMMTran> BuildMaterialIssueTransactions(AMProdMatl prodMatl, decimal totalIssueQty, string issueUom, int? siteID, int? locationID, out string exception)
        {
            if (prodMatl == null)
            {
                throw new PXArgumentException(nameof(prodMatl));
            }

            var itemResult = (PXResult<InventoryItem, INLotSerClass>)PXSelectJoin<InventoryItem,
                LeftJoin<INLotSerClass, On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>>,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.SelectWindowed(_graph, 0, 1, prodMatl.InventoryID);

            return BuildMaterialIssueTransactions(prodMatl, itemResult, itemResult, totalIssueQty, issueUom, siteID, locationID, out exception);
        }

        /// <summary>
        /// Build a set of transaction lines based on item and available locations
        /// </summary>
        /// <param name="prodMatl">Production material record</param>
        /// <param name="inventoryItem">Inventory item record related to prodMatl record</param>
        /// <param name="lotSerClass">Item lot serial class record related to inventoryItem record</param>
        /// <param name="totalIssueQty">Transaction qty to issue for given item/warehouse</param>
        /// <param name="issueUom">Transaction unit of measure</param>
        /// <param name="siteID">warehouse</param>
        /// <param name="locationID">optional bin location. If no location specified the defaults then pick priory bins</param>
        /// <returns>new built transaction lines</returns>
        public virtual List<AMMTran> BuildMaterialIssueTransactions(AMProdMatl prodMatl, InventoryItem inventoryItem, INLotSerClass lotSerClass, decimal totalIssueQty, string issueUom, int? siteID, int? locationID, out string exception)
        {
            exception = null;

            var ammTrans = new List<AMMTran>();

            if (totalIssueQty == 0)
            {
                return ammTrans;
            }

            if (prodMatl == null || prodMatl.InventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(prodMatl));
            }

            if (prodMatl.QtyReq.GetValueOrDefault() <= 0 || prodMatl.IsByproduct.GetValueOrDefault())
            {
                PXTrace.WriteWarning("Process does not allow negative quantity or by-product item. Production Order {0} {1} Operation ({2}) Material LineNbr {3}", 
                    prodMatl.OrderType, prodMatl.ProdOrdID.TrimIfNotNullEmpty(), prodMatl.OperationID, prodMatl.LineID);
                return ammTrans;
            }

            if (inventoryItem == null || inventoryItem.InventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryItem));
            }

            if (string.IsNullOrWhiteSpace(issueUom))
            {
                throw new PXArgumentException(nameof(issueUom));
            }

            if (siteID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            if (lotSerClass == null || string.IsNullOrWhiteSpace(lotSerClass.LotSerClassID))
            {
                lotSerClass = new INLotSerClass
                {
                    LotSerClassID = string.Empty, 
                    LotSerTrack = INLotSerTrack.NotNumbered,
                    LotSerIssueMethod = INLotSerIssueMethod.UserEnterable
                };
            }

            var remainingQty = totalIssueQty;
            if (inventoryItem.StkItem.GetValueOrDefault())
            {
                //Get the split records to determine what lots are hard allocated to this specific prod order
                var splits = AMProdMatl.GetSplits(_graph, prodMatl);

                if (StatusByLotSerial 
                    && lotSerClass.LotSerTrack != INLotSerTrack.NotNumbered 
                    && lotSerClass.LotSerIssueMethod != INLotSerIssueMethod.UserEnterable
                    && lotSerClass.LotSerAssign != INLotSerAssign.WhenUsed)
                {
                    //Need to first pull from allocated qty first...
                    if(splits != null)
                    {
                        foreach (var allocatedSplit in splits.Where(x => x.IsAllocated == true && x.LotSerialNbr != null))
                        {
                            CreateAMMTranLotSerial(_graph.Caches<AMProdMatl>(), prodMatl, splits, inventoryItem, lotSerClass, issueUom, siteID, 
                                allocatedSplit.LocationID, allocatedSplit.LotSerialNbr, ref ammTrans, ref remainingQty);
                        }
                    }

                    if (remainingQty > 0)
                    {
                        // We only want the default locations if one isn't requested, and if the part is not using expiration date
                        var defaultLocations = (locationID == null && lotSerClass.LotSerIssueMethod != INLotSerIssueMethod.Expiration) ?
                            InventoryHelper.DfltLocation.GetDefaults(_graph, InventoryHelper.DfltLocation.BinType.Ship, prodMatl.InventoryID, siteID, true) :
                            new List<int>();

                        foreach (var defaultLocation in defaultLocations)
                        {
                            //
                            //  Here we will pull from the items default locations first (Item Warehouse, Item (matching site), Warehouse).
                            //      The defaults are unique so the bin locations will not re-occur
                            //
                            CreateAMMTranLotSerial(_graph.Caches<AMProdMatl>(), prodMatl, splits, inventoryItem, lotSerClass, issueUom, siteID,
                                defaultLocation, null, ref ammTrans, ref remainingQty);
                        }

                        CreateAMMTranLotSerial(_graph.Caches<AMProdMatl>(), prodMatl, splits, inventoryItem, lotSerClass, issueUom, siteID, 
                            locationID, null, ref ammTrans, ref remainingQty);
                    }
                }
                else
                {
                    //Need to first pull from allocated qty first...
                    if (splits != null)
                    {
                        foreach (var allocatedSplit in splits.Where(x => x.IsAllocated == true))
                        {
                            CreateAMMTranLocation(_graph.Caches<AMProdMatl>(), prodMatl, splits, inventoryItem, issueUom, siteID, allocatedSplit.LocationID, ref ammTrans, ref remainingQty);
                        }
                    }

                    if (remainingQty > 0)
                    {
                        // We only want the default locations if one isn't requested
                        var defaultLocations = locationID == null ?
                            InventoryHelper.DfltLocation.GetDefaults(_graph, InventoryHelper.DfltLocation.BinType.Ship, prodMatl.InventoryID, siteID, true) :
                            new List<int>();

                        foreach (var defaultLocation in defaultLocations)
                        {
                            //
                            //  Here we will pull from the items default locations first (Item Warehouse, Item (matching site), Warehouse).
                            //      The defaults are unique so the bin locations will not re-occur
                            //
                            CreateAMMTranLocation(_graph.Caches<AMProdMatl>(), prodMatl, splits, inventoryItem, issueUom, siteID, defaultLocation, ref ammTrans, ref remainingQty);
                        }

                        CreateAMMTranLocation(_graph.Caches<AMProdMatl>(), prodMatl, splits, inventoryItem, issueUom, siteID, locationID, ref ammTrans, ref remainingQty);
                    }
                }
            }

            if (remainingQty > 0 
                && (inventoryItem.NegQty.GetValueOrDefault()
                 || !inventoryItem.StkItem.GetValueOrDefault()))
            {
                ammTrans.Add(CreateAMMTran(prodMatl, remainingQty, issueUom, siteID, locationID ?? GetDefaultIssueLocationID(prodMatl.InventoryID, siteID, true)));
                remainingQty = 0m;
            }

            if (ExcludePreviousTransactions)
            {
                AddBuiltTranLines(ammTrans);
            }

            if (remainingQty > 0)
            {
                var prodOper = (AMProdOper) PXSelect<
                        AMProdOper, 
                        Where<AMProdOper.orderType, Equal<Required<AMProdOper.orderType>>,
                            And<AMProdOper.prodOrdID, Equal<Required<AMProdOper.prodOrdID>>,
                            And<AMProdOper.operationID, Equal<Required<AMProdOper.operationID>>>>>>
                        .Select(_graph, prodMatl.OrderType, prodMatl.ProdOrdID, prodMatl.OperationID);

                exception = Messages.GetLocal(Messages.BackFlushMaterialShortage,
                    prodMatl.UOM.TrimIfNotNullEmpty(),
                    UomHelper.FormatQty(remainingQty),
                    inventoryItem.InventoryCD.TrimIfNotNullEmpty(),
                    prodMatl.OrderType,
                    prodMatl.ProdOrdID.TrimIfNotNullEmpty(),
                    prodOper?.OperationCD ?? $"({prodMatl.OperationID})",
                    prodMatl.LineNbr);
            }

            return ammTrans;
        }

        protected virtual void CreateAMMTranLocation(PXCache cache, AMProdMatl prodMatl, List<AMProdMatlSplit> splits, InventoryItem inventoryItem, string issueUom,
            int? siteId, int? locationId, ref List<AMMTran> ammTrans, ref decimal remainingQty)
        {
            if (remainingQty <= 0 || prodMatl?.InventoryID == null || inventoryItem?.InventoryID == null || string.IsNullOrWhiteSpace(issueUom))
            {
                return;
            }

            foreach (INLocationStatus locationStatus in GetLocationStatus(_graph, prodMatl.InventoryID, prodMatl.SubItemID, siteId, locationId))
            {
                var lotSplitAlloc = splits?.Where(x => x.IsAllocated == true).Sum(x => x.BaseQty) ?? 0m;
                var processedQty = GetExcludedBaseQty(ammTrans, prodMatl, locationStatus);

                if (locationStatus != null)
                {
                    locationStatus.QtyHardAvail = locationStatus.QtyHardAvail.GetValueOrDefault() + lotSplitAlloc - processedQty;
                }

                var ammTranLoc = CreateAMMTranLocation(_graph.Caches<AMProdMatl>(), prodMatl, inventoryItem, locationStatus, remainingQty, issueUom);
                if (ammTranLoc == null)
                {
                    continue;
                }

                ammTrans.Add(ammTranLoc);
                remainingQty -= ammTranLoc.Qty.GetValueOrDefault();
                if (remainingQty <= 0)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Create a transaction line based on location status
        /// </summary>
        /// <param name="cache">cache for translating uom quantity (use AMProdMatl cache)</param>
        /// <param name="prodMatl">Production material record</param>
        /// <param name="inventoryItem">Inventory item record related to prodMatl record</param>
        /// <param name="locationStatus">Location status record to issue from containing qty on hand</param>
        /// <param name="issueQty">Transaction qty to issue for given locationStatus record</param>
        /// <param name="issueUom">Transaction unit of measure</param>
        /// <returns>new built transaction line</returns>
        protected virtual AMMTran CreateAMMTranLocation(PXCache cache, AMProdMatl prodMatl, InventoryItem inventoryItem, INLocationStatus locationStatus, decimal issueQty, string issueUom)
        {
            if (inventoryItem?.InventoryID == null)
            {
                throw new ArgumentNullException(nameof(inventoryItem));
            }

            var excludedBaseQty = ExcludePreviousTransactions ?
                GetExcludedBaseQty(_AllBuiltTranLines, inventoryItem?.InventoryID, prodMatl.SubItemID, locationStatus.SiteID, locationStatus.LocationID, null) :
                0m;

            var locBaseQty = locationStatus.QtyHardAvail.GetValueOrDefault() - excludedBaseQty;
            if (locBaseQty <= 0)
            {
                return null;
            }

            var issueBaseQty = ConvertToBaseUOM<AMProdMatl.inventoryID>(cache, prodMatl, inventoryItem, issueQty, issueUom);

            if (TryRoundBaseQty(prodMatl, inventoryItem, issueBaseQty, issueQty, issueUom, UomHelper.QtyDecimalPrecision, out var wholeBaseQty))
            {
                issueBaseQty = wholeBaseQty;
            }

            if (locBaseQty > issueBaseQty)
            {
                locBaseQty = issueBaseQty;
            }

            // Converting to base above and then back here will help remove any rounding issues when trying calculate the adjusted issue qty to base units
            var locQty = ConvertToTranUOM<AMProdMatl.inventoryID>(cache, prodMatl, inventoryItem, locBaseQty, issueUom);

            return CreateAMMTran(prodMatl, locationStatus, locQty, issueUom, locBaseQty);
        }

        protected virtual void CreateAMMTranLotSerial(PXCache cache, AMProdMatl prodMatl, List<AMProdMatlSplit> splits, InventoryItem inventoryItem, INLotSerClass lotSerClass, string issueUom,
            int? siteId, int? locationId, string lotSerialNbr, ref List<AMMTran> ammTrans, ref decimal remainingQty)
        {
            if(remainingQty <= 0 || prodMatl?.InventoryID == null || inventoryItem?.InventoryID == null || lotSerClass?.LotSerClassID == null || string.IsNullOrWhiteSpace(issueUom))
            {
                return;
            }

            foreach (var result in GetLotSerialStatusList(lotSerClass, prodMatl.InventoryID, prodMatl.SubItemID, siteId, locationId, lotSerialNbr))
            {
                var lotSerialStatus = (INLotSerialStatus)result;
                var itemLotSerial = (INItemLotSerial)result;
                var lotSplitAlloc = splits?.Where(x => x.IsAllocated == true && x.LotSerialNbr == itemLotSerial.LotSerialNbr).Sum(x => x.BaseQty) ?? 0m;
                var processedQty = GetExcludedBaseQty(ammTrans, prodMatl, lotSerialStatus);

                if (lotSerialStatus != null && itemLotSerial != null)
                {
                    lotSerialStatus.QtyHardAvail = Math.Min(lotSerialStatus.QtyHardAvail.GetValueOrDefault(), itemLotSerial.QtyHardAvail.GetValueOrDefault()) + lotSplitAlloc - processedQty;
                }

                var ammTranLS = CreateAMMTranLotSerial(cache, prodMatl, inventoryItem, lotSerialStatus, remainingQty, issueUom);
                if (ammTranLS == null)
                {
                    continue;
                }

                ammTrans.Add(ammTranLS);
                remainingQty -= ammTranLS.Qty.GetValueOrDefault();
                if (remainingQty <= 0)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Create a transaction line based on lot/serial status
        /// </summary>
        /// <param name="cache">cache for translating uom quantity (use AMProdMatl cache)</param>
        /// <param name="prodMatl">Production material record</param>
        /// <param name="inventoryItem">Inventory item record related to prodMatl record</param>
        /// <param name="lotSerialStatus">Lot/Serial status record to issue from containing qty on hand</param>
        /// <param name="issueQty">Transaction qty to issue for given locationStatus record</param>
        /// <param name="issueUom">Transaction unit of measure</param>
        /// <returns>new built transaction line</returns>
        protected virtual AMMTran CreateAMMTranLotSerial(PXCache cache, AMProdMatl prodMatl, InventoryItem inventoryItem, INLotSerialStatus lotSerialStatus, decimal issueQty, string issueUom)
        {
            if(inventoryItem?.InventoryID == null)
            {
                throw new ArgumentNullException(nameof(inventoryItem));
            }

            var excludedBaseQty = ExcludePreviousTransactions ?
                GetExcludedBaseQty(_AllBuiltTranLines, inventoryItem?.InventoryID, prodMatl.SubItemID, lotSerialStatus.SiteID, lotSerialStatus.LocationID, lotSerialStatus.LotSerialNbr) :
                0m;
 
            var locBaseQty = lotSerialStatus.QtyHardAvail.GetValueOrDefault() - excludedBaseQty;
            if (locBaseQty <= 0)
            {
                return null;
            }

            var issueBaseQty = ConvertToBaseUOM<AMProdMatl.inventoryID>(cache, prodMatl, inventoryItem, issueQty, issueUom);

            if(TryRoundBaseQty(prodMatl, inventoryItem, issueBaseQty, issueQty, issueUom, UomHelper.QtyDecimalPrecision, out var wholeBaseQty))
            {
                issueBaseQty = wholeBaseQty;
            }

            if (locBaseQty > issueBaseQty)
            {
                locBaseQty = issueBaseQty;
            }

            // Converting to base above and then back here will help remove any rounding issues when trying calculate the adjusted issue qty to base units
            var locQty = ConvertToTranUOM<AMProdMatl.inventoryID>(cache, prodMatl, inventoryItem, locBaseQty, issueUom);

            return CreateAMMTran(prodMatl, lotSerialStatus, locQty, issueUom, locBaseQty);
        }

        /// <summary>
        /// Round up the base qty if its found to be a small rounding factor keeping the value from being a whole value
        /// </summary>
        public static bool TryRoundBaseQty(AMProdMatl prodMatl, InventoryItem inventoryItem, decimal baseQty, decimal qty, string tranUom, int qtyPrecision, out decimal baseQtyOut)
        {
            baseQtyOut = baseQty;
            if(prodMatl == null || baseQty == 0 || inventoryItem?.BaseUnit == null || string.IsNullOrEmpty(tranUom) || inventoryItem.BaseUnit.EqualsWithTrim(tranUom))
            {
                return false;
            }

            var convFactor1 = prodMatl.TotalQtyRequired.GetValueOrDefault() == 0
                    ? 1m
                    : prodMatl.BaseTotalQtyRequired.GetValueOrDefault() / prodMatl.TotalQtyRequired.GetValueOrDefault();

            if (convFactor1 == 1 || convFactor1 % 1 != 0)
            {
                return false;
            }

            var remain = Math.Abs(baseQty) % 1;
            if(remain == 0)
            {
                return false;
            }

            var mult = baseQty >= 0 ? 1 : -1;
            var fromWhole = 1 - remain;

            var min = Convert.ToDecimal(Math.Pow(10, qtyPrecision * -1) * 2);

            if (fromWhole <= min)
            {
                baseQtyOut = baseQty + (fromWhole * mult);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get an items default issue/ship warehouse location ID
        /// </summary>
        protected virtual int? GetDefaultIssueLocationID(int? inventoryID, int? siteID, bool returnAnyBin)
        {
            return InventoryHelper.DfltLocation.GetDefault(_graph, InventoryHelper.DfltLocation.BinType.Ship, inventoryID, siteID, returnAnyBin);
        }

        /// <summary>
        /// Get an items default receipt warehouse location ID
        /// </summary>
        protected virtual int? GetDefaultReceiptLocationID(int? inventoryID, int? siteID, bool returnAnyBin)
        {
            return InventoryHelper.DfltLocation.GetDefault(_graph, InventoryHelper.DfltLocation.BinType.Receipt, inventoryID, siteID, returnAnyBin);
        }

        protected static AMMTran CreateAMMTran(AMProdMatl prodMatl, INLocationStatus locationStatus, decimal tranQty, string tranUOM)
        {
            return CreateAMMTran(prodMatl, locationStatus, tranQty, tranUOM, null);
        }

        protected static AMMTran CreateAMMTran(AMProdMatl prodMatl, INLocationStatus locationStatus, decimal tranQty, string tranUOM, decimal? baseQty)
        {
            var ammTran = CreateAMMTran(prodMatl, tranQty, tranUOM, baseQty);

            if (ammTran != null)
            {
                ammTran.SiteID = locationStatus.SiteID;
                ammTran.LocationID = locationStatus.LocationID;
            }

            return ammTran;
        }

        protected static AMMTran CreateAMMTran(AMProdMatl prodMatl, INLotSerialStatus lotSerialStatus, decimal tranQty, string tranUOM)
        {
            return CreateAMMTran(prodMatl, lotSerialStatus, tranQty, tranUOM, null);
        }

        protected static AMMTran CreateAMMTran(AMProdMatl prodMatl, INLotSerialStatus lotSerialStatus, decimal tranQty, string tranUOM, decimal? baseQty)
        {
            var ammTran = CreateAMMTran(prodMatl, tranQty, tranUOM, baseQty);

            if (ammTran != null)
            {
                ammTran.SiteID = lotSerialStatus.SiteID;
                ammTran.LocationID = lotSerialStatus.LocationID;
                ammTran.LotSerialNbr = lotSerialStatus.LotSerialNbr;
                ammTran.ExpireDate = lotSerialStatus.ExpireDate;
            }

            return ammTran;
        }

        protected virtual AMMTran CreateAMMTran(AMProdMatl prodMatl, decimal tranQty, string tranUOM, int? siteID, int? locationID)
        {
            var newTran = CreateAMMTran(prodMatl, tranQty, tranUOM);

            if (newTran != null)
            {
                newTran.SiteID = siteID;
                newTran.LocationID = locationID;
            }

            return newTran;
        }

        protected static AMMTran CreateAMMTran(AMProdMatl prodMatl, decimal tranQty, string tranUOM)
        {
            return CreateAMMTran(prodMatl, tranQty, tranUOM, null);
        }

        protected static AMMTran CreateAMMTran(AMProdMatl prodMatl, decimal tranQty, string tranUOM, decimal? baseQty)
        {
            if (prodMatl == null)
            {
                throw new PXArgumentException(nameof(prodMatl));
            }

            if (tranQty == 0)
            {
                return null;
            }

            var tranBaseQty = baseQty;
            if(tranBaseQty == null)
            {
                //Using TotalQtyRequired only as a method for calculating a conversion factor
                var toBaseQtyConversion = prodMatl.TotalQtyRequired.GetValueOrDefault() == 0
                    ? 1
                    : prodMatl.BaseTotalQtyRequired.GetValueOrDefault() / prodMatl.TotalQtyRequired.GetValueOrDefault();

                tranBaseQty = tranQty * toBaseQtyConversion;
            }

            return new AMMTran
            {
                OrderType = prodMatl.OrderType,
                ProdOrdID = prodMatl.ProdOrdID,
                OperationID = prodMatl.OperationID,
                InventoryID = prodMatl.InventoryID,
                SubItemID = prodMatl.SubItemID,
                MatlLineId = prodMatl.LineID,
                Qty = tranQty,
                UOM = tranUOM,
                BaseQty = tranBaseQty,
                IsByproduct = prodMatl.IsByproduct,
                SubcontractSource = prodMatl.SubcontractSource
            };
        }

        /// <summary>
        /// Query INLocationStatus and return the results
        /// </summary>
        public static PXResultset<INLocationStatus> GetLocationStatus(PXGraph graph, int? inventoryID, int? subitemID, int? siteID, int? locationID)
        {
            return InventoryHelper.GetLocationStatus(graph, inventoryID, subitemID, siteID, locationID, null, true, true);
        }

        /// <summary>
        /// Query INLotSerialStatus and return the results
        /// </summary>
        public static PXResultset<INLotSerialStatus> GetLotSerialStatus(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID)
        {
            return GetLotSerialStatus(graph, lotSerClass, inventoryID, subitemID, siteID, locationID, null);
        }

        /// <summary>
        /// Query INLotSerialStatus and return the results
        /// </summary>
        public static PXResultset<INLotSerialStatus> GetLotSerialStatus(PXGraph graph, INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr)
        {
            return InventoryHelper.GetLotSerialStatus(graph, lotSerClass, inventoryID, subitemID, siteID, locationID, lotSerialNbr, null, true, true);
        }

        protected virtual List<PXResult<INLotSerialStatus, INLocation, INItemLotSerial>> GetLotSerialStatusList(INLotSerClass lotSerClass, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr)
        {
            return InventoryHelper.GetLotSerialStatusList(_graph, lotSerClass, inventoryID, subitemID, siteID, locationID, lotSerialNbr, null, true, true);
        }

        /// <summary>
        /// Find quantity for a given item/warehouse/location that has already been recorded on another transaction line
        /// </summary>
        public static decimal GetExcludedTranQty<InventoryIDField>(PXCache cache, object row, List<AMMTran> ammTrans, InventoryItem inventoryItem, 
            string tranUom, int? subitemID, int? siteID, int? locationID, string lotSerialNbr) where InventoryIDField : IBqlField
        {
            if (inventoryItem == null
                || inventoryItem.InventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryItem));
            }

            var excludedBaseQty = GetExcludedBaseQty(ammTrans, inventoryItem.InventoryID, subitemID, siteID, locationID, lotSerialNbr);
            return ConvertToTranUOM<InventoryIDField>(cache, row, inventoryItem, excludedBaseQty, tranUom);
        }

        /// <summary>
        /// Convert the base quantity to the transaction unit of measure
        /// </summary>
        protected static decimal ConvertToBaseUOM<InventoryIDField>(PXCache cache, object row, InventoryItem inventoryItem, decimal baseQty, string tranUom) where InventoryIDField : IBqlField
        {
            if (tranUom.EqualsWithTrim(inventoryItem.BaseUnit) || baseQty == 0)
            {
                return baseQty;
            }

            return UomHelper.TryConvertToBaseQty<InventoryIDField>(cache, row,
                tranUom,
                baseQty,
                out var tranQty) ? tranQty.GetValueOrDefault() : baseQty;
        }

        /// <summary>
        /// Convert the base quantity to the transaction unit of measure
        /// </summary>
        protected static decimal ConvertToTranUOM<InventoryIDField>(PXCache cache, object row, InventoryItem inventoryItem, decimal baseQty, string tranUom) where InventoryIDField : IBqlField
        {
            if (tranUom.EqualsWithTrim(inventoryItem.BaseUnit) || baseQty == 0)
            {
                return baseQty;
            }

            return UomHelper.TryConvertFromBaseQty<InventoryIDField>(cache, row,
                tranUom,
                baseQty,
                out var tranQty) ? tranQty.GetValueOrDefault() : baseQty;
        }

        protected static decimal GetExcludedBaseQty(List<AMMTran> ammTrans, AMProdMatl prodMatl, INLotSerialStatus lotSerialStatus)
        {
            return GetExcludedBaseQty(ammTrans, prodMatl?.InventoryID, prodMatl?.SubItemID, lotSerialStatus?.SiteID, lotSerialStatus?.LocationID, lotSerialStatus?.LotSerialNbr);
        }

        protected static decimal GetExcludedBaseQty(List<AMMTran> ammTrans, AMProdMatl prodMatl, INLocationStatus locationStatus)
        {
            return GetExcludedBaseQty(ammTrans, prodMatl?.InventoryID, prodMatl?.SubItemID, locationStatus?.SiteID, locationStatus?.LocationID, null);
        }

        /// <summary>
        /// Find matching transactions quantity and return
        /// </summary>
        public static decimal GetExcludedBaseQty(List<AMMTran> ammTrans, int? inventoryID, int? subitemID, int? siteID, int? locationID, string lotSerialNbr)
        {
            var excludedBaseQty = 0m;
            if (ammTrans == null)
            {
                return excludedBaseQty;
            }

            if (inventoryID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(inventoryID));
            }

            if (siteID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(siteID));
            }

            if (locationID.GetValueOrDefault() == 0)
            {
                throw new PXArgumentException(nameof(locationID));
            }

            foreach (var tran in ammTrans)
            {
                if (tran == null)
                {
                    continue;
                }

                if (tran.InventoryID == inventoryID 
                    && (subitemID.GetValueOrDefault() == 0 || tran.SubItemID == subitemID.GetValueOrDefault())
                    && tran.SiteID == siteID
                    && tran.LocationID == locationID
                    && (string.IsNullOrWhiteSpace(lotSerialNbr) 
                        || (!string.IsNullOrWhiteSpace(tran.LotSerialNbr) 
                            && tran.LotSerialNbr.EqualsWithTrim(lotSerialNbr))))
                {
                    excludedBaseQty += tran.BaseQty.GetValueOrDefault();
                }
            }

            return excludedBaseQty;
        }

        public static void CreateMaterialTransaction(MaterialEntry materialEntry, List<AMMTran> newTransactions, AMMTran origAmmTran)
        {
            if (materialEntry == null)
            {
                throw new ArgumentNullException(nameof(materialEntry));
            }

            var currentBatch = materialEntry?.batch?.Current;
            if (currentBatch != null && currentBatch.Released.GetValueOrDefault())
            {
                throw new PXException(Messages.BatchIsReleased, AMDocType.GetDocTypeDesc(currentBatch.DocType), currentBatch.BatNbr);
            }

            using (new DisableSelectorValidationScope(materialEntry.transactions.Cache))
            using (new DisableSelectorValidationScope(materialEntry.splits.Cache))
            {
                foreach (var mergeMaterialTransaction in MergeMaterialTransactions(newTransactions))
                {
                    materialEntry.InsertNewTransaction(mergeMaterialTransaction.Item1, mergeMaterialTransaction.Item2, origAmmTran);
                }
            }
        }

        private static IEnumerable<Tuple<AMMTran, List<AMMTranSplit>>> MergeMaterialTransactions(List<AMMTran> newTransactions)
        {
            var ret = new List<Tuple<AMMTran, List<AMMTranSplit>>>();
            var remainingTransactions = new List<AMMTran>(newTransactions);
            var safetycntr = 0;
            while (remainingTransactions.Count > 0 || safetycntr >= 9999)
            {
                safetycntr++;
                var currentTran = ConsolidateNextRemainingTranLines(remainingTransactions, out remainingTransactions, out var tranSplits);
                if (currentTran == null)
                {
                    continue;
                }
                ret.Add(new Tuple<AMMTran, List<AMMTranSplit>>(currentTran, tranSplits));
            }

            return ret;
        }

        private static AMMTran ConsolidateNextRemainingTranLines(IReadOnlyCollection<AMMTran> allTrans, out List<AMMTran> allTransRemainder, out List<AMMTranSplit> currentTranSplits)
        {
            allTransRemainder = new List<AMMTran>();
            currentTranSplits = new List<AMMTranSplit>();
     
            if (allTrans == null || allTrans.Count == 0)
            {
                return null;
            }

            var isReturn = false;
            AMMTran currentTran = null;
            foreach (var tran in allTrans)
            {
                if (currentTran == null && tran != null)
                {
                    isReturn = tran.Qty < 0;
                    currentTran = tran;
                    if (isReturn)
                    {
                        currentTran.Qty *= -1;
                        currentTran.BaseQty *= -1;
                    }

                    if (!string.IsNullOrWhiteSpace(currentTran?.LotSerialNbr))
                    {
                        var split = (AMMTranSplit) currentTran;
                        split.OrigSource = currentTran.OrigDocType;
                        split.OrigBatNbr = currentTran.OrigBatNbr;
                        split.OrigLineNbr = currentTran.OrigLineNbr;

                        currentTranSplits.Add(split);
                    }
                    continue;
                }

                if (currentTran == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(currentTran.LotSerialNbr) && IsSameMaterialTranReference(currentTran, tran))
                {
                    var tranSplit = (AMMTranSplit) tran;
                    if (!string.IsNullOrWhiteSpace(tranSplit?.LotSerialNbr))
                    {
                        tranSplit.OrigSource = currentTran.OrigDocType;
                        tranSplit.OrigBatNbr = currentTran.OrigBatNbr;
                        tranSplit.OrigLineNbr = currentTran.OrigLineNbr;
                        currentTranSplits.Add(tranSplit);
                        currentTran.Qty += tranSplit.Qty.GetValueOrDefault();
                        currentTran.BaseQty += tranSplit.BaseQty.GetValueOrDefault();
                        continue;
                    }
                }

                allTransRemainder.Add(tran);
            }

            if (isReturn && currentTran.Qty.GetValueOrDefault() > 0)
            {
                // For return material the allocations/lot serial are positive even if the tran line is negative.
                currentTran.Qty *= -1;
                currentTran.BaseQty *= -1;
            }

            return currentTran;
        }

        private static bool IsSameMaterialTranReference(AMMTran matlTran1, AMMTran matlTran2)
        {
            return matlTran1 != null && matlTran2 != null &&
                   matlTran1.OrderType == matlTran2.OrderType &&
                   matlTran1.ProdOrdID == matlTran2.ProdOrdID &&
                   matlTran1.OperationID == matlTran2.OperationID &&
                   matlTran1.InventoryID == matlTran2.InventoryID &&
                   matlTran1.SubItemID == matlTran2.SubItemID &&
                   matlTran1.MatlLineId == matlTran2.MatlLineId &&
                   matlTran1.UOM == matlTran2.UOM;
        }
    }
}