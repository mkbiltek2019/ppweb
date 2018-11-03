using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Detail Energy Expenditure
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class DetailEnergyExpenditure : System.Web.Services.WebService {

    public DetailEnergyExpenditure() {
    }

    public class Activity {
        public int? id { get; set; }
        public string activity { get; set; }

        public HourMin from = new HourMin();

        public HourMin to = new HourMin();
        public int duration { get; set; }
        public double energy { get; set; }

    }

    public class HourMin {
        public int hour { get; set; }
        public int min { get; set; }
    }

    public class Activities {
        public double energy;
        public List<Activity> activities;
    }

    public class DailyActivities {
        public double energy;
        public Activities getDailyActivities(string userId, string clientId) {
            Activities x = new Activities();
            x.activities = JsonConvert.DeserializeObject<List<Activity>>(GetJsonFile(userId, clientId));
            x.energy = x.activities.Sum(a => a.energy);
            //List<Activity> activities = new List<Activity>();
            //activities = JsonConvert.DeserializeObject<Activities>(GetJsonFile(userId, clientId));
            return x;
        }

        public string GetJsonFile(string userId, string clientId) {
            string path = "~/App_Data/users/" + userId + "/clients/" + clientId + "/dailyActivities.json";
            string json = "";
            WebService s = new WebService();
            if (File.Exists(s.Server.MapPath(path))) {
                json = File.ReadAllText(s.Server.MapPath(path));
            }
            return json;
        }

    }


    #region WebMethods
    [WebMethod]
    public string Init() {
        Activity x = new Activity();
        x.id = null;
        x.activity = null;
        x.from = new HourMin();
        x.from.hour = 0;
        x.from.min = 0;
        x.to = new HourMin();
        x.to.hour = 0;
        x.to.min = 0;
        x.duration = 0;
        x.energy = 0;
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Save(string userId, string clientId, List<Activity> activities) {
        try {
            return SaveJsonToFile(userId, clientId, JsonConvert.SerializeObject(activities, Formatting.Indented));
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Get(string userId, string clientId) {
        try {
            return GetJsonFile(userId, clientId);
        } catch (Exception e) { return ("Error: " + e); }
    }

    #endregion

    #region Methods
    public string SaveJsonToFile(string userId, string clientId, string json) {
        try {
            string path = "~/App_Data/users/" + userId + "/clients/" + clientId;
            string filepath = path + "/dailyActivities.json";
            CreateFolder(path);
            WriteFile(filepath, json);
            return "saved";
        } catch (Exception e) { return ("Error: " + e); }
    }

    private string GetJsonFile(string userId, string clientId) {
        string path = "~/App_Data/users/" + userId + "/clients/" + clientId + "/dailyActivities.json";
        string json = "";
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }

    protected void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    protected void WriteFile(string path, string value) {
        File.WriteAllText(Server.MapPath(path), value);
    }
    #endregion


}
