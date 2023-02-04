using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System;
using Toolbox.AutoCreate;
using Toolbox.Collections;
using Toolbox.Lazarus;

namespace Toolbox.Behaviours
{
    /// <summary>
    /// Global singleton used to play one-shot audio clips at specific world positions.
    /// </summary>
    [AutoCreate(CreationActions.DeserializeSingletonData)]
    public sealed class TempAudioSourcePlayer
    {
        [Serializable]
        public class SoundCategory
        {
            [Tooltip("Acts as the resource path to the prefab as well as an identifier.")]
            public HashedString Id;
            public int AllocChunk = 10;
            public int MaxPool = 100;
        }
        public List<SoundCategory> SoundCategories;
        [Tooltip("Editor-only. Must set before playing.")]
        public bool HideAudioSources = true;

        public static TempAudioSourcePlayer Instance { get; private set; }
        
        //Used to track who played what source last. This way we can stop the previous sound
        //if the same user tries to play another one that would overlap it.
        static readonly HashMap<int, TempAudioSource> Cache = new(25);

        [AutoResolve]
        IPoolSystem Lazarus;
        List<GameObject> Prefabs;
        bool BugFix;

        void AutoAwake()
        {
            Instance = this;
            RefreshSourceList();

            if (!BugFix)
            {
                //Debug.LogError("TempAudioSourcePlayer - Singleton Awake Called");
                
                //YIKES! This is getting called twice!!
                SceneManager.sceneLoaded += OnSceneLoaded;
                BugFix = true;
            }
        }

        void AutoDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //Cache.Clear();
        }

