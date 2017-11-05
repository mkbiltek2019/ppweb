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
/// Instal
/// </summary>
[WebService(Namespace = "http://programprehrane.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Instal : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["WebDataBase"];
    DataBase db = new DataBase();
    public Instal() {
    }

    public class NewInstal {
        public int id { get; set; }
        public string instalDate { get; set; }
        public string application { get; set; }
        public string version { get; set; }
        public string action { get; set; }
        public string ipAddress { get; set; }
    }

    [WebMethod]
    public string Init() {
        NewInstal x = new NewInstal();
        x.id = 0;
        x.instalDate = DateTime.Now.ToString();
        x.application = "";
        x.version = "";
        x.action = "";
        x.ipAddress = HttpContext.Current.Request.UserHostAddress;
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Load() {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"SELECT rowid, instalDate, application, version, action, ipAddress
                        FROM instals
                        ORDER BY rowid DESC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewInstal> xx = new List<NewInstal>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewInstal x = new NewInstal();
                x.id = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                x.instalDate = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.application = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.version = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.action = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.ipAddress = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(NewInstal x) {
        try {
            string path = HttpContext.Current.Server.MapPath("~/App_Data/" + dataBase);
            db.CreateGlobalDataBase(path, db.instals);
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"INSERT INTO instals VALUES  
                       (@instalDate, @application, @version, @action, @ipAddress)";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("instalDate", x.instalDate));
            command.Parameters.Add(new SQLiteParameter("application", x.application));
            command.Parameters.Add(new SQLiteParameter("version", x.version));
            command.Parameters.Add(new SQLiteParameter("action", x.action));
            command.Parameters.Add(new SQLiteParameter("ipAddress", x.ipAddress));
            command.ExecuteNonQuery();
            connection.Close();
            return ("OK");
        } catch (Exception e) { return ("Error: " + e); }
    }


}
