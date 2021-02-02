using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	/// <summary>
	/// Provides a selector for the <see cref="Terms"> items, which may be put into the <see cref="APInvoice"> document <br/>
	/// The list is filtered by the visible rights <see cref="TermsVisibleTo.All"> and <see cref="TermsVisibleTo.Vendor"> <br/>
	/// and restricted by multiple installement type <see cref="TermsInstallmentType.Multiple"> 
	/// if the AP migration mode is activated <see cref="APSetup.MigrationMode">. <br/>
	/// <example>
	/// [APTermsSelector]
	/// </example>
	/// </summary>
	[PXSelector(typeof(Search<Terms.termsID, 
		Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, 
			Or<Terms.visibleTo, Equal<TermsVisibleTo.vendor>>>>), 
		DescriptionField = typeof(Terms.descr), Filterable = true)]
	[PXRestrictor(typeof(Where<
		Current<APSetup.migrationMode>, NotEqual<True>,
		Or<Terms.installmentType, NotEqual<TermsInstallmentType.multiple>>>), CS.Messages.CannotBeEmpty)]
	public class APTermsSelectorAttribute : PXAggregateAttribute
	{
		public APTermsSelectorAttribute()
			: base()
		{
		}
	}
}