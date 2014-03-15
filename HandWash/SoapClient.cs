using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HandWash
{
    class SoapClient
    {
        public string SendSoapRequest( string url, string action, string rawSoapRequest)
        {
            var client = new HttpClient();
            var content = new StringContent(rawSoapRequest);
            client.DefaultRequestHeaders.Add("SOAPAction", action);
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            var t = client.PostAsync(url, content);
            t.Wait();

            var res = t.Result;
            string resultXml;

            if (res.Content.Headers.ContentEncoding.ElementAt(0) == "gzip")
            {
                var contentTask = res.Content.ReadAsStreamAsync();
                contentTask.Wait();
                using (GZipStream stream = new GZipStream( contentTask.Result, CompressionMode.Decompress))
                {
                    const int size = 4096;
                    byte[] buffer = new byte[size];
                    using (MemoryStream memory = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, size);
                            if (count > 0)
                            {
                                memory.Write(buffer, 0, count);
                            }
                        }
                        while (count > 0);

                        resultXml = System.Text.Encoding.Default.GetString(memory.ToArray());
                    }
                }
            }
            else
            {
                var contentTask = res.Content.ReadAsStringAsync();
                contentTask.Wait();

                resultXml = contentTask.Result;

            }


            return resultXml;
        }
    }
}
