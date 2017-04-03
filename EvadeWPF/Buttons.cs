using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Xml.Serialization;
using System.IO;

namespace EvadeWPF
{
    public partial class MainWindow : Window
    {

        /// <summary>
        /// tlačítka ve hře
        /// </summary>

        System.Windows.Media.Effects.DropShadowEffect dropshadow = new System.Windows.Media.Effects.DropShadowEffect();
        public string fileName = ""; //jméno uloženého souboru
        public static RoutedCommand MyCommand = new RoutedCommand(); //klávesové zkratky

        private void MenuOpen(object sender, RoutedEventArgs e)
        {
            bool pause = false;
            if (gameState == GameState.play)
            {
                MenuPause(sender, e);
                pause = true;
            }
            OpenSettingsGame(sender, e, pause);
            fileName = ""; //jméno souboru k uložení (rozdělení na save a save as)
        }
        



        public void OpenSettingsGame(object sender, RoutedEventArgs e, bool pause) //otevření hry
        {
            
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Soubory xml|*.xml";
            openFileDialog.Title = "Otevřít uloženou hru";            
           System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                GameSettings gameSettings = new GameSettings();
                try
                {
                    XmlSerializer serializer = new XmlSerializer(gameSettings.GetType());
                    using (StreamReader sr = new StreamReader(openFileDialog.FileName))
                    {
                        gameSettings = (GameSettings)serializer.Deserialize(sr);
                    }
                    
                    Timer.Stop();
                    gameState = GameState.play;
                    count = 0;
                    historyView.Items.Clear();
                    manager.ResetBoard();
                    SettingsPlayersOnStart();
                    manager.StartGame();

                    historyView.Items.Add(trymove);
                    historyView.ScrollIntoView(historyView.Items[historyView.Items.Count - 1]);
                    historyView.SelectedIndex = (historyView.Items.Count - 1);
                    Informations.Background = System.Windows.Media.Brushes.LightGray;
                  
                    manager.White.AutoPlayer = gameSettings.white.autoPlayer;
                    manager.White.DepthUI = gameSettings.white.depthUI;
                    manager.White.UI = gameSettings.white.UI;
                    manager.Black.AutoPlayer = gameSettings.black.autoPlayer;
                    manager.Black.DepthUI = gameSettings.black.depthUI;
                    manager.Black.UI = gameSettings.black.UI;
                    
                    foreach (move move in gameSettings.historyMove)
                    {
                        if (manager.Controlor.ControlValidMove(move, manager))
                        {
                            manager.Board.MoveStone(move, manager);
                            HistoryToString();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Špatný datový soubor\n" + "Nesprávná hodnota v kameni:\n " + manager.ChangeColorPlayerToCzech(move.properties) + manager.ChangeNameStoneToCzech(move.properties) + " z " + PositionToString(move.beforePos) + " do " + PositionToString(move.nextPos));
                            return;
                        }                        
                    }

                    if (gameSettings.historyMoveBack.Count != 0)
                    {
                        foreach (move move in gameSettings.historyMoveBack)
                        {
                            if (manager.Controlor.ControlValidMove(move, manager))
                            {
                                manager.Board.MoveStone(move, manager);
                                HistoryToString();
                            }
                            else
                            {                                
                                System.Windows.MessageBox.Show("Špatný datový soubor\n" + "Nesprávná hodnota v kameni:\n " + manager.ChangeColorPlayerToCzech(move.properties) + manager.ChangeNameStoneToCzech(move.properties) + " z " + PositionToString(move.beforePos) + " do " + PositionToString(move.nextPos));
                                return;
                            }
                        }
                    }

                    gameState = gameSettings.gameState;
                    tick = gameSettings.tick;
                    manager.End = gameSettings.end;
                    if (gameSettings.end)
                    {
                        if (gameSettings.winner == "white")
                        { manager.Winner = manager.White; }
                        else
                        {
                            if (gameSettings.winner == "black")
                            { manager.Winner = manager.Black; }
                            else
                            { manager.Winner = null; }
                        }
                    }
                    DrawFields();
                    DrawStones();
                    SettingsButtonsPause();
                    RunningGame();                    
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Špatný datový soubor: \n\n" + ex.ToString());
                    MenuOpen(sender, e);
                }
            }
            if (pause && result != System.Windows.Forms.DialogResult.OK)
            { MenuPlay(sender, e); }
        }

       

