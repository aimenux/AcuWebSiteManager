<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AP204000.aspx.cs" Inherits="Page_AP204000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AP.APDiscountMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
            <px:PXDSCallbackCommand Name="Delete" Visible="false" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ViewAPDiscountSequence" DependOnGrid="grid" Visible="False"/>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="Filter" TabIndex="11700" NoteIndicator="False" FilesIndicator="False" ActivityIndicator="False">
        <Template>
            <px:PXLayoutRule runat="server" StartGroup="True" LabelsWidth="S" ControlSize="M">
            </px:PXLayoutRule>
            <px:PXSegmentMask runat="server" DataField="AcctCD" ID="edAcctCD" CommitChanges="True">
            </px:PXSegmentMask>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataMember="CurrentVendor" DataSourceID="ds">
        <Items>
            <px:PXTabItem Text="Discount Codes">
                <Template>
                    <px:PXGrid ID="grid" runat="server" Height="144px" Width="100%" Style="z-index: 100" AllowPaging="True" ActionsPosition="Top"
                        AutoAdjustColumns="True" AllowSearch="True" DataSourceID="ds" SkinID="DetailsInTab" MatrixMode="True" AdjustPageSize="Auto" TabIndex="1900">
                        <Mode InitNewRow="True"/>
                        <Levels>
                            <px:PXGridLevel DataMember="CurrentDiscounts">
                                <Columns>
                                    <px:PXGridColumn DataField="DiscountID" DisplayFormat="&gt;aaaaaaaaaa" LinkCommand="ViewAPDiscountSequence" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn AllowNull="False" DataField="Type" RenderEditorText="True" AutoCallBack="true" />
                                    <px:PXGridColumn AllowNull="False" DataField="ApplicableTo" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="IsManual" TextAlign="Center" Type="CheckBox" Width="60px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ExcludeFromDiscountableAmt" TextAlign="Center" Type="CheckBox" Width="60px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SkipDocumentDiscounts" TextAlign="Center" Type="CheckBox" Width="60px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn AllowNull="False" DataField="IsAutoNumber" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="LastNumber" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Calculation Settings">
                <Template>
                    <px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="SM" StartColumn="True">
                    </px:PXLayoutRule>
                    <px:PXDropDown ID="edLineDiscountTarget" runat="server" DataField="LineDiscountTarget">
                    </px:PXDropDown>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
