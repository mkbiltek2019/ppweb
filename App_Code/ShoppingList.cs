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
/// ShoppingList
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class ShoppingList : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["AppDataBase"];
    Translate t = new Translate();

    public ShoppingList() {
    }

    #region Classes
    public class NewShoppingList {
        public List<Food> foods;
        public Total total;
    }

    public class Food {
        public string id;
        public string food;
        public double qty;
        public string unit;
        public double mass;
        public string smartQty;
        public string smartMass;
        public double price;
        public string currency;
    }

    public class Total {
        public double price;
        public string currency;
    }

    public class FoodQty {
        public string id;
        public string food;
        public double qty;
        public string unit;
    }
    #endregion

    #region WebMethods
    [WebMethod]
    public string Create(List<Foods.NewFood> x, int consumers, string lang) {
        try {
            return JsonConvert.SerializeObject(CreateShoppingList(x, consumers, lang), Formatting.None);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string CreateWeeklyShoppingList(string userId, List<string> menuList, int consumers, string lang) {
        try {
            Menues me = new Menues();
            List<Foods.NewFood> x = new List<Foods.NewFood>();
            foreach (string m in menuList) {
                if (!string.IsNullOrEmpty(m)) {
                    Menues.NewMenu wm = me.WeeklyMenu(userId, m);
                    x.AddRange(wm.data.selectedFoods);
                }
            }
            return JsonConvert.SerializeObject(CreateShoppingList(x, consumers, lang), Formatting.None);
        } catch (Exception e) {
            return e.Message;
        }
    }
    #endregion

    #region Methods
    public object CreateShoppingList(List<Foods.NewFood> x, int consumers, string lang) {
        List<FoodQty> fq = LoadFoodQty();
        object res = new object();
        List<Foods.NewFood> list = new List<Foods.NewFood>();
        Foods f = new Foods();
        List<Foods.NewFood> foods = new List<Foods.NewFood>();
        if (consumers >= 1) {
            foods = f.MultipleConsumers(x, consumers);
        } else {
            foods = x;
        }
        var group = foods.GroupBy(a => a.food).Select(a => new {
            id = a.Select(i => i.id).FirstOrDefault(),
            food = a.Key,
            qty = a.Sum(q => q.quantity),
            unit = f.GetUnit(a.Sum(q => q.quantity), a.Select(u => u.unit).FirstOrDefault()),
            mass = Math.Round(a.Sum(m => m.mass), 0),
            smartQty = SmartQty(a.Select(i => i.id).FirstOrDefault()
                                , a.Sum(q => q.quantity)
                                , f.GetUnit(a.Sum(q => q.quantity), a.Select(u => u.unit).FirstOrDefault())
                                , Math.Round(a.Sum(m => m.mass), 0)
                                , fq
                                , lang),
            smartMass = SmartMass(Math.Round(a.Sum(m => m.mass), 0), lang),
            price = Math.Round(a.Sum(p => p.price.value), 2),
            currency = a.Select(u => u.price.currency).FirstOrDefault()
        }).ToList();
        var totalPrice = Math.Round(foods.Sum(a => a.price.value), 2);
        var currency = foods.Select(a => a.price.currency).FirstOrDefault();
        res = new {
            foods = group,
            total = new {
                price = totalPrice,
                currency = currency
            }
        };
        return res;
    }

    public List<FoodQty> LoadFoodQty() {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = "SELECT id, food, qty, unit FROM foodQty";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<FoodQty> xx = new List<FoodQty>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                FoodQty x = new FoodQty();
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.food = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.qty = reader.GetValue(2) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(2));
                x.unit = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                xx.Add(x);
            }
            connection.Close();
            return xx;
        } catch (Exception e) { return new List<FoodQty>(); }
    }

    public NewShoppingList Deserialize(object x) {
        NewShoppingList sl = new NewShoppingList();
        string json = JsonConvert.SerializeObject(x, Formatting.None);
        return JsonConvert.DeserializeObject<NewShoppingList>(json);
    }

    public string SmartMass(double mass, string lang) {
        if (mass > 999) {
            return string.Format("{0} {1}", Math.Round((mass / 1000), 1), t.Tran("kg", lang));
        } else {
            return string.Format("{0} {1}", Math.Round(mass, 0), t.Tran("g", lang));
        }
    }

    public string SmartQty(string id, double qty, string unit, double mass, List<FoodQty> foodQty, string lang) {
        Foods food = new Foods();
        double baseQty = 0;
        string unit_ = food.GetUnit(qty, unit);
        FoodQty fq = foodQty.Where(a => a.id == id).FirstOrDefault();

        if (fq != null) {
            baseQty = fq.qty;
            unit = fq.unit;
        }
        if (baseQty > 0 && mass > baseQty) {
            qty = Math.Round(mass / baseQty, 1);
            unit_ = food.GetUnit(qty, t.Tran(unit, lang));
        }

        return string.Format("{0} {1}{2}"
            , qty.ToString()
            , unit_
            , baseQty > 0 && mass > baseQty ? string.Format(" (1 {0} = {1} {2})", t.Tran(unit, lang), baseQty, t.Tran("g", lang)) : "");
    }
    #endregion

}
