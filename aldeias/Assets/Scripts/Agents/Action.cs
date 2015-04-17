using UnityEngine;

public abstract class Action {
	public abstract void apply (WorldInfo world, Agent agent);
	public void moveAgent(WorldInfo world, Agent agent, Vector2 newPos) {
		// Translate agent to new position

	}
}

public class Walk : Action {
	public override void apply (WorldInfo world, Agent agent) {}
}

public class Turn : Action {
	public override void apply (WorldInfo world, Agent agent) {}
}

public class Attack : Action {
	public override void apply (WorldInfo world, Agent agent) {}
}

public class CutTree : Action {
	public override void apply (WorldInfo world, Agent agent) {}
}

public class PickupTree : Action {
	public override void apply (WorldInfo world, Agent agent) {}
}

public class DropTree : Action {
	public override void apply (WorldInfo world, Agent agent) {}
}

public class PlaceFlag : Action {
	public override void apply (WorldInfo world, Agent agent) {}
}

public class RemoveFlag : Action {
	public override void apply (WorldInfo world, Agent agent) {}
}

public class PickupFood : Action {
	public override void apply (WorldInfo world, Agent agent) {}
}

public class DropFood : Action {
	public override void apply (WorldInfo world, Agent agent) {}
}

public class Eat : Action {
	public override void apply (WorldInfo world, Agent agent) {}
}