
using UnityEngine;
using UnityEngine.InputSystem;
using VehiclePhysics;

public class CheckpointController : MonoBehaviour
{
    [HideInInspector]
    public int checkpointId;

    private bool _addTarget = false;
    private Vector2 _targetRange = new Vector2(-0.5f, 0.5f);
    private MeshFilter _targetBox;
    private GameObject _targetPoint;

    public BoxCollider boxCollider { get; private set; }

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        _targetBox = GetComponentInChildren<MeshFilter>(true);
        _targetBox.GetComponent<MeshRenderer>().enabled = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MeshCollider>() != null)
        {
            Vector3 contactPoint = boxCollider.ClosestPointOnBounds(other.transform.position);
            contactPoint = transform.InverseTransformPoint(contactPoint);
            CarController car = other.GetComponentInParent<CarController>();
            if (car != null)
            {
                GameController.Instance.OnCheckpoint(car, checkpointId, contactPoint);
            }
        }
    }

    public Vector3 GetClosestTarget(Vector3 position)
    {
        GameObject target = new GameObject();
        target.transform.SetParent(_targetBox.transform);
        target.transform.position = boxCollider.ClosestPoint(position); ;
        target.transform.localPosition = new Vector3(
            target.transform.localPosition.x,
            target.transform.localPosition.y,
            Mathf.Clamp(target.transform.localPosition.z, _targetRange.x, _targetRange.y));
        position = target.transform.position;
        Destroy(target);
        return position;
    }

    private void addTargetPoint()
    {
        if (_targetPoint == null)
        {
            if (_addTarget)
            {
                _targetPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _targetPoint.GetComponent<SphereCollider>().enabled = false;
                _targetPoint.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                _targetPoint = new GameObject();
            }
            _targetPoint.transform.SetParent(_targetBox.transform);
        }
    }

    private float getVariance()
    {
        return ((float)GameController.Instance.Random.NextDouble() - 0.5f) * 0.1f;
    }

    public Vector3 AddTarget(Vector3 position, Vector3 prevContact)
    {
        addTargetPoint();
        _targetPoint.transform.position = boxCollider.ClosestPoint(position);
        _targetPoint.transform.localPosition = new Vector3(
            _targetPoint.transform.localPosition.x,
            _targetPoint.transform.localPosition.y,
            Mathf.Clamp(prevContact.z + getVariance(), _targetRange.x, _targetRange.y));
        return _targetPoint.transform.position;
    }

    public Vector3 AddTarget(Vector3 position)
    {
        addTargetPoint();
        _targetPoint.transform.position = boxCollider.ClosestPoint(position);
        _targetPoint.transform.localPosition = new Vector3(
            _targetPoint.transform.localPosition.x,
            _targetPoint.transform.localPosition.y,
            Mathf.Clamp(_targetPoint.transform.localPosition.z + getVariance(), _targetRange.x, _targetRange.y));
        return _targetPoint.transform.position;
    }
}
