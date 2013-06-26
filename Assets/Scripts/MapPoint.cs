using UnityEngine;
using System.Collections;

public class MapPoint  {
  public int x;
  public int y;

  public MapPoint() {}

  public MapPoint(int atX, int atY) {
      x = atX;
      y = atY;
  }
  
  public MapPoint NearestMapPointInDirection(CardinalDirection direction) {
    return MapPointInDirectionAtDistance(direction, 1);
  }
  
  public MapPoint MapPointInDirectionAtDistance(CardinalDirection direction, int distance) {
    switch (direction){
      case CardinalDirection.North:
        return new MapPoint(x, y + distance);
      case CardinalDirection.South:
        return new MapPoint(x, y - distance);
      case CardinalDirection.East:
        return new MapPoint(x + distance, y);
      case CardinalDirection.West:
        return new MapPoint(x - distance, y);
      default:
          Debug.Log("Big Problem!");
          return new MapPoint(x - distance, y);
    }
  }
    
}
