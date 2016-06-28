using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Validus.FileNet
{
	public partial class P8ContentEngine : IP8CESearch
	{
		public IList<IDictionary<string, object>> ContentSearch(string keywords,
																IDictionary<string, string> properties = null,
																IList<DateRange> dateRanges = null,
		                                                        ObjectStore objectStore = P8ContentEngine.DefaultObjectStore,
																DocumentClass documentClass = P8ContentEngine.DefaultDocumentClass,
																bool adminOverride = false)
		{
			var whereClause = string.Concat(properties != null
				? Regex.Replace(properties.Aggregate
					(
						string.Empty, (current, pt) => string.Concat
						(
							current, string.Format
							(
								" AND dc1.[{0}] LIKE '%{1}%'",
								pt.Key,
								pt.Value.Replace("'", "''")
							)
						)
					), @"^\s+AND\s+?", string.Empty, RegexOptions.IgnoreCase)
				: string.Empty, BuildDateRangeSQL(dateRanges));

			var contentSearch = new RepositorySearch
			{
				SearchScope = new ObjectStoreScope
				{
					objectStore = objectStore.GetDescription()
				},
				SearchSQL = string.Format(
					@"SELECT TOP 500 cs.*, dc1.*
						FROM {0} dc1 INNER JOIN ContentSearch cs ON dc1.This = cs.QueriedObject
						WHERE CONTAINS(dc1.*, '{1}') {2}
						ORDER BY cs.Rank DESC
						OPTIONS (FULLTEXTROWLIMIT 500)",
					documentClass.GetDescription(),
					keywords.Replace("'", "''"),
					!string.IsNullOrEmpty(whereClause) 
					? string.Format("AND ({0})", whereClause) : string.Empty)
			};

//			var contentSearch = new RepositorySearch
//			{
//				SearchScope = new ObjectStoreScope
//				{
//					objectStore = objectStore.GetDescription()
//				},
//				SearchSQL = string.Format(
//					@"SELECT TOP 500 *
//						FROM ({0} dc1 INNER JOIN ContentSearch cs ON dc1.This = cs.QueriedObject)
//						RIGHT OUTER JOIN {0} dc2 ON dc1.ID = dc2.ID
//						WHERE CONTAINS(dc1.*, '{1}') 
//						AND ({2})
//						OR (dc1.ID = dc2.RelatedDocumentID OR dc1.RelatedDocumentID = dc2.RelatedDocumentID)
//						ORDER BY cs.Rank DESC
//						OPTIONS (FULLTEXTROWLIMIT 500)",
//					documentClass.GetDescription(),
//					keywords.Replace("'", "''"),
//					Regex.Replace(whereClause, @"^\s+AND\s+?", string.Empty, RegexOptions.IgnoreCase))
//			};

			var searchResults = Search(contentSearch, adminOverride);

			var contentResults = new List<IDictionary<string, object>>();

			if (searchResults != null && searchResults.Object != null)
			{
				foreach (var searchResult in searchResults.Object)
				{
					var dmsProperties = new Dictionary<string, object>();

					foreach (var dmsProperty in searchResult.Property
					                                        .Where
															(
																p => !dmsProperties.ContainsKey(p.propertyId)
															))
					{
						dmsProperties.Add(dmsProperty.propertyId, Utilities.GetPropertyValue(dmsProperty));
					}

					contentResults.Add(dmsProperties);
				}
			}

			return contentResults;
		}

		public IList<IDictionary<string, object>> RepositorySearch(IDictionary<string, string> properties,
		                                                           IList<DateRange> dateRanges = null,
		                                                           ObjectStore objectStore = P8ContentEngine.DefaultObjectStore,
		                                                           DocumentClass documentClass = P8ContentEngine.DefaultDocumentClass,
																   bool adminOverride = false)
		{
            var whereClause = string.Concat(properties.Aggregate
                (
                    string.Empty, (current, pt) =>
                    {
                        string strRet = "";
                        if (pt.Key == "WorkflowGUID")
                        {
                            strRet += string.Concat
                            (
                                current, string.Format
                                (
                                    " AND dc1.[{0}] = '{1}'",
                                    pt.Key,
                                    pt.Value.Replace("'", "''")
                                )
                            );
                        }
                        else
                        {
                            strRet += string.Concat
                            (
                                current, string.Format
                                (
                                    " AND dc1.[{0}] LIKE '%{1}%'",
                                    pt.Key,
                                    pt.Value.Replace("'", "''")
                                )
                            );
                        }
                        return strRet;
                    }
                ), BuildDateRangeSQL(dateRanges));

			var repositorySearch = new RepositorySearch
			{
				SearchScope = new ObjectStoreScope
				{
					objectStore = objectStore.GetDescription()
				},
				SearchSQL = string.Format(
					@"SELECT TOP 500 * FROM {0} dc1 WHERE {1}",
					documentClass.GetDescription(),
					Regex.Replace(whereClause, @"^\s+AND\s+?", string.Empty, RegexOptions.IgnoreCase))
			};

//			var repositorySearch = new RepositorySearch
//			{
//				SearchScope = new ObjectStoreScope
//				{
//					objectStore = objectStore.GetDescription()
//				},
//				SearchSQL = string.Format(
//					@"SELECT TOP 500 dc1.*
//						FROM {0} dc1 RIGHT OUTER JOIN {0} dc2 ON dc1.ID = dc2.ID
//						WHERE ({1})
//						OR (dc1.ID = dc2.RelatedDocumentID OR dc1.RelatedDocumentID = dc2.RelatedDocumentID)
//						OPTIONS (FULLTEXTROWLIMIT 500)",
//					documentClass.GetDescription(),
//					Regex.Replace(whereClause, @"^\s+AND\s+?", string.Empty, RegexOptions.IgnoreCase))
//			};

			var searchResult = Search(repositorySearch, adminOverride);

			var repositoryResults = new List<IDictionary<string, object>>();

			if (searchResult != null && searchResult.Object != null)
			{
				repositoryResults.AddRange
				(
					searchResult.Object.Select
					(
						o => o.Property.ToDictionary<PropertyType, string, object>
						(
							p => p.propertyId, Utilities.GetPropertyValue
						)
					)
				);
			}

			return repositoryResults;
		}

        public IList<IDictionary<string, object>> RepositorySearchForPolId(IDictionary<string, string> properties,
		                                                           IList<DateRange> dateRanges = null,
		                                                           ObjectStore objectStore = P8ContentEngine.DefaultObjectStore,
		                                                           DocumentClass documentClass = P8ContentEngine.DefaultDocumentClass,
																   bool adminOverride = false)
		{
			var whereClause = string.Concat(properties.Aggregate
				(
					string.Empty, (current, pt) => string.Concat
					(
						current, string.Format
						(
							" AND '{1}' IN dc1.[{0}] ",
							pt.Key,
							pt.Value.Replace("'", "''")
						)
					)
				), BuildDateRangeSQL(dateRanges));

			var repositorySearch = new RepositorySearch
			{
				SearchScope = new ObjectStoreScope
				{
					objectStore = objectStore.GetDescription()
				},
				SearchSQL = string.Format(
					@"SELECT TOP 500 * FROM {0} dc1 WHERE {1}",
					documentClass.GetDescription(),
					Regex.Replace(whereClause, @"^\s+AND\s+?", string.Empty, RegexOptions.IgnoreCase))
			};

			var searchResult = Search(repositorySearch, adminOverride);

			var repositoryResults = new List<IDictionary<string, object>>();

			if (searchResult != null && searchResult.Object != null)
			{
				repositoryResults.AddRange
				(
					searchResult.Object.Select
					(
						o => o.Property.ToDictionary<PropertyType, string, object>
						(
							p => p.propertyId, Utilities.GetPropertyValue
						)
					)
				);
			}

			return repositoryResults;
		}

		protected string BuildDateRangeSQL(IList<DateRange> dateRanges)
		{
			var dateRangeSQL = string.Empty;

			if (dateRanges != null)
			{
				foreach (var dateRange in dateRanges)
				{
					if (dateRange.From.HasValue && dateRange.To.HasValue)
					{
						dateRangeSQL = string.Concat
						(
							dateRangeSQL, string.Format
							(
								" AND (dc1.[{0}] >= {1} AND dc1.[{0}] <= {2})",
								dateRange.Name,
								dateRange.From.Value.ToString(DateRange.FromFormat),
								dateRange.To.Value.ToString(DateRange.ToFormat)
							)
						);
					}
					else if (dateRange.From.HasValue)
					{
						dateRangeSQL = string.Concat
						(
							dateRangeSQL, string.Format
							(
								" AND dc1.[{0}] >= {1}",
								dateRange.Name,
								dateRange.From.Value.ToString(DateRange.FromFormat)
							)
						);
					}
					else if (dateRange.To.HasValue)
					{
						dateRangeSQL = string.Concat
						(
							dateRangeSQL, string.Format
							(
								" AND dc1.[{0}] <= {1}",
								dateRange.Name,
								dateRange.To.Value.ToString(DateRange.ToFormat)
							)
						);
					}
				}
			}

			return dateRangeSQL;
		}
	}
}