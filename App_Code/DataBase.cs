using System;
using System.Web;
using System.Configuration;
using System.IO;
using System.Data.SQLite;

/// <summary>
/// DataBase
/// </summary>
namespace Igprog {
    public class DataBase {
        string dataBase = ConfigurationManager.AppSettings["UserDataBase"];

        public DataBase() {
        }

        //tables
        public string users = "users";
        public string clients = "clients";
        public string clientsData = "clientsdata";
        public string myFoods = "myfoods";
        public string menues = "menues";
        public string scheduler = "scheduler";
        public string instals = "instals";
        public string orders = "orders";
        public string prices = "prices";
        public string invoices = "invoices";
        public string recipes = "recipes";
        public string meals = "meals";

        #region CreateTable (users.ddb)
        public void Users(string path) {
            string sql = @"CREATE TABLE IF NOT EXISTS users
                        (userId NVARCHER (50),
                        userType INTEGER,
                        firstName NVARCHAR (50),
                        lastName NVARCHAR (50),
                        companyName NVARCHAR (50),
                        address NVARCHAR (50),
                        postalCode NVARCHAR (50),
                        city NVARCHAR (50),
                        country NVARCHAR (50),
                        pin NVARCHAR (50),
                        phone NVARCHAR (50),
                        email NVARCHAR (50),
                        userName NVARCHAR (50),
                        password NVARCHAR (100),
                        adminType INTEGER,
                        userGroupId INTEGER,
                        activationDate VARCHAR(50),
                        expirationDate VARCHAR(50),
                        isActive INTEGER,
                        ipAddress NVARCHAR (50))";
            CreateTable(path, sql);
        }
        #endregion

        #region CreateTable (app)
        public void Clients(string path) {
            string sql = @"CREATE TABLE IF NOT EXISTS clients
                (clientId VARCHAR(50) PRIMARY KEY,
                firstName NVARCHAR(50),
                lastName NVARCHAR(50),
                birthDate VARCHAR(50),
                gender INTEGER,
                phone VARCHAR(50),
                email VARCHAR(50),
                userId VARCHAR(50),
                date VARCHAR(50),
                isActive INTEGER)";
            CreateTable(path, sql);
        }

        public void ClientsData(string path) {
            string sql = @"CREATE TABLE IF NOT EXISTS clientsdata
                (clientId VARCHAR(50),
                height VARCHAR(50),
                weight VARCHAR(50),
                waist VARCHAR(50),
                hip VARCHAR(50),
                pal VARCHAR(50),
                goal VARCHAR(50),
                activities NVARCHAR(200),
                diet NVARCHAR(200),
                meals NVARCHAR(200),
                date VARCHAR(50),
                userId VARCHAR(50))";
            CreateTable(path, sql);
        }

        public void MyFoods(string path) {
            string sql = @"CREATE TABLE IF NOT EXISTS myfoods
                (id VARCHAR (50) PRIMARY KEY,
                food NVARCHAR (50),
                foodGroup VARCHAR (50),
                foodGroupVitaminLost VARCHAR (50),
                quantity INTEGER,
                unit NVARCHAR (50),
                mass VARCHAR (50),
                energy VARCHAR (50),
                carbohydrates VARCHAR (50),
                proteins VARCHAR (50),
                fats VARCHAR (50),
                cerealsServ VARCHAR (50),
                vegetablesServ VARCHAR (50),
                fruitServ VARCHAR (50),
                meatServ VARCHAR (50),
                milkServ VARCHAR (50),
                fatsServ VARCHAR (50),
                otherFoodsServ VARCHAR (50),
                starch VARCHAR (50),
                totalSugar VARCHAR (50),
                glucose VARCHAR (50),
                fructose VARCHAR (50),
                saccharose VARCHAR (50),
                maltose VARCHAR (50),
                lactose VARCHAR (50),
                fibers VARCHAR (50),
                saturatedFats VARCHAR (50),
                monounsaturatedFats VARCHAR (50),
                polyunsaturatedFats VARCHAR (50),
                trifluoroaceticAcid VARCHAR (50),
                cholesterol VARCHAR (50),
                sodium VARCHAR (50),
                potassium VARCHAR (50),
                calcium VARCHAR (50),
                magnesium VARCHAR (50),
                phosphorus VARCHAR (50),
                iron VARCHAR (50),
                copper VARCHAR (50),
                zinc VARCHAR (50),
                chlorine VARCHAR (50),
                manganese VARCHAR (50),
                selenium VARCHAR (50),
                iodine VARCHAR (50),
                retinol VARCHAR (50),
                carotene VARCHAR (50),
                vitaminD VARCHAR (50),
                vitaminE VARCHAR (50),
                vitaminB1 VARCHAR (50),
                vitaminB2 VARCHAR (50),
                vitaminB3 VARCHAR (50),
                vitaminB6 VARCHAR (50),
                vitaminB12 VARCHAR (50),
                folate VARCHAR (50),
                pantothenicAcid VARCHAR (50),
                biotin VARCHAR (50),
                vitaminC VARCHAR (50),
                vitaminK VARCHAR (50))";
            CreateTable(path, sql);
        }

        public void Menues(string path) {
            string sql = @"CREATE TABLE IF NOT EXISTS menues
                (id VARCHAR(50),
                title NVARCHAR(50),
                diet NVARCHAR(50),
                date VARCHAR(50),
                note NVARCHAR(200),
                userId VARCHAR(50),
                clientId VARCHAR(50),
                userGroupId VARCHAR(50),
                energy VARCHAR(50),
                PRIMARY KEY(id, clientId))";
            CreateTable(path, sql);
        }

