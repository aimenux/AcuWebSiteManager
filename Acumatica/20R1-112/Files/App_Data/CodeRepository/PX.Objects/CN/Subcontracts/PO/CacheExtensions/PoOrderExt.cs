using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.CN.Subcontracts.PO.Descriptor;
using PX.Objects.CN.Subcontracts.PO.Descriptor.Attributes;
using PX.Objects.CN.Subcontracts.SC.Descriptor.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.PO.CacheExtensions
{
    public sealed class PoOrderExt : PXCacheExtension<POOrder>
    {
        [CRAttributesField(typeof(subcontractClassID), typeof(POOrder.noteID))]
        public string[] Attributes
        {
            get;
            set;
        }

        [PXString(20)]
        public string SubcontractClassID => Constants.SubcontractClassId;

        [ToWords(typeof(POOrder.curyOrderTotal))]
        public string OrderTotalInWords
        {
            get;
            set;
        }

        /// <summary>
        /// Used only for attribute.
        /// </summary>
        [PXBool]
        [UploadFileNameCorrectorForSubcontracts]
        public bool? UploadFileNameCorrectorStub
        {
            get;
            set;
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXNote(new Type[0],
            ShowInReferenceSelector = true, Selector = typeof(Search<POOrder.noteID,
                Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>>))]
        [PXRemoveBaseAttribute(typeof(PXSearchableAttribute))]
        [SubcontractSearchable]
        public Guid? NoteID
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class orderTotalInWords : IBqlField
        {
        }

        public abstract class subcontractClassID : IBqlField
        {
        }

        public abstract class attributes : IBqlField
        {
        }

        public abstract class uploadFileNameCorrectorStub : IBqlField
        {
        }

        public class subcontractClass : BqlString.Constant<subcontractClass>
        {
            public subcontractClass()
                : base(Constants.SubcontractClassId)
            {
            }
        }

        public class pOOrderExtTypeName : BqlString.Constant<pOOrderExtTypeName>
        {
            public pOOrderExtTypeName()
                : base(typeof(PoOrderExt).FullName)
            {
            }
        }
    }
}