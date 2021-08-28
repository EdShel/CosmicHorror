using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class TurretLaser : MonoBehaviour
{
    public float MaxDistance = 100f;
    public Transform GunTip;

    public float ChargeTime = 0.5f;
    public float Recoil = 100f;

    private LineRenderer lineRenderer;
    private new Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        this.lineRenderer = GetComponent<LineRenderer>();
        this.rigidbody = GetComponent<Rigidbody>();

        StartCoroutine(CorShootLaser());
    }

    // Update is called once per frame
    void Update()
    {
        if (RaycastLaser(out var hitInfo))
        {
            this.lineRenderer.positionCount = 2;
            this.lineRenderer.SetPositions(new Vector3[]
            {
                GunTip.position, hitInfo.point
            });
        }
        else
        {
            this.lineRenderer.positionCount = 0;
        }
    }

    bool RaycastLaser(out RaycastHit hitInfo)
    {
        return Physics.Raycast(origin: GunTip.position, direction: GunTip.forward, maxDistance: MaxDistance, hitInfo: out hitInfo);
    }

    IEnumerator CorShootLaser()
    {
        while (true)
        {
            float time = 0f;
            while (time < this.ChargeTime)
            {
                float laserWidth = Mathf.SmoothStep(0.1f, 0.01f, time / this.ChargeTime);
                this.lineRenderer.startWidth = laserWidth;

                time += Time.deltaTime;
                yield return null;
            }

            // Shoot
            this.rigidbody.AddForceAtPosition((transform.forward - transform.up).normalized * (-this.Recoil), GunTip.position);

            time = 0f;
            while (time < this.ChargeTime)
            {
                float laserWidth = Mathf.SmoothStep(0.01f, 0.1f, time / 0.1f);
                this.lineRenderer.startWidth = laserWidth;

                time += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
