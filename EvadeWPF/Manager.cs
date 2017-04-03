using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeWPF
{
    [FlagsAttribute]
    public enum Field //pole s kameny a vlastnostmi kamenů
    {
        empty = 0,
        white = 1,
        black = 2,
        spawn = 4,
        king = 8,
        onTurn = 16,
        choice = 32,
        possible = 64,
        freeze = 128,
        best = 256
    }

    public struct pos //struktura pozice 
    {
        public int x;
        public int y;
    }


    struct Stone //struktura kámen
    {
        public Field properties;
        public pos position;
    }


    public struct move                                     //struktura jeden tah
    {
        public Field properties;                    //jméno hráče, typ kamene
        public pos beforePos;                       //předcházející pozice kamene
        public pos nextPos;                         //následující pozice kamene
    }


    class Manager                                           
        {

        /// <summary>
        /// řídící manažer programu
        /// </summary>

           Board board = new Board();                          //třída plocha
           Player black = new Player("black", 5);              //třída hráč - černý hráč
           Player white = new Player("white", 0);              //třída hráč - bílý hráč
           Controlor control = new Controlor();                //třída kontrol
           bool end;                                           //konec hry
           Player winner;                                      //vítěz
           string oneMoveText;                                 //jeden tah pro výpis tahu
           string historyText;                                 //historie tahů pro výpis tahů
           List<move> historyMove = new List<move>();          //historie tahů
           List<move> historyMoveBack = new List<move>();      //historie vrácených tahů


            public void StartGame()
            {            
                SetStones(white);
                SetStones(black);
                GetStonesToPlayers();            
            }
      

            public Controlor Controlor //get třída Control
            {
                get { return control; }
            }


            public Board Board //get hrací deska
            {
                get { return board; }
            }

            public bool End //get set konec
            {
                get { return end; }
                set { end = value; }
            }


            public void ChangeOnTurn() //změna hráče na tahu
            {
                if (white.OnTurn == true)
                {
                    white.OnTurn = false;
                    black.OnTurn = true;
                }
                else
                {
                    white.OnTurn = true;
                    black.OnTurn = false;
                }
            }


            public Player PlayerOnTurn //get hráč na tahu
            {
                get
                {
                    if (white.OnTurn == true && black.OnTurn == false)
                    {
                        return white;
                    }
                    else
                    {
                        return black;
                    }
                }

                set
                {
                    if (value == white)
                    {
                        white.OnTurn = true;
                        black.OnTurn = false;
                    }
                    if (value == black)
                    {
                        white.OnTurn = false;
                        black.OnTurn = true;
                    }
                }
            }

            public Player OpositePlayer(Player player) //protihráč
            {
                if (player == white)
                {
                    return black;
                }
                else
                {
                    return white;
                }
            }

            public List<move> HistoryMoveBack
            {
                get { return historyMoveBack; }
            set { historyMove = value; }
            }

            public string OneMoveText
            {
                get { return oneMoveText; }
                set { oneMoveText = value; }
            }

            public string HistoryText
            {
                get { return historyText; }
                set { historyText = value; }
            }

            public List<move> HistoryMove //seznam historie tahů

            {
                get { return historyMove; }
                set { historyMove = value; }
            }

            public Player Winner //vítěz
            {
                get { return winner; }
                set { winner = value; }
            }


            public void SetStones(Player player)
            {
                int index = 0;

                foreach (int i in player.PositionSpawn) // vytváření pěšců index 1, 2, 5, 6
                {
                    board.fields[i, player.YPosition] = board.SetColor(player.Color) | board.SetType("spawn");
                    Stone stone = new Stone();
                    stone.properties = board.SetColor(player.Color) | board.SetType("spawn");
                    stone.position.x = i + 1;
                    stone.position.y = player.YPosition + 1;
                    player.Stones.Add(stone); //zapsání do pole všech kamenů
                    index++;
                }

                foreach (int i in player.PositionKing) // vytváření králů index 3, 4
                {
                    board.fields[i, player.YPosition] = board.SetColor(player.Color) | board.SetType("king");
                    Stone stone = new Stone();
                    stone.properties = board.SetColor(player.Color) | board.SetType("king");
                    stone.position.x = i + 1;
                    stone.position.y = player.YPosition + 1;
                    player.Stones.Add(stone); //zapsání do pole všech kamnenů
                    index++;
                }
            }



            public void AddToOneMoveText(Field properties, pos beforePos, pos nextPos) //přidání tahu do historie
            {
                move move = new move();
                move.properties = properties;
                move.beforePos = beforePos;
                move.nextPos = nextPos;
                historyMove.Add(move);
            }

          

            public Player FindPlayerByName(move move, Manager manager) //najdi hráče podle jména
            {
                if ((move.properties & Field.white) == Field.white)
                {
                    return manager.White;
                }
                else
                {
                    return manager.Black;
                }
            }

 

            public void GetStonesToPlayers()  // aktualizuje kameny v seznamu hráčů
            {

                white.Stones.Clear();
                black.Stones.Clear();

                int index = 0;

                foreach (Field field in board.fields)
                {
                    int x = index / 6;
                    int y = index % 6;

                    if ((field & Field.black) == Field.black)
                    {
                        Stone st = new Stone();
                        st.properties = field;
                        st.position.x = x + 1;
                        st.position.y = y + 1;
                        black.Stones.Add(st);

                    }

                    if ((field & Field.white) == Field.white)
                    {
                        Stone st = new Stone();
                        st.properties = field;
                        st.position.x = x + 1;
                        st.position.y = y + 1;
                        white.Stones.Add(st);
                    }
                    index++;                
                }                
            }

            public void Redo() //tah znovu
            {
                if (historyMoveBack.Count > 0)
                {
                if (Controlor.ControlValidMove(historyMoveBack.Last(), this))
                {
                    Board.MoveStone(historyMoveBack.Last(), this);
                    historyMoveBack.RemoveAt(historyMoveBack.Count - 1);
                }
                else
                {
                    System.Windows.MessageBox.Show("Špatný tah: \n\n " + historyMoveBack.Last().properties.ToString() + ": " + historyMoveBack.Last ().beforePos.x.ToString() + ":" + historyMoveBack.Last().beforePos.y.ToString() + " ; " + historyMoveBack.Last().nextPos.x.ToString() + ":" + historyMoveBack.Last().nextPos.y.ToString());                    
                }                    
                }
            }

            public void ResetBoard() //resetování pole
            {
                board = new Board();
                white = new Player("white", 0);
                black = new Player("black", 5);
                end = false;

                historyMove.Clear();
                HistoryMoveBack.Clear();
                StartGame();
            }

            public string ChangeNameStoneToCzech(Field stone) //změň jméno kamene do češtiny
            {
                string newName = "bezejmenný: ";

                if ((stone & Field.king) == Field.king)
                {
                    newName = "král";
                }
                if ((stone & Field.spawn) == Field.spawn)
                {
                    newName = "pešec";
                }
                return newName;
            }

            public string ChangeColorPlayerToCzech(Field stone) //změn barvu hráče do češtiny
            {
                string newColor = "Bezbarvý ";
                if ((stone & Field.white) == Field.white)
                {
                    newColor = "Bílý ";
                }
                if ((stone & Field.black) == Field.black)

                {
                    newColor = "Černý ";
                }
                return newColor;
            }



            public Player White //get bílý hráč
            {
                get { return white; }
            }

            public Player Black //get černý hráč
            {
                get
                { return black; }
            }


            public void AutoMoveStone() //automatický přesun kamene
            {
                if (PlayerOnTurn.UI)
                { PlayerOnTurn.AutoMoveStoneUI(this); }
                else
                { PlayerOnTurn.AutoMoveStone(this); }
                board.EraseSelectStone(this);
            }

            public void MoveChoisenStone(pos selectField, Stone selectStone) //posun kamene po kliku
            {
                Board.MoveStone(selectStone, selectField, this); //přesun kamene                
                Controlor.UnmarkPossibleFields(Board);//odznačení předchozích možných pozic
            }

            public void ChooseStone(pos SelectField, Stone selectStone) //kliknutí na určité pole
            {
                board.MakeSelectField(SelectField, PlayerOnTurn, this);  //označení vybraného pole
                Board.Stone = selectStone;        //vybrání kamene dle označeného pole
                List<pos> possiblePosition = new List<pos>();
                possiblePosition = Controlor.PossiblePosition(selectStone, selectStone.position, Board);    //zapsání všech možných pozic
                Controlor.MarkPossibleField(Board, possiblePosition);  //označení všech možných pozic
            }
        }
    }

