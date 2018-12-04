using System;
using System.Collections.Generic;
using XO1.Models;

namespace XO1.Services
{
    // should it have Game as a field or as a parameter to every function?
    public class GameService
    {
        public static char sign(Game.STATE state)
        {
            switch (state)
            {
                case Game.STATE.EMPTY: return ' ';
                case Game.STATE.X: return 'X';
                case Game.STATE.O: return 'O';
                default: return ' ';
            }
        }

        /**
         * two two-dimensional arrays of strategyInfo are used in AI decision making
         */
        private struct StrategyInfo : IComparable
        {
            public byte line, col;
            public List<byte> winOuts;
            public StrategyInfo(byte line, byte col)
            {
                this.line = line;
                this.col = col;
                winOuts = new List<byte>();
            }

            public int CompareTo(object other)
            {
                if (other == null) return 1;
                if (other.GetType() != typeof(StrategyInfo)) throw new ArgumentException("Tried to compare StrategyInfo to other class object");
                StrategyInfo si = (StrategyInfo)other;

                if (winOuts.Count == 0) return 1;  // empty lists should sink to bottom (have greatest index)
                if (si.winOuts.Count == 0) return -1;
                for (int i = 0; i < Math.Min(winOuts.Count, si.winOuts.Count); i++)
                {
                    if (winOuts[i] > si.winOuts[i]) return 1;
                    if (winOuts[i] < si.winOuts[i]) return -1;
                }
                if (winOuts.Count < si.winOuts.Count) return 1;
                if (winOuts.Count > si.winOuts.Count) return -1;
                return 0;
            }
        }

        public class StepAfterVictoryException : Exception
        {
            public StepAfterVictoryException(string message) : base(message) { }
        }

        private Game game;
        private StrategyInfo[][][] AIData;
        private Game.STATE AIState;
        private Game.STATE[] bothStates;
        private int[][] bypassConfigurations;
        //private Game.STATE winner;

        public GameService() { }

        public void setup(Game game)
        {
            this.game = game;
            int l = game.lines, c = game.cols;
            int wl = game.winLength;

            AIData = new StrategyInfo[2][][];
            AIData[0] = new StrategyInfo[l][];
            AIData[1] = new StrategyInfo[l][];
            for (byte i = 0; i < l; i++)
            {
                AIData[0][i] = new StrategyInfo[c];
                AIData[1][i] = new StrategyInfo[c];
                for (byte j = 0; j < c; j++)
                {
                    AIData[0][i][j] = new StrategyInfo(i, j);
                    AIData[1][i][j] = new StrategyInfo(i, j);
                }
            }
            AIState = game.playerMoveFirst ? Game.STATE.O : Game.STATE.X;
            bothStates = new Game.STATE[] { AIState, AIState == Game.STATE.X ? Game.STATE.O : Game.STATE.X };

            bypassConfigurations = new int[4][];
            bypassConfigurations[0] = new int[] { 0, l, 0, c - wl + 1, 0, 1 };  // horizontal check
            bypassConfigurations[1] = new int[] { 0, l - wl + 1, 0, c, 1, 0 };  // vertical check
            bypassConfigurations[2] = new int[] { 0, l - wl + 1, 0, c - wl + 1, 1, 1 };  // diagonal NW - SE check
            bypassConfigurations[3] = new int[] { 0, l - wl + 1, wl - 1, c, 1, -1 };  // diagonal NE - SW check (reason why this array exists at all)
            //winner = Game.STATE.EMPTY;
        }

