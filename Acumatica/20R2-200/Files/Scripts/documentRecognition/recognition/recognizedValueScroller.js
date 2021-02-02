function RecognizedValueScroller() {
    this._cellInfo = [];
}

RecognizedValueScroller.prototype.createCellScrollingIndex = function (recognizedPages, recognizedValues) {
    for (let pageIndex = 0; pageIndex < recognizedPages.length; pageIndex++) {
        let page = recognizedPages[pageIndex];

        if (page.tables && page.tables.length) {
            this._createPageCellInfo(recognizedValues, page, pageIndex);
        }
        else {
            this._addEmpyPageCellEntry(pageIndex);
        }
    }
}

RecognizedValueScroller.prototype._createPageCellInfo = function (recognizedValues, page, pageIndex) {
    for (let tableIndex = 0; tableIndex < page.tables.length; tableIndex++) {
        let table = page.tables[tableIndex];

        if (table.cells && table.cells.length) {
            this._createTableCellInfo(recognizedValues, table, tableIndex, pageIndex);
        }
        else {
            this._addEmptyTableCellEntry(pageIndex, tableIndex);
        }
    }
}

RecognizedValueScroller.prototype._createTableCellInfo = function (recognizedValues, table, tableIndex, pageIndex) {
    if (!table.columnNumber) {
        return;
    }

    for (let cellIndex = 0; cellIndex < table.cells.length; cellIndex++) {
        let cell = table.cells[cellIndex];

        if (cell && (cell.rowIndex === 0 || cell.rowIndex)) {
            this._createCellInfo(recognizedValues, cell, cellIndex, tableIndex, pageIndex, table.columnNumber);
        }
        else {
            this._addEmptyCellEntry(pageIndex, tableIndex, cellIndex);
        }
    }
}

RecognizedValueScroller.prototype._createCellInfo = function (recognizedValues, cell, cellIndex, tableIndex, pageIndex, columnNumber) {
    const that = this;

    recognizedValues.forEach(function (rv) {
        const isRelatedToCellFinder = function (bb) {
            if (bb.page !== pageIndex) {
                return false;
            }

            if (bb.table === tableIndex && bb.cell === cellIndex) {
                return true;
            }

            if (cell.ocr && cell.ocr.boundingBoxes) {
                for (let i = 0; i < cell.ocr.boundingBoxes.length; i++) {
                    const cellBb = cell.ocr.boundingBoxes[i];

                    if (cellBb.page === pageIndex && (cellBb.word === 0 || cellBb.word) && cellBb.word === bb.word) {
                        return true;
                    }
                }
            }

            return false;
        }

        const isRetatedToCell = rv.boundingBoxes.findIndex(isRelatedToCellFinder) !== -1;
        if (isRetatedToCell == true) {
            rv.addCellInfo(pageIndex, tableIndex, cellIndex, cell.rowIndex, cell.columnIndex, columnNumber);
            that._addCellInfoEntry(pageIndex, tableIndex, cellIndex, rv);
        }
    });
}

RecognizedValueScroller.prototype.scrollToRecognizedValue = function (container, recognizedValue) {
    if (!recognizedValue) {
        return;
    }

    let recognizedValueElement = recognizedValue.getScrollTarget();
    if (recognizedValueElement === null) {
        return;
    }

    const recognizedClientRect = recognizedValueElement.getBoundingClientRect();
    const parentClientRect = container.getBoundingClientRect();

    const scrollIsNotNeeded = this._isClientRectVisible(parentClientRect, recognizedClientRect);
    if (scrollIsNotNeeded === true) {
        return;
    }

    const headerElement = this._getHeaderElement(recognizedClientRect, parentClientRect, recognizedValue);
    if (headerElement != null) {
        this._scrollIntoView(headerElement, container, 0);
    }
    else {
        this._scrollIntoView(recognizedValueElement, container, 100);
    }
}

RecognizedValueScroller.prototype._scrollIntoView = function (element, container, marginTop) {
    element.scrollIntoView();

    container.scrollTop -= marginTop;
}

RecognizedValueScroller.prototype._getHeaderElement = function (recognizedClientRect, parentClientRect, recognizedValue) {
    const cellInfo = recognizedValue.cellInfo;
    if (cellInfo.pageIndex === null || cellInfo.tableIndex === null || cellInfo.cellIndex === null ||
        cellInfo.rowIndex === null || cellInfo.columnNumber === null) {
        return null;
    }

    const headerCellIndex = cellInfo.cellIndex - cellInfo.columnNumber * cellInfo.rowIndex;
    const headerCellInfo = this._getCellInfo(cellInfo.pageIndex, cellInfo.tableIndex, headerCellIndex);
    if (!headerCellInfo) {
        return null;
    }

    const headerValue = headerCellInfo[0];
    const headerValueElement = headerValue.getScrollTarget();
    if (headerValueElement === null) {
        return null;
    }

    const headerClientRect = headerValueElement.getBoundingClientRect();
    const canFitHeaderAndRv = (recognizedClientRect.bottom - headerClientRect.top) <= parentClientRect.height;

    return canFitHeaderAndRv ? headerValueElement : null;
}

RecognizedValueScroller.prototype._isClientRectVisible = function (parentRect, recognizedRect) {
    if (parentRect.top > recognizedRect.top) {
        return false;
    }

    if (parentRect.left > recognizedRect.left) {
        return false;
    }

    if (parentRect.bottom < recognizedRect.bottom) {
        return false;
    }

    if (parentRect.right < recognizedRect.right) {
        return false;
    }

    return true;
}

RecognizedValueScroller.prototype._getPageCellInfo = function (pageIndex) {
    if (!this._cellInfo[pageIndex]) {
        this._cellInfo[pageIndex] = [];
    }

    return this._cellInfo[pageIndex];
}

RecognizedValueScroller.prototype._getTableCellInfo = function (pageIndex, tableIndex) {
    const pageInfo = this._getPageCellInfo(pageIndex);

    if (!pageInfo[tableIndex]) {
        pageInfo[tableIndex] = [];
    }

    return pageInfo[tableIndex];
}

RecognizedValueScroller.prototype._getCellInfo = function (pageIndex, tableIndex, cellIndex) {
    const tableInfo = this._getTableCellInfo(pageIndex, tableIndex);

    if (!tableInfo[cellIndex]) {
        tableInfo[cellIndex] = [];
    }

    return tableInfo[cellIndex];
}

RecognizedValueScroller.prototype._addCellInfoEntry = function (pageIndex, tableIndex, cellIndex, recognizedValue) {
    const cellInfo = this._getCellInfo(pageIndex, tableIndex, cellIndex);

    cellInfo.push(recognizedValue);
}

RecognizedValueScroller.prototype._addEmpyPageCellEntry = function (pageIndex) {
    this._cellInfo[pageIndex] = [];
}

RecognizedValueScroller.prototype._addEmptyTableCellEntry = function (pageIndex, tableIndex) {
    const pageInfo = this._getPageCellInfo(pageIndex);

    pageInfo[tableIndex] = [];
}

RecognizedValueScroller.prototype._addEmptyCellEntry = function (pageIndex, tableIndex, cellIndex) {
    const tableInfo = this._getTableCellInfo(pageIndex, tableIndex);

    tableInfo[cellIndex] = [];
}
