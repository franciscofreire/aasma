using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ValidationVisitor {
    private Attitude attitude;
    private Beliefs beliefs;

    public ValidationVisitor(Attitude attitude, Beliefs beliefs) {
        this.attitude = attitude;
        this.beliefs = beliefs;
    }

    public bool isWalkValid(Walk a) {
        return beliefs.KnownObstacles.ObstacleMap[a.target] != KnownObstacles.ObstacleMapEntry.Obstacle;
    }

    public bool isAttackValid(Attack a) {
        //valid if attack points to a position that I believe there is an animal or enemy habitant
        var attackingAnimal = beliefs.AnimalsAreNear.RelevantCells.Contains(a.target);
        var attackingEnemy = beliefs.EnemiesAreNear.RelevantCells.Contains(a.target);
        return attackingAnimal || attackingEnemy;
    }
    
    /**********/
    /* ANIMAL */
    /**********/

    public bool isAnimalAccelerateValid(AnimalAccelerate a) {
        return true;
    }

    public bool isAnimalAttackHabitantValid(AnimalAttackHabitant a) {
        return true;
    }
    
    /************/
    /* HABITANT */
    /************/

    public bool isCutTreeValid(CutTree a) {
        // Target has a tree that is alive.
        var knownWoodAtTarget = beliefs.KnownWood.Map[a.target];
        return knownWoodAtTarget.HasValue && knownWoodAtTarget.Value.Alive;
    }

    public bool isChopTreeValid(ChopTree a) {
        // Target has a tree that is not alive and that has wood.
        var knownWoodAtTarget = beliefs.KnownWood.Map[a.target];
        return knownWoodAtTarget.HasValue && 
            !knownWoodAtTarget.Value.Alive && knownWoodAtTarget.Value.HasWood();
    }

    public bool isDropTreeValid(DropTree a) {
        // Target position is a known meeting point?
        return beliefs.NearMeetingPoint.Map[a.target];
    }

    public bool isPlaceFlagValid(PlaceFlag a) {
        // Target is a unclaimed cell.
        return beliefs.TribeTerritories.Territories[a.target] == null &&
            beliefs.TribeHasFewFlags.flagsCountInLastPercept > 0;
    }

    public bool isRemoveFlagValid (RemoveFlag a) {
        // Target has enemy flag.
        var territoryOwner = beliefs.TribeTerritories.Territories[a.target];
        return territoryOwner != null && territoryOwner != a.Habitant.tribe;
    }

    public bool isPickupFoodValid(PickupFood a) {
        // Target contains food.
        return beliefs.PickableFood.Map[a.target];
    }

    public bool isDropFoodValid(DropFood a) {
        // Target is in meeting point.
        return beliefs.NearMeetingPoint.Map[a.target];
    }

    public bool isEatCarriedFoodValid(EatCarriedFood a) {
        // Self is carrying food.
        return beliefs.SelfState.CarriedFood != FoodQuantity.Zero;
    }

    public bool isEatInTribeValid(EatInTribe a) {
        // Self is in tribe territory and tribe has food.
        var foodInTribe = beliefs.SelfState.Tribe.FoodStock != FoodQuantity.Zero;
        var inTribeTerritory = beliefs.TribeTerritories.Territories[beliefs.SelfState.Position] == beliefs.SelfState.Tribe;
        return foodInTribe && inTribeTerritory;
    }

    public bool isTurnLeftValid(TurnLeft a) {
        return true;
    }

    public bool isTurnOppositeDirectionValid(TurnOppositeDirection a) {
        return true;
    }
}