        private void MenuSave(object sender, RoutedEventArgs e)
        {            
            if (gameState == GameState.play)
            {
                MenuPause(sender, e);                
            }

            SaveSettingsGame(sender, e);            
        }

        private void MenuSaveAs(object sender, RoutedEventArgs e)
        {
            fileName = "";
            
            if (gameState == GameState.play)
            {
                MenuPause(sender, e);               
            }
            SaveSettingsGame(sender, e);
        }

        private void MenuClose(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MenuPlay(object sender, RoutedEventArgs e)
        {
            if (gameState == GameState.repeat)
            {
                SettingsButtonsRepeat();
                ButtonPauseOn();
                TimerRepeat.Start();
            }
            else
            {
                if (!manager.End)
                {
                    ButtonPauseOn();
                    if (manager.HistoryMoveBack.Count > 0)
                    {
                        int index = manager.HistoryMoveBack.Count;
                        try
                        {
                            manager.PlayerOnTurn = (manager.OpositePlayer(manager.FindPlayerByName(manager.HistoryMove.Last(), manager)));
                        }
                        catch { manager.PlayerOnTurn = manager.White; }
                        for (int i = 0; i < index; i++)
                        {
                            historyView.Items.RemoveAt(historyView.Items.Count - 1);
                        }
                        manager.HistoryMoveBack.Clear();
                    }
                    Timer.Start();
                    gameState = GameState.play;
                }
                RunningGame();
            }            
        }

        private void MenuPause(object sender, RoutedEventArgs e)
        {
            if (gameState == GameState.repeat) //při opakování hry
            {
                TimerRepeat.Stop();
                                    
                if (movingRepeat.Count == 0)
                {
                    gameState = GameState.stop;
                }
                else
                {
                    gameState = GameState.repeat;
                    ButtonPlayOn();
                    ButtonPauseOff();
                }                
            }
            else //normální pause
            {
                Timer.Stop();
                manager.Board.EraseSelectStone(manager);
                manager.Board.EraseSelectField(manager);
                selectStone = new Stone();
                DrawStones();
                ImagePlayerOn.Source = null;
                command.Content = "Hra je pozastavena";
                OnTurnPlayer.Content = "Hra je pozastavena";
                gameState = GameState.pause;
                RunningGame();
            }
        }

        private void MenuStop(object sender, RoutedEventArgs e)
        {
            if (gameState == GameState.repeat) // při opakování hry
            {
                TimerRepeat.Stop();
                if (movingRepeat.Count > 0)
                {
                    for (int i = 0; i < movingRepeat.Count;)
                    {
                        manager.Board.MoveStone(movingRepeat.First(), manager);
                        movingRepeat.RemoveAt(0);
                        HistoryToString();
                    }

                    DrawFields();
                    DrawStones();
                }

                RunningGame();
                count = manager.HistoryMove.Count;
                CountNumber.Content = "Tah číslo: " + count.ToString();
                if (count == 0) CountNumber.Content = "Začátek hry";
                manager.End = true;
                RunningGame();
                command.Background = System.Windows.Media.Brushes.LightGray;
                OnTurnPlayer.Background = System.Windows.Media.Brushes.White;
                gameState = GameState.stop;
            }
            else //normální stop
            {
                Timer.Stop();
                manager.End = true;
                manager.Controlor.IsEnd(manager, manager.PlayerOnTurn);
                manager.Board.EraseSelectStone(manager);
                manager.Board.EraseSelectField(manager);
                selectStone = new Stone();
                DrawStones();
                ImagePlayerOn.Source = null;
                command.Content = "Hra je zastavena";
                OnTurnPlayer.Content = "Hra je zastavena";
                gameState = GameState.stop;
                RunningGame();
            }            
        }

        private void MenuNewGame(object sender, RoutedEventArgs e) //menu nová hra
        {            
            Timer.Stop();
            gameState = GameState.play;
            count = 0;
            historyView.Items.Clear();
            manager.ResetBoard();
            SettingsPlayersOnStart();
            RunSettingsOnStart();
            manager.StartGame();

            DrawFields();
            DrawStones();

            historyView.Items.Add(trymove);
            historyView.ScrollIntoView(historyView.Items[historyView.Items.Count - 1]);
            historyView.SelectedIndex = (historyView.Items.Count - 1);
            Informations.Background = System.Windows.Media.Brushes.LightGray;
            Informations.Content = "";
            fileName = "";
            RunningGame();
        }

        private void MenuBack(object sender, RoutedEventArgs e) //menu zpět
        {
            if (gameState != GameState.play)
            {
                manager.Board.Back(manager);
                DrawStones();
                count = manager.HistoryMove.Count;
                CountNumber.Content = "Tah číslo: " + count.ToString();
                if (count == 0) CountNumber.Content = "Začátek hry";
                historyView.SelectedIndex = (manager.HistoryMove.Count);
            }
        }

        private void MenuRedo(object sender, RoutedEventArgs e) // menu tah znovu
        {
            if (gameState != GameState.play)
            {
                manager.Redo();
                DrawStones();
                count = manager.HistoryMove.Count;
                CountNumber.Content = "Tah číslo: " + count.ToString();
                if (count == 0) CountNumber.Content = "Začátek hry";
                historyView.SelectedIndex = (manager.HistoryMove.Count);
            }
        }


        DispatcherTimer TimerRepeat = new DispatcherTimer(); //časovač opakované hry
        List<move> movingRepeat = new List<move>(); //seznam tahů opakované hry


        private void MenuRepeatGame(object sender, RoutedEventArgs e) //menu opakovat hru
        {
            if (!TimerRepeat.IsEnabled && manager.End)
            {
                if (manager.HistoryMove.Count > 0)
                {
                    foreach (move move in manager.HistoryMove)
                    {
                        movingRepeat.Add(move);
                    }

                    if (manager.HistoryMoveBack.Count > 0)
                    {
                        manager.HistoryMoveBack.Reverse();
                        foreach (move move in manager.HistoryMoveBack)
                        {
                            movingRepeat.Add(move);
                        }
                        manager.HistoryMoveBack.Reverse();
                    }
                }

                Timer.Stop();
                count = 0;
                historyView.Items.Clear();
                manager.ResetBoard();
                manager.StartGame();

                DrawFields();
                DrawStones();

                historyView.Items.Add(trymove);
                historyView.ScrollIntoView(historyView.Items[historyView.Items.Count - 1]);
                historyView.SelectedIndex = (historyView.Items.Count - 1);
                Informations.Background = System.Windows.Media.Brushes.LightGreen;
                Informations.Content = "";
                command.Content = "Opakování hry";
                command.Background = System.Windows.Media.Brushes.LightGreen;
                OnTurnPlayer.Content = "Opakování hry";
                OnTurnPlayer.Background = System.Windows.Media.Brushes.LightGreen;

                TimerRepeat.Start();
                gameState = GameState.repeat;
                RunningGame();
            }
        }


        private void TimerRepeat_Tick(object sender, EventArgs e) //tick pro opakování hry
        {
            try
            {                
                manager.Board.MoveStone(movingRepeat.First(), manager);
                manager.ChangeOnTurn();
                movingRepeat.RemoveAt(0);
                HistoryToString();

                DrawFields();
                DrawStones();
                
                count = manager.HistoryMove.Count;
                CountNumber.Content = "Tah číslo: " + count.ToString();
                if (count == 0) CountNumber.Content = "Začátek hry";

                if (movingRepeat.Count == 0)
                {
                    TimerRepeat.Stop();
                    manager.End = true;
                    RunningGame();
                    command.Background = System.Windows.Media.Brushes.LightGray;
                    OnTurnPlayer.Background = System.Windows.Media.Brushes.White;
                    gameState = GameState.stop;
                }
                RunningGame();
            }
            catch { }           
        }

        DispatcherTimer BestMoveTimer = new DispatcherTimer(); //časovač pro probliknutí nejlepší volby
        move findBestMove = new move();
        int countBlick = 0;

        private void Timer_BestMove(object sender, EventArgs e) //tick pro probliknutí nejlepší volby
        {
            manager.Board.fields[findBestMove.beforePos.x - 1, findBestMove.beforePos.y - 1] = manager.Board.fields[findBestMove.beforePos.x - 1, findBestMove.beforePos.y - 1] | Field.best;
            Informations.Content = "Nejlepší tah: " + manager.ChangeColorPlayerToCzech(findBestMove.properties) + manager.ChangeNameStoneToCzech(findBestMove.properties) + " z " + PositionToString(findBestMove.beforePos) + " na " + PositionToString(findBestMove.nextPos);

            if (countBlick % 2 == 1) //probliknutí
            {
                manager.Board.fields[findBestMove.nextPos.x - 1, findBestMove.nextPos.y - 1] &= ~Field.best;
            }
            else
            {
                manager.Board.fields[findBestMove.nextPos.x - 1, findBestMove.nextPos.y - 1] = manager.Board.fields[findBestMove.nextPos.x - 1, findBestMove.nextPos.y - 1] | Field.best;
            }
            countBlick++;
            DrawStones();
            if (countBlick >= 10)
            {
                BestMoveTimer.Stop();
                manager.Board.fields[findBestMove.beforePos.x - 1, findBestMove.beforePos.y - 1] &= ~Field.best;
                manager.Board.fields[findBestMove.nextPos.x - 1, findBestMove.nextPos.y - 1] &= ~Field.best;
                DrawStones();
                Informations.Background = System.Windows.Media.Brushes.LightGray;
                Informations.Content = "";
                countBlick = 0;
            }
        }


        private void MenuBestMove(object sender, RoutedEventArgs e) //menu nejlepší tah
        {            
            int depth = 5;
            countBlick = 0;
            Informations.Content = "Přemýšlím";
            Informations.Background = System.Windows.Media.Brushes.Orange;
            manager.Controlor.UnmarkPossibleFields(manager.Board);
            findBestMove = manager.Controlor.BestMoveAlfaBeta(manager, depth);            
            BestMoveTimer.Start();
        }

        private void MenuSettings(object sender, RoutedEventArgs e) //menu nastavení
        {
            Timer.Stop();
            Settings settings = new Settings();
            settings.Topmost = true;
            settings.Show();
            SettingsByManager(settings);            //nastavení podle skutečnosti
            settings.EventSend += new SettingsHandler(ChangeSettings);
            settings.SpeedBoxSend += new SpeedBoxHandler(ChangeSpeed);
            settings.StartGameBoxSend += new StartGameBoxHandler(RunStart);
        }

        private void MenuHelp(object sender, RoutedEventArgs e) //menu nápověda
        {
            System.Windows.Forms.Help.ShowHelp(null, System.Windows.Forms.Application.StartupPath + @"\Evade.chm");
        }

        private void MenuAbout(object sender, RoutedEventArgs e) //menu o hře
        {
            about.PlayTimer();            
        }

        protected override void OnClosed(EventArgs e) //zavření všech inicializovaných oken při zavření hlavního okna
        {
            base.OnClosed(e);
            System.Windows.Application.Current.Shutdown();
        }

        private void NewGameButton(object sender, MouseButtonEventArgs e) //tlačítko Nová hra
        {

            NewGame1.Effect = null;
            NewGame1.IsEnabled = true;
            MenuNewGame(sender, e);
        }

        private void ButtonOpen(object sender, MouseButtonEventArgs e)//tlačítko otevřít hru
        {
            OpenGame.Effect = null;
            OpenGame.IsEnabled = true;
            MenuOpen(sender, e);
        }

        private void ButtonSettings(object sender, MouseButtonEventArgs e)//tlačítko nastavení
        {
            Settings1.Effect = null;
            Settings1.IsEnabled = true;
            MenuSettings(sender, e);
        }

        private void ButtonRedo(object sender, MouseButtonEventArgs e)//tlačítko tah znovu
        {
            Redo1.Effect = null;
            Redo1.IsEnabled = true;
            MenuRedo(sender, e);
        }

        private void ButtonBack(object sender, MouseButtonEventArgs e)//tlačítko tah zpět
        {
            Back1.Effect = null;
            Back1.IsEnabled = true;
            MenuBack(sender, e);
        }

        private void Repeat_MouseDown(object sender, MouseButtonEventArgs e)//tlačítko opakovat
        {
            Repeat.Effect = null;
            Repeat.IsEnabled = true;
            MenuRepeatGame(sender, e);
        }

        private void ButtonStop(object sender, MouseButtonEventArgs e)//tlačítko stop
        {
            StopGame.Effect = null;
            StopGame.IsEnabled = true;
            MenuStop(sender, e);
            ButtonPlayOff();
        }

        private void ButtonPause(object sender, MouseButtonEventArgs e)//tlačítko pozastavit hru
        {
            PauseGame.Effect = null;
            PauseGame.IsEnabled = true;
            MenuPause(sender, e);
            ButtonPlayOn();
        }

        private void ButtonPlay(object sender, MouseButtonEventArgs e)//tlačítko hrát hru
        {
            PlayGame.Effect = null;
            PlayGame.IsEnabled = true;                        
        }

        private void ButtonSave(object sender, MouseButtonEventArgs e)//tlačítko uložit hru
        {
            SaveGame.Effect = null;            
            MenuSave(sender, e);
        }

        private void ButtonHelp(object sender, MouseButtonEventArgs e)//tlačítko nápověda
        {
            Help2.Effect = null;
            MenuHelp(sender, e);
        }


        private void ButtonBestMove1(object sender, System.Windows.Input.MouseButtonEventArgs e) //tlačítko nejlepší tah
        {
            BestMove1.Effect = null;
            MenuBestMove(sender, e);
        }

        private BitmapImage ImageButton(string image) //vytváření obrázku tlačítek
        {
            BitmapImage Bitmap = new BitmapImage();
            Bitmap.BeginInit();
            Bitmap.UriSource = new Uri(image, UriKind.RelativeOrAbsolute);
            Bitmap.EndInit();
            return Bitmap;
        }


        private void PlayGame_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MenuPlay(sender, e);
            PlayGame.Effect = null;            
        }

