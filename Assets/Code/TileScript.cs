using UnityEngine;
using System.Collections;

public class TileScript : MonoBehaviour
{
    public bool isOccupied { get; set; }
    public int pieceIndex { get; set; }
    public bool isSpecialTile { get; set; }
    public bool isPieceBlack { get; set; }
    public bool isKing { get; set; }
    public bool isThrone { get; set; }

    //sets the variables in the tile
    public void setVariables(TileScript input)
    {
        isOccupied = input.isOccupied;
        pieceIndex = input.pieceIndex;
        isPieceBlack = input.isPieceBlack;
        isKing = input.isKing;
    }

    //clears the space out
    public void clearTile()
    {
        isOccupied = false;
        pieceIndex = 0;
        isPieceBlack = false;
        isKing = false;
    }
}
