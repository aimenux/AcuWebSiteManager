using System;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Compliance.PO.CacheExtensions;
using PX.Objects.PO;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote.ComplianceDocumentEntityStrategies
{
    public class PoOrderStrategy : ComplianceDocumentEntityStrategy
    {
        public PoOrderStrategy()
        {
            EntityType = typeof(POOrder);
            FilterExpression = typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>);
            TypeField = typeof(POOrder.orderType);
            TypeFilterValues = Activator.CreateInstance<POOrderType.ListAttribute>().ValueLabelDic.Select(x => x.Key)
                .ToArray();
            TypeFilterLabels = Activator.CreateInstance<POOrderType.ListAttribute>().ValueLabelDic.Select(x => x.Value)
                .ToArray();
        }

        public override Guid? GetNoteId(PXGraph graph, string clDisplayName)
        {
            var key = ComplianceReferenceTypeHelper.ConvertToDocumentKey<POOrder>(clDisplayName);

            var noteId = new PXSelect<POOrder,
                Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
                And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>,
                And<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>>>>(graph)
                .Select(key.DocType, key.RefNbr)
                .FirstTableItems
                .ToList()
                .SingleOrDefault()
                ?.NoteID;

            return noteId;
        }
    }
}