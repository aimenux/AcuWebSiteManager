function RecognitionCorrection(recognizedData, feedbackControl) {
    this.recognizedValues = [];
    this.controlFieldMap = [];
    this.gridCellsFieldMap = [];
    this.selectedControl = null;
    this.selectedGridCell = null;
    this.detailsGrid = null;
    this.tmpBox = null;
    this.recognizedDocument = this.getDocumentFromData(recognizedData);
    this.isUserInput = true;
    this.feedbackControl = feedbackControl;
    this.documentBoundingInfo = {
        primaryFields: {
        },
        detailFields: [
        ]
    };
}

RecognitionCorrection.prototype.updateFeedback = function (updateValue) {
    if (this.feedbackControl === null) {
        return;
    }

    let feedback = {};
    let primaryFields = this.documentBoundingInfo.primaryFields;

    for (let field in primaryFields) {
        let words = [];
        let recognizedValue = primaryFields[field].recognizedValue;

        if (recognizedValue !== null) {

            recognizedValue.boundingBoxesInfo.forEach(function (box) {
                words = words.concat(box.word);
            })
        }

        feedback[field] = words;
    }

    let feedbackJson = JSON.stringify(feedback);

    if (updateValue === true) {
        this.feedbackControl.setValue('');
        this.feedbackControl.updateValue(feedbackJson);
    }
    else {
        this.feedbackControl.setValue(feedbackJson);
    }
}

RecognitionCorrection.prototype.getDocumentFromData = function (recognizedData) {
    if (recognizedData && recognizedData.documents && recognizedData.documents[0]) {
        return recognizedData.documents[0];
    }

    return null;
}

RecognitionCorrection.prototype.trackFormControls = function (form, formControlsMapping, recognizedData) {
    let that = this;

    for (let controlName in form.controls) {
        let c = form.controls[controlName];

        c.events.addEventHandler('focus', function (control) {
            that.handleFormControlFocus(control);
        });

        c.events.addEventHandler('valueChanged', function (control) {
            that.clearPrimaryBounding(control);
        });

        let idArray = c.ID.split('_');
        let formControlId = idArray[idArray.length - 1];

        if (formControlId === formControlsMapping.vendor.formControl) {
            this.controlFieldMap[c.ID] = formControlsMapping.vendor.recognitionField;
            this.addControlToPrimaryBounding(c, formControlsMapping.vendor.recognitionField);
        }
        else if (formControlId === formControlsMapping.date.formControl) {
            this.controlFieldMap[c.ID] = formControlsMapping.date.recognitionField;
            this.addControlToPrimaryBounding(c, formControlsMapping.date.recognitionField);
        }
        else if (formControlId === formControlsMapping.amount.formControl) {
            this.controlFieldMap[c.ID] = formControlsMapping.amount.recognitionField;
            this.addControlToPrimaryBounding(c, formControlsMapping.amount.recognitionField);
        }
    }
}

RecognitionCorrection.prototype.addControlToPrimaryBounding = function (ctrl, fieldName) {
    this.documentBoundingInfo.primaryFields[fieldName] = {
        control: ctrl,
        recognizedValue: null
    };

    this.updateFeedback(false);
}

RecognitionCorrection.prototype.addRecognizedValueToPrimaryBounding = function (recognizedValue) {
    let field = this.documentBoundingInfo.primaryFields[recognizedValue.fieldName];

    if (field) {
        field.recognizedValue = recognizedValue;
    }
    else {
        this.documentBoundingInfo.primaryFields[recognizedValue.fieldName] = {
            control: null,
            recognizedValue: recognizedValue
        }
    }

    this.updateFeedback(false);
}

RecognitionCorrection.prototype.correctPrimaryBounding = function (control, recognizedValue) {
    let field = this._getPrimaryBoundingFieldByControl(control);
    if (field === null || field.recognizedValue === recognizedValue) {
        return;
    }

    field.recognizedValue = recognizedValue;

    this.updateFeedback(true);
}

RecognitionCorrection.prototype.clearPrimaryBounding = function (control) {
    if (!this.isUserInput) {
        return;
    }

    let field = this._getPrimaryBoundingFieldByControl(control);
    if (field === null) {
        return;
    }

    field.recognizedValue = null;

    this.updateFeedback(true);
}

RecognitionCorrection.prototype._getPrimaryBoundingFieldByControl = function (control) {
    for (let fieldName in this.documentBoundingInfo.primaryFields) {
        let field = this.documentBoundingInfo.primaryFields[fieldName];

        if (field.control && field.control === control) {
            return field;
        }
    }

    return null;
}

