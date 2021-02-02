using System;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.AP.CacheExtensions;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote.ComplianceDocumentEntityStrategies
{
    public class ApInvoiceStrategy : ComplianceDocumentEntityStrategy
    {
        public ApInvoiceStrategy()
        {
            EntityType = typeof(APInvoice);
            FilterExpression = typeof(Where<APInvoice.docType, Equal<APDocType.invoice>,
                Or<APInvoice.docType, Equal<APDocType.creditAdj>,
                    Or<APInvoice.docType, Equal<APDocType.debitAdj>,
                        Or<APInvoice.docType, Equal<APDocType.prepayment>>>>>);
            TypeField = typeof(APInvoice.docType);
            TypeFilterValues = Activator.CreateInstance<APInvoiceType.ListAttribute>().ValueLabelDic.Select(x => x.Key)
                .ToArray();
            TypeFilterLabels = Activator.CreateInstance<APInvoiceType.ListAttribute>().ValueLabelDic
                .Select(x => x.Value).ToArray();
        }

        public override Guid? GetNoteId(PXGraph graph, string clDisplayName)
        {
            var key = ComplianceReferenceTypeHelper.ConvertToDocumentKey<APInvoice>(clDisplayName);

            var noteId = new PXSelect<APInvoice,
                Where<APInvoice.docType, Equal<Required<APInvoice.docType>>,
                And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>(graph)
                .Select(key.DocType, key.RefNbr)
                .FirstTableItems
                .ToList()
                .SingleOrDefault()
                ?.NoteID;

            return noteId;
        }
    }
}