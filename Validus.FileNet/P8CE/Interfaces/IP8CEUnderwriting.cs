using System;
using System.Collections.Generic;

namespace Validus.FileNet
{
	public interface IP8CEUnderwriting
	{
		IList<Guid> UploadUnderwritingDocuments(IList<Underwriting2014Document> documents);

		bool UpdateUnderwritingDocuments(IList<Underwriting2014Document> documents);

		IList<Underwriting2014Document> SearchUnderwriting(IDictionary<string, string> properties,
													       DateRange inceptionDateRange = null,
													       DateRange writtenDateRange = null,
													       DateRange createdDateRange = null,
		                                                   bool adminOverride = false,
                                                           uint limit = 0);

		IList<Underwriting2014Document> SearchUnderwritingByPolicyId(string id,
		                                                             bool adminOverride = false);
	}
}