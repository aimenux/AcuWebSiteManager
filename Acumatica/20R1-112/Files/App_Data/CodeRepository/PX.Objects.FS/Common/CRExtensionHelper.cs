using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    public static class CRExtensionHelper
    {
        public static void LaunchEmployeeBoard(PXGraph graph, int? sOID)
        {
            if (sOID == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = GetServiceOrder(graph, sOID);

            if (fsServiceOrderRow != null)
            {
                ServiceOrderEntry graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();

                graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
                                .Search<FSServiceOrder.refNbr>(fsServiceOrderRow.RefNbr, fsServiceOrderRow.SrvOrdType);

                graphServiceOrder.OpenEmployeeBoard();
            }
        }

        public static void LaunchServiceOrderScreen(PXGraph graph, int? sOID)
        {
            if (sOID == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = GetServiceOrder(graph, sOID);

            if (fsServiceOrderRow != null)
            {
                ServiceOrderEntry graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();

                graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
                                .Search<FSServiceOrder.refNbr>(fsServiceOrderRow.RefNbr, fsServiceOrderRow.SrvOrdType);

                throw new PXRedirectRequiredException(graphServiceOrder, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow};
            }
        }

        public static FSServiceOrder GetServiceOrder(PXGraph graph, int? sOID)
        {
            return PXSelect<FSServiceOrder,
                   Where<
                       FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                   .Select(graph, sOID);
        }

        public static void UpdateServiceOrderHeader(ServiceOrderEntry graphServiceOrderEntry,
                                                    PXCache cache,
                                                    CRCase crCaseRow,
                                                    FSCreateServiceOrderOnCaseFilter fsCreateServiceOrderOnCaseFilterRow,
                                                    FSServiceOrder fsServiceOrderRow,
                                                    bool updatingExistingSO)
        {
            if (fsServiceOrderRow.Status == ID.Status_ServiceOrder.CLOSED)
            {
                return;
            }

            bool somethingChanged = false;

            FSSrvOrdType fsSrvOrdTypeRow = GetServiceOrderType(graphServiceOrderEntry, fsServiceOrderRow.SrvOrdType);

            if (fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT)
            {
                if (fsServiceOrderRow.CustomerID != crCaseRow.CustomerID)
                {
                    graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.customerID>(fsServiceOrderRow, crCaseRow.CustomerID);
                    somethingChanged = true;
                }

                if (fsServiceOrderRow.LocationID != crCaseRow.LocationID)
                {
                    graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.locationID>(fsServiceOrderRow, crCaseRow.LocationID);
                    somethingChanged = true;
                }
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
            {
                Customer customer = (Customer)PXSelect<Customer,
                                                Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
                                            .Select(graphServiceOrderEntry, crCaseRow.CustomerID);

                if (customer != null 
                    && fsServiceOrderRow.CuryID != customer.CuryID)
                {
                    graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.curyID>(fsServiceOrderRow, customer.CuryID);
                    somethingChanged = true;
                }
            }

            if (fsServiceOrderRow.BranchLocationID != fsCreateServiceOrderOnCaseFilterRow.BranchLocationID)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.branchLocationID>(fsServiceOrderRow, fsCreateServiceOrderOnCaseFilterRow.BranchLocationID);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.ContactID != crCaseRow.ContactID)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.contactID>(fsServiceOrderRow, crCaseRow.ContactID);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.DocDesc != crCaseRow.Subject)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.docDesc>(fsServiceOrderRow, crCaseRow.Subject);
                somethingChanged = true;
            }

            if (crCaseRow.OwnerID != null)
            {
                if (crCaseRow.OwnerID != (Guid?)cache.GetValueOriginal<CROpportunity.ownerID>(crCaseRow))
                {
                    int? salesPersonID = GetSalesPersonID(graphServiceOrderEntry, crCaseRow.OwnerID);

                    if (salesPersonID != null)
                    {
                        graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.salesPersonID>(fsServiceOrderRow, salesPersonID);
                        somethingChanged = true;
                    }
                }
            }

            if (crCaseRow.CreatedDateTime.HasValue 
                    && fsServiceOrderRow.OrderDate != crCaseRow.CreatedDateTime.Value.Date)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.orderDate>(fsServiceOrderRow, crCaseRow.CreatedDateTime.Value.Date);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.SLAETA != crCaseRow.SLAETA)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.sLAETA>(fsServiceOrderRow, crCaseRow.SLAETA);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.AssignedEmpID != fsCreateServiceOrderOnCaseFilterRow.AssignedEmpID)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.assignedEmpID>(fsServiceOrderRow, fsCreateServiceOrderOnCaseFilterRow.AssignedEmpID);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.ProblemID != fsCreateServiceOrderOnCaseFilterRow.ProblemID)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.problemID>(fsServiceOrderRow, fsCreateServiceOrderOnCaseFilterRow.ProblemID);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.LongDescr != crCaseRow.Description)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.longDescr>(fsServiceOrderRow, crCaseRow.Description);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.Priority != crCaseRow.Priority)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.priority>(fsServiceOrderRow, crCaseRow.Priority);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.Severity != crCaseRow.Severity)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.severity>(fsServiceOrderRow, crCaseRow.Severity);
                somethingChanged = true;
            }

            if (somethingChanged && updatingExistingSO)
            {
                graphServiceOrderEntry.ServiceOrderRecords.Update(fsServiceOrderRow);
            }
        }

        public static void UpdateServiceOrderHeader(ServiceOrderEntry graphServiceOrderEntry,
                                                    PXCache cache,
                                                    CROpportunity crOpportunityRow,
                                                    FSCreateServiceOrderFilter fsCreateServiceOrderOnOpportunityFilterRow,
                                                    FSServiceOrder fsServiceOrderRow,
                                                    CRContact crContactRow,
                                                    CRAddress crAddressRow,
                                                    bool updatingExistingSO)
        {
            bool somethingChanged = false;

            FSSrvOrdType fsSrvOrdTypeRow = GetServiceOrderType(graphServiceOrderEntry, fsServiceOrderRow.SrvOrdType);

            if (fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT)
            {
                if (fsServiceOrderRow.CustomerID != crOpportunityRow.BAccountID)
                {
                    graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.customerID>(fsServiceOrderRow, crOpportunityRow.BAccountID);
                    somethingChanged = true;
                }

                if (fsServiceOrderRow.LocationID != crOpportunityRow.LocationID)
                {
                    graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.locationID>(fsServiceOrderRow, crOpportunityRow.LocationID);
                    somethingChanged = true;
                }
            }

            if (fsServiceOrderRow.CuryID != crOpportunityRow.CuryID)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.curyID>(fsServiceOrderRow, crOpportunityRow.CuryID);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.BranchID != crOpportunityRow.BranchID)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.branchID>(fsServiceOrderRow, crOpportunityRow.BranchID);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.BranchLocationID != fsCreateServiceOrderOnOpportunityFilterRow.BranchLocationID)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.branchLocationID>(fsServiceOrderRow, fsCreateServiceOrderOnOpportunityFilterRow.BranchLocationID);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.ContactID != crOpportunityRow.ContactID)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.contactID>(fsServiceOrderRow, crOpportunityRow.ContactID);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.DocDesc != crOpportunityRow.Subject)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.docDesc>(fsServiceOrderRow, crOpportunityRow.Subject);
                somethingChanged = true;
            }

            if (fsServiceOrderRow.ProjectID != crOpportunityRow.ProjectID)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.projectID>(fsServiceOrderRow, crOpportunityRow.ProjectID);
                somethingChanged = true;
            }

            if (crOpportunityRow.OwnerID != null)
            {
                if (crOpportunityRow.OwnerID != (Guid?)cache.GetValueOriginal<CROpportunity.ownerID>(crOpportunityRow))
                {
                    int? salesPersonID = GetSalesPersonID(graphServiceOrderEntry, crOpportunityRow.OwnerID);

                    if (salesPersonID != null)
                    {
                        graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.salesPersonID>(fsServiceOrderRow, salesPersonID);
                        somethingChanged = true;
                    }
                }
            }

            if (fsServiceOrderRow.OrderDate != crOpportunityRow.CloseDate)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.orderDate>(fsServiceOrderRow, crOpportunityRow.CloseDate);
                somethingChanged = true;
            }

            FSAddress fsAddressRow = graphServiceOrderEntry.ServiceOrder_Address.Select();
            FSContact fsContactRow = graphServiceOrderEntry.ServiceOrder_Contact.Select();

            ApplyChangesfromContactInfo(graphServiceOrderEntry, crContactRow, fsContactRow, ref somethingChanged);
            ApplyChangesfromAddressInfo(graphServiceOrderEntry, crAddressRow, fsAddressRow, ref somethingChanged);

            if (fsServiceOrderRow.TaxZoneID != crOpportunityRow.TaxZoneID)
            {
                graphServiceOrderEntry.ServiceOrderRecords.SetValueExt<FSServiceOrder.taxZoneID>(fsServiceOrderRow, crOpportunityRow.TaxZoneID);
                somethingChanged = true;
            }

            if (somethingChanged && updatingExistingSO)
            {
                graphServiceOrderEntry.ServiceOrderRecords.Update(fsServiceOrderRow);
            }
        }

        public static FSSrvOrdType GetServiceOrderType(PXGraph graph, string srvOrdType)
        {
            if (string.IsNullOrEmpty(srvOrdType))
            {
                return null;
            }

            return PXSelect<FSSrvOrdType,
                   Where<
                       FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>>>
                   .Select(graph, srvOrdType);
        }

        private static void ApplyChangesfromAddressInfo(ServiceOrderEntry graphServiceOrderEntry, CRAddress crAddressRow, FSAddress fsAddressRow, ref bool somethingChanged)
        {
            if (crAddressRow == null)
            {
                return;
            }

            if (fsAddressRow.AddressLine1 != crAddressRow.AddressLine1)
            {
                graphServiceOrderEntry.ServiceOrder_Address.SetValueExt<FSAddress.addressLine1>(fsAddressRow, crAddressRow.AddressLine1);
                somethingChanged = true;
            }

            if (fsAddressRow.AddressLine2 != crAddressRow.AddressLine2)
            {
                graphServiceOrderEntry.ServiceOrder_Address.SetValueExt<FSAddress.addressLine2>(fsAddressRow, crAddressRow.AddressLine2);
                somethingChanged = true;
            }

            if (fsAddressRow.AddressLine3 != crAddressRow.AddressLine3)
            {
                graphServiceOrderEntry.ServiceOrder_Address.SetValueExt<FSAddress.addressLine3>(fsAddressRow, crAddressRow.AddressLine3);
                somethingChanged = true;
            }

            if (fsAddressRow.City != crAddressRow.City)
            {
                graphServiceOrderEntry.ServiceOrder_Address.SetValueExt<FSAddress.city>(fsAddressRow, crAddressRow.City);
                somethingChanged = true;
            }

            if (fsAddressRow.CountryID != crAddressRow.CountryID)
            {
                graphServiceOrderEntry.ServiceOrder_Address.SetValueExt<FSAddress.countryID>(fsAddressRow, crAddressRow.CountryID);
                somethingChanged = true;
            }

            if (fsAddressRow.State != crAddressRow.State)
            {
                graphServiceOrderEntry.ServiceOrder_Address.SetValueExt<FSAddress.state>(fsAddressRow, crAddressRow.State);
                somethingChanged = true;
            }

            if (fsAddressRow.PostalCode != crAddressRow.PostalCode)
            {
                graphServiceOrderEntry.ServiceOrder_Address.SetValueExt<FSAddress.postalCode>(fsAddressRow, crAddressRow.PostalCode);
                somethingChanged = true;
            }
        }

        private static void ApplyChangesfromContactInfo(ServiceOrderEntry graphServiceOrderEntry, CRContact crContactRow, FSContact fsContactRow, ref bool somethingChanged)
        {
            if (crContactRow == null)
            {
                return;
            }

            if (fsContactRow.Title != crContactRow.Title)
            {
                graphServiceOrderEntry.ServiceOrder_Contact.SetValueExt<FSContact.title>(fsContactRow, crContactRow.Title);
                somethingChanged = true;
            }

            if (fsContactRow.Attention != crContactRow.Attention)
            {
                graphServiceOrderEntry.ServiceOrder_Contact.SetValueExt<FSContact.attention>(fsContactRow, crContactRow.Attention);
                somethingChanged = true;
            }

            if (fsContactRow.Email != crContactRow.Email)
            {
                graphServiceOrderEntry.ServiceOrder_Contact.SetValueExt<FSContact.email>(fsContactRow, crContactRow.Email);
                somethingChanged = true;
            }

            if (fsContactRow.Phone1 != crContactRow.Phone1)
            {
                graphServiceOrderEntry.ServiceOrder_Contact.SetValueExt<FSContact.phone1>(fsContactRow, crContactRow.Phone1);
                somethingChanged = true;
            }

            if (fsContactRow.Phone2 != crContactRow.Phone2)
            {
                graphServiceOrderEntry.ServiceOrder_Contact.SetValueExt<FSContact.phone2>(fsContactRow, crContactRow.Phone2);
                somethingChanged = true;
            }

            if (fsContactRow.Phone3 != crContactRow.Phone3)
            {
                graphServiceOrderEntry.ServiceOrder_Contact.SetValueExt<FSContact.phone3>(fsContactRow, crContactRow.Phone3);
                somethingChanged = true;
            }

            if (fsContactRow.Fax != crContactRow.Fax)
            {
                graphServiceOrderEntry.ServiceOrder_Contact.SetValueExt<FSContact.fax>(fsContactRow, crContactRow.Fax);
                somethingChanged = true;
            }
        }

        private static int? GetSalesPersonID(PXGraph graph, Guid? userID)
        {
            EPEmployee epeEmployeeRow = PXSelect<EPEmployee,
                                        Where<
                                            EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>
                                        .Select(graph, userID);

            if (epeEmployeeRow != null)
            {
                return epeEmployeeRow.SalesPersonID;
            }

            return null;
        }

        public static FSServiceOrder InitNewServiceOrder(string srvOrdType, string sourceType)
        {
            FSServiceOrder fsServiceOrderRow = new FSServiceOrder();
            fsServiceOrderRow.SrvOrdType = srvOrdType;
            fsServiceOrderRow.SourceType = sourceType;

            return fsServiceOrderRow;
        }

        public static FSServiceOrder GetRelatedServiceOrder(PXGraph graph, PXCache chache, IBqlTable crTable, int? sOID)
        {
            FSServiceOrder fsServiceOrderRow = null;

            if (sOID != null && chache.GetStatus(crTable) != PXEntryStatus.Inserted)
            {
                fsServiceOrderRow = PXSelect<FSServiceOrder,
                                    Where<
                                        FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                                    .Select(graph, sOID);
            }

            return fsServiceOrderRow;
        }

        #region Opportunity Methods
        public static FSServiceOrder GetServiceOrderLinkedToOpportunity(PXGraph graph, CROpportunity crOpportunityRow)
        {
            if (graph == null || crOpportunityRow == null)
            {
                return null;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)PXSelectJoin<FSServiceOrder,
                                               InnerJoin<CROpportunity,
                                               On<
                                                   CROpportunity.opportunityID, Equal<FSServiceOrder.sourceRefNbr>,
                                                   And<FSServiceOrder.sourceType, Equal<FSServiceOrder.sourceType.Opportunity>>>>,
                                               Where<
                                                   CROpportunity.noteID, Equal<Required<CROpportunity.noteID>>>>
                                               .Select(graph, crOpportunityRow.NoteID);

            return fsServiceOrderRow;
        }

        public static void InsertFSSODetFromOpportunity(ServiceOrderEntry graphServiceOrder,
                                                        PXCache cacheOpportunityProducts,
                                                        CRSetup crSetupRow,
                                                        CROpportunityProducts crOpportunityProductRow, 
                                                        FSxCROpportunityProducts fsxCROpportunityProductsRow,
                                                        InventoryItem inventoryItemRow)
        {
            if (graphServiceOrder == null
                    || crOpportunityProductRow == null
                    || fsxCROpportunityProductsRow == null
                    || inventoryItemRow == null)
            {
                return;
            }

            //Insert a new SODet line
            FSSODet fsSODetRow = new FSSODet();

            UpdateFSSODetFromOpportunity(graphServiceOrder.ServiceOrderDetails.Cache,
                                            fsSODetRow,
                                            crOpportunityProductRow,
                                            fsxCROpportunityProductsRow,
                                            SharedFunctions.GetLineTypeFromInventoryItem(inventoryItemRow));

            SharedFunctions.CopyNotesAndFiles(cacheOpportunityProducts,
                                                graphServiceOrder.ServiceOrderDetails.Cache,
                                                crOpportunityProductRow, graphServiceOrder.ServiceOrderDetails.Current,
                                                crSetupRow.CopyNotes,
                                                crSetupRow.CopyFiles);
        }

        public static void UpdateFSSODetFromOpportunity(PXCache soDetCache, 
                                                        FSSODet fsSODetRow, 
                                                        CROpportunityProducts crOpportunityProductRow, 
                                                        FSxCROpportunityProducts fsxCROpportunityProductsRow, 
                                                        string lineType)
        {
            if (crOpportunityProductRow == null || fsxCROpportunityProductsRow == null)
            {
                return;
            }

            fsSODetRow.SourceNoteID = crOpportunityProductRow.NoteID;
            soDetCache.Current = fsSODetRow = (FSSODet)soDetCache.Insert(fsSODetRow);

            fsSODetRow.LineType = lineType;
            fsSODetRow.InventoryID = crOpportunityProductRow.InventoryID;
            fsSODetRow.IsBillable = crOpportunityProductRow.IsFree == false;

            soDetCache.Current = fsSODetRow = (FSSODet)soDetCache.Update(fsSODetRow);
            fsSODetRow = (FSSODet)soDetCache.CreateCopy(fsSODetRow);

            fsSODetRow.BillingRule = fsxCROpportunityProductsRow.BillingRule;
            fsSODetRow.TranDesc = crOpportunityProductRow.Descr;

            if (crOpportunityProductRow.SiteID != null)
            {
                fsSODetRow.SiteID = crOpportunityProductRow.SiteID;
            }

            fsSODetRow.EstimatedDuration = fsxCROpportunityProductsRow.EstimatedDuration;
            fsSODetRow.EstimatedQty = crOpportunityProductRow.Qty;
            soDetCache.Current = fsSODetRow = (FSSODet)soDetCache.Update(fsSODetRow);

            fsSODetRow.CuryUnitPrice = crOpportunityProductRow.CuryUnitPrice;
            fsSODetRow.ManualPrice = crOpportunityProductRow.ManualPrice;

            fsSODetRow.ProjectID = crOpportunityProductRow.ProjectID;
            fsSODetRow.ProjectTaskID = crOpportunityProductRow.TaskID;
            fsSODetRow.CostCodeID = crOpportunityProductRow.CostCodeID;

            fsSODetRow.CuryUnitCost = crOpportunityProductRow.CuryUnitCost;
            fsSODetRow.ManualCost = crOpportunityProductRow.POCreate;

            fsSODetRow.EnablePO = crOpportunityProductRow.POCreate;
            fsSODetRow.POVendorID = crOpportunityProductRow.VendorID;
            fsSODetRow.POVendorLocationID = fsxCROpportunityProductsRow.VendorLocationID;

            fsSODetRow.TaxCategoryID = crOpportunityProductRow.TaxCategoryID;

            fsSODetRow.DiscPct = crOpportunityProductRow.DiscPct;
            fsSODetRow.CuryDiscAmt = crOpportunityProductRow.CuryDiscAmt;
            fsSODetRow.CuryBillableExtPrice = crOpportunityProductRow.CuryExtPrice;

            soDetCache.Current = soDetCache.Update(fsSODetRow);
        }
        #endregion
    }
}