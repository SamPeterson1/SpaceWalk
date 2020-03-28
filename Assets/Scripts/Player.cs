using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float sensitivity;
    public float speed;

    public bool canMove;
    public float jumpForce;

    bool rising = false;
    bool falling = false;
    Vector3Int deltaChunk;

    Rigidbody body;

    Vector3Int chunkPos;

    Camera camera;
    Vector3 moveAmount;
    Vector3 speedOnJump;

    Vector3Int pastChunk;

    public GameObject teapot;
    public Inventory inventory;
    TerrainGenerator generator;
    TetherNetwork tetherNetwork;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        tetherNetwork = GameObject.FindGameObjectWithTag("Planet").GetComponent<TetherNetwork>();
        body = GetComponent<Rigidbody>();
        chunkPos = new Vector3Int(0, 0, 0);
        deltaChunk = new Vector3Int(0, 0, 0);
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        generator = GameObject.FindGameObjectWithTag("Generator").GetComponent<TerrainGenerator>();
    }

    public bool AddItem(ResourceType type)
    {
        return inventory.AddItem(type);
    }

    // Update is called once per frame
    void Update()
    {

        if (canMove)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                {
                    tetherNetwork.PlaceTether(hit.point + hit.point.normalized * 2, false);
                }
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                {
                    tetherNetwork.PlaceTether(hit.point + hit.point.normalized * 2, true);
                }
            }

            if (Input.GetKey(KeyCode.Q))
            {
                deform(1);
            }

            if (Input.GetKey(KeyCode.E))
            {
                deform(-1);
            }

            if (Input.GetKey(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }

            float jumpMultiplier = 1f;
            if (rising || falling) jumpMultiplier = 0.5f;
            moveAmount = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * speed * jumpMultiplier;
            if (Input.GetKeyDown(KeyCode.Space) && !(rising || falling))
            {
                jump();
                rising = true;
                falling = false;
            }

            if (rising && body.velocity.y < 0)
            {
                rising = false;
                falling = true;
            }

            Vector2 mousePos = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            float rotX = -mousePos.y * sensitivity;
            float rotY = mousePos.x * sensitivity;

            transform.Rotate(new Vector3(0, rotY, 0));
            camera.transform.Rotate(new Vector3(rotX, 0, 0));
        } else
        {
            moveAmount = Vector3.zero;
        }
    }
    private void FixedUpdate()
    {
        Vector3 momentum = speedOnJump * 0.25f;
        if (!(rising || falling)) momentum = new Vector3(0, 0, 0);
        body.MovePosition(body.position + transform.TransformDirection(moveAmount + momentum) * Time.fixedDeltaTime);
    }

    private void deform(int subtract)
    {
        int layerMask = 1 << 0;

        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit, 20, layerMask))
        {
            generator.Deform(hit.point, 5, subtract, hit.normal);
        } 
    }

    private void jump()
    {
        speedOnJump = moveAmount;
        Vector3 pointOnUnitSphere = transform.position.normalized;
        body.AddForce(pointOnUnitSphere * jumpForce);
    }

    public void readChunkData()
    {
        chunkPos = TerrainChunk.GetChunkFromPos(transform.position);
        deltaChunk = pastChunk - chunkPos;
    }

    public void readPastChunk()
    {
        pastChunk = TerrainChunk.GetChunkFromPos(transform.position);
    }

    public Vector3Int getDeltaChunk()
    {
        return deltaChunk;
    }

    public bool changedChunks()
    {
        return !deltaChunk.Equals(new Vector3Int(0,0,0));
    }

    public Vector3Int getChunkPosition()
    {
        return chunkPos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        falling = false;
    }
}