        /// <summary>
        /// Returns true if the IDed GameObject is playing the given audio clip already.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="clip"></param>
        public bool IsPlayingClip(int instanceId, AudioClip clip)
        {
            if (Cache.TryGetValue(instanceId, out TempAudioSource source))
            {
                if (source.Source.isPlaying && source.Source.clip == clip)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the IDed GameObject is playing any audio clip already.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="clip"></param>
        public bool IsPlayingAny(int instanceId)
        {
            if (Cache.TryGetValue(instanceId, out TempAudioSource source))
            {
                if (source.Source.isPlaying)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the audio channel has any un-used voices left.
        /// </summary>
        /// <returns></returns>
        public bool HasUnusedVoicesAvailableOnChannel(int audioId)
        {
            return Lazarus.InactivePoolCount(Prefabs[audioId]) > 0 ||
                   Lazarus.IsVirginPool(Prefabs[audioId]);
        }

        /// <summary>
        /// Helper method that allows us to play a sound with a given interrupt mode.
        /// </summary>
        public void PlayWithInterruptMode(InterruptModes mode, int audioId, int instanceId, AudioClip clip, Vector3 position, float volume = 1.0f)
        {
            if (mode == InterruptModes.Interrupt)
                StopLastAndPlayNew(audioId, instanceId, clip, position, volume);
            else if (mode == InterruptModes.DropIfSameClip && IsPlayingClip(instanceId, clip))
                return;
            else if (mode == InterruptModes.DropIfAny && IsPlayingAny(instanceId))
                return;
            else if (mode == InterruptModes.Overlap)
                Play(audioId, clip, position, volume);
           else if(mode == InterruptModes.DropIfChannelFlooded && HasUnusedVoicesAvailableOnChannel(audioId))
                StopLastAndPlayNew(audioId, instanceId, clip, position, volume);
        }

        /// <summary>
        /// Helper method that allows us to play a sound with a given interrupt mode.
        /// </summary>
        public void PlayWithInterruptMode(InterruptModes mode, int audioId, int instanceId, AudioClip clip, AudioMixerGroup mixerGroup, Vector3 position, float volume = 1.0f)
        {
            if (mode == InterruptModes.Interrupt)
                StopLastAndPlayNew(audioId, instanceId, clip, mixerGroup, position, volume);
            else if (mode == InterruptModes.DropIfSameClip && IsPlayingClip(instanceId, clip))
                return;
            else if (mode == InterruptModes.DropIfAny && IsPlayingAny(instanceId))
                return;
            else if (mode == InterruptModes.Overlap)
                Play(audioId, clip, mixerGroup, position, volume);
            else if (mode == InterruptModes.DropIfChannelFlooded && HasUnusedVoicesAvailableOnChannel(audioId))
                StopLastAndPlayNew(audioId, instanceId, clip, mixerGroup, position, volume);
        }

        /// <summary>
        /// Plays a one-shot audio clip at a position in world space.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="worldPos"></param>
        /// <param name="volume"></param>
        public void Play(AudioClip clip, Vector3 worldPos, float volume = 1.0f)
        {
            Play(0, clip, worldPos, volume);
        }

        /// <summary>
        /// Plays a one-shot audio clip at a position in world space.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="worldPos"></param>
        /// <param name="volume"></param>
        public void Play(AudioClip clip, AudioMixerGroup mixerGroup, Vector3 worldPos, float volume = 1.0f)
        {
            Play(0, clip, mixerGroup, worldPos, volume);
        }

        /// <summary>
        /// Plays a one-shot audio clip at a position in world space.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="worldPos"></param>
        /// <param name="volume"></param>
        public void Play(int prefabIndex, AudioClip clip, Vector3 worldPos, float volume = 1.0f)
        {
            if (clip == null) return;
            
            GameObject go = Lazarus.RecycleSummon(Prefabs[prefabIndex], worldPos);
            #if UNITY_EDITOR
            if(HideAudioSources)
                go.hideFlags = HideFlags.HideInHierarchy;// HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.NotEditable;
            #endif
            var source = go.GetComponent<TempAudioSource>();
            source.LastUserId = TempAudioSource.NoUser;
            source.Source.clip = clip;
            source.Source.volume = volume;
            source.Source.Play();
        }

        /// <summary>
        /// Plays a one-shot audio clip at a position in world space.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="worldPos"></param>
        /// <param name="volume"></param>
        public void Play(int prefabIndex, AudioClip clip, AudioMixerGroup mixerGroup, Vector3 worldPos, float volume = 1.0f)
        {
            if (clip == null) return;

            GameObject go = Lazarus.RecycleSummon(Prefabs[prefabIndex], worldPos);
            #if UNITY_EDITOR
            if (HideAudioSources)
                go.hideFlags = HideFlags.HideInHierarchy;// HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.NotEditable;
            #endif
            var source = go.GetComponent<TempAudioSource>();
            source.LastUserId = TempAudioSource.NoUser;
            source.Source.clip = clip;
            source.Source.volume = volume;
            source.Source.outputAudioMixerGroup = mixerGroup;
            source.Source.Play();
        }

        /// <summary>
        /// Plays a one-shot audio clip at a position in world space.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="worldPos"></param>
        /// <param name="volume"></param>
        public void Play(int prefabIndex, int instanceId, AudioClip clip, Vector3 worldPos, float volume = 1.0f)
        {
            if (clip == null) return;

            GameObject go = Lazarus.RecycleSummon(Prefabs[prefabIndex], worldPos);
            #if UNITY_EDITOR
            if (HideAudioSources)
                go.hideFlags = HideFlags.HideInHierarchy;// HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.NotEditable;
            #endif
            var source = go.GetComponent<TempAudioSource>();
            source.LastUserId = TempAudioSource.NoUser;
            source.Source.clip = clip;
            source.Source.volume = volume;
            source.Source.Play();
            Cache[instanceId] = source;
        }

        /// <summary>
        /// Plays a one-shot audio clip at a position in world space.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="worldPos"></param>
        /// <param name="volume"></param>
        public void Play(int prefabIndex, int instanceId, AudioClip clip, AudioMixerGroup mixerGroup, Vector3 worldPos, float volume = 1.0f)
        {
            if (clip == null) return;

            GameObject go = Lazarus.RecycleSummon(Prefabs[prefabIndex], worldPos);
#if UNITY_EDITOR
            if (HideAudioSources)
                go.hideFlags = HideFlags.HideInHierarchy;// HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.NotEditable;
#endif
            var source = go.GetComponent<TempAudioSource>();
            source.LastUserId = TempAudioSource.NoUser;
            source.Source.clip = clip;
            source.Source.volume = volume;
            source.Source.Play();
            source.Source.outputAudioMixerGroup = mixerGroup;
            Cache[instanceId] = source;
        }

        /// <summary>
        /// Plays a one-shot audio clip at a position in world space. The user is
        /// cached so that if the same user tries to play the same clip next time,
        /// the previous one-shot will be stopped first if it is still playing.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="clip"></param>
        /// <param name="worldPos"></param>
        /// <param name="volume"></param>
        public void StopLastAndPlayNew(int instanceId, AudioClip clip, Vector3 worldPos, float volume = 1.0f)
        {
            StopLastAndPlayNew(0, instanceId, clip, worldPos, volume);
        }

        /// <summary>
        /// Stops all sounds that originated from a particular GameObject instance.
        /// </summary>
        /// <param name="instanceId"></param>
        public void StopAllFromSource(int instanceId)
        {
            if (Cache.TryGetValue(instanceId, out TempAudioSource source))
            {
                //added this check for null due to issues with changing scenes and lost data pools
                if (TypeHelper.IsReferenceNull(source) || TypeHelper.IsReferenceNull(source.Source))
                    Cache.Remove(instanceId);
                else if (source.Source.isPlaying)
                {
                    source.Source.Stop();
                    Cache.Remove(instanceId);
                }
            }
        }

        /// <summary>
        /// Plays a one-shot audio clip at a position in world space. The user is
        /// cached so that if the same user tries to play the same clip next time,
        /// the previous one-shot will be stopped first if it is still playing.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="clip"></param>
        /// <param name="worldPos"></param>
        /// <param name="volume"></param>
        public void StopLastAndPlayNew(int prefabIndex, int instanceId, AudioClip clip, Vector3 worldPos, float volume = 1.0f)
        {
            if (Cache.TryGetValue(instanceId, out TempAudioSource source))
            {
                //added this check for null due to issues with changing scenes and lost data pools
                if (TypeHelper.IsReferenceNull(source) || TypeHelper.IsReferenceNull(source.Source))
                    Cache.Remove(instanceId);
                else if (clip == null || (source.Source.clip == clip && source.Source.isPlaying))
                {
                    source.Source.Stop();
                    Cache.Remove(instanceId);
                }
            }

            if (clip == null) return;
            GameObject go = Lazarus.RecycleSummon(Prefabs[prefabIndex], worldPos);
            //go.hideFlags = HideFlags.HideInHierarchy;//HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.NotEditable;
            source = go.GetComponent<TempAudioSource>();
            source.LastUserId = instanceId;
            source.Source.clip = clip;
            source.Source.volume = volume;
            source.Source.Play();
            Cache[instanceId] = source;
        }

        /// <summary>
        /// Plays a one-shot audio clip at a position in world space. The user is
        /// cached so that if the same user tries to play the same clip next time,
        /// the previous one-shot will be stopped first if it is still playing.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="clip"></param>
        /// <param name="worldPos"></param>
        /// <param name="volume"></param>
        public void StopLastAndPlayNew(int prefabIndex, int instanceId, AudioClip clip, AudioMixerGroup mixerGroup, Vector3 worldPos, float volume = 1.0f)
        {
            if (Cache.TryGetValue(instanceId, out TempAudioSource source))
            {
                //added this check for null due to issues with changing scenes and lost data pools
                if (TypeHelper.IsReferenceNull(source) || TypeHelper.IsReferenceNull(source.Source))
                    Cache.Remove(instanceId);
                else if (clip == null || (source.Source.clip == clip && source.Source.isPlaying))
                {
                    source.Source.Stop();
                    Cache.Remove(instanceId);
                }
            }

            if (clip == null) return;
            GameObject go = Lazarus.RecycleSummon(Prefabs[prefabIndex], worldPos);
            //go.hideFlags = HideFlags.HideInHierarchy;//HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.NotEditable;
            source = go.GetComponent<TempAudioSource>();
            source.LastUserId = instanceId;
            source.Source.clip = clip;
            source.Source.volume = volume;
            source.Source.outputAudioMixerGroup = mixerGroup;
            source.Source.Play();
            Cache[instanceId] = source;
        }

        /// <summary>
        /// Called by the TempAudioSource when it has stopped.
        /// This lets the source player know it can release
        /// that instance id from its internal cache.
        /// </summary>
        /// <param name="instanceId"></param>
        public void ClearCachedId(int instanceId)
        {
            if(instanceId != TempAudioSource.NoUser) 
                Cache.Remove(instanceId);
        }
        
        /// <summary>
        /// 
        /// </summary>
        void RefreshSourceList()
        {
            Prefabs = new List<GameObject>();
            if (SoundCategories != null)
            {
                for (int i = 0; i < SoundCategories.Count; i++)
                {
                    var res = Resources.Load<GameObject>(SoundCategories[i].Id.Value);
                    if (res != null) Prefabs.Add(res);
                }
            }

            //this should force the AutoPooler to set the chunk
            //and max pool sizes for each of these prefabs
            for (int i = 0; i < Prefabs.Count; i++)
            {
                Lazarus.ForceRecreatePool(Prefabs[i], SoundCategories[i].AllocChunk, 1, SoundCategories[i].MaxPool);
            }
        }
        
    }
}
