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

	private bool worldHasChanged = false;
	
	void Start() {
		worldInfo.AddCreationListener(()=>{
			CreateObjects();
		});
		worldInfo.AddChangeListener(()=>{worldHasChanged=true;});
	}
	
	void Update() {
		if(worldHasChanged) {
			ApplyWorldInfo();
			worldHasChanged = false;
		}
	}
	
	public Vector3 worldXZToVec3(int x, int z) {
		float halfTileSize = tileSize * 0.5f;
		return new Vector3(x + halfTileSize, 0, z + halfTileSize);
	}
	
	public abstract void CreateObjects();
	
	public abstract void ApplyWorldInfo();
}