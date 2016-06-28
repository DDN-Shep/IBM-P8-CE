using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Validus.FileNet.Api.Common;

namespace Validus.FileNet.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/underwriting")]
    public class UnderwritingController : ApiController
    {
        public const ObjectStore os = ObjectStore.Underwriting;
        public const DocumentClass dc = DocumentClass.Underwriting2014;

        // GET api/underwriting/00000000-0000-0000-0000-000000000000
        [HttpGet]
        [Route("{id}")]
        public HttpResponseMessage Get(Guid id)
        {
            var response = default(HttpResponseMessage);

            using (var engine = new P8CEUnderwriting())
            {
                var underwritingDocument = new Underwriting2014Document();

                var document = engine.GetDocumentProperties(id, null, os, dc);

                document.MapToFileNet(underwritingDocument, true, true);

                response = Request.CreateResponse(HttpStatusCode.OK, underwritingDocument);
            }

            return response;
        }

        // GET api/underwriting?policyId=ZZZ0000Z000
        [HttpGet]
        [Route("")]
        public HttpResponseMessage GetByPolicyId(string policyId)
        {
            var response = default(HttpResponseMessage);

            using (var engine = new P8CEUnderwriting())
            {
                var results = engine.SearchUnderwriting(new Dictionary<string, string>
                {
                    { "uwPolicyID", policyId }
                });

                response = Request.CreateResponse(HttpStatusCode.OK, results);
            }

            return response;
        }

        // GET api/underwriting/search?policyId=CFD04&documentType=Slip
        [HttpGet]
        [Route("search")]
        public HttpResponseMessage Search([FromUri]SearchCriteria criteria)
        {
            var response = default(HttpResponseMessage);

            Action<Dictionary<string, string>, string, string> addCriteria = (collection, key, value) =>
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (collection.ContainsKey(key)) collection[key] = value;
                    else collection.Add(key, value);
                }
            };

            using (var engine = new P8CEUnderwriting())
            {
                var underwritingCriteria = new Dictionary<string, string>();

                addCriteria(underwritingCriteria, "uwPolicyID", criteria.PolicyId);
                addCriteria(underwritingCriteria, "uwDocType", criteria.DocumentType);

                var results = engine.SearchUnderwriting(underwritingCriteria);

                response = Request.CreateResponse(HttpStatusCode.OK, results);
            }

            return response;
        }

        // GET api/underwriting/download/00000000-0000-0000-0000-000000000000
        // GET api/underwriting/download/00000000-0000-0000-0000-000000000000?inline=true
        [HttpGet]
        [Route("download/{id}")]
        public HttpResponseMessage Download(Guid id, bool inline = false)
        {
            var response = default(HttpResponseMessage);

            using (var engine = new P8ContentEngine())
            {
                var properties = engine.GetDocument(id, os, dc);

                if (properties != null && properties.Any())
                {
                    var document = properties.MapToFileNet(new Document(os, dc), true, true);

                    var mimeType = (!MimeTypeUtility.DefaultType.Equals(document.MimeType, StringComparison.CurrentCultureIgnoreCase)
                                        ? document.MimeType : null)
                                        ?? MimeTypeUtility.GetMimeType(document.Name, null)
                                        ?? MimeTypeUtility.GetMimeType(document.Title);

                    var fileExtension = MimeTypeUtility.GetFileExtension(mimeType);

                    var fileName = document.Title ?? document.Name;

                    if (!fileName.EndsWith(fileExtension))
                    {
                        fileName += fileExtension;
                    }

                    var content = ((IList<byte[]>)properties["ContentElements"]).First();

                    response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(content)
                    };

                    response.Content.Headers.ContentLength = content.Length;
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(!inline ? "attachment" : "inline")
                    {
                        FileName = fileName
                    };
                }
            }

            return response;
        }

        // POST api/underwriting/upload
        [HttpPost]
        [Route("upload")]
        public Task<HttpResponseMessage> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new CustomMultipartFormDataStreamProvider();

            return Request.Content.ReadAsMultipartAsync(provider).ContinueWith(t =>
            {
                var content = default(byte[]);
                var location = default(string);

                if (t.IsFaulted || t.IsCanceled)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                }

                foreach (var file in provider.CustomFileData)
                {
                    var details = new
                    {
                        MimeType = MimeTypeUtility.GetMimeType(file.LocalFileName),
                        LocalName = file.LocalFileName,
                        FileName = file.Headers.ContentDisposition.FileName,
                        Size = file.Headers.ContentDisposition.Size,
                        Type = file.Headers.ContentType,

                        Stream = file.MemoryStream,

                        Metadata = file.Metadata
                    };

                    using (var stream = details.Stream)
                    {
                        content = stream.ToArray();
                    }

                    using (var engine = new P8CEUnderwriting())
                    {
                        var results = engine.UploadUnderwritingDocuments(new[] {
                            new Underwriting2014Document
                            {
                                Name = details.FileName,
                                MimeType = details.MimeType,
                                Content = content
                            }
                        });

                        if (results != null && results.Any()) location = results.First().ToString("B");
                    }
                }

                var response = Request.CreateResponse(HttpStatusCode.Created);

                response.Headers.Location = new Uri(location);

                return response;
            });
        }

        //// PUT api/values/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //public void Delete(int id)
        //{
        //}
    }
}