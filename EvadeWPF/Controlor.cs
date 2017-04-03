using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeWPF
{



    class Controlor
    {

        /// <summary>
        /// kontrola ve hře
        /// </summary>

        Random rnd = new Random();
        const int MAX = 4000;                              //maximální hodnota pole
        const int HIGH = 400;                             //vysoká hodnota pole


        public List<pos> PossiblePosition(Stone stone, pos position, Board board) //najde všechny možné pozice - s ohledem na kraj a své kameny
        {
            List<pos> possiblePosition = new List<pos>();

            if (!((stone.properties & Field.freeze) == Field.freeze))
            {
                int i = 0;
                for (int j = -1; j < 2; j++)
                {
                    for (int k = -1; k < 2; k++)
                    {
                        pos pos;
                        pos.x = k + position.x;
                        pos.y = j + position.y;

                        if (!(
                            pos.x < 1 || //mimo pole
                            pos.y < 1 || //mimo pole
                            pos.x > 6 || // mimo pole
                            pos.y > 6 || // mimo pole
                            ((board.ContainStone(board.fields[(pos.x - 1), (pos.y - 1)]))
                            && (board.GetStoneColor(board.fields[(pos.x - 1), (pos.y - 1)]) == board.GetStoneColor(stone.properties))) ||// kámen stejného hráče v sousedním poli
                            (board.fields[(pos.x - 1), (pos.y - 1)] & Field.freeze) == Field.freeze || // zamrzlé pole
                            (stone.properties & Field.king) == Field.king && (board.ContainStone(board.fields[(pos.x - 1), (pos.y - 1)])))  // král nesmí brát kámen                          
                            )
                        {
                            possiblePosition.Add(pos);
                        }
                        i++;
                    }
                }
            }
            return possiblePosition;
        }

        public List<pos> NextField(Stone stone, pos position, Manager manager) //najde všechny možné pozice - s ohledem na kraj bez ohledu na  ostatní kameny
        {
            List<pos> nextField = new List<pos>();

            if (!((stone.properties & Field.freeze) == Field.freeze))
            {
                int i = 0;
                for (int j = -1; j < 2; j++)
                {
                    for (int k = -1; k < 2; k++)
                    {
                        pos pos;
                        pos.x = k + position.x;
                        pos.y = j + position.y;

                        if (!(
                            pos.x < 1 || //mimo pole
                            pos.y < 1 || //mimo pole
                            pos.x > 6 || // mimo pole
                            pos.y > 6 ||   // mimo pole
                            (pos.x == stone.position.x && pos.y == stone.position.y)  //stejné pole jako kámen
                            ))
                        {
                            nextField.Add(pos);
                        }
                        i++;
                    }
                }
            }
            return nextField;
        }

        public bool ControlValidMove (move move, Manager manager) //kontrola správnosti tahu
        {
            
            if (move.properties == manager.Board.fields[move.beforePos.x -1, move.beforePos.y - 1]) //kontrola existence prvku
            {
                
                List<pos> possibleMove = new List<pos>();
                Stone stone = new Stone();
                stone.position = move.beforePos;
                stone.properties = move.properties;
                pos NextPosition = move.nextPos;

                possibleMove = PossiblePosition(stone, move.beforePos, manager.Board);

            if (possibleMove.Contains(move.nextPos)) //kontrola možnosti tahu
                {
                    return true;
                }
            else
                { return false; }
            }
            else
            { return false; }                        
        }

        public void IsEnd(Manager manager, Player player) //kontrola skončení hry
        {
            if (player.Stones.Count == 0) //hráči mají všechny kameny zamrzlé
            {
                manager.End = true;
            }
           
            List<Stone> kings = new List<Stone>();

            foreach (Stone stone in player.Stones)
            {
                if ((stone.properties & Field.king) == Field.king)
                { kings.Add(stone); }
            }

            foreach (Stone stone in manager.OpositePlayer(player).Stones)
            {
                if ((stone.properties & Field.king) == Field.king)
                { kings.Add(stone); }
            }

            foreach (Stone stone in kings)
            {
                if ((stone.properties & Field.white) == Field.white && stone.position.y == 6) //král v konečné pozici
                {
                    manager.End = true;
                    manager.Winner = manager.White;
                }
                if ((stone.properties & Field.black) == Field.black && stone.position.y == 1) //král v konečné pozici
                {
                    manager.End = true;
                    manager.Winner = manager.Black;
                }
            }

            int draw = 0;
            foreach (Stone stone in manager.Board.FreezeStones)
            {
                if ((stone.properties & Field.king) == Field.king)
                { draw++; }
            }
            if (draw == 4) { manager.End = true; } //všechny zamrzlé krále - nerozhodně
        }

        public bool IsPossibleMove(pos posMove, Stone selectStone, Manager manager)  //vrátí bool jestli je možný tah
        {
            List<pos> possiblePosition = new List<pos>();
            possiblePosition = PossiblePosition(selectStone, selectStone.position, manager.Board);
            return possiblePosition.Contains(posMove);
        }


        public void MarkPossibleField(Board board, List<pos> possiblePosition) //označení možných dalších tahů do šachovnice
        {
            try
            {
                foreach (pos pos in possiblePosition)
                {
                    board.fields[pos.x - 1, pos.y - 1] = Field.possible | board.fields[pos.x - 1, pos.y - 1];
                }
            }
            catch { }
        }

        public void UnmarkPossibleFields(Board board) //odznačení možných dalších tahů do šachovnice
        {
            for (int i = 0; i < board.fields.Length; i++)
            {
                int x = i / 6;
                int y = i % 6;

                if ((board.fields[x, y] & Field.possible) == Field.possible)
                {
                    board.fields[x, y] &= ~Field.possible;
                }
                if ((board.fields[x, y] & Field.best) == Field.best)
                {
                    board.fields[x, y] &= ~Field.best;
                }
            }
        }


        public List<int> GetRandomFieldsStone(List<Stone> stones) //vrátí list x čísel náhodně seřazených (pro náhodné vybrání kamenu jednoho hráče
        {
            List<int> origField = new List<int>(stones.Count);
            List<int> rndField = new List<int>(stones.Count);

            for (int i = 0; i < stones.Count; i++) //naplnení listu
            {
                origField.Add(i);
            }
            while (origField.Count > 0)
            {
                int index = rnd.Next(origField.Count);
                rndField.Add(origField[index]);
                origField.RemoveAt(index);
            }
            return rndField;
        }


        public List<int> GetRandomListMove(List<move> moves) //vrátí list x čísel náhodně seřazených (pro náhodné vybrání posunů jednoho hráče
        {
            List<int> origField = new List<int>(moves.Count);
            List<int> rndField = new List<int>(moves.Count);

            for (int i = 0; i < moves.Count; i++) //naplnení listu
            {
                origField.Add(i);
            }
            while (origField.Count > 0)
            {
                int index = rnd.Next(origField.Count);
                rndField.Add(origField[index]);
                origField.RemoveAt(index);
            }
            return rndField;
        }



        public List<move> GeneratorMove(Board board, Player player) //generuje všechny možné tahy jednoho hráče
        {
            List<move> moving = new List<move>();
            List<int> RndStone = GetRandomFieldsStone(player.Stones);        //náhodně seřadí kameny hráče

            foreach (int numberStone in RndStone)
            {
                Stone stone = player.Stones[numberStone];
                List<pos> possiblePosition = PossiblePosition(stone, stone.position, board);
                foreach (pos position in possiblePosition)
                {
                    move move = new move();
                    move.properties = stone.properties;
                    move.beforePos = stone.position;
                    move.nextPos = position;
                    moving.Add(move);
                }
            }

            List<int> RndMove = GetRandomListMove(moving); //náhodné seřazení vše move
            List<move> RndMoving = new List<move>();

            foreach (int numberMove in RndMove) // 
            {
                RndMoving.Add(moving[numberMove]);
            }
            return RndMoving;
        }


        private int Further(int value)
        {
            if (value > HIGH)
            {
                return value + 1;
            }
            if (value < -HIGH)
            {
                return value - 1;
            }
            return value;
        }

        private int Closer(int value)
        {
            if (value > HIGH)
            {
                return value - 1;
            }
            if (value < -HIGH)
            {
                return value + 1;
            }
            return value;
        }

        public int AlfaBeta (Manager ManagerAlfaBeta, int depth, int alfa, int beta)
        {
            int value = 0; //ohodnocení pozice

            IsEnd(ManagerAlfaBeta, ManagerAlfaBeta.PlayerOnTurn);

            if (ManagerAlfaBeta.End)
            {
                if (ManagerAlfaBeta.Winner == null)
                {
                    value = 0;
                    ManagerAlfaBeta.End = false;
                    return value;
                }

                if (ManagerAlfaBeta.Winner.Color == ManagerAlfaBeta.PlayerOnTurn.Color)
                {
                    value = MAX;
                    ManagerAlfaBeta.End = false;                    
                    return value;
                }

                if (ManagerAlfaBeta.Winner.Color != ManagerAlfaBeta.PlayerOnTurn.Color)
                {
                    value = -MAX;
                    ManagerAlfaBeta.End = false;
                    return value;
                }
            }


            if (depth == 0)
            {
                value = Evaluate(ManagerAlfaBeta); //ohodnocení pole pokud hloubka je 0
                return value;
            }

            List<move> moves = new List<move>();
            
            moves = GeneratorMove(ManagerAlfaBeta.Board, ManagerAlfaBeta.PlayerOnTurn); //generování všech možných tahů

            foreach (move move in moves)
            {
                ManagerAlfaBeta.Board.MoveStone(move, ManagerAlfaBeta); //zahrání tahu 
                ManagerAlfaBeta.ChangeOnTurn();
                value = -AlfaBeta(ManagerAlfaBeta, depth - 1, Further(-beta), Further(-alfa));
                value = Closer(value);
                if (value > alfa)
                {
                    alfa = value;
                    if (value >= beta)
                    {
                        ManagerAlfaBeta.Board.Back(ManagerAlfaBeta);
                        return beta;
                    }
                }
                ManagerAlfaBeta.Board.Back(ManagerAlfaBeta);                
            }
            return alfa;       
        }


        public move BestMoveAlfaBeta(Manager manager, int depth)
        {
            Manager ManagerAlfaBeta = new Manager();                 //vytvoření nového manažeru pouze pro minimax
            ManagerAlfaBeta.PlayerOnTurn = ManagerAlfaBeta.White;
            ManagerAlfaBeta.StartGame();                             //start manažeru
            IList<move> moving = new List<move>();                  //seznam všech předchozích tahů
            moving = manager.HistoryMove.AsReadOnly();
            ManagerAlfaBeta.Board.MoveStone(moving, ManagerAlfaBeta); //zahrání všech předchozích tahů   

            if (manager.PlayerOnTurn == manager.White)
            {
                ManagerAlfaBeta.PlayerOnTurn = ManagerAlfaBeta.White;
            }
            if (manager.PlayerOnTurn == manager.Black)
            {
                ManagerAlfaBeta.PlayerOnTurn = ManagerAlfaBeta.Black;
            }
            

            int value = 0;
            move bestMove = new move();
            List<move> moves = new List<move>();
            moves = GeneratorMove(ManagerAlfaBeta.Board, ManagerAlfaBeta.PlayerOnTurn); //generování všech možných tahů

            int alpha = -MAX;

            foreach (move move in moves) //rychlý konec hry
            {
                ManagerAlfaBeta.Board.MoveStone(move, ManagerAlfaBeta); //zahrání tahu 
                
                if (Evaluate(ManagerAlfaBeta) >= MAX) //nejbližší tah výhra
                { return move; }
                ManagerAlfaBeta.Board.Back(ManagerAlfaBeta);
                
            }

            foreach (move move in moves)
            {
                ManagerAlfaBeta.Board.MoveStone(move, ManagerAlfaBeta); //zahrání tahu 
                ManagerAlfaBeta.ChangeOnTurn();
                
                value = -AlfaBeta(ManagerAlfaBeta, depth - 1, -MAX, Further(-alpha));                
                value = Closer(value);


                if (value > alpha)
                {
                    alpha = value;
                    bestMove = move;
                }
                ManagerAlfaBeta.Board.Back(ManagerAlfaBeta);                
            }
            return bestMove;
        }


        public int Evaluate(Manager manager) //ohodnocování polí
        {
            int value = 0;
            Stone stone = new Stone();
            int x = 0;
            int y = 0;
            int i = 0;

            foreach (Field field in manager.Board.fields) //procházení všech polí
            {
                x = i / 6;
                y = i % 6;

                if (field != Field.empty && field != Field.freeze)
                {
                    stone.properties = field;
                    stone.position.x = x;
                    stone.position.y = y;
                    if ((stone.properties & Field.king) == Field.king) //hodnoty dané králem
                    {                     
                        value += Rule1(stone, manager); //král blíže konci
                        if (value >= MAX)
                        { return value; }
                        value += Rule2(stone, manager); //král v bezpečí 
                    }                         
                }
                i++;
            }

            if (manager.Board.FreezeStones.Count > 0)
            {
                value += Rule3(manager); //zamrzlý král
            }
            if (value > MAX)
                value = MAX;
            return value;
        }


        private int Rule1(Stone stone, Manager manager) //král blíže konci
        {
            int value = 0;
            int whiteValue = 0;
            int blackValue = 0;
           
                if ((stone.properties & Field.white) == Field.white)
                {                    
                    switch (stone.position.y)
                    {
                        case 0:
                            blackValue = 0;
                        break;
                        case 1:
                            whiteValue = 60;
                        break;
                        case 2:
                            whiteValue = 121;
                        break;
                        case 3:
                            whiteValue = 243;
                        break;
                        case 4:
                            whiteValue = 487;
                        break;
                        case 5:
                        if (manager.PlayerOnTurn == manager.White)
                        { return MAX; }
                        else { return -MAX; }
                    }
                }

            if ((stone.properties & Field.black) == Field.black)
                {
                    switch (stone.position.y)
                    {                        
                        case 0:
                        if (manager.PlayerOnTurn == manager.Black)
                        { return MAX; }
                        else { return -MAX; }
                        case 1:
                            blackValue = 487;
                        break;
                        case 2:
                            blackValue = 243;
                        break;
                        case 3:
                            blackValue = 121;
                        break;
                        case 4:
                            blackValue = 60;
                        break;
                        case 5:
                            blackValue = 0;
                        break;
                    }
                }
            
                if (manager.PlayerOnTurn == manager.White)
            {
                value += whiteValue;
                value -= blackValue;
            }
                if (manager.PlayerOnTurn == manager.Black)
            {
                value -= whiteValue;
                value += blackValue;
            }
            return value;
        }

        private int Rule2(Stone stone, Manager manager) //král v bezpečí
        {
            int value = 0;
            int whiteValue = 0;
            int blackValue = 0;

            List<pos> nextPos = new List<pos>();
            nextPos = NextField(stone, stone.position, manager);
            foreach (pos pos in nextPos)
            {
                if (manager.Board.fields[pos.x - 1, pos.y - 1] != Field.empty)
                {
                    if ((stone.properties & Field.white) == Field.white)
                    {
                        if (manager.Board.fields[pos.x - 1, pos.y - 1] == Field.freeze || (manager.Board.fields[pos.x - 1, pos.y - 1] & Field.white) == Field.white)
                        {
                            whiteValue += 15;
                        }
                    }

                    if ((stone.properties & Field.black) == Field.black)
                    {
                        if (manager.Board.fields[pos.x - 1, pos.y - 1] == Field.freeze || (manager.Board.fields[pos.x - 1, pos.y - 1] & Field.black) == Field.black)
                        {
                            blackValue += 15;
                        }
                    }
                }
            }

            if (manager.PlayerOnTurn == manager.White)
            {
                value += whiteValue;
                value -= blackValue;
            }
            if (manager.PlayerOnTurn == manager.Black)
            {
                value -= whiteValue;
                value += blackValue;
            }
            return value;
        }


        private int Rule3(Manager manager) // zamrzlý král
        {
            int value = 0;
            int whiteValue = 0;
            int blackValue = 0;

            foreach (Stone st in manager.Board.FreezeStones)
            {
                if ((st.properties & Field.king) == Field.king)
                {
                    if ((st.properties & Field.white) == Field.white)
                    {
                        blackValue += 400;
                    }
                    if ((st.properties & Field.black) == Field.black)
                    {
                        whiteValue += 400;
                    }
                }
            }

            if (manager.PlayerOnTurn == manager.White)
            {
                value += whiteValue;
            }
            if (manager.PlayerOnTurn == manager.Black)
            { 
                value += blackValue;
            }
            return value;
        }
    }
}
