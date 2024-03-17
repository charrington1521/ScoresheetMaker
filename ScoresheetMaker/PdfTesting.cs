using Aspose.Pdf;
using Aspose.Pdf.Forms;
using Aspose.Pdf.Operators;
//using Aspose.Pdf.Plugins;
using Aspose.Pdf.Text;
using Microsoft.AspNetCore.Authentication;
using System.Diagnostics.Tracing;
using System.IO.Ports;
using System.IO;
using System.Text;
using Aspose.Pdf.LogicalStructure;
using Aspose.Pdf.Facades;
using System.Runtime.CompilerServices;
using Aspose.Pdf.Annotations;

public static class PdfTesting
{
  
    static Serial serialPort = new Serial();
        
    static ChessBoard _chessBoard = new ChessBoard();

    static ChessBoard.Color _playerTurn = ChessBoard.Color.WHITE;

    static List<ChessBoard>? _boards;
   
    static int _moveNumber = 1;

    static bool _hasIllegalMove = false;
   
    private static readonly string _dataDir = "wwwroot/documents"; 

    private static readonly string _pgnPath = Path.Combine(_dataDir, "output.pgn");

    private static readonly string _outputFileName = Path.Combine(_dataDir, "outputScoreSheet.pdf");
   
    private static Document? _outputPdf;

    private static FileSystemWatcher? _pgnWatcher;
   
    private static int _boardNum = 0;

    private static int _rowNum = 1;

    private static string _message = "";

    private static bool _whiteHasCastlingRightsA = true;
    private static bool _whiteHasCastlingRightsH = true;
    private static bool _blackHasCastlingRightsA = true;
    private static bool _blackHasCastlingRightsH = true;


    public static void HelloWorld()
    {
        bool _continue = true;


        createTemplateScoresheet();

        emptyPgn();

        _pgnWatcher = new FileSystemWatcher()
        {
            Path = _dataDir,
            Filter = "*.pgn",
            NotifyFilter = NotifyFilters.Attributes
                         | NotifyFilters.CreationTime
                         | NotifyFilters.DirectoryName
                         | NotifyFilters.FileName
                         | NotifyFilters.LastAccess
                         | NotifyFilters.LastAccess
                         | NotifyFilters.Security
                         | NotifyFilters.Size,
            EnableRaisingEvents = true,
        };
        _pgnWatcher.Changed += new FileSystemEventHandler(pgnChanged);
        _pgnWatcher.Created += new FileSystemEventHandler(pgnChanged);
        serialPort.SerialOutInit();

        _boards = new List<ChessBoard>(5);
        for (int i = 0; i < 5; i++)
        {
            _boards.Add(new ChessBoard());
        }

        _chessBoard.startingPosition();

        //mockInput();

        // Thread.Sleep(8000);
        // writeToPgn("1-0 ");
        // Thread.Sleep(5000);
        // writeToPgn("6. garbage ");
        // Thread.Sleep(5000);
        // writeToPgn("more garbage ");

        _boardNum = 0;
        _rowNum = 1;
         _boards[0] = _chessBoard.Clone();
        while (_continue) {
            try
            {
                _message = serialPort.readLineBlocking();
                // Console.WriteLine(_message);
                //writeToPgn(_message);
                if (string.Compare(" ", _message) == 0) {
                    if (_rowNum <= ChessBoard.BOARD_HEIGHT)
                    {
                        disposeOfRead();
                    } 
                    else if (_boardNum > 0 && ChessBoard.Equals(_boards[_boardNum], _boards[_boardNum - 1]))
                    {
                    }
                    else
                    {
                        Console.WriteLine("New Board");
                        Console.WriteLine(_boards[_boardNum].ToString());
                        _boardNum ++;
                    }
                    _rowNum = 1;
                } else if (string.Compare("Turn changed", _message) == 0) {
                    processBoards();
                    if (_playerTurn == ChessBoard.Color.WHITE)
                    {
                        _playerTurn = ChessBoard.Color.BLACK;
                    }
                    else
                    {
                        _playerTurn = ChessBoard.Color.WHITE;
                    }
                    _boardNum = 0;
                    _rowNum = 1;
                } else if (string.Compare("Promotion", 0, _message, 0, 9) == 0) {
                } else if (string.Compare("Game Result", 0, _message, 0, 11) == 0) {
                    if (_playerTurn == ChessBoard.Color.WHITE)
                    {
                        writeToPgn(_moveNumber + ". ");
                    }
                    writeToPgn(_message.Substring(12, _message.Length - 12));
                    _continue = false;
                } else if (string.Compare("quit", _message) == 0) {
                    processBoards();
                    _continue = false;
                } else {
                    if (_message.Length == 8)
                    {
                        if (_rowNum > ChessBoard.BOARD_HEIGHT)
                        {
                            disposeOfRead();
                        }
                        else
                        {
                            postMessageToBoard(_boardNum, _rowNum, _message);
                            _rowNum++;
                        }
                    } 
                    else
                    {
                        disposeOfRead();
                    }
                }
            }
            catch (TimeoutException) {}
        }

        Console.WriteLine("Complete");
    }