        /**
         * Strategy is following:
         * For each empty cell, construct offensive and defensive lists containing AI's and player's possible win outs (steps to win).
         * Order each of lists ascendingly.
         * Sort array of lists in ascending order: the more smallest values present, the lower list's index will be.
         * List (1, 1, 1, 2, 2, 3, 3) will have index less than list (1, 1, 1, 2, 3, 3, 4, 4) because first has two 2s and second has one.
         * If defenceData[0][0]. is less than our list's[0][0] then step to that cell
         * If values are equal then step to our cell
         * If we have more than one cell like lists[0][0], then {current version, faster, but probably less effective}
         * Just choose 
         * 
         * Strategy is ideal (should be at least)
         * Strategy uses O(lines^2 * cols^2) memory and O(gamesize^2) time (worst case)
         */
        public void AIMove(out byte line, out byte col)
        {
            //if (winner != Game.STATE.EMPTY) throw new StepAfterVictoryException("Tried to make move on a finished game board");
            byte i, j, k, stateIndex;
            int steps;

            for (i = 0; i < game.lines; i++)
                for (j = 0; j < game.cols; j++)
                {
                    AIData[0][i][j].winOuts.Clear();
                    AIData[1][i][j].winOuts.Clear();
                }
            int[] tmpptr;  // just for code truncate
            int idx;
            for (idx = 0, tmpptr = bypassConfigurations[idx]; idx < 4; idx++)
                for (i = (byte)tmpptr[0]; i < (byte)tmpptr[1]; i++)
                    for (j = (byte)tmpptr[2]; j < (byte)tmpptr[3]; j++)
                        for (stateIndex = 0; stateIndex < 2; stateIndex++)
                            if ((steps = stepsToWin(bothStates[stateIndex], i, j, tmpptr[4], tmpptr[5])) != -1)
                                for (k = 0; k < game.winLength; k++)
                                    if (game.field[i + k * tmpptr[4]][j + k * tmpptr[5]] != Game.STATE.EMPTY)
                                        AIData[stateIndex][i + k * tmpptr[4]][j + k * tmpptr[5]].winOuts.Add((byte)steps);

            StrategyInfo[,] multiDimAI = new StrategyInfo[game.lines, game.cols];
            StrategyInfo[,] multiDimHuman = new StrategyInfo[game.lines, game.cols];

            for (i = 0; i < game.lines; i++)
                for (j = 0; j < game.cols; j++)
                {
                    AIData[0][i][j].winOuts.Sort();
                    AIData[1][i][j].winOuts.Sort();
                    multiDimAI[i, j] = AIData[0][i][j];
                    multiDimHuman[i, j] = AIData[1][i][j];
                }

            Array.Sort(multiDimAI);
            Array.Sort(multiDimHuman);

            if (multiDimAI[0, 0].winOuts[0] > multiDimHuman[0, 0].winOuts[0])
            {
                line = multiDimHuman[0, 0].line;
                col = multiDimHuman[0, 0].col;
                return;
            }
            line = multiDimAI[0, 0].line;
            col = multiDimAI[0, 0].col;
            game.makeMove(AIState, line, col);
        }

        public Game.STATE victory()
        {
            int idx, stateIndex;
            int[] tmpptr;
            for (idx = 0, tmpptr = bypassConfigurations[idx]; idx < 4; idx++)
                for (byte i = (byte)tmpptr[0]; i < (byte)tmpptr[1]; i++)
                    for (byte j = (byte)tmpptr[2]; j < (byte)tmpptr[3]; j++)
                        for (stateIndex = 0; stateIndex < 2; stateIndex++)
                            if (stepsToWin(bothStates[stateIndex], i, j, tmpptr[4], tmpptr[5]) == 0)
                                return bothStates[stateIndex];
            return Game.STATE.EMPTY;
        }

        /**
         * In given direction (both shiftX and shiftY are -1, 0 or 1) counts steps to build winning composition for given state.
         * If it's impossible than returns -1
         */
        private int stepsToWin(Game.STATE state, byte x, byte y, int shiftX, int shiftY)
        {
            int result = game.winLength;
            for (byte i = 1; i < game.winLength; i++)
            {
                if (game.field[x + shiftX * i][y + shiftY * i] != state) return -1;
                if (game.field[x + shiftX * i][y + shiftY * i] == Game.STATE.EMPTY) result--;
            }
            return result;
        }
    }
}