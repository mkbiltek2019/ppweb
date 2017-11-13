using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

/// <summary>
/// Admin
/// </summary>
[WebService(Namespace = "http://programprehrane.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Admin : System.Web.Services.WebService {

    public Admin() {
    }

    [WebMethod]
    public bool Login(string username, string password) {
        if(username == "" && password == "") {
            return true;
        } else {
            return false;
        }
    }

}
