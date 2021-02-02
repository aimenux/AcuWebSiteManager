using System;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote.ComplianceDocumentEntityStrategies;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote
{
    public class ComplianceDocumentEntityHelper
    {
        private static readonly ComplianceDocumentEntityStrategy[] Strategies =
        {
            new ApInvoiceStrategy(),
            new ApPaymentStrategy(),
            new ArInvoiceStrategy(),
            new ArPaymentStrategy(),
            new PoOrderStrategy(),
            new PmRegisterStrategy()
        };

        private readonly Type itemType;
        private readonly ComplianceDocumentEntityStrategy complianceDocumentEntityStrategy;

        public ComplianceDocumentEntityHelper(Type itemType)
        {
            this.itemType = itemType;
            complianceDocumentEntityStrategy = Strategies.Single(x => x.EntityType == itemType);
        }

        public bool IsStrategyExist => complianceDocumentEntityStrategy != null;

        public PXView CreateView(PXGraph graph)
        {
            var command = BqlCommand.CreateInstance(typeof(Select<>), itemType);
            if (IsStrategyExist)
            {
                if (complianceDocumentEntityStrategy.FilterExpression != null)
                {
                    command = command.WhereNew(complianceDocumentEntityStrategy.FilterExpression);
                }
                var selectorEntityCache = graph.Caches[itemType];
                if (selectorEntityCache != null)
                {
                    PXStringListAttribute.SetList(selectorEntityCache, null,
                        complianceDocumentEntityStrategy.TypeField.Name,
                        complianceDocumentEntityStrategy.TypeFilterValues,
                        complianceDocumentEntityStrategy.TypeFilterLabels);
                }
            }
            return new PXView(graph, true, command);
        }

        public Guid? GetNoteId(PXGraph graph, string clDisplayName)
        {
            return complianceDocumentEntityStrategy.GetNoteId(graph, clDisplayName);
        }
    }
}