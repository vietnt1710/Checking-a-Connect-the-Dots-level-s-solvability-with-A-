// Tham khao giai bang thuat toan quay lui https://www.101computing.net/connect-flow-backtracking-algorithm/#
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class ConnectTheDotsAlgorithm : MonoBehaviour

{   private int[,] grid; // Ma trận lưới chứa các giá trị điểm
    private int[,] _tempGrid; // Ma trận tạm để lưu trữ lưới gốc

    [Header("Tile gốc ")]
    public GameObject cloneSquare;

    [Header("Màu sắc")]
    public List<Color> Colors;

    [Header("Parent để chứa các tile được tạo ra")]
    public Transform parent; 

    void Start()
    {
        // Khởi tạo ma trận lưới với các giá trị mặc định
        grid = new int[10, 10]
        {
          { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
          { -1,  0,  0,  2,  0,  0,  0,  0,  0, -1 },
          { -1,  0,  0,  3,  4,  5,  0,  0,  0, -1 },
          { -1,  0,  0,  6,  0,  0,  0,  0,  0, -1 },
          { -1,  0,  0,  0,  0,  0,  6,  0,  4, -1 },
          { -1,  0,  0,  0,  2,  0,  3,  0,  0, -1 },
          { -1,  5,  0,  7,  0,  0,  7,  8,  0, -1 },
          { -1,  9,  0,  0,  0,  8,  0,  0,  0, -1 },
          { -1,  0,  0,  9,  0,  0,  0,  0,  0, -1 },
          { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
        };
        CreateCloneNumberMatrix(); // Tạo bản sao của ma trận lưới
        DetectingAllDotsInMatrix(); // Kiểm tra tất cả các điểm trong ma trận
    }

    /// <summary>
    /// Kiểm tra tất cả các điểm trong ma trận và lưu vào danh sách allDotInMatrix.
    /// </summary>
    List<adot> _allDots = new List<adot>();
    public void DetectingAllDotsInMatrix()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (grid[i, j] != 0)
                {
                    var newdot = new adot(i, j, grid[i, j]);
                    _allDots.Add(newdot);
                }
            }
        }

        DetectingAllCoupleDotInMatrix(); // Kiểm tra tất cả các cặp điểm trong ma trận
    }

    /// <summary>
    /// Kiểm tra tất cả các cặp điểm trong ma trận và lưu vào danh sách allCoupleDotInMatrix.
    /// </summary>
    List<coupledots> _allCoupleDots = new List<coupledots>();
    public void DetectingAllCoupleDotInMatrix()
    {
        int value = _allDots[0].value;
        if (value != -1)
        {
            for (int i = 1; i < _allDots.Count; i++)
            {
                if (_allDots[i].value == value)
                {
                    var newcoupledot = new coupledots(_allDots[0], _allDots[i], value);
                    _allCoupleDots.Add(newcoupledot);
                    _allDots.Remove(_allDots[i]);
                    _allDots.Remove(_allDots[0]);
                    break;
                }
            }
        }
        else
        {
            _allDots.Remove(_allDots[0]);
        }

        if (_allDots.Count >= 2)
        {
            DetectingAllCoupleDotInMatrix(); // Đệ quy để kiểm tra tiếp các cặp điểm
        }
        else
        {
            StartAstarByDistance(); // Bắt đầu thuật toán A* dựa trên khoảng cách
            DebugMatrix(grid);
            MakeColor(grid); // Tô màu các điểm trên ma trận
        }
    }

    /// <summary>
    /// Sắp xếp các cặp điểm theo khoảng cách và bắt đầu thuật toán A*.
    /// </summary>
    bool _currentCouplesFindingEnd;
    List<coupledots> sortCouples = new List<coupledots>();
    public void StartAstarByDistance()
    {
        var enum1 = from cpdot in _allCoupleDots
                    orderby cpdot.distance
                    select cpdot;

        foreach (var e in enum1)
        {
            sortCouples.Add(e);
        }
        var CurrentStep = 0;
        ReSort(CurrentStep); // Bắt đầu thuật toán backtracking
    }

    /// <summary>
    ///  Kiểm tra và điều chỉnh thứ tự các cặp điểm.
    /// </summary>
    public void ReSort(int CurrentStep)
    {
        CurrentStep++;
        if (CurrentStep < 100)
        {
            for (int i = 0; i < sortCouples.Count; i++)
            {
                var dotStart = new dots(sortCouples[i].start.x, sortCouples[i].start.y);
                var dotEnd = new dots(sortCouples[i].end.x, sortCouples[i].end.y);

                Debug.Log("Check" + dotStart.x + ":" + dotStart.y + "  " + dotEnd.x + ":" + dotEnd.y);
                Astar(dotStart, dotEnd, sortCouples[i].value, grid); // Thực hiện thuật toán A*

                if (!_currentCouplesFindingEnd) // nếu cặp hiện tại không tìm thấy đường đi
                {
                    CheckInAStep(i, 0, CurrentStep, 0); // Kiểm tra và điều chỉnh thứ tự các cặp điểm
                    break;
                }
            }
        }
        else
        {
            Debug.Log("Khong co loi giai"); // Không tìm thấy giải pháp
        }
    }

    /// <summary>
    /// Kiểm tra và điều chỉnh thứ tự các cặp điểm nếu có điểm không tìm được đường đi.
    /// </summary>
    bool _detectingRoad;
    public void CheckInAStep(int step, int a, int CurrentStep, int b)
    {
        b++;
        if (b < 100)
        {
            _detectingRoad = true;
            ClearNumberMatrix(); // Xóa và khôi phục ma trận về trạng thái ban đầu
            for (int i = 0; i <= step; i++)
            {
                var dotStart = new dots(sortCouples[i].start.x, sortCouples[i].start.y);
                var dotEnd = new dots(sortCouples[i].end.x, sortCouples[i].end.y);
                Astar(dotStart, dotEnd, sortCouples[i].value, grid); // Thực hiện thuật toán A*
                if (!_currentCouplesFindingEnd)
                {
                    _detectingRoad = false;
                }
            }

            if (_detectingRoad)
            {
                ReSort(CurrentStep); // Tiếp tục thuật toán backtracking
               
            }
            else
            {
                var arrayTemp = sortCouples.ToArray();
                var index = step - a;
                try
                {
                    var dot1 = sortCouples[index];
                    var dot2 = sortCouples[index - 1];
                    arrayTemp[index - 1] = dot1;
                    arrayTemp[index] = dot2;
                    a++;
                }
                catch (Exception e)
                {
                }
                sortCouples.Clear();
                sortCouples = arrayTemp.ToList();
                CheckInAStep(step, a, CurrentStep, b); // Đệ quy để kiểm tra lại
            }
        }
        else
        {
            Debug.Log("Khong co loi giai"); // Không tìm thấy giải pháp
        }
    }

    /// <summary>
    /// Thuật toán A* để tìm đường đi ngắn nhất giữa hai điểm.
    /// </summary>
    public void Astar(dots Start, dots End, int value, int[,] number)
    {
        var CurrentDot = Start;
        int CountLoop = 0;
        List<value> Q = new List<value>(); // Danh sách các điểm đang xét
        List<value> dotExpanded = new List<value>(); // Danh sách các điểm đã xét
        List<value> allDot = new List<value>(); // Tất cả các điểm đã và đang xét

        AstarLoop(Start, End, Q, CurrentDot, dotExpanded, allDot, value, CountLoop, number); // Bắt đầu vòng lặp A*
    }

    /// <summary>
    /// Vòng lặp chính của thuật toán A*.
    /// </summary>
    public void AstarLoop(dots Start, dots End, List<value> Q, dots CurrentDot, List<value> dotExpanded, List<value> allDot, int value, int CountLoop, int[,] number)
    {
        CountLoop++;
        // Xét các điểm liền kề
        ConsiderAdjacentDot(1, 0, Start, Q, CurrentDot, allDot, value, number); 
        ConsiderAdjacentDot(-1, 0, Start, Q, CurrentDot, allDot, value, number);
        ConsiderAdjacentDot(0, 1, Start, Q, CurrentDot, allDot, value, number);
        ConsiderAdjacentDot(0, -1, Start, Q, CurrentDot, allDot, value, number);

        float minDis = 10000;
        int index = 0;

        try
        {
            //Tìm điểm có khoảng cách ngắn nhất trong danh sách Q (open list)
            for (int i = 0; i < Q.Count; i++) // Tìm điểm có khoảng cách ngắn nhất
            {
                if (Q[i].distance < minDis) // Kiểm tra nếu khoảng cách của điểm hiện tại nhỏ hơn minDis
                {
                    Debug.Log("Lap toi da" + Q[i].start.x + ":"+ Q[i].start.y +"  " + Q[i].end.x + ":" + Q[i].end.y);
                    minDis = Q[i].distance;
                    index = i; // Lưu vị trí của điểm có khoảng cách ngắn nhất
                }
            }
            dotExpanded.Add(Q[index]); // Đưa điểm này vào danh sách các điểm đã mở rộng (closed list)



            //Kiểm tra xem đã đến đích chưa
            if (Q[index].end.x == End.x && Q[index].end.y == End.y) // Nếu tìm thấy điểm kết thúc
            {
                Debug.Log("Tim thay diem cuoi cung" ) ;
                List<dots> Result = new List<dots>();
                var dotEnd = dotExpanded[dotExpanded.Count - 1].end;
                var dotStart = dotExpanded[dotExpanded.Count - 1].start;

                Result.Add(dotEnd);
                Result.Add(dotStart);

                var dot = dotExpanded[dotExpanded.Count - 1].start;
                EndAstarLoop(Start, dot, dotExpanded, Result, value, number); // Kết thúc vòng lặp A*
                _currentCouplesFindingEnd = true;
                return;
            }
            else
            {
                CurrentDot = Q[index].end;
                Q.Remove(Q[index]);
                if (CountLoop < 10 * 10) // Giới hạn số lần lặp bằng kích thước ma trận 
                {
                    
                    AstarLoop(Start, End, Q, CurrentDot, dotExpanded, allDot, value, CountLoop, number);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Loi giai sai"); // Lỗi giải thuật
            _currentCouplesFindingEnd = false;
        }
    }

    /// <summary>
    /// Kết thúc vòng lặp A* và lưu kết quả đường đi.
    /// </summary>
    public void EndAstarLoop(dots Start, dots dot, List<value> dotExpanded, List<dots> Result, int value, int[,] number)
    {
        for (int i = 0; i < dotExpanded.Count; i++)
        {
            if (dot.x == dotExpanded[i].end.x && dot.y == dotExpanded[i].end.y)
            {
                var _dot = dotExpanded[i].start;
                Result.Add(_dot);
                if (dotExpanded[i].start.x == Start.x && dotExpanded[i].start.y == Start.y)
                {
                    ChangNumberListValue(Result, value, number); // Thay đổi giá trị trên ma trận
                }
                else
                {
                    EndAstarLoop(Start, _dot, dotExpanded, Result, value, number); // Đệ quy để tiếp tục tìm đường
                }
            }
        }
    }


    /// <summary>
    /// Xét các điểm liền kề với điểm hiện tại.
    /// </summary>
    public void ConsiderAdjacentDot(int x, int y, dots Start, List<value> Q, dots CurrentDot, List<value> allDot, int value, int[,] number)
    {
        try
        {
            if (number[(int)CurrentDot.x + x, (int)CurrentDot.y + y] != -1) // Kiểm tra xem ô tiếp theo có phải là tường không
            {
                if (number[(int)CurrentDot.x + x, (int)CurrentDot.y + y] == 0 || number[(int)CurrentDot.x + x, (int)CurrentDot.y + y] == value) // Kiểm tra xem ô đó có thể đi qua không
                {
                    // Kiểm tra xem điểm đã được xét chưa
                    var checkDot = new dots(CurrentDot.x + x, CurrentDot.y + y);
                    if (!CheckDotInAllDot(checkDot, allDot)) 
                    {
                        var distance = Math.Abs(Start.x - checkDot.x) + Math.Abs(Start.y - checkDot.y) + Math.Sqrt(Math.Pow(Math.Abs(Start.x - checkDot.x), 2) + Math.Pow(Math.Abs(Start.y - checkDot.y), 2));
                        var _value = new value(CurrentDot, checkDot, (float)distance);
                        allDot.Add(_value);
                        Q.Add(_value);
                    }
                }
            }
        }
        catch
        {
            Debug.Log("Loi khi di chuyen");
        }
    }

    /// <summary>
    /// Kiểm tra xem một điểm đã tồn tại trong danh sách các điểm đã xét chưa.
    /// </summary>
    public bool CheckDotInAllDot(dots endot, List<value> allDot)
    {
        bool right = true;
        if (allDot.Count == 0)
        {
            right = false;
        }
        else
        {
            for (int i = 0; i < allDot.Count; i++)
            {
                if (endot.x == allDot[i].start.x && endot.y == allDot[i].start.y)
                {
                    right = true;
                    break;
                }
                else
                {
                    right = false;
                }
                if (endot.x == allDot[i].end.x && endot.y == allDot[i].end.y)
                {
                    right = true;
                    break;
                }
                else
                {
                    right = false;
                }
            }
        }
        return right;
    }

    /// <summary>
    /// Thay đổi giá trị trên ma trận dựa trên kết quả đường đi.
    /// </summary>
    public void ChangNumberListValue(List<dots> Result, int value, int[,] number)
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                for (int k = 0; k < Result.Count; k++)
                {
                    if (i == Result[k].x && j == Result[k].y)
                    {
                        number[i, j] = value;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Tạo bản sao của ma trận lưới để sử dụng sau này.
    /// </summary>
    public void CreateCloneNumberMatrix()
    {
        if (grid == null)
        {
            Debug.LogError("grid chưa được khởi tạo!");
            return;
        }

        if (_tempGrid == null)
        {
            _tempGrid = new int[10, 10]; // Khởi tạo nếu chưa có
        }

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                _tempGrid[i, j] = grid[i, j];
            }
        }
    }

    /// <summary>
    /// Khôi phục ma trận lưới về trạng thái ban đầu từ bản sao.
    /// </summary>
    public void ClearNumberMatrix()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                grid[i, j] = _tempGrid[i, j];
            }
        }
    }

    /// <summary>
    /// Tô màu các điểm trên ma trận dựa trên giá trị của chúng.
    /// </summary>
    public void MakeColor(int[,] number)
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {

                var obj = Instantiate(cloneSquare, new Vector3(9 - i, 9 - j, 0), quaternion.identity);
                try
                {
                    obj.GetComponent<SpriteRenderer>().color = Colors[number[i, j]];
                }
                catch (Exception e)
                {
                    // Bỏ qua nếu có lỗi
                }
                obj.transform.SetParent(parent);
            }
        }
    }
    public void DebugMatrix(int[,] matrix)
    {
        string matrixString = "";
        for (int i = 0; i < matrix.GetLength(0); i++) // Duyệt qua các hàng
        {
            string row = "";
            for (int j = 0; j < matrix.GetLength(1); j++) // Duyệt qua các cột
            {
                row += matrix[i, j] + "\t"; // Thêm giá trị của mỗi ô vào row và phân cách bằng tab
            }
            matrixString += row + "\n"; // Thêm dòng vào chuỗi kết quả
        }
        Debug.Log(matrixString); // In toàn bộ ma trận ra console
    }
}
/// <summary>
/// Struct để lưu trữ tọa độ x, y của một điểm (dot).
/// </summary>
[Serializable]
public struct dots
{
    public float x;
    public float y;

