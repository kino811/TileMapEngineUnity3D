using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kino.TileMap
{
    using Kino.TileMap.PathFind;
    using Kino.TileMap.PathFind.Algorithm;
    
    public class TileMapEngine : ScriptableObject
    {
        #region Inner Classes

        class PathFinder
        {            
            private IAlgorithm algorithm;

            public PathFinder(IAlgorithm algorithm)
            {
                this.algorithm = algorithm;
            }

            public bool Invalid(ITileMapNode node)
            {
                return algorithm.Invalid(node, false);
            }

            public List<T> Calculate<T>(T start, T goal, bool checkObj, bool goalCheckObj) where T : ITileMapNode
            {
                return algorithm.Calculate(start, goal, checkObj, goalCheckObj);
            }
        }

        #endregion

        #region Static Fields

        private static TileMapEngine instance;

        #endregion

        #region Fields

        private PathFinder pathFinder;
        private bool screen2D = false;
        private int tileWidthCount;
        private int tileHeightCount;
        //private Dictionary<int, SquareTileMapNode> nodeMap = new Dictionary<int, SquareTileMapNode>();
        private Dictionary<TilePos, SquareTileMapNode> nodeMap = new Dictionary<TilePos, SquareTileMapNode>();

        #endregion

        #region Static Properties

        public static TileMapEngine Instance {
            get {
                if (TileMapEngine.instance == null) {
                    TileMapEngine.instance = new TileMapEngine();
                }

                return TileMapEngine.instance;
            }
        }

        #endregion

        #region Properties

        public bool Screen2D {
            get {return screen2D;}
        }

        public TileMapRoot CurMap {
            get {
                if (this.nodeMap.Count > 0) {
                    SquareTileMapNode node = this.nodeMap.GetEnumerator().Current.Value;
                    return node.GetComponentInParent<TileMapRoot>();
                }

                return null;
            }
        }

        #endregion

        #region Constructors

        public TileMapEngine()
        {
            pathFinder = new PathFinder(new AStartAlogrithm());
        }

        #endregion

        #region Methods

        public void Init(bool screen2D, int tileWidthCount, int tileHeightCount, SquareTileMapNode[] nodes)
        {
            this.screen2D = screen2D;
            this.tileWidthCount = tileWidthCount;
            this.tileHeightCount = tileHeightCount;

            nodeMap.Clear();
            foreach (SquareTileMapNode node in nodes)
            {
                nodeMap[node.TilePos] = node;
            }

            MakeConnectionEachOtherNodes(nodeMap);
        }

        public SquareTileMapNode GetTileNode(TilePos tilePos)
        {
            if (!nodeMap.ContainsKey(tilePos)) {
                return null;
            }

            return nodeMap[tilePos];
        }

        public bool Invalid(ITileMapNode node)
        {
            return pathFinder.Invalid(node);
        }

        //
        // @return : if cannot find path, return null or list that size is 0.
        //
        public List<T> Calculate<T>(T start, T goal, bool checkObj = false, bool goalCheckObj = false) where T : ITileMapNode
        {
            if (start == null || goal == null)
                return null;

            return pathFinder.Calculate(start, goal, checkObj, goalCheckObj);
        }

        void MakeConnectionEachOtherNodes(Dictionary<TilePos, SquareTileMapNode> nodeMap)
        {
            int nodeIndex = 0;

            for (int y = 0; y < tileHeightCount; ++ y)
            {
                for (int x = 0; x < tileWidthCount; ++ x, ++ nodeIndex)
                {
                    TilePos nodePos = new TilePos(x, y);
                    SquareTileMapNode node = nodeMap[nodePos];
                    node.ClearConnectionNode();

                    if (TileMapEngine.Instance.Invalid(node)) {
                        continue;
                    }

                    List<TilePos> adjacentNodeIndexes = new List<TilePos>();

                    if (x != 0) { //left node
                        adjacentNodeIndexes.Add(nodePos - new TilePos(1, 0));
                    }

                    if (y < tileHeightCount - 1) { // top node
                        adjacentNodeIndexes.Add(nodePos + new TilePos(0, 1));
                    }

                    if (x < tileWidthCount - 1) { // rightn node
                        adjacentNodeIndexes.Add(nodePos + new TilePos(1, 0));
                    }

                    if (y != 0) { // bottom node
                        adjacentNodeIndexes.Add(nodePos - new TilePos(0, 1));
                    }

                    foreach (TilePos adjacentNodeIndex in adjacentNodeIndexes)
                    {
                        SquareTileMapNode adjacentNode = null;
                        nodeMap.TryGetValue(adjacentNodeIndex, out adjacentNode);

                        if (adjacentNode == null) {
                            Debug.LogError(string.Format("invalid nodePos={0}_{1}", adjacentNodeIndex.x, adjacentNodeIndex.y));
                            continue;
                        }

                        if (TileMapEngine.Instance.Invalid(adjacentNode)) {
                            continue;
                        }

                        node.AddConnectionNode(adjacentNode);
                    }
                }
            }
        }

        #endregion
    }
}