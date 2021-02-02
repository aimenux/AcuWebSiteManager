using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PX.Payroll;
using PX.Payroll.Data;
using PX.Api.Payroll;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRTaxType)]
	[Serializable]
	public class PRTaxType : IBqlTable
	{
		#region TypeName
		public abstract class typeName : IBqlField { }
		[PXString(50, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Name", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public string TypeName { get; set; }
		#endregion
		#region TaxCode
		public abstract class taxCode : IBqlField { }
		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Code", Visibility = PXUIVisibility.SelectorVisible)]
		public string TaxCode { get; set; }
		#endregion
		#region Description
		public abstract class description : IBqlField { }
		[PXString(250, IsUnicode = true)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
		public string Description { get; set; }
		#endregion
		#region TaxCategory
		public abstract class taxCategory : IBqlField { }
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[PXDefault]
		[TaxCategory.List]
		public string TaxCategory { get; set; }
		#endregion
		#region TaxJurisdiction
		public abstract class taxJurisdiction : IBqlField { }
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Jurisdiction Level", Visibility = PXUIVisibility.SelectorVisible)]
		[TaxJurisdiction.List]
		public string TaxJurisdiction { get; set; }
		#endregion
		#region IsUserDefined
		public abstract class isUserDefined : IBqlField { }
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is User-Defined")]
		public bool? IsUserDefined { get; set; }
		#endregion
		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		[PXString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Tax State", Visibility = PXUIVisibility.SelectorVisible)]
		public string State { get; set; }
		#endregion

	}

	public class TaxTypeSelectorAttribute : PXCustomSelectorAttribute, IDynamicSettingsUpdateObserver
	{
		private Type _TaxCategoryField;
		private MetaDynamicEntityDictionary<PRTax.Meta> _TaxTypeSettings = null;

		public TaxTypeSelectorAttribute(Type taxCategoryField)
			: base(typeof(PRTaxType.typeName))
		{
			this._TaxCategoryField = taxCategoryField;
			this.SubstituteKey = typeof(PRTaxType.taxCode);
			this.DescriptionField = typeof(PRTaxType.description);
		}

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            _TaxTypeSettings = GetAllMeta();
            PXPayrollAssemblyScope.SubscribeDynamicSettingsUpdate(this);
        }

        protected virtual IEnumerable GetRecords()
		{
            foreach (var taxType in _TaxTypeSettings.GetEnumerator())
			{
				yield return new PRTaxType
				{
					TypeName = taxType.TypeName,
					TaxCode = PRTax.GetUserFriendlyCode(taxType.Code),
					Description = taxType.TypeMeta.Description,
					TaxCategory = TaxCategory.GetTaxCategory(taxType.TypeMeta.TaxCategory),
					TaxJurisdiction = TaxJurisdiction.GetTaxJurisdiction(taxType.TypeMeta.TaxJurisdiction),
					State = (taxType.TypeMeta as IStateSpecific)?.State
				};
			}
		}

		private PXCache GetCurrentCache()
		{
			return this._Graph.Caches[_BqlTable];
		}

		private MetaDynamicEntityDictionary<PRTax.Meta> GetAllMeta()
		{
			using (var payrollAssemblyScope = new PXPayrollAssemblyScope<PayrollMetaProxy>())
			{
				return payrollAssemblyScope.Proxy.GetAllMeta<PRTax, TaxTypeAttribute>();
			}

		}

		public void DynamicSettingsUpdated()
		{
			_TaxTypeSettings = GetAllMeta();
		}
	}
}
