using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.IN
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class INUnitMaint : PXGraph<INUnitMaint>
	{
		public PXSelect<INUnit, Where<INUnit.unitType,Equal<INUnitType.global>>> Unit;
        public PXSavePerRow<INUnit> Save;
        public PXCancel<INUnit> Cancel;

        protected virtual void INUnit_FromUnit_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void INUnit_ToUnit_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            e.Cancel = true;
        }

		protected virtual void INUnit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (INUnit)e.Row;
			if (row == null)
				return;

			if (e.Operation == PXDBOperation.Delete)
				return;

			if(row.FromUnit == row.ToUnit && row.UnitRate != 1m)
				throw new PXRowPersistingException(typeof(INUnit.unitRate).Name, null, Messages.WrongUnitConversion, row.FromUnit);
		}

		protected virtual void INUnit_UnitRate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            Decimal? conversion = (Decimal?)e.NewValue;
            if (conversion <= 0m)
            {
                throw new PXSetPropertyException(CS.Messages.Entry_GT, "0");
            }
        }

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if(viewName == nameof(Unit) && searches?.Any() == true)
			{
				bool removeFromSearches(string field, Func<object, bool> condition = null)
				{
					var i = Array.IndexOf(sortcolumns, field);
					if(i >= 0 
						&& (condition == null || condition(searches[i])))
					{
						searches[i] = null;
						return true;
					}
					return false;
				};
				if(removeFromSearches(nameof(INUnit.UnitType), v => v as short? == INUnitType.InventoryItem))
				{
					removeFromSearches(nameof(INUnit.InventoryID));
					removeFromSearches(nameof(INUnit.ItemClassID));
					removeFromSearches(nameof(INUnit.ToUnit));
				}
			}
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}
	}
}
