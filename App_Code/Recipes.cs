using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.IO;
using Igprog;

/// <summary>
/// Recipes
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Recipes : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    string appDataBase = ConfigurationManager.AppSettings["AppDataBase"];
    DataBase db = new DataBase();

    public Recipes() {
    }

    #region Class

    public class NewRecipe {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public double energy { get; set; }
        public string mealGroup { get; set; }

        public JsonFile data = new JsonFile();

        public List<CodeMeal> mealGroups = new List<CodeMeal>();
    }

    public class JsonFile {
        public List<Foods.NewFood> selectedFoods { get; set; }
        public List<Foods.NewFood> selectedInitFoods { get; set; }
    }

    public class CodeMeal {
        public string code;
        public string title;
    }

    static string mealGroup = "mealGroup";  // new column in recipes tbl.
    #endregion Class

    #region WebMethods

    #region UsersRecipes
    [WebMethod]
    public string Init() {
        NewRecipe x = new NewRecipe();
        x.id = null;
        x.title = null;
        x.description = null;
        x.energy = 0;
        x.mealGroup = null;
        JsonFile data = new JsonFile();
        data.selectedFoods = new List<Foods.NewFood>();
        data.selectedInitFoods = new List<Foods.NewFood>();
        x.data = data;
        x.mealGroups = InitMealGroups();
        string json = JsonConvert.SerializeObject(x, Formatting.None);
        return json;
    }

    [WebMethod]
    public string Load(string userId) {
        try {
            db.CreateDataBase(userId, db.recipes);
            db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.recipes, mealGroup);  //new column in recipes tbl.
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"SELECT id, title, description, energy, mealGroup
                        FROM recipes
                        ORDER BY rowid DESC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewRecipe> xx = new List<NewRecipe>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewRecipe x = new NewRecipe();
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.energy = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                x.mealGroup = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                xx.Add(x);
            }
            connection.Close();
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Get(string userId, string id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = @"SELECT id, title, description, energy, mealGroup
                        FROM recipes
                        WHERE id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", id));
            NewRecipe x = new NewRecipe();
            Clients.Client client = new Clients.Client();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.energy = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                x.mealGroup = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.data = JsonConvert.DeserializeObject<JsonFile>(GetJsonFile(userId, x.id));
                x.mealGroups = InitMealGroups();
            }
            connection.Close();
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Save(string userId, NewRecipe x) {
        try {
            db.CreateDataBase(userId, db.recipes);
            db.AddColumn(userId, db.GetDataBasePath(userId, dataBase), db.recipes, mealGroup);  //new column in recipes tbl.
            string sql = "";
            if (x.id == null) {
                x.id = Convert.ToString(Guid.NewGuid());
            }
            x.energy = x.data.selectedFoods.Sum(a => a.energy);
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            sql = @"BEGIN;
                    INSERT OR REPLACE INTO recipes (id, title, description, energy, mealGroup)
                    VALUES (@id, @title, @description, @energy, @mealGroup);
                    COMMIT;";
            command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", x.id));
            command.Parameters.Add(new SQLiteParameter("title", x.title));
            command.Parameters.Add(new SQLiteParameter("description", x.description));
            command.Parameters.Add(new SQLiteParameter("energy", x.energy));
            command.Parameters.Add(new SQLiteParameter("mealGroup", x.mealGroup));
            command.ExecuteNonQuery();
            connection.Close();
            SaveJsonToFile(userId, x.id, JsonConvert.SerializeObject(x.data, Formatting.None));
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Delete(string userId, string id) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = "delete from recipes where id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("id", id));
            command.ExecuteNonQuery();
            connection.Close();
            DeleteJson(userId, id);
            /******* Delete from My Foods if exists (Recipes as My Food) *******/
            MyFoods mf = new MyFoods();
            mf.Delete(userId, id);
            /*******************************************************************/
        } catch (Exception e) { return (e.Message); }
        return "OK";
    }

    [WebMethod]
    public string SaveAsFood(string userId, NewRecipe recipe, string unit) {
        try {
            NewRecipe x = JsonConvert.DeserializeObject<NewRecipe>(Save(userId, recipe));
            Foods.NewFood food = TransformRecipeToFood(recipe);
            food.id = x.id;
            food.unit = unit;
            MyFoods mf = new MyFoods();
            mf.Save(userId, food);
            return "saved";
        } catch (Exception e) { return (e.Message); }
    }
    #endregion UsersRecipes

    #region AppRecipes
    [WebMethod]
    public string LoadAppRecipes(string lang) {
        try {
            //add mealGroup
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath(string.Format("~/App_Data/{0}", appDataBase)));
            connection.Open();
            string sql = string.Format(@"SELECT id, title, description, energy FROM recipes
                        WHERE language = '{0}'
                        ORDER BY rowid DESC", lang);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewRecipe> xx = new List<NewRecipe>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewRecipe x = new NewRecipe();
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.energy = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                xx.Add(x);
            }
            connection.Close();
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string GetAppRecipe(string id, string lang) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath(string.Format("~/App_Data/{0}", appDataBase)));
            connection.Open();
            string sql = string.Format(@"SELECT id, title, description, energy FROM recipes WHERE id = '{0}'", id);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            NewRecipe x = new NewRecipe();
            Clients.Client client = new Clients.Client();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.id = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.energy = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                x.data = JsonConvert.DeserializeObject<JsonFile>(GetAppJsonFile(x.id, lang));
            }
            connection.Close();
            return JsonConvert.SerializeObject(x, Formatting.None);
        }
        catch (Exception e) { return (e.Message); }
    }
    #endregion AppRecipes

    #endregion WebMethods

    #region Methods
    public void SaveJsonToFile(string userId, string filename, string json) {
        string path = string.Format("~/App_Data/users/{0}/recipes", userId);
        string filepath = string.Format("{0}/{1}.json", path, filename);
            CreateFolder(path);
            WriteFile(filepath, json);
    }

    protected void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    protected void WriteFile(string path, string value) {
        File.WriteAllText(Server.MapPath(path), value);
    }

    public void DeleteJson(string userId, string filename) {
        string path = Server.MapPath(string.Format("~/App_Data/users/{0}/recipes", userId));
        string filepath = string.Format("{0}/{1}.json", path, filename);
        if (File.Exists(filepath)) {
            File.Delete(filepath);
        }
    }
    public string GetJsonFile(string userId, string filename) {
        string path = string.Format("~/App_Data/users/{0}/recipes/{1}.json", userId, filename);
        string json = "";
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }

    private string GetAppJsonFile(string filename, string lang) {
        string path = string.Format("~/App_Data/recipes/{0}/{1}.json", lang, filename);
        string json = "";
        if (File.Exists(Server.MapPath(path))) {
            json = File.ReadAllText(Server.MapPath(path));
        }
        return json;
    }

    private bool Check(string userId, NewRecipe x) {
        try {
            bool result = false;
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
            connection.Open();
            string sql = string.Format("SELECT EXISTS(SELECT id FROM recipes WHERE LOWER(title) = '{0}')", x.title.ToLower());
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                result = reader.GetBoolean(0);
            }
            connection.Close();
            return result;
        } catch (Exception e) { return false; }
    }

    private Foods.NewFood TransformRecipeToFood(NewRecipe recipe) {
        try {
            Foods foods = new Foods();
            Foods.NewFood f = foods.GetFoodsTotal(recipe.data.selectedFoods);
            Foods.NewFood x = new Foods.NewFood();
            x.id = null;
            x.food = recipe.title;
            x.quantity = 1;
            x.mass = f.mass;
            x.energy = f.energy;
            x.carbohydrates = f.carbohydrates;
            x.proteins = f.proteins;
            x.fats = f.fats;
            x.servings.cerealsServ = f.servings.cerealsServ;
            x.servings.vegetablesServ = f.servings.vegetablesServ;
            x.servings.fruitServ = f.servings.fruitServ;
            x.servings.meatServ = f.servings.meatServ;
            x.servings.milkServ = f.servings.milkServ;
            x.servings.fatsServ = f.servings.fatsServ;
            x.servings.otherFoodsServ = f.servings.otherFoodsServ;
            x.servings.otherFoodsEnergy = f.servings.otherFoodsEnergy;
            x.starch = f.starch;
            x.totalSugar = f.totalSugar;
            x.glucose = f.glucose;
            x.fructose = f.fructose;
            x.saccharose = f.saccharose;
            x.maltose = f.maltose;
            x.lactose = f.lactose;
            x.fibers = f.fibers;
            x.saturatedFats = f.saturatedFats;
            x.monounsaturatedFats = f.monounsaturatedFats;
            x.polyunsaturatedFats = f.polyunsaturatedFats;
            x.trifluoroaceticAcid = f.trifluoroaceticAcid;
            x.cholesterol = f.cholesterol;
            x.sodium = f.sodium;
            x.potassium = f.potassium;
            x.calcium = f.calcium;
            x.magnesium = f.magnesium;
            x.phosphorus = f.phosphorus;
            x.iron = f.iron;
            x.copper = f.copper;
            x.zinc = f.zinc;
            x.chlorine = f.chlorine;
            x.manganese = f.manganese;
            x.selenium = f.selenium;
            x.iodine = f.iodine;
            x.retinol = f.retinol;
            x.carotene = f.carotene;
            x.vitaminD = f.vitaminD;
            x.vitaminE = f.vitaminE;
            x.vitaminB1 = f.vitaminB1;
            x.vitaminB2 = f.vitaminB2;
            x.vitaminB3 = f.vitaminB3;
            x.vitaminB6 = f.vitaminB6;
            x.vitaminB12 = f.vitaminB12;
            x.folate = f.folate;
            x.pantothenicAcid = f.pantothenicAcid;
            x.biotin = f.biotin;
            x.vitaminC = f.vitaminC;
            x.vitaminK = f.vitaminK;
            return x;
        } catch (Exception e) {
            return new Foods.NewFood();
        }
    }

    private List<CodeMeal> InitMealGroups() {
        List<CodeMeal> xx = new List<CodeMeal>();
        CodeMeal x = new CodeMeal();
        x.code = "G";
        x.title = "general";
        xx.Add(x);
        x = new CodeMeal();
        x.code = "B";
        x.title = "breakfast";
        xx.Add(x);
        x = new CodeMeal();
        x.code = "S";
        x.title = "snack";
        xx.Add(x);
        x = new CodeMeal();
        x.code = "L";
        x.title = "lunch";
        xx.Add(x);
        x = new CodeMeal();
        x.code = "D";
        x.title = "dinner";
        xx.Add(x);
        return xx;
    }
    #endregion Methods

}
