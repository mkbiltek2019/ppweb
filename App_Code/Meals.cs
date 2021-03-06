﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SQLite;
using Igprog;

/// <summary>
/// Meals
/// </summary>
[WebService(Namespace = "http://programprehrane.com/app")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Meals : System.Web.Services.WebService {
    string dataBase = ConfigurationManager.AppSettings["AppDataBase"];

    public Meals() {
    }

    public class NewMeal {
        public string code;
        public string title;
        public string description;

        //TODO desctiption
        // public List<DishDesctiption> description;

        public bool isSelected;
        public bool isDisabled;
       
    }

    //public class DishDesctiption {
    //    public string title;
    //    public string description;
    //}

    [WebMethod]
    public string Load() {
        try {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + Server.MapPath("~/App_Data/" + dataBase));
            connection.Open();
            string sql = @"SELECT code, title FROM codeBook WHERE codeGroup = 'MEALS' ORDER BY codeOrder ASC";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            List<NewMeal> xx = new List<NewMeal>();
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                NewMeal x = new NewMeal();
                x.code = reader.GetValue(0) == DBNull.Value ? "B" : reader.GetString(0);
                x.title = reader.GetValue(1) == DBNull.Value ? GetMealTitle("B", connection) : reader.GetString(1);
                x.description = "";
                x.isSelected = true;
                x.isDisabled = x.code == "B" || x.code == "L" || x.code == "D" ? true : false;
                xx.Add(x);
            }
            connection.Close();
            string json = JsonConvert.SerializeObject(xx, Formatting.None);
            return json;
        } catch (Exception e) { return ("Error: " + e); }
    }

    public static string GetMealTitle(string code, SQLiteConnection connection) {
        string title = "";
        try {
            string sql = @"SELECT title
                        FROM codeBook 
                        WHERE code = @code AND codeGroup = 'MEALS'";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection)) {
                command.Parameters.Add(new SQLiteParameter("code", code));
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        title = reader.GetValue(0) == DBNull.Value ? "" : reader.GetString(0);
                    }
                } 
            }
        } catch (Exception e) { return null; }
        return title;
    }

}
