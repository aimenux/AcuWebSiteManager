using PX.Data;

namespace PX.Objects.CN.Subcontracts.PO.DAC
{
    [PXHidden]
    public class PurchaseOrderTypeFilter : IBqlTable
    {
        [PXString]
        public string Type1
        {
            get;
            set;
        }

        [PXString]
        public string Type2
        {
            get;
            set;
        }

        [PXString]
        public string Type3
        {
            get;
            set;
        }

        [PXString]
        public string Type4
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(Visibility = PXUIVisibility.Invisible)]
        public virtual string Graph
        {
            get;
            set;
        }

        public abstract class graph : IBqlField
        {
        }

        public abstract class type1 : IBqlField
        {
        }

        public abstract class type2 : IBqlField
        {
        }

        public abstract class type3 : IBqlField
        {
        }

        public abstract class type4 : IBqlField
        {
        }
    }
}