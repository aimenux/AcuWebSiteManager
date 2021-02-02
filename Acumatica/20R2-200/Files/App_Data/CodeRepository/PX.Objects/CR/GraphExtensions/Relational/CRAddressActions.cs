using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.MassProcess;
using PX.Objects.Common;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Api;
using FieldValue = PX.Data.MassProcess.FieldValue;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Common.Mail;
using System.Reflection;

namespace PX.Objects.CR.Extensions.Relational
{
	/// <exclude/>
	public abstract class CRAddressActions<TGraph, TMain> : CRParentChild<TGraph, CRAddressActions<TGraph, TMain>>
		where TGraph : PXGraph
		where TMain : class, IBqlTable, new()
	{
		#region Events

		protected virtual void _(Events.RowSelected<Document> e)
		{
			var row = e.Row as Document;
			if (row == null)
				return;

			var currentAddress = GetChildByID(row.ChildID)?.Base as Address;

			ValidateAddress.SetEnabled(currentAddress?.IsValidated != true);
		}

		#endregion

		#region Actions

		public PXAction<TMain> ValidateAddress;

		[PXUIField(DisplayName = CS.Messages.ValidateAddress, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		protected virtual IEnumerable validateAddress(PXAdapter adapter)
		{
			Document primaryDocument = PrimaryDocument.Current;
			if (primaryDocument == null)
				return adapter.Get();

			var child = GetChildByID(primaryDocument.ChildID);

			Address address = ChildDocument.Cache.GetMain(child) as Address;

			if (address != null && address.IsValidated != true)
				PXAddressValidator.Validate<Address>(Base, address, true);

			return adapter.Get();
		}

		public PXAction<TMain> ViewOnMap;

		[PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		protected virtual IEnumerable viewOnMap(PXAdapter adapter)
		{
			Document primaryDocument = PrimaryDocument.Current;
			if (primaryDocument == null)
				return adapter.Get();

			var child = GetChildByID(primaryDocument.ChildID);

			Address address = ChildDocument.Cache.GetMain(child) as Address;

			if (address != null)
				BAccountUtility.ViewOnMap(address);

			return adapter.Get();
		}
		#endregion
	}
}
