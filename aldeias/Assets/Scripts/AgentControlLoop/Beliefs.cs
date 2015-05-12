using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    // It only adds if it does not exists
    public void addRelevantCell(Vector2I cell) {
        bool isPresent = false;
        foreach(var pos in RelevantCells) {
            if(pos == cell) {
                isPresent = true;
                break;
            }
        }
        if(!isPresent) {
            RelevantCells.Add (cell);
        }
    }

    public bool IsActive {
        get { return this.isActive; }
        set { this.isActive = value; }
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
    // default disable
    public void DisableBelief() {
        this.isActive = false;
        this.previousSensorData.Clear();
        this.relevantCells.Clear();
        this.timesToBeActive = 0;
    }

    public void DisableBeliefKeepingRelevantCells() {
        this.isActive = false;
        this.previousSensorData.Clear();
        this.timesToBeActive = 0;
    }

    public virtual void UpdateBelief(Percept p) {
        //FIXME: I don't know the purpose of saving SensorDatas in the base class.
        // append sensorData to the beginning
        AddSensorData(p.SensorData);
    }

    public static void brf(Beliefs beliefs, Percept p) {
        foreach(var b in beliefs.AllBeliefs) {
            b.UpdateBelief (p);
        }
    }
}

public class Beliefs {

    public NearMeetingPoint NearMeetingPoint;
    public TribeIsBeingAttacked TribeIsBeingAttacked;
    public TribeHasLowFoodLevel TribeHasLowFoodLevel;
    public TribeHasFewFlags TribeHasFewFlags;
    public AnimalsAreNear AnimalsAreNear;
    public NearEnemyTribe NearEnemyTribe;
    public ForestNear ForestNear;
    public DroppedFood DroppedFood;
    public DroppedWood DroppedWood;
    public HabitantHasLowEnergy HabitantHasLowEnergy;
    public UnclaimedTerritoryIsNear UnclaimedTerritoryIsNear;
    public KnownObstacles KnownObstacles;

    public IEnumerable<Belief> AllBeliefs {
        get {
            yield return NearMeetingPoint;
            yield return TribeIsBeingAttacked;
            yield return TribeHasLowFoodLevel;
            yield return TribeHasFewFlags;
            yield return AnimalsAreNear;
            yield return NearEnemyTribe;
            yield return ForestNear;
            yield return DroppedFood;
            yield return DroppedWood;
            yield return HabitantHasLowEnergy;
            yield return UnclaimedTerritoryIsNear;
            yield return KnownObstacles;
        }
    }

    public Beliefs(Habitant h) {
        NearMeetingPoint=new NearMeetingPoint();
        TribeIsBeingAttacked=new TribeIsBeingAttacked();
        TribeHasLowFoodLevel=new TribeHasLowFoodLevel();
        TribeHasFewFlags=new TribeHasFewFlags();
        AnimalsAreNear=new AnimalsAreNear();
        NearEnemyTribe=new NearEnemyTribe();
        ForestNear=new ForestNear();
        DroppedFood=new DroppedFood();
        DroppedWood=new DroppedWood();
        HabitantHasLowEnergy=new HabitantHasLowEnergy();
        UnclaimedTerritoryIsNear=new UnclaimedTerritoryIsNear();
        KnownObstacles=new KnownObstacles(h);
    }
}

/* Conditions: 
 *  - MeetingPoint is on Agents' vision
 */  
public class NearMeetingPoint : Belief {
    // Belief remains active if the last sensor
    // satisfies the condition (Agent saw meeting point cells)
    // Found meeting point cells are saved in Relevant Cells
    // even when belief is inactive
   
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        foreach(var cell in p.SensorData.MeetingPointCells) {
            addRelevantCell(cell);
        }

        if(p.SensorData.MeetingPointCells.Count != 0 ) {
            EnableBelief();
        } else if(IsActive) {
            try {
                SensorData prevSensorData = GetSensorData(1);
                if(prevSensorData.MeetingPointCells.Count == 0) {
                   DisableBeliefKeepingRelevantCells();
                }
            } catch (SensorDataDoesNotExists) {
                ; // do nothing
            }
        }     
    }
}

