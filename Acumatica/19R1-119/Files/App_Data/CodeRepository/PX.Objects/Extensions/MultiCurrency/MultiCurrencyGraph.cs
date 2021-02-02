using System;
using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;

namespace PX.Objects.Extensions.MultiCurrency
{
	/// <summary>The generic graph extension that defines the multi-currency functionality.</summary>
	/// <typeparam name="TGraph">A <see cref="PX.Data.PXGraph" /> type.</typeparam>
	/// <typeparam name="TPrimary">A DAC (a <see cref="PX.Data.IBqlTable" /> type).</typeparam>
	public abstract class MultiCurrencyGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>, IPXCurrencyHelper
			where TGraph : PXGraph
			where TPrimary : class, IBqlTable, new()
	{
		#region IPXCurrencyHelper implementation
		public string GetCuryID()
		{
			return GetCurrencyInfo(Documents.Current)?.CuryID;
		}
		public string GetBaseCuryID()
		{
			return GetCurrencyInfo(Documents.Current).BaseCuryID;
		}
		public decimal RoundCury(decimal val)
		{
			CurrencyInfo info = GetCurrencyInfo(Documents.Current);
			if (info != null)
			{
				PopulatePrecision(currencyinfo.Cache, info);
				return Math.Round(val, (int)info.CuryPrecision, MidpointRounding.AwayFromZero);
			}
			return val;
		}

		public void CuryConvBase(decimal curyval, out decimal baseval)
		{
			CurrencyInfo info = GetCurrencyInfo(Documents.Current);
			CuryConvBase(info, curyval, out baseval);
		}

		public void CuryConvBase(CurrencyInfo info, decimal curyval, out decimal baseval)
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
					throw new CM.PXRateNotFoundException();
				}
				if (rate == 0.0m)
				{
					rate = 1.0m;
				}
				bool mult = info.CuryMultDiv != "D";
				baseval = mult ? curyval * rate : curyval / rate;

