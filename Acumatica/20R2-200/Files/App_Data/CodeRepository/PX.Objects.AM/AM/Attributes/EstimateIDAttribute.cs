using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Standard Estimate ID field attribute
    /// </summary>
    [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
    [PXUIField(DisplayName = "Estimate ID", Visibility = PXUIVisibility.SelectorVisible, FieldClass = Features.ESTIMATINGFIELDCLASS)]
    public class EstimateIDAttribute : AcctSubAttribute
    {

    }
}