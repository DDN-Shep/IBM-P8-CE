;(function main() {
	'use strict';

	toastr.options.newestOnTop = true;
	toastr.options.positionClass = 'toast-bottom-right';
	toastr.options.timeOut = 3000;

	console.log('Application started...');
	toastr.info('Application started...');

	var pages = {
		claims: null,
		underwriting: null
	};

	function router() {
		console.log(location.hash);

		switch (location.hash) {
			case '#claims': {
				pages.claims = pages.claims || require('./claims.js')();
			} break;
			default: {
				pages.underwriting = pages.underwriting || require('./underwriting.js')();
			}
		}
	}

	$(window).off('hashchange').on('hashchange', function(e) {
		console.log('hashchange', location.hash);

		router();
	});

	router();
})();