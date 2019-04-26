public interface IPhase
{
    void Begin();
    bool IsFinished();
    void End();
    IPhase GetNextPhase();
}

IPhase currentPhase;

static IMyShipMergeBlock mergeUpper, mergeLower;
static IMyPistonBase pistonMergeUpper, pistonMergeLower, pistonLiftLower, pistonLiftUpper;
static IMyShipWelder welderA, welderB;
static float extendSpeed, retractSpeed;


public Program()

{

    mergeUpper = GridTerminalSystem.GetBlockWithName("Merge Block - Upper") as IMyShipMergeBlock;
    mergeLower = GridTerminalSystem.GetBlockWithName("Merge Block - Lower") as IMyShipMergeBlock;

    pistonMergeUpper = GridTerminalSystem.GetBlockWithName("Piston - Merge Upper") as IMyPistonBase;
    pistonMergeLower = GridTerminalSystem.GetBlockWithName("Piston - Merge Lower") as IMyPistonBase;
    pistonLiftUpper = GridTerminalSystem.GetBlockWithName("Piston - Lift Upper") as IMyPistonBase;
    pistonLiftLower = GridTerminalSystem.GetBlockWithName("Piston - Lift Lower") as IMyPistonBase;

    welderA = GridTerminalSystem.GetBlockWithName("Welder 1") as IMyShipWelder;
    welderB = GridTerminalSystem.GetBlockWithName("Welder 2") as IMyShipWelder;

    Activate(welderA);
    Activate(welderB);

    Runtime.UpdateFrequency = UpdateFrequency.Update100;
    
    extendSpeed = 0.5F;
    retractSpeed = -0.5F;

    currentPhase = new MergeTop();
    currentPhase.Begin();

    pistonLiftLower.MinLimit = 2.2F;
    pistonMergeLower.MaxLimit = 2.5F;
    pistonMergeUpper.MaxLimit = 2.5F;
}


public static void Activate(IMyShipMergeBlock mergeBlock) 
{
    mergeBlock.ApplyAction("OnOff_On");
}

public static void Activate(IMyShipWelder welder)
{
    welder.ApplyAction("OnOff_On");
}

public static void Deactivate(IMyShipMergeBlock mergeBlock) 
{
    mergeBlock.ApplyAction("OnOff_Off");
}

public static void Deactivate(IMyShipWelder welder)
{
    welder.ApplyAction("OnOff_Off");
}

public static void SetSpeed(IMyPistonBase piston, float velocity)
{
    piston.Velocity = velocity;
}

public static void Extend(IMyPistonBase piston)
{
    SetSpeed(piston, extendSpeed);
}

public static void Retract(IMyPistonBase piston)
{
    SetSpeed(piston, retractSpeed);
}


// Merge the top piston
public class MergeTop : IPhase
{
    public void Begin()
    {
        Activate(mergeUpper);
        Extend(pistonMergeUpper);
    }

    public bool IsFinished()
    {
        return (pistonMergeUpper.Status == PistonStatus.Extended && mergeUpper.IsConnected);
    }

    public void End()
    {

    }
    
    public IPhase GetNextPhase()
    {
        return new UnmergeBottom();
    }
}

// Unmerge the top piston
public class UnmergeTop : IPhase
{
    public void Begin()
    {
        Deactivate(mergeUpper);
        Retract(pistonMergeUpper);
    }

    public bool IsFinished()
    {
        return (pistonMergeUpper.Status == PistonStatus.Retracted);
    }

    public void End()
    {
    }
    
    public IPhase GetNextPhase()
    {
        return new Build();
    }
}

// Merge the bottom piston
public class MergeBottom : IPhase
{
    public void Begin()
    {
        Activate(mergeLower);
        Extend(pistonMergeLower);
    }

    public bool IsFinished()
    {
        return (pistonMergeLower.Status == PistonStatus.Extended && mergeLower.IsConnected);
    }

    public void End()
    {
    }
    
    public IPhase GetNextPhase()
    {
        return new UnmergeTop();
    }
}

// Unmerge the bottom piston
public class UnmergeBottom : IPhase
{
    public void Begin()
    {
        Deactivate(mergeLower);
        Retract(pistonMergeLower);
    }

    public bool IsFinished()
    {
        return (pistonMergeLower.Status == PistonStatus.Retracted);
    }

    public void End()
    {
    }
    
    public IPhase GetNextPhase()
    {
        return new Lift();
    }
}

// Lift up the assembly (hanging from the top)
public class Lift : IPhase
{
    public void Begin()
    {
        Retract(pistonLiftUpper);
        Retract(pistonLiftLower);
    }

    public bool IsFinished()
    {
        return (pistonLiftUpper.Status == PistonStatus.Retracted && pistonLiftLower.Status == PistonStatus.Retracted);
    }

    public void End()
    {
    
    }

    public IPhase GetNextPhase()
    {
        return new MergeBottom();
    }
}

public class Build : IPhase
{
    public void Begin()
    {
        Extend(pistonLiftUpper);
        Extend(pistonLiftLower);
    }

        public bool IsFinished()
    {
        return (pistonLiftUpper.Status == PistonStatus.Extended && pistonLiftLower.Status == PistonStatus.Extended);
    }

    public void End()
    {
       
    }

    public IPhase GetNextPhase()
    {
        return new MergeTop();
    }
}


public void Main(string argument, UpdateType updateSource)

{

    if (currentPhase == null) 
    {
        Runtime.UpdateFrequency = UpdateFrequency.None;
        Echo("Finished Executing");
        return;
    }

    if (currentPhase.IsFinished()) 
    {
        Echo("Executing...");
        currentPhase.End();
        currentPhase = currentPhase.GetNextPhase();
        currentPhase.Begin();
    }
}

