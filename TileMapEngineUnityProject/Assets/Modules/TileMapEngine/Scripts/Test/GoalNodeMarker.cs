using UnityEngine;
using System.Collections;

namespace Kino.TileMap.Test
{
    public class GoalNodeMarker : MonoBehaviour {
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
            }
        }
    }
}