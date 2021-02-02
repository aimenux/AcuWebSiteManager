using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.DAC;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
	public class AttributeGroupExt : PXGraphExtension<InventoryItemMaintBase>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.matrixItem>();
		}

		[PXHidden]
		public PXSelect<INAttributeDescriptionGroup> attributeDescriptionGroup;

		[PXHidden]
		public PXSelect<INAttributeDescriptionItem> attributeDescriptionItem;
	}
}
