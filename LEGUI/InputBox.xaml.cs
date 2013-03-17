using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LEGUI
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();

            tbText.Focus();
        }

        public string Instruction
        {
            get { return lText.Content.ToString(); }
            set { lText.Content = value; }
        }

        public string Text
        {
            get { return tbText.Text; }
            set { tbText.Text = value; }
        }

        public string OkText
        {
            get { return bOk.Content.ToString(); }
            set { bOk.Content = value; }
        }

        public string CancelText
        {
            get { return bCancel.Content.ToString(); }
            set { bCancel.Content = value; }
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void tbText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                DialogResult = true;
            }
        }
    }
}
