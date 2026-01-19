using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
    public GameObject player;
    
    private Camera _mainCamera;
    private ClickToMove clickToMove;

    public GameObject ui_canvas;
    private GraphicRaycaster ui_raycaster;

    private PointerEventData click_data;
    private List<RaycastResult> click_results;
    private GameManager gameManager;
    
    [SerializeField] private GameObject startShootingButton;
    [SerializeField] private GameObject stopShootingButton;
    [SerializeField] private GameObject repairButton;
    
    private void Awake()
    {
        _mainCamera = Camera.main;
        clickToMove = FindFirstObjectByType<ClickToMove>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        ui_raycaster = ui_canvas.GetComponent<GraphicRaycaster>();
        click_data = new PointerEventData(EventSystem.current);
        click_results = new List<RaycastResult>();
    }

    private bool IsClickOverUI()
    {
        click_data.position = Mouse.current.position.ReadValue();
        click_results.Clear();

        ui_raycaster.Raycast(click_data, click_results);

        return click_results.Count > 0;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        if (IsClickOverUI()) return;

        var rayHit = Physics2D.GetRayIntersection(_mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));

        if (rayHit.collider)
        {
            if (rayHit.collider.gameObject.CompareTag("NPC"))
            {
                SelectObject.Instance.Select(rayHit.collider.gameObject);
            }
        }
        else
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            mousePosition.z = -_mainCamera.transform.position.z;
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
            clickToMove.OnClick(worldPosition);
        }
    }
    
    public void OnSpace(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        if (IsClickOverUI()) return;

        var target = gameManager.GetComponent<SelectObject>().selectedNPC;
        player.GetComponent<Cannon>().StartShooting(target);
    }

    public void OnShootButtonPress()
    {
        var target = gameManager.GetComponent<SelectObject>().selectedNPC;
        player.GetComponent<Cannon>().StartShooting(target);
        startShootingButton.GetComponent<Image>().color = Color.red;
    }
    
    public void OnStopShootButtonPress()
    {
        player.GetComponent<Cannon>().StopShooting();
        startShootingButton.GetComponent<Image>().color = Color.white;
    }
}