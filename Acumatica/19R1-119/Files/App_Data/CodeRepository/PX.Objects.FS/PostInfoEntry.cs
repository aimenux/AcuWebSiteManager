using PX.Data;

namespace PX.Objects.FS 
{
    public class PostInfoEntry : PXGraph<PostInfoEntry, FSPostInfo>
    {
        public PXSelect<FSPostInfo> PostInfoRecords;
    }
}