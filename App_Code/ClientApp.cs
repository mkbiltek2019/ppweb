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
[WebService(Namespace = "http://tempuri.org/")]
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
        // userId, userGroupId TODO
        public string code;
        public string lang;
    }

    #region Web Methods
    [WebMethod]
    public string Init() {
        NewClientApp x = new NewClientApp();
        x.id = null;
        x.clientId = null;
        x.userId = null;
        x.code = null;
        x.lang = null;
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string Get(string clientId) {
        try {
            return JsonConvert.SerializeObject(GetCode(clientId), Formatting.Indented);
        } catch (Exception e) { return null; }
    }

    [WebMethod]
    public string GetActivationCode(NewClientApp x) {
        try {
            x.code = GetCode(x.clientId).code;
            if (string.IsNullOrEmpty(x.code)) {
                x.code = CreateActivationCode();
                if(string.IsNullOrEmpty(x.id)) {
                    x.id = Guid.NewGuid().ToString();
                }
                string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
                db.CreateGlobalDataBase(path, db.clientapp);
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
            return JsonConvert.SerializeObject(x, Formatting.Indented);
        } catch (Exception e) {
            return null;
        }
    }

    [WebMethod]
    public string Activate(NewClientApp client) {
        try {
            string path = Server.MapPath(string.Format("~/App_Data/{0}", dataBase));
            db.CreateGlobalDataBase(path, db.clientapp);
            SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0}", path));
            connection.Open();
            string sql = string.Format(@"SELECT id, clientId, userId, code, lang FROM clientapp WHERE code = '{0}'", client.code);
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
            return JsonConvert.SerializeObject(x, Formatting.Indented);
        } catch (Exception e) { return (e.Message); }
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
        return r.Next(10000, 99999).ToString();
        //CheckIfExists(code)  get all from db and check with linq and loof for 10 times and break when code not exists, test with code to 0-5
    }
    #endregion Methods

}
