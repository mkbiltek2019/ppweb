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

    public WeeklyMenus() {
    }

    public class NewWeeklyMenus {
        public string id;
        public string title;
        public string note;
        public string diet;
        public List<string> menuList;
        public DateTime date;
        public string clientId;
        public string userId;
        public string userGroupId;
    }

    [WebMethod]
    public string Init(Users.NewUser user) {
        NewWeeklyMenus x = new NewWeeklyMenus();
        x.id = null;
        x.title = null;
        x.note = null;
        x.diet = null;
        x.menuList = new List<string>();
        x.date = DateTime.UtcNow;
        x.clientId = null;
        x.userId = user.userId;
        x.userGroupId = user.userGroupId;
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

     [WebMethod]
    public string Load(string userId) {
        try {
            return JsonConvert.SerializeObject(LoadWeeklyMenus(userId), Formatting.Indented);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = string.Format("SELECT id, title, note, diet, menuList, date, clientId, userId, userGroupId FROM weeklyMenus WHERE id = '{0}'", id);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            NewWeeklyMenus x = new NewWeeklyMenus();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.note = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.diet = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.menuList = reader.GetValue(4) == DBNull.Value ? new List<string>() : reader.GetString(4).Split(',').ToList();
                x.date = reader.GetValue(5) == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(reader.GetString(5));
                x.clientId = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                x.userId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                x.userGroupId = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
                
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
                    INSERT OR REPLACE INTO weeklymenus (id, title, note, diet, menuList, date, clientId, userId, userGroupId)
                    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}');
                    COMMIT;", x.id, x.title, x.note, x.diet, string.Join(",",x.menuList), x.date, x.clientId, x.userId, x.userGroupId);
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return JsonConvert.SerializeObject(x, Formatting.Indented);
            }
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Delete(string userId, string id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = string.Format("DELETE FROM weeklymenus WHERE id = '{0}'", id);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
            connection.Close();
            List<NewWeeklyMenus> xx = LoadWeeklyMenus(userId);
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) { return (e.Message); }
    }

     public List<NewWeeklyMenus> LoadWeeklyMenus(string userId) {
        db.CreateDataBase(userId, db.weeklymenus);
        SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
        connection.Open();
        string sql = "SELECT id, title, note, diet, menuList, date, clientId, userId, userGroupId FROM weeklymenus ORDER BY rowid DESC";
        SQLiteCommand command = new SQLiteCommand(sql, connection);
        List<NewWeeklyMenus> xx = new List<NewWeeklyMenus>();
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read()) {
            NewWeeklyMenus x = new NewWeeklyMenus();
            x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
            x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
            x.note = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
            x.diet = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
            x.menuList = reader.GetValue(4) == DBNull.Value ? new List<string>() : reader.GetString(4).Split(',').ToList();
            x.date = reader.GetValue(5) == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(reader.GetString(5));
            x.clientId = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
            x.userId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
            x.userGroupId = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
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
