using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvadeWPF
{


    class Board 
    {
        /// <summary>
        /// hrací deska
        /// </summary>
        /// 


        public Field[,] fields = new Field[6, 6]; // pole polí skládající se z hracích polí
        pos selectField;
        Controlor control = new Controlor();
        Random rnd = new Random();
        List<Stone> freezeStones = new List<Stone>();
        Stone stone = new Stone();
        public bool start = true;


        public string GetStoneColor(Field field) //vrací barvu kamene 
        {
            if ((field & Field.black) == Field.black)
            { return "black"; }
            if ((field & Field.white) == Field.white)
            { return "white"; }
            return null;
        }

        public string GetStoneType(Field field) //vrací typ kamene
        {
            if ((field & Field.spawn) == Field.spawn)
            { return "spawn"; }
            if ((field & Field.king) == Field.king)
            { return "king"; }
            return null;
        }


        public Field SetColor(string color)//ulož barvu dle stringu
        {
            if (color == "white")
            { return Field.white; }
            if (color == "black")
            { return Field.black; }
            return Field.empty;
        }


        public Field SetType(string type)//ulož typ dle stringu
        {
            if (type == "spawn")
            { return Field.spawn; }
            if (type == "king")
            { return Field.king; }
            return Field.empty;
        }


        public Stone SetStoneFromField(Field field, int x, int y) //vytvoř kámen z pole
        {
            Stone stone = new Stone();
            stone.properties = field;
            stone.position.x = x;
            stone.position.y = y;
            return stone;
        }

        public List<Stone> FreezeStones//get, set zamrzlých kamenů
        {
            get { return freezeStones; }
            set { freezeStones = value; }
        }

        public Stone Stone//get, set kamene na tahu
        {
            get { return stone; }
            set { stone = value; }
        }


        public pos ChoicePossibleMove(List<pos> possiblePosition) //automatické vybrání možné pozice
        {
            pos position = new pos();
            int x = rnd.Next(0, possiblePosition.Count - 1);
            position = possiblePosition[x];
            return position;
        }


        public bool ContainStone(Field field) //vrací pravdu či nepravdu - obsahuje pole kámen
        {
            return ((field & Field.white) == Field.white || (field & Field.black) == Field.black);               
        }


        public void MoveStone(Stone stone, pos pos, Manager manager) //přesun kamene
        {
            int stoneX = stone.position.x - 1;
            int stoneY = stone.position.y - 1;
            int nextX = pos.x - 1;
            int nextY = pos.y - 1;
            
            if (manager.Board.ContainStone(fields[nextX, nextY]))                        //zamrznutí
            {

                stone.properties &= ~Field.onTurn;                                  //vymazání že je kámen na tahu
                stone.properties &= ~Field.choice;                                  //vymazání že je kámen vybrán                           
                manager.Board.freezeStones.Add(stone);                              //přidání útočícího kamene do listu zamrznutých kamenů
                manager.Board.fields[stoneX, stoneY] = Field.empty;                 //vymazání útočícího kamene                
                manager.Board.freezeStones.Add(manager.Board.SetStoneFromField(manager.Board.fields[nextX, nextY], nextX + 1, nextY + 1));  //vytvoření kamene z pole na které se útočí a přidání do listu zamrznutých kamenů
                manager.Board.fields[nextX, nextY] = Field.freeze;                  //zamrznutí pole
                manager.Controlor.UnmarkPossibleFields(this);                       //odznačení pole

                EraseSelectField(manager);                                          //vymazání 
                EraseSelectStone(manager);

                pos posBefore;
                posBefore.x = stoneX + 1;
                posBefore.y = stoneY + 1;
                pos posNext;
                posNext.x = nextX + 1;
                posNext.y = nextY + 1;
                manager.AddToOneMoveText(stone.properties, posBefore, posNext);
            }
            else //normální posun
            {
                stone.properties &= ~Field.onTurn;                                  //vymazání že je kámen na tahu
                stone.properties &= ~Field.choice;                                  //vymazání že je kámen vybrán
                stone.position = pos;                                               //přepsání pozice útočícího kamene
                manager.Board.fields[nextX, nextY] = stone.properties;              //přesun kamene na nové pole
                manager.Board.fields[stoneX, stoneY] = Field.empty;                 //vymazání kamene z předchozího pole
                manager.Controlor.UnmarkPossibleFields(this);                       //odznačení možných polí

                EraseSelectField(manager);
                EraseSelectStone(manager);

                pos posBefore;
                posBefore.x = stoneX + 1;
                posBefore.y = stoneY + 1;
                pos posNext;
                posNext.x = nextX + 1;
                posNext.y = nextY + 1;
                manager.AddToOneMoveText(stone.properties, posBefore, posNext);
            }
            manager.GetStonesToPlayers();
        }


        public void MoveStone(move move, Manager manager) //přetížená metoda přesun kamenů podle přesunu
        {
            Stone stone = new Stone();
            stone.position = move.beforePos;
            stone.properties = fields[move.beforePos.x - 1, move.beforePos.y - 1];
            MoveStone(stone, move.nextPos, manager);
        }

        public void MoveStone(IList<move> moving, Manager manager) //přesun kamenů dle listu
        {
            foreach (move move in moving)
            {
                MoveStone(move, manager);
            }
        }


        public void Back(Manager manager) //tah zpět
        {
            if (manager.HistoryMove.Count > 0)
            {
                manager.PlayerOnTurn = (manager.FindPlayerByName(manager.HistoryMove.Last(), manager));
                Stone stone = new Stone();
                stone.position = manager.HistoryMove.Last().nextPos;
                stone.properties = manager.HistoryMove.Last().properties;
                pos nextPos = manager.HistoryMove.Last().nextPos;
                pos beforePos = manager.HistoryMove.Last().beforePos;

                if ((manager.Board.fields[nextPos.x - 1, nextPos.y - 1] & Field.freeze) == Field.freeze) //zamrznuté pole
                {
                    Stone stoneOpposite = new Stone();
                    stoneOpposite.position = beforePos;
                    stoneOpposite.properties = manager.Board.FreezeStones.Last().properties;
                    manager.OpositePlayer(manager.PlayerOnTurn).Stones.Add(stoneOpposite);
                    manager.Board.fields[beforePos.x - 1, beforePos.y - 1] = stone.properties;
                    manager.Board.fields[nextPos.x - 1, nextPos.y - 1] = manager.Board.FreezeStones.Last().properties;
                    manager.Board.FreezeStones.RemoveAt(manager.Board.FreezeStones.Count - 1);
                    manager.Board.FreezeStones.RemoveAt(manager.Board.FreezeStones.Count - 1);
                    manager.PlayerOnTurn.Stones.Add(stone);
                    manager.Board.EraseSelectField(manager);                                            //vymazání 
                    manager.Board.EraseSelectStone(manager);
                }
                else //normální back
                {
                    manager.Board.MoveStone(stone, manager.HistoryMove.Last().beforePos, manager);
                    manager.HistoryMove.RemoveAt(manager.HistoryMove.Count - 1);
                }
                manager.HistoryMoveBack.Add(manager.HistoryMove.Last());
                manager.HistoryMove.RemoveAt(manager.HistoryMove.Count - 1);
                manager.GetStonesToPlayers();
            }
        }


        public void EraseSelectStone(Manager manager)//zrušení vybraného kamene
        {
            manager.Board.Stone = new Stone();
        }


        public void EraseSelectField(Manager manager) //vymázání vybraného pole
        {
            for (int i = 0; i < manager.Board.fields.Length; i++)
            {
                int x = i / 6;
                int y = i % 6;
                manager.Board.fields[x, y] &= ~Field.choice;
                manager.Board.fields[x, y] &= ~Field.onTurn;
                manager.Board.fields[x, y] &= ~Field.possible;
            }
            manager.Board.selectField.x = 0;
            manager.Board.selectField.y = 0;
        }


        public void SetSelectField(int x, int y) //uložení vybraného pole
        {
            selectField.x = x;
            selectField.y = y;
        }
        

        public bool ContainStonePlayer(Field field, Player player)//kontrola, zda pole obsahuje kámen
        {
            return (field.ToString().Contains(player.Color));
        }

        public void MakeSelectField(pos selectField, Player playeronTurn, Manager manager) //vybrání pole
        {
            int nowX = selectField.x - 1;
            int nowY = selectField.y - 1;
            int beforeX;
            int beforeY;

            if (ContainStonePlayer(fields[nowX, nowY], playeronTurn))
            {
                beforeX = selectField.x-1;
                beforeY = selectField.y-1;
                Stone stone = new Stone();
                stone.properties = fields[nowX, nowY];
                stone.position = selectField;

                if ((fields[beforeX, beforeY] & Field.choice) == Field.choice)
                {
                    fields[beforeX, beforeY] &= ~Field.choice;
                }
                fields[nowX, nowY] = fields[nowX, nowY] | Field.choice;
                SetSelectField(nowX + 1, nowY + 1);
                control.UnmarkPossibleFields(this);

                List<pos> possiblePosition = new List<pos>();
                possiblePosition = control.PossiblePosition(stone, stone.position, this);
                control.MarkPossibleField(this, possiblePosition);
            }
        }
    }
}
