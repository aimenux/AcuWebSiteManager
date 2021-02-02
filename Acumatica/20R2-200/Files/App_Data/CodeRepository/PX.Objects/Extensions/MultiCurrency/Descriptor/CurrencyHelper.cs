using System;
using PX.Data;
using PX.Objects.CM.Extensions;

namespace PX.Objects.Extensions.MultiCurrency
{
	public interface IPXCurrencyHelper
	{
		decimal RoundCury(decimal val);
		void CuryConvBase(decimal curyval, out decimal baseval);
		void CuryConvBase(CurrencyInfo info, decimal curyval, out decimal baseval);
		void CuryConvCury(decimal baseval, out decimal curyval);
		void CuryConvCury(CurrencyInfo info, decimal baseval, out decimal curyval);
		PXView GetView(Type table, Type field);
		string GetCuryID(long? curyInfoID);
		string GetBaseCuryID(long? curyInfoID);
	}
}
