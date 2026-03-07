using UnityEngine;
using TMPro;
using System.Text;

public class MatrixBitWriter : MonoBehaviour
{
    public TMP_Text textArea;

    public int columns = 60;
    public int rows = 20;

    public float rowFillInterval = 0.05f;

    private char[,] buffer;
    private bool[,] mask;

    private int currentRow;
    private float timer;

    private bool finished;

    private StringBuilder sb;

    public System.Action OnFinished;

    void Start()
    {
        buffer = new char[rows, columns];
        mask = new bool[rows, columns];
        sb = new StringBuilder(rows * (columns + 5));

        // Başlangıçta tüm buffer boş
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                buffer[r, c] = ' ';

        currentRow = rows - 1;

        GenerateMask();
        Render();
    }

    void Update()
    {
        if (finished)
            return;

        timer += Time.unscaledDeltaTime;

        if (timer >= rowFillInterval)
        {
            timer = 0f;

            FillRow(currentRow);
            Render();

            currentRow--;

            if (currentRow < 0)
            {
                finished = true;
                OnFinished?.Invoke();
            }
        }
    }

    void FillRow(int r)
    {
        for (int c = 0; c < columns; c++)
        {
            if (mask[r, c])
                buffer[r, c] = Random.value > 0.8f ? '1' : '0';
            else
                buffer[r, c] = ' '; // mask dışı tamamen boş
        }
    }

    void Render()
    {
        sb.Clear();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (mask[r, c])
                    sb.Append("<color=#FFDD55>").Append(buffer[r, c]).Append("</color>");
                else
                    sb.Append(" "); // mask dışı boş
            }
            sb.Append('\n');
        }

        textArea.text = sb.ToString();
    }

    void GenerateMask()
    {
        string[] lines = new string[]
        {
            "FFF  I  X X    TTT  H  H  EEEE     I  N  N  DDD  EEEE  X X",
            "F    I  X X     T   H  H  E        I  NN N  D  D E     X X",
            "FFF  I   X      T   HHHH  EEE      I  N NN  D  D EEE    X ",
            "F    I  X X     T   H  H  E        I  N  N  D  D E     X X",
            "F    I  X X     T   H  H  EEEE     I  N  N  DDD  EEEE  X X"
        };

        int startRow = 7;
        int startCol = 2;

        for (int r = 0; r < lines.Length; r++)
        {
            for (int c = 0; c < lines[r].Length; c++)
            {
                if (startRow + r < rows && startCol + c < columns)
                {
                    if (lines[r][c] != ' ')
                        mask[startRow + r, startCol + c] = true;
                }
            }
        }
    }
}
