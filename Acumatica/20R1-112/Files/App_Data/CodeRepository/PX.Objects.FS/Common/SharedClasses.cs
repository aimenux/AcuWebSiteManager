using PX.Data;
using PX.Objects.AR;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class SharedClasses
    {
        public class TransactionScopeException : Exception
        {
        }

        public class CpnyLocationDistance
        {
            public CpnyLocationDistance(FSBranchLocation fsBranchLocation, string address, int distance)
            {
                this.fsBranchLocation = fsBranchLocation;
                this.address          = address;
                this.distance         = distance;
            }

            public FSBranchLocation fsBranchLocation;
            public string address;
            public int distance;
        }

        public class RouteSelected_view : PXSelectJoin<FSRouteDocument,
                                          InnerJoin<FSRoute,
                                          On<
                                              FSRoute.routeID, Equal<FSRouteDocument.routeID>>>,
                                          Where<
                                              FSRouteDocument.routeDocumentID, Equal<Current<FSRouteDocument.routeDocumentID>>>>
        {
            public RouteSelected_view(PXGraph graph) : base(graph)
            {
            }

            public RouteSelected_view(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        #region PostingInvoiceClasses

        public class InvoiceGroup
        {
            public int Pivot;
            public string BatchNbr;
            public string DocumentType;
            public string DocumentRefNbr;
            public bool? PostToAppNegBalances;
            public int? BillingCycle;

            public decimal? TotalSumAR_AP;
            public decimal? TotalSumSO;
            public List<InvoiceItem> ItemsToPost;

            public InvoiceGroup(int pivot, bool? postToAppNegBalances, List<InvoiceItem> invoiceItemList)
            {
                this.BatchNbr = null;
                this.DocumentType = null;
                this.DocumentRefNbr = null;
                this.Pivot = pivot;
                this.ItemsToPost = invoiceItemList;
                this.PostToAppNegBalances = postToAppNegBalances;

                this.BillingCycle = invoiceItemList[0] != null && invoiceItemList[0].FSAppointmentRow  != null ? invoiceItemList[0].FSAppointmentRow.BillingCycleID : null;

                if (this.BillingCycle == null)
                {
                    this.BillingCycle = invoiceItemList[0] != null && invoiceItemList[0].FSServiceOrderRow != null ? invoiceItemList[0].FSServiceOrderRow.BillingCycleID : null;
                }

                this.TotalSumAR_AP = invoiceItemList.Sum(x => x.FSPostingLineDetailsToPostInAR_AP.Sum(y => y.CuryTranAmt));
                this.TotalSumSO = invoiceItemList.Sum(x => x.FSPostingLineDetailsToPostInSO.Sum(y => y.CuryTranAmt));
            }
        }

        public class AppointmentInfo
        {
            public AppointmentToPost FSAppointmentRow;
            public ServiceOrderToPost FSServiceOrderRow;
            public FSSrvOrdType FSSrvOrdTypeRow;

            public PXResultset<FSAppointmentDet> FSAppointmentDetToPostInAR_AP;
            public PXResultset<FSAppointmentDet> FSAppointmentDetToPostInSO;
            public PXResultset<FSAppointmentDet> FSAppointmentInventoryItemToPostIn_AR_AP;
            public PXResultset<FSAppointmentDet> FSAppointmentInventoryItemToPostInSO;

            public AppointmentInfo()
            {
            }

            public AppointmentInfo(AppointmentToPost fsAppointmentRow,
                                   ServiceOrderToPost fsServiceOrderRow,
                                   FSSrvOrdType fsSrvOrdTypeRow,
                                   PXResultset<FSAppointmentDet> bqlResultSet_FSAppointmentDet_PostAR_AP,
                                   PXResultset<FSAppointmentDet> bqlResultSet_FSAppointmentDet_PostSO,
                                   PXResultset<FSAppointmentDet> bqlResultSet_FSAppointmentInventoryItem_PostAR_AP,
                                   PXResultset<FSAppointmentDet> bqlResultSet_FSAppointmentInventoryItem_PostSO)
            {
                this.FSAppointmentRow = fsAppointmentRow;
                this.FSServiceOrderRow = fsServiceOrderRow;
                this.FSSrvOrdTypeRow = fsSrvOrdTypeRow;
                this.FSAppointmentDetToPostInAR_AP = bqlResultSet_FSAppointmentDet_PostAR_AP;
                this.FSAppointmentDetToPostInSO = bqlResultSet_FSAppointmentDet_PostSO;
                this.FSAppointmentInventoryItemToPostIn_AR_AP = bqlResultSet_FSAppointmentInventoryItem_PostAR_AP;
                this.FSAppointmentInventoryItemToPostInSO = bqlResultSet_FSAppointmentInventoryItem_PostSO;
            }
        }

        public class ServiceOrderInfo
        {
            public ServiceOrderToPost FSServiceOrderRow;
            public FSSrvOrdType FSSrvOrdTypeRow;

            public PXResultset<FSSODet> FSSODetToPostInAR_AP;
            public PXResultset<FSSODet> FSSODetToPostInSO;

            public ServiceOrderInfo()
            {
            }

            public ServiceOrderInfo(ServiceOrderToPost fsServiceOrderRow,
                                    FSSrvOrdType fsSrvOrdTypeRow,
                                    PXResultset<FSSODet> bqlResultSet_FSSODet_PostAR_AP,
                                    PXResultset<FSSODet> bqlResultSet_FSSODet_PostSO)
            {
                this.FSServiceOrderRow = fsServiceOrderRow;
                this.FSSrvOrdTypeRow = fsSrvOrdTypeRow;
                this.FSSODetToPostInAR_AP = bqlResultSet_FSSODet_PostAR_AP;
                this.FSSODetToPostInSO = bqlResultSet_FSSODet_PostSO;
            }
        }

        public class PostingLine
        {
            public string TableSource;
            public string LineType;
            public int? LineID;
            public int? BranchID;
            public int? InventoryID;
            public string UOM;
            public int? AcctID;
            public int? SubID;
            public int? SiteID;
            public int? LocationID;
            public int? SubItemID;
            public string TranDesc;
            public int? ProjectID;
            public int? ProjectTaskID;
            public decimal? CuryUnitPrice;
            public decimal? Qty;
            public decimal? CuryTranAmt;
            public int? PostID;
            public bool? IsBillable;
        }

        public class GroupTransition
        {
            public int Index;
            public AppointmentToPost FSAppointmentRow;
            public ServiceOrderToPost FSServiceOrderRow;

            public GroupTransition(int index, AppointmentToPost fSAppointmentRow)
            {
                this.Index              = index;
                this.FSAppointmentRow   = fSAppointmentRow;
            }

            public GroupTransition(int index, ServiceOrderToPost fsServiceOrderRow)
            {
                this.Index = index;
                this.FSServiceOrderRow = fsServiceOrderRow;
            }
        }

        public class InvoiceItem
        {
            public AppointmentToPost FSAppointmentRow;
            public ServiceOrderToPost FSServiceOrderRow;
            public FSSrvOrdType FSSrvOrdTypeRow;

            public List<PostingLine> FSPostingLineDetailsToPostInAR_AP;
            public List<PostingLine> FSPostingLineDetailsToPostInSO;

            public int Index;
            public int? CustomerID;
            public int? SOID;
            public int? AppointmentID;
            public bool? PostToAPNegBalances;
            public int? CustomerLocationID;
            public string CustPORefNbr;
            public string CustWorkOrderRefNbr;

            private int? GetAcct(PXGraph graph, 
                                 IFSSODetBase fsSODetBase,
                                 FSAppointmentDet fsAppointmentInventoryItem, 
                                 FSServiceOrder fsServiceOrderRow, 
                                 FSSrvOrdType fsSrvOrdTypeRow)
            {
                int? acctID = null;

                if (fsSODetBase != null)
                {
                    if (fsSODetBase.AcctID != null)
                    {
                        acctID = fsSODetBase.AcctID;
                    }
                    else
                    {
                        acctID = ServiceOrderCore.Get_TranAcctID_DefaultValue(graph,
                                                                              fsSrvOrdTypeRow.SalesAcctSource,
                                                                              fsSODetBase.InventoryID,
                                                                              fsSODetBase.SiteID,
                                                                              fsServiceOrderRow);
                    }
                }
                else if (fsAppointmentInventoryItem != null)
                {
                    acctID = ServiceOrderCore.Get_TranAcctID_DefaultValue(graph,
                                                                          fsSrvOrdTypeRow.SalesAcctSource,
                                                                          fsAppointmentInventoryItem.InventoryID,
                                                                          fsAppointmentInventoryItem.SiteID,
                                                                          fsServiceOrderRow);
                }

                return acctID;
            }

            /// <summary>
            /// Adds the line <c>fsAppointmentDetRow</c> or <c>fsAppointmentInventoryItem</c> to the List <c>fsAppointmentDetailsToPostInAR_AP</c> or <c>fsAppointmentDetailsToPostInSO</c> depending on <c>addToArApList</c> and <c>addToSoList</c> flags.
            /// </summary>
            private void AddAppointmentLineToList(PXGraph graph, FSAppointmentDet fsAppointmentDetRow, FSAppointmentDet fsAppointmentInventoryItem, AppointmentInfo appointmentInfo, bool addToArApList, bool addToSoList)
            {
                PostingLine appointmentLineRow = new PostingLine();

                if (fsAppointmentDetRow != null)
                {
                    int? itemID = fsAppointmentDetRow.InventoryID;

                    appointmentLineRow.TableSource      = ID.TablePostSource.FSAPPOINTMENT_DET;
                    appointmentLineRow.LineType         = fsAppointmentDetRow.LineType;
                    appointmentLineRow.LineID           = fsAppointmentDetRow.AppDetID;
                    appointmentLineRow.BranchID         = appointmentInfo.FSServiceOrderRow.BranchID;
                    appointmentLineRow.InventoryID      = itemID;
                    appointmentLineRow.UOM              = fsAppointmentDetRow.UOM;

                    appointmentLineRow.AcctID           = this.GetAcct(graph, fsAppointmentDetRow, null, appointmentInfo.FSServiceOrderRow, appointmentInfo.FSSrvOrdTypeRow);
                    appointmentLineRow.SubID            = fsAppointmentDetRow.SubID;

                    appointmentLineRow.SiteID           = fsAppointmentDetRow.SiteID;
                    appointmentLineRow.LocationID       = fsAppointmentDetRow.SiteLocationID;
                    appointmentLineRow.SubItemID        = fsAppointmentDetRow.SubItemID;
                    appointmentLineRow.TranDesc         = fsAppointmentDetRow.TranDesc;
                    appointmentLineRow.ProjectID        = fsAppointmentDetRow.ProjectID;
                    appointmentLineRow.ProjectTaskID    = fsAppointmentDetRow.ProjectTaskID;
                    appointmentLineRow.CuryUnitPrice    = fsAppointmentDetRow.CuryUnitPrice;
                    appointmentLineRow.Qty              = fsAppointmentDetRow.Qty;
                    appointmentLineRow.CuryTranAmt          = fsAppointmentDetRow.TranAmt;
                    appointmentLineRow.PostID           = fsAppointmentDetRow.PostID;
                    appointmentLineRow.IsBillable       = fsAppointmentDetRow.IsBillable;
                }
                else if (fsAppointmentInventoryItem != null)
                {
                    appointmentLineRow.TableSource      = ID.TablePostSource.FSAPPOINTMENT_DET;
                    appointmentLineRow.LineType         = fsAppointmentInventoryItem.LineType;
                    appointmentLineRow.LineID           = fsAppointmentInventoryItem.AppDetID;
                    appointmentLineRow.BranchID         = appointmentInfo.FSServiceOrderRow.BranchID;
                    appointmentLineRow.InventoryID      = fsAppointmentInventoryItem.InventoryID;
                    appointmentLineRow.UOM              = fsAppointmentInventoryItem.UOM;

                    appointmentLineRow.AcctID           = this.GetAcct(graph, null, fsAppointmentInventoryItem, appointmentInfo.FSServiceOrderRow, appointmentInfo.FSSrvOrdTypeRow);
                    appointmentLineRow.SubID            = null;

                    appointmentLineRow.SiteID           = fsAppointmentInventoryItem.SiteID;
                    appointmentLineRow.SubItemID        = fsAppointmentInventoryItem.SubItemID;
                    appointmentLineRow.TranDesc         = fsAppointmentInventoryItem.TranDesc;
                    appointmentLineRow.ProjectID        = fsAppointmentInventoryItem.ProjectID;
                    appointmentLineRow.ProjectTaskID    = fsAppointmentInventoryItem.ProjectTaskID;
                    appointmentLineRow.CuryUnitPrice    = fsAppointmentInventoryItem.CuryUnitPrice;
                    appointmentLineRow.Qty              = fsAppointmentInventoryItem.Qty;
                    appointmentLineRow.CuryTranAmt      = fsAppointmentInventoryItem.TranAmt;
                    appointmentLineRow.PostID           = fsAppointmentInventoryItem.PostID;
                    appointmentLineRow.IsBillable       = true;
                }

                if (addToArApList)
                {
                    this.FSPostingLineDetailsToPostInAR_AP.Add(appointmentLineRow);
                }
                else if (addToSoList)
                {
                    this.FSPostingLineDetailsToPostInSO.Add(appointmentLineRow);
                }
            }

            /// <summary>
            /// Adds the line <c>fsSODetRow</c> to the List <c>FSPostingLineDetailsToPostInAR_AP</c> or <c>FSPostingLineDetailsToPostInSO</c> depending on <c>addToArApList</c> and <c>addToSoList</c> flags.
            /// </summary>
            private void AddSOLineToList(PXGraph graph, FSSODet fsSODetRow, ServiceOrderInfo serviceOrderInfo, bool addToArApList, bool addToSoList)
            {
                PostingLine soLineRow = new PostingLine();

                if (fsSODetRow != null)
                {
                    int? itemID = fsSODetRow.InventoryID;

                    soLineRow.TableSource      = ID.TablePostSource.FSSO_DET;
                    soLineRow.LineType         = fsSODetRow.LineType;
                    soLineRow.LineID           = fsSODetRow.SODetID;
                    soLineRow.BranchID         = serviceOrderInfo.FSServiceOrderRow.BranchID;
                    soLineRow.InventoryID      = itemID;
                    soLineRow.UOM              = fsSODetRow.UOM;

                    soLineRow.AcctID           = this.GetAcct(graph, fsSODetRow, null, serviceOrderInfo.FSServiceOrderRow, serviceOrderInfo.FSSrvOrdTypeRow);
                    soLineRow.SubID            = fsSODetRow.SubID;

                    soLineRow.SiteID           = fsSODetRow.SiteID;
                    soLineRow.LocationID       = fsSODetRow.SiteLocationID;
                    soLineRow.SubItemID        = fsSODetRow.SubItemID;
                    soLineRow.TranDesc         = fsSODetRow.TranDesc;
                    soLineRow.ProjectID        = fsSODetRow.ProjectID;
                    soLineRow.ProjectTaskID    = fsSODetRow.ProjectTaskID;
                    soLineRow.CuryUnitPrice    = fsSODetRow.CuryUnitPrice;
                    soLineRow.Qty              = fsSODetRow.EstimatedQty;
                    soLineRow.CuryTranAmt      = fsSODetRow.CuryEstimatedTranAmt;
                    soLineRow.PostID           = fsSODetRow.PostID;
                    soLineRow.IsBillable       = fsSODetRow.IsBillable;
                }

                if (addToArApList)
                {
                    this.FSPostingLineDetailsToPostInAR_AP.Add(soLineRow);
                }
                else if (addToSoList)
                {
                    this.FSPostingLineDetailsToPostInSO.Add(soLineRow);
                }
            }

            public InvoiceItem(PXGraph graph, AppointmentInfo appointmentInfo, int index)
            {
                this.FSAppointmentRow = appointmentInfo.FSAppointmentRow;
                this.FSServiceOrderRow = appointmentInfo.FSServiceOrderRow;
                this.FSSrvOrdTypeRow = appointmentInfo.FSSrvOrdTypeRow;
                this.Index = index;
                this.CustomerID = appointmentInfo.FSServiceOrderRow.BillCustomerID;
                this.SOID = appointmentInfo.FSServiceOrderRow.SOID;
                this.AppointmentID = appointmentInfo.FSAppointmentRow.AppointmentID;
                this.CustomerLocationID = appointmentInfo.FSServiceOrderRow.BillLocationID;
                this.CustPORefNbr = appointmentInfo.FSServiceOrderRow.CustPORefNbr;
                this.CustWorkOrderRefNbr = appointmentInfo.FSServiceOrderRow.CustWorkOrderRefNbr;
                this.PostToAPNegBalances = appointmentInfo.FSSrvOrdTypeRow.PostNegBalanceToAP;

                this.FSPostingLineDetailsToPostInAR_AP = new List<PostingLine>();
                this.FSPostingLineDetailsToPostInSO = new List<PostingLine>();

                foreach (FSAppointmentDet fsAppointmentDetRow in appointmentInfo.FSAppointmentDetToPostInAR_AP)
                {
                    this.AddAppointmentLineToList(graph, fsAppointmentDetRow, null, appointmentInfo, true, false);
                }

                foreach (FSAppointmentDet fsAppointmentDetRow in appointmentInfo.FSAppointmentDetToPostInSO)
                {
                    this.AddAppointmentLineToList(graph, fsAppointmentDetRow, null, appointmentInfo, false, true);
                }

                foreach (FSAppointmentDet fsAppointmentInventoryItemRow in appointmentInfo.FSAppointmentInventoryItemToPostIn_AR_AP)
                {
                    this.AddAppointmentLineToList(graph, null, fsAppointmentInventoryItemRow, appointmentInfo, true, false);
                }

                foreach (FSAppointmentDet fsAppointmentInventoryItemRow in appointmentInfo.FSAppointmentInventoryItemToPostInSO)
                {
                    this.AddAppointmentLineToList(graph, null, fsAppointmentInventoryItemRow, appointmentInfo, false, true);
                }
            }

            public InvoiceItem(PXGraph graph, ServiceOrderInfo serviceOrderInfo, int index)
            {
                this.FSServiceOrderRow = serviceOrderInfo.FSServiceOrderRow;
                this.FSSrvOrdTypeRow = serviceOrderInfo.FSSrvOrdTypeRow;
                this.Index = index;
                this.CustomerID = serviceOrderInfo.FSServiceOrderRow.BillCustomerID;
                this.SOID = serviceOrderInfo.FSServiceOrderRow.SOID;
                this.CustomerLocationID = serviceOrderInfo.FSServiceOrderRow.BillLocationID;
                this.CustPORefNbr = serviceOrderInfo.FSServiceOrderRow.CustPORefNbr;
                this.CustWorkOrderRefNbr = serviceOrderInfo.FSServiceOrderRow.CustWorkOrderRefNbr;
                this.PostToAPNegBalances = serviceOrderInfo.FSSrvOrdTypeRow.PostNegBalanceToAP;

                this.FSPostingLineDetailsToPostInAR_AP = new List<PostingLine>();
                this.FSPostingLineDetailsToPostInSO = new List<PostingLine>();

                foreach (FSSODet fsSODetRow in serviceOrderInfo.FSSODetToPostInAR_AP)
                {
                    this.AddSOLineToList(graph, fsSODetRow, serviceOrderInfo, true, false);
                }

                foreach (FSSODet fsSODetRow in serviceOrderInfo.FSSODetToPostInSO)
                {
                    this.AddSOLineToList(graph, fsSODetRow, serviceOrderInfo, false, true);
                }
            }
        }

        #endregion

        #region PostingIventoryClasses

        public class AppointmentInventoryItemInfo
        {
            public FSAppointment FSAppointmentRow;
            public FSServiceOrder FSServiceOrderRow;
            public FSSrvOrdType FSSrvOrdTypeRow;
            public FSAppointmentDet FSAppointmentDet;
            public FSAppointmentDet FSAppointmentInventoryItem;
            public int Index;
            public string ServiceType;
            public int? AppointmentID;

            public AppointmentInventoryItemInfo()
            { 
            }

            public AppointmentInventoryItemInfo(FSAppointment fsAppointmentRow,
                                                FSServiceOrder fsServiceOrderRow,
                                                FSSrvOrdType fsSrvOrdTypeRow,
                                                FSAppointmentDet fsAppointmentDet,
                                                FSAppointmentDet fsAppointmentInventoryItemRow,
                                                int index)
            {
                this.FSAppointmentRow           = fsAppointmentRow;
                this.FSServiceOrderRow          = fsServiceOrderRow;
                this.FSSrvOrdTypeRow            = fsSrvOrdTypeRow;
                this.FSAppointmentDet           = fsAppointmentDet;
                this.FSAppointmentInventoryItem = fsAppointmentInventoryItemRow;
                this.AppointmentID              = fsAppointmentRow.AppointmentID;
                this.ServiceType                = fsAppointmentDet.ServiceType;
                this.Index                      = index;
            }
        }

        public class AppointmentInventoryItemGroup
        {
            public int Pivot;
            public string ServiceType;
            public string BatchNbr;
            public string DocumentType;
            public string DocumentRefNbr;

            public List<AppointmentInventoryItemInfo> AppointmentInventoryItems;

            public AppointmentInventoryItemGroup(int pivot, string serviceType, List<AppointmentInventoryItemInfo> appointmentInventoryItemList)
            {
                this.BatchNbr                   = null;
                this.DocumentType               = null;
                this.DocumentRefNbr             = null;
                this.Pivot                      = pivot;
                this.ServiceType                = serviceType;
                this.AppointmentInventoryItems  = appointmentInventoryItemList;
            }
        }
        #endregion

        public class ItemList
        {
            public int? itemID;
            public List<object> list;

            public ItemList()
            {
            }

            public ItemList(int? itemID)
            {
                this.itemID = itemID;
                this.list = new List<object>();
            }
        }

        /// <summary>
        /// This class allows the use of a cero decimal in a BQL type declaration.
        /// </summary>
        public class decimal_0 : PX.Data.BQL.BqlDecimal.Constant<decimal_0>
		{
            public decimal_0()
                : base(0)
            {
            }
        }

        public class decimal_1 : PX.Data.BQL.BqlDecimal.Constant<decimal_1>
		{
            public decimal_1()
                : base(1m)
            {
            }
        }

        public class decimal_100 : PX.Data.BQL.BqlDecimal.Constant<decimal_100>
		{
            public decimal_100()
                : base(100)
            {
            }
        }

        public class decimal_60 : PX.Data.BQL.BqlDecimal.Constant<decimal_60>
		{
            public decimal_60()
                : base(60m)
            {
            }
        }

        public class int_0 : PX.Data.BQL.BqlInt.Constant<int_0>
		{
            public int_0()
                : base(0)
            {
            }
        }

        public class int_1 : PX.Data.BQL.BqlInt.Constant<int_1>
        {
            public int_1()
                : base(1)
            {
            }
        }

        public class SOARLineEquipmentComponent
        {
            public string equipmentAction;
            public int? componentID;
            public int? currentLineRef;
            public string sourceLineRef;
            public string sourceNewTargetEquipmentLineNbr;
            public FSxSOLine fsxSOLineRow;
            public FSxARTran fsxARTranRow;

            public SOARLineEquipmentComponent(IDocLine docLine, SOLine sOLineRow, FSxSOLine fSxSOLineRow)
            {
                this.componentID = docLine.ComponentID;
                this.currentLineRef = sOLineRow.LineNbr;
                this.equipmentAction = docLine.EquipmentAction;
                this.sourceLineRef = docLine.LineRef;
                this.sourceNewTargetEquipmentLineNbr = docLine.NewTargetEquipmentLineNbr;
                this.fsxSOLineRow = fSxSOLineRow;
                this.fsxARTranRow = null;
            }

            public SOARLineEquipmentComponent(IDocLine docLine, ARTran arTranRow, FSxARTran fsxARTranRow)
            {
                this.componentID = docLine.ComponentID;
                this.currentLineRef = arTranRow.LineNbr;
                this.equipmentAction = docLine.EquipmentAction;
                this.sourceLineRef = docLine.LineRef;
                this.sourceNewTargetEquipmentLineNbr = docLine.NewTargetEquipmentLineNbr;
                this.fsxARTranRow = fsxARTranRow;
                this.fsxSOLineRow = null;
            }

            public SOARLineEquipmentComponent(ContractInvoiceLine docLine, SOLine sOLineRow, FSxSOLine fSxSOLineRow)
            {
                this.componentID = docLine.ComponentID;
                this.currentLineRef = sOLineRow.LineNbr;
                this.equipmentAction = docLine.EquipmentAction;
                this.sourceLineRef = docLine.LineRef;
                this.sourceNewTargetEquipmentLineNbr = docLine.NewTargetEquipmentLineNbr;
                this.fsxSOLineRow = fSxSOLineRow;
                this.fsxARTranRow = null;
            }
        }

        public class SOPrepaymentBySO
        {
            public int? SOID;
            public decimal? originalAmount;
            public decimal? unpaidAmount;
            public List<FSxSOLine> fsxSOLineList;
            public List<FSxARTran> fsxARTranList;

            public SOPrepaymentBySO(SOLine soLineRow, FSxSOLine fsxSOLineRow, decimal soTaxLine)
            {
                SOID = fsxSOLineRow.SOID;
                unpaidAmount = 0;
                unpaidAmount += (soLineRow.CuryLineAmt ?? 0m) + soTaxLine;
                originalAmount = unpaidAmount;
                fsxSOLineList = new List<FSxSOLine>();
                fsxSOLineList.Add(fsxSOLineRow);
            }

            public SOPrepaymentBySO(ARTran arTranRow, FSxARTran fsxARTran)
            {
                SOID = fsxARTran.SOID;
                unpaidAmount = 0;
                unpaidAmount += arTranRow.CuryTranAmt ?? 0m;
                originalAmount = unpaidAmount;
                fsxARTranList = new List<FSxARTran>();
                fsxARTranList.Add(fsxARTran);
            }

            public PXResultset<ARPayment> GetPrepaymentBySO(PXGraph graph)
            {
                if (SOID == null)
                {
                    return null;
                }

                return PXSelectReadonly2<ARPayment,
                       InnerJoin<FSAdjust,
                       On<
                           ARPayment.docType, Equal<FSAdjust.adjgDocType>,
                           And<ARPayment.refNbr, Equal<FSAdjust.adjgRefNbr>>>,
                       InnerJoin<FSServiceOrder,
                       On<
                           FSAdjust.adjdOrderType, Equal<FSServiceOrder.srvOrdType>,
                           And<FSAdjust.adjdOrderNbr, Equal<FSServiceOrder.refNbr>>>>>,
                       Where<
                           FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                       .Select(graph, SOID);

            }

        }

        public class SOPrepaymentHelper
        {
            public List<SOPrepaymentBySO> SOPrepaymentList;

            public SOPrepaymentHelper()
            {
                SOPrepaymentList = new List<SOPrepaymentBySO>();
            }

            public void Add(SOLine soLineRow, FSxSOLine fsxSOLineRow, decimal soTaxLine)
            {
                if (fsxSOLineRow.SOID == null)
                    return;

                SOPrepaymentBySO row = SOPrepaymentList.Find(x => x.SOID.Equals(fsxSOLineRow.SOID));

                if (row != null)
                {
                    row.unpaidAmount += (soLineRow.CuryLineAmt ?? 0m) + soTaxLine;
                    row.fsxSOLineList.Add(fsxSOLineRow);
                }
                else
                {
                    SOPrepaymentBySO newRow = new SOPrepaymentBySO(soLineRow, fsxSOLineRow, soTaxLine);
                    SOPrepaymentList.Add(newRow);
                }
            }

            public void Add(ARTran arTranRow, FSxARTran fsxARTranRow)
            {
                if (fsxARTranRow.SOID == null)
                    return;

                SOPrepaymentBySO row = SOPrepaymentList.Find(x => x.SOID.Equals(fsxARTranRow.SOID));

                if (row != null)
                {
                    row.unpaidAmount += arTranRow.CuryTranAmt ?? 0m;
                    row.fsxARTranList.Add(fsxARTranRow);
                }
                else
                {
                    SOPrepaymentBySO newRow = new SOPrepaymentBySO(arTranRow, fsxARTranRow);
                    SOPrepaymentList.Add(newRow);
                }
            }
        }

        public class SubAccountIDTupla
        {
            public int? branchLocation_SubID;
            public int? branch_SubID;
            public int? inventoryItem_SubID;
            public int? customerLocation_SubID;
            public int? postingClass_SubID;
            public int? salesPerson_SubID;
            public int? srvOrdType_SubID;
            public int? warehouse_SubID;

            public SubAccountIDTupla(int? branchLocation_SubID, int? company_SubID, int? item_SubID, int? customer_SubID, int? postingClass_SubID, int? salesPerson_SubID, int? srvOrdType_SubID, int? warehouse_SubID)
            {
                this.branchLocation_SubID = branchLocation_SubID;
                this.branch_SubID = company_SubID;
                this.inventoryItem_SubID = item_SubID;
                this.customerLocation_SubID = customer_SubID;
                this.postingClass_SubID = postingClass_SubID;
                this.salesPerson_SubID = salesPerson_SubID;
                this.srvOrdType_SubID = srvOrdType_SubID;
                this.warehouse_SubID = warehouse_SubID;
            }
        }
    }

    public class DateHandler
    {
        private DateTime date;

        public DateHandler()
        {
            this.date = DateTime.Now;
        }

        public DateHandler(double date)
        {
            this.date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local).AddMilliseconds(date);
        }

        public DateHandler(DateTime date)
        {
            this.date = date;
        }

        public DateHandler(DateTime? date)
        {
            if (date.HasValue == true)
            {
                this.date = date.Value;
            }
        }

        public void SetDate(DateTime date)
        {
            this.date = date;
        }

        public DateTime GetDate()
        {
            return this.date;
        }

        public DateTime StartOfDay()
        {
            return new DateTime(this.date.Year, this.date.Month, this.date.Day, 0, 0, 0, 0);
        }

        public DateTime BeginOfNextDay()
        {
            return (new DateTime(this.date.Year, this.date.Month, this.date.Day, 0, 0, 0, 0)).AddDays(1);
        }

        public DateTime EndOfDay()
        {
            return new DateTime(this.date.Year, this.date.Month, this.date.Day, 23, 59, 59);
        }

        public DateTime SetHours(DateTime? date)
        {
            return new DateTime(this.date.Year, this.date.Month, this.date.Day, date.Value.Hour, date.Value.Minute, date.Value.Second, date.Value.Millisecond);
        }

        public string GetDay()
        {
            return this.date.ToString("ddd");
        }

        public bool IsSameDate(DateTime date)
        {
            if (this.date.Day == date.Day && this.date.Month == date.Month && this.date.Year == date.Year)
                return true;

            return false;
        }
    }
}
