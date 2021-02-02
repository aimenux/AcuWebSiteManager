using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Commerce.Core;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.SM;

namespace PX.Commerce.Objects
{
	public class BCINCategoryMaintExt : PXGraphExtension<INCategoryMaint>
	{
		public static bool IsActive() { return FeaturesHelper.CommerceEdition; }

		[PXBreakInheritance]
		[PXHidden]
		public class SelectedINCategory : INCategory
		{
			public abstract new class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			[PXNote]
			public override Guid? NoteID { get; set; }

		}
		public PXSelectReadonly<SelectedINCategory> SelectedCategory;

		public override void Initialize()
		{
			base.Initialize();

			if (SelectedCategory.Current != null)
			{
				Base.Folders.Cache.ActiveRow = SelectedCategory.Current;
				SelectedCategory.Current = null;
			}
		}

		//Sync Time 
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PX.Commerce.Core.BCSyncExactTime()]
		public void INCategory_LastModifiedDateTime_CacheAttached(PXCache sender) { }
	}
}