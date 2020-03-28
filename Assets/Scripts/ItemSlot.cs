using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Inventory;

public class ItemSlot : MonoBehaviour
{
    public ResourceType type = ResourceType.NONE;
    public int count;
    float maxWidth;
    float fillPercent = 0;

    Transform fillBar;
    Transform fillBG;

    public bool selected;
    public GameObject icon;

    private void Start()
    {
        fillBG = transform.GetChild(0);
        fillBar = transform.GetChild(1);
        maxWidth = fillBar.localScale.x;
    }

    Vector3 SmoothVector(Vector3 a, Vector3 b, float t)
    {
        float x = Mathf.SmoothStep(a.x, b.x, t);
        float y = Mathf.SmoothStep(a.y, b.y, t);
        float z = Mathf.SmoothStep(a.z, b.z, t);
        return new Vector3(x, y, z);
    }

    public static void Swap(ItemSlot a, ItemSlot b, Dictionary<ResourceType, ItemStackData> slotData)
    {
        int temp = a.count;
        a.count = b.count;
        b.count = temp;
        ResourceType temp2 = a.type;
        a.type = b.type;
        b.type = temp2;

        a.Recalculate(slotData);
        b.Recalculate(slotData);
    }

    public void Recalculate(Dictionary<ResourceType, ItemStackData> slotData)
    {
        if (icon != null) Destroy(icon);
        if(slotData.TryGetValue(type, out ItemStackData stackData))
        {
            fillPercent = (float) count / stackData.maxStack;
            icon = Instantiate(stackData.prefab, transform);
        } else
        {
            fillPercent = 0;
        }
    }

    private void Update()
    {
        if(selected)
        {
            Color newColor;
            Vector3 rgb = SmoothVector(new Vector3(0, 1, 0), new Vector3(1, 1, 1), .5f * (Mathf.Sin(Time.time * 4) + 1));
            newColor.r = rgb.x;
            newColor.g = rgb.y;
            newColor.b = rgb.z;
            newColor.a = 1.0f;
            GetComponent<Image>().color = newColor;
        } else
        {
            GetComponent<Image>().color = Color.white;
        }
        if (fillPercent > 0)
        {
            Vector3 localScale = fillBar.transform.localScale;
            fillBar.transform.localScale = new Vector3(fillPercent * maxWidth, localScale.y, localScale.z);
            if (!fillBG.gameObject.activeSelf || !fillBar.gameObject.activeSelf)
            {
                fillBG.gameObject.SetActive(true);
                fillBar.gameObject.SetActive(true);
            }
        } else if(fillBG.gameObject.activeSelf || fillBar.gameObject.activeSelf)
        {
            fillBG.gameObject.SetActive(false);
            fillBar.gameObject.SetActive(false);
        }
    }

    public bool TestCollision(Vector2 pos)
    {
        Vector2 canvasSpace = pos * 128.0f;
        RectTransform rt = GetComponent<RectTransform>();
        float width = 40;
        float height = 80;
        Vector2 thisPos = rt.localPosition;
        return canvasSpace.x > thisPos.x - width / 2.0f && canvasSpace.x < thisPos.x + width / 2.0f &&
            canvasSpace.y > thisPos.y - height / 2.0f && canvasSpace.y < thisPos.y + height / 2.0f;
    }

    public bool TryAddItem(Inventory.ItemStackData stackData)
    {
        if (type == ResourceType.NONE)
        {
            icon = Instantiate(stackData.prefab, transform);
            Debug.Log(stackData.type + " " + type);
            type = stackData.type;
            Debug.Log(type);
            count++;
            fillPercent = (float)count / stackData.maxStack;
            return true;
        }
        if (stackData.type == type && count < stackData.maxStack)
        {
            count++;
            fillPercent = (float)count / stackData.maxStack;
            return true;
        }
        return false;
    }

    public void InstantiateIcon(GameObject iconPrefab)
    {
        Instantiate(iconPrefab, transform);
    }
}
