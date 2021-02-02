using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Revision field attribute
    /// (Length 10)
    /// </summary>
    [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
    [PXUIField(DisplayName = "Revision")]
    public class RevisionIDFieldAttribute : AcctSubAttribute
    {
    }
}