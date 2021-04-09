using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AutoNeuralNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Threading;
//using System.Windows.Forms;

namespace AiBigStand
{
    public class Game1 : Game
    {
        Keys lastKey = Keys.Down;
        StreamWriter streamWriter = new StreamWriter(@"C:\Users\Mikhail\source\repos\AiBigStand\AiBigStand\traindata.txt", true);
        double[] answers;
        private Timer neuralUpdate;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Texture2D rectangleBlock;
        NeuralCore net;
        Rectangle r;
        private const int cellSize = 15;
        private const int cellCount = 25;
        private double[,] matrix;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = cellCount * cellSize + 2 * cellSize + cellSize;
            _graphics.PreferredBackBufferHeight = cellCount * cellSize;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            answers = new double[10];
            net = new NeuralCore(@"links.txt");
            matrix = new double[cellCount, cellCount];
            int num = 0;
            neuralUpdate = new Timer(new TimerCallback(NeuralUpdate_Tick), num, 0, 250);
            base.Initialize();
        }

        private void NeuralUpdate_Tick(object state)
        {
            var localMatrix = new List<double>();
            foreach (var item in matrix)
            {
                localMatrix.Add(item);
            }
            answers = net.RunNet(localMatrix.ToArray());
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
            rectangleBlock = new Texture2D(GraphicsDevice, 1, 1);
            r = new Rectangle(new Point(0, 0), new Point(cellSize, cellSize));
        }

        private void AddToMatrix(Vector2 pos)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            matrix[x, y] = 1;
            if (x + 1 < cellCount)
            {
                if (matrix[x + 1, y] == 0)
                    matrix[x + 1, y] = 0.5;
            }
            if (x - 1 > -1)
            {
                if (matrix[x - 1, y] == 0)
                    matrix[x - 1, y] = 0.5;
            }
            if (y + 1 < cellCount)
            {
                if (matrix[x, y + 1] == 0)
                    matrix[x, y + 1] = 0.5;
            }
            if (y - 1 > -1)
            {
                if (matrix[x, y - 1] == 0)
                    matrix[x, y - 1] = 0.5;
            }
        }

        private void SaveImage(string a, StreamWriter stream)
        {
            string dataLine = string.Empty;
            List<string> parts = new List<string>();
            foreach (var item in matrix)
            {
                parts.Add(item.ToString());
            }
            dataLine = string.Join("|", parts);
            stream.WriteLine(dataLine + $" {a}");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            MouseState state = Mouse.GetState();
            Vector2 selectedCell = new Vector2(state.X / cellSize, state.Y / cellSize);
            if (selectedCell.X < cellCount && selectedCell.Y < cellCount && selectedCell.Y > -1 && selectedCell.X > -1)
            {
                if (state.LeftButton == ButtonState.Pressed) AddToMatrix(selectedCell);
                else if (state.RightButton == ButtonState.Pressed) matrix[(int)selectedCell.X, (int)selectedCell.Y] = 0;
                else if (Keyboard.GetState().IsKeyDown(Keys.Q)) matrix = new double[cellCount, cellCount];
            }
            KeyboardState keyboardState = Keyboard.GetState();

            if (!keyboardState.GetPressedKeys().Contains(lastKey) && keyboardState.IsKeyDown(Keys.LeftControl) && keyboardState.GetPressedKeys().Length == 3)
            {
                if (lastKey == Keys.Down)
                    lastKey = Keys.Up;
                else
                    lastKey = Keys.Down;
                string answer = string.Empty;
                switch (keyboardState.GetPressedKeys()[1])
                {
                    case Keys.NumPad0:
                        answer = "0000000001";
                        break;
                    case Keys.NumPad1:
                        answer = "1000000000";
                        break;
                    case Keys.NumPad2:
                        answer = "0100000000";
                        break;
                    case Keys.NumPad3:
                        answer = "0010000000";
                        break;
                    case Keys.NumPad4:
                        answer = "0001000000";
                        break;
                    case Keys.NumPad5:
                        answer = "0000100000";
                        break;
                    case Keys.NumPad6:
                        answer = "0000010000";
                        break;
                    case Keys.NumPad7:
                        answer = "0000001000";
                        break;
                    case Keys.NumPad8:
                        answer = "0000000100";
                        break;
                    case Keys.NumPad9:
                        answer = "0000000010";
                        break;
                    default:
                        answer = "non";
                        break;
                }
                try
                {
                    if (answer != "non")
                    {
                        SaveImage(answer, streamWriter);
                        matrix = new double[cellCount, cellCount];
                    }
                }
                finally
                {

                }
            }
            base.Update(gameTime);
        }

        public void DrawRect(Vector2 pos, Color color)
        {
            Color xnaColorBorder = new Color(128, 128, 128); // default color gray
            rectangleBlock.SetData(new[] { xnaColorBorder });
            r.Location = new Point((int)pos.X, (int)pos.Y);
            r.Size = new Point(cellSize, cellSize);
            _spriteBatch.Draw(rectangleBlock, r, color);

        }

        public void DrawRect(Vector2 size, Vector2 pos, Color color)
        {

            Color xnaColorBorder = new Color(128, 128, 128); // default color gray
            rectangleBlock.SetData(new[] { xnaColorBorder });
            r.Location = new Point((int)pos.X, (int)pos.Y);
            r.Size = new Point((int)size.X, (int)size.Y);
            _spriteBatch.Draw(rectangleBlock, r, color);

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    DrawRect(new Vector2(cellSize * i, cellSize * j), new Color((int)(255 * matrix[i, j]), (int)(255 * matrix[i, j]), (int)(255 * matrix[i, j])));
                }
            }
            for (int i = 0; i < 10; i++)
            {
                var k = answers[i];
                if (k != answers.Max()) k = 0;
                DrawRect(new Vector2(cellSize * cellCount + cellSize, cellSize * i + 10 * i), new Color((float)(255 * k), (float)(255 * k), (float)(225 * k)));
            }
            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            streamWriter.Close();         
            streamWriter.Dispose();
            base.UnloadContent();
        }
    }
}
