using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FloorGrid : MonoBehaviour {

	public FloorTile tilePrefab;
	public int rows = 50;
	public int columns = 50;

	private List<List<FloorTile>> tileMatrix;

	// Use this for initialization
	void Start () {
		initializeTileMatrix();
	}

	void initializeTileMatrix() {
		tileMatrix = new List<List<FloorTile>>(rows);

		Quaternion tileRotation = Quaternion.AngleAxis(90, Vector3.right);
		for(int row=0; row < rows; row++) {
			List<FloorTile> rowList = new List<FloorTile>(columns);
			tileMatrix.Add(rowList);
			for(int column=0; column < columns; column++) {
				GameObject tile = (GameObject) Object.Instantiate(tilePrefab.gameObject, Vector3.right * row + Vector3.forward * column, tileRotation);
				tile.transform.parent = transform;
				FloorTile floorTile = tile.GetComponent<FloorTile>();
				floorTile.initTile(row, column);
				rowList.Add(floorTile);
			}
		}
	}
}
