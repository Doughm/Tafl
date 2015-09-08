using UnityEngine;
using UnityEngine.UI;
using System;

public class Menu : MonoBehaviour
{
    //cached scripts
    private Game gameScript;

    // Use this for initialization
    void Start()
    {
        gameScript = GameObject.Find("Program").GetComponent<Game>();
        gameScript.inMenu = true;
    }
    
    // Update is called once per frame
    void Update()
    {
        //what happens when you hit the ESC key
        if (Input.GetButtonDown("Menu") == true)
        {
            if (gameScript.inMenu == true &&
                GameObject.Find("MenuBackground").GetComponent<CanvasGroup>().alpha == 0)
            {
                backToGame();
            }
            else if (gameScript.inMenu == false &&
                GameObject.Find("MenuBackground").GetComponent<CanvasGroup>().alpha == 0)
            {
                gameMenu();
            }
            else if (gameScript.inMenu == true &&
                GameObject.Find("MenuBackground").GetComponent<CanvasGroup>().alpha == 0.5f)
            {
                backToMainMenu();
            }
        }

        
        //if the game is over
        if (gameScript.isGameOver == true &&
            (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Menu") == true))
        {
            backToMainMenu();
        }
    }

    //starts the game with ether hotseat or network
    public void startGame(bool isHotseat)
    {
        GameObject.Find("MainMenu").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("MainMenu").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("MainMenu").GetComponent<CanvasGroup>().blocksRaycasts = false;

        if (isHotseat == true)
        {
            GameObject.Find("ScoreBoard").GetComponent<ScoreBoard>().setHotSeat();
            selectBoard(true);
        }
        else
        {
            GameObject.Find("BoardSelectNetwork").GetComponent<CanvasGroup>().alpha = 0;
            GameObject.Find("BoardSelectNetwork").GetComponent<CanvasGroup>().interactable = false;
            GameObject.Find("BoardSelectNetwork").GetComponent<CanvasGroup>().blocksRaycasts = false;
            GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().interactable = true;
            GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().blocksRaycasts = true;

            if (gameScript.gameBoard == string.Empty)
            {
                GameObject.Find("ButtonHost").GetComponent<Button>().interactable = false;
            }
            else
            {
                GameObject.Find("ButtonHost").GetComponent<Button>().interactable = true;
            }
        }
    }

    //selects the board
    public void selectBoard(bool isHotseat)
    {
        if (isHotseat == true)
        {
            GameObject.Find("BoardSelectHotseat").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("BoardSelectHotseat").GetComponent<CanvasGroup>().interactable = true;
            GameObject.Find("BoardSelectHotseat").GetComponent<CanvasGroup>().blocksRaycasts = true;
            GameObject.Find("ButtonPlayGame").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("BoardSelectNetwork").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("BoardSelectNetwork").GetComponent<CanvasGroup>().interactable = true;
            GameObject.Find("BoardSelectNetwork").GetComponent<CanvasGroup>().blocksRaycasts = true;
            GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().alpha = 0;
            GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().interactable = false;
            GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().blocksRaycasts = false;
            GameObject.Find("ButtonSelect").GetComponent<Button>().interactable = false;
        }
    }

    //starts the tutorial on how to play
    public void instructions()
    {
        GameObject.Find("MainMenu").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("MainMenu").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("MainMenu").GetComponent<CanvasGroup>().blocksRaycasts = false;
        GameObject.Find("Instructions").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("Instructions").GetComponent<CanvasGroup>().interactable = true;
        GameObject.Find("Instructions").GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    //exits the game
    public void quitGame()
    {
        Application.Quit();
    }

    //sets the game type
    public void setGameType(string gameType)
    {
        gameScript.gameBoard = gameType;
        GameObject.Find("PreviewImage").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("PreviewImage").GetComponent<BoardPreview>().setSprite(gameType);

        if (GameObject.Find("BoardSelectHotseat").GetComponent<CanvasGroup>().alpha == 1)
        {
            GameObject.Find("ButtonPlayGame").GetComponent<Button>().interactable = true;
        }
        else if (GameObject.Find("BoardSelectHotseat").GetComponent<CanvasGroup>().alpha == 0)
        {
            GameObject.Find("ButtonSelect").GetComponent<Button>().interactable = true;
        }
    }

    //load game board
    public void loadBoard()
    {
        gameScript.inMenu = false;

        GameObject.Find("BoardSelectHotseat").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("BoardSelectHotseat").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("BoardSelectHotseat").GetComponent<CanvasGroup>().blocksRaycasts = false;
        GameObject.Find("MenuBackground").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("ScoreBoard").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("ButtonGameMenu").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("ButtonGameMenu").GetComponent<CanvasGroup>().interactable = true;
        GameObject.Find("ButtonGameMenu").GetComponent<CanvasGroup>().blocksRaycasts = true;
        GameObject.Find("PreviewImage").GetComponent<CanvasGroup>().alpha = 0;

        gameScript.createBoard(gameScript.gameBoard);
    }

    //brings up the game menu
    public void gameMenu()
    {
        gameScript.inMenu = true;

        GameObject.Find("ButtonGameMenu").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("ButtonGameMenu").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("ButtonGameMenu").GetComponent<CanvasGroup>().blocksRaycasts = false;
        GameObject.Find("InGameMenu").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("InGameMenu").GetComponent<CanvasGroup>().interactable = true;
        GameObject.Find("InGameMenu").GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    //goes back to game
    public void backToGame()
    {
        gameScript.inMenu = false;

        GameObject.Find("ButtonGameMenu").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("ButtonGameMenu").GetComponent<CanvasGroup>().interactable = true;
        GameObject.Find("ButtonGameMenu").GetComponent<CanvasGroup>().blocksRaycasts = true;
        GameObject.Find("InGameMenu").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("InGameMenu").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("InGameMenu").GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    //goes back to the main menu
    public void backToMainMenu()
    {
        gameScript.unload();
        gameScript.gameBoard = string.Empty;
        gameScript.inMenu = true;
        gameScript.isConnecting = false;
        gameScript.isConnected = false;
        gameScript.networkTurn = true;
        gameScript.isGameOver = false;

        GameObject.Find("InGameMenu").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("InGameMenu").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("InGameMenu").GetComponent<CanvasGroup>().blocksRaycasts = false;
        GameObject.Find("MainMenu").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("MainMenu").GetComponent<CanvasGroup>().interactable = true;
        GameObject.Find("MainMenu").GetComponent<CanvasGroup>().blocksRaycasts = true;
        GameObject.Find("BoardSelectHotseat").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("BoardSelectHotseat").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("BoardSelectHotseat").GetComponent<CanvasGroup>().blocksRaycasts = false;
        GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("NetworkMenu").GetComponent<CanvasGroup>().blocksRaycasts = false;
        GameObject.Find("Instructions").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("Instructions").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("Instructions").GetComponent<CanvasGroup>().blocksRaycasts = false;
        GameObject.Find("MenuBackground").GetComponent<CanvasGroup>().alpha = 0.5f;
        GameObject.Find("ScoreBoard").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("BigText").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("PreviewImage").GetComponent<CanvasGroup>().alpha = 0;
    }

    //sets if the host is the attacker or defender
    public void setSide(bool side)
    {
        gameScript.isBlackTurn = side;
    }
}
