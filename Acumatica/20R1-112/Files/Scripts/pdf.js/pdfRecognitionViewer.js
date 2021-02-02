function PdfRecognitionViewer(fileUrl, pdfjsLib, container, recognizedData, feedbackControl) {
    this.container = container;
    this.fileUrl = fileUrl;
    this.pdfjsLib = pdfjsLib;
    this.recognizedData = recognizedData;
    this.viewerContainer = null;
    this.pageInfo = [];
    this.scaleSpan = null;
    this.currentPageNum = 1;
    this.scale = 1.0;
    this.scaleStep = 0.1;
    this.minScale = this.scaleStep;
    this.maxScale = this.scaleStep * 100;
    this.scrolling = false;
    this.pageBoundingClientRectsOnScrolling = [];
    this.pageNumPending = null;
    this.currentPageSpan = null;
    this.pagesCountSpan = null;
    this.recognitionCorrection = new RecognitionCorrection(recognizedData, feedbackControl);

    this.createChildControls(container);
}

PdfRecognitionViewer.prototype.trackFormControls = function (form, controlsInfo) {
    this.recognitionCorrection.trackFormControls(form, controlsInfo, this.recognizedData);
}

PdfRecognitionViewer.prototype.trackGridControls = function (grid, gridControlsMapping) {
    this.recognitionCorrection.trackGridControls(grid, gridControlsMapping);
}

PdfRecognitionViewer.prototype.createChildControls = function (container) {
    this.createToolbarControls(container);
    this.createViewerControls(container);
}

PdfRecognitionViewer.prototype.createToolbarControls = function (container) {
    this.toolbarContainer = document.createElement('div');

    let that = this;
    const buttonClassName = 'pdf-viewer-button';

    let prevButton = document.createElement('button');
    prevButton.type = 'button';
    prevButton.className = buttonClassName;
    let prevButtonText = document.createTextNode('<');
    prevButton.appendChild(prevButtonText);
    prevButton.addEventListener("click", function () { that.onPrevPage(that) });
    this.toolbarContainer.appendChild(prevButton);

    let nextButton = document.createElement('button');
    nextButton.type = 'button';
    nextButton.className = buttonClassName;
    let nextButtonText = document.createTextNode('>');
    nextButton.appendChild(nextButtonText);

    nextButton.addEventListener("click", function () { that.onNextPage(that) });
    this.toolbarContainer.appendChild(nextButton);

    let pagesInfoSpan = document.createElement('span');
    this.currentPageSpan = document.createElement('span');
    let defaultPageNum = document.createTextNode('1');
    this.currentPageSpan.appendChild(defaultPageNum);
    this.pagesCountSpan = document.createElement('span');
    pagesInfoSpan.appendChild(this.currentPageSpan);
    let pagesInfoSeparator = document.createTextNode(' / ');
    pagesInfoSpan.appendChild(pagesInfoSeparator);
    pagesInfoSpan.appendChild(this.pagesCountSpan);
    this.toolbarContainer.appendChild(pagesInfoSpan);

    let zoomOutButton = document.createElement('button');
    zoomOutButton.type = 'button';
    zoomOutButton.className = buttonClassName;
    let zoomOutButtonText = document.createTextNode('-');
    zoomOutButton.appendChild(zoomOutButtonText);
    zoomOutButton.addEventListener("click", function () { that.onZoomOut(that) });
    this.toolbarContainer.appendChild(zoomOutButton);

    let zoomInButton = document.createElement('button');
    zoomInButton.type = 'button';
    zoomInButton.className = buttonClassName;
    let zoomInButtonText = document.createTextNode('+');
    zoomInButton.appendChild(zoomInButtonText);
    zoomInButton.addEventListener("click", function () { that.onZoomIn(that) });
    this.toolbarContainer.appendChild(zoomInButton);

    let scaleInPercentSpan = document.createElement('span');
    this.scaleSpan = document.createElement('span');
    scaleInPercentSpan.appendChild(this.scaleSpan);
    let percentChar = document.createTextNode('%');
    scaleInPercentSpan.appendChild(percentChar);
    this.toolbarContainer.appendChild(scaleInPercentSpan);

    container.appendChild(this.toolbarContainer);
}

