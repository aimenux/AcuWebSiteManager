<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM205510.aspx.cs" Inherits="Pages_SM205510" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Audit" TypeName="PX.SM.AUAuditMaintenance">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Refresh" Visible="false" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" PopupVisible="False" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" PopupVisible="False" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Cancel" PostData="Self" CommitChanges="True" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Delete" PostData="Self" CommitChanges="True" PopupVisible="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Audit" Caption="Screen Selection">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="M" LabelsWidth="S" StartColumn="True" />
            <px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID"  DisplayMode="Text" FilterByAllFields="true" />
            
						<px:PXTextEdit ID="edScreenIDCopy" runat="server" DataField="VirtualScreenID" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="M" LabelsWidth="S"
                StartColumn="True" />
            <px:PXDropDown ID="edShowFieldsType" runat="server" DataField="ShowFieldsType" CommitChanges="True">
                <AutoCallBack Enabled="true" ActiveBehavior="true">
                    <Behavior RepaintControlsIDs="grid" />
                </AutoCallBack>
            </px:PXDropDown>
            <px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" CommitChanges="true" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXSplitContainer runat="server" ID="sp1" SkinID="Horizontal" SplitterPosition="300">
        <AutoSize Enabled="True" Container="Window" />
        <Template1>
            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
                SkinID="Inquire" SyncPosition="true" AdjustPageSize="Auto" AllowPaging="True" Caption="Tables" CaptionVisible="true">
                <Levels>
                    <px:PXGridLevel DataMember="Tables">
										<Mode AllowAddNew="False" />
                        <Columns>
                            <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" />
                            <px:PXGridColumn DataField="TableName" Width="250px" />
                            <px:PXGridColumn DataField="TableDisplayName" Width="250px" />
                            <px:PXGridColumn DataField="ShowFieldsType" Width="100px" AutoCallBack="true" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Enabled="True" Container="Parent" />
                <AutoCallBack Command="Refresh" Target="grid2">
                </AutoCallBack>
            </px:PXGrid>
        </Template1>
        <Template2>
            <px:PXGrid ID="grid2" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="260px"
                SkinID="Inquire" AdjustPageSize="Auto" AllowPaging="True" Caption="Fields" CaptionVisible="true">
                <Levels>
                    <px:PXGridLevel DataMember="Fields">
										<Mode AllowAddNew="False" />
                        <Columns>
                            <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" />
                            <px:PXGridColumn DataField="FieldName" Width="250px" />
                            <px:PXGridColumn DataField="FieldDisplayName" Width="250px" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Enabled="True" Container="Parent" MinHeight="150" />
            </px:PXGrid>
        </Template2>
    </px:PXSplitContainer>
</asp:Content>