        public void Prices(string path) {
            string sql = @"CREATE TABLE IF NOT EXISTS prices
                (id VARCHAR(50) PRIMARY KEY,
                foodId VARCHAR(50),
                food NVARCHAR(50),
                netPrice VARCHAR(50),
                currency NVARCHAR(50),
                mass INTEGER,
                unit NVARCHAR(50),
                unitPrice VARCHAR(50),
                note NVARCHAR(50))";
            CreateTable(path, sql);
        }

        public void Recipes(string path) {
            string sql = @"CREATE TABLE IF NOT EXISTS recipes
                (id VARCHAR(50) PRIMARY KEY,
                title NVARCHAR(50),
                description NVARCHAR(200),
                energy VARCHAR(50))";
            CreateTable(path, sql);
        }

        public void Meals(string path) {
            string sql = @"CREATE TABLE IF NOT EXISTS meals
                (id VARCHAR(50) PRIMARY KEY,
                title NVARCHAR(50),
                description NVARCHAR(200),
                userId VARCHAR(50),
                userGroupId VARCHAR(50))";
            CreateTable(path, sql);
        }

        public void Scheduler(string path) {
            string sql = @"CREATE TABLE IF NOT EXISTS scheduler
                (room INTEGER,
                clientId VARCHAR(50),
                content NVARCHAR(200),
                startDate INTEGER,
                endDate INTEGER,
                userId VARCHAR(50))";
            CreateTable(path, sql);
        }
        #endregion

        #region CreateTable (web page)
        public void Orders(string path) {
            string sql = @"CREATE TABLE IF NOT EXISTS orders
                (firstName NVARCHAR(50),
                lastName NVARCHAR(50),
                companyName NVARCHAR(50),
                address NVARCHAR(50),
                postalCode NVARCHAR(50),
                city NVARCHAR(50),
                country NVARCHAR(50),
                pin VARCHAR(50),
                email VARCHAR(50),
                ipAddress VARCHAR(50),
                application VARCHAR(50),
                version VARCHAR(50),
                licence VARCHAR(50),
                licenceNumber VARCHAR(50),
                price VARCHAR(50),
                priceEur VARCHAR(50),
                orderDate VARCHAR(50),
                additionalService NVARCHAR(200),
                note NVARCHAR(200))";
            CreateTable(path, sql);
        }

        public void Instals(string path) {
            string sql = @"CREATE TABLE IF NOT EXISTS instals
                (instalDate NVARCHAR(50),
                application NVARCHAR(50),
                version NVARCHAR(50),
                action NVARCHAR(50),
                ipAddress NVARCHAR(50))";
            CreateTable(path, sql);
        }

        public void Invoices(string path) {
            string sql = @"CREATE TABLE IF NOT EXISTS invoices
                (id VARCHAR (50) PRIMARY KEY,
                number INTEGER,
                fileName NVARCHAR(50),
                orderNumber INTEGER,
                dateAndTime NVARCHAR(50),
                year INTEGER,
                firstName NVARCHAR(50),
                lastName NVARCHAR(50),
                companyName NVARCHAR(50),
                address NVARCHAR(50),
                postalCode NVARCHAR(50),
                city NVARCHAR(50),
                country NVARCHAR(50),
                pin VARCHAR(50),
                note NVARCHAR(200),
                items NVARCHAR(200),
                isPaid INTEGER,
                paidAmount VARCHAR(50),
                paidDate VARCHAR(50))";
            CreateTable(path, sql);
        }
        #endregion

        public void CreateDataBase(string userId, string table) {
            try {
                string path = GetDataBasePath(userId, dataBase);
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                if (!File.Exists(path)) {
                    SQLiteConnection.CreateFile(path);
                }
                CreateTables(table, path);
            } catch (Exception e) { }
        }

        public void CreateGlobalDataBase(string path, string table) {
            try {
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                if (!File.Exists(path)) {
                    SQLiteConnection.CreateFile(path);
                }
                CreateTables(table, path);
            } catch (Exception e) { }
        }

        private void CreateTables(string table, string path) {
           
            switch (table) {
                case "users":
                    Users(path);
                    break;
                case "clients":
                    Clients(path);
                    break;
                case "clientsdata":
                    ClientsData(path);
                    break;
                case "myfoods":
                    MyFoods(path);
                    break;
                case "menues":
                    Menues(path);
                    break;
                case "prices":
                    Prices(path);
                    break;
                case "scheduler":
                    Scheduler(path);
                    break;
                case "instals":
                    Instals(path);
                    break;
                case "orders":
                    Orders(path);
                    break;
                case "invoices":
                    Invoices(path);
                    break;
                case "recipes":
                    Recipes(path);
                    break;
                case "meals":
                    Meals(path);
                    break;
                default:
                    break;
            }
        }

        private void CreateTable(string path, string sql) {
            try {
                if (File.Exists(path)){
                    SQLiteConnection connection = new SQLiteConnection("Data Source=" + path);
                    connection.Open();
                    SQLiteCommand command = new SQLiteCommand(sql, connection);
                    command.ExecuteNonQuery();
                    connection.Close();
                };
            } catch (Exception e) { }
        }

        public string GetDataBasePath(string userId, string dataBase) {
            return HttpContext.Current.Server.MapPath("~/App_Data/users/" + userId + "/" + dataBase);
        }

    }

}
