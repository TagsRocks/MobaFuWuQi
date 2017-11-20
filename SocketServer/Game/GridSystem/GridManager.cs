using System.Collections;
using System.Collections.Generic;
using EpPathFinding.cs;
using System;
using SimpleJSON;
using System.Numerics;
using System.IO;
using MyLib;

/// <summary>
/// 地图网格管理器
/// 加载Json地图数据
/// 寻路
/// </summary>
public class GridManager : MyLib.Component {
    public GridManager()
    {
    }
    private bool loadMapYet = false;
    public void InitMap()
    {
        if (loadMapYet)
        {
            return;
        }
        loadMapYet = true;
        using (var f = new StreamReader("ConfigData/MapSourceConfig.json"))
        {
            var con = f.ReadToEnd();
            this.LoadMap(con);
        }
    }

    public void LoadMap(string jsonData)
    {
        MyLib.LogHelper.Log("Grid", "LoadMap");
        var data = SimpleJSON.JSON.Parse(jsonData);
        width = System.Convert.ToInt32(data[0]["width"].AsFloat);
        height = Convert.ToInt32(data[0]["height"].AsFloat);

        var cenArray = data[0]["center"].AsObject;
        var cx = cenArray["x"].AsFloat;
        var cy = cenArray["y"].AsFloat;
        var cz = cenArray["z"].AsFloat;
        
        center = new Vector3(cx, cy, cz);
        nodeSize = data[0]["nodeSize"].AsFloat;

        var mapData = data[0]["mapdata"].AsArray;
        grids = new StaticGrid(width, height);
        var i = 0;
        foreach(SimpleJSON.JSONNode d in mapData)
        {
            var v = d.AsInt;
            if(v == 0)
            {
                var r = i / width;
                var c = i % width;
                grids.SetWalkableAt(c, r, true);
            }
            i++;
        }

        var mh = data[0]["mapHeight"].AsArray;
        mapHeight = new List<float>(width * height);
        foreach(JSONNode d in mh)
        {
            mapHeight.Add(d.AsFloat);
        }

        jpParam = new JumpPointParam(grids);
    }

    public Vector2 mapPosToGrid(Vector3 pos)
    {
        var off = pos - center;
        var left = off.X + nodeSize * width / 2;
        var bottom = off.Z + nodeSize * width / 2;
        var gx = (int)(left / nodeSize);
        var gy = (int)(bottom / nodeSize);
        gx = MathUtil.Clamp(gx, 0, width-1);
        gy = MathUtil.Clamp(gy, 0, height - 1);
        return new Vector2(gx, gy);
    }

    public List<GridPos> FindPath(Vector2 p1, Vector2 p2)
    {
        jpParam.Reset(
            new GridPos(Convert.ToInt32(p1.X), Convert.ToInt32(p1.Y)),
            new GridPos(Convert.ToInt32(p2.X), Convert.ToInt32(p2.Y))
            );
        /*
            true,
            DiagonalMovement.Always,
            HeuristicMode.MANHATTAN);
         */

        MyLib.LogHelper.Log("GridManager", "FindPath:"+p1+":"+p2);
        var path = JumpPointFinder.FindPath(jpParam);
        return path;
    }

    public Vector3 gridToMapPos(Vector2 grid)
    {
        var gx = MathUtil.Clamp((int)(grid.X), 0, width - 1);
        var gy = MathUtil.Clamp((int)(grid.Y), 0, height - 1);
        var gid = (int)(gx + gy * width);
        var h = mapHeight[gid];
        var px = gx*nodeSize -nodeSize * width / 2;
        var py = gy*nodeSize -nodeSize * height / 2;
        var pos = new Vector3(px, 0, py) + center;
        pos.Y = h;
        return pos;
    }

    private JumpPointParam jpParam;
    private BaseGrid grids;
    private int width, height;
    private Vector3 center;
    private float nodeSize;
    private List<float> mapHeight;


