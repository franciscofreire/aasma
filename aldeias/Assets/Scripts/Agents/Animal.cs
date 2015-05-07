using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class AnimalBoidParameters {
    /* Vehicle model parameters. */
    public float MaximumSpeed = 0.75f;
    public float MaximumSpeedDelta = 0.05f;
    public float MaximumDirDelta = 0.25f;

    /* Boid decision parameters. */
    public float SeparatioWeight = 5.0f;
    public float AlignmentWeight = 1.0f;
    public float CohesionWeight = 1.0f;
    public float AvoidBorderWeight = 10f;

    /* Boid perception parameters. */
    public float MaxVisDist = 5.0f;
    public Degrees HalfFieldOfViewAngle = new Degrees(330f/2f);
}

public class AnimalBoidImplementation : AgentImplementation {
    Animal animal;
    AnimalBoidParameters BoidParams {
        get {
            return animal.BoidParams;
        }
    }
    IEnumerable<Animal> Neighbors {
        get {
            return animal.Neighbors;
        }
    }
    Vector2I WorldSize {
        get {
            return animal.worldInfo.Size;
        }
    }
    Vector2 Pos {
        get {
            return animal.pos;
        }
    }

    public AnimalBoidImplementation(Animal animal) {
        this.animal = animal;
    }

    public Action doAction() {
        return new AnimalAccelerate(animal, BoidAcceleration());
    }

    private Vector2 BoidAcceleration() {
        Vector2?[] accs = new Vector2?[]{SeparationAcceleration(), AlignmentAcceleration(), CohesionAcceleration(), AvoidBorderAcceleration()};
        Vector2 a = accs.Select(acc=>acc??Vector2.zero).Aggregate((a1,a2)=>a1+a2);
        return a;
    }

    private Vector2? SeparationAcceleration() {
        if (!Neighbors.Any())
            return null;
        Vector2 neighborDistSum = Neighbors.Select(a=>a.pos-Pos).Aggregate(Vector2.zero,(v1,v2)=>v1+v2);
        Vector2 neighborDistMean = neighborDistSum / (float)Neighbors.Count();
        Vector2 acc = (-neighborDistMean / neighborDistMean.sqrMagnitude) * BoidParams.SeparatioWeight;
        Debug.DrawRay(new Vector3(Pos.x, 2, Pos.y), new Vector3(acc.x, 0, acc.y), Color.red);
        return acc;
    }
    
    private Vector2? AlignmentAcceleration() {
        if (!Neighbors.Any())
            return null;
        Radians neighborOrientSum = Neighbors.Select(a=>a.orientation.ToRadiansToUp()).Aggregate(new Radians(0), (r1,r2)=>r1+r2);
        Orientation desiredOrientation = Orientation.FromRadians(new Radians(neighborOrientSum / (float)Neighbors.Count()));
        Vector2 acc = desiredOrientation.ToVector2() * BoidParams.AlignmentWeight;
        //Debug.DrawRay(new Vector3(pos.x, 2, pos.y), new Vector3(acc.x, 0, acc.y), Color.blue);
        return acc;
    }
    
    private Vector2? CohesionAcceleration() {
        if (!Neighbors.Any())
            return null;
        Vector2 neighborPosSum = Neighbors.Select(a=>a.pos).Aggregate(Vector2.zero, (p1,p2)=>p1+p2);
        Vector2 neighborMeanPos = neighborPosSum / (float) Neighbors.Count();
        Vector2 toDesiredPosition = neighborMeanPos - Pos;
        Vector2 acc = toDesiredPosition * BoidParams.CohesionWeight;
        //Debug.DrawRay(new Vector3(pos.x, 2, pos.y), new Vector3(acc.x, 0, acc.y), Color.green);
        return acc;
    }
    
    private Vector2? AvoidBorderAcceleration() {
        //TODO: Transform the way this acceleration is compute. It is currently done by looking directly into the WorldInfo.
        //TODO:    Perhaps adding a perception of the distance to the closest pair of borders.
        System.Func<Vector2,Vector2> leftForceField = (Vector2 apos)=>{
            Vector2 awayFromEdgeForce = Vector2.right;
            float maximumForceCoord = 0;//At edge.
            float minimumForceCoord = 10;//Where the force becomes zero.
            
            float closenessToEdge = Mathf.InverseLerp(minimumForceCoord,maximumForceCoord,apos.x);
            return awayFromEdgeForce * closenessToEdge;
        };
        System.Func<Vector2,Vector2> rightForceField = (Vector2 apos)=>{
            Vector2 awayFromEdgeForce = -Vector2.right;
            float maximumForceCoord = WorldSize.x;//At edge.
            float minimumForceCoord = WorldSize.x-10;//Where the force becomes zero.
            
            float closenessToEdge = Mathf.InverseLerp(minimumForceCoord,maximumForceCoord,apos.x);
            return awayFromEdgeForce * closenessToEdge;
        };
        System.Func<Vector2,Vector2> topForceField = (Vector2 apos)=>{
            Vector2 awayFromEdgeForce = -Vector2.up;
            float maximumForceCoord = WorldSize.y;//At edge.
            float minimumForceCoord = WorldSize.y-10;//Where the force becomes zero.
            
            float closenessToEdge = Mathf.InverseLerp(minimumForceCoord,maximumForceCoord,apos.y);
            return awayFromEdgeForce * closenessToEdge;
        };
        System.Func<Vector2,Vector2> bottomForceField = (Vector2 apos)=>{
            Vector2 awayFromEdgeForce = Vector2.up;
            float maximumForceCoord = 0;//At edge.
            float minimumForceCoord = 10;//Where the force becomes zero.
            
            float closenessToEdge = Mathf.InverseLerp(minimumForceCoord,maximumForceCoord,apos.y);
            return awayFromEdgeForce * closenessToEdge;
        };
        Vector2 forceFieldSum = leftForceField(Pos)+rightForceField(Pos)+bottomForceField(Pos)+topForceField(Pos);
        Vector2 acc = forceFieldSum * BoidParams.AvoidBorderWeight;
        Debug.DrawRay(new Vector3(Pos.x, 2, Pos.y), new Vector3(acc.x, 0, acc.y), Color.magenta);
        return acc;
    }
}

