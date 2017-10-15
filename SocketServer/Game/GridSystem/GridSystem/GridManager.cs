using System.Collections;
using System.Collections.Generic;
using EpPathFinding.cs;
using System;
using SimpleJSON;
using System.Numerics;

/// <summary>
/// 地图网格管理器
/// 加载Json地图数据
/// 寻路
/// </summary>
public class GridManager : MyLib.Component {
    public GridManager()
    {
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
        var jpParam = new JumpPointParam(grids, new GridPos(Convert.ToInt32(p1.X), Convert.ToInt32(p1.Y)), new GridPos(Convert.ToInt32(p2.X), Convert.ToInt32(p2.Y)));
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


    private BaseGrid grids;
    private int width, height;
    private Vector3 center;
    private float nodeSize;
    private List<float> mapHeight;
	
}
