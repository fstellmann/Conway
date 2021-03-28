using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Conway
{
    public partial class Form1 : Form
    {
        struct Cell
        {
            public bool living;
            public int x;
            public int y;
        }

        private bool formShown = false;
        private bool running = false;
        private int resolutionFaktor = 4;
        private double pseudoFramerate;//smaller is faster
        bool[,] bMainArr;
        bool[,] initialState;
        int xOffSet = 64;
        int yOffSet = 64;
        int generation = 1;
        Color cellColor = Color.Black;

        ManualResetEvent mre = new ManualResetEvent(false);
        public Form1()
        {
            InitializeArray();
            InitializeComponent();
            this.Width = 15 + xOffSet * xOffSet / resolutionFaktor;
            this.Height = 40 + yOffSet * yOffSet / resolutionFaktor;
            pictureBox1.Width = 15 + xOffSet * xOffSet / resolutionFaktor;
            pictureBox1.Height = 40 + yOffSet * yOffSet / resolutionFaktor;
            lblGens.Text = "Generation: 0";
            numericUpDown1.Value = 5;

            new Thread(() =>
            {
                StartGameLoop();
            }
            ).Start();
        }

        #region Methods
        public void Resume() => mre.Set();

        public void Pause() => mre.Reset();

        private void StartGameLoop()
        {
            while (true)
            {
                mre.WaitOne();
                bool[,] currentGeneration = bMainArr.Clone() as bool[,];

                for (int dim1 = 0; dim1 < bMainArr.GetLength(1); dim1++)
                {
                    for (int dim2 = 0; dim2 < bMainArr.GetLength(0); dim2++)
                    {
                        Cell currCell = new Cell() { living = bMainArr[dim1, dim2], x = dim2, y = dim1 };
                        List<bool> lNeighbours = new List<bool>();
                        List<int> lBlocksToCheck = new List<int>() { 1, 2, 3, 4, 6, 7, 8, 9 };
                        #region getNeighbours
                        if (dim1 == 0)
                        {
                            lBlocksToCheck.Remove(1);
                            lBlocksToCheck.Remove(2);
                            lBlocksToCheck.Remove(3);
                            if (dim2 == 0)
                            {
                                lBlocksToCheck.Remove(4);
                                lBlocksToCheck.Remove(7);
                            }
                            if (dim2 == xOffSet - 1)
                            {
                                lBlocksToCheck.Remove(6);
                                lBlocksToCheck.Remove(9);
                            }
                        }
                        if (dim1 == yOffSet - 1)
                        {
                            lBlocksToCheck.Remove(7);
                            lBlocksToCheck.Remove(8);
                            lBlocksToCheck.Remove(9);
                            if (dim2 == 0)
                            {
                                lBlocksToCheck.Remove(1);
                                lBlocksToCheck.Remove(4);
                            }
                            if (dim2 == xOffSet - 1)
                            {
                                lBlocksToCheck.Remove(3);
                                lBlocksToCheck.Remove(6);
                            }
                        }
                        if (dim1 > 0 && dim1 < yOffSet - 1)
                        {
                            if (dim2 == 0)
                            {
                                lBlocksToCheck.Remove(1);
                                lBlocksToCheck.Remove(4);
                                lBlocksToCheck.Remove(7);
                            }
                            if (dim2 == xOffSet - 1)
                            {
                                lBlocksToCheck.Remove(3);
                                lBlocksToCheck.Remove(6);
                                lBlocksToCheck.Remove(9);
                            }
                        }
                        #endregion

                        foreach (int i in lBlocksToCheck)
                        {
                            switch (i)
                            {
                                case 1:
                                    lNeighbours.Add(bMainArr[currCell.x - 1, currCell.y - 1]);
                                    break;
                                case 2:
                                    lNeighbours.Add(bMainArr[currCell.x, currCell.y - 1]);
                                    break;
                                case 3:
                                    lNeighbours.Add(bMainArr[currCell.x + 1, currCell.y - 1]);
                                    break;
                                case 4:
                                    lNeighbours.Add(bMainArr[currCell.x - 1, currCell.y]);
                                    break;
                                case 6:
                                    lNeighbours.Add(bMainArr[currCell.x + 1, currCell.y]);
                                    break;
                                case 7:
                                    lNeighbours.Add(bMainArr[currCell.x - 1, currCell.y + 1]);
                                    break;
                                case 8:
                                    lNeighbours.Add(bMainArr[currCell.x, currCell.y + 1]);
                                    break;
                                case 9:
                                    lNeighbours.Add(bMainArr[currCell.x + 1, currCell.y + 1]);
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (lNeighbours.Where(x => x == true).ToList().Count < 2)
                        {
                            currentGeneration[currCell.x, currCell.y] = false;
                        }
                        if (lNeighbours.Where(x => x == true).ToList().Count > 3)
                        {
                            currentGeneration[currCell.x, currCell.y] = false;
                        }

                        if (lNeighbours.Where(x => x == true).ToList().Count == 3)
                        {
                            currentGeneration[currCell.x, currCell.y] = true;
                        }
                        lNeighbours.Clear();
                    }
                }

                bMainArr = currentGeneration.Clone() as bool[,];
                if (checkBerserk.Checked)
                {
                    if (generation % 1000 == 0)
                    {
                        InvokeUI(() =>
                        {
                            pictureBox1.Refresh();
                            lblGens.Text = "Generation: " + generation;
                            generation++;
                        });
                        Thread.Sleep(TimeSpan.FromSeconds(pseudoFramerate));
                    }
                    else
                    {
                        generation++;
                    }
                }
                else
                {
                    InvokeUI(() =>
                    {
                        pictureBox1.Refresh();
                        lblGens.Text = "Generation: " + generation;
                        generation++;
                    });
                    Thread.Sleep(TimeSpan.FromSeconds(pseudoFramerate));
                }
            }
        }

        private void InvokeUI(Action a)
        {
            while (!formShown)
            {
                Thread.Sleep(100);
            }
            this.BeginInvoke(new MethodInvoker(a));
        }

        private void InitializeArray()
        {
            bMainArr = new bool[xOffSet, yOffSet];
            //put Presets here
        }
        #endregion

        #region Presets
        private void startWithGlider()
        {
            bMainArr[3, 3] = true;
            bMainArr[3, 4] = true;
            bMainArr[3, 5] = true;
            bMainArr[2, 5] = true;
            bMainArr[1, 4] = true;
        }

        private void startWithPentomino()
        {
            bMainArr[20, 20] = true;
            bMainArr[21, 20] = true;
            bMainArr[20, 21] = true;
            bMainArr[19, 21] = true;
            bMainArr[20, 22] = true;
        }

        private void startWithEmpty(int xOffsetLokal = 0, int yOffsetLokal = 0)
        {
            bMainArr[30 - xOffsetLokal, 8 - yOffsetLokal] = true;
            bMainArr[30 - xOffsetLokal, 9 - yOffsetLokal] = true;
            bMainArr[30 - xOffsetLokal, 10 - yOffsetLokal] = true;

            bMainArr[30 - xOffsetLokal, 12 - yOffsetLokal] = true;
            bMainArr[30 - xOffsetLokal, 13 - yOffsetLokal] = true;
            bMainArr[30 - xOffsetLokal, 14 - yOffsetLokal] = true;

            bMainArr[31 - xOffsetLokal, 8 - yOffsetLokal] = true;
            bMainArr[31 - xOffsetLokal, 14 - yOffsetLokal] = true;

            bMainArr[32 - xOffsetLokal, 8 - yOffsetLokal] = true;
            bMainArr[32 - xOffsetLokal, 9 - yOffsetLokal] = true;
            bMainArr[32 - xOffsetLokal, 10 - yOffsetLokal] = true;

            bMainArr[32 - xOffsetLokal, 12 - yOffsetLokal] = true;
            bMainArr[32 - xOffsetLokal, 13 - yOffsetLokal] = true;
            bMainArr[32 - xOffsetLokal, 14 - yOffsetLokal] = true;
        }
        #endregion

        #region Event-Methods
        private void Form1_Shown(object sender, EventArgs e)
        {
            formShown = true;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            for (int dim1 = 0; dim1 < bMainArr.GetLength(0); dim1++)
            {
                for (int dim2 = 0; dim2 < bMainArr.GetLength(1); dim2++)
                {
                    if (bMainArr[dim1, dim2])
                    {
                        SolidBrush brush = new SolidBrush(cellColor);
                        Rectangle rec = new Rectangle(dim1 * xOffSet / resolutionFaktor, dim2 * yOffSet / resolutionFaktor, xOffSet / resolutionFaktor, yOffSet / resolutionFaktor);
                        e.Graphics.FillRectangle(brush, rec);
                    }
                }
            }

            ControlPaint.DrawGrid(e.Graphics, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height), new Size(1, yOffSet / resolutionFaktor), Color.White);
            ControlPaint.DrawGrid(e.Graphics, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height), new Size(xOffSet / resolutionFaktor, 1), Color.White);           
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            formShown = false;
            e.Cancel = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (lblGens.Text == "Generation: 0")
            {
                initialState = bMainArr.Clone() as bool[,];
            }

            if (!running)
            {
                running = true;
                Resume();
                btnStart.Text = "Stop";
            }
            else
            {
                running = false;
                Pause();
                btnStart.Text = "Start";
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            int xArr = e.X / (xOffSet / resolutionFaktor);
            int yArr = e.Y / (yOffSet / resolutionFaktor);
            bMainArr[xArr, yArr] = !bMainArr[xArr, yArr];
            pictureBox1.Refresh();
        }

        private void btnClearGrid_Click(object sender, EventArgs e)
        {
            if (running)
            {
                running = false;
                Pause();
                btnStart.Text = "Start";
            }
            bMainArr = new bool[xOffSet, yOffSet];
            initialState = new bool[xOffSet, yOffSet];
            pictureBox1.Refresh();
            generation = 1;
            lblGens.Text = "Generation: 0";
        }

        private void btnResetGrid_Click(object sender, EventArgs e)
        {
            if (running)
            {
                running = false;
                Pause();
                btnStart.Text = "Start";
            }
            bMainArr = initialState;
            pictureBox1.Refresh();
            generation = 1;
            lblGens.Text = "Generation: 0";
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            switch ((sender as NumericUpDown).Value)
            {
                case 1:
                    pseudoFramerate = 0.7;
                    break;
                case 2:
                    pseudoFramerate = 0.5;
                    break;
                case 3:
                    pseudoFramerate = 0.3;
                    break;
                case 4:
                    pseudoFramerate = 0.1;
                    break;
                case 5:
                    pseudoFramerate = 0.05;
                    break;
                case 6:
                    pseudoFramerate = 0.04;
                    break;
                case 7:
                    pseudoFramerate = 0.03;
                    break;
                case 8:
                    pseudoFramerate = 0.01;
                    break;
                case 9:
                    pseudoFramerate = 0.007;
                    break;
                case 10:
                    pseudoFramerate = 0.005;
                    break;
                default:
                    pseudoFramerate = 0.1;
                    break;
            }
        }
        #endregion

        private void btnSingleStep_Click(object sender, EventArgs e)
        {
            bool[,] currentGeneration = bMainArr.Clone() as bool[,];

            for (int dim1 = 0; dim1 < bMainArr.GetLength(1); dim1++)
            {
                for (int dim2 = 0; dim2 < bMainArr.GetLength(0); dim2++)
                {
                    Cell currCell = new Cell() { living = bMainArr[dim1, dim2], x = dim2, y = dim1 };
                    List<bool> lNeighbours = new List<bool>();
                    List<int> lBlocksToCheck = new List<int>() { 1, 2, 3, 4, 6, 7, 8, 9 };
                    #region getNeighbours
                    if (dim1 == 0)
                    {
                        lBlocksToCheck.Remove(1);
                        lBlocksToCheck.Remove(2);
                        lBlocksToCheck.Remove(3);
                        if (dim2 == 0)
                        {
                            lBlocksToCheck.Remove(4);
                            lBlocksToCheck.Remove(7);
                        }
                        if (dim2 == xOffSet - 1)
                        {
                            lBlocksToCheck.Remove(6);
                            lBlocksToCheck.Remove(9);
                        }
                    }
                    if (dim1 == yOffSet - 1)
                    {
                        lBlocksToCheck.Remove(7);
                        lBlocksToCheck.Remove(8);
                        lBlocksToCheck.Remove(9);
                        if (dim2 == 0)
                        {
                            lBlocksToCheck.Remove(1);
                            lBlocksToCheck.Remove(4);
                        }
                        if (dim2 == xOffSet - 1)
                        {
                            lBlocksToCheck.Remove(3);
                            lBlocksToCheck.Remove(6);
                        }
                    }
                    if (dim1 > 0 && dim1 < yOffSet - 1)
                    {
                        if (dim2 == 0)
                        {
                            lBlocksToCheck.Remove(1);
                            lBlocksToCheck.Remove(4);
                            lBlocksToCheck.Remove(7);
                        }
                        if (dim2 == xOffSet - 1)
                        {
                            lBlocksToCheck.Remove(3);
                            lBlocksToCheck.Remove(6);
                            lBlocksToCheck.Remove(9);
                        }
                    }
                    #endregion

                    foreach (int i in lBlocksToCheck)
                    {
                        switch (i)
                        {
                            case 1:
                                lNeighbours.Add(bMainArr[currCell.x - 1, currCell.y - 1]);
                                break;
                            case 2:
                                lNeighbours.Add(bMainArr[currCell.x, currCell.y - 1]);
                                break;
                            case 3:
                                lNeighbours.Add(bMainArr[currCell.x + 1, currCell.y - 1]);
                                break;
                            case 4:
                                lNeighbours.Add(bMainArr[currCell.x - 1, currCell.y]);
                                break;
                            case 6:
                                lNeighbours.Add(bMainArr[currCell.x + 1, currCell.y]);
                                break;
                            case 7:
                                lNeighbours.Add(bMainArr[currCell.x - 1, currCell.y + 1]);
                                break;
                            case 8:
                                lNeighbours.Add(bMainArr[currCell.x, currCell.y + 1]);
                                break;
                            case 9:
                                lNeighbours.Add(bMainArr[currCell.x + 1, currCell.y + 1]);
                                break;
                            default:
                                break;
                        }
                    }

                    if (lNeighbours.Where(x => x == true).ToList().Count < 2)
                    {
                        currentGeneration[currCell.x, currCell.y] = false;
                    }
                    if (lNeighbours.Where(x => x == true).ToList().Count > 3)
                    {
                        currentGeneration[currCell.x, currCell.y] = false;
                    }

                    if (lNeighbours.Where(x => x == true).ToList().Count == 3)
                    {
                        currentGeneration[currCell.x, currCell.y] = true;
                    }
                    lNeighbours.Clear();
                }
            }

            bMainArr = currentGeneration.Clone() as bool[,];

            pictureBox1.Refresh();
            lblGens.Text = "Generation: " + generation;
            generation++;
            Thread.Sleep(TimeSpan.FromSeconds(pseudoFramerate));
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            cellColor = colorDialog1.Color;
            pictureBox1.Refresh();
        }
    }
}