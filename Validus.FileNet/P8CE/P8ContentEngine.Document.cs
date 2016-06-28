using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Validus.FileNet
{
    public partial class P8ContentEngine : IP8CEDocument
    {
        public const string DOCUMENT_TITLE = "DocumentTitle";
        public const string MIME_TYPE = "MimeType";

        public IDictionary<string, object> ResolveTitleAndMimeType(string name,
                                                                   IDictionary<string, object> properties = null)
        {
            string title = null;
            string mimeType = null;

            if (properties == null)
            {
                properties = new Dictionary<string, object>();
            }
            
            if (!properties.ContainsKey(DOCUMENT_TITLE))
            {
                properties.Add(DOCUMENT_TITLE, name);
            }

            if (!properties.ContainsKey(MIME_TYPE))
            {
                properties.Add(MIME_TYPE, null);
            }

            title = properties[DOCUMENT_TITLE] as string;
            if (string.IsNullOrEmpty(title))
            {
                properties[DOCUMENT_TITLE] = name;
            }

            mimeType = properties[MIME_TYPE] as string;
            if (string.IsNullOrEmpty(mimeType))
            {
                mimeType = MimeTypeUtility.GetMimeType(name);
                properties[MIME_TYPE] = mimeType;
            }
            else
            {
                if (Path.HasExtension(mimeType))
                {
                    mimeType = Path.GetExtension(mimeType);
                }

                mimeType = mimeType.Trim(MimeTypeUtility.Excess);
                var mimeTypes = MimeTypeUtility.Collection.Where(kvp => kvp.Key.Equals(mimeType, StringComparison.OrdinalIgnoreCase) || kvp.Value.Equals(mimeType, StringComparison.OrdinalIgnoreCase)).ToList();
                if (mimeTypes.Count > 0)
                {
                    properties[MIME_TYPE] = mimeTypes[0].Value;
                }
                else
                {
                    properties[MIME_TYPE] = MimeTypeUtility.DefaultType;
                }
            }
            
            return properties;
        }

        public Guid? CreateDocument(string name, byte[] content,
                                    IDictionary<string, object> properties = null,
                                    ObjectStore objectStore = P8ContentEngine.DefaultObjectStore,
                                    DocumentClass documentClass = P8ContentEngine.DefaultDocumentClass)
        {
            if (name == string.Empty)
                throw new ArgumentException(nameof(name));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            properties = ResolveTitleAndMimeType(name, properties);

            var mimeType = properties.ContainsKey(MIME_TYPE) ? properties[MIME_TYPE] as string : MimeTypeUtility.DefaultType;

            var actionProperties = new List<ModifiablePropertyType>
			{
				new ListOfObject
				{
					propertyId = "ContentElements",
					Value = new[]
					{
						new DependentObjectType
						{
							classId = "ContentTransfer",
							dependentAction = DependentObjectTypeDependentAction.Insert,
							dependentActionSpecified = true,
							Property = new PropertyType[]
							{
								new SingletonString
								{
									propertyId = "ContentType",
									Value = mimeType
								},
								new SingletonString
								{
									propertyId = "RetrievalName",
									Value = name
								},
								new ContentData
								{
									propertyId = "Content",
									Value = new InlineContent
									{
										Binary = content
									}
								}
							}
						}
					}
				}
			};

            if (properties != null)
            {
                actionProperties.AddRange(Utilities.GetPropertyCollection(properties));
            }

            var executeRequest = new ExecuteChangesRequest
            {
                ChangeRequest = new[]
				{
					new ChangeRequestType
					{
						id = "1",
						Action = new ActionType[]
						{
							new CreateAction
							{
								classId = documentClass.GetDescription()
							},
							new CheckinAction
							{
								autoClassify = true,
								autoClassifySpecified = true,
								checkinMinorVersion = true,
								checkinMinorVersionSpecified = true
							},
							new PromoteVersionAction()
						},
						ActionProperties = actionProperties.ToArray(),
						TargetSpecification = new ObjectReference
						{
							classId = "ObjectStore",
							objectStore = objectStore.GetDescription()
						}
					}
				},
                refresh = true,
                refreshSpecified = true
            };

            var createResults = Execute(executeRequest, "ID");

            var id = (from cr in createResults.FirstOrDefault()
                      where cr.Key.Equals("ID", StringComparison.CurrentCultureIgnoreCase)
                      select cr.Value).FirstOrDefault();

            if (id != null)
                return new Guid(id.ToString());

            return null;
        }

        public bool UpdateDocument(Guid id, string name, byte[] content,
                                   IDictionary<string, object> properties = null,
                                   ObjectStore objectStore = DefaultObjectStore,
                                   DocumentClass documentClass = DefaultDocumentClass)
        {
            var updateContent = name != null && content != null;
            var updateProperties = properties != null && properties.Count > 0;

            if (!updateContent && !updateProperties)
            {
                throw new ArgumentException("Insufficient arguments provided (name, content and/or properties)");
            }

            properties = ResolveTitleAndMimeType(name, properties);

            var mimeType = properties.ContainsKey(MIME_TYPE) ? properties[MIME_TYPE] as string : MimeTypeUtility.DefaultType;

            var reservationID = updateContent
                                    ? CheckoutDocument(id, objectStore, documentClass)
                                    : Guid.Empty;

            if (reservationID != null && reservationID != Guid.Empty)
            {
                id = reservationID.Value;
            }

            var actionProperties = new List<ModifiablePropertyType>();

            if (updateContent)
            {
                actionProperties.Add(new ListOfObject
                {
                    propertyId = "ContentElements",
                    Value = new[]
					{
						new DependentObjectType
						{
							classId = "ContentTransfer",
							dependentAction = DependentObjectTypeDependentAction.Insert,
							dependentActionSpecified = true,
							Property = new PropertyType[]
							{
								new SingletonString
								{
									propertyId = "ContentType",
									Value = mimeType
								},
								new ContentData
								{
									propertyId = "Content",
									Value = new InlineContent
									{
										Binary = content
									}
								}
							}
						}
					}
                });
            }

            if (updateProperties)
            {
                actionProperties.AddRange(Utilities.GetPropertyCollection(properties));
            }

            var updateRequest = new ExecuteChangesRequest
            {
                ChangeRequest = new[]
				{
					new ChangeRequestType
					{
						id = "1",
						Action = new ActionType[] { new UpdateAction() },
						ActionProperties = actionProperties.ToArray(),
						TargetSpecification = new ObjectSpecification
						{
							objectStore = objectStore.GetDescription(),
							classId = documentClass.GetDescription(),
							objectId = id.ToString(DefaultIDFormat)
						}
					}
				},
                refresh = true,
                refreshSpecified = true
            };

            Execute(updateRequest);

            return (!updateContent) || CheckinDocument(id, objectStore, documentClass);
        }

        public bool UpdateDocumentContent(Guid id, string name, byte[] content,
                                          ObjectStore objectStore = P8ContentEngine.DefaultObjectStore,
                                          DocumentClass documentClass = P8ContentEngine.DefaultDocumentClass)
        {
            if (name == string.Empty)
                throw new ArgumentException(nameof(name));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            return UpdateDocument(id, name, content, null, objectStore, documentClass);
        }

        public bool UpdateDocumentProperties(Guid id,
                                             IDictionary<string, object> properties,
                                             ObjectStore objectStore = DefaultObjectStore,
                                             DocumentClass documentClass = DefaultDocumentClass)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (properties.Count == 0)
                throw new ArgumentException(nameof(properties));

            return UpdateDocument(id, null, null, properties, objectStore, documentClass);
        }

        public IDictionary<string, object> GetDocument(Guid id,
                                                       ObjectStore objectStore = DefaultObjectStore,
                                                       DocumentClass documentClass = DefaultDocumentClass)
        {
            var documentObject = GetDocumentObject(id, null, null, objectStore, documentClass);
            var document = default(IDictionary<string, object>);

            if (documentObject != null)
            {
                document = GetDocumentProperties(id, documentObject, objectStore, documentClass);

                document["ContentElements"] = GetDocumentContent(id, documentObject, objectStore, documentClass);
            }

            return document;
        }

        public IEnumerable<SingleObjectResponse> GetDocumentObject(Guid id,
                                                                   IList<string> excludeProperties = null,
                                                                   IList<string> includeProperties = null,
                                                                   ObjectStore objectStore = DefaultObjectStore,
                                                                   DocumentClass documentClass = DefaultDocumentClass)
        {
            var documentInfoRequest = new[]
			{
				new ObjectRequestType
				{
					SourceSpecification = new ObjectSpecification
					{
						objectStore = objectStore.GetDescription(),
						classId = documentClass.GetDescription(),
						objectId = id.ToString(DefaultIDFormat)
					},
					PropertyFilter = new PropertyFilterType
					{
						maxRecursion = 2,
						maxRecursionSpecified = true
					}
				}
			};

            if (excludeProperties != null && excludeProperties.Any())
            {
                documentInfoRequest.First().PropertyFilter.ExcludeProperties = excludeProperties.ToArray();
            }

            if (includeProperties != null && includeProperties.Any())
            {
                documentInfoRequest.First().PropertyFilter.IncludeProperties = (from ip in includeProperties
                                                                                select new FilterElementType
                                                                                {
                                                                                    Value = ip
                                                                                }).ToArray();
            }

            return GetObjects(documentInfoRequest).OfType<SingleObjectResponse>().ToList();
        }

        public IList<byte[]> GetDocumentContent(Guid id,
                                                IEnumerable<SingleObjectResponse> documentObject = null,
                                                ObjectStore objectStore = DefaultObjectStore,
                                                DocumentClass documentClass = DefaultDocumentClass)
        {
            var inlineContents = new List<byte[]>();

            var documentInfoResponse = documentObject ?? GetDocumentObject(id, null, null, objectStore, documentClass);

            var contentElements = documentInfoResponse.SelectMany
                (
                    o => o.Object.Property.OfType<ListOfObject>().Where
                    (
                        p => p.propertyId.Equals("ContentElements", StringComparison.CurrentCultureIgnoreCase)
                    )
                ).ToList();

            foreach (var contentElement in contentElements.Where(ce => ce.Value != null))
            {
                for (var contentIter = 0; contentIter < contentElement.Value.Length; contentIter++)
                {
                    var contentRequest = new[]
					{
						new ContentRequestType
						{
							id = "1",
							cacheAllowed = true,
							cacheAllowedSpecified = true,
							maxBytes = 1024 * 1024,
							maxBytesSpecified = false,
							startOffset = 0,
							startOffsetSpecified = true,
							continueFrom = null,
							ElementSpecification = new ElementSpecificationType
							{
								itemIndex = contentIter,
								itemIndexSpecified = true,
								elementSequenceNumber = 0,
								elementSequenceNumberSpecified = false
							},
							SourceSpecification = new ObjectSpecification
							{
								objectStore = objectStore.GetDescription(),
								classId = documentClass.GetDescription(),
								objectId = id.ToString(DefaultIDFormat)
							}
						}
					};

                    var contentResponse = GetContents(contentRequest);

                    inlineContents.AddRange
                    (
                        contentResponse.OfType<ContentElementResponse>().Select
                        (
                            o => ((InlineContent)o.Content).Binary
                        ).ToList()
                    );
                }
            }

            return inlineContents;
        }

        public IDictionary<string, object> GetDocumentProperties(Guid id,
                                                     IEnumerable<SingleObjectResponse> documentObject = null,
                                                     ObjectStore objectStore = DefaultObjectStore,
                                                     DocumentClass documentClass = DefaultDocumentClass) => (documentObject ?? GetDocumentObject(id, null, null, objectStore, documentClass))
            .SelectMany(o => o.Object.Property)
            .ToDictionary<PropertyType, string, object>
            (
                p => p.propertyId, Utilities.GetPropertyValue
            );

        public Guid? CheckoutDocument(Guid id,
                                      ObjectStore objectStore = DefaultObjectStore,
                                      DocumentClass documentClass = DefaultDocumentClass)
        {
            var checkoutInfoRequest = new[]
			{
				new ObjectRequestType
				{
					SourceSpecification = new ObjectSpecification
					{
						objectStore = objectStore.GetDescription(),
						classId = documentClass.GetDescription(),
						objectId = id.ToString(DefaultIDFormat)
					},
					PropertyFilter = new PropertyFilterType
					{
						maxRecursion = 1,
						maxRecursionSpecified = true,
						IncludeProperties = new[]
						{
							new FilterElementType { Value = "CurrentVersion" },
							new FilterElementType { Value = "Reservation" }
						}
					}
				}
			};

            var checkoutInfoResponse = GetObjects(checkoutInfoRequest);

            var reservation = (from responseProperty in (((SingleObjectResponse)checkoutInfoResponse[0]).Object).Property
                               where responseProperty.propertyId == "Reservation"
                               select ((SingletonObject)responseProperty).Value as ObjectValue).FirstOrDefault();

            if (reservation != null)
            {
                return new Guid(reservation.objectId);
            }

            var currentVersion = (from responseProperty in (((SingleObjectResponse)checkoutInfoResponse[0]).Object).Property
                                  where responseProperty.propertyId == "CurrentVersion"
                                  select ((SingletonObject)responseProperty).Value as ObjectValue).FirstOrDefault();

            if (currentVersion != null)
            {
                id = new Guid(currentVersion.objectId);
            }

            var executeRequest = new ExecuteChangesRequest
            {
                ChangeRequest = new[]
				{
					new ChangeRequestType
					{
						id = "1",
						Action = new ActionType[]
						{
							new CheckoutAction
							{
								reservationType = ReservationType.Exclusive,
								reservationTypeSpecified = true
							}
						},
						TargetSpecification = new ObjectReference
						{
							objectStore = objectStore.GetDescription(),
							classId = documentClass.GetDescription(),
							objectId = id.ToString(DefaultIDFormat)
						},
						RefreshFilter = new PropertyFilterType
						{
							maxRecursion = 1,
							maxRecursionSpecified = true,
							IncludeProperties = new[]
							{
								new FilterElementType { Value = "Reservation" }
							}
						}
					}
				},
                refresh = true,
                refreshSpecified = true
            };

            var checkoutResponse = Execute(executeRequest);

            reservation = (from responseProperty in checkoutResponse[0].Property
                           where responseProperty.propertyId == "Reservation"
                           select ((SingletonObject)responseProperty).Value as ObjectValue).FirstOrDefault();

            if (reservation != null)
                return new Guid(reservation.objectId);

            return null;
        }

        public bool CheckinDocument(Guid id,
                                    ObjectStore objectStore = DefaultObjectStore,
                                    DocumentClass documentClass = DefaultDocumentClass)
        {
            var checkinInfoRequest = new[]
			{
				new ObjectRequestType
				{
					SourceSpecification = new ObjectSpecification
					{
						objectStore = objectStore.GetDescription(),
						classId = documentClass.GetDescription(),
						objectId = id.ToString(DefaultIDFormat)
					},
					PropertyFilter = new PropertyFilterType
					{
						maxRecursion = 1,
						maxRecursionSpecified = true,
						IncludeProperties = new[]
						{
							new FilterElementType { Value = "Reservation" }
						}
					}
				}
			};

            var checkinInfoResponse = GetObjects(checkinInfoRequest);

            var reservation = (from responseProperty in (((SingleObjectResponse)checkinInfoResponse[0]).Object).Property
                               where responseProperty.propertyId == "Reservation"
                               select ((SingletonObject)responseProperty).Value as ObjectValue).FirstOrDefault();

            if (reservation != null)
            {
                id = new Guid(reservation.objectId);
            }

            var checkinRequest = new ExecuteChangesRequest
            {
                ChangeRequest = new[]
				{
					new ChangeRequestType
					{
						id = "1",
						Action = new ActionType[]
						{
							new CheckinAction
							{
								autoClassify = true,
								autoClassifySpecified = true,
								checkinMinorVersion = true,
								checkinMinorVersionSpecified = true
							},
							new PromoteVersionAction()
						},
						TargetSpecification = new ObjectReference
						{
							objectStore = objectStore.GetDescription(),
							classId = documentClass.GetDescription(),
							objectId = id.ToString(DefaultIDFormat)
						},
						RefreshFilter = new PropertyFilterType
						{
							maxRecursion = 1,
							maxRecursionSpecified = true,
							IncludeProperties = new[]
							{
								new FilterElementType { Value = "VersionStatus" }
							}
						}
					}
				},
                refresh = true,
                refreshSpecified = true
            };

            var checkinResponse = Execute(checkinRequest);

            var versionStatus = (from responseProperty in checkinResponse[0].Property
                                 where responseProperty.propertyId == "VersionStatus"
                                 select responseProperty as SingletonInteger32).FirstOrDefault();

            return versionStatus != null &&
                   versionStatus.Value == (int)VersionStatus.Released;
        }

        public bool DeleteDocument(Guid id, bool allVersions,
                                   ObjectStore objectStore = DefaultObjectStore,
                                   DocumentClass documentClass = DefaultDocumentClass)
        {
            if (allVersions)
            {
                var deleteInfoRequest = new[]
				{
					new ObjectRequestType
					{
						SourceSpecification = new ObjectSpecification
						{
							objectStore = objectStore.GetDescription(),
							classId = documentClass.GetDescription(),
							objectId = id.ToString(DefaultIDFormat)
						},
						PropertyFilter = new PropertyFilterType
						{
							maxRecursion = 1,
							maxRecursionSpecified = true,
							IncludeProperties = new[]
							{
								new FilterElementType { Value = "VersionSeries" }
							}
						}
					}
				};

                var deleteInfoResponse = GetObjects(deleteInfoRequest);

                var series = (from responseProperty in (((SingleObjectResponse)deleteInfoResponse[0]).Object).Property
                              where responseProperty.propertyId == "VersionSeries"
                              select ((SingletonObject)responseProperty).Value as ObjectValue).FirstOrDefault();

                if (series != null)
                {
                    id = new Guid(series.objectId);
                }
                else
                {
                    throw new ApplicationException(string.Format("Could not retrieve the version series for {0}",
                                                                 id.ToString(P8ContentEngine.DefaultIDFormat)));
                }
            }

            var deleteRequest = new ExecuteChangesRequest
            {
                ChangeRequest = new[]
				{
					new ChangeRequestType
					{
						id = "1",
						Action = new ActionType[] { new DeleteAction() },
						TargetSpecification = new ObjectSpecification
						{
							objectStore = objectStore.GetDescription(),
							classId = (allVersions) ? "VersionSeries" : documentClass.GetDescription(),
							objectId = id.ToString(P8ContentEngine.DefaultIDFormat)
						}
					}
				},
                refresh = true,
                refreshSpecified = true
            };

            var deleteResult = Execute(deleteRequest);

            return (deleteResult.Length > 0) && deleteResult[0].id == "1";
        }
    }
}