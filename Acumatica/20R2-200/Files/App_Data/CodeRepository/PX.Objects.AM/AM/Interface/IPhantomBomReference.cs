namespace PX.Objects.AM
{
    public interface IPhantomBomReference
    {
        string PhtmBOMID { get; set; }
        string PhtmBOMRevisionID { get; set; }
        int? PhtmBOMLineRef { get; set; }
        int? PhtmBOMOperationID { get; set; }
        int? PhtmLevel { get; set; }
        string PhtmMatlBOMID { get; set; }
        string PhtmMatlRevisionID { get; set; }
        int? PhtmMatlLineRef { get; set; }
        int? PhtmMatlOperationID { get; set; }
    }
}