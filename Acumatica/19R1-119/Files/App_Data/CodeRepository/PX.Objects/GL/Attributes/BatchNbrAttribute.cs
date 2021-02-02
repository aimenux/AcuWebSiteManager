using System;

using PX.Data;

namespace PX.Objects.GL
{
	public class BatchNbrAttribute : PXSelectorAttribute
	{
		public virtual Type IsMigratedRecordField
		{
			get;
			set;
		}

		public BatchNbrAttribute(Type searchType)
			: base(searchType)
		{ }

		private bool IsMigratedRecord(PXCache cache, object data)
		{
			string fieldName = cache.GetField(IsMigratedRecordField);
			return (cache.GetValue(data, fieldName) as bool?) == true;
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (IsMigratedRecord(sender, e.Row))
			{
				e.Cancel = true;
			}
			else
			{
				base.FieldVerifying(sender, e);
			}
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			if (IsMigratedRecord(sender, e.Row))
			{
				e.ReturnValue = Common.Messages.MigrationModeBatchNbr;
			}
		}
	}

	public class BatchNbrExtAttribute : BatchNbrAttribute
	{
		public BatchNbrExtAttribute(Type searchType)
			: base(searchType)
		{ }
		protected override bool IsReadDeletedSupported => false;
	}
}