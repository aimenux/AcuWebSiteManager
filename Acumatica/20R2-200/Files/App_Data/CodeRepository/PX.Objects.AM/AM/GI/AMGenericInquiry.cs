using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using PX.Common;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Assists in building calls to generic inquiries
    /// </summary>
    public abstract class AMGenericInquiry
    {
        /// <summary>
        /// Base Acumatica generic inquriy URL
        /// </summary>
        protected const string BASEURL = PXGenericInqGrph.INQUIRY_URL;
        /// <summary>
        /// Data view name for Generic Inquiry graph
        /// </summary>
        protected const string GiGraphResultViewName = "Results";

        /// <summary>
        /// The Key ID value that represents the Generic Inquiry
        /// </summary>
        protected Guid ID;

        /// <summary>
        /// Contains the set list of parameters for use in building the URL to call the GI
        /// </summary>
        protected Dictionary<string, string> ParameterDictionary;

        /// <summary>
        /// Used to filter GI data by non parameter fields
        /// </summary>
        protected List<PXFilterRow> Filters;

        /// <summary>
        /// Format used for adding parameters to the URL
        /// </summary>
        protected const string URLPARAMFORMAT = "&{0}={1}";
        /// <summary>
        /// Used for force set a value to null when there might be an auto populated/default value
        /// </summary>
        public const string NullValue = "null";

        public AMGenericInquiry(Guid id)
        {
            ID = id;
            ParameterDictionary = new Dictionary<string, string>();
            Filters = new List<PXFilterRow>();
        }

        /// <summary>
        /// Sets a GI parameter to pass when opening the generic inquiry to a null value
        /// </summary>
        /// <param name="key">parameter key</param>
        public virtual void SetParameterNull(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            ParameterDictionary[key] = null;
        }

        /// <summary>
        /// Sets a GI parameter to pass when opening the generic inquiry
        /// </summary>
        /// <param name="key">parameter key</param>
        /// <param name="value">parameter value</param>
        public virtual void SetParameter(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            ParameterDictionary[key] = value;
        }

        public virtual void AddFilter(Type table, Type field, PXCondition filterCondition, object value)
        {
            Filters.Add(new PXFilterRow
            {
                DataField = FilterDataFieldName(table, field),
                Condition = filterCondition,
                Value = value
            });
        }

        protected string FilterDataFieldName(Type table, Type field)
        {
            return $"{table.Name}_{field.Name}";
        }

        /// <summary>
        /// Sets a GI date parameter to pass when opening the generic inquiry
        /// </summary>
        public virtual void SetParameter(string key, DateTime? value, PXGraph graph)
        {
            SetParameter(key, Common.Dates.ToCultureString(graph, value));
        }

        /// <summary>
        /// Sets a GI date parameter to pass when opening the generic inquiry
        /// </summary>
        public virtual void SetParameter(string key, DateTime value, PXGraph graph)
        {
            SetParameter(key, Common.Dates.ToCultureString(graph, value));
        }

        /// <summary>
        /// Sets a GI date parameter with formating to pass when opening the generic inquiry
        /// </summary>
        public void SetParameter(string key, DateTime? value, IFormatProvider fp)
        {
            if (string.IsNullOrWhiteSpace(key)
                || value == null)
            {
                return;
            }

            SetParameter(key, value.GetValueOrDefault().ToString("g", fp));
        }

        /// <summary>
        /// special characters need encoded to use in the URL such as dates
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Url Encoded Value</returns>
        public static string Encode(string value)
        {
            return !string.IsNullOrWhiteSpace(value) ? HttpContext.Current.Server.UrlEncode(value) : null;
        }

        /// <summary>
        /// Open the Generic Inquiry in a new window
        /// </summary>
        public virtual void CallGenericInquiry()
        {
            CallGenericInquiry(PXBaseRedirectException.WindowMode.NewWindow);
        }

        /// <summary>
        /// Open the Generic Inquiry
        /// </summary>
        public virtual void CallGenericInquiry(PXBaseRedirectException.WindowMode windowMode)
        {
            var redirect = new PXRedirectToUrlException(BuildURL(), windowMode, string.Empty);
            if (Filters != null && Filters.Count > 0)
            {
                redirect.Filters.Add(new PXBaseRedirectException.Filter(GiGraphResultViewName, Filters.ToArray()));
            }
            throw redirect;
        }

        /// <summary>
        /// Create the generic inquiry URL including the parameter values
        /// </summary>
        /// <returns></returns>
        protected virtual string BuildURL()
        {
            var sb = new StringBuilder();
            sb.Append(BASEURL);
            sb.Append("?");
            sb.AppendFormat("ID={0}", ID);
            sb.Append(BuildParameters());
#if DEBUG
            PXTrace.WriteInformation("GI URL BUILD: {0}", sb.ToString()); 
            AMDebug.TraceWriteLine("GI URL BUILD: {0}", sb.ToString());
#endif

            return sb.ToString();
        }

        protected virtual string BuildParameters()
        {
            var sb = new StringBuilder();
            foreach (var parameter in ParameterDictionary)
            {
                if (string.IsNullOrWhiteSpace(parameter.Value))
                {
                    continue;
                }

                if (TryFormatParameter(URLPARAMFORMAT, parameter.Key, parameter.Value.TrimIfNotNullEmpty(), out var formatValue))
                {
                    sb.Append(formatValue);
                }
            }
#if DEBUG
            PXTrace.WriteInformation("GI URL BUILD: {0}", sb.ToString());
#endif

            return sb.ToString();
        }

        protected virtual bool TryFormatParameter(string format, string key, string value, out string formatValue)
        {
            formatValue = null;
            if (string.IsNullOrWhiteSpace(format) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            formatValue = string.Format(format, key, value == NullValue ? string.Empty : Encode(value) );
            return true;
        }
    }
}