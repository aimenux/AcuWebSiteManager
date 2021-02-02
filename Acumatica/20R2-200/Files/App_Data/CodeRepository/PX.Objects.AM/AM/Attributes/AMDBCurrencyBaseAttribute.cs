using System;
using PX.Common;
using PX.Data;
using PX.Objects.CM;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Reverse the logic to allow the base cury values to contain the entered/formula values with the cury fields being calculated.
    /// The standard PXDBCurrencyAttribute assumse cury is the field being entered/formula and the base is calculated.
    /// (Blend/copy of PXDBCurrencyAttribute and PXCurrencyHelper - 5.30.1128)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
    public class AMDBCurrencyBaseAttribute : PXDBCurrencyAttribute
    {
        protected System.Type _KeyField;
        protected System.Type _ResultField;
        protected int _ResultOrdinal;
        protected System.Type _ClassType;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyField">Field in this table used as a key for CurrencyInfo
        /// table. If 'null' is passed then the constructor will try to find field
        /// in this table named 'CuryInfoID'.</param>
        /// <param name="resultField">Field in this table to store the result of
        /// currency conversion. If 'null' is passed then the constructor will try
        /// to find field in this table name of which start with 'base'.</param>
        public AMDBCurrencyBaseAttribute(Type keyField, Type resultField)
            : base(keyField, resultField)
        {
            this._KeyField = keyField;
            this._ResultField = resultField;
        }

        public AMDBCurrencyBaseAttribute(Type precision, Type keyField, Type resultField)
            : base(precision, keyField, resultField)
        {
            this._KeyField = keyField;
            this._ResultField = resultField;
        }

        public override void CacheAttached(PXCache sender)
        {
            _ClassType = sender.GetItemType();
            _ResultOrdinal = sender.GetFieldOrdinal(_ResultField.Name);
            base.CacheAttached(sender);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="precision">Precision for value of 'decimal' type</param>
        /// <param name="keyField">Field in this table used as a key for CurrencyInfo
        /// table. If 'null' is passed then the constructor will try to find field
        /// in this table named 'CuryInfoID'.</param>
        /// <param name="resultField">Field in this table to store the result of
        /// currency conversion. If 'null' is passed then the constructor will try
        /// to find field in this table name of which start with 'base'.</param>
        public AMDBCurrencyBaseAttribute(int precision, Type keyField, Type resultField)
            : base(precision, keyField, resultField)
        {
            this._KeyField = keyField;
            this._ResultField = resultField;
        }

        public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            object NewValue = sender.GetValue(e.Row, _FieldOrdinal);
            this.CalcCuryValue(sender, new PXFieldVerifyingEventArgs(e.Row, NewValue, false));
        }

        public override void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            object NewValue = sender.GetValue(e.Row, _FieldOrdinal);
            this.CalcCuryValue(sender, new PXFieldVerifyingEventArgs(e.Row, NewValue, e.ExternalCall));
        }

        public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            this.CalcCuryValue(sender, e);
        }

        public virtual void CalcCuryValue(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (!BaseCalc)
            {
                return;
            }
            PXView curyinfo;
            CurrencyInfo info = null;
            if (e.NewValue != null && (info = getInfo(sender, e.Row, out curyinfo)) != null && info.CuryRate != null && info.BaseCalc == true)
            {
                decimal rate = (decimal)info.CuryRate;
                if (rate == 0.0m)
                {
                    rate = 1.0m;
                }
                // the one line below is the only reason for this entire class attribute
                bool mult = info.CuryMultDiv == "D";   //changing here to flip...
                decimal cval = (decimal)e.NewValue;
                object value = mult ? cval * rate : cval / rate;
                sender.RaiseFieldUpdating(_ResultField.Name, e.Row, ref value);
#pragma warning disable PX1047 // A DAC instance passed to the event handler cannot be modified inside the event handler  [ignored as this is a large copy from an Acumatica attribute to change 1 line of code]
                sender.SetValue(e.Row, _ResultOrdinal, value);
#pragma warning restore PX1047
            }
            else if (info == null || info.BaseCalc == true)
            {
                object value = e.NewValue;
                sender.RaiseFieldUpdating(_ResultField.Name, e.Row, ref value);
#pragma warning disable PX1047 // A DAC instance passed to the event handler cannot be modified inside the event handler  [ignored as this is a large copy from an Acumatica attribute to change 1 line of code]
                sender.SetValue(e.Row, _ResultOrdinal, value);
#pragma warning restore PX1047
            }
        }

        //PXCurrencyHelper.GetView
        private static bool GetView(PXGraph graph, Type classType, Type keyField, out PXView view)
        {
            string KeyFieldName = keyField.Name.With(_ => char.ToUpper(_[0]) + _.Substring(1));
            string ViewName = "_" + classType.Name + "_CurrencyInfo_" + (KeyFieldName == "CuryInfoID" ? "" : KeyFieldName + "_");
            if (!graph.Views.TryGetValue(ViewName, out view))
            {
                ViewName = "_CurrencyInfo_" + classType.FullName + "." + KeyFieldName + "_";
                if (!graph.Views.TryGetValue(ViewName, out view))
                {
                    BqlCommand cmd = BqlCommand.CreateInstance(
                                            typeof(Select<,>),
                                            typeof(CurrencyInfo),
                                            typeof(Where<,>),
                                            typeof(CurrencyInfo.curyInfoID),
                                            typeof(Equal<>),
                                            typeof(Optional<>),
                                            keyField
                                            );
                    graph.Views[ViewName] = view = new PXView(graph, false, cmd);
                }
            }
            return true;
        }

        //PXCurrencyHelper.LocateInfor
        private CurrencyInfo LocateInfo(PXCache cache, CurrencyInfo info)
        {
            //Normalize() is called in RowPersisted() of CurrencyInfo, until it Locate() will return null and Select() will place additional copy of CurrencyInfo in _Items.
            foreach (CurrencyInfo cached in cache.Inserted)
            {
                if (object.Equals(cached.CuryInfoID, info.CuryInfoID))
                {
                    return cached;
                }
            }
            return null;
        }

        //PXCurrencyHelper.getInfo
        private CurrencyInfo getInfo(PXCache sender, object row, out PXView curyinfoview)
        {
            if (GetView(sender.Graph, _ClassType, _KeyField, out curyinfoview))
            {
                CurrencyInfo info = curyinfoview.Cache.Current as CurrencyInfo;
                if (info != null)
                {
                    long? key = (long?)sender.GetValue(row, _KeyField.Name);
                    if (row == null || !object.Equals(info.CuryInfoID, key))
                    {
                        info = new CurrencyInfo();
                        info.CuryInfoID = key;
                        info = LocateInfo(curyinfoview.Cache, info) ?? curyinfoview.Cache.Locate(info) as CurrencyInfo;
                        if (info == null)
                        {
                            if (key == null)
                            {
                                object val;
                                if (sender.RaiseFieldDefaulting(_KeyField.Name, null, out val))
                                {
                                    sender.RaiseFieldUpdating(_KeyField.Name, null, ref val);
                                }
                                if (val != null)
                                {
                                    info = new CurrencyInfo();
                                    info.CuryInfoID = Convert.ToInt64(val);
                                    info = curyinfoview.Cache.Locate(info) as CurrencyInfo;
                                }
                            }
                            if (key == null)
                            {
                                //emulate Current<> behavior to avoid lock violation
                                object val = sender.GetValue(sender.Current, _KeyField.Name);
                                if (val != null)
                                {
                                    info = new CurrencyInfo();
                                    info.CuryInfoID = Convert.ToInt64(val);
                                    info = LocateInfo(curyinfoview.Cache, info) ?? curyinfoview.Cache.Locate(info) as CurrencyInfo;
                                }
                            }

                            if (info == null)
                            {
                                info = curyinfoview.SelectSingleBound(new object[] { row }) as CurrencyInfo;
                            }
                        }
                    }
                }
                else
                {
                    info = new CurrencyInfo();
                    info.CuryInfoID = (long?)sender.GetValue(row, _KeyField.Name);
                    info = LocateInfo(curyinfoview.Cache, info) ?? curyinfoview.Cache.Locate(info) as CurrencyInfo;
                    if (info == null)
                    {
                        info = curyinfoview.SelectSingleBound(new object[] { row }) as CurrencyInfo;
                    }
                }
                return info;
            }
            return null;
        }
    }
}