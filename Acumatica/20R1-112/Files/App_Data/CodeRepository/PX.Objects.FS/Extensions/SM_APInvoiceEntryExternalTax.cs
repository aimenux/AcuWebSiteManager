using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.CR;
using PX.Objects.IN;
using System.Linq;
using PX.Objects.AP;

namespace PX.Objects.FS
{
    public class SM_APInvoiceEntryExternalTax : PXGraphExtension<APInvoiceEntryExternalTax, APInvoiceEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public delegate IAddressBase GetFromAddressLineDelegate(APInvoice invoice, APTran tran);
        public delegate IAddressBase GetToAddressLineDelegate(APInvoice invoice, APTran tran);

        [PXOverride]
        public virtual IAddressBase GetFromAddress(APInvoice invoice, APTran tran, GetFromAddressLineDelegate del)
        {
            int? SOID = GetSOIDRelated(tran);

            if (SOID != null)
            {
                IAddressBase returnAddress = null;

                returnAddress = PXSelectJoin<FSAddress,
                                InnerJoin<FSServiceOrder,
                                    On<FSServiceOrder.serviceOrderAddressID, Equal<FSAddress.addressID>>>,
                                Where<
                                    FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                                    .Select(Base, SOID)
                                    .RowCast<FSAddress>()
                                    .FirstOrDefault();

                return returnAddress;
            }

            return del(invoice, tran);
        }

        [PXOverride]
        public virtual IAddressBase GetToAddress(APInvoice invoice, APTran tran, GetToAddressLineDelegate del)
        {
            int? SOID = GetSOIDRelated(tran);

            if (SOID != null)
            {
                IAddressBase returnAddress = null;

                returnAddress = PXSelectJoin<FSAddress,
                               InnerJoin<
                                   FSBranchLocation,
                                   On<FSBranchLocation.branchLocationAddressID, Equal<FSAddress.addressID>>,
                               InnerJoin<FSServiceOrder,
                                   On<FSServiceOrder.branchLocationID, Equal<FSBranchLocation.branchLocationID>>>>,
                               Where<
                                   FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                                   .Select(Base, SOID)
                                   .RowCast<FSAddress>()
                                   .FirstOrDefault();

                return returnAddress;
            }
            
            return del(invoice, tran);
        }

        protected int? GetSOIDRelated(APTran line)
        {
            FSxAPTran row = PXCache<APTran>.GetExtension<FSxAPTran>(line);
            return row?.SOID;            
        }
    }
}
