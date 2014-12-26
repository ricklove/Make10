using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using System.Linq;

public class AdRequestController : MonoBehaviour
{
    public static AdRequestController Instance;

    private GameObject main;
    private Button skipButton;
    private Button showVideoButton;
    private Button showRewardButton;

    private Text requestText;

    void Start()
    {
        Instance = this;
        main = transform.GetChild(0).gameObject;
        main.SetActive(false);

        skipButton = main.transform.GetChild(0).FindChild("SkipButton").gameObject.GetComponent<Button>();
        showVideoButton = main.transform.GetChild(0).FindChild("ShowVideoButton").gameObject.GetComponent<Button>();
        showRewardButton = main.transform.GetChild(0).FindChild("ShowRewardButton").gameObject.GetComponent<Button>();

        requestText = main.transform.GetChild(0).FindChild("RequestText").gameObject.GetComponent<Text>();

        //skipButton.onClick.AddListener(() => { Skip(); });
        //showVideoButton.onClick.AddListener(() => { ShowVideo(); });
        //showRewardButton.onClick.AddListener(() => { ShowReward(); });


    }

    void Update()
    {

    }

    private void Done()
    {
        main.SetActive(false);
        _callback();
    }

    public void Skip()
    {
        Debug.Log("Skip");
        Done();
    }

    private string messageToShow;
    private System.Action _callback;

    public void ShowVideo()
    {
        Debug.Log("ShowVideo");
        Advertisement.Show("rewardedVideoZone");

        Done();
    }

    public void ShowReward()
    {
        // Kiip Moment
#if UNITY_ANDROID
        Kiip.saveMoment(messageToShow);
#endif

        Done();
    }

    public void RequestGameVideo(string message, System.Action callback)
    {
        _callback = callback;
        messageToShow = message;
        main.SetActive(true);
        showRewardButton.gameObject.SetActive(false);
        showVideoButton.gameObject.SetActive(true);
        requestText.text = GetLine(requestVideoMessagesString);
    }

    public void RequestReward(string message, System.Action callback)
    {
        _callback = callback;
        messageToShow = message;
        main.SetActive(true);
        showRewardButton.gameObject.SetActive(true);
        showVideoButton.gameObject.SetActive(false);
        requestText.text = GetLine(requestRewardMessagesString);
    }

    private string GetLine(string lineText)
    {
        var lines = lineText.Split("\r\n".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => l.Length > 0).ToList();

        var i = Random.Range(0, lines.Count);
        if (i >= lines.Count) { i--; }

        return lines[i];
    }

    private static string requestVideoMessagesString = @"
Give a guy some help!
You get to see a video!
How about a game video?
Watching game videos helps me!
Support indie developers!
";

    private static string requestRewardMessagesString = @"
How about a reward?
You won a reward!
Would you like a reward?
Here is a reward!
";
}
