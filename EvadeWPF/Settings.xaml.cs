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
using System.Windows.Shapes;

namespace EvadeWPF
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    /// 

    public delegate void SettingsHandler(object sender, uint flag);
    public delegate void SpeedBoxHandler(int speed);
    public delegate void StartGameBoxHandler(bool StartGame);


    public partial class Settings : Window
    {

        /// <summary>
        /// okno nastavení ve hře
        /// </summary>

        public event SettingsHandler EventSend;
        public event SpeedBoxHandler SpeedBoxSend;
        public event StartGameBoxHandler StartGameBoxSend;

        public Settings()
        {
            InitializeComponent();
        }

        
        

        private void WhitePerson_Selected(object sender, RoutedEventArgs e)
        {
            DifficultWhite.Visibility = Visibility.Hidden;
            WhiteDif.Visibility = Visibility.Hidden;
        }



        private void WhiteComputer_Selected(object sender, RoutedEventArgs e)
        {
            DifficultWhite.Visibility = Visibility.Visible;
            DifficultWhite.IsEnabled = true;
            WhiteDif.Visibility = Visibility.Visible;
        }



        private void BlackPerson_Selected(object sender, RoutedEventArgs e)
        {
            DifficultBlack.Visibility = Visibility.Hidden;
            BlackDif.Visibility = Visibility.Hidden;
        }

        private void BlackComputer_Selected(object sender, RoutedEventArgs e)
        {
            DifficultBlack.Visibility = Visibility.Visible;
            DifficultBlack.IsEnabled = true;
            BlackDif.Visibility = Visibility.Visible;
        }


        private void Storno_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerWhite.SelectedItem == WhitePerson)
                EventSend(sender, 0x01);
            if (DifficultWhite.SelectedItem == DifNothingW)
                EventSend(sender, 0x02);
            if (PlayerWhite.SelectedItem == WhiteComputer)
                EventSend(sender, 0x03);
            if (DifficultWhite.SelectedItem == DifEasyW)
                EventSend(sender, 0x04);
            if (DifficultWhite.SelectedItem == DifMiddleW)
                EventSend(sender, 0x05);
            if (DifficultWhite.SelectedItem == DifHeavyW)
                EventSend(sender, 0x06);
            if (PlayerBlack.SelectedItem == BlackPerson)
                EventSend(sender, 0x07);
            if (PlayerBlack.SelectedItem == BlackComputer)
                EventSend(sender, 0x08);
            if (DifficultBlack.SelectedItem == DifNothingB)
                EventSend(sender, 0x09);
            if (DifficultBlack.SelectedItem == DifEasyB)
                EventSend(sender, 0x10);
            if (DifficultBlack.SelectedItem == DifMiddleB)
                EventSend(sender, 0x11);
            if (DifficultBlack.SelectedItem == DifHeavyB)
                EventSend(sender, 0x12);
            SpeedBoxSend(Int32.Parse(SpeedBox.Text));
            StartGameBoxSend(StartGameBox.IsChecked == true);
            this.Close();
        }

        public struct SettingsPlayer
        {
            public int TypePlayer;
            public int DepthPlayer;                
        }

        private void ChangeSettingsPlayers_Click(object sender, RoutedEventArgs e) // prohození hráčů
        {
            SettingsPlayer settingsWhitePlayer = new SettingsPlayer();
            SettingsPlayer settingsBlackPlayer = new SettingsPlayer();

            settingsWhitePlayer.TypePlayer = PlayerWhite.SelectedIndex;
            settingsWhitePlayer.DepthPlayer = DifficultWhite.SelectedIndex;
            settingsBlackPlayer.TypePlayer = PlayerBlack.SelectedIndex;
            settingsBlackPlayer.DepthPlayer = DifficultBlack.SelectedIndex;

            PlayerBlack.SelectedIndex = settingsWhitePlayer.TypePlayer;
            DifficultBlack.SelectedIndex = settingsWhitePlayer.DepthPlayer;
            PlayerWhite.SelectedIndex = settingsBlackPlayer.TypePlayer;
            DifficultWhite.SelectedIndex = settingsBlackPlayer.DepthPlayer;
        }
    }
}
