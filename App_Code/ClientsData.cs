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
/// ClientsData
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class ClientsData : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    string usersDataBase = ConfigurationManager.AppSettings["UsersDataBase"];

    DataBase db = new DataBase();
    Users.CheckUser c = new Users.CheckUser();
    Calculations calculation = new Calculations();
    Global g = new Global();
    Equations E = new Equations();

    public ClientsData() {
    }

    public class NewClientData {
        public int? id { get; set; }
        public string clientId { get; set; }
        public int age {get; set;}

        public Clients.Gender gender = new Clients.Gender();

        public double height { get; set; }
        public double  weight { get; set; }
        public double waist { get; set; }
        public double hip { get; set; }

        public Calculations.Pal pal = new Calculations.Pal();

        public Goals.NewGoal goal = new Goals.NewGoal();

        public List<Activities.ClientActivity> activities { get; set; }

        public Diets.NewDiet diet { get; set; }
        public List<Meals.NewMeal> meals { get; set; }

        public string date { get; set; }

        public string userId { get; set; }

        public DetailEnergyExpenditure.Activities dailyActivities = new DetailEnergyExpenditure.Activities();

        public MyMeals.NewMyMeals myMeals = new MyMeals.NewMyMeals();

        public string clientNote { get; set; }

        public string bmrEquation;

        //public List<DetailEnergyExpenditure.Activity> dailyActivities = new List<DetailEnergyExpenditure.Activity>();

        //TODO add detailTee;
    }

    //public class DailyActivities {
    //    public double energy;
    //    public List<DetailEnergyExpenditure.Activity> activities = new List<DetailEnergyExpenditure.Activity>();
    //}

    #region WebMethods
    [WebMethod]
    public string Init(Clients.NewClient client) {
        NewClientData x = new NewClientData();
        x.id = null;
        x.clientId = client.clientId;
        x.age = calculation.Age(client.birthDate);
        x.gender = GetGender(client.gender.value);
        x.gender.title = client.gender.title;
        x.height = 0;
        x.weight = 0;
        x.waist = 0;
        x.hip = 0;
        x.pal = new Calculations.Pal();
        x.goal = new Goals.NewGoal();
        x.activities = new List<Activities.ClientActivity>();
        x.diet = new Diets.NewDiet();
        x.meals = new List<Meals.NewMeal>();
        x.date = DateTime.UtcNow.ToString();
        x.userId = null;
        x.dailyActivities = new DetailEnergyExpenditure.Activities();
        x.myMeals = new MyMeals.NewMyMeals();
        x.clientNote = null;
        x.bmrEquation = E.MifflinStJeor;
        string json = JsonConvert.SerializeObject(x, Formatting.None);
        return json;
    }

    [WebMethod]
    public string Load(string userId) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"SELECT cd.rowid, cd.clientId, c.birthDate, c.gender, cd.height, cd.weight, cd.waist, cd.hip, cd.pal, cd.goal, cd.activities, cd.diet, cd.meals, cd.date, cd.userId
                        FROM clientsdata as cd
                        LEFT OUTER JOIN clients as c
                        ON cd.clientId = c.clientId
                        ORDER BY cd.rowid DESC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewClientData> xx = new List<NewClientData>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewClientData x = new NewClientData();
                Calculations c = new Calculations();
                Goals g = new Goals();
                x.id = reader.GetInt32(0);
                x.clientId = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.age = calculation.Age(reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2));
                x.gender.value = reader.GetValue(3) == DBNull.Value ? 0 : reader.GetInt32(3);
                x.gender.title = GetGender(x.gender.value).title;
                x.height = reader.GetValue(4) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(4));
                x.weight = reader.GetValue(5) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(5));
                x.waist = reader.GetValue(6) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(6));
                x.hip = reader.GetValue(7) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(7));
                x.pal = c.GetPal(reader.GetValue(8) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(8)));
                x.goal.code = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                x.goal.title = g.GetGoal(x.goal.code).title;
                x.activities = JsonConvert.DeserializeObject<List<Activities.ClientActivity>>(reader.GetString(10));
                x.diet = JsonConvert.DeserializeObject<Diets.NewDiet>(reader.GetString(11));
                x.meals = JsonConvert.DeserializeObject<List<Meals.NewMeal>>(reader.GetString(12));
                x.date = reader.GetValue(13) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(13);
                x.userId = reader.GetValue(14) == DBNull.Value ? "" : reader.GetString(14);
                DetailEnergyExpenditure.DailyActivities da = new DetailEnergyExpenditure.DailyActivities();
                x.dailyActivities = da.getDailyActivities(userId, x.clientId);
                x.myMeals = new MyMeals.NewMyMeals();
                xx.Add(x);
            }
            connection.Close();
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(string userId, NewClientData x, int userType) {
        try {
            db.CreateDataBase(userId, db.clientsData);
            db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.clients, "note");  //new column in clients tbl.
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = "";
                if (Check(userId, x) != false) {
                    sql = string.Format(@"INSERT INTO clientsdata (clientId, height, weight, waist, hip, pal, goal, activities, diet, meals, date, userId)
                            VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}');
                            UPDATE clients SET note = '{12}' WHERE clientId = '{0}';"
                            , x.clientId, x.height, x.weight, x.waist, x.hip, x.pal.value, x.goal.code
                            , JsonConvert.SerializeObject(x.activities, Formatting.None)
                            , JsonConvert.SerializeObject(x.diet, Formatting.None)
                            , JsonConvert.SerializeObject(x.meals, Formatting.None)
                            , x.date, x.userId
                            , x.clientNote);
                } else {
                    sql = string.Format(@"UPDATE clientsdata SET  
                            height = '{0}', weight = '{1}', waist = '{2}', hip = '{3}', pal = '{4}', goal = '{5}', activities = '{6}', diet = '{7}', meals = '{8}', date = '{9}'
                            WHERE clientId = '{10}' AND ((strftime('%d', date) = '{11}' AND strftime('%m', date) = '{12}' AND strftime('%Y', date) = '{13}') OR date = '{9}');
                            UPDATE clients SET note = '{14}' WHERE clientId = '{10}';"
                            , x.height, x.weight, x.waist, x.hip, x.pal.value, x.goal.code
                            , JsonConvert.SerializeObject(x.activities, Formatting.None)
                            , JsonConvert.SerializeObject(x.diet, Formatting.None)
                            , JsonConvert.SerializeObject(x.meals, Formatting.None)
                            , x.date, x.clientId, Convert.ToDateTime(x.date).Day, (Convert.ToDateTime(x.date).Month < 10 ? string.Format("0{0}", Convert.ToDateTime(x.date).Month) : Convert.ToDateTime(x.date).Month.ToString())
                            , Convert.ToDateTime(x.date).Year
                            , x.clientNote);
                }
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteTransaction transaction = connection.BeginTransaction()) {
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }
                connection.Close();
            }
            if (userType > 1) {
                SaveMyMeals(userId, x.clientId, x.myMeals);
            }
            return "saved";
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Get(string userId, string clientId) {
        try {
            NewClientData x = new NewClientData();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                x = GetClientData(userId, clientId, connection);
                connection.Close();
            } 
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string GetClientLog(string userId, string clientId) {
        try {
            List<NewClientData> xx = new List<NewClientData>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"SELECT cd.rowid, cd.clientId, c.birthDate, c.gender, cd.height, cd.weight, cd.waist, cd.hip, cd.pal, cd.goal, cd.activities, cd.diet, cd.meals, cd.date, cd.userId
                        FROM clientsdata as cd
                        LEFT OUTER JOIN clients as c
                        ON cd.clientId = c.clientId
                        WHERE cd.clientId = '{0}'", clientId);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewClientData x = new NewClientData();
                            Calculations c = new Calculations();
                            Goals g = new Goals();
                            x.id = reader.GetInt32(0);
                            x.clientId = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.age = calculation.Age(reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2));
                            x.gender.value = reader.GetValue(3) == DBNull.Value ? 0 : reader.GetInt32(3);
                            x.gender.title = GetGender(x.gender.value).title;
                            x.height = reader.GetValue(4) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(4));
                            x.weight = reader.GetValue(5) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(5));
                            x.waist = reader.GetValue(6) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(6));
                            x.hip = reader.GetValue(7) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(7));
                            x.pal = c.GetPal(reader.GetValue(8) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(8)));
                            x.goal.code = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                            x.goal.title = g.GetGoal(x.goal.code).title;
                            x.activities = JsonConvert.DeserializeObject<List<Activities.ClientActivity>>(reader.GetString(10));
                            x.diet = JsonConvert.DeserializeObject<Diets.NewDiet>(reader.GetString(11));
                            x.meals = JsonConvert.DeserializeObject<List<Meals.NewMeal>>(reader.GetString(12));
                            x.date = reader.GetValue(13) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(13);
                            x.userId = reader.GetValue(14) == DBNull.Value ? "" : reader.GetString(14);
                            xx.Add(x);
                        }
                    } 
                } 
                connection.Close();
            }
            xx = xx.OrderByDescending(a => Convert.ToDateTime(a.date)).ToList();
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string UpdateClientLog(string userId, NewClientData clientData) {
        try {
            db.CreateDataBase(userId, db.clientsData);
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"UPDATE clientsdata SET clientId = '{0}', height = '{1}', weight ='{2}', waist = '{3}', hip = '{4}', date = '{5}'
                                           WHERE clientId = '{0}' AND rowid = '{6}'"
                                           , clientData.clientId, clientData.height, clientData.weight, clientData.waist, clientData.hip, clientData.date, clientData.id);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteTransaction transaction = connection.BeginTransaction()) {
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                } 
                connection.Close();
            }
            return "saved";
        } catch (Exception e) { return ("Error: " + e.Message); }
    }

    [WebMethod]
    public string Delete(string userId, NewClientData clientData) {
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format("delete from clientsdata where rowid='{0}' AND clientId='{1}'", clientData.id, clientData.clientId);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                } 
                connection.Close();
            }
            return "OK";
        } catch (Exception e) { return ("Error: " + e); }
    }

    #region ClientApp
    [WebMethod]
    public string SaveClientDataFromAndroid(string clientId, string height, string weight, string waist, string hip, string pal, string date, string userId) {
        try {
            NewClientData x = new NewClientData();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                x = GetClientData(userId, clientId, connection);
                connection.Close();
            }
            if (x.clientId == null) {
                x.pal = new Calculations.Pal();
                x.goal = new Goals.NewGoal();
                x.activities = new List<Activities.ClientActivity>();
                x.diet = new Diets.NewDiet();
                x.meals = new List<Meals.NewMeal>();
                x.date = g.FormatDate(DateTime.UtcNow);
                x.dailyActivities = new DetailEnergyExpenditure.Activities();
                x.myMeals = new MyMeals.NewMyMeals();
            } else {
                x.clientId = clientId;
                x.height = Convert.ToDouble(height);
                x.weight = Convert.ToDouble(weight);
                x.waist = Convert.ToDouble(waist);
                x.hip = Convert.ToDouble(hip);
                x.pal.value = Convert.ToDouble(pal);
                x.date = g.FormatDate(Convert.ToDateTime(date));
                x.userId = userId;
            }
            return Save(userId, x, 0);
        } catch (Exception e) {
            return ("Error: " + e.Message);
        }
    }
    #endregion
    #endregion Web Methods

    #region Methods
    private bool Check(string userId, NewClientData x){
        try {
            int count = 0;
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase))) {
                connection.Open();
                string sql = string.Format(@"SELECT COUNT([rowid]) FROM clientsdata 
                    WHERE clientId = '{0}' AND ((strftime('%d', date) = '{1}' AND strftime('%m', date) = '{2}' AND strftime('%Y', date) = '{3}') OR date = '{4}')"
                            , x.clientId, Convert.ToDateTime(x.date).Day, (Convert.ToDateTime(x.date).Month < 10 ? string.Format("0{0}", Convert.ToDateTime(x.date).Month): Convert.ToDateTime(x.date).Month.ToString()), Convert.ToDateTime(x.date).Year, g.FormatDate(Convert.ToDateTime(x.date)));
                using (SQLiteCommand command = new SQLiteCommand(sql , connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            count = reader.GetInt32(0);
                        }
                    } 
                }  
                connection.Close();
            } 
            if (count == 0) { return true; }
            else { return false; }
        } catch (Exception e) { return false; }
    }

    public Clients.Gender GetGender(int value) {
        Clients.Gender x = new Clients.Gender();
        x.value = value;
        x.title = value == 0 ? "male" : "female";
        return x;
    }

     public NewClientData GetClientData(string userId, string clientId, SQLiteConnection connection) {
         try {
            List<NewClientData> xx = new List<NewClientData>();
            NewClientData x = new NewClientData();
            db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.clients, "note");  //new column in clients tbl.
            string sql = string.Format(@"SELECT cd.rowid, cd.clientId, c.birthDate, c.gender, cd.height, cd.weight, cd.waist, cd.hip, cd.pal, cd.goal, cd.activities, cd.diet, cd.meals, cd.date, cd.userId, c.note
                        FROM clientsdata as cd
                        LEFT OUTER JOIN clients as c
                        ON cd.clientId = c.clientId
                        WHERE cd.clientId = '{0}'", clientId);
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                Calculations c = new Calculations();
                Goals g = new Goals();
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = new NewClientData();
                        x.id = reader.GetInt32(0);
                        x.clientId = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                        x.age = calculation.Age(reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2));
                        x.gender.value = reader.GetValue(3) == DBNull.Value ? 0 : reader.GetInt32(3);
                        x.gender.title = GetGender(x.gender.value).title;
                        x.height = reader.GetValue(4) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(4));
                        x.weight = reader.GetValue(5) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(5));
                        x.waist = reader.GetValue(6) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(6));
                        x.hip = reader.GetValue(7) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(7));
                        x.pal = c.GetPal(reader.GetValue(8) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(8)));
                        x.goal.code = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                        x.goal.title = g.GetGoal(x.goal.code).title;
                        x.activities = JsonConvert.DeserializeObject<List<Activities.ClientActivity>>(reader.GetString(10));
                        x.diet = JsonConvert.DeserializeObject<Diets.NewDiet>(reader.GetString(11));
                        x.meals = JsonConvert.DeserializeObject<List<Meals.NewMeal>>(reader.GetString(12));
                        x.date = reader.GetValue(13) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(13);
                        x.userId = reader.GetValue(14) == DBNull.Value ? "" : reader.GetString(14);
                        x.clientNote = reader.GetValue(15) == DBNull.Value ? "" : reader.GetString(15);
                        DetailEnergyExpenditure.DailyActivities da = new DetailEnergyExpenditure.DailyActivities();
                        x.dailyActivities = da.getDailyActivities(userId, x.clientId);
                        x.myMeals = GetMyMeals(userId, x.clientId);
                        x.bmrEquation = E.MifflinStJeor; // TODO GetBmrEquation() & SaveBMREquation()
                        xx.Add(x);
                    }
                }
            }
            if (xx.Count > 0) {
                x = xx.OrderByDescending(a => Convert.ToDateTime(a.date)).FirstOrDefault();
            }
            return x;
        } catch (Exception e) { return new NewClientData(); }
    }

    private void SaveMyMeals(string userId, string clientId, MyMeals.NewMyMeals myMeals) {
        try {
            if (myMeals.data != null) {
                string path = string.Format("~/App_Data/users/{0}/clients/{1}", userId, clientId);
                string filepath = string.Format("{0}/myMeals.json", path);
                CreateFolder(path);
                WriteFile(filepath, JsonConvert.SerializeObject(myMeals, Formatting.None));
            }
        }
        catch (Exception e) {}
    }

    protected void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    protected void WriteFile(string path, string value) {
        File.WriteAllText(Server.MapPath(path), value);
    }

    private MyMeals.NewMyMeals GetMyMeals (string userId, string clientId) {
        MyMeals.NewMyMeals x = new MyMeals.NewMyMeals();
        x = JsonConvert.DeserializeObject<MyMeals.NewMyMeals>(GetJsonFile(userId, clientId));
        if(x == null) {
            x = new MyMeals.NewMyMeals();
        }
        return x;
    }

    public string GetJsonFile(string userId, string clientId) {
        string path = string.Format("~/App_Data/users/{0}/clients/{1}/myMeals.json", userId, clientId);
        string json = "";
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }
    #endregion

}
