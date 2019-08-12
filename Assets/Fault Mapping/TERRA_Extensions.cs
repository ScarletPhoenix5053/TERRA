using UnityEngine;
using System.Collections;

namespace SCARLET.TERRA
{
    public static class TERRA_Extensions
    {
        public static Transform GetChildWIthTag(this Transform transform, string tag)
        {
            // Return the first child found with the specified tag.
            foreach (Transform child in transform)
            {
                if (child.tag == tag) return child;
            }
            return null;
        }

        public static bool Contains(this string[] array, string targetString)
        {
            // Return true as soon as a match is found
            foreach (string child in array)
                if (child == targetString) return true;

            return false;
        }
    }
}