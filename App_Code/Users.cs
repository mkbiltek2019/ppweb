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
    DataBase db = new DataBase();
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
    }

    public string demo = "demo";
    public string expired = "expired";
    public string active = "active";

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
    public string Singup(NewUser x) {
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
                SendMail(x);
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
    public string Load() {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = "SELECT userId, userType, firstName, lastName, companyName, address, postalCode, city, country, pin, phone, email, userName, password, adminType, userGroupId, activationDate, expirationDate, isActive, iPAddress FROM users ORDER BY rowid DESC";
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
               // x.password = reader.GetValue(13) == DBNull.Value ? "" : Decrypt(reader.GetString(13));
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
        } catch (Exception e) { return ("error: " + e); }
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
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("error: " + e); }
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
    public string ForgotPassword(string email) {
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
            string messageSubject = "Program Prehrane - lozinka";
            string messageBody = string.Format(
                @"
<p>Podaci za prijavu u aplikaciju <strong>Program Prehrane Web</strong>:</p>
<p>Korisničko ime: <strong>{0}</strong></p>
<p>Lozinka: <strong>{1}</strong></p>
<p>Prijava u aplikaciju: <a href=""http://www.programprehrane.com"">www.programprehrane.com</a></p>
<br />
<br />
<div style=""color:gray"">
<p>IG PROG - obrt za računalno programiranje</p>
<p>Ludvetov breg 5, 51000 Rijeka, HR</p>
<p>+385 98 330 966</p>
<a href=""mailto:program.prehrane@yahoo.com"">program.prehrane@yahoo.com</a>
<br />
<a href=""http://www.programprehrane.com"">www.programprehrane.com</a>
</div>"
, x.userName
, x.password);

            string response = "";
            if (x.userName == null) {
                response = "wrong e-mail";
            }
            else {
                mail.SendMail(x.email, messageSubject, messageBody);
                response = "password has been sent to your e-mail";
            }

            string json = JsonConvert.SerializeObject(response, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("error: " + e); }
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
        x.companyName = "IGPROG";
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

    private void SendMail(NewUser x) {
        Mail mail = new Mail();
        string messageSubject = "Program Prehrane - Registracija";
        string messageBody = string.Format(
            @"
<p>Uspješno ste se registrirali u aplikaciju <strong>Program Prehrane Web</strong></p>
<br />
<p>Korisničko ime: <strong>{0}</strong></p>
<p>Lozinka: <strong>{1}</strong></p>
<br />
<p>Prijava u aplikaciju: <a href=""http://www.programprehrane.com/app"">www.programprehrane.com</a>.</p>
<br />
<br />
<div style=""color:gray"">
<p>IG PROG - obrt za računalno programiranje</p>
<p>Ludvetov breg 5, 51000 Rijeka, HR</p>
<p>+385 98 330 966</p>
<a href=""mailto:program.prehrane@yahoo.com"">program.prehrane@yahoo.com</a>
<br />
<a href=""http://www.programprehrane.com"">www.programprehrane.com</a>
</div>"
, x.userName
, Decrypt(x.password));
            mail.SendMail(x.email, messageSubject, messageBody);
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
    #endregion

}
