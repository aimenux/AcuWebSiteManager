using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    internal static class LicenseHelper
    {
        private static void CheckStaffMembersLicense(int staffMembersCount)
        {
            int LicenseCount = 0;

            if (PXAccess.FeatureInstalled<FeaturesSet.serviceManagementStaffMembersPack50>() == true)
            {
                LicenseCount = 50;
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.serviceManagementStaffMembersPack10>() == true)
            {
                LicenseCount = 10;
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.serviceManagementStaffMembersPackUnlimited>() == true)
            {
                LicenseCount = int.MaxValue;
            }

            if (staffMembersCount > LicenseCount)
            {
                throw new PXException(TX.Error.STAFF_MEMBERS_COUNT_EXCEEDS_LICENSE_LIMIT, staffMembersCount, LicenseCount);
            }
        }

        internal static void CheckStaffMembersLicense(PXGraph graph, int? bAccountID, bool? SMEnabled, string status)
        {
            if (bAccountID != null && SMEnabled != null && status != null && (SMEnabled == false || status == BAccount.status.Inactive))
            {
                return;
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.serviceManagementStaffMembersPackUnlimited>() == true)
            {
                return;
            }

            int staffMembersCount = PXSelectJoin<BAccount,
                                    LeftJoin<Vendor,
                                        On<Vendor.bAccountID, Equal<BAccount.bAccountID>>,
                                    LeftJoin<EPEmployee,
                                        On<EPEmployee.bAccountID, Equal<BAccount.bAccountID>>>>,
                                    Where2<
                                        Where<
                                            Required<BAccount.bAccountID>, IsNull,
                                            Or<
                                            BAccount.bAccountID, NotEqual<Required<BAccount.bAccountID>>>>,
                                        And<
                                            Where2<
                                                Where<
                                                    FSxVendor.sDEnabled, Equal<True>,
                                                    And<Vendor.status, NotEqual<BAccount.status.inactive>>>,
                                                Or<
                                                    Where<
                                                        FSxEPEmployee.sDEnabled, Equal<True>,
                                                        And<EPEmployee.status, NotEqual<BAccount.status.inactive>>>>>>>>
                                  .Select(graph, bAccountID, bAccountID).Count;

            if (bAccountID != null && SMEnabled != null && status != null && SMEnabled == true && status != BAccount.status.Inactive)
            {
                staffMembersCount += 1;
            }

            CheckStaffMembersLicense(staffMembersCount);
        }

        private static void CheckVehiclesLicense(int vehiclesCount)
        {
            int LicenseCount = 0;

            if (PXAccess.FeatureInstalled<FeaturesSet.routeManagementVehiclesPack50>() == true)
            {
                LicenseCount = 50;
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.routeManagementVehiclesPack10>() == true)
            {
                LicenseCount = 10;
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.routeManagementVehiclesPackUnlimited>() == true)
            {
                LicenseCount = int.MaxValue;
            }

            if (vehiclesCount > LicenseCount)
            {
                throw new PXException(TX.Error.VEHICLES_COUNT_EXCEEDS_LICENSE_LIMIT, vehiclesCount, LicenseCount);
            }
        }

        internal static void CheckVehiclesLicense(PXGraph graph, int? SMEquipmentID, string status)
        {
            if (SMEquipmentID != null && status != null && status != EPEquipmentStatus.Active)
            {
                return;
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.routeManagementVehiclesPackUnlimited>() == true)
            {
                return;
            }

            int vehiclesCount = (int)PXSelectGroupBy<FSEquipment,
                                     Where<
                                         FSEquipment.status, Equal<EPEquipmentStatus.EquipmentStatusActive>,
                                     And<
                                         FSEquipment.isVehicle, Equal<True>,
                                     And<
                                         Where2<
                                             Where<
                                                 Required<FSEquipment.SMequipmentID>, IsNull>,
                                             Or<
                                                 FSEquipment.SMequipmentID, NotEqual<Required<FSEquipment.SMequipmentID>>>>>>>,
                                     Aggregate<Count>>
                                     .Select(graph, SMEquipmentID, SMEquipmentID).RowCount;

            if (SMEquipmentID != null && status != null && status == EPEquipmentStatus.Active)
            {
                vehiclesCount += 1;
            }

            CheckVehiclesLicense(vehiclesCount);
        }
    }
}