using System.Collections;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CM
{
    [TableAndChartDashboardType]
    public class TranslationRelease : PXGraph<TranslationRelease>
    {
        [PXFilterable]
        public PXAction<TranslationHistory> cancel;
        [PX.SM.PXViewDetailsButton(typeof(TranslationHistory.referenceNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
        public PXProcessing<TranslationHistory, Where<TranslationHistory.released, Equal<False>>> TranslationReleaseList;
        public PXSetup<CMSetup> CMSetup;

        #region Implementation 
        public TranslationRelease()
        {
            CMSetup setup = CMSetup.Current;
            TranslationReleaseList.SetProcessDelegate(
                delegate(TranslationHistory transl)
                {
					var graph = CreateInstance<TranslationHistoryMaint>();
					graph.CreateBatch(transl, false);
                });

            TranslationReleaseList.SetProcessCaption(Messages.Release);
            TranslationReleaseList.SetProcessAllVisible(false);
        }
        #endregion

        #region Buttons
        [PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
        [PXCancelButton]
        protected virtual IEnumerable Cancel(PXAdapter adapter)
        {
            TranslationReleaseList.Cache.Clear();
            TimeStamp = null;
            PXLongOperation.ClearStatus(this.UID);
            return adapter.Get();
        }
        #endregion
    }
}