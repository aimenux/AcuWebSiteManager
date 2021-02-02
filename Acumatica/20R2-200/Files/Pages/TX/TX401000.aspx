<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX401000.aspx.cs" Inherits="Page_TX401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Objects.TX.TaxExplorer" PrimaryView="Filter"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Filter" Caption="Selection">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector CommitChanges="True" ID="edState" runat="server" DataField="State" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="PXTab1" runat="server" Height="300px" Width="100%">
		<Items>
			<px:PXTabItem Text="Taxes">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="DetailsInTab" AdjustPageSize="Auto" AllowPaging="True">
						<Levels>
							<px:PXGridLevel DataMember="TaxRecords">
								<Columns>
									<px:PXGridColumn DataField="TaxID" />
									<px:PXGridColumn DataField="Description" />
									<px:PXGridColumn DataField="Rate" TextAlign="Right" />
									<px:PXGridColumn DataField="EffectiveDate" />
									<px:PXGridColumn DataField="PreviousRate" TextAlign="Right" />
									<px:PXGridColumn DataField="IsTaxable" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="IsFreight" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="IsService" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="IsLabor" TextAlign="Center" Type="CheckBox" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					    <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Zones">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="500px">
                        <AutoSize Enabled="true" />
                        <Template1>
                            <px:PXGrid ID="gridZone" runat="server" DataSourceID="ds" Width="100%" Height="100px" SkinID="DetailsInTab" AdjustPageSize="Auto"
                                AllowPaging="True">
                                <Levels>
                                    <px:PXGridLevel DataMember="ZoneRecords">
                                        <Columns>
                                            <px:PXGridColumn DataField="ZoneID" />
                                            <px:PXGridColumn DataField="Description" />
                                            <px:PXGridColumn DataField="CombinedRate" TextAlign="Right" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoCallBack Command="Refresh" Target="gridZoneDetail" />
                                <AutoSize Enabled="True" MinHeight="150" />
                                <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridZoneDetail" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Inquire" Caption="Taxes Applicable"
                                BorderWidth="0px" AdjustPageSize="Auto" AllowPaging="True" RestrictFields="True">
                                <Levels>
                                    <px:PXGridLevel DataMember="ZoneDetailRecords">
                                        <Columns>
                                            <px:PXGridColumn DataField="ZoneID" />
                                            <px:PXGridColumn DataField="TaxID" />
                                            <px:PXGridColumn DataField="TaxRecord__Description" />
                                            <px:PXGridColumn DataField="TaxRecord__Rate" TextAlign="Right" />
                                            <px:PXGridColumn DataField="TaxRecord__EffectiveDate" />
                                            <px:PXGridColumn DataField="TaxRecord__PreviousRate" TextAlign="Right" />
                                            <px:PXGridColumn DataField="TaxRecord__IsTaxable" TextAlign="Center" Type="CheckBox" />
                                            <px:PXGridColumn DataField="TaxRecord__IsFreight" TextAlign="Center" Type="CheckBox" />
                                            <px:PXGridColumn DataField="TaxRecord__IsService" TextAlign="Center" Type="CheckBox" />
                                            <px:PXGridColumn DataField="TaxRecord__IsLabor" TextAlign="Center" Type="CheckBox" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" MinHeight="150" />
                                <Parameters>
                                    <px:PXControlParam ControlID="gridZoneDetail" Name="ZoneID" PropertyName="DataKey[&quot;ZoneID&quot;]" Type="String" />
                                </Parameters>
                                <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="210" />
	</px:PXTab>
</asp:Content>
