using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using System.IO;
using Igprog;

/// <summary>
/// MyMeals
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class MyMeals : System.Web.Services.WebService {
    string filename = "mymealse";

    public MyMeals() {
    }

    public class NewMeal {
        public string code;
        public string title;
        public string description;
        public bool isSelected;
        public bool isDisabled;
        public double energyPerc;
    }

    [WebMethod]
    public string Load(string userId) {
        try {
           List<NewMeal> xx = new List<NewMeal>();
            string json = GetJsonFile(userId, filename);
            return json;
            
        }
        catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(string userId, string json) {
        try {
            string path = string.Format("~/App_Data/users/{0}/meals/{1}", userId);
            string filepath = string.Format("{0}/{1}.json", path, filename);
            SaveJsonToFile(userId, filename, json);
            return JsonConvert.SerializeObject(Get(userId, filename), Formatting.Indented);
        } catch (Exception e) { return ("Error: " + e); }
    }

    public void SaveJsonToFile(string userId, string filename, string json) {
            string path = string.Format("~/App_Data/users/{0}/meals", userId);
            string filepath = string.Format("{0}/{1}.json", path, filename);
            CreateFolder(path);
            WriteFile(filepath, json);
    }

    /*
    public void DeleteJson(string userId, string filename) {
        string path = Server.MapPath(string.Format("~/App_Data/users/{0}/menues", userId));
        string filepath = string.Format("{0}/{1}.json", path, filename);
        if (File.Exists(filepath)) {
            File.Delete(filepath);
        }
    }*/

    private List<NewMeal> Get(string userId, string filename) {
        return JsonConvert.DeserializeObject<List<NewMeal>>(GetJsonFile(userId, filename));
    }

    private string GetJsonFile(string userId, string filename) {
        string path = string.Format("~/App_Data/users/{0}/meals/{1}.json", userId, filename);
        string json = null;
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }

    /*private string GetJsonFile(string path) {
        string json = null;
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }*/

    protected void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    protected void WriteFile(string path, string value) {
        File.WriteAllText(Server.MapPath(path), value);
    }





    

}
