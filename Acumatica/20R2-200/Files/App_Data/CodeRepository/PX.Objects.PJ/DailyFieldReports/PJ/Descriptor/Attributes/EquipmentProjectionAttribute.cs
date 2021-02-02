using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class EquipmentProjectionAttribute : PXProjectionAttribute
    {
        public EquipmentProjectionAttribute()
            : base(typeof(SelectFrom<EPEquipmentDetail>
            .LeftJoin<EPEquipmentTimeCard>.On<EPEquipmentDetail.timeCardCD.IsEqual<EPEquipmentTimeCard.timeCardCD>>
            .LeftJoin<EPEquipment>.On<EPEquipment.equipmentID.IsEqual<EPEquipmentTimeCard.equipmentID>>
            .LeftJoin<DailyFieldReportEquipment>
                .On<DailyFieldReportEquipment.equipmentTimeCardCd.IsEqual<EPEquipmentDetail.timeCardCD>
                    .And<DailyFieldReportEquipment.equipmentDetailLineNumber.IsEqual<EPEquipmentDetail.lineNbr>>>),
            new[]
            {
                typeof(DailyFieldReportEquipment)
            })
        {
        }

        public override bool PersistDeleted(PXCache cache, object row)
        {
            return true;
        }
    }
}