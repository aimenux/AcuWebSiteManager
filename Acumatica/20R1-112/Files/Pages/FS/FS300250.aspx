<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS300250.aspx.cs" Inherits="Page_FS300250" Title="Untitled Page" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" PrimaryView="Filter" TypeName="PX.Objects.FS.AppCompleteServiceEntry" SuspendUnloading="False">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" Visible="False" />
            <px:PXDSCallbackCommand Name="Insert" Visible="False" />
            <px:PXDSCallbackCommand Name="Delete" Visible="False" />
            <px:PXDSCallbackCommand Name="First" Visible="False" />
            <px:PXDSCallbackCommand Name="Last" Visible="False" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
            <px:PXDSCallbackCommand Name="Previous" Visible="False" />
            <px:PXDSCallbackCommand Name="Next" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" AllowAutoHide="false" AllowCollapse="True">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" ControlSize="S" LabelsWidth="S" />
            <px:PXSelector ID="edSrvOrdType" runat="server" DataField="SrvOrdType" DataSourceID="ds" AllowEdit="True" />
            <px:PXSelector ID="edAppointmentID" runat="server" DataField="AppointmentID" DataSourceID="ds" AutoRefresh="True" AllowEdit="True" CommitChanges="True" />
            <px:PXSelector ID="edSOID" runat="server" DataField="SOID" DataSourceID="ds" AutoRefresh="True" AllowEdit="True" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="S" LabelsWidth="S" />
            <px:PXDropDown ID="edActionLog" runat="server" DataField="Action" CommitChanges="True"/>
            <px:PXDropDown ID="edTypeLog" runat="server" DataField="Type" CommitChanges="True"/>
            <px:PXCheckBox ID="edMeLogAction" runat="server" CommitChanges="True" DataField="Me" />
            <px:PXDateTimeEdit ID="edLogTime" runat="server" CommitChanges="True" DataField="LogTime_Time" TimeMode="True"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100;" Width="100%" DataSourceID="ds" DataMember="Filter">
        <Items>
            <px:PXTabItem Text="Selection">
                <Template>
                  <px:PXGrid ID="PXLogTravelLinesGrid" runat="server" DataSourceID="ds" Style="z-index: 100" SkinID="Inquire" TabIndex="800" SyncPosition="True" AllowPaging="True" AdjustPageSize="Auto" Width="100%" FilesIndicator="False" NoteIndicator ="False">
                    <Levels>
                        <px:PXGridLevel DataMember="LogActionServiceDetails" DataKeyNames="LineRef">
                            <RowTemplate>
                                <px:PXSelector ID="edBAccountID" runat="server" DataField="BAccountID"/>
                            </RowTemplate>
                            <Columns>
                                <px:PXGridColumn DataField="Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
                                <px:PXGridColumn DataField="BAccountID" DisplayMode="Hint"/>
                                <px:PXGridColumn DataField="LineRef"/>
                                <px:PXGridColumn DataField="Travel" TextAlign="Center" Type="CheckBox"/>
                                <px:PXGridColumn DataField="InventoryID"/>
                                <px:PXGridColumn DataField="Descr"/>
                                <px:PXGridColumn DataField="DateTimeBegin_Time" CommitChanges="True"/>
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Enabled="True" />
                    <Mode AllowAddNew="False" AllowDelete="False" />
                  </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>