    #region physic
    /// <summary>
    /// 得到点Pos 最近的可以走的位置 
    /// 所在网格可以行走
    /// 不能行走则临近最近网格接触点
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector3 FindNearestWalkableGridPos(Vector3 pos)
    {
        //假设玩家是一个nodeSize 半径的球
        //和周围的网格进行碰撞计算
        //得到实际的位置
        //1:位置得到Circle
        //2: 第一遍碰撞 修正位置
        //3：迭代第二次碰撞 修正位置 迭代多次
        var gPos = mapPosToGridFloat(pos);
        var allGrids = BroadColGrids(gPos);

        var count = 0;
        while (count < iterNum)
        {
            Vector2 firstGrid;
            //只迭代1次
            var col = FindFirstColGrid(gPos, allGrids, out firstGrid);
            if (col)
            {
                var newGPos = FixPos(gPos, firstGrid);
                //return gridToMapPosFloat(newGPos);
                gPos = newGPos;
            }
            else
            {
                //TODO:可能需要插值
                break;
            }
        }

        var mp2 = gridToMapPosFloat(gPos);
        return mp2;
    }

    
    public bool GetWalkable(Vector2 p)
    {
        return grids.IsWalkableAt((int)p.X, (int)p.Y);
    }

    /// <summary>
    /// 寻找第一个碰撞的网格
    /// </summary>
    /// <param name="gPos"></param>
    /// <param name="allGrids"></param>
    /// <param name="firstPos"></param>
    /// <returns></returns>
    private bool FindFirstColGrid(Vector2 gPos, List<Vector2> allGrids, out Vector2 firstPos)
    {
        //网格空间坐标
        var radius = 1 / 2.0f;
        var gridRadius = 1 / 2.0f;
        var dist = (radius + gridRadius);
        dist *= dist;

        //radius *= radius;

        foreach (var n in allGrids)
        {
            var dx = gPos.X - n.X;
            var dy = gPos.Y - n.Y;
            var newV2 = new Vector2(dx, dy);
            if (newV2.LengthSquared() < dist)
            {
                firstPos = n;
                return true;
            }
        }
        firstPos = Vector2.Zero;
        return false;
    }


    private const float EPSILON = 0.01f;
    private const int iterNum = 1;

    /// <summary>
    /// 根据第一个碰撞网格修正位置
    /// </summary>
    /// <param name="gPos"></param>
    /// <param name="firstGrid"></param>
    /// <returns></returns>
    private Vector2 FixPos(Vector2 gPos, Vector2 firstGrid)
    {
        var radius = 1 / 2.0f;
        var gridRadius = 1 / 2.0f;

        var dx = gPos.X - firstGrid.X;
        var dy = gPos.Y - firstGrid.Y;
        var dirV = new Vector2(dx, dy);
        var offPos = Vector2.Normalize(dirV) * (radius + gridRadius + EPSILON);
        return firstGrid + offPos;
    }
    
    /// <summary>
    /// 查找附近网格
    /// </summary>
    /// <param name="gPos"></param>
    /// <returns></returns>
    private List<Vector2> BroadColGrids(Vector2 gPos)
    {
        var igx = (int)gPos.X;
        var igy = (int)gPos.Y;
        var walk = grids.IsWalkableAt(igx, igy);
        var neibors = new List<Vector2>();
        if (!walk)
        {
            neibors.Add(new Vector2(igx, igy));
        }

        for (var i = igx - 1; i <= (igx + 1); i++)
        {
            for (var j = igy - 1; j <= (igy + 1); j++)
            {
                walk = grids.IsWalkableAt(i, j);
                if (!walk)
                {
                    neibors.Add(new Vector2(i, j));
                }
            }
        }
        return neibors;
    }

    /// <summary>
    /// 网格坐标带小数部分
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private Vector2 mapPosToGridFloat(Vector3 pos)
    {
        var off = pos - center;
        var left = off.X + nodeSize * width / 2;
        var bottom = off.Z + nodeSize * width / 2;
        var gx = (left / nodeSize);
        var gy = (bottom / nodeSize);
        gx = MathUtil.Clamp(gx, 0, width - 1);
        gy = MathUtil.Clamp(gy, 0, height - 1);
        return new Vector2(gx, gy);
    }

    /// <summary>
    /// 浮点网格坐标转化为实际Unity坐标
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    private Vector3 gridToMapPosFloat(Vector2 grid)
    {
        var gx = MathUtil.Clamp(grid.X, 0, width - 1);
        var gy = MathUtil.Clamp(grid.Y, 0, height - 1);

        //TODO:高度可能需要插值
        var igx = (int)gx;
        var igy = (int)gy;
        var gid = (int)(igx + igy * width);
        var h = mapHeight[gid];


        var px = gx * nodeSize - nodeSize * width / 2;
        var py = gy * nodeSize - nodeSize * height / 2;
        var pos = new Vector3(px, 0, py) + center;
        pos.Y = h;
        return pos;

    }
    #endregion
}
