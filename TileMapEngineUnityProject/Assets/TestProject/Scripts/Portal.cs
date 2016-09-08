using UnityEngine;
using System.Collections;

namespace Kino.TileMap.Test {
    [RequireComponent(typeof(TileMapObjectGroup))]
    public class Portal : MonoBehaviour {
        public Portal linkedPortal;

        public Portal LinkedPortal {
            get {return linkedPortal;}
        }
    }
}