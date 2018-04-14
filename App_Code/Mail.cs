using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Text;

/// <summary>
/// SendMail
/// </summary>
[WebService(Namespace = "http://programprehrane.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Mail : System.Web.Services.WebService {
    string myEmail = ConfigurationManager.AppSettings["myEmail"]; // "program.prehrane@yahoo.com";
    string myPassword = ConfigurationManager.AppSettings["myPassword"]; // "Tel546360";
    int myServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["myServerPort"]); // 587;
    string myServerHost = ConfigurationManager.AppSettings["myServerHost"]; // "smtp.mail.yahoo.com";

    public Mail() {
    }

   
    public void SendOrder(Orders.NewUser user) {

        //-----------Send mail to me---------
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

        SendMail(myEmail, messageSubject, messageBody);
        // ---------------------------------------

        //------ Send mailo to customer-------
        messageSubject = "Program Prehrane - podaci za uplatu";
        messageBody = string.Format(
@"
<p>Poštovani/a,</p>
<p>Zahvaljujemo na Vašem interesu za {0} {1}.
<br />
<p>Aplikacija će biti aktivna nakon primitka uplate ili nakon što nam pošaljete potvrdu o uplati.</p> 
<br />
<b>Podaci za uplatu na žiro račun:</b>
<hr/>
<p>IBAN: HR84 2340 0091 1603 4249 6</p>
<p>Banka : Privredna banka Zagreb d.d. , Račkoga 6, 10000 Zagreb, Hrvatska</p>
<p>Primatelj: IG PROG, vl. Igor Gašparović</p>
<p>Adresa: Ludvetov breg 5, 51000 Rijeka</p>
<p>Opis plaćanja: {0} {1}</p>
<p>Iznos: {2} kn</p>
<p>Model: HR99</p>
<p>Poziv na broj: {3}</p>
<hr/>
<br />
<b>Podaci za uplatu izvan hrvatske:</b>
<hr/>
<p>IBAN: HR84 2340 0091 1603 4249 6</p>
<p>SWIFT CODE: PBZGHR2X</p>
<p>Iznos: {4} €</p>
<hr/>
<br />
<p>Lijep pozdrav</p>
<br />
<br />
<div style=""color:gray"">
<p>IG PROG - obrt za racunalno programiranje</p>
<p>Ludvetov breg 5, 51000 Rijeka, HR</p>
<p>+385 98 330 966</p>
<a href=""mailto:program.prehrane@yahoo.com"">program.prehrane@yahoo.com</a>
<br />
<a href=""http://www.programprehrane.com"">www.programprehrane.com</a>
</div>"
, user.application
, user.version
, user.price
, user.pin = String.IsNullOrWhiteSpace(user.pin) ? Convert.ToString(DateTime.Now.Ticks).Substring(10) : user.pin
, Math.Round(user.priceEur,0));

        SendMail(user.email, messageSubject, messageBody);
        //-----------------------------------------

    }

    public void SendMail(string sendTo, string messageSubject, string messageBody) {
        try {
            MailMessage mailMessage = new MailMessage();
            SmtpClient Smtp_Server = new SmtpClient();
            Smtp_Server.UseDefaultCredentials = false;
            Smtp_Server.Credentials = new NetworkCredential(myEmail, myPassword);
            Smtp_Server.Port = myServerPort;
            Smtp_Server.EnableSsl = true;
            Smtp_Server.Host = myServerHost;
            mailMessage.To.Add(sendTo);
            mailMessage.From = new MailAddress(myEmail);
            mailMessage.Subject = messageSubject;
            mailMessage.Body = messageBody;
            mailMessage.IsBodyHtml = true;
            Smtp_Server.Send(mailMessage);
        } catch (Exception e) {}
    }


    [WebMethod]
    public string Send(string name, string email, string messageSubject, string message) {
      //  messageSubject = "Program Prehrane - podaci za uplatu";
       string messageBody = string.Format(
@"
<hr>Novi upit</h3>
<p>Ime: {0}</p>
<p>Email: {1}</p>
<p>Poruka: {2}</p>", name, email, message);
        try {
            SendMail(myEmail, messageSubject, messageBody);
            return "ok";
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string SendMenu(string email, string messageSubject, Menues.NewMenu currentMenu) {
        try {
            StringBuilder sb = new StringBuilder();
            StringBuilder meal1 = new StringBuilder();
            StringBuilder meal2 = new StringBuilder();
            StringBuilder meal3 = new StringBuilder();
            StringBuilder meal4 = new StringBuilder();
            StringBuilder meal5 = new StringBuilder();
            StringBuilder meal6 = new StringBuilder();

            sb.AppendLine(string.Format(@"<h3>{0}</h3><hr />", messageSubject));

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

            SendMail(email, messageSubject, sb.ToString());
            return "menu sent successfully";
        } catch (Exception e) { return ("error: " + e); }
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


}
