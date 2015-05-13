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
    public WorldInfo WorldInfo { get; set; }

    public NearMeetingPoint NearMeetingPoint;
    public TribeIsBeingAttacked TribeIsBeingAttacked;
    public TribeHasLowFoodLevel TribeHasLowFoodLevel;
    public TribeHasFewFlags TribeHasFewFlags;
    public AnimalsAreNear AnimalsAreNear;
    public EnemiesAreNear EnemiesAreNear;
    public NearEnemyTribe NearEnemyTribe;
    public KnownWood KnownWood;
    public PickableFood DroppedFood;
    public PickableWood DroppedWood;
    public HabitantHasLowEnergy HabitantHasLowEnergy;
    public UnclaimedTerritoryIsNear UnclaimedTerritoryIsNear;
    public KnownObstacles KnownObstacles;
    public TribeTerritories TribeTerritories;
    public CellSeenOrders CellSeenOrders;

    public IEnumerable<Belief> AllBeliefs {
        get {
            yield return NearMeetingPoint;
            yield return TribeIsBeingAttacked;
            yield return TribeHasLowFoodLevel;
            yield return TribeHasFewFlags;
            yield return EnemiesAreNear;
            yield return AnimalsAreNear;
            yield return NearEnemyTribe;
            yield return KnownWood;
            yield return DroppedFood;
            yield return DroppedWood;
            yield return HabitantHasLowEnergy;
            yield return UnclaimedTerritoryIsNear;
            yield return KnownObstacles;
            yield return TribeTerritories;
            yield return CellSeenOrders;
        }
    }

    public Beliefs(Habitant h) {
        WorldInfo = h.worldInfo;

        NearMeetingPoint=new NearMeetingPoint();
        TribeIsBeingAttacked=new TribeIsBeingAttacked();
        TribeHasLowFoodLevel=new TribeHasLowFoodLevel();
        TribeHasFewFlags=new TribeHasFewFlags();
        AnimalsAreNear=new AnimalsAreNear();
        EnemiesAreNear=new EnemiesAreNear();
        NearEnemyTribe=new NearEnemyTribe();
        KnownWood=new KnownWood(h);
        DroppedFood=new PickableFood();
        DroppedWood=new PickableWood();
        HabitantHasLowEnergy=new HabitantHasLowEnergy();
        UnclaimedTerritoryIsNear=new UnclaimedTerritoryIsNear();
        KnownObstacles=new KnownObstacles(h);
        TribeTerritories=new TribeTerritories(h);
        CellSeenOrders=new CellSeenOrders(h);
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

public class EnemiesAreNear : Belief {
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(p.SensorData.Enemies.Count > 0) {
            RelevantCells = new List<Vector2I>();
            foreach(Habitant h in p.SensorData.Enemies) {
                RelevantCells.Add(CoordConvertions.AgentPosToTile(h.pos));
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

public class KnownWood : Belief {
    public struct KnownWoodEntry {
        WoodQuantity Wood;
        bool Alive;
        public KnownWoodEntry(WoodQuantity wood, bool alive) {
            this.Wood = wood;
            this.Alive = alive;
        }
    }
    public Matrix<KnownWoodEntry?> Map;
    public IEnumerable<Vector2I> CoordsWithWood {
        get {
            return Map.AllCoords.Where(c=>Map[c]!=null);
        }
    }

    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);

        foreach(Tree t in p.SensorData.Trees.Concat(p.SensorData.Stumps)) {
            Map[t.Pos] = t.HasWood ? new KnownWoodEntry(t.Wood, t.Alive) : (KnownWoodEntry?)null;
        }
    }
    
    public KnownWood(Habitant h) {
        Map = new Matrix<KnownWoodEntry?>(h.worldInfo.Size);
    }
}

public class PickableFood : Belief {
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

public class PickableWood : Belief {
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
    public enum ObstacleMapEntry { Obstacle, Free, Unknown };
    public Matrix<ObstacleMapEntry> ObstacleMap;
    public bool CoordIsFree(Vector2I coord) {
        return ObstacleMap[coord]!=ObstacleMapEntry.Obstacle;
    }
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        //Update obstacle positions.
        foreach(var obsCoord in SensorDataObstacles(p.SensorData)) {
            ObstacleMap[obsCoord] = ObstacleMapEntry.Obstacle;
        }
        //Update free positions.
        foreach(var freeCoord in p.SensorData.Cells.Except(SensorDataObstacles(p.SensorData))) {
            ObstacleMap[freeCoord] = ObstacleMapEntry.Free;
        }
    }
    public KnownObstacles(Habitant h) {
        EnableBelief();
        CreateObstacleMapForHabitant(h);
    }
    private void CreateObstacleMapForHabitant(Habitant h) {
        var mapSize = h.worldInfo.Size;
        ObstacleMap = new Matrix<ObstacleMapEntry>(mapSize,ObstacleMapEntry.Unknown);
    }
    private IEnumerable<Vector2I> SensorDataObstacles(SensorData sensorData) {
        return sensorData.Trees.Concat(sensorData.Stumps)
            .Where(t=>t.HasWood)
                .Select(t=>t.Pos);
    }
}

public class TribeTerritories : Belief {
    public Matrix<Tribe> Territories;
    public IEnumerable<Vector2I> UnclaimedTerritories {
        get {
            return Territories.AllCoords.Where(c=>Territories[c]==null);
        }
    }


    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        foreach(var coordTribe in p.SensorData.Territories) {
            Vector2I c = coordTribe.Key;
            Territories[c] = coordTribe.Value;
        }
        foreach(var c in p.SensorData.UnclaimedCells) {
            Territories[c] = null;
        }
    }

    public TribeTerritories(Habitant h) {
        EnableBelief();
        var size = h.worldInfo.Size;
        Territories = new Matrix<Tribe>(size);

        h.sensorData.Territories = new List<KeyValuePair<Vector2I,Tribe>>();
        for (int i = h.tribe.start_x; i < h.tribe.cell_line; i++)
            for (int j = h.tribe.start_y; j < h.tribe.cell_line; j++)
                h.sensorData.Territories.Add(new KeyValuePair<Vector2I, Tribe>(new Vector2I(i,j),h.tribe));
    }
}

public class CellSeenOrders : Belief {
    public Matrix<int> LastSeenOrders;
    public int CurrentOrder {
        get; 
        private set;
    }
    private readonly int NotSeenBeforeOrder = int.MinValue;
    public CellSeenOrders(Habitant h) {
        CurrentOrder = 0;
        LastSeenOrders = new Matrix<int>(h.worldInfo.Size, NotSeenBeforeOrder);
    }
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        CurrentOrder += 1;
        //Mark each seen cell with CurrentOrder.
        foreach(var seenCoord in p.SensorData.Cells) {//FIXME: Make sure these are all the seen cells.
            LastSeenOrders[seenCoord] = CurrentOrder;
        }
    }
}

public class SensorDataDoesNotExists : SystemException {

}