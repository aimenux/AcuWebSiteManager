using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.Descriptor.Attributes;

namespace PX.Objects.PJ.DrawingLogs.PJ.DAC
{
    [PXCacheName("Linked Drawing Log Relation")]
    public class LinkedDrawingLogRelation : DrawingLogRelation, IPXSelectable
    {
        [PXBool]
        [UiInformationField]
        public bool? Selected
        {
            get;
            set;
        }

        public abstract class selected : BqlBool.Field<selected>
        {
        }
    }
}
