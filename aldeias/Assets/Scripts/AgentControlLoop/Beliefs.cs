using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Belief {
    /*  Belief Revision Function
     *  brf: Beliefs x Per -> Beliefs
     */
    private bool isActive;
    private IList<Vector2I> relevantCells;
    private List<SensorData> previousSensorData;
    private const int MAX_SIZE_SENSOR_DATA = 10;

    public IList<Vector2I> RelevantCells {
        get { return this.relevantCells; }
        set { this.relevantCells = value; }
    }

    public bool IsActive {
        get { return this.isActive; }
    }

    public void AddSensorData(SensorData sensorData) {
        if(previousSensorData.Count == MAX_SIZE_SENSOR_DATA) {
            previousSensorData.RemoveAt(previousSensorData.Count - 1);
        }
        previousSensorData.Insert(0, sensorData);
    }

    /// <exception cref="SensorDataDoesNotExists">index given does not exists</exception>
    public SensorData GetSensorData(int index) {
        try {
            return previousSensorData[index];
        } catch (ArgumentOutOfRangeException) {
            throw new SensorDataDoesNotExists();
        }
    }


    public void EnableBelief() {
        this.isActive = true;
    }

    public void DisableBelief() {
        this.isActive = false;
        this.previousSensorData.Clear();
    }

    public abstract void UpdateBelief(Agent agent, SensorData sensorData);

    public static void brf(Beliefs beliefs, Agent agent, SensorData sensorData) {
        foreach(var b in beliefs.AllBeliefs) {
            b.UpdateBelief (agent, sensorData);
            // append sensorData to the beginning
            b.AddSensorData(sensorData);
        }
    }

    public Belief(IList<Vector2I> relevantCells) {
        this.relevantCells = relevantCells;
    }

    public Belief() {
        this.relevantCells = new List<Vector2I>();
        this.previousSensorData = new List<SensorData>(MAX_SIZE_SENSOR_DATA);
        this.DisableBelief();
    }
}

public class Beliefs {

    public NearMeetingPoint NearMeetingPoint=new NearMeetingPoint();
    public TribeIsBeingAttacked TribeIsBeingAttacked=new TribeIsBeingAttacked();
    public TribeHasLowFoodLevel TribeHasLowFoodLevel=new TribeHasLowFoodLevel();
    public TribeHasLittleFlags TribeHasLittleFlags=new TribeHasLittleFlags();
    public AnimalsAreNear AnimalsAreNear=new AnimalsAreNear();
    public NearEnemyTribe NearEnemyTribe=new NearEnemyTribe();
    public ForestNear ForestNear=new ForestNear();
    public DroppedFood DroppedFood=new DroppedFood();
    public DroppedWood DroppedWood=new DroppedWood();
    public HabitantHasLowEnergy HabitantHasLowEnergy=new HabitantHasLowEnergy();
    public UnclaimedTerritoryIsNear UnclaimedTerritoryIsNear=new UnclaimedTerritoryIsNear();

    public IEnumerable<Belief> AllBeliefs {
        get {
            yield return NearMeetingPoint;
            yield return TribeIsBeingAttacked;
            yield return TribeHasLowFoodLevel;
            yield return TribeHasLittleFlags;
            yield return AnimalsAreNear;
            yield return NearEnemyTribe;
            yield return ForestNear;
            yield return DroppedFood;
            yield return DroppedWood;
            yield return HabitantHasLowEnergy;
            yield return UnclaimedTerritoryIsNear;
        }
    }
}

/* Conditions: 
 *  - MeetingPoint is on Agents' vision
 */  
public class NearMeetingPoint : Belief {
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(sensorData.MeetingPointCells.Count != 0) {
            RelevantCells = sensorData.MeetingPointCells;
            EnableBelief();
        } else {
            if(IsActive) {
                try {
                    SensorData prevSensorData = GetSensorData(2);
                    if(prevSensorData.MeetingPointCells.Count != 0) {
                        DisableBelief();
                    }
                } catch (SensorDataDoesNotExists) {
                    ; // do nothing
                }
            }
        }
    }
}

public class TribeIsBeingAttacked : Belief {
    /* Conditions:
     *  - Habitant is under attack
     *  - Tribe territory is decreasing
     *  - 
     */ 
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        throw new System.NotImplementedException ();
    }
}

public class TribeHasLowFoodLevel : Belief {
    public FoodQuantity foodQuantity;

    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(sensorData.FoodTribe < new FoodQuantity(Tribe.CRITICAL_FOOD_LEVEL)) {
            this.foodQuantity = sensorData.FoodTribe;
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class TribeHasLittleFlags : Belief {
    public int flagsCount;

    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(sensorData.TribeFlags < Tribe.CRITICAL_FLAG_QUANTITY) {
            this.flagsCount = sensorData.TribeFlags;
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class AnimalsAreNear : Belief {
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(sensorData.Animals.Count > 0) {
            RelevantCells = new List<Vector2I>();
            foreach(Animal a in sensorData.Animals) {
                RelevantCells.Add(CoordConvertions.AgentPosToTile(a.pos));
            }
            EnableBelief();
        } else {

            DisableBelief();
        }
    }
}

public class NearEnemyTribe : Belief {
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(sensorData.EnemyTribeCells.Count > 0) {
            RelevantCells = sensorData.EnemyTribeCells;
            EnableBelief();
        } else {
            DisableBelief();
        }
    }   
}

public class ForestNear : Belief {
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(sensorData.Trees.Count > 0) {
            RelevantCells = new List<Vector2I>();
            foreach(Tree t in sensorData.Trees) {
                RelevantCells.Add(t.Pos);
            }
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class DroppedFood : Belief {
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(sensorData.Food.Count > 0) {
            RelevantCells = new List<Vector2I>();
            foreach(Animal a in sensorData.Food) {
                RelevantCells.Add(CoordConvertions.AgentPosToTile(a.pos));
            }
            EnableBelief();
        } else {
            
            DisableBelief();
        }
    }
}

public class DroppedWood : Belief {
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(sensorData.Stumps.Count > 0) {
            RelevantCells = new List<Vector2I>();
            foreach(Tree t in sensorData.Stumps) {
                RelevantCells.Add(t.Pos);
            }
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class HabitantHasLowEnergy : Belief {
    public Energy habitantEnergy;

    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(agent.LowEnergy()) {
            habitantEnergy = agent.energy;
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class UnclaimedTerritoryIsNear : Belief {

    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(sensorData.UnclaimedCells.Count > 0) {
            RelevantCells = sensorData.UnclaimedCells;
            EnableBelief();
        } else {
            DisableBelief ();
        }
    }
}

public class SensorDataDoesNotExists : SystemException {

}