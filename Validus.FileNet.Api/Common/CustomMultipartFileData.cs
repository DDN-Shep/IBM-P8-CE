using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Validus.FileNet.Api.Common
{
    public class CustomMultipartFileData : MultipartFileData
    {
        public MemoryStream MemoryStream { get; set; }

        public Dictionary<string, object> Metadata { get; set; }

        public CustomMultipartFileData(HttpContentHeaders headers, string name)
            : base(headers, name)
        { }
    }
}