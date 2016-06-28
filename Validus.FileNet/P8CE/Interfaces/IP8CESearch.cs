using System.Collections.Generic;

namespace Validus.FileNet
{
	public interface IP8CESearch
	{
		IList<IDictionary<string, object>> ContentSearch(string keywords,
		                                                 IDictionary<string, string> properties = null,
		                                                 IList<DateRange> dateRanges = null,
		                                                 ObjectStore objectStore = ObjectStore.Document,
		                                                 DocumentClass documentClass = DocumentClass.Document,
		                                                 bool adminOverride = false);

		IList<IDictionary<string, object>> RepositorySearch(IDictionary<string, string> properties,
		                                                    IList<DateRange> dateRanges = null,
		                                                    ObjectStore objectStore = ObjectStore.Document,
		                                                    DocumentClass documentClass = DocumentClass.Document,
		                                                    bool adminOverride = false);

		// TODO: IList<IDictionary<string, object>> PrincipalSearch();

		// TODO: IList<IDictionary<string, object>> PropertySearch();
	}
}