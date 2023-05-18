using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//クラスPriorityQueue　始まり
class PriorityQueue<T> where T : IComparable<T>
{
    private readonly T[] _array;
    private readonly IComparer _comparer;
    public int Count { get; private set; } = 0;
    public T Root => _array[0];

    public PriorityQueue(int capacity, IComparer comparer = null)
    {
        _array = new T[capacity];
        _comparer = comparer;
    }

    /// <summary>
    /// 要素を挿入する
    /// </summary>
    public void Push(T item)
    {
        _array[this.Count] = item;
        Count += 1;

        var n = Count - 1;                  // 末尾(追加した)のノードの番号
        while (n != 0)
        {
            var parent = (n - 1) / 2;       // 親ノードの番号

            if (Compare(_array[n], _array[parent]) > 0)
            {
                Swap(n, parent);
                n = parent;
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 優先度の一番高いものを取り出す
    /// </summary>

    public void Pop()
    {
        Swap(0, this.Count - 1);            // 先頭要素を末尾にする
        Count -= 1;

        var parent = 0;                     // 親ノードの番号
        while (true)
        {
            var child = 2 * parent + 1;     // 子ノードの番号
            if (child > Count - 1) break;

            // 値の大きい方の子を選ぶ
            if (child < Count - 1 && Compare(_array[child], _array[child + 1]) < 0) child += 1;

            // 子の方が親より大きければ入れ替える
            if (Compare(_array[parent], _array[child]) < 0)
            {
                Swap(parent, child);
                parent = child;
            }
            else
            {
                break;
            }
        }

        // return _array[Count];
    }

    public bool empty()
    {
        if(Count == 0) return true;
        else return false;
    }
    /// <summary>
    /// 大きいものから列挙していく
    /// withPop = falseのときは順番関係なく取り出しもしないが素早く全要素を取得できる　
    /// </summary>
    // public IEnumerable<T> GetAllElements(bool withPop = true)
    // {
    //     int count = Count;
    //     for (int i = 0; i < count; i++)
    //     {
    //         if (withPop) yield return Pop();
    //         else yield return _array[count - i - 1];
    //     }
    // }

    private int Compare(T a, T b)
    {
        if (_comparer == null) return a.CompareTo(b);
        return _comparer.Compare(a, b);
    }

    private void Swap(int a, int b)
    {
        var tmp = _array[a];
        _array[a] = _array[b];
        _array[b] = tmp;
    }
}
//PriorityQueue　終わり
public class Dijkstra : MonoBehaviour
{
    public int idx_of_target; //最短経路を進むときの添え字
    public Vector3 target;    //最短経路を移動しているときに次の目標となる点
    public int n;             //ノードの数
    public int m;             //エッジの数
    public List<Vector3> route = new List<Vector3>();//最短経路に含まれる点を格納    // private double Distace(Vector3 a, Vector3 b){
    //     int dx = (int)(a.x - b.x);
    //     int dy = (int)(a.y - b.y);
    //     int dz = (int)(a.z - b.z);
    //     return Math.Sqrt(dx*dx + dy*dy + dz*dz);
    // }

    // private void input(ref Dictionary<int, List<int>> edges_to, ref int[,] edges_cost, Dictionary<int, Vector3> cor, Dictionary<Vector3, int> cor2)
    // {   

    //     // for(int i = 0; i < size-1; i++){
    //     //     for(int j = i+1; j < size; j++){
    //     //         if(Distace(trace[i], trace[j]) < 8){ //2点間の距離が3以下だったら点と点を結ぶ
    //     //             int a = cor2[trace[i]];
    //     //             int b = cor2[trace[j]];
    //     //             if(!edges_to.ContainsKey(a)){
    //     //                 edges_to.Add(a, new List<int>());
    //     //                 edges_to[a].Add(b);
    //     //             }
    //     //             else if(edges_to.ContainsKey(a)){
    //     //                 edges_to[a].Add(b);
    //     //             }

    //     //             if(!edges_to.ContainsKey(b)){
    //     //                 edges_to.Add(b, new List<int>());
    //     //                 edges_to[b].Add(a);
    //     //             }
    //     //             else if(edges_to.ContainsKey(b)){
    //     //                 edges_to[b].Add(a);
    //     //             }

    //     //             edges_cost[a,b] = (int)(Distace(trace[i], trace[j])*10);
    //     //             edges_cost[b,a] = (int)(Distace(trace[i], trace[j])*10);
    //     //         }
    //     //     }
    //     // }

    //     for(int i = 0; i < m; i++){
            
            
    //     }
    // }
    private void find_route(int n, int m, Vector3[] trace, Vector3[,] node_to_node, int[] weight, Vector3 node)
    {
        Dictionary<int, Vector3> cor = new Dictionary<int, Vector3>(); // corespondence(対応関係)
        Dictionary<Vector3, int> cor2 = new Dictionary<Vector3, int>(); 

        var edges_to = new Dictionary<int, List<int>>(); //それぞれの点がどの点とつながっているのか
        var edges_cost = new int[n, n];                  //添え字を点の番号にしてエッジのコストを格納
    
        // input(ref edges_to, ref edges_cost, cor, cor2);
        for(int i =0; i < n; i++){ // 対応付け
            cor.Add(i, trace[i]);
            cor2.Add(trace[i],i);
        }

        for(int i = 0; i < m; i++){ // グラフの作成
            int a = cor2[node_to_node[i,0]];
            int b = cor2[node_to_node[i,1]];
            int c = weight[i];
            if(!edges_to.ContainsKey(a)){
                edges_to.Add(a, new List<int>());
                edges_to[a].Add(b);
            }
            else if(edges_to.ContainsKey(a)){
                edges_to[a].Add(b);
            }

            if(!edges_to.ContainsKey(b)){
                edges_to.Add(b, new List<int>());
                edges_to[b].Add(a);
            }
            else if(edges_to.ContainsKey(b)){
                edges_to[b].Add(a);
            }

            edges_cost[a,b] = c;
            edges_cost[b,a] = c;

        }

        var done = new Dictionary<int, bool>();       // 点への最小コストが決定したかどうかを判断する
        var cost = new Dictionary<int, int>();        //　点への最小コストを格納する
        var route_front = new Dictionary<int, int>(); // 最小コストが決まった時の一つ前の点
        for(int i = 0; i< n; i++){
            done.Add(i, false);     // すべての点をfalseで初期化
            cost.Add(i, 1<<30);     // すべての点を1<<30 で初期化
            route_front.Add(i, -1); // すべての点を-1で初期化
        }

        var pq = new PriorityQueue<int>(1000, Comparer<int>.Create((x, y) => (int)(y - x)));//優先度付きqueue
        int idx =0;
        cost[idx] = 0;
        done[idx] = true;
        for(int i = 0; i < edges_to[idx].Count; i++) {
            var x = edges_to[idx][i];
            pq.Push(edges_cost[idx,x]);
            cost[x] = edges_cost[idx,x];
            route_front[x] = idx;
        }

        while(!pq.empty()) {
            int x = pq.Root;
            pq.Pop();
            for(int i = 0; i < n; i++){
                if(done[i] == false && cost[i] == x) {
                    done[i] = true;
                    idx = i;
                    break;
                }
            }
            for(int i = 0; i < edges_to[idx].Count; i++){
                var x2 = edges_to[idx][i];
                if(done[x2] == true) continue;
                else{
                    if(cost[x2] > cost[idx] + edges_cost[idx,x2]){
                        cost[x2] = cost[idx] + edges_cost[idx,x2];//最小コストを更新
                        route_front[x2] = idx;
                        pq.Push(cost[x2]);
                    }
                }
            }
        }

        int w = cor2[node]; // 行きたい点(戻りたい点)の添え字
        route.Add(cor[w]);

        while(route_front[w] != -1){
            w = route_front[w];
            route.Add(cor[w]);
        }

        route.Reverse();

    }
    private void into(ref Vector3[] trace, ref Vector3[,] node_to_node, ref int[] weight)
    {
        for(int i = 0; i < n; i++){
            trace[i].y = 2.0f;
        }
        trace[0].x = 0;
        trace[0].z = -10.0f;

        trace[1].x = -10.0f;
        trace[1].z = 0;

        trace[2].x = 0;
        trace[2].z = 0;

        trace[3].x = 10.0f;
        trace[3].z = 0;

        trace[4].x = 0;
        trace[4].z = 10.0f;

        node_to_node[0,0] = trace[0];
        node_to_node[0,1] = trace[1];
        weight[0] = 8;

        node_to_node[1,0] = trace[0];
        node_to_node[1,1] = trace[2];
        weight[1] = 4;

        node_to_node[2,0] = trace[0];
        node_to_node[2,1] = trace[3];
        weight[2] = 1;

        node_to_node[3,0] = trace[1];
        node_to_node[3,1] = trace[4];
        weight[3] = 1;

        node_to_node[4,0] = trace[1];
        node_to_node[4,1] = trace[2];
        weight[4] = 10;

        node_to_node[5,0] = trace[2];
        node_to_node[5,1] = trace[4];
        weight[5] = 6;

        node_to_node[6,0] = trace[3];
        node_to_node[6,1] = trace[4];
        weight[6] = 5;

    }
    // Start is called before the first frame update
    void Start()
    {
        n = 5;// ノードの数
        m = 7 ; // エッジの数
        Vector3[] trace = new Vector3[n];// 今まで通ってきた点たち
        Vector3[,] node_to_node = new Vector3[m,2];// 辺の両端にある点たち
        int[] weight = new int[m];//辺の重さ
        Vector3 node = new Vector3();// 戻りたい点

        into(ref trace, ref node_to_node, ref weight);

        node = trace[1];// 戻りたい点を代入

        find_route(n, m, trace, node_to_node, weight,node);

        idx_of_target = 1;
        target = route[idx_of_target];
    }

    // Update is called once per frame
    void Update()
    {
        Transform myTransform = this.transform;
        Vector3 pos = myTransform.position;
        if(pos != target){// ターゲットに行くまで移動
            transform.position = 
            Vector3.MoveTowards(transform.position, target, Time.deltaTime*5);
        }else if(idx_of_target < route.Count-1){//　ターゲットに着いたらターゲットを変更
            idx_of_target++;
            target = route[idx_of_target];
        }
    }
}

