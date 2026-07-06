using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KalmanFilter1D
{
    public float position = 0.0f;
    public float velocity = 0.0f;

    public float[,] P = {
        {1, 0}, 
        {0, 1}
    };

    public float qPosition = 0.001f;
    public float qVelocity = 0.01f;
    public float measurementNoise = 0.1f;
    
    public float Update(float measurement, float dt)
    {
        // Predict state
        position += velocity * dt;

        // Predict covariance
        float pred_p00 = P[0,0];
        float pred_p01 = P[0,1];
        float pred_p10 = P[1,0];
        float pred_p11 = P[1,1];

        P[0,0] = pred_p00 + dt * (pred_p10 + pred_p01) + dt * dt * P[1,1];
        P[0,1] = pred_p01 + dt * pred_p11;
        P[1,0] = pred_p10 + dt * pred_p11;

        P[0, 0] += qPosition;
        P[1, 1] += qVelocity;

        // Innovation
        float y = measurement - position;
        
        // Innovation covariance
        float S = P[0,0] + measurementNoise;

        // Kalman gain
        float k0 = P[0,0] / S;
        float k1 = P[1,0] / S;

        // Update state
        position += k0 * y;
        velocity += k1 * y;

        // Update covariance
        float update_p00 = P[0,0];
        float update_p01 = P[0,1];
        float update_p10 = P[1,0];
        float update_p11 = P[1,1];

        P[0,0] = (1 - k0) * update_p00;
        P[0,1] = (1 - k0) * update_p01;
        P[1,0] = update_p10 - (k1 * pred_p00);
        P[1,1] = update_p11 - (k1 * pred_p01);

        // Return axis value
        return position; 
    }

    public void Reset()
    {
        position = 0.0f;
        velocity = 0.0f;

        P[0,0] = 1.0f;
        P[0,1] = 0.0f;
        P[1,0] = 0.0f;
        P[1,1] = 1.0f;
    }
}

public class PoseKalmanFilter
{
    private readonly KalmanFilter1D X = new();
    private readonly KalmanFilter1D Y = new();
    private readonly KalmanFilter1D Z = new();

    public Vector3 Update(Vector3 measurement, float dt)
    {
        return new(
            X.Update(measurement.x, dt),
            Y.Update(measurement.y, dt),
            Z.Update(measurement.z, dt)
        );
    }
}
