<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX205100.aspx.cs" Inherits="Page_TX205100" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" TypeName="PX.Objects.TX.TaxReportMaint" PrimaryView="TaxVendor">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />            
			<px:PXDSCallbackCommand Visible="false" Name="ViewGroupDetails" DependOnGrid="grid2" />

            <px:PXDSCallbackCommand DependOnGrid="grid1" Name="Up" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid1" Name="Down" Visible="False" />	
            
            <%-- PXOrderSelect commands--%> 
            <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="grid1" />
            <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="grid1" />
            <%--PXOrderSelect commands --%>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="TaxVendor" Caption="Tax Agency Settings" NoteIndicator="False" FilesIndicator="False" >
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
			<px:PXSegmentMask CommitChanges="True" ID="edBAccountID" runat="server" DataField="BAccountID" AllowEdit="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowNoTemp" runat="server" DataField="ShowNoTemp" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="180px" Width="100%">
		<Items>
			<px:PXTabItem Text="Report Lines">
				<Template>
					<px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="150px" Width="100%"  SkinID="DetailsInTab" SyncPosition="true" KeepPosition="true">
						<Mode InitNewRow="True" AllowDragRows="True" />
						<ActionBar>
                            <CustomItems>                               
								<px:PXToolBarButton CommandName="Up" CommandSourceID="ds">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandName="Down" CommandSourceID="ds">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<CallbackCommands PasteCommand="PasteLine">
							<Save PostData="Page" />
						</CallbackCommands>
						<Levels>
							<px:PXGridLevel DataMember="ReportLine">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr"/>
                                    <px:PXNumberEdit ID="edSortOrder" runat="server" DataField="SortOrder"/>
									<px:PXCheckBox ID="chkNetTax" runat="server" DataField="NetTax" />
									<px:PXCheckBox ID="chkHideReportLine" runat="server" DataField="HideReportLine" />
									<px:PXCheckBox ID="chkTempLine" runat="server" DataField="TempLine" />
									<px:PXTextEdit ID="edLineDescr" runat="server" DataField="Descr" />
									<px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" AllowEdit="True" />
								</RowTemplate>
								<Columns>
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right"/>
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right"/>
									<px:PXGridColumn DataField="Descr" AllowDragDrop="true"/>
									<px:PXGridColumn DataField="LineType" Type="DropDownList"/>
									<px:PXGridColumn DataField="LineMult" Type="DropDownList" />
									<px:PXGridColumn DataField="TaxZoneID" AllowShowHide="False"/>
									<px:PXGridColumn DataField="TempLine" TextAlign="Center" AutoCallBack="true" Type="CheckBox"/>
									<px:PXGridColumn DataField="NetTax" TextAlign="Center" AutoCallBack="true" Type="CheckBox"/>
									<px:PXGridColumn DataField="HideReportLine" TextAlign="Center" AutoCallBack="true" Type="CheckBox"/>
									<px:PXGridColumn DataField="ReportLineNbr" Label="Tax Box Number"/>
                                    <px:PXGridColumn DataField="BucketSum"/>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150"/>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Reporting Groups">
				<Template>
					<px:PXGrid ID="grid2" runat="server" DataSourceID="ds" Height="150px" Width="100%" SkinID="DetailsInTab">
						<Mode InitNewRow="True" />
						<ActionBar>
							<CustomItems>
							    <px:PXToolBarButton Text="Group Details" CommandSourceID="ds" CommandName="ViewGroupDetails"/>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Bucket">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXTextEdit ID="edBucketDescr" runat="server" DataField="Name" />
									</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Name" />
									<px:PXGridColumn DataField="BucketType" Type="DropDownList" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="True" Container="Window" MinHeight="150" />
	</px:PXTab>
</asp:Content>
