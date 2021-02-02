using PX.Data;

namespace PX.Objects.EP
{
	public class EventTaskCategoryMaint : PXGraph<EventTaskCategoryMaint>
	{
		public PXSelect<EPEventCategory> 
			Categories;
        public PXSavePerRow<EPEventCategory> Save;
        public PXCancel<EPEventCategory> Cancel;

		public PXSelect<EPEventCategory, 
			Where<EPEventCategory.color, IsNotNull>> 
			ColoredCategories;
	}
}
