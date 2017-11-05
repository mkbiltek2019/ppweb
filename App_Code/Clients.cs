using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Clients
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Clients : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
   // public string dataBase = "data.ddb";

    DataBase db = new DataBase();
    Users.CheckUser c = new Users.CheckUser();
    public Clients() {
    }
    public class NewClient {
        public string clientId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string birthDate { get; set; }
        //public int? gender { get; set; }

        public Gender gender = new Gender();

        public string phone { get; set; }
        public string email { get; set; }
        public string userId { get; set; }
        public string date { get; set; }
        public int isActive { get; set; }

        public ClientsData.NewClientData clientData = new ClientsData.NewClientData();
    }

    public class Gender {
        public int value { get; set; }
        public string title { get; set; }

    }


    public class Client {

          public NewClient GetClient(string userId, string clientId) {
            DataBase db = new DataBase();
            string dataBase = ConfigurationManager.AppSettings["UserDataBase"];

            try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT clientId, firstName, lastName, birthDate, gender, phone, email, userId, date, isActive FROM clients WHERE clientId = @clientId", connection);
            command.Parameters.Add(new SQLiteParameter("clientId", clientId));
            NewClient x = new NewClient();
                Gender g = new Gender();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.clientId = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                x.firstName = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.lastName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.birthDate = reader.GetValue(3) == DBNull.Value ? DateTime.Now.ToString() : reader.GetString(3);
                x.gender.value = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt32(4);
                x.gender.title = GetGenderTitle(x.gender.value);
                x.phone = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                x.email = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                x.userId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                x.date = reader.GetValue(8) == DBNull.Value ? DateTime.Today.ToString() : reader.GetString(8);
                x.isActive = reader.GetValue(9) == DBNull.Value ? 1 : reader.GetInt32(9);
                x.clientData = null; // c.GetClientData(userId, clientId, connection);
                }
            connection.Close();
            return x;
        } catch (Exception e) { return null; }

    }


        //public string clientId { get; set; }
        //public string firstName { get; set; }
        //public string lastName { get; set; }
    }

    #region WebMethods
    [WebMethod]
    public string Init() {
        NewClient x = new NewClient();
        x.clientId = null; // Convert.ToString(Guid.NewGuid()); // null;
        x.firstName = "";
        x.lastName = "";
        x.birthDate = DateTime.UtcNow.ToString();
        //Gender g = new Gender();
        //g.value = 0;
        //g.title = GetGenderTitle(g.value);
        x.gender.value = 0;
        x.gender.title = GetGenderTitle(x.gender.value);
        x.phone = "";
        x.email = "";
        x.userId = null;
        x.date = DateTime.UtcNow.ToString();
        x.isActive = 1;
        x.clientData = new ClientsData.NewClientData();
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Load(string userId) {
        try {
            string json = JsonConvert.SerializeObject(GetClients(userId), Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(string userId, NewClient x) {
        try {
            //TODO check user
         //   if (c.CheckUserId(userId, true) == false) { return JsonConvert.SerializeObject(c.message, Formatting.Indented); }
            db.CreateDataBase(userId, db.clients);
            if (x.clientId == null && Check(userId, x) == false) {
                return ("client is already registered");
            } else {
                if (x.clientId == null) { x.clientId = Convert.ToString(Guid.NewGuid()); }
                SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
                connection.Open();
                string sql = "";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                sql = @"INSERT OR REPLACE INTO clients VALUES
                            (@clientId, @firstName, @lastName, @birthDate, @gender, @phone, @email, @userId, @date, @isActive)";
                command = new SQLiteCommand(sql, connection);
                command.Parameters.Add(new SQLiteParameter("clientId", x.clientId));
                command.Parameters.Add(new SQLiteParameter("firstName", x.firstName));
                command.Parameters.Add(new SQLiteParameter("lastName", x.lastName));
                command.Parameters.Add(new SQLiteParameter("birthDate", x.birthDate));
                command.Parameters.Add(new SQLiteParameter("gender", x.gender.value));
                command.Parameters.Add(new SQLiteParameter("phone", x.phone));
                command.Parameters.Add(new SQLiteParameter("email", x.email));
                command.Parameters.Add(new SQLiteParameter("userId", x.userId));
                command.Parameters.Add(new SQLiteParameter("date", x.date));
                command.Parameters.Add(new SQLiteParameter("isActive", x.isActive));
                command.ExecuteNonQuery();
                connection.Close();
                string json = JsonConvert.SerializeObject(x, Formatting.Indented);
                return json;
            }
        } catch (Exception e) { return ("error" + e); }
    }

    [WebMethod]
    public string Get(string userId, string clientId) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT clientId, firstName, lastName, birthDate, gender, phone, email, userId, date, isActive FROM clients WHERE clientId = @clientId", connection);
            command.Parameters.Add(new SQLiteParameter("clientId", clientId));
            NewClient xx = new NewClient();
            ClientsData c = new ClientsData();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                xx.clientId = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                xx.firstName = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                xx.lastName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                xx.birthDate = reader.GetValue(3) == DBNull.Value ? DateTime.Now.ToString() : reader.GetString(3);
                xx.gender.value = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt32(4);
                xx.gender.title = GetGender(xx.gender.value).title;
                xx.phone = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                xx.email = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                xx.userId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                xx.date = reader.GetValue(8) == DBNull.Value ? DateTime.Today.ToString() : reader.GetString(8);
                xx.isActive = reader.GetValue(9) == DBNull.Value ? 1 : reader.GetInt32(9);
                xx.clientData = c.GetClientData(userId, clientId, connection);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string Delete(string userId, string clientId) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"delete from clients where clientId = @clientId;
                        delete from clientsdata where clientId = @clientId";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("clientId", clientId));
            command.ExecuteNonQuery();
            connection.Close();
        } catch (Exception e) { return ("error: " + e); }
        string json = JsonConvert.SerializeObject(GetClients(userId), Formatting.Indented);
        return json;
    }
    #endregion

    #region Methods
    private bool Check(string userId, NewClient x) {
        try {
            int count = 0;
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(
                "SELECT COUNT([rowid]) FROM clients WHERE firstName = @firstName AND lastName = @lastName AND birthDate = @birthDate AND gender = @gender", connection);
            command.Parameters.Add(new SQLiteParameter("firstName", x.firstName));
            command.Parameters.Add(new SQLiteParameter("lastName", x.lastName));
            command.Parameters.Add(new SQLiteParameter("birthDate", x.birthDate));
            command.Parameters.Add(new SQLiteParameter("gender", x.gender));
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                count = reader.GetInt32(0);
            }
            connection.Close();
            if (count == 0) { return true; }
            else { return false; }
        } catch (Exception e) { return false; }
    }

    public Gender GetGender(int value) {
        Gender x = new Gender();
        x.value = value;
        x.title = value == 0 ? "man" : "woman";
        return x;
    }

    private static string GetGenderTitle(int value) {
        string title = value == 0 ? "man" : "woman";
        return title;
    }

    public List<NewClient> GetClients(string userId) {
        List<NewClient> xx = new List<NewClient>();
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"SELECT clientId, firstName, lastName, birthDate, gender, phone, email, userId, date, isActive
                        FROM clients ORDER BY clientId DESC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            Gender g = new Gender();
            while (reader.Read()) {
                NewClient x = new NewClient();
                x.clientId = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.firstName = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.lastName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.birthDate = reader.GetValue(3) == DBNull.Value ? DateTime.Today.ToString() : reader.GetString(3);
                x.gender.value = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt32(4);
                x.gender.title = GetGender(x.gender.value).title;
                x.phone = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                x.email = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                x.userId = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                x.date = reader.GetValue(8) == DBNull.Value ? DateTime.Today.ToString() : reader.GetString(8);
                x.isActive = reader.GetValue(9) == DBNull.Value ? 1 : reader.GetInt32(9);
                xx.Add(x);
            }
            connection.Close(); 
        } catch (Exception e) { return (new List<NewClient>()); }
        return xx;
    }
    #endregion






}
