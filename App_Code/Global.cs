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

        /****************** Date ***********************/
        public int DateDiff(DateTime date1, DateTime date2) {
            try {
                return Convert.ToInt32(Math.Abs((date2 - date1).TotalDays));
            } catch (Exception e) {
                return 0;
            }
        }

        public int DateDiff(string date1, string date2) {
            try {
                DateTime date1_ = Convert.ToDateTime(date1);
                DateTime date2_ = Convert.ToDateTime(date2);
                return DateDiff(date1_, date2_);
            } catch (Exception e) {
                return 0;
            }
            
        }

        public int DateDiff(string date) {
            try {
                return DateDiff(Convert.ToDateTime(date), DateTime.UtcNow);
            } catch (Exception e) {
                return 0;
            }
        }
        /*************************************************/


    }
}