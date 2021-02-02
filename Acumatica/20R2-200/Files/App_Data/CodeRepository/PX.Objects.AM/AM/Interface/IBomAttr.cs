namespace PX.Objects.AM
{
    public interface IBomAttr : IBomRevision, IOperation
    {
        int? LineNbr { get; set; }
    }
}
