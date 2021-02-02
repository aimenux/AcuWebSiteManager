using PX.Data;

namespace PX.Objects.IN
{
	public class INAvailabilitySchemeMaint : PXGraph<INAvailabilitySchemeMaint, INAvailabilityScheme>
	{
		public PXSelect<INAvailabilityScheme> Schemes;

		protected virtual void INAvailabilityScheme_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var itemClass = (INItemClass)PXSelectReadonly<INItemClass, 
				Where<INItemClass.availabilitySchemeID, Equal<Current<INAvailabilityScheme.availabilitySchemeID>>>>.SelectWindowed(this, 0, 1);
			if (itemClass != null)
			{
				throw new PXException(Messages.NotPossibleDeleteINAvailScheme);
			}
		}
	}
}
