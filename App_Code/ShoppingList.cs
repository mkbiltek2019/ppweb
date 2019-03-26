using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using Igprog;

/// <summary>
/// ShoppingList
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class ShoppingList : System.Web.Services.WebService {
    Translate t = new Translate();

    public ShoppingList() {
    }


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

    public object CreateShoppingList(List<Foods.NewFood> x, int consumers, string lang) {
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

    public string SmartQty(string id, double qty, string unit, double mass, string lang) {
        Foods food = new Foods();
        double baseunit = 0;
        string unit_ = food.GetUnit(qty, unit);
        switch (id) {
            case "4592323d-95aa-425f-8794-931886c0b70c":  // bread white
                baseunit = 500;
                unit = "loaf";
                break;
            case "a45d8e18-1310-48e1-878c-8d69655180a9":  // bread french
                baseunit = 300;
                unit = "loaf";
                break;
        }
        if (baseunit > 0 && mass > baseunit) {
            qty = Math.Round(mass / baseunit, 1);
            unit_ = food.GetUnit(qty, t.Tran(unit, lang));
        }

        return string.Format("{0} {1}{2}"
            , qty.ToString()
            , unit_
            , baseunit > 0 && mass > baseunit ? string.Format(" (1 {0} = {1} {2})", t.Tran(unit, lang), baseunit, t.Tran("g", lang)) : "");
    }

}
