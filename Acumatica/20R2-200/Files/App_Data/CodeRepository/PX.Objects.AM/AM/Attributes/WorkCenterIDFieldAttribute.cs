using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Work Center field attribute
    /// </summary>
    [PXDBString(20, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCC")]
    [PXUIField(DisplayName = "Work Center")]
    public class WorkCenterIDFieldAttribute : AcctSubAttribute 
    {
    }
}