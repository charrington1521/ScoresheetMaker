
public class ChessBoard
{
    public static readonly int BOARD_WIDTH  = 8;
    public static readonly int BOARD_HEIGHT = 8;

    public enum Color
    {
        WHITE,
        BLACK
    }
    public enum File 
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H
    }
    public enum ChessPiece 
    {
        WHITE_PAWN = 'p',
        WHITE_KNIGHT = 'n',
        WHITE_BISHOP = 'b',
        WHITE_ROOK = 'r',
        WHITE_QUEEN = 'q',
        WHITE_KING = 'k',
        BLACK_PAWN = 'P',
        BLACK_KNIGHT = 'N',
        BLACK_BISHOP = 'B',
        BLACK_ROOK = 'R',
        BLACK_QUEEN = 'Q',
        BLACK_KING = 'K',
        NO_PIECE = '0',
        SOME_PIECE = '1'
    }
    ChessPiece[] _chessBoard = {ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE,
                                ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE,
                                ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE,
                                ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE,
                                ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE,
                                ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE,
                                ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE,
                                ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE, ChessPiece.NO_PIECE };

    public void startingPosition()
    {
        setPosition(1, File.E, ChessPiece.WHITE_KING);
        setPosition(1, File.H, ChessPiece.WHITE_ROOK);
        setPosition(2, File.E, ChessPiece.WHITE_PAWN);

        setPosition(8, File.E, ChessPiece.BLACK_KING);
        setPosition(8, File.H, ChessPiece.BLACK_ROOK);
        setPosition(7, File.E, ChessPiece.BLACK_PAWN);
        // setPosition(1, File.G, ChessPiece.WHITE_KNIGHT);
        // setPosition(8, File.G, ChessPiece.BLACK_KNIGHT);


        // setPosition(1, File.A, ChessPiece.WHITE_ROOK);
        // setPosition(1, File.B, ChessPiece.WHITE_KNIGHT);
        // setPosition(1, File.C, ChessPiece.WHITE_BISHOP);
        // setPosition(1, File.D, ChessPiece.WHITE_QUEEN);
        // setPosition(1, File.E, ChessPiece.WHITE_KING);
        // setPosition(1, File.F, ChessPiece.WHITE_BISHOP);
        // setPosition(1, File.G, ChessPiece.WHITE_KNIGHT);
        // setPosition(1, File.H, ChessPiece.WHITE_ROOK);

        // for (int col = 0; col < BOARD_WIDTH; col++)
        // {
        //     setPosition(2, (File)col, ChessPiece.WHITE_PAWN);
        // }

        // for (int col = 0; col < BOARD_WIDTH; col++)
        // {
        //     setPosition(7, (File)col, ChessPiece.BLACK_PAWN);
        // }

        // setPosition(8, File.A, ChessPiece.BLACK_ROOK);
        // setPosition(8, File.B, ChessPiece.BLACK_KNIGHT);
        // setPosition(8, File.C, ChessPiece.BLACK_BISHOP);
        // setPosition(8, File.D, ChessPiece.BLACK_QUEEN);
        // setPosition(8, File.E, ChessPiece.BLACK_KING);
        // setPosition(8, File.F, ChessPiece.BLACK_BISHOP);
        // setPosition(8, File.G, ChessPiece.BLACK_KNIGHT);
        // setPosition(8, File.H, ChessPiece.BLACK_ROOK);

    }

    public void setPosition(int row, File col, ChessPiece piece)
    {
        _chessBoard[(row - 1) * BOARD_WIDTH + (int)col] = piece;
    }

    public ChessPiece getPosition(int row, File col)
    {
        return _chessBoard[(row - 1) * BOARD_WIDTH + (int)col];
    }

    public ChessBoard Clone()
    {
        ChessBoard newBoard = new ChessBoard();
        newBoard._chessBoard = (ChessPiece[])this._chessBoard.Clone();
        return newBoard;
    }

    public string ToString()
    {
        string toReturn = "";
        for (int row = 1; row <= BOARD_HEIGHT; row++)
        {
            for (int col = 0; col < BOARD_WIDTH; col++)
            {
                toReturn += pieceAsString(getPosition(row, (ChessBoard.File)col));
            }
            toReturn += "\n";
        }
        return toReturn;
    }

    public static bool Equals(ChessBoard board1, ChessBoard board2)
    {
        if (board1 == board2)
        {
            return true;
        }
        
        return board1._chessBoard.SequenceEqual(board2._chessBoard);
    }

    public static bool isEmpty(ChessBoard board)
    {
        for (int row = 1; row <= ChessBoard.BOARD_HEIGHT; row++)
        {
            for (int col = 0; col < BOARD_WIDTH; col++)
            {   
                if (isPiece(board.getPosition(row, (File)col)))
                {
                    return false;
                }
            }
        }
        return true;
    }
    public static bool isColor(ChessPiece piece, Color color)
    {
        if ((char)piece < 'a')
        {
            if (color == Color.WHITE)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            if (color == Color.WHITE) 
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static bool isPiece(ChessPiece piece)
    {
        return piece != ChessPiece.NO_PIECE;
    }
    public static string generateMoveString(int originRow, File originCol, int destRow, File destCol, ChessPiece piece, bool isCapture, int moveNumber)
    {
        string toReturn = "";
        if (isColor(piece, Color.WHITE))
        {
            toReturn += moveNumber + ". ";
        }
        else
        {
            //
        }
        if (! (piece == ChessPiece.WHITE_PAWN || piece == ChessPiece.BLACK_PAWN))
        {
            toReturn += pieceAsString(piece).ToUpper();
        }
        toReturn += fileAsString(originCol);
        toReturn += originRow.ToString();
        if (isCapture)
        {
            toReturn += "x";
        }
        toReturn += fileAsString(destCol);
        toReturn += destRow.ToString();
        return toReturn += " ";
    }

    public static bool canPieceCastle(ChessPiece piece)
    {
        return  piece == ChessPiece.BLACK_KING || piece == ChessPiece.WHITE_KING ||
                piece == ChessPiece.BLACK_ROOK || piece == ChessPiece.WHITE_ROOK;
    }

    public static string fileAsString(File col)
    {
        switch(col)
        {
            case File.A:
                return "a";
            case File.B:
                return "b";
            case File.C:
                return "c";
            case File.D:
                return "d";
            case File.E:
                return "e";
            case File.F:
                return "f";
            case File.G:
                return "g";
            case File.H:
                return "h";
            default:
                return "";
        }
    }
    public static string pieceAsString(ChessPiece piece)
    {
        switch(piece)
        {
            case ChessPiece.WHITE_PAWN: 
                return "p";
            case ChessPiece.BLACK_PAWN:
                return "P";
            case ChessPiece.WHITE_KNIGHT: 
                return "n";
            case ChessPiece.BLACK_KNIGHT:
                return "N";
            case ChessPiece.WHITE_BISHOP: 
                return "b";
            case ChessPiece.BLACK_BISHOP:
                return "B";
            case ChessPiece.WHITE_ROOK: 
                return "r";
            case ChessPiece.BLACK_ROOK:
                return "R";
            case ChessPiece.WHITE_QUEEN: 
                return "q";
            case ChessPiece.BLACK_QUEEN:
                return "Q";
            case ChessPiece.WHITE_KING: 
                return "k";
            case ChessPiece.BLACK_KING:
                return "K";
            case ChessPiece.NO_PIECE:
                return "0";
            case ChessPiece.SOME_PIECE:
                return "1";
            default:
                return "";
        }
    }

}