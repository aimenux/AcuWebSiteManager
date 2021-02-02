#region Copyright (c) 1994-2006 PXSoft, Inc. All rights reserved.
/* ---------------------------------------------------------------------*
*                               PXSoft, Inc.                            *
*              Copyright (c) 1994-2006 All rights reserved.             *
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
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY PXSoft PRODUCT.          *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 1994-2006 PXSoft, Inc. All rights reserved.

using System;
using System.ComponentModel;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Common;
using PX.Data;
using System.Threading;
using System.Globalization;
using System.Reflection;
using PX.SM;

namespace PX.Web.Controls
{
	public partial class Controls_TraceItem : System.Web.UI.UserControl
	{
		private TraceItem Tag;

		public void Initialise(TraceItem item)
		{
			Tag = item;
		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			lblType.Text = GetItemType(Tag) + ":";
			lblCaption.Text = Tag.Message.Replace("\n", "<br>"); //asp label doesn't support new line character so we replace it to html equivalent
			lblDate.Text = "Raised At: \t" + Tag.RaiseDateTime.ToString();
			lblScreen.Text = "Screen: \t" + Tag.ScreenID;
			lblDetails.Text = Tag.Details.Replace("\n", "<br>").Replace("\t", "&nbsp").Replace("  ", "&nbsp&nbsp");

			Int32 sIndex = String.IsNullOrEmpty(Tag.Source) ? -1 : Tag.Source.LastIndexOf("|");
			if (sIndex >= 0) lblSource.Text = "Command: \t" + Tag.Source.Substring(sIndex + 1);
			else lblSource.Visible = false;
		}
		protected void btnSend_Click(object sender, EventArgs e)
		{
			TraceMaint.SendTrace(this.Page, Tag);
		}
		protected String GetItemType(TraceItem item)
		{
			Int32 value = item.EventType.Value;
			PropertyInfo pi = typeof(TraceItem).GetProperty("EventType", BindingFlags.Instance | BindingFlags.Public);
			PXIntListAttribute attr = (PXIntListAttribute)pi.GetCustomAttributes(typeof(PXIntListAttribute), false)[0];
			return attr.ValueLabelDic[value];
		}
	}
}
