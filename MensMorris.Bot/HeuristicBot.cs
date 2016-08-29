﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MensMorris.Engine;

namespace MensMorris.Bot
{
    public class HeuristicBot : IPlayer
    {
        private static int[] PlaceCoefficients = new int[] { 18, 26, 1, 9, 10, 7, 8 };
        private static int[] MoveCoefficients = new int[] { 18, 26, 1, 9, 10, 7, 8 };
        private static int[] FlyCoefficients = new int[] { 18, 26, 1, 9, 10, 7, 8 };

        private static Func<Slot, int>[] SlotProperties = new Func<Slot, int>[]
        {
            slot => slot.GetMorrisCount(),
            slot => slot.GetBlockedPieceCount(),
            slot => slot.GetTilesOnBoard().Count,
            slot => slot.GetTwoPieceConfigurationCount(),
            slot => slot.GetThreePieceConfigurationCount(),
            slot => slot.GetDoubleMorrisCount()
        };

        public string GetName()
        {
            return "Heuristic Bot";
        }

        public PlaceAction ChoosePlaceAction(List<PlaceAction> possibleActions, Match match)
        {
            // Determine the best PlaceAction
            return this.ChooseAction(possibleActions.Cast<BaseAction>().ToList(), match) as PlaceAction;
        }

        public MoveAction ChooseMoveAction(List<MoveAction> possibleActions, Match match)
        {
            // Determine the best MoveAction
            return this.ChooseAction(possibleActions.Cast<BaseAction>().ToList(), match) as MoveAction;
        }

        public KickAction ChooseKickAction(List<KickAction> possibleActions, Match match)
        {
            // Determine the best KickAction
            return this.ChooseAction(possibleActions.Cast<BaseAction>().ToList(), match) as KickAction;
        }

        private BaseAction ChooseAction(List<BaseAction> possibleActions, Match match)
        {
            Dictionary<BaseAction, int> evaluation = new Dictionary<BaseAction, int>();
            foreach (BaseAction action in possibleActions)
            {
                // Simulate the action in this scope
                using (match.SimulateAction(action))
                {
                    evaluation.Add(action, this.EvaluateAction(action, match, HeuristicBot.PlaceCoefficients));
                }
            }
            // Return the action with the highest evaluated value
            return evaluation.OrderByDescending(eval => eval.Value).First().Key;
        }

        private int EvaluateAction(BaseAction action, Match match, int[] coefficients)
        {
            // Extract the own and the opponent match slot
            Slot me = action.Executing, opponent = me.GetOpponent();
            // Calculate the property differences between the slots and apply the coefficients
            return HeuristicBot.SlotProperties
                .Select((property, index) => coefficients[index] * (property(me) - property(opponent)))
                .Aggregate((x, y) => x + y);
        }

    }

    public static class HeuristicBotHelpers
    {
        public static int GetBlockedPieceCount(this Slot slot)
        {
            return slot.GetTilesOnBoard()
                .Count(tile => !tile.At.GetNeighbors()
                    .Any(neigh => neigh.IsFree));
        }

        public static int GetMorrisCount(this Slot slot)
        {
            return 1;
        }

        public static int GetDoubleMorrisCount(this Slot slot)
        {
            return 0;
        }

        public static int GetTwoPieceConfigurationCount(this Slot slot)
        {
            return 3;
        }

        public static int GetThreePieceConfigurationCount(this Slot slot)
        {
            return 2;
        }
    }
}
