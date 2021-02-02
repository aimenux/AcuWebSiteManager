using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Standard Production order type field attribute
    /// </summary>
    [PXDBString(2, IsFixed = true, InputMask = ">aa")]
    [PXUIField(DisplayName = "Order Type")]
    public class AMOrderTypeFieldAttribute : PX.Objects.GL.AcctSubAttribute
    {

    }
}