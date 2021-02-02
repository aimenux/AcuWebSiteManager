<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="GL307000.aspx.cs" Inherits="Page_GL307000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Filter" 
        TypeName="PX.Objects.GL.GLVoucherBatchEntry" Visible="True" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="EditRecord" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="ViewDocument" Visible="false" DependOnGrid="gridDocuments" />
            <px:PXDSCallbackCommand Name="Cancel" Visible="false" />
            <px:PXDSCallbackCommand Name="Process" Visible="false" />
            <px:PXDSCallbackCommand Name="ProcessAll" Visible="false" />
            <px:PXDSCallbackCommand Name="Schedule" Visible="false" />
            <px:PXDSCallbackCommand Name="Delete" Visible="true" DependOnGrid="grid" StateColumn="NotReleased" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="frmHeader" runat="server" DataSourceID="ds" Width="100%" Caption="Work book"
		DataMember="Filter"  NoteIndicator="True" FilesIndicator="True"
		ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edWorkBookID"
		TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edWorkBookID" runat="server" DataField="WorkBookID" CommitChanges="true" >
			    <GridProperties FastFilterFields="WorkBookID" />
			</px:PXSelector>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="300px" Width="100%" AdjustPageSize="Auto"
        SkinID="Primary" AllowPaging="True" AllowSearch="True" DataSourceID="ds" TabIndex="7600" AutoAdjustColumns="true" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="VoucherBatches">
                <Columns>
                    <px:PXGridColumn DataField="Selected" CommitChanges="true" Type="CheckBox" AllowCheckAll="true" AllowSort="false" AllowResize="false" TextAlign="Center" />
                    <px:PXGridColumn DataField="VoucherBatchNbr" LinkCommand="EditRecord" />
                    <px:PXGridColumn DataField="Descr" />
                    <px:PXGridColumn DataField="DocCount"/>
                    <px:PXGridColumn DataField="Released" Type="CheckBox"  TextAlign="Center" />
                    <px:PXGridColumn DataField="NotReleased" Type="CheckBox" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <Layout FormViewHeight="250px" />
        <AutoCallBack Target="gridDocuments" Command="Refresh" ActiveBehavior="true">
        </AutoCallBack>
        <ActionBar>
            <Actions>
                <FilterBar MenuVisible="true" Enabled="true" />
                <FilterSet MenuVisible="true" Enabled="true" />
                <Delete MenuVisible="false" Enabled="false" />
                <AddNew MenuVisible="false" Enabled="false" />
                <Refresh MenuVisible="false" Enabled="false" />
            </Actions>
        </ActionBar>
        <Mode InitNewRow="true" />
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
    <px:PXGrid ID="gridDocuments" runat="server" Width="100%" AdjustPageSize="Auto" Height="150px"
        SkinID="Details" AllowPaging="True" AllowSearch="True" DataSourceID="ds" TabIndex="9000" AutoAdjustColumns="true" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="VouchersInBatch">
                <Columns>
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewDocument" />
                    <px:PXGridColumn DataField="Module" Type="DropDownList" />
                    <px:PXGridColumn DataField="DocType" Type="DropDownList" MatrixMode="true" DisplayMode="Text" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar DefaultAction="ViewDocument" ActionsVisible="false">
        </ActionBar>
        <AutoSize Enabled="true" />
    </px:PXGrid>
    <px:PXSmartPanel ID="pnlCreateBatch" runat="server" Key="BatchCreation"
        Caption="Create Voucher Batch" AcceptButtonID="btnCreateBatchOk" CaptionVisible="True" DesignView="Content"
        Overflow="Hidden" AutoReload="true" LoadOnDemand="true" HideAfterAction="false" CancelButtonID ="btnCreateBatchCancel">
        <px:PXFormView runat="server" ID="frmCreateBatch" DataMember="BatchCreation" SkinID="Transparent" DefaultControlID="edVoucherBatchNbr" TabIndex="101">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXTextEdit runat="server" DataField="VoucherBatchNbr" ID="edVoucherBatchNbr" CommitChanges="true" TabIndex="102" />
                <px:PXTextEdit runat="server" DataField="BatchDescription" ID="edBatchDescription" CommitChanges="true" TabIndex="103" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="pxPanelBatchCreationBtns" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnCreateBatchOk" runat="server" DialogResult="OK" Text="Create" TabIndex="104"/>
            <px:PXButton ID="btnCreateBatchCancel" runat="server" DialogResult="Cancel" Text="Cancel" TabIndex="105" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
