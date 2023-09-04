using UnityEngine;
using Sirenix.OdinInspector;
using Peg.AutoCreate;
using Peg.Lazarus;

namespace Peg.Behaviours
{
    /// <summary>
    /// Destroys the attached particlesystem upon finishing.
    /// </summary>
    public class ParticleEffectAutoDestruct : MonoBehaviour
    {
        [HideIf("IsActionRelenquish")]
        [Tooltip("The particle system this affects.")]
        public ParticleSystem Effect;

        [ShowIf("IsActionRelenquish")]
        [Tooltip("If set, this object will be acted on instead of the Effect gameobject.")]
        public GameObject Obj;

        [Tooltip("What action occurs when this particle system ends. Note that if set to <i>Relenquish</i> the whole GameObject is affected.")]
        public DestructEffect ActionWhenFinished;

        [HideIf("IsActionRelenquish")]
        [Tooltip("Is the whole GameObject destroyed or just the particle system?")]
        public bool DestroyWholeGameObject = true;

        IPoolSystem Lazarus;
        bool Started;

        private void Awake()
        {
            Lazarus = AutoCreator.AsSingleton<IPoolSystem>();
        }

        /// <summary>
        /// Internal helper for FullInspector.ShowIf.
        /// </summary>
        /// <returns></returns>
        bool IsActionRelenquish()
        {
            return ActionWhenFinished == DestructEffect.Relenquish;
        }

        void OnEnable()
        {
            //sentinal against trying to update when we shouldn't
            if (Effect == null)
                enabled = false;
        }

        void Update()
        {
            if (!Started)
            {
                if (Effect.isPlaying)
                    Started = true;
                return;
            }


            if (!Effect.IsAlive(true))
            {
                if (ActionWhenFinished == DestructEffect.Destroy)
                {
                    if (DestroyWholeGameObject)
                        Destroy(Effect.gameObject);
                    else Destroy(Effect);
                }
                else
                {
                    if (Obj != null)
                        Lazarus.RelenquishToPool(Obj);
                    else Lazarus.RelenquishToPool(Effect.gameObject);
                }
            }
        }

    }
}
