using System;
using System.Net;

namespace Novaroma {

    public class NovaromaWebClient: WebClient {

        protected override WebRequest GetWebRequest(Uri address) {
            var request = base.GetWebRequest(address) as HttpWebRequest;

            if (request != null) 
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            return request;
        }
    }
}
