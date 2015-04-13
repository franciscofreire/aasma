
public abstract class Action {
	public abstract void apply (WorldInfo world, Agent agent);
}

public class Walk : Action {

}

public class Turn : Action {

}

public class Attack : Action {

}

public class CutTree : Action {

}

public class PickupTree : Action {

}

public class DropTree : Action {

}

public class PlaceFlag : Action {

}

public class RemoveFlag : Action {

}

public class PickupFood : Action {

}

public class DropFood : Action {

}

public class Eat : Action {

}