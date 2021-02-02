using System;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.AR.CacheExtensions;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote.ComplianceDocumentEntityStrategies
{
    public class ArInvoiceStrategy : ComplianceDocumentEntityStrategy
    {
        public ArInvoiceStrategy()
        {
            EntityType = typeof(ARInvoice);
            FilterExpression = typeof(Where<ARInvoice.docType, Equal<ARDocType.invoice>,
                Or<ARInvoice.docType, Equal<ARDocType.debitMemo>,
                    Or<ARInvoice.docType, Equal<ARDocType.creditMemo>,
                        Or<ARInvoice.docType, Equal<ARDocType.finCharge>,
                            Or<ARInvoice.docType, Equal<ARDocType.smallCreditWO>
                            >>>>>);
            TypeField = typeof(ARInvoice.docType);
            TypeFilterValues = Activator.CreateInstance<ARInvoiceType.ListAttribute>().ValueLabelDic.Select(x => x.Key)
                .ToArray();
            TypeFilterLabels = Activator.CreateInstance<ARInvoiceType.ListAttribute>().ValueLabelDic
                .Select(x => x.Value).ToArray();
        }

        public override Guid? GetNoteId(PXGraph graph, string clDisplayName)
        {
            var key = ComplianceReferenceTypeHelper.ConvertToDocumentKey<ARInvoice>(clDisplayName);

            var noteId = new PXSelect<ARInvoice,
                Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
                And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>(graph)
                .Select(key.DocType, key.RefNbr)
                .FirstTableItems
                .ToList()
                .SingleOrDefault()
                ?.NoteID;

            return noteId;
        }
    }
}