using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public partial class UpdateInventoryFilter : IBqlTable
    {
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXInt]
        [PXDefault(typeof(AccessInfo.branchID))]
        [PXUIField(DisplayName = "Branch")]
        public virtual int? BranchID { get; set; }
        #endregion
        #region CutOffDate
        public abstract class cutOffDate : PX.Data.BQL.BqlDateTime.Field<cutOffDate> { }

        [PXDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Up to Date")]
        public virtual DateTime? CutOffDate { get; set; }
        #endregion
        #region DocumentDate
        public abstract class documentDate : PX.Data.BQL.BqlDateTime.Field<documentDate> { }

        [PXDate]
        [PXFormula(typeof(UpdateInventoryFilter.cutOffDate))]
        [PXUIField(DisplayName = "Document Date")]
        public virtual DateTime? DocumentDate { get; set; }
        #endregion
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        [AROpenPeriod(typeof(UpdateInventoryFilter.cutOffDate), typeof(branchID))]
        [PXUIField(DisplayName = "Document Period", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string FinPeriodID { get; set; }
        #endregion
        #region RouteDocumentID
        public abstract class routeDocumentID : PX.Data.BQL.BqlInt.Field<routeDocumentID> { }

        [PXInt]
        [PXUIField(DisplayName = "Route Nbr.")]
        [FSSelectorRouteDocumentPostingIN]
        [PXRestrictor(typeof(Where<FSRouteDocument.date, LessEqual<Current<UpdateInventoryFilter.cutOffDate>>>), "")]
        [PXRestrictor(typeof(Where<FSRouteDocument.status, Equal<ListField_Status_Route.Closed>, Or<FSRouteDocument.status, Equal<ListField_Status_Route.Completed>>>), "")]
        [PXRestrictor(typeof(Where<FSSrvOrdType.behavior, Equal<ListField_Behavior_SrvOrdType.RouteAppointment>, And<FSSrvOrdType.enableINPosting, Equal<True>>>), "")]
        public virtual int? RouteDocumentID { get; set; }
        #endregion    
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXInt]
        [PXUIField(DisplayName = "Appointment Nbr.")]
        [FSSelectorAppointmentPostingIN]
        public virtual int? AppointmentID { get; set; }
        #endregion
    }
}
