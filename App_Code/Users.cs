using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Users : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UsersDataBase"];
    string userDataBase = ConfigurationManager.AppSettings["UserDataBase"];
    string webDataBase = ConfigurationManager.AppSettings["WebDataBase"];
    DataBase db = new DataBase();
    Translate t = new Translate();
    Global G = new Global();
    Files f = new Files();
    string EncryptionKey = ConfigurationManager.AppSettings["EncryptionKey"];
    string supervisorUserName = ConfigurationManager.AppSettings["SupervisorUserName"];
    string supervisorPassword = ConfigurationManager.AppSettings["SupervisorPassword"];
    string trialDays = ConfigurationManager.AppSettings["TrialDays"];
    string headerinfo = "headerinfo.txt";
    public Users() {
    }
    public class NewUser {
        public string userId { get; set; }
        public int userType { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string companyName { get; set; }
        public string address { get; set; }
        public string postalCode { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string pin { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public int adminType { get; set; }
        public string userGroupId { get; set; }
        public string activationDate { get; set; }
        public string expirationDate { get; set; }
        public int daysToExpite { get; set; }
        public bool isActive { get; set; }
        public string licenceStatus { get; set; }
        public string ipAddress { get; set; }
        public int rowid { get; set; }
        public int subusers { get; set; }
        public int maxNumberOfUsers { get; set; }
        public string package { get; set; }

        public DataSum datasum = new DataSum();

        public string headerInfo { get; set; }  //TODO

    }

    public const string demo = "demo";
    public const string expired = "expired";
    public const string active = "active";

    public const string configFile = "config";

    public class Totals {
        public int active { get; set; }
        public int demo { get; set; }
        public int expired { get; set; }
        public int licence { get; set; }
        public int subuser { get; set; }
        public int total { get; set; }
        public double licencepercentage { get; set; }
        public Object city { get; set; }
        public Object monthly { get; set; }
        public int clientapp { get; set; }
    }

    public class UserConfig {
        public int maxNumberOfUsers;
    }

    public class DataSum {
        public ClientsTotal clients = new ClientsTotal();
        public int menues;
        public int weeklyMenus;
        public int myfoods;
        public int recipes;
        public int meals;
        public ClientsScheduler scheduler = new ClientsScheduler();
    }

    public class ClientsTotal {
        public int total;
        public string currMonth;
        public string currYear;
        public int currMonthTotal;
        public int maxMonthlyNumberOfClients;
    }

    public class ClientsScheduler {
        public int total;
        public int appointments;
    }

    public class CheckUser {
        public bool CheckUserId(string userId, bool isActive) {
            try {
                bool result = false;
                string dataBase = ConfigurationManager.AppSettings["UsersDataBase"];
                string path = HttpContext.Current.Server.MapPath("~/App_Data/" + dataBase);
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + path)) {
                    connection.Open();
                    string sql = string.Format("SELECT EXISTS (SELECT userId FROM users WHERE userId = '{0}' AND isActive = '{1}')", userId, isActive);
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        using (SQLiteDataReader reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                result = reader.GetBoolean(0);
                            }
                        }
                    }
                    connection.Close();
                }
                return result;
            } catch (Exception e) { return false; }
        }
        public string message = "you do not have permission to open this file";
    }

    #region WebMethods
    [WebMethod]
    public string Init() {
        try {
            NewUser x = new NewUser();
            x.userId = null;
            x.userType = 0;
            x.firstName = "";
            x.lastName = "";
            x.companyName = "";
            x.address = "";
            x.postalCode = "";
            x.city = "";
            x.country = "";
            x.pin = "";
            x.phone = "";
            x.email = "";
            x.userName = "";
            x.password = "";
            x.adminType = 0;
            x.userGroupId = null;
            x.activationDate = DateTime.UtcNow.ToString();
            x.expirationDate = string.IsNullOrEmpty(trialDays) ? DateTime.UtcNow.ToString() : DateTime.UtcNow.AddDays(Convert.ToInt32(trialDays)).ToString();
            x.daysToExpite = 0;
            x.isActive = string.IsNullOrEmpty(trialDays) ? false : true;
            x.licenceStatus = string.IsNullOrEmpty(trialDays) ? demo : active;
            x.ipAddress = HttpContext.Current.Request.UserHostAddress;
            x.rowid = 0;
            x.subusers = 0;
            x.maxNumberOfUsers = 1;
            x.package = "";
            x.datasum = new DataSum();
            x.headerInfo = "";
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Login(string userName, string password) {
        try {
            NewUser x = new NewUser();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = string.Format(@"SELECT userId, userType, firstName, lastName, companyName, address, postalCode, city, country, pin, phone, email, userName, password, adminType, userGroupId, activationDate, expirationDate, isActive, iPAddress FROM users WHERE lower(userName) = '{0}' AND password = '{1}'"
                                            , userName.ToLower(), Encrypt(password));
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read()) {
                        x.userId = reader.GetString(0);
                        x.userType = reader.GetInt32(1);
                        x.firstName = reader.GetString(2);
                        x.lastName = reader.GetString(3);
                        x.companyName = reader.GetString(4);
                        x.address = reader.GetString(5);
                        x.postalCode = reader.GetString(6);
                        x.city = reader.GetString(7);
                        x.country = reader.GetString(8);
                        x.pin = reader.GetString(9);
                        x.phone = reader.GetString(10);
                        x.email = reader.GetString(11);
                        x.userName = reader.GetString(12);
                        x.password = Decrypt(reader.GetString(13));
                        x.adminType = reader.GetInt32(14);
                        x.userGroupId = reader.GetString(15);
                        x.activationDate = reader.GetString(16);
                        x.expirationDate = reader.GetString(17);
                        x.daysToExpite = G.DateDiff(x.expirationDate);
                        x.isActive = Convert.ToBoolean(reader.GetInt32(18));
                        x.licenceStatus = GetLicenceStatus(x);
                        x.ipAddress = reader.GetString(19);
                        x.subusers = GetUsersCountByUserGroup(x.userGroupId, connection);
                        x.maxNumberOfUsers = GetMaxNumberOfUsers(x.userGroupId, x.userType);
                        x.package = GetPackage(x.licenceStatus, x.userType);
                        x.headerInfo = f.ReadFile(x.userGroupId, headerinfo);
                        /****** SubUsers ******/
                        if (x.userId != x.userGroupId) {
                            x = GetUserGroupInfo(x, connection);
                        }
                        /**********************/
                    }
                }
                connection.Close();
            }
            x.datasum = GetDataSum(x.userGroupId, x.userId, x.userType, x.adminType);
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Signup(NewUser x, string lang) {
        string path = HttpContext.Current.Server.MapPath("~/App_Data/" + dataBase);
        db.CreateGlobalDataBase(path, db.users);
        if (Check(x) != false) {
            return ("the email address you have entered is already registered");
        }
        else {
            try {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                    x.userId = Convert.ToString(Guid.NewGuid());
                    x.userGroupId = x.userGroupId == null ? x.userId : x.userGroupId;
                    x.password = Encrypt(x.password);
                    connection.Open();
                    string sql = @"INSERT INTO users VALUES  
                       (@UserId, @UserType, @FirstName, @LastName, @CompanyName, @Address, @PostalCode, @City, @Country, @Pin, @Phone, @Email, @UserName, @Password, @AdminType, @UserGroupId, @ActivationDate, @ExpirationDate, @IsActive, @IPAddress)";
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        command.Parameters.Add(new SQLiteParameter("userId", x.userId));
                        command.Parameters.Add(new SQLiteParameter("UserType", x.userType));
                        command.Parameters.Add(new SQLiteParameter("FirstName", x.firstName));
                        command.Parameters.Add(new SQLiteParameter("LastName", x.lastName));
                        command.Parameters.Add(new SQLiteParameter("CompanyName", x.companyName));
                        command.Parameters.Add(new SQLiteParameter("Address", x.address));
                        command.Parameters.Add(new SQLiteParameter("PostalCode", x.postalCode));
                        command.Parameters.Add(new SQLiteParameter("City", x.city));
                        command.Parameters.Add(new SQLiteParameter("Country", x.country));
                        command.Parameters.Add(new SQLiteParameter("Pin", x.pin));
                        command.Parameters.Add(new SQLiteParameter("Phone", x.phone));
                        command.Parameters.Add(new SQLiteParameter("Email", x.email.Trim().ToLower()));
                        command.Parameters.Add(new SQLiteParameter("UserName", x.userName.Trim().ToLower()));
                        command.Parameters.Add(new SQLiteParameter("Password", x.password));
                        command.Parameters.Add(new SQLiteParameter("adminType", x.adminType));
                        command.Parameters.Add(new SQLiteParameter("UserGroupId", x.userGroupId = x.userGroupId == null ? x.userId : x.userGroupId));
                        command.Parameters.Add(new SQLiteParameter("ActivationDate", x.activationDate));
                        command.Parameters.Add(new SQLiteParameter("ExpirationDate", x.expirationDate));
                        command.Parameters.Add(new SQLiteParameter("IsActive", x.isActive));
                        command.Parameters.Add(new SQLiteParameter("IPAddress", x.ipAddress));
                        using (SQLiteTransaction transaction = connection.BeginTransaction()) {
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                    }
                    connection.Close();
                }
                if (x.email.Contains("@")) {
                    SendMail(x, lang);
                }
                return ("registration completed successfully");
            } catch (Exception e) { return ("error: " + e); }
        }
    }

    [WebMethod]
    public string Update(NewUser x) {
        try {
            x.password = Encrypt(x.password);
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = @"UPDATE Users SET  
                            UserType = @UserType, FirstName = @FirstName, LastName = @LastName, CompanyName = @CompanyName, Address = @Address, PostalCode = @PostalCode, City = @City, Country = @Country, Pin = @Pin, Phone = @Phone, Email = @Email, UserName = @UserName, Password = @Password, AdminType = @AdminType, UserGroupId = @UserGroupId, ActivationDate = @ActivationDate, ExpirationDate = @ExpirationDate, IsActive = @IsActive, IPAddress = @IPAddress
                            WHERE UserId = @UserId";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.Parameters.Add(new SQLiteParameter("UserId", x.userId));
                    command.Parameters.Add(new SQLiteParameter("UserType", x.userType));
                    command.Parameters.Add(new SQLiteParameter("FirstName", x.firstName));
                    command.Parameters.Add(new SQLiteParameter("LastName", x.lastName));
                    command.Parameters.Add(new SQLiteParameter("CompanyName", x.companyName));
                    command.Parameters.Add(new SQLiteParameter("Address", x.address));
                    command.Parameters.Add(new SQLiteParameter("PostalCode", x.postalCode));
                    command.Parameters.Add(new SQLiteParameter("City", x.city));
                    command.Parameters.Add(new SQLiteParameter("Country", x.country));
                    command.Parameters.Add(new SQLiteParameter("Pin", x.pin));
                    command.Parameters.Add(new SQLiteParameter("Phone", x.phone));
                    command.Parameters.Add(new SQLiteParameter("Email", x.email));
                    command.Parameters.Add(new SQLiteParameter("UserName", x.userName));
                    command.Parameters.Add(new SQLiteParameter("Password", x.password));
                    command.Parameters.Add(new SQLiteParameter("adminType", x.adminType));
                    command.Parameters.Add(new SQLiteParameter("UserGroupId", x.userGroupId));
                    command.Parameters.Add(new SQLiteParameter("ActivationDate", x.activationDate));
                    command.Parameters.Add(new SQLiteParameter("ExpirationDate", x.expirationDate));
                    command.Parameters.Add(new SQLiteParameter("IsActive", x.isActive));
                    command.Parameters.Add(new SQLiteParameter("IPAddress", x.ipAddress));
                    using (SQLiteTransaction transaction = connection.BeginTransaction()) {
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }
                connection.Close();
            }
            if(x.userType == 2) {
                //*********** Only for userType == 2 and more than 5 users (Schools) **************
                if (x.maxNumberOfUsers > 5) {
                    UserConfig uc = new UserConfig();
                    uc.maxNumberOfUsers = x.maxNumberOfUsers;
                    string configJson = JsonConvert.SerializeObject(uc, Formatting.None);
                    f.SaveJsonToFile(string.Format("users/{0}", x.userGroupId), configFile, configJson);
                }
                //**************************************************************************
                f.SaveFile(x.userGroupId, headerinfo, x.headerInfo);
            }

            return ("saved");
        } catch (Exception e) {
            return ("error: " + e);
        }
    }

    [WebMethod]
    public string Load(int? limit, int? page) {
        try {
            return JsonConvert.SerializeObject(GetUsers(limit, page), Formatting.None);
        } catch (Exception e) {
            return (e.Message);
        }
    }

    [WebMethod]
    public string Total(int year) {
        try {
            Totals x = new Totals();
            ClientApp ca = new ClientApp();
            List<NewUser> users = GetUsers(null, null);
            x.active = users.Where(a => a.isActive == true).Count();
            x.demo = users.Where(a => a.isActive == false && a.activationDate == a.expirationDate).Count();
            x.expired = users.Where(a => a.licenceStatus == expired && G.DateDiff(a.activationDate, a.expirationDate) > 15).Count();
            x.licence = users.Where(a => a.isActive == true && a.userId == a.userGroupId && G.DateDiff(a.activationDate, a.expirationDate) > 15).Count();
            x.subuser = users.Where(a => a.isActive == true && a.userId != a.userGroupId).Count();
            x.total = users.Count();
            x.licencepercentage = x.total == x.subuser ? 0 : Math.Round((Convert.ToDouble(x.licence) / (x.total - x.subuser) * 100), 1);
            x.city = GetCityCount(users);
            x.monthly = GetMonthlyUsers(users, year);
            x.clientapp = ca.GetClientAppUsers();
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return (e.Message);
        }
    }

    [WebMethod]
    public string TotalList() {
        try {
            List<Totals> xx = new List<Totals>();
            List<NewUser> users = GetUsers(null, null);
            int i = 1;
            foreach (NewUser u in users) {
                Totals x = new Totals();
                x.active = users.Take(i).Where(a => a.isActive == true).Count();
                x.demo = users.Take(i).Where(a => a.isActive == false && a.activationDate == a.expirationDate).Count();
                x.expired = users.Take(i).Where(a => a.isActive == false && Convert.ToDateTime(a.activationDate) < Convert.ToDateTime(a.expirationDate)).Count();
                x.licence = users.Take(i).Where(a => a.isActive == true && a.userId == a.userGroupId && G.DateDiff(a.activationDate, a.expirationDate) > 15).Count();
                x.subuser = users.Take(i).Where(a => a.isActive == true && a.userId != a.userGroupId).Count();
                x.total = users.Take(i).Count();
                x.licencepercentage = x.total == x.subuser ? 0 : Math.Round((Convert.ToDouble(x.licence) / (x.total - x.subuser) * 100), 1);
                xx.Add(x);
                i++;
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return (e.Message);
        }
    }

    [WebMethod]
    public string Search(string query, int? limit, int? page, bool activeUsers) {
        try {
            List<NewUser> xx = new List<NewUser>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string limitSql = "";
                if (limit != null && page != null) {
                    limitSql = string.Format("LIMIT {0} OFFSET {1}", limit, (page - 1) * limit);
                }
                string aciveUsersSql = "";
                if (activeUsers == true) {
                    aciveUsersSql = "AND isActive = 1";
                }
                string sql = string.Format(@"
                        SELECT userId, userType, firstName, lastName, companyName, address, postalCode, city, country, pin, phone, email, userName, password, adminType, userGroupId, activationDate, expirationDate, isActive, iPAddress, rowid
                        FROM users                       
                        WHERE (firstName LIKE '%{0}%' OR lastName LIKE '%{0}%' OR companyName LIKE '%{0}%' OR email LIKE '%{0}%' OR userId LIKE '%{0}%' OR userGroupId LIKE '%{0}%') {2}
                        ORDER BY rowid DESC {1}", query, limitSql, aciveUsersSql);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewUser x = new NewUser();
                            x.userId = reader.GetString(0);
                            x.userType = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1);
                            x.firstName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.lastName = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.companyName = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                            x.address = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                            x.postalCode = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                            x.city = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                            x.country = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
                            x.pin = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                            x.phone = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                            x.email = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
                            x.userName = reader.GetValue(12) == DBNull.Value ? "" : reader.GetString(12);
                            x.password = reader.GetValue(13) == DBNull.Value ? "" : Decrypt(reader.GetString(13));
                            x.adminType = reader.GetValue(14) == DBNull.Value ? 0 : reader.GetInt32(14);
                            x.userGroupId = reader.GetString(15);
                            x.activationDate = reader.GetValue(16) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(16);
                            x.expirationDate = reader.GetValue(17) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(17);
                            x.daysToExpite = G.DateDiff(x.expirationDate);
                            x.isActive = reader.GetValue(18) == DBNull.Value ? true : Convert.ToBoolean(reader.GetInt32(18));
                            x.licenceStatus = GetLicenceStatus(x);
                            x.ipAddress = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
                            x.rowid = reader.GetValue(20) == DBNull.Value ? 0 : reader.GetInt32(20);
                            x.subusers = GetUsersCountByUserGroup(x.userGroupId, connection);
                            x.maxNumberOfUsers = GetMaxNumberOfUsers(x.userGroupId, x.userType);
                            x.package = GetPackage(x.licenceStatus, x.userType);
                            /****** SubUsers ******/
                            if (x.userId != x.userGroupId) {
                                x = GetUserGroupInfo(x, connection);
                            }
                            /**********************/
                            xx.Add(x);
                        }
                    }
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Get(string userId) {
        try {
            NewUser x = new NewUser();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = string.Format("SELECT userId, userType, firstName, lastName, companyName, address, postalCode, city, country, pin, phone, email, userName, password, adminType, userGroupId, activationDate, expirationDate, isActive, iPAddress FROM users WHERE userId = '{0}'", userId);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.userId = reader.GetString(0);
                            x.userType = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1);
                            x.firstName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.lastName = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.companyName = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                            x.address = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                            x.postalCode = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                            x.city = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                            x.country = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
                            x.pin = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                            x.phone = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                            x.email = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
                            x.userName = reader.GetValue(12) == DBNull.Value ? "" : reader.GetString(12);
                            x.password = reader.GetValue(13) == DBNull.Value ? "" : Decrypt(reader.GetString(13));
                            x.adminType = reader.GetValue(14) == DBNull.Value ? 0 : reader.GetInt32(14);
                            x.userGroupId = reader.GetString(15);
                            x.activationDate = reader.GetValue(16) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(16);
                            x.expirationDate = reader.GetValue(17) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(17);
                            x.daysToExpite = G.DateDiff(x.expirationDate);
                            x.isActive = reader.GetValue(18) == DBNull.Value ? true : Convert.ToBoolean(reader.GetInt32(18));
                            x.licenceStatus = GetLicenceStatus(x);
                            x.ipAddress = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
                            x.subusers = GetUsersCountByUserGroup(x.userGroupId, connection);
                            x.maxNumberOfUsers = GetMaxNumberOfUsers(x.userGroupId, x.userType);
                            x.package = GetPackage(x.licenceStatus, x.userType);
                            x.headerInfo = f.ReadFile(x.userGroupId, headerinfo);
                            /****** SubUsers ******/
                            if (x.userId != x.userGroupId) {
                                x = GetUserGroupInfo(x, connection);
                            }
                            /**********************/
                        }
                    }   
                }
                connection.Close();
            }
            x.datasum = GetDataSum(x.userGroupId, x.userId, x.userType, x.adminType);
            string json = JsonConvert.SerializeObject(x, Formatting.None);
            return json;
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string GetUsersByUserGroup(string userGroupId) {
        try {
            List<NewUser> xx = new List<NewUser>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = string.Format("SELECT userId, userType, firstName, lastName, companyName, address, postalCode, city, country, pin, phone, email, userName, password, adminType, userGroupId, activationDate, expirationDate, isActive, iPAddress FROM users WHERE userGroupId = '{0}'", userGroupId);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewUser x = new NewUser();
                            x.userId = reader.GetString(0);
                            x.userType = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1);
                            x.firstName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.lastName = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.companyName = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                            x.address = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                            x.postalCode = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                            x.city = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                            x.country = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
                            x.pin = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                            x.phone = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                            x.email = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
                            x.userName = reader.GetValue(12) == DBNull.Value ? "" : reader.GetString(12);
                            x.password = reader.GetValue(13) == DBNull.Value ? "" : Decrypt(reader.GetString(13));
                            x.adminType = reader.GetValue(14) == DBNull.Value ? 0 : reader.GetInt32(14);
                            x.userGroupId = reader.GetString(15);
                            x.activationDate = reader.GetValue(16) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(16);
                            x.expirationDate = reader.GetValue(17) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(17);
                            x.daysToExpite = G.DateDiff(x.expirationDate);
                            x.isActive = reader.GetValue(18) == DBNull.Value ? true : Convert.ToBoolean(reader.GetInt32(18));
                            x.licenceStatus = GetLicenceStatus(x);
                            x.ipAddress = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
                            /****** SubUsers ******/
                            if (x.userId != x.userGroupId) {
                                x = GetUserGroupInfo(x, connection);
                            }
                            x.maxNumberOfUsers = GetMaxNumberOfUsers(x.userGroupId, x.userType);
                            /**********************/
                            xx.Add(x);
                        } 
                    }
                }
                connection.Close();
            }   
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string Delete(NewUser x) {
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = string.Format("DELETE FROM users WHERE userId = '{0}'", x.userId);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return "ok";
        } catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string DeleteAllUserGroup(NewUser x) {
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = string.Format("DELETE FROM users WHERE userGroupId = '{0}'", x.userGroupId);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
                Files f = new Files();
                f.DeleteUserFolder(x.userGroupId);
            }
            return "ok";
        } catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string ForgotPassword(string email, string lang) {
        try {
            NewUser x = new NewUser();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = string.Format("SELECT userId, userType, firstName, lastName, companyName, address, postalCode, city, country, pin, phone, email, userName, password, adminType, userGroupId, activationDate, expirationDate, isActive, iPAddress FROM users WHERE email = '{0}'", email);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.userId = reader.GetString(0);
                            x.userType = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1);
                            x.firstName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.lastName = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.companyName = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                            x.address = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                            x.postalCode = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                            x.city = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                            x.country = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
                            x.pin = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                            x.phone = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                            x.email = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
                            x.userName = reader.GetValue(12) == DBNull.Value ? "" : reader.GetString(12);
                            x.password = reader.GetValue(13) == DBNull.Value ? "" : Decrypt(reader.GetString(13));
                            x.adminType = reader.GetValue(14) == DBNull.Value ? 0 : reader.GetInt32(14);
                            x.userGroupId = reader.GetString(15);
                            x.activationDate = reader.GetValue(16) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(16);
                            x.expirationDate = reader.GetValue(17) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(17);
                            x.daysToExpite = G.DateDiff(x.expirationDate);
                            x.isActive = reader.GetValue(18) == DBNull.Value ? true : Convert.ToBoolean(reader.GetInt32(18));
                            x.licenceStatus = GetLicenceStatus(x);
                            x.ipAddress = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
                            x.subusers = GetUsersCountByUserGroup(x.userGroupId, connection);
                            x.maxNumberOfUsers = GetMaxNumberOfUsers(x.userGroupId, x.userType);
                            x.package = GetPackage(x.licenceStatus, x.userType);
                            x.headerInfo = f.ReadFile(x.userGroupId, headerinfo);
                            /****** SubUsers ******/
                            if (x.userId != x.userGroupId) {
                                x = GetUserGroupInfo(x, connection);
                            }
                            /**********************/
                        }
                    }  
                    connection.Close();
                } 
            }
                
            Mail mail = new Mail();
            string messageSubject = t.Tran("nutrition program", lang).ToUpper() + " - " + t.Tran("password", lang);

            string messageBody = string.Format(
                @"
<p>{0}</p>
<p><i>{1}:</i></p>
<hr/>
<p>{2}: <strong>{3}</strong></p>
<p>{4}: <strong>{5}</strong></p>
<p>{6}: {7}</p>
<hr/>
{8}
<br />
<br />"
, t.Tran("nutrition program", lang).ToUpper()
, t.Tran("login details", lang)
, t.Tran("user name", lang)
, x.userName
, t.Tran("password", lang)
, x.password
, t.Tran("app access link", lang)
, string.Format("<a href='https://www.{0}/app'>https://www.{0}/app</a>", GetWebPage(lang))
, string.Format(@"<i>* {0}</i>", t.Tran("this is an automatically generated email – please do not reply to it", lang)));

            string response = "";
            if (x.userName == null) {
                response = t.Tran("user not found", lang);
            }
            else {
                mail.SendMail(x.email, messageSubject, messageBody, lang, null, false);
                response = t.Tran("password has been sent to your e-mail", lang);
            }

            return JsonConvert.SerializeObject(response, Formatting.None);
        } catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string GetUserSum(string userGroupId, string userId, int userType, int adminType) {
        try {
            return JsonConvert.SerializeObject(GetDataSum(userGroupId, userId, userType, adminType), Formatting.None);
        } catch (Exception e) {
            return (e.Message);
        }
    }

    [WebMethod]
    public string ConfirmPayPal(string userName, string password, string lang) {
        try {
            string userId = null;
            string response = "";
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql = string.Format("SELECT userId FROM users WHERE userName = '{0}' AND password = '{1}'", userName, Encrypt(password));
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    NewUser x = new NewUser();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            userId = reader.GetString(0);
                        }
                    }
                }
                if (string.IsNullOrEmpty(userId)) {
                    response = t.Tran("user not found", lang);
                } else {
                    sql = string.Format(@"UPDATE Users SET ExpirationDate = '{0}', IsActive = '1' WHERE UserId = '{1}' AND IsActive = '0'", DateTime.Now.AddYears(2), userId);
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                    response = "your account has been successfully activated";
                }
                connection.Close();
            } 
            return JsonConvert.SerializeObject(response, Formatting.None);
        } catch (Exception e) { return ("Error: " + e); }
    }

    //******** Only for correcting User tbl ***********
    [WebMethod]
    public string UpdateUserInfoFromOrdersTbl(string email) {
        try {
            Orders.NewUser x = new Orders.NewUser();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + webDataBase))) {
                connection.Open();
                string sql = string.Format(@"SELECT companyName, address, postalCode, city, country, pin, email
                        FROM orders where email='{0}' ORDER BY rowid DESC LIMIT 1", email);
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.companyName = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                            x.address = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                            x.postalCode = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                            x.city = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                            x.country = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                            x.pin = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                            x.email = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                        }
                    }   
                }   
                connection.Close();
            }
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                connection.Open();
                string sql1 = string.Format(@"UPDATE Users SET  
                            CompanyName='{0}', Address='{1}', PostalCode='{2}', City='{3}', Country='{4}', Pin='{5}'
                            WHERE email='{6}'", x.companyName, x.address, x.postalCode, x.city, x.country, x.pin, x.email);
                using (SQLiteCommand command = new SQLiteCommand(sql1, connection)) {
                    command.ExecuteNonQuery();
                }   
                connection.Close();
            }
            return JsonConvert.SerializeObject("OK", Formatting.None);
        } catch (Exception e) {
            return (JsonConvert.SerializeObject(e.Message, Formatting.None));
        }
    }
    //**********************************************


    //******** Creating subusers from admin (For Schools) ***********
    [WebMethod]
    public string CreateSubusers(NewUser x, string prefix) {
        try {
            for (int i = 1; i < x.maxNumberOfUsers; i++) {
                if (Check(x)) {
                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                        x.userId = Convert.ToString(Guid.NewGuid());
                        x.firstName = prefix;
                        x.lastName = i.ToString();
                        x.email = string.Format("{0}{1}", prefix, i);
                        x.userName = x.email;
                        x.password = x.email;
                        x.password = Encrypt(x.password);
                        x.adminType = 2;
                        connection.Open();
                        string sql = string.Format(@"INSERT INTO users VALUES  
                                       ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}')",
                                       x.userId, x.userType, x.firstName, x.lastName, x.companyName, x.address, x.postalCode, x.city, x.country, x.pin, x.phone, x.email, x.userName, x.password, x.adminType, x.userGroupId, x.activationDate, x.expirationDate, Convert.ToInt16(x.isActive), x.ipAddress);
                        using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                            using (SQLiteTransaction transaction = connection.BeginTransaction()) {
                                command.ExecuteNonQuery();
                                transaction.Commit();
                            }
                        }
                        connection.Close();
                    }
                }
            }
            return JsonConvert.SerializeObject("OK", Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    #endregion

    #region Methods
    protected string Encrypt(string clearText) {
        byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
        using (Aes encryptor = Aes.Create()) {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream()) {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write)) {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                clearText = Convert.ToBase64String(ms.ToArray());
            }
        }
        return clearText;
    }

    protected string Decrypt(string cipherText) {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes encryptor = Aes.Create()) {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream()) {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write)) {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
        }
        return cipherText;
    }

    private bool Check(NewUser x) {
        try {
            bool result = false;
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
                string sql = string.Format("SELECT EXISTS(SELECT userId FROM users WHERE email = '{0}')", x.email.Trim().ToLower()); 
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            result = reader.GetBoolean(0);
                        }
                    }
                }
                connection.Close();
            }
            return result;
        } catch (Exception e) { return false; }
    }

    private string Supervisor() {
        NewUser x = new NewUser();
        x.userId = Convert.ToString(Guid.NewGuid()); ;
        x.userType = 0;
        x.firstName = "Igor";
        x.lastName = "Gašparović";
        x.companyName = "IG PROG";
        x.address = "Ludvetov breg 5";
        x.postalCode = "51000";
        x.city = "Rijeka";
        x.country = "Hrvatska";
        x.pin = "58331314923";
        x.phone = "098330966";
        x.email = "igprog@yahoo.com";
        x.userName = supervisorUserName;
        x.password = supervisorPassword;
        x.adminType = 0;
        x.userGroupId = x.userId;
        x.activationDate = DateTime.UtcNow.ToString();
        x.expirationDate = DateTime.UtcNow.ToString();
        x.isActive = true;
        x.licenceStatus = active;
        x.ipAddress = "";

        string json = JsonConvert.SerializeObject(x, Formatting.None);
        return json;
    }

    private void SendMail(NewUser x, string lang) {
        Mail mail = new Mail();
        string messageSubject = t.Tran("nutrition program", lang).ToUpper() + " - " + t.Tran("registration", lang);

        string messageBody = string.Format(
                @"
<p>{0}</p>
<p>{1}</p>
<br />
<p><i>{2}:</i></p>
<hr/>
<p>{3}: <strong>{4}</strong></p>
<p>{5}: <strong>{6}</strong></p>
<p>{7}: {8}</p>
<p>({9})</p>
<hr/>
{10}
<br />
<br />"
, t.Tran("nutrition program", lang).ToUpper()
, t.Tran("registration completed successfully", lang).ToUpper()
, t.Tran("login details", lang)
, t.Tran("user name", lang)
, x.userName
, t.Tran("password", lang)
, Decrypt(x.password)
, t.Tran("app access link", lang)
, string.Format("<a href='https://www.{0}/app'>https://www.{0}/app</a>", GetWebPage(lang))
, string.Format(@"<i>{0}</i>", t.Tran("for a better experience in using the application, please use some of the modern browsers such as google chrome, mozilla firefox, microsoft edge etc.", lang))
, string.Format(@"<i>* {0}</i>", t.Tran("this is an automatically generated email – please do not reply to it", lang)));

        mail.SendMail(x.email, messageSubject, messageBody, lang, null, false);
    }

    private string GetLicenceStatus(NewUser x) {
        try {
            if (x.isActive == false) {
                return demo;
            }
            if (x.isActive == true && Convert.ToDateTime(x.expirationDate) < DateTime.UtcNow) {
                return expired;
            } else {
                return active;
            }
        } catch (Exception e) {
            return active;
        }
    }

    private List<NewUser> GetUsers(int? limit, int? page) {
        List<NewUser> xx = new List<NewUser>();
        string limitSql = "";
        if (limit != null && page != null) {
            limitSql = string.Format("LIMIT {0} OFFSET {1}", limit, (page - 1) * limit);
        }
        string sql = string.Format(@"
                    SELECT userId, userType, firstName, lastName, companyName, address, postalCode, city, country, pin, phone, email, userName, password, adminType, userGroupId, activationDate, expirationDate, isActive, iPAddress, rowid
                    FROM users
                    ORDER BY rowid DESC {0}", limitSql);
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase))) {
            connection.Open();
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    NewUser x = new NewUser();
                    x.userId = reader.GetString(0);
                    x.userType = reader.GetValue(1) == DBNull.Value ? 0 : reader.GetInt32(1);
                    x.firstName = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                    x.lastName = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                    x.companyName = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                    x.address = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                    x.postalCode = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
                    x.city = reader.GetValue(7) == DBNull.Value ? "" : reader.GetString(7);
                    x.country = reader.GetValue(8) == DBNull.Value ? "" : reader.GetString(8);
                    x.pin = reader.GetValue(9) == DBNull.Value ? "" : reader.GetString(9);
                    x.phone = reader.GetValue(10) == DBNull.Value ? "" : reader.GetString(10);
                    x.email = reader.GetValue(11) == DBNull.Value ? "" : reader.GetString(11);
                    x.userName = reader.GetValue(12) == DBNull.Value ? "" : reader.GetString(12);
                    x.password = reader.GetValue(13) == DBNull.Value ? "" : Decrypt(reader.GetString(13));
                    x.adminType = reader.GetValue(14) == DBNull.Value ? 0 : reader.GetInt32(14);
                    x.userGroupId = reader.GetString(15);
                    x.activationDate = reader.GetValue(16) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(16);
                    x.expirationDate = reader.GetValue(17) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(17);
                    x.isActive = reader.GetValue(18) == DBNull.Value ? true : Convert.ToBoolean(reader.GetInt32(18));
                    x.licenceStatus = GetLicenceStatus(x);
                    x.ipAddress = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
                    x.subusers = GetUsersCountByUserGroup(x.userGroupId, connection);
                    x.rowid = reader.GetValue(20) == DBNull.Value ? 0 : reader.GetInt32(20);
                    x.maxNumberOfUsers = GetMaxNumberOfUsers(x.userGroupId, x.userType);
                    x.headerInfo = f.ReadFile(x.userGroupId, headerinfo);
                    /****** SubUsers ******/
                    if (x.userId != x.userGroupId) {
                        x = GetUserGroupInfo(x, connection);
                    }
                    /**********************/
                    xx.Add(x);
                }
            }
            connection.Close();
        }  
        return xx;
    }

    private DataSum GetDataSum(string userGroupId, string userId, int userType, int adminType) {
        DataSum x = new DataSum();
        try {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userGroupId, userDataBase))) {
                connection.Open();
                string sql = "";
                string tbl = "";
                SQLiteCommand command = null;
                SQLiteDataReader reader = null;

                tbl = "clients";
                sql = CheckTblExistsSql(tbl);
                command = new SQLiteCommand(sql, connection);
                reader = command.ExecuteReader();
                if (IsTblExists(reader)) {
                    sql = string.Format("SELECT COUNT(rowid) FROM {0}", tbl);
                    command = new SQLiteCommand(sql, connection);
                    reader = command.ExecuteReader();
                    while (reader.Read()) {
                        x.clients.total = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                    }
                    Clients c = new Clients();
                    x.clients.currMonth = DateTime.Now.Month.ToString();
                    x.clients.currYear = DateTime.Now.Year.ToString();
                    x.clients.currMonthTotal = c.NumberOfClientsPerMonth(userGroupId);
                    x.clients.maxMonthlyNumberOfClients = c.MonthlyLimitOfClients(userType);
                }

                tbl = "menues";
                sql = CheckTblExistsSql(tbl);
                command = new SQLiteCommand(sql, connection);
                reader = command.ExecuteReader();
                if (IsTblExists(reader)) {
                    sql = string.Format("SELECT COUNT(id) FROM {0}", tbl);
                    command = new SQLiteCommand(sql, connection);
                    reader = command.ExecuteReader();
                    while (reader.Read()) {
                        x.menues = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                    }
                }

                tbl = "weeklymenus";
                sql = CheckTblExistsSql(tbl);
                command = new SQLiteCommand(sql, connection);
                reader = command.ExecuteReader();
                if (IsTblExists(reader)) {
                    sql = string.Format("SELECT COUNT(id) FROM {0}", tbl);
                    command = new SQLiteCommand(sql, connection);
                    reader = command.ExecuteReader();
                    while (reader.Read()) {
                        x.weeklyMenus = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                    }
                }

                tbl = "myfoods";
                sql = CheckTblExistsSql(tbl);
                command = new SQLiteCommand(sql, connection);
                reader = command.ExecuteReader();
                if (IsTblExists(reader)) {
                    sql = string.Format("SELECT COUNT(id) FROM {0}", tbl);
                    command = new SQLiteCommand(sql, connection);
                    reader = command.ExecuteReader();
                    while (reader.Read()) {
                        x.myfoods = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                    }
                }

                tbl = "recipes";
                sql = CheckTblExistsSql(tbl);
                command = new SQLiteCommand(sql, connection);
                reader = command.ExecuteReader();
                if (IsTblExists(reader)) {
                    sql = string.Format("SELECT COUNT(id) FROM {0}", tbl);
                    command = new SQLiteCommand(sql, connection);
                    reader = command.ExecuteReader();
                    while (reader.Read()) {
                        x.recipes = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                    }
                }

                tbl = "meals";
                sql = CheckTblExistsSql(tbl);
                command = new SQLiteCommand(sql, connection);
                reader = command.ExecuteReader();
                if (IsTblExists(reader)) {
                    sql = string.Format("SELECT COUNT(id) FROM {0}", tbl);
                    command = new SQLiteCommand(sql, connection);
                    reader = command.ExecuteReader();
                    while (reader.Read()) {
                        x.meals = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                    }
                }

                tbl = "scheduler";
                sql = CheckTblExistsSql(tbl);
                command = new SQLiteCommand(sql, connection);
                reader = command.ExecuteReader();
                if (IsTblExists(reader)) {
                    sql = string.Format(@"SELECT COUNT(rowid) FROM {0} {1}"
                                    , tbl, adminType == 0 ? "" : string.Format(" WHERE userId = '{0}'", userId));
                    command = new SQLiteCommand(sql, connection);
                    reader = command.ExecuteReader();
                    while (reader.Read()) {
                        x.scheduler.total = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                    }
                    sql = string.Format(@"SELECT COUNT(rowid) FROM {0} WHERE CAST((startDate/1000) AS INT) > CAST(strftime('%s', 'now') AS INT) {1}"
                                      , tbl, adminType == 0 ? "" : string.Format(" AND userId = '{0}'", userId));
                    command = new SQLiteCommand(sql, connection);
                    reader = command.ExecuteReader();
                    while (reader.Read()) {
                        x.scheduler.appointments = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                    }
                }
                connection.Close();
            };
            return x;
        } catch (Exception e) { return x; }
    }

    private int GetUsersCountByUserGroup(string userGroupId, SQLiteConnection connection) {
        int x = 0;
        string sql = string.Format("SELECT COUNT(userId) FROM users WHERE userGroupId = '{0}'", userGroupId);
        using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
            using (SQLiteDataReader reader = command.ExecuteReader()) {
                List<NewUser> xx = new List<NewUser>();
                while (reader.Read()) {
                    x = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0) - 1;
                }
            }  
        }
        return x;
    }

    public object GetCityCount(List<NewUser> users) {
        var aa = from r in users
                 where r.isActive == true && r.userId == r.userGroupId && G.DateDiff(r.activationDate, r.expirationDate) > 15
                 orderby r.city
                 group r by r.city.ToUpper() into g
                 select new { name = g.Key, count = g.Count() };
        aa = aa.OrderByDescending(a => a.count);
        return aa.ToList();
    }

    public object GetMonthlyUsers(List<NewUser> users, int year) {
        System.Globalization.CultureInfo culturInfo = new System.Globalization.CultureInfo("hr-HR", false);
        var aa = from r in users
                 where Convert.ToDateTime(r.activationDate).Year == Convert.ToInt32(year)
                 group r by Convert.ToDateTime(r.activationDate).Month into g
                 select new {
                     month = culturInfo.DateTimeFormat.GetMonthName(g.Key).ToUpper(),
                     registration = g.Count(),
                     activation = g.Count(a => a.isActive == true),
                     licence = g.Count(a => a.isActive == true && a.userId == a.userGroupId && G.DateDiff(a.activationDate, a.expirationDate) > 15),
                     percentage = Math.Round((decimal)(g.Count(a => a.isActive == true && a.userId == a.userGroupId && G.DateDiff(a.activationDate, a.expirationDate) > 15) * 100) / g.Count(), 1)
                 };
        return aa.ToList();
    }

    private string GetWebPage(string lang) {
        switch (lang) {
            case "en":
                return "nutriprog.com";
            /*  case "hr":
                  return "programprehrane.com";
              case "sr": case "sr_cyrl":
                  return "plan-ishrane.com"; */
            default:
                return "programprehrane.com";
        }
    }

    private string GetEmail(string lang) {
        switch (lang) {
            case "en":
                return ConfigurationManager.AppSettings["myEmail_en"];
            default:
                return ConfigurationManager.AppSettings["myEmail"];
        }
    }

    private NewUser GetUserGroupInfo(NewUser x, SQLiteConnection connection) {
        try {
            string sql = string.Format(@"SELECT userType, companyName, pin, expirationDate FROM users WHERE userId  = '{0}'", x.userGroupId);
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x.userType = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                        x.companyName = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                        x.pin = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                        x.expirationDate = reader.GetValue(3) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(3);
                        x.licenceStatus = GetLicenceStatus(x);
                        x.maxNumberOfUsers = GetMaxNumberOfUsers(x.userGroupId, x.userType);
                        x.package = GetPackage(x.licenceStatus, x.userType);
                    }
                }   
            }
            return x;
        } catch (Exception e) { return new NewUser(); }
    }

    private string CheckTblExistsSql(string tbl) {
        return string.Format("SELECT Count(*) FROM sqlite_master WHERE type='table' AND name='{0}'", tbl);
    }

    private bool IsTblExists(SQLiteDataReader reader) {
        int count = 0;
        while (reader.Read()) {
            count = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
        }
        return count > 0 ? true : false;
    }

    private int GetMaxNumberOfUsers(string userId, int userType) {
        switch (userType) {
            case 0: return 1;  // START
            case 1: return 2;  // STANDARD
            case 2: return GetUserConfig(userId).maxNumberOfUsers > 5 ? GetUserConfig(userId).maxNumberOfUsers : 5;  // PREMIUM
            default: return 1;
        }
    }

    private UserConfig GetUserConfig(string userId) {
        UserConfig x = new UserConfig();
        x.maxNumberOfUsers = 1;
        try {
            Files f = new Files();
            string json = f.GetFile(string.Format("users/{0}", userId), configFile);
            if (!string.IsNullOrEmpty(json)) {
                return JsonConvert.DeserializeObject<UserConfig>(json);
            } else {
                return x;
            }
        } catch (Exception e) {
            return x;
        }
    }

    private string GetPackage(string licenceStatus, int userType) {
        if (licenceStatus == "demo") {
            return "demo";
        }
        switch (userType) {
            case 0: return "start";
            case 1: return "standard";
            case 2: return "premium";
            default: return "";
        }
    }
    #endregion

}
