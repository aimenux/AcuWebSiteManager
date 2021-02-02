using PX.Data;

namespace PX.Objects.FS
{
    public class ManufacturerModelMaint : PXGraph<ManufacturerModelMaint, FSManufacturerModel>
    {
        #region Selects
        public 
            PXSelect<FSManufacturerModel,
            Where<
                FSManufacturerModel.manufacturerID, Equal<Optional<FSManufacturerModel.manufacturerID>>>>
            ManufacturerModelRecords;

        public 
            PXSelect<FSManufacturerModel,
            Where<
                FSManufacturerModel.manufacturerModelID, Equal<Current<FSManufacturerModel.manufacturerModelID>>>>
            ManufacturerModelSelected;
        #endregion
    }
}