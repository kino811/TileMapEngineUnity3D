using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kino.TileMap;
using Kino.Util;

namespace Kino.TileMap.Test
{
    [RequireComponent(typeof(Actor))]
    public class ActorController : MonoBehaviour {

        public AudioClip warpSound;

        private Animator anim;
        private bool moving = false;
        private Direction LTRBDir = Direction.None;
        private Direction keyLTRB = Direction.None;
        private TilePos curTilePos = new TilePos(0, 0);
        private Vector3 curTileWorldPos;
        private TilePos targetTilePos = new TilePos(0, 0);
        private Vector3 targetTileWorldPos;
        private bool autoMoving = false;
        private TilePos autoMovingDestTilePos = new TilePos(0, 0);
        private List<SquareTileMapNode> autoMovingPathNodes;
        private Rigidbody2D rig2D;
        private bool resetKeyboardWhenStopMove = false;
        private bool autoMovingPathChanged = false;
        private Actor actor;
        private bool warpping = false;

        public bool AutoMoving {
            get {return autoMoving;}
        }

        public TilePos CurTilePos {
            get {return curTilePos;}
        }

        public TilePos TargetTilePos {
            get {return targetTilePos;}
        }

        void Awake()
        {
            actor = GetComponent<Actor>();
            anim = GetComponent<Animator>();
            rig2D = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            SetPos(new TilePos(0, 0));
        }
    	
    	// Update is called once per frame
    	void Update () {
            if (Input.GetKey(KeyCode.A))
            {
                if (!resetKeyboardWhenStopMove)
                {
                    Move(Direction.Left);
                }
            }
            else if (Input.GetKey(KeyCode.W))
            {
                if (!resetKeyboardWhenStopMove)
                    Move(Direction.Top);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                if (!resetKeyboardWhenStopMove)
                    Move(Direction.Right);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                if (!resetKeyboardWhenStopMove)
                    Move(Direction.Bottom);
            }
            else 
            {
                keyLTRB = Direction.None;
                resetKeyboardWhenStopMove = false;
            }	    

            if (actor.Dead && Input.GetKey(KeyCode.Z))
            {
                actor.Revive();
            }
    	}

        void OnTriggerEnter2D(Collider2D other)
        {
            TileMapObject objUnit = other.GetComponent<TileMapObject>();
            if (objUnit) {
                IDamageAble damageAbleObj = objUnit.GetComponentInParent<IDamageAble>();
                if (damageAbleObj != null) {
                    IAttackAble attackAble = actor as IAttackAble;
                    if (damageAbleObj != null && attackAble != null) {
                        damageAbleObj.OnDamage(attackAble.AttackPower, attackAble);
                    }

                    StopMove();
                }

                if (!this.warpping) {
                    Portal portal = objUnit.GetComponentInParent<Portal>();
                    if (portal) {
                        EnterPortal(portal);
                    }
                }

                if (!this.warpping) {
                    EnterToOtherMap[] enterToOtherMapList = objUnit.GetComponentsInParent<EnterToOtherMap>();
                    if (enterToOtherMapList != null && enterToOtherMapList.Length > 0) {
                        EnterToOtherMap enterToOtherMap = enterToOtherMapList[0];
                        enterToOtherMap.EnterBy(this.actor);
                    }
                }
            }
        }

        public void EnterToOtherMap(TilePos startPos) {
            StopMove();
            SetPos(startPos);
            SetDir(Direction.Bottom);

            if (this.warpSound) {
                AudioSource.PlayClipAtPoint(warpSound, Camera.main.transform.position);
            }

            this.warpping = true;

            StartCoroutine(CoroutineUtil.DelayRoutine(1.0f,
                delegate() {
                    this.warpping = false;
                }
            ));
        }

        void EnterPortal(Portal portal) {
            if (portal == null)
                return;

            Portal linkedPortal = portal.LinkedPortal;
            if (linkedPortal == null)
                return;

            ITileMapObject tileMapObj = linkedPortal.GetComponent<ITileMapObject>();
            if (tileMapObj == null)
                return;

            StopMove();
            SetPos(tileMapObj.TilePos);
            SetDir(Direction.Bottom);

            if (this.warpSound) {
                AudioSource.PlayClipAtPoint(warpSound, Camera.main.transform.position);
            }

            this.warpping = true;

            StartCoroutine(CoroutineUtil.DelayRoutine(1.0f,
                delegate() {
                    this.warpping = false;
                }
            ));
        }

