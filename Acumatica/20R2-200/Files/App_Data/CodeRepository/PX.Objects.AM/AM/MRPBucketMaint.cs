using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Graph for MRP Bucket Maintenance
    /// </summary>

    public class MRPBucketMaint : PXGraph<MRPBucketMaint, AMMRPBucket>
    {
        public PXSelect<AMMRPBucket> CurrentBucket;
        [PXImport(typeof(AMMRPBucket))]
        public PXSelect<AMMRPBucketDetail, Where<AMMRPBucketDetail.bucketID, Equal<Current<AMMRPBucket.bucketID>>>> BucketRecords;
    }
}
