using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.IO;
using Igprog;

/// <summary>
/// MyMeals
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class MyMeals : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    DataBase db = new DataBase();
   // string filename = "mymeals";
    Translate t = new Translate();

    public MyMeals() {
    }

    public class NewMyMeals {
        //TODO 
        public string id;
        public string title;
        public string description;
        public string userId;
        public string userGroupId;
        public Data data;
    }

    public class Data {
        public List<Meals.NewMeal> meals;
        public List<Foods.MealsRecommendationEnergy> energyPerc;
    }

    [WebMethod]
    public string Init(Users.NewUser user) {
        NewMyMeals x = new NewMyMeals();
        x.id = Guid.NewGuid().ToString();
        x.title = null;
        x.description = null;
        x.userId = user.userId;
        x.userGroupId = user.userGroupId;
        List<Meals.NewMeal> mm = new List<Meals.NewMeal>();
        List<Foods.MealsRecommendationEnergy> ee = new List<Foods.MealsRecommendationEnergy>();
        Meals.NewMeal m = new Meals.NewMeal();
        m.code = "MM0";
        m.title = "";
        m.description = "";
        m.isSelected = true;
        m.isDisabled = false;
        mm.Add(m);
        Foods.MealsRecommendationEnergy e = new Foods.MealsRecommendationEnergy();
        e.meal.code = m.code;
        e.meal.energyMinPercentage = 0;
        e.meal.energyMaxPercentage = 0;
        ee.Add(e);
        Data data = new Data();
        data.meals = mm;
        data.energyPerc = ee;
        x.data = data;
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string Template(Users.NewUser user, string lang) {
        NewMyMeals x = new NewMyMeals();
        x.id = Guid.NewGuid().ToString();
        x.title = t.Tran("template", lang);
        x.description = t.Tran("this is only template, not a recommendation", lang);
        x.userId = user.userId;
        x.userGroupId = user.userGroupId;
        List<Meals.NewMeal> mm = new List<Meals.NewMeal>();
        List<Foods.MealsRecommendationEnergy> ee = new List<Foods.MealsRecommendationEnergy>();
        string meal = t.Tran("meal", lang);
        Meals.NewMeal m = new Meals.NewMeal();
        m.code = "MMT0";
        m.title = string.Format("{0} 1", meal);
        m.description = "07:00";
        m.isSelected = true;
        m.isDisabled = false;
        mm.Add(m);
        Foods.MealsRecommendationEnergy e = new Foods.MealsRecommendationEnergy();
        e.meal.code = m.code;
        e.meal.energyMinPercentage = 10;
        e.meal.energyMaxPercentage = 15;
        ee.Add(e);
        m = new Meals.NewMeal();
        m.code = "MMT1";
        m.title = string.Format("{0} 2", meal);
        m.description = "9:30";
        m.isSelected = true;
        m.isDisabled = false;
        mm.Add(m);
        e = new Foods.MealsRecommendationEnergy();
        e.meal.code = m.code;
        e.meal.energyMinPercentage = 5;
        e.meal.energyMaxPercentage = 10;
        ee.Add(e);
        m = new Meals.NewMeal();
        m.code = "MMT2";
        m.title = string.Format("{0} 3", meal);
        m.description = "11:00";
        m.isSelected = true;
        m.isDisabled = false;
        mm.Add(m);
        e = new Foods.MealsRecommendationEnergy();
        e.meal.code = m.code;
        e.meal.energyMinPercentage = 15;
        e.meal.energyMaxPercentage = 25;
        ee.Add(e);
        Data data = new Data();
        data.meals = mm;
        data.energyPerc = ee;
        x.data = data;
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    //TODO
    [WebMethod]
    public string Load(string userId) {
        try {
            List<Meals.NewMeal> xx = new List<Meals.NewMeal>();
            string json = ""; // GetJsonFile(userId, filename);
            return json;
        }
        catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(string userId, NewMyMeals x) {
        db.CreateDataBase(userId, db.meals);
        try {
            string sql = "";
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            sql = string.Format(@"BEGIN;
                    INSERT OR REPLACE INTO meals (id, title, description, userId, userGroupId)
                    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}');
                    COMMIT;", x.id, x.title, x.description, x.userId, x.userGroupId);
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
            connection.Close();
            SaveJsonToFile(userId, x.id, JsonConvert.SerializeObject(x, Formatting.Indented));
            return JsonConvert.SerializeObject(x, Formatting.Indented);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            List<Meals.NewMeal> xx = new List<Meals.NewMeal>();
            string json = GetJsonFile(userId, id);
            return json;
        }
        catch (Exception e) { return ("Error: " + e); }
    }

    /*
    [WebMethod]
    public string Save_(string userId, string json) {
        try {
            string path = string.Format("~/App_Data/users/{0}/meals", userId);
            string filepath = string.Format("{0}/{1}.json", path, filename);
            SaveJsonToFile(userId, filename, json);
            return JsonConvert.SerializeObject(GetJson(userId, filename), Formatting.Indented);
        } catch (Exception e) { return ("Error: " + e); }
    }
    */

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

/*
    private List<Meals.NewMeal> GetJson(string userId, string filename) {
        return JsonConvert.DeserializeObject<List<Meals.NewMeal>>(GetJsonFile(userId, filename));
    }
*/

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
