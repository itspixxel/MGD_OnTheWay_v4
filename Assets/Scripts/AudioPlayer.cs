//	Created by: Sunny Valley Studio 
//	https://svstudio.itch.io

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SVS
{
    [System.Serializable]
    public class SoundButton
    {
        public string buttonName;
        public AudioClip soundClip;
        public Button buttonReference;
    }

    public class AudioPlayer : MonoBehaviour
    {
        public AudioClip placementSound;
        public AudioClip coinCollectionSound1; // Sound for 1 coin
        public AudioClip coinCollectionSound2; // Sound for 2 coins
        public AudioClip coinCollectionSound3; // Sound for 3 coins
        public AudioSource audioSource;

        [Header("Sound Buttons")]
        [Tooltip("Add buttons with associated sound clips here")]
        public SoundButton[] soundButtons;

        public static AudioPlayer instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(this.gameObject);
        }

        private void Start()
        {
            InitializeSoundButtons();
        }

        private void InitializeSoundButtons()
        {
            // Setup listeners for all buttons
            if (soundButtons != null && soundButtons.Length > 0)
            {
                foreach (SoundButton soundButton in soundButtons)
                {
                    if (soundButton.buttonReference != null)
                    {
                        // Create a local variable to capture the current soundButton
                        SoundButton currentButton = soundButton;

                        // Remove any existing listeners to prevent duplicates
                        soundButton.buttonReference.onClick.RemoveAllListeners();

                        // Add listener to play the associated sound when clicked
                        soundButton.buttonReference.onClick.AddListener(() => PlayButtonSound(currentButton));
                    }
                    else
                    {
                        Debug.LogWarning($"Button reference for '{soundButton.buttonName}' is not assigned!");
                    }
                }
            }
        }

        public void PlayButtonSound(SoundButton soundButton)
        {
            if (soundButton != null && soundButton.soundClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(soundButton.soundClip);
            }
        }

        // Play sound by button index
        public void PlayButtonSoundByIndex(int index)
        {
            if (soundButtons != null && index >= 0 && index < soundButtons.Length)
            {
                if (soundButtons[index].soundClip != null)
                {
                    audioSource.PlayOneShot(soundButtons[index].soundClip);
                }
            }
            else
            {
                Debug.LogWarning($"Invalid sound button index: {index}");
            }
        }

        public void PlayPlacementSound()
        {
            if (placementSound != null)
            {
                audioSource.PlayOneShot(placementSound);
            }
        }

        public void PlayCoinCollectionSound(int coinsCollected)
        {
            AudioClip soundToPlay = null;

            switch (coinsCollected)
            {
                case 1:
                    soundToPlay = coinCollectionSound1;
                    break;
                case 2:
                    soundToPlay = coinCollectionSound2;
                    break;
                case 3:
                    soundToPlay = coinCollectionSound3;
                    break;
            }

            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }
    }
}