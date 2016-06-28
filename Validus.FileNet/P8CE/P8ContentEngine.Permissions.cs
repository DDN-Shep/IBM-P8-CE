using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Validus.FileNet
{
	public partial class P8ContentEngine : IP8CEPermissions
	{
		public bool AllowDocumentAccess(Guid id,
		                                IList<string> allowUsers,
		                                ObjectStore objectStore = DefaultObjectStore,
		                                DocumentClass documentClass = DefaultDocumentClass)
		{
			var accessProperties = new List<DependentObjectType>();

			if (allowUsers != null)
			{
				accessProperties.AddRange(allowUsers.Select(allowUser => new DependentObjectType
				{
					classId = "AccessPermission",
					dependentAction = DependentObjectTypeDependentAction.Insert,
					dependentActionSpecified = true,
					Property = new PropertyType[]
					{
						new SingletonString
						{
							propertyId = "GranteeName",
							Value = allowUser
						},
						new SingletonInteger32
						{
							propertyId = "AccessType",
							Value = (int)AccessType.Allow,
							ValueSpecified = true
						},
						new SingletonInteger32
						{
							propertyId = "AccessMask",
							Value = (int)AccessLevel.WriteDocument,
							ValueSpecified = true
						},
						new SingletonInteger32
						{
							propertyId = "InheritableDepth",
							Value = 0,
							ValueSpecified = true
						}
					}
				}).ToList());
			}

			var actionProperties = new List<ModifiablePropertyType>
			{
				new ListOfObject
				{
					propertyId = "Permissions",
					Value = accessProperties.ToArray()
				}
			};
			
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
							objectId = id.ToString(P8ContentEngine.DefaultIDFormat)
						}
					}
				},
				refresh = true,
				refreshSpecified = true
			};

			return Execute(updateRequest, true).Length > 0;
		}

		public bool DenyDocumentAccess(Guid id,
		                               IList<string> denyUsers,
		                               ObjectStore objectStore = DefaultObjectStore,
		                               DocumentClass documentClass = DefaultDocumentClass)
		{
			var accessProperties = new List<DependentObjectType>();

			if (denyUsers != null)
			{
				accessProperties.AddRange(denyUsers.Select(denyUser => new DependentObjectType
				{
					classId = "AccessPermission",
					dependentAction = DependentObjectTypeDependentAction.Insert,
					dependentActionSpecified = true,
					Property = new PropertyType[]
					{
						new SingletonString
						{
							propertyId = "GranteeName",
							Value = denyUser
						},
						new SingletonInteger32
						{
							propertyId = "AccessType",
							Value = (int)AccessType.Deny,
							ValueSpecified = true
						},
						new SingletonInteger32
						{
							propertyId = "AccessMask",
							Value = (int)AccessLevel.FullControlDocument,
							ValueSpecified = true
						},
						new SingletonInteger32
						{
							propertyId = "InheritableDepth",
							Value = 0,
							ValueSpecified = true
						}
					}
				}).ToList());
			}

			var actionProperties = new List<ModifiablePropertyType>
			{
				new ListOfObject
				{
					propertyId = "Permissions",
					Value = accessProperties.ToArray()
				}
			};

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

			return Execute(updateRequest, true).Length > 0;
		}

		public bool RemoveDocumentAccess(Guid id,
		                                 IList<string> removeUsers,
		                                 ObjectStore objectStore = DefaultObjectStore,
		                                 DocumentClass documentClass = DefaultDocumentClass)
		{
			var permissionsList = RetrieveDocumentAccess(id, objectStore, documentClass);
			var accessPermissions = removeUsers.Select
				(
					u => permissionsList.FirstOrDefault
					(
						kvp => kvp["GranteeName"].ToString().Equals(u, StringComparison.CurrentCultureIgnoreCase)
					)
				).Select(permissionsList.IndexOf).Where(i => i >= 0).Select
				(
					userIndex => new DependentObjectType
					{
						classId = "AccessPermission",
						dependentAction = DependentObjectTypeDependentAction.Delete,
						dependentActionSpecified = true,
						originalIndex = userIndex,
						originalIndexSpecified = true
					}
				).ToArray();

			if (accessPermissions.Length == 0)
				return true;

			var actionProperties = new List<ModifiablePropertyType>
			{
				new ListOfObject
				{
					propertyId = "Permissions",
					Value = accessPermissions
				}
			};

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

			return Execute(updateRequest, true).Length > 0;
		}

		public bool ResetDocumentAccess(Guid id,
		                                ObjectStore objectStore = DefaultObjectStore,
		                                DocumentClass documentClass = DefaultDocumentClass)
		{
			throw new NotImplementedException();
		}

		public IList<IDictionary<string, object>> RetrieveDocumentAccess(Guid id,
		                                                                 ObjectStore objectStore = DefaultObjectStore,
		                                                                 DocumentClass documentClass = DefaultDocumentClass)
		{
			var accessInfoRequest = new[]
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
							new FilterElementType { Value = "Permissions" },
							new FilterElementType { Value = "AccessPermission" },
							new FilterElementType { Value = "AccessMask" },
							new FilterElementType { Value = "AccessType" },
							new FilterElementType { Value = "GranteeName" },
							new FilterElementType { Value = "GranteeType" },
							new FilterElementType { Value = "InheritableDepth" },
							new FilterElementType { Value = "PermissionSource" }
						}
					}
				}
			};

			var accessInfoResponse = GetObjects(accessInfoRequest);

			var permissionsList = accessInfoResponse.OfType<SingleObjectResponse>()
			                                        .SelectMany(o => o.Object.Property)
			                                        .Where(p => p.propertyId == "Permissions")
			                                        .Select(p => p as ListOfObject);

			var accessPermissions = new List<IDictionary<string, object>>();

			foreach (var permission in permissionsList.SelectMany(o => o.Value))
			{
				dynamic accessPermission = new ExpandoObject();

				foreach (var property in permission.Property)
				{
					switch (property.propertyId)
					{
						case "AccessMask":
							accessPermission.AccessMask = ((SingletonInteger32)property).Value;
							break;
						case "AccessType":
							accessPermission.AccessType = ((SingletonInteger32)property).Value;
							break;
						case "GranteeName":
							accessPermission.GranteeName = ((SingletonString)property).Value;
							break;
						case "GranteeType":
							accessPermission.GranteeType = ((SingletonInteger32)property).Value;
							break;
						case "InheritableDepth":
							accessPermission.InheritableDepth = ((SingletonInteger32)property).Value;
							break;
						case "PermissionSource":
							accessPermission.PermissionSource = ((SingletonInteger32)property).Value;
							break;
						default:
							continue;
					}
				}

				accessPermissions.Add(accessPermission);
			}

			return accessPermissions;
		}

		public bool TestDocumentAccess(Guid id,
		                               ObjectStore objectStore = DefaultObjectStore,
		                               DocumentClass documentClass = DefaultDocumentClass)
		{
			throw new NotImplementedException();
		}
	}
}