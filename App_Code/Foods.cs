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
/// Foods
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Foods : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["AppDataBase"];
  //  string userDataBase = ConfigurationManager.AppSettings["UserDataBase"];

    DataBase db = new DataBase();
    public Foods() {
    }
    // id, food, foodGroup, foodGroupVitaminLost, quantity, unit, mass, energy, carbohydrates, proteins, fats, cerealsServ, vegetablesServ, fruitServ, meatServ, milkServ, fatsServ, otherFoodsServ, starch, totalSugar, glucose, fructose, saccharose, maltose, lactose, fibers, saturatedFats, monounsaturatedFats, polyunsaturatedFats, trifluoroaceticAcid, cholesterol, sodium, potassium, calcium, magnesium, phosphorus, iron, copper, zinc, chlorine, manganese, selenium, iodine, retinol, carotene, vitaminD, vitaminE, vitaminB1, vitaminB2, vitaminB3, vitaminB6, vitaminB12, folate, pantothenicAcid, biotin, vitaminC, vitaminK
    #region Classes
    public class NewFood {
        public string id { get; set; }
        public string food { get; set; }

        public Group foodGroup = new Group();
        public string foodGroupVitaminLost { get; set; }
        public List<ThermalTreatment> thermalTreatments { get; set; }

        public CodeTitle meal = new CodeTitle();
        public double quantity { get; set; }
        public string unit { get; set; }
        public double mass { get; set; }
        public double energy { get; set; }
        public double carbohydrates { get; set; }
        public double proteins { get; set; }
        public double fats { get; set; }

        public Servings servings = new Servings();

        public double starch { get; set; }
        public double totalSugar { get; set; }
        public double glucose { get; set; }
        public double fructose { get; set; }
        public double saccharose { get; set; }
        public double maltose { get; set; }
        public double lactose { get; set; }
        public double fibers { get; set; }
        public double saturatedFats { get; set; }
        public double monounsaturatedFats { get; set; }
        public double polyunsaturatedFats { get; set; }
        public double trifluoroaceticAcid { get; set; }
        public double cholesterol { get; set; }
        public double sodium { get; set; }
        public double potassium { get; set; }
        public double calcium { get; set; }
        public double magnesium { get; set; }
        public double phosphorus { get; set; }
        public double iron { get; set; }
        public double copper { get; set; }
        public double zinc { get; set; }
        public double chlorine { get; set; }
        public double manganese { get; set; }
        public double selenium { get; set; }
        public double iodine { get; set; }
        public double retinol { get; set; }
        public double carotene { get; set; }
        public double vitaminD { get; set; }
        public double vitaminE { get; set; }
        public double vitaminB1 { get; set; }
        public double vitaminB2 { get; set; }
        public double vitaminB3 { get; set; }
        public double vitaminB6 { get; set; }
        public double vitaminB12 { get; set; }
        public double folate { get; set; }
        public double pantothenicAcid { get; set; }
        public double biotin { get; set; }
        public double vitaminC { get; set; }
        public double vitaminK { get; set; }

        public Prices.UnitPrice price = new Prices.UnitPrice();
    }

    public class FoodData {
        public List<NewFood> foods = new List<NewFood>();
        public List<NewFood> myFoods = new List<NewFood>();
        public List<FoodGroup> foodGroups = new List<FoodGroup>();
    }


    public class FoodGroup {
        public CodeTitle group = new CodeTitle();
        public string parent { get; set; }
        public int groupOrder { get; set; }
    }

    public class Group {
        public string code { get; set; }
        public string title { get; set; }
        public string parent { get; set; }
    }

    public class CodeTitle {
        public string code { get; set; }
        public string title { get; set; }
    }

    public class ThermalTreatment {
        public string foodGroupVitaminLost { get; set; }

        public CodeTitle thermalTreatment = new CodeTitle();
        public bool isSelected { get; set; }
        public double vitaminE { get; set; }
        public double vitaminB1 { get; set; }
        public double vitaminB2 { get; set; }
        public double vitaminB3 { get; set; }
        public double vitaminB6 { get; set; }
        public double vitaminB12 { get; set; }
        public double folate { get; set; }
        public double pantothenicAcid { get; set; }
        public double biotin { get; set; }
        public double vitaminC { get; set; }

        //foodGroupVitaminLost, thermalTreatment, vitaminE, vitaminB1, vitaminB2, vitaminB3, vitaminB6, vitaminB12, folate, pantothenicAcid, biotin, vitaminC
    }


    public class Servings {
        public double cerealsServ { get; set; }
        public double vegetablesServ { get; set; }
        public double fruitServ { get; set; }
        public double meatServ { get; set; }
        public double milkServ { get; set; }
        public double fatsServ { get; set; }
        public double otherFoodsServ { get; set; }
        public double otherFoodsEnergy { get; set; }
    }

    public class Totals {
        public double mass { get; set; }
        public double energy { get; set; }
        public double carbohydrates { get; set; }
        public double carbohydratesPercentage { get; set; }
        public double proteins { get; set; }
        public double proteinsPercentage { get; set; }
        public double fats { get; set; }
        public double fatsPercentage { get; set; }

        public Servings servings = new Servings();

        public double starch { get; set; }
        public double totalSugar { get; set; }
        public double glucose { get; set; }
        public double fructose { get; set; }
        public double saccharose { get; set; }
        public double maltose { get; set; }
        public double lactose { get; set; }
        public double fibers { get; set; }
        public double saturatedFats { get; set; }
        public double monounsaturatedFats { get; set; }
        public double polyunsaturatedFats { get; set; }
        public double trifluoroaceticAcid { get; set; }
        public double cholesterol { get; set; }
        public double sodium { get; set; }
        public double potassium { get; set; }
        public double calcium { get; set; }
        public double magnesium { get; set; }
        public double phosphorus { get; set; }
        public double iron { get; set; }
        public double copper { get; set; }
        public double zinc { get; set; }
        public double chlorine { get; set; }
        public double manganese { get; set; }
        public double selenium { get; set; }
        public double iodine { get; set; }
        public double retinol { get; set; }
        public double carotene { get; set; }
        public double vitaminD { get; set; }
        public double vitaminE { get; set; }
        public double vitaminB1 { get; set; }
        public double vitaminB2 { get; set; }
        public double vitaminB3 { get; set; }
        public double vitaminB6 { get; set; }
        public double vitaminB12 { get; set; }
        public double folate { get; set; }
        public double pantothenicAcid { get; set; }
        public double biotin { get; set; }
        public double vitaminC { get; set; }
        public double vitaminK { get; set; }

        public List<MealsTotalEnergy> mealsTotalEnergy = new List<MealsTotalEnergy>();

        public Prices.UnitPrice price = new Prices.UnitPrice();
    }

    public class MealsTotalEnergy {
        public CodeEnegy meal = new CodeEnegy();
    }

    public class CodeEnegy {
        public string code { get; set; }
        public double energy { get; set; }
        public double energyPercentage { get; set; }
    }

    public class Recommendations {
        public int energy { get; set; }
        public int carbohydratesMin { get; set; }
        public int carbohydratesMax { get; set; }
        public int carbohydratesPercentageMin { get; set; }
        public int carbohydratesPercentageMax { get; set; }
        public int proteinsMin { get; set; }
        public int proteinsMax { get; set; }
        public int proteinsPercentageMin { get; set; }
        public int proteinsPercentageMax { get; set; }
        public int fatsMin { get; set; }
        public int fatsMax { get; set; }
        public int fatsPercentageMin { get; set; }
        public int fatsPercentageMax { get; set; }

        public Servings servings = new Servings();

        public ParameterRecommendation starch = new ParameterRecommendation();
        public ParameterRecommendation totalSugar = new ParameterRecommendation();
        public ParameterRecommendation glucose = new ParameterRecommendation();
        public ParameterRecommendation fructose = new ParameterRecommendation();
        public ParameterRecommendation saccharose = new ParameterRecommendation();
        public ParameterRecommendation maltose = new ParameterRecommendation();
        public ParameterRecommendation lactose = new ParameterRecommendation();
        public ParameterRecommendation fibers = new ParameterRecommendation();
        public ParameterRecommendation saturatedFats = new ParameterRecommendation();
        public ParameterRecommendation monounsaturatedFats = new ParameterRecommendation();
        public ParameterRecommendation polyunsaturatedFats = new ParameterRecommendation();
        public ParameterRecommendation trifluoroaceticAcid = new ParameterRecommendation();
        public ParameterRecommendation cholesterol = new ParameterRecommendation();
        public ParameterRecommendation sodium = new ParameterRecommendation();
        public ParameterRecommendation potassium = new ParameterRecommendation();
        public ParameterRecommendation calcium = new ParameterRecommendation();
        public ParameterRecommendation magnesium = new ParameterRecommendation();
        public ParameterRecommendation phosphorus = new ParameterRecommendation();
        public ParameterRecommendation iron = new ParameterRecommendation();
        public ParameterRecommendation copper = new ParameterRecommendation();
        public ParameterRecommendation zinc = new ParameterRecommendation();
        public ParameterRecommendation chlorine = new ParameterRecommendation();
        public ParameterRecommendation manganese = new ParameterRecommendation();
        public ParameterRecommendation selenium = new ParameterRecommendation();
        public ParameterRecommendation iodine = new ParameterRecommendation();
        public ParameterRecommendation retinol = new ParameterRecommendation();
        public ParameterRecommendation carotene = new ParameterRecommendation();
        public ParameterRecommendation vitaminD = new ParameterRecommendation();
        public ParameterRecommendation vitaminE = new ParameterRecommendation();
        public ParameterRecommendation vitaminB1 = new ParameterRecommendation();
        public ParameterRecommendation vitaminB2 = new ParameterRecommendation();
        public ParameterRecommendation vitaminB3 = new ParameterRecommendation();
        public ParameterRecommendation vitaminB6 = new ParameterRecommendation();
        public ParameterRecommendation vitaminB12 = new ParameterRecommendation();
        public ParameterRecommendation folate = new ParameterRecommendation();
        public ParameterRecommendation pantothenicAcid = new ParameterRecommendation();
        public ParameterRecommendation biotin = new ParameterRecommendation();
        public ParameterRecommendation vitaminC = new ParameterRecommendation();
        public ParameterRecommendation vitaminK = new ParameterRecommendation();

        public List<MealsRecommendationEnergy> mealsRecommendationEnergy = new List<MealsRecommendationEnergy>();
    }

    public class MealsRecommendationEnergy {
        public MealRecommendation meal = new MealRecommendation();
    }

    public class MealRecommendation {
        public string code { get; set; }
        public int energyMinPercentage { get; set; }
        public int energyMaxPercentage { get; set; }
        public int energyMin { get; set; }
        public int energyMax { get; set; }
    }

    public class MealEnergy {
        public int min { get; set; }
        public int max { get; set; }
    }

    public class ParameterRecommendation {
        public double? mda { get; set; }
        public double? ui { get; set; }
        public double? rda { get; set; }
    }

    public class InitData {

       public NewFood food = new NewFood();
       public List<string> units { get; set; }
       public List<FoodGroup> foodGroups { get; set; }
    }

    #endregion

    #region WebMethods
    [WebMethod]
    public string Init() {
        SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));

        InitData data = new InitData();

        NewFood x = new NewFood();
        x.id = null;
        x.food = null;
        x.foodGroup = new Group();
        x.foodGroupVitaminLost = "";
        x.thermalTreatments = new List<ThermalTreatment>();
        x.meal.code = "B";
        x.meal.title = Meals.GetMealTitle(x.meal.code, connection);
        x.quantity = 1;
        x.unit = "";
        x.mass = 0;
        x.energy = 0;
        x.carbohydrates = 0;
        x.proteins = 0;
        x.fats = 0;
        x.servings = new Servings();
        x.starch = 0;
        x.totalSugar = 0;
        x.glucose = 0;
        x.fructose = 0;
        x.saccharose = 0;
        x.maltose = 0;
        x.lactose = 0;
        x.fibers = 0;
        x.saturatedFats = 0;
        x.monounsaturatedFats = 0;
        x.polyunsaturatedFats = 0;
        x.trifluoroaceticAcid = 0;
        x.cholesterol = 0;
        x.sodium = 0;
        x.potassium = 0;
        x.calcium = 0;
        x.magnesium = 0;
        x.phosphorus = 0;
        x.iron = 0;
        x.copper = 0;
        x.zinc = 0;
        x.chlorine = 0;
        x.manganese = 0;
        x.selenium = 0;
        x.iodine = 0;
        x.retinol = 0;
        x.carotene = 0;
        x.vitaminD = 0;
        x.vitaminE = 0;
        x.vitaminB1 = 0;
        x.vitaminB2 = 0;
        x.vitaminB3 = 0;
        x.vitaminB6 = 0;
        x.vitaminB12 = 0;
        x.folate = 0;
        x.pantothenicAcid = 0;
        x.biotin = 0;
        x.vitaminC = 0;
        x.vitaminK = 0;

        x.price = new Prices.UnitPrice();

        data.food = x;
        data.foodGroups = GetMainFoodGroups(connection);
        data.units = Units();

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Load(string userId) {
        try {
            //SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + lang + "/" + dataBase));
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"SELECT f.id, f.food, f.foodGroup, fg.title
                        FROM foods AS f
                        LEFT OUTER JOIN foodGroups AS fg
                        ON f.foodGroup = fg.code
                        ORDER BY food ASC";
            //string sql = @"SELECT f.id, f.food, f.foodGroup, fg.title, f.foodGroupVitaminLost, f.quantity, f.unit, f.mass, f.energy, f.carbohydrates, f.proteins, f.fats,
            //            f.cerealsServ, f.vegetablesServ, f.fruitServ, f.meatServ, f.milkServ, f.fatsServ, f.otherFoodsServ,
            //            f.starch, f.totalSugar, f.glucose, f.fructose, f.saccharose, f.maltose, f.lactose, f.fibers, f.saturatedFats,
            //            f.monounsaturatedFats, f.polyunsaturatedFats, f.trifluoroaceticAcid, f.cholesterol, f.sodium, f.potassium,
            //            f.calcium, f.magnesium,f.phosphorus, f.iron, f.copper, f.zinc, f.chlorine, f.manganese, f.selenium, f.iodine,
            //            f.retinol, f.carotene, f.vitaminD, f.vitaminE, f.vitaminB1, f.vitaminB2,f.vitaminB3, f.vitaminB6, f.vitaminB12,
            //            f.folate, f.pantothenicAcid, f.biotin, f.vitaminC, f.vitaminK
            //            FROM foods AS f
            //            LEFT OUTER JOIN foodGroups AS fg
            //            ON f.foodGroup = fg.code
            //            ORDER BY food ASC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewFood> xx = new List<NewFood>();
            FoodData foodData = new FoodData();
            List<FoodGroup> foodGroups = new List<FoodGroup>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewFood x = new NewFood();
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.food = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.foodGroup.code = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.foodGroup.title = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.foodGroup.parent = GetParentGroup(connection, x.foodGroup.code);
                //x.foodGroupVitaminLost = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                //x.thermalTreatments = GetThermalTreatments(connection, x.foodGroupVitaminLost);
                //x.meal.code = "B";
                //x.meal.title = lang != null ? Meals.GetMealTitle(lang, x.meal.code, connection) : "";
                //x.quantity = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt32(5);
                //x.unit = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                //x.mass = reader.GetValue(7) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(7));
                //x.energy = reader.GetValue(8) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(8));
                //x.carbohydrates = reader.GetValue(9) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(9));
                //x.proteins = reader.GetValue(10) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(10));
                //x.fats = reader.GetValue(11) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(11));
                //x.cerealsServ = reader.GetValue(12) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(12));
                //x.vegetablesServ = reader.GetValue(13) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(13));
                //x.fruitServ = reader.GetValue(14) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(14));
                //x.meatServ = reader.GetValue(15) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(15));
                //x.milkServ = reader.GetValue(16) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(16));
                //x.fatsServ = reader.GetValue(17) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(17));
                //x.otherFoodsServ = reader.GetValue(18) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(18));
                //x.otherFoodsEnergy = x.otherFoodsServ > 0 ? x.energy : 0;
                xx.Add(x);
            }

            foodData.foods = xx;
            foodData.foodGroups = GetFoodGroups(connection);
            connection.Close();

           
            MyFoods.Data myf = new MyFoods.Data();
            foodData.myFoods = myf.GetMyFoods(userId);
           // foodData.foods.AddRange(myf.GetMyFoods(userId));
           // foodData.myFoods = myf.GetMyFoods(userId);

            string json = JsonConvert.SerializeObject(foodData, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            //SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + lang + "/" + dataBase));
            connection.Open();
            string sql = @"SELECT f.id, f.food, f.foodGroup, fg.title, f.foodGroupVitaminLost, f.quantity, f.unit, f.mass, f.energy, f.carbohydrates, f.proteins, f.fats,
                        f.cerealsServ, f.vegetablesServ, f.fruitServ, f.meatServ, f.milkServ, f.fatsServ, f.otherFoodsServ,
                        f.starch, f.totalSugar, f.glucose, f.fructose, f.saccharose, f.maltose, f.lactose, f.fibers, f.saturatedFats,
                        f.monounsaturatedFats, f.polyunsaturatedFats, f.trifluoroaceticAcid, f.cholesterol, f.sodium, f.potassium,
                        f.calcium, f.magnesium,f.phosphorus, f.iron, f.copper, f.zinc, f.chlorine, f.manganese, f.selenium, f.iodine,
                        f.retinol, f.carotene, f.vitaminD, f.vitaminE, f.vitaminB1, f.vitaminB2,f.vitaminB3, f.vitaminB6, f.vitaminB12,
                        f.folate, f.pantothenicAcid, f.biotin, f.vitaminC, f.vitaminK
                        FROM foods AS f
                        LEFT OUTER JOIN foodGroups AS fg
                        ON f.foodGroup = fg.code
                        WHERE id = @id
                        ORDER BY food ASC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", id));
            NewFood x = new NewFood();
           
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.food = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.foodGroup.code = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.foodGroup.title = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.foodGroup.parent = GetParentGroup(connection, x.foodGroup.code);
                x.foodGroupVitaminLost = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.thermalTreatments = GetThermalTreatments(connection, x.foodGroupVitaminLost);
                x.meal.code = "B";
                x.meal.title = Meals.GetMealTitle(x.meal.code, connection);
                //x.meal.title = lang != null ? Meals.GetMealTitle(x.meal.code, connection) : "";
                x.quantity = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt32(5);
                x.unit = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                x.mass = reader.GetValue(7) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(7));
                x.energy = reader.GetValue(8) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(8));
                x.carbohydrates = reader.GetValue(9) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(9));
                x.proteins = reader.GetValue(10) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(10));
                x.fats = reader.GetValue(11) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(11));


                //x.servings = GetServings(connection, x.id);

                x.servings.cerealsServ = reader.GetValue(12) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(12));
                x.servings.vegetablesServ = reader.GetValue(13) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(13));
                x.servings.fruitServ = reader.GetValue(14) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(14));
                x.servings.meatServ = reader.GetValue(15) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(15));
                x.servings.milkServ = reader.GetValue(16) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(16));
                x.servings.fatsServ = reader.GetValue(17) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(17));
                x.servings.otherFoodsServ = reader.GetValue(18) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(18));
                x.servings.otherFoodsEnergy = x.servings.otherFoodsServ > 0 ? x.energy : 0;
                //   x.foodGroupServ = GetFoodGroupServ(connection, x);
                x.starch = reader.GetValue(19) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(19));
                x.totalSugar = reader.GetValue(20) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(20));
                x.glucose = reader.GetValue(21) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(21));
                x.fructose = reader.GetValue(22) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(22));
                x.saccharose = reader.GetValue(23) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(23));
                x.maltose = reader.GetValue(24) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(24));
                x.lactose = reader.GetValue(25) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(25));
                x.fibers = reader.GetValue(26) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(26));
                x.saturatedFats = reader.GetValue(27) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(27));
                x.monounsaturatedFats = reader.GetValue(28) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(28));
                x.polyunsaturatedFats = reader.GetValue(29) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(29));
                x.trifluoroaceticAcid = reader.GetValue(30) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(30));
                x.cholesterol = reader.GetValue(31) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(31));
                x.sodium = reader.GetValue(32) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(32));
                x.potassium = reader.GetValue(33) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(33));
                x.calcium = reader.GetValue(34) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(34));
                x.magnesium = reader.GetValue(35) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(35));
                x.phosphorus = reader.GetValue(36) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(36));
                x.iron = reader.GetValue(37) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(37));
                x.copper = reader.GetValue(38) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(38));
                x.zinc = reader.GetValue(39) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(39));
                x.chlorine = reader.GetValue(40) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(40));
                x.manganese = reader.GetValue(41) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(41));
                x.selenium = reader.GetValue(42) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(42));
                x.iodine = reader.GetValue(43) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(43));
                x.retinol = reader.GetValue(44) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(44));
                x.carotene = reader.GetValue(45) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(45));
                x.vitaminD = reader.GetValue(46) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(46));
                x.vitaminE = reader.GetValue(47) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(47));
                x.vitaminB1 = reader.GetValue(48) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(48));
                x.vitaminB2 = reader.GetValue(49) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(49));
                x.vitaminB3 = reader.GetValue(50) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(50));
                x.vitaminB6 = reader.GetValue(51) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(51));
                x.vitaminB12 = reader.GetValue(52) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(52));
                x.folate = reader.GetValue(53) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(53));
                x.pantothenicAcid = reader.GetValue(54) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(54));
                x.biotin = reader.GetValue(55) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(55));
                x.vitaminC = reader.GetValue(56) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(56));
                x.vitaminK = reader.GetValue(57) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(57));
            }
            connection.Close();

            Prices.NewPrice p = new Prices.NewPrice();
            x.price = p.GetUnitPrice(userId, x.id);
            x.price.value = (x.price.value * x.mass)/1000;

            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        }
        catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string GetTotals(List<NewFood> selectedFoods, List<Meals.NewMeal> meals) {
        Totals x = new Totals();
        x.mass = Math.Round(selectedFoods.Sum(a => a.mass), 1);
        x.energy = Math.Round(selectedFoods.Sum(a => a.energy), 1);
        x.carbohydrates = Math.Round(selectedFoods.Sum(a => a.carbohydrates), 1);
        x.carbohydratesPercentage = Math.Round(GetNutrientPercentage(selectedFoods, x.carbohydrates), 1);
        x.proteins = Math.Round(selectedFoods.Sum(a => a.proteins), 1);
        x.proteinsPercentage = Math.Round(GetNutrientPercentage(selectedFoods, x.proteins), 1);
        x.fats = Math.Round(selectedFoods.Sum(a => a.fats), 1);
        x.fatsPercentage = Math.Round(GetNutrientPercentage(selectedFoods, x.fats), 1);

        x.servings.cerealsServ = Math.Round(selectedFoods.Sum(a => a.servings.cerealsServ),1);
        x.servings.vegetablesServ = Math.Round(selectedFoods.Sum(a => a.servings.vegetablesServ),1);
        x.servings.fruitServ = Math.Round(selectedFoods.Sum(a => a.servings.fruitServ),1);
        x.servings.meatServ = Math.Round(selectedFoods.Sum(a => a.servings.meatServ),1);
        x.servings.milkServ = Math.Round(selectedFoods.Sum(a => a.servings.milkServ),1);
        x.servings.fatsServ = Math.Round(selectedFoods.Sum(a => a.servings.fatsServ),1);
        x.servings.otherFoodsServ = Math.Round(selectedFoods.Sum(a => a.servings.otherFoodsServ),1);
        x.servings.otherFoodsEnergy = Math.Round(selectedFoods.Where(a => a.servings.otherFoodsServ > 0).Sum(a => a.energy),1);

        x.mealsTotalEnergy = GetMealsTotalEnergy(selectedFoods, meals);
        x.starch = Math.Round(selectedFoods.Sum(a => a.starch), 1);
        x.totalSugar = Math.Round(selectedFoods.Sum(a => a.totalSugar), 1);
        x.glucose = Math.Round(selectedFoods.Sum(a => a.glucose), 1);
        x.fructose = Math.Round(selectedFoods.Sum(a => a.fructose), 1);
        x.saccharose = Math.Round(selectedFoods.Sum(a => a.saccharose), 1);
        x.maltose = Math.Round(selectedFoods.Sum(a => a.maltose), 1);
        x.lactose = Math.Round(selectedFoods.Sum(a => a.lactose), 1);
        x.fibers = Math.Round(selectedFoods.Sum(a => a.fibers), 1);
        x.saturatedFats = Math.Round(selectedFoods.Sum(a => a.saturatedFats), 1);
        x.monounsaturatedFats = Math.Round(selectedFoods.Sum(a => a.monounsaturatedFats), 1);
        x.polyunsaturatedFats = Math.Round(selectedFoods.Sum(a => a.polyunsaturatedFats), 1);
        x.trifluoroaceticAcid = Math.Round(selectedFoods.Sum(a => a.trifluoroaceticAcid), 1);
        x.cholesterol = Math.Round(selectedFoods.Sum(a => a.cholesterol), 1);
        x.sodium = Math.Round(selectedFoods.Sum(a => a.sodium), 1);
        x.potassium = Math.Round(selectedFoods.Sum(a => a.potassium), 1);
        x.calcium = Math.Round(selectedFoods.Sum(a => a.calcium), 1);
        x.magnesium = Math.Round(selectedFoods.Sum(a => a.magnesium), 1);
        x.phosphorus = Math.Round(selectedFoods.Sum(a => a.phosphorus), 1);
        x.iron = Math.Round(selectedFoods.Sum(a => a.iron), 1);
        x.copper = Math.Round(selectedFoods.Sum(a => a.copper), 1);
        x.zinc = Math.Round(selectedFoods.Sum(a => a.zinc), 1);
        x.chlorine = Math.Round(selectedFoods.Sum(a => a.chlorine), 1);
        x.manganese = Math.Round(selectedFoods.Sum(a => a.manganese), 1);
        x.selenium = Math.Round(selectedFoods.Sum(a => a.selenium), 1);
        x.iodine = Math.Round(selectedFoods.Sum(a => a.iodine), 1);
        x.retinol = Math.Round(selectedFoods.Sum(a => a.retinol), 1);
        x.carotene = Math.Round(selectedFoods.Sum(a => a.carotene), 1);
        x.vitaminD = Math.Round(selectedFoods.Sum(a => a.vitaminD), 1);
        x.vitaminE = Math.Round(selectedFoods.Sum(a => a.vitaminE), 1);
        x.vitaminB1 = Math.Round(selectedFoods.Sum(a => a.vitaminB1), 1);
        x.vitaminB2 = Math.Round(selectedFoods.Sum(a => a.vitaminB2), 1);
        x.vitaminB3 = Math.Round(selectedFoods.Sum(a => a.vitaminB3), 1);
        x.vitaminB6 = Math.Round(selectedFoods.Sum(a => a.vitaminB6), 1);
        x.vitaminB12 = Math.Round(selectedFoods.Sum(a => a.vitaminB12), 1);
        x.folate = Math.Round(selectedFoods.Sum(a => a.folate), 1);
        x.pantothenicAcid = Math.Round(selectedFoods.Sum(a => a.pantothenicAcid), 1);
        x.biotin = Math.Round(selectedFoods.Sum(a => a.biotin), 1);
        x.vitaminC = Math.Round(selectedFoods.Sum(a => a.vitaminC), 1);
        x.vitaminK = Math.Round(selectedFoods.Sum(a => a.vitaminK), 1);

        x.price.value = Math.Round(selectedFoods.Sum(a => a.price.value), 1);
        //TODO price currency
       // if (selectedFoods.Count > 0) {
           // x.price.currency = selectedFoods.Select(a => a.price.currency = "");
            //x.price.unit = selectedFoods[0].price.unit;
       // }

        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    //TODO
    [WebMethod]
    public string GetRecommendations(ClientsData.NewClientData client) {
        Recommendations x = new Recommendations();
        Calculations c = new Calculations();
        x.energy = c.RecommendedEnergyIntake(client);

        //TODO
        x.carbohydratesMin = Convert.ToInt32(client.weight * 4);
        x.carbohydratesMax = Convert.ToInt32(client.weight * 4);  //TODO
        x.carbohydratesPercentageMin = client.diet.carbohydratesMin;
        x.carbohydratesPercentageMax = client.diet.carbohydratesMax;
        x.proteinsMin = Convert.ToInt32(client.weight * 0.8);
        x.proteinsMax = Convert.ToInt32(client.weight * 0.8);  //TODO
        x.proteinsPercentageMin = client.diet.proteinsMin;
        x.proteinsPercentageMax = client.diet.proteinsMax;
        x.fatsMin = 90;  
        x.fatsMax = 90;  //TODO
        x.fatsPercentageMin = client.diet.fatsMin;  //TODO
        x.fatsPercentageMax = client.diet.fatsMax;  //TODO

        x.servings = GetRecommendedServings(client, x.energy);

        //x.servings.cerealsServ = GetCerealsServ(client, x.energy); // Convert.ToInt32(x.energy * 0.0047);
        //x.servings.vegetablesServ = Convert.ToInt32((x.energy * 0.0029) - (x.energy / 1100 - 1) - (x.energy / 4500) + 0.65);
        //x.servings.fruitServ = Convert.ToInt32(x.energy * 0.00196);
        //x.servings.meatServ = Convert.ToInt32(x.energy * 0.0025 - (x.energy / 1700 - 2.1));
        //x.servings.milkServ = Convert.ToInt32(x.energy * 0.0015 - (x.energy / 1800 - 1));
        //x.servings.fatsServ = Convert.ToInt32(x.energy * 0.004 + x.energy / 1800 - 1);
        //x.servings.otherFoodsEnergy = Convert.ToInt32((x.energy * 0.107) * (x.energy / 1800));  //max

        //x.cerealsServ =  Convert.ToInt32(x.energy * 0.0047);
        //x.vegetablesServ = Convert.ToInt32((x.energy * 0.0029) - (x.energy / 1100 - 1) - (x.energy / 4500) + 0.65);
        //x.fruitServ = Convert.ToInt32(x.energy * 0.00196);
        //x.meatServ = Convert.ToInt32(x.energy * 0.0025 - (x.energy / 1700 - 2.1));
        //x.milkServ = Convert.ToInt32(x.energy * 0.0015 - (x.energy / 1800 - 1));
        //x.fatsServ = Convert.ToInt32(x.energy * 0.004 + x.energy / 1800 - 1);
        //x.otherFoodsEnergyMax = Convert.ToInt32((x.energy * 0.107) * (x.energy / 1800));

        x.mealsRecommendationEnergy = GetMealsRecommendations(client.meals, x.energy);

        //TODO - test
                                            // MDA,  UI,  RDA  
        x.starch = GetParameterRecommendation(null, null, null);
        x.totalSugar = GetParameterRecommendation(null, null, null);
        x.glucose = GetParameterRecommendation(null, null, null);
        x.fructose = GetParameterRecommendation(null, null, null);
        x.saccharose = GetParameterRecommendation(null, null, null);
        x.maltose = GetParameterRecommendation(null, null, null);
        x.lactose = GetParameterRecommendation(null, null, null);
        x.fibers = GetParameterRecommendation(null, null, 25);
        x.saturatedFats = GetParameterRecommendation(null, Math.Round(x.energy * 0.1, 1), null);
        x.monounsaturatedFats = GetParameterRecommendation(null, Math.Round(x.energy * 0.2, 1), Math.Round(x.energy * 0.15, 1));
        x.polyunsaturatedFats = GetParameterRecommendation(null, Math.Round(x.energy * 0.11, 1), Math.Round(x.energy * 0.8, 1));
        x.trifluoroaceticAcid = GetParameterRecommendation(null, Math.Round(x.energy * 0.02, 1), null);
        x.cholesterol = GetParameterRecommendation(null, 300, null);
        x.sodium = GetParameterRecommendation(500, 2400, null);
        x.potassium = GetParameterRecommendation(2000, null, null);
        x.calcium = GetParameterRecommendation(null, 1500, 800);
        x.magnesium = GetParameterRecommendation(null, 700, 375);
        x.phosphorus = GetParameterRecommendation(null, 1400, 700);
        x.iron = GetParameterRecommendation(null, 30, 14);
        x.copper = GetParameterRecommendation(null, 3, 1);
        x.zinc = GetParameterRecommendation(null, 15, 10);
        x.chlorine = GetParameterRecommendation(800, null, null);
        x.manganese = GetParameterRecommendation(null, 4, 2);
        x.selenium = GetParameterRecommendation(null, 100, 55);
        x.iodine = GetParameterRecommendation(null, 225, 150);
        x.retinol = GetParameterRecommendation(null, 1500, 800);
        x.carotene = GetParameterRecommendation(null, null, null);
        x.vitaminD = GetParameterRecommendation(null, 10, 5);
        x.vitaminE = GetParameterRecommendation(null, 100, 12);
        x.vitaminB1 = GetParameterRecommendation(null, 4, 1.1);
        x.vitaminB2 = GetParameterRecommendation(null, 4, 1.4);
        x.vitaminB3 = GetParameterRecommendation(null, 35, 16);
        x.vitaminB6 = GetParameterRecommendation(null, 6, 1.4);
        x.vitaminB12 = GetParameterRecommendation(null, 9, 2.5);
        x.folate = GetParameterRecommendation(null, 600, 200);
        x.pantothenicAcid = GetParameterRecommendation(null, 15, 6);
        x.biotin = GetParameterRecommendation(null, 100, 50);
        x.vitaminC = GetParameterRecommendation(null, 500, 80);
        x.vitaminK = GetParameterRecommendation(null, 100, 75);

        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string ChangeFoodQuantity(NewFood initFood, double newQuantity, double newMass, string type) {
        NewFood x = new NewFood();
        double k = 1;
        x = initFood;

        switch (type) {
            case "quantity":
                k = newQuantity / initFood.quantity;
                x.quantity = newQuantity;
                x.mass = Math.Round(initFood.mass * k, 1);
                break;
            case "mass":
                k = (newMass / initFood.mass);
                x.mass = newMass;
                x.quantity = Math.Round(initFood.quantity * k, 1);
                break;
            default:
                break;
        }

        x.energy = Math.Round(initFood.energy * k, 1);
        x.carbohydrates = Math.Round(initFood.carbohydrates * k, 1);
        x.proteins = Math.Round(initFood.proteins * k, 1);
        x.fats = Math.Round(initFood.fats * k, 1);
        x.servings.cerealsServ = Math.Round(initFood.servings.cerealsServ * k, 3);
        x.servings.vegetablesServ = Math.Round(initFood.servings.vegetablesServ * k, 3);
        x.servings.fruitServ = Math.Round(initFood.servings.fruitServ * k, 3);
        x.servings.meatServ = Math.Round(initFood.servings.meatServ * k, 3);
        x.servings.milkServ = Math.Round(initFood.servings.milkServ * k, 3);
        x.servings.fatsServ = Math.Round(initFood.servings.fatsServ * k, 3);
        x.servings.otherFoodsServ = Math.Round(initFood.servings.otherFoodsServ * k, 3);
        x.servings.otherFoodsEnergy = Math.Round(initFood.servings.otherFoodsEnergy * k, 3);
        x.starch = Math.Round(initFood.starch * k, 1);
        x.totalSugar = Math.Round(initFood.totalSugar * k, 1);
        x.glucose = Math.Round(initFood.glucose * k, 1);
        x.fructose = Math.Round(initFood.fructose * k, 1);
        x.saccharose = Math.Round(initFood.saccharose * k, 1);
        x.maltose = Math.Round(initFood.maltose * k, 1);
        x.lactose = Math.Round(initFood.lactose * k, 1);
        x.fibers = Math.Round(initFood.fibers * k, 1);
        x.saturatedFats = Math.Round(initFood.saturatedFats * k, 1);
        x.monounsaturatedFats = Math.Round(initFood.monounsaturatedFats * k, 1);
        x.polyunsaturatedFats = Math.Round(initFood.polyunsaturatedFats * k, 1);
        x.trifluoroaceticAcid = Math.Round(initFood.trifluoroaceticAcid * k, 1);
        x.cholesterol = Math.Round(initFood.cholesterol * k, 1);
        x.sodium = Math.Round(initFood.sodium * k, 1);
        x.potassium = Math.Round(initFood.potassium * k, 1);
        x.calcium = Math.Round(initFood.calcium * k, 1);
        x.magnesium = Math.Round(initFood.magnesium * k, 1);
        x.phosphorus = Math.Round(initFood.phosphorus * k, 1);
        x.iron = Math.Round(initFood.iron * k, 1);
        x.copper = Math.Round(initFood.copper * k, 1);
        x.zinc = Math.Round(initFood.zinc * k, 1);
        x.chlorine = Math.Round(initFood.chlorine * k, 1);
        x.manganese = Math.Round(initFood.manganese * k, 1);
        x.selenium = Math.Round(initFood.selenium * k, 1);
        x.iodine = Math.Round(initFood.iodine * k, 1);
        x.retinol = Math.Round(initFood.retinol * k, 1);
        x.carotene = Math.Round(initFood.carotene * k, 1);
        x.vitaminD = Math.Round(initFood.vitaminD * k, 1);
        x.vitaminE = Math.Round(initFood.vitaminE * k, 1);
        x.vitaminB1 = Math.Round(initFood.vitaminB1 * k, 1);
        x.vitaminB2 = Math.Round(initFood.vitaminB2 * k, 1);
        x.vitaminB3 = Math.Round(initFood.vitaminB3 * k, 1);
        x.vitaminB6 = Math.Round(initFood.vitaminB6 * k, 1);
        x.vitaminB12 = Math.Round(initFood.vitaminB12 * k, 1);
        x.folate = Math.Round(initFood.folate * k, 1);
        x.pantothenicAcid = Math.Round(initFood.pantothenicAcid * k, 1);
        x.biotin = Math.Round(initFood.biotin * k, 1);
        x.vitaminC = Math.Round(initFood.vitaminC * k, 1);
        x.vitaminK = Math.Round(initFood.vitaminK * k, 1);

        //TODO
        x.price.value = Math.Round(initFood.price.value * k, 1);

        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;

    }
    
    [WebMethod]
    public string GetUnits() {
      return JsonConvert.SerializeObject(Units(), Formatting.Indented);
    }

    //TODO multile consumers
    [WebMethod]
    public string ChangeNumberOfConsumers(List<NewFood> foods, int number){
        List<NewFood> xx = new List<NewFood>();
        foreach(var f in foods) {
            NewFood x = new NewFood();


            //          public string id { get; set; }
            //public string food { get; set; }

            //public Group foodGroup = new Group();
            //public string foodGroupVitaminLost { get; set; }
            //public List<ThermalTreatment> thermalTreatments { get; set; }

            //public CodeTitle meal = new CodeTitle();

            x.id = f.id;
            x.food = f.food;
            x.foodGroup = f.foodGroup;
            x.foodGroupVitaminLost = f.foodGroupVitaminLost;
            x.thermalTreatments = f.thermalTreatments;
            x.meal = f.meal;
            x.quantity = Math.Round(f.quantity * number, 1);
            x.unit = f.unit;
            x.mass = Math.Round(f.mass * number, 1);
            x.energy = Math.Round(f.energy * number, 1);
            x.carbohydrates = Math.Round(f.carbohydrates * number, 1);
            x.proteins = Math.Round(f.proteins * number, 1);
            x.fats = Math.Round(f.fats * number, 1);
            x.servings.cerealsServ = Math.Round(f.servings.cerealsServ * number, 1);
            x.servings.vegetablesServ = Math.Round(f.servings.vegetablesServ * number, 1);
            x.servings.fruitServ = Math.Round(f.servings.fruitServ * number, 1);
            x.servings.meatServ = Math.Round(f.servings.meatServ * number, 1);
            x.servings.milkServ = Math.Round(f.servings.milkServ * number, 1);
            x.servings.fatsServ = Math.Round(f.servings.fatsServ * number, 1);
            x.servings.otherFoodsServ = Math.Round(f.servings.otherFoodsServ * number, 1);
            x.servings.otherFoodsEnergy = Math.Round(f.servings.otherFoodsEnergy * number, 1);
            x.starch = Math.Round(f.starch * number, 1);
            x.totalSugar = Math.Round(f.totalSugar * number, 1);
            x.glucose = Math.Round(f.glucose * number, 1);
            x.fructose = Math.Round(f.fructose * number, 1);
            x.saccharose = Math.Round(f.saccharose * number, 1);
            x.maltose = Math.Round(f.maltose * number, 1);
            x.lactose = Math.Round(f.lactose * number, 1);
            x.fibers = Math.Round(f.fibers * number, 1);
            x.saturatedFats = Math.Round(f.saturatedFats * number, 1);
            x.monounsaturatedFats = Math.Round(f.monounsaturatedFats * number, 1);
            x.polyunsaturatedFats = Math.Round(f.polyunsaturatedFats * number, 1);
            x.trifluoroaceticAcid = Math.Round(f.trifluoroaceticAcid * number, 1);
            x.cholesterol = Math.Round(f.cholesterol * number, 1);
            x.sodium = Math.Round(f.sodium * number, 1);
            x.potassium = Math.Round(f.potassium * number, 1);
            x.calcium = Math.Round(f.calcium * number, 1);
            x.magnesium = Math.Round(f.magnesium * number, 1);
            x.phosphorus = Math.Round(f.phosphorus * number, 1);
            x.iron = Math.Round(f.iron * number, 1);
            x.copper = Math.Round(f.copper * number, 1);
            x.zinc = Math.Round(f.zinc * number, 1);
            x.chlorine = Math.Round(f.chlorine * number, 1);
            x.manganese = Math.Round(f.manganese * number, 1);
            x.selenium = Math.Round(f.selenium * number, 1);
            x.iodine = Math.Round(f.iodine * number, 1);
            x.retinol = Math.Round(f.retinol * number, 1);
            x.carotene = Math.Round(f.carotene * number, 1);
            x.vitaminD = Math.Round(f.vitaminD * number, 1);
            x.vitaminE = Math.Round(f.vitaminE * number, 1);
            x.vitaminB1 = Math.Round(f.vitaminB1 * number, 1);
            x.vitaminB2 = Math.Round(f.vitaminB2 * number, 1);
            x.vitaminB3 = Math.Round(f.vitaminB3 * number, 1);
            x.vitaminB6 = Math.Round(f.vitaminB6 * number, 1);
            x.vitaminB12 = Math.Round(f.vitaminB12 * number, 1);
            x.folate = Math.Round(f.folate * number, 1);
            x.pantothenicAcid = Math.Round(f.pantothenicAcid * number, 1);
            x.biotin = Math.Round(f.biotin * number, 1);
            x.vitaminC = Math.Round(f.vitaminC * number, 1);
            x.vitaminK = Math.Round(f.vitaminK * number, 1);
            xx.Add(x);
        }

        string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
        return json;

    }

    #endregion

    #region Methods
    private string GetParentGroup(SQLiteConnection connection, string code) {
        string group = "";
        try {
            string sql = @"SELECT parent FROM foodGroups WHERE code = @code";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("code", code));
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                group = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
            }
        } catch (Exception e) { return ""; }
        return group;
    }

    private List<FoodGroup> GetFoodGroups(SQLiteConnection connection) {
        List<FoodGroup> xx = new List<FoodGroup>();
        try {
            string sql = @"SELECT code, title, parent, groupOrder
                        FROM foodGroups
                        ORDER BY groupOrder ASC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                FoodGroup x = new FoodGroup();
                x.group.code = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.group.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.parent = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.groupOrder = reader.GetValue(3) == DBNull.Value ? 0 : reader.GetInt32(3);
                xx.Add(x);
            }
        }
        catch (Exception e) { return null; }
        return xx;
    }

    public List<FoodGroup> GetMainFoodGroups(SQLiteConnection connection) {
        List<FoodGroup> xx = new List<FoodGroup>();
        try {
            connection.Open();
            string sql = @"SELECT code, title, parent, groupOrder
                        FROM foodGroups
                        WHERE parent = 'A'
                        ORDER BY groupOrder ASC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                FoodGroup x = new FoodGroup();
                x.group.code = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.group.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.parent = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.groupOrder = reader.GetValue(3) == DBNull.Value ? 0 : reader.GetInt32(3);
                xx.Add(x);
            }
            connection.Close();
        }
        catch (Exception e) { return null; }
        return xx;
    }

    private CodeTitle GetFoodGroupServ(SQLiteConnection connection, string code) {
        CodeTitle x = new CodeTitle();
        try {
            string sql = @"SELECT code, title
                        FROM codeBook
                        WHERE codeGroup = 'FGVL' AND code = @code";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("code", code));
            SQLiteDataReader reader = command.ExecuteReader();
          
            while (reader.Read()) {
                x.code = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
            }
        } catch (Exception e) { }
        return x;
    }

    private List<ThermalTreatment> GetThermalTreatments(SQLiteConnection connection, string foodGroupVitaminLost) {
        List<ThermalTreatment> xx = new List<ThermalTreatment>();
        try {
            string sql = @"SELECT vl.foodGroupVitaminLost, vl.thermalTreatment, cb.title, vl.vitaminE, vl.vitaminB1, vl.vitaminB2, vl.vitaminB3, vl.vitaminB6, vl.vitaminB12, vl.folate, vl.pantothenicAcid, vl.biotin, vl.vitaminC
                        FROM vitaminLost AS vl
                        LEFT OUTER JOIN codeBook AS cb
                        ON vl.thermalTreatment = cb.code AND cb.codeGroup = 'TT'
                        WHERE vl.foodGroupVitaminLost = @foodGroupVitaminLost
                        ORDER BY cb.title ASC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("foodGroupVitaminLost", foodGroupVitaminLost));
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                ThermalTreatment x = new ThermalTreatment();
                x.foodGroupVitaminLost = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.thermalTreatment.code = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.thermalTreatment.title = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.isSelected = false;
                x.vitaminE = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                x.vitaminB1 = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
                x.vitaminB2 = reader.GetValue(5) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(5));
                x.vitaminB3 = reader.GetValue(6) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(6));
                x.vitaminB6 = reader.GetValue(7) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(7));
                x.vitaminB12 = reader.GetValue(8) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(8));
                x.folate = reader.GetValue(9) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(9));
                x.pantothenicAcid = reader.GetValue(10) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(10));
                x.biotin = reader.GetValue(11) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(11));
                x.vitaminC = reader.GetValue(12) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(12));
                xx.Add(x);
            }
        } catch (Exception e) { return null; }
        return xx;
    }

    private double GetNutrientPercentage(List<NewFood> selectedFoods, double nutrient) {
        double percentage = 0;
        double totalCarbohydrates = selectedFoods.Sum(a => a.carbohydrates);
        double totalProteins = selectedFoods.Sum(a => a.proteins);
        double totalFats = selectedFoods.Sum(a => a.fats);
        double totalNutrients = totalCarbohydrates + totalProteins + totalFats;
        if(nutrient > 0 || totalNutrients > 0) {
            percentage = (nutrient / totalNutrients) * 100;
        }
        return percentage;
    }

    private List<MealsTotalEnergy> GetMealsTotalEnergy(List<NewFood> selectedFoods, List<Meals.NewMeal> meals) {
      List<MealsTotalEnergy> xx = new List<MealsTotalEnergy>();
        foreach (var obj in meals) {
            MealsTotalEnergy x = new MealsTotalEnergy();
            x.meal.code = obj.code;
            x.meal.energy = selectedFoods.Where(a => a.meal.code == obj.code).Sum(a => a.energy);
            x.meal.energyPercentage = (x.meal.energy / selectedFoods.Sum(a => a.energy)) * 100;
            xx.Add(x);
        }
        return xx;
    }

    //TODO
    private List<MealsRecommendationEnergy> GetMealsRecommendations(List<Meals.NewMeal> meals, int energy) {
        List<MealsRecommendationEnergy> xx = new List<MealsRecommendationEnergy>();
        int counter = 0;
        foreach (var obj in meals) {
            counter++;
            MealsRecommendationEnergy x = new MealsRecommendationEnergy();
            x.meal.code = obj.code;
            x.meal.energyMinPercentage = GetMealRecommendationPercentage(meals, counter).min;
            x.meal.energyMaxPercentage = GetMealRecommendationPercentage(meals, counter).max;
            x.meal.energyMin = Convert.ToInt32(x.meal.energyMinPercentage * 0.01 * energy);
            x.meal.energyMax = Convert.ToInt32(x.meal.energyMaxPercentage * 0.01 * energy);
            xx.Add(x);
        }
        return xx;
    }

    private MealEnergy GetMealRecommendationPercentage(List<Meals.NewMeal> meals, int counter) {
        MealEnergy x = new MealEnergy();
        //1 case all meals
        if (meals[0].isSelected == true &&
            meals[1].isSelected == true &&
            meals[2].isSelected == true &&
            meals[3].isSelected == true &&
            meals[4].isSelected == true &&
            meals[5].isSelected == true) {
            switch (counter) {
                case 1:
                    x.min = 20;
                    x.max = 25;
                    break;
                case 2:
                    x.min = 5;
                    x.max = 10;
                    break;
                case 3:
                    x.min = 30;
                    x.max = 40;
                    break;
                case 4:
                    x.min = 5;
                    x.max = 10;
                    break;
                case 5:
                    x.min = 20;
                    x.max = 23;
                    break;
                case 6:
                    x.min = 2;
                    x.max = 5;
                    break;
                default:
                    x.min = 0;
                    x.max = 0;
                    break;
            }
        }

        //2 case exclude meal before sleep
        if (meals[0].isSelected == true &&
            meals[1].isSelected == true &&
            meals[2].isSelected == true &&
            meals[3].isSelected == true &&
            meals[4].isSelected == true &&
            meals[5].isSelected == false) {
            switch (counter) {
                case 1:
                    x.min = 20;
                    x.max = 25;
                    break;
                case 2:
                    x.min = 5;
                    x.max = 10;
                    break;
                case 3:
                    x.min = 30;
                    x.max = 40;
                    break;
                case 4:
                    x.min = 5;
                    x.max = 10;
                    break;
                case 5:
                    x.min = 20;
                    x.max = 25;
                    break;
                case 6:
                    x.min = 0;
                    x.max = 0;
                    break;
                default:
                    x.min = 0;
                    x.max = 0;
                    break;
            }
        }

        //3 cas exlude morning snack and meal before sleep
        if (meals[0].isSelected == true &&
            meals[1].isSelected == false &&
            meals[2].isSelected == true &&
            meals[3].isSelected == true &&
            meals[4].isSelected == true &&
            meals[5].isSelected == false) {
            switch (counter) {
                case 1:
                    x.min = 25;
                    x.max = 35;
                    break;
                case 2:
                    x.min = 0;
                    x.max = 0;
                    break;
                case 3:
                    x.min = 30;
                    x.max = 40;
                    break;
                case 4:
                    x.min = 5;
                    x.max = 10;
                    break;
                case 5:
                    x.min = 20;
                    x.max = 25;
                    break;
                case 6:
                    x.min = 0;
                    x.max = 0;
                    break;
                default:
                    x.min = 0;
                    x.max = 0;
                    break;
            }
        }

        //3 cas exlude morning snack, afternoon snack and meal before sleep
        if (meals[0].isSelected == true &&
            meals[1].isSelected == false &&
            meals[2].isSelected == true &&
            meals[3].isSelected == false &&
            meals[4].isSelected == true &&
            meals[5].isSelected == false) {
            switch (counter){
                case 1:
                    x.min = 25;
                    x.max = 35;
                    break;
                case 2:
                    x.min = 0;
                    x.max = 0;
                    break;
                case 3:
                    x.min = 35;
                    x.max = 45;
                    break;
                case 4:
                    x.min = 0;
                    x.max = 0;
                    break;
                case 5:
                    x.min = 25;
                    x.max = 30;
                    break;
                case 6:
                    x.min = 0;
                    x.max = 0;
                    break;
                default:
                    x.min = 0;
                    x.max = 0;
                    break;
            }
        }
        return x;
    }

    private ParameterRecommendation GetParameterRecommendation(double? mda, double? ui, double? rda) {
        ParameterRecommendation x = new ParameterRecommendation();
        x.mda = mda;
        x.ui = ui;
        x.rda = rda;
        return x;
    }

    private List<string> Units() {
        try {
          //  string db = ConfigurationManager.AppSettings["AppDataBase"];
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/"  + dataBase));
            //SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + lang + "/" + db));
            connection.Open();
            string sql = @"SELECT DISTINCT unit FROM foods ORDER BY unit ASC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<string> xx = new List<string>();
            string x = "";
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                xx.Add(x);
            }
            connection.Close();
         // string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return xx;
        } catch (Exception e) { return new List<string>(); }
    }


    #region Servings
    private Servings GetRecommendedServings(ClientsData.NewClientData clientData, int tee) {
        Servings x = new Servings();
        switch (clientData.diet.id) {
            case "d8":  // Jelovnik u završnoj fazi deponiranja glikogena u mišiće kod sportaša
                x = DietD8(tee);
                break;
            case "d9": case "d14":  // Lagana dijeta, Dijeta kod kroničnih bolesti jetre
                x = DietD9(tee);
                break;
            case "d15": case "d16":  // Dijeta kod upalnih bolesti crijeva
                x = DietD15(tee);
                break;
            case "d18": case "d19": case "d20": case "d21": case "d22": // Dijabetička dijeta  d18, d19, d20, d21, d22, 
                x = DietD18(tee);
                break;
                 case "d23": case "d24":  // Laktoovo - vegetarijanska dijeta, Semivegetarijanska dijeta  d23, d24
                x = DietD23(tee);
                break;
            default:
                x = DietD1(tee);  //Normalna prehrana, Dijeta bez glutena ???  TODO
                //x.cerealsServ = Convert.ToInt32(tee * 0.005);
                //x.vegetablesServ = Convert.ToInt32((tee * 0.0029) - (tee / 1100.0 - 1) - (tee / 4500.0) + 0.65);
                //x.fruitServ = Convert.ToInt32(tee * 0.00185);
                //x.meatServ = Convert.ToInt32(tee * 0.0024 - (tee / 1700.0 - 2.1));
                //x.milkServ = Convert.ToInt32(tee * 0.0015 - (tee / 1800.0 - 1));
                //x.fatsServ = Convert.ToInt32(tee * 0.0034 + tee / 1800.0 - 1);
                //x.otherFoodsEnergy = Convert.ToInt32((tee * 0.107) * (tee / 1800.0));  //max
                break;
        }
        return x;
    }

    private Servings DietD1(int tee) {
        //JNormalna prehrana, Dijeta bez glutena ???  TODO
        Servings x = new Servings();
        if (tee == 0) {
            x.cerealsServ = 0;
            x.vegetablesServ = 0;
            x.fruitServ = 0;
            x.meatServ = 0;
            x.milkServ = 0;
            x.fatsServ = 0;
        }
        if (tee > 0 && tee <= 1900 ) {
            x.cerealsServ = 8;
            x.vegetablesServ = 5;
            x.fruitServ = 4;
            x.meatServ = 6;
            x.milkServ = 2;
            x.fatsServ = 6;
        }
        if (tee > 1900 && tee <= 2100) {
            x.cerealsServ = 9;
            x.vegetablesServ = 5;
            x.fruitServ = 4;
            x.meatServ = 6;
            x.milkServ = 3;
            x.fatsServ = 7;
        }
        if (tee > 2100 && tee <= 2300) {
            x.cerealsServ = 11;
            x.vegetablesServ = 6;
            x.fruitServ = 4;
            x.meatServ = 6;
            x.milkServ = 3;
            x.fatsServ = 8;
        }
        if (tee > 2300 && tee <= 2500) {
            x.cerealsServ = 12;
            x.vegetablesServ = 6;
            x.fruitServ = 4;
            x.meatServ = 6;
            x.milkServ = 3;
            x.fatsServ = 9;
        }
        if (tee > 2500 && tee <= 2700) {
            x.cerealsServ = 13;
            x.vegetablesServ = 6;
            x.fruitServ = 5;
            x.meatServ = 7;
            x.milkServ = 3;
            x.fatsServ = 10;
        }
        if (tee > 2700 && tee <= 2900) {
            x.cerealsServ = 14;
            x.vegetablesServ = 6;
            x.fruitServ = 5;
            x.meatServ = 7;
            x.milkServ = 3;
            x.fatsServ = 11;
        }
        if (tee > 2900 && tee <= 3100) {
            x.cerealsServ = 15;
            x.vegetablesServ = 6;
            x.fruitServ = 6;
            x.meatServ = 8;
            x.milkServ = 3;
            x.fatsServ = 12;
        }
        if (tee > 3100 && tee <= 3300) {
            x.cerealsServ = 16;
            x.vegetablesServ = 6;
            x.fruitServ = 6;
            x.meatServ = 8;
            x.milkServ = 4;
            x.fatsServ = 13;
        }
        if (tee > 3300 && tee <= 3500) {
            x.cerealsServ = 17;
            x.vegetablesServ = 7;
            x.fruitServ = 6;
            x.meatServ = 8;
            x.milkServ = 4;
            x.fatsServ = 14;
        }
        if (tee > 3500 && tee <= 3700) {
            x.cerealsServ = 18;
            x.vegetablesServ = 7;
            x.fruitServ = 7;
            x.meatServ = 8;
            x.milkServ = 4;
            x.fatsServ = 14;
        }
        if (tee > 3700 && tee <= 3900) {
            x.cerealsServ = 19;
            x.vegetablesServ = 7;
            x.fruitServ = 7;
            x.meatServ = 9;
            x.milkServ = 4;
            x.fatsServ = 15;
        }
        if (tee > 3900) {
            x.cerealsServ = 20;
            x.vegetablesServ = 8;
            x.fruitServ = 8;
            x.meatServ = 9;
            x.milkServ = 4;
            x.fatsServ = 15;
        }

        x.otherFoodsEnergy = Convert.ToInt32((tee * 0.107) * (tee / 1800.0));
        return x;
    }
    private Servings DietD8(int tee) {
        //Jelovnik u završnoj fazi deponiranja glikogena u mišiće kod sportaša
        Servings x = new Servings();
        if (tee == 0) {
            x.cerealsServ = 0;
            x.vegetablesServ = 0;
            x.fruitServ = 0;
            x.meatServ = 0;
            x.milkServ = 0;
            x.fatsServ = 0;
        }
        if (tee > 0 && tee <= 1900 ) {
            x.cerealsServ = 17;
            x.vegetablesServ = 2;
            x.fruitServ = 2;
            x.meatServ = 0;
            x.milkServ = 1;
            x.fatsServ = 3;
        }
        if (tee > 1900 && tee <= 2100) {
            x.cerealsServ = 17;
            x.vegetablesServ = 4;
            x.fruitServ = 3;
            x.meatServ = 0;
            x.milkServ = 2;
            x.fatsServ = 3;
        }
        if (tee > 2100 && tee <= 2300) {
            x.cerealsServ = 17;
            x.vegetablesServ = 5;
            x.fruitServ = 4;
            x.meatServ = 1;
            x.milkServ = 2;
            x.fatsServ = 4;
        }
        if (tee > 2300 && tee <= 2500) {
            x.cerealsServ = 18;
            x.vegetablesServ = 5;
            x.fruitServ = 5;
            x.meatServ = 1;
            x.milkServ = 3;
            x.fatsServ = 4;
        }
        if (tee > 2500 && tee <= 2700) {
            x.cerealsServ = 18;
            x.vegetablesServ = 5;
            x.fruitServ = 5;
            x.meatServ = 1;
            x.milkServ = 3;
            x.fatsServ = 5;
        }
        if (tee > 2700 && tee <= 2900) {
            x.cerealsServ = 19;
            x.vegetablesServ = 6;
            x.fruitServ = 6;
            x.meatServ = 2;
            x.milkServ = 3;
            x.fatsServ = 5;
        }
        if (tee > 2900 && tee <= 3100) {
            x.cerealsServ = 19;
            x.vegetablesServ = 6;
            x.fruitServ = 7;
            x.meatServ = 2;
            x.milkServ = 3;
            x.fatsServ = 5;
        }
        if (tee > 3100 && tee <= 3300) {
            x.cerealsServ = 19;
            x.vegetablesServ = 6;
            x.fruitServ = 7;
            x.meatServ = 3;
            x.milkServ = 3;
            x.fatsServ = 6;
        }
        if (tee > 3300 && tee <= 3500) {
            x.cerealsServ = 19;
            x.vegetablesServ = 7;
            x.fruitServ = 7;
            x.meatServ = 4;
            x.milkServ = 3;
            x.fatsServ = 7;
        }
        if (tee > 3500 && tee <= 3700) {
            x.cerealsServ = 20;
            x.vegetablesServ = 7;
            x.fruitServ = 7;
            x.meatServ = 4;
            x.milkServ = 3;
            x.fatsServ = 7;
        }
        if (tee > 3700 && tee <= 3900) {
            x.cerealsServ = 21;
            x.vegetablesServ = 8;
            x.fruitServ = 7;
            x.meatServ = 4;
            x.milkServ = 3;
            x.fatsServ = 7;
        }
        if (tee > 3900) {
            x.cerealsServ = 22;
            x.vegetablesServ = 9;
            x.fruitServ = 8;
            x.meatServ = 4;
            x.milkServ = 4;
            x.fatsServ = 7;
        }

        x.otherFoodsEnergy = Convert.ToInt32((tee * 0.107) * (tee / 1800.0));
        return x;
    }
    private Servings DietD9(int tee) {
        //Lagana dijeta
        Servings x = new Servings();
        if (tee == 0) {
            x.cerealsServ = 0;
            x.vegetablesServ = 0;
            x.fruitServ = 0;
            x.meatServ = 0;
            x.milkServ = 0;
            x.fatsServ = 0;
        }
        if (tee > 0 && tee <= 1900 ) {
            x.cerealsServ = 9;
            x.vegetablesServ = 6;
            x.fruitServ = 5;
            x.meatServ = 5;
            x.milkServ = 2;
            x.fatsServ = 5;
        }
        if (tee > 1900 && tee <= 2100) {
            x.cerealsServ = 11;
            x.vegetablesServ = 6;
            x.fruitServ = 5;
            x.meatServ = 5;
            x.milkServ = 2;
            x.fatsServ = 6;
        }
        if (tee > 2100 && tee <= 2300) {
            x.cerealsServ = 13;
            x.vegetablesServ = 7;
            x.fruitServ = 5;
            x.meatServ = 6;
            x.milkServ = 3;
            x.fatsServ = 7;
        }
        if (tee > 2300 && tee <= 2500) {
            x.cerealsServ = 14;
            x.vegetablesServ = 7;
            x.fruitServ = 6;
            x.meatServ = 7;
            x.milkServ = 3;
            x.fatsServ = 8;
        }
        if (tee > 2500 && tee <= 2700) {
            x.cerealsServ = 14;
            x.vegetablesServ = 8;
            x.fruitServ = 6;
            x.meatServ = 7;
            x.milkServ = 3;
            x.fatsServ = 9;
        }
        if (tee > 2700 && tee <= 2900) {
            x.cerealsServ = 16;
            x.vegetablesServ = 8;
            x.fruitServ = 7;
            x.meatServ = 8;
            x.milkServ = 3;
            x.fatsServ = 10;
        }
        if (tee > 2900 && tee <= 3100) {
            x.cerealsServ = 16;
            x.vegetablesServ = 9;
            x.fruitServ = 7;
            x.meatServ = 8;
            x.milkServ = 3;
            x.fatsServ = 10;
        }
        if (tee > 3100 && tee <= 3300) {
            x.cerealsServ = 17;
            x.vegetablesServ = 9;
            x.fruitServ = 8;
            x.meatServ = 8;
            x.milkServ = 3;
            x.fatsServ = 12;
        }
        if (tee > 3300 && tee <= 3500) {
            x.cerealsServ = 18;
            x.vegetablesServ = 9;
            x.fruitServ = 8;
            x.meatServ = 8;
            x.milkServ = 4;
            x.fatsServ = 12;
        }
        if (tee > 3500 && tee <= 3700) {
            x.cerealsServ = 19;
            x.vegetablesServ = 10;
            x.fruitServ = 8;
            x.meatServ = 9;
            x.milkServ = 4;
            x.fatsServ = 14;
        }
        if (tee > 3700 && tee <= 3900) {
            x.cerealsServ = 19;
            x.vegetablesServ = 11;
            x.fruitServ = 8;
            x.meatServ = 10;
            x.milkServ = 4;
            x.fatsServ = 15;
        }
        if (tee > 3900) {
            x.cerealsServ = 20;
            x.vegetablesServ = 11;
            x.fruitServ = 8;
            x.meatServ = 10;
            x.milkServ = 5;
            x.fatsServ = 15;
        }

        x.otherFoodsEnergy = Convert.ToInt32((tee * 0.107) * (tee / 1800.0));
        return x;
    }
    private Servings DietD15(int tee) {
        //Dijeta kod upalnih bolesti crijeva
        Servings x = new Servings();
        if (tee == 0) {
            x.cerealsServ = 0;
            x.vegetablesServ = 0;
            x.fruitServ = 0;
            x.meatServ = 0;
            x.milkServ = 0;
            x.fatsServ = 0;
        }
        if (tee > 0 && tee <= 1900 ) {
            x.cerealsServ = 8;
            x.vegetablesServ = 7;
            x.fruitServ = 6;
            x.meatServ = 6;
            x.milkServ = 1;
            x.fatsServ = 6;
        }
        if (tee > 1900 && tee <= 2100) {
            x.cerealsServ = 10;
            x.vegetablesServ = 7;
            x.fruitServ = 6;
            x.meatServ = 6;
            x.milkServ = 1;
            x.fatsServ = 7;
        }
        if (tee > 2100 && tee <= 2300) {
            x.cerealsServ = 12;
            x.vegetablesServ = 7;
            x.fruitServ = 6;
            x.meatServ = 6;
            x.milkServ = 1;
            x.fatsServ = 7;
        }
        if (tee > 2300 && tee <= 2500) {
            x.cerealsServ = 14;
            x.vegetablesServ = 8;
            x.fruitServ = 6;
            x.meatServ = 7;
            x.milkServ = 1;
            x.fatsServ = 10;
        }
        if (tee > 2500 && tee <= 2700) {
            x.cerealsServ = 16;
            x.vegetablesServ = 9;
            x.fruitServ = 7;
            x.meatServ = 8;
            x.milkServ = 1;
            x.fatsServ = 11;
        }
        if (tee > 2700 && tee <= 2900) {
            x.cerealsServ = 16;
            x.vegetablesServ = 8;
            x.fruitServ = 7;
            x.meatServ = 8;
            x.milkServ = 1;
            x.fatsServ = 10;
        }
        if (tee > 2900 && tee <= 3100) {
            x.cerealsServ = 17;
            x.vegetablesServ = 9;
            x.fruitServ = 8;
            x.meatServ = 9;
            x.milkServ = 1;
            x.fatsServ = 12;
        }
        if (tee > 3100 && tee <= 3300) {
            x.cerealsServ = 18;
            x.vegetablesServ = 9;
            x.fruitServ = 8;
            x.meatServ = 9;
            x.milkServ = 2;
            x.fatsServ = 13;
        }
        if (tee > 3300 && tee <= 3500) {
            x.cerealsServ = 19;
            x.vegetablesServ = 10;
            x.fruitServ = 8;
            x.meatServ = 10;
            x.milkServ = 2;
            x.fatsServ = 15;
        }
        if (tee > 3500 && tee <= 3700) {
            x.cerealsServ = 20;
            x.vegetablesServ = 11;
            x.fruitServ = 9;
            x.meatServ = 10;
            x.milkServ = 2;
            x.fatsServ = 16;
        }
        if (tee > 3700 && tee <= 3900) {
            x.cerealsServ = 21;
            x.vegetablesServ = 11;
            x.fruitServ = 9;
            x.meatServ = 10;
            x.milkServ = 2;
            x.fatsServ = 17;
        }
        if (tee > 3900) {
            x.cerealsServ = 22;
            x.vegetablesServ = 12;
            x.fruitServ = 10;
            x.meatServ = 11;
            x.milkServ = 2;
            x.fatsServ = 18;
        }

        x.otherFoodsEnergy = Convert.ToInt32((tee * 0.107) * (tee / 1800.0));
        return x;
    }
    private Servings DietD18(int tee) {
        //Dijabetička dijeta  d18, d19, d20, d21, d22, 
        Servings x = new Servings();
        if (tee == 0) {
            x.cerealsServ = 0;
            x.vegetablesServ = 0;
            x.fruitServ = 0;
            x.meatServ = 0;
            x.milkServ = 0;
            x.fatsServ = 0;
        }
        if (tee > 0 && tee <= 1900 ) {
            x.cerealsServ = 10;
            x.vegetablesServ = 7;
            x.fruitServ = 5;
            x.meatServ = 6;
            x.milkServ = 2;
            x.fatsServ = 6;
        }
        if (tee > 1900 && tee <= 2100) {
            x.cerealsServ = 11;
            x.vegetablesServ = 7;
            x.fruitServ = 5;
            x.meatServ = 6;
            x.milkServ = 3;
            x.fatsServ = 7;
        }
        if (tee > 2100 && tee <= 2300) {
            x.cerealsServ = 12;
            x.vegetablesServ = 8;
            x.fruitServ = 5;
            x.meatServ = 6;
            x.milkServ = 3;
            x.fatsServ = 9;
        }
        if (tee > 2300 && tee <= 2500) {
            x.cerealsServ = 14;
            x.vegetablesServ = 8;
            x.fruitServ = 6;
            x.meatServ = 6;
            x.milkServ = 3;
            x.fatsServ = 9;
        }
        if (tee > 2500 && tee <= 2700) {
            x.cerealsServ = 16;
            x.vegetablesServ = 8;
            x.fruitServ = 7;
            x.meatServ = 8;
            x.milkServ = 3;
            x.fatsServ = 11;
        }
        if (tee > 2700 && tee <= 2900) {
            x.cerealsServ = 16;
            x.vegetablesServ = 8;
            x.fruitServ = 7;
            x.meatServ = 8;
            x.milkServ = 3;
            x.fatsServ = 13;
        }
        if (tee > 2900 && tee <= 3100) {
            x.cerealsServ = 16;
            x.vegetablesServ = 8;
            x.fruitServ = 8;
            x.meatServ = 10;
            x.milkServ = 4;
            x.fatsServ = 15;
        }
        if (tee > 3100 && tee <= 3300) {
            x.cerealsServ = 17;
            x.vegetablesServ = 8;
            x.fruitServ = 8;
            x.meatServ = 10;
            x.milkServ = 4;
            x.fatsServ = 15;
        }
        if (tee > 3300 && tee <= 3500) {
            x.cerealsServ = 18;
            x.vegetablesServ = 9;
            x.fruitServ = 8;
            x.meatServ = 10;
            x.milkServ = 5;
            x.fatsServ = 15;
        }
        if (tee > 3500 && tee <= 3700) {
            x.cerealsServ = 19;
            x.vegetablesServ = 10;
            x.fruitServ = 9;
            x.meatServ = 11;
            x.milkServ = 5;
            x.fatsServ = 16;
        }
        if (tee > 3700 && tee <= 3900) {
            x.cerealsServ = 20;
            x.vegetablesServ = 11;
            x.fruitServ = 10;
            x.meatServ = 12;
            x.milkServ = 5;
            x.fatsServ = 16;
        }
        if (tee > 3900) {
            x.cerealsServ = 21;
            x.vegetablesServ = 12;
            x.fruitServ = 10;
            x.meatServ = 12;
            x.milkServ = 5;
            x.fatsServ = 17;
        }

        x.otherFoodsEnergy = Convert.ToInt32((tee * 0.107) * (tee / 1800.0));
        return x;
    }
    private Servings DietD23(int tee) {
        //Laktoovo - vegetarijanska dijeta, Semivegetarijanska dijeta  d23, d24
        Servings x = new Servings();
        if (tee == 0) {
            x.cerealsServ = 0;
            x.vegetablesServ = 0;
            x.fruitServ = 0;
            x.meatServ = 0;
            x.milkServ = 0;
            x.fatsServ = 0;
        }
        if (tee > 0 && tee <= 1900 ) {
            x.cerealsServ = 10;
            x.vegetablesServ = 5;
            x.fruitServ = 4;
            x.meatServ = 7;
            x.milkServ = 2;
            x.fatsServ = 6;
        }
        if (tee > 1900 && tee <= 2100) {
            x.cerealsServ = 11;
            x.vegetablesServ = 5;
            x.fruitServ = 4;
            x.meatServ = 7;
            x.milkServ = 3;
            x.fatsServ = 7;
        }
        if (tee > 2100 && tee <= 2300) {
            x.cerealsServ = 13;
            x.vegetablesServ = 6;
            x.fruitServ = 4;
            x.meatServ = 7;
            x.milkServ = 3;
            x.fatsServ = 8;
        }
        if (tee > 2300 && tee <= 2500) {
            x.cerealsServ = 14;
            x.vegetablesServ = 6;
            x.fruitServ = 4;
            x.meatServ = 7;
            x.milkServ = 3;
            x.fatsServ = 9;
        }
        if (tee > 2500 && tee <= 2700) {
            x.cerealsServ = 15;
            x.vegetablesServ = 6;
            x.fruitServ = 5;
            x.meatServ = 8;
            x.milkServ = 3;
            x.fatsServ = 10;
        }
        if (tee > 2700 && tee <= 2900) {
            x.cerealsServ = 16;
            x.vegetablesServ = 6;
            x.fruitServ = 5;
            x.meatServ = 8;
            x.milkServ = 3;
            x.fatsServ = 11;
        }
        if (tee > 2900 && tee <= 3100) {
            x.cerealsServ = 17;
            x.vegetablesServ = 6;
            x.fruitServ = 6;
            x.meatServ = 9;
            x.milkServ = 3;
            x.fatsServ = 12;
        }
        if (tee > 3100 && tee <= 3300) {
            x.cerealsServ = 18;
            x.vegetablesServ = 6;
            x.fruitServ = 6;
            x.meatServ = 9;
            x.milkServ = 4;
            x.fatsServ = 13;
        }
        if (tee > 3300 && tee <= 3500) {
            x.cerealsServ = 19;
            x.vegetablesServ = 7;
            x.fruitServ = 6;
            x.meatServ = 9;
            x.milkServ = 4;
            x.fatsServ = 14;
        }
        if (tee > 3500 && tee <= 3700) {
            x.cerealsServ = 20;
            x.vegetablesServ = 7;
            x.fruitServ = 7;
            x.meatServ = 9;
            x.milkServ = 4;
            x.fatsServ = 14;
        }
        if (tee > 3700 && tee <= 3900) {
            x.cerealsServ = 21;
            x.vegetablesServ = 7;
            x.fruitServ = 7;
            x.meatServ = 10;
            x.milkServ = 4;
            x.fatsServ = 15;
        }
        if (tee > 3900) {
            x.cerealsServ = 22;
            x.vegetablesServ = 8;
            x.fruitServ = 8;
            x.meatServ = 10;
            x.milkServ = 4;
            x.fatsServ = 15;
        }

        x.otherFoodsEnergy = Convert.ToInt32((tee * 0.107) * (tee / 1800.0));
        return x;
    }
    #endregion

    #endregion

}
