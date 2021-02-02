using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// String attribute for formula based fields.
    /// Helps to identify all formula fields.
    /// </summary>
    public class FormulaStringAttribute : PXDBStringAttribute
    {
        public FormulaStringAttribute()
        {
            IsUnicode = true;
        }

        public FormulaStringAttribute(int length) : base(length)
        {
            IsUnicode = true;
        }
    }
}