using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Translate
/// </summary>
namespace Igprog {
    public class Translate {
        public Translate() {
        }

        public string Tran(string title, string lang) {
            try {
                string path = string.Format("~/app/assets/json/translations/{0}/main.json", lang); ;
                string path1 = HttpContext.Current.Server.MapPath(path);
                if (File.Exists(HttpContext.Current.Server.MapPath(path))) {
                    string json = File.ReadAllText(HttpContext.Current.Server.MapPath(path));
                    string[] ss = Regex.Split(json, ",\r\n");
                    foreach (string s in ss) {
                        string[] _s = s.Split(':');
                        if (_s.Count() == 2) {
                            if (_s[0].Replace("\"", "").Replace("\r", "").Replace("\n","").Replace("{\r\n", "").Trim().ToLower().ToString() == title.ToLower()) {
                                title = s.Split(':')[1].Replace("\"", "").Replace("\r\n}", "").Trim().ToString();
                            }
                        }
                    }
                    return title;
                } else {
                    return title;
                }
            } catch (Exception e) { return (e.Message); }
        }
    }

}
