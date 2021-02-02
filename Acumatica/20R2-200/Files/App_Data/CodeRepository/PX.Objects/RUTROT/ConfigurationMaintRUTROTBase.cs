using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.RUTROT.DAC;

namespace PX.Objects.RUTROT
{
	[Serializable]
	public abstract class ConfigurationMaintRUTROTBase<TGraph> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>();
		}

		/// <summary>
		/// Default mapping
		/// </summary>
		protected class RUTROTConfigurationHolderMapping : IBqlMapping
		{
			/// <exclude />
			public Type Extension => typeof(RUTROTConfigurationHolder);
			/// <exclude />
			protected Type _table;
			/// <exclude />
			public Type Table => _table;

			/// <exclude />
			public RUTROTConfigurationHolderMapping(Type table)
			{
				_table = table;
			}

			/// <exclude />
			public Type AllowsRUTROT = typeof(RUTROTConfigurationHolder.allowsRUTROT);
			/// <exclude />
			public Type RUTDeductionPct = typeof(RUTROTConfigurationHolder.rUTDeductionPct);
			/// <exclude />
			public Type RUTPersonalAllowanceLimit = typeof(RUTROTConfigurationHolder.rUTPersonalAllowanceLimit);
			/// <exclude />
			public Type RUTExtraAllowanceLimit = typeof(RUTROTConfigurationHolder.rUTExtraAllowanceLimit);
			/// <exclude />
			public Type ROTDeductionPct = typeof(RUTROTConfigurationHolder.rOTDeductionPct);
			/// <exclude />
			public Type ROTPersonalAllowanceLimit = typeof(RUTROTConfigurationHolder.rOTPersonalAllowanceLimit);
			/// <exclude />
			public Type ROTExtraAllowanceLimit = typeof(RUTROTConfigurationHolder.rOTExtraAllowanceLimit);
			/// <exclude />
			public Type RUTROTCuryID = typeof(RUTROTConfigurationHolder.rUTROTCuryID);
			/// <exclude />
			public Type RUTROTClaimNextRefNbr = typeof(RUTROTConfigurationHolder.rUTROTClaimNextRefNbr);
			/// <exclude />
			public Type RUTROTOrgNbrValidRegEx = typeof(RUTROTConfigurationHolder.rUTROTOrgNbrValidRegEx);
			/// <exclude />
			public Type DefaultRUTROTType = typeof(RUTROTConfigurationHolder.defaultRUTROTType);
			/// <exclude />
			public Type TaxAgencyAccountID = typeof(RUTROTConfigurationHolder.taxAgencyAccountID);
			/// <exclude />
			public Type BalanceOnProcess = typeof(RUTROTConfigurationHolder.balanceOnProcess);
		}

		protected abstract RUTROTConfigurationHolderMapping GetDocumentMapping();

		#region Events Handlers
		public virtual void RUTROTConfigurationHolder_AllowsRUTROT_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null)
				return;

			var configurationHolder = PXCache<RUTROTConfigurationHolder>.CreateCopy((RUTROTConfigurationHolder)e.Row);

			if (configurationHolder.AllowsRUTROT == true)
			{
				configurationHolder.RUTROTCuryID = ((Company)PXSelect<Company>.Select(Base))?.BaseCuryID;
				configurationHolder.RUTPersonalAllowanceLimit = 25000.0m;
				configurationHolder.RUTExtraAllowanceLimit = 50000.0m;
				configurationHolder.RUTDeductionPct = 50.0m;

				configurationHolder.ROTPersonalAllowanceLimit = 50000.0m;
				configurationHolder.ROTExtraAllowanceLimit = 50000.0m;
				configurationHolder.ROTDeductionPct = 30.0m;
				configurationHolder.RUTROTOrgNbrValidRegEx = "^(\\d{10})$";
				configurationHolder.DefaultRUTROTType = RUTROTTypes.RUT;
			}
			else
			{
				configurationHolder.ROTDeductionPct = 0.0m;
				configurationHolder.ROTPersonalAllowanceLimit = 0.0m;
				configurationHolder.ROTExtraAllowanceLimit = 0.0m;
				configurationHolder.RUTDeductionPct = 0.0m;
				configurationHolder.RUTPersonalAllowanceLimit = 0.0m;
				configurationHolder.RUTExtraAllowanceLimit = 0.0m;
				configurationHolder.RUTROTCuryID = null;
			}

			(sender as PXModelExtension<RUTROTConfigurationHolder>)?.UpdateExtensionMapping(configurationHolder);
		}

		public virtual void RUTROTConfigurationHolder_RUTROTClaimNextRefNbr_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			var configurationHolder = (RUTROTConfigurationHolder)e.Row;

			if (configurationHolder == null
				|| PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>() == false
				|| configurationHolder.AllowsRUTROT != true)
			{
				return;
			}

			int newValue = (int?)e.NewValue ?? 0;
			int oldValue = configurationHolder.RUTROTClaimNextRefNbr ?? 0;

			if (newValue < oldValue)
			{
				PXUIFieldAttribute.SetWarning<RUTROTConfigurationHolder.rUTROTClaimNextRefNbr>(sender, configurationHolder, RUTROTMessages.ClaimNextRefDecreased);
			}
		}

		public virtual void RUTROTConfigurationHolder_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;

			UpdateRUTROTControlsState(sender, e.Row as RUTROTConfigurationHolder);
		}

		private void UpdateRUTROTControlsState(PXCache sender, RUTROTConfigurationHolder rutrotConfigurationHolder)
		{
			bool showRUTROTFields = rutrotConfigurationHolder.AllowsRUTROT == true;

			PXUIFieldAttribute.SetEnabled<RUTROTConfigurationHolder.rUTROTCuryID>(sender, rutrotConfigurationHolder, showRUTROTFields);
			PXUIFieldAttribute.SetEnabled<RUTROTConfigurationHolder.rOTPersonalAllowanceLimit>(sender, rutrotConfigurationHolder, showRUTROTFields);
			PXUIFieldAttribute.SetEnabled<RUTROTConfigurationHolder.rOTExtraAllowanceLimit>(sender, rutrotConfigurationHolder, showRUTROTFields);
			PXUIFieldAttribute.SetEnabled<RUTROTConfigurationHolder.rOTDeductionPct>(sender, rutrotConfigurationHolder, showRUTROTFields);
			PXUIFieldAttribute.SetEnabled<RUTROTConfigurationHolder.rUTPersonalAllowanceLimit>(sender, rutrotConfigurationHolder, showRUTROTFields);
			PXUIFieldAttribute.SetEnabled<RUTROTConfigurationHolder.rUTExtraAllowanceLimit>(sender, rutrotConfigurationHolder, showRUTROTFields);
			PXUIFieldAttribute.SetEnabled<RUTROTConfigurationHolder.rUTDeductionPct>(sender, rutrotConfigurationHolder, showRUTROTFields);
			PXUIFieldAttribute.SetEnabled<RUTROTConfigurationHolder.rUTROTClaimNextRefNbr>(sender, rutrotConfigurationHolder, showRUTROTFields);
			PXUIFieldAttribute.SetEnabled<RUTROTConfigurationHolder.defaultRUTROTType>(sender, rutrotConfigurationHolder, showRUTROTFields);
			PXUIFieldAttribute.SetEnabled<RUTROTConfigurationHolder.taxAgencyAccountID>(sender, rutrotConfigurationHolder, showRUTROTFields);
			PXUIFieldAttribute.SetEnabled<RUTROTConfigurationHolder.balanceOnProcess>(sender, rutrotConfigurationHolder, showRUTROTFields);

			var persistingCheck = showRUTROTFields ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;

			PXDefaultAttribute.SetPersistingCheck<RUTROTConfigurationHolder.rUTROTCuryID>(sender, rutrotConfigurationHolder, persistingCheck);
			PXDefaultAttribute.SetPersistingCheck<RUTROTConfigurationHolder.rOTPersonalAllowanceLimit>(sender, rutrotConfigurationHolder, persistingCheck);
			PXDefaultAttribute.SetPersistingCheck<RUTROTConfigurationHolder.rOTExtraAllowanceLimit>(sender, rutrotConfigurationHolder, persistingCheck);
			PXDefaultAttribute.SetPersistingCheck<RUTROTConfigurationHolder.rOTDeductionPct>(sender, rutrotConfigurationHolder, persistingCheck);
			PXDefaultAttribute.SetPersistingCheck<RUTROTConfigurationHolder.rUTPersonalAllowanceLimit>(sender, rutrotConfigurationHolder, persistingCheck);
			PXDefaultAttribute.SetPersistingCheck<RUTROTConfigurationHolder.rUTExtraAllowanceLimit>(sender, rutrotConfigurationHolder, persistingCheck);
			PXDefaultAttribute.SetPersistingCheck<RUTROTConfigurationHolder.rUTDeductionPct>(sender, rutrotConfigurationHolder, persistingCheck);
			PXDefaultAttribute.SetPersistingCheck<RUTROTConfigurationHolder.rUTROTCuryID>(sender, rutrotConfigurationHolder, persistingCheck);
			PXDefaultAttribute.SetPersistingCheck<RUTROTConfigurationHolder.defaultRUTROTType>(sender, rutrotConfigurationHolder, persistingCheck);
			PXDefaultAttribute.SetPersistingCheck<RUTROTConfigurationHolder.taxAgencyAccountID>(sender, rutrotConfigurationHolder, persistingCheck);
			PXDefaultAttribute.SetPersistingCheck<RUTROTConfigurationHolder.balanceOnProcess>(sender, rutrotConfigurationHolder, persistingCheck);
		}
		#endregion
	}
}
