'use strict';

const boxModeClass = 'mode-box';
const tableModeClass = 'mode-table';
const columnModeClass = 'mode-column';

function RecognizedValueMapper(viewerContainer, fieldBoundFeedbackControl, tableRelatedFeedbackControl, vendorFieldName, dumpTableFeedbackCallback) {
    this.recognizedValues = [];
    this.recognizedTables = [];

    this.selectedControl = null;
    this.selectedCell = null;
    this.selectedTable = null;
    this.selectedColumn = null;
    this.isUserInput = true;

    this.formMappingByField = [];
    this.gridMappingByFieldRow = [];
    this.fieldRowSeparator = '*';
    this.gridDeletedRowRegexp = /^_\d+$/;

    this.viewerContainer = viewerContainer;
    this.detailsGridControl = null;
    this.feedbackCollector = new FeedbackCollector(dumpTableFeedbackCallback, fieldBoundFeedbackControl, tableRelatedFeedbackControl, vendorFieldName);
    this.scroller = new RecognizedValueScroller();

    this._switchToTableMode();
}

RecognizedValueMapper.prototype.clear = function () {
    this.recognizedValues = [];
    this.recognizedTables = [];

    this._clearFormMapping();
    this._clearGridMapping();
    this._switchToTableMode();
}

RecognizedValueMapper.prototype._switchToBoxMode = function () {
    this.viewerContainer.classList.remove(tableModeClass);
    this.viewerContainer.classList.remove(columnModeClass);

    this.viewerContainer.classList.add(boxModeClass);
}

RecognizedValueMapper.prototype._switchToTableMode = function () {
    this.viewerContainer.classList.remove(boxModeClass);
    this.viewerContainer.classList.remove(columnModeClass);

    this.viewerContainer.classList.add(tableModeClass);
}

RecognizedValueMapper.prototype._switchToColumnMode = function () {
    this.viewerContainer.classList.remove(boxModeClass);
    this.viewerContainer.classList.remove(tableModeClass);

    this.viewerContainer.classList.add(columnModeClass);
}

RecognizedValueMapper.prototype._navigateToMappedRect = function (control, getMappingValues) {
    const recognizedValues = getMappingValues(control);
    if (!recognizedValues) {
        return;
    }

    recognizedValues.forEach(function (rv) {
        rv.markAsMapped();
    });

    if (recognizedValues.length > 0) {
        const firstRv = recognizedValues[0];

        this.scroller.scrollToRecognizedValue(this.viewerContainer, firstRv);
    }
}

RecognizedValueMapper.prototype.trackFormControls = function (form) {
    this.feedbackCollector.formViewName = form.dataMember;
    const that = this;

    for (let controlName in form.controls) {
        let c = form.controls[controlName];

        c.events.addEventHandler('focus', function (control) {
            that.handleFormControlFocus(control);
        });

        c.events.addEventHandler('blur', function (control) {
            that.handleFormControlBlur(control);
        })

        c.events.addEventHandler('valueChanged', function (control) {
            that.handleFormControlValueChanged(control);
        });

        this._initFormMappingByControl(c);
    }
}

RecognizedValueMapper.prototype.handleFormControlFocus = function (control) {
    this._clearSelectedCell();
    this._switchToBoxMode();

    this.selectedControl = control;
    this._stopRecognizedTableMapping();

    this._navigateToFormRect(control);
}

RecognizedValueMapper.prototype._navigateToFormRect = function (control) {
    const that = this;
    const getMappingValues = function (control) {
        return that._getFormMappingValuesByControl(control);
    };

    this._navigateToMappedRect(control, getMappingValues);
}

RecognizedValueMapper.prototype.handleFormControlBlur = function (control) {
    this._switchToTableMode();

    this.selectedControl = null;

    const recognizedValues = this._getFormMappingValuesByControl(control);
    if (!recognizedValues) {
        return;
    }

    recognizedValues.forEach(function (rv) {
        rv.markAsNotMapped();
    });
}

RecognizedValueMapper.prototype.handleFormControlValueChanged = function (control) {
    if (this.isUserInput === false) {
        return;
    }

    const recognizedValues = this._getFormMappingValuesByControl(control);
    if (!recognizedValues) {
        return;
    }

    this._correctFormMapping(control, null, false);
}

RecognizedValueMapper.prototype._getFieldFromRecognizedValue = function (recognizedValue) {
    const fieldName = recognizedValue.fieldName;

    const indexOfDot = fieldName.indexOf('.');
    if (indexOfDot == -1) {
        return fieldName;
    }

    const length = fieldName.length;
    if (indexOfDot + 1 === length) {
        return fieldName;
    }

    return fieldName.slice(indexOfDot + 1);
}

