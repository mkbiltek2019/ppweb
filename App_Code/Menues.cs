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
        JsonFile data = new JsonFile();
        data.selectedFoods = new List<Foods.NewFood>();
        data.selectedInitFoods = new List<Foods.NewFood>();
        data.meals = new List<Meals.NewMeal>();
        x.data = data;

        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Load(string userId) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();

            string sql = @"SELECT id, title, diet, date, note, userId, clientId
                        FROM menues
                        ORDER BY date DESC";
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
                x.data = JsonConvert.DeserializeObject<JsonFile>(GetJsonFile(userId, x.id));
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(string userId, NewMenu x) {
        db.CreateDataBase(userId, db.menues);
        if (x.id == null && Check(userId, x) != false) {
            return ("there is already a menu with the same name");
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
                    INSERT OR REPLACE INTO menues (id, title, diet, date, note, userId, clientId)
                    VALUES (@id, @title, @diet, @date, @note, @userId, @clientId);
                    COMMIT;";
                command = new SQLiteCommand(sql, connection);

                command.Parameters.Add(new SQLiteParameter("id", x.id));
                command.Parameters.Add(new SQLiteParameter("title", x.title));
                command.Parameters.Add(new SQLiteParameter("diet", x.diet));
                command.Parameters.Add(new SQLiteParameter("date", x.date));
                command.Parameters.Add(new SQLiteParameter("note", x.note));
                command.Parameters.Add(new SQLiteParameter("userId", x.userId));
                command.Parameters.Add(new SQLiteParameter("clientId", x.client.clientId));
                command.ExecuteNonQuery();
                connection.Close();
                SaveJsonToFile(userId, x.id, JsonConvert.SerializeObject(x.data, Formatting.Indented));

                return "saved";
            } catch (Exception e) { return ("Error: " + e); }
        }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"SELECT id, title, diet, date, note, userId
                        FROM menues
                        WHERE id = id";
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
                x.data = JsonConvert.DeserializeObject<JsonFile>(GetJsonFile(userId, x.id));
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Delete(string userId, int id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = "delete from menues where id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", id));
            command.ExecuteNonQuery();
            connection.Close();
        } catch (Exception e) { return ("Error: " + e); }
        return "OK";
    }
    #endregion

    #region Methods
    public void SaveJsonToFile(string userId, string filename, string json) {
       // try {
            string path = "~/App_Data/users/" + userId + "/menues";
            string filepath = path + "/" + filename + ".json";
            CreateFolder(path);
            WriteFile(filepath, json);
        //    return "OK";
        //} catch (Exception e) { return ("Error: " + e); }
    }

    private string GetJsonFile(string userId, string filename) {
        string path = "~/App_Data/users/" + userId + "/menues/" + filename + ".json" ;
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
