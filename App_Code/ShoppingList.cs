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
        public double price;
        public string currency;
    }

    public class Total {
        public double price;
        public string currency;
    }


    [WebMethod]
    public string Create(List<Foods.NewFood> x, int consumers) {
        try {
            return JsonConvert.SerializeObject(CreateShoppingList(x, consumers), Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string CreateWeeklyShoppingList(string userId, List<string> menuList, int consumers) {
        try {
            Menues me = new Menues();
            List<Foods.NewFood> x = new List<Foods.NewFood>();
            foreach (string m in menuList) {
                if (!string.IsNullOrEmpty(m)) {
                    Menues.NewMenu wm = me.WeeklyMenu(userId, m);
                    x.AddRange(wm.data.selectedFoods);
                }
            }
            return JsonConvert.SerializeObject(CreateShoppingList(x, consumers), Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    public object CreateShoppingList(List<Foods.NewFood> x, int consumers) {
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
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return JsonConvert.DeserializeObject<NewShoppingList>(json);
    }

}
