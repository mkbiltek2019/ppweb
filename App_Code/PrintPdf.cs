using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Configuration;
using Igprog;

/// <summary>
/// PrintPdf
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class PrintPdf : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    DataBase db = new DataBase();

    public PrintPdf() {
    }

    [WebMethod]
    public string MenuPdf(string userId, string fileName, Menues.NewMenu currentMenu, ClientsData.NewClientData clientData, Foods.Totals totals, string lang) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            Font arial8 = FontFactory.GetFont("Arial", 8, Color.BLACK);
            Font arial12 = FontFactory.GetFont("Arial", 12, Color.BLACK);
            Font arial16 = FontFactory.GetFont("Arial", 16, Color.BLACK);
            Font courier = new Font(Font.COURIER, 9f);
            Font brown = new Font(Font.COURIER, 9f, Font.NORMAL, new Color(163, 21, 21));
            Font verdana = FontFactory.GetFont("Verdana", 16, Font.BOLDITALIC, new Color(255, 255, 255));
            Font arial8_itelic = FontFactory.GetFont("Arial", 8, Font.ITALIC, Color.BLACK);

            Font normalFont = FontFactory.GetFont(Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 9);

            string logoPath = Server.MapPath(string.Format("~/app/assets/img/logo1.png"));
            Image logo = Image.GetInstance(logoPath);
            logo.Alignment = Image.ALIGN_RIGHT;
            logo.ScalePercent(80f);
            doc.Add(logo);

            doc.Add(new Paragraph(currentMenu.title, arial12));
            doc.Add(new Paragraph(currentMenu.note, arial8_itelic));

            iTextSharp.text.pdf.draw.LineSeparator line = new iTextSharp.text.pdf.draw.LineSeparator(0f, 100f, Color.BLACK, Element.ALIGN_LEFT, 1);
            doc.Add(new Chunk(line));

            List<Foods.NewFood> meal1 = currentMenu.data.selectedFoods.Where(a => a.meal.code == "B").ToList();
            List<Foods.NewFood> meal2 = currentMenu.data.selectedFoods.Where(a => a.meal.code == "MS").ToList();
            List<Foods.NewFood> meal3 = currentMenu.data.selectedFoods.Where(a => a.meal.code == "L").ToList();
            List<Foods.NewFood> meal4 = currentMenu.data.selectedFoods.Where(a => a.meal.code == "AS").ToList();
            List<Foods.NewFood> meal5 = currentMenu.data.selectedFoods.Where(a => a.meal.code == "D").ToList();
            List<Foods.NewFood> meal6 = currentMenu.data.selectedFoods.Where(a => a.meal.code == "MBS").ToList();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format(@"
                                        "));

            sb.AppendLine(AppendMeal(meal1, currentMenu.data.meals));
            sb.AppendLine(AppendMeal(meal2, currentMenu.data.meals));
            sb.AppendLine(AppendMeal(meal3, currentMenu.data.meals));
            sb.AppendLine(AppendMeal(meal4, currentMenu.data.meals));
            sb.AppendLine(AppendMeal(meal5, currentMenu.data.meals));
            sb.AppendLine(AppendMeal(meal6, currentMenu.data.meals));

            doc.Add(new Paragraph(sb.ToString(), normalFont));

            string tot = string.Format(@"
{0}
{1}: {5} kcal
{2}: {6} g ({7})%
{3}: {8} g ({9})%
{4}: {10} g ({11})%",
                        Translate("total", lang).ToUpper(),
                        Translate("energy", lang),
                        Translate("carbohydrates", lang),
                        Translate("proteins", lang),
                        Translate("fats", lang),
                        Convert.ToString(totals.energy),
                        Convert.ToString(totals.carbohydrates),
                        Convert.ToString(totals.carbohydratesPercentage),
                        Convert.ToString(totals.proteins),
                        Convert.ToString(totals.proteinsPercentage),
                        Convert.ToString(totals.fats),
                        Convert.ToString(totals.fatsPercentage)
                        );
            doc.Add(new Paragraph(tot, normalFont));
            doc.Add(new Chunk(line));
            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.StackTrace;
        }
    }

    protected void CreateFolder(string path) {
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }

    protected void DeleteFolder(string path) {
        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
        }
    }

    private string AppendMeal(List<Foods.NewFood> meal, List<Meals.NewMeal> meals) {
        StringBuilder sb = new StringBuilder();
        if (meal.Count > 0) {
            sb.AppendLine(string.Format(@"{0}", meal[0].meal.title).ToUpper());
            string description = meals.Where(a => a.code == meal[0].meal.code).FirstOrDefault().description;
            if (!string.IsNullOrEmpty(description)) {
            sb.AppendLine(string.Format(@"{0}
                                            ", description));
        } 
        foreach (Foods.NewFood food in meal) {
            sb.AppendLine(string.Format(@"- {0} {1} {2}, ({3} g)", food.food, food.quantity, food.unit, food.mass));
        }
        sb.AppendLine("________________________________________________________________________");
        }
        return sb.ToString();
    }

    private string Translate(string title, string lang) {
        switch (lang) {
            case "hr":
                if (title.ToLower() == "menu") { return "jelovnik"; }
                if (title.ToLower() == "total") { return "ukupno"; }
                if (title.ToLower() == "energy") { return "energija"; }
                if (title.ToLower() == "carbohydrates") { return "ugljikohidrati"; }
                if (title.ToLower() == "proteins") { return "bjelančevine"; }
                if (title.ToLower() == "fats") { return "masti"; }
                break;
            case "sr":
                if (title.ToLower() == "menu") { return "jelovnik"; }
                if (title.ToLower() == "total") { return "ukupno"; }
                if (title.ToLower() == "energy") { return "energija"; }
                if (title.ToLower() == "carbohydrates") { return "ugljeni hidrati"; }
                if (title.ToLower() == "proteins") { return "belančevine"; }
                if (title.ToLower() == "fats") { return "masti"; }
                break;
            default:
                return title;
        }
        return title;
    }

}
