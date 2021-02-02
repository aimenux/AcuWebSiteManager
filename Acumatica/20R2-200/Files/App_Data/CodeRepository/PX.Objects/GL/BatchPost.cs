using System;
using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.CM;
using PX.SM;

namespace PX.Objects.GL
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class BatchPost : PXGraph<BatchPost>
	{
        #region Cache Attached Events
   
        #region ControlTotal
        
        [PXDBBaseCury(typeof(Batch.ledgerID))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Control Total")]
        protected virtual void Batch_ControlTotal_CacheAttached(PXCache sender)
        {
        }
        #endregion        

        #endregion

        public PXCancel<Batch> Cancel;
		[PXViewDetailsButton(typeof(Batch.batchNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
       
		[PXFilterable]
		public PXProcessing<Batch, Where<Batch.status, Equal<statusU>>> BatchList;
    

        public BatchPost()
        {
			GLSetup setup = GLSetup.Current;
			BatchList.SetProcessDelegate<PostGraph>(
                delegate(PostGraph pg, Batch batch)
                {
                    pg.Clear();
                    pg.PostBatchProc(batch);
                });

			BatchList.SetProcessCaption(Messages.ProcPost);
			BatchList.SetProcessAllCaption(Messages.ProcPostAll);
			PXNoteAttribute.ForcePassThrow<Batch.noteID>(BatchList.Cache);
        }
	
		public PXSetup<GLSetup> GLSetup;
	}
}