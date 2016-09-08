using UnityEngine;
using System.Collections;
using Kino.TileMap;

namespace Kino.TileMap.Test
{
    [RequireComponent(typeof(Animator))]
    public class Actor : MonoBehaviour, IAttackAble, IDamageAble {
        public int attackPower;
        public int hp;
        public int maxHP;

        private Animator anim;
        private Direction dir = Direction.Bottom;
        private bool dead = false;
        private bool damageEffecting = false;

        #region Interface Property
        public int AttackPower {
            get {return attackPower;}
        }
        #endregion

        #region Property
        public bool Dead {
            get {return dead;}
        }
        #endregion

        #region Interface Functions
        public void OnDamage(int damage, IAttackAble by) {
            this.hp -= damage;

            if (this.hp <= 0) {
                if (!this.dead)
                    Die();
            }
            else {
                if (!this.damageEffecting)
                    StartCoroutine(this.DamagedEffectPocess());
            }
        }
        #endregion

        public void Revive() {
            if (!this.dead)
                return;

            this.hp = this.maxHP;
            this.dead = false;

            anim.SetBool("Dead", false);
            SetDirection(Direction.Bottom);
        }

        IEnumerator DamagedEffectPocess() {
            const float deadTerm = 0.2f;
            float beginTimer = Time.time;

            SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
            Color originColor = renderer.color;
            renderer.color = Color.red;

            while (true) {
                float deltaTimer = Time.time - beginTimer;

                if (deltaTimer >= deadTerm)
                    break;

                yield return null;
            }

            renderer.color = originColor;

            this.damageEffecting = false;

            yield return null;
        }

        void Die() {
            this.dead = true;

            anim.SetBool("Dead", true);
        }

        void Awake()
        {
            anim = GetComponent<Animator>();
        }

    	// Use this for initialization
    	void Start () {
            SetDirection(Direction.Bottom);
    	}   

        void SetDirection(Direction dir)
        {
            this.dir = dir;
            anim.SetInteger("LTRBDir", (int)dir);
        }
    }
}