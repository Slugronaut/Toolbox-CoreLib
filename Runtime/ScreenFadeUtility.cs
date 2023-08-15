using System;
using System.Collections;
using System.Collections.Generic;
using Toolbox.AutoCreate;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Toolbox
{
    /// <summary>
    /// Utility class for autonomously handling a screen color fade.
    /// </summary>
    [AutoCreate(CreationTime=RuntimeInitializeLoadType.AfterSceneLoad)]
    public class ScreenFadeUtility
    {
        Color CachedColor = Color.black;
        Texture2D CachedTex;
        float Alpha = 0;
        bool Fading;
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
        public static ScreenFadeUtility Instance => _Instance;

        void AutoAwake()
        {
            //we are using Start instead of Awake because the global coroutine object needs to initialize first in awake
            _Instance = this;
            RenderPipelineManager.endContextRendering += HandleRenderFrame;
        }

        void AutoDestroy()
        {
            RenderPipelineManager.endContextRendering -= HandleRenderFrame;
            _Instance = null;
        }

        public void EndFadeout()
        {
            Fading = true;
            Alpha = 0;
        }

        public void FadeTo(Color color, float time, bool hold, Action completedCallback, Action frameCallback)
        {
            GlobalCoroutine.Start(CoroutineFadeTo(color, time, hold, completedCallback, frameCallback));
        }

        public void FadeFrom(Color color, float time, Action completedCallback, Action frameCallback)
        {
            GlobalCoroutine.Start(CoroutineFadeFrom(color, time, completedCallback, frameCallback));
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

        void HandleRenderFrame(ScriptableRenderContext context, List<Camera> cams)
        {
            if (Alpha > 0)
                Render(Alpha);
        }

        void Render(float alpha)
        {
            //Unity 2018.3 changed the rules on us so now we need to set a custom matrix in order for the
            //motherfuker to *actually* rendering in screen pixel space. Godamn motherfucker fuckers!
            //Always fuckin' motherfuckers!
            //Looks like the rules changed again in URP. Now we need a fucking' motherfucking ortho matrix.
            //God-damnit godamnit!
            GL.PushMatrix();
            GL.LoadOrtho();
            //GL.LoadPixelMatrix();
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