    static private void disposeOfRead()
    {
        Console.WriteLine("incomplete");
        // Console.WriteLine(_message);
        _boards[_boardNum] = new ChessBoard();
        _rowNum = 1;
    }
    static private void createTemplateScoresheet()
    {
        emptyPdf();

        _outputPdf = new Document();
        Page scoresheet = _outputPdf.Pages.Add();

        Table table = new Table()
        {
            Border = new BorderInfo(BorderSide.All, .5f, Color.Black),
            DefaultCellBorder = new BorderInfo(BorderSide.All, .5f, Color.LightGray)
        };

        insertScoreSheetHeader(table);

        for (int i = 0; i < 25; i++) {
            Row row = table.Rows.Add();

            row.Cells.Add(" ").Alignment = HorizontalAlignment.Center;
            row.Cells.Add(" ").Alignment = HorizontalAlignment.Center;
            row.Cells.Add(" ").Alignment = HorizontalAlignment.Center;
            row.Cells.Add(" ").Alignment = HorizontalAlignment.Center;
        }
        
        scoresheet.Paragraphs.Add(table);
        
        _outputPdf.Save(_outputFileName);
        
        addTextFields(_outputFileName);        
    }

    static private void pgnChanged(object sender, FileSystemEventArgs e)
    {
        if (_pgnWatcher != null)
        {
            _pgnWatcher.EnableRaisingEvents = false;
            writePgnToPdf(_outputPdf);
            _outputPdf.Save(_outputFileName);
            addTextFields(_outputFileName); //How to ensure text fields are not deleted. . .
            _pgnWatcher.EnableRaisingEvents = true;
        }
    }

