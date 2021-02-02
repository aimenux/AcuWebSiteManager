<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR408000.aspx.cs" Inherits="Page_AR408000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.ARDunningLetterByCustomerEnq">
	        <CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" Name="ViewDocument" CommitChanges="true" DependOnGrid="grid" />
		</CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" TabIndex="1100">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True"/>
            <px:PXSelector  CommitChanges="true" ID="edBAccountID" runat="server" DataField="BAccountID"/>
		    <px:PXDateTimeEdit CommitChanges="true" ID="edBeginDate" runat="server" DataField="BeginDate"/>
            <px:PXDateTimeEdit  CommitChanges="true" ID="edEndDate" runat="server" DataField="EndDate"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Details" TabIndex="1300" NoteIndicator="false" FilesIndicator="false" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataKeyNames="DunningLetterID" DataMember="EnqResults">
			    <Columns>
                    <px:PXGridColumn DataField="BranchID" DisplayMode="Text"/>
                    <px:PXGridColumn DataField="BAccountID" DisplayMode="Text"/>
                    <px:PXGridColumn DataField="Customer__AcctName"/>
                    <px:PXGridColumn DataField="DunningLetterLevel"/>
                    <px:PXGridColumn DataField="Status"  Type="DropDownList"/>
                    <px:PXGridColumn DataField="ARDunningLetterDetail__OverdueBal" />
                    <px:PXGridColumn DataField="DunningLetterDate"/>
                    <px:PXGridColumn DataField="DetailsCount"/>

                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />        
        <ActionBar DefaultAction="ViewDocument">
			<CustomItems>
                   <px:PXToolBarButton Text="View Dunning Letter" Key="cmdViewDocument">
					    <AutoCallBack Command="ViewDocument" Target="ds" />
				    </px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
