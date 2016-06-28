using System;
using System.Collections.Generic;

namespace Validus.FileNet
{
	public interface IP8CEDocument
	{
		IDictionary<string, object> ResolveTitleAndMimeType(string name,
															IDictionary<string, object> properties = null);

		Guid? CreateDocument(string name, byte[] content,
							 IDictionary<string, object> properties = null,
							 ObjectStore objectStore = ObjectStore.Document,
							 DocumentClass documentClass = DocumentClass.Document);

		/* TODO
		Guid? CopyDocument(Guid id, string name,
						   ObjectStore objectStore = ObjectStore.Document,
						   DocumentClass documentClass = DocumentClass.Document);*/

		bool UpdateDocument(Guid id, string name, byte[] content,
							IDictionary<string, object> properties = null,
							ObjectStore objectStore = ObjectStore.Document,
							DocumentClass documentClass = DocumentClass.Document);

		bool UpdateDocumentContent(Guid id, string name, byte[] content,
								   ObjectStore objectStore = ObjectStore.Document,
								   DocumentClass documentClass = DocumentClass.Document);

		bool UpdateDocumentProperties(Guid id,
									  IDictionary<string, object> properties,
									  ObjectStore objectStore = ObjectStore.Document,
									  DocumentClass documentClass = DocumentClass.Document);

		IEnumerable<SingleObjectResponse> GetDocumentObject(Guid id,
															IList<string> excludeProperties = null,
															IList<string> includeProperties = null,
															ObjectStore objectStore = ObjectStore.Document,
															DocumentClass documentClass = DocumentClass.Document);

		IList<byte[]> GetDocumentContent(Guid id,
										 IEnumerable<SingleObjectResponse> documentObject = null,
										 ObjectStore objectStore = ObjectStore.Document,
										 DocumentClass documentClass = DocumentClass.Document);

		IDictionary<string, object> GetDocumentProperties(Guid id,
														  IEnumerable<SingleObjectResponse> documentObject = null,
														  ObjectStore objectStore = ObjectStore.Document,
														  DocumentClass documentClass = DocumentClass.Document);

		Guid? CheckoutDocument(Guid id,
							   ObjectStore objectStore = ObjectStore.Document,
							   DocumentClass documentClass = DocumentClass.Document);

		bool CheckinDocument(Guid id,
							 ObjectStore objectStore = ObjectStore.Document,
							 DocumentClass documentClass = DocumentClass.Document);

		bool DeleteDocument(Guid id, bool allVersions,
							ObjectStore objectStore = ObjectStore.Document,
							DocumentClass documentClass = DocumentClass.Document);
	}
}