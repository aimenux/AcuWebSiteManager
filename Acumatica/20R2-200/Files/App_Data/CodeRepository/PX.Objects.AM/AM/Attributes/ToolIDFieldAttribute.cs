using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing ToolID field attribute
    /// </summary>
    [PXDBString(30, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
    [PXUIField(DisplayName = "Tool ID")]
    public class ToolIDFieldAttribute : AcctSubAttribute
    {
    }
}