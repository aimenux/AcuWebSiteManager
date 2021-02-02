using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.CR;
using PX.Objects.IN;
using System.Linq;
using PX.Objects.AR;

namespace PX.Objects.FS
{
    public class SM_ARInvoiceEntryExternalTax : PXGraphExtension<ARInvoiceEntryExternalTax, ARInvoiceEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public delegate IAddressBase GetFromAddressLineDelegate(ARInvoice invoice, ARTran tran);
        public delegate IAddressBase GetToAddressLineDelegate(ARInvoice invoice, ARTran tran);

        [PXOverride]
        public virtual IAddressBase GetFromAddress(ARInvoice invoice, ARTran tran, GetFromAddressLineDelegate del)
        {
            int? SOID = GetSOIDRelated(tran);
            if (SOID != null && tran.SiteID == null)
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

        [PXOverride]
        public virtual IAddressBase GetToAddress(ARInvoice invoice, ARTran tran, GetToAddressLineDelegate del)
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

        protected int? GetSOIDRelated(ARTran tran)
        {
            int? SOID;

            FSxARTran fsxARTranRow = PXCache<ARTran>.GetExtension<FSxARTran>(tran);
            SOID = fsxARTranRow?.SOID;

            if (SOID == null)
            {
                var soLine = PXSelect<SOLine,
                            Where<SOLine.orderType, Equal<Required<ARTran.sOOrderType>>,
                            And<SOLine.orderNbr, Equal<Required<ARTran.sOOrderNbr>>,
                            And<SOLine.lineNbr, Equal<Required<ARTran.sOOrderLineNbr>>>>>>
                            .Select(Base, tran.SOOrderType, tran.SOOrderNbr, tran.SOOrderLineNbr)
                            .RowCast<SOLine>()
                            .FirstOrDefault();

                if (soLine != null)
                {
                    FSxSOLine fsxSOLineRow = PXCache<SOLine>.GetExtension<FSxSOLine>(soLine);
                    SOID = fsxSOLineRow?.SOID;
                }

            }

            return SOID;
        }
    }
}
