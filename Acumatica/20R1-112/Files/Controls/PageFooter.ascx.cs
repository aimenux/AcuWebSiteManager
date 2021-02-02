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

#region

using System;
using System.Reflection;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Web.UI;

#endregion

[Themeable(true)]
public partial class User_PageFooter : System.Web.UI.UserControl
{
	protected void Page_Init(object sender, EventArgs e)
	{
		if (PXDataSource.RedirectHelper.IsPopupPage(Page)) Visible = false;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		// sets the controls styles
		if (!string.IsNullOrEmpty(cssClass))
		{
			pnlFooter.Style.Clear();
			pnlFooter.ControlStyle.Reset();
			pnlFooter.CssClass = cssClass;
		}
		SetControlCss(lblCopyright, cssClassText);
		// sets the copyright string from PX.Web.UI assembly
		AssemblyCopyrightAttribute copyR = Attribute.GetCustomAttribute(
			typeof(PXDataSource).Assembly, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
		if (copyR != null) lblCopyright.Text = copyR.Copyright;
	}

	/// <summary>
	/// Sets the specified control CSS class.
	/// </summary>
	private void SetControlCss(WebControl ctrl, string cssClass)
	{
		if (!string.IsNullOrEmpty(cssClass))
		{
			ctrl.ControlStyle.Reset();
			ctrl.CssClass = cssClass;
		}
	}

	#region Public properties

	/// <summary>
	/// Gets or sets the skin to apply to the control.
	/// </summary>
	[Browsable(true)]
	public override string SkinID
	{
		get { return base.SkinID; }
		set { base.SkinID = value; }
	}

	/// <summary>
	/// Gets or sets the control CSS class name.
	/// </summary>
	[Category("Styles"), Description("CSS class name applied to the control.")]
	public string CssClass
	{
		get { return cssClass; }
		set { cssClass = value; }
	}

	/// <summary>
	/// Gets or sets the screen ID label CSS class name.
	/// </summary>
	[Category("Styles"), Description("CSS class name applied to the text labels.")]
	public string CssClassText
	{
		get { return cssClassText; }
		set { cssClassText = value; }
	}

	#endregion

	private string cssClass = "PageFooter";
	private string cssClassText = "ScreenID";
}