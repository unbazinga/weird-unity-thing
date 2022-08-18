using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    public float pickupDistance, pickupRadius;
    public int weaponLayer;

    public Transform weaponHolder, playerCamera, orientation;
    public StrengthBarHandler StrengthBarHandler;
    private bool _isWeaponHeld;
    private BaseWeapon _heldWeapon;
    private float throwPowerOverTime;
    public float throwPowerMax = .8f;
    public float timeForUIToFadeOut;
    private bool shouldFadeOut, shouldFadeIn;

    private void Awake()
    {
        StrengthBarHandler.maxStrength = throwPowerMax;
    }

    private void Update()
    {
        if (_isWeaponHeld)
        {
            
            if (Input.GetKey(KeyCode.Q))
            {
                shouldFadeIn = true;
                if (throwPowerOverTime < throwPowerMax)
                    throwPowerOverTime += (Time.deltaTime * 1.5f);
                else
                    throwPowerOverTime = throwPowerMax;
                StrengthBarHandler.UpdateBar(throwPowerOverTime);
            }
            if (Input.GetKeyUp(KeyCode.Q))
            {
                _heldWeapon.Drop(playerCamera, throwPowerOverTime);
                _heldWeapon = null;
                _isWeaponHeld = false;
                throwPowerOverTime = 0f;
                StrengthBarHandler.ResetBar();
                shouldFadeOut = true;

            }
        } else if (Input.GetKeyDown(KeyCode.E))
        {
            var hitList = new RaycastHit[256];
            var hitNum = Physics.CapsuleCastNonAlloc(playerCamera.position,
                playerCamera.position + playerCamera.forward * pickupDistance, pickupRadius, playerCamera.forward,
                hitList);

            var tList = new List<RaycastHit>();
            for (int i = 0; i < hitNum; i++)
            {
                var hit = hitList[i];
                if (hit.transform.gameObject.layer != weaponLayer) continue;
                if (hit.point == Vector3.zero)
                {
                    tList.Add(hit);
                } else if (Physics.Raycast(playerCamera.position, hit.point - playerCamera.position, out var hitInfo,
                               hit.distance + 0.1f) && hitInfo.transform == hit.transform)
                {
                    tList.Add(hit);
                }
            }

            if (tList.Count == 0) return;
            
            tList.Sort((hit1, hit2) =>
            {
                var dist1 = GetDistanceTo(hit1);
                var dist2 = GetDistanceTo(hit2);
                return Mathf.Abs(dist1 - dist2) < 0.001f ? 0 : dist1 < dist2 ? -1 : 1;
            });

            _isWeaponHeld = true;
            _heldWeapon = tList[0].transform.GetComponent<BaseWeapon>();
            _heldWeapon.PickUp(weaponHolder);
            
        }

        if (shouldFadeIn)
        {
            StrengthBarHandler.FadeIn(.35f);
            shouldFadeIn = false;
        } else if (shouldFadeOut)
        {
            StrengthBarHandler.StartCoroutine(StrengthBarHandler.FadeOut(.35f, timeForUIToFadeOut));
            shouldFadeOut = false;
        }
    }

    private float GetDistanceTo(RaycastHit raycastHit)
    {
        return Vector3.Distance(playerCamera.position,
            raycastHit.point == Vector3.zero ? raycastHit.transform.position : raycastHit.point);
    }
}