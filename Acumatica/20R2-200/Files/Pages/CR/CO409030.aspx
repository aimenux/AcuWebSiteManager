<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="CO409030.aspx.cs" Inherits="Pages_CO409030"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="true" Width="100%"
        PrimaryView="Emails" TypeName="PX.Objects.CR.CRCommunicationArchive">
        <CallbackCommands>
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewEMail" Visible="False"/>
            <px:PXDSCallbackCommand DependOnGrid="gridInbox" Name="reply" Visible="False" RepaintControls="All" />
			<px:PXDSCallbackCommand DependOnGrid="gridInbox" Name="replyAll" Visible="False" RepaintControls="All" />
			<px:PXDSCallbackCommand DependOnGrid="gridInbox" Name="forward" Visible="False" RepaintControls="All" />
			  <px:PXDSCallbackCommand DependOnGrid="gridInbox" Name="viewEntity" Visible="false" RepaintControls="All" /> 
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" ActionsPosition="Top"
        AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" SyncPosition="True" NoteIndicator="false"
        SkinID="PrimaryInquire" Width="100%" RestrictFields="False" FastFilterFields="Subject,MailTo,EMailAccount__Description">
        <Levels>
            <px:PXGridLevel DataMember="Emails">
                <Columns>
                    <px:PXGridColumn DataField="Selected" Width="50px" AllowCheckAll="True" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="IsIncome" Width="50px" TextAlign="Center" Type="CheckBox" AllowShowHide="True" Visible="false" SyncVisible="false"/>
                    <px:PXGridColumn DataField="Subject" Width="450px" LinkCommand="ViewEMail" />
                    <px:PXGridColumn DataField="MailTo" Width="250px" />
                    <px:PXGridColumn DataField="MailCc" Width="250px" AllowShowHide="True" Visible="false" SyncVisible="false"/>
                    <px:PXGridColumn DataField="MailBcc" Width="250px" AllowShowHide="True" Visible="false" SyncVisible="false"/>
                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="120px" />
                    <px:PXGridColumn DataField="MailFrom" Width="200px" />
                    <px:PXGridColumn DataField="Source" Width="150px" LinkCommand="viewEntity" AllowSort="false" AllowFilter="false" />
					<px:PXGridColumn DataField="WorkgroupID" Width="90px" DisplayMode="Text" />
                    <px:PXGridColumn DataField="OwnerID" Width="90px" DisplayMode="Text" />
                    <px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="90px" />
                    <px:PXGridColumn DataField="IsArchived" Width="70px" Type="CheckBox" TextAlign="Center" AllowShowHide="True" Visible="false" SyncVisible="false" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
		<ActionBar DefaultAction="DoubleClick" PagerVisible="False">
		    <Actions>
		        <AddNew Enabled="False"/>
                <Delete GroupIndex="1"/>
		    </Actions>
		</ActionBar>
        <Mode AllowUpdate="False" AllowAddNew="False" />
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>