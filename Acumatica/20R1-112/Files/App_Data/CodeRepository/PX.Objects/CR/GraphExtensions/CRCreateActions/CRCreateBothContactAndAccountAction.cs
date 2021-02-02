using System;
using System.Collections;
using PX.Data;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	public abstract class CRCreateBothContactAndAccountAction<TGraph, TMaster, TAccountExt, TContactExt>
		: PXGraphExtension<TGraph>
		where TGraph : PXGraph, new()
		where TAccountExt : CRCreateAccountAction<TGraph, TMaster>
		where TContactExt : CRCreateContactAction<TGraph, TMaster>
		where TMaster : class, IBqlTable, new()
	{
		public TAccountExt AccountExt { get; private set; }
		public TContactExt ContactExt { get; private set; }

		public override void Initialize()
		{
			base.Initialize();

			AccountExt = Base.GetExtension<TAccountExt>()
				?? throw new PXException(Messages.GraphHaveNoExt, typeof(TAccountExt).Name);
			ContactExt = Base.GetExtension<TContactExt>()
				?? throw new PXException(Messages.GraphHaveNoExt, typeof(TContactExt).Name);
		}

		public PXAction<TMaster> CreateBothContactAndAccount;
		[PXUIField(DisplayName = Messages.CreateAccount, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable createBothContactAndAccount(PXAdapter adapter)
		{
			var existingContact = ContactExt.ExistingContact.SelectSingle();
			var existingAccount = AccountExt.ExistingAccount.SelectSingle();

			if (existingContact?.BAccountID != null && existingAccount == null)
			{
				AccountExt.Documents.Cache.SetValue<Document.bAccountID>(AccountExt.Documents.Current, existingContact.BAccountID);

				Base.Caches<TMaster>().Update(AccountExt.GetMainCurrent());

				throw new PXSetPropertyException(Messages.AccountAlreadyExists);
			}

			if (AccountExt.AskExtConvert(out bool redirect, ContactExt.PopupValidator))
			{
				var processingGraph = Base.Clone();

				PXLongOperation.StartOperation(Base, () =>
				{
					var extension = processingGraph.GetProcessingExtension<CRCreateBothContactAndAccountAction<TGraph, TMaster, TAccountExt, TContactExt>>();

					extension.DoConvert(redirect);
				});
			}
			return adapter.Get();
		}

		public virtual void DoConvert(bool redirect)
		{
			ConversionResult<BAccount> result;

			using (var ts = new PXTransactionScope())
			{
				result = AccountExt.Convert(new AccountConversionOptions
				{
					HoldCurrentsCallback = ContactExt.HoldCurrents
				});

				var contact = ContactExt.Convert(new ContactConversionOptions
				{
					GraphWithRelation = result.Graph
				});

				if (AccountExt.Documents.Current.RefContactID == null)
					throw new PXException(Messages.CannotCreateAccount);

				ts.Complete();
			}

			if(redirect)
				AccountExt.Redirect(result);
		}

		public virtual void _(Events.RowSelected<TMaster> e)
		{
			CreateBothContactAndAccount.SetEnabled(AccountExt.CreateBAccount.GetEnabled());
		}
	}
}