RecognizedValueMapper.prototype._getFieldRow = function (fieldName, rowIndex) {
    return fieldName + this.fieldRowSeparator + rowIndex;
}

RecognizedValueMapper.prototype._getFieldRowInfo = function (fieldRow) {
    const info = fieldRow.split(this.fieldRowSeparator);

    return {
        fieldName: info[0],
        rowIndex: info[1]
    };
}

RecognizedValueMapper.prototype._getFieldRowFromRecognizedValue = function (recognizedValue) {
    const fieldName = this._getFieldFromRecognizedValue(recognizedValue);

    return this._getFieldRow(fieldName, recognizedValue.rowIndex);
}

RecognizedValueMapper.prototype._getFieldRowFromCell = function (cell) {
    const fieldName = this._getFieldFromCell(cell);
    const rowIndex = cell.row.getIndex();

    return this._getFieldRow(fieldName, rowIndex);
}

RecognizedValueMapper.prototype._getFieldFromControl = function (control) {
    return control.serverID;
}

RecognizedValueMapper.prototype._getFieldFromCell = function (cell) {
    return cell.column.dataField;
}

RecognizedValueMapper.prototype._clearFormMapping = function () {
    for (let field in this.formMappingByField) {
        const mapping = this.formMappingByField[field];

        mapping.recognizedValues = [];
    }
}

RecognizedValueMapper.prototype._initFormMappingByControl = function (control) {
    const field = this._getFieldFromControl(control);
    const mapping = {
        control: control,
        recognizedValues: []
    };

    this.formMappingByField[field] = mapping;
}

RecognizedValueMapper.prototype._setFormMapping = function (control, recognizedValue, appendValue) {
    const field = this._getFieldFromControl(control);
    let mapping = this.formMappingByField[field];
    const rvArray = recognizedValue === null ? [] : [recognizedValue];
    let collectFeedback = true;

    if (!mapping) {
        mapping = {
            control: control,
            recognizedValues: rvArray
        };

        if (rvArray.length === 0) {
            collectFeedback = false;
        }
    }
    else if (appendValue === true) {
        mapping.recognizedValues.push(recognizedValue);
    }
    else {
        if (mapping.recognizedValues.length === 0 && rvArray.length === 0) {
            collectFeedback = false;
        }

        mapping.recognizedValues = rvArray;
    }

    this.formMappingByField[field] = mapping;

    if (collectFeedback === true) {
        const that = this;
        setTimeout(function () {
            that.feedbackCollector.collectFormFeedback(field, mapping.recognizedValues);
        }, 10);
    }
}

RecognizedValueMapper.prototype._setFormMappingByValue = function (recognizedValue) {
    const field = this._getFieldFromRecognizedValue(recognizedValue);

    const mapping = this.formMappingByField[field];
    if (!mapping) {
        return;
    }

    mapping.recognizedValues = [recognizedValue];
}

RecognizedValueMapper.prototype._getFormMappingValuesByControl = function (control) {
    const field = this._getFieldFromControl(control);
    const mapping = this.formMappingByField[field];

    return mapping.recognizedValues;
}

RecognizedValueMapper.prototype._correctMapping = function (control, recognizedValue, getMappingByControl, setMapping, appendValue) {
    const prevRecognizedValues = getMappingByControl(control);

    setMapping(control, recognizedValue);

    if (prevRecognizedValues) {
        if (appendValue === false) {
            prevRecognizedValues.forEach(function (rv) {
                rv.markAsNotMapped();
            });
        }
        else {
            prevRecognizedValues.forEach(function (rv) {
                rv.markAsMapped();
            });
        }
    }

    if (recognizedValue) {
        recognizedValue.markAsMapped();
    }
}

RecognizedValueMapper.prototype._correctFormMapping = function (control, recognizedValue, appendValue) {
    const that = this;
    const getMappingByControl = function (control) {
        return that._getFormMappingValuesByControl(control);
    };
    const setMapping = function (control, recognizedValue) {
        that._setFormMapping(control, recognizedValue, appendValue);
    };

    this._correctMapping(control, recognizedValue, getMappingByControl, setMapping, appendValue);
}

