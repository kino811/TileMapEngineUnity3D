using UnityEngine;
using System.Collections;

namespace Kino.TileMap.Test {
    [RequireComponent(typeof(TileMapObjectGroup))]
    public class EnterToOtherMap : MonoBehaviour {
        public TileMapRoot map;
        public TilePos startPos;

        public void EnterBy(Actor actor) {
            if (this.map == null)
                return;

            ActorController actorCtrl = actor.GetComponent<ActorController>();
            if (actorCtrl == null)
                return;

            this.map.gameObject.SetActive(true);
            this.map.InitTileMapEngine();

            actorCtrl.EnterToOtherMap(this.startPos);
        }
    }
}