				if (info.BasePrecision != null)
					baseval = Math.Round(baseval, (int)info.BasePrecision, MidpointRounding.AwayFromZero);
			}
			else
			{
				baseval = curyval;
			}
		}

		public void CuryConvCury(decimal baseval, out decimal curyval)
		{
			CurrencyInfo info = GetCurrencyInfo(Documents.Current);
			CuryConvCury(info, baseval, out curyval);
		}

		public void CuryConvCury(CurrencyInfo info, decimal baseval, out decimal curyval)
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
					throw new CM.PXRateNotFoundException();
				}
				if (rate == 0.0m)
				{
					rate = 1.0m;
				}
				bool mult = info.CuryMultDiv == "D";
				curyval = mult ? baseval * rate : baseval / rate;

				if (info.BasePrecision != null)
					curyval = Math.Round(curyval, (int)info.BasePrecision, MidpointRounding.AwayFromZero);
			}
			else
			{
				curyval = baseval;
			}
		}

		public PXView GetView(Type table, Type field)
		{
			PXSelectBase ret = CurytoBase.FirstOrDefault(_ => table.IsAssignableFrom(_.Item1.Cache.GetItemType())).Item1;
			if (ret == null)
			{
				return new PXView(Base, false, ret.View.BqlSelect, new PXSelectDelegate(() =>
					{
						long curyInfoID = GetCurrencyInfo(Documents.Current)?.CuryInfoID ?? 0;
						int startRow = PXView.StartRow;
						int maximumRows = PXView.MaximumRows;
						int totalRows = 0;
						return ret.View.Select(null, null, null, null, null,
						new PXFilterRow[] { new PXFilterRow(field.Name, PXCondition.EQ, curyInfoID) }, ref startRow, maximumRows, ref totalRows);
					}));
			}
			return null;
		}
		#endregion

		#region Mappings
		/// <summary>A class that defines the default mapping of the <see cref="Document" /> class to a DAC.</summary>
		protected class DocumentMapping : IBqlMapping
		{
			/// <exclude />
			public Type Extension => typeof(Document);
			/// <exclude />
			protected Type _table;
			/// <exclude />
			public Type Table => _table;

			/// <summary>Creates the default mapping of the <see cref="Document" /> mapped cache extension to the specified table.</summary>
			/// <param name="table">A DAC.</param>
			public DocumentMapping(Type table)
			{
				_table = table;
			}
			/// <exclude />
			public Type BAccountID = typeof(Document.bAccountID);
			/// <exclude />
			public Type CuryInfoID = typeof(Document.curyInfoID);
			/// <exclude />
			public Type CuryID = typeof(Document.curyID);
			/// <exclude />
			public Type DocumentDate = typeof(Document.documentDate);
		}

		/// <summary>A class that defines the default mapping of the <see cref="CurySource" /> mapped cache extension to a DAC.</summary>
		protected class CurySourceMapping : IBqlMapping
		{
			/// <exclude />
			public Type Extension => typeof(CurySource);
			/// <exclude />
			protected Type _table;
			/// <exclude />
			public Type Table => _table;

			/// <summary>Creates the default mapping of the <see cref="CurySource" /> mapped cache extension to the specified table.</summary>
			/// <param name="table">A DAC.</param>
			public CurySourceMapping(Type table)
			{
				_table = table;
			}
			/// <exclude />
			public Type CuryID = typeof(CurySource.curyID);
			/// <exclude />
			public Type CuryRateTypeID = typeof(CurySource.curyRateTypeID);
			/// <exclude />
			public Type AllowOverrideCury = typeof(CurySource.allowOverrideCury);
			/// <exclude />
			public Type AllowOverrideRate = typeof(CurySource.allowOverrideRate);
		}

		/// <summary>Returns the mapping of the <see cref="Document" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
		/// <remarks>In the implementation graph for a particular graph, you  can either return the default mapping or override the default
		/// mapping in this method.</remarks>
		/// <example>
		///   <code title="Example" description="The following code shows the method that overrides the GetDocumentMapping() method in the implementation class. The  method overrides the default mapping of the %Document% mapped cache extension to a DAC: For the CROpportunity DAC, the DocumentDate field of the mapped cache extension is mapped to the closeDate field of the DAC; other fields of the extension are mapped by default." lang="CS">
		/// protected override DocumentMapping GetDocumentMapping()
		///  {
		///          return new DocumentMapping(typeof(CROpportunity)) {DocumentDate =  typeof(CROpportunity.closeDate)};
		///  }</code>
		/// </example>
		protected abstract DocumentMapping GetDocumentMapping();
		/// <summary>Returns the mapping of the <see cref="CurySource" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
		/// <remarks>In the implementation graph for a particular graph, you can either return the default mapping or override the default mapping in this method.</remarks>
		/// <example>
		///   <code title="Example" description="The following code shows the method that overrides the GetCurySourceMapping() method in the implementation class. The method returns the defaul mapping of the %CurySource% mapped cache extension to the Customer DAC." lang="CS">
		/// protected override CurySourceMapping GetCurySourceMapping()
		///  {
		///      return new CurySourceMapping(typeof(Customer));
		///  }</code>
		/// </example>
		protected abstract CurySourceMapping GetCurySourceMapping();

		protected abstract PXSelectBase[] GetChildren();

		/// <summary>A mapping-based view of the <see cref="Document" /> data.</summary>
		public PXSelectExtension<Document> Documents;
		/// <summary>A mapping-based view of the <see cref="CurySource" /> data.</summary>
		public PXSelectExtension<CurySource> CurySource;

		/// <summary>Returns the current currency source.</summary>
		/// <returns>The default implementation returns the <see cref="CurySource" /> data view.</returns>
		/// <example>
		///   <code title="Example" description="The following code shows sample implementation of the method in the implementation class." lang="CS">
		/// public PXSelect&lt;CRSetup&gt; crCurrency;
		/// protected PXSelectExtension&lt;CurySource&gt; SourceSetup =&gt; new PXSelectExtension&lt;CurySource&gt;(crCurrency);
		///  
		/// protected virtual CurySourceMapping GetSourceSetupMapping()
		/// { 
		///       return new CurySourceMapping(typeof(CRSetup)) {CuryID = typeof(CRSetup.defaultCuryID), CuryRateTypeID = typeof(CRSetup.defaultRateTypeID)};                        
		///  }
		///  
		/// protected override CurySource CurrentSourceSelect()
		/// {
		///        CurySource settings = base.CurrentSourceSelect();
		///        if (settings == null)
		///              return SourceSetup.Select();
		///        if (settings.CuryID == null || settings.CuryRateTypeID == null)
		///        {
		///              CurySource setup = SourceSetup.Select();
		///              settings = (CurySource)CurySource.Cache.CreateCopy(settings);
		///              settings.CuryID = settings.CuryID ?? setup.CuryID;
		///              settings.CuryRateTypeID = settings.CuryRateTypeID ?? setup.CuryRateTypeID;
		///        }                                    
		///        return settings;
		/// }</code>
		/// </example>
		protected virtual CurySource CurrentSourceSelect()
		{
			return CurySource.Select();
		}
		#endregion

		#region Selects and Actions
		/// <summary>The current <see cref="CurrencyInfo" /> object of the document.</summary>
		public PXSelect<CurrencyInfo>
					currencyinfo;
		protected IEnumerable currencyInfo()
		{
			CurrencyInfo info = PXSelect<CurrencyInfo,
					Where<CurrencyInfo.curyInfoID, Equal<Current<Document.curyInfoID>>>>
					.Select(Base);
			if (info != null)
			{
				info.IsReadOnly = (!Base.UnattendedMode && !Documents.AllowUpdate);
				yield return info;
			}
			yield break;
		}
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>
					currencyinfobykey;

		/// <summary>The <strong>Currency Toggle</strong> action.</summary>
		public PXAction<TPrimary> currencyView;
		[PXUIField(DisplayName = "Toggle Currency", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Money, Tooltip = CM.Messages.ToggleCurrencyViewTooltip)]
		protected virtual IEnumerable CurrencyView(PXAdapter adapter)
		{
			Base.Accessinfo.CuryViewState = !Base.Accessinfo.CuryViewState;
			PXCache cache = adapter.View.Cache;
			bool anyDiff = !cache.IsDirty;
			foreach (object ret in adapter.Get())
			{
				if (!anyDiff)
				{
					TPrimary item;
					if (ret is PXResult)
					{
						item = (TPrimary)((PXResult)ret)[0];
					}
					else
					{
						item = (TPrimary)ret;
					}
					if (item == null)
					{
						anyDiff = true;
					}
					else
					{
						TPrimary oldItem = _oldRow as TPrimary;
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
		#endregion

		#region Initialization
		protected List<Tuple<PXSelectBase, List<Tuple<string, string>>>> CurytoBase;

		public override void Initialize()
		{
			base.Initialize();

			CurytoBase = new List<Tuple<PXSelectBase, List<Tuple<string, string>>>>();
			Dictionary<Type, string> topCuryInfoIDs = new Dictionary<Type, string>();

			foreach (PXSelectBase s in GetChildren())
			{
				List<Tuple<string, string>> fields = new List<Tuple<string, string>>();
				foreach (PXEventSubscriberAttribute attr in s.Cache.GetAttributesReadonly(null))
				{
					if (attr is PXCurrencyAttribute)
					{
						fields.Add(new Tuple<string, string>(attr.FieldName, ((PXCurrencyAttribute)attr).ResultField.Name));
					}
					else if (attr is PXDBCurrencyAttribute)
					{
						fields.Add(new Tuple<string, string>(attr.FieldName, ((PXDBCurrencyAttribute)attr).ResultField.Name));
					}
					else if (attr is CurrencyInfoAttribute)
					{
						if (((CurrencyInfoAttribute)attr).IsTopLevel)
						{
							topCuryInfoIDs[s.Cache.GetItemType()] = attr.FieldName;
						}
					}
				}
				CurytoBase.Add(new Tuple<PXSelectBase, List<Tuple<string, string>>>(s, fields));
			}
			foreach (Tuple<PXSelectBase, List<Tuple<string, string>>> table in CurytoBase)
			{
				Base.RowInserting.AddHandler(table.Item1.Cache.GetItemType(),
					(s, e) => CuryRowInserting(s, e, table.Item2, topCuryInfoIDs));
				Base.RowPersisting.AddHandler(table.Item1.Cache.GetItemType(),
					(s, e) => CuryRowPersisting(s, e, table.Item2));
				foreach (Tuple<string, string> field in table.Item2)
				{
					Base.FieldUpdating.AddHandler(table.Item1.Cache.GetItemType(), field.Item2,
						(s, e) => BaseFieldUpdating(s, e, field.Item1, field.Item2));
					Base.FieldVerifying.AddHandler(table.Item1.Cache.GetItemType(), field.Item1,
						(s, e) => CuryFieldVerifying(s, e, field.Item1, field.Item2));
					Base.FieldSelecting.AddHandler(table.Item1.Cache.GetItemType(), field.Item1,
						(s, e) => CuryFieldSelecting(s, e, field.Item1, field.Item2));
				}
			}
		}
		#endregion

		#region Currency Fields Processing
		protected virtual CurrencyInfo GetCurrencyInfo(Document row)
		{
			long? key = row?.CuryInfoID;
			if (key == null)
			{
				object defaultValue;
				Documents.Cache.RaiseFieldDefaulting<Document.curyID>(row, out defaultValue);
				if (defaultValue != null)
					key = (long?)defaultValue;
			}
			CurrencyInfo info = null;
			if (key != null)
			{
				info = currencyinfo.Current;
				if (info != null)
				{
					if (!object.Equals(info.CuryInfoID, key))
					{
						info = new CurrencyInfo();
						info.CuryInfoID = key;
						info = currencyinfo.Locate(info);
						if (info == null)
						{
							info = currencyinfobykey.SelectSingle(key);
						}
					}
				}
				else
				{
					info = new CurrencyInfo();
					info.CuryInfoID = key;
					info = currencyinfo.Locate(info);
					if (info == null)
					{
						info = currencyinfobykey.SelectSingle(key);
					}
				}
			}
			return info;
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

		private static void PopulatePrecision(PXCache cache, CurrencyInfo info)
		{
			if (info != null)
			{
				if (info.CuryPrecision == null)
				{
					object prec;
					cache.RaiseFieldDefaulting<CurrencyInfo.curyPrecision>(info, out prec);
					info.CuryPrecision = Convert.ToInt16(prec);
					if (cache.GetStatus(info) == PXEntryStatus.Notchanged)
					{
						cache.SetStatus(info, PXEntryStatus.Held);
					}
				}

				if (info.BasePrecision == null)
				{
					object prec;
					cache.RaiseFieldDefaulting<CurrencyInfo.basePrecision>(info, out prec);
					info.BasePrecision = Convert.ToInt16(prec);
					if (cache.GetStatus(info) == PXEntryStatus.Notchanged)
					{
						cache.SetStatus(info, PXEntryStatus.Held);
					}
				}
			}
		}

		protected virtual void recalculateRowBaseValues(PXCache sender, object row, List<Tuple<string, string>> fields)
		{
			foreach (Tuple<string, string> field in fields)
			{
				recalculateFieldBaseValue(sender, row, sender.GetValue(row, field.Item1), field.Item1, field.Item2);
			}
		}

		protected virtual void CuryRowInserting(PXCache sender, PXRowInsertingEventArgs e, List<Tuple<string, string>> fields, Dictionary<Type, string> topCuryInfoIDs)
		{
			string curyInfoName;
			if (sender.GetItemType() != GetDocumentMapping().Table && topCuryInfoIDs.TryGetValue(sender.GetItemType(), out curyInfoName))
			{
				CurrencyInfo info = new CurrencyInfo();
				info = currencyinfo.Insert(info);
				currencyinfo.Cache.IsDirty = false;
				if (info != null)
				{
					sender.SetValue(e.Row, curyInfoName, info.CuryInfoID);
					sender.SetValue(e.Row, nameof(CurrencyInfo.curyID), info.CuryID);
				}
			}
			recalculateRowBaseValues(sender, e.Row, fields);
		}

		protected virtual void CuryRowPersisting(PXCache sender, PXRowPersistingEventArgs e, List<Tuple<string, string>> fields)
		{
			recalculateRowBaseValues(sender, e.Row, fields);
		}

		protected virtual void BaseFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e, string curyField, string baseField)
		{
			if (!FormatValue(e, sender.Graph.Culture)) return;

			CurrencyInfo info = GetCurrencyInfo(Documents.Current);

			if (info != null)
			{
				PopulatePrecision(currencyinfo.Cache, info);
				e.NewValue = Math.Round((decimal)e.NewValue, (int)(info.BasePrecision ?? (short)2), MidpointRounding.AwayFromZero);
			}
		}
		
		[Obsolete(PX.Objects.Common.InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2)]
		protected virtual void CuryFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e, string curyField, string baseField)
		{
			if (!FormatValue(e, sender.Graph.Culture)) return;

			if (Base.Accessinfo.CuryViewState
				&& e.Row != null && e.NewValue != null && object.ReferenceEquals(sender.GetValuePending(e.Row, curyField), e.NewValue))
			{
				e.NewValue = sender.GetValue(e.Row, curyField);
			}
			else
			{
				CurrencyInfo info = GetCurrencyInfo(Documents.Current);

				if (info != null)
				{
					PopulatePrecision(currencyinfo.Cache, info);
					var value = Convert.ToDecimal(e.NewValue);
					e.NewValue = Math.Round(value, info.CuryPrecision ?? 2, MidpointRounding.AwayFromZero);
				}
			}
		}
		
		protected virtual void CuryFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e, string curyField, string baseField)
		{
			recalculateFieldBaseValue(sender, e.Row, e.NewValue, curyField, baseField);
		}

		protected virtual void recalculateFieldBaseValue(PXCache sender, object row, object value, string curyField, string baseField)
		{
			CurrencyInfo info = null;
			if (value != null && (info = GetCurrencyInfo(Documents.Current)) != null && info.CuryRate != null && info.BaseCalc == true)
			{
				decimal rate = (decimal)info.CuryRate;
				if (rate == 0.0m)
				{
					rate = 1.0m;
				}
				bool mult = info.CuryMultDiv != "D";
				decimal cval = (decimal)value;
				value = mult ? cval * rate : cval / rate;
				sender.RaiseFieldUpdating(baseField, row, ref value);
				sender.SetValue(row, baseField, value);
			}
			else if (info == null || info.BaseCalc == true)
			{
				sender.RaiseFieldUpdating(baseField, row, ref value);
				sender.SetValue(row, baseField, value);
			}
		}

		protected virtual void CuryFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, string curyField, string baseField)
		{
			bool curyviewstate = Base.Accessinfo.CuryViewState;
			CurrencyInfo info = GetCurrencyInfo(Documents.Current);

			short? actualPrecision = 
				 !curyviewstate
							? (info?.CuryPrecision ?? 2)
							: (info?.BasePrecision ?? 2);
			if (curyviewstate)
			{
				recalculateFieldBaseValue(sender, e.Row, sender.GetValue(e.Row, curyField), curyField, baseField);
				e.ReturnValue = sender.GetValue(e.Row, baseField);

				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null, null, actualPrecision, null, null, baseField, null, null, null, PXErrorLevel.Undefined, curyviewstate ? (bool?)false : null, null, null, PXUIVisibility.Undefined, null, null, null);
			}
		}
		#endregion

		#region Document Events
		/// <summary>The FieldUpdated2 event handler for the <see cref="Document.BAccountID" /> field. When the BAccountID field value is changed, <see cref="Document.CuryID" /> is assigned the default
		/// value.</summary>
		/// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.FieldUpdated<Document, Document.bAccountID> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				if (e.Row == null) return;

				if (e.ExternalCall || e.Row.CuryID == null)
				{
					CurrencyInfo info = GetCurrencyInfo(e.Row);
					if (info != null)
					{
						CurrencyInfo old = PXCache<CurrencyInfo>.CreateCopy(info);
						currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyID>(info);
						currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyRateTypeID>(info);
						currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyEffDate>(info);
						if (currencyinfo.Cache.GetStatus(info) == PXEntryStatus.Notchanged || currencyinfo.Cache.GetStatus(info) == PXEntryStatus.Held)
						{
							currencyinfo.Cache.SetStatus(info, PXEntryStatus.Updated);
						}
						currencyinfo.Cache.RaiseRowUpdated(info, old);
					}
					string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(e.Cache.Graph.Caches[typeof(CurrencyInfo)], info);
					if (string.IsNullOrEmpty(message) == false)
					{
						Documents.Cache.RaiseExceptionHandling<Document.documentDate>(e.Row,
							e.Row.DocumentDate,
							new PXSetPropertyException(message, PXErrorLevel.Warning));
					}

					if (info != null)
					{
						Documents.Cache.SetValue<Document.curyID>(e.Row, info.CuryID);
					}
				}
			}
		}
		/// <summary>The FieldDefaulting2 event handler for the <see cref="Document.DocumentDate" /> field. When the DocumentDate field value is changed, <see cref="CurrencyInfo.curyEffDate"/> is changed to DocumentDate value.</summary>
		/// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.FieldUpdated<Document, Document.documentDate> e)
		{
			if (e.Row == null) return;

			CurrencyInfo info = currencyinfo.Select();
			if (info != null)
			{
				CurrencyInfo copy = PXCache<CurrencyInfo>.CreateCopy(info);
				currencyinfo.SetValueExt<CurrencyInfo.curyEffDate>(info, e.Row.DocumentDate);
				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) == false)
				{
					e.Cache.RaiseExceptionHandling<Document.documentDate>(e.Row, null, new PXSetPropertyException(message, PXErrorLevel.Warning));
				}

				currencyinfo.Cache.RaiseRowUpdated(info, copy);

				if (currencyinfo.Cache.GetStatus(info) != PXEntryStatus.Inserted)
				{
					currencyinfo.Cache.SetStatus(info, PXEntryStatus.Updated);
				}
			}
		}

		protected virtual void _(Events.FieldSelecting<Document, Document.curyID> e)
		{
			bool curyviewstate = Base.Accessinfo.CuryViewState;
			CurrencyInfo info = GetCurrencyInfo(e.Row);
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

		protected virtual void _(Events.FieldVerifying<Document, Document.curyID> e)
		{
			if (Base.Accessinfo.CuryViewState)
			{
				e.NewValue = e.Row?.CuryID;
				return;
			}
			CurrencyInfo info = GetCurrencyInfo(e.Row);
			if (info != null && !object.Equals(info.CuryID, e.NewValue))
			{
				CurrencyInfo old = PXCache<CurrencyInfo>.CreateCopy(info);
				currencyinfo.SetValueExt<CurrencyInfo.curyID>(info, e.ExternalCall ? new PXCache.ExternalCallMarker(e.NewValue) : e.NewValue);
				string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyID>(currencyinfo.Cache, info);
				if (string.IsNullOrEmpty(message) == false)
				{
					e.Cache.RaiseExceptionHandling<Document.curyID>(e.Row, e.NewValue, new PXSetPropertyException(message, PXErrorLevel.Warning));
				}

				if (currencyinfo.Cache.GetStatus(info) == PXEntryStatus.Notchanged || currencyinfo.Cache.GetStatus(info) == PXEntryStatus.Held)
				{
					currencyinfo.Cache.SetStatus(info, PXEntryStatus.Updated);
				}
				currencyinfo.Cache.RaiseRowUpdated(info, old);
			}
		}

		protected virtual void _(Events.FieldSelecting<Document, Document.curyViewState> e)
		{
			e.ReturnValue = Base.Accessinfo.CuryViewState;
		}

		protected virtual void _(Events.FieldSelecting<Document, Document.curyRate> e)
		{
			bool curyviewstate = Base.Accessinfo.CuryViewState;

			CurrencyInfo info = GetCurrencyInfo(e.Row);

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

		/// <summary>The RowSelected event handler for the <see cref="Document" /> DAC. The handler sets the value of the Enabled property of <see cref="Document.CuryID"/> according to the value of this property of <see cref="CurySource.AllowOverrideCury"/>.</summary>
		/// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.RowSelected<Document> e)
		{
			if (e.Row != null)
			{
				CurySource source = CurrentSourceSelect();
				PXUIFieldAttribute.SetEnabled<Document.curyID>(e.Cache, e.Row,
					(source == null || source.AllowOverrideCury == true) && !Base.Accessinfo.CuryViewState);
			}
		}

		object _oldRow;
		protected virtual void _(Events.RowUpdating<Document> e)
		{
			_oldRow = e.Row;
			long? key = e.NewRow.CuryInfoID;
			if (key != null && key.Value < 0L)
			{
				bool found = false;
				foreach (CurrencyInfo cached in currencyinfo.Cache.Inserted)
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
					e.Row.CuryInfoID = null;
					key = null;
				}
			}

			if (key == null)
			{
				CurrencyInfo info = new CurrencyInfo();
				info = currencyinfo.Insert(info);
				currencyinfo.Cache.IsDirty = false;
				if (info != null)
				{
					e.Cache.SetValue<Document.curyInfoID>(e.NewRow, info.CuryInfoID);
					e.Cache.SetValue<Document.curyID>(e.NewRow, info.CuryID);
				}
			}
		}

		protected virtual void _(Events.RowInserting<Document> e)
		{
			if (e.Row.CuryInfoID == null)
			{
				CurrencyInfo info = new CurrencyInfo();
				info = currencyinfo.Insert(info);
				currencyinfo.Cache.IsDirty = false;
				if (info != null)
				{
					e.Cache.SetValue<Document.curyInfoID>(e.Row, info.CuryInfoID);
					e.Cache.SetValue<Document.curyID>(e.Row, info.CuryID);
				}
			}
		}
		#endregion

		#region CurrencyInfo Events
		/// <summary>The FieldDefaulting2 event handler for the <see cref="CurrencyInfo.CuryID" /> field. The CuryID field takes the current value of <see cref="CurySource.CuryID"/>.</summary>
		/// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyID> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurySource source = CurrentSourceSelect();
				if (!string.IsNullOrEmpty(source?.CuryID))
				{
					e.NewValue = source.CuryID;
				}
				else
				{
					e.NewValue = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(Base).BaseCuryID();
				}
			}
			else
			{
				e.NewValue = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(Base).BaseCuryID();
			}
			e.Cancel = true;
		}

		protected virtual void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.baseCuryID> e)
		{
			e.NewValue = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(Base).BaseCuryID();
			e.Cancel = true;
		}

		/// <summary>The FieldDefaulting2 event handler for the <see cref="CurrencyInfo.CuryRateTypeID" /> field. The CuryRateTypeID field takes the current value of <see cref="CurySource.CuryRateTypeID"/>.</summary>
		/// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyRateTypeID> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurySource source = CurrentSourceSelect();
				if (!string.IsNullOrEmpty(source?.CuryRateTypeID))
				{
					e.NewValue = source.CuryRateTypeID;
					e.Cancel = true;
				}
				else if (e.Row != null && !String.IsNullOrEmpty(e.Row.ModuleCode))
				{
					e.NewValue = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(Base).DefaultRateTypeID(e.Row.ModuleCode);
					e.Cancel = true;
				}
			}
		}

		protected abstract void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.moduleCode> e);

		protected virtual void _(Events.FieldUpdated<CurrencyInfo, CurrencyInfo.curyRateTypeID> e)
		{
			defaultEffectiveDate(e.Cache, e.Row);
			try
			{
				defaultCurrencyRate(e.Cache, e.Row, true);
			}
			catch (PXSetPropertyException ex)
			{
				if (e.ExternalCall)
				{
					e.Cache.RaiseExceptionHandling<CurrencyInfo.curyRateTypeID>(e.Row, e.Row.CuryRateTypeID, ex);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<CurrencyInfo, CurrencyInfo.curyRateTypeID> e)
		{
			if (!PXAccess.FeatureInstalled<CS.FeaturesSet.multicurrency>())
			{
				e.Cancel = true;
			}
		}

		/// <summary>The FieldDefaulting2 event handler for the <see cref="CurrencyInfo.CuryEffDate" /> field. The CuryEffDate field takes the current value of <see cref="Document.DocumentDate"/>.</summary>
		/// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyEffDate> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				e.NewValue = Documents.Cache.Current != null && Documents.Current.DocumentDate != null
					? Documents.Current.DocumentDate
					: e.Cache.Graph.Accessinfo.BusinessDate;
			}
		}

		protected virtual void _(Events.FieldUpdated<CurrencyInfo, CurrencyInfo.curyEffDate> e)
		{
			try
			{
				defaultCurrencyRate(e.Cache, e.Row, true);
			}
			catch (PXSetPropertyException ex)
			{
				e.Cache.RaiseExceptionHandling<CurrencyInfo.curyEffDate>(e.Row, e.Row.CuryEffDate, ex);
			}
		}

		protected virtual void _(Events.FieldUpdated<CurrencyInfo, CurrencyInfo.curyID> e)
		{
			defaultEffectiveDate(e.Cache, e.Row);
			try
			{
				defaultCurrencyRate(e.Cache, e.Row, true);
			}
			catch (PXSetPropertyException ex)
			{
				e.Cache.RaiseExceptionHandling<CurrencyInfo.curyID>(e.Row, e.Row.CuryID, ex);
			}
			e.Row.CuryPrecision = null;
		}

		protected bool? currencyInfoDirty;

		protected virtual void _(Events.RowUpdating<CurrencyInfo> e)
		{
			if (e.Row.IsReadOnly == true)
			{
				e.Cancel = true;
			}
			else
			{
				currencyInfoDirty = e.Cache.IsDirty;
			}
		}

		protected virtual void _(Events.RowUpdated<CurrencyInfo> e)
		{
			if ((String.IsNullOrEmpty(e.Row.CuryID) || String.IsNullOrEmpty(e.Row.BaseCuryID)))
			{
				e.Row.BaseCuryID = e.OldRow.BaseCuryID;
				e.Row.CuryID = e.OldRow.CuryID;
			}
			if (currencyInfoDirty == false
				&& e.Row.CuryID == e.OldRow.CuryID
				&& e.Row.CuryRateTypeID == e.OldRow.CuryRateTypeID
				&& e.Row.CuryEffDate == e.OldRow.CuryEffDate
				&& e.Row.CuryMultDiv == e.OldRow.CuryMultDiv
				&& e.Row.CuryRate == e.OldRow.CuryRate)
			{
				e.Cache.IsDirty = false;
				currencyInfoDirty = null;
			}
			foreach (Tuple<PXSelectBase, List<Tuple<string, string>>> child in CurytoBase)
			{
				if (child.Item1.Cache.GetItemType() == GetDocumentMapping().Table)
				{
					recalculateRowBaseValues(Documents.Cache, Documents.Current, child.Item2);
				}
				else
				{
					foreach (object result in child.Item1.View.SelectMulti())
					{
						object row = result is PXResult ? ((PXResult)result)[0] : result;
						recalculateRowBaseValues(child.Item1.Cache, row, child.Item2);
					}
				}
			}
		}
		
		/// <summary>The RowSelected event handler for the <see cref="CurrencyInfo" /> DAC. The handler sets the values of the Enabled property of the UI fields of <see cref="CurrencyInfo"/> according to the values of this property of the corresponding fields of <see cref="CurySource"/>.</summary>
		/// <param name="e">Parameters of the event.</param>
		protected virtual void _(Events.RowSelected<CurrencyInfo> e)
		{
			if (e.Row != null)
			{
				bool curyenabled = true;
				CurySource source = CurrentSourceSelect();

				if (source != null && source.AllowOverrideRate != true
					|| e.Row.IsReadOnly == true || (e.Row.CuryID == e.Row.BaseCuryID))
				{
					curyenabled = false;
				}

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyMultDiv>(e.Cache, e.Row, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.baseCuryID>(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.displayCuryID>(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyID>(e.Cache, e.Row, true);

				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(e.Cache, e.Row, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(e.Cache, e.Row, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(e.Cache, e.Row, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(e.Cache, e.Row, curyenabled);
			}
		}

		protected virtual void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyPrecision> e)
		{
			e.NewValue = Convert.ToInt16(ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(Base)
						.CuryDecimalPlaces(e.Row.CuryID));
		}

		protected virtual void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.basePrecision> e)
		{
			e.NewValue = Convert.ToInt16(ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(Base)
						.CuryDecimalPlaces(e.Row.BaseCuryID));
		}

		protected virtual void defaultEffectiveDate(PXCache sender, CurrencyInfo info)
		{
			object newValue;
			if (sender.RaiseFieldDefaulting<CurrencyInfo.curyEffDate>(this, out newValue))
			{
				sender.RaiseFieldUpdating<CurrencyInfo.curyEffDate>(this, ref newValue);
			}
			info.CuryEffDate = (DateTime?)newValue;
		}

		protected virtual void defaultCurrencyRate(PXCache sender, CurrencyInfo info, bool forceDefault)
		{
			IPXCurrencyRate rate = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(Base)
				.GetRate(info.CuryID, info.BaseCuryID, info.CuryRateTypeID, info.CuryEffDate);
			if (rate != null)
			{
				DateTime? UserCuryEffDate = info.CuryEffDate;

				info.CuryEffDate = rate.CuryEffDate;
				info.CuryRate = Math.Round((decimal)rate.CuryRate, 8);
				info.CuryMultDiv = rate.CuryMultDiv;
				info.RecipRate = Math.Round((decimal)rate.RateReciprocal, 8);

				if (rate.CuryEffDate < UserCuryEffDate)
				{
					int days = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(Base)
						.GetRateEffDays(info.CuryRateTypeID);
					if (days > 0 && ((TimeSpan)(UserCuryEffDate - rate.CuryEffDate)).Days >= days)
					{
						throw new CM.PXRateIsNotDefinedForThisDateException(info.CuryRateTypeID, rate.FromCuryID, rate.ToCuryID, (DateTime)UserCuryEffDate);
					}
				}
			}
			else if (forceDefault)
			{
				if (string.Equals(info.CuryID, info.BaseCuryID))
				{
					bool dirty = sender.IsDirty;
					CurrencyInfo dflt = new CurrencyInfo();
					sender.SetDefaultExt<CurrencyInfo.curyRate>(dflt);
					sender.SetDefaultExt<CurrencyInfo.curyMultDiv>(dflt);
					sender.SetDefaultExt<CurrencyInfo.recipRate>(dflt);
					info.CuryRate = Math.Round((decimal)dflt.CuryRate, 8);
					info.CuryMultDiv = dflt.CuryMultDiv;
					info.RecipRate = Math.Round((decimal)dflt.RecipRate, 8);
					sender.IsDirty = dirty;
				}
				else if (info.CuryRateTypeID == null || info.CuryEffDate == null)
				{
					info.CuryRate = null;
					info.RecipRate = null;
					info.CuryMultDiv = "M";
				}
				else
				{
					info.CuryRate = null;
					info.RecipRate = null;
					info.CuryMultDiv = "M";
					throw new PXSetPropertyException(CM.Messages.RateNotFound, PXErrorLevel.Warning);
				}
			}
		}

		protected virtual bool checkRateVariance(PXCache sender, CurrencyInfo info)
		{
			return false;
		}

		protected virtual void _(Events.FieldUpdated<CurrencyInfo, CurrencyInfo.sampleRecipRate> e)
		{
			if (e.ExternalCall)
			{
				decimal rate = Math.Round((decimal)e.Row.SampleRecipRate, 8);
				if (rate == 0)
					rate = 1;
				e.Row.CuryRate = rate;
				e.Row.RecipRate = Math.Round((decimal)(1 / rate), 8);
				e.Row.CuryMultDiv = "D";
				if (checkRateVariance(e.Cache, e.Row))
				{
					PXUIFieldAttribute.SetWarning<CurrencyInfo.sampleRecipRate>(e.Cache, e.Row, CM.Messages.RateVarianceExceeded);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<CurrencyInfo, CurrencyInfo.baseCuryID> e)
		{
			e.Row.BasePrecision = null;
		}

		protected virtual void _(Events.FieldUpdated<CurrencyInfo, CurrencyInfo.sampleCuryRate> e)
		{
			if (!e.ExternalCall) return;

			decimal rate = Math.Round((decimal)e.Row.SampleCuryRate, 8);

			bool hasCurrencyRateDefaulted = false;

			if (rate == 0)
			{
				try
				{
					defaultCurrencyRate(e.Cache, e.Row, true);
					hasCurrencyRateDefaulted = true;
				}
				catch (PXSetPropertyException)
				{
					rate = 1;
				}
			}

			if (!hasCurrencyRateDefaulted)
			{
				e.Row.CuryRate = rate;
				e.Row.RecipRate = Math.Round(1m / rate, 8);
				e.Row.CuryMultDiv = CuryMultDivType.Mult;
			}

			if (checkRateVariance(e.Cache, e.Row))
			{
				PXUIFieldAttribute.SetWarning<CurrencyInfo.sampleCuryRate>(
					e.Cache,
					e.Row,
					CM.Messages.RateVarianceExceeded);
			}
		}
		#endregion
	}
}
