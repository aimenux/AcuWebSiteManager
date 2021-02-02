using PX.Data;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.PR
{
	/// <summary>
	/// Specialized for PR version of the <see cref="OpenPeriodAttribute"/><br/>
	/// Selector. Provides a list  of the active Fin. Periods, having PRClosed flag = false <br/>
	/// <example>
	/// [PROpenPeriod(typeof(PRPayment.paymentDate))]
	/// </example>
	/// </summary>
	public class PROpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor

		/// <summary>
		/// Extended Ctor. 
		/// </summary>
		/// <param name="SourceType">Must be IBqlField. Refers a date, based on which "current" period will be defined</param>
		public PROpenPeriodAttribute(Type SourceType)
			: base(typeof(Search<FinPeriod.finPeriodID, Where<FinPeriod.pRClosed, Equal<False>, And<FinPeriod.status, Equal<FinPeriod.status.open>>>>), SourceType)
		{
		}

		public PROpenPeriodAttribute()
			: this(null)
		{
		}
		#endregion

		#region Implementation

		public static void DefaultFirstOpenPeriod(PXCache sender, string FieldName)
		{
			foreach (PeriodIDAttribute attr in sender.GetAttributesReadonly(FieldName).OfType<PeriodIDAttribute>())
			{
				attr.SearchType = typeof(Search2<FinPeriod.finPeriodID, CrossJoin<GLSetup>, Where<FinPeriod.endDate, Greater<Required<FinPeriod.endDate>>, And<FinPeriod.active, Equal<True>, And<Where<GLSetup.restrictAccessToClosedPeriods, NotEqual<True>, Or<FinPeriod.pRClosed, Equal<False>>>>>>, OrderBy<Asc<FinPeriod.finPeriodID>>>);
			}
		}

		public static void DefaultFirstOpenPeriod<Field>(PXCache sender)
			where Field : IBqlField
		{
			DefaultFirstOpenPeriod(sender, typeof(Field).Name);
		}

		//public override void IsValidPeriod(PXCache sender, object row, object newValue)
		//{
		//    base.IsValidPeriod(sender, row, newValue);

		//    if (newValue == null || _ValidatePeriod == PeriodValidation.Nothing) return;

		//    FinPeriod financialPeriod = PXSelect<
		//        FinPeriod,
		//        Where<
		//            FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>>
		//        .Select(sender.Graph, (string)newValue);

		//    if (financialPeriod.PRClosed != true) return;

		//    if (PostClosedPeriods(sender.Graph))
		//    {
		//        sender.RaiseExceptionHandling(
		//            _FieldName,
		//            row,
		//            null,
		//            new FiscalPeriodClosedException(financialPeriod.FinPeriodID, PXErrorLevel.Warning));
		//    }
		//    else
		//    {
		//        throw new FiscalPeriodClosedException(financialPeriod.FinPeriodID);
		//    }
		//}
		#endregion
	}
}
