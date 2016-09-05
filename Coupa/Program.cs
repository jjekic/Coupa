using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Coupa
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmCoupa());
        }

        //Ex("15.23+45.6/2.3-5.6*1.2-45.6/7.1") = 21.913551745254132
        //Ex("15.2 + (2.3 + sin(0.7 + .01) * 5)") = 20.759168855107681
        public static double Ex(string sExpr)
        {
            double fRes = 0f;

            sExpr = sExpr.Replace(" ", "");

            if (Validate(sExpr) != "ok")
                return -1;

            while (sExpr.IndexOf("(") > -1)
            {
                Int32 iStart = 0, iLen = 0, iPos = 0;
                char[] ncExpr = sExpr.ToCharArray();
                foreach (char ch in ncExpr)
                {
                    if (ch == '(')
                        iStart = iPos+1;
                    if (ch == ')')
                    {
                        iLen = iPos - iStart;
                        break;
                    }
                    iPos++;
                }
                string sTmp = sExpr.Substring(iStart, iLen);
                fRes = Ev(sTmp);
                string sFun = GetFunName(sExpr, iStart);
                if (sFun.Length > 0)
                    fRes = Func(sFun, fRes);

                sExpr = sExpr.Replace(sFun + "(" + sTmp + ")", fRes.ToString());
            }

            fRes = Ev(sExpr);
            return fRes;
        }

        private static string GetFunName(string sExpr, Int32 iStart)
        {
            string sRes = "";
            Int32 iPos = iStart - 2;
            char[] ncExpr = sExpr.ToCharArray();
            while (Char.IsLetter(ncExpr[iPos]))
            {
                iPos--;
            }
            if (iPos == iStart - 2)
                sRes = "";
            else
                sRes = sExpr.Substring(iPos+1, iStart - 2 - iPos);
            return sRes;
        }

        private static string Validate(string sExpr)
        {
            Int32 iStart = 0;
            char[] ncExpr = sExpr.ToCharArray();
            foreach (char ch in ncExpr)
            {
                if (ch == '(')
                    iStart++;
                if (ch == ')')
                    iStart--;
            }
            if (iStart == 0)
                return "ok";
            else
                return "invalid numbers of brackets";
        }

        private static double Ev(string sExpr)
        {
            string sOpers = "^*/+-";//"+-*/%|&^";
            char[] cOpers = sOpers.ToCharArray();
            string[] nNums = sExpr.Split(cOpers);
            double fRes = double.MaxValue;

            while (nNums.Length > 1)
            {
                string sOper = "", sTE = sExpr;

                for (Int32 i = 1; i < nNums.Length; i++)
                {
                    sTE = sTE.Replace(nNums[i - 1], "");
                    sOper += sTE.Substring(0, 1);
                    sTE = sTE.Substring(1);
                }
                
                foreach (char ch in cOpers)
                {
                    Int32 iPos = sOper.IndexOf(ch);
                    if (iPos > -1)
                    {
                        fRes = Oper(double.Parse(nNums[iPos]), sOper.Substring(iPos, 1), double.Parse(nNums[iPos + 1]));
                        sExpr = sExpr.Replace(nNums[iPos] + sOper.Substring(iPos, 1) + nNums[iPos + 1], fRes.ToString());
                        break;
                    }
                }

                nNums = sExpr.Split(cOpers);
            }

            return fRes;
        }

        private static double Func(string sFuncs, double f1)
        {
            double fRes = 0f;
            switch (sFuncs)
            {
                case "sin": fRes = (double)Math.Sin(f1); break;
                case "cos": fRes = (double)Math.Cos(f1); break;
                case "tan": fRes = (double)Math.Tan(f1); break;
                case "asin": fRes = (double)Math.Asin(f1); break;
                case "acos": fRes = (double)Math.Acos(f1); break;
                case "atan": fRes = (double)Math.Atan(f1); break;
                case "exp": fRes = (double)Math.Exp(f1); break;
                case "log": fRes = (double)Math.Log10(f1); break;
                case "abs": fRes = (double)Math.Abs(f1); break;
                case "sign": fRes = (double)Math.Sign(f1); break;
                case "sqrt": fRes = (double)Math.Sqrt(f1); break;
            }
            return fRes;
        }

        private static double Oper(double f1, string sOper, double f2)
        {
            double fRes = 0f;
            switch (sOper)
            { 
                case "+": fRes = f1 + f2; break;
                case "-": fRes = f1 - f2; break;
                case "*": fRes = f1 * f2; break;
                case "/": fRes = f1 / f2; break;
                //case '%': fRes = f1 + f2; break;
                //case '|': fRes = f1 + f2; break;
                //case '%': fRes = f1 + f2; break;
                //case '|': fRes = f1 + f2; break;
                //case '&': fRes = f1 + f2; break;
                case "^": fRes = (double)Math.Pow(f1,f2); break;
            }
            return fRes;
        }

    }
}
