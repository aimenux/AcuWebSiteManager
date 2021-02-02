<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA204000.aspx.cs"
    Inherits="Page_FA204000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.FA.UsageScheduleMaint" PrimaryView="UsageSchedule" Visible="True">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" DataSourceID="ds"
        AdjustPageSize="Auto" AllowSearch="True" SkinID="Primary">
        <Levels>
            <px:PXGridLevel DataMember="UsageSchedule">
                <Mode InitNewRow="True" />
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                    <px:PXMaskEdit ID="edScheduleCD" runat="server" DataField="ScheduleCD" InputMask="&gt;aaaaaaaaaa" />
                    <px:PXSelector ID="edUsageUOM" runat="server" DataField="UsageUOM" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXNumberEdit Size="s" ID="edReadUsageEveryValue" runat="server" DataField="ReadUsageEveryValue" />
                    <px:PXDropDown Size="xs" ID="edReadUsageEveryPeriod" runat="server" AllowNull="False" DataField="ReadUsageEveryPeriod" SelectedIndex="2"
                        SuppressLabel="True" />
                    <px:PXLayoutRule runat="server" Merge="False" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Size="XL" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="ScheduleCD" DisplayFormat="&gt;aaaaaaaaaa" />
                    <px:PXGridColumn DataField="UsageUOM" DisplayFormat="&gt;aaaaaa" />
                    <px:PXGridColumn DataField="ReadUsageEveryValue" TextAlign="Right" AllowNull="False" Label="Read Every" />
                    <px:PXGridColumn AllowNull="False" DataField="ReadUsageEveryPeriod" RenderEditorText="True" Label="Period" />
                    <px:PXGridColumn DataField="Description" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Mode AllowFormEdit="True" />
    </px:PXGrid>
</asp:Content>
