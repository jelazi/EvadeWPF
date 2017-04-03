using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace EvadeWPF
{
    public partial class MainWindow : Window
    {

        /// <summary>
        /// pohyb kamene myší
        /// </summary>

        pos selectField;
        pos beforeField;
        Stone selectStone = new Stone();
        bool captured = false; //držíš objekt - pohyb myší
        double x_shape, x_canvas, y_shape, y_canvas; // 
        Point beforePoint = new Point(); //předchozí bod - pohyb myší
        UIElement source = null; //zdroj - kámen - pohyb myší
        bool whiteMoveStone = false;
        bool blackMoveStone = false;

        private void field_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) //klik na pole při vybraném kameni přesun kamene
        {
            if (gameState == GameState.play)
            {
                countBlick = 10;

                if (selectStone.properties != Field.empty)
                {
                    selectField = FieldPosition(Convert.ToInt32(e.GetPosition(BoardForm).X), Convert.ToInt32(e.GetPosition(BoardForm).Y));
                    if (manager.Controlor.IsPossibleMove(selectField, selectStone, manager))//možný tah?
                    {
                        manager.MoveChoisenStone(selectField, selectStone);  //přesun vybraného kamene
                        selectStone = new Stone();
                        manager.Controlor.IsEnd(manager, manager.PlayerOnTurn); //kontrola konce hry
                        HistoryToString();
                        RunningGame();                  //popisky ke hře
                        if (manager.End == false) //není konec hry
                        {
                            manager.ChangeOnTurn();             //změna hráče na tahu
                            manager.PlayerOnTurn.MarkStones(manager.Board); //označení kamenů hráče na tahu
                            Timer.Start();              //spuštění časovače
                        }

                        DrawStones();

                    }

                }
            }
            
        }

        private void shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) //klik myší na kámen
        {
            if (gameState == GameState.play)
            {
                countBlick = 10;
                source = (UIElement)sender;
                Cursor = Cursors.Hand;
                beforePoint.X = Canvas.GetLeft(source);
                beforePoint.Y = Canvas.GetTop(source);
                Mouse.Capture(source);
                captured = true;
                x_shape = Canvas.GetLeft(source);
                x_canvas = e.GetPosition(BoardForm).X;
                y_shape = Canvas.GetTop(source);
                y_canvas = e.GetPosition(BoardForm).Y;
                CanvasZindex = Canvas.GetZIndex(source);
                Canvas.SetZIndex(source, 1000); //object vždy navrchu




                selectField = beforeField = FieldPosition(Convert.ToInt32(e.GetPosition(BoardForm).X), Convert.ToInt32(e.GetPosition(BoardForm).Y));

                if (selectStone.properties == Field.empty)
                {
                    selectStone.properties = manager.Board.fields[selectField.x - 1, selectField.y - 1];
                    selectStone.position = beforeField;

                    manager.ChooseStone(selectField, selectStone); //vybrání nového kamene
                }



                if (manager.Board.ContainStone(manager.Board.fields[selectField.x - 1, selectField.y - 1]) &&
                    manager.Board.ContainStonePlayer(manager.Board.fields[selectField.x - 1, selectField.y - 1], manager.PlayerOnTurn)) //vybral jsem kámen stejného hráče
                {
                    manager.Board.EraseSelectField(manager); //odznačení vybraného pole
                    selectStone = new Stone(); //odznačení vybraného kamene

                    selectStone.properties = manager.Board.fields[selectField.x - 1, selectField.y - 1];
                    selectStone.position = beforeField;
                    manager.ChooseStone(selectField, selectStone); //vybrání nového kamene

                }
                if (manager.Board.ContainStone(manager.Board.fields[selectField.x - 1, selectField.y - 1]) &&
                    manager.Board.ContainStonePlayer(manager.Board.fields[selectField.x - 1, selectField.y - 1], manager.OpositePlayer(manager.PlayerOnTurn)))
                {
                    if (manager.Controlor.IsPossibleMove(selectField, selectStone, manager))//možný tah?
                    {

                        manager.MoveChoisenStone(selectField, selectStone);  //přesun vybraného kamene
                        selectStone = new Stone();
                        manager.Controlor.IsEnd(manager, manager.PlayerOnTurn); //kontrola konce hry
                        HistoryToString();
                        RunningGame();                  //popisky ke hře
                        if (manager.End == false) //není konec hry
                        {
                            manager.ChangeOnTurn();             //změna hráče na tahu
                            manager.PlayerOnTurn.MarkStones(manager.Board); //označení kamenů hráče na tahu
                            Timer.Start();              //spuštění časovače
                        }
                    }
                }

                manager.Board.fields[selectField.x - 1, selectField.y - 1] = Field.empty;


                DrawStones();
                RunningGame();
            }

            
        }


        private void Shape_MouseLeftButtonDownOn(object sender, MouseEventArgs e) //klik na kámen hráče k zamrznutí
        {

            if (gameState == GameState.play)
            {
                selectField = beforeField = FieldPosition(Convert.ToInt32(e.GetPosition(BoardForm).X), Convert.ToInt32(e.GetPosition(BoardForm).Y));
                Point newPosition = new Point();
                if (manager.Board.ContainStone(manager.Board.fields[selectField.x - 1, selectField.y - 1]) &&
                   manager.Board.ContainStonePlayer(manager.Board.fields[selectField.x - 1, selectField.y - 1], manager.OpositePlayer(manager.PlayerOnTurn)))
                {
                    if (manager.Controlor.IsPossibleMove(selectField, selectStone, manager))//možný tah?
                    {

                        newPosition.X = Canvas.GetLeft(source);
                        newPosition.Y = Canvas.GetTop(source);

                        manager.MoveChoisenStone(selectField, selectStone);  //přesun vybraného kamene
                        selectStone = new Stone();
                        manager.Controlor.IsEnd(manager, manager.PlayerOnTurn); //kontrola konce hry
                        HistoryToString();
                        RunningGame();                  //popisky ke hře

                        if (manager.End == false) //není konec hry
                        {
                            manager.ChangeOnTurn();             //změna hráče na tahu
                            manager.PlayerOnTurn.MarkStones(manager.Board); //označení kamenů hráče na tahu
                            Timer.Start();              //spuštění časovače
                        }



                    }
                }



                DrawStones();
            }
            

        }


        private void shape_MouseMove(object sender, MouseEventArgs e) //pohyb myší s kamenem
        {
            if (gameState == GameState.play)
            {
                if (captured)
                {
                    double x = e.GetPosition(BoardForm).X;
                    double y = e.GetPosition(BoardForm).Y;
                    x_shape += x - x_canvas;
                    Canvas.SetLeft(source, x_shape);
                    x_canvas = x;
                    y_shape += y - y_canvas;
                    Canvas.SetTop(source, y_shape);
                    y_canvas = y;
                }
            }

        }

        private void shape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) //odklik myší z kamene
        {
            try
            {
                countBlick = 10;


                if (gameState == GameState.play)
                {
                    Mouse.Capture(null);
                    captured = false;
                    Canvas.SetZIndex(source, CanvasZindex); //object dolů
                    Cursor = Cursors.Arrow;



                    Canvas.SetLeft(source, beforePoint.X);
                    Canvas.SetTop(source, beforePoint.Y);




                    selectField = FieldPosition(Convert.ToInt32(e.GetPosition(BoardForm).X), Convert.ToInt32(e.GetPosition(BoardForm).Y));

                    if (selectField.x == selectStone.position.x && selectField.y == selectStone.position.y)
                    {
                        manager.Board.fields[beforeField.x - 1, beforeField.y - 1] = selectStone.properties; //vrácení zpátky

                    }
                    else
                    {
                        if (manager.Controlor.IsPossibleMove(selectField, selectStone, manager))//možný tah?
                        {

                            manager.MoveChoisenStone(selectField, selectStone);  //přesun vybraného kamene
                            selectStone = new Stone();
                            manager.Controlor.IsEnd(manager, manager.PlayerOnTurn); //kontrola konce hry
                            HistoryToString();
                            RunningGame();                  //popisky ke hře

                            if (manager.End == false) //není konec hry
                            {
                                manager.ChangeOnTurn();             //změna hráče na tahu
                                manager.PlayerOnTurn.MarkStones(manager.Board); //označení kamenů hráče na tahu
                                Timer.Start();              //spuštění časovače
                            }


                        }
                        else
                        {
                            manager.Board.fields[beforeField.x - 1, beforeField.y - 1] = selectStone.properties; //vrácení zpátky


                        }
                    }


                    DrawStones();
                    RunningGame();
                }
            } catch { }

        }

        DispatcherTimer TimerMoving = new DispatcherTimer();
        double MovingstepX;
        double MovingstepY;
        Point MovingactualPosition;
        int Movingindex;

        public void TimerMovingOn()
        {
            TimerMoving.Tick += new EventHandler(MovingTimerOneTick);
            TimerMoving.Interval = new TimeSpan(0, 0, 0, 0, 20);

        }


        private void MovingTimerOneTick(object sender, EventArgs e)
        {
            Canvas.SetLeft(source, MovingactualPosition.X);
            Canvas.SetTop(source, MovingactualPosition.Y);
            MovingactualPosition.X = MovingactualPosition.X + MovingstepX;
            MovingactualPosition.Y = MovingactualPosition.Y + MovingstepY;

            Movingindex++;

            if (Movingindex == 100)
            {
                TimerMoving.Stop();
                Movingindex = 0;
            }

        }

        private void MovingObject(Point before, Point after, UIElement source)
        {
            Canvas.SetZIndex(source, 1000); //object vždy navrchu
            MovingstepX = (after.X - before.X) / 100;
            MovingstepY = (after.Y - before.Y) / 100;
            MovingactualPosition = before;
            TimerMovingOn();
            TimerMoving.Start();

        }

        public pos FieldPosition(int x, int y) // vrací pozici pole podle polohy myši
        {
            int px = (x / (sizeField + sizeSpace)) + 1;
            int py = (y / (sizeField + sizeSpace)) + 1;
            pos fieldPosition;
            fieldPosition.x = px;
            fieldPosition.y = py;
            return fieldPosition;
        }

    }
}
