using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
    public interface IUnitRateService
    {
		decimal? CalculateUnitPrice(PXCache sender, int? projectID, int? projectTaskID, int? inventoryID, string UOM, decimal? qty, DateTime? date, long? curyInfoID);
		
		decimal? CalculateUnitCost(PXCache sender, int? projectID, int? projectTaskID, int? inventoryID, string UOM, int? employeeID, DateTime? date, long? curyInfoID);
	}

    public class UnitRateService : IUnitRateService
	{
		public virtual decimal? CalculateUnitPrice(PXCache sender, int? projectID, int? projectTaskID, int? inventoryID, string UOM, decimal? qty, DateTime? date, long? curyInfoID)
		{
			if (inventoryID != null && inventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				if (date == null)
					date = sender.Graph.Accessinfo.BusinessDate;

				string customerPriceClass = ARPriceClass.EmptyPriceClass;

				if (projectTaskID != null)
				{
					PMTask projectTask = PMTask.PK.Find(sender.Graph, projectTaskID);
					CR.Location c = PXSelect<CR.Location, Where<CR.Location.locationID, Equal<Required<CR.Location.locationID>>>>.Select(sender.Graph, projectTask.LocationID);
					if (c != null && !string.IsNullOrEmpty(c.CPriceClassID))
						customerPriceClass = c.CPriceClassID;
				}

				PMProject project = PMProject.PK.Find(sender.Graph, projectID);
				CurrencyInfo curyInfo = null;

				if (curyInfoID != null)
                {
					curyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(sender.Graph, curyInfoID);
				}

				if (curyInfo != null)
                {
					return ARSalesPriceMaint.CalculateSalesPrice(sender.Graph.Caches[typeof(PMTran)], customerPriceClass, project?.CustomerID, inventoryID, null, curyInfo, UOM, qty, date.Value, 0);
				}
                else
                {
					//retrive price without conversion. (used in Templates when Currency is set but to Fx Rate)
					curyInfo = new CM.CurrencyInfo();
					curyInfo.CuryID = project?.CuryID ?? sender.Graph.Accessinfo.BaseCuryID;
					curyInfo.BaseCuryID = sender.Graph.Accessinfo.BaseCuryID;

					var salePriceMaint = ARSalesPriceMaint.SingleARSalesPriceMaint;
					bool alwaysFromBase = salePriceMaint.GetAlwaysFromBaseCurrencySetting(sender);
					ARSalesPriceMaint.SalesPriceItem spItem = salePriceMaint.CalculateSalesPriceItem(sender, customerPriceClass, project?.CustomerID, inventoryID, null, curyInfo, qty, UOM, date.Value, alwaysFromBase, false);

					if (spItem != null)
                    {
						decimal salesPrice = spItem.Price;
						if (spItem.UOM != UOM)
						{
							decimal salesPriceInBase = INUnitAttribute.ConvertFromBase(sender, inventoryID, spItem.UOM, salesPrice, INPrecision.UNITCOST);
							salesPrice = INUnitAttribute.ConvertToBase(sender, inventoryID, UOM, salesPriceInBase, INPrecision.UNITCOST);
						}

						return salesPrice;
					}
				}
			}

			return null;
		}

		public virtual decimal? CalculateUnitCost(PXCache sender, int? projectID, int? projectTaskID, int? inventoryID, string UOM, int? employeeID, DateTime? date, long? curyInfoID)
		{
			if (inventoryID != null && inventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				if (date == null)
					date = sender.Graph.Accessinfo.BusinessDate;

				bool lookForLaborRates = employeeID != null;
				InventoryItem item = InventoryItem.PK.Find(sender.Graph, inventoryID);
				PMProject project = PMProject.PK.Find(sender.Graph, projectID);
				int? laborItemID = null;

				if (item != null)
				{
					if (item.ItemType == INItemTypes.LaborItem)
					{
						laborItemID = item.InventoryID;
					}
					else
					{
						lookForLaborRates = false;
					}
				}

				decimal unitcostInBaseCury = 0;
				if (lookForLaborRates)
				{
					if (laborItemID == null && inventoryID == null)
					{
						EP.EPEmployee employee = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(sender.Graph, employeeID);

						if (employee != null)
						{
							laborItemID = employee.LabourItemID;
						}
					}

					//EmployeeID and LaborItemID.
					var cost = CreateEmployeeCostEngine(sender).CalculateEmployeeCost(null, GetRegulatHoursType(sender), laborItemID, projectID, projectTaskID, project?.CertifiedJob, null, employeeID, date.GetValueOrDefault(DateTime.Now));
					if (cost == null && laborItemID != null)
					{
						//EmployeeID only
						cost = CreateEmployeeCostEngine(sender).CalculateEmployeeCost(null, GetRegulatHoursType(sender), null, projectID, projectTaskID, project?.CertifiedJob, null, employeeID, date.GetValueOrDefault(DateTime.Now));
					}

					if (cost == null && laborItemID != null)
					{
						//LaborItemID only
						cost = CreateEmployeeCostEngine(sender).CalculateEmployeeCost(null, GetRegulatHoursType(sender), laborItemID, projectID, projectTaskID, project.CertifiedJob, null, null, date.GetValueOrDefault(DateTime.Now));
					}

					if (cost != null)
					{
						decimal unitCostForBaseUnit = cost.Rate.GetValueOrDefault();
						unitcostInBaseCury = unitCostForBaseUnit;

						if (inventoryID != null || laborItemID != null)
							unitcostInBaseCury = INUnitAttribute.ConvertFromBase(sender, inventoryID ?? laborItemID, UOM ?? EPSetup.Hour, unitCostForBaseUnit, INPrecision.UNITCOST);
					}
					else if (laborItemID != null && item != null)//fallback to Items Std Cost.
					{
						decimal unitCostForBaseUnit = item.StdCost.GetValueOrDefault();
						unitcostInBaseCury = INUnitAttribute.ConvertFromBase(sender, inventoryID ?? laborItemID, UOM ?? EPSetup.Hour, unitCostForBaseUnit, INPrecision.UNITCOST);
					}
				}
				else if (item != null)
				{
					decimal unitCostForBaseUnit = 0;
					if (item.ItemType == INItemTypes.LaborItem)
					{
						var cost = CreateEmployeeCostEngine(sender).CalculateEmployeeCost(null, GetRegulatHoursType(sender), inventoryID, projectID, projectTaskID, project?.CertifiedJob, null, null, date.GetValueOrDefault(DateTime.Now));

						if (cost != null)
						{
							unitCostForBaseUnit = cost.Rate.GetValueOrDefault();
						}
						else //fallback to Items Std Cost.
						{
							unitCostForBaseUnit = item.StdCost.GetValueOrDefault();
						}
					} 
					else if (item.StkItem == true)
					{
						INItemCost itemCost = INItemCost.PK.Find(sender.Graph, inventoryID);
						unitCostForBaseUnit = itemCost.AvgCost.GetValueOrDefault();
					}
					else
					{
						unitCostForBaseUnit = item.StdCost.GetValueOrDefault();
					}
					unitcostInBaseCury = INUnitAttribute.ConvertFromBase(sender, inventoryID, UOM, unitCostForBaseUnit, INPrecision.UNITCOST);
				}

				CurrencyInfo curyInfo = GetCurrencyInfo(sender, curyInfoID);

				decimal unitCostInCury;
				PXCurrencyAttribute.PXCurrencyHelper.CuryConvCury(sender, curyInfo, unitcostInBaseCury, out unitCostInCury);
				return unitCostInCury;
			}

			return null;
		}

		protected virtual bool GetAlwaysFromBaseCurrencySetting(PXCache sender)
		{
			ARSetup arsetup = (ARSetup)sender.Graph.Caches[typeof(ARSetup)].Current ?? PXSelect<ARSetup>.Select(sender.Graph);

			return arsetup != null
				? arsetup.AlwaysFromBaseCury == true
				: false;
		}

		protected virtual CurrencyInfo GetCurrencyInfo(PXCache sender, long? curyInfoID)
		{
			CurrencyInfo curyInfo;
			if (curyInfoID != null)
			{
				curyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(sender.Graph, curyInfoID);
			}
			else
			{
				curyInfo = new CM.CurrencyInfo();
				curyInfo.CuryID = sender.Graph.Accessinfo.BaseCuryID;
				curyInfo.BaseCuryID = sender.Graph.Accessinfo.BaseCuryID;
				curyInfo.CuryRate = 1;
			}

			return curyInfo;
		}

		protected virtual EmployeeCostEngine CreateEmployeeCostEngine(PXCache sender)
		{
			return new EmployeeCostEngine(sender.Graph);
		}

		protected virtual string GetRegulatHoursType(PXCache sender)
        {
			EPSetup setup = PXSelect<EPSetup>.Select(sender.Graph);
			if (setup != null)
            {
				return setup.RegularHoursType;
            }

			return "RG";
		}
	}
}
