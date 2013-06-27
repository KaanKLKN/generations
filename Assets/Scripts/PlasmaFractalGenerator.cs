using UnityEngine;
using System.Collections;

public class PlasmaFractalGenerator {

  public double gRoughness;
  public double gBigSize;

  public double[,] Generate(int iWidth, int iHeight, double iRoughness)
  {
      double c1, c2, c3, c4;
      double[,] points = new double[iWidth+1, iHeight+1];
      
      //Assign the four corners of the intial grid random color values
      //These will end up being the colors of the four corners of the applet.   
      c1 = Random.value;
      c2 = Random.value;
      c3 = Random.value;
      c4 = Random.value;
      gRoughness = iRoughness;
      gBigSize = iWidth + iHeight;
      DivideGrid(ref points, 0, 0, iWidth, iHeight, c1, c2, c3, c4);
      return points;
  }

  public void DivideGrid(ref double[,] points, double x, double y, double width, double height, double c1, double c2, double c3, double c4)
  {
      double Edge1, Edge2, Edge3, Edge4, Middle;

      double newWidth = Mathf.Floor((float)(width / 2));
      double newHeight = Mathf.Floor((float)(height / 2));

      if (width > 1 || height > 1)
      {
          Middle = ((c1 + c2 + c3 + c4) / 4)+Displace(newWidth + newHeight);  //Randomly displace the midpoint!
          Edge1 = ((c1 + c2) / 2);  //Calculate the edges by averaging the two corners of each edge.
          Edge2 = ((c2 + c3) / 2);
          Edge3 = ((c3 + c4) / 2);
          Edge4 = ((c4 + c1) / 2);//
          //Make sure that the midpoint doesn't accidentally "randomly displaced" past the boundaries!
          Middle= Rectify(Middle);
          Edge1 = Rectify(Edge1);
          Edge2 = Rectify(Edge2);
          Edge3 = Rectify(Edge3);
          Edge4 = Rectify(Edge4);
          //Do the operation over again for each of the four new grids.     
          DivideGrid(ref points, x, y, newWidth, newHeight, c1, Edge1, Middle, Edge4);
          DivideGrid(ref points, x + newWidth, y, width - newWidth, newHeight, Edge1, c2, Edge2, Middle);
          DivideGrid(ref points, x + newWidth, y + newHeight, width - newWidth, height - newHeight, Middle, Edge2, c3, Edge3);
          DivideGrid(ref points, x, y + newHeight, newWidth, height - newHeight, Edge4, Middle, Edge3, c4);
      }
      else  //This is the "base case," where each grid piece is less than the size of a pixel.
      {
          //The four corners of the grid piece will be averaged and drawn as a single pixel.
          double c = (c1 + c2 + c3 + c4) / 4;

          points[(int)(x), (int)(y)] = c;
          if (width == 2)
          {
              points[(int)(x+1), (int)(y)] = c;
          }
          if (height == 2)
          {
              points[(int)(x), (int)(y+1)] = c;
          }
          if ((width == 2) && (height == 2)) 
          {
              points[(int)(x + 1), (int)(y+1)] = c;
          }
      }
  }

  private double Displace(double SmallSize)
  {
      
      double Max = SmallSize/ gBigSize * gRoughness;
      return (Random.value - 0.5) * Max;
  }

  private double Rectify(double iNum)
  {
      if (iNum < 0)
      {
          iNum = 0;
      }
      else if (iNum > 1.0)
      {
          iNum = 1.0;
      }
      return iNum;
  }

}
