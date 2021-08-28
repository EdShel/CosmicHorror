using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    public float GrabMaxDistance = 1;

    public float HoldDistance = 1;

    public float AccelerateSpeed = 1;
    public float DontApplyForceDistance = 0.05f;
    public float NormalizationSpeed = 1;

    public float ThrowForce = 100f;

    public Rigidbody GrabbedItem;
    public Vector3 GrabPoint;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var lookRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.DrawLine(lookRay.origin, lookRay.GetPoint(GrabMaxDistance), Color.yellow, 1f);
            if (GrabbedItem == null)
            {
                if (Physics.Raycast(lookRay, out var hit, GrabMaxDistance) && (hit.rigidbody?.useGravity ?? false))
                {
                    GrabbedItem = hit.rigidbody;
                    GrabPoint = GrabbedItem.transform.InverseTransformPoint(hit.point);
                }

            }
            else
            {
                GrabbedItem.transform.parent = this.transform.parent;
                GrabbedItem.useGravity = true;
                var grabWorldPosition = GrabbedItem.transform.TransformPoint(GrabPoint);
                GrabbedItem.AddForceAtPosition(lookRay.direction * ThrowForce, grabWorldPosition);
                GrabbedItem = null;
            }
        }
        if (Input.GetButtonDown("Fire2"))
        {
            if (GrabbedItem != null)
            {
                GrabbedItem.transform.parent = this.transform.parent;
                GrabbedItem.useGravity = true;
                GrabbedItem = null;
            }
        }
    }

    private IEnumerator CorGiveAccurateCollisionUntilStops(Rigidbody obj)
    {
        while (!obj.IsSleeping())
        {
            yield return new WaitForFixedUpdate();
        }
        obj.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    private void FixedUpdate()
    {
        var lookRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (GrabbedItem != null)
        {
            var idealHoldPosition = lookRay.GetPoint(HoldDistance);
            var grabWorldPosition = GrabbedItem.transform.TransformPoint(GrabPoint);
            if ((idealHoldPosition - grabWorldPosition).sqrMagnitude > DontApplyForceDistance * DontApplyForceDistance)
            {
                GrabbedItem.useGravity = true;
                GrabbedItem.AddForceAtPosition((idealHoldPosition - grabWorldPosition).normalized * AccelerateSpeed, grabWorldPosition, ForceMode.Force);
                GrabbedItem.transform.parent = this.transform.parent;
            }
            else
            {
                //GrabbedItem.useGravity = false;
                GrabbedItem.velocity = Vector3.zero;
                GrabbedItem.angularVelocity = Vector3.zero;
                GrabPoint = Vector3.Lerp(GrabPoint, GrabbedItem.centerOfMass, NormalizationSpeed);
                //GrabbedItem.rotation = Quaternion.RotateTowards(GrabbedItem.rotation, Quaternion.identity, NormalizationSpeed);
                GrabbedItem.position = Vector3.Lerp(GrabbedItem.position, GrabbedItem.position + (idealHoldPosition - grabWorldPosition), NormalizationSpeed * GrabbedItem.mass);
                GrabbedItem.transform.parent = this.transform;
            }

            Debug.DrawLine(grabWorldPosition, idealHoldPosition, Color.red);
            Debug.DrawLine(lookRay.origin, idealHoldPosition, Color.blue);
        }
    }
}
