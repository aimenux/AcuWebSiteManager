<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR503420.aspx.cs" Inherits="Page_CR503420"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
		TypeName="PX.Objects.CR.CRAccountValidationProcess" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
	    <CallbackCommands>
	        <px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" /> 
            <px:PXDSCallbackCommand Visible="false" DependOnGrid="grdItems" Name="Contacts_BAccount_ViewDetails" />	
	    </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" DataMember="Filter" Caption="General Settings" AllowCollapse="False" TabIndex="100">
        <Template>
            <px:PXGroupBox ID="gbValidateType" runat="server" DataField="ValidationType" RenderStyle="RoundBorder" CommitChanges="True">
			    <Template>
				    <px:PXRadioButton ID="gbValidateType_op0" runat="server" Text="Validate New Records Only"
					    Value="0" GroupName="gbValidateType" />
				    <px:PXRadioButton ID="gbValidateType_op1" runat="server" Text="Validate All Records (Takes More Time and Resets Validation Status for Validated Accounts)"
					    Value="1" GroupName="gbValidateType" />
			    </Template>
			    <ContentLayout Layout="Stack" />
            </px:PXGroupBox>
        </Template>
    </px:PXFormView>
	<px:PXGrid ID="grdItems" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" Caption="Grams" AllowPaging="True" AdjustPageSize="auto" NoteIndicator="False" FilesIndicator="False"
		SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="Contacts">
				<Columns>
				    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="0px" AllowCheckAll="True" />                    					
                    <px:PXGridColumn DataField="Type" />
                    <px:PXGridColumn DataField="BAccountID" LinkCommand="Contacts_BAccount_ViewDetails"/>
                    <px:PXGridColumn DataField="AcctName" />					
                    <px:PXGridColumn DataField="AccountStatus" />
					<px:PXGridColumn DataField="DuplicateStatus" />                    
				</Columns>
			</px:PXGridLevel>
		</Levels>
        <ActionBar PagerVisible="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
