using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Overhead field attribute
    /// </summary>
    [PXDBString(20, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCC")]
    [PXUIField(DisplayName = "Overhead ID")]
    public class OverheadIDFieldAttribute : AcctSubAttribute
    {
    }
}