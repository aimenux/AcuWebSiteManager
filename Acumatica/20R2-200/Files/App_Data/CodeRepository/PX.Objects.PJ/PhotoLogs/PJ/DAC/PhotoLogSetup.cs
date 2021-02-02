using System;
using PX.Objects.PJ.PhotoLogs.PJ.Graphs;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CS;

namespace PX.Objects.PJ.PhotoLogs.PJ.DAC
{
    [Serializable]
    [PXPrimaryGraph(typeof(PhotoLogSetupMaint))]
    [PXCacheName("Photo Log Preferences")]
    public class PhotoLogSetup : BaseCache, IBqlTable
    {
        [PXDBString(10, IsUnicode = true)]
        [PXDefault("PHOTOLOG")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Photo Log Numbering Sequence")]
        public string PhotoLogNumberingId
        {
            get;
            set;
        }

        [PXDBString(10, IsUnicode = true)]
        [PXDefault("PHOTO")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Photo Numbering Sequence")]
        public string PhotoNumberingId
        {
            get;
            set;
        }

        public abstract class photoLogNumberingId : BqlString.Field<photoLogNumberingId>
        {
        }

        public abstract class photoNumberingId : BqlString.Field<photoNumberingId>
        {
        }
    }
}