        void StopMove()
        {
            //Debug.Log(string.Format("StopMove:{0},{1}", curTilePos.x, curTilePos.y));
            moving = false;
            SetPos(curTilePos);
            resetKeyboardWhenStopMove = true;
            autoMoving = false;
        }

        public void AutoMove(ref List<SquareTileMapNode> pathNodes)
        {
            if (actor.Dead)
                return;

            if (autoMoving) {
                autoMovingPathChanged = true;
            }

            autoMoving = true;
            autoMovingPathNodes = pathNodes;

            if (!autoMovingPathChanged)
                StartCoroutine(AutoMovePathNodesProcess());
        }

        IEnumerator AutoMovePathNodesProcess()
        {
            autoMovingPathChanged = false;
            SquareTileMapNode curNode = autoMovingPathNodes[0];

            while (curNode != null)
            {
                if (autoMovingPathChanged) {
                    if (autoMovingPathNodes.Count == 0)
                        yield break;

                    curNode = autoMovingPathNodes[0];
                    autoMovingPathChanged = false;
                }

                if (!autoMoving)
                    yield break;

                if (!moving)
                {
                    if (curTilePos.x != curNode.TilePosX ||
                        curTilePos.y != curNode.TilePosY)
                    {
                        Direction LTRBDir = Direction.None;

                        if (curTilePos.x != curNode.TilePosX)
                        {
                            LTRBDir = (curNode.TilePosX > curTilePos.x) ? Direction.Right : Direction.Left;
                        }
                        else if (curTilePos.y != curNode.TilePosY)
                        {
                            LTRBDir = (curNode.TilePosY > curTilePos.y) ? Direction.Top : Direction.Bottom;
                        }

                        Move(LTRBDir);
                    }

                    autoMovingPathNodes.RemoveAt(0);
                    if (autoMovingPathNodes.Count > 0)
                        curNode = autoMovingPathNodes[0];
                    else
                        curNode = null;
                }

                yield return null;
            }

            autoMoving = false;

            yield return null;
        }

        public void SetPos(TilePos tilePos)
        {
            this.curTilePos = tilePos;

            SquareTileMapNode node = TileMapEngine.Instance.GetTileNode(tilePos);
            if (node) {
                curTileWorldPos = node.WorldPosition;
                curTileWorldPos.z = 0.0f;
            }

            transform.position = curTileWorldPos;
        }

        void SetDir(Direction LTRBDir)
        {
            if (this.LTRBDir != LTRBDir)
            {
                anim.SetInteger("LTRBDir", (int)LTRBDir);
                this.LTRBDir = LTRBDir;
            }
        }

        void Move(Direction LTRBDir)
        {
            if (actor.Dead)
                return;

            this.keyLTRB = LTRBDir;

            if (!moving) 
            {
                SetDir(LTRBDir);

                TilePos targetTilePos = curTilePos + LTRBDir;
                SquareTileMapNode node = TileMapEngine.Instance.GetTileNode(targetTilePos);
                if (node) {
                    if (!node.Invalid) {
                        this.targetTilePos = targetTilePos;
                        this.targetTileWorldPos = node.WorldPosition;

                        StartCoroutine(MoveProcess());
                    }
                }
            }
        }

        IEnumerator MoveProcess()
        {
            //Debug.Log("start MoveProcess");
            moving = true;
            float beginTime = Time.time;
            float speed = 2.0f;

            while (true)
            {
                while (transform.position != targetTileWorldPos)
                {
                    if (!moving)
                    {
                        yield break;
                    }

                    transform.position = Vector3.Lerp(curTileWorldPos, targetTileWorldPos, (Time.time - beginTime) * speed);
                    //Debug.Log("MoveProcess setpos");

                    yield return null;
                }

                if (!moving)
                {
                    yield break;
                }

                if (keyLTRB != Direction.None)
                {
                    this.curTilePos = this.targetTilePos;
                    this.curTileWorldPos = this.transform.position;

                    // next move exist.
                    SetDir(keyLTRB);

                    TilePos targetTilePos = curTilePos + LTRBDir;
                    SquareTileMapNode node = TileMapEngine.Instance.GetTileNode(targetTilePos);
                    if (node) {
                        if (!node.Invalid) {
                            this.targetTilePos = targetTilePos;
                            this.targetTileWorldPos = node.WorldPosition;
                        }
                    }

                    beginTime = Time.time;
                }
                else 
                {
                    break;
                }

                yield return null;
            }

            moving = false;
            this.curTilePos = this.targetTilePos;
            this.curTileWorldPos = this.transform.position;

            yield return null;
        }
    }
}