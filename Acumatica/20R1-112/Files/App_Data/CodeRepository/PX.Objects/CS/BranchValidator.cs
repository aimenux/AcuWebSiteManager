using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.EP;
using PX.Objects.FA;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.IN;

namespace PX.Objects.CS
{
	public class BranchValidator
	{
		protected int EntityCountInErrorMessage = 10;

		protected readonly PXGraph Graph;

		public BranchValidator(PXGraph graph)
		{
			Graph = graph;
		}

		public virtual void CanBeBranchesDeletedSeparately(IReadOnlyCollection<Branch> branches)
		{
			foreach (var branch in branches)
			{
				Graph.Caches[typeof(Branch)].ClearQueryCacheObsolete();

				Organization organization = OrganizationMaint.FindOrganizationByID(Graph, branch.OrganizationID);

				if (organization != null
				    && organization.OrganizationType == OrganizationTypes.WithoutBranches)
				{
					throw new PXException(Messages.TheBranchCannotBeDeletedBecauseItBelongsToTheCompanyOfTheWithoutBranchesType,
											branch.BranchCD.Trim(),
											organization.OrganizationCD.Trim());
				}
			}

			CanBeBranchesDeleted(branches);
		}

		public virtual void CanBeBranchesDeleted(IReadOnlyCollection<Branch> branches, bool isOrganizationWithoutBranchesDeletion = false)
		{
			int?[] baccountIDs = branches.Select(b => b.BAccountID).ToArray();
			int?[] branchIDs = branches.Select(b => b.BranchID).ToArray();

			using (new PXReadBranchRestrictedScope())
			{
				CheckRelatedCashAccountsDontExist(branchIDs);

				CheckRelatedEmployeesDoNotExist(baccountIDs);

				CheckRelatedGLHistoryDoesNotExist(branchIDs);

				CheckRelatedGLTranDoesNotExist(branchIDs);

				string warehouseMessage = null;
				string fixedAssetMessage = null;

				if (isOrganizationWithoutBranchesDeletion)
				{
					warehouseMessage = GL.Messages.CompanyCannotDeletedBecauseRelatedWarehousesExist;
					fixedAssetMessage = GL.Messages.CompanyCannotDeletedBecauseRelatedFixedAssetsExist;
				}
				else
				{
					warehouseMessage = GL.Messages.BranchCannotDeletedBecauseRelatedWarehousesExist;
					fixedAssetMessage = GL.Messages.BranchCannotDeletedBecauseRelatedFixedAssetsExist;
				}

				CheckRelatedWarehousesDontExist(branchIDs, warehouseMessage);

				CheckRelatedFixedAssetsDontExist(branchIDs, fixedAssetMessage);
			}
		}

		public virtual void CheckRelatedGLTranDoesNotExist(int?[] branchIDs)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			GLTran tran = PXSelectReadonly<GLTran,
											Where<GLTran.branchID, In<Required<GLTran.branchID>>>>
											.SelectSingleBound(Graph, null, branchIDs);

			if (tran != null)
			{
				Branch branch = BranchMaint.FindBranchByID(Graph, tran.BranchID);

				throw new PXException(Messages.TheBranchOrBranchesCannotBeDeletedBecauseTheRelatedTransactionHasBeenPosted, 
										branch.BranchCD.Trim(), 
										tran.ToString());
			}
		}

		public virtual void CheckRelatedCashAccountsDontExist(int?[] branchIDs)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			CA.CashAccount[] cashAccounts = PXSelectReadonly<CA.CashAccount,
															Where<CA.CashAccount.branchID, In<Required<CA.CashAccount.branchID>>,
																And<CA.CashAccount.restrictVisibilityWithBranch, Equal<boolTrue>>>>
															.SelectWindowed(Graph, 0, EntityCountInErrorMessage + 1, branchIDs)
															.RowCast<CA.CashAccount>()
															.ToArray();
			
