using UnityEngine;
using System.Collections;

namespace Kino.TileMap.Test
{
    public class BattleCamera : MonoBehaviour {

        public ActorController player;

    	// Use this for initialization
    	void Start () {
            if (this.player == null)
                this.player = FindObjectOfType<ActorController>();

    	    StartCoroutine(FollowPlayer());
    	}

        IEnumerator FollowPlayer()
        {
            while (true)
            {
                if (this.player) {
                    transform.position = Vector3.Lerp(
                        transform.position, 
                        this.player.gameObject.transform.position + Vector3.back, 
                        Time.deltaTime);
                }

                yield return null;
            }

            yield return null;
        }
    }
}