using UnityEngine;
using System.Collections;

namespace Kino.TileMap.Test {
    [RequireComponent(typeof(SquareTileMapNode))]
    public class AttackableGroundTile : MonoBehaviour, IAttackAble {
        public int attackPower;

        #region Interface
        public int AttackPower {
            get {return attackPower;}
        }
        #endregion

        void OnTriggerEnter2D(Collider2D other) {
            IDamageAble damageAble = other.GetComponent<IDamageAble>();
            if (damageAble != null) {
                damageAble.OnDamage(this.AttackPower, this);
            }
        }
    }
}