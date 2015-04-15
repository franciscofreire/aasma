using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class FloorGrid : MonoBehaviour {
	
	public int size_x = 50;
	public int size_z = 50;
	public float tileSize = 1.0f;

	public Texture2D terrainTiles;
	public int tileResolution;

	public TileTexInfo atlasInfo;
	public Mesh floorMesh;
	
	void Start () {
		BuildMesh();
	}

	void Update () {
	}
	
	public struct TileTexInfo {
		public Vector2[,] tileCorners; // the (0,0) corner of the tile texture
		public Vector2 tileSquareSize;
		public int NumTiles {
			get {return tileCorners.GetLength(0);}
		}
	}
	TileTexInfo MakeAtlasCoordinates(Texture2D altasTexture, int tileRes) {
		int numTilesPerRow = terrainTiles.width / tileRes;
		int numRows = terrainTiles.height / tileRes;
		
		Vector2[,] tileCorners = new Vector2[ numRows*numTilesPerRow, 4 ];
		Vector2 tileSquareSize = new Vector2( tileRes / (float) terrainTiles.width, tileRes / (float) terrainTiles.height );
		
		for(int y=0; y<numRows; y++) {
			for(int x=0; x<numTilesPerRow; x++) {
				int tileIndex = y*numTilesPerRow + x;
				Vector2 corner_0_0 = new Vector2( tileSquareSize.x*x, tileSquareSize.y*y );
				//(0,0)
				tileCorners[ tileIndex, 0 ] = corner_0_0;
				//(1,0)
				tileCorners[ tileIndex, 1 ] = new Vector2(corner_0_0.x+tileSquareSize.x, corner_0_0.y);
				//(1,1)
				tileCorners[ tileIndex, 2 ] = new Vector2(corner_0_0.x+tileSquareSize.x, corner_0_0.y+tileSquareSize.y);
				//(0,1)
				tileCorners[ tileIndex, 3 ] = new Vector2(corner_0_0.x, corner_0_0.y+tileSquareSize.y);
			}
		}
		
		TileTexInfo res;
		res.tileCorners = tileCorners;
		res.tileSquareSize = tileSquareSize;
		return res;
	}
	void SetAtlas(Texture2D atlasTexture, int tileRes) {
		TileTexInfo info = MakeAtlasCoordinates(terrainTiles, tileRes);
		atlasInfo = info;
		terrainTiles = atlasTexture;
		tileResolution = tileRes;
	}
	//A function that returns the tile to be used as the tile of the (x,z) tile.
	public delegate int GetTile(int x, int z);
	void SetTiles(GetTile tileFunction) {
		Vector2[] newUVs = new Vector2[floorMesh.uv.Length];
		for(int z=0; z < size_z; z++) {
			for(int x=0; x < size_x; x++) {
				int quadIndex = z*size_x + x;
				int quadVertexBaseIndex = quadIndex*4;
				
				int curAtlasTile = tileFunction(x,z);
				newUVs[ quadVertexBaseIndex + 0 ] = atlasInfo.tileCorners[curAtlasTile, 0];
				newUVs[ quadVertexBaseIndex + 1 ] = atlasInfo.tileCorners[curAtlasTile, 1];
				newUVs[ quadVertexBaseIndex + 2 ] = atlasInfo.tileCorners[curAtlasTile, 2];
				newUVs[ quadVertexBaseIndex + 3 ] = atlasInfo.tileCorners[curAtlasTile, 3];
			}
		}
		
		floorMesh.uv = newUVs;
	}

	void SetDebugUVs() {
		int curAtlasTile = 0;
		int atlasSize = atlasInfo.NumTiles;
		
		GetTile tileFunc = (int x, int z) => {
			int res = 0;

			GameObject g = GameObject.Find ("World");
			WorldInfo wti = (WorldInfo) g.GetComponent<WorldInfo>();
			wti.GenerateWorldTileInfo ();
			wti.SetDebugWorldTileInfo ();
			if (wti.worldTileInfo[x,z].hasTree)
				res = 1;

			curAtlasTile = (curAtlasTile+1)%atlasSize;
			return res;
		};
		SetTiles(tileFunc);
	}

	public void BuildMesh() {
		int numTiles = size_x * size_z;
		
		int vsize_x = size_x + 1;
		int vsize_z = size_z + 1;
		int numQuads = numTiles;
		int numVerts = numQuads * 4;
		int numTris = numQuads * 2;
		
		// Generate the mesh data
		Vector3[] vertices = new Vector3[ numVerts ];
		Vector3[] normals = new Vector3[ numVerts ];
		Vector2[] uv = new Vector2[ numVerts ];
		
		int[] triangles = new int[ numTris * 3 ];
		
		int x, z;
		for(z=0; z < size_z; z++) {
			for(x=0; x < size_x; x++) {
				int quadIndex = z*size_x + x;
				int quadVertexBaseIndex = quadIndex*4;

				//x=0,z=0
				vertices[ quadVertexBaseIndex + 0 ] = new Vector3( x*tileSize, 0, z*tileSize );
				normals[ quadVertexBaseIndex + 0 ] = Vector3.up;
				uv[ quadVertexBaseIndex + 0 ] = new Vector2( 0, 0 );
				//x=1,z=0
				vertices[ quadVertexBaseIndex + 1 ] = new Vector3( (x+1)*tileSize, 0, z*tileSize );
				normals[ quadVertexBaseIndex + 1 ] = Vector3.up;
				uv[ quadVertexBaseIndex + 1 ] = new Vector2( 1, 0 );
				//x=1,z=1
				vertices[ quadVertexBaseIndex + 2 ] = new Vector3( (x+1)*tileSize, 0, (z+1)*tileSize );
				normals[ quadVertexBaseIndex + 2 ] = Vector3.up;
				uv[ quadVertexBaseIndex + 2 ] = new Vector2( 1, 1 );
				//x=0,z=1
				vertices[ quadVertexBaseIndex + 3 ] = new Vector3( x*tileSize, 0, (z+1)*tileSize );
				normals[ quadVertexBaseIndex + 3 ] = Vector3.up;
				uv[ quadVertexBaseIndex + 3 ] = new Vector2( 0, 1 );
			}
		}
		Debug.Log ("Done Verts!");
		
		for(z=0; z < size_z; z++) {
			for(x=0; x < size_x; x++) {
				int quadIndex = z*size_x + x;
				int quadVertexBaseIndex = quadIndex*4;
				int corner_0_0 = quadVertexBaseIndex + 0;
				int corner_1_0 = quadVertexBaseIndex + 1;
				int corner_1_1 = quadVertexBaseIndex + 2;
				int corner_0_1 = quadVertexBaseIndex + 3;

				int triBaseIndex = quadIndex * 6;
				triangles[ triBaseIndex + 0 ] = corner_0_0;
				triangles[ triBaseIndex + 1 ] = corner_0_1;
				triangles[ triBaseIndex + 2 ] = corner_1_1;

				triangles[ triBaseIndex + 3 ] = corner_1_1;
				triangles[ triBaseIndex + 4 ] = corner_1_0;
				triangles[ triBaseIndex + 5	] = corner_0_0;
			}
		}
		Debug.Log ("Done Triangles!");
		
		// Create a new Mesh and populate with the data
		Mesh mesh = new Mesh();
		mesh.MarkDynamic();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uv;
		
		// Assign our mesh to our filter/renderer/collider
		MeshFilter mesh_filter = GetComponent<MeshFilter>();
		MeshCollider mesh_collider = GetComponent<MeshCollider>();

		floorMesh = mesh;
		mesh_filter.mesh = mesh;
		mesh_collider.sharedMesh = mesh;
		Debug.Log ("Done Mesh!");
		
		//BuildTexture();
		SetAtlas(terrainTiles, tileResolution);

		SetDebugUVs();
		MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
		mesh_renderer.sharedMaterials[0].mainTexture = terrainTiles;
	}
}