// This File is Distributed as Part of Acumatica Shared Source Code 
/* ---------------------------------------------------------------------*
*                               Acumatica Inc.                          *
*              Copyright (c) 1994-2011 All rights reserved.             *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ProjectX PRODUCT.        *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Collections;
using System.Threading;
using PX.Api.Soap.Screen;
using PX.Common;
using PX.Data.EP;
using PX.SM;
using PX.Web.UI;
using PX.Api;
using System.Collections.Concurrent;

namespace PX.Data
{
	#region PXNameAttribute
	/// <summary>The base class for <tt>PXCacheName</tt>
	/// and <tt>PXViewName</tt> attributes. Do not use
	/// this attribute directly.</summary>
	public class PXNameAttribute : Attribute
	{
		/// <summary>Gets the value specified as the name in the
		/// constructor.</summary>
		public string Name
		{
			get { return this._name; }
		}
		protected string _name;
		/// <exclude/>
		public virtual string GetName()
		{
			string prefix;
			return PXMessages.Localize(_name, out prefix);
		}
		/// <summary>
		/// Initializes a new instance of the attribute that assigns the provided name to the object.
		/// </summary>
		/// <param name="name">The value used as the name of the object.</param>
		public PXNameAttribute(string name)
		{
			_name = name;
		}
	}
	#endregion

	#region PXCacheNameAttribute
	/// <summary>Sets the user-friendly name and type of the data access class (DAC).</summary>
	/// <remarks>
	///   <para>The attribute is added to the DAC declaration. The name can be obtained at run time through the
	/// <see cref="PXUIFieldAttribute.GetItemName(PXCache)">GetItemName(PXCache)</see> static method of the <tt>PXUIField</tt> attribute.</para>
	///	  <para>The CacheGlobal flag can be obtained at run time through the <see cref="UseGlobalCache(Type)"/> static method of the current attribute.</para>
	///   <para>The attribute is used on the Generic Inquiry (SM208000) form to filter the list of DACs during the selection. By default, the user is suggested to
	/// use DACs with the non-empty attribute.</para>
	/// </remarks>
	/// <example>
	///   <code title="" description="" lang="CS">
	/// [PXCacheName("Currency Info", PXDacType.Config)]
	/// public partial class CurrencyInfo : PX.Data.IBqlTable
	/// {
	///     ...
	/// }</code>
	/// </example>
	///  <example>
	///   <code title="" description="" lang="CS">
	/// [PXCacheName("Inventory Item", PXDacType.Catalogue, CacheGlobal = true)]
	/// public partial class InventoryItem : PX.Data.IBqlTable
	/// {
	///     ...
	/// }</code>
	/// </example>
	public class PXCacheNameAttribute : PXNameAttribute
	{
		/// <summary>
		/// Gets the <see cref="PXDacType"/> value specified as the type of DAC in the constructor. 
		/// By default, the property is set to <see cref="PXDacType.Unknown"/>.
		/// </summary>
		public PXDacType DacType
		{
			get; private set;
		}

		/// <summary>
		/// If the flag set corresponding type rows should cache.
		/// By default, the data records does not cache.
		/// The flag can be obtained at run time through the <see cref="UseGlobalCache(Type)"/> static method of the current attribute.
		/// </summary>
		public bool CacheGlobal { get; set; }

		/// <summary>
		/// Initializes a new instance that assigns the specified name to the DAC.
		/// </summary>
		/// <param name="name">The name to assign to the DAC.</param>
		public PXCacheNameAttribute(string name)
			: this(name, PXDacType.Unknown)
		{
		}

		/// <summary>
		/// Initializes a new instance that assigns the specified name and type to the DAC.
		/// </summary>
		/// <param name="name">The name to assign to the DAC.</param>
		/// <param name="dacType">The type to assign to the DAC. This parameter is optional. By default, the property is set to <see cref="PXDacType.Unknown"/></param>
		public PXCacheNameAttribute(string name, PXDacType dacType)
			: base(name)
		{
			DacType = dacType;
		}

		public virtual string GetName(object row)
		{
			return GetName();
		}

		#region Runtime

		private static ConcurrentDictionary<Type, bool> GlobalCacheUses;

		/// <summary>Returns the flag of the data access class (DAC).
		/// The flag is set using the <see cref="CacheGlobal"/> property of the <see cref="PXCacheNameAttribute"/> attribute.</summary>
		/// <param name="entityType">data access class (DAC)</param>
		public static bool UseGlobalCache(Type entityType)
		{
			bool useGlobalCache;
			if (GlobalCacheUses == null)
				GlobalCacheUses = new ConcurrentDictionary<Type, bool>();
			else if (GlobalCacheUses.TryGetValue(entityType, out useGlobalCache))
				return useGlobalCache;
			var attribute = entityType.GetCustomAttributes<PXCacheNameAttribute>(false).FirstOrDefault();
			useGlobalCache = attribute?.CacheGlobal == true;
			GlobalCacheUses.TryAdd(entityType, useGlobalCache);
			return useGlobalCache;
		}

		#endregion
	}

	/// <summary>
	/// Type(category) of data access class (DAC).
	/// </summary>
	[Serializable]
	public enum PXDacType
	{
		/// <summary>
		/// Default value, not set
		/// </summary>
		Unknown,
		/// <summary>
		/// The type to assign to the DAC which have a config fields.
		/// </summary>
		Config,
		/// <summary>
		/// The type to assign to the DAC which is a catalogue.
		/// </summary>
		Catalogue,
		/// <summary>
		/// The type to assign to the DAC which is a document.
		/// </summary>
		Document,
		/// <summary>
		/// The type to assign to the DAC which is a document detail.
		/// </summary>
		Details,
		/// <summary>
		/// The type to assign to the DAC which have a history of operations.
		/// </summary>
		History,
		/// <summary>
		/// The type to assign to the DAC which have a balance fields.
		/// </summary>
		Balance
	}

	#endregion

	#region PXViewNameAttribute
	/// <summary>Defines the user-friendly name of a data view.</summary>
	/// <remarks>The attribute is added to the view declaration.</remarks>
	/// <example>
	/// 	<code title="Example" description="The code below shows the usage of the PXViewName attribute on the data view definition in a graph. (Messages.Orders is a constant defined by the application.)" lang="CS">
	/// [PXViewName(Messages.Orders)]
	/// public PXSelectReadonly&lt;SOOrder,
	///     Where&lt;SOOrder.customerID, Equal&lt;Current&lt;BAccount.bAccountID&gt;&gt;&gt;&gt;
	///     Orders;</code>
	/// </example>
	public class PXViewNameAttribute : PXNameAttribute
	{
		/// <summary>
		/// Initializes a new instance that sets the provided string as
		/// the user-friendly name of the data view.
		/// </summary>
		/// <param name="name">The string used as the name of the data view.</param>
		public PXViewNameAttribute(string name)
			: base(name)
		{
		}
	}
	#endregion

	#region PXEMailSourceAttibute
	/// <example>
	/// 	<code title="Example" description="The code below shows the use of the attribute on the declaration of a DAC." lang="CS">
	/// [System.SerializableAttribute()]
	/// [PXPrimaryGraph(typeof(ARStatementUpdate))]
	/// [PXEMailSource]
	/// public partial class ARStatement : PX.Data.IBqlTable
	/// { ... }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Class)]
	public class PXEMailSourceAttribute : Attribute
	{
		private readonly Type[] _types;

		/// 
		public PXEMailSourceAttribute() { }

		/// 
		public PXEMailSourceAttribute(params Type[] types)
		{
			_types = types;
		}

		/// <summary>Get.</summary>
		public Type[] Types
		{
			get { return _types ?? new Type[0]; }
		}
	}
	#endregion

	#region PXHiddenAttribute
	/// <summary>Hides a data access class (DAC), the business logic controller (graph), or the view from the selectors of DACs and graphs as well as from the Web Service API clients.</summary>
	/// <remarks>If you want an object to be visible to the API Web Service, you can set the <tt>ServiceVisible</tt> property to <tt>true</tt>.
	/// If you derive a class from another class with the <tt>PXHidden</tt> attribute then such a class will be hidden as well.
	/// </remarks>
	/// <example>
	/// 	<code title="Example" description="In the example below, the attribute is placed on the DAC declaration." lang="CS">
	/// [Serializable]
	/// [PXHidden]
	/// public partial class ActivitySource : IBqlTable { ... }</code>
	/// 	<code title="Example2" description="In the example below, the attribute is placed on the graph declaration." groupname="Example" lang="CS">
	/// [PXHidden()]
	/// public class CAReleaseProcess : PXGraph&lt;CAReleaseProcess&gt; { ... }</code>
	/// 	<code title="Example3" description="In the example below, the attribute is placed on the view declaration in some graph." groupname="Example2" lang="CS">
	/// [PXHidden]
	/// public PXSelect&lt;CurrencyInfo&gt; CurrencyInfoSelect;</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class PXHiddenAttribute : Attribute
	{
		private bool _ServiceVisible;
		/// <summary>Gets or sets the value that indicates whether the object
		/// marked with the attribute is visible to the Web Service API (in
		/// particular, to the Report Designer). By default the property
		/// equals <tt>false</tt>, and the object is hidden from all
		/// selectors.</summary>
		public bool ServiceVisible
		{
			get
			{
				return _ServiceVisible;
			}
			set
			{
				_ServiceVisible = value;
			}
		}

		/// <exclude/>
		public Type Target { get; set; }
	}
	#endregion

	#region PXPrimaryGraphBaseAttribute

	/// <exclude/>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public abstract class PXPrimaryGraphBaseAttribute : Attribute
	{
		public abstract Type GetGraphType(PXCache cache, ref object row, bool checkRights, Type preferedType);
		public virtual bool UseParent { get; set; }
	}

	#endregion

	#region PXPrimaryGraphAttribute
	/// <summary>Sets the primary graph for the DAC. The primary graph
	/// determines the default page where a user is redirected for editing a
	/// data record.</summary>
	/// <remarks>
	/// 	<para>The attribute can be placed on the following
	/// declarations:</para>
	/// 	<list type="bullet">
	/// 		<item>
	/// 			<description>On the DAC to specify the
	/// primary graph for this DAC.</description>
	/// 		</item>
	/// 		<item>
	/// 			<description>On
	/// the graph to indicate that it is the primary graph for the specified
	/// DACs.</description>
	/// 		</item>
	/// 	</list>
	/// 	<para>The second methods overrides the primary graph set by the first
	/// method.</para>
	/// 	<para>You can specify several graphs and a set of the corresponding
	/// conditions. In this case, the first graph for which the condition
	/// holds true at run time is considered the primary graph. A condition is
	/// a BQL query based on either the <tt>Where</tt> class or the
	/// <tt>Select</tt> class.</para>
	/// </remarks>
	/// <example>
	/// 	<code title="Example" description="In the example below, the attribute specifies the primary graph for a DAC." lang="CS">
	/// [PXPrimaryGraph(typeof(SalesPersonMaint))]
	/// public partial class SalesPerson : PX.Data.IBqlTable
	/// {
	///     ...
	/// }</code>
	/// 	<code title="Example2" description="In the example below, the attribute specifies the graph that is used as the primary graph for a DAC if the condition holds true for the data in the cache." groupname="Example" lang="CS">
	/// [PXPrimaryGraph(
	///     new Type[] { typeof(ShipTermsMaint)},
	///     new Type[] { typeof(Select&lt;ShipTerms, 
	///         Where&lt;ShipTerms.shipTermsID, Equal&lt;Current&lt;ShipTerms.shipTermsID&gt;&gt;&gt;&gt;)
	///     })]
	/// public partial class ShipTerms : PX.Data.IBqlTable
	/// {
	///     ...
	/// }</code>
	/// 	<code title="Example3" description="In the example below, the attribute specifies the graph that is used as the primary graph for a DAC if the Select statement retrieves a non-empty data set." groupname="Example2" lang="CS">
	/// [PXPrimaryGraph(
	///     new Type[] { typeof(CountryMaint)},
	///     new Type[] { typeof(Select&lt;State, 
	///         Where&lt;State.countryID, Equal&lt;Current&lt;State.countryID&gt;&gt;, 
	///             And&lt;State.stateID, Equal&lt;Current&lt;State.stateID&gt;&gt;&gt;&gt;&gt;)
	///     })]
	/// public partial class State : PX.Data.IBqlTable
	/// {
	///     ...
	/// }</code>
	/// 	<code title="Example4" description="In the example below, the attribute specifies two graphs and the corresponding Select statements. The first graph for which the Select statement returns a non-empty data set is used as the primary graph for the DAC." groupname="Example3" lang="CS">
	/// [PXPrimaryGraph(
	///     new Type[] {
	///         typeof(APQuickCheckEntry),
	///         typeof(APPaymentEntry)
	///     },
	///     new Type[] {
	///         typeof(Select&lt;APQuickCheck,
	///             Where&lt;APQuickCheck.docType, Equal&lt;Current&lt;APPayment.docType&gt;&gt;, 
	///                 And&lt;APQuickCheck.refNbr, Equal&lt;Current&lt;APPayment.refNbr&gt;&gt;&gt;&gt;&gt;),
	///         typeof(Select&lt;APPayment, 
	///             Where&lt;APPayment.docType, Equal&lt;Current&lt;APPayment.docType&gt;&gt;, 
	///                 And&lt;APPayment.refNbr, Equal&lt;Current&lt;APPayment.refNbr&gt;&gt;&gt;&gt;&gt;)
	///     })]
	/// public partial class APPayment : APRegister, IInvoice
	/// {
	///     ...
	/// }</code>
	/// </example>
	public class PXPrimaryGraphAttribute : PXPrimaryGraphBaseAttribute
	{
		#region Nested
		/// <exclude/>
		private class PrimaryAttributeInfo
		{
			public PXPrimaryGraphBaseAttribute Attribute { get; private set; }
			public object Row { get; private set; }
			public Type GraphType { get; private set; }
			public Type DeclaredType { get; private set; }
			public PXCache DeclaredCache { get; private set; }

			public PrimaryAttributeInfo(Type declaredType, PXPrimaryGraphBaseAttribute attr)
			{
				DeclaredType = declaredType;
				Attribute = attr;
			}

			public PrimaryAttributeInfo(PXPrimaryGraphBaseAttribute attr, object row, Type graphType, Type declaredType, PXCache declaredCache)
			{
				Attribute = attr;
				Row = row;
				GraphType = graphType;
				DeclaredType = declaredType;
				DeclaredCache = declaredCache;
			}
		}

		#endregion

		private static Dictionary<Type, List<Attribute>> _pgAtts;
		private static readonly object _syncObj = new object();
		protected static Dictionary<Type, List<KeyValuePair<Type, PXPrimaryGraphAttribute>>> _graphDefined;
		protected static Dictionary<Type, List<KeyValuePair<Type, PXPrimaryGraphAttribute>>> GraphDefined
		{
			get
			{
				if (_graphDefined != null) return _graphDefined;

				lock (_syncObj)
				{
					if (_graphDefined != null) return _graphDefined;

					_graphDefined = new Dictionary<Type, List<KeyValuePair<Type, PXPrimaryGraphAttribute>>>();
					foreach (Graph graph in ServiceManager.AllGraphsNotCustomized)
					{
						Type graphType = ServiceManager.GetGraphTypeByFullName(graph.GraphName);
						if (graphType == null) continue;

						foreach (Type extensionType in PXGraph._GetExtensions(graphType).AsEnumerable().Reverse())
						{
							AppendGraphsAttributes(extensionType, graphType);
						}
						AppendGraphsAttributes(graphType, null);
					}
				}
				return _graphDefined;
			}
		}

		protected bool _UseParent = true;
		protected Type _TheOnlyGraph;
		protected Type _TheOnlyDac;
		protected Type[] _GraphTypes;
		protected Type[] _Conditions;
		protected Type[] _DacTypes;
		protected Type _Filter;
		protected Type[] _Filters;
		/// <exclude/>
		public virtual Type Filter
		{
			get
			{
				return _Filter;
			}
			set
			{
				_Filter = value;
			}
		}
		/// <exclude/>
		public virtual Type[] Filters
		{
			get
			{
				return _Filters;
			}
			set
			{
				_Filters = value;
			}
		}
		
		/// <exclude/>
		public override bool UseParent
		{
			get { return _UseParent; }
			set { _UseParent = value; }
		}

		/// <summary>Initializes a new instance that will use the provided graph
		/// to edit a data record.</summary>
		/// <param name="type">The business logic controller (graph) or the DAC.
		/// The graph should derive from <tt>PXGraph</tt>. The DAC should
		/// implement <tt>IBqlTable</tt>.</param>
		public PXPrimaryGraphAttribute(Type type)
		{
			if (type.IsSubclassOf(typeof(PXGraph))) _TheOnlyGraph = type;
			if (type.GetInterface(typeof(IBqlTable).FullName) != null) _TheOnlyDac = type;
		}
		/// <summary>Initializes a new instance that will use the graph
		/// corresponding to the first satisfied condition. Provide the array of
		/// graphs and the array of corresponding conditions.</summary>
		/// <param name="type">The array of business logic controllers (graphs) or
		/// DACs. A graph should derive from <tt>PXGraph</tt>. A DAC should
		/// implement <tt>IBqlTable</tt>.</param>
		/// <param name="conditions">The array of conditions that correspond to
		/// the graphs or DACs specified in the first parameter. Specify BQL
		/// queries, either <tt>Where</tt> expressions or <tt>Select</tt>
		/// commands.</param>
		/// <example>
		/// In the example below, the attribute specifies the graph that is used as
		/// the primary graph for a DAC if the condition holds true for the data in
		/// the cache.
		/// <code>
		/// [PXPrimaryGraph(
		///     new Type[] { typeof(ShipTermsMaint)},
		///     new Type[] { typeof(Select&lt;ShipTerms, 
		///         Where&lt;ShipTerms.shipTermsID, Equal&lt;Current&lt;ShipTerms.shipTermsID&gt;&gt;&gt;&gt;)
		///     })]
		/// public partial class ShipTerms : PX.Data.IBqlTable
		/// {
		///     ...
		/// }
		/// </code>
		/// </example>
		public PXPrimaryGraphAttribute(Type[] types, Type[] conditions)
		{
			List<Type> dacs = new List<Type>();
			List<Type> graphs = new List<Type>();
			foreach (Type type in types)
			{
				if (type.IsSubclassOf(typeof(PXGraph))) graphs.Add(type);
				if (type.GetInterface(typeof(IBqlTable).FullName) != null) dacs.Add(type);
			}

			if (graphs.Count >= dacs.Count)	_GraphTypes = graphs.ToArray();
			else _DacTypes = dacs.ToArray();
			_Conditions = conditions;
		}

		internal IEnumerable<Type> GetConditions()
		{
			if (_Conditions == null) return Enumerable.Empty<Type>();
			return _Conditions;
		}

		/// <exclude/>
		public override Type GetGraphType(PXCache cache, ref object row, bool checkRights, Type preferedType)
		{
			if (_TheOnlyGraph != null)
			{
				return _TheOnlyGraph;
			}
			if (_GraphTypes == null ||
				_GraphTypes.Length == 0 ||
				_Conditions == null
				)
			{
				return null;
			}

			for (int i = 0; i < _GraphTypes.Length && i < _Conditions.Length; i++)
			{
				if (!typeof(PXGraph).IsAssignableFrom(_GraphTypes[i]) || checkRights && !PXAccess.VerifyRights(_GraphTypes[i]))
				{
					continue;
				}
				if (typeof(IBqlWhere).IsAssignableFrom(_Conditions[i]))
				{
					IBqlWhere where = (IBqlWhere)Activator.CreateInstance(_Conditions[i]);
					bool? result = null;
					object value = null;
					where.Verify(cache, row, new List<object>(), ref result, ref value);
					if (result == true && (preferedType == null || preferedType.IsAssignableFrom(_GraphTypes[i])))
					{
						if (_Filters != null && i < _Filters.Length)
						{
							_Filter = Filters[i];
						}
						return _GraphTypes[i];
					}
				}
				else if (typeof(BqlCommand).IsAssignableFrom(_Conditions[i]))
				{
					BqlCommand command = (BqlCommand)Activator.CreateInstance(_Conditions[i]);
					PXView view = new PXView(cache.Graph, false, command);
					object item = view.SelectSingleBound(new object[] { row });
					if (item != null && (preferedType == null || preferedType.IsAssignableFrom(_GraphTypes[i])))
					{
						row = item;

						if (row is PXResult)
						{
							row = ((PXResult)row)[0];
						}

						if (_Filters != null && i < _Filters.Length)
						{
							_Filter = Filters[i];
						}
						return _GraphTypes[i];
					}
				}
			}
			if (row == null && _GraphTypes != null && _GraphTypes.Length > 0)
			{
				return _GraphTypes[_GraphTypes.Length - 1];
			}
			return null;
		}
		/// <exclude/>
		public virtual Type ValidateGraphType(PXCache cache, Type graphType, Type dacType, ref object row, bool checkRights)
		{
			if (_TheOnlyDac != null)
			{
				return _TheOnlyDac;
			}
			if (_DacTypes == null ||
				_DacTypes.Length == 0 ||
				_Conditions == null
				)
			{
				return null;
			}

			for (int i = 0; i < _DacTypes.Length && i < _Conditions.Length; i++)
			{
				if (!dacType.IsAssignableFrom(_DacTypes[i])) continue;
				if (!typeof(IBqlTable).IsAssignableFrom(_DacTypes[i]) || checkRights && !PXAccess.VerifyRights(graphType))
				{
					continue;
				}
				if (typeof(IBqlWhere).IsAssignableFrom(_Conditions[i]))
				{
					IBqlWhere where = (IBqlWhere)Activator.CreateInstance(_Conditions[i]);
					bool? result = null;
					object value = null;
					where.Verify(cache, row, new List<object>(), ref result, ref value);
					if (result == true)
					{
						if (_Filters != null && i < _Filters.Length)
						{
							_Filter = Filters[i];
						}
						return graphType;
					}
				}
				else if (typeof(BqlCommand).IsAssignableFrom(_Conditions[i]))
				{
					BqlCommand command = (BqlCommand)Activator.CreateInstance(_Conditions[i]);
					PXView view = new PXView(cache.Graph, false, command);
					object item = view.SelectSingleBound(new object[] { row });
					if (item != null)
					{
						row = item;

						if (row is PXResult)
						{
							row = ((PXResult)row)[0];
						}

						if (_Filters != null && i < _Filters.Length)
						{
							_Filter = Filters[i];
						}
						return graphType;
					}
				}
			}
			return null;
		}

		/// <exclude />
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, out Type graphType)
		{
			Object row = null;
			Type declaredType = null;
			PXCache declaredCache = null;

			return FindPrimaryGraph(cache, ref row, out graphType, out declaredType, out declaredCache);
		}
		/// <exclude />
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, ref Object row, out Type graphType)
		{
			return FindPrimaryGraph(cache, true, ref row, out graphType);
		}
		/// <exclude />
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, Boolean checkRights, ref Object row, out Type graphType)
		{
			Type declaredType = null;
			PXCache declaredCache = null;
			return FindPrimaryGraph(cache, checkRights, ref row, out graphType, out declaredType, out declaredCache);
		}
		/// <exclude />
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, ref Object row, out Type graphType, out Type declaredType, out PXCache declaredCache)
		{
			return FindPrimaryGraph(cache, true, ref row, out graphType, out declaredType, out declaredCache);
		}
		/// <exclude />
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, Type preferedType, ref Object row, out Type graphType, out Type declaredType, out PXCache declaredCache)
		{
			return FindPrimaryGraph(cache, preferedType, true, ref row, out graphType, out declaredType, out declaredCache);
		}
		/// <exclude />
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, Boolean checkRights, ref Object row, out Type graphType, out Type declaredType, out PXCache declaredCache)
		{
			return FindPrimaryGraph(cache, null, checkRights, ref row, out graphType, out declaredType, out declaredCache);
		}

		/// <exclude />
		public static PXPrimaryGraphBaseAttribute FindPrimaryGraph(PXCache cache, Type preferedType, bool checkRights, ref object row, out Type graphType, out Type declaredType, out PXCache declaredCache)
		{
			PrimaryAttributeInfo info = FindPrimaryGraphs(cache, preferedType, checkRights, row).FirstOrDefault();
			if (info != null)
			{
				row = info.Row;
				graphType = info.GraphType;
				declaredType = info.DeclaredType;
				declaredCache = info.DeclaredCache;
			}
			else
			{
				graphType = null;
				declaredType = cache.GetItemType();
				declaredCache = cache;
			}
			return info?.Attribute;
		}
		private static IEnumerable<PrimaryAttributeInfo> FindPrimaryGraphs(PXCache cache, Type preferedType, bool checkRights, object row)
		{
			Type origType = null;
			Type graphType = null;
			PXCache declaredCache = cache;
			Type declaredType = declaredCache.GetItemType();
			while (declaredCache != null && declaredType != null && declaredType != origType)
			{
				origType = declaredType;

				//search graphs
				while (declaredType != null)
				{
					if (GraphDefined.ContainsKey(declaredType))
					{
						foreach (KeyValuePair<Type, PXPrimaryGraphAttribute> pair in GraphDefined[declaredType])
						{
							Type dacType = pair.Value.ValidateGraphType(cache, pair.Key, declaredType, ref row, checkRights);
							if (dacType != null)
							{
								graphType = pair.Key;
								yield return new PrimaryAttributeInfo(pair.Value, row, graphType, declaredType, declaredCache);
							}
						}
					}
					declaredType = declaredType.BaseType;
				}

				declaredType = origType;
				PXPrimaryGraphBaseAttribute attribute = null;

				//search extensions
				foreach (Type ext in cache.GetExtensionTypes().Reverse())
				{
					foreach(PrimaryAttributeInfo info in GetPrimaryAttribute(ext, true))
					{
						if (Unwrap(info, declaredCache, checkRights, preferedType, ref row, out graphType, out declaredType, out attribute))
						{
							yield return new PrimaryAttributeInfo(attribute, row, graphType, declaredType, declaredCache);
						}
					}
				}

				declaredType = origType;

				//search dacs
				bool UseParent = true;
				foreach(PrimaryAttributeInfo info in GetPrimaryAttribute(declaredType, true))
				{
					UseParent = UseParent && info.Attribute.UseParent;
					if (Unwrap(info, declaredCache, checkRights, preferedType, ref row, out graphType, out declaredType, out attribute))
					{
						yield return new PrimaryAttributeInfo(attribute, row, graphType, declaredType, declaredCache);
					}
				}
				bool found = false;
				//searching parent
				bool clear = false;
				foreach (PXEventSubscriberAttribute attr in declaredCache.GetAttributes(row, null))
				{
					if (attr is PXParentAttribute && UseParent)
					{
						found = true;
						if (row == null)
						{
							declaredType = PXParentAttribute.GetParentType(declaredCache);
							break;
						}
						else
						{
							PXView view = ((PXParentAttribute)attr).GetParentSelect(declaredCache);
							row = view.SelectSingleBound(new object[] { row });

							if (row == null)
							{
								clear = true;
								continue;
							}
							else
							{
								clear = false;
								declaredType = row.GetType();
								declaredCache = cache.Graph.Caches[declaredType];
								break;
							}
						}
					}
					if (clear)
					{
						declaredType = null;
						declaredCache = null;
					}
				}
				if (!found) yield break;
			}
		}
		
		private static IEnumerable<Attribute> GetAssemblyAttribute(Type type)
		{
			lock (_syncObj)
			{
				if (_pgAtts == null)
				{
					_pgAtts = new Dictionary<Type, List<Attribute>>();
					foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
					{
						// ignore some assemblies including dynamic ones
						if (!PXSubstManager.IsSuitableTypeExportAssembly(a, true))
						{
							continue;
						}

						try
						{
							foreach (PXDACDescriptionAttribute att in a.GetCustomAttributes(typeof(PXDACDescriptionAttribute), true))
							{
								List<Attribute> list;
								if (!_pgAtts.TryGetValue(att.Target, out list))
								{
									list = new List<Attribute>();
									_pgAtts[att.Target] = list;
								}
								list.Add(att.Attribute);
							}
						}
						catch { }
					}
				}
				List<Attribute> res;
				if (_pgAtts.TryGetValue(type, out res)) return res;
				return new Attribute[0];
			}
		}
		private static IEnumerable<PrimaryAttributeInfo> GetPrimaryAttribute(Type declaredType, bool searchBase)
		{
			if (declaredType.IsDefined(typeof(PXPrimaryGraphBaseAttribute), true))
			{
				while (declaredType != null)
				{
					Boolean isAvailable = false;
					if (typeof(IBqlTable).IsAssignableFrom(declaredType)) isAvailable = true;
					else if (declaredType.IsSubclassOf(typeof(PXGraph))) isAvailable = true;
					else if (declaredType.IsSubclassOf(typeof(PXCacheExtension))) isAvailable = true;
					else if (declaredType.IsSubclassOf(typeof(PXGraphExtension))) isAvailable = true;

					if (isAvailable)
					{
						List<Attribute> attributes = new List<Attribute>();
						attributes.AddRange(GetAssemblyAttribute(declaredType));
						attributes.AddRange(declaredType.GetCustomAttributes(false).Cast<Attribute>());
						foreach (Attribute attr in attributes)
						{
							PXPrimaryGraphBaseAttribute pAttr = attr as PXPrimaryGraphBaseAttribute;
							if (pAttr != null)
								yield return new PrimaryAttributeInfo(declaredType, pAttr);
						}
					}
					
					declaredType = searchBase ? declaredType.BaseType : null;
				}
			}
		}

		/// <exclude />
		public static IEnumerable<PXPrimaryGraphBaseAttribute> GetAttributes(PXCache cache)
		{
			return FindPrimaryGraphs(cache, null, false, null).Select(info => info.Attribute);
		}

		private static void AppendGraphsAttributes(Type type, Type primaryType)
		{
			foreach (PrimaryAttributeInfo info in GetPrimaryAttribute(type, false))
			{
				PXPrimaryGraphAttribute attr = info.Attribute as PXPrimaryGraphAttribute;
				if (attr == null || (attr._TheOnlyDac == null && (attr._DacTypes == null || attr._DacTypes.Length <= 0))) return;

				IEnumerable<Type> types = attr._TheOnlyDac != null ? new Type[] { attr._TheOnlyDac} : attr._DacTypes;
				foreach (Type dacType in types)
				{
					List<KeyValuePair<Type, PXPrimaryGraphAttribute>> list;
					if (!_graphDefined.ContainsKey(dacType)) list = _graphDefined[dacType] = list = new List<KeyValuePair<Type, PXPrimaryGraphAttribute>>();
					else list = _graphDefined[dacType];

					if (!list.Any(p => p.Key == primaryType)) list.Add(new KeyValuePair<Type, PXPrimaryGraphAttribute>(primaryType ?? type, attr));
				}
			}
		}
		private static Boolean Unwrap(PrimaryAttributeInfo info, PXCache cache, Boolean checkRights, Type preferedType, ref Object row, out Type graphType, out Type declaredType, out PXPrimaryGraphBaseAttribute attribute)
		{
			attribute = info.Attribute;
			declaredType = info.DeclaredType;

			if (preferedType != null)
			{
				Object copy = row;
				graphType = attribute.GetGraphType(cache, ref copy, checkRights, preferedType);
				if (graphType != null)
				{
					row = copy;
					return true;
				}
			}
			graphType = attribute.GetGraphType(cache, ref row, checkRights, null);
			return graphType != null;			
		}
	}
	#endregion

	#region PXDACDescriptionAttribute
	/// <exclude/>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class PXDACDescriptionAttribute : Attribute
	{
		private readonly Type _target;
		private readonly Attribute _attribute;

		public PXDACDescriptionAttribute(Type target, Attribute attribute)
		{
			if (target == null) throw new ArgumentNullException("target");
			if (attribute == null) throw new ArgumentNullException("attribute");

			_target = target;
			_attribute = attribute;
		}

		/// <summary>Get.</summary>
		public Type Target
		{
			get { return _target; }
		}

		/// <summary>Get.</summary>
		public Attribute Attribute
		{
			get { return _attribute; }
		}
	}
	#endregion

	#region PXNotCleanableCache<TNode>

	/// <exclude/>
	internal class PXNotCleanableCache<TNode> : PXCache<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXNotCleanableCache(PXGraph graph)
			: base(graph)
		{
		}

		public PXNotCleanableCache(PXGraph graph, PXCache source)
			: this(graph)
		{
			foreach (var item in source.Cached)
			{
				Insert(item);
				SetStatus(item, source.GetStatus(item));
			}
		}

		public override void Clear()
		{
			//base.Clear();
		}

		public static PXNotCleanableCache<TNode> Attach(PXGraph graph)
		{
			var state = new PXNotCleanableCache<TNode>(graph);
			state.Load();
			graph.Caches[typeof(TNode)] = state;
			graph.SynchronizeByItemType(state);
			return state;
		}

		public static PXNotCleanableCache<TNode> Attach(PXGraph graph, PXCache cache)
		{
			var state = new PXNotCleanableCache<TNode>(graph, cache);
			state.Load();
			graph.Caches[typeof(TNode)] = state;
			graph.SynchronizeByItemType(state);
			return state;
		}
	}

	#endregion

	#region PXLineNbrMarkerAttribute

	/// <exclude/>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class PXLineNbrMarkerAttribute : PXEventSubscriberAttribute { }

	#endregion

	#region PXImportAttribute
	/// <summary>Adds the table toobar button that allows the user to load data from a file to the table. The attribute is placed on the view that is used to obtain the data
	/// for the table.</summary>
	/// <remarks>
	///   <para>The attribute is placed on the view declaration in the graph. As a result, the table that uses the view as a data provider will include the button that
	/// opens the data import wizard. By using this wizard, a user can load data from an Excel or CSV file to the table.</para>
	///   <para>You can control all steps of the data import if you make the graph implement the <tt>PXImportAttribute.IPXPrepareItems</tt> interface.</para>
	/// </remarks>
	/// <example>
	///   <code title="Example" description="The following attibute adds the upload button to the toolbar of the table that uses the Transactions view for data retrieval. In this example, the primary view DAC is INRegister, and this DAC is passed to the attribute as a parameter. " lang="CS">
	/// // Primary view declaration
	/// public PXSelect&lt;INRegister,
	///     Where&lt;INRegister.docType, Equal&lt;INDocType.adjustment&gt;&gt;&gt; adjustment;
	/// ...
	///  
	/// [PXImport(typeof(INRegister))]
	/// public PXSelect&lt;INTran,
	///           Where&lt;INTran.docType, Equal&lt;Current&lt;INRegister.docType&gt;&gt;,
	///               And&lt;INTran.refNbr, Equal&lt;Current&lt;INRegister.refNbr&gt;&gt;&gt;&gt;&gt;
	///        Transactions;</code>
	///   <code title="Example2" description="In the following example, the graph implements the PXImportAttribute.IPXPrepareItems interface to control the data import." groupname="Example" lang="CS">
	/// public class APInvoiceEntry : APDataEntryGraph&lt;APInvoiceEntry, APInvoice&gt;,
	///                               PXImportAttribute.IPXPrepareItems
	/// {
	///     ...
	///     // The attribute is placed on the view declaration
	///     [PXImport(typeof(APInvoice))]
	///     public PXSelectJoin&lt;APTran,
	///         LeftJoin&lt;POReceiptLine,
	///             On&lt;POReceiptLine.receiptNbr, Equal&lt;APTran.receiptNbr&gt;, 
	///             And&lt;POReceiptLine.lineNbr, Equal&lt;APTran.receiptLineNbr&gt;&gt;&gt;&gt;,
	///         Where&lt;APTran.tranType, Equal&lt;Current&lt;APInvoice.docType&gt;&gt;,
	///             And&lt;APTran.refNbr, Equal&lt;Current&lt;APInvoice.refNbr&gt;&gt;&gt;&gt;,
	///         OrderBy&lt;Asc&lt;APTran.tranType,
	///                 Asc&lt;APTran.refNbr, Asc&lt;APTran.lineNbr&gt;&gt;&gt;&gt;&gt;
	///         Transactions;
	///     ...
	///  
	///     // Implementation of the IPXPrepareItems methods
	///     public virtual bool PrepareImportRow(
	///         string viewName, IDictionary keys, IDictionary values)
	///     {
	///         if (string.Compare(viewName, "Transactions", true) == 0)
	///         {
	///             if (values.Contains("tranType")) values["tranType"] = 
	///                 Document.Current.DocType;
	///             else values.Add("tranType", Document.Current.DocType);
	///             if (values.Contains("tranType")) values["refNbr"] = 
	///                 Document.Current.RefNbr;
	///             else values.Add("refNbr", Document.Current.RefNbr);
	///         }
	///         return true;
	///     }
	///  
	///     public bool RowImporting(string viewName, object row)
	///     {
	///         return row == null;
	///     }
	///  
	///     public bool RowImported(string viewName, object row, object oldRow)
	///     {
	///         return oldRow == null;
	///     }
	///  
	///     public virtual void PrepareItems(string viewName, IEnumerable items) { }
	///     ...
	/// }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class PXImportAttribute : PXViewExtensionAttribute
	{
		/// <exclude/>
		public class ImportMode
		{
			/// <exclude/>
			public enum Value
			{
				UpdateExisting,
				BypassExisting,
				InsertAllRecords
			}

			public const string UPDATE_EXISTING = "U";
			public const string BYPASS_EXISTING = "B";
			public const string INSERT_ALL_RECORDS = "I";

			private static readonly Dictionary<string, Value> _mapping = new Dictionary<string, Value>
			{
				{ UPDATE_EXISTING, Value.UpdateExisting },
				{ BYPASS_EXISTING, Value.BypassExisting },
				{ INSERT_ALL_RECORDS, Value.InsertAllRecords }
			};

			public static Value Parse(string value)
			{
				Value parsedValue;

				if (_mapping.TryGetValue(value, out parsedValue))
				{
					return parsedValue;
				}

				throw new PXException(ErrorMessages.WrongImportMode);
			}

		}
		/// <exclude/>
		public sealed class MappingPropertiesInitEventArgs : EventArgs
		{
			public List<string> Names { get; set; }
			public List<string> DisplayNames { get; set; }

			public MappingPropertiesInitEventArgs(List<string> names, List<string> displayNames)
			{
				Names = names;
				DisplayNames = displayNames;
			}
		}

		/// <exclude/>
		public event EventHandler<MappingPropertiesInitEventArgs> MappingPropertiesInit
		{
			add
			{
				if (_importer != null)
				{
					_importer.MappingPropertiesInit += value;
				}
			}
			remove
			{
				if (_importer != null)
				{
					_importer.MappingPropertiesInit -= value;
				}
			}
		}

		/// <exclude/>
		public sealed class CommonSettingsDialogShowingEventArgs : CancelEventArgs
		{
			public PXImportSettings Settings { get; set; }

			public CommonSettingsDialogShowingEventArgs(PXImportSettings settings)
			{
				Settings = settings;
			}
		}

		public event EventHandler<CommonSettingsDialogShowingEventArgs> CommonSettingsDialogShowing
		{
			add
			{
				if (_importer != null)
				{
					_importer.CommonSettingsDialogShowing += value;
				}
			}
			remove
			{
				if (_importer != null)
				{
					_importer.CommonSettingsDialogShowing -= value;
				}
			}
		}

		/// <exclude/>
		public sealed class MappingDialogShowingEventArgs : CancelEventArgs
		{
			public List<PXImportColumnsSettings> Settings { get; set; }

			public MappingDialogShowingEventArgs(List<PXImportColumnsSettings> settings)
			{
				Settings = settings;
			}
		}

		public event EventHandler<MappingDialogShowingEventArgs> MappingDialogShowing
		{
			add
			{
				if (_importer != null)
				{
					_importer.MappingDialogShowing += value;
				}
			}
			remove
			{
				if (_importer != null)
				{
					_importer.MappingDialogShowing -= value;
				}
			}
		}

		/// <exclude/>
		public sealed class FileUploadingEventArgs : CancelEventArgs
		{
			public byte[] Data { get; set; }
			public string FileExtension { get; set; }

			public FileUploadingEventArgs(byte[] data, string fileExtension)
			{
				Data = data;
				FileExtension = fileExtension;
			}
		}

		public event EventHandler<FileUploadingEventArgs> FileUploading
		{
			add
			{
				if (_importer != null)
				{
					_importer.FileUploading += value;
				}
			}
			remove
			{
				if (_importer != null)
				{
					_importer.FileUploading -= value;
				}
			}
		}

		/// <exclude/>
		public sealed class RowImportingEventArgs : CancelEventArgs
		{
			public IDictionary Keys { get; set; }
			public IDictionary Values { get; set; }
			public ImportMode.Value Mode { get; private set; }

			public RowImportingEventArgs(IDictionary keys, IDictionary values, ImportMode.Value mode)
			{
				Keys = keys;
				Values = values;
				Mode = mode;
			}
		}

		public event EventHandler<RowImportingEventArgs> RowImporting
		{
			add
			{
				if (_importer != null)
				{
					_importer.RowImporting += value;
				}
			}
			remove
			{
				if (_importer != null)
				{
					_importer.RowImporting -= value;
				}
			}
		}

		#region PXContentBag
		/// <exclude/>
		[Serializable]
		public class PXContentBag : IBqlTable
		{
			#region FileExtension
			/// <exclude/>
			public abstract class fileExtension : PX.Data.BQL.BqlString.Field<fileExtension> { }
			protected String _FileExtension;
			[PXString]
			public virtual String FileExtension
			{
				get
				{
					return this._FileExtension;
				}
				set
				{
					this._FileExtension = value;
				}
			}
			#endregion

			#region Data
			/// <exclude/>
			public abstract class data : PX.Data.BQL.BqlByteArray.Field<data> { }
			protected object _Data;
			[PXDBVariant]
			public virtual object Data
			{
				get
				{
					return this._Data;
				}
				set
				{
					this._Data = value;
				}
			}
			#endregion

			#region Loaded
			/// <exclude/>
			public abstract class loaded : PX.Data.BQL.BqlBool.Field<loaded> { }
			protected Boolean? _Loaded;
			[PXBool]
			[PXUIField(Visible = false)]
			public virtual Boolean? Loaded
			{
				get
				{
					return this._Loaded;
				}
				set
				{
					this._Loaded = value;
				}
			}
			#endregion
		}
		#endregion

		#region PXImportSettings
		/// <exclude/>
		[Serializable]
		public class PXImportSettings : IBqlTable
		{
			#region Index
			/// <exclude/>
			public abstract class index : PX.Data.BQL.BqlInt.Field<index> { }
			private Int32? _Index;
			[PXDBIdentity(IsKey = true)]
			[PXUIField(Visible = false)]
			public Int32? Index
			{
				get
				{
					return this._Index;
				}
				set
				{
					this._Index = value;
				}
			}
			#endregion

			#region ViewName

			/// <exclude/>
			public abstract class viewName : PX.Data.BQL.BqlString.Field<viewName> { }

			protected String _ViewName;

			[PXString]
			[PXUIField(Visible = false)]
			public virtual String ViewName
			{
				get { return _ViewName; }
				set { _ViewName = value; }
			}

			#endregion

			#region FileExtension
			/// <exclude/>
			public abstract class fileExtension : PX.Data.BQL.BqlString.Field<fileExtension> { }
			protected String _FileExtension;
			[PXString]
			[PXUIField(DisplayName = "File Extension")]
			public virtual String FileExtension
			{
				get
				{
					return this._FileExtension;
				}
				set
				{
					this._FileExtension = value;
				}
			}
			#endregion

			#region Data
			/// <exclude/>
			public abstract class data : PX.Data.BQL.BqlByteArray.Field<data> { }
			protected object _Data;
			[PXDBVariant]
			[PXUIField(Visible = false)]
			public virtual object Data
			{
				get
				{
					return this._Data;
				}
				set
				{
					this._Data = value;
				}
			}
			#endregion

			#region NullValue
			/// <exclude/>
			public abstract class nullValue : PX.Data.BQL.BqlString.Field<nullValue> { }
			protected String _NullValue;
			[PXString]
			[PXUIField(DisplayName = "Null Value")]
			public virtual String NullValue
			{
				get
				{
					return this._NullValue;
				}
				set
				{
					this._NullValue = value;
				}
			}
			#endregion

			#region Culture
			/// <exclude/>
			public abstract class culture : PX.Data.BQL.BqlInt.Field<culture> { }
			protected Int32? _Culture;
			[PXInt]
			[PXUIField(DisplayName = "Culture")]
			public Int32? Culture
			{
				get
				{
					return this._Culture;
				}
				set
				{
					this._Culture = value;
				}
			}
			#endregion

			/// <exclude/>
			public abstract class mode : PX.Data.BQL.BqlString.Field<mode> { }
			[PXString]
			[PXStringList(new[] { ImportMode.UPDATE_EXISTING, ImportMode.BYPASS_EXISTING, ImportMode.INSERT_ALL_RECORDS }, new[] { "Update Existing", "Bypass Existing", "Insert All Records" })]
			[PXUIField(DisplayName = "Mode")]
			public virtual string Mode { get; set; }
		}
		#endregion

		#region CSVSettings
		/// <exclude/>
		[Serializable]
		public sealed class CSVSettings : PXImportSettings
		{
			#region ViewName

			/// <exclude/>
			public new abstract class viewName : PX.Data.BQL.BqlString.Field<viewName> { }

			#endregion

			#region Separator
			/// <exclude/>
			public abstract class separator : PX.Data.BQL.BqlString.Field<separator> { }

			private String _Separator;
			[PXString(50)]
			[PXUIField(DisplayName = "Separator Chars")]
			public String Separator
			{
				get
				{
					return this._Separator;
				}
				set
				{
					this._Separator = value;
				}
			}
			#endregion

			#region CodePage
			/// <exclude/>
			public abstract class codePage : PX.Data.BQL.BqlInt.Field<codePage> { }

			private Int32? _CodePage;
			[PXInt]
			[PXUIField(DisplayName = "Encoding")]
			public Int32? CodePage
			{
				get
				{
					return this._CodePage;
				}
				set
				{
					this._CodePage = value;
				}
			}
			#endregion
		}
		#endregion

		#region XLSXSettings
		/// <exclude/>
		[Serializable]
		[PXHidden]
		public sealed class XLSXSettings : PXImportSettings
		{
			#region ViewName

			/// <exclude/>
			public new abstract class viewName : PX.Data.BQL.BqlString.Field<viewName> { }

			#endregion
		}
		#endregion

		#region PXImportColumnsSettings
		/// <exclude/>
		[Serializable]
		public class PXImportColumnsSettings : IBqlTable
		{
			#region Index
			/// <exclude/>
			public abstract class index : PX.Data.BQL.BqlInt.Field<index> { }
			private Int32? _Index;
			[PXDBIdentity(IsKey = true)]
			[PXUIField(Visible = false)]
			public Int32? Index
			{
				get
				{
					return this._Index;
				}
				set
				{
					this._Index = value;
				}
			}
			#endregion

			#region ViewName

			/// <exclude/>
			public abstract class viewName : PX.Data.BQL.BqlString.Field<viewName> { }

			protected String _ViewName;

			[PXString]
			[PXUIField(Visible = false)]
			public virtual String ViewName
			{
				get { return _ViewName; }
				set { _ViewName = value; }
			}

			#endregion

			#region ColumnIndex
			/// <exclude/>
			public abstract class columnIndex : PX.Data.BQL.BqlInt.Field<columnIndex> { }
			private Int32? _ColumnIndex;
			[PXDBIdentity()]
			[PXUIField(Visible = false)]
			public Int32? ColumnIndex
			{
				get
				{
					return this._ColumnIndex;
				}
				set
				{
					this._ColumnIndex = value;
				}
			}
			#endregion

			#region ColumnName
			/// <exclude/>
			public abstract class columnName : PX.Data.BQL.BqlString.Field<columnName> { }
			private String _ColumnName;
			[PXString]
			[PXUIField(DisplayName = "Column Name", Enabled = false)]
			public String ColumnName
			{
				get
				{
					return this._ColumnName;
				}
				set
				{
					this._ColumnName = value;
				}
			}
			#endregion

			#region PropertyName
			/// <exclude/>
			public abstract class propertyName : PX.Data.BQL.BqlString.Field<propertyName> { }
			private String _PropertyName;
			[PXString]
			[PXUIField(DisplayName = "Property Name")]
			public String PropertyName
			{
				get
				{
					return this._PropertyName;
				}
				set
				{
					this._PropertyName = value;
				}
			}
			#endregion
		}
		#endregion

		#region CommonSettings

		/// <exclude/>
		public struct CommonSettings
		{
			private readonly byte[] _content;
			private readonly string _nullValue;
			private readonly string _culture;
			private readonly string _mode;

			public CommonSettings(byte[] content, string nullValue, string culture, string mode)
			{
				_content = content;
				_nullValue = nullValue;
				_culture = culture;
				_mode = mode;
			}

			public byte[] Content
			{
				get { return _content; }
			}

			public string NullValue
			{
				get { return _nullValue; }
			}

			public string Culture
			{
				get { return _culture; }
			}

			public string Mode
			{
				get { return _mode; }
			}
		}

		#endregion

		#region Interfaces

		/// <exclude/>
		public interface IPXImportWizard
		{
			bool TryUploadData(byte[] content, string ext);
			void RunWizard();
			void PreRunWizard();
			event CreateImportRowEventHandler OnCreateImportRow;
			event RowImportingEventHandler OnRowImporting;
			event RowImportedEventHandler OnRowImported;
			event ImportDoneEventHandler OnImportDone;
			event EventHandler<MappingPropertiesInitEventArgs> MappingPropertiesInit;
			event EventHandler<CommonSettingsDialogShowingEventArgs> CommonSettingsDialogShowing;
			event EventHandler<MappingDialogShowingEventArgs> MappingDialogShowing;
			event EventHandler<FileUploadingEventArgs> FileUploading;
			event EventHandler<RowImportingEventArgs> RowImporting;
		}

		/// <exclude/>
		public sealed class CreateImportRowEventArguments
		{
			private readonly IDictionary _keys;
			private readonly IDictionary _values;
			private readonly ImportMode.Value _mode;

			public CreateImportRowEventArguments(IDictionary keys, IDictionary values, string mode)
			{
				if (keys == null) throw new ArgumentNullException("keys");
				if (values == null) throw new ArgumentNullException("values");

				_keys = keys;
				_values = values;
				_mode = ImportMode.Parse(mode);
			}

			public IDictionary Keys
			{
				get { return _keys; }
			}

			public IDictionary Values
			{
				get { return _values; }
			}

			public bool Cancel { get; set; }

			public ImportMode.Value Mode
			{
				get { return _mode; }
			}
		}

		/// <exclude/>
		public sealed class RowImportingEventArguments
		{
			private readonly object _row;

			public RowImportingEventArguments(object row)
			{
				_row = row;
			}

			public bool Cancel { get; set; }

			public object Row
			{
				get { return _row; }
			}
		}

		/// <exclude/>
		public sealed class RowImportedEventArguments
		{
			private readonly object _row;
			private readonly object _oldRow;

			public RowImportedEventArguments(object row, object oldRow)
			{
				if (row == null) throw new ArgumentNullException("row");

				_row = row;
				_oldRow = oldRow;
			}

			public bool Cancel { get; set; }

			public object Row
			{
				get { return _row; }
			}

			public object OldRow
			{
				get { return _oldRow; }
			}
		}

		/// <exclude/>
		public sealed class ImporDoneEventArguments
		{
			private readonly ImportMode.Value _mode;

			public ImporDoneEventArguments(string mode)
			{
				if (mode == null) throw new ArgumentNullException("mode");
				_mode = ImportMode.Parse(mode);
			}

			public ImporDoneEventArguments(ImportMode.Value mode)
			{
				_mode = mode;
			}

			public ImportMode.Value Mode
			{
				get { return _mode; }
			}
		}

		/// <exclude/>
		public delegate void CreateImportRowEventHandler(CreateImportRowEventArguments args, IPXPrepareItems prepareItemsHandler);

		/// <exclude/>
		public delegate void RowImportingEventHandler(RowImportingEventArguments args, IPXPrepareItems prepareItemsHandler);

		/// <exclude/>
		public delegate void RowImportedEventHandler(RowImportedEventArguments args, IPXPrepareItems prepareItemsHandler);

		/// <exclude/>
		public delegate void ImportDoneEventHandler(ImporDoneEventArguments args, IPXProcess importProcess);

		#endregion

		#region CSVImporter

		/// <exclude/>
		public sealed class CSVImporter<TBatchTable> : PXImporter<TBatchTable>
			where TBatchTable : class, IBqlTable, new()
		{
			private static int[] _codePages;
			private static string[] _codePagesNames;

			static CSVImporter()
			{
				InitCodePagesInfo();
			}

			public CSVImporter(PXCache itemsCache, string viewName, bool rollbackPreviousImport)
				: base(itemsCache, viewName, typeof(CSVSettings), typeof(CSVSettings.viewName), ImportCSVSettingsName, rollbackPreviousImport)
			{
				itemsCache.Graph.FieldSelecting.AddHandler<CSVSettings.codePage>(GetCodePages);
			}

			private static void GetCodePages(PXCache sender, PXFieldSelectingEventArgs args)
			{
				args.ReturnState = PXIntState.CreateInstance(args.ReturnState, "CodePage", null,
					null, null, null, _codePages, _codePagesNames, typeof(Int32), Encoding.ASCII.CodePage);
			}

			private static void InitCodePagesInfo()
			{
				var codePages = new List<int>();
				var codePagesNames = new List<string>();
				var infoList = new List<EncodingInfo>(Encoding.GetEncodings());
				infoList.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, true));
				foreach (var info in infoList)
				{
					codePages.Add(info.CodePage);
					codePagesNames.Add(info.DisplayName);
				}
				_codePages = codePages.ToArray();
				_codePagesNames = codePagesNames.ToArray();
			}

			protected override IContentReader GetReader(byte[] content)
			{
				var commonSettings = (CSVSettings)ImportSettingsCurrent;
				return new CSVReader(content, commonSettings.CodePage ?? Encoding.ASCII.CodePage)
				{
					Separator = commonSettings.Separator
				};
			}

			protected override PXImportSettings CreateDefaultCommonSettings()
			{
				return new CSVSettings()
				{
					Separator = ",",
					CodePage = Encoding.ASCII.CodePage
				};
			}
		}

		#endregion

		#region XLSXImporter

		/// <exclude/>
		public sealed class XLSXImporter<TBatchTable> : PXImporter<TBatchTable>
			where TBatchTable : class, IBqlTable, new()
		{
			public XLSXImporter(PXCache itemsCache, string viewName, bool rollbackPreviousImport)
				: base(itemsCache, viewName, typeof(XLSXSettings), typeof(XLSXSettings.viewName), ImportXLSXSettingsName, rollbackPreviousImport)
			{
			}

			protected override IContentReader GetReader(byte[] content)
			{
				return new XLSXReader(content);
			}

			protected override PXImportSettings CreateDefaultCommonSettings()
			{
				return new XLSXSettings()
				{
					Culture = CultureInfo.CurrentCulture.LCID
				};
			}
		}

		#endregion

		#region PXImporter

		/// <exclude/>
		public abstract class PXImporter<TBatchTable> : IPXImportWizard
			where TBatchTable : class, IBqlTable, new()
		{
			#region StateInfo

			/// <exclude/>
			protected class StateInfo
			{
				private readonly PXFieldState _state;

				private string _inputeMask;

				private Dictionary<string, object> _labelValuePairs;

				public StateInfo(PXFieldState state)
				{
					if (state == null) throw new ArgumentNullException("state");

					_state = state;
				}

				public string Language
				{
					get
					{
						if (_state is PXStringState)
						{
							return ((PXStringState)_state).Language;
						}
						return null;
					}
				}

				public string InputMask
				{
					get
					{
						if (_inputeMask == null && _state is PXStringState)
						{
							_inputeMask = ((PXStringState)_state).InputMask;
							var segmentState = _state as PXSegmentedState;
							if (segmentState != null && !string.IsNullOrEmpty(segmentState.Wildcard))
							{
								var arr = _inputeMask.Split('|');
								if (arr.Length < 2)
								{
									_inputeMask += "||";
								}
								else if (arr.Length < 3)
								{
									_inputeMask += "|";
								}
								_inputeMask += segmentState.Wildcard;
							}
						}
						return _inputeMask;
					}
				}

				public Type DataType
				{
					get { return _state.DataType; }
				}

				public string ViewName
				{
					get { return _state.ViewName; }
				}

				public Dictionary<string, object> LabelValuePairs
				{
					get
					{
						if (_labelValuePairs == null)
						{
							_labelValuePairs = new Dictionary<string, object>();
							var strState = _state as PXStringState;
							if (strState != null && strState.AllowedValues != null)
							{
								for (int i = 0; i < strState.AllowedValues.Length; i++)
								{
									var label = strState.AllowedLabels[i];
									var val = strState.AllowedValues[i];
									_labelValuePairs[label] = val;
								}
							}
							var intState = _state as PXIntState;
							if (intState != null && intState.AllowedValues != null)
							{
								for (int i = 0; i < intState.AllowedValues.Length; i++)
								{
									var label = intState.AllowedLabels[i];
									var val = intState.AllowedValues[i];
									_labelValuePairs[label] = val;
								}
							}
						}
						return _labelValuePairs;
					}
				}
			}

			#endregion

			#region PXCachedView
			/// <exclude/>
			private class PXCachedView : PXView
			{
				public PXCachedView(PXCache cache)
					: base(cache.Graph, true,
					BqlCommand.CreateInstance(typeof(Select<>), typeof(TBatchTable)),
					new PXSelectDelegate(delegate () { return cache.Cached; }))
				{
				}

				public bool Contains(IDictionary keys)
				{
					foreach (TBatchTable table in SelectMulti())
					{
						bool equalKeys = true;
						foreach (string key in _Cache.Keys)
						{
							object keyValue = keys[key];
							_Cache.RaiseFieldUpdating(key, null, ref keyValue);
							if (keyValue == null || !_Cache.GetValueExt(table, key).Equals(keyValue))
							{
								equalKeys = false;
								break;
							}
						}
						if (equalKeys) return true;
					}
					return false;
				}
			}
			#endregion

			#region Constants
			private const string _CSV_EXT_NAME = "csv";
			private const string _XLSX_EXT_NAME = "xlsx";

			private const string _ERRORS_MESSAGE = "Import errors";
			#endregion

			#region Fields
			private static readonly bool _isCRSelectable;
			private readonly bool _rollbackPreviousOperation;
			private readonly PXCache _cache;
			private readonly PXCache _backupCache;
			private readonly PXCache _importedCache;
			private readonly PXView _importCommonSettings;
			private readonly PXView _importColumnsSettings;
			private string[] _propertiesNames;
			private string[] _propertiesDisplayNames;
			private ICollection<string> _lineProperties;
			private Dictionary<string, int> _languages = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			private ICollection<string> _shortLineProperties;

			private readonly IDictionary<Type, IList<Exception>> _exceptions;
			private readonly PXCachedView _cachedItems;
			private readonly string _viewName;

			private static int[] _cultures;
			private static string[] _culturesNames;

			public event EventHandler<MappingPropertiesInitEventArgs> MappingPropertiesInit;
			public event EventHandler<CommonSettingsDialogShowingEventArgs> CommonSettingsDialogShowing;
			public event EventHandler<MappingDialogShowingEventArgs> MappingDialogShowing;
			public event EventHandler<FileUploadingEventArgs> FileUploading;
			public event EventHandler<RowImportingEventArgs> RowImporting;

			#endregion

			#region Ctors
			static PXImporter()
			{
				_isCRSelectable = typeof(IPXSelectable).IsAssignableFrom(typeof(TBatchTable));
				InitCulturesInfo();
			}

			protected PXImporter(PXCache itemsCache, string viewName, Type commonSettingsType, Type commonSettingsViewNameField,
								 string commonSettingsViewName, bool rollbackPreviousImport)
			{
				AssertType<TBatchTable>(itemsCache);
				AssertType<PXImportSettings>(commonSettingsType);

				_rollbackPreviousOperation = rollbackPreviousImport;

				_exceptions = new Dictionary<Type, IList<Exception>>();
				_cache = itemsCache;
				_viewName = viewName;
				_cachedItems = new PXCachedView(itemsCache);
				if (_rollbackPreviousOperation)
				{
					_backupCache = new PXCache<TBatchTable>(itemsCache.Graph);
					_importedCache = new PXCache<TBatchTable>(itemsCache.Graph);
				}
				_importCommonSettings = AddView(itemsCache.Graph, viewName + commonSettingsViewName,
					commonSettingsType,
					BqlCommand.Compose(typeof(Where<,>), commonSettingsViewNameField,
						typeof(Equal<>), typeof(Required<>), commonSettingsViewNameField),
					null);
				_importColumnsSettings = AddView(itemsCache.Graph, viewName + ImportColumnsSettingsName,
						typeof(PXImportColumnsSettings),
						typeof(Where<PXImportColumnsSettings.viewName, Equal<Required<PXImportColumnsSettings.viewName>>>),
						typeof(OrderBy<Asc<PXImportColumnsSettings.index>>));

				_cache.Graph.FieldSelecting.AddHandler<PXImportColumnsSettings.propertyName>(GetTBatchTableProperties);
				_cache.Graph.FieldUpdating.AddHandler<PXImportColumnsSettings.propertyName>(CorrectColumnAssociations);

				_cache.Graph.FieldSelecting.AddHandler(commonSettingsType, "Culture", GetCultures);
			}

			private static void GetCultures(PXCache sender, PXFieldSelectingEventArgs args)
			{
				args.ReturnState = PXIntState.CreateInstance(args.ReturnState, "Culture", null,
					null, null, null, _cultures, _culturesNames, typeof(Int32), Encoding.ASCII.CodePage);
			}

			private static void InitCulturesInfo()
			{
				var culturesList = new List<int>();
				var culturesNamesList = new List<string>();
				var infoList = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.AllCultures));
				infoList.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, true));
				foreach (var info in infoList)
				{
					if (info.LCID == 0x1000) continue; // LOCALE_CUSTOM_UNSPECIFIED
					culturesList.Add(info.LCID);
					culturesNamesList.Add(info.DisplayName);
				}
				_cultures = culturesList.ToArray();
				_culturesNames = culturesNamesList.ToArray();
			}

			private void InitPropertiesInfo()
			{
				if (_propertiesNames == null || _propertiesDisplayNames == null ||
					_lineProperties == null || _shortLineProperties == null)
				{
					ForceInitPropertiesInfo(_cache);
				}
			}

			private void ForceInitPropertiesInfo(PXCache cache)
			{
				var sortedProperties = new List<KeyValuePair<string, string>>();
				_lineProperties = new List<string>(cache.Fields.Count);
				_shortLineProperties = new List<string>(cache.Fields.Count);
				foreach (string field in cache.Fields)
				{
					var state = cache.GetStateExt(null, field) as PXFieldState;
					if (state != null)
					{
						if (state.Visible && state.Enabled && !state.IsReadOnly && state.Visibility != PXUIVisibility.Invisible)
						{
							PXStringState sstate = state as PXStringState;
							string[] languages;
							if (sstate != null && !String.IsNullOrEmpty(sstate.Language)
								&& (languages = _cache.GetValueExt(null, field + "Translations") as string[]) != null)
							{
								int i = 0;
								foreach (string lang in languages)
								{
									sortedProperties.Add(new KeyValuePair<string, string>(field + " " + lang.ToUpper(), state.DisplayName + " " + lang.ToUpper()));
									_languages[lang.ToUpper()] = i;
									i++;
								}
							}
							else
							{
								sortedProperties.Add(new KeyValuePair<string, string>(field, state.DisplayName));
							}
						}
						foreach (var att in cache.GetAttributes(field))
							if (att is PXLineNbrAttribute || att is PXLineNbrMarkerAttribute)
							{
								switch (Type.GetTypeCode(state.DataType))
								{
									case TypeCode.Int32:
										_lineProperties.Add(field);
										break;
									case TypeCode.Int16:
										_shortLineProperties.Add(field);
										break;
								}
								break;
							}
					}
				}
				sortedProperties.Sort((x, y) => string.Compare(x.Value, y.Value, true));

				_propertiesNames = new string[sortedProperties.Count + 1];
				_propertiesDisplayNames = new string[sortedProperties.Count + 1];

				_propertiesNames[0] = string.Empty;
				_propertiesDisplayNames[0] = string.Empty;
				for (int i = 1; i <= sortedProperties.Count; i++)
				{
					var pair = sortedProperties[i - 1];
					_propertiesNames[i] = pair.Key;
					_propertiesDisplayNames[i] = pair.Value;
				}

				MappingPropertiesInitEventArgs e = new MappingPropertiesInitEventArgs(new List<string>(_propertiesNames), new List<string>(_propertiesDisplayNames));
				OnMappingPropertiesInit(e);
				_propertiesNames = e.Names.ToArray();
				_propertiesDisplayNames = e.DisplayNames.ToArray();
			}

			#endregion

			#region Public Methods

			public static IPXImportWizard Create(string fileExt, PXCache itemsCache, string viewName, bool rollbackPreviousImport)
			{
				switch (fileExt)
				{
					case _CSV_EXT_NAME:
						return new CSVImporter<TBatchTable>(itemsCache, viewName, rollbackPreviousImport);
					case _XLSX_EXT_NAME:
						return new XLSXImporter<TBatchTable>(itemsCache, viewName, rollbackPreviousImport);
				}
				return null;
			}

			public bool RollbackPreviousOperation
			{
				get { return _rollbackPreviousOperation; }
			}

			public bool TryUploadData(byte[] content, string ext)
			{
				if (_cache == null || content == null || string.IsNullOrEmpty(ext)) return false;

				FileUploadingEventArgs e = new FileUploadingEventArgs(content, ext);
				OnFileUploading(e);

				if (!e.Cancel)
				{
					SetContent(e.Data, e.FileExtension);

					return true;
				}

				return false;
			}

			public void RunWizard()
			{
				if (AskCommonSettings()) AskColumnAssociations();
			}

			public virtual void PreRunWizard()
			{
				_cachedItems.Cache.Graph.Views[_viewName].SelectMulti();
			}

			public event CreateImportRowEventHandler OnCreateImportRow;

			public event RowImportingEventHandler OnRowImporting;

			public event RowImportedEventHandler OnRowImported;

			public event ImportDoneEventHandler OnImportDone;

			public IEnumerable<KeyValuePair<Type, IList<Exception>>> Exceptions
			{
				get { return _exceptions; }
			}

			#endregion

			#region Protected Methods

			protected virtual void OnMappingPropertiesInit(MappingPropertiesInitEventArgs e)
			{
				if (MappingPropertiesInit != null)
				{
					MappingPropertiesInit(this, e);
				}
			}

			protected virtual void OnCommonSettingsDialogShowing(CommonSettingsDialogShowingEventArgs e)
			{
				if (CommonSettingsDialogShowing != null)
				{
					CommonSettingsDialogShowing(this, e);
				}
			}

			protected virtual void OnMappingDialogShowing(MappingDialogShowingEventArgs e)
			{
				if (MappingDialogShowing != null)
				{
					MappingDialogShowing(this, e);
				}
			}

			protected virtual void OnFileUploading(FileUploadingEventArgs e)
			{
				if (FileUploading != null)
				{
					FileUploading(this, e);
				}
			}

			protected virtual void OnBeforeRowImporting(RowImportingEventArgs e)
			{
				if (RowImporting != null)
				{
					RowImporting(this, e);
				}
			}

			#region SaftyPerformOperation
			/// <exclude/>
			protected delegate void Operation();

			protected bool SaftyPerformOperation(Operation handler)
			{
				if (handler == null) throw new ArgumentNullException("handler");
				bool result = true;
				try
				{
					handler();
				}
				catch (OutOfMemoryException) { throw; }
				catch (StackOverflowException) { throw; }
				catch (Exception e)
				{
					AddException(e);
					result = false;
				}
				return result;
			}
			#endregion

			protected abstract IContentReader GetReader(byte[] content);

			protected virtual void SetValue(TBatchTable result, string fieldName, string value)
			{
				var stringState = _cache.GetStateExt(result, fieldName) as PXStringState;
				if (stringState != null && !string.IsNullOrEmpty(stringState.InputMask))
					value = PX.Common.Mask.Parse(stringState.InputMask, value);

				SaftyPerformOperation(() => _cache.SetValueExt(result, fieldName, value));
			}

			protected void AddException(Exception e)
			{
				Type eType = e.GetType();
				if (!_exceptions.ContainsKey(eType)) _exceptions.Add(eType, new List<Exception>());
				_exceptions[eType].Add(e);
			}

			protected PXImportSettings ImportSettingsCurrent
			{
				get
				{
					var currentSettings = _importCommonSettings.SelectSingle(_viewName);
					if (currentSettings != null) return (PXImportSettings)currentSettings;
					SetDefaultCommonSettings();
					return (PXImportSettings)_importCommonSettings.SelectSingle(_viewName);
				}
			}
			#endregion

			#region Private Methods

			private IEnumerable<KeyValuePair<IDictionary, IDictionary>> ReadItems(IContentReader reader, PXCache cache, System.Globalization.CultureInfo culture)
			{
				reader.Reset();
				var statesCache = new Dictionary<string, StateInfo>();
				int index = 1;
				while (reader.MoveNext())
				{
					KeyValuePair<IDictionary, IDictionary> item;
					if (GetKeysAndValues(reader, index, culture, cache, statesCache, out item))
					{
						yield return item;
						index++;
					}
				}
			}

			private bool GetKeysAndValues(IContentReader contentReader, int lineNumber, System.Globalization.CultureInfo culture, PXCache cache,
				IDictionary<string, StateInfo> statesCache, out KeyValuePair<IDictionary, IDictionary> result)
			{
				result = new KeyValuePair<IDictionary, IDictionary>(
					new OrderedDictionary(StringComparer.OrdinalIgnoreCase),
					new OrderedDictionary(StringComparer.OrdinalIgnoreCase));

				var importSettingsCurrent = ImportSettingsCurrent;
				var nullValueStr = importSettingsCurrent == null ? null : importSettingsCurrent.NullValue;
				var lineFields = new List<string>(_lineProperties);
				var shortLineFields = new List<string>(_shortLineProperties);
				var hasAnyData = false;
				foreach (PXImportColumnsSettings property in _importColumnsSettings.SelectMulti(_viewName))
				{
					if (string.IsNullOrEmpty(property.PropertyName)) continue;
					string contentValue = contentReader.GetValue((int)property.ColumnIndex);
					if (string.Compare(contentValue, nullValueStr, false) == 0) contentValue = null;
					if (contentValue != null)
					{
						hasAnyData |= contentValue.Trim() != string.Empty;
						StateInfo stateInfo;
						string realPropertyName = property.PropertyName;
						if (!_cachedItems.Cache.Fields.Contains(realPropertyName)
							&& _languages.Count > 0 && realPropertyName.Length > 3
							&& realPropertyName[realPropertyName.Length - 3] == ' '
							&& _languages.ContainsKey(realPropertyName.Substring(realPropertyName.Length - 2)))
						{
							realPropertyName = realPropertyName.Substring(0, realPropertyName.Length - 3);
							if (!_cachedItems.Cache.Fields.Contains(realPropertyName))
							{
								realPropertyName = property.PropertyName;
							}
						}
						if (!statesCache.TryGetValue(realPropertyName, out stateInfo))
						{
							var state = _cachedItems.Cache.GetStateExt(null, realPropertyName) as PXFieldState;
							if (state != null) stateInfo = new StateInfo(state);
							statesCache.Add(realPropertyName, stateInfo);
						}
						if (stateInfo != null)
						{
							if (string.IsNullOrEmpty(contentValue) && !string.IsNullOrEmpty(stateInfo.ViewName))
							{
								contentValue = null;
							}
							else
							{
								if (!string.IsNullOrEmpty(stateInfo.InputMask))
								{
									contentValue = Mask.Parse(stateInfo.InputMask, contentValue);
								}
								object dropDownValue;
								if (stateInfo.LabelValuePairs != null && stateInfo.LabelValuePairs.TryGetValue(contentValue, out dropDownValue))
								{
									contentValue = dropDownValue != null ? dropDownValue.ToString() : null;
								}
								else if (culture != null && stateInfo.DataType != null)
									contentValue = ParseValue(contentValue, stateInfo.DataType, culture);
							}
						}
						if (string.IsNullOrEmpty(contentValue)) contentValue = null;
						if (cache.Keys.Contains(realPropertyName))
						{
							if (result.Key.Contains(realPropertyName))
								result.Key[realPropertyName] = contentValue;
							else result.Key.Add(realPropertyName, contentValue);
						}
						if (realPropertyName != property.PropertyName)
						{
							realPropertyName += "Translations";
							string[] alternatives = null;
							if (result.Value.Contains(realPropertyName))
							{
								alternatives = result.Value[realPropertyName] as string[];
							}
							if (alternatives == null)
							{
								alternatives = new string[_languages.Count];
							}
							string lang = property.PropertyName.Substring(property.PropertyName.Length - 2);
							int idx;
							if (_languages.TryGetValue(lang, out idx))
							{
								alternatives[idx] = contentValue;
							}
							result.Value[realPropertyName] = alternatives;
						}
						else if (result.Value.Contains(realPropertyName))
						{
							result.Value[realPropertyName] = contentValue;
						}
						else
						{
							result.Value.Add(realPropertyName, contentValue);
						}
					}
					lineFields.Remove(property.PropertyName);
					shortLineFields.Remove(property.PropertyName);
				}
				if (!hasAnyData) return false;
				foreach (var field in lineFields)
					if (cache.Keys.Contains(field)) result.Key.Add(field, lineNumber);
					else result.Value.Add(field, lineNumber);
				var shortLineNumber = (short)lineNumber;
				foreach (var field in shortLineFields)
					if (cache.Keys.Contains(field)) result.Key.Add(field, shortLineNumber);
					else result.Value.Add(field, shortLineNumber);

				if (cache.Keys.Count > result.Key.Count)
				{
					var defaultRow = cache.CreateInstance();
					foreach (string key in cache.Keys)
						if (!result.Key.Contains(key))
						{
							object defaultValue;
							if (!cache.RaiseFieldDefaulting(key, defaultRow, out defaultValue))
							{
								cache.RaiseFieldSelecting(key, defaultRow, ref defaultValue, false);
								defaultValue = PXFieldState.UnwrapValue(defaultValue);
							}

							result.Key.Add(key, defaultValue);
						}
				}

				result.Value.Add(PXImportAttribute.ImportFlag, PXCache.NotSetValue);
				return true;
			}

			// ReSharper disable AssignmentInConditionalExpression
			private static string ParseValue(string contentValue, Type targetType, IFormatProvider formatProvider)
			{
				// Mimic the behavior from Convert class, but without throwing unneeded exceptions in corner cases
				object parsedValue = null;
				bool success = false;

				try
				{
					switch (Type.GetTypeCode(targetType))
					{
						case TypeCode.Boolean:
							if (contentValue == null)
								parsedValue = false;
							if (success = Boolean.TryParse(contentValue, out bool boolResult))
								parsedValue = boolResult;
							break;
						case TypeCode.Byte:
							if (contentValue == null)
								parsedValue = default(byte);
							if (success = Byte.TryParse(contentValue, NumberStyles.Integer, formatProvider, out byte byteResult))
								parsedValue = byteResult;
							break;
						case TypeCode.DateTime:
							if (contentValue == null)
								parsedValue = default(DateTime);
							if (success = DateTime.TryParse(contentValue, formatProvider, DateTimeStyles.None, out DateTime dateTimeResult))
								parsedValue = dateTimeResult;
							break;
						case TypeCode.Decimal:
							if (contentValue == null)
								parsedValue = Decimal.Zero;
							if (success = Decimal.TryParse(contentValue, NumberStyles.Number, formatProvider, out decimal decimalResult))
								parsedValue = decimalResult;
							break;
						case TypeCode.Double:
							if (contentValue == null)
								parsedValue = 0.0;
							if (success = Double.TryParse(contentValue, NumberStyles.Float | NumberStyles.AllowThousands, formatProvider, out double doubleResult))
								parsedValue = doubleResult;
							break;
						case TypeCode.Int16:
							if (contentValue == null)
								parsedValue = default(short);
							if (success = Int16.TryParse(contentValue, NumberStyles.Integer, formatProvider, out short shortResult))
								parsedValue = shortResult;
							break;
						case TypeCode.Int32:
							if (contentValue == null)
								parsedValue = default(int);
							if (success = Int32.TryParse(contentValue, NumberStyles.Integer, formatProvider, out int intResult))
								parsedValue = intResult;
							break;
						case TypeCode.Int64:
							if (contentValue == null)
								parsedValue = default(long);
							if (success = Int64.TryParse(contentValue, NumberStyles.Integer, formatProvider, out long longResult))
								parsedValue = longResult;
							break;
						case TypeCode.SByte:
							if (contentValue == null)
								parsedValue = default(sbyte);
							if (success = SByte.TryParse(contentValue, NumberStyles.Integer, formatProvider, out sbyte sbyteResult))
								parsedValue = sbyteResult;
							break;
						case TypeCode.Single:
							if (contentValue == null)
								parsedValue = 0.0f;
							if (success = Single.TryParse(contentValue, NumberStyles.Float | NumberStyles.AllowThousands, formatProvider, out float floatResult))
								parsedValue = floatResult;
							break;
						case TypeCode.UInt16:
							if (contentValue == null)
								parsedValue = default(ushort);
							if (success = UInt16.TryParse(contentValue, NumberStyles.Integer, formatProvider, out ushort ushortResult))
								parsedValue = ushortResult;
							break;
						case TypeCode.UInt32:
							if (contentValue == null)
								parsedValue = default(uint);
							if (success = UInt32.TryParse(contentValue, NumberStyles.Integer, formatProvider, out uint uintResult))
								parsedValue = uintResult;
							break;
						case TypeCode.UInt64:
							if (contentValue == null)
								parsedValue = default(ulong);
							if (success = UInt64.TryParse(contentValue, NumberStyles.Integer, formatProvider, out ulong ulongResult))
								parsedValue = ulongResult;
							break;
					}
				}
				catch (FormatException) { }
				catch (OverflowException) { }

				if (success && parsedValue != null)
					return parsedValue.ToString();
				return contentValue;
			}
			// ReSharper restore AssignmentInConditionalExpression

			private void GetTBatchTableProperties(PXCache sender, PXFieldSelectingEventArgs args)
			{
				var row = args.Row as PXImportColumnsSettings;
				if (row == null || string.Compare(row.ViewName, _viewName, true) != 0) return;

				InitPropertiesInfo();
				args.ReturnState = PXStringState.CreateInstance(args.ReturnState, null, null, "PropertyName", null,
								-1, null, _propertiesNames, _propertiesDisplayNames, true, null);
			}

			private void CorrectColumnAssociations(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				var row = e.Row as PXImportColumnsSettings;
				if (row == null || string.Compare(row.ViewName, _viewName, true) != 0) return;

				//TODO: set only unique property-column pairs
				var newValue = e.NewValue == null ? string.Empty : e.NewValue.ToString();
				foreach (PXImportColumnsSettings item in sender.Cached)
					if (!item.Index.Equals(row.Index) &&
						string.Compare(item.PropertyName, newValue, true) == 0)
					{
						e.Cancel = true;
						break;
					}
			}

			private void SetDefaultColumnsAssociations(byte[] content)
			{
				var reader = GetReader(content);
				reader.Reset();
				if (!reader.MoveNext()) throw new Exception(PX.TM.Messages.DataAreAbsent);

				ForceInitPropertiesInfo(_cache);

				foreach (PXImportColumnsSettings cachedItem in _importColumnsSettings.SelectMulti(_viewName))
					_importColumnsSettings.Cache.Delete(cachedItem);

				foreach (var key in reader.IndexKeyPairs)
				{
					var item = new PXImportColumnsSettings
					{
						ViewName = _viewName,
						ColumnIndex = key.Key,
						ColumnName = key.Value
					};
					int nameIndex = Array.IndexOf<string>(_propertiesDisplayNames, key.Value);
					if (nameIndex < 0) nameIndex = Array.IndexOf<string>(_propertiesNames, key.Value);
					if (nameIndex < 0 && _languages.Count > 0)
					{
						nameIndex = Array.IndexOf<string>(_propertiesNames, key.Value + " " + _languages.Keys.First());
					}
					if (nameIndex > -1) item.PropertyName = _propertiesNames[nameIndex];
					_importColumnsSettings.Cache.Insert(item);
				}
				_importColumnsSettings.Cache.IsDirty = false;
			}

			//private static void ForceCacheClear(PXCache cache)
			//{
			//    var deleteItems = new List<object>();
			//    foreach (var item in cache.Inserted)
			//        deleteItems.Add(item);
			//    foreach (var item in cache.Updated)
			//        deleteItems.Add(item);
			//    foreach (var item in deleteItems)
			//        cache.Delete(item);
			//}

			private void SetDefaultCommonSettings()
			{
				foreach (var cachedItem in _importCommonSettings.SelectMulti(_viewName))
					_importCommonSettings.Cache.Delete(cachedItem);

				var importSettings = CreateDefaultCommonSettings();
				importSettings.ViewName = _viewName;
				importSettings.Culture = Thread.CurrentThread.CurrentCulture.LCID;
				importSettings.Mode = ImportMode.UPDATE_EXISTING;
				_importCommonSettings.Cache.Insert(importSettings);
				_importCommonSettings.Cache.IsDirty = false;
			}

			private void SetContent(byte[] content, string ext)
			{
				var importSettings = ImportSettingsCurrent;
				importSettings.FileExtension = ext;
				importSettings.Data = content;
			}

			protected abstract PXImportSettings CreateDefaultCommonSettings();

			private void RemoveLineNbrFields(IDictionary fields)
			{
				List<object> keysToDelete = fields
											.Cast<DictionaryEntry>()
											.Where(entry => _lineProperties.Contains(entry.Key) || _shortLineProperties.Contains(entry.Key))
											.Select(entry => entry.Key)
											.ToList();
				foreach (object key in keysToDelete)
				{
					fields.Remove(key);
				}
			}

			private void ConvertData(byte[] content, PXCache cache, System.Globalization.CultureInfo culture, string mode)
			{
				_exceptions.Clear();

				ImportMode.Value importMode = ImportMode.Parse(mode);

				if (RollbackPreviousOperation)
				{
					foreach (TBatchTable item in _backupCache.Cached)
						cache.Update(item);
					_backupCache.Clear();
					foreach (TBatchTable item in _importedCache.Cached)
						cache.Delete(item);
					_importedCache.Clear();
				}

				ForceInitPropertiesInfo(cache);

				var prepareItemsHandler = cache.Graph as IPXPrepareItems;
				var importProcess = cache.Graph as IPXProcess;
				if (cache.Graph.Extensions != null)
				{
					for (int i = 0; i < cache.Graph.Extensions.Length; i++)
					{
						if (cache.Graph.Extensions[i] is IPXPrepareItems)
						{
							prepareItemsHandler = cache.Graph.Extensions[i] as IPXPrepareItems;
						}
					}
					for (int i = 0; i < cache.Graph.Extensions.Length; i++)
					{
						if (cache.Graph.Extensions[i] is IPXProcess)
						{
							importProcess = cache.Graph.Extensions[i] as IPXProcess;
						}
					}
				}

				using (var reader = GetReader(content))
					foreach (var item in ReadItems(reader, cache, culture))
					{
						var keys = item.Key;
						var values = item.Value;

						if (importMode == ImportMode.Value.InsertAllRecords)
						{
							RemoveLineNbrFields(keys);
							RemoveLineNbrFields(values);
						}

						if (_isCRSelectable && !values.Contains("Selected")) values.Add("Selected", true);

						RowImportingEventArgs e = new RowImportingEventArgs(keys, values, importMode);
						OnBeforeRowImporting(e);
						if (e.Cancel)
						{
							continue;
						}

						if (OnCreateImportRow != null && prepareItemsHandler != null)
						{
							var args = new CreateImportRowEventArguments(keys, values, ImportSettingsCurrent.Mode);
							OnCreateImportRow(args, prepareItemsHandler);
							if (args.Cancel) continue;
						}
						var oldCurrent = cache.Current;
						var originalRow = cache.Locate(keys) > 0 ? cache.Current : null;
						var wasInserted = originalRow == null;

						if (importMode == ImportMode.Value.BypassExisting && !wasInserted)
						{
							continue;
						}

						bool onlyInsert = importMode == ImportMode.Value.BypassExisting || importMode == ImportMode.Value.InsertAllRecords;
						if (onlyInsert)
						{
							if (OnRowImporting != null && prepareItemsHandler != null)
							{
								var args = new RowImportingEventArguments(originalRow);
								OnRowImporting(args, prepareItemsHandler);
								if (args.Cancel) continue;
							}
							if (originalRow != null) originalRow = cache.CreateCopy(originalRow);
						}
						if (!SaftyPerformOperation(() => cache.Update(keys, values)))
						{
							cache.Current = oldCurrent;
							continue;
						}
						var impRow = cache.Current;
						if (onlyInsert)
						{
							var impArgs = new RowImportedEventArguments(impRow, originalRow);
							if (OnRowImported != null && prepareItemsHandler != null)
								OnRowImported(impArgs, prepareItemsHandler);
							else impArgs.Cancel = !wasInserted;
							if (impArgs.Cancel)
							{
								cache.Remove(cache.Current);
								cache.Current = oldCurrent;
							}
						}
						if (RollbackPreviousOperation)
						{
							if (wasInserted) InsertHeldItem(_importedCache, keys);
							else InsertHeldItem(_backupCache, keys);
						}
					}
				if (OnImportDone != null && importProcess != null)
					OnImportDone(new ImporDoneEventArguments(importMode), importProcess);

			}

			private static void InsertHeldItem(PXCache cache, IDictionary keys)
			{
				object keysObj = cache.CreateInstance();
				foreach (DictionaryEntry key in keys)
					cache.SetValueExt(keysObj, key.Key.ToString(), key.Value);

				object locatedObj = cache.Locate(keysObj);
				cache.SetStatus(locatedObj, PXEntryStatus.Held);

				cache.Insert(locatedObj);
			}

			private bool AskCommonSettings()
			{
				var commonSettings = ImportSettingsCurrent;
				if (commonSettings == null) return true;

				CommonSettingsDialogShowingEventArgs e = new CommonSettingsDialogShowingEventArgs(null);

				if (_importCommonSettings.Answer == WebDialogResult.None)
				{
					byte[] content = commonSettings.Data as byte[];
					string ext = commonSettings.FileExtension;

					SetDefaultCommonSettings();
					SetContent(content, ext);

					e.Settings = ImportSettingsCurrent;
					OnCommonSettingsDialogShowing(e);
				}

				bool askIsOk = e.Cancel
					? true :
					_importCommonSettings.AskExt() == WebDialogResult.OK;

				return askIsOk;
			}

			private void AskColumnAssociations()
			{
				MappingDialogShowingEventArgs e = new MappingDialogShowingEventArgs(null);

				if (_importColumnsSettings.Answer == WebDialogResult.None)
				{
					SetDefaultColumnsAssociations(ImportSettingsCurrent.Data as byte[]);

					List<PXImportColumnsSettings> defaultSettings = _importColumnsSettings.SelectMulti(_viewName)
																	.Select(s => (PXImportColumnsSettings)s)
																	.ToList();

					e.Settings = defaultSettings;
					OnMappingDialogShowing(e);

					foreach (PXImportColumnsSettings cachedItem in _importColumnsSettings.SelectMulti(_viewName))
					{
						_importColumnsSettings.Cache.Delete(cachedItem);
					}
					foreach (PXImportColumnsSettings newItem in e.Settings)
					{
						_importColumnsSettings.Cache.Insert(newItem);
					}
					_importColumnsSettings.Cache.IsDirty = false;
				}

				bool askIsOk = e.Cancel
					? true
					: _importColumnsSettings.AskExt() == WebDialogResult.OK;

				if (askIsOk)
				{
					var commonSettings = ImportSettingsCurrent;
					var content = commonSettings.Data as byte[];
					if (content != null && content.Length > 0)
					{
						System.Globalization.CultureInfo culture = null;
						if (commonSettings.Culture != null) culture = CultureInfo.GetCultureInfo((int)commonSettings.Culture);

						var originalGraph = _cache.Graph;
						//prepare all caches in original graph
						foreach (Type t in originalGraph.Views.RestorableCaches.ToArray())
						{
							PXCache c = originalGraph.Caches[t];
						}

						//create import graph. basically import graph is copy of original graph.
						//we use preservescope to allow load of necessary cache in PXImportAttibute.AddView
						//that's the reason why later we are forced to clear import graph caches
						originalGraph.Unload();
						PXGraph importGraph;
						using (new PXPreserveScope())
						{
							importGraph = PXGraph.CreateInstance(_cache.Graph.GetType());
							importGraph.Load();
						}

						//copy all items from original graph to import graph
						foreach (var src in originalGraph.Caches.Caches)
						{
							var dst = importGraph.Caches[src.GetItemType()];
							dst.Clear();
							foreach (var row in src.Cached)
							{
								object copy = src.CreateCopy(row); //create item copy, place into import cache
								dst.SetStatus(copy, src.GetStatus(row));
								if (object.ReferenceEquals(row, src._Current))
								{
									dst._Current = copy;
								}
							}
							dst.IsDirty = src.IsDirty;
						}

						MoveEventsToImportGraph(importGraph);
						//at this point importGraph should be complete copy of originalGraph with same cache/item structure.
						//TODO: remove all references to original graph and cache in longrun, control lambda capture.
						PXLongOperation.StartOperation(originalGraph, () =>
							{
								var threadCul = Thread.CurrentThread.CurrentCulture;
								var threadUICul = Thread.CurrentThread.CurrentUICulture;
								var graphCul = _cache.Graph.Culture;
								Thread.CurrentThread.CurrentCulture = _cache.Graph.Culture = importGraph.Culture = culture ?? CultureInfo.InvariantCulture;
								Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

								PXDatabase.ReadBranchRestricted = false; // PXLongOperation.StartOperation applies PXReadBranchRestrictedScope to the delegate (see c23c39eae9f17707678354039c23e10d908b7be3)
								PXContext.SetSlot<PX.Translation.PXDictionaryManager>(null); //NOTE: because of localization initialization bug
								importGraph.IsImportFromExcel = true;

								ConvertData(content, importGraph.Caches[_cache.GetItemType()], culture, commonSettings.Mode);

								originalGraph._UpdatePrimaryView(commonSettings.ViewName);
								Thread.CurrentThread.CurrentCulture = threadCul;
								Thread.CurrentThread.CurrentUICulture = threadUICul;
								originalGraph.Culture = graphCul;

								PXLongOperation.SetCustomInfo(importGraph, new object[0]);
								ShowExceptions();
							});
					}

				}

			}

			private void MoveEventsToImportGraph(PXGraph importGraph)
			{
				PXImporter<TBatchTable> importer = null;
				//find requested select in graph or its extensions
				foreach (var graph in Enumerable.Union<object>(
					new object[] { importGraph },
					importGraph.Extensions ?? Enumerable.Empty<object>()))
				{
					FieldInfo field = graph.GetType().GetField(this._viewName);
					if (field == null) continue;
					var sb = field.GetValue(graph) as PXSelectBase;
					var importAttr = sb.GetAttribute<PXImportAttribute>();
					if (importAttr != null)
					{
						importer = (PXImporter<TBatchTable>)importAttr._importer;
						break;
					}
				}

				if (importer != null)
				{
					if (RowImporting != null)
					{
						foreach (Delegate d in RowImporting.GetInvocationList())
							RowImporting -= (EventHandler<RowImportingEventArgs>)d;
						foreach (Delegate d in importer.RowImporting.GetInvocationList())
							RowImporting += (EventHandler<RowImportingEventArgs>)d;
					}
					if (FileUploading != null)
					{
						foreach (Delegate d in FileUploading.GetInvocationList())
							FileUploading -= (EventHandler<FileUploadingEventArgs>)d;
						foreach (Delegate d in importer.FileUploading.GetInvocationList())
							FileUploading += (EventHandler<FileUploadingEventArgs>)d;
					}

					if (MappingDialogShowing != null)
					{
						foreach (Delegate d in MappingDialogShowing.GetInvocationList())
							MappingDialogShowing -= (EventHandler<MappingDialogShowingEventArgs>)d;
						foreach (Delegate d in importer.MappingDialogShowing.GetInvocationList())
							MappingDialogShowing += (EventHandler<MappingDialogShowingEventArgs>)d;
					}
					if (CommonSettingsDialogShowing != null)
					{
						foreach (Delegate d in CommonSettingsDialogShowing.GetInvocationList())
							CommonSettingsDialogShowing -= (EventHandler<CommonSettingsDialogShowingEventArgs>)d;
						foreach (Delegate d in importer.CommonSettingsDialogShowing.GetInvocationList())
							CommonSettingsDialogShowing += (EventHandler<CommonSettingsDialogShowingEventArgs>)d;
					}
					if (MappingPropertiesInit != null)
					{
						foreach (Delegate d in MappingPropertiesInit.GetInvocationList())
							MappingPropertiesInit -= (EventHandler<MappingPropertiesInitEventArgs>)d;
						foreach (Delegate d in importer.MappingPropertiesInit.GetInvocationList())
							MappingPropertiesInit += (EventHandler<MappingPropertiesInitEventArgs>)d;
					}
				}
			}

			private void ShowExceptions()
			{
				StringBuilder sb = new StringBuilder();
				int limit = 10; //TODO: need scroll in dialog box.
				foreach (KeyValuePair<Type, IList<Exception>> pair in Exceptions)
					foreach (Exception exception in pair.Value)
					{
						sb.AppendLine(exception.Message);
						if (--limit < 1) break;
					}
				string message = sb.ToString();
				if (!string.IsNullOrEmpty(message))
					throw new PXDialogRequiredException(_viewName, null, _ERRORS_MESSAGE, message,
						MessageButtons.RetryCancel, MessageIcon.Error);
			}

			private static void AssertType<ItemType>(PXCache itemsCache)
			{
				Type itemsCacheType = itemsCache.GetItemType();
				AssertType<ItemType>(itemsCacheType);
			}

			private static void AssertType<ItemType>(Type cacheItemType)
			{
				if (!typeof(ItemType).IsAssignableFrom(cacheItemType))
					throw new ArgumentException(string.Format("The items type '{0}' of cache must be an inheritor of the '{1}' type.",
															  cacheItemType, typeof(ItemType)));
			}

			#endregion
		}

		#endregion

		#region PXImportException

		/// <exclude/>
		public sealed class PXImportException : Exception
		{
			public readonly KeyValuePair<IDictionary, IDictionary> Row;

			public PXImportException(string message, KeyValuePair<IDictionary, IDictionary> row, Exception innerException)
				: base(message, innerException)
			{
				Row = row;
			}

			public PXImportException(SerializationInfo info, StreamingContext context)
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

		#endregion

		#region IPXPrepareItems
		/// <summary>Defines methods that can be implemented by the graph to control the data import.</summary>
		public interface IPXPrepareItems
		{
			/// <summary>Prepares a record from the imported file for conversion into a DAC instance.</summary>
			/// <param name="viewName">The name of the view that is marked with the attribute.</param>
			/// <param name="keys">The keys of the data to import.</param>
			/// <param name="values">The values corresponding to the keys.</param>
			bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values);
			/// <summary>Implements the logic executed before the insertion of a data record into the cache.</summary>
			/// <param name="viewName">The name of the view that is marked with the attribute.</param>
			/// <param name="row">The record to import as a DAC instance.</param>
			bool RowImporting(string viewName, object row);
			/// <summary>Implements the logic executed after the insertion of a data record into the cache.</summary>
			/// <param name="viewName">The name of the view that is marked with the attribute.</param>
			/// <param name="row">The imported record as a DAC instance.</param>
			bool RowImported(string viewName, object row, object oldRow);
			/// <summary>Verifies the imported items before they are saved in the database.</summary>
			/// <param name="viewName">The name of the view that is marked with the attribute.</param>
			/// <param name="items">The collection of objects to import as instances of the DAC.</param>
			void PrepareItems(string viewName, IEnumerable items);
		}
		#endregion

		#region IPXProcess
		/// <exclude/>
		public interface IPXProcess
		{
			void ImportDone(ImportMode.Value mode);
		}
		#endregion

		#region Constants
		private const string _IMPORT_MESSAGE = "Import";

		/// <exclude/>
		public const string _RUNWIZARD_ACTION_NAME = "$ImportWizardAction";
		/// <exclude/>
		public const string _IMPORT_ACTION_NAME = "$ImportAction";

		/// <exclude/>
		public const string ImportCSVSettingsName = "$ImportCSVSettings";
		/// <exclude/>
		public const string ImportXLSXSettingsName = "$ImportXLSXSettings";
		/// <exclude/>
		public const string ImportColumnsSettingsName = "$ImportColumns";
		/// <exclude/>
		public const string ImportContentBagName = "$ImportContentBag";

		/// <exclude/>
		public const string _DONT_UPDATE_EXIST_RECORDS = "_DONT_UPDATE_EXIST_RECORDS";
		#endregion

		#region Fields
		private readonly Type _table;

		private IPXImportWizard _importer;
		private PXCache _itemsCache;
		private string _itemsViewName;

		/// <exclude/>
		public static readonly string ImportFlag = Guid.NewGuid().ToString();

		#endregion

		#region Ctors
		/// <summary>Initializes a new instance of the attribute. The primary view of the graph will be detected based on PXGraph.PrimaryView.</summary>
		public PXImportAttribute()
		{
			_table = null;
		}

		/// <summary>Initializes a new instance of the attribute. The parameter sets the primary view DAC.</summary>
		/// <param name="primaryTable">The first DAC that is referenced by the primary view of the graph where the current view is declared.</param>
		public PXImportAttribute(Type primaryTable)
		{
			_table = primaryTable;
		}

		/// <summary>Initializes a new instance of the attribute. The first
		/// parameter is the primary table of the view the attribute is attached
		/// to.</summary>
		/// <param name="primaryTable">The first table that is referenced in the
		/// view (primary table).</param>
		/// <param name="importer">An object that implements the <tt>IPXImportWizard</tt> interface.</param>
		public PXImportAttribute(Type primaryTable, IPXImportWizard importer)
			: this(primaryTable)
		{
			_importer = importer;
		}

		/// <exclude/>
		public override void ViewCreated(PXGraph graph, string viewName)
		{
			var primaryView = _table ?? graph.Views[graph.PrimaryView].GetItemType();

			PXView view = graph.Views[viewName];
			//view.BqlTarget
			_itemsCache = graph.Views[viewName].Cache;
			_itemsViewName = viewName;

			PXSelectBase select = graph.Views.GetExternalMember(view);
			select.Attributes.Add(this);

			var importActionName = GetImportActionName(viewName);
			AddAction(importActionName, primaryView, graph, new PXButtonDelegate(Import));

			var runWizardActionName = GetRunWizardActionName(viewName);
			AddAction(runWizardActionName, primaryView, graph, new PXButtonDelegate(ImportWizard));

			var importContentBag = AddView(graph, _itemsViewName + ImportContentBagName, typeof(PXContentBag));
			if (_importer == null)
			{
				var current = importContentBag.Cache.Current as PXContentBag;
				var createParams = new object[]
									 {
										 current == null ? null : current.FileExtension,
										 _itemsCache,
										 _itemsViewName,
										 RollbackPreviousImport
									 };
				var importerType = MakeGenericType(typeof(PXImporter<>), _itemsCache.GetItemType());
				_importer = importerType.GetMethod("Create").Invoke(null, createParams) as IPXImportWizard;
			}
			if (_importer != null)
			{
				TryUploadData(_importer, importContentBag);
				if (_itemsCache.Graph is IPXPrepareItems
					|| _itemsCache.Graph.Extensions != null
					&& _itemsCache.Graph.Extensions.Any(_ => _ is IPXPrepareItems))
				{
					_importer.OnCreateImportRow +=
						(args, prepareItemsHandler) =>
							{
								using (PXExecutionContext.Scope.Instantiate(new PXExecutionContext()))
								{
									PXExecutionContext.Current.Bag[_DONT_UPDATE_EXIST_RECORDS] = args.Mode == ImportMode.Value.BypassExisting || args.Mode == ImportMode.Value.InsertAllRecords;
									args.Cancel = !_importer_OnImportRow(args.Keys, args.Values, prepareItemsHandler);
								}
							};
					_importer.OnRowImporting +=
						(args, prepareItemsHandler) =>
							{
								args.Cancel = !_importer_OnRowImporting(args.Row, prepareItemsHandler);
							};
					_importer.OnRowImported +=
						(args, prepareItemsHandler) =>
							{
								args.Cancel = !_importer_OnRowImported(args.Row, args.OldRow, prepareItemsHandler);
							};
				}
				if (_itemsCache.Graph is IPXProcess
					|| _itemsCache.Graph.Extensions != null
					&& _itemsCache.Graph.Extensions.Any(_ => _ is IPXProcess))
				{
					_importer.OnImportDone +=
						(args, importProcess) =>
						{
							_importer_OnImportDone(args.Mode, importProcess);
						};
				}
				graph.RowUpdated.AddHandler<PXContentBag>(
					(sender, args) =>
					{
						if (!TryUploadData(_importer, importContentBag))
							throw new PXDialogRequiredException(_itemsViewName, null, Messages.ValidationFailedHeader,
																Messages.FileContainsIncorrectData,
																MessageButtons.OK, MessageIcon.Error);
					});
			}
		}
		#endregion

		#region Public Methods

		/// <summary>Enables or disables the control that the attribute adds to the table.</summary>
		/// <param name="graph">The graph where the view marked with the attribute
		/// is defined.</param>
		/// <param name="viewName">The name of the view that is marked with the
		/// attribute.</param>
		/// <param name="isEnabled">The value that indicates whether the method
		/// enables or disables the control.</param>
		public static void SetEnabled(PXGraph graph, string viewName, bool isEnabled)
		{
			if (graph == null)
			{
				throw new PXArgumentException(nameof(graph));
			}

			if (string.IsNullOrEmpty(viewName))
			{
				throw new PXArgumentException(nameof(viewName));
			}

			var runWizardActionName = GetRunWizardActionName(viewName);
			var runWizardAction = graph.Actions[runWizardActionName];

			if (runWizardAction != null)
			{
				runWizardAction.SetEnabled(isEnabled);
			}

			var importActionName = GetImportActionName(viewName);
			var importAction = graph.Actions[importActionName];

			if (importAction != null)
			{
				importAction.SetEnabled(isEnabled);
			}
		}

		public static bool GetEnabled(PXGraph graph, string viewName)
		{
			if (graph == null)
			{
				throw new PXArgumentException(nameof(graph));
			}

			if (string.IsNullOrEmpty(viewName))
			{
				throw new PXArgumentException(nameof(viewName));
			}

			var actionName = GetImportActionName(viewName);
			var action = graph.Actions[actionName];

			if (action == null)
			{
				return false;
			}

			return action.GetEnabled();
		}

		private static string GetImportActionName(string viewName)
		{
			return viewName + _IMPORT_ACTION_NAME;
		}

		private static string GetRunWizardActionName(string viewName)
		{
			return viewName + _RUNWIZARD_ACTION_NAME;
		}

		/// <exclude/>
		public bool RollbackPreviousImport { get; set; }

		#endregion

		#region Protected Methods
		protected static PXAction AddAction(string name, string viewName, Type table, PXGraph graph, PXButtonDelegate handler)
		{
			var actionName = viewName + name;

			return AddAction(actionName, table, graph, handler);
		}


		protected static PXAction AddAction(string actionName, Type table, PXGraph graph, PXButtonDelegate handler)
		{
			var action = (PXAction)Activator.CreateInstance(
				MakeGenericType(typeof(PXNamedAction<>), table), graph, actionName, handler);
			action.SetVisible(false);
			graph.Actions[actionName] = action;
			return action;
		}

		protected static PXView AddView(PXGraph graph, string viewName, Type itemType)
		{
			return AddView(graph, viewName, itemType, null, null);
		}

		protected static PXView AddView(PXGraph graph, string viewName, Type itemType, Type whereType, Type orderType)
		{
			if (graph.Views.ContainsKey(viewName)) return graph.Views[viewName];

			var itemCacheType = MakeGenericType(typeof(PXNotCleanableCache<>), itemType);
			if (!graph.Caches.ContainsKey(itemType) ||
				!itemCacheType.IsAssignableFrom(graph.Caches[itemType].GetType()))
			{
				var newCache = (PXCache)Activator.CreateInstance(itemCacheType, graph);
				newCache.Load();
				graph.Caches[itemType] = newCache;
				graph.SynchronizeByItemType(newCache);
			}

			var handler = new PXSelectInsertedHandler();
			var command = BqlCommand.CreateInstance(typeof(Select<>), itemType);
			if (whereType != null) command = command.WhereNew(whereType);
			if (orderType != null) command = command.OrderByNew(orderType);

			handler.View = new PXView(graph, false, command, new PXSelectDelegate(handler.Select));
			graph.Views.Add(viewName, handler.View);
			graph.RowUpdated.AddHandler(itemType, new PXRowUpdated(ClearDirtyOnRowUpdated));
			graph.RowInserted.AddHandler(itemType, new PXRowInserted(ClearDirtyOnRowInserted));
			return handler.View;
		}

		/// <exclude/>
		private class PXSelectInsertedHandler
		{
			public PXView View;

			public IEnumerable Select()
			{
				View.Cache.IsDirty = false;
				return View.Cache.Inserted;
			}

		}

		/// <exclude/>
		private sealed class viewErrorInterceptor : PXView
		{
			private PXView _View;

			private viewErrorInterceptor(PXGraph graph, bool isReadOnly, BqlCommand select)
				: base(graph, isReadOnly, select)
			{
			}

			private viewErrorInterceptor(PXGraph graph, bool isReadOnly, BqlCommand select, Delegate handler)
				: base(graph, isReadOnly, select, handler)
			{
			}

			public static viewErrorInterceptor FromView(PXView view)
			{
				viewErrorInterceptor instance = view.BqlDelegate == null ?
					new viewErrorInterceptor(view.Graph, view.IsReadOnly, view.BqlSelect) :
					new viewErrorInterceptor(view.Graph, view.IsReadOnly, view.BqlSelect, view.BqlDelegate);
				instance._View = view;
				return instance;
			}

			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				if (maximumRows == 0)
				{
					return _View.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
				}
				int start = 0;
				int maximum = 0;
				List<object> sel = _View.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref start, maximum, ref totalRows);
				List<object> ret = new List<object>();
				for (int i = 0; i < sel.Count;)
				{
					object item = sel[i];
					if (item is PXResult)
					{
						item = ((PXResult)item)[0];
					}
					PXEntryStatus status = _View.Cache.GetStatus(item);
					if (status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated)
					{
						Dictionary<string, string> errors = PXUIFieldAttribute.GetErrors(_View.Cache, item);
						if (errors.Count > 0)
						{
							ret.Add(sel[i]);
							sel.RemoveAt(i);
						}
						else
						{
							i++;
						}
					}
					else
					{
						i++;
					}
				}
				if (startRow < 0)
				{
					startRow = 0;
				}
				for (int i = startRow; i < sel.Count && ret.Count <= maximumRows; i++)
				{
					ret.Add(sel[i]);
				}
				return ret;
			}
		}
		#endregion

		#region Private Methods
		private static bool TryUploadData(IPXImportWizard importer, PXView contentBag)
		{
			var current = contentBag.Cache.Current as PXContentBag;
			return current != null && current.Loaded.HasValue && current.Loaded.Value
				   && importer.TryUploadData((byte[])current.Data, current.FileExtension);
		}

		private static void ClearDirtyOnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.IsDirty = false;
		}

		private static void ClearDirtyOnRowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.IsDirty = false;
		}

		private bool _importer_OnImportRow(IDictionary keys, IDictionary values, IPXPrepareItems prepareItemsHandler)
		{
			return prepareItemsHandler.PrepareImportRow(_itemsViewName, keys, values);
		}

		private bool _importer_OnRowImporting(object row, IPXPrepareItems prepareItemsHandler)
		{
			return prepareItemsHandler.RowImporting(_itemsViewName, row);
		}

		private bool _importer_OnRowImported(object row, object oldRow, IPXPrepareItems prepareItemsHandler)
		{
			return prepareItemsHandler.RowImported(_itemsViewName, row, oldRow);
		}

		private void _importer_OnImportDone(ImportMode.Value mode, IPXProcess importProcess)
		{
			importProcess.ImportDone(mode);
		}

		[PXUIField(Visible = false)]
		[PXButton(CommitChanges = true)]
		private IEnumerable ImportWizard(PXAdapter adapter)
		{
			if (_importer != null)
			{
				_importer.PreRunWizard();
				_importer.RunWizard();
				_itemsCache.Graph.Views[_itemsViewName].RequestRefresh();
				adapter.View.Graph.Views[_itemsViewName] = viewErrorInterceptor.FromView(adapter.View.Graph.Views[_itemsViewName]);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = _IMPORT_MESSAGE, Enabled = true)]
		[PXButton(ImageKey = Sprite.Main.Process, CommitChanges = true)]
		private IEnumerable Import(PXAdapter adapter)
		{
			IPXPrepareItems prepare = _itemsCache.Graph as IPXPrepareItems;
			if (_itemsCache.Graph.Extensions != null)
			{
				for (int i = 0; i < _itemsCache.Graph.Extensions.Length; i++)
				{
					if (_itemsCache.Graph.Extensions[i] is IPXPrepareItems)
					{
						prepare = _itemsCache.Graph.Extensions[i] as IPXPrepareItems;
					}
				}
			}
			if (prepare != null)
				prepare.PrepareItems(_itemsViewName, GetImportedItems());
			SafetyPersistInserted(_itemsCache, GetImportedItems());
			return adapter.Get();
		}

		private IEnumerable GetImportedItems()
		{
			foreach (object item in _itemsCache.Inserted)
			{
				var selectable = item as IPXSelectable;
				if ((selectable == null || selectable.Selected == true)/* &&
					_itemsCache.GetValuePending(item, PXImportAttribute.ImportFlag) != null*/)
				{
					yield return item;
				}
			}
		}

		private void SafetyPersistInserted(PXCache cache, IEnumerable items)
		{
			bool isAborted = false;
			PXTransactionScope tscope = null;
			var persistedItems = new List<object>();
			try
			{
				tscope = new PXTransactionScope();
				foreach (var item in items)
				{
					cache.PersistInserted(item);
					persistedItems.Add(item);
				}
				tscope.Complete(cache.Graph);
			}
			catch (Exception)
			{
				isAborted = true;
				throw;
			}
			finally
			{
				if (tscope != null) tscope.Dispose();
				cache.Normalize();
				cache.Persisted(isAborted);
				cache.Graph.Views[_itemsViewName].Clear();
			}
		}

		private static Type MakeGenericType(params Type[] types)
		{
			int index = 0;
			return MakeGenericType(types, ref index);
		}

		private static Type MakeGenericType(Type[] types, ref int index)
		{
			if (types == null) throw new ArgumentNullException("types");
			if (types.Length == 0) throw new ArgumentException("The types list is empty.");
			if (index >= types.Length) throw new ArgumentOutOfRangeException("types", "The types list is not correct.");

			Type ret = types[index];
			index++;
			if (!ret.IsGenericTypeDefinition) return ret;
			Type[] args = new Type[ret.GetGenericArguments().Length];
			for (int i = 0; i < args.Length; i++)
				args[i] = MakeGenericType(types, ref index);
			return ret.MakeGenericType(args);
		}
		#endregion
	}
	#endregion

	#region PXLineNbrAttribute
	/// <summary>Automatically generates unique line numbers that identify for
	/// child data records in the parent-child relationship. This attribute
	/// does not work without the <see cref="PXParentAttribute">PXParent</see>
	/// attribute.</summary>
	/// <remarks>
	/// 	<para>The attribute should be placed on the child DAC field that
	/// stores the line number. The line number is a two-byte integer
	/// incremented by the <tt>IncrementStep</tt> property value, which equals
	/// 1 by default. The line number uniquely identifies a data record among
	/// the child data records related to a given parent data record. The
	/// attribute calculates each next value by incrementing the current
	/// number of the child data records.</para>
	/// 	<para>The child DAC field to store the line number typically has the
	/// <tt>short?</tt> data type. It also should be a key field. You indicate
	/// that the field is a key field by setting the <tt>IsKey</tt> property
	/// of the data type attribute to <tt>true</tt>.</para>
	/// 	<para>As a parameter, you should pass either the parent DAC field that
	/// stores the number of related child data records or the parent DAC
	/// itself. In the latter case, the attribute will determine the number of
	/// related child data records by itself. If the parent DAC field is
	/// specified, the attribute automatically updates its value.</para>
	/// </remarks>
	/// <example>
	/// 	<code title="Example" description="The attribute below takes the number of related child data records from the provided parent field. The PXParent attribute must be added to some other field of this DAC." lang="CS">
	/// [PXDBShort(IsKey = true)]
	/// [PXLineNbr(typeof(ARRegister.lineCntr))]
	/// public virtual short? LineNbr { get; set; }</code>
	/// 	<code title="Example2" description="In the following example, the attribute calculates the number of related child data records by itself." groupname="Example" lang="CS">
	/// [PXDBShort(IsKey = true)]
	/// [PXLineNbr(typeof(Vendor))]
	/// [PXParent(typeof(
	///     Select&lt;Vendor,
	///         Where&lt;Vendor.bAccountID, Equal&lt;Current&lt;TaxReportLine.vendorID&gt;&gt;&gt;&gt;))]
	/// public virtual short? LineNbr { get; set; }</code>
	/// </example>
	public class PXLineNbrAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXRowDeletedSubscriber, IPXRowInsertedSubscriber, IPXRowPersistedSubscriber 
	{
		private Type _dataType;
		private readonly Type _sourceType;
		private readonly string _sourceField;
		private readonly bool _enabled;
		
		/// <exclude/>
		protected class LastDefault
		{
			protected object _Value;
			public List<object> Rows = new List<object>();
			private readonly SortedSet<object> Gaps = new SortedSet<Object>();
			private readonly PXLineNbrAttribute _owner;

			public LastDefault(PXLineNbrAttribute owner)
			{
				_owner = owner;
				Initialize();
			}

			public void Clear()
			{
				Rows.Clear();
				Initialize();
			}

			public void Initialize()
			{
				_Value = _owner.DefaultValue;
			}

			public void StoreGap(object value)
			{
				if (value != null)
					Gaps.Add(value);
			}

			public object GetGap()
			{
				if (Gaps.Any())
				{
					Object min = Gaps.Min;
					Gaps.Remove(min);
					return min;
				}
				return null;
			}

			private bool _gapsRestored;
			public void RestoreGaps(PXCache cache, object source = null)
			{
				if (_gapsRestored) return;

				PXCache sourceCache = cache.Graph.Caches[_owner._sourceType];
				source = source ?? sourceCache.Current;
				if (source != null)
				{
					var lineNbrs = PXParentAttribute
						.SelectChildren(cache, source, _owner._sourceType)
						.Select(t => cache.GetValue(t, _owner._FieldOrdinal))
						.Union(_owner._sourceField.With(fn => sourceCache.GetValue(source, fn).With(v => new [] {v})) ?? Enumerable.Empty<object>())
						.WhereNotNull()
						.OrderBy(t => t)
						.ToArray();
					object current = _owner.Increment(_owner.DefaultValue, 1);
					foreach (object ln in lineNbrs)
					{
						while (((IComparable)ln).CompareTo(current) > 0)
						{
							StoreGap(current);
							current = _owner.Increment(current, _owner.IncrementStep);
						}
						current = _owner.Increment(current, _owner.IncrementStep);
					}
				}
				_gapsRestored = true;
			}

			public object Value
			{
				get { return _Value; }
				set { _Value = value; }
			}

			public static LastDefault operator ++(LastDefault value)
			{
				value._Value = value._owner.Increment(value._Value, value._owner.IncrementStep);
				return value;
			}

			public static LastDefault operator --(LastDefault value)
			{
				value._Value = value._owner.Decrement(value._Value, value._owner.IncrementStep);
				return value;
			}

			public static explicit operator int (LastDefault value)
			{
				return (int)Convert.ChangeType(value.Value, typeof(int));
			}
		}
		protected LastDefault _LastDefaultValue;


		/// <summary>
		/// Initializes a new instance of the attribute. As a parameter you can provide
		/// the parent data record field that stores the number of child data records or
		/// the parent DAC if there is no such field. In the latter case the attribute
		/// will calculate the number of child data records automatically.
		/// </summary>
		/// <param name="sourceType">The parent data record field that stores the number
		/// of children or the parent DAC.</param>
		/// <param name="Enabled">Allows to make field editable.</param>
		/// <example>
		/// In the following example, the attribute calculates the number of related child
		/// data records by itself.
		/// <code>
		/// [PXDBShort(IsKey = true)]
		/// [PXLineNbr(typeof(Vendor))]
		/// [PXParent(typeof(
		///     Select&lt;Vendor,
		///         Where&lt;Vendor.bAccountID, Equal&lt;Current&lt;TaxReportLine.vendorID&gt;&gt;&gt;&gt;))]
		/// public virtual short? LineNbr { get; set; }
		/// </code>
		/// </example>
		public PXLineNbrAttribute(Type sourceType) : this(sourceType, false) { }

		/// <summary>
		/// Initializes a new instance of the attribute. As a parameter you can provide
		/// the parent data record field that stores the number of child data records or
		/// the parent DAC if there is no such field. In the latter case the attribute
		/// will calculate the number of child data records automatically.
		/// </summary>
		/// <param name="sourceType">The parent data record field that stores the number
		/// of children or the parent DAC.</param>
		/// <param name="Enabled">Allows to make field editable.</param>
		/// <example>
		/// In the following example, the attribute calculates the number of related child
		/// data records by itself.
		/// <code>
		/// [PXDBShort(IsKey = true)]
		/// [PXLineNbr(typeof(Vendor))]
		/// [PXParent(typeof(
		///     Select&lt;Vendor,
		///         Where&lt;Vendor.bAccountID, Equal&lt;Current&lt;TaxReportLine.vendorID&gt;&gt;&gt;&gt;))]
		/// public virtual short? LineNbr { get; set; }
		/// </code>
		/// </example>
		public PXLineNbrAttribute(Type sourceType, bool Enabled)
		{
			_enabled = Enabled;
			if (typeof(IBqlField).IsAssignableFrom(sourceType) && sourceType.IsNested)
			{
				_sourceType = BqlCommand.GetItemType(sourceType);
				_sourceField = sourceType.Name;
			}
			else if (typeof(IBqlTable).IsAssignableFrom(sourceType))
			{
				_sourceType = sourceType;
			}
			else
			{
				throw new PXArgumentException("type", ErrorMessages.CantCreateForeignKeyReference, sourceType);
			}
		}

		/// <summary>Gets or sets the number by which the line number is
		/// incremented or decremented. By default, the property equals
		/// 1.</summary>
		public short IncrementStep { get; set; } = 1;

		/// <summary>Indicates whether the source field would be decremented 
		/// on the row deleting or not. By default, the property equals true.</summary>
		public bool DecrementOnDelete { get; set; } = true;

		/// <summary>
		/// Indicates whether the line number gaps produced by the delete operation should be reused
		/// </summary>
		public bool ReuseGaps { get; set; }

		/// <exclude/>
		public virtual void LineNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnValue, null, null, null, null,
				null, null, null, _FieldName, null, null, null, PXErrorLevel.Undefined, _enabled, null, null,
				PXUIVisibility.Undefined, null, null, null);
			}
		}

		/// <exclude/>
		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_sourceField != null)
			{
				PXCache sourceCache = sender.Graph.Caches[_sourceType];
				if (sourceCache.Current != null)
				{
					object currentSourceVal = sourceCache.GetValue(sourceCache.Current, _sourceField);
					object lineNbr = sender.GetValue(e.Row, _FieldOrdinal);

					if (lineNbr != null && ((IComparable)currentSourceVal).CompareTo(lineNbr) < 0)
					{
						_LastDefaultValue.Clear();

						sourceCache.SetValue(sourceCache.Current, _sourceField, lineNbr);
						sourceCache.MarkUpdated(sourceCache.Current);
					}
				}
			}
		}

		/// <exclude/>
		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_sourceField != null && DecrementOnDelete)
			{
				PXCache sourceCache = sender.Graph.Caches[_sourceType];
				if (sourceCache.Current != null)
				{
					object currentSourceValue = sourceCache.GetValue(sourceCache.Current, _sourceField);
					object lineNbr = sender.GetValue(e.Row, _FieldOrdinal);
					if (lineNbr != null && ((IComparable)currentSourceValue).CompareTo(lineNbr) == 0)
					{
						_LastDefaultValue.Clear();

						sourceCache.SetValue(sourceCache.Current, _sourceField, Decrement(currentSourceValue, 1));
						sourceCache.MarkUpdated(sourceCache.Current);
					}
				}
			}
			if (ReuseGaps && e.Row != null)
				_LastDefaultValue.StoreGap(sender.GetValue(e.Row, _FieldOrdinal));
		}

		/// <exclude/>
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed)
			{
				ClearLastDefaultValue();
			}
		}
		/// <exclude/>
		public virtual void ClearLastDefaultValue()
		{
			_LastDefaultValue.Clear();
		}
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			PXFieldState state = (PXFieldState)sender.GetStateExt(null, _FieldName);
			_dataType = state.DataType;
			_LastDefaultValue = new LastDefault(this);
			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _FieldName, LineNbr_FieldSelecting);
			sender.Graph.OnClear += delegate { ClearLastDefaultValue(); };
		}

		/// <exclude/>
		public object NewLineNbr(PXCache sender, object Current)
		{
			object currentSourceVal = DefaultValue;

			if (Current != null)
			{
				if (ReuseGaps)
				{
					_LastDefaultValue.RestoreGaps(sender, Current);
					object gap = _LastDefaultValue.GetGap();
					if (gap != null)
						return gap;
				}

				if (!string.IsNullOrEmpty(_sourceField))
				{
					PXCache sourceCache = sender.Graph.Caches[_sourceType];
					currentSourceVal = Increment(sourceCache.GetValue(Current, _sourceField), IncrementStep);
					sourceCache.SetValue(Current, _sourceField, currentSourceVal);
					sourceCache.MarkUpdated(Current);
				}
				else
				{
					foreach (object data in PXParentAttribute.SelectChildren(sender, Current, _sourceType))
					{
						var cache = sender;

						if (data.GetType() != sender.GetItemType())
						{
							cache = sender.Graph.Caches[data.GetType()];
						}

						object lastLineNbr = cache.GetValue(data, _FieldOrdinal);

						if (((IComparable)lastLineNbr).CompareTo(currentSourceVal) > 0)
						{
							currentSourceVal = lastLineNbr;
						}
					}
					currentSourceVal = Increment(currentSourceVal, IncrementStep);
				}
			}
			return currentSourceVal;
		}

		/// <summary>Returns the next line number for the provided parent data
		/// record. The returned value should be used as the child identifier
		/// stored in the specified field.</summary>
		/// <param name="cache">The cache object to search for the</param>
		/// <param name="Current">The parent data record for which the next child
		/// identifier (line number) is returned.</param>
		/// <returns>The line number as an object. Cast to <tt>short?</tt>.</returns>
		public static object NewLineNbr<TField>(PXCache cache, object Current)
			where TField : class, IBqlField
		{
			return cache.GetAttributes<TField>().OfType<PXLineNbrAttribute>().FirstOrDefault()?.NewLineNbr(cache, Current);
		}

		/// <exclude/>
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (ReuseGaps) _LastDefaultValue.RestoreGaps(sender);

			if ((int)_LastDefaultValue > 0)
			{
				if (e.Row != null)
				{
					foreach (object row in _LastDefaultValue.Rows)
					{
						if (sender.Locate(row) != null)
						{
							_LastDefaultValue.Rows.Clear();

							object gap = ReuseGaps ? _LastDefaultValue.GetGap() : null;
							if (gap != null)
								_LastDefaultValue.Value = gap;
							else
								_LastDefaultValue++;

							break;
						}
					}
					_LastDefaultValue.Rows.Add(e.Row);
				}
				e.NewValue = _LastDefaultValue.Value;
			}
			else
			{
				object gap = ReuseGaps ? _LastDefaultValue.GetGap() : null;
				if (gap != null)
				{
					_LastDefaultValue.Value = Decrement(gap, 1);
				}
				else
				{
					if (_sourceField != null)
					{
						PXCache sourceCache = sender.Graph.Caches[_sourceType];
						if (sourceCache.Current != null)
						{
							_LastDefaultValue.Value = sourceCache.GetValue(sourceCache.Current, _sourceField);
						}
					}
					else
					{
						bool found = false;

						foreach (object data in sender.Inserted)
						{
							var lastLineNbr = sender.GetValue(data, _FieldOrdinal) as IComparable;
							if (lastLineNbr.CompareTo(_LastDefaultValue.Value) > 0)
							{
								_LastDefaultValue.Value = lastLineNbr;
								found = true;
							}
						}

						//we need to check sender.Updated to examine items that were first deleted then inserted
						foreach (object data in sender.Updated)
						{
							var lastLineNbr = sender.GetValue(data, _FieldOrdinal) as IComparable;
							if (lastLineNbr.CompareTo(_LastDefaultValue.Value) > 0)
								_LastDefaultValue.Value = lastLineNbr;
						}

						if (!found)
						{
							foreach (object data in PXParentAttribute.SelectSiblings(sender, e.Row, _sourceType))
							{
								var cache = sender;
								if (data.GetType() != sender.GetItemType())
								{
									cache = sender.Graph.Caches[data.GetType()];
								}

								object lastLineNbr = cache.GetValue(data, _FieldOrdinal);
								if (((IComparable)lastLineNbr).CompareTo(_LastDefaultValue.Value) > 0)
								{
									_LastDefaultValue.Value = lastLineNbr;
								}
							}
						}
					} 
				}
				
				_LastDefaultValue++;
				e.NewValue = _LastDefaultValue.Value;
				
				if (e.Row != null)
				{
					_LastDefaultValue.Rows.Add(e.Row);
				}
			}
		}

		public object DefaultValue
		{
			get
			{
				if (_dataType == typeof(Int32))
				{
					return (Int32)0;
				}
				else if (_dataType == typeof(Int16))
				{
					return (Int16)0;
				}
				else if (_dataType == typeof(UInt16))
				{
					return (UInt16)0;
				}
				return (long)0;
			}
		}

		protected object Increment(object value, short step)
		{
			if (_dataType == typeof(Int32))
			{
				return (Int32)((Int32)value + step);
			}
			else if (_dataType == typeof(Int16))
			{
				return (Int16)((Int16)value + step);
			}
			else if (_dataType == typeof(UInt16))
			{
				return (UInt16)((UInt16)value + step);
			}
			return (long)((long)value + step);
		}

		protected object Decrement(object value, short step)
		{
			if (_dataType == typeof(Int32))
			{
				return (Int32)((Int32)value - step);
			}
			else if (_dataType == typeof(Int16))
			{
				return (Int16)((Int16)value - step);
			}
			else if (_dataType == typeof(UInt16))
			{
				return (UInt16)((UInt16)value - step);
			}
			return (long)((long)value - step);
		}
	}

	#endregion

	#region PXTimeZoneAttribute

	/// <exclude/>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class PXTimeZoneAttribute : PXStringListAttribute
	{
		private static readonly string[] _values;
		private static readonly string[] _labels;
		/// 
		public override bool IsLocalizable { get { return false; } }

		static PXTimeZoneAttribute()
		{
			var values = new List<string>();
			var labels = new List<string>();
			var zoneCollection = new List<PXTimeZoneInfo>(PXTimeZoneInfo.GetSystemTimeZones());
			zoneCollection.Sort((x, y) =>
			{
				var diff = x.BaseUtcOffset.CompareTo(y.BaseUtcOffset);
				if (diff == 0) diff = x.DisplayName.CompareTo(y.DisplayName);
				return diff;
			});
			foreach (var zone in zoneCollection)
			{
				values.Add(zone.Id);
				labels.Add(zone.DisplayName);
			}
			_values = values.ToArray();
			_labels = labels.ToArray();
		}

		public PXTimeZoneAttribute(bool allowEmpty = true)
			: base(allowEmpty ? _values.Prepend(String.Empty) : _values, allowEmpty ? _labels.Prepend(String.Empty) : _labels) { }
	}

	#endregion

	#region DB List Attributes

	#region PXDBStringListAttribute

	/// <summary>
	/// Configures a dropdown control for a string field. The values and labels
	/// for the dropdown control are taken from the specified database table.
	/// </summary>
	/// <example>
	/// 	<code title="Example" description="In the example below, the lists of values and labels are taken from the CROpportunityProbability table." lang="CS">
	/// [PXDBString(1, IsUnicode = true)]
	/// [PXUIField(DisplayName = "Probability", Visibility = PXUIVisibility.SelectorVisible)]
	/// [PXDBStringList(
	///     typeof(CROpportunityProbability),
	///     typeof(CROpportunityProbability.code),
	///     typeof(CROpportunityProbability.description),
	///     DefaultValueField = typeof(CROpportunityProbability.isDefault))]
	/// public virtual string Probability { get; set; }</code>
	/// </example>
	public sealed class PXDBStringListAttribute : PXBaseListAttribute
	{
		/// <exclude/>
		private class PXDBStringAttributeHelper : PXDBListAttributeHelper<string>
		{
			public PXDBStringAttributeHelper(Type table, Type valueField, Type descriptionField)
				: base(table, valueField, descriptionField)
			{
			}

			protected override object CreateState(PXCache sender, PXFieldSelectingEventArgs e, string[] values, string[] labels,
												  string fieldName, string defaultValue)
			{
				if (values.Length != labels.Length)
				{
					PXTrace.WriteInformation(string.Format("CRStringAttributeHelper CreateState {0}_{1}: Invalide values and labels",
						sender.GetItemType().Name, fieldName));
					int count = Math.Max(values.Length, labels.Length);
					for (int i = 0; i < count; i++)
					{
						PXTrace.WriteInformation(string.Format("'{0}' -> '{1}'",
															   i < values.Length ? values[i] : "<error>",
															   i < labels.Length ? labels[i] : "<error>"));
					}
				}
				return PXStringState.CreateInstance(e.ReturnState, null, null, fieldName, null,
													-1, null, values, labels, true, defaultValue);
			}

			protected override string EmptyLabelValue
			{
				get
				{
					return string.Empty;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the attribute.
		/// </summary>
		/// <param name="table">The DAC representing the table whose fields
		/// are used as sources of values and labels.</param>
		/// <param name="valueField">The field holding string values.</param>
		/// <param name="descriptionField">The field holding labels.</param>
		public PXDBStringListAttribute(Type table, Type valueField, Type descriptionField)
			: base(new PXDBStringAttributeHelper(table, valueField, descriptionField))
		{
		}
	}

	#endregion

	#region PXDBIntListAttribute

	/// <summary>
	/// Configures a drop-down control for an integer field. The values and labels
	/// for the drop-down control are taken from the specified database table.
	/// </summary>
	/// <example>
	/// <code>
	/// [PXDBInt]
	/// [PXUIField(DisplayName = "Stage", Visibility = PXUIVisibility.SelectorVisible)]
	/// [PXDBIntList(
	///    typeof(CROpportunityStage),
	///    typeof(CROpportunityStage.stageCode),
	///    typeof(CROpportunityStage.description),
	///    DefaultValueField = typeof(CROpportunityStage.isDefault))]
	/// public virtual int? StageID { get; set; }
	/// </code>
	/// </example>
	public sealed class PXDBIntListAttribute : PXBaseListAttribute
	{
		/// <exclude/>
		private class PXDBIntAttributeHelper : PXDBListAttributeHelper<int>
		{
			public PXDBIntAttributeHelper(Type table, Type valueField, Type descriptionField)
				: base(table, valueField, descriptionField)
			{
			}

			protected override object CreateState(PXCache sender, PXFieldSelectingEventArgs e, int[] values, string[] labels,
												  string fieldName, int defaultValue)
			{
				return PXIntState.CreateInstance(e.ReturnState, fieldName, null,
												 -1, null, null, values, labels, null, defaultValue);
			}
		}

		/// <summary>
		/// Initializes a new instance of the attribute.
		/// </summary>
		/// <param name="table">The DAC representing the table whose fields
		/// are used as sources of values and labels.</param>
		/// <param name="valueField">The field holding integer values.</param>
		/// <param name="descriptionField">The field holding labels.</param>
		public PXDBIntListAttribute(Type table, Type valueField, Type descriptionField)
			: base(new PXDBIntAttributeHelper(table, valueField, descriptionField))
		{
		}
	}

	#endregion

	#region IPXDBListAttributeHelper

	/// <exclude/>
	public interface IPXDBListAttributeHelper : ILocalizableValues
	{
		Type DefaultValueField { get; set; }
		String EmptyLabel { get; set; }
		object DefaultValue { get; }
		void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, PXAttributeLevel attributeLevel, string fieldName);
		Dictionary<object, string> ValueLabelDic(PXGraph graph);
	}

	#endregion

	#region PXBaseListAttribute

	/// <exclude/>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	[PXAttributeFamily(typeof(PXBaseListAttribute))]
	public abstract class PXBaseListAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber,
												IPXFieldDefaultingSubscriber, ILocalizableValues
	{
		private readonly IPXDBListAttributeHelper _helper;

		protected PXBaseListAttribute(IPXDBListAttributeHelper helper)
		{
			_helper = helper;
		}

		public Type DefaultValueField
		{
			get { return _helper.DefaultValueField; }
			set { _helper.DefaultValueField = value; }
		}

		public string EmptyLabel
		{
			get { return _helper.EmptyLabel; }
			set { _helper.EmptyLabel = value; }
		}

		#region IPXFieldSelectingSubscriber Members

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			_helper.FieldSelecting(sender, e, _AttributeLevel, base._FieldName);
		}

		#endregion

		public Dictionary<object, string> ValueLabelDic(PXGraph graph)
		{
			return _helper.ValueLabelDic(graph);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_helper.DefaultValueField != null)
			{
				e.NewValue = _helper.DefaultValue;
			}
		}

		#region ILocalizableValues
		public string Key
		{
			get { return _helper.Key; }
		}

		public string[] Values
		{
			get { return _helper.Values; }
		}
		#endregion
	}

	#endregion

	#region PXDBListAttributeHelper<TValue>

	/// <exclude/>
	public abstract class PXDBListAttributeHelper<TValue> : IPXDBListAttributeHelper
	{
		#region ListParameters

		/// <exclude/>
		private struct ListParameters
		{
			public readonly Type DescriptionField;
			public readonly Type ValueField;
			public readonly Type DefaultValueField;
			public readonly Type Table;
			public readonly KeyValuePair<string, TValue> EmptyLabel;

			public ListParameters(Type table, Type descriptionField, Type valueField, Type defaultValueField, KeyValuePair<string, TValue> emptyLabel)
			{
				Table = table;
				DescriptionField = descriptionField;
				ValueField = valueField;
				DefaultValueField = defaultValueField;
				EmptyLabel = emptyLabel;
			}

			public ListParameters(Type table, Type descriptionField, Type valueField, KeyValuePair<string, TValue> emptyLabel)
				: this(table, descriptionField, valueField, null, emptyLabel)
			{
			}

			public ListParameters(Type table, Type descriptionField, Type valueField)
				: this(table, descriptionField, valueField, null, new KeyValuePair<string, TValue>(null, default(TValue)))
			{
			}

			public ListParameters ChangeDefaultValueField(Type defaultValueField)
			{
				return new ListParameters(Table, DescriptionField, ValueField, defaultValueField, EmptyLabel);
			}

			public ListParameters ChangeEmptyLabel(string label, TValue value)
			{
				return new ListParameters(Table, DescriptionField, ValueField, DefaultValueField, new KeyValuePair<string, TValue>(label, value));
			}
		}

		#endregion

		#region ValueLabelPairs

		/// <exclude/>
		private class ValueLabelPairs : IPrefetchable<ListParameters>
		{
			private TValue[] _values;
			private string[] _labels;
			private string _descriptionFieldName;

			private TValue _defaultValue;

			public void Prefetch(ListParameters parameter)
			{
				Type table = parameter.Table;
				Type valueField = parameter.ValueField;
				Type descriptionField = parameter.DescriptionField;
				Type defaultValueField = parameter.DefaultValueField;

				_defaultValue = default(TValue);
				List<TValue> allowedValues = new List<TValue>();
				List<string> allowedLabels = new List<string>();

				if (parameter.EmptyLabel.Key != null)
				{
					allowedValues.Add(parameter.EmptyLabel.Value);
					allowedLabels.Add(parameter.EmptyLabel.Key);
				}

				List<PXDataField> dataFields = new List<PXDataField>(3);
				dataFields.Add(new PXDataField(valueField.Name));
				dataFields.Add(new PXDataField(descriptionField.Name));
				if (defaultValueField != null) dataFields.Add(new PXDataField(defaultValueField.Name));
				foreach (PXDataRecord record in PXDatabase.SelectMulti(table, dataFields.ToArray()))
				{
					TValue currentValue = (TValue)record.GetValue(0);
					allowedValues.Add(currentValue);
					allowedLabels.Add(record.GetString(1));
					if (defaultValueField != null)
					{
						bool? isDefault = record.GetBoolean(2);
						if (isDefault.HasValue && isDefault.Value)
							_defaultValue = currentValue;
					}
				}

				_values = allowedValues.ToArray();
				_labels = allowedLabels.ToArray();
				_descriptionFieldName = descriptionField.Name;
			}

			public TValue DefaultValue
			{
				get { return _defaultValue; }
			}

			public TValue[] Values
			{
				get { return _values; }
			}

			public string[] Labels
			{
				get { return _labels; }
			}

			public string DescriptionFieldName
			{
				get { return _descriptionFieldName; }
			}
		}

		#endregion

		private ListParameters _parameters;
		private readonly string _slotKey;
		private readonly string _locKey;

		protected PXDBListAttributeHelper(Type table, Type valueField, Type descriptionField)
		{
			_parameters = new ListParameters(table, descriptionField, valueField);
			_slotKey = string.Format("_{0}_slotKey", GetType());
			_locKey = table.FullName;
		}

		public Type DefaultValueField
		{
			get { return _parameters.DefaultValueField; }
			set { _parameters = _parameters.ChangeDefaultValueField(value); }
		}

		public string EmptyLabel
		{
			get { return _parameters.EmptyLabel.Key; }
			set { _parameters = _parameters.ChangeEmptyLabel(value, EmptyLabelValue); }
		}

		protected virtual TValue EmptyLabelValue
		{
			get
			{
				return default(TValue);
			}
		}

		public object DefaultValue
		{
			get
			{
				ValueLabelPairs pairs = Data;
				return pairs == null ? default(TValue) : pairs.DefaultValue;
			}
		}

		private ValueLabelPairs Data
		{
			get
			{
				return PXDatabase.GetSlot<ValueLabelPairs, ListParameters>(
					_slotKey, _parameters, _parameters.Table);
			}
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, PXAttributeLevel attributeLevel,
								   string fieldName)
		{
			if (attributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				ValueLabelPairs pairs = Data;
				e.ReturnState = CreateState(sender, e, pairs.Values, Localize(pairs.DescriptionFieldName, pairs.Labels), fieldName, pairs.DefaultValue);
			}
		}

		protected string[] Localize(string descriptionFieldName, string[] labels)
		{
			if (string.IsNullOrEmpty(descriptionFieldName) || labels == null)
				return null;
			string[] trans = new string[labels.Length];
			for (int i = 0; i < labels.Length; i++)
				trans[i] = PXLocalizer.Localize(labels[i], Key);
			return trans;
		}

		protected abstract object CreateState(PXCache sender, PXFieldSelectingEventArgs e, TValue[] values, string[] labels,
											  string fieldName, TValue defaultValue);

		public Dictionary<object, string> ValueLabelDic(PXGraph graph)
		{
			ValueLabelPairs pairs = Data;
			Dictionary<object, string> result = new Dictionary<object, string>(pairs.Values.Length);
			for (int index = 0; index < pairs.Values.Length; index++)
				result.Add(pairs.Values[index], pairs.Labels[index]);
			return result;
		}

		#region ILocalizableValues
		public string Key
		{
			get { return _locKey; }
		}
		public string[] Values
		{
			get { return Localize(Data.DescriptionFieldName, Data.Labels); }
		}
		#endregion
	}

	#endregion

	#endregion

	#region PXPreviewAttribute

	/// <exclude/>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class PXPreviewAttribute : PXViewExtensionAttribute
	{
		private const string _ACTION_POSTFIX = "$RefreshPreview";
		private const string _VIEW_POSTFIX = "$Preview";

		private readonly Type _primaryViewType;
		private Type _previewType;

		private PXGraph _graph;
		private string _viewName;
		private Type _cacheType;
		private PXSelectDelegate _dataHandler;

		private BqlCommand _bqlSelect;

		/// 
		public PXPreviewAttribute(Type primaryViewType) : this(primaryViewType, null) { }

		/// 
		public PXPreviewAttribute(Type primaryViewType, Type previewType)
		{
			if (primaryViewType == null) throw new ArgumentNullException("primaryViewType");
			if (previewType != null && !typeof(IBqlTable).IsAssignableFrom(previewType))
				throw new ArgumentException(string.Format("'{0}' must impement PX.Data.IBqlTable interface.", previewType), "previewType");

			_primaryViewType = primaryViewType;
			_previewType = previewType;
		}

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			_graph = graph;
			_viewName = viewName;
			_cacheType = Graph.Views[ViewName].GetItemType();
			if (PreviewType == null) _previewType = CacheType;

			AddView();
			AddAction();
		}

		private void AddAction()
		{
			var newAction = PXNamedAction.AddAction(Graph, _primaryViewType, ViewName + _ACTION_POSTFIX, null, RefreshPreview);
			newAction.SetVisible(false);			
		}

		private void AddView()
		{
			var select = BqlSelect;
			var newView = new PXView(Graph, false, select, SelectHandler);
			Graph.Views.Add(ViewName + _VIEW_POSTFIX, newView);
		}

		protected virtual BqlCommand BqlSelect
		{
			get
			{
				if (_bqlSelect == null)
					_bqlSelect = (BqlCommand)Activator.CreateInstance(BqlCommand.Compose(typeof(Select<>), PreviewType));
				return _bqlSelect;
			}
		}

		protected virtual PXSelectDelegate SelectHandler
		{
			get
			{
				return () => new[] { Graph.Caches[PreviewType].Current };
			}
		}

		protected Type PreviewType
		{
			get { return _previewType; }
		}

		protected PXGraph Graph
		{
			get { return _graph; }
		}

		protected string ViewName
		{
			get { return _viewName; }
		}

		protected Type CacheType
		{
			get { return _cacheType; }
		}

		[PermissionSet(SecurityAction.Assert, Name = "FullTrust")]
		protected virtual IEnumerable GetPreview()
		{
			if (_dataHandler == null)
			{
				var dataHandler = new PXSelectDelegate(() =>
														{
															var cache = Graph.Caches[PreviewType];
															return new[] { cache.Current ?? cache.CreateInstance() };
														});
				var customHandlerName = PreviewType + "_GetPreview";
				var customHandler = Graph.GetType().GetMethod(customHandlerName,
						BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { CacheType }, null);
				if (customHandler != null && typeof(IEnumerable).IsAssignableFrom(customHandler.ReturnType))
					dataHandler = new PXSelectDelegate(() =>
														{
															var row = Graph.Caches[CacheType].Current;
															return (IEnumerable)customHandler.Invoke(Graph, new[] { row });
														});
				_dataHandler = dataHandler;
			}
			return _dataHandler().Cast<object>().ToList();
		}

		[PXButton]
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select)]
		private IEnumerable RefreshPreview(PXAdapter adapter)
		{
			PerformRefresh();
			return adapter.Get();
		}

		protected virtual void PerformRefresh()
		{
			var previewCache = Graph.Caches[PreviewType];
			foreach (object row in GetPreview())
				previewCache.Current = row;
		}
	}

	#endregion

	#region PXFontListAttribute

	/// <exclude/>
	public sealed class PXFontListAttribute : PXStringListAttribute
	{
		private static readonly string[] _values;
		private static readonly string[] _labels;

		static PXFontListAttribute()
		{
			var fonts = PX.Common.FontFamilyEx.GetFontNames();
			_values = new string[fonts.Length];
			_labels = new string[fonts.Length];
			var i = 0;
			foreach (string font in fonts)
			{
				_values[i] = font;
				_labels[i] = font;
				i++;
			}
		}

		public PXFontListAttribute()
			: base(_values, _labels)
		{

		}
	}

	#endregion

	#region PXFontSizeListAttribute
	/// <exclude/>
	public sealed class PXFontSizeListAttribute : PXIntListAttribute
	{
		private static readonly int[] _values;
		private static readonly string[] _labels;

		static PXFontSizeListAttribute()
		{
			var sizes = PX.Common.FontFamilyEx.DefaultSizes;
			_values = new int[sizes.Length];
			_labels = new string[sizes.Length];
			var i = 0;
			foreach (int font in sizes)
			{
				_values[i] = font;
				_labels[i] = font.ToString();
				i++;
			}
		}

		public PXFontSizeListAttribute()
			: base(_values, _labels)
		{			
		}
	}
	/// <exclude/>
	public sealed class PXFontSizeStrListAttribute : PXIntListAttribute
	{
		public PXFontSizeStrListAttribute()
			: base(PX.Common.FontFamilyEx.FontSizes, PX.Common.FontFamilyEx.FontSizesStr)
		{
		}
	}

	#endregion

	#region TypeDelete
	/// <exclude/>
	public class TypeDeleteAttribute : PXIntListAttribute
	{
		public const int _Any = 0;
		public const int _Failed = 1;
		public const int _Successful = 2;

		public TypeDeleteAttribute()
			: base(
				new[] { _Any, _Failed, _Successful },
				new[] { SM.Messages.AnyT, SM.Messages.FailedT, SM.Messages.SuccessfulT })
		{

		}
	}
	#endregion


	#region PXVirtualAttribute
	/// <summary>
	/// 	<para>You add this attribute to a DAC class definition to prevent specific DAC data records from being saved to the database.</para>
	/// </summary>
	/// <remarks>
	/// 	<para dir="ltr">It is mandatory that you invoke the <tt>PXVirtual</tt> statuc constructor by adding the following line of code into either a BLC constuctor or
	/// the Initialize method within a BLC extension.</para>
	/// 	<blockquote style="MARGIN-RIGHT: 0px" dir="ltr">
	/// 		<para>
	/// 			<tt>typeof(PX.Data.MassProcess.FieldValue).GetCustomAttributes(typeof(PXVirtualAttribute), false);</tt>
	/// 		</para>
	/// 	</blockquote>
	/// 	<para>See examples below.</para>
	/// </remarks>
	/// <example>
	/// 	<code title="Example" description="The example below shows a general PXVirtual attribute declaration." lang="CS">
	/// [PXVirtual]
	/// [PXCacheName(Messages.TimeCardDetail)]
	/// [Serializable]
	/// public partial class EPTimeCardSummary : IBqlTable
	/// { ... }</code>
	/// 	<code title="Example2" description="In the following code you invoke a static constructor for the PXVirtual attribute within a BLC constructor." groupname="Example" lang="CS">
	/// public PXGenericInqGrph()
	/// {
	///     ...
	///     typeof(FieldValue).GetCustomAttributes(typeof(PXVirtualAttribute), false);
	///     ...
	/// }</code>
	/// 	<code title="Example3" description="In the following code you invoke a static constructor for the PXVirtual attribute within a BLC initialize method." groupname="Example2" lang="CS">
	/// public override void Initialize()
	/// {
	///     typeof(FieldValue).GetCustomAttributes(typeof(PXVirtualAttribute), false);
	///     ...
	/// }</code>
	/// </example>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class PXVirtualAttribute : Attribute
	{
		static PXVirtualAttribute()
		{
			PXCacheCollection.OnCacheCreated += CacheAttached;
			PXCacheCollection.OnCacheChanged += CacheAttached;
		}

		private static void CacheAttached(PXGraph graph, PXCache cache)
		{
			if (cache != null && IsDefined(cache.GetItemType(), typeof(PXVirtualAttribute), true))
			{
				new PXVirtualDACAttribute().CacheAttached(graph, cache);
				graph.RowPersisting.AddHandler(cache.GetItemType(), RowPersisting);
			}
		}

		private static void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}
	}

	#endregion

	#region PXBypassAuditAttribute
	/// <exclude/>
	public class PXBypassAuditAttribute : PXEventSubscriberAttribute
	{
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if(!sender.BypassAuditFields.Contains(this.FieldName))
				sender.BypassAuditFields.Add(this.FieldName);
		}
	}
	#endregion


	#region PXDBUserPasswordAttribute
	/// <exclude/>
	public class PXDBUserPasswordAttribute : PXDBCalcedAttribute, IPXFieldUpdatingSubscriber
	{
		public static string DefaultVeil
		{
			get
			{
				return new string('*', 8);
			}
		}

		public PXDBUserPasswordAttribute()
			: base(typeof(Users.password), typeof(string))
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (!sender.BypassAuditFields.Contains(this.FieldName))
				sender.BypassAuditFields.Add(this.FieldName);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null) e.ReturnValue = GetViewString(e.ReturnValue as String);

			base.FieldSelecting(sender, e);
		}
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null && (!string.IsNullOrWhiteSpace(((Users)e.Row).Password)) && (e.NewValue as string) == DefaultVeil)
			{
				e.NewValue = ((Users)e.Row).Password;
				e.Cancel = true;
			}
		}

		private string GetViewString(String str)
		{
			return String.IsNullOrEmpty(str) ? str : DefaultVeil;
		}
	} 
	#endregion


	#region PXRowsListAttribute
	/// <summary>User friendly name of a DAC class.</summary>
	/// <exclude/>
	public class PXPossibleRowsListAttribute : Attribute
	{
		protected BqlCommand _Select;
		protected string _IDFieldName;
		protected string _ValueFieldName;
		public PXPossibleRowsListAttribute(Type select, Type idField, Type valueField)
		{
			if (select == null)
			{
				throw new PXArgumentException(nameof(select), ErrorMessages.ArgumentNullException);
			}
			if (valueField == null)
			{
				throw new PXArgumentException(nameof(valueField), ErrorMessages.ArgumentNullException);
			}
			if (idField == null)
			{
				throw new PXArgumentException(nameof(idField), ErrorMessages.ArgumentNullException);
			}
			_ValueFieldName = char.ToUpper(valueField.Name[0]) + (valueField.Name.Length > 1 ? valueField.Name.Substring(1) : "");
			_IDFieldName = char.ToUpper(idField.Name[0]) + (idField.Name.Length > 1 ? idField.Name.Substring(1) : "");
			if (typeof(IBqlSearch).IsAssignableFrom(select))
			{
				_Select = BqlCommand.CreateInstance(select);
				Type idfield = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
			}
			else if (select.IsNested && typeof(IBqlField).IsAssignableFrom(select))
			{
				_Select = BqlCommand.CreateInstance(typeof(Search<>), select);
			}
			else
			{
				throw new PXArgumentException("type", ErrorMessages.CantCreateForeignKeyReference, select);
			}
		}
		public virtual List<string> GetPossibleRows(PXGraph graph, out string idField, out string valueField)
		{
			idField = _IDFieldName;
			valueField = _ValueFieldName;
			HashSet<string> ret = new HashSet<string>();
			PXView view = new PXView(graph, true, _Select);
			string key = ((IBqlSearch)_Select).GetField().Name;
			foreach (object row in view.SelectMulti())
			{
				string val = view.Cache.GetValue(row, key) as string;
				if (!String.IsNullOrWhiteSpace(val))
				{
					ret.Add(val);
				}
			}
			return ret.ToList();
		}
	}
	#endregion

	#region PXRoleRightAttribute

	/// <exclude/>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PXRoleRightAttribute : PXIntListAttribute
	{
		List<int> values = new List<int>();
		List<string> labels = new List<string>();

		public PXRoleRightAttribute()
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			values = new List<int>(ListRoleRight.GetIndexesByType(ListRoleRight.RoleRight._All));
			labels = new List<string>(ListRoleRight.GetByType(ListRoleRight.RoleRight._All));
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			Role row = e.Row as Role;
			if (row == null)
			{
				for (int i = 0; i < labels.Count; i++)
				{
					labels[i] = PXMessages.LocalizeNoPrefix(labels[i]);
				}

				e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, false, 1, null, null,
					values.ToArray(),
					labels.ToArray(),
					null, null);
				return;
			}

			if (e.ReturnState is PXIntState && 
				((PXIntState)e.ReturnState).AllowedValues != null)
				return; // already set by someone else.


			values.Clear();
			labels.Clear();

			string identifier = GetIdentifier(row, sender.Graph);
			values.AddRange(ListRoleRight.GetIndexesByType(identifier));
			labels.AddRange(ListRoleRight.GetByType(identifier));

			for (int i = 0; i < labels.Count; i++)
				labels[i] = PXMessages.LocalizeNoPrefix(labels[i]);

			e.ReturnState = PXIntState.CreateInstance(e.ReturnState, _FieldName, false, 1, null, null,
				values.ToArray(), labels.ToArray(),
				null, null);
		}

		protected virtual string GetIdentifier(Role row, PXGraph graph)
		{
			return ListRoleRight.GetRoleRightIdentifier(row, graph);
		}
	}

	#endregion

	#region PXReadOnlyViewAttribute

	/// <summary>
	/// When attached to a data view in a graph, excludes the cache behind the view
	/// from the collection of updatable caches.
	/// The key consequence is that this cache is not persisted by the graph's
	/// Persist() method and by the Save action.
	/// The cache can still be persisted manually.
	/// </summary>
	public class PXReadOnlyViewAttribute : PXViewExtensionAttribute
	{
		private string _viewName;

		/// <exclude />
		public override void ViewCreated(PXGraph graph, string viewName)
		{
			_viewName = viewName;

			graph.Initialized += sender =>
			{
				var cache = graph.Views[_viewName].Cache;

				sender.Views.Caches.Remove(cache.GetItemType());
			};
		}
	}

	#endregion

	#region ScreenIDUpdatableAttribute
	public class ScreenIDUpdatableAttribute : Attribute
	{
	}
	#endregion
}