			if (cashAccounts.Any())
			{
				IEnumerable<Branch> branches = BranchMaint.FindBranchesByID(Graph, branchIDs).ToArray();

				throw new PXException(Messages.CashAccountsForBranch,
										branches.Select(b => b.BranchCD.Trim()).ToArray().JoinIntoStringForMessage(),
										cashAccounts.Select(a => a.CashAccountCD.Trim()).ToArray().JoinIntoStringForMessage(EntityCountInErrorMessage));
			}
		}

		public virtual void CheckRelatedEmployeesDoNotExist(int?[] branchBAccountIDs)
		{
			if (branchBAccountIDs == null || branchBAccountIDs.IsEmpty())
				return;

			EPEmployee[] employees = PXSelectReadonly<EPEmployee,
														Where<EPEmployee.parentBAccountID, In<Required<EPEmployee.parentBAccountID>>>>
														.SelectWindowed(Graph, 0, EntityCountInErrorMessage + 1, branchBAccountIDs)
														.RowCast<EPEmployee>()
														.ToArray();

			if (employees.Any())
			{
				IEnumerable<Branch> branches = PXSelectReadonly<Branch,
																Where<Branch.bAccountID, In<Required<Branch.bAccountID>>>>
																.Select(Graph, employees.Take(EntityCountInErrorMessage).Select(e => e.ParentBAccountID).ToArray())
																.RowCast<Branch>();

				throw new PXException(Messages.TheBranchOrBranchesCannotBeDeletedBecauseTheFollowingEmployeesAreAssigned,
										branches.Select(b => b.BranchCD.Trim()).ToArray().JoinIntoStringForMessage(),
										employees.Select(e => e.AcctCD.Trim()).ToArray().JoinIntoStringForMessage(EntityCountInErrorMessage));
			}
		}

		public virtual void CheckRelatedGLHistoryDoesNotExist(int?[] branchIDs)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			GLHistory history = GLUtility.GetRelatedToBranchGLHistory(Graph, branchIDs);

			if (history != null)
			{
				Branch branch = BranchMaint.FindBranchByID(Graph, history.BranchID);

				if (branch != null)
				{
					throw new PXException(Messages.BranchCanNotBeDeletedBecausePostedGLTransExist,
						branch.BranchCD.Trim());
				}
			}
		}

		public virtual void ValidateActiveField(int?[] branchIDs, bool? newValue, Organization organization) =>
			ValidateActiveField(branchIDs, newValue, organization, false);

		public virtual void ValidateActiveField(int?[] branchIDs, bool? newValue, Organization organization, bool skipActivateValidation = false)
		{
			if (newValue != true)
			{
				using (new PXReadBranchRestrictedScope())
				{
					string warehouseErrorMessaage = null;
					string fixedAssetsErrorMessaage = null;

					if (organization?.OrganizationType == OrganizationTypes.WithoutBranches)
					{
						warehouseErrorMessaage = GL.Messages.CompanyCannotBeSetAsInactiveBecauseRelatedWarehousesExist;
						fixedAssetsErrorMessaage = GL.Messages.CompanyCannotBeSetAsInactiveBecauseRelatedFixedAssetsExist;
					}
					else
					{
						warehouseErrorMessaage = GL.Messages.BranchCannotBeSetAsInactiveBecauseRelatedWarehousesExist;
						fixedAssetsErrorMessaage = GL.Messages.BranchCannotBeSetAsInactiveBecauseRelatedFixedAssetsExist;
					}

					CheckRelatedWarehousesDontExist(branchIDs, warehouseErrorMessaage);

					CheckRelatedFixedAssetsDontExist(branchIDs, fixedAssetsErrorMessaage);
				}
			}

			if (newValue == true && organization != null && organization.Active != true && skipActivateValidation == false)
			{
				throw new PXSetPropertyException(GL.Messages.BranchCannotBeActivatedBecauseItsParentCompanyIsInactive);
			}
		}

		public virtual void CheckRelatedFixedAssetsDontExist(int?[] branchIDs, string exceptionMessage)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			FixedAsset[] assets = PXSelectReadonly<FixedAsset,
													Where<FixedAsset.branchID, In<Required<FixedAsset.branchID>>>>
													.SelectWindowed(Graph, 0, EntityCountInErrorMessage + 1, branchIDs)
													.RowCast<FixedAsset>()
													.ToArray();

			if (assets.Any())
			{
				IEnumerable<Branch> branches =
					BranchMaint.FindBranchesByID(Graph, assets.Take(EntityCountInErrorMessage).Select(a => a.BranchID).ToArray());

				throw new PXSetPropertyException(exceptionMessage,
												branches.Select(b => b.BranchCD.Trim()).ToArray().JoinIntoStringForMessage(),
												assets.Select(s => s.AssetCD.Trim()).ToArray().JoinIntoStringForMessage(EntityCountInErrorMessage));
			}
		}

		public virtual void CheckRelatedWarehousesDontExist(int?[] branchIDs, string exceptionMessage)
		{
			if (branchIDs == null || branchIDs.IsEmpty())
				return;

			INSite[] sites = PXSelectReadonly<INSite,
												Where<INSite.branchID, In<Required<INSite.branchID>>>>
												.SelectWindowed(Graph, 0, EntityCountInErrorMessage + 1, branchIDs)
												.RowCast<INSite>()
												.ToArray();

			if (sites.Any())
			{
				IEnumerable<Branch> branches =
					BranchMaint.FindBranchesByID(Graph, sites.Take(EntityCountInErrorMessage).Select(a => a.BranchID).ToArray());

				throw new PXSetPropertyException(exceptionMessage,
													branches.Select(b => b.BranchCD.Trim()).ToArray().JoinIntoStringForMessage(),
													sites.Select(s => s.SiteCD.Trim()).ToArray().JoinIntoStringForMessage(EntityCountInErrorMessage));
			}
		}
	}
}
