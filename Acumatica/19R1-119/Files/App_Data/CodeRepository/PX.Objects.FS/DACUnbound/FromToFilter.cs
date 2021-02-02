using System;
using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class FromToFilter : PX.Data.IBqlTable
    {
        #region DateBegin
        public abstract class dateBegin : PX.Data.BQL.BqlDateTime.Field<dateBegin> { }

        [PXDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "From", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? DateBegin { get; set; }
        #endregion
        #region DateEnd

        public abstract class dateEnd : PX.Data.BQL.BqlDateTime.Field<dateEnd> { }

        [PXDate]
        [PXUIField(DisplayName = "To", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? DateEnd { get; set; }
        #endregion
    }
}