        private void PlayGame_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (PlayGame.IsEnabled)
            { PlayGame.Effect = dropshadow; }                     
        }

        private void NewGame1_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NewGame1.Effect = dropshadow;
        }

        private void NewGame1_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            NewGame1.Effect = dropshadow;
        }

        private void OpenGame_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenGame.Effect = dropshadow;
        }

        private void OpenGame_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            OpenGame.Effect = dropshadow;
        }

        private void SaveGame_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SaveGame.Effect = dropshadow;
        }

        private void SaveGame_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SaveGame.Effect = dropshadow;
        }

        private void PauseGame_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (PauseGame.IsEnabled)
            { PauseGame.Effect = dropshadow; }                
        }

        private void PauseGame_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (PauseGame.IsEnabled)
            {
                PauseGame.Effect = dropshadow;
                ButtonPlayOn();
            }
        }

        private void StopGame_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
           if (StopGame.IsEnabled)
            {StopGame.Effect = dropshadow; }                
        }

        private void StopGame_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            if (StopGame.IsEnabled)
            { StopGame.Effect = dropshadow; }
        }

        private void Back1_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Back1.Effect = dropshadow;
        }

        private void Back1_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Back1.Effect = dropshadow;
        }

        private void Redo1_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Redo1.Effect = dropshadow;
        }

        private void Redo1_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Redo1.Effect = dropshadow;
        }        

        private void Repeat_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Repeat.Effect = dropshadow;            
        }        

        private void Repeat_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Repeat.Effect = dropshadow;
        }

        private void BestMove1_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BestMove1.Effect = dropshadow;
        }

        private void BestMove1_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BestMove1.Effect = dropshadow;
        }

        private void Settings1_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Settings1.Effect = dropshadow;
        }

        private void Settings1_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Settings1.Effect = dropshadow;
        }

        private void Help2_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Help2.Effect = dropshadow;
        }

        private void Help2_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Help2.Effect = dropshadow;
        }


        private void ButtonPlayOn() //zapnout tlačítko hrát hru
        {
            PlayGame.Effect = dropshadow;
            PlayGame.Source = ImageButton("Button-Play-icon.png");
            PlayGame.IsEnabled = true;
            Play.IsEnabled = true;
        }

        private void ButtonPlayOff() //vypnout tlačítko hrát hru
        {
            PlayGame.Effect = null;
            PlayGame.Source = ImageButton("Button-Play-icon-gray.png");
            PlayGame.IsEnabled = false;
            Play.IsEnabled = false;
        }

        private void ButtonPauseOn() //zapnout tlačítko pozastavit hru
        {
            PauseGame.Effect = dropshadow;
            PauseGame.Source = ImageButton("Button-Pause-icon.png");
            PauseGame.IsEnabled = true;
            Pause.IsEnabled = true;
        }

        private void ButtonPauseOff()  //vypnout tlačítko pozastavit hru
        {
            PauseGame.Effect = null;
            PauseGame.Source = ImageButton("Button-Pause-icon-gray.png");
            PauseGame.IsEnabled = false;
            Pause.IsEnabled = false;            
        }

        private void ButtonStopOn() //zapnout tlačítko zastavit hru
        {
            StopGame.Effect = dropshadow;
            StopGame.Source = ImageButton("Button-Stop-icon.png");
            StopGame.IsEnabled = true;
            Stop.IsEnabled = true;
        }

        private void ButtonStopOff() //vypnout tlačítko zastavit hru
        {
            StopGame.Effect = null;
            StopGame.Source = ImageButton("Button-Stop-gray-icon.png");
            StopGame.IsEnabled = false;
            Stop.IsEnabled = false;
        }

        private void ButtonRepeatOn() //zapnout tlačítko opakovat hru

        {
            Repeat.Effect = dropshadow;
            Repeat.Source = ImageButton("Button-Reload-icon.png");
            Repeat.IsEnabled = true;
            RepeatGame.IsEnabled = true;
        }


        private void ButtonRepeatOff() //vypnout tlačítko opakovat hru
        {
            Repeat.Effect = null;
            Repeat.Source = ImageButton("Button-Reload-icon-gray.png");
            Repeat.IsEnabled = false;
            RepeatGame.IsEnabled = false;
        }

        private void ButtonBackOn() //zapnout tlačítko tah zpět
        {
            Back1.Effect = dropshadow;
            Back1.Source = ImageButton("Button-Previous-icon.png");
            Back1.IsEnabled = true;
            Back.IsEnabled = true;
        }

        private void ButtonBackOff() //vypnout tlačítko tah zpět
        {
            Back1.Effect = null;
            Back1.Source = ImageButton("Button-Previous-icon-gray.png");
            Back1.IsEnabled = false;
            Back.IsEnabled = false;
        }


        private void ButtonRedoOn() //zapnout tlačítko tah znovu
        {
            Redo1.Effect = dropshadow;
            Redo1.Source = ImageButton("Button-Next-icon.png");
            Redo1.IsEnabled = true;
            Redo.IsEnabled = true;
        }

        private void ButtonRedoOff()  //vypnout tlačítko tah znovu
        {
            Redo1.Effect = null;
            Redo1.Source = ImageButton("Button-Next-icon-gray.png");
            Redo1.IsEnabled = false;
            Redo.IsEnabled = false;
        }


        private void ButtonBestMoveOn() //zapnout tlačítko nejlepší tah
        {
            BestMove1.Effect = dropshadow;
            BestMove1.Source = ImageButton("Think-icon.png");
            BestMove1.IsEnabled = true;
            BestMove.IsEnabled = true;
        }

        private void ButtonBestMoveOff()  //vypnout tlačítko nejlepší tah
        {
            BestMove1.Effect = null;
            BestMove1.Source = ImageButton("Think-icon-gray.png");
            BestMove1.IsEnabled = false;
            BestMove.IsEnabled = false;
        }

        private void ButtonNewGameOn() //zapnout tlačítko nová hra
        {
            NewGame1.Effect = dropshadow;
            NewGame1.Source = ImageButton("Actions-document-new-icon.png");
            NewGame1.IsEnabled = true;
            NewGame.IsEnabled = true; 
        }

        private void ButtonNewGameOff() //vypnout tlačítko nová hra
        {
           NewGame1.Effect = null;
           NewGame1.Source = ImageButton("Actions-document-new-icon-gray.png");
           NewGame1.IsEnabled = false;
           NewGame.IsEnabled = false;
        }


        private void ButtonOpenGameOn() //zapnout tlačítko otevřít hru
        {
            OpenGame.Effect = dropshadow;
            OpenGame.Source = ImageButton("Folder-Open-icon.png");
            OpenGame.IsEnabled = true;
            Open.IsEnabled = true;
        }


        private void ButtonOpenGameOff()  //vypnout tlačítko otevřít hru
        {
            OpenGame.Effect = null;
            OpenGame.Source = ImageButton("Folder-Open-icon-gray.png");
            OpenGame.IsEnabled = false;
            Open.IsEnabled = false;
        }


        private void ButtonSaveGameOn() //zapnout tlačítko uložit hru
        {
            SaveGame.Effect = dropshadow;
            SaveGame.Source = ImageButton("Save-icon.png");
            SaveGame.IsEnabled = true;
            Save.IsEnabled = true;
            SaveAs.IsEnabled = true;
        }

        private void ButtonSaveGameOff() //vypnout tlačítko uložit hru
        {
            SaveGame.Effect = null;
            SaveGame.Source = ImageButton("Save-icon-gray.png");
            SaveGame.IsEnabled = false;
            Save.IsEnabled = false;
            SaveAs.IsEnabled = false; 
        }

        private void ButtonSettingsOn() //zapnout tlačítko nastavení hry
        {
            Settings1.Effect = dropshadow;
            Settings1.Source = ImageButton("settings-icon.png");
            Settings1.IsEnabled = true;
            Settings.IsEnabled = true;            
        }

        private void ButtonSettingsOff() //vypnout tlačítko nastavení hry
        { 
            Settings1.Effect = null;
            Settings1.Source = ImageButton("settings-icon-gray.png");
            Settings1.IsEnabled = false;
            Settings.IsEnabled = false;            
        }

        public void SettingsButtonsPlay () //nastavení tlačítek při hraní hry
        {            
            ButtonPlayOff();
            ButtonPauseOn();
            ButtonStopOn();
            ButtonRepeatOff();
            ButtonBackOff();
            ButtonRedoOff();
            ButtonNewGameOn();
            ButtonOpenGameOn();
            ButtonSaveGameOn();
            ButtonSettingsOn();
            ButtonBestMoveOn();
        }


        public void SettingsButtonsPause() //nastavení tlačítek při pozastavení hry
        {
            ButtonPlayOn();
            ButtonPauseOff();
            ButtonStopOn();
            ButtonRepeatOff();
            ButtonBackOn();
            ButtonRedoOn();
            ButtonNewGameOn();
            ButtonOpenGameOn();
            ButtonSaveGameOn();
            ButtonSettingsOn();
            ButtonBestMoveOff();
            command.Content = "Hra je pozastavena";
            OnTurnPlayer.Content = "Hra je pozastavena";
        }

        public void SettingsButtonsStop() //nastavení tlačítek při zastavení hry
        {
            ButtonPlayOff();
            ButtonPauseOff();
            ButtonStopOff();
            ButtonRepeatOn();
            ButtonBackOn();
            ButtonRedoOn();
            ButtonNewGameOn();
            ButtonOpenGameOn();
            ButtonSaveGameOn();
            ButtonSettingsOn();
            ButtonBestMoveOff();
            command.Content = "Hra je zastavena";
            OnTurnPlayer.Content = "Hra je zastavena";
        }

        public void SettingsButtonsRepeat() //nastavení tlačítek při opakování hry
        {
            ButtonPlayOff();
            ButtonPauseOn();
            ButtonStopOn();
            ButtonRepeatOff();
            ButtonBackOff();
            ButtonRedoOff();
            ButtonBestMoveOff();
            ButtonNewGameOff();
            ButtonOpenGameOff();
            ButtonSaveGameOff();
            ButtonSettingsOff();
        }

        

        private void Hra_Evade_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) //klávesové zkratky
        {
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (NewGame.IsEnabled)
                {
                    MenuNewGame(sender, e);
                }
            }


            if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (OpenGame.IsEnabled)
                {
                   MenuOpen(sender, e);
                }
            }

            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (SaveGame.IsEnabled)
                {
                    MenuSave(sender, e);
                }
            }

            if (e.Key == Key.P && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (Play.IsEnabled)
                {
                   MenuPlay(sender, e);
                    return;
                }

                if (Pause.IsEnabled)
                {
                    MenuPause(sender, e);
                }
            }

            if (e.Key == Key.T && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (Stop.IsEnabled)
                {
                    MenuStop(sender, e);
                }
            }


            if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (Back.IsEnabled)
                {
                   MenuBack(sender, e);
                }
            }

            if (e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (Redo.IsEnabled)
                {
                   MenuRedo(sender, e);
                }               
            }

            if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (Repeat.IsEnabled)
                {
                    MenuRepeatGame(sender, e);
                }
            }

            if (e.Key == Key.B && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (BestMove.IsEnabled)
                {
                    MenuBestMove(sender, e);
                }
            }

            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (Settings.IsEnabled)
                {
                    MenuSettings(sender, e);
                }
            }

            if (e.Key == Key.F1)
            {                
                MenuHelp(sender, e);                
            }
        }
    }
}
