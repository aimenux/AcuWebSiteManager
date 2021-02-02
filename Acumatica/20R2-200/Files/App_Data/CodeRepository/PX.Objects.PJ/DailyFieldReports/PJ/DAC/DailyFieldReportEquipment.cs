using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Equipment")]
    public class DailyFieldReportEquipment : IBqlTable
    {
        [PXDBIdentity]
        public virtual int? DailyFieldReportEquipmentId
        {
            get;
            set;
        }

        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId>>))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDBString(10, IsKey = true, IsUnicode = true)]
        [PXParent(typeof(SelectFrom<EPEquipmentTimeCard>
            .Where<EPEquipmentTimeCard.timeCardCD.IsEqual<equipmentTimeCardCd>>))]
        public virtual string EquipmentTimeCardCd
        {
            get;
            set;
        }

        [PXDBInt(IsKey = true)]
        [PXParent(typeof(SelectFrom<EPEquipmentDetail>
            .Where<EPEquipmentDetail.lineNbr.IsEqual<equipmentDetailLineNumber>
                .And<EPEquipmentDetail.timeCardCD.IsEqual<equipmentTimeCardCd>>>))]
        public virtual int? EquipmentDetailLineNumber
        {
            get;
            set;
        }

        public abstract class dailyFieldReportEquipmentId : BqlInt.Field<dailyFieldReportEquipmentId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class equipmentTimeCardCd : BqlString.Field<equipmentTimeCardCd>
        {
        }

        public abstract class equipmentDetailLineNumber : BqlInt.Field<equipmentDetailLineNumber>
        {
        }
    }
}