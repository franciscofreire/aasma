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

	public Agent() {
	}

	public Agent (Vector2 pos) {
		this.pos = pos;
		this.orientation = Orientation.forward;
		this.energy = 100;
	}

	/// 
	/// ACTUATORS
	///

	public void move(Agent a, Vector2 target) {
		a.pos[0] = (int) target[0];
		a.pos[1] = (int) target[1];
		
		// Update worldtileInfo
		worldInfo.worldTileInfo[(int)  a.pos[0], (int)  a.pos[1]].hasAgent = false;
		worldInfo.worldTileInfo[(int) target[0], (int) target[1]].hasAgent = true;
	}

	public abstract Action doAction();

	public abstract void OnWorldTick();
}

public struct Orientation {
	float angleToZ;

	public static Orientation FromAngleToZ(float angle) {
		return new Orientation(0);
	}

	public static Orientation forward {
		get { return FromAngleToZ(0); }
	}

	public Vector2 ToVector2() {
		return new Vector2(Mathf.Cos(angleToZ), Mathf.Sin(angleToZ));
	}

	public Quaternion ToQuaternion() {
		return Quaternion.AngleAxis(angleToZ+90.0f, Vector3.up);
	}

	private Orientation(float angleToZ) {
		this.angleToZ = angleToZ;
	}
}