using System;
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
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

     [WebMethod]
    public string Load(string userId, string lang) {
        try {
            return JsonConvert.SerializeObject(LoadWeeklyMenus(userId, lang), Formatting.Indented);
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
            return JsonConvert.SerializeObject(x, Formatting.Indented);
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
                return JsonConvert.SerializeObject(x, Formatting.Indented);
            }
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
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
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
