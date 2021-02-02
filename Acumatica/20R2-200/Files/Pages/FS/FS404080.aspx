<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="FS404080.aspx.cs" Title="Untitled Page" Inherits="Page_FS404080" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="RouteAppointmentGPSLocationFilter" TypeName="PX.Objects.FS.RouteAppointmentGPSLocationInq">
				<CallbackCommands>
					<px:PXDSCallbackCommand Name="FilterManually" CommitChanges="True"/>
					<px:PXDSCallbackCommand Name="Report" CommitChanges="True"/>
					<px:PXDSCallbackCommand Name="EditDetail" Visible="False"/>
					<px:PXDSCallbackCommand Name="OpenLocationScreen" Visible="False"/>
				</CallbackCommands>
			</px:PXDataSource>
        </asp:Content>
        <asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	            <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="RouteAppointmentGPSLocationFilter" Width="100%" AllowAutoHide="False">
		        <Template>
		            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
					<px:PXSelector ID="edsRouteID" runat="server" DataField="RouteID" CommitChanges="True">
					</px:PXSelector>
					<px:PXSelector ID="edRouteDocumentID" runat="server" DataField="RouteDocumentID" AutoRefresh="True" CommitChanges="True">
		            </px:PXSelector>
					<px:PXSelector ID="edServiceID" runat="server" DataField="ServiceID" CommitChanges="True">
		            </px:PXSelector>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M">
		            </px:PXLayoutRule>
		            <px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID" CommitChanges="True">
		            </px:PXSelector>
		            <px:PXSelector ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" AutoRefresh="True" CommitChanges="True">
		            </px:PXSelector>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M">
		            </px:PXLayoutRule>
					<px:PXDateTimeEdit ID="edDateFrom" runat="server" CommitChanges="True" DataField="DateFrom">
		            </px:PXDateTimeEdit>
		            <px:PXDateTimeEdit ID="edDateTo" runat="server" CommitChanges="True" DataField="DateTo">
		            </px:PXDateTimeEdit>
		        </Template>
		    </px:PXFormView>
        </asp:Content>
        <asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
			 <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
				 Width="100%" SkinID="Inquire" AllowPaging="True" AdjustPageSize="Auto" SyncPosition="True" KeepPosition="True">
				 <Levels>
					 <px:PXGridLevel DataMember="RouteAppointmentGPSLocationRecords">
						 <RowTemplate>
							<px:PXSelector ID="edRouteID" runat="server" DataField="RouteID" AllowEdit="True">
							</px:PXSelector>
							<px:PXSelector ID="edRouteDocumentID" runat="server" DataField="RouteDocumentID" AllowEdit="True">
							</px:PXSelector>
							<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="True">
							</px:PXSelector>
							<px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True">
							</px:PXSelector>
							<px:PXSelector ID="edLocationID" runat="server" DataField="LocationID" AllowEdit="True">
							</px:PXSelector>
						</RowTemplate>
						<Columns>
							<px:PXGridColumn DataField="RouteID" DisplayMode="Hint"/>
							<px:PXGridColumn DataField="RouteDocumentID"/>
							<px:PXGridColumn DataField="RefNbr" LinkCommand="EditDetail"/>
							<px:PXGridColumn DataField="CustomerID" DisplayMode="Hint"/>
							<px:PXGridColumn DataField="LocationID" DisplayMode="Hint" LinkCommand="OpenLocationScreen"/>
							<px:PXGridColumn DataField="ActualDateTimeBegin" />
							<px:PXGridColumn DataField="Address" />
							<px:PXGridColumn DataField="GPSStartCoordinate" />
							<px:PXGridColumn DataField="GPSStartAddress" />
							<px:PXGridColumn DataField="GPSCompleteCoordinate" />
							<px:PXGridColumn DataField="GPSCompleteAddress" />
						</Columns>
					 </px:PXGridLevel>
				 </Levels>
				 <AutoSize Container="Window" Enabled="True" MinHeight="150" />
			 </px:PXGrid>
        </asp:Content>