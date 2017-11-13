using System;
using System.Collections.Generic;
using System.Web.Services;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Prices
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Prices : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    DataBase db = new DataBase();

    public Prices() { 
    }

    #region classes
    public class NewPrice {
        public string id { get; set; }
        public IdTitle food { get; set; }
        public ValueCurrency netPrice { get; set; }
        public ValueUnit mass { get; set; }
        public UnitPrice unitPrice { get; set; }
        public string note { get; set; }

        public UnitPrice GetUnitPrice(string userId, string foodId) {
            string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
            DataBase db = new DataBase();
            try {
                SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
                connection.Open();
                string sql = @"SELECT unitPrice, currency, unit
                        FROM prices WHERE foodId = @foodId";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.Add(new SQLiteParameter("foodId", foodId));
                UnitPrice x = new UnitPrice();
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    x.value = reader.GetValue(0) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(0));
                    x.currency = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                    x.unit = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                }
                connection.Close();
                return x;
            } catch (Exception e) { return new UnitPrice(); }
        }

    }

    public class IdTitle {
        public string id { get; set; }
        public string title { get; set; }
    }

    public class ValueCurrency {
        public double value { get; set; }
        public string currency { get; set; }
    }
    public class ValueUnit {
        public int value { get; set; }
        public string unit { get; set; }
    }

    public class UnitPrice {
        public double value { get; set; }
        public string currency { get; set; }
        public string unit { get; set; }
    }

    #endregion Classes


    #region WebMethods
    [WebMethod]
    public string Init() {
        NewPrice x = new NewPrice();
        x.id = null;
        x.food = new IdTitle();
        x.food.id = null;
        x.food.title = null;
        x.netPrice = new ValueCurrency();
        x.netPrice.value = 0.0;
        x.netPrice.currency = null;
        x.mass = new ValueUnit();
        x.mass.value = 1000;
        x.mass.unit = "g";
        x.unitPrice = new UnitPrice();
        x.unitPrice.value = 0.0;
        x.unitPrice.currency = null;
        x.unitPrice.unit = "g";
        x.note = null;
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Load(string userId) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"SELECT id, foodId, food, netPrice, currency, mass, unit, unitPrice, note
                        FROM prices
                        ORDER BY food ASC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewPrice> xx = new List<NewPrice>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewPrice x = new NewPrice();
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.food = new IdTitle();
                x.food.id = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.food.title = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.netPrice = new ValueCurrency();
                x.netPrice.value = reader.GetValue(3) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(3));
                x.netPrice.currency = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.mass = new ValueUnit();
                x.mass.value = reader.GetValue(5) == DBNull.Value ? 1 : reader.GetInt32(5);
                x.mass.unit = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                x.unitPrice = new UnitPrice();
                x.unitPrice.value = reader.GetValue(7) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(7));
                x.unitPrice.currency = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.unitPrice.unit = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                x.note = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        }
        catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(string userId, NewPrice x) {
        try {
            db.CreateDataBase(userId, db.prices);
            x.id = x.id != null ? x.id : Guid.NewGuid().ToString();
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = "";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            sql = @"BEGIN;
                    INSERT OR REPLACE INTO prices (id, foodId, food, netPrice, currency, mass, unit, unitPrice, note)
                    VALUES (@id, @foodId, @food, @netPrice, @currency, @mass, @unit, @unitPrice, @note);
                    COMMIT;";
            command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", x.id));
            command.Parameters.Add(new SQLiteParameter("foodId", x.food.id));
            command.Parameters.Add(new SQLiteParameter("food", x.food.title));
            command.Parameters.Add(new SQLiteParameter("netPrice", x.netPrice.value));
            command.Parameters.Add(new SQLiteParameter("currency", x.netPrice.currency));
            command.Parameters.Add(new SQLiteParameter("mass", x.mass.value));
            command.Parameters.Add(new SQLiteParameter("unit", x.mass.unit));
            command.Parameters.Add(new SQLiteParameter("unitPrice", x.unitPrice.value));
            command.Parameters.Add(new SQLiteParameter("note", x.note));
            command.ExecuteNonQuery();
            connection.Close();
            return "saved";
        }
        catch (Exception e) { return ("Error: " + e); }
    }

     [WebMethod]
    public string Delete(string userId, NewPrice x) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = "";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            sql = @"BEGIN;
                    DELETE FROM prices WHERE id = @id;
                    COMMIT;";
            command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", x.id));
            command.ExecuteNonQuery();
            connection.Close();
            return "ok";
        } catch (Exception e) { return ("Error: " + e); }
    }
    #endregion WebMethods

}
