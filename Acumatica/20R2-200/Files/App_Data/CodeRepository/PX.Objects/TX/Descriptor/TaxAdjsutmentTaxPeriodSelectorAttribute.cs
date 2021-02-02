using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.GL;

namespace PX.Objects.TX.Descriptor
{
	[PXUIField(DisplayName = "Tax Period", Visibility = PXUIVisibility.SelectorVisible)]
	[PXDefault(null, typeof(
		Search2<TaxPeriod.taxPeriodID,
			InnerJoin<Branch,
				On<TaxPeriod.organizationID, Equal<Branch.organizationID>>>,
			Where<TaxPeriod.vendorID, Equal<Current<TaxAdjustment.vendorID>>,
				And<Branch.branchID, Equal<Current<TaxAdjustment.branchID>>,
					And<TaxPeriod.status, Equal<TaxPeriodStatus.prepared>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
	[PXSelector(typeof(
		Search2<TaxPeriod.taxPeriodID,
			InnerJoin<Branch,
				On<TaxPeriod.organizationID, Equal<Branch.organizationID>>>,
			Where<TaxPeriod.vendorID, Equal<Current<TaxAdjustment.vendorID>>,
				And<Branch.branchID, Equal<Current<TaxAdjustment.branchID>>>>>),
        typeof(TaxPeriod.taxPeriodID), typeof(TaxPeriod.startDateUI), typeof(TaxPeriod.endDateUI), typeof(TaxPeriod.status),
		SelectorMode = PXSelectorMode.NoAutocomplete)]
	[PXRestrictor(typeof(Where<TaxPeriod.status, Equal<TaxPeriodStatus.prepared>,
							Or<Current<TaxPeriod.status>, IsNull>>),
						Messages.TaxPeriodStatusIs,
						typeof(TaxPeriod.status))]
	public class TaxAdjsutmentTaxPeriodSelectorAttribute: FinPeriodIDAttribute, IPXFieldVerifyingSubscriber
	{
		protected int SelectorAtrributeIndex = -1;
		protected int RestrictorAtrributeIndex = -1;

		public TaxAdjsutmentTaxPeriodSelectorAttribute(): base()
		{
			PXEventSubscriberAttribute[] staticDefinedAtts = this.GetType().GetCustomAttributes(typeof (PXEventSubscriberAttribute), true)
																			.Cast<PXEventSubscriberAttribute>()
																			.ToArray();

			foreach (PXEventSubscriberAttribute attribute in staticDefinedAtts)
			{
				if (!_Attributes.Contains(attribute))
				{
					_Attributes.Add(attribute);

					if (attribute is PXSelectorAttribute)
					{
						SelectorAtrributeIndex = _Attributes.Count - 1;
					}
					else if (attribute is PXRestrictorAttribute)
					{
						RestrictorAtrributeIndex = _Attributes.Count - 1;
					}
				}
			}
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			try
			{
				((IPXFieldVerifyingSubscriber)_Attributes[SelectorAtrributeIndex]).FieldVerifying(sender, e);
				((IPXFieldVerifyingSubscriber)_Attributes[RestrictorAtrributeIndex]).FieldVerifying(sender, e);
			}
			catch (PXSetPropertyException)
			{
				e.NewValue = FormatForDisplay((string)e.NewValue);
				throw;
			}
		}
	}
}
