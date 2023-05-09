using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;
//using System.Drawing;
//using System.Diagnostics;

public class AStaralgo : MonoBehaviour
{
    public const int field_size = 40;
    public GameObject obj;
    //public const GameObject goal;
    public  Vector3 sp = new Vector3(0, 0, 0);//start.transform.position;

    public  Vector3 gp = new Vector3(10, 0, 10);//goal.transform.position;

    public int[,,] wall = new int[field_size + 1, field_size + 1, field_size + 1];

    private void init()     //配列の初期化(道空間なのでどこに壁や障害物があるかはわからないのでwallはすべて0にする)
    {
        for (int i = 0; i <= field_size; i++)
        {
            for (int j = 0; j <= field_size; j++)
            {
                for (int k = 0; k <= field_size; k++)
                {
                    wall[i, j, k] = 0;
                }
            }
        }
    }

    //radarの問題点見かけ上幅2の経路4から6.5->6ならok4から6.4->5はだめ
    //6.4は壁の際が5.9->5として計算される。
    //4.5->4から6.5->6もだめ？？
    private void radar(Vector3 origin)
    {

        for (int i = 0; i < 360; i++)    //360度回転し、周りの遮蔽物を記録
        {
            transform.Rotate(0, 1, 0);    //1度ずつ回転
            Vector3 direction = transform.forward;
            Ray ray = new Ray(origin, direction);   //Ray型:光線を示す。origin:現在位置、direction:光線を出す方向
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))   //障害物が当たったらhitに情報が格納され、Raycast関数は1を返す
            {
                //string name = hit.collider.gameObject.name;
                // Debug.Log(name);
                Vector3 hit_vec = hit.point;
                int hit_int_x = (int)hit_vec.x;     //当たった座標をint型にキャスト
                int hit_int_y = (int)hit_vec.y;
                int hit_int_z = (int)hit_vec.z;
                // Debug.Log(hit_int_z);
                wall[hit_int_x, 0, hit_int_z] = 1;      //wallを1にする
                // Debug.Log(hit_int_x+","+hit_int_z);
                //Debug.DrawRay(origin, direction, Color.red, Vector3.Distance(origin, hit_vec)); //レーザーを可視化
            }
        }
    }
    //ワールド内の隣接する八マスを格納
    public List<Vector3> GetAdjacences(Vector3 v)
    {
        var adjacences = new List<Vector3>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;
                //if (dx*dz == -1 || dx*dz == 1 || (dx == 0 && dz == 0)) continue;
                Vector3 cell = new Vector3(v.x + dx, v.y, v.z + dz);
                if (cell.x < 0 || cell.x > field_size || cell.z < 0 || cell.z > field_size) continue;
                adjacences.Add(cell);
            }
        }
        return adjacences;
    }

    private class AStarInfo
    {
        public Vector3 cell;
        public AStarInfo previous;
        public float step;
        public float distance;
        public float Weight { get { return step + distance; } }
    }

    public List<Vector3> GetShortestWay(Vector3 StartVec, Vector3 GoalVec)
    {
        List<Vector3> res = new List<Vector3>();
        List<Vector3> passedCells = new List<Vector3>();
        List<AStarInfo> recentTargets = new List<AStarInfo>();
        passedCells.Add(StartVec);
        recentTargets.Add(GetAStarInfo(StartVec, GoalVec, null));
        AStarInfo goalInfo = null;

        while (true)
        {
            // recentTargetsのうちweightが最も低いものを計算対象とする
            AStarInfo currentTarget = recentTargets
                .OrderBy(info => info.Weight)
                .FirstOrDefault();

            radar(currentTarget.cell);
            //transform.position = currentTarget.cell;

            // ターゲットの隣接セルの情報を取得する
            List<AStarInfo> adjacentInfos = GetAdjacences(currentTarget.cell)
                .Where(cell => {
                    // タイプが道でもなくゴールのセルでもない場合は対象外
                    if (wall[(int)cell.x, (int)cell.y, (int)cell.z] == 1 && cell != GoalVec)
                    {
                        return false;
                    }
                    // 計算済みのセルは対象外
                    if (passedCells.Contains(cell))
                    {
                        return false;
                    }
                    return true;
                })
                .Select(cell => GetAStarInfo(cell, GoalVec, currentTarget))
                .ToList();

            // recentTargetsとpassedCellsを更新
            recentTargets.Remove(currentTarget);
            recentTargets.AddRange(adjacentInfos);
            passedCells.Add(currentTarget.cell);

            // ゴールが含まれていたらそこで終了
            goalInfo = adjacentInfos.FirstOrDefault(info => info.cell == GoalVec);
            if (goalInfo != null)
            {
                break;
            }
            // recentTargetsがゼロだったら行き止まりなので終了
            if (recentTargets.Count == 0)
            {
                
                break;
            }
        }

        // ゴールが結果に含まれていない場合は最短経路が見つからなかった
        if (goalInfo == null)
        {
            Debug.Log("Not Find Goal");
            return res;
        }

        Debug.Log("Find Goal");

        // Previousを辿ってセルのリストを作成する
        res.Add(goalInfo.cell);
        AStarInfo current = goalInfo;
        while (true)
        {
            if (current.previous != null)
            {
                res.Add(current.previous.cell);
                current = current.previous;
            }
            else
            {
                break;
            }
        }
        return res;
    }


    private AStarInfo GetAStarInfo(Vector3 targetVec, Vector3 goalVec, AStarInfo previousInfo)
    {
        var result = new AStarInfo();
        result.cell = targetVec;
        result.previous = previousInfo;
        result.step = previousInfo == null ? 0 : previousInfo.step + 1;
        result.distance = Vector3.Distance(targetVec, goalVec);
        return result;
    }

    List<Vector3> _shortestWay;
    void Start()
    {
        init();
        //Debug.Log((int)(6.4));
        //Debug.Log((int)(6.5));
        //切り捨てintのキャスト
        _shortestWay = GetShortestWay(gp, sp);

        StartCoroutine(DelayCoroutine());

        /*for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                Console.Write(wall[i, 0, j]);
            }
            Console.WriteLine();
        }*/

    }
    private IEnumerator DelayCoroutine()
    {

        foreach (var Item in _shortestWay)
        {
            yield return new WaitForSeconds(1);
            Debug.Log(Item);
            obj.transform.position = Item;
        }

    }
    void Update()
    {
        
    }

}
