using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class AnimalBoidParameters {
    public float MaximumSpeed = 0.75f;
    public float MaximumSpeedDelta = 0.05f;
    public float MaximumDirDelta = 0.25f;
    
    public float SeparatioWeight = 5.0f;
    public float AlignmentWeight = 1.0f;
    public float CohesionWeight = 1.0f;
    public float AvoidBorderWeight = 10f;
    
    public float MaxVisDist = 5.0f;
    public Degrees HalfFieldOfViewAngle = new Degrees(330f/2f);
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
            
            /*Vector2 forward = orientation.ToVector2();
            float desiredSpeed = value.ProjectIntoFactor(forward);
            float speedDelta = desiredSpeed - InstantSpeed;
            Vector2 right = new Orientation(orientation.ToRadiansToUp()+new Degrees(90)).ToVector2();
            float  = value.ProjectIntoFactor(right);*/
            
            
            /*var speedDelta = Mathf.Min (value.magnitude, MaximumSpeedDelta);
            InstantSpeed = Mathf.Min(InstantSpeed+speedDelta, MaximumSpeed);
            var angleToX = new Radians(Mathf.Atan2(value.y, value.x));//Sentido directo
            var angleToY = angleToX - (Radians)new Degrees(90);//Sentido directo
            var desiredOrienAngle = -angleToY;//Converter para sentido indirecto
            var desiredOrienDelta = desiredOrienAngle - orientation.ToRadiansToUp();
            var orienDelta = new Degrees(Mathf.Clamp(Angles.ToZero360(desiredOrienDelta)-180, 
                                                     -MaximumOrientationDelta, 
                                                     MaximumOrientationDelta)
                                         +180);


            orientation = Orientation.FromRadians(orientation.ToRadiansToUp()+orienDelta);*/
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
            var closeEnough = others.Where(a=>(a.pos-this.pos).sqrMagnitude < BoidParams.MaxVisDist*BoidParams.MaxVisDist);
			var inFront = closeEnough.Where(a=>{ 
				Degrees forwardToNeighbor = new Degrees(Vector2.Angle(this.orientation.ToVector2(), (a.pos-this.pos).normalized));
                return forwardToNeighbor.value < BoidParams.HalfFieldOfViewAngle.value;
			});
			return inFront;
		}
	}

	private Vector2? SeparationAcceleration() {
		if (!Neighbors.Any())
			return null;
		Vector2 neighborDistSum = Neighbors.Select(a=>a.pos-this.pos).Aggregate(Vector2.zero,(v1,v2)=>v1+v2);
		Vector2 neighborDistMean = neighborDistSum / (float)Neighbors.Count();
        Vector2 acc = (-neighborDistMean / neighborDistMean.sqrMagnitude) * BoidParams.SeparatioWeight;
		Debug.DrawRay(new Vector3(pos.x, 2, pos.y), new Vector3(acc.x, 0, acc.y), Color.red);
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
		Vector2 toDesiredPosition = neighborMeanPos - this.pos;
        Vector2 acc = toDesiredPosition * BoidParams.CohesionWeight;
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
        Vector2 acc = forceFieldSum * BoidParams.AvoidBorderWeight;
        Debug.DrawRay(new Vector3(pos.x, 2, pos.y), new Vector3(acc.x, 0, acc.y), Color.magenta);
        return acc;
    }

    private void ApplyAcceleration(Vector2 a) {
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