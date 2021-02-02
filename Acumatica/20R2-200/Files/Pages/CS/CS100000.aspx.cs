using System;
using System.Collections.Generic;
using System.Configuration;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Objects.CS;
using PX.Web.UI;
using PX.Data;

public partial class Page_CS100000 : PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		//if (this.form.TemplateContainer.Controls.Count <= 1) return;
		PXCache cache = this.ds.DataGraph.Caches[typeof(FeaturesSet)];
		var features = new List<string>();
		var disabled = new List<string>();
		var subfeatures = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);

		foreach (string field in cache.Fields)
		{
			string parentName = null;
			bool featureFound = false;

			
			var featureAttributes = cache.GetAttributes(null, field).OfType<FeatureAttribute>();
			foreach (FeatureAttribute feature in featureAttributes)
			{
				featureFound = feature.Visible;
				if (feature.Top != true && feature.Parent != null)
					parentName = feature.Parent.Name;
			}

			if (!featureFound) continue;

			if (parentName != null)
			{
				if (!subfeatures.ContainsKey(parentName))
					subfeatures.Add(parentName, new List<string>());
				subfeatures[parentName].Add(field);
			}
			else
			{
				features.Add(field);
			}

			if (PXAccess.FeatureReadOnly(field) || (parentName != null && PXAccess.FeatureReadOnly(parentName)))
			{
				disabled.Add(field);
			}
		}
		AddControls(0, features, subfeatures, disabled);
	}
	protected void Page_Load(object sender, EventArgs e)
	{
	}

	private void AddControls(int level, IEnumerable<string> features,   Dictionary<string, List<string>> subfeatures, IEnumerable<string> disabled)
	{
		foreach (string feature in features)
		{
			if (this.form.TemplateContainer.FindControl(feature) != null) continue;
			if (PX.Data.PXLicenseHelper.License.Licensed && !PXAccess.BypassLicense && !PX.Data.PXLicenseHelper.License.Features.Any(f => f.EndsWith(feature, StringComparison.InvariantCultureIgnoreCase))) continue;
			
			PXCheckBox control = new PXCheckBox();
			control.ID = feature;
			control.DataField = feature;
			control.Enabled = !disabled.Contains(feature);
			control.TextAlign = TextAlign.Right;
			if(level > 0)
				control.Style["margin-left"] = level*25 + "px";			
			control.CommitChanges = true;
			control.ApplyStyleSheetSkin(this.Page);		    
			this.form.TemplateContainer.Controls.Add(control);
			if (subfeatures.ContainsKey(feature))
				AddControls(level +1, subfeatures[feature], subfeatures, disabled);
		}
	}
}
