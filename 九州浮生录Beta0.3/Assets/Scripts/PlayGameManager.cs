/****************************************************
    文件：PlayGameManager.cs
	作者：Kaiy
    邮箱: 3120713254@qq.com
    日期：#CreateTime#	
	功能：Nothing
*****************************************************/

using UnityEngine;

public class PlayGameManager : Singleton<PlayGameManager>
{

    public void SetObj(GameObject obj, bool bol = true)
    {
        obj.SetActive(bol);
    }
}