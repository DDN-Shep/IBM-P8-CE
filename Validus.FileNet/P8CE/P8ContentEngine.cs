using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Web.Services.Protocols;
using Microsoft.Web.Services3.Security.Tokens;

namespace Validus.FileNet
{
	public partial class P8ContentEngine : IP8ContentEngine
	{
		protected const ObjectStore DefaultObjectStore = ObjectStore.Development;
		protected const DocumentClass DefaultDocumentClass = DocumentClass.Development;
		protected const string DefaultIDFormat = "B";
		protected const int DefaultTTL = 3600;

		protected readonly string KerberosSPN;
		protected readonly long KerberosTTL;

		protected P8CEService Service { get; set; }
		protected KerberosToken Kerberos { get; set; }

		public P8ContentEngine(string serviceURL = null, string kerberosSPN = null,
							   long kerberosTTL = DefaultTTL)
		{
			KerberosSPN = kerberosSPN ?? Config.KerberosSPN;
			KerberosTTL = kerberosTTL;

			Service = new P8CEService
            {
				Url = serviceURL ?? Config.ServiceURL,
				SoapVersion = SoapProtocolVersion.Soap12,
				RequireMtom = true
			};
		}

		protected void InitialiseToken(bool adminOverride = false)
		{
			if (Kerberos != null)
			{
				Kerberos.Dispose();
				Kerberos = null;
			}

			WindowsImpersonationContext windowsContext = null;

			try
			{
				var userToken = IntPtr.Zero;
				var logonAsAdmin = false;
				var logonAsAppPool = false;

				if (adminOverride || (bool.TryParse(Config.LogonAsAdmin, out logonAsAdmin) && logonAsAdmin))
				{
					if (!string.IsNullOrEmpty(Config.DomainName)
						&& !string.IsNullOrEmpty(Config.AdminUsername)
						&& !string.IsNullOrEmpty(Config.AdminPassword))
					{
						if (NativeMethods.LogonUser(Config.AdminUsername, Config.DomainName, Config.AdminPassword,
							                        (int)LogonType.Interactive, (int)LogonProvider.Default, out userToken))
						{
							windowsContext = WindowsIdentity.Impersonate(userToken);
						}
						else
						{
							throw new ApplicationException("Logon for the Administrator failed.");
						}
					}
					else
					{
						throw new ApplicationException("Please provide Domain, Username and Password for the Administrator.");
					}
				}
				else if (bool.TryParse(Config.LogonAsAppPool, out logonAsAppPool) && !logonAsAppPool)
				{
                    if (ServiceSecurityContext.Current == null)
                    {
                        windowsContext = WindowsIdentity.GetCurrent().Impersonate();
                    }
                    else
                    {
                        windowsContext = ServiceSecurityContext.Current.WindowsIdentity.Impersonate();
                    }
				}

				Kerberos = new KerberosToken(KerberosSPN, ImpersonationLevel.Impersonation);

#pragma warning disable CS0618 // Member is obselete, but it is required by the IBM P8 content engine API
                Service.RequestSoapContext.Security.Tokens.Clear();
                Service.RequestSoapContext.Security.Tokens.Add(Kerberos);
				Service.RequestSoapContext.Security.Timestamp.TtlInSeconds = KerberosTTL;
#pragma warning restore CS0618
            }
            finally
			{
				if (windowsContext != null)
					windowsContext.Undo();
			}
		}

        public IDictionary<string, object> GetObject(ObjectRequestType[] request, string property) => GetObject(request, new List<string> { property });

        public IDictionary<string, object> GetObject(ObjectRequestType[] request, IList<string> properties)
		{
			var objectResult = GetObjects(request, properties);

			return objectResult.Count > 0 ? objectResult[0] : null;
		}

        public IList<IDictionary<string, object>> GetObjects(ObjectRequestType[] request, string property) => GetObjects(request, new List<string> { property });

        public IList<IDictionary<string, object>> GetObjects(ObjectRequestType[] request, IList<string> properties)
		{
			var objectResponse = GetObjects(request);

			return objectResponse.OfType<SingleObjectResponse>().Select
				(
					o => o.Object.Property.Where
					(
						p => properties.Contains
						(
							p.propertyId, StringComparer.CurrentCultureIgnoreCase
						)
					).ToDictionary<PropertyType, string, object>
					(
						p => p.propertyId, Utilities.GetPropertyValue
					)
				).Cast<IDictionary<string, object>>().ToList();
		}

        public IList<IDictionary<string, object>> Search(RepositorySearch request, string property) => Search(request, new List<string> { property });

        public IList<IDictionary<string, object>> Search(RepositorySearch request, IList<string> properties)
		{
			var searchResponse = Search(request);

			var searchResults = new List<IDictionary<string, object>>();

			if (searchResponse != null && searchResponse.Object != null)
			{
				searchResults.AddRange
				(
					searchResponse.Object.Select
					(
						o => o.Property.ToDictionary<PropertyType, string, object>
						(
							p => p.propertyId, Utilities.GetPropertyValue
						)
					)
				);
			}

			return searchResults;
		}

        public IList<IDictionary<string, object>> Execute(ExecuteChangesRequest request, string property) => Execute(request, new List<string> { property });

        public IList<IDictionary<string, object>> Execute(ExecuteChangesRequest request, IList<string> properties)
		{
			var executeResponse = Execute(request);

			var executeResults = new List<IDictionary<string, object>>();

			if (executeResponse != null && executeResponse.Length > 0)
			{
				executeResults.AddRange
				(
					executeResponse.Select
					(
						o => o.Property.Where
						(
							p => properties.Contains
							(
								p.propertyId, StringComparer.CurrentCultureIgnoreCase
							)
						).ToDictionary<PropertyType, string, object>
						(
							p => p.propertyId, Utilities.GetPropertyValue
						)
					)
				);
			}

			return executeResults;
		}
		
		public ContentResponseType[] GetContents(ContentRequestType[] request, bool adminOverride = false)
		{
			InitialiseToken(adminOverride);

			var serviceResponse = Service.GetContent(new GetContentRequest
			{
				ContentRequest = request,
				validateOnly = false,
				validateOnlySpecified = true
			});

			foreach (var responseObject in serviceResponse.OfType<ContentErrorResponse>())
			{
				throw new InvalidOperationException(responseObject.ErrorStack.ErrorName.ToString());
			}

			return serviceResponse;
		}

		public ObjectResponseType[] GetObjects(ObjectRequestType[] request, bool adminOverride = false)
		{
			InitialiseToken(adminOverride);

			var serviceResponse = Service.GetObjects(request);

			foreach (var responseObject in serviceResponse.OfType<ErrorStackResponse>())
			{
				throw new InvalidOperationException(responseObject.ErrorStack.ErrorName.ToString());
			}

			return serviceResponse;
		}

		public ObjectSetType Search(RepositorySearch request, bool adminOverride = false)
		{
			InitialiseToken(adminOverride);

			return Service.ExecuteSearch(request);
		}

		public ChangeResponseType[] Execute(ExecuteChangesRequest request, bool adminOverride = false)
		{
			InitialiseToken(adminOverride);

			return Service.ExecuteChanges(request);
		}

		public void Dispose()
		{
			if (Service != null)
			{
				Service.Dispose();
				Service = null;
			}

			if (Kerberos != null)
			{
				Kerberos.Dispose();
				Kerberos = null;
			}
		}
	}
}