using UnityEngine;
using System.Collections;

namespace Kino.TileMap.Test
{
    public class StartNodeMarker : MonoBehaviour {
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
            }
        }
    }
}