using UnityEngine;
using System.Collections.Generic;

public class DTileMap {
	int size_x;
	int size_y;
	
	int[,] map_data;

	/*
	 * 0 = neutral
	 * 1 = blue tribe
	 * 2 = red tribe
	 */
	
	public DTileMap(int size_x, int size_y) {
		this.size_x = size_x;
		this.size_y = size_y;
		
		map_data = new int[size_x,size_y];
		
		for(int x=0;x<size_x;x++) {
			for(int y=0;y<size_y;y++) {
				map_data[x,y] = 0;
			}
		}

		// Start locations for 2 tribes
		map_data[2,2] = 1;
		map_data[size_x - 2, size_y - 4] = 2;
	}
	
	public int GetTileAt(int x, int y) {
		return map_data[x,y];
	}
	
	public void SetTileAt(int x, int y, int value) {
		map_data[x,y] = value;
	}
}	
