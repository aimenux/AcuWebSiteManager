using PX.Data;
using System;

namespace PX.Objects.Extensions
{
	public class PXSetupBase<TSelf, TGraph, THeader, TSetup, Where> : PXGraphExtension<TGraph>
		where TSelf : PXSetupBase<TSelf, TGraph, THeader, TSetup, Where>
		where TGraph : PXGraph
		where THeader : class, IBqlTable, new()
		where TSetup : class, IBqlTable, new()
		where Where : IBqlWhere, new()
	{
		public TSetup UserSetup => EnsureUserSetup();

		public PXSelect<TSetup, Where> UserSetupView;

		public PXAction<THeader> UserSetupDialog;
		[PXButton, PXUIField(DisplayName = "User Settings")]
		protected virtual void userSetupDialog()
		{
			EnsureUserSetup();
			PXCache setupCache = Base.Caches[typeof(TSetup)];
			if (UserSetupView.AskExt() == WebDialogResult.OK)
			{
				using (var tr = new PXTransactionScope())
				{
					setupCache.Persist(PXDBOperation.Insert);
					setupCache.Persist(PXDBOperation.Update);
					tr.Complete();
				}
				Base.Clear();
			}
			else
			{
				setupCache.Clear();
				setupCache.ClearQueryCacheObsolete();
			}
		}

		protected virtual TSetup EnsureUserSetup()
		{
			UserSetupView.Current = UserSetupView.Select();
			if (UserSetupView.Current == null)
				UserSetupView.Insert((TSetup)UserSetupView.Cache.CreateInstance());
			return UserSetupView.Current;
		}

		/// <summary>
		/// Gets a <typeparamref name="TSetup"/> instance for a <typeparamref name="TGraph"/> instance.
		/// </summary>
		public static TSetup For(TGraph graph) => graph.GetExtension<TSelf>().UserSetup;
	}
}
