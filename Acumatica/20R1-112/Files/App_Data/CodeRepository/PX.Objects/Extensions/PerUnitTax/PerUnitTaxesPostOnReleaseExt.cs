using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using PX.Data;
using PX.Common;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.TX;

namespace PX.Objects.Extensions.PerUnitTax
{
	/// <summary>
	/// A per unit taxes post on AP/AR release graph extension base class.
	/// </summary>
	public abstract class PerUnitTaxesPostOnReleaseExt<TReleaseGraph, TDocument, TLine, TLineTax, TAggregatedTax> : PXGraphExtension<TReleaseGraph>
		where TReleaseGraph : PXGraph<TReleaseGraph>
		where TDocument : class, IBqlTable, new()
		where TLine : class, IBqlTable, new()
		where TLineTax : TaxDetail, IBqlTable
		where TAggregatedTax : TaxTran, new()
	{
		protected static bool IsActiveBase() => PXAccess.FeatureInstalled<FeaturesSet.perUnitTaxSupport>();

		protected virtual void AggregatedTax_RowPersisting(Events.RowPersisting<TAggregatedTax> e)
		{
			Tax tax = GetTax(e.Row);

			if (tax == null)
				return;

			bool areAccountAndSubaccountRequired = tax.TaxType != CSTaxType.PerUnit || tax.PerUnitTaxPostMode == PerUnitTaxPostOptions.TaxAccount;
			PXPersistingCheck persistingCheck = areAccountAndSubaccountRequired
				? PXPersistingCheck.NullOrBlank
				: PXPersistingCheck.Nothing;

			e.Cache.Adjust<PXDefaultAttribute>(e.Row)
				   .For<TaxTran.accountID>(a => a.PersistingCheck = persistingCheck)
				   .SameFor<TaxTran.subID>();
		}

		/// <summary>
		/// Gets a tax from <see cref="TaxDetail"/>.
		/// </summary>
		/// <param name="taxDetail">The taxDetail to act on.</param>
		/// <returns/>
		private Tax GetTax(TaxDetail taxDetail)
		{
			if (taxDetail == null)
				return null;

			return PXSelect<Tax,
					  Where<Tax.taxID, Equal<Required<Tax.taxID>>>>
				  .SelectSingleBound(Base, currents: null, pars: taxDetail.TaxID);
		}

		[PXOverride]
		public void PostPerUnitTaxAmounts(JournalEntry journalEntry, TDocument document, CurrencyInfo newCurrencyInfo,
										  TAggregatedTax perUnitAggregatedTax, Tax perUnitTax, bool isDebitTaxTran)
		{
			if (!CheckInputDocument(document) || !CheckPerUnitTax(perUnitTax))
			{
				return;
			}
			 
			CreateAndPostGLTransactions(journalEntry, document, newCurrencyInfo, perUnitAggregatedTax, perUnitTax, isDebitTaxTran);
		}

		protected virtual bool CheckInputDocument(TDocument document)
		{
			document.ThrowOnNull(nameof(document));
			return true;
		}

		protected virtual bool CheckPerUnitTax(Tax perUnitTax) => perUnitTax?.TaxCalcType == CSTaxCalcType.Item;

		protected virtual void CreateAndPostGLTransactions(JournalEntry journalEntry, TDocument document, CurrencyInfo newCurrencyInfo,
														   TAggregatedTax perUnitAggregatedTax, Tax perUnitTax, bool isDebitTaxTran)
		{
			switch (perUnitTax.PerUnitTaxPostMode)
			{
				case PerUnitTaxPostOptions.TaxAccount:
					CreateAndPostGLTransactionsOnTaxAccount(journalEntry, document, newCurrencyInfo, perUnitAggregatedTax,
															perUnitTax, isDebitTaxTran);
					return;
				case PerUnitTaxPostOptions.LineAccount:
					CreateAndPostGLTransactionsOnLineAccounts(journalEntry, document, newCurrencyInfo, perUnitAggregatedTax,
															  perUnitTax, isDebitTaxTran);
					return;
			}
		}

		protected abstract void CreateAndPostGLTransactionsOnTaxAccount(JournalEntry journalEntry, TDocument document, CurrencyInfo newCurrencyInfo,
																		TAggregatedTax perUnitAggregatedTax, Tax perUnitTax, bool isDebitTaxTran);

		private void CreateAndPostGLTransactionsOnLineAccounts(JournalEntry journalEntry, TDocument document, CurrencyInfo newCurrencyInfo,
															   TAggregatedTax perUnitAggregatedTax, Tax perUnitTax, bool isDebitTaxTran)
		{
			IEnumerable<(TLineTax, TLine)> linesWithTaxes = GetTaxWithLines(perUnitTax, perUnitAggregatedTax);

			foreach (var (perUnitLineTax, apTran) in linesWithTaxes)
			{
				GLTran newGlTran = CreateGLTranForPerUnitLineTax(document, newCurrencyInfo, perUnitAggregatedTax,
																 apTran, perUnitLineTax, isDebitTaxTran);

				InsertNewGLTran(journalEntry, document, apTran, perUnitAggregatedTax, perUnitLineTax, newGlTran);
			}
		}

		protected abstract IEnumerable<(TLineTax Tax, TLine Line)> GetTaxWithLines(Tax perUnitTax, TAggregatedTax perUnitAggregatedTax);

		protected virtual GLTran CreateGLTranForPerUnitLineTax(TDocument document, CurrencyInfo newCurrencyInfo, TAggregatedTax perUnitAggregatedTax, 
															   TLine docLine, TLineTax perUnitLineTax, bool isDebitTaxTran)
		{
			GLTran newGlTran = new GLTran
			{
				CuryInfoID = newCurrencyInfo.CuryInfoID,
				TranType = perUnitAggregatedTax.TranType,
				TranClass = GLTran.tranClass.Tax,
				RefNbr = perUnitAggregatedTax.RefNbr,
				TranDate = perUnitAggregatedTax.TranDate,
				TranDesc = perUnitLineTax.TaxID,
				Released = true
			};

			if (isDebitTaxTran)
			{
				newGlTran.CuryCreditAmt = 0m;
				newGlTran.CreditAmt = 0m;
			}
			else
			{
				newGlTran.CuryDebitAmt = 0m;
				newGlTran.DebitAmt = 0m;
			}

			return newGlTran;
		}

		protected abstract GLTran InsertNewGLTran(JournalEntry journalEntry, TDocument document, TLine docLine, TAggregatedTax perUnitAggregatedTax,
												  TLineTax perUnitLineTax, GLTran newGlTran);

		/// <summary>
		/// A hack to fill a delegate field of graph extension with appropriate delegate signature with a call to the protected method of the graph.
		/// </summary>
		/// <typeparam name="TDelegateType">Type of the delegate type.</typeparam>
		/// <param name="protectedMethodName">Name of the protected method of the graph to call.</param>
		/// <param name="protectedMethodField">[out] The delegate field of the graph extension.</param>
		protected void FillProtectedMethodDelegate<TDelegateType>(string protectedMethodName, out TDelegateType protectedMethodField)
		where TDelegateType : class
		{
			if (string.IsNullOrWhiteSpace(protectedMethodName))
			{
				protectedMethodField = default;
				return;
			}

			Type delegateType = typeof(TDelegateType);
			Type[] parameterTypes = Array.Empty<Type>();
			
			if (delegateType.IsGenericType)
			{
				parameterTypes = delegateType.GetGenericArguments();

				if (delegateType.Name.StartsWith("Func"))
				{
					parameterTypes = parameterTypes.Take(parameterTypes.Length - 1).ToArray();	//For Func<...> delegates skip the last argument to get parameter types
				}
			}
			else
			{
				MethodInfo method = delegateType.GetMethod("Invoke");
				parameterTypes = method?.GetParameters()
										.Select(parameterInfo => parameterInfo.ParameterType).ToArray() ?? parameterTypes;
			}

			Type graphType = Base.GetType();
			const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
			MethodInfo protectedGraphMethod = 
				graphType.GetMethod(protectedMethodName, bindingFlags, binder: null, types: parameterTypes, modifiers: null) ??
				PX.Api.CustomizedTypeManager.GetTypeNotCustomized(graphType)
										   ?.GetMethod(protectedMethodName, bindingFlags, binder: null, types: parameterTypes, modifiers: null);

			protectedMethodField = protectedGraphMethod?.CreateDelegate(typeof(TDelegateType), Base) as TDelegateType;
		}
	}
}