RecognitionCorrection.prototype.trackGridControls = function (grid, gridControlsMapping) {
    this.gridCellsFieldMap[gridControlsMapping.inventory.gridField] = gridControlsMapping.inventory.recognitionField;
    this.gridCellsFieldMap[gridControlsMapping.description.gridField] = gridControlsMapping.description.recognitionField;
    this.gridCellsFieldMap[gridControlsMapping.uom.gridField] = gridControlsMapping.uom.recognitionField;
    this.gridCellsFieldMap[gridControlsMapping.qty.gridField] = gridControlsMapping.qty.recognitionField;
    this.gridCellsFieldMap[gridControlsMapping.unitPrice.gridField] = gridControlsMapping.unitPrice.recognitionField;

    this.detailsGrid = grid;

    let that = this;

    grid.events.addEventHandler('startCellEdit', function (g, e) {
        that.handleGridStartCellEdit(g, e);
    })

    grid.events.addEventHandler('endCellEdit', function (g, e) {
        that.handleGridEndCellEdit(g, e);
    })

    grid.events.addEventHandler('cellClick', function (g, e) {
        that.handleGridCellClick(g, e);
    })
}

RecognitionCorrection.prototype.handleGridCellClick = function (grid, e) {
    let rowIndex = e.cell.row.getIndex();
    let fieldName = this.gridCellsFieldMap[e.cell.column.dataField];

    for (let i = 0; i < this.recognizedValues.length; i++) {
        if (this.recognizedValues[i].fieldName === fieldName && this.recognizedValues[i].rowIndex === rowIndex) {
            this.recognizedValues[i].svgRectangle.scrollIntoView({ behavior: 'smooth' });
        }
    }
}

RecognitionCorrection.prototype.handleGridStartCellEdit = function (grid, e) {
    this.selectedControl = null;
    this.selectedGridCell = e.cell;

    let rowIndex = e.cell.row.getIndex();
    let fieldName = this.gridCellsFieldMap[e.cell.column.dataField];

    for (let i = 0; i < this.recognizedValues.length; i++) {
        if (this.recognizedValues[i].fieldName === fieldName && this.recognizedValues[i].rowIndex === rowIndex) {
            this.recognizedValues[i].svgRectangle.scrollIntoView({ behavior: 'smooth' });
        }
    }
}

RecognitionCorrection.prototype.handleGridEndCellEdit = function (grid, e) {
    this.selectedGridCell = null;
}

RecognitionCorrection.prototype.handleFormControlFocus = function (control) {
    this.selectedGridCell = null;
    this.selectedControl = control;

    let fieldName = this.controlFieldMap[control.ID];

    for (let i = 0; i < this.recognizedValues.length; i++) {
        if (this.recognizedValues[i].fieldName === fieldName) {
            this.recognizedValues[i].svgRectangle.scrollIntoView({ behavior: 'smooth' });
            break;
        }
    }
}

RecognitionCorrection.prototype.trackRecognizedValue = function (recognizedValue) {
    if (recognizedValue.isPrimaryField) {
        this.addRecognizedValueToPrimaryBounding(recognizedValue);
    }

    this.recognizedValues.push(recognizedValue);

    let that = this;
    recognizedValue.svgRectangle.addEventListener("click", function (event) {
        that.handleRecognizedValueClick(recognizedValue, event);
    });
}

RecognitionCorrection.prototype.handleRecognizedValueClick = function (recognizedValue, event) {
    let newValue = recognizedValue.value;

    if (this.selectedControl !== null) {
        this.correctPrimaryBounding(this.selectedControl, recognizedValue);
        this.isUserInput = false;

        if (this.selectedControl.updateValue !== null) {
            this.selectedControl.updateValue(newValue)
        }
        else {
            this.selectedControl.setValue(newValue);
        }

        this.isUserInput = true;
        this.selectedControl.focus();
    }
    else if (this.selectedGridCell !== null) {
        if (this.selectedGridCell.updateValue !== null) {
            this.isUserInput = false;
            this.selectedGridCell.updateValue(newValue);
            this.isUserInput = true;

            this.detailsGrid.executeCommand('Save');
        }
        else {
            this.isUserInput = false;
            this.selectedGridCell.setValue(newValue);
            this.isUserInput = true;
        }
    }
}
