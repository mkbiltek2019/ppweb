using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;

/// <summary>
/// Goal
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Goals : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["AppDataBase"];
    public Goals() {
    }

    public class NewGoal {
        public string code { get; set; }
        public string title { get; set; }
        public bool isDisabled { get; set; }
    }


    #region WebMethods
    [WebMethod]
    public string Load() {
        try {
            List<NewGoal> xx = GetGoals();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Get(string code) {
        try {
            NewGoal x  = GetGoal(code);
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }
    #endregion

    #region Methods

       public List<NewGoal> GetGoals() {
        SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
        connection.Open();
        string sql = @"SELECT code, title FROM codeBook WHERE codeGroup = 'GOAL' ORDER BY codeOrder ASC";
        SQLiteCommand command = new SQLiteCommand(sql, connection);
        List<NewGoal> xx = new List<NewGoal>();
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read()) {
            NewGoal x = new NewGoal() {
                code = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0),
                title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1),
                isDisabled = false
            };
            xx.Add(x);
        }
        connection.Close();
        return xx; 
    }

    public NewGoal GetGoal(string code) {
        SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
        connection.Open();
        string sql = @"SELECT code, title FROM codeBook WHERE codeGroup = 'GOAL' AND code = @code";
        SQLiteCommand command = new SQLiteCommand(sql, connection);
        command.Parameters.Add(new SQLiteParameter("code", code));
        SQLiteDataReader reader = command.ExecuteReader();
        NewGoal x = new NewGoal();
        while (reader.Read()) {
            x.code = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
            x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
            x.isDisabled = false;
        }
        connection.Close();
        return x;
    }
    #endregion
}
