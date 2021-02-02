using PX.Api;
using PX.Data;
using PX.Objects.PR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		public class TaxEarningDetailsSplits : Dictionary<int?, decimal?> { }
		public class DedBenEarningDetailsSplits : Dictionary<int?, DedBenAmount> { }

		protected class PaymentCalculationInfo
		{
			public PaymentCalculationInfo(PRPayment payment)
			{
				Payment = payment;
			}

			public PRPayment Payment { get; set; }

			public decimal NetIncomeAccumulator { get; set; } = 0m;

			public Dictionary<int?, DedBenAmount> NominalTaxableDedBenAmounts { get; set; } = new Dictionary<int?, DedBenAmount>();

			public Dictionary<ProjectDedBenPackageKey, PackageDedBenCalculation> NominalProjectPackageAmounts { get; set; } 
				= new Dictionary<ProjectDedBenPackageKey, PackageDedBenCalculation>();

			public Dictionary<UnionDedBenPackageKey, PackageDedBenCalculation> NominalUnionPackageAmounts { get; set; }
				= new Dictionary<UnionDedBenPackageKey, PackageDedBenCalculation>();

			public Dictionary<FringeBenefitDecreasingRateKey, PRPaymentFringeBenefitDecreasingRate> FringeRateReducingBenefits { get; set; }
				= new Dictionary<FringeBenefitDecreasingRateKey, PRPaymentFringeBenefitDecreasingRate>();

			public Dictionary<FringeEarningDecreasingRateKey, PRPaymentFringeEarningDecreasingRate> FringeRateReducingEarnings { get; set; }
				= new Dictionary<FringeEarningDecreasingRateKey, PRPaymentFringeEarningDecreasingRate>();

			public List<FringeSourceEarning> FringeRates { get; set; } = new List<FringeSourceEarning>();

			public Dictionary<int?, FringeAmountInfo> FringeAmountsPerProject { get; set; } = new Dictionary<int?, FringeAmountInfo>();

			// Key is TaxID
			public Dictionary<int?, TaxEarningDetailsSplits> TaxesSplitByEarning { get; set; } = new Dictionary<int?, TaxEarningDetailsSplits>();

			// Key is CodeID
			public Dictionary<int?, DedBenEarningDetailsSplits> TaxableDeductionsAndBenefitsSplitByEarning { get; set; } = new Dictionary<int?, DedBenEarningDetailsSplits>();
		}

		protected class PaymentCalculationInfoCollection : IEnumerable<PaymentCalculationInfo>
		{
			private Dictionary<string, PaymentCalculationInfo> _PaymentInfoList = new Dictionary<string, PaymentCalculationInfo>();

			public PaymentCalculationInfo this[PRPayment key]
			{
				get
				{
					return this[key.PaymentDocAndRef];
				}
			}

			public PaymentCalculationInfo this[string paymentRef]
			{
				get
				{
					try
					{
						return _PaymentInfoList[paymentRef];
					}
					catch (KeyNotFoundException)
					{
						throw new PXException(Messages.CalculationEngineError, paymentRef);
					}
				}
			}

			public void Add(PRPayment key)
			{
				_PaymentInfoList[key.PaymentDocAndRef] = new PaymentCalculationInfo(key);
			}

			public IEnumerable<PRPayment> GetAllPayments()
			{
				return _PaymentInfoList.Values.Select(x => x.Payment);
			}

			public void UpdatePayment(PRPayment payment)
			{
				this[payment].Payment = payment;
			}

			public IEnumerator<PaymentCalculationInfo> GetEnumerator()
			{
				return _PaymentInfoList.Values.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _PaymentInfoList.Values.GetEnumerator();
			}
		}
	}
}
