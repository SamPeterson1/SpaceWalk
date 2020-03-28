using UnityEngine;

public class TetherConnection : MonoBehaviour
{
    public Tether tetherA;
    public Tether tetherB;
    public Color colorA;
    public Color colorB;
    [Range(0, 10)]
    public float rate;
    private Material material;
    private MeshRenderer meshRenderer;
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (tetherA == null || tetherB == null) {
            Destroy(gameObject);
            return;
        }

        if (!tetherA.tetherNode.network.InRange(tetherA.tetherNode, tetherB.tetherNode))
        {
            meshRenderer.enabled = false;
        } else
        {
            meshRenderer.enabled = true;
        }

        float emissionMultiplier;
        float colorMultiplier;
        if(tetherA.tetherNode.hasOxygen || tetherB.tetherNode.hasOxygen)
        {
            emissionMultiplier = 0.5f;
            colorMultiplier = 1.0f;
        } else
        {
            Debug.Log("NO O2");
            colorMultiplier = 0.3f;
            emissionMultiplier = 0.0f;
        }
        

        transform.position = (tetherA.transform.position + tetherB.transform.position) / 2.0f;
        transform.LookAt(tetherA.transform);
        transform.Rotate(new Vector3(90, 0, 0));
        transform.localScale = new Vector3(transform.localScale.x, (tetherA.transform.position - tetherB.transform.position).magnitude / 2.0f, transform.localScale.z);
        
        float factor = Mathf.Clamp((Mathf.Sin(Time.time * rate) + 1.0f), 0, 1);
        Debug.Log(factor);
        Color finalColor = colorA * factor + colorB * (1 - factor);

        material.SetColor("_UnlitColor", finalColor * colorMultiplier);
        material.SetColor("_EmissiveColor", finalColor * emissionMultiplier);
    }
}
