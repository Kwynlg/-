/****************************************************
    文件：AudioManager.cs
	作者：Kaiy
    邮箱: 3120713254@qq.com
    日期：#CreateTime#	
	功能：Nothing
*****************************************************/

using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public AudioSource audioSource;

    void Update()
    {
        
    }

    public bool PlayAudio()
    {
        if (audioSource != null && !audioSource.isPlaying && audioSource.time > 0f)
        {
            Debug.Log("播放完毕");

            audioSource = null; // 防止重复输出

            //隐藏面板
            GlobalObj.Instance._Panel_Dialogue.SetActive(false);
            GlobalObj.Instance._Panel_Tips.SetActive(false);

            return true;
        }
        return false;
    }
}