using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class FSPostTo
    {
        public class CustomListAttribute : PXStringListAttribute
        {
            public string[] AllowedValues => _AllowedValues;

            public string[] AllowedLabels => _AllowedLabels;

            public CustomListAttribute(string[] allowedValues, string[] allowedLabels) : base(allowedValues, allowedLabels) { }

            public CustomListAttribute(Tuple<string, string>[] valuesToLabels) : base(valuesToLabels) { }
        }

        public static CustomListAttribute GetDropDownList(bool showSOInvoice, bool showNone, bool showProjects, bool showAPAR)
        {
            List<Tuple<string, string>> result = new List<Tuple<string, string>>();

            if (showAPAR == true)
            {
                result.Add(new Tuple<string, string>(ID.Batch_PostTo.AR_AP, TX.Batch_PostTo.AR_AP));
            }
            else
            {
                result.Add(new Tuple<string, string>(ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE, TX.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE));
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
            {
                result.Add(new Tuple<string, string>(ID.SrvOrdType_PostTo.SALES_ORDER_MODULE, TX.SrvOrdType_PostTo.SALES_ORDER_MODULE));

                if (PXAccess.FeatureInstalled<FeaturesSet.advancedSOInvoices>() && showSOInvoice)
                    result.Add(new Tuple<string, string>(ID.SrvOrdType_PostTo.SALES_ORDER_INVOICE, TX.SrvOrdType_PostTo.SALES_ORDER_INVOICE));
            }

            if (PXAccess.FeatureInstalled<FeaturesSet.projectModule>() && showProjects)
                result.Add(new Tuple<string, string>(ID.SrvOrdType_PostTo.PROJECTS, TX.SrvOrdType_PostTo.PROJECTS));

            if (showNone)
                result.Add(new Tuple<string, string>(ID.SrvOrdType_PostTo.NONE, TX.SrvOrdType_PostTo.NONE));

            return new CustomListAttribute(result.ToArray());
        }

        public class ListAttribute : CustomListAttribute
        {
            public ListAttribute() : base(
                new[]
                {
                    Pair(ID.Batch_PostTo.AR_AP, TX.Batch_PostTo.AR_AP),
                    Pair(ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE, TX.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE),
                    Pair(ID.SrvOrdType_PostTo.SALES_ORDER_MODULE, TX.SrvOrdType_PostTo.SALES_ORDER_MODULE),
                    Pair(ID.SrvOrdType_PostTo.SALES_ORDER_INVOICE, TX.SrvOrdType_PostTo.SALES_ORDER_INVOICE),
                    Pair(ID.SrvOrdType_PostTo.PROJECTS, TX.SrvOrdType_PostTo.PROJECTS),
                    Pair(ID.SrvOrdType_PostTo.NONE, TX.SrvOrdType_PostTo.NONE),
                })
            { }
        }

        public static void SetLineTypeList<LineTypeField>(PXCache cache, object row, bool showSOInvoice = false, bool showNone = false, bool showProjects = false, bool showAPAR = false)
            where LineTypeField : class, IBqlField
        {
            CustomListAttribute dropDownList = GetDropDownList(showSOInvoice, showNone, showProjects, showAPAR);
            PXStringListAttribute.SetList<LineTypeField>(cache, row, dropDownList.AllowedValues, dropDownList.AllowedLabels);
        }

        public class None : PX.Data.BQL.BqlString.Constant<None>
        {
            public None() : base(ID.SrvOrdType_PostTo.NONE) {; }
        }

        public class Accounts_Receivable_Module : PX.Data.BQL.BqlString.Constant<Accounts_Receivable_Module>
        {
            public Accounts_Receivable_Module() : base(ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE) {; }
        }

        public class Sales_Order_Module : PX.Data.BQL.BqlString.Constant<Sales_Order_Module>
        {
            public Sales_Order_Module() : base(ID.SrvOrdType_PostTo.SALES_ORDER_MODULE) {; }
        }

        public class Sales_Order_Invoice : PX.Data.BQL.BqlString.Constant<Sales_Order_Invoice>
        {
            public Sales_Order_Invoice() : base(ID.SrvOrdType_PostTo.SALES_ORDER_INVOICE) {; }
        }

        public class Projects : PX.Data.BQL.BqlString.Constant<Projects>
        {
            public Projects() : base(ID.SrvOrdType_PostTo.PROJECTS) {; }
        }
    }
}
