using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Belief {

    private const int MAX_SIZE_SENSOR_DATA = 10;

    private bool isActive;
    private IList<Vector2I> relevantCells;
    private List<SensorData> previousSensorData;
    protected int timesToBeActive;

    public Belief() {
        this.relevantCells = new List<Vector2I>();
        this.previousSensorData = new List<SensorData>(MAX_SIZE_SENSOR_DATA);
        this.DisableBelief();
        this.timesToBeActive = 0;
    }

    public Belief(IList<Vector2I> relevantCells) {
        this.relevantCells = relevantCells;
    }

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

    public int PreviousSensorDataCount {
        get { return previousSensorData.Count; }
    }

    public void EnableBelief() {
        this.isActive = true;
    }
    public void EnableBelief(int count) {
        this.isActive = true;
        this.timesToBeActive = count;
    }

    public void DisableBelief() {
        this.isActive = false;
        this.previousSensorData.Clear();
        this.relevantCells.Clear();
        this.timesToBeActive = 0;
    }

    public abstract void UpdateBelief(Agent agent, SensorData sensorData);

    public static void brf(Beliefs beliefs, Agent agent, SensorData sensorData) {
        foreach(var b in beliefs.AllBeliefs) {
            b.UpdateBelief (agent, sensorData);
            // append sensorData to the beginning
            b.AddSensorData(sensorData);
        }
    }

}

public class Beliefs {

    public NearMeetingPoint NearMeetingPoint=new NearMeetingPoint();
    public TribeIsBeingAttacked TribeIsBeingAttacked=new TribeIsBeingAttacked();
    public TribeHasLowFoodLevel TribeHasLowFoodLevel=new TribeHasLowFoodLevel();
    public TribeHasFewFlags TribeHasLittleFlags=new TribeHasFewFlags();
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
    // Belief remains active if the last but one sensor
    // satisfies the condition (Agent saw meeting point cells
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
     *  - Habitant is near enemy and it is insideTribe
     *  - Tribe territory is decreasing
     *  - 
     */
    private bool ArePreconditionsSatisfied(SensorData sensorData) {
        return ((PreviousSensorDataCount > 0 &&
                GetSensorData(1).TribeCellCount > sensorData.TribeCellCount) ||
                sensorData.Enemies.Count > 0);
    }
   
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(!IsActive && ArePreconditionsSatisfied(sensorData)) {
            foreach (Habitant h in sensorData.Enemies) {
                RelevantCells.Add (CoordConvertions.AgentPosToTile(h.pos));
            }
            EnableBelief();
        } else if(IsActive) {
            // Here we only disable if statistics show us that things are getting better,
            // i.e. in previous sensorData, tribe cells number did not decrease
            // and we didn't see enemies
            int initialCellCount = 0;
            int finalCellCount = 0;
            int rangeMax = Mathf.CeilToInt(PreviousSensorDataCount/2f);
            int enemiesCount = 0;
            for(int i = 0; i < rangeMax; i++) {
                SensorData sd = GetSensorData(i);
                if(i == 0) {
                    initialCellCount = sd.TribeCellCount;
                } else if(i == rangeMax-1) {
                    finalCellCount = sd.TribeCellCount;
                }
                enemiesCount = (sd.Enemies.Count > 0) ? enemiesCount + 1 : enemiesCount;
            }
            if(enemiesCount == 0 && (finalCellCount >= initialCellCount)) {
                DisableBelief();
            }
        }
    }
}

public class TribeHasLowFoodLevel : Belief {
    public FoodQuantity foodQuantity;

    // Here we let Food Low level be active for 3 updates, even 
    // when perceptions conditions are not satisfied
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(sensorData.FoodTribe < new FoodQuantity(Tribe.CRITICAL_FOOD_LEVEL)) {
            this.foodQuantity = sensorData.FoodTribe;
            EnableBelief(2);
        } else if(timesToBeActive > 0) {
            timesToBeActive--;
        } else {
            DisableBelief();
        }
    }
}

public class TribeHasFewFlags : Belief {
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