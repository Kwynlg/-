using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BtnKey : MonoBehaviour
{
    public Button playButton;
    public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        playButton = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();

        playButton.onClick.AddListener(playSound);

    }

    void playSound() { 
    
    audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
