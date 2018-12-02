using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Newtonsoft.Json;
using System.Data.SQLite;
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
    Font font_qty = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 9, Font.ITALIC);
    Font normalFont_8_italic = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, 8, Font.ITALIC);

    string logoPPPath = HttpContext.Current.Server.MapPath(string.Format("~/app/assets/img/logo.png"));
    string logoPathIgProg = HttpContext.Current.Server.MapPath(string.Format("~/assets/img/logo_igprog.png"));

    iTextSharp.text.pdf.draw.LineSeparator line = new iTextSharp.text.pdf.draw.LineSeparator(0f, 100f, Color.BLACK, Element.ALIGN_LEFT, 1);

    int weeklyMealIdx = 0;

    public PrintPdf() {
    }

    public class PrintMenuSettings {
        public string pageSize;
        public bool showQty;
        public bool showMass;
        public bool showServ;
        public bool showTitle; //TODO separate somehow title from description (#....#) or new line
        public bool showDescription;
        public string orientation;
		public bool showClientData;
        public bool showFoods;
        public bool showTotals;
        // TODO activities, prices
    }

    #region Webmethods
    [WebMethod]
    public string InitMenuSettings() {
        PrintMenuSettings x = new PrintMenuSettings();
        x.pageSize = "A3";
        x.showQty = true;
        x.showMass = true;
        x.showServ = false;
        x.showTitle = true;
        x.showDescription = true;
        x.orientation = "L";
		x.showClientData = true;
        x.showFoods = true;
        x.showTotals = true;
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string MenuPdf(string userId, Menues.NewMenu currentMenu, Foods.Totals totals, int consumers, string lang, PrintMenuSettings settings) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();
            AppendHeader(doc, userId);
            if (settings.showClientData) {
                ShowClientData(doc, currentMenu.client, lang);
            }
            doc.Add(new Paragraph(currentMenu.title, normalFont_12));
            doc.Add(new Paragraph(currentMenu.note, normalFont_8));
            if(consumers > 1) {
                doc.Add(new Paragraph(t.Tran("number of consumers", lang) + ": " + consumers, normalFont_8));
            }

            doc.Add(new Chunk(line));

            var meals = currentMenu.data.selectedFoods.Select(a => a.meal.code).Distinct().ToList();
            List<string> orderedMeals = GetOrderedMeals(meals);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format(@"
                                        "));


            foreach (string m in orderedMeals) {
                List<Foods.NewFood> meal = currentMenu.data.selectedFoods.Where(a => a.meal.code == m).ToList();
                sb.AppendLine(AppendMeal(meal, currentMenu.data.meals, lang, settings));
            }

            doc.Add(new Paragraph(sb.ToString(), normalFont));

            if (settings.showTotals) {
                string tot = string.Format(@"
{0}
{1}: {5} kcal
{2}: {6} g ({7})%
{3}: {8} g ({9})%
{4}: {10} g ({11})%",
                        t.Tran("total", lang).ToUpper() + (consumers > 1 ? " (" + t.Tran("per consumer", lang) + ")" : ""),
                        t.Tran("energy value", lang),
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
            }
            doc.Add(new Chunk(line));
            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.Message;
        }
    }

    [WebMethod]
    public string WeeklyMenuPdf(string userId, WeeklyMenus.NewWeeklyMenus weeklyMenu, int consumers, string lang, PrintMenuSettings settings) {
        try {
            Rectangle ps = PageSize.A3;
            switch (settings.pageSize) {
                case "A4": ps = PageSize.A4; break;
                case "A3": ps = PageSize.A3; break;
                case "A2": ps = PageSize.A2; break;
                case "A1": ps = PageSize.A1; break;
                default: ps = PageSize.A3; break;
            }
            Document doc = new Document(settings.orientation == "L" ? ps.Rotate() : ps, 40, 40, 40, 40);
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc, userId);

            if (settings.showClientData) {
                ShowClientData(doc, weeklyMenu.client, lang);
            }
            doc.Add(new Paragraph(weeklyMenu.title, normalFont_12));
            doc.Add(new Paragraph(weeklyMenu.note, normalFont_8));

            if (consumers > 1) {
                doc.Add(new Paragraph(t.Tran("number of consumers", lang) + ": " + consumers, normalFont_10));
            } else {
                //TODO show sclient data when there are more than 1 consumens
                //ShowClientData(doc, currentMenu, clientData, settings.showClientData, lang);
            }
            doc.Add(new Chunk(line));

            PdfPTable table = new PdfPTable(8);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1.5f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("meals", lang).ToUpper(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("monday", lang).ToUpper(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("tuesday", lang).ToUpper(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("wednesday", lang).ToUpper(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("thursday", lang).ToUpper(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("friday", lang).ToUpper(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("saturday", lang).ToUpper(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("sunday", lang).ToUpper(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            if (weeklyMenu.menuList.Count > 0) {
                weeklyMealIdx = 0;
                foreach (var ml in weeklyMenu.menuList) {
                    AppendDayMeal(table, weeklyMenu.menuList, consumers, userId, settings, lang);
                }
            }
            
            doc.Add(table);
            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.Message;
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
            AppendHeader(doc, userId);
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

            int i = 0;
            foreach(var m in currentMenu.data.meals) {
                AppendMealDistribution(tblMeals, totals, recommendations, lang, i, m.title);
                i++;
            }

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
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
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
            AppendHeader(doc, userId);
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
            tblParameters.SetWidths(new float[] { 3f, 1f, 1f, 1f, 1f, 1f });
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

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("pantothenic acid", lang), normalFont)) { Border = 0 });
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
            return "";
        }
    }

    [WebMethod]
    public string ClientPdf(string userId, Clients.NewClient client, ClientsData.NewClientData clientData, string lang) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc, userId);
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
            doc.Close();

            return fileName;
        } catch(Exception e) {
            return "";
        }
    }

    [WebMethod]
    public string ClientLogPdf(string userId, Clients.NewClient client, ClientsData.NewClientData clientData, List<ClientsData.NewClientData> clientLog, string lang, string imageData) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc, userId);
            doc.Add(new Paragraph((client.firstName + " " + client.lastName), normalFont_12));
            doc.Add(new Chunk(line));

            string c = string.Format(@"
{0}: {1}
{2}: {3}"
            , t.Tran("gender", lang), t.Tran(clientData.gender.title, lang)
            , t.Tran("age", lang), clientData.age);

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
                doc.Add(new Chunk(line));
                doc.Add(new Paragraph(t.Tran("chart", lang), normalFont));

                if (!string.IsNullOrEmpty(imageData)) {
                    string imgPath = UploadImg(userId, imageData);
                    Image clientChart = Image.GetInstance(Server.MapPath(string.Format("~{0}", imgPath)));
                    clientChart.Alignment = Image.ALIGN_CENTER;
                    float width = 52f;
                    clientChart.ScalePercent(width);
                    doc.Add(clientChart);
                }
            }

            doc.Close();

            return fileName;
        } catch(Exception e) {
            return "";
        }
    }

    [WebMethod]
    public string CalculationPdf(string userId, Clients.NewClient client, ClientsData.NewClientData clientData, Calculations.NewCalculation calculation, Calculations.NewCalculation myCalculation, string lang) {
        try {
            var doc = new Document();
            string path = Server.MapPath(string.Format("~/upload/users/{0}/pdf/", userId));
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            AppendHeader(doc, userId);
            doc.Add(new Paragraph((client.firstName + " " + client.lastName), normalFont_12));
            doc.Add(new Chunk(line));
            doc.Add(new Paragraph(t.Tran("calculation", lang).ToUpper(), normalFont_12));
            string c = string.Format(@"
{0} ({1}): {2} kcal
{3} ({4}): {5} kcal"
            , "BMR"
            , t.Tran("basal metabolic rate", lang)
            , calculation.bmr
            , "TEE"
            , t.Tran("total energy expenditure", lang)
            , calculation.tee);
            doc.Add(new Paragraph(c, normalFont));

            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 2f, 1f, 1f, 2f });
            table.AddCell(new PdfPCell(new Phrase("", normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("measured", lang).ToUpper(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang).ToUpper(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("note", lang).ToUpper(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            table.AddCell(new PdfPCell(new Phrase(t.Tran("weight", lang).ToUpper() + ":", normalFont)) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(clientData.weight.ToString() + " kg", normalFont)) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(calculation.recommendedWeight.min.ToString() + " - " + calculation.recommendedWeight.max.ToString() + " kg", normalFont)) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase("", normalFont)) { Border = 0 });

            table.AddCell(new PdfPCell(new Phrase(t.Tran("bmi", lang).ToUpper() + " (" + t.Tran("body mass index", lang) + "):", normalFont)) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(Math.Round(calculation.bmi.value, 1).ToString() + " kg/m2", normalFont)) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase("18.5 - 25 kg/m2", normalFont)) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran(calculation.bmi.title, lang), normalFont)) { Border = 0 });

            if(calculation.whr.value > 0 && !double.IsInfinity(calculation.whr.value)) {
                table.AddCell(new PdfPCell(new Phrase(t.Tran("whr", lang).ToUpper() + " (" + t.Tran("waist–hip ratio", lang) + "):", normalFont)) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase(Math.Round(calculation.whr.value, 1).ToString(), normalFont)) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase("< " + calculation.whr.increasedRisk.ToString(), normalFont)) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran(calculation.whr.title, lang) + " (" + t.Tran(calculation.whr.description, lang) + ")", normalFont)) { Border = 0 });
            }
            
            if(calculation.waist.value > 0) {
                table.AddCell(new PdfPCell(new Phrase(t.Tran("waist", lang).ToUpper() + ":", normalFont)) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase(calculation.waist.value.ToString() + " cm", normalFont)) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase("< " + calculation.waist.increasedRisk.ToString() + " cm", normalFont)) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran(calculation.waist.title, lang) + " (" + t.Tran(calculation.waist.description, lang) + ")", normalFont)) { Border = 0 });
            }

            doc.Add(table);
            doc.Add(new Chunk(line));

            string g = string.Format(@"
{0}: {1}"
            , t.Tran("goal", lang).ToUpper()
            , t.Tran(calculation.goal.title, lang));
            doc.Add(new Paragraph(g, normalFont));

            string r = string.Format(@"
{0}: {1} kcal
{2}: {3} kcal"
            , t.Tran("recommended energy intake", lang).ToUpper()
            , string.IsNullOrEmpty(myCalculation.recommendedEnergyIntake.ToString()) ? calculation.recommendedEnergyIntake : myCalculation.recommendedEnergyIntake
            , t.Tran("recommended additional energy expenditure", lang).ToUpper()
            , string.IsNullOrEmpty(myCalculation.recommendedEnergyExpenditure.ToString()) ? calculation.recommendedEnergyExpenditure : myCalculation.recommendedEnergyExpenditure);
            doc.Add(new Paragraph(r, normalFont_12));
            doc.Add(new Chunk(line));

            doc.Close();

            return fileName;
        } catch(Exception e) {
            return "";
        }
    }

    [WebMethod]
    public string InvoicePdf(Invoice.NewInvoice invoice, bool isForeign, double totPrice_eur, int clientLeftSpacing) {
        try {
            normalFont_8_italic.SetColor(255, 122, 56);
            Paragraph p = new Paragraph();
            var doc = new Document();
            string path = Server.MapPath("~/upload/invoice/temp/");
            DeleteFolder(path);
            CreateFolder(path);
            string fileName = Guid.NewGuid().ToString();
            string filePath = Path.Combine(path, string.Format("{0}.pdf", fileName));
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

            doc.Open();

            Image logo = Image.GetInstance(logoPathIgProg);
            logo.ScalePercent(9f);
            string info = string.Format(@"
Ludvetov breg 5, HR-51000 Rijeka
OIB 58331314923; MB 97370371
IBAN HR8423400091160342496
");

            PdfPTable header_table = new PdfPTable(2);
            header_table.AddCell(new PdfPCell(logo) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingBottom = 10, VerticalAlignment = PdfCell.ALIGN_BOTTOM });
            header_table.AddCell(new PdfPCell(new Phrase(info, normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingBottom = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            header_table.WidthPercentage = 100f;
            float[] header_widths = new float[] { 2f, 1f };
            header_table.SetWidths(header_widths);
            doc.Add(header_table);

            doc.Add(new Chunk(line));
 
            string client = string.Format(@"
{0}
{1}
{2} {3}
{4}

{5}",
          !string.IsNullOrWhiteSpace(invoice.companyName) ? invoice.companyName : string.Format("{0} {1}", invoice.firstName, invoice.lastName),
            invoice.address,
            invoice.postalCode,
            invoice.city,
            invoice.country,
            !string.IsNullOrWhiteSpace(invoice.pin) ? string.Format("OIB{0}: {1}", isForeign ? string.Format(" / {0}", t.Tran("pin", "en").ToUpper()) : "", invoice.pin) : "");

            Paragraph client_paragrapf = new Paragraph();
            float clientLeftSpacing_float = Convert.ToSingle(clientLeftSpacing);
            client_paragrapf.SpacingBefore = 20f;
            client_paragrapf.SpacingAfter = 20f;
            client_paragrapf.IndentationLeft = clientLeftSpacing_float;
            client_paragrapf.Font = normalFont_10;
            client_paragrapf.Add(client);
            doc.Add(client_paragrapf);

            p = new Paragraph();
            p.Add(new Chunk("RAČUN R2", normalFont_12));
            if (isForeign) { p.Add(new Chunk(" / INVOICE", normalFont_8_italic)); }
            doc.Add(p);

            p = new Paragraph();
            p.Add(new Chunk("Obračun prema naplaćenoj naknadi", normalFont_italic));
            if (isForeign) { p.Add(new Chunk(" / calculation according to a paid compensation", normalFont_8_italic)); }
            doc.Add(p);

            p = new Paragraph();
            p.Add(new Chunk("Broj računa", normalFont));
            if (isForeign) { p.Add(new Chunk(" / invoice number", normalFont_8_italic)); }
            p.Add(new Chunk(":", isForeign ? normalFont_8_italic : normalFont_10));
            p.Add(new Chunk(string.Format(" {0}/1/1", invoice.number), normalFont_10));
            doc.Add(p);

            PdfPTable table = new PdfPTable(5);

            p = new Paragraph();
            p.Add(new Paragraph("Redni broj", normalFont));
            if (isForeign) { p.Add(new Chunk("number", normalFont_8_italic)); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            p = new Paragraph();
            p.Add(new Paragraph("Naziv proizvoda / usluge", normalFont));
            if (isForeign) { p.Add(new Chunk("description", normalFont_8_italic)); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, });

            p = new Paragraph();
            p.Add(new Paragraph("Količina", normalFont));
            if (isForeign) { p.Add(new Chunk("quantity", normalFont_8_italic)); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            p = new Paragraph();
            p.Add(new Paragraph("Jedinična cijena", normalFont));
            if (isForeign) { p.Add(new Chunk("unit price", normalFont_8_italic)); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            p = new Paragraph();
            p.Add(new Paragraph("Ukupno", normalFont));
            if (isForeign) { p.Add(new Chunk("total", normalFont_8_italic)); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            int row = 0;
            double totPrice = 0;
            foreach (Invoice.Item item in invoice.items) {
                row++;
                totPrice = totPrice + (item.unitPrice * item.qty);
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0}.", row), normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.title, normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5 });
                table.AddCell(new PdfPCell(new Phrase(item.qty.ToString(), normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0} kn", string.Format("{0:N}", item.unitPrice)), normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0} kn", string.Format("{0:N}", item.unitPrice * item.qty)), normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            }
            
            table.AddCell(new PdfPCell(new Phrase("", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5 });
            table.AddCell(new PdfPCell(new Phrase("", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5 });
            table.AddCell(new PdfPCell(new Phrase("Ukupan iznos računa: ", normalFont_10)) { Border = PdfPCell.TOP_BORDER, Padding = 2,  PaddingTop = 5, Colspan = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0} kn", string.Format("{0:N}", totPrice)), normalFont_10_bold)) { Border = PdfPCell.TOP_BORDER, Padding = 2, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            if (isForeign) {
                table.AddCell(new PdfPCell(new Phrase("", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2 });
                table.AddCell(new PdfPCell(new Phrase("", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 2 });
                table.AddCell(new PdfPCell(new Phrase("Total: ", normalFont_8_italic)) { Border = PdfPCell.NO_BORDER, Padding = 2, Colspan = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0} €", string.Format("{0:N}", totPrice_eur)), normalFont_10_bold)) { Border = PdfPCell.NO_BORDER, Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            }


            table.WidthPercentage = 100f;
            float[] widths = new float[] { 1f, 3f, 1f, 1f, 1f };
            table.SetWidths(widths);
            doc.Add(table);

            p = new Paragraph();
            p.Add(new Chunk("PDV nije obračunat jer obveznik IG PROG nije u sustavu PDV - a po čl. 90, st. 1.Zakona o porezu na dodanu vrijednost.", normalFont_italic));
            if (isForeign) { p.Add(new Chunk(" / VAT is not charged because taxpayer IG PROG is not registerd for VAT under Art 90, para 1 of the Law om VAT.", normalFont_8_italic)); }
            doc.Add(p);

            PdfPTable invoiceInfo_table = new PdfPTable(2);

            p = new Paragraph();
            p.Add(new Chunk("Datum i vrijeme", normalFont));
            if (isForeign) { p.Add(new Chunk(" / date and time", normalFont_8_italic)); }
            p.Add(new Chunk(":", isForeign ? normalFont_8_italic : normalFont_10));
            invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 20 });
            invoiceInfo_table.AddCell(new PdfPCell(new Phrase(invoice.dateAndTime, normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 20, });

            p = new Paragraph();
            p.Add(new Chunk("Oznaka operatera", normalFont));
            if (isForeign) { p.Add(new Chunk(" / operator", normalFont_8_italic)); }
            p.Add(new Chunk(":", isForeign ? normalFont_8_italic : normalFont_10));
            invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5 });
            invoiceInfo_table.AddCell(new PdfPCell(new Phrase("IG", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, });

            p = new Paragraph();
            p.Add(new Chunk("Način plaćanja", normalFont));
            if (isForeign) { p.Add(new Chunk(" / payment method", normalFont_8_italic)); }
            p.Add(new Chunk(":", isForeign ? normalFont_8_italic : normalFont_10));
            invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5 });
            p = new Paragraph();
            p.Add(new Chunk("Transakcijski račun", normalFont));
            if (isForeign) { p.Add(new Chunk(" / transaction occount", normalFont_8_italic)); }
            invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, });

            p = new Paragraph();
            p.Add(new Chunk("Mjesto isporuke", normalFont));
            if (isForeign) { p.Add(new Chunk(" / place of issue", normalFont_8_italic)); }
            p.Add(new Chunk(":", isForeign ? normalFont_8_italic : normalFont_10));
            invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5 });
            invoiceInfo_table.AddCell(new PdfPCell(new Phrase("Rijeka", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, });

            invoiceInfo_table.WidthPercentage = 100f;
            float[] invoiceInfo_widths = new float[] { 1f, 4f };
            if(isForeign) { invoiceInfo_widths = new float[] { 2f, 4f }; }
            invoiceInfo_table.SetWidths(invoiceInfo_widths);
            doc.Add(invoiceInfo_table);

            float spacing = 140f;
            if (row == 1) { spacing = 160f; }
            if (row == 2) { spacing = 140f; }
            if (row == 3) { spacing = 100f; }
            if (row == 4) { spacing = 60f; }
            if (row == 5) { spacing = 20f; }

            if (!string.IsNullOrWhiteSpace(invoice.note)) {
                Paragraph title = new Paragraph();
                title.SpacingBefore = 20f;
                title.Font = normalFont;
                title.Add(invoice.note);
                doc.Add(title);
                spacing = spacing - 40f;
            }
            if(isForeign) { spacing = spacing - 40f; }

            PdfPTable sign_table = new PdfPTable(2);
            sign_table.SpacingBefore = spacing;
            sign_table.AddCell(new PdfPCell(new Phrase("", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            sign_table.AddCell(new PdfPCell(new Phrase("Odgovorna osoba:", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            sign_table.AddCell(new PdfPCell(new Phrase("", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            sign_table.AddCell(new PdfPCell(new Phrase("Igor Gašparović", normalFont)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            sign_table.WidthPercentage = 100f;
            float[] sign_widths = new float[] { 4f, 1f };
            sign_table.SetWidths(sign_widths);
            doc.Add(sign_table);

            PdfPTable footer_table = new PdfPTable(1);
            footer_table.AddCell(new PdfPCell(new Phrase("mob: +385 98 330 966   |   email: igprog@yahoo.com   |   web: www.igprog.hr", normalFont_8)) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 80, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            doc.Add(footer_table);

            doc.Close();

            return fileName;
        } catch(Exception e) {
            return e.Message;
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

    private string AppendMeal(List<Foods.NewFood> meal, List<Meals.NewMeal> meals, string lang, PrintMenuSettings settings) {
        StringBuilder sb = new StringBuilder();
        if (meal.Count > 0) {
            if(meals.Find(a => a.code == meal[0].meal.code).isSelected == true) {
                sb.AppendLine(string.Format(@"{0}", t.Tran(GetMealTitle(meal[0].meal), lang)).ToUpper());
                string description = meals.Where(a => a.code == meal[0].meal.code).FirstOrDefault().description;
                if (!string.IsNullOrWhiteSpace(description)) {
                    if (description.Contains('~')) {
                        string[] desList = description.Split('|');
                        if (desList.Length > 0) {
                            var list = (from p in desList
                                        select new {
                                            title = p.Split('~')[0],
                                            description = p.Split('~').Length > 1 ? p.Split('~')[1] : ""
                                        }).ToList();
                            foreach (var l in list) {
                                if (settings.showTitle) {
                                    sb.AppendLine(l.title);
                                }
                                if (settings.showDescription) {
                                    sb.AppendLine(string.Format(@"{0}
                                                    ", l.description));
                                }
                            }
                        }
                    } else {
                        if (settings.showDescription) {
                            sb.AppendLine(description);
                        }
                    }


                    //    string[] desList = description.Split('|');
                    //    if (desList.Length > 0) {
                    //        var list = (from p in desList
                    //                    select new {
                    //                        title = p.Split('~')[0],
                    //                        description = p.Split('~').Length > 1 ? p.Split('~')[1] : ""
                    //                    }).ToList();
                    //    foreach (var l in list) {
                    //        if (settings.showTitle) {
                    //            sb.AppendLine(l.title);
                    //        }
                    //        if (settings.showDescription) {
                    //            sb.AppendLine(string.Format(@"{0}
                    //                            ", l.description));
                    //        }
                    //    }
                    //}
                }
                if (settings.showFoods) {
                    foreach (Foods.NewFood food in meal) {
                        sb.AppendLine(string.Format(@"- {0}{1}{2}{3}"
                            , food.food
                            , string.Format(@"{0}", settings.showQty ? string.Format(@", {0} {1}", food.quantity, food.unit) : "")
                            , string.Format(@"{0}", settings.showMass ? string.Format(@", {0} g", food.mass) : "")
                            , string.Format(@"{0}", settings.showServ && !string.IsNullOrEmpty(getServingDescription(food.servings, lang)) ? string.Format(@", ({0})", getServingDescription(food.servings, lang)) : "")));
                    }
                    sb.AppendLine("________________________________________________________________________");
                }
            }
        }
        return sb.ToString();
    }

    private string getServingDescription(Foods.Servings x, string lang) {
        string des = "";
        if (x.cerealsServ > 0) { des = ServDesc(des, x.cerealsServ, "cereals_", lang); }
        if (x.vegetablesServ > 0) { des = ServDesc(des, x.vegetablesServ, "vegetables_", lang); }
        if (x.fruitServ > 0) { des = ServDesc(des, x.fruitServ, "fruit_", lang); }
        if (x.meatServ > 0) { des = ServDesc(des, x.meatServ, "meat_", lang); }
        if (x.milkServ > 0) { des = ServDesc(des, x.milkServ, "milk_", lang); }
        if (x.fatsServ > 0) { des = ServDesc(des, x.fatsServ, "fats_", lang); }
        return des;
    }

    private string ServDesc(string des, double serv, string title, string lang) {
        return string.Format("{0}{1} serv. {2}", (string.IsNullOrEmpty(des) ? "" : string.Format("{0}, ", des)), Math.Round(serv, 1), t.Tran(title, lang));
    }

    private void AppendHeader(Document doc, string userId) {
        string logoPath = null;
        string logoClientPath = Server.MapPath(string.Format("~/upload/users/{0}/logo.png", userId));
        logoPath = File.Exists(logoClientPath) ? logoClientPath : logoPPPath;
        if (File.Exists(logoPath)) {
            Image logo = Image.GetInstance(logoPath);
            logo.Alignment = Image.ALIGN_RIGHT;
            logo.ScalePercent(15f);
            logo.SpacingAfter = 15f;
            doc.Add(logo);
        }
    }

    private void AppendMealDistribution(PdfPTable tblMeals, Foods.Totals totals, Foods.Recommendations recommendations, string lang, int i, string meal) {
        if (totals.mealsTotalEnergy[i].meal.energy > 0) {
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran(meal, lang), normalFont)) { Border = 0 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.mealsTotalEnergy[i].meal.energy.ToString() + " kcal (" + Math.Round(Convert.ToDouble(totals.mealsTotalEnergy[i].meal.energyPercentage), 1).ToString() + " %)", normalFont_bold)) { Border = 0 });
            tblMeals.AddCell(new PdfPCell(new Phrase(recommendations.mealsRecommendationEnergy[i].meal.energyMin.ToString() + "-" + recommendations.mealsRecommendationEnergy[i].meal.energyMax.ToString() + " kcal (" + recommendations.mealsRecommendationEnergy[i].meal.energyMinPercentage.ToString() + "-" + recommendations.mealsRecommendationEnergy[i].meal.energyMaxPercentage.ToString() + " %)", normalFont)) { Border = 0 });
        }
    }

    private string GetMealTitle(Foods.CodeTitle meal) {
        switch (meal.code) {
            case "B": return "breakfast";
            case "MS": return "morning snack";
            case "L": return "lunch";
            case "AS": return "afternoon snack";
            case "D": return "dinner";
            case "MBS": return "meal before sleep";
            default: return meal.title;
        }
    }

    private void AppendDayMeal(PdfPTable table, List<string> menuList, int consumers, string userId, PrintMenuSettings settings, string lang) {
        try {
            font_qty.SetColor(8, 61, 134);
            List<Foods.NewFood> meal = new List<Foods.NewFood>();
            Phrase p = new Phrase();
            Foods food = new Foods();
            Menues me = new Menues();
            int i = 0;

            string menuTitle = "";
            foreach(string m in menuList) {
                if(m != null) {
                    Menues.NewMenu wm = me.WeeklyMenu(userId, m);
                    menuTitle = wm.data.meals[weeklyMealIdx].title;
                    break;
                }
            }
            table.AddCell(new PdfPCell(new Phrase(menuTitle.ToUpper(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            for (i = 0; i < menuList.Count; i++) {
                Menues.NewMenu weeklyMenu = !string.IsNullOrEmpty(menuList[i]) ? me.WeeklyMenu(userId, menuList[i]): new Menues.NewMenu();
                string currMeal = !string.IsNullOrEmpty(menuList[i]) ? weeklyMenu.data.meals[weeklyMealIdx].code : "";
                p = new Phrase();
                if (!string.IsNullOrEmpty(weeklyMenu.id)) {
                    meal = weeklyMenu.data.selectedFoods.Where(a => a.meal.code == currMeal).ToList();
                    string description = weeklyMenu.data.meals.Find(a => a.code == currMeal).description;
                    List<Foods.NewFood> meal_ = food.MultipleConsumers(meal, consumers);
                    if (!string.IsNullOrEmpty(description) && settings.showDescription == true) {
                        p.Add(new Chunk(description, normalFont_10));
                        p.Add(new Chunk("\n\n", normalFont));
                    }
                    if(settings.showFoods) {
                        foreach (Foods.NewFood f in meal_) {
                            p.Add(new Chunk(string.Format(@"- {0}", f.food), normalFont));
                            p.Add(new Chunk(string.Format(@"{0}{1}{2}"
                                    , settings.showQty ? string.Format(", {0} {1}", f.quantity, f.unit) : ""
                                    , settings.showMass ? string.Format(", {0} g", f.mass) : ""
                                    , settings.showServ && !string.IsNullOrEmpty(getServingDescription(f.servings, lang)) ? string.Format(", ({0})", getServingDescription(f.servings, lang)) : ""), font_qty));
                            p.Add(new Chunk("\n", normalFont));
                        }
                    }
                } else {
                    p.Add(new Chunk("", normalFont));
                }
                table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, MinimumHeight = 30, PaddingTop = 5, PaddingRight = 2, PaddingBottom = 5, PaddingLeft = 2 });
            }
            weeklyMealIdx += 1;
        } catch (Exception e) {}
    }

    private void AppendTotal(PdfPTable table, string[] menuList, string userId) {
        StringBuilder sb = new StringBuilder();
        Menues me = new Menues();
        int i = 0;
        for (i = 0; i < 7; i++) {
            Menues.NewMenu weeklyMenu = me.WeeklyMenu(userId, menuList[i]);
            sb = new StringBuilder();
            if (!string.IsNullOrEmpty(weeklyMenu.id)) {
                sb.AppendLine(string.Format("{0} kcal", weeklyMenu.energy));
            }
            table.AddCell(new PdfPCell(new Phrase(sb.ToString(), normalFont)) { Border = PdfPCell.BOTTOM_BORDER, MinimumHeight = 30, PaddingTop = 15, PaddingRight = 2, PaddingBottom = 5, PaddingLeft = 2 });
        }
    }

    private string UploadImg(string userId, string imageData) {
        string path = Server.MapPath(string.Format("~/upload/users/{0}/img/", userId));
        DeleteFolder(path);
        CreateFolder(path);
        string fileName = Guid.NewGuid().ToString();
        string filePath = Path.Combine(path, string.Format("{0}.png", fileName));
        using (FileStream fs = new FileStream(filePath, FileMode.Create)) {
            using (BinaryWriter bw = new BinaryWriter(fs)) {
                byte[] data = Convert.FromBase64String(imageData);
                bw.Write(data);
                bw.Close();
            }
        }
        return string.Format("/upload/users/{0}/img/{1}.png", userId, fileName);
    }
	
	private void ShowClientData(Document doc, Clients.NewClient client, string lang) {
            doc.Add(new Paragraph(string.Format("{0} {1}"
            , client.firstName
            , client.lastName)
            , normalFont_8));
            doc.Add(new Paragraph(string.Format("{0}, {1} {2} {3}"
            , string.Format("{0}: {1} cm", t.Tran("height", lang), client.clientData.height)
            , string.Format("{0}: {1} kg", t.Tran("weight", lang), client.clientData.weight)
            , client.clientData.waist > 0 ? string.Format(", {0}: {1} kg", t.Tran("waist", lang), client.clientData.waist) : ""
            , client.clientData.hip > 0 ? string.Format(", {0}: {1} kg", t.Tran("hip", lang), client.clientData.hip) : "")
            , normalFont_8));
            doc.Add(new Paragraph(string.Format("{0}: {1}", t.Tran("diet", lang), client.clientData.diet.diet), normalFont_8));
    }

    private List<string> GetOrderedMeals(List<string> meals) {
        List<string> x = new List<string>();
        if (meals.Count > 0) {
            if (meals[0].StartsWith("MM")) {
                // ********* My meals ***********
                x = meals.OrderByDescending(a => a == "MM0")
               .ThenByDescending(a => a == "MM1")
               .ThenByDescending(a => a == "MM2")
               .ThenByDescending(a => a == "MM3")
               .ThenByDescending(a => a == "MM4")
               .ThenByDescending(a => a == "MM5")
               .ThenByDescending(a => a == "MM6")
               .ThenByDescending(a => a == "MM7")
               .ToList();
            } else {
                // ******* Standard meals *******
                x = meals.OrderByDescending(a => a == "B")
                .ThenByDescending(a => a == "MS")
                .ThenByDescending(a => a == "L")
                .ThenByDescending(a => a == "AS")
                .ThenByDescending(a => a == "D")
                .ThenByDescending(a => a == "MBS")
                .ToList();
            }
        }
        return x;
    }
    #endregion Methods

}
