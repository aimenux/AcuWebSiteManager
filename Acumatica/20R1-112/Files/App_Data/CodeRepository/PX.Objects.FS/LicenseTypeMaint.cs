using PX.Data;

namespace PX.Objects.FS
{
    public class LicenseTypeMaint : PXGraph<LicenseTypeMaint, FSLicenseType>
    {
        [PXImport(typeof(FSLicenseType))]
        public PXSelect<FSLicenseType> LicenseTypeRecords;
    }
}