using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Payroll.Data;
using PX.Api.Payroll;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRTaxType)]
	[Serializable]
	public class PRDynType : IBqlTable
	{
		#region TypeID
		public abstract class id : IBqlField { }
		[PXInt(IsKey = true)]
		[PXUIField(DisplayName = "Type ID", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public int? ID { get; set; }
		#endregion
		#region TypeName
		public abstract class name : IBqlField { }
		[PXString(20)]
		[PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible)]
		public string Name { get; set; }
		#endregion
		#region Description
		public abstract class description : IBqlField { }
		[PXString(250)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public string Description { get; set; }
		#endregion

	}

	public abstract class PRTypeSelectorBaseAttribute : PXCustomSelectorAttribute, IDynamicSettingsUpdateObserver, IPXFieldDefaultingSubscriber
	{
		protected List<PRTypeMeta> _PRTypeList = null;
		protected Type _PRTypeClass;
		private bool _ShowOnlyWithDescription;
		private bool _SetDefaultValue;
		private int? _DefaultValue;

		protected bool AllowNull { get; set; } = true;

		public PRTypeSelectorBaseAttribute(Type prTypeClass, bool showOnlyWithDescription, bool setDefaultValue, int? defaultValue = null)
			: base(typeof(PRDynType.id))
		{
			this._ShowOnlyWithDescription = showOnlyWithDescription;
			this._SetDefaultValue = setDefaultValue;
			this._DefaultValue = defaultValue;
			this.ValidateValue = false;
			this.SubstituteKey = typeof(PRDynType.name);
			if (showOnlyWithDescription)
			{
				this.DescriptionField = typeof(PRDynType.description);
			}

			_PRTypeClass = prTypeClass;
		}

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            _PRTypeList = GetAll(UseReportingType, _PRTypeClass, false);
            PXPayrollAssemblyScope.SubscribeDynamicSettingsUpdate(this);
        }

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			base.FieldVerifying(sender, e);

			if (!AllowNull && e.NewValue == null)
			{
				throw new PXSetPropertyException(Messages.FieldCantBeNull);
			}

			PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
		}

		public abstract bool UseReportingType { get; }

		protected virtual IEnumerable GetRecords()
		{
			foreach (var taxType in _PRTypeList)
			{
				if (!_ShowOnlyWithDescription || taxType.HasDescription)
				{
					var name = taxType.Name.ToUpper();
					var description = _ShowOnlyWithDescription
									  ? taxType.Description
									  : name;

					yield return new PRDynType
					{
						ID = taxType.ID,
						Name = name,
						Description = description
					};
				}
			}
		}

		protected static int? GetDefaultID(bool useReportingType, Type prTypeClass)
		{
			return GetAll(useReportingType, prTypeClass).FirstOrDefault(x => x.IsDefault)?.ID;
		}

		protected static int? GetDefaultID<PRType>(bool useReportingType) where PRType : IPRTypeClass
		{
			return GetDefaultID(useReportingType, typeof(PRType));
		}

		protected static int? GetAatrixMapping<PRType>(bool useReportingType, int itemID) where PRType : IPRTypeClass
		{
			return GetAll(useReportingType, typeof(PRType)).FirstOrDefault(x => x.ID == itemID)?.AatrixMapping?.Field;
		}

		protected static List<PRTypeMeta> GetAll(bool useReportingType, Type prTypeClass, bool useCaching = true)
		{
			Dictionary<(Type, bool), List<PRTypeMeta>> prTypeCache;
			if (useCaching && _PRTypeCache != null && _PRTypeCache.TryGetTarget(out prTypeCache) && prTypeCache.ContainsKey((prTypeClass, useReportingType)))
			{
				return prTypeCache[(prTypeClass, useReportingType)];
			}
			else
			{
				using (var payrollAssemblyScope = new PXPayrollAssemblyScope<PayrollMetaProxy>())
				{
					return payrollAssemblyScope.Proxy.GetPRTypeMeta(prTypeClass, useReportingType);
				}
			}
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!_SetDefaultValue)
				return;

			e.NewValue = _PRTypeList.FirstOrDefault(x => x.IsDefault)?.ID ?? _DefaultValue.GetValueOrDefault(0);
		}

		public void DynamicSettingsUpdated()
		{
			_PRTypeList = GetAll(UseReportingType, _PRTypeClass, false);
		}

		#region PR Type Caching
		private static readonly object CacheLock = new object();
		private static WeakReference<Dictionary<(Type, bool), List<PRTypeMeta>>> _PRTypeCache = null;

		public class PRTypeSelectorCache : IDisposable, IDynamicSettingsUpdateObserver
		{
			private Dictionary<(Type, bool), List<PRTypeMeta>> _CacheInstance;

			public PRTypeSelectorCache(Type[] cacheTypes)
			{
				lock (CacheLock)
				{
					if (_PRTypeCache != null && _PRTypeCache.TryGetTarget(out _CacheInstance))
					{
						foreach (Type t in cacheTypes)
						{
							if (!_CacheInstance.ContainsKey((t, false)))
							{
								_CacheInstance[(t, false)] = GetAll(false, t, false);
							}
							if (!_CacheInstance.ContainsKey((t, true)))
							{
								_CacheInstance[(t, true)] = GetAll(true, t, false);
							}
						}
					}
					else
					{
						_CacheInstance = new Dictionary<(Type, bool), List<PRTypeMeta>>();
						foreach (Type t in cacheTypes)
						{
							_CacheInstance[(t, false)] = GetAll(false, t, false);
							_CacheInstance[(t, true)] = GetAll(true, t, false);
						}

						if (_PRTypeCache != null)
						{
							_PRTypeCache.SetTarget(_CacheInstance);
						}
						else
						{
							_PRTypeCache = new WeakReference<Dictionary<(Type, bool), List<PRTypeMeta>>>(_CacheInstance);
						}
					}
				}

				PXPayrollAssemblyScope.SubscribeDynamicSettingsUpdate(this);
			}

			public void Dispose()
			{
				_CacheInstance = null;
				GC.Collect();
			}

			public void DynamicSettingsUpdated()
			{
				foreach ((Type t, bool isReportingType) in _CacheInstance?.Keys)
				{
					_CacheInstance[(t, isReportingType)] = GetAll(isReportingType, t, false);
				}
			}
		}
		#endregion PR Type Caching
	}

	public class PRTypeSelectorAttribute : PRTypeSelectorBaseAttribute
	{
		public PRTypeSelectorAttribute(Type prTypeClass, bool showOnlyWithDescription = true) : base(prTypeClass, showOnlyWithDescription, false) { }
		public PRTypeSelectorAttribute(Type prTypeClass, int defaultValue, bool showOnlyWithDescription = true) : base(prTypeClass, showOnlyWithDescription, true, defaultValue)
		{
			AllowNull = false;
		}

		public static int? GetDefaultReportingType<PRType>(int itemID) where PRType : IPRTypeClass
		{
			return GetAll(false, typeof(PRType)).FirstOrDefault(x => x.ID == itemID)?.DefaultReportType;
		}

		public override bool UseReportingType { get => false; }

		public static int? GetDefaultID<PRType>() where PRType : IPRTypeClass
		{
			return PRTypeSelectorBaseAttribute.GetDefaultID<PRType>(false);
		}

		public static int? GetAatrixMapping<PRType>(int itemID) where PRType : IPRTypeClass
		{
			return PRTypeSelectorBaseAttribute.GetAatrixMapping<PRType>(false, itemID);
		}

		public static List<PRTypeMeta> GetAll<PRType>()
		{
			return PRTypeSelectorBaseAttribute.GetAll(false, typeof(PRType));
		}
	}

	public class PRReportingTypeSelectorAttribute : PRTypeSelectorBaseAttribute
	{
        private PX.Payroll.TaxCategory _ReportTypeScope;
        
        public PRReportingTypeSelectorAttribute(Type prTypeClass, PX.Payroll.TaxCategory reportTypeScope = PX.Payroll.TaxCategory.Any, bool setDefaultValue = true, bool showOnlyWithDescription = true) : base(prTypeClass, showOnlyWithDescription, setDefaultValue)
		{
            _ReportTypeScope = reportTypeScope;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            if (_ReportTypeScope != PX.Payroll.TaxCategory.Any)
            {
                //_PRTypeList gets populated in base CacheAttached
                _PRTypeList = _PRTypeList.Where(x => x.ReportTypeScope == _ReportTypeScope || x.ReportTypeScope == PX.Payroll.TaxCategory.Any).ToList();
            }
        }

        public static PX.Payroll.TaxCategory? GetReportingTypeScope<PRType>(int itemID) where PRType : IPRTypeClass
		{
			return GetAll(true, typeof(PRType)).FirstOrDefault(x => x.ID == itemID)?.ReportTypeScope;
		}

		public override bool UseReportingType { get => true; }

		public static int? GetDefaultID<PRType>() where PRType : IPRTypeClass
		{
			return PRTypeSelectorBaseAttribute.GetDefaultID<PRType>(true);
		}

		public static int? GetAatrixMapping<PRType>(int itemID) where PRType : IPRTypeClass
		{
			return PRTypeSelectorBaseAttribute.GetAatrixMapping<PRType>(true, itemID);
		}

		public static List<PRTypeMeta> GetAll<PRType>()
		{
			return PRTypeSelectorBaseAttribute.GetAll(true, typeof(PRType));
		}
	}
}
