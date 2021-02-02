using System.Collections;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.GL;

namespace PX.Objects.AM.GraphExtensions
{
    public class JournalEntryAMExtension : PXGraphExtension<JournalEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        /// <summary>
        /// Is the journal entry running as an internal manufacturing call/process
        /// </summary>
        protected bool IsInternalCall;

        /// <summary>
        /// Mark the journal entry as an internal Manufacturing call when true
        /// </summary>
        internal static void SetIsInternalCall(JournalEntry graph, bool isInternal)
        {
            var graphExt = graph?.GetExtension<JournalEntryAMExtension>();
            if (graphExt == null)
            {
                return;
            }

            graphExt.IsInternalCall = isInternal;
        }

        public PXAction<Batch> viewDocument;
        [PXLookupButton]
        [PXUIField(DisplayName = PX.Objects.GL.Messages.ViewSourceDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable ViewDocument(PXAdapter adapter)
        {
            if (Base.BatchModule.Current != null 
                && Base.BatchModule.Current.Module == BatchModule.GL
                && Base.BatchModule.Current.OrigModule == Common.ModuleAM)
            {
                // This is a Manufacturing GL entry. View the MFG document that created this

                var extension = PXCache<Batch>.GetExtension<BatchExt>(Base.BatchModule.Current);
                if (extension != null && !string.IsNullOrWhiteSpace(extension.AMDocType) && !string.IsNullOrWhiteSpace(extension.AMBatNbr))
                {
                    AMDocType.DocTypeRedirectRequiredException(extension.AMDocType, extension.AMBatNbr, Base);
                }

                return adapter.Get();
            }

            return Base.ViewDocument(adapter);
        }

        protected virtual bool IsManufacturingBatch(Batch batch)
        {
            return batch != null && batch.Module == BatchModule.GL && batch.OrigModule == Common.ModuleAM;
        }

        [PXOverride]
        public virtual void Batch_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected method)
        {
            method?.Invoke(cache, e);

            var glBatch = (Batch)e.Row;

            var isMfg = IsManufacturingBatch(glBatch);
            if (isMfg)
            {
                Base.viewDocument.SetEnabled(true);
            }

            if (glBatch == null || IsInternalCall || glBatch.Released.GetValueOrDefault() || !isMfg)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled(cache, e.Row, false);

            //Keep the key fields enabled so the user can change to different batch numbers
            PXUIFieldAttribute.SetEnabled<Batch.module>(cache, e.Row, true);
            PXUIFieldAttribute.SetEnabled<Batch.batchNbr>(cache, e.Row, true);

            if (glBatch.Hold.GetValueOrDefault())
            {
                // Disable batch as the release should come from the Manufacturing Batch
                //  In some cases there are batches with no lines/zero cost and if no cost give the users the ability to delete
                cache.AllowDelete = glBatch.CuryCreditTotal.GetValueOrDefault() == 0 && glBatch.CuryDebitTotal.GetValueOrDefault() == 0;
                Base.GLTranModuleBatNbr.AllowInsert = false;
                Base.GLTranModuleBatNbr.AllowDelete = false;
            }
        }

        [PXOverride]
        public virtual void GLTran_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected method)
        {
            method?.Invoke(cache, e);
            var batch = Base.BatchModule?.Current;
            var glTran = (GLTran)e.Row;
            if (glTran == null || IsInternalCall || batch == null || batch.Released == true)
            {
                return;
            }

            if (IsManufacturingBatch(batch) && !Base.GLTranModuleBatNbr.AllowInsert)
            {
                PXUIFieldAttribute.SetEnabled(Base.GLTranModuleBatNbr.Cache, glTran, false);
            }
        }
    }
}