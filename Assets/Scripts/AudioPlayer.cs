//	Created by: Sunny Valley Studio 
//	https://svstudio.itch.io

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SVS
{

    public class AudioPlayer : MonoBehaviour
    {
        public AudioClip placementSound;
        public AudioClip coinCollectionSound1; // Sound for 1 coin
        public AudioClip coinCollectionSound2; // Sound for 2 coins
        public AudioClip coinCollectionSound3; // Sound for 3 coins
        public AudioSource audioSource;

        public static AudioPlayer instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(this.gameObject);

        }

        public void PlayPlacementSound()
        {
            if(placementSound != null)
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

            if(soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }
    }
}