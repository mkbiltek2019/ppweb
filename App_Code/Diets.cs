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
/// Diets
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Diets : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["AppDataBase"];
    DataBase db = new DataBase();

    public Diets() {
    }

    public class NewDiet {
        public string id { get; set; }
        public string diet { get; set; }
        public string dietDescription { get; set; }
        public int carbohydratesMin { get; set; }
        public int carbohydratesMax { get; set; }
        public int proteinsMin { get; set; }
        public int proteinsMax { get; set; }
        public int fatsMin { get; set; }
        public int fatsMax { get; set; }
        public int saturatedFatsMin { get; set; }
        public int saturatedFatsMax { get; set; }
        public string note { get; set; }
    }

    #region WebMethods
    [WebMethod]
    public string Init() {
        NewDiet x = new NewDiet();
        x.id = "";
        x.diet = "";
        x.dietDescription = "";
        x.carbohydratesMin = 0;
        x.carbohydratesMax = 0;
        x.proteinsMin = 0;
        x.proteinsMax = 0;
        x.fatsMin = 0;
        x.fatsMax = 0;
        x.saturatedFatsMin = 0;
        x.saturatedFatsMax = 0;
        x.note = "";
        string json = JsonConvert.SerializeObject(x, Formatting.None);
        return json;
    }

    [WebMethod]
    public string Load() {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"SELECT id, diet, dietDescription, carbohydratesMin, carbohydratesMax, proteinsMin, proteinsMax, fatsMin, fatsMax, saturatedFatsMin, saturatedFatsMax, note
                        FROM diets
                        ORDER BY rowid ASC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewDiet> xx = new List<NewDiet>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewDiet x = new NewDiet() {
                    id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0),
                    diet = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1),
                    dietDescription = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2),
                    carbohydratesMin = reader.GetValue(3) == DBNull.Value ? 0 : reader.GetInt32(3),
                    carbohydratesMax = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt32(4),
                    proteinsMin = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt32(5),
                    proteinsMax = reader.GetValue(6) == DBNull.Value ? 0 : reader.GetInt32(6),
                    fatsMin = reader.GetValue(7) == DBNull.Value ? 0 : reader.GetInt32(7),
                    fatsMax = reader.GetValue(8) == DBNull.Value ? 0 : reader.GetInt32(8),
                    saturatedFatsMin = reader.GetValue(9) == DBNull.Value ? 0 : reader.GetInt32(9),
                    saturatedFatsMax = reader.GetValue(10) == DBNull.Value ? 0 : reader.GetInt32(10),
                    note = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11)
            };
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.None);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

     [WebMethod]
    public string Get(string id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"SELECT id, diet, dietDescription, carbohydratesMin, carbohydratesMax, proteinsMin, proteinsMax, fatsMin, fatsMax, saturatedFatsMin, saturatedFatsMax, note
                        FROM diets
                        WHERE id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", id));
            SQLiteDataReader reader = command.ExecuteReader();
            NewDiet x = new NewDiet();
            while (reader.Read()) {
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.diet = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.dietDescription = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.carbohydratesMin = reader.GetValue(3) == DBNull.Value ? 0 : reader.GetInt32(3);
                x.carbohydratesMax = reader.GetValue(4) == DBNull.Value ? 0 : reader.GetInt32(4);
                x.proteinsMin = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt32(5);
                x.proteinsMax = reader.GetValue(6) == DBNull.Value ? 0 : reader.GetInt32(6);
                x.fatsMin = reader.GetValue(7) == DBNull.Value ? 0 : reader.GetInt32(7);
                x.fatsMax = reader.GetValue(8) == DBNull.Value ? 0 : reader.GetInt32(8);
                x.saturatedFatsMin = reader.GetValue(9) == DBNull.Value ? 0 : reader.GetInt32(9);
                x.saturatedFatsMax = reader.GetValue(10) == DBNull.Value ? 0 : reader.GetInt32(10);
                x.note = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(x, Formatting.None);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }
    #endregion

}
