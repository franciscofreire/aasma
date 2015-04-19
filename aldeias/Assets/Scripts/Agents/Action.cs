using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Action {
	protected Agent agent;
	protected Vector2I target;

	// Atributes for quick access
	protected IList<Vector2I> cells;
	protected WorldInfo world;

	public abstract void apply ();

	public Action(Agent agent, Vector2I target) {
		this.agent  = agent;
		this.target = target;	

		this.cells = agent.sensorData.Cells;
		this.world = agent.worldInfo;
	}
}

public class Walk : Action {
	public override void apply () {
		// Update worldtileInfo
		WorldInfo.WorldTileInfo agentTileInfo = 
			world.WorldTileInfoAtCoord(world.AgentPosToWorldXZ(agent.pos));
		WorldInfo.WorldTileInfo targetTileInfo = 
			world.WorldTileInfoAtCoord(target);
		agentTileInfo.hasAgent = false;
		targetTileInfo.hasAgent = true;
		
		// Orientation
		int x_origin = (int) agent.pos[0];
		int z_origin = (int) agent.pos[1];
		int x_target = (int) target.x;
		int z_target = (int) target.y;
		if (x_origin > x_target) {
			agent.orientation = ORIENTATION.LEFT;
		} else if (x_origin < x_target) {
			agent.orientation = ORIENTATION.RIGHT;
		} else if (z_origin > z_target) {
			agent.orientation = ORIENTATION.UP;
		} else {
			agent.orientation = ORIENTATION.DOWN;
		}
		
		// Position
		agent.pos = target.ToVector2();
	}
	public Walk(Agent agent, Vector2I target) : base(agent, target) {}
}

public class Attack : Action {
	public override void apply () {}
	public Attack(Agent agent, Vector2I target) : base(agent, target) {}
}

public class CutTree : Action {
	public override void apply () {
		if (world.WorldTileInfoAtCoord(target).tree.isStump == false) {
			world.WorldTileInfoAtCoord(target).tree.turnToStump = true;
			world.WorldTileInfoAtCoord(target).tree.isStump = true;
		}
	}
	public CutTree(Agent agent, Vector2I target) : base(agent, target) {}
}

public class PickupTree : Action {
	public override void apply () {
		world.WorldTileInfoAtCoord(target).tree.Chop();
		((Habitant) agent).isCarryingWood = true;
	}
	public PickupTree(Agent agent, Vector2I target) : base(agent, target) {}
}

public class DropTree : Action {
	public override void apply () {}
	public DropTree(Agent agent, Vector2I target) : base(agent, target) {}
}

public class PlaceFlag : Action {
	public override void apply () {}
	public PlaceFlag(Agent agent, Vector2I target) : base(agent, target) {}
}

public class RemoveFlag : Action {
	public override void apply () {}
	public RemoveFlag(Agent agent, Vector2I target) : base(agent, target) {}
}

public class PickupFood : Action {
	public override void apply () {}
	public PickupFood(Agent agent, Vector2I target) : base(agent, target) {}
}

public class DropFood : Action {
	public override void apply () {}
	public DropFood(Agent agent, Vector2I target) : base(agent, target) {}
}

public class Eat : Action {
	public override void apply () {}
	public Eat(Agent agent, Vector2I target) : base(agent, target) {}
}