using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kino.TileMap.Test
{
    public class NodePathMarker : MonoBehaviour {        
        private static string gameObjName = "_NodePathMarker";

        public List<SquareTileMapNode> findedPath;

        public static string GameObjName {
            get {return gameObjName;}
        }

    	void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.black;

                if (findedPath != null) {
                    for (int i = 0; i < findedPath.Count - 1; ++ i)
                    {
                        Gizmos.DrawLine(findedPath[i].WorldPosition, findedPath[i + 1].WorldPosition);
                    }
                }
            }
        }
    }
}