    private static void writePgnToPdf(Document pdf)
    {
        TableAbsorber absorber = new TableAbsorber();

        absorber.Visit(pdf.Pages[1]);

        AbsorbedTable table = absorber.TableList[0];

        using (FileStream pgnFs = File.Open(_pgnPath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            int moveNumber = 0;
            int movesAdded = 0;
            char[]? token;
            AbsorbedRow row = table.RowList[0];
            do
            {
                token = nextToken(pgnFs);
                if (token != null)
                {
                    string? tokenText = new string(token);
                    if (tokenText != null)
                    {
                        if (tokenText.Contains('.'))
                        {
                            int index = (++moveNumber % table.RowList.Count) + 3;
                            row = table.RowList[index];
                        }
                        else
                        {
                            int index = 2 * moveNumber / table.RowList.Count + movesAdded++ % 2;

                            row.CellList[index].TextFragments[1].Text = tokenText;
                        }
                    }
                }    
            } while (token != null);
        }
    }

    private static void insertScoreSheetHeader(Table table)
    { // Could still make the top rows larger ?
        Cell cellToAdd; 
        BorderInfo border;
        
        Row row = table.Rows.Add();
        
        cellToAdd = row.Cells.Add("Event");
        cellToAdd.ColSpan = 2;
        // cellToAdd.RowSpan = 2;
        border = new BorderInfo(BorderSide.All, Color.Black)
        {
            Top = null,
            Left = null
        };
        cellToAdd.Border = border;

        cellToAdd = row.Cells.Add("Date");
        cellToAdd.ColSpan = 2;
        // cellToAdd.RowSpan = 2;
        border = new BorderInfo(BorderSide.All, Color.Black)
        {
            Top = null,
            Right = null
        };
        cellToAdd.Border = border;

        // row = table.Rows.Add();
        
        // cellToAdd = row.Cells.Add(" ");
        // row.Cells.Add(cellToAdd);
        // cellToAdd.ColSpan = 4;
        // cellToAdd.Border = null;
        
        row = table.Rows.Add();

        row.Cells.Add("Round");
        row.Cells.Add("Board");
        row.Cells.Add("Section");
        row.Cells.Add("Opening");
        
        row = table.Rows.Add();
        Cell nameCell = row.Cells.Add("White (Name)");
        nameCell.ColSpan = 2;
        nameCell = row.Cells.Add("Black (name)");
        nameCell.ColSpan = 2;

        row = table.Rows.Add();
        row.Cells.Add("White");
        row.Cells.Add("Black");
        row.Cells.Add("White");
        row.Cells.Add("Black");
    }

    private static void addTextFields(String _outputFileName)
    {
        var editor = new FormEditor();
        editor.BindPdf(_outputFileName);
        editor.AddField(FieldType.Text, "Event"  , 1, 135, 759, 285, 769);
        editor.AddField(FieldType.Text, "Date"   , 1, 335, 759, 485, 769);

        editor.AddField(FieldType.Text, "Round"  , 1, 135, 758, 185, 748);
        editor.AddField(FieldType.Text, "Board"  , 1, 235, 758, 285, 748);
        editor.AddField(FieldType.Text, "Section", 1, 335, 758, 385, 748);
        editor.AddField(FieldType.Text, "Opening", 1, 435, 758, 485, 748);

        editor.AddField(FieldType.Text, "White"  , 1, 160, 747, 285, 737);
        editor.AddField(FieldType.Text, "Black"  , 1, 360, 747, 485, 737);

        editor.SetFieldLimit("Event", 20);
        editor.SetFieldLimit("Date", 20);
        editor.SetFieldLimit("Round", 20);
        editor.SetFieldLimit("Board", 20);
        editor.SetFieldLimit("Section", 20);
        editor.SetFieldLimit("Opening", 20);
        editor.SetFieldLimit("White", 20);
        editor.SetFieldLimit("Black", 20);

        editor.Save(_outputFileName);
    }

    private static void writeToPgn(String item) {
        using (FileStream fs = File.Open(_pgnPath, FileMode.Append, FileAccess.Write, FileShare.Read)) {
            Byte[] info = new UTF8Encoding(true).GetBytes(item);
            fs.Write(info, 0, info.Length);
        }
    }

    private static void emptyPgn() {
        using (FileStream pgnFs = File.Open(_pgnPath, FileMode.Truncate, FileAccess.Write, FileShare.Read)) {
            //?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
        }
    }

    private static void emptyPdf()
    {
        Document doc = new Document();
        doc.Pages.Add();
        doc.Pages[1].Paragraphs.Add(new TextFragment(""));
        doc.Save(_outputFileName);
    }

    //Could improve by allowing encodings as paramter. . .
    private static char[]? nextToken(FileStream fs)
    {
        char[] toReturn = new char[0];
        int toReturnLen = 0;
        if (fs.CanRead)
        {
            bool isDone = false;
            while (! isDone)
            {
                int readResult = fs.ReadByte();
                if (readResult == -1)
                {
                    if (toReturn.Length == 0)
                    {
                        toReturn = null;
                    }
                    isDone = true;
                }
                else
                {
                    char[] temp;
                    temp = new char[++toReturnLen];
                    Array.Copy(toReturn, temp, toReturnLen - 1);
                    
                    Byte[] byteArr = new byte[1];
                    byteArr[0] = (Byte)readResult;

                    new UTF8Encoding(true).GetChars(byteArr, 0, 1, temp, toReturnLen - 1);
                    
                    if (temp[toReturnLen - 1] == ' ')
                    {
                        isDone = true;
                    }
                    else
                    {
                        toReturn = temp;
                    }
                }
            }
        }
        else
        {
        }
        return toReturn;
    }

    private static void postMessageToBoard(int boardNum, int row, string message)
    {
        _boards.EnsureCapacity(boardNum + 1);
        while (_boards.Count() <= boardNum + 1)
        {
            _boards.Add(new ChessBoard());
        }
        char[] messageData = message.ToCharArray();
        for (int col = 0; col < ChessBoard.BOARD_WIDTH; col++)
        {
            ChessBoard.ChessPiece toAdd = ChessBoard.ChessPiece.NO_PIECE;
            if (messageData[col] == '1') {
                toAdd = ChessBoard.ChessPiece.SOME_PIECE;
            }
            _boards[boardNum].setPosition(row, (ChessBoard.File)col, toAdd);
        }
    }

    private static void processBoards() {
        ChessBoard prevBoard = _chessBoard.Clone();

        bool isProcessing = true;

        int boardIndex = 0;
        if (ChessBoard.Equals(_boards[0], prevBoard))
        {
            if (_boards.Count() > 0)
            {
                boardIndex = 1;
            }
            else
            {
                isProcessing = false;
            }
        }

        int originRow = 0;
        ChessBoard.File originFile = ChessBoard.File.A;
        int destRow = 0;
        ChessBoard.File destFile = ChessBoard.File.A;
        ChessBoard.ChessPiece piece = ChessBoard.ChessPiece.NO_PIECE;;
        bool isCapture = false;
        int captureRow = 0;
        ChessBoard.File captureFile = 0;

        bool isCastling = false;

        while (isProcessing)
        {
            for (int row = 1; row < ChessBoard.BOARD_HEIGHT + 1; row++)
            {
                for (int col = 0; col < ChessBoard.BOARD_WIDTH; col++)
                {
                    ChessBoard.ChessPiece newPieceAtLocation = _boards[boardIndex].getPosition(row, (ChessBoard.File)col);
                    ChessBoard.ChessPiece oldPieceAtLocation = prevBoard.getPosition(row, (ChessBoard.File)col);
                    if (ChessBoard.isPiece(newPieceAtLocation) || ChessBoard.isPiece(oldPieceAtLocation))
                    {

                    }
                    if ( ChessBoard.isPiece(newPieceAtLocation) != ChessBoard.isPiece(oldPieceAtLocation) ) 
                    {
                        if (ChessBoard.isPiece(oldPieceAtLocation))
                        {
                            if (ChessBoard.isColor(oldPieceAtLocation, _playerTurn))
                            {
                                if (piece != ChessBoard.ChessPiece.NO_PIECE)
                                {
                                    if (row == originRow && (ChessBoard.File)col == originFile)
                                    {
                                        //Then all we did earlier was lift and put the piece back
                                        //The new origin row will be accurate, next placement will be accurate
                                        piece = oldPieceAtLocation;
                                        originRow = row + 1;
                                        originFile = (ChessBoard.File)col;
                                    }
                                    else if (row == destRow && (ChessBoard.File)col == destFile)
                                    {
                                        //Then we are lifting up the piece we moved earlier
                                        //The origin square should not change
                                        //If the move earlierwas a captures, this will not handle the player placing the piece 
                                        //on a new square, that is illegal and will show up as something
                                    }
                                    else if (ChessBoard.canPieceCastle(piece)
                                            && ChessBoard.canPieceCastle(oldPieceAtLocation))
                                    {
                                        if (_playerTurn == ChessBoard.Color.WHITE)
                                        {
                                            if (_whiteHasCastlingRightsA)
                                            {
                                                if (originRow == 1 && originFile == ChessBoard.File.A ||
                                                    row == 1       && (ChessBoard.File)col == ChessBoard.File.A)
                                                {
                                                    isCastling = true;
                                                    _whiteHasCastlingRightsA = false;
                                                    _whiteHasCastlingRightsH = false;
                                                }
                                            }
                                            else if (_whiteHasCastlingRightsH)
                                            {
                                                if (originRow == 1 && originFile == ChessBoard.File.G ||
                                                    row == 1       && (ChessBoard.File)col == ChessBoard.File.G)
                                                {
                                                    isCastling = true;
                                                    _whiteHasCastlingRightsA = false;
                                                    _whiteHasCastlingRightsH = false;
                                                }
                                            }
                                            else
                                            {
                                                _hasIllegalMove = true;
                                            }
                                        }
                                        else if (_playerTurn == ChessBoard.Color.BLACK)
                                        {
                                            if (_blackHasCastlingRightsA)
                                            {
                                                if (originRow == 8 && originFile == ChessBoard.File.A ||
                                                    row == 8       && (ChessBoard.File)col == ChessBoard.File.A)
                                                {
                                                    isCastling = true;
                                                    _blackHasCastlingRightsA = false;
                                                    _blackHasCastlingRightsH = false;
                                                }
                                            }
                                            else if (_blackHasCastlingRightsH)
                                            {   
                                                if (originRow == 8 && originFile == ChessBoard.File.A ||
                                                    row == 8       && (ChessBoard.File)col == ChessBoard.File.A)
                                                {
                                                    isCastling = true;
                                                    _blackHasCastlingRightsA = false;
                                                    _blackHasCastlingRightsH = false;
                                                }
                                            }
                                            else
                                            {
                                                _hasIllegalMove = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //This is a double lift. . . the program is no longer qualified to understand what is happening
                                        _hasIllegalMove = true;
                                    }
                                }
                                else
                                {
                                    piece = oldPieceAtLocation;
                                    originRow = row;
                                    originFile = (ChessBoard.File)col;
                                }
                            }
                            else
                            {
                                isCapture = true;
                                captureRow = row;
                                captureFile = (ChessBoard.File)col;
                                //Does not necessitate the destination square, e.g. en passant
                            }
                            prevBoard.setPosition(row, (ChessBoard.File)col, ChessBoard.ChessPiece.NO_PIECE);

                        }
                        else 
                        {
                            destRow = row;
                            destFile = (ChessBoard.File)col;
                            if (isCapture)
                            {
                                if (destRow != captureRow || destFile != captureFile)
                                {
                                    Console.WriteLine(destRow + ", " + destFile);
                                    Console.WriteLine(captureRow + ", " + captureFile);
                                    _hasIllegalMove = true;
                                }
                            }
                        }
                    }
                }
            }
            boardIndex++;
            if (ChessBoard.isEmpty(_boards[boardIndex]))
            {
                isProcessing = false;
            };

        }
        if (destRow != 0)
        {
            prevBoard.setPosition(destRow, destFile, piece);
        }
        _chessBoard = prevBoard;
        if (_hasIllegalMove)
        {
            string  moveString = ChessBoard.generateMoveString(originRow, originFile, destRow, destFile, piece, isCapture, _moveNumber);
            writeToPgn(moveString.Substring(0, moveString.Length - 1)
                        + "!illegalMoveDetected ");
        }
        else
        {
            if (isCastling)
            {
                if (destFile > ChessBoard.File.D)
                {
                    writeToPgn("O-O ");

                }
                else if (destFile < ChessBoard.File.E)
                {
                    writeToPgn("O-O-O ");
                }
                //Check for illegal castling to mark the move. . .
            }
            else
            {
                writeToPgn(ChessBoard.generateMoveString(originRow, originFile, destRow, destFile, piece, isCapture, _moveNumber));
            }
        }
        if (_playerTurn == ChessBoard.Color.BLACK)
        {
            //Can detect the non move case and mark as illegal. . .
            _moveNumber++;
        }
                if (piece == ChessBoard.ChessPiece.WHITE_KING)
        {
            _whiteHasCastlingRightsA = false;
            _whiteHasCastlingRightsH = false;
        }
        if (piece == ChessBoard.ChessPiece.WHITE_ROOK)
        {
            if (originFile == ChessBoard.File.A)
            {
                _whiteHasCastlingRightsA = false;
            }
            if (originFile == ChessBoard.File.H)
            {
                _whiteHasCastlingRightsH = false;
            }
        }
        if (piece == ChessBoard.ChessPiece.BLACK_KING)
        {
            _blackHasCastlingRightsA = false;
            _blackHasCastlingRightsH = false;
        }
        if (piece == ChessBoard.ChessPiece.BLACK_ROOK)
        {
            if (originFile == ChessBoard.File.A)
            {
                _blackHasCastlingRightsA = false;
            }
            if (originFile == ChessBoard.File.H)
            {
                _blackHasCastlingRightsH = false;
            }
        }

        _boards.Clear();
    }
    private static void fillWithSomePieces(ChessBoard board)
    {
        for (int col = 0; col < ChessBoard.BOARD_WIDTH; col++)
        {
            board.setPosition(1, (ChessBoard.File)col, ChessBoard.ChessPiece.SOME_PIECE);
            board.setPosition(2, (ChessBoard.File)col, ChessBoard.ChessPiece.SOME_PIECE);
            board.setPosition(7, (ChessBoard.File)col, ChessBoard.ChessPiece.SOME_PIECE);
            board.setPosition(8, (ChessBoard.File)col, ChessBoard.ChessPiece.SOME_PIECE);
        }
    }
    private static void mockInput()
    {
            _boards.EnsureCapacity(4);
            _boards[0] = new ChessBoard();
            _boards[1] = new ChessBoard();
            _boards[2] = new ChessBoard();
            _boards[3] = new ChessBoard();

            fillWithSomePieces(_boards[0]);
            _boards[0].setPosition(2, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(4, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);

            processBoards();

            _playerTurn = ChessBoard.Color.BLACK;

            fillWithSomePieces(_boards[0]);
            _boards[0].setPosition(7, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(5, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);

            processBoards();

            _playerTurn = ChessBoard.Color.WHITE;
            fillWithSomePieces(_boards[0]);
            _boards[0].setPosition(2, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(4, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[0].setPosition(7, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(5, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[0].setPosition(1, ChessBoard.File.G, ChessBoard.ChessPiece.NO_PIECE);
            
            fillWithSomePieces(_boards[1]);
            _boards[1].setPosition(2, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(4, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[1].setPosition(7, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(5, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[1].setPosition(1, ChessBoard.File.G, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(3, ChessBoard.File.F, ChessBoard.ChessPiece.SOME_PIECE);

            processBoards();

            _playerTurn = ChessBoard.Color.BLACK;
            fillWithSomePieces(_boards[0]);
            _boards[0].setPosition(2, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(4, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[0].setPosition(7, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(5, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[0].setPosition(1, ChessBoard.File.G, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(1, ChessBoard.File.G, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(3, ChessBoard.File.F, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[0].setPosition(8, ChessBoard.File.B, ChessBoard.ChessPiece.NO_PIECE);
            
            fillWithSomePieces(_boards[1]);
            _boards[1].setPosition(2, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(4, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[1].setPosition(7, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(5, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[1].setPosition(1, ChessBoard.File.G, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(1, ChessBoard.File.G, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(3, ChessBoard.File.F, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[1].setPosition(8, ChessBoard.File.B, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(6, ChessBoard.File.C, ChessBoard.ChessPiece.SOME_PIECE);

            processBoards();

            _playerTurn = ChessBoard.Color.WHITE;

            fillWithSomePieces(_boards[0]);
            _boards[0].setPosition(2, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(4, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[0].setPosition(7, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(5, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[0].setPosition(1, ChessBoard.File.G, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(1, ChessBoard.File.G, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(3, ChessBoard.File.F, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[0].setPosition(8, ChessBoard.File.B, ChessBoard.ChessPiece.NO_PIECE);
            _boards[0].setPosition(6, ChessBoard.File.C, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[0].setPosition(5, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);

            fillWithSomePieces(_boards[1]);
            _boards[1].setPosition(2, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(4, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[1].setPosition(7, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(5, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[1].setPosition(1, ChessBoard.File.G, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(1, ChessBoard.File.G, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(3, ChessBoard.File.F, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[1].setPosition(8, ChessBoard.File.B, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(6, ChessBoard.File.C, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[1].setPosition(5, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[1].setPosition(3, ChessBoard.File.F, ChessBoard.ChessPiece.NO_PIECE);

            
            fillWithSomePieces(_boards[2]);
            _boards[2].setPosition(2, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[2].setPosition(4, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[2].setPosition(7, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[2].setPosition(5, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[2].setPosition(1, ChessBoard.File.G, ChessBoard.ChessPiece.NO_PIECE);
            _boards[2].setPosition(1, ChessBoard.File.G, ChessBoard.ChessPiece.NO_PIECE);
            _boards[2].setPosition(3, ChessBoard.File.F, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[2].setPosition(8, ChessBoard.File.B, ChessBoard.ChessPiece.NO_PIECE);
            _boards[2].setPosition(6, ChessBoard.File.C, ChessBoard.ChessPiece.SOME_PIECE);
            _boards[2].setPosition(5, ChessBoard.File.E, ChessBoard.ChessPiece.NO_PIECE);
            _boards[2].setPosition(3, ChessBoard.File.F, ChessBoard.ChessPiece.NO_PIECE);
            _boards[2].setPosition(5, ChessBoard.File.E, ChessBoard.ChessPiece.SOME_PIECE);

            processBoards();
    }
}


