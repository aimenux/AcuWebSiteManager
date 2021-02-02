<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM205000.aspx.cs" Inherits="Page_SM205000"
	Title="Automation Step Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUStepMaint"
		PrimaryView="Step">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="MoveUpButton" Visible="False"  />
			<px:PXDSCallbackCommand CommitChanges="True" Name="MoveDownButton" Visible="False" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ReOrderMenu" Visible="False" DependOnGrid="gridActions" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="FieldValues" Visible="False" DependOnGrid="gridActions" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Flow" Visible="False" DependOnGrid="gridActions" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ComboValues" Visible="False" RepaintControls="None" SelectControlsIDs="formStep" DependOnGrid="gridFields" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ReloadActions" Visible="False" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ReloadCombos" Visible="False"
				DependOnGrid="gridFields" />
			<px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewScreen" StartNewGroup="true" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="DeactivateSteps" RepaintControls="All" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="Graphs" TreeKeys="GraphName,IsNamespace" />
		</DataTrees>
		<ClientEvents CommandPerformed="commandResult"/>
	</pxa:AUDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formStep" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" Caption="Automation Step" DataMember="Step" NoteIndicator="True"
		FilesIndicator="True" TemplateContainer="">
        <Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" 
				LabelsWidth="SM" />
            <px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID"  DisplayMode="Text" FilterByAllFields="true" />
			<px:PXSelector ID="edStepID" runat="server" DataField="StepID" 
				AutoRefresh="True" DataSourceID="ds">
				<Parameters>
					<px:PXControlParam Name="AUStep.screenID" ControlID="formStep" PropertyName="DataControls[&quot;edScreenID&quot;].Value"
						Type="String" Size="8" />
				</Parameters>
			</px:PXSelector>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" 
				LabelsWidth="SM">
			</px:PXLayoutRule>
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" 
				DataField="IsActive" AlignLeft="True" SuppressLabel="True" Size="S" />
			<px:PXCheckBox ID="chkIsStart" runat="server" Checked="True" 
				DataField="IsStart" AlignLeft="True" SuppressLabel="True" Size="SM" />
			<px:PXLayoutRule runat="server" />
        </Template>
		<Parameters>
			<px:PXControlParam ControlID="formStep" Name="AUStep.screenID" PropertyName="NewDataKey[&quot;ScreenID&quot;]"
				Type="String" />
		</Parameters>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="168%" Style="z-index: 100" Width="100%"
		DataSourceID="ds" DataMember="CurrentStep">
		<Items>
			<px:PXTabItem Text="Conditions">
			<Template>
				<px:PXGrid ID="gridConditions" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details"
						MatrixMode="true">
					<ActionBar>
					</ActionBar>
					<Levels>
						<px:PXGridLevel DataMember="Filters" >
							<Mode InitNewRow="True" />
							<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="OpenBrackets" Type="DropDownList" Width="100px"
										AutoCallBack="true" />
									<px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" />
									<px:PXGridColumn AllowNull="False" DataField="Condition" Type="DropDownList" />
									<px:PXGridColumn AllowNull="False" DataField="IsRelative" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="Value" Width="200px" />
									<px:PXGridColumn DataField="Value2" Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="CloseBrackets" Type="DropDownList"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="Operator" Type="DropDownList" Width="60px" />
							</Columns>
						</px:PXGridLevel>
					</Levels>
					<AutoSize Enabled="True" MinHeight="150" />
				</px:PXGrid>
			</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Actions">
			<Template>
				<px:PXGrid ID="gridActions" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details"
						MatrixMode="true">
					<ActionBar>
						<CustomItems>
							<px:PXToolBarButton Text="Fill with Values">
							    <AutoCallBack Target="ds" Command="FieldValues" />
							</px:PXToolBarButton>
							<px:PXToolBarButton Text="Reload Actions">
							    <AutoCallBack Target="ds" Command="ReloadActions" />
							</px:PXToolBarButton>
							<px:PXToolBarButton Text="Reorder Actions">
							    <AutoCallBack Enabled="True" Target="ds" Command="ReOrderMenu" />
							</px:PXToolBarButton>
							<px:PXToolBarButton Text="Flow">
							    <AutoCallBack Target="ds" Command="Flow" />
							</px:PXToolBarButton>
						</CustomItems>
					</ActionBar>
					<AutoSize Enabled="True" MinHeight="150" />
					<Levels>
						<px:PXGridLevel DataMember="Buttons" >
							<Mode InitNewRow="True" />
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="L" />
									<px:PXCheckBox ID="edIsActive" runat="server" DataField="IsActive" />
									<px:PXTextEdit ID="edActionName" runat="server" DataField="ActionName" />
									<px:PXCheckBox ID="edIsAutomatic" runat="server" DataField="IsAutomatic" AllowEdit="true" />
									<px:PXCheckBox ID="edIsDefault" runat="server" DataField="IsDefault" />
									<px:PXCheckBox ID="edIsDisabled" runat="server" DataField="IsDisabled" />
									<px:PXCheckBox ID="edBatchMode" runat="server" DataField="BatchMode" />
									<px:PXTextEdit ID="edMenuText" runat="server" DataField="MenuText" />
									<px:PXDropDown ID="PXDropDown1" runat="server" DataField="MenuIcon" />
									<px:PXDropDown ID="edAutoSave" runat="server" DataField="AutoSave" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="ActionName" Width="200px" Type="DropDownList" AutoCallBack="true" />
									<px:PXGridColumn AllowNull="False" DataField="IsAutomatic" AutoCallBack="true" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="IsDefault" AutoCallBack="true" TextAlign="Center"	Type="CheckBox" Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="IsDisabled" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="BatchMode" TextAlign="Center" Type="CheckBox"	Width="60px" />
									<px:PXGridColumn DataField="MenuText" Width="200px" />
									<px:PXGridColumn DataField="MenuIcon" Width="200px" MatrixMode="False" />
									<px:PXGridColumn AllowNull="False" DataField="AutoSave" Type="DropDownList" Width="108px" />
								</Columns>
                            <RowTemplate>
								<px:PXDropDown ID="edMenuIcon" runat="server" DataField="MenuIcon" AllowEdit="true" > </px:PXDropDown>
                             </RowTemplate>
						</px:PXGridLevel>
					</Levels>
				</px:PXGrid>
					<px:PXSmartPanel ID="pnlValues" runat="server" Style="z-index: 108;
						left: 351px; position: absolute; top: 99px" Width="786px" Caption="Fill with Values"
						CaptionVisible="true" LoadOnDemand="true" Key="Buttons" AutoCallBack-Enabled="true"
						AutoCallBack-Target="gridValues" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
						CallBackMode-PostData="Page">
				<div style="padding: 5px">
					<px:PXGrid ID="gridValues" runat="server" DataSourceID="ds" Height="243px" Style="z-index: 100"
						Width="100%" MatrixMode="true">
						<Levels>
							<px:PXGridLevel DataMember="Fills" >
								<Mode InitNewRow="true" />
								<Columns>
											<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
												Width="60px" />
											<px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" />
											<px:PXGridColumn AllowNull="False" DataField="IsRelative" TextAlign="Center" Type="CheckBox"
												Width="60px" AutoCallBack="true" />
											<px:PXGridColumn DataField="Value" Width="200px" />
											<px:PXGridColumn AllowNull="False" DataField="IsDelayed" TextAlign="Center" Type="CheckBox"
												Width="100px" />
											<px:PXGridColumn AllowNull="False" DataField="IgnoreError" TextAlign="Center" Type="CheckBox"
												Width="60px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<CallbackCommands>
							<FetchRow RepaintControls="None" />
						</CallbackCommands>
					</px:PXGrid>
					</div>
					<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
						<px:PXButton ID="btnValues" runat="server" DialogResult="OK" Text="Close" />
					</px:PXPanel>
				</px:PXSmartPanel>
					<px:PXSmartPanel ID="pnlFlow" runat="server" Height="555px" Style="z-index: 108;
						left: 351px; position: absolute; top: 99px" Width="425px" Caption="Flow" CaptionVisible="true"
						DesignView="Content" LoadOnDemand="true" Key="CurrentAction" AutoCallBack-Enabled="true"
						AutoCallBack-Target="formFlow" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
						CallBackMode-PostData="Page">
					<div style="padding: 5px">
							<px:PXFormView ID="formFlow" runat="server" DataSourceID="ds" Style="z-index: 100"
								Width="100%" CaptionVisible="False" DataMember="CurrentAction" TemplateContainer="">
						<ContentStyle BackColor="Transparent" BorderStyle="None" />
						<Template>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" 
										ControlSize="M" />
									<px:PXLayoutRule runat="server" StartGroup="True" 
										GroupCaption="Retry Attempts" />
									<px:PXCheckBox ID="chkIsRetryActive" runat="server" Checked="True" DataField="IsRetryActive" />
                                    <px:PXSelector ID="edRetryScreenID" runat="server" DataField="RetryScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
									<px:PXSelector CommitChanges="True" ID="edRetryStepID" runat="server" DataField="RetryStepID"
										AutoRefresh="True" DataSourceID="ds">
									<Parameters>
											<px:PXControlParam Name="AUStepAction.retryScreenID" ControlID="formFlow" PropertyName="DataControls[&quot;edRetryScreenID&quot;].Value"
												Type="String" Size="8" />
									</Parameters>
								</px:PXSelector>
								<px:PXDropDown ID="cmbRetryActionName" runat="server" 
										DataField="RetryActionName" Size="M" />
									<px:PXNumberEdit ID="edRetryCntr" runat="server" AllowNull="False" 
										DataField="RetryCntr" Size="M" />
									<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="On Success" />
									<px:PXCheckBox ID="chkIsSuccessActive" runat="server" Checked="True" DataField="IsSuccessActive" />
                                    <px:PXSelector ID="edSuccessScreenID" runat="server" DataField="SuccessScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
									<px:PXSelector CommitChanges="True" ID="edSuccessStepID" runat="server" DataField="SuccessStepID"
										AutoRefresh="True" DataSourceID="ds">
									<Parameters>
											<px:PXControlParam Name="AUStepAction.successScreenID" ControlID="formFlow" PropertyName="DataControls[&quot;edSuccessScreenID&quot;].Value"
												Type="String" Size="8" />
									</Parameters>
								</px:PXSelector>
								<px:PXDropDown ID="edSuccessActionName" runat="server" 
										DataField="SuccessActionName" Size="M" />
									<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="On Failure" />
									<px:PXCheckBox ID="chkIsFailActive" runat="server" Checked="True" DataField="IsFailActive" />
                                    <px:PXSelector ID="edFailScreenID" runat="server" DataField="FailScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
									<px:PXSelector CommitChanges="True" ID="edFailStepID" runat="server" DataField="FailStepID"
										AutoRefresh="True" DataSourceID="ds">
									<Parameters>
											<px:PXControlParam Name="AUStepAction.failScreenID" ControlID="formFlow" PropertyName="DataControls[&quot;edFailScreenID&quot;].Value"
												Type="String" Size="8" />
									</Parameters>
								</px:PXSelector>
									<px:PXDropDown ID="cmbFailActionName" runat="server" DataField="FailActionName" 
										Size="M" />
									<px:PXLayoutRule runat="server" StartGroup="True" 
										GroupCaption="Mass Processing" />
                                    <px:PXSelector ID="edProcessingScreenID" runat="server" DataField="ProcessingScreenID"  DisplayMode="Text" FilterByAllFields="true" />
							    <px:PXCheckBox ID="chkSplitByValues" runat="server" DataField="SplitByValues" />
						</Template>
					</px:PXFormView>
					</div>
					<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
						<px:PXButton ID="btnFlow" runat="server" DialogResult="OK" Text="Close" />
					</px:PXPanel>
				</px:PXSmartPanel>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Fields">
			<Template>
				<px:PXGrid ID="gridFields" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details"
						MatrixMode="true">
					<ActionBar>
						<CustomItems>
							<px:PXToolBarButton Text="Combo Box Values">
							    <AutoCallBack Target="ds" Command="ComboValues" />
							</px:PXToolBarButton>
							<px:PXToolBarButton Text="Reload Combo Box Values">
							    <AutoCallBack Target="ds" Command="ReloadCombos" />
							</px:PXToolBarButton>
						</CustomItems>
					</ActionBar>
					<AutoSize Enabled="True" MinHeight="150" />
					<Levels>
						<px:PXGridLevel DataMember="Fields" >
							<Mode InitNewRow="true" />
							<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="TableName" Width="200px" Type="DropDownList" AutoCallBack="true" />
									<px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" />
									<px:PXGridColumn AllowNull="False" DataField="UseSavedState" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="IsDisabled" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="IsInvisible" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="IsRelative" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="MinValue" Width="200px" />
									<px:PXGridColumn DataField="MaxValue" Width="200px" />
									<px:PXGridColumn DataField="DefaultValue" Width="200px" />
									<px:PXGridColumn DataField="InputMask" Width="200px" />
							</Columns>
						</px:PXGridLevel>
					</Levels>
				</px:PXGrid>
				<px:PXSmartPanel ID="pnlCombos" runat="server" Style="z-index: 108;
					left: 351px; position: absolute; top: 99px" Width="430px" Caption="Combo Box Values"
						CaptionVisible="true" LoadOnDemand="true" Key="Fields" AutoCallBack-Enabled="true"
						AutoCallBack-Target="gridCombos" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
						CallBackMode-PostData="Page">
				<div style="padding: 5px">
					<px:PXGrid ID="gridCombos" runat="server" DataSourceID="ds" Height="243px" Style="z-index: 100"
						Width="100%">
						<Levels>
							<px:PXGridLevel  DataMember="Combos">
								<Columns>
											<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
												Width="60px" />
											<px:PXGridColumn AllowNull="False" DataField="IsExplicit" TextAlign="Center" Type="CheckBox"
												Width="60px" />
											<px:PXGridColumn DataField="Value" />
											<px:PXGridColumn DataField="Description" Width="200px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<CallbackCommands>
							<FetchRow RepaintControls="None" />
						</CallbackCommands>
					</px:PXGrid>
					</div>
					<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
						<px:PXButton ID="btnCombos" runat="server" DialogResult="OK" Text="Close" />
					</px:PXPanel>
				</px:PXSmartPanel>
				</Template>
			</px:PXTabItem>
		</Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
    <script language="javascript" type="text/javascript">
    function commandResult(ds, context)
    {
    	if (context.command == "MoveDownButton")
    	{
    		var grid = px_all["ctl00_phG_panelMenu_gridMenu"];
    		var row = grid.activeRow.nextRow();
    		if (row != null) row.activate();
    	}
    	else if (context.command == "MoveUpButton")
    	{
    		var grid = px_all["ctl00_phG_panelMenu_gridMenu"];
    		var row = grid.activeRow.prevRow();
    		if (row != null) row.activate();
    	}
    }
	</script>	
	<px:PXSmartPanel runat="server" ID="panelMenu" Key="MenuButtons" LoadOnDemand="true"
		ShowAfterLoad="false" Width="600px" Height="400px" Caption="Menu Actions" CaptionVisible="true"
		AutoCallBack-Command='Refresh' AutoCallBack-Enabled="True"
		AutoCallBack-Target="gridMenu" DesignView="Content">
	    <px:PXGrid ID="gridMenu" runat="server" DataSourceID="ds" AutoAdjustColumns="true" Width="100%" SkinID="Details"
			AdjustPageSize="Auto" SyncPosition="true">
	        <Mode AllowAddNew="false" AllowUpdate="False" AllowDelete="false" 
                AllowSort="False"/>	        
            <ActionBar>
				<CustomItems>
					<px:PXToolBarButton Text="Combo Box Values" ImageKey="ArrowUp" ImageSet="main">
						<AutoCallBack Enabled="true" Target="ds" Command="MoveUpButton">
						</AutoCallBack>
					</px:PXToolBarButton>
					<px:PXToolBarButton Text="Reload Combo Box Values" ImageKey="ArrowDown" ImageSet="main">
						<AutoCallBack Enabled="True" Target="ds" Command="MoveDownButton">
						</AutoCallBack>
					</px:PXToolBarButton>
				</CustomItems>
			</ActionBar>
		    <Levels>
                <px:PXGridLevel DataKeyNames="ScreenID,ActionName,MenuText" 
                    DataMember="MenuButtons">
                    <Columns>
                        <px:PXGridColumn DataField="MenuText" Width="200px">
                        </px:PXGridColumn>
                        <px:PXGridColumn DataField="MenuIcon" Width="200px">
                        </px:PXGridColumn>
                    </Columns>
                    <Layout FormViewHeight="" />
                </px:PXGridLevel>
            </Levels>
            <Parameters>
               <px:PXControlParam ControlID="gridActions" Name="AUStepAction.actionName" PropertyName="DataValues[&quot;ActionName&quot;]" Type="String" />
            </Parameters>
	        <AutoSize Enabled="true"/>	        
		</px:PXGrid>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Ok" />			
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
