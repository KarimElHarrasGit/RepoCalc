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
            {
                System.Globalization.CultureInfo frCult = System.Globalization.CultureInfo.CreateSpecificCulture("fr-FR");
                string formatedResult = string.Format(frCult, "{0:n}", double.Parse(CalculateEcranDeTravaille(EcranDeTravaille)));
                EcranDeTravaille += " =" + Environment.NewLine + formatedResult;
            }
        }

        #endregion

        //event of button Enter pressed
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (CheckValidityTextEntered())
                {
                    System.Globalization.CultureInfo frCult = System.Globalization.CultureInfo.CreateSpecificCulture("fr-FR");
                    string formatedResult = string.Format(frCult, "{0:n}", double.Parse(CalculateEcranDeTravaille(EcranDeTravaille)));
                    EcranDeTravaille += " =" + Environment.NewLine + formatedResult;
                }
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

                        //enleve les parentheses et leur contenu puis ajout du resultat du calcule de l'expression qui est a l'interieur des parentheses
                        text = text.Remove(openIndex, closeIndex - openIndex + 1).Insert(openIndex, CalculateExpressionWithinParentheses(openIndex, closeIndex, text));

                        break;
                    }
                }
            }

            // en cas de d'une expression de type operande*-operande ou operande/-operande, on supprime le signe -
            // puis on cherche s'il y a un signe - ou + qui precede cette expression, si c le cas on le remplace par son inverse : - devient + et vice versa
            // ex : +operande*-operande devient -operande*operande
            // sinon on ajoute - au tout debut de l'expression
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
            // on décompose expression dans la collection textList s'il contient -
            string[] chaine1 = expression.Split('-');
            List<string> textList = new List<string>();

            for (int i = 0; i < chaine1.Length; i++)
            {
                textList.Add(chaine1[i]);
                if (i != chaine1.Length - 1)
                {
                    textList.Add("-");
                }
            }

            for (int i = 0; i < textList.Count; i++)
            {
                // on décompose les valeurs de la collection textList s'il contient +
                if (textList[i].Contains('+') && textList[i].Length > 1)
                {
                    string[] chaine2 = textList[i].Split('+');
                    textList.RemoveAt(i);

                    for (int j = chaine2.Length - 1; j >= 0; j--)
                    {
                        textList.Insert(i, chaine2[j]);
                        if (j != 0)
                        {
                            textList.Insert(i, "+");
                        }
                    }
                }
            }

            double total;
            // si textList contient que des operations de type * ou /, on appele DivideMultiply
            if (textList[0].Contains('*') || textList[0].Contains('/'))
            {
                total = DivideMultiply(textList[0]);
            }
            else
            {
                total = Convert.ToDouble(textList[0]);
            }

            // on calcule le total en parcourant les operations - + puis les operandes contenu dans textList
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
            // on décompose expression dans la collection textList s'il contient *
            string[] chaine1 = expression.Split('*');
            List<string> textList = new List<string>();

            for (int i = 0; i < chaine1.Length; i++)
            {
                textList.Add(chaine1[i]);
                if (i != chaine1.Length - 1)
                {
                    textList.Add("*");
                }
            }

            for (int i = 0; i < textList.Count; i++)
            {
                if (textList[i].Contains('/') && textList[i].Length > 1)
                {
                    // on décompose les valeurs de la collection textList s'il contient /
                    string[] chaine2 = textList[i].Split('/');
                    textList.RemoveAt(i);

                    for (int j = chaine2.Length - 1; j >= 0; j--)
                    {
                        textList.Insert(i, chaine2[j]);
                        if (j != 0)
                        {
                            textList.Insert(i, "/");
                        }
                    }
                }
            }

            // on calcule le total en parcourant les operations / * puis les operandes contenu dans textList
            double total = Convert.ToDouble(textList[0]);
            for (int i = 2; i < textList.Count; i += 2)
            {
                if (textList[i - 1] == "/")
                {
                    total /= Convert.ToDouble(textList[i]);
                }
                else if (textList[i - 1] == "*")
                {
                    total *= Convert.ToDouble(textList[i]);
                }
            }

            return total;

        }

        #endregion
    }
}
