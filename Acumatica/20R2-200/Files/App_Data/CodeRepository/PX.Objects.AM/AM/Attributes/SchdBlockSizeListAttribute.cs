using System.Linq;
using PX.Data;

namespace PX.Objects.AM.Attributes
{
    public class SchdBlockSizeListAttribute : PXIntListAttribute
    {
        public SchdBlockSizeListAttribute()
            : base(
                new int[] { 5, 10, 15, 30, 60 },
                new string[] { "00:05", "00:10", "00:15", "00:30", "01:00" }){ }

        /// <summary>
        /// Is the block size a standard block size value?
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static bool Contains(int? blockSize)
        {
            var intList = new SchdBlockSizeListAttribute();
            return intList._AllowedValues.Contains(blockSize.GetValueOrDefault());
        }
    }
}