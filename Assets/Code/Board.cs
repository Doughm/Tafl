using System;
using UnityEngine;
using System.Collections.Generic;

public class Board
{
    private string boardName;
    private string imageName;
    private List<Vector2> blackPieces;
    private List<Vector2> whitePieces;
    private List<Vector2> specialSpaces;
    private Vector2 kingSpot;
    private int boardSize;

    public Board(string bName, string image, int bSize, Vector2 kSpot,
                 List<Vector2> sSpaces, List<Vector2> bPieces, List<Vector2> wPieces)
    {
        boardName = bName;
        imageName = image;
        blackPieces = bPieces;
        whitePieces = wPieces;
        specialSpaces = sSpaces;
        kingSpot = kSpot;
        boardSize = bSize;
    }

    //returns a black piece
    public Vector2 getBlackPiece(int index)
    {
        return blackPieces[index];
    }

    //returns the number of black pieces
    public int getNumberOfBlackPieces()
    {
        return blackPieces.Count;
    }

    //returns a white piece
    public Vector2 getWhitePiece(int index)
    {
        return whitePieces[index];
    }

    //returns the number of white pieces
    public int getNumberOfWhitePieces()
    {
        return whitePieces.Count;
    }

    //returns a special space
    public Vector2 getSpecialSpace(int index)
    {
        return specialSpaces[index];
    }

    //returns the number of special spaces
    public int getNumberOfSpecialSpaces()
    {
        return specialSpaces.Count;
    }

    //returns the king spot
    public Vector2 getKingSpot()
    {
        return kingSpot;
    }

    //returns the size of the board
    public Vector2 getSizeOfBoard()
    {
        return kingSpot;
    }

    //returns the name of the image
    public string getImageName()
    {
        return imageName;
    }

    //returns the size of the board
    public int getBoardSize()
    {
        return boardSize;
    }

    //returns the name of the board
    public string getBoardName()
    {
        return boardName;
    }
}