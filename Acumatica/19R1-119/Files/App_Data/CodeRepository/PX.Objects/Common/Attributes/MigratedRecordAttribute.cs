using System;

using PX.Data;

namespace PX.Objects.Common.MigrationMode
{
	/// <summary>
	/// Attribute that sets <c>true</c> on the underlying field defaulting 
	/// if migration mode is activated in the specified setup field, and
	/// <c>false</c> otherwise.
	/// </summary>
	[PXDBBool]
	[PXDefault]
	public class MigratedRecordAttribute : PXAggregateAttribute, IPXFieldDefaultingSubscriber
	{
		public virtual Type MigrationModeSetupField
		{
			get;
			private set;
		}

		public virtual bool IsMigrationModeEnabled
		{
			get;
			private set;
		}

		public MigratedRecordAttribute(Type migrationModeSetupField)
			: base()
		{
			MigrationModeSetupField = migrationModeSetupField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			BqlCommand search = (BqlCommand)Activator.CreateInstance(BqlCommand.Compose(typeof(Search<>), MigrationModeSetupField));
			PXView view = new PXView(sender.Graph, true, BqlCommand.CreateInstance(search.GetSelectType()));
			object setup = view.SelectSingle();

			IsMigrationModeEnabled = (view.Cache.GetValue(setup, MigrationModeSetupField.Name) as bool?) == true;
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = IsMigrationModeEnabled;
		}
	}
}
