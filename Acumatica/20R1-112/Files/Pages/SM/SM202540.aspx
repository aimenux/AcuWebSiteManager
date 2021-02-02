<%@ Page Title="External File Storage" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master"
	AutoEventWireup="true" CodeFile="SM202540.aspx.cs" Inherits="Pages_SM_SM202540" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" TypeName="PX.SM.BlobStorageMaint" Visible="True"
		PrimaryView="Filter" PageLoadBehavior="GoFirstRecord" Width="100%">
        <CallbackCommands>
             <px:PXDSCallbackCommand Name="actionPanelMessagesOK" Visible="False"/>
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView runat="server" ID="FormFilter" DataSourceID="ds" DataMember="Filter"
		Width="100%" Caption="Options">
		<Template>
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
		    <px:PXDropDown ID="PXDropDown1" CommitChanges="True" runat="server" DataField="Provider" />
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
		    <px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="AllowWrite" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid runat="server" ID="GridSettings" DataSourceID="ds" Width="100%" Caption="Provider Settings" FeedbackMode="DisableAll">
		<AutoSize Enabled="true" Container="Window" />
		<Levels>
			<px:PXGridLevel DataMember="Settings">
				<Columns>
					<px:PXGridColumn DataField="Name" Width="200" />
					<px:PXGridColumn DataField="Value" Width="400" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
	</px:PXGrid>
    <px:PXSmartPanel runat="server" ID="pnlMessages" Width="600px" Height="500px" CaptionVisible="True" CloseButtonDialogResult="OK"
                     Caption="Messages" ShowMaximizeButton="True" Key="BlobStorageMessages" AutoRepaint="True">
        <px:PXFormView ID="frmMessages" runat="server" SkinID="Transparent" DataMember="BlobStorageMessages" AutoRepaint="True">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXTextEdit ID="PXMessagesText" runat="server" DataField="Messages" TextMode="MultiLine" SuppressLabel="true" Width="540px" Height="440px" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanelMessagesButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnMessagesOk" runat="server" DialogResult="OK" Text="Ok" PopupPanel="pnlMessages">
                <AutoCallBack Enabled="True" Target="ds" Command="actionPanelMessagesOK"/>
            </px:PXButton>
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