RecognizedValueMapper.prototype._correctGridMapping = function (cell, recognizedValue, appendValue) {
    const that = this;
    const getMappingByControl = function (cell) {
        return that._getGridMappingValuesByCell(cell);
    };
    const setMapping = function (cell, recognizedValue) {
        that._setGridMapping(cell, recognizedValue, appendValue);
    };

    this._correctMapping(cell, recognizedValue, getMappingByControl, setMapping, appendValue);
}

RecognizedValueMapper.prototype.trackGridControls = function (grid) {
    this.feedbackCollector.gridViewName = grid.dataMember.toLowerCase();
    this.detailsGridControl = grid;

    const that = this;

    grid.events.addEventHandler('startCellEdit', function (g, e) {
        that._handleStartCellEdit(e);
    });

    grid.events.addEventHandler('endCellEdit', function (g, e) {
        that._handleEndCellEdit(e);
    });

    grid.events.addEventHandler('beforeCellUpdate', function (g, e) {
        that._handleBeforeCellUpdate(e);
    });

    grid.events.addEventHandler('cellClick', function (g, e) {
        that._handleCellClick(e);
    });

    grid.events.addEventHandler('beforeRowDelete', function (g, e) {
        that._handleBeforeRowDelete(e.row, e.rows);
    });
}

RecognizedValueMapper.prototype._handleBeforeRowDelete = function (row, rows) {
    const that = this;
    setTimeout(function () {
        that.feedbackCollector.dumpGridFeedback();
    }, 10);

    this._clearSelectedCell();

    let rowsArray;

    if (row) {
        rowsArray = [row];
    }
    else if (rows) {
        rowsArray = [];

        for (let r in rows) {
            if (this.gridDeletedRowRegexp.test(r) !== true) {
                continue;
            }

            rowsArray.push(rows[r]);
        }
    }
    else {
        return;
    }

    this._adjustGridMappingsBeforeRowDelete(rowsArray);
}

RecognizedValueMapper.prototype._adjustGridMappingsBeforeRowDelete = function (rowsToDelete) {
    const adjustedGridMappingbyFieldRow = [];

    for (let fieldRow in this.gridMappingByFieldRow) {
        const mapping = this.gridMappingByFieldRow[fieldRow];
        const fieldRowInfo = this._getFieldRowInfo(fieldRow);
        const fieldRowIndex = fieldRowInfo.rowIndex;
        const fieldRowName = fieldRowInfo.fieldName;
        let keepRow = true;
        let rowIndexDecrement = 0;

        for (let i = 0; i < rowsToDelete.length; i++) {
            let rowIndexToDelete = rowsToDelete[i].getIndex();

            if (fieldRowIndex == rowIndexToDelete) {
                keepRow = false;
                break;
            }

            if (fieldRowIndex > rowIndexToDelete) {
                rowIndexDecrement++;
            }
        }

        if (keepRow === false) {
            continue;
        }

        if (rowIndexDecrement > 0) {
            const adjustedRowIndex = fieldRowIndex - rowIndexDecrement;
            const adjustedFieldRow = this._getFieldRow(fieldRowName, adjustedRowIndex);

            adjustedGridMappingbyFieldRow[adjustedFieldRow] = mapping;
        }
        else {
            adjustedGridMappingbyFieldRow[fieldRow] = mapping;
        }
    }

    this.gridMappingByFieldRow = adjustedGridMappingbyFieldRow;
}

RecognizedValueMapper.prototype._navigateToCellRect = function (cell) {
    const that = this;
    const getMappingValues = function (control) {
        return that._getGridMappingValuesByCell(control);
    };

    this._navigateToMappedRect(cell, getMappingValues);
}

RecognizedValueMapper.prototype._clearSelectedCell = function () {
    if (this.selectedCell === null) {
        return;
    }

    const recognizedValues = this._getGridMappingValuesByCell(this.selectedCell);
    if (recognizedValues) {
        recognizedValues.forEach(function (rv) {
            rv.markAsNotMapped();
        });
    }

    this.selectedCell = null;
}

RecognizedValueMapper.prototype._handleCellClick = function (e) {
    if (this.isUserInput === false) {
        return;
    }

    this._clearSelectedCell();

    this.selectedControl = null;
    this.selectedCell = e.cell;

    this._switchToBoxMode();
    this._navigateToCellRect(e.cell);
}

RecognizedValueMapper.prototype._handleStartCellEdit = function (e) {
    if (this.isUserInput === false) {
        return;
    }

    if (this.selectedCell === e.cell) {
        return;
    }

    this._clearSelectedCell();

    this.selectedControl = null;
    this.selectedCell = e.cell;

    this._switchToBoxMode();
}

