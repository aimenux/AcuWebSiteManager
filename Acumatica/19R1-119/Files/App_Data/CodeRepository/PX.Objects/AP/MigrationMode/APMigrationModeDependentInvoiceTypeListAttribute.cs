using PX.Data;

namespace PX.Objects.AP.MigrationMode
{
	public class APMigrationModeDependentInvoiceTypeListAttribute : APInvoiceType.ListAttribute
	{
		public APMigrationModeDependentInvoiceTypeListAttribute()
			: base()
		{ }

		public override void CacheAttached(PXCache sender)
		{
			APSetup setup = 
				sender.Graph.Caches[typeof(APSetup)].Current as APSetup
				?? PXSelect<APSetup>.SelectWindowed(sender.Graph, 0, 1);

            if (setup == null || setup.MigrationMode != true)
            {
                base.CacheAttached(sender);
                return;
            }

			_AllowedValues = new[] { APDocType.Invoice, APDocType.DebitAdj, APDocType.CreditAdj };
			_AllowedLabels = new[] { Messages.Invoice, Messages.DebitAdj, Messages.CreditAdj };
			_NeutralAllowedLabels = new[] { Messages.Invoice, Messages.DebitAdj, Messages.CreditAdj };

			base.CacheAttached(sender);
		}
	}
}
