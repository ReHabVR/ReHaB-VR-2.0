using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ColorGame : MonoBehaviour 
{
    public float delay = 3.0f;
    public int totalRounds = 4;

    public GameObject firstButton;
    public GameObject secondButton;
    public GameObject thirdButton;
    
    public AudioClip validSoundCue;
    public AudioClip wrongSoundCue;
    
    [Header("Materials")]
    public Material blueLight;
    public Material blueDark;
    public Material greenLight;
    public Material greenDark;
    public Material yellowLight;
    public Material yellowDark;
    
    // <timeElapsed, tries>
    public UnityEvent<float, int> correctButtonPressed;

    private readonly System.Random _random = new();
    private int _currentActiveButtonId;
    private int currentRound = 0;

    private DateTime startTime = DateTime.Now;
    private int tries = 0;
    
    void Start()
    {
        StartCoroutine(ChooseNextButton());
    }

    public void ChooseButton(int id)
    {
        // Increment tries on every button press, even it it was not the right button.
        tries++;

        if (id == _currentActiveButtonId) // On successful press
        {
            GetComponent<AudioSource>().PlayOneShot(validSoundCue);
            currentRound++;
            MakeAllDark();

            // Measure time elapsed
            float timeElapsed = (float)(DateTime.Now - startTime).TotalSeconds;
            correctButtonPressed?.Invoke(timeElapsed, tries);
            Debug.Log($"Time elapsed: {timeElapsed:F2}\nTries: {tries}. Accuracy: {1.0f / tries * 100.0f}%");

            // Keep playing unless it's last round
            if (currentRound < totalRounds)
            {
                StartCoroutine(ChooseNextButton());
            }   
        }
        else
        {
            GetComponent<AudioSource>().PlayOneShot(wrongSoundCue);
        }
    }

    IEnumerator ChooseNextButton()
    {
        yield return new WaitForSeconds(delay);

        tries = 0;
        int nextButton = _random.Next(0, 3);
        _currentActiveButtonId = nextButton;
        switch(nextButton)
        {
            case 0:
            {
                firstButton.GetComponent<MeshRenderer>().material = blueLight;
                firstButton.GetComponent<AudioSource>().PlayOneShot(validSoundCue);
                break;
            }
    
            case 1:
            {
                secondButton.GetComponent<MeshRenderer>().material = greenLight;
                secondButton.GetComponent<AudioSource>().PlayOneShot(validSoundCue);
                break;
            }

            case 2:
            {
                thirdButton.GetComponent<MeshRenderer>().material = yellowLight;
                thirdButton.GetComponent<AudioSource>().PlayOneShot(validSoundCue);
                break;
            }
        }
        // Start measuring elapsed time
        startTime = DateTime.Now;
    }

    void MakeAllDark()
    {
        firstButton.GetComponent<MeshRenderer>().material = blueDark;
        secondButton.GetComponent<MeshRenderer>().material = greenDark;
        thirdButton.GetComponent<MeshRenderer>().material = yellowDark;
    }
}
