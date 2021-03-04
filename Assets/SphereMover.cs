using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMover : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    private Vector3 _initialPosition;

    public enum Move
    {
        None,
        Up,
        Down,
        Left,
        Right,
        Forward,
        Backward
    }

    private void Start()
    {
        _initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) ApplyMove(Move.Right);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) ApplyMove(Move.Left);
        if (Input.GetKeyDown(KeyCode.UpArrow)) ApplyMove(Move.Up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) ApplyMove(Move.Down);
        if (Input.GetKeyDown(KeyCode.Home)) ApplyMove(Move.Forward);
        if (Input.GetKeyDown(KeyCode.End)) ApplyMove(Move.Backward);
    }

    private void OnGUI()
    {
        var width = 300;
        if (GUI.Button(new Rect(Screen.width - width, 0, width, 50), "Reset Sphere"))
        {
            _rigidbody.angularVelocity = _rigidbody.velocity = Vector3.zero;
            transform.position = _initialPosition;

        }
    }

    public void ApplyMove(Move move)
    {
        Vector3 force;
        switch (move)
        {
            case Move.Up:
                force = Vector3.up;
                break;
            case Move.Down:
                force = Vector3.down;
                break;
            case Move.Left:
                force = Vector3.left;
                break;
            case Move.Right:
                force = Vector3.right;
                break;
            case Move.Forward:
                force = Vector3.forward;
                break;
            case Move.Backward:
                force = Vector3.back;
                break;
            case Move.None:
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(move), move, null);
        }

        _rigidbody.AddForce(force, ForceMode.Impulse);
    }
}
