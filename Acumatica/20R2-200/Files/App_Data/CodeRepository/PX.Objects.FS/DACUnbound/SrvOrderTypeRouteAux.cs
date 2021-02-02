using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class SrvOrderTypeRouteAux : IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type")]
        [PXDefault(typeof(Coalesce<
            Search2<FSxUserPreferences.dfltSrvOrdType,
            InnerJoin<
                FSSrvOrdType, On<FSSrvOrdType.srvOrdType, Equal<FSxUserPreferences.dfltSrvOrdType>>>,
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>,
                And<FSSrvOrdType.behavior, Equal<ListField_Behavior_SrvOrdType.RouteAppointment>>>>,
            Search<FSRouteSetup.dfltSrvOrdType>>))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType,
                    Where<FSSrvOrdType.active, Equal<True>, 
                    And<FSSrvOrdType.behavior, Equal<FSSrvOrdType.behavior.RouteAppointment>>>>))]
        public virtual string SrvOrdType { get; set; }
        #endregion
    }
}