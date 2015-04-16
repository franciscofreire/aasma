﻿using UnityEngine;
using System.Collections;

public class HabitatLayer : MonoBehaviour {

	public WorldInfo worldInfo;
	public int size_z {
		get {
			return worldInfo.zSize;
		}
	}
	public int size_x {
		get {
			return worldInfo.xSize;
		}
	}
	public float tileSize = 1.0f;

	private bool worldHasChanged=false;
	private Mesh layerMesh;

	void Start() {
		BuildMesh();
		worldInfo.AddChangeListener(()=>{worldHasChanged=true;});
	}

	void Update() {
		if(worldHasChanged){
			SetHabitatFacesFromWorldInfo();
			worldHasChanged=false;
		}
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

		//Default triangles
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
		
		layerMesh = mesh;
		mesh_filter.mesh = mesh;
		mesh_collider.sharedMesh = mesh;
		Debug.Log ("Done Mesh!");
		
		//MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
		//mesh_renderer.sharedMaterials[0].mainTexture = terrainTiles;
	}

	private void SetHabitatFacesFromWorldInfo() {
		int[] triangles = layerMesh.triangles;

		for(int z=0; z < size_z; z++) {
			for(int x=0; x < size_x; x++) {
				int quadIndex = z*size_x + x;

				int corner_0_0;
				int corner_1_0;
				int corner_1_1;
				int corner_0_1;
				if(worldInfo.worldTileInfo[x,z].isHabitat) {
					int quadVertexBaseIndex = quadIndex*4;
					corner_0_0 = quadVertexBaseIndex + 0;
					corner_1_0 = quadVertexBaseIndex + 1;
					corner_1_1 = quadVertexBaseIndex + 2;
					corner_0_1 = quadVertexBaseIndex + 3;
				} else {
					corner_0_0 = 0;
					corner_1_0 = 0;
					corner_1_1 = 0;
					corner_0_1 = 0;
				}

				int triBaseIndex = quadIndex * 6;
				triangles[ triBaseIndex + 0 ] = corner_0_0;
				triangles[ triBaseIndex + 1 ] = corner_0_1;
				triangles[ triBaseIndex + 2 ] = corner_1_1;
				
				triangles[ triBaseIndex + 3 ] = corner_1_1;
				triangles[ triBaseIndex + 4 ] = corner_1_0;
				triangles[ triBaseIndex + 5	] = corner_0_0;
			}
		}
		layerMesh.triangles = triangles;
	}
}