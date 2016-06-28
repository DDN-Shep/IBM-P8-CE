using System;
using System.Collections.Generic;

namespace Validus.FileNet
{
	public interface IP8CEPermissions
	{
/*
		void GetObjectStorePermissions(string ???,
									   ObjectStore objectStore = ObjectStore.Document);

		void GetFolderPermissions(string ???,
								  ObjectStore objectStore = ObjectStore.Document,
								  DocumentClass documentClass = DocumentClass.Document);

		void GetDocumentPermissions(string ???,
									ObjectStore objectStore = ObjectStore.Document,
									DocumentClass documentClass = DocumentClass.Document);

		void SetObjectStorePermissions(string ???,
									   ObjectStore objectStore = ObjectStore.Document);

		void SetFolderPermissions(string ???,
								  ObjectStore objectStore = ObjectStore.Document,
								  DocumentClass documentClass = DocumentClass.Document);

		void SetDocumentPermissions(string ???,
									ObjectStore objectStore = ObjectStore.Document,
									DocumentClass documentClass = DocumentClass.Document);*/

		bool AllowDocumentAccess(Guid id,
		                         IList<string> allowUsers,
		                         ObjectStore objectStore = ObjectStore.Document,
		                         DocumentClass documentClass = DocumentClass.Document);

		bool DenyDocumentAccess(Guid id,
		                        IList<string> denyUsers,
		                        ObjectStore objectStore = ObjectStore.Document,
		                        DocumentClass documentClass = DocumentClass.Document);

		bool RemoveDocumentAccess(Guid id,
		                          IList<string> removeUsers,
		                          ObjectStore objectStore = ObjectStore.Document,
		                          DocumentClass documentClass = DocumentClass.Document);

		bool ResetDocumentAccess(Guid id,
		                         ObjectStore objectStore = ObjectStore.Document,
		                         DocumentClass documentClass = DocumentClass.Document);

		IList<IDictionary<string, object>> RetrieveDocumentAccess(Guid id,
		                                                          ObjectStore objectStore = ObjectStore.Document,
		                                                          DocumentClass documentClass = DocumentClass.Document);

		bool TestDocumentAccess(Guid id,
		                        ObjectStore objectStore = ObjectStore.Document,
		                        DocumentClass documentClass = DocumentClass.Document);
	}
}