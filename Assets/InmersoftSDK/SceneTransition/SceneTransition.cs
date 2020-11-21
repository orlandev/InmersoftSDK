// Copyright (C) 2017-2018 InmersoftSDK. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace InmersoftSDK.SceneTransition
{
    // This class is responsible for loading the next scene in a transition.
    public class SceneTransition : MonoBehaviour
    {
        /**
         *  SceneTransition Inmersoft 2020
         * Programed by Orlando Novas Rodriguez
         * Inmersoft Programmers Team
         *         2020
         */
        public Color color = Color.black;

        public float duration = 1.0f;
        public string scene = "<Insert scene name>";
        public Sprite fadeSprite;

        /// <summary>
        ///     Performs the transition to the next scene using a background color.
        /// </summary>
        public void PerformTransition()
        {
            Transition.LoadLevel(scene, duration, color);
        }

        /// <summary>
        /// Performs the transition to the next scene using a sprite like a background.
        /// </summary>
        public void PerformSpriteTransition()
        {
            Transition.LoadLevel(scene, duration, color, fadeSprite);
        }
    }
}