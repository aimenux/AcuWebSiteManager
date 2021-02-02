using System;
using System.Collections.Generic;
using PX.Data;
using CommonServiceLocator;
using PX.Objects.Extensions.MultiCurrency;

namespace PX.Objects.CM.Extensions
{
	/// <summary>
	/// Converts currencies. When attached to a Field that stores Amount in pair with BaseAmount Field automatically
	/// handles conversion and rounding when one of the fields is updated. 
	/// This class also includes static Util Methods for Conversion and Rounding.
	/// Use this Attribute for DB fields. See <see cref="PXCurrencyAttribute"/> for Non-DB version.
	/// </summary>
	/// <example>
	/// CuryUnitPrice field on the ARTran is decorated with the following attribute:
	/// [PXDBCurrency(typeof(Search<INSetup.decPlPrcCst>), typeof(ARTran.curyInfoID), typeof(ARTran.unitPrice))]
	/// Here first parameter specifies the 'Search' for precision.
	/// second parameter reference to CuryInfoID field.
	/// third parameter is the reference to unitPrice field. This field will store unitPrice is base currency.
	/// UnitPrice field will automatically be calculated and updated whenever CuryUnitPrice is modified. 
	/// /// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBCurrencyAttribute : PXDBDecimalAttribute
	{
		#region State
		internal protected Type ResultField;
		internal protected Type KeyField;
		protected Dictionary<long, string> _Matches;
		public virtual bool BaseCalc
		{
			get;
			set;
		}
		#endregion

		#region Ctor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="keyField">Field in this table used as a key for CurrencyInfo
		/// table. If 'null' is passed then the constructor will try to find field
		/// in this table named 'CuryInfoID'.</param>
		/// <param name="resultField">Field in this table to store the result of
		/// currency conversion. If 'null' is passed then the constructor will try
		/// to find field in this table name of which start with 'base'.</param>
		public PXDBCurrencyAttribute(Type keyField, Type resultField)
			: base()
		{
			ResultField = resultField;
			KeyField = keyField;
			BaseCalc = true;
		}

		public PXDBCurrencyAttribute(Type precision, Type keyField, Type resultField)
			: base(precision)
		{
			ResultField = resultField;
			KeyField = keyField;
			BaseCalc = true;
		}
		#endregion

		#region Implementation
		protected override void _ensurePrecision(PXCache sender, object row)
		{
			if (_Select is IBqlSearch && ((IBqlSearch)_Select).GetField() == typeof(CS.CommonSetup.decPlPrcCst))
			{
				_Precision = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(sender.Graph).PriceCostDecimalPlaces();
			}
			else if (_Select is IBqlSearch && ((IBqlSearch)_Select).GetField() == typeof(CS.CommonSetup.decPlQty))
			{
				_Precision = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(sender.Graph).QuantityDecimalPlaces();
			}
			else if (_Matches != null)
			{
				long? id = (long?)sender.GetValue(row, KeyField.Name);
				string cury;
				if (id != null && _Matches.TryGetValue((long)id, out cury))
				{
					_Precision = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(sender.Graph)
						.CuryDecimalPlaces(cury);
				}
				else
				{
					_Precision = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(sender.Graph)
						.CuryDecimalPlaces("");
				}
			}
			else
			{
				long? id = null;
				if (KeyField != null)
				{
					id = (long?)sender.GetValue(row, KeyField.Name);
				}
				if (sender.Graph.Accessinfo.CuryViewState)
				{
					_Precision = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(sender.Graph)
						.CuryDecimalPlaces(sender.Graph.FindImplementation<IPXCurrencyHelper>()?.GetBaseCuryID(id));
				}
				else
				{
					_Precision = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(sender.Graph)
						.CuryDecimalPlaces(sender.Graph.FindImplementation<IPXCurrencyHelper>()?.GetCuryID(id));
				}
			}
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (KeyField != null)
			{
				_Matches = CurrencyInfo.CuryIDStringAttribute.GetMatchesDictionary(sender);
			}
		}
		#endregion
	}
}
