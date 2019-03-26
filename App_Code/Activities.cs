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
/// Activities
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Activities : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["AppDataBase"];
    DataBase db = new DataBase();
    public Activities() {
    }

    public class NewActivity {
        public int id { get; set; }
        public string activity { get; set; }
        public double factorKcal { get; set; }
        public int isSport { get; set; }
    }

    public class ClientActivity {
        public int id { get; set; }
        public string activity { get; set; }
        public double duration { get; set; }
        public double energy { get; set; }
    }

    #region WebMethods
    [WebMethod]
    public string Init() {
        NewActivity x = new NewActivity();
        x.id = 0;
        x.activity = "";
        x.factorKcal = 0.0;
        x.isSport = 1;
        string json = JsonConvert.SerializeObject(x, Formatting.None);
        return json;
    }

    [WebMethod]
    public string InitClientActivity() {
        ClientActivity x = new ClientActivity();
        x.id = 0;
        x.activity = "";
        x.duration = 0.0;
        string json = JsonConvert.SerializeObject(x, Formatting.None);
        return json;
    }

    [WebMethod]
    public string Load() {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            //SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + lang + "/" + dataBase));
            connection.Open();

            string sql = @"SELECT rowid, activity, factorKcal, isSport
                        FROM activities
                        ORDER BY activity ASC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewActivity> xx = new List<NewActivity>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewActivity x = new NewActivity() {
                    id = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0),
                    activity = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1),
                    factorKcal = reader.GetValue(2) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(2)),
                    isSport = reader.GetValue(3) == DBNull.Value ? 0 : reader.GetInt32(3)
                };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.None);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }
    #endregion


}
