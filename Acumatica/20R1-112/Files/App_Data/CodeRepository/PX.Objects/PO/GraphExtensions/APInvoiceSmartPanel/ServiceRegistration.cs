using Autofac;
using PX.Data.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;

namespace PX.Objects.PO.GraphExtensions.APInvoiceSmartPanel
{
    public class ServiceRegistration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.ActivateOnApplicationStart<ExtensionSorting>();
        }

        private class ExtensionSorting
        {
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

            public ExtensionSorting()
            {
                PXBuildManager.SortExtensions += StableSort;
            }

            private static void StableSort(List<Type> list)
            {
				PXBuildManager.PartialSort(list, _order);
			}
        }
    }
}
