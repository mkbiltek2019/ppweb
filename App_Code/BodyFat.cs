using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.IO;
using Igprog;

/// <summary>
/// BodyFat
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class BodyFat : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
    DataBase db = new DataBase();
    Equations E = new Equations();
    public BodyFat() {
    }

    //Jackson/Pollock 3 Caliper Method
    public class CaliperMeasurement {
        public string code;
        public string title;
        public string description;
        public double value;
        public bool isSelected;
    }

    public class CaliperMethod {
        //public string id;
        public string recordDate;
        public string code;
        public string title;
        public string description;
        public List<CaliperMeasurement> measurements;
        public double bodyFat;
        public ClientsData.NewClientData clientData;
    }

    public class CaliperData {
        public CaliperMethod data;
        public List<CaliperMethod> methods;
    }

    public string Chest = "CH";
    public string Abdominal = "AB";
    public string Thigh = "TH";
    public string Tricep = "TR";
    public string Subscapular = "SUB";
    public string Suprailiac = "SU";
    public string Midaxillary = "MI";
    public string Bicep = "BI";

    //TODO: CalipherMethods
    public string JacksonPollock3 = "JP3"; // Jackson/Pollock 3 Caliper Method
    public string JacksonPollock4 = "JP4"; // Jackson/Pollock 4 Caliper Method
    public string JacksonPollock7 = "JP7"; // Jackson/Pollock 7 Caliper Method
    public string DurninWomersley = "DW"; // Durnin/Womersley Caliper Method


    #region WebMethods
    [WebMethod]
    public string InitCaliperMeasurements(ClientsData.NewClientData clientData) {
        try {
            return JsonConvert.SerializeObject(InitCaliper(clientData), Formatting.None);
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string CaliperCalculate(CaliperMethod data) {
        try {
            return JsonConvert.SerializeObject(CaliperCalc(data), Formatting.None);
        }
        catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(CaliperMethod x) {
        try {
            db.CreateDataBase(x.clientData.userId, db.bodyfat);
            //TODO: save measurements (code:val;code:val)
            string sql = string.Format(@"BEGIN;
                    INSERT OR REPLACE INTO bodyfat (recordDate, clientId, bodyFat, records, recordMethod)
                    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}');
                    COMMIT;", x.recordDate, x.clientData.clientId, x.bodyFat, "TODO: Measurements", x.code);
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(x.clientData.userId, dataBase))) {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(x.bodyFat, Formatting.None);
        } catch (Exception e) { return (e.Message); }
    }


    public CaliperMethod GetLastMeasurement(ClientsData.NewClientData clientData) {
        List<CaliperMethod> xx = new List<CaliperMethod>();
        CaliperMethod lastRecord = new CaliperMethod();
        db.CreateDataBase(clientData.userId, db.bodyfat);
        string sql = string.Format("SELECT recordDate, bodyFat, records, recordMethod FROM bodyfat WHERE clientId = '{0}'", clientData.clientId);
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(clientData.userId, dataBase))) {
            connection.Open();
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        CaliperMethod x = new CaliperMethod();
                        x.recordDate = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                        x.bodyFat = reader.GetValue(1) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(1));
                        //x.measurements = reader.GetValue(2) == DBNull.Value ? "" : reader.GetString(2);  //todo
                        x.code = reader.GetValue(3) == DBNull.Value ? "" : reader.GetString(3);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        lastRecord = xx.OrderByDescending(a => Convert.ToDateTime(a.recordDate)).FirstOrDefault();
        return lastRecord;
    }


    #endregion WebMethods



    public CaliperData InitCaliper(ClientsData.NewClientData clientData) {
        CaliperData x = new CaliperData();
        x.methods = GetCaliperMethods(clientData.gender.value);
        x.data = GetLastMeasurement(clientData);

        if (string.IsNullOrEmpty(x.data.code)) {
            x.data = x.methods[0];  // Jackson/Pollock 3 Caliper Method
        }
        x.data.clientData = clientData;
        return x;
    }

    public List<CaliperMethod> GetCaliperMethods(int gender) {
        List<CaliperMethod> x = new List<CaliperMethod>();
        x.Add(GetCaliperMeasurements(JacksonPollock3, "Jackson/Pollock 3-Site Caliper Method", null, gender));
        x.Add(GetCaliperMeasurements(JacksonPollock4, "Jackson/Pollock 4-Site Caliper Method", null, gender));
        x.Add(GetCaliperMeasurements(JacksonPollock7, "Jackson/Pollock 7-Site Caliper Method", null, gender));
        x.Add(GetCaliperMeasurements(DurninWomersley, "Durnin/Womersley Caliper Method", null, gender));
        return x;
    }

    public CaliperMethod GetCaliperMeasurements(string code, string title, string description, int gender) {
        CaliperMethod x = new CaliperMethod();
        x.code = code;
        x.title = title;
        x.description = description;
        x.measurements = new List<CaliperMeasurement>();
        x.measurements.Add(new CaliperMeasurement { code = Chest, title = "Chest", description = "", value = 0, isSelected = CheckCaliperMethod(code, Chest, gender) });
        x.measurements.Add(new CaliperMeasurement { code = Abdominal, title = "Abdominal", description = "", value = 0, isSelected = CheckCaliperMethod(code, Abdominal, gender) });
        x.measurements.Add(new CaliperMeasurement { code = Thigh, title = "Thigh", description = "", value = 0, isSelected = CheckCaliperMethod(code, Thigh, gender) });
        x.measurements.Add(new CaliperMeasurement { code = Tricep, title = "Tricep", description = "", value = 0, isSelected = CheckCaliperMethod(code, Tricep, gender) });
        x.measurements.Add(new CaliperMeasurement { code = Subscapular, title = "Subscapular", description = "", value = 0, isSelected = CheckCaliperMethod(code, Subscapular, gender) });
        x.measurements.Add(new CaliperMeasurement { code = Suprailiac, title = "Suprailiac", description = "", value = 0, isSelected = CheckCaliperMethod(code, Suprailiac, gender) });
        x.measurements.Add(new CaliperMeasurement { code = Midaxillary, title = "Midaxillary", description = "", value = 0, isSelected = CheckCaliperMethod(code, Midaxillary, gender) });
        x.measurements.Add(new CaliperMeasurement { code = Bicep, title = "Bicep", description = "", value = 0, isSelected = CheckCaliperMethod(code, Bicep, gender) });

        return x;
    }

    public bool CheckCaliperMethod(string method, string measure, int gender) {
        bool x = false;
        if (method == JacksonPollock3) {
            if (gender == 0) {
                if (measure == Chest || measure == Abdominal || measure == Thigh) {
                    x = true;
                }
            } else {
                if (measure == Thigh || measure == Tricep || measure == Suprailiac) {
                    x = true;
                }
            }
        }
        if (method == JacksonPollock4) {
            if (measure == Abdominal || measure == Thigh || measure == Tricep || measure == Suprailiac) {
                x = true;
            }
        }
        if (method == JacksonPollock7) {
            if (measure == Chest || measure == Abdominal || measure == Thigh || measure == Tricep || measure == Subscapular || measure == Suprailiac || measure == Midaxillary) {
                x = true;
            }
        }
        if (method == DurninWomersley) {
            if (measure == Bicep || measure == Tricep || measure == Subscapular || measure == Suprailiac) {
                x = true;
            }
        }
        return x;
    }

    public double CaliperCalc(CaliperMethod data) {
        double x = 0;
        double skinfolds = data.measurements.Where(a => a.isSelected == true).Sum(a => a.value);
        double bodyDensity = 0;
        int gender = data.clientData.gender.value;
        int age = data.clientData.age;

        if (data.code == JacksonPollock3) {
            if (gender == 0) {
                //Body Density = 1.10938 – (0.0008267 x sum of skinfolds) +(0.0000016 x square of the sum of skinfolds) – (0.0002574 x age)
                bodyDensity = 1.10938 - (0.0008267 * skinfolds) + (0.0000016 * skinfolds * skinfolds) - (0.0002574 * age);
            } else {
                //Body Density = 1.0994921 – (0.0009929 x sum of skinfolds) +(0.0000023 x square of the sum of skinfolds) – (0.0001392 x age)
                bodyDensity = 1.0994921 - (0.0009929 * skinfolds) + (0.0000023 * skinfolds * skinfolds) - (0.0001392 * age);
            }
            x = (495 / bodyDensity) - 450;
        }
        if (data.code == JacksonPollock4) {
            if (gender == 0) {
                //Body Density = (0.29288 x sum of skinfolds) – (0.0005 x square of the sum of skinfolds) + (0.15845 x age) – 5.76377
                x = (0.29288 * skinfolds) - (0.0005 * skinfolds * skinfolds) + (0.15845 * age) - 5.76377;
            } else {
                //Body Density = (0.29669 x sum of skinfolds) – (0.00043 x square of the sum of skinfolds) + (0.02963 x age) + 1.4072
                x = (0.29669 * skinfolds) - (0.00043 * skinfolds * skinfolds) + (0.02963 * age) + 1.4072;
            }
        }
        if (data.code == JacksonPollock7) {
            if (gender == 0) {
                //Body Density = 1.112 – (0.00043499 x sum of skinfolds) + (0.00000055 x square of the sum of skinfold sites) – (0.00028826 x age)
                bodyDensity = 1.112 - (0.00043499 * skinfolds) + (0.00000055 * skinfolds * skinfolds) - (0.00028826 * age);
            } else {
                //Body Density = 1.097 – (0.00046971 x sum of skinfolds) + (0.00000056 x square of the sum of skinfold sites) – (0.00012828 x age)
                bodyDensity = 1.097 - (0.00046971 * skinfolds) + (0.00000056 * skinfolds * skinfolds) - (0.00012828 * age);
            }
            x = (495 / bodyDensity) - 450;
        }
        if (data.code == DurninWomersley) {
            double log = Math.Log10(skinfolds); //Log of the sum of skinfolds
            if (gender == 0) {
                if (age < 17) {
                    bodyDensity = 1.1533 - (0.0643 * log);
                }
                if (age > 17 && age < 20) {
                    bodyDensity = 1.1620 - (0.0630 * log);
                }
                if (age > 20 && age < 30) {
                    bodyDensity = 1.1631 - (0.0632 * log);
                }
                if (age > 30 && age < 40) {
                    bodyDensity = 1.1422 - (0.0544 * log);
                }
                if (age > 40 && age < 50) {
                    bodyDensity = 1.1620 - (0.0700 * log);
                } if (age > 50) {
                    bodyDensity = 1.1715 - (0.0779 * log);
                }
            } else {
                if (age < 17) {
                    bodyDensity = 1.1369 - (0.0598 * log);
                }
                if (age > 17 && age < 20) {
                    bodyDensity = 1.1549 - (0.0678 * log);
                }
                if (age > 20 && age < 30) {
                    bodyDensity = 1.1599 - (0.0717 * log);
                }
                if (age > 30 && age < 40) {
                    bodyDensity = 1.1423 - (0.0632 * log);
                }
                if (age > 40 && age < 50) {
                    bodyDensity = 1.1333 - (0.0612 * log);
                }
                if (age > 50) {
                    bodyDensity = 1.1339 - (0.0645 * log);
                }
            }
            x = (495 / bodyDensity) - 450;
        }

        return Math.Round(x, 1);
    }


}
