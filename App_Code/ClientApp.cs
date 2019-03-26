using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// ClientApp
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class ClientApp : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UsersDataBase"];
    DataBase db = new DataBase();

    public ClientApp() {
    }

    public class NewClientApp {
        public string id;
        public string clientId;
        public string userId;
        public string code;
        public string lang;
    }

    #region Web Methods
    [WebMethod]
    public string Get(string clientId) {
        try {
            return JsonConvert.SerializeObject(GetCode(clientId), Formatting.None);
        } catch (Exception e) { return null; }
    }

    [WebMethod]
    public string GetActivationCode(NewClientApp x) {
        try {
            if (string.IsNullOrEmpty(x.code)) {
                string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
                db.CreateGlobalDataBase(path, db.clientapp);
                x.code = CreateActivationCode();
                if(x.code == null) { return null; }
                if(string.IsNullOrEmpty(x.id)) {
                    x.id = Guid.NewGuid().ToString();
                }
                SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", path));
                connection.Open();
                string sql = string.Format(@"BEGIN;
                    INSERT OR REPLACE INTO clientapp (id, clientId, userId, code, lang)
                    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}');
                    COMMIT;", x.id, x.clientId, x.userId, x.code, x.lang);
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return null;
        }
    }

    [WebMethod]
    public string Activate(string code) {
        try {
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.clientapp);
            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", path));
            connection.Open();
            string sql = string.Format(@"SELECT id, clientId, userId, code, lang FROM clientapp WHERE code = '{0}'", code);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            NewClientApp x = new NewClientApp();
            while (reader.Read()) {
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.clientId = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.userId = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.code = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.lang = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
            }
            connection.Close();
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return null; }
    }
    #endregion Web Methods

    #region Methods
    private NewClientApp GetCode(string clientId) {
        try {
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.clientapp);
            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", path));
            connection.Open();
            string sql = string.Format(@"SELECT id, clientId, userId, code, lang FROM clientapp WHERE clientId = '{0}'", clientId);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            NewClientApp x = new NewClientApp();
            while (reader.Read()) {
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.clientId = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.userId = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.code = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.lang = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
            }
            connection.Close();
            return x;
        } catch (Exception e) { return new NewClientApp(); }
    }
    
    private string CreateActivationCode() {
        Random r = new Random();
        string code = null;
        for (int i=0; i<20; i++) {
            code = r.Next(10000, 100000).ToString();
            if(Check(code)) { break; }
            code = null;
        }
        if(code == null) { code = r.Next(100000, 1000000).ToString(); }
        return code;
    }

    private bool Check(string code) {
        try {
            int count = 0;
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", path));
            connection.Open();
            string sql = string.Format("SELECT COUNT([rowid]) FROM clientapp WHERE code = '{0}'", code);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                count = reader.GetInt32(0);
            }
            connection.Close();
            if (count == 0) { return true; }
            else { return false; }
        } catch (Exception e) { return false; }
    }

    public int GetClientAppUsers() {
        try {
            int count = 0;
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", path));
            connection.Open();
            string sql = "SELECT COUNT([rowid]) FROM clientapp";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                count = reader.GetInt32(0);
            }
            connection.Close();
            return count;
        } catch (Exception e) {
            return 0;
        }
    }
    #endregion Methods
}
