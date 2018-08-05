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
    string EncryptionKey = ConfigurationManager.AppSettings["EncryptionKey"];
    string supervisorUserName = ConfigurationManager.AppSettings["SupervisorUserName"];
    string supervisorPassword = ConfigurationManager.AppSettings["SupervisorPassword"];
    public Users () {
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
        public bool isActive { get; set; }
        public string licenceStatus { get; set; }
        public string ipAddress { get; set; }
        public int rowid { get; set; }
        public int subusers { get; set; }

        public DataSum datasum = new DataSum();
    }

    public string demo = "demo";
    public string expired = "expired";
    public string active = "active";

    public class Totals {
        public int active { get; set; }
        public int demo { get; set; }
        public int expired { get; set; }
        public int licence { get; set; }
        public int subuser { get; set; }
        public int total { get; set; }
        public double licencepercentage { get; set; }
        public Object city { get; set; } 
    }

    public class DataSum {
        public int clients { get; set; }
        public int menues { get; set; }
        public int myfoods {get;set;}
        public int recipes { get; set; }
        public int scheduler { get; set; }
    }

    public class CheckUser {
        public bool CheckUserId(string userId, bool isActive) {
            try {
                bool result = false;
                string dataBase = ConfigurationManager.AppSettings["UsersDataBase"];
                string path = HttpContext.Current.Server.MapPath("~/App_Data/" +  dataBase);
                SQLiteConnection connection = new SQLiteConnection("Data Source=" + path);
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(
                     "SELECT EXISTS (SELECT userId FROM users WHERE userId = @userId AND isActive = @isActive)", connection);
                command.Parameters.Add(new SQLiteParameter("userId", userId));
                command.Parameters.Add(new SQLiteParameter("isActive", isActive));
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    result = reader.GetBoolean(0);
                }
                connection.Close();
                return result;
            } catch (Exception e) { return false; }
        }
        public string message = "you do not have permission to open this file";
    }

    #region WebMethods
    [WebMethod]
    public string Init() {
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
            x.expirationDate = DateTime.UtcNow.ToString();
            x.isActive = false;
            x.licenceStatus = demo;
            x.ipAddress = HttpContext.Current.Request.UserHostAddress;
            x.rowid = 0;
            x.subusers = 0;
            x.datasum = new DataSum();
        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    [WebMethod]
    public string Login(string userName, string password) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = "SELECT userId, userType, firstName, lastName, companyName, address, postalCode, city, country, pin, phone, email, userName, password, adminType, userGroupId, activationDate, expirationDate, isActive, iPAddress FROM users WHERE userName = @userName AND password = @password";
            SQLiteCommand command = new SQLiteCommand(
                  sql, connection);
            command.Parameters.Add(new SQLiteParameter("userName", userName));
            command.Parameters.Add(new SQLiteParameter("password", Encrypt(password)));
            NewUser x = new NewUser();
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
                x.expirationDate = x.userId != x.userGroupId ? GetUserGroupExpirationDate(x.userGroupId, connection) : reader.GetString(17);
                x.isActive = Convert.ToBoolean(reader.GetInt32(18));
                x.licenceStatus = GetLicenceStatus(x);
                x.ipAddress = reader.GetString(19);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        }
        catch (Exception e) { return ("Error: " + e); }
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
                SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
                x.userId = Convert.ToString(Guid.NewGuid());
                x.userGroupId = x.userGroupId == null ? x.userId : x.userGroupId;
                x.password = Encrypt(x.password);
                connection.Open();
                string sql = @"INSERT INTO users VALUES  
                       (@UserId, @UserType, @FirstName, @LastName, @CompanyName, @Address, @PostalCode, @City, @Country, @Pin, @Phone, @Email, @UserName, @Password, @AdminType, @UserGroupId, @ActivationDate, @ExpirationDate, @IsActive, @IPAddress)";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
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
                command.ExecuteNonQuery();
                connection.Close();
                SendMail(x, lang);
                return ("registration completed successfully");
            } catch (Exception e) { return ("error: " + e); }
        }
    }

    [WebMethod]
    public string Update(NewUser x) {
        try {
            x.password = Encrypt(x.password);
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"UPDATE Users SET  
                            UserType = @UserType, FirstName = @FirstName, LastName = @LastName, CompanyName = @CompanyName, Address = @Address, PostalCode = @PostalCode, City = @City, Country = @Country, Pin = @Pin, Phone = @Phone, Email = @Email, UserName = @UserName, Password = @Password, AdminType = @AdminType, UserGroupId = @UserGroupId, ActivationDate = @ActivationDate, ExpirationDate = @ExpirationDate, IsActive = @IsActive, IPAddress = @IPAddress
                            WHERE UserId = @UserId";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
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
            command.ExecuteNonQuery();
            connection.Close();
            return ("saved");
        }
        catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string Load(int? limit, int? page) {
        try {
            return JsonConvert.SerializeObject(GetUsers(limit, page), Formatting.Indented);
        } catch (Exception e) {
            return (e.Message);
        }
    }

    [WebMethod]
    public string Total() {
        try {
            Totals x = new Totals();
            List<NewUser> users = GetUsers(null, null);
            x.active = users.Where(a => a.isActive == true).Count();
            x.demo = users.Where(a => a.isActive == false && a.activationDate == a.expirationDate).Count();
            x.expired = users.Where(a => a.isActive == false && Convert.ToDateTime(a.activationDate) < Convert.ToDateTime(a.expirationDate)).Count();
            x.licence = users.Where(a => a.isActive == true && a.userId == a.userGroupId).Count();
            x.subuser = users.Where(a => a.isActive == true && a.userId != a.userGroupId).Count();
            x.total = users.Count();
            x.licencepercentage = x.total == x.subuser ? 0 : Math.Round((Convert.ToDouble(x.licence) / (x.total - x.subuser) * 100), 1);
            x.city = GetCityCount(users);
            return JsonConvert.SerializeObject(x, Formatting.Indented);
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
            foreach(NewUser u in users) {
                Totals x = new Totals();
                x.active = users.Take(i).Where(a => a.isActive == true).Count();
                x.demo = users.Take(i).Where(a => a.isActive == false && a.activationDate == a.expirationDate).Count();
                x.expired = users.Take(i).Where(a => a.isActive == false && Convert.ToDateTime(a.activationDate) < Convert.ToDateTime(a.expirationDate)).Count();
                x.licence = users.Take(i).Where(a => a.isActive == true && a.userId == a.userGroupId).Count();
                x.subuser = users.Take(i).Where(a => a.isActive == true && a.userId != a.userGroupId).Count();
                x.total = users.Take(i).Count();
                x.licencepercentage = x.total == x.subuser ? 0 : Math.Round((Convert.ToDouble(x.licence) / (x.total - x.subuser) * 100), 1);
                xx.Add(x);
                i++;
            }
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) {
            return (e.Message);
        }
    }

    [WebMethod]
    public string Search(string query, int? limit, int? page, bool activeUsers) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
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
                        WHERE (firstName LIKE '%{0}%' OR lastName LIKE '%{0}%' OR companyName LIKE '%{0}%' OR email LIKE '%{0}%') {2}
                        ORDER BY rowid DESC {1}", query, limitSql, aciveUsersSql);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewUser> xx = new List<NewUser>();
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
                x.rowid = reader.GetValue(20) == DBNull.Value ? 0 : reader.GetInt32(20);
                x.subusers = GetUsersCountByUserGroup(x.userGroupId, connection);
                xx.Add(x);
            }
            connection.Close();
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string Get(string userId) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT userId, userType, firstName, lastName, companyName, address, postalCode, city, country, pin, phone, email, userName, password, adminType, userGroupId, activationDate, expirationDate, isActive, iPAddress FROM users WHERE userId = @userId", connection);
            command.Parameters.Add(new SQLiteParameter("userId", userId));
            NewUser x = new NewUser();
            SQLiteDataReader reader = command.ExecuteReader();
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
                x.isActive = reader.GetValue(18) == DBNull.Value ? true : Convert.ToBoolean(reader.GetInt32(18));
                x.licenceStatus = GetLicenceStatus(x);
                x.ipAddress = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
            }
            connection.Close();
            x.datasum = GetDataSum(userId);
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        } catch (Exception e) { return (e.Message); }
    }

    [WebMethod]
    public string GetUsersByUserGroup(string userGroupId) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT userId, userType, firstName, lastName, companyName, address, postalCode, city, country, pin, phone, email, userName, password, adminType, userGroupId, activationDate, expirationDate, isActive, iPAddress FROM users WHERE userGroupId = @userGroupId", connection);
            command.Parameters.Add(new SQLiteParameter("userGroupId", userGroupId));
            SQLiteDataReader reader = command.ExecuteReader();
            List<NewUser> xx = new List<NewUser>();
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
                x.expirationDate = x.userId != x.userGroupId ? GetUserGroupExpirationDate(x.userGroupId, connection) : reader.GetString(17);
                x.isActive = reader.GetValue(18) == DBNull.Value ? true : Convert.ToBoolean(reader.GetInt32(18));
                x.licenceStatus = GetLicenceStatus(x);
                x.ipAddress = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        }
        catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string Delete(NewUser x) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"DELETE FROM users WHERE userId = @userId";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("userId", x.userId));
            command.ExecuteNonQuery();
            connection.Close();
            return "ok";
        } catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string ForgotPassword(string email, string lang) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT userId, userType, firstName, lastName, companyName, address, postalCode, city, country, pin, phone, email, userName, password, adminType, userGroupId, activationDate, expirationDate, isActive, iPAddress FROM users WHERE email = @email", connection);
            command.Parameters.Add(new SQLiteParameter("email", email));
            NewUser x = new NewUser();
            SQLiteDataReader reader = command.ExecuteReader();
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
                x.isActive = reader.GetValue(18) == DBNull.Value ? true : Convert.ToBoolean(reader.GetInt32(18));
                x.licenceStatus = GetLicenceStatus(x);
                x.ipAddress = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
            }
            connection.Close();

            Mail mail = new Mail();
            string messageSubject = t.Tran("nutrition plan", lang).ToUpper() + " - " + t.Tran("password", lang);
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
<br />
<div style=""color:gray"">
<p>{9}</p>
<p>Ludvetov breg 5, 51000 Rijeka, HR</p>
{10}
{11}
<br />
{12}
</div>"
, t.Tran("nutrition plan", lang).ToUpper()
, t.Tran("login details", lang)
, t.Tran("user name", lang)
, x.userName
, t.Tran("password", lang)
, x.password
, t.Tran("app access link", lang)
, string.Format("<a href='https://www.{0}/app'>https://www.{0}/app</a>", GetWebPage(lang))
, string.Format(@"<i>* {0}</i>", t.Tran("this is an automatically generated email – please do not reply to it", lang))
, lang == "en" ? "IG PROG" : "IG PROG - obrt za računalno programiranje"
, lang == "en" ? "" : string.Format("<p>{0}</p>", "+385 98 330 966")
, string.Format("<a href='mailto:{0}'>{0}</a>", GetEmail(lang))
, string.Format("<a href='https://www.{0}'>www.{0}</a>", GetWebPage(lang)));

            string response = "";
            if (x.userName == null) {
                response = t.Tran("user not found",lang);
            }
            else {
                mail.SendMail(x.email, messageSubject, messageBody, lang);
                response = t.Tran("password has been sent to your e-mail",lang);
            }

            string json = JsonConvert.SerializeObject(response, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string GetUserSum(string userId) {
        try {
            return JsonConvert.SerializeObject(GetDataSum(userId), Formatting.Indented);
        } catch (Exception e) {
            return (e.Message);
        }
    }

    [WebMethod]
    public string ConfirmPayPal(string userName, string password, string lang) {
        try {
            string userId = null;
            string response = "";
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = string.Format("SELECT userId FROM users WHERE userName = '{0}' AND password = '{1}'", userName, Encrypt(password));
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            NewUser x = new NewUser();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                userId = reader.GetString(0);
            }
            if (string.IsNullOrEmpty(userId)) {
                connection.Close();
                response = t.Tran("user not found", lang);
            } else {
                sql = string.Format(@"UPDATE Users SET ExpirationDate = '{0}', IsActive = '1' WHERE UserId = '{1}' AND IsActive = '0'", DateTime.Now.AddYears(2), userId);
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                response = "your account has been successfully activated";
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(response, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    //******** Only for correcting User tbl ******************
    [WebMethod]
    public string UpdateUserInfoFromOrdersTbl(string email) {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + webDataBase));
            connection.Open();
            string sql = string.Format(@"SELECT companyName, address, postalCode, city, country, pin, email
                        FROM orders where email='{0}' ORDER BY rowid DESC LIMIT 1", email);
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            Orders.NewUser x = new Orders.NewUser();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.companyName = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.address = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.postalCode = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.city = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                x.country = reader.GetValue(4) == DBNull.Value ? "" : reader.GetString(4);
                x.pin = reader.GetValue(5) == DBNull.Value ? "" : reader.GetString(5);
                x.email = reader.GetValue(6) == DBNull.Value ? "" : reader.GetString(6);
            }
            connection.Close();

            connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql1 = string.Format(@"UPDATE Users SET  
                            CompanyName='{0}', Address='{1}', PostalCode='{2}', City='{3}', Country='{4}', Pin='{5}'
                            WHERE email='{6}'", x.companyName, x.address, x.postalCode, x.city, x.country, x.pin, x.email);
            command = new SQLiteCommand(sql1, connection);
            command.ExecuteNonQuery();
            connection.Close();

            return JsonConvert.SerializeObject("OK", Formatting.Indented);
        } catch (Exception e) {
            return (e.Message);
        }
    }
    //*****************************************************
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
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(
                 "SELECT EXISTS (SELECT userId FROM users WHERE email = @email)", connection);
            command.Parameters.Add(new SQLiteParameter("email", x.email.Trim().ToLower()));
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                result = reader.GetBoolean(0);
            }
            connection.Close();
            return result;
        } catch (Exception e) { return false; }
    }

    private string Supervisor() {
        NewUser x = new NewUser();
        x.userId = Convert.ToString(Guid.NewGuid());;
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

        string json = JsonConvert.SerializeObject(x, Formatting.Indented);
        return json;
    }

    private void SendMail(NewUser x, string lang) {
        Mail mail = new Mail();
        string messageSubject = t.Tran("nutrition plan", lang).ToUpper() + " - " + t.Tran("registration", lang);
        string messageBody = string.Format(
                @"
<p>{0}</p>
<p>{13}</p>
<br />
<p><i>{1}:</i></p>
<hr/>
<p>{2}: <strong>{3}</strong></p>
<p>{4}: <strong>{5}</strong></p>
<p>{6}: {7}</p>
<hr/>
{8}
<br />
<br />
<div style=""color:gray"">
<p>{9}</p>
<p>Ludvetov breg 5, 51000 Rijeka, HR</p>
{10}
{11}
<br />
{12}
</div>"
, t.Tran("nutrition plan", lang).ToUpper()
, t.Tran("login details", lang)
, t.Tran("user name", lang)
, x.userName
, t.Tran("password", lang)
, Decrypt(x.password)
, t.Tran("app access link", lang)
, string.Format("<a href='https://www.{0}/app'>https://www.{0}/app</a>", GetWebPage(lang))
, string.Format(@"<i>* {0}</i>", t.Tran("this is an automatically generated email – please do not reply to it", lang))
, lang == "en" ? "IG PROG" : "IG PROG - obrt za računalno programiranje"
, lang == "en" ? "" : string.Format("<p>{0}</p>", "+385 98 330 966")
, string.Format("<a href='mailto:{0}'>{0}</a>", GetEmail(lang))
, string.Format("<a href='https://www.{0}'>www.{0}</a>", GetWebPage(lang))
, t.Tran("registration completed successfully", lang).ToUpper());

            mail.SendMail(x.email, messageSubject, messageBody, lang);
    }

    private string GetLicenceStatus(NewUser x) {
        if (x.isActive == false) {
            return demo;
        }
        if (x.isActive == true && Convert.ToDateTime(x.expirationDate) < DateTime.UtcNow) {
            return expired;
        } else {
            return active;
        }
    }

    private string GetUserGroupExpirationDate(string userGroupId, SQLiteConnection connection) {
        try {
            SQLiteCommand command = new SQLiteCommand("SELECT expirationDate FROM users WHERE userGroupId = @userGroupId", connection);
            command.Parameters.Add(new SQLiteParameter("userGroupId", userGroupId));
            SQLiteDataReader reader = command.ExecuteReader();
            string expirationDate = "";
            while (reader.Read()) {
                 expirationDate = reader.GetValue(0) == DBNull.Value ? DateTime.UtcNow.ToString() : reader.GetString(0);
            }
            return expirationDate;
        }
        catch (Exception e) { return ("error: " + e); }
    }

    private List<NewUser> GetUsers(int? limit, int? page) {
        SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
        connection.Open();
        string limitSql = "";
        if (limit !=null && page != null) {
            limitSql = string.Format("LIMIT {0} OFFSET {1}", limit, (page - 1) * limit);
        }
        string sql = string.Format(@"
                    SELECT userId, userType, firstName, lastName, companyName, address, postalCode, city, country, pin, phone, email, userName, password, adminType, userGroupId, activationDate, expirationDate, isActive, iPAddress, rowid
                    FROM users
                    ORDER BY rowid DESC {0}", limitSql);
        SQLiteCommand command = new SQLiteCommand(sql, connection);
        SQLiteDataReader reader = command.ExecuteReader();
        List<NewUser> xx = new List<NewUser>();
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
            x.expirationDate = x.userId != x.userGroupId ? GetUserGroupExpirationDate(x.userGroupId, connection) : reader.GetString(17);
            x.isActive = reader.GetValue(18) == DBNull.Value ? true : Convert.ToBoolean(reader.GetInt32(18));
            x.licenceStatus = GetLicenceStatus(x);
            x.ipAddress = reader.GetValue(19) == DBNull.Value ? "" : reader.GetString(19);
            x.rowid = reader.GetValue(20) == DBNull.Value ? 0 : reader.GetInt32(20);
            x.subusers = GetUsersCountByUserGroup(x.userGroupId, connection);
            xx.Add(x);
        }
        connection.Close();
        return xx;
    }

    private DataSum GetDataSum(string userId) {
        DataSum x = new DataSum();
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, userDataBase));
            connection.Open();
            string sql = "SELECT COUNT(rowid) FROM clients";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.clients = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
            }
            sql = "SELECT COUNT(id) FROM menues";
            command = new SQLiteCommand(sql, connection);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                x.menues = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
            }
            sql = "SELECT COUNT(id) FROM myfoods";
            command = new SQLiteCommand(sql, connection);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                x.myfoods = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
            }
            sql = "SELECT COUNT(id) FROM recipes";
            command = new SQLiteCommand(sql, connection);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                x.recipes = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
            }
            sql = "SELECT COUNT(rowid) FROM scheduler";
            command = new SQLiteCommand(sql, connection);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                x.scheduler = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
            }
            connection.Close();
            return x;
        } catch (Exception e) { return x; }
    }

    private int GetUsersCountByUserGroup(string userGroupId, SQLiteConnection connection) {
        int x = 0;
        string sql = string.Format("SELECT COUNT(userId) FROM users WHERE userGroupId = '{0}'", userGroupId);
        SQLiteCommand command = new SQLiteCommand(sql, connection);
        SQLiteDataReader reader = command.ExecuteReader();
        List<NewUser> xx = new List<NewUser>();
        while (reader.Read()) {
            x = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
        }
        return x;
    }

    public Object GetCityCount(List<NewUser> users) {
        var aa = from r in users
                 where r.isActive == true
                 orderby r.city
                 group r by r.city.ToUpper() into g
                 select new { name = g.Key, count = g.Count() };
        aa = aa.OrderByDescending(a => a.count);
        return aa.ToList();
    }

    private string GetWebPage(string lang) {
        switch (lang) {
            case "en":
                return "nutrition-plan.com";
            case "hr":
                return "programprehrane.com";
            case "sr": case "sr_cyrl":
                return "plan-ishrane.com";
            default:
                return "programprehrane.com";
        }
    }
      
    private string GetEmail(string lang) {
        switch (lang) {
            case "en":
                return "nutrition.plan@yahoo.com";
            default:
                return "program.prehrane@yahoo.com";
        }
    }
    #endregion

}
