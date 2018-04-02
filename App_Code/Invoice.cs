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

    public Invoice() {
    }

    #region Class
    public class NewInvoice {
        public int id { get; set; } 
        public string number { get; set; }
        public string dateAndTime { get; set; }
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
        x.number = "1/1/1";
        x.dateAndTime = DateTime.Now.ToString("dd.MM.yyyy, HH:mm");
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
        x.number = "1/1/1";
        x.dateAndTime = DateTime.Now.ToString("dd.MM.yyyy, HH:mm");
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
    #endregion Methods



}
