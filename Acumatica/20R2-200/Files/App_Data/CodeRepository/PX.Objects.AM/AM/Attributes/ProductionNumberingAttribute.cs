using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM.Attributes
{
    public class ProductionNumberingAttribute : AutoNumberAttribute
    {
        public ProductionNumberingAttribute()
                : base(typeof(Search<AMOrderType.prodNumberingID, Where<AMOrderType.orderType, Equal<Current<AMProdItem.orderType>>>>), typeof(AMProdItem.prodDate))
        {
        }
    }
}
