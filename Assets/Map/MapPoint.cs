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
      case CardinalDirection.NorthEast:
        return new MapPoint(x + distance, y + distance);
      case CardinalDirection.NorthWest:
        return new MapPoint(x - distance, y + distance);
      case CardinalDirection.SouthEast:
        return new MapPoint(x + distance, y - distance);
      case CardinalDirection.SouthWest:
        return new MapPoint(x - distance, y - distance);
      default:
          Debug.Log("Big Problem!");
          return new MapPoint(x - distance, y);
    }
  }
  
  public float DistanceFromMapPoint(MapPoint otherPoint) {
    float dx = otherPoint.x - x;
    float dy = otherPoint.y - y;
    return Mathf.Sqrt(Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2));
  }

  public CardinalDirection DirectionToMapPoint(MapPoint otherPoint) {
      
    if (otherPoint.x > x) {
      if (otherPoint.y > y)
        return CardinalDirection.NorthEast;
      else if (otherPoint.y < y)
        return CardinalDirection.SouthEast;
      else
        return CardinalDirection.East;
    }
    
    if (otherPoint.x < x) {
      if (otherPoint.y > y)
        return CardinalDirection.NorthWest;
      else if (otherPoint.y < y)
        return CardinalDirection.SouthWest;
      else
        return CardinalDirection.West;
    }
    
    if (otherPoint.y > y) {
      return CardinalDirection.North;
    }
    
    if (otherPoint.y < y) {
      return CardinalDirection.South;
    }
    
    return CardinalDirection.North;
  }


}
