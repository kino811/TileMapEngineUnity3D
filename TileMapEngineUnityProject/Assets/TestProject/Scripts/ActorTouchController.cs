using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Kino.TileMap;

namespace Kino.TileMap.Test
{
    [RequireComponent(typeof(ActorController))]
    public class ActorTouchController : MonoBehaviour {
        public bool checkObjWhenPathFind = false;
        public bool goalCheckObjWhenPathFind = false;

        private ActorController player;

        void Awake()
        {
            this.player = GetComponent<ActorController>();
        }
    	
    	// Update is called once per frame
    	void Update () {        
            if (Input.GetMouseButtonUp(0)) {                
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit;
                int layerMask = 1 << LayerMask.NameToLayer("MapTile");

                hit = Physics2D.Raycast(ray.origin, ray.direction, 100.0f, layerMask);
                if (hit.collider) {
                    SquareTileMapNode mapTile = hit.collider.gameObject.GetComponent<SquareTileMapNode>();
                    SquareTileMapNode tileNode = null;
                    if (player.AutoMoving) {
                        tileNode = TileMapEngine.Instance.GetTileNode(player.TargetTilePos);
                    }
                    else {
                        tileNode = TileMapEngine.Instance.GetTileNode(player.CurTilePos);
                    }

                    if (mapTile && tileNode) {
                        List<SquareTileMapNode> pathNodes = TileMapEngine.Instance.Calculate(tileNode, mapTile, checkObjWhenPathFind, goalCheckObjWhenPathFind);

                        if (pathNodes != null && pathNodes.Count > 0)
                            player.AutoMove(ref pathNodes);
                    }
                }
            }
        }	    
    }
}