using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.BQLConstants;
using PX.Objects.AP;

namespace PX.Objects.AR
{
	[PXHidden()]
	[Obsolete("This graph is not used anymore and will be removed in Acumatica ERP 8.0.")]
	public class ARTempCrLimitMaint : PXGraph<ARTempCrLimitMaint>
	{

		public PXSave<ARTempCrLimitFilter> Save;

		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable Cancel(PXAdapter adapter)
		{
			ARTempCreditLimitRecord.Cache.Clear();
			TimeStamp = null;
			return adapter.Get();
		}

		public PXAction<ARTempCrLimitFilter> cancel;

		public PXFilter<ARTempCrLimitFilter> Filter;


		public PXSelect<ARTempCreditLimit,
			 Where<ARTempCreditLimit.customerID, Equal<Current<ARTempCrLimitFilter.customerID>>>> ARTempCreditLimitRecord;

		protected virtual void ARTempCreditLimit_StartDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			ARTempCreditLimit row = (ARTempCreditLimit)e.Row;
			DateTime? newValue = (DateTime?)e.NewValue;

			if (newValue.HasValue && row.EndDate.HasValue && (newValue > row.EndDate))
			{
				throw new PXSetPropertyException(Messages.TempCrLimitInvalidDate);
			}

			if (newValue.HasValue)
			{
				ARTempCreditLimit arCrL1 = PXSelect<ARTempCreditLimit,
						   		Where<ARTempCreditLimit.customerID, Equal<Required<ARTempCreditLimit.customerID>>,
							  		And<ARTempCreditLimit.lineID, NotEqual<Required<ARTempCreditLimit.lineID>>,
								  	And<ARTempCreditLimit.startDate, LessEqual<Required<ARTempCreditLimit.startDate>>,
										And<ARTempCreditLimit.endDate, GreaterEqual<Required<ARTempCreditLimit.endDate>>>>>>>.
									 Select(this, row.CustomerID, row.LineID, newValue, newValue);

				if ((arCrL1 != null) && (arCrL1.CustomerID != null) && (arCrL1.StartDate != null) && (arCrL1.EndDate != null))
				{
					throw new PXSetPropertyException(Messages.TempCrLimitPeriodsCrossed);
				}

				if (row.EndDate.HasValue)
				{
					ARTempCreditLimit arCrL2 = PXSelect<ARTempCreditLimit,
						Where<ARTempCreditLimit.customerID, Equal<Required<ARTempCreditLimit.customerID>>,
	  					And<ARTempCreditLimit.lineID, NotEqual<Required<ARTempCreditLimit.lineID>>,
            	And<ARTempCreditLimit.startDate, 
							    Between<Required<ARTempCreditLimit.startDate>, Required<ARTempCreditLimit.startDate>>,
							And<ARTempCreditLimit.endDate, 
					        Between<Required<ARTempCreditLimit.endDate>, Required<ARTempCreditLimit.endDate>>>>>>>.
										 Select(this, row.CustomerID, row.LineID, newValue, row.EndDate, newValue, row.EndDate);

					if ((arCrL2 != null) && (arCrL2.CustomerID != null) && (arCrL2.StartDate != null) && (arCrL2.EndDate != null))
					{
						throw new PXSetPropertyException(Messages.TempCrLimitPeriodsCrossed);
					}
				}
			}
		}

		protected virtual void ARTempCreditLimit_EndDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			ARTempCreditLimit row = (ARTempCreditLimit)e.Row;
			DateTime? newValue = (DateTime?)e.NewValue;

			if (newValue.HasValue && row.StartDate.HasValue && (newValue < row.StartDate))
			{
				throw new PXSetPropertyException(Messages.TempCrLimitInvalidDate);
			}

			if (newValue.HasValue)
			{
				ARTempCreditLimit arCrL1 = PXSelect<ARTempCreditLimit,
									Where<ARTempCreditLimit.customerID, Equal<Required<ARTempCreditLimit.customerID>>,
										And<ARTempCreditLimit.lineID, NotEqual<Required<ARTempCreditLimit.lineID>>,
										And<ARTempCreditLimit.startDate, LessEqual<Required<ARTempCreditLimit.startDate>>,
										And<ARTempCreditLimit.endDate, GreaterEqual<Required<ARTempCreditLimit.endDate>>>>>>>.
									 Select(this, row.CustomerID, row.LineID, newValue, newValue);

				if ((arCrL1 != null) && (arCrL1.CustomerID != null) && (arCrL1.StartDate != null) && (arCrL1.EndDate != null))
				{
					throw new PXSetPropertyException(Messages.TempCrLimitPeriodsCrossed);
				}

				if (row.StartDate.HasValue)
				{
					ARTempCreditLimit arCrL2 = PXSelect<ARTempCreditLimit,
						Where<ARTempCreditLimit.customerID, Equal<Required<ARTempCreditLimit.customerID>>,
							And<ARTempCreditLimit.lineID, NotEqual<Required<ARTempCreditLimit.lineID>>,
							And<ARTempCreditLimit.startDate,
									Between<Required<ARTempCreditLimit.startDate>, Required<ARTempCreditLimit.startDate>>,
							And<ARTempCreditLimit.endDate,
									Between<Required<ARTempCreditLimit.endDate>, Required<ARTempCreditLimit.endDate>>>>>>>.
										 Select(this, row.CustomerID, row.LineID, row.StartDate, newValue, row.StartDate, newValue);

					if ((arCrL2 != null) && (arCrL2.CustomerID != null) && (arCrL2.StartDate != null) && (arCrL2.EndDate != null))
					{
						throw new PXSetPropertyException(Messages.TempCrLimitPeriodsCrossed);
					}
				}
			}
		}
	}
}
