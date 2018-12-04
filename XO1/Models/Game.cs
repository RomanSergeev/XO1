using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XO1.Models
{
    [Table("Game")]
    public class Game
    {
        public const byte MIN_SIZE = 3;
        public const byte MAX_SIZE = 10;
        public enum STATE { EMPTY, X, O };

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int id { get; set; }
        public byte lines { get; private set; }
        public byte cols { get; private set; }
        public byte winLength { get; private set; }
        public bool playerMoveFirst { get; private set; }
        public STATE[][] field { get; private set; }

        public Game()
        {
            lines = MIN_SIZE;
            cols = MIN_SIZE;
            winLength = MIN_SIZE;
            playerMoveFirst = true;
        }

        // how to call this constructor automatically from view form?
        public Game(byte lines, byte cols, byte winLength, bool playerMoveFirst)
        {
            this.lines = lines;
            this.cols = cols;
            this.winLength = winLength;
            this.playerMoveFirst = playerMoveFirst;
            field = new STATE[lines][];
            for (int i = 0; i < lines; i++)
            {
                field[i] = new STATE[cols];
                for (int j = 0; j < cols; j++)
                {
                    field[i][j] = STATE.EMPTY;
                }
            }
        }
        

        public bool makeMove(STATE state, byte i, byte j)
        {
            if (!cellIsAccessable(i, j) || field[i][j] != STATE.EMPTY) return false;
            field[i][j] = state;
            return true;
        }

        public bool cellIsAccessable(byte i, byte j)
        {
            return 0 <= i && i <= lines && 0 <= j && j <= cols;
        }
    }
}