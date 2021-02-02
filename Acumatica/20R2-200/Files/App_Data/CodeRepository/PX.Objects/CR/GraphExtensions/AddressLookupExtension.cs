using PX.AddressValidator;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CR.Extensions
{
	#region Constants for filter Address Plug-Ins
	public class AddressLookupNamespaceName : PX.Data.BQL.BqlString.Constant<AddressLookupNamespaceName>
	{
		public AddressLookupNamespaceName()
			: base(nameof(PX.AddressLookup))
		{
		}
	}

	public class AddressValidatorNamespaceName : PX.Data.BQL.BqlString.Constant<AddressValidatorNamespaceName>
	{
		public AddressValidatorNamespaceName()
			: base(nameof(PX.AddressValidator))
		{
		}
	}
	#endregion

	#region PreferencesGeneral AddressLookup Extension
	[Serializable]
	public sealed class PreferencesGeneralAddressLookupExtension : PXCacheExtension<PX.SM.PreferencesGeneral>
	{
		#region AddressLookupPluginID
		public abstract class addressLookupPluginID : PX.Data.BQL.BqlString.Field<addressLookupPluginID> { }
		/// <value>
		/// <see cref="PX.Objects.CS.AddressValidatorPlugin.addressValidatorPluginID"/> of a Address Lookup which will be used. 
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Lookup Plug-In", FieldClass = FeaturesSet.addressLookup.FieldClass)]
		[PXSelector(typeof(CS.AddressValidatorPlugin.addressValidatorPluginID), DescriptionField = typeof(CS.AddressValidatorPlugin.description))]
		[PXRestrictor(typeof(Where<AddressValidatorPlugin.isActive, Equal<True>, And<AddressValidatorPlugin.pluginTypeName, Contains<AddressLookupNamespaceName>>>), CS.Messages.AddressLookupPluginIsNotActive)]
		public string AddressLookupPluginID { get; set; }
		#endregion
	}
	#endregion

	#region Country AddressLookup Extension
	[Serializable]
	public sealed class CountryAddressLookupExtension : PXCacheExtension<Country>
	{
		#region AddressLookupPluginID
		[PXRestrictor(typeof(Where<AddressValidatorPlugin.isActive, Equal<True>, And<AddressValidatorPlugin.pluginTypeName, Contains<AddressValidatorNamespaceName>>>), CS.Messages.AddressVerificationPluginIsNotActive)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public string AddressValidatorPluginID { get; set; }
		#endregion
	}
	#endregion


	public class AddressLookupPreferencesGeneralMaintExt : PXGraphExtension<PreferencesGeneralMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.addressLookup>();
		}

		public virtual void _(Events.FieldVerifying<PreferencesGeneral.addressLookupPluginID> e)
		{
			PreferencesGeneral row = e.Row as PreferencesGeneral;
			if (row != null &&
				string.IsNullOrEmpty(e?.NewValue?.ToString()) == false)
			{
				AddressValidatorPlugin addressValidatorPlugin =
					PXSelect<
						AddressValidatorPlugin,
						Where<AddressValidatorPlugin.addressValidatorPluginID, Equal<Required<AddressValidatorPlugin.addressValidatorPluginID>>>>
						.SelectWindowed(Base, 0, 1, e.NewValue.ToString());
				if (addressValidatorPlugin != null &&
					addressValidatorPlugin.PluginTypeName.Contains(nameof(AddressLookup)) == false)
				{
					throw new PXSetPropertyException(Messages.WrongPluginTypeSelected);
				}
			}
		}
	}

	[Serializable]
	[PXHidden]
	public partial class AddressLookupFilter : IBqlTable, IAddressBase
	{
		#region SearchAddress
		public abstract class searchAddress : PX.Data.BQL.BqlString.Field<searchAddress> { }
		[PXString]
		[PXUIField(Visible = true)]
		public virtual string SearchAddress { get; set; }
		#endregion

		#region ViewName
		public abstract class viewName : PX.Data.BQL.BqlString.Field<viewName> { }
		[PXString]
		[PXUIField(Visible = false)]
		public virtual string ViewName { get; set; }
		#endregion

		#region IAddressBase fields

		#region AddressLine1
		public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }
		[PXString]
		[PXUIField(Visible = false)]
		public virtual string AddressLine1 { get; set; }
		#endregion

		#region AddressLine2
		public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }
		[PXDBString]
		[PXUIField(Visible = false)]
		public virtual string AddressLine2 { get; set; }
		#endregion

		#region AddressLine3
		public abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }
		[PXString]
		[PXUIField(Visible = false)]
		public virtual string AddressLine3 { get; set; }
		#endregion

		#region City
		public abstract class city : PX.Data.BQL.BqlString.Field<city> { }
		[PXString]
		[PXUIField(Visible = false)]
		public virtual string City { get; set; }
		#endregion

		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		[PXString]
		[PXUIField(Visible = false)]
		public virtual string CountryID { get; set; }
		#endregion

		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		[PXString]
		[PXUIField(Visible = false)]
		public virtual string State { get; set; }
		#endregion

		#region PostalCode
		public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
		[PXString]
		[PXUIField(Visible = false)]
		public virtual string PostalCode { get; set; }
		#endregion

		#region Latitude
		public abstract class latitude : PX.Data.BQL.BqlDecimal.Field<latitude> { }
		[PXDBDecimal(6)]
		[PXUIField(Visible = false)]
		public virtual decimal? Latitude { get; set; }
		#endregion

		#region Longitude
		public abstract class longitude : PX.Data.BQL.BqlDecimal.Field<longitude> { }
		[PXDBDecimal(6)]
		[PXUIField(Visible = false)]
		public virtual decimal? Longitude { get; set; }
		#endregion

		#endregion
	}

	public abstract class AddressLookupExtension<TGraph, TMaster, TAddress> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TMaster : class, IBqlTable, new()
		where TAddress : class, IBqlTable, IAddressLocation, new()
	{
		#region State

		public string ActionName = null;
		public const string AddressLookupSelectActionName = "AddressLookupSelectAction";
		private const string _VIEWONMAP_ACTION = "ViewOnMap";
		#endregion

		#region ctor

		public override void Initialize()
		{
			base.Initialize();

			ActionName = ActionName ?? GetType().Name;

			if (ActionName.StartsWith(typeof(TGraph).Name))
			{
				ActionName = ActionName.Substring(typeof(TGraph).Name.Length);
			}

			if (ActionName.EndsWith("Extension"))
			{
				ActionName = ActionName.Substring(0, ActionName.Length - "Extension".Length);
			}

			AddAction(ActionName, CR.Messages.AddressLookup, AddressLookup, false);
			AddAction(AddressLookupSelectActionName, CR.Messages.AddressLookupButton, AddressLookupSelectAction, true);
		}

		protected void AddAction(string name, string displayName, PXButtonDelegate handler, bool visible)
		{
			PXUIFieldAttribute uiAtt = new PXUIFieldAttribute
			{
				DisplayName = PXMessages.LocalizeNoPrefix(displayName),
				MapEnableRights = PXCacheRights.Update,
				MapViewRights = PXCacheRights.Select,
				Visible = visible,
				Enabled = true
			};

			PXButtonAttribute buttAttr = new PXButtonAttribute();
			List<PXEventSubscriberAttribute> addAttrs = new List<PXEventSubscriberAttribute> { uiAtt, buttAttr };

			Base.Actions[name] = new PXNamedAction<TMaster>(Base, name, handler, addAttrs.ToArray());
		}

		#endregion

		#region Views

		public PXFilter<AddressLookupFilter> AddressLookupFilter;

		protected abstract string AddressView { get; }
		protected virtual string ViewOnMap => null;
		protected virtual string ViewOnMapActionName => _VIEWONMAP_ACTION;

		#endregion

		#region Actions

		protected virtual IAddressLocation CurrentAddress
		{
			get
			{
				PXView view = Base.Views[AddressView];

				object result = view.SelectSingle();

				IAddressLocation currentAddress = (IAddressLocation)((result is PXResult res) ? res[0] : result);
				return currentAddress;
			}
		}

		protected virtual IEnumerable AddressLookupSelectAction(PXAdapter adapter)
		{
			AddressLookupFilter addressAutocompleteResponse = AddressLookupFilter?.Current;
			string addressView = addressAutocompleteResponse.ViewName ?? AddressView;
			if (string.IsNullOrEmpty(addressView))
			{
				return adapter.Get();
			}
			PXView view = Base.Views[addressView];

			object result = view.SelectSingle();

			IAddressLocation currentAddress = (IAddressLocation)((result is PXResult res) ? res[0] : result);

			if (addressAutocompleteResponse != null && currentAddress != null && 
				!string.IsNullOrEmpty(addressAutocompleteResponse.CountryID) &&
				!string.IsNullOrEmpty(addressAutocompleteResponse.City))
			{
				currentAddress.Longitude = addressAutocompleteResponse.Longitude;
				currentAddress.Latitude = addressAutocompleteResponse.Latitude;
				currentAddress.AddressLine1 = addressAutocompleteResponse.AddressLine1 ?? "";
				currentAddress.AddressLine2 = addressAutocompleteResponse.AddressLine2 ?? "";
				currentAddress.AddressLine3 = addressAutocompleteResponse.AddressLine3 ?? "";
				currentAddress.CountryID = addressAutocompleteResponse.CountryID ?? currentAddress.CountryID;
				currentAddress.City = addressAutocompleteResponse.City ?? "";
				currentAddress.PostalCode = addressAutocompleteResponse.PostalCode ?? "";
				currentAddress.State = addressAutocompleteResponse?.State ?? "";
				if (string.IsNullOrEmpty(currentAddress.State) == false)
				{
					State state =
						PXSelectReadonly<
							State,
							Where<State.name, Equal<Required<State.name>>>>
							.SelectSingleBound(Base, null, currentAddress.State);
					if (state != null)
					{
						currentAddress.State = state.StateID;
					}
				}
				view.Cache.Update(currentAddress);
			}
			AddressLookupFilter.Cache.Clear();
			AddressLookupFilter.Cache.IsDirty = false;
			return adapter.Get();
		}

		protected virtual IEnumerable AddressLookup(PXAdapter adapter)
		{
			switch (AddressLookupFilter.View.Answer.GetAnswerType())
			{
				case DialogAnswerType.None:
					Base.Actions[AddressLookupSelectActionName].SetVisible(true);
					Base.Actions[AddressLookupSelectActionName].SetEnabled(GetIsEnabled());
					AddressLookupFilter.Reset();
					AddressLookupFilter currentAddress =
						new AddressLookupFilter
						{
							SearchAddress = "",
							ViewName = AddressView,

							Longitude = CurrentAddress?.Longitude,
							Latitude = CurrentAddress?.Latitude,
							AddressLine1 = CurrentAddress?.AddressLine1,
							AddressLine2 = CurrentAddress?.AddressLine2,
							AddressLine3 = CurrentAddress?.AddressLine3,
							CountryID = CurrentAddress?.CountryID,
							City = CurrentAddress?.City,
							PostalCode = CurrentAddress?.PostalCode,
							State = CurrentAddress?.State
						};
					Base.Caches<AddressLookupFilter>().Clear();
					Base.Caches<AddressLookupFilter>().Insert(currentAddress);
					Base.Caches<AddressLookupFilter>().IsDirty = false;
					AddressLookupFilter.SelectSingle();
					AddressLookupFilter.AskExt(true);
					break;

				case DialogAnswerType.Positive:
				case DialogAnswerType.Negative:
				case DialogAnswerType.Neutral:
				default:
					break;
			}
			return adapter.Get();
		}

		#endregion

		#region Events
		public virtual void _(Events.RowSelected<TMaster> e)
		{
			bool isVisible = false;
			if (string.IsNullOrEmpty(ActionName) == false && Base.Actions.Contains(ActionName))
			{
				isVisible = PXAddressLookup.IsEnabled(Base);
				Base.Actions[ActionName].SetVisible(isVisible);
				Base.Actions[ActionName].SetEnabled(true);
			}

			if (string.IsNullOrEmpty(ViewOnMap) == false && Base.Actions.Contains(ViewOnMap))
			{
				Base.Actions[ViewOnMap].SetVisible(!isVisible);
			}
			else if (string.IsNullOrEmpty(ViewOnMapActionName) == false && Base.Actions.Contains(ViewOnMapActionName))
			{
				Base.Actions[ViewOnMapActionName].SetVisible(!isVisible);
			}
		}
		#endregion

		#region Methods
		protected virtual bool GetIsEnabled()
		{
			PXView view = Base.Views[AddressView];
			object result = view.SelectSingle();
			TAddress address = (TAddress)((result is PXResult res) ? res[0] : result);
			if (address == null)
			{
				bool isDirty = view.Cache.IsDirty;
				address = (TAddress)view.Cache.Insert();
				view.Cache.IsDirty = isDirty;
			}

			bool isEnabled =
				(view.Cache.GetStateExt<Address.addressLine1>(address) as PXFieldState).Enabled
				&& (view.Cache.GetStateExt<Address.addressLine2>(address) as PXFieldState).Enabled
				&& (view.Cache.GetStateExt<Address.addressLine3>(address) as PXFieldState).Enabled
				&& (view.Cache.GetStateExt<Address.city>(address) as PXFieldState).Enabled
				&& (view.Cache.GetStateExt<Address.state>(address) as PXFieldState).Enabled
				&& (view.Cache.GetStateExt<Address.postalCode>(address) as PXFieldState).Enabled
				&& (view.Cache.GetStateExt<Address.countryID>(address) as PXFieldState).Enabled
				&& (view.AllowInsert || view.AllowUpdate);

			return isEnabled;
		}
		#endregion
	}
}
