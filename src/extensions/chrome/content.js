var scriptElement = document.createElement('script');

scriptElement.src = chrome.runtime.getURL('stealth.js');

scriptElement.onload = function() {

    this.remove();

};

(document.head || document.documentElement).appendChild(scriptElement);