RecognizedValueMapper.prototype._handleEndCellEdit = function (e) {
    if (this.isUserInput === false) {
        return;
    }

    if (this.selectedCell !== e.cell) {
        return;
    }

    this._switchToTableMode();
    this._clearSelectedCell();
}

RecognizedValueMapper.prototype._handleBeforeCellUpdate = function (e) {
    if (this.isUserInput === false) {
        return;
    }

    this._correctGridMapping(e.cell, null, false);
}

RecognizedValueMapper.prototype._clearGridMapping = function () {
    for (let fieldRow in this.gridMappingByFieldRow) {
        const mapping = this.gridMappingByFieldRow[fieldRow];

        mapping.recognizedValues = [];
    }
}

RecognizedValueMapper.prototype._initGridMappingByValue = function (recognizedValue) {
    const fieldName = this._getFieldFromRecognizedValue(recognizedValue);
    const row = this.detailsGridControl.rows.getRow(recognizedValue.rowIndex);

    const cell = row.getCell(fieldName);
    if (!cell) {
        return;
    }

    const mapping = {
        cell: cell,
        recognizedValues: [recognizedValue]
    };

    const fieldRow = this._getFieldRowFromRecognizedValue(recognizedValue);
    this.gridMappingByFieldRow[fieldRow] = mapping;
}

RecognizedValueMapper.prototype._getGridMappingValuesByCell = function (cell) {
    const fieldRow = this._getFieldRowFromCell(cell);
    const mapping = this.gridMappingByFieldRow[fieldRow];

    return mapping ? mapping.recognizedValues : null;
}

RecognizedValueMapper.prototype._setGridMapping = function (cell, recognizedValue, appendValue) {
    const fieldRow = this._getFieldRowFromCell(cell);
    let mapping = this.gridMappingByFieldRow[fieldRow];
    const rvArray = recognizedValue === null ? [] : [recognizedValue];
    let prevRecognizedValues = null;

    if (!mapping) {
        mapping = {
            cell: cell,
            recognizedValues: rvArray
        };
    }
    else if (appendValue === true) {
        prevRecognizedValues = mapping.recognizedValues;

        if (recognizedValue !== null) {
            mapping.recognizedValues.push(recognizedValue);
        }
    }
    else {
        prevRecognizedValues = mapping.recognizedValues;
        mapping.recognizedValues = rvArray;
    }

    this.gridMappingByFieldRow[fieldRow] = mapping;
    this.collectGridFeedback(cell, mapping.recognizedValues, prevRecognizedValues);
}

RecognizedValueMapper.prototype.collectGridFeedback = function (cell, recognizedValues, prevRecognizedValues) {
    const detailColumn = cell.column.dataField;
    const detailRow = cell.row.getIndex();
    let pageIndex = null;
    let tableIndex = null;
    let rowIndex = null;
    let columnIndexArray = [];

    if (recognizedValues.length === 0) {
        if (prevRecognizedValues === null || prevRecognizedValues.length === 0) {
            return;
        }

        const prevValue = prevRecognizedValues[0];
        if (prevValue === null || prevValue.cellInfo === null || prevValue.cellInfo.pageIndex === null || prevValue.cellInfo.tableIndex === null ||
            prevValue.cellInfo.rowIndex === null || prevValue.cellInfo.columnIndex === null) {
            return;
        }

        pageIndex = prevValue.cellInfo.pageIndex;
        tableIndex = prevValue.cellInfo.tableIndex;
        rowIndex = prevValue.cellInfo.rowIndex;
        columnIndexArray.push(-1);
    }
    else {
        const cellInfo = this._getCellInfo(recognizedValues);
        if (cellInfo === null) {
            return;
        }

        pageIndex = cellInfo.pageIndex;
        tableIndex = cellInfo.tableIndex;
        rowIndex = cellInfo.rowIndex;
        columnIndexArray = cellInfo.columnIndexArray;
    }

    const that = this;
    setTimeout(function () {
        that.feedbackCollector.collectGridFeedback(detailColumn, detailRow, pageIndex, tableIndex, columnIndexArray, rowIndex);
    }, 10);
}

