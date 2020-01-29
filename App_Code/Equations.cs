using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Equations
/// </summary>
namespace Igprog {
    public class Equations {
        public Equations() {
        }

        public class BmrEquation {
            public string code;
            public string title;
            public string description;
        }

        public string HarrisBenedicts = "HB";
        public string Mifflin = "MSJ";
        public string Cunningham = "C";
        public string Owen = "O";

        public List<BmrEquation> GetBmrEquations() {
            List<BmrEquation> x = new List<BmrEquation>();
            x.Add(new BmrEquation { code = HarrisBenedicts, title = "Harris - Benedict", description = "" });
            x.Add(new BmrEquation { code = Mifflin, title = "Mifflin - St Jeor", description = "" });
            //x.Add(new BmrEquation { code = Cunningham, title = "Cunningham", description = "" });
            x.Add(new BmrEquation { code = Owen, title = "Owen", description = "" });
            return x;
        }

        public double Bmr(ClientsData.NewClientData x) {
            double BMR = 0;
            string type = x.bmrEquation;
            if (type == HarrisBenedicts) {
                /****** Harris - Benedicts = 66.47 + 13.75(weight in kg) + 5(height in cm) − 6.76(age) ******/

                //TODO:
                //Men: RMR = 66.47 + 13.75 X weight +5.003 X height – 6.755 X age
                //Women: RMR = 655.1 + 9.563 X weight +1.85 X height – 4.676 X age

                BMR = 66.47 + 13.75 * x.weight + 5 * x.height - 6.76 * x.age;
            } else if (type == Mifflin) {
                /****** Mifflin - St.Jeor = 5 + 10(weight in kg) + 6.25(height in cm) − 5(age) ******/

                //BMR (Men) = (10 × weight in kg) +(6.25 × height in cm) − (5 × age in years) +5
                //BMR (Women) = (10 × weight in kg) + (6.25 × height in cm) − (5 × age in years) − 161

                int a = x.gender.value == 0 ? 5 : -161;
                BMR = 10 * x.weight + 6.25 * x.height - 5 * x.age + a;
            } else if (type == Cunningham) {
                /****** Cunninghams = 500 + 22(lean body mass[LBM] in kg) ******/
            } else if (type == Owen) {
                /****** Owen = 655.096 + 1.8496(height in cm) + 9.5634(weight in kg) − 4.6759(age) << A reappraisal of caloric requirements in healthy women. ******/

                //TODO:
                //Men: RMR = 879 + 10.2 X weight
                //Women: RMR = 795 + 7.18 X weight

                BMR = 655.096 + 1.8496 * x.height + 9.5634 * x.weight - 4.6759 * x.age;
                //BMR = 655.096 * client.weight + 6.25 * client.height - 5 * client.age + a;
            } else {
                /****** DEFAULT:  Mifflin - St.Jeor = 5 + 10(weight in kg) + 6.25(height in cm) − 5(age) ******/
                int a = x.gender.value == 0 ? 5 : -161;
                BMR = 10 * x.weight + 6.25 * x.height - 5 * x.age + a;
            }
            return BMR;
        }
    }
}
