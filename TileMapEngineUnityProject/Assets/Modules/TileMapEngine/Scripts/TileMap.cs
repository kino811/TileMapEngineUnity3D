using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kino
{
    namespace TileMap
    {
        public enum Direction
        {
			None = -1,
            Left = 0,
            Top,
            Right,
            Bottom,
        }

        public interface ITileMapNode
        {
            List<ITileMapNode> Connections {get;}
            Vector3 WorldPosition {get;}
            bool Invalid {get;}
            int NodeID {get;}
            bool HasTileObj();
        }

		public enum MapObjType
		{
			Construction,
			Player,
		}

        public interface ITileMapObject
        {
            TilePos TilePos {get;}
			MapObjType Type {get;}
        }

        [System.Serializable]
        public struct TilePos
        {
            public int x;
            public int y;

            public TilePos(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public static TilePos operator + (TilePos lhs, TilePos rhs) {
                return new TilePos(lhs.x + rhs.x, lhs.y + rhs.y);
            }

            public static TilePos operator - (TilePos lhs, TilePos rhs) {
                return new TilePos(lhs.x - rhs.x, lhs.y - rhs.y);
            }

            public static TilePos operator + (TilePos pos, Direction dir) {
                TilePos resultPos = pos;

                switch (dir) {
                    case Direction.Left: {
                        -- resultPos.x;
                    } break;
                    case Direction.Top: {
                        ++ resultPos.y;
                    } break;
                    case Direction.Right: {
                        ++ resultPos.x;
                    } break;
                    case Direction.Bottom: {
                        -- resultPos.y;
                    } break;
                }

                return resultPos;
            }

            public static TilePos operator - (TilePos pos, Direction dir) {
                TilePos resultPos = pos;

                switch (dir) {
                    case Direction.Left: {
                        ++ resultPos.x;
                    } break;
                    case Direction.Top: {
                        -- resultPos.y;
                    } break;
                    case Direction.Right: {
                        -- resultPos.x;
                    } break;
                    case Direction.Bottom: {
                        ++ resultPos.y;
                    } break;
                }

                return resultPos;
            }

            public static bool operator == (TilePos lhs, TilePos rhs) {
                return lhs.x == rhs.x && lhs.y == rhs.y;
            }

            public static bool operator != (TilePos lhs, TilePos rhs) {
                return ! (lhs == rhs);
            }
        }

        [System.Serializable]
        public struct TileMapSize
        {
            public uint width;
            public uint height;

            public TileMapSize(int width, int height)
            {
                this.width = (uint)System.Math.Abs(width);
                this.height = (uint)System.Math.Abs(height);
            }

            public TileMapSize(uint width, uint height)
            {
                this.width = width;
                this.height = height;
            }
        }

        namespace PathFind
        {            
            namespace Algorithm
            {
                interface IAlgorithm
                {
                    List<T> Calculate<T>(T start, T goal, bool checkObj, bool goalCheckObj) where T : ITileMapNode;
                    bool Invalid(ITileMapNode node, bool checkObj);
                }

                public class AStartAlogrithm : IAlgorithm
                { 
                    #region Methods

                    public List<T> Calculate<T>(T start, T goal, bool checkObj, bool goalCheckObj) where T : ITileMapNode
                    {
                        List<T> closedset = new List<T>();
                        List<T> openset = new List<T>();
                        openset.Add(start);
                        Dictionary<T, T> came_from = new Dictionary<T, T>();

                        Dictionary<T, float> g_score = new Dictionary<T, float>();
                        g_score[start] = 0.0f;

                        Dictionary<T, float> h_score = new Dictionary<T, float>();
                        h_score[start] = HeuristicCostEstimate(start, goal, false, goalCheckObj);

                        Dictionary<T, float> f_score = new Dictionary<T, float>();
                        f_score[start] = h_score[start];

                        while (openset.Count != 0)
                        {
                            T x = LowestScore(openset, f_score);

                            bool xCheckObj = checkObj;
                            if (Object.ReferenceEquals(x, goal))
                                xCheckObj = goalCheckObj;

                            if (x.Equals(goal))
                            {
                                List<T> result = new List<T>();
                                ReconstructPath(came_from, x, ref result);
                                return result;
                            }

                            openset.Remove(x);
                            closedset.Add(x);

                            foreach (T y in x.Connections)
                            {
                                bool yCheckObj = checkObj;
                                if (Object.ReferenceEquals(y, goal))
                                    yCheckObj = goalCheckObj;

                                if (Invalid(y, yCheckObj) || closedset.Contains(y))
                                    continue;

                                float tentative_g_score = g_score[x] + Distance(x, y, xCheckObj, yCheckObj);
                                bool tentative_is_better = false;

                                if (!openset.Contains(y))
                                {
                                    openset.Add(y);
                                    tentative_is_better = true;
                                }
                                else if (tentative_g_score < g_score[y])
                                    tentative_is_better = true;

                                if (tentative_is_better)
                                {
                                    came_from[y] = x;
                                    g_score[y] = tentative_g_score;
                                    h_score[y] = HeuristicCostEstimate(y, goal, yCheckObj, goalCheckObj);
                                    f_score[y] = g_score[y] + h_score[y];
                                }
                            }
                        }

                        return null;
                    }

                    public bool Invalid(ITileMapNode node, bool checkObj)
                    {
                        if (node == null || node.Invalid)
                            return true;

                        if (checkObj && node.HasTileObj())
                            return true;

                        return false;
                    }
                        
                    float Distance(ITileMapNode start, ITileMapNode goal, bool checkObj, bool goalCheckObj)
                    {
                        if (Invalid(start, checkObj) || Invalid(goal, goalCheckObj))
                            return float.MaxValue;

                        return Vector3.Distance(start.WorldPosition, goal.WorldPosition);
                    }

                    T LowestScore<T>(List<T> openset, Dictionary<T, float> scores) where T : ITileMapNode
                    {
                        int index = 0;
                        float lowScore = float.MaxValue;

                        for (int i = 0; i < openset.Count; ++ i)
                        {
                            if (scores[openset[i]] > lowScore)
                                continue;

                            index = i;
                            lowScore = scores[openset[i]];
                        }

                        return openset[index];
                    }

                    float HeuristicCostEstimate(ITileMapNode start, ITileMapNode goal, bool checkObj, bool goalCheckObj)
                    {
                        return Distance(start, goal, checkObj, goalCheckObj);
                    }

                    void ReconstructPath<T>(Dictionary<T, T> came_from, T current_node, ref List<T> result) where T : ITileMapNode
                    {
                        if (came_from.ContainsKey(current_node))
                        {
                            ReconstructPath(came_from, came_from[current_node], ref result);
                            result.Add(current_node);
                            return;
                        }

                        result.Add(current_node);
                    }

                    #endregion
                }
            }
        }
    }
}