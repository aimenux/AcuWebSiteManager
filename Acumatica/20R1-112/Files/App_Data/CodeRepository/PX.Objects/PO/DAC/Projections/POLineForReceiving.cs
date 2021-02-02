using System;
using PX.Data;

namespace PX.Objects.PO
{
    //This class contains POLineS projection definition only. POLineS DAC is currently located in POReceiptEntry 
    //TODO: Refactor POLineS and similar classes
    public class POLineForReceivingProjection : PXProjectionAttribute
    {
        public POLineForReceivingProjection()
            : base(typeof(Select<POLine>))
        {
        }

        protected override Type GetSelect(PXCache sender)
        {
            POSetup posetup = PXSetup<POSetup>.Select(sender.Graph);
				
            if (posetup.AddServicesFromNormalPOtoPR == true && posetup.AddServicesFromDSPOtoPR != true)
            {
                return typeof(Select<POLine, Where<POLine.lineType, NotEqual<POLineType.service>,
                    Or<POLine.orderType, Equal<POOrderType.regularOrder>>>>);
            }
            else if (posetup.AddServicesFromNormalPOtoPR != true && posetup.AddServicesFromDSPOtoPR == true)
            {
                return typeof(Select<POLine, Where<POLine.lineType, NotEqual<POLineType.service>,
                    Or<POLine.orderType, Equal<POOrderType.dropShip>>>>);
            }
            else if (posetup.AddServicesFromNormalPOtoPR == true && posetup.AddServicesFromDSPOtoPR == true)
            {
                return typeof(Select<POLine, Where<POLine.lineType, NotEqual<POLineType.service>,
                    Or<POLine.orderType, Equal<POOrderType.regularOrder>,
                        Or<POLine.orderType, Equal<POOrderType.dropShip>>>>>);
            }
            else
            {
                return typeof(Select<POLine, Where<POLine.lineType, NotEqual<POLineType.service>>>);
            }
        }
    }
}