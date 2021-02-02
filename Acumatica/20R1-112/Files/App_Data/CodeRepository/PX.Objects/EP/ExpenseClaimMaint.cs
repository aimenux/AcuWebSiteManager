using System;
using System.Collections;
using PX.Data;
using PX.TM;
using PX.Data.EP;

namespace PX.Objects.EP
{
	public class ExpenseClaimMaint : PXGraph<ExpenseClaimMaint>
	{
		public class ExpenseClaimMaintReceiptExt : ExpenseClaimDetailGraphExtBase<ExpenseClaimMaint>
		{
			public override PXSelectBase<EPExpenseClaimDetails> Receipts => Base.Details;

			public override PXSelectBase<EPExpenseClaim> Claim => Base.Claim;
		}

		[PXSelector(typeof(PX.SM.Users.pKID), SubstituteKey = typeof(PX.SM.Users.fullName), DescriptionField = typeof(PX.SM.Users.fullName), CacheGlobal = true)]
		[PXDBGuid(false)]
		[PXUIField(DisplayName = "Created By", Enabled = false)]
		protected virtual void EPExpenseClaim_CreatedByID_CacheAttached(PXCache sender) { }

		#region Select

		public PXFilter<ExpenseClaimFilter> Filter;
		[PXFilterable]
		public PXSelectJoin<EPExpenseClaim,
							LeftJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPExpenseClaim.employeeID>>>,
								Where<Current2<ExpenseClaimFilter.employeeID>, IsNotNull,
									And<EPExpenseClaim.employeeID, Equal<Current2<ExpenseClaimFilter.employeeID>>,
									Or<Current2<ExpenseClaimFilter.employeeID>, IsNull,
									And<Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
									Or<EPExpenseClaim.createdByID, Equal<Current<AccessInfo.userID>>,
									Or<EPExpenseClaim.employeeID, WingmanUser<Current<AccessInfo.userID>>,
									Or<EPEmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
									Or<EPExpenseClaim.noteID, Approver<Current<AccessInfo.userID>>>>>>>>>>>,
								OrderBy<Desc<EPExpenseClaim.refNbr>>> Claim;
		public PXSelect<EPExpenseClaimDetails, Where<EPExpenseClaimDetails.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>> Details;
		#endregion

		public ExpenseClaimMaint()
		{
			Claim.View.IsReadOnly = true;
			PXUIFieldAttribute.SetVisible<EPExpenseClaim.branchID>(Claim.Cache, null, false);
		}

		#region Action

		public PXSave<EPExpenseClaim> Save;

		public PXAction<ExpenseClaimFilter> createNew;
		[PXInsertButton]
		[PXUIField(DisplayName = "")]
		[PXEntryScreenRights(typeof(EPExpenseClaim), nameof(ExpenseClaimEntry.Insert))]
		protected virtual void CreateNew()
		{
			using (new PXPreserveScope())
			{
				ExpenseClaimEntry graph = (ExpenseClaimEntry)PXGraph.CreateInstance(typeof(ExpenseClaimEntry));
				graph.Clear(PXClearOption.ClearAll);
				EPExpenseClaim claim = (EPExpenseClaim)graph.ExpenseClaim.Cache.CreateInstance();
				graph.ExpenseClaim.Insert(claim);
				graph.ExpenseClaim.Cache.IsDirty = false;
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
			}
		}

		public PXAction<ExpenseClaimFilter> EditDetail;
		[PXEditDetailButton]
		[PXUIField(DisplayName = "")]
		protected virtual void editDetail()
		{
			if (Claim.Current == null) return;
			EPExpenseClaim row = PXSelect<EPExpenseClaim, Where<EPExpenseClaim.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>>.SelectSingleBound(this, null, Claim.Current.RefNbr);
			PXRedirectHelper.TryRedirect(this, row, PXRedirectHelper.WindowMode.InlineWindow);
		}

		public PXAction<ExpenseClaimFilter> delete;
		[PXDeleteButton]
		[PXUIField(DisplayName = "")]
		[PXEntryScreenRights(typeof(EPExpenseClaim))]
		protected void Delete()
		{
			if (Claim.Current == null) return;

			if (Claim.Current.Released == true)
				throw new PXException(Messages.ReleasedDocumentMayNotBeDeleted);

			Claim.Delete(Claim.Current);
			Save.Press();
		}

		public PXAction<ExpenseClaimFilter> submit;
		[PXButton]
		[PXUIField(DisplayName = Messages.Submit)]
		[PXEntryScreenRights(typeof(EPExpenseClaim))]
		protected void Submit()
		{
			if (Claim.Current != null)
			{
				ExpenseClaimEntry graph = (ExpenseClaimEntry)PXGraph.CreateInstance(typeof(ExpenseClaimEntry));
				graph.Clear(PXClearOption.ClearAll);
				graph.ExpenseClaim.Current = graph.ExpenseClaim.Search<EPExpenseClaim.refNbr>(Claim.Current.RefNbr);
				var a = new PXAdapter(graph.ExpenseClaim);
				a.SortColumns = new string[] { typeof(EPExpenseClaim.refNbr).Name };
				a.Searches = new object[] { Claim.Current.RefNbr };
				a.Menu = Messages.Submit;
				a.MaximumRows = 1;
				foreach (var r in graph.action.Press(a))
				{
				}
			}
		}
		#endregion

		[Serializable]
		[PXHidden]
		public partial class ExpenseClaimFilter : IBqlTable
		{
			private int? _employeeId;

			#region EmployeeID
			public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

			[PXDBInt]
			[PXUIField(DisplayName = "Employee")]
			[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXSubordinateAndWingmenSelector()]
			[PXFieldDescription]
			public virtual Int32? EmployeeID
			{
				get { return _employeeId; }
				set { _employeeId = value; }
			}

			#endregion
		}

		protected virtual void EPExpenseClaimDetails_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			EPExpenseClaimDetails detail = e.Row as EPExpenseClaimDetails;
			if (detail != null)
			{
				FindImplementation<ExpenseClaimMaintReceiptExt>().RemoveReceipt(Details.Cache, detail);
				e.Cancel = true;
			}
		}
	}
}