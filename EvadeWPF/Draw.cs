using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EvadeWPF
{
    public partial class MainWindow : Window
    {

        /// <summary>
        /// vykreslování kamenů a polí
        /// </summary>
        public void DrawStones()  // vykreslení kamenů
        {

            float thick = 4f; //tloušťka čáry výběru

            int index = 0;
            int x, y;
            float width = sizeField * ratio;
            float height = sizeField * ratio;
            whiteMoveStone = (!manager.White.AutoPlayer && manager.White.OnTurn);
            blackMoveStone = (!manager.Black.AutoPlayer && manager.Black.OnTurn);
            bool possible = false;

            foreach (Field i in manager.Board.fields) //šachovnice
            {
                x = (index / 6);
                y = (index % 6);

                float positionX = (x * sizeField + (x * sizeSpace) + (sizeField / 2) - (ratio * (sizeField / 2)));
                float positionY = (y * sizeField + (y * sizeSpace) + (sizeField / 2) - (ratio * (sizeField / 2)));
                int fieldPositionX = x * sizeField + (x * sizeSpace) + (sizeField / 2) - (sizeField / 2);
                int fieldPositionY = y * sizeField + (y * sizeSpace) + (sizeField / 2) - (sizeField / 2);

                if ((i & Field.possible) == Field.possible) //označení možných budoucích tahů
                {
                    possible = true;
                }
                DrawField(fieldPositionX, fieldPositionY, sizeField, sizeField, possible);
                possible = false;

                if ((i & Field.onTurn) == Field.onTurn) //označení polí vybraného hráče
                {
                    DrawMarkField(x * (sizeField + sizeSpace) + 3, y * (sizeField + sizeSpace) + 3, thick, sizeField - 6, Brushes.DarkGreen);
                }


                if ((i & Field.possible) == Field.possible) //označení možných budoucích tahů
                {
                    DrawMarkField(x * (sizeField + sizeSpace) + 3, y * (sizeField + sizeSpace) + 3, thick, sizeField - 6, Brushes.Blue);
                }

                if ((i & Field.best) == Field.best) //označení nejlepšího tahu
                {
                    DrawMarkField(x * (sizeField + sizeSpace) + 3, y * (sizeField + sizeSpace) + 3, thick, sizeField - 6, Brushes.Red);
                }

                if ((i & Field.freeze) == Field.freeze)
                {
                    DrawMarkField(x * (sizeField + sizeSpace) + 3, y * (sizeField + sizeSpace) + 3, thick, sizeField - 6, Brushes.Aqua);
                    manager.Board.FreezeStones.Reverse();
                    foreach (Stone stone in manager.Board.FreezeStones)
                    {
                        if (stone.position.x == x + 1 && stone.position.y == y + 1)
                        {
                            if ((stone.properties & Field.black) == Field.black)
                            {
                                if ((stone.properties & Field.spawn) == Field.spawn)
                                {
                                    System.Windows.Media.Imaging.BitmapImage bimg = new BitmapImage();
                                    bimg.BeginInit();
                                    bimg.UriSource = new Uri("freezeBlack.png", UriKind.Relative);
                                    bimg.EndInit();
                                    DrawStone(positionX, positionY, width, height, "freezeBlack", bimg, false, false);
                                    break;
                                }

                                if ((stone.properties & Field.king) == Field.king)
                                {

                                    BitmapImage bimg = new BitmapImage();
                                    bimg.BeginInit();
                                    bimg.UriSource = new Uri("freezeBlackKing.png", UriKind.Relative);
                                    bimg.EndInit();
                                    DrawStone(positionX, positionY, width, height, "freezeBlackKing", bimg, false, false);
                                    break;
                                }
                            }

                            if ((stone.properties & Field.white) == Field.white)
                            {
                                if ((stone.properties & Field.spawn) == Field.spawn)
                                {
                                    BitmapImage bimg = new BitmapImage();
                                    bimg.BeginInit();
                                    bimg.UriSource = new Uri("freezeWhite.png", UriKind.Relative);
                                    bimg.EndInit();
                                    DrawStone(positionX, positionY, width, height, "freezeWhite", bimg, false, false);
                                    break;
                                }

                                if ((stone.properties & Field.king) == Field.king)
                                {
                                    BitmapImage bimg = new BitmapImage();
                                    bimg.BeginInit();
                                    bimg.UriSource = new Uri("freezeWhiteKing.png", UriKind.Relative);
                                    bimg.EndInit();
                                    DrawStone(positionX, positionY, width, height, "freezeWhiteKing", bimg, false, false);
                                    break;
                                }
                            }
                        }
                    }
                    manager.Board.FreezeStones.Reverse();
                }

                if ((i & Field.black) == Field.black || (i & Field.white) == Field.white && !((i & Field.freeze) == Field.freeze))
                {
                    StoneToField(manager.Board.GetStoneColor(i), manager.Board.GetStoneType(i), x, y);
                }
                index++;
            }
            if (selectStone.properties != Field.empty && (manager.Board.fields[selectStone.position.x - 1, selectStone.position.y -1] & Field.best) != Field.best)
            {
                DrawMarkField((selectStone.position.x - 1) * (sizeField + sizeSpace) + 3, (selectStone.position.y - 1) * (sizeField + sizeSpace) + 3, thick, sizeField - 6, Brushes.Yellow);
            }
        }


        public void DrawMarkField(float positionX, float positionY, float width, float height, Brush color)
        {
            Rectangle rect = new Rectangle();
            Pen pen = new Pen(color, width);
            rect.Width = height;
            rect.Height = height;
            rect.Fill = null;
            rect.Stroke = color;
            rect.StrokeThickness = width;
            BoardForm.Children.Add(rect);
            Canvas.SetLeft(rect, positionX);
            Canvas.SetTop(rect, positionY);
        }

        public void DrawFields()
        {
            int index = 0;
            int x, y;
            bool possible = false;

            foreach (Field i in manager.Board.fields) //šachovnice
            {
                x = (index / 6);
                y = (index % 6);
                int positionX = x * sizeField + (x * sizeSpace) + (sizeField / 2) - (sizeField / 2);
                int positionY = y * sizeField + (y * sizeSpace) + (sizeField / 2) - (sizeField / 2);

                if ((i & Field.possible) == Field.possible) //označení možných budoucích tahů
                {
                    possible = true;
                }
                DrawField(positionX, positionY, sizeField, sizeField, possible);
                index++;
            }
        }


        public void StoneToField(string color, string name, int x, int y) // vykreslení kamene do pole
        {
            float positionX = (x * sizeField + (x * sizeSpace) + (sizeField / 2) - (ratio * (sizeField / 2)));
            float positionY = (y * sizeField + (y * sizeSpace) + (sizeField / 2) - (ratio * (sizeField / 2)));
            float width = sizeField * ratio;
            float height = sizeField * ratio;
            bool freezePossible = false;

            if ((!manager.PlayerOnTurn.GetColorPlayer().Equals(color)) && (manager.Board.fields[x, y] & Field.possible) == Field.possible)
            {
                freezePossible = true;
            }

            if (color == "white" && name.Contains("king"))
            {
                BitmapImage bimg = new BitmapImage();
                bimg.BeginInit();
                bimg.UriSource = new Uri("whiteKing.png", UriKind.Relative);
                bimg.EndInit();
                DrawStone(positionX, positionY, width, height, "whiteKing", bimg, whiteMoveStone, freezePossible);
            }

            if (color == "white" && name.Contains("spawn"))
            {
                BitmapImage bimg = new BitmapImage();
                bimg.BeginInit();
                bimg.UriSource = new Uri("whiteSpawn.png", UriKind.Relative);
                bimg.EndInit();
                DrawStone(positionX, positionY, width, height, "whiteSpawn", bimg, whiteMoveStone, freezePossible);
            }

            if (color == "black" && name.Contains("king"))
            {
                BitmapImage bimg = new BitmapImage();
                bimg.BeginInit();
                bimg.UriSource = new Uri("blackKing.png", UriKind.Relative);
                bimg.EndInit();
                DrawStone(positionX, positionY, width, height, "blackKing", bimg, blackMoveStone, freezePossible);
            }

            if (color == "black" && name.Contains("spawn"))
            {
                BitmapImage bimg = new BitmapImage();
                bimg.BeginInit();
                bimg.UriSource = new Uri("blackSpawn.png", UriKind.Relative);
                bimg.EndInit();
                DrawStone(positionX, positionY, width, height, "blackSpawn", bimg, blackMoveStone, freezePossible);
            }
        }


        public void DrawField(float positionX, float positionY, float width, float height, bool possible) //kresba pole
        {
            BitmapImage bimg = new BitmapImage();
            bimg.BeginInit();
            bimg.UriSource = new Uri("fieldboard.png", UriKind.Relative);
            bimg.EndInit();
            Image img = new Image();
            img.Source = bimg;
            Double Xposition = positionX;
            Double Yposition = positionY;
            if (possible)
            {
                img.MouseLeftButtonDown += field_MouseLeftButtonDown;
            }
            img.Width = width;
            img.Height = height;
            BoardForm.Children.Add(img);
            Canvas.SetLeft(img, Xposition);
            Canvas.SetTop(img, Yposition);
        }


        public void DrawStone(float positionX, float positionY, float width, float height, string name, BitmapImage bitmap, bool MouseMove, bool possibleFreeze) //kresba kamene
        {
            Image img = new Image();
            img.Source = bitmap;
            if (MouseMove)
            {
                img.MouseLeftButtonDown += shape_MouseLeftButtonDown;
                img.MouseLeftButtonUp += shape_MouseLeftButtonUp;
                img.MouseMove += shape_MouseMove;
            }

            if (possibleFreeze)
            {
                img.MouseLeftButtonDown += Shape_MouseLeftButtonDownOn;
            }
            else
            {
                img.MouseLeftButtonDown -= Shape_MouseLeftButtonDownOn;
            }

            img.Name = name;
            Double Xposition = positionX;
            Double Yposition = positionY;
            img.Width = width;
            img.Height = height;

            BoardForm.Children.Add(img);
            Canvas.SetLeft(img, Xposition);
            Canvas.SetTop(img, Yposition);
        }
    }
}
