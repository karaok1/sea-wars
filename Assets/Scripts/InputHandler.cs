using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Netcode;

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

    private bool EnsurePlayerFound()
    {
        if (player != null && clickToMove != null) return true;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClient?.PlayerObject != null)
        {
            var netPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
            player = netPlayer;
            clickToMove = netPlayer.GetComponent<ClickToMove>();
        }
        
        if (clickToMove == null)
        {
            clickToMove = FindFirstObjectByType<ClickToMove>();
            if (clickToMove != null && player == null)
            {
                player = clickToMove.gameObject;
            }
        }

        if (player == null && clickToMove != null)
        {
            player = clickToMove.gameObject;
        }
        
        return player != null && clickToMove != null;
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
            if (EnsurePlayerFound())
            {
                Vector3 mousePosition = Mouse.current.position.ReadValue();
                mousePosition.z = -_mainCamera.transform.position.z;
                Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
                clickToMove.OnClick(worldPosition);
            }
        }
    }
    
    public void OnSpace(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        if (IsClickOverUI()) return;

        if (!EnsurePlayerFound()) return;

        var target = gameManager.GetComponent<SelectObject>().selectedNPC;
        player.GetComponent<Cannon>().StartShooting(target);
    }

    public void OnShootButtonPress()
    {
        if (!EnsurePlayerFound()) return;
        
        var target = gameManager.GetComponent<SelectObject>().selectedNPC;
        player.GetComponent<Cannon>().StartShooting(target);
        startShootingButton.GetComponent<Image>().color = Color.red;
    }
    
    public void OnStopShootButtonPress()
    {
        if (!EnsurePlayerFound()) return;
        
        player.GetComponent<Cannon>().StopShooting();
        startShootingButton.GetComponent<Image>().color = Color.white;
    }
}