using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Agent {

	//Every agent runs on it's own thread.
	//Every frame, each agent is signaled to decide what action it is going to perform.
	//   Agents that take too much time to decide will have to cope with their actions being performed late.
	//Every frame, the sensor values of each agent are computed and made available to their agents.
	//   They might be timestamped.
	//Every decision cycle, each agent copies it's sensor data to internal memory.

	//The state of every agent must be swapped on each new frame.

	public WorldInfo worldInfo;

	public Vector2 pos;
	public Orientation orientation;
	public int energy; // 0: No energy; 100: Full energy

	protected const int CRITICAL_ENERGY_LEVEL = 20;

	public struct SensorData {
		public IList<Vector2I> _cells;
		public Vector2I _front_cell;

		public IList<Vector2I> Cells
		{
			get { return _cells; }
			set { _cells = value; }
		}
		
		public Vector2I FrontCell
		{
			get { return _front_cell; }
			set { _front_cell = value; }
		}
		
		public SensorData(IList<Vector2I> cells, Vector2I front_cell)
		{
			_cells = cells;
			_front_cell = front_cell;
		}
	}
	public SensorData sensorData;

	public Agent() {
	}

	public Agent (Vector2 pos) {
		this.pos = pos;
		this.orientation = ORIENTATION.UP;
		this.energy = 100;
	}

	public abstract Action doAction();

	public abstract void OnWorldTick();
	
	public void updateSensorData() {
		sensorData.Cells = worldInfo.nearbyFreeCells(worldInfo.nearbyCells(this));
		
		Vector2 posInFront = pos + orientation.ToVector2();
		Vector2I tileCoordInFront = worldInfo.AgentPosToWorldXZ(posInFront);
		sensorData.FrontCell = worldInfo.isInsideWorld(tileCoordInFront)
			? tileCoordInFront
			: new Vector2I(pos); // FIXME: Not sure about this...
	}

    public abstract bool IsAlive();

	//*************
	//** SENSORS **
	//*************

	public abstract bool EnemyInFront();

	public bool TreeInFront() {
		return worldInfo.WorldTileInfoAtCoord(sensorData.FrontCell).tree.hasTree;
	}
	
	public bool StumpInFront() {
		return worldInfo.WorldTileInfoAtCoord(sensorData.FrontCell).tree.isStump;
	}
}