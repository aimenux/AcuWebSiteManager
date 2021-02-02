using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Standard Production Nbr field attribute
    /// </summary>
    [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
    [PXUIField(DisplayName = "Production Nbr", Visibility = PXUIVisibility.SelectorVisible)]
    public class ProductionNbrAttribute : AcctSubAttribute
    {

    }
}
