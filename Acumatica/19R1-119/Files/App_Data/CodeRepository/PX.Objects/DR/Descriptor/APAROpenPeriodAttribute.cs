using System;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.DR
{
	public class APAROpenPeriodAttribute : OpenPeriodAttribute
	{
		public class OrigModulePh : BqlPlaceholderBase { }
		private readonly Type _origModuteField;
		public PXErrorLevel errorLevel { get; set; } = PXErrorLevel.Error;
		#region Ctor
		public APAROpenPeriodAttribute(Type origModule,
			Type sourceType,
			Type branchSourceType,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type useMasterCalendarSourceType = null,
			Type defaultType = null,
			bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
			Type masterFinPeriodIDType = null,
			bool redefaultOnDateChanged = true)
			: base(BqlTemplate.OfCommand<
					Search<FinPeriod.finPeriodID,
					Where<FinPeriod.status, Equal<FinPeriod.status.open>,
						And<Where2<
								Where<Current<OrigModulePh>, Equal<BatchModule.moduleAP>, And<FinPeriod.aPClosed, Equal<False>>>,
							Or2<
								Where<Current<OrigModulePh>, Equal<BatchModule.moduleAR>, And<FinPeriod.aRClosed, Equal<False>>>,
							Or<
								Current<OrigModulePh>, IsNull>>>>>>>
				  .Replace<OrigModulePh>(origModule).ToType(),
					sourceType,
				branchSourceType: branchSourceType,
				branchSourceFormulaType: branchSourceFormulaType,
				organizationSourceType: organizationSourceType,
				useMasterCalendarSourceType: useMasterCalendarSourceType,
				defaultType: defaultType,
				redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
				masterFinPeriodIDType: masterFinPeriodIDType,
				redefaultOnDateChanged: redefaultOnDateChanged)
		{
			_origModuteField = origModule;
		}
		#endregion
		#region Implementation
		public static void VerifyPeriod<Field>(PXCache cache, object row)
			where Field : IBqlField
		{
			foreach (APAROpenPeriodAttribute attr in cache.GetAttributesReadonly<Field>(row).OfType<APAROpenPeriodAttribute>())
			{
				attr.IsValidPeriod(cache, row, cache.GetValue<Field>(row));
			}
		}

		protected override PeriodValidationResult ValidateOrganizationFinPeriodStatus(PXCache sender, object row, FinPeriod finPeriod)
		{
			PeriodValidationResult result = base.ValidateOrganizationFinPeriodStatus(sender, row, finPeriod);

			if (!result.HasWarningOrError)
			{
				var origModule = (string)sender.GetValue(row, _origModuteField.Name);

				if (origModule == BatchModule.AP && finPeriod.APClosed == true ||
					origModule == BatchModule.AR && finPeriod.ARClosed == true)
				{
					result = HandleErrorThatPeriodIsClosed(sender, finPeriod);
				}
			}

			return result;
		}
		#endregion
	}
}
