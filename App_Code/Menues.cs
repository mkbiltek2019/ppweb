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
/// Menues
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Menues : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    string appDataBase = ConfigurationManager.AppSettings["AppDataBase"];
    DataBase db = new DataBase();

    public Menues() {
    }

    public class NewMenu {
        public string id { get; set; }
        public string title { get; set; }
        public string diet { get; set; }
        public DateTime date { get; set; }
        public string note { get; set; }
        public string userId { get; set; }

        public Clients.NewClient client = new Clients.NewClient();
        public string userGroupId { get; set; }
        public double energy { get; set; }

        public JsonFile data = new JsonFile();
    }

    public class JsonFile {
        public List<Foods.NewFood> selectedFoods { get; set; }
        public List<Foods.NewFood> selectedInitFoods { get; set; }
        public List<Meals.NewMeal> meals { get; set; }

    }

    #region WebMethods
    [WebMethod]
    public string Init() {
        NewMenu x = new NewMenu();
        x.id = null;
        x.title = "";
        x.diet = "";
        x.date = DateTime.UtcNow;
        x.note = "";
        x.userId = null;
        x.client =  new Clients.NewClient();
        x.userGroupId = null;
        x.energy = 0;
        JsonFile data = new JsonFile();
        data.selectedFoods = new List<Foods.NewFood>();
        data.selectedInitFoods = new List<Foods.NewFood>();
        data.meals = new List<Meals.NewMeal>();
        x.data = data;

        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    #region ClientMenues
    [WebMethod]
    public string Load(string userId) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"SELECT id, title, diet, date, note, userId, clientId, userGroupId, energy
                        FROM menues
                        ORDER BY rowid DESC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewMenu> xx = new List<NewMenu>();
            Clients.Client client = new Clients.Client();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewMenu x = new NewMenu();
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.diet = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.date = reader.GetValue(3) == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(reader.GetString(3));
                x.note = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.userId = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                x.client = reader.GetValue(6) == DBNull.Value ? new Clients.NewClient() : client.GetClient(x.userId, reader.GetString(6));
                x.userGroupId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                x.energy = reader.GetValue(8) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(8));
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"SELECT id, title, diet, date, note, userId, clientId, userGroupId, energy
                        FROM menues
                        WHERE id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", id));
            NewMenu x = new NewMenu();
            Clients.Client client = new Clients.Client();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.diet = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.date = reader.GetValue(3) == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(reader.GetString(3));
                x.note = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.userId = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                x.client = reader.GetValue(6) == DBNull.Value ? new Clients.NewClient() : client.GetClient(x.userId, reader.GetString(6));
                x.userGroupId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                x.energy = reader.GetValue(8) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(8));
                x.data = JsonConvert.DeserializeObject<JsonFile>(GetJsonFile(userId, x.id));
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Save(string userId, NewMenu x) {
        db.CreateDataBase(userId, db.menues);
        if (x.id == null && Check(userId, x) != false) {
            return "error";
        } else {
            try {
                string sql = "";
                if (x.id == null) {
                    x.id = Convert.ToString(Guid.NewGuid());
                }
                SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                sql = @"BEGIN;
                    INSERT OR REPLACE INTO menues (id, title, diet, date, note, userId, clientId, userGroupId, energy)
                    VALUES (@id, @title, @diet, @date, @note, @userId, @clientId, @userGroupId, @energy);
                    COMMIT;";
                command = new SQLiteCommand(sql, connection);

                command.Parameters.Add(new SQLiteParameter("id", x.id));
                command.Parameters.Add(new SQLiteParameter("title", x.title));
                command.Parameters.Add(new SQLiteParameter("diet", x.diet));
                command.Parameters.Add(new SQLiteParameter("date", x.date));
                command.Parameters.Add(new SQLiteParameter("note", x.note));
                command.Parameters.Add(new SQLiteParameter("userId", userId));
                command.Parameters.Add(new SQLiteParameter("clientId", x.client.clientId));
                command.Parameters.Add(new SQLiteParameter("userGroupId", x.userGroupId));
                command.Parameters.Add(new SQLiteParameter("energy", x.energy));
                command.ExecuteNonQuery();
                connection.Close();
                SaveJsonToFile(userId, x.id, JsonConvert.SerializeObject(x.data, Formatting.Indented));

                string json = JsonConvert.SerializeObject(x, Formatting.Indented);
                return json;
            } catch (Exception e) { return (e.Message); }
        }
    }

    [WebMethod]
    public string Delete(string userId, string id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = "delete from menues where id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", id));
            command.ExecuteNonQuery();
            connection.Close();
            DeleteJson(userId, id);
        } catch (Exception e) { return (e.Message); }
        return "OK";
    }
    #endregion ClientMenues


    #region AppMenues

    #endregion AppMenues
    [WebMethod]
    public string LoadAppMenues(string lang) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath(string.Format("~/App_Data/{0}", appDataBase)));
            connection.Open();
            string sql = string.Format(@"SELECT id, title, diet, note, energy
                        FROM menues WHERE language = '{0}'
                        ORDER BY rowid ASC", lang);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewMenu> xx = new List<NewMenu>();
            Clients.Client client = new Clients.Client();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewMenu x = new NewMenu();
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.diet = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.note = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.energy = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string GetAppMenu(string id, string lang) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath(string.Format("~/App_Data/{0}", appDataBase)));
            connection.Open();
            string sql = @"SELECT id, title, diet, note, energy
                        FROM menues
                        WHERE id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", id));
            NewMenu x = new NewMenu();
            Clients.Client client = new Clients.Client();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.diet = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.note = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.energy = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
                x.data = JsonConvert.DeserializeObject<JsonFile>(GetJsonFile(string.Format("~/App_Data/menues/{0}/{1}.json", lang, x.id)));
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string SaveAppMenu(NewMenu x, string lang) {
            try {
            string id = Convert.ToString(Guid.NewGuid());
            string sql = string.Format(@"BEGIN;
                    INSERT OR REPLACE INTO menues (id, title, diet, note, energy, language)
                    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');
                    COMMIT;", id, x.title, x.diet, x.note, x.energy, lang);
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath(string.Format("~/App_Data/{0}", appDataBase)));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
            connection.Close();
            SaveAppMenuJsonToFile(id, lang, JsonConvert.SerializeObject(x.data, Formatting.Indented));
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        } catch (Exception e) { return (e.Message); }
    }
    #endregion


    #region Methods
    public void SaveAppMenuJsonToFile(string id, string lang, string json) {
        string path = string.Format(@"~/App_Data/menues/{0}", lang);
        string filepath = string.Format(@"~/App_Data/menues/{0}/{1}.json", lang, id);
        CreateFolder(path);
        WriteFile(filepath, json);
    }
    public void SaveJsonToFile(string userId, string filename, string json) {
            string path = "~/App_Data/users/" + userId + "/menues";
            string filepath = path + "/" + filename + ".json";
            CreateFolder(path);
            WriteFile(filepath, json);
    }

    public void DeleteJson(string userId, string filename) {
        string path = Server.MapPath("~/App_Data/users/" + userId + "/menues");
        string filepath = path + "/" + filename + ".json";
        if (File.Exists(filepath)) {
            File.Delete(filepath);
        }
    }

    private string GetJsonFile(string userId, string filename) {
        string path = "~/App_Data/users/" + userId + "/menues/" + filename + ".json" ;
        string json = "";
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }

    private string GetJsonFile(string path) {
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

    private bool Check(string userId, NewMenu x) {
        try {
            bool result = false;
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(
                "SELECT EXISTS (SELECT id FROM menues WHERE title = @title AND clientId = @clientId)", connection);
            command.Parameters.Add(new SQLiteParameter("title", x.title));
            command.Parameters.Add(new SQLiteParameter("clientId", x.client.clientId));
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                result = reader.GetBoolean(0);
            }
            connection.Close();
            return result;
        } catch (Exception e) { return false; }
    }

    #endregion




}
