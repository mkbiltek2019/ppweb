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

    protected void CreateFolder(string path) {
        if (!Directory.Exists(Server.MapPath(path))) {
            Directory.CreateDirectory(Server.MapPath(path));
        }
    }

    [WebMethod]
    public string MenuPdf(string userId, string fileName, Menues.NewMenu currentMenu, ClientsData.NewClientData clientData, Foods.Totals totals) {
        var doc = new Document();
        //List<Menues.JsonFile> xx = new List<Menues.JsonFile>();
        //xx = JsonConvert.DeserializeObject<List<Menues.JsonFile>>(json);

        //  string path = "~/UsersFiles/" + foldername + "/pdf/";
        string path = "~/upload/users/" + userId + "/pdf/";
        CreateFolder(path);
        PdfWriter.GetInstance(doc, new FileStream(Server.MapPath(path + fileName + ".pdf"), FileMode.Create));

        doc.Open();

        Font arial = FontFactory.GetFont("Arial", 8, Color.BLACK);
        Font arial16 = FontFactory.GetFont("Arial", 16, Color.CYAN);
        Font courier = new Font(Font.COURIER, 9f);
        Font brown = new Font(Font.COURIER, 9f, Font.NORMAL, new Color(163, 21, 21));
        Font verdana = FontFactory.GetFont("Verdana", 16, Font.BOLDITALIC, new Color(255, 255, 255));

        //unicode font
        BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\fonts\ARIALUNI.TTF", BaseFont.IDENTITY_H, true);
        Font normalFont = new iTextSharp.text.Font(bf, 12, Font.NORMAL, Color.BLACK);

        doc.Add(new Paragraph("Jelovnik", normalFont));
        //PdfPTable table = new PdfPTable(5);

        //PdfPCell cell = new PdfPCell(new Phrase("Jelovnik", normalFont));
        //cell.Colspan = 5;
        //cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
        //cell.BackgroundColor = new Color(0, 179, 179);
        //cell.Border = 0;
        //table.AddCell(cell);

        //table.AddCell("Id");
        //table.AddCell("Namirnica");
        //table.AddCell("Količina");
        //table.AddCell("Mjera");
        //table.AddCell("Masa");
        //table.AddCell("Energija");

        //  foreach (Foods.NewFood x in currentMenu.data.selectedFoods) {

        List<Foods.NewFood> meal1 = currentMenu.data.selectedFoods.Where(a => a.meal.code == "B").ToList();
            List<Foods.NewFood> meal2 = currentMenu.data.selectedFoods.Where(a => a.meal.code == "MS").ToList();
            List<Foods.NewFood> meal3 = currentMenu.data.selectedFoods.Where(a => a.meal.code == "L").ToList();
            List<Foods.NewFood> meal4 = currentMenu.data.selectedFoods.Where(a => a.meal.code == "AS").ToList();
            List<Foods.NewFood> meal5 = currentMenu.data.selectedFoods.Where(a => a.meal.code == "D").ToList();
            List<Foods.NewFood> meal6 = currentMenu.data.selectedFoods.Where(a => a.meal.code == "MBS").ToList();

        //PdfPCell cell1 = new PdfPCell(new Phrase(meal1[0].meal.title.ToString(), normalFont));
        //cell.Colspan = 5;
        //cell1.Border = 0;
        //table.AddCell(cell1);

       

        //PdfPTable table = new PdfPTable(5);

        //PdfPCell cell = new PdfPCell(new Phrase("Jelovnik", normalFont));
        //cell.Colspan = 5;
        //cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
        //cell.BackgroundColor = new Color(0, 179, 179);
        //cell.Border = 0;
        //table.AddCell(cell);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine(string.Format(@"{0}
___________________________________", meal1[0].meal.title.ToString()));
        foreach (Foods.NewFood food1 in meal1) {
            sb.AppendLine(string.Format(@"{0} {1} {2}, ({3} g)", food1.food, food1.quantity, food1.unit, food1.mass));
                //PdfPCell cell2 = new PdfPCell(new Phrase(food1.food.ToString(), normalFont));
                //cell2.Border = 0;
                //table.AddCell(cell2);
                //PdfPCell cell3 = new PdfPCell(new Phrase(food1.quantity.ToString(), normalFont));
                //cell3.Border = 0;
                //table.AddCell(cell3);
                //PdfPCell cell4 = new PdfPCell(new Phrase(food1.unit.ToString(), normalFont));
                //cell4.Border = 0;
                //table.AddCell(cell4);
                //PdfPCell cell5 = new PdfPCell(new Phrase(food1.mass.ToString(), normalFont));
                //cell5.Border = 0;
                //table.AddCell(cell5);
                //PdfPCell cell6 = new PdfPCell(new Phrase(food1.energy.ToString(), normalFont));
                //cell6.Border = 0;
                //table.AddCell(cell6);
            }

        // doc.Add(table);
        //cell1 = new PdfPCell(new Phrase(meal2[0].meal.title.ToString(), normalFont));
        //cell.Colspan = 5;
        //cell1.Border = 0;
        //table.AddCell(cell1);
        //doc.Add(new Paragraph(meal2[0].meal.title.ToString(), normalFont));
        sb.AppendLine(string.Format(@"{0}
___________________________________", meal2[0].meal.title.ToString()));
        foreach (Foods.NewFood food2 in meal2) {
            sb.AppendLine(string.Format(@"{0} {1} {2}, ({3} g)", food2.food, food2.quantity, food2.unit, food2.mass));

            //PdfPCell cell2 = new PdfPCell(new Phrase(food2.food.ToString(), normalFont));
            //    cell2.Border = 0;
            //    table.AddCell(cell2);
            //    PdfPCell cell3 = new PdfPCell(new Phrase(food2.quantity.ToString(), normalFont));
            //    cell3.Border = 0;
            //    table.AddCell(cell3);
            //    PdfPCell cell4 = new PdfPCell(new Phrase(food2.unit.ToString(), normalFont));
            //    cell4.Border = 0;
            //    table.AddCell(cell4);
            //    PdfPCell cell5 = new PdfPCell(new Phrase(food2.mass.ToString(), normalFont));
            //    cell5.Border = 0;
            //    table.AddCell(cell5);
            //    PdfPCell cell6 = new PdfPCell(new Phrase(food2.energy.ToString(), normalFont));
            //    cell6.Border = 0;
            //    table.AddCell(cell6);
        }

        doc.Add(new Paragraph(sb.ToString(), normalFont));
        //PdfPCell cell1 = new PdfPCell(new Phrase(x.meal.title.ToString(), normalFont));
        //cell1.Border = 0;
        //table.AddCell(cell1);
        //PdfPCell cell2 = new PdfPCell(new Phrase(x.food.ToString(), normalFont));
        //cell2.Border = 0;
        //table.AddCell(cell2);
        //PdfPCell cell3 = new PdfPCell(new Phrase(x.quantity.ToString(), normalFont));
        //cell3.Border = 0;
        //table.AddCell(cell3);
        //PdfPCell cell4 = new PdfPCell(new Phrase(x.unit.ToString(), normalFont));
        //cell4.Border = 0;
        //table.AddCell(cell4);
        //PdfPCell cell5 = new PdfPCell(new Phrase(x.mass.ToString(), normalFont));
        //cell5.Border = 0;
        //table.AddCell(cell5);
        //PdfPCell cell6 = new PdfPCell(new Phrase(x.energy.ToString(), normalFont));
        //cell6.Border = 0;
        //table.AddCell(cell6);
        //  }

        //   doc.Add(table);

        string tot = string.Format(@"
        Total
        Energy: {0} kcal
        Carbohydrates: {1}g ({2})%
        Proteins: {3} g ({4})%
        Fats: {5} g ({6})%",
                    Convert.ToString(totals.energy),
                    Convert.ToString(totals.carbohydrates),
                    Convert.ToString(totals.carbohydratesPercentage),
                    Convert.ToString(totals.proteins),
                    Convert.ToString(totals.proteinsPercentage),
                    Convert.ToString(totals.fats),
                    Convert.ToString(totals.fatsPercentage)
                    );
        doc.Add(new Paragraph(tot, normalFont));
        doc.Close();   

        return "OK.";
    }



    //[WebMethod]
    //public string RealizationsPdf(string foldername, string filename, string json) {
    //    var doc = new Document();
    //    List<Realizations.NewRealization> xx = new List<Realizations.NewRealization>();
    //    xx = JsonConvert.DeserializeObject<List<Realizations.NewRealization>>(json);

    //    string path = "~/UsersFiles/" + foldername + "/pdf/";
    //    CreateFolder(path);
    //    PdfWriter.GetInstance(doc, new FileStream(Server.MapPath(path + filename + ".pdf"), FileMode.Create));

    //    doc.Open();

    //    Font arial = FontFactory.GetFont("Arial", 8, Color.BLACK);
    //    Font arial16 = FontFactory.GetFont("Arial", 16, Color.CYAN);
    //    Font courier = new Font(Font.COURIER, 9f);
    //    Font brown = new Font(Font.COURIER, 9f, Font.NORMAL, new Color(163, 21, 21));
    //    Font verdana = FontFactory.GetFont("Verdana", 16, Font.BOLDITALIC, new Color(255, 255, 255));

    //    PdfPTable table = new PdfPTable(6);

    //    PdfPCell cell = new PdfPCell(new Phrase("Realizacija", verdana));
    //    cell.Colspan = 6;
    //    cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
    //    cell.BackgroundColor = new Color(0, 179, 179);
    //    table.AddCell(cell);

    //    table.AddCell("Id");
    //    table.AddCell("GIR");
    //    table.AddCell("Škola");
    //    table.AddCell("Odjeljenje");
    //    table.AddCell("Trajanje");
    //    table.AddCell("Datum");
    //    foreach (var x in xx) {
    //        PdfPCell cell1 = new PdfPCell(new Phrase(x.id.ToString(), courier));
    //        cell1.Border = 0;
    //        table.AddCell(cell1);
    //        PdfPCell cell2 = new PdfPCell(new Phrase(x.type.ToString(), courier));
    //        cell2.Border = 0;
    //        table.AddCell(cell2);
    //        PdfPCell cell3 = new PdfPCell(new Phrase(x.school.ToString(), courier));
    //        cell3.Border = 0;
    //        table.AddCell(cell3);
    //        PdfPCell cell4 = new PdfPCell(new Phrase(x.schoolClass.ToString(), courier));
    //        cell4.Border = 0;
    //        table.AddCell(cell4);
    //        PdfPCell cell5 = new PdfPCell(new Phrase(x.duration.ToString(), courier));
    //        cell5.Border = 0;
    //        table.AddCell(cell5);
    //        PdfPCell cell6 = new PdfPCell(new Phrase(x.date.ToString(), courier));
    //        cell6.Border = 0;
    //        table.AddCell(cell6);
    //    }
    //    doc.Add(table);

    //    string text = @"Lorem ipsum dolor sit amet, civibus epicurei pericula cum te, cu eos audire denique. Ei electram voluptaria usu. Tale saperet te vim, sea meliore quaerendum scribentur ne, ad ridens corpora pro. Eam id purto cibo timeam, sale dissentias cu duo. Ex scaevola electram has, ei eius mazim nominati pri. Dolor expetendis est at. ";

    //    doc.Add(new Paragraph(text, brown));
    //    doc.Close();   

    //    return "OK.";
    //}

    //[WebMethod]
    //public string RealizationsPdf(string foldername, string filename, string json) {
    //    var doc = new Document();
    //    List<Realizations.NewRealization> xx = new List<Realizations.NewRealization>();
    //    xx = JsonConvert.DeserializeObject<List<Realizations.NewRealization>>(json);

    //    string path = "~/UsersFiles/" + foldername + "/pdf/";
    //    CreateFolder(path);
    //    PdfWriter.GetInstance(doc, new FileStream(Server.MapPath(path + filename + ".pdf"), FileMode.Create));

    //    doc.Open();

    //    Font arial = FontFactory.GetFont("Arial", 8, Color.BLACK);
    //    Font arial16 = FontFactory.GetFont("Arial", 16, Color.CYAN);
    //    Font courier = new Font(Font.COURIER, 9f);
    //    Font brown = new Font(Font.COURIER, 9f, Font.NORMAL, new Color(163, 21, 21));
    //    Font verdana = FontFactory.GetFont("Verdana", 16, Font.BOLDITALIC, new Color(255, 255, 255));

    //    PdfPTable table = new PdfPTable(6);

    //    PdfPCell cell = new PdfPCell(new Phrase("Realizacija", verdana));
    //    cell.Colspan = 6;
    //    cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
    //    cell.BackgroundColor = new Color(0, 179, 179);
    //    table.AddCell(cell);

    //    table.AddCell("Id");
    //    table.AddCell("GIR");
    //    table.AddCell("Škola");
    //    table.AddCell("Odjeljenje");
    //    table.AddCell("Trajanje");
    //    table.AddCell("Datum");
    //    foreach (var x in xx) {
    //        PdfPCell cell1 = new PdfPCell(new Phrase(x.id.ToString(), courier));
    //        cell1.Border = 0;
    //        table.AddCell(cell1);
    //        PdfPCell cell2 = new PdfPCell(new Phrase(x.type.ToString(), courier));
    //        cell2.Border = 0;
    //        table.AddCell(cell2);
    //        PdfPCell cell3 = new PdfPCell(new Phrase(x.school.ToString(), courier));
    //        cell3.Border = 0;
    //        table.AddCell(cell3);
    //        PdfPCell cell4 = new PdfPCell(new Phrase(x.schoolClass.ToString(), courier));
    //        cell4.Border = 0;
    //        table.AddCell(cell4);
    //        PdfPCell cell5 = new PdfPCell(new Phrase(x.duration.ToString(), courier));
    //        cell5.Border = 0;
    //        table.AddCell(cell5);
    //        PdfPCell cell6 = new PdfPCell(new Phrase(x.date.ToString(), courier));
    //        cell6.Border = 0;
    //        table.AddCell(cell6);
    //    }
    //    doc.Add(table);

    //    string text = @"Lorem ipsum dolor sit amet, civibus epicurei pericula cum te, cu eos audire denique. Ei electram voluptaria usu. Tale saperet te vim, sea meliore quaerendum scribentur ne, ad ridens corpora pro. Eam id purto cibo timeam, sale dissentias cu duo. Ex scaevola electram has, ei eius mazim nominati pri. Dolor expetendis est at. ";

    //    doc.Add(new Paragraph(text, brown));
    //    doc.Close();   

    //    return "OK.";
    //}

    //[WebMethod]
    //public string RealizationsPdf(string foldername, string filename, string json) {
    //    var doc = new Document();
    //    List<Realizations.NewRealization> xx = new List<Realizations.NewRealization>();
    //    xx = JsonConvert.DeserializeObject<List<Realizations.NewRealization>>(json);

    //    string path = "~/UsersFiles/" + foldername + "/pdf/";
    //    CreateFolder(path);
    //    PdfWriter.GetInstance(doc, new FileStream(Server.MapPath(path + filename + ".pdf"), FileMode.Create));

    //    doc.Open();

    //    Font arial = FontFactory.GetFont("Arial", 8, Color.BLACK);
    //    Font arial16 = FontFactory.GetFont("Arial", 16, Color.CYAN);
    //    Font courier = new Font(Font.COURIER, 9f);
    //    Font brown = new Font(Font.COURIER, 9f, Font.NORMAL, new Color(163, 21, 21));
    //    Font verdana = FontFactory.GetFont("Verdana", 16, Font.BOLDITALIC, new Color(255, 255, 255));

    //    PdfPTable table = new PdfPTable(6);

    //    PdfPCell cell = new PdfPCell(new Phrase("Realizacija", verdana));
    //    cell.Colspan = 6;
    //    cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
    //    cell.BackgroundColor = new Color(0, 179, 179);
    //    table.AddCell(cell);

    //    table.AddCell("Id");
    //    table.AddCell("GIR");
    //    table.AddCell("Škola");
    //    table.AddCell("Odjeljenje");
    //    table.AddCell("Trajanje");
    //    table.AddCell("Datum");
    //    foreach (var x in xx) {
    //        PdfPCell cell1 = new PdfPCell(new Phrase(x.id.ToString(), courier));
    //        cell1.Border = 0;
    //        table.AddCell(cell1);
    //        PdfPCell cell2 = new PdfPCell(new Phrase(x.type.ToString(), courier));
    //        cell2.Border = 0;
    //        table.AddCell(cell2);
    //        PdfPCell cell3 = new PdfPCell(new Phrase(x.school.ToString(), courier));
    //        cell3.Border = 0;
    //        table.AddCell(cell3);
    //        PdfPCell cell4 = new PdfPCell(new Phrase(x.schoolClass.ToString(), courier));
    //        cell4.Border = 0;
    //        table.AddCell(cell4);
    //        PdfPCell cell5 = new PdfPCell(new Phrase(x.duration.ToString(), courier));
    //        cell5.Border = 0;
    //        table.AddCell(cell5);
    //        PdfPCell cell6 = new PdfPCell(new Phrase(x.date.ToString(), courier));
    //        cell6.Border = 0;
    //        table.AddCell(cell6);
    //    }
    //    doc.Add(table);

    //    string text = @"Lorem ipsum dolor sit amet, civibus epicurei pericula cum te, cu eos audire denique. Ei electram voluptaria usu. Tale saperet te vim, sea meliore quaerendum scribentur ne, ad ridens corpora pro. Eam id purto cibo timeam, sale dissentias cu duo. Ex scaevola electram has, ei eius mazim nominati pri. Dolor expetendis est at. ";

    //    doc.Add(new Paragraph(text, brown));
    //    doc.Close();   

    //    return "OK.";
    //}


}
