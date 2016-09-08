using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kino.TileMap.Test {
    public class TileObject : MonoBehaviour, IDamageAble {
        public int hp;
        public int maxHP;

        private bool dead = false;
        private bool damageEffecting = false;

        #region Interface
        public void OnDamage(int damage, IAttackAble by) {
            this.hp -= damage;

            if (this.hp <= 0) 
                Dead();
            else {
                if (!this.damageEffecting)
                    StartCoroutine(this.DamagedEffectPocess());
            }
        }
        #endregion

        IEnumerator DamagedEffectPocess() {
            const float deadTerm = 0.2f;
            float beginTimer = Time.time;

            Dictionary<SpriteRenderer, Color> originColorDic = new Dictionary<SpriteRenderer, Color>();
            foreach (SpriteRenderer renderer in gameObject.GetComponentsInChildren<SpriteRenderer>()) {                
                originColorDic[renderer] = renderer.color;
                renderer.color = Color.red;
            }

            while (true) {
                float deltaTimer = Time.time - beginTimer;

                if (deltaTimer >= deadTerm)
                    break;

                yield return null;
            }

            foreach (var pair in originColorDic) {
                SpriteRenderer renderer = (SpriteRenderer)pair.Key;
                renderer.color = (Color)pair.Value;
            }

            this.damageEffecting = false;

            yield return null;
        }

        void Dead() {
            if (!dead) {
                dead = true;
                StartCoroutine(this.DeadProcess());
            }
        }
        IEnumerator DeadProcess() {
            const float deadTerm = 0.5f;
            float beginTimer = Time.time;

            while (true) {
                float deltaTimer = Time.time - beginTimer;
                float alphaValue = 1.0f - deltaTimer / deadTerm;

                SpriteRenderer[] renderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer renderer in renderers) {
                    Color updatingColor = renderer.color;
                    updatingColor.a = alphaValue;

                    renderer.color = updatingColor;
                }

                if (deltaTimer >= deadTerm)
                    break;

                yield return null;
            }

            DestroyImmediate(gameObject);

            yield return null;
        }
    }
}