using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Net;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// USDA
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Usda : System.Web.Services.WebService {

    public Usda() {
    }

    string apiUrl = "https://api.nal.usda.gov/fdc/v1/";
    string apiKey = "RqTcaxwMWOal7N4XZySPEtx8hZBXFkZ1v320FQwQ";

    //https://api.nal.usda.gov/fdc/v1/search?api_key=RqTcaxwMWOal7N4XZySPEtx8hZBXFkZ1v320FQwQ&generalSearchInput=Cheddar%20Cheese

    //Documentation: https://fdc.nal.usda.gov/api-guide.html#bkmk-6


    //[WebMethod]
    //public string Load(string page) {
    //    return RequestData(RequestStr(string.Format("pageNumber={0}", page)));
    //}

    [WebMethod]
    public string Search(string param) {
        return RequestData(RequestStr(param));
    }

    [WebMethod]
    public string Get(string id) {
        string request = string.Format("{0}{1}?api_key={2}", apiUrl, id, apiKey);
        return RequestData(request);
    }

    private string RequestStr(string param) {
        string url = string.Format("{0}search", apiUrl);
        return string.Format("{0}?api_key={1}&{2}", url, apiKey, param);
    }

    private string RequestData(string url) {
        try {
            //in .NET 4.0, TLS 1.2 is not supported, but if you have .NET 4.5 (or above) installed on the system
            //then you still can opt in for TLS 1.2 even if your application framework doesn't support it.
            //The only problem is that SecurityProtocolType in .NET 4.0 doesn't have an entry for TLS1.2,
            //so we'd have to use a numerical representation of this enum value:
            //ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            //instead of:
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;

            // Create a request for the URL.  
            WebRequest request = WebRequest.Create(url);
            // If required by the server, set the credentials.  
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.  
            WebResponse response = request.GetResponse();
            // Display the status.  
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.  
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();
            // Display the content.  
            Console.WriteLine(responseFromServer);
            // Clean up the streams and the response.  
            reader.Close();
            response.Close();
            return responseFromServer;
        } catch (Exception e) {
            return e.Message;
        } 
    }

}
