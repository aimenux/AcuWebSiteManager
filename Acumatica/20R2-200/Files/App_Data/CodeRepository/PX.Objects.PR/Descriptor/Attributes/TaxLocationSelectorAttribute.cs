using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Payroll.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class TaxLocationSelectorAttribute : PXCustomSelectorAttribute
	{
		[Serializable]
		public class TaxLocation : IBqlTable
		{
			#region TaxID
			public class taxID : IBqlField { }
			[PXString(30, IsKey = true, IsUnicode = true)]
			[PXUIField(DisplayName = "Unique Tax ID", Visibility = PXUIVisibility.SelectorVisible)]
			public string TaxID { get; set; }
			#endregion

			#region Description
			public class description : IBqlField { }
			[PXString(100, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
			public string Description { get; set; }
			#endregion
		}

		protected Type _TaxTypeField;
		protected Type _JurisdictionLevel;
		protected Type _TaxState;

		public TaxLocationSelectorAttribute(Type taxTypeField, Type jurisdictionLevel, Type taxState) : base(typeof(TaxLocation.taxID))
		{
			_DescriptionField = typeof(TaxLocation.description);
			_TaxTypeField = taxTypeField;
			_JurisdictionLevel = jurisdictionLevel;
			_TaxState = taxState;
		}

		public virtual IEnumerable GetRecords()
		{
			//Assumes that all fields are on the same cache
			var cache = _Graph.Caches[BqlCommand.GetItemType(_TaxTypeField)];

			var locationSearch = GetLocationSearch(cache);

			var service = new PayrollTaxClient();
			var taxItems = service.GetSpecificTaxTypes(cache.GetValue(cache.Current, _TaxTypeField.Name)?.ToString(),
													   locationSearch);

			foreach (var item in taxItems)
			{
				yield return new TaxLocation { TaxID = item.UniqueTaxID, Description = item.TaxIDDescription };
			}
		}

		public string GetLocationSearch(PXCache cache)
		{
			var jurisdictionLevel = cache.GetValue(cache.Current, _JurisdictionLevel.Name)?.ToString();
			if (jurisdictionLevel != TaxJurisdiction.Federal)
			{
				string stateID = cache.GetValue(cache.Current, _TaxState.Name)?.ToString();
				if (!string.IsNullOrEmpty(stateID))
				{
					return PRState.FromAbbr(stateID).LocationCode.ToString("00");
				}
			}
			return "00";
		}
	}
}
