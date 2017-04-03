using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace EvadeWPF
    
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// práce se soubory
        /// </summary>
        
        public moveToString trymove = new moveToString();
        public List<move> fileHistory = new List<move>();



        public enum NamePlayer
        {
            Start,
            Bílý,
            Černý            
        }

        public enum NameStone
        {
            hry,
            král,
            pěšec            
        }

        public class moveToString
        {
            public NamePlayer namePlayer {get; set;}
            public NameStone nameStone {get; set;}
            public string beforePos { get; set; }
            public string nextPos { get; set; }
        }

        public moveToString AddMoveToFile(move move) //přidání tahů do historie
        {
            moveToString moveToString = new moveToString();
            if ((move.properties & Field.white) == Field.white)
            {
                moveToString.namePlayer = NamePlayer.Bílý;
            }

            if ((move.properties & Field.black) == Field.black)
            {
                moveToString.namePlayer = NamePlayer.Černý;
            }

            if ((move.properties & Field.king) == Field.king)
            {
                moveToString.nameStone = NameStone.král;
            }

            if ((move.properties & Field.spawn) == Field.spawn)
            {
                moveToString.nameStone = NameStone.pěšec;
            }

            moveToString.beforePos = PositionToString(move.beforePos);
            moveToString.nextPos = PositionToString(move.nextPos);
            return moveToString;
        }

        public string PositionToString(pos pos) //pozice do textu
        {
            char xString = new char();

            switch (pos.x)
            {
                case 1:
                    xString = 'A';
                    break;
                case 2:
                    xString = 'B';
                    break;
                case 3:
                    xString = 'C';
                    break;
                case 4:
                    xString = 'D';
                    break;
                case 5:
                    xString = 'E';
                    break;
                case 6:
                    xString = 'F';
                    break;
            }
            return (xString + " : " + pos.y.ToString());
        }

        public class GameSettings // třída uložené hry
        {
            public GameState gameState = new GameState();
            public int tick;
            public bool end;
            public string winner;
            public Player white = new Player();
            public Player black = new Player();

            public struct Player
            {
                public bool autoPlayer;
                public int depthUI;
                public bool UI;
            }
            public List<move> historyMove = new List<move>();
            public List<move> historyMoveBack = new List<move>();
        }


        public void SaveSettingsGame(object sender, RoutedEventArgs e)
        {            
            if (fileName == "")
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Soubory xml|*.xml";
                saveFileDialog.Title = "Uložit hru";
                saveFileDialog.ShowDialog();
                fileName = saveFileDialog.FileName;
            }

            if (fileName != "")
            {
                GameSettings gameSettings = new GameSettings();
                gameSettings.gameState = gameState;
                gameSettings.tick = tick;
                gameSettings.end = manager.End;
                if (manager.End)
                {
                    if (manager.Winner == manager.White)
                    { gameSettings.winner = "white"; }
                    else
                    {
                        if (manager.Winner == manager.Black)
                        { gameSettings.winner = "black"; }
                        else
                        { gameSettings.winner = "nobody"; }
                    }
                }
                gameSettings.white.autoPlayer = manager.White.AutoPlayer;
                gameSettings.white.depthUI = manager.White.DepthUI;
                gameSettings.white.UI = manager.White.UI;
                gameSettings.black.autoPlayer = manager.Black.AutoPlayer;
                gameSettings.black.depthUI = manager.Black.DepthUI;
                gameSettings.black.UI = manager.Black.UI;

                gameSettings.historyMove = manager.HistoryMove;
                gameSettings.historyMoveBack = manager.HistoryMoveBack;

                XmlSerializer serializer = new XmlSerializer(gameSettings.GetType());
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    serializer.Serialize(sw, gameSettings);
                }
            }            
        }
    }
}
