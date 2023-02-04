using System;
using Toolbox.AutoCreate;
using Toolbox.Lazarus;
using UnityEngine;


namespace Toolbox.Behaviours
{
    /// <summary>
    /// Attached to a temporary sound. Will remove itself after sound has finished playing.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [DisallowMultipleComponent]
    public sealed class TempAudioSource : MonoBehaviour
    {
        public const int NoUser = int.MaxValue;
        public AudioSource Source;

        [HideInInspector]
        [NonSerialized]
        public int LastUserId = int.MaxValue;

        IPoolSystem Lazarus;
        TempAudioSourcePlayer AudioPlayer;


        void Awake()
        {
            Source = GetComponent<AudioSource>();
            Lazarus = AutoCreator.AsSingleton<IPoolSystem>();
            AudioPlayer = AutoCreator.AsSingleton<TempAudioSourcePlayer>();
        }

        void OnDisable()
        {
            LastUserId = NoUser;
        }
        
        private void LateUpdate()
        {
            if (Source == null || !Source.isPlaying)
            {
                if(Source != null) Source.Stop();
                Lazarus.RelenquishToPool(gameObject);
                AudioPlayer.ClearCachedId(LastUserId);
            }
        }
        
    }


    /// <summary>
    /// 
    /// </summary>
    public enum InterruptModes : byte
    {
        Interrupt,
        Overlap,
        DropIfSameClip,
        DropIfAny,
        DropIfChannelFlooded,
    }
}
