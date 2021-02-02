using System;
using PX.Data;
using PX.Objects.CA;


namespace PX.Objects.GL
{
	[PXDBString(10, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class CashAccountRawAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "CASHACCOUNT";

		public CashAccountRawAttribute()
		{
			Type searchType = typeof(Search2<CashAccount.cashAccountCD,
								   InnerJoin<
											 Account, On<Account.accountID, Equal<CashAccount.accountID>,
											 And2<
												  Match<Account, Current<AccessInfo.userName>>,
											  And<Match<Account, Current<AccessInfo.branchID>>>>>,
								   InnerJoin<
											 Sub, On<Sub.subID, Equal<CashAccount.subID>,
											 And<Match<Sub, Current<AccessInfo.userName>>>>>>>);

			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(_DimensionName, searchType)
			{
				CacheGlobal = true,
				DescriptionField = typeof(CashAccount.descr)
			};

			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}
}
