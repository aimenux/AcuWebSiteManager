using PX.Data;
using System;

namespace PX.Objects.PR
{
	public class DeductCodeSourceAttribute : DeductionSourceListAttribute, IPXFieldUpdatingSubscriber
	{
		private Type _IsWorkersCompensationField;
		private Type _IsCertifiedProjectField;
		private Type _IsUnionField;

		public DeductCodeSourceAttribute(Type isWorkersCompensationField, Type isCertifiedProjectField, Type isUnionField)
		{
			_IsWorkersCompensationField = isWorkersCompensationField;
			_IsCertifiedProjectField = isCertifiedProjectField;
			_IsUnionField = isUnionField;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			if (GetBool(sender, _IsWorkersCompensationField, e.Row))
			{
				e.ReturnValue = WorkCode;
			}
			else if (GetBool(sender, _IsCertifiedProjectField, e.Row))
			{
				e.ReturnValue = CertifiedProject;
			}
			else if (GetBool(sender, _IsUnionField, e.Row))
			{
				e.ReturnValue = Union;
			}
			else
			{
				e.ReturnValue = EmployeeSettings;
			}
		}

		public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			sender.SetValueExt(e.Row, _IsWorkersCompensationField.Name, WorkCode.Equals(e.NewValue));
			sender.SetValueExt(e.Row, _IsCertifiedProjectField.Name, CertifiedProject.Equals(e.NewValue));
			sender.SetValueExt(e.Row, _IsUnionField.Name, Union.Equals(e.NewValue));
		}

		private bool GetBool(PXCache sender, Type field, object row)
		{
			return true.Equals(sender.GetValue(row, field.Name));
		}
	}
}
