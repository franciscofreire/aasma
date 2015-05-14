using System;
using System.Collections.Generic;
using HabitantMessages;
using HabitantMessages.Messages;
//
//################################################################################
//---Message Sending and Receiving---
//
//Habitant.SendMessageToAllies(HabitantMessage message)
//|
//+---Tribe.BroadcastMessageToHabitants(HabitantMessage message)
//    |
//    +---Habitant.ReceiveMessageFromAllie(HabitantMessage message)
//        |
//        +---HabitantMessage.AcceptVisitor(HabitantMessageVisitor visitor)
//            |
//            +---HabitantMessageVisitor.Visit*Type*Message(*Type*Message message)
//
//################################################################################
//
public partial class Tribe {
    public void BroadcastMessageToHabitants(Message message) {
        foreach(var h in this.habitants) {
            h.ReceiveMessageFromAlly(message);
        }
    }
}
public partial class Habitant {
    public Queue<Message> PendingMessages = new Queue<Message>();
    public void SendMessageToAllies(Message message) {
        this.tribe.BroadcastMessageToHabitants(message);
    }
    public void ReceiveMessageFromAlly(Message message) {
        if(message.Sender != this) {
            this.PendingMessages.Enqueue(message);
        }
    }
}
//
//################################################################################
//---Message Types---
//
//HabitantMessage
//  +--- *InfoSharing*
//  | +--- TreeAtPosition
//  | |
//  | +--- AnimalAtPosition
//  | +--- EnemyAtPosition
//  | +--- TerritoryAtPosition
//  | |
//  | +--- HabitantCurrentIntention
//  | |
//  | +--- AllyBeingAttackedAtPosition
//  |
//  +--- *Grouping*
//  | +--- MakeGroupFor*Something* (contains GroupId(habitant+num))
//  | +--- Accept -> To asker
//  | +--- Reject -> To asker
//  | +--- GroupFormed -> To allies
//  | |
//  | +--- *GroupGuiding* -> To group
//  |   +--- WhereToGo
//  |   +--- InformGroupOf*Something*
//
//#################################################################################
//
namespace HabitantMessages {
    public abstract class Message {
        public readonly Habitant Sender;
        public Message(Habitant sender) {
            this.Sender = sender;
        }
        public abstract void AcceptMessageVisitor(IMessageVisitor visitor);
    }
    public interface IMessageVisitor {
        //Methods for all the types of messages.
        void VisitHabitantBeingAttacked(HabitantBeingAttacked message);
    }
    namespace Messages {
        public class HabitantBeingAttacked : Message {
            public IList<Vector2I> EnemyPositions;
            public HabitantBeingAttacked(Habitant sender, IList<Vector2I> currentEnemyPositions) : base(sender) {
                this.EnemyPositions = currentEnemyPositions;
            }
            public void AcceptVisitor(IMessageVisitor visitor) {
                visitor.VisitHabitantBeingAttacked(this);
            }
        }
    }
}
