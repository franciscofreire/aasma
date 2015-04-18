using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum ORIENTATION {UP=0, DOWN=180, LEFT=270, RIGHT=90};

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

	public Agent() {
	}

	public Agent (Vector2 pos) {
		this.pos = pos;
		this.orientation = ORIENTATION.UP;
		this.energy = 100;
	}

	/// 
	/// ACTUATORS
	///

	public void move(Agent a, Vector2 target) {
		// Update worldtileInfo
		int x_origin = (int) a.pos[0];
		int z_origin = (int) a.pos[1];
		int x_target = (int) target[0];
		int z_target = (int) target[1];
		worldInfo.worldTileInfo[x_origin, z_origin].hasAgent = false;
		worldInfo.worldTileInfo[x_target, z_target].hasAgent = true;

		// Orientation
		if (x_origin > x_target) {
			a.orientation = ORIENTATION.LEFT;
		} else if (x_origin < x_target) {
			a.orientation = ORIENTATION.RIGHT;
		} else if (z_origin > z_target) {
			a.orientation = ORIENTATION.UP;
		} else {
			a.orientation = ORIENTATION.DOWN;
		}

		// Position
		a.pos[0] = target[0];
		a.pos[1] = target[1];
	}

	public abstract Action doAction();

	public abstract void OnWorldTick();
}

public struct Orientation {
	private ORIENTATION orientation;

	public static implicit operator Orientation(ORIENTATION orientation) {
		return new Orientation(orientation);
	}
	
	public static Orientation FromORIENTATION(ORIENTATION orientation) {
		return orientation;
	}

	public Vector2 ToVector2() {
		return new Vector2(Mathf.Cos((float)orientation), Mathf.Sin((float)orientation));
	}

	public Quaternion ToQuaternion() {
		return Quaternion.AngleAxis((float)orientation, Vector3.up);
	}

	private Orientation(ORIENTATION orientation) {
		this.orientation = orientation;
	}
}