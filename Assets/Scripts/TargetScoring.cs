using UnityEngine;

public class TargetScoring : MonoBehaviour
{
    // --- Public Variables ---
    // These will show up in the Inspector window
    
    [Header("Scoring")]
    public float maxScore = 100f; // The score for a perfect bullseye
    
    [Tooltip("The radius of the target's scoring area in Unity units. A hit at this distance is 0 points.")]
    public float targetRadius = 1.0f; // <-- IMPORTANT: You MUST set this in the Inspector!

    [Header("Scoring Manager (Optional)")]
    [Tooltip("Drag your global score manager object here later.")]
    public ScoreManager scoreManager; // We'll set this up next

    // --- Private ---
    private Vector3 targetCenter;

    void Start()
    {
        // Get the center of the target at the start of the game
        targetCenter = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // --- 1. Get Hit Location ---
        // Get the exact point where the arrow hit
        Vector3 hitPoint = collision.contacts[0].point;

        // --- 2. Calculate Distance ---
        // Find the distance between the target's center and the hit point
        float distance = Vector3.Distance(targetCenter, hitPoint);

        // --- 3. Calculate Score ---
        
        // This 'InverseLerp' function maps the distance to a 0-1 range.
        // If distance = targetRadius, value is 0.
        // If distance = 0, value is 1.
        float scorePercentage = Mathf.InverseLerp(targetRadius, 0f, distance);

        // Multiply the 0-1 percentage by your max score to get the final score
        float finalScore = scorePercentage * maxScore;

        // Use Clamp to make sure the score is never less than 0 or more than 100
        finalScore = Mathf.Clamp(finalScore, 0f, maxScore);

        // --- 4. Display Score ---
        // Print the score to the console. We use (int) to show a whole number.
        Debug.Log("SCORE: " + (int)finalScore);

        // --- 5. (Future Step) Send to Global Score Manager ---
        // if (scoreManager != null)
        // {
        //     scoreManager.AddScore((int)finalScore);
        // }
    }
}