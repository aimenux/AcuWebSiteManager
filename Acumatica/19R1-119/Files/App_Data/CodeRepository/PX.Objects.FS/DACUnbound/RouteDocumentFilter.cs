using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public partial class RouteDocumentFilter : IBqlTable
    {
        #region RouteID
        public abstract class routeID : PX.Data.BQL.BqlInt.Field<routeID> { }

        [PXInt]
        [PXUIField(DisplayName = "Route ID")]
        [FSSelectorRouteID]
        public virtual int? RouteID { get; set; }
        #endregion
        #region StatusOpen
        public abstract class statusOpen : PX.Data.BQL.BqlBool.Field<statusOpen> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Open")]
        public virtual bool? StatusOpen { get; set; }
        #endregion
        #region StatusInProcess
        public abstract class statusInProcess : PX.Data.BQL.BqlBool.Field<statusInProcess> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "In Process")]
        public virtual bool? StatusInProcess { get; set; }
        #endregion
        #region StatusCanceled
        public abstract class statusCanceled : PX.Data.BQL.BqlBool.Field<statusCanceled> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Canceled")]
        public virtual bool? StatusCanceled { get; set; }
        #endregion
        #region StatusCompleted
        public abstract class statusCompleted : PX.Data.BQL.BqlBool.Field<statusCompleted> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Completed")]
        public virtual bool? StatusCompleted { get; set; }
        #endregion
        #region StatusClosed
        public abstract class statusClosed : PX.Data.BQL.BqlBool.Field<statusClosed> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Closed")]
        public virtual bool? StatusClosed { get; set; }
        #endregion
        #region FromDate
        public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }

        [PXDate]
        [PXUIField(DisplayName = "From", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? FromDate { get; set; }
        #endregion
        #region ToDate
        public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }

        [PXDate]
        [PXUIField(DisplayName = "To", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ToDate { get; set; }
        #endregion
    }
}
