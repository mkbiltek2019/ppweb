﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// WeeklyMenus
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class WeeklyMenus : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    DataBase db = new DataBase();
    Translate t = new Translate();

    public WeeklyMenus() {
    }

    public class NewWeeklyMenus {
        public string id;
        public string title;
        public string note;
        public Diets.NewDiet diet;
        public List<string> menuList;
        public DateTime date;
        public Clients.NewClient client;
        public string userId;
        public string userGroupId;
    }

 
    [WebMethod]
    public string Init(Users.NewUser user, Clients.NewClient client, string lang) {
        NewWeeklyMenus x = new NewWeeklyMenus();
        x.id = null;
        x.title = null;
        x.note = null;
        x.diet = new Diets.NewDiet();
        x.diet.id = client.clientData.diet.id;
        x.diet.diet = t.Tran(client.clientData.diet.diet, lang);
        x.menuList = new List<string>() { "", "", "", "", "", "", "" };
        x.date = DateTime.UtcNow;
        x.client = client;
        x.userId = user.userId;
        x.userGroupId = user.userGroupId;
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

     [WebMethod]
    public string Load(string userId, string lang) {
        try {
            return JsonConvert.SerializeObject(LoadWeeklyMenus(userId, lang), Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Get(string userId, string id, string lang) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = string.Format(@"SELECT w.id, w.title, w.note, w.dietId, w.diet, w.menuList, w.date, w.clientId, c.firstName, c.lastName, w.userId, w.userGroupId FROM weeklymenus w
                    LEFT OUTER JOIN clients c ON w.clientId = w.clientId
                    WHERE w.id = '{0}' GROUP BY w.id ORDER BY w.rowid DESC", id);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            NewWeeklyMenus x = new NewWeeklyMenus();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.note = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.diet = new Diets.NewDiet();
                x.diet.id = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.diet.diet = reader.GetValue(4) == DBNull.Value ? "" : t.Tran(reader.GetString(4), lang);
                x.menuList = reader.GetValue(5) == DBNull.Value ? new List<string>() : reader.GetString(5).Split(',').ToList();
                x.date = reader.GetValue(6) == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(reader.GetString(6));
                x.client = new Clients.NewClient();
                x.client.clientId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                x.client.firstName = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
                x.client.lastName = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                x.userId = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                x.userGroupId = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
            }
            connection.Close();
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Save(string userId, NewWeeklyMenus x) {
        try {
            db.CreateDataBase(userId, db.weeklymenus);
            if (string.IsNullOrEmpty(x.id) && Check(userId, x.title)) {
                return "error";
            } else {
                if(string.IsNullOrEmpty(x.id)) {
                    x.id = Convert.ToString(Guid.NewGuid());
                }
                string sql = "";
                SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                sql = string.Format(@"BEGIN;
                    INSERT OR REPLACE INTO weeklymenus (id, title, note, dietId, diet, menuList, date, clientId, userId, userGroupId)
                    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}');
                    COMMIT;", x.id, x.title, x.note, x.diet.id, x.diet.diet, string.Join(",",x.menuList), x.date, x.client.clientId, x.userId, x.userGroupId);
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return JsonConvert.SerializeObject(x, Formatting.None);
            }
        } catch (Exception e) { return (e.Message); }
    }


    //TODO:
    [WebMethod]
    public string GetWeeklyMenusTotals(string userId, List<string> menuList) {
        try {
            Foods f = new Foods();
            List<Foods.Totals> xx = new List<Foods.Totals>();

            List<List<Foods.MealsTotal>> lmt = new List<List<Foods.MealsTotal>>();  

            foreach (string menu in menuList) {
                if (!string.IsNullOrEmpty(menu)) {
                    Menues m = new Menues();
                    Menues.NewMenu nm = new Menues.NewMenu();
                    nm = m.WeeklyMenu(userId, menu);
                    Foods.Totals t = new Foods.Totals();
                    t = f.GetTotals_(nm.data.selectedFoods, nm.data.meals);
                    xx.Add(t);

                    List<Foods.MealsTotal> mt = new List<Foods.MealsTotal>();
                    mt = f.GetMealsTotal(nm.data.selectedFoods, nm.data.meals);
                    lmt.Add(mt);

                    //TODO
                    //foreach(List<Foods.MealsTotal> l in lmt) {
                    //    List<Foods.MealsTotal> mt_ = new List<Foods.MealsTotal>();
                    //}


                }
            }
            Foods.Totals x = new Foods.Totals();
            x.mass = f.SmartRound(xx.Average(a => a.mass));
            x.energy = f.SmartRound(xx.Average(a => a.energy));
            x.carbohydrates = f.SmartRound(xx.Average(a => a.carbohydrates));
            x.carbohydratesPercentage = f.SmartRound(xx.Average(a => a.carbohydratesPercentage));
            x.proteins = f.SmartRound(xx.Average(a => a.proteins));
            x.proteinsPercentage = f.SmartRound(xx.Average(a => a.proteinsPercentage));
            x.fats = f.SmartRound(xx.Average(a => a.fats));
            x.fatsPercentage = f.SmartRound(xx.Average(a => a.fatsPercentage));
            x.servings.cerealsServ = f.SmartRound(xx.Average(a => a.servings.cerealsServ));
            x.servings.vegetablesServ = f.SmartRound(xx.Average(a => a.servings.vegetablesServ));
            x.servings.fruitServ = f.SmartRound(xx.Average(a => a.servings.fruitServ));
            x.servings.meatServ = f.SmartRound(xx.Average(a => a.servings.meatServ));
            x.servings.milkServ = f.SmartRound(xx.Average(a => a.servings.milkServ));
            x.servings.fatsServ = f.SmartRound(xx.Average(a => a.servings.fatsServ));
            x.servings.otherFoodsServ = f.SmartRound(xx.Average(a => a.servings.otherFoodsServ));
            x.servings.otherFoodsEnergy = f.SmartRound(xx.Average(a => a.servings.otherFoodsEnergy));


            // x.mealsTotal = GetMealsTotal(selectedFoods, meals));  //TODO


            x.starch = f.SmartRound(xx.Average(a => a.starch));
            x.totalSugar = f.SmartRound(xx.Average(a => a.totalSugar));
            x.glucose = f.SmartRound(xx.Average(a => a.glucose));
            x.fructose = f.SmartRound(xx.Average(a => a.fructose));
            x.saccharose = f.SmartRound(xx.Average(a => a.saccharose));
            x.maltose = f.SmartRound(xx.Average(a => a.maltose));
            x.lactose = f.SmartRound(xx.Average(a => a.lactose));
            x.fibers = f.SmartRound(xx.Average(a => a.fibers));
            x.saturatedFats = f.SmartRound(xx.Average(a => a.saturatedFats));
            x.monounsaturatedFats = f.SmartRound(xx.Average(a => a.monounsaturatedFats));
            x.polyunsaturatedFats = f.SmartRound(xx.Average(a => a.polyunsaturatedFats));
            x.trifluoroaceticAcid = f.SmartRound(xx.Average(a => a.trifluoroaceticAcid));
            x.cholesterol = f.SmartRound(xx.Average(a => a.cholesterol));
            x.sodium = f.SmartRound(xx.Average(a => a.sodium));
            x.potassium = f.SmartRound(xx.Average(a => a.potassium));
            x.calcium = f.SmartRound(xx.Average(a => a.calcium));
            x.magnesium = f.SmartRound(xx.Average(a => a.magnesium));
            x.phosphorus = f.SmartRound(xx.Average(a => a.phosphorus));
            x.iron = f.SmartRound(xx.Average(a => a.iron));
            x.copper = f.SmartRound(xx.Average(a => a.copper));
            x.zinc = f.SmartRound(xx.Average(a => a.zinc));
            x.chlorine = f.SmartRound(xx.Average(a => a.chlorine));
            x.manganese = f.SmartRound(xx.Average(a => a.manganese));
            x.selenium = f.SmartRound(xx.Average(a => a.selenium));
            x.iodine = f.SmartRound(xx.Average(a => a.iodine));
            x.retinol = f.SmartRound(xx.Average(a => a.retinol));
            x.carotene = f.SmartRound(xx.Average(a => a.carotene));
            x.vitaminD = f.SmartRound(xx.Average(a => a.vitaminD));
            x.vitaminE = f.SmartRound(xx.Average(a => a.vitaminE));
            x.vitaminB1 = f.SmartRound(xx.Average(a => a.vitaminB1));
            x.vitaminB2 = f.SmartRound(xx.Average(a => a.vitaminB2));
            x.vitaminB3 = f.SmartRound(xx.Average(a => a.vitaminB3));
            x.vitaminB6 = f.SmartRound(xx.Average(a => a.vitaminB6));
            x.vitaminB12 = f.SmartRound(xx.Average(a => a.vitaminB12));
            x.folate = f.SmartRound(xx.Average(a => a.folate));
            x.pantothenicAcid = f.SmartRound(xx.Average(a => a.pantothenicAcid));
            x.biotin = f.SmartRound(xx.Average(a => a.biotin));
            x.vitaminC = f.SmartRound(xx.Average(a => a.vitaminC));
            x.vitaminK = f.SmartRound(xx.Average(a => a.vitaminK));

            x.price.value = Math.Round(xx.Average(a => a.price.value), 2);
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Delete(string userId, string id, string lang) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = string.Format("DELETE FROM weeklymenus WHERE id = '{0}'", id);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
            connection.Close();
            List<NewWeeklyMenus> xx = LoadWeeklyMenus(userId, lang);
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

     public List<NewWeeklyMenus> LoadWeeklyMenus(string userId, string lang) {
        db.CreateDataBase(userId, db.weeklymenus);
        SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
        connection.Open();
        string sql = @"SELECT w.id, w.title, w.note, w.dietId, w.diet, w.menuList, w.date, w.clientId, c.firstName, c.lastName, w.userId, w.userGroupId FROM weeklymenus w
                    LEFT OUTER JOIN clients c ON w.clientId = w.clientId
                    GROUP BY w.id
                    ORDER BY w.rowid DESC";
        SQLiteCommand command = new SQLiteCommand(sql, connection);
        List<NewWeeklyMenus> xx = new List<NewWeeklyMenus>();
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read()) {
            NewWeeklyMenus x = new NewWeeklyMenus();
            x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
            x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
            x.note = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
            x.diet = new Diets.NewDiet();
            x.diet.id = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
            x.diet.diet = reader.GetValue(4) == DBNull.Value ? "" : t.Tran(reader.GetString(4), lang);
            x.menuList = reader.GetValue(5) == DBNull.Value ? new List<string>() : reader.GetString(5).Split(',').ToList();
            x.date = reader.GetValue(6) == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(reader.GetString(6));
            x.client = new Clients.NewClient();
            x.client.clientId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
            x.client.firstName = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
            x.client.lastName = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
            x.userId = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
            x.userGroupId = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
            xx.Add(x);
        }
        connection.Close();
        return xx;
    }

    private bool Check(string userId, string title) {
        try {
            bool result = false;
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = string.Format("SELECT EXISTS (SELECT id FROM weeklymenus WHERE LOWER(title) = '{0}')", title.ToLower());
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                result = reader.GetBoolean(0);
            }
            connection.Close();
            return result;
        } catch (Exception e) { return false; }
    }

}
