using UnityEngine;

public class KalmanFilter1D
{
    public float position = 0.0f;
    public float velocity = 0.0f;

    // Covariance matrix
    public float[,] P = {
        {1, 0}, 
        {0, 1}
    };

    public float accelerationNoise = 0.5f; // player movement acceleration
    public float networkNoise = 0.05f; // network jitter

    public void Predict(float dt)
    {
        position += velocity * dt;

        // P = F * P * F^T + Q
        float p00 = P[0,0];
        float p01 = P[0,1];
        float p10 = P[1,0];
        float p11 = P[1,1];

        // F * P * F^T
        P[0,0] = p00 + dt * (p10 + p01) + dt * dt * p11;
        P[0,1] = p01 + dt * p11;
        P[1,0] = p10 + dt * p11;
        P[1,1] = p11;

        // Add noise (Q)
        float dt2 = dt * dt;
        float dt3 = dt2 * dt;
        float dt4 = dt3 * dt;

        P[0,0] += 0.25f * dt4 * accelerationNoise;
        P[0,1] += 0.5f * dt3 * accelerationNoise;
        P[1,0] += 0.5f * dt3 * accelerationNoise;
        P[1,1] += dt2 * accelerationNoise;
    }
    
    public void Correct(float measurement)
    {
        // Innovation
        float y = measurement - position;
        
        // S = H * P * H^T + R
        float S = P[0,0] + networkNoise;

        // K = P * H^T * S^-1
        float k0 = P[0,0] / S;
        float k1 = P[1,0] / S;

        // Update state
        position += k0 * y;
        velocity += k1 * y;

        // P = (I - K * H) * P
        float prev_p00 = P[0,0];
        float prev_p01 = P[0,1];
        float prev_p10 = P[1,0];
        float prev_p11 = P[1,1];

        P[0,0] = (1.0f - k0) * prev_p00;
        P[0,1] = (1.0f - k0) * prev_p01;
        P[1,0] = prev_p10 - (k1 * prev_p00);
        P[1,1] = prev_p11 - (k1 * prev_p01);
    }

    public void Reset(float startPosition = 0.0f)
    {
        position = startPosition;
        velocity = 0.0f;

        P[0,0] = 1.0f;
        P[0,1] = 0.0f;
        P[1,0] = 0.0f;
        P[1,1] = 1.0f;
    }
}

/// <summary>
/// PoseKalmanFilter uses three independent KalmanFilter1D - one for each axis.
/// It assumes X, Y and Z axes are independent from one another.
/// </summary>
public class PoseKalmanFilter
{
    private readonly KalmanFilter1D X = new();
    private readonly KalmanFilter1D Y = new();
    private readonly KalmanFilter1D Z = new();

    public void Predict(float dt)
    {
        X.Predict(dt); 
        Y.Predict(dt); 
        Z.Predict(dt);
    }

    public void Correct(Vector3 measurement)
    {
        X.Correct(measurement.x);
        Y.Correct(measurement.y);
        Z.Correct(measurement.z);
    }

    public Vector3 GetPosition() => new(X.position, Y.position, Z.position);

    public void Reset(Vector3 startPos)
    {
        X.Reset(startPos.x);
        Y.Reset(startPos.y);
        Z.Reset(startPos.z);
    }
}
