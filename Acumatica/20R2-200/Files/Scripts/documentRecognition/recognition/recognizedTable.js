'use strict';

const tableClass = 'recognition-table';
const hiddenInColumnModeClass = 'hidden-in-column-mode';

function RecognizedTable(table, containerWidth, containerHeight) {
    this.columns = [];
    
    RecognizedRectangle.call(this, table.boundingBox, containerWidth, containerHeight, false);

    this.element.classList.add(tableClass);
    this._initColumns(table, containerWidth, containerHeight);
}

RecognizedTable.prototype = Object.create(RecognizedRectangle.prototype);
RecognizedTable.prototype.constructor = RecognizedTable;

RecognizedTable.prototype._initColumns = function (table, containerWidth, containerHeight) {
    for (let i = 0; i < table.columnNumber; i++) {
        const column = new RecognizedColumn(table, i, containerWidth, containerHeight);

        this.columns.push(column);
    }
}

RecognizedTable.prototype.appendToParent = function (parent) {
    RecognizedRectangle.prototype.appendToParent.call(this, parent);

    this.columns.forEach(function (column) {
        column.appendToParent(parent);
    })
}

RecognizedTable.prototype.markAsNotMapped = function () {
    RecognizedRectangle.prototype.markAsNotMapped.call(this);

    this.columns.forEach(function (column) {
        column.markAsNotMapped();
    });
}

RecognizedTable.prototype.hideInColumnMode = function () {
    this._hideInColumnMode(this.element);

    const that = this;
    this.columns.forEach(function (column) {
        that._hideInColumnMode(column.element);
    });
}

RecognizedTable.prototype.showInColumnMode = function () {
    this._showInColumnMode(this.element);

    const that = this;
    this.columns.forEach(function (column) {
        that._showInColumnMode(column.element);
    });
}

RecognizedTable.prototype._hideInColumnMode = function (element) {
    element.classList.add(hiddenInColumnModeClass);
}

RecognizedTable.prototype._showInColumnMode = function (element) {
    element.classList.remove(hiddenInColumnModeClass);
}

RecognizedTable.prototype.subscribeOnColumnMousedown = function (callback) {
    const that = this;

    this.columns.forEach(function (column) {
        column.subscribeOnMousedown(function (c) {
            callback(that, c);
        });
    });
}

RecognizedTable.prototype.hideNotMappedColumns = function () {
    const that = this;

    this.columns.forEach(function (column) {
        if (column.isMapped() === false) {
            that._hideInColumnMode(column.element);
        }
    });
}
