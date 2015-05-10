using System.Collections.Generic;

public abstract class Belief {
    /*  Belief Revision Function
     *  brf: Beliefs x Per -> Beliefs
     */
    private bool isActive;
    private IList<Vector2I> relevantCells;

    public IList<Vector2I> RelevantCells {
        get { return this.relevantCells; }
        set { this.relevantCells = value; }
    }

    public IList<Vector2I> IsActive {
        get { return this.IsActive; }
    }

    public void EnableBelief() {
        this.isActive = true;
    }

    public void DisableBelief() {
        this.isActive = false;
    }

    public abstract void UpdateBelief(Agent agent, SensorData sensorData);

    public static void brf(Beliefs beliefs, Agent agent, SensorData sensorData) {
        for(int i = 0; i < beliefs.Count(); i++) {
            Belief b = beliefs.Get (i);
            // FIXME: only for testing
            if(i == 0) {
                b.UpdateBelief (agent, sensorData);
            }
        }
    }

    public Belief(IList<Vector2I> relevantCells) {
        this.relevantCells = relevantCells;
    }

    public Belief() {
        this.relevantCells = new List<Vector2I>();
        this.DisableBelief();
    }
}

public class Beliefs {
    private IList<Belief> beliefsLst;

    // Returns null when index is out of bound
    public Belief Get(int index) {
        if(index >= 0 && index <= (Count()-1)) {
            return beliefsLst[index];
        }
        else {
            return null;
        }
    } 
    public void AddBelief(Belief belief) {
        beliefsLst.Add(belief);
    }
    public bool removeBelief(Belief belief) {
        int count = beliefsLst.Count;
        beliefsLst.Remove(belief);
        return Count() < count;
    }

    public int Count() {
        return beliefsLst.Count;
    }
    public Beliefs() {
        beliefsLst = new List<Belief>();
        // Add all the beliefs (disabled)
        AddBelief (new NearMeetingPoint());
        AddBelief (new TribeIsBeingAttacked());
        AddBelief (new TribeHasLowFoodLevel());
        AddBelief (new TribeHasLittleFlags());
        AddBelief (new AnimalsAreNear());
        AddBelief (new NearEnemyTribe());
        AddBelief (new ForestNear());
        AddBelief (new DroppedFood());
        AddBelief (new DroppedWood());
        AddBelief (new HabitantHasLowEnergy());
        AddBelief (new UnclaimedTerritoryIsNear());
    }
}

// Conditions: 
//  - MeetingPoint is on Agents' vision
public class NearMeetingPoint : Belief {
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        if(sensorData.MeetingPointCells.Count != 0) {
            RelevantCells = sensorData.MeetingPointCells;
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class TribeIsBeingAttacked : Belief {
    public Agent lastHabitantAttacked;

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
        throw new System.NotImplementedException ();
    }
}

public class NearEnemyTribe : Belief {
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        throw new System.NotImplementedException ();
    }
}

public class ForestNear : Belief {
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        throw new System.NotImplementedException ();
    }
}

public class DroppedFood : Belief {
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        throw new System.NotImplementedException ();
    }
}

public class DroppedWood : Belief {
    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        throw new System.NotImplementedException ();
    }
}

public class HabitantHasLowEnergy : Belief {
    public Energy habitantEnergy;

    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        throw new System.NotImplementedException ();
    }
}

public class UnclaimedTerritoryIsNear : Belief {
    public IList<Vector2I> unclaimedCells;

    public override void UpdateBelief (Agent agent, SensorData sensorData) {
        throw new System.NotImplementedException ();
    }
}