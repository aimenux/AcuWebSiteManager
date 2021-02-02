using System;
namespace PX.Objects.AM
{
    public interface IBomDetail : IBomRevision, IOperation
    {
        int? LineID { get; set; }
    }
}
