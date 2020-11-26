(function(f, define){
    define([ "./kendo.data", "./kendo.editable", "./kendo.window", "./kendo.filtermenu", "./kendo.columnmenu", "./kendo.groupable", "./kendo.pager", "./kendo.selectable", "./kendo.sortable", "./kendo.reorderable", "./kendo.resizable", "./kendo.mobile.actionsheet", "./kendo.mobile.pane" ], f);
})(function(){

var __meta__ = {
    id: "grid",
    name: "Grid",
    category: "web",
    description: "The Grid widget displays tabular data and offers rich support for interacting with data,including paging, sorting, grouping, and selection.",
    depends: [ "data" ],
    features: [ {
        id: "grid-editing",
        name: "Editing",
        description: "Support for record editing",
        depends: [ "editable", "window" ]
    }, {
        id: "grid-filtering",
        name: "Filtering",
        description: "Support for record filtering",
        depends: [ "filtermenu" ]
    }, {
        id: "grid-columnmenu",
        name: "Column menu",
        description: "Support for header column menu",
        depends: [ "columnmenu" ]
    }, {
        id: "grid-grouping",
        name: "Grouping",
        description: "Support for grid grouping",
        depends: [ "groupable" ]
    }, {
        id: "grid-paging",
        name: "Paging",
        description: "Suppot for grid paging",
        depends: [ "pager" ]
    }, {
        id: "grid-selection",
        name: "Selection",
        description: "Support for row selection",
        depends: [ "selectable" ]
    }, {
        id: "grid-column-reorder",
        name: "Column reordering",
        description: "Support for column reordering",
        depends: [ "reorderable" ]
    }, {
        id: "grid-column-resize",
        name: "Column resizing",
        description: "Support for column resizing",
        depends: [ "resizable" ]
    }, {
        id: "grid-mobile",
        name: "Grid adaptive rendering",
        description: "Support for adaptive rendering",
        depends: [ "mobile.actionsheet", "mobile.pane" ]
    } ]
};

/* jshint eqnull: true */
(function($, undefined) {
    var kendo = window.kendo,
        ui = kendo.ui,
        DataSource = kendo.data.DataSource,
        Groupable = ui.Groupable,
        tbodySupportsInnerHtml = kendo.support.tbodyInnerHtml,
        activeElement = kendo._activeElement,
        Widget = ui.Widget,
        keys = kendo.keys,
        isPlainObject = $.isPlainObject,
        extend = $.extend,
        map = $.map,
        grep = $.grep,
        isArray = $.isArray,
        inArray = $.inArray,
        push = Array.prototype.push,
        proxy = $.proxy,
        isFunction = kendo.isFunction,
        isEmptyObject = $.isEmptyObject,
        math = Math,
        PROGRESS = "progress",
        ERROR = "error",
        DATA_CELL = ":not(.k-group-cell):not(.k-hierarchy-cell):visible",
        SELECTION_CELL_SELECTOR = "tbody>tr:not(.k-grouping-row):not(.k-detail-row):not(.k-group-footer) > td:not(.k-group-cell):not(.k-hierarchy-cell)",
        NAVROW = "tr:not(.k-footer-template):visible",
        NAVCELL = ":not(.k-group-cell):not(.k-hierarchy-cell):visible",
        FIRSTNAVITEM = NAVROW + ":first>" + NAVCELL + ":first",
        HEADERCELLS = "th.k-header:not(.k-group-cell):not(.k-hierarchy-cell)",
        NS = ".kendoGrid",
        EDIT = "edit",
        SAVE = "save",
        REMOVE = "remove",
        DETAILINIT = "detailInit",
        FILTERMENUINIT = "filterMenuInit",
        COLUMNMENUINIT = "columnMenuInit",
        CHANGE = "change",
        COLUMNHIDE = "columnHide",
        COLUMNSHOW = "columnShow",
        SAVECHANGES = "saveChanges",
        DATABOUND = "dataBound",
        DETAILEXPAND = "detailExpand",
        DETAILCOLLAPSE = "detailCollapse",
        FOCUSED = "k-state-focused",
        SELECTED = "k-state-selected",
        COLUMNRESIZE = "columnResize",
        COLUMNREORDER = "columnReorder",
        COLUMNLOCK = "columnLock",
        COLUMNUNLOCK = "columnUnlock",
        CLICK = "click",
        HEIGHT = "height",
        TABINDEX = "tabIndex",
        FUNCTION = "function",
        STRING = "string",
        DELETECONFIRM = "Are you sure you want to delete this record?",
        CONFIRMDELETE = "Delete",
        CANCELDELETE = "Cancel",
        formatRegExp = /(\}|\#)/ig,
        templateHashRegExp = /#/ig,
        whitespaceRegExp = "[\\x20\\t\\r\\n\\f]",
        nonDataCellsRegExp = new RegExp("(^|" + whitespaceRegExp + ")" + "(k-group-cell|k-hierarchy-cell)" + "(" + whitespaceRegExp + "|$)"),
        COMMANDBUTTONTMPL = '<a class="k-button k-button-icontext #=className#" #=attr# href="\\#"><span class="#=iconClass# #=imageClass#"></span>#=text#</a>',
        DIR = "dir",
        ASC = "asc",
        SINGLE = "single",
        FIELD = "field",
        DESC = "desc",
        sorterNS = ".kendoSorter",
        TLINK = ".k-link",
        ARIASORT = "aria-sort",
        isRtl = false,
        browser = kendo.support.browser,
        isIE7 = browser.msie && browser.version == 7,
        isIE8 = browser.msie && browser.version == 8;

    var VirtualScrollable =  Widget.extend({
        init: function(element, options) {
            var that = this;

            Widget.fn.init.call(that, element, options);
            that._refreshHandler = proxy(that.refresh, that);
            that.setDataSource(options.dataSource);
            that.wrap();
        },

        setDataSource: function(dataSource) {
            var that = this;
            if (that.dataSource) {
                that.dataSource.unbind(CHANGE, that._refreshHandler);
            }
            that.dataSource = dataSource;
            that.dataSource.bind(CHANGE, that._refreshHandler);
        },

        options: {
            name: "VirtualScrollable",
            itemHeight: $.noop
        },

        destroy: function() {
            var that = this;

            Widget.fn.destroy.call(that);

            that.dataSource.unbind(CHANGE, that._refreshHandler);
            that.wrapper.add(that.verticalScrollbar).off(NS);

            if (that.drag) {
                that.drag.destroy();
                that.drag = null;
            }
            that.wrapper = that.element = that.verticalScrollbar = null;
            that._refreshHandler = null;
        },

        wrap: function() {
            var that = this,
                // workaround for IE issue where scroll is not raised if container is same width as the scrollbar
                scrollbar = kendo.support.scrollbar() + 1,
                element = that.element,
                wrapper;

            element.css( {
                width: "auto",
                overflow: "hidden"
            }).css((isRtl ? "padding-left" : "padding-right"), scrollbar);
            that.content = element.children().first();
            wrapper = that.wrapper = that.content.wrap('<div class="k-virtual-scrollable-wrap"/>')
                                .parent()
                                .bind("DOMMouseScroll" + NS + " mousewheel" + NS, proxy(that._wheelScroll, that));

            if (kendo.support.kineticScrollNeeded) {
                that.drag = new kendo.UserEvents(that.wrapper, {
                    global: true,
                    move: function(e) {
                        that.verticalScrollbar.scrollTop(that.verticalScrollbar.scrollTop() - e.y.delta);
                        wrapper.scrollLeft(wrapper.scrollLeft() - e.x.delta);
                        e.preventDefault();
                    }
                });
            }

            that.verticalScrollbar = $('<div class="k-scrollbar k-scrollbar-vertical" />')
                                        .css({
                                            width: scrollbar
                                        }).appendTo(element)
                                        .bind("scroll" + NS, proxy(that._scroll, that));
        },

        _wheelScroll: function(e) {
            var scrollTop = this.verticalScrollbar.scrollTop(),
                delta = kendo.wheelDeltaY(e);

            if (delta) {
                e.preventDefault();
                this.verticalScrollbar.scrollTop(scrollTop + (-delta));
            }
        },

        _scroll: function(e) {
            var that = this,
                scrollTop = e.currentTarget.scrollTop,
                dataSource = that.dataSource,
                rowHeight = that.itemHeight,
                skip = dataSource.skip() || 0,
                start = that._rangeStart || skip,
                height = that.element.innerHeight(),
                isScrollingUp = !!(that._scrollbarTop && that._scrollbarTop > scrollTop),
                firstItemIndex = math.max(math.floor(scrollTop / rowHeight), 0),
                lastItemIndex = math.max(firstItemIndex + math.floor(height / rowHeight), 0);

            that._scrollTop = scrollTop - (start * rowHeight);
            that._scrollbarTop = scrollTop;

            if (!that._fetch(firstItemIndex, lastItemIndex, isScrollingUp)) {
                that.wrapper[0].scrollTop = that._scrollTop;
            }
        },

        _fetch: function(firstItemIndex, lastItemIndex, scrollingUp) {
            var that = this,
                dataSource = that.dataSource,
                itemHeight = that.itemHeight,
                take = dataSource.take(),
                rangeStart = that._rangeStart || dataSource.skip() || 0,
                currentSkip = math.floor(firstItemIndex / take) * take,
                fetching = false,
                prefetchAt = 0.33;

            if (firstItemIndex < rangeStart) {

                fetching = true;
                rangeStart = math.max(0, lastItemIndex - take);
                that._scrollTop = (firstItemIndex - rangeStart) * itemHeight;
                that._page(rangeStart, take);

            } else if (lastItemIndex >= rangeStart + take && !scrollingUp) {

                fetching = true;
                rangeStart = firstItemIndex;
                that._scrollTop = itemHeight;
                that._page(rangeStart, take);

            } else if (!that._fetching) {

                if (firstItemIndex < (currentSkip + take) - take * prefetchAt && firstItemIndex > take) {
                    dataSource.prefetch(currentSkip - take, take);
                }
                if (lastItemIndex > currentSkip + take * prefetchAt) {
                    dataSource.prefetch(currentSkip + take, take);
                }

            }
            return fetching;
        },

        _page: function(skip, take) {
            var that = this,
                dataSource = that.dataSource;

            clearTimeout(that._timeout);
            that._fetching = true;
            that._rangeStart = skip;

            if (dataSource.inRange(skip, take)) {
                dataSource.range(skip, take);
            } else {
                kendo.ui.progress(that.wrapper.parent(), true);
                that._timeout = setTimeout(function() {
                    dataSource.range(skip, take);
                }, 100);
            }
        },

        refresh: function() {
            var that = this,
                html = "",
                maxHeight = 250000,
                dataSource = that.dataSource,
                rangeStart = that._rangeStart,
                scrollbar = !kendo.support.kineticScrollNeeded ? kendo.support.scrollbar() : 0,
                wrapperElement = that.wrapper[0],
                totalHeight,
                idx,
                itemHeight;

            kendo.ui.progress(that.wrapper.parent(), false);
            clearTimeout(that._timeout);

            itemHeight = that.itemHeight = that.options.itemHeight() || 0;

            var addScrollBarHeight = (wrapperElement.scrollWidth > wrapperElement.offsetWidth) ? scrollbar : 0;

            totalHeight = dataSource.total() * itemHeight + addScrollBarHeight;

            for (idx = 0; idx < math.floor(totalHeight / maxHeight); idx++) {
                html += '<div style="width:1px;height:' + maxHeight + 'px"></div>';
            }

            if (totalHeight % maxHeight) {
                html += '<div style="width:1px;height:' + (totalHeight % maxHeight) + 'px"></div>';
            }

            that.verticalScrollbar.html(html);
            wrapperElement.scrollTop = that._scrollTop;

            if (that.drag) {
                that.drag.cancel();
            }

            if (rangeStart && !that._fetching) { // we are rebound from outside local range should be reset
                that._rangeStart = dataSource.skip();

                if (dataSource.page() === 1) {// reset the scrollbar position if datasource is filtered
                    that.verticalScrollbar[0].scrollTop = 0;
                }
            }
            that._fetching = false;
        }
    });

    function groupCells(count) {
        return new Array(count + 1).join('<td class="k-group-cell">&nbsp;</td>');
    }

    function stringifyAttributes(attributes) {
        var attr,
            result = " ";

        if (attributes) {
            if (typeof attributes === STRING) {
                return attributes;
            }

            for (attr in attributes) {
                result += attr + '="' + attributes[attr] + '"';
            }
        }
        return result;
    }

    var defaultCommands = {
        create: {
            text: "Add new record",
            imageClass: "k-i-add",
            className: "k-grid-add",
            iconClass: "k-icon"
        },
        cancel: {
            text: "Cancel changes",
            imageClass: "k-i-cancel",
            className: "k-grid-cancel-changes",
            iconClass: "k-icon"
        },
        save: {
            text: "Save changes",
            imageClass: "k-i-check",
            className: "k-grid-save-changes",
            iconClass: "k-icon"
        },
        destroy: {
            text: "Delete",
            imageClass: "k-i-delete",
            className: "k-grid-delete",
            iconClass: "k-icon"
        },
        edit: {
            text: "Edit",
            imageClass: "k-i-edit",
            className: "k-grid-edit",
            iconClass: "k-icon"
        },
        update: {
            text: "Update",
            imageClass: "k-i-check",
            className: "k-grid-update",
            iconClass: "k-icon"
        },
        canceledit: {
            text: "Cancel",
            imageClass: "k-i-cancel",
            className: "k-grid-cancel",
            iconClass: "k-icon"
        }
    };

    function heightAboveHeader(context) {
        var top = 0;
        $('> .k-grouping-header, > .k-grid-toolbar', context).each(function () {
            top += this.offsetHeight;
        });
        return top;
    }

    function cursor(context, value) {
        $('th, th .k-grid-filter, th .k-link', context)
            .add(document.body)
            .css('cursor', value);
    }

    function buildEmptyAggregatesObject(aggregates) {
            var idx,
                length,
                aggregate = {},
                fieldsMap = {};

            if (!isEmptyObject(aggregates)) {
                if (!isArray(aggregates)){
                    aggregates = [aggregates];
                }

                for (idx = 0, length = aggregates.length; idx < length; idx++) {
                    aggregate[aggregates[idx].aggregate] = 0;
                    fieldsMap[aggregates[idx].field] = aggregate;
                }
            }

            return fieldsMap;
    }

    function reorder(selector, source, dest, before) {
        source = selector.eq(source);

        if (typeof dest == "number") {
            source[before ? "insertBefore" : "insertAfter"](selector.eq(dest));
        } else {
            source.appendTo(dest);
        }
    }

    function elements(lockedContent, content, filter) {
        return $(lockedContent).add(content).find(filter);
    }

    function attachCustomCommandEvent(context, container, commands) {
        var idx,
            length,
            command,
            commandName;

        commands = !isArray(commands) ? [commands] : commands;

        for (idx = 0, length = commands.length; idx < length; idx++) {
            command = commands[idx];

            if (isPlainObject(command) && command.click) {
                commandName = command.name || command.text;
                container.on(CLICK + NS, "a.k-grid-" + (commandName || "").replace(/\s/g, ""), { commandName: commandName }, proxy(command.click, context));
            }
        }
    }

    function visibleColumns(columns) {
        return grep(columns, function(column) {
            return !column.hidden;
        });
    }

    function columnsWidth(cols) {
        var colWidth, width = 0;

        for (var idx = 0, length = cols.length; idx < length; idx++) {
            colWidth = cols[idx].style.width;
            if (colWidth && colWidth.indexOf("%") == -1) {
                width += parseInt(colWidth, 10);
            }
        }

        return width;
    }

    function lockedColumns(columns) {
        return grep(columns, function(column) {
            return column.locked;
        });
    }

    function nonLockedColumns(columns) {
        return grep(columns, function(column) {
            return !column.locked;
        });
    }

    function visibleNonLockedColumns(columns) {
        return grep(columns, function(column) {
            return !column.locked && !column.hidden;
        });
    }

    function visibleLockedColumns(columns) {
        return grep(columns, function(column) {
            return column.locked && !column.hidden;
        });
    }

    function appendContent(tbody, table, html) {
        var placeholder,
            tmp = tbody;

        if (tbodySupportsInnerHtml) {
            tbody[0].innerHTML = html;
        } else {
            placeholder = document.createElement("div");
            placeholder.innerHTML = "<table><tbody>" + html + "</tbody></table>";
            tbody = placeholder.firstChild.firstChild;
            table[0].replaceChild(tbody, tmp[0]);
            tbody = $(tbody);
        }
        return tbody;
    }

    function addHiddenStyle(attr) {
        attr = attr || {};
        var style = attr.style;

        if(!style) {
            style = "display:none";
        } else {
            style = style.replace(/((.*)?display)(.*)?:([^;]*)/i, "$1:none");
            if(style === attr.style) {
                style = style.replace(/(.*)?/i, "display:none;$1");
            }
        }

        return extend({}, attr, { style: style });
    }

    function removeHiddenStyle(attr) {
        attr = attr || {};
        var style = attr.style;

        if(style) {
            attr.style = style.replace(/(display\s*:\s*none\s*;?)*/ig, "");
        }

        return attr;
    }

    function normalizeCols(table, visibleColumns, hasDetails, groups) {
        var colgroup = table.find(">colgroup"),
            width,
            cols = map(visibleColumns, function(column) {
                    width = column.width;
                    if (width && parseInt(width, 10) !== 0) {
                        return kendo.format('<col style="width:{0}"/>', typeof width === STRING? width : width + "px");
                    }

                    return "<col />";
                });

        if (hasDetails || colgroup.find(".k-hierarchy-col").length) {
            cols.splice(0, 0, '<col class="k-hierarchy-col" />');
        }

        if (colgroup.length) {
            colgroup.remove();
        }

        colgroup = $(new Array(groups + 1).join('<col class="k-group-col">') + cols.join(""));
        if (!colgroup.is("colgroup")) {
            colgroup = $("<colgroup/>").append(colgroup);
        }

        table.prepend(colgroup);

        // fill gap after column hiding
        if (browser.msie && browser.version == 8) {
            table.css("display", "inline-table");
            window.setTimeout(function(){table.css("display", "");}, 1);
        }
    }

    function normalizeHeaderCells(th, columns) {
        var lastIndex = 0;
        var idx , len;

        for (idx = 0, len = columns.length; idx < len; idx ++) {
            if (columns[idx].locked) {
                th.eq(idx).insertBefore(th.eq(lastIndex));
                lastIndex ++;
            }
        }
    }

    function convertToObject(array) {
        var result = {},
            item,
            idx,
            length;

        for (idx = 0, length = array.length; idx < length; idx++) {
            item = array[idx];
            result[item.value] = item.text;
        }

        return result;
    }

    function formatGroupValue(value, format, columnValues) {
        var isForiegnKey = columnValues && columnValues.length && isPlainObject(columnValues[0]) && "value" in columnValues[0],
            groupValue = isForiegnKey ? convertToObject(columnValues)[value] : value;

        groupValue = groupValue != null ? groupValue : "";

        return format ? kendo.format(format, groupValue) : groupValue;
    }

    function setCellVisibility(cells, index, visible) {
        var pad = 0,
            state,
            cell = cells[pad];

        while (cell) {
            state = visible ? true : cell.style.display !== "none";

            if (state && !nonDataCellsRegExp.test(cell.className) && --index < 0) {
                cell.style.display = visible ? "" : "none";
                break;
            }

            cell = cells[++pad];
        }
    }

    function hideColumnCells(rows, columnIndex) {
        var idx = 0,
            length = rows.length,
            cell, row;

        for ( ; idx < length; idx += 1) {
            row = rows.eq(idx);
            if (row.is(".k-grouping-row,.k-detail-row")) {
                cell = row.children(":not(.k-group-cell):first,.k-detail-cell").last();
                cell.attr("colspan", parseInt(cell.attr("colspan"), 10) - 1);
            } else {
                if (row.hasClass("k-grid-edit-row") && (cell = row.children(".k-edit-container")[0])) {
                    cell = $(cell);
                    cell.attr("colspan", parseInt(cell.attr("colspan"), 10) - 1);
                    cell.find("col").eq(columnIndex).remove();
                    row = cell.find("tr:first");
                }

                setCellVisibility(row[0].cells, columnIndex, false);
            }
        }
    }

    function showColumnCells(rows, columnIndex) {
        var idx = 0,
            length = rows.length,
            cell, row, columns;

        for ( ; idx < length; idx += 1) {
            row = rows.eq(idx);
            if (row.is(".k-grouping-row,.k-detail-row")) {
                cell = row.children(":not(.k-group-cell):first,.k-detail-cell").last();
                cell.attr("colspan", parseInt(cell.attr("colspan"), 10) + 1);
            } else {
                if (row.hasClass("k-grid-edit-row") && (cell = row.children(".k-edit-container")[0])) {
                    cell = $(cell);
                    cell.attr("colspan", parseInt(cell.attr("colspan"), 10) + 1);
                    normalizeCols(cell.find(">form>table"), visibleColumns(columns), false,  0);
                    row = cell.find("tr:first");
                }

                setCellVisibility(row[0].cells, columnIndex, true);
            }
        }
    }

    function updateColspan(toAdd, toRemove) {
        var item, idx, length;
        for (idx = 0, length = toAdd.length; idx < length; idx += 1) {
            item = toAdd.eq(idx).children().last();
            item.attr("colspan", parseInt(item.attr("colspan"), 10) + 1);

            item = toRemove.eq(idx).children().last();
            item.attr("colspan", parseInt(item.attr("colspan"), 10) - 1);
        }
    }

    function tableWidth(table) {
        var idx, length, width = 0;
        var cols = table.find(">colgroup>col");

        for (idx = 0, length = cols.length; idx < length; idx += 1) {
            width += parseInt(cols[idx].style.width, 10);
        }

        return width;
    }

    var Grid = Widget.extend({
        init: function(element, options) {
            var that = this;

            options = isArray(options) ? { dataSource: options } : options;

            Widget.fn.init.call(that, element, options);

            isRtl = kendo.support.isRtl(element);

            that._element();

            that._aria();

            that._columns(that.options.columns);

            that._dataSource();

            that._tbody();

            that._pageable();

            that._thead();

            that._groupable();

            that._toolbar();

            that._setContentHeight();

            that._templates();

            that._navigatable();

            that._selectable();

            that._details();

            that._editable();

            that._attachCustomCommandsEvent();

            if (that.options.autoBind) {
                that.dataSource.fetch();
            } else {
                that._footer();
            }

            if (that.lockedContent) {
                that.wrapper.addClass("k-grid-lockedcolumns");
                that._resizeHandler = function()  { that.resize(); };
                $(window).on("resize" + NS, that._resizeHandler);
            }

            kendo.notify(that);
        },

        events: [
           CHANGE,
           "dataBinding",
           "cancel",
           DATABOUND,
           DETAILEXPAND,
           DETAILCOLLAPSE,
           DETAILINIT,
           FILTERMENUINIT,
           COLUMNMENUINIT,
           EDIT,
           SAVE,
           REMOVE,
           SAVECHANGES,
           COLUMNRESIZE,
           COLUMNREORDER,
           COLUMNSHOW,
           COLUMNHIDE,
           COLUMNLOCK,
           COLUMNUNLOCK
        ],

        setDataSource: function(dataSource) {
            var that = this;

            that.options.dataSource = dataSource;

            that._dataSource();

            that._pageable();

            if (that.options.groupable) {
                that._groupable();
            }

            that._thead();

            if (that.virtualScrollable) {
                that.virtualScrollable.setDataSource(that.options.dataSource);
            }

            if (that.selectable) {
                that._selectable();
            }

            if (that.options.autoBind) {
                dataSource.fetch();
            }
        },

        options: {
            name: "Grid",
            columns: [],
            toolbar: null,
            autoBind: true,
            filterable: false,
            scrollable: true,
            sortable: false,
            selectable: false,
            navigatable: false,
            pageable: false,
            editable: false,
            groupable: false,
            rowTemplate: "",
            altRowTemplate: "",
            dataSource: {},
            height: null,
            resizable: false,
            reorderable: false,
            columnMenu: false,
            detailTemplate: null,
            columnResizeHandleWidth: 3,
            mobile: ""
        },

        destroy: function() {
            var that = this,
                element;

            Widget.fn.destroy.call(that);

            if (that._resizeHandler) {
                $(window).off("resize" + NS, that._resizeHandler);
            }

            if (that.pager && that.pager.element) {
                that.pager.destroy();
            }

            that.pager = null;

            if (that.groupable && that.groupable.element) {
                that.groupable.element.kendoGroupable("destroy");
            }

            that.groupable = null;

            if (that.options.reorderable) {
                that.wrapper.data("kendoReorderable").destroy();
            }

            if (that.selectable) {
                that.selectable.destroy();
            }

            if (that.resizable) {
                that.resizable.destroy();

                if (that._resizeUserEvents) {
                    if (that._resizeHandleDocumentClickHandler) {
                        $(document).off("click", that._resizeHandleDocumentClickHandler);
                    }
                    that._resizeUserEvents.destroy();
                    that._resizeUserEvents = null;
                }
                that.resizable = null;
            }

            if (that.virtualScrollable && that.virtualScrollable.element) {
                that.virtualScrollable.destroy();
            }

            that.virtualScrollable = null;

            that._destroyColumnAttachments();

            that._destroyEditable();

            if (that.dataSource) {
                that.dataSource.unbind(CHANGE, that._refreshHandler)
                           .unbind(PROGRESS, that._progressHandler)
                           .unbind(ERROR, that._errorHandler);

                that._refreshHandler = that._progressHandler = that._errorHandler = null;
            }

            element = that.element
                .add(that.wrapper)
                .add(that.table)
                .add(that.thead)
                .add(that.wrapper.find(">.k-grid-toolbar"));

            if (that.content) {
                element = element
                        .add(that.content)
                        .add(that.content.find(">.k-virtual-scrollable-wrap"));
            }

            if (that.lockedHeader) {
                that._removeLockedContainers();
            }

            if (that.pane) {
                that.pane.destroy();
            }

            if (that._draggableInstance && that._draggableInstance.element) {
                that._draggableInstance.destroy();
            }

            that._draggableInstance = null;

            element.off(NS);

            kendo.destroy(that.wrapper);

            that.scrollables =
            that.thead =
            that.tbody =
            that.element =
            that.table =
            that.content =
            that.footer =
            that.wrapper =
            that._groupableClickHandler =
            that._setContentWidthHandler = null;
        },

        setOptions: function(options) {
            var that = this;

            Widget.fn.setOptions.call(this, options);

            that._templates();
        },

        items: function() {
            return this.tbody.children().filter(function() {
                var tr = $(this);
                return !tr.hasClass("k-grouping-row") && !tr.hasClass("k-detail-row") && !tr.hasClass("k-group-footer");
            });
        },

        _destroyColumnAttachments: function() {
            var that = this;

            that.resizeHandle = null;

            if (!that.thead) {
                return;
            }

            that.thead.find("th").each(function(){
                var th = $(this),
                    filterMenu = th.data("kendoFilterMenu"),
                    sortable = th.data("kendoSorter"),
                    columnMenu = th.data("kendoColumnMenu");

                if (filterMenu) {
                    filterMenu.destroy();
                }

                if (sortable) {
                    sortable.destroy();
                }

                if (columnMenu) {
                    columnMenu.destroy();
                }
            });
        },

        _attachCustomCommandsEvent: function() {
            var that = this,
                columns = that.columns || [],
                command,
                idx,
                length;

            for (idx = 0, length = columns.length; idx < length; idx++) {
                command = columns[idx].command;

                if (command) {
                    attachCustomCommandEvent(that, that.wrapper, command);
                }
            }
        },

        _aria: function() {
            var id = this.element.attr("id") || "aria";

            if (id) {
                this._cellId = id + "_active_cell";
            }
        },

        _element: function() {
            var that = this,
                table = that.element;

            if (!table.is("table")) {
                if (that.options.scrollable) {
                    table = that.element.find("> .k-grid-content > table");
                } else {
                    table = that.element.children("table");
                }

                if (!table.length) {
                    table = $("<table />").appendTo(that.element);
                }
            }

            if (isIE7) {
                table.attr("cellspacing", 0);
            }

            that.table = table.attr("role", that._hasDetails() ? "treegrid" : "grid");

            that._wrapper();
        },

        _createResizeHandle: function(container, th) {
            var that = this;
            var indicatorWidth = that.options.columnResizeHandleWidth;
            var scrollable = that.options.scrollable;
            var resizeHandle = that.resizeHandle;
            var left;

            if (resizeHandle && that.lockedContent && resizeHandle.data("th")[0] !== th[0]) {
                resizeHandle.remove();
                resizeHandle = null;
            }

            if (!resizeHandle) {
                resizeHandle = that.resizeHandle = $('<div class="k-resize-handle"><div class="k-resize-handle-inner"></div></div>');
                container.append(resizeHandle);
            }

            if (!isRtl) {
                left = th[0].offsetWidth;

                th.prevAll(":visible").each(function() {
                    left += this.offsetWidth;
                });
            } else {
                left = th.position().left;
                if (scrollable) {
                    var headerWrap = th.closest(".k-grid-header-wrap, .k-grid-header-locked"),
                        ieCorrection = browser.msie ? headerWrap.scrollLeft() : 0,
                        webkitCorrection = browser.webkit ? (headerWrap[0].scrollWidth - headerWrap[0].offsetWidth - headerWrap.scrollLeft()) : 0,
                        firefoxCorrection = browser.mozilla ? (headerWrap[0].scrollWidth - headerWrap[0].offsetWidth - (headerWrap[0].scrollWidth - headerWrap[0].offsetWidth - headerWrap.scrollLeft())) : 0;

                    left -= webkitCorrection - firefoxCorrection + ieCorrection;
                }
            }

            resizeHandle.css({
                top: scrollable ? 0 : heightAboveHeader(that.wrapper),
                left: left - indicatorWidth,
                height: th.outerHeight(),
                width: indicatorWidth * 3
            })
            .data("th", th)
            .show();
        },

        _positionColumnResizeHandle: function(container) {
            var that = this,
                indicatorWidth = that.options.columnResizeHandleWidth,
                lockedHead = that.lockedHeader ? that.lockedHeader.find("thead:first") : $();

            that.thead.add(lockedHead).on("mousemove" + NS, "th", function(e) {
                var th = $(this);

                if (th.hasClass("k-group-cell") || th.hasClass("k-hierarchy-cell")) {
                    return;
                }

                var clientX = e.clientX,
                    winScrollLeft = $(window).scrollLeft(),
                    position = th.offset().left + (!isRtl ? this.offsetWidth : 0);

                if(clientX + winScrollLeft > position - indicatorWidth && clientX + winScrollLeft < position + indicatorWidth) {
                    that._createResizeHandle(th.closest("div"), th);
                } else if (that.resizeHandle) {
                    that.resizeHandle.hide();
                } else {
                    cursor(that.wrapper, "");
                }
            });
        },

        _resizeHandleDocumentClick: function(e) {
            if ($(e.target).closest(".k-column-active").length) {
                return;
            }

            $(document).off(e);

            this._hideResizeHandle();
        },

        _hideResizeHandle: function() {
            if (this.resizeHandle) {
                this.resizeHandle.data("th")
                    .removeClass("k-column-active");

                if (this.lockedContent && !this._isMobile) {
                    this.resizeHandle.remove();
                    this.resizeHandle = null;
                } else {
                    this.resizeHandle.hide();
                }
            }
        },

        _positionColumnResizeHandleTouch: function(container) {
            var that = this,
                lockedHead = that.lockedHeader ? that.lockedHeader.find("thead:first") : $();

            that._resizeUserEvents = new kendo.UserEvents(lockedHead.add(that.thead), {
                filter: "th:not(.k-group-cell):not(.k-hierarchy-cell)",
                threshold: 10,
                hold: function(e) {
                    var th = $(e.target);

                    e.preventDefault();

                    th.addClass("k-column-active");
                    that._createResizeHandle(th.closest("div"), th);

                    if (!that._resizeHandleDocumentClickHandler) {
                        that._resizeHandleDocumentClickHandler = proxy(that._resizeHandleDocumentClick, that);
                    }

                    $(document).on("click", that._resizeHandleDocumentClickHandler);
                }
            });
        },

        _resizable: function() {
            var that = this,
                options = that.options,
                container,
                columnStart,
                columnWidth,
                gridWidth,
                isMobile = this._isMobile,
                scrollbar = !kendo.support.mobileOS ? kendo.support.scrollbar() : 0,
                isLocked,
                col, th;

            if (options.resizable) {
                container = options.scrollable ? that.wrapper.find(".k-grid-header-wrap:first") : that.wrapper;

                if (isMobile) {
                    that._positionColumnResizeHandleTouch(container);
                } else {
                    that._positionColumnResizeHandle(container);
                }

                if (that.resizable) {
                    that.resizable.destroy();
                }

                that.resizable = new ui.Resizable(container.add(that.lockedHeader), {
                    handle: ".k-resize-handle",
                    hint: function(handle) {
                        return $('<div class="k-grid-resize-indicator" />').css({
                            height: handle.data("th").outerHeight() + that.tbody.attr("clientHeight")
                        });
                    },
                    start: function(e) {
                        th = $(e.currentTarget).data("th");

                        if (isMobile) {
                            that._hideResizeHandle();
                        }

                        var index = $.inArray(th[0], th.parent().children(":visible")),
                            header = th.closest("table");

                        isLocked = header.parent().hasClass("k-grid-header-locked");

                        var contentTable =  isLocked ? that.lockedTable : that.table,
                            footer = that.footer || $();

                        if (that.footer && that.lockedContent) {
                            footer = isLocked ? that.footer.children(".k-grid-footer-locked") : that.footer.children(".k-grid-footer-wrap");
                        }

                        cursor(that.wrapper, 'col-resize');

                        if (options.scrollable) {
                            col = header.find("col:eq(" + index + ")")
                                .add(contentTable.children("colgroup").find("col:eq(" + index + ")"))
                                .add(footer.find("colgroup").find("col:eq(" + index + ")"));
                        } else {
                            col = contentTable.children("colgroup").find("col:eq(" + index + ")");
                        }

                        columnStart = e.x.location;
                        columnWidth = th.outerWidth();
                        gridWidth = isLocked ? contentTable.children("tbody").outerWidth() : that.tbody.outerWidth(); // IE returns 0 if grid is empty and scrolling is enabled
                    },
                    resize: function(e) {
                        var rtlMultiplier = isRtl ? -1 : 1,
                            currentWidth = columnWidth + (e.x.location * rtlMultiplier) - (columnStart * rtlMultiplier);

                        if (options.scrollable) {
                            var footer = (isLocked ? that.lockedFooter.children("table") : that.footer.find(">.k-grid-footer-wrap>table")) || $();
                            var header = th.closest("table");
                            var contentTable = isLocked ? that.lockedTable : that.table;
                            var constrain = false;
                            var totalWidth = that.wrapper.width() - scrollbar;
                            var width = currentWidth;

                            if (isLocked && gridWidth - columnWidth + width > totalWidth) {
                                width = columnWidth + (totalWidth - gridWidth - scrollbar * 2);
                                if (width < 0) {
                                    width = currentWidth;
                                }
                                constrain = true;
                            }

                            if (width > 10) {
                                col.css('width', width);

                                if (gridWidth) {
                                    if (constrain) {
                                        width = totalWidth - scrollbar * 2;
                                    } else {
                                        width = gridWidth + (e.x.location * rtlMultiplier) - (columnStart * rtlMultiplier);
                                    }

                                    contentTable
                                        .add(header)
                                        .add(footer)
                                        .css('width', width);

                                    if (!isLocked) {
                                        that._footerWidth = width;
                                    }
                                }
                            }
                        } else if (currentWidth > 10) {
                            col.css('width', currentWidth);
                        }
                    },
                    resizeend: function() {
                        var newWidth = th.outerWidth(),
                            column,
                            header;

                        cursor(that.wrapper, "");

                        if (columnWidth != newWidth) {
                            header = that.lockedHeader ? that.lockedHeader.find("thead:first").add(that.thead) : th.parent();

                            column = that.columns[header.find("th:not(.k-group-cell):not(.k-hierarchy-cell)").index(th)];

                            column.width = newWidth;

                            that.trigger(COLUMNRESIZE, {
                                column: column,
                                oldWidth: columnWidth,
                                newWidth: newWidth
                            });

                            that._applyLockedContainersWidth();
                            that._syncLockedContentHeight();
                            that._syncLockedHeaderHeight();
                        }

                        that._hideResizeHandle();
                        th = null;
                    }
                });

            }
        },

        _draggable: function() {
            var that = this;
            if (that.options.reorderable) {
                if (that._draggableInstance) {
                    that._draggableInstance.destroy();
                }

                that._draggableInstance = that.wrapper.kendoDraggable({
                    group: kendo.guid(),
                    filter: that.content ? ".k-grid-header:first " + HEADERCELLS : "table:first>.k-grid-header " + HEADERCELLS,
                    drag: function() {
                        that._hideResizeHandle();
                    },
                    hint: function(target) {
                        return $('<div class="k-header k-drag-clue" />')
                            .css({
                                width: target.width(),
                                paddingLeft: target.css("paddingLeft"),
                                paddingRight: target.css("paddingRight"),
                                lineHeight: target.height() + "px",
                                paddingTop: target.css("paddingTop"),
                                paddingBottom: target.css("paddingBottom")
                            })
                            .html(target.attr(kendo.attr("title")) || target.attr(kendo.attr("field")) || target.text())
                            .prepend('<span class="k-icon k-drag-status k-denied" />');
                    }
                }).data("kendoDraggable");
            }
        },

        _reorderable: function() {
            var that = this;
            if (that.options.reorderable) {
                if (that.wrapper.data("kendoReorderable")) {
                    that.wrapper.data("kendoReorderable").destroy();
                }

                that.wrapper.kendoReorderable({
                    draggable: that._draggableInstance,
                    dragOverContainers: function(index) {
                        return that.columns[index].lockable !== false;
                    },
                    inSameContainer: function(x, y) {
                        return $(x).parent()[0] === $(y).parent()[0];
                    },
                    change: function(e) {
                        var column = that.columns[e.oldIndex];

                        that.trigger(COLUMNREORDER, {
                            newIndex: e.newIndex,
                            oldIndex: inArray(column, that.columns),
                            column: column
                        });

                        that.reorderColumn(e.newIndex, column, e.position === "before");
                    }
                });
            }
        },

        reorderColumn: function(destIndex, column, before) {
            var that = this,
                columns = that.columns,
                sourceIndex = inArray(column, columns),
                destColumn = columns[destIndex],
                colSourceIndex = inArray(column, visibleColumns(columns)),
                colDest = inArray(destColumn, visibleColumns(columns)),
                headerCol = colDest,
                footerCol = colDest,
                lockedRows = $(),
                rows,
                idx,
                length,
                lockChanged,
                isLocked = !!destColumn.locked,
                lockedCount = lockedColumns(columns).length,
                footer = that.footer || that.wrapper.find(".k-grid-footer");

            if (sourceIndex === destIndex) {
                return;
            }

            if (!column.locked && isLocked && nonLockedColumns(columns).length == 1) {
                return;
            }

            if (column.locked && !isLocked && lockedCount == 1) {
                return;
            }

            if (destColumn.hidden) {
                if (isLocked) {
                    colDest = that.lockedTable.find("colgroup");
                    headerCol = that.lockedHeader.find("colgroup");
                    footerCol = $(that.lockedFooter).find(">table>colgroup");
                } else {
                    colDest = that.tbody.prev();
                    headerCol = that.thead.prev();
                    footerCol = footer.find(".k-grid-footer-wrap").find(">table>colgroup");
                }
            }

            lockChanged = !!column.locked;
            lockChanged = lockChanged != isLocked;
            column.locked = isLocked;

            that._hideResizeHandle();

            if (before === undefined) {
                before = destIndex < sourceIndex;
            }

            columns.splice(before ? destIndex : destIndex + 1, 0, column);
            columns.splice(sourceIndex < destIndex ? sourceIndex : sourceIndex + 1, 1);
            that._templates();

            reorder(elements(that.lockedHeader, that.thead.prev(), "col:not(.k-group-col,.k-hierarchy-col)"), colSourceIndex, headerCol, before);
            if (that.options.scrollable) {
                reorder(elements(that.lockedTable, that.tbody.prev(), "col:not(.k-group-col,.k-hierarchy-col)"), colSourceIndex, colDest, before);
            }

            reorder(elements(that.lockedHeader, that.thead, "th.k-header:not(.k-group-cell,.k-hierarchy-cell)"), sourceIndex, destIndex, before);

            if (footer && footer.length) {
                reorder(elements(that.lockedFooter, footer.find(".k-grid-footer-wrap"), ">table>colgroup>col:not(.k-group-col,.k-hierarchy-col)"), colSourceIndex, footerCol, before);
                reorder(footer.find(".k-footer-template>td:not(.k-group-cell,.k-hierarchy-cell)"), sourceIndex, destIndex, before);
            }

            rows = that.tbody.children(":not(.k-grouping-row,.k-detail-row)");
            if (that.lockedTable) {
                if (lockedCount > destIndex) {
                    if (lockedCount <= sourceIndex) {
                        updateColspan(
                            that.lockedTable.find(">tbody>tr.k-grouping-row"),
                            that.table.find(">tbody>tr.k-grouping-row")
                        );
                    }
                } else if (lockedCount > sourceIndex) {
                    updateColspan(
                        that.table.find(">tbody>tr.k-grouping-row"),
                        that.lockedTable.find(">tbody>tr.k-grouping-row")
                    );
                }

                lockedRows = that.lockedTable.find(">tbody>tr:not(.k-grouping-row,.k-detail-row)");
            }

            for (idx = 0, length = rows.length; idx < length; idx += 1) {
                reorder(elements(lockedRows[idx], rows[idx], ">td:not(.k-group-cell,.k-hierarchy-cell)"), sourceIndex, destIndex, before);
            }

            that._updateTablesWidth();
            that._applyLockedContainersWidth();
            that._syncLockedContentHeight();

            if(!lockChanged) {
                return;
            }

            if (isLocked) {
                that.trigger(COLUMNLOCK, {
                    column: column
                });
            } else {
                that.trigger(COLUMNUNLOCK, {
                    column: column
                });
            }
        },

        lockColumn: function(column) {
            var columns = this.columns;

            if (typeof column == "number") {
                column = columns[column];
            } else {
                column = grep(columns, function(item) {
                    return item.field === column;
                })[0];
            }

            if (!column || column.locked || column.hidden) {
                return;
            }

            var index = lockedColumns(columns).length - 1;
            this.reorderColumn(index, column, false);
        },

        unlockColumn: function(column) {
            var columns = this.columns;

            if (typeof column == "number") {
                column = columns[column];
            } else {
                column = grep(columns, function(item) {
                    return item.field === column;
                })[0];
            }

            if (!column || !column.locked || column.hidden) {
                return;
            }

            var index = lockedColumns(columns).length;
            this.reorderColumn(index, column, true);
        },

        cellIndex: function(td) {
            var lockedColumnOffset = 0;

            if (this.lockedTable && !$.contains(this.lockedTable[0], td[0])) {
                lockedColumnOffset = lockedColumns(this.columns).length;
            }

            return $(td).parent().children('td:not(.k-group-cell,.k-hierarchy-cell)').index(td) + lockedColumnOffset;
        },

        _modelForContainer: function(container) {
            container = $(container);

            if (!container.is("tr") && this._editMode() !== "popup") {
                container = container.closest("tr");
            }

            var id = container.attr(kendo.attr("uid"));

            return this.dataSource.getByUid(id);
        },

        _editable: function() {
            var that = this,
                selectable = that.selectable && that.selectable.options.multiple,
                editable = that.options.editable,
                handler = function () {
                    var target = activeElement(),
                        cell = that._editContainer;

                    if (cell && !$.contains(cell[0], target) && cell[0] !== target && !$(target).closest(".k-animation-container").length) {
                        if (that.editable.end()) {
                            that.closeCell();
                        }
                    }
                };

            if (editable) {
                var mode = that._editMode();
                if (mode === "incell") {
                    if (editable.update !== false) {
                        that.wrapper.on(CLICK + NS, "tr:not(.k-grouping-row) > td", function(e) {
                            var td = $(this),
                                isLockedCell = that.lockedTable && td.closest("table")[0] === that.lockedTable[0];

                            if (td.hasClass("k-hierarchy-cell") ||
                                td.hasClass("k-detail-cell") ||
                                td.hasClass("k-group-cell") ||
                                td.hasClass("k-edit-cell") ||
                                td.has("a.k-grid-delete").length ||
                                td.has("button.k-grid-delete").length ||
                                (td.closest("tbody")[0] !== that.tbody[0] && !isLockedCell) ||
                                $(e.target).is(":input")) {
                                return;
                            }

                            if (that.editable) {
                                if (that.editable.end()) {
                                    if (selectable) {
                                        $(activeElement()).blur();
                                    }
                                    that.closeCell();
                                    that.editCell(td);
                                }
                            } else {
                                that.editCell(td);
                            }

                        })
                        .on("focusin" + NS, function() {
                            clearTimeout(that.timer);
                            that.timer = null;
                        })
                        .on("focusout" + NS, function() {
                            that.timer = setTimeout(handler, 1);
                        });
                    }
                } else {
                    if (editable.update !== false) {
                        that.wrapper.on(CLICK + NS, "tbody>tr:not(.k-detail-row,.k-grouping-row):visible a.k-grid-edit", function(e) {
                            e.preventDefault();
                            that.editRow($(this).closest("tr"));
                        });
                    }
                }

                if (editable.destroy !== false) {
                    that.wrapper.on(CLICK + NS, "tbody>tr:not(.k-detail-row,.k-grouping-row):visible .k-grid-delete", function(e) {
                        e.preventDefault();
                        e.stopPropagation();
                        that.removeRow($(this).closest("tr"));
                    });
                } else {
                    //Required for the MVC server wrapper delete button
                    that.wrapper.on(CLICK + NS, "tbody>tr:not(.k-detail-row,.k-grouping-row):visible button.k-grid-delete", function(e) {
                        e.stopPropagation();

                        if (!that._confirmation()) {
                            e.preventDefault();
                        }
                    });
                }
            }
        },

        editCell: function(cell) {
            cell = $(cell);

            var that = this,
                column = that.columns[that.cellIndex(cell)],
                model = that._modelForContainer(cell);

            if (model && (!model.editable || model.editable(column.field)) && !column.command && column.field) {

                that._attachModelChange(model);

                that._editContainer = cell;

                that.editable = cell.addClass("k-edit-cell")
                    .kendoEditable({
                        fields: { field: column.field, format: column.format, editor: column.editor, values: column.values },
                        model: model,
                        change: function(e) {
                            if (that.trigger(SAVE, { values: e.values, container: cell, model: model } )) {
                                e.preventDefault();
                            }
                        }
                    }).data("kendoEditable");

                var tr = cell.parent().addClass("k-grid-edit-row");

                if (that.lockedContent) {
                    adjustRowHeight(tr[0], that._relatedRow(tr).addClass("k-grid-edit-row")[0]);
                }

                that.trigger(EDIT, { container: cell, model: model });
            }
        },

        _adjustLockedHorizontalScrollBar: function() {
            var table = this.table,
                content = table.parent();

            var scrollbar = table[0].offsetWidth > content[0].clientWidth ? kendo.support.scrollbar() : 0;
            this.lockedContent.height(content.height() - scrollbar);
        },

        _syncLockedContentHeight: function() {
            if (this.lockedTable) {
                this._adjustLockedHorizontalScrollBar();
                this._adjustRowsHeight(this.table, this.lockedTable);
            }
        },

        _syncLockedHeaderHeight: function() {
            if (this.lockedHeader) {
                this._adjustRowsHeight(this.lockedHeader.children("table"), this.thead.parent());
            }
        },

        _syncLockedFooterHeight: function() {
            if (this.lockedFooter && this.footer && this.footer.length) {
                this._adjustRowsHeight(this.lockedFooter.children("table"), this.footer.find(".k-grid-footer-wrap > table"));
            }
        },

        _destroyEditable: function() {
            var that = this;

            var destroy = function() {
                if (that.editable) {

                    that._detachModelChange();
                    that.editable.destroy();
                    that.editable = null;
                    that._editContainer = null;
                    that._destroyEditView();
                }
            };

            if (that.editable) {
                if (that._editMode() === "popup" && !that._isMobile) {
                    that._editContainer.data("kendoWindow").bind("deactivate", destroy).close();
                } else {
                    destroy();
                }
            }
            if (that._actionSheet) {
                that._actionSheet.destroy();
                that._actionSheet = null;
            }
        },

        _destroyEditView: function() {
            if (this.editView) {
                this.editView.purge();
                this.editView = null;
                this.pane.navigate("");
            }
        },

        _attachModelChange: function(model) {
            var that = this;

            that._modelChangeHandler = function(e) {
                that._modelChange({ field: e.field, model: this });
            };

            model.bind("change", that._modelChangeHandler);
        },

        _detachModelChange: function() {
            var that = this,
                container = that._editContainer,
                model = that._modelForContainer(container);

            if (model) {
                model.unbind(CHANGE, that._modelChangeHandler);
            }
        },

        closeCell: function(isCancel) {
            var that = this,
                cell = that._editContainer,
                id,
                column,
                tr,
                model;

            if (!cell) {
                return;
            }

            id = cell.closest("tr").attr(kendo.attr("uid"));
            model = that.dataSource.getByUid(id);

            if (isCancel && that.trigger("cancel", { container: cell, model: model })) {
                return;
            }

            cell.removeClass("k-edit-cell");
            column = that.columns[that.cellIndex(cell)];

            tr = cell.parent().removeClass("k-grid-edit-row");

            that._destroyEditable(); // editable should be destoryed before content of the container is changed

            that._displayCell(cell, column, model);

            if (cell.hasClass("k-dirty-cell")) {
                $('<span class="k-dirty"/>').prependTo(cell);
            }

            if (that.lockedContent) {
                adjustRowHeight(tr.css("height", "")[0], that._relatedRow(tr).css("height", "")[0]);
            }
        },

        _displayCell: function(cell, column, dataItem) {
            var that = this,
                state = { storage: {}, count: 0 },
                settings = extend({}, kendo.Template, that.options.templateSettings),
                tmpl = kendo.template(that._cellTmpl(column, state), settings);

            if (state.count > 0) {
                tmpl = proxy(tmpl, state.storage);
            }

            cell.empty().html(tmpl(dataItem));
        },

        removeRow: function(row) {
            if (!this._confirmation(row)) {
                return;
            }

            this._removeRow(row);
        },

        _removeRow: function(row) {
            var that = this,
                model,
                mode = that._editMode();

            if (mode !== "incell") {
                that.cancelRow();
            }

            row = $(row).hide();
            model = that._modelForContainer(row);

            if (model && !that.trigger(REMOVE, { row: row, model: model })) {

                that.dataSource.remove(model);

                if (mode === "inline" || mode === "popup") {
                    that.dataSource.sync();
                }
            } else if (mode === "incell") {
                that._destroyEditable();
            }
        },

        _editMode: function() {
            var mode = "incell",
                editable = this.options.editable;

            if (editable !== true) {
                if (typeof editable == "string") {
                    mode = editable;
                } else {
                    mode = editable.mode || mode;
                }
            }

            return mode;
        },

        editRow: function(row) {
            var model;
            var that = this;

            if (row instanceof kendo.data.ObservableObject) {
                model = row;
            } else {
                row = $(row);
                model = that._modelForContainer(row);
            }

            var mode = that._editMode();
            var navigatable = that.options.navigatable;
            var container;

            that.cancelRow();

            if (model) {

                that._attachModelChange(model);

                if (mode === "popup") {
                    that._createPopupEditor(model);
                } else if (mode === "inline") {
                    that._createInlineEditor(row, model);
                } else if (mode === "incell") {
                    $(row).children(DATA_CELL).each(function() {
                        var cell = $(this);
                        var column = that.columns[cell.index()];

                        model = that._modelForContainer(cell);

                        if (model && (!model.editable || model.editable(column.field)) && column.field) {
                            that.editCell(cell);
                            return false;
                        }
                    });
                }

                container = that.editView ? that.editView.element : that._editContainer;

                container.on(CLICK + NS, "a.k-grid-cancel", function(e) {
                    e.preventDefault();
                    e.stopPropagation();

                    if (that.trigger("cancel", { container: container, model: model })) {
                        return;
                    }

                    var currentIndex = that.items().index($(that.current()).parent());

                    that.cancelRow();

                    if (navigatable) {
                        that.current(that.items().eq(currentIndex).children().filter(NAVCELL).first());
                        focusTable(that.table, true);
                    }
                });

                container.on(CLICK + NS, "a.k-grid-update", function(e) {
                    e.preventDefault();
                    e.stopPropagation();

                    that.saveRow();
                });
            }
        },

        _createPopupEditor: function(model) {
            var that = this,
                html = '<div ' + kendo.attr("uid") + '="' + model.uid + '" class="k-popup-edit-form' + (that._isMobile ? ' k-mobile-list' : '') + '"><div class="k-edit-form-container">',
                column,
                command,
                fields = [],
                idx,
                length,
                tmpl,
                updateText,
                cancelText,
                tempCommand,
                attr,
                editable = that.options.editable,
                template = editable.template,
                options = isPlainObject(editable) ? editable.window : {},
                settings = extend({}, kendo.Template, that.options.templateSettings);

            options = options || {};

            if (template) {
                if (typeof template === STRING) {
                    template = window.unescape(template);
                }

                html += (kendo.template(template, settings))(model);

                for (idx = 0, length = that.columns.length; idx < length; idx++) {
                    column = that.columns[idx];
                    if (column.command) {
                        tempCommand = getCommand(column.command, "edit");
                        if (tempCommand) {
                            command = tempCommand;
                        }
                    }
                }
            } else {
                for (idx = 0, length = that.columns.length; idx < length; idx++) {
                    column = that.columns[idx];

                    if (!column.command) {
                        html += '<div class="k-edit-label"><label for="' + column.field + '">' + (column.title || column.field || "") + '</label></div>';

                        if ((!model.editable || model.editable(column.field)) && column.field) {
                            fields.push({ field: column.field, format: column.format, editor: column.editor, values: column.values });
                            html += '<div ' + kendo.attr("container-for") + '="' + column.field + '" class="k-edit-field"></div>';
                        } else {
                            var state = { storage: {}, count: 0 };

                            tmpl = kendo.template(that._cellTmpl(column, state), settings);

                            if (state.count > 0) {
                                tmpl = proxy(tmpl, state.storage);
                            }

                            html += '<div class="k-edit-field">' + tmpl(model) + '</div>';
                        }
                    } else if (column.command) {
                        tempCommand = getCommand(column.command, "edit");
                        if (tempCommand) {
                            command = tempCommand;
                        }
                    }
                }
            }

            if (command) {
                if (isPlainObject(command)) {
                   if (command.text && isPlainObject(command.text)) {
                       updateText = command.text.update;
                       cancelText = command.text.cancel;
                   }

                   if (command.attr) {
                       attr = command.attr;
                   }
                }
            }

            var container;

            if (!that._isMobile) {
                html += '<div class="k-edit-buttons k-state-default">';
                html += that._createButton({ name: "update", text: updateText, attr: attr }) + that._createButton({ name: "canceledit", text: cancelText, attr: attr });
                html += '</div></div></div>';

                container = that._editContainer = $(html)
                .appendTo(that.wrapper).eq(0)
                .kendoWindow(extend({
                    modal: true,
                    resizable: false,
                    draggable: true,
                    title: "Edit",
                    visible: false,
                    close: function(e) {
                        if (e.userTriggered) {
                            //The bellow line is required due to: draggable window in IE, change event will be triggered while the window is closing
                            e.sender.element.focus();
                            if (that.trigger("cancel", { container: container, model: model })) {
                                e.preventDefault();
                                return;
                            }

                            var currentIndex = that.items().index($(that.current()).parent());

                            that.cancelRow();
                            if (that.options.navigatable) {
                                that.current(that.items().eq(currentIndex).children().filter(NAVCELL).first());
                                focusTable(that.table, true);
                            }
                        }
                    }
                }, options));
            } else {
                html += "</div></div>";
                that.editView = that.pane.append(
                    '<div data-' + kendo.ns + 'role="view" data-' + kendo.ns + 'init-widgets="false" class="k-grid-edit-form">'+
                        '<div data-' + kendo.ns + 'role="header" class="k-header">'+
                            that._createButton({ name: "update", text: updateText, attr: attr }) +
                            (options.title || "Edit") +
                            that._createButton({ name: "canceledit", text: cancelText, attr: attr }) +
                        '</div>'+
                        html +
                    '</div>');
                container = that._editContainer = that.editView.element.find(".k-popup-edit-form");
            }

            that.editable = that._editContainer
                .kendoEditable({
                    fields: fields,
                    model: model,
                    clearContainer: false
                }).data("kendoEditable");

            // TODO: Replace this code with labels and for="ID"
            if (that._isMobile) {
                container.find("input[type=checkbox],input[type=radio]")
                         .parent(".k-edit-field")
                         .addClass("k-check")
                         .prev(".k-edit-label")
                         .addClass("k-check")
                         .click(function() {
                             $(this).next().children("input").click();
                         });
            }

            that._openPopUpEditor();

            that.trigger(EDIT, { container: container, model: model });
        },

        _openPopUpEditor: function() {
            if (!this._isMobile) {
                this._editContainer.data("kendoWindow").center().open();
            } else {
                this.pane.navigate(this.editView, this._editAnimation);
            }
        },

        _createInlineEditor: function(row, model) {
            var that = this,
                column,
                cell,
                command,
                fields = [];


            if (that.lockedContent) {
                row = row.add(that._relatedRow(row));
            }

            row.children(":not(.k-group-cell,.k-hierarchy-cell)").each(function() {
                cell = $(this);
                column = that.columns[that.cellIndex(cell)];

                if (!column.command && column.field && (!model.editable || model.editable(column.field))) {
                    fields.push({ field: column.field, format: column.format, editor: column.editor, values: column.values });
                    cell.attr(kendo.attr("container-for"), column.field);
                    cell.empty();
                } else if (column.command) {
                    command = getCommand(column.command, "edit");
                    if (command) {
                        cell.empty();

                        var updateText,
                            cancelText,
                            attr;

                        if (isPlainObject(command)) {
                            if (command.text && isPlainObject(command.text)) {
                                updateText = command.text.update;
                                cancelText = command.text.cancel;
                            }

                            if (command.attr) {
                                attr = command.attr;
                            }
                        }

                        $(that._createButton({ name: "update", text: updateText, attr: attr }) +
                            that._createButton({ name: "canceledit", text: cancelText, attr: attr})).appendTo(cell);
                    }
                }
            });

            that._editContainer = row;

            that.editable = new kendo.ui.Editable(row
                .addClass("k-grid-edit-row"),{
                    fields: fields,
                    model: model,
                    clearContainer: false
                });

            if (row.length > 1) {

                adjustRowHeight(row[0], row[1]);
                that._applyLockedContainersWidth();
            }

            that.trigger(EDIT, { container: row, model: model });
        },

        cancelRow: function() {
            var that = this,
                container = that._editContainer,
                model,
                tr;

            if (container) {
                model = that._modelForContainer(container);

                that._destroyEditable();

                that.dataSource.cancelChanges(model);

                if (that._editMode() !== "popup") {
                    that._displayRow(container);
                } else {
                    that._displayRow(that.items().filter("[" + kendo.attr("uid") + "=" + model.uid + "]"));
                }
            }
        },

        saveRow: function() {
            var that = this,
                container = that._editContainer,
                model = that._modelForContainer(container),
                editable = that.editable;

            if (container && editable && editable.end() &&
                !that.trigger(SAVE, { container: container, model: model } )) {

                that.dataSource.sync();
            }
        },

        _displayRow: function(row) {
            var that = this,
                model = that._modelForContainer(row),
                related,
                newRow,
                nextRow,
                isAlt = row.hasClass("k-alt");

            if (model) {

                if (that.lockedContent) {
                    related = $((isAlt ? that.lockedAltRowTemplate : that.lockedRowTemplate)(model));
                    that._relatedRow(row.last()).replaceWith(related);
                }

                newRow = $((isAlt ? that.altRowTemplate : that.rowTemplate)(model));
                row.replaceWith(newRow);

                if (related) {
                    adjustRowHeight(newRow[0], related[0]);
                }

                nextRow = newRow.next();
                if (nextRow.hasClass("k-detail-row") && nextRow.is(":visible")) {
                    newRow.find(".k-hierarchy-cell .k-icon")
                        .removeClass("k-i-plus")
                        .addClass("k-i-minus");
                }
            }
        },

        _showMessage: function(messages, row) {
            var that = this;

            if (!that._isMobile) {
                return window.confirm(messages.title);
            }

            var template = kendo.template('<ul>'+
                '<li class="km-actionsheet-title">#:title#</li>'+
                '<li><a href="\\#" class="k-button k-grid-delete">#:confirmDelete#</a></li>'+
            '</ul>');

            var html = $(template(messages)).appendTo(that.view.element);

            var actionSheet = that._actionSheet = new kendo.mobile.ui.ActionSheet(html, {
                cancel: messages.cancelDelete,
                cancelTemplate: '<li class="km-actionsheet-cancel"><a class="k-button" href="\\#">#:cancel#</a></li>',
                close: function() {
                    this.destroy();
                },
                command: function(e) {
                    var item = $(e.currentTarget).parent();
                    if (!item.hasClass("km-actionsheet-cancel")) {
                        that._removeRow(row);
                    }
                },
                popup: that._actionSheetPopupOptions
            });

            actionSheet.open(row);

            return false;
        },

        _confirmation: function(row) {
            var that = this,
                editable = that.options.editable,
                confirmation = editable === true || typeof editable === STRING ? DELETECONFIRM : editable.confirmation;

            if (confirmation !== false && confirmation != null) {

                if (typeof confirmation === FUNCTION) {
                    confirmation = confirmation(that._modelForContainer(row));
                }

                return that._showMessage({
                        confirmDelete: editable.confirmDelete || CONFIRMDELETE,
                        cancelDelete: editable.cancelDelete || CANCELDELETE,
                        title: confirmation === true ? DELETECONFIRM : confirmation
                    }, row);
            }

            return true;
        },

        cancelChanges: function() {
            this.dataSource.cancelChanges();
        },

        saveChanges: function() {
            var that = this;

            if (((that.editable && that.editable.end()) || !that.editable) && !that.trigger(SAVECHANGES)) {
                that.dataSource.sync();
            }
        },

        addRow: function() {
            var that = this,
                index,
                dataSource = that.dataSource,
                mode = that._editMode(),
                createAt = that.options.editable.createAt || "",
                pageSize = dataSource.pageSize(),
                view = dataSource.view() || [];

            if ((that.editable && that.editable.end()) || !that.editable) {
                if (mode != "incell") {
                    that.cancelRow();
                }

                index = dataSource.indexOf(view[0]);

                if (createAt.toLowerCase() == "bottom") {
                    index += view.length;

                    if (pageSize && !dataSource.options.serverPaging && pageSize <= view.length) {
                        index -= 1;
                    }
                }

                if (index < 0) {
                    if (dataSource.page() > dataSource.totalPages()) {
                        index = (dataSource.page() - 1) * pageSize;
                    } else {
                        index = 0;
                    }
                }

                var model = dataSource.insert(index, {}),
                    id = model.uid,
                    table = that.lockedContent ? that.lockedTable : that.table,
                    row = table.find("tr[" + kendo.attr("uid") + "=" + id + "]"),
                    cell = row.children("td:not(.k-group-cell,.k-hierarchy-cell)").eq(that._firstEditableColumnIndex(row));

                if (mode === "inline" && row.length) {
                    that.editRow(row);
                } else if (mode === "popup") {
                    that.editRow(model);
                } else if (cell.length) {
                    that.editCell(cell);
                }
            }
        },

        _firstEditableColumnIndex: function(container) {
            var that = this,
                column,
                columns = that.columns,
                idx,
                length,
                model = that._modelForContainer(container);

            for (idx = 0, length = columns.length; idx < length; idx++) {
                column = columns[idx];

                if (model && (!model.editable || model.editable(column.field)) && !column.command && column.field) {
                    return idx;
                }
            }
            return -1;
        },

        _toolbar: function() {
            var that = this,
                wrapper = that.wrapper,
                toolbar = that.options.toolbar,
                editable = that.options.editable,
                container;

            if (toolbar) {
                container = that.wrapper.find(".k-grid-toolbar");

                if (!container.length) {
                    if (!isFunction(toolbar)) {
                        toolbar = (typeof toolbar === STRING ? toolbar : that._toolbarTmpl(toolbar).replace(templateHashRegExp, "\\#"));
                        toolbar = proxy(kendo.template(toolbar), that);
                    }

                    container = $('<div class="k-toolbar k-grid-toolbar" />')
                        .html(toolbar({}))
                        .prependTo(wrapper);
                }

                if (editable && editable.create !== false) {
                    container.on(CLICK + NS, ".k-grid-add", function(e) { e.preventDefault(); that.addRow(); })
                        .on(CLICK + NS, ".k-grid-cancel-changes", function(e) { e.preventDefault(); that.cancelChanges(); })
                        .on(CLICK + NS, ".k-grid-save-changes", function(e) { e.preventDefault(); that.saveChanges(); });
                }
            }
        },

        _toolbarTmpl: function(commands) {
            var that = this,
                idx,
                length,
                html = "";

            if (isArray(commands)) {
                for (idx = 0, length = commands.length; idx < length; idx++) {
                    html += that._createButton(commands[idx]);
                }
            }
            return html;
        },

        _createButton: function(command) {
            var template = command.template || COMMANDBUTTONTMPL,
                commandName = typeof command === STRING ? command : command.name || command.text,
                className = defaultCommands[commandName] ? defaultCommands[commandName].className : "k-grid-" + (commandName || "").replace(/\s/g, ""),
                options = { className: className, text: commandName, imageClass: "", attr: "", iconClass: "" };

            if (!commandName && !(isPlainObject(command) && command.template))  {
                throw new Error("Custom commands should have name specified");
            }

            if (isPlainObject(command)) {
                if (command.className) {
                    command.className += " " + options.className;
                }

                if (commandName === "edit" && isPlainObject(command.text)) {
                    command = extend(true, {}, command);
                    command.text = command.text.edit;
                }

                if (command.attr && isPlainObject(command.attr)) {
                    command.attr = stringifyAttributes(command.attr);
                }

                options = extend(true, options, defaultCommands[commandName], command);
            } else {
                options = extend(true, options, defaultCommands[commandName]);
            }

            return kendo.template(template)(options);
        },

        _hasFooters: function() {
            return !!this.footerTemplate ||
                !!this.groupFooterTemplate ||
                (this.footer && this.footer.length > 0) ||
                this.wrapper.find(".k-grid-footer").length > 0;
        },

        _groupable: function() {
            var that = this;

            if (that._groupableClickHandler) {
                that.table.off(CLICK + NS, that._groupableClickHandler);
            } else {
                that._groupableClickHandler = function(e) {
                    var element = $(this),
                    group = element.closest("tr");

                    if(element.hasClass('k-i-collapse')) {
                        that.collapseGroup(group);
                    } else {
                        that.expandGroup(group);
                    }
                    e.preventDefault();
                    e.stopPropagation();
                };
            }

            if (that._isLocked()) {
                that.lockedTable.on(CLICK + NS, ".k-grouping-row .k-i-collapse, .k-grouping-row .k-i-expand", that._groupableClickHandler);
            } else {
                that.table.on(CLICK + NS, ".k-grouping-row .k-i-collapse, .k-grouping-row .k-i-expand", that._groupableClickHandler);
            }

            that._attachGroupable();
        },

        _attachGroupable: function() {
            var that = this,
                wrapper = that.wrapper,
                groupable = that.options.groupable,
                GROUPINGDRAGGABLES = HEADERCELLS + ":visible[" + kendo.attr("field") + "]",
                GROUPINGFILTER =  HEADERCELLS + "[" + kendo.attr("field") + "]";

            if (groupable) {

                if(!wrapper.has("div.k-grouping-header")[0]) {
                    $("<div>&nbsp;</div>").addClass("k-grouping-header").prependTo(wrapper);
                }

                if (that.groupable) {
                    that.groupable.destroy();
                }

                that.groupable = new Groupable(wrapper, extend({}, groupable, {
                    draggable: that._draggableInstance,
                    groupContainer: ">div.k-grouping-header",
                    dataSource: that.dataSource,
                    draggableElements: that.content ? ".k-grid-header:first " + GROUPINGDRAGGABLES : "table:first>.k-grid-header " + GROUPINGDRAGGABLES,
                    filter: that.content ? ".k-grid-header:first " + GROUPINGFILTER : "table:first>.k-grid-header " + GROUPINGFILTER,
                    allowDrag: that.options.reorderable
                }));
            }
        },

        _continuousItems: function(filter, cell) {
            if (!this.lockedContent) {
                return;
            }

            var that = this;

            var elements = that.table.add(that.lockedTable);

            var lockedItems = $(filter, elements[0]);
            var nonLockedItems = $(filter, elements[1]);
            var columns = cell ? lockedColumns(that.columns).length : 1;
            var nonLockedColumns = cell ? that.columns.length - columns : 1;
            var result = [];

            for (var idx = 0; idx < lockedItems.length; idx += columns) {
                push.apply(result, lockedItems.slice(idx, idx + columns));
                push.apply(result, nonLockedItems.splice(0, nonLockedColumns));
            }

            return result;
        },

        _selectable: function() {
            var that = this,
                multi,
                cell,
                notString = [],
                isLocked = that._isLocked(),
                selectable = that.options.selectable;

            if (selectable) {

                if (that.selectable) {
                    that.selectable.destroy();
                }

                multi = typeof selectable === STRING && selectable.toLowerCase().indexOf("multiple") > -1;
                cell = typeof selectable === STRING && selectable.toLowerCase().indexOf("cell") > -1;

                if (that._hasDetails()) {
                    notString[notString.length] = ".k-detail-row";
                }
                if (that.options.groupable || that._hasFooters()) {
                    notString[notString.length] = ".k-grouping-row,.k-group-footer";
                }

                notString = notString.join(",");

                if (notString !== "") {
                    notString = ":not(" + notString + ")";
                }

                var elements = that.table;
                if (isLocked) {
                    elements = elements.add(that.lockedTable);
                }

                var filter = ">" + (cell ? SELECTION_CELL_SELECTOR : "tbody>tr" + notString);
                that.selectable = new kendo.ui.Selectable(elements, {
                    filter: filter,
                    aria: true,
                    multiple: multi,
                    change: function() {
                        that.trigger(CHANGE);
                    },
                    useAllItems: isLocked && multi && cell,
                    relatedTarget: function(items) {
                        if (cell || !isLocked) {
                            return;
                        }

                        var related;
                        var result = $();
                        for (var idx = 0, length = items.length; idx < length; idx ++) {
                            related = that._relatedRow(items[idx]);

                            if (inArray(related[0], items) < 0) {
                                result = result.add(related);
                            }
                        }

                        return result;
                    },
                    continuousItems: function() {
                        return that._continuousItems(filter, cell);
                    }
                });

                if (that.options.navigatable) {
                    elements.on("keydown" + NS, function(e) {
                        var current = that.current();
                        var target = e.target;
                        if (e.keyCode === keys.SPACEBAR && $.inArray(target, elements) > -1 &&
                            !current.is(".k-edit-cell,.k-header") &&
                            current.parent().is(":not(.k-grouping-row,.k-detail-row,.k-group-footer)")) {
                            e.preventDefault();
                            e.stopPropagation();
                            current = cell ? current : current.parent();

                            if (isLocked && !cell) {
                                current = current.add(that._relatedRow(current));
                            }

                            if(multi) {
                                if(!e.ctrlKey) {
                                    that.selectable.clear();
                                } else {
                                    if(current.hasClass(SELECTED)) {
                                        current.removeClass(SELECTED);
                                        that.trigger(CHANGE);
                                        return;
                                    }
                                }
                            } else {
                                that.selectable.clear();
                            }

                            that.selectable.value(current);
                        }
                    });
                }
            }
        },

        _relatedRow: function(row) {
            var lockedTable = this.lockedTable;
            row = $(row);

            if (!lockedTable) {
                return row;
            }

            var table = row.closest(this.table.add(this.lockedTable));
            var index = table.find(">tbody>tr").index(row);

            table = table[0] === this.table[0] ? lockedTable : this.table;

            return table.find(">tbody>tr").eq(index);
        },

        clearSelection: function() {
            var that = this;
            that.selectable.clear();
            that.trigger(CHANGE);
        },

        select: function(items) {
            var that = this,
                selectable = that.selectable;

            items = $(items);
            if(items.length) {
                if(!selectable.options.multiple) {
                    selectable.clear();
                    items = items.first();
                }

                if (that._isLocked()) {
                    items = items.add(items.map(function() {
                        return that._relatedRow(this);
                    }));
                }

                selectable.value(items);
                return;
            }

            return selectable.value();
        },

        current: function(element) {
            var that = this,
                scrollable = that.options.scrollable,
                current = that._current,
                table = that.table.add(that.thead.parent());

            if (element !== undefined && element.length) {
                if (!current || current[0] !== element[0]) {
                    if (current) {
                        current.removeClass(FOCUSED).removeAttr("id");
                        table.removeAttr("aria-activedescendant");
                    }

                    element.attr("id", that._cellId);
                    that._current = element.addClass(FOCUSED);

                    table.attr("aria-activedescendant", that._cellId);

                    if(element.length && scrollable) {
                        var content = element.closest("table").parent();
                        if (content.is(".k-grid-content,.k-grid-content-locked")) {
                            that._scrollTo(element.parent()[0], that.content[0]);
                        }

                        if (!content.is(".k-grid-content-locked,.k-grid-header-locked")) {
                            if (scrollable.virtual) {
                                that._scrollTo(element[0], that.content.find(">.k-virtual-scrollable-wrap")[0]);
                            } else {
                                that._scrollTo(element[0], that.content[0]);
                            }
                        }
                    }
                }
            }

            return that._current;
        },

        _removeCurrent: function() {
            if (this._current) {
                this._current.removeClass(FOCUSED);
                this._current = null;
            }
        },

        _scrollTo: function(element, container) {
            var elementToLowercase = element.tagName.toLowerCase(),
                isHorizontal =  elementToLowercase === "td" || elementToLowercase === "th",
                elementOffset = element[isHorizontal ? "offsetLeft" : "offsetTop"],
                elementOffsetDir = element[isHorizontal ? "offsetWidth" : "offsetHeight"],
                containerScroll = container[isHorizontal ? "scrollLeft" : "scrollTop"],
                containerOffsetDir = container[isHorizontal ? "clientWidth" : "clientHeight"],
                bottomDistance = elementOffset + elementOffsetDir,
                result = 0;

                if (containerScroll > elementOffset) {
                    result = elementOffset;
                } else if (bottomDistance > (containerScroll + containerOffsetDir)) {
                    if (elementOffsetDir <= containerOffsetDir) {
                        result = (bottomDistance - containerOffsetDir);
                    } else {
                        result = elementOffset;
                    }
                } else {
                    result = containerScroll;
                }
                container[isHorizontal ? "scrollLeft" : "scrollTop"] = result;
        },

        _navigatable: function() {
            var that = this,
                currentProxy = proxy(that.current, that),
                table = that.table.add(that.lockedTable),
                headerTable = that.thead.parent().add($(">table", that.lockedHeader)),
                isLocked = that._isLocked(),
                dataTable = table,
                isRtl = kendo.support.isRtl(that.element);

            if (!that.options.navigatable) {
                return;
            }

            if (that.options.scrollable) {
                dataTable = table.add(headerTable);
                headerTable.attr(TABINDEX, -1);
            }

            headerTable.on("keydown" + NS, function(e) {
                if (e.altKey && e.keyCode == keys.DOWN) {
                    currentProxy().find(".k-grid-filter, .k-header-column-menu").click();
                    e.stopImmediatePropagation();
                }
            })
            .find("a.k-link").attr("tabIndex", -1);

            table
            .attr(TABINDEX, math.max(table.attr(TABINDEX) || 0, 0))
            .on("mousedown" + NS + " keydown" + NS, ".k-detail-cell", function(e) {
                if (e.target !== e.currentTarget) {
                    e.stopImmediatePropagation();
                }
            });

            dataTable
            .on((kendo.support.touch ? "touchstart" + NS : "mousedown" + NS), NAVROW + ">" + NAVCELL, proxy(tableClick, that))
            .on("focus" + NS, function() {
                if (kendo.support.touch) {
                    return;
                }

                var current = currentProxy();
                if (current && current.is(":visible")) {
                    current.addClass(FOCUSED);
                } else {
                    currentProxy($(this).find(FIRSTNAVITEM));
                }

                table.attr(TABINDEX, -1);
                headerTable.attr(TABINDEX, -1);
                $(this).attr(TABINDEX, 0);
            })
            .on("focusout" + NS, function() {
                var current = currentProxy();
                if (current) {
                    current.removeClass(FOCUSED);
                }
            })
            .on("keydown" + NS, function(e) {
                var key = e.keyCode,
                    handled = false,
                    canHandle = !e.isDefaultPrevented() && !$(e.target).is(":button,a,:input,a>.k-icon"),
                    pageable = that.options.pageable,
                    dataSource = that.dataSource,
                    isInCell = that._editMode() == "incell",
                    active,
                    currentIndex,
                    row,
                    index,
                    tableToFocus,
                    shiftKey = e.shiftKey,
                    relatedRow = proxy(that._relatedRow, that),
                    current = currentProxy();

                if (current && current.is("th")) {
                    canHandle = true;
                }

                if (canHandle && key == keys.UP) {
                    currentProxy(moveVertical(current, e.currentTarget, table, headerTable, true));
                    handled = true;
                } else if (canHandle && key == keys.DOWN) {
                    currentProxy(moveVertical(current, e.currentTarget, table, headerTable));
                    handled = true;
                } else if (canHandle && key == (isRtl ? keys.RIGHT : keys.LEFT)) {
                    currentProxy(moveLeft(current, e.currentTarget, table, headerTable, relatedRow));
                    handled = true;
                } else if (canHandle && key == (isRtl ? keys.LEFT : keys.RIGHT)) {
                    currentProxy(moveRight(current, e.currentTarget, table, headerTable, relatedRow));
                    handled = true;
                } else if (canHandle && pageable && keys.PAGEDOWN == key) {
                    dataSource.page(dataSource.page() + 1);
                    handled = true;
                } else if (canHandle && pageable && keys.PAGEUP == key) {
                    dataSource.page(dataSource.page() - 1);
                    handled = true;
                } else if (key == keys.ENTER || keys.F2 == key) {
                    current = current ? current : table.find(FIRSTNAVITEM);
                    if (current.is("th")) {
                        current.find(".k-link").click();
                        handled = true;
                    } else if (current.parent().is(".k-master-row,.k-grouping-row")) {
                        current.parent().find(".k-icon:first").click();
                        handled = true;
                    } else {
                        var focusable = current.find(":kendoFocusable:first");
                        if (!current.hasClass("k-edit-cell") && focusable[0] && current.hasClass("k-state-focused")) {
                            focusable.focus();
                            handled = true;
                        } else if (that.options.editable && !$(e.target).is(":button,.k-button,textarea")) {
                            var container = $(e.target).closest("[role=gridcell]");
                            if (!container[0]) {
                                container = current;
                            }

                            that._handleEditing(container, false, isInCell ? e.currentTarget : table[0]);
                            handled = true;
                        }
                    }
                } else if (keys.ESC == key) {
                    active = activeElement();
                    if (current && $.contains(current[0], active) && !current.hasClass("k-edit-cell") && !current.parent().hasClass("k-grid-edit-row")) {
                        focusTable(e.currentTarget, true);
                        handled = true;
                    } else if (that._editContainer && (!current || that._editContainer.has(current[0]) || current[0] === that._editContainer[0])) {
                        if (isInCell) {
                            that.closeCell(true);
                        } else {
                            currentIndex = $(current).parent().index();
                            if (active) {
                                active.blur();
                            }
                            that.cancelRow();
                            if (currentIndex >= 0) {
                                that.current(table.find(">tbody>tr").eq(currentIndex).children().filter(NAVCELL).first());
                            }
                        }

                        if (browser.msie && browser.version < 9) {
                            document.body.focus();
                        }
                        focusTable(isInCell ? e.currentTarget : table[0], true);
                        handled = true;
                    }
                } else if (keys.TAB == key) {
                    var cell;

                    current = $(current);
                    if (that.options.editable && isInCell) {
                         cell = $(activeElement()).closest(".k-edit-cell");

                         if (cell[0] && cell[0] !== current[0]) {
                             current = cell;
                         }
                    }

                    cell = tabNext(current, e.currentTarget, table, relatedRow, shiftKey);

                    if (!current.is("th") && cell.length && that.options.editable && isInCell) {
                        that._handleEditing(current, cell, cell.closest(table));
                        handled = true;
                    }
                }

                if (handled) {
                    //prevent browser scrolling
                    e.preventDefault();
                    //required in hierarchy
                    e.stopPropagation();
                }
            });
        },

        _handleEditing: function(current, next, table) {
            var that = this,
                active = $(activeElement()),
                mode = that._editMode(),
                isIE = browser.msie,
                oldIE = isIE && browser.version < 9,
                editContainer = that._editContainer,
                focusable,
                isEdited;

            table = $(table);
            if (mode == "incell") {
                isEdited = current.hasClass("k-edit-cell");
            } else {
                isEdited = current.parent().hasClass("k-grid-edit-row");
            }

            if (that.editable) {
                if ($.contains(editContainer[0], active[0])) {
                    if (browser.opera || oldIE) {
                        active.change().triggerHandler("blur");
                    } else {
                        active.blur();
                        if (isIE) {
                            //IE10 with jQuery 1.9.x does not trigger blur handler
                            //numeric textbox does trigger change
                            active.blur();
                        }
                    }
                }

                if (!that.editable) {
                    focusTable(table);
                    return;
                }

                if (that.editable.end()) {
                    if (mode == "incell") {
                        that.closeCell();
                    } else {
                        that.saveRow();
                        isEdited = true;
                    }
                } else {
                    if (mode == "incell") {
                        that.current(editContainer);
                    } else {
                        that.current(editContainer.children().filter(DATA_CELL).first());
                    }
                    focusable = editContainer.find(":kendoFocusable:first")[0];
                    if (focusable) {
                        focusable.focus();
                    }
                    return;
                }
            }

            if (next) {
                that.current(next);
            }

            if (oldIE) {
                document.body.focus();
            }
            focusTable(table, true);
            if ((!isEdited && !next) || next) {
                if (mode == "incell") {
                    that.editCell(that.current());
                } else {
                    that.editRow(that.current().parent());
                }
            }
        },

        _wrapper: function() {
            var that = this,
                table = that.table,
                height = that.options.height,
                wrapper = that.element;

            if (!wrapper.is("div")) {
               wrapper = wrapper.wrap("<div/>").parent();
            }

            that.wrapper = wrapper.addClass("k-grid k-widget");

            if (height) {
                that.wrapper.css(HEIGHT, height);
                table.css(HEIGHT, "auto");
            }

            that._initMobile();
        },

        _initMobile: function() {
            var options = this.options;
            var that = this;

            this._isMobile = (options.mobile === true && kendo.support.mobileOS) ||
                                options.mobile === "phone" ||
                                options.mobile === "tablet";

            if (this._isMobile) {
                var html = this.wrapper.addClass("k-grid-mobile").wrap(
                        '<div data-' + kendo.ns + 'role="view" ' +
                        'data-' + kendo.ns + 'init-widgets="false"></div>'
                    )
                    .parent();

                this.pane = kendo.mobile.ui.Pane.wrap(html);
                this.view = this.pane.view();
                this._actionSheetPopupOptions = $(document.documentElement).hasClass("km-root") ? { modal: false } : {
                    align: "bottom center",
                    position: "bottom center",
                    effect: "slideIn:up"
                };

                if (options.height) {
                    this.pane.element.parent().css(HEIGHT, options.height);
                }

                this._editAnimation = "slide";

                this.view.bind("show", function() {
                    if (that._isLocked()) {
                        that._updateTablesWidth();
                        that._applyLockedContainersWidth();
                        that._syncLockedContentHeight();
                        that._syncLockedHeaderHeight();
                        that._syncLockedFooterHeight();
                    }
                });
            }
        },

        _tbody: function() {
            var that = this,
                table = that.table,
                tbody;

            tbody = table.find(">tbody");

            if (!tbody.length) {
                tbody = $("<tbody/>").appendTo(table);
            }

            that.tbody = tbody.attr("role", "rowgroup");
        },

        _scrollable: function() {
            var that = this,
                header,
                table,
                options = that.options,
                scrollable = options.scrollable,
                hasVirtualScroll = scrollable !== true && scrollable.virtual && !that.virtualScrollable,
                scrollbar = !kendo.support.kineticScrollNeeded || hasVirtualScroll ? kendo.support.scrollbar() : 0;

            if (scrollable) {
                header = that.wrapper.children(".k-grid-header");

                if (!header[0]) {
                    header = $('<div class="k-grid-header" />').insertBefore(that.table);
                }

                // workaround for IE issue where scroll is not raised if container is same width as the scrollbar
                header.css((isRtl ? "padding-left" : "padding-right"), scrollable.virtual ? scrollbar + 1 : scrollbar);
                table = $('<table role="grid" />');
                if (isIE7) {
                    table.attr("cellspacing", 0);
                }
                table.append(that.thead);
                header.empty().append($('<div class="k-grid-header-wrap" />').append(table));


                that.content = that.table.parent();

                if (that.content.is(".k-virtual-scrollable-wrap, .km-scroll-container")) {
                    that.content = that.content.parent();
                }

                if (!that.content.is(".k-grid-content, .k-virtual-scrollable-wrap")) {
                    that.content = that.table.wrap('<div class="k-grid-content" />').parent();
                }
                if (hasVirtualScroll) {
                    that.virtualScrollable = new VirtualScrollable(that.content, {
                        dataSource: that.dataSource,
                        itemHeight: function() { return that._averageRowHeight(); }
                    });
                }

                that.scrollables = header.children(".k-grid-header-wrap");

                // the footer may exists if rendered from the server
                var footer = that.wrapper.find(".k-grid-footer"),
                    webKitRtlCorrection = (isRtl && browser.webkit) ? scrollbar : 0;

                if (footer.length) {
                    that.scrollables = that.scrollables.add(footer.children(".k-grid-footer-wrap"));
                }

                if (scrollable.virtual) {
                    that.content.find(">.k-virtual-scrollable-wrap").bind("scroll" + NS, function () {
                        that.scrollables.scrollLeft(this.scrollLeft + webKitRtlCorrection);
                        if (that.lockedContent) {
                            that.lockedContent[0].scrollTop = this.scrollTop;
                        }
                    });
                } else {
                    that.content.bind("scroll" + NS, function () {
                        that.scrollables.scrollLeft(this.scrollLeft + webKitRtlCorrection);
                        if (that.lockedContent) {
                            that.lockedContent[0].scrollTop = this.scrollTop;
                        }
                    });

                    var touchScroller = kendo.touchScroller(that.content);
                    if (touchScroller && touchScroller.movable) {
                        touchScroller.movable.bind("change", function(e) {
                            that.scrollables.scrollLeft(-e.sender.x);
                            if (that.lockedContent) {
                                that.lockedContent.scrollTop(-e.sender.y);
                            }
                        });
                    }
                }
            }
        },

        _setContentWidth: function() {
            var that = this,
                hiddenDivClass = 'k-grid-content-expander',
                hiddenDiv = '<div class="' + hiddenDivClass + '"></div>',
                resizable = that.resizable,
                expander;

            if (that.options.scrollable && that.wrapper.is(":visible")) {
                expander = that.table.parent().children('.' + hiddenDivClass);
                that._setContentWidthHandler = proxy(that._setContentWidth, that);
                if (!that.dataSource || !that.dataSource.view().length) {
                    if (!expander[0]) {
                        expander = $(hiddenDiv).appendTo(that.table.parent());
                        if (resizable) {
                            resizable.bind("resize", that._setContentWidthHandler);
                        }
                    }
                    if (that.thead) {
                        expander.width(that.thead.width());
                    }
                } else if (expander[0]) {
                    expander.remove();
                    if (resizable) {
                        resizable.unbind("resize", that._setContentWidthHandler);
                    }
                }

                that._applyLockedContainersWidth();
           }
        },

        _applyLockedContainersWidth: function() {
            if (this.options.scrollable && this.lockedHeader) {
                var headerTable = this.thead.parent(),
                    headerWrap = headerTable.parent(),
                    contentWidth = this.wrapper[0].clientWidth,
                    groups = this._groups(),
                    scrollbar = kendo.support.scrollbar(),
                    cols = this.lockedHeader.find(">table>colgroup>col:not(.k-group-col, .k-hierarchy-col)"),
                    nonLockedCols = headerTable.find(">colgroup>col:not(.k-group-col, .k-hierarchy-col)"),
                    width = columnsWidth(cols),
                    nonLockedColsWidth = columnsWidth(nonLockedCols),
                    footerWrap;

                if (groups > 0) {
                    width += this.lockedHeader.find(".k-group-cell:first").outerWidth() * groups;
                }

                if (width >= contentWidth) {
                    width = contentWidth - 3 * scrollbar;
                }

                this.lockedHeader
                    .add(this.lockedContent)
                    .width(width);

                headerWrap[0].style.width = headerWrap.parent().width() - width - 2 + "px";

                headerTable.add(this.table).width(nonLockedColsWidth);

                if (this.virtualScrollable) {
                    contentWidth -= scrollbar;
                }

                this.content[0].style.width = contentWidth - width - 2 + "px";

                if (this.lockedFooter && this.lockedFooter.length) {
                    this.lockedFooter.width(width);
                    footerWrap = this.footer.find(".k-grid-footer-wrap");
                    footerWrap[0].style.width = headerWrap[0].clientWidth + "px";
                    footerWrap.children().first().width(nonLockedColsWidth);
                }
            }
        },

        _setContentHeight: function() {
            var that = this,
                options = that.options,
                height = that.wrapper.innerHeight(),
                header = that.wrapper.children(".k-grid-header"),
                scrollbar = kendo.support.scrollbar();

            if (options.scrollable && that.wrapper.is(":visible")) {

                height -= header.outerHeight();

                if (that.pager) {
                    height -= that.pager.element.outerHeight();
                }

                if(options.groupable) {
                    height -= that.wrapper.children(".k-grouping-header").outerHeight();
                }

                if(options.toolbar) {
                    height -= that.wrapper.children(".k-grid-toolbar").outerHeight();
                }

                if (that.footerTemplate) {
                    height -= that.wrapper.children(".k-grid-footer").outerHeight();
                }

                var isGridHeightSet = function(el) {
                    var initialHeight, newHeight;
                    if (el[0].style.height) {
                        return true;
                    } else {
                        initialHeight = el.height();
                    }

                    el.height("auto");
                    newHeight = el.height();

                    if (initialHeight != newHeight) {
                        el.height("");
                        return true;
                    }
                    el.height("");
                    return false;
                };

                if (isGridHeightSet(that.wrapper)) { // set content height only if needed
                    if (height > scrollbar * 2) { // do not set height if proper scrollbar cannot be displayed
                        if (that.lockedContent) {
                            scrollbar = that.table[0].offsetWidth > that.table.parent()[0].clientWidth ? scrollbar : 0;
                            that.lockedContent.height(height - scrollbar);
                        }

                        that.content.height(height);
                    } else {
                        that.content.height(scrollbar * 2 + 1);
                    }
                }
            }
        },

        _averageRowHeight: function() {
            var that = this,
                itemsCount = that.items().length,
                rowHeight = that._rowHeight;

            if (itemsCount === 0) {
                return rowHeight;
            }

            if (!that._rowHeight) {
                that._rowHeight = rowHeight = that.table.outerHeight() / itemsCount;
                that._sum = rowHeight;
                that._measures = 1;
            }

            var currentRowHeight = that.table.outerHeight() / itemsCount;

            if (rowHeight !== currentRowHeight) {
                that._measures ++;
                that._sum += currentRowHeight;
                that._rowHeight = that._sum / that._measures;
            }
            return rowHeight;
        },

        _dataSource: function() {
            var that = this,
                options = that.options,
                pageable,
                dataSource = options.dataSource;

            dataSource = isArray(dataSource) ? { data: dataSource } : dataSource;

            if (isPlainObject(dataSource)) {
                extend(dataSource, { table: that.table, fields: that.columns });

                pageable = options.pageable;

                if (isPlainObject(pageable) && pageable.pageSize !== undefined) {
                    dataSource.pageSize = pageable.pageSize;
                }
            }

            if (that.dataSource && that._refreshHandler) {
                that.dataSource.unbind(CHANGE, that._refreshHandler)
                                .unbind(PROGRESS, that._progressHandler)
                                .unbind(ERROR, that._errorHandler);
            } else {
                that._refreshHandler = proxy(that.refresh, that);
                that._progressHandler = proxy(that._requestStart, that);
                that._errorHandler = proxy(that._error, that);
            }

            that.dataSource = DataSource.create(dataSource)
                                .bind(CHANGE, that._refreshHandler)
                                .bind(PROGRESS, that._progressHandler)
                                .bind(ERROR, that._errorHandler);
        },

        _error: function() {
            this._progress(false);
        },

        _requestStart: function() {
            this._progress(true);
        },

        _modelChange: function(e) {
            var that = this,
                model = e.model,
                row = that.tbody.find("tr[" + kendo.attr("uid") + "=" + model.uid +"]"),
                relatedRow,
                cell,
                column,
                isAlt = row.hasClass("k-alt"),
                tmp,
                idx = that.items().index(row),
                isLocked = that.lockedContent,
                length;

            if (isLocked) {
                relatedRow = that._relatedRow(row);
            }

            if (row.add(relatedRow).children(".k-edit-cell").length && !that.options.rowTemplate) {
                row.add(relatedRow).children(":not(.k-group-cell,.k-hierarchy-cell)").each(function() {
                    cell = $(this);
                    column = that.columns[that.cellIndex(cell)];

                    if (column.field === e.field) {
                        if (!cell.hasClass("k-edit-cell")) {
                            that._displayCell(cell, column, model);
                            $('<span class="k-dirty"/>').prependTo(cell);
                        } else {
                            cell.addClass("k-dirty-cell");
                        }
                    }
                });

            } else if (!row.hasClass("k-grid-edit-row")) {

                if (isLocked) {
                    tmp = (isAlt ? that.lockedAltRowTemplate : that.lockedRowTemplate)(model);

                    relatedRow.replaceWith(tmp);
                }

                tmp = (isAlt ? that.altRowTemplate : that.rowTemplate)(model);

                row.replaceWith(tmp);

                tmp = that.items().eq(idx);

                if (isLocked) {
                    relatedRow = that._relatedRow(tmp)[0];
                    adjustRowHeight(tmp[0], relatedRow);

                    tmp = tmp.add(relatedRow);
                }

                for (idx = 0, length = that.columns.length; idx < length; idx++) {
                    column = that.columns[idx];

                    if (column.field === e.field) {
                        cell = tmp.children(":not(.k-group-cell,.k-hierarchy-cell)").eq(idx);
                        $('<span class="k-dirty"/>').prependTo(cell);
                    }
                }

                that.trigger("itemChange", { item: tmp, data: model, ns: ui });
            }

        },

        _pageable: function() {
            var that = this,
                wrapper,
                pageable = that.options.pageable;

            if (pageable) {
                wrapper = that.wrapper.children("div.k-grid-pager");

                if (!wrapper.length) {
                    wrapper = $('<div class="k-pager-wrap k-grid-pager"/>').appendTo(that.wrapper);
                }

                if (that.pager) {
                    that.pager.destroy();
                }

                if (typeof pageable === "object" && pageable instanceof kendo.ui.Pager) {
                    that.pager = pageable;
                } else {
                    that.pager = new kendo.ui.Pager(wrapper, extend({}, pageable, { dataSource: that.dataSource }));
                }
            }
        },

        _footer: function() {
            var that = this,
                aggregates = that.dataSource.aggregates(),
                html = "",
                footerTemplate = that.footerTemplate,
                options = that.options,
                footerWrap,
                footer = that.footer || that.wrapper.find(".k-grid-footer");

            if (footerTemplate) {
                aggregates = !isEmptyObject(aggregates) ? aggregates : buildEmptyAggregatesObject(that.dataSource.aggregate());

                html = $(that._wrapFooter(footerTemplate(aggregates)));

                if (footer.length) {
                    var tmp = html;

                    footer.replaceWith(tmp);
                    footer = that.footer = tmp;
                } else {
                    if (options.scrollable) {
                        footer = that.footer = options.pageable ? html.insertBefore(that.wrapper.children("div.k-grid-pager")) : html.appendTo(that.wrapper);
                    } else {
                        footer = that.footer = html.insertBefore(that.tbody);
                    }
                }
            } else if (footer && !that.footer) {
                that.footer = footer;
            }

            if (footer.length) {
                if (options.scrollable) {
                    footerWrap = footer.attr("tabindex", -1).children(".k-grid-footer-wrap");
                    that.scrollables = that.scrollables
                        .filter(function() { return !$(this).is(".k-grid-footer-wrap"); })
                        .add(footerWrap);
                }

                if (that._footerWidth) {
                    footer.find("table").css('width', that._footerWidth);
                }

                if (footerWrap) {
                    var offset = that.content.scrollLeft();

                    var hasVirtualScroll = options.scrollable !== true && options.scrollable.virtual && !that.virtualScrollable;
                    if(hasVirtualScroll){
                        offset = that.wrapper.find('.k-virtual-scrollable-wrap').scrollLeft();
                    }
                    footerWrap.scrollLeft(offset);
                }
            }

            if (that.lockedContent) {
                that._appendLockedColumnFooter();
                that._applyLockedContainersWidth();
                that._syncLockedFooterHeight();
            }
        },

        _wrapFooter: function(footerRow) {
            var that = this,
                html = "",
                scrollbar = !kendo.support.mobileOS ? kendo.support.scrollbar() : 0;

            if (that.options.scrollable) {
                html = $('<div class="k-grid-footer"><div class="k-grid-footer-wrap"><table' + (isIE7 ? ' cellspacing="0"' : '') + '><tbody>' + footerRow + '</tbody></table></div></div>');
                that._appendCols(html.find("table"));
                html.css((isRtl ? "padding-left" : "padding-right"), scrollbar); // Update inner fix.

                return html;
            }

            return '<tfoot class="k-grid-footer">' + footerRow + '</tfoot>';
        },

        _columnMenu: function() {
            var that = this,
                menu,
                columns = that.columns,
                column,
                options = that.options,
                columnMenu = options.columnMenu,
                menuOptions,
                sortable,
                filterable,
                cells,
                isMobile = this._isMobile,
                initCallback = function(e) {
                    that.trigger(COLUMNMENUINIT, { field: e.field, container: e.container });
                },
                closeCallback = function(element) {
                    focusTable(element.closest("table"), true);
                };

            if (columnMenu) {
                if (typeof columnMenu == "boolean") {
                    columnMenu = {};
                }

                cells = that.thead.find("th:not(.k-hierarchy-cell):not(.k-group-cell)");

                for (var idx = 0, length = cells.length; idx < length; idx++) {
                    column = columns[idx];
                    var cell = cells.eq(idx);

                    if (!column.command && (column.field || cell.attr("data-" + kendo.ns + "field"))) {
                        menu = cell.data("kendoColumnMenu");
                        if (menu) {
                            menu.destroy();
                        }
                        sortable = column.sortable !== false && columnMenu.sortable !== false ? options.sortable : false;
                        filterable = options.filterable && column.filterable !== false && columnMenu.filterable !== false ? extend({ pane: that.pane }, column.filterable, options.filterable) : false;
                        menuOptions = {
                            dataSource: that.dataSource,
                            values: column.values,
                            columns: columnMenu.columns,
                            sortable: sortable,
                            filterable: filterable,
                            messages: columnMenu.messages,
                            owner: that,
                            closeCallback: closeCallback,
                            init: initCallback,
                            pane: that.pane,
                            filter: isMobile ? ":not(.k-column-active)" : "",
                            lockedColumns: column.lockable !== false && lockedColumns(columns).length > 0
                        };

                        cell.kendoColumnMenu(menuOptions);
                    }
                }
            }
        },

        _headerCells: function() {
            return this.thead.find("th").filter(function() {
                var th = $(this);
                return !th.hasClass("k-group-cell") && !th.hasClass("k-hierarchy-cell");
            });
        },

        _filterable: function() {
            var that = this,
                columns = that.columns,
                filterMenu,
                cells,
                cell,
                filterInit = function(e) {
                    that.trigger(FILTERMENUINIT, { field: e.field, container: e.container });
                },
                closeCallback = function(element) {
                    focusTable(element.closest("table"), true);
                },
                filterable = that.options.filterable;

            if (filterable && !that.options.columnMenu) {
                cells = that._headerCells();

                for (var idx = 0, length = cells.length; idx < length; idx++) {
                    cell = cells.eq(idx);

                    if (columns[idx].filterable !== false && !columns[idx].command && (columns[idx].field || cell.attr("data-" + kendo.ns + "field"))) {
                        filterMenu = cell.data("kendoFilterMenu");

                        if (filterMenu) {
                            filterMenu.destroy();
                        }

                        var columnFilterable = columns[idx].filterable;

                        var options = extend({},
                            filterable,
                            columnFilterable,
                            {
                                dataSource: that.dataSource,
                                values: columns[idx].values,
                                closeCallback: closeCallback,
                                init: filterInit,
                                pane: that.pane
                            }
                        );

                        if (columnFilterable && columnFilterable.messages) {
                            options.messages = extend(true, {}, filterable.messages, columnFilterable.messages);
                        }

                        cell.kendoFilterMenu(options);
                    }
                }
            }
        },

        _sortable: function() {
            var that = this,
                columns = that.columns,
                column,
                sortableInstance,
                cell,
                sortable = that.options.sortable;

            if (sortable) {
                var cells = that._headerCells();

                for (var idx = 0, length = cells.length; idx < length; idx++) {
                    column = columns[idx];

                    if (column.sortable !== false && !column.command && column.field) {
                        cell = cells.eq(idx);

                        sortableInstance = cell.data("kendoSorter");

                        if (sortableInstance) {
                            sortableInstance.destroy();
                        }

                        cell.attr("data-" + kendo.ns +"field", column.field)
                            .kendoSorter(
                                extend({}, sortable, column.sortable, { dataSource: that.dataSource, aria: true, filter: ":not(.k-column-active)" })
                            );
                    }
                }
                cells = null;
            }
        },

        _columns: function(columns) {
            var that = this,
                table = that.table,
                encoded,
                cols = table.find("col"),
                lockedCols,
                dataSource = that.options.dataSource;

            // using HTML5 data attributes as a configuration option e.g. <th data-field="foo">Foo</foo>
            columns = columns.length ? columns : map(table.find("th"), function(th, idx) {
                th = $(th);
                var sortable = th.attr(kendo.attr("sortable")),
                    filterable = th.attr(kendo.attr("filterable")),
                    type = th.attr(kendo.attr("type")),
                    groupable = th.attr(kendo.attr("groupable")),
                    field = th.attr(kendo.attr("field")),
                    menu = th.attr(kendo.attr("menu"));

                if (!field) {
                   field = th.text().replace(/\s|[^A-z0-9]/g, "");
                }

                return {
                    field: field,
                    type: type,
                    sortable: sortable !== "false",
                    filterable: filterable !== "false",
                    groupable: groupable !== "false",
                    menu: menu,
                    template: th.attr(kendo.attr("template")),
                    width: cols.eq(idx).css("width")
                };
            });

            encoded = !(that.table.find("tbody tr").length > 0 && (!dataSource || !dataSource.transport));

            if (that.options.scrollable) {
                var initialColumns = columns;
                lockedCols = lockedColumns(columns);
                columns = nonLockedColumns(columns);

                if (lockedCols.length > 0 && columns.length === 0) {
                    throw new Error("There should be at least one non locked columns");
                }

                normalizeHeaderCells(that.element.find("tr:has(th):first").find("th:not(.k-group-cell)"), initialColumns);
                columns = lockedCols.concat(columns);
            }

            that.columns = map(columns, function(column) {
                column = typeof column === STRING ? { field: column } : column;
                if (column.hidden) {
                    column.attributes = addHiddenStyle(column.attributes);
                    column.footerAttributes = addHiddenStyle(column.footerAttributes);
                    column.headerAttributes = addHiddenStyle(column.headerAttributes);
                }

                return extend({ encoded: encoded }, column);
            });
        },

        _groups: function() {
            var group = this.dataSource.group();

            return group ? group.length : 0;
        },

        _tmpl: function(rowTemplate, columns, alt, skipGroupCells) {
            var that = this,
                settings = extend({}, kendo.Template, that.options.templateSettings),
                idx,
                length = columns.length,
                template,
                state = { storage: {}, count: 0 },
                column,
                type,
                hasDetails = that._hasDetails(),
                className = [],
                groups = that._groups();

            if (!rowTemplate) {
                rowTemplate = "<tr";

                if (alt) {
                    className.push("k-alt");
                }

                if (hasDetails) {
                    className.push("k-master-row");
                }

                if (className.length) {
                    rowTemplate += ' class="' + className.join(" ") + '"';
                }

                if (length) { // data item is an object
                    rowTemplate += ' ' + kendo.attr("uid") + '="#=' + kendo.expr("uid", settings.paramName) + '#"';
                }

                rowTemplate += " role='row'>";

                if (groups > 0 && !skipGroupCells) {
                    rowTemplate += groupCells(groups);
                }

                if (hasDetails) {
                    rowTemplate += '<td class="k-hierarchy-cell"><a class="k-icon k-i-plus" href="\\#" tabindex="-1"></a></td>';
                }

                for (idx = 0; idx < length; idx++) {
                    column = columns[idx];
                    template = column.template;
                    type = typeof template;

                    rowTemplate += "<td" + stringifyAttributes(column.attributes) + " role='gridcell'>";
                    rowTemplate += that._cellTmpl(column, state);

                    rowTemplate += "</td>";
                }

                rowTemplate += "</tr>";
            }

            rowTemplate = kendo.template(rowTemplate, settings);

            if (state.count > 0) {
                return proxy(rowTemplate, state.storage);
            }

            return rowTemplate;
        },

        _headerCellText: function(column) {
            var that = this,
                settings = extend({}, kendo.Template, that.options.templateSettings),
                template = column.headerTemplate,
                type = typeof(template),
                text = column.title || column.field || "";

            if (type === FUNCTION) {
                text = kendo.template(template, settings)({});
            } else if (type === STRING) {
                text = template;
            }
            return text;
        },

        _cellTmpl: function(column, state) {
            var that = this,
                settings = extend({}, kendo.Template, that.options.templateSettings),
                template = column.template,
                paramName = settings.paramName,
                field = column.field,
                html = "",
                idx,
                length,
                format = column.format,
                type = typeof template,
                columnValues = column.values;

            if (column.command) {
                if (isArray(column.command)) {
                    for (idx = 0, length = column.command.length; idx < length; idx++) {
                        html += that._createButton(column.command[idx]);
                    }
                    return html.replace(templateHashRegExp, "\\#");
                }
                return that._createButton(column.command).replace(templateHashRegExp, "\\#");
            }
            if (type === FUNCTION) {
                state.storage["tmpl" + state.count] = template;
                html += "#=this.tmpl" + state.count + "(" + paramName + ")#";
                state.count ++;
            } else if (type === STRING) {
                html += template;
            } else if (columnValues && columnValues.length && isPlainObject(columnValues[0]) && "value" in columnValues[0] && field) {
                html += "#var v =" + kendo.stringify(convertToObject(columnValues)) + "#";
                html += "#var f = v[";

                if (!settings.useWithBlock) {
                    html += paramName + ".";
                }

                html += field + "]#";
                html += "${f != null ? f : ''}";
            } else {
                html += column.encoded ? "#:" : "#=";

                if (format) {
                    html += 'kendo.format(\"' + format.replace(formatRegExp,"\\$1") + '\",';
                }

                if (field) {
                    field = kendo.expr(field, paramName);
                    html += field + "==null?'':" + field;
                } else {
                    html += "''";
                }

                if (format) {
                    html += ")";
                }

                html += "#";
            }
            return html;
        },

        _templates: function() {
            var that = this,
                options = that.options,
                dataSource = that.dataSource,
                groups = dataSource.group(),
                footer = that.footer || that.wrapper.find(".k-grid-footer"),
                aggregates = dataSource.aggregate(),
                columnsLocked = lockedColumns(that.columns),
                columns = options.scrollable ? nonLockedColumns(that.columns) : that.columns;

            if (options.scrollable && columnsLocked.length) {
                if (options.rowTemplate || options.altRowTemplate) {
                    throw new Error("Having both row template and locked columns is not supported");
                }

                that.rowTemplate = that._tmpl(options.rowTemplate, columns, false, true);
                that.altRowTemplate = that._tmpl(options.altRowTemplate || options.rowTemplate, columns, true, true);

                that.lockedRowTemplate = that._tmpl(options.rowTemplate, columnsLocked);
                that.lockedAltRowTemplate = that._tmpl(options.altRowTemplate || options.rowTemplate, columnsLocked, true);
            } else {
                that.rowTemplate = that._tmpl(options.rowTemplate, columns);
                that.altRowTemplate = that._tmpl(options.altRowTemplate || options.rowTemplate, columns, true);
            }

            if (that._hasDetails()) {
                that.detailTemplate = that._detailTmpl(options.detailTemplate || "");
            }

            if ((that._group && !isEmptyObject(aggregates)) || (!isEmptyObject(aggregates) && !footer.length) ||
                grep(that.columns, function(column) { return column.footerTemplate; }).length) {

                that.footerTemplate = that._footerTmpl(that.columns, aggregates, "footerTemplate", "k-footer-template");
            }

            if (groups && grep(that.columns, function(column) { return column.groupFooterTemplate; }).length) {
                aggregates = $.map(groups, function(g) { return g.aggregates; });

                that.groupFooterTemplate = that._footerTmpl(columns, aggregates, "groupFooterTemplate", "k-group-footer", columnsLocked.length);

                if (options.scrollable && columnsLocked.length) {
                    that.lockedGroupFooterTemplate = that._footerTmpl(columnsLocked, aggregates, "groupFooterTemplate", "k-group-footer");
                }
            }
        },

        _footerTmpl: function(columns, aggregates, templateName, rowClass, skipGroupCells) {
            var that = this,
                settings = extend({}, kendo.Template, that.options.templateSettings),
                paramName = settings.paramName,
                html = "",
                idx,
                length,
                template,
                type,
                storage = {},
                count = 0,
                scope = {},
                groups = that._groups(),
                fieldsMap = buildEmptyAggregatesObject(aggregates),
                column;

            html += '<tr class="' + rowClass + '">';

            if (groups > 0 && !skipGroupCells) {
                html += groupCells(groups);
            }

            if (that._hasDetails()) {
                html += '<td class="k-hierarchy-cell">&nbsp;</td>';
            }

            for (idx = 0, length = columns.length; idx < length; idx++) {
                column = columns[idx];
                template = column[templateName];
                type = typeof template;

                html += "<td" + stringifyAttributes(column.footerAttributes) + ">";

                if (template) {
                    if (type !== FUNCTION) {
                        scope = fieldsMap[column.field] ? extend({}, settings, { paramName: paramName + "." + column.field }) : {};
                        template = kendo.template(template, scope);
                    }

                    storage["tmpl" + count] = template;
                    html += "#=this.tmpl" + count + "(" + paramName + ")#";
                    count ++;
                } else {
                    html += "&nbsp;";
                }

                html += "</td>";
            }

            html += '</tr>';

            html = kendo.template(html, settings);

            if (count > 0) {
                return proxy(html, storage);
            }

            return html;
        },

        _detailTmpl: function(template) {
            var that = this,
                html = "",
                settings = extend({}, kendo.Template, that.options.templateSettings),
                paramName = settings.paramName,
                templateFunctionStorage = {},
                templateFunctionCount = 0,
                groups = that._groups(),
                colspan = visibleColumns(that.columns).length,
                type = typeof template;

            html += '<tr class="k-detail-row">';
            if (groups > 0) {
                html += groupCells(groups);
            }
            html += '<td class="k-hierarchy-cell"></td><td class="k-detail-cell"' + (colspan? ' colspan="' + colspan + '"' : '') + ">";

            if (type === FUNCTION) {
                templateFunctionStorage["tmpl" + templateFunctionCount] = template;
                html += "#=this.tmpl" + templateFunctionCount + "(" + paramName + ")#";
                templateFunctionCount ++;
            } else {
                html += template;
            }

            html += "</td></tr>";

            html = kendo.template(html, settings);

            if (templateFunctionCount > 0) {
                return proxy(html, templateFunctionStorage);
            }

            return html;
        },

        _hasDetails: function() {
            var that = this;

            return that.options.detailTemplate !== null  || (that._events[DETAILINIT] || []).length;
        },

        _details: function() {
            var that = this;

            if (that.options.scrollable && that._hasDetails() && lockedColumns(that.columns).length) {
                throw new Error("Having both detail template and locked columns is not supported");
            }

            that.table.on(CLICK + NS, ".k-hierarchy-cell .k-i-plus, .k-hierarchy-cell .k-i-minus", function(e) {
                var button = $(this),
                    expanding = button.hasClass("k-i-plus"),
                    masterRow = button.closest("tr.k-master-row"),
                    detailRow,
                    detailTemplate = that.detailTemplate,
                    data,
                    hasDetails = that._hasDetails();

                button.toggleClass("k-i-plus", !expanding)
                    .toggleClass("k-i-minus", expanding);

                if(hasDetails && !masterRow.next().hasClass("k-detail-row")) {
                    data = that.dataItem(masterRow);
                    $(detailTemplate(data))
                        .addClass(masterRow.hasClass("k-alt") ? "k-alt" : "")
                        .insertAfter(masterRow);

                    that.trigger(DETAILINIT, { masterRow: masterRow, detailRow: masterRow.next(), data: data, detailCell: masterRow.next().find(".k-detail-cell") });
                }

                detailRow = masterRow.next();

                that.trigger(expanding ? DETAILEXPAND : DETAILCOLLAPSE, { masterRow: masterRow, detailRow: detailRow});
                detailRow.toggle(expanding);

                if (that._current) {
                    that._current.attr("aria-expanded", expanding);
                }

                e.preventDefault();
                return false;
            });
        },

        dataItem: function(tr) {
            tr = $(tr)[0];
            if (!tr) {
                return null;
            }

            var rows = this.tbody.children(),
                classesRegEx = /k-grouping-row|k-detail-row|k-group-footer/,
                idx = tr.sectionRowIndex,
                j, correctIdx;

            correctIdx = idx;

            for (j = 0; j < idx; j++) {
                if (classesRegEx.test(rows[j].className)) {
                    correctIdx--;
                }
            }

            return this._data[correctIdx];
        },

        expandRow: function(tr) {
            $(tr).find('> td .k-i-plus, > td .k-i-expand').click();
        },

        collapseRow: function(tr) {
            $(tr).find('> td .k-i-minus, > td .k-i-collapse').click();
        },

        _createHeaderCells: function(columns) {
            var that = this,
                idx,
                th,
                text,
                html = "",
                length;

            for (idx = 0, length = columns.length; idx < length; idx++) {
                th = columns[idx];
                text = that._headerCellText(th);

                if (!th.command) {
                    html += "<th role='columnheader' " + kendo.attr("field") + "='" + (th.field || "") + "' ";
                    if (th.title) {
                        html += kendo.attr("title") + '="' + th.title.replace(/'/g, "\'") + '" ';
                    }

                    if (th.groupable !== undefined) {
                        html += kendo.attr("groupable") + "='" + th.groupable + "' ";
                    }

                    if (th.aggregates) {
                        html += kendo.attr("aggregates") + "='" + th.aggregates + "'";
                    }

                    html += stringifyAttributes(th.headerAttributes);

                    html += ">" + text + "</th>";
                } else {
                    html += "<th" + stringifyAttributes(th.headerAttributes) + ">" + text + "</th>";
                }
            }
            return html;
        },

        _appendLockedColumnContent: function() {
            var columns = this.columns,
                idx,
                colgroup = this.table.find("colgroup"),
                cols = colgroup.find("col:not(.k-group-col,.k-hierarchy-col)"),
                length,
                lockedCols = $(),
                container;

            for (idx = 0, length = columns.length; idx < length; idx++) {
                if (columns[idx].locked && !columns[idx].hidden) {
                    lockedCols = lockedCols.add(cols.eq(idx));
                }
            }

            container = $('<div class="k-grid-content-locked"><table' + (isIE7 ? ' cellspacing="0"' : '') + '><colgroup/><tbody></tbody></table></div>');
            // detach is required for IE8, otherwise it switches to compatibility mode
            colgroup.detach();
            container.find("colgroup").append(lockedCols);
            colgroup.insertBefore(this.table.find("tbody"));

            this.lockedContent = container.insertBefore(this.content);
            this.lockedTable = container.children("table");
        },

        _appendLockedColumnFooter: function() {
            var that = this;
            var footer = that.footer;
            var cells = footer.find(".k-footer-template>td");
            var cols = footer.find(".k-grid-footer-wrap>table>colgroup>col");
            var html = $('<div class="k-grid-footer-locked"><table><colgroup /><tbody><tr class="k-footer-template"></tr></tbody></table></div>');
            var idx, length;
            var groups = that._groups();
            var lockedCells = $(), lockedCols = $();

            lockedCells = lockedCells.add(cells.filter(".k-group-cell"));
            for (idx = 0, length = lockedColumns(that.columns).length; idx < length; idx++) {
                lockedCells = lockedCells.add(cells.eq(idx + groups));
            }

            lockedCols = lockedCols.add(cols.filter(".k-group-col"));
            for (idx = 0, length = visibleLockedColumns(that.columns).length; idx < length; idx++) {
                lockedCols = lockedCols.add(cols.eq(idx + groups));
            }

            lockedCells.appendTo(html.find("tr"));
            lockedCols.appendTo(html.find("colgroup"));
            that.lockedFooter = html.prependTo(footer);
        },

        _appendLockedColumnHeader: function(container) {
            var that = this,
                columns = this.columns,
                idx,
                html,
                length,
                colgroup,
                tr,
                table,
                header,
                skipHiddenCount = 0,
                cols = $(),
                cells = $();

            colgroup = that.thead.prev().find("col:not(.k-group-col,.k-hierarchy-col)");
            header = that.thead.find(".k-header:not(.k-group-cell,.k-hierarchy-cell)");

            for (idx = 0, length = columns.length; idx < length; idx++) {
                if (columns[idx].locked) {
                    if (!columns[idx].hidden) {
                        cols = cols.add(colgroup.eq(idx - skipHiddenCount));
                    }
                    cells = cells.add(header.eq(idx));
                }
                if (columns[idx].hidden) {
                    skipHiddenCount++;
                }
            }

            if (cells.length) {
                html = '<div class="k-grid-header-locked" style="width:1px"><table' + (isIE7 ? ' cellspacing="0"' : '') + '><colgroup/><thead><tr>' +
                    '</tr></thead></table></div>';

                table = $(html);

                colgroup = table.find("colgroup");
                tr = table.find("thead tr");

                colgroup.append(that.thead.prev().find("col.k-group-col").add(cols));
                tr.append(that.thead.find(".k-group-cell").add(cells));

                this.lockedHeader = table.prependTo(container);
                this._syncLockedHeaderHeight();
            }
        },

        _removeLockedContainers: function() {
            var elements = this.lockedHeader
                .add(this.lockedContent)
                .add(this.lockedFooter);

            elements.off(NS).remove();

            this.lockedHeader = this.lockedContent = this.lockedFooter = null;
        },

        _thead: function() {
            var that = this,
                columns = that.columns,
                hasDetails = that._hasDetails() && columns.length,
                idx,
                length,
                html = "",
                thead = that.table.find(">thead"),
                tr,
                text,
                th;

            if (!thead.length) {
                thead = $("<thead/>").insertBefore(that.tbody);
            }

            if (that.lockedHeader && that.thead) {
                tr = that.thead.find("tr:has(th):first").html("");

                that._removeLockedContainers();

            } else {
                tr = that.element.find("tr:has(th):first");
            }

            if (!tr.length) {
                tr = thead.children().first();
                if (!tr.length) {
                    tr = $("<tr/>");
                }
            }

            if (!tr.children().length) {
                if (hasDetails) {
                    html += '<th class="k-hierarchy-cell">&nbsp;</th>';
                }
                html += that._createHeaderCells(that.columns);

                tr.html(html);
            } else if (hasDetails && !tr.find(".k-hierarchy-cell")[0]) {
                tr.prepend('<th class="k-hierarchy-cell">&nbsp;</th>');
            }

            tr.attr("role", "row").find("th").addClass("k-header");

            if(!that.options.scrollable) {
                thead.addClass("k-grid-header");
            }

            tr.find("script").remove().end().appendTo(thead);

            if (that.thead) {
                that._destroyColumnAttachments();
            }

            that.thead = thead.attr("role", "rowgroup");

            that._sortable();

            that._filterable();

            that._scrollable();

            that._updateCols();

            that._columnMenu();

            if (this.options.scrollable && lockedColumns(this.columns).length) {

                that._appendLockedColumnHeader(that.thead.closest(".k-grid-header"));

                that._appendLockedColumnContent();

                that._applyLockedContainersWidth();
            }

            that._resizable();

            that._draggable();

            that._reorderable();

            if (that.groupable) {
                that._attachGroupable();
            }
        },

        _isLocked: function() {
            return this.lockedHeader != null;
        },

        _updateCols: function(table) {
            table = table || this.thead.parent().add(this.table);

            this._appendCols(table, this._isLocked());
        },

        _updateLockedCols: function(table) {
            if (this._isLocked()) {
                table = table || this.lockedHeader.find("table").add(this.lockedTable);

                normalizeCols(table, visibleLockedColumns(this.columns), this._hasDetails(), this._groups());
            }
        },

        _appendCols: function(table, locked) {
            if (locked) {
                normalizeCols(table, visibleNonLockedColumns(this.columns), this._hasDetails(), 0);
            } else {
                normalizeCols(table, visibleColumns(this.columns), this._hasDetails(), this._groups());
            }
        },

        _autoColumns: function(schema) {
            if (schema && schema.toJSON) {
                var that = this,
                    field;

                schema = schema.toJSON();

                for (field in schema) {
                    that.columns.push({ field: field });
                }

                that._thead();

                that._templates();
            }
        },

        _rowsHtml: function(data, templates) {
            var that = this,
                html = "",
                idx,
                rowTemplate = templates.rowTemplate,
                altRowTemplate = templates.altRowTemplate,
                length;

            for (idx = 0, length = data.length; idx < length; idx++) {
                if (idx % 2) {
                    html += altRowTemplate(data[idx]);
                } else {
                    html += rowTemplate(data[idx]);
                }

                that._data.push(data[idx]);
            }

            return html;
        },

        _groupRowHtml: function(group, colspan, level, groupHeaderBuilder, templates, skipColspan) {
            var that = this,
                html = "",
                idx,
                length,
                field = group.field,
                column = grep(that.columns, function(column) { return column.field == field; })[0] || { },
                template = column.groupHeaderTemplate,
                text =  (column.title || field) + ': ' + formatGroupValue(group.value, column.format, column.values),
                data = extend({}, { field: group.field, value: group.value }, group.aggregates[group.field]),
                footerDefaults = that._groupAggregatesDefaultObject || {},
                rowTemplate = templates.rowTemplate,
                altRowTemplate = templates.altRowTemplate,
                groupFooterTemplate = templates.groupFooterTemplate,
                groupItems = group.items;

            if (template) {
                text  = typeof template === FUNCTION ? template(data) : kendo.template(template)(data);
            }

            html += groupHeaderBuilder(colspan, level, text);

            if(group.hasSubgroups) {
                for(idx = 0, length = groupItems.length; idx < length; idx++) {
                    html += that._groupRowHtml(groupItems[idx], skipColspan ? colspan : colspan - 1, level + 1, groupHeaderBuilder, templates, skipColspan);
                }
            } else {
                html += that._rowsHtml(groupItems, templates);
            }

            if (groupFooterTemplate) {
                html += groupFooterTemplate(extend(footerDefaults, group.aggregates));
            }
            return html;
        },

        collapseGroup: function(group) {
            group = $(group);

            var level,
                footerCount = 1,
                offset,
                relatedGroup = $(),
                idx,
                length,
                tr;

            if (this._isLocked()) {
                if (!group.closest("div").hasClass("k-grid-content-locked")) {
                    relatedGroup = group.nextAll("tr");
                    group = this.lockedTable.find(">tbody>tr:eq(" + group.index() + ")");
                } else {
                    relatedGroup = this.tbody.children("tr:eq(" + group.index() + ")").nextAll("tr");
                }
            }

            level = group.find(".k-group-cell").length;
            group.find(".k-icon").addClass("k-i-expand").removeClass("k-i-collapse");
            group.find("td:first").attr("aria-expanded", false);
            group = group.nextAll("tr");

            for (idx = 0, length = group.length; idx < length; idx ++ ) {
                tr = group.eq(idx);
                offset = tr.find(".k-group-cell").length;

                if (tr.hasClass("k-grouping-row")) {
                    footerCount++;
                } else if (tr.hasClass("k-group-footer")) {
                    footerCount--;
                }

                if (offset <= level || (tr.hasClass("k-group-footer") && footerCount < 0)) {
                    break;
                }

                tr.hide();
                relatedGroup.eq(idx).hide();
            }
        },

        expandGroup: function(group) {
            group = $(group);

            var that = this,
                level,
                tr,
                offset,
                relatedGroup = $(),
                idx,
                length,
                groupsCount = 1;

            if (this._isLocked()) {
                if (!group.closest("div").hasClass("k-grid-content-locked")) {
                    relatedGroup = group.nextAll("tr");
                    group = this.lockedTable.find(">tbody>tr:eq(" + group.index() + ")");
                } else {
                    relatedGroup = this.tbody.children("tr:eq(" + group.index() + ")").nextAll("tr");
                }
            }

            level = group.find(".k-group-cell").length;
            group.find(".k-icon").addClass("k-i-collapse").removeClass("k-i-expand");
            group.find("td:first").attr("aria-expanded", true);
            group = group.nextAll("tr");

            for (idx = 0, length = group.length; idx < length; idx ++ ) {
                tr = group.eq(idx);
                offset = tr.find(".k-group-cell").length;
                if (offset <= level) {
                    break;
                }

                if (offset == level + 1 && !tr.hasClass("k-detail-row")) {
                    tr.show();
                    relatedGroup.eq(idx).show();

                    if (tr.hasClass("k-grouping-row") && tr.find(".k-icon").hasClass("k-i-collapse")) {
                        that.expandGroup(tr);
                    }

                    if (tr.hasClass("k-master-row") && tr.find(".k-icon").hasClass("k-minus")) {
                        tr.next().show();
                        relatedGroup.eq(idx + 1).show();
                    }
                }

                if (tr.hasClass("k-grouping-row")) {
                    groupsCount ++;
                }

                if (tr.hasClass("k-group-footer")) {
                    if (groupsCount == 1) {
                        tr.show();
                        relatedGroup.eq(idx).show();
                    } else {
                        groupsCount --;
                    }
                }
            }
        },

        _updateHeader: function(groups) {
            var that = this,
                container = that._isLocked() ? that.lockedHeader : that.thead,
                cells = container.find("th.k-group-cell"),
                length = cells.length;

            if(groups > length) {
                $(new Array(groups - length + 1).join('<th class="k-group-cell k-header">&nbsp;</th>')).prependTo(container.find("tr"));
            } else if(groups < length) {
                length = length - groups;
                $(grep(cells, function(item, index) { return length > index; } )).remove();
            }
        },

        _firstDataItem: function(data, grouped) {
            if(data && grouped) {
                if(data.hasSubgroups) {
                    data = this._firstDataItem(data.items[0], grouped);
                } else {
                    data = data.items[0];
                }
            }
            return data;
        },

        _updateTablesWidth: function() {
            var that = this,
                tables;

            if (!that._isLocked()) {
                return;
            }

            tables =
                $(">.k-grid-footer>.k-grid-footer-wrap>table", that.wrapper)
                .add(that.thead.parent())
                .add(that.table);

            that._footerWidth = tableWidth(tables.eq(0));
            tables.width(that._footerWidth);

            tables =
                $(">.k-grid-footer>.k-grid-footer-locked>table", that.wrapper)
                .add(that.lockedHeader.find(">table"))
                .add(that.lockedTable);

            tables.width(tableWidth(tables.eq(0)));
        },

        hideColumn: function(column) {
            var that = this,
                cell,
                tables,
                idx,
                cols,
                colWidth,
                width = 0,
                length,
                footer = that.footer || that.wrapper.find(".k-grid-footer"),
                columns = that.columns,
                visibleLocked = visibleLockedColumns(columns).length,
                columnIndex;

            if (typeof column == "number") {
                column = columns[column];
            } else {
                column = grep(columns, function(item) {
                    return item.field === column;
                })[0];
            }

            if (!column || column.hidden) {
                return;
            }

            columnIndex = inArray(column, visibleColumns(columns));
            column.hidden = true;
            column.attributes = addHiddenStyle(column.attributes);
            column.footerAttributes = addHiddenStyle(column.footerAttributes);
            column.headerAttributes = addHiddenStyle(column.headerAttributes);
            that._templates();

            that._updateCols();
            that._updateLockedCols();
            setCellVisibility(elements($(">table>thead", that.lockedHeader), that.thead, ">tr>th"), columnIndex, false);
            if (footer[0]) {
                that._updateCols(footer.find(">.k-grid-footer-wrap>table"));
                that._updateLockedCols(footer.find(">.k-grid-footer-locked>table"));
                setCellVisibility(footer.find(".k-footer-template>td"), columnIndex, false);
            }

            if (that.lockedTable && visibleLocked > columnIndex) {
                hideColumnCells(that.lockedTable.find(">tbody>tr"), columnIndex);
            } else {
                hideColumnCells(that.tbody.children(), columnIndex - visibleLocked);
            }

            if (that.lockedTable) {
                that._updateTablesWidth();
                that._applyLockedContainersWidth();
                that._syncLockedContentHeight();
                that._syncLockedHeaderHeight();
                that._syncLockedFooterHeight();
            } else {
                cols = that.thead.prev().find("col");
                for (idx = 0, length = cols.length; idx < length; idx += 1) {
                    colWidth = cols[idx].style.width;
                    if (colWidth && colWidth.indexOf("%") == -1) {
                        width += parseInt(colWidth, 10);
                    } else {
                        width = 0;
                        break;
                    }
                }

                tables = $(">.k-grid-header table:first,>.k-grid-footer table:first",that.wrapper)
                .add(that.table);
                that._footerWidth = null;

                if (width) {
                    tables.width(width);
                    that._footerWidth = width;
                }

                if(browser.msie && browser.version == 8) {
                    tables.css("display", "inline-table");
                    setTimeout(function() {
                        tables.css("display", "table");
                    }, 1);
                }
            }

            that.trigger(COLUMNHIDE, { column: column });
        },

        showColumn: function(column) {
            var that = this,
                idx,
                length,
                cell,
                tables,
                width,
                colWidth,
                cols,
                columns = that.columns,
                footer = that.footer || that.wrapper.find(".k-grid-footer"),
                lockedColumnsCount = lockedColumns(columns).length,
                columnIndex;

            if (typeof column == "number") {
                column = columns[column];
            } else {
                column = grep(columns, function(item) {
                    return item.field === column;
                })[0];
            }

            if (!column || !column.hidden) {
                return;
            }

            columnIndex = inArray(column, columns);
            column.hidden = false;
            column.attributes = removeHiddenStyle(column.attributes);
            column.footerAttributes = removeHiddenStyle(column.footerAttributes);
            column.headerAttributes = removeHiddenStyle(column.headerAttributes);
            that._templates();

            that._updateCols();
            that._updateLockedCols();
            setCellVisibility(elements($(">table>thead", that.lockedHeader), that.thead, ">tr>th"), columnIndex, true);
            if (footer[0]) {
                that._updateCols(footer.find(">.k-grid-footer-wrap>table"));
                that._updateLockedCols(footer.find(">.k-grid-footer-locked>table"));
                setCellVisibility(footer.find(".k-footer-template>td"), columnIndex, true);
            }

            if (that.lockedTable && lockedColumnsCount > columnIndex) {
                showColumnCells(that.lockedTable.find(">tbody>tr"), columnIndex);
            } else {
                showColumnCells(that.tbody.children(), columnIndex - lockedColumnsCount);
            }

            if (that.lockedTable) {
                that._updateTablesWidth();
                that._applyLockedContainersWidth();
                that._syncLockedContentHeight();
                that._syncLockedHeaderHeight();
            } else {
                tables = $(">.k-grid-header table:first,>.k-grid-footer table:first",that.wrapper).add(that.table);
                if (!column.width) {
                    tables.width("");
                } else {
                    width = 0;
                    cols = that.thead.prev().find("col");
                    for (idx = 0, length = cols.length; idx < length; idx += 1) {
                        colWidth = cols[idx].style.width;
                        if (colWidth.indexOf("%") > -1) {
                            width = 0;
                            break;
                        }
                        width += parseInt(colWidth, 10);
                    }

                    that._footerWidth = null;
                    if (width) {
                        tables.width(width);
                        that._footerWidth = width;
                    }
                }
            }

            that.trigger(COLUMNSHOW, { column: column });
        },

        _progress: function(toggle) {
            var element = this.element;

            if (this.lockedContent) {
                element = this.wrapper;
            } else if (this.element.is("table")) {
                element = this.element.parent();
            } else if (this.content && this.content.length) {
                element = this.content;
            }

            kendo.ui.progress(element, toggle);
        },

        _resize: function() {
            if (this.content) {
                this._setContentHeight();
                this._setContentWidth();
            }
        },

        _isActiveInTable: function() {
            var active = activeElement();

            return this.table[0] === active ||
                $.contains(this.table[0], active) ||
                (this._isLocked() &&
                    (this.lockedTable[0] === active || $.contains(this.lockedTable[0], active))
                );
        },

        refresh: function(e) {
            var that = this,
                length,
                idx,
                html = "",
                data = that.dataSource.view(),
                navigatable = that.options.navigatable,
                currentIndex,
                current = $(that.current()),
                isCurrentInHeader = false,
                groups = (that.dataSource.group() || []).length,
                colspan = groups + visibleColumns(that.columns).length;

            if (e && e.action === "itemchange" && that.editable) { // skip rebinding if editing is in progress
                return;
            }

            e = e || {};

            if (that.trigger("dataBinding", { action: e.action || "rebind", index: e.index, items: e.items })) {
                return;
            }

            if (navigatable && (that._isActiveInTable() || (that._editContainer && that._editContainer.data("kendoWindow")))) {
                isCurrentInHeader = current.is("th");
                currentIndex = 0;
                if (isCurrentInHeader) {
                    currentIndex = that.thead.find("th:not(.k-group-cell)").index(current);
                }
            }

            that._destroyEditable();

            that._progress(false);

            that._hideResizeHandle();

            that._data = [];

            if (!that.columns.length) {
                that._autoColumns(that._firstDataItem(data[0], groups));
                colspan = groups + that.columns.length;
            }

            that._group = groups > 0 || that._group;

            if(that._group) {
                that._templates();
                that._updateCols();
                that._updateLockedCols();
                that._updateHeader(groups);
                that._group = groups > 0;
            }

            that._renderContent(data, colspan, groups);

            that._renderLockedContent(data, colspan, groups);

            that._footer();

            that._setContentHeight();

            that._setContentWidth();

            if (currentIndex >= 0) {
                that._removeCurrent();
                if (!isCurrentInHeader) {
                    that.current(that.table.add(that.lockedTable).find(FIRSTNAVITEM).first());
                } else {
                    that.current(that.thead.find("th:not(.k-group-cell)").eq(currentIndex));
                }

                if (that._current) {
                    focusTable(that._current.closest("table")[0], true);
                }
            }

            that.trigger(DATABOUND);
       },

       _renderContent: function(data, colspan, groups) {
            var that = this,
                idx,
                length,
                html = "",
                isLocked = that.lockedContent != null,
                templates = {
                        rowTemplate: that.rowTemplate,
                        altRowTemplate: that.altRowTemplate,
                        groupFooterTemplate: that.groupFooterTemplate
                    };

            colspan = isLocked ? colspan - visibleLockedColumns(that.columns).length : colspan;

            if(groups > 0) {

                colspan = isLocked ? colspan - groups : colspan;

                if (that.detailTemplate) {
                    colspan++;
                }

                if (that.groupFooterTemplate) {
                    that._groupAggregatesDefaultObject = buildEmptyAggregatesObject(that.dataSource.aggregate());
                }

                for (idx = 0, length = data.length; idx < length; idx++) {
                    html += that._groupRowHtml(data[idx], colspan, 0, isLocked ? groupRowLockedContentBuilder : groupRowBuilder, templates, isLocked);
                }
            } else {
                html += that._rowsHtml(data, templates);
            }

            that.tbody = appendContent(that.tbody, that.table, html);
       },

       _renderLockedContent: function(data, colspan, groups) {
           var html = "",
               idx,
               length,
               templates = {
                   rowTemplate: this.lockedRowTemplate,
                   altRowTemplate: this.lockedAltRowTemplate,
                   groupFooterTemplate: this.lockedGroupFooterTemplate
               };

           if (this.lockedContent) {

               var table = this.lockedTable;

               if (groups > 0) {
                   colspan = colspan - visibleNonLockedColumns(this.columns).length;
                   for (idx = 0, length = data.length; idx < length; idx++) {
                       html += this._groupRowHtml(data[idx], colspan, 0, groupRowBuilder, templates);
                   }
               } else {
                   html = this._rowsHtml(data, templates);
               }

               appendContent(table.children("tbody"), table, html);

               this._syncLockedContentHeight();
           }
       },

       _adjustRowsHeight: function(table1, table2) {
          var rows = table1[0].rows,
            length = rows.length,
            idx,
            rows2 = table2[0].rows,
            containers = table1.add(table2),
            containersLength = containers.length,
            heights = [];

          for (idx = 0; idx < length; idx++) {
              if (rows[idx].style.height) {
                  rows[idx].style.height = rows2[idx].style.height = "";
              }

              var offsetHeight1 = rows[idx].offsetHeight;
              var offsetHeight2 = rows2[idx].offsetHeight;
              var height = 0;

              if (offsetHeight1 > offsetHeight2) {
                  height = offsetHeight1;
              } else if (offsetHeight1 < offsetHeight2) {
                  height = offsetHeight2;
              }

              heights.push(height);
          }

          for (idx = 0; idx < containersLength; idx++) {
              containers[idx].style.display = "none";
          }

          for (idx = 0; idx < length; idx++) {
              if (heights[idx]) {
                  rows[idx].style.height = rows2[idx].style.height = heights[idx] + "px";
              }
          }

          for (idx = 0; idx < containersLength; idx++) {
              containers[idx].style.display = "";
          }
       }
   });

   function adjustRowHeight(row1, row2) {
       var height;
       var clientHeight1 = row1.clientHeight;
       var clientHeight2 = row2.clientHeight;

       if (clientHeight1 > clientHeight2) {
           height = clientHeight1 + "px";
       } else if (clientHeight1 < clientHeight2) {
           height = clientHeight2 + "px";
       }

       if (height) {
           row1.style.height = row2.style.height = height;
       }
   }


   function getCommand(commands, name) {
       var idx, length, command;

       if (typeof commands === STRING && commands === name) {
          return commands;
       }

       if (isPlainObject(commands) && commands.name === name) {
           return commands;
       }

       if (isArray(commands)) {
           for (idx = 0, length = commands.length; idx < length; idx++) {
               command = commands[idx];

               if ((typeof command === STRING && command === name) || (command.name === name)) {
                   return command;
               }
           }
       }
       return null;
   }

   function focusTable(table, direct) {
       var msie = browser.msie;
       if (direct === true) {
           table = $(table);
           var condition = msie && table.parent().is(".k-grid-content,.k-grid-header-wrap"),
               scrollTop, scrollLeft;
           if (condition) {
               scrollTop = table.parent().scrollTop();
               scrollLeft = table.parent().scrollLeft();
           }

           if (msie) {
               try {
                   //The setActive method does not cause the document to scroll to the active object in the current page
                   table[0].setActive();
               } catch(e) {
                   table[0].focus();
               }
           } else {
               table[0].focus(); //because preventDefault bellow, IE cannot focus the table alternative is unselectable=on
           }

           if (condition) {
               table.parent().scrollTop(scrollTop);
               table.parent().scrollLeft(scrollLeft);
           }

       } else {
           $(table).one("focusin", function(e) { e.preventDefault(); }).focus();
       }
   }

   function tableClick(e) {
       var currentTarget = $(e.currentTarget),
           isHeader = currentTarget.is("th"),
           table = this.table.add(this.lockedTable),
           headerTable = this.thead.parent().add($(">table", this.lockedHeader)),
           currentTable = currentTarget.closest("table")[0];

       if (kendo.support.touch) {
           return;
       }

       if (currentTable !== table[0] && currentTable !== table[1] && currentTable !== headerTable[0] && currentTable !== headerTable[1]) {
           return;
       }

       this.current(currentTarget);

       if (isHeader || !$(e.target).is(":button,a,:input,a>.k-icon,textarea,span.k-icon,span.k-link,.k-input,.k-multiselect-wrap")) {
           setTimeout(function() {
               //Do not focus if widget, because in IE8 a DDL will be closed
               if (!(isIE8 && $(kendo._activeElement()).hasClass("k-widget"))) {
                    //DOMElement.focus() only for header, because IE doesn't really focus the table
                    focusTable(currentTable, true);
                }
           });
       }

       if (isHeader) {
           e.preventDefault(); //if any problem occurs, call preventDefault only for the clicked header links
       }
   }

   function verticalTable(current, downTable, upTable, up) {
       current = $(current);
       if (up) {
           var temp = downTable;
           downTable = upTable;
           upTable = temp;
       }

       if (downTable.not(current).length != downTable.length) {
           return current;
       }

       return current[0] == upTable[0] ?
                   downTable.eq(0) : downTable.eq(1);
   }

   function moveVertical(current, currentTable, dataTable, headerTable, up) {
       var row, index;
       var nextFn = up ? "prevAll" : "nextAll";

       if (current) {
           row = current.parent()[nextFn](NAVROW).first();
           if (!row[0] && (up || current.is("th"))) {
               currentTable = verticalTable(currentTable, dataTable, headerTable, up);
               focusTable(currentTable);
               row = currentTable.find((up ? ">thead>" : ">tbody>") + NAVROW).first();
           }
           index = current.index();
           current = row.children().eq(index);
           if (!current[0] || !current.is(NAVCELL)) {
               current = row.children(NAVCELL).first();
           }
       } else {
           current = dataTable.find(FIRSTNAVITEM);
       }

       return current;
   }

   function moveLeft(current, currentTable, dataTable, headerTable, relatedRow) {
       var isLocked = dataTable.length > 1;

       if (current) {
           if (current.prev()[0]) {
               current = current.prevAll(DATA_CELL).first();
           } else if (isLocked) {
               if (currentTable == dataTable[1]) {
                   focusTable(dataTable[0]);
                   current = relatedRow(current.parent()).children(DATA_CELL).last();
               } else if (currentTable == headerTable[1]) {
                   focusTable(headerTable[0]);
                   current = headerTable.eq(0).find("tr>" + DATA_CELL).last();
               }
           }
       } else {
           current = dataTable.find(FIRSTNAVITEM);
       }

       return current;
   }

   function moveRight(current, currentTable, dataTable, headerTable, relatedRow) {
       var isLocked = dataTable.length > 1;

       if (current) {
           if (current.next()[0]) {
               current = current.nextAll(DATA_CELL).first();
           } else if (isLocked) {
               if (currentTable == dataTable[0]) {
                   focusTable(dataTable[1]);
                   current = relatedRow(current.parent()).children(DATA_CELL).first();
               } else if (currentTable == headerTable[0]) {
                   focusTable(headerTable[1]);
                   current = headerTable.eq(1).find("tr>" + DATA_CELL).first();
               }
           }
       } else {
           current = dataTable.find(FIRSTNAVITEM);
       }

       return current;
   }

   function tabNext(current, currentTable, dataTable, relatedRow, back) {
       var isLocked = dataTable.length == 2;
       var switchRow = true;
       var next = back ? current.prevAll(DATA_CELL + ":first") : current.nextAll(":visible:first");

       if (!next.length) {
           next = current.parent();
           if (isLocked) {
               switchRow = (back && currentTable == dataTable[0]) || (!back && currentTable == dataTable[1]);
               next = relatedRow(next);
           }

           if (switchRow) {
               next = next[back ? "prevAll" : "nextAll"]("tr:not(.k-grouping-row):not(.k-detail-row):visible:first");
           }
           next = next.children(DATA_CELL + (back ? ":last" : ":first"));
       }

       return next;
   }

   function groupRowBuilder(colspan, level, text) {
       return '<tr class="k-grouping-row">' + groupCells(level) +
           '<td colspan="' + colspan + '" aria-expanded="true">' +
           '<p class="k-reset">' +
           '<a class="k-icon k-i-collapse" href="#" tabindex="-1"></a>' + text +
       '</p></td></tr>';
   }

   function groupRowLockedContentBuilder(colspan, level, text) {
       return '<tr class="k-grouping-row">' +
           '<td colspan="' + colspan + '" aria-expanded="true">' +
           '<p class="k-reset">&nbsp;</p></td></tr>';
   }

    var Sorter = Widget.extend({
       init: function(element, options) {
           var that = this, link;

           Widget.fn.init.call(that, element, options);

           that._refreshHandler = proxy(that.refresh, that);

           that.dataSource = that.options.dataSource.bind("change", that._refreshHandler);

           link = that.element.find(TLINK);

           if (!link[0]) {
               link = that.element.wrapInner('<a class="k-link" href="#"/>').find(TLINK);
           }

           that.link = link;

           that.element.on("click" + sorterNS, proxy(that._click, that));
       },

       options: {
           name: "Sorter",
           mode: SINGLE,
           allowUnsort: true,
           compare: null,
           filter: ""
       },

       destroy: function() {
           var that = this;

           Widget.fn.destroy.call(that);

           that.element.off(sorterNS);

           that.dataSource.unbind("change", that._refreshHandler);
           that._refreshHandler = that.element = that.link = that.dataSource = null ;
       },

       refresh: function() {
           var that = this,
               sort = that.dataSource.sort() || [],
               idx,
               length,
               descriptor,
               dir,
               element = that.element,
               field = element.attr(kendo.attr(FIELD));

           element.removeAttr(kendo.attr(DIR));
           element.removeAttr(ARIASORT);

           for (idx = 0, length = sort.length; idx < length; idx++) {
               descriptor = sort[idx];

               if (field == descriptor.field) {
                   element.attr(kendo.attr(DIR), descriptor.dir);
               }
           }

           dir = element.attr(kendo.attr(DIR));

           element.find(".k-i-arrow-n,.k-i-arrow-s").remove();

           if (dir === ASC) {
               $('<span class="k-icon k-i-arrow-n" />').appendTo(that.link);
               element.attr(ARIASORT, "ascending");
           } else if (dir === DESC) {
               $('<span class="k-icon k-i-arrow-s" />').appendTo(that.link);
               element.attr(ARIASORT, "descending");
           }
       },

       _click: function(e) {
           var that = this,
           element = that.element,
           field = element.attr(kendo.attr(FIELD)),
           dir = element.attr(kendo.attr(DIR)),
           options = that.options,
           compare = that.options.compare == null ? undefined : that.options.compare,
           sort = that.dataSource.sort() || [],
           idx,
           length;

           e.preventDefault();

           if (options.filter && !element.is(options.filter)) {
               return;
           }

           if (dir === ASC) {
               dir = DESC;
           } else if (dir === DESC && options.allowUnsort) {
               dir = undefined;
           } else {
               dir = ASC;
           }

           if (options.mode === SINGLE) {
               sort = [ { field: field, dir: dir, compare: compare } ];
           } else if (options.mode === "multiple") {
               for (idx = 0, length = sort.length; idx < length; idx++) {
                   if (sort[idx].field === field) {
                       sort.splice(idx, 1);
                       break;
                   }
               }
               sort.push({ field: field, dir: dir, compare: compare });
           }


           that.dataSource.sort(sort);
       }
   });

   ui.plugin(Grid);
   ui.plugin(VirtualScrollable);
   ui.plugin(Sorter);

})(window.kendo.jQuery);

return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });
