using System;
using System.Windows;
using System.Windows.Controls;

namespace LEGUI
{
    /// <summary>
    ///     Interaction logic for MaskTextBox.xaml
    /// </summary>
    public partial class MaskedTextBox : TextBox
    {
        private string _maskText = string.Empty;

        public MaskedTextBox()
        {
            InitializeComponent();
        }

        public string MaskText
        {
            get { return _maskText; }
            set
            {
                _maskText = value;

                if (String.IsNullOrEmpty(base.Text))
                {
                    base.Text = MaskText;
                    FontStyle = FontStyles.Italic;
                }
            }
        }

        public new string Text
        {
            get
            {
                if (base.Text == MaskText && FontStyle == FontStyles.Italic)
                {
                    return string.Empty;
                }
                return base.Text;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    base.Text = MaskText;
                    FontStyle = FontStyles.Italic;
                }
                else
                {
                    base.Text = value;
                    FontStyle = FontStyles.Normal;
                }
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(Text))
            {
                base.Text = MaskText;
                FontStyle = FontStyles.Italic;
            }

            base.OnLostFocus(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (base.Text == MaskText && FontStyle == FontStyles.Italic)
            {
                base.Text = string.Empty;
                FontStyle = FontStyles.Normal;
            }

            base.OnGotFocus(e);
        }
    }
}