using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Animal : Agent {

	public static readonly Energy INITIAL_ENERGY = new Energy(20);

	public static float MaximumSpeed = 0.25f;
	public static float MaximumSpeedDelta = 0.1f;
	public static Degrees MaximumOrientationDelta = new Degrees(15f);

    public static float SeparatioWeight = 1.0f;
    public static float AlignmentWeight = 1.0f;
    public static float CohesionWeight = 1.0f;
    public static float AvoidBorderWeight = 10f;

	public static float MaxVisDist = 5.0f;
	public static Degrees HalfFieldOfViewAngle = new Degrees(330f/2f);


	public float InstantSpeed = 0.001f;
	public Vector2 InstantVelocity {
		get {
			return InstantSpeed * orientation.ToVector2();
		}
		set {
			var speedDelta = Mathf.Min (value.magnitude, MaximumSpeedDelta);
			InstantSpeed = Mathf.Min(InstantSpeed+speedDelta, MaximumSpeed);
			var desiredOrienAngle = new Degrees(Vector2.Angle(Vector2.up, value));//FIXME: This is only giving half a circle. We need the full circle.
			var desiredOrienDelta = new Degrees(desiredOrienAngle - (Degrees)orientation.ToRadiansToUp());
			var orienDelta = new Degrees(Mathf.Min (MaximumOrientationDelta, Mathf.Max (-MaximumOrientationDelta, desiredOrienDelta)));

			orientation = Orientation.FromDegrees((Degrees)orientation.ToRadiansToUp()+orienDelta);
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
    }


    /********************
     ** BOID BEHAVIOUR **
     ********************/

	private void BehaveLikeABoid() {
		SteerLikeABoid();
		MoveLikeAVehicle();
	}

	private void SteerLikeABoid() {

		Vector2?[] accs = new Vector2?[]{SeparationAcceleration(), AlignmentAcceleration(), CohesionAcceleration(), AvoidBorderAcceleration()};
		Vector2 a = accs.Select(acc=>acc??Vector2.zero).Aggregate((a1,a2)=>a1+a2);
		ApplyAcceleration(a);
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
			var closeEnough = others.Where(a=>(a.pos-this.pos).sqrMagnitude < MaxVisDist*MaxVisDist);
			var inFront = closeEnough.Where(a=>{ 
				Degrees forwardToNeighbor = new Degrees(Vector2.Angle(this.orientation.ToVector2(), (a.pos-this.pos).normalized));
				return forwardToNeighbor.value < HalfFieldOfViewAngle.value;
			});
			return inFront;
		}
	}

	private Vector2? SeparationAcceleration() {
		if (!Neighbors.Any())
			return null;
		Vector2 neighborDistSum = Neighbors.Select(a=>a.pos-this.pos).Aggregate(Vector2.zero,(v1,v2)=>v1+v2);
		Vector2 neighborDistMean = neighborDistSum / (float)Neighbors.Count();
        Vector2 acc = (-neighborDistMean / neighborDistMean.sqrMagnitude) * SeparatioWeight;
		Debug.DrawRay(new Vector3(pos.x, 2, pos.y), new Vector3(acc.x, 0, acc.y), Color.red);
		return acc;
	}

	private Vector2? AlignmentAcceleration() {
		if (!Neighbors.Any())
			return null;
		Radians neighborOrientSum = Neighbors.Select(a=>a.orientation.ToRadiansToUp()).Aggregate(new Radians(0), (r1,r2)=>r1+r2);
		Orientation desiredOrientation = Orientation.FromRadians(new Radians(neighborOrientSum / (float)Neighbors.Count()));
		Vector2 acc = desiredOrientation.ToVector2() * AlignmentWeight;
		//Debug.DrawRay(new Vector3(pos.x, 2, pos.y), new Vector3(acc.x, 0, acc.y), Color.blue);
		return acc;
	}

	private Vector2? CohesionAcceleration() {
		if (!Neighbors.Any())
			return null;
		Vector2 neighborPosSum = Neighbors.Select(a=>a.pos).Aggregate(Vector2.zero, (p1,p2)=>p1+p2);
		Vector2 neighborMeanPos = neighborPosSum / (float) Neighbors.Count();
		Vector2 toDesiredPosition = neighborMeanPos - this.pos;
        Vector2 acc = toDesiredPosition * CohesionWeight;
		//Debug.DrawRay(new Vector3(pos.x, 2, pos.y), new Vector3(acc.x, 0, acc.y), Color.green);
		return acc;
	}

    private Vector2? AvoidBorderAcceleration() {
        //TODO: have a strong force field close to the borders of the world.
        //TODO: the force gets stronger the closer it gets to the border.
        //TODO: the force should point away from the borders.
        System.Func<Vector2,Vector2> leftForceField = (Vector2 apos)=>{
            Vector2 awayFromEdgeForce = Vector2.right;
            float maximumForceCoord = 0;//At edge.
            float minimumForceCoord = 10;//Where the force becomes zero.

            float closenessToEdge = Mathf.InverseLerp(minimumForceCoord,maximumForceCoord,apos.x);
            return awayFromEdgeForce * closenessToEdge;
        };
        System.Func<Vector2,Vector2> rightForceField = (Vector2 apos)=>{
            Vector2 awayFromEdgeForce = -Vector2.right;
            float maximumForceCoord = worldInfo.xSize;//At edge.
            float minimumForceCoord = worldInfo.xSize-10;//Where the force becomes zero.

            float closenessToEdge = Mathf.InverseLerp(minimumForceCoord,maximumForceCoord,apos.x);
            return awayFromEdgeForce * closenessToEdge;
        };
        System.Func<Vector2,Vector2> topForceField = (Vector2 apos)=>{
            Vector2 awayFromEdgeForce = -Vector2.up;
            float maximumForceCoord = worldInfo.zSize;//At edge.
            float minimumForceCoord = worldInfo.zSize-10;//Where the force becomes zero.
            
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
        Vector2 forceFieldSum = leftForceField(this.pos)+rightForceField(this.pos)+bottomForceField(this.pos)+topForceField(this.pos);
        Vector2 acc = forceFieldSum * AvoidBorderWeight;
        Debug.DrawRay(new Vector3(pos.x, 2, pos.y), new Vector3(acc.x, 0, acc.y), Color.magenta);
        return acc;
    }

	private void ApplyAcceleration(Vector2 a) {
		//TODO: Clamp the acceleration
		Vector2 currentVelocity = InstantVelocity;
		Vector2 desiredVelocity = currentVelocity + a;
		InstantVelocity = desiredVelocity;

		//float speedDelta = a.ProjectIntoFactor(orientation.ToVector2());
		//Degrees orientationDelta = //the signed angle between the current orienation and 
	}

	private void MoveLikeAVehicle() {
		Vector2 desiredPos = pos + InstantVelocity;
		Vector2 newPos = CoordConvertions.ClampAgentPosToWorldSize(desiredPos,worldInfo);
		ChangePosition(newPos);
	}


	public override void OnWorldTick () {
		BehaveLikeABoid();
		//Vector2 sum = pos+Vector2.right;
		//pos = new Vector2(sum.x%worldInfo.xSize, sum.y);
        /*updateSensorData();
        Action a = doAction();
        a.apply();*/
	}

    public override bool EnemyInFront() {
        return false;
    }
}