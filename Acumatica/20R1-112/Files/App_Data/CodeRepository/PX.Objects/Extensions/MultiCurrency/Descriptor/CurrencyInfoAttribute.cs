using System;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.CM.Extensions
{
	/// <summary>
	/// Manages Foreign exchage data for the document or the document detail.
	/// When used for the detail useally a reference to the parent document is passed through ParentCuryInfoID in a constructor.
	/// </summary>
	/// <example>
	/// [CurrencyInfo(ModuleCode = "AR")]  - Document declaration
	/// [CurrencyInfo(typeof(ARRegister.curyInfoID))] - Detail declaration
	/// </example>
	public class CurrencyInfoAttribute : PXDBDefaultAttribute, IPXReportRequiredField, IPXDependsOnFields
	{
		#region State
		protected Dictionary<long, string> _Matches;
		#endregion

		#region Ctor
		public CurrencyInfoAttribute()
		{
		}

		protected override void EnsureIsRestriction(PXCache sender)
		{
			if (_IsRestriction.Value == null)
			{
				_IsRestriction.Value = true;
			}
		}

		public CurrencyInfoAttribute(Type ParentCuryInfoID)
			: base(ParentCuryInfoID)
		{
		}
		#endregion

		#region Implementation
		public virtual bool IsTopLevel
		{
			get
			{
				return _SourceType == null || typeof(CurrencyInfo).IsAssignableFrom(_SourceType);
			}
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object key;
			if (IsTopLevel && (key = sender.GetValue(e.Row, FieldName)) != null)
			{
				PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];
				CurrencyInfo info = (CurrencyInfo)cache.Locate(new CurrencyInfo { CuryInfoID = Convert.ToInt64(key) });
				PXEntryStatus status = PXEntryStatus.Notchanged;
				if (info != null && ((status = cache.GetStatus(info)) == PXEntryStatus.Inserted))
				{
					cache.PersistInserted(info);
				}
				else if (status == PXEntryStatus.Updated)
				{
					cache.PersistUpdated(info);
				}
				else if (status == PXEntryStatus.Deleted)
				{
					cache.PersistDeleted(info);
				}
			}
			base.RowPersisting(sender, e);
		}

		public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (IsTopLevel && e.TranStatus != PXTranStatus.Open)
			{
				PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];
				cache.Persisted(e.TranStatus == PXTranStatus.Aborted);
			}
			base.RowPersisted(sender, e);
		}

		protected virtual void RowSelectingCollectMatches(PXCache sender, PXRowSelectingEventArgs e)
		{
			long? id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			if (id != null)
			{
				string cury = (string)sender.GetValue(e.Row, "CuryID");
				if (!String.IsNullOrEmpty(cury))
				{
					_Matches[(long)id] = cury;
				}
			}
		}

		public ISet<Type> GetDependencies(PXCache cache)
		{
			var res = new HashSet<Type>();
			var field = cache.GetBqlField("CuryID");
			if (field != null) res.Add(field);
			return res;
		}
		#endregion

		#region Runtime
		/// <exclude/>
		public static long? GetPersistedCuryInfoID(PXCache sender, long? curyInfoID)
		{
			if (curyInfoID == null || curyInfoID.Value > 0L)
			{
				return curyInfoID;
			}
			foreach (var attr in sender.GetAttributesReadonly(null))
			{
				if (attr is CurrencyInfoAttribute)
				{
					CurrencyInfoAttribute cattr = (CurrencyInfoAttribute)attr;
					object parent;
					if (cattr._SourceType != null && cattr._IsRestriction.Persisted != null && cattr._IsRestriction.Persisted.TryGetValue(curyInfoID, out parent))
					{
						long? ret = sender.Graph.Caches[cattr._SourceType].GetValue(parent, cattr._SourceField ?? cattr._FieldName) as long?;
						if (ret != null && ret.Value > 0L)
						{
							return ret;
						}
					}
					return curyInfoID;
				}
			}
			return curyInfoID;
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (sender.GetBqlField("CuryID") != null
				&& (_Matches = CurrencyInfo.CuryIDStringAttribute.GetMatchesDictionary(sender)) != null)
			{
				sender.Graph.RowSelecting.AddHandler(sender.GetItemType(), RowSelectingCollectMatches);
			}
		}
		#endregion
	}
}
