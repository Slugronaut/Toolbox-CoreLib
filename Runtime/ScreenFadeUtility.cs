using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Toolbox
{
    /// <summary>
    /// Utility class for autonomously handling a screen color fade.
    /// 
    /// TODO: Auto instantiation disabled pending review. 
    /// </summary>
    public class ScreenFadeUtility : MonoBehaviour
    {
        Color CachedColor = Color.black;
        Texture2D CachedTex;
        float Alpha = 0;
        bool Fading;
        //Dictionary<Color, Texture2D> FadeTexs = new Dictionary<Color, Texture2D>();
        Texture2D WhiteTexture;

        Texture2D Texture
        {
            get
            {
                if (WhiteTexture == null)
                {
                    WhiteTexture = new Texture2D(1, 1);
                    WhiteTexture.SetPixel(0, 0, Color.white);
                    WhiteTexture.Apply();
                }
                return WhiteTexture;
            }
        }

        static ScreenFadeUtility _Instance;
        public static ScreenFadeUtility Instance
        {
            get
            {
                if (_Instance == null)
                {
                    var go = Camera.main;
                    if(go != null)
                        _Instance = go.gameObject.AddComponent<ScreenFadeUtility>();
                }

                return _Instance;
            }
        }

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void CreateInstance()
        {
            if (_Instance == null)
            {
                var go = Camera.main;
                _Instance = go.gameObject.AddComponent<ScreenFadeUtility>();
                var temp = _Instance.Texture; //init the texture now
            }
        }

        private void Awake()
        {
            _Instance = this;
        }

        private void OnDestroy()
        {
            _Instance = null;
        }

        public void EndFadeout()
        {
            Fading = true;
            Alpha = 0;
        }

        public void FadeTo(Color color, float time, bool hold, Action completedCallback, Action frameCallback)
        {
            throw new UnityException("Disabled. Needs review.");
            StartCoroutine(CoroutineFadeTo(color, time, hold, completedCallback, frameCallback));
        }

        public void FadeFrom(Color color, float time, Action completedCallback, Action frameCallback)
        {
            throw new UnityException("Disabled. Needs review.");
            StartCoroutine(CoroutineFadeFrom(color, time, completedCallback, frameCallback));
        }

        IEnumerator CoroutineFadeFrom(Color targetColor, float time, Action completedCallback, Action frameCallback)
        {
            Fading = true;
            CachedColor = targetColor;
            CachedTex = Texture;
            Alpha = 1;
            while (Fading && Alpha > 0)
            {
                yield return null;
                Alpha -= (1 / time) * Time.unscaledDeltaTime;
                frameCallback?.Invoke();
            }

            completedCallback?.Invoke();
            Alpha = 0;
        }

        IEnumerator CoroutineFadeTo(Color targetColor, float time, bool hold, Action completedCallback, Action frameCallback)
        {
            Fading = true;
            CachedColor = targetColor;
            CachedTex = Texture;
            Alpha = 0;
            while (Fading && Alpha < 1)
            {
                yield return null;
                Alpha += (1 / time) * Time.unscaledDeltaTime;
                frameCallback?.Invoke();
            }

            completedCallback?.Invoke();

            Alpha = 1;

            //one last render to be sure we render at full opacity
            Render(Alpha);
        }

        private IEnumerator OnPostRender()
        {
            yield return CoroutineWaitFactory.EndOfFrame;
            if (Alpha > 0)
                Render(Alpha);
        }

        void Render(float alpha)
        {
            //Unity 2018.3 changed the rules on us so now we need to set a custom matrix in order for the
            //motherfuker to *actually* rendering in screen pixel space. Godamn motherfucker fuckers!
            //Always fuckin' motherfuckers!
            GL.PushMatrix();
            GL.LoadPixelMatrix();
            UnityEngine.Graphics.DrawTexture(
                    new Rect(0,0, Screen.width, Screen.height),
                    CachedTex,
                    new Rect(0, 0, 1, 1), 0, 0, 0, 0,
                    new Color(CachedColor.r, CachedColor.g, CachedColor.b, alpha),
                    null);
            GL.PopMatrix();
        }
    }


    /// <summary>
    /// Can be passed to a fadeout operation by ScreenFadeUtility to handle events at certain fade checkpoints.
    /// </summary>
    [Serializable]
    public class FadeCheckpoint
    {
        public float Percentage;
        public UnityEvent OnFadeCheckpoint;

        public bool Triggered { get; set; }

    }
}
