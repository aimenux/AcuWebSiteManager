using PX.Data;
using System;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Attribute to display a screen title related to the identified source screen ID
    /// </summary>
    public class SiteMapTitleAttribute : PXStringAttribute, IPXFieldSelectingSubscriber
    {
        private Type _SiteMapScreenID;
        public SiteMapTitleAttribute(Type siteMapScreenID) : base(50)
        {
            _SiteMapScreenID = siteMapScreenID;
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (e.Row == null) return;

            string screenID = sender.GetValue(e.Row, _SiteMapScreenID.Name) as string;
            if (!string.IsNullOrEmpty(screenID))
            {
                var siteMapNode = PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID);
                if (siteMapNode != null)
                {
                    e.ReturnValue = siteMapNode.Title;
                }
            }
        }
    }
}