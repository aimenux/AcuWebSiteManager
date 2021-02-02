'use strict';

function FeedbackCollector(dumpTableFeedbackCallback, fieldBoundFeedbackControl, tableRelatedFeedbackControl, vendorFieldName) {
    this.dumpTableFeedbackCallback = dumpTableFeedbackCallback;
    this.fieldBoundFeedbackControl = fieldBoundFeedbackControl;
    this.tableRelatedFeedbackControl = tableRelatedFeedbackControl;
    this.vendorFieldName = vendorFieldName;

    this.pageKeyValuePairByWord = [];
    this.recognizedPages = null;
    this.formViewName = null;
    this.gridViewName = null;
}

FeedbackCollector.prototype.fillPageKeyValuePairByWord = function (recognizedPages) {
    this.recognizedPages = JSON.parse(JSON.stringify(recognizedPages));
    this.pageKeyValuePairByWord = new Array(recognizedPages.length);
    const that = this;

    recognizedPages.forEach(function (page, i) {
        that.pageKeyValuePairByWord[i] = new Array(page.words.length);
    });

    recognizedPages.forEach(function (page) {
        if (page.keyValuePairs != null) {
            page.keyValuePairs.forEach(function (kv, kvIndex) {
                if (kv.value != null && kv.value.ocr != null && kv.value.ocr.boundingBoxes != null) {
                    kv.value.ocr.boundingBoxes.forEach(function (bb) {
                        const boxPage = bb.page;
                        const boxWord = bb.word;
                        const keyValuePair = kvIndex;

                        that.pageKeyValuePairByWord[boxPage][boxWord] = keyValuePair;
                    });
                }
            });
        }
    });
}

FeedbackCollector.prototype.collectFormFeedback = function (fieldName, recognizedValues) {
    let boundingBoxes = [];
    const that = this;

    recognizedValues.forEach(function (rv) {
        rv.boundingBoxes.forEach(function (bb) {
            const page = that.recognizedPages[bb.page];

            if (bb.table != null && bb.cell != null) {
                const table = page.tables[bb.table];
                const cell = table.cells[bb.cell];

                if (cell.ocr) {
                    boundingBoxes = boundingBoxes.concat(cell.ocr.boundingBoxes);
                }
            }
            else if (bb.keyValuePair != null) {
                boundingBoxes.push(bb);
            }
            else if (bb.page != null && bb.word != null) {
                let newBox = {
                    page: bb.page,
                    word: bb.word
                };
                const keyValuePair = that.pageKeyValuePairByWord[bb.page][bb.word];

                if (keyValuePair != null) {
                    newBox.word = null;
                    newBox.keyValuePair = keyValuePair;
                }

                boundingBoxes.push(newBox);
            }
        });
    });

    let feedback = {
    };

    const feedbackFieldName = this.formViewName + '.' + fieldName;
    if (feedbackFieldName === this.vendorFieldName) {
        const fullTextTerm = this._getFullTextSearchFieldTerm(boundingBoxes);

        feedback[feedbackFieldName] = fullTextTerm;
    }
    else {
        const ocr = this._getFieldBoundOcr(boundingBoxes);

        feedback[feedbackFieldName] = ocr;
    }

    const feedbackString = JSON.stringify(feedback);
    const feedbackEncoded = encodeURIComponent(feedbackString);

    this.fieldBoundFeedbackControl.updateValue(feedbackEncoded);
}

FeedbackCollector.prototype._getFieldBoundOcr = function (boundingBoxes) {
    return {
        ocr: {
            boundingBoxes: boundingBoxes
        }
    };
}

FeedbackCollector.prototype._getFullTextSearchFieldTerm = function (boundingBoxes) {
    if (boundingBoxes.length > 0) {
        return {
            fullTextTerms: [
                {
                    boundingBoxes: boundingBoxes
                }
            ]
        };
    }

    return {
        fullTextTerms: []
    };
}

FeedbackCollector.prototype.collectGridFeedback = function (detailColumn, detailRow, pageIndex, tableIndex, columnIndexArray, rowIndex) {
    const feedbackColumnName = this.gridViewName + '.' + detailColumn;
    let cellBound = {
        detailColumn: feedbackColumnName,
        detailRow: detailRow,
        page: pageIndex,
        table: tableIndex,
        columns: columnIndexArray,
        row: rowIndex
    };

    const feedbackString = JSON.stringify(cellBound);
    const feedbackEncoded = encodeURIComponent(feedbackString);

    this.tableRelatedFeedbackControl.updateValue(feedbackEncoded);
}

FeedbackCollector.prototype.dumpGridFeedback = function () {
    this.dumpTableFeedbackCallback();
}
