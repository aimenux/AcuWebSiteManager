using PX.Data;
using PX.Objects.IN;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    [Serializable]
    public class StaffSelectionFilter : IBqlTable
    {
        #region ServiceLineRef
        public abstract class serviceLineRef : PX.Data.BQL.BqlString.Field<serviceLineRef> { }

        [PXString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Service Line Ref.")]
        [FSSelectorAppointmentSODetID]
        public virtual string ServiceLineRef { get; set; }
        #endregion
        #region PostalCode
        public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }

        [PXDefault(typeof(Search2<FSAddress.postalCode,
                            InnerJoin<FSServiceOrder,
                                On<FSServiceOrder.serviceOrderAddressID, Equal<FSAddress.addressID>>>,
                            Where<FSServiceOrder.sOID, Equal<Current<FSServiceOrder.sOID>>>>))]
        [PXString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Postal Code", Enabled = false)]
        public virtual string PostalCode { get; set; }
        #endregion
        #region GeoZoneID
        public abstract class geoZoneID : PX.Data.BQL.BqlInt.Field<geoZoneID> { }

        [PXInt]
        [PXUIField(DisplayName = "Service Area", Required = false)]
        [PXSelector(typeof(FSGeoZone.geoZoneID), SubstituteKey = typeof(FSGeoZone.geoZoneCD), DescriptionField = typeof(FSGeoZone.descr))]
        public virtual int? GeoZoneID { get; set; }
        #endregion
        #region ScheduledDateTimeBegin
        public abstract class scheduledDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<scheduledDateTimeBegin> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "Scheduled Date")]
        public virtual DateTime? ScheduledDateTimeBegin { get; set; }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXInt]
        [PXDefault(typeof(Search<FSServiceOrder.projectID, Where<FSServiceOrder.sOID, Equal<Current<FSServiceOrder.sOID>>>>))]
        [PXFormula(typeof(Default<FSServiceOrder.sOID>))]
        [PXUIField(DisplayName = "Project ID")]
        public virtual int? ProjectID { get; set; }
        #endregion

        #region ExistContractEmployees
        public abstract class existContractEmployees : PX.Data.BQL.BqlBool.Field<existContractEmployees> { }

        public virtual bool? ExistContractEmployees { get; set; }
        #endregion
    }

    [System.SerializableAttribute]
    public class SkillGridFilter : FSSkill
    {
        #region SkillCD
        [PXDBString(15, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Skill ID", Enabled = false)]
        public override string SkillCD { get; set; }

        #endregion
        #region Descr
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        public override string Descr { get; set; }
        #endregion
        #region Mem_Selected
        public abstract class mem_Selected : PX.Data.BQL.BqlBool.Field<mem_Selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Mem_Selected { get; set; }
        #endregion
        #region Mem_ServicesList
        public abstract class mem_ServicesList : PX.Data.BQL.BqlString.Field<mem_ServicesList> { }

        [PXString(200, IsUnicode = true)]
        [PXUIField(DisplayName = "Service List", Enabled = false)]
        public virtual string Mem_ServicesList { get; set; }
        #endregion

        public SkillGridFilter()
        {
        }

        /// <summary>
        /// Gets the Inventory CDs of the given services associated with the given skill.
        /// </summary>
        /// <param name="graph">Context graph that will be used in the query execution.</param>
        /// <param name="serviceIDList">Service identifier list which their Inventory CDs will be retrieved.</param>
        /// <param name="skillID">Skill identifier associated with the services to which they Inventory CDs will be retrieved.</param>
        /// <returns>String with the concatenation of the Inventory CDs, separated by commas, of the resulting services.</returns>
        public static string GetServiceListField(PXGraph graph, List<int?> serviceIDList, int? skillID)
        {
            string serviceList = string.Empty;

            if (serviceIDList.Count > 0 && skillID != null)
            {
                //Loading skill list for each service in serviceIDList
                List<SharedClasses.ItemList> serviceSkillList = SharedFunctions.GetItemWithList<InventoryItem,
                                                                                                InnerJoin<FSServiceSkill,
                                                                                                    On<FSServiceSkill.serviceID, Equal<InventoryItem.inventoryID>>>,
                                                                                                InventoryItem.inventoryID,
                                                                                                InventoryItem.inventoryCD,
                                                                                                Where<
                                                                                                    FSServiceSkill.skillID, Equal<Required<FSServiceSkill.skillID>>>>
                                                                                                (graph, serviceIDList, skillID);

                foreach (SharedClasses.ItemList element in serviceSkillList)
                {
                    string inventoryCD = (string)element.list[0];

                    if (string.IsNullOrEmpty(serviceList) == true)
                    {
                        serviceList = inventoryCD.Trim();
                    }
                    else
                    {
                        serviceList += ", " + inventoryCD.Trim();
                    }
                }
            }

            return serviceList;
        }
    }

    [System.SerializableAttribute]
    public class LicenseTypeGridFilter : FSLicenseType
    {
        #region LicenseTypeCD
        [PXDBString(15, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "License Type ID", Enabled = false)]
        public override string LicenseTypeCD { get; set; }
        #endregion
        #region Descr
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Enabled = false)]
        public override string Descr { get; set; }
        #endregion
        #region Mem_Selected
        public abstract class mem_Selected : PX.Data.BQL.BqlBool.Field<mem_Selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Mem_Selected { get; set; }
        #endregion
        #region Mem_FromService
        public abstract class mem_FromService : PX.Data.BQL.BqlBool.Field<mem_FromService> { }

        [PXBool]
        [PXUIField(DisplayName = "FromService")]
        public virtual bool? Mem_FromService { get; set; }
        #endregion

        public LicenseTypeGridFilter()
        {
        }

        /// <summary>
        /// Check if the given license type identifier is required for any service specified in the given service identifier list.
        /// </summary>
        /// <param name="graph">Context graph that will be used in the query execution.</param>
        /// <param name="licenseTypeID">License type identifier that will be use to check if the requirements are met.</param>
        /// <param name="serviceIDList">Service identifier list that will be consulted for the license type requirement.</param>
        /// <returns>Returns true if any of the services.</returns>
        public static bool IsThisLicenseTypeRequiredByAnyService(PXGraph graph, int? licenseTypeID, List<int?> serviceIDList)
        {
            if (serviceIDList.Count == 0 || licenseTypeID == null)
            {
                return false;
            }

            //Loading licenseType list for each service in serviceIDList
            List<SharedClasses.ItemList> serviceLicenseTypeList = SharedFunctions.GetItemWithList<FSServiceLicenseType,
                                                                                                  FSServiceLicenseType.serviceID,
                                                                                                  FSServiceLicenseType.licenseTypeID,
                                                                                                  Where<FSServiceLicenseType.licenseTypeID, 
                                                                                                    Equal<Required<FSServiceLicenseType.licenseTypeID>>>>
                                                                                                  (graph, serviceIDList, licenseTypeID);

            return serviceLicenseTypeList.Count > 0;
        }
    }
}
