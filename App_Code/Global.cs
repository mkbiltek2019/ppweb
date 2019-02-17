using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Global
/// </summary>
namespace Igprog {
    public class Global {
        public Global() {
        }

        public double DateDiff(string date1, string date2) {
            try {
                DateTime date1_ = Convert.ToDateTime(date1);
                DateTime date2_ = Convert.ToDateTime(date2);
                return (date2_ - date1_).TotalDays;
            } catch(Exception e) {
                return 0;
            }
        }
    }
}