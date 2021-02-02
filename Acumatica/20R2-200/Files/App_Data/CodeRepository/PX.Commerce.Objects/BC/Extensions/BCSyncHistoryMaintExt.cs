using PX.Commerce.Core;
using PX.CS;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects
{
	public class BCSyncHistoryMaintExt : PXGraphExtension<BCSyncHistoryMaint>
	{
		protected virtual void BCSyncDetail_LocalID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			BCSyncDetail row =(BCSyncDetail) e.Row;
			Object value = row?.LocalID;
			if (row?.LocalID != null)
			{
				Type type = null;
				switch (row.EntityType)
				{
					case BCEntitiesAttribute.OrderLine:
						{
							type = typeof(SOLine);
							break;
						}
					case BCEntitiesAttribute.ProductOption:
						{
							type = typeof(CSAttribute);
							break;
						}
					case BCEntitiesAttribute.ProductOptionValue:
						{
							type = typeof(CSAttributeDetail);
							break;
						}
					case BCEntitiesAttribute.ProductImage:
					case BCEntitiesAttribute.ProductVideo:
						{
							type = typeof(BCInventoryFileUrls);
							break;
						}
					default:
						break;
				}
			
				using (new PXReadDeletedScope())
				{
					String description = new EntityHelper(Base).GetEntityDescription(row?.LocalID, type);
					value = String.IsNullOrEmpty(description) ? row?.LocalID.ToString() : description;
				}
			}
			e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 100,
				true, typeof(BCSyncDetail.localID).Name, false, 1, null, null, null, false, null);
			e.ReturnValue = value;
		}
	}
}
