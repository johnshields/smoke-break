namespace _Scripts.enums
{
    public enum ItemType
    {
        Ammo,
        Coin,
        Health,
        Other
    }
    
    public enum AIState
    {
        Patrolling,
        Chasing,
        Attacking
    }
    
    public enum ActionType
    {
        Quit,
        NewGame,
        None
    }

    public enum ConfirmAction
    {
        Open,
        Confirm,
        Cancel
    }

    public enum DialogueKey
    {
        Opening,
        Closing,
        MissionStart,
        Warning,
        Victory
    }
    
    public enum PodPartType
    {
        JackStand,
        WinchCables,
        Crank,
        Struts,
        Wings,
        Thrusters,
        EngineParts,
        LastComp
    }

}