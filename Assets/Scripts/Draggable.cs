using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Draggable : MonoBehaviour
{
    [Header("-- Tuning --")]
    public float spring_constant;
    public float damping_constant;
    [Tooltip("Maximum distance the object can be pulled from its rest position. Set to 0 for no limit.")]
    public float max_distance = 0f;

    [Header("-- Debug --")]
    public bool draw_debug_input = false;
    public bool draw_debug_force = false;

    // --- Private Variables ---
    private Collider2D obj_collider;
    private Rigidbody2D obj_rigidbody;

    private bool is_holding;

    private Vector2 pos_mouse;      // World Position of Mouse
    private Vector2 pos_contact;    // Local Position of Contact Point on the Object
    private Vector2 displacement;   // Displacement from pos_contact to pos_mouse
    private Vector2 direction;      // Normalized Direction from pos_contact to pos_mouse
    private float prev_compression; // Compression of the spring in the previous frame
    private float curr_compression; // Compression of the spring in the current frame
    private Vector2 force_spring;   // Force applied by the spring
    private Vector2 force_damping;  // Force applied by the damper


    void Start()
    {
        // Initialize Variables
        is_holding = false;
        obj_collider = GetComponent<Collider2D>();
        obj_rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        UpdateIsHolding();
        if (is_holding) UpdateMousePosition();
    }

    void FixedUpdate()
    {
        if (is_holding) ApplyForceToObject();
    }

    private void UpdateIsHolding()
    {
        if (Input.GetMouseButtonDown(0)) {
            UpdateMousePosition();
            if (obj_collider.OverlapPoint(pos_mouse))
            {
                is_holding = true;
                pos_contact = transform.InverseTransformPoint(pos_mouse);
                prev_compression = 0f;
                curr_compression = 0f;
            }
        }
        else if (Input.GetMouseButtonUp(0)) is_holding = false;
    }

    private void UpdateMousePosition()
    {
        pos_mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void ApplyForceToObject()
    {
        displacement = pos_mouse - (Vector2)transform.TransformPoint(pos_contact);
        direction = displacement.normalized;

        if (max_distance > 0f && displacement.magnitude > max_distance)
        {
            is_holding = false;
            return;
        }

        prev_compression = curr_compression;
        curr_compression = 0f - displacement.magnitude;
        float relative_velocity = (curr_compression - prev_compression) / Time.fixedDeltaTime;

        force_spring = spring_constant * displacement;
        force_damping = -damping_constant * (relative_velocity * direction);
        Vector2 force = force_spring + force_damping;

        obj_rigidbody.AddForceAtPosition(force, transform.TransformPoint(pos_contact));
    }

    private void OnDrawGizmos()
    {
        if (!is_holding) return;

        if (draw_debug_input)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pos_mouse, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.TransformPoint(pos_contact), 0.1f);
            Gizmos.color = Color.red;
            if (max_distance > 0f)
            {
                Vector2 pos_break = (Vector2)transform.TransformPoint(pos_contact) + direction * max_distance;
                Gizmos.DrawSphere(pos_break, 0.1f);
            }
        }

        if (draw_debug_force)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.TransformPoint(pos_contact), force_spring / (obj_rigidbody.mass * 10f));
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.TransformPoint(pos_contact), force_damping / (obj_rigidbody.mass * 10f));
        }
    }    
}
