using PX.Objects.CN.Subcontracts.SM.Descriptor;
using PX.Objects.PO;
using PX.SM;

namespace PX.Objects.CN.Subcontracts.SM.Extension
{
    public static class CacheEntityItemExtensions
    {
        public static string GetSubcontractViewName(this CacheEntityItem cacheEntityItem)
        {
            return cacheEntityItem.SubKey == typeof(POOrder).FullName
                ? Constants.SubcontractTypeName
                : cacheEntityItem.SubKey == typeof(POLine).FullName
                    ? Constants.SubcontractLine
                    : cacheEntityItem.Name;
        }
    }
}