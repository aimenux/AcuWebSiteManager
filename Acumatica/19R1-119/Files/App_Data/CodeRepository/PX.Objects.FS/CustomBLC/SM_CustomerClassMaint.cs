using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.SM;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class SM_CustomerClassMaint : PXGraphExtension<CustomerClassMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region Selects

        public PXSelectJoin<FSCustomerClassBillingSetup,
               CrossJoin<FSSetup>,
               Where2<
                    Where<
                        FSSetup.customerMultipleBillingOptions, Equal<True>,
                        And<FSCustomerClassBillingSetup.customerClassID, Equal<Current<CustomerClass.customerClassID>>,
                        And<FSCustomerClassBillingSetup.srvOrdType, IsNotNull>>>,
                    Or<
                        Where<
                            FSSetup.customerMultipleBillingOptions, Equal<False>,
                            And<FSCustomerClassBillingSetup.customerClassID, Equal<Current<CustomerClass.customerClassID>>,
                            And<FSCustomerClassBillingSetup.srvOrdType, IsNull>>>>>>
                BillingCycles;
        
        #endregion

        #region Private Methods

        private bool IsThisLineValid(FSCustomerClassBillingSetup fsCustomerClassBillingSetupRow_Current)
        {
            int count = 0;

            foreach (FSCustomerClassBillingSetup fsCustomerClassBillingRow in BillingCycles.Select())
            {
                if (fsCustomerClassBillingSetupRow_Current.SrvOrdType != null
                        && fsCustomerClassBillingSetupRow_Current.SrvOrdType.Equals(fsCustomerClassBillingRow.SrvOrdType))
                {
                    count++;
                }
            }

            return count <= 1;
        }

        private void DisplayBillingOptions(PXCache cache, CustomerClass customerClassRow, FSxCustomerClass fsxCustomerClassRow)
        {
            FSSetup fsSetupRow = PXSelect<FSSetup>.Select(Base);

            bool enableMultipleServicesBilling = fsSetupRow != null ? fsSetupRow.CustomerMultipleBillingOptions == true : false;

            PXUIFieldAttribute.SetVisible<FSxCustomerClass.dfltBillingCycleID>(cache, customerClassRow, !enableMultipleServicesBilling);
            PXUIFieldAttribute.SetVisible<FSxCustomerClass.sendInvoicesTo>(cache, customerClassRow, !enableMultipleServicesBilling);
            PXUIFieldAttribute.SetVisible<FSxCustomerClass.billShipmentSource>(cache, customerClassRow, !enableMultipleServicesBilling);

            BillingCycles.AllowSelect = enableMultipleServicesBilling;

            if (fsxCustomerClassRow != null)
            {
                FSBillingCycle fsBillingCycleRow = PXSelect<FSBillingCycle,
                                                    Where<
                                                        FSBillingCycle.billingCycleID, Equal<Required<FSBillingCycle.billingCycleID>>>>
                                                    .Select(Base, fsxCustomerClassRow.DfltBillingCycleID);

                bool forbidUpdateBillingOptions = SharedFunctions.IsNotAllowedBillingOptionsModification(fsBillingCycleRow);

                PXUIFieldAttribute.SetEnabled<FSxCustomerClass.sendInvoicesTo>(
                                                    cache,
                                                    customerClassRow,
                                                    forbidUpdateBillingOptions == false);

                PXUIFieldAttribute.SetEnabled<FSxCustomerClass.billShipmentSource>(
                                                    cache,
                                                    customerClassRow,
                                                    forbidUpdateBillingOptions == false);

                PXUIFieldAttribute.SetEnabled<FSxCustomerClass.dfltBillingCycleID>(
                                                    cache,
                                                    customerClassRow);

                if (fsxCustomerClassRow.DfltBillingCycleID != null
                        && forbidUpdateBillingOptions == false)
                {
                    PXDefaultAttribute.SetPersistingCheck<FSxCustomerClass.sendInvoicesTo>(cache, customerClassRow, PXPersistingCheck.NullOrBlank);
                    PXDefaultAttribute.SetPersistingCheck<FSxCustomerClass.billShipmentSource>(cache, customerClassRow, PXPersistingCheck.NullOrBlank);
                }
                else
                {
                    PXDefaultAttribute.SetPersistingCheck<FSxCustomerClass.sendInvoicesTo>(cache, customerClassRow, PXPersistingCheck.Nothing);
                    PXDefaultAttribute.SetPersistingCheck<FSxCustomerClass.billShipmentSource>(cache, customerClassRow, PXPersistingCheck.Nothing);
                }
            }
        }

        private void ResetSendInvoicesToFromBillingCycle(CustomerClass customerClassRow, FSCustomerClassBillingSetup fsCustomerClassBillingSetupRow)
        {
            List<object> args = new List<object>();
            FSBillingCycle fsBillingCycleRow = null;
            BqlCommand billingCycleCommand = 
                new Select<FSBillingCycle,
                    Where<
                        FSBillingCycle.billingCycleID, Equal<Required<FSBillingCycle.billingCycleID>>>>();

            PXView billingCycleView = new PXView(Base, true, billingCycleCommand);

            if (customerClassRow != null)
            {
                FSxCustomerClass fsxCustomerClassRow = PXCache<CustomerClass>.GetExtension<FSxCustomerClass>(customerClassRow);
                args.Add(fsxCustomerClassRow.DfltBillingCycleID);

                fsBillingCycleRow = (FSBillingCycle)billingCycleView.SelectSingle(args.ToArray());

                if (fsBillingCycleRow != null)
                {
                    if (SharedFunctions.IsNotAllowedBillingOptionsModification(fsBillingCycleRow))
                    {
                        fsxCustomerClassRow.SendInvoicesTo = ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO;
                        fsxCustomerClassRow.BillShipmentSource = ID.Ship_To.SERVICE_ORDER_ADDRESS;
                    }
                }
            }
            else if (fsCustomerClassBillingSetupRow != null)
            {
                args.Add(fsCustomerClassBillingSetupRow.BillingCycleID);
                fsBillingCycleRow = (FSBillingCycle)billingCycleView.SelectSingle(args.ToArray());

                if (fsBillingCycleRow != null)
                {
                    if (SharedFunctions.IsNotAllowedBillingOptionsModification(fsBillingCycleRow))
                    {
                        fsCustomerClassBillingSetupRow.SendInvoicesTo = ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO;
                        fsCustomerClassBillingSetupRow.BillShipmentSource = ID.Ship_To.SERVICE_ORDER_ADDRESS;
                    }
                }
            }
        }

        private void EnableDisableCustomerBilling(PXCache cache, CustomerClass customerClassRow, FSxCustomerClass fsxCustomerClassRow)
        {
            bool isSpecificCustomer = fsxCustomerClassRow.DefaultBillingCustomerSource == ID.Default_Billing_Customer_Source.SPECIFIC_CUSTOMER;

            PXUIFieldAttribute.SetVisible<FSxCustomerClass.billCustomerID>(cache, customerClassRow, isSpecificCustomer);
            PXUIFieldAttribute.SetVisible<FSxCustomerClass.billLocationID>(cache, customerClassRow, isSpecificCustomer);
            PXDefaultAttribute.SetPersistingCheck<FSxCustomerClass.billCustomerID>(cache, customerClassRow, isSpecificCustomer == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
            PXDefaultAttribute.SetPersistingCheck<FSxCustomerClass.billLocationID>(cache, customerClassRow, isSpecificCustomer == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

            if (isSpecificCustomer == false)
            {
                fsxCustomerClassRow.BillCustomerID = null;
                fsxCustomerClassRow.BillLocationID = null;
            }
        }
        #endregion

        #region Event Handlers

        protected void CustomerClass_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            CustomerClass customerClassRow = (CustomerClass)e.Row;
            FSxCustomerClass fsxCustomerClassRow = cache.GetExtension<FSxCustomerClass>(customerClassRow);
            this.DisplayBillingOptions(cache, customerClassRow, fsxCustomerClassRow);
            EnableDisableCustomerBilling(cache, customerClassRow, fsxCustomerClassRow);
        }

        protected void FSCustomerClassBillingSetup_BillingCycleID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSCustomerClassBillingSetup fsCustomerClassBillingSetupRow = (FSCustomerClassBillingSetup)e.Row;
            this.ResetSendInvoicesToFromBillingCycle(null, fsCustomerClassBillingSetupRow);
        }

        protected void FSCustomerClassBillingSetup_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSCustomerClassBillingSetup fsCustomerClassBillingSetupRow = (FSCustomerClassBillingSetup)e.Row;

            if (!IsThisLineValid(fsCustomerClassBillingSetupRow))
            {
                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.billingCycleID>(cache, fsCustomerClassBillingSetupRow, false);
                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.sendInvoicesTo>(cache, fsCustomerClassBillingSetupRow, false);
                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.billShipmentSource>(cache, fsCustomerClassBillingSetupRow, false);
                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.frequencyType>(cache, fsCustomerClassBillingSetupRow, false);
                return;
            }

            bool enableBillingCycleIDField = fsCustomerClassBillingSetupRow.BillingCycleID == null && string.IsNullOrEmpty(fsCustomerClassBillingSetupRow.SrvOrdType) == false;

            PXUIFieldAttribute.SetEnabled<FSCustomerBillingSetup.billingCycleID>(cache, fsCustomerClassBillingSetupRow, enableBillingCycleIDField);

            //Disables the TimeCycleType field if the type of the BillingCycleID selected is Time Cycle.
            if (fsCustomerClassBillingSetupRow.BillingCycleID != null)
            {
                FSBillingCycle fsBillingCycleRow =
                    PXSelect<FSBillingCycle,
                    Where<
                        FSBillingCycle.billingCycleID, Equal<Required<FSBillingCycle.billingCycleID>>>>
                    .Select(Base, fsCustomerClassBillingSetupRow.BillingCycleID);

                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.frequencyType>(cache, fsCustomerClassBillingSetupRow, fsBillingCycleRow.BillingCycleType != ID.Billing_Cycle_Type.TIME_FRAME);

                bool forbidUpdateBillingOptions = SharedFunctions.IsNotAllowedBillingOptionsModification(fsBillingCycleRow);

                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.sendInvoicesTo>(cache, fsCustomerClassBillingSetupRow, forbidUpdateBillingOptions == false);
                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.billShipmentSource>(cache, fsCustomerClassBillingSetupRow, forbidUpdateBillingOptions == false);
            }
            else
            {
                PXUIFieldAttribute.SetEnabled<FSCustomerClassBillingSetup.frequencyType>(cache, fsCustomerClassBillingSetupRow, false);
            }
        }

        #endregion
    }
}
