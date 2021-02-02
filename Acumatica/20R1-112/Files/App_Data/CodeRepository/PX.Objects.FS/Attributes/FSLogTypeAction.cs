using PX.Data;
using System;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class FSLogTypeAction
    {
        public class CustomListAttribute : PXStringListAttribute
        {
            public string[] AllowedValues => _AllowedValues;

            public string[] AllowedLabels => _AllowedLabels;

            public CustomListAttribute(string[] allowedValues, string[] allowedLabels) : base(allowedValues, allowedLabels) { }

            public CustomListAttribute(Tuple<string, string>[] valuesToLabels) : base(valuesToLabels) { }
        }

        public static CustomListAttribute GetDropDownList(string action)
        {
            List<Tuple<string, string>> result = new List<Tuple<string, string>>();

            result.Add(new Tuple<string, string>(ID.Type_Log.SERVICE, TX.Type_Log.SERVICE));
            result.Add(new Tuple<string, string>(ID.Type_Log.TRAVEL, TX.Type_Log.TRAVEL));

            if (action == ID.LogActions.START)
            {
                result.Add(new Tuple<string, string>(ID.Type_Log.STAFF_ASSIGMENT, TX.Type_Log.STAFF_ASSIGMENT));
                result.Add(new Tuple<string, string>(ID.Type_Log.SERV_BASED_ASSIGMENT, TX.Type_Log.SERV_BASED_ASSIGMENT));
            }

            return new CustomListAttribute(result.ToArray());
        }

        public class ListAttribute : CustomListAttribute
        {
            public ListAttribute() : base(
                new[]
                {
                    Pair(ID.Type_Log.SERVICE, TX.Type_Log.SERVICE),
                    Pair(ID.Type_Log.TRAVEL, TX.Type_Log.TRAVEL),
                    Pair(ID.Type_Log.STAFF_ASSIGMENT, TX.Type_Log.STAFF_ASSIGMENT),
                    Pair(ID.Type_Log.SERV_BASED_ASSIGMENT, TX.Type_Log.SERV_BASED_ASSIGMENT),
                })
            { }
        }

        public static void SetLineTypeList<TypeField>(PXCache cache, object row, string action)
            where TypeField : class, IBqlField
        {
            CustomListAttribute dropDownList = GetDropDownList(action);
            PXStringListAttribute.SetList<TypeField>(cache, row, dropDownList.AllowedValues, dropDownList.AllowedLabels);
        }

        public class Service : Data.BQL.BqlString.Constant<Service>
        {
            public Service() : base(ID.Type_Log.SERVICE) {; }
        }

        public class Travel : Data.BQL.BqlString.Constant<Travel>
        {
            public Travel() : base(ID.Type_Log.TRAVEL) {; }
        }

        public class StaffAssignment : Data.BQL.BqlString.Constant<StaffAssignment>
        {
            public StaffAssignment() : base(ID.Type_Log.STAFF_ASSIGMENT) {; }
        }

        public class SrvBasedOnAssignment : Data.BQL.BqlString.Constant<SrvBasedOnAssignment>
        {
            public SrvBasedOnAssignment() : base(ID.Type_Log.SERV_BASED_ASSIGMENT) {; }
        }
    }
}
