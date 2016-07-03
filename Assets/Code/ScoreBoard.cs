using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    private bool turn = true;
    private bool isHotSeat = true;
    private bool isBlackPlayer = true;

    //switches the turn
    public void switchTurn()
    {
        if (isHotSeat == true)
        {
            if (turn == true)
            {
                turn = false;
                this.GetComponentInChildren<Text>().text = "Defenders\nTurn";
                this.GetComponentInChildren<Text>().color = new Color(255, 255, 255);
            }
            else
            {
                turn = true;
                this.GetComponentInChildren<Text>().text = "Attackers\nTurn";
                this.GetComponentInChildren<Text>().color = new Color(0, 0, 0);
            }
        }
        else
        {
            if (isBlackPlayer == true)
            {
                if (turn == true)
                {
                    turn = false;
                    this.GetComponentInChildren<Text>().text = "Opponents \nTurn";
                    this.GetComponentInChildren<Text>().color = new Color(255, 255, 255);
                }
                else
                {
                    turn = true;
                    this.GetComponentInChildren<Text>().text = "Your\nTurn";
                    this.GetComponentInChildren<Text>().color = new Color(0, 0, 0);
                }
            }
            else
            {
                if (turn == true)
                {
                    turn = false;
                    this.GetComponentInChildren<Text>().text = "Your\nTurn";
                    this.GetComponentInChildren<Text>().color = new Color(255, 255, 255);
                }
                else
                {
                    turn = true;
                    this.GetComponentInChildren<Text>().text = "Opponents \nTurn";
                    this.GetComponentInChildren<Text>().color = new Color(0, 0, 0);
                }
            }
        }
    }

    //sets it to hotseat mode
    public void setHotSeat()
    {
        isHotSeat = true;
        this.GetComponentInChildren<Text>().text = "Attackers\nTurn";
    }

    //sets it to hotseat mode
    public void setNetwork(bool isBlack)
    {
        isHotSeat = false;
        isBlackPlayer = isBlack;

        if (isBlack == true)
        {
            this.GetComponentInChildren<Text>().text = "Your\nTurn";
        }
        else
        {
            this.GetComponentInChildren<Text>().text = "Opponents \nTurn";
        }
    }

    //resets the counter
    public void reset()
    {
        turn = true;
    }
}
