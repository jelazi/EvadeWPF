using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EvadeWPF
{
    public partial class MainWindow : Window
    {

        /// <summary>
        /// popisky ve hře
        /// </summary>

        int count = 0; // počítání tahů
        public enum GameState//stav hry
        {
            play,
            pause,
            stop,
            repeat
        }

        GameState gameState = new GameState();
        
    
        private void RunningGame() //popisky při běhu hry
        {
            CountNumber.Content = "Tah číslo: " + count.ToString();
            if (count == 0) CountNumber.Content = "Začátek hry";

            ChangeImagePlayerOn();


            if (!manager.PlayerOnTurn.AutoPlayer)
            {

                string pos;                
                string typeStone;
                string colorStone;                

                if (selectStone.properties == Field.empty) // label označení pole
                {
                    Informations.Content = "Žádné označené pole";
                }
                else
                {
                    pos = PositionToString(selectStone.position);
                    typeStone = manager.ChangeNameStoneToCzech(selectStone.properties);
                    colorStone = manager.ChangeColorPlayerToCzech(selectStone.properties);
                    Informations.Content = colorStone + typeStone + " " + "na poli " + pos; //label označené pole
                }
            }

            string turn = " není žádný ";
            string turn1 = "Nyní nikdo netáhne";
            if (manager.End)

            {
                turn = "už není žádný";
                string winner;
                if (manager.Winner != null)
                {
                    if (manager.Winner.Color == "black")
                    {
                        winner = "černý";
                    }
                    else
                    {
                        winner = "bílý";

                    }
                    Informations.Content = "Vítězem se stává " + winner + " hráč.";
                }
                else
                {
                    Informations.Content = "Tato hra skončila nerozhodně";
                }                
                gameState = GameState.stop;
                Informations.Background = Brushes.Red;
            }
            else
            {
                if (manager.PlayerOnTurn.Color == "white")
                { turn = " je bílý "; turn1 = "Táhne bílý"; }

                if (manager.PlayerOnTurn.Color == "black")
                { turn = " je černý "; turn1 = "Táhne černý"; }
            }
            command.Content = "Na tahu " + turn + " hráč.";
            OnTurnPlayer.Content = turn1 + ":";

            if (gameState == GameState.play)
            { SettingsButtonsPlay(); }
            if (gameState == GameState.pause)
            { SettingsButtonsPause(); }
            if (gameState == GameState.stop)
            { SettingsButtonsStop(); }
            if (gameState == GameState.repeat)
            { SettingsButtonsRepeat(); }            
        }

       

        private void ChangeImagePlayerOn() //změna labelu hráče na tahu
        {
            if (!manager.End)
            {
                if (manager.PlayerOnTurn == manager.White)
                {
                    BitmapImage ImgPlayerOn = new BitmapImage();
                    ImgPlayerOn.BeginInit();
                    ImgPlayerOn.UriSource = new Uri("whiteSpawn.png", UriKind.Relative);
                    ImgPlayerOn.EndInit();
                    ImagePlayerOn.Source = ImgPlayerOn;                    
                }

                if (manager.PlayerOnTurn == manager.Black)
                {
                    BitmapImage ImgPlayerOn = new BitmapImage();
                    ImgPlayerOn.BeginInit();
                    ImgPlayerOn.UriSource = new Uri("blackSpawn.png", UriKind.Relative);
                    ImgPlayerOn.EndInit();
                    ImagePlayerOn.Source = ImgPlayerOn;                   
                }
            }

            else
            {                
                ImagePlayerOn.Source = null;
            }
        }

        void CreateHistoryViewColumns(ListView listView) //nastavení sloupců historie
        {
            var gridView = new GridView();
            listView.View = gridView;

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Hráč",
                DisplayMemberBinding = new Binding("namePlayer"),
                Width = 50

            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Kámen",
                DisplayMemberBinding = new Binding("nameStone"),
                Width = 50
            });


            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Před",
                DisplayMemberBinding = new Binding("beforePos"),
                Width = 50
            });

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Po",
                DisplayMemberBinding = new Binding("nextPos"),
                Width = 50
            });
        }


        private void MovingByClickHistory()
        {
            if (gameState != GameState.play)
            {
                int actualHistory = 0;
                int index = 0;

                actualHistory = historyView.SelectedIndex;

                if (actualHistory <= manager.HistoryMove.Count)
                {
                    index = manager.HistoryMove.Count - actualHistory;

                    for (int i = 0; i < index; i++)
                    {
                        manager.Board.Back(manager);
                    }
                }
                else
                {
                    index = actualHistory - manager.HistoryMove.Count;

                    for (int i = 0; i < index; i++)
                    {
                        manager.Redo();
                    }
                }
                DrawStones();
            }
        }


        private void historyView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!TimerRepeat.IsEnabled)
            {
                MovingByClickHistory();
                count = manager.HistoryMove.Count;
                CountNumber.Content = "Tah číslo: " + count.ToString();
                if (count == 0) CountNumber.Content = "Začátek hry";
            }
        }


        private void historyView_PreviewKeyDown(object sender, KeyEventArgs e)
        {           
        }

        private void historyView_PreviewKeyUp(object sender, KeyEventArgs e)
        {         
        }


        private void HistoryToString()
        {
            historyView.Items.Add(AddMoveToFile(manager.HistoryMove.Last()));
            historyView.ScrollIntoView(historyView.Items[historyView.Items.Count - 1]);
            historyView.SelectedIndex = (historyView.Items.Count - 1);
        }
    }
}
