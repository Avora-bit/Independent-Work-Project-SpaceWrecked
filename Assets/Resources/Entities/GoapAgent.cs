using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPAction
{
    public string actionName;
    public int cost;
    public int priority;
    public HashSet<string> preconditions;
    public HashSet<string> effects;

    public GOAPAction(string name, int cost, HashSet<string> pre, HashSet<string> eff)
    {
        actionName = name;
        this.cost = cost;
        preconditions = pre;
        effects = eff;
    }
}

public class GoapAgent : MonoBehaviour
{
    private HashSet<string> currentState;
    private HashSet<GOAPAction> availableActions;
    private HashSet<GOAPAction> currentAction;

    private Dictionary<string, GOAPAction> taskMemory;
    private Dictionary<string, int> selfNeeds;

    private BehaviorNode behaviorTree;

    private void Start()
    {
        currentState = new HashSet<string>();
        availableActions = new HashSet<GOAPAction>();
        currentAction = new HashSet<GOAPAction>();

        // Define actions and their preconditions/effects
        GOAPAction action1 = new GOAPAction("Action1", 2, new HashSet<string> { "Pre1" }, new HashSet<string> { "Eff1" });
        GOAPAction action2 = new GOAPAction("Action2", 3, new HashSet<string> { "Pre2" }, new HashSet<string> { "Eff2" });
        availableActions.Add(action1);
        availableActions.Add(action2);


        // Initialize self needs
        selfNeeds.Add("Hunger", 50);
        selfNeeds.Add("Thirst", 50);
        selfNeeds.Add("Rest", 50);

        // Define behavior tree
        behaviorTree = new PrioritySelectorNode(new List<BehaviorNode> {
            new SequenceNode(new List<BehaviorNode> {
                new NeedsCheckNode("Hunger", 60),
                new TaskExecutionNode(action1)
            }),
            new SequenceNode(new List<BehaviorNode> {
                new NeedsCheckNode("Thirst", 60),
                new TaskExecutionNode(action2)
            }),
            new SequenceNode(new List<BehaviorNode> {
                new NeedsCheckNode("Rest", 60),
                new RestNode()
            }),
            new TaskGenerationNode()
        });
    }

    private void Update()
    {
        // Update the current state of the world

        // Check if the current plan is complete
        if (currentAction.Count == 0)
        {
            // Generate a new plan
            currentAction = GeneratePlan();
        }

        // Execute behavior tree
        behaviorTree.Execute();

        // Choose the highest priority task to perform
        GOAPAction nextAction = GetHighestPriorityAction();
        if (nextAction != null)
        {
            // Check if the agent's self needs are met
            if (CheckSelfNeeds(nextAction))
            {
                // Execute the task
                if (PerformAction(nextAction))
                {
                    // Task succeeded, remove it from the current tasks
                    currentAction.Remove(nextAction);
                }
            }
            else
            {
                // Self needs not met, move to a safe location and rest
                MoveToSafeLocation();
                Rest();
            }
        }
    }

    private HashSet<GOAPAction> GeneratePlan()
    {
        HashSet<GOAPAction> plan = new HashSet<GOAPAction>();

        // TODO: Implement GOAP planning algorithm

        return plan;
    }

    private GOAPAction GetHighestPriorityAction()
    {
        GOAPAction highestPriorityAction = null;
        foreach (GOAPAction action in currentAction)
        {
            if (highestPriorityAction == null || action.priority > highestPriorityAction.priority)
            {
                highestPriorityAction = action;
            }
        }
        return highestPriorityAction;
    }

    private bool CheckSelfNeeds(GOAPAction task)
    {
        // Check if the agent's self needs are met
        foreach (string need in selfNeeds.Keys)
        {
            if (selfNeeds[need] < task.priority)
            {
                return false;
            }
        }
        return true;
    }

