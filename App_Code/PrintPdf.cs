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
    string logoPPPath = HttpContext.Current.Server.MapPath(string.Format("~/app/assets/img/logo.png"));
    string logoPathIgProg = HttpContext.Current.Server.MapPath(string.Format("~/assets/img/logo_igprog.png"));

    iTextSharp.text.pdf.draw.LineSeparator line = new iTextSharp.text.pdf.draw.LineSeparator(0f, 100f, Color.BLACK, Element.ALIGN_LEFT, 1);

    int weeklyMealIdx = 0;
    public List<Foods.Totals> weeklyMenuTotalList = new List<Foods.Totals>();
    public Foods.Totals weeklyMenuTotal = new Foods.Totals();

    public PrintPdf() {
    }

    public class PrintMenuSettings {
        public string pageSize;
        public bool showQty;
        public bool showMass;
        public bool showServ;
        public bool showTitle;
        public bool showDescription;
        public string orientation;
		public bool showClientData;
        public bool showFoods;
        public bool showTotals;
        // TODO activities, prices
    }

    #region WebMethods
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
            doc.Add(new Paragraph(currentMenu.title, GetFont(12)));
            doc.Add(new Paragraph(currentMenu.note, GetFont(8)));
            if(consumers > 1) {
                doc.Add(new Paragraph(t.Tran("number of consumers", lang) + ": " + consumers, GetFont(8)));
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

            doc.Add(new Paragraph(sb.ToString(), GetFont()));

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
                doc.Add(new Paragraph(tot, GetFont()));
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
            doc.Add(new Paragraph(weeklyMenu.title, GetFont(12)));
            doc.Add(new Paragraph(weeklyMenu.note, GetFont(8)));

            if (consumers > 1) {
                doc.Add(new Paragraph(t.Tran("number of consumers", lang) + ": " + consumers, GetFont(10)));
            } else {
                //TODO show sclient data when there are more than 1 consumens
                //ShowClientData(doc, currentMenu, clientData, settings.showClientData, lang);
            }
            doc.Add(new Chunk(line));

            PdfPTable table = new PdfPTable(8);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 1.5f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("meals", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("monday", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("tuesday", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("wednesday", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("thursday", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("friday", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("saturday", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("sunday", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            //****************** Totals *****************
            weeklyMenuTotalList = new List<Foods.Totals>();
            //*******************************************

            if (weeklyMenu.menuList.Count > 0) {
                weeklyMealIdx = 0;
                foreach (var ml in weeklyMenu.menuList) {
                    AppendDayMeal(table, weeklyMenu.menuList, consumers, userId, settings, lang);
                }
            }

            doc.Add(table);

            //************* Totals *************
            if (settings.showTotals) {
                table = new PdfPTable(8);
                table.WidthPercentage = 100f;
                table.SetWidths(new float[] { 1.5f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0}:", t.Tran("energy value", lang)), GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 7 });
                int ii = 0;
                for (int i = 0; i < 7; i++) {
                    if (!string.IsNullOrEmpty(weeklyMenu.menuList[i]) && ii < weeklyMenuTotalList.Count) {
                        table.AddCell(new PdfPCell(new Phrase(string.Format("{0} {1}", weeklyMenuTotalList[ii].energy.ToString(), t.Tran("kcal", lang)), GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 7 });
                        ii++;
                    } else {
                        table.AddCell(new PdfPCell(new Phrase("", GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                    }
                }
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0}:", t.Tran("carbohydrates", lang)), GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                ii = 0;
                for (int i = 0; i < 7; i++) {
                    if (!string.IsNullOrEmpty(weeklyMenu.menuList[i]) && ii < weeklyMenuTotalList.Count) {
                        table.AddCell(new PdfPCell(new Phrase(string.Format("{0} {1}, ({2}%)", weeklyMenuTotalList[ii].carbohydrates.ToString(), t.Tran("g", lang), weeklyMenuTotalList[ii].carbohydratesPercentage.ToString()), GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                        ii++;
                    } else {
                        table.AddCell(new PdfPCell(new Phrase("", GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                    }
                }
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0}:", t.Tran("proteins", lang)), GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                ii = 0;
                for (int i = 0; i < 7; i++) {
                    if (!string.IsNullOrEmpty(weeklyMenu.menuList[i]) && ii < weeklyMenuTotalList.Count) {
                        table.AddCell(new PdfPCell(new Phrase(string.Format("{0} {1}, ({2}%)", weeklyMenuTotalList[ii].proteins.ToString(), t.Tran("g", lang), weeklyMenuTotalList[ii].proteinsPercentage.ToString()), GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                        ii++;
                    } else {
                        table.AddCell(new PdfPCell(new Phrase("", GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                    }
                }
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0}:", t.Tran("fats", lang)), GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                ii = 0;
                for (int i = 0; i < 7; i++) {
                    if (!string.IsNullOrEmpty(weeklyMenu.menuList[i]) && ii < weeklyMenuTotalList.Count) {
                        table.AddCell(new PdfPCell(new Phrase(string.Format("{0} {1}, ({2}%)", weeklyMenuTotalList[ii].fats.ToString(), t.Tran("g", lang), weeklyMenuTotalList[ii].fatsPercentage.ToString()), GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                        ii++;
                    } else {
                        table.AddCell(new PdfPCell(new Phrase("", GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 2 });
                    }
                }
                doc.Add(table);
            }
            //*******************************************

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
            doc.Add(new Paragraph(t.Tran("manu analysis", lang).ToUpper(), GetFont(10)));

            string menu = string.Format(@"
{0}: {1}
{2}: {3}
{4}: {5}"
            , t.Tran("title", lang), currentMenu.title
            , t.Tran("note", lang), currentMenu.note
            , t.Tran("diet", lang), t.Tran(currentMenu.diet, lang) );

            doc.Add(new Paragraph(menu, GetFont()));
            doc.Add(new Chunk(line));
            doc.Add(Chunk.NEWLINE);
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph(t.Tran("energy value", lang).ToUpper(), GetFont(10)));
            PdfPTable tblMeals = new PdfPTable(3);
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("meals", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            int i = 0;
            foreach(var m in currentMenu.data.meals) {
                AppendMealDistribution(tblMeals, totals, recommendations, lang, i, m.title);
                i++;
            }

            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran("total", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.energy.ToString() + " " + t.Tran("kcal", lang), GetFont(CheckEnergy(totals.energy, recommendations.energy)))) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblMeals.AddCell(new PdfPCell(new Phrase(recommendations.energy.ToString() + " " + t.Tran("kcal", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            doc.Add(tblMeals);

            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph(t.Tran("macronutrients", lang).ToUpper(), GetFont(10)));
            PdfPTable tblTotal = new PdfPTable(3);
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("carbohydrates", lang), GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(totals.carbohydrates.ToString() + " " + t.Tran("g", lang) + ", (" + totals.carbohydratesPercentage.ToString() + " %)" , GetFont(CheckTotal(totals.carbohydratesPercentage, recommendations.carbohydratesPercentageMin, recommendations.carbohydratesPercentageMax)))) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(recommendations.carbohydratesPercentageMin.ToString() + "-" + recommendations.carbohydratesPercentageMax + " %", GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("proteins", lang), GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(totals.proteins.ToString() + " " + t.Tran("g", lang) + ", (" + totals.proteinsPercentage.ToString() + " %)", GetFont(CheckTotal(totals.proteinsPercentage, recommendations.proteinsPercentageMin, recommendations.proteinsPercentageMax)))) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(recommendations.proteinsPercentageMin.ToString() + "-" + recommendations.proteinsPercentageMax + " %", GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(t.Tran("fats", lang), GetFont())) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(totals.fats.ToString() + " " + t.Tran("g", lang) + ", (" + totals.fatsPercentage.ToString() + " %)", GetFont(CheckTotal(totals.fatsPercentage, recommendations.fatsPercentageMin, recommendations.fatsPercentageMax)))) { Border = 0 });
            tblTotal.AddCell(new PdfPCell(new Phrase(recommendations.fatsPercentageMin.ToString() + "-" + recommendations.fatsPercentageMax + " %", GetFont())) { Border = 0 });
            doc.Add(tblTotal);
            doc.Add(Chunk.NEWLINE);

            doc.Add(new Paragraph(t.Tran("unit servings", lang).ToUpper(), GetFont(10)));
            PdfPTable tblServings = new PdfPTable(3);
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("food group", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("cereals and cereal products", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.cerealsServ.ToString(), GetFont(CheckServ(totals.servings.cerealsServ, recommendations.servings.cerealsServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.cerealsServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("vegetables", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.vegetablesServ.ToString(), GetFont(CheckServ(totals.servings.vegetablesServ, recommendations.servings.vegetablesServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.vegetablesServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("fruit", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.fruitServ.ToString(), GetFont(CheckServ(totals.servings.fruitServ, recommendations.servings.fruitServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.fruitServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("meat and substitutes", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.meatServ.ToString(), GetFont(CheckServ(totals.servings.meatServ, recommendations.servings.meatServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.meatServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("milk and dairy products", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.milkServ.ToString(), GetFont(CheckServ(totals.servings.milkServ, recommendations.servings.milkServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.milkServ.ToString(), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(t.Tran("fats", lang), GetFont())) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(totals.servings.fatsServ.ToString(), GetFont(CheckServ(totals.servings.fatsServ, recommendations.servings.fatsServ)))) { Border = 0 });
            tblServings.AddCell(new PdfPCell(new Phrase(recommendations.servings.fatsServ.ToString(), GetFont())) { Border = 0 });
            doc.Add(tblServings);

            PdfPTable tblOtherFoods = new PdfPTable(3);
            tblOtherFoods.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblOtherFoods.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblOtherFoods.AddCell(new PdfPCell(new Phrase(t.Tran("acceptable", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            tblOtherFoods.AddCell(new PdfPCell(new Phrase(t.Tran("other foods", lang), GetFont())) { Border = 0 });
            tblOtherFoods.AddCell(new PdfPCell(new Phrase(totals.servings.otherFoodsEnergy.ToString() + " " + t.Tran("kcal", lang), GetFont(CheckOtherFoods(totals.servings.otherFoodsEnergy, recommendations.servings.otherFoodsEnergy)))) { Border = 0 });
            tblOtherFoods.AddCell(new PdfPCell(new Phrase(recommendations.servings.otherFoodsEnergy.ToString() + " " + t.Tran("kcal", lang), GetFont())) { Border = 0 });
            doc.Add(tblOtherFoods);

            doc.Add(Chunk.NEWLINE);

            doc.NewPage();
            AppendHeader(doc, userId);
            doc.Add(new Paragraph(t.Tran("parameters", lang).ToUpper(), GetFont(10)));

            string note = string.Format(@"
{0}: {1}
{2}: {3}
{4}: {5}"
            ,"*mda", t.Tran("minimum dietary allowance", lang)
            ,"*ul", t.Tran("upper intake level", lang)
            ,"*rda", t.Tran("recommended dietary allowance", lang));
            doc.Add(new Paragraph(note, GetFont(9, Font.ITALIC)));


            PdfPTable tblParameters = new PdfPTable(6);
            tblParameters.SetWidths(new float[] { 3f, 1f, 1f, 1f, 1f, 1f });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("parameter", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("unit", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("choosen", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("*mda", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("*ui", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("*rda", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("starch", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.starch.ToString(), GetFont(CheckParam(totals.starch, recommendations.starch)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.starch.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.starch.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.starch.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("total sugar", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.totalSugar.ToString(), GetFont(CheckParam(totals.totalSugar, recommendations.totalSugar)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.totalSugar.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.totalSugar.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.totalSugar.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("glucose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.glucose.ToString(), GetFont(CheckParam(totals.glucose, recommendations.glucose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.glucose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.glucose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.glucose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("fructose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.fructose.ToString(), GetFont(CheckParam(totals.fructose, recommendations.fructose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fructose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fructose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fructose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("saccharose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.saccharose.ToString(), GetFont(CheckParam(totals.saccharose, recommendations.saccharose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saccharose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saccharose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saccharose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("maltose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.maltose.ToString(), GetFont(CheckParam(totals.maltose, recommendations.maltose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.maltose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.maltose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.maltose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("lactose", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.lactose.ToString(), GetFont(CheckParam(totals.lactose, recommendations.lactose)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.lactose.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.lactose.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.lactose.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("fibers", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.fibers.ToString(), GetFont(CheckParam(totals.fibers, recommendations.fibers)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fibers.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fibers.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.fibers.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("saturated fats", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.saturatedFats.ToString(), GetFont(CheckParam(totals.saturatedFats, recommendations.saturatedFats)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saturatedFats.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saturatedFats.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.saturatedFats.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("monounsaturated fats", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.monounsaturatedFats.ToString(), GetFont(CheckParam(totals.monounsaturatedFats, recommendations.monounsaturatedFats)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.monounsaturatedFats.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.monounsaturatedFats.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.monounsaturatedFats.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("polyunsaturated fats", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.polyunsaturatedFats.ToString(), GetFont(CheckParam(totals.polyunsaturatedFats, recommendations.polyunsaturatedFats)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.polyunsaturatedFats.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.polyunsaturatedFats.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.polyunsaturatedFats.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("trifluoroacetic acid", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("g", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.trifluoroaceticAcid.ToString(), GetFont(CheckParam(totals.trifluoroaceticAcid, recommendations.trifluoroaceticAcid)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.trifluoroaceticAcid.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.trifluoroaceticAcid.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.trifluoroaceticAcid.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("cholesterol", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("mg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.cholesterol.ToString(), GetFont(CheckParam(totals.cholesterol, recommendations.cholesterol)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.cholesterol.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.cholesterol.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.cholesterol.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("sodium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("mg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.sodium.ToString(), GetFont(CheckParam(totals.sodium, recommendations.sodium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.sodium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.sodium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.sodium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("potassium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("mg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.potassium.ToString(), GetFont(CheckParam(totals.potassium, recommendations.potassium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.potassium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.potassium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.potassium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("calcium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.calcium.ToString(), GetFont(CheckParam(totals.calcium, recommendations.calcium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.calcium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.calcium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.calcium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("magnesium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.magnesium.ToString(), GetFont(CheckParam(totals.magnesium, recommendations.magnesium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.magnesium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.magnesium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.magnesium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("phosphorus", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.phosphorus.ToString(), GetFont(CheckParam(totals.phosphorus, recommendations.phosphorus)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.phosphorus.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.phosphorus.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.phosphorus.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("iron", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.iron.ToString(), GetFont(CheckParam(totals.iron, recommendations.iron)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iron.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iron.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iron.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("copper", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.copper.ToString(), GetFont(CheckParam(totals.copper, recommendations.copper)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.copper.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.copper.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.copper.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("zinc", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.zinc.ToString(), GetFont(CheckParam(totals.zinc, recommendations.zinc)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.zinc.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.zinc.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.zinc.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("chlorine", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.chlorine.ToString(), GetFont(CheckParam(totals.chlorine, recommendations.chlorine)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.chlorine.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.chlorine.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.chlorine.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("manganese", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.manganese.ToString(), GetFont(CheckParam(totals.manganese, recommendations.manganese)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.manganese.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.manganese.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.manganese.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("selenium", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("μg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.selenium.ToString(), GetFont(CheckParam(totals.selenium, recommendations.selenium)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.selenium.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.selenium.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.selenium.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("iodine", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("μg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.iodine.ToString(), GetFont(CheckParam(totals.iodine, recommendations.iodine)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iodine.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iodine.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.iodine.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("retinol", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran(t.Tran("μg", lang), lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.retinol.ToString(), GetFont(CheckParam(totals.retinol, recommendations.retinol)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.retinol.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.retinol.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.retinol.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("carotene", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.carotene.ToString(), GetFont(CheckParam(totals.carotene, recommendations.carotene)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.carotene.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.carotene.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.carotene.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin D", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminD.ToString(), GetFont(CheckParam(totals.vitaminD, recommendations.vitaminD)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminD.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminD.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminD.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin E", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminE.ToString(), GetFont(CheckParam(totals.vitaminE, recommendations.vitaminE)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminE.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminE.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminE.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B1", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB1.ToString(), GetFont(CheckParam(totals.vitaminB1, recommendations.vitaminB1)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB1.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB1.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB1.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B2", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB2.ToString(), GetFont(CheckParam(totals.vitaminB2, recommendations.vitaminB2)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB2.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB2.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB2.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B3", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB3.ToString(), GetFont(CheckParam(totals.vitaminB3, recommendations.vitaminB3)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB3.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB3.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB3.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B6", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB6.ToString(), GetFont(CheckParam(totals.vitaminB6, recommendations.vitaminB6)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB6.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB6.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB6.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitamin B12", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminB12.ToString(), GetFont(CheckParam(totals.vitaminB12, recommendations.vitaminB12)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB12.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB12.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminB12.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("folate", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.folate.ToString(), GetFont(CheckParam(totals.folate, recommendations.folate)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.folate.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.folate.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.folate.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("pantothenic acid", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.pantothenicAcid.ToString(), GetFont(CheckParam(totals.pantothenicAcid, recommendations.pantothenicAcid)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.pantothenicAcid.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.pantothenicAcid.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.pantothenicAcid.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("biotin", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.biotin.ToString(), GetFont(CheckParam(totals.biotin, recommendations.biotin)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.biotin.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.biotin.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.biotin.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitaminC", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("mg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminC.ToString(), GetFont(CheckParam(totals.vitaminC, recommendations.vitaminC)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminC.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminC.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminC.rda.ToString(), GetFont())) { Border = 0 });

            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("vitaminK", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(t.Tran("μg", lang), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(totals.vitaminK.ToString(), GetFont(CheckParam(totals.vitaminK, recommendations.vitaminK)))) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminK.mda.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminK.ui.ToString(), GetFont())) { Border = 0 });
            tblParameters.AddCell(new PdfPCell(new Phrase(recommendations.vitaminK.rda.ToString(), GetFont())) { Border = 0 });

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
            doc.Add(new Paragraph((client.firstName + " " + client.lastName), GetFont(12)));
            doc.Add(new Paragraph(((!string.IsNullOrEmpty(client.email) ? t.Tran("email", lang) + ": " + client.email + "   " : "") + (!string.IsNullOrEmpty(client.phone) ? t.Tran("phone", lang) + ": " + client.phone : "")), GetFont(10)));
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

            doc.Add(new Paragraph(c, GetFont()));
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

            doc.Add(new Paragraph(string.Format("{0} {1}" , client.firstName, client.lastName), GetFont(8)));
            doc.Add(new Paragraph(string.Format("{0}: {1}", t.Tran("gender", lang), t.Tran(clientData.gender.title, lang)), GetFont(8)));
            doc.Add(new Paragraph(string.Format("{0}: {1}", t.Tran("age", lang), clientData.age), GetFont(8)));
            doc.Add(new Chunk(line));

            if (clientLog.Count > 0) {
                doc.Add(new Paragraph(t.Tran("tracking of anthropometric measures", lang), GetFont()));

                PdfPTable table = new PdfPTable(5);
                table.AddCell(new PdfPCell(new Phrase(t.Tran("date", lang), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran("height", lang) + " (cm)", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran("weight", lang) + " (cm)", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran("waist", lang) + " (cm)", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran("hip", lang) + " (cm)", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

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
                doc.Add(new Paragraph(t.Tran("chart", lang), GetFont()));

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
            doc.Add(new Paragraph((client.firstName + " " + client.lastName), GetFont(12)));
            doc.Add(new Chunk(line));
            doc.Add(new Paragraph(t.Tran("calculation", lang).ToUpper(), GetFont(12)));
            string c = string.Format(@"
{0} ({1}): {2} kcal
{3} ({4}): {5} kcal"
            , "BMR"
            , t.Tran("basal metabolic rate", lang)
            , calculation.bmr
            , "TEE"
            , t.Tran("total energy expenditure", lang)
            , calculation.tee);
            doc.Add(new Paragraph(c, GetFont()));

            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100f;
            table.SetWidths(new float[] { 2f, 1f, 1f, 2f });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("measured", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("recommended", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran("note", lang).ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            table.AddCell(new PdfPCell(new Phrase(t.Tran("weight", lang).ToUpper() + ":", GetFont())) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(clientData.weight.ToString() + " kg", GetFont())) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(calculation.recommendedWeight.min.ToString() + " - " + calculation.recommendedWeight.max.ToString() + " kg", GetFont())) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = 0 });

            table.AddCell(new PdfPCell(new Phrase(t.Tran("bmi", lang).ToUpper() + " (" + t.Tran("body mass index", lang) + "):", GetFont())) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(Math.Round(calculation.bmi.value, 1).ToString() + " kg/m2", GetFont())) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase("18.5 - 25 kg/m2", GetFont())) { Border = 0 });
            table.AddCell(new PdfPCell(new Phrase(t.Tran(calculation.bmi.title, lang), GetFont())) { Border = 0 });

            if(calculation.whr.value > 0 && !double.IsInfinity(calculation.whr.value)) {
                table.AddCell(new PdfPCell(new Phrase(t.Tran("whr", lang).ToUpper() + " (" + t.Tran("waist–hip ratio", lang) + "):", GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase(Math.Round(calculation.whr.value, 1).ToString(), GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase("< " + calculation.whr.increasedRisk.ToString(), GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran(calculation.whr.title, lang) + " (" + t.Tran(calculation.whr.description, lang) + ")", GetFont())) { Border = 0 });
            }
            
            if(calculation.waist.value > 0) {
                table.AddCell(new PdfPCell(new Phrase(t.Tran("waist", lang).ToUpper() + ":", GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase(calculation.waist.value.ToString() + " cm", GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase("< " + calculation.waist.increasedRisk.ToString() + " cm", GetFont())) { Border = 0 });
                table.AddCell(new PdfPCell(new Phrase(t.Tran(calculation.waist.title, lang) + " (" + t.Tran(calculation.waist.description, lang) + ")", GetFont())) { Border = 0 });
            }

            doc.Add(table);
            doc.Add(new Chunk(line));

            string g = string.Format(@"
{0}: {1}"
            , t.Tran("goal", lang).ToUpper()
            , t.Tran(calculation.goal.title, lang));
            doc.Add(new Paragraph(g, GetFont()));

            string r = string.Format(@"
{0}: {1} kcal
{2}: {3} kcal"
            , t.Tran("recommended energy intake", lang).ToUpper()
            , string.IsNullOrEmpty(myCalculation.recommendedEnergyIntake.ToString()) ? calculation.recommendedEnergyIntake : myCalculation.recommendedEnergyIntake
            , t.Tran("recommended additional energy expenditure", lang).ToUpper()
            , string.IsNullOrEmpty(myCalculation.recommendedEnergyExpenditure.ToString()) ? calculation.recommendedEnergyExpenditure : myCalculation.recommendedEnergyExpenditure);
            doc.Add(new Paragraph(r, GetFont(12)));
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
            GetFont(8, Font.ITALIC).SetColor(255, 122, 56);
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
            header_table.AddCell(new PdfPCell(new Phrase(info, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingBottom = 10, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
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
            client_paragrapf.Font = GetFont(10);
            client_paragrapf.Add(client);
            doc.Add(client_paragrapf);

            p = new Paragraph();
            p.Add(new Chunk("RAČUN R2", GetFont(12)));
            if (isForeign) { p.Add(new Chunk(" / INVOICE", GetFont(8, Font.ITALIC))); }
            doc.Add(p);

            p = new Paragraph();
            p.Add(new Chunk("Obračun prema naplaćenoj naknadi", GetFont(9, Font.ITALIC)));
            if (isForeign) { p.Add(new Chunk(" / calculation according to a paid compensation", GetFont(8, Font.ITALIC))); }
            doc.Add(p);

            p = new Paragraph();
            p.Add(new Chunk("Broj računa", GetFont()));
            if (isForeign) { p.Add(new Chunk(" / invoice number", GetFont(8, Font.ITALIC))); }
            p.Add(new Chunk(":", isForeign ? GetFont(8, Font.ITALIC) : GetFont(10)));
            p.Add(new Chunk(string.Format(" {0}/1/1", invoice.number), GetFont(10)));
            doc.Add(p);

            PdfPTable table = new PdfPTable(5);

            p = new Paragraph();
            p.Add(new Paragraph("Redni broj", GetFont()));
            if (isForeign) { p.Add(new Chunk("number", GetFont(8, Font.ITALIC))); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            p = new Paragraph();
            p.Add(new Paragraph("Naziv proizvoda / usluge", GetFont()));
            if (isForeign) { p.Add(new Chunk("description", GetFont(8, Font.ITALIC))); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, });

            p = new Paragraph();
            p.Add(new Paragraph("Količina", GetFont()));
            if (isForeign) { p.Add(new Chunk("quantity", GetFont(8, Font.ITALIC))); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            p = new Paragraph();
            p.Add(new Paragraph("Jedinična cijena", GetFont()));
            if (isForeign) { p.Add(new Chunk("unit price", GetFont(8, Font.ITALIC))); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            p = new Paragraph();
            p.Add(new Paragraph("Ukupno", GetFont()));
            if (isForeign) { p.Add(new Chunk("total", GetFont(8, Font.ITALIC))); }
            table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            int row = 0;
            double totPrice = 0;
            foreach (Invoice.Item item in invoice.items) {
                row++;
                totPrice = totPrice + (item.unitPrice * item.qty);
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0}.", row), GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.title, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5 });
                table.AddCell(new PdfPCell(new Phrase(item.qty.ToString(), GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0} kn", string.Format("{0:N}", item.unitPrice)), GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0} kn", string.Format("{0:N}", item.unitPrice * item.qty)), GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            }
            
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5 });
            table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2, PaddingTop = 5 });
            table.AddCell(new PdfPCell(new Phrase("Ukupan iznos računa: ", GetFont(10))) { Border = PdfPCell.TOP_BORDER, Padding = 2,  PaddingTop = 5, Colspan = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            table.AddCell(new PdfPCell(new Phrase(string.Format("{0} kn", string.Format("{0:N}", totPrice)), GetFont(10, Font.BOLD))) { Border = PdfPCell.TOP_BORDER, Padding = 2, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });

            if (isForeign) {
                table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2 });
                table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 2 });
                table.AddCell(new PdfPCell(new Phrase("Total: ", GetFont(8, Font.ITALIC))) { Border = PdfPCell.NO_BORDER, Padding = 2, Colspan = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(string.Format("{0} €", string.Format("{0:N}", totPrice_eur)), GetFont(10, Font.BOLD))) { Border = PdfPCell.NO_BORDER, Padding = 2, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            }


            table.WidthPercentage = 100f;
            float[] widths = new float[] { 1f, 3f, 1f, 1f, 1f };
            table.SetWidths(widths);
            doc.Add(table);

            p = new Paragraph();
            p.Add(new Chunk("PDV nije obračunat jer obveznik IG PROG nije u sustavu PDV - a po čl. 90, st. 1.Zakona o porezu na dodanu vrijednost.", GetFont(9, Font.ITALIC)));
            if (isForeign) { p.Add(new Chunk(" / VAT is not charged because taxpayer IG PROG is not registerd for VAT under Art 90, para 1 of the Law om VAT.", GetFont(8, Font.ITALIC))); }
            doc.Add(p);

            PdfPTable invoiceInfo_table = new PdfPTable(2);

            p = new Paragraph();
            p.Add(new Chunk("Datum i vrijeme", GetFont()));
            if (isForeign) { p.Add(new Chunk(" / date and time", GetFont(8, Font.ITALIC))); }
            p.Add(new Chunk(":", isForeign ? GetFont(8, Font.ITALIC) : GetFont(10)));
            invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 20 });
            invoiceInfo_table.AddCell(new PdfPCell(new Phrase(invoice.dateAndTime, GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 20, });

            p = new Paragraph();
            p.Add(new Chunk("Oznaka operatera", GetFont()));
            if (isForeign) { p.Add(new Chunk(" / operator", GetFont(8, Font.ITALIC))); }
            p.Add(new Chunk(":", isForeign ? GetFont(8, Font.ITALIC) : GetFont(10)));
            invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5 });
            invoiceInfo_table.AddCell(new PdfPCell(new Phrase("IG", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, });

            p = new Paragraph();
            p.Add(new Chunk("Način plaćanja", GetFont()));
            if (isForeign) { p.Add(new Chunk(" / payment method", GetFont(8, Font.ITALIC))); }
            p.Add(new Chunk(":", isForeign ? GetFont(8, Font.ITALIC) : GetFont(10)));
            invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5 });
            p = new Paragraph();
            p.Add(new Chunk("Transakcijski račun", GetFont()));
            if (isForeign) { p.Add(new Chunk(" / transaction occount", GetFont(8, Font.ITALIC))); }
            invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, });

            p = new Paragraph();
            p.Add(new Chunk("Mjesto isporuke", GetFont()));
            if (isForeign) { p.Add(new Chunk(" / place of issue", GetFont(8, Font.ITALIC))); }
            p.Add(new Chunk(":", isForeign ? GetFont(8, Font.ITALIC) : GetFont(10)));
            invoiceInfo_table.AddCell(new PdfPCell(p) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5 });
            invoiceInfo_table.AddCell(new PdfPCell(new Phrase("Rijeka", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, });

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
                title.Font = GetFont();
                title.Add(invoice.note);
                doc.Add(title);
                spacing = spacing - 40f;
            }
            if(isForeign) { spacing = spacing - 40f; }

            PdfPTable sign_table = new PdfPTable(2);
            sign_table.SpacingBefore = spacing;
            sign_table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            sign_table.AddCell(new PdfPCell(new Phrase("Odgovorna osoba:", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            sign_table.AddCell(new PdfPCell(new Phrase("", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_RIGHT });
            sign_table.AddCell(new PdfPCell(new Phrase("Igor Gašparović", GetFont())) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 5, HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            sign_table.WidthPercentage = 100f;
            float[] sign_widths = new float[] { 4f, 1f };
            sign_table.SetWidths(sign_widths);
            doc.Add(sign_table);

            PdfPTable footer_table = new PdfPTable(1);
            footer_table.AddCell(new PdfPCell(new Phrase("mob: +385 98 330 966   |   email: igprog@yahoo.com   |   web: www.igprog.hr", GetFont(8))) { Border = PdfPCell.NO_BORDER, Padding = 0, PaddingTop = 80, HorizontalAlignment = PdfPCell.ALIGN_CENTER });
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
                    sb = AppendMealDescription(sb, description, settings);
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
            tblMeals.AddCell(new PdfPCell(new Phrase(t.Tran(meal, lang), GetFont())) { Border = 0 });
            tblMeals.AddCell(new PdfPCell(new Phrase(totals.mealsTotalEnergy[i].meal.energy.ToString() + " " + t.Tran("kcal", lang) + " (" + Math.Round(Convert.ToDouble(totals.mealsTotalEnergy[i].meal.energyPercentage), 1).ToString() + " %)", GetFont(CheckTotal(totals.mealsTotalEnergy[i].meal.energyPercentage, recommendations.mealsRecommendationEnergy[i].meal.energyMinPercentage, recommendations.mealsRecommendationEnergy[i].meal.energyMaxPercentage)))) { Border = 0 });
            tblMeals.AddCell(new PdfPCell(new Phrase(recommendations.mealsRecommendationEnergy[i].meal.energyMin.ToString() + "-" + recommendations.mealsRecommendationEnergy[i].meal.energyMax.ToString() + " " + t.Tran("kcal", lang) + " (" + recommendations.mealsRecommendationEnergy[i].meal.energyMinPercentage.ToString() + "-" + recommendations.mealsRecommendationEnergy[i].meal.energyMaxPercentage.ToString() + " %)", GetFont())) { Border = 0 });
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
            Font font_qty = GetFont(9, Font.ITALIC);
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
                    if (wm.data.meals != null) {
                        if(wm.data.meals.Count > weeklyMealIdx) {
                            menuTitle = wm.data.meals[weeklyMealIdx].title;
                            break;
                        }
                    }
                }
            }

            table.AddCell(new PdfPCell(new Phrase(menuTitle.ToUpper(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, Padding = 2, MinimumHeight = 30, PaddingTop = 15 });

            for (i = 0; i < menuList.Count; i++) {
                Menues.NewMenu weeklyMenu = !string.IsNullOrEmpty(menuList[i]) ? me.WeeklyMenu(userId, menuList[i]): new Menues.NewMenu();
                string currMeal = !string.IsNullOrEmpty(menuList[i]) ? weeklyMenu.data.meals[weeklyMealIdx].code : "";
                p = new Phrase();
                if (!string.IsNullOrEmpty(weeklyMenu.id)) {
                    meal = weeklyMenu.data.selectedFoods.Where(a => a.meal.code == currMeal).ToList();
                    string description = weeklyMenu.data.meals.Find(a => a.code == currMeal).description;
                    List<Foods.NewFood> meal_ = food.MultipleConsumers(meal, consumers);
                    if (!string.IsNullOrWhiteSpace(description)) {
                        StringBuilder sb = new StringBuilder();
                        p.Add(new Chunk(AppendMealDescription(sb, description, settings).ToString(), GetFont(10)));
                        p.Add(new Chunk("\n\n", GetFont()));
                    }
                    if (settings.showFoods) {
                        foreach (Foods.NewFood f in meal_) {
                            p.Add(new Chunk(string.Format(@"- {0}", f.food), GetFont()));
                            p.Add(new Chunk(string.Format(@"{0}{1}{2}"
                                    , settings.showQty ? string.Format(", {0} {1}", f.quantity, f.unit) : ""
                                    , settings.showMass ? string.Format(", {0} g", f.mass) : ""
                                    , settings.showServ && !string.IsNullOrEmpty(getServingDescription(f.servings, lang)) ? string.Format(", ({0})", getServingDescription(f.servings, lang)) : ""), font_qty));
                            p.Add(new Chunk("\n", GetFont()));
                        }
                    }
                    //************ Totals ***************
                    weeklyMenuTotal = new Foods.Totals();
                    Foods foods = new Foods();
                    weeklyMenuTotal.energy = weeklyMenu.energy;
                    weeklyMenuTotal.carbohydrates = Math.Round(weeklyMenu.data.selectedFoods.Sum(a => a.carbohydrates), 1);
                    weeklyMenuTotal.carbohydratesPercentage = Math.Round(foods.GetNutrientPercentage(weeklyMenu.data.selectedFoods, weeklyMenuTotal.carbohydrates), 1);
                    weeklyMenuTotal.proteins = Math.Round(weeklyMenu.data.selectedFoods.Sum(a => a.proteins), 1);
                    weeklyMenuTotal.proteinsPercentage = Math.Round(foods.GetNutrientPercentage(weeklyMenu.data.selectedFoods, weeklyMenuTotal.proteins), 1);
                    weeklyMenuTotal.fats = Math.Round(weeklyMenu.data.selectedFoods.Sum(a => a.fats), 1);
                    weeklyMenuTotal.fatsPercentage = Math.Round(foods.GetNutrientPercentage(weeklyMenu.data.selectedFoods, weeklyMenuTotal.fats), 1);
                    weeklyMenuTotalList.Add(weeklyMenuTotal);
                    //************************************
                } else {
                    p.Add(new Chunk("", GetFont()));
                }
                table.AddCell(new PdfPCell(p) { Border = PdfPCell.BOTTOM_BORDER, MinimumHeight = 30, PaddingTop = 5, PaddingRight = 2, PaddingBottom = 5, PaddingLeft = 2 });
            }
            weeklyMealIdx += 1;
        } catch (Exception e) {}
    }

    private StringBuilder AppendMealDescription(StringBuilder sb, string description, PrintMenuSettings settings) {
         if (description.Contains('~')) {
            string[] desList = description.Split('|');
            if (desList.Length > 0) {
                var list = (from p_ in desList
                            select new {
                                title = p_.Split('~')[0],
                                description = p_.Split('~').Length > 1 ? p_.Split('~')[1] : ""
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
        return sb;
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
            table.AddCell(new PdfPCell(new Phrase(sb.ToString(), GetFont())) { Border = PdfPCell.BOTTOM_BORDER, MinimumHeight = 30, PaddingTop = 15, PaddingRight = 2, PaddingBottom = 5, PaddingLeft = 2 });
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
            , GetFont(8)));
            doc.Add(new Paragraph(string.Format("{0}, {1} {2} {3}"
            , string.Format("{0}: {1} cm", t.Tran("height", lang), client.clientData.height)
            , string.Format("{0}: {1} kg", t.Tran("weight", lang), client.clientData.weight)
            , client.clientData.waist > 0 ? string.Format(", {0}: {1} kg", t.Tran("waist", lang), client.clientData.waist) : ""
            , client.clientData.hip > 0 ? string.Format(", {0}: {1} kg", t.Tran("hip", lang), client.clientData.hip) : "")
            , GetFont(8)));
            doc.Add(new Paragraph(string.Format("{0}: {1}", t.Tran("diet", lang), t.Tran(client.clientData.diet.diet, lang)), GetFont(8)));
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

    private Font GetFont(int size, int style) {
        Font font = FontFactory.GetFont(HttpContext.Current.Server.MapPath("~/app/assets/fonts/ARIALUNI.TTF"), BaseFont.IDENTITY_H, false, size, style);
        return font;
    }

    private Font GetFont() {
        return GetFont(9, 0); // Normal font
    }

    private Font GetFont(int size) {
        return GetFont(size, 0);
    }

    private Font GetFont(bool x) {
        return GetFont(9, x == true ? Font.BOLD: Font.NORMAL);
    }

    private bool CheckTotal(double total, int min, int max) {
        return total < min || total > max ? true : false;
    }

    private bool CheckEnergy(double total, double r) {
        return Math.Abs((total / r) - 1) > 0.05 ? true : false;
    }

    private bool CheckServ(double total, double r) {
        return Math.Abs(total - r) > 1 ? true : false;
    }

    private bool CheckOtherFoods(double total, double r) {
        return total > r ? true : false;
    }

    private bool CheckParam(double total, Foods.ParameterRecommendation r) {
        return r.mda != null && total < r.mda || r.ui != null && total > r.ui ? true : false;
    }
    #endregion Methods

}
