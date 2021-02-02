using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.Relational;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.CS
{
	public class SeparateBAccountMaint : PXGraph<SeparateBAccountMaint, BAccount>
	{
		#region Public Selects

		[PXViewName(CR.Messages.BAccount)]
		public PXSelect<
				BAccount,
			Where<
				Match<Current<AccessInfo.userName>>>>
			BAccount;

		[PXHidden]
		public PXSelect<CRLocation>
			BaseLocations;

		[PXHidden]
		public PXSelect<Address>
			AddressDummy;

		[PXHidden]
		public PXSelect<Contact>
			ContactDummy;

		public PXSetup<GL.Company>
			Commpany;

		#endregion

		#region Events Handlers

		[PXDimensionSelector("BRANCH", typeof(BAccount.acctCD), typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<BAccount.acctCD> e) { }

		#endregion

		#region Extensions

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class SeparateDefSharedContactOverrideGraphExt : SharedChildOverrideGraphExt<SeparateBAccountMaint, SeparateDefSharedContactOverrideGraphExt>
		{
			#region Initialization 

			public override bool ViewHasADelegate => true;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLocation))
				{
					RelatedID = typeof(CRLocation.bAccountID),
					ChildID = typeof(CRLocation.defContactID),
					IsOverrideRelated = typeof(CRLocation.overrideContact)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(BAccount))
				{
					RelatedID = typeof(BAccount.bAccountID),
					ChildID = typeof(BAccount.defContactID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Contact))
				{
					ChildID = typeof(Contact.contactID),
					RelatedID = typeof(Contact.bAccountID),
				};
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class SeparateDefSharedAddressOverrideGraphExt : SharedChildOverrideGraphExt<SeparateBAccountMaint, SeparateDefSharedAddressOverrideGraphExt>
		{
			#region Initialization 

			public override bool ViewHasADelegate => true;

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CRLocation))
				{
					RelatedID = typeof(CRLocation.bAccountID),
					ChildID = typeof(CRLocation.defAddressID),
					IsOverrideRelated = typeof(CRLocation.overrideAddress)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(BAccount))
				{
					RelatedID = typeof(BAccount.bAccountID),
					ChildID = typeof(BAccount.defAddressID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Address))
				{
					ChildID = typeof(Address.addressID),
					RelatedID = typeof(Address.bAccountID),
				};
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefContactAddressExt : DefContactAddressExt<SeparateBAccountMaint, BAccount, BAccount.acctName> { }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class DefLocationExt : DefLocationExt<SeparateBAccountMaint, DefContactAddressExt, LocationDetailsExt, BAccount, BAccount.bAccountID, BAccount.defLocationID>
			.WithUIExtension { }

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class LocationDetailsExt : LocationDetailsExt<SeparateBAccountMaint, DefContactAddressExt, BAccount, BAccount.bAccountID> { }

		#endregion
	}
}
