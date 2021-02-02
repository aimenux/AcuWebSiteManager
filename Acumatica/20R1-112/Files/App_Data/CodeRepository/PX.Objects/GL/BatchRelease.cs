using System;
using System.Text;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.SM;
using System.Linq;

namespace PX.Objects.GL
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class BatchRelease : PXGraph<BatchRelease>
	{
        #region Type Override events

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
		[PXViewDetailsButton(typeof(Batch.batchNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow )]
		[PXFilterable]
		public PXProcessing<Batch, 
			Where<Batch.released, Equal<boolFalse>, 
			And<Batch.scheduled, Equal<boolFalse>, 
			And<Batch.voided,Equal<boolFalse>,
            And<Batch.hold, Equal<boolFalse>>>>>> BatchList;

        public BatchRelease()
		{
			GLSetup setup = GLSetup.Current;
            BatchList.SetProcessDelegate<PostGraph>(ReleaseBatch);
            BatchList.SetProcessCaption(Messages.ProcRelease);
            BatchList.SetProcessAllCaption(Messages.ProcReleaseAll);
			PXNoteAttribute.ForcePassThrow<Batch.noteID>(BatchList.Cache);
		}

		public PXSetup<GLSetup> GLSetup;
		
		public static void ReleaseBatch(PostGraph pg, Batch batch)
        {
            pg.Clear();
            pg.ReleaseBatchProc(batch);
            if ((bool)batch.AutoReverse)
            {
                Batch copy = pg.ReverseBatchProc(batch);
                if (pg.AutoPost)
                {
                    pg.PostBatchProc(batch);
                }
                pg.Clear();
				pg.TimeStamp = copy.tstamp;
                pg.ReleaseBatchProc(copy);
                if (pg.AutoPost)
                {
                    pg.PostBatchProc(copy);
                }
            }
            else if (pg.AutoPost)
            {
                pg.PostBatchProc(batch);
            }
        }


	}
}