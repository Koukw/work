using UnityEngine;
using System.IO.Ports;

public class BoatMovement : MonoBehaviour
{
    SerialPort serialPort = new SerialPort("COM8", 9600);

    public float baseMoveDistance = 0.5f;   // 每次划槳的基本移動距離
    public float moveSpeed = 5.0f;          // 角色基本速度
    public float speedMultiplier = 1.0f;    // 速度增益
    public float maxMultiplier = 5.0f;      // 最大加速倍數
    public float decayRate = 0.9f;          // 速度衰減率
    public float rotationStep = 5f;

    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private float smoothTime = 0.3f;

    private bool previousLeftPaddle = false;
    private bool previousRightPaddle = false;
    
    private float currentRotation = 0f;  // 目前的旋轉角度
    private float targetRotation = 0f;   // 目標旋轉角度
    private float rotationSpeed = 2.0f;  // 旋轉速度

    void Start()
    {
        serialPort.Open();
        serialPort.ReadTimeout = 10;
        targetPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (!serialPort.IsOpen) return;

        try
        {
            if (serialPort.BytesToRead > 0)
            {
                string data = serialPort.ReadLine().Trim();
                bool isLeftPaddle = (data == "1");  // 左槳
                bool isRightPaddle = (data == "2"); // 右槳

                // **只有當狀態從未偵測 → 偵測到時才觸發**
                if ((isLeftPaddle && !previousLeftPaddle) || (isRightPaddle && !previousRightPaddle))
                {
                    float actualMoveDistance = baseMoveDistance * speedMultiplier;

                    // **當划左槳時，向左前方推進並向左旋轉**
                    if (isLeftPaddle)
                    {
                        targetPosition = transform.position + Quaternion.Euler(0, currentRotation, 0) * new Vector3(-actualMoveDistance, 0, actualMoveDistance);
                        targetRotation -= rotationStep; // 小幅向左旋轉
                    }
                    // **當划右槳時，向右前方推進並向右旋轉**
                    else if (isRightPaddle)
                    {
                        targetPosition = transform.position + Quaternion.Euler(0, currentRotation, 0) * new Vector3(actualMoveDistance, 0, actualMoveDistance);
                        targetRotation += rotationStep; // 小幅向右旋轉
                    }

                    // **速度增益機制**
                    speedMultiplier = Mathf.Min(speedMultiplier + 0.5f, maxMultiplier);
                }
                
                // **更新上次的狀態**
                previousLeftPaddle = isLeftPaddle;
                previousRightPaddle = isRightPaddle;
            }
        }
        catch (System.Exception) { }

        // **使用 SmoothDamp 讓移動更平滑**
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // **平滑旋轉**
        currentRotation = Mathf.Lerp(currentRotation, targetRotation, Time.deltaTime * rotationSpeed);
        transform.rotation = Quaternion.Euler(0, currentRotation, 0);

        // **速度衰減**
        speedMultiplier = Mathf.Max(speedMultiplier * decayRate, 1.0f);
    }

    void OnApplicationQuit()
    {
        serialPort.Close();
    }
}