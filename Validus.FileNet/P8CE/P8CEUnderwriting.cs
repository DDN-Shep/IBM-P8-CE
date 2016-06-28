using System;
using System.Collections.Generic;
using System.Linq;

namespace Validus.FileNet
{
	public class P8CEUnderwriting : P8ContentEngine, IP8CEUnderwriting
	{
		protected new const ObjectStore DefaultObjectStore = ObjectStore.Underwriting;
		protected new const DocumentClass DefaultDocumentClass = DocumentClass.Underwriting;

		public P8CEUnderwriting(string serviceURL = null,
		                        string kerberosSPN = null,
		                        long kerberosTTL = DefaultTTL)
			: base(serviceURL, kerberosSPN, kerberosTTL)
		{ }

        public IList<Guid> UploadUnderwritingDocuments(IList<Underwriting2014Document> documents)
		{
			var documentIDs = new List<Guid>();

			foreach (var document in documents)
			{
				var documentProperties = new Dictionary<string, object>();

				document.UnMapFromFileNet(documentProperties);

				var documentID = CreateDocument(document.Name,
				                                     document.Content,
				                                     documentProperties,
				                                     DefaultObjectStore,
				                                     DocumentClass.Underwriting);

				if (!documentID.HasValue)
					continue;

				documentIDs.Add(documentID.Value);
			}

			return documentIDs;
		}

		public bool UpdateUnderwritingDocuments(IList<Underwriting2014Document> documents)
		{
			var isSuccess = true;

			foreach (var document in documents)
			{
				if (!document.ID.HasValue)
					continue;

				var documentProperties = new Dictionary<string, object>();

				document.UnMapFromFileNet(documentProperties);

				isSuccess &= UpdateDocument(document.ID.Value,
				                                 document.Name,
				                                 document.Content,
				                                 documentProperties,
				                                 DefaultObjectStore,
				                                 DocumentClass.Underwriting);
			}

			return isSuccess;
		}

		public IList<Underwriting2014Document> SearchUnderwriting(IDictionary<string, string> properties,
		                                                      DateRange inceptionDateRange = null,
		                                                      DateRange writtenDateRange = null,
		                                                      DateRange createdDateRange = null,
		                                                      bool adminOverride = false,
                                                              uint limit = 500)
		{
			var dateRanges = new List<DateRange>();

			if (createdDateRange != default(DateRange))
			{
				createdDateRange.Name = "DateCreated";

				dateRanges.Add(createdDateRange);
			}

			if (inceptionDateRange != default(DateRange))
			{
				inceptionDateRange.Name = "uwInceptionDate";

				dateRanges.Add(inceptionDateRange);
			}

			if (writtenDateRange != default(DateRange))
			{
				writtenDateRange.Name = "uwWrittenDate";

				dateRanges.Add(writtenDateRange);
			}
			
			var searchProperties1 = (from dp in typeof(UnderwritingDocument).GetProperties()
									 from a in dp.GetCustomAttributes(typeof(FileNetFieldAttribute), true)
												 .OfType<FileNetFieldAttribute>()
									 join p in properties
									 on a.ID equals p.Key
									 select p).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			var searchProperties2 = (from dp in typeof(Underwriting2014Document).GetProperties()
									 from a in dp.GetCustomAttributes(typeof(FileNetFieldAttribute), true)
												 .OfType<FileNetFieldAttribute>()
									 join p in properties
									 on a.ID equals p.Key
									 select p).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			var searchResults1 = searchProperties1.Any() || dateRanges.Any()
									? RepositorySearch(searchProperties1,
															dateRanges,
															DefaultObjectStore,
															DocumentClass.Underwriting,
															adminOverride) : null;

			var searchResults2 = searchProperties2.Any() || dateRanges.Any()
									? RepositorySearch(searchProperties2,
															dateRanges,
															DefaultObjectStore,
															DocumentClass.Underwriting2014,
															adminOverride) : null;

			var underwritingDocuments = new List<Underwriting2014Document>();

			foreach (var searchResults in new[] { searchResults1, searchResults2 })
			{
				if (searchResults == null) continue;

				foreach (var searchResult in searchResults)
				{
					var underwritingDocument = new Underwriting2014Document();

					searchResult.MapToFileNet(underwritingDocument, true, true);

					underwritingDocuments.Add(underwritingDocument);
				}
			}

			return underwritingDocuments;
		}

		public IList<Underwriting2014Document> SearchUnderwritingByPolicyId(string id,
		                                                                bool adminOverride = false)
		{
			//return SearchUnderwriting(new Dictionary<string, string> { { "uwPolicyID", id } });
            var properties = new Dictionary<string, string> { { "uwPolicyID", id } } ;
            DateRange inceptionDateRange = null;
            DateRange writtenDateRange = null;
            DateRange createdDateRange = null;

            var dateRanges = new List<DateRange>();

			if (createdDateRange != default(DateRange))
			{
				createdDateRange.Name = "DateCreated";

				dateRanges.Add(createdDateRange);
			}

			if (inceptionDateRange != default(DateRange))
			{
				inceptionDateRange.Name = "uwInceptionDate";

				dateRanges.Add(inceptionDateRange);
			}

			if (writtenDateRange != default(DateRange))
			{
				writtenDateRange.Name = "uwWrittenDate";

				dateRanges.Add(writtenDateRange);
			}
			
			var searchProperties1 = (from dp in typeof(UnderwritingDocument).GetProperties()
									 from a in dp.GetCustomAttributes(typeof(FileNetFieldAttribute), true)
												 .OfType<FileNetFieldAttribute>()
									 join p in properties
									 on a.ID equals p.Key
									 select p).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			var searchProperties2 = (from dp in typeof(Underwriting2014Document).GetProperties()
									 from a in dp.GetCustomAttributes(typeof(FileNetFieldAttribute), true)
												 .OfType<FileNetFieldAttribute>()
									 join p in properties
									 on a.ID equals p.Key
									 select p).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			var searchResults1 = searchProperties1.Any() || dateRanges.Any()
									? RepositorySearchForPolId(searchProperties1,
															dateRanges,
															DefaultObjectStore,
															DocumentClass.Underwriting,
															adminOverride) : null;

			var searchResults2 = searchProperties2.Any() || dateRanges.Any()
									? RepositorySearchForPolId(searchProperties2,
															dateRanges,
															DefaultObjectStore,
															DocumentClass.Underwriting,
															adminOverride) : null;

			var underwritingDocuments = new List<Underwriting2014Document>();

			foreach (var searchResults in new[] { searchResults1, searchResults2 })
			{
				if (searchResults == null) continue;

				foreach (var searchResult in searchResults)
				{
					var underwritingDocument = new Underwriting2014Document();

					searchResult.MapToFileNet(underwritingDocument, true, true);

					underwritingDocuments.Add(underwritingDocument);
				}
			}

			return underwritingDocuments;
		}
	}
}