using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace CalculatriceApp
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private Dictionary<string, object> _propertyValues = new Dictionary<string, object>();

        public T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            if (_propertyValues.ContainsKey(propertyName))
                return (T)_propertyValues[propertyName];
            return default(T);
        }
        public bool SetValue<T>(T newValue, [CallerMemberName] string propertyName = null)
        {
            var currentValue = GetValue<T>(propertyName);
            if (currentValue == null && newValue != null
             || currentValue != null && !currentValue.Equals(newValue))
            {
                _propertyValues[propertyName] = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        #endregion

        public char operation
        {
            get { return GetValue<char>(); }
            set { SetValue(value); }
        }

        public string EcranDeTravaille
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }


        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();
        }



        private void C_Click(object sender, RoutedEventArgs e)
        {
            EcranDeTravaille = "";
        }

        private void Operation_Click(object sender, RoutedEventArgs e)
        {
            Button ClickedButton = (Button)sender;
            operation = Convert.ToChar(ClickedButton.Content);
            EcranDeTravaille += operation;
        }

        private void Operande_Click(object sender, RoutedEventArgs e)
        {
            Button ClickedButton = (Button)sender;
            EcranDeTravaille += ClickedButton.Content;
        }

        private void Resultat_Click(object sender, RoutedEventArgs e)
        {
            EcranDeTravaille += " =" + Environment.NewLine + EnleveParentheses(EcranDeTravaille);
        }

        private void Point_Click(object sender, RoutedEventArgs e)
        {
            EcranDeTravaille += ".";
        }

        private void Add_Negatif_Click(object sender, RoutedEventArgs e)
        {
            EcranDeTravaille += " ";
        }

        private string EnleveParentheses(string text)
        {
            while (text.Contains('(') && text.Contains(')'))
            {
                int openIndex = 0;
                int closeIndex = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == '(')
                        openIndex = i;

                    if (text[i] == ')')
                    {
                        closeIndex = i;

                        text = text.Remove(openIndex, closeIndex - openIndex + 1).Insert(openIndex, ResoudreInterieurParentheses(openIndex, closeIndex, text));

                        break;
                    }
                }
            }
            for (int i = 1; i < text.Length; i++)
            {
                if (text[i] == '-' && (text[i - 1] == '*' || text[i - 1] == '/'))
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (text[j] == '+')
                        {
                            StringBuilder stringBuilder = new StringBuilder(text);
                            stringBuilder[j] = '-';
                            text = stringBuilder.ToString();
                            text = text.Remove(i, 1);
                            break;
                        }
                        else if (text[j] == '-')
                        {
                            StringBuilder stringBuilder = new StringBuilder(text);
                            stringBuilder[j] = '+';
                            text = stringBuilder.ToString();
                            text = text.Remove(i, 1);
                            break;
                        }
                    }
                }
            }

            for (int i = 1; i < text.Length; i++)
            {
                if (text[i] == '-' && (text[i - 1] == '-' || text[i - 1] == '+'))
                {
                    if (text[i - 1] == '-')
                    {
                        StringBuilder stringBuilder = new StringBuilder(text);
                        stringBuilder[i] = '+';
                        text = stringBuilder.ToString();
                        text = text.Remove(i - 1, 1);
                    }
                    else
                    {
                        StringBuilder stringBuilder = new StringBuilder(text);
                        stringBuilder[i] = '-';
                        text = stringBuilder.ToString();
                        text = text.Remove(i - 1, 1);
                    }

                }
                else if (text[i] == '+' && (text[i - 1] == '-' || text[i - 1] == '+'))
                {
                    if (text[i - 1] == '-')
                    {
                        StringBuilder stringBuilder = new StringBuilder(text);
                        stringBuilder[i] = '-';
                        text = stringBuilder.ToString();
                        text = text.Remove(i - 1, 1);
                    }
                    else
                    {
                        StringBuilder stringBuilder = new StringBuilder(text);
                        stringBuilder[i] = '+';
                        text = stringBuilder.ToString();
                        text = text.Remove(i - 1, 1);
                    }
                }
            }

            if (text[0] == '-')
                text = 0 + text;

            return Calculate(text);
        }

        private string ResoudreInterieurParentheses(int openIndex, int closeIndex, string textParentheses)
        {
            string resulatParentheses = Calculate(textParentheses.Substring(openIndex + 1, closeIndex - openIndex - 1));
            return resulatParentheses;
        }


        private string Calculate(string expressionACalculer)
        {
            double resultatFinal = AddSubstract(expressionACalculer);
            return resultatFinal.ToString();
        }

        private double AddSubstract(string expression)
        {
            string[] chaine = expression.Split('-');
            List<string> textList = new List<string>();

            for (int i = 0; i < chaine.Length; i++)
            {
                textList.Add(chaine[i]);
                if (i != chaine.Length - 1)
                {
                    textList.Add("-");
                }
            }

            for (int i = 0; i < textList.Count; i++)
            {
                if (textList[i].Contains('+') && textList[i].Length > 1)
                {
                    string[] textPart = textList[i].Split('+');
                    textList.RemoveAt(i);

                    for (int j = textPart.Length - 1; j >= 0; j--)
                    {
                        textList.Insert(i, textPart[j]);
                        if (j != 0)
                        {
                            textList.Insert(i, "+");
                        }
                    }
                }
            }

            double total;
            if (textList[0].Contains('*') || textList[0].Contains('/'))
            {
                total = DivideMultiply(textList[0]);
            }
            else
            {
                total = Convert.ToDouble(textList[0]);
            }
            for (int i = 2; i < textList.Count; i += 2)
            {
                if (textList[i - 1] == "-")
                {
                    total -= DivideMultiply(textList[i]);
                }
                else if (textList[i - 1] == "+")
                {
                    total += DivideMultiply(textList[i]);
                }
            }

            return total;
        }



        private double DivideMultiply(string expression)
        {

            string[] chaine = expression.Split('*');
            List<string> textList = new List<string>();

            for (int i = 0; i < chaine.Length; i++)
            {
                textList.Add(chaine[i]);
                if (i != chaine.Length - 1)
                {
                    textList.Add("*");
                }
            }

            for (int i = 0; i < textList.Count; i++)
            {
                if (textList[i].Contains('/') && textList[i].Length > 1)
                {
                    string[] textPart = textList[i].Split('/');
                    textList.RemoveAt(i);

                    for (int j = textPart.Length - 1; j >= 0; j--)
                    {
                        textList.Insert(i, textPart[j]);
                        if (j != 0)
                        {
                            textList.Insert(i, "/");
                        }
                    }
                }
            }

            double total = Convert.ToDouble(textList[0]);
            for (int i = 2; i < textList.Count; i += 2)
            {
                if (textList[i - 1] == "/")
                {
                    total = total / Convert.ToDouble(textList[i]);
                }
                else if (textList[i - 1] == "*")
                {
                    total = total * Convert.ToDouble(textList[i]);
                }
            }

            return total;
            
        }

        
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            //System.Console.WriteLine(">" + EcranDeTravaille);
            if (e.Key == Key.Enter)
            {
                EcranDeTravaille += " =" + Environment.NewLine + EnleveParentheses(EcranDeTravaille);
            }
        }
    }
}
