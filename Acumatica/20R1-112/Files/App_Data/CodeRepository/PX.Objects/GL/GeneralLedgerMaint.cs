using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using PX.Api;
using PX.Data.RichTextEdit;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CS.DAC;
using PX.Objects.GL.DAC;
using PX.SM;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;

namespace PX.Objects.GL
{
	[PXPrimaryGraph(
		new Type[] { typeof(GeneralLedgerMaint) },
		new Type[] { typeof(Select<Ledger,
			Where<Ledger.ledgerID, Equal<Current<Ledger.ledgerID>>>>)
		})]
	public class GeneralLedgerMaint : PXGraph<GeneralLedgerMaint, Ledger>
	{
		public static void RedirectTo(int? ledgerID)
		{
			var ledgerMaint = CreateInstance<GeneralLedgerMaint>();

			if (ledgerID != null)
			{
				ledgerMaint.LedgerRecords.Current = ledgerMaint.LedgerRecords.Search<Ledger.ledgerID>(ledgerID);
			}

			throw new PXRedirectRequiredException(ledgerMaint, true, string.Empty)
			{
				Mode = PXBaseRedirectException.WindowMode.NewWindow
			};
		}

		public static Ledger FindLedgerByID(PXGraph graph, int? ledgerID, bool isReadonly = true)
		{
			if (isReadonly)
			{
				return PXSelectReadonly<Ledger,
						Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>
					.Select(graph, ledgerID);
			}
			else
			{
				return PXSelect<Ledger,
						Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>
					.Select(graph, ledgerID);
			}
		}

		#region Graph Extensions

		public class OrganizationLedgerLinkMaint : OrganizationLedgerLinkMaintBase<GeneralLedgerMaint, Ledger>
		{
			protected Dictionary<int?, Ledger> LedgerIDMap = new Dictionary<int?, Ledger>();

			public PXAction<Ledger> ViewOrganization;

			public PXSelectJoin<OrganizationLedgerLink,
									LeftJoin<Organization,
										On<OrganizationLedgerLink.organizationID, Equal<Organization.organizationID>>>,
									Where<OrganizationLedgerLink.ledgerID, Equal<Current<Ledger.ledgerID>>>>
									OrganizationLedgerLinkWithOrganizationSelect;

			public override PXSelectBase<OrganizationLedgerLink> OrganizationLedgerLinkSelect => OrganizationLedgerLinkWithOrganizationSelect;

			public override PXSelectBase<Organization> OrganizationViewBase => Base.OrganizationView;

			public override PXSelectBase<Ledger> LedgerViewBase => Base.LedgerRecords;

			protected override Organization GetUpdatingOrganization(int? organizationID)
			{
				return Base.OrganizationView.Search<Organization.organizationID>(organizationID);
			}

			protected override Type VisibleField => typeof(OrganizationLedgerLink.organizationID);

			[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
			[PXButton]
			public virtual IEnumerable viewOrganization(PXAdapter adapter)
			{
				OrganizationLedgerLink link = OrganizationLedgerLinkSelect.Current;

				if (link != null)
				{
					OrganizationMaint.RedirectTo(link.OrganizationID);
				}

				return adapter.Get();
			}

			public virtual void Organization_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
			{
				e.Cancel = true;
			}

			public virtual void Ledger_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
			{
				var ledger = e.Row as Ledger;

				if (ledger == null)
					return;

				if (ledger.LedgerID < 0)
				{
					LedgerIDMap[ledger.LedgerID] = ledger;
				}
			}

			public virtual void OnPersist(IEnumerable<Organization> organizations)
			{
				OrganizationMaint organizationMaint = CreateInstance<OrganizationMaint>();

				PXCache organizationCache = organizationMaint.OrganizationView.Cache;

				PXDBTimestampAttribute timestampAttribute = organizationCache
					.GetAttributesOfType<PXDBTimestampAttribute>(null, nameof(Organization.tstamp))
					.First();

				timestampAttribute.RecordComesFirst = true;

				foreach (Organization organization in organizations)
				{
					organizationMaint.Clear();

					organizationMaint.BAccount.Current = organizationMaint.BAccount.Search<OrganizationBAccount.bAccountID>(organization.BAccountID);

					organizationCache.Clear();
					organizationCache.ClearQueryCacheObsolete();

					if (organization.ActualLedgerID < 0)
					{
						organization.ActualLedgerID = LedgerIDMap[organization.ActualLedgerID].LedgerID;
					}

					organizationCache.Current = organization;
					organizationCache.SetStatus(organizationMaint.OrganizationView.Current, PXEntryStatus.Updated);

					organizationMaint.Actions.PressSave();
				}
			}
		}
		#endregion

