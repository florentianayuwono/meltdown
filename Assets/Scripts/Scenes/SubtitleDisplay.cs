using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SubtitleDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private Image subtitleBackground;

    public SubtitleLine[] subtitleLines;
    
    private void Awake()
    {
        // Initialize default subtitles if none are set
        if (subtitleLines == null || subtitleLines.Length == 0)
        {
            subtitleLines = new SubtitleLine[]
            {
                new SubtitleLine { text = "Attention all shoppers!", startTime = 0.0f, duration = 2.0f },
                new SubtitleLine { text = "Global food shortages have reached critical levels.", startTime = 2.0f, duration = 3.0f },
                new SubtitleLine { text = "Every wasted meal brings us closer to disaster.", startTime = 5.0f, duration = 3.0f },
                new SubtitleLine { text = "Your mission: Gather ingredients, cook a meal,", startTime = 8.0f, duration = 3.0f },
                new SubtitleLine { text = "and prove that a zero-waste future is possible.", startTime = 11.0f, duration = 3.0f },
                new SubtitleLine { text = "Choose wisely, or face the consequences.", startTime = 14.0f, duration = 3.0f }
            };
        }
        
        // Hide panel initially
        if (subtitlePanel)
            subtitlePanel.SetActive(false);


        ApplyTextStyling();        
    }

    private void LateUpdate()
    {
        FollowUserGaze();
    }

    public void SetSubtitles(SubtitleLine[] newSubtitles)
    {
        subtitleLines = newSubtitles;
    }

    private void FollowUserGaze()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Calculate forward and downward offset
            Vector3 forward = mainCamera.transform.forward;
            Vector3 down = -mainCamera.transform.up;
            
            // Position the subtitle slightly in front and below the user's gaze
            float distanceFromUser = 1.4f;
            float verticalOffset = -0.03f;
            transform.position = mainCamera.transform.position + forward * distanceFromUser + down * verticalOffset;
            
            // Make the subtitle face the camera (billboarding effect)
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }
    
    public void ShowSubtitles()
    {
        StartCoroutine(DisplaySubtitles());
    }
    
    private IEnumerator DisplaySubtitles()
    {
        if (subtitlePanel == null || subtitleText == null)
        {
            Debug.LogError("Subtitle panel or text component not assigned!");
            yield break;
        }

        subtitlePanel.SetActive(true);
        subtitleBackground.gameObject.SetActive(true);
        
        foreach (SubtitleLine line in subtitleLines)
        {
            // Calculate wait time
            float waitTime = 0;
            if (System.Array.IndexOf(subtitleLines, line) > 0)
            {
                int prevIndex = System.Array.IndexOf(subtitleLines, line) - 1;
                waitTime = line.startTime - (subtitleLines[prevIndex].startTime + subtitleLines[prevIndex].duration);
                if (waitTime < 0) waitTime = 0;
            }
            else
            {
                waitTime = line.startTime;
            }
            
            // Wait until it's time to show this line
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);
            
            // Show the subtitle
            subtitleText.text = line.text;
            
            // Wait for the duration
            yield return new WaitForSeconds(line.duration);
            
            // Clear the text
            subtitleText.text = "";
        }
        
        // Hide the panel when done
        subtitlePanel.SetActive(false);
        subtitleBackground.gameObject.SetActive(false);
    }
    private void ApplyTextStyling()
    {
        if (subtitleText == null)
        {
            Debug.LogError("SubtitleText component is not assigned!");
            return;
        }

        subtitleText.fontSize = 36f; 
        subtitleText.color = Color.white;
        subtitleText.fontStyle = FontStyles.Bold;
        subtitleText.outlineWidth = 0.2f; 
        subtitleText.outlineColor = Color.black;
        subtitleText.alignment = TextAlignmentOptions.Center;
       if (subtitleBackground != null)
            {
                subtitleBackground.color = new Color(0, 0, 0, 0.5f);
            }
            else
            {
                Debug.LogWarning("SubtitleBackground is not assigned!");
            }
    }
}