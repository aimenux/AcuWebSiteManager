using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.GraphExtensions
{
    public class TransactionInquiryExt : PXGraphExtension<TransactionInquiry>
    {
        public delegate IEnumerable ExecuteSelectDelegate(string viewName, object[] parameters, object[] searches,
            string[] sortColumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows,
            ref int totalRows);

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXOverride]
        public IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortColumns,
            bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows,
            ExecuteSelectDelegate baseMethod)
        {
            if (viewName == Base.Transactions.Name && filters != null)
            {
                ChangeBusinessAccountFilterDataFieldIfExist(filters);
            }
            return baseMethod(viewName, parameters, searches, sortColumns, descendings, filters, ref startRow,
                maximumRows, ref totalRows);
        }

        private void ChangeBusinessAccountFilterDataFieldIfExist(PXFilterRow[] filters)
        {
            var businessAccountFilterDataField = string.Concat(nameof(PMTran.BAccountID),
                Common.Descriptor.SharedMessages.DescriptionFieldPostfix);
            var filter = filters.FirstOrDefault(x => x.DataField == businessAccountFilterDataField);
            if (filter != null)
            {
                filter.DataField = nameof(PMTran.BAccountID);
            }
        }
    }
}
