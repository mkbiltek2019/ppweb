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
    string myEmail_en = ConfigurationManager.AppSettings["myEmail_en"];
    string myPassword_en = ConfigurationManager.AppSettings["myPassword_en"];
    int myServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["myServerPort"]);
    string myServerHost = ConfigurationManager.AppSettings["myServerHost"];
    double usd = Convert.ToDouble(ConfigurationManager.AppSettings["USD"]);
    Translate t = new Translate();

    public Mail() {
    }

    #region WebMethods
    [WebMethod]
    public string Send(string name, string email, string messageSubject, string message, string lang) {
      //  messageSubject = "Program Prehrane - podaci za uplatu";
       string messageBody = string.Format(
           @"
<hr>{0}</h3>
<p>{1}: {2}</p>
<p>{3}: {4}</p>
<p>{5}: {6}</p>", t.Tran("new inquiry", lang), t.Tran("name", lang), name, t.Tran("email", lang), email, t.Tran("message", lang), message);
//@"
//<hr>Novi upit</h3>
//<p>Ime: {0}</p>
//<p>Email: {1}</p>
//<p>Poruka: {2}</p>", name, email, message);
        try {
            SendMail(myEmail, messageSubject, messageBody, lang);
            return "ok";
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

            string subject = string.Format("{0}", !string.IsNullOrWhiteSpace(user.companyName) ? user.companyName : string.Format("{0} {1}", user.firstName, user.lastName));

            SendMail(email, subject, sb.ToString(), lang);
            return "menu sent successfully";
        } catch (Exception e) { return ("error: " + e); }
    }

    [WebMethod]
    public string SendMessage(string sendTo, string messageSubject, string messageBody, string lang) {
        try {
            SendMail(sendTo, messageSubject, messageBody, lang);
            return "mail sent successfully";
        } catch (Exception e) { return (e.Message); }
    }
    #endregion WebMethods

    #region Methods
    public void SendOrder(Orders.NewUser user, string lang) {

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

        SendMail(myEmail, messageSubject, messageBody, lang);
        //**************************************************

        //************ Send mail to customer****************
        messageSubject = t.Tran("nutrition plan", lang).ToUpper() + " - " + t.Tran("payment details", lang);
        messageBody = PaymentDetails(user, lang);
        SendMail(user.email, messageSubject, messageBody, lang);
        //**************************************************

    }

    public void SendMail(string sendTo, string messageSubject, string messageBody, string lang) {
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
        } catch (Exception e) {}
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
<a href=""https://www.nutrition-plan.com/paypal.html""><img alt=""PayPal"" src=""https://www.programprehrane.com/assets/img/paypal.jpg""></a>
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
<a href=""https://www.nutrition-plan.com/"">www.nutrition-plan.com</a>
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
<p>Aplikacija će biti aktivna nakon primitka uplate ili nakon što nam pošaljete potvrdu o uplati.</p> 
<br />
<b>Podaci za uplatu na žiro račun:</b>
<hr/>
<p>IBAN: HR84 2340 0091 1603 4249 6</p>
<p>Banka : Privredna banka Zagreb d.d., Račkoga 6, 10000 Zagreb, Hrvatska</p>
<p>Primatelj: IG PROG, vl. Igor Gašparović</p>
<p>Adresa: Ludvetov breg 5, 51000 Rijeka, Hrvatska</p>
<p>Opis plaćanja: {0} {1}</p>
<p>Iznos: <b>{2} kn</b></p>
<p>Model: {5}</p>
<p>{3}</p>
<hr/>
<br />
<b>Podaci za uplatu izvan hrvatske:</b>
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
<a href=""mailto:program.prehrane@yahoo.com"">program.prehrane@yahoo.com</a>
<br />
<a href=""http://www.programprehrane.com"">www.programprehrane.com</a>
</div>"
, user.application
, user.version
, user.price
, string.IsNullOrWhiteSpace(user.pin) ? "" : string.Format("Poziv na broj: {0}", user.pin)
, Math.Round(user.priceEur, 2)
, string.IsNullOrWhiteSpace(user.pin) ? "HR99" : "HR00");
        }
    }
    #endregion methods


}
