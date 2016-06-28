module.exports = (function underwriting() {
	'use strict';

	function DrawWebPolicyLink() {
		if (!urls.webpolicy) return;

		var options = [
			  'height=650',
			  'width=1100',
			  'directories=0',
			  'location=0',
			  'menubar=0',
			  'scrollbars=1',
			  'status=0',
			  'resizable=0',
			  'titlebar=0',
			  'toolbar=0'
		].join(',');

		$('.webpolicy', this).off('click').on('click', function(e) {
			e.preventDefault();

			window.open(urls.webpolicy + this.innerText, '_blank', options);
		});
	}

	function InitialiseLogSave() {
		var t = this;

		if (!t._) return;

		$save.off('click').on('click', function(e) {
			e.preventDefault();

			for (var i = 0, rows = [], data = t._('tr', {
				filter: 'applied'
			}) ; i < data.length; i++) {
				rows.push(data[i]);
			}

			console.save(rows, 'Underwriting Search Results' + Date.now().toLocaleString() + '.json', 0);
		}).removeClass('hide');
	}

	var urls = {
		search: 'http://localhost:1264/api/underwriting/search',
		upload: 'http://localhost:1264/api/underwriting/upload',
		webpolicy: 'http://webpolicy.globaldev.local?PolicyId=',
	};

	var options = (function() {
		var columns = function() {
			return [{
				data: 'ID',
				visible: false
			}, {
				data: 'Title',
				title: 'Document Title'
			}, {
				data: 'Description',
				title: 'Description'
			}, {
				data: 'CreatedOn',
				title: 'Date To DMS'
			}, {
				data: 'DocumentType',
				title: 'Document Type'
			}, {
				data: 'PolicyIDs',
				title: 'Policy ID'
			}, {
				data: 'InsuredName',
				title: 'Insured Name'
			}, {
				data: 'Underwriter',
				title: 'UWR'
			}, {
				data: 'COB',
				title: 'COB'
			}, {
				data: 'InceptionDate',
				title: 'Inception Date'
			}, {
				data: 'Status',
				title: 'Status'
			}, {
				data: 'EntryStatus',
				title: 'Entry Status'
			}]
		}();

		var columnDefinitions = function() {
			function findOne(name) {
				return columns.findIndex(function(i) {
					return i.data === name;
				});
			}

			function find(names) {
				var indexes = [];

				names.forEach(function(name) {
					var i = findOne(name);

					if (i >= 0) indexes.push(i);
				});

				return indexes;
			}

			return [{
				render: function(data, type, row) {
					var policies = data.split(';'),
						text = [];

					for (var i = 0, p; i < policies.length && (p = policies[i]) ; i++) {
						text.push([
						  '<a class="webpolicy" href="#">', p, '</a>'
						].join(''));
					}

					return text.join(' ');
				},
				targets: findOne('PolicyIDs')
			}, {
				render: function(data, type, row) {
					return moment(data).format('DD MMM YYYY');
				},
				targets: find(['CreatedOn', 'InceptionDate'])
			}, {
				render: function(data, type, row) {
					return [
					  '<div class="cell-tooltip" data-toggle="tooltip" data-placement="top" title="', data, '">',
					  data,
					  '</a>'
					].join('');
				},
				targets: find(['Title', 'Description'])
			}];
		}();

		return {
			columns: columns,
			columnDefinitions: columnDefinitions,
			initialisedCallback: function initialisedCallback() {
				InitialiseLogSave.call(this);
			},
			drawCallback: function drawCallback() {
				DrawWebPolicyLink.call(this);

				$('[data-toggle="tooltip"]', $results).tooltip();
			}
		};
	})();

	var $panel = $('#underwriting-search'),
		$form = $('.search-form', $panel),
		$save = $('.save-search-results', $panel),
		$search = $('.btn[type="submit"]', $panel),
		$results = $('.search-results', $panel),
		table = require('./table.js')($results, options);

	$form.off('submit').on('submit', function(e) {
		e.preventDefault();

		var o = {
			url: urls.search,
			type: this.method || 'get',
			crossDomain: true,
			xhrFields: {
				withCredentials: true
			},
			success: function(json) {
				toastr.success('Search succeeded');
				table.load(json);
			},
			error: function() {
				toastr.error('Search failed');
			},
			complete: function() {
				$search.removeClass('loading');
			}
		};

		if (o.type !== 'get') {
			o.data = new FormData(this);

			o.processData = o.contentType = false;
		}
		else o.data = $(this).serialize();

		$search.addClass('loading');
		toastr.info('Searching...');

		$.ajax(o);
	});
});