		[PXImport(typeof(Ledger))]
		public PXSelect<Ledger> LedgerRecords;
		public PXSetup<Company> company;

		public PXSelectReadonly2<Branch,
									InnerJoin<Organization,
										On<Branch.organizationID, Equal<Organization.organizationID>>,
									InnerJoin<OrganizationLedgerLink,
										On<Organization.organizationID, Equal<OrganizationLedgerLink.organizationID>>>>,
									Where<OrganizationLedgerLink.ledgerID, Equal<Current<Ledger.ledgerID>>>>
									BranchesView;

		public PXSelect<Organization> OrganizationView;

		public GeneralLedgerMaint()
		{
			var mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();

			PXUIFieldAttribute.SetVisible<Ledger.baseCuryID>(LedgerRecords.Cache, null, mcFeatureInstalled);

			BranchesView.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.branch>();
		}

		protected virtual void Ledger_BaseCuryID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Ledger ledger = e.Row as Ledger;
			if (ledger != null && ledger.LedgerID != null && ledger.BaseCuryID != null)
			{
				if (GLUtility.IsLedgerHistoryExist(this, (int)ledger.LedgerID))
				{
					throw new PXSetPropertyException(Messages.CantChangeField, "BaseCuryID");
				}

				if (ledger.BalanceType == LedgerBalanceType.Actual && company.Current.BaseCuryID != (string)e.NewValue)
				{
					throw new PXSetPropertyException(Messages.ActualLedgerInBaseCurrency, ledger.LedgerCD, company.Current.BaseCuryID);
				}
			}
		}

		protected virtual void Ledger_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			var ledger = e.Row as Ledger;

			if (ledger == null)
				return;

			CanBeLedgerDeleted(ledger);
		}

		protected virtual void Ledger_BalanceType_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Ledger ledger = e.Row as Ledger;

			string newBalanceType = (string) e.NewValue;

