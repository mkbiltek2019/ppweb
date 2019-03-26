using System;
using System.Collections.Generic;
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
        public DateTime date { get; set; }
        public string userId { get; set; }

        public DetailEnergyExpenditure.Activities dailyActivities = new DetailEnergyExpenditure.Activities();

        public MyMeals.NewMyMeals myMeals = new MyMeals.NewMyMeals();

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
        x.date = DateTime.UtcNow;
        x.userId = null;
        x.dailyActivities = new DetailEnergyExpenditure.Activities();
        x.myMeals = new MyMeals.NewMyMeals();
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
                x.date = reader.GetValue(13) == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(reader.GetString(13));
                x.userId = reader.GetValue(14) == DBNull.Value ? "" : reader.GetString(14);
                DetailEnergyExpenditure.DailyActivities da = new DetailEnergyExpenditure.DailyActivities();
                x.dailyActivities = da.getDailyActivities(userId, x.clientId);
                x.myMeals = new MyMeals.NewMyMeals();
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.None);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(string userId, NewClientData x, int userType) {
        try {
            db.CreateDataBase(userId, db.clientsData);
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = "";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            if (Check(userId, x) != false) {
                sql = @"INSERT INTO clientsdata (clientId, height, weight, waist, hip, pal, goal, activities, diet, meals, date, userId)
                        VALUES (@clientId, @height, @weight, @waist, @hip, @pal, @goal, @activities, @diet, @meals, @date, @userId)";
                command = new SQLiteCommand(sql, connection);
            } else {
                sql = @"UPDATE clientsdata SET  
                          clientId = @clientId, height = @height, weight = @weight, waist = @waist, hip = @hip, pal = @pal, goal = @goal, activities = @activities, diet = @diet, meals = @meals, date = @date, userId = @userId
                            WHERE clientId = @clientId AND date = @date";
                command = new SQLiteCommand(sql, connection);
            }
            command.Parameters.Add(new SQLiteParameter("clientId", x.clientId));
            command.Parameters.Add(new SQLiteParameter("height", x.height));
            command.Parameters.Add(new SQLiteParameter("weight", x.weight));
            command.Parameters.Add(new SQLiteParameter("waist", x.waist));
            command.Parameters.Add(new SQLiteParameter("hip", x.hip));
            command.Parameters.Add(new SQLiteParameter("pal", x.pal.value));
            command.Parameters.Add(new SQLiteParameter("goal", x.goal.code));
            command.Parameters.Add(new SQLiteParameter("activities", JsonConvert.SerializeObject(x.activities, Formatting.None)));
            command.Parameters.Add(new SQLiteParameter("diet", JsonConvert.SerializeObject(x.diet, Formatting.None)));
            command.Parameters.Add(new SQLiteParameter("meals", JsonConvert.SerializeObject(x.meals, Formatting.None)));
            command.Parameters.Add(new SQLiteParameter("date", x.date));
            command.Parameters.Add(new SQLiteParameter("userId", x.userId));
            command.ExecuteNonQuery();
            connection.Close();
            if (userType > 1) {
                SaveMyMeals(userId, x.clientId, x.myMeals);
            }
            return "saved";
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Get(string userId, string clientId) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            NewClientData x = new NewClientData();
            x = GetClientData(userId, clientId, connection);
            connection.Close();
            string json = JsonConvert.SerializeObject(x, Formatting.None);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string GetClientLog(string userId, string clientId) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"SELECT cd.rowid, cd.clientId, c.birthDate, c.gender, cd.height, cd.weight, cd.waist, cd.hip, cd.pal, cd.goal, cd.activities, cd.diet, cd.meals, cd.date, cd.userId
                        FROM clientsdata as cd
                        LEFT OUTER JOIN clients as c
                        ON cd.clientId = c.clientId
                        WHERE cd.clientId = @clientId
                        ORDER BY cd.date ASC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("clientId", clientId));
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
                x.date = reader.GetValue(13) == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(reader.GetString(13));
                x.userId = reader.GetValue(14) == DBNull.Value ? "" : reader.GetString(14);
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.None);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string UpdateClientLog(string userId, NewClientData clientData) {
        try {
            db.CreateDataBase(userId, db.clientsData);
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"UPDATE clientsdata SET  
                        clientId = @clientId, height = @height, weight = @weight, waist = @waist, hip = @hip, date = @date
                        WHERE clientId = @clientId AND rowid = @id";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("clientId", clientData.clientId));
            command.Parameters.Add(new SQLiteParameter("height", clientData.height));
            command.Parameters.Add(new SQLiteParameter("weight", clientData.weight));
            command.Parameters.Add(new SQLiteParameter("waist", clientData.waist));
            command.Parameters.Add(new SQLiteParameter("hip", clientData.hip));
            command.Parameters.Add(new SQLiteParameter("date", clientData.date));
            command.Parameters.Add(new SQLiteParameter("id", clientData.id));
            command.ExecuteNonQuery();
            connection.Close();
            return "saved";
        } catch (Exception e) { return ("Error: " + e.Message); }
    }

    [WebMethod]
    public string Delete(string userId, NewClientData clientData) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = "delete from clientsdata where rowid=@rowid AND clientId=@clientId ";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("rowid", clientData.id));
            command.Parameters.Add(new SQLiteParameter("clientId", clientData.clientId));
            command.ExecuteNonQuery();
            connection.Close();
        } catch (Exception e) { return ("Error: " + e); }
        return "OK";
    }

    #region ClientApp
    [WebMethod]
    public string SaveClientDataFromAndroid(string clientId, string height, string weight, string waist, string hip, string pal, string date, string userId) {
        try {
            NewClientData x = new NewClientData();
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            x = GetClientData(userId, clientId, connection);
            connection.Close();
            if (x.clientId == null) {
                x.pal = new Calculations.Pal();
                x.goal = new Goals.NewGoal();
                x.activities = new List<Activities.ClientActivity>();
                x.diet = new Diets.NewDiet();
                x.meals = new List<Meals.NewMeal>();
                x.date = DateTime.UtcNow;
                x.dailyActivities = new DetailEnergyExpenditure.Activities();
                x.myMeals = new MyMeals.NewMyMeals();
            } else {
                x.clientId = clientId;
                x.height = Convert.ToDouble(height);
                x.weight = Convert.ToDouble(weight);
                x.waist = Convert.ToDouble(waist);
                x.hip = Convert.ToDouble(hip);
                x.pal.value = Convert.ToDouble(pal);
                x.date = Convert.ToDateTime(date);
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
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(
                "SELECT COUNT([rowid]) FROM clientsdata WHERE clientId = @clientId AND date = @date", connection);
            command.Parameters.Add(new SQLiteParameter("clientId", x.clientId));
            command.Parameters.Add(new SQLiteParameter("date", x.date));
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                count = reader.GetInt32(0);
            }
            connection.Close();
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
            string sql = @"SELECT cd.rowid, cd.clientId, c.birthDate, c.gender, cd.height, cd.weight, cd.waist, cd.hip, cd.pal, cd.goal, cd.activities, cd.diet, cd.meals, cd.date, cd.userId
                        FROM clientsdata as cd
                        LEFT OUTER JOIN clients as c
                        ON cd.clientId = c.clientId
                        WHERE cd.clientId = @clientId
                        ORDER BY cd.date DESC LIMIT 1";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("clientId", clientId));
            NewClientData x = new NewClientData();
            Calculations c = new Calculations();
            Goals g = new Goals();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
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
                x.date = reader.GetValue(13) == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(reader.GetString(13));
                x.userId = reader.GetValue(14) == DBNull.Value ? "" : reader.GetString(14);
                DetailEnergyExpenditure.DailyActivities da = new DetailEnergyExpenditure.DailyActivities();
                x.dailyActivities = da.getDailyActivities(userId, x.clientId);
                x.myMeals = GetMyMeals(userId, x.clientId);
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
