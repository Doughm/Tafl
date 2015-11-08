using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.IO;

public class Game : MonoBehaviour
{
    //drag and drop objects
    public Transform tile;
    public Material special;
    public Material kingTile;
    public Material black;
    public Material white;
    public Light overlight;
    public Transform blackPiece;
    public Transform whitePiece;
    public Transform kingPiece;
    public Transform moveTile;

    //mouse position
    private Ray ray;
    private Plane playerPlane;
    private Vector3 startPoint = new Vector3();
    private Vector3 mouseWorld = new Vector3();
    private float hitDistance;

    //game values
    private GameObject[,] boardArray;
    private List<GameObject> pieces = new List<GameObject>();
    private GameObject overheadLight;
    private List<GameObject> movementTiles = new List<GameObject>();
    private List<Vector2> winSpaces = new List<Vector2>();
    private string boardPath;
    Dictionary<string, Board> board = new Dictionary<string, Board>();
    private float cameraSpeed = 3;
    private int boardSize = 0;
    public string gameBoard { get; set; }
    public Vector2 selectedTile = new Vector2();
    private Vector2 selectedPiece = new Vector2();
    private Vector3 kingSpace = new Vector3();
    private int blackPieceCount;
    private int whitePiecesCount;
    public bool inMenu { get; set; }
    public bool isBlackTurn { get; set; }
    public bool isGameOver { get; set; }
    private bool isDrawn;
    private bool demoMode = false;

    //network
    private Network network = new Network();
    private bool isHost = false;
    private string[] messageArray;
    public bool isConnecting { get; set; }
    public bool isConnected { get; set; }
    public bool networkTurn { get; set; }
    

	// Use this for initialization
	void Start ()
    {
        //starting values
        isBlackTurn = true;
        boardPath = Application.dataPath + "/Board";
        isGameOver = false;
        isConnected = false;
        isConnecting = false;
        networkTurn = true;
        gameBoard = string.Empty;

        //loads in board files
        try
        {
            DirectoryInfo directory = new DirectoryInfo(boardPath);
            FileInfo[] info = directory.GetFiles("*.xml");

            foreach (FileInfo file in info)
            {
                XMLparse boardXML = new XMLparse(file.FullName);

                string name = "empty";
                string image = "empty";
                int size = -1;
                Vector2 kingsSpot = new Vector2();
                List<Vector2> specialSpaces = new List<Vector2>();
                List<Vector2> blackPieces = new List<Vector2>();
                List<Vector2> whitePieces = new List<Vector2>();

                for (int i = 0; i < boardXML.numberOfElements(); i++)
                {
                    if (boardXML.findType(i) == "Board")
                    {
                        name = boardXML.findValue(i, "name");
                        image = boardXML.findValue(i, "image");
                        size = Convert.ToInt32(boardXML.findValue(i, "size"));
                    }
                    else if (boardXML.findType(i) == "KingSpot")
                    {
                        kingsSpot = new Vector2(Convert.ToSingle(boardXML.findValue(i, "X")),
                                                Convert.ToSingle(boardXML.findValue(i, "Y")));
                    }
                    else if (boardXML.findType(i) == "SpecialSpace")
                    {
                        specialSpaces.Add(new Vector2(Convert.ToSingle(boardXML.findValue(i, "X")),
                                                Convert.ToSingle(boardXML.findValue(i, "Y"))));
                    }
                    else if (boardXML.findType(i) == "BlackPiece")
                    {
                        blackPieces.Add(new Vector2(Convert.ToSingle(boardXML.findValue(i, "X")),
                                                Convert.ToSingle(boardXML.findValue(i, "Y"))));
                    }
                    else if (boardXML.findType(i) == "WhitePiece")
                    {
                        whitePieces.Add(new Vector2(Convert.ToSingle(boardXML.findValue(i, "X")),
                                                Convert.ToSingle(boardXML.findValue(i, "Y"))));
                    }
                }

                board.Add(name, new Board(name, image, size, kingsSpot, specialSpaces, blackPieces, whitePieces));
            }
        }
        catch
        {
            Debug.LogError("Error loading boards");
        }
	}

    //hosts a game
    public void hostGame()
    {
        if (GameObject.Find("InputPort").GetComponent<InputField>().text != string.Empty)
        {
            isConnecting = true;

            isHost = true;

            GameObject.Find("BigText").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("BigText").GetComponent<Text>().text = "Waiting for Player";
            GameObject.Find("PreviewImage").GetComponent<CanvasGroup>().alpha = 0;
            GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().alpha = 0;
            GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().interactable = false;
            GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().blocksRaycasts = false;
            GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().interactable = true;
            GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().blocksRaycasts = true;

            network.host(Convert.ToUInt16(GameObject.Find("InputPort").GetComponent<InputField>().text));
        }
    }

