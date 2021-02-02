<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR503410.aspx.cs" Inherits="Page_CR503410"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
		TypeName="PX.Objects.CR.CRLeadContactValidationProcess" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
	    <CallbackCommands>
	        <px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" /> 
            <px:PXDSCallbackCommand Visible="false" DependOnGrid="grdItems" Name="Contacts_Contact_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grdItems" Name="Contacts_BAccount_ViewDetails" />	
	    </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" DataMember="Filter" Caption="General Settings" AllowCollapse="False" TabIndex="100">
        <Template>
            <px:PXGroupBox ID="gbValidateType" runat="server" DataField="ValidationType" RenderStyle="Simple" CommitChanges="True" Style="margin-left: -3px;">
			    <Template>
			        <px:PXLayoutRule ID="PXLr0" runat="server" StartColumn="True"/>
				    <px:PXRadioButton ID="gbValidateType_op0" runat="server" Text="Validate New Records Only"
					    Value="0" GroupName="gbValidateType" />
				    <px:PXRadioButton ID="gbValidateType_op1" runat="server" Text="Validate All Records (Takes More Time and Resets Validation Status for Validated Leads)"
					    Value="1" GroupName="gbValidateType" />
                    
			    </Template>
			    <ContentLayout OuterSpacing="Around"/>
            </px:PXGroupBox>
            
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartRow="True" LabelsWidth="XXL" ControlSize="XXS" />
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" SuppressLabel="True" ControlSize="XXS" />
            <px:PXCheckBox ID="lblCloseNoActivityLeads" runat="server" DataField="CloseNoActivityLeads"/>
            
            <px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XXS" />
            <px:PXNumberEdit ID="edCloseThreshold" Size="xxs" runat="server" DataField="CloseThreshold" />
        </Template>
    </px:PXFormView>
	<px:PXGrid ID="grdItems" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" Caption="Grams" AllowPaging="True" AdjustPageSize="auto" NoteIndicator="False" FilesIndicator="False"
		SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="Contacts">
				<Columns>
				    <px:PXGridColumn DataField="Selected" Width="0px" Type="CheckBox" AllowCheckAll="true" />
                    <px:PXGridColumn DataField="DisplayName" LinkCommand="Contacts_Contact_ViewDetails" />
                    <px:PXGridColumn DataField="ContactType" />
                    <px:PXGridColumn DataField="BAccountID" LinkCommand="Contacts_BAccount_ViewDetails" />
                    <px:PXGridColumn DataField="AcctName" />
                    <px:PXGridColumn DataField="Status" />
					<px:PXGridColumn DataField="DuplicateStatus" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
	    <ActionBar PagerVisible="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
