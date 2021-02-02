using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using PX.Common;
using PX.Common.Collection;
using PX.Data;

using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CR;
using PX.Objects.Common.Abstractions;

using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Data.Maintenance.GI;
using PX.Dashboards;
using PX.Objects.DR.Descriptor;
using static PX.Objects.CM.PXCurrencyAttribute;

namespace PX.Objects.CM
{
	public class GainLossAcctSubDefault
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues
			{
				get
				{
					return _AllowedValues;
				}
			}

			public string[] AllowedLabels
			{
				get
				{
					return _AllowedLabels;
				}
			}

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels)
				: base(AllowedValues, AllowedLabels)
			{
			}
		}

		public class ListAttribute : CustomListAttribute
		{
			public ListAttribute()
				: base(new string[] { MaskCurrency, MaskCompany }, new string[] { Messages.MaskCurrency, Messages.MaskCompany })
			{
			}
		}
		public const string MaskCurrency = "N";
		public const string MaskCompany = "C";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = SubAccountAttribute.DimensionName)]
	public sealed class GainLossSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "CMSETUP";
		public GainLossSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, GainLossAcctSubDefault.MaskCurrency, new GainLossAcctSubDefault.ListAttribute().AllowedValues, new GainLossAcctSubDefault.ListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static int? GetSubID<Field>(PXGraph graph, int? BranchID, Currency currency)
			where Field : IBqlField
		{
			int? currency_SubID = (int?)graph.Caches[typeof(Currency)].GetValue<Field>(currency);

			CMSetup cmsetup = PXSelect<CMSetup>.Select(graph);

			if (cmsetup == null || string.IsNullOrEmpty(cmsetup.GainLossSubMask))
			{
				return currency_SubID;
			}

			CRLocation companyloc = PXSelectJoin<CRLocation, 
				InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>, 
				InnerJoin<Branch, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(graph, BranchID);

			int? company_SubID = (int?)graph.Caches[typeof(CRLocation)].GetValue<CRLocation.cMPGainLossSubID>(companyloc);

			object value;
			try
			{
				value = MakeSub<CMSetup.gainLossSubMask>(graph, cmsetup.GainLossSubMask,
					new object[] { currency_SubID, company_SubID },
					new Type[] { typeof(Field), typeof(Location.cMPGainLossSubID) });

				graph.Caches[typeof(Currency)].RaiseFieldUpdating<Field>(currency, ref value);
			}
			catch (PXException)
			{
				value = null;
			}

			return (int?)value;
		}

		private static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new GainLossAcctSubDefault.ListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new GainLossAcctSubDefault.ListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}


	public class PXRateIsNotDefinedForThisDateException : PXSetPropertyException
	{
		public PXRateIsNotDefinedForThisDateException(string CuryRateType, string FromCuryID, string ToCuryID, DateTime CuryEffDate)
			: base(Messages.RateIsNotDefinedForThisDateVerbose, PXErrorLevel.Warning, CuryRateType, FromCuryID, ToCuryID, CuryEffDate.ToShortDateString())
		{
		}
		public PXRateIsNotDefinedForThisDateException(CurrencyInfo info)
			: this(info.CuryRateTypeID, info.CuryID, info.BaseCuryID, (DateTime)info.CuryEffDate)
		{ 
		}
		public PXRateIsNotDefinedForThisDateException(SerializationInfo info, StreamingContext context)
			: base(info, context){}

	}

	public class PXRateNotFoundException : PXException
	{
		public PXRateNotFoundException()
			:base(Messages.RateNotFound)
		{ 
		}
		public PXRateNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}
	}

	#region CurrencyRateAttribute

	/// <summary>
	/// Manages Foreign exchage data for the document or the document detail.
	/// When used for the detail useally a reference to the parent document is passed through ParentCuryInfoID in a constructor.
	/// </summary>
	/// <example>
	/// [CurrencyInfo(ModuleCode = "AR")]  - Document declaration
	/// [CurrencyInfo(typeof(ARRegister.curyInfoID))] - Detail declaration
	/// </example>
	public class CurrencyInfoAttribute : PXAggregateAttribute, IPXRowInsertingSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber, IPXRowUpdatingSubscriber, IPXReportRequiredField, IPXDependsOnFields
	{
		#region State
		/// <summary>
		/// The negative value of the record's currency info ID field
		/// that will be assigned back to it upon transaction abort. 
		/// This field handles DB transaction abort from the record side 
		/// (<see cref="RowPersisted"/>). Compare with <see cref="_SelfKeyToAbort"/>.
		/// </summary>
		protected object _KeyToAbort;
		public const string _CuryViewField = "CuryViewState";
		protected Type _ChildType;
		protected object _oldRow = null;
		protected bool _NeedSync;
		protected string _ModuleCode;
		public string ModuleCode
		{
			get
			{
				return this._ModuleCode;
			}
			set
			{
				this._ModuleCode = value;
			}
		}

		public const string DefaultCuryIDFieldName = "CuryID";
		protected string _CuryIDField = DefaultCuryIDFieldName;
		public string CuryIDField
		{
			get
			{
				return this._CuryIDField;
			}
			set
			{
				this._CuryIDField = value;
			}
		}

		public const string DefaultCuryRateFieldName = "CuryRate";
		protected string _CuryRateField = DefaultCuryRateFieldName;
		public string CuryRateField
		{
			get
			{
				return this._CuryRateField;
			}
			set
			{
				this._CuryRateField = value;
			}
		}

		public const string DefaultCuryIDDisplayName = Messages.CuryDisplayName;
		protected string _CuryDisplayName = DefaultCuryIDDisplayName;
		public string CuryDisplayName
		{
			get
			{
				return PXMessages.LocalizeNoPrefix(this._CuryDisplayName);
			}
			set
			{
				this._CuryDisplayName = value;
			}
		}
		protected bool _Enabled = true;
		public bool Enabled
		{
			get
			{
				return _Enabled;
			}
			set
			{
				_Enabled = value;
			}
		}
		Type _ParentType = null;
		#endregion
		#region Ctor
		public CurrencyInfoAttribute()
		{
		}

		protected class CurrencyInfoDefaultAttribute : PXDBDefaultAttribute
		{
			public CurrencyInfoDefaultAttribute(Type sourceType)
				: base(sourceType)
			{
			}

			protected override void EnsureIsRestriction(PXCache sender)
			{
				if (_IsRestriction.Value == null)
				{
					_IsRestriction.Value = true;
				}
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);

				_DoubleDefaultAttribute = true;
			}
		}

		public CurrencyInfoAttribute(Type ParentCuryInfoID)
		{
			_Attributes.Add(new CurrencyInfoDefaultAttribute(ParentCuryInfoID){PersistingCheck = PXPersistingCheck.Nothing});
			_ParentType = BqlCommand.GetItemType(ParentCuryInfoID);
		}

		#endregion
		#region Implementation

		private static Type GetPrimaryType(PXGraph graph)
		{
			foreach (DictionaryEntry action in graph.Actions)
			{
				try
				{
					Type primary;
					if ((primary = ((PXAction)action.Value).GetRowType()) != null)
						return primary;
				}
				catch (Exception)
				{
				}
			}
			return null;
		}

		public static PXView GetView(PXGraph graph, Type ClassType, Type KeyField)
		{
			if (!graph.Views.Caches.Contains(ClassType))
			{
				return null;
			}

			PXView view;
			string viewname = "_" + ClassType.Name + "." + KeyField.Name + "_CurrencyInfo.CuryInfoID_";
			if (!graph.Views.TryGetValue(viewname, out view))
			{
				view = CreateSiblingsView(graph, ClassType, KeyField);
				graph.Views[viewname] = view;
			}

			return view;
		}

		public static PXView GetViewInversed(PXGraph graph, Type ClassType, Type KeyField)
		{
			PXView view;
			string viewname = "_CurrencyInfo.CuryInfoID_" + ClassType.Name + "." + KeyField.Name + "_";
			if (!graph.Views.TryGetValue(viewname, out view))
			{
				BqlCommand cmd = BqlCommand.CreateInstance(
					typeof(Select<,>),
					typeof(CurrencyInfo),
					typeof(Where<,>),
					typeof(CurrencyInfo.curyInfoID),
					typeof(Equal<>),
					typeof(Current<>),
					KeyField
					);
				view = new PXView(graph, false, cmd);
				graph.Views[viewname] = view;
			}

			return view;
		}

		public static void SetEffectiveDate<Field, CuryKeyField>(PXCache sender, PXFieldUpdatedEventArgs e)
			where Field : IBqlField
			where CuryKeyField : IBqlField
		{
			SetEffectiveDate<Field>(sender, e, typeof(CuryKeyField));
		}

		public static void SetEffectiveDate<Field>(PXCache sender, PXFieldUpdatedEventArgs e)
			where Field : IBqlField
		{
			Type curyKeyField = null;

			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is CurrencyInfoAttribute)
				{
					curyKeyField = sender.GetBqlField(attr.FieldName);
					break;
				}
			}

			SetEffectiveDate<Field>(sender, e, curyKeyField);
		}

		protected static void SetEffectiveDate<Field>(PXCache sender, PXFieldUpdatedEventArgs e, Type curyKeyField)
			where Field : IBqlField
		{
			SetEffectiveDate<Field>(sender, e.Row, curyKeyField);
		}

		public static void SetEffectiveDate<Field>(PXCache sender, object row, Type curyKeyField)
			where Field : IBqlField
		{
			if (curyKeyField != null)
			{
				PXView view = GetViewInversed(sender.Graph, BqlCommand.GetItemType(curyKeyField), curyKeyField);

				foreach (CurrencyInfo info in view.SelectMulti())
				{
					PXCache cache = view.Cache;
					object value = sender.GetValue<Field>(row);

					CurrencyInfo copy = PXCache<CurrencyInfo>.CreateCopy(info);
					cache.SetValueExt<CurrencyInfo.curyEffDate>(info, value);
					string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(cache, info);
					if (string.IsNullOrEmpty(message) == false)
					{
						sender.RaiseExceptionHandling<Field>(row, null, new PXSetPropertyException(message, PXErrorLevel.Warning));
					}

					cache.RaiseRowUpdated(info, copy);

					if (cache.GetStatus(info) != PXEntryStatus.Inserted)
					{
						cache.SetStatus(info, PXEntryStatus.Updated);
					}
				}
			}
		}

		bool _InternalCall = false;
		public virtual void CurrencyInfo_CuryRate_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (e.Exception is PXSetPropertyException && _InternalCall == false)
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				foreach (object item in cache.Inserted)
				{
					if ((long?)cache.GetValue(item, _FieldOrdinal) == (long?)_KeyToAbort)
					{
						_InternalCall = true;
						try
						{
							sender.RaiseExceptionHandling<CurrencyInfo.sampleCuryRate>(e.Row, e.NewValue, e.Exception);
						}
						finally
						{
							_InternalCall = false;
						}

						cache.RaiseExceptionHandling(_CuryIDField, item, cache.GetValue(item, _CuryIDField), e.Exception);
					}
				}

				foreach (object item in cache.Updated)
				{
					if ((long?)cache.GetValue(item, _FieldOrdinal) == (long?)_KeyToAbort)
					{
						_InternalCall = true;
						try
						{
							sender.RaiseExceptionHandling<CurrencyInfo.sampleCuryRate>(e.Row, e.NewValue, e.Exception);
						}
						finally
						{
							_InternalCall = false;
						}

						cache.RaiseExceptionHandling(_CuryIDField, item, cache.GetValue(item, _CuryIDField), e.Exception);
					}
				}
			}
		}

		/// <summary>
		/// The negative value of the <see cref="CurrencyInfo.CuryInfoID"/>
		/// that will be assigned back to the record's currency info field
		/// upon transaction abort. This field handles DB transaction abort
		/// from the CurrencyInfo side (<see cref="CurrencyInfo_RowPersisted"/>). 
		/// Compare with <see cref="_KeyToAbort"/>.
		/// </summary>
		object _SelfKeyToAbort;
		Dictionary<long?, object> _persisted;

		public virtual void CurrencyInfo_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_SelfKeyToAbort = sender.GetValue<CurrencyInfo.curyInfoID>(e.Row);
			}
		}

		public virtual void CurrencyInfo_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[_ChildType];
			long? NewKey;

			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open && _SelfKeyToAbort != null)
			{
				NewKey = (long?)sender.GetValue<CurrencyInfo.curyInfoID>(e.Row);

				if (!_persisted.ContainsKey(NewKey))
				{
					_persisted.Add(NewKey, _SelfKeyToAbort);
				}

				foreach (object item in cache.Inserted)
				{
					if ((long?)cache.GetValue(item, _FieldOrdinal) == (long?)_SelfKeyToAbort)
					{
						cache.SetValue(item, _FieldOrdinal, NewKey);
					}
				}

				foreach (object item in cache.Updated)
				{
					if ((long?)cache.GetValue(item, _FieldOrdinal) == (long?)_SelfKeyToAbort)
					{
						cache.SetValue(item, _FieldOrdinal, NewKey);
					}
				}

				_SelfKeyToAbort = null;
			}

			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
			{
				Dictionary<long?, object> aborted = new Dictionary<long?, object>();
				
				foreach (object item in cache.Inserted)
				{
					if ((NewKey = (long?)cache.GetValue(item, _FieldOrdinal)) != null && _persisted.TryGetValue(NewKey, out _SelfKeyToAbort))
					{
						cache.SetValue(item, _FieldOrdinal, _SelfKeyToAbort);
						if (!aborted.ContainsKey(NewKey))
						{
							aborted.Add(NewKey, _SelfKeyToAbort);
						}
					}
				}

				foreach (object item in cache.Updated)
				{
					if ((NewKey = (long?)cache.GetValue(item, _FieldOrdinal)) != null && _persisted.TryGetValue(NewKey, out _SelfKeyToAbort))
					{
						cache.SetValue(item, _FieldOrdinal, _SelfKeyToAbort);
						if (!aborted.ContainsKey(NewKey))
						{
							aborted.Add(NewKey, _SelfKeyToAbort);
						}
					}
				}

				int count = aborted.Count;
				if (count > 0)
				{
					foreach (CurrencyInfo info in sender.Inserted)
					{
						object key;
						if (aborted.TryGetValue(info.CuryInfoID, out key))
						{
							sender.SetValue<CurrencyInfo.curyInfoID>(info, key);
							if (--count == 0) break;
						}
					}
				}
			}

			if (e.TranStatus == PXTranStatus.Completed)
			{
				NewKey = sender.GetValue<CurrencyInfo.curyInfoID>(e.Row) as long?;
				_persisted.Remove(NewKey);
			}
		}


		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];
			object key;
			if ((key = sender.GetValue(e.Row, _FieldOrdinal)) == null)
			{
				CurrencyInfo info = new CurrencyInfo();
				if (!String.IsNullOrEmpty(_ModuleCode))
				{
					info.ModuleCode = _ModuleCode;
				}
				info = (CurrencyInfo)cache.Insert(info);
				cache.IsDirty = false;
				if (info != null)
				{
					sender.SetValue(e.Row, _FieldOrdinal, info.CuryInfoID);
					if (_NeedSync)
					{
						sender.SetValue(e.Row, _CuryIDField, info.CuryID);
					}
				}
			}
			else if (_NeedSync)
			{
				CurrencyInfo info = null;
				//Normalize() is called in RowPersisted() of CurrencyInfo, until it Locate() will return null and Select() will place additional copy of CurrencyInfo in _Items.
				foreach (CurrencyInfo cached in cache.Inserted)
				{
					if (object.Equals(cached.CuryInfoID, key))
					{
						info = cached;
						break;
					}
				}

				if (info == null)
				{
					info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(sender.Graph, key);
				}
				if (info != null)
				{
					sender.SetValue(e.Row, _CuryIDField, info.CuryID);
				}
			}
		}
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object key = sender.GetValue(e.Row, _FieldOrdinal);
			if (key != null)
			{
				PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];
				if (Convert.ToInt64(key) < 0)
				{
					foreach (CurrencyInfo data in cache.Inserted)
					{
						if (object.Equals(key, data.CuryInfoID))
						{
							_KeyToAbort = sender.GetValue(e.Row, _FieldOrdinal);
							cache.PersistInserted(data);
							long id = Convert.ToInt64(PXDatabase.SelectIdentity());
							if (id == 0)
							{
								throw new PXException(Messages.CurrencyInfoNotSaved, sender.GetItemType().Name);
							}
							sender.SetValue(e.Row, _FieldOrdinal, id);
							data.CuryInfoID = id;
                            cache.Normalize();
							break;
						}
					}
				}
				else
				{
					foreach (CurrencyInfo data in cache.Updated)
					{
						if (object.Equals(key, data.CuryInfoID))
						{
							cache.PersistUpdated(data);
							break;
						}
					}
				}
			}
		}
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus != PXTranStatus.Open)
			{
				PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];
				if (e.TranStatus == PXTranStatus.Aborted)
				{
					if (_KeyToAbort != null)
					{
						object key = sender.GetValue(e.Row, _FieldOrdinal);
						sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
						foreach (CurrencyInfo data in cache.Inserted)
						{
							if (object.Equals(key, data.CuryInfoID))
							{
								cache.RaiseRowPersisted(data, PXDBOperation.Insert, PXTranStatus.Aborted, e.Exception);
								data.CuryInfoID = Convert.ToInt64(_KeyToAbort);
								cache.ResetPersisted(data);
							}
						}
					}
					else
					{
						object key = sender.GetValue(e.Row, _FieldOrdinal);
						foreach (CurrencyInfo data in cache.Updated)
						{
							if (object.Equals(key, data.CuryInfoID))
							{
								cache.ResetPersisted(data);
							}
						}
					}
				}
				else
				{
					object key = sender.GetValue(e.Row, _FieldOrdinal);
					foreach (CurrencyInfo data in cache.Inserted)
					{
						if (object.Equals(key, data.CuryInfoID))
						{
							cache.SetStatus(data, PXEntryStatus.Notchanged);
							cache.RaiseRowPersisted(data, PXDBOperation.Insert, e.TranStatus, e.Exception);
							PXTimeStampScope.PutPersisted(cache, data, sender.Graph.TimeStamp);
							cache.ResetPersisted(data);
						}
					}
					foreach (CurrencyInfo data in cache.Updated)
					{
						if (object.Equals(key, data.CuryInfoID))
						{
							cache.SetStatus(data, PXEntryStatus.Notchanged);
							cache.RaiseRowPersisted(data, PXDBOperation.Update, e.TranStatus, e.Exception);
							PXTimeStampScope.PutPersisted(cache, data, sender.Graph.TimeStamp);
							cache.ResetPersisted(data);
						}
					}
					cache.IsDirty = false;
				}

				_KeyToAbort = null;

				cache.Normalize();
			}
		}
		protected virtual void curyViewFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnValue = sender.Graph.Accessinfo.CuryViewState;
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(Boolean), false, null, -1, null, null, null, _CuryViewField, null, null, null, PXErrorLevel.Undefined, true, true, null, PXUIVisibility.Visible, null, null, null);
			}
		}
		public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item && !sender.IsDirty)
			{
				_oldRow = e.Row;
			}

			PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];
			object key;
			if ((key = sender.GetValue(e.NewRow, _FieldOrdinal)) != null && Convert.ToInt64(key) < 0L)
			{
				bool found = false;
				foreach (CurrencyInfo cached in cache.Inserted)
				{
					if (object.Equals(cached.CuryInfoID, key))
					{
						found = true;
						break;
					}
				}

				//when populatesavedvalues is called in ExecuteSelect we can sometimes endup here
				if (!found)
				{
					sender.SetValue(e.Row, _FieldOrdinal, null);
					key = null;
				}
			}

			if (key == null)
			{
				CurrencyInfo info = new CurrencyInfo();
				if (!String.IsNullOrEmpty(_ModuleCode))
				{
					info.ModuleCode = _ModuleCode;
				}
				info = (CurrencyInfo)cache.Insert(info);
				cache.IsDirty = false;
				if (info != null)
				{
					sender.SetValue(e.NewRow, _FieldOrdinal, info.CuryInfoID);
					if (_NeedSync)
					{
						sender.SetValue(e.NewRow, _CuryIDField, info.CuryID);
					}
				}
			}
		}
		protected virtual void curyRateFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			bool curyviewstate = sender.Graph.Accessinfo.CuryViewState;
			PXView view = sender.Graph.Views["_" + sender.GetItemType().Name + "_CurrencyInfo_" + (_FieldName == "CuryInfoID" ? "" : _FieldName + "_")];
			long? key = (long?)sender.GetValue(e.Row, _FieldOrdinal);

			if (key == null || _ParentType != null && key < 0L)
			{
				object defaultValue;
				sender.RaiseFieldDefaulting(_FieldName, e.Row, out defaultValue);

				if (defaultValue != null)
					key = (long?)defaultValue;
			}

			CurrencyInfo info = null;

			if (key != null)
			{
				info = view.Cache.Current as CurrencyInfo;

				if (info != null)
				{
					if (!object.Equals(info.CuryInfoID, key))
					{
						info = new CurrencyInfo();
						info.CuryInfoID = key;
						info = view.Cache.Locate(info) as CurrencyInfo;

						if (info == null)
						{
							info = view.SelectSingle(key) as CurrencyInfo;
						}
					}
				}
				else
				{
					info = new CurrencyInfo();
					info.CuryInfoID = key;
					info = view.Cache.Locate(info) as CurrencyInfo;

					if (info == null)
					{
						info = view.SelectSingle(key) as CurrencyInfo;
					}
				}

				if (info != null)
				{
					if (!curyviewstate)
					{
						e.ReturnValue = info.SampleCuryRate;
					}
					else
					{
						e.ReturnValue = 1m;
					}
				}
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered || curyviewstate)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(decimal), false, null, -1, null, 5, null, _CuryRateField, null, null, null,
															PXErrorLevel.Undefined, _Enabled, true, null, PXUIVisibility.Visible, null, null, null);
				if (curyviewstate)
				{
					((PXFieldState)e.ReturnState).Enabled = false;
				}
			}
		}
		protected virtual void curyIdFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (sender.Graph.GetType() == typeof(PXGenericInqGrph) || sender.Graph.GetType() == typeof(PXGraph))
				return;

			bool curyviewstate = sender.Graph.Accessinfo.CuryViewState;
			PXView view = sender.Graph.Views["_" + sender.GetItemType().Name + "_CurrencyInfo_" + (_FieldName == "CuryInfoID" ? "" : _FieldName + "_")];
			long? key = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			if (key == null || _ParentType != null && key < 0L)
			{
				object defaultValue;
				sender.RaiseFieldDefaulting(_FieldName, e.Row, out defaultValue);
				if (defaultValue != null)
					key = (long?)defaultValue;
			}
			CurrencyInfo info = null;
			if (key != null)
			{
				info = view.Cache.Current as CurrencyInfo;
				if (info != null)
				{
					if (!object.Equals(info.CuryInfoID, key))
					{
						info = new CurrencyInfo();
						info.CuryInfoID = key;
						info = view.Cache.Locate(info) as CurrencyInfo;
						if (info == null)
						{
							info = view.SelectSingle(key) as CurrencyInfo;
						}
					}
				}
				else
				{
					info = new CurrencyInfo();
					info.CuryInfoID = key;
					info = view.Cache.Locate(info) as CurrencyInfo;
					if (info == null)
					{
						info = view.SelectSingle(key) as CurrencyInfo;
					}
				}
				if (info != null)
				{
					if (!curyviewstate)
					{
						e.ReturnValue = info.CuryID;
					}
					else
					{
						e.ReturnValue = info.BaseCuryID;
					}
				}
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered || curyviewstate)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(string), false, null, -1, null, 5, null, _CuryIDField, null, CuryDisplayName, null, PXErrorLevel.Undefined, _Enabled, true, null, PXUIVisibility.Visible, null, null, null);
				if (curyviewstate)
				{
					((PXFieldState)e.ReturnState).Enabled = false;
				}
			}
		}
		protected virtual void curyIdFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (sender.Graph.Accessinfo.CuryViewState)
			{
				e.NewValue = sender.GetValue(e.Row, _CuryIDField);
				return;
			}
			PXView view = sender.Graph.Views["_" + sender.GetItemType().Name + "_CurrencyInfo_" + (_FieldName == "CuryInfoID" ? "" : _FieldName + "_")];
			CurrencyInfo info = null;
			long? key = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			if (key != null)
			{
				info = view.SelectSingle(key) as CurrencyInfo;
			}
			if (info != null && !object.Equals(info.CuryID, e.NewValue))
			{
				CurrencyInfo old = PXCache<CurrencyInfo>.CreateCopy(info);
				view.Cache.SetValueExt<CurrencyInfo.curyID>(info, e.ExternalCall ? new PXCache.ExternalCallMarker(e.NewValue) : e.NewValue);
				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyID>(view.Cache, info);
				if (string.IsNullOrEmpty(message) == false)
				{
					sender.RaiseExceptionHandling(_CuryIDField, e.Row, e.NewValue, new PXSetPropertyException(message, PXErrorLevel.Warning));
				}

				view.Cache.MarkUpdated(info);
				view.Cache.RaiseRowUpdated(info, old);
			}
		}
		#endregion

		#region Runtime
		public static string GetCuryID(PXCache cache, object row, string keyField)
		{
			foreach (CurrencyInfoAttribute attr in cache.GetAttributesReadonly(row, keyField).OfType<CurrencyInfoAttribute>())
			{
				if (attr._NeedSync)
				{
					return (string)cache.GetValue(row, attr._CuryIDField);
				}
				return null;
			}
			return null;
		}
		public static object GetOldRow(PXCache cache, object item)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(item, null))
			{
				if (attr is CurrencyInfoAttribute)
				{
					return ((CurrencyInfoAttribute)attr)._oldRow;
				}
			}
			return null;
		}
		public static CurrencyInfo SetDefaults<Field>(PXCache cache, object item)
			where Field : IBqlField
		{
		    return SetDefaults<Field>(cache, item, cache.GetItemType());
		}
	    public static CurrencyInfo SetDefaults<Field>(PXCache cache, object item, Type viewType)
	        where Field : IBqlField
	    {
	        string viewname = "_" + viewType.Name + "_CurrencyInfo_";
	        PXView view = null;
	        cache.Graph.Views.TryGetValue(viewname, out view);
	        if (view == null)
	        {
	            viewname += typeof(Field).Name + "_";
	            view = cache.Graph.Views[viewname];
	        }
	        CurrencyInfo info = view.SelectSingle(cache.GetValue(item, typeof(Field).Name)) as CurrencyInfo;
	        if (info != null)
	        {
	            CurrencyInfo old = PXCache<CurrencyInfo>.CreateCopy(info);
	            view.Cache.SetDefaultExt<CurrencyInfo.curyID>(info);
	            view.Cache.SetDefaultExt<CurrencyInfo.curyRateTypeID>(info);
	            view.Cache.SetDefaultExt<CurrencyInfo.curyEffDate>(info);
				view.Cache.MarkUpdated(info);
	            view.Cache.RaiseRowUpdated(info, old);
	        }
	        return info;
	    }
        #endregion

        #region Initialization
        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (_Attributes.Count > 0 && (typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber) || typeof(ISubscriber) == typeof(IPXRowPersistedSubscriber)))
			{
				subscribers.Add(_Attributes[0] as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			_ChildType = sender.GetItemType();
			sender.Graph.Views["_" + sender.GetItemType().Name + "_CurrencyInfo_" + (_FieldName == "CuryInfoID" ? "" : _FieldName + "_")] = new CurrencyInfoView(sender.Graph, this);

			if (!CompareIgnoreCase.IsInList(sender.Fields, _CuryIDField))
			{
				sender.Fields.Add(_CuryIDField);
			}
			else if (sender.GetFieldOrdinal(_CuryIDField) < 0)
			{
				throw new PXArgumentException(nameof(CuryIDField), ErrorMessages.InvalidField, _CuryIDField);
			}
			else
			{
				_NeedSync = true;
			}

			if (!CompareIgnoreCase.IsInList(sender.Fields, _CuryRateField))
			{
				sender.Fields.Add(_CuryRateField);
			}

			if (!CompareIgnoreCase.IsInList(sender.Fields, _CuryViewField))
			{
				sender.Fields.Add(_CuryViewField);
			}

			sender.Graph.FieldSelecting.AddHandler(_ChildType, _CuryRateField, curyRateFieldSelecting);
			sender.Graph.FieldSelecting.AddHandler(_ChildType, _CuryIDField, curyIdFieldSelecting);
			sender.Graph.FieldVerifying.AddHandler(_ChildType, _CuryIDField, curyIdFieldVerifying);
			sender.Graph.FieldSelecting.AddHandler(_ChildType, _CuryViewField, curyViewFieldSelecting);

			if (_Attributes.Count == 0)
			{
				sender.Graph.RowPersisting.AddHandler<CurrencyInfo>(CurrencyInfo_RowPersisting);
				sender.Graph.RowPersisted.AddHandler<CurrencyInfo>(CurrencyInfo_RowPersisted);
				sender.Graph.ExceptionHandling.AddHandler<CurrencyInfo.sampleCuryRate>(CurrencyInfo_CuryRate_ExceptionHandling);
			}
			else if (_NeedSync)
			{
				sender.Graph.RowUpdated.AddHandler(_ParentType, (PXRowUpdated)delegate(PXCache cache, PXRowUpdatedEventArgs e)
				{
					object val = cache.GetValue(e.Row, _CuryIDField);
					string parentCuriInfoFieldName = GetAttributes().OfType<CurrencyInfoDefaultAttribute>().Select(currencyDefaultAttribute => currencyDefaultAttribute.FieldName).FirstOrDefault();
					Int64? parentCuriInfoID = (Int64?)cache.GetValue(e.Row, parentCuriInfoFieldName);
					if (!object.Equals(val, cache.GetValue(e.OldRow, _CuryIDField)))
					{
						foreach (object item in PXParentAttribute.SelectSiblings(sender, null, _ParentType))
						{
							Int64? curiInfoID = (Int64?)sender.GetValue(item, FieldName);

							if (parentCuriInfoID != curiInfoID)
								continue;

							sender.SetValueExt(item, _CuryIDField, val);

							sender.MarkUpdated(item);
						}
					}
				});
			}

			if (sender.Graph.GetType() == typeof(PXGenericInqGrph) || sender.Graph.GetType() == typeof(PXGraph))
			{
				sender.Graph.RowSelected.AddHandler<CurrencyInfo>((cache, e) =>
					{
						cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
					});
			}

			base.CacheAttached(sender);
			_persisted = new Dictionary<long?, object>();
		}
		#endregion

		#region SkipReadOnly
		/// <summary>
		/// Skip setting <see cref="CurrencyInfo.IsReadOnly"/> in <see cref="CurrencyInfoView.Select"/>
		/// </summary>
		public class SkipReadOnly : IDisposable
		{
			private CurrencyInfoView _view;

			public SkipReadOnly(PXView view)
			{
				_view = view as CurrencyInfoView;
				if (_view != null)
					_view.skipReadOnly = true;
			}

			void IDisposable.Dispose()
			{
				if (_view != null)
					_view.skipReadOnly = false;
			}
		}
		#endregion

		#region View
		private sealed class CurrencyInfoView : PXView
		{
			private CurrencyInfoAttribute _Owner;
			private PXView _innerView;
			public bool skipReadOnly = false;

			public CurrencyInfoView(PXGraph graph, CurrencyInfoAttribute owner)
				: base(graph, false, new Select<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Optional<CurrencyInfo.curyInfoID>>>>())
			{
				_Owner = owner;
				_innerView = new PXView(graph, false, new Select<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Optional<CurrencyInfo.curyInfoID>>>>());
			}
			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				searches = null;
				PXCache cache = _Graph.Caches[_Owner._ChildType];
				if (parameters == null || parameters.Length == 0 || parameters[0] == null)
				{
					object row = cache.InternalCurrent;
					if (currents != null)
					{
						for (int i = 0; i < currents.Length; i++)
						{
							if (currents[i] != null && (currents[i].GetType() == _Owner._ChildType || currents[i].GetType().IsSubclassOf(_Owner._ChildType)))
							{
								row = currents[i];
								break;
							}
						}
					}
					parameters = new object[1];
					if (row != null)
					{
						parameters[0] = cache.GetValue(row, _Owner._FieldOrdinal);
					}
					if (parameters[0] == null)
					{
						if (cache.RaiseFieldDefaulting(_Owner._FieldName, null, out parameters[0]))
						{
							cache.RaiseFieldUpdating(_Owner._FieldName, null, ref parameters[0]);
						}
					}
				}
				List<object> ret = null;
				foreach (CurrencyInfo info in Cache.Cached)
				{
					if (object.Equals(info.CuryInfoID, parameters[0]))
					{
						ret = new List<object>();
						ret.Add(info);
						break;
					}
				}
				if (ret == null)
				{
					//if parent type is defined then record is probably committed to the database and has positive identity
					//while parameter is still negative
					//resetting to null will default parameter from parent cache which is already positive
					if (parameters[0] != null && parameters[0] is long && (long)parameters[0] < 0L && _Owner._ParentType != null)
					{
						parameters[0] = null;
						currents = null;

						if (cache.RaiseFieldDefaulting(_Owner._FieldName, null, out parameters[0]))
						{
							cache.RaiseFieldUpdating(_Owner._FieldName, null, ref parameters[0]);
						}
					}

					ret = _innerView.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
					if (parameters[0] != null && ret.Count == 0)
					{
						// it's a temporary fix for the situation when the CurrencyInfo is persisted but the PXTransactionScope is not committed yet
						// and the system tries to select the persisted record from the separate PXConnectionScope
						// TODO: review after the fix of AC-108271
						long curyInfoID = long.MinValue;
						if (parameters.Length == 1 && parameters[0] is long)
						{
							curyInfoID = (long)parameters[0];
						}
						if (curyInfoID > 0 && PXTransactionScope.IsScoped)
						{
							_innerView.RemoveCached(new PXCommandKey(new object[] { curyInfoID }, singleRow: true));
						}
					}
				}
				if (ret.Count > 0)
				{
					CurrencyInfo info = (CurrencyInfo)ret[0];
					info.ModuleCode = _Owner._ModuleCode;
					if (!skipReadOnly)
					{
					info.IsReadOnly = (!Graph.UnattendedMode && !_Graph.Caches[_Owner._ChildType].AllowUpdate);
				}
				}
				return ret;
			}
		}

		private sealed class SiblingsView : PXView
		{
			private static BqlCommand GetCommand(PXGraph graph, Type ClassType, Type KeyField)
			{
				BqlCommand cmd = null;
				Type primary = GetPrimaryType(graph);
			    if (KeyField.DeclaringType != ClassType)
			        KeyField = ClassType.GetNestedType(KeyField.Name, BindingFlags.Public) ?? KeyField;
				if (primary != null)
				{
					foreach (KeyValuePair<string, PXView> kv in graph.Views)
					{
						if (kv.Value.GetItemType() == ClassType && kv.Value.IsReadOnly == false)
						{
							bool found = false;
							IBqlParameter[] pars = kv.Value.BqlSelect.GetParameters();

							for (int i = 0; i < pars.Length; i++)
							{
								if (pars[i].IsVisible == false && pars[i].HasDefault == true)
								{
									Type rt = pars[i].GetReferencedType();
									if (rt.IsNested && BqlCommand.GetItemType(rt) == primary)
									{
										found = true;
									}
								}
								else
								{
									found = false;
									break;
								}
							}

							if (found)
							{
								cmd = kv.Value.BqlSelect.WhereAnd(typeof(Where<,>).MakeGenericType(KeyField, typeof(Equal<Current<CurrencyInfo.curyInfoID>>)));
								break;
							}
						}
					}
				}

				if (cmd == null)
				{
					cmd = BqlCommand.CreateInstance(typeof(Select<,>), ClassType, typeof(Where<,>), KeyField, typeof(Equal<Current<CurrencyInfo.curyInfoID>>));
				}

				return cmd;
			}

			public SiblingsView(PXGraph graph, Type ClassType, Type KeyField)
				: base(graph, false, GetCommand(graph, ClassType, KeyField))
			{
			}

			/*
			private void Parameter_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
			{
				long? Key;
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && (e.Operation & PXDBOperation.Option) != PXDBOperation.External &&
					(e.Operation & PXDBOperation.Option) != PXDBOperation.ReadOnly && e.Row == null && (Key = e.Value as long?) != null)
				{
					if (Key < 0L)
					{
						e.DataValue = null;
						e.Cancel = true;
					}
				}
			}

			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				this._Graph.CommandPreparing.AddHandler<CurrencyInfo.curyInfoID>(Parameter_CommandPreparing);

				try
				{
					return base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
				}
				finally
				{
					this._Graph.CommandPreparing.RemoveHandler<CurrencyInfo.curyInfoID>(Parameter_CommandPreparing);
				}
			}
			*/
		}

		private static PXView CreateSiblingsView(PXGraph graph, Type classType, Type keyField)
		{
			BqlCommand cmd = null;
			Type primary = GetPrimaryType(graph);
			if (primary != null)
			{
				foreach (KeyValuePair<string, PXView> kv in graph.Views)
				{
					if (kv.Value.GetItemType() == classType && kv.Value.IsReadOnly == false)
					{
						bool found = false;
						IBqlParameter[] pars = kv.Value.BqlSelect.GetParameters();

						for (int i = 0; i < pars.Length; i++)
						{
							if (pars[i].IsVisible == false && pars[i].HasDefault == true)
							{
								Type rt = pars[i].GetReferencedType();
								if (rt.IsNested && BqlCommand.GetItemType(rt) == primary)
								{
									found = true;
								}
							}
							else
							{
								found = false;
								break;
							}
						}

						if (found)
						{
							cmd = kv.Value.BqlSelect.WhereAnd(typeof(Where<,>).MakeGenericType(keyField, typeof(Equal<Current<CurrencyInfo.curyInfoID>>)));
							break;
						}
					}
				}
			}

			if (cmd == null)
			{
				cmd = BqlCommand.CreateInstance(typeof(Select<,>), classType, typeof(Where<,>), keyField, typeof(Equal<Current<CurrencyInfo.curyInfoID>>));
			}

			return new PXView(graph, false, cmd);
		}

		#endregion

		public ISet<Type> GetDependencies(PXCache cache)
		{
			var res = new HashSet<Type>();
			var field = cache.GetBqlField(_CuryIDField);
			if (field != null) res.Add(field);
			return res;
		}
	}

	#endregion

	public enum CMPrecision
	{
		TRANCURY = 0,
		BASECURY = 1,
		CUSTOM = 2,
	}

	#region PXCurrencyAttribute
	/// <summary>
	/// Converts currencies. When attached to a Field that stores Amount in pair with BaseAmount Field automatically
	/// handles conversion and rounding when one of the fields is updated. 
	/// This class also includes static Util Methods for Conversion and Rounding.
	/// Use this Attribute for Non DB fields. See <see cref="PXDBCurrencyAttribute"/> for DB version.
	/// <example>
	/// CuryDiscPrice field on the SOLine is decorated with the following attribute:
	/// [PXCurrency(typeof(Search<INSetup.decPlPrcCst>), typeof(SOLine.curyInfoID), typeof(SOLine.discPrice))]
	/// Here first parameter specifies the 'Search' for precision.
	/// second parameter reference to CuryInfoID field.
	/// third parameter is the reference to discPrice (which is also NON-DB) field. This field will store discPrice is base currency.
	/// DiscPrice field will automatically be calculated and updated whenever CuryDiscPrice is modified. 
	/// /// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXCurrencyAttribute : PXDecimalAttribute, IPXFieldVerifyingSubscriber, IPXRowInsertingSubscriber
	{
		#region State
		internal PXCurrencyHelper _helper;
		protected bool _FixedPrec = false;
		protected override Int32? Precision => _helper.Precision;
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
		public PXCurrencyAttribute(Type keyField, Type resultField)
		{
			_helper = new PXCurrencyHelper(keyField, resultField, _FieldName, _FieldOrdinal, _Precision, _AttributeLevel);
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
		public PXCurrencyAttribute(int precision, Type keyField, Type resultField)
			: base(precision)
		{
			_helper = new PXCurrencyHelper(keyField, resultField, _FieldName, _FieldOrdinal, _Precision, _AttributeLevel);
		}

		public PXCurrencyAttribute(Type precision, Type keyField, Type resultField)
			: base(precision)
		{
			_helper = new PXCurrencyHelper(keyField, resultField, _FieldName, _FieldOrdinal, _Precision, _AttributeLevel);
			this._FixedPrec = true;
		}

		#endregion

		#region Runtime
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.SetAltered(_FieldName, true);
			_helper.CacheAttached(sender);
			sender.Graph.FieldUpdating.AddHandler(BqlCommand.GetItemType(_helper.ResultField),
				_helper.ResultField.Name,
			  ResultFieldUpdating);

			PXDecimalAttribute.SetPrecision(sender, _helper.ResultField.Name, FixedPrec ? _Precision : null);
			if (!FixedPrec)
			{
				sender.SetAltered(_helper.ResultField.Name, true);
				sender.Graph.FieldSelecting.AddHandler(BqlCommand.GetItemType(_helper.ResultField),
					_helper.ResultField.Name,
				  ResultFieldSelecting);
			}
		}
		#endregion

		#region Implementation

		public bool FixedPrec
		{
			get
			{
				return _FixedPrec;
			}
		}
		public virtual bool BaseCalc
		{
			get
			{
				return _helper.BaseCalc;
			}
			set
			{
				_helper.BaseCalc = value;
			}
		}

		public override int FieldOrdinal
		{
			get
			{
				return base.FieldOrdinal;
			}
			set
			{
				base.FieldOrdinal = value;
				_helper.FieldOrdinal = value;
			}
		}

		public override string FieldName
		{
			get
			{
				return base.FieldName;
			}
			set
			{
				base.FieldName = value;
				_helper.FieldName = value;
			}
		}

		public override PXEventSubscriberAttribute Clone(PXAttributeLevel attributeLevel)
		{
			PXCurrencyAttribute attr = (PXCurrencyAttribute)base.Clone(attributeLevel);
			attr._helper = this._helper.Clone(attributeLevel);
			return attr;
		}

		public static bool IsNullOrEmpty(decimal? val)
		{
			return (val == null || val == decimal.Zero);
		}

		public static void CuryConvCury(PXCache sender, object row, decimal baseval, out decimal curyval)
		{
			PXCurrencyHelper.CuryConvCury(sender, row, baseval, out curyval);
		}

		public static void CuryConvCury<CuryField>(PXCache sender, object row)
			where CuryField : IBqlField
		{
			PXCurrencyHelper.CuryConvCury<CuryField>(sender, row);
		}

		public static void CuryConvCury<InfoKeyField>(PXCache sender, object row, decimal baseval, out decimal curyval, bool skipRounding)
			where InfoKeyField : IBqlField
		{
			PXCurrencyHelper.CuryConvCury<InfoKeyField>(sender, row, baseval, out curyval, skipRounding);
		}

		public static void CuryConvCury<InfoKeyField>(PXCache sender, object row, decimal baseval, out decimal curyval)
			where InfoKeyField : IBqlField
		{
			PXCurrencyHelper.CuryConvCury<InfoKeyField>(sender, row, baseval, out curyval);
		}

		public static void CuryConvCury<InfoKeyField>(PXCache sender, object row, decimal baseval, out decimal curyval, int precision)
			where InfoKeyField : IBqlField
		{
			PXCurrencyHelper.CuryConvCury<InfoKeyField>(sender, row, baseval, out curyval, precision);
		}

		public static void CuryConvCury(PXCache cache, CurrencyInfo info, decimal baseval, out decimal curyval)
		{
			PXCurrencyHelper.CuryConvCury(cache, info, baseval, out curyval);
		}

		public static void CuryConvCury(PXCache cache, CurrencyInfo info, decimal baseval, out decimal curyval, bool skipRounding)
		{
			PXCurrencyHelper.CuryConvCury(cache, info, baseval, out curyval, skipRounding);
		}

		public static void CuryConvCury(PXCache sender, object row, decimal baseval, out decimal curyval, bool skipRounding)
		{
			PXCurrencyHelper.CuryConvCury(sender, row, baseval, out curyval, skipRounding);
		}

		public static void CuryConvCury(PXCache sender, object row, decimal baseval, out decimal curyval, int precision)
		{
			PXCurrencyHelper.CuryConvCury(sender, row, baseval, out curyval, precision);
		}

		public static void CuryConvBase(PXCache sender, object row, decimal curyval, out decimal baseval)
		{
			PXCurrencyHelper.CuryConvBase(sender, row, curyval, out baseval);
		}

		public static void CuryConvBase<InfoKeyField>(PXCache sender, object row, decimal curyval, out decimal baseval)
			where InfoKeyField : IBqlField
		{
			PXCurrencyHelper.CuryConvBase<InfoKeyField>(sender, row, curyval, out baseval);
		}

		public static void CuryConvBase<InfoKeyField>(PXCache sender, object row, decimal curyval, out decimal baseval, int precision)
			where InfoKeyField : IBqlField
		{
			PXCurrencyHelper.CuryConvBase<InfoKeyField>(sender, row, curyval, out baseval, precision);
		}

		public static void CuryConvBase(PXCache cache, CurrencyInfo info, decimal curyval, out decimal baseval)
		{
			PXCurrencyHelper.CuryConvBase(cache, info, curyval, out baseval);
		}

		public static void CuryConvBase(PXCache cache, CurrencyInfo info, decimal curyval, out decimal baseval, bool skipRounding)
		{
			PXCurrencyHelper.CuryConvBase(cache, info, curyval, out baseval, skipRounding);
		}

		public static void CuryConvBase(PXCache sender, object row, decimal curyval, out decimal baseval, bool skipRounding)
		{
			PXCurrencyHelper.CuryConvBase(sender, row, curyval, out baseval, skipRounding);
		}

		public static void CuryConvBase(PXCache sender, object row, decimal baseval, out decimal curyval, int precision)
		{
			PXCurrencyHelper.CuryConvBase(sender, row, baseval, out curyval, precision);
		}

		public static decimal BaseRound(PXGraph graph, decimal value)
		{
			return PXCurrencyHelper.BaseRound(graph, value);
		}

		public static decimal BaseRound(PXGraph graph, decimal? value)
		{
			return BaseRound(graph, (decimal)value);
		}

		public static decimal Round(PXCache sender, object row, decimal val, CMPrecision prec)
		{
			return PXCurrencyHelper.Round(sender, row, val, prec);
		}

		public static decimal Round(PXCache sender, object row, decimal val, CMPrecision prec, int customPrecision)
		{
			return PXCurrencyHelper.Round(val, customPrecision);
		}

		public static decimal Round<InfoKeyField>(PXCache sender, object row, decimal val, CMPrecision prec)
			where InfoKeyField : IBqlField
		{
			return PXCurrencyHelper.Round<InfoKeyField>(sender, row, val, prec);
		}

		public static decimal RoundCury(PXCache sender, object row, decimal val)
		{
			return PXCurrencyHelper.Round(sender, row, val, CMPrecision.TRANCURY);
		}

		public static decimal RoundCury<InfoKeyField>(PXCache sender, object row, decimal val)
			where InfoKeyField : IBqlField
		{
			return PXCurrencyHelper.Round<InfoKeyField>(sender, row, val, CMPrecision.TRANCURY);
		}

		public static decimal RoundCury<InfoKeyField>(PXCache sender, object row, decimal? val)
			where InfoKeyField : IBqlField
		{
			return RoundCury<InfoKeyField>(sender, row, (decimal)val);
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			object NewValue = sender.GetValue(e.Row, _FieldOrdinal);
			_helper.CalcBaseValues(sender, new PXFieldVerifyingEventArgs(e.Row, NewValue, e.ExternalCall));
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			_helper.CalcBaseValues(sender, e);
		}

		public static void CalcBaseValues<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			PXCurrencyHelper.CalcBaseValues<Field>(cache, data);
		}

		public static void CalcBaseValues(PXCache cache, object data, string name)
		{
			PXCurrencyHelper.CalcBaseValues(cache, data, name);
		}

		public static void SetBaseCalc<Field>(PXCache cache, object data, bool isBaseCalc)
			where Field : IBqlField
		{
			PXCurrencyHelper.SetBaseCalc<Field>(cache, data, isBaseCalc);
		}

		public static void SetBaseCalc(PXCache cache, object data, string name, bool isBaseCalc)
		{
			PXCurrencyHelper.SetBaseCalc(cache, data, name, isBaseCalc);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			_helper.FieldSelecting(sender, e, FixedPrec);
		}

		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (sender.Graph.Accessinfo.CuryViewState
				&& e.Row != null && e.NewValue != null && object.ReferenceEquals(sender.GetValuePending(e.Row, _FieldName), e.NewValue))
			{
				e.NewValue = sender.GetValue(e.Row, _FieldOrdinal);
			}
			else
			{
				if (FixedPrec)
					base.FieldUpdating(sender, e);
				else
					_helper.FieldUpdating(sender, e);
			}
		}

		public virtual void ResultFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			_helper.ResultFieldSelecting(sender, e, FixedPrec);
		}

		public virtual void ResultFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (FixedPrec)
				base.FieldUpdating(sender, e);
			else
				_helper.ResultFieldUpdating(sender, e);
		}
		#endregion

		#region PXCurrencyHelper
		public class PXCurrencyHelper
		{
			private const int UseNoPrecision = -1;
			private const int UseCuryPrecision = -2;

			#region State
			protected Type _KeyField;
			protected Type _ResultField;
			protected int _ResultOrdinal;
			protected Type _ClassType;
			protected bool _BaseCalc = true;

			string _FieldName;
			int _FieldOrdinal;
			int? _Precision;
			PXAttributeLevel _AttributeLevel;
			#endregion

			public Type ResultField => _ResultField;
			internal int Precision => _Precision ?? 2;

			public bool BaseCalc
			{
				get
				{
					return this._BaseCalc;
				}
				set
				{
					this._BaseCalc = value;
				}
			}

			public int FieldOrdinal
			{
				get
				{
					return this._FieldOrdinal;
				}
				set
				{
					this._FieldOrdinal = value;
				}
			}

			public string FieldName
			{
				get
				{
					return this._FieldName;
				}
				set
				{
					this._FieldName = value;
				}
			}

			public PXCurrencyHelper Clone(PXAttributeLevel attributeLevel)
			{
				if (attributeLevel == PXAttributeLevel.Item)
				{
					return this;
				}

				PXCurrencyHelper helper = (PXCurrencyHelper)MemberwiseClone();
				helper._AttributeLevel = attributeLevel;
				return helper;
			}

			public PXCurrencyHelper(Type keyField, Type resultField, string FieldName, int FieldOrdinal, int? Precision, PXAttributeLevel AttributeLevel)
			{
				if (keyField != null && !typeof(IBqlField).IsAssignableFrom(keyField))
					throw new PXArgumentException("keyField", Messages.InvalidField, keyField);
				if (resultField != null && !typeof(IBqlField).IsAssignableFrom(resultField))
					throw new PXArgumentException("resultField", Messages.InvalidField, resultField);

				_KeyField = keyField;
				_ResultField = resultField;
				_FieldName = FieldName;
				_FieldOrdinal = FieldOrdinal;
				_Precision = Precision;
				_AttributeLevel = AttributeLevel;
			}

			public virtual void CacheAttached(PXCache sender)
			{
				_ClassType = sender.GetItemType();
				if (_KeyField == null)
					_KeyField = PXCurrencyHelper.SearchKeyField(sender);
				if (_ResultField == null)
					_ResultField = SearchResultField(sender);

				if (sender.HasAttribute<CurrencyInfoAttribute>())
				{
					sender.Graph.RowUpdated.AddHandler<CurrencyInfo>(currencyInfoRowUpdated);
				}

				_ResultOrdinal = sender.GetFieldOrdinal(_ResultField.Name);
			}

			private static Type SearchKeyField(PXCache sender)
			{
				for (int i = 0; i < sender.BqlFields.Count; i++)
					if (String.Compare(sender.BqlFields[i].Name, "CuryInfoID", true) == 0)
						return sender.BqlFields[i];
				throw new PXArgumentException("_KeyField", Messages.InvalidField, "CuryInfoID");
			}

			private Type SearchResultField(PXCache sender)
			{
				string fieldtosearch = "base" + _FieldName;
				for (int i = 0; i < sender.BqlFields.Count; i++)
					if (String.Compare(sender.BqlFields[i].Name, fieldtosearch, true) == 0)
						return sender.BqlFields[i];
				throw new PXArgumentException("_ResultField", Messages.InvalidField, _ResultField);
			}

			private static CurrencyInfo GetCurrencyInfo(PXCache sender, object row, Type _KeyField)
			{
				PXView curyinfoview;
				Type _ClassType = sender.BqlTable;

				if (GetView(sender.Graph, _ClassType, _KeyField, out curyinfoview))
				{
					CurrencyInfo info = null;
					if ((info = curyinfoview.SelectSingleBound(new object[] { row }) as CurrencyInfo) != null)
					{
						PopulatePrecision(curyinfoview.Cache, info);
					}
					return info;
				}

				throw new PXArgumentException("sender", Messages.InvalidCache, sender.GetItemType().Name);
			}

			private static CurrencyInfo GetCurrencyInfo(PXCache sender, object row)
			{
				Type _KeyField = PXCurrencyHelper.SearchKeyField(sender);

				return GetCurrencyInfo(sender, row, _KeyField);
			}

			private static CurrencyInfo GetCurrencyInfo<InfoKeyField>(PXCache sender, object row)
				where InfoKeyField : IBqlField
			{
				return GetCurrencyInfo(sender, row, typeof(InfoKeyField));
			}

			public static void CuryConvCury(PXCache sender, object row, decimal baseval, out decimal curyval)
			{
				CurrencyInfo info = GetCurrencyInfo(sender, row);
				CuryConvCury(info, baseval, out curyval);
			}

			public static void CuryConvCury(PXCache sender, object row, decimal baseval, out decimal curyval, int precision)
			{
				CurrencyInfo info = GetCurrencyInfo(sender, row);
				CuryConvCury(info, baseval, out curyval, precision);
			}

			public static void CuryConvCury(PXCache sender, object row, decimal baseval, out decimal curyval, bool SkipRounding)
			{
				CurrencyInfo info = GetCurrencyInfo(sender, row);
				CuryConvCury(info, baseval, out curyval, SkipRounding ? UseNoPrecision : UseCuryPrecision);
			}

			public static void CuryConvCury<CuryField>(PXCache sender, object row)
				where CuryField : IBqlField
			{
				foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly<CuryField>())
				{
					if (attr.AttributeLevel == PXAttributeLevel.Cache && attr is PXDBCurrencyAttribute)
					{
						Type keyfield = ((PXDBCurrencyAttribute)attr)._helper._KeyField;
						int resultordinal = ((PXDBCurrencyAttribute)attr)._helper._ResultOrdinal;
						string fieldname = ((PXDBCurrencyAttribute)attr)._helper.FieldName;

						CurrencyInfo info = GetCurrencyInfo(sender, row, keyfield);
						decimal? baseval = (decimal?)sender.GetValue(row, resultordinal);
						decimal curyval;
						CuryConvCury(info, baseval ?? 0m, out curyval);
						sender.SetValueExt(row, fieldname, curyval);

						return;
					}
				}
			}

			public static void CuryConvCury<InfoKeyField>(PXCache sender, object row, decimal baseval, out decimal curyval)
				where InfoKeyField : IBqlField
			{
				CurrencyInfo info = GetCurrencyInfo<InfoKeyField>(sender, row);
				CuryConvCury(info, baseval, out curyval, false);
			}

			public static void CuryConvCury<InfoKeyField>(PXCache sender, object row, decimal baseval, out decimal curyval, bool skipRounding)
				where InfoKeyField : IBqlField
			{
				CurrencyInfo info = GetCurrencyInfo<InfoKeyField>(sender, row);
				CuryConvCury(info, baseval, out curyval, skipRounding);
			}

			public static void CuryConvCury<InfoKeyField>(PXCache sender, object row, decimal curyval, out decimal baseval, int precision)
				where InfoKeyField : IBqlField
			{
				CurrencyInfo info = GetCurrencyInfo<InfoKeyField>(sender, row);
				CuryConvCury(info, curyval, out baseval, precision);
			}

			protected static void CuryConvCury(CurrencyInfo info, decimal baseval, out decimal curyval)
			{
				CuryConvCury(info, baseval, out curyval, false);
			}

			protected static void CuryConvCury(CurrencyInfo info, decimal baseval, out decimal curyval, bool skipRounding)
			{
				int precision = CurrencyInfo.curyPrecision.Default;
				if (info != null)
				{
					precision = (int)info.CuryPrecision;
				}

				CuryConvCury(info, baseval, out curyval, skipRounding ? UseNoPrecision : UseCuryPrecision);
			}

			protected static void CuryConvCury(CurrencyInfo info, decimal baseval, out decimal curyval, int precision)
			{
				if (info != null)
				{
					decimal rate;
					try
					{
						rate = (decimal)info.CuryRate;
					}
					catch (InvalidOperationException)
					{
						throw new PXRateNotFoundException();
					}
					if (rate == 0.0m)
					{
						rate = 1.0m;
					}
					bool mult = info.CuryMultDiv == CuryMultDivType.Div;
					curyval = mult ? baseval * rate : baseval / rate;

					if (precision == UseCuryPrecision && info.CuryPrecision != null)
						curyval = Math.Round(curyval, (int)info.CuryPrecision, MidpointRounding.AwayFromZero);
					else if (precision.IsNotIn(UseNoPrecision, UseCuryPrecision))
						curyval = Math.Round(curyval, precision, MidpointRounding.AwayFromZero);
				}
				else
				{
					curyval = baseval;
				}
			}

			public static void CuryConvCury(PXCache sender, CurrencyInfo info, decimal baseval, out decimal curyval)
			{
				PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];
				PopulatePrecision(cache, info);
				CuryConvCury(info, baseval, out curyval, false);
			}

			public static void CuryConvCury(PXCache sender, CurrencyInfo info, decimal baseval, out decimal curyval, bool skipRounding)
			{
				PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];
				PopulatePrecision(cache, info);
				CuryConvCury(info, baseval, out curyval, skipRounding);
			}

			public static void CuryConvBase(PXCache sender, object row, decimal curyval, out decimal baseval)
			{
				CurrencyInfo info = GetCurrencyInfo(sender, row);
				CuryConvBase(info, curyval, out baseval);
			}

			public static void CuryConvBase(PXCache sender, object row, decimal curyval, out decimal baseval, bool skipRounding)
			{
				CurrencyInfo info = GetCurrencyInfo(sender, row);
				CuryConvBase(info, curyval, out baseval, skipRounding);
			}

			public static void CuryConvBase<InfoKeyField>(PXCache sender, object row, decimal curyval, out decimal baseval)
				where InfoKeyField : IBqlField
			{
				CurrencyInfo info = GetCurrencyInfo<InfoKeyField>(sender, row);
				CuryConvBase(info, curyval, out baseval);
			}

			public static void CuryConvBase<InfoKeyField>(PXCache sender, object row, decimal curyval, out decimal baseval, int precision)
				where InfoKeyField : IBqlField
			{
				CurrencyInfo info = GetCurrencyInfo<InfoKeyField>(sender, row);
				CuryConvBase(info, curyval, out baseval, precision);
			}

			protected static void CuryConvBase(CurrencyInfo info, decimal curyval, out decimal baseval)
			{
				CuryConvBase(info, curyval, out baseval, false);
			}

			protected static void CuryConvBase(CurrencyInfo info, decimal curyval, out decimal baseval, bool skipRounding)
			{
				CuryConvBase(info, curyval, out baseval, skipRounding ? UseNoPrecision : UseCuryPrecision);
			}
			protected static void CuryConvBase(CurrencyInfo info, decimal curyval, out decimal baseval, int precision)
			{
				if (info != null)
				{
					decimal rate;
					try
					{
						rate = (decimal)info.CuryRate;
					}
					catch (InvalidOperationException)
					{
						throw new PXRateNotFoundException();
					}
					if (rate == 0.0m)
					{
						rate = 1.0m;
					}
					bool mult = info.CuryMultDiv != CuryMultDivType.Div;
					baseval = mult ? curyval * rate : curyval / rate;

					if (precision == UseCuryPrecision && info.BasePrecision != null)
						baseval = Math.Round(baseval, (int)info.BasePrecision, MidpointRounding.AwayFromZero);
					else if (precision.IsNotIn(UseNoPrecision, UseCuryPrecision))
						baseval = Math.Round(baseval, precision, MidpointRounding.AwayFromZero);
				}
				else
				{
					baseval = curyval;
				}
			}

			public static void CuryConvBase(PXCache sender, CurrencyInfo info, decimal curyval, out decimal baseval)
			{
				PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];
				PopulatePrecision(cache, info);
				CuryConvBase(info, curyval, out baseval, false);
			}

			public static void CuryConvBase(PXCache sender, CurrencyInfo info, decimal curyval, out decimal baseval, bool skipRounding)
			{
				PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];
				PopulatePrecision(cache, info);
				CuryConvBase(info, curyval, out baseval, skipRounding);
			}

			public static void CuryConvBase(PXCache sender, object row, decimal baseval, out decimal curyval, int precision)
			{
				CurrencyInfo info = GetCurrencyInfo(sender, row);
				CuryConvBase(info, baseval, out curyval, precision);
			}

			public static decimal Round(PXCache sender, object row, decimal val, CMPrecision prec)
			{
				CurrencyInfo info = GetCurrencyInfo(sender, row);
				if (info != null)
				{
					switch (prec)
					{
						case CMPrecision.TRANCURY:
							return Math.Round(val, (int)info.CuryPrecision, MidpointRounding.AwayFromZero);
						case CMPrecision.BASECURY:
							return Math.Round(val, (int)info.BasePrecision, MidpointRounding.AwayFromZero);
					}
				}
				return val;
			}

			public static decimal Round(decimal val, int customPrecision)
			{
				return Math.Round(val, customPrecision, MidpointRounding.AwayFromZero);
			}

			public static decimal Round<InfoKeyField>(PXCache sender, object row, decimal val, CMPrecision prec)
				where InfoKeyField : IBqlField
			{
				CurrencyInfo info = GetCurrencyInfo<InfoKeyField>(sender, row);
				if (info != null)
				{
					switch (prec)
					{
						case CMPrecision.TRANCURY:
							return Math.Round(val, (int)info.CuryPrecision, MidpointRounding.AwayFromZero);
						case CMPrecision.BASECURY:
							return Math.Round(val, (int)info.BasePrecision, MidpointRounding.AwayFromZero);
					}
				}
				return val;
			}

			public static void SetBaseCalc<Field>(PXCache cache, object data, bool isBaseCalc)
				where Field : IBqlField
			{
				if (data == null)
				{
					cache.SetAltered<Field>(true);
				}
				foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly<Field>())
				{
					if (attr is PXCurrencyAttribute)
					{
						((PXCurrencyAttribute)attr).BaseCalc = isBaseCalc;
					}
					if (attr is PXDBCurrencyAttribute)
					{
						((PXDBCurrencyAttribute)attr).BaseCalc = isBaseCalc;
					}
				}
			}

			public static void SetBaseCalc(PXCache cache, object data, string name, bool isBaseCalc)
			{
				if (data == null)
				{
					cache.SetAltered(name, true);
				}
				foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(name))
				{
					if (attr is PXCurrencyAttribute)
					{
						((PXCurrencyAttribute)attr).BaseCalc = isBaseCalc;
					}
					if (attr is PXDBCurrencyAttribute)
					{
						((PXDBCurrencyAttribute)attr).BaseCalc = isBaseCalc;
					}
				}
			}

			public static void CalcBaseValues<Field>(PXCache cache, object data)
				where Field : IBqlField
			{
				if (data == null)
				{
					cache.SetAltered<Field>(true);
				}
				foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly<Field>())
				{
					if (attr is PXCurrencyAttribute)
					{
						object NewValue = cache.GetValue(data, attr.FieldOrdinal);
						((PXCurrencyAttribute)attr)._helper.CalcBaseValues(cache, new PXFieldVerifyingEventArgs(data, NewValue, false));
					}
					if (attr is PXDBCurrencyAttribute)
					{
						object NewValue = cache.GetValue(data, attr.FieldOrdinal);
						((PXDBCurrencyAttribute)attr)._helper.CalcBaseValues(cache, new PXFieldVerifyingEventArgs(data, NewValue, false));
					}
				}
			}

			public static void CalcBaseValues(PXCache cache, object data, string name)
			{
				if (data == null)
				{
					cache.SetAltered(name, true);
				}
				foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(name))
				{
					if (attr is PXCurrencyAttribute)
					{
						object NewValue = cache.GetValue(data, attr.FieldOrdinal);
						((PXCurrencyAttribute)attr)._helper.CalcBaseValues(cache, new PXFieldVerifyingEventArgs(data, NewValue, false));
					}
					if (attr is PXDBCurrencyAttribute)
					{
						object NewValue = cache.GetValue(data, attr.FieldOrdinal);
						((PXDBCurrencyAttribute)attr)._helper.CalcBaseValues(cache, new PXFieldVerifyingEventArgs(data, NewValue, false));
					}
				}
			}

			internal void CalcBaseValues(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				if (!_BaseCalc)
				{
					return;
				}
				CurrencyInfo info = null;
				if (e.NewValue != null && (info = getInfoInt(sender, e.Row)) != null && info.CuryRate != null && info.BaseCalc == true)
				{
					decimal rate = (decimal)info.CuryRate;
					if (rate == 0.0m)
					{
						rate = 1.0m;
					}
					bool mult = info.CuryMultDiv != "D";
					decimal cval = (decimal)e.NewValue;
					object value = mult ? cval * rate : cval / rate;
					sender.RaiseFieldUpdating(_ResultField.Name, e.Row, ref value);
					sender.SetValue(e.Row, _ResultOrdinal, value);
				}
				else if (info == null || info.BaseCalc == true)
				{
					object value = e.NewValue;
					sender.RaiseFieldUpdating(_ResultField.Name, e.Row, ref value);
					sender.SetValue(e.Row, _ResultOrdinal, value);
				}
			}

			public virtual void currencyInfoRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				if (!_BaseCalc)
				{
					return;
				}

				CurrencyInfo newRow = e.Row as CurrencyInfo;
				CurrencyInfo oldRow = e.OldRow as CurrencyInfo;
				if (newRow != null && (oldRow == null || newRow.CuryRate != oldRow.CuryRate || newRow.CuryMultDiv != oldRow.CuryMultDiv))
				{
					PXView siblings = CurrencyInfoAttribute.GetView(sender.Graph, _ClassType, _KeyField);
					if (siblings != null && newRow.CuryRate != null)
					{
						decimal rate = (decimal)newRow.CuryRate;
						if (rate == 0.0m)
						{
							rate = 1.0m;
						}
						bool mult = newRow.CuryMultDiv != "D";
						PXCache cache = siblings.Cache;
						foreach (object data in siblings.SelectMultiBound(new object[] { e.Row }))
						{
							object item = data is PXResult ? ((PXResult)data)[0] : data;
							if (cache.GetValue(item, _FieldOrdinal) != null)
							{
								decimal cval = (decimal)cache.GetValue(item, _FieldOrdinal);
								object value = mult ? cval * rate : cval / rate;
								cache.RaiseFieldUpdating(_ResultField.Name, item, ref value);
								cache.SetValue(item, _ResultOrdinal, value);
								cache.MarkUpdated(item);
							}
						}
					}
				}
			}


			private static short GetBasePrecision(PXGraph graph)
			{
				CurrencyInfo info = new CurrencyInfo();
				object BaseCuryID;
				PXCache cache = graph.Caches[typeof(CurrencyInfo)];
				cache.RaiseFieldDefaulting<CurrencyInfo.baseCuryID>(info, out BaseCuryID);
				info.BaseCuryID = (string)BaseCuryID;
				info.CuryPrecision = 4;
				object prec;
				cache.RaiseFieldDefaulting<CurrencyInfo.basePrecision>(info, out prec);
				return (short)prec;
			}

			public static decimal BaseRound(PXGraph graph, decimal value)
			{
				short prec = GetBasePrecision(graph);
				return Math.Round(value, prec, MidpointRounding.AwayFromZero);
			}
			
			public static void PopulatePrecision(PXCache cache, CurrencyInfo info)
			{
				if (info != null)
				{
					if (info.CuryPrecision == null)
					{
						object prec;
						cache.RaiseFieldDefaulting<CurrencyInfo.curyPrecision>(info, out prec);
						info.CuryPrecision = (short?)prec;
						cache.Hold(info);
					}

					if (info.BasePrecision == null)
					{
						object prec;
						cache.RaiseFieldDefaulting<CurrencyInfo.basePrecision>(info, out prec);
						info.BasePrecision = (short?)prec;
						cache.Hold(info);
					}
				}
			}

			public int GetCurrencyInfoPrecision(PXCache sender, object row)
			{
				CurrencyInfo info = getInfo(sender, row);
				return info?.CuryPrecision ?? Precision;
			}

			public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, bool fixedPrecision)
			{
				bool curyviewstate = sender.Graph.Accessinfo.CuryViewState;
				int? actualPrecision = null;

				if (!fixedPrecision)
				{
					CurrencyInfo info = getInfo(sender, e.Row);

					if (info != null)
					{
						PXCurrencyHelper.PopulatePrecision(sender.Graph.Caches<CurrencyInfo>(), info);
						_Precision = !curyviewstate
							? (info.CuryPrecision ?? 2)
							: (info.BasePrecision ?? 2);
						actualPrecision = _Precision;
					}
				}

				if (curyviewstate)
				{
					object NewValue = sender.GetValue(e.Row, _FieldOrdinal);
					CalcBaseValues(sender, new PXFieldVerifyingEventArgs(e.Row, NewValue, false));

					e.ReturnValue = sender.GetValue(e.Row, _ResultOrdinal);
				}
				if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
				{
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null, null, actualPrecision, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, curyviewstate ? (bool?)false : null, null, null, PXUIVisibility.Undefined, null, null, null);
				}
			}

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

			private CurrencyInfo getInfo(PXCache sender, object row)
			{
				if (sender.Graph.GetType() == typeof(PXGenericInqGrph) ||
					sender.Graph.GetType() == typeof(LayoutMaint) ||
					sender.Graph.GetType() == typeof(PXGraph))
				{
					CurrencyInfo info = new CurrencyInfo();
					info.CuryInfoID = (long?)sender.GetValue(row, _KeyField.Name);

					PXCache cache = sender.Graph.Caches<CurrencyInfo>();
					object cached;
					if ((cached = cache.Locate(info)) == null)
					{
						info.CuryID = CurrencyInfoAttribute.GetCuryID(sender, row, _KeyField.Name);
						object newValue;
						if (cache.RaiseFieldDefaulting<CurrencyInfo.baseCuryID>(info, out newValue))
						{
							cache.RaiseFieldUpdating<CurrencyInfo.baseCuryID>(info, ref newValue);
						}
						cache.SetValue<CurrencyInfo.baseCuryID>(info, newValue);
					}
					else
					{
						info = cached as CurrencyInfo;
					}

					return info;
				}

				return getInfoInt(sender, row);
			}

			private CurrencyInfo getInfoInt(PXCache sender, object row)
			{
				PXView curyinfoview;
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
								if (key == null && info == null)
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
									using (new CurrencyInfoAttribute.SkipReadOnly(curyinfoview))
									{
									info = curyinfoview.SelectSingleBound(new object[] { row }) as CurrencyInfo;
								}
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
							using (new CurrencyInfoAttribute.SkipReadOnly(curyinfoview))
							{
							info = curyinfoview.SelectSingleBound(new object[] { row }) as CurrencyInfo;
								PopulatePrecision(curyinfoview.Cache, info);
						}
					}
					}
					return info;
				}
				return null;
			}

			public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				if (!FormatValue(e, sender.Graph.Culture)) return;

				CurrencyInfo info = getInfo(sender, e.Row);

				if (info != null)
				{
					PopulatePrecision(sender.Graph.Caches<CurrencyInfo>(), info);
				    var value = Convert.ToDecimal(e.NewValue);
				    e.NewValue = Math.Round(value, info.CuryPrecision ?? 2, MidpointRounding.AwayFromZero);
				}
			}

			public virtual void ResultFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, bool fixedPrecision)
			{
				bool curyviewstate = sender.Graph.Accessinfo.CuryViewState;
				int? actualPrecision = null;

				if (!fixedPrecision)
				{
					CurrencyInfo info = getInfo(sender, e.Row);

					if (info != null)
					{
						PXCurrencyHelper.PopulatePrecision(sender.Graph.Caches<CurrencyInfo>(), info);
						actualPrecision = (info.BasePrecision ?? 2);
					}
				}

				if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
				{
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null, null, actualPrecision, null, null, null, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
				}
			}

			public virtual void ResultFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				if (!FormatValue(e, sender.Graph.Culture)) return;

				CurrencyInfo info = getInfo(sender, e.Row);

				if (info != null)
				{
					PXCurrencyHelper.PopulatePrecision(sender.Graph.Caches<CurrencyInfo>(), info);
					e.NewValue = Math.Round((decimal)e.NewValue, (int)(info.BasePrecision ?? (short)2), MidpointRounding.AwayFromZero);
				}
			}

			private static bool FormatValue(PXFieldUpdatingEventArgs e, System.Globalization.CultureInfo culture)
			{
				if (e.NewValue is string)
				{
					decimal val;
					if (decimal.TryParse((string)e.NewValue, System.Globalization.NumberStyles.Any, culture, out val))
					{
						e.NewValue = val;
					}
					else
					{
						e.NewValue = null;
					}
				}
				return e.NewValue != null;
			}
		}
		#endregion
	}
	#endregion

	#region PXDBCurrencyAttribute
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
	public class PXDBCurrencyAttribute : PXDBDecimalAttribute, IPXFieldVerifyingSubscriber, IPXRowInsertingSubscriber
	{
		#region State
		internal PXCurrencyAttribute.PXCurrencyHelper _helper;
		protected override Int32? Precision => _helper.Precision;
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
			_helper = new PXCurrencyAttribute.PXCurrencyHelper(keyField, resultField, _FieldName, _FieldOrdinal, _Precision, _AttributeLevel);
		}

		public PXDBCurrencyAttribute(Type precision, Type keyField, Type resultField)
			: base(precision)
		{
			_helper = new PXCurrencyAttribute.PXCurrencyHelper(keyField, resultField, _FieldName, _FieldOrdinal, _Precision, _AttributeLevel);
			this.FixedPrec = true;
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
		public PXDBCurrencyAttribute(int precision, Type keyField, Type resultField)
			: base(precision)
		{
			_helper = new PXCurrencyAttribute.PXCurrencyHelper(keyField, resultField, _FieldName, _FieldOrdinal, _Precision, _AttributeLevel);
			this.FixedPrec = true;
		}

		#endregion

		#region Runtime
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.SetAltered(_FieldName, true);
			_helper.CacheAttached(sender);
			sender.Graph.FieldUpdating.AddHandler(BqlCommand.GetItemType(_helper.ResultField),
				_helper.ResultField.Name,
			  ResultFieldUpdating);

			PXDBDecimalAttribute.SetPrecision(sender, _helper.ResultField.Name, FixedPrec ? _Precision : null);
			if (!FixedPrec)
			{
				sender.SetAltered(_helper.ResultField.Name, true);
				sender.Graph.FieldSelecting.AddHandler(BqlCommand.GetItemType(_helper.ResultField),
					_helper.ResultField.Name,
					ResultFieldSelecting);
			}
		}
		#endregion

		#region Implementation

		bool FixedPrec = false;

		public virtual bool BaseCalc
		{
			get
			{
				return _helper.BaseCalc;
			}
			set
			{
				_helper.BaseCalc = value;
			}
		}

		public override int FieldOrdinal
		{
			get
			{
				return base.FieldOrdinal;
			}
			set
			{
				base.FieldOrdinal = value;
				_helper.FieldOrdinal = value;
			}
		}

		public override string FieldName
		{
			get
			{
				return base.FieldName;
			}
			set
			{
				base.FieldName = value;
				_helper.FieldName = value;
			}
		}

		public override PXEventSubscriberAttribute Clone(PXAttributeLevel attributeLevel)
		{
			PXDBCurrencyAttribute attr = (PXDBCurrencyAttribute)base.Clone(attributeLevel);
			attr._helper = this._helper.Clone(attributeLevel);
			return attr;
		}


		/// <summary>
		/// Converts from amount from Base Currency to another
		/// </summary>
		public static void CuryConvCury(PXCache sender, object row, decimal baseval, out decimal curyval)
		{
			PXCurrencyAttribute.CuryConvCury(sender, row, baseval, out curyval);
		}

		public static void CuryConvCury(PXCache sender, object row, decimal baseval, out decimal curyval, bool skipRounding)
		{
			PXCurrencyAttribute.CuryConvCury(sender, row, baseval, out curyval, skipRounding);
		}

		public static void CuryConvCury(PXCache sender, object row, decimal baseval, out decimal curyval, int precision)
		{
			PXCurrencyAttribute.CuryConvCury(sender, row, baseval, out curyval, precision);
		}

		/// <summary>
		/// Converts from amount from Base Currency to another
		/// </summary>
		public static void CuryConvCury<InfoKeyField>(PXCache sender, object row, decimal baseval, out decimal curyval)
			where InfoKeyField : IBqlField
		{
			PXCurrencyAttribute.CuryConvCury<InfoKeyField>(sender, row, baseval, out curyval);
		}

		public static void CuryConvCury<InfoKeyField>(PXCache sender, object row, decimal baseval, out decimal curyval, bool skipRounding)
			where InfoKeyField : IBqlField
		{
			PXCurrencyAttribute.CuryConvCury<InfoKeyField>(sender, row, baseval, out curyval, skipRounding);
		}

		/// <summary>
		/// Converts amount to Base Currency
		/// </summary>
		public static void CuryConvCury<InfoKeyField>(PXCache sender, object row, decimal curyval, out decimal baseval, int precision)
			where InfoKeyField : IBqlField
		{
			PXCurrencyAttribute.CuryConvCury<InfoKeyField>(sender, row, curyval, out baseval, precision);
		}

		/// <summary>
		/// Converts from amount from Base Currency to another
		/// </summary>
		public static void CuryConvCury(PXCache cache, CurrencyInfo info, decimal baseval, out decimal curyval)
		{
			PXCurrencyAttribute.CuryConvCury(cache, info, baseval, out curyval);
		}

		public static void CuryConvCury(PXCache cache, CurrencyInfo info, decimal baseval, out decimal curyval, bool skipRounding)
		{
			PXCurrencyAttribute.CuryConvCury(cache, info, baseval, out curyval, skipRounding);
		}

		/// <summary>
		/// Converts amount to Base Currency
		/// </summary>
		public static void CuryConvBase(PXCache sender, object row, decimal curyval, out decimal baseval)
		{
			PXCurrencyAttribute.CuryConvBase(sender, row, curyval, out baseval);
		}

		public static void CuryConvBase(PXCache sender, object row, decimal curyval, out decimal baseval, bool skipRounding)
		{
			PXCurrencyAttribute.CuryConvBase(sender, row, curyval, out baseval, skipRounding);
		}

		public static void CuryConvBase(PXCache sender, object row, decimal baseval, out decimal curyval, int precision)
		{
			PXCurrencyAttribute.CuryConvBase(sender, row, baseval, out curyval, precision);
		}

		/// <summary>
		/// Converts amount to Base Currency
		/// </summary>
		public static void CuryConvBase<InfoKeyField>(PXCache sender, object row, decimal curyval, out decimal baseval)
			where InfoKeyField : IBqlField
		{
			PXCurrencyAttribute.CuryConvBase<InfoKeyField>(sender, row, curyval, out baseval);
		}

		/// <summary>
		/// Converts amount to Base Currency
		/// </summary>
		public static void CuryConvBase(PXCache cache, CurrencyInfo info, decimal curyval, out decimal baseval)
		{
			PXCurrencyAttribute.CuryConvBase(cache, info, curyval, out baseval);
		}

		public static void CuryConvBase(PXCache cache, CurrencyInfo info, decimal curyval, out decimal baseval, bool skipRounding)
		{
			PXCurrencyAttribute.CuryConvBase(cache, info, curyval, out baseval, skipRounding);
		}

		/// <summary>
		/// Converts amount to Base Currency
		/// </summary>
		public static void CuryConvBase<InfoKeyField>(PXCache sender, object row, decimal curyval, out decimal baseval, int precision)
			where InfoKeyField : IBqlField
		{
			PXCurrencyAttribute.CuryConvBase<InfoKeyField>(sender, row, curyval, out baseval, precision);
		}

		/// <summary>
		/// Rounds amount according to Base Currency rules.
		/// </summary>
		public static decimal BaseRound(PXGraph graph, decimal value)
		{
			return PXCurrencyAttribute.BaseRound(graph, value);
		}

		/// <summary>
		/// Rounds given amount either according to Base Currency or current Currency rules.
		/// </summary>
		public static decimal Round(PXCache sender, object row, decimal val, CMPrecision prec)
		{
			return PXCurrencyAttribute.Round(sender, row, val, prec);
		}

		/// <summary>
		/// Rounds given amount either according to Base Currency or current Currency rules.
		/// </summary>
		public static decimal Round<InfoKeyField>(PXCache sender, object row, decimal val, CMPrecision prec)
			where InfoKeyField : IBqlField
		{
			return PXCurrencyAttribute.Round<InfoKeyField>(sender, row, val, prec);
		}

		/// <summary>
		/// Rounds given amount according to current Currency rules.
		/// </summary>		
		public static decimal RoundCury(PXCache sender, object row, decimal val)
		{
			return PXCurrencyAttribute.Round(sender, row, val, CMPrecision.TRANCURY);
		}

		/// <summary>
		/// Rounds given amount according to current Currency rules or using the custom precision.
		/// </summary>		
		public static decimal RoundCury(PXCache sender, object row, decimal val, int? customPrecision)
		{
			if(customPrecision == null)
			{
				return RoundCury(sender, row, val);
			}

			return PXCurrencyAttribute.Round(sender, row, val, CMPrecision.CUSTOM, customPrecision.Value);
		}

		/// <summary>
		/// Rounds given amount according to current Currency rules.
		/// </summary>
		public static decimal RoundCury<InfoKeyField>(PXCache sender, object row, decimal val)
			where InfoKeyField : IBqlField
		{
			return PXCurrencyAttribute.Round<InfoKeyField>(sender, row, val, CMPrecision.TRANCURY);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object NewValue = sender.GetValue(e.Row, _FieldOrdinal);
			_helper.CalcBaseValues(sender, new PXFieldVerifyingEventArgs(e.Row, NewValue, false));
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			object NewValue = sender.GetValue(e.Row, _FieldOrdinal);
			_helper.CalcBaseValues(sender, new PXFieldVerifyingEventArgs(e.Row, NewValue, e.ExternalCall));
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			_helper.CalcBaseValues(sender, e);

			if (e.NewValue != null)
			{
				int precision = FixedPrec ? (int)_Precision :
					_helper.GetCurrencyInfoPrecision(sender, e.Row);

				e.NewValue = Math.Round((decimal)e.NewValue, precision, MidpointRounding.AwayFromZero);
			}
		}

		/// <summary>
		/// Automaticaly Converts and updates Base Currency field with the current value.
		/// BaseCurrency field is supplied through Field 
		/// </summary>
		public static void CalcBaseValues<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			PXCurrencyAttribute.CalcBaseValues<Field>(cache, data);
		}

		/// <summary>
		/// Automaticaly Converts and updates Base Currency field with the current value.
		/// </summary>
		public static void CalcBaseValues(PXCache cache, object data, string name)
		{
			PXCurrencyAttribute.CalcBaseValues(cache, data, name);
		}

		public static void SetBaseCalc<Field>(PXCache cache, object data, bool isBaseCalc)
			where Field : IBqlField
		{
			PXCurrencyAttribute.SetBaseCalc<Field>(cache, data, isBaseCalc);
		}

		public static void SetBaseCalc(PXCache cache, object data, string name, bool isBaseCalc)
		{
			PXCurrencyAttribute.SetBaseCalc(cache, data, name, isBaseCalc);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			_helper.FieldSelecting(sender, e, FixedPrec);
		}

		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (sender.Graph.Accessinfo.CuryViewState
				&& e.Row != null && e.NewValue != null && object.ReferenceEquals(sender.GetValuePending(e.Row, _FieldName), e.NewValue))
			{
				e.NewValue = sender.GetValue(e.Row, _FieldOrdinal);
			}
			else
			{
				if (FixedPrec)
					base.FieldUpdating(sender, e);
				else
				{
					_helper.FieldUpdating(sender, e);
					string error = Check(e.NewValue);
					if (error != null)
					{
						throw new PXSetPropertyException(error);
					}
				}
			}
		}

		public virtual void ResultFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			_helper.ResultFieldSelecting(sender, e, FixedPrec);
		}

		public virtual void ResultFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (FixedPrec)
				base.FieldUpdating(sender, e);
			else
				_helper.ResultFieldUpdating(sender, e);
		}

		#endregion
	}
	#endregion

	#region PXDBBaseCuryAttribute
	/// <summary>
	/// Extends <see cref="PXDBDecimalAttribute"/> by defaulting the precision property.
	/// If LedgerID is supplied than Precision is taken form the Ledger's base currency; otherwise Precision is taken from Base Currency that is configured on the Company level.
	/// </summary>
	public class PXDBBaseCuryAttribute : PXDBDecimalAttribute
	{
		public PXDBBaseCuryAttribute()
			: base(typeof(Search2<Currency.decimalPlaces, InnerJoin<GL.Company, On<GL.Company.baseCuryID, Equal<Currency.curyID>>>>))
		{
		}

		public PXDBBaseCuryAttribute(Type LedgerIDType)
			: base(BqlCommand.Compose(typeof(Search2<,,>), typeof(Currency.decimalPlaces), typeof(InnerJoin<GL.Ledger, On<GL.Ledger.baseCuryID, Equal<Currency.curyID>>>), typeof(Where<,>), typeof(GL.Ledger.ledgerID), typeof(Equal<>), typeof(Current<>), LedgerIDType))
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			sender.SetAltered(_FieldName, true);
			base.CacheAttached(sender);
		}

	}
	#endregion

	#region PXBaseCuryAttribute
	/// <summary>
	/// Extends <see cref="PXDecimalAttribute"/> by defaulting the precision property.
	/// If LedgerID is supplied than Precision is taken form the Ledger's base currency; otherwise Precision is taken from Base Currency that is configured on the Company level.
	/// </summary>
	/// <remarks>This is a NON-DB attribute. Use it for calculated fields that are not storred in database.</remarks>
	public class PXBaseCuryAttribute : PXDecimalAttribute
	{
		public PXBaseCuryAttribute()
			: base(typeof(Search2<Currency.decimalPlaces, InnerJoin<GL.Company, On<GL.Company.baseCuryID, Equal<Currency.curyID>>>>))
		{
		}

		public PXBaseCuryAttribute(Type LedgerIDType)
			: base(BqlCommand.Compose(typeof(Search2<,,>), typeof(Currency.decimalPlaces), typeof(InnerJoin<GL.Ledger, On<GL.Ledger.baseCuryID, Equal<Currency.curyID>>>), typeof(Where<,>), typeof(GL.Ledger.ledgerID), typeof(Equal<>), typeof(Current<>), LedgerIDType))
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			sender.SetAltered(_FieldName, true);
			base.CacheAttached(sender);
		}
	}
	#endregion

	#region PXDBCuryAttribute
	/// <summary>
	/// Extends <see cref="PXDBDecimalAttribute"/> by defaulting the precision property.
	/// Precision is taken from given Currency.
	/// </summary>
	public class PXDBCuryAttribute : PXDBDecimalAttribute
	{
		public PXDBCuryAttribute(Type CuryIDType)
			: base(BqlCommand.Compose(typeof(Search<,>), typeof(Currency.decimalPlaces), typeof(Where<,>), typeof(Currency.curyID), typeof(Equal<>), typeof(Current<>), CuryIDType))
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			sender.SetAltered(_FieldName, true);
			base.CacheAttached(sender);
		}
	}
	#endregion

	#region PXCuryAttribute
	/// <summary>
	/// Extends <see cref="PXDecimalAttribute"/> by defaulting the precision property.
	/// Precision is taken from given Currency.
	/// </summary>
	/// <remarks>This is a NON-DB attribute. Use it for calculated fields that are not storred in database.</remarks>
	public class PXCuryAttribute : PXDecimalAttribute
	{
		public PXCuryAttribute(Type CuryIDType)
			: base(BqlCommand.Compose(typeof(Search<,>), typeof(Currency.decimalPlaces), typeof(Where<,>), typeof(Currency.curyID), typeof(Equal<>), typeof(Current<>), CuryIDType))
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			sender.SetAltered(_FieldName, true);
			base.CacheAttached(sender);
		}
	}
	#endregion

	#region PXCalcCurrencyAttribute
	public class PXUnitPriceCuryConvAttribute : PXDecimalAttribute, IPXRowSelectedSubscriber
	{
		#region State
		protected int _SourceOrdinal;
		protected int _KeyOrdinal;
		protected Type _KeyField = null;
		protected Type _SourceField = null;
		#endregion

		#region Ctor
		public PXUnitPriceCuryConvAttribute()
		{
		}

		public PXUnitPriceCuryConvAttribute(Type keyField, Type sourceField)
		{
			_KeyField = keyField;
			_SourceField = sourceField;
		}
		#endregion

		#region Runtime
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Precision = IN.CommonSetupDecPl.PrcCst;

			if (_SourceField != null)
			{
				_SourceOrdinal = sender.GetFieldOrdinal(_SourceField.Name);
			}

			if (_KeyField != null)
			{
				_KeyOrdinal = sender.GetFieldOrdinal(_KeyField.Name);
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_KeyField), _KeyField.Name, KeyFieldUpdated);
			}
		}
		#endregion

		#region Implementation

		protected virtual void CalcTran(PXCache sender, object data)
		{
			if (_SourceField != null)
			{
				object NewValue = sender.GetValue(data, _SourceOrdinal);

				if (NewValue != null)
				{
					Decimal curyVale;
					PXCurrencyAttribute.CuryConvCury(sender, data, (Decimal)NewValue, out curyVale, (int)_Precision);
					sender.SetValue(data, _FieldOrdinal, curyVale);
				}
			}
		}

		public virtual void KeyFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcTran(sender, e.Row);
		}
		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (sender.GetValue(e.Row, _FieldOrdinal) == null)
				CalcTran(sender, e.Row);
		}
		#endregion
	}
	#endregion


	#region ToggleCurrency
	public class ToggleCurrency<TNode> : PXAction<TNode>
		where TNode : class, IBqlTable, new()
	{
		public ToggleCurrency(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		[PXUIField(DisplayName = "Toggle Currency", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Money, Tooltip = Messages.ToggleCurrencyViewTooltip)]
		protected override System.Collections.IEnumerable Handler(PXAdapter adapter)
		{
			_Graph.Accessinfo.CuryViewState = !_Graph.Accessinfo.CuryViewState;
			PXCache cache = adapter.View.Cache;
			bool anyDiff = !cache.IsDirty;
			foreach (object ret in adapter.Get())
			{
				if (!anyDiff)
				{
					TNode item;
					if (ret is PXResult)
					{
						item = (TNode)((PXResult)ret)[0];
					}
					else
					{
						item = (TNode)ret;
					}
					if (item == null)
					{
						anyDiff = true;
					}
					else
					{
						TNode oldItem = CurrencyInfoAttribute.GetOldRow(cache, item) as TNode;
						if (item == null || oldItem == null)
						{
							anyDiff = true;
						}
						else
						{
							foreach (string field in cache.Fields)
							{
								object oldV = cache.GetValue(oldItem, field);
								object newV = cache.GetValue(item, field);
								if ((oldV != null || newV != null) && !object.Equals(oldV, newV) && (!(oldV is DateTime && newV is DateTime) || ((DateTime)oldV).Date != ((DateTime)newV).Date))
								{
									anyDiff = true;
								}
							}
						}
					}
				}
				yield return ret;
			}
			if (!anyDiff)
			{
				cache.IsDirty = false;
			}
		}
	}
	#endregion


	public interface IRegister : IDocumentKey
	{
		DateTime? DocDate { get; set; }
		String CuryID { get; set; }
		String DocDesc { get; set; }
		string OrigModule { get; set; }
		Decimal? CuryOrigDocAmt { get; set; }
		Decimal? OrigDocAmt { get; set; }
		Int64? CuryInfoID { get; set; }
	}

	public interface IInvoice: IRegister
    {
		Decimal? CuryDocBal
		{
			get;
			set;
		}
		Decimal? DocBal
		{
			get;
			set;
		}
		Decimal? CuryDiscBal
		{
			get;
			set;
		}
		Decimal? DiscBal
		{
			get;
			set;
		}
		Decimal? CuryWhTaxBal
		{
			get;
			set;
		}
		Decimal? WhTaxBal
		{
			get;
			set;
		}
		DateTime? DiscDate
		{
			get;
			set;
		}
	}

	public interface IDocumentTran
	{
		string TranType { get; set; }
		string RefNbr { get; set; }
		int? LineNbr { get; set; }
		decimal? TranAmt { get; set; }
		decimal? CuryTranAmt { get; set; }
		string TranDesc { get; set; }
		DateTime? TranDate { get; set; }

		long? CuryInfoID { get; set; }

		decimal? CuryCashDiscBal { get; set; }
		decimal? CashDiscBal { get; set; }
		decimal? CuryTranBal { get; set; }
		decimal? TranBal { get; set; }
	}

	public interface ITranTax
	{
		string TranType { get; set; }
		string RefNbr { get; set; }
		int? LineNbr { get; set; }
		string TaxID { get; set; }
		decimal? CuryTaxableAmt { get; set; }
		decimal? TaxableAmt { get; set; }
		decimal? CuryTaxAmt { get; set; }
		decimal? TaxAmt { get; set; }
	}

	public class CalcBalancesAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		#region State
		protected string _CuryAmtField;
		protected string _BaseAmtField;

		protected string _PayCuryIDField = "CashCuryID";
		public string PayCuryIDField
		{
			get
			{
				return this._PayCuryIDField;
			}
			set
			{
				this._PayCuryIDField = value;
			}
		}
		protected string _DocCuryIDField = "DocCuryID";
		public string DocCuryIDField
		{
			get
			{
				return this._DocCuryIDField;
			}
			set
			{
				this._DocCuryIDField = value;
			}
		}
		protected string _BaseCuryIDField = "BaseCuryID";
		public string BaseCuryIDField
		{
			get
			{
				return this._BaseCuryIDField;
			}
			set
			{
				this._BaseCuryIDField = value;
			}
		}
		protected string _PayCuryRateField = "CashCuryRate";
		public string PayCuryRateField
		{
			get
			{
				return this._PayCuryRateField;
			}
			set
			{
				this._PayCuryRateField = value;
			}
		}
		protected string _PayCuryMultDivField = "CashCuryMultDiv";
		public string PayCuryMultDivField
		{
			get
			{
				return this._PayCuryMultDivField;
			}
			set
			{
				this._PayCuryMultDivField = value;
			}
		}
		protected string _DocCuryRateField = "DocCuryRate";
		public string DocCuryRateField
		{
			get
			{
				return this._DocCuryRateField;
			}
			set
			{
				this._DocCuryRateField = value;
			}
		}
		protected string _DocCuryMultDivField = "DocCuryMultDiv";
		public string DocCuryMultDivField
		{
			get
			{
				return this._DocCuryMultDivField;
			}
			set
			{
				this._DocCuryMultDivField = value;
			}
		}
		#endregion
		#region Ctor
		public CalcBalancesAttribute(Type CuryAmtField, Type BaseAmtField)
		{
			_CuryAmtField = CuryAmtField.Name;
			_BaseAmtField = BaseAmtField.Name;
		}
		#endregion
		#region Implementation
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			object CuryAmt = sender.GetValue(e.Row, _CuryAmtField);
			object BaseAmt = sender.GetValue(e.Row, _BaseAmtField);

			string PayCuryID = (string)sender.GetValue(e.Row, _PayCuryIDField);
			string DocCuryID = (string)sender.GetValue(e.Row, _DocCuryIDField);
			string BaseCuryID = (string)sender.GetValue(e.Row, _BaseCuryIDField);
			object PayCuryRate = sender.GetValue(e.Row, _PayCuryRateField);
			string PayCuryMultDiv = (string)sender.GetValue(e.Row, _PayCuryMultDivField);
			object DocCuryRate = sender.GetValue(e.Row, _DocCuryRateField);
			string DocCuryMultDiv = (string)sender.GetValue(e.Row, _DocCuryMultDivField);

			if (PayCuryRate == null)
			{
				PayCuryRate = 1m;
				PayCuryMultDiv = "M";
			}

			if (DocCuryRate == null)
			{
				DocCuryRate = 1m;
				DocCuryMultDiv = "M";
			}

			sender.RaiseFieldSelecting(_CuryAmtField, e.Row, ref CuryAmt, true);
			sender.RaiseFieldSelecting(_BaseAmtField, e.Row, ref BaseAmt, true);

			e.ReturnValue = PaymentEntry.CalcBalances((decimal?)((PXFieldState)CuryAmt).Value, (decimal?)((PXFieldState)BaseAmt).Value, PayCuryID, DocCuryID, BaseCuryID, (decimal)PayCuryRate, PayCuryMultDiv, (decimal)DocCuryRate, DocCuryMultDiv, ((PXFieldState)CuryAmt).Precision, ((PXFieldState)BaseAmt).Precision);
			e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, ((PXFieldState)CuryAmt).Precision, _FieldName, null, null, null, null);
		}
		#endregion
	}

	public static class CurrencyInfoCache
	{
		public static CurrencyInfo GetInfo(PXSelectBase<CurrencyInfo> select, long? curyInfoID)
		{
			if (curyInfoID != null)
			{
				CurrencyInfo info = (CurrencyInfo)select.Cache.Locate(
					new CurrencyInfo { CuryInfoID = curyInfoID }
				);
				if (info == null)
				{
					info = select.Select(curyInfoID);
					PXCurrencyHelper.PopulatePrecision(select.Cache, info);
					StoreCached(select, info);
				}
				return info;
			}
			return select.Cache.Current as CurrencyInfo;
		}

		public static void StoreCached(PXSelectBase<CurrencyInfo> select, CurrencyInfo info)
		{
			if (select.Cache.Locate(info) == null)
			{
				select.Cache.SetStatus(info, PXEntryStatus.Notchanged);
			}
		}

		public static void StoreCached(PXSelectBase<CurrencyInfo> select, long? curyInfoID)
		{
			GetInfo(select, curyInfoID);
		}
	}

	public static class PaymentEntry
	{
		
		public static void CalcBalances<TInvoice, TAdjustment>(PXSelectBase<CurrencyInfo> currencyinfoselect, long? PaymentCuryInfoID, DateTime? PayDate, TInvoice voucher, TAdjustment adj)
			where TInvoice : class, IBqlTable, IInvoice
			where TAdjustment : class, IBqlTable, IAdjustment
		{
			CurrencyInfo voucher_info = CurrencyInfoCache.GetInfo(currencyinfoselect,voucher.CuryInfoID);
			CurrencyInfo voucher_payinfo = PXCache<CurrencyInfo>.CreateCopy(voucher_info);
			voucher_payinfo.CuryInfoID = null;
			
			using (ReadOnlyScope rs = new ReadOnlyScope(currencyinfoselect.Cache))
			{
				voucher_payinfo = (CurrencyInfo)currencyinfoselect.Cache.Insert(voucher_payinfo);
				//currencyinfoselect.Cache.SetValueExt<CurrencyInfo.curyEffDate>(voucher_payinfo, PayDate);
				voucher_payinfo.SetCuryEffDate(currencyinfoselect.Cache, PayDate);
				currencyinfoselect.Cache.Update(voucher_payinfo);

				CalcBalances<TInvoice, TAdjustment>(currencyinfoselect, PaymentCuryInfoID, voucher_payinfo.CuryInfoID, voucher, adj);

				currencyinfoselect.Cache.Delete(voucher_payinfo);
			}
		}

		public static void CuryConvCury(decimal? BaseAmt, out decimal? CuryAmt, decimal CuryRate, string CuryMultDiv, int CuryPrecision)
		{
			if (CuryMultDiv == "D" && BaseAmt != null)
			{
				CuryAmt = Math.Round((decimal)BaseAmt * CuryRate, CuryPrecision, MidpointRounding.AwayFromZero);
			}
			else if (CuryRate != 0m && BaseAmt != null)
			{
				CuryAmt = Math.Round((decimal)BaseAmt / CuryRate, CuryPrecision, MidpointRounding.AwayFromZero);
			}
			else
			{
				CuryAmt = BaseAmt;
			}
		}

		public static void CuryConvBase(decimal? CuryAmt, out decimal? BaseAmt, decimal CuryRate, string CuryMultDiv, int BasePrecision)
		{
			if (CuryMultDiv == "M" && CuryAmt != null)
			{
				BaseAmt = Math.Round((decimal)CuryAmt * CuryRate, BasePrecision, MidpointRounding.AwayFromZero);
			}
			else if (CuryRate != 0m && CuryAmt != null)
			{
				BaseAmt = Math.Round((decimal)CuryAmt / CuryRate, BasePrecision, MidpointRounding.AwayFromZero);
			}
			else
			{
				BaseAmt = CuryAmt;
			}
		}

		public static decimal? CalcBalances(decimal? CuryDocBal, decimal? DocBal, string PayCuryID, string DocCuryID, string BaseCuryID, decimal PayCuryRate, string PayCuryMultDiv, decimal DocCuryRate, string DocCuryMultDiv, int CuryPrecision, int BasePrecision)
		{
			decimal? payment_curydocbal;
			decimal? payment_docbal;

			if (object.Equals(PayCuryID, DocCuryID))
			{
				payment_curydocbal = CuryDocBal;
			}
			else if (object.Equals(BaseCuryID, DocCuryID))
			{
				CuryConvCury(DocBal, out payment_curydocbal, PayCuryRate, PayCuryMultDiv, CuryPrecision);
			}
			else
			{
				CuryConvBase(CuryDocBal, out payment_docbal, DocCuryRate, DocCuryMultDiv, BasePrecision);
				CuryConvCury(payment_docbal, out payment_curydocbal, PayCuryRate, PayCuryMultDiv, CuryPrecision);
			}

			return payment_curydocbal;
		}

		public static void CalcBalances<TInvoice, TAdjustment>(
			PXSelectBase<CurrencyInfo> currencyinfoselect,
			long? PaymentCuryInfoID,
			long? VoucherPayCuryInfoID,
			TInvoice voucher,
			TAdjustment adj)

			where TInvoice : IInvoice
			where TAdjustment : class, IBqlTable, IAdjustment
		{
			CalcBalances<TInvoice, TAdjustment>(currencyinfoselect, PaymentCuryInfoID, VoucherPayCuryInfoID, voucher, adj, null);
		}

		/// <summary>
		/// The method to initialize application
		/// balances in Payment currency.
		/// </summary>
		public static void CalcBalances<TInvoice, TAdjustment>(
			PXSelectBase<CurrencyInfo> currencyinfoselect, 
			long? PaymentCuryInfoID, 
			long? VoucherPayCuryInfoID, 
			TInvoice voucher, 
			TAdjustment adj,
			IDocumentTran tran)

			where TInvoice : IInvoice
			where TAdjustment : class, IBqlTable, IAdjustment
		{
			decimal payment_curydocbal;
			decimal payment_docbal;

			CalcBalance(
				currencyinfoselect, 
				PaymentCuryInfoID, 
				VoucherPayCuryInfoID,
				tran?.CuryInfoID ?? voucher.CuryInfoID,
				tran?.CuryTranBal ?? voucher.CuryDocBal, 
				tran?.TranBal ?? voucher.DocBal, 
				out payment_curydocbal, 
				out payment_docbal);

			adj.CuryDocBal = payment_curydocbal;
			adj.DocBal = payment_docbal;

			CalcBalance(
				currencyinfoselect,
				PaymentCuryInfoID,
				VoucherPayCuryInfoID,
				tran?.CuryInfoID ?? voucher.CuryInfoID,
				tran?.CuryCashDiscBal ?? voucher.CuryDiscBal,
				tran?.CashDiscBal ?? voucher.DiscBal,
				out payment_curydocbal,
				out payment_docbal);

			adj.CuryDiscBal = payment_curydocbal;
			adj.DiscBal = payment_docbal;

			CalcBalance(
				currencyinfoselect,
				PaymentCuryInfoID,
				VoucherPayCuryInfoID,
				voucher.CuryInfoID,
				voucher.CuryWhTaxBal,
				voucher.WhTaxBal,
				out payment_curydocbal,
				out payment_docbal);

			adj.CuryWhTaxBal = payment_curydocbal;
			adj.WhTaxBal = payment_docbal;
		}

		public static void CalcBalance(
			PXSelectBase<CurrencyInfo> currencyinfoselect,
			long? toCuryInfoID, 
			long? fromCuryInfoID,
			long? fromOrigInfoID,
			decimal? fromCuryDocBal,
			decimal? fromDocBal,
			out decimal toCuryDocBal,
			out decimal toDocBal)
		{
			CurrencyInfo to_info = CurrencyInfoCache.GetInfo(currencyinfoselect,toCuryInfoID);
			CurrencyInfo from_originfo = CurrencyInfoCache.GetInfo(currencyinfoselect,fromOrigInfoID);

			if (Equals(to_info.CuryID, from_originfo.CuryID))
			{
				toCuryDocBal = (decimal)fromCuryDocBal;
				PXCurrencyAttribute.CuryConvBase(currencyinfoselect.Cache, to_info, (decimal)fromCuryDocBal, out toDocBal);
			}
			else if (Equals(from_originfo.CuryID, from_originfo.BaseCuryID))
			{
				toDocBal = (decimal)fromDocBal;
				PXDBCurrencyAttribute.CuryConvCury(currencyinfoselect.Cache, to_info, (decimal)fromDocBal, out toCuryDocBal);
			}
			else
			{
				CurrencyInfo from_info = CurrencyInfoCache.GetInfo(currencyinfoselect, fromCuryInfoID);

				PXCurrencyAttribute.CuryConvBase(currencyinfoselect.Cache, from_info, (decimal)fromCuryDocBal, out toDocBal, true);
				PXCurrencyAttribute.CuryConvCury(currencyinfoselect.Cache, to_info, toDocBal, out toCuryDocBal);
				PXCurrencyAttribute.CuryConvBase(currencyinfoselect.Cache, to_info, toCuryDocBal, out toDocBal);
			}
		}

		public static void CalcDiscount<TInvoice, TAdjustment>(DateTime? PayDate, TInvoice voucher, TAdjustment adj)
			where TInvoice :  IInvoice
			where TAdjustment : class, IBqlTable, IAdjustment
		{
			if (PayDate != null && voucher.DiscDate != null && ((DateTime)PayDate).CompareTo((DateTime)voucher.DiscDate) > 0)
			{
				adj.CuryDiscBal = 0m;
				adj.DiscBal = 0m;
			}
		}

		public static void WarnDiscount<TInvoice, TAdjust>(PXGraph graph, DateTime? PayDate, TInvoice invoice, TAdjust adj)
			where TInvoice : IInvoice
			where TAdjust : class, IBqlTable, IAdjustment
		{
			if (adj.Released != true && invoice.DiscDate != null && adj.AdjgDocDate != null &&
				((DateTime)adj.AdjgDocDate).CompareTo((DateTime)invoice.DiscDate) > 0 && adj.CuryAdjgDiscAmt > 0m)
			{
				graph.Caches[typeof(TAdjust)].RaiseExceptionHandling("CuryAdjgDiscAmt", adj, adj.CuryAdjgDiscAmt, new PXSetPropertyException(AR.Messages.DiscountOutOfDate, PXErrorLevel.Warning, invoice.DiscDate));
			}

		}

		public static void WarnPPDiscount<TInvoice, TAdjust>(PXGraph graph, DateTime? PayDate, TInvoice invoice, TAdjust adj, decimal? CuryAdjgPPDAmt)
			where TInvoice : IInvoice
			where TAdjust : class, IBqlTable, IAdjustment
		{
			if (adj.Released != true && invoice.DiscDate != null && adj.AdjgDocDate != null &&
				((DateTime)adj.AdjgDocDate).CompareTo((DateTime)invoice.DiscDate) > 0 && CuryAdjgPPDAmt > 0m)
			{
				graph.Caches[typeof(TAdjust)].RaiseExceptionHandling("CuryAdjgPPDAmt", adj, CuryAdjgPPDAmt, new PXSetPropertyException(AR.Messages.DiscountOutOfDate, PXErrorLevel.Warning, invoice.DiscDate));
			}
		}

		/// <summary>
		/// The method to adjust application
		/// balances in Payment currency.
		/// </summary>
		public static void AdjustBalance<TAdjustment>(
			PXSelectBase<CurrencyInfo> currencyinfoselect, 
			TAdjustment adj)
			
			where TAdjustment : class, IBqlTable, IAdjustment
		{
			AdjustBalance(
				currencyinfoselect, 
				adj, 
				adj.AdjgCuryInfoID, 
				adj.CuryAdjgAmt, 
				adj.CuryAdjgDiscAmt, 
				adj.CuryAdjgWhTaxAmt,
				true);
		}

		public static void AdjustBalance<TAdjustment>(
			PXSelectBase<CurrencyInfo> currencyinfoselect,
			TAdjustment adj,
			long? toCuryInfoID,
			decimal? curyAdjAmt,
			decimal? curyAdjDiscAmt,
			decimal? curyAdjWhTaxAmt,
			bool adjustBalanceByExtraAmounts)

			where TAdjustment : class, IBqlTable, IAdjustment
		{
			if (adj.Released == true) return;

			CurrencyInfo to_info = CurrencyInfoCache.GetInfo(currencyinfoselect,toCuryInfoID);
			decimal to_adjamt = 0m;
			decimal to_adjdiscamt = 0m;
			decimal to_adjwhtaxamt = 0m;

			if (curyAdjAmt != null)
			{
				decimal decimalValue = (decimal) curyAdjAmt;
				PXDBCurrencyAttribute.CuryConvBase(currencyinfoselect.Cache, to_info, decimalValue, out to_adjamt);

				adj.AdjAmt = to_adjamt;
				adj.CuryDocBal -= decimalValue;

				if (adj.CuryDocBal == 0m)
				{
					adj.AdjAmt = adj.DocBal;
					adj.DocBal = 0m;
				}
				else
				{
					adj.DocBal = adj.DocBal - to_adjamt;
					adj.DocBal = AdjustWhenTheSameCuryAndRate(currencyinfoselect, adj, adj.CuryDocBal, adj.DocBal);
				}
			}

			if (curyAdjWhTaxAmt != null)
			{
				decimal decimalValue = (decimal) curyAdjWhTaxAmt;
				PXDBCurrencyAttribute.CuryConvBase(currencyinfoselect.Cache, to_info, decimalValue, out to_adjwhtaxamt);

				adj.AdjWhTaxAmt = to_adjwhtaxamt;
				adj.CuryWhTaxBal -= decimalValue;
				adj.WhTaxBal = adj.CuryWhTaxBal == 0m ? 0m : adj.WhTaxBal - to_adjwhtaxamt;

				if (adjustBalanceByExtraAmounts)
				{
					adj.CuryDocBal -= decimalValue;

					if (adj.CuryDocBal == 0m)
					{
						adj.DocBal = 0m;
					}
					else
					{
						adj.DocBal = adj.DocBal - to_adjwhtaxamt;
						adj.DocBal = AdjustWhenTheSameCuryAndRate(currencyinfoselect, adj, adj.CuryDocBal, adj.DocBal);
					}
				}
			}

			if (curyAdjDiscAmt != null)
			{
				decimal decimalValue = (decimal) curyAdjDiscAmt;
				PXDBCurrencyAttribute.CuryConvBase(currencyinfoselect.Cache, to_info, decimalValue, out to_adjdiscamt);

				adj.AdjDiscAmt = to_adjdiscamt;
				adj.CuryDiscBal -= decimalValue;
				adj.DiscBal = adj.CuryDiscBal == 0m ? 0m : adj.DiscBal - to_adjdiscamt;

				if (adjustBalanceByExtraAmounts)
				{
					adj.CuryDocBal -= decimalValue;

					if (adj.CuryDocBal == 0m)
					{
						adj.DocBal = 0m;
					}
					else
					{
						adj.DocBal = adj.DocBal - to_adjdiscamt;
						adj.DocBal = AdjustWhenTheSameCuryAndRate(currencyinfoselect, adj, adj.CuryDocBal, adj.DocBal);
					}
				}
			}
		}
		/// <summary>
		/// prevent discrepancy due rounding (unexpected for user in that case)
		/// </summary>
		public static decimal? AdjustWhenTheSameCuryAndRate<TAdjustment>(
			PXSelectBase<CurrencyInfo> currencyinfoselect,
			TAdjustment adj,
			decimal? curyValue,
			decimal? value,
			bool calcRGOL = false)

			where TAdjustment : class, IBqlTable, IAdjustment
		{
			CurrencyInfo invoice_info = CurrencyInfoCache.GetInfo(currencyinfoselect,adj.AdjdOrigCuryInfoID);
			CurrencyInfo payment_info = CurrencyInfoCache.GetInfo(currencyinfoselect,adj.AdjgCuryInfoID);

			bool isSameCuryAndRate =
				invoice_info.CuryID == payment_info.CuryID &&
				invoice_info.CuryRate == payment_info.CuryRate &&
				invoice_info.RecipRate == payment_info.RecipRate;

			if (isSameCuryAndRate)
			{
				decimal calcValue = 0m;
				PXDBCurrencyAttribute.CuryConvBase(currencyinfoselect.Cache, invoice_info, (decimal)curyValue, out calcValue);
				if(calcRGOL)
					adj.RGOLAmt += ((decimal)value - calcValue);
				value = calcValue;
			}

			return value;
		}

		public static void CalcRGOL(
			PXCache curyinfocache,
			CurrencyInfo from_info,
			CurrencyInfo to_info,
			CurrencyInfo to_originfo,
			Decimal? applicationCuryBal,
			Decimal? documentCuryBal,
			Decimal? documentBaseBal,
			Decimal? fromCuryAdjAmt,
			Decimal? fromAdjAmt,
			out decimal toCuryAdjAmt,
			out decimal rgolAmt)
		{
			if (applicationCuryBal == 0m && fromCuryAdjAmt != 0m)
			{
				toCuryAdjAmt = (decimal)documentCuryBal;
				rgolAmt = (decimal)documentBaseBal - (decimal)fromAdjAmt;
			}
			else CalcRGOL(
				curyinfocache,
				from_info,
				to_info,
				to_originfo,
				fromCuryAdjAmt,
				fromAdjAmt,
				out toCuryAdjAmt,
				out rgolAmt);
		}

		public static void CalcRGOL(
			PXCache curyinfocache, 
			CurrencyInfo from_info, 
			CurrencyInfo to_info, 
			CurrencyInfo to_originfo, 
			Decimal? fromCuryAdjAmt, 
			Decimal? fromAdjAmt, 
			out decimal toCuryAdjAmt, 
			out decimal rgolAmt)
		{
			Decimal to_curyadjamt;
			Decimal to_adjamt;

			if (Equals(from_info.CuryID, to_info.CuryID))
			{
				to_curyadjamt = (decimal)fromCuryAdjAmt;
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvCury(curyinfocache, to_info, (decimal)fromAdjAmt, out to_curyadjamt);
			}

			if (Equals(from_info.CuryID, to_originfo.CuryID) && 
				Equals(from_info.CuryRate, to_originfo.CuryRate) && 
				Equals(from_info.CuryMultDiv, to_originfo.CuryMultDiv))
			{
				to_adjamt = (decimal)fromAdjAmt;
			}
			else
			{
				PXDBCurrencyAttribute.CuryConvBase(curyinfocache, to_originfo, to_curyadjamt, out to_adjamt);
			}

			toCuryAdjAmt = to_curyadjamt;
			rgolAmt = to_adjamt - (decimal)fromAdjAmt;
		}

		public static void CalcRGOL<TInvoice, TAdjustment>(
			PXSelectBase<CurrencyInfo> currencyinfoselect,
			TInvoice voucher,
			TAdjustment adj)

			where TInvoice : IInvoice
			where TAdjustment : class, IBqlTable, IAdjustment
		{
			CalcRGOL<TInvoice, TAdjustment>(currencyinfoselect, voucher, adj, null);
		}

		/// <summary>
		/// The method to calculate application RGOL
		/// from the Payment document side.
		/// </summary>
		public static void CalcRGOL<TInvoice, TAdjustment>(
			PXSelectBase<CurrencyInfo> currencyinfoselect, 
			TInvoice voucher, 
			TAdjustment adj,
			IDocumentTran tran)

			where TInvoice : IInvoice
			where TAdjustment : class, IBqlTable, IAdjustment
		{
			if (adj.CuryAdjgAmt == null || adj.CuryAdjgDiscAmt == null || adj.CuryAdjgWhTaxAmt == null) return;

			CurrencyInfo payment_info = CurrencyInfoCache.GetInfo(currencyinfoselect,adj.AdjgCuryInfoID);
			CurrencyInfo voucher_info = CurrencyInfoCache.GetInfo(currencyinfoselect,adj.AdjdCuryInfoID);
			CurrencyInfo voucher_originfo = CurrencyInfoCache.GetInfo(currencyinfoselect,adj.AdjdOrigCuryInfoID);

				decimal adj_rgol_amt;
				decimal disc_rgol_amt;
				decimal whtax_rgol_amt;
				decimal curyadjdamt;

			CalcRGOL(
				currencyinfoselect.Cache, 
				payment_info, 
				voucher_info, 
				voucher_originfo, 
				adj.CuryDiscBal, 
				tran?.CuryCashDiscBal ?? voucher.CuryDiscBal,
				tran?.CashDiscBal ?? voucher.DiscBal, 
				adj.CuryAdjgDiscAmt, 
				adj.AdjDiscAmt, 
				out curyadjdamt, 
				out disc_rgol_amt);
			adj.CuryAdjdDiscAmt = curyadjdamt;

			CalcRGOL(
				currencyinfoselect.Cache, 
				payment_info, 
				voucher_info, 
				voucher_originfo, 
				adj.CuryAdjgWhTaxAmt, 
				adj.AdjWhTaxAmt, 
				out curyadjdamt, 
				out whtax_rgol_amt);
			adj.CuryAdjdWhTaxAmt = curyadjdamt;

			decimal? curyAvailableBal = tran?.CuryTranBal ?? voucher.CuryDocBal;
			decimal? baseAvailableBal = tran?.TranBal ?? voucher.DocBal;

			if (adj.CuryDocBal == decimal.Zero)
			{
				adj.CuryAdjdAmt = curyAvailableBal - adj.CuryAdjdDiscAmt - adj.CuryAdjdWhTaxAmt;
				adj_rgol_amt = (decimal)baseAvailableBal
					- (decimal)adj.AdjDiscAmt
					- (decimal)adj.AdjWhTaxAmt
					- (decimal)adj.AdjAmt
					- disc_rgol_amt
					- whtax_rgol_amt;
			}
			else
			{
				CalcRGOL(
					currencyinfoselect.Cache,
					payment_info,
					voucher_info,
					voucher_originfo,
					adj.CuryDocBal,
					curyAvailableBal,
					baseAvailableBal,
					adj.CuryAdjgAmt,
					adj.AdjAmt,
					out curyadjdamt,
					out adj_rgol_amt);
				adj.CuryAdjdAmt = curyadjdamt;
			}

			adj.RGOLAmt = adj_rgol_amt + disc_rgol_amt + whtax_rgol_amt;
		}
	}

	public sealed class CMSetupSelect : PX.Data.PXSetupSelect<CMSetup>
	{
		public CMSetupSelect(PXGraph graph) : base(graph) { }

		protected override void FillDefaultValues(CMSetup record)
		{
			base.FillDefaultValues(record);

			record.APCuryOverride = false;
			record.APRateTypeOverride = false;
		}
	}
	public sealed class PXCuryViewStateScope : IDisposable
	{
		private readonly bool saveState;
		private readonly PXGraph graph;
		public PXCuryViewStateScope(PXGraph graph)
			: this(graph, false)
		{
		}

		public PXCuryViewStateScope(PXGraph graph, bool curyState)
		{
			this.graph = graph;
			saveState = graph.Accessinfo.CuryViewState;
			graph.Accessinfo.CuryViewState = curyState;
		}

		#region IDisposable Members

		public void Dispose()
		{
			graph.Accessinfo.CuryViewState = saveState;
		}

		#endregion
	}

	public sealed class ContactGISelector : PXCustomSelectorAttribute
	{
		public ContactGISelector()
		 : base(typeof(Search3<GIDesign.designID, OrderBy<Asc<GIDesign.name>>>))
		{
			SubstituteKey = typeof(GIDesign.name);
		}

		public IEnumerable GetRecords()
		{
			return PXGenericInqGrph.Def
				.Where(d =>
					 d.Tables.Any(t => 
						t.Name.IsIn(typeof(Contact).FullName, typeof(CRLead).FullName)
						&& (d.Relations.Any(x => x.ParentTable == t.Alias || x.ChildTable == t.Alias || x.ParentTable == null)
							|| !d.Relations.Any())))
				.Select(d => d.Design);
		}
	}
}
