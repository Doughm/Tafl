using UnityEngine;
using UnityEngine.UI;

public class BoardPreview : MonoBehaviour
{

    public Sprite aleaevangelii;
    public Sprite ardri;
    public Sprite brandubh;
    public Sprite hnefatafl;
    public Sprite tablut;
    public Sprite tawlbwrdd;

    //sets the sprite image
    public void setSprite(string boardImage)
    {
        switch (boardImage)
        {
            case "Brandubh":
                this.GetComponent<Image>().sprite = brandubh;
                break;

            case "Ard Ri":
                this.GetComponent<Image>().sprite = ardri;
                break;

            case "Tablut":
                this.GetComponent<Image>().sprite = tablut;
                break;

            case "Hnefatafl":
                this.GetComponent<Image>().sprite = hnefatafl;
                break;

            case "Tawlbwrdd":
                this.GetComponent<Image>().sprite = tawlbwrdd;
                break;

            case "Alea Evangelii":
                this.GetComponent<Image>().sprite = aleaevangelii;
                break;
        }
    }
}
