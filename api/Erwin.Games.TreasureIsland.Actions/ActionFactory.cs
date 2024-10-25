using System;
using System.Reflection;

namespace Erwin.Games.TreasureIsland.Actions
{
    public static class ActionFactory
    {
        public static IAction? CreateAction(string actionName, params object[] args)
        {
            // Concatenate the action name with "Action"
            string className = actionName + "Action";

            // Get the current assembly
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Find the type that matches the class name
            Type? actionType = assembly.GetType($"Erwin.Games.TreasureIsland.Actions.{className}");

            if (actionType == null)
            {
                return null;
            }

            // Create an instance of the type
            var actionInstance = Activator.CreateInstance(actionType, args) as IAction;

            return actionInstance;
        }
    }
}