using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    #region Lookups Group - BusinessAccount (Customer, Vendor, Combined and Prospect)

    #region BusinessAccount - Base

    public class FSSelectorBAccount_BaseAttribute : PXDimensionSelectorAttribute
    {
        public FSSelectorBAccount_BaseAttribute(string dimensionName, Type whereType)
            : base(
                dimensionName,
                BqlCommand.Compose(
                            typeof(Search2<,,>),
                            typeof(BAccount.bAccountID),
                            typeof(LeftJoin<,,>),
                            typeof(Contact),
                            typeof(On<
                                    Contact.bAccountID, Equal<BAccount.bAccountID>,
                                    And<Contact.contactID, Equal<BAccount.defContactID>>>),
                            typeof(LeftJoin<,,>),
                            typeof(Address),
                            typeof(On<
                                    Address.bAccountID, Equal<BAccount.bAccountID>,
                                    And<Address.addressID, Equal<BAccount.defAddressID>>>),
                            typeof(LeftJoin<,>),
                            typeof(Customer),
                            typeof(On<
                                    Customer.bAccountID, Equal<BAccount.bAccountID>>),
                            typeof(Where2<,>),
                            whereType,
                            typeof(And<Match<Current<AccessInfo.userName>>>)),
                typeof(BAccount.acctCD),
                new Type[]
                {
                    typeof(BAccount.acctCD),
                    typeof(BAccount.acctName),
                    typeof(BAccount.type),
                    typeof(Customer.customerClassID),
                    typeof(BAccount.status),
                    typeof(Contact.phone1),
                    typeof(Address.addressLine1),
                    typeof(Address.addressLine2),
                    typeof(Address.postalCode),
                    typeof(Address.city),
                    typeof(Address.countryID)
                })
        {
            DescriptionField = typeof(BAccount.acctName);
            Filterable = true;
        }
    }

    public class FSSelectorBusinessAccount_BaseAttribute : PXDimensionSelectorAttribute
    {
        public static readonly Type[] BAccountSelectorColumns = new Type[]
                {
                    typeof(BAccountSelectorBase.acctCD),
                    typeof(BAccountSelectorBase.acctName),
                    typeof(BAccountSelectorBase.type),
                    typeof(Customer.customerClassID),
                    typeof(BAccountSelectorBase.status),
                    typeof(Contact.phone1),
                    typeof(Address.addressLine1),
                    typeof(Address.addressLine2),
                    typeof(Address.postalCode),
                    typeof(Address.city),
                    typeof(Address.countryID)
                };
        public static readonly Type[] CustomerSelectorColumns = new Type[]
        {
                    typeof(Customer.acctCD),
                    typeof(Customer.acctName),
                    typeof(Customer.type),
                    typeof(Customer.customerClassID),
                    typeof(Customer.status),
                    typeof(Contact.phone1),
                    typeof(Address.addressLine1),
                    typeof(Address.addressLine2),
                    typeof(Address.postalCode),
                    typeof(Address.city),
                    typeof(Address.countryID)
        };
        public static readonly Type[] VendorSelectorColumns = new Type[]
                {
                    typeof(Vendor.acctCD),
                    typeof(Vendor.acctName),
                    typeof(Vendor.type),
                    typeof(Vendor.vendorClassID),
                    typeof(Vendor.status),
                    typeof(Contact.phone1),
                    typeof(Address.addressLine1),
                    typeof(Address.addressLine2),
                    typeof(Address.postalCode),
                    typeof(Address.city),
                    typeof(Address.countryID)
                };

        public FSSelectorBusinessAccount_BaseAttribute(string dimensionName, Type whereType)
            : base(
                dimensionName,
                BqlCommand.Compose(
                            typeof(Search2<,,>),
                            typeof(BAccountSelectorBase.bAccountID),
                            typeof(LeftJoin<,,>),
                            typeof(Contact),
                            typeof(On<
                                    Contact.bAccountID, Equal<BAccountSelectorBase.bAccountID>,
                                    And<Contact.contactID, Equal<BAccountSelectorBase.defContactID>>>),
                            typeof(LeftJoin<,,>),
                            typeof(Address),
                            typeof(On<
                                    Address.bAccountID, Equal<BAccountSelectorBase.bAccountID>,
                                    And<Address.addressID, Equal<BAccountSelectorBase.defAddressID>>>),
                            typeof(LeftJoin<,>),
                            typeof(Customer),
                            typeof(On<
                                    Customer.bAccountID, Equal<BAccountSelectorBase.bAccountID>>),
                            typeof(Where2<,>),
                            whereType,
							typeof(And<Match<Current<AccessInfo.userName>>>)),
                typeof(BAccountSelectorBase.acctCD),
                FSSelectorBusinessAccount_BaseAttribute.BAccountSelectorColumns)
        {
            DescriptionField = typeof(BAccountSelectorBase.acctName);
        }
    }
    #endregion

    #region BusinessAccount - Customer
    // TODO: AC-137974 Delete this Selector
    public class FSSelectorBAccountTypeCustomerOrCombinedAttribute : FSSelectorCustomerAttribute
    {
    }

    public class FSSelectorBAccountCustomerOrCombinedAttribute : FSSelectorBusinessAccount_BaseAttribute
    {
        public FSSelectorBAccountCustomerOrCombinedAttribute()
            : base(
                CustomerAttribute.DimensionName,
                typeof(Where<
                            BAccountSelectorBase.type, Equal<BAccountType.customerType>,
                       Or<
                           BAccountSelectorBase.type, Equal<BAccountType.combinedType>>>))
        {
        }
    }
    #endregion

    #region BusinessAccount - Vendors
    // TODO: AC-137974 rename it to FSSelectorVendors
    public class FSSelectorBusinessAccount_VEAttribute : PXDimensionSelectorAttribute
    {
        public FSSelectorBusinessAccount_VEAttribute()
            : base(
                VendorAttribute.DimensionName,
                BqlCommand.Compose(
                            typeof(Search2<,,>),
                            typeof(Vendor.bAccountID),
                            typeof(LeftJoin<,,>),
                            typeof(Contact),
                            typeof(On<
                                    Contact.bAccountID, Equal<Vendor.bAccountID>,
                                    And<Contact.contactID, Equal<Vendor.defContactID>>>),
                            typeof(LeftJoin<,>),
                            typeof(Address),
                            typeof(On<
                                    Address.bAccountID, Equal<Vendor.bAccountID>,
                                    And<Address.addressID, Equal<Vendor.defAddressID>>>),
                            typeof(Where<Match<Vendor, Current<AccessInfo.userName>>>)
                            ),
                typeof(Vendor.acctCD),
                FSSelectorBusinessAccount_BaseAttribute.VendorSelectorColumns)
        {
            DescriptionField = typeof(Vendor.acctName);
        }
    }
    #endregion

    #region BusinessAccount - Customers and Prospects
    // TODO: AC-137974 rename it to FSSelectorBusinessAccount_CustomersAndProspects
    public class FSSelectorBusinessAccount_CU_PR_VCAttribute : FSSelectorBusinessAccount_BaseAttribute
    {
        public FSSelectorBusinessAccount_CU_PR_VCAttribute()
            : base(

                BAccountAttribute.DimensionName, 
                typeof(
                    Where2<
                        Where<BAccountSelectorBase.type, Equal<BAccountType.prospectType>,
                            And<Customer.bAccountID, IsNull>>,
                        Or<
                            Where<
                                Where2<
                                    Where<BAccountSelectorBase.type, Equal<BAccountType.customerType>,
                                        Or<BAccountSelectorBase.type, Equal<BAccountType.combinedType>>>,
                                    And<
                                        Customer.bAccountID, IsNotNull,
                                        And<Match<Customer, Current<AccessInfo.userName>>>>>>>>))
        {
        }
    }

    [Obsolete("Remove in major release")]
    public class FSSelectorBAccount_CU_PR_VCAttribute : FSSelectorBAccount_BaseAttribute
    {
        public FSSelectorBAccount_CU_PR_VCAttribute()
            : base(
                CustomerAttribute.DimensionName,
                typeof(Where<
                            BAccount.type, Equal<BAccountType.customerType>,
                       Or<
                           BAccount.type, Equal<BAccountType.prospectType>,
                       Or<
                           BAccount.type, Equal<BAccountType.combinedType>>>>))
        {
        }
    }
    #endregion

    #region Customer
    public class FSSelectorCustomerAttribute : PXDimensionSelectorAttribute
    {
        public FSSelectorCustomerAttribute()
            : base(
                CustomerAttribute.DimensionName,
                BqlCommand.Compose(
                    typeof(Search2<,,>),
                    typeof(Customer.bAccountID),
                    typeof(LeftJoin<,,>),
                    typeof(Contact),
                    typeof(On<
                            Contact.bAccountID, Equal<Customer.bAccountID>,
                            And<Contact.contactID, Equal<Customer.defContactID>>>),
                    typeof(LeftJoin<,>),
                    typeof(Address),
                    typeof(On<
                            Address.bAccountID, Equal<Customer.bAccountID>,
                            And<Address.addressID, Equal<Customer.defAddressID>>>),
                    typeof(Where<Match<Customer, Current<AccessInfo.userName>>>)
                    ),
                typeof(Customer.acctCD),
                FSSelectorBusinessAccount_BaseAttribute.CustomerSelectorColumns)
        {
            DescriptionField = typeof(Customer.acctName);
            Filterable = true;
        }
    }

    public class FSCustomerAttribute : PXDimensionSelectorAttribute
    {
        public FSCustomerAttribute()
            : base(
                CustomerAttribute.DimensionName,
                typeof(Search2<Customer.bAccountID,
                       LeftJoin<Contact,
                       On<
                           Contact.bAccountID, Equal<Customer.bAccountID>,
                           And<Contact.contactID, Equal<Customer.defContactID>>>,
                       LeftJoin<Address,
                       On<
                           Address.bAccountID, Equal<Customer.bAccountID>,
                           And<Address.addressID, Equal<Customer.defAddressID>>>>>,
                       Where<
                           Customer.type, Equal<BAccountType.customerType>,
                       Or<
                           Customer.type, Equal<BAccountType.combinedType>>>>),
                typeof(Customer.acctCD),
                new Type[]
                {
                    typeof(Customer.acctCD),
                    typeof(Customer.status),
                    typeof(Customer.acctName),
                    typeof(Customer.classID),
                    typeof(Contact.phone1),
                    typeof(Address.city),
                    typeof(Address.countryID)
                })
        {
            DescriptionField = typeof(Customer.acctName);
            DirtyRead = true;
            Filterable = true;
        }
    }
    #endregion

    #region Contract Schedule Customer
    public class FSSelectorContractScheduleCustomerAttribute : PXDimensionSelectorAttribute
    {
        public FSSelectorContractScheduleCustomerAttribute(Type whereType)
            : base(
                CustomerAttribute.DimensionName,
                BqlCommand.Compose(
                            typeof(Search5<,,,>),
                            typeof(Customer.bAccountID),
                            typeof(InnerJoin<,,>),
                            typeof(FSServiceContract),
                            typeof(On<
                                    FSServiceContract.customerID, Equal<Customer.bAccountID>>),
                            typeof(LeftJoin<,,>),
                            typeof(Contact),
                            typeof(On<
                                    Contact.bAccountID, Equal<Customer.bAccountID>,
                                    And<Contact.contactID, Equal<Customer.defContactID>>>),
                            typeof(LeftJoin<,>),
                            typeof(Address),
                            typeof(On<
                                    Address.bAccountID, Equal<Customer.bAccountID>,
                                    And<Address.addressID, Equal<Customer.defAddressID>>>),
                            typeof(Where2<,>),
                            typeof(Where<Match<Customer, Current<AccessInfo.userName>>>),
                            typeof(And<>),
                            whereType,
                            typeof(Aggregate<>),
                            typeof(GroupBy<Customer.bAccountID>)),
                typeof(Customer.acctCD),
                FSSelectorBusinessAccount_BaseAttribute.CustomerSelectorColumns)
                {
            DescriptionField = typeof(Customer.acctName);
        }
    }
    #endregion

    #endregion

    #region Contact
    public class FSSelectorContactAttribute : PXSelectorAttribute
    {
        public FSSelectorContactAttribute()
            : base(
                typeof(Search2<Contact.contactID,
                       InnerJoin<BAccount,
                       On<
                           BAccount.bAccountID, Equal<Contact.bAccountID>>>,
                       Where<
                           Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>,
                       And<
                           Where2<
                               Where<
                                   BAccount.type, Equal<BAccountType.customerType>,
                               Or<
                                   BAccount.type, Equal<BAccountType.prospectType>,
                               Or<
                                   BAccount.type, Equal<BAccountType.combinedType>>>>,
                           And<
                               Where<
                                   BAccount.bAccountID, Equal<Current<FSServiceOrder.customerID>>,
                               Or<
                                   Current<FSServiceOrder.customerID>, IsNull>>>>>>>))
        {
            SubstituteKey = typeof(Contact.displayName);
            Filterable = true;
            DirtyRead = true;
        }
    }

    #endregion

    #region Contract
    public class FSSelectorContractAttribute : PXSelectorAttribute
    {
        public FSSelectorContractAttribute()
            : base(
                typeof(Search<Contract.contractID,
                       Where<
                           Contract.baseType, Equal<CTPRType.contract>,
                       And<
                           Where<
                               Current<FSServiceOrder.customerID>, IsNull,
                           Or<
                               Contract.customerID, Equal<Current<FSServiceOrder.customerID>>,
                               And<
                                   Where<
                                       Current<FSServiceOrder.locationID>, IsNull,
                                   Or<
                                       Contract.locationID, Equal<Current<FSServiceOrder.locationID>>>>>>>>>,
                       OrderBy<
                           Desc<Contract.contractCD>>>))
        {
            SubstituteKey = typeof(Contract.contractCD);
            Filterable = true;
        }
    }

    public class FSSelectorContractRefNbrAttributeAttribute : PXSelectorAttribute
    {
        public FSSelectorContractRefNbrAttributeAttribute(Type serviceContract_RecordType)
            : this(serviceContract_RecordType, typeof(Where<True, Equal<True>>))
        {
        }

        public FSSelectorContractRefNbrAttributeAttribute(Type serviceContract_RecordType, Type TWhere)
            : base(
                  BqlTemplate.OfCommand<
                    Search2<FSServiceContract.refNbr,
                        LeftJoin<Customer,
                            On<Customer.bAccountID, Equal<FSServiceContract.customerID>>,
                        LeftJoin<Address,
                            On<Address.bAccountID, Equal<Customer.bAccountID>,
                                And<Address.addressID, Equal<Customer.defAddressID>>>>>,
                        Where<
                            FSServiceContract.recordType, Equal<BqlPlaceholder.A>,
                            And2<
                                Where<Customer.bAccountID, IsNull, Or<Match<Customer, Current<AccessInfo.userName>>>>,
                            And<BqlPlaceholder.B>>>,
                        OrderBy<Desc<FSServiceContract.refNbr>>>>.
                  Replace<BqlPlaceholder.A>(serviceContract_RecordType).
                  Replace<BqlPlaceholder.B>(TWhere).
                  ToType(),
                new Type[]
                {
                    typeof(FSServiceContract.refNbr),
                    typeof(FSServiceContract.customerContractNbr),
                    typeof(FSServiceContract.customerID),
                    typeof(Customer.acctName), 
                    typeof(FSServiceContract.status), 
                    typeof(FSServiceContract.customerLocationID),
                    typeof(Address.addressLine1),
                    typeof(Address.city),
                    typeof(Address.state)
                })
        {
        }
    }

    public class FSSelectorCustomerContractNbrAttributeAttribute : PXSelectorAttribute
    {
        public FSSelectorCustomerContractNbrAttributeAttribute(Type serviceContract_RecordType, Type CurrentCustomer)
            : this(serviceContract_RecordType, CurrentCustomer, typeof(Where<True, Equal<True>>))
        {
        }

        public FSSelectorCustomerContractNbrAttributeAttribute(Type serviceContract_RecordType, Type CurrentCustomer, Type Where)
            : base(
                    BqlCommand.Compose(typeof(Search2<,,,>),
                                            typeof(FSServiceContract.customerContractNbr),
                                       typeof(LeftJoin<,,>),
                                            typeof(Customer),
                                       typeof(On<Customer.bAccountID, Equal<FSServiceContract.customerID>>),
                                       typeof(LeftJoin<,>),
                                       typeof(Address),
                                       typeof(On<
                                                Address.bAccountID, Equal<Customer.bAccountID>,
                                                And<Address.addressID, Equal<Customer.defAddressID>>>),
                                        typeof(Where2<,>),
                                       typeof(Where<,,>),
                                            typeof(FSServiceContract.customerID),
                                            typeof(Equal<>),
                                            typeof(Current<>),
                                            CurrentCustomer,
                                       typeof(And<,,>),
                                            typeof(FSServiceContract.recordType),
                                       typeof(Equal<>),
                                            serviceContract_RecordType,
                                        typeof(And<Where<
                                            Customer.bAccountID, IsNull,
                                            Or<Match<Customer, Current<AccessInfo.userName>>>>>),
                                       typeof(And<>),
                                            Where,
                                       typeof(OrderBy<Desc<FSServiceContract.customerContractNbr>>)),
                new Type[]
                {
                    typeof(FSServiceContract.refNbr),
                    typeof(FSServiceContract.customerID),
                    typeof(Customer.acctName),
                    typeof(FSServiceContract.status),
                    typeof(FSServiceContract.customerLocationID),
                    typeof(Address.addressLine1),
                    typeof(Address.city),
                    typeof(Address.state)
                })
        {
        }
    }


    public class FSSelectorPrepaidServiceContract : PXSelectorAttribute
    {
        public FSSelectorPrepaidServiceContract(Type currentCustomerID, Type currentBillingCustomerID)
            : base(
                  BqlCommand.Compose(
                      typeof(Search<,,>),
                            typeof(FSServiceContract.serviceContractID),
                      typeof(Where2<,>),
                            typeof(Where<,,>),
                                typeof(FSServiceContract.customerID),
                                typeof(Equal<>),
                                    typeof(Current<>),
                                        currentCustomerID,
                                typeof(And<>),
                                    typeof(Where<,>),
                                        typeof(FSServiceContract.billCustomerID),
                                        typeof(Equal<>),
                                            typeof(Current<>),
                                                currentBillingCustomerID,
                      typeof(And<
                                Where<FSServiceContract.status, Equal<FSServiceContract.status.Active>,
                                And<FSServiceContract.billingType, Equal<FSServiceContract.billingType.StandardizedBillings>>>>),
                      typeof(OrderBy<Asc<FSServiceContract.refNbr>>)),
                  new Type[]
                {
                    typeof(FSServiceContract.customerID),
                    typeof(FSServiceContract.customerLocationID),
                    typeof(FSServiceContract.refNbr),
                    typeof(FSServiceContract.customerContractNbr),
                    typeof(FSServiceContract.status),
                    typeof(FSServiceContract.vendorID),
                    typeof(FSServiceContract.sourcePrice),
                    typeof(FSServiceContract.billCustomerID),
                    typeof(FSServiceContract.billLocationID),
                    typeof(FSServiceContract.docDesc),
                })
        {
            SubstituteKey = typeof(FSServiceContract.refNbr);
            DescriptionField = typeof(FSServiceContract.docDesc);
            Filterable = true;
        }
    }
    #endregion

    #region Contract - BillingPeriod

    public class FSSelectorContractBillingPeriodAttribute : PXCustomSelectorAttribute
    {
        public FSSelectorContractBillingPeriodAttribute()
            : base(typeof(FSContractPeriod.contractPeriodID), typeof(FSContractPeriod.billingPeriod), typeof(FSContractPeriod.status), typeof(FSContractPeriod.invoiced))
        {
            this.SubstituteKey = typeof(FSContractPeriod.billingPeriod);
            this.ValidateValue = false;
        }

        protected virtual IEnumerable GetRecords()
        {
            FSContractPeriod currentRow = null;
            PXCache cache = this._Graph.Caches[typeof(FSContractPeriod)];

            foreach (object item in PXView.Currents)
            {
                if (item != null && (item.GetType() == typeof(FSContractPeriod)))
                {
                    currentRow = (FSContractPeriod)item;
                    break;
                }
            }

            if (currentRow == null)
            {
                currentRow = (FSContractPeriod)cache.Current;
            }

            if (currentRow != null)
            {
                if (currentRow.ContractPeriodID > 0)
                {
                    var rows = PXSelect<FSContractPeriod,
                               Where<
                                   FSContractPeriod.serviceContractID, Equal<Current<FSServiceContract.serviceContractID>>>,
                               OrderBy<Desc<FSContractPeriod.endPeriodDate>>>
                               .Select(_Graph);

                    string actionType = ID.ContractPeriod_Actions.SEARCH_BILLING_PERIOD;
                    FSContractPeriodFilter fsContractPeriodFilterRow = (FSContractPeriodFilter)_Graph.Caches[typeof(FSContractPeriodFilter)].Current;

                    if (fsContractPeriodFilterRow != null)
                    {
                        actionType = fsContractPeriodFilterRow.Actions;
                    }

                    if (rows.Count > 0)
                    {
                        foreach (FSContractPeriod row in rows)
                        {
                            FSContractPeriod fsContractPeriodRow = (FSContractPeriod)row;
                            if (fsContractPeriodRow.StartPeriodDate.HasValue
                                    && fsContractPeriodRow.EndPeriodDate.HasValue)
                            {
                                fsContractPeriodRow.BillingPeriod = fsContractPeriodRow.StartPeriodDate.Value.ToString("MM/dd/yyyy") + " - " + fsContractPeriodRow.EndPeriodDate.Value.ToString("MM/dd/yyyy");
                            }

                            if (actionType == ID.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD
                                    && fsContractPeriodRow.Status == ID.Status_ContractPeriod.INACTIVE)
                            {
                                yield return fsContractPeriodRow;
                            }
                            else if (actionType == ID.ContractPeriod_Actions.SEARCH_BILLING_PERIOD
                                        && (fsContractPeriodRow.Status != ID.Status_ContractPeriod.INACTIVE
                                                || fsContractPeriodRow.Invoiced == true))
                            {
                                yield return fsContractPeriodRow;
                            }
                        }
                    }
                }
                else
                {
                    if (currentRow.StartPeriodDate.HasValue && currentRow.EndPeriodDate.HasValue)
                    {
                        currentRow.BillingPeriod = currentRow.StartPeriodDate.Value.ToString("MM/dd/yyyy") + " - " + currentRow.EndPeriodDate.Value.ToString("MM/dd/yyyy");
                    }
                    yield return currentRow;
                }
            }
        }
    }
    #endregion

    #region ContractPeriod - Equipment for Maintenance
    public class FSSelectorContractPeriodEquipmentAttribute : PXSelectorAttribute
    {
        public FSSelectorContractPeriodEquipmentAttribute()
            : base(
                typeof(Search2<FSEquipment.SMequipmentID,
                       CrossJoinSingleTable<FSSetup>,
                       Where<
                           FSEquipment.requireMaintenance, Equal<True>,
                       And<
                           FSSetup.enableAllTargetEquipment, Equal<True>,
                       Or<
                           Where2<
                               /*Begin of Owner is Third Party*/
                               Where<
                                   FSEquipment.ownerType, Equal<ListField_OwnerType_Equipment.Customer>,
                               And<
                                   Where2<
                                       /*Begin of Owner is Customer*/
                                       Where<
                                           FSEquipment.ownerID, Equal<Current<FSServiceContract.customerID>>>,
                                       /*End of Owner is Customer*/
                                       Or<
                                           /*Begin of Location is Customer*/
                                           Where<
                                               FSEquipment.locationType, Equal<ListField_LocationType.Customer>,
                                           And<
                                               FSEquipment.customerID, Equal<Current<FSServiceContract.customerID>>,
                                           And<
                                               Where2<
                                                   Where<
                                                       FSEquipment.customerLocationID, Equal<Current<FSServiceContract.customerLocationID>>>,
                                               Or<
                                                   Where<
                                                       FSEquipment.customerLocationID, IsNull>>>>>>>>>>,
                               /*End of Location is Customer*/
                               /*End of Owner is Third Party and Location is Customer*/
                               Or<
                                   /*Begin of Owner is Own and Location is Customer*/
                                   Where<
                                       FSEquipment.ownerType, Equal<ListField_OwnerType_Equipment.OwnCompany>,
                                   And<
                                       FSEquipment.locationType, Equal<ListField_LocationType.Customer>,
                                   And<
                                       FSEquipment.customerID, Equal<Current<FSServiceContract.customerID>>,
                                   And<
                                       Where2<
                                           Where<
                                               FSEquipment.customerLocationID, Equal<Current<FSServiceContract.customerLocationID>>>,
                                           Or<
                                               Where<
                                                   FSEquipment.customerLocationID, IsNull>>>>>>>>>>>>>),
                    /*End for Owner Company and Location is Customer*/
                    SelectorBase_Equipment.selectorColumns)
        {
            SubstituteKey = typeof(FSEquipment.refNbr);
            DescriptionField = typeof(FSEquipment.descr);
        }
    }
    #endregion

    #region Lookups Group - AccountLocation

    public class FSSelectorLocationAttribute : PXDimensionSelectorAttribute
    {
        public FSSelectorLocationAttribute()
            : base(LocationIDBaseAttribute.DimensionName,
                   typeof(Search<Location.locationID>),
                   typeof(Location.locationCD))
        {
            DescriptionField = typeof(Location.descr);
            DirtyRead = true;
        }

        public FSSelectorLocationAttribute(Type currentBAccountID)
            : base(
                LocationIDBaseAttribute.DimensionName,
                BqlCommand.Compose(typeof(Search<,>),
                                        typeof(Location.locationID),
                                   typeof(Where<,>),
                                        typeof(Location.bAccountID),
                                   typeof(Equal<>),
                                   typeof(Current<>),
                                   currentBAccountID),
                typeof(Location.locationCD))
        {
            DescriptionField = typeof(Location.descr);
            DirtyRead = true;
        }
    }

    #endregion

    #region Lookups Group - Service

    #region ServiceAttribute
    [PXDBInt]
    [PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
    public class ServiceAttribute : FSInventoryAttribute
    {
        private static Type[] defaultHeaders = new Type[]
        {
            typeof(InventoryItem.inventoryCD), 
            typeof(InventoryItem.itemClassID),
            typeof(FSxServiceClass.mem_RouteService),
            typeof(InventoryItem.itemStatus),
            typeof(InventoryItem.descr), 
            typeof(InventoryItem.itemType),
            typeof(InventoryItem.baseUnit),
            typeof(InventoryItem.salesUnit),
            typeof(InventoryItem.purchaseUnit),
            typeof(InventoryItem.basePrice),
            typeof(FSxService.actionType)
        };

        public ServiceAttribute(Type[] headers = null)
            : this(typeof(Where<True, Equal<True>>), headers)
        {
        }
        
        public ServiceAttribute(Type whereType, Type[] headers = null)
            : base(
                    BqlCommand.Compose(
                            typeof(Search2<,,>),
                            typeof(InventoryItem.inventoryID),
                            typeof(InnerJoin<,>),
                            typeof(INItemClass),
                            typeof(On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>),
                            typeof(Where2<,>),
                            typeof(Match<Current<AccessInfo.userName>>),
                            typeof(And2<,>),
                            typeof(Where<
                                        InventoryItem.itemType, Equal<INItemTypes.serviceItem>,
                                        And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
                                        And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
                                        And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>>>>>),
                            typeof(And<>),
                            whereType),
                    typeof(InventoryItem.inventoryCD),
                    typeof(InventoryItem.descr),
                    headers ?? defaultHeaders)
        {
        }
    }
    #endregion

    #region InventoryIDByLineTypeAttribute
    [PXDBInt]
    [PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<
                            InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
                         And<
                            InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
                         And<
                            InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>>>>),
                  IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus))]
    [PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>>), PM.Messages.ReservedForProject)]
    public class InventoryIDByLineTypeAttribute : FSInventoryAttribute
    {
        public class LineType : BqlPlaceholderBase { }
        public class WhereType : BqlPlaceholderBase { }

        private static Type[] defaultHeaders = new Type[]
        {
            typeof(InventoryItem.inventoryCD),
            typeof(InventoryItem.itemClassID),
            typeof(FSxServiceClass.mem_RouteService),
            typeof(InventoryItem.itemStatus),
            typeof(InventoryItem.descr),
            typeof(InventoryItem.itemType),
            typeof(InventoryItem.baseUnit),
            typeof(InventoryItem.salesUnit),
            typeof(InventoryItem.purchaseUnit),
            typeof(InventoryItem.basePrice),
            typeof(FSxService.actionType)
        };

        public InventoryIDByLineTypeAttribute(Type lineType, Type[] headers = null)
            : this(typeof(Where<True, Equal<True>>), lineType, headers)
        {
        }

        public InventoryIDByLineTypeAttribute(Type whereType, Type lineType, Type[] headers = null)
            : base(BqlTemplate.OfCommand<
                   Search5<InventoryItem.inventoryID,
                   LeftJoin<INItemClass,
                   On<
                       INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>,
                   LeftJoin<FSServiceInventoryItem,
                   On<
                       FSServiceInventoryItem.inventoryID, Equal<InventoryItem.inventoryID>>>>,
                   Where2<
                       Where<Match<Current<AccessInfo.userName>>>,
                       And2<
                           Where2<
                               Where<Current<LineType>, IsNull>,
                               Or<
                               Where2<
                                   Where<
                                       Current<LineType>, Equal<ListField_LineType_ALL.Inventory_Item>,
                                   And<
                                       InventoryItem.stkItem, Equal<True>>>,
                               Or2<
                                   Where<
                                       Current<LineType>, Equal<ListField_LineType_ALL.Service>,
                                   And<
                                       InventoryItem.stkItem, Equal<False>, And<InventoryItem.itemType, Equal<INItemTypes.serviceItem>>>>,
                                   Or2<
                                       Where<
                                           Current<LineType>, Equal<ListField_LineType_ALL.NonStockItem>,
                                       And<
                                           InventoryItem.stkItem, Equal<False>, And<InventoryItem.itemType, NotEqual<INItemTypes.serviceItem>>>>,
                                       Or<
                                           Where<
                                               Current<LineType>, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                                           And<
                                               Current<FSAppointmentDet.pickupDeliveryServiceID>, Equal<FSServiceInventoryItem.serviceID>>>>>>>>>,
                           And<WhereType>>>,
                   Aggregate<GroupBy<InventoryItem.inventoryID>>>>
                   .Replace<LineType>(lineType).Replace<WhereType>(whereType).ToType(),
                   typeof(InventoryItem.inventoryCD),
                   typeof(InventoryItem.descr),
                   headers ?? defaultHeaders)
        {
        }
    }
    #endregion

    #region EquipmentModelItemAttribute
    [PXDBInt]
    [PXUIField(DisplayName = "Model Equipment", Visibility = PXUIVisibility.Visible)]
    public class EquipmentModelItemAttribute : FSInventoryAttribute
    {
        private static Type[] defaultHeaders = new Type[]
        {
            typeof(InventoryItem.inventoryCD), 
            typeof(InventoryItem.descr),
            typeof(InventoryItem.itemClassID),
            typeof(InventoryItem.itemType),
            typeof(InventoryItem.baseUnit), 
            typeof(InventoryItem.salesUnit),
            typeof(InventoryItem.basePrice)
        };

        public EquipmentModelItemAttribute(Type[] headers = null)
            : this(typeof(Where<True, Equal<True>>), headers)
        {
        }

        public EquipmentModelItemAttribute(Type whereType, Type[] headers = null)
            : base(
                    BqlCommand.Compose(
                            typeof(Search<,>),
                            typeof(InventoryItem.inventoryID),
                            typeof(Where<FSxEquipmentModel.eQEnabled, Equal<True>>)),
                    typeof(InventoryItem.inventoryCD),
                    typeof(InventoryItem.descr),
                    headers ?? defaultHeaders)
        {
        }
    }
    #endregion

    #endregion

    #region Lookups Group - Project and Task

    #region Project
    public class FSSelectorProjectAttribute : PXSelectorAttribute
    {
        public FSSelectorProjectAttribute()
            : base(typeof(Search<Contract.contractID,
                          Where<
                              Contract.baseType, Equal<CTPRType.project>,
                          And<
                              Contract.nonProject, Equal<False>>>>))
        {
            SubstituteKey = typeof(Contract.contractCD);
            DescriptionField = typeof(Contract.description);
        }
    }
    #endregion

    #region Task
    public class FSSelectorActive_AR_SO_ProjectTaskAttribute : PXSelectorAttribute
    {
        public FSSelectorActive_AR_SO_ProjectTaskAttribute(Type whereType)
            : base(
                    BqlCommand.Compose(
                            typeof(Search2<,,>),
                            typeof(PMTask.taskID),
                            typeof(InnerJoin<,>),
                            typeof(FSSrvOrdType),
                            typeof(On<FSSrvOrdType.srvOrdType, Equal<Current<FSSrvOrdType.srvOrdType>>>),
                            typeof(Where2<,>),
                            whereType,
                            typeof(And<
                                    PMTask.isCancelled, Equal<False>,
                                    And<PMTask.isCompleted, Equal<False>,
                                    And2<
                                       Where<FSSrvOrdType.enableINPosting, Equal<False>, Or<PMTask.visibleInIN, Equal<True>>>,
                                    And<
                                        Where2<
                                            Where<
                                                FSSrvOrdType.postTo, Equal<FSPostTo.None>, 
                                                Or<FSSrvOrdType.postTo, Equal<FSPostTo.Projects>>>,
                                            Or<
                                                Where2<
                                                    Where<
                                                        FSSrvOrdType.postTo, Equal<FSPostTo.Accounts_Receivable_Module>,
                                                        And<
                                                            Where<
                                                                PMTask.visibleInAR, Equal<True>>>>,
                                                Or<
                                                    Where2<
                                                        Where<
                                                            FSSrvOrdType.postTo, Equal<FSPostTo.Sales_Order_Module>,
                                                                Or<FSSrvOrdType.postTo, Equal<FSPostTo.Sales_Order_Invoice>>>,
                                                        And<
                                                            Where<
                                                                PMTask.visibleInSO, Equal<True>>>>>>>>>>>>)),
                typeof(PMTask.taskCD),
                typeof(PMTask.description))
                {
                    SubstituteKey = typeof(PMTask.taskCD);
                    DescriptionField = typeof(PMTask.description);
                    DirtyRead = true;
                }
    }
    #endregion

    #endregion

    #region Staff Member

    public class FSSelector_StaffMember_ServiceOrderProjectIDAttribute : PXDimensionSelectorAttribute
    {
        public FSSelector_StaffMember_ServiceOrderProjectIDAttribute()
            : base(
                BAccountAttribute.DimensionName,
                typeof(
                    Search2<BAccountStaffMember.bAccountID,
                       LeftJoin<Vendor,
                       On<
                           Vendor.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                        And<Vendor.status, NotEqual<BAccount.status.inactive>>>,
                       LeftJoin<EPEmployee,
                       On<
                           EPEmployee.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                        And<EPEmployee.status, NotEqual<BAccount.status.inactive>>>,
                       LeftJoin<PMProject,
                       On<
                           PMProject.contractID, Equal<Current<FSServiceOrder.projectID>>>,
                       LeftJoin<EPEmployeeContract,
                       On<
                           EPEmployeeContract.contractID, Equal<PMProject.contractID>,
                           And<EPEmployeeContract.employeeID, Equal<BAccountStaffMember.bAccountID>>>,
                       LeftJoin<EPEmployeePosition,
                       On<
                           EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>,
                           And<EPEmployeePosition.isActive, Equal<True>>>>>>>>,
                       Where<
                           PMProject.isActive, Equal<True>,
                       And<
                           PMProject.baseType, Equal<CT.CTPRType.project>,
                       And<
                           Where2<
                               Where<
                                   FSxVendor.sDEnabled, Equal<True>>,
                               Or<
                                   Where<
                                       FSxEPEmployee.sDEnabled, Equal<True>,
                                   And<
                                       Where<
                                           PMProject.restrictToEmployeeList, Equal<False>,
                                       Or<
                                           EPEmployeeContract.employeeID, IsNotNull>>>>>>>>>,
                       OrderBy<
                           Asc<BAccountStaffMember.acctCD>>>),
                typeof(BAccountStaffMember.acctCD),
                new Type[]
                {
                    typeof(BAccountStaffMember.acctCD),
                    typeof(BAccountStaffMember.acctName),
                    typeof(BAccountStaffMember.type),
                    typeof(BAccountStaffMember.status),
                    typeof(EPEmployeePosition.positionID)
                })
        {
            DescriptionField = typeof(BAccountStaffMember.acctName);
        }
    }

    public class FSSelector_StaffMember_AllAttribute : PXDimensionSelectorAttribute
    {
        // The SubstituteKey parameter as AcctName is used to correct an issue in Employee-Room screen (SD-5617)
        public FSSelector_StaffMember_AllAttribute(Type parmSubstituteKey = null)
            : base(
                BAccountAttribute.DimensionName,
                typeof(
                    Search2<BAccountStaffMember.bAccountID,
                       LeftJoin<Vendor,
                       On<
                           Vendor.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                        And<Vendor.status, NotEqual<BAccount.status.inactive>>>,
                       LeftJoin<EPEmployee,
                       On<
                           EPEmployee.bAccountID, Equal<BAccountStaffMember.bAccountID>,
                        And<EPEmployee.status, NotEqual<BAccount.status.inactive>>>,
                       LeftJoin<EPEmployeePosition,
                       On<
                           EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>,
                           And<EPEmployeePosition.isActive, Equal<True>>>>>>,
                       Where2<
                           Where<
                               FSxVendor.sDEnabled, Equal<True>,
                           And<
                               Where<
                                   Vendor.status, Equal<BAccountStaffMember.status.active>,
                               Or<
                                   Vendor.status, Equal<BAccountStaffMember.status.oneTime>>>>>,
                       Or<
                           Where<
                               FSxEPEmployee.sDEnabled, Equal<True>>>>,
                       OrderBy<
                           Asc<BAccountStaffMember.acctCD>>>),
                parmSubstituteKey ?? typeof(BAccountStaffMember.acctCD),
                new Type[]
                {
                    typeof(BAccountStaffMember.acctCD),
                    typeof(BAccountStaffMember.acctName),
                    typeof(BAccountStaffMember.type),
                    typeof(BAccountStaffMember.status),
                    typeof(EPEmployeePosition.positionID)
                })
        {
            DescriptionField = typeof(BAccountStaffMember.acctName);
        }
    }

    public class FSSelector_StaffMember_Employee_OnlyAttribute : PXDimensionSelectorAttribute
    {
        public FSSelector_StaffMember_Employee_OnlyAttribute()
            : base(
                BAccountAttribute.DimensionName,
                typeof(
                    Search2<BAccountStaffMember.bAccountID,
                       LeftJoin<EPEmployee,
                       On<
                           EPEmployee.bAccountID, Equal<BAccountStaffMember.bAccountID>>,
                       LeftJoin<EPEmployeePosition,
                       On<
                           EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>,
                           And<EPEmployeePosition.isActive, Equal<True>>>>>,
                       Where<
                           FSxEPEmployee.sDEnabled, Equal<True>>,
                       OrderBy<
                           Asc<BAccountStaffMember.acctCD>>>),
                typeof(BAccountStaffMember.acctCD),
                new Type[]
                {
                    typeof(BAccountStaffMember.acctCD),
                    typeof(BAccountStaffMember.acctName),
                    typeof(BAccountStaffMember.type),
                    typeof(BAccountStaffMember.status),
                    typeof(EPEmployeePosition.positionID)
                })
        {
            DescriptionField = typeof(BAccountStaffMember.acctName);
        }
    }

    // Route Drivers
    public class FSSelector_Driver_AllAttribute : PXDimensionSelectorAttribute
    {
        public FSSelector_Driver_AllAttribute()
            : base(
                BAccountAttribute.DimensionName,
                typeof(
                    Search<EPEmployee.bAccountID,
                       Where<
                           FSxEPEmployee.sDEnabled, Equal<True>,
                       And<
                           EPEmployee.status, NotEqual<BAccount.status.inactive>,
                       And<
                           FSxEPEmployee.isDriver, Equal<True>>>>,
                       OrderBy<
                           Asc<EPEmployee.acctCD>>>),
                typeof(EPEmployee.acctCD),
                new Type[]
                {
                    typeof(EPEmployee.acctCD), 
                    typeof(EPEmployee.acctName), 
                    typeof(EPEmployee.status),
                    typeof(EPEmployee.departmentID)
                })
        {
            DescriptionField = typeof(EPEmployee.acctName);
        }
    }

    public class FSSelector_Driver_RouteDocumentRouteIDAttribute : PXSelectorWithCustomOrderByAttribute
    {
        public FSSelector_Driver_RouteDocumentRouteIDAttribute()
            : base(
                typeof(Search<EPEmployeeFSRouteEmployee.bAccountID,
                       Where<
                           EPEmployeeFSRouteEmployee.routeID, Equal<Current<FSRouteDocument.routeID>>>,
                       OrderBy<
                           Asc<EPEmployeeFSRouteEmployee.priorityPreference>>>),
                new Type[]
                {
                    typeof(EPEmployeeFSRouteEmployee.acctCD), 
                    typeof(EPEmployeeFSRouteEmployee.acctName), 
                    typeof(EPEmployeeFSRouteEmployee.priorityPreference),
                    typeof(EPEmployeeFSRouteEmployee.status),
                    typeof(EPEmployeeFSRouteEmployee.departmentID)
                })
        {
            SubstituteKey = typeof(EPEmployeeFSRouteEmployee.acctCD);
            DescriptionField = typeof(EPEmployeeFSRouteEmployee.acctName);
        }
    }

    //All Employees
    public class FSSelector_Employee_AllAttribute : PXDimensionSelectorAttribute
    {
        public FSSelector_Employee_AllAttribute()
            : base(
                BAccountAttribute.DimensionName,
                typeof(
                    Search<EPEmployee.bAccountID,
                       Where<
                           FSxEPEmployee.sDEnabled, Equal<True>,
                       And<
                           EPEmployee.status, NotEqual<BAccount.status.inactive>>>,
                       OrderBy<
                           Asc<EPEmployee.acctCD>>>),
                typeof(EPEmployee.acctCD),
                new Type[]
                {
                    typeof(EPEmployee.acctCD), 
                    typeof(EPEmployee.acctName), 
                    typeof(EPEmployee.status),
                    typeof(EPEmployee.departmentID)
                })
        {
            DescriptionField = typeof(EPEmployee.acctName);
        }
    }

    #endregion

    #region Lookups Group - ServiceOrder and ServiceOrderDetail

    #region SORefNbr
    public class FSSelectorSORefNbrAttribute : PXSelectorAttribute
    {
        public FSSelectorSORefNbrAttribute()
            : base(
                typeof(Search2<FSServiceOrder.refNbr,
                       LeftJoin<BAccountSelectorBase,
                            On<BAccountSelectorBase.bAccountID, Equal<FSServiceOrder.customerID>>,
                       LeftJoin<Location, 
                            On<Location.bAccountID, Equal<FSServiceOrder.customerID>,
                                And<Location.locationID, Equal<FSServiceOrder.locationID>>>,
                       LeftJoin<Customer,
                           On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>>>,
                       Where2<
                       Where<
                            FSServiceOrder.srvOrdType, Equal<Optional<FSServiceOrder.srvOrdType>>>,
                           And<Where<
                                Customer.bAccountID, IsNull,
                                Or<Match<Customer, Current<AccessInfo.userName>>>>>>,
                       OrderBy<
                            Desc<FSServiceOrder.refNbr>>>),
                new Type[]
                {
                    typeof(FSServiceOrder.refNbr), 
                    typeof(FSServiceOrder.srvOrdType),
                    typeof(BAccountSelectorBase.type),
                    typeof(BAccountSelectorBase.acctCD),
                    typeof(BAccountSelectorBase.acctName),
                    typeof(Location.locationCD),
                    typeof(FSServiceOrder.status),
                    typeof(FSServiceOrder.priority), 
                    typeof(FSServiceOrder.severity),
                    typeof(FSServiceOrder.orderDate), 
                    typeof(FSServiceOrder.sLAETA),
                    typeof(FSServiceOrder.assignedEmpID),
                    typeof(FSServiceOrder.sourceType), 
                    typeof(FSServiceOrder.sourceRefNbr)
                })
        {
            Filterable = true;
        }
    }
    #endregion

    #region SORefNbr_Appointment
    public class FSSelectorSORefNbr_AppointmentAttribute : PXSelectorAttribute
    {
        public FSSelectorSORefNbr_AppointmentAttribute()
            : base(
                typeof(Search2<FSServiceOrder.refNbr,
                LeftJoin<Customer,
                    On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>,
                Where<
                    FSServiceOrder.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>, 
                    And<
                        Where2<
                            Where<Current<FSAppointment.appointmentID>, Greater<Zero>,
                                Or<FSServiceOrder.status, Equal<FSServiceOrder.status.Open>>>,
                            And<Where<Customer.bAccountID, IsNull, 
                            Or<
                                    Where<Customer.bAccountID, IsNotNull,
                                        And<Match<Customer, Current<AccessInfo.userName>>>>>>>>>>,
                OrderBy<
                    Desc<FSServiceOrder.refNbr>>>),
                new Type[]
                {
                    typeof(FSServiceOrder.refNbr), 
                    typeof(FSServiceOrder.srvOrdType),
                    typeof(FSServiceOrder.customerID), 
                    typeof(FSServiceOrder.status),
                    typeof(FSServiceOrder.priority), 
                    typeof(FSServiceOrder.severity),
                    typeof(FSServiceOrder.orderDate), 
                    typeof(FSServiceOrder.sLAETA),
                    typeof(FSServiceOrder.assignedEmpID),
                    typeof(FSServiceOrder.sourceType), 
                    typeof(FSServiceOrder.sourceRefNbr)
                })
        {
            Filterable = true;
        }
    }
    #endregion

    #region SODetIDService
    public class FSSelectorSODetIDServiceAttribute : PXSelectorAttribute
    {
        public FSSelectorSODetIDServiceAttribute()
            : base(
                typeof(Search<FSSODet.sODetID,
                       Where<
                           FSSODet.sOID, Equal<Current<FSAppointment.sOID>>,
                           And<FSSODet.status, NotEqual<ListField_Status_SODet.Canceled>,
                           And<FSSODet.status, NotEqual<ListField_Status_SODet.Scheduled>,
                           And<FSSODet.status, NotEqual<ListField_Status_SODet.Completed>>>>>>),
                new Type[]
                {
                    typeof(FSSODet.lineRef), 
                    typeof(FSSODet.lineType),
                    typeof(FSSODet.status), 
                    typeof(FSSODet.inventoryID), 
                    typeof(FSSODet.lastModifiedDateTime)
                })
        {
            SubstituteKey = typeof(FSSODet.lineRef);
        }
    }
    #endregion

    #region SODetID
    public class FSSelectorSODetIDAttribute : PXSelectorAttribute
    {
        public FSSelectorSODetIDAttribute()
            : base(
                typeof(Search<FSSODet.sODetID,
                       Where<
                           FSSODet.sOID, Equal<Current<FSAppointment.sOID>>,
                       And<
                           FSSODet.status, NotEqual<FSSODet.status.Scheduled>,
                       And<
                           FSSODet.status, NotEqual<FSSODet.status.Canceled>,
                       And<
                           FSSODet.status, NotEqual<FSSODet.status.Completed>>>>>>),
                new Type[]
                {
                    typeof(FSSODet.lineRef),
                    typeof(FSSODet.lineType),
                    typeof(FSSODet.status),
                    typeof(FSSODet.inventoryID),
                    typeof(FSSODet.lastModifiedDateTime)
                })
        {
            SubstituteKey = typeof(FSSODet.lineRef);
        }
    }
    #endregion

    #region SODetIDService
    public class FSSelectorServiceOrderSODetIDAttribute : PXSelectorAttribute
    {
        public FSSelectorServiceOrderSODetIDAttribute()
            : base(
                typeof(Search<FSSODet.lineRef,
                       Where<
                           FSSODet.sOID, Equal<Current<FSServiceOrder.sOID>>,
                       And<
                           FSSODet.lineType, Equal<FSLineType.Service>>>>),
                new Type[]
                {
                    typeof(FSSODet.lineRef),
                    typeof(FSSODet.lineType),
                    typeof(FSSODet.status),
                    typeof(FSSODet.inventoryID),
                    typeof(FSSODet.tranDesc)
                })
        {
            SubstituteKey = typeof(FSSODet.lineRef);
            DescriptionField = typeof(FSSODet.inventoryID);
            DirtyRead = true;
        }
    }

    public class FSSelectorAppointmentSODetIDAttribute : PXSelectorAttribute
    {
        public FSSelectorAppointmentSODetIDAttribute()
            : base(
                typeof(Search<FSAppointmentDet.lineRef,
                       Where<
                           FSAppointmentDet.appointmentID, Equal<Current<FSAppointment.appointmentID>>,
                       And<
                           FSAppointmentDet.lineType, Equal<FSLineType.Service>,
                       And<
                           FSAppointmentDet.lineRef, IsNotNull>>>>),
                new Type[]
                {
                    typeof(FSAppointmentDet.lineRef),
                    typeof(FSAppointmentDet.lineType),
                    typeof(FSAppointmentDet.status),
                    typeof(FSAppointmentDet.inventoryID),
                    typeof(FSAppointmentDet.tranDesc)
                })
        {
            SubstituteKey = typeof(FSAppointmentDet.lineRef);
            DescriptionField = typeof(FSAppointmentDet.inventoryCD);
            DirtyRead = true;
        }

        public FSSelectorAppointmentSODetIDAttribute(Type whereType)
            : base(
                BqlCommand.Compose(
                                    typeof(Search<,>),
                                    typeof(FSAppointmentDet.lineRef),
                                    typeof(Where2<,>),
                                    typeof(Where<
                                                FSAppointmentDet.appointmentID, Equal<Current<FSAppointment.appointmentID>>,
                                                And<FSAppointmentDet.lineRef, IsNotNull,
                                                And<Where<
                                                    FSAppointmentDet.lineType, Equal<FSLineType.Service>,
                                                    Or<FSAppointmentDet.lineType, Equal<FSLineType.NonStockItem>>>>>>),
                                    typeof(And<>),
                                    whereType),
                new Type[]
                {
                    typeof(FSAppointmentDet.lineRef),
                    typeof(FSAppointmentDet.lineType),
                    typeof(FSAppointmentDet.status),
                    typeof(FSAppointmentDet.inventoryID),
                    typeof(FSAppointmentDet.tranDesc)
                })
        {
            SubstituteKey = typeof(FSAppointmentDet.lineRef);
            DescriptionField = typeof(FSAppointmentDet.inventoryCD);
            DirtyRead = true;
        }
    }
    #endregion

    #region Appointment Line Ref of services in the Appointment for Pickup/Delivery
    public class FSSelectorServiceInAppointmentAttribute : PXSelectorAttribute
    {
        public FSSelectorServiceInAppointmentAttribute()
            : base(
                typeof(Search<FSAppointmentDet.lineRef,
                       Where<
                           FSAppointmentDet.appointmentID, Equal<Current<FSAppointment.appointmentID>>,
                       And<
                           FSAppointmentDet.lineType, Equal<FSLineType.Service>,
                       And<
                           Where<
                               FSAppointmentDet.serviceType, Equal<FSAppointmentDet.serviceType.Delivered_Items>,
                           Or<
                               FSAppointmentDet.serviceType, Equal<FSAppointmentDet.serviceType.Picked_Up_Items>>>>>>>),
                new Type[]
                {
                    typeof(FSAppointmentDet.lineRef),
                    typeof(FSAppointmentDet.lineType),
                    typeof(FSAppointmentDet.status),
                    typeof(FSAppointmentDet.inventoryID),
                    typeof(FSAppointmentDet.tranDesc)
                })
        {
            SubstituteKey = typeof(FSAppointmentDet.lineRef);
            DescriptionField = typeof(FSAppointmentDet.inventoryID);
            DirtyRead = true;
        }
    }
    #endregion

    #region Equipment Type
    public class FSSelectorEquipmentTypeAttribute : PXSelectorAttribute
    {
        public FSSelectorEquipmentTypeAttribute()
            : base(typeof(Search<FSEquipmentType.equipmentTypeID>))
        {
            SubstituteKey = typeof(FSEquipmentType.equipmentTypeCD);
            DescriptionField = typeof(FSEquipmentType.descr);
        }
    }
    #endregion

    #region CompanyLocation
    public class FSSelectorBranchLocationAttribute : PXSelectorAttribute
    {
        public FSSelectorBranchLocationAttribute()
            : base(
                typeof(Search<FSBranchLocation.branchLocationID,
                       Where<
                           FSBranchLocation.branchID, Equal<Current<AccessInfo.branchID>>>>))
        {
            SubstituteKey = typeof(FSBranchLocation.branchLocationCD);
            DescriptionField = typeof(FSBranchLocation.descr);
        }
    }
    #endregion

    public class FSSelectorBranchLocationByFSScheduleAttribute : PXSelectorAttribute
    {
        public FSSelectorBranchLocationByFSScheduleAttribute()
            : base(
                typeof(Search<FSBranchLocation.branchLocationID,
                       Where<
                           FSBranchLocation.branchID, Equal<Current<FSSchedule.branchID>>>>))
        {
            SubstituteKey = typeof(FSBranchLocation.branchLocationCD);
            DescriptionField = typeof(FSBranchLocation.descr);
        }
    }
    #endregion

    #region Lookups Group - Appointment

    internal class FSSelectorAppointmentPostingINAttribute : PXSelectorAttribute
    {
        public FSSelectorAppointmentPostingINAttribute()
            : base(
                typeof(Search5<FSAppointment.appointmentID,
                       InnerJoin<FSSrvOrdType,
                       On<
                           FSSrvOrdType.srvOrdType, Equal<FSAppointment.srvOrdType>>,
                       InnerJoin<FSAppointmentDet,
                       On<
                           FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>>>>,
                       Where<
                            FSAppointmentDet.lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>,
                       And<
                           FSAppointment.status, Equal<ListField_Status_Appointment.Closed>,
                       And<
                           FSAppointment.executionDate, LessEqual<Current<UpdateInventoryFilter.cutOffDate>>,
                       And<
                           FSSrvOrdType.enableINPosting, Equal<True>,
                       And<
                           Where<
                               FSAppointment.routeDocumentID, Equal<Current<UpdateInventoryFilter.routeDocumentID>>,
                           Or<
                               Current<UpdateInventoryFilter.routeDocumentID>, IsNull>>>>>>>,
                       Aggregate<
                           GroupBy<FSAppointment.appointmentID>>>))
        {
            SubstituteKey = typeof(FSAppointment.refNbr);
        }
    }
    #endregion

    #region Lookups Group - Workflow and WorkflowStage

    #region Workflow
    public class FSSelectorWorkflowAttribute : PXSelectorAttribute
    {
        public FSSelectorWorkflowAttribute()
            : base(typeof(Search<FSSrvOrdType.srvOrdTypeID,
                          Where<
                              FSSrvOrdType.active, Equal<True>>>))
        {
            SubstituteKey = typeof(FSSrvOrdType.srvOrdType);
            DescriptionField = typeof(FSSrvOrdType.descr);
        }
    }
    #endregion

    #region WorkflowStage
    public class FSSelectorWorkflowStageAttribute : PXSelectorAttribute
    {
        public FSSelectorWorkflowStageAttribute(Type currentSrvOrdType)
            : base(
                  BqlCommand.Compose(
                      typeof(Search2<,,,>),
                      typeof(FSWFStage.wFStageID),
                      typeof(InnerJoin<FSSrvOrdType,
                             On<FSSrvOrdType.srvOrdTypeID, Equal<FSWFStage.wFID>>>),
                      typeof(Where<,>),
                      typeof(FSSrvOrdType.srvOrdType),
                      typeof(Equal<>),
                      typeof(Current<>),
                      currentSrvOrdType,
                      typeof(OrderBy<
                          Asc<FSWFStage.parentWFStageID,
                          Asc<FSWFStage.sortOrder>>>)))
        {
            SubstituteKey = typeof(FSWFStage.wFStageCD);
            DescriptionField = typeof(FSWFStage.descr);
        }
    }
    #endregion

    #region WorkflowStage In Reason
    public class FSSelectorWorkflowStageInReasonAttribute : PXSelectorAttribute
    {
        public FSSelectorWorkflowStageInReasonAttribute()
            : base(
                typeof(Search<FSWFStage.wFStageID,
                       Where<
                           FSWFStage.wFID, Equal<Current<FSReasonFilter.wFID>>>,
                       OrderBy<
                           Asc<FSWFStage.parentWFStageID,
                           Asc<FSWFStage.sortOrder>>>>))
        {
            SubstituteKey = typeof(FSWFStage.wFStageCD);
        }
    }
    #endregion

    #endregion

    #region Lookups Group - SrvOrdType

    public class FSSelectorSrvOrdTypeAttribute : PXSelectorAttribute
    {
        public FSSelectorSrvOrdTypeAttribute()
            : base(
                typeof(Search<FSSrvOrdType.srvOrdType>),
                new Type[]
                {
                    typeof(FSSrvOrdType.srvOrdType), 
                    typeof(FSSrvOrdType.descr), 
                    typeof(FSSrvOrdType.behavior) 
                })
        {
            DescriptionField = typeof(FSSrvOrdType.descr);
        }
    }

    public class FSSelectorContractSrvOrdTypeAttribute : PXSelectorAttribute
    {
        public FSSelectorContractSrvOrdTypeAttribute()
            : base(
                typeof(Search<FSSrvOrdType.srvOrdType,
                       Where<
                           FSSrvOrdType.active, Equal<True>,
                       And<
                           FSSrvOrdType.behavior, NotEqual<FSSrvOrdType.behavior.Quote>,
                       And<
                           FSSrvOrdType.behavior, NotEqual<FSSrvOrdType.behavior.RouteAppointment>,
                       And<
                           FSSrvOrdType.behavior, NotEqual<FSSrvOrdType.behavior.InternalAppointment>>>>>>))
        {
            DescriptionField = typeof(FSSrvOrdType.descr);
        }
    }

    public class FSSelectorRouteContractSrvOrdTypeAttribute : PXSelectorAttribute
    {
        public FSSelectorRouteContractSrvOrdTypeAttribute()
            : base(
                typeof(Search<FSSrvOrdType.srvOrdType,
                       Where<
                           FSSrvOrdType.active, Equal<True>,
                       And<
                           FSSrvOrdType.behavior, Equal<FSSrvOrdType.behavior.RouteAppointment>>>>))
        {
            DescriptionField = typeof(FSSrvOrdType.descr);
        }
    }

    public class FSSelectorSrvOrdTypeNOTQuoteAttribute : PXSelectorAttribute
    {
        public FSSelectorSrvOrdTypeNOTQuoteAttribute()
            : base(
                typeof(Search<FSSrvOrdType.srvOrdType,
                       Where<
                           FSSrvOrdType.behavior, NotEqual<FSSrvOrdType.behavior.Quote>>>),
                new Type[]
                {
                    typeof(FSSrvOrdType.srvOrdType), 
                    typeof(FSSrvOrdType.descr), 
                    typeof(FSSrvOrdType.behavior) 
                })
        {
            DescriptionField = typeof(FSSrvOrdType.descr);
        }
    }

    public class FSSelectorSrvOrdTypeNOTQuoteInternalAttribute : PXSelectorAttribute
    {
        public FSSelectorSrvOrdTypeNOTQuoteInternalAttribute()
            : base(
                typeof(Search<FSSrvOrdType.srvOrdType,
                       Where<
                           FSSrvOrdType.active, Equal<True>,
                       And<
                           FSSrvOrdType.behavior, NotEqual<FSSrvOrdType.behavior.Quote>,
                       And<
                           FSSrvOrdType.behavior, NotEqual<FSSrvOrdType.behavior.InternalAppointment>>>>>),
                new Type[]
                {
                    typeof(FSSrvOrdType.srvOrdType),
                    typeof(FSSrvOrdType.descr),
                    typeof(FSSrvOrdType.behavior)
                })
        {
            DescriptionField = typeof(FSSrvOrdType.descr);
        }
    }

    public class FSSelectorSrvOrdTypeRoute : PXSelectorAttribute
    {
        public FSSelectorSrvOrdTypeRoute()
            : base(
                typeof(Search<FSSrvOrdType.srvOrdType,
                       Where<
                           FSSrvOrdType.active, Equal<True>,
                       And<
                           FSSrvOrdType.behavior, Equal<FSSrvOrdType.behavior.RouteAppointment>>>>),
                new Type[]
                {
                    typeof(FSSrvOrdType.srvOrdType), 
                    typeof(FSSrvOrdType.descr), 
                    typeof(FSSrvOrdType.behavior) 
                })
        {
            DescriptionField = typeof(FSSrvOrdType.descr);
        }
    }

    public class FSSelectorActiveSrvOrdType : PXSelectorAttribute
    {
        public FSSelectorActiveSrvOrdType()
            : base(
                typeof(Search<FSSrvOrdType.srvOrdType,
                       Where<FSSrvOrdType.active, Equal<True>>>))
        {
            DescriptionField = typeof(FSSrvOrdType.descr);
        }
    }
    #endregion

    #region Lookups Group - Equipment, Resources and Vehicles

    public static class SelectorBase_Equipment
    {
        public static Type[] selectorColumns = new Type[]
                                                {
                                                    typeof(FSEquipment.refNbr),
                                                    typeof(FSEquipment.descr),
                                                    typeof(FSEquipment.serialNumber),
                                                    typeof(FSEquipment.ownerType),
                                                    typeof(FSEquipment.ownerID),
                                                    typeof(FSEquipment.locationType),
                                                    typeof(FSEquipment.customerID),
                                                    typeof(FSEquipment.customerLocationID),
                                                    typeof(FSEquipment.branchID),
                                                    typeof(FSEquipment.branchLocationID),
                                                    typeof(FSEquipment.inventoryID),
                                                    typeof(FSEquipment.iNSerialNumber),
                                                    typeof(FSEquipment.color),
                                                    typeof(FSEquipment.status)
                                                };
    }

    #region All SMEquipment
    public class FSSelectorSMEquipmentRefNbrAttribute : PXSelectorAttribute
    {
        public FSSelectorSMEquipmentRefNbrAttribute()
            : base(
                typeof(
                        Search2<FSEquipment.refNbr,
                            LeftJoinSingleTable<Customer,
                                On<Customer.bAccountID, Equal<FSEquipment.customerID>>>,
                        Where<
                            Customer.bAccountID, IsNull,
                            Or<Match<Customer, Current<AccessInfo.userName>>>>,
                       OrderBy<
                           Asc<FSEquipment.refNbr>>>),
                SelectorBase_Equipment.selectorColumns)
        {
        }
    }
    #endregion

    #region Equipment for Maintenance
    public class FSSelectorMaintenanceEquipmentAttribute : PXSelectorAttribute
    {
        public FSSelectorMaintenanceEquipmentAttribute(Type srvOrdType, Type billCustomerID, Type customerID, Type customerLocationID, Type branchLocationID)
            : this(srvOrdType, billCustomerID, customerID, customerLocationID, typeof(AccessInfo.branchID), branchLocationID)
        {
        }

        public FSSelectorMaintenanceEquipmentAttribute(Type srvOrdType, Type billCustomerID, Type customerID, Type customerLocationID, Type branchID, Type branchLocationID)
            : base(
                 BqlCommand.Compose(
                            typeof(Search2<,,>),
                            typeof(FSEquipment.SMequipmentID),
                            typeof(InnerJoin<,,>),
                            typeof(FSSrvOrdType),
                                    typeof(On<,>), typeof(FSSrvOrdType.srvOrdType), typeof(Equal<>), typeof(Current<>), srvOrdType,
                            typeof(CrossJoinSingleTable<>),
                            typeof(FSSetup),
                            typeof(Where<,,>),
                                typeof(FSEquipment.requireMaintenance), typeof(Equal<True>),
                            typeof(And<>),
                                typeof(Where2<,>),
                                    typeof(Where<FSSetup.enableAllTargetEquipment, Equal<True>>),
                                    typeof(Or<>),
                                        typeof(Where2<,>),
                                            typeof(Where<,,>),
                                                //LocationType: Customer
                                                typeof(FSEquipment.locationType), typeof(Equal<ListField_LocationType.Customer>),
                                                    typeof(And2<,>),
                                                        typeof(Where<,,>),
                                                            typeof(FSEquipment.customerID), typeof(Equal<>), typeof(Current<>), customerID,
                                                            typeof(And<>),
                                                                typeof(Where<,,>),
                                                                    typeof(FSEquipment.customerLocationID), typeof(Equal<>), typeof(Current<>), customerLocationID,
                                                                    typeof(Or<FSEquipment.customerLocationID, IsNull>),
                                                        typeof(And<>),
                                                            typeof(Where2<,>),
                                                                typeof(Where<,,>),
                                                                    //In case OwnerType: Customer -> Check if billCustomer is either the Equipment's customer or its OwnerID.
                                                                    typeof(FSEquipment.ownerType), typeof(Equal<FSEquipment.ownerType.Customer>),
                                                                    typeof(And<>),
                                                                        typeof(Where2<,>),
                                                                            typeof(Where<,>),
                                                                                typeof(FSEquipment.customerID), typeof(Equal<>), typeof(Current<>), billCustomerID,
                                                                        typeof(Or<>),
                                                                            typeof(Where<,>),
                                                                                typeof(FSEquipment.ownerID), typeof(Equal<>), typeof(Current<>), billCustomerID,
                                                                typeof(Or<
                                                                          Where<FSEquipment.ownerType, Equal<FSEquipment.ownerType.OwnCompany>>>),
                                            typeof(Or<>),
                                                //LocationType: Company
                                                typeof(Where<,,>),
                                                    typeof(FSEquipment.locationType), typeof(Equal<ListField_LocationType.Company>),
                                                    typeof(And<,,>),
                                                        typeof(FSEquipment.branchID), typeof(Equal<>), typeof(Current<>), branchID,
                                                        typeof(And2<,>),
                                                            typeof(Where<,,>),
                                                                typeof(FSEquipment.branchLocationID), typeof(Equal<>), typeof(Current<>), branchLocationID,                                                        
                                                                typeof(Or<FSEquipment.branchLocationID, IsNull>),
                                                        typeof(And<>),
                                                            typeof(Where2<,>),
                                                                typeof(Where<,,>),
                                                                    typeof(FSEquipment.ownerType), typeof(Equal<FSEquipment.ownerType.Customer>),
                                                                    typeof(And<,>),
                                                                        typeof(FSEquipment.ownerID), typeof(Equal<>), typeof(Current<>), billCustomerID,
                                                                typeof(Or<>),
                                                                    typeof(Where<,,>),
                                                                        //All equipments with LocationType: Company and OwnerType: Company only appear in Internal Appointments.
                                                                        typeof(FSEquipment.ownerType), typeof(Equal<FSEquipment.ownerType.OwnCompany>),
                                                                        typeof(And<,>),
                                                                            typeof(FSSrvOrdType.behavior), typeof(Equal<FSSrvOrdType.behavior.InternalAppointment>)
                    ),
        SelectorBase_Equipment.selectorColumns)
        {
            SubstituteKey = typeof(FSEquipment.refNbr);
            DescriptionField = typeof(FSEquipment.descr);
        }
    }

    #endregion

    #region Resource Equipment

    public class FSSelectorServiceOrderResourceEquipmentAttribute : PXSelectorAttribute
    {
        public FSSelectorServiceOrderResourceEquipmentAttribute()
            : base(
                typeof(Search<FSEquipment.SMequipmentID,
                       Where<
                           FSEquipment.resourceEquipment, Equal<True>,
                       And<
                           Where2<
                               Where2<
                                   Where<
                                       FSEquipment.locationType, Equal<ListField_LocationType.Company>,
                                   And<
                                       FSEquipment.branchID, Equal<Current<FSServiceOrder.branchID>>,
                                   And<
                                       FSEquipment.branchLocationID, Equal<Current<FSServiceOrder.branchLocationID>>>>>,
                                   Or<
                                       Where<
                                           FSEquipment.locationType, Equal<ListField_LocationType.Company>,
                                       And<
                                           FSEquipment.branchID, Equal<Current<FSServiceOrder.branchID>>,
                                       And<
                                           FSEquipment.branchLocationID, IsNull>>>>>,
                               Or2<
                                   Where<
                                       FSEquipment.locationType, Equal<ListField_LocationType.Customer>,
                                   And<
                                       FSEquipment.customerID, Equal<Current<FSServiceOrder.customerID>>,
                                   And<
                                       FSEquipment.customerLocationID, Equal<Current<FSServiceOrder.locationID>>>>>,
                                   Or<
                                       Where<
                                           FSEquipment.locationType, Equal<ListField_LocationType.Customer>,
                                       And<
                                           FSEquipment.customerID, Equal<Current<FSServiceOrder.customerID>>,
                                       And<
                                           FSEquipment.customerLocationID, IsNull>>>>>>>>>),
                    SelectorBase_Equipment.selectorColumns)
        {
            SubstituteKey = typeof(FSEquipment.refNbr);
            DescriptionField = typeof(FSEquipment.descr);
        }
    }

    #endregion

    #region Vehicles
    public class FSSelectorVehicleAttribute : PXSelectorAttribute
    {
        public FSSelectorVehicleAttribute()
            : base(
                typeof(Search<FSVehicle.SMequipmentID,
                       Where<
                           FSVehicle.isVehicle, Equal<True>>,
                       OrderBy<
                           Asc<FSVehicle.refNbr>>>),
                new Type[]
                {
                    typeof(FSVehicle.refNbr), 
                    typeof(FSVehicle.status),
                    typeof(FSVehicle.vehicleTypeID),
                    typeof(FSVehicle.descr),
                    typeof(FSVehicle.registrationNbr),
                    typeof(FSVehicle.manufacturerModelID),
                    typeof(FSVehicle.manufacturerID),
                    typeof(FSVehicle.manufacturingYear),
                    typeof(FSVehicle.color)
                })
        {
            SubstituteKey = typeof(FSVehicle.refNbr);
            DescriptionField = typeof(FSVehicle.descr);
        }
    }
    #endregion

    #endregion

    #region Lookups Equipment Management
    #region ModelEquipment - FSxEquipmentModel
    public class FSSelectorComponentIDAttribute : PXSelectorAttribute
    {
        public FSSelectorComponentIDAttribute()
            : base(
                typeof(Search<FSModelTemplateComponent.componentID,
                       Where<
                            FSModelTemplateComponent.modelTemplateID, Equal<Current<InventoryItem.itemClassID>>,
                       And<
                            FSModelTemplateComponent.active, Equal<True>>>>),
                new Type[]
                {
                    typeof(FSModelTemplateComponent.componentCD),
                    typeof(FSModelTemplateComponent.descr),
                    typeof(FSModelTemplateComponent.optional),
                    typeof(FSModelTemplateComponent.classID)
                })
        {
            this.SubstituteKey = typeof(FSModelTemplateComponent.componentCD);
        }
    }
    #endregion
    #region FSEquipment
    public class FSSelectorComponentIDEquipmentAttribute : PXSelectorAttribute
    {
        public FSSelectorComponentIDEquipmentAttribute()
            : base(
                typeof(Search2<FSModelTemplateComponent.componentID,
                       InnerJoin<FSModelComponent,
                       On<
                           FSModelComponent.componentID, Equal<FSModelTemplateComponent.componentID>>>,
                       Where<
                           FSModelComponent.modelID, Equal<Current<FSEquipment.inventoryID>>>>),
                new Type[]
                {
                    typeof(FSModelTemplateComponent.componentCD),
                    typeof(FSModelTemplateComponent.optional),
                    typeof(FSModelComponent.active),
                    typeof(FSModelComponent.descr),
                    typeof(FSModelComponent.classID)
                })
        {
            SubstituteKey = typeof(FSModelTemplateComponent.componentCD);
        }
    }
    #endregion
    #region SalesOrder
    public class FSSelectorComponentIDSalesOrderAttribute : PXCustomSelectorAttribute
    {
        public FSSelectorComponentIDSalesOrderAttribute()
            : base(typeof(FSModelTemplateComponent.componentID),
                   typeof(FSModelTemplateComponent.componentCD),
                   typeof(FSModelComponent.optional),
                   typeof(FSModelComponent.classID))
        {
            this.SubstituteKey = typeof(FSModelTemplateComponent.componentCD);
        }

        protected virtual IEnumerable GetRecords()
        {
            PXCache cache = this._Graph.Caches[typeof(SOLine)];
            int? pivotInventoryID = null;
            int? pivotItemClassID = null;

            object current = null;

            foreach (object item in PXView.Currents)
            {
                if (item != null && (item.GetType() == typeof(SOLine)))
                {
                    current = item;
                    break;
                }
            }

            if (current == null)
            {
                current = cache.Current;
            }

            SOLine currentSOLine = (SOLine)current;
            FSxSOLine currentFSxSOLineRow = PXCache<SOLine>.GetExtension<FSxSOLine>(currentSOLine);
            InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(_Graph, currentSOLine.InventoryID);

            if (currentFSxSOLineRow.NewTargetEquipmentLineNbr != null)
            {
                foreach (object item in cache.Cached)
                {
                    SOLine currentSOLine_temp = item as SOLine;

                    if (currentSOLine_temp.LineNbr == currentFSxSOLineRow.NewTargetEquipmentLineNbr)
                    {
                        pivotInventoryID = currentSOLine_temp.InventoryID;
                        break;
                    }
                }
            }
            else if (currentFSxSOLineRow.SMEquipmentID != null)
            {
                FSEquipment fsEquipmentRow = SharedFunctions.GetEquipmentRow(_Graph, currentFSxSOLineRow.SMEquipmentID);
                pivotInventoryID = fsEquipmentRow.InventoryID;
            }

            if (pivotInventoryID != null)
            {
                pivotItemClassID = inventoryItemRow.ItemClassID;
                PXResultset<FSModelTemplateComponent> resultRows;

                if (currentFSxSOLineRow.EquipmentAction != ID.Equipment_Action.NONE)
                {
                    resultRows = PXSelectJoin<FSModelTemplateComponent,
                                 InnerJoin<FSModelComponent,
                                 On<
                                     FSModelTemplateComponent.componentID, Equal<FSModelComponent.componentID>>>,
                                 Where<
                                     FSModelComponent.active, Equal<True>,
                                 And<
                                     FSModelComponent.modelID, Equal<Required<FSModelComponent.modelID>>,
                                 And<
                                     FSModelComponent.classID, Equal<Required<FSModelComponent.classID>>>>>>
                                 .Select(_Graph, pivotInventoryID, pivotItemClassID);
                }
                else
                {
                    resultRows = PXSelectJoin<FSModelTemplateComponent,
                                 InnerJoin<FSModelComponent,
                                 On<
                                     FSModelTemplateComponent.componentID, Equal<FSModelComponent.componentID>>>,
                                 Where<
                                     FSModelComponent.active, Equal<True>,
                                 And<
                                     FSModelComponent.modelID, Equal<Required<FSModelComponent.modelID>>>>>
                                 .Select(_Graph, pivotInventoryID);
                }

                return resultRows;
            }

            return null;
        }
    }
    public class FSSelectorNewTargetEquipmentSalesOrderAttribute : PXCustomSelectorAttribute
    {
        [PXHidden]
        public partial class FSSOLine : IBqlTable
        {
            #region LineNbr
            public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
            [PXInt]
            [PXUIField(DisplayName = "Line Nbr.")]
            public int? LineNbr { get; set; }
            #endregion
            #region SortOrder
            public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
            [PXInt]
            [PXUIField(DisplayName = "Line Order.", Visibility = PXUIVisibility.SelectorVisible)]
            public int? SortOrder { get; set; }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlString.Field<inventoryID> { }
            [PXString()]
            [PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
            public virtual String InventoryID { get; set; }
            #endregion
        }

        public FSSelectorNewTargetEquipmentSalesOrderAttribute()
            : base(typeof(FSSOLine.lineNbr), typeof(FSSOLine.sortOrder), typeof(FSSOLine.inventoryID))
        {
            this.SubstituteKey = typeof(FSSOLine.sortOrder);
        }

        protected virtual IEnumerable GetRecords()
        {
            var rows = PXSelectJoin<SOLine,
                       InnerJoin<InventoryItem,
                       On<
                           InventoryItem.inventoryID, Equal<SOLine.inventoryID>>>,
                       Where<
                           FSxEquipmentModel.equipmentItemClass, Equal<ListField_EquipmentItemClass.ModelEquipment>,
                       And<
                           FSxSOLine.equipmentAction, Equal<FSxSOLine.equipmentAction.SellingTargetEquipment>,
                       And<
                           Where<
                               SOLine.orderNbr, Equal<Current<SOLine.orderNbr>>,
                           And<
                               SOLine.orderType, Equal<Current<SOLine.orderType>>>>>>>>
                       .Select(_Graph);

            if (rows.Count > 0)
            {
                foreach (PXResult<SOLine, InventoryItem> row in rows)
                {
                    SOLine sOLineRow = (SOLine)row;
                    InventoryItem inventoryItemRow = (InventoryItem)row;
                    yield return new FSSOLine { LineNbr = sOLineRow.LineNbr, SortOrder = sOLineRow.SortOrder, InventoryID = inventoryItemRow.InventoryCD };
                }
            }
        }
    }

    public class FSSelectorNewTargetEquipmentSOInvoiceAttribute : PXCustomSelectorAttribute
    {
        [PXHidden]
        public partial class FSARTran : IBqlTable
        {
            #region LineNbr
            public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
            [PXInt]
            [PXUIField(DisplayName = "Line Nbr.")]
            public int? LineNbr { get; set; }
            #endregion
            #region SortOrder
            public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
            [PXInt]
            [PXUIField(DisplayName = "Line Order.", Visibility = PXUIVisibility.SelectorVisible)]
            public int? SortOrder { get; set; }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlString.Field<inventoryID> { }
            [PXString()]
            [PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
            public virtual String InventoryID { get; set; }
            #endregion
        }

        public FSSelectorNewTargetEquipmentSOInvoiceAttribute()
            : base(typeof(ARTran.lineNbr), typeof(ARTran.sortOrder), typeof(ARTran.inventoryID))
        {
            this.SubstituteKey = typeof(ARTran.sortOrder);
        }

        protected virtual IEnumerable GetRecords()
        {
            var rows = PXSelectJoin<ARTran,
                       InnerJoin<InventoryItem,
                       On<
                           InventoryItem.inventoryID, Equal<ARTran.inventoryID>>>,
                       Where<
                           FSxEquipmentModel.equipmentItemClass, Equal<ListField_EquipmentItemClass.ModelEquipment>,
                       And<
                           FSxARTran.equipmentAction, Equal<FSxARTran.equipmentAction.SellingTargetEquipment>,
                       And<
                           Where<
                               ARTran.refNbr, Equal<Current<ARTran.refNbr>>,
                           And<
                               ARTran.tranType, Equal<Current<ARTran.tranType>>>>>>>>
                       .Select(_Graph);

            if (rows.Count > 0)
            {
                foreach (PXResult<ARTran, InventoryItem> row in rows)
                {
                    ARTran ARTranRow = (ARTran)row;
                    InventoryItem inventoryItemRow = (InventoryItem)row;
                    yield return new FSARTran { LineNbr = ARTranRow.LineNbr, SortOrder = ARTranRow.SortOrder, InventoryID = inventoryItemRow.InventoryCD };
                }
            }
        }
    }
    public class FSSelectorEquipmentLineRefSalesOrderAttribute : PXSelectorAttribute
    {
        public FSSelectorEquipmentLineRefSalesOrderAttribute()
            : base(
                typeof(Search2<FSEquipmentComponent.lineNbr,
                       InnerJoin<InventoryItem,
                       On<
                           InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>,
                       Where2<
                           Where<
                               FSEquipmentComponent.status, Equal<FSEquipmentComponent.status.Active>,
                           And<
                               FSEquipmentComponent.SMequipmentID, Equal<Current<FSxSOLine.sMEquipmentID>>>>,
                           And<
                               Where2<
                                   Where<
                                       FSEquipmentComponent.itemClassID, Equal<InventoryItem.itemClassID>,
                                   Or<
                                       Current<FSxSOLine.equipmentAction>, Equal<FSxSOLine.equipmentAction.None>>>,
                               And<
                                   Where<
                                       Current<FSxSOLine.componentID>, IsNull,
                                   Or<
                                       FSEquipmentComponent.componentID, Equal<Current<FSxSOLine.componentID>>>>>>>>>),
                new Type[]
                {
                    typeof(FSEquipmentComponent.lineRef),
                    typeof(FSEquipmentComponent.componentID),
                    typeof(FSEquipmentComponent.longDescr),
                    typeof(FSEquipmentComponent.serialNumber),
                    typeof(FSEquipmentComponent.comment)
                })
        {
            this.SubstituteKey = typeof(FSEquipmentComponent.lineRef);
        }
    }

    public class FSSelectorEquipmentLineRefSOInvoiceAttribute : PXSelectorAttribute
    {
        public FSSelectorEquipmentLineRefSOInvoiceAttribute()
            : base(
                typeof(Search2<FSEquipmentComponent.lineNbr,
                       InnerJoin<InventoryItem,
                            On<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>>>,
                       Where2<
                           Where<
                               FSEquipmentComponent.status, Equal<FSEquipmentComponent.status.Active>,
                                And<FSEquipmentComponent.SMequipmentID, Equal<Current<FSxARTran.sMEquipmentID>>>>,
                           And<
                                Where2<
                                    Where<FSEquipmentComponent.itemClassID, Equal<InventoryItem.itemClassID>,
                                        Or<Current<FSxARTran.equipmentAction>, Equal<FSxARTran.equipmentAction.None>>>,
                                And<
                                    Where<
                                        Current<FSxARTran.componentID>, IsNull,
                                        Or<FSEquipmentComponent.componentID, Equal<Current<FSxARTran.componentID>>>>>>>>>),
                new Type[]
                {
                    typeof(FSEquipmentComponent.lineRef),
                    typeof(FSEquipmentComponent.componentID),
                    typeof(FSEquipmentComponent.longDescr),
                    typeof(FSEquipmentComponent.serialNumber),
                    typeof(FSEquipmentComponent.comment)
                })
        {
            this.SubstituteKey = typeof(FSEquipmentComponent.lineRef);
        }
    }
    #endregion
    #region ServiceOrder - Appointment

    public class FSSelectorComponentIDServiceOrderAttribute : PXCustomSelectorAttribute
    {
        private readonly Type CurrentTable;
        private readonly Type SourceTable;

        public FSSelectorComponentIDServiceOrderAttribute(Type currentTable, Type sourceTable)
            : base(typeof(FSModelTemplateComponent.componentID),
                   typeof(FSModelTemplateComponent.componentCD),
                   typeof(FSModelComponent.optional),
                   typeof(FSModelComponent.classID))
        {
            this.CurrentTable = currentTable;
            this.SourceTable = sourceTable;
            this.SubstituteKey = typeof(FSModelTemplateComponent.componentCD);
            this.CacheGlobal = false;
        }

        protected virtual IEnumerable GetRecords()
        {
            PXCache currentCache = this._Graph.Caches[CurrentTable];
            PXCache sourceCache = this._Graph.Caches[SourceTable];
            PXCache pivotCache;
            FSSODet currentRow = null;
            var rows = new PXResultset<FSModelTemplateComponent>();
            InventoryItem inventoryItemRow;

            int? pivotInventoryID = null;
            int? pivotItemClassID = null;

            foreach (object item in PXView.Currents)
            {
                if (item != null && (item.GetType() == CurrentTable))
                {
                    currentRow = item as FSSODet;
                    break;
                }
            }

            if (currentRow == null)
            {
                currentRow = currentCache.Current as FSSODet;
                if (currentRow == null)
                {
                    return null;
                }
            }

            pivotCache = currentCache.Equals(sourceCache) ? currentCache : sourceCache;
            inventoryItemRow = SharedFunctions.GetInventoryItemRow(_Graph, currentRow.InventoryID);

            if (currentRow.NewTargetEquipmentLineNbr != null)
            {
                foreach (object item in pivotCache.Cached)
                {
                    FSSODet currentFSSODet_temp = item as FSSODet;

                    if (currentFSSODet_temp.LineRef == currentRow.NewTargetEquipmentLineNbr)
                    {
                        pivotInventoryID = currentFSSODet_temp.InventoryID;
                        break;
                    }
                }
            }
            else if (currentRow.SMEquipmentID != null)
            {
                FSEquipment fsEquipmentRow = SharedFunctions.GetEquipmentRow(_Graph, currentRow.SMEquipmentID);
                pivotInventoryID = fsEquipmentRow.InventoryID;
            }

            if (pivotInventoryID != null)
            {
                pivotItemClassID = inventoryItemRow?.ItemClassID;
                PXResultset<FSModelTemplateComponent> resultRows;

                if (currentRow.EquipmentAction != ID.Equipment_Action.NONE)
                {
                    resultRows = PXSelectJoin<FSModelTemplateComponent,
                                 InnerJoin<FSModelComponent,
                                 On<
                                     FSModelTemplateComponent.componentID, Equal<FSModelComponent.componentID>>>,
                                 Where2<
                                    Where<
                                        FSModelComponent.active, Equal<True>,
                                        And<FSModelComponent.modelID, Equal<Required<FSModelComponent.modelID>>>>,
                                    And<
                                        Where<
                                            FSModelComponent.classID, Equal<Required<FSModelComponent.classID>>,
                                            Or<Required<FSModelComponent.classID>, IsNull>>>>>
                                 .Select(_Graph, pivotInventoryID, pivotItemClassID, pivotItemClassID);
                }
                else
                {
                    resultRows = PXSelectJoin<FSModelTemplateComponent,
                                 InnerJoin<FSModelComponent,
                                 On<
                                     FSModelTemplateComponent.componentID, Equal<FSModelComponent.componentID>>>,
                                 Where<
                                    FSModelComponent.active, Equal<True>,
                                    And<FSModelComponent.modelID, Equal<Required<FSModelComponent.modelID>>>>>
                                 .Select(_Graph, pivotInventoryID);
                }

                return resultRows;
            }

            return null;
        }
    }
    public class FSSelectorEquipmentLineRefServiceOrderAppointmentAttribute : PXSelectorAttribute
    {
        public FSSelectorEquipmentLineRefServiceOrderAppointmentAttribute(Type inventoryID, Type smEquipmentID, Type componentID , Type equipmentAction)
            : 
            base(

                BqlCommand.Compose(
                            typeof(Search2<,,>),
                                typeof(FSEquipmentComponent.lineNbr),
                            typeof(LeftJoin<,>),
                                typeof(InventoryItem),
                            typeof(On<,>),
                                    typeof(InventoryItem.inventoryID),
                                    typeof(Equal<>),
                                    typeof(Current<>),
                                    inventoryID,
                            typeof(Where2<,>),
                                typeof(Where<,,>),
                                    typeof(FSEquipmentComponent.SMequipmentID),
                                    typeof(Equal<>),
                                    typeof(Current<>),
                                        smEquipmentID,
                                    typeof(And<FSEquipmentComponent.status, Equal<FSEquipmentComponent.status.Active>>),
                            typeof(And<>),
                                typeof(Where2<,>),
                                    typeof(Where<,,>),
                                        typeof(FSEquipmentComponent.itemClassID),
                                        typeof(Equal<InventoryItem.itemClassID>),
                                    typeof(Or<,,>),
                                        typeof(Current<>),
                                            inventoryID,
                                        typeof(IsNull),
                                    typeof(Or<,>),
                                        typeof(Current<>),
                                            equipmentAction,
                                        typeof(Equal<ListField_EquipmentAction.None>),
                                typeof(And<>),
                                        typeof(Where<,,>),
                                            typeof(FSEquipmentComponent.componentID),
                                            typeof(Equal<>),
                                            typeof(Current<>),
                                            componentID,
                                        typeof(Or<,>),
                                            typeof(Current<>),
                                                componentID,
                                            typeof(IsNull)),
                new Type[]
                {
                    typeof(FSEquipmentComponent.lineRef),
                    typeof(FSEquipmentComponent.componentID),
                    typeof(FSEquipmentComponent.longDescr),
                    typeof(FSEquipmentComponent.serialNumber),
                    typeof(FSEquipmentComponent.comment)
                })
        {
            this.SubstituteKey = typeof(FSEquipmentComponent.lineRef);
        }
    }

    public class FSSelectorNewTargetEquipmentServiceOrderAttribute : PXSelectorAttribute
    {
        public FSSelectorNewTargetEquipmentServiceOrderAttribute()
            : base(
                typeof(Search2<FSSODet.lineRef,
                       InnerJoin<InventoryItem,
                       On<
                           InventoryItem.inventoryID, Equal<FSSODet.inventoryID>>>,
                       Where<
                           FSxEquipmentModel.equipmentItemClass, Equal<ListField_EquipmentItemClass.ModelEquipment>,
                       And<
                           FSSODet.equipmentAction, Equal<FSSODet.equipmentAction.SellingTargetEquipment>,
                       And<
                           FSSODet.sOID, Equal<Current<FSServiceOrder.sOID>>>>>>),
                new Type[]
                {
                typeof(FSSODet.lineRef),
                typeof(FSSODet.inventoryID)
                })
        {
        }
    }

    public class FSSelectorComponentIDAppointmentAttribute : PXCustomSelectorAttribute
    {
        private readonly Type CurrentTable;
        private readonly Type SourceTable;

        public FSSelectorComponentIDAppointmentAttribute(Type currentTable, Type sourceTable)
             : base(typeof(FSModelTemplateComponent.componentID),
                 new Type[]
                 {
                    typeof(FSModelTemplateComponent.componentCD),
                    typeof(FSModelTemplateComponent.descr),
                    typeof(FSModelTemplateComponent.optional),
                    typeof(FSModelTemplateComponent.classID)
                 })
        {
            this.CurrentTable = currentTable;
            this.SourceTable = sourceTable;
            this.SubstituteKey = typeof(FSModelTemplateComponent.componentCD);
            this.CacheGlobal = false;
        }

        protected virtual IEnumerable GetRecords()
        {
            PXCache currentCache = this._Graph.Caches[CurrentTable];
            PXCache sourceCache = this._Graph.Caches[SourceTable];
            PXCache pivotCache;
            FSAppointmentDet currentRow = null;
            var rows = new PXResultset<FSModelTemplateComponent>();
            InventoryItem inventoryItemRow;

            int? pivotInventoryID = null;
            int? pivotItemClassID = null;

            foreach (object item in PXView.Currents)
            {
                if (item != null && (item.GetType() == CurrentTable))
                {
                    currentRow = item as FSAppointmentDet;
                    break;
                }
            }

            if (currentRow == null)
            {
                currentRow = currentCache.Current as FSAppointmentDet;
                if (currentRow == null)
                {
                    return null;
                }
            }

            pivotCache = currentCache.Equals(sourceCache) ? currentCache : sourceCache;
            inventoryItemRow = SharedFunctions.GetInventoryItemRow(_Graph, currentRow.InventoryID);

            if (currentRow.NewTargetEquipmentLineNbr != null)
            {
                foreach (object item in pivotCache.Cached)
                {
                    FSAppointmentDet currentAppointmentDet_temp = item as FSAppointmentDet;

                    if (currentAppointmentDet_temp.LineRef == currentRow.NewTargetEquipmentLineNbr)
                    {
                        pivotInventoryID = currentAppointmentDet_temp.InventoryID;
                        break;
                    }
                }
            }
            else if (currentRow.SMEquipmentID != null)
            {
                FSEquipment fsEquipmentRow = SharedFunctions.GetEquipmentRow(_Graph, currentRow.SMEquipmentID);
                pivotInventoryID = fsEquipmentRow.InventoryID;
            }

            PXResultset<FSModelTemplateComponent> resultRows = new PXResultset<FSModelTemplateComponent>();

            if (pivotInventoryID != null)
            {
                pivotItemClassID = inventoryItemRow?.ItemClassID;

                if (currentRow.EquipmentAction != ID.Equipment_Action.NONE)
                {
                    resultRows = PXSelectJoin<FSModelTemplateComponent,
                                 LeftJoin<FSModelComponent,
                                 On<
                                     FSModelTemplateComponent.componentID, Equal<FSModelComponent.componentID>>>,
                                 Where2<
                                     Where<
                                         FSModelComponent.active, Equal<True>,
                                     And<
                                         FSModelComponent.modelID, Equal<Required<FSModelComponent.modelID>>>>,
                                     And<
                                         Where<
                                             FSModelComponent.classID, Equal<Required<FSModelComponent.classID>>,
                                         Or<
                                             Required<FSModelComponent.classID>, IsNull>>>>>
                                 .Select(_Graph, pivotInventoryID, pivotItemClassID, pivotItemClassID);
                }
                else
                {
                    resultRows = PXSelectJoin<FSModelTemplateComponent,
                                 InnerJoin<FSModelComponent,
                                 On<
                                     FSModelTemplateComponent.componentID, Equal<FSModelComponent.componentID>>>,
                                 Where<
                                     FSModelComponent.active, Equal<True>,
                                 And<
                                     FSModelComponent.modelID, Equal<Required<FSModelComponent.modelID>>>>>
                                 .Select(_Graph, pivotInventoryID);
                }

                return resultRows;
            }

            return resultRows;
        }
    }

    public class FSSelectorNewTargetEquipmentAppointmentAttribute : PXSelectorAttribute
    {
        public FSSelectorNewTargetEquipmentAppointmentAttribute()
            : base(
                typeof(Search2<FSAppointmentDet.lineRef,
                       InnerJoin<InventoryItem,
                       On<
                           InventoryItem.inventoryID, Equal<FSAppointmentDet.inventoryID>>>,
                       Where<
                            FSxEquipmentModel.equipmentItemClass, Equal<ListField_EquipmentItemClass.ModelEquipment>,
                       And<
                           FSAppointmentDet.lineType, Equal<FSLineType.Inventory_Item>,
                       And<
                           FSAppointmentDet.equipmentAction, Equal<FSAppointmentDet.equipmentAction.SellingTargetEquipment>,
                       And<
                           FSAppointmentDet.appointmentID, Equal<Current<FSAppointment.appointmentID>>,
                       And<
                           FSAppointmentDet.lineRef, IsNotNull>>>>>>),
                new Type[]
                {
                    typeof(FSAppointmentDet.lineRef),
                    typeof(FSAppointmentDet.inventoryID)
                })
        {
            DirtyRead = true;
        }
    }
    #endregion
    #region Schedule

    public class FSSelectorEquipmentLineRefAttribute : PXSelectorAttribute
    {
        public FSSelectorEquipmentLineRefAttribute(Type smEquipmentID, Type componentID)
            :
            base(
                BqlCommand.Compose(
                            typeof(Search<,>),
                                typeof(FSEquipmentComponent.lineNbr),
                            typeof(Where2<,>),
                                typeof(Where<,,>),
                                    typeof(FSEquipmentComponent.SMequipmentID),
                                    typeof(Equal<>),
                                    typeof(Current<>),
                                        smEquipmentID,
                                    typeof(And<FSEquipmentComponent.status, Equal<FSEquipmentComponent.status.Active>>),
                            typeof(And<>),
                                    typeof(Where<,,>),
                                        typeof(Current<>),
                                            componentID,
                                        typeof(IsNull),
                                    typeof(Or<,>),
                                        typeof(FSEquipmentComponent.componentID),
                                        typeof(Equal<>),
                                        typeof(Current<>),
                                        componentID),
                new Type[]
                {
                    typeof(FSEquipmentComponent.lineRef),
                    typeof(FSEquipmentComponent.componentID),
                    typeof(FSEquipmentComponent.longDescr),
                    typeof(FSEquipmentComponent.serialNumber),
                    typeof(FSEquipmentComponent.comment)
                })
        {
            this.SubstituteKey = typeof(FSEquipmentComponent.lineRef);
        }
    }

    public class FSSelectorComponentIDByFSEquipmentComponentAttribute : PXSelectorAttribute
    {
        public FSSelectorComponentIDByFSEquipmentComponentAttribute(Type smEquipmentID)
            :
            base(
                BqlCommand.Compose(
                            typeof(Search2<,,>),
                                typeof(FSModelTemplateComponent.componentID),
                            typeof(LeftJoin<,>),
                                typeof(FSEquipmentComponent),
                            typeof(On<,>),
                                typeof(FSEquipmentComponent.componentID),
                                typeof(Equal<>),
                                typeof(FSModelTemplateComponent.componentID),
                            typeof(Where<,,>),
                                typeof(FSEquipmentComponent.SMequipmentID),
                                typeof(Equal<>),
                                typeof(Current<>),
                                smEquipmentID,
                            typeof(And<FSEquipmentComponent.status, Equal<FSEquipmentComponent.status.Active>>)),
                new Type[]
                {
                    typeof(FSModelTemplateComponent.componentCD),
                    typeof(FSModelTemplateComponent.descr),
                    typeof(FSModelTemplateComponent.optional),
                    typeof(FSModelTemplateComponent.classID)
                })
        {
            this.SubstituteKey = typeof(FSModelTemplateComponent.componentCD);
        }
    }

    #endregion
    #endregion

    #region Route
    public class FSSelectorRouteAttribute : PXSelectorAttribute
    {
        public FSSelectorRouteAttribute()
            : base(
                typeof(Search<FSRouteDocument.routeDocumentID>),
                new Type[]
                {
                    typeof(FSRouteDocument.refNbr),
                    typeof(FSRouteDocument.date),
                    typeof(FSRouteDocument.driverID),
                    typeof(FSRouteDocument.routeID),
                    typeof(FSRouteDocument.status),
                    typeof(FSRouteDocument.vehicleID)                                                                     
                })
        {
            SubstituteKey = typeof(FSRouteDocument.refNbr);
        }
    }

    public class FSSelectorRouteIDAttribute : PXSelectorAttribute
    {
        public FSSelectorRouteIDAttribute()
            : base(
                typeof(Search<FSRoute.routeID>),
                new Type[]
                {
                    typeof(FSRoute.routeCD), 
                    typeof(FSRoute.descr), 
                    typeof(FSRoute.routeShort), 
                    typeof(FSRoute.weekCode)
                })
        {
            SubstituteKey = typeof(FSRoute.routeCD);
            DescriptionField = typeof(FSRoute.descr);
        }
    }

    internal class FSSelectorRouteDocumentPostingINAttribute : PXSelectorAttribute
    {
        public FSSelectorRouteDocumentPostingINAttribute()
            : base(
                typeof(Search5<FSRouteDocument.routeDocumentID,
                       InnerJoin<FSAppointment,
                       On<
                           FSAppointment.routeDocumentID, Equal<FSRouteDocument.routeDocumentID>>,
                       InnerJoin<FSAppointmentDet,
                       On<
                           FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>,
                       InnerJoin<FSSrvOrdType,
                       On<
                           FSSrvOrdType.srvOrdType, Equal<FSAppointment.srvOrdType>>>>>,
                       Where<
                           FSAppointmentDet.lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>>,
                       Aggregate<
                           GroupBy<FSRouteDocument.routeDocumentID>>>))
        {
            SubstituteKey = typeof(FSRouteDocument.refNbr);
        }
    }

    #endregion

    #region Vendors - SDEnabled
    public class FSSelectorVendorAttribute : PXDimensionSelectorAttribute
    {
        public FSSelectorVendorAttribute()
            : base(
                VendorAttribute.DimensionName,
                typeof(Search2<Vendor.bAccountID,
                       LeftJoin<Contact,
                       On<
                           Contact.bAccountID, Equal<Vendor.bAccountID>,
                           And<Contact.contactID, Equal<Vendor.defContactID>>>,
                       LeftJoin<Address,
                       On<
                           Address.bAccountID, Equal<Vendor.bAccountID>,
                           And<Address.addressID, Equal<Vendor.defAddressID>>>>>,
                       Where<
                           FSxVendor.sDEnabled, Equal<True>,
                       And<
                           Vendor.status, NotEqual<BAccount.status.inactive>>>>),
                typeof(Vendor.acctCD),
                new Type[]
                {
                    typeof(Vendor.acctCD),
                    typeof(Vendor.status),
                    typeof(Vendor.acctName),
                    typeof(Vendor.classID),                
                    typeof(Contact.phone1),
                    typeof(Address.city),
                    typeof(Address.countryID)
                })
        {
            DescriptionField = typeof(Vendor.acctName);
            DirtyRead = true;
        }
    }
    #endregion


    public class FSINLotSerialNbrAttribute : PXCustomSelectorAttribute
    {
        #region Custom INLotSerialStatus
        [Serializable]
        [PXCacheName(IN.Messages.INLotSerialStatus)]
        public class ApptINLotSerialStatus : PX.Data.IBqlTable, IStatus
        {
            #region Keys
            public new class PK : Data.ReferentialIntegrity.Attributes.PrimaryKeyOf<ApptINLotSerialStatus>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr, sODetID>
            {
                public static ApptINLotSerialStatus Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID, string lotSerialNbr, int? sODetID)
                    => FindBy(graph, inventoryID, subItemID, siteID, locationID, lotSerialNbr, sODetID);
            }
            public new static class FK
            {
                public class Location : INLocation.PK.ForeignKeyOf<ApptINLotSerialStatus>.By<locationID> { }
                public class LocationStatus : INLocationStatus.PK.ForeignKeyOf<ApptINLotSerialStatus>.By<inventoryID, subItemID, siteID, locationID> { }
                public class SubItem : INSubItem.PK.ForeignKeyOf<ApptINLotSerialStatus>.By<subItemID> { }
                public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<ApptINLotSerialStatus>.By<inventoryID> { }
                public class ItemLotSerial : INItemLotSerial.PK.ForeignKeyOf<ApptINLotSerialStatus>.By<inventoryID, lotSerialNbr> { }
                public class Site : INSite.PK.ForeignKeyOf<ApptINLotSerialStatus>.By<siteID> { }
            }
            #endregion
            #region InventoryID
            public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
            protected Int32? _InventoryID;
            [StockItem(IsKey = true)]
            [PXDefault()]
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
            #region SubItemID
            public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
            protected Int32? _SubItemID;
            [SubItem(IsKey = true)]
            [PXDefault()]
            public virtual Int32? SubItemID
            {
                get
                {
                    return this._SubItemID;
                }
                set
                {
                    this._SubItemID = value;
                }
            }
            #endregion
            #region SiteID
            public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
            protected Int32? _SiteID;
            [Site(IsKey = true)]
            [PXDefault()]
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
            #region LocationID
            public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
            protected Int32? _LocationID;
            [Location(typeof(ApptINLotSerialStatus.siteID), IsKey = true)]
            [PXDefault()]
            public virtual Int32? LocationID
            {
                get
                {
                    return this._LocationID;
                }
                set
                {
                    this._LocationID = value;
                }
            }
            #endregion
            #region LotSerialNbr
            public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr>
            {
                public const int LENGTH = 100;
            }
            protected String _LotSerialNbr;
            [PXDefault()]
            [LotSerialNbr(IsKey = true)]
            public virtual String LotSerialNbr
            {
                get
                {
                    return this._LotSerialNbr;
                }
                set
                {
                    this._LotSerialNbr = value;
                }
            }
            #endregion

            #region CostID
            public abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
            protected Int64? _CostID;
            [PXDBLong()]
            [PXDefault()]
            public virtual Int64? CostID
            {
                get
                {
                    return this._CostID;
                }
                set
                {
                    this._CostID = value;
                }
            }
            #endregion

            #region QtyFSSrvOrdBooked
            public abstract class qtyFSSrvOrdBooked : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdBooked> { }
            protected Decimal? _QtyFSSrvOrdBooked;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty. Allocated in Appointments", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
            public virtual Decimal? QtyFSSrvOrdBooked
            {
                get
                {
                    return this._QtyFSSrvOrdBooked;
                }
                set
                {
                    this._QtyFSSrvOrdBooked = value;
                }
            }
            #endregion
            #region QtyFSSrvOrdAllocated
            public abstract class qtyFSSrvOrdAllocated : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdAllocated> { }
            protected Decimal? _QtyFSSrvOrdAllocated;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty. Allocated in Service Order", Enabled = false, FieldClass = "SERVICEMANAGEMENT")]
            public virtual Decimal? QtyFSSrvOrdAllocated
            {
                get
                {
                    return this._QtyFSSrvOrdAllocated;
                }
                set
                {
                    this._QtyFSSrvOrdAllocated = value;
                }
            }
            #endregion
            #region QtyFSSrvOrdPrepared
            public abstract class qtyFSSrvOrdPrepared : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdPrepared> { }
            protected Decimal? _QtyFSSrvOrdPrepared;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(FieldClass = "SERVICEMANAGEMENT")]
            public virtual Decimal? QtyFSSrvOrdPrepared
            {
                get
                {
                    return this._QtyFSSrvOrdPrepared;
                }
                set
                {
                    this._QtyFSSrvOrdPrepared = value;
                }
            }
            #endregion

            #region Additional ServiceOrder Allocation fields
            #region SrvOrdAllocation
            public abstract class srvOrdAllocation : PX.Data.BQL.BqlBool.Field<srvOrdAllocation> { }
            [PXBool()]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Allocated in Service Order", Enabled = false)]
            public virtual Boolean? SrvOrdAllocation { get; set; }
            #endregion
            #region SODetID
            public abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }
            [PXInt]
            [PXDefault(-1)]
            public virtual Int32? SODetID { get; set; }
            #endregion
            #endregion

            #region QtyOnHand
            public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
            protected Decimal? _QtyOnHand;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty. On Hand")]
            public virtual Decimal? QtyOnHand
            {
                get
                {
                    return this._QtyOnHand;
                }
                set
                {
                    this._QtyOnHand = value;
                }
            }
            #endregion
            #region QtyAvail
            public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
            protected Decimal? _QtyAvail;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty. Available")]
            public virtual Decimal? QtyAvail
            {
                get
                {
                    return this._QtyAvail;
                }
                set
                {
                    this._QtyAvail = value;
                }
            }
            #endregion
            #region QtyNotAvail
            public abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
            protected Decimal? _QtyNotAvail;
            [PXDecimal(6)]
            [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual Decimal? QtyNotAvail
            {
                get
                {
                    return this._QtyNotAvail;
                }
                set
                {
                    this._QtyNotAvail = value;
                }
            }
            #endregion
            #region QtyExpired
            public abstract class qtyExpired : PX.Data.BQL.BqlDecimal.Field<qtyExpired> { }
            protected Decimal? _QtyExpired;
            [PXDecimal(6)]
            [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual Decimal? QtyExpired
            {
                get
                {
                    return this._QtyExpired;
                }
                set
                {
                    this._QtyExpired = value;
                }
            }
            #endregion
            #region QtyHardAvail
            public abstract class qtyHardAvail : PX.Data.BQL.BqlDecimal.Field<qtyHardAvail> { }
            protected Decimal? _QtyHardAvail;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty. Hard Available")]
            public virtual Decimal? QtyHardAvail
            {
                get
                {
                    return this._QtyHardAvail;
                }
                set
                {
                    this._QtyHardAvail = value;
                }
            }
            #endregion
            #region QtyActual
            public abstract class qtyActual : PX.Data.BQL.BqlDecimal.Field<qtyActual> { }
            protected decimal? _QtyActual;
            [PXDBQuantity]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty. Available for Issue")]
            public virtual decimal? QtyActual
            {
                get { return _QtyActual; }
                set { _QtyActual = value; }
            }
            #endregion
            #region QtyInTransit
            public abstract class qtyInTransit : PX.Data.BQL.BqlDecimal.Field<qtyInTransit> { }
            protected Decimal? _QtyInTransit;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? QtyInTransit
            {
                get
                {
                    return this._QtyInTransit;
                }
                set
                {
                    this._QtyInTransit = value;
                }
            }
            #endregion
            #region QtyInTransitToSO
            public abstract class qtyInTransitToSO : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToSO> { }
            protected Decimal? _QtyInTransitToSO;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? QtyInTransitToSO
            {
                get
                {
                    return this._QtyInTransitToSO;
                }
                set
                {
                    this._QtyInTransitToSO = value;
                }
            }
            #endregion
            #region QtyINReplaned
            public decimal? QtyINReplaned
            {
                get { return 0m; }
                set { }
            }
            #endregion
            #region QtyPOPrepared
            public abstract class qtyPOPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
            protected Decimal? _QtyPOPrepared;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? QtyPOPrepared
            {
                get
                {
                    return this._QtyPOPrepared;
                }
                set
                {
                    this._QtyPOPrepared = value;
                }
            }
            #endregion
            #region QtyPOOrders
            public abstract class qtyPOOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOOrders> { }
            protected Decimal? _QtyPOOrders;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? QtyPOOrders
            {
                get
                {
                    return this._QtyPOOrders;
                }
                set
                {
                    this._QtyPOOrders = value;
                }
            }
            #endregion
            #region QtyPOReceipts
            public abstract class qtyPOReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOReceipts> { }
            protected Decimal? _QtyPOReceipts;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? QtyPOReceipts
            {
                get
                {
                    return this._QtyPOReceipts;
                }
                set
                {
                    this._QtyPOReceipts = value;
                }
            }
            #endregion
            #region QtySOBackOrdered
            public abstract class qtySOBackOrdered : PX.Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
            protected Decimal? _QtySOBackOrdered;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? QtySOBackOrdered
            {
                get
                {
                    return this._QtySOBackOrdered;
                }
                set
                {
                    this._QtySOBackOrdered = value;
                }
            }
            #endregion
            #region QtySOPrepared
            public abstract class qtySOPrepared : PX.Data.BQL.BqlDecimal.Field<qtySOPrepared> { }
            protected Decimal? _QtySOPrepared;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? QtySOPrepared
            {
                get
                {
                    return this._QtySOPrepared;
                }
                set
                {
                    this._QtySOPrepared = value;
                }
            }
            #endregion
            #region QtySOBooked
            public abstract class qtySOBooked : PX.Data.BQL.BqlDecimal.Field<qtySOBooked> { }
            protected Decimal? _QtySOBooked;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? QtySOBooked
            {
                get
                {
                    return this._QtySOBooked;
                }
                set
                {
                    this._QtySOBooked = value;
                }
            }
            #endregion
            #region QtySOShipped
            public abstract class qtySOShipped : PX.Data.BQL.BqlDecimal.Field<qtySOShipped> { }
            protected Decimal? _QtySOShipped;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? QtySOShipped
            {
                get
                {
                    return this._QtySOShipped;
                }
                set
                {
                    this._QtySOShipped = value;
                }
            }
            #endregion
            #region QtySOShipping
            public abstract class qtySOShipping : PX.Data.BQL.BqlDecimal.Field<qtySOShipping> { }
            protected Decimal? _QtySOShipping;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual Decimal? QtySOShipping
            {
                get
                {
                    return this._QtySOShipping;
                }
                set
                {
                    this._QtySOShipping = value;
                }
            }
            #endregion
            #region QtyINIssues
            public abstract class qtyINIssues : PX.Data.BQL.BqlDecimal.Field<qtyINIssues> { }
            protected Decimal? _QtyINIssues;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Inventory Issues")]
            public virtual Decimal? QtyINIssues
            {
                get
                {
                    return this._QtyINIssues;
                }
                set
                {
                    this._QtyINIssues = value;
                }
            }
            #endregion
            #region QtyINReceipts
            public abstract class qtyINReceipts : PX.Data.BQL.BqlDecimal.Field<qtyINReceipts> { }
            protected Decimal? _QtyINReceipts;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Inventory Receipts")]
            public virtual Decimal? QtyINReceipts
            {
                get
                {
                    return this._QtyINReceipts;
                }
                set
                {
                    this._QtyINReceipts = value;
                }
            }
            #endregion
            #region QtyINAssemblyDemand
            public abstract class qtyINAssemblyDemand : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblyDemand> { }
            protected Decimal? _QtyINAssemblyDemand;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty Demanded by Kit Assembly")]
            public virtual Decimal? QtyINAssemblyDemand
            {
                get
                {
                    return this._QtyINAssemblyDemand;
                }
                set
                {
                    this._QtyINAssemblyDemand = value;
                }
            }
            #endregion
            #region QtyINAssemblySupply
            public abstract class qtyINAssemblySupply : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblySupply> { }
            protected Decimal? _QtyINAssemblySupply;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Kit Assembly")]
            public virtual Decimal? QtyINAssemblySupply
            {
                get
                {
                    return this._QtyINAssemblySupply;
                }
                set
                {
                    this._QtyINAssemblySupply = value;
                }
            }
            #endregion
            #region QtyInTransitToProduction
            public abstract class qtyInTransitToProduction : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToProduction> { }
            protected Decimal? _QtyInTransitToProduction;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity In Transit to Production.  
            /// </summary>
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty In Transit to Production")]
            public virtual Decimal? QtyInTransitToProduction
            {
                get
                {
                    return this._QtyInTransitToProduction;
                }
                set
                {
                    this._QtyInTransitToProduction = value;
                }
            }
            #endregion
            #region QtyProductionSupplyPrepared
            public abstract class qtyProductionSupplyPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProductionSupplyPrepared> { }
            protected Decimal? _QtyProductionSupplyPrepared;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity Production Supply Prepared.  
            /// </summary>
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty Production Supply Prepared")]
            public virtual Decimal? QtyProductionSupplyPrepared
            {
                get
                {
                    return this._QtyProductionSupplyPrepared;
                }
                set
                {
                    this._QtyProductionSupplyPrepared = value;
                }
            }
            #endregion
            #region QtyProductionSupply
            public abstract class qtyProductionSupply : PX.Data.BQL.BqlDecimal.Field<qtyProductionSupply> { }
            protected Decimal? _QtyProductionSupply;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On Production Supply.  
            /// </summary>
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Production Supply")]
            public virtual Decimal? QtyProductionSupply
            {
                get
                {
                    return this._QtyProductionSupply;
                }
                set
                {
                    this._QtyProductionSupply = value;
                }
            }
            #endregion
            #region QtyPOFixedProductionPrepared
            public abstract class qtyPOFixedProductionPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedProductionPrepared> { }
            protected Decimal? _QtyPOFixedProductionPrepared;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On Purchase for Prod. Prepared.  
            /// </summary>
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Purchase for Prod. Prepared")]
            public virtual Decimal? QtyPOFixedProductionPrepared
            {
                get
                {
                    return this._QtyPOFixedProductionPrepared;
                }
                set
                {
                    this._QtyPOFixedProductionPrepared = value;
                }
            }
            #endregion
            #region QtyPOFixedProductionOrders
            public abstract class qtyPOFixedProductionOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedProductionOrders> { }
            protected Decimal? _QtyPOFixedProductionOrders;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On Purchase for Production.  
            /// </summary>
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Purchase for Production")]
            public virtual Decimal? QtyPOFixedProductionOrders
            {
                get
                {
                    return this._QtyPOFixedProductionOrders;
                }
                set
                {
                    this._QtyPOFixedProductionOrders = value;
                }
            }
            #endregion
            #region QtyProductionDemandPrepared
            public abstract class qtyProductionDemandPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProductionDemandPrepared> { }
            protected Decimal? _QtyProductionDemandPrepared;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On Production Demand Prepared.  
            /// </summary>
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Production Demand Prepared")]
            public virtual Decimal? QtyProductionDemandPrepared
            {
                get
                {
                    return this._QtyProductionDemandPrepared;
                }
                set
                {
                    this._QtyProductionDemandPrepared = value;
                }
            }
            #endregion
            #region QtyProductionDemand
            public abstract class qtyProductionDemand : PX.Data.BQL.BqlDecimal.Field<qtyProductionDemand> { }
            protected Decimal? _QtyProductionDemand;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On Production Demand.  
            /// </summary>
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Production Demand")]
            public virtual Decimal? QtyProductionDemand
            {
                get
                {
                    return this._QtyProductionDemand;
                }
                set
                {
                    this._QtyProductionDemand = value;
                }
            }
            #endregion
            #region QtyProductionAllocated
            public abstract class qtyProductionAllocated : PX.Data.BQL.BqlDecimal.Field<qtyProductionAllocated> { }
            protected Decimal? _QtyProductionAllocated;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On Production Allocated.  
            /// </summary>
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Production Allocated")]
            public virtual Decimal? QtyProductionAllocated
            {
                get
                {
                    return this._QtyProductionAllocated;
                }
                set
                {
                    this._QtyProductionAllocated = value;
                }
            }
            #endregion
            #region QtySOFixedProduction
            public abstract class qtySOFixedProduction : PX.Data.BQL.BqlDecimal.Field<qtySOFixedProduction> { }
            protected Decimal? _QtySOFixedProduction;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On SO to Production.  
            /// </summary>
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On SO to Production")]
            public virtual Decimal? QtySOFixedProduction
            {
                get
                {
                    return this._QtySOFixedProduction;
                }
                set
                {
                    this._QtySOFixedProduction = value;
                }
            }
            #endregion

            #region QtyFixedFSSrvOrd
            public abstract class qtyFixedFSSrvOrd : PX.Data.BQL.BqlDecimal.Field<qtyFixedFSSrvOrd> { }
            protected decimal? _QtyFixedFSSrvOrd;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? QtyFixedFSSrvOrd
            {
                get
                {
                    return this._QtyFixedFSSrvOrd;
                }
                set
                {
                    this._QtyFixedFSSrvOrd = value;
                }
            }
            #endregion
            #region QtyPOFixedFSSrvOrd
            public abstract class qtyPOFixedFSSrvOrd : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrd> { }
            protected decimal? _QtyPOFixedFSSrvOrd;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? QtyPOFixedFSSrvOrd
            {
                get
                {
                    return this._QtyPOFixedFSSrvOrd;
                }
                set
                {
                    this._QtyPOFixedFSSrvOrd = value;
                }
            }
            #endregion
            #region QtyPOFixedFSSrvOrdPrepared
            public abstract class qtyPOFixedFSSrvOrdPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdPrepared> { }
            protected decimal? _QtyPOFixedFSSrvOrdPrepared;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? QtyPOFixedFSSrvOrdPrepared
            {
                get
                {
                    return this._QtyPOFixedFSSrvOrdPrepared;
                }
                set
                {
                    this._QtyPOFixedFSSrvOrdPrepared = value;
                }
            }
            #endregion
            #region QtyPOFixedFSSrvOrdReceipts
            public abstract class qtyPOFixedFSSrvOrdReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdReceipts> { }
            protected decimal? _QtyPOFixedFSSrvOrdReceipts;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? QtyPOFixedFSSrvOrdReceipts
            {
                get
                {
                    return this._QtyPOFixedFSSrvOrdReceipts;
                }
                set
                {
                    this._QtyPOFixedFSSrvOrdReceipts = value;
                }
            }
            #endregion

            #region QtyProdFixedPurchase
            // M9
            public abstract class qtyProdFixedPurchase : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedPurchase> { }
            protected Decimal? _QtyProdFixedPurchase;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On Production to Purchase.  
            /// </summary>
            [PXDBQuantity]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Production to Purchase", Enabled = false)]
            public virtual Decimal? QtyProdFixedPurchase
            {
                get
                {
                    return this._QtyProdFixedPurchase;
                }
                set
                {
                    this._QtyProdFixedPurchase = value;
                }
            }
            #endregion
            #region QtyProdFixedProduction
            // MA
            public abstract class qtyProdFixedProduction : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProduction> { }
            protected Decimal? _QtyProdFixedProduction;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On Production to Production
            /// </summary>
            [PXDBQuantity]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Production to Production", Enabled = false)]
            public virtual Decimal? QtyProdFixedProduction
            {
                get
                {
                    return this._QtyProdFixedProduction;
                }
                set
                {
                    this._QtyProdFixedProduction = value;
                }
            }
            #endregion
            #region QtyProdFixedProdOrdersPrepared
            // MB
            public abstract class qtyProdFixedProdOrdersPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrdersPrepared> { }
            protected Decimal? _QtyProdFixedProdOrdersPrepared;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On Production for Prod. Prepared
            /// </summary>
            [PXDBQuantity]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Production for Prod. Prepared", Enabled = false)]
            public virtual Decimal? QtyProdFixedProdOrdersPrepared
            {
                get
                {
                    return this._QtyProdFixedProdOrdersPrepared;
                }
                set
                {
                    this._QtyProdFixedProdOrdersPrepared = value;
                }
            }
            #endregion
            #region QtyProdFixedProdOrders
            // MC
            public abstract class qtyProdFixedProdOrders : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrders> { }
            protected Decimal? _QtyProdFixedProdOrders;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On Production for Production
            /// </summary>
            [PXDBQuantity]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Production for Production", Enabled = false)]
            public virtual Decimal? QtyProdFixedProdOrders
            {
                get
                {
                    return this._QtyProdFixedProdOrders;
                }
                set
                {
                    this._QtyProdFixedProdOrders = value;
                }
            }
            #endregion
            #region QtyProdFixedSalesOrdersPrepared
            // MD
            public abstract class qtyProdFixedSalesOrdersPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrdersPrepared> { }
            protected Decimal? _QtyProdFixedSalesOrdersPrepared;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On Production for SO Prepared
            /// </summary>
            [PXDBQuantity]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Production for SO Prepared", Enabled = false)]
            public virtual Decimal? QtyProdFixedSalesOrdersPrepared
            {
                get
                {
                    return this._QtyProdFixedSalesOrdersPrepared;
                }
                set
                {
                    this._QtyProdFixedSalesOrdersPrepared = value;
                }
            }
            #endregion
            #region QtyProdFixedSalesOrders
            // ME
            public abstract class qtyProdFixedSalesOrders : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrders> { }
            protected Decimal? _QtyProdFixedSalesOrders;
            /// <summary>
            /// Production / Manufacturing 
            /// Specifies the quantity On Production for SO
            /// </summary>
            [PXDBQuantity]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Qty On Production for SO", Enabled = false)]
            public virtual Decimal? QtyProdFixedSalesOrders
            {
                get
                {
                    return this._QtyProdFixedSalesOrders;
                }
                set
                {
                    this._QtyProdFixedSalesOrders = value;
                }
            }
            #endregion
            #region QtySOFixed
            public abstract class qtySOFixed : PX.Data.BQL.BqlDecimal.Field<qtySOFixed> { }
            protected decimal? _QtySOFixed;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? QtySOFixed
            {
                get
                {
                    return this._QtySOFixed;
                }
                set
                {
                    this._QtySOFixed = value;
                }
            }
            #endregion
            #region QtyPOFixedOrders
            public abstract class qtyPOFixedOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedOrders> { }
            protected decimal? _QtyPOFixedOrders;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? QtyPOFixedOrders
            {
                get
                {
                    return this._QtyPOFixedOrders;
                }
                set
                {
                    this._QtyPOFixedOrders = value;
                }
            }
            #endregion
            #region QtyPOFixedPrepared
            public abstract class qtyPOFixedPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedPrepared> { }
            protected decimal? _QtyPOFixedPrepared;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? QtyPOFixedPrepared
            {
                get
                {
                    return this._QtyPOFixedPrepared;
                }
                set
                {
                    this._QtyPOFixedPrepared = value;
                }
            }
            #endregion
            #region QtyPOFixedReceipts
            public abstract class qtyPOFixedReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedReceipts> { }
            protected decimal? _QtyPOFixedReceipts;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? QtyPOFixedReceipts
            {
                get
                {
                    return this._QtyPOFixedReceipts;
                }
                set
                {
                    this._QtyPOFixedReceipts = value;
                }
            }
            #endregion
            #region QtySODropShip
            public abstract class qtySODropShip : PX.Data.BQL.BqlDecimal.Field<qtySODropShip> { }
            protected decimal? _QtySODropShip;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? QtySODropShip
            {
                get
                {
                    return this._QtySODropShip;
                }
                set
                {
                    this._QtySODropShip = value;
                }
            }
            #endregion
            #region QtyPODropShipOrders
            public abstract class qtyPODropShipOrders : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipOrders> { }
            protected decimal? _QtyPODropShipOrders;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? QtyPODropShipOrders
            {
                get
                {
                    return this._QtyPODropShipOrders;
                }
                set
                {
                    this._QtyPODropShipOrders = value;
                }
            }
            #endregion
            #region QtyPODropShipPrepared
            public abstract class qtyPODropShipPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipPrepared> { }
            protected decimal? _QtyPODropShipPrepared;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? QtyPODropShipPrepared
            {
                get
                {
                    return this._QtyPODropShipPrepared;
                }
                set
                {
                    this._QtyPODropShipPrepared = value;
                }
            }
            #endregion
            #region QtyPODropShipReceipts
            public abstract class qtyPODropShipReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipReceipts> { }
            protected decimal? _QtyPODropShipReceipts;
            [PXDBQuantity()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            public virtual decimal? QtyPODropShipReceipts
            {
                get
                {
                    return this._QtyPODropShipReceipts;
                }
                set
                {
                    this._QtyPODropShipReceipts = value;
                }
            }
            #endregion
            #region ExpireDate
            public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
            protected DateTime? _ExpireDate;
            [PXDBDate(BqlField = typeof(INItemLotSerial.expireDate))]
            [PXUIField(DisplayName = "Expiry Date")]
            public virtual DateTime? ExpireDate
            {
                get
                {
                    return this._ExpireDate;
                }
                set
                {
                    this._ExpireDate = value;
                }
            }
            #endregion
            #region ReceiptDate
            public abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
            protected DateTime? _ReceiptDate;
            [PXDBDate()]
            [PXDefault()]
            public virtual DateTime? ReceiptDate
            {
                get
                {
                    return this._ReceiptDate;
                }
                set
                {
                    this._ReceiptDate = value;
                }
            }
            #endregion
            #region LotSerTrack
            public abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
            protected String _LotSerTrack;
            [PXDBString(1, IsFixed = true)]
            [PXDefault()]
            public virtual String LotSerTrack
            {
                get
                {
                    return this._LotSerTrack;
                }
                set
                {
                    this._LotSerTrack = value;
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
            #region LastModifiedDateTime
            public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
            protected DateTime? _LastModifiedDateTime;
            [PXDBLastModifiedDateTime()]
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

            public static ApptINLotSerialStatus GetApptINLotSerialStatus(PXCache inCache, INLotSerialStatus inLotSerialStatus, int? soDetID, PXCache apptCache, ref List<string> lotSerialStatusFields)
            {
                var apptINLotSerialStatus = new ApptINLotSerialStatus();

                if (lotSerialStatusFields == null)
                {
                    lotSerialStatusFields = new List<string>();

                    foreach (Type field in inCache.BqlFields)
                    {
                        string fieldName = apptCache.Fields.Where(f => f.ToLower() == field.Name.ToLower()).FirstOrDefault();
                        if (fieldName != null)
                        {
                            lotSerialStatusFields.Add(fieldName);
                        }
                    }
                }

                foreach (string fieldName in lotSerialStatusFields)
                {
                    apptCache.SetValue(apptINLotSerialStatus, fieldName, inCache.GetValue(inLotSerialStatus, fieldName));
                }

                apptINLotSerialStatus.SODetID = soDetID;

                return apptINLotSerialStatus;
            }
        }
        #endregion

        // TODO: Delete these variables and their corresponding parameters from the constructor, in the next major release.
        private readonly Type SiteID;
        private readonly Type InventoryType;
        private readonly Type SubItemType;
        private readonly Type LocationType;
        private readonly Type SrvOrdLineID;
        //*****************************************************************************************************************

        private PXView ApptINLotSerialStatusView;
        private List<string> LotSerialStatusFields = null;

        public FSINLotSerialNbrAttribute(Type SiteID, Type InventoryType, Type SubItemType, Type LocationType, Type SrvOrdLineID)
            : base(typeof(Search<ApptINLotSerialStatus.lotSerialNbr>),
                new Type[]
                {
                    typeof(ApptINLotSerialStatus.lotSerialNbr),
                    typeof(ApptINLotSerialStatus.siteID),
                    typeof(ApptINLotSerialStatus.locationID),
                    typeof(ApptINLotSerialStatus.qtyOnHand),
                    typeof(ApptINLotSerialStatus.qtyAvail),
                    typeof(ApptINLotSerialStatus.expireDate),
                    typeof(ApptINLotSerialStatus.srvOrdAllocation),
                    typeof(ApptINLotSerialStatus.qtyFSSrvOrdAllocated),
                    typeof(ApptINLotSerialStatus.qtyFSSrvOrdBooked)
                })
        {
            this.SiteID = SiteID;
            this.InventoryType = InventoryType;
            this.SubItemType = SubItemType;
            this.LocationType = LocationType;
            this.SrvOrdLineID = SrvOrdLineID;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            // TODO: Delete this view in the next major release. We only need the Cache.
            ApptINLotSerialStatusView = new PXView(sender.Graph, false, new Select<ApptINLotSerialStatus>(), new PXSelectDelegate(GetApptINLotSerialStatus));
            sender.Graph.Views.Add(Prefixed(nameof(ApptINLotSerialStatus).ToLower()), ApptINLotSerialStatusView);

            sender.Graph.Caches[typeof(ApptINLotSerialStatus)].DisableReadItem = true;
            sender.Graph.RowPersisting.AddHandler<ApptINLotSerialStatus>(ApptINLotSerialStatus_RowPersisting);
        }

        // TODO: Delete this method in the next major release.
        private IEnumerable GetApptINLotSerialStatus()
        {
            // We never do Select from this view. We only use its cache
            return new List<ApptINLotSerialStatus>();
        }

        private void ApptINLotSerialStatus_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            e.Cancel = true;
        }

        // TODO: Delete this method in the next major release.
        public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            base.FieldVerifying(sender, e);
        }

        protected virtual IEnumerable GetRecords()
        {
            FSAppointmentDet apptLine = null;

            foreach (object item in PXView.Currents)
            {
                if (item != null && (item.GetType() == typeof(FSAppointmentDet)))
                {
                    apptLine = item as FSAppointmentDet;
                    break;
                }
            }

            if (apptLine == null)
            {
                apptLine = _Graph.Caches[typeof(FSAppointmentDet)].Current as FSAppointmentDet;
                if (apptLine == null)
                {
                    return null;
                }
            }

            var lotSerialList = new List<ApptINLotSerialStatus>();
            int availableLotSerialCount = 0;

            if (apptLine.SODetID != null && apptLine.SODetID > 0)
            {
                AddLotSerialsFromSrvOrdSplit(apptLine, lotSerialList);

                foreach (ApptINLotSerialStatus lotSerial in lotSerialList)
                {
                    if (lotSerial.QtyFSSrvOrdAllocated - lotSerial.QtyFSSrvOrdBooked > 0m)
                    {
                        availableLotSerialCount++;
                    }
                }
            }

            if (availableLotSerialCount == 0)
            {
                AddLotSerialsFromIN(apptLine, lotSerialList);
            }

            return lotSerialList;
        }

        public virtual void AddLotSerialsFromIN(FSAppointmentDet apptLine, List<ApptINLotSerialStatus> lotSerialList)
        {
            if (apptLine == null)
            {
                return;
            }

            var existingLotSerials = new List<string>();
            foreach (ApptINLotSerialStatus lotSerialRow in lotSerialList)
            {
                existingLotSerials.Add(lotSerialRow.LotSerialNbr);
            }

            var lotSerials = PXSelectJoin<INLotSerialStatus,
                        InnerJoin<INSiteLotSerial, On<
                            INSiteLotSerial.inventoryID, Equal<INLotSerialStatus.inventoryID>,
                            And<INSiteLotSerial.siteID, Equal<INLotSerialStatus.siteID>,
                            And<INSiteLotSerial.lotSerialNbr, Equal<INLotSerialStatus.lotSerialNbr>>>>>,
                        Where<
                            INLotSerialStatus.inventoryID, Equal<Required<INLotSerialStatus.inventoryID>>,
                            And<INLotSerialStatus.subItemID, Equal<Required<INLotSerialStatus.subItemID>>,
                            And<INLotSerialStatus.qtyOnHand, Greater<decimal0>,
                            And<Where<
                                    INLotSerialStatus.locationID, Equal<Required<INLotSerialStatus.locationID>>,
                                    Or<Required<INLotSerialStatus.locationID>, IsNull>>>>>>>.
                        Select(_Graph,
                                apptLine.InventoryID,
                                apptLine.SubItemID,
                                apptLine.SiteLocationID,
                                apptLine.SiteLocationID);

            foreach (PXResult<INLotSerialStatus, INSiteLotSerial> result in lotSerials)
            {
                var inLotSerialStatus = (INLotSerialStatus)result;
                var inSiteLotSerial = (INSiteLotSerial)result;

                if (existingLotSerials.Contains(inLotSerialStatus.LotSerialNbr) == true)
                {
                    continue;
                }

                var lotSerialRow = ApptINLotSerialStatus.GetApptINLotSerialStatus(_Graph.Caches[typeof(INLotSerialStatus)], inLotSerialStatus, -1, ApptINLotSerialStatusView.Cache, ref LotSerialStatusFields);

                lotSerialRow.SrvOrdAllocation = false;
                lotSerialRow.QtyFSSrvOrdAllocated = null;
                lotSerialRow.QtyFSSrvOrdBooked = null;
                lotSerialRow.QtyAvail = inSiteLotSerial.QtyAvail;

                lotSerialRow = (ApptINLotSerialStatus)ApptINLotSerialStatusView.Cache.Update(lotSerialRow);
                ApptINLotSerialStatusView.Cache.SetStatus(lotSerialRow, PXEntryStatus.Held);
                lotSerialList.Add(lotSerialRow);
            }

            ApptINLotSerialStatusView.Cache.IsDirty = false;
        }

        public virtual void AddLotSerialsFromSrvOrdSplit(FSAppointmentDet apptLine, List<ApptINLotSerialStatus> lotSerialList)
        {
            if (apptLine == null || apptLine.SODetID == null || apptLine.SODetID < 0)
            {
                return;
            }

            FSSODet relatedSrvOrdLine = FSSODet.UK.Find(_Graph, apptLine.SODetID);
            if (relatedSrvOrdLine == null)
            {
                return;
            }

            var lotSerials = PXSelectJoin<INLotSerialStatus,
                        InnerJoin<INSiteLotSerial, On<
                            INSiteLotSerial.inventoryID, Equal<INLotSerialStatus.inventoryID>,
                            And<INSiteLotSerial.siteID, Equal<INLotSerialStatus.siteID>,
                            And<INSiteLotSerial.lotSerialNbr, Equal<INLotSerialStatus.lotSerialNbr>>>>,
                        InnerJoin<FSSODetSplit, On<
                            FSSODetSplit.inventoryID, Equal<INLotSerialStatus.inventoryID>,
                            And<FSSODetSplit.siteID, Equal<INLotSerialStatus.siteID>,
                            And<FSSODetSplit.lotSerialNbr, Equal<INLotSerialStatus.lotSerialNbr>>>>>>,
                        Where<
                            INLotSerialStatus.inventoryID, Equal<Required<INLotSerialStatus.inventoryID>>,
                            And<INLotSerialStatus.subItemID, Equal<Required<INLotSerialStatus.subItemID>>,
                            And2<
                                Where<
                                    INLotSerialStatus.locationID, Equal<Required<INLotSerialStatus.locationID>>,
                                    Or<Required<INLotSerialStatus.locationID>, IsNull>>,
                            And<Where<
                                    FSSODetSplit.srvOrdType, Equal<Required<FSSODetSplit.srvOrdType>>,
                                    And<FSSODetSplit.refNbr, Equal<Required<FSSODetSplit.refNbr>>,
                                    And<FSSODetSplit.lineNbr, Equal<Required<FSSODetSplit.lineNbr>>>>>>>>>,
                        OrderBy<
                            Asc<FSSODetSplit.splitLineNbr>>>.
                        Select(_Graph,
                                apptLine.InventoryID,
                                apptLine.SubItemID,
                                apptLine.SiteLocationID,
                                apptLine.SiteLocationID,
                                relatedSrvOrdLine.SrvOrdType,
                                relatedSrvOrdLine.RefNbr,
                                relatedSrvOrdLine.LineNbr);

            foreach (PXResult<INLotSerialStatus, INSiteLotSerial, FSSODetSplit> result in lotSerials)
            {
                var inLotSerialStatus = (INLotSerialStatus)result;
                var inSiteLotSerial = (INSiteLotSerial)result;
                var fsSODetSplit = (FSSODetSplit)result;

                var lotSerialRow = ApptINLotSerialStatus.GetApptINLotSerialStatus(_Graph.Caches[typeof(INLotSerialStatus)], inLotSerialStatus, relatedSrvOrdLine.SODetID, ApptINLotSerialStatusView.Cache, ref LotSerialStatusFields);

                decimal lotSerialAvailQty = 0m;
                decimal lotSerialUsedQty = 0m;
                bool foundServiceOrderAllocation;

                GetLotSerialAvailability(_Graph, apptLine, lotSerialRow.LotSerialNbr, false, out lotSerialAvailQty, out lotSerialUsedQty, out foundServiceOrderAllocation);

                lotSerialRow.SrvOrdAllocation = true;
                lotSerialRow.QtyFSSrvOrdAllocated = fsSODetSplit.Qty;
                lotSerialRow.QtyFSSrvOrdBooked = lotSerialUsedQty;

                lotSerialRow = (ApptINLotSerialStatus)ApptINLotSerialStatusView.Cache.Update(lotSerialRow);
                ApptINLotSerialStatusView.Cache.SetStatus(lotSerialRow, PXEntryStatus.Held);
                lotSerialList.Add(lotSerialRow);
            }

            ApptINLotSerialStatusView.Cache.IsDirty = false;
        }

        // TODO:
        [Obsolete("This method will be released in the next major release.")]
        public virtual List<FSApptLineSplit> GetServiceOrderSplitNumbers(PXCache sender, INLotSerClass lsClass, ILotSerNumVal lotSerNum, INLotSerTrack.Mode mode, bool ForceAutoNextNbr, decimal count, List<FSApptLineSplit> existingSplits)
        {
            List<FSApptLineSplit> ret = new List<FSApptLineSplit>();

            if (lsClass != null)
            {
                string LotSerTrack = (mode & INLotSerTrack.Mode.Create) > 0 ? lsClass.LotSerTrack : INLotSerTrack.NotNumbered;
                //bool AutoNextNbr = (mode & INLotSerTrack.Mode.Manual) == 0 && (lsClass.AutoNextNbr == true || ForceAutoNextNbr);
                bool AutoNextNbr = true;

                int allocatedQty = 0;

                foreach (ApptINLotSerialStatus lotSerialStatus in GetRecords())
                {
                    if (string.IsNullOrEmpty(lotSerialStatus.LotSerialNbr) == true)
                    {
                        continue;
                    }
                    else
                    {
                        FSApptLineSplit existingSplit = existingSplits.Where(s => s.LotSerialNbr == lotSerialStatus.LotSerialNbr).FirstOrDefault();
                        if (existingSplit != null)
                        {
                            if ((int)existingSplit.BaseQty >= (int)lotSerialStatus.QtyAvail)
                            {
                                continue;
                            }
                        }
                    }

                    FSApptLineSplit split = INLotSerialNbrAttribute.GetNextSplit<FSApptLineSplit>(sender, lsClass, lotSerNum);
                    split.LotSerialNbr = lotSerialStatus.LotSerialNbr;

                    ret.Add(split);

                    if (LotSerTrack == "S")
                    {
                        allocatedQty++;

                        if (allocatedQty == (int)count)
                        {
                            break;
                        }
                    }
                    else
                    {
                        allocatedQty = (int)count;
                        break;
                    }
                }

                if (allocatedQty < (int)count)
                {
                    switch (LotSerTrack)
                    {
                        case "N":
                            FSApptLineSplit detail = new FSApptLineSplit();
                            detail.AssignedNbr = string.Empty;
                            detail.LotSerialNbr = string.Empty;
                            detail.LotSerClassID = string.Empty;

                            ret.Add(detail);
                            break;
                        case "L":
                            if (AutoNextNbr)
                            {
                                detail = INLotSerialNbrAttribute.GetNextSplit<FSApptLineSplit>(sender, lsClass, lotSerNum);
                                detail.LotSerialNbr = string.Empty;
                                ret.Add(detail);
                            }
                            break;
                        case "S":
                            if (AutoNextNbr)
                            {
                                //for (int i = allocatedQty; i < (int)count; i++)
                                //{
                                detail = INLotSerialNbrAttribute.GetNextSplit<FSApptLineSplit>(sender, lsClass, lotSerNum);
                                detail.LotSerialNbr = string.Empty;
                                ret.Add(detail);
                                //}
                            }
                            break;
                    }
                }
            }
            return ret;
        }

        // TODO: Delete this method in the next major release.
        protected string Prefixed(string name)
        {
            return string.Format("{0}_{1}", GetType().Name, name);
        }

        // TODO:
        [Obsolete("This method will be deleted in the next major release.")]
        public virtual void GetLotSerialAvailability(PXGraph graphToQuery, FSAppointmentDet apptLine, int? soDetID, int? apptDetID, string lotSerialNbr, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
        => FSApptLotSerialNbrAttribute.GetLotSerialAvailabilityInt(graphToQuery, apptLine, soDetID, apptDetID, lotSerialNbr, out lotSerialAvailQty, out lotSerialUsedQty, out foundServiceOrderAllocation);

        public virtual void GetLotSerialAvailability(PXGraph graphToQuery, FSAppointmentDet apptLine, string lotSerialNbr, bool ignoreUseByApptLine, out decimal lotSerialAvailQty, out decimal lotSerialUsedQty, out bool foundServiceOrderAllocation)
        => FSApptLotSerialNbrAttribute.GetLotSerialAvailabilityInt(graphToQuery, apptLine, lotSerialNbr, ignoreUseByApptLine, out lotSerialAvailQty, out lotSerialUsedQty, out foundServiceOrderAllocation);
    }

    [PXDBInt()]
    [PXUIField(DisplayName = "Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
    public class FSSiteAvailAttribute : SiteAvailAttribute
    {
        #region Ctor
        public FSSiteAvailAttribute(Type InventoryType, Type SubItemType)
            : base(InventoryType, SubItemType)
        {
        }
        #endregion
        #region Implementation
        public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            base.FieldDefaulting(sender, e);

            FSSrvOrdType fsSrvOrdTypeRow = null;
            FSBranchLocation fsBranchLocationRow = null;

            if (sender.Graph is ServiceOrderEntry)
            {
                fsSrvOrdTypeRow = ((ServiceOrderEntry)sender.Graph).ServiceOrderTypeSelected.Current;
                fsBranchLocationRow = ((ServiceOrderEntry)sender.Graph).CurrentBranchLocation.Current;
            }
            else if (sender.Graph is AppointmentEntry)
            {
                fsSrvOrdTypeRow = ((AppointmentEntry)sender.Graph).ServiceOrderTypeSelected.Current;
                fsBranchLocationRow = ((AppointmentEntry)sender.Graph).CurrentBranchLocation.Current;
            }

            if (fsSrvOrdTypeRow?.PostTo == ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE)
            {
                e.NewValue = null;
            }
            else if (fsSrvOrdTypeRow?.PostTo != ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE && e.NewValue == null)
            {
                e.NewValue = fsBranchLocationRow?.DfltSiteID != null ? fsBranchLocationRow.DfltSiteID : e.NewValue;
            }
        }
        #endregion
    }
}
