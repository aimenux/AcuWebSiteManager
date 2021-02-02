using System;
using PX.Commerce.Core;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Commerce.Objects
{
    [Serializable]
	[PXCacheName("Locations")]
	public class BCLocations : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BCLocations>.By<BCLocations.bCLocationsID>
		{
			public static BCLocations Find(PXGraph graph, int? bCLocationsID) => FindBy(graph, bCLocationsID);
		}
		public static class FK
		{
			public class Binding : BCBinding.BindingIndex.ForeignKeyOf<BCLocations>.By<bindingID> { }
			public class Site : INSite.PK.ForeignKeyOf<BCLocations>.By<siteID> { }
			public class Location : INLocation.PK.ForeignKeyOf<BCLocations>.By<locationID> { }
		}
		#endregion

		#region BCLocationsID
		[PXDBIdentity(IsKey = true)]
        public int? BCLocationsID { get; set; }
        public abstract class bCLocationsID : IBqlField { }
        #endregion

        #region BindingID
        [PXDBInt()]
		[PXUIField(DisplayName = "Store")]
		[PXSelector(typeof(BCBinding.bindingID),
					typeof(BCBinding.bindingName),
					SubstituteKey = typeof(BCBinding.bindingName))]
		[PXParent(typeof(Select<BCBinding,
			Where<BCBinding.bindingID, Equal<Current<BCLocations.bindingID>>>>))]
		[PXDBDefault(typeof(BCBinding.bindingID), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? BindingID { get; set; }
		public abstract class bindingID : IBqlField { }
		#endregion

		#region SiteID
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse")]
		[PXSelector(typeof(INSite.siteID),
					SubstituteKey = typeof(INSite.siteCD),
					DescriptionField = typeof(INSite.descr))]
		[PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>),
							 PX.Objects.IN.Messages.TransitSiteIsNotAvailable)]
		[PXDefault()]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : IBqlField { }
		#endregion

		#region LocationID
		[PXDBInt()]
        [PXUIField(DisplayName = "Location ID")]
		[PXSelector(typeof(Search<INLocation.locationID, 
			Where<INLocation.siteID, Equal<Current<BCLocations.siteID>>>>),
			SubstituteKey = typeof(INLocation.locationCD),
			DescriptionField = typeof(INLocation.descr)
			)]
		public virtual int? LocationID { get; set; }
        public abstract class locationID : IBqlField { }
        #endregion
    }
}