RecognizedValueMapper.prototype._getCellInfo = function (recognizedValues) {
    let cellInfo = {
        pageIndex: null,
        tableIndex: null,
        rowIndex: null,
        columnIndexArray: []
    };

    for (let i = 0; i < recognizedValues.length; i++) {
        let rv = recognizedValues[i];

        if (rv.cellInfo === null || rv.cellInfo.pageIndex === null || rv.cellInfo.tableIndex === null || rv.cellInfo.rowIndex === null ||
            rv.cellInfo.columnIndex === null) {
            return null;
        }

        if (cellInfo.pageIndex === null) {
            cellInfo.pageIndex = rv.cellInfo.pageIndex;
        }
        else if (cellInfo.pageIndex !== rv.cellInfo.pageIndex) {
            return null;
        }

        if (cellInfo.tableIndex === null) {
            cellInfo.tableIndex = rv.cellInfo.tableIndex;
        }
        else if (cellInfo.tableIndex !== rv.cellInfo.pageIndex) {
            return null;
        }

        if (cellInfo.rowIndex === null) {
            cellInfo.rowIndex = rv.cellInfo.rowIndex;
        }
        else if (cellInfo.rowIndex !== rv.cellInfo.rowIndex) {
            return null;
        }

        cellInfo.columnIndexArray.push(rv.cellInfo.columnIndex);
    }

    return cellInfo;
}

RecognizedValueMapper.prototype.trackRecognizedValue = function (recognizedValue) {
    if (recognizedValue.isPrimaryField === true) {
        this._setFormMappingByValue(recognizedValue);
    }
    else if (recognizedValue.isDetailField === true) {
        this._initGridMappingByValue(recognizedValue);
    }

    this.recognizedValues.push(recognizedValue);

    const that = this;

    recognizedValue.subscribeOnMousedown(function (v, e) {
        that._mapRecognizedValue(v, e);
    });
}

RecognizedValueMapper.prototype._recognizedValueMappingInProgress = function () {
    return this.selectedControl !== null || this.selectedCell !== null;
}

RecognizedValueMapper.prototype._mapRecognizedValue = function (recognizedValue, event) {
    if (this._recognizedValueMappingInProgress() === false) {
        return;
    }

    // do not lose focus from the selected control
    event.preventDefault();

    const appendValue = event.ctrlKey === true || event.metaKey === true;
    const that = this;

    let control;
    let getMappingByControl;
    let performMapping;

    if (this.selectedControl !== null) {
        control = this.selectedControl;

        getMappingByControl = function () {
            return that._getFormMappingValuesByControl(that.selectedControl);
        };

        performMapping = function (recognizedValue, newValue, appendValue) {
            that._mapRecognizedValueToForm(recognizedValue, newValue, appendValue);
        };

    }
    else if (this.selectedCell !== null) {
        control = this.selectedCell;

        getMappingByControl = function () {
            return that._getGridMappingValuesByCell(that.selectedCell);
        }

        performMapping = function (recognizedValue, newValue, appendValue) {
            that._mapRecognizedValueToCell(recognizedValue, newValue, appendValue);
        }
    }

    this._mapRecognizedValueToControl(control, recognizedValue, appendValue, getMappingByControl, performMapping);
}

RecognizedValueMapper.prototype._mapRecognizedValueToControl = function (control, recognizedValue, appendValue, getMappingByControl, performMapping) {
    const mappedValues = getMappingByControl();

    if (mappedValues !== null) {
        const rvIndex = mappedValues.findIndex(function (rv) {
            return rv === recognizedValue;
        });
        const alreadyMapped = rvIndex !== -1;
        if (alreadyMapped === true) {
            return;
        }
    }

    const value = recognizedValue.value != null ? recognizedValue.value : recognizedValue.text;
    if (value === null) {
        return;
    }

    const newControlValue = this._getValueToMap(control, value, appendValue);
    const validationResult = this._validValueAndParams(control, newControlValue, appendValue);

    if (validationResult.isValid === true) {
        performMapping(recognizedValue, newControlValue, validationResult.append);
    }
}

RecognizedValueMapper.prototype._isDateControl = function (control) {
    const controlValue = control.getValue();
    const isDateControlValue = (controlValue && controlValue.constructor.name === 'Date') ||
        (control.__className == true && control.__className.indexOf('Date') > 0);

    return isDateControlValue;
}

RecognizedValueMapper.prototype._isStringValue = function (value) {
    const isStringValue = value && value.constructor.name === 'String';

    return isStringValue;
}

RecognizedValueMapper.prototype._validValueAndParams = function (control, value, appendValue) {
    if (!value) {
        return {
            isValid: false,
            append: false
        };
    }

    if (control.getReadOnly && control.getReadOnly() === true) {
        return {
            isValid: false,
            append: false
        };
    }

    if (control.parseValue) {
        if (isNaN(control.parseValue(value)) === true) {
            return {
                isValid: false,
                append: false
            };
        }
        else {
            return {
                isValid: true,
                append: false
            };
        }
    }

    const append = appendValue && this._isDateControl(control) === false && this._isStringValue(value) === true;

    return {
        isValid: true,
        append: append
    };
}