PdfRecognitionViewer.prototype.createViewerControls = function (container) {
    this.viewerContainer = document.createElement('div');
    this.viewerContainer.className = 'pdf-viewer-container';

    let that = this;
    this.viewerContainer.addEventListener('scroll', function (e) { that.onPageScroll(e); })

    container.appendChild(this.viewerContainer);
}

PdfRecognitionViewer.prototype.createPageControls = function (page) {
    let pageContainer = document.createElement('div');
    pageContainer.id = 'pdfPage' + page.pageNumber;
    pageContainer.className = 'pdf-viewer-page';

    let canvas = document.createElement('canvas');
    canvas.className = 'pdf-viewer-canvas';
    let canvasContext = canvas.getContext('2d');
    pageContainer.appendChild(canvas);

    let svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    svg.setAttribute('preserveAspectRatio', 'none');
    svg.setAttribute('class', 'pdf-viewer-svg');
    pageContainer.appendChild(svg);

    this.viewerContainer.appendChild(pageContainer);
    this.pageInfo[page.pageNumber - 1] = {
        pdfPage: page,
        container: pageContainer,
        canvas: canvas,
        canvasContext: canvasContext,
        svg: svg
    };

    this.createPageRecognizedValues(page.pageNumber);
}

PdfRecognitionViewer.prototype.onPageScroll = function (e) {
    for (let i = 0; i < this.pageInfo.length; i++) {
        this.pageBoundingClientRectsOnScrolling[i] = this.pageInfo[i].container.getBoundingClientRect();
    }

    if (!this.scrolling) {
        let that = this;

        window.requestAnimationFrame(function () {
            that.trackCurrentPageNum();
            that.scrolling = false;
        });

        this.scrolling = true;
    }
}

PdfRecognitionViewer.prototype.trackCurrentPageNum = function () {
    let newCurrentPageNum = this.currentPageNum;
    let viewerRect = this.viewerContainer.getBoundingClientRect();
    let maxVisiblePageHeight = 0;

    for (let i = 0; i < this.pageBoundingClientRectsOnScrolling.length; i++) {
        let pageRect = this.pageBoundingClientRectsOnScrolling[i];

        if (pageRect.top > viewerRect.bottom || pageRect.bottom < viewerRect.top) {
            continue;
        }

        let pageTop = pageRect.top <= viewerRect.top ? viewerRect.top : pageRect.top;
        let pageBottom = pageRect.bottom >= viewerRect.bottom ? viewerRect.bottom : pageRect.bottom;
        let visibleHeight = pageBottom - pageTop;

        if (visibleHeight > maxVisiblePageHeight) {
            maxVisiblePageHeight = visibleHeight;
            newCurrentPageNum = i + 1;
        }
    }

    if (newCurrentPageNum !== this.currentPageNum) {
        this.currentPageNum = newCurrentPageNum;
        this.currentPageSpan.textContent = this.currentPageNum;
    }
}

PdfRecognitionViewer.prototype.renderPdf = function () {
    let pdfViewer = this;

    this.pdfjsLib.getDocument(this.fileUrl).promise.then(function (pdfDoc) {
        pdfViewer.pagesCountSpan.textContent = pdfDoc.numPages;

        let initialScale = null;

        for (let i = 1; i <= pdfDoc.numPages; i++) {
            pdfDoc.getPage(i).then(function (page) {
                if (initialScale === null) {
                    let view = page.view;
                    let width = view[2];
                    let height = view[3];
                    let widthScale = pdfViewer.viewerContainer.clientWidth / width;
                    let heightScale = pdfViewer.viewerContainer.clientHeight / height;

                    initialScale = Math.max(widthScale, heightScale) - (pdfViewer.scaleStep / 2);
                    pdfViewer.scale = initialScale;
                    pdfViewer.updateScaleText();
                }

                pdfViewer.createPageControls(page);
                pdfViewer.renderPage(i);

                if (i === pdfDoc.numPages) {
                    pdfViewer.recognitionCorrection.updateFeedback(true);
                }
            });
        }
    });
};

