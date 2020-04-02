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
            return Calculate(text);
        }

        private string ResoudreInterieurParentheses(int openIndex, int closeIndex, string textParentheses)
        {
            string resulatParentheses = Calculate(textParentheses.Substring(openIndex + 1, closeIndex - openIndex - 1));
            return resulatParentheses;
        }


        private string Calculate(string expressionACalculer)
        {
            double resultatFinal = Substract(expressionACalculer);
            return resultatFinal.ToString();
        }

        private double Substract(string expressionToSubstract)
        {
            string[] chaine = expressionToSubstract.Split('-');
            double total = Add(chaine[0]);
            for (int i = 1; i < chaine.Length; i++)
            {
                total -= Add(chaine[i]);
            }

            return total;
        }

        private double Add(string expressionToAdd)
        {
            string[] chaine = expressionToAdd.Split('+');
            double total = Multiply(chaine[0]);
            for (int i = 1; i < chaine.Length; i++)
            {
                total += Multiply(chaine[i]);
            }

            return total;
        }

        private double Multiply(string expressionToMultiply)
        {
            string[] chaine = expressionToMultiply.Split('*');
            double total = Devide(chaine[0]);
            for (int i = 1; i < chaine.Length; i++)
            {
                total *= Devide(chaine[i]);
            }

            return total;
        }

        private double Devide(string expressionToDevide)
        {
            string[] chaine = expressionToDevide.Split('/');
            double total = Convert.ToDouble(chaine[0]);
            for (int i = 1; i < chaine.Length; i++)
            {
                total /= Convert.ToDouble(chaine[i]);
            }

            return total;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {//pour prendre en compte les touches clavier
            //if (e.Key == Key.Enter)
            //{
            //    EcranDeTravaille += " =" + Environment.NewLine + EnleveParentheses(EcranDeTravaille);
            //}
        }
    }
}
