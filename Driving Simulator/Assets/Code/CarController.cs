using TMPro;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider wheelFL, wheelFR, wheelRL, wheelRR;
    public Transform wheelFLTransform, wheelFRTransform, wheelRLTransform, wheelRRTransform;

    public float maxTorque = 300f;
    public float maxSteerAngle = 30f;

    public int currentGear = 0; // -1 = Reverse, 0 = Neutral, 1+ = Forward
    public float[] gearRatios = { -2f, 0f, 2f, 3f, 4f, 5f }; // Gears: R, N, 1, 2, 3, 4
    public float finalDriveRatio;
    private bool clutchPressed = false;

    public float engineRPM;
    public float maxRPM = 7000f;
    private bool revCut = false;
    private float revCutTimer = 0f;

    public TextMeshProUGUI gearText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI rpmText;

    public float brakeForce = 3000f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        clutchPressed = Input.GetKey(KeyCode.LeftShift);

        if (clutchPressed)
        {
            //Debug.Log("Clutch pressed");
            if (Input.GetKeyDown(KeyCode.X)) ShiftUp();
            if (Input.GetKeyDown(KeyCode.Z)) ShiftDown();
            StopMotor();
        }

        if (gearText != null)
        {
            if (currentGear == 0)
                gearText.text = "N";
            else if (currentGear == -1)
                gearText.text = "R";
            else
                gearText.text = currentGear.ToString();
        }

        if (speedText != null)
        {
            float speed = GetComponent<Rigidbody>().velocity.magnitude * 2.237f; // m/s to km/h
            speedText.text = Mathf.RoundToInt(speed) + " mph";
        }

        if (rpmText != null)
        {
            rpmText.text = Mathf.RoundToInt(engineRPM) + " RPM";
            if (engineRPM < 5000)
            {
                rpmText.color = Color.green;
            }
            else if (engineRPM < 6500)
            {
                rpmText.color = Color.yellow;
            }
            else
            {
                rpmText.color = Color.red;
            }
        }

    }

    void FixedUpdate()
    {
        float motorInput = Input.GetAxis("Vertical");
        float steering = maxSteerAngle * Input.GetAxis("Horizontal");

        wheelFL.steerAngle = steering;
        wheelFR.steerAngle = steering;

        // calculate engine rpm
        float avgWheelRPM = (wheelRL.rpm + wheelRR.rpm) / 2f;
        float torque = 0f;

        if (currentGear != 0)
        {
            float targetRPM = Mathf.Abs(avgWheelRPM * gearRatios[currentGear + 1] * finalDriveRatio);
            engineRPM = Mathf.Lerp(engineRPM, targetRPM, Time.deltaTime * 10f); // 5f is rpm smoothing factor
        }
        else
        {
            engineRPM = Mathf.Abs(avgWheelRPM);
        }

        // calculate torque

        if (currentGear != 0 && !revCut && engineRPM < maxRPM)
        {
            torque = maxTorque * motorInput * gearRatios[currentGear + 1] * finalDriveRatio;
        }

        // calculate rev cut
        if (engineRPM > maxRPM && !revCut)
        {
            revCut = true;
            revCutTimer = 0.2f;
        }

        if (revCut)
        {
            torque = 0f;
            revCutTimer -= Time.deltaTime;
            if (revCutTimer <= 0f)
            {
                revCut = false;
            }
        }


        if (!clutchPressed)
        {
            //Debug.Log("give torque");
            wheelRL.motorTorque = torque;
            wheelRR.motorTorque = torque;
        }
        else
        {
            //Debug.Log("clutch pressed, no torque");
            wheelRL.motorTorque = 0;
            wheelRR.motorTorque = 0;
        }

        float brake = 0f;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            brake = brakeForce;
        }

        wheelFL.brakeTorque = brake;
        wheelFR.brakeTorque = brake;
        wheelRL.brakeTorque = brake;
        wheelRR.brakeTorque = brake;

        // handbrake
        if(Input.GetKey(KeyCode.Space)){
            wheelRL.brakeTorque = brakeForce * 2f;
            wheelRR.brakeTorque = brakeForce * 2f;
        }


        UpdateWheel(wheelFL, wheelFLTransform);
        UpdateWheel(wheelFR, wheelFRTransform);
        UpdateWheel(wheelRL, wheelRLTransform);
        UpdateWheel(wheelRR, wheelRRTransform);
    }

    void ShiftUp()
    {
        if (currentGear < gearRatios.Length - 2)
            currentGear++;
    }

    void ShiftDown()
    {
        if (currentGear > -1)
            currentGear--;
    }

    void StopMotor()
    {
        wheelRL.motorTorque = 0;
        wheelRR.motorTorque = 0;
    }

    void UpdateWheel(WheelCollider col, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        col.GetWorldPose(out pos, out rot);
        trans.position = pos;
        trans.rotation = rot;
    }
}
