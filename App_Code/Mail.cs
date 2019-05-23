using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Text;
using Igprog;

/// <summary>
/// SendMail
/// </summary>
[WebService(Namespace = "http://programprehrane.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Mail : System.Web.Services.WebService {
    string myEmail = ConfigurationManager.AppSettings["myEmail"];
    string myPassword = ConfigurationManager.AppSettings["myPassword"];
    int myServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["myServerPort"]);
    string myServerHost = ConfigurationManager.AppSettings["myServerHost"];
    string myEmail_en = ConfigurationManager.AppSettings["myEmail_en"];
    string myPassword_en = ConfigurationManager.AppSettings["myPassword_en"];
    int myServerPort_en = Convert.ToInt32(ConfigurationManager.AppSettings["myServerPort_en"]);
    string myServerHost_en = ConfigurationManager.AppSettings["myServerHost_en"];
    string myEmail_cc = ConfigurationManager.AppSettings["myEmail_cc"];
    double usd = Convert.ToDouble(ConfigurationManager.AppSettings["USD"]);
    Translate t = new Translate();

    public Mail() {
    }

    #region WebMethods
    [WebMethod]
    public string Send(string name, string email, string messageSubject, string message, string lang) {
       string messageBody = string.Format(
           @"
<hr>{0}</h3>
<p>{1}: {2}</p>
<p>{3}: {4}</p>
<p>{5}: {6}</p>", t.Tran("new inquiry", lang), t.Tran("name", lang), name, t.Tran("email", lang), email, t.Tran("message", lang), message);
        try {
            bool sent = SendMail(myEmail, messageSubject, messageBody, lang, null, true); /*SendMail(myEmail, messageSubject, messageBody, lang);*/
            return sent == true ? t.Tran("ok", lang) : t.Tran("mail is not sent", lang);

        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string SendMenu(string email, Menues.NewMenu currentMenu, Users.NewUser user, string lang) {
        try {
            StringBuilder sb = new StringBuilder();
            StringBuilder meal1 = new StringBuilder();
            StringBuilder meal2 = new StringBuilder();
            StringBuilder meal3 = new StringBuilder();
            StringBuilder meal4 = new StringBuilder();
            StringBuilder meal5 = new StringBuilder();
            StringBuilder meal6 = new StringBuilder();

            sb.AppendLine(string.Format(@"<h3>{0}</h3>", currentMenu.title));
            if (!string.IsNullOrWhiteSpace(currentMenu.note)) {
                sb.AppendLine(string.Format(@"<p>{0}</p>", currentMenu.note));
            }
            sb.AppendLine("<hr />");

            foreach (Meals.NewMeal x in currentMenu.data.meals) {
                switch (x.code) {
                    case "B":
                        meal1.AppendLine(AppendMeal(x, currentMenu.data.selectedFoods));
                        break;
                    case "MS":
                        meal2.AppendLine(AppendMeal(x, currentMenu.data.selectedFoods));
                        break;
                    case "L":
                        meal3.AppendLine(AppendMeal(x, currentMenu.data.selectedFoods));
                        break;
                    case "AS":
                        meal4.AppendLine(AppendMeal(x, currentMenu.data.selectedFoods));
                        break;
                    case "D":
                        meal5.AppendLine(AppendMeal(x, currentMenu.data.selectedFoods));
                        break;
                    case "MBS":
                        meal6.AppendLine(AppendMeal(x, currentMenu.data.selectedFoods));
                        break;
                    default:
                        break;
                }
            }

            sb.AppendLine(meal1.ToString());
            sb.AppendLine(meal2.ToString());
            sb.AppendLine(meal3.ToString());
            sb.AppendLine(meal4.ToString());
            sb.AppendLine(meal5.ToString());
            sb.AppendLine(meal6.ToString());

            sb.AppendLine("<hr />");
            sb.AppendLine(string.Format(@"<i>* {0}</i>", t.Tran("this is an automatically generated email – please do not reply to it", lang)));

            string subject = string.Format("{0} - {1}"
                , !string.IsNullOrWhiteSpace(user.companyName) ? user.companyName : string.Format("{0} {1}", user.firstName, user.lastName)
                , currentMenu.title);

            bool sent = SendMail_menu(email, subject, sb.ToString(), lang, null);  // SendMail(email, subject, sb.ToString(), lang);
            return sent == true ? t.Tran("menu sent successfully", lang) : t.Tran("menu is not sent", lang);

        } catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string SendWeeklyMenu(string email, Users.NewUser user, string pdfLink, string title, string note, string lang) {
        try {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format(@"<h3>{0}</h3>", title));
            if (!string.IsNullOrWhiteSpace(note)) {
                sb.AppendLine(string.Format(@"<p>{0}</p>", note));
            }
            sb.AppendLine("<hr />");
            sb.AppendLine(string.Format(@"<i>* {0}</i>", t.Tran("this is an automatically generated email – please do not reply to it", lang)));

            string subject = string.Format("{0} - {1}"
                , !string.IsNullOrWhiteSpace(user.companyName) ? user.companyName : string.Format("{0} {1}", user.firstName, user.lastName)
                , title);

            bool sent = SendMail_menu(email, subject, sb.ToString(), lang, pdfLink); /*SendMail(email, subject, sb.ToString(), lang, pdfLink);*/
            return sent == true ? t.Tran("menu sent successfully", lang) : t.Tran("menu is not sent", lang);
        } catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string SendMessage(string sendTo, string messageSubject, string messageBody, string lang) {
        try {
            bool sent = SendMail(sendTo, messageSubject, messageBody, lang, null, true);
            return sent == true ? t.Tran("mail sent successfully", lang) : t.Tran("mail is not sent", lang);
        } catch (Exception e) { return (e.Message); }
    }
    #endregion WebMethods

    #region Methods
    public bool SendOrder(Orders.NewUser user, string lang) {
        bool sent = false;
        //*****************Send mail to me****************
        string messageSubject = "Nova narudžba";
        string messageBody = string.Format(
@"
<h3>Nova Narudžba:</h3>
<p>Ime i prezime: {0} {1},</p>
<p>Tvrtka: {2}</p>
<p>Ulica i broj: {3}</p>
<p>Poštanski broj: {4}</p>
<p>Grad: {5}</p>
<p>Država: {6}</p>
<p>OIB: {7}</p>
<p>Email: {8}</p>
<p>Verzija: {9} {10}</p>
<p>Licenca: {11} ({12})</p>"
        , user.firstName
        , user.lastName
        , user.companyName
        , user.address
        , user.postalCode
        , user.city
        , user.country
        , user.pin
        , user.email
        , user.application
        , user.version
        , user.licenceNumber
        , GetLicenceDuration(user.licence));

        bool sentToMe = SendMail(myEmail, messageSubject, messageBody, lang, null, true);
        //**************************************************

        //************ Send mail to customer****************
        messageSubject = (user.application == "Program Prehrane 5.0" ? user.application : t.Tran("nutrition program", lang).ToUpper()) + " - " + t.Tran("payment details", lang);
        messageBody = PaymentDetails(user, lang);
        bool sentToCustomer = SendMail(user.email, messageSubject, messageBody, lang, null, false);
        //**************************************************
        if(sentToMe == false || sentToCustomer == false) {
            sent = false;
        } else {
            sent = true;
        }
        return sent;
    }

    /*  //OLD
    public bool SendMail(string sendTo, string messageSubject, string messageBody, string lang) {
        try {
            string my_email = lang == "en" ? myEmail_en : myEmail;
            string my_password = lang == "en" ? myPassword_en : myPassword;
            MailMessage mailMessage = new MailMessage();
            SmtpClient Smtp_Server = new SmtpClient();
            Smtp_Server.UseDefaultCredentials = false;
            Smtp_Server.Credentials = new NetworkCredential(my_email, my_password);
            Smtp_Server.Port = myServerPort;
            Smtp_Server.EnableSsl = true;
            Smtp_Server.Host = myServerHost;
            mailMessage.To.Add(sendTo);
            mailMessage.From = new MailAddress(my_email);
            mailMessage.Subject = messageSubject;
            mailMessage.Body = messageBody;
            mailMessage.IsBodyHtml = true;
            Smtp_Server.Send(mailMessage);
            return true;
        } catch (Exception e) { return false; }
    }

    
    public bool SendMail(string sendTo, string messageSubject, string messageBody, string lang, string file){
        try {
            string my_email = lang == "en" ? myEmail_en : myEmail;
            string my_password = lang == "en" ? myPassword_en : myPassword;
            MailMessage mailMessage = new MailMessage();
            SmtpClient Smtp_Server = new SmtpClient();
            Smtp_Server.UseDefaultCredentials = false;
            Smtp_Server.Credentials = new NetworkCredential(my_email, my_password);
            Smtp_Server.Port = myServerPort;
            Smtp_Server.EnableSsl = true;
            Smtp_Server.Host = myServerHost;
            mailMessage.To.Add(sendTo);
            mailMessage.From = new MailAddress(my_email);
            mailMessage.Subject = messageSubject;
            mailMessage.Body = messageBody;
            mailMessage.IsBodyHtml = true;
            Attachment attachment = new Attachment(Server.MapPath(file.Replace("../","~/")));
            mailMessage.Attachments.Add(attachment);
            Smtp_Server.Send(mailMessage);
            return true;
        } catch (Exception e) { return false; }
    }
    */

    /**New Mail**/
    public bool SendMail(string sendTo, string subject, string body, string lang, string file, bool send_cc) {
        try {
            string footer = "";
            if (lang == "en") {
                myServerHost = myServerHost_en;
                myServerPort = myServerPort_en;
                myEmail = myEmail_en;
                myPassword = myPassword_en;
                footer = @"
<br>
<br>
<br>
<div><img alt=""nutriprog.com"" height=""40"" src=""https://www.nutriprog.com/assets/img/logo.svg"" style=""float:left"" width=""190"" /></div>
<br>
<br>
<br>
<div>IG PROG</div>
<div><a href=""mailto:nutrition.plan@yahoo.com?subject=Upit"">nutrition.plan@yahoo.com</a></div>
<div><a href = ""https://www.nutriprog.com"">www.nutriprog.com</a></div>";
            } else {
                footer = @"
<br>
<br>
<br>
<div>
    <img alt=""ProgramPrehrane.com"" height=""40"" src=""https://www.programprehrane.com/assets/img/logo.svg"" style=""float:left"" width=""190"" />
</div>
<br>
<br>
<br>
<div style=""color:gray"">
    IG PROG - obrt za računalno programiranje<br>
    Ludvetov breg 5, 51000 Rijeka, HR<br>
    <a href=""tel:+385 98 330 966"">+385 98 330 966</a><br>
    <a href=""mailto:info@programprehrane@com?subject=Upit"">info@programprehrane@com</a><br>
    <a href=""https://www.programprehrane.com"">www.programprehrane.com</a>
</div>";
            }
            //myServerHost = "mail.programprehrane.com";
            //myServerPort = 25;
            //myEmail = "info@programprehrane.com";
            //myPassword = "Ipp123456$";

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(myEmail);
            mail.To.Add(sendTo);
            if (send_cc) {
                mail.CC.Add(myEmail_cc);
            }
            mail.Subject =  subject;
            mail.Body = string.Format(@"
{0}

{1}", body, footer);
            mail.IsBodyHtml = true;
            if (!string.IsNullOrEmpty(file)) {
                Attachment attachment = new Attachment(Server.MapPath(file.Replace("../", "~/")));
                mail.Attachments.Add(attachment);
            }
            SmtpClient smtp = new SmtpClient(myServerHost, myServerPort);
            NetworkCredential Credentials = new NetworkCredential(myEmail, myPassword);
            smtp.Credentials = Credentials;
            smtp.Send(mail);
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    public bool SendMail_menu(string sendTo, string subject, string body, string lang, string file) {
        try {
            if (lang == "en") {
                myServerHost = myServerHost_en;
                myServerPort = myServerPort_en;
                myEmail = myEmail_en;
                myPassword = myPassword_en;
            } else {
                myEmail = "jelovnik@programprehrane.com";
                myPassword = "Jpp123456$";
            }
                
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(myEmail);
            mail.To.Add(sendTo);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            if(!string.IsNullOrEmpty(file)) {
                Attachment attachment = new Attachment(Server.MapPath(file.Replace("../", "~/")));
                mail.Attachments.Add(attachment);
            }
            SmtpClient smtp = new SmtpClient(myServerHost, myServerPort);
            NetworkCredential Credentials = new NetworkCredential(myEmail, myPassword);
            smtp.Credentials = Credentials;
            smtp.Send(mail);
            return true;
        } catch (Exception e) {
            return false;
        }
    }


    private string AppendMeal(Meals.NewMeal meal, List<Foods.NewFood> selectedFoods) {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine(string.Format(@"<h4 style=""text-transform: uppercase"">{0}</h4>", meal.title));

        if (!string.IsNullOrWhiteSpace(meal.description)){
            sb.AppendLine(string.Format(@"
                                    <p style=""font-style: italic"">{0}</p>"
                                    , meal.description));
        }

        sb.AppendLine("<ul>");
        foreach (Foods.NewFood x in selectedFoods) {
            if(x.meal.code == meal.code) {
                sb.AppendLine(string.Format(@"
                                   <li><b>{0}</b>, {1} {2}, {3} g</li>"
                                    , x.food
                                    , x.quantity
                                    , x.unit
                                    , x.mass));
            }
        }
        sb.AppendLine("</ul>");
        return sb.ToString();
    }

    private string GetLicenceDuration(string licence) {
        switch (licence) {
            case "0":
                return "trajna";
            case "1":
                return "godišnja";
            case "2":
                return "dvogodišnja";
            default:
                return "";
        }
    }

    private string PaymentDetails(Orders.NewUser user, string lang) {
        switch (lang){
            case "en":
                return
                    string.Format(
@"
<p>{0},</p>
<p>{1} <b>{2} {3}</b>.</p>
<p>{4}: <a href=""mailto:nutrition.plan@yahoo.com"">nutrition.plan@yahoo.com</a></p> 
<br />
<b>{5}:</b>
<hr/>
<p>IBAN: HR84 2340 0091 1603 4249 6</p>
<p>SWIFT CODE: PBZGHR2X</p>
<p>{6}: Privredna banka Zagreb d.d., Račkoga 6, 10000 Zagreb, {7}</p>
<p>{8}: IG PROG, vl. Igor Gasparovic</p>
<p>{9}: Ludvetov breg 5, 51000 Rijeka, {7}</p>
<p>{10}: {2} {3}</p>
<p>{11}: <b>{12} {13}</b></p>
<hr/>
<a href=""https://www.nutriprog.com/paypal.html""><img alt=""PayPal"" src=""https://www.nutriprog.com/assets/img/paypal.jpg""></a>
<hr/>
<br />
<br />
<p>{14}</p>
<br />
<div style=""color:gray"">
<p>IG PROG</p>
<p>Ludvetov breg 5, 51000 Rijeka, {7}</p>
<a href=""mailto:nutrition.plan@yahoo.com"">nutrition.plan@yahoo.com</a>
<br />
<a href=""https://www.nutriprog.com/"">www.nutriprog.com</a>
</div>"
, t.Tran("dear", lang)
, t.Tran("thank you for your interest in", lang)
, user.application
, user.version
, t.Tran("your account will be active within 24 hours of your payment receipt or after you send us a payment confirmation to email", lang)
, t.Tran("payment details", lang)
, t.Tran("bank", lang)
, t.Tran("croatia", lang)
, t.Tran("recipient", lang)
, "Address"
, t.Tran("payment description", lang)
, t.Tran("amount", lang)
, Math.Round(user.price / usd, 2)
, "$"
, t.Tran("best regards", lang));
            default:
                return
                    string.Format(
@"
<p>Poštovani/a,</p>
<p>Zahvaljujemo na Vašem interesu za <b>{0} {1}</b>.</p>
<p>{6}.</p> 
<br />
<b>Podaci za uplatu:</b>
<hr/>
<p>IBAN: HR84 2340 0091 1603 4249 6</p>
<p>Banka: Privredna banka Zagreb d.d., Račkoga 6, 10000 Zagreb, Hrvatska</p>
<p>Primatelj: IG PROG, vl. Igor Gašparović</p>
<p>Adresa: Ludvetov breg 5, 51000 Rijeka, Hrvatska</p>
<p>Opis plaćanja: {0} {1}</p>
<p>Iznos: <b>{2} kn</b></p>
<p>Model: {5}</p>
<p>{3}</p>
<hr/>
<br />
<b>Podaci za uplatu izvan Hrvatske:</b>
<hr/>
<p>IBAN: HR84 2340 0091 1603 4249 6</p>
<p>SWIFT CODE: PBZGHR2X</p>
<p>Iznos: <b>{4} €</b></p>
<a href=""https://www.programprehrane.com/paypal.html""><img alt=""PayPal"" src=""https://www.programprehrane.com/assets/img/paypal.jpg""></a>
<hr/>
<br />
<p>Srdačan pozdrav</p>
<br />
<div style=""color:gray"">
<p>IG PROG - obrt za računalno programiranje</p>
<p>Ludvetov breg 5, 51000 Rijeka, HR</p>
<p>+385 98 330 966</p>
<a href=""mailto:info@programprehrane.com"">info@programprehrane.com</a>
<br />
<a href=""https://www.programprehrane.com"">www.programprehrane.com</a>
</div>"
, user.application
, user.version
, user.price
, string.IsNullOrWhiteSpace(user.pin) ? "" : string.Format("Poziv na broj: {0}", user.pin)
, Math.Round(user.priceEur, 2)
, string.IsNullOrWhiteSpace(user.pin) ? "HR99" : "HR00"
, user.application == "Program Prehrane 5.0" ? "Nakon primitka Vaše uplate ili nakon što nam pošaljete potvrdu o uplati, aktivacijski kod šaljemo na Vašu E-mail adresu" : "Aplikacija će biti aktivna nakon primitka Vaše uplate ili nakon što nam pošaljete potvrdu o uplati");
        }
    }
    #endregion methods


}
