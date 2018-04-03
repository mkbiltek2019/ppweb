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
///Order
/// </summary>
[WebService(Namespace = "http://programprehrane.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Orders : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["WebDataBase"];
    DataBase db = new DataBase();
    public Orders() { 
    }
    public class NewUser {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string companyName { get; set; }
        public string address { get; set; }
        public string postalCode { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string pin { get; set; }
        public string email { get; set; }
        public string ipAddress { get; set; }
        public string application { get; set; }
        public string version { get; set; }
        public string licence { get; set; }
        public string licenceNumber { get; set; }
        public double price { get; set; }
        public double priceEur { get; set; }
        public string orderDate { get; set; }
        public string additionalService { get; set; }
        public string note { get; set; }

    }

    [WebMethod]
    public string Init() {
        NewUser x = new NewUser();
            x.id = 0;
            x.firstName = "";
            x.lastName = "";
            x.companyName = "";
            x.address = "";
            x.postalCode = "";
            x.city = "";
            x.country = "";
            x.pin = null;
            x.email = "";
            x.ipAddress = HttpContext.Current.Request.UserHostAddress;
            x.application = "";
            x.version = "";
            x.licence = "";
            x.licenceNumber = "";
            x.price = 0.0;
            x.priceEur = 0.0;
            x.orderDate = DateTime.Now.ToString();
            x.additionalService = "";
            x.note = "";
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Load() {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"SELECT rowid, firstName, lastName, companyName, address, postalCode, city, country, pin, email, ipAddress, application, version, licence, licenceNumber, price, priceEur, orderDate, additionalService, note
                        FROM orders
                        ORDER BY rowid DESC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewUser> xx = new List<NewUser>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewUser x = new NewUser();
                x.id = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                x.firstName = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.lastName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.companyName = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.address = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.postalCode = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                x.city = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                x.country = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                x.pin = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
                x.email = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                x.ipAddress = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                x.application = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
                x.version = reader.GetValue(12) == DBNull.Value ? "" : reader.GetString(12);
                x.licence = reader.GetValue(13) == DBNull.Value ? "" : reader.GetString(13);
                x.licenceNumber = reader.GetValue(14) == DBNull.Value ? "" : reader.GetString(14);
                x.price = reader.GetValue(15) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(15));
                x.priceEur = reader.GetValue(16) == DBNull.Value ? 0.0 : Convert.ToDouble(reader.GetString(16));
                x.orderDate = reader.GetValue(17) == DBNull.Value ? "" : reader.GetString(17);
                x.additionalService = reader.GetValue(18) == DBNull.Value ? "" : reader.GetString(18);
                x.note = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string SendOrder(NewUser x) {
            try {
            string path = HttpContext.Current.Server.MapPath("~/App_Data/" + dataBase);
            db.CreateGlobalDataBase(path, db.orders);
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"INSERT INTO orders VALUES  
                       (@firstName, @lastName, @companyName, @address, @postalCode, @city, @country, @pin, @email, @ipAddress, @application, @version, @licence, @licenceNumber, @price, @priceEur, @orderDate, @additionalService, @note)";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("firstName", x.firstName));
            command.Parameters.Add(new SQLiteParameter("lastName", x.lastName));
            command.Parameters.Add(new SQLiteParameter("companyName", x.companyName));
            command.Parameters.Add(new SQLiteParameter("address", x.address));
            command.Parameters.Add(new SQLiteParameter("postalCode", x.postalCode));
            command.Parameters.Add(new SQLiteParameter("city", x.city));
            command.Parameters.Add(new SQLiteParameter("country", x.country));
            command.Parameters.Add(new SQLiteParameter("pin", x.pin));
            command.Parameters.Add(new SQLiteParameter("email", x.email));
            command.Parameters.Add(new SQLiteParameter("ipAddress", x.ipAddress));
            command.Parameters.Add(new SQLiteParameter("application", x.application));
            command.Parameters.Add(new SQLiteParameter("version", x.version));
            command.Parameters.Add(new SQLiteParameter("licence", x.licence));
            command.Parameters.Add(new SQLiteParameter("licenceNumber", x.licenceNumber));
            command.Parameters.Add(new SQLiteParameter("price", x.price));
            command.Parameters.Add(new SQLiteParameter("priceEur", x.priceEur));
            command.Parameters.Add(new SQLiteParameter("orderDate", Convert.ToString(x.orderDate)));
            command.Parameters.Add(new SQLiteParameter("additionalService", x.additionalService));
            command.Parameters.Add(new SQLiteParameter("note", x.note));
            command.ExecuteNonQuery();
            connection.Close();
            Mail m = new Mail();
            m.SendOrder(x);
            return ("OK");
            } catch (Exception e) { return ("Error: " + e); }
        }

}
