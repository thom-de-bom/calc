using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Data;



namespace CalculatorApp.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }    
}

namespace CalculatorApp
{
    public partial class MainWindow : Window
    {
        private decimal? lastResult = null;
        private bool isDecimal = false;
        private string equation = string.Empty;
        private bool newEquation = false;
        private string currentNumber = "";  // Initialize with empty string.
        private string selectedOperator = string.Empty;
        private decimal result = 0;  // Change result to decimal.
        private ObservableCollection<HistoryItem> history = new ObservableCollection<HistoryItem>();

        public class HistoryItem
        {
            public string Equation { get; set; }
            public string Result { get; set; }

            public override string ToString()
            {
                return $"{Equation} = {Result}";
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            historyListView.ItemsSource = history;
            deleteHistoryButton.Click += DeleteHistoryButton_Click;
            if (calc.Properties.Settings.Default.ShowHelpOnStartup)
            {
                HelpMessage();
            }
        }


        private void DeleteHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (history.Count > 0)
            {
                history.Clear();
                historyListView.SelectedItem = null;
            }
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D8 && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                OperatorButton_Click(multiplyButton, e);
            }
            else if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                string numKey = (e.Key >= Key.NumPad0) ? (e.Key - Key.NumPad0).ToString() : (e.Key - Key.D0).ToString();
                currentNumber += numKey;
                equation += numKey;
                displayTextBox.Text = equation;
            }
            else if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                OperatorButton_Click(plusButton, e);
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                OperatorButton_Click(minusButton, e);
            }
            else if (e.Key == Key.Multiply)
            {
                OperatorButton_Click(multiplyButton, e);
            }
            else if (e.Key == Key.Divide || e.Key == Key.OemQuestion)
            {
                OperatorButton_Click(divideButton, e);
            }
            else if (e.Key == Key.Enter)
            {
                EqualsButton_Click(null, null);
            }
            else if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                ClearButton_Click(null, null);
            }
            else if (e.Key == Key.Decimal || e.Key == Key.OemPeriod)
            {
                if (!currentNumber.Contains("."))
                {
                    currentNumber += ".";
                    equation += ".";
                    displayTextBox.Text = equation;
                }
            }
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string clickedNumber = button.Content.ToString();

            if (newEquation)
            {
                displayTextBox.Text = "";
                equation = "";
                newEquation = false;
            }

            currentNumber += clickedNumber;
            equation += clickedNumber;
            displayTextBox.Text = equation;

            this.Focus();  // move the focus back to the Window
        }

        private void OperatorButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string clickedOperator = button.Content.ToString();

            if (!string.IsNullOrEmpty(currentNumber))
            {
                decimal currentDecimal = decimal.Parse(currentNumber, CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(selectedOperator))
                {
                    PerformOperation(currentDecimal); // Perform operation with currentNumber and store to result
                }
                else
                {
                    result = currentDecimal;
                }

                currentNumber = "";
                selectedOperator = clickedOperator;
                equation += " " + selectedOperator + " ";
                displayTextBox.Text = equation;
            }
            else if (lastResult != null)
            {
                result = lastResult.Value;
                selectedOperator = clickedOperator;
                equation += " " + selectedOperator + " ";
                displayTextBox.Text = equation;
            }
        }


        private void PerformOperation(decimal operand)
        {
            switch (selectedOperator)
            {
                case "+":
                    result += operand;
                    break;
                case "-":
                    result -= operand;
                    break;
                case "*":
                    result *= operand;
                    break;
                case "/":
                    if (operand == 0)
                    {
                        MessageBox.Show("Cannot divide by zero.");
                        return;
                    }
                    result /= operand;
                    break;
                default:
                    MessageBox.Show("Invalid operation.");
                    break;
            }
        }


        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(equation))
            {
                if (!string.IsNullOrEmpty(currentNumber))
                {
                    PerformOperation(decimal.Parse(currentNumber, CultureInfo.InvariantCulture));
                }
                string resultString = result.ToString("F2");
                lastResult = result; // Save result to lastResult
                history.Add(new HistoryItem { Equation = equation, Result = resultString });
                equation = " = " + resultString;
                displayTextBox.Text = resultString; // Changed from 'equation' to 'resultString'
                currentNumber = "";
                equation = "";
                selectedOperator = ""; // Clear selectedOperator after calculation
            }
            else
            {
                displayTextBox.Text = "";
                equation = "";
            }
        }






        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            displayTextBox.Text = "";
            currentNumber = "0";
            selectedOperator = "";
            equation = "";
            result = 0;
            lastResult = null; // Clear lastResult
            isDecimal = false;
        }


        private void DecimalButton_Click(object sender, RoutedEventArgs e)
        {
            if (!currentNumber.Contains("."))
            {
                if (string.IsNullOrEmpty(currentNumber)) // Check if currentNumber is empty
                {
                    currentNumber = "0."; // If it is, append "0." instead of just "."
                }
                else
                {
                    currentNumber += ".";
                }

                equation += ".";
                displayTextBox.Text = equation;
            }
        }

        private void HistoryListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (historyListView.SelectedItem != null)
            {
                HistoryItem selectedHistory = (HistoryItem)historyListView.SelectedItem;
                Clipboard.SetText(selectedHistory.Result); // Copies the result to the clipboard
            }
            e.Handled = true; // This will stop the single click event from being called
        }

        private void HistoryListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (historyListView.SelectedItem != null)
            {
                HistoryItem selectedHistory = (HistoryItem)historyListView.SelectedItem;
                currentNumber = selectedHistory.Equation;
                equation = selectedHistory.Equation;
                displayTextBox.Text = equation;
            }
        }

        public static double Evaluate(string expression)
        {
            var stack = new Stack<double>();

            foreach (var token in expression.Split(' '))
            {
                double result;
                if (double.TryParse(token, out result))
                {
                    stack.Push(result);
                }
                else
                {
                    var right = stack.Pop();
                    var left = stack.Pop();

                    switch (token)
                    {
                        case "+": result = left + right; break;
                        case "-": result = left - right; break;
                        case "*": result = left * right; break;
                        case "/": result = left / right; break;
                        default: throw new ArgumentException("Unsupported operator: " + token);
                    }

                    stack.Push(result);
                }
            }

            return stack.Pop();
        }
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpMessage();
        }


        private void HelpMessage()
        {
            var msg = new MessageBoxResult();
            msg = MessageBox.Show("Welcome to the calculator application!\n\nHere's a quick guide to help you get started:\n\n" +
                                  "1. Click on the numbers and operations to form an equation, then hit the equals sign (=) or enter to get the result.\n" +
                                  "2. Use the Clear (C) button or backspace (←) to remove the current equation.\n" +
                                  "3. The history section shows all past equations and results. Single clicking an equation will place it in the current equation box, double clicking will copy the result to clipboard.\n\n" +
                                  "Don't show this message again?",
                                  "Help", MessageBoxButton.YesNoCancel);
            if (msg == MessageBoxResult.Yes)
            {
                calc.Properties.Settings.Default.ShowHelpOnStartup = false;
                calc.Properties.Settings.Default.Save();
            }
            else if (msg == MessageBoxResult.No)
            {
                calc.Properties.Settings.Default.ShowHelpOnStartup = true;
                calc.Properties.Settings.Default.Save();
            }
        }
    }
}