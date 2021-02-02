using System;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Compliance.PM.CacheExtensions;
using PX.Objects.GL;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote.ComplianceDocumentEntityStrategies
{
    public class PmRegisterStrategy : ComplianceDocumentEntityStrategy
    {
        public PmRegisterStrategy()
        {
            EntityType = typeof(PMRegister);
            TypeField = typeof(PMRegister.module);
            TypeFilterValues = Activator.CreateInstance<BatchModule.PMListAttribute>().ValueLabelDic
                .Select(x => x.Key).ToArray();
            TypeFilterLabels = Activator.CreateInstance<BatchModule.PMListAttribute>().ValueLabelDic
                .Select(x => x.Value).ToArray();
        }

        public override Guid? GetNoteId(PXGraph graph, string clDisplayName)
        {
            var key = ComplianceReferenceTypeHelper.ConvertToDocumentKey<PMRegister>(clDisplayName);

            var noteId = new PXSelect<PMRegister,
                Where<PMRegister.module, Equal<Required<PMRegister.module>>,
                And<PMRegister.refNbr, Equal<Required<PMRegister.refNbr>>>>>(graph)
                .Select(key.DocType, key.RefNbr)
                .FirstTableItems
                .ToList()
                .SingleOrDefault()
                ?.NoteID;

            return noteId;
        }
    }
}