using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Validus.FileNet.Api.Common
{
    public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public List<CustomMultipartFileData> CustomFileData { get; set; }

        public bool InMemory { get; set; }

        public CustomMultipartFormDataStreamProvider(bool inMemory = true)
            : base("~/")
        {
            InMemory = inMemory;

            CustomFileData = new List<CustomMultipartFileData>();
        }

        public CustomMultipartFormDataStreamProvider(string path, bool inMemory = false)
            : base(path)
        {
            InMemory = inMemory;

            CustomFileData = new List<CustomMultipartFileData>();
        }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (!InMemory || headers.ContentDisposition == null || string.IsNullOrWhiteSpace(headers.ContentDisposition.FileName))
                return base.GetStream(parent, headers);

            var data = new CustomMultipartFileData(headers, GetLocalFileName(headers))
            {
                MemoryStream = new MemoryStream()
            };

            FileData.Add(data);
            CustomFileData.Add(data);

            return data.MemoryStream;
        }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            var name = headers.ContentDisposition.FileName;

            if (string.IsNullOrWhiteSpace(name))
                return base.GetLocalFileName(headers);

            // Trim quotations and ensure we just get the name of the file (exclude the path)
            return new FileInfo(name.Replace(@"""", string.Empty)).Name;
        }
    }
}