using PX.Data;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class FSLineType
    {
        public class CustomListAttribute : PXStringListAttribute
        {
            public string[] AllowedValues => _AllowedValues;

            public string[] AllowedLabels => _AllowedLabels;

            public CustomListAttribute(string[] allowedValues, string[] allowedLabels) : base(allowedValues, allowedLabels) { }

            public CustomListAttribute(Tuple<string, string>[] valuesToLabels) : base(valuesToLabels) { }
        }

        public static CustomListAttribute GetDropDownList(bool includeIN, bool includeTemplate, bool includePickup)
        {
            List<Tuple<string, string>> result = new List<Tuple<string, string>>();

            result.Add(new Tuple<string, string>(ID.LineType_ALL.SERVICE, TX.LineType_ALL.SERVICE));
            result.Add(new Tuple<string, string>(ID.LineType_ALL.NONSTOCKITEM, TX.LineType_ALL.NONSTOCKITEM));

            if (includeIN == true)
            {
                result.Add(new Tuple<string, string>(ID.LineType_ALL.INVENTORY_ITEM, TX.LineType_ALL.INVENTORY_ITEM));
            }

            result.Add(new Tuple<string, string>(ID.LineType_ALL.COMMENT, TX.LineType_ALL.COMMENT));
            result.Add(new Tuple<string, string>(ID.LineType_ALL.INSTRUCTION, TX.LineType_ALL.INSTRUCTION));

            if (includePickup == true)
            {
                result.Add(new Tuple<string, string>(ID.LineType_ALL.PICKUP_DELIVERY, TX.LineType_ALL.PICKUP_DELIVERY));
            }

            if (includeTemplate == true)
            {
                result.Add(new Tuple<string, string>(ID.LineType_ALL.SERVICE_TEMPLATE, TX.LineType_ALL.SERVICE_TEMPLATE));
            }

            return new CustomListAttribute(result.ToArray());
        }

        public class ListAttribute : CustomListAttribute
        {
            public ListAttribute() : base(
                new[]
                {
                    Pair(ID.LineType_ALL.SERVICE, TX.LineType_ALL.SERVICE),
                    Pair(ID.LineType_ALL.NONSTOCKITEM, TX.LineType_ALL.NONSTOCKITEM),
                    Pair(ID.LineType_ALL.INVENTORY_ITEM, TX.LineType_ALL.INVENTORY_ITEM),
                    Pair(ID.LineType_ALL.COMMENT, TX.LineType_ALL.COMMENT),
                    Pair(ID.LineType_ALL.INSTRUCTION, TX.LineType_ALL.INSTRUCTION),
                    Pair(ID.LineType_ALL.SERVICE_TEMPLATE, TX.LineType_ALL.SERVICE_TEMPLATE),
                    Pair(ID.LineType_ALL.PICKUP_DELIVERY, TX.LineType_ALL.PICKUP_DELIVERY),
                })
            { }
        }

        public static void SetLineTypeList<LineTypeField>(PXCache cache, object row, bool includeIN, bool includeTemplate, bool includePickup)
            where LineTypeField : class, IBqlField
        {
            CustomListAttribute dropDownList = GetDropDownList(includeIN, includeTemplate, includePickup);
            PXStringListAttribute.SetList<LineTypeField>(cache, row, dropDownList.AllowedValues, dropDownList.AllowedLabels);
        }

        public class Service : PX.Data.BQL.BqlString.Constant<Service>
        {
            public Service() : base(ID.LineType_ALL.SERVICE) {; }
        }

        public class NonStockItem : PX.Data.BQL.BqlString.Constant<NonStockItem>
        {
            public NonStockItem() : base(ID.LineType_ALL.NONSTOCKITEM) {; }
        }

        public class Inventory_Item : PX.Data.BQL.BqlString.Constant<Inventory_Item>
        {
            public Inventory_Item() : base(ID.LineType_ALL.INVENTORY_ITEM) {; }
        }

        public class Comment : PX.Data.BQL.BqlString.Constant<Comment>
        {
            public Comment() : base(ID.LineType_ALL.COMMENT) {; }
        }

        public class Instruction : PX.Data.BQL.BqlString.Constant<Instruction>
        {
            public Instruction() : base(ID.LineType_ALL.INSTRUCTION) {; }
        }

        public class ServiceTemplate : PX.Data.BQL.BqlString.Constant<ServiceTemplate>
        {
            public ServiceTemplate() : base(ID.LineType_ALL.SERVICE_TEMPLATE) {; }
        }
    }
}
