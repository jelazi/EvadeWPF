using System;
using System.Windows;
using System.Windows.Threading;

namespace EvadeWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Manager manager = new Manager(); // manager programu - logická část
        
        About about = new About();
        DispatcherTimer Timer = new DispatcherTimer(); //časovač pohybu kamenů při automatu
        const int sizeField = 80; // velikost pole
        const int sizeSpace = 4; // mezera mezi poli
        const float ratio = 0.8f; // poměr velikosti kamenů vůči poli
        int tick; //rychlost autohry      
        int CanvasZindex;


        public MainWindow()
        {
            tick = 400; //rychlost autohry
            InitializeComponent();
            CreateHistoryViewColumns(historyView); //nastavení sloupců historie
            SettingsPlayersOnStart(); //nastavení hráčů na startu
            RunSettingsOnStart(); //spouštění nastavení při startu hry
            manager.StartGame(); //uložení kamenů na pole a k hráčům
            DrawFields(); //vykreslení pole            

            historyView.Items.Add(trymove);
            historyView.ScrollIntoView(historyView.Items[historyView.Items.Count - 1]);
            historyView.SelectedIndex = (historyView.Items.Count - 1);

            TimerOn();
        }       

        void SettingsPlayersOnStart() //nastavení hráčů na startu
        {
            manager.PlayerOnTurn = manager.White;
            manager.White.AutoPlayer = true;
            manager.Black.AutoPlayer = false;
            manager.White.UI = false;
            manager.Black.UI = false;
            manager.White.DepthUI = 0;
            manager.Black.DepthUI = 0;
            manager.PlayerOnTurn.MarkStones(manager.Board);
        }

        void RunSettingsOnStart() //spouštění nastavení při startu hry
        {
            Settings settings = new Settings();
            settings.EventSend += new SettingsHandler(ChangeSettings);
            settings.SpeedBoxSend += new SpeedBoxHandler(ChangeSpeed);
            settings.StartGameBoxSend += new StartGameBoxHandler(RunStart);
            settings.Topmost = true;
            SettingsByManager(settings);
            settings.Show();
        }

        void ChangeSettings(object sender, uint flag) //událost změny v settings
        {
            switch (flag)
            {
                case 0x01: //změna bílého hráče na člověk
                    manager.White.AutoPlayer = false;
                    break;
                case 0x02://bílý hráč obtížnost žádná
                    manager.White.UI = false;
                    manager.White.DepthUI = 0;
                    break;
                case 0x03: //změna bílého hráče na počítač
                    manager.White.AutoPlayer = true;
                    break;
                case 0x04: //bílý hráč obtížnost lehká
                    manager.White.UI = true;
                    manager.White.DepthUI = 1;
                    break;
                case 0x05: //bílý hráč obtížnost střední
                    manager.White.UI = true;
                    manager.White.DepthUI = 3;
                    break;
                case 0x06: //bílý hráč obtížnost těžká
                    manager.White.UI = true;
                     manager.White.DepthUI = 5;
                    break;
                case 0x07: //změna černého hráče na člověk
                    manager.Black.AutoPlayer = false;
                    break;
                case 0x08: //změna černého hráče na počítač
                    manager.Black.AutoPlayer = true;
                    break;
                case 0x09: //černý hráč obtížnost žádná
                    manager.Black.UI = false;
                    manager.Black.DepthUI = 0;
                    break;
                case 0x10: //černý hráč obtížnost lehká
                    manager.Black.UI = true;
                    manager.Black.DepthUI = 1;
                    break;
                case 0x11: //černý hráč obtížnost střední
                    manager.Black.UI = true;
                    manager.Black.DepthUI = 3;
                    break;
                case 0x12: //černý hráč obtížnost těžká
                    manager.Black.UI = true;
                    manager.Black.DepthUI = 5;
                    break;
            }
            DrawStones();
        }

        public void ChangeSpeed(int speed) //změna rychlosti automat
        {
            tick = speed;
            Timer.Interval = new TimeSpan(0, 0, 0, 0, tick);
        }

        public void RunStart(bool runstart) 
        {
            if (runstart)
            {
                gameState = GameState.play;
                Timer.Start();
            }
            else
            {
                gameState = GameState.pause;
                Timer.Stop();
            }                        
        }

        public void SettingsByManager(Settings settings) //visual settings dle aktuálního stavu
        {
            switch (manager.White.AutoPlayer)
            {
                case true:
                    settings.PlayerWhite.SelectedItem = settings.WhiteComputer;
                    break;
                case false:
                    settings.PlayerWhite.SelectedItem = settings.WhitePerson;
                    settings.DifficultWhite.IsEnabled = false;
                    break;
            }

            switch (manager.Black.AutoPlayer)
            {
                case true:
                    settings.PlayerBlack.SelectedItem = settings.BlackComputer;
                    break;
                case false:
                    settings.PlayerBlack.SelectedItem = settings.BlackPerson;
                    settings.DifficultBlack.IsEnabled = false;
                    break;
            }


            if (manager.White.UI)
            {
                switch (manager.White.DepthUI)
                {
                    case 1:
                        settings.DifficultWhite.SelectedItem = settings.DifEasyW;
                        break;
                    case 3:
                        settings.DifficultWhite.SelectedItem = settings.DifMiddleW;
                        break;
                    case 5:
                        settings.DifficultWhite.SelectedItem = settings.DifHeavyW;
                        break;
                }
                
            }
            else
            {
                settings.DifficultWhite.SelectedItem = settings.DifNothingW;
            }

            if (manager.Black.UI)
            {
                switch (manager.Black.DepthUI)
                {
                    case 1:
                        settings.DifficultBlack.SelectedItem = settings.DifEasyB;
                        break;
                    case 3:
                        settings.DifficultBlack.SelectedItem = settings.DifMiddleB;
                        break;
                    case 5:
                        settings.DifficultBlack.SelectedItem = settings.DifHeavyB;
                        break;
                }
            }
            else
            {
                settings.DifficultBlack.SelectedItem = settings.DifNothingB;
            }

            settings.SpeedBox.Text = tick.ToString();

            if (gameState == GameState.play)
            { settings.StartGameBox.IsChecked = true; }
            else
            { settings.StartGameBox.IsChecked = false; }             
        }


        public void TimerOn () //timery
        {
            Timer.Tick += new EventHandler(Timer_Tick); // timer pro automat
            Timer.Interval = new TimeSpan(0, 0, 0, 0, tick);
            TimerRepeat.Tick += new EventHandler(TimerRepeat_Tick); // timer pro repeat
            TimerRepeat.Interval = new TimeSpan(0, 0, 0, 0, 500);
            BestMoveTimer.Tick += new EventHandler(Timer_BestMove);
            BestMoveTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            gameState = GameState.play;
            manager.PlayerOnTurn.MarkStones(manager.Board);
            DrawStones();
            
            if (manager.PlayerOnTurn.AutoPlayer == false) //je hráč na tahu automat?
            {                                               //hráč je manual
                ButtonBestMoveOn();
                Timer.Stop();
                count = manager.HistoryMove.Count;
                RunningGame();
            }
            else
            {                                                 //hráč je automat
                if (gameState == GameState.play)
                {
                    ButtonBestMoveOff();
                }
                else
                { ButtonBestMoveOn(); }

                Timer.Start();
                count++;
                manager.AutoMoveStone();
                manager.Controlor.IsEnd(manager, manager.PlayerOnTurn);
                HistoryToString();
                manager.ChangeOnTurn();
                manager.PlayerOnTurn.MarkStones(manager.Board);
                RunningGame();
                DrawStones();
            }

            if (manager.End == true)
            {
                Timer.Stop();
            }
        }
    }
}

