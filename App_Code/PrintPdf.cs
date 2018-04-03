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
    Translate t = new Translate();

    Font courier = new Font(Font.COURIER, 9f);
    Font normalFont = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 9);
    Font normalFont_8 = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 8);
    Font normalFont_10 = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 10);
    Font normalFont_12 = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 12);
    Font normalFont_bold = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 9, Font.BOLD);
    Font normalFont_10_bold = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 10, Font.BOLD);
    Font normalFont_italic = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 9, Font.ITALIC);

    string logoPath = HttpContext.Current.Server.MapPath(string.Format("~/app/assets/img/logo1.png"));
    string logoPathIgProg = HttpContext.Current.Server.MapPath(string.Format("~/assets/img/logo_igprog.png"));

    iTextSharp.text.pdf.draw.LineSeparator line = new iTextSharp.text.pdf.draw.LineSeparator(0f, 100f, Color.BLACK, Element.ALIGN_LEFT, 1);

    public PrintPdf() {
    }

    #region Webmethods
    [WebMethod]
    public string MenuPdf(string userId, Menues.NewMenu currentMenu, ClientsData.NewClientData clientData, Foods.Totals totals, int consumers, string lang) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc);

            doc.Add(new Paragraph(currentMenu.title, normalFont_12));
            doc.Add(new Paragraph(currentMenu.note, normalFont_8));
            if(consumers > 1) {
                doc.Add(new Paragraph(t.Tran("number of consumers", lang) + ": " + consumers, normalFont_8));
            }

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

            sb.AppendLine(AppendMeal(meal1, currentMenu.data.meals, lang));
            sb.AppendLine(AppendMeal(meal2, currentMenu.data.meals, lang));
            sb.AppendLine(AppendMeal(meal3, currentMenu.data.meals, lang));
            sb.AppendLine(AppendMeal(meal4, currentMenu.data.meals, lang));
            sb.AppendLine(AppendMeal(meal5, currentMenu.data.meals, lang));
            sb.AppendLine(AppendMeal(meal6, currentMenu.data.meals, lang));

            doc.Add(new Paragraph(sb.ToString(), normalFont));

            string tot = string.Format(@"
{0}
{1}: {5} kcal
{2}: {6} g ({7})%
{3}: {8} g ({9})%
{4}: {10} g ({11})%",
                        t.Tran("total", lang).ToUpper() + (consumers > 1 ? " (" + t.Tran("per consumer", lang) + ")" : ""),
                        t.Tran("energy", lang),
                        t.Tran("carbohydrates", lang),
                        t.Tran("proteins", lang),
                        t.Tran("fats", lang),
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

    [WebMethod]
    public string MenuDetailsPdf(string userId, Menues.NewMenu currentMenu, Calculations.NewCalculation calculation, Foods.Totals totals, Foods.Recommendations recommendations, string lang) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc);

            doc.Add(new Paragraph(t.Tran("manu analysis", lang).ToUpper(), normalFont_10));

            string menu = string.Format(@"
{0}: {1}
{2}: {3}
{4}: {5}"
            , t.Tran("title", lang), currentMenu.title
            , t.Tran("note", lang), currentMenu.note
            , t.Tran("diet", lang), t.Tran(currentMenu.diet, lang) );

            doc.Add(new Paragraph(menu, normalFont));
            doc.Add(new Chunk(line));
            doc.Add(Chunk.NEWLINE);
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph(t.Tran("energy value", lang).ToUpper(), normalFont_10));
            PdfPTable tblMeals = new PdfPTable(3);
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("meals", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            AppendMealDistribution(tblMeals, totals, recommendations, lang, 0, "breakfast");
            AppendMealDistribution(tblMeals, totals, recommendations, lang, 1, "morning snack");
            AppendMealDistribution(tblMeals, totals, recommendations, lang, 2, "lunch");
            AppendMealDistribution(tblMeals, totals, recommendations, lang, 3, "afternoon snack");
            AppendMealDistribution(tblMeals, totals, recommendations, lang, 4, "dinner");
            AppendMealDistribution(tblMeals, totals, recommendations, lang, 5, "meal before sleep");

            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("total", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.energy.ToString() + " kcal", normalFont_bold)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblMeals.AddCell(new PdfPCell(new Phrase(recommendations.energy.ToString()  + " kcal", normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            doc.Add(tblMeals);
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph(t.Tran("macronutrients", lang).ToUpper(), normalFont_10));
            PdfPTable tblTotal = new PdfPTable(3);
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("carbohydrates", lang), normalFont)) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(totals.carbohydrates.ToString() + " g, (" + totals.carbohydratesPercentage.ToString() + " %)" , normalFont_bold)) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(recommendations.carbohydratesPercentageMin.ToString() + "-" + recommendations.carbohydratesPercentageMax + " %", normalFont)) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("proteins", lang), normalFont)) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(totals.proteins.ToString() + " g, (" + totals.proteinsPercentage.ToString() + " %)", normalFont_bold)) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(recommendations.proteinsPercentageMin.ToString() + "-" + recommendations.proteinsPercentageMax + " %", normalFont)) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("fats", lang), normalFont)) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(totals.fats.ToString() + " g, (" + totals.fatsPercentage.ToString() + " %)", normalFont_bold)) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(recommendations.fatsPercentageMin.ToString() + "-" + recommendations.fatsPercentageMax + " %", normalFont)) { Border = 0 });
            doc.Add(tblTotal);
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph(t.Tran("unit servings", lang).ToUpper(), normalFont_10));
            PdfPTable tblServings = new PdfPTable(3);
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("food group", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("servings", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("cereals", lang), normalFont)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.cerealsServ.ToString(), normalFont_bold)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.cerealsServ.ToString(), normalFont)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("vegetables", lang), normalFont)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.vegetablesServ.ToString(), normalFont_bold)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.vegetablesServ.ToString(), normalFont)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("fruit", lang), normalFont)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.fruitServ.ToString(), normalFont_bold)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.fruitServ.ToString(), normalFont)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("meat", lang), normalFont)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.meatServ.ToString(), normalFont_bold)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.meatServ.ToString(), normalFont)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("milk", lang), normalFont)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.milkServ.ToString(), normalFont_bold)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.milkServ.ToString(), normalFont)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("fats", lang), normalFont)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.fatsServ.ToString(), normalFont_bold)) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.fatsServ.ToString(), normalFont)) { Border = 0 });
            doc.Add(tblServings);
            doc.Add(Chunk.NEWLINE);



            doc.NewPage();
            AppendHeader(doc);
            doc.Add(new Paragraph(t.Tran("parameters", lang).ToUpper(), normalFont_10));

            string note = string.Format(@"
{0}: {1}
{2}: {3}
{4}: {5}"
            ,"*mda", t.Tran("minimum dietary allowance", lang)
            ,"*ul", t.Tran("upper intake level", lang)
            ,"*rda", t.Tran("recommended dietary allowance", lang));
            doc.Add(new Paragraph(note, normalFont_italic));


            PdfPTable tblParameters = new PdfPTable(6);
            tblParameters.SetWidths(new int[] { 3, 1, 1, 1, 1, 1 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("parameter", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("unit", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("*mda", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("*ui", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("*rda", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("starch", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("g", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.starch.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.starch.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.starch.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.starch.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("total sugar", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("g", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.totalSugar.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.totalSugar.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.totalSugar.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.totalSugar.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("glucose", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("g", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.glucose.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.glucose.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.glucose.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.glucose.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("fructose", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("g", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.fructose.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fructose.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fructose.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fructose.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("saccharose", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("g", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.saccharose.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saccharose.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saccharose.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saccharose.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("maltose", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("g", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.maltose.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.maltose.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.maltose.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.maltose.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("lactose", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("g", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.lactose.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.lactose.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.lactose.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.lactose.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("fibers", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("g", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.fibers.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fibers.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fibers.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fibers.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("saturated fats", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("g", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.saturatedFats.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saturatedFats.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saturatedFats.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saturatedFats.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("monounsaturated fats", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("g", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.monounsaturatedFats.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.monounsaturatedFats.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.monounsaturatedFats.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.monounsaturatedFats.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("polyunsaturated fats", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("g", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.polyunsaturatedFats.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.polyunsaturatedFats.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.polyunsaturatedFats.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.polyunsaturatedFats.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("trifluoroacetic acid", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("g", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.trifluoroaceticAcid.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.trifluoroaceticAcid.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.trifluoroaceticAcid.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.trifluoroaceticAcid.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("cholesterol", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.cholesterol.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.cholesterol.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.cholesterol.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.cholesterol.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("sodium", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.sodium.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.sodium.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.sodium.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.sodium.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("potassium", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.potassium.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.potassium.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.potassium.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.potassium.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("calcium", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.calcium.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.calcium.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.calcium.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.calcium.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("magnesium", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.magnesium.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.magnesium.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.magnesium.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.magnesium.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("phosphorus", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.phosphorus.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.phosphorus.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.phosphorus.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.phosphorus.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("iron", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.iron.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iron.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iron.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iron.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("copper", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.copper.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.copper.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.copper.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.copper.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("zinc", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.zinc.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.zinc.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.zinc.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.zinc.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("chlorine", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.chlorine.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.chlorine.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.chlorine.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.chlorine.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("manganese", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.manganese.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.manganese.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.manganese.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.manganese.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("selenium", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("μg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.selenium.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.selenium.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.selenium.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.selenium.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("iodine", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("μg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.iodine.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iodine.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iodine.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iodine.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("retinol", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("μg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.retinol.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.retinol.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.retinol.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.retinol.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("carotene", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("μg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.carotene.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.carotene.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.carotene.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.carotene.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin D", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("μg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminD.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminD.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminD.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminD.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin E", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminE.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminE.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminE.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminE.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B1", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB1.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB1.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB1.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB1.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B2", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB2.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB2.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB2.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB2.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B3", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB3.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB3.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB3.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB3.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B6", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB6.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB6.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB6.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB6.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B12", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("μg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB12.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB12.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB12.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB12.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("folate", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("μg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.folate.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.folate.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.folate.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.folate.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("pantothenicAcid", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.pantothenicAcid.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.pantothenicAcid.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.pantothenicAcid.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.pantothenicAcid.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("biotin", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("μg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.biotin.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.biotin.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.biotin.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.biotin.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitaminC", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("mg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminC.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminC.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminC.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminC.rda.ToString(), normalFont)) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitaminK", lang), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase("μg", normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminK.ToString(), normalFont_bold)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminK.mda.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminK.ui.ToString(), normalFont)) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminK.rda.ToString(), normalFont)) { Border = 0 });

            doc.Add(tblParameters);
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Chunk(line));
            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.StackTrace;
        }
    }

    [WebMethod]
    public string ClientPdf(string userId, Clients.NewClient client, ClientsData.NewClientData clientData, List<ClientsData.NewClientData> clientLog, string lang) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc);
            doc.Add(new Paragraph((client.firstName + " " + client.lastName), normalFont_12));
            doc.Add(new Paragraph(((!string.IsNullOrEmpty(client.email) ? t.Tran("email", lang) + ": " + client.email + "   " : "") + (!string.IsNullOrEmpty(client.phone) ? t.Tran("phone", lang) + ": " + client.phone : "")), normalFont_10));
            doc.Add(new Chunk(line));

            string c = string.Format(@"
{0}: {1}
{2}: {3}
{4}: {5} cm
{6}: {7} cm
{8}: {9} cm
{10}: {11} cm
{12}: {13} ({14})"
            , t.Tran("gender", lang), t.Tran(clientData.gender.title, lang)
            , t.Tran("age", lang), clientData.age
            , t.Tran("height", lang), clientData.height
            , t.Tran("weight", lang), clientData.weight
            , t.Tran("waist", lang), clientData.waist
            , t.Tran("hip", lang), clientData.hip
            , t.Tran("physical activity level", lang), t.Tran(clientData.pal.title, lang), t.Tran(clientData.pal.description, lang));

            doc.Add(new Paragraph(c, normalFont));

            doc.Add(new Chunk(line));

            if (clientLog.Count > 0) {
                doc.Add(new Paragraph(t.Tran("tracking of anthropometric measures", lang), normalFont));

                PdfPTable table = new PdfPTable(5);
                table.AddCell(new PdfPCell(new Phrase(t.Tran("date", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran("height", lang) + " (cm)", normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran("weight", lang) + " (cm)", normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran("waist", lang) + " (cm)", normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran("hip", lang) + " (cm)", normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

                StringBuilder sb = new StringBuilder();
                foreach (ClientsData.NewClientData cl in clientLog) {
                    PdfPCell cell1 = new PdfPCell(new Phrase(cl.date.ToString("dd.MM.yyyy"), courier));
                    cell1.Border = 0;
                    table.AddCell(cell1);
                    PdfPCell cell2 = new PdfPCell(new Phrase(cl.height.ToString(), courier));
                    cell2.Border = 0;
                    table.AddCell(cell2);
                    PdfPCell cell3 = new PdfPCell(new Phrase(cl.weight.ToString(), courier));
                    cell3.Border = 0;
                    table.AddCell(cell3);
                    PdfPCell cell4 = new PdfPCell(new Phrase(cl.waist.ToString(), courier));
                    cell4.Border = 0;
                    table.AddCell(cell4);
                    PdfPCell cell5 = new PdfPCell(new Phrase(cl.hip.ToString(), courier));
                    cell5.Border = 0;
                    table.AddCell(cell5);
                }
                doc.Add(table);
            }


            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.StackTrace;
        }
    }
    
    [WebMethod]
    public string InvoicePdf(Invoice.NewInvoice invoice) {
        try {
            var doc = new Document();
            string path = Server.MapPath("~/upload/invoice/temp/");
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            Image logo = Image.GetInstance(logoPathIgProg);
            logo.Alignment = Image.ALIGN_LEFT;
            logo.ScalePercent(8f);

            doc.Add(logo);
            string info = string.Format(@"
Ludvetov breg 5, HR-51000 Rijeka
OIB 58331314923; MB 97370371
IBAN HR8423400091160342496
");
            doc.Add(new Paragraph(info, normalFont));

            doc.Add(new Chunk(line));
           // doc.Add(new Paragraph("Naručitelj:", normalFont_10));

            string client = string.Format(@"


                                                                                                            {0}
                                                                                                            {1}
                                                                                                            {2} {3}
                                                                                                            {4}

                                                                                                            {5}


",
            !string.IsNullOrWhiteSpace(invoice.companyName) ? invoice.companyName : string.Format("{0} {1}", invoice.firstName, invoice.lastName),
            invoice.address,
            invoice.postalCode,
            invoice.city,
            invoice.country,
            !string.IsNullOrWhiteSpace(invoice.pin) ? string.Format("OIB: {0}", invoice.pin): "");

            doc.Add(new Paragraph(client, normalFont_10));

            doc.Add(new Paragraph("RAČUN R2", normalFont_12));
            doc.Add(new Paragraph("Obračun prema naplaćenoj naknadi", normalFont_italic));

            doc.Add(new Paragraph(string.Format("Broj računa: {0}/1/1", invoice.number) , normalFont));

            PdfPTable table = new PdfPTable(5);
            table.AddCell(new PdfPCell(new Phrase("Redni broj", normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Naziv proizvoda / usluge", normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15,  });
            table.AddCell(new PdfPCell(new Phrase("Količina", normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Jedinična cijena", normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("Ukupno", normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            int row = 0;
            double totPrice = 0;
            foreach (Invoice.Item item in invoice.items) {
                row++;
                totPrice = totPrice + (item.unitPrice * item.qty);
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0}.", row), normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.title, normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(item.qty.ToString(), normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0} kn", string.Format("{0:N}", item.unitPrice)), normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0} kn", string.Format("{0:N}", item.unitPrice * item.qty)), normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            }
            
            table.AddCell(new PdfPCell(new Phrase("", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 10 });
            table.AddCell(new PdfPCell(new Phrase("", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 10 });
            table.AddCell(new PdfPCell(new Phrase("Ukupan iznos računa: ", normalFont_10)) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 10, Colspan = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0} kn", string.Format("{0:N}", totPrice)), normalFont_10_bold)) { Border = PdfPCell.TOP_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            table.WidthPercentage = 100f;
            float[] widths = new float[] { 1f, 3f, 1f, 1f, 1f };
            table.SetWidths(widths);
            doc.Add(table);

            doc.Add(new Paragraph("PDV nije obračunat jer obveznik IG PROG nije u sustavu PDV-a po čl. 90, st. 1. Zakona o porezu na dodanu vrijednost.", normalFont_italic));

            PdfPTable invoiceInfo_table = new PdfPTable(2);
            invoiceInfo_table.AddCell(new PdfPCell(new Phrase("Datum i vrijeme:", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 20 });
            invoiceInfo_table.AddCell(new PdfPCell(new Phrase(invoice.dateAndTime, normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 20, });

            invoiceInfo_table.AddCell(new PdfPCell(new Phrase("Oznaka operatera:", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5 });
            invoiceInfo_table.AddCell(new PdfPCell(new Phrase("IG", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, });

            invoiceInfo_table.AddCell(new PdfPCell(new Phrase("Način plaćanja:", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5 });
            invoiceInfo_table.AddCell(new PdfPCell(new Phrase("Transakcijski račun", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, });

            invoiceInfo_table.AddCell(new PdfPCell(new Phrase("Mjesto isporuke:", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0,  PaddingTop = 5 });
            invoiceInfo_table.AddCell(new PdfPCell(new Phrase("Rijeka", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, });

            invoiceInfo_table.WidthPercentage = 100f;
            float[] invoiceInfo_widths = new float[] { 1f, 4f };
            invoiceInfo_table.SetWidths(invoiceInfo_widths);
            doc.Add(invoiceInfo_table);

            Single spacing = 140f;
            if (row == 1) { spacing = 160f; }
            if (row == 2) { spacing = 140f; }
            if (row == 3) { spacing = 100f; }
            if (row == 4) { spacing = 60f; }
            if (row == 5) { spacing = 20f; }
            if (row == 6) { spacing = 40f; }

            PdfPTable sign_table = new PdfPTable(2);
            sign_table.SpacingBefore = spacing;
            sign_table.AddCell(new PdfPCell(new Phrase("", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            sign_table.AddCell(new PdfPCell(new Phrase("Dokument izdao:", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            sign_table.AddCell(new PdfPCell(new Phrase("", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            sign_table.AddCell(new PdfPCell(new Phrase("Igor Gašparović", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });


            sign_table.WidthPercentage = 100f;
            float[] sign_widths = new float[] { 4f, 1f };
            sign_table.SetWidths(sign_widths);
            doc.Add(sign_table);

            PdfPTable footer_table = new PdfPTable(1);
            footer_table.AddCell(new PdfPCell(new Phrase("mob: +385 98 330 966   |   email: igprog@yahoo.com   |   web: www.igprog.hr", normalFont_8)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 80, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(footer_table);

            //  doc.Add(new Paragraph(doc.BottomMargin, "mob: +385 98 330 966   |   email: igprog@yahoo.com   |   web: www.igprog.hr", normalFont_8));

            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.StackTrace;
        }
    }
    #endregion WebMethods

    #region Methods
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

    private string AppendMeal(List<Foods.NewFood> meal, List<Meals.NewMeal> meals, string lang) {
        StringBuilder sb = new StringBuilder();
        if (meal.Count > 0) {
            sb.AppendLine(string.Format(@"{0}", t.Tran(GetMealTitle(meal[0].meal.code), lang)).ToUpper());
            string description = meals.Where(a => a.code == meal[0].meal.code).FirstOrDefault().description;
            if (!string.IsNullOrEmpty(description)) {
            sb.AppendLine(string.Format(@"{0}
                                            ", description));
        } 
        foreach (Foods.NewFood food in meal) {
            sb.AppendLine(string.Format(@"- {0}, {1} {2}, ({3} g)", food.food, food.quantity, food.unit, food.mass));
        }
        sb.AppendLine("________________________________________________________________________");
        }
        return sb.ToString();
    }

    private void AppendHeader(Document doc) {
        Image logo = Image.GetInstance(logoPath);
        logo.Alignment = Image.ALIGN_RIGHT;
        logo.ScalePercent(80f);
        doc.Add(logo);
    }

    private void AppendMealDistribution(PdfPTable tblMeals, Foods.Totals totals, Foods.Recommendations recommendations, string lang, int i, string meal) {
        if (totals.mealsTotalEnergy[i].meal.energy > 0) {
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran(meal, lang), normalFont)) { Border = 0 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.mealsTotalEnergy[i].meal.energy.ToString() + " kcal (" + Math.Round(Convert.ToDouble(totals.mealsTotalEnergy[i].meal.energyPercentage), 1).ToString() + " %)", normalFont_bold)) { Border = 0 });
            tblMeals.AddCell(new PdfPCell(new Phrase(recommendations.mealsRecommendationEnergy[i].meal.energyMin.ToString() + "-" + recommendations.mealsRecommendationEnergy[i].meal.energyMax.ToString() + " kcal (" + recommendations.mealsRecommendationEnergy[i].meal.energyMinPercentage.ToString() + "-" + recommendations.mealsRecommendationEnergy[i].meal.energyMaxPercentage.ToString() + " %)", normalFont)) { Border = 0 });
        }
    }

    private string GetMealTitle(string code) {
        switch (code) {
            case "B": return "breakfast";
            case "MS": return "morning snack";
            case "L": return "lunch";
            case "AS": return "afternoon snack";
            case "D": return "dinner";
            case "MBS": return "meal before sleep";
            default: return "";
        }
    }
    #endregion Methods

}
