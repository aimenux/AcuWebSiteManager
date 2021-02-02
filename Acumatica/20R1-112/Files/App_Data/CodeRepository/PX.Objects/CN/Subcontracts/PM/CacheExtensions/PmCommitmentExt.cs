using System;
using PX.Data;
using PX.Objects.CN.Subcontracts.PM.Descriptor.Attributes;
using PX.Objects.CS;
using PX.Objects.PM;
using Messages = PX.Objects.CN.Subcontracts.PM.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PM.CacheExtensions
{
    public sealed class PmCommitmentExt : PXCacheExtension<PMCommitment>
    {
        [PXString]
        [PXUIField(DisplayName = Messages.PmCommitment.RelatedDocumentType, Visible = false, Enabled = false,
            Visibility = PXUIVisibility.SelectorVisible)]
        [PXStringList(new[]
        {
            Messages.PmCommitment.PurchaseOrderType,
            Messages.PmCommitment.SalesOrderType,
            Messages.PmCommitment.SubcontractType
        }, new[]
        {
            Messages.PmCommitment.PurchaseOrderLabel,
            Messages.PmCommitment.SalesOrderLabel,
            Messages.PmCommitment.SubcontractLabel
        })]
        public string RelatedDocumentType
        {
            get;
            set;
        }

        [PXRemoveBaseAttribute(typeof(PMCommitment.PXRefNoteAttribute))]
        [CommitmentRefNote]
        public Guid? RefNoteID
        {
            get;
            set;
        }

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.construction>();
		}

		public abstract class relatedDocumentType : IBqlField
        {
        }
    }
}