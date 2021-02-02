using System;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.AR.CacheExtensions;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote.ComplianceDocumentEntityStrategies
{
    public class ArPaymentStrategy : ComplianceDocumentEntityStrategy
    {
        public ArPaymentStrategy()
        {
            EntityType = typeof(ARPayment);
            FilterExpression = typeof(Where<ARPayment.docType, Equal<ARDocType.payment>,
                Or<ARPayment.docType, Equal<ARDocType.creditMemo>,
                    Or<ARPayment.docType, Equal<ARDocType.prepayment>,
                        Or<ARPayment.docType, Equal<ARDocType.refund>,
                            Or<ARPayment.docType, Equal<ARDocType.voidPayment>,
                                Or<ARPayment.docType, Equal<ARDocType.smallBalanceWO>,
                                    Or<ARPayment.docType, Equal<ARDocType.voidRefund>
                                    >>>>>>>);
            TypeField = typeof(ARPayment.docType);
            TypeFilterValues = Activator.CreateInstance<ARPaymentType.ListAttribute>().ValueLabelDic
                .Select(x => x.Key).ToArray();
            TypeFilterLabels = Activator.CreateInstance<ARPaymentType.ListAttribute>().ValueLabelDic
                .Select(x => x.Value).ToArray();
        }

        public override Guid? GetNoteId(PXGraph graph, string clDisplayName)
        {
            var key = ComplianceReferenceTypeHelper.ConvertToDocumentKey<ARPayment>(clDisplayName);

            var noteId = new PXSelect<ARPayment,
                Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
                And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>(graph)
                .Select(key.DocType, key.RefNbr)
                .FirstTableItems
                .ToList()
                .SingleOrDefault()
                ?.NoteID;

            return noteId;
        }
    }
}