    private bool PerformAction(GOAPAction action)
    {
        // Check if the task's preconditions are met
        if (currentState.IsSupersetOf(action.preconditions))
        {
            // Apply the task's effects to the current state and the agent's self needs
            currentState.UnionWith(action.effects);
            foreach (string need in selfNeeds.Keys)
            {
                if (action.effects.Contains(need))
                {
                    selfNeeds[need] += action.priority;
                }
            }

            // Perform the task
            // TODO: Implement task execution code

            return true;
        }
        else
        {
            // Task preconditions not met, store the task in memory
            if (!taskMemory.ContainsKey(action.actionName))
            {
                taskMemory.Add(action.actionName, action);
            }

            // Move to the task's preconditions
            // TODO: Implement movement code

            return false;
        }
    }

    private void MoveToSafeLocation()
    {
        // Move to a safe location
        // TODO: Implement movement code
    }

    private void Rest()
    {
        // Rest to replenish self needs
        // TODO: Implement rest code
    }

    // Define behavior tree nodes
    public class NeedsCheckNode : BehaviorNode
    {
        private string need;
        private int threshold;

        public NeedsCheckNode(string need, int threshold)
        {
            this.need = need;
            this.threshold = threshold;
        }

        public override bool Execute()
        {
            //if (selfNeeds[need] <= threshold) return true;
            return false;
        }
    }

    public class TaskExecutionNode : BehaviorNode
    {
        private GOAPAction action;

        public TaskExecutionNode(GOAPAction action)
        {
            this.action = action;
        }

        public override bool Execute()
        {
            //if (currentState.IsSupersetOf(action.preconditions))
            //{
            //    // Apply the task's effects to the current state and the agent's self needs
            //    currentState.UnionWith(action.effects);
            //    foreach (string need in selfNeeds.Keys)
            //    {
            //        if (action.effects.Contains(need)) selfNeeds[need] += action.priority;
            //    }

            //    // Store the task in memory
            //    if (!taskMemory.ContainsKey(action.taskName))taskMemory.Add(task.taskName, action);

            //    // Perform the task
            //    // TODO: Implement task execution code

            //    return true;
            //}
            //else
            //{
            //    // Task preconditions not met, store the task in memory
            //    if (!taskMemory.ContainsKey(task.taskName)) taskMemory.Add(task.taskName, task);

            //    // Move to the task's preconditions
            //    // TODO: Implement movement code

            //    return false;
            //}
            return false;
        }
    }

    public class RestNode : BehaviorNode
    {
        public override bool Execute()
        {
            // Rest to replenish self needs
            // TODO: Implement rest code

            return true;
        }
    }

    public class TaskGenerationNode : BehaviorNode
    {
        public override bool Execute()
        {
            // Generate a new set of tasks
            //currentTasks = GenerateTasks();
            return true;
        }
    }
}

public class BehaviorNode
{
    public virtual bool Execute()
    {
        return true;
    }
}

public class SelectorNode : BehaviorNode
{
    private List<BehaviorNode> childNodes;

    public SelectorNode(List<BehaviorNode> children)
    {
        childNodes = children;
    }

    public override bool Execute()
    {
        foreach (BehaviorNode node in childNodes)
        {
            if (node.Execute())
            {
                return true;
            }
        }
        return false;
    }
}

public class SequenceNode : BehaviorNode
{
    private List<BehaviorNode> childNodes;

    public SequenceNode(List<BehaviorNode> children)
    {
        childNodes = children;
    }

    public override bool Execute()
    {
        foreach (BehaviorNode node in childNodes)
        {
            if (!node.Execute())
            {
                return false;
            }
        }
        return true;
    }
}

public class PrioritySelectorNode : BehaviorNode
{
    private List<BehaviorNode> childNodes;

    public PrioritySelectorNode(List<BehaviorNode> children)
    {
        childNodes = children;
    }

    public override bool Execute()
    {
        foreach (BehaviorNode node in childNodes)
        {
            if (node.Execute())
            {
                return true;
            }
        }
        return false;
    }
}
