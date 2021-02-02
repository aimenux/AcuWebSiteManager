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

            result.Add(new Tuple<string, string>(FSLogActionFilter.type.Values.Service, TX.Type_Log.SERVICE));
            result.Add(new Tuple<string, string>(FSLogActionFilter.type.Values.Travel, TX.Type_Log.TRAVEL));

            if (action == ID.LogActions.START)
            {
                result.Add(new Tuple<string, string>(FSLogActionFilter.type.Values.Staff, TX.Type_Log.STAFF));
                result.Add(new Tuple<string, string>(FSLogActionFilter.type.Values.ServBasedAssignment, TX.Type_Log.SERV_BASED_ASSIGMENT));
            }

            return new CustomListAttribute(result.ToArray());
        }

        public class ListAttribute : CustomListAttribute
        {
            public ListAttribute() : base(
                new[]
                {
                    Pair(FSLogActionFilter.type.Values.Service, TX.Type_Log.SERVICE),
                    Pair(FSLogActionFilter.type.Values.Travel, TX.Type_Log.TRAVEL),
                    Pair(FSLogActionFilter.type.Values.Staff, TX.Type_Log.STAFF),
                    Pair(FSLogActionFilter.type.Values.ServBasedAssignment, TX.Type_Log.SERV_BASED_ASSIGMENT),
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
            public Service() : base(FSLogActionFilter.type.Values.Service) {; }
        }

        public class Travel : Data.BQL.BqlString.Constant<Travel>
        {
            public Travel() : base(FSLogActionFilter.type.Values.Travel) {; }
        }

        public class StaffAssignment : Data.BQL.BqlString.Constant<StaffAssignment>
        {
            public StaffAssignment() : base(FSLogActionFilter.type.Values.Staff) {; }
        }

        public class SrvBasedOnAssignment : Data.BQL.BqlString.Constant<SrvBasedOnAssignment>
        {
            public SrvBasedOnAssignment() : base(FSLogActionFilter.type.Values.ServBasedAssignment) {; }
        }
    }
}
