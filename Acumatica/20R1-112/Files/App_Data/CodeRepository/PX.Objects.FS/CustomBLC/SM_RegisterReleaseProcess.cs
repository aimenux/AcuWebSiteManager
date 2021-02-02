using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class SM_RegisterReleaseProcess : PXGraphExtension<RegisterReleaseProcess>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region Overrides
        public delegate PMRegister OnBeforeReleaseDelegate(PMRegister doc);

        [PXOverride]
        public virtual PMRegister OnBeforeRelease(PMRegister doc, OnBeforeReleaseDelegate del)
        {
            ValidatePostBatchStatus(PXDBOperation.Update, ID.Batch_PostTo.PM, doc.Module, doc.RefNbr);

            if (del != null)
            {
                return del(doc);
            }

            return doc;
        }
        #endregion

        #region Validations
        public virtual void ValidatePostBatchStatus(PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
        {
            DocGenerationHelper.ValidatePostBatchStatus(Base, dbOperation, postTo, createdDocType, createdRefNbr);
        }
        #endregion
    }
}
