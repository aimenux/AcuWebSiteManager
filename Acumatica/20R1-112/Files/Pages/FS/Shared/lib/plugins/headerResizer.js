Ext.define('Override.HeaderResizer', {
    override: 'Ext.grid.plugin.HeaderResizer',

    getConstrainRegion: function() {
        var me = this,
            dragHdEl = me.dragHd.el,
            rightAdjust = 0,
            nextHd,
            lockedGrid,
            columnsResize,
            maxColWidth = me.headerCt.getWidth() - me.headerCt.visibleColumnManager.getColumns().length * me.minColWidth;

        if (!me.markerOwner.columnsResize) {
            columnsResize = 1.25;
        } else {
            columnsResize = me.markerOwner.columnsResize;
        }
        // *********************************
        // this is to get around EXTJS-16775
        // see https://www.sencha.com/forum/showthread.php?298039
        // if you have too many cols in too small a space         
        // the above calculation can be negative - which messes up the resize operation
        // cols jump around and sometimes disappear
        if (maxColWidth < me.minColWidth) {
            // let it grow by 25%
            var currWidth = me.dragHd.getWidth();
            maxColWidth = Math.floor(currWidth * columnsResize);
        }
        // *********************************

        // If forceFit, then right constraint is based upon not being able to force the next header
        // beyond the minColWidth. If there is no next header, then the header may not be expanded.
        if (me.headerCt.forceFit) {
            nextHd = me.dragHd.nextNode('gridcolumn:not([hidden]):not([isGroupHeader])');
            if (nextHd && me.headerInSameGrid(nextHd)) {
                rightAdjust = nextHd.getWidth() - me.minColWidth;
            }
        }

        // If resize header is in a locked grid, the maxWidth has to be 30px within the available locking grid's width
        else if ((lockedGrid = me.dragHd.up('tablepanel')).isLocked) {
            rightAdjust = me.dragHd.up('[scrollerOwner]').getTargetEl().getWidth() - lockedGrid.getWidth() - (lockedGrid.ownerLockable.normalGrid.visibleColumnManager.getColumns().length * me.minColWidth + Ext.getScrollbarSize().width);
        }

        // Else ue our default max width
        else {
            rightAdjust = maxColWidth - dragHdEl.getWidth();
        }

        return me.adjustConstrainRegion(
            dragHdEl.getRegion(),
            0,
            rightAdjust - me.xDelta,
            0,
            me.minColWidth - me.xDelta
        );
    }
});