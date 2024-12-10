using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WallTypesHelper
{
 
    // The following sets represent the configurations of different wall tiles with respect to their adjacent tiles.
    // Each value is a binary number, where each bit represents a specific direction:
    //   - The 1st bit represents the top side.
    //   - The 2nd bit represents the left side.
    //   - The 3rd bit represents the right side.
    //   - The 4th bit represents the bottom side.
    //   - Diagonal sides are represented by different sets.
    // The presence of a 1 indicates a tile presence, while 0 indicates absence.



    public static HashSet<int> wallTop = new HashSet<int>
    {
        0b1111,  // A wall with all directions occupied.
        0b0110,  // A wall with only left and right occupied.
        0b0011,  // A wall with only bottom and right occupied.
        0b0010,  // A wall with only bottom occupied.
        0b1010,  // A wall with left and bottom occupied.
        0b1100,  // A wall with left and top occupied.
        0b1110,  // A wall with top, right, and bottom occupied.
        0b1011,  // A wall with top, left, and bottom occupied.
        0b0111   // A wall with top, right, and left occupied.
    };
    // Walls that have tiles on the left side (left direction)
    public static HashSet<int> wallSideLeft = new HashSet<int>
    {
        0b0100
    };
    // Walls that have tiles on the right side (right direction)
    public static HashSet<int> wallSideRight = new HashSet<int>
    {
        0b0001
    };
    // Walls that have tiles on the bottom side (bottom direction)
    public static HashSet<int> wallBottm = new HashSet<int>
    {
        0b1000
    };
    // Inner corner walls with a downward left corner.
    public static HashSet<int> wallInnerCornerDownLeft = new HashSet<int>
    {
        0b11110001,
        0b11100000,
        0b11110000,
        0b11100001,
        0b10100000,
        0b01010001,
        0b11010001,
        0b01100001,
        0b11010000,
        0b01110001,
        0b00010001,
        0b10110001,
        0b10100001,
        0b10010000,
        0b00110001,
        0b10110000,
        0b00100001,
        0b10010001
    };
    // Inner corner walls with a downward right corner.
    public static HashSet<int> wallInnerCornerDownRight = new HashSet<int>
    {
        0b11000111,
        0b11000011,
        0b10000011,
        0b10000111,
        0b10000010,
        0b01000101,
        0b11000101,
        0b01000011,
        0b10000101,
        0b01000111,
        0b01000100,
        0b11000110,
        0b11000010,
        0b10000100,
        0b01000110,
        0b10000110,
        0b11000100,
        0b01000010

    };
    // Diagonal corner wall pointing down left
    public static HashSet<int> wallDiagonalCornerDownLeft = new HashSet<int>
    {
        0b01000000
    };
    // Diagonal corner wall pointing down right
    public static HashSet<int> wallDiagonalCornerDownRight = new HashSet<int>
    {
        0b00000001
    };
    // Diagonal corner wall pointing up left
    public static HashSet<int> wallDiagonalCornerUpLeft = new HashSet<int>
    {
        0b00010000,
        0b01010000,
    };
    // Diagonal corner wall pointing up right
    public static HashSet<int> wallDiagonalCornerUpRight = new HashSet<int>
    {
        0b00000100,
        0b00000101
    };
    // Full walls with standard configurations (not diagonals or corners).
    public static HashSet<int> wallFull = new HashSet<int>
    {
        0b1101,
        0b0101,
        0b1101,
        0b1001

    };
    // Walls that cover all eight directions (top, left, right, bottom, plus diagonals)
    public static HashSet<int> wallFullEightDirections = new HashSet<int>
    {
        0b00010100,
        0b11100100,
        0b10010011,
        0b01110100,
        0b00010111,
        0b00010110,
        0b00110100,
        0b00010101,
        0b01010100,
        0b00010010,
        0b00100100,
        0b00010011,
        0b01100100,
        0b10010111,
        0b11110100,
        0b10010110,
        0b10110100,
        0b11100101,
        0b11010011,
        0b11110101,
        0b11010111,
        0b11010111,
        0b11110101,
        0b01110101,
        0b01010111,
        0b01100101,
        0b01010011,
        0b01010010,
        0b00100101,
        0b00110101,
        0b01010110,
        0b11010101,
        0b11010100,
        0b10010101

    };
    // Walls with tiles on the bottom side and in all eight directions
    public static HashSet<int> wallBottmEightDirections = new HashSet<int>
    {
        0b01000001
    };

}