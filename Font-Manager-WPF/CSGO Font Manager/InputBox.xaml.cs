using System;
using System.Windows;

namespace CSGO_Font_Manager
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();
        }

        public void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(User_Input.Text))
            {
                MessageBox.Show("Textbox is empty?");
            }
            else
                MainWindow.csgoPath = User_Input.Text;
            this.Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.running = false;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            MainWindow.running = true;
        }
    }
}