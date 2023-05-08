using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public class AStaralgo : MonoBehaviour
{
    public const int field_size = 40;
    public GameObject obj;
    //public const GameObject goal;
    public Vector3 sp = new Vector3(0, 0, 0);//start.transform.position;

    public Vector3 gp = new Vector3(10, 0, 10);//goal.transform.position;

    public int[,,] wall = new int[field_size + 1, field_size + 1, field_size + 1];

    private void init()     //�z��̏�����(����ԂȂ̂łǂ��ɕǂ��Q�������邩�͂킩��Ȃ��̂�wall�͂��ׂ�0�ɂ���)
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

    private void radar()
    {
        Vector3 origin = transform.position;

        for (int i = 0; i < 360; i++)    //360�x��]���A����̎Օ������L�^
        {
            transform.Rotate(0, 1, 0);    //1�x����]
            Vector3 direction = transform.forward;
            Ray ray = new Ray(origin, direction);   //Ray�^:�����������Borigin:���݈ʒu�Adirection:�������o������
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))   //��Q��������������hit�ɏ�񂪊i�[����ARaycast�֐���1��Ԃ�
            {
                //string name = hit.collider.gameObject.name;
                // Debug.Log(name);
                Vector3 hit_vec = hit.point;
                int hit_int_x = (int)hit_vec.x;     //�����������W��int�^�ɃL���X�g
                int hit_int_y = (int)hit_vec.y;
                int hit_int_z = (int)hit_vec.z;
                // Debug.Log(hit_int_z);
                wall[hit_int_x, 0, hit_int_z] = 1;      //wall��1�ɂ���
                // Debug.Log(hit_int_x+","+hit_int_z);
            }
        }
    }
    //���[���h���̗אڂ��锪�}�X���i�[
    public List<Vector3> GetAdjacences(Vector3 v)
    {
        var adjacences = new List<Vector3>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;
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
            // recentTargets�̂���weight���ł��Ⴂ���̂��v�Z�ΏۂƂ���
            AStarInfo currentTarget = recentTargets
                .OrderBy(info => info.Weight)
                .FirstOrDefault();

            //radar();
            //transform.position = currentTarget.cell;

            // �^�[�Q�b�g�̗אڃZ���̏����擾����
            List<AStarInfo> adjacentInfos = GetAdjacences(currentTarget.cell)
                .Where(cell => {
                    // �^�C�v�����ł��Ȃ��S�[���̃Z���ł��Ȃ��ꍇ�͑ΏۊO
                    if (wall[(int)cell.x, (int)cell.y, (int)cell.z] == 1 && cell != GoalVec)
                    {
                        return false;
                    }
                    // �v�Z�ς݂̃Z���͑ΏۊO
                    if (passedCells.Contains(cell))
                    {
                        return false;
                    }
                    return true;
                })
                .Select(cell => GetAStarInfo(cell, GoalVec, currentTarget))
                .ToList();

            // recentTargets��passedCells���X�V
            recentTargets.Remove(currentTarget);
            recentTargets.AddRange(adjacentInfos);
            passedCells.Add(currentTarget.cell);

            // �S�[�����܂܂�Ă����炻���ŏI��
            goalInfo = adjacentInfos.FirstOrDefault(info => info.cell == GoalVec);
            if (goalInfo != null)
            {
                break;
            }
            // recentTargets���[����������s���~�܂�Ȃ̂ŏI��
            if (recentTargets.Count == 0)
            {
                break;
            }
        }

        // �S�[�������ʂɊ܂܂�Ă��Ȃ��ꍇ�͍ŒZ�o�H��������Ȃ�����
        if (goalInfo == null)
        {
            return res;
        }

        // Previous��H���ăZ���̃��X�g���쐬����
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
        _shortestWay = GetShortestWay(sp, gp);

        StartCoroutine(DelayCoroutine());

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