using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float BridgeCarLeftX = -25.5f;
    private const float BridgeCarRightX = 25.5f;
    private readonly List<BridgeCar> bridgeCars = new List<BridgeCar>();
    private Transform bridgeTrafficRoot;
    private float nextBridgeCarAt;

    private sealed class BridgeCar
    {
        public Transform Root;
        public Transform[] Wheels;
        public float Speed;
        public float Direction;
        public float BaseY;
        public float BobSeed;
    }

    private void CreateBridgeTraffic()
    {
        if (worldRoot == null || bridgeTrafficRoot != null)
        {
            return;
        }

        bridgeTrafficRoot = new GameObject("Bridge Passing Traffic").transform;
        bridgeTrafficRoot.SetParent(worldRoot, false);
        ScheduleNextBridgeCar(8f, 22f);
    }

    private void UpdateBridgeTraffic(float deltaTime)
    {
        if (bridgeTrafficRoot == null)
        {
            return;
        }

        if (Time.time >= nextBridgeCarAt && bridgeCars.Count == 0)
        {
            SpawnBridgeCar();
            ScheduleNextBridgeCar(12f, 38f);
        }

        for (int i = bridgeCars.Count - 1; i >= 0; i--)
        {
            BridgeCar car = bridgeCars[i];
            if (car.Root == null)
            {
                bridgeCars.RemoveAt(i);
                continue;
            }

            Vector3 pos = car.Root.localPosition;
            pos.x += car.Speed * car.Direction * deltaTime;
            pos.y = car.BaseY + Mathf.Sin(Time.time * 5.4f + car.BobSeed) * 0.012f;
            car.Root.localPosition = pos;
            RotateBridgeCarWheels(car, deltaTime);

            if ((car.Direction > 0f && pos.x > BridgeCarRightX) || (car.Direction < 0f && pos.x < BridgeCarLeftX))
            {
                Destroy(car.Root.gameObject);
                bridgeCars.RemoveAt(i);
            }
        }
    }

    private void SpawnBridgeCar()
    {
        float direction = Random.value > 0.5f ? 1f : -1f;
        float z = Random.value > 0.5f ? 1.0f : 2.05f;
        float y = 3.57f;
        GameObject rootObject = new GameObject("Passing low-poly car");
        rootObject.transform.SetParent(bridgeTrafficRoot, false);
        rootObject.transform.localPosition = new Vector3(direction > 0f ? BridgeCarLeftX : BridgeCarRightX, y, z);
        rootObject.transform.localRotation = direction > 0f ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);

        Color bodyColor = BridgeCarBodyColor();
        string bodyKey = "bridge_car_body_" +
            Mathf.RoundToInt(bodyColor.r * 255f) + "_" +
            Mathf.RoundToInt(bodyColor.g * 255f) + "_" +
            Mathf.RoundToInt(bodyColor.b * 255f);
        Material body = Mat(bodyKey, bodyColor);
        Material dark = Mat("bridge_car_dark", new Color(0.025f, 0.027f, 0.030f));
        Material glass = Mat("bridge_car_glass", new Color(0.08f, 0.12f, 0.16f));
        Material head = EmissiveMat("bridge_car_headlights", new Color(1f, 0.86f, 0.55f), 1.1f);
        Material tail = EmissiveMat("bridge_car_taillights", new Color(1f, 0.10f, 0.06f), 0.85f);

        TrafficBox("Car body", rootObject.transform, new Vector3(0f, 0.24f, 0f), new Vector3(1.12f, 0.28f, 0.50f), body);
        TrafficBox("Car cabin", rootObject.transform, new Vector3(-0.08f, 0.48f, 0f), new Vector3(0.48f, 0.26f, 0.42f), body);
        TrafficBox("Car windshield", rootObject.transform, new Vector3(0.20f, 0.50f, -0.215f), new Vector3(0.18f, 0.15f, 0.025f), glass);
        TrafficBox("Car headlight L", rootObject.transform, new Vector3(0.58f, 0.27f, -0.19f), new Vector3(0.055f, 0.08f, 0.04f), head);
        TrafficBox("Car headlight R", rootObject.transform, new Vector3(0.58f, 0.27f, 0.19f), new Vector3(0.055f, 0.08f, 0.04f), head);
        TrafficBox("Car taillight L", rootObject.transform, new Vector3(-0.58f, 0.26f, -0.19f), new Vector3(0.045f, 0.07f, 0.04f), tail);
        TrafficBox("Car taillight R", rootObject.transform, new Vector3(-0.58f, 0.26f, 0.19f), new Vector3(0.045f, 0.07f, 0.04f), tail);

        Transform[] wheels = CreateBridgeCarWheels(rootObject.transform, dark);
        CreateBridgeCarHeadlights(rootObject.transform, z);
        AttachBridgeCarEngineAudio(rootObject.transform);

        bridgeCars.Add(new BridgeCar
        {
            Root = rootObject.transform,
            Wheels = wheels,
            Speed = Random.Range(3.2f, 5.4f),
            Direction = direction,
            BaseY = y,
            BobSeed = Random.Range(0f, 20f)
        });
    }

    private void ScheduleNextBridgeCar(float minDelay, float maxDelay)
    {
        float roll = Random.value;
        float delay = Random.Range(minDelay, maxDelay);
        if (roll < 0.24f)
        {
            delay += Random.Range(14f, 34f);
        }
        else if (roll > 0.92f)
        {
            delay = Random.Range(5.0f, 8.5f);
        }

        nextBridgeCarAt = Time.time + delay;
    }

    private void CreateBridgeCarHeadlights(Transform root, float laneZ)
    {
        AddCarHeadlight(root, "Headlight sweep L", new Vector3(0.74f, 0.24f, -0.34f), laneZ, -0.22f);
        AddCarHeadlight(root, "Headlight sweep R", new Vector3(0.74f, 0.24f, 0.34f), laneZ, 0.22f);
    }

    private void AddCarHeadlight(Transform root, string name, Vector3 localSource, float laneZ, float sideOffset)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(root, false);
        lightObject.transform.localPosition = localSource + new Vector3(0.12f, -0.06f, -laneZ - 0.22f + sideOffset);

        Vector3 direction = new Vector3(0.42f, -1.0f, -0.44f + sideOffset * 0.12f).normalized;
        lightObject.transform.localRotation = Quaternion.LookRotation(direction, Vector3.up);

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = new Color(1f, 0.82f, 0.48f);
        light.range = 15.5f;
        light.intensity = 8.4f;
        light.spotAngle = 56f;
        light.innerSpotAngle = 19f;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.86f;
        light.shadowBias = 0.008f;
        light.shadowNormalBias = 0.045f;
        light.shadowNearPlane = 0.04f;
        light.renderMode = LightRenderMode.ForcePixel;
    }

    private Transform[] CreateBridgeCarWheels(Transform root, Material material)
    {
        Vector3[] positions =
        {
            new Vector3(-0.36f, 0.10f, -0.24f),
            new Vector3(0.36f, 0.10f, -0.24f),
            new Vector3(-0.36f, 0.10f, 0.24f),
            new Vector3(0.36f, 0.10f, 0.24f)
        };
        Transform[] wheels = new Transform[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject wheel = TrafficCylinder("Car wheel " + i, root, positions[i], new Vector3(0.105f, 0.105f, 0.07f), material);
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            wheels[i] = wheel.transform;
        }

        return wheels;
    }

    private void RotateBridgeCarWheels(BridgeCar car, float deltaTime)
    {
        if (car.Wheels == null)
        {
            return;
        }

        float spin = -car.Speed * deltaTime * 360f;
        for (int i = 0; i < car.Wheels.Length; i++)
        {
            if (car.Wheels[i] != null)
            {
                car.Wheels[i].Rotate(Vector3.forward, spin, Space.Self);
            }
        }
    }

    private GameObject TrafficBox(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject item = CreateBox(name, parent, position, scale, material);
        StripTrafficCollider(item);
        item.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        return item;
    }

    private GameObject TrafficCylinder(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject item = CreateCylinder(name, parent, position, scale, material);
        StripTrafficCollider(item);
        item.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        return item;
    }

    private void StripTrafficCollider(GameObject item)
    {
        Collider collider = item.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }

    private Color BridgeCarBodyColor()
    {
        Color[] colors =
        {
            new Color(0.82f, 0.16f, 0.10f),
            new Color(0.95f, 0.72f, 0.18f),
            new Color(0.20f, 0.58f, 0.78f),
            new Color(0.18f, 0.62f, 0.36f),
            new Color(0.82f, 0.82f, 0.72f),
            new Color(0.62f, 0.28f, 0.82f),
            new Color(0.72f, 0.36f, 0.18f)
        };
        return colors[Random.Range(0, colors.Length)];
    }
}
