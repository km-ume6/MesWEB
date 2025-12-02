// SortableJS init for mapping-list
(function(){
    if (typeof Sortable === 'undefined') return;

    window.initMappingSortable = function (dotNetHelper) {
        var el = document.getElementById('mapping-list');
        if (!el) return;

        // destroy previous sortable if exists
        if (el._sortableInstance) {
            el._sortableInstance.destroy();
            el._sortableInstance = null;
        }

        var sortable = Sortable.create(el, {
            handle: '.drag-handle',
            animation: 150,
            onEnd: function (evt) {
                // evt.oldIndex, evt.newIndex
                try {
                    dotNetHelper.invokeMethodAsync('OnSortableEnd', evt.oldIndex, evt.newIndex);
                } catch (e) {
                    console.error('invoke OnSortableEnd failed', e);
                }
            }
        });

        el._sortableInstance = sortable;
    };

    // Scroll helper function - scroll to the container's top or bottom
    window.scrollToElement = function (elementId, position) {
        var el = document.getElementById(elementId);
        if (!el) return;

        var firstChild = el.firstElementChild;
        var lastChild = el.lastElementChild;

        if (position === 'end' && lastChild) {
            // scroll to last child with smooth animation
            lastChild.scrollIntoView({ behavior: 'smooth', block: 'end' });
        } else if (position === 'start' && firstChild) {
            // scroll to first child with smooth animation
            firstChild.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    };
})();
