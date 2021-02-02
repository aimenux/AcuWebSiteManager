using System;
using System.Collections;
using PX.Data;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	public class CRCreateOpportunityAllAction<TGraph, TMaster, TOpportunityExt, TAccountExt, TContactExt>
		: PXGraphExtension<TGraph>
		where TGraph : PXGraph, new()
		where TOpportunityExt : CRCreateOpportunityAction<TGraph, TMaster>
		where TAccountExt : CRCreateAccountAction<TGraph, TMaster>
		where TContactExt : CRCreateContactAction<TGraph, TMaster>
		where TMaster : class, IBqlTable, new()
	{
		public TOpportunityExt OpportunityExt { get; private set; }
		public TAccountExt AccountExt { get; private set; }
		public TContactExt ContactExt { get; private set; }


		public override void Initialize()
		{
			base.Initialize();

			OpportunityExt = Base.GetExtension<TOpportunityExt>()
				?? throw new PXException(Messages.GraphHaveNoExt, typeof(TOpportunityExt).Name);
			AccountExt = Base.GetExtension<TAccountExt>()
				?? throw new PXException(Messages.GraphHaveNoExt, typeof(TAccountExt).Name);
			ContactExt = Base.GetExtension<TContactExt>()
				?? throw new PXException(Messages.GraphHaveNoExt, typeof(TContactExt).Name);
		}

		public PXAction<TMaster> ConvertToOpportunityAll;
		[PXUIField(DisplayName = Messages.CreateOpportunity, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable convertToOpportunityAll(PXAdapter adapter)
		{
			var existingContact = ContactExt.ExistingContact.SelectSingle();
			var existingAccount = AccountExt.ExistingAccount.SelectSingle();

			if (existingContact?.BAccountID != null && existingAccount == null)
			{
				AccountExt.Documents.Cache.SetValue<Document.bAccountID>(AccountExt.Documents.Current, existingContact.BAccountID);

				Base.Caches<TMaster>().Update(AccountExt.GetMainCurrent());
			}

			if (OpportunityExt.AskExtConvert(out bool redirect,
				ContactExt.PopupValidator, AccountExt.PopupValidator))
			{
				var processingGraph = Base.Clone();

				PXLongOperation.StartOperation(Base, () =>
				{
					var extension = processingGraph.GetProcessingExtension<CRCreateOpportunityAllAction<TGraph, TMaster, TOpportunityExt, TAccountExt, TContactExt>>();

					extension.DoConvert(redirect);
				});
			}

			return adapter.Get();
		}

		public virtual void DoConvert(bool redirect)
		{
			ConversionResult<CROpportunity> result;

			using (var ts = new PXTransactionScope())
			{
				// to save original value because it would be changed during contact/account creation
				var overrideContact =
					OpportunityExt.Documents.Cache.GetValueOriginal<Document.overrideRefContact>(OpportunityExt.Documents.Current) as bool?;

				var account = AccountExt.Convert(new AccountConversionOptions
				{
					HoldCurrentsCallback = AccountExt.JoinHoldCurrents(ContactExt.HoldCurrents, OpportunityExt.HoldCurrents)
				});

				var contact = ContactExt.Convert(new ContactConversionOptions
				{
					HoldCurrentsCallback = OpportunityExt.HoldCurrents,
					GraphWithRelation = account.Graph
				});

				result = OpportunityExt.Convert(new OpportunityConversionOptions
				{
					ForceOverrideContact = overrideContact,
				});

				if (OpportunityExt.Documents.Current.RefContactID == null)
					throw new PXException(Messages.CannotCreateOpportunity);

				ts.Complete();
			}

			if(redirect)
				OpportunityExt.Redirect(result);
		}

	}
}
