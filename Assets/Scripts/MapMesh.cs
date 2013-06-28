using UnityEngine;
using System.Collections;

public class MapMesh : MonoBehaviour {

  public Map map;

  Texture2D heightMap;
  public Material material;

  void  Start (){
    //GenerateHeightmap();
  }

  public void GenerateFromTiles(MapTile[,] tiles) {

    int mapWidth   = map.size;
    int mapHeight  = map.size;
    float tileSize = map.tileSize;
    float padding   = map.tilePadding;

    Vector3 origin = new Vector3(tileSize, 0, tileSize);

    // Create the game object containing the renderer
    gameObject.AddComponent<MeshFilter>();
    gameObject.AddComponent<MeshRenderer>();

    if (material)
      renderer.material = material;
    else
      renderer.material.color = Color.white;

    // Retrieve a mesh instance
    Mesh mesh = GetComponent<MeshFilter>().mesh;

    int width = mapWidth * 2;
    int height = mapHeight * 2;

    int y = 0;
    int x = 0;

    // Convert tiles into vertices
    Vector3[,] rawVertices = new Vector3[width, height];
    for (y=0; y < mapHeight; y++) {
      for (x=0; x < mapWidth; x++) {
        MapTile tile = tiles[x, y];

        float h = tile.height * map.maxElevation;

        rawVertices[x*2,      y*2]      = new Vector3(x*padding + (x-1)*tileSize,  h,  y*padding + (y-1)*tileSize);
        rawVertices[x*2 + 1,  y*2]      = new Vector3(x*padding + x*tileSize,      h,  y*padding + (y-1)*tileSize);
        rawVertices[x*2,      y*2 + 1]  = new Vector3(x*padding + (x-1)*tileSize,  h,  y*padding + y*tileSize);
        rawVertices[x*2 + 1,  y*2 + 1]  = new Vector3(x*padding + x*tileSize,      h,  y*padding + y*tileSize);

      }
    }

    // Normalize vertices and create UVs
    Vector3[] vertices= new Vector3[height * width];
    Vector2[] uv= new Vector2[height * width];
    
    Vector2 uvScale = new Vector2 (1.0f / (width - 1), 1.0f / (height - 1));
    
    for (y=0; y < height; y++) {
      for (x=0; x < width; x++) {

        Vector3 vertex = rawVertices[x, y];
        vertices[y * width + x] = origin + vertex;

        uv[y * width + x] = Vector2.Scale(new Vector2(vertex.x, vertex.z), uvScale);

        // Calculate tangent vector: a vector that goes from previous vertex
        // to next along X direction. We need tangents if we intend to
        // use bumpmap shaders on the mesh.
        //Vector3 vertexL= new Vector3( x-1, heightMap.GetPixel(x-1, y).grayscale, y );
        //Vector3 vertexR= new Vector3( x+1, heightMap.GetPixel(x+1, y).grayscale, y );
        //Vector3 tan= Vector3.Scale( sizeScale, vertexR - vertexL ).normalized;
        //tangents[y*width + x] = new Vector4( tan.x, tan.y, tan.z, -1.0f );
      }
    }
    
    // Assign them to the mesh
    mesh.vertices = vertices;
    mesh.uv = uv;

    // Build triangle indices: 3 indices into vertex array for each triangle
    int[] triangles= new int[(height - 1) * (width - 1) * 6];
    int index= 0;
    for (y=0;y<height-1;y++) {
      for (x=0;x<width-1;x++) {
        // For each grid cell output two triangles
        triangles[index++] = (y     * width) + x;
        triangles[index++] = ((y+1) * width) + x;
        triangles[index++] = (y     * width) + x + 1;

        triangles[index++] = ((y+1) * width) + x;
        triangles[index++] = ((y+1) * width) + x + 1;
        triangles[index++] = (y     * width) + x + 1;
      }
    }

    // And assign them to the mesh
    mesh.triangles = triangles;
      
    // Auto-calculate vertex normals from the mesh
    mesh.RecalculateNormals();
    mesh.Optimize();
    
    // Assign tangents after recalculating normals
    CalculateMeshTangents(mesh);

    GenerateTexture(tiles);
  }

