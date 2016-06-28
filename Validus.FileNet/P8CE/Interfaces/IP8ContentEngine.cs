using System;
using System.Collections.Generic;

namespace Validus.FileNet
{
	public interface IP8ContentEngine : IDisposable
	{
		IDictionary<string, object> GetObject(ObjectRequestType[] request, string property);

		IDictionary<string, object> GetObject(ObjectRequestType[] request, IList<string> properties);

		IList<IDictionary<string, object>> GetObjects(ObjectRequestType[] request, string property);

		IList<IDictionary<string, object>> GetObjects(ObjectRequestType[] request, IList<string> properties);

		IList<IDictionary<string, object>> Search(RepositorySearch request, string property);

		IList<IDictionary<string, object>> Search(RepositorySearch request, IList<string> properties);

		IList<IDictionary<string, object>> Execute(ExecuteChangesRequest request, string property);

		IList<IDictionary<string, object>> Execute(ExecuteChangesRequest request, IList<string> properties);

		ContentResponseType[] GetContents(ContentRequestType[] request, bool adminOverride = false);

		ObjectResponseType[] GetObjects(ObjectRequestType[] request, bool adminOverride = false);

		ObjectSetType Search(RepositorySearch request, bool adminOverride = false);

		ChangeResponseType[] Execute(ExecuteChangesRequest request, bool adminOverride = false);
	}
}