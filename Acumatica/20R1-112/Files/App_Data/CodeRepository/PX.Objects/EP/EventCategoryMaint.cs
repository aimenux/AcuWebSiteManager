using PX.Data;

namespace PX.Objects.EP
{
	public class EventCategoryMaint : PXGraph<EventCategoryMaint>
	{
		public PXSelect<EPEventShowAs> 
			ShowAsTimes;
        public PXSavePerRow<EPEventShowAs> Save;
        public PXCancel<EPEventShowAs> Cancel;
        
        public PXSelect<EPEventShowAs, 
			Where<EPEventShowAs.color, IsNotNull>> 
			ColoredShowAsTimes;
	}
}