RecognizedValueMapper.prototype._getValueToMap = function (control, newValue, appendValue) {
    const isDateControlValue = this._isDateControl(control);
    const isStringNewValue = this._isStringValue(newValue);

    if (isDateControlValue === true && isStringNewValue === true) {
        try {
            const dateValue = new Date(newValue);

            if (dateValue < control.getMinValue() || dateValue > control.getMaxValue()) {
                return null;
            }

            // Convert date to mm/dd/yyyy format
            return Intl.DateTimeFormat('en-US').format(dateValue);
        }
        catch (e) {
            return null;
        }
    }

    if (appendValue === false || isStringNewValue === false) {
        return newValue;
    }

    const controlValue = control.getValue();
    const appendedValue = controlValue ? controlValue + ' ' + newValue : newValue;

    return appendedValue;
}

RecognizedValueMapper.prototype._mapRecognizedValueToForm = function (recognizedValue, value, appendValue) {
    let correctMapping = true;
    this.isUserInput = false;

    try {
        if (this.selectedControl.updateValue !== null) {
            this.selectedControl.updateValue(value)
        }
        else {
            this.selectedControl.setValue(value);
        }
    }
    catch (e) {
        correctMapping = false;
    }
    finally {
        this.isUserInput = true;
    }

    if (correctMapping === true) {
        this._correctFormMapping(this.selectedControl, recognizedValue, appendValue);
    }
}

RecognizedValueMapper.prototype._mapRecognizedValueToCell = function (recognizedValue, value, appendValue) {
    let correctMapping = true;
    let endEditOnError = false;

    if (!this.detailsGridControl.editMode) {
        this.detailsGridControl.beginEdit();
        endEditOnError = true;
    }

    if (this.selectedCell.updateValue !== null) {
        this.isUserInput = false;

        try {
            this.selectedCell.updateValue(value);
            this.detailsGridControl.executeCommand('Save');
        }
        catch (e) {
            correctMapping = false;

            if (endEditOnError === true) {
                this.detailsGridControl.endEdit();
            }
        }
        finally {
            this.isUserInput = true;
        }
    }
    else {
        this.isUserInput = false;

        try {
            this.selectedCell.setValue(value);
        }
        catch (e) {
            correctMapping = false;

            if (endEditOnError === true) {
                this.detailsGridControl.endEdit();
            }
        }
        finally {
            this.isUserInput = true;
        }
    }

    if (correctMapping === true) {
        this._correctGridMapping(this.selectedCell, recognizedValue, appendValue);
    }
}

RecognizedValueMapper.prototype.trackRecognizedTable = function (recognizedTable) {
    this.recognizedTables.push(recognizedTable);

    const that = this;

    recognizedTable.subscribeOnMousedown(function (table) {
        that._startRecognizedTableMapping(table);
    });

    recognizedTable.subscribeOnColumnMousedown(function (table, column) {
        column.markAsMapped();
        table.hideNotMappedColumns();
    });
}

RecognizedValueMapper.prototype._startRecognizedTableMapping = function (recognizedTable) {
    if (this._recognizedValueMappingInProgress() === true) {
        return;
    }

    const isSameTable = this.selectedTable === recognizedTable;
    if (isSameTable) {
        return;
    }

    this._stopRecognizedTableMapping();
    this.selectedTable = recognizedTable;
    this.selectedTable.markAsMapped();

    this._switchToColumnMode();

    this.recognizedTables.forEach(function (table) {
        if (table !== recognizedTable) {
            table.hideInColumnMode();
        }
    });
}

RecognizedValueMapper.prototype._stopRecognizedTableMapping = function () {
    if (this.selectedTable === null) {
        return;
    }

    this.selectedTable.markAsNotMapped();
    this.selectedTable = null;
    this.selectedColumn = null;

    this.recognizedTables.forEach(function (table) {
        table.showInColumnMode();
    });
}

RecognizedValueMapper.prototype.enrichValue = function (recognizedValue) {
    return this.recognizedValues.some(function (value) {
        if (recognizedValue.equals(value)) {
            if (value.rowIndex === null && recognizedValue.rowIndex !== null) {
                value.rowIndex = recognizedValue.rowIndex;
            }

            return true;
        }

        return false;
    });
}
