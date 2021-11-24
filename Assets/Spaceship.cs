using System.Collections;
using TMPro;
using UnityEngine;


public class Spaceship : MonoBehaviour
{
    [SerializeField] private Transform forwardTransform;

    private Rigidbody _rigidbody;
    [SerializeField] private float _forceMultiplier = 2;
    [SerializeField] private bool _isConnected;
    [SerializeField] private bool _isMoving;
    [SerializeField] private int _delayTime = 7;

    public TextMeshPro countdownText;
    [SerializeField] private CameraFollower _cameraFollower;
    [SerializeField] private bool _isThrusting;
    [SerializeField] private bool _isCentered;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.All))
        {
            _isCentered = true;
        }

        if (!_isMoving || !_isCentered)
            return;

        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.All))
        {
            Thrust();
        }

        float forceMultiplier = _isThrusting ? _forceMultiplier * _forceMultiplier : _forceMultiplier;

        _rigidbody.AddForce(transform.forward * forceMultiplier);
    }

    [ContextMenu("Test Connect")]
    public void TestConnect()
    {
        SetConneted(true);
    }

    public void SetConneted(bool isConnected)
    {
        if (isConnected == _isConnected)
            return;

        _isConnected = isConnected;
        if (isConnected)
            StartCoroutine(CountdownAndMove());
        else
            _isMoving = false;
    }

    private IEnumerator CountdownAndMove()
    {
        for (int i = _delayTime - 1; i >= 0; i--)
        {
            yield return new WaitForSeconds(1);
            countdownText.text = i.ToString();
        }

        countdownText.gameObject.SetActive(false);

        _isMoving = true;
    }

    public void Thrust()
    {
        if (_isThrusting)
            return;

        _isThrusting = true;

        _cameraFollower.ZoomOut(() => { _isThrusting = false;});
    }
}