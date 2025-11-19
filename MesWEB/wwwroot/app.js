// --- getInputValue.js ---
window.getInputValue = (input) => {
    if (!input) return "";

    const el = input instanceof HTMLElement ? input : document.getElementById(input);
    if (!el) return "";

    return el.value;
}

// --- insertAtCursorAndFocus.js ---
window.insertAtCursorAndFocus = (input, symbol) => {
    if (!input) return;

    const el = input instanceof HTMLElement ? input : document.getElementById(input);
    if (!el) return;

    const start = el.selectionStart;
    const end = el.selectionEnd;
    const value = el.value;

    el.value = value.substring(0, start) + symbol + value.substring(end);
    el.selectionStart = el.selectionEnd = start + symbol.length;
    el.focus();
}

// --- autocomplete.js ---
window.autocomplete = (function () {
    return {
        init: function (elementId, suggestions) {
            try {
                var el = document.getElementById(elementId);
                if (!el) return;
                // If Awesomplete is not loaded, do nothing
                if (typeof Awesomplete === 'undefined') return;

                // If instance already exists, update list
                if (el.awesomplete) {
                    el.awesomplete.list = suggestions || [];
                } else {
                    // Create new instance
                    el.awesomplete = new Awesomplete(el, {
                        list: suggestions || [],
                        minChars: 0,
                        maxItems: 10,
                        autoFirst: true,
                        replace: function(suggestion) {
                            // Custom replace function to ensure the input gets the suggestion text
                            this.input.value = suggestion;
                        }
                    });

                    // Show suggestions on focus
                    el.addEventListener('focus', function () {
                        if (el.value === '') {
                            el.awesomplete.evaluate();
                        }
                    });

                    // Show suggestions on click (for mobile)
                    el.addEventListener('click', function () {
                        if (el.value === '') {
                            el.awesomplete.evaluate();
                        }
                    });

                    // Handle selection completion
                    el.addEventListener('awesomplete-selectcomplete', function (e) {
                        console.log('Android Autocomplete: selectcomplete event for', elementId, 'value:', el.value);
                        
                        // Manually trigger multiple events to ensure Blazor recognizes the change
                        var events = ['input', 'change', 'blur'];
                        events.forEach(eventType => {
                            var event = new Event(eventType, { 
                                bubbles: true, 
                                cancelable: true 
                            });
                            el.dispatchEvent(event);
                        });
                        
                        // Force a small delay and trigger again to ensure Blazor processes
                        setTimeout(() => {
                            el.dispatchEvent(new Event('input', { bubbles: true }));
                            console.log('Android Autocomplete: delayed input event fired for', elementId);
                        }, 100);
                    });

                    // Also handle the select event (before completion)
                    el.addEventListener('awesomplete-select', function (e) {
                        console.log('Android Autocomplete: select event for', elementId, 'selection:', e.text);
                        
                        // Set value immediately
                        setTimeout(() => {
                            el.value = e.text.value || e.text;
                            el.dispatchEvent(new Event('input', { bubbles: true }));
                            el.dispatchEvent(new Event('change', { bubbles: true }));
                            console.log('Android Autocomplete: value set to', el.value);
                        }, 50);
                    });

                    // For additional safety, monitor value changes and ensure Blazor is notified
                    var lastValue = el.value;
                    setInterval(() => {
                        if (el.value !== lastValue) {
                            console.log('Android Autocomplete: value changed detected for', elementId, 'from', lastValue, 'to', el.value);
                            lastValue = el.value;
                            el.dispatchEvent(new Event('input', { bubbles: true }));
                        }
                    }, 500);
                }
            } catch (e) {
                console.error('autocomplete.init error', e);
            }
        },
        update: function (elementId, suggestions) {
            var el = document.getElementById(elementId);
            if (!el || !el.awesomplete) return;
            el.awesomplete.list = suggestions || [];
        },
        destroy: function (elementId) {
            var el = document.getElementById(elementId);
            if (!el || !el.awesomplete) return;
            // Awesomplete doesn't provide destroy; remove reference
            el.awesomplete = null;
        }
    };
})();

// --- device-detection.js ---
window.deviceDetection = {
    getUserAgent: function() {
        return navigator.userAgent;
    },
    
    detectDevice: function() {
        const userAgent = navigator.userAgent.toLowerCase();
        const platform = navigator.platform.toLowerCase();
        const vendor = navigator.vendor.toLowerCase();
        
        // Android detection
        const isAndroid = /android/.test(userAgent) || 
          /android/.test(platform) ||
           userAgent.includes('mobile') && userAgent.includes('safari') && !userAgent.includes('chrome') === false;

        return {
            userAgent: navigator.userAgent,
       platform: navigator.platform,
   vendor: navigator.vendor,
    isAndroid: isAndroid,
     isMobile: /mobi|android|iphone|ipad|blackberry|opera mini|iemobile/i.test(userAgent),
         screen: {
         width: screen.width,
        height: screen.height
},
     window: {
       width: window.innerWidth,
          height: window.innerHeight
   }
      };
    }
};

// --- file-download.js ---
window.downloadFileFromBase64 = function(base64String, fileName, contentType) {
    try {
        // Base64文字列をバイト配列に変換
    const byteCharacters = atob(base64String);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
     }
        const byteArray = new Uint8Array(byteNumbers);
        
        // Blobを作成
        const blob = new Blob([byteArray], { type: contentType || 'application/octet-stream' });
        
        // ダウンロードリンクを作成してクリック
        const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
     document.body.appendChild(link);
 link.click();
        
        // クリーンアップ
     setTimeout(() => {
 document.body.removeChild(link);
       URL.revokeObjectURL(url);
        }, 100);
        
        return true;
    } catch (error) {
        console.error('ファイルダウンロードエラー:', error);
        return false;
    }
};

// ExcelCompare.razor で使用される downloadFile 関数を追加
window.downloadFile = function(fileName, base64String) {
    return window.downloadFileFromBase64(base64String, fileName, 'text/csv;charset=utf-8');
};
