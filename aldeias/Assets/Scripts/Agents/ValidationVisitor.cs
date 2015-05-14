using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ValidationVisitor {
    private Attitude attitude;

    public ValidationVisitor(Attitude attitude) {
        this.attitude = attitude;
    }

    public bool isWalkValid(Walk a) {
        return !a.performer.worldInfo.worldTiles.WorldTileInfoAtCoord(a.target).HasAgent
            && (!a.performer.worldInfo.worldTiles.WorldTileInfoAtCoord(a.target).HasTree ||
                !a.performer.worldInfo.worldTiles.WorldTileInfoAtCoord(a.target).Tree.HasWood);
    }

    public bool isAttackValid(Attack a) {
        return true;
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
        return a.Habitant.AliveTree(a.target);
    }

    public bool isChopTreeValid(ChopTree a) {
        //return a.performer.StumpWithWood(a.target);
        Agent fulano = a.performer;
        WorldTileInfo tile = fulano.worldInfo.worldTiles.WorldTileInfoAtCoord(a.target);
        Tree t = tile.Tree;
        return t.HasWood;
    }

    public bool isDropTreeValid(DropTree a) {
        return true;
    }

    public bool isPlaceFlagValid(PlaceFlag a) {
        return !a.performer.worldInfo.worldTiles.WorldTileInfoAtCoord(a.target)
                 .tribeTerritory.IsClaimed; // TODO: Enemy flag case
    }

    public bool isRemoveFlagValid (RemoveFlag a) {
        WorldTileInfo wti = a.performer.worldInfo.worldTiles.WorldTileInfoAtCoord(a.target);
        return wti.tribeTerritory.IsClaimed && 
            !wti.tribeTerritory.Flag.Value.Tribe.Equals(a.Habitant.tribe);

    }

    public bool isPickupFoodValid(PickupFood a) {
        return true;
    }

    public bool isDropFoodValid(DropFood a) {
        return true;
    }

    public bool isEatCarriedFoodValid(EatCarriedFood a) {
        return true;
    }

    public bool isEatInTribeValid(EatInTribe a) {
        return true;
    }

    public bool isTurnLeftValid(TurnLeft a) {
        return true;
    }

    public bool isTurnOppositeDirectionValid(TurnOppositeDirection a) {
        return true;
    }
}