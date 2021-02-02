using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.PJ.Common.CacheExtensions
{
    [Serializable]
    public sealed class NoteDocExt : PXCacheExtension<NoteDoc>
    {
        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Add")]
        public bool? IsAttached
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = "File Name", Enabled = false)]
        public string FileName
        {
            get;
            set;
        }

        [PXBool]
        [PXUIField(DisplayName = "Current", Enabled = false)]
        public bool? IsDrawingLogCurrentFile
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class isAttached : IBqlField
        {
        }

        public abstract class fileName : IBqlField
        {
        }

        public abstract class isDrawingLogCurrentFile : IBqlField
        {
        }
    }
}