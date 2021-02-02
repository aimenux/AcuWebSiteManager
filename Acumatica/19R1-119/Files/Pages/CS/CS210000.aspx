<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS210000.aspx.cs" Inherits="Page_CS210000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" PrimaryView="deferredcode" TypeName="PX.Objects.CS.DeferredCodeMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="500px" Style="z-index: 100" Width="100%"  DataMember="deferredcode" Caption="Deferred Code">
		
		<Template>
			<px:PXLabel ID="lblDeferredCodeID" runat="server" Style="z-index: 100; left: 9px; position: absolute;
				top: 9px">DeferredCode ID :</px:PXLabel>
			<px:PXSelector ID="edDeferredCodeID" runat="server" AllowNull="True" DataField="DeferredCodeID"
				   LabelID="lblDeferredCodeID"
				 Style="z-index: 101; left: 162px; position: absolute; top: 9px"
				TabIndex="1" Width="108px">
				<AutoCallBack Command="Cancel" Enabled="True" Target="ds">
				</AutoCallBack>
				<GridProperties>
					<Columns>
						<px:PXGridColumn DataField="DeferredCodeID"  Width="70px">
							<Header Text="DeferredCode ID">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowNull="False" DataField="Descr"  Width="200px">
							<Header Text="Description">
							</Header>
						</px:PXGridColumn>
					</Columns>
					<Layout ColumnsMenu="False" />
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
			</px:PXSelector>
			<px:PXLabel ID="lblDescr" runat="server" Style="z-index: 102; left: 9px; position: absolute;
				top: 36px">Description :</px:PXLabel>
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" LabelID="lblDescr"
				 Style="z-index: 103; left: 162px; position: absolute; top: 36px"
				TabIndex="2" Width="297px">
			</px:PXTextEdit>
			<px:PXLabel ID="lblDeferPeriods" runat="server" Style="z-index: 102; left: 9px; position: absolute;
				 top: 63px">Defer Over Periods :</px:PXLabel>
			<px:PXNumberEdit ID="edDeferPeriods" runat="server" DataField="DeferPeriods" LabelID="lblDeferPeriods"
				  Style="z-index: 103; left: 162px; position: absolute; top: 63px"
				 TabIndex="3" ValueType="Int16" Width="54px">
			</px:PXNumberEdit>
			<px:PXLabel ID="lblReconPeriods" runat="server" Style="z-index: 102; left: 9px; position: absolute;
				 top: 90px">Recognize Now Periods :</px:PXLabel>
			<px:PXNumberEdit ID="edReconPeriods" runat="server" DataField="ReconPeriods" LabelID="lblReconPeriods"
				  Style="z-index: 103; left: 162px; position: absolute; top: 90px"
				 TabIndex="4" ValueType="Int16" Width="54px">
			</px:PXNumberEdit>
			<px:PXGroupBox ID="gbPeriodically" runat="server" Caption="Schedule Options" 
                DataField="PeriodDateSel" Height="117px" Style="z-index: 117;
				left: 9px; position: absolute; top: 117px; width: 450px;" ValueType="String">
				<Template>
					<px:PXRadioButton ID="rbStartOfPeriod" runat="server" Style="z-index: 100; left: 144px;
						position: absolute; top: 36px" Value="S" Text="Start of Financial Period" Width="221px" 
                        TabIndex="6" />
					<px:PXRadioButton ID="rbEndOfPeriod" runat="server" Style="z-index: 101; left: 144px;
						position: absolute; top: 63px" Value="E" Text="End of Financial Period" Width="221px" 
                        TabIndex="7"/>
					<px:PXRadioButton ID="rbFixedDay" runat="server" Style="z-index: 102; left: 144px;
						position: absolute; top: 90px; width: 221px;" Value="D" Text="Fixed Day of the Period" 
                        TabIndex="8"/>
				<px:PXLabel ID="lblPeriodFrequency" runat="server" Style="z-index: 103; left: 9px;
					position: absolute; top: 9px">Every period(s) :</px:PXLabel>
				<px:PXNumberEdit ID="edPeriodFrequency" runat="server" AllowNull="True" DataField="PeriodFrequency"
					LabelID="lblPeriodFrequency"   Style="z-index: 104;
					left: 144px; position: absolute; top: 9px" TabIndex="5" ValueType="Int16" Width="45px">
				</px:PXNumberEdit>
					<px:PXNumberEdit ID="edPeriodFixedDay" runat="server" AllowNull="True" 
                        DataField="PeriodFixedDay"   Style="z-index: 105;
						left: 368px; position: absolute; top: 90px" TabIndex="9" ValueType="Int16" Width="45px">
					</px:PXNumberEdit>
					<px:PXLabel ID="PXLabel1" runat="server" Style="z-index: 107; left: 9px; position: absolute;
						top: 36px">Document Date Selection</px:PXLabel>
				</Template>
				<AutoCallBack Command="Save" Enabled="True" Target="form">
				</AutoCallBack>
			</px:PXGroupBox>
		 <px:PXLabel ID="lblAccountType" runat="server" Style="z-index: 100; left: 9px; position: absolute;
			 top: 270px">Code Type :</px:PXLabel>
		 <px:PXDropDown ID="edAccountType" runat="server" DataField="AccountType" LabelID="lblAccountType"
			 Style="z-index: 101; left: 162px; position: absolute; top: 270px" TabIndex="9"
			 Width="108px">
			 
		 </px:PXDropDown>
			<px:PXLabel ID="lblAccountIDH" runat="server" Style="z-index: 100; left: 279px;
				position: absolute; top: 297px"></px:PXLabel>
		  <px:PXLabel ID="lblAccountID" runat="server" Style="z-index: 100; left: 9px;
				position: absolute; top: 297px">Account :</px:PXLabel>
		  <px:PXSegmentMask ID="edAccountID" runat="server" AllowNull="True" DataField="AccountID"
				   HintField="description"
				LabelID="lblAccountID" Style="z-index: 101; left: 162px; position: absolute;
				top: 297px" TabIndex="10" Width="108px" HintLabelID="lblAccountIDH" AutoGenerateColumns="true">
				<AutoCallBack Command="Save" Target="form" Enabled="true"/>
				<GridProperties FastFilterFields="Description">
					 
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
		  </px:PXSegmentMask>
		  <px:PXLabel ID="lblSubIDH" runat="server" Style="z-index: 100; left: 279px;
				position: absolute; top: 324px"></px:PXLabel>
		  <px:PXLabel ID="lblSubID" runat="server" Style="z-index: 102; left: 9px;
				position: absolute; top: 324px">Sub :</px:PXLabel>
		 <px:PXSegmentMask ID="edSubID" runat="server" AllowNull="True" DataField="SubID"
			  
			  HintField="description" LabelID="lblSubID" Style="z-index: 103;
			 left: 162px; position: absolute; top: 324px" TabIndex="11" Width="108px" HintLabelID="lblSubIDH" AutoRefresh="true">
			 <GridProperties FastFilterFields="Description">
				 
			 <PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
			 <Parameters>
				 <px:PXControlParam Name="DeferredCode.accountID"  ControlID="form" PropertyName="DataControls[&quot;edAccountID&quot;].Value"/>
			 </Parameters>
		 </px:PXSegmentMask>
		</Template>
		<AutoSize Enabled="True" Container="Window" />
	</px:PXFormView>
</asp:Content>