    public dots(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}

/// <summary>
/// Struct để lưu trữ thông tin về một cặp điểm (start, end) và khoảng cách giữa chúng.
/// </summary>
[Serializable]
public struct value
{
    public dots start;
    public dots end;
    public float distance;

    public value(dots start, dots end, float distance)
    {
        this.start = start;
        this.end = end;
        this.distance = distance;
    }
}

/// <summary>
/// Struct để lưu trữ thông tin về một điểm (dot) với giá trị (value) tương ứng.
/// </summary>
[Serializable]
public struct adot
{
    public float x;
    public float y;
    public int value;

    public adot(float x, float y, int value)
    {
        this.x = x;
        this.y = y;
        this.value = value;
    }
}

/// <summary>
/// Struct để lưu trữ thông tin về một cặp điểm (start, end) với giá trị (value) và khoảng cách giữa chúng.
/// </summary>
[Serializable]
public struct coupledots
{
    public adot start;
    public adot end;
    public int value;
    public float distance;

    public coupledots(adot start, adot end, int value)
    {
        this.start = start;
        this.end = end;
        this.value = value;
        distance = (float)Math.Sqrt(Math.Pow(Math.Abs(start.x - end.x), 2) + Math.Pow(Math.Abs(start.y - end.y), 2));
    }
}
