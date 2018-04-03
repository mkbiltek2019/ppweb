using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Invoice
/// </summary>
[WebService(Namespace = "http://programprehrane.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Invoice : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["WebDataBase"];
    DataBase db = new DataBase();
    public Invoice() {
    }

    #region Class
    public class NewInvoice {
        public int id { get; set; } 
        public string number { get; set; }
        public string fileName { get; set; }
        public int orderNumber { get; set; }
        public string dateAndTime { get; set; }
        public int year { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string companyName { get; set; }
        public string address { get; set; }
        public string postalCode { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string pin { get; set; }
        public string note { get; set; }

        public List<Item> items = new List<Item>();
    }

    public class Item {
        public string title { get; set; }
        public int qty { get; set; }
        public double unitPrice { get; set; }
    }
    #endregion Class

    #region WebMethods
    [WebMethod]
    public string Init() {
        NewInvoice x = new NewInvoice();
        x.id = 0;
        x.number = GetNextOrderNumber();
        x.fileName = null;
        x.orderNumber = 0;
        x.dateAndTime = DateTime.Now.ToString("dd.MM.yyyy, HH:mm");
        x.year = DateTime.Now.Year;
        x.firstName = null;
        x.lastName = null;
        x.companyName = null;
        x.address = null;
        x.postalCode = null;
        x.city = null;
        x.country = null;
        x.pin = null;
        x.note = null;
        x.items = new List<Item>();
        Item item = new Item();
        item.title = null;
        item.qty = 1;
        item.unitPrice = 0;
        x.items.Add(item);
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string InitPP(Orders.NewUser order) {
        NewInvoice x = new NewInvoice();
        x.id = 0;
        x.number = null;
        x.fileName = null;
        x.orderNumber = order.id;
        x.dateAndTime = DateTime.Now.ToString("dd.MM.yyyy, HH:mm");
        x.year = DateTime.Now.Year;
        x.firstName = order.firstName;
        x.lastName = order.lastName;
        x.companyName = order.companyName;
        x.address = order.address;
        x.postalCode = order.postalCode;
        x.city = order.city;
        x.country = order.country;
        x.pin = order.pin;
        x.note = null;
        x.items = GetItems(order);
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Load() {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"SELECT rowid, number, fileName, orderNumber, dateAndTime, year, firstName, lastName, companyName, address, postalCode, city, country, pin, note, items
                        FROM invoices
                        ORDER BY rowid DESC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewInvoice> xx = new List<NewInvoice>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewInvoice x = new NewInvoice();
                x.id = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                x.number = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.fileName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.orderNumber = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToInt32(reader.GetString(3));
                x.dateAndTime = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.year = reader.GetValue(5) == DBNull.Value ? DateTime.Now.Year : Convert.ToInt32(reader.GetString(5));
                x.firstName = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                x.lastName = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                x.companyName = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
                x.address = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                x.postalCode = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                x.city = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
                x.country = reader.GetValue(12) == DBNull.Value ? "" : reader.GetString(12);
                x.pin = reader.GetValue(13) == DBNull.Value ? "" : reader.GetString(13);
                x.note = reader.GetValue(14) == DBNull.Value ? "" : reader.GetString(14);
                x.items = reader.GetValue(15) == DBNull.Value ? new List<Item>() : JsonConvert.DeserializeObject<List<Item>>(reader.GetString(15));
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(NewInvoice x, string pdf) {
            try {
            string path = Server.MapPath("~/App_Data/" + dataBase);
            string pdfTempPath = Server.MapPath(string.Format("~/upload/invoice/temp/{0}.pdf", pdf));
            int year = x.year; // Convert.ToDateTime(x.dateAndTime).Year;
            string fileName = string.Format("{0}_{1}", x.number, year);
            string pdfDir = string.Format("~/upload/invoice/{0}/", year);
            string pdfPath = Server.MapPath(string.Format("{0}{1}.pdf", pdfDir, fileName));

            db.CreateGlobalDataBase(path, db.invoices);
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"INSERT OR REPLACE INTO invoices VALUES  
                       (@number, @fileName, @orderNumber, @dateAndTime, @year, @firstName, @lastName, @companyName, @address, @postalCode, @city, @country, @pin, @note, @items)";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("number", x.number));
            command.Parameters.Add(new SQLiteParameter("fileName", fileName));
            command.Parameters.Add(new SQLiteParameter("orderNumber", x.orderNumber));
            command.Parameters.Add(new SQLiteParameter("dateAndTime", x.dateAndTime));
            command.Parameters.Add(new SQLiteParameter("year", x.year));
            command.Parameters.Add(new SQLiteParameter("firstName", x.firstName));
            command.Parameters.Add(new SQLiteParameter("lastName", x.lastName));
            command.Parameters.Add(new SQLiteParameter("companyName", x.companyName));
            command.Parameters.Add(new SQLiteParameter("address", x.address));
            command.Parameters.Add(new SQLiteParameter("postalCode", x.postalCode));
            command.Parameters.Add(new SQLiteParameter("city", x.city));
            command.Parameters.Add(new SQLiteParameter("country", x.country));
            command.Parameters.Add(new SQLiteParameter("pin", x.pin));
            command.Parameters.Add(new SQLiteParameter("note", x.note));
            command.Parameters.Add(new SQLiteParameter("items", JsonConvert.SerializeObject(x.items, Formatting.Indented)));

            command.ExecuteNonQuery();
            connection.Close();

            if (!Directory.Exists(Server.MapPath(pdfDir))) {
                Directory.CreateDirectory(Server.MapPath(pdfDir));
            }
            File.Copy(pdfTempPath, pdfPath, true);
            return string.Format("{0}/{1}", year, fileName);
            } catch (Exception e) { return ("Error: " + e); }
        } 
    #endregion WebMethods

    #region Methods
    private List<Item> GetItems(Orders.NewUser order) {
        Item x = new Item();
        x.title = string.Format("{0} {1}", order.application, order.version);
        x.qty = Convert.ToInt32(order.licenceNumber);
        x.unitPrice = order.price;
        List<Item> xx = new List<Item>();
        xx.Add(x);
        return xx;
    }

    private string GetNextOrderNumber() {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"SELECT MAX(CAST(number as int) +1 ) FROM invoices";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            int nextNumber = 0;
            while (reader.Read()) {
                nextNumber = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
            }
            connection.Close();
            return nextNumber.ToString();
        } catch (Exception e) { return null; }
    }
    #endregion Methods



}
