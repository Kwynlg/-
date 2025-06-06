/****************************************************
    文件：PlayerAttribute.cs
	作者：Kaiy
    邮箱: 3120713254@qq.com
    日期：#CreateTime#	
	功能：Nothing
*****************************************************/

using UnityEngine;

public class PlayerAttribute : Singleton<PlayerAttribute> 
{
    public bool isWq = false;//是否为持有武器状态
    public double att = 35; //玩家攻击力

    public bool sk1 = false; //技能1
    public float sk1CD = 4f; //技能1冷却时间

    public bool sk2 = false; //技能2
    public float sk2CD = 5f; //技能2冷却时间

    public bool sk3 = false; //技能3
    public float sk3CD = 7f; //技能3冷却时间

    public bool sk4 = false; //技能4
    public float sk4CD = 9f; //技能4冷却时间

    public bool art = false; //轻功

    public bool roll = false;//翻滚
}