public class TribeIsBeingAttacked : Belief {
    /* Conditions:
     *  - Habitant is near enemy and it is insideTribe
     *  - Tribe territory is decreasing
     */
    private bool ArePreconditionsSatisfied(SensorData sensorData) {
        try {
            return ((PreviousSensorDataCount > 0 &&
                    GetSensorData(1).TribeCellCount > sensorData.TribeCellCount) ||
                    sensorData.Enemies.Count > 0);
        } catch(SensorDataDoesNotExists) {
            return false;
        }
    }
   
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(!IsActive && ArePreconditionsSatisfied(p.SensorData)) {
            foreach (Habitant h in p.SensorData.Enemies) {
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
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(p.SensorData.FoodTribe < new FoodQuantity(Tribe.CRITICAL_FOOD_LEVEL)) {
            this.foodQuantity = p.SensorData.FoodTribe;
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

    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(p.SensorData.TribeFlags < Tribe.CRITICAL_FLAG_QUANTITY) {
            this.flagsCount = p.SensorData.TribeFlags;
            EnableBelief(2);
        } else if(IsActive && timesToBeActive-- == 0){
            DisableBelief();
        }
    }
}

public class AnimalsAreNear : Belief {
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(p.SensorData.Animals.Count > 0) {
            RelevantCells = new List<Vector2I>();
            foreach(Animal a in p.SensorData.Animals) {
                RelevantCells.Add(CoordConvertions.AgentPosToTile(a.pos));
            }
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class NearEnemyTribe : Belief {
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(p.SensorData.EnemyTribeCells.Count > 0) {
            RelevantCells = p.SensorData.EnemyTribeCells;
            EnableBelief();
        } else {
            DisableBelief();
        }
    }   
}

public class ForestNear : Belief {
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(p.SensorData.Trees.Count > 0) {
            RelevantCells = new List<Vector2I>();
            foreach(Tree t in p.SensorData.Trees) {
                RelevantCells.Add(t.Pos);
            }
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class DroppedFood : Belief {
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(p.SensorData.Food.Count > 0) {
            RelevantCells = new List<Vector2I>();
            foreach(Animal a in p.SensorData.Food) {
                RelevantCells.Add(CoordConvertions.AgentPosToTile(a.pos));
            }
            EnableBelief();
        } else {
            
            DisableBelief();
        }
    }
}

public class DroppedWood : Belief {
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(p.SensorData.Stumps.Count > 0) {
            RelevantCells = new List<Vector2I>();
            foreach(Tree t in p.SensorData.Stumps) {
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

    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(p.Habitant.LowEnergy()) {
            habitantEnergy = p.Habitant.energy;
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class UnclaimedTerritoryIsNear : Belief {

    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(p.SensorData.UnclaimedCells.Count > 0) {
            RelevantCells = p.SensorData.UnclaimedCells;
            EnableBelief(2);
        } else if(IsActive && timesToBeActive-- == 0){
            DisableBelief();
        }
    }
}

public class KnownObstacles : Belief {
    public ObstacleMapEntry[,] ObstacleMap;

    public enum ObstacleMapEntry { Obstacle, Free, Unknown };
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        //Update obstacle positions.
        foreach(var obsCoord in SensorDataObstacles(p.SensorData)) {
            ObstacleMap[obsCoord.x, obsCoord.y] = ObstacleMapEntry.Obstacle;
        }
        //Update free positions.
        foreach(var freeCoord in p.SensorData.Cells.Except(SensorDataObstacles(p.SensorData))) {
            ObstacleMap[freeCoord.x, freeCoord.y] = ObstacleMapEntry.Free;
        }
    }
    public KnownObstacles(Habitant h) {
        EnableBelief();
        CreateObstacleMapForHabitant(h);
    }
    private void CreateObstacleMapForHabitant(Habitant h) {
        var mapSize = h.worldInfo.Size;
        ObstacleMap = new ObstacleMapEntry[mapSize.x,mapSize.y];
        foreach(var x in Enumerable.Range(0,mapSize.x)) {
            foreach(var y in Enumerable.Range(0,mapSize.y)) {
                ObstacleMap[x,y] = ObstacleMapEntry.Obstacle;
            }
        }
    }
    private IEnumerable<Vector2I> SensorDataObstacles(SensorData sensorData) {
        return sensorData.Trees.Concat(sensorData.Stumps).Select(t=>t.Pos);
    }
}

public class TribeTerritories : Belief {
    public Tribe[,] Territories;

    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        foreach(var coordTribe in p.SensorData.Territories) {
            Vector2I c = coordTribe.Key;
            Territories[c.x,c.y] = coordTribe.Value;
        }
        foreach(var c in p.SensorData.UnclaimedCells) {
            Territories[c.x,c.y] = null;
        }
    }

    public TribeTerritories(Habitant h) {
        EnableBelief();
        var size = h.worldInfo.Size;
        Territories = new Tribe[size.x,size.y];
    }
}

public class SensorDataDoesNotExists : SystemException {

}