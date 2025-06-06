/****************************************************
    文件：GlobalObj.cs
	作者：Kaiy
    邮箱: 3120713254@qq.com
    日期：#CreateTime#	
	功能：Nothing
*****************************************************/

using UnityEngine;


/// <summary>
/// 全局对象组件
/// </summary>
public class GlobalObj : Singleton<GlobalObj> 
{
    [Header("UI面板")]
    public GameObject _Panel_Dialogue; // 对话面板
    public GameObject _Panel_Tips; // 提示面板

    



}