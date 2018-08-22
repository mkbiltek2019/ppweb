using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Calculations
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Calculations : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["AppDataBase"];

    public Calculations() {
    }

    #region Classes
    public class NewCalculation {
        public ValueTitle bmi { get; set; }
        public ValueTitleDescription whr { get; set; }
        public WaistValue waist { get; set; }
        public double bmr { get; set; }
        public double tee { get; set; }
        public int recommendedEnergyIntake { get; set; }
        public int recommendedEnergyExpenditure { get; set; }
        public RecommenderWeight recommendedWeight { get; set; }

        public Goals.NewGoal goal = new Goals.NewGoal();
    }

    public class ValueTitle {
        public double value { get; set; }
        public string title { get; set; }
    }

    public class ValueTitleDescription {
        public double value { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public double max { get; set; }

    }

    public class Pal {
        public string code { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public double value { get; set; }
        public double min { get; set; }
        public double max { get; set; }

    }

    public class RecommenderWeight {
        public double min { get; set; }
        public double max { get; set; }
    }

    public class WaistValue {
        public double value { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int increasedRisk { get; set; }
        public int highRisk { get; set; }
    }
    #endregion

    #region WebMethods
    [WebMethod]
    public string Init() {
        NewCalculation x = new NewCalculation();
        x.bmi = new ValueTitle();
        x.whr = new ValueTitleDescription();
        x.waist = new WaistValue();
        x.bmr = 0.0;
        x.tee = 0.0;
        x.recommendedEnergyIntake = 0;
        x.recommendedEnergyExpenditure = 0;
        x.recommendedWeight = new RecommenderWeight();
        x.goal = new Goals.NewGoal();
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string GetCalculation(ClientsData.NewClientData client) {
        NewCalculation x = new NewCalculation();
        x.bmi = Bmi(client);
        x.whr = Whr(client);
        x.waist = Waist(client);
        x.bmr = Bmr(client);
        x.tee = Tee(client);
        x.recommendedEnergyIntake = RecommendedEnergyIntake(client);
        x.recommendedEnergyExpenditure = RecommendedEnergyExpenditure(client);
        x.recommendedWeight = RecommendedWeight(client);
        x.goal = RecommendedGoal(client);
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

     [WebMethod]
    public string LoadPal() {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"SELECT code, title, palDescription, palMin, palMax FROM pal";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<Pal> xx = new List<Pal>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                Pal x = new Pal();
                x.code = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.min = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                x.max = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
                x.value = Math.Round((x.min + x.max) / 2, 2);
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

   

    [WebMethod]
    public string GetPalDetails(double palValue) {
        try {
            Pal x = new Pal();
            x = GetPal(palValue);
            string json = JsonConvert.SerializeObject(x, Formatting.Indented);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    #endregion

    #region Methods
    public int Age(string birthDate) {
        int today = DateTime.UtcNow.Year;
        return today - Convert.ToDateTime(birthDate).Year - (Convert.ToDateTime(birthDate).DayOfYear > DateTime.UtcNow.DayOfYear ? 1 : 0);
    }

    public Pal GetPal(double palValue) {
          try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"SELECT code, title, palDescription, palMin, palMax FROM pal WHERE @palValue >= palMin AND @palValue < palMax";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.Add(new SQLiteParameter("palValue", palValue));
            Pal x = new Pal();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                x.code = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
                x.description = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);
                x.min = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                x.max = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
                x.value = Math.Round((x.min + x.max) / 2, 2);
            }
            connection.Close();
            return x;
        } catch (Exception e) { return new Pal(); }

    }

    private ValueTitle Bmi(ClientsData.NewClientData client) {
        ValueTitle x = new ValueTitle();
        x.value = Math.Round(client.weight * 10000 / (client.height * client.height), 2);
        if (x.value < 18.5) { x.title = "underweight"; }
        if (x.value >= 18.5 && x.value <= 25) { x.title = "normal weight"; }
        if (x.value > 25 && x.value < 30) { x.title = "overweight"; }
        if (x.value >= 30) { x.title = "obese"; }
        return x;
    }

    private ValueTitleDescription Whr(ClientsData.NewClientData client) {
        ValueTitleDescription x = new ValueTitleDescription();
        x.value = Convert.ToDouble(client.waist) / Convert.ToDouble(client.hip);

        //U dodatnoj procjeni kardiovaskularnog rizika koristimo omjer opsega struka i opsega bokova (WHR - Waist - Hip ratio) koji za žene mora biti viši od 1.0, a za muškarce 1.1.,
        // The WHO states that abdominal obesity is defined as a waist-hip ratio above 0.90 for males and above 0.85 for females, or a body mass index (BMI) above 30.0.[5] The National Institute of Diabetes, Digestive and Kidney Diseases (NIDDK) states that women with waist-hip ratios of more than 0.8, and men with more than 1.0, are at increased health risk because of their fat distribution

        if (x.value <= 1 ) {    
            x.title = "android fat distribution";
            x.description = "in the case of fatty tissue accumulation, it accumulates in the area of the waist";
            //TODO x.reccommandation min max    
        }
        if (x.value > 1) {
            x.title = "gynoid fat distribution";
            x.description = "in the case of fatty tissue accumulation, it accumulates in the area of the hips";
        }
        if (client.gender.value == 0) { x.max = 1.1; }  // male
        if (client.gender.value == 1) { x.max = 1; }  // female

        return x;
    }

    private WaistValue Waist(ClientsData.NewClientData client) {
        WaistValue x = new WaistValue();
        //TODO
        // Pri tom se služimo opsegom struka koji za bijelu rasu za žene ne smije prelaziti 88 cm, a za muškarce 102 cm.

        x.value = client.waist;
        //opseg struka između 94 i 102 cm predstavlja povećan rizik od pojave različitih bolesti (npr. šećerne bolesti i bolesti srca)
        //opseg struka iznad 102 cm predstavlja vrlo visok rizik od pojave različitih bolesti (npr. šećerne bolesti i bolesti srca)

        //A waist circumference of 102 centimetres(40 inches) or more in men, or 88 centimetres(35 inches) or more in women,
        //is associated with health problems such as type 2 diabetes, heart disease and high blood pressure.

        // ***** male *****
        if (client.gender.value == 0) {
            x.increasedRisk = 94;
            x.highRisk = 102;
        }

        // ***** female *****
        if (client.gender.value == 1) {
            x.increasedRisk = 80;
            x.highRisk = 88;
        }  

        if (x.value >= x.increasedRisk && x.value < x.highRisk) {
            x.title = "increased risk of various diseases";
            x.description = string.Format("the waist circumference between {0} and {1} cm represents an increased risk of various diseases (eg diabetes and heart disease)", x.increasedRisk, x.highRisk);
        }
        if (x.value >= x.highRisk) {
            x.title = "very high risk of various diseases";
            x.description = string.Format("the waist circumference above {0} cm represents a very high risk of various diseases (eg diabetes and heart disease)", x.highRisk);
        }


        /****** OLD   **********
          if (x.value >= 94 && x.value <= 102) {
             x.title = "increased risk of various diseases";
             x.description = "the waist circumference between 94 and 102 cm represents an increased risk of various diseases (eg diabetes and heart disease)";
         }
         if (x.value > 102) {
             x.title = "very high risk of various diseases";
             x.description = "the waist circumference above 102 cm represents a very high risk of various diseases (eg diabetes and heart disease)";
         }
         if (client.gender.value == 0) { x.max = 102; }  // male
         if (client.gender.value == 1) { x.max = 88; }  // female
         **************/

        return x;
    }

    private double Bmr(ClientsData.NewClientData client) {
        int a = client.gender.value == 0 ? 5 : -161;
        double BMR = 10 * client.weight + 6.25 * client.height - 5 * client.age + a;
        return BMR;
    }

    private double Tee(ClientsData.NewClientData client) {
        /*
        The Harris–Benedict equations revised by Mifflin and St Jeor in 1990
        Men	BMR = (10 × weight in kg) + (6.25 × height in cm) - (5 × age in years) + 5
        Women	BMR = (10 × weight in kg) + (6.25 × height in cm) - (5 × age in years) - 161

        Little to no exercise	Daily kilocalories needed = BMR x 1.2
        Light exercise (1–3 days per week)	Daily kilocalories needed = BMR x 1.375
        Moderate exercise (3–5 days per week)	Daily kilocalories needed = BMR x 1.55
        Heavy exercise (6–7 days per week)	Daily kilocalories needed = BMR x 1.725
        Very heavy exercise (twice per day, extra heavy workouts)	Daily kilocalories needed = BMR x 1.9
        
        Both BMR, and RMR, estimate the number of calories you burn at rest,
        but RMR takes additional factors into consideration when determining needs.
        BMR measures your basal energy expenditure, or BEE.
        The BEE is a 24 hour estimation of the number of calories you burn maintaining your most basic bodily functions,
        such as breathing, circulating blood and growing and repairing cells.
        RMR measures your resting energy expenditure.
        REE determines the number of calories you burn in a 24 hour period maintaining basic bodily functions,
        but also includes the number of calories burned eating and conducting small amounts of activity. 
         */

        //int a = client.gender.value == 0 ? 5 : -161;
        //double BMR = 10 * client.weight + 6.25 * client.height - 5 * client.age + a;

        double BMR = Bmr(client);



        double DIT = 0.1 * (client.pal.value * BMR);
        double TEE = client.pal.value * BMR + DIT;
        return Math.Round(TEE, 2);
    }

    public int RecommendedEnergyIntake(ClientsData.NewClientData client) {
        ValueTitle b = Bmi(client);
        double bmi = b.value;
        int tee = Convert.ToInt32(Tee(client));

        int expenditure = RecommendedEnergyExpenditure(client);

        int x = 0;
        if (bmi < 18.5) {
            x = tee + 300;
        }
        if (bmi >= 18.5 && bmi <= 25) {
            x = tee + expenditure;
        }
        if (bmi > 25) {
            x = tee - 300;
        }
        return x;
    }

    private int RecommendedEnergyExpenditure(ClientsData.NewClientData client) {
        int x = 0;
        double pal = client.pal.value;
        double bmi = Bmi(client).value;
        if (pal < 1.55) { x = 200; }
        if (pal < 1.55 && bmi <= 25) { x = 200; }
        if (pal >= 1.55 && pal < 1.8 && bmi > 25) { x = 100; }
        if (pal >= 1.55 && pal < 1.8 && bmi <= 25){ x = 100; }
        if (pal >= 1.8 && bmi <= 25) { x = 0; }
        if (pal >= 1.8 && bmi > 25) { x = 0; }

        return x;
    }

    private RecommenderWeight RecommendedWeight(ClientsData.NewClientData client) {
        RecommenderWeight x = new RecommenderWeight();
        x.min = Math.Round((18.5 * client.height * client.height) / 10000, 1);
        x.max = Math.Round((25.0 * client.height * client.height) / 10000, 1);
        return x;
    }

    private Goals.NewGoal RecommendedGoal(ClientsData.NewClientData client) {
        Goals.NewGoal x = new Goals.NewGoal();
        double bmi = Bmi(client).value;

        if (bmi < 18.5) { x.code = "G3"; }
        if (bmi >= 18.5 && bmi <= 25) { x.code = "G2"; }
        if (bmi > 25 && bmi < 30) { x.code = "G1"; }
        if (bmi >= 30) { x.code = "G1"; }

        x.isDisabled = false;
        return x;
    }
    #endregion

}
