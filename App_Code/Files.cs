using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// files
/// </summary>
[WebService(Namespace = "http://programprehrane.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Files : System.Web.Services.WebService {
    public Files() {
    }

    #region WebMethods
    [WebMethod]
    public string SaveJsonToFile(string foldername, string filename, string json) {
        try {
            string path = "~/App_Data/" + foldername;
            string filepath = path + "/" +  filename + ".json";
            CreateFolder(path);
            WriteFile(filepath, json);
            return GetFile(foldername, filename);
        } catch(Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string GetFile(string foldername, string filename) {
        try {
            string path = "~/App_Data/" + foldername;
            string filepath = path + "/" + filename + ".json";
            if (File.Exists(Server.MapPath(filepath))) {
                return File.ReadAllText(Server.MapPath(filepath));
            } else {
                return null;
            }
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string DeleteLogo(string userId, string filename) {
        try {
            string path = string.Format("~/upload/users/{0}/{1}", userId, filename);
            if (File.Exists(Server.MapPath(path))) {
                File.Delete(Server.MapPath(path));
                return "OK";
            } else {
                return "no file";
            }
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string IsLogoExists(string userId, string filename) {
        try {
            string path = string.Format("~/upload/users/{0}/{1}", userId, filename);
            if (File.Exists(Server.MapPath(path))) {
                return "TRUE";
            } else {
                return "FALSE";
            }
        } catch (Exception e) { return (e.Message); }
    }
    #endregion WebMethods

    #region Methods
    protected void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    protected void WriteFile(string path, string value) {
        File.WriteAllText(Server.MapPath(path), value);
    }
    #endregion Methods




}