    //joins a game
    public void joinGame()
    {
        if (GameObject.Find("InputPort").GetComponent<InputField>().text != string.Empty &&
            GameObject.Find("InputIP").GetComponent<InputField>().text != string.Empty)
        {
            isConnecting = true;
            isHost = false;

            GameObject.Find("BigText").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("BigText").GetComponent<Text>().text = "Connecting to Host";
            GameObject.Find("PreviewImage").GetComponent<CanvasGroup>().alpha = 0;
            GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().alpha = 0;
            GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().interactable = false;
            GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().blocksRaycasts = false;
            GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().interactable = true;
            GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }
	
    //connects to a network game
    private void connecting()
    {
        //connects to the game
        if (isConnecting == true)
        {
            if (isHost == true)
            {
                if (network.listenForConnections() == true)
                {
                    GameObject.Find("BigText").GetComponent<CanvasGroup>().alpha = 0;

                    isConnecting = false;
                    isConnected = true;

                    if (isBlackTurn == true)
                    {
                        network.sendMessage("2," + gameBoard + ",W");
                        networkTurn = true;
                        GameObject.Find("ScoreBoard").GetComponent<ScoreBoard>().setNetwork(true);
                    }
                    else
                    {
                        network.sendMessage("2," + gameBoard + ",B");
                        networkTurn = false;
                        GameObject.Find("ScoreBoard").GetComponent<ScoreBoard>().setNetwork(false);
                    }

                    createBoard(gameBoard);

                    GameObject.Find("MenuBackground").GetComponent<CanvasGroup>().alpha = 0;
                    GameObject.Find("ScoreBoard").GetComponent<CanvasGroup>().alpha = 1;
                    GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().alpha = 0;
                    GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().interactable = false;
                    GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().blocksRaycasts = false;

                    inMenu = false;
                }
            }
            else
            {
                if (network.connect(GameObject.Find("InputIP").GetComponent<InputField>().text,
                                Convert.ToUInt16(GameObject.Find("InputPort").GetComponent<InputField>().text)) == true)
                {
                    GameObject.Find("BigText").GetComponent<CanvasGroup>().alpha = 0;

                    isConnecting = false;
                    isConnected = true;

                    GameObject.Find("MenuBackground").GetComponent<CanvasGroup>().alpha = 0;
                    GameObject.Find("ScoreBoard").GetComponent<CanvasGroup>().alpha = 1;
                    GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().alpha = 0;
                    GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().interactable = false;
                    GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().blocksRaycasts = false;

                    inMenu = false;
                }
            }
        }
    }

    //stops a network action
    public void cancelNetwork()
    {
        isConnecting = false;
        isConnected = false;
        network.stopHost();

        GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().interactable = true;
        GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().blocksRaycasts = true;
        GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("ButtonCancel").GetComponent<CanvasGroup>().blocksRaycasts = false;
        GameObject.Find("BigText").GetComponent<CanvasGroup>().alpha = 0;
    }

    //ingame network update
    private void networkUpdate()
    {
        //network update
        if (isConnected == true)
        {

            network.receiveMessages();
            //Debug.LogError(network.getNumberOfMessages());
            //1 = game update
            //2 = system update
            //3 = disconnect
            for (int i = 0; i < network.getNumberOfMessages(); i++)
            {
                //Debug.LogError(network.getMessage(i));

                //game update
                //0 = message type
                //1 = x selected piece
                //2 = y selected piece
                //3 = x place to move
                //4 = y place to move
                if (network.getMessage(i)[0] == '1')
                {
                    //debug
                    //Debug.LogError("game update");

                    messageArray = network.getMessage(i).Split(',');

                    movePiece(new Vector2(Convert.ToSingle(messageArray[1]), Convert.ToSingle(messageArray[2])),
                              new Vector2(Convert.ToSingle(messageArray[3]), Convert.ToSingle(messageArray[4])));
                }
                //system update
                //0 = message type
                //1 = board selected
                //2 = starting side
                else if (network.getMessage(i)[0] == '2')
                {
                    //debug
                    //Debug.LogError("system update");

                    messageArray = network.getMessage(i).Split(',');

                    if (messageArray[2] == "B")
                    {
                        networkTurn = true;
                        isBlackTurn = true;
                        GameObject.Find("ScoreBoard").GetComponent<ScoreBoard>().setNetwork(true);
                    }
                    else if (messageArray[2] == "W")
                    {
                        networkTurn = false;
                        isBlackTurn = false;
                        GameObject.Find("ScoreBoard").GetComponent<ScoreBoard>().setNetwork(false);
                    }

                    createBoard(messageArray[1]);
                }
                //disconnect
                //0 = type of message
                else if (network.getMessage(i)[0] == '3')
                {
                    //debug
                    //Debug.LogError("disconnect");

                    GameObject.Find("MenuBackground").GetComponent<CanvasGroup>().alpha = 0.5f;
                    GameObject.Find("BigText").GetComponent<CanvasGroup>().alpha = 1;

                    if (isBlackTurn == true)
                    {
                        GameObject.Find("BigText").GetComponent<Text>().text = "Attackers Win!";
                    }
                    else
                    {
                        GameObject.Find("BigText").GetComponent<Text>().text = "Defenders Win!";
                    }
                    isGameOver = true;
                    isConnected = false;
                }
                network.clearMessages();
            }
        }
    }

    //moves a piece
    private void movePiece(Vector2 sPiece, Vector2 sTile)
    {
        //moves the piece
        pieces[boardArray[(int)sPiece.x, (int)sPiece.y].GetComponent<TileScript>().pieceIndex].transform.position = new Vector3(sTile.x + 0.5f, sTile.y + 0.5f, -0.25f);

        //changes the king spot
        if (boardArray[(int)sPiece.x, (int)sPiece.y].GetComponent<TileScript>().isKing == true)
        {
            kingSpace = new Vector3(sTile.x, sTile.y, 0.25f);
        }

        //move the board tile
        boardArray[(int)sTile.x, (int)sTile.y].GetComponent<TileScript>().setVariables(
                    boardArray[(int)sPiece.x, (int)sPiece.y].GetComponent<TileScript>());

        //clears the new empty space
        boardArray[(int)sPiece.x, (int)sPiece.y].GetComponent<TileScript>().clearTile();

        //finds if there is a piece capture
        checkSandwich(sTile);

        if (isConnected == false)
        {
            //changes the turn
            if (isBlackTurn == true)
            {
                isBlackTurn = false;
            }
            else
            {
                isBlackTurn = true;
            }

            GameObject.Find("ScoreBoard").GetComponentInChildren<ScoreBoard>().switchTurn();
        }
        else
        {
            //changes the turn
            if (networkTurn == true)
            {
                networkTurn = false;
            }
            else
            {
                networkTurn = true;
            }

            GameObject.Find("ScoreBoard").GetComponentInChildren<ScoreBoard>().switchTurn();
        }
    }

    //rotates the camera with the mouse
    private void rotateCamera()
    {
        //right click
        if (Input.GetMouseButton(1))
        {
            if (Input.GetAxis("Mouse X") > 0)
            {
                Camera.main.transform.RotateAround(boardArray[boardSize / 2, boardSize / 2].transform.position, Vector3.back, cameraSpeed * Input.GetAxis("Mouse X"));
            }
            else if (Input.GetAxis("Mouse X") < 0)
            {
                Camera.main.transform.RotateAround(boardArray[boardSize / 2, boardSize / 2].transform.position, Vector3.back, cameraSpeed * Input.GetAxis("Mouse X"));
            }
        }
    }

    //rotates the camera in demo mode
    private void demoModeEnable()
    {
        //turns demo mode on
        if (Input.GetButtonDown("Demo") == true)
        {
            demoMode = true;
        }
        //moves the camera if demo mode is turned on
        if (demoMode == true)
        {
            Camera.main.transform.RotateAround(boardArray[boardSize / 2, boardSize / 2].transform.position, Vector3.back, cameraSpeed * 0.1f);
        }
    }

    //selects a piece
    private void selectPiece()
    { 
        //left click
        if (Input.GetMouseButtonDown(0))
        {
            selectedTile = findMousePositionBoard();
            if (selectedTile.x != -1)
            {
                //selects a piece
                if (boardArray[(int)selectedTile.x, (int)selectedTile.y].GetComponent<TileScript>().isOccupied == true &&
                    boardArray[(int)selectedTile.x, (int)selectedTile.y].GetComponent<TileScript>().isPieceBlack == isBlackTurn &&
                    networkTurn == true)
                {
                    findMovement(selectedTile);
                    selectedPiece = selectedTile;
                }

                //selects a place to move
                else if (boardArray[(int)findMousePositionBoard().x, (int)findMousePositionBoard().y].GetComponent<TileScript>().isOccupied == false)
                {
                    for (int i = 0; i < movementTiles.Count; i++)
                    {
                        if (movementTiles[i].transform.position.x - 0.5f == selectedTile.x &&
                            movementTiles[i].transform.position.y - 0.5f == selectedTile.y)
                        {
                            movePiece(selectedPiece, selectedTile);

                            if (isConnected == true)
                            {
                                network.sendMessage("1," + selectedPiece.x.ToString() + "," + selectedPiece.y.ToString() + "," +
                                                    selectedTile.x.ToString() + "," + selectedTile.y.ToString());
                            }

                            checkWin();
                            checkDraw();

                            break;
                        }
                    }
                    for (int i = 0; i < movementTiles.Count; i++)
                    {
                        Destroy(movementTiles[i]);
                    }
                    movementTiles.Clear();
                }
            }
        }
    }

	// Update is called once per frame
	void Update()
    {
        //network
        connecting();
        networkUpdate();

        //normal gameplay
        if (inMenu == false && isGameOver == false)
        {
            selectPiece();
            rotateCamera();
            demoModeEnable();
        }
	}

    //creates the board and pieces
    public void createBoard(string gameType)
    {
        //sets that the game is not over
        isGameOver = false;
        isDrawn = false;

        //creates the overhead light
        overheadLight = Instantiate(overlight.gameObject, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0)) as GameObject;

        //resets the camera angle to default
        Camera.main.transform.rotation = Quaternion.Euler(315,0,0);

        boardArray = new GameObject[board[gameType].getBoardSize(), board[gameType].getBoardSize()];

        boardSize = board[gameType].getBoardSize();
        blackPieceCount = board[gameType].getNumberOfBlackPieces();

        //sets up the tile objects
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                boardArray[x, y] = Instantiate(tile.gameObject, new Vector2(x + 0.5f, y + 0.5f), Quaternion.Euler(270, 0, 0)) as GameObject;
                boardArray[x, y].GetComponent<TileScript>().isSpecialTile = false;
                boardArray[x, y].GetComponent<TileScript>().isKing = false;
                boardArray[x, y].GetComponent<TileScript>().isOccupied = false;
                boardArray[x, y].GetComponent<TileScript>().isPieceBlack = false;
                boardArray[x, y].GetComponent<TileScript>().isThrone = false;
                boardArray[x, y].name = "Tile:" + x.ToString() + "," + y.ToString();
            }
        }

        //special tiles
        for (int i = 0; i < board[gameType].getNumberOfSpecialSpaces(); i++)
        {
            boardArray[(int)board[gameType].getSpecialSpace(i).x, (int)board[gameType].getSpecialSpace(i).y].GetComponent<Renderer>().material = special;
            boardArray[(int)board[gameType].getSpecialSpace(i).x, (int)board[gameType].getSpecialSpace(i).y].GetComponent<TileScript>().isSpecialTile = true;
        }
        //black pieces
        for (int i = 0; i < board[gameType].getNumberOfBlackPieces(); i++)
        {

            boardArray[(int)board[gameType].getBlackPiece(i).x, (int)board[gameType].getBlackPiece(i).y].GetComponent<Renderer>().material = black;
            pieces.Add(Instantiate(blackPiece.gameObject, new Vector3(board[gameType].getBlackPiece(i).x + 0.5f, board[gameType].getBlackPiece(i).y + 0.5f, -0.25f), Quaternion.Euler(270, 0, 0)) as GameObject);
            boardArray[(int)board[gameType].getBlackPiece(i).x, (int)board[gameType].getBlackPiece(i).y].GetComponent<TileScript>().isPieceBlack = true;
            boardArray[(int)board[gameType].getBlackPiece(i).x, (int)board[gameType].getBlackPiece(i).y].GetComponent<TileScript>().isOccupied = true;
        }
        //white pieces
        for (int i = 0; i < board[gameType].getNumberOfWhitePieces(); i++)
        {
            boardArray[(int)board[gameType].getWhitePiece(i).x, (int)board[gameType].getWhitePiece(i).y].GetComponent<Renderer>().material = white;
            pieces.Add(Instantiate(whitePiece.gameObject, new Vector3(board[gameType].getWhitePiece(i).x + 0.5f, board[gameType].getWhitePiece(i).y + 0.5f, -0.25f), Quaternion.Euler(270, 0, 0)) as GameObject);
            boardArray[(int)board[gameType].getWhitePiece(i).x, (int)board[gameType].getWhitePiece(i).y].GetComponent<TileScript>().isPieceBlack = false;
            boardArray[(int)board[gameType].getWhitePiece(i).x, (int)board[gameType].getWhitePiece(i).y].GetComponent<TileScript>().isOccupied = true;
        }

        //king
        boardArray[(int)board[gameType].getKingSpot().x, (int)board[gameType].getKingSpot().y].GetComponent<Renderer>().material = kingTile;
        pieces.Add(Instantiate(kingPiece.gameObject, new Vector3(board[gameType].getKingSpot().x + 0.5f, board[gameType].getKingSpot().y + 0.5f, -0.25f), Quaternion.Euler(270, 0, 0)) as GameObject);
        boardArray[(int)board[gameType].getKingSpot().x, (int)board[gameType].getKingSpot().y].GetComponent<TileScript>().isOccupied = true;
        boardArray[(int)board[gameType].getKingSpot().x, (int)board[gameType].getKingSpot().y].GetComponent<TileScript>().isKing = true;
        boardArray[(int)board[gameType].getKingSpot().x, (int)board[gameType].getKingSpot().y].GetComponent<TileScript>().isThrone = true;
        kingSpace = board[gameType].getKingSpot();

        for (int i = 0; i < pieces.Count; i++)
        {
            boardArray[(int)pieces[i].transform.position.x, (int)pieces[i].transform.position.y].GetComponent<TileScript>().pieceIndex = i;
        }

        for (int i = 0; i < board[gameType].getNumberOfSpecialSpaces(); i++)
        {
            winSpaces.Add(board[gameType].getSpecialSpace(i));
        }

        overheadLight.transform.position = boardArray[boardSize / 2, boardSize / 2].transform.position;
        overheadLight.transform.position = new Vector3(overheadLight.transform.position.x, overheadLight.transform.position.y, -5);
        
        Camera.main.transform.position = new Vector3(boardArray[boardSize / 2, boardSize / 2].transform.position.x,
                                                         boardArray[boardSize / 2, boardSize / 2].transform.position.y - 14 + (0.6f * (7 - boardSize)),
                                                         -14 + (0.5f * (7 - boardSize)));
    }

    //finds where the mouse is on the board
    private Vector2 findMousePositionBoard()
    {
        //sets playerPlane so that it intersects startpoint and slopes upward
        playerPlane = new Plane(Vector3.forward, startPoint);


        //sets ray to come from the mouse position
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // if ray is parallel to the plane, Raycast will return false.
        if (playerPlane.Raycast(ray, out hitDistance))
        {
            //mouseWorldPositionXYZ to a point along a ray that intersects with distance from plane
            mouseWorld = ray.GetPoint(hitDistance);
        }
        //finds if the mouse is outside the board
        if (mouseWorld.x < 0 || mouseWorld.y < 0 ||
            mouseWorld.x >= boardSize || mouseWorld.y >= boardSize)
        {
            mouseWorld.x = -1;
            mouseWorld.y = -1;
        }
        //turns the floats into ints
        mouseWorld.x = (int)mouseWorld.x;
        mouseWorld.y = (int)mouseWorld.y;

        return mouseWorld;
    }

    //unloads the board
    public void unload()
    {
        if (boardSize > 0)
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int ii = 0; ii < boardSize; ii++)
                {
                    Destroy(boardArray[i, ii]);
                }
            }
            boardArray = new GameObject[0, 0];
            for (int i = 0; i < pieces.Count; i++)
            {
                Destroy(pieces[i]);
            }
            pieces.Clear();
            for (int i = 0; i < movementTiles.Count; i++)
            {
                Destroy(movementTiles[i]);
            }
            movementTiles.Clear();
            Camera.main.transform.position = new Vector3(0, 0, 0);
            Destroy(overheadLight);
            isBlackTurn = true;
            boardSize = 0;
            winSpaces.Clear();
            GameObject.Find("ScoreBoard").GetComponentInChildren<ScoreBoard>().reset();

            if (isConnected == true)
            {
                network.sendMessage("3");
                network.disconnect();
                isConnected = false;
            }
        }
    }

    //finds the movement of a piece and places movement indicators on the board
    private void findMovement(Vector2 position)
    {
        for (int i = 0; i < movementTiles.Count; i++)
        {
            Destroy(movementTiles[i]);
        }
        movementTiles.Clear();

        //not king
        if (boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isKing == false)
        {
            //X+
            for (int i = 1; i < boardSize - position.x; i++)
            {
                if (boardArray[(int)position.x + i, (int)position.y].GetComponent<TileScript>().isOccupied == true)
                {
                    break;
                }
                if(boardArray[(int)position.x + i, (int)position.y].GetComponent<TileScript>().isSpecialTile == false &&
                    boardArray[(int)position.x + i, (int)position.y].GetComponent<TileScript>().isThrone == false)
                {
                    movementTiles.Add(Instantiate(moveTile.gameObject,
                                      new Vector3(boardArray[(int)position.x, (int)position.y].transform.position.x + i, boardArray[(int)position.x, (int)position.y].transform.position.y, -0.01f),
                                      Quaternion.Euler(270, 0, 0)) as GameObject);
                }
            }
            //X-
            for (int i = 1; i < position.x + 1; i++)
            {
                if (boardArray[(int)position.x - i, (int)position.y].GetComponent<TileScript>().isOccupied == true)
                {
                    break;
                }
                if (boardArray[(int)position.x - i, (int)position.y].GetComponent<TileScript>().isSpecialTile == false &&
                    boardArray[(int)position.x - i, (int)position.y].GetComponent<TileScript>().isThrone == false)
                {
                    movementTiles.Add(Instantiate(moveTile.gameObject,
                                      new Vector3(boardArray[(int)position.x, (int)position.y].transform.position.x - i, boardArray[(int)position.x, (int)position.y].transform.position.y, -0.01f),
                                      Quaternion.Euler(270, 0, 0)) as GameObject);
                }
            }
            //Y+
            for (int i = 1; i < boardSize - position.y; i++)
            {
                if (boardArray[(int)position.x, (int)position.y + i].GetComponent<TileScript>().isOccupied == true)
                {
                    break;
                }
                if (boardArray[(int)position.x, (int)position.y + i].GetComponent<TileScript>().isSpecialTile == false &&
                    boardArray[(int)position.x, (int)position.y + i].GetComponent<TileScript>().isThrone == false)
                {
                    movementTiles.Add(Instantiate(moveTile.gameObject,
                                      new Vector3(boardArray[(int)position.x, (int)position.y].transform.position.x, boardArray[(int)position.x, (int)position.y].transform.position.y + i, -0.01f),
                                      Quaternion.Euler(270, 0, 0)) as GameObject);
                }
            }
            //Y-
            for (int i = 1; i < position.y + 1; i++)
            {
                if (boardArray[(int)position.x, (int)position.y - i].GetComponent<TileScript>().isOccupied == true)
                {
                    break;
                }
                if (boardArray[(int)position.x, (int)position.y - i].GetComponent<TileScript>().isSpecialTile == false &&
                    boardArray[(int)position.x, (int)position.y - i].GetComponent<TileScript>().isThrone == false)
                {
                    movementTiles.Add(Instantiate(moveTile.gameObject,
                                      new Vector3(boardArray[(int)position.x, (int)position.y].transform.position.x, boardArray[(int)position.x, (int)position.y].transform.position.y - i, -0.01f),
                                      Quaternion.Euler(270, 0, 0)) as GameObject);
                }
            }
        }
        else
        {
            //king
            //X+
            for (int i = 1; i < boardSize - position.x; i++)
            {
                if (boardArray[(int)position.x + i, (int)position.y].GetComponent<TileScript>().isOccupied == true)
                {
                    break;
                }
                movementTiles.Add(Instantiate(moveTile.gameObject,
                                  new Vector3(boardArray[(int)position.x, (int)position.y].transform.position.x + i, boardArray[(int)position.x, (int)position.y].transform.position.y, -0.01f),
                                  Quaternion.Euler(270, 0, 0)) as GameObject);
            }
            //X-
            for (int i = 1; i < position.x + 1; i++)
            {
                if (boardArray[(int)position.x - i, (int)position.y].GetComponent<TileScript>().isOccupied == true)
                {
                    break;
                }
                movementTiles.Add(Instantiate(moveTile.gameObject,
                                  new Vector3(boardArray[(int)position.x, (int)position.y].transform.position.x - i, boardArray[(int)position.x, (int)position.y].transform.position.y, -0.01f),
                                  Quaternion.Euler(270, 0, 0)) as GameObject);
            }
            //Y+
            for (int i = 1; i < boardSize - position.y; i++)
            {
                if (boardArray[(int)position.x, (int)position.y + i].GetComponent<TileScript>().isOccupied == true)
                {
                    break;
                }
                movementTiles.Add(Instantiate(moveTile.gameObject,
                                  new Vector3(boardArray[(int)position.x, (int)position.y].transform.position.x, boardArray[(int)position.x, (int)position.y].transform.position.y + i, -0.01f),
                                  Quaternion.Euler(270, 0, 0)) as GameObject);
            }
            //Y-
            for (int i = 1; i < position.y + 1; i++)
            {
                if (boardArray[(int)position.x, (int)position.y - i].GetComponent<TileScript>().isOccupied == true)
                {
                    break;
                }
                movementTiles.Add(Instantiate(moveTile.gameObject,
                                  new Vector3(boardArray[(int)position.x, (int)position.y].transform.position.x, boardArray[(int)position.x, (int)position.y].transform.position.y - i, -0.01f),
                                  Quaternion.Euler(270, 0, 0)) as GameObject);
            }
        }
    }

    //checks for a sandwich
    private void checkSandwich(Vector2 position)
    {
        //basic capture
        //X+
        if (position.x < boardSize - 2)
        {
            if (boardArray[(int)position.x + 1, (int)position.y].GetComponent<TileScript>().isOccupied &&
                boardArray[(int)position.x + 1, (int)position.y].GetComponent<TileScript>().isPieceBlack !=
                boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isPieceBlack &&
                boardArray[(int)position.x + 1, (int)position.y].GetComponent<TileScript>().isKing == false)
            {
                if (boardArray[(int)position.x + 2, (int)position.y].GetComponent<TileScript>().isOccupied &&
                boardArray[(int)position.x + 2, (int)position.y].GetComponent<TileScript>().isPieceBlack ==
                boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isPieceBlack)
                {
                    if (boardArray[(int)position.x + 1, (int)position.y].GetComponent<TileScript>().isPieceBlack)
                    {
                        blackPieceCount--;
                    }
                    else
                    {
                        whitePiecesCount--;
                    }
                    pieces[boardArray[(int)position.x + 1, (int)position.y].GetComponent<TileScript>().pieceIndex].gameObject.SetActive(false);
                    boardArray[(int)position.x + 1, (int)position.y].GetComponent<TileScript>().clearTile();
                }
            }
        }
        //X-
        if (position.x > 1)
        {
            if (boardArray[(int)position.x - 1, (int)position.y].GetComponent<TileScript>().isOccupied &&
                boardArray[(int)position.x - 1, (int)position.y].GetComponent<TileScript>().isPieceBlack !=
                boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isPieceBlack &&
                boardArray[(int)position.x - 1, (int)position.y].GetComponent<TileScript>().isKing == false)
            {
                if (boardArray[(int)position.x - 2, (int)position.y].GetComponent<TileScript>().isOccupied &&
                boardArray[(int)position.x - 2, (int)position.y].GetComponent<TileScript>().isPieceBlack ==
                boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isPieceBlack)
                {
                    if (boardArray[(int)position.x - 1, (int)position.y].GetComponent<TileScript>().isPieceBlack)
                    {
                        blackPieceCount--;
                    }
                    else
                    {
                        whitePiecesCount--;
                    }
                    pieces[boardArray[(int)position.x - 1, (int)position.y].GetComponent<TileScript>().pieceIndex].gameObject.SetActive(false);
                    boardArray[(int)position.x - 1, (int)position.y].GetComponent<TileScript>().clearTile();
                }
            }
        }
        //Y+
        if (position.y < boardSize - 2)
        {
            if (boardArray[(int)position.x, (int)position.y + 1].GetComponent<TileScript>().isOccupied &&
                boardArray[(int)position.x, (int)position.y + 1].GetComponent<TileScript>().isPieceBlack !=
                boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isPieceBlack &&
                boardArray[(int)position.x, (int)position.y + 1].GetComponent<TileScript>().isKing == false)
            {
                if (boardArray[(int)position.x, (int)position.y + 2].GetComponent<TileScript>().isOccupied &&
                boardArray[(int)position.x, (int)position.y + 2].GetComponent<TileScript>().isPieceBlack ==
                boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isPieceBlack)
                {
                    if (boardArray[(int)position.x, (int)position.y + 1].GetComponent<TileScript>().isPieceBlack)
                    {
                        blackPieceCount--;
                    }
                    else
                    {
                        whitePiecesCount--;
                    }
                    pieces[boardArray[(int)position.x, (int)position.y + 1].GetComponent<TileScript>().pieceIndex].gameObject.SetActive(false);
                    boardArray[(int)position.x, (int)position.y + 1].GetComponent<TileScript>().clearTile();
                }
            }
        }
        //Y-
        if (position.y > 1)
        {
            if (boardArray[(int)position.x, (int)position.y - 1].GetComponent<TileScript>().isOccupied &&
                boardArray[(int)position.x, (int)position.y - 1].GetComponent<TileScript>().isPieceBlack !=
                boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isPieceBlack &&
                boardArray[(int)position.x, (int)position.y - 1].GetComponent<TileScript>().isKing == false)
            {
                if (boardArray[(int)position.x, (int)position.y - 2].GetComponent<TileScript>().isOccupied &&
                boardArray[(int)position.x, (int)position.y - 2].GetComponent<TileScript>().isPieceBlack ==
                boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isPieceBlack)
                {
                    if (boardArray[(int)position.x, (int)position.y - 1].GetComponent<TileScript>().isPieceBlack)
                    {
                        blackPieceCount--;
                    }
                    else
                    {
                        whitePiecesCount--;
                    }
                    pieces[boardArray[(int)position.x, (int)position.y - 1].GetComponent<TileScript>().pieceIndex].gameObject.SetActive(false);
                    boardArray[(int)position.x, (int)position.y - 1].GetComponent<TileScript>().clearTile();
                }
            }
        }
        
        //advanced capture
        //piece capture against special space
        //X+
        if (position.x < boardSize - 2)
        {
            if (boardArray[(int)position.x + 1, (int)position.y].GetComponent<TileScript>().isOccupied &&
                boardArray[(int)position.x + 1, (int)position.y].GetComponent<TileScript>().isPieceBlack !=
                boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isPieceBlack &&
                boardArray[(int)position.x + 1, (int)position.y].GetComponent<TileScript>().isKing == false)
            {
                if (boardArray[(int)position.x + 2, (int)position.y].GetComponent<TileScript>().isSpecialTile &&
                    boardArray[(int)position.x + 2, (int)position.y].GetComponent<TileScript>().isKing == false)
                {
                    pieces[boardArray[(int)position.x + 1, (int)position.y].GetComponent<TileScript>().pieceIndex].gameObject.SetActive(false);
                    boardArray[(int)position.x + 1, (int)position.y].GetComponent<TileScript>().clearTile();
                }
            }
        }
        //X-
        if (position.x > 1)
        {
            if (boardArray[(int)position.x - 1, (int)position.y].GetComponent<TileScript>().isOccupied &&
                boardArray[(int)position.x - 1, (int)position.y].GetComponent<TileScript>().isPieceBlack !=
                boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isPieceBlack &&
                boardArray[(int)position.x - 1, (int)position.y].GetComponent<TileScript>().isKing == false)
            {
                if (boardArray[(int)position.x - 2, (int)position.y].GetComponent<TileScript>().isSpecialTile &&
                    boardArray[(int)position.x - 2, (int)position.y].GetComponent<TileScript>().isKing == false)
                {
                    pieces[boardArray[(int)position.x - 1, (int)position.y].GetComponent<TileScript>().pieceIndex].gameObject.SetActive(false);
                    boardArray[(int)position.x - 1, (int)position.y].GetComponent<TileScript>().clearTile();
                }
            }
        }
        //Y+
        if (position.y < boardSize - 2)
        {
            if (boardArray[(int)position.x, (int)position.y + 1].GetComponent<TileScript>().isOccupied &&
                boardArray[(int)position.x, (int)position.y + 1].GetComponent<TileScript>().isPieceBlack !=
                boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isPieceBlack &&
                boardArray[(int)position.x, (int)position.y + 1].GetComponent<TileScript>().isKing == false)
            {
                if (boardArray[(int)position.x, (int)position.y + 2].GetComponent<TileScript>().isSpecialTile &&
                    boardArray[(int)position.x, (int)position.y + 2].GetComponent<TileScript>().isKing == false)
                {
                    pieces[boardArray[(int)position.x, (int)position.y + 1].GetComponent<TileScript>().pieceIndex].gameObject.SetActive(false);
                    boardArray[(int)position.x, (int)position.y + 1].GetComponent<TileScript>().clearTile();
                }
            }
        }
        //Y-
        if (position.y > 1)
        {
            if (boardArray[(int)position.x, (int)position.y - 1].GetComponent<TileScript>().isOccupied &&
                boardArray[(int)position.x, (int)position.y - 1].GetComponent<TileScript>().isPieceBlack !=
                boardArray[(int)position.x, (int)position.y].GetComponent<TileScript>().isPieceBlack &&
                boardArray[(int)position.x, (int)position.y - 1].GetComponent<TileScript>().isKing == false)
            {
                if (boardArray[(int)position.x, (int)position.y - 2].GetComponent<TileScript>().isSpecialTile &&
                    boardArray[(int)position.x, (int)position.y - 2].GetComponent<TileScript>().isKing == false)
                {
                    pieces[boardArray[(int)position.x, (int)position.y - 1].GetComponent<TileScript>().pieceIndex].gameObject.SetActive(false);
                    boardArray[(int)position.x, (int)position.y - 1].GetComponent<TileScript>().clearTile();
                }
            }
        }
    }

    //checks to see if the game has been won
    private void checkWin()
    {
        //white win
        //king makes it to safe space
        for (int i = 0; i < winSpaces.Count; i++)
        {
            if (kingSpace.x == winSpaces[i].x && kingSpace.y == winSpaces[i].y)
            {
                GameObject.Find("MenuBackground").GetComponent<CanvasGroup>().alpha = 0.5f;
                GameObject.Find("BigText").GetComponent<CanvasGroup>().alpha = 1;
                GameObject.Find("BigText").GetComponent<Text>().text = "Defenders Win!";
                isGameOver = true;
                demoMode = false;
                gameMenuButtonDisappear();
            }
        }

        //black win
        //king capture
        if (kingSpace.x != 0 && kingSpace.y != 0 &&
            kingSpace.x != boardSize - 1 && kingSpace.y != boardSize - 1)
        {
            if (boardArray[(int)kingSpace.x + 1, (int)kingSpace.y].GetComponent<TileScript>().isPieceBlack &&
                boardArray[(int)kingSpace.x - 1, (int)kingSpace.y].GetComponent<TileScript>().isPieceBlack &&
                boardArray[(int)kingSpace.x, (int)kingSpace.y + 1].GetComponent<TileScript>().isPieceBlack &&
                boardArray[(int)kingSpace.x, (int)kingSpace.y - 1].GetComponent<TileScript>().isPieceBlack)
            {
                GameObject.Find("MenuBackground").GetComponent<CanvasGroup>().alpha = 0.5f;
                GameObject.Find("BigText").GetComponent<CanvasGroup>().alpha = 1;
                GameObject.Find("BigText").GetComponent<Text>().text = "Attakers Win!";
                isGameOver = true;
                demoMode = false;
                gameMenuButtonDisappear();
            }
        }
    }

    //checks for a draw
    private void checkDraw()
    {
        isDrawn = true;
        
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].gameObject.activeSelf == true)
            {
                if (boardArray[(int)pieces[i].transform.position.x,
                    (int)pieces[i].transform.position.y].GetComponent<TileScript>().isPieceBlack == true)
                {
                    //X+
                    if (pieces[i].transform.position.x - 0.5f < boardSize - 1)
                    {
                        if (boardArray[(int)pieces[i].transform.position.x + 1,
                            (int)pieces[i].transform.position.y].GetComponent<TileScript>().isOccupied == false)
                        {
                            isDrawn = false;
                            break;
                        }
                    }
                    //X-
                    if (pieces[i].transform.position.x - 0.5f > 0)
                    {
                        if (boardArray[(int)pieces[i].transform.position.x - 1,
                            (int)pieces[i].transform.position.y].GetComponent<TileScript>().isOccupied == false)
                        {
                            isDrawn = false;
                            break;
                        }
                    }
                    //Y+
                    if (pieces[i].transform.position.y - 0.5f < boardSize - 1)
                    {
                        if (boardArray[(int)pieces[i].transform.position.x,
                            (int)pieces[i].transform.position.y + 1].GetComponent<TileScript>().isOccupied == false)
                        {
                            isDrawn = false;
                            break;
                        }
                    }
                    //Y-
                    if (pieces[i].transform.position.y - 0.5f > 0)
                    {
                        if (boardArray[(int)pieces[i].transform.position.x,
                            (int)pieces[i].transform.position.y - 1].GetComponent<TileScript>().isOccupied == false)
                        {
                            isDrawn = false;
                            break;
                        }
                    }
                }
            }
        }
        
        if (isDrawn == true)
        {
            GameObject.Find("MenuBackground").GetComponent<CanvasGroup>().alpha = 0.5f;
            GameObject.Find("BigText").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("BigText").GetComponent<Text>().text = "The Game is a Draw";
            isGameOver = true;
        }
        else
        {
            isDrawn = true;
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].gameObject.activeSelf == true)
            {
                if (boardArray[(int)pieces[i].transform.position.x,
                    (int)pieces[i].transform.position.y].GetComponent<TileScript>().isPieceBlack == false)
                {
                    //X+
                    if (pieces[i].transform.position.x - 0.5f < boardSize - 1)
                    {
                        if (boardArray[(int)pieces[i].transform.position.x + 1,
                            (int)pieces[i].transform.position.y].GetComponent<TileScript>().isOccupied == false)
                        {
                            isDrawn = false;
                            break;
                        }
                    }
                    //X-
                    if (pieces[i].transform.position.x - 0.5f > 0)
                    {
                        if (boardArray[(int)pieces[i].transform.position.x - 1,
                            (int)pieces[i].transform.position.y].GetComponent<TileScript>().isOccupied == false)
                        {
                            isDrawn = false;
                            break;
                        }
                    }
                    //Y+
                    if (pieces[i].transform.position.y - 0.5f < boardSize - 1)
                    {
                        if (boardArray[(int)pieces[i].transform.position.x,
                            (int)pieces[i].transform.position.y + 1].GetComponent<TileScript>().isOccupied == false)
                        {
                            isDrawn = false;
                            break;
                        }
                    }
                    //Y-
                    if (pieces[i].transform.position.y - 0.5f > 0)
                    {
                        if (boardArray[(int)pieces[i].transform.position.x,
                            (int)pieces[i].transform.position.y - 1].GetComponent<TileScript>().isOccupied == false)
                        {
                            isDrawn = false;
                            break;
                        }
                    }
                }
            }
        }

        if (isDrawn == true)
        {
            GameObject.Find("MenuBackground").GetComponent<CanvasGroup>().alpha = 0.5f;
            GameObject.Find("BigText").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("BigText").GetComponent<Text>().text = "The Game is a Draw";
            isGameOver = true;
            demoMode = false;
            gameMenuButtonDisappear();
        }
    }

    //on program close
    void OnApplicationQuit()
    {
        if (isConnected == true)
        {
            network.sendMessage("3");
            network.disconnect();
        }
    }

    //makes the game menu button disappear
    private void gameMenuButtonDisappear()
    {
        GameObject.Find("ButtonGameMenu").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("ButtonGameMenu").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("ButtonGameMenu").GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
}