public class AnimalAccelerate : Action {
    private Animal animal;
    private Vector2 acceleration;
    public override Agent performer {
        get {
            return animal;
        }
    }
    public AnimalAccelerate(Animal animal, Vector2 acceleration):base(new Vector2I(0,0))/*SHUT UP, COMPILER!*/ {
        this.animal = animal;
        this.acceleration = acceleration;
    }
    public override void apply() {
        animal.ApplyAcceleration(acceleration);
    }
}

public class Animal : Agent {

	public static readonly Energy INITIAL_ENERGY = new Energy(20);

    public AnimalBoidParameters BoidParams {
        get {
            return worldInfo.BoidParams;
        }
    }

    public float InstantSpeed = 0.001f;
    public Vector2 InstantVelocity {
        get {
            return InstantSpeed * orientation.ToVector2();
        }
        set {
            InstantSpeed = Mathf.Min (value.magnitude, BoidParams.MaximumSpeed);
            Vector2 dir = value.normalized;
            orientation = Orientation.FromRadians(new Radians(-Mathf.Atan2(dir.y, dir.x))+(Radians)new Degrees(90));
        }
    }

    private FoodQuantity food;
    public  FoodQuantity Food {
        get {
            return food;
        }
        set {
            food = value;
        }
    }
    public bool HasFood {
        get { 
            return food > FoodQuantity.Zero;
        }
    }

    public override void RemoveEnergy(Energy e) {
        energy.Subtract(e);
        if (!Alive) {
            Debug.Log("[RIP] Animal @(" + pos.x + "," + pos.y + ")");
            worldInfo.NotifyAgentDiedListeners(new Vector2I(pos));
        }
    }

    public FoodQuantity Tear() {
        if (Alive) {
            return FoodQuantity.Zero;
        } else {
            //FIXME: These are testing values!
            food.Count -= 50;
            FoodQuantity removed = food;
            return removed;
        }
    }

    public Animal(WorldInfo world, Vector2 pos, FoodQuantity food)
    : base(world, pos, INITIAL_ENERGY) {
        this.food = food;
        this.AgentImpl = new AnimalBoidImplementation(this);
    }


	private IEnumerable<Animal> Myself {
		get {
			yield return this;
		}
	}

	public IEnumerable<Animal> Neighbors {
		get {
			var all = worldInfo.AllAnimals;
			var others = all.Except(Myself);
            var closeEnough = others.Where(a=>(a.pos-this.pos).sqrMagnitude < BoidParams.MaxVisDist*BoidParams.MaxVisDist);
			var inFront = closeEnough.Where(a=>{ 
				Degrees forwardToNeighbor = new Degrees(Vector2.Angle(this.orientation.ToVector2(), (a.pos-this.pos).normalized));
                return forwardToNeighbor.value < BoidParams.HalfFieldOfViewAngle.value;
			});
			return inFront;
		}
	}

    public void ApplyAcceleration(Vector2 a) {
        Vector2 curDir = orientation.ToVector2();
        float desiredSpeedDelta = a.ProjectIntoFactor(curDir);
        float clampedSpeedDelta = Mathf.Clamp(desiredSpeedDelta, -BoidParams.MaximumSpeedDelta, BoidParams.MaximumSpeedDelta);
        
        Vector2 orthoDir = Orientation.FromDegrees((Degrees)orientation.ToRadiansToUp()+new Degrees(90)).ToVector2();
        float desiredDirDelta = a.ProjectIntoFactor(orthoDir);
        float clampedDirDelta = Mathf.Clamp(desiredDirDelta, -BoidParams.MaximumDirDelta, BoidParams.MaximumDirDelta);
        
        Vector2 clampedDelta = curDir * clampedSpeedDelta + orthoDir * clampedDirDelta;
        Debug.DrawRay(new Vector3(pos.x, 2, pos.y), new Vector3(clampedDelta.x, 0, clampedDelta.y), Color.cyan);
        InstantVelocity = InstantVelocity + clampedDelta;
    }

	private void MoveLikeAVehicle() {
		Vector2 desiredPos = pos + InstantVelocity;
		Vector2 newPos = CoordConvertions.ClampAgentPosToWorldSize(desiredPos,worldInfo);
		ChangePosition(newPos);
	}

    public override bool EnemyInFront() {
        return false;
    }

    public override void OnWorldTick () {
        base.OnWorldTick ();
        MoveLikeAVehicle();
    }
}