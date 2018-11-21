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
    string filename = "mymeals";
    Translate t = new Translate();

    public MyMeals() {
    }

    public class MyMealsData {
        //TODO 
        public string id;
        public string title;
        public string description;
        public string userId;
        public string userGroupId;
        public List<Meals.NewMeal> meals;
        public List<Foods.MealsRecommendationEnergy> energyPerc;
    }

    [WebMethod]
    public string Init(Users.NewUser user) {
        MyMealsData data = new MyMealsData();
        data.id = Guid.NewGuid().ToString();
        data.title = null;
        data.description = null;
        data.userId = user.userId;
        data.userGroupId = user.userGroupId;
        List<Meals.NewMeal> xx = new List<Meals.NewMeal>();
        List<Foods.MealsRecommendationEnergy> ee = new List<Foods.MealsRecommendationEnergy>();
        Meals.NewMeal x = new Meals.NewMeal();
        x.code = "MM0";
        x.title = "";
        x.description = "";
        x.isSelected = true;
        x.isDisabled = false;
        xx.Add(x);
        Foods.MealsRecommendationEnergy e = new Foods.MealsRecommendationEnergy();
        e.meal.code = x.code;
        e.meal.energyMinPercentage = 0;
        e.meal.energyMaxPercentage = 0;
        ee.Add(e);
        data.meals = xx;
        data.energyPerc = ee;
        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    [WebMethod]
    public string Template(Users.NewUser user, string lang) {
        MyMealsData data = new MyMealsData();
        data.id = Guid.NewGuid().ToString();
        data.title = t.Tran("template", lang);
        data.description = t.Tran("this is only template, not a recommendation", lang);
        data.userId = user.userId;
        data.userGroupId = user.userGroupId;
        List<Meals.NewMeal> xx = new List<Meals.NewMeal>();
        List<Foods.MealsRecommendationEnergy> ee = new List<Foods.MealsRecommendationEnergy>();
        string meal = t.Tran("meal", lang);
        Meals.NewMeal x = new Meals.NewMeal();
        x.code = "MMT0";
        x.title = string.Format("{0} 1", meal);
        x.description = "07:00";
        x.isSelected = true;
        x.isDisabled = false;
        xx.Add(x);
        Foods.MealsRecommendationEnergy e = new Foods.MealsRecommendationEnergy();
        e.meal.code = x.code;
        e.meal.energyMinPercentage = 10;
        e.meal.energyMaxPercentage = 15;
        ee.Add(e);
        x = new Meals.NewMeal();
        x.code = "MMT1";
        x.title = string.Format("{0} 2", meal);
        x.description = "9:30";
        x.isSelected = true;
        x.isDisabled = false;
        xx.Add(x);
        e = new Foods.MealsRecommendationEnergy();
        e.meal.code = x.code;
        e.meal.energyMinPercentage = 5;
        e.meal.energyMaxPercentage = 10;
        ee.Add(e);
        x = new Meals.NewMeal();
        x.code = "MMT2";
        x.title = string.Format("{0} 3", meal);
        x.description = "11:00";
        x.isSelected = true;
        x.isDisabled = false;
        xx.Add(x);
        e = new Foods.MealsRecommendationEnergy();
        e.meal.code = x.code;
        e.meal.energyMinPercentage = 15;
        e.meal.energyMaxPercentage = 25;
        ee.Add(e);
        data.meals = xx;
        data.energyPerc = ee;
        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    [WebMethod]
    public string Load(string userId) {
        try {
            List<Meals.NewMeal> xx = new List<Meals.NewMeal>();
            string json = GetJsonFile(userId, filename);
            return json;
        }
        catch (Exception e) { return ("Error: " + e); }
    }

    //TODO
    [WebMethod]
    public string Save_(string userId, MyMealsData x) {
        db.CreateDataBase(userId, db.meals);
        try {
            string sql = "";
            //if (x.id == null) {
            //    x.id = Convert.ToString(Guid.NewGuid());
            //}

            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            sql = string.Format(@"BEGIN;
                    INSERT OR REPLACE INTO meals (id, title, description, userId, userGroupId)
                    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}');
                    COMMIT;");
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
            connection.Close();
            SaveJsonToFile(userId, filename, JsonConvert.SerializeObject(x.meals, Formatting.Indented));
            return JsonConvert.SerializeObject(x, Formatting.Indented);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Save(string userId, string json) {
        try {
            string path = string.Format("~/App_Data/users/{0}/meals", userId);
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

    private List<Meals.NewMeal> Get(string userId, string filename) {
        return JsonConvert.DeserializeObject<List<Meals.NewMeal>>(GetJsonFile(userId, filename));
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
