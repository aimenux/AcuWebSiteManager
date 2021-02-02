 <%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AR203000.aspx.cs" Inherits="Pages_AR203000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds"  runat="server" Visible="True" Width="100%" PrimaryView="RUTROTSetupView" TypeName="PX.Objects.RUTROT.RUTROTWorkTypesMaint" >
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="MoveUp" Visible="False" />
			<px:PXDSCallbackCommand Name="MoveDown" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="frmHeader" runat="server" DataSourceID="ds" Width="100%" Caption="Header"
		DataMember="RUTROTSetupView"  NoteIndicator="True" FilesIndicator="True"
		ActivityIndicator="True" ActivityField="NoteActivity"
		TabIndex="100">
		<Template>
		    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Header" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XXL" />
			<px:PXTextEdit ID="edNsMain" runat="server" DataField="NsMain" />
            <px:PXTextEdit ID="edNsHtko" runat="server" DataField="NsHtko" />
            <px:PXTextEdit ID="edNsXsi" runat="server" DataField="NsXsi" />
            <px:PXTextEdit ID="edSchemaLocation" runat="server" DataField="SchemaLocation" />
            <px:PXFormView RenderStyle="Simple" DataMember="FilterView" runat="server" ID="typeForm">
                <Template>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="RUTROT Work Types" LabelsWidth="SM" ControlSize="XXL"/>
                    <px:PXDropDown ID="edRUTROTType" runat="server" DataField="RUTROTType" CommitChanges="true" />
                </Template>
            </px:PXFormView>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True" AllowSearch="True" SkinID="Details" TabIndex="100" 
        SyncPosition="true" SyncPositionWithGraph="true">
		<Levels>
			<px:PXGridLevel DataMember="WorkTypes">
				<Columns>
					<px:PXGridColumn DataField="RUTROTType" Type="DropDownList" CommitChanges="true" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="XMLTag" />
					<px:PXGridColumn DataField="StartDate"  CommitChanges="true" />
					<px:PXGridColumn DataField="EndDate" CommitChanges="true" />
					<px:PXGridColumn DataField="Position" CommitChanges="true" />
				</Columns>
				<Mode InitNewRow="True" />
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Mode AllowUpload="True" />
        <ActionBar>
            <CustomItems>
                <px:PXToolBarButton ImageKey="ArrowDown"  Tooltip="Move Down">
                    <AutoCallBack Command="MoveDown" Target="ds">
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXToolBarButton>
                 <px:PXToolBarButton ImageKey="ArrowUp"  Tooltip="Move Up">
                    <AutoCallBack Command="MoveUp" Target="ds">
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
	</px:PXGrid>
</asp:Content>
