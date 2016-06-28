;(function() {
	'use strict';

	if (typeof Blob !== 'function') {
		console.log('Blob does not exist');
		return;
	}

	console.save = console.save || function(data, filename, spacing) {
		if (!data) {
			console.error('Console save error: no data provided');
			return;
		}

		if (!filename) {
			filename = 'console.json';
		}

		if (typeof data === 'object') {
			data = JSON.stringify(data, undefined, typeof spacing !== 'undefined' ? spacing : 2);
		}

		var type = 'text/json',
			blob = new Blob([data], {
				type: type
			});

		if (window.navigator.msSaveOrOpenBlob) {
			window.navigator.msSaveOrOpenBlob(blob, filename);
		}
		else {
			var a = document.createElement('a'),
				e = document.createEvent('MouseEvents');

			e.initMouseEvent('click', true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null);
			a.href = window.URL.createObjectURL(blob);
			a.download = filename;
			a.dispatchEvent(e);
		}
	};
})();

;(function() {
	'use strict';

	Array.prototype.findIndex = Array.prototype.findIndex || function(predicate) {
		if (this === null) {
			throw new TypeError('Array.prototype.findIndex called on null or undefined');
		}

		if (typeof predicate !== 'function') {
			throw new TypeError('predicate must be a function');
		}

		var list = Object(this),
			length = list.length >>> 0,
			arg = arguments[1],
			value;

		for (var i = 0; i < length; i++) {
			value = list[i];

			if (predicate.call(arg, value, i, list)) return i;
		}

		return -1;
	};
})();