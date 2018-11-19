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
    string userType0FoodLimit = ConfigurationManager.AppSettings["UserType0FoodLimit"];
    DataBase db = new DataBase();
    Translate t = new Translate();

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
    public string InitThermalTreatment() {
        ThermalTreatment x = new ThermalTreatment();
        x.foodGroupVitaminLost = null;
        x.isSelected = false;
        x.vitaminE = 0;
        x.vitaminB1 = 0;
        x.vitaminB2 = 0;
        x.vitaminB3 = 0;
        x.vitaminB12 = 0;
        x.folate = 0;
        x.pantothenicAcid = 0;
        x.biotin = 0;
        x.vitaminC = 0;
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Load(string userId, string userType, string lang) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = string.Format(@"SELECT f.id, f.food, f.foodGroup, fg.title
                        FROM foods AS f
                        LEFT OUTER JOIN foodGroups AS fg
                        ON f.foodGroup = fg.code {0}", userType=="0"?string.Format("LIMIT {0}", userType0FoodLimit):"");
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewFood> xx = new List<NewFood>();
            FoodData foodData = new FoodData();
            List<FoodGroup> foodGroups = new List<FoodGroup>();
            string[] translations = t.Translations(lang);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewFood x = new NewFood();
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.food = reader.GetValue(1) == DBNull.Value ? "" : t.Tran(reader.GetString(1), translations, string.IsNullOrEmpty(lang)?"hr":lang);
                x.foodGroup.code = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.foodGroup.title = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.foodGroup.parent = GetParentGroup(connection, x.foodGroup.code);
                xx.Add(x);
            }
            foodData.foods = xx.OrderBy(a=>a.food).ToList();
            foodData.foodGroups = GetFoodGroups(connection);
            connection.Close();

            MyFoods.Data myf = new MyFoods.Data();
            foodData.myFoods = myf.GetMyFoods(userId);

            string json = JsonConvert.SerializeObject(foodData, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
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
                x.quantity = reader.GetValue(5) == DBNull.Value ? 0 : reader.GetInt32(5);
                x.unit = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                x.mass = reader.GetValue(7) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(7));
                x.energy = reader.GetValue(8) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(8));
                x.carbohydrates = reader.GetValue(9) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(9));
                x.proteins = reader.GetValue(10) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(10));
                x.fats = reader.GetValue(11) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(11));

                x.servings.cerealsServ = reader.GetValue(12) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(12));
                x.servings.vegetablesServ = reader.GetValue(13) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(13));
                x.servings.fruitServ = reader.GetValue(14) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(14));
                x.servings.meatServ = reader.GetValue(15) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(15));
                x.servings.milkServ = reader.GetValue(16) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(16));
                x.servings.fatsServ = reader.GetValue(17) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(17));
                x.servings.otherFoodsServ = reader.GetValue(18) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(18));
                x.servings.otherFoodsEnergy = x.servings.otherFoodsServ > 0 ? x.energy : 0;

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
        x.mass = SmartRound(selectedFoods.Sum(a => a.mass));
        x.energy = SmartRound(selectedFoods.Sum(a => a.energy));
        x.carbohydrates = SmartRound(selectedFoods.Sum(a => a.carbohydrates));
        x.carbohydratesPercentage = SmartRound(GetNutrientPercentage(selectedFoods, x.carbohydrates));
        x.proteins = SmartRound(selectedFoods.Sum(a => a.proteins));
        x.proteinsPercentage = SmartRound(GetNutrientPercentage(selectedFoods, x.proteins));
        x.fats = SmartRound(selectedFoods.Sum(a => a.fats));
        x.fatsPercentage = SmartRound(GetNutrientPercentage(selectedFoods, x.fats));

        x.servings.cerealsServ = SmartRound(selectedFoods.Sum(a => a.servings.cerealsServ));
        x.servings.vegetablesServ = SmartRound(selectedFoods.Sum(a => a.servings.vegetablesServ));
        x.servings.fruitServ = SmartRound(selectedFoods.Sum(a => a.servings.fruitServ));
        x.servings.meatServ = SmartRound(selectedFoods.Sum(a => a.servings.meatServ));
        x.servings.milkServ = SmartRound(selectedFoods.Sum(a => a.servings.milkServ));
        x.servings.fatsServ = SmartRound(selectedFoods.Sum(a => a.servings.fatsServ));
        x.servings.otherFoodsServ = SmartRound(selectedFoods.Sum(a => a.servings.otherFoodsServ));
        x.servings.otherFoodsEnergy = SmartRound(selectedFoods.Where(a => a.servings.otherFoodsServ > 0).Sum(a => a.energy));

        x.mealsTotalEnergy = GetMealsTotalEnergy(selectedFoods, meals);
        x.starch = SmartRound(selectedFoods.Sum(a => a.starch));
        x.totalSugar = SmartRound(selectedFoods.Sum(a => a.totalSugar));
        x.glucose = SmartRound(selectedFoods.Sum(a => a.glucose));
        x.fructose = SmartRound(selectedFoods.Sum(a => a.fructose));
        x.saccharose = SmartRound(selectedFoods.Sum(a => a.saccharose));
        x.maltose = SmartRound(selectedFoods.Sum(a => a.maltose));
        x.lactose = SmartRound(selectedFoods.Sum(a => a.lactose));
        x.fibers = SmartRound(selectedFoods.Sum(a => a.fibers));
        x.saturatedFats = SmartRound(selectedFoods.Sum(a => a.saturatedFats));
        x.monounsaturatedFats = SmartRound(selectedFoods.Sum(a => a.monounsaturatedFats));
        x.polyunsaturatedFats = SmartRound(selectedFoods.Sum(a => a.polyunsaturatedFats));
        x.trifluoroaceticAcid = SmartRound(selectedFoods.Sum(a => a.trifluoroaceticAcid));
        x.cholesterol = SmartRound(selectedFoods.Sum(a => a.cholesterol));
        x.sodium = SmartRound(selectedFoods.Sum(a => a.sodium));
        x.potassium = SmartRound(selectedFoods.Sum(a => a.potassium));
        x.calcium = SmartRound(selectedFoods.Sum(a => a.calcium));
        x.magnesium = SmartRound(selectedFoods.Sum(a => a.magnesium));
        x.phosphorus = SmartRound(selectedFoods.Sum(a => a.phosphorus));
        x.iron = SmartRound(selectedFoods.Sum(a => a.iron));
        x.copper = SmartRound(selectedFoods.Sum(a => a.copper));
        x.zinc = SmartRound(selectedFoods.Sum(a => a.zinc));
        x.chlorine = SmartRound(selectedFoods.Sum(a => a.chlorine));
        x.manganese = SmartRound(selectedFoods.Sum(a => a.manganese));
        x.selenium = SmartRound(selectedFoods.Sum(a => a.selenium));
        x.iodine = SmartRound(selectedFoods.Sum(a => a.iodine));
        x.retinol = SmartRound(selectedFoods.Sum(a => a.retinol));
        x.carotene = SmartRound(selectedFoods.Sum(a => a.carotene));
        x.vitaminD = SmartRound(selectedFoods.Sum(a => a.vitaminD));
        x.vitaminE = SmartRound(selectedFoods.Sum(a => a.vitaminE));
        x.vitaminB1 = SmartRound(selectedFoods.Sum(a => a.vitaminB1));
        x.vitaminB2 = SmartRound(selectedFoods.Sum(a => a.vitaminB2));
        x.vitaminB3 = SmartRound(selectedFoods.Sum(a => a.vitaminB3));
        x.vitaminB6 = SmartRound(selectedFoods.Sum(a => a.vitaminB6));
        x.vitaminB12 = SmartRound(selectedFoods.Sum(a => a.vitaminB12));
        x.folate = SmartRound(selectedFoods.Sum(a => a.folate));
        x.pantothenicAcid = SmartRound(selectedFoods.Sum(a => a.pantothenicAcid));
        x.biotin = SmartRound(selectedFoods.Sum(a => a.biotin));
        x.vitaminC = SmartRound(selectedFoods.Sum(a => a.vitaminC));
        x.vitaminK = SmartRound(selectedFoods.Sum(a => a.vitaminK));

        x.price.value = Math.Round(selectedFoods.Sum(a => a.price.value), 2);
        //TODO price currency
       // if (selectedFoods.Count > 0) {
           // x.price.currency = selectedFoods.Select(a => a.price.currency = "");
            //x.price.unit = selectedFoods[0].price.unit;
       // }

        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string GetRecommendations(ClientsData.NewClientData client, string myRecommendedEnergyIntake, List<MealsRecommendationEnergy> myMealsEnergyPerc) {
        Recommendations x = new Recommendations();
        Calculations c = new Calculations();
        x.energy = string.IsNullOrEmpty(myRecommendedEnergyIntake) ? c.RecommendedEnergyIntake(client) : Convert.ToInt32(myRecommendedEnergyIntake);

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
        x.mealsRecommendationEnergy = GetMealsRecommendations(client.meals, x.energy, myMealsEnergyPerc);

        //TODO - persons from 18, children from 9-10, 10-14, 14-18

                                            // MDA,  UI,  RDA  
        x.starch = GetParameterRecommendation(null, null, null);
        x.totalSugar = GetParameterRecommendation(null, null, null);
        x.glucose = GetParameterRecommendation(null, null, null);
        x.fructose = GetParameterRecommendation(null, null, null);
        x.saccharose = GetParameterRecommendation(null, null, null);
        x.maltose = GetParameterRecommendation(null, null, null);
        x.lactose = GetParameterRecommendation(null, null, null);
        x.fibers = GetParameterRecommendation(null, null, 25);
        x.saturatedFats = GetParameterRecommendation(null, Math.Round((x.energy * 0.1)/9, 1), null);
        x.monounsaturatedFats = GetParameterRecommendation(null, Math.Round((x.energy * 0.2)/9, 1), Math.Round((x.energy * 0.15)/9, 1));
        x.polyunsaturatedFats = GetParameterRecommendation(null, Math.Round((x.energy * 0.11)/9, 1), Math.Round((x.energy * 0.8)/9, 1));
        x.trifluoroaceticAcid = GetParameterRecommendation(null, Math.Round((x.energy * 0.02)/9, 1), null);
        x.cholesterol = GetParameterRecommendation(null, 300, null);
        x.sodium = SodiumRecommendation(client);
        x.potassium = PotassiumRecommendation(client);
        x.calcium = CalciumRecommendation(client);
        x.magnesium = MagnesiumRecommendation(client);
        x.phosphorus = PhosphorusRecommendation(client);
        x.iron = IronRecommendation(client);
        x.copper = CopperRecommendation(client);
        x.zinc = ZincRecommendation(client);
        x.chlorine = ChlorineRecommendation(client);
        x.manganese = ManganeseRecommendation(client);
        x.selenium = SeleniumRecommendation(client);
        x.iodine = IodineRecommendation(client);
        x.retinol = GetParameterRecommendation(null, 1500, 800);
        x.carotene = GetParameterRecommendation(null, null, null);
        x.vitaminD = VitaminDRecommendation(client);
        x.vitaminE = VitaminERecommendation(client);
        x.vitaminB1 = GetParameterRecommendation(null, 4, 1.1);
        x.vitaminB2 = GetParameterRecommendation(null, 4, 1.4);
        x.vitaminB3 = GetParameterRecommendation(null, 35, 16);
        x.vitaminB6 = VitaminB6Recommendation(client);
        x.vitaminB12 = VitaminB12Recommendation(client);
        x.folate = FolateRecommendation(client);
        x.pantothenicAcid = PantothenicAcidRecommendation(client);
        x.biotin = BiotinRecommendation(client);
        x.vitaminC = VitaminCRecommendation(client);
        x.vitaminK = VitaminKRecommendation(client);

        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string ChangeFoodQuantity(NewFood initFood, double newQuantity, double newMass, string type, ThermalTreatment thermalTreatment) {
        NewFood x = new NewFood();
        double k = 1;
        x = initFood;
        switch (type) {
            case "quantity":
                k = newQuantity / initFood.quantity;
                x.quantity = newQuantity;
                x.mass = SmartRound(initFood.mass * k);
                break;
            case "mass":
                k = (newMass / initFood.mass);
                x.mass = newMass;
                x.quantity = SmartRound(initFood.quantity * k);
                break;
            default:
                break;
        }
        x = ChangeFQ(initFood, x, k);

		if(thermalTreatment != null) {
			if (!string.IsNullOrEmpty(thermalTreatment.thermalTreatment.code)) {
				x = IncludeTT(initFood, x, thermalTreatment);
			}
		}
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string GetUnits() {
      return JsonConvert.SerializeObject(Units(), Formatting.Indented);
    }

    [WebMethod]
    public string ChangeNumberOfConsumers(List<NewFood> foods, int number){
        try {
            return JsonConvert.SerializeObject(MultipleConsumers(foods, number), Formatting.Indented);
        } catch (Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string LoadFoods(string lang) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = "SELECT id, food, quantity, unit, mass, energy FROM foods";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            string[] translations = t.Translations(lang);
            SQLiteDataReader reader = command.ExecuteReader();
            var xx = new List<object>();
            while (reader.Read()) {
                xx.Add(new {
                    id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0),
                    food = reader.GetValue(1) == DBNull.Value ? "" : t.Tran(reader.GetString(1), translations, string.IsNullOrEmpty(lang) ? "hr" : lang),
                    quantity = reader.GetValue(2) == DBNull.Value ? 0 : reader.GetInt32(2),
                    unit = reader.GetValue(3) == DBNull.Value ? "" : t.Tran(reader.GetString(3), translations, string.IsNullOrEmpty(lang) ? "hr" : lang),
                    mass = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4)),
                    energy = reader.GetValue(5) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(5))
                });
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string IncludeThermalTreatment(NewFood initFood, NewFood food, ThermalTreatment thermalTreatment) {
        try {
            return JsonConvert.SerializeObject(IncludeTT(initFood, food, thermalTreatment), Formatting.Indented);
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string InitFoodForEdit(NewFood food) {
        try {
            double k = 1 / food.quantity;
            food.mass = SmartRound(food.mass * k);
            food.quantity = 1;
            food = ChangeFQ(food, food, k);
            food = ExcludeTT(food);
            return JsonConvert.SerializeObject(food, Formatting.Indented);
        } catch (Exception e) { return ("Error: " + e); }
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
            if (reader.StepCount > 0) {
                ThermalTreatment t = new ThermalTreatment();
                t.thermalTreatment.title = "no thermal treatment";
                xx.Add(t);
            }
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
    private List<MealsRecommendationEnergy> GetMealsRecommendations(List<Meals.NewMeal> meals, int energy, List<MealsRecommendationEnergy> myMealsEnergyPerc) {
        List<MealsRecommendationEnergy> xx = new List<MealsRecommendationEnergy>();
        int idx = 0;
        foreach (var obj in meals) {
            MealsRecommendationEnergy x = new MealsRecommendationEnergy();
            x.meal.code = obj.code;
            if(myMealsEnergyPerc != null) {
                if (idx < myMealsEnergyPerc.Count) {
                    x.meal.energyMinPercentage = myMealsEnergyPerc[idx].meal.energyMinPercentage;
                    x.meal.energyMaxPercentage = myMealsEnergyPerc[idx].meal.energyMinPercentage;
                }
            } else {
                x.meal.energyMinPercentage = GetMealRecommendationPercentage(meals, idx).min;
                x.meal.energyMaxPercentage = GetMealRecommendationPercentage(meals, idx).max;
            }
            x.meal.energyMin = Convert.ToInt32(x.meal.energyMinPercentage * 0.01 * energy);
            x.meal.energyMax = Convert.ToInt32(x.meal.energyMaxPercentage * 0.01 * energy);
            xx.Add(x);
            idx++;
        }
        return xx;
    }

    private MealEnergy GetMealRecommendationPercentage(List<Meals.NewMeal> meals, int idx) {
        MealEnergy x = new MealEnergy();
        //1 case all meals
        if (meals[0].isSelected == true &&
            meals[1].isSelected == true &&
            meals[2].isSelected == true &&
            meals[3].isSelected == true &&
            meals[4].isSelected == true &&
            meals[5].isSelected == true) {
            switch (idx) {
                case 0:
                    x.min = 20;
                    x.max = 25;
                    break;
                case 1:
                    x.min = 5;
                    x.max = 10;
                    break;
                case 2:
                    x.min = 30;
                    x.max = 40;
                    break;
                case 3:
                    x.min = 5;
                    x.max = 10;
                    break;
                case 4:
                    x.min = 20;
                    x.max = 23;
                    break;
                case 5:
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
            switch (idx) {
                case 0:
                    x.min = 20;
                    x.max = 25;
                    break;
                case 1:
                    x.min = 5;
                    x.max = 10;
                    break;
                case 2:
                    x.min = 30;
                    x.max = 40;
                    break;
                case 3:
                    x.min = 5;
                    x.max = 10;
                    break;
                case 4:
                    x.min = 20;
                    x.max = 25;
                    break;
                case 5:
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
            switch (idx) {
                case 0:
                    x.min = 25;
                    x.max = 35;
                    break;
                case 1:
                    x.min = 0;
                    x.max = 0;
                    break;
                case 2:
                    x.min = 30;
                    x.max = 40;
                    break;
                case 3:
                    x.min = 5;
                    x.max = 10;
                    break;
                case 4:
                    x.min = 20;
                    x.max = 25;
                    break;
                case 5:
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
            switch (idx){
                case 0:
                    x.min = 25;
                    x.max = 35;
                    break;
                case 1:
                    x.min = 0;
                    x.max = 0;
                    break;
                case 2:
                    x.min = 35;
                    x.max = 45;
                    break;
                case 3:
                    x.min = 0;
                    x.max = 0;
                    break;
                case 4:
                    x.min = 25;
                    x.max = 30;
                    break;
                case 5:
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


    /********* Food and Nutrition Board, Institute of Medicine, National Academies (www.nap.edu) *********/
    private ParameterRecommendation SodiumRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = 300;
            x.ui = 1500;
            x.rda = 1000;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = 400;
            x.ui = 1900;
            x.rda = 1200;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = 500;
            x.ui = 2200;
            x.rda = 1500;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = 500;
            x.ui = 2300;
            x.rda = 1500;
        }
        if(client.age >= 19 && client.age < 31) {
            x.mda = 500;
            x.ui = 2300;
            x.rda = 1500;
        }
        if(client.age >= 31 && client.age < 51) {
            x.mda = 500;
            x.ui = 2300;
            x.rda = 1500;
        }
        if(client.age >= 51 && client.age < 71) {
            x.mda = 500;
            x.ui = 2300;
            x.rda = 1300;
        }
        if(client.age >= 71) {
            x.mda = 500;
            x.ui = 2300;
            x.rda = 1200;
        }
        return x;
    }

    private ParameterRecommendation PotassiumRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = 1600;
            x.ui = null;
            x.rda = 3000;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = 1800;
            x.ui = null;
            x.rda = 3800;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = 2000;
            x.ui = null;
            x.rda = 4500;
        }
        if(client.age >= 14) {
            x.mda = 2000;
            x.ui = null;
            x.rda = 4700;
        }
        return x;
    }

    private ParameterRecommendation CalciumRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 2500;
            x.rda = 700;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 2500;
            x.rda = 1000;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 3000;
            x.rda = 1300;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 3000;
            x.rda = 1300;
        }
        if(client.age >= 19 && client.age < 31) {
            x.mda = null;
            x.ui = 2500;
            x.rda = 1000;
        }
        if(client.age >= 31 && client.age < 51) {
            x.mda = null;
            x.ui = 2500;
            x.rda = 1000;
        }
        if(client.age >= 51 && client.age < 71) {
            x.mda = null;
            x.ui = 2000;
            if (client.gender.value == 0) {
                x.rda = 1000;
            } else {
                x.rda = 1200;
            }
        }
        if(client.age >= 71) {
            x.mda = null;
            x.ui = 2000;
            x.rda = 1200;
        }
        return x;
    }

    private ParameterRecommendation MagnesiumRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 300; //??
            x.rda = 80;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 350;  //??
            x.rda = 130;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 700;
            x.rda = 240;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 700;
           if (client.gender.value == 0) {
                x.rda = 410;
            } else {
                x.rda = 360;
            }
        }
        if(client.age >= 19 && client.age < 31) {
            x.mda = null;
            x.ui = 700;
            if (client.gender.value == 0) {
                x.rda = 400;
            } else {
                x.rda = 310;
            }
        }
        if(client.age >= 31) {
            x.mda = null;
            x.ui = 700;
            if (client.gender.value == 0) {
                x.rda = 420;
            } else {
                x.rda = 320;
            }
        }
        return x;
    }

    private ParameterRecommendation PhosphorusRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 3000;
            x.rda = 460;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 3000;
            x.rda = 500;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 4000;
            x.rda = 1250;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 4000;
            x.rda = 1250;
        }
        if(client.age >= 19 && client.age < 31) {
            x.mda = null;
            x.ui = 4000;
            x.rda = 700;
        }
        if(client.age >= 31 && client.age < 51) {
            x.mda = null;
            x.ui = 4000;
            x.rda = 700;
        }
        if(client.age >= 51 && client.age < 71) {
            x.mda = null;
            x.ui = 4000;
            x.rda = 700;
        }
        if(client.age >= 71) {
            x.mda = null;
            x.ui = 3000;
            x.rda = 700;
        }
        return x;
    }

    private ParameterRecommendation IronRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 40;
            x.rda = 7;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 40;
            x.rda = 10;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 40;
            x.rda = 8;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 45;
           if (client.gender.value == 0) {
                x.rda = 11;
            } else {
                x.rda = 15;
            }
        }
        if(client.age >= 19 && client.age < 31) {
            x.mda = null;
            x.ui = 45;
            if (client.gender.value == 0) {
                x.rda = 8;
            } else {
                x.rda = 18;
            }
        }
        if(client.age >= 31 && client.age < 51) {
            x.mda = null;
            x.ui = 45;
            if (client.gender.value == 0) {
                x.rda = 8;
            } else {
                x.rda = 18;
            }
        }
        if(client.age >= 51) {
            x.mda = null;
            x.ui = 45;
            x.rda = 8;
        }
        return x;
    }

    private ParameterRecommendation CopperRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 1;
            x.rda = 0.34;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 3;
            x.rda = 0.44;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 5;
            x.rda = 0.7;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 8;
            x.rda = 0.89;
        }
        if(client.age >= 19 && client.age < 31) {
            x.mda = null;
            x.ui = 10;
            x.rda = 0.9;
        }
        if(client.age >= 31 && client.age < 51) {
            x.mda = null;
            x.ui = 10;
            x.rda = 0.9;
        }
        if(client.age >= 51 && client.age < 71) {
            x.mda = null;
            x.ui = 10;
            x.rda = 0.9;
        }
        if(client.age >= 71) {
            x.mda = null;
            x.ui = 10;
            x.rda = 0.9;
        }
        return x;
    }

    private ParameterRecommendation ZincRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 7;
            x.rda = 3;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 12;
            x.rda = 5;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 23;
            x.rda = 8;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 34;
            if (client.gender.value == 0) {
                x.rda = 11;
            } else {
                x.rda = 9;
            }
        }
        if(client.age >= 19) {
            x.mda = null;
            x.ui = 40;
            if (client.gender.value == 0) {
                x.rda = 11;
            } else {
                x.rda = 8;
            }
        }
        return x;
    }

    private ParameterRecommendation ChlorineRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 2300;
            x.rda = 1500;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 2900;
            x.rda = 1900;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 3400;
            x.rda = 2300;
        }
        if(client.age >= 14 && client.age < 51) {
            x.mda = null;
            x.ui = 3600;
            x.rda = 2300;
        }
        if(client.age >= 51 && client.age < 71) {
            x.mda = null;
            x.ui = 3600;
            x.rda = 2000;
        }
        if(client.age >= 71) {
            x.mda = null;
            x.ui = 3600;
            x.rda = 1800;
        }
        return x;
    }

    private ParameterRecommendation ManganeseRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 2;
            x.rda = 1.2;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 3;
            x.rda = 1.5;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 6;
            if (client.gender.value == 0) {
                x.rda = 1.9;
            } else {
                x.rda = 1.6;
            }
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 9;
            if (client.gender.value == 0) {
                x.rda = 2.2;
            } else {
                x.rda = 1.6;
            }
        }
        if(client.age >= 19) {
            x.mda = null;
            x.ui = 11;
            if (client.gender.value == 0) {
                x.rda = 2.3;
            } else {
                x.rda = 1.8;
            }
        }
        return x;
    }

    private ParameterRecommendation SeleniumRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 90;
            x.rda = 20;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 150;
            x.rda = 30;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 280;
            x.rda = 40;
        }
        if(client.age >= 14) {
            x.mda = null;
            x.ui = 400;
            x.rda = 55;
        }
        return x;
    }

    private ParameterRecommendation IodineRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 200;
            x.rda = 90;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 300;
            x.rda = 90;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 600;
            x.rda = 120;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 900;
            x.rda = 150;

        }
        if(client.age >= 19) {
            x.mda = null;
            x.ui = 1100;
            x.rda = 150;
        }
        return x;
    }

    private ParameterRecommendation VitaminDRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 63;
            x.rda = 15;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 75;
            x.rda = 15;
        }
        if (client.age >= 9 && client.age < 71) {
            x.mda = null;
            x.ui = 100;
            x.rda = 15;
        }
        if(client.age >= 71) {
            x.mda = null;
            x.ui = 100;
            x.rda = 20;
        }
        return x;
    }

    private ParameterRecommendation VitaminERecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 200;
            x.rda = 6;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 300;
            x.rda = 7;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 600;
            x.rda = 11;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 800;
            x.rda = 15;
        }
        if(client.age >= 19) {
            x.mda = null;
            x.ui = 1000;
            x.rda = 15;
        }
        return x;
    }

    private ParameterRecommendation VitaminB6Recommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 30;
            x.rda = 0.5;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 40;
            x.rda = 0.6;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 60;
            x.rda = 1;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 80;
            if (client.gender.value == 0) {
                x.rda = 1.3;
            } else {
                x.rda = 1.2;
            }
        }
        if(client.age >= 19 && client.age < 51) {
            x.mda = null;
            x.ui = 100;
            x.rda = 1.3;
        }
        if(client.age >= 51) {
            x.mda = null;
            x.ui = 100;
            if (client.gender.value == 0) {
                x.rda = 1.7;
            } else {
                x.rda = 1.5;
            }
        }
        return x;
    }

    private ParameterRecommendation VitaminB12Recommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 7;
            x.rda = 0.9;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 8;
            x.rda = 1.2;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 9;
            x.rda = 1.8;
        }
        if(client.age >= 14) {
            x.mda = null;
            x.ui = 9;
            x.rda = 2.4;
        }
        return x;
    }

    private ParameterRecommendation FolateRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 300;
            x.rda = 150;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 400;
            x.rda = 200;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 600;
            x.rda = 300;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 800;
            x.rda = 400;
        }
        if(client.age >= 19) {
            x.mda = null;
            x.ui = 1000;
            x.rda = 400;
        }
        return x;
    }

    private ParameterRecommendation PantothenicAcidRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 13;
            x.rda = 2;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 14;
            x.rda = 3;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 15;
            x.rda = 4;
        }
        if(client.age >= 14) {
            x.mda = null;
            x.ui = 15;
            x.rda = 5;
        }
        return x;
    }

    private ParameterRecommendation BiotinRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 60;
            x.rda = 8;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 80;
            x.rda = 12;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 100;
            x.rda = 20;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 100;
            x.rda = 25;
        }
        if(client.age >= 19) {
            x.mda = null;
            x.ui = 100;
            x.rda = 30;
        }
        return x;
    }

    private ParameterRecommendation VitaminCRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 400;
            x.rda = 15;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 650;
            x.rda = 25;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 1200;
            x.rda = 45;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 1800;
            if (client.gender.value == 0) {
                x.rda = 75;
            } else {
                x.rda = 65;
            }
        }
        if(client.age >= 19) {
            x.mda = null;
            x.ui = 2000;
            if (client.gender.value == 0) {
                x.rda = 90;
            } else {
                x.rda = 75;
            }
        }
        return x;
    }

    private ParameterRecommendation VitaminKRecommendation(ClientsData.NewClientData client) {
        ParameterRecommendation x = new ParameterRecommendation();
        if(client.age >= 1 && client.age < 4) {
            x.mda = null;
            x.ui = 60;
            x.rda = 30;
        }
        if (client.age >= 4 && client.age < 9) {
            x.mda = null;
            x.ui = 80;
            x.rda = 55;
        }
        if (client.age >= 9 && client.age < 14) {
            x.mda = null;
            x.ui = 100;
            x.rda = 60;
        }
        if(client.age >= 14 && client.age < 19) {
            x.mda = null;
            x.ui = 100;
            x.rda = 75;
        }
        if(client.age >= 19) {
            x.mda = null;
            x.ui = 100;
            if (client.gender.value == 0) {
                x.rda = 120;
            } else {
                x.rda = 90;
            }
        }
        return x;
    }

    /*****************************************************************************************************/

    private List<string> Units() {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/"  + dataBase));
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
            return xx;
        } catch (Exception e) { return new List<string>(); }
    }

    private string GetUnit(double qty, string unit) {
        switch (unit){
            #region hr
            case "jušna žlica":
                if (qty > 1 && qty < 5 ) { unit = "jušne žlice"; }
                if (qty >= 5) { unit = "jušnih žlica"; }
                break;
            case "šalica":
                if (qty > 1 && qty < 5) { unit = "šalice"; }
                if (qty >= 5) { unit = "šalice"; }
                break;
            case "plod":
                if (qty > 1 && qty < 5) { unit = "ploda"; }
                if (qty >= 5) { unit = "plodova"; }
                break;
            case "čajna žličica":
                if (qty > 1 && qty < 5) { unit = "čajne žličice"; }
                if (qty >= 5) { unit = "čajnih žličica"; }
                break;
            case "porcija":
                if (qty > 1 && qty < 5) { unit = "porcije"; }
                if (qty >= 5) { unit = "porcija"; }
                break;
            case "limenka":
                if (qty > 1 && qty < 5) { unit = "limenke"; }
                if (qty >= 5) { unit = "limenki"; }
                break;
            case "kriška":
                if (qty > 1 && qty < 5) { unit = "kriške"; }
                if (qty >= 5) { unit = "kriški"; }
                break;
            case "boca":
                if (qty > 1 && qty < 5) { unit = "boce"; }
                if (qty >= 5) { unit = "boca"; }
                break;
            case "čaša":
                if (qty > 1 && qty < 5) { unit = "čaše"; }
                if (qty >= 5) { unit = "čaša"; }
                break;
            case "polovica":
                if (qty > 1 && qty < 5) { unit = "polovice"; }
                if (qty >= 5) { unit = "polovica"; }
                break;
            case "mali komad":
                if (qty > 1 && qty < 5) { unit = "mala komada"; }
                if (qty >= 5) { unit = "malih komada"; }
                break;
            case "listić":
                if (qty > 1 && qty < 5) { unit = "listića"; }
                if (qty >= 5) { unit = "listića"; }
                break;
            case "zrno":
                if (qty > 1 && qty < 5) { unit = "zrna"; }
                if (qty >= 5) { unit = "zrna"; }
                break;
            case "veliki plod":
                if (qty > 1 && qty < 5) { unit = "velika ploda"; }
                if (qty >= 5) { unit = "velikih plodova"; }
                break;
            case "srednji plod":
                if (qty > 1 && qty < 5) { unit = "srednja ploda"; }
                if (qty >= 5) { unit = "srednjih plodova"; }
                break;
            case "veliki komad":
                if (qty > 1 && qty < 5) { unit = "velika komada"; }
                if (qty >= 5) { unit = "velikih komada"; }
                break;
            case "komad":
                if (qty > 1 && qty < 5) { unit = "komada"; }
                if (qty >= 5) { unit = "komada"; }
                break;
            case "list":
                if (qty > 1 && qty < 5) { unit = "lista"; }
                if (qty >= 5) { unit = "listova"; }
                break;
            case "filet":
                if (qty > 1 && qty < 5) { unit = "fileta"; }
                if (qty >= 5) { unit = "fileta"; }
                break;
            case "čašica":
                if (qty > 1 && qty < 5) { unit = "čašice"; }
                if (qty >= 5) { unit = "čašica"; }
                break;
            #endregion hr
            #region sr
            case "šoljica":
                if (qty > 1 && qty < 5) { unit = "šoljice"; }
                if (qty >= 5) { unit = "šoljica"; }
                break;
            case "parče":
                if (qty > 1 && qty < 5) { unit = "parčeta"; }
                if (qty >= 5) { unit = "parčeta"; }
                break;
            case "čajna kašika":
                if (qty > 1 && qty < 5) { unit = "čajne kašike"; }
                if (qty >= 5) { unit = "čajnih kašika"; }
                break;
            case "supena kašika":
                if (qty > 1 && qty < 5) { unit = "supene kašike"; }
                if (qty >= 5) { unit = "supenih kašika"; }
                break;
            case "malo parče":
                if (qty > 1 && qty < 5) { unit = "mala parčeta"; }
                if (qty >= 5) { unit = "malih parčeta"; }
                break;
            case "veliko parče":
                if (qty > 1 && qty < 5) { unit = "velika parčeta"; }
                if (qty >= 5) { unit = "velikih parčeta"; }
                break;
            case "kašičica":
                if (qty > 1 && qty < 5) { unit = "kašičice"; }
                if (qty >= 5) { unit = "kašičica"; }
                break;
            case "flaša":
                if (qty > 1 && qty < 5) { unit = "flaše"; }
                if (qty >= 5) { unit = "flaša"; }
                break;
            #endregion sr
            default:
                break;
        }
        return unit;
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

    public List<NewFood> MultipleConsumers(List<NewFood> foods, int number){
        List<NewFood> xx = new List<NewFood>();
        foreach(var f in foods) {
            NewFood x = new NewFood();
            x.id = f.id;
            x.food = f.food;
            x.foodGroup = f.foodGroup;
            x.foodGroupVitaminLost = f.foodGroupVitaminLost;
            x.thermalTreatments = f.thermalTreatments;
            x.meal = f.meal;
            x.quantity = Math.Round(f.quantity * number, 1);
            x.unit = GetUnit(x.quantity, f.unit);
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
        return xx;
    }

    private int DecimalPlace(double value) {
        int i = 1;
        if (value >= 1) { i = 1; }
        if (value < 1 && value >= 0.1) { i = 2; }
        if (value < 0.1 && value >= 0.01) { i = 3; }
        if (value < 0.01 && value >= 0.001) { i = 4; }
        if (value < 0.001 && value >= 0.0001) { i = 5; }
        if (value < 0.0001) { i = 10; }
        return i;
    }

    private double SmartRound(double value) {
        return Math.Round(value, DecimalPlace(value));
    }

    private NewFood IncludeTT(NewFood initFood, NewFood food, ThermalTreatment tt) {
        food.thermalTreatments.Find(a => a.thermalTreatment.code == tt.thermalTreatment.code).isSelected = true;
        food.vitaminE = SmartRound(initFood.vitaminE * (1 - tt.vitaminE) * food.quantity);
        food.vitaminB1 = SmartRound(initFood.vitaminB1 * (1 - tt.vitaminB1) * food.quantity);
        food.vitaminB2 = SmartRound(initFood.vitaminB2 * (1 - tt.vitaminB2) * food.quantity);
        food.vitaminB3 = SmartRound(initFood.vitaminB3 * (1 - tt.vitaminB3) * food.quantity);
        food.vitaminB6 = SmartRound(initFood.vitaminB6 * (1 - tt.vitaminB6) * food.quantity);
        food.vitaminB12 = SmartRound(initFood.vitaminB12 * (1 - tt.vitaminB12) * food.quantity);
        food.folate = SmartRound(initFood.folate * (1 - tt.folate) * food.quantity);
        food.pantothenicAcid = SmartRound(initFood.pantothenicAcid * (1 - tt.pantothenicAcid) * food.quantity);
        food.biotin = SmartRound(initFood.biotin * (1 - tt.biotin) * food.quantity);
        food.vitaminC = SmartRound(initFood.vitaminC * (1 - tt.vitaminC) * food.quantity);
        return food;
    }

    private NewFood ExcludeTT(NewFood food) {
        ThermalTreatment tt = food.thermalTreatments.Find(a => a.isSelected == true);
        if(tt != null) {
            food.thermalTreatments.Find(a => a.thermalTreatment.code == tt.thermalTreatment.code).isSelected = false;
            food.vitaminE = SmartRound(food.vitaminE / (1 - tt.vitaminE));
            food.vitaminB1 = SmartRound(food.vitaminB1 / (1 - tt.vitaminB1));
            food.vitaminB2 = SmartRound(food.vitaminB2 / (1 - tt.vitaminB2));
            food.vitaminB3 = SmartRound(food.vitaminB3 / (1 - tt.vitaminB3));
            food.vitaminB6 = SmartRound(food.vitaminB6 / (1 - tt.vitaminB6));
            food.vitaminB12 = SmartRound(food.vitaminB12 / (1 - tt.vitaminB12));
            food.folate = SmartRound(food.folate / (1 - tt.folate));
            food.pantothenicAcid = SmartRound(food.pantothenicAcid / (1 - tt.pantothenicAcid));
            food.biotin = SmartRound(food.biotin / (1 - tt.biotin));
            food.vitaminC = SmartRound(food.vitaminC / (1 - tt.vitaminC));
        }
        return food;
    }

    private NewFood ChangeFQ(NewFood initFood, NewFood x, double k) {
        x.unit = GetUnit(x.quantity, initFood.unit);
        x.energy = SmartRound(initFood.energy * k);
        x.carbohydrates = SmartRound(initFood.carbohydrates * k);
        x.proteins = SmartRound(initFood.proteins * k);
        x.fats = SmartRound(initFood.fats * k);
        x.servings.cerealsServ = SmartRound(initFood.servings.cerealsServ * k);
        x.servings.vegetablesServ = SmartRound(initFood.servings.vegetablesServ * k);
        x.servings.fruitServ = SmartRound(initFood.servings.fruitServ * k);
        x.servings.meatServ = SmartRound(initFood.servings.meatServ * k);
        x.servings.milkServ = SmartRound(initFood.servings.milkServ * k);
        x.servings.fatsServ = SmartRound(initFood.servings.fatsServ * k);
        x.servings.otherFoodsServ = SmartRound(initFood.servings.otherFoodsServ * k);
        x.servings.otherFoodsEnergy = SmartRound(initFood.servings.otherFoodsEnergy * k);

        //TODO servings description

        x.starch = SmartRound(initFood.starch * k);
        x.totalSugar = SmartRound(initFood.totalSugar * k);
        x.glucose = SmartRound(initFood.glucose * k);
        x.fructose = SmartRound(initFood.fructose * k);
        x.saccharose = SmartRound(initFood.saccharose * k);
        x.maltose = SmartRound(initFood.maltose * k);
        x.lactose = SmartRound(initFood.lactose * k);
        x.fibers = SmartRound(initFood.fibers * k);
        x.saturatedFats = SmartRound(initFood.saturatedFats * k);
        x.monounsaturatedFats = SmartRound(initFood.monounsaturatedFats * k);
        x.polyunsaturatedFats = SmartRound(initFood.polyunsaturatedFats * k);
        x.trifluoroaceticAcid = SmartRound(initFood.trifluoroaceticAcid * k);
        x.cholesterol = SmartRound(initFood.cholesterol * k);
        x.sodium = SmartRound(initFood.sodium * k);
        x.potassium = SmartRound(initFood.potassium * k);
        x.calcium = SmartRound(initFood.calcium * k);
        x.magnesium = SmartRound(initFood.magnesium * k);
        x.phosphorus = SmartRound(initFood.phosphorus * k);
        x.iron = SmartRound(initFood.iron * k);
        x.copper = SmartRound(initFood.copper * k);
        x.zinc = SmartRound(initFood.zinc * k);
        x.chlorine = SmartRound(initFood.chlorine * k);
        x.manganese = SmartRound(initFood.manganese * k);
        x.selenium = SmartRound(initFood.selenium * k);
        x.iodine = SmartRound(initFood.iodine * k);
        x.retinol = SmartRound(initFood.retinol * k);
        x.carotene = SmartRound(initFood.carotene * k);
        x.vitaminD = SmartRound(initFood.vitaminD * k);
        x.vitaminE = SmartRound(initFood.vitaminE * k);
        x.vitaminB1 = SmartRound(initFood.vitaminB1 * k);
        x.vitaminB2 = SmartRound(initFood.vitaminB2 * k);
        x.vitaminB3 = SmartRound(initFood.vitaminB3 * k);
        x.vitaminB6 = SmartRound(initFood.vitaminB6 * k);
        x.vitaminB12 = SmartRound(initFood.vitaminB12 * k);
        x.folate = SmartRound(initFood.folate * k);
        x.pantothenicAcid = SmartRound(initFood.pantothenicAcid * k);
        x.biotin = SmartRound(initFood.biotin * k);
        x.vitaminC = SmartRound(initFood.vitaminC * k);
        x.vitaminK = SmartRound(initFood.vitaminK * k);

        //TODO
        x.price.value = Math.Round(initFood.price.value * k, 2);
        return x;
    }
    #endregion

}
