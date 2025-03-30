using UnityEngine;
using System.IO.Ports;

public class MoveRight : MonoBehaviour
{
    SerialPort serialPort = new SerialPort("COM8", 9600); // 修改為右側感測器的 COM 連接埠
    public float baseMoveDistance = 0.5f;
    public float baseMoveSpeed = 5.0f;
    public float speedMultiplier = 1.0f;
    public float maxMultiplier = 5.0f;
    public float decayRate = 0.9f;
    
    private Vector3 targetPosition;
    private bool wasDetected = false;
    private float lastTriggerTime = 0.0f;
    private float timeThreshold = 0.3f;

    void Start()
    {
        try
        {
            serialPort.Open();
            serialPort.ReadTimeout = 100;
        }
        catch (System.Exception e)
        {
            Debug.LogError("無法開啟序列埠: " + e.Message);
        }

        targetPosition = transform.position;
    }

    void Update()
    {
        if (!serialPort.IsOpen) return;

        try
        {
            if (serialPort.BytesToRead > 0)
            {
                string data = serialPort.ReadLine().Trim();
                bool isDetected = (data == "1");

                if (isDetected && !wasDetected)
                {
                    float currentTime = Time.time;
                    if (currentTime - lastTriggerTime < timeThreshold)
                    {
                        speedMultiplier = Mathf.Min(speedMultiplier + 0.5f, maxMultiplier);
                    }
                    else
                    {
                        speedMultiplier = 1.0f;
                    }

                    lastTriggerTime = currentTime;
                    float actualMoveDistance = baseMoveDistance * speedMultiplier;
                    
                    // 向右前方移動
                    targetPosition = transform.position + new Vector3(actualMoveDistance, 0, actualMoveDistance);
                }

                wasDetected = isDetected;
            }
        }
        catch (System.Exception) { }

        float actualMoveSpeed = baseMoveSpeed * speedMultiplier * 10;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * actualMoveSpeed);

        if (!wasDetected)
        {
            speedMultiplier = Mathf.Max(speedMultiplier * decayRate, 1.0f);
        }
    }

    void OnApplicationQuit()
    {
        if (serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }
}