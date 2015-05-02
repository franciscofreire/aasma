using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Animal : Agent {

	public static readonly Energy INITIAL_ENERGY = new Energy(20);

	public static float MaximumSpeed = 0.25f;
	public static float MaximumSpeedDelta = 0.1f;
	public static Degrees MaximumOrientationDelta = new Degrees(15f);

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
			var desiredOrienAngle = new Degrees(Vector2.Angle(Vector2.up, value));
			var desiredOrienDelta = new Degrees(desiredOrienAngle - (Degrees)orientation.ToRadiansToUp());
			var orienDelta = new Degrees(Mathf.Min (MaximumOrientationDelta, Mathf.Max (-MaximumOrientationDelta, desiredOrienDelta)));

			orientation = Orientation.FromDegrees((Degrees)orientation.ToRadiansToUp()+orienDelta);
		}
	}



	public Animal(WorldInfo world, Vector2 pos): base(world, pos, INITIAL_ENERGY) { }

	private void BehaveLikeABoid() {
		SteerLikeABoid();
		MoveLikeAVehicle();
	}

	private void SteerLikeABoid() {

		Vector2?[] accs = new Vector2?[]{SeparationAcceleration(), AlignmentAcceleration(), CohesionAcceleration()};
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
		Vector2 neighborDistSum = Neighbors.Select(a=>a.pos).Aggregate(Vector2.zero,(v1,v2)=>v1+v2);
		Vector2 neighborDistMean = -neighborDistSum / (float)Neighbors.Count();
		Vector2 acc = neighborDistMean.normalized * 10;
		Debug.DrawRay(new Vector3(pos.x, 2, pos.y), new Vector3(acc.x, 0, acc.y), Color.red);
		return acc;
	}

	private Vector2? AlignmentAcceleration() {
		if (!Neighbors.Any())
			return null;
		Radians neighborOrientSum = Neighbors.Select(a=>a.orientation.ToRadiansToUp()).Aggregate(new Radians(0), (r1,r2)=>r1+r2);
		Orientation desiredOrientation = Orientation.FromRadians(new Radians(neighborOrientSum / (float)Neighbors.Count()));
		Vector2 acc = desiredOrientation.ToVector2();
		Debug.DrawRay(new Vector3(pos.x, 2, pos.y), new Vector3(acc.x, 0, acc.y), Color.blue);
		return acc;
	}

	private Vector2? CohesionAcceleration() {
		if (!Neighbors.Any())
			return null;
		Vector2 neighborPosSum = Neighbors.Select(a=>a.pos).Aggregate(Vector2.zero, (p1,p2)=>p1+p2);
		Vector2 neighborMeanPos = neighborPosSum / (float) Neighbors.Count();
		Vector2 acc = neighborMeanPos - this.pos;
		Debug.DrawRay(new Vector3(pos.x, 2, pos.y), new Vector3(acc.x, 0, acc.y), Color.green);
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



	public override Action doAction() {

        int index = WorldRandom.Next(sensorData.Cells.Count);
        Vector2I target = sensorData.Cells[index];
        return new Walk(this, target);
	}

	public override void OnWorldTick () {
		BehaveLikeABoid();
		//Vector2 sum = pos+Vector2.right;
		//pos = new Vector2(sum.x%worldInfo.xSize, sum.y);
        /*updateSensorData();
        Action a = doAction();
        a.apply();*/
	}

	//*************
	//** SENSORS **
	//*************

	public override bool EnemyInFront() {
		return false;
	}
}