using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	/// <summary>
	/// Provides a selector for the <see cref="Terms"> items, which may be put into the <see cref="ARInvoice"> document <br/>
	/// The list is filtered by the visible rights <see cref="TermsVisibleTo.All"> and <see cref="TermsVisibleTo.Customer"> <br/>
	/// and restricted by multiple installement type <see cref="TermsInstallmentType.Multiple"> 
	/// if the AR migration mode is activated <see cref="ARSetup.MigrationMode">. <br/>
	/// <example>
	/// [ARTermsSelector]
	/// </example>
	/// </summary>
	[PXSelector(typeof(Search<Terms.termsID, 
		Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, 
			Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), 
		DescriptionField = typeof(Terms.descr), Filterable = true)]
	[PXRestrictor(typeof(Where<
		Current<ARSetup.migrationMode>, NotEqual<True>,
		Or<Terms.installmentType, NotEqual<TermsInstallmentType.multiple>>>), CS.Messages.CannotBeEmpty)]
	public class ARTermsSelectorAttribute : PXAggregateAttribute
	{
		public ARTermsSelectorAttribute()
			: base()
		{
		}
	}
}