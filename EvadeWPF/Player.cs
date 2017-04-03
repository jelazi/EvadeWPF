using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeWPF
{


    class Player
    {
        /// <summary>
        /// hráč
        /// </summary>

        string color; // barva hráče
        int yposition; // startovní řádek hráče
        int[] positionSpawn = { 0, 1, 4, 5 }; // pozice pěšců na šachovnici
        int[] positionKing = { 2, 3 }; // pozice králů na šachovnici
     // int[] positionSpawn = { 4 }; // pozice pěšců na šachovnici
     // int[] positionKing = { 2,3 }; // pozice králů na šachovnici
        List<Stone> stones = new List<Stone>();  // pole všech kamenů
        bool autoPlayer = true; //je hráč automat či manual
        bool onTurn = false; //je hráč na tahu
        Random rnd = new Random();
        move bestPosition = new move(); //nejlepší pozice hráče
        bool ui; //"Umělá inteligence" ano ne
        int depthUI = 0; // hloubka umělé inteligence



        public Player(string color, int yposition)
        {
            this.color = color; // barva hráče
            this.yposition = yposition; // pozice na řádce y
        }


        public bool AutoPlayer //get set zda hráč je auto či manual
        {
            get { return autoPlayer; }
            set { autoPlayer = value; }
        }

        public int[] PositionSpawn//pozice pěšců
        {
            get { return positionSpawn; }
        }

        public int[] PositionKing//pozice králů
        {
            get { return positionKing; }
        }

        public int YPosition//pozice Y
        {
            get { return yposition; }
        }

        public int DepthUI //get set hloubka umělé inteligence
        {
            get { return depthUI; }
            set { depthUI = value; }
        }


        public string Color //get barva hráče
        {
            get { return color; }
        }

        public Stone AutoChoiceStone(Board board) //náhodné vybrání kamene
        {
            int x;
            x = rnd.Next(0, stones.Count);
            return (stones[x]);
        }

        public bool UI //get set UI
        {
            get { return ui; }
            set { ui = value; }
        }


        public void AutoMoveStone(Manager manager) //automatický přesun kamene jen dle náhody
        {
            List<pos> possiblePosition = new List<pos>();
            Stone choisenStone = new Stone();

            do
            {
                choisenStone = AutoChoiceStone(manager.Board);                        //automatické vybrání kamene
                possiblePosition = manager.Controlor.PossiblePosition(choisenStone, choisenStone.position, manager.Board); //vybrání všech možných pozic
            } while (possiblePosition.Count == 0);    //kontrola, zda vybraný kámen má nějakou možnou pozici

            manager.Board.MoveStone(choisenStone, manager.Board.ChoicePossibleMove(possiblePosition), manager);
        }


        public void AutoMoveStoneUI(Manager manager) // přesun kamene dle ohodnocení polí
        {
            bestPosition = manager.Controlor.BestMoveAlfaBeta(manager, depthUI);
            manager.Board.MoveStone(bestPosition, manager);
        }



        public void MarkStones(Board board) // označení všech možných kamenů
        {
            foreach (Stone stone in stones)
            {
                board.fields[stone.position.x - 1, stone.position.y - 1] = board.fields[stone.position.x - 1, stone.position.y - 1] | Field.onTurn;
            }
        }


        public bool OnTurn //get set je hráč na tahu?
        {
            get { return onTurn; }
            set { onTurn = value; }
        }


        public List<Stone> Stones //get seznam kamenů
        {
            get { return stones; }
            set { stones = value; }

        }

        public string GetColorPlayer()
        { return color; }
    }
}
