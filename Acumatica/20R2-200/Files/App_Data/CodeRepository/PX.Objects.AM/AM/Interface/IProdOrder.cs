namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing - Production order DAC interface
    /// </summary>
    public interface IProdOrder
    {
        string OrderType { get; set; }
        string ProdOrdID { get; set; }
    }
}