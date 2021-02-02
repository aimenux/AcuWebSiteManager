using PX.Common;
using PX.Data;
using System;

namespace PX.Objects.PR
{
	public class HoursForCertifiedProjectAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXRowSelectedSubscriber, IPXFieldSelectingSubscriber
	{
		private Type _HoursPerYearField;
		private Type _OverrideHoursPerYearForCertifiedField;
		private Type _HoursPerYearForCertifiedUseDfltField;

		public HoursForCertifiedProjectAttribute(Type hoursPerYearField, Type overrideHoursPerYearForCertifiedField, Type hoursPerYearForCertifiedUseDfltField = null)
		{
			_HoursPerYearField = hoursPerYearField;
			_OverrideHoursPerYearForCertifiedField = overrideHoursPerYearForCertifiedField;
			_HoursPerYearForCertifiedUseDfltField = hoursPerYearForCertifiedUseDfltField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _OverrideHoursPerYearForCertifiedField.Name, OverrideHoursPerYearForCertifiedFieldUpdated);
		}

		public void OverrideHoursPerYearForCertifiedFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			bool useOverrideForCertified = GetBoolValue(sender, e.Row, _OverrideHoursPerYearForCertifiedField.Name);

			if (useOverrideForCertified &&
				(_HoursPerYearForCertifiedUseDfltField == null || false.Equals(sender.GetValue(e.Row, _HoursPerYearForCertifiedUseDfltField.Name))))
			{
				decimal? hoursPerYear = GetValueExt(sender, e.Row, _HoursPerYearField.Name) as decimal?;
				int roundedHoursPerYear = (int)Math.Round(hoursPerYear.GetValueOrDefault());
				sender.SetValue(e.Row, _FieldName, roundedHoursPerYear);
			}
			else if (!useOverrideForCertified)
			{
				sender.SetValue(e.Row, _FieldName, null);
			}
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			bool useOverrideForCertified = GetBoolValue(sender, e.Row, _OverrideHoursPerYearForCertifiedField.Name);
			if (!useOverrideForCertified && _HoursPerYearForCertifiedUseDfltField != null)
			{
				sender.SetValue(e.Row, _HoursPerYearForCertifiedUseDfltField.Name, true);
			}

			if (useOverrideForCertified && GetValueExt(sender, e.Row, _FieldName) == null)
			{
				sender.RaiseExceptionHandling(
					_FieldName,
					e.Row,
					null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, _FieldName));
			}
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (_HoursPerYearForCertifiedUseDfltField == null)
			{
				return;
			}

			bool useOverrideForCertified = GetBoolValue(sender, e.Row, _OverrideHoursPerYearForCertifiedField.Name);
			PXUIFieldAttribute.SetVisible(sender, e.Row, _HoursPerYearForCertifiedUseDfltField.Name, useOverrideForCertified);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, _HoursPerYearForCertifiedUseDfltField.Name, useOverrideForCertified);
		}

		private object GetValueExt(PXCache sender, object row, string fieldName)
		{
			object valueExt = sender.GetValueExt(row, fieldName);
			return valueExt is PXFieldState ? ((PXFieldState)valueExt).Value : valueExt;
		}

		private bool GetBoolValue(PXCache sender, object row, string fieldName)
		{
			return GetValueExt(sender, row, fieldName)?.Equals(true) == true;
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXFieldState fieldState = e.ReturnState as PXFieldState;
			if (fieldState == null)
			{
				fieldState = PXIntState.CreateInstance(
					sender.GetValue(e.Row, _FieldName),
					_FieldName,
					false,
					null,
					0,
					null,
					null,
					null,
					typeof(int),
					null);
			}

			bool useOverrideForCertified = GetBoolValue(sender, e.Row, _OverrideHoursPerYearForCertifiedField.Name);
			bool enable = true;

			if (_HoursPerYearForCertifiedUseDfltField != null)
			{
				enable = false.Equals(sender.GetValue(e.Row, _HoursPerYearForCertifiedUseDfltField.Name));
			}

			fieldState.Visible = useOverrideForCertified;
			fieldState.Enabled = useOverrideForCertified && enable;
			fieldState.Required = useOverrideForCertified && enable;
			e.ReturnState = fieldState;
		}
	}
}
