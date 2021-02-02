using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class FSSelectorHelper : PX.Data.IBqlTable
    {
        #region Mem_int
        public abstract class mem_int : PX.Data.BQL.BqlInt.Field<mem_int> { }

        [PXInt]
        [PXUIField(DisplayName = "helper")]
        public virtual int? Mem_int { get; set; }
        #endregion
    }
}