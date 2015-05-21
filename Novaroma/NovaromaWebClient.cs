using System;
using System.Net;

namespace Novaroma {

    public class NovaromaWebClient : WebClient {

        protected override WebRequest GetWebRequest(Uri address) {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Ssl3;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var request = base.GetWebRequest(address) as HttpWebRequest;

            if (request != null)
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            return request;
        }
    }
}
