using System.Collections;

public class GameConst 
{
    public int TEAM_RED = 0;
    public int TEAM_BLUE = 1;

    public float MaxRotateChange = 3.0f;
    public float TowerMaxRotateChange = 3.0f;
    public float MoveSpeed = 8.0f;
    public float BulletSpeed = 20.0f;


    public float SectorDegree = 10;
    public float SectorDistance = 20;
    public float SectorLength = 15;

    public int AssistDamage = 50;

    public int StaticShootBuffShowDelay = 0;
    public float StaticShootBuffDamageRatio = 2;

    public static GameConst Instance;

    public static int LeftNotEnterTime = 30;
    public static int TotalLeftTime = 10;

    private void Awake()
    {
        Instance = this;
    }
}
