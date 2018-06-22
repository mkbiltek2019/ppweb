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
/// Recipes
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Recipes : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    string appDataBase = ConfigurationManager.AppSettings["AppDataBase"];
    DataBase db = new DataBase();

    public Recipes() {
    }

    #region Class

    public class NewRecipe {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public double energy { get; set; }
        public JsonFile data = new JsonFile();
    }

    public class JsonFile {
        public List<Foods.NewFood> selectedFoods { get; set; }
        public List<Foods.NewFood> selectedInitFoods { get; set; }

    }
    #endregion Class

    #region WebMethods

    #region ClientRecipes
    [WebMethod]
    public string Init() {
        NewRecipe x = new NewRecipe();
        x.id = null;
        x.title = null;
        x.description = null;
        x.energy = 0;
        JsonFile data = new JsonFile();
        data.selectedFoods = new List<Foods.NewFood>();
        data.selectedInitFoods = new List<Foods.NewFood>();
        x.data = data;
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Load(string userId) {
        try {
            db.CreateDataBase(userId, db.recipes);
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"SELECT id, title, description, energy
                        FROM recipes
                        ORDER BY rowid DESC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewRecipe> xx = new List<NewRecipe>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewRecipe x = new NewRecipe();
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.energy = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                xx.Add(x);
            }
            connection.Close();
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"SELECT id, title, description, energy
                        FROM recipes
                        WHERE id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", id));
            NewRecipe x = new NewRecipe();
            Clients.Client client = new Clients.Client();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.energy = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                x.data = JsonConvert.DeserializeObject<JsonFile>(GetJsonFile(userId, x.id));
            }
            connection.Close();
            return JsonConvert.SerializeObject(x, Formatting.Indented);
        }
        catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Save(string userId, NewRecipe x) {
        db.CreateDataBase(userId, db.recipes);
            try {
                string sql = "";
                if (x.id == null) {
                    x.id = Convert.ToString(Guid.NewGuid());
                }
                x.energy = x.data.selectedFoods.Sum(a => a.energy);
                SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                sql = @"BEGIN;
                    INSERT OR REPLACE INTO recipes (id, title, description, energy)
                    VALUES (@id, @title, @description, @energy);
                    COMMIT;";
                command = new SQLiteCommand(sql, connection);
                command.Parameters.Add(new SQLiteParameter("id", x.id));
                command.Parameters.Add(new SQLiteParameter("title", x.title));
                command.Parameters.Add(new SQLiteParameter("description", x.description));
                command.Parameters.Add(new SQLiteParameter("energy", x.energy));
                command.ExecuteNonQuery();
                connection.Close();
                SaveJsonToFile(userId, x.id, JsonConvert.SerializeObject(x.data, Formatting.Indented));

                return JsonConvert.SerializeObject(x, Formatting.Indented);
            } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Delete(string userId, string id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = "delete from recipes where id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", id));
            command.ExecuteNonQuery();
            connection.Close();
            DeleteJson(userId, id);
        } catch (Exception e) { return (e.Message); }
        return "OK";
    }
    #endregion ClientRecipes

    #region AppRecipes
    [WebMethod]
    public string LoadAppRecipes(string lang) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath(string.Format("~/App_Data/{0}", appDataBase)));
            connection.Open();
            string sql = string.Format(@"SELECT id, title, description, energy FROM recipes
                        WHERE language = '{0}'
                        ORDER BY rowid DESC", lang);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewRecipe> xx = new List<NewRecipe>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewRecipe x = new NewRecipe();
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.energy = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                xx.Add(x);
            }
            connection.Close();
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string GetAppRecipe(string id, string lang) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath(string.Format("~/App_Data/{0}", appDataBase)));
            connection.Open();
            string sql = string.Format(@"SELECT id, title, description, energy FROM recipes WHERE id = '{0}'", id);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            NewRecipe x = new NewRecipe();
            Clients.Client client = new Clients.Client();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.energy = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                x.data = JsonConvert.DeserializeObject<JsonFile>(GetAppJsonFile(x.id, lang));
            }
            connection.Close();
            return JsonConvert.SerializeObject(x, Formatting.Indented);
        }
        catch (Exception e) { return (e.Message); }
    }

    #endregion AppRecipes

    #endregion WebMethods

    #region Methods
    public void SaveJsonToFile(string userId, string filename, string json) {
        string path = string.Format("~/App_Data/users/{0}/recipes", userId);
        string filepath = string.Format("{0}/{1}.json", path, filename);
            CreateFolder(path);
            WriteFile(filepath, json);
    }

    protected void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    protected void WriteFile(string path, string value) {
        File.WriteAllText(Server.MapPath(path), value);
    }

    public void DeleteJson(string userId, string filename) {
        string path = Server.MapPath(string.Format("~/App_Data/users/{0}/recipes", userId));
        string filepath = string.Format("{0}/{1}.json", path, filename);
        if (File.Exists(filepath)) {
            File.Delete(filepath);
        }
    }
    private string GetJsonFile(string userId, string filename) {
        string path = string.Format("~/App_Data/users/{0}/recipes/{1}.json", userId, filename);
        string json = "";
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }

    private string GetAppJsonFile(string filename, string lang) {
        string path = string.Format("~/App_Data/recipes/{0}/{1}.json", lang, filename);
        string json = "";
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }
    #endregion Methods


    


}
