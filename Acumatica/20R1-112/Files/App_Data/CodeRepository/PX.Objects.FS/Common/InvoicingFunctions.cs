using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static PX.Objects.FS.MessageHelper;

namespace PX.Objects.FS
{
    public static class InvoicingFunctions
    {
        public class ContactAddressSource
        {
            public string BillingSource;
            public string ShippingSource;
        }

        public static IInvoiceGraph CreateInvoiceGraph(string targetScreen)
        {
            if (targetScreen == ID.Batch_PostTo.SO)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
                {
                    return PXGraph.CreateInstance<SOOrderEntry>().GetExtension<SM_SOOrderEntry>();
                }
                else
                {
                    throw new PXException(TX.Error.DISTRIBUTION_MODULE_IS_DISABLED);
                }
            }
            else if (targetScreen == ID.Batch_PostTo.SI)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() && PXAccess.FeatureInstalled<FeaturesSet.advancedSOInvoices>())
                {
                    return PXGraph.CreateInstance<SOInvoiceEntry>().GetExtension<SM_SOInvoiceEntry>();
                }
                else
                {
                    throw new PXException(TX.Error.ADVANCED_SO_INVOICE_IS_DISABLED);
                }
            }
            else if (targetScreen == ID.Batch_PostTo.AR)
            {
                return PXGraph.CreateInstance<ARInvoiceEntry>().GetExtension<SM_ARInvoiceEntry>();
            }
            else if (targetScreen == ID.Batch_PostTo.AP)
            {
                return PXGraph.CreateInstance<APInvoiceEntry>().GetExtension<SM_APInvoiceEntry>();
            }
            else if (targetScreen == ID.Batch_PostTo.PM)
            {
                return PXGraph.CreateInstance<RegisterEntry>().GetExtension<SM_RegisterEntry>();
            }
            else if (targetScreen == ID.Batch_PostTo.IN)
            {
                return PXGraph.CreateInstance<INIssueEntry>().GetExtension<SM_INIssueEntry>();
            }
            else
            {
                throw new PXException(TX.Error.POSTING_MODULE_IS_INVALID, targetScreen);
            }
        }

        public static IInvoiceProcessGraph CreateInvoiceProcessGraph(string billingBy)
        {
            if (billingBy == ID.Billing_By.SERVICE_ORDER)
            {
                return PXGraph.CreateInstance<CreateInvoiceByServiceOrderPost>();
            }
            else if (billingBy == ID.Billing_By.APPOINTMENT)
            {
                return PXGraph.CreateInstance<CreateInvoiceByAppointmentPost>();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Sets the SubID for AR or SalesSubID for SO.
        /// </summary>
        public static void SetCombinedSubID(PXGraph graph,
                                            PXCache sender,
                                            ARTran tranARRow,
                                            APTran tranAPRow,
                                            SOLine tranSORow,
                                            FSSrvOrdType fsSrvOrdTypeRow,
                                            int? branchID,
                                            int? inventoryID,
                                            int? customerLocationID,
                                            int? branchLocationID,
                                            int? salesPersonID,
                                            bool isService)
        {
            if (string.IsNullOrEmpty(fsSrvOrdTypeRow.CombineSubFrom) == true)
            {
                throw new PXException(TX.Error.SALES_SUB_MASK_UNDEFINED_IN_SERVICE_ORDER_TYPE, fsSrvOrdTypeRow.SrvOrdType);
            }

            if (branchID == null || inventoryID == null || customerLocationID == null || branchLocationID == null)
            {
                throw new PXException(TX.Error.SOME_SUBACCOUNT_SEGMENT_SOURCE_IS_NOT_SPECIFIED);
            }

            if ((tranARRow != null && tranARRow.AccountID != null) || (tranAPRow != null && tranAPRow.AccountID != null) || (tranSORow != null && tranSORow.SalesAcctID != null))
            {
                SharedClasses.SubAccountIDTupla subAcctIDs = SharedFunctions.GetSubAccountIDs(graph, fsSrvOrdTypeRow, inventoryID, branchID, customerLocationID, branchLocationID, salesPersonID, isService);

                object value;

                try
                {
                    if (tranARRow != null)
                    {
                        value = SubAccountMaskAttribute.MakeSub<ARSetup.salesSubMask>(graph,
                                                                                      fsSrvOrdTypeRow.CombineSubFrom,
                                                                                      new object[] { subAcctIDs.branchLocation_SubID, subAcctIDs.branch_SubID, subAcctIDs.inventoryItem_SubID, subAcctIDs.customerLocation_SubID, subAcctIDs.postingClass_SubID, subAcctIDs.salesPerson_SubID, subAcctIDs.srvOrdType_SubID, subAcctIDs.warehouse_SubID },
                                                                                      new Type[] { typeof(FSBranchLocation.subID), typeof(Location.cMPSalesSubID), typeof(InventoryItem.salesSubID), typeof(Location.cSalesSubID), typeof(INPostClass.salesSubID), typeof(SalesPerson.salesSubID), typeof(FSSrvOrdType.subID), isService ? typeof(INSite.salesSubID) : typeof(InventoryItem.salesSubID) });

                        sender.RaiseFieldUpdating<ARTran.subID>(tranARRow, ref value);
                        tranARRow.SubID = (int?)value;
                    }
                    else if (tranAPRow != null)
                    {
                        value = SubAccountMaskAttribute.MakeSub<APSetup.expenseSubMask>(graph,
                                                                                        fsSrvOrdTypeRow.CombineSubFrom,
                                                                                        new object[] { subAcctIDs.branchLocation_SubID, subAcctIDs.branch_SubID, subAcctIDs.inventoryItem_SubID, subAcctIDs.customerLocation_SubID, subAcctIDs.postingClass_SubID, subAcctIDs.salesPerson_SubID, subAcctIDs.srvOrdType_SubID, subAcctIDs.warehouse_SubID },
                                                                                        new Type[] { typeof(FSBranchLocation.subID), typeof(Location.cMPSalesSubID), typeof(InventoryItem.salesSubID), typeof(Location.cSalesSubID), typeof(INPostClass.salesSubID), typeof(SalesPerson.salesSubID), typeof(FSSrvOrdType.subID), isService ? typeof(INSite.salesSubID) : typeof(InventoryItem.salesSubID) });

                        sender.RaiseFieldUpdating<APTran.subID>(tranSORow, ref value);
                        tranAPRow.SubID = (int?)value;
                    }
                    else if (tranSORow != null)
                    {
                        value = SubAccountMaskAttribute.MakeSub<SOOrderType.salesSubMask>(graph,
                                                                                          fsSrvOrdTypeRow.CombineSubFrom,
                                                                                          new object[] { subAcctIDs.branchLocation_SubID, subAcctIDs.branch_SubID, subAcctIDs.inventoryItem_SubID, subAcctIDs.customerLocation_SubID, subAcctIDs.postingClass_SubID, subAcctIDs.salesPerson_SubID, subAcctIDs.srvOrdType_SubID, subAcctIDs.warehouse_SubID },
                                                                                          new Type[] { typeof(FSBranchLocation.subID), typeof(Location.cMPSalesSubID), typeof(InventoryItem.salesSubID), typeof(Location.cSalesSubID), typeof(INPostClass.salesSubID), typeof(SalesPerson.salesSubID), typeof(FSSrvOrdType.subID), isService ? typeof(INSite.salesSubID) : typeof(InventoryItem.salesSubID) });

                        sender.RaiseFieldUpdating<SOLine.salesSubID>(tranSORow, ref value);
                        tranSORow.SalesSubID = (int?)value;
                    }
                }
                catch (PXException)
                {
                    if (tranARRow != null)
                    {
                        tranARRow.SubID = null;
                    }
                    else if (tranSORow != null)
                    {
                        tranSORow.SalesSubID = null;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the SubID for AR or SalesSubID for SO.
        /// </summary>
        public static void SetCombinedSubID(PXGraph graph,
                                            PXCache sender,
                                            ARTran tranARRow,
                                            APTran tranAPRow,
                                            SOLine tranSORow,
                                            FSSetup fsSetupRow,
                                            int? branchID,
                                            int? inventoryID,
                                            int? customerLocationID,
                                            int? branchLocationID)
        {
            if (branchID == null || inventoryID == null || customerLocationID == null || branchLocationID == null)
            {
                throw new PXException(TX.Error.SOME_SUBACCOUNT_SEGMENT_SOURCE_IS_NOT_SPECIFIED);
            }

            if ((tranARRow != null && tranARRow.AccountID != null) || (tranAPRow != null && tranAPRow.AccountID != null) || (tranSORow != null && tranSORow.SalesAcctID != null))
            {
                InventoryItem inventoryItemRow = PXSelect<InventoryItem,
                                                 Where<
                                                     InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                                                 .Select(graph, inventoryID);

                Location companyLocationRow = PXSelectJoin<Location,
                                              InnerJoin<BAccountR,
                                              On<
                                                  Location.bAccountID, Equal<BAccountR.bAccountID>,
                                                  And<Location.locationID, Equal<BAccountR.defLocationID>>>,
                                              InnerJoin<GL.Branch,
                                              On<
                                                  BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>,
                                              Where<
                                                  GL.Branch.branchID, Equal<Required<ARTran.branchID>>>>
                                              .Select(graph, branchID);

                Location customerLocationRow = PXSelect<Location,
                                               Where<
                                                   Location.locationID, Equal<Required<Location.locationID>>>>
                                               .Select(graph, customerLocationID);

                FSBranchLocation fsBranchLocationRow = PXSelect<FSBranchLocation,
                                                       Where<
                                                           FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                                       .Select(graph, branchLocationID);

                int? customer_SubID = customerLocationRow.CSalesSubID;
                int? item_SubID = inventoryItemRow.SalesSubID;
                int? company_SubID = companyLocationRow.CMPSalesSubID;
                int? branchLocation_SubID = fsBranchLocationRow.SubID;

                object value;

                try
                {
                    if (tranARRow != null)
                    {
                        value = SubAccountMaskAttribute.MakeSub<ARSetup.salesSubMask>(graph,
                                                                                      fsSetupRow.ContractCombineSubFrom,
                                                                                      new object[] { customer_SubID, item_SubID, company_SubID, branchLocation_SubID },
                                                                                      new Type[] { typeof(Location.cSalesSubID), typeof(InventoryItem.salesSubID), typeof(Location.cMPSalesSubID), typeof(FSBranchLocation.subID) },
                                                                                      true);

                        sender.RaiseFieldUpdating<ARTran.subID>(tranARRow, ref value);
                        tranARRow.SubID = (int?)value;
                    }
                    else if (tranAPRow != null)
                    {
                        value = SubAccountMaskAttribute.MakeSub<APSetup.expenseSubMask>(graph,
                                                                                        fsSetupRow.ContractCombineSubFrom,
                                                                                        new object[] { customer_SubID, item_SubID, company_SubID, branchLocation_SubID },
                                                                                        new Type[] { typeof(Location.cSalesSubID), typeof(InventoryItem.salesSubID), typeof(Location.cMPSalesSubID), typeof(FSBranchLocation.subID) },
                                                                                        true);

                        sender.RaiseFieldUpdating<APTran.subID>(tranSORow, ref value);
                        tranAPRow.SubID = (int?)value;
                    }
                    else if (tranSORow != null)
                    {
                        value = SubAccountMaskAttribute.MakeSub<SOOrderType.salesSubMask>(graph,
                                                                                          fsSetupRow.ContractCombineSubFrom,
                                                                                          new object[] { customer_SubID, item_SubID, company_SubID, branchLocation_SubID },
                                                                                          new Type[] { typeof(Location.cSalesSubID), typeof(InventoryItem.salesSubID), typeof(Location.cMPSalesSubID), typeof(FSBranchLocation.subID) },
                                                                                          true);

                        sender.RaiseFieldUpdating<SOLine.salesSubID>(tranSORow, ref value);
                        tranSORow.SalesSubID = (int?)value;
                    }
                }
                catch (PXException)
                {
                    if (tranARRow != null)
                    {
                        tranARRow.SubID = null;
                    }
                    else if (tranSORow != null)
                    {
                        tranSORow.SalesSubID = null;
                    }
                }
            }
        }

        private static PXResult<Location, Contact, Address> GetContactAndAddressFromLocation(PXGraph graph, int? locationID)
        {
            return (PXResult<Location, Contact, Address>)
                   PXSelectJoin<Location,
                   LeftJoin<Contact,
                   On<
                       Contact.contactID, Equal<Location.defContactID>>,
                   LeftJoin<Address,
                   On<
                       Address.addressID, Equal<Location.defAddressID>>>>,
                   Where<
                       Location.locationID, Equal<Required<Location.locationID>>>>
                   .Select(graph, locationID);
        }

        private static PXResult<Location, Customer, Contact, Address> GetContactAddressFromDefaultLocation(PXGraph graph, int? bAccountID)
        {
            return (PXResult<Location, Customer, Contact, Address>)
                   PXSelectJoin<Location,
                   InnerJoin<Customer,
                   On<
                       Customer.bAccountID, Equal<Location.bAccountID>,
                       And<Customer.defLocationID, Equal<Location.locationID>>>,
                   LeftJoin<Contact,
                   On<
                       Contact.contactID, Equal<Location.defContactID>>,
                   LeftJoin<Address,
                   On<
                       Address.addressID, Equal<Location.defAddressID>>>>>,
                   Where<
                       Location.bAccountID, Equal<Required<Location.bAccountID>>>>
                   .Select(graph, bAccountID);
        }

        public static void GetSrvOrdContactAddress(PXGraph graph, FSServiceOrder fsServiceOrder, out FSContact fsContact, out FSAddress fsAddress)
        {
            fsContact = PXSelect<FSContact,
                        Where<FSContact.contactID, Equal<Required<FSContact.contactID>>>>
                        .Select(graph, fsServiceOrder.ServiceOrderContactID);

            fsAddress = PXSelect<FSAddress,
                        Where<FSAddress.addressID, Equal<Required<FSAddress.addressID>>>>
                        .Select(graph, fsServiceOrder.ServiceOrderAddressID);
        }

        public static void SetContactAndAddress(PXGraph graph, FSServiceOrder fsServiceOrderRow)
        {
            int? billCustomerID = fsServiceOrderRow.BillCustomerID;
            int? billLocationID = fsServiceOrderRow.BillLocationID;
            Customer billCustomer = SharedFunctions.GetCustomerRow(graph, billCustomerID);

            ContactAddressSource contactAddressSource = GetBillingContactAddressSource(graph, fsServiceOrderRow, billCustomer);

            if (contactAddressSource == null
                || (contactAddressSource != null && string.IsNullOrEmpty(contactAddressSource.BillingSource)))
            {
                throw new PXException(TX.Error.MISSING_CUSTOMER_BILLING_ADDRESS_SOURCE);
            }

            IAddress addressRow = null;
            IContact contactRow = null;

            switch (contactAddressSource.BillingSource)
            {
                case ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO:

                    contactRow = ContactAddressHelper.GetIContact(
                        PXSelect<Contact,
                        Where<
                            Contact.contactID, Equal<Required<Contact.contactID>>>>
                        .Select(graph, billCustomer.DefBillContactID));

                    addressRow = ContactAddressHelper.GetIAddress(
                        PXSelect<Address,
                        Where<
                            Address.addressID, Equal<Required<Address.addressID>>>>
                        .Select(graph, billCustomer.DefBillAddressID));

                    break;
                case ID.Send_Invoices_To.SO_BILLING_CUSTOMER_LOCATION:

                    PXResult<Location, Contact, Address> locData = GetContactAndAddressFromLocation(graph, billLocationID);

                    if (locData != null)
                    {
                        addressRow = ContactAddressHelper.GetIAddress(locData);
                        contactRow = ContactAddressHelper.GetIContact(locData);
                    }

                    break;
                case ID.Send_Invoices_To.SERVICE_ORDER_ADDRESS:
                    GetSrvOrdContactAddress(graph, fsServiceOrderRow, out FSContact fsContact, out FSAddress fsAddress);
                    contactRow = fsContact;
                    addressRow = fsAddress;

                    break;
                default:
                    PXResult<Location, Customer, Contact, Address> defaultLocData = GetContactAddressFromDefaultLocation(graph, billCustomerID);

                    if (defaultLocData != null)
                    {
                        addressRow = ContactAddressHelper.GetIAddress(defaultLocData);
                        contactRow = ContactAddressHelper.GetIContact(defaultLocData);
                    }

                    break;
            }

            if (addressRow == null)
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.ADDRESS_CONTACT_CANNOT_BE_NULL, TX.MessageParm.ADDRESS), PXErrorLevel.Error);
            }

            if (contactRow == null)
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.ADDRESS_CONTACT_CANNOT_BE_NULL, TX.MessageParm.CONTACT), PXErrorLevel.Error);
            }

            if (graph is SOOrderEntry)
            {
                SOOrderEntry SOgraph = (SOOrderEntry)graph;

                SOBillingContact billContact = new SOBillingContact();
                SOBillingAddress billAddress = new SOBillingAddress();

                ContactAddressHelper.CopyContact(billContact, contactRow);
                billContact.CustomerID = SOgraph.customer.Current.BAccountID;
                billContact.RevisionID = 0;

                ContactAddressHelper.CopyAddress(billAddress, addressRow);
                billAddress.CustomerID = SOgraph.customer.Current.BAccountID;
                billAddress.CustomerAddressID = SOgraph.customer.Current.DefAddressID;
                billAddress.RevisionID = 0;

                billContact.IsDefaultContact = false;
                billAddress.IsDefaultAddress = false;

                SOgraph.Billing_Contact.Current = billContact = SOgraph.Billing_Contact.Insert(billContact);
                SOgraph.Billing_Address.Current = billAddress = SOgraph.Billing_Address.Insert(billAddress);

                SOgraph.Document.Current.BillAddressID = billAddress.AddressID;
                SOgraph.Document.Current.BillContactID = billContact.ContactID;

                addressRow = null;
                contactRow = null;

                GetShippingContactAddress(graph, contactAddressSource.ShippingSource, billCustomerID, fsServiceOrderRow, out contactRow, out addressRow);

                if (addressRow == null)
                {
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.ADDRESS_CONTACT_CANNOT_BE_NULL, TX.WildCards.SHIPPING_ADDRESS), PXErrorLevel.Error);
                }

                if (contactRow == null)
                {
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.ADDRESS_CONTACT_CANNOT_BE_NULL, TX.WildCards.SHIPPING_CONTACT), PXErrorLevel.Error);
                }

                SOShippingContact shipContact = new SOShippingContact();
                SOShippingAddress shipAddress = new SOShippingAddress();

                ContactAddressHelper.CopyContact(shipContact, contactRow);
                shipContact.CustomerID = SOgraph.customer.Current.BAccountID;
                shipContact.RevisionID = 0;

                ContactAddressHelper.CopyAddress(shipAddress, addressRow);
                shipAddress.CustomerID = SOgraph.customer.Current.BAccountID;
                shipAddress.CustomerAddressID = SOgraph.customer.Current.DefAddressID;
                shipAddress.RevisionID = 0;

                shipContact.IsDefaultContact = false;
                shipAddress.IsDefaultAddress = false;

                SOgraph.Shipping_Contact.Current = shipContact = SOgraph.Shipping_Contact.Insert(shipContact);
                SOgraph.Shipping_Address.Current = shipAddress = SOgraph.Shipping_Address.Insert(shipAddress);

                SOgraph.Document.Current.ShipAddressID = shipAddress.AddressID;
                SOgraph.Document.Current.ShipContactID = shipContact.ContactID;
            }
            else if (graph is ARInvoiceEntry)
            {
                ARInvoiceEntry ARgraph = (ARInvoiceEntry)graph;

                ARContact arContact = new ARContact();
                ARAddress arAddress = new ARAddress();

                ContactAddressHelper.CopyContact(arContact, contactRow);
                arContact.CustomerID = ARgraph.customer.Current.BAccountID;
                arContact.RevisionID = 0;
                arContact.IsDefaultContact = false;

                ContactAddressHelper.CopyAddress(arAddress, addressRow);
                arAddress.CustomerID = ARgraph.customer.Current.BAccountID;
                arAddress.CustomerAddressID = ARgraph.customer.Current.DefAddressID;
                arAddress.RevisionID = 0;
                arAddress.IsDefaultBillAddress = false;

                ARgraph.Billing_Contact.Current = arContact = ARgraph.Billing_Contact.Update(arContact);
                ARgraph.Billing_Address.Current = arAddress = ARgraph.Billing_Address.Update(arAddress);

                ARgraph.Document.Current.BillAddressID = arAddress.AddressID;
                ARgraph.Document.Current.BillContactID = arContact.ContactID;

                addressRow = null;
                contactRow = null;

                GetShippingContactAddress(graph, contactAddressSource.ShippingSource, billCustomerID, fsServiceOrderRow, out contactRow, out addressRow);

                if (addressRow == null)
                {
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.ADDRESS_CONTACT_CANNOT_BE_NULL, TX.WildCards.SHIPPING_ADDRESS), PXErrorLevel.Error);
                }

                if (contactRow == null)
                {
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.ADDRESS_CONTACT_CANNOT_BE_NULL, TX.WildCards.SHIPPING_CONTACT), PXErrorLevel.Error);
                }

                ARShippingContact shipContact = new ARShippingContact();
                ARShippingAddress shipAddress = new ARShippingAddress();

                ContactAddressHelper.CopyContact(shipContact, contactRow);
                shipContact.CustomerID = ARgraph.customer.Current.BAccountID;
                shipContact.RevisionID = 0;

                ContactAddressHelper.CopyAddress(shipAddress, addressRow);
                shipAddress.CustomerID = ARgraph.customer.Current.BAccountID;
                shipAddress.CustomerAddressID = ARgraph.customer.Current.DefAddressID;
                shipAddress.RevisionID = 0;

                shipContact.IsDefaultContact = false;
                shipAddress.IsDefaultAddress = false;

                ARgraph.Shipping_Contact.Current = shipContact = ARgraph.Shipping_Contact.Insert(shipContact);
                ARgraph.Shipping_Address.Current = shipAddress = ARgraph.Shipping_Address.Insert(shipAddress);

                ARgraph.Document.Current.ShipAddressID = shipAddress.AddressID;
                ARgraph.Document.Current.ShipContactID = shipContact.ContactID;
            }
        }

        /// <summary>
        /// Returns the TermID from the Vendor or Customer.
        /// </summary>
        public static string GetTermsIDFromCustomerOrVendor(PXGraph graph, int? customerID, int? vendorID)
        {
            if (customerID != null)
            {
                Customer customerRow = PXSelect<Customer,
                                       Where<
                                            Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
                                       .Select(graph, customerID);

                return customerRow?.TermsID;
            }
            else if (vendorID != null)
            {
                Vendor vendorRow = PXSelect<Vendor,
                                   Where<
                                        Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>
                                   .Select(graph, vendorID);

                return vendorRow?.TermsID;
            }

            return null;
        }

        public static ContactAddressSource GetBillingContactAddressSource(PXGraph graph, FSServiceOrder fsServiceOrderRow, Customer billCustomer)
        {
            FSSetup fSSetupRow = PXSelect<FSSetup>.Select(graph);
            ContactAddressSource contactAddressSource = null;

            if (fSSetupRow != null)
            {
                contactAddressSource = new ContactAddressSource();

                if (fSSetupRow.CustomerMultipleBillingOptions == true)
                {
                    FSCustomerBillingSetup customerBillingSetup = PXSelect<FSCustomerBillingSetup,
                                                                  Where<
                                                                      FSCustomerBillingSetup.customerID, Equal<Required<FSCustomerBillingSetup.customerID>>,
                                                                  And<
                                                                      FSCustomerBillingSetup.srvOrdType, Equal<Required<FSCustomerBillingSetup.srvOrdType>>,
                                                                  And<
                                                                      FSCustomerBillingSetup.active, Equal<True>>>>>
                                                                  .Select(graph, billCustomer.BAccountID, fsServiceOrderRow.SrvOrdType);

                    if (customerBillingSetup != null)
                    {
                        contactAddressSource.BillingSource = customerBillingSetup.SendInvoicesTo;
                        contactAddressSource.ShippingSource = customerBillingSetup.BillShipmentSource;
                    }
                }
                else if (fSSetupRow.CustomerMultipleBillingOptions == false && billCustomer != null)
                {
                    FSxCustomer fsxCustomerRow = PXCache<Customer>.GetExtension<FSxCustomer>(billCustomer);
                    contactAddressSource.BillingSource = fsxCustomerRow.SendInvoicesTo;
                    contactAddressSource.ShippingSource = fsxCustomerRow.BillShipmentSource;
                }
            }

            return contactAddressSource;
        }

        /// <summary>
        /// Cleans the posting information <c>(FSContractPostDoc, FSContractPostDet, FSContractPostBatch, FSContractPostRegister)</c> 
        /// when erasing the entire posted document (SO, AR) coming from a contract.
        /// </summary>
        public static void CleanContractPostingInfoLinkedToDoc(object postedDoc)
        {
            if (postedDoc == null)
            {
                return;
            }

            PXGraph cleanerGraph = new PXGraph();

            string createdDocType = string.Empty;
            string createdRefNbr = string.Empty;
            string postTo = string.Empty;

            if (postedDoc is SOOrder)
            {
                createdDocType = ((SOOrder)postedDoc).OrderType;
                createdRefNbr = ((SOOrder)postedDoc).RefNbr;
                postTo = ID.Contract_PostTo.SALES_ORDER_MODULE;
            }
            else if (postedDoc is ARInvoice)
            {
                createdDocType = ((ARRegister)postedDoc).DocType;
                createdRefNbr = ((ARRegister)postedDoc).RefNbr;
                postTo = ID.Contract_PostTo.ACCOUNTS_RECEIVABLE_MODULE;
            }

            PXResultset<FSContractPostDoc> fsContractPostDocRow = PXSelect<FSContractPostDoc, 
                                                                  Where<
                                                                      FSContractPostDoc.postDocType, Equal<Required<FSContractPostDoc.postDocType>>, 
                                                                  And<
                                                                      FSContractPostDoc.postRefNbr, Equal<Required<FSContractPostDoc.postRefNbr>>, 
                                                                  And<
                                                                      FSContractPostDoc.postedTO, Equal<Required<FSContractPostDoc.postedTO>>>>>>
                                                                  .Select(cleanerGraph, createdDocType, createdRefNbr, postTo);

            if (fsContractPostDocRow.Count > 0)
            {
                int? contractPostBatchID = fsContractPostDocRow.FirstOrDefault().GetItem<FSContractPostDoc>().ContractPostBatchID;
                int? contractPostDocID = fsContractPostDocRow.FirstOrDefault().GetItem<FSContractPostDoc>().ContractPostDocID;
                int? serviceContractID = fsContractPostDocRow.FirstOrDefault().GetItem<FSContractPostDoc>().ServiceContractID;
                int? contractPeriodID = fsContractPostDocRow.FirstOrDefault().GetItem<FSContractPostDoc>().ContractPeriodID;

                PXDatabase.Delete<FSContractPostRegister>(
                    new PXDataFieldRestrict<FSContractPostRegister.contractPostBatchID>(contractPostBatchID),
                    new PXDataFieldRestrict<FSContractPostRegister.postedTO>(postTo),
                    new PXDataFieldRestrict<FSContractPostRegister.postDocType>(createdDocType),
                    new PXDataFieldRestrict<FSContractPostRegister.postRefNbr>(createdRefNbr));

                PXDatabase.Delete<FSContractPostDoc>(
                    new PXDataFieldRestrict<FSContractPostDoc.contractPostDocID>(contractPostDocID),
                    new PXDataFieldRestrict<FSContractPostDoc.postedTO>(postTo),
                    new PXDataFieldRestrict<FSContractPostDoc.postDocType>(createdDocType),
                    new PXDataFieldRestrict<FSContractPostDoc.postRefNbr>(createdRefNbr));

                PXDatabase.Delete<FSContractPostDet>(
                    new PXDataFieldRestrict<FSContractPostDet.contractPostDocID>(contractPostDocID));

                PXUpdate<
                    Set<FSContractPeriod.invoiced, False, 
                    Set<FSContractPeriod.status, ListField_Status_ContractPeriod.Pending>>, 
                FSContractPeriod,
                Where<
                    FSContractPeriod.serviceContractID, Equal<Required<FSContractPeriod.serviceContractID>>,
                And<
                    FSContractPeriod.contractPeriodID, Equal<Required<FSContractPeriod.contractPeriodID>>>>>
                .Update(cleanerGraph, serviceContractID, contractPeriodID);

                ContractPostBatchMaint contractPostBatchgraph = PXGraph.CreateInstance<ContractPostBatchMaint>();
                contractPostBatchgraph.ContractBatchRecords.Current = contractPostBatchgraph.ContractBatchRecords.Search<FSContractPostDoc.contractPostBatchID>(contractPostBatchID);

                if (contractPostBatchgraph.ContractPostDocRecords.Select().Count() == 0)
                {
                    contractPostBatchgraph.ContractBatchRecords.Delete(contractPostBatchgraph.ContractBatchRecords.Current);
                }

                contractPostBatchgraph.Save.Press();
            }
        }

        /// <summary>
        /// Cleans the posting information <c>(FSCreatedDoc, FSPostDoc, FSPostInfo, FSPostDet, FSPostBatch, FSPostRegister)</c> when erasing the entire posted document (SO, SI, AR, AP).
        /// </summary>
        public static void CleanPostingInfoLinkedToDoc(object postedDoc)
        {
            if (postedDoc == null)
            {
                return;
            }

            PXGraph cleanerGraph = new PXGraph();

            string createdDocType = string.Empty;
            string createdRefNbr = string.Empty;
            string postTo = string.Empty;

            PXResultset<FSPostDet> fsPostDetRows = new PXResultset<FSPostDet>();

            if (postedDoc is SOOrder)
            {
                createdDocType = ((SOOrder)postedDoc).OrderType;
                createdRefNbr = ((SOOrder)postedDoc).RefNbr;
                postTo = ID.Batch_PostTo.SO;
                fsPostDetRows = PXSelect<FSPostDet,
                                Where<
                                    FSPostDet.sOOrderNbr, Equal<Required<FSPostDet.sOOrderNbr>>,
                                And<
                                    FSPostDet.sOOrderType, Equal<Required<FSPostDet.sOOrderType>>>>>
                                .Select(cleanerGraph, createdRefNbr, createdDocType);
            }
            else if (postedDoc is SOInvoice)
            {
                createdDocType = ((SOInvoice)postedDoc).DocType;
                createdRefNbr = ((SOInvoice)postedDoc).RefNbr;
                postTo = ID.Batch_PostTo.SI;
                fsPostDetRows = PXSelect<FSPostDet,
                                Where<
                                    FSPostDet.sOInvRefNbr, Equal<Required<FSPostDet.sOInvRefNbr>>,
                                And<
                                    FSPostDet.sOInvDocType, Equal<Required<FSPostDet.sOInvDocType>>>>>
                                .Select(cleanerGraph, createdRefNbr, createdDocType);
            }
            else if (postedDoc is ARInvoice)
            {
                createdDocType = ((ARInvoice)postedDoc).DocType;
                createdRefNbr = ((ARInvoice)postedDoc).RefNbr;
                postTo = ID.Batch_PostTo.AR;
                fsPostDetRows = PXSelect<FSPostDet,
                                Where<
                                    FSPostDet.arRefNbr, Equal<Required<FSPostDet.arRefNbr>>,
                                And<
                                    FSPostDet.arDocType, Equal<Required<FSPostDet.arDocType>>>>>
                                .Select(cleanerGraph, createdRefNbr, createdDocType);
            }
            else if (postedDoc is APInvoice)
            {
                createdDocType = ((APInvoice)postedDoc).DocType;
                createdRefNbr = ((APInvoice)postedDoc).RefNbr;
                postTo = ID.Batch_PostTo.AP;
                fsPostDetRows = PXSelect<FSPostDet,
                                Where<
                                    FSPostDet.apRefNbr, Equal<Required<FSPostDet.apRefNbr>>,
                                And<
                                    FSPostDet.apDocType, Equal<Required<FSPostDet.apDocType>>>>>
                                .Select(cleanerGraph, createdRefNbr, createdDocType);
            }

            if (fsPostDetRows.Count > 0)
            {
                int? batchID = fsPostDetRows.FirstOrDefault().GetItem<FSPostDet>().BatchID;

                PXDatabase.Delete<FSCreatedDoc>(
                    new PXDataFieldRestrict<FSCreatedDoc.batchID>(batchID),
                    new PXDataFieldRestrict<FSCreatedDoc.postTo>(postTo),
                    new PXDataFieldRestrict<FSCreatedDoc.createdDocType>(createdDocType),
                    new PXDataFieldRestrict<FSCreatedDoc.createdRefNbr>(createdRefNbr));

                PXDatabase.Delete<FSPostRegister>(
                    new PXDataFieldRestrict<FSPostRegister.batchID>(batchID),
                    new PXDataFieldRestrict<FSPostRegister.postedTO>(postTo),
                    new PXDataFieldRestrict<FSPostRegister.postDocType>(createdDocType),
                    new PXDataFieldRestrict<FSPostRegister.postRefNbr>(createdRefNbr));

                PXDatabase.Delete<FSPostDoc>(
                    new PXDataFieldRestrict<FSPostDoc.batchID>(batchID),
                    new PXDataFieldRestrict<FSPostDoc.postedTO>(postTo),
                    new PXDataFieldRestrict<FSPostDoc.postDocType>(createdDocType),
                    new PXDataFieldRestrict<FSPostDoc.postRefNbr>(createdRefNbr));

                IInvoiceGraph invoiceGraph = InvoicingFunctions.CreateInvoiceGraph(postTo);

                PostBatchMaint graphPostBatchMaint = PXGraph.CreateInstance<PostBatchMaint>();
                graphPostBatchMaint.BatchRecords.Current = graphPostBatchMaint.BatchRecords.Search<FSPostBatch.batchID>(batchID);
                bool batchLoadedSuccessfully = graphPostBatchMaint.BatchRecords.Current != null;

                foreach (FSPostDet fsPostDetRow in fsPostDetRows)
                {
                    invoiceGraph.CleanPostInfo(cleanerGraph, fsPostDetRow);

                    int postedByAPP = PXUpdateJoin<
                                        Set<FSAppointment.finPeriodID, Null,
                                        Set<FSAppointment.postingStatusAPARSO, ListField_Status_Posting.PendingToPost,
                                        Set<FSAppointment.pendingAPARSOPost, True>>>,
                                      FSAppointment,
                                      InnerJoin<FSAppointmentDet,
                                      On<
                                          FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>>>,
                                      Where<
                                          FSAppointmentDet.postID, Equal<Required<FSAppointmentDet.postID>>,
                                      And<
                                          FSAppointment.pendingAPARSOPost, Equal<False>>>>
                                      .Update(cleanerGraph, fsPostDetRow.PostID);

                    if (postedByAPP > 0)
                    {
                        int? sOID = GetServiceOrderFromAppPostID(cleanerGraph, fsPostDetRow.PostID);

                        if (AreAppointmentsPostedInSO(cleanerGraph, sOID) == false)
                        {
                            PXUpdateJoin<
                                Set<FSServiceOrder.finPeriodID, Null,
                                Set<FSServiceOrder.postedBy, Null,
                                Set<FSServiceOrder.pendingAPARSOPost, True>>>,
                            FSServiceOrder,
                            InnerJoin<FSAppointment,
                            On<
                                FSAppointment.sOID, Equal<FSServiceOrder.sOID>>,
                            InnerJoin<FSAppointmentDet,
                            On<
                                FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>>>>,
                            Where<
                                FSAppointmentDet.postID, Equal<Required<FSAppointmentDet.postID>>,
                            And<
                                FSAppointment.pendingAPARSOPost, Equal<True>>>>
                            .Update(cleanerGraph, fsPostDetRow.PostID);
                        }
                    }

                    PXUpdateJoin<
                        Set<FSServiceOrder.finPeriodID, Null,
                        Set<FSServiceOrder.postedBy, Null,
                        Set<FSServiceOrder.pendingAPARSOPost, True>>>,
                    FSServiceOrder,
                    InnerJoin<FSSODet,
                    On<
                        FSSODet.sOID, Equal<FSServiceOrder.sOID>>>,
                    Where<
                        FSSODet.postID, Equal<Required<FSSODet.postID>>,
                    And<
                        FSServiceOrder.pendingAPARSOPost, Equal<False>,
                    And<
                        FSServiceOrder.postedBy, Equal<FSServiceOrder.postedBy.ServiceOrder>>>>>
                    .Update(cleanerGraph, fsPostDetRow.PostID);

                    if (batchLoadedSuccessfully == true)
                    graphPostBatchMaint.BatchDetails.Delete(fsPostDetRow);
                }

                int docCount = PXSelect<FSPostDoc,
                    Where<FSPostDoc.batchID, Equal<Required<FSPostDoc.batchID>>>>.
                    Select(graphPostBatchMaint, batchID).Count();

                if (batchLoadedSuccessfully == true)
                {
                if (docCount == 0)
                {
                    //Erasing batch if there are no detail lines.
                    graphPostBatchMaint.BatchRecords.Delete(graphPostBatchMaint.BatchRecords.Current);
                }
                else
                {
                    graphPostBatchMaint.BatchRecords.Current.QtyDoc = docCount;
                    graphPostBatchMaint.BatchRecords.Update(graphPostBatchMaint.BatchRecords.Current);
                }

                graphPostBatchMaint.Save.Press();
            }
        }
        }

        public static bool AreAppointmentsPostedInSO(PXGraph graph, int? sOID)
        {
            if (sOID == null)
            {
                return false;
            }

            return PXSelectReadonly<FSAppointment,
                   Where<
                       FSAppointment.pendingAPARSOPost, Equal<False>,
                   And<
                       FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>>>>
                   .Select(graph, sOID).Count() > 0;
        }

        public static int? GetServiceOrderFromAppPostID(PXGraph graph, int? postID)
        {
            if (postID == null)
            {
                return null;
            }

            FSAppointment fsAppointmentRow = PXSelectJoin<FSAppointment,
                                             InnerJoin<FSAppointmentDet,
                                                  On<FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>>>,
                                             Where<
                                                  FSAppointmentDet.postID, Equal<Required<FSAppointmentDet.postID>>>>
                                             .Select(graph, postID);

            return fsAppointmentRow != null ? fsAppointmentRow.SOID : null;
        }

        public static int? GetServiceOrderFromSOPostID(PXGraph graph, int? postID)
        {
            if (postID == null)
            {
                return null;
            }

            FSServiceOrder fsServiceOrderRow = PXSelectJoin<FSServiceOrder,
                                               InnerJoin<FSSODet,
                                                    On<FSSODet.sOID, Equal<FSServiceOrder.sOID>>>,
                                               Where<
                                                      FSSODet.postID, Equal<Required<FSSODet.postID>>>>
                                               .Select(graph, postID);

            return fsServiceOrderRow != null ? fsServiceOrderRow.SOID : null;
        }

        private static void GetShippingContactAddress(PXGraph graph,
                                                      string contactAddressSource,
                                                      int? billCustomerID,
                                                      FSServiceOrder fsServiceOrderRow,
                                                      out IContact contactRow,
                                                      out IAddress addressRow)
        {
            contactRow = null;
            addressRow = null;
            PXResult<Location, Contact, Address> locData = null;

            switch (contactAddressSource)
            {
                // The name of the following constant and its corresponding label
                // does not correspond with the data sought.
                case ID.Ship_To.BILLING_CUSTOMER_BILL_TO:
                    PXResult<Location, Customer, Contact, Address> defaultLocData = null;
                    defaultLocData = GetContactAddressFromDefaultLocation(graph, billCustomerID);

                    contactRow = ContactAddressHelper.GetIContact(defaultLocData);
                    addressRow = ContactAddressHelper.GetIAddress(defaultLocData);

                    break;

                case ID.Ship_To.SERVICE_ORDER_ADDRESS:
                    GetSrvOrdContactAddress(graph, fsServiceOrderRow, out FSContact fsContact, out FSAddress fsAddress);
                    contactRow = fsContact;
                    addressRow = fsAddress;

                    break;

                case ID.Ship_To.SO_CUSTOMER_LOCATION:
                    locData = GetContactAndAddressFromLocation(graph, fsServiceOrderRow.LocationID);
                    contactRow = ContactAddressHelper.GetIContact(locData);
                    addressRow = ContactAddressHelper.GetIAddress(locData);

                    break;

                case ID.Ship_To.SO_BILLING_CUSTOMER_LOCATION:
                    locData = GetContactAndAddressFromLocation(graph, fsServiceOrderRow.BillLocationID);
                    contactRow = ContactAddressHelper.GetIContact(locData);
                    addressRow = ContactAddressHelper.GetIAddress(locData);

                    break;
            }
        }

        public static void ApplyInvoiceActions(PXGraph graph, CreateInvoiceFilter filter, Guid currentProcessID)
        {
            switch (filter.PostTo)
            {
                case ID.Batch_PostTo.SO:

                    if (filter.EmailSalesOrder == true
                        || filter.PrepareInvoice == true
                            || filter.SOQuickProcess == true)
                    {
                        SOOrderEntry soOrderEntryGraph = PXGraph.CreateInstance<SOOrderEntry>();

                        var rows = PXSelectJoin<SOOrder,
                                   InnerJoin<FSPostDoc,
                                        On<FSPostDoc.postRefNbr, Equal<SOOrder.orderNbr>,
                                        And<
                                            Where<FSPostDoc.postOrderType, Equal<SOOrder.orderType>,
                                                Or<FSPostDoc.postOrderTypeNegativeBalance, Equal<SOOrder.orderType>>>>>>,
                                   Where<FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>>>
                                   .Select(graph, currentProcessID);

                        foreach (var row in rows)
                        {
                            SOOrder sOOrder = (SOOrder)row;
                            soOrderEntryGraph.Document.Current = soOrderEntryGraph.Document.Search<SOOrder.orderNbr>(sOOrder.OrderNbr, sOOrder.OrderType);

                            if (sOOrder.Hold == true)
                            {
                                soOrderEntryGraph.Document.Cache.SetValueExt<SOOrder.hold>(sOOrder, false);
                                soOrderEntryGraph.Save.Press();
                            }

                            PXAdapter adapterSO = new PXAdapter(soOrderEntryGraph.CurrentDocument);

                            if (filter.EmailSalesOrder == true)
                            {
                                var args = new Dictionary<string, object>();
                                args["notificationCD"] = "SALES ORDER";

                                adapterSO.Arguments = args;

                                soOrderEntryGraph.notification.PressButton(adapterSO);
                            }

                            if (filter.SOQuickProcess == true
                                    && soOrderEntryGraph.soordertype.Current != null
                                        && soOrderEntryGraph.soordertype.Current.AllowQuickProcess == true)
                            {
                                SO.SOOrderEntry.SOQuickProcess.InitQuickProcessPanel(soOrderEntryGraph, "");
                                PXQuickProcess.Start(soOrderEntryGraph, sOOrder, soOrderEntryGraph.SOQuickProcessExt.QuickProcessParameters.Current);
                            }
                            else
                            {
                                if (filter.PrepareInvoice == true)
                                {
                                    if (soOrderEntryGraph.prepareInvoice.GetEnabled() == true)
                                    {
                                        adapterSO.MassProcess = true;
                                        soOrderEntryGraph.prepareInvoice.PressButton(adapterSO);
                                    }

                                    if (filter.ReleaseInvoice == true)
                                    {
                                        var shipmentsList = soOrderEntryGraph.shipmentlist.Select();

                                        if (shipmentsList.Count > 0)
                                        {
                                            SOOrderShipment soOrderShipmentRow = shipmentsList[0];
                                            SOInvoiceEntry soInvoiceEntryGraph = PXGraph.CreateInstance<SOInvoiceEntry>();
                                            soInvoiceEntryGraph.Document.Current = soInvoiceEntryGraph.Document.Search<ARInvoice.docType, ARInvoice.refNbr>(soOrderShipmentRow.InvoiceType, soOrderShipmentRow.InvoiceNbr, soOrderShipmentRow.InvoiceType);

                                            PXAdapter adapterAR = new PXAdapter(soInvoiceEntryGraph.CurrentDocument);
                                            adapterAR.MassProcess = true;

                                            soInvoiceEntryGraph.release.PressButton(adapterAR);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    break;

                // @TODO AC-142850
                case ID.Batch_PostTo.AR_AP:
                    break;
            }
        }

        public static Exception GetErrorInfoInLines(List<ErrorInfo> errorInfoList, Exception e)
        {
            StringBuilder errorMsgBuilder = new StringBuilder();

            errorMsgBuilder.Append(e.Message.EnsureEndsWithDot() + " ");

            foreach (ErrorInfo errorInfo in errorInfoList)
            {
                errorMsgBuilder.Append(errorInfo.ErrorMessage.EnsureEndsWithDot() + " ");
            }

            return new PXException(errorMsgBuilder.ToString().TrimEnd());
        }
    }
}
