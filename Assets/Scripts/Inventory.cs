using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{

    public struct ItemJSONData
    {
        public string prefab;
        public ResourceType type;
        public int maxStack;
    }

    public struct ItemStackData
    {
        public ResourceType type;
        public GameObject prefab;
        public int maxStack;
    }

    Dictionary<ResourceType, ItemStackData> itemIcons;
    public ItemSlot[] slots;
    public TextAsset iconsJSON;
    public Camera camera;
    public GameObject cube;
    public GameObject tabletScreen;
    public GameObject cursor;
    ItemSlot selected;

    // Start is called before the first frame update
    void Start()
    {
        slots = transform.GetComponentsInChildren<ItemSlot>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        LoadIcons(iconsJSON.text);

        
    }

    private void Update()
    {
        Vector2 screenPos = GetMousePosInInventory(out bool hitScreen, tabletScreen.transform);
        if (hitScreen)
        {
            cursor.SetActive(true);
            RectTransform rect = cursor.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = (screenPos + Vector2.one) / 2.0f;
            rect.anchorMax = (screenPos + Vector2.one) / 2.0f;
            Cursor.visible = false;

            if(Input.GetMouseButtonDown(0))
            {
                foreach (ItemSlot slot in slots)
                {
                    if (slot.TestCollision(screenPos))
                    {
                        slot.selected = true;
                        if (selected == null) selected = slot;
                        else
                        {
                            ItemSlot.Swap(selected, slot, itemIcons);
                            selected.selected = false;
                            slot.selected = false;
                            selected = null;
                        }
                    }
                }
            }

        } else if(Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.visible = true;
            cursor.SetActive(false);
        }
    }

    Vector2 GetMousePosInInventory(out bool hitScreen, Transform tabletPos)
    {
        Vector2 screenPoint = Input.mousePosition;
        Ray ray = camera.ScreenPointToRay(screenPoint);
        Debug.DrawRay(ray.origin, ray.direction);
        LayerMask layer = LayerMask.GetMask(new string[] { "UI" });
        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layer))
        {
            if (hit.collider.gameObject.CompareTag("Screen"))
            {
                Vector3 localHitPos = hit.point - tabletPos.position;
                cube.transform.position = transform.position;
                Vector3 screenHitPos = Matrix4x4.Rotate(Quaternion.Inverse(tabletPos.rotation)) * localHitPos;
                hitScreen = true;
                return new Vector2(2*screenHitPos.x/tabletPos.localScale.x, 2*screenHitPos.y/tabletPos.localScale.y);
            }
        }

        hitScreen = false;
        return new Vector2(0, 0);
    }

    // Update is called once per frame
    public bool AddItem(ResourceType type)
    {
        foreach(ItemSlot slot in slots)
        {
            if(itemIcons.TryGetValue(type, out ItemStackData stackData)) {
                if(slot.TryAddItem(stackData))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void LoadIcons(string json)
    {
        itemIcons = new Dictionary<ResourceType, ItemStackData>();
        json = json.Replace("\n", "");
        string[] array = json.Split(new string[] { "ITEM" }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string str in array)
        {
            ItemJSONData data = JsonUtility.FromJson<ItemJSONData>(str);
            GameObject iconPrebab = (GameObject)Resources.Load("Prefabs/" + data.prefab);
            ItemStackData stackData;
            stackData.prefab = iconPrebab;
            stackData.maxStack = data.maxStack;
            stackData.type = data.type;
            itemIcons.Add(stackData.type, stackData);
        }
    }
}
