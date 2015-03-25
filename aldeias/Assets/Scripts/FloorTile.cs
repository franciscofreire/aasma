using UnityEngine;
using System.Collections;

public class FloorTile : MonoBehaviour {
	
	public int row;
	public int column;

	public void initTile (int row, int column) {
		this.row = row;
		this.column = column;
	}

}
