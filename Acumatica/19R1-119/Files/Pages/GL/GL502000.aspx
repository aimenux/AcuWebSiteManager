<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="GL502000.aspx.cs" Inherits="Page_GL502000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="BatchList" TypeName="PX.Objects.GL.BatchPost"
        Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Visible="False" Name="BatchList_batchNbr_ViewDetails" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True"
        AdjustPageSize="Auto" AllowSearch="True" BatchUpdate="True" SkinID="PrimaryInquire"
        Caption="Batches" DataSourceID="ds" FastFilterFields="BatchNbr,Description"
        NoteIndicator="True" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="BatchList">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True"
                        AllowSort="False" AllowMove="False" Width="30px" />
                    <px:PXGridColumn DataField="Module" Width="70px" />
                    <px:PXGridColumn DataField="BatchNbr" Width="100px" />
                    <px:PXGridColumn DataField="LedgerID" Width="100px" />
                    <px:PXGridColumn DataField="DateEntered" Width="100px" />
                    <px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="150px" />
                    <px:PXGridColumn DataField="FinPeriodID" Width="100px" />
                    <px:PXGridColumn DataField="ControlTotal" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="Description" Width="200px">
                    </px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <Mode AllowUpdate="False" />
    </px:PXGrid>
</asp:Content>
