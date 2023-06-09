using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;



//using System.Drawing;
//using System.Diagnostics;

public class AStaralgorithm : MonoBehaviour
{
    public const int field_size = 40;
    public GameObject obj;
    
    public Vector3 sp; //スタート位置の整数座標

    public Vector3 gp;//ゴール位置

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

    private void radar(Vector3 origin) //引数に現在位置
    {

        for (int i = 0; i < 360; i++)    //360度回転し、周りの遮蔽物を記録
        {
            transform.Rotate(0, 1, 0);    //1度ずつ回転
            Vector3 direction = transform.forward;
            Ray ray = new Ray(origin, direction);   //Ray型:光線を示す。origin:現在位置、direction:光線を出す方向
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))   //障害物が当たったらhitに情報が格納され、Raycast関数は1を返す
            {
                
                Vector3 hit_vec = hit.point;

                double hit_int_x = Math.Round(hit_vec.x);     //当たった座標をint型にキャスト
                double hit_int_y = Math.Round(hit_vec.y);
                double hit_int_z = Math.Round(hit_vec.z);
//----------------------変更箇所------------------------------
                if (hit_vec == gp)
                {
                    Debug.Log("Find Goal");
                    return;
                }
                if (wall[(int)hit_int_x, (int)hit_int_y, (int)hit_int_z] == 0)
                {
                    wall[(int)hit_int_x, (int)hit_int_y, (int)hit_int_z] = 1;      
                }

                //Debug.DrawRay(origin, direction, Color.red, Vector3.Distance(origin, hit_vec)); //レーザーを可視化
  //----------------------変更箇所------------------------------
            }
        }
    }
    public List<Vector3> GetAdjacences(Vector3 v) //ワールド内の隣接する八マスを格納
    {
        List<Vector3> adjacences = new List<Vector3>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;     //隣接八マス
                //if (dx*dz == -1 || dx*dz == 1 || (dx == 0 && dz == 0)) continue;　//隣接四マス
                Vector3 cell = new Vector3(v.x + dx, v.y, v.z + dz);
                if (cell.x < 0 || cell.x > field_size || cell.z < 0 || cell.z > field_size)
                {
                    continue;
                }
//----------------------変更箇所------------------------------
                if (passedCells.Contains(cell))
                {
                    continue;
                }
                if (wall[(int)cell.x, (int)cell.y, (int)cell.z] == 1)
                {
                    continue;
                }
//----------------------変更箇所------------------------------
                adjacences.Add(cell);
            }
        }
        return adjacences;
    }


    public class AStarInfo
    {
        public Vector3 cell;
        public AStarInfo previous;
        public float step;
        public float distance;
        public float Weight { get { return (step + distance); } } //一応かっこでくくった
    }

    //----------------------変更箇所------------------------------
    //この関数は使わないで、while文をUpdateのなかで書き直している。
    //これにより、レーダーと移動物体の逐次探索が可視化できるようになっている
    //探索終了をまたずにSceneが見れる。
    /*
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

            //Debug.Log(currentTarget.cell);
            //obj.transform.position = currentTarget.cell;

            // ターゲットの隣接セルの情報を取得する
            List<AStarInfo> adjacentInfos = new List<AStarInfo>();
            foreach (Vector3 c in GetAdjacences(currentTarget.cell))
            {
                if (wall[(int)c.x, (int)c.y, (int)c.z] == 1 && c != GoalVec)
                {
                    continue;
                }
                // 計算済みのセルは対象外
                if (passedCells.Contains(c))
                {
                    continue;
                }
                adjacentInfos.Add(GetAStarInfo(c, GoalVec, currentTarget));
            }

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
    */

    public List<Vector3> RouteFind()
    {
        //getshortestwaのwhereの部分
        if (goalInfo == null)
        {

            // recentTargetsのうちweightが最も低いものを計算対象とする
            AStarInfo currentTarget = recentTargets
                .OrderBy(info => info.Weight)
                .FirstOrDefault();

            radar(currentTarget.cell);  //残留レーダーじゃなくて探索済のますが色でぬれたらいいね
            Debug.Log(currentTarget.cell);
            //Debug.Log(currentTarget.step);
            //obj.transform.position = currentTarget.cell;

            // ターゲットの隣接セルの情報を取得する
            List<AStarInfo> adjacentInfos = new List<AStarInfo>();
            foreach (Vector3 c in GetAdjacences(currentTarget.cell))
            {
                if (wall[(int)c.x, (int)c.y, (int)c.z] == 1 && c != gp)
                {
                    continue;
                }
                // 計算済みのセルは対象外
                if (passedCells.Contains(c))
                {
                    continue;
                }
                var tmp = GetAStarInfo(c, gp, currentTarget);
                bool can = false;
                foreach (var Item in recentTargets) //whereじゃなくした
                {
                    if (Item.cell == tmp.cell)      //269行から278の操作で解決。
                    {
                        can = true;
                        break;
                    }
                }
                if (can)
                {
                    continue;
                }
                adjacentInfos.Add(tmp);
            }

            // recentTargetsとpassedCellsを更新
            recentTargets.Remove(currentTarget);
            recentTargets.AddRange(adjacentInfos);
            passedCells.Add(currentTarget.cell);
            Debug.Log(string.Join(", ", GetAdjacences(currentTarget.cell))); //リストを改行なしで出力する方法
            // ゴールが含まれていたらそこで終了
            goalInfo = adjacentInfos.FirstOrDefault(info => info.cell == gp);
            if (goalInfo != null)
            {
                Debug.Log("Find Goal");

                Debug.Log("print passedCells");
                Debug.Log(string.Join(",", passedCells));

                //経路復元
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
                res.Reverse();
                return res;
            }
            // recentTargetsがゼロだったら行き止まりなので終了
            if (recentTargets.Count == 0)
            {
                Debug.Log("Not Find Goal");
                return null;
            }
            return null;
        }
        return null;
    }




    private AStarInfo GetAStarInfo(Vector3 targetVec, Vector3 goalVec, AStarInfo previousInfo)
    {
        AStarInfo result = new AStarInfo();
        result.cell = targetVec;
        result.previous = previousInfo;
        result.step = (previousInfo == null) ? 0 : (previousInfo.step + 1); //一応かっこでくくる
        result.distance = Vector3.Distance(targetVec, goalVec);
        return result;
    }
    
//----------------------変更箇所------------------------------
    List<Vector3> _shortestWay = new List<Vector3>();
    List<Vector3> res = new List<Vector3>();
    List<Vector3> passedCells = new List<Vector3>();
    List<AStarInfo> recentTargets = new List<AStarInfo>();
    AStarInfo goalInfo = null;
    int can = 0;
    void Start()
    {
        init();

        passedCells.Add(sp);
        recentTargets.Add(GetAStarInfo(sp, gp, null)); //GetShortestwayの最初の部分

    }
    void Update()
    {
        _shortestWay = RouteFind();
        if(goalInfo != null && can == 0)
        {
            /*for (int i = 0; i < _shortestWay.Count() - 1; i++)
            {
                Debug.Log(_shortestWay[i]);
                obj.transform.position = Vector3.Lerp(_shortestWay[i], _shortestWay[i + 1], 0.01f);

            }*/
            //StartCoroutine(DelayCoroutine());
            can = 1;
        }
    }
    /*private IEnumerator DelayCoroutine()
    {

        for (int i = 0; i < res.Count()-1; i++)
        {
            yield return new WaitForSeconds(1);
            Debug.Log(res[i]);
            obj.transform.position = Vector3.MoveTowards(res[i], res[i+1], 0.5f);
        }

    }*/
    //----------------------変更箇所------------------------------
}