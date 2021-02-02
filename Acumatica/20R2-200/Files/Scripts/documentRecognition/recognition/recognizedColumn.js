'use strict';

const columnClass = 'recognition-column';

function RecognizedColumn(table, columnIndex, containerWidth, containerHeight) {
    const coordinates = this._getCoordinates(table, columnIndex);
    RecognizedRectangle.call(this, coordinates, containerWidth, containerHeight, false);

    this.element.classList.add(columnClass);
}

RecognizedColumn.prototype = Object.create(RecognizedRectangle.prototype);
RecognizedColumn.prototype.constructor = RecognizedColumn;

RecognizedColumn.prototype.rescale = function (containerWidth, containerHeight) {
    const isSizeSet = this.element.style.width !== '';
    if (isSizeSet) {
        return;
    }
    this.element.style.left = this.convertRelativeToPixel(this.xRelative, containerWidth) + 'px';
    this.element.style.top = this.convertRelativeToPixel(this.yRelative, containerHeight)  + 'px';
    this.element.style.width = this.convertRelativeToPixel(this.widthRelative, containerWidth)  + 'px';
    this.element.style.height = this.convertRelativeToPixel(this.heightRelative, containerHeight)  + 'px';
}

RecognizedColumn.prototype._getCoordinates = function (table, columnIndex) {
    const columnCells = table.cells.filter(function (cell) {
        return cell.columnIndex === columnIndex;
    });

    let leftTop = null; // xMin, yMin
    let rightTop = null; // xMax, yMin
    let rightBottom = null; // xMax, yMax
    let leftBottom = null; // xMin, yMax

    columnCells.forEach(function (cell) {
        const coordinates = cell.boundingBox;
        const leftTopCell = coordinates[0];
        const rightTopCell = coordinates[1];
        const rightBottomCell = coordinates[2];
        const leftBottomCell = coordinates[3];

        if (leftTop === null) {
            leftTop = leftTopCell;
        }
        else {
            if (leftTop.x > leftTopCell.x) {
                leftTop.x = leftTopCell.x;
            }

            if (leftTop.y > leftTopCell.y) {
                leftTop.y = leftTopCell.y;
            }
        }

        if (rightTop === null) {
            rightTop = rightTopCell;
        }
        else {
            if (rightTop.x < rightTopCell.x) {
                rightTop.x = rightTopCell.x;
            }

            if (rightTop.y > rightTopCell.y) {
                rightTop.y = rightTopCell.y;
            }
        }

        if (rightBottom === null) {
            rightBottom = rightBottomCell;
        }
        else {
            if (rightBottom.x < rightBottomCell.x) {
                rightBottom.x = rightBottomCell.x;
            }

            if (rightBottom.y < rightBottomCell.y) {
                rightBottom.y = rightBottomCell.y;
            }
        }

        if (leftBottom === null) {
            leftBottom = leftBottomCell;
        }
        else {
            if (leftBottom.x > leftBottomCell.x) {
                leftBottom.x = leftBottomCell.x;
            }

            if (leftBottom.y < leftBottomCell.y) {
                leftBottom.y = leftBottomCell.y;
            }
        }
    });

    return [leftTop, rightTop, rightBottom, leftBottom];
}