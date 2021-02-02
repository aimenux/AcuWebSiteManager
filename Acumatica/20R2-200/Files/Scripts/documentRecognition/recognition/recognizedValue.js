'use strict';

const valueClass = 'recognition-value';

function RecognizedValue(fieldInfo, recognizedPages, pagesInfo, wordInfo) {
    this.fieldName = null;
    this.rowIndex = null;
    this.isPrimaryField = false;
    this.isDetailField = false;

    this.recognizedPages = recognizedPages;
    this.pagesInfo = pagesInfo;

    this.rectangles = [];
    this.boundingBoxes = [];
    this.rectangleScrollIndex = 0;

    this.text = null;
    this.value = null;

    this.cellInfo = {
        rectangleIndex: 0,
        pageIndex: null,
        tableIndex: null,
        cellIndex: null,
        rowIndex: null,
        columnIndex: null,
        columnNumber: null
    };

    this.scrollRowIndex = null;
    this.scrollColumnNumber = null;

    let recognizedField = null;
    let searchTerm = null;
    if (fieldInfo !== null) {
        this.fieldName = fieldInfo.fieldName;
        this.rowIndex = fieldInfo.rowIndex;
        this.isPrimaryField = fieldInfo.isPrimaryField;

        recognizedField = fieldInfo.recognizedField;
        searchTerm = fieldInfo.searchTerm;

        if (this.rowIndex !== null) {
            this.isDetailField = true;
        }
    }

    let ocr = null;
    if (recognizedField !== null) {
        this.value = recognizedField.value;
        ocr = recognizedField.ocr;
    }

    this._initText(ocr, wordInfo, searchTerm);
    this._initBoundingBoxes(ocr, wordInfo, searchTerm);
}

RecognizedValue.prototype.equals = function (otherValue) {
    if (otherValue.value !== this.value) {
        return false;
    }

    if (otherValue.text !== this.text) {
        return false;
    }

    if (otherValue.rectangles.length !== this.rectangles.length) {
        return false;
    }

    const that = this;

    return otherValue.rectangles.every(function (otherRect) {
        return that.rectangles.some(function (thisRect) {
            return thisRect.equals(otherRect);
        });
    });
}

RecognizedValue.prototype._initText = function (ocr, wordInfo, searchTerm) {
    if (ocr && ocr.text) {
        this.text = ocr.text;
    }
    else if (wordInfo && wordInfo.word && wordInfo.word.text) {
        this.text = wordInfo.word.text;
    }
    else if (searchTerm && searchTerm.text) {
        this.text = searchTerm.text;
    }
}

RecognizedValue.prototype._initBoundingBoxes = function (ocr, wordInfo, searchTerm) {
    if (ocr && ocr.boundingBoxes) {
        this._initOcrBoundingBoxes(ocr.boundingBoxes);
    }
    else if (wordInfo && wordInfo.word && wordInfo.wordIndex != null) {
        this._initWordBoundingBox(wordInfo.word, wordInfo.wordIndex, wordInfo.pageIndex);
    }
    else if (searchTerm) {
        this._initOcrBoundingBoxes(searchTerm.boundingBoxes);
    }
}

RecognizedValue.prototype._initOcrBoundingBoxes = function (boundingBoxes) {
    const that = this;

    boundingBoxes.forEach(function (box) {
        const page = that.recognizedPages[box.page];
        const container = that.pagesInfo[box.page].canvas;

        let coordinates = null;

        if (box.word != null) {
            let word = page.words[box.word];

            coordinates = word.boundingBox;
        }
        else if (box.keyValuePair != null) {
            let keyValuePair = page.keyValuePairs[box.keyValuePair];

            coordinates = keyValuePair.value.boundingBox;
        }
        else if (box.table != null && box.cell != null) {
            let table = page.tables[box.table];
            let cell = table.cells[box.cell];

            coordinates = cell.boundingBox;
        }

        if (coordinates !== null) {
            that._addRectangle(coordinates, container.width, container.height);
            that.boundingBoxes.push(box);
        }
    });
}

RecognizedValue.prototype._initWordBoundingBox = function (word, wordIndex, pageIndex) {
    let coordinates = word.boundingBox;
    if (coordinates === null || !coordinates.length || coordinates.length !== 4) {
        return;
    }

    const container = this.pagesInfo[pageIndex].canvas;

    this._addRectangle(coordinates, container.width, container.height);

    let boundingBox = {
        page: pageIndex,
        word: wordIndex
    };

    this.boundingBoxes.push(boundingBox);
}

RecognizedValue.prototype._addRectangle = function (coordinates, containerWidth, containerHeight) {
    const rect = new RecognizedRectangle(coordinates, containerWidth, containerHeight);

    rect.element.classList.add(valueClass);
    this.rectangles.push(rect);
}

RecognizedValue.prototype.subscribeOnMousedown = function (callback) {
    const that = this;

    this.rectangles.forEach(function (rect) {
        rect.subscribeOnMousedown(function (r, event) {
            callback(that, event);
        });
    })
}

RecognizedValue.prototype.appendToPages = function () {
    for (let i = 0; i < this.rectangles.length; i++) {
        let rect = this.rectangles[i];
        let page = this.boundingBoxes[i].page;
        let parent = this.pagesInfo[page].container;

        rect.appendToParent(parent);
    }
}

RecognizedValue.prototype.markAsMapped = function () {
    this.rectangles.forEach(function (rect) {
        rect.markAsMapped();
    });
}

RecognizedValue.prototype.markAsNotMapped = function () {
    this.rectangles.forEach(function (rect) {
        rect.markAsNotMapped();
    });
}

RecognizedValue.prototype.rescale = function () {
    for (let i = 0; i < this.rectangles.length; i++) {
        let rect = this.rectangles[i];
        let page = this.boundingBoxes[i].page;
        let container = this.pagesInfo[page].canvas;

        rect.rescale(container.width, container.height);
    }
}

RecognizedValue.prototype.getScrollTarget = function () {
    if (this.rectangles.length === 0) {
        return null;
    }

    const firstRectangle = this.rectangles[this.cellInfo.rectangleIndex];
    const rectangleElement = firstRectangle.element;

    return rectangleElement;
}

RecognizedValue.prototype.addCellInfo = function (pageIndex, tableIndex, cellIndex, rowIndex, columnIndex, columnNumber) {
    this.cellInfo.pageIndex = pageIndex;
    this.cellInfo.tableIndex = tableIndex;
    this.cellInfo.cellIndex = cellIndex;
    this.cellInfo.rowIndex = rowIndex;
    this.cellInfo.columnIndex = columnIndex;
    this.cellInfo.columnNumber = columnNumber;
}
