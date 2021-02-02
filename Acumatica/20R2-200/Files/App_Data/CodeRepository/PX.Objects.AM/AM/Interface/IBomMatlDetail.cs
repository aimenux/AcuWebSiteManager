namespace PX.Objects.AM
{
    public interface IBomMatlDetail : IBomRevision, IOperation, IBomDetail
    {
        int? MatlLineID { get; set; }
    }
}
