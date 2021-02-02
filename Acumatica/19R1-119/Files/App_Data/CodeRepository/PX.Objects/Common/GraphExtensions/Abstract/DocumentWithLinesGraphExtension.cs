using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.GL;

namespace PX.Objects.Common.GraphExtensions.Abstract
{
    public abstract class DocumentWithLinesGraphExtension<TGraph> : DocumentWithLinesGraphExtension<TGraph, Document, DocumentMapping>
        where TGraph : PXGraph
    {

    }

    public abstract class DocumentWithLinesGraphExtension<TGraph, TDocument, TDocumentMapping> : PXGraphExtension<TGraph>,
        IDocumentWithFinDetailsGraphExtension
        where TGraph : PXGraph 
        where TDocument : Document, new()
        where TDocumentMapping: IBqlMapping
    {
        protected abstract TDocumentMapping GetDocumentMapping();

        protected abstract DocumentLineMapping GetDocumentLineMapping();

        /// <summary>A mapping-based view of the <see cref="Document" /> data.</summary>
        public PXSelectExtension<TDocument> Documents;
        /// <summary>A mapping-based view of the <see cref="DocumentLine" /> data.</summary>
        public PXSelectExtension<DocumentLine> Lines;

        public List<int?> GetOrganizationIDsInDetails()
        {
            return Lines.Select()
                .AsEnumerable()
                .Select(row => PXAccess.GetParentOrganizationID(((DocumentLine) row).BranchID))
                .Distinct()
                .ToList();
        }

        protected virtual void _(Events.RowUpdated<TDocument> e)
        {
            if (ShouldUpdateLinesOnDocumentUpdated(e))
            {
                foreach (DocumentLine line in Lines.Select())
                {
                    ProcessLineOnDocumentUpdated(e, line);

                    (e.Cache as PXModelExtension<DocumentLine>)?.UpdateExtensionMapping(line);

                    Lines.Cache.MarkUpdated(line);
                }
            }
        }

        protected virtual bool ShouldUpdatePeriodOnDocumentUpdated(Events.RowUpdated<TDocument> e)
        {
            return !e.Cache.ObjectsEqual<Document.headerTranPeriodID>(e.Row, e.OldRow);
        }

        protected virtual bool ShouldUpdateDetailsOnDocumentUpdated(Events.RowUpdated<TDocument> e)
        {
            return ShouldUpdatePeriodOnDocumentUpdated(e);
        }

        protected virtual bool ShouldUpdateLinesOnDocumentUpdated(Events.RowUpdated<TDocument> e)
        {
            return ShouldUpdateDetailsOnDocumentUpdated(e);
        }

        protected virtual void ProcessLineOnDocumentUpdated(Events.RowUpdated<TDocument> e, DocumentLine line)
        {
            if (ShouldUpdatePeriodOnDocumentUpdated(e))
            {
                FinPeriodIDAttribute.DefaultPeriods<DocumentLine.finPeriodID>(Lines.Cache, line);
            }
        }
    }
}
