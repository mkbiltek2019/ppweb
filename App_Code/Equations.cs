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
            public bool isDisabled;
        }

        public string MifflinStJeor = "MSJ";
        public string HarrisBenedictsRozaAndShizgal = "HBRS";
        public string KatchMcArdle = "KMA";
        public string HarrisBenedicts = "HB";
        public string Cunningham = "C";
        public string Owen = "O";

        //public class Lbm {
        //    public double val;
        //    public string desc;
        //}

        public class BodyFat {
            public double bodyFatPerc; /*** (%) ***/
            public double bodyFatMass; /*** (kg) ***/
            public double lbm;     /*** LBM lean body mass ***/
            public string description;
        }

        public List<BmrEquation> GetBmrEquations(int userType) {
            List<BmrEquation> x = new List<BmrEquation>();
            x.Add(new BmrEquation { code = MifflinStJeor, title = "Mifflin-St Jeor", description = "The Harris–Benedict equations revised by Mifflin and St Jeor in 1990", isDisabled = IsDisabled(MifflinStJeor, userType) });
            x.Add(new BmrEquation { code = HarrisBenedictsRozaAndShizgal, title = "Harris-Benedict (Roza and Shizgal)", description = "The Harris–Benedict equations revised by Roza and Shizgal in 1984", isDisabled = IsDisabled(HarrisBenedictsRozaAndShizgal, userType) });
            x.Add(new BmrEquation { code = KatchMcArdle, title = "Katch-McArdle", description = "The equation that takes into account lean body mass", isDisabled = IsDisabled(KatchMcArdle, userType) });
            x.Add(new BmrEquation { code = HarrisBenedicts, title = "Harris-Benedict", description = "The original Harris–Benedict equations published in 1918 and 1919", isDisabled = IsDisabled(HarrisBenedicts, userType) });
            //x.Add(new BmrEquation { code = Cunningham, title = "Cunningham", description = "", isDisabled = IsDisabled(Cunningham, userType) });
            x.Add(new BmrEquation { code = Owen, title = "Owen", description = "The older equation that is generally not as accurate as the others", isDisabled = IsDisabled(Owen, userType) });
            return x;
        }

        /*********  https://completehumanperformance.com/2013/10/08/calorie-needs/  ************/

        public double Bmr(ClientsData.NewClientData x) {
            double BMR = 0;
            string type = x.bmrEquation;
            if (type == HarrisBenedicts) {
                /***** The original Harris–Benedict equations published in 1918 and 1919 *****/
                if (x.gender.value == 0) {
                    BMR = 66.5 + 13.75 * x.weight + 5.003 * x.height - 6.755 * x.age;  // Men
                } else {
                    BMR = 655.1 + 9.563 * x.weight + 1.85 * x.height - 4.676 * x.age;  // Women
                }
            } else if (type == HarrisBenedictsRozaAndShizgal) {
                /***** The Harris–Benedict equations revised by Roza and Shizgal in 1984 *****/
                //Men BMR = 88.362 + (13.397 × weight in kg) +(4.799 × height in cm) -(5.677 × age in years)
                //Women BMR = 447.593 + (9.247 × weight in kg) +(3.098 × height in cm) -(4.330 × age in years)
                if (x.gender.value == 0) {
                    BMR = 88.362 + (13.397 * x.weight) + (4.799 * x.height) - (5.677 * x.age);  // Men
                } else {
                    BMR = 447.593 + (9.247 * x.weight) + (3.098 * x.height) - (4.330 * x.age);  // Women
                }
            } else if (type == MifflinStJeor) {
                //BMR (Men) = (10 × weight in kg) +(6.25 × height in cm) − (5 × age in years) +5
                //BMR (Women) = (10 × weight in kg) + (6.25 × height in cm) − (5 × age in years) − 161
                int a = x.gender.value == 0 ? 5 : -161;
                BMR = 10 * x.weight + 6.25 * x.height - 5 * x.age + a;
            } else if (type == KatchMcArdle) {
                //TODO:
                //        Katch-Mcardle BMR Formula:
                //BMR = 370 + (21.6 x Lean Body Mass(kg) )
                //Lean Body Mass = (Weight(kg) x(100-(Body Fat)))/100
                BMR = 370 + 21.6 * GetBodyFat(x).lbm;
            } else if (type == Cunningham) {
                //TODO:
                /****** Cunninghams = 500 + 22(lean body mass[LBM] in kg) ******/
            } else if (type == Owen) {
                //Men: RMR = 879 + 10.2 X weight
                //Women: RMR = 795 + 7.18 X weight
                if (x.gender.value == 0) {
                    BMR = 879 + 10.2 * x.weight;  // Men
                } else {
                    BMR = 795 + 7.18 * x.weight;  // Women
                }
            } else {
                /****** DEFAULT:  Mifflin - St.Jeor = 5 + 10(weight in kg) + 6.25(height in cm) − 5(age) ******/
                int a = x.gender.value == 0 ? 5 : -161;
                BMR = 10 * x.weight + 6.25 * x.height - 5 * x.age + a;
            }
            return BMR;
        }

        /***** Lean Body Mass *****/
        public BodyFat GetBodyFat(ClientsData.NewClientData x) {
            BodyFat bf = new BodyFat();
            // Lean Body Mass = (Weight(kg) x(100-(Body Fat)))/100
            if (x.bodyFat.bodyFatPerc > 0) {
                bf.lbm = (x.weight * (100 - (x.bodyFat.bodyFatPerc))) / 100;
                bf.bodyFatMass = x.weight - bf.lbm;
                bf.bodyFatPerc = x.bodyFat.bodyFatPerc;
                bf.description = GetLmbDesc(x);
            }
            return bf;
        }

        public string GetLmbDesc(ClientsData.NewClientData x) {

            /***** http://www.linear-software.com/online.html *****/
            //            Body Fat Chart
            //Classification  Women Men
            //Essential Fat   10 - 12 % 2 - 4 %
            //Athletes    14 - 20 % 6 - 13 %
            //Fitness 21 - 24 % 14 - 17 %
            //Acceptable  25 - 31 % 18 - 25 %
            //Obese   32 % plus    25 % plus
            string desc = null;
            double val = x.bodyFat.bodyFatPerc;
            if (x.gender.value == 0) {
                if (val >= 2 && val < 6) {
                    desc = "essential fat";
                }
                if (val >= 6 && val < 14) {
                    desc = "athletes";
                }
                if (val >= 14 && val < 18) {
                    desc = "ftness";
                }
                if (val >= 18 && val < 25) {
                    desc = "acceptable";
                }
                if (val >= 25) {
                    desc = "obese";
                }
            } else {
                if (val >= 10 && val < 14) {
                    desc = "essential fat";
                }
                if (val >= 14 && val < 21) {
                    desc = "athletes";
                }
                if (val >= 21 && val < 25) {
                    desc = "ftness";
                }
                if (val >= 25 && val < 32) {
                    desc = "acceptable";
                }
                if (val >= 32) {
                    desc = "Obese";
                }
            }
            return desc;
        }

        public bool IsDisabled(string code, int userType) {
            bool x = true;
            if (userType < 1) {
                if (code == MifflinStJeor) {
                    x = false;
                }
            } else if (userType == 1) {
                if (code == MifflinStJeor || code == HarrisBenedictsRozaAndShizgal || code == HarrisBenedicts) {
                    x = false;
                }
            } else {
                x = false;
            }
            return x;
        }
    }
}
