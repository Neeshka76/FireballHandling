using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using SnippetCode;
using System.Collections;

namespace SnippetCode
{
    static class SnippetCode
    {
        /// <summary>
        /// Vector pointing away from the palm
        /// </summary>
        public static Vector3 PalmDir(this RagdollHand hand) => hand.transform.forward * -1;

        /// <summary>
        /// Vector pointing in the direction of the thumb
        /// </summary>
        public static Vector3 ThumbDir(this RagdollHand hand) => (hand.side == Side.Right) ? hand.transform.up : -hand.transform.up;

        /// <summary>
        /// Vector pointing away in the direction of the fingers
        /// </summary>
        public static Vector3 PointDir(this RagdollHand hand) => -hand.transform.right;

        /// <summary>
        /// Get a point above the player's hand
        /// </summary>
        public static Vector3 PosAboveBackOfHand(this RagdollHand hand) => hand.transform.position - hand.transform.right * 0.1f + hand.transform.forward * 0.2f;
    }
}
