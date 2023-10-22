using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nav_manager : MonoBehaviour
{
    public int x_steps;
    public int y_steps;
    public float step_size;
    public Transform nav_block;
    public Transform[,] grid_map;
    public int[,] grid_nav ;
    // Start is called before the first frame update
    void Start()
    {
        InitializeGrid();
    }

    // Update is called once per frame
 
    public int[] find_cloest_point(Transform car)
    {
        float closestDistance = float.MaxValue;
        int[] grid_num;
        grid_num = new int[2];
        Transform closestObject = null;
        for (int x = 0; x < x_steps; x++)
        {
            for (int y = 0; y < y_steps; y++)
            {
                float distance = Vector3.Distance(grid_map[x,y].position, car.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = grid_map[x, y];
                    grid_num[0] = x;
                    grid_num[1] = y;
                }
            }
        }
        return grid_num;
    }
    public int[] get_random_destination()
    {
        int x, y;
        int ret;
        do
        {
            x = Random.Range(0, x_steps);
            y = Random.Range(0, y_steps);
            ret = grid_nav[x, y];
        } while (ret == 1);

        int[] fin = new int[2];
        fin[0] = x;
        fin[1] = y;
        return fin;
    }
    public List<(int, int)> get_path(int[] start_pos, int[] end_pos)
    {
        var start = (start_pos[0], start_pos[1]);
        var end = (end_pos[0], end_pos[1]);
         int[] dx = { -1, 1, 0, 0 }; // For moving left, right, up, and down
         int[] dy = { 0, 0, -1, 1 }; // Corresponding changes in row and column


        int numRows = grid_nav.GetLength(0);
        int numCols = grid_nav.GetLength(1);

        // Check if start and end points are valid
        if (start.Item1 < 0 || start.Item1 >= numRows || start.Item2 < 0 || start.Item2 >= numCols ||
            end.Item1 < 0 || end.Item1 >= numRows || end.Item2 < 0 || end.Item2 >= numCols)
        {
            print("INVALID STARTING POINT!");
        }

        // Create a 2D array to store the minimum distance to each cell
        int[,] distance = new int[numRows, numCols];
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                distance[i, j] = int.MaxValue;
            }
        }

        // Create a 2D array to keep track of visited cells
        bool[,] visited = new bool[numRows, numCols];

        // Create a queue for BFS
        Queue<(int, int)> queue = new Queue<(int, int)>();
        queue.Enqueue(start);
        distance[start.Item1, start.Item2] = 0;

        
        int warning = 0;
        while (queue.Count > 0)
        {
            if(warning == 5000)
            {
                List<(int, int)> warning_list = new List<(int, int)>();
                warning_list.Add((-1, -1));
                print("returned null 1");
                return warning_list;
            }
            warning++;
            var current = queue.Dequeue();
            int x = current.Item1;
            int y = current.Item2;

            visited[x, y] = true;

            // Explore neighbors
            for (int dir = 0; dir < 4; dir++)
            {
                int newX = x + dx[dir];
                int newY = y + dy[dir];

                if (IsValidCell(newX, newY, numRows, numCols) && !visited[newX, newY] && grid_nav[newX, newY] != 1)
                {
                    int newDistance = distance[x, y] + 1;

                    if (newDistance < distance[newX, newY])
                    {
                        distance[newX, newY] = newDistance;
                        queue.Enqueue((newX, newY));
                    }
                }
            }
        }

        // Reconstruct the shortest path
        List<(int, int)> shortestPath = new List<(int, int)>();
        int curX = end.Item1;
        int curY = end.Item2;
        warning = 0;
        while (curX != start.Item1 || curY != start.Item2)
        {
            if (warning == 5000)
            {
                List<(int, int)> warning_list = new List<(int, int)>();
                warning_list.Add((-1, -1));
                print("returned null 1");
                return warning_list;
            }
            warning++;
            shortestPath.Add((curX, curY));

            for (int dir = 0; dir < 4; dir++)
            {
                int prevX = curX - dx[dir];
                int prevY = curY - dy[dir];

                if (IsValidCell(prevX, prevY, numRows, numCols) &&
                    distance[prevX, prevY] == distance[curX, curY] - 1)
                {
                    curX = prevX;
                    curY = prevY;
                    break;
                }
            }
        }

        shortestPath.Add(start);
        shortestPath.Reverse();

        return shortestPath;
    }
    private static bool IsValidCell(int x, int y, int numRows, int numCols)
    {
        return x >= 0 && x < numRows && y >= 0 && y < numCols;
    }
    public LayerMask blockedLayer;
    void InitializeGrid()
    {
        grid_map = new Transform[x_steps, y_steps];
        grid_nav = new int[x_steps, y_steps];
        for (int x = 0; x < x_steps; x++)
        {
            for (int y = 0; y < y_steps; y++)
            {
                grid_nav[x,y] = 0;
                Transform instantiatedTransform = Instantiate(nav_block, new Vector3(transform.position.x + step_size * x, transform.position.y, transform.position.z + step_size * y), Quaternion.identity);
                grid_map[x, y] = instantiatedTransform;

                Ray ray = new Ray(instantiatedTransform.position + new Vector3(0f, 1400f, 0), Vector3.down);

                // Perform the raycast and store the hit information.
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1500f, blockedLayer))
                {
                    // Check if the hit object is on the "blocked" layer.
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("blocked"))
                    {
                        print("Blocked!");
                        grid_nav[x,y] = 1;
                    }
                }
            }
        }
    }

    public Transform find_pos_of_grid(int x, int y)
    {
  
        return grid_map[x,y];
    }

    public float target_angle_calculator(Transform target)
    {

        return 0f;
    }

    public Transform get_n_pathes_ahead(int n)
    {
        Transform nav_point = null;

        return nav_point;
    }
}
