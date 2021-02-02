function RecognizedValue(recognizedField, fieldName, rowIndex, recognizedData) {
    this.pageIndex = null;
    this.svgRectangle = [];
    this.boundingBoxesInfo = [];
    this.fieldName = fieldName;
    this.rowIndex = rowIndex;
    this.isPrimaryField = rowIndex === null;
    this.value = recognizedField.value;
    this.text = recognizedField.ocr.text;
    this.processBoundingBoxes(recognizedField.ocr.boundingBoxes, recognizedData);
}

RecognizedValue.prototype.processBoundingBoxes = function (boundingBoxes, recognizedData) {
    let rectangle = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
    rectangle.setAttribute('class', 'pdf-viewer-rectangle');
    rectangle.setAttribute('rx', '2px');

    this.pageIndex = boundingBoxes[0].page;

    if (!recognizedData || !recognizedData.pages || !recognizedData.pages[this.pageIndex]) {
        return;
    }

    // Use the same page for all boxes
    let page = recognizedData.pages[this.pageIndex];
    let pageWidth = page.width;
    let pageHeight = page.height;

    let rectXMin = null;
    let rectYMin = null;
    let rectXMax = null;
    let rectYMax = null;

    for (let i = 0; i < boundingBoxes.length; i++) {
        let boundingBoxInfo = boundingBoxes[i];
        let word = page.words[boundingBoxInfo.word];
        let boundingBox = word.boundingBox;

        let boxXMin = Math.min(boundingBox[0].x, boundingBox[1].x, boundingBox[2].x, boundingBox[3].x);
        let boxYmin = Math.min(boundingBox[0].y, boundingBox[1].y, boundingBox[2].y, boundingBox[3].y);
        let boxXMax = Math.max(boundingBox[0].x, boundingBox[1].x, boundingBox[2].x, boundingBox[3].x);
        let boxYmax = Math.max(boundingBox[0].y, boundingBox[1].y, boundingBox[2].y, boundingBox[3].y);

        rectXMin = rectXMin === null ?
            boxXMin :
            Math.min(rectXMin, boxXMin);
        rectYMin = rectYMin === null ?
            boxYmin :
            Math.min(rectYMin, boxYmin);
        rectXMax = rectXMax === null ?
            boxXMax :
            Math.max(rectXMax, boxXMax);
        rectYMax = rectYMax === null ?
            boxYmax :
            Math.max(rectYMax, boxYmax);

        this.boundingBoxesInfo.push(boundingBoxInfo);
    }

    let width = rectXMax - rectXMin;
    let height = rectYMax - rectYMin;

    let xPercents = this.calculatePercents(pageWidth, rectXMin);
    rectangle.setAttribute('x', xPercents);

    let yPercents = this.calculatePercents(pageHeight, rectYMin);
    rectangle.setAttribute('y', yPercents);

    let widthPercents = this.calculatePercents(pageWidth, width);
    rectangle.setAttribute('width', widthPercents);

    let heightPercents = this.calculatePercents(pageHeight, height);
    rectangle.setAttribute('height', heightPercents);

    this.svgRectangle = rectangle;
}

RecognizedValue.prototype.calculatePercents = function (percent100, percentX) {
    let value = (percentX * 100) / percent100;

    return value + '%';
}
