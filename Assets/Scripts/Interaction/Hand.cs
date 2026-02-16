using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class Hand : MonoBehaviour
{    
    [SerializeField]
    private ActionBasedController controller;

    [SerializeField] 
    private float followSpeed = 30.0f;

    [SerializeField] 
    private float rotationSpeed = 100.0f;

    [SerializeField] 
    private Vector3 positionOffset;

    [SerializeField] 
    private Vector3 rotationOffset;

    [SerializeField]
    private Transform palm;

    [SerializeField]
    private float reachDistance = 0.1f;

    [SerializeField]
    private float joinDistance = 0.05f;

    [SerializeField]
    private LayerMask grabbablesLayer;

    private Transform _followTarget;
    private Rigidbody _rb;

    private bool _isGrabbing;
    private GameObject _heldObject;
    private Transform _grabPoint;
    private FixedJoint _firstJoint;
    private FixedJoint _secondJoint;

    private void Start()
    {
        _followTarget = controller.gameObject.transform;

        //Setup rigidbody
        _rb = GetComponent<Rigidbody>();
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.mass = 20.0f;
        _rb.maxAngularVelocity = 20.0f;
        //Setup controller
        controller.selectAction.action.started += Grab;
        controller.selectAction.action.canceled += Release;

        //Update hands to match controller
        _rb.position = _followTarget.position;
        _rb.rotation = _followTarget.rotation;
    }

    private void Update()
    {
        // Update position
        Vector3 worldPos = _followTarget.TransformPoint(positionOffset);
        float distance = Vector3.Distance(worldPos, transform.position);
        _rb.velocity = (worldPos - transform.position).normalized * (followSpeed * distance);

        // Update rotation
        Quaternion worldRot = _followTarget.rotation * Quaternion.Euler(rotationOffset);
        Quaternion q = worldRot * Quaternion.Inverse(_rb.rotation);
        q.ToAngleAxis(out float angle, out Vector3 axis);
        _rb.angularVelocity = axis * (angle * Mathf.Deg2Rad * rotationSpeed);
    }

    void Grab(InputAction.CallbackContext context) {
        if (_isGrabbing || _heldObject)
            return;

        Collider[] colliders = Physics.OverlapSphere(palm.position,
            reachDistance,
            grabbablesLayer);

        //Check if we have colliders
        if (colliders.Length < 1)
            return;

        GameObject grabbedObject = colliders[0].transform.gameObject;
        Rigidbody objectRb = grabbedObject.GetComponent<Rigidbody>();

        //Check if target has rigidbody
        if (objectRb != null) 
        {
            _heldObject = objectRb.gameObject;
        }
        else
        {
            objectRb = grabbedObject.GetComponentInParent<Rigidbody>();
            if (objectRb == null) 
                return;

            _heldObject = objectRb.gameObject;
        }
        
        StartCoroutine(GrabObject(colliders[0], objectRb));
    }

    void Release(InputAction.CallbackContext context)
    {
        // Release joints and grab point
        if (_firstJoint != null) Destroy(_firstJoint);
        if (_secondJoint != null) Destroy(_secondJoint);
        if (_grabPoint != null) Destroy(_grabPoint.gameObject);

        if (_heldObject != null) 
        {
            Rigidbody targetRb = _heldObject.GetComponent<Rigidbody>();
            targetRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            targetRb.interpolation = RigidbodyInterpolation.None;
            _heldObject = null;
        }

        _isGrabbing = false;
        _followTarget = controller.gameObject.transform;
    }
    

    IEnumerator GrabObject(Collider collider, Rigidbody targetBody)
    {
        _isGrabbing = true;
        
        //Create grab point
        _grabPoint = new GameObject().transform;
        _grabPoint.position = collider.ClosestPoint(palm.position);
        _grabPoint.parent = _heldObject.transform;

        //Move to grab point
        _followTarget = _grabPoint;

        // Wait for hand to reach grab point
        while (_grabPoint != null 
                && Vector3.Distance(_grabPoint.position, palm.position) > joinDistance 
                && _isGrabbing)
            yield return new WaitForEndOfFrame();

        //Freeze hand and object motion
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        targetBody.velocity = Vector3.zero;
        targetBody.angularVelocity = Vector3.zero;

        targetBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        targetBody.interpolation = RigidbodyInterpolation.Interpolate;

        //Attach joints
        _firstJoint = gameObject.AddComponent<FixedJoint>();
        _firstJoint.connectedBody = targetBody;
        _firstJoint.breakForce = float.PositiveInfinity;
        _firstJoint.breakTorque = float.PositiveInfinity;
        _firstJoint.connectedMassScale = 1;
        _firstJoint.massScale = 1;
        _firstJoint.enableCollision = false;
        _firstJoint.enablePreprocessing = false;
        
        _secondJoint = _heldObject.AddComponent<FixedJoint>();
        _secondJoint.connectedBody = _rb;
        _secondJoint.breakForce = float.PositiveInfinity;
        _secondJoint.breakTorque = float.PositiveInfinity;
        _secondJoint.connectedMassScale = 1;
        _secondJoint.massScale = 1;
        _secondJoint.enableCollision = false;
        _secondJoint.enablePreprocessing = false;

        //Reset follow target
        _followTarget = controller.gameObject.transform;
    }
}