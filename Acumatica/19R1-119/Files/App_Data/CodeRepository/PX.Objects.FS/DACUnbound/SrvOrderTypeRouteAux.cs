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
        [PXDefault(typeof(FSRouteSetup.dfltSrvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType,
                    Where<FSSrvOrdType.active, Equal<True>, 
                    And<FSSrvOrdType.behavior, Equal<FSSrvOrdType.behavior.RouteAppointment>>>>))]
        public virtual string SrvOrdType { get; set; }
        #endregion
    }
}