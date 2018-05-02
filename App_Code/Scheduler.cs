using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Scheduler
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Scheduler : System.Web.Services.WebService {
    // SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    DataBase db = new DataBase();

    public Scheduler() {
    }

    public class Event {
        public int? id { get; set; }
        public int room { get; set; }
        public string clientId { get; set; }
        public string content { get; set; }
        public long startDate { get; set; }
        public long endDate { get; set; }
        public string userId { get; set; }
    }

    [WebMethod]
    public string Init() {
        Event x = new Event();
        x.id = null;
        x.room = 0;
        x.clientId = null;
        x.content = "";
        x.startDate = Convert.ToInt64(DateTime.UtcNow.Ticks);
        x.endDate = Convert.ToInt64(DateTime.UtcNow.Ticks);
        x.userId = null;
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }



    [WebMethod]
    public string Load(string userGroupId, string userId) {
        // string path = db.GetDataBasePath(userId, dataBase); // GetDataBasePath(userId);
        db.CreateDataBase(userGroupId, db.scheduler);
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userGroupId, dataBase));
            connection.Open();
            string sql = "SELECT rowid, room, clientId, content, startDate, endDate, userId FROM scheduler";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            List<Event> events = new List<Event>();
            while (reader.Read()) {
                Event x = new Event();
                x.id = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                x.room = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1);
                x.clientId = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.content = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.startDate = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt64(4);
                x.endDate = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt64(5);
                x.userId = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                events.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(events, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(string userGroupId, string userId, Event x) {
        //  string path = GetDataBasePath(userId);
        db.CreateDataBase(userGroupId, db.scheduler);
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userGroupId, dataBase));
            connection.Open();
            string sql = @"INSERT INTO scheduler (room, clientId, content, startDate, endDate, userId)
                        VALUES (@room, @clientId, @content, @startDate, @endDate, @userId)";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("clientId", x.clientId));
            command.Parameters.Add(new SQLiteParameter("room", x.room));
            command.Parameters.Add(new SQLiteParameter("content", x.content));
            command.Parameters.Add(new SQLiteParameter("startDate", x.startDate));
            command.Parameters.Add(new SQLiteParameter("endDate", x.endDate));
            command.Parameters.Add(new SQLiteParameter("userId", x.userId));

            command.ExecuteNonQuery();
            connection.Close();
            return ("OK.");
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Delete(string userGroupId, string userId, Event x) {
        //  string path = GetDataBasePath(userId);
        db.CreateDataBase(userGroupId, db.scheduler);
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userGroupId, dataBase));
            connection.Open();
            //string sql = @"DELETE scheduler WHERE scheduler.content = @Content AND startDate = @StartDate AND room = @Room";
            string sql = @"DELETE FROM scheduler WHERE [content] = @Content AND [startDate] = @StartDate AND [room] = @Room AND [userId] = @userId";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("Content", x.content));
            command.Parameters.Add(new SQLiteParameter("StartDate", x.startDate));
            command.Parameters.Add(new SQLiteParameter("Room", x.room));
            command.Parameters.Add(new SQLiteParameter("userId", x.userId));
            command.ExecuteNonQuery();
            connection.Close();
            return ("OK.");
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string GetSchedulerByRoom(string userGroupId, string userId, int room) {
        //string path = GetDataBasePath(userId);
        db.CreateDataBase(userGroupId, db.scheduler);
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userGroupId, dataBase));
            connection.Open();
            string sql = @"SELECT rowid, room, clientId, content, startDate, endDate FROM scheduler WHERE room = @Room";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("Room", room));
            SQLiteDataReader reader = command.ExecuteReader();
            List<Event> xx = new List<Event>();
            while (reader.Read()) {
                Event x = new Event();
                x.id = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                x.room = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1);
                x.clientId = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.content = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.startDate = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt64(4);
                x.endDate = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt64(5);
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    //#region Methods
    //private string GetDataBasePath(string userId) {
    //    return Server.MapPath("~/App_Data/users/" + userId + "/" + dataBase);
    //}
    //#endregion Methods

}
