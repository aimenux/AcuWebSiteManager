<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN209500.aspx.cs"
    Inherits="Page_IN209500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INKitSpecMaint" PrimaryView="Hdr">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="frmHeader" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Hdr" Caption="Kit Specification Summary"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edKitInventoryID"
        TabIndex="2500">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSegmentMask  ID="edKitInventoryID" runat="server" DataField="KitInventoryID" AutoRefresh="True" AllowEdit="True">
                <AutoCallBack Command="Cancel" Target="ds" />
                <GridProperties>
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSegmentMask>
            <px:PXCheckBox ID="chkIsNonStock" runat="server" DataField="IsNonStock" Enabled="False" />
            <px:PXSelector ID="edRevisionID" runat="server" DataField="RevisionID" AutoRefresh="True">
                <AutoCallBack Command="Cancel" Target="ds" />
            </px:PXSelector>
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="XS" StartColumn="True" />
            <px:PXSegmentMask ID="edKitSubItemID" runat="server" DataField="KitSubItemID" />
            <px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
            <px:PXCheckBox ID="chkAllowCompAddition" runat="server" DataField="AllowCompAddition" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="216px" Style="z-index: 100;" Width="100%" TabIndex="23">
        <Items>
            <px:PXTabItem Text="Stock Components">
                <Template>
                    <px:PXGrid ID="gridStock" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
                        TabIndex="200" SkinID="Details" BorderStyle="None" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="StockDet">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="SM" />
                                    <px:PXSegmentMask ID="edSCompInventoryID" runat="server" DataField="CompInventoryID" Width="81px" AllowAddNew="True" AllowEdit="True" AutoRefresh="True">
                                        <Items>
                                            <px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" TextCase="Upper" />
                                        </Items>
                                        <GridProperties FastFilterFields="Descr">
                                            <PagerSettings Mode="NextPrevFirstLast" />
                                            <PagerSettings Mode="NextPrevFirstLast" />
                                        </GridProperties>
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSCompSubItemID" runat="server" DataField="CompSubItemID" Width="45px" AutoRefresh="true">
                                        <Items>
                                            <px:PXMaskItem EditMask="AlphaNumeric" EmptyChar="_" Length="2" Separator="-" TextCase="Upper" />
                                            <px:PXMaskItem EditMask="AlphaNumeric" Length="1" Separator="-" TextCase="Upper" />
                                        </Items>
                                        <GridProperties FastFilterFields="Descr">
                                            <PagerSettings Mode="NextPrevFirstLast" />
                                        </GridProperties>
                                        <Parameters>
                                            <px:PXControlParam ControlID="gridStock" Name="INKitSpecStkDet.compInventoryID" PropertyName="DataValues[&quot;CompInventoryID&quot;]"
                                                Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXNumberEdit ID="edSDfltCompQty" runat="server" AllowNull="False" DataField="DfltCompQty" Decimals="6" ValueType="Decimal"
                                        Width="108px" />
                                    <px:PXNumberEdit ID="edSMinCompQty" runat="server" AllowNull="True" DataField="MinCompQty" Decimals="6" ValueType="Decimal"
                                        Width="108px" />
                                    <px:PXNumberEdit ID="edSMaxCompQty" runat="server" AllowNull="True" DataField="MaxCompQty" Decimals="6" ValueType="Decimal"
                                        Width="108px" />
                                    <px:PXNumberEdit ID="edSDisassemblyCoeff" runat="server" AllowNull="True" DataField="DisassemblyCoeff" Decimals="6" ValueType="Decimal"
                                        Width="108px" />
                                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" Width="63px" AllowEdit="True" AutoRefresh="True" >
                                        <GridProperties>
                                            <PagerSettings Mode="NextPrevFirstLast" />
                                        </GridProperties>
                                    </px:PXSelector>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="CompInventoryID" AllowNull="False" DisplayFormat="&gt;AAAAAAAAAAAAAAAAAAAAAAAAA" Width="81px"
                                        AutoCallBack="True" />
									<px:PXGridColumn DataField="CompInventoryID_InventoryItem_Descr" />
                                    <px:PXGridColumn DataField="CompSubItemID" AllowNull="False" DisplayFormat="&gt;AA-A-&gt;AA" Width="45px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="63px" CommitChanges="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="DfltCompQty" DataType="Decimal" Decimals="3" DefValueText="0.0" TextAlign="Right"
                                        Width="108px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="AllowQtyVariation" TextAlign="Center" DataType="Boolean" Type="CheckBox" DefValueText="False"
                                        Width="108px" />
                                    <px:PXGridColumn AllowNull="True" DataField="MinCompQty" DataType="Decimal" Decimals="3" DefValueText="0.0" TextAlign="Right"
                                        Width="108px" />
                                    <px:PXGridColumn AllowNull="True" DataField="MaxCompQty" DataType="Decimal" Decimals="3" DefValueText="0.0" TextAlign="Right"
                                        Width="108px" />
                                    <px:PXGridColumn AllowNull="False" DataField="DisassemblyCoeff" DataType="Decimal" Decimals="6" DefValueText="1.0" TextAlign="Right"
                                        Width="100px" />
                                    <px:PXGridColumn DataField="AllowSubstitution" TextAlign="Center" DataType="Boolean" Type="CheckBox" DefValueText="False"
                                        Width="108px" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Non Stock Components">
                <Template>
                    <px:PXGrid ID="gridNonStock" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
                        TabIndex="200" SkinID="Details" BorderStyle="None" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="NonStockDet">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edNSCompInventoryID" runat="server" DataField="CompInventoryID" Width="81px" AllowAddNew="True" AllowEdit="True">
                                        <Items>
                                            <px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" TextCase="Upper" />
                                        </Items>
                                        <GridProperties FastFilterFields="Descr">
                                            <PagerSettings Mode="NextPrevFirstLast" />
                                        </GridProperties>
                                    </px:PXSegmentMask>
                                    <px:PXNumberEdit ID="edNSDfltCompQty" runat="server" AllowNull="False" DataField="DfltCompQty" Decimals="6" ValueType="Decimal" Width="108px"/>
                                    <px:PXNumberEdit ID="edNSMinCompQty" runat="server" AllowNull="True" DataField="MinCompQty" Decimals="6" ValueType="Decimal" Width="108px"/>
                                    <px:PXNumberEdit ID="edNSMaxCompQty" runat="server" AllowNull="True" DataField="MaxCompQty" Decimals="6" ValueType="Decimal" Width="108px"/>
                                    <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" Width="63px" AllowEdit="True" AutoRefresh="True" >
                                        <GridProperties>
                                            <PagerSettings Mode="NextPrevFirstLast" />
                                        </GridProperties>
                                    </px:PXSelector>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="CompInventoryID" AllowNull="False" DisplayFormat="&gt;AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" Width="81px"
                                        AutoCallBack="True"/>
									<px:PXGridColumn DataField="CompInventoryID_InventoryItem_Descr" />
                                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="63px" CommitChanges="True"/>
                                    <px:PXGridColumn AllowNull="False" DataField="DfltCompQty" DataType="Decimal" Decimals="6" DefValueText="0.0" TextAlign="Right"
                                        Width="108px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="AllowQtyVariation" TextAlign="Center" DataType="Boolean" Type="CheckBox" Width="108px"/>
                                    <px:PXGridColumn AllowNull="True" DataField="MinCompQty" DataType="Decimal" Decimals="6" DefValueText="0.0" TextAlign="Right"
                                        Width="108px"/>
                                    <px:PXGridColumn AllowNull="True" DataField="MaxCompQty" DataType="Decimal" Decimals="6" DefValueText="0.0" TextAlign="Right"
                                        Width="108px"/>
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
</asp:Content>
