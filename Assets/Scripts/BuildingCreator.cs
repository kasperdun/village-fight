using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingCreator : MonoBehaviour
{
    public float forceAmount = 500;
    public Camera targetCamera;
    public BuildingBase mobToCreate;

    public TerrainCollider terrain;
    public GameObject spawnZoneTop;
    public GameObject spawnZoneBottom;


    bool dragging = false;
    BuildingBase ghost;
    Renderer spawnZoneRenderer;

    public void Start()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties["Team"].ToString() == "Top")
        {
            spawnZoneRenderer = spawnZoneTop.GetComponent<Renderer>();
        }
        else
        {
            spawnZoneRenderer = spawnZoneBottom.GetComponent<Renderer>();
        }
    }

    public void OnStartDrag()
    {
        spawnZoneRenderer.gameObject.SetActive(true);

        StartCoroutine(CreateGhost());
        
        dragging = true;
        CameraHandler.DontMove = true;

    }

    IEnumerator CreateGhost()
    {
        yield return new WaitForSeconds(0.1f);
        ghost = Instantiate(mobToCreate, Vector3.zero, Quaternion.identity);
        ghost.IsGhost = true;
    }

    void Update()
    {
        if (!targetCamera)
            return;


        if (ghost != null && dragging && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
        {
            var position = ghost.transform.position;
            Destroy(ghost.gameObject);
            ghost = null;

            PhotonNetwork.Instantiate(Path.Combine("Prefabs", mobToCreate.name), position, Quaternion.identity);

            dragging = false;
            CameraHandler.DontMove = false;

            spawnZoneRenderer.gameObject.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (ghost != null && dragging)
        {
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;
            if (terrain.Raycast(ray, out hitInfo, Mathf.Infinity)) // using infinity for the ray length for example
            {
                Vector3 pos = hitInfo.point;
                pos.x = Mathf.Clamp(pos.x, spawnZoneRenderer.bounds.min.x, spawnZoneRenderer.bounds.max.x);
                pos.z = Mathf.Clamp(pos.z, spawnZoneRenderer.bounds.min.z, spawnZoneRenderer.bounds.max.z);
                pos.y = 0;

                ghost.transform.position = pos;
            }
        }
    }
}