			if (ledger != null && ledger.LedgerID != null && ledger.CreatedByID != null)
			{
				if (GLUtility.IsLedgerHistoryExist(this, (int)ledger.LedgerID))
				{
					throw new PXSetPropertyException(Messages.CantChangeField, "BalanceType");
				}

				if (newBalanceType == LedgerBalanceType.Actual)
				{
					if (company.Current.BaseCuryID != ledger.BaseCuryID)
					{
						throw new PXSetPropertyException(Messages.ActualLedgerInBaseCurrency, 
							ledger.LedgerCD,
							company.Current.BaseCuryID);
					}

					CanBeLedgerSetAsActual(ledger, GetExtension<OrganizationLedgerLinkMaint>());

					IEnumerable<Organization> organizations = 
										PXSelectJoin<Organization,
													InnerJoin<OrganizationLedgerLink,
														On<Organization.organizationID, Equal<OrganizationLedgerLink.organizationID>>>,
													Where<OrganizationLedgerLink.ledgerID, Equal<Required<OrganizationLedgerLink.ledgerID>>>>
													.Select(this, ledger.LedgerID)
													.RowCast<Organization>();

					foreach (Organization organization in organizations)
					{
						organization.ActualLedgerID = ledger.LedgerID;

						OrganizationView.Cache.SmartSetStatus(organization, PXEntryStatus.Updated);
					}										
				}
			}
		}

		protected virtual void Ledger_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Ledger ledger = e.Row as Ledger;

			if (ledger?.LedgerID != null)
			{
				bool hasHistory = GLUtility.IsLedgerHistoryExist(this, ledger.LedgerID);

				PXUIFieldAttribute.SetEnabled<Ledger.balanceType>(LedgerRecords.Cache, ledger, !hasHistory);

				bool canChangeCurrency = ledger.BalanceType != LedgerBalanceType.Actual && !hasHistory &&
				                         PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();

				PXUIFieldAttribute.SetEnabled<Ledger.baseCuryID>(LedgerRecords.Cache, ledger, canChangeCurrency);
			}
		}

		public override void Persist()
		{
			OrganizationLedgerLinkMaint linkMaint = GetExtension<OrganizationLedgerLinkMaint>();

			foreach (Ledger ledger in LedgerRecords.Cache.Deleted)
			{
				CanBeLedgerDeleted(ledger);
			}

			foreach (Ledger ledger in LedgerRecords.Cache.Updated)
			{
				string origBalanceType = LedgerRecords.Cache.GetValueOriginal<Ledger.balanceType>(ledger) as string;

				if (origBalanceType != ledger.BalanceType)
				{
					GLTran existingReleasedTran = PXSelectReadonly<GLTran,
															Where<GLTran.ledgerID, Equal<Required<GLTran.ledgerID>>,
																	And<GLTran.released, Equal<True>>>>
															.SelectSingleBound(this, null, ledger.LedgerID);

					if (existingReleasedTran != null)
					{
						throw new PXException(Messages.TheTypeOfTheLedgerCannotBeChangedBecauseAtLeastOneReleasedGLTransactionExists, ledger.LedgerCD);
					}

					if (ledger.BalanceType == LedgerBalanceType.Actual)
					{
						CanBeLedgerSetAsActual(ledger, linkMaint);
					}

					if (origBalanceType == LedgerBalanceType.Actual)
					{
						SetActualLedgerIDNullInRelatedCompanies(ledger, linkMaint);
					}
				}
			}

			Organization[] organizations = OrganizationView.Cache.Updated.Cast<Organization>()
																.Select(PXCache<Organization>.CreateCopy).ToArray();

			using (var tranScope = new PXTransactionScope())
			{
				base.Persist();

				OrganizationView.Cache.Clear();

				linkMaint.OnPersist(organizations);

				tranScope.Complete();
			}
		}

		protected virtual void CanBeLedgerSetAsActual(Ledger ledger, OrganizationLedgerLinkMaint linkMaint)
		{
			linkMaint.CheckActualLedgerCanBeAssigned(ledger, GetLinkedOrganizationIDs(ledger).ToArray());
		}

		private void SetActualLedgerIDNullInRelatedCompanies(Ledger ledger, OrganizationLedgerLinkMaint linkMaint)
		{
			linkMaint.SetActualLedgerIDNullInRelatedCompanies(ledger, GetOrganizationIDsWithActualLedger(ledger).ToArray());
		}

		private IEnumerable<int?> GetOrganizationIDsWithActualLedger(Ledger ledger)
		{
			return PXSelect<Organization, Where<Organization.actualLedgerID, Equal<Required<Organization.actualLedgerID>>>>
				.Select(this, ledger.LedgerID)
				.RowCast<Organization>()
				.Select(l => l.OrganizationID);
		}

		private IEnumerable<int?> GetLinkedOrganizationIDs(Ledger ledger)
		{
			return PXSelect<OrganizationLedgerLink,
																	Where<OrganizationLedgerLink.ledgerID, Equal<Required<OrganizationLedgerLink.ledgerID>>>>
																	.Select(this, ledger.LedgerID)
				.RowCast<OrganizationLedgerLink>()
				.Select(l => l.OrganizationID);
		}

		protected virtual void CanBeLedgerDeleted(Ledger ledger)
		{
			CheckLinksToOrganizationsOnDelete(ledger);

			Batch existingBatch = PXSelectReadonly<Batch,
					Where<Batch.ledgerID, Equal<Required<Batch.ledgerID>>>>
				.SelectSingleBound(this, null, ledger.LedgerID);

			if (existingBatch != null)
			{
				throw new PXException(Messages.TheLedgerCannotBeDeletedBecauseAtLeastOneGeneralLedgerBatchExists, ledger.LedgerCD);
			}

			GLTran existingTran = PXSelectReadonly<GLTran,
					Where<GLTran.ledgerID, Equal<Required<GLTran.ledgerID>>>>
				.SelectSingleBound(this, null, ledger.LedgerID);

			if (existingTran != null)
			{
				throw new PXException(Messages.TheLedgerCannotBeDeletedBecauseAtLeastOneGeneralLedgerTransactionHasBeenReleased, ledger.LedgerCD);
			}
		}

		// TODO: Rework to RIC on Delete engine after many-to-many messages fix
		protected virtual void CheckLinksToOrganizationsOnDelete(Ledger ledger)
		{
			Organization[] organizations = PXSelectJoin<OrganizationLedgerLink,
											InnerJoin<Organization,
												On<Organization.organizationID, Equal<OrganizationLedgerLink.organizationID>>>,
											Where<OrganizationLedgerLink.ledgerID, Equal<Required<OrganizationLedgerLink.ledgerID>>>>
											.Select(this, ledger.LedgerID).AsEnumerable()
											.Cast<PXResult<OrganizationLedgerLink, Organization>>()
											.Select(row => (Organization)row)
											.ToArray();

			if (organizations.Any())
			{
				throw new PXException(Messages.LedgerCannotBeDeletedBecauseCompanyOrCompaniesAreAssociated,
					ledger.LedgerCD.Trim(),
					organizations.Select(l => l.OrganizationCD.Trim()).ToArray().JoinIntoStringForMessage());
			}
		}
	}
}
