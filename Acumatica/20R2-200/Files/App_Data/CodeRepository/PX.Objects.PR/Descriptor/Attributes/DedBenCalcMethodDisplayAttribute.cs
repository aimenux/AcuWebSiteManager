using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.PR
{
	public abstract class DedBenCalcMethodDisplayAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		protected abstract bool IsDeductionField { get; }

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PRDeductCode row = e.Row as PRDeductCode;
			if (row == null)
			{
				return;
			}

			if (IsDeductionField && row.ContribType == ContributionType.EmployerContribution ||
				!IsDeductionField && row.ContribType == ContributionType.EmployeeDeduction)
			{
				e.ReturnValue = null;
			}
		}
	}

	public class DeductionCalcMethodDisplayAttribute : DedBenCalcMethodDisplayAttribute
	{
		protected override bool IsDeductionField => true;
	}

	public class BenefitCalcMethodDisplayAttribute : DedBenCalcMethodDisplayAttribute
	{
		protected override bool IsDeductionField => false;
	}
}
