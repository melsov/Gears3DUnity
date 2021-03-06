﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ViewControls : MonoBehaviour {

    public float zoomSpeed = 1.1f;
    public float panSpeed = 4f;
    private Camera cam;
    private Vector3 lastMouseGlobal;
    private readonly Vector3 _panScale = new Vector3(1f, 0f, 1f);
    private Vector3 targetPosition;

    void Awake () {
        cam = GetComponent<Camera>();
        cam.transform.position = TransformUtil.SetY(cam.transform.position, YLayer.camera);
        targetPosition = cam.transform.position;
	}
	
	void Update () {

        if (!EventSystem.current.IsPointerOverGameObject()) {
            /* scolling zooms in-out */
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll < -float.Epsilon || scroll > float.Epsilon) {
                if (mouseIsOverScreen())
                    cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + scroll * zoomSpeed *-1f, .45f, 40f);
            }

            /* MMB pans camera */
            if (Input.GetMouseButtonDown(2)) {
                targetPosition = cam.transform.position;
                lastMouseGlobal = cam.ScreenToWorldPoint(Input.mousePosition);
            }
            if (Input.GetMouseButton(2)) {
                Vector3 mouseGlobal = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3 nudge = (mouseGlobal - lastMouseGlobal);
                nudge.Scale(_panScale);
                targetPosition -= nudge;
                lastMouseGlobal = mouseGlobal;
            }
        }
        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPosition, panSpeed * Time.deltaTime);
	}

    private bool mouseIsOverScreen() {
        return Input.mousePosition.x > 0 && Input.mousePosition.y > 0 && Input.mousePosition.x < Screen.width && Input.mousePosition.y < Screen.height;
    }
}
