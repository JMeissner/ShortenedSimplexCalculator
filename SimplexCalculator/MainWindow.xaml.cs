using Fractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimplexCalculator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Fraction[][] tableau;
        public Fraction[][] oldtableau;

        public string[] labelX;
        public string[] labelY;
        public int r = 0;
        public int s = 0;
        public Fraction ars;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Primal(object sender, RoutedEventArgs e)
        {
            GetPivotPrimal();

            if(r == -1 && s == -1)
            {
                //Stoprule I
                MessageBox.Show("Can't do primal step. Stoprule I - No negative Ci");
                CalcFctValue();
                return;
            }
            else if (r == -2 && s == -1)
            {
                //Stoprule II
                MessageBox.Show("Can't do primal step. Stoprule II - No postive Bi/Asi");
                CalcFctValue();
                return;
            }

            SimplexStep();
        }

        private void Button_Dual(object sender, RoutedEventArgs e)
        {
            GetPivotDual();

            if (r == -1 && s == -1)
            {
                //Stoprule I
                MessageBox.Show("Can't do dual step. Stoprule I - No negative Bi");
                CalcFctValue();
                return;
            }
            else if (s == -2)
            {
                //Stoprule II
                MessageBox.Show("Can't do dual step. Stoprule II - No negative Ci/Arj");
                CalcFctValue();
                return;
            }

            SimplexStep();
        }
        private void Button_Clear(object sender, RoutedEventArgs e)
        {
            function.Text = "";
            st.Text = "";
            StepText.Text = "";
        }
        private void Button_Setup(object sender, RoutedEventArgs e)
        {
            string[] maxfct = function.Text.Split(' ');

            string[] restrictions = st.Text.Split('\n');

            //add ci and z(x)
            tableau = new Fraction[restrictions.Length + 1][];
            tableau[0] = new Fraction[maxfct.Length + 1];
            int l = 0;
            foreach(string cs in maxfct)
            {
                if (cs.Contains("x"))
                {
                    int xi = cs.IndexOf('x');
                    string to = cs.Remove(xi);
                    if (to.Contains("+"))
                        to = to.Replace("+", "");
                    if (to.Trim().Equals(""))
                        to = "1";
                    tableau[0][l] = Fraction.FromString(to) * -1;
                }
                else
                {
                    tableau[0][l] = Fraction.FromString(cs) * -1;
                }
                l++;
            }
            tableau[0][maxfct.Length] = new Fraction(0);

            for(int i = 0; i < restrictions.Length; i++)
            {
                string[] restSpl = restrictions[i].Split(' ');
                tableau[i+1] = new Fraction[restSpl.Length];

                for (int j = 0; j < restSpl.Length; j++)
                {
                    if (restSpl[j].Contains("x"))
                    {
                        int xi = restSpl[j].IndexOf('x');
                        string xl = restSpl[j].Remove(xi);
                        if (xl.Contains("+"))
                            xl = xl.Replace("+", "");
                        if (xl.Trim().Equals(""))
                            xl = "1";
                        tableau[i+1][j] = Fraction.FromString(xl);
                    }
                    else if (restSpl[j].Contains("="))
                    {
                        string xl = restSpl[j].Replace("=", "");
                        tableau[i+1][j] = Fraction.FromString(xl);
                    }
                    else
                    {
                        tableau[i+1][j] = Fraction.FromString(restSpl[j]);
                    }
                }
            }

            //Fill Label Arrays
            labelX = new string[(tableau.Length + tableau[0].Length - 2)];
            labelY = new string[(tableau.Length + tableau[0].Length - 2)];
            for (int i = 0; i < (tableau.Length + tableau[0].Length - 2); i++)
            {
                labelX[i] = "x" + (i + 1);
                labelY[i] = "y" + (i + 1);
            }

            DrawTableau();
        }

        public void DrawTableau()
        {
            string toWrite = "";

            //First Row, Add x1...xj-1
            toWrite += "    |";
            for (int j = 0; j < tableau[0].Length; j++)
            {

                if (j < tableau[0].Length - 1)
                {
                    string CellContent = labelX[j];
                    CellContent = CellContent.PadRight(8);
                    toWrite += CellContent + "|";
                }
                else
                {
                    toWrite += "        \n";
                }
            }
            int twl = toWrite.Length - 2;
            for(int i = 0; i < twl; i++)
            {
                toWrite += "-";
            }
            toWrite += "\n";


            for (int i = 0; i < tableau.Length; i++)
            {
                if(i == 0)
                {
                    toWrite += "  z |";
                }
                else
                {
                    string CellContent2 = labelX[(tableau[0].Length - 2 + i)];
                    CellContent2 = CellContent2.PadLeft(4);
                    toWrite += CellContent2 + "|";
                }
                
                for (int j = 0; j < tableau[0].Length; j++)
                {
                    if(i == r && j == s)
                    {
                        string CellContent = ">" + tableau[i][j].ToString();
                        CellContent = CellContent.PadRight(8);
                        toWrite += CellContent + "|";
                    }
                    else
                    {
                        string CellContent = tableau[i][j].ToString();
                        CellContent = CellContent.PadRight(8);
                        toWrite += CellContent + "|";
                    }
                    

                    //Add Y Label
                    if(j == (tableau[0].Length - 1) && i > 0)
                    {
                        string CellContent2 = labelY[i - 1];
                        CellContent2 = CellContent2.PadLeft(4);
                        toWrite += CellContent2;
                    }
                }
                toWrite += "\n";

                for (int y = 0; y < twl; y++)
                {
                    toWrite += "-";
                }
                toWrite += "\n";

            }
            for (int j = 0; j < tableau[0].Length; j++)
            {
                if(j == 0)
                {
                    toWrite += "    |";
                }
                else
                {
                    string CellContent2 = labelY[tableau.Length - 2 + j];
                    CellContent2 = CellContent2.PadLeft(8);
                    toWrite += CellContent2 + "|";
                }
            }

            toWrite += "\n \n";
            toWrite += "Pivot Element at: (" + r + "," + s + ")";

            StepText.Text = toWrite;
        }

        public void SimplexStep()
        {
            DeepCopyTableau();

            ars = oldtableau[r][s];

            CalcPvtRowAndCol();

            for (int i = 0; i < tableau.Length; i++)
            {
                for (int j = 0; j < tableau[0].Length; j++)
                {
                    if (i != r && j != s)
                    {
                        tableau[i][j] = (oldtableau[i][j] - ((oldtableau[i][s] * oldtableau[r][j]) / ars)).Reduce();
                    }
                }
            }

            string tmp;
            tmp = labelX[s];
            labelX[s] = labelX[tableau[0].Length - 2 + r];
            labelX[tableau[0].Length - 2 + r] = tmp;

            tmp = labelY[r - 1];
            labelY[r - 1] = labelY[tableau.Length - 1 + s];
            labelY[tableau.Length - 1 + s] = tmp;

            DrawTableau();
        }

        public void CalcPvtRowAndCol()
        {
            for(int i = 0; i < tableau.Length; i++)
            {
                for(int j = 0; j < tableau[0].Length; j++)
                {
                    if(i == r && j == s)
                    {
                        tableau[i][j] = (1 / ars).Reduce();
                    }else if(i == r)
                    {
                        tableau[i][j] = (tableau[i][j] / ars).Reduce();
                    }
                    else if(j == s)
                    {
                        tableau[i][j] = (tableau[i][j] / (-1 * ars)).Reduce();
                    }
                }
            }
        }

        public void DeepCopyTableau()
        {
            oldtableau = new Fraction[tableau.Length][];
            for(int i = 0; i < tableau.Length; i++)
            {
                oldtableau[i] = new Fraction[tableau[0].Length];
                for(int j = 0; j < tableau[0].Length; j++)
                {
                    oldtableau[i][j] = new Fraction(tableau[i][j].Numerator, tableau[i][j].Denominator);
                }
            }
        }

        public void CalcFctValue()
        {

        }

        public void GetPivotPrimal()
        {
            int pvtcol = -1;
            Fraction Value = new Fraction(0);
            Fraction[] coll = tableau[0].Take(tableau[0].Length - 1).ToArray();
            
            for(int i = 0; i < coll.Length; i++)
            {
                if(coll[i] < Value)
                {
                    pvtcol = i;
                    Value = coll[i];
                }
            }

            s = pvtcol;
            if(pvtcol == -1)
            {
                r = -1;
                return;
            }

            Fraction bdivpvtcolprev = new Fraction(99999);
            int pvtrow = -2;
            for(int i = 1; i < tableau.Length; i++)
            {
                if(tableau[i][s] == 0)
                {
                    continue;
                }

                Fraction bdivpvtcol = tableau[i][tableau[0].Length - 1] / tableau[i][s];
                if(bdivpvtcol > 0)
                {
                    if(bdivpvtcol < bdivpvtcolprev)
                    {
                        pvtrow = i;
                        bdivpvtcolprev = bdivpvtcol;
                    }
                }
            }

            if(pvtrow != -2)
            {
                r = pvtrow;
            }
        }

        public void GetPivotDual()
        {
            int pvtrow = -1;
            Fraction Value = new Fraction(0);

            for (int i = 1; i < tableau.Length; i++)
            {
                if (tableau[i][tableau[0].Length - 1] == 0)
                {
                    continue;
                }

                if(tableau[i][tableau[0].Length - 1] < Value)
                {
                    pvtrow = i;
                    Value = tableau[i][tableau[0].Length - 1];
                }
            }

            if(pvtrow == -1)
            {
                r = -1;
                s = -1;
                return;
            }

            r = pvtrow;

            int pvtcol = -2;
            Value = new Fraction(-99999);

            for(int j = 0; j < tableau[0].Length - 1; j++)
            {
                if(tableau[r][j] != 0 && (tableau[0][j]/tableau[r][j]) > Value && (tableau[0][j] / tableau[r][j]) < 0)
                {
                    pvtcol = j;
                    Value = (tableau[0][j] / tableau[r][j]);
                }
            }
            s = pvtcol;

        }
    }
}
