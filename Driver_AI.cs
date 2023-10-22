using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Driver_AI : MonoBehaviour
{
    public float steering_anlge;
    public float raycast_length;
    public bool[] front_bool_ary = new bool[3];
    public Transform ray_left;
    public Transform ray_forward;
    public Transform ray_right;
    public float ray_left_dist;
    public float ray_right_dist;


    public bool stop_accelerating;

    public int[] closest_position;
    public int[] final_destination;
    public Transform target;

    public Transform front_vector;

    public GameObject navigator_manager;
    public List<(int, int)> shortestPath;
    public int nav_count_now;

    public Transform redball;
    public Transform blueball;

    public float current_max_speed;
    public float max_speed;
    public float slow_max_speed;
    public float angle2;
    public int saki_yomi;

    public bool is_Started;
    // Start is called before the first frame update
    void Start()
    {
        navigator_manager = GameObject.Find("nav_manager");
        final_destination = null;
        current_max_speed = max_speed;
        is_Started = true;
    }

    
    // Update is called once per frame
    void Update()
    {

        if (is_Started)
        {
            find_destination();
            acceleration_controller();
            steering_controller(null);
            front_eyes();

            //redball.position = target.position + new Vector3(0, 30f, 0);
        }

    }
    void find_destination()
    {
        if (target == null)
        {
            closest_position = navigator_manager.GetComponent<Nav_manager>().find_cloest_point(transform);
            target = navigator_manager.GetComponent<Nav_manager>().find_pos_of_grid(closest_position[0], closest_position[1]);
        }
        if (final_destination == null)
        {
            nav_count_now = 0;
            final_destination = navigator_manager.GetComponent<Nav_manager>().get_random_destination();
            closest_position = navigator_manager.GetComponent<Nav_manager>().find_cloest_point(transform);
            shortestPath = navigator_manager.GetComponent<Nav_manager>().get_path(closest_position, final_destination);

            if(shortestPath.Contains((-1, -1)))
            {
                print("DID IT AGAIN!");
                nav_count_now = 0;
                final_destination = navigator_manager.GetComponent<Nav_manager>().get_random_destination();
                closest_position = navigator_manager.GetComponent<Nav_manager>().find_cloest_point(transform);
                shortestPath = navigator_manager.GetComponent<Nav_manager>().get_path(closest_position, final_destination);
            }

            target = navigator_manager.GetComponent<Nav_manager>().find_pos_of_grid(shortestPath[nav_count_now].Item1, shortestPath[nav_count_now].Item2);
            foreach (var point in shortestPath)
            {
                //print($"({point.Item1}, {point.Item2})");
            }
            //print(shortestPath);

            transform.position = target.position;
            Vector3 direction = (navigator_manager.GetComponent<Nav_manager>().find_pos_of_grid(shortestPath[nav_count_now + saki_yomi].Item1, shortestPath[nav_count_now + saki_yomi].Item2).position) - target.position;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

        }



        float deltaX = transform.position.x - target.position.x;
        float deltaY = transform.position.z - target.position.z;

        float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);

        if (distance < 20f && nav_count_now == shortestPath.Count - 1)
        {
            nav_count_now = 0;
            final_destination = null;
        }

    if ((distance) < 20f)
        {
            nav_count_now += 1;
            target = navigator_manager.GetComponent<Nav_manager>().find_pos_of_grid(shortestPath[nav_count_now].Item1, shortestPath[nav_count_now].Item2);
        }
        deltaX = transform.position.x - target.position.x;
        deltaY = transform.position.z - target.position.z;
        
        //print(distance);

    }
    void steering_controller(Transform destination)
    {
        float final_steering_angle = 0f;
        float steering_angle = 0f;
        //steering_angle_destination = dot product calculation
        float additional_steering_angle = 0f;



        //for destination
        if (target != null)
        {
            float a = target_angle_calculator(target);

            if (Vector3.Distance(transform.position, target.position) > 1f){
                //print(a);
                if (a < 180 && a > 5) {
                    steering_angle = 15f;
                }
                else if (a > 180 && a < 355) {
                    steering_angle = -15f;
                }
                if (a == 0f || a == 360f)
                {
                    steering_angle = 0f;
                }
            }
        }

        //for avoiding
        if (front_bool_ary[0] && front_bool_ary[2])
        {
            stop_accelerating = true;
            if (ray_right_dist >= ray_left_dist)
            {
                steering_angle = 30f;
            }
            else
            {
                steering_angle = -30f;
            }
        }
        else if (front_bool_ary[0])
        {
            stop_accelerating = true;
            steering_angle = 30f;
        }
        else if (front_bool_ary[2])
        {
            stop_accelerating = true;
            steering_angle = -30f;
        }
        else
        {
            stop_accelerating = false;
        }
        //print(steering_angle);
        final_steering_angle = steering_angle;
        if(gameObject.GetComponent<carController>().verticalInput < 0)
        {
            final_steering_angle *= -1;
        }
        gameObject.GetComponent<carController>().currentSteerAngle = final_steering_angle;
    }
    float target_angle_calculator(Transform target)
    {
        Vector3 worldPosition = front_vector.TransformPoint(Vector3.zero);

        Vector3 a = new Vector3(worldPosition.x - transform.position.x, worldPosition.y - transform.position.y, worldPosition.z - transform.position.z);
        Vector3 b = new Vector3(target.position.x - transform.position.x, target.position.y - transform.position.y, target.position.z - transform.position.z);

        float angle = Mathf.Acos(Vector3.Dot(a.normalized, b.normalized)) * Mathf.Rad2Deg;

        // Calculate the cross product to determine if it's on the left or right
        Vector3 cross = Vector3.Cross(a, b);

        // Check the sign of the cross product's y component
        if (cross.y < 0)
        {
            angle = 360 - angle; // Adjust angle for the left side
        }

        return angle;
    }
    float target_angle_calculator2(Transform target_a, Transform target_b,Transform target_origin)
    {
        //Vector3 worldPosition = front_vector.TransformPoint(Vector3.zero);

        Vector3 b = new Vector3(target_b.position.x - target_origin.position.x, target_b.position.y - target_origin.position.y, target_b.position.z - target_origin.position.z);
        Vector3 a = new Vector3(target_a.position.x - target_origin.position.x, target_a.position.y - target_origin.position.y, target_a.position.z - target_origin.position.z);

        angle2 = Mathf.Acos(Vector3.Dot(a.normalized, b.normalized)) * Mathf.Rad2Deg;

        // Calculate the cross product to determine if it's on the left or right
        Vector3 cross = Vector3.Cross(a, b);

        // Check the sign of the cross product's y component
        //Debug.Log(angle2);
       
        return angle2;
    }
    void acceleration_controller()
    {
        if (front_bool_ary[1])
        {
            Vector3 rayOrigin = ray_forward.position;
            Vector3 rayDirection = ray_forward.forward;

            Ray ray = new Ray(rayOrigin, rayDirection);

            // Declare a RaycastHit variable to store information about the hit
            RaycastHit hit;

            // Perform the raycast and check if it hits something
            if (Physics.Raycast(ray, out hit, raycast_length))
            {
                // Check if the hit object is on either layer "a" or "b"
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Car") || hit.collider.gameObject.layer == LayerMask.NameToLayer("blocked"))
                {
                    // Handle the hit here (e.g., print or do something with the hit object)
                    //Debug.Log(childTransform.name + " hit on layer " + hit.collider.gameObject.layer + " at point: " + hit.point);
                    //if(hit.collider.gameObject.name == "car rear")
                    //{

                    //}
                    float distance = Vector3.Distance(transform.position, hit.collider.transform.position);

                    if (distance > 4f)
                    {
                        gameObject.GetComponent<carController>().verticalInput += -1f * Time.deltaTime;
                        if (gameObject.GetComponent<carController>().verticalInput < -1)
                        {
                            gameObject.GetComponent<carController>().verticalInput = -1;
                        }
                    }
                    else if (distance > 2f)
                    {
                        gameObject.GetComponent<carController>().isBreaking = true;

                    }
                    else
                    {
                        gameObject.GetComponent<carController>().verticalInput += -1f * Time.deltaTime;
                        if (gameObject.GetComponent<carController>().verticalInput < -1)
                        {
                            gameObject.GetComponent<carController>().verticalInput = -1;
                        }
                    }
                }
            }

        }
        else
        {
            if (nav_count_now + saki_yomi < shortestPath.Count)
            {
                if (30f < target_angle_calculator2(navigator_manager.GetComponent<Nav_manager>().find_pos_of_grid(shortestPath[nav_count_now + saki_yomi].Item1, shortestPath[nav_count_now + saki_yomi].Item2), navigator_manager.GetComponent<Nav_manager>().find_pos_of_grid(shortestPath[nav_count_now].Item1, shortestPath[nav_count_now].Item2), transform))
                {
                    current_max_speed = slow_max_speed;
                }
                else
                {
                    current_max_speed = max_speed;
                }
               // blueball.position = navigator_manager.GetComponent<Nav_manager>().find_pos_of_grid(shortestPath[nav_count_now + saki_yomi].Item1, shortestPath[nav_count_now + saki_yomi].Item2).position;

            }
            else
            {
                current_max_speed = slow_max_speed;
            }


            gameObject.GetComponent<carController>().isBreaking = false;
            
            move_forward();
        }
    }
    void front_eyes()
    {
        front_bool_ary[0] = PerformRaycastFromTransform(ray_left);
        front_bool_ary[1] = PerformRaycastFromTransform(ray_forward);
        front_bool_ary[2] = PerformRaycastFromTransform(ray_right);

    }
    void move_forward()
    {
        //print(gameObject.GetComponent<Rigidbody>().velocity.magnitude);

        if (!stop_accelerating)
        {
            //print(gameObject.GetComponent<Rigidbody>().velocity.magnitude);
            if (gameObject.GetComponent<Rigidbody>().velocity.magnitude <= current_max_speed)
            {
                gameObject.GetComponent<carController>().verticalInput += 1f * Time.deltaTime;
                gameObject.GetComponent<carController>().verticalInput = Mathf.Clamp01(gameObject.GetComponent<carController>().verticalInput);
            }
            else
            {
                gameObject.GetComponent<carController>().verticalInput -= 1f * Time.deltaTime;
                gameObject.GetComponent<carController>().verticalInput = Mathf.Clamp01(gameObject.GetComponent<carController>().verticalInput);
            }
        }
        
    }


    bool PerformRaycastFromTransform(Transform childTransform)
    {
        if (childTransform == null)
        {
            Debug.LogError("Child transform not found.");
            return false;
        }

        // Calculate the ray's origin and direction
        Vector3 rayOrigin = childTransform.position;
        Vector3 rayDirection = childTransform.forward;
        Ray ray = new Ray(rayOrigin, rayDirection);


        RaycastHit hit;
        // Perform the raycast
        if (Physics.Raycast(ray, out hit, raycast_length))
        {


            // Iterate through the raycast hits

            // Check if the hit object is on either layer "a" or "b"
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("blocked") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Car"))
            {
                if (childTransform.gameObject.name == "left")
                {
                    ray_left_dist = Vector3.Distance(childTransform.position, hit.collider.transform.position);
                }
                else if (childTransform.gameObject.name == "right")
                {
                    ray_right_dist = Vector3.Distance(childTransform.position, hit.collider.transform.position);
                }
                // Handle the hit here (e.g., print or do something with the hit object)
                //Debug.Log(childTransform.name + " hit on layer " + hit.collider.gameObject.layer + " at point: " + hit.point);
                return true;
            }
        }
        return false;
    }


}
