using Autofac;
using System;
using System.Collections.Generic;
using System.Web.Compilation;

namespace PX.Objects.PO.GraphExtensions.APInvoiceSmartPanel
{
    public class ExtensionSorting : Module
    {
        protected override void Load(ContainerBuilder builder) => builder.RunOnApplicationStart(() =>
            PXBuildManager.SortExtensions += list => PXBuildManager.PartialSort(list, _order)
            );

        private static readonly Dictionary<Type, int> _order = new Dictionary<Type, int>
        {
            {typeof(APInvoiceEntryExt.Prepayments), 0},
            {typeof(LinkLineExtension), 1 },
            {typeof(AddPOOrderExtension), 2 },
            {typeof(AddPOOrderLineExtension), 3 },
            {typeof(AddPOReceiptExtension), 4 },
            {typeof(AddPOReceiptLineExtension), 5 },
            {typeof(AddLandedCostExtension), 6 },
        };

    }
}
