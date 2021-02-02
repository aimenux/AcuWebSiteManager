using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.AP.Services;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.CalculationServices;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.InvoiceEntry
{
	public class ApInvoiceEntryAdjustJointAmountExtension : PXGraphExtension<ApInvoiceEntryExt, APInvoiceEntry>
	{
		public PXAction<APInvoice> AdjustJointAmounts;

		private IEnumerable<JointPayee> JointPayees => Base1.JointPayees.Select().FirstTableItems;

		private APInvoiceJCExt InvoiceExtension =>
			PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(Base.Document.Current);

		private bool IsAdjustingInProgress => InvoiceExtension?.IsAdjustingJointAmountsInProgress == true;

		public JointPayeeAmountsCalculationService JointPayeeAmountsCalculationService;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.construction>() &&
				!SiteMapExtension.IsTaxBillsAndAdjustmentsScreenId();
		}

		public override void Initialize()
		{
			JointPayeeAmountsCalculationService = new JointPayeeAmountsCalculationService(Base);

			base.Initialize();
		}

		[PXUIField(DisplayName = "Adjust Joint Amounts",
			MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual void adjustJointAmounts()
		{
			SetOriginalValuesForJoinPayees();
			InvoiceExtension.IsAdjustingJointAmountsInProgress = true;
		}

		[PXOverride]
		public virtual void Persist(Action baseHandler)
		{
			baseHandler();
			if (Base.Document.Current != null)
			{
				InvoiceExtension.IsAdjustingJointAmountsInProgress = false;
			}
		}

		public virtual void _(Events.RowSelected<APInvoice> args)
		{
			if (args.Row == null)
			{
				return;
			}
			AdjustJointAmounts.SetEnabled(IsAdjustJointAmountActionAvailable());
			Base1.JointPayees.AllowUpdate = Base1.JointPayees.AllowUpdate || IsAdjustingInProgress;
		}

		public virtual void _(Events.RowSelected<JointPayee> args)
		{
			var jointPayee = args.Row;
			if (jointPayee == null)
			{
				return;
			}
			ModifyJointPayeeFieldsAvailability(jointPayee);
		}

		[Obsolete(PX.Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2020R2)]
		public virtual void _(Events.RowPersisting<APInvoice> args)
		{

		}

		private void ModifyJointPayeeFieldsAvailability(JointPayee jointPayee)
		{
			SetJointPayeeFieldEnabled<JointPayee.jointPayeeInternalId>(jointPayee, !IsAdjustingInProgress);
			SetJointPayeeFieldEnabled<JointPayee.jointPayeeExternalName>(jointPayee, !IsAdjustingInProgress);
			SetJointPayeeFieldEnabled<JointPayee.billLineNumber>(jointPayee, !IsAdjustingInProgress);
		}

		private void SetJointPayeeFieldEnabled<TField>(JointPayee jointPayee, bool isEnabled)
			where TField : IBqlField
		{
			PXUIFieldAttribute.SetEnabled<TField>(Base1.JointPayees.Cache, jointPayee, isEnabled);
		}

		private bool IsAdjustJointAmountActionAvailable()
		{
			APInvoice document = Base.Document.Current;

			var isOpenStatus = document?.Status == APDocStatus.Open;
			return document?.IsRetainageDocument != true && isOpenStatus &&
				   !IsAdjustingInProgress;
		}

		private void SetOriginalValuesForJoinPayees()
		{
			foreach (var jointPayee in JointPayees)
			{
				jointPayee.OriginalJointAmountOwed = jointPayee.JointAmountOwed;
				jointPayee.OriginalJointPreparedBalance = JointPayeeAmountsCalculationService.GetJointPreparedBalance(jointPayee);
				jointPayee.OriginalJointBalance = jointPayee.JointBalance;
			}
		}
	}
}