  public static void CalculateMeshTangents(Mesh mesh) {
      //speed up math by copying the mesh arrays
      int[] triangles = mesh.triangles;
      Vector3[] vertices = mesh.vertices;
      Vector2[] uv = mesh.uv;
      Vector3[] normals = mesh.normals;
   
      //variable definitions
      int triangleCount = triangles.Length;
      int vertexCount = vertices.Length;
   
      Vector3[] tan1 = new Vector3[vertexCount];
      Vector3[] tan2 = new Vector3[vertexCount];
   
      Vector4[] tangents = new Vector4[vertexCount];
   
      for (long a = 0; a < triangleCount; a += 3)
      {
          long i1 = triangles[a + 0];
          long i2 = triangles[a + 1];
          long i3 = triangles[a + 2];
   
          Vector3 v1 = vertices[i1];
          Vector3 v2 = vertices[i2];
          Vector3 v3 = vertices[i3];
   
          Vector2 w1 = uv[i1];
          Vector2 w2 = uv[i2];
          Vector2 w3 = uv[i3];
   
          float x1 = v2.x - v1.x;
          float x2 = v3.x - v1.x;
          float y1 = v2.y - v1.y;
          float y2 = v3.y - v1.y;
          float z1 = v2.z - v1.z;
          float z2 = v3.z - v1.z;
   
          float s1 = w2.x - w1.x;
          float s2 = w3.x - w1.x;
          float t1 = w2.y - w1.y;
          float t2 = w3.y - w1.y;
   
          float r = 1.0f / (s1 * t2 - s2 * t1);
   
          Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
          Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
   
          tan1[i1] += sdir;
          tan1[i2] += sdir;
          tan1[i3] += sdir;
   
          tan2[i1] += tdir;
          tan2[i2] += tdir;
          tan2[i3] += tdir;
      }
   
   
      for (long a = 0; a < vertexCount; ++a)
      {
          Vector3 n = normals[a];
          Vector3 t = tan1[a];
   
          //Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
          //tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
          Vector3.OrthoNormalize(ref n, ref t);
          tangents[a].x = t.x;
          tangents[a].y = t.y;
          tangents[a].z = t.z;
   
          tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
      }
   
      mesh.tangents = tangents;
  }

  public void GenerateTexture(MapTile[,] tiles) {

    Texture2D texture = new Texture2D(map.size * 2 - 1, map.size * 2 - 1, TextureFormat.RGB24, true);
    texture.wrapMode = TextureWrapMode.Clamp;

    for (int y=0; y < map.size; y++) {
      for (int x=0; x < map.size; x++) {

        MapTile tile = tiles[x, y];
        MapTile nextX = null;
        if (x + 1 < map.size)
          nextX = tiles[x + 1, y];
        MapTile nextY = null;
        if (y + 1 < map.size)
          nextY = tiles[x, y + 1];
        MapTile nextBoth = null;
        if (y + 1 < map.size && x + 1 < map.size)
          nextBoth = tiles[x + 1, y + 1];


        texture.SetPixel(x * 2, y * 2, tile.TileColor());
        if (nextX != null)
          texture.SetPixel(x * 2 + 1, y * 2, Color.Lerp(tile.TileColor(), nextX.TileColor(), 0.5F));
        if (nextBoth != null)
          texture.SetPixel(x * 2 + 1, y * 2 + 1, Color.Lerp(tile.TileColor(), nextBoth.TileColor(), 0.5F));
        if (nextY != null)
          texture.SetPixel(x * 2, y * 2 + 1, Color.Lerp(tile.TileColor(), nextY.TileColor(), 0.5F));

      }
    }

    texture.Apply();

    renderer.material.mainTexture = texture;

  }


}