PdfRecognitionViewer.prototype.createPageRecognizedValues = function (pageNumber) {
    if (!this.recognizedData || !this.recognizedData.documents || !this.recognizedData.documents[0]) {
        return;
    }

    let document = this.recognizedData.documents[0];
    let pageIndex = pageNumber - 1;

    if (document.fields) {
        for (let fieldName in document.fields) {
            let field = document.fields[fieldName];

            if (field === null) {
                continue;
            }

            this.createFieldRecognizedValues(field, fieldName, null, pageIndex);
        }
    }

    if (!document.details || !document.details.value || !document.details.value.length) {
        return;
    }

    for (let i = 0; i < document.details.value.length; i++) {
        let detail = document.details.value[i];

        if (detail === null || !detail.fields) {
            continue;
        }

        for (let fieldName in detail.fields) {
            let field = detail.fields[fieldName];

            if (field === null) {
                continue;
            }

            this.createFieldRecognizedValues(field, fieldName, i, pageIndex);
        }
    }
}

PdfRecognitionViewer.prototype.createFieldRecognizedValues = function (field, fieldName, rowIndex, pageIndex) {
    if (!this.recognizedData) {
        return;
    }

    let recognizedValue = new RecognizedValue(field, fieldName, rowIndex, this.recognizedData);

    if (recognizedValue.pageIndex !== pageIndex) {
        return;
    }

    this.pageInfo[pageIndex].svg.appendChild(recognizedValue.svgRectangle);
    this.recognitionCorrection.trackRecognizedValue(recognizedValue);
}

PdfRecognitionViewer.prototype.renderPage = function (num) {
    let pageInfo = this.pageInfo[num - 1];
    let page = pageInfo.pdfPage;
    let viewport = page.getViewport({ scale: this.scale });
    let fixedHeight = viewport.height.toFixed(0);
    let fixedWidth = viewport.width.toFixed(0);

    pageInfo.canvas.height = fixedHeight;
    pageInfo.canvas.width = fixedWidth;

    pageInfo.svg.setAttribute('height', fixedHeight);
    pageInfo.svg.setAttribute('width', fixedWidth);

    var renderContext = {
        canvasContext: pageInfo.canvasContext,
        viewport: viewport
    };
    var renderTask = page.render(renderContext);

    renderTask.promise.then(function () {
    });
}

PdfRecognitionViewer.prototype.onZoomOut = function (pdfViewer) {
    if (pdfViewer.scale <= pdfViewer.minScale) {
        return;
    }

    pdfViewer.scale -= pdfViewer.scaleStep;
    pdfViewer.rescalePages();
}

PdfRecognitionViewer.prototype.onZoomIn = function (pdfViewer) {
    if (pdfViewer.scale >= pdfViewer.maxScale) {
        return;
    }

    pdfViewer.scale += pdfViewer.scaleStep;
    pdfViewer.rescalePages();
}

PdfRecognitionViewer.prototype.updateScaleText = function () {
    let percent = (this.scale * 100).toFixed(0);

    this.scaleSpan.textContent = percent;
}

PdfRecognitionViewer.prototype.rescalePages = function () {
    this.updateScaleText();

    for (let i = 1; i <= this.pageInfo.length; i++) {
        this.renderPage(i);
    }
}

PdfRecognitionViewer.prototype.onPrevPage = function (pdfViewer) {
    if (pdfViewer.currentPageNum === 1) {
        return;
    }

    pdfViewer.currentPageNum--;
    pdfViewer.switchPage();
}

PdfRecognitionViewer.prototype.onNextPage = function (pdfViewer) {
    if (pdfViewer.currentPageNum === pdfViewer.pageInfo.length) {
        return;
    }

    pdfViewer.currentPageNum++;
    pdfViewer.switchPage();
}

PdfRecognitionViewer.prototype.switchPage = function () {
    let pageContainer = this.pageInfo[this.currentPageNum - 1].container;

    pageContainer.scrollIntoView({ behavior: 'smooth' });
}
