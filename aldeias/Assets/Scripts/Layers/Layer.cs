using UnityEngine;
using System.Collections;

public abstract class Layer : MonoBehaviour {
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
	
	protected float moveSpeed = 10f;

	private bool worldHasChanged = false;
	
	void Start() {
		worldInfo.AddCreationListener(()=>{
			CreateObjects();
		});
		worldInfo.AddChangeListener(()=>{
            worldHasChanged = true;
        });
	}
	
	void Update() {
		if(worldHasChanged) {
			ApplyWorldInfo();
			worldHasChanged = false;
		}
	}
	
	public Vector3 WorldXZToVec3(int x, int z) {
		float halfTileSize = tileSize * 0.5f;
		return new Vector3(x + halfTileSize, 0, z + halfTileSize);
	}

	public Vector3 WorldXZToVec3(Vector2I xz) {
		return WorldXZToVec3(xz.x, xz.y);
	}

	public Vector3 AgentPosToVec3(Vector2 pos) {
		float halfTileSize = tileSize / 2.0f;
		return new Vector3(pos.x + halfTileSize, 0, pos.y + halfTileSize);
	}

	public abstract void CreateObjects();
	
	public abstract void ApplyWorldInfo();
}