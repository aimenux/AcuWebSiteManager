<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA205000.aspx.cs"
    Inherits="Page_FA205000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.FA.BookMaint" PrimaryView="Book" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="showCalendar" DependOnGrid="grid" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Width="100%" Style="z-index: 100; left: 0px; top: 0px; height: 286px;" AllowPaging="True"
        DataSourceID="ds" AdjustPageSize="Auto" AllowSearch="True" SkinID="Primary">
        <Levels>
            <px:PXGridLevel DataMember="Book">
                <Mode InitNewRow="True" />
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXMaskEdit ID="edBookCode" runat="server" DataField="BookCode" InputMask="&gt;CCCCCCCCCC" />
                    <px:PXCheckBox CommitChanges="True" ID="chkUpdateGL" runat="server" DataField="UpdateGL" />
                    <px:PXDropDown ID="edMidMonthType" runat="server" AllowNull="False" DataField="MidMonthType" />
                    <px:PXNumberEdit ID="edMidMonthDay" runat="server" DataField="MidMonthDay" />
                    <px:PXTextEdit ID="edFirstCalendarYear" runat="server" DataField="FirstCalendarYear" Enabled="False" />
                    <px:PXTextEdit ID="edLastCalendarYear" runat="server" DataField="LastCalendarYear" Enabled="False" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="BookCode" DisplayFormat="&gt;CCCCCCCCCC" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn AllowNull="False" AutoCallBack="True" DataField="UpdateGL" Label="Update GL" TextAlign="Center" Type="CheckBox"
                        Width="60px" />
                    <px:PXGridColumn AllowNull="False" DataField="MidMonthType" Label="Mid-Month Type" RenderEditorText="True" />
                    <px:PXGridColumn AllowNull="False" DataField="MidMonthDay" Label="Mid-Month Day" TextAlign="Right" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Mode AllowFormEdit="True" InitNewRow="True" />
        <AutoCallBack/>
        <LevelStyles>
            <RowForm Height="150px" Width="270px"/>
        </LevelStyles>
        <ActionBar>
            <Actions>
                <NoteShow Enabled="False" />
            </Actions>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
