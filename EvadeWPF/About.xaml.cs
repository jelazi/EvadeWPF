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
using System.Windows.Threading;

namespace EvadeWPF
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        DispatcherTimer Timer = new DispatcherTimer(); //časovač pohybu kamenů při automatu
        double blackMoveLeftMax = 0;
        double blackMoveLeftMin = 0;
        bool right = true;
        
        private void TimerOn()
        {
            Timer.Tick += new EventHandler(Timer_Tick); // timer pro automat
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            blackMoveLeftMax = WhiteKing.Margin.Left;
            blackMoveLeftMin = BlackKing.Margin.Left;
              
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (right)
            {
                if (BlackKing.Margin.Left < blackMoveLeftMax)
                {
                    BlackKing.Margin = new Thickness(BlackKing.Margin.Left + 1, BlackKing.Margin.Top, BlackKing.Margin.Right - 1, BlackKing.Margin.Bottom);
                    WhiteKing.Margin = new Thickness(WhiteKing.Margin.Left - 1, WhiteKing.Margin.Top, WhiteKing.Margin.Right + 1, WhiteKing.Margin.Bottom);
                }
                else
                {
                    right = false;
                    Canvas.SetZIndex(WhiteKing, Canvas.GetZIndex(BlackKing) + 1);

                }
            }
            
            else

            {
                if (BlackKing.Margin.Left > blackMoveLeftMin)
                {
                    BlackKing.Margin = new Thickness(BlackKing.Margin.Left - 1, BlackKing.Margin.Top, BlackKing.Margin.Right + 1, BlackKing.Margin.Bottom);
                    WhiteKing.Margin = new Thickness(WhiteKing.Margin.Left + 1, WhiteKing.Margin.Top, WhiteKing.Margin.Right - 1, WhiteKing.Margin.Bottom);
                }
                else
                {
                    right = true;
                    Canvas.SetZIndex(BlackKing, Canvas.GetZIndex(WhiteKing) + 1);

                }
            }


        }


        public void PlayTimer()
        {
            this.Show();
            Timer.Start();
        }

        public About()
        {
            InitializeComponent();

            TimerOn();
            


        }




        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            this.Hide();
            e.Cancel = true;
            Timer.Stop();

        }
    }
}
