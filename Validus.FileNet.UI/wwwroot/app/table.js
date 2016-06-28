module.exports = (function table($table, options) {
	'use strict';

	if (!$table instanceof jQuery) {
		console.log('Element must be a valid jQuery instance');
		return;
	}

	var current = {
		data: [],
		settings: {}
	};

	var defaults = {
		dom: '<"table-responsive" t>ip',
		displayLength: 20
	};

	function settings(data, o) {
		o = o || current.settings;

		return current.settings = {
			data: data || current.data,
			dom: o.dom || defaults.dom,
			displayLength: o.displayLength || defaults.displayLength,
			columns: o.columns,
			columnDefs: o.columnDefinitions,
			initComplete: o.initialisedCallback || function() { },
			drawCallback: o.drawCallback || function() { }
		};
	}

	function initialise(data) {
		return $table.DataTable(settings(data, options));
	}

	function load(data) {
		if ($.fn.DataTable.isDataTable($table)) {
			var api = $.fn.DataTable.Api($table);

			api.clear();
			api.rows.add(data);
			api.draw();
		}
		else initialise(data);
	}

	return {
		load: load
	};
});