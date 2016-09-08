using UnityEngine;
using System.Collections;

namespace Kino.TileMap.Test {
    public interface IAttackAble {
        int AttackPower {get;}
    }

    public interface IDamageAble {
        void OnDamage(int damage, IAttackAble by);
    }

    public class BattleSystem : MonoBehaviour {
        public TileMapRoot tileMap;

    	// Use this for initialization
    	void Start () {
            if (this.tileMap == null)
                this.tileMap = FindObjectOfType<TileMapRoot>();

            if (this.tileMap)
                this.tileMap.InitTileMapEngine();

            TileMapObjectGroup[] objs = FindObjectsOfType<TileMapObjectGroup>();
            foreach (TileMapObjectGroup obj in objs) {
                obj.AddOnObjectToTileNodesByCurPos();
            }
    	}
    }
}