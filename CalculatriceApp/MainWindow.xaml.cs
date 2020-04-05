using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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


        #region Events of buttons
        private void C_Click(object sender, RoutedEventArgs e)
        {
            EcranDeTravaille = "";
        }

        private void Delete_Last_Char_Click(object sender, RoutedEventArgs e)
        {
            EcranDeTravaille = EcranDeTravaille.Remove(EcranDeTravaille.Length - 1);
        }

        private void Operation_Click(object sender, RoutedEventArgs e)
        {
            Button ClickedButton = (Button)sender;
            EcranDeTravaille += Convert.ToChar(ClickedButton.Content);
        }

        private void Operande_Click(object sender, RoutedEventArgs e)
        {
            Button ClickedButton = (Button)sender;
            EcranDeTravaille += ClickedButton.Content;
        }

        private void Point_Click(object sender, RoutedEventArgs e)
        {
            EcranDeTravaille += ".";
        }

        private void Resultat_Click(object sender, RoutedEventArgs e)
        {
            if (CheckValidityTextEntered())
                EcranDeTravaille += " =" + Environment.NewLine + CalculateEcranDeTravaille(EcranDeTravaille);
        }

        #endregion

        //event of button Enter pressed
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (CheckValidityTextEntered())
                    EcranDeTravaille += " =" + Environment.NewLine + CalculateEcranDeTravaille(EcranDeTravaille);
            }
        }

        private Boolean CheckValidityTextEntered()
        {
            if (EcranDeTravaille != null)
            {
                if (Regex.IsMatch(EcranDeTravaille, @"^[0-9|\+|\-|\*|\/|\.|\(|\)]*$"))
                    return true;
                else
                {
                    MessageBox.Show("Veuillez saisir que les caractères correspondant aux boutons de l'interface SVP", "Erreur détectée dans le text saisi", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            else
                return false;

        }

        #region Algorithm of the calculator
        private string CalculateEcranDeTravaille(string text)
        {
            //contient le text des parentheses ? 
            while (text.Contains('(') && text.Contains(')'))
            {
                int openIndex = 0;
                int closeIndex = 0;

                //si oui cherche les index des parentheses 
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == '(')
                        openIndex = i;

                    if (text[i] == ')')
                    {
                        closeIndex = i;

                        //ajout du resultat du calcule de l'expression qui est a l'interieur des parentheses
                        //enlever les parentheses et leur contenu  
                        text = text.Remove(openIndex, closeIndex - openIndex + 1).Insert(openIndex, CalculateExpressionWithinParentheses(openIndex, closeIndex, text));

                        break;
                    }
                }
            }

            // en cas de d'une expression de type operande*-operande ou operande/-operande, on supprime le signe -
            // et on remplace le signe qui se place avant cette expression par son inverse s'il est + ou -
            // ex : +operande*-operande devient -operande*operande
            // sinon on ajoute - au tout debut du text
            // ex : operande*operande*-operande devient -operande*operande*operande
            for (int i = 1; i < text.Length; i++)
            {
                if (text[i] == '-' && (text[i - 1] == '*' || text[i - 1] == '/'))
                {
                    for (int j = i - 2; j >= 0; j--)
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
                        else if (j == 0)
                        {
                            text = text.Remove(i, 1);
                            text = '-' + text;
                        }
                    }
                }
            }

            // chercher -- ou +-
            for (int i = 1; i < text.Length; i++)
            {
                if (text[i] == '-' && (text[i - 1] == '-' || text[i - 1] == '+'))
                {
                    if (text[i - 1] == '-')
                    {
                        // remplacer -- en +
                        StringBuilder stringBuilder = new StringBuilder(text);
                        stringBuilder[i] = '+';
                        text = stringBuilder.ToString();
                        text = text.Remove(i - 1, 1);
                    }
                    else
                    {
                        // remplacer +- en -
                        text = text.Remove(i - 1, 1);
                    }

                }

            }

            if (text[0] == '-')
                text = 0 + text;

            return Calculate(text);
        }

        private string CalculateExpressionWithinParentheses(int openIndex, int closeIndex, string textParentheses)
        {
            return Calculate(textParentheses.Substring(openIndex + 1, closeIndex - openIndex - 1));
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

        #endregion
    }
}
