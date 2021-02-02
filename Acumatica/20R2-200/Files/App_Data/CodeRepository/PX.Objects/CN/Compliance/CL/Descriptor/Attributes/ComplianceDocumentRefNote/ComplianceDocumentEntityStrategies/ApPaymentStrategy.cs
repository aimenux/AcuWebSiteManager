using System;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.AP.CacheExtensions;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote.ComplianceDocumentEntityStrategies
{
    public class ApPaymentStrategy : ComplianceDocumentEntityStrategy
    {
        public ApPaymentStrategy()
        {
            EntityType = typeof(APPayment);
            FilterExpression = typeof(Where<APPayment.docType, Equal<APDocType.check>,
                Or<APPayment.docType, Equal<APDocType.debitAdj>,
                    Or<APPayment.docType, Equal<APDocType.prepayment>,
                        Or<APPayment.docType, Equal<APDocType.refund>,
                            Or<APPayment.docType, Equal<APDocType.voidCheck>,
                                Or<APPayment.docType, Equal<APDocType.voidRefund>>>>>>>);
            TypeField = typeof(APPayment.docType);
            TypeFilterValues = Activator.CreateInstance<APPaymentType.ListAttribute>().ValueLabelDic
                .Select(x => x.Key).ToArray();
            TypeFilterLabels = Activator.CreateInstance<APPaymentType.ListAttribute>().ValueLabelDic
                .Select(x => x.Value).ToArray();
        }

        public override Guid? GetNoteId(PXGraph graph, string clDisplayName)
        {
            var key = ComplianceReferenceTypeHelper.ConvertToDocumentKey<APPayment>(clDisplayName);

            var noteId = new PXSelect<APPayment,
                Where<APPayment.docType, Equal<Required<APPayment.docType>>,
                And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>(graph)
                .Select(key.DocType, key.RefNbr)
                .FirstTableItems
                .ToList()
                .SingleOrDefault()
                ?.NoteID;

            return noteId